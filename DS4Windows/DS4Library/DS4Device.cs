using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;

using System.Runtime.InteropServices;
using System.Diagnostics;

using System.Linq;
using System.Drawing;
using DS4Windows.DS4Library;

namespace DS4Windows
{
    public struct DS4Color
    {
        public byte red;
        public byte green;
        public byte blue;
        public DS4Color(Color c)
        {
            red = c.R;
            green = c.G;
            blue = c.B;
        }

        public DS4Color(byte r, byte g, byte b)
        {
            red = r;
            green = g;
            blue = b;
        }

        public override bool Equals(object obj)
        {
            if (obj is DS4Color)
            {
                DS4Color dsc = ((DS4Color)obj);
                return (this.red == dsc.red && this.green == dsc.green && this.blue == dsc.blue);
            }
            else
                return false;
        }

        public Color ToColor => Color.FromArgb(red, green, blue);
        public Color ToColorA
        {
            get
            {
                byte alphacolor = Math.Max(red, Math.Max(green, blue));
                Color reg = Color.FromArgb(red, green, blue);
                Color full = HuetoRGB(reg.GetHue(), reg.GetBrightness(), reg);
                return Color.FromArgb((alphacolor > 205 ? 255 : (alphacolor + 50)), full);
            }
        }

        private Color HuetoRGB(float hue, float light, Color rgb)
        {
            float L = (float)Math.Max(.5, light);
            float C = (1 - Math.Abs(2 * L - 1));
            float X = (C * (1 - Math.Abs((hue / 60) % 2 - 1)));
            float m = L - C / 2;
            float R = 0, G = 0, B = 0;
            if (light == 1) return Color.White;
            else if (rgb.R == rgb.G && rgb.G == rgb.B) return Color.White;
            else if (0 <= hue && hue < 60) { R = C; G = X; }
            else if (60 <= hue && hue < 120) { R = X; G = C; }
            else if (120 <= hue && hue < 180) { G = C; B = X; }
            else if (180 <= hue && hue < 240) { G = X; B = C; }
            else if (240 <= hue && hue < 300) { R = X; B = C; }
            else if (300 <= hue && hue < 360) { R = C; B = X; }
            return Color.FromArgb((int)((R + m) * 255), (int)((G + m) * 255), (int)((B + m) * 255));
        }

        public static bool TryParse(string value, ref DS4Color ds4color)
        {
            try
            {
                string[] ss = value.Split(',');
                return byte.TryParse(ss[0], out ds4color.red) && byte.TryParse(ss[1], out ds4color.green) && byte.TryParse(ss[2], out ds4color.blue);
            }
            catch { return false; }
        }

        public override string ToString() => $"Red: {red} Green: {green} Blue: {blue}";
    }

    public enum ConnectionType : byte { BT, SONYWA, USB }; // Prioritize Bluetooth when both BT and USB are connected.

    /**
     * The haptics engine uses a stack of these states representing the light bar and rumble motor settings.
     * It (will) handle composing them and the details of output report management.
     */
    public struct DS4HapticState
    {
        public DS4Color LightBarColor;
        public bool LightBarExplicitlyOff;
        public byte LightBarFlashDurationOn, LightBarFlashDurationOff;
        public byte RumbleMotorStrengthLeftHeavySlow, RumbleMotorStrengthRightLightFast;
        public bool RumbleMotorsExplicitlyOff;

        public bool IsLightBarSet()
        {
            return LightBarExplicitlyOff || LightBarColor.red != 0 || LightBarColor.green != 0 || LightBarColor.blue != 0;
        }

        public bool IsRumbleSet()
        {
            return RumbleMotorsExplicitlyOff || RumbleMotorStrengthLeftHeavySlow != 0 || RumbleMotorStrengthRightLightFast != 0;
        }
    }

    public class DS4Device
    {
        internal const int BT_OUTPUT_REPORT_LENGTH = 78;
        internal const int BT_INPUT_REPORT_LENGTH = 547;
        internal const int BT_OUTPUT_CHANGE_LENGTH = 13;
        internal const int USB_OUTPUT_CHANGE_LENGTH = 11;
        // Use large value for worst case scenario
        internal const int READ_STREAM_TIMEOUT = 3000;
        // Isolated BT report can have latency as high as 15 ms
        // due to hardware.
        internal const int WARN_INTERVAL_BT = 20;
        internal const int WARN_INTERVAL_USB = 10;
        // Maximum values for battery level when no USB cable is connected
        // and when a USB cable is connected
        internal const int BATTERY_MAX = 8;
        internal const int BATTERY_MAX_USB = 11;
        public const string blankSerial = "00:00:00:00:00:00";
        private HidDevice hDevice;
        private string Mac;
        private DS4State cState = new DS4State();
        private DS4State pState = new DS4State();
        private ConnectionType conType;
        private byte[] accel = new byte[6];
        private byte[] gyro = new byte[6];
        private byte[] inputReport;
        private byte[] btInputReport = null;
        private byte[] outReportBuffer, outputReport;
        private readonly DS4Touchpad touchpad = null;
        private readonly DS4SixAxis sixAxis = null;
        private byte rightLightFastRumble;
        private byte leftHeavySlowRumble;
        private DS4Color ligtBarColor;
        private byte ledFlashOn, ledFlashOff;
        private Thread ds4Input, ds4Output;
        private int battery;
        private DS4Audio audio = null;
        private DS4Audio micAudio = null;
        public DateTime lastActive = DateTime.UtcNow;
        public DateTime firstActive = DateTime.UtcNow;
        private bool charging;
        private bool outputRumble = false;
        private int warnInterval = WARN_INTERVAL_USB;
        public int getWarnInterval()
        {
            return warnInterval;
        }

        private bool exitOutputThread = false;
        public bool ExitOutputThread => exitOutputThread;
        private bool exitInputThread = false;
        private object exitLocker = new object();

        public event EventHandler<EventArgs> Report = null;
        public event EventHandler<EventArgs> Removal = null;
        public event EventHandler<EventArgs> SyncChange = null;
        public event EventHandler<EventArgs> SerialChange = null;

        public HidDevice HidDevice => hDevice;
        public bool IsExclusive => HidDevice.IsExclusive;
        public bool isExclusive()
        {
            return HidDevice.IsExclusive;
        }

        private bool isDisconnecting = false;
        public bool IsDisconnecting
        {
            get { return isDisconnecting; }
            private set
            {
                this.isDisconnecting = value;
            }
        }

        public bool isDisconnectingStatus()
        {
            return this.isDisconnecting;
        }

        private bool isRemoving = false;
        public bool IsRemoving
        {
            get { return isRemoving; }
            set
            {
                this.isRemoving = value;
            }
        }

        private bool isRemoved = false;
        public bool IsRemoved
        {
            get { return isRemoved; }
            set
            {
                this.isRemoved = value;
            }
        }

        public object removeLocker = new object();

        public string MacAddress =>  Mac;
        public string getMacAddress()
        {
            return this.Mac;
        }

        public ConnectionType ConnectionType => conType;
        public ConnectionType getConnectionType()
        {
            return this.conType;
        }

        // behavior only active when > 0
        private int idleTimeout = 0;
        public int IdleTimeout
        {
            get { return idleTimeout; }
            set
            {
                idleTimeout = value;
            }
        }

        public int getIdleTimeout()
        {
            return idleTimeout;
        }

        public void setIdleTimeout(int value)
        {
            if (idleTimeout != value)
            {
                idleTimeout = value;
            }
        }

        public int Battery => battery;
        public int getBattery()
        {
            return battery;
        }

        public bool Charging => charging;
        public bool isCharging()
        {
            return charging;
        }

        private long lastTimeElapsed = 0;
        public long getLastTimeElapsed()
        {
            return lastTimeElapsed;
        }

        public double lastTimeElapsedDouble = 0.0;
        public double getLastTimeElapsedDouble()
        {
            return lastTimeElapsedDouble;
        }

        public byte RightLightFastRumble
        {
            get { return rightLightFastRumble; }
            set
            {
                if (rightLightFastRumble != value)
                    rightLightFastRumble = value;
            }
        }

        public byte LeftHeavySlowRumble
        {
            get { return leftHeavySlowRumble; }
            set
            {
                if (leftHeavySlowRumble != value)
                    leftHeavySlowRumble = value;
            }
        }

        public byte getLeftHeavySlowRumble()
        {
            return leftHeavySlowRumble;
        }

        public DS4Color LightBarColor
        {
            get { return ligtBarColor; }
            set
            {
                if (ligtBarColor.red != value.red || ligtBarColor.green != value.green || ligtBarColor.blue != value.blue)
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

        public byte getLightBarOnDuration()
        {
            return ledFlashOn;
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

        public byte getLightBarOffDuration()
        {
            return ledFlashOff;
        }

        // Specify the poll rate interval used for the DS4 hardware when
        // connected via Bluetooth
        private int btPollRate = 0;
        public int BTPollRate
        {
            get { return btPollRate; }
            set
            {
                if (btPollRate != value && value >= 0 && value <= 16)
                {
                    btPollRate = value;
                }
            }
        }

        public int getBTPollRate()
        {
            return btPollRate;
        }

        public void setBTPollRate(int value)
        {
            if (btPollRate != value && value >= 0 && value <= 16)
            {
                btPollRate = value;
            }
        }

        public DS4Touchpad Touchpad { get { return touchpad; } }
        public DS4SixAxis SixAxis { get { return sixAxis; } }

        public static ConnectionType HidConnectionType(HidDevice hidDevice)
        {
            ConnectionType result = ConnectionType.USB;
            if (hidDevice.Capabilities.InputReportByteLength == 64)
            {
                if (hidDevice.Capabilities.NumberFeatureDataIndices == 22)
                {
                    result = ConnectionType.SONYWA;
                }
            }
            else
            {
                result = ConnectionType.BT;
            }

            return result;
        }

        private SynchronizationContext uiContext = null;
        public SynchronizationContext getUiContext()
        {
            return uiContext;
        }
        public void setUiContext(SynchronizationContext uiContext)
        {
            this.uiContext = uiContext;
        }

        private Queue<Action> eventQueue = new Queue<Action>();
        private object eventQueueLock = new object();

        private Thread timeoutCheckThread = null;
        private bool timeoutExecuted = false;
        private bool timeoutEvent = false;

        public DS4Device(HidDevice hidDevice)
        {
            hDevice = hidDevice;
            conType = HidConnectionType(hDevice);
            Mac = hDevice.readSerial();
            if (conType == ConnectionType.USB || conType == ConnectionType.SONYWA)
            {
                inputReport = new byte[64];
                outputReport = new byte[hDevice.Capabilities.OutputReportByteLength];
                outReportBuffer = new byte[hDevice.Capabilities.OutputReportByteLength];
                if (conType == ConnectionType.USB)
                {
                    warnInterval = WARN_INTERVAL_USB;
                    HidDeviceAttributes tempAttr = hDevice.Attributes;
                    if (tempAttr.VendorId == 0x054C && tempAttr.ProductId == 0x09CC)
                    {
                        audio = new DS4Audio();
                        micAudio = new DS4Audio(DS4Library.CoreAudio.DataFlow.Capture);
                    }

                    synced = true;
                }
                else
                {
                    warnInterval = WARN_INTERVAL_BT;
                    audio = new DS4Audio();
                    micAudio = new DS4Audio(DS4Library.CoreAudio.DataFlow.Capture);
                    synced = isValidSerial();
                }
            }
            else
            {
                btInputReport = new byte[BT_INPUT_REPORT_LENGTH];
                inputReport = new byte[BT_INPUT_REPORT_LENGTH - 2];
                outputReport = new byte[BT_OUTPUT_REPORT_LENGTH];
                outReportBuffer = new byte[BT_OUTPUT_REPORT_LENGTH];
                warnInterval = WARN_INTERVAL_BT;
                synced = isValidSerial();
            }

            touchpad = new DS4Touchpad();
            sixAxis = new DS4SixAxis();
            Crc32Algorithm.InitializeTable(DefaultPolynomial);
            refreshCalibration();

            if (!hDevice.IsFileStreamOpen())
            {
                hDevice.OpenFileStream(inputReport.Length);
            }

            sendOutputReport(true, true); // initialize the output report
        }

        private void timeoutTestThread()
        {
            while (!timeoutExecuted)
            {
                if (timeoutEvent)
                {
                    timeoutExecuted = true;
                    this.sendOutputReport(true, true); // Kick Windows into noticing the disconnection.
                }
                else
                {
                    timeoutEvent = true;
                    Thread.Sleep(READ_STREAM_TIMEOUT);
                }
            }
        }

        const int DS4_FEATURE_REPORT_5_LEN = 41;
        const int DS4_FEATURE_REPORT_5_CRC32_POS = DS4_FEATURE_REPORT_5_LEN - 4;
        public void refreshCalibration()
        {
            byte[] calibration = new byte[41];
            calibration[0] = conType == ConnectionType.BT ? (byte)0x05 : (byte)0x02;

            if (conType == ConnectionType.BT)
            {
                bool found = false;
                for (int tries = 0; !found && tries < 5; tries++)
                {
                    hDevice.readFeatureData(calibration);
                    uint recvCrc32 = calibration[DS4_FEATURE_REPORT_5_CRC32_POS] |
                                (uint)(calibration[DS4_FEATURE_REPORT_5_CRC32_POS + 1] << 8) |
                                (uint)(calibration[DS4_FEATURE_REPORT_5_CRC32_POS + 2] << 16) |
                                (uint)(calibration[DS4_FEATURE_REPORT_5_CRC32_POS + 3] << 24);

                    uint calcCrc32 = ~Crc32Algorithm.Compute(new byte[] { 0xA3 });
                    calcCrc32 = ~Crc32Algorithm.CalculateBasicHash(ref calcCrc32, ref calibration, 0, DS4_FEATURE_REPORT_5_LEN - 4);
                    bool validCrc = recvCrc32 == calcCrc32;
                    if (!validCrc && tries >= 5)
                    {
                        Log.LogToGui("Gyro Calibration Failed", true);
                        continue;
                    }
                    else if (validCrc)
                    {
                        found = true;
                    }
                }

                sixAxis.setCalibrationData(ref calibration, conType == ConnectionType.USB);
            }
            else
            {
                hDevice.readFeatureData(calibration);
                sixAxis.setCalibrationData(ref calibration, conType == ConnectionType.USB);
            }
        }

        public void StartUpdate()
        {
            if (ds4Input == null)
            {
                if (conType == ConnectionType.BT)
                {
                    ds4Output = new Thread(performDs4Output);
                    ds4Output.Priority = ThreadPriority.AboveNormal;
                    ds4Output.Name = "DS4 Output thread: " + Mac;
                    ds4Output.IsBackground = true;
                    ds4Output.Start();

                    timeoutCheckThread = new Thread(timeoutTestThread);
                    timeoutCheckThread.Priority = ThreadPriority.BelowNormal;
                    timeoutCheckThread.Name = "DS4 Timeout thread: " + Mac;
                    timeoutCheckThread.IsBackground = true;
                    timeoutCheckThread.Start();
                }

                ds4Input = new Thread(performDs4Input);
                ds4Input.Priority = ThreadPriority.AboveNormal;
                ds4Input.Name = "DS4 Input thread: " + Mac;
                ds4Input.IsBackground = true;
                ds4Input.Start();
            }
            else
                Console.WriteLine("Thread already running for DS4: " + Mac);
        }

        public void StopUpdate()
        {
            if (ds4Input != null &&
                ds4Input.IsAlive && !ds4Input.ThreadState.HasFlag(System.Threading.ThreadState.Stopped) &&
                !ds4Input.ThreadState.HasFlag(System.Threading.ThreadState.AbortRequested))
            {
                try
                {
                    exitInputThread = true;
                    //ds4Input.Abort();
                    ds4Input.Join();
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
            lock (exitLocker)
            {
                if (ds4Output != null &&
                    ds4Output.IsAlive && !ds4Output.ThreadState.HasFlag(System.Threading.ThreadState.Stopped) &&
                    !ds4Output.ThreadState.HasFlag(System.Threading.ThreadState.AbortRequested))
                {
                    try
                    {
                        exitOutputThread = true;
                        ds4Output.Interrupt();
                        ds4Output.Join();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
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
                return hDevice.WriteOutputReportViaInterrupt(outReportBuffer, READ_STREAM_TIMEOUT);
            }
        }

        private byte outputPendCount = 0;
        private unsafe void performDs4Output()
        {
            try
            {
                int lastError = 0;
                bool result = false, currentRumble = false;
                while (!exitOutputThread)
                {
                    if (currentRumble)
                    {
                        lock(outputReport)
                        {
                            result = writeOutput();
                        }

                        currentRumble = false;
                        if (!result)
                        {
                            currentRumble = true;
                            exitOutputThread = true;
                            int thisError = Marshal.GetLastWin32Error();
                            if (lastError != thisError)
                            {
                                Console.WriteLine(Mac.ToString() + " " + System.DateTime.UtcNow.ToString("o") + "> encountered write failure: " + thisError);
                                //Log.LogToGui(Mac.ToString() + " encountered write failure: " + thisError, true);
                                lastError = thisError;
                            }
                        }
                    }

                    if (!currentRumble)
                    {
                        lastError = 0;
                        lock (outReportBuffer)
                        {
                            Monitor.Wait(outReportBuffer);
                            fixed (byte* byteR = outputReport, byteB = outReportBuffer)
                            {
                                for (int i = 0, arlen = BT_OUTPUT_CHANGE_LENGTH; i < arlen; i++)
                                    byteR[i] = byteB[i];
                            }
                            //outReportBuffer.CopyTo(outputReport, 0);
                            outputPendCount--;
                            outputRumble = false;
                        }

                        currentRumble = true;
                    }
                }
            }
            catch (ThreadInterruptedException) { }
        }

        /** Is the device alive and receiving valid sensor input reports? */
        public bool IsAlive()
        {
            return priorInputReport30 != 0xff;
        }

        private byte priorInputReport30 = 0xff;

        private bool synced = false;
        public bool Synced
        {
            get { return synced; }
            set
            {
                if (synced != value)
                {
                    synced = value;
                }
            }
        }

        public bool isSynced()
        {
            return synced;
        }

        public double Latency = 0.0;
        public string error;
        public bool firstReport = true;
        public bool oldCharging = false;
        double curTimeDouble = 0.0;
        double oldTimeDouble = 0.0;
        DateTime utcNow = DateTime.UtcNow;
        bool ds4InactiveFrame = true;
        bool idleInput = true;

        bool timeStampInit = false;
        uint timeStampPrevious = 0;
        uint deltaTimeCurrent = 0;


        const int BT_INPUT_REPORT_CRC32_POS = BT_OUTPUT_REPORT_LENGTH - 4; //last 4 bytes of the 78-sized input report are crc32
        const uint DefaultPolynomial = 0xedb88320u;
        uint HamSeed = 2351727372;

        private unsafe void performDs4Input()
        {
            firstActive = DateTime.UtcNow;
            NativeMethods.HidD_SetNumInputBuffers(hDevice.safeReadHandle.DangerousGetHandle(), 2);
            Queue<long> latencyQueue = new Queue<long>(21); // Set capacity at max + 1 to avoid any resizing
            int tempLatencyCount = 0;
            long oldtime = 0;
            string currerror = string.Empty;
            long curtime = 0;
            Stopwatch sw = new Stopwatch();
            sw.Start();
            timeoutEvent = false;
            ds4InactiveFrame = true;
            idleInput = true;
            bool syncWriteReport = conType != ConnectionType.BT;

            int maxBatteryValue = 0;
            int tempBattery = 0;
            uint tempStamp = 0;
            double elapsedDeltaTime = 0.0;
            uint tempDelta = 0;
            byte tempByte = 0;
            int CRC32_POS_1 = BT_INPUT_REPORT_CRC32_POS + 1,
                CRC32_POS_2 = BT_INPUT_REPORT_CRC32_POS + 2,
                CRC32_POS_3 = BT_INPUT_REPORT_CRC32_POS + 3;
            int crcpos = BT_INPUT_REPORT_CRC32_POS;
            int crcoffset = 0;

            while (!exitInputThread)
            {
                oldCharging = charging;
                currerror = string.Empty;

                if (tempLatencyCount >= 20)
                {
                    latencyQueue.Dequeue();
                    tempLatencyCount--;
                }

                latencyQueue.Enqueue(this.lastTimeElapsed);
                tempLatencyCount++;

                Latency = latencyQueue.Average();

                if (conType == ConnectionType.BT)
                {
                    //HidDevice.ReadStatus res = hDevice.ReadFile(btInputReport);
                    //HidDevice.ReadStatus res = hDevice.ReadAsyncWithFileStream(btInputReport, READ_STREAM_TIMEOUT);
                    HidDevice.ReadStatus res = hDevice.ReadWithFileStream(btInputReport);
                    timeoutEvent = false;
                    if (res == HidDevice.ReadStatus.Success)
                    {
                        //Array.Copy(btInputReport, 2, inputReport, 0, inputReport.Length);
                        fixed (byte* byteP = &btInputReport[2], imp = inputReport)
                        {
                            for (int j = 0; j < BT_INPUT_REPORT_LENGTH-2;j++)
                            {
                                imp[j] = byteP[j];
                            }
                        }

                        //uint recvCrc32 = BitConverter.ToUInt32(btInputReport, BT_INPUT_REPORT_CRC32_POS);
                        uint recvCrc32 = btInputReport[BT_INPUT_REPORT_CRC32_POS] |
                            (uint)(btInputReport[CRC32_POS_1] << 8) |
                            (uint)(btInputReport[CRC32_POS_2] << 16) |
                            (uint)(btInputReport[CRC32_POS_3] << 24);

                        uint calcCrc32 = ~Crc32Algorithm.CalculateFasterBTHash(ref HamSeed, ref btInputReport, ref crcoffset, ref crcpos);
                        if (recvCrc32 != calcCrc32)
                        {
                            //Log.LogToGui("Crc check failed", true);
                            //Console.WriteLine(MacAddress.ToString() + " " + System.DateTime.UtcNow.ToString("o") + "" +
                            //                    "> invalid CRC32 in BT input report: 0x" + recvCrc32.ToString("X8") + " expected: 0x" + calcCrc32.ToString("X8"));

                            //cState.PacketCounter = pState.PacketCounter + 1; //still increase so we know there were lost packets
                            continue;
                        }
                    }
                    else
                    {
                        if (res == HidDevice.ReadStatus.WaitTimedOut)
                        {
                            Log.LogToGui(Mac.ToString() + " disconnected due to timeout", true);
                        }
                        else
                        {
                            int winError = Marshal.GetLastWin32Error();
                            Console.WriteLine(Mac.ToString() + " " + DateTime.UtcNow.ToString("o") + "> disconnect due to read failure: " + winError);
                            //Log.LogToGui(Mac.ToString() + " disconnected due to read failure: " + winError, true);
                        }

                        sendOutputReport(true, true); // Kick Windows into noticing the disconnection.
                        StopOutputUpdate();
                        isDisconnecting = true;
                        uiContext.Send(new SendOrPostCallback(delegate (object state4)
                        {
                            Removal?.Invoke(this, EventArgs.Empty);
                        }), null);

                        timeoutExecuted = true;
                        return;
                    }
                }
                else
                {
                    //HidDevice.ReadStatus res = hDevice.ReadFile(inputReport);
                    //Array.Clear(inputReport, 0, inputReport.Length);
                    //HidDevice.ReadStatus res = hDevice.ReadAsyncWithFileStream(inputReport, READ_STREAM_TIMEOUT);
                    HidDevice.ReadStatus res = hDevice.ReadWithFileStream(inputReport);
                    if (res != HidDevice.ReadStatus.Success)
                    {
                        if (res == HidDevice.ReadStatus.WaitTimedOut)
                        {
                            Log.LogToGui(Mac.ToString() + " disconnected due to timeout", true);
                        }
                        else
                        {
                            int winError = Marshal.GetLastWin32Error();
                            Console.WriteLine(Mac.ToString() + " " + DateTime.UtcNow.ToString("o") + "> disconnect due to read failure: " + winError);
                            //Log.LogToGui(Mac.ToString() + " disconnected due to read failure: " + winError, true);
                        }

                        StopOutputUpdate();
                        isDisconnecting = true;
                        uiContext.Send(new SendOrPostCallback(delegate (object state4)
                        {
                            Removal?.Invoke(this, EventArgs.Empty);
                        }), null);

                        timeoutExecuted = true;
                        return;
                    }
                }

                curTimeDouble = sw.Elapsed.TotalMilliseconds;
                curtime = sw.ElapsedMilliseconds;

                lastTimeElapsed = curtime - oldtime;
                lastTimeElapsedDouble = (curTimeDouble - oldTimeDouble);

                oldtime = curtime;
                oldTimeDouble = curTimeDouble;

                if (conType == ConnectionType.BT && btInputReport[0] != 0x11)
                {
                    //Received incorrect report, skip it
                    continue;
                }

                utcNow = DateTime.UtcNow; // timestamp with UTC in case system time zone changes
                resetHapticState();
                cState.ReportTimeStamp = utcNow;
                cState.LX = inputReport[1];
                cState.LY = inputReport[2];
                cState.RX = inputReport[3];
                cState.RY = inputReport[4];
                cState.L2 = inputReport[8];
                cState.R2 = inputReport[9];

                tempByte = inputReport[5];
                cState.Triangle = (tempByte & (1 << 7)) != 0;
                cState.Circle = (tempByte & (1 << 6)) != 0;
                cState.Cross = (tempByte & (1 << 5)) != 0;
                cState.Square = (tempByte & (1 << 4)) != 0;

                // First 4 bits denote dpad state. Clock representation
                // with 8 meaning centered and 0 meaning DpadUp.
                byte dpad_state = (byte)(tempByte & 0x0F);

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
                    case 8:
                    default: cState.DpadUp = false; cState.DpadDown = false; cState.DpadLeft = false; cState.DpadRight = false; break;
                }

                tempByte = inputReport[6];
                cState.R3 = (tempByte & (1 << 7)) != 0;
                cState.L3 = (tempByte & (1 << 6)) != 0;
                cState.Options = (tempByte & (1 << 5)) != 0;
                cState.Share = (tempByte & (1 << 4)) != 0;
                cState.R1 = (tempByte & (1 << 1)) != 0;
                cState.L1 = (tempByte & (1 << 0)) != 0;

                tempByte = inputReport[7];
                cState.PS = (tempByte & (1 << 0)) != 0;
                cState.TouchButton = (tempByte & 0x02) != 0;
                cState.FrameCounter = (byte)(tempByte >> 2);

                tempByte = inputReport[30];
                charging = (tempByte & 0x10) != 0;
                maxBatteryValue = charging ? BATTERY_MAX_USB : BATTERY_MAX;
                tempBattery = (tempByte & 0x0f) * 100 / maxBatteryValue;
                battery = Math.Min((byte)tempBattery, (byte)100);
                cState.Battery = (byte)battery;
                //System.Diagnostics.Debug.WriteLine("CURRENT BATTERY: " + (inputReport[30] & 0x0f) + " | " + tempBattery + " | " + battery);
                if (tempByte != priorInputReport30)
                {
                    priorInputReport30 = tempByte;
                    //Console.WriteLine(MacAddress.ToString() + " " + System.DateTime.UtcNow.ToString("o") + "> power subsystem octet: 0x" + inputReport[30].ToString("x02"));
                }

                tempStamp = (uint)((ushort)(inputReport[11] << 8) | inputReport[10]);
                if (timeStampInit == false)
                {
                    timeStampInit = true;
                    deltaTimeCurrent = tempStamp * 16u / 3u;
                }
                else if (timeStampPrevious > tempStamp)
                {
                    tempDelta = ushort.MaxValue - timeStampPrevious + tempStamp + 1u;
                    deltaTimeCurrent = tempDelta * 16u / 3u;
                }
                else
                {
                    tempDelta = tempStamp - timeStampPrevious;
                    deltaTimeCurrent = tempDelta * 16u / 3u;
                }

                timeStampPrevious = tempStamp;
                elapsedDeltaTime = 0.000001 * deltaTimeCurrent; // Convert from microseconds to seconds
                cState.elapsedTime = elapsedDeltaTime;

                // XXX DS4State mapping needs fixup, turn touches into an array[4] of structs.  And include the touchpad details there instead.
                try
                {
                    // Only care if one touch packet is detected. Other touch packets
                    // don't seem to contain relevant data. ds4drv does not use them either.
                    for (int touches = Math.Max((int)(inputReport[-1 + DS4Touchpad.TOUCHPAD_DATA_OFFSET - 1]), 1), touchOffset = 0; touches > 0; touches--, touchOffset += 9)
                    //for (int touches = inputReport[-1 + DS4Touchpad.TOUCHPAD_DATA_OFFSET - 1], touchOffset = 0; touches > 0; touches--, touchOffset += 9)
                    {
                        cState.TouchPacketCounter = inputReport[-1 + DS4Touchpad.TOUCHPAD_DATA_OFFSET + touchOffset];
                        cState.Touch1 = (inputReport[0 + DS4Touchpad.TOUCHPAD_DATA_OFFSET + touchOffset] >> 7) != 0 ? false : true; // finger 1 detected
                        cState.Touch1Identifier = (byte)(inputReport[0 + DS4Touchpad.TOUCHPAD_DATA_OFFSET + touchOffset] & 0x7f);
                        cState.Touch2 = (inputReport[4 + DS4Touchpad.TOUCHPAD_DATA_OFFSET + touchOffset] >> 7) != 0 ? false : true; // finger 2 detected
                        cState.Touch2Identifier = (byte)(inputReport[4 + DS4Touchpad.TOUCHPAD_DATA_OFFSET + touchOffset] & 0x7f);
                        cState.Touch1Finger = cState.Touch1 || cState.Touch2; // >= 1 touch detected
                        cState.Touch2Fingers = cState.Touch1 && cState.Touch2; // 2 touches detected
                        int touchX = (((inputReport[2 + DS4Touchpad.TOUCHPAD_DATA_OFFSET + touchOffset] & 0xF) << 8) | inputReport[1 + DS4Touchpad.TOUCHPAD_DATA_OFFSET + touchOffset]);
                        cState.TouchLeft = touchX >= 1920 * 2 / 5 ? false : true;
                        cState.TouchRight = touchX < 1920 * 2 / 5 ? false : true;
                        // Even when idling there is still a touch packet indicating no touch 1 or 2
                        touchpad.handleTouchpad(inputReport, cState, touchOffset);
                    }
                }
                catch { currerror = "Index out of bounds: touchpad"; }

                // Store Gyro and Accel values
                //Array.Copy(inputReport, 13, gyro, 0, 6);
                //Array.Copy(inputReport, 19, accel, 0, 6);
                fixed (byte* pbInput = &inputReport[13], pbGyro = gyro, pbAccel = accel)
                {
                    for (int i = 0; i < 6; i++)
                    {
                        pbGyro[i] = pbInput[i];
                    }

                    for (int i = 6; i < 12; i++)
                    {
                        pbAccel[i-6] = pbInput[i];
                    }
                }
                sixAxis.handleSixaxis(gyro, accel, cState, elapsedDeltaTime);

                /* Debug output of incoming HID data:
                if (cState.L2 == 0xff && cState.R2 == 0xff)
                {
                    Console.Write(MacAddress.ToString() + " " + System.DateTime.UtcNow.ToString("o") + ">");
                    for (int i = 0; i < inputReport.Length; i++)
                        Console.Write(" " + inputReport[i].ToString("x2"));
                    Console.WriteLine();
                }
                */

                if (conType == ConnectionType.SONYWA)
                {
                    bool controllerSynced = inputReport[31] == 0;
                    if (controllerSynced != synced)
                    {
                        synced = controllerSynced;
                        SyncChange?.Invoke(this, EventArgs.Empty);
                        sendOutputReport(true, true);
                    }
                }

                ds4InactiveFrame = cState.FrameCounter == pState.FrameCounter;
                if (!ds4InactiveFrame)
                {
                    isRemoved = false;
                }

                if (conType == ConnectionType.USB)
                {
                    if (idleTimeout == 0)
                    {
                        lastActive = utcNow;
                    }
                    else
                    {
                        idleInput = isDS4Idle();
                        if (!idleInput)
                        {
                            lastActive = utcNow;
                        }
                    }
                }
                else
                {
                    bool shouldDisconnect = false;
                    if (!isRemoved && idleTimeout > 0)
                    {
                        idleInput = isDS4Idle();
                        if (idleInput)
                        {
                            DateTime timeout = lastActive + TimeSpan.FromSeconds(idleTimeout);
                            if (!charging)
                                shouldDisconnect = utcNow >= timeout;
                        }
                        else
                        {
                            lastActive = utcNow;
                        }
                    }
                    else
                    {
                        lastActive = utcNow;
                    }

                    if (shouldDisconnect)
                    {
                        Log.LogToGui(Mac.ToString() + " disconnecting due to idle disconnect", false);

                        if (conType == ConnectionType.BT)
                        {
                            if (DisconnectBT(true))
                            {
                                timeoutExecuted = true;
                                return; // all done
                            }
                        }
                        else if (conType == ConnectionType.SONYWA)
                        {
                            DisconnectDongle();
                        }
                    }
                }

                if (conType == ConnectionType.BT && oldCharging != charging)
                {
                    if (Global.getQuickCharge() && charging)
                    {
                        DisconnectBT(true);
                        timeoutExecuted = true;
                        return;
                    }
                }

                if (Report != null)
                    Report(this, EventArgs.Empty);

                sendOutputReport(syncWriteReport);

                if (!string.IsNullOrEmpty(currerror))
                    error = currerror;
                else if (!string.IsNullOrEmpty(error))
                    error = string.Empty;

                cState.CopyTo(pState);

                lock (eventQueueLock)
                {
                    Action tempAct = null;
                    for (int actInd = 0, actLen = eventQueue.Count; actInd < actLen; actInd++)
                    {
                        tempAct = eventQueue.Dequeue();
                        tempAct.Invoke();
                    }
                }
            }

            timeoutExecuted = true;
        }

        public void FlushHID()
        {
            hDevice.flush_Queue();
        }

        private unsafe void sendOutputReport(bool synchronous, bool force = false)
        {
            setTestRumble();
            setHapticState();

            bool quitOutputThread = false;
            bool usingBT = conType == ConnectionType.BT;

            lock (outReportBuffer)
            {
                bool output = outputPendCount > 0, change = force;

                if (usingBT)
                {
                    outReportBuffer[0] = 0x11;
                    outReportBuffer[1] = (byte)(0x80 | btPollRate); // input report rate
                    // enable rumble (0x01), lightbar (0x02), flash (0x04)
                    outReportBuffer[3] = 0xf7;
                    outReportBuffer[6] = rightLightFastRumble; // fast motor
                    outReportBuffer[7] = leftHeavySlowRumble; // slow motor
                    outReportBuffer[8] = ligtBarColor.red; // red
                    outReportBuffer[9] = ligtBarColor.green; // green
                    outReportBuffer[10] = ligtBarColor.blue; // blue
                    outReportBuffer[11] = ledFlashOn; // flash on duration
                    outReportBuffer[12] = ledFlashOff; // flash off duration

                    fixed (byte* byteR = outputReport, byteB = outReportBuffer)
                    {
                        for (int i = 0, arlen = BT_OUTPUT_CHANGE_LENGTH; !change && i < arlen; i++)
                            change = byteR[i] != byteB[i];
                    }
                }
                else
                {
                    outReportBuffer[0] = 0x05;
                    // enable rumble (0x01), lightbar (0x02), flash (0x04)
                    outReportBuffer[1] = 0xf7;
                    outReportBuffer[4] = rightLightFastRumble; // fast motor
                    outReportBuffer[5] = leftHeavySlowRumble; // slow  motor
                    outReportBuffer[6] = ligtBarColor.red; // red
                    outReportBuffer[7] = ligtBarColor.green; // green
                    outReportBuffer[8] = ligtBarColor.blue; // blue
                    outReportBuffer[9] = ledFlashOn; // flash on duration
                    outReportBuffer[10] = ledFlashOff; // flash off duration

                    fixed (byte* byteR = outputReport, byteB = outReportBuffer)
                    {
                        for (int i = 0, arlen = USB_OUTPUT_CHANGE_LENGTH; !change && i < arlen; i++)
                            change = byteR[i] != byteB[i];
                    }

                    if (change && audio != null)
                    {
                        // Headphone volume levels
                        outReportBuffer[19] = outReportBuffer[20] =
                            Convert.ToByte(audio.getVolume());
                        // Microphone volume level
                        outReportBuffer[21] = Convert.ToByte(micAudio.getVolume());
                    }
                }

                if (synchronous)
                {
                    outputRumble = false;
                    outputPendCount = 3;

                    if (change)
                    {
                        if (usingBT)
                        {
                            Monitor.Enter(outputReport);
                        }

                        outReportBuffer.CopyTo(outputReport, 0);

                        try
                        {
                            if (!writeOutput())
                            {
                                int winError = Marshal.GetLastWin32Error();
                                quitOutputThread = true;
                            }
                        }
                        catch { } // If it's dead already, don't worry about it.

                        if (usingBT)
                        {
                            Monitor.Exit(outputReport);
                        }
                    }
                }
                else
                {
                    //for (int i = 0, arlen = outputReport.Length; !change && i < arlen; i++)
                    //    change = outputReport[i] != outReportBuffer[i];

                    if (output || change)
                    {
                        if (change)
                        {
                            outputPendCount = 3;
                        }

                        outputRumble = true;
                        Monitor.Pulse(outReportBuffer);
                    }
                }
            }

            if (quitOutputThread)
            {
                StopOutputUpdate();
                exitOutputThread = true;
            }
        }

        public bool DisconnectBT(bool callRemoval = false)
        {
            if (Mac != null)
            {
                // Wait for output report to be written
                StopOutputUpdate();
                Console.WriteLine("Trying to disconnect BT device " + Mac);
                IntPtr btHandle = IntPtr.Zero;
                int IOCTL_BTH_DISCONNECT_DEVICE = 0x41000c;

                byte[] btAddr = new byte[8];
                string[] sbytes = Mac.Split(':');
                for (int i = 0; i < 6; i++)
                {
                    // parse hex byte in reverse order
                    btAddr[5 - i] = Convert.ToByte(sbytes[i], 16);
                }

                long lbtAddr = BitConverter.ToInt64(btAddr, 0);

                bool success = false;

                lock (outputReport)
                {
                    NativeMethods.BLUETOOTH_FIND_RADIO_PARAMS p = new NativeMethods.BLUETOOTH_FIND_RADIO_PARAMS();
                    p.dwSize = Marshal.SizeOf(typeof(NativeMethods.BLUETOOTH_FIND_RADIO_PARAMS));
                    IntPtr searchHandle = NativeMethods.BluetoothFindFirstRadio(ref p, ref btHandle);
                    int bytesReturned = 0;

                    while (!success && btHandle != IntPtr.Zero)
                    {
                        success = NativeMethods.DeviceIoControl(btHandle, IOCTL_BTH_DISCONNECT_DEVICE, ref lbtAddr, 8, IntPtr.Zero, 0, ref bytesReturned, IntPtr.Zero);
                        NativeMethods.CloseHandle(btHandle);
                        if (!success)
                        {
                            if (!NativeMethods.BluetoothFindNextRadio(searchHandle, ref btHandle))
                                btHandle = IntPtr.Zero;
                        }
                    }

                    NativeMethods.BluetoothFindRadioClose(searchHandle);
                    Console.WriteLine("Disconnect successful: " + success);
                }

                success = true; // XXX return value indicates failure, but it still works?
                if (success)
                {
                    IsDisconnecting = true;

                    if (callRemoval)
                    {
                        uiContext.Send(new SendOrPostCallback(delegate (object state)
                        {
                            Removal?.Invoke(this, EventArgs.Empty);
                        }), null);

                        //System.Threading.Tasks.Task.Factory.StartNew(() => { Removal?.Invoke(this, EventArgs.Empty); });
                    }
                }

                return success;
            }

            return false;
        }

        public bool DisconnectDongle(bool remove = false)
        {
            bool result = false;
            byte[] disconnectReport = new byte[65];
            disconnectReport[0] = 0xe2;
            disconnectReport[1] = 0x02;
            Array.Clear(disconnectReport, 2, 63);

            if (remove)
                StopOutputUpdate();

            lock (outputReport)
            {
                result = hDevice.WriteFeatureReport(disconnectReport);
            }

            if (result && remove)
            {
                isDisconnecting = true;

                uiContext.Send(new SendOrPostCallback(delegate (object state4)
                {
                    Removal?.Invoke(this, EventArgs.Empty);
                }), null);

                //System.Threading.Tasks.Task.Factory.StartNew(() => { Removal?.Invoke(this, EventArgs.Empty); });
                //Removal?.Invoke(this, EventArgs.Empty);
            }
            else if (result && !remove)
            {
                isRemoved = true;
            }

            return result;
        }

        private DS4HapticState testRumble = new DS4HapticState();

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
                pushHapticState(ref testRumble);
                if (testRumble.RumbleMotorsExplicitlyOff)
                    testRumble.RumbleMotorsExplicitlyOff = false;
            }
        }

        public DS4State getCurrentState()
        {
            return cState.Clone();
        }

        public DS4State getPreviousState()
        {
            return pState.Clone();
        }

        public void getCurrentState(DS4State state)
        {
            cState.CopyTo(state);
        }

        public void getPreviousState(DS4State state)
        {
            pState.CopyTo(state);
        }

        public DS4State getCurrentStateRef()
        {
            return cState;
        }

        public DS4State getPreviousStateRef()
        {
            return pState;
        }

        private bool isDS4Idle()
        {
            if (cState.Square || cState.Cross || cState.Circle || cState.Triangle)
                return false;
            if (cState.DpadUp || cState.DpadLeft || cState.DpadDown || cState.DpadRight)
                return false;
            if (cState.L3 || cState.R3 || cState.L1 || cState.R1 || cState.Share || cState.Options)
                return false;
            if (cState.L2 != 0 || cState.R2 != 0)
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

        private DS4HapticState[] hapticState = new DS4HapticState[1];
        private int hapticStackIndex = 0;
        private void resetHapticState()
        {
            hapticStackIndex = 0;
        }

        delegate void HapticItem(ref DS4HapticState haptic);

        // Use the "most recently set" haptic state for each of light bar/motor.
        private void setHapticState()
        {
            byte lightBarFlashDurationOn = ledFlashOn, lightBarFlashDurationOff = ledFlashOff;
            byte rumbleMotorStrengthLeftHeavySlow = leftHeavySlowRumble,
                rumbleMotorStrengthRightLightFast = rightLightFastRumble;
            int hapticLen = hapticState.Length;
            for (int i=0; i < hapticLen; i++)
            {
                if (i == hapticStackIndex)
                    break; // rest haven't been used this time

                ((HapticItem)((ref DS4HapticState haptic) => {
                    if (haptic.IsLightBarSet())
                    {
                        ligtBarColor = haptic.LightBarColor;
                        lightBarFlashDurationOn = haptic.LightBarFlashDurationOn;
                        lightBarFlashDurationOff = haptic.LightBarFlashDurationOff;
                    }

                    if (haptic.IsRumbleSet())
                    {
                        rumbleMotorStrengthLeftHeavySlow = haptic.RumbleMotorStrengthLeftHeavySlow;
                        rumbleMotorStrengthRightLightFast = haptic.RumbleMotorStrengthRightLightFast;
                    }
                }))(ref hapticState[i]);
            }

            ledFlashOn = lightBarFlashDurationOn;
            ledFlashOff = lightBarFlashDurationOff;
            leftHeavySlowRumble = rumbleMotorStrengthLeftHeavySlow;
            rightLightFastRumble = rumbleMotorStrengthRightLightFast;
        }

        public void pushHapticState(ref DS4HapticState hs)
        {
            int hapsLen = hapticState.Length;
            if (hapticStackIndex == hapsLen)
            {
                DS4HapticState[] newHaptics = new DS4HapticState[hapsLen + 1];
                Array.Copy(hapticState, newHaptics, hapsLen);
                hapticState = newHaptics;
            }

            hapticState[hapticStackIndex++] = hs;
        }

        override
        public string ToString()
        {
            return Mac;
        }

        public void runRemoval()
        {
            Removal?.Invoke(this, EventArgs.Empty);
        }

        public void removeReportHandlers()
        {
            this.Report = null;
        }

        public void queueEvent(Action act)
        {
            lock (eventQueueLock)
            {
                eventQueue.Enqueue(act);
            }
        }

        public void updateSerial()
        {
            hDevice.resetSerial();
            string tempMac = hDevice.readSerial();
            if (tempMac != Mac)
            {
                Mac = tempMac;
                SerialChange?.Invoke(this, EventArgs.Empty);
            }
        }

        public bool isValidSerial()
        {
            return !Mac.Equals(blankSerial);
        }

        public static bool isValidSerial(string test)
        {
            return !test.Equals(blankSerial);
        }
    }
}
