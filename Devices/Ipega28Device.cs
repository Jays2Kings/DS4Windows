using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using EAll4Windows.EAll4Library;

namespace EAll4Windows
{
    public class Ipega28Device : IEAll4Device
    {
        private const int InputReportByteLengthBT = 11;
        private HidDevice hDevice;
        private string Mac;
        private ControllerState cState = new ControllerState();
        private ControllerState pState = new ControllerState();
        private ConnectionType conType;
        private byte[] inputReport;
        private byte[] btInputReport = null;
        private byte rightLightFastRumble;
        private byte leftHeavySlowRumble;
        private Thread eall4Input, eall4Output;
        private int battery;
        public short InputReportByteLengthUSB { get; } = 64;
        public DateTime lastActive { get; set; } = DateTime.UtcNow;
        public DateTime firstActive { get; set; } = DateTime.UtcNow;
        private bool charging;
        public event EventHandler<EventArgs> Report = null;
        public event EventHandler<EventArgs> Removal = null;

        public HidDevice HidDevice { get { return hDevice; } }
        public bool IsExclusive { get { return HidDevice.IsExclusive; } }
        public EAll4Touchpad Touchpad { get; }
        public bool IsDisconnecting { get; private set; }

        public string MacAddress { get { return Mac; } }

        public ConnectionType ConnectionType { get { return conType; } }
        public int IdleTimeout { get; set; }
        public int[] VIDs { get; } = { 0x1949 };
        public int[] PIDs { get; } = { 0x0402 };

        // behavior only active when > 0

        public int Battery { get { return battery; } }
        public bool Charging { get { return charging; } }

        public byte RightLightFastRumble
        {
            get { return rightLightFastRumble; }
            set
            {
                if (value == rightLightFastRumble) return;
                rightLightFastRumble = value;
            }
        }

        public byte LeftHeavySlowRumble
        {
            get { return leftHeavySlowRumble; }
            set
            {
                if (value == leftHeavySlowRumble) return;
                leftHeavySlowRumble = value;
            }
        }

        public EAll4Color? LightBarColor
        {
            get { return null; }
            set { }
        }

        public byte LightBarOnDuration => 0;

        public byte LightBarOffDuration => 0;

        public static ConnectionType HidConnectionType(HidDevice hidDevice)
        {
            return ConnectionType.BT;
        }

        public void Load(HidDevice hidDevice)
        {
            hDevice = hidDevice;
            conType = HidConnectionType(hDevice);
            Mac = hDevice.readSerial();
            //if (conType == ConnectionType.USB)
            //{
            //    inputReport = new byte[64];
            //    outputReport = new byte[hDevice.Capabilities.OutputReportByteLength];
            //    outputReportBuffer = new byte[hDevice.Capabilities.OutputReportByteLength];
            //}
            //else
            //{
            btInputReport = new byte[InputReportByteLengthBT];
            inputReport = new byte[btInputReport.Length];
            //outputReport = new byte[OutputReportByteLengthBT];
            //outputReportBuffer = new byte[OutputReportByteLengthBT];
            //}
            //touchpad = new EAll4Touchpad();
        }

        public void StartUpdate()
        {
            if (eall4Input == null)
            {
                Console.WriteLine(MacAddress.ToString() + " " + System.DateTime.UtcNow.ToString("o") + "> start");
                eall4Input = new Thread(performEAll4Input);
                eall4Input.Name = "EAll4 Input thread: " + Mac;
                eall4Input.Start();
            }
            else
                Console.WriteLine("Thread already running for Mi: " + Mac);
        }

        public void StopUpdate()
        {
            if (eall4Input.ThreadState != System.Threading.ThreadState.Stopped || eall4Input.ThreadState != System.Threading.ThreadState.Aborted)
            {
                try
                {
                    eall4Input.Abort();
                    eall4Input.Join();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
            StopOutputUpdate();
        }

        private void StopOutputUpdate()
        {
        }

        /** Is the device alive and receiving valid sensor input reports? */
        public bool IsAlive()
        {
            return true;
        }
        public double Latency => 0;
        bool warn;
        public string error { get; set; }

        private void performEAll4Input()
        {
            firstActive = DateTime.UtcNow;
            System.Timers.Timer readTimeout = new System.Timers.Timer(); // Await 30 seconds for the initial packet, then 3 seconds thereafter.
            readTimeout.Elapsed += delegate { HidDevice.CancelIO(); };
            List<long> Latency = new List<long>();
            long oldtime = 0;
            Stopwatch sw = new Stopwatch();
            sw.Start();
            while (true)
            {
                string currerror = string.Empty;
                Latency.Add(sw.ElapsedMilliseconds - oldtime);
                oldtime = sw.ElapsedMilliseconds;

                if (Latency.Count > 100)
                    Latency.RemoveAt(0);

                //this.Latency = Latency.Average();

                if (this.Latency > 10 && !warn && sw.ElapsedMilliseconds > 4000)
                {
                    warn = true;
                    //System.Diagnostics.Trace.WriteLine(System.DateTime.UtcNow.ToString("o") + "> " + "Controller " + /*this.DeviceNum*/ + 1 + " (" + this.MacAddress + ") is experiencing latency issues. Currently at " + Math.Round(this.Latency, 2).ToString() + "ms of recomended maximum 10ms");
                }
                else if (this.Latency <= 10 && warn) warn = false;

                if (readTimeout.Interval != 3000.0)
                {
                    if (readTimeout.Interval != 30000.0)
                        readTimeout.Interval = 30000.0;
                    else
                        readTimeout.Interval = 3000.0;
                }
                readTimeout.Enabled = true;
                if (conType != ConnectionType.USB)
                {
                    HidDevice.ReadStatus res = hDevice.ReadFile(btInputReport);
                    readTimeout.Enabled = false;
                    if (res == HidDevice.ReadStatus.Success)
                    {
                        Array.Copy(btInputReport, 0, inputReport, 0, inputReport.Length);
                    }
                    //else
                    //{
                    //    Console.WriteLine(MacAddress.ToString() + " " + System.DateTime.UtcNow.ToString("o") + "> disconnect due to read failure: " + Marshal.GetLastWin32Error());
                    //    sendOutputReport(true); // Kick Windows into noticing the disconnection.
                    //    StopOutputUpdate();
                    //    IsDisconnecting = true;
                    //    if (Removal != null)
                    //        Removal(this, EventArgs.Empty);
                    //    return;

                    //}
                }
                else
                {
                    HidDevice.ReadStatus res = hDevice.ReadFile(inputReport);
                    readTimeout.Enabled = false;
                    if (res != HidDevice.ReadStatus.Success)
                    {
                        Console.WriteLine(MacAddress.ToString() + " " + System.DateTime.UtcNow.ToString("o") + "> disconnect due to read failure: " + Marshal.GetLastWin32Error());
                        StopOutputUpdate();
                        IsDisconnecting = true;
                        if (Removal != null)
                            Removal(this, EventArgs.Empty);
                        return;
                    }
                }
                //if (ConnectionType == ConnectionType.BT && btInputReport[0] != 0x11)
                //{
                //    //Received incorrect report, skip it
                //    continue;
                //}
                DateTime utcNow = System.DateTime.UtcNow; // timestamp with UTC in case system time zone changes
                //resetHapticState();
                cState.ReportTimeStamp = utcNow;
                cState.LX = inputReport[1];
                cState.LY = inputReport[2];
                cState.RX = inputReport[3];
                cState.RY = inputReport[4];
                cState.LT = inputReport[9];
                cState.RT = inputReport[8];

                cState.A = ((byte)inputReport[6] & Convert.ToByte(1)) != 0;
                cState.B = ((byte)inputReport[6] & Convert.ToByte(2)) != 0;
                cState.X = ((byte)inputReport[6] & Convert.ToByte(8)) != 0;
                cState.Y = ((byte)inputReport[6] & Convert.ToByte(16)) != 0;
                switch (inputReport[5])
                {
                    case 0: cState.DpadUp = true; cState.DpadDown = false; cState.DpadLeft = false; cState.DpadRight = false; break;
                    case 1: cState.DpadUp = true; cState.DpadDown = false; cState.DpadLeft = false; cState.DpadRight = true; break;
                    case 2: cState.DpadUp = false; cState.DpadDown = false; cState.DpadLeft = false; cState.DpadRight = true; break;
                    case 3: cState.DpadUp = false; cState.DpadDown = true; cState.DpadLeft = false; cState.DpadRight = true; break;
                    case 4: cState.DpadUp = false; cState.DpadDown = true; cState.DpadLeft = false; cState.DpadRight = false; break;
                    case 5: cState.DpadUp = false; cState.DpadDown = true; cState.DpadLeft = true; cState.DpadRight = false; break;
                    case 6: cState.DpadUp = false; cState.DpadDown = false; cState.DpadLeft = true; cState.DpadRight = false; break;
                    case 7: cState.DpadUp = true; cState.DpadDown = false; cState.DpadLeft = true; cState.DpadRight = false; break;
                    default: cState.DpadUp = false; cState.DpadDown = false; cState.DpadLeft = false; cState.DpadRight = false; break;
                }

                var rightStick = ((byte)inputReport[7] & Convert.ToByte(64)) != 0;
                cState.RS = rightStick;
                var leftStick = ((byte)inputReport[7] & Convert.ToByte(32)) != 0;
                cState.LS = leftStick;
                var menu = ((byte)inputReport[7] & Convert.ToByte(8)) != 0;
                var back = ((byte)inputReport[7] & Convert.ToByte(4)) != 0;
                cState.Start = menu;
                cState.Back = back;
                cState.LB = ((byte)inputReport[6] & Convert.ToByte(64)) != 0;
                cState.RB = ((byte)inputReport[6] & Convert.ToByte(128)) != 0;

                cState.Guide = rightStick && leftStick;
                // XXX fix initialization ordering so the null checks all go away
                if (Report != null)
                    Report(this, EventArgs.Empty);
                //sendOutputReport(false);
                if (!string.IsNullOrEmpty(error))
                    error = string.Empty;
                if (!string.IsNullOrEmpty(currerror))
                    error = currerror;
                cState.CopyTo(pState);
            }
        }

        public void FlushHID()
        {
            hDevice.flush_Queue();
        }

        public bool DisconnectBT()
        {
            if (Mac != null)
            {
                Console.WriteLine("Trying to disconnect BT device " + Mac);
                IntPtr btHandle = IntPtr.Zero;
                int IOCTL_BTH_DISCONNECT_DEVICE = 0x41000c;

                byte[] btAddr = new byte[8];
                string[] sbytes = Mac.Split(':');
                for (int i = 0; i < 6; i++)
                {
                    //parse hex byte in reverse order
                    btAddr[5 - i] = Convert.ToByte(sbytes[i], 16);
                }
                long lbtAddr = BitConverter.ToInt64(btAddr, 0);

                NativeMethods.BLUETOOTH_FIND_RADIO_PARAMS p = new NativeMethods.BLUETOOTH_FIND_RADIO_PARAMS();
                p.dwSize = Marshal.SizeOf(typeof(NativeMethods.BLUETOOTH_FIND_RADIO_PARAMS));
                IntPtr searchHandle = NativeMethods.BluetoothFindFirstRadio(ref p, ref btHandle);
                int bytesReturned = 0;
                bool success = false;
                while (!success && btHandle != IntPtr.Zero)
                {
                    success = NativeMethods.DeviceIoControl(btHandle, IOCTL_BTH_DISCONNECT_DEVICE, ref lbtAddr, 8, IntPtr.Zero, 0, ref bytesReturned, IntPtr.Zero);
                    NativeMethods.CloseHandle(btHandle);
                    if (!success)
                        if (!NativeMethods.BluetoothFindNextRadio(searchHandle, ref btHandle))
                            btHandle = IntPtr.Zero;

                }
                NativeMethods.BluetoothFindRadioClose(searchHandle);
                Console.WriteLine("Disconnect successful: " + success);
                success = true; // XXX return value indicates failure, but it still works?
                if (success)
                {
                    IsDisconnecting = true;
                    StopOutputUpdate();
                    if (Removal != null)
                        Removal(this, EventArgs.Empty);
                }
                return success;
            }
            return false;
        }

        private EAll4HapticState testRumble = new EAll4HapticState();
        public void setRumble(byte rightLightFastMotor, byte leftHeavySlowMotor)
        {
            testRumble.RumbleMotorStrengthRightLightFast = rightLightFastMotor;
            testRumble.RumbleMotorStrengthLeftHeavySlow = leftHeavySlowMotor;
            testRumble.RumbleMotorsExplicitlyOff = rightLightFastMotor == 0 && leftHeavySlowMotor == 0;
        }

        public void pushHapticState(EAll4HapticState haptics)
        {
        }

        public ControllerState getCurrentState()
        {
            return cState.Clone();
        }

        public ControllerState getPreviousState()
        {
            return pState.Clone();
        }

        public void getExposedState(EAll4StateExposed expState, ControllerState state)
        {
            cState.CopyTo(state);
        }

        public void getCurrentState(ControllerState state)
        {
            cState.CopyTo(state);
        }

        public void getPreviousState(ControllerState state)
        {
            pState.CopyTo(state);
        }

        override
            public String ToString()
        {
            return Mac;
        }
    }
}