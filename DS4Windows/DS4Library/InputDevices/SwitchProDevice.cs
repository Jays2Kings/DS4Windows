using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DS4Windows.InputDevices
{
    public class SwitchProDevice : DS4Device
    {
        public class RumbleTableData
        {
            public byte high;
            public ushort low;
            public int amp;

            public RumbleTableData(byte high, ushort low, int amp)
            {
                this.high = high;
                this.low = low;
                this.amp = amp;
            }
        }

        public static class SwitchProSubCmd
        {
            public const byte SET_INPUT_MODE = 0x03;
            public const byte SET_LOW_POWER_STATE = 0x08;
            public const byte SPI_FLASH_READ = 0x10;
            public const byte SET_LIGHTS = 0x30; // LEDs on controller
            public const byte SET_HOME_LIGHT = 0x38;
            public const byte ENABLE_IMU = 0x40;
            public const byte SET_IMU_SENS = 0x41;
            public const byte ENABLE_VIBRATION = 0x48;
        }

        private const int AMP_REAL_MIN = 0;
        //private const int AMP_REAL_MAX = 1003;
        private const int AMP_LIMIT_MAX = 404;

        private static RumbleTableData[] fixedRumbleTable = new RumbleTableData[]
        {
            new RumbleTableData(high: 0x00, low: 0x0040, amp: 0),
            new RumbleTableData(high: 0x02, low: 0x8040, amp: 10), new RumbleTableData(high: 0x04, low: 0x0041, amp: 12), new RumbleTableData(high: 0x06, low: 0x8041, amp: 14),
            new RumbleTableData(high: 0x08, low: 0x0042, amp: 17), new RumbleTableData(high: 0x0A, low: 0x8042, amp: 20), new RumbleTableData(high: 0x0C, low: 0x0043, amp: 24),
            new RumbleTableData(high: 0x0E, low: 0x8043, amp: 28), new RumbleTableData(high: 0x10, low: 0x0044, amp: 33), new RumbleTableData(high: 0x12, low: 0x8044, amp: 40),
            new RumbleTableData(high: 0x14, low: 0x0045, amp: 47), new RumbleTableData(high: 0x16, low: 0x8045, amp: 56), new RumbleTableData(high: 0x18, low: 0x0046, amp: 67),
            new RumbleTableData(high: 0x1A, low: 0x8046, amp: 80), new RumbleTableData(high: 0x1C, low: 0x0047, amp: 95), new RumbleTableData(high: 0x1E, low: 0x8047, amp: 112),
            new RumbleTableData(high: 0x20, low: 0x0048, amp: 117), new RumbleTableData(high: 0x22, low: 0x8048, amp: 123), new RumbleTableData(high: 0x24, low: 0x0049, amp: 128),
            new RumbleTableData(high: 0x26, low: 0x8049, amp: 134), new RumbleTableData(high: 0x28, low: 0x004A, amp: 140), new RumbleTableData(high: 0x2A, low: 0x804A, amp: 146),
            new RumbleTableData(high: 0x2C, low: 0x004B, amp: 152), new RumbleTableData(high: 0x2E, low: 0x804B, amp: 159), new RumbleTableData(high: 0x30, low: 0x004C, amp: 166),
            new RumbleTableData(high: 0x32, low: 0x804C, amp: 173), new RumbleTableData(high: 0x34, low: 0x004D, amp: 181), new RumbleTableData(high: 0x36, low: 0x804D, amp: 189),
            new RumbleTableData(high: 0x38, low: 0x004E, amp: 198), new RumbleTableData(high: 0x3A, low: 0x804E, amp: 206), new RumbleTableData(high: 0x3C, low: 0x004F, amp: 215),
            new RumbleTableData(high: 0x3E, low: 0x804F, amp: 225), new RumbleTableData(high: 0x40, low: 0x0050, amp: 230), new RumbleTableData(high: 0x42, low: 0x8050, amp: 235),
            new RumbleTableData(high: 0x44, low: 0x0051, amp: 240), new RumbleTableData(high: 0x46, low: 0x8051, amp: 245), new RumbleTableData(high: 0x48, low: 0x0052, amp: 251),
            new RumbleTableData(high: 0x4A, low: 0x8052, amp: 256), new RumbleTableData(high: 0x4C, low: 0x0053, amp: 262), new RumbleTableData(high: 0x4E, low: 0x8053, amp: 268),
            new RumbleTableData(high: 0x50, low: 0x0054, amp: 273), new RumbleTableData(high: 0x52, low: 0x8054, amp: 279), new RumbleTableData(high: 0x54, low: 0x0055, amp: 286),
            new RumbleTableData(high: 0x56, low: 0x8055, amp: 292), new RumbleTableData(high: 0x58, low: 0x0056, amp: 298), new RumbleTableData(high: 0x5A, low: 0x8056, amp: 305),
            new RumbleTableData(high: 0x5C, low: 0x0057, amp: 311), new RumbleTableData(high: 0x5E, low: 0x8057, amp: 318), new RumbleTableData(high: 0x60, low: 0x0058, amp: 325),
            new RumbleTableData(high: 0x62, low: 0x8058, amp: 332), new RumbleTableData(high: 0x64, low: 0x0059, amp: 340), new RumbleTableData(high: 0x66, low: 0x8059, amp: 347),
            new RumbleTableData(high: 0x68, low: 0x005A, amp: 355), new RumbleTableData(high: 0x6A, low: 0x805A, amp: 362), new RumbleTableData(high: 0x6C, low: 0x005B, amp: 370),
            new RumbleTableData(high: 0x6E, low: 0x805B, amp: 378), new RumbleTableData(high: 0x70, low: 0x005C, amp: 387), new RumbleTableData(high: 0x72, low: 0x805C, amp: 395),
            new RumbleTableData(high: 0x74, low: 0x005D, amp: 404), new RumbleTableData(high: 0x76, low: 0x805D, amp: 413), new RumbleTableData(high: 0x78, low: 0x005E, amp: 422),
            new RumbleTableData(high: 0x7A, low: 0x805E, amp: 431), new RumbleTableData(high: 0x7C, low: 0x005F, amp: 440), new RumbleTableData(high: 0x7E, low: 0x805F, amp: 450),
            new RumbleTableData(high: 0x80, low: 0x0060, amp: 460), new RumbleTableData(high: 0x82, low: 0x8060, amp: 470), new RumbleTableData(high: 0x84, low: 0x0061, amp: 480),
            new RumbleTableData(high: 0x86, low: 0x8061, amp: 491), new RumbleTableData(high: 0x88, low: 0x0062, amp: 501), new RumbleTableData(high: 0x8A, low: 0x8062, amp: 512),
            new RumbleTableData(high: 0x8C, low: 0x0063, amp: 524), new RumbleTableData(high: 0x8E, low: 0x8063, amp: 535), new RumbleTableData(high: 0x90, low: 0x0064, amp: 547),
            new RumbleTableData(high: 0x92, low: 0x8064, amp: 559), new RumbleTableData(high: 0x94, low: 0x0065, amp: 571), new RumbleTableData(high: 0x96, low: 0x8065, amp: 584),
            new RumbleTableData(high: 0x98, low: 0x0066, amp: 596), new RumbleTableData(high: 0x9A, low: 0x8066, amp: 609), new RumbleTableData(high: 0x9C, low: 0x0067, amp: 623),
            new RumbleTableData(high: 0x9E, low: 0x8067, amp: 636), new RumbleTableData(high: 0xA0, low: 0x0068, amp: 650), new RumbleTableData(high: 0xA2, low: 0x8068, amp: 665),
            new RumbleTableData(high: 0xA4, low: 0x0069, amp: 679), new RumbleTableData(high: 0xA6, low: 0x8069, amp: 694), new RumbleTableData(high: 0xA8, low: 0x006A, amp: 709),
            new RumbleTableData(high: 0xAA, low: 0x806A, amp: 725), new RumbleTableData(high: 0xAC, low: 0x006B, amp: 741), new RumbleTableData(high: 0xAE, low: 0x806B, amp: 757),
            new RumbleTableData(high: 0xB0, low: 0x006C, amp: 773), new RumbleTableData(high: 0xB2, low: 0x806C, amp: 790), new RumbleTableData(high: 0xB4, low: 0x006D, amp: 808),
            new RumbleTableData(high: 0xB6, low: 0x806D, amp: 825), new RumbleTableData(high: 0xB8, low: 0x006E, amp: 843), new RumbleTableData(high: 0xBA, low: 0x806E, amp: 862),
            new RumbleTableData(high: 0xBC, low: 0x006F, amp: 881), new RumbleTableData(high: 0xBE, low: 0x806F, amp: 900), new RumbleTableData(high: 0xC0, low: 0x0070, amp: 920),
            new RumbleTableData(high: 0xC2, low: 0x8070, amp: 940), new RumbleTableData(high: 0xC4, low: 0x0071, amp: 960), new RumbleTableData(high: 0xC6, low: 0x8071, amp: 981),
            new RumbleTableData(high: 0xC8, low: 0x0072, amp: 1003),
        };

        private static RumbleTableData[] compiledRumbleTable = new Func<RumbleTableData[]>(() =>
        {
            RumbleTableData[] tmpBuffer = new RumbleTableData[fixedRumbleTable.Last().amp + 1];
            int currentOffset = 0;
            RumbleTableData previousEntry = fixedRumbleTable[0];
            tmpBuffer[currentOffset] = previousEntry;
            int currentAmp = previousEntry.amp + 1;
            currentOffset++;

            for (int i = 1; i < fixedRumbleTable.Length; i++)
            //foreach(RumbleTableData entry in fixedRumbleTable)
            {
                RumbleTableData entry = fixedRumbleTable[i];
                if (currentAmp < entry.amp)
                {
                    while (currentAmp < entry.amp)
                    {
                        tmpBuffer[currentOffset] = previousEntry;
                        currentOffset++;
                        currentAmp++;
                    }
                }

                tmpBuffer[currentOffset] = entry;
                currentAmp = entry.amp + 1;
                currentOffset++;
                previousEntry = entry;
            }

            //fixedRumbleTable = null;
            return tmpBuffer;
        })();

        public struct StickAxisData
        {
            public ushort max;
            public ushort mid;
            public ushort min;
        };

        private static byte[] commandBuffHeader =
            { 0x0, 0x1, 0x40, 0x40, 0x0, 0x1, 0x40, 0x40 };

        private const int SUBCOMMAND_HEADER_LEN = 8;
        private const int SUBCOMMAND_BUFFER_LEN = 64;
        private const int SUBCOMMAND_RESPONSE_TIMEOUT = 500;
        public const int IMU_XAXIS_IDX = 0, IMU_YAW_IDX = 0;
        public const int IMU_YAXIS_IDX = 1, IMU_PITCH_IDX = 1;
        public const int IMU_ZAXIS_IDX = 2, IMU_ROLL_IDX = 2;

        public const short ACCEL_ORIG_HOR_OFFSET_X = -688;
        public const short ACCEL_ORIG_HOR_OFFSET_Y = 0;
        public const short ACCEL_ORIG_HOR_OFFSET_Z = 4038;

        //private const ushort SAMPLE_STICK_MAX = 3200;
        private const ushort SAMPLE_STICK_MAX = 3300;
        private const ushort SAMPLE_STICK_MIN = 500;
        private const ushort SAMPLE_STICK_MID = SAMPLE_STICK_MAX - SAMPLE_STICK_MIN;
        private const double STICK_AXIS_MAX_CUTOFF = 0.96;
        private const double STICK_AXIS_MIN_CUTOFF = 1.04;

        private StickAxisData leftStickXData;
        private StickAxisData leftStickYData;
        private StickAxisData rightStickXData;
        private StickAxisData rightStickYData;

        private const string BLUETOOTH_HID_GUID = "{00001124-0000-1000-8000-00805F9B34FB}";

        private byte frameCount = 0;
        public byte FrameCount { get => frameCount; set => frameCount = value; }

        private const int INPUT_REPORT_LEN = 362;
        private const int OUTPUT_REPORT_LEN = 49;
        private const int RUMBLE_REPORT_LEN = 64;
        // Converts raw gyro input value to dps. Equal to (4588/65535)
        private const float GYRO_IN_DEG_SEC_FACTOR = 0.070f;
        private new const int WARN_INTERVAL_BT = 40;
        private new const int WARN_INTERVAL_USB = 30;
        private byte[] inputReportBuffer;
        private byte[] outputReportBuffer;
        private byte[] rumbleReportBuffer;

        public int InputReportLen { get => INPUT_REPORT_LEN; }
        public int OutputReportLen { get => OUTPUT_REPORT_LEN; }
        public int RumbleReportLen { get => RUMBLE_REPORT_LEN; }

        private ushort[] leftStickCalib = new ushort[6];
        private ushort leftStickOffsetX = 0;
        private ushort leftStickOffsetY = 0;

        private ushort[] rightStickCalib = new ushort[6];
        private ushort rightStickOffsetX = 0;
        private ushort rightStickOffsetY = 0;

        //public ushort[] LeftStickCalib { get => leftStickCalib; }
        //public ushort[] RightStickCalib { get => rightStickCalib; }

        private short[] accelNeutral = new short[3];
        private short[] accelSens = new short[3];
        private double[] accelSensMulti = new double[3];
        private double[] accelCoeff = new double[3];

        private short[] gyroBias = new short[3];
        private short[] gyroSens = new short[3];
        private short[] gyroCalibOffsets = new short[3];
        private double[] gyroSensMulti = new double[3];
        private double[] gyroCoeff = new double[3];

        private double combLatency;
        public double CombLatency { get => combLatency; set => combLatency = value; }

        private bool enableHomeLED = true;
        public bool EnableHomeLED { get => enableHomeLED; set => enableHomeLED = value; }

        private SwitchProControllerOptions nativeOptionsStore;

        public override event ReportHandler<EventArgs> Report = null;
        public override event EventHandler<EventArgs> Removal = null;

        public override event EventHandler BatteryChanged;
        public override event EventHandler ChargingChanged;

        public SwitchProDevice(HidDevice hidDevice,
            string disName, VidPidFeatureSet featureSet = VidPidFeatureSet.DefaultDS4) :
            base(hidDevice, disName, featureSet)
        {
            runCalib = false;
            synced = true;

            leftStickXData.max = SAMPLE_STICK_MAX; leftStickXData.min = SAMPLE_STICK_MIN;
            leftStickXData.mid = SAMPLE_STICK_MID;

            leftStickYData.max = SAMPLE_STICK_MAX; leftStickYData.min = SAMPLE_STICK_MIN;
            leftStickYData.mid = SAMPLE_STICK_MID;

            rightStickXData.max = SAMPLE_STICK_MAX; rightStickXData.min = SAMPLE_STICK_MIN;
            rightStickXData.mid = SAMPLE_STICK_MID;

            rightStickYData.max = SAMPLE_STICK_MAX; rightStickYData.min = SAMPLE_STICK_MIN;
            rightStickYData.mid = SAMPLE_STICK_MID;

            warnInterval = WARN_INTERVAL_BT;

            DeviceSlotNumberChanged += (sender, e) => {
                CalculateDeviceSlotMask();
            };
        }

        public override void PostInit()
        {
            deviceType = InputDeviceType.SwitchPro;
            gyroMouseSensSettings = new GyroMouseSens();
            conType = DetermineConnectionType(hDevice);
            optionsStore = nativeOptionsStore = new SwitchProControllerOptions(deviceType);
            SetupOptionsEvents();

            if (conType == ConnectionType.BT)
            {
                warnInterval = WARN_INTERVAL_BT;
            }
            else
            {
                warnInterval = WARN_INTERVAL_USB;
            }

            inputReportBuffer = new byte[INPUT_REPORT_LEN];
            outputReportBuffer = new byte[OUTPUT_REPORT_LEN];
            rumbleReportBuffer = new byte[RUMBLE_REPORT_LEN];

            if (!hDevice.IsFileStreamOpen())
            {
                hDevice.OpenFileStream(inputReportBuffer.Length);
            }
        }

        public static ConnectionType DetermineConnectionType(HidDevice hDevice)
        {
            ConnectionType result;
            if (hDevice.DevicePath.ToUpper().Contains(BLUETOOTH_HID_GUID))
            {
                result = ConnectionType.BT;
            }
            else
            {
                result = ConnectionType.USB;
            }

            return result;
        }

        public override void StartUpdate()
        {
            this.inputReportErrorCount = 0;
            SetOperational();

            if (ds4Input == null)
            {
                ds4Input = new Thread(ReadInput);
                ds4Input.IsBackground = true;
                ds4Input.Priority = ThreadPriority.AboveNormal;
                ds4Input.Name = "Switch Pro Reader Thread";
                ds4Input.Start();
            }
        }

        protected override void StopOutputUpdate()
        {
        }

        protected unsafe void ReadInput()
        {
            byte[] stick_raw = { 0, 0, 0 };
            byte[] stick_raw2 = { 0, 0, 0 };
            short[] accel_raw = { 0, 0, 0 };
            short[] gyro_raw = new short[9];
            short[] gyro_out = new short[9];
            //short gyroYaw = 0, gyroYaw2 = 0, gyroYaw3 = 0;
            //short gyroPitch = 0, gyroPitch2 = 0, gyroPitch3 = 0;
            //short gyroRoll = 0, gyroRoll2 = 0, gyroRoll3 = 0;
            short tempShort = 0;
            int tempAxis = 0;

            /*long currentTime = 0;
            long previousTime = 0;
            long deltaElapsed = 0;
            double lastElapsed;
            double tempTimeElapsed;
            bool firstReport = true;
            */

            unchecked
            {
                firstActive = DateTime.UtcNow;
                NativeMethods.HidD_SetNumInputBuffers(hDevice.safeReadHandle.DangerousGetHandle(), 2);
                Queue<long> latencyQueue = new Queue<long>(21); // Set capacity at max + 1 to avoid any resizing
                int tempLatencyCount = 0;
                long oldtime = 0;
                string currerror = string.Empty;
                long curtime = 0;
                long testelapsed = 0;
                timeoutEvent = false;
                ds4InactiveFrame = true;
                idleInput = true;
                bool syncWriteReport = conType != ConnectionType.BT;
                bool forceWrite = false;

                int maxBatteryValue = 0;
                int tempBattery = 0;
                bool tempCharging = charging;
                uint tempStamp = 0;
                double elapsedDeltaTime = 0.0;
                uint tempDelta = 0;
                byte tempByte = 0;

                long latencySum = 0;
                standbySw.Start();

                while (!exitInputThread)
                {
                    oldCharging = charging;
                    currerror = string.Empty;

                    readWaitEv.Set();

                    HidDevice.ReadStatus res = hDevice.ReadWithFileStream(inputReportBuffer);
                    if (res == HidDevice.ReadStatus.Success)
                    {
                        if (inputReportBuffer[0] != 0x30)
                        {
                            //Console.WriteLine("Got unexpected input report id 0x{0:X2}. Try again",
                            //    inputReportBuffer[0]);

                            readWaitEv.Reset();
                            inputReportErrorCount++;
                            if (inputReportErrorCount > 10)
                            {
                                exitInputThread = true;
                                isDisconnecting = true;
                                Removal?.Invoke(this, EventArgs.Empty);
                            }

                            continue;
                        }
                    }
                    else
                    {
                        readWaitEv.Reset();
                        exitInputThread = true;
                        isDisconnecting = true;
                        Removal?.Invoke(this, EventArgs.Empty);
                        continue;
                    }

                    readWaitEv.Wait();
                    readWaitEv.Reset();

                    inputReportErrorCount = 0;
                    curtime = Stopwatch.GetTimestamp();
                    testelapsed = curtime - oldtime;
                    lastTimeElapsedDouble = testelapsed * (1.0 / Stopwatch.Frequency) * 1000.0;
                    lastTimeElapsed = (long)lastTimeElapsedDouble;
                    oldtime = curtime;
                    elapsedDeltaTime = lastTimeElapsedDouble * .001;
                    combLatency += elapsedDeltaTime;

                    if (elapsedDeltaTime <= 0.005)
                    {
                        continue;
                    }

                    if (tempLatencyCount >= 20)
                    {
                        latencySum -= latencyQueue.Dequeue();
                        tempLatencyCount--;
                    }

                    latencySum += this.lastTimeElapsed;
                    latencyQueue.Enqueue(this.lastTimeElapsed);
                    tempLatencyCount++;

                    //Latency = latencyQueue.Average();
                    Latency = latencySum / (double)tempLatencyCount;

                    utcNow = DateTime.UtcNow; // timestamp with UTC in case system time zone changes
                    cState.PacketCounter = pState.PacketCounter + 1;
                    cState.ReportTimeStamp = utcNow;

                    cState.elapsedTime = combLatency;
                    cState.totalMicroSec = pState.totalMicroSec + (uint)(combLatency * 1000000);
                    combLatency = 0.0;

                    if ((this.featureSet & VidPidFeatureSet.NoBatteryReading) == 0)
                    {
                        tempByte = inputReportBuffer[2];
                        tempBattery = ((tempByte & 0xE0) >> 4) * 100 / 8;
                        tempBattery = Math.Min(tempBattery, 100);
                        if (tempBattery != battery)
                        {
                            battery = tempBattery;
                            BatteryChanged?.Invoke(this, EventArgs.Empty);
                        }

                        cState.Battery = (byte)tempBattery;

                        tempCharging = (tempByte & 0x10) != 0;
                        if (tempCharging != charging)
                        {
                            charging = tempCharging;
                            ChargingChanged?.Invoke(this, EventArgs.Empty);
                        }
                    }
                    else
                    {
                        battery = 99;
                        cState.Battery = 99;
                    }

                    tempByte = inputReportBuffer[3];
                    cState.Circle = (tempByte & 0x08) != 0;
                    cState.Cross = (tempByte & 0x04) != 0;
                    cState.Triangle = (tempByte & 0x02) != 0;
                    cState.Square = (tempByte & 0x01) != 0;
                    cState.R1 = (tempByte & 0x40) != 0;
                    cState.R2Btn = (tempByte & 0x80) != 0;
                    cState.R2 = (byte)(cState.R2Btn ? 255 : 0);

                    tempByte = inputReportBuffer[4];
                    cState.Share = (tempByte & 0x01) != 0;
                    cState.Options = (tempByte & 0x02) != 0;
                    cState.PS = (tempByte & 0x10) != 0;
                    //cState.Capture = (tempByte & 0x20) != 0;
                    cState.L3 = (tempByte & 0x08) != 0;
                    cState.R3 = (tempByte & 0x04) != 0;

                    tempByte = inputReportBuffer[5];
                    cState.DpadUp = (tempByte & 0x02) != 0;
                    cState.DpadDown = (tempByte & 0x01) != 0;
                    cState.DpadLeft = (tempByte & 0x08) != 0;
                    cState.DpadRight = (tempByte & 0x04) != 0;
                    cState.L1 = (tempByte & 0x40) != 0;
                    cState.L2Btn = (tempByte & 0x80) != 0;
                    cState.L2 = (byte)(cState.L2Btn ? 255 : 0);

                    stick_raw[0] = inputReportBuffer[6];
                    stick_raw[1] = inputReportBuffer[7];
                    stick_raw[2] = inputReportBuffer[8];

                    tempAxis = (stick_raw[0] | ((stick_raw[1] & 0x0F) << 8)) - leftStickOffsetX;
                    tempAxis = tempAxis > leftStickXData.max ? leftStickXData.max : (tempAxis < leftStickXData.min ? leftStickXData.min : tempAxis);
                    cState.LX = (byte)((tempAxis - leftStickXData.min) / (double)(leftStickXData.max - leftStickXData.min) * 255);

                    tempAxis = ((stick_raw[1] >> 4) | (stick_raw[2] << 4)) - leftStickOffsetY;
                    tempAxis = tempAxis > leftStickYData.max ? leftStickYData.max : (tempAxis < leftStickYData.min ? leftStickYData.min : tempAxis);
                    cState.LY = (byte)((((tempAxis - leftStickYData.min) / (double)(leftStickYData.max - leftStickYData.min) - 0.5) * -1.0 + 0.5) * 255);

                    stick_raw2[0] = inputReportBuffer[9];
                    stick_raw2[1] = inputReportBuffer[10];
                    stick_raw2[2] = inputReportBuffer[11];

                    tempAxis = (stick_raw2[0] | ((stick_raw2[1] & 0x0F) << 8)) - rightStickOffsetX;
                    tempAxis = tempAxis > rightStickXData.max ? rightStickXData.max : (tempAxis < rightStickXData.min ? rightStickXData.min : tempAxis);
                    cState.RX = (byte)((tempAxis - rightStickXData.min) / (double)(rightStickXData.max - rightStickXData.min) * 255);

                    tempAxis = ((stick_raw2[1] >> 4) | (stick_raw2[2] << 4)) - rightStickOffsetY;
                    tempAxis = tempAxis > rightStickYData.max ? rightStickYData.max : (tempAxis < rightStickYData.min ? rightStickYData.min : tempAxis);
                    //cState.RY = (byte)((tempAxis - STICK_MIN) / (STICK_MAX - STICK_MIN) * 255);
                    cState.RY = (byte)((((tempAxis - rightStickYData.min) / (double)(rightStickYData.max - rightStickYData.min) - 0.5) * -1.0 + 0.5) * 255);

                    for (int i = 0; i < 3; i++)
                    {
                        int data_offset = i * 12;
                        int gyro_offset = i * 3;
                        accel_raw[IMU_XAXIS_IDX] = (short)((ushort)(inputReportBuffer[16 + data_offset] << 8) | inputReportBuffer[15 + data_offset]);
                        accel_raw[IMU_YAXIS_IDX] = (short)((ushort)(inputReportBuffer[14 + data_offset] << 8) | inputReportBuffer[13 + data_offset]);
                        accel_raw[IMU_ZAXIS_IDX] = (short)((ushort)(inputReportBuffer[18 + data_offset] << 8) | inputReportBuffer[17 + data_offset]);

                        tempShort = gyro_raw[IMU_YAW_IDX + gyro_offset] = (short)((ushort)(inputReportBuffer[24 + data_offset] << 8) | inputReportBuffer[23 + data_offset]);
                        //gyro_out[IMU_YAW_IDX + gyro_offset] = (short)(tempShort - device.gyroBias[IMU_YAW_IDX]);
                        gyro_out[IMU_YAW_IDX + gyro_offset] = (short)(tempShort);

                        tempShort = gyro_raw[IMU_PITCH_IDX + gyro_offset] = (short)((ushort)(inputReportBuffer[22 + data_offset] << 8) | inputReportBuffer[21 + data_offset]);
                        //gyro_out[IMU_PITCH_IDX + gyro_offset] = (short)(tempShort - device.gyroBias[IMU_PITCH_IDX]);
                        gyro_out[IMU_PITCH_IDX + gyro_offset] = (short)(tempShort);

                        tempShort = gyro_raw[IMU_ROLL_IDX + gyro_offset] = (short)((ushort)(inputReportBuffer[20 + data_offset] << 8) | inputReportBuffer[19 + data_offset]);
                        //gyro_out[IMU_ROLL_IDX + gyro_offset] = (short)(tempShort - device.gyroBias[IMU_ROLL_IDX]);
                        gyro_out[IMU_ROLL_IDX + gyro_offset] = (short)(tempShort);

                        //Console.WriteLine($"IDX: ({i}) Accel: X({accel_raw[IMU_XAXIS_IDX]}) Y({accel_raw[IMU_YAXIS_IDX]}) Z({accel_raw[IMU_ZAXIS_IDX]})");
                        //Console.WriteLine($"IDX: ({i}) Gyro: Yaw({gyro_raw[IMU_YAW_IDX + gyro_offset]}) Pitch({gyro_raw[IMU_PITCH_IDX + gyro_offset]}) Roll({gyro_raw[IMU_ROLL_IDX + gyro_offset]})");
                        //Console.WriteLine($"IDX: ({i}) Gyro OUT: Yaw({gyro_out[IMU_YAW_IDX + gyro_offset]}) Pitch({gyro_out[IMU_PITCH_IDX + gyro_offset]}) Roll({gyro_out[IMU_ROLL_IDX + gyro_offset]})");
                        //Console.WriteLine();
                    }

                    // For Accel, just use most recent sampled values
                    short accelX = accel_raw[IMU_XAXIS_IDX];
                    short accelY = accel_raw[IMU_YAXIS_IDX];
                    short accelZ = accel_raw[IMU_ZAXIS_IDX];

                    // Just use most recent sample for now
                    short gyroYaw = (short)(-1 * (gyro_out[6 + IMU_YAW_IDX] - gyroBias[IMU_YAW_IDX] + gyroCalibOffsets[IMU_YAW_IDX]));
                    short gyroPitch = (short)(gyro_out[6 + IMU_PITCH_IDX] - gyroBias[IMU_PITCH_IDX] - gyroCalibOffsets[IMU_PITCH_IDX]);
                    short gyroRoll = (short)(gyro_out[6 + IMU_ROLL_IDX] - gyroBias[IMU_ROLL_IDX] - gyroCalibOffsets[IMU_ROLL_IDX]);
                    //cState.Motion.populate(gyroYaw, gyroPitch, gyroRoll, accelX, accelY, accelZ, cState.elapsedTime, pState.Motion);
                    
                    // Need to populate the SixAxis object manually to work around conversions
                    //Console.WriteLine("GyroYaw: {0}", gyroYaw);
                    SixAxis tempMotion = cState.Motion;
                    tempMotion.gyroYawFull = gyroYaw; tempMotion.gyroPitchFull = -gyroPitch; tempMotion.gyroRollFull = gyroRoll;
                    tempMotion.accelXFull = accelX * 2; tempMotion.accelYFull = -accelZ * 2; tempMotion.accelZFull = -accelY * 2;
                    
                    tempMotion.elapsed = elapsedDeltaTime;
                    tempMotion.previousAxis = pState.Motion;
                    tempMotion.gyroYaw = gyroYaw / 256; tempMotion.gyroPitch = -gyroPitch / 256; tempMotion.gyroRoll = gyroRoll / 256;
                    tempMotion.accelX = accelX / 31; tempMotion.accelY = -accelZ / 31; tempMotion.accelZ = -accelY / 31;
                    //tempMotion.outputAccelX = tempMotion.accelX; tempMotion.outputAccelY = tempMotion.accelY; tempMotion.outputAccelZ = tempMotion.accelZ;
                    tempMotion.outputAccelX = 0; tempMotion.outputAccelY = 0; tempMotion.outputAccelZ = 0;
                    tempMotion.outputGyroControls = false;
                    //Console.WriteLine(gyroRoll);
                    tempMotion.accelXG = (accelX * 2) / DS4Windows.SixAxis.F_ACC_RES_PER_G;
                    tempMotion.accelYG = (-accelZ * 2) / DS4Windows.SixAxis.F_ACC_RES_PER_G;
                    tempMotion.accelZG = (-accelY * 2) / DS4Windows.SixAxis.F_ACC_RES_PER_G;

                    tempMotion.angVelYaw = gyroYaw * GYRO_IN_DEG_SEC_FACTOR;
                    tempMotion.angVelPitch = -gyroPitch * GYRO_IN_DEG_SEC_FACTOR;
                    tempMotion.angVelRoll = gyroRoll * GYRO_IN_DEG_SEC_FACTOR;

                    SixAxisEventArgs args = new SixAxisEventArgs(cState.ReportTimeStamp, cState.Motion);
                    sixAxis.FireSixAxisEvent(args);

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
                            AppLogger.LogToGui(Mac.ToString() + " disconnecting due to idle disconnect", false);

                            if (conType == ConnectionType.BT)
                            {
                                if (DisconnectBT(true))
                                {
                                    timeoutExecuted = true;
                                    return; // all done
                                }
                            }
                        }
                    }

                    Report?.Invoke(this, EventArgs.Empty);
                    WriteReport();

                    forceWrite = false;

                    if (!string.IsNullOrEmpty(currerror))
                        error = currerror;
                    else if (!string.IsNullOrEmpty(error))
                        error = string.Empty;

                    pState.Motion.copy(cState.Motion);
                    cState.CopyTo(pState);

                    if (hasInputEvts)
                    {
                        lock (eventQueueLock)
                        {
                            Action tempAct = null;
                            for (int actInd = 0, actLen = eventQueue.Count; actInd < actLen; actInd++)
                            {
                                tempAct = eventQueue.Dequeue();
                                tempAct.Invoke();
                            }

                            hasInputEvts = false;
                        }
                    }
                }
            }

            timeoutExecuted = true;
        }

        public void SetOperational()
        {
            if (!hDevice.IsFileStreamOpen())
            {
                hDevice.OpenFileStream(InputReportLen);
            }

            if (conType == ConnectionType.USB)
            {
                RunUSBSetup();
                Thread.Sleep(300);
            }

            //Thread.Sleep(1000);

            //EnableFastPollRate();

            //Thread.Sleep(5000);
            //byte[] tmpReport = new byte[INPUT_REPORT_LEN];
            //byte[] command2 = new byte[SUBCOMMAND_BUFFER_LEN];
            //bool result;
            //HidDevice.ReadStatus res;

            // Set device to normal power state
            byte[] powerChoiceArray = new byte[] { 0x00 };
            Subcommand(SwitchProSubCmd.SET_LOW_POWER_STATE, powerChoiceArray, 1, checkResponse: true);

            if (enableHomeLED)
            {
                // Turn on Home light (Solid)
                byte[] light = Enumerable.Repeat((byte)0xFF, 25).ToArray();
                light[0] = 0x1F; light[1] = 0xF0;
                //Thread.Sleep(1000);
                Subcommand(0x38, light, 25, checkResponse: true);
            }

            // Turn on bottom LEDs
            byte[] leds = new byte[] { deviceSlotMask };
            //Thread.Sleep(1000);
            Subcommand(0x30, leds, 1, checkResponse: true);

            // Enable Gyro
            byte[] imuEnable = new byte[] { 0x01 };
            //Thread.Sleep(1000);
            Subcommand(0x40, imuEnable, 1, checkResponse: true);

            // Enable High Performance Gyro mode
            byte[] gyroModeBuffer = new byte[] { 0x03, 0x00, 0x00, 0x01 };
            //Thread.Sleep(1000);
            Subcommand(0x41, gyroModeBuffer, 4, checkResponse: true);

            // Enable Rumble
            byte[] rumbleEnable = new byte[] { 0x01 };
            // Disable Rumble
            //byte[] rumbleEnable = new byte[] { 0x00 };
            //Thread.Sleep(1000);
            Subcommand(0x48, rumbleEnable, 1, checkResponse: true);

            //Thread.Sleep(1000);
            EnableFastPollRate();

            // USB Connections seem to need a delay after switching input modes
            if (conType == ConnectionType.USB)
            {
                Thread.Sleep(1000);
            }

            SetInitRumble();
            //Thread.Sleep(1000);
            CalibrationData();

            Console.WriteLine("FINISHED");

            //if (connectionType == ConnectionType.USB)
            //{
            //    Thread.Sleep(300);
            //    //SetInitRumble();
            //}
        }

        private void RunUSBSetup()
        {
            bool result;
            //byte[] tmpReport = new byte[INPUT_REPORT_LEN];

            byte[] modeSwitchCommand = new byte[] { 0x3F };
            Subcommand(0x03, modeSwitchCommand, 1, checkResponse: false);

            byte[] data = new byte[64];
            data[0] = 0x80; data[1] = 0x01;
            //result = hidDevice.WriteAsyncOutputReportViaInterrupt(data);
            result = hDevice.WriteOutputReportViaInterrupt(data, 0);
            //Array.Clear(tmpReport, 0 , 64);
            //res = hidDevice.ReadWithFileStream(tmpReport);
            //Console.WriteLine("TEST BYTE: {0}", tmpReport[2]);

            data[0] = 0x80; data[1] = 0x02; // USB Pairing
            //result = hidDevice.WriteOutputReportViaControl(data);
            //Thread.Sleep(2000);
            //Thread.Sleep(1000);
            result = hDevice.WriteOutputReportViaControl(data);

            data[0] = 0x80; data[1] = 0x03; // 3Mbit baud rate
            //result = hidDevice.WriteAsyncOutputReportViaInterrupt(data);
            result = hDevice.WriteOutputReportViaControl(data);
            //Thread.Sleep(2000);

            data[0] = 0x80; data[1] = 0x02; // Handshake at new baud rate
            result = hDevice.WriteOutputReportViaControl(data);
            //Thread.Sleep(1000);
            //result = hidDevice.WriteOutputReportViaInterrupt(command, 500);
            //Thread.Sleep(2000);

            data[0] = 0x80; data[1] = 0x4; // Prevent HID timeout
            result = hDevice.WriteOutputReportViaControl(data);
            hDevice.fileStream.Flush();
            //result = hidDevice.WriteOutputReportViaInterrupt(command, 500);
        }

        // Deprecated method. Leave a stub for now
        private void RunBluetoothSetup()
        {
        }

        private void EnableFastPollRate()
        {
            // Enable fatest poll rate
            byte[] tempArray = new byte[] { 0x30 };
            Subcommand(0x03, tempArray, 1, checkResponse: true);
            //Thread.Sleep(1000);
        }

        public void SetInitRumble()
        {
            bool result;
            HidDevice.ReadStatus res;
            //byte[] tmpReport = new byte[64];
            byte[] rumble_data = new byte[8];
            rumble_data[0] = 0x0;
            rumble_data[1] = 0x1;
            rumble_data[2] = 0x40;
            rumble_data[3] = 0x40;

            for (int i = 0; i < 4; i++)
            {
                rumble_data[4 + i] = rumble_data[i];
            }

            byte[] tmpRumble = new byte[RUMBLE_REPORT_LEN];
            Array.Copy(rumble_data, 0, tmpRumble, 2, rumble_data.Length);
            tmpRumble[0] = 0x10;
            tmpRumble[1] = frameCount;
            frameCount = (byte)(++frameCount & 0x0F);

            result = hDevice.WriteOutputReportViaInterrupt(tmpRumble, 0);
            hDevice.fileStream.Flush();
            //res = hidDevice.ReadWithFileStream(tmpReport, 500);
            //res = hidDevice.ReadFile(tmpReport);
        }

        public byte[] Subcommand(byte subcommand, byte[] tmpBuffer, uint bufLen,
            bool checkResponse = false)
        {
            bool result;
            byte[] commandBuffer = new byte[SUBCOMMAND_BUFFER_LEN];
            Array.Copy(commandBuffHeader, 0, commandBuffer, 2, SUBCOMMAND_HEADER_LEN);
            Array.Copy(tmpBuffer, 0, commandBuffer, 11, bufLen);

            commandBuffer[0] = 0x01;
            commandBuffer[1] = frameCount;
            frameCount = (byte)(++frameCount & 0x0F);
            commandBuffer[10] = subcommand;

            result = hDevice.WriteOutputReportViaInterrupt(commandBuffer, 0);
            hDevice.fileStream.Flush();

            byte[] tmpReport = null;
            if (checkResponse)
            {
                tmpReport = new byte[INPUT_REPORT_LEN];
                HidDevice.ReadStatus res;
                res = hDevice.ReadWithFileStream(tmpReport, SUBCOMMAND_RESPONSE_TIMEOUT);
                int tries = 1;
                while (res == HidDevice.ReadStatus.Success &&
                    tmpReport[0] != 0x21 && tmpReport[14] != subcommand && tries < 100)
                {
                    //Console.WriteLine("TRY AGAIN: {0}", tmpReport[0]);
                    res = hDevice.ReadWithFileStream(tmpReport, SUBCOMMAND_RESPONSE_TIMEOUT);
                    tries++;
                }

                //Console.WriteLine("END GAME: {0} {1} {2}", subcommand, tmpReport[0], tries);
            }

            return tmpReport;
        }

        public double currentLeftAmpRatio;
        public double currentRightAmpRatio;

        public void PrepareRumbleData(byte[] buffer)
        {
            //Array.Copy(commandBuffHeader, 0, buffer, 2, SUBCOMMAND_HEADER_LEN);
            buffer[0] = 0x10;
            buffer[1] = frameCount;
            frameCount = (byte)(++frameCount & 0x0F);

            ushort freq_data_high = 0x0001; // 320
            byte freq_data_low = 0x40; // 160
            int idx = (int)(currentLeftAmpRatio * AMP_LIMIT_MAX);
            RumbleTableData entry = compiledRumbleTable[idx];
            byte amp_high = entry.high;
            ushort amp_low = entry.low;
            //byte amp_high = 0x9a; // 609
            //ushort amp_low = 0x8066; // 609
            //buffer[2] = 0x28; // 0
            //buffer[3] = 0xc8; // 1
            //buffer[4] = 0x81; // 2
            //buffer[5] = 0x71; // 3

            //buffer[6] = 0x28; // 4
            //buffer[7] = 0xc8; // 5
            //buffer[8] = 0x81; // 6
            //buffer[9] = 0x71; // 7

            buffer[2] = (byte)((freq_data_high >> 8) & 0xFF); // 0
            buffer[3] = (byte)((freq_data_high & 0xFF) + amp_high); // 1
            buffer[4] = (byte)(freq_data_low + (amp_low >> 8) & 0xFF); // 2
            buffer[5] = (byte)(amp_low & 0xFF); // 3

            idx = (int)(currentRightAmpRatio * AMP_LIMIT_MAX);
            entry = compiledRumbleTable[idx];
            amp_high = entry.high;
            amp_low = entry.low;
            buffer[6] = (byte)((freq_data_high >> 8) & 0xFF); // 4
            buffer[7] = (byte)((freq_data_high & 0xFF) + amp_high); // 5
            buffer[8] = (byte)(freq_data_low + (amp_low >> 8) & 0xFF); // 6
            buffer[9] = (byte)(amp_low & 0xFF); // 7
            //Console.WriteLine("RUMBLE BUFF: {0}", string.Join(", ", buffer));
            //Console.WriteLine("RUMBLE BUFF: {0}",
            //    string.Concat(buffer.Select(i => string.Format("{0:x2} ", i))));
        }

        public void CalibrationData()
        {
            const int SPI_RESP_OFFSET = 20;
            byte[] command;
            byte[] tmpBuffer;

            //command = new byte[] { 0x00, 0x50, 0x00, 0x00, 0x01 };
            //tmpBuffer = Subcommand(SwitchProSubCmd.SPI_FLASH_READ, command, 5, checkResponse: true);
            //Console.WriteLine("THE POWER");
            //Console.WriteLine(string.Join(",", tmpBuffer));
            //Console.WriteLine(tmpBuffer[SPI_RESP_OFFSET]);
            //Console.WriteLine();

            bool foundUserCalib = false;
            command = new byte[] { 0x10, 0x80, 0x00, 0x00, 0x02 };
            tmpBuffer = Subcommand(0x10, command, 5, checkResponse: true);
            if (tmpBuffer[SPI_RESP_OFFSET] == 0xB2 && tmpBuffer[SPI_RESP_OFFSET + 1] == 0xA1)
            {
                foundUserCalib = true;
            }

            if (foundUserCalib)
            {
                command = new byte[] { 0x12, 0x80, 0x00, 0x00, 0x09 };
                tmpBuffer = Subcommand(0x10, command, 5, checkResponse: true);
                //Console.WriteLine("FOUND USER CALIB");
            }
            else
            {
                command = new byte[] { 0x3D, 0x60, 0x00, 0x00, 0x09 };
                tmpBuffer = Subcommand(0x10, command, 5, checkResponse: true);
                //Console.WriteLine("CHECK FACTORY CALIB");
            }

            leftStickCalib[0] = (ushort)(((tmpBuffer[1 + SPI_RESP_OFFSET] << 8) & 0xF00) | tmpBuffer[0 + SPI_RESP_OFFSET]); // X Axis Max above center
            leftStickCalib[1] = (ushort)((tmpBuffer[2 + SPI_RESP_OFFSET] << 4) | (tmpBuffer[1 + SPI_RESP_OFFSET] >> 4)); // Y Axis Max above center
            leftStickCalib[2] = (ushort)(((tmpBuffer[4 + SPI_RESP_OFFSET] << 8) & 0xF00) | tmpBuffer[3 + SPI_RESP_OFFSET]); // X Axis Center
            leftStickCalib[3] = (ushort)((tmpBuffer[5 + SPI_RESP_OFFSET] << 4) | (tmpBuffer[4 + SPI_RESP_OFFSET] >> 4)); // Y Axis Center
            leftStickCalib[4] = (ushort)(((tmpBuffer[7 + SPI_RESP_OFFSET] << 8) & 0xF00) | tmpBuffer[6 + SPI_RESP_OFFSET]); // X Axis Min below center
            leftStickCalib[5] = (ushort)((tmpBuffer[8 + SPI_RESP_OFFSET] << 4) | (tmpBuffer[7 + SPI_RESP_OFFSET] >> 4)); // Y Axis Min below center

            if (foundUserCalib)
            {
                leftStickXData.max = (ushort)(leftStickCalib[0] + leftStickCalib[2]);
                leftStickXData.mid = leftStickCalib[2];
                leftStickXData.min = (ushort)(leftStickCalib[2] - leftStickCalib[4]);

                leftStickYData.max = (ushort)(leftStickCalib[1] + leftStickCalib[3]);
                leftStickYData.mid = leftStickCalib[3];
                leftStickYData.min = (ushort)(leftStickCalib[3] - leftStickCalib[5]);
            }
            else
            {
                leftStickXData.max = (ushort)((leftStickCalib[0] + leftStickCalib[2]) * STICK_AXIS_MAX_CUTOFF);
                leftStickXData.mid = leftStickCalib[2];
                leftStickXData.min = (ushort)((leftStickCalib[2] - leftStickCalib[4]) * STICK_AXIS_MIN_CUTOFF);

                leftStickYData.max = (ushort)((leftStickCalib[1] + leftStickCalib[3]) * STICK_AXIS_MAX_CUTOFF);
                leftStickYData.mid = leftStickCalib[3];
                leftStickYData.min = (ushort)((leftStickCalib[3] - leftStickCalib[5]) * STICK_AXIS_MIN_CUTOFF);
                //leftStickOffsetX = leftStickOffsetY = 140;
            }

            //leftStickOffsetX = leftStickCalib[2];
            //leftStickOffsetY = leftStickCalib[3];

            //Console.WriteLine(string.Join(",", tmpBuffer));
            //Console.WriteLine();
            //Console.WriteLine(string.Join(",", leftStickCalib));

            foundUserCalib = false;
            command = new byte[] { 0x1B, 0x80, 0x00, 0x00, 0x02 };
            tmpBuffer = Subcommand(0x10, command, 5, checkResponse: true);
            if (tmpBuffer[SPI_RESP_OFFSET] == 0xB2 && tmpBuffer[SPI_RESP_OFFSET + 1] == 0xA1)
            {
                foundUserCalib = true;
            }

            if (foundUserCalib)
            {
                command = new byte[] { 0x1D, 0x80, 0x00, 0x00, 0x09 };
                tmpBuffer = Subcommand(0x10, command, 5, checkResponse: true);
                //Console.WriteLine("FOUND RIGHT USER CALIB");
            }
            else
            {
                command = new byte[] { 0x46, 0x60, 0x00, 0x00, 0x09 };
                tmpBuffer = Subcommand(0x10, command, 5, checkResponse: true);
                //Console.WriteLine("CHECK RIGHT FACTORY CALIB");
            }

            rightStickCalib[2] = (ushort)(((tmpBuffer[1 + SPI_RESP_OFFSET] << 8) & 0xF00) | tmpBuffer[0 + SPI_RESP_OFFSET]); // X Axis Center
            rightStickCalib[3] = (ushort)((tmpBuffer[2 + SPI_RESP_OFFSET] << 4) | (tmpBuffer[1 + SPI_RESP_OFFSET] >> 4)); // Y Axis Center
            rightStickCalib[4] = (ushort)(((tmpBuffer[4 + SPI_RESP_OFFSET] << 8) & 0xF00) | tmpBuffer[3 + SPI_RESP_OFFSET]); // X Axis Min below center
            rightStickCalib[5] = (ushort)((tmpBuffer[5 + SPI_RESP_OFFSET] << 4) | (tmpBuffer[4 + SPI_RESP_OFFSET] >> 4)); // Y Axis Min below center
            rightStickCalib[0] = (ushort)(((tmpBuffer[7 + SPI_RESP_OFFSET] << 8) & 0xF00) | tmpBuffer[6 + SPI_RESP_OFFSET]); // X Axis Max above center
            rightStickCalib[1] = (ushort)((tmpBuffer[8 + SPI_RESP_OFFSET] << 4) | (tmpBuffer[7 + SPI_RESP_OFFSET] >> 4)); // Y Axis Max above center

            if (foundUserCalib)
            {
                rightStickXData.max = (ushort)(rightStickCalib[2] + rightStickCalib[4]);
                rightStickXData.mid = rightStickCalib[2];
                rightStickXData.min = (ushort)(rightStickCalib[2] - rightStickCalib[0]);

                rightStickYData.max = (ushort)(rightStickCalib[3] + rightStickCalib[5]);
                rightStickYData.mid = rightStickCalib[3];
                rightStickYData.min = (ushort)(rightStickCalib[3] - rightStickCalib[1]);
            }
            else
            {
                rightStickXData.max = (ushort)((rightStickCalib[2] + rightStickCalib[0]) * STICK_AXIS_MAX_CUTOFF);
                rightStickXData.mid = rightStickCalib[2];
                rightStickXData.min = (ushort)((rightStickCalib[2] - rightStickCalib[4]) * STICK_AXIS_MIN_CUTOFF);

                rightStickYData.max = (ushort)((rightStickCalib[3] + rightStickCalib[1]) * STICK_AXIS_MAX_CUTOFF);
                rightStickYData.mid = rightStickCalib[3];
                rightStickYData.min = (ushort)((rightStickCalib[3] - rightStickCalib[5]) * STICK_AXIS_MIN_CUTOFF);
                //rightStickOffsetX = rightStickOffsetY = 140;
            }

            //rightStickOffsetX = rightStickCalib[2];
            //rightStickOffsetY = rightStickCalib[5];

            //Console.WriteLine(string.Join(",", tmpBuffer));
            //Console.WriteLine();
            //Console.WriteLine(string.Join(",", rightStickCalib));

            /*
            // Grab Factory LS Dead Zone
            command = new byte[] { 0x86, 0x60, 0x00, 0x00, 0x10 };
            byte[] deadZoneBuffer = Subcommand(0x10, command, 5, checkResponse: true);
            deadzoneLS = (ushort)((deadZoneBuffer[4 + SPI_RESP_OFFSET] << 8) & 0xF00 | deadZoneBuffer[3 + SPI_RESP_OFFSET]);
            //Console.WriteLine("DZ Left: {0}", deadzoneLS);
            //Console.WriteLine(string.Join(",", deadZoneBuffer));

            // Grab Factory RS Dead Zone
            command = new byte[] { 0x98, 0x60, 0x00, 0x00, 0x10 };
            deadZoneBuffer = Subcommand(0x10, command, 5, checkResponse: true);
            deadzoneRS = (ushort)((deadZoneBuffer[4 + SPI_RESP_OFFSET] << 8) & 0xF00 | deadZoneBuffer[3 + SPI_RESP_OFFSET]);
            //Console.WriteLine("DZ Right: {0}", deadzoneRS);
            //Console.WriteLine(string.Join(",", deadZoneBuffer));*/

            foundUserCalib = false;
            command = new byte[] { 0x26, 0x80, 0x00, 0x00, 0x02 };
            tmpBuffer = Subcommand(0x10, command, 5, checkResponse: true);
            if (tmpBuffer[SPI_RESP_OFFSET] == 0xB2 && tmpBuffer[SPI_RESP_OFFSET + 1] == 0xA1)
            {
                foundUserCalib = true;
            }

            //Console.WriteLine("{0}", string.Join(",", tmpBuffer.Skip(offset).ToArray()));
            if (foundUserCalib)
            {
                command = new byte[] { 0x28, 0x80, 0x00, 0x00, 0x18 };
                tmpBuffer = Subcommand(0x10, command, 5, checkResponse: true);
                //Console.WriteLine("FOUND USER CALIB");
            }
            else
            {
                command = new byte[] { 0x20, 0x60, 0x00, 0x00, 0x18 };
                tmpBuffer = Subcommand(0x10, command, 5, checkResponse: true);
                //Console.WriteLine("CHECK FACTORY CALIB");
            }

            accelNeutral[IMU_XAXIS_IDX] = (short)((tmpBuffer[3 + SPI_RESP_OFFSET] << 8) & 0xFF00 | tmpBuffer[2 + SPI_RESP_OFFSET]); // Accel X Offset
            accelNeutral[IMU_YAXIS_IDX] = (short)((tmpBuffer[1 + SPI_RESP_OFFSET] << 8) & 0xFF00 | tmpBuffer[0 + SPI_RESP_OFFSET]); // Accel Y Offset
            accelNeutral[IMU_ZAXIS_IDX] = (short)((tmpBuffer[5 + SPI_RESP_OFFSET] << 8) & 0xFF00 | tmpBuffer[4 + SPI_RESP_OFFSET]); // Accel Z Offset
            //Console.WriteLine("ACCEL NEUTRAL: {0}", string.Join(",", accelNeutral));
            //Console.WriteLine("{0}", string.Join(",", tmpBuffer.Skip(offset).ToArray()));

            accelSens[IMU_XAXIS_IDX] = (short)((tmpBuffer[9 + SPI_RESP_OFFSET] << 8) & 0xFF00 | tmpBuffer[8 + SPI_RESP_OFFSET]); // Accel X Sens
            accelSens[IMU_YAXIS_IDX] = (short)((tmpBuffer[7 + SPI_RESP_OFFSET] << 8) & 0xFF00 | tmpBuffer[6 + SPI_RESP_OFFSET]); // Accel Y Sens
            accelSens[IMU_ZAXIS_IDX] = (short)((tmpBuffer[11 + SPI_RESP_OFFSET] << 8) & 0xFF00 | tmpBuffer[10 + SPI_RESP_OFFSET]); // Accel Z Sens
            //Console.WriteLine("ACCEL SENS: {0}", string.Join(",", accelSens));
            //Console.WriteLine("{0}", string.Join(",", tmpBuffer.Skip(SPI_RESP_OFFSET).ToArray()));

            accelCoeff[IMU_XAXIS_IDX] = 1.0 / (accelSens[IMU_XAXIS_IDX] - accelNeutral[IMU_XAXIS_IDX]) * 4.0;
            accelCoeff[IMU_YAXIS_IDX] = 1.0 / (accelSens[IMU_YAXIS_IDX] - accelNeutral[IMU_YAXIS_IDX]) * 4.0;
            accelCoeff[IMU_ZAXIS_IDX] = 1.0 / (accelSens[IMU_ZAXIS_IDX] - accelNeutral[IMU_ZAXIS_IDX]) * 4.0;
            //accelCoeff[IMU_XAXIS_IDX] = (accelSens[IMU_XAXIS_IDX] - accelNeutral[IMU_XAXIS_IDX]) / 65535.0 / 1000.0;
            //accelCoeff[IMU_YAXIS_IDX] = (accelSens[IMU_YAXIS_IDX] - accelNeutral[IMU_YAXIS_IDX]) / 65535.0 / 1000.0;
            //accelCoeff[IMU_ZAXIS_IDX] = (accelSens[IMU_ZAXIS_IDX] - accelNeutral[IMU_ZAXIS_IDX]) / 65535.0 / 1000.0;
            //Console.WriteLine("ACCEL COEFF: {0}", string.Join(",", accelCoeff));

            //accelSensMulti[IMU_XAXIS_IDX] = accelSens[IMU_XAXIS_IDX] / (2 * 8192.0);
            //accelSensMulti[IMU_YAXIS_IDX] = accelSens[IMU_YAXIS_IDX] / (2 * 8192.0);
            //accelSensMulti[IMU_ZAXIS_IDX] = accelSens[IMU_ZAXIS_IDX] / (2 * 8192.0);

            gyroBias[0] = (short)((tmpBuffer[17 + SPI_RESP_OFFSET] << 8) & 0xFF00 | tmpBuffer[16 + SPI_RESP_OFFSET]); // Gyro Yaw Offset
            gyroBias[1] = (short)((tmpBuffer[15 + SPI_RESP_OFFSET] << 8) & 0xFF00 | tmpBuffer[14 + SPI_RESP_OFFSET]); // Gyro Pitch Offset
            gyroBias[2] = (short)((tmpBuffer[13 + SPI_RESP_OFFSET] << 8) & 0xFF00 | tmpBuffer[12 + SPI_RESP_OFFSET]); // Gyro Roll Offset

            gyroSens[IMU_YAW_IDX] = (short)((tmpBuffer[23 + SPI_RESP_OFFSET] << 8) & 0xFF00 | tmpBuffer[22 + SPI_RESP_OFFSET]); // Gyro Yaw Sens
            gyroSens[IMU_PITCH_IDX] = (short)((tmpBuffer[21 + SPI_RESP_OFFSET] << 8) & 0xFF00 | tmpBuffer[20 + SPI_RESP_OFFSET]); // Gyro Pitch Sens
            gyroSens[IMU_ROLL_IDX] = (short)((tmpBuffer[19 + SPI_RESP_OFFSET] << 8) & 0xFF00 | tmpBuffer[18 + SPI_RESP_OFFSET]); // Gyro Roll Sens

            //Console.WriteLine("GYRO BIAS: {0}", string.Join(",", gyroBias));
            //Console.WriteLine("GYRO SENS: {0}", string.Join(",", gyroSens));
            //Console.WriteLine("{0}", string.Join(",", tmpBuffer.Skip(SPI_RESP_OFFSET).ToArray()));

            gyroCoeff[IMU_YAW_IDX] = 936.0 / (gyroSens[IMU_YAW_IDX] - gyroBias[IMU_YAW_IDX]);
            gyroCoeff[IMU_PITCH_IDX] = 936.0 / (gyroSens[IMU_PITCH_IDX] - gyroBias[IMU_PITCH_IDX]);
            gyroCoeff[IMU_ROLL_IDX] = 936.0 / (gyroSens[IMU_ROLL_IDX] - gyroBias[IMU_ROLL_IDX]);
            //gyroCoeff[IMU_YAW_IDX] = (gyroSens[IMU_YAW_IDX] - gyroBias[IMU_YAW_IDX]) / 65535.0;
            //gyroCoeff[IMU_PITCH_IDX] = (gyroSens[IMU_PITCH_IDX] - gyroBias[IMU_PITCH_IDX]) / 65535.0;
            //gyroCoeff[IMU_ROLL_IDX] = (gyroSens[IMU_ROLL_IDX] - gyroBias[IMU_ROLL_IDX]) / 65535.0;
            //Console.WriteLine("GYRO COEFF: {0}", string.Join(",", gyroCoeff));
        }

        public override bool DisconnectWireless(bool callRemoval = false)
        {
            bool result = false;
            result = DisconnectBT(callRemoval);
            //StopOutputUpdate();
            //Detach();
            return result;
        }

        public override bool DisconnectBT(bool callRemoval = false)
        {
            StopOutputUpdate();
            Detach();

            int IOCTL_BTH_DISCONNECT_DEVICE = 0x41000c;

            byte[] btAddr = new byte[8];
            string[] sbytes = Mac.Split(':');
            for (int i = 0; i < 6; i++)
            {
                // parse hex byte in reverse order
                btAddr[5 - i] = Convert.ToByte(sbytes[i], 16);
            }

            long lbtAddr = BitConverter.ToInt64(btAddr, 0);

            IntPtr btHandle = IntPtr.Zero;
            bool success = false;
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
            success = true;

            if (callRemoval)
            {
                isDisconnecting = true;
                Removal?.Invoke(this, EventArgs.Empty);
            }

            return success;
        }

        public override bool DisconnectDongle(bool remove = false)
        {
            StopOutputUpdate();
            Detach();

            if (remove)
            {
                isDisconnecting = true;
                Removal?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                isRemoved = true;
            }

            return true;
        }

        public void Detach()
        {
            bool result;

            // Disable Gyro
            byte[] tmpOffBuffer = new byte[] { 0x0 };
            Subcommand(0x40, tmpOffBuffer, 1, checkResponse: true);

            // Possibly disable rumble? Leave commented
            tmpOffBuffer = new byte[] { 0x0 };
            Subcommand(0x48, tmpOffBuffer, 1, checkResponse: true);

            // Revert back to low power state
            byte[] powerChoiceArray = new byte[] { 0x01 };
            Subcommand(SwitchProSubCmd.SET_LOW_POWER_STATE, powerChoiceArray, 1, checkResponse: true);

            if (conType == ConnectionType.USB)
            {
                byte[] data = new byte[64];
                data[0] = 0x80; data[1] = 0x05;
                result = hDevice.WriteOutputReportViaControl(data);

                data[0] = 0x80; data[1] = 0x06;
                result = hDevice.WriteOutputReportViaControl(data);
            }
        }

        public void WriteReport()
        {
            MergeStates();

            bool dirty;
            double tempRatio = currentHap.rumbleState.RumbleMotorStrengthLeftHeavySlow / 255.0;
            dirty = tempRatio != 0 || tempRatio != currentLeftAmpRatio;
            currentLeftAmpRatio = tempRatio;

            tempRatio = currentHap.rumbleState.RumbleMotorStrengthRightLightFast / 255.0;
            dirty = dirty || tempRatio != 0 || tempRatio != currentRightAmpRatio;
            currentRightAmpRatio = tempRatio;

            if (dirty)
            {
                PrepareRumbleData(rumbleReportBuffer);
                bool result = hDevice.WriteOutputReportViaInterrupt(rumbleReportBuffer, 100);
            }
        }

        public override bool IsAlive()
        {
            return !isDisconnecting;
        }

        private void CalculateDeviceSlotMask()
        {
            // Map 1-15 as a set of 4 LED lights
            switch (deviceSlotNumber)
            {
                case 0:
                case 1:
                case 2:
                case 3:
                    deviceSlotMask = (byte)(1 << deviceSlotNumber);
                    break;
                case 4:
                    deviceSlotMask = 0x01 | 0x02;
                    break;
                case 5:
                    deviceSlotMask = 0x01 | 0x04;
                    break;
                case 6:
                    deviceSlotMask = 0x01 | 0x08;
                    break;

                case 7:
                    deviceSlotMask = 0x02 | 0x04;
                    break;
                case 8:
                    deviceSlotMask = 0x02 | 0x08;
                    break;

                case 9:
                    deviceSlotMask = 0x04 | 0x08;
                    break;

                case 10:
                    deviceSlotMask = 0x01 | 0x02 | 0x04;
                    break;
                case 11:
                    deviceSlotMask = 0x01 | 0x02 | 0x08;
                    break;
                case 12:
                    deviceSlotMask = 0x01 | 0x04 | 0x08;
                    break;

                case 13:
                    deviceSlotMask = 0x02 | 0x04 | 0x08;
                    break;

                case 14:
                    deviceSlotMask = 0x01 | 0x02 | 0x04 | 0x08;
                    break;
                default:
                    deviceSlotMask = 0x00;
                    break;
            }
        }

        private void SetupOptionsEvents()
        {
            if (nativeOptionsStore != null)
            {
            }
        }

        public override void LoadStoreSettings()
        {
            if (nativeOptionsStore != null)
            {
                enableHomeLED = nativeOptionsStore.EnableHomeLED;
            }
        }
    }
}
