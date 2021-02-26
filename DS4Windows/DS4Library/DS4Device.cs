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
    public struct DS4Color : IEquatable<DS4Color>
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

        public bool Equals(DS4Color other)
        {
            return this.red == other.red && this.green == other.green && this.blue == other.blue;
        }

        public Color ToColor => Color.FromArgb(red, green, blue);
        public Color ToColorA
        {
            get
            {
                byte alphacolor = Math.Max(red, Math.Max(green, blue));
                Color reg = Color.FromArgb(red, green, blue);
                Color full = HuetoRGB(reg.GetHue(), reg.GetBrightness(), ref reg);
                return Color.FromArgb((alphacolor > 205 ? 255 : (alphacolor + 50)), full);
            }
        }

        private Color HuetoRGB(float hue, float light, ref Color rgb)
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
    public struct DS4ForceFeedbackState : IEquatable<DS4ForceFeedbackState>
    {
        public byte RumbleMotorStrengthLeftHeavySlow, RumbleMotorStrengthRightLightFast;
        public bool RumbleMotorsExplicitlyOff;

        public bool Equals(DS4ForceFeedbackState other)
        {
            return RumbleMotorStrengthLeftHeavySlow == other.RumbleMotorStrengthLeftHeavySlow &&
                RumbleMotorStrengthRightLightFast == other.RumbleMotorStrengthRightLightFast &&
                RumbleMotorsExplicitlyOff == other.RumbleMotorsExplicitlyOff;
        }

        public bool IsRumbleSet()
        {
            const byte zero = 0;
            return RumbleMotorsExplicitlyOff || RumbleMotorStrengthLeftHeavySlow != zero || RumbleMotorStrengthRightLightFast != zero;
        }
    }

    public struct DS4LightbarState : IEquatable<DS4LightbarState>
    {
        public DS4Color LightBarColor;
        public bool LightBarExplicitlyOff;
        public byte LightBarFlashDurationOn, LightBarFlashDurationOff;

        public bool Equals(DS4LightbarState other)
        {
            return LightBarColor.Equals(other.LightBarColor) &&
                LightBarExplicitlyOff == other.LightBarExplicitlyOff &&
                LightBarFlashDurationOn == other.LightBarFlashDurationOn &&
                LightBarFlashDurationOff == other.LightBarFlashDurationOff;
        }

        public bool IsLightBarSet()
        {
            return LightBarExplicitlyOff || LightBarColor.red != 0 || LightBarColor.green != 0 || LightBarColor.blue != 0;
        }
    }

    public struct DS4HapticState : IEquatable<DS4HapticState>
    {
        public DS4LightbarState lightbarState;
        public DS4ForceFeedbackState rumbleState;

        public bool Equals(DS4HapticState other)
        {
            return lightbarState.Equals(other.lightbarState) &&
                rumbleState.Equals(other.rumbleState);
        }

        public bool IsLightBarSet()
        {
            return lightbarState.IsLightBarSet();
        }

        public bool IsRumbleSet()
        {
            const byte zero = 0;
            return rumbleState.RumbleMotorsExplicitlyOff || rumbleState.RumbleMotorStrengthLeftHeavySlow != zero || rumbleState.RumbleMotorStrengthRightLightFast != zero;
        }
    }

    public class DS4Device
    {
        public class GyroMouseSens
        {
            public double mouseOffset = 0.2;
            public double mouseCoefficient = 0.012;
            public double mouseSmoothOffset = 0.2;
        }

        public enum ExclusiveStatus : byte
        {
            Shared = 0,
            Exclusive = 1,
            HidGuardAffected = 2,
        }

        //internal const int BT_OUTPUT_REPORT_LENGTH = 78;
        protected const int BT_OUTPUT_REPORT_LENGTH = 334;
        private const int BT_OUTPUT_REPORT_0x15_LENGTH = BT_OUTPUT_REPORT_LENGTH;
        private const int BT_OUTPUT_REPORT_0x11_LENGTH = 78;
        internal const int BT_INPUT_REPORT_LENGTH = 547;
        internal const int BT_OUTPUT_CHANGE_LENGTH = 13;
        internal const int USB_OUTPUT_CHANGE_LENGTH = 11;
        // Use large value for worst case scenario
        internal const int READ_STREAM_TIMEOUT = 3000;
        // Isolated BT report can have latency as high as 15 ms
        // due to hardware.
        internal const int WARN_INTERVAL_BT = 40;
        internal const int WARN_INTERVAL_USB = 20;
        // Maximum values for battery level when no USB cable is connected
        // and when a USB cable is connected
        internal const int BATTERY_MAX = 8;
        internal const int BATTERY_MAX_USB = 11;
        public const string BLANK_SERIAL = "00:00:00:00:00:00";
        public const byte SERIAL_FEATURE_ID = 18;
        private const string SONYWA_AUDIO_SEARCHNAME = "DUALSHOCK®4 USB Wireless Adaptor";
        private const string RAIJU_TE_AUDIO_SEARCHNAME = "Razer Raiju Tournament Edition Wired";
        protected HidDevice hDevice;
        protected string Mac;
        protected DS4State cState = new DS4State();
        protected DS4State pState = new DS4State();
        protected ConnectionType conType;
        protected byte[] accel = new byte[6];
        protected byte[] gyro = new byte[6];
        protected byte[] inputReport;
        protected byte[] btInputReport = null;
        protected byte[] outReportBuffer, outputReport;
        protected int inputReportErrorCount = 0; // Num of consequtive input report errors (fex if BT device fails 5 times in crc32 and 0x11 data type check then switch over to handle incoming BT packets as those were usb PC-friendly packets. Some fake DS4 gamepads needs this)
        protected readonly DS4Touchpad touchpad = null;
        protected readonly DS4SixAxis sixAxis = null;
        protected Thread ds4Input, ds4Output;
        protected int battery;
        protected DS4Audio audio = null;
        protected DS4Audio micAudio = null;
        public DateTime lastActive = DateTime.UtcNow;
        public DateTime firstActive = DateTime.UtcNow;
        protected bool charging;
        protected bool readyQuickChargeDisconnect;
        protected int warnInterval = WARN_INTERVAL_USB;
        public int getWarnInterval()
        {
            return warnInterval;
        }

        public bool ReadyQuickChargeDisconnect
        {
            get => readyQuickChargeDisconnect;
            set => readyQuickChargeDisconnect = value;
        }

        public ControllerOptionsStore optionsStore;
        private DS4ControllerOptions nativeOptionsStore;

        public Int32 wheelPrevPhysicalAngle = 0;
        public Int32 wheelPrevFullAngle = 0;
        public Int32 wheelFullTurnCount = 0;

        public Point wheelCenterPoint;
        public Point wheel90DegPointLeft;
        public Point wheelCircleCenterPointLeft;
        public Point wheel90DegPointRight;
        public Point wheelCircleCenterPointRight;

        public DateTime wheelPrevRecalibrateTime;

        protected int wheelRecalibrateActiveState = 0;
        public int WheelRecalibrateActiveState
        {
            get { return wheelRecalibrateActiveState; }
            set
            {
                wheelRecalibrateActiveState = value;
            }
        }

        public enum WheelCalibrationPoint
        {
            None = 0,
            Center = 1,
            Right90 = 2,
            Left90 = 4,
            All = Center | Right90 | Left90
        }
        public WheelCalibrationPoint wheelCalibratedAxisBitmask;

        protected bool exitOutputThread = false;
        public bool ExitOutputThread => exitOutputThread;
        protected bool exitInputThread = false;
        protected object exitLocker = new object();
        protected ExclusiveStatus exclusiveStatus = ExclusiveStatus.Shared;

        public delegate void ReportHandler<TEventArgs>(DS4Device sender, TEventArgs args);

        //public event EventHandler<EventArgs> Report = null;
        public virtual event ReportHandler<EventArgs> Report = null;
        public virtual event EventHandler<EventArgs> Removal = null;
        public event EventHandler<EventArgs> SyncChange = null;
        public event EventHandler<EventArgs> SerialChange = null;
        //public EventHandler<EventArgs> MotionEvent = null;
        public ReportHandler<EventArgs> MotionEvent = null;

        public HidDevice HidDevice => hDevice;
        public bool IsHidExclusive => HidDevice.IsExclusive;
        public bool isHidExclusive()
        {
            return HidDevice.IsExclusive;
        }

        public bool IsExclusive
        {
            get { return exclusiveStatus > ExclusiveStatus.Shared; }
        }

        public bool isExclusive()
        {
            return exclusiveStatus > ExclusiveStatus.Shared;
        }

        public ExclusiveStatus CurrentExclusiveStatus
        {
            get => exclusiveStatus;
            set
            {
                exclusiveStatus = value;
            }
        }

        protected bool isDisconnecting = false;
        public bool IsDisconnecting
        {
            get { return isDisconnecting; }
            protected set
            {
                this.isDisconnecting = value;
            }
        }

        public bool isDisconnectingStatus()
        {
            return this.isDisconnecting;
        }

        protected bool isRemoving = false;
        public bool IsRemoving
        {
            get { return isRemoving; }
            set
            {
                this.isRemoving = value;
            }
        }

        protected bool isRemoved = false;
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
        public event EventHandler MacAddressChanged;
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
        protected int idleTimeout = 0;
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

        // Feature set of gamepad (some non-official DS4 gamepads require a bit different logic than a genuine Sony DS4). 0=Default DS4 gamepad feature set.
        protected VidPidFeatureSet featureSet;
        public VidPidFeatureSet FeatureSet
        {
            get { return featureSet;  }
            set { featureSet = value; }
        }
        public VidPidFeatureSet ModifyFeatureSetFlag(VidPidFeatureSet featureBitFlag, bool flagSet)
        {
            if (flagSet) featureSet |= featureBitFlag;
            else featureSet &= ~featureBitFlag;
            return featureSet;
        }

        private const byte DEFAULT_BT_REPORT_TYPE = 0x15;
        private byte knownGoodBTOutputReportType = DEFAULT_BT_REPORT_TYPE;

        private const byte DEFAULT_OUTPUT_FEATURES = 0xF7;
        private const byte COPYCAT_OUTPUT_FEATURES = 0xF3; // Remove flash flag
        private byte outputFeaturesByte = DEFAULT_OUTPUT_FEATURES;

        protected bool useRumble = true;
        public bool UseRumble { get => useRumble; set => useRumble = value; }

        public int Battery => battery;
        public delegate void BatteryUpdateHandler(object sender, EventArgs e);
        public virtual event EventHandler BatteryChanged;
        public int getBattery()
        {
            return battery;
        }

        public bool Charging => charging;
        public virtual event EventHandler ChargingChanged;
        public bool isCharging()
        {
            return charging;
        }

        protected long lastTimeElapsed = 0;
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
            get { return currentHap.rumbleState.RumbleMotorStrengthRightLightFast; }
            set
            {
                if (currentHap.rumbleState.RumbleMotorStrengthRightLightFast != value)
                    currentHap.rumbleState.RumbleMotorStrengthRightLightFast = value;
            }
        }

        public byte LeftHeavySlowRumble
        {
            get { return currentHap.rumbleState.RumbleMotorStrengthLeftHeavySlow; }
            set
            {
                if (currentHap.rumbleState.RumbleMotorStrengthLeftHeavySlow != value)
                    currentHap.rumbleState.RumbleMotorStrengthLeftHeavySlow = value;
            }
        }

        public byte getLeftHeavySlowRumble()
        {
            return currentHap.rumbleState.RumbleMotorStrengthLeftHeavySlow;
        }


        private int rumbleAutostopTime = 0;
        public int RumbleAutostopTime
        {
            get { return rumbleAutostopTime; }
            set
            {
                // Value in milliseconds
                rumbleAutostopTime = value;

                // If autostop timer is disabled (value 0) then stop existing autostop timer otherwise restart it
                if (value <= 0)
                    rumbleAutostopTimer.Reset();
                else
                    rumbleAutostopTimer.Restart();
            }
        }

        public DS4Color LightBarColor
        {
            get { return currentHap.lightbarState.LightBarColor; }
            set
            {
                if (currentHap.lightbarState.LightBarColor.red != value.red || currentHap.lightbarState.LightBarColor.green != value.green || currentHap.lightbarState.LightBarColor.blue != value.blue)
                {
                    currentHap.lightbarState.LightBarColor = value;
                }
            }
        }

        public byte getLightBarOnDuration()
        {
            return currentHap.lightbarState.LightBarFlashDurationOn;
        }

        // Specify the poll rate interval used for the DS4 hardware when
        // connected via Bluetooth
        protected int btPollRate = 0;
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

        protected Queue<Action> eventQueue = new Queue<Action>();
        protected object eventQueueLock = new object();

        protected Thread timeoutCheckThread = null;
        protected bool timeoutExecuted = false;
        protected bool timeoutEvent = false;
        protected bool runCalib;
        protected bool hasInputEvts = false;
        protected string displayName;
        public string DisplayName => displayName;
        public bool ShouldRunCalib()
        {
            return runCalib;
        }

        protected ManualResetEventSlim readWaitEv = new ManualResetEventSlim();
        public ManualResetEventSlim ReadWaitEv { get => readWaitEv; }

        public virtual byte SerialReportID { get => SERIAL_FEATURE_ID; }

        public enum BTOutputReportMethod : uint
        {
            WriteFile,
            HidD_SetOutputReport,
        }

        private BTOutputReportMethod btOutputMethod;
        public BTOutputReportMethod BTOutputMethod { get => btOutputMethod; set => btOutputMethod = value; }

        protected InputDevices.InputDeviceType deviceType;
        public InputDevices.InputDeviceType DeviceType { get => deviceType; }

        protected GyroMouseSens gyroMouseSensSettings;
        public virtual GyroMouseSens GyroMouseSensSettings { get => gyroMouseSensSettings; }

        protected int deviceSlotNumber = -1;
        public int DeviceSlotNumber
        {
            get => deviceSlotNumber;
            set
            {
                if (deviceSlotNumber == value) return;
                deviceSlotNumber = value;
                DeviceSlotNumberChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        protected event EventHandler DeviceSlotNumberChanged;
        protected byte deviceSlotMask = 0x00;

        public DS4Device(HidDevice hidDevice, string disName, VidPidFeatureSet featureSet = VidPidFeatureSet.DefaultDS4)
        {
            hDevice = hidDevice;
            displayName = disName;
            this.featureSet = featureSet;

            conType = HidConnectionType(hDevice);
            exclusiveStatus = ExclusiveStatus.Shared;
            if (hidDevice.IsExclusive)
            {
                exclusiveStatus = ExclusiveStatus.Exclusive;
            }

            if (this.FeatureSet != VidPidFeatureSet.DefaultDS4)
                AppLogger.LogToGui($"The gamepad {displayName} ({conType}) uses custom feature set ({this.FeatureSet.ToString("F")})", false);

            Mac = hDevice.ReadSerial(SerialReportID);
            runCalib = (this.featureSet & VidPidFeatureSet.NoGyroCalib) == 0;

            touchpad = new DS4Touchpad();
            sixAxis = new DS4SixAxis();
        }

        public virtual void PostInit()
        {
            HidDevice hidDevice = hDevice;
            deviceType = InputDevices.InputDeviceType.DS4;
            gyroMouseSensSettings = new GyroMouseSens();
            optionsStore = nativeOptionsStore = new DS4ControllerOptions(deviceType);
            SetupOptionsEvents();

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
                        audio = new DS4Audio(searchDeviceInstance: hidDevice.ParentPath);
                        micAudio = new DS4Audio(DS4Library.CoreAudio.DataFlow.Capture,
                            searchDeviceInstance: hidDevice.ParentPath);
                    }
                    else if (tempAttr.VendorId == DS4Devices.RAZER_VID &&
                        tempAttr.ProductId == 0x1007)
                    {
                        audio = new DS4Audio(searchDeviceInstance: hidDevice.ParentPath);
                        micAudio = new DS4Audio(DS4Library.CoreAudio.DataFlow.Capture,
                            searchDeviceInstance: hidDevice.ParentPath);
                    }
                    else if (featureSet.HasFlag(VidPidFeatureSet.MonitorAudio))
                    {
                        audio = new DS4Audio(searchDeviceInstance: hidDevice.ParentPath);
                        micAudio = new DS4Audio(DS4Library.CoreAudio.DataFlow.Capture,
                            searchDeviceInstance: hidDevice.ParentPath);
                    }

                    synced = true;
                }
                else
                {
                    warnInterval = WARN_INTERVAL_BT;
                    audio = new DS4Audio(searchDeviceInstance: hidDevice.ParentPath);
                    micAudio = new DS4Audio(DS4Library.CoreAudio.DataFlow.Capture,
                        searchDeviceInstance: hidDevice.ParentPath);
                    runCalib = synced = isValidSerial();
                }
            }
            else
            {
                btInputReport = new byte[BT_INPUT_REPORT_LENGTH];
                inputReport = new byte[BT_INPUT_REPORT_LENGTH - 2];
                // If OnlyOutputData0x05 feature is not set then use the default DS4 output buffer size. However, some Razer gamepads use 32 bytes output buffer and output data type 0x05 in BT mode (writeData fails if the code tries to write too many unnecessary bytes)
                if ((this.featureSet & VidPidFeatureSet.OnlyOutputData0x05) == 0)
                {
                    // Default DS4 logic while writing data to gamepad
                    outputReport = new byte[BT_OUTPUT_REPORT_LENGTH];
                    outReportBuffer = new byte[BT_OUTPUT_REPORT_LENGTH];
                }
                else
                {
                    // Use the gamepad specific output buffer size (but minimum of 15 bytes to avoid out-of-index errors in this app)
                    outputReport = new byte[hDevice.Capabilities.OutputReportByteLength <= 15 ? 15 : hDevice.Capabilities.OutputReportByteLength];
                    outReportBuffer = new byte[hDevice.Capabilities.OutputReportByteLength <= 15 ? 15 : hDevice.Capabilities.OutputReportByteLength];
                }
                warnInterval = WARN_INTERVAL_BT;
                synced = isValidSerial();
            }

            if (runCalib)
                RefreshCalibration();

            if (!hDevice.IsFileStreamOpen())
            {
                hDevice.OpenFileStream(outputReport.Length);
            }

            if (conType == ConnectionType.BT &&
                !featureSet.HasFlag(VidPidFeatureSet.NoOutputData) && !featureSet.HasFlag(VidPidFeatureSet.OnlyOutputData0x05))
            {
                CheckOutputReportTypes();
            }

            sendOutputReport(true, true, false); // initialize the output report (don't force disconnect the gamepad on initialization even if writeData fails because some fake DS4 gamepads don't support writeData over BT)
        }

        private void CheckOutputReportTypes()
        {
            // Use Tuple here for convenience
            var reportIds = new (byte Id, int Length)[]
            {
                (0x15, BT_OUTPUT_REPORT_0x15_LENGTH),
                (0x11, BT_OUTPUT_REPORT_0x11_LENGTH),
            };

            byte finalReport = 0x00;
            foreach(var element in reportIds)
            {
                int len = element.Length;
                byte[] outputBuffer = new byte[element.Length];
                outputBuffer[0] = element.Id;
                //outputBuffer[1] = (byte)(0xC0 | 0x04);
                outputBuffer[2] = 0xA0;

                // Need to calculate and populate CRC-32 data so controller will accept the report
                uint calcCrc32 = ~Crc32Algorithm.Compute(outputBTCrc32Head);
                calcCrc32 = ~Crc32Algorithm.CalculateBasicHash(ref calcCrc32, ref outputBuffer, 0, len - 4);
                outputBuffer[len - 4] = (byte)calcCrc32;
                outputBuffer[len - 3] = (byte)(calcCrc32 >> 8);
                outputBuffer[len - 2] = (byte)(calcCrc32 >> 16);
                outputBuffer[len - 1] = (byte)(calcCrc32 >> 24);

                if (WriteOutput(outputBuffer))
                {
                    finalReport = element.Id;
                    knownGoodBTOutputReportType = element.Id;
                    outputReport = new byte[len];
                    outReportBuffer = new byte[len];
                    break;
                }
            }

            if (finalReport == 0x00)
            {
                ModifyFeatureSetFlag(VidPidFeatureSet.NoOutputData, true);
            }
        }

        private void TimeoutTestThread()
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

        protected const int DS4_FEATURE_REPORT_5_LEN = 41;
        protected const int DS4_FEATURE_REPORT_5_CRC32_POS = DS4_FEATURE_REPORT_5_LEN - 4;
        public virtual void RefreshCalibration()
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
                        AppLogger.LogToGui("Gyro Calibration Failed", true);
                        continue;
                    }
                    else if (validCrc)
                    {
                        found = true;
                    }
                }

                sixAxis.setCalibrationData(ref calibration, conType == ConnectionType.USB);

                if (hDevice.Attributes.ProductId == 0x5C4 && hDevice.Attributes.VendorId == 0x054C &&
                    sixAxis.fixupInvertedGyroAxis())
                    AppLogger.LogToGui($"Automatically fixed inverted YAW gyro axis in DS4 v.1 BT gamepad ({Mac.ToString()})", false);
            }
            else
            {
                hDevice.readFeatureData(calibration);
                sixAxis.setCalibrationData(ref calibration, conType == ConnectionType.USB);
            }
        }

        public virtual void StartUpdate()
        {
            this.inputReportErrorCount = 0;

            if (ds4Input == null)
            {
                if (conType == ConnectionType.BT)
                {
                    if (btOutputMethod == BTOutputReportMethod.HidD_SetOutputReport)
                    {
                        ds4Output = new Thread(performDs4Output);
                        ds4Output.Priority = ThreadPriority.Normal;
                        ds4Output.Name = "DS4 Output thread: " + Mac;
                        ds4Output.IsBackground = true;
                        ds4Output.Start();
                    }

                    timeoutCheckThread = new Thread(TimeoutTestThread);
                    timeoutCheckThread.Priority = ThreadPriority.BelowNormal;
                    timeoutCheckThread.Name = "DS4 Timeout thread: " + Mac;
                    timeoutCheckThread.IsBackground = true;
                    timeoutCheckThread.Start();
                }
                else
                {
                    ds4Output = new Thread(OutReportCopy);
                    ds4Output.Priority = ThreadPriority.Normal;
                    ds4Output.Name = "DS4 Arr Copy thread: " + Mac;
                    ds4Output.IsBackground = true;
                    ds4Output.Start();
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

        public virtual void StopUpdate()
        {
            if (ds4Input != null &&
                ds4Input.IsAlive && !ds4Input.ThreadState.HasFlag(System.Threading.ThreadState.Stopped) &&
                !ds4Input.ThreadState.HasFlag(System.Threading.ThreadState.AbortRequested))
            {
                try
                {
                    exitInputThread = true;
                    //ds4Input.Interrupt();
                    if (!abortInputThread)
                        ds4Input.Join();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }

            StopOutputUpdate();
        }

        protected virtual void StopOutputUpdate()
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

        protected bool WriteOutput(byte[] outputBuffer)
        {
            if (conType == ConnectionType.BT)
            {
                //if ((this.featureSet & VidPidFeatureSet.OnlyOutputData0x05) == 0)
                //    return hDevice.WriteOutputReportViaControl(outputReport);

                if (btOutputMethod == BTOutputReportMethod.WriteFile)
                {
                    // Use Interrupt endpoint for almost BT DS4 connected devices now
                    return hDevice.WriteOutputReportViaInterrupt(outputBuffer, READ_STREAM_TIMEOUT);
                }
                else
                {
                    // Mainly needed for Windows 7 support
                    return hDevice.WriteOutputReportViaControl(outputBuffer);
                }
            }
            else
            {
                return hDevice.WriteOutputReportViaInterrupt(outputBuffer, READ_STREAM_TIMEOUT);
            }
        }

        protected bool writeOutput()
        {
            if (conType == ConnectionType.BT)
            {
                //if ((this.featureSet & VidPidFeatureSet.OnlyOutputData0x05) == 0)
                //    return hDevice.WriteOutputReportViaControl(outputReport);

                if (btOutputMethod == BTOutputReportMethod.WriteFile)
                {
                    // Use Interrupt endpoint for almost BT DS4 connected devices now
                    return hDevice.WriteOutputReportViaInterrupt(outputReport, READ_STREAM_TIMEOUT);
                }
                else
                {
                    // Mainly needed for Windows 7 support
                    return hDevice.WriteOutputReportViaControl(outputReport);
                }
            }
            else
            {
                return hDevice.WriteOutputReportViaInterrupt(outReportBuffer, READ_STREAM_TIMEOUT);
            }
        }

        private readonly Stopwatch rumbleAutostopTimer = new Stopwatch(); // Autostop timer to stop rumble motors if those are stuck in a rumble state

        private byte outputPendCount = 0;
        private const int OUTPUT_MIN_COUNT_BT = 3;
        private byte[] outputBTCrc32Head = new byte[] { 0xA2 };
        protected readonly Stopwatch standbySw = new Stopwatch();
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
                            if (outputPendCount > 1)
                                outputPendCount--;
                            else if (outputPendCount == 1)
                            {
                                outputPendCount--;
                                standbySw.Restart();
                            }
                            else
                                standbySw.Restart();
                        }

                        currentRumble = true;
                    }
                }
            }
            catch (ThreadInterruptedException) { }
        }

        /** Is the device alive and receiving valid sensor input reports? */
        public virtual bool IsAlive()
        {
            return priorInputReport30 != 0xff;
        }

        private byte priorInputReport30 = 0xff;

        protected bool synced = false;
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
        protected DateTime utcNow = DateTime.UtcNow;
        protected bool ds4InactiveFrame = true;
        protected bool idleInput = true;

        bool timeStampInit = false;
        uint timeStampPrevious = 0;
        uint deltaTimeCurrent = 0;


        protected const int BT_INPUT_REPORT_CRC32_POS = 74; //last 4 bytes of the 78-sized input report are crc32
        public const uint DefaultPolynomial = 0xedb88320u;
        protected uint HamSeed = 2351727372;

        protected unsafe void performDs4Input()
        {
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
                bool syncWriteReport = conType != ConnectionType.BT ||
                    btOutputMethod == BTOutputReportMethod.WriteFile;
                //bool syncWriteReport = true;
                bool forceWrite = false;

                int maxBatteryValue = 0;
                int tempBattery = 0;
                bool tempCharging = charging;
                uint tempStamp = 0;
                double elapsedDeltaTime = 0.0;
                uint tempDelta = 0;
                byte tempByte = 0;
                int CRC32_POS_1 = BT_INPUT_REPORT_CRC32_POS + 1,
                    CRC32_POS_2 = BT_INPUT_REPORT_CRC32_POS + 2,
                    CRC32_POS_3 = BT_INPUT_REPORT_CRC32_POS + 3;
                int crcpos = BT_INPUT_REPORT_CRC32_POS;
                int crcoffset = 0;
                long latencySum = 0;
                standbySw.Start();

                while (!exitInputThread)
                {
                    oldCharging = charging;
                    currerror = string.Empty;

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

                    readWaitEv.Set();

                    // Sony DS4 and compatible gamepads send data packets with 0x11 type code in BT mode. 
                    // However, couple non-Sony gamepads behave like USB devices in BT mode also, so if OnlyInputData0x01 is set then BT specific crc32 checks are not calculated.
                    if (conType == ConnectionType.BT && (this.featureSet & VidPidFeatureSet.OnlyInputData0x01) == 0)
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
                                for (int j = 0; j < BT_INPUT_REPORT_LENGTH - 2; j++)
                                {
                                    imp[j] = byteP[j];
                                }
                            }

                            //uint recvCrc32 = BitConverter.ToUInt32(btInputReport, BT_INPUT_REPORT_CRC32_POS);
                            uint recvCrc32 = btInputReport[BT_INPUT_REPORT_CRC32_POS] |
                                (uint)(btInputReport[CRC32_POS_1] << 8) |
                                (uint)(btInputReport[CRC32_POS_2] << 16) |
                                (uint)(btInputReport[CRC32_POS_3] << 24);

                            uint calcCrc32 = ~Crc32Algorithm.CalculateFasterBT78Hash(ref HamSeed, ref btInputReport, ref crcoffset, ref crcpos);
                            if (recvCrc32 != calcCrc32)
                            {
                                //Log.LogToGui("Crc check failed", true);
                                //Console.WriteLine(MacAddress.ToString() + " " + System.DateTime.UtcNow.ToString("o") + "" +
                                //                    "> invalid CRC32 in BT input report: 0x" + recvCrc32.ToString("X8") + " expected: 0x" + calcCrc32.ToString("X8"));

                                cState.PacketCounter = pState.PacketCounter + 1; //still increase so we know there were lost packets

                                // If the incoming data packet doesn't have the native DS4 type (0x11) in BT mode then the gamepad sends PC-friendly 0x01 data packets even in BT mode. Switch over to accept 0x01 data packets in BT mode.
                                if (this.inputReportErrorCount >= 10)
                                {
                                    if (btInputReport[0] == 0x01)
                                    {
                                        this.inputReportErrorCount = 0;
                                        this.ModifyFeatureSetFlag(VidPidFeatureSet.OnlyInputData0x01, true);
                                        AppLogger.LogToGui(Mac.ToString() + " switching over to accept PC-friendly data packets in BT mode", false);
                                    }
                                }
                                else
                                    this.inputReportErrorCount++;

                                readWaitEv.Reset();
                                continue;
                            }
                            else
                                this.inputReportErrorCount = 0;
                        }
                        else
                        {
                            if (res == HidDevice.ReadStatus.WaitTimedOut)
                            {
                                AppLogger.LogToGui(Mac.ToString() + " disconnected due to timeout", true);
                            }
                            else
                            {
                                int winError = Marshal.GetLastWin32Error();
                                Console.WriteLine(Mac.ToString() + " " + DateTime.UtcNow.ToString("o") + "> disconnect due to read failure: " + winError);
                                //Log.LogToGui(Mac.ToString() + " disconnected due to read failure: " + winError, true);
                            }

                            readWaitEv.Reset();
                            sendOutputReport(true, true); // Kick Windows into noticing the disconnection.
                            StopOutputUpdate();
                            isDisconnecting = true;
                            Removal?.Invoke(this, EventArgs.Empty);

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
                                AppLogger.LogToGui(Mac.ToString() + " disconnected due to timeout", true);
                            }
                            else
                            {
                                int winError = Marshal.GetLastWin32Error();
                                Console.WriteLine(Mac.ToString() + " " + DateTime.UtcNow.ToString("o") + "> disconnect due to read failure: " + winError);
                                //Log.LogToGui(Mac.ToString() + " disconnected due to read failure: " + winError, true);
                            }

                            readWaitEv.Reset();
                            StopOutputUpdate();
                            isDisconnecting = true;
                            Removal?.Invoke(this, EventArgs.Empty);

                            timeoutExecuted = true;
                            return;
                        }
                    }

                    readWaitEv.Wait();
                    readWaitEv.Reset();

                    curtime = Stopwatch.GetTimestamp();
                    testelapsed = curtime - oldtime;
                    lastTimeElapsedDouble = testelapsed * (1.0 / Stopwatch.Frequency) * 1000.0;
                    lastTimeElapsed = (long)lastTimeElapsedDouble;
                    oldtime = curtime;

                    if (conType == ConnectionType.BT && btInputReport[0] != 0x11 && (this.featureSet & VidPidFeatureSet.OnlyInputData0x01) == 0)
                    {
                        //Received incorrect report, skip it
                        continue;
                    }

                    utcNow = DateTime.UtcNow; // timestamp with UTC in case system time zone changes

                    cState.PacketCounter = pState.PacketCounter + 1;
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
                    cState.R2Btn = (inputReport[6] & (1 << 3)) != 0;
                    cState.L2Btn = (inputReport[6] & (1 << 2)) != 0;
                    cState.R1 = (tempByte & (1 << 1)) != 0;
                    cState.L1 = (tempByte & (1 << 0)) != 0;

                    tempByte = inputReport[7];
                    cState.PS = (tempByte & (1 << 0)) != 0;
                    cState.TouchButton = (tempByte & 0x02) != 0;
                    cState.OutputTouchButton = cState.TouchButton;
                    cState.FrameCounter = (byte)(tempByte >> 2);

                    if ((this.featureSet & VidPidFeatureSet.NoBatteryReading) == 0)
                    {
                        tempByte = inputReport[30];
                        tempCharging = (tempByte & 0x10) != 0;
                        if (tempCharging != charging)
                        {
                            charging = tempCharging;
                            ChargingChanged?.Invoke(this, EventArgs.Empty);
                        }

                        maxBatteryValue = charging ? BATTERY_MAX_USB : BATTERY_MAX;
                        tempBattery = (tempByte & 0x0f) * 100 / maxBatteryValue;
                        tempBattery = Math.Min(tempBattery, 100);
                        if (tempBattery != battery)
                        {
                            battery = tempBattery;
                            BatteryChanged?.Invoke(this, EventArgs.Empty);
                        }

                        cState.Battery = (byte)battery;
                        //System.Diagnostics.Debug.WriteLine("CURRENT BATTERY: " + (inputReport[30] & 0x0f) + " | " + tempBattery + " | " + battery);
                        if (tempByte != priorInputReport30)
                        {
                            priorInputReport30 = tempByte;
                            //Console.WriteLine(MacAddress.ToString() + " " + System.DateTime.UtcNow.ToString("o") + "> power subsystem octet: 0x" + inputReport[30].ToString("x02"));
                        }
                    }
                    else
                    {
                        // Some gamepads don't send battery values in DS4 compatible data fields, so use dummy 99% value to avoid constant low battery warnings
                        priorInputReport30 = 0x0F;
                        battery = 99;
                        cState.Battery = 99;
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

                    // Make sure timestamps don't match
                    if (deltaTimeCurrent != 0)
                    {
                        elapsedDeltaTime = 0.000001 * deltaTimeCurrent; // Convert from microseconds to seconds
                        cState.totalMicroSec = pState.totalMicroSec + deltaTimeCurrent;
                    }
                    else
                    {
                        // Duplicate timestamp. Use system clock for elapsed time instead
                        elapsedDeltaTime = lastTimeElapsedDouble * .001;
                        cState.totalMicroSec = pState.totalMicroSec + (uint)(elapsedDeltaTime * 1000000);
                    }

                    cState.elapsedTime = elapsedDeltaTime;
                    cState.ds4Timestamp = (ushort)tempStamp;
                    timeStampPrevious = tempStamp;

                    //Simpler touch storing
                    cState.TrackPadTouch0.RawTrackingNum = inputReport[35];
                    cState.TrackPadTouch0.Id = (byte)(inputReport[35] & 0x7f);
                    cState.TrackPadTouch0.IsActive = (inputReport[35] & 0x80) == 0;
                    cState.TrackPadTouch0.X = (short)(((ushort)(inputReport[37] & 0x0f) << 8) | (ushort)(inputReport[36]));
                    cState.TrackPadTouch0.Y = (short)(((ushort)(inputReport[38]) << 4) | ((ushort)(inputReport[37] & 0xf0) >> 4));

                    cState.TrackPadTouch1.RawTrackingNum = inputReport[39];
                    cState.TrackPadTouch1.Id = (byte)(inputReport[39] & 0x7f);
                    cState.TrackPadTouch1.IsActive = (inputReport[39] & 0x80) == 0;
                    cState.TrackPadTouch1.X = (short)(((ushort)(inputReport[41] & 0x0f) << 8) | (ushort)(inputReport[40]));
                    cState.TrackPadTouch1.Y = (short)(((ushort)(inputReport[42]) << 4) | ((ushort)(inputReport[41] & 0xf0) >> 4));

                    if (conType == ConnectionType.SONYWA)
                    {
                        bool controllerSynced = inputReport[31] == 0;
                        if (controllerSynced != synced)
                        {
                            runCalib = synced = controllerSynced;
                            SyncChange?.Invoke(this, EventArgs.Empty);
                            if (synced)
                            {
                                forceWrite = true;
                            }
                            else
                            {
                                standbySw.Reset();
                            }
                        }
                    }

                    // XXX DS4State mapping needs fixup, turn touches into an array[4] of structs.  And include the touchpad details there instead.
                    try
                    {
                        // Only care if one touch packet is detected. Other touch packets
                        // don't seem to contain relevant data. ds4drv does not use them either.
                        for (int touches = Math.Max((int)(inputReport[-1 + DS4Touchpad.DS4_TOUCHPAD_DATA_OFFSET - 1]), 1), touchOffset = 0; touches > 0; touches--, touchOffset += 9)
                        //for (int touches = inputReport[-1 + DS4Touchpad.TOUCHPAD_DATA_OFFSET - 1], touchOffset = 0; touches > 0; touches--, touchOffset += 9)
                        {
                            cState.TouchPacketCounter = inputReport[-1 + DS4Touchpad.DS4_TOUCHPAD_DATA_OFFSET + touchOffset];
                            cState.Touch1 = (inputReport[0 + DS4Touchpad.DS4_TOUCHPAD_DATA_OFFSET + touchOffset] >> 7) != 0 ? false : true; // finger 1 detected
                            cState.Touch1Identifier = (byte)(inputReport[0 + DS4Touchpad.DS4_TOUCHPAD_DATA_OFFSET + touchOffset] & 0x7f);
                            cState.Touch2 = (inputReport[4 + DS4Touchpad.DS4_TOUCHPAD_DATA_OFFSET + touchOffset] >> 7) != 0 ? false : true; // finger 2 detected
                            cState.Touch2Identifier = (byte)(inputReport[4 + DS4Touchpad.DS4_TOUCHPAD_DATA_OFFSET + touchOffset] & 0x7f);
                            cState.Touch1Finger = cState.Touch1 || cState.Touch2; // >= 1 touch detected
                            cState.Touch2Fingers = cState.Touch1 && cState.Touch2; // 2 touches detected
                            int touchX = (((inputReport[2 + DS4Touchpad.DS4_TOUCHPAD_DATA_OFFSET + touchOffset] & 0xF) << 8) | inputReport[1 + DS4Touchpad.DS4_TOUCHPAD_DATA_OFFSET + touchOffset]);
                            cState.TouchLeft = touchX >= DS4Touchpad.RESOLUTION_X_MAX * 2 / 5 ? false : true;
                            cState.TouchRight = touchX < DS4Touchpad.RESOLUTION_X_MAX * 2 / 5 ? false : true;
                            // Even when idling there is still a touch packet indicating no touch 1 or 2
                            if (synced)
                            {
                                touchpad.handleTouchpad(inputReport, cState, DS4Touchpad.DS4_TOUCHPAD_DATA_OFFSET, touchOffset);
                            }
                        }
                    }
                    catch (Exception ex) { currerror = $"Touchpad: {ex.Message}"; }

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
                            pbAccel[i - 6] = pbInput[i];
                        }

                        if (synced)
                        {
                            sixAxis.handleSixaxis(pbGyro, pbAccel, cState, elapsedDeltaTime);
                        }
                    }

                    /* Debug output of incoming HID data:
                    if (cState.L2 == 0xff && cState.R2 == 0xff)
                    {
                        Console.Write(MacAddress.ToString() + " " + System.DateTime.UtcNow.ToString("o") + ">");
                        for (int i = 0; i < inputReport.Length; i++)
                            Console.Write(" " + inputReport[i].ToString("x2"));
                        Console.WriteLine();
                    }
                    */

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
                            AppLogger.LogToGui(Mac.ToString() + " disconnecting due to idle disconnect", false);

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

                    if (Report != null)
                        Report(this, EventArgs.Empty);

                    sendOutputReport(syncWriteReport, forceWrite);
                    forceWrite = false;

                    if (!string.IsNullOrEmpty(currerror))
                        error = currerror;
                    else if (!string.IsNullOrEmpty(error))
                        error = string.Empty;

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

        private unsafe void PrepareOutputReportInner(ref bool change, ref bool haptime)
        {
            bool usingBT = conType == ConnectionType.BT;

            if (usingBT && (this.featureSet & VidPidFeatureSet.OnlyOutputData0x05) == 0)
            {
                outReportBuffer[0] = knownGoodBTOutputReportType;
                //outReportBuffer[0] = 0x15;
                //outReportBuffer[1] = (byte)(0x80 | btPollRate); // input report rate
                outReportBuffer[1] = (byte)(0xC0 | btPollRate); // input report rate
                outReportBuffer[2] = 0xA0;

                // enable rumble (0x01), lightbar (0x02), flash (0x04). Default: 0xF7
                outReportBuffer[3] = outputFeaturesByte;
                outReportBuffer[4] = 0x04;

                outReportBuffer[6] = currentHap.rumbleState.RumbleMotorStrengthRightLightFast; // fast motor
                outReportBuffer[7] = currentHap.rumbleState.RumbleMotorStrengthLeftHeavySlow; // slow motor
                outReportBuffer[8] = currentHap.lightbarState.LightBarColor.red; // red
                outReportBuffer[9] = currentHap.lightbarState.LightBarColor.green; // green
                outReportBuffer[10] = currentHap.lightbarState.LightBarColor.blue; // blue
                outReportBuffer[11] = currentHap.lightbarState.LightBarFlashDurationOn; // flash on duration
                outReportBuffer[12] = currentHap.lightbarState.LightBarFlashDurationOff; // flash off duration

                fixed (byte* byteR = outputReport, byteB = outReportBuffer)
                {
                    for (int i = 0, arlen = BT_OUTPUT_CHANGE_LENGTH; !change && i < arlen; i++)
                        change = byteR[i] != byteB[i];
                }

                /*if (change)
                {
                    Console.WriteLine("CHANGE: {0} {1} {2} {3} {4} {5}", currentHap.LightBarColor.red, currentHap.LightBarColor.green, currentHap.LightBarColor.blue, currentHap.RumbleMotorStrengthRightLightFast, currentHap.RumbleMotorStrengthLeftHeavySlow, DateTime.Now.ToString());
                }
                */

                haptime = haptime || change;
            }
            else
            {
                outReportBuffer[0] = 0x05;
                // enable rumble (0x01), lightbar (0x02), flash (0x04). Default: 0xF7
                outReportBuffer[1] = outputFeaturesByte;
                outReportBuffer[2] = 0x04;
                outReportBuffer[4] = currentHap.rumbleState.RumbleMotorStrengthRightLightFast; // fast motor
                outReportBuffer[5] = currentHap.rumbleState.RumbleMotorStrengthLeftHeavySlow; // slow  motor
                outReportBuffer[6] = currentHap.lightbarState.LightBarColor.red; // red
                outReportBuffer[7] = currentHap.lightbarState.LightBarColor.green; // green
                outReportBuffer[8] = currentHap.lightbarState.LightBarColor.blue; // blue
                outReportBuffer[9] = currentHap.lightbarState.LightBarFlashDurationOn; // flash on duration
                outReportBuffer[10] = currentHap.lightbarState.LightBarFlashDurationOff; // flash off duration

                fixed (byte* byteR = outputReport, byteB = outReportBuffer)
                {
                    for (int i = 0, arlen = USB_OUTPUT_CHANGE_LENGTH; !change && i < arlen; i++)
                        change = byteR[i] != byteB[i];
                }

                haptime = haptime || change;
                if (haptime && audio != null)
                {
                    // Headphone volume levels
                    outReportBuffer[19] = outReportBuffer[20] =
                        Convert.ToByte(audio.getVolume());
                    // Microphone volume level
                    outReportBuffer[21] = Convert.ToByte(micAudio.getVolume());
                }
            }
        }

        private void sendOutputReport(bool synchronous, bool force = false, bool quitOutputThreadOnError = true)
        {
            MergeStates();
            //setTestRumble();
            //setHapticState();

            bool quitOutputThread = false;
            bool usingBT = conType == ConnectionType.BT;

            // Some gamepads don't support lightbar and rumble, so no need to write out anything (writeOut always fails, so DS4Windows would accidentally force quit the gamepad connection).
            // If noOutputData featureSet flag is set then don't try to write out anything to the gamepad device.
            if ((this.featureSet & VidPidFeatureSet.NoOutputData) != 0)
            {
                if (exitOutputThread == false && (IsRemoving || IsRemoved))
                {
                    // Gamepad disconnecting or disconnected. Signal closing of OutputUpdate thread
                    StopOutputUpdate();
                    exitOutputThread = true;
                }

                return;
            }

            //bool output = outputPendCount > 0, change = force;
            bool output = outputPendCount > 0, change = force;
            //bool output = false, change = force;
            bool haptime = output || standbySw.ElapsedMilliseconds >= 4000L;

            if (usingBT &&
                btOutputMethod == BTOutputReportMethod.HidD_SetOutputReport)
            {
                Monitor.Enter(outReportBuffer);
            }

            PrepareOutputReportInner(ref change, ref haptime);

            if (rumbleAutostopTimer.IsRunning)
            {
                // Workaround to a bug in ViGem driver. Force stop potentially stuck rumble motor on the next output report if there haven't been new rumble events within X seconds
                if (rumbleAutostopTimer.ElapsedMilliseconds >= rumbleAutostopTime)
                    setRumble(0, 0);
            }

            if (synchronous)
            {
                if (output || haptime)
                {
                    if (change)
                    {
                        outputPendCount = OUTPUT_MIN_COUNT_BT;
                        standbySw.Reset();
                    }
                    else if (outputPendCount > 1)
                        outputPendCount--;
                    else if (outputPendCount == 1)
                    {
                        outputPendCount--;
                        standbySw.Restart();
                    }
                    else
                        standbySw.Restart();
                    //standbySw.Restart();

                    if (usingBT)
                    {
                        if (btOutputMethod == BTOutputReportMethod.HidD_SetOutputReport)
                        {
                            Monitor.Enter(outputReport);
                        }

                        outReportBuffer.CopyTo(outputReport, 0);

                        if ((this.featureSet & VidPidFeatureSet.OnlyOutputData0x05) == 0)
                        {
                            // Need to calculate and populate CRC-32 data so controller will accept the report
                            int len = outputReport.Length;
                            uint calcCrc32 = ~Crc32Algorithm.Compute(outputBTCrc32Head);
                            calcCrc32 = ~Crc32Algorithm.CalculateBasicHash(ref calcCrc32, ref outputReport, 0, len - 4);
                            outputReport[len - 4] = (byte)calcCrc32;
                            outputReport[len - 3] = (byte)(calcCrc32 >> 8);
                            outputReport[len - 2] = (byte)(calcCrc32 >> 16);
                            outputReport[len - 1] = (byte)(calcCrc32 >> 24);

                            //Console.WriteLine("Write CRC-32 to output report");
                        }
                    }

                    try
                    {
                        if (!writeOutput())
                        {
                            if (quitOutputThreadOnError)
                            {
                                int winError = Marshal.GetLastWin32Error();

                                // Logfile notification that the gamepad is force disconnected because of writeOutput failed
                                if (quitOutputThread == false && !isDisconnecting)
                                    AppLogger.LogToGui($"Gamepad data write connection is lost. Disconnecting the gamepad. LastErrorCode={winError}", false);

                                quitOutputThread = true;
                            }
                        }
                    }
                    catch { } // If it's dead already, don't worry about it.

                    if (usingBT)
                    {
                        if (btOutputMethod == BTOutputReportMethod.HidD_SetOutputReport)
                        {
                            Monitor.Exit(outputReport);
                        }
                    }
                    else
                    {
                        lock(outReportBuffer)
                        {
                            Monitor.Pulse(outReportBuffer);
                        }
                    }
                }
            }
            else
            {
                //for (int i = 0, arlen = outputReport.Length; !change && i < arlen; i++)
                //    change = outputReport[i] != outReportBuffer[i];

                if (output || haptime)
                {
                    if (change)
                    {
                        outputPendCount = OUTPUT_MIN_COUNT_BT;
                        standbySw.Reset();
                    }

                    Monitor.Pulse(outReportBuffer);
                }
            }

            if (usingBT &&
                btOutputMethod == BTOutputReportMethod.HidD_SetOutputReport)
            {
                Monitor.Exit(outReportBuffer);
            }

            if (quitOutputThread)
            {
                StopOutputUpdate();
                exitOutputThread = true;
            }
        }

        // Perform outReportBuffer copy on a separate thread to save
        // time on main input thread
        private void OutReportCopy()
        {
            try
            {
                while (!exitOutputThread)
                {
                    lock (outReportBuffer)
                    {
                        outReportBuffer.CopyTo(outputReport, 0);
                        Monitor.Wait(outReportBuffer);
                    }
                }
            }
            catch (ThreadInterruptedException) { }
        }

        public virtual bool DisconnectWireless(bool callRemoval = false)
        {
            bool result = false;
            if (conType == ConnectionType.BT)
            {
                result = DisconnectBT(callRemoval);
            }
            else if (conType == ConnectionType.SONYWA)
            {
                result = DisconnectDongle(callRemoval);
            }

            return result;
        }

        public virtual bool DisconnectBT(bool callRemoval = false)
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
                        Removal?.Invoke(this, EventArgs.Empty);

                        //System.Threading.Tasks.Task.Factory.StartNew(() => { Removal?.Invoke(this, EventArgs.Empty); });
                    }
                }

                return success;
            }

            return false;
        }

        public virtual bool DisconnectDongle(bool remove = false)
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

                Removal?.Invoke(this, EventArgs.Empty);

                //System.Threading.Tasks.Task.Factory.StartNew(() => { Removal?.Invoke(this, EventArgs.Empty); });
                //Removal?.Invoke(this, EventArgs.Empty);
            }
            else if (result && !remove)
            {
                isRemoved = true;
            }

            return result;
        }

        protected DS4HapticState testRumble = new DS4HapticState();

        public void setRumble(byte rightLightFastMotor, byte leftHeavySlowMotor)
        {
            testRumble.rumbleState.RumbleMotorStrengthRightLightFast = rightLightFastMotor;
            testRumble.rumbleState.RumbleMotorStrengthLeftHeavySlow = leftHeavySlowMotor;
            testRumble.rumbleState.RumbleMotorsExplicitlyOff = rightLightFastMotor == 0 && leftHeavySlowMotor == 0;

            // If rumble autostop timer (msecs) is enabled for this device then restart autostop timer everytime rumble is modified (or stop the timer if rumble is set to zero)
            if (rumbleAutostopTime > 0)
            {
                if (testRumble.rumbleState.RumbleMotorsExplicitlyOff)
                    rumbleAutostopTimer.Reset();   // Stop an autostop timer because ViGem driver sent properly a zero rumble notification
                else if (currentHap.rumbleState.RumbleMotorStrengthLeftHeavySlow != leftHeavySlowMotor || currentHap.rumbleState.RumbleMotorStrengthRightLightFast != rightLightFastMotor)
                    rumbleAutostopTimer.Restart(); // Start an autostop timer to stop potentially stuck rumble motor because of lost rumble notification events from ViGem driver
            }
        }

        protected void MergeStates()
        {
            if (testRumble.IsRumbleSet())
            {
                if (testRumble.rumbleState.RumbleMotorsExplicitlyOff)
                    testRumble.rumbleState.RumbleMotorsExplicitlyOff = false;

                //currentHap.rumbleState.RumbleMotorStrengthLeftHeavySlow = testRumble.rumbleState.RumbleMotorStrengthLeftHeavySlow;
                //currentHap.rumbleState.RumbleMotorStrengthRightLightFast = testRumble.rumbleState.RumbleMotorStrengthRightLightFast;
                currentHap.rumbleState = testRumble.rumbleState;
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

        public bool isDS4Idle()
        {
            if (cState.Square || cState.Cross || cState.Circle || cState.Triangle)
                return false;
            if (cState.DpadUp || cState.DpadLeft || cState.DpadDown || cState.DpadRight)
                return false;
            if (cState.L3 || cState.R3 || cState.L1 || cState.R1 || cState.Share || cState.Options || cState.PS)
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

        protected DS4HapticState currentHap = new DS4HapticState();
        public void SetHapticState(ref DS4HapticState hs)
        {
            currentHap = hs;
        }

        public void SetLightbarState(ref DS4LightbarState lightState)
        {
            currentHap.lightbarState = lightState;
        }

        public void SetRumbleState(ref DS4ForceFeedbackState rumbleState)
        {
            currentHap.rumbleState = rumbleState;
        }

        public override string ToString()
        {
            return Mac;
        }

        protected void RunRemoval()
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
                hasInputEvts = true;
            }
        }

        public void updateSerial()
        {
            hDevice.resetSerial();
            string tempMac = hDevice.ReadSerial(SerialReportID);
            if (tempMac != Mac)
            {
                Mac = tempMac;
                SerialChange?.Invoke(this, EventArgs.Empty);
                MacAddressChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public bool isValidSerial()
        {
            return !Mac.Equals(BLANK_SERIAL);
        }

        public static bool isValidSerial(string test)
        {
            return !test.Equals(BLANK_SERIAL);
        }

        private bool abortInputThread = false;
        public void PrepareAbort()
        {
            abortInputThread = true;
        }

        private void PrepareOutputFeaturesByte()
        {
            if (nativeOptionsStore != null)
            {
                if (nativeOptionsStore.IsCopyCat)
                {
                    outputFeaturesByte = COPYCAT_OUTPUT_FEATURES;
                }
                else
                {
                    outputFeaturesByte = DEFAULT_OUTPUT_FEATURES;
                }
            }
        }

        private void SetupOptionsEvents()
        {
            if (nativeOptionsStore != null)
            {
                nativeOptionsStore.IsCopyCatChanged += (sender, e) =>
                {
                    PrepareOutputFeaturesByte();
                };
            }
        }

        public virtual void PrepareTriggerEffect(InputDevices.TriggerId trigger,
            InputDevices.TriggerEffects effect)
        {
        }

        public virtual void CheckControllerNumDeviceSettings(int numControllers)
        {
        }

        public virtual void LoadStoreSettings()
        {
            if (nativeOptionsStore != null)
            {
                PrepareOutputFeaturesByte();
            }
        }
    }
}
