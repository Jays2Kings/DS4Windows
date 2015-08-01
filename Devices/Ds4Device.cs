using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;

using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;
using System.Diagnostics;
using System.Threading.Tasks;


using System.Linq;
using System.Text;
using System.IO;
using System.Collections;
using EAll4Windows.EAll4Library;

namespace EAll4Windows
{
    public class Ds4Device : IEAll4Device
    {
        private const int OutputReportByteLengthBT = 78;
        private const int InputReportByteLengthBT = 547;
        private HidDevice hDevice;
        private string Mac;
        private ControllerState cState = new ControllerState();
        private ControllerState pState = new ControllerState();
        private ConnectionType conType;
        private byte[] accel = new byte[6];
        private byte[] gyro = new byte[6];
        private byte[] inputReport;
        private byte[] btInputReport = null;
        private byte[] outputReportBuffer, outputReport;
        private EAll4Touchpad touchpad = null;
        private byte rightLightFastRumble;
        private byte leftHeavySlowRumble;
        private EAll4Color ligtBarColor;
        private byte ledFlashOn, ledFlashOff;
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
        public bool IsDisconnecting { get; private set; }

        public string MacAddress { get { return Mac; } }

        public ConnectionType ConnectionType { get { return conType; } }
        public int IdleTimeout { get; set; }
        public int[] PIDs { get; } = { 0x5C4 };
        public int[] VIDs { get; } = { 0x54C };

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

        public EAll4Color LightBarColor
        {
            get { return ligtBarColor; }
            set
            {
                if (ligtBarColor.Red != value.Red || ligtBarColor.Green != value.Green || ligtBarColor.Blue != value.Blue)
                {
                    ligtBarColor = value;
                }
            }
        }

        public byte LightBarOnDuration
        {
            get { return ledFlashOn; }
            set
            {
                if (ledFlashOn != value)
                {
                    ledFlashOn = value;
                }
            }
        }

        public byte LightBarOffDuration
        {
            get { return ledFlashOff; }
            set
            {
                if (ledFlashOff != value)
                {
                    ledFlashOff = value;
                }
            }
        }

        public EAll4Touchpad Touchpad { get { return touchpad; } }

        public ConnectionType HidConnectionType(HidDevice hidDevice)
        {
            return hidDevice.Capabilities.InputReportByteLength == InputReportByteLengthUSB ? ConnectionType.USB : ConnectionType.BT;
        }

        public void Load(HidDevice hidDevice)
        {
            hDevice = hidDevice;
            conType = HidConnectionType(hDevice);
            Mac = hDevice.readSerial();
            if (conType == ConnectionType.USB)
            {
                inputReport = new byte[64];
                outputReport = new byte[hDevice.Capabilities.OutputReportByteLength];
                outputReportBuffer = new byte[hDevice.Capabilities.OutputReportByteLength];
            }
            else
            {
                btInputReport = new byte[InputReportByteLengthBT];
                inputReport = new byte[btInputReport.Length - 2];
                outputReport = new byte[OutputReportByteLengthBT];
                outputReportBuffer = new byte[OutputReportByteLengthBT];
            }
            touchpad = new EAll4Touchpad();
        }

        public void StartUpdate()
        {
            if (eall4Input == null)
            {
                Console.WriteLine(MacAddress.ToString() + " " + System.DateTime.UtcNow.ToString("o") + "> start");
                sendOutputReport(true); // initialize the output report
                eall4Output = new Thread(performEAll4Output);
                eall4Output.Name = "EAll4 Output thread: " + Mac;
                eall4Output.Start();
                eall4Input = new Thread(performEAll4Input);
                eall4Input.Name = "EAll4 Input thread: " + Mac;
                eall4Input.Start();
            }
            else
                Console.WriteLine("Thread already running for EAll4: " + Mac);
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
            if (eall4Output.ThreadState != System.Threading.ThreadState.Stopped || eall4Output.ThreadState != System.Threading.ThreadState.Aborted)
            {
                try
                {
                    eall4Output.Abort();
                    eall4Output.Join();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }

        private bool writeOutput()
        {
            if (conType == ConnectionType.BT)
            {
                return hDevice.WriteOutputReportViaControl(outputReport);
            }
            else
            {
                return hDevice.WriteOutputReportViaInterrupt(outputReport, 8);
            }
        }

        private void performEAll4Output()
        {
            lock (outputReport)
            {
                int lastError = 0;
                while (true)
                {
                    if (writeOutput())
                    {
                        lastError = 0;
                        if (testRumble.IsRumbleSet()) // repeat test rumbles periodically; rumble has auto-shut-off in the EAll4 firmware
                            Monitor.Wait(outputReport, 10000); // EAll4 firmware stops it after 5 seconds, so let the motors rest for that long, too.
                        else
                            Monitor.Wait(outputReport);
                    }
                    else
                    {
                        int thisError = Marshal.GetLastWin32Error();
                        if (lastError != thisError)
                        {
                            Console.WriteLine(MacAddress.ToString() + " " + System.DateTime.UtcNow.ToString("o") + "> encountered write failure: " + thisError);
                            lastError = thisError;
                        }
                    }
                }
            }
        }

        /** Is the device alive and receiving valid sensor input reports? */
        public bool IsAlive()
        {
            return priorInputReport30 != 0xff;
        }
        private byte priorInputReport30 = 0xff;
        public double Latency { get; set; } = 0;
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

                this.Latency = Latency.Average();

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
                        Array.Copy(btInputReport, 2, inputReport, 0, inputReport.Length);
                    }
                    else
                    {
                        Console.WriteLine(MacAddress.ToString() + " " + System.DateTime.UtcNow.ToString("o") + "> disconnect due to read failure: " + Marshal.GetLastWin32Error());
                        sendOutputReport(true); // Kick Windows into noticing the disconnection.
                        StopOutputUpdate();
                        IsDisconnecting = true;
                        if (Removal != null)
                            Removal(this, EventArgs.Empty);
                        return;

                    }
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
                if (ConnectionType == ConnectionType.BT && btInputReport[0] != 0x11)
                {
                    //Received incorrect report, skip it
                    continue;
                }
                DateTime utcNow = System.DateTime.UtcNow; // timestamp with UTC in case system time zone changes
                resetHapticState();
                cState.ReportTimeStamp = utcNow;
                cState.LX = inputReport[1];
                cState.LY = inputReport[2];
                cState.RX = inputReport[3];
                cState.RY = inputReport[4];
                cState.LT = inputReport[8];
                cState.RT = inputReport[9];

                cState.Y = ((byte)inputReport[5] & (1 << 7)) != 0;
                cState.B = ((byte)inputReport[5] & (1 << 6)) != 0;
                cState.A = ((byte)inputReport[5] & (1 << 5)) != 0;
                cState.X = ((byte)inputReport[5] & (1 << 4)) != 0;
                cState.DpadUp = ((byte)inputReport[5] & (1 << 3)) != 0;
                cState.DpadDown = ((byte)inputReport[5] & (1 << 2)) != 0;
                cState.DpadLeft = ((byte)inputReport[5] & (1 << 1)) != 0;
                cState.DpadRight = ((byte)inputReport[5] & (1 << 0)) != 0;

                //Convert dpad into individual On/Off bits instead of a clock representation
                byte dpad_state = 0;

                dpad_state = (byte)(
                ((cState.DpadRight ? 1 : 0) << 0) |
                ((cState.DpadLeft ? 1 : 0) << 1) |
                ((cState.DpadDown ? 1 : 0) << 2) |
                ((cState.DpadUp ? 1 : 0) << 3));

                switch (dpad_state)
                {
                    case 0: cState.DpadUp = true; cState.DpadDown = false; cState.DpadLeft = false; cState.DpadRight = false; break;
                    case 1: cState.DpadUp = true; cState.DpadDown = false; cState.DpadLeft = false; cState.DpadRight = true; break;
                    case 2: cState.DpadUp = false; cState.DpadDown = false; cState.DpadLeft = false; cState.DpadRight = true; break;
                    case 3: cState.DpadUp = false; cState.DpadDown = true; cState.DpadLeft = false; cState.DpadRight = true; break;
                    case 4: cState.DpadUp = false; cState.DpadDown = true; cState.DpadLeft = false; cState.DpadRight = false; break;
                    case 5: cState.DpadUp = false; cState.DpadDown = true; cState.DpadLeft = true; cState.DpadRight = false; break;
                    case 6: cState.DpadUp = false; cState.DpadDown = false; cState.DpadLeft = true; cState.DpadRight = false; break;
                    case 7: cState.DpadUp = true; cState.DpadDown = false; cState.DpadLeft = true; cState.DpadRight = false; break;
                    case 8: cState.DpadUp = false; cState.DpadDown = false; cState.DpadLeft = false; cState.DpadRight = false; break;
                }

                cState.RS = ((byte)inputReport[6] & (1 << 7)) != 0;
                cState.LS = ((byte)inputReport[6] & (1 << 6)) != 0;
                cState.Start = ((byte)inputReport[6] & (1 << 5)) != 0;
                cState.Back = ((byte)inputReport[6] & (1 << 4)) != 0;
                cState.RB = ((byte)inputReport[6] & (1 << 1)) != 0;
                cState.LB = ((byte)inputReport[6] & (1 << 0)) != 0;

                cState.Guide = ((byte)inputReport[7] & (1 << 0)) != 0;
                cState.TouchButton = (inputReport[7] & (1 << 2 - 1)) != 0;
                cState.FrameCounter = (byte)(inputReport[7] >> 2);

                // Store Gyro and Accel values
                Array.Copy(inputReport, 14, accel, 0, 6);
                Array.Copy(inputReport, 20, gyro, 0, 6);


                try
                {
                    charging = (inputReport[30] & 0x10) != 0;
                    battery = (inputReport[30] & 0x0f) * 10;
                    cState.Battery = (byte)battery;
                    if (inputReport[30] != priorInputReport30)
                    {
                        priorInputReport30 = inputReport[30];
                        Console.WriteLine(MacAddress.ToString() + " " + System.DateTime.UtcNow.ToString("o") + "> power subsystem octet: 0x" + inputReport[30].ToString("x02"));
                    }
                }
                catch { currerror = "Index out of bounds: battery"; }
                // XXX ControllerState mapping needs fixup, turn touches into an array[4] of structs.  And include the touchpad details there instead.
                try
                {
                    for (int touches = inputReport[-1 + EAll4Touchpad.TOUCHPAD_DATA_OFFSET - 1], touchOffset = 0; touches > 0; touches--, touchOffset += 9)
                    {
                        cState.TouchPacketCounter = inputReport[-1 + EAll4Touchpad.TOUCHPAD_DATA_OFFSET + touchOffset];
                        cState.Touch1 = (inputReport[0 + EAll4Touchpad.TOUCHPAD_DATA_OFFSET + touchOffset] >> 7) != 0 ? false : true; // >= 1 touch detected
                        cState.Touch1Identifier = (byte)(inputReport[0 + EAll4Touchpad.TOUCHPAD_DATA_OFFSET + touchOffset] & 0x7f);
                        cState.Touch2 = (inputReport[4 + EAll4Touchpad.TOUCHPAD_DATA_OFFSET + touchOffset] >> 7) != 0 ? false : true; // 2 touches detected
                        cState.Touch2Identifier = (byte)(inputReport[4 + EAll4Touchpad.TOUCHPAD_DATA_OFFSET + touchOffset] & 0x7f);
                        cState.TouchLeft = (inputReport[1 + EAll4Touchpad.TOUCHPAD_DATA_OFFSET + touchOffset] + ((inputReport[2 + EAll4Touchpad.TOUCHPAD_DATA_OFFSET + touchOffset] & 0xF) * 255) >= 1920 * 2 / 5) ? false : true;
                        cState.TouchRight = (inputReport[1 + EAll4Touchpad.TOUCHPAD_DATA_OFFSET + touchOffset] + ((inputReport[2 + EAll4Touchpad.TOUCHPAD_DATA_OFFSET + touchOffset] & 0xF) * 255) < 1920 * 2 / 5) ? false : true;
                        // Even when idling there is still a touch packet indicating no touch 1 or 2
                        touchpad.handleTouchpad(inputReport, cState, touchOffset);
                    }
                }
                catch { currerror = "Index out of bounds: touchpad"; }

                /* Debug output of incoming HID data:
                if (cState.LT == 0xff && cState.RT == 0xff)
                {
                    Console.Write(MacAddress.ToString() + " " + System.DateTime.UtcNow.ToString("o") + ">");
                    for (int i = 0; i < inputReport.Length; i++)
                        Console.Write(" " + inputReport[i].ToString("x2"));
                    Console.WriteLine();
                } */
                if (!isNonSixaxisIdle())
                    lastActive = utcNow;
                if (conType == ConnectionType.BT)
                {
                    bool shouldDisconnect = false;
                    if (IdleTimeout > 0)
                    {
                        if (isNonSixaxisIdle())
                        {
                            DateTime timeout = lastActive + TimeSpan.FromSeconds(IdleTimeout);
                            if (!Charging)
                                shouldDisconnect = utcNow >= timeout;
                        }
                    }
                    if (shouldDisconnect && DisconnectBT())
                        return; // all done
                }
                // XXX fix initialization ordering so the null checks all go away
                if (Report != null)
                    Report(this, EventArgs.Empty);
                sendOutputReport(false);
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
        private void sendOutputReport(bool synchronous)
        {
            setTestRumble();
            setHapticState();
            if (conType == ConnectionType.BT)
            {
                outputReportBuffer[0] = 0x11;
                outputReportBuffer[1] = 0x80;
                outputReportBuffer[3] = 0xff;
                outputReportBuffer[6] = rightLightFastRumble; //fast motor
                outputReportBuffer[7] = leftHeavySlowRumble; //slow motor
                outputReportBuffer[8] = LightBarColor.Red; //red
                outputReportBuffer[9] = LightBarColor.Green; //green
                outputReportBuffer[10] = LightBarColor.Blue; //blue
                outputReportBuffer[11] = ledFlashOn; //flash on duration
                outputReportBuffer[12] = ledFlashOff; //flash off duration
            }
            else
            {
                outputReportBuffer[0] = 0x05;
                outputReportBuffer[1] = 0xff;
                outputReportBuffer[4] = rightLightFastRumble; //fast motor
                outputReportBuffer[5] = leftHeavySlowRumble; //slow  motor
                outputReportBuffer[6] = LightBarColor.Red; //red
                outputReportBuffer[7] = LightBarColor.Green; //green
                outputReportBuffer[8] = LightBarColor.Blue; //blue
                outputReportBuffer[9] = ledFlashOn; //flash on duration
                outputReportBuffer[10] = ledFlashOff; //flash off duration
            }
            lock (outputReport)
            {
                if (synchronous)
                {
                    outputReportBuffer.CopyTo(outputReport, 0);
                    try
                    {
                        if (!writeOutput())
                        {
                            Console.WriteLine(MacAddress.ToString() + " " + System.DateTime.UtcNow.ToString("o") + "> encountered synchronous write failure: " + Marshal.GetLastWin32Error());
                            eall4Output.Abort();
                            eall4Output.Join();
                        }
                    }
                    catch
                    {
                        // If it's dead already, don't worry about it.
                    }
                }
                else
                {
                    bool output = false;
                    for (int i = 0; !output && i < outputReport.Length; i++)
                        output = outputReport[i] != outputReportBuffer[i];
                    if (output)
                    {
                        outputReportBuffer.CopyTo(outputReport, 0);
                        Monitor.Pulse(outputReport);
                    }
                }
            }
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

        private void setTestRumble()
        {
            if (testRumble.IsRumbleSet())
            {
                pushHapticState(testRumble);
                if (testRumble.RumbleMotorsExplicitlyOff)
                    testRumble.RumbleMotorsExplicitlyOff = false;
            }
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
            expState.Accel = accel;
            expState.Gyro = gyro;
        }

        public void getCurrentState(ControllerState state)
        {
            cState.CopyTo(state);
        }

        public void getPreviousState(ControllerState state)
        {
            pState.CopyTo(state);
        }

        private bool isNonSixaxisIdle()
        {
            if (cState.X || cState.A || cState.B || cState.Y)
                return false;
            if (cState.DpadUp || cState.DpadLeft || cState.DpadDown || cState.DpadRight)
                return false;
            if (cState.LS || cState.RS || cState.LB || cState.RB || cState.Back || cState.Start)
                return false;
            if (cState.LT != 0 || cState.RT != 0)
                return false;
            // TODO calibrate to get an accurate jitter and center-play range and centered position
            const int slop = 64;
            if (cState.LX <= 127 - slop || cState.LX >= 128 + slop || cState.LY <= 127 - slop || cState.LY >= 128 + slop)
                return false;
            if (cState.RX <= 127 - slop || cState.RX >= 128 + slop || cState.RY <= 127 - slop || cState.RY >= 128 + slop)
                return false;
            if (cState.Touch1 || cState.Touch2 || cState.TouchButton)
                return false;
            return true;
        }

        private EAll4HapticState[] hapticState = new EAll4HapticState[1];
        private int hapticStackIndex = 0;
        private void resetHapticState()
        {
            hapticStackIndex = 0;
        }

        // Use the "most recently set" haptic state for each of light bar/motor.
        private void setHapticState()
        {
            int i = 0;
            EAll4Color lightBarColor = LightBarColor;
            byte lightBarFlashDurationOn = LightBarOnDuration, lightBarFlashDurationOff = LightBarOffDuration;
            byte rumbleMotorStrengthLeftHeavySlow = LeftHeavySlowRumble, rumbleMotorStrengthRightLightFast = rightLightFastRumble;
            foreach (EAll4HapticState haptic in hapticState)
            {
                if (i++ == hapticStackIndex)
                    break; // rest haven't been used this time
                if (haptic.IsLightBarSet())
                {
                    lightBarColor = haptic.LightBarColor;
                    lightBarFlashDurationOn = haptic.LightBarFlashDurationOn;
                    lightBarFlashDurationOff = haptic.LightBarFlashDurationOff;
                }
                if (haptic.IsRumbleSet())
                {
                    rumbleMotorStrengthLeftHeavySlow = haptic.RumbleMotorStrengthLeftHeavySlow;
                    rumbleMotorStrengthRightLightFast = haptic.RumbleMotorStrengthRightLightFast;
                }
            }
            LightBarColor = lightBarColor;
            LightBarOnDuration = lightBarFlashDurationOn;
            LightBarOffDuration = lightBarFlashDurationOff;
            LeftHeavySlowRumble = rumbleMotorStrengthLeftHeavySlow;
            RightLightFastRumble = rumbleMotorStrengthRightLightFast;
        }

        public void pushHapticState(EAll4HapticState hs)
        {
            if (hapticStackIndex == hapticState.Length)
            {
                EAll4HapticState[] newHaptics = new EAll4HapticState[hapticState.Length + 1];
                Array.Copy(hapticState, newHaptics, hapticState.Length);
                hapticState = newHaptics;
            }
            hapticState[hapticStackIndex++] = hs;
        }

        override
        public String ToString()
        {
            return Mac;
        }
    }
}
