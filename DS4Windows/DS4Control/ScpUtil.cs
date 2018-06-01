using System;
using System.Collections.Generic;
using System.Linq;

using System.IO;
using System.Reflection;
using System.Xml;
using System.Drawing;

using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using System.Globalization;

namespace DS4Windows
{
    [Flags]
    public enum DS4KeyType : byte { None = 0, ScanCode = 1, Toggle = 2, Unbound = 4, Macro = 8, HoldMacro = 16, RepeatMacro = 32 }; // Increment by exponents of 2*, starting at 2^0
    public enum Ds3PadId : byte { None = 0xFF, One = 0x00, Two = 0x01, Three = 0x02, Four = 0x03, All = 0x04 };
    public enum DS4Controls : byte { None, LXNeg, LXPos, LYNeg, LYPos, RXNeg, RXPos, RYNeg, RYPos, L1, L2, L3, R1, R2, R3, Square, Triangle, Circle, Cross, DpadUp, DpadRight, DpadDown, DpadLeft, PS, TouchLeft, TouchUpper, TouchMulti, TouchRight, Share, Options, GyroXPos, GyroXNeg, GyroZPos, GyroZNeg, SwipeLeft, SwipeRight, SwipeUp, SwipeDown };
    public enum X360Controls : byte { None, LXNeg, LXPos, LYNeg, LYPos, RXNeg, RXPos, RYNeg, RYPos, LB, LT, LS, RB, RT, RS, X, Y, B, A, DpadUp, DpadRight, DpadDown, DpadLeft, Guide, Back, Start, LeftMouse, RightMouse, MiddleMouse, FourthMouse, FifthMouse, WUP, WDOWN, MouseUp, MouseDown, MouseLeft, MouseRight, Unbound };

    public class DS4ControlSettings
    {
        public DS4Controls control;
        public string extras = null;
        public DS4KeyType keyType = DS4KeyType.None;
        public enum ActionType : byte { Default, Key, Button, Macro };
        public ActionType actionType = ActionType.Default;
        public object action = null;
        public ActionType shiftActionType = ActionType.Default;
        public object shiftAction = null;
        public int shiftTrigger = 0;
        public string shiftExtras = null;
        public DS4KeyType shiftKeyType = DS4KeyType.None;

        public DS4ControlSettings(DS4Controls ctrl)
        {
            control = ctrl;
        }

        public void Reset()
        {
            extras = null;
            keyType = DS4KeyType.None;
            actionType = ActionType.Default;
            action = null;
            shiftActionType = ActionType.Default;
            shiftAction = null;
            shiftTrigger = 0;
            shiftExtras = null;
            shiftKeyType = DS4KeyType.None;
        }

        internal void UpdateSettings(bool shift, object act, string exts, DS4KeyType kt, int trigger = 0)
        {
            if (!shift)
            {
                if (act is int || act is ushort)
                    actionType = ActionType.Key;
                else if (act is string || act is X360Controls)
                    actionType = ActionType.Button;
                else if (act is int[])
                    actionType = ActionType.Macro;
                else
                    actionType = ActionType.Default;

                action = act;
                extras = exts;
                keyType = kt;
            }
            else
            {
                if (act is int || act is ushort)
                    shiftActionType = ActionType.Key;
                else if (act is string || act is X360Controls)
                    shiftActionType = ActionType.Button;
                else if (act is int[])
                    shiftActionType = ActionType.Macro;
                else
                    shiftActionType = ActionType.Default;

                shiftAction = act;
                shiftExtras = exts;
                shiftKeyType = kt;
                shiftTrigger = trigger;
            }
        }
    }

    public class DebugEventArgs : EventArgs
    {
        protected DateTime m_Time = DateTime.Now;
        protected string m_Data = string.Empty;
        protected bool warning = false;
        public DebugEventArgs(string Data, bool warn)
        {
            m_Data = Data;
            warning = warn;
        }

        public DateTime Time => m_Time;
        public string Data => m_Data;
        public bool Warning => warning;
    }

    public class MappingDoneEventArgs : EventArgs
    {
        protected int deviceNum = -1;

        public MappingDoneEventArgs(int DeviceID)
        {
            deviceNum = DeviceID;
        }

        public int DeviceID => deviceNum;
    }

    public class ReportEventArgs : EventArgs
    {
        protected Ds3PadId m_Pad = Ds3PadId.None;
        protected byte[] m_Report = new byte[64];

        public ReportEventArgs()
        {
        }

        public ReportEventArgs(Ds3PadId Pad)
        {
            m_Pad = Pad;
        }

        public Ds3PadId Pad
        {
            get { return m_Pad; }
            set { m_Pad = value; }
        }

        public Byte[] Report
        {
            get { return m_Report; }
        }
    }

    public class BatteryReportArgs : EventArgs
    {
        private int index;
        private int level;
        private bool charging;

        public BatteryReportArgs(int index, int level, bool charging)
        {
            this.index = index;
            this.level = level;
            this.charging = charging;
        }

        public int getIndex()
        {
            return index;
        }

        public int getLevel()
        {
            return level;
        }

        public bool isCharging()
        {
            return charging;
        }
    }

    public class ControllerRemovedArgs : EventArgs
    {
        private int index;

        public ControllerRemovedArgs(int index)
        {
            this.index = index;
        }

        public int getIndex()
        {
            return this.index;
        }
    }

    public class DeviceStatusChangeEventArgs : EventArgs
    {
        private int index;

        public DeviceStatusChangeEventArgs(int index)
        {
            this.index = index;
        }

        public int getIndex()
        {
            return index;
        }
    }

    public class SerialChangeArgs : EventArgs
    {
        private int index;
        private string serial;

        public SerialChangeArgs(int index, string serial)
        {
            this.index = index;
            this.serial = serial;
        }

        public int getIndex()
        {
            return index;
        }

        public string getSerial()
        {
            return serial;
        }
    }

    public class Global
    {
        protected static BackingStore m_Config = new BackingStore();
        protected static Int32 m_IdleTimeout = 600000;
        public static string exepath = Directory.GetParent(Assembly.GetExecutingAssembly().Location).FullName;
        public static string appdatapath;
        public static bool firstRun = false;
        public static bool multisavespots = false;
        public static bool oldappdatafail = false;
        public static string appDataPpath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\DS4Windows";
        public static string oldappdatapath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\DS4Tool";
        public static bool runHotPlug = false;
        public const int XINPUT_UNPLUG_SETTLE_TIME = 250; // Inhibit races that occur with the asynchronous teardown of ScpVBus -> X360 driver instance.
        public static string[] tempprofilename = new string[5] { string.Empty, string.Empty, string.Empty, string.Empty, string.Empty };
        public static bool[] tempprofileDistance = new bool[5] { false, false, false, false, false };
        public static bool[] useDInputOnly = new bool[5] { true, true, true, true, true };
        public static bool[] linkedProfileCheck = new bool[4] { true, true, true, true };

        public static X360Controls[] defaultButtonMapping = { X360Controls.None, X360Controls.LXNeg, X360Controls.LXPos,
            X360Controls.LYNeg, X360Controls.LYPos, X360Controls.RXNeg, X360Controls.RXPos, X360Controls.RYNeg, X360Controls.RYPos,
            X360Controls.LB, X360Controls.LT, X360Controls.LS, X360Controls.RB, X360Controls.RT, X360Controls.RS, X360Controls.X,
            X360Controls.Y, X360Controls.B, X360Controls.A, X360Controls.DpadUp, X360Controls.DpadRight, X360Controls.DpadDown,
            X360Controls.DpadLeft, X360Controls.Guide, X360Controls.None, X360Controls.None, X360Controls.None, X360Controls.None,
            X360Controls.Back, X360Controls.Start, X360Controls.None, X360Controls.None, X360Controls.None, X360Controls.None,
            X360Controls.None, X360Controls.None, X360Controls.None, X360Controls.None
        };

        // Create mapping array at runtime
        public static DS4Controls[] reverseX360ButtonMapping = new Func<DS4Controls[]>(() =>
        {
            DS4Controls[] temp = new DS4Controls[defaultButtonMapping.Length];
            for (int i = 0, arlen = defaultButtonMapping.Length; i < arlen; i++)
            {
                X360Controls mapping = defaultButtonMapping[i];
                if (mapping != X360Controls.None)
                {
                    temp[(int)mapping] = (DS4Controls)i;
                }
            }

            return temp;
        })();

        public static void SaveWhere(string path)
        {
            appdatapath = path;
            m_Config.m_Profile = appdatapath + "\\Profiles.xml";
            m_Config.m_Actions = appdatapath + "\\Actions.xml";
            m_Config.m_linkedProfiles = Global.appdatapath + "\\LinkedProfiles.xml";
        }

        /// <summary>
        /// Check if Admin Rights are needed to write in Appliplation Directory
        /// </summary>
        /// <returns></returns>
        public static bool AdminNeeded()
        {
            try
            {
                File.WriteAllText(exepath + "\\test.txt", "test");
                File.Delete(exepath + "\\test.txt");
                return false;
            }
            catch (UnauthorizedAccessException)
            {
                return true;
            }
        }

        public static bool IsAdministrator()
        {
            var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        public static bool IsScpVBusInstalled()
        {
            bool result = false;
            Guid sysGuid = Guid.Parse("{4d36e97d-e325-11ce-bfc1-08002be10318}");
            NativeMethods.SP_DEVINFO_DATA deviceInfoData = new NativeMethods.SP_DEVINFO_DATA();
            deviceInfoData.cbSize = System.Runtime.InteropServices.Marshal.SizeOf(deviceInfoData);
            var dataBuffer = new byte[4096];
            ulong propertyType = 0;
            var requiredSize = 0;
            IntPtr deviceInfoSet = NativeMethods.SetupDiGetClassDevs(ref sysGuid, null, 0, 0);
            for (int i = 0; !result && NativeMethods.SetupDiEnumDeviceInfo(deviceInfoSet, i, ref deviceInfoData); i++)
            {
                if (NativeMethods.SetupDiGetDeviceProperty(deviceInfoSet, ref deviceInfoData, ref NativeMethods.DEVPKEY_Device_HardwareIds, ref propertyType,
                    dataBuffer, dataBuffer.Length, ref requiredSize, 0))
                {
                    string hardwareId = dataBuffer.ToUTF16String();
                    //if (hardwareIds.Contains("Scp Virtual Bus Driver"))
                    //    result = true;
                    if (hardwareId.Equals(@"root\ScpVBus"))
                        result = true;
                }
            }

            if (deviceInfoSet.ToInt64() != NativeMethods.INVALID_HANDLE_VALUE)
            {
                NativeMethods.SetupDiDestroyDeviceInfoList(deviceInfoSet);
            }

            return result;
        }

        public static void FindConfigLocation()
        {
            if (File.Exists(exepath + "\\Auto Profiles.xml")
                && File.Exists(appDataPpath + "\\Auto Profiles.xml"))
            {
                Global.firstRun = true;
                Global.multisavespots = true;
            }
            else if (File.Exists(exepath + "\\Auto Profiles.xml"))
                SaveWhere(exepath);
            else if (File.Exists(appDataPpath + "\\Auto Profiles.xml"))
                SaveWhere(appDataPpath);
            else if (File.Exists(oldappdatapath + "\\Auto Profiles.xml"))
            {
                try
                {
                    if (Directory.Exists(appDataPpath))
                        Directory.Move(appDataPpath, Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\DS4Windows Old");
                    Directory.Move(oldappdatapath, appDataPpath);
                    SaveWhere(appDataPpath);
                }
                catch
                {
                    Global.oldappdatafail = true;
                }
            }
            else if (!File.Exists(exepath + "\\Auto Profiles.xml")
                && !File.Exists(appDataPpath + "\\Auto Profiles.xml"))
            {
                Global.firstRun = true;
                Global.multisavespots = false;
            }
        }

        public static void SetCulture(string culture)
        {
            try
            {
                Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(culture);
                CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.GetCultureInfo(culture);
            }
            catch { /* Skip setting culture that we cannot set */ }
        }



        public static event EventHandler<EventArgs> ControllerStatusChange; // called when a controller is added/removed/battery or touchpad mode changes/etc.
        public static void ControllerStatusChanged(object sender)
        {
            if (ControllerStatusChange != null)
                ControllerStatusChange(sender, EventArgs.Empty);
        }

        public static event EventHandler<BatteryReportArgs> BatteryStatusChange;
        public static void OnBatteryStatusChange(object sender, int index, int level, bool charging)
        {
            if (BatteryStatusChange != null)
            {
                BatteryReportArgs args = new BatteryReportArgs(index, level, charging);
                BatteryStatusChange(sender, args);
            }
        }

        public static event EventHandler<ControllerRemovedArgs> ControllerRemoved;
        public static void OnControllerRemoved(object sender, int index)
        {
            if (ControllerRemoved != null)
            {
                ControllerRemovedArgs args = new ControllerRemovedArgs(index);
                ControllerRemoved(sender, args);
            }
        }

        public static event EventHandler<DeviceStatusChangeEventArgs> DeviceStatusChange;
        public static void OnDeviceStatusChanged(object sender, int index)
        {
            if (DeviceStatusChange != null)
            {
                DeviceStatusChangeEventArgs args = new DeviceStatusChangeEventArgs(index);
                DeviceStatusChange(sender, args);
            }
        }

        public static event EventHandler<SerialChangeArgs> DeviceSerialChange;
        public static void OnDeviceSerialChange(object sender, int index, string serial)
        {
            if (DeviceSerialChange != null)
            {
                SerialChangeArgs args = new SerialChangeArgs(index, serial);
                DeviceSerialChange(sender, args);
            }
        }

        // general values
        public static bool UseExclusiveMode
        {
            set { m_Config.useExclusiveMode = value; }
            get { return m_Config.useExclusiveMode; }
        }

        public static bool getUseExclusiveMode()
        {
            return m_Config.useExclusiveMode;
        }

        public static DateTime LastChecked
        {
            set { m_Config.lastChecked = value; }
            get { return m_Config.lastChecked; }
        }

        public static int CheckWhen
        {
            set { m_Config.CheckWhen = value; }
            get { return m_Config.CheckWhen; }
        }

        public static int Notifications
        {
            set { m_Config.notifications = value; }
            get { return m_Config.notifications; }
        }

        public static bool DCBTatStop
        {
            set { m_Config.disconnectBTAtStop = value; }
            get { return m_Config.disconnectBTAtStop; }
        }

        public static bool SwipeProfiles
        {
            set { m_Config.swipeProfiles = value; }
            get { return m_Config.swipeProfiles; }
        }

        public static bool DS4Mapping
        {
            set { m_Config.ds4Mapping = value; }
            get { return m_Config.ds4Mapping; }
        }

        public static bool QuickCharge
        {
            set { m_Config.quickCharge = value; }
            get { return m_Config.quickCharge; }
        }

        public static bool getQuickCharge()
        {
            return m_Config.quickCharge;
        }

        public static int FirstXinputPort
        {
            set { m_Config.firstXinputPort = value; }
            get { return m_Config.firstXinputPort; }
        }

        public static bool CloseMini
        {
            set { m_Config.closeMini = value; }
            get { return m_Config.closeMini; }
        }

        public static bool StartMinimized
        {
            set { m_Config.startMinimized = value; }
            get { return m_Config.startMinimized; }
        }

        public static bool MinToTaskbar
        {
            set { m_Config.minToTaskbar = value; }
            get { return m_Config.minToTaskbar; }
        }

        public static int FormWidth
        {
            set { m_Config.formWidth = value; }
            get { return m_Config.formWidth; }
        }

        public static int FormHeight
        {
            set { m_Config.formHeight = value; }
            get { return m_Config.formHeight; }
        }

        public static int FormLocationX
        {
            set { m_Config.formLocationX = value; }
            get { return m_Config.formLocationX; }
        }

        public static int FormLocationY
        {
            set { m_Config.formLocationY = value; }
            get { return m_Config.formLocationY; }
        }

        public static string UseLang
        {
            set { m_Config.useLang = value; }
            get { return m_Config.useLang; }
        }

        public static bool DownloadLang
        {
            set { m_Config.downloadLang = value; }
            get { return m_Config.downloadLang; }
        }

        public static bool FlashWhenLate
        {
            set { m_Config.flashWhenLate = value; }
            get { return m_Config.flashWhenLate; }
        }

        public static bool getFlashWhenLate()
        {
            return m_Config.flashWhenLate;
        }

        public static int FlashWhenLateAt
        {
            set { m_Config.flashWhenLateAt = value; }
            get { return m_Config.flashWhenLateAt; }
        }

        public static int getFlashWhenLateAt()
        {
            return m_Config.flashWhenLateAt;
        }

        public static bool UseWhiteIcon
        {
            set { m_Config.useWhiteIcon = value; }
            get { return m_Config.useWhiteIcon; }
        }

        // controller/profile specfic values
        public static int[] ButtonMouseSensitivity => m_Config.buttonMouseSensitivity;

        public static byte[] RumbleBoost => m_Config.rumble;
        public static byte getRumbleBoost(int index)
        {
            return m_Config.rumble[index];
        }

        public static double[] Rainbow => m_Config.rainbow;
        public static double getRainbow(int index)
        {
            return m_Config.rainbow[index];
        }

        public static bool[] FlushHIDQueue => m_Config.flushHIDQueue;
        public static bool getFlushHIDQueue(int index)
        {
            return m_Config.flushHIDQueue[index];
        }

        public static bool[] EnableTouchToggle => m_Config.enableTouchToggle;
        public static bool getEnableTouchToggle(int index)
        {
            return m_Config.enableTouchToggle[index];
        }

        public static int[] IdleDisconnectTimeout => m_Config.idleDisconnectTimeout;
        public static int getIdleDisconnectTimeout(int index)
        {
            return m_Config.idleDisconnectTimeout[index];
        }

        public static byte[] TouchSensitivity => m_Config.touchSensitivity;
        public static byte[] getTouchSensitivity()
        {
            return m_Config.touchSensitivity;
        }

        public static byte getTouchSensitivity(int index)
        {
            return m_Config.touchSensitivity[index];
        }

        public static byte[] FlashType => m_Config.flashType;
        public static byte getFlashType(int index)
        {
            return m_Config.flashType[index];
        }

        public static int[] FlashAt => m_Config.flashAt;
        public static int getFlashAt(int index)
        {
            return m_Config.flashAt[index];
        }

        public static bool[] LedAsBatteryIndicator => m_Config.ledAsBattery;
        public static bool getLedAsBatteryIndicator(int index)
        {
            return m_Config.ledAsBattery[index];
        }

        public static int[] ChargingType => m_Config.chargingType;
        public static int getChargingType(int index)
        {
            return m_Config.chargingType[index];
        }

        public static bool[] DinputOnly => m_Config.dinputOnly;
        public static bool getDInputOnly(int index)
        {
            return m_Config.dinputOnly[index];
        }

        public static bool[] StartTouchpadOff => m_Config.startTouchpadOff;

        public static bool[] UseTPforControls => m_Config.useTPforControls;
        public static bool getUseTPforControls(int index)
        {
            return m_Config.useTPforControls[index];
        }

        public static bool[] UseSAforMouse => m_Config.useSAforMouse;
        public static bool isUsingSAforMouse(int index)
        {
            return m_Config.useSAforMouse[index];
        }

        public static string[] SATriggers => m_Config.sATriggers;
        public static string getSATriggers(int index)
        {
            return m_Config.sATriggers[index];
        }

        public static int[][] TouchDisInvertTriggers => m_Config.touchDisInvertTriggers;
        public static int[] getTouchDisInvertTriggers(int index)
        {
            return m_Config.touchDisInvertTriggers[index];
        }

        public static int[] GyroSensitivity => m_Config.gyroSensitivity;
        public static int getGyroSensitivity(int index)
        {
            return m_Config.gyroSensitivity[index];
        }

        public static int[] GyroSensVerticalScale => m_Config.gyroSensVerticalScale;
        public static int getGyroSensVerticalScale(int index)
        {
            return m_Config.gyroSensVerticalScale[index];
        }

        public static int[] GyroInvert => m_Config.gyroInvert;
        public static int getGyroInvert(int index)
        {
            return m_Config.gyroInvert[index];
        }

        public static bool[] GyroTriggerTurns => m_Config.gyroTriggerTurns;
        public static bool getGyroTriggerTurns(int index)
        {
            return m_Config.gyroTriggerTurns[index];
        }

        public static bool[] GyroSmoothing => m_Config.gyroSmoothing;
        public static bool getGyroSmoothing(int index)
        {
            return m_Config.gyroSmoothing[index];
        }

        public static double[] GyroSmoothingWeight => m_Config.gyroSmoothWeight;
        public static double getGyroSmoothingWeight(int index)
        {
            return m_Config.gyroSmoothWeight[index];
        }

        public static int[] GyroMouseHorizontalAxis => m_Config.gyroMouseHorizontalAxis;
        public static int getGyroMouseHorizontalAxis(int index)
        {
            return m_Config.gyroMouseHorizontalAxis[index];
        }

        public static DS4Color[] MainColor => m_Config.m_Leds;
        public static DS4Color getMainColor(int index)
        {
            return m_Config.m_Leds[index];
        }

        public static DS4Color[] LowColor => m_Config.m_LowLeds;
        public static DS4Color getLowColor(int index)
        {
            return m_Config.m_LowLeds[index];
        }

        public static DS4Color[] ChargingColor => m_Config.m_ChargingLeds;
        public static DS4Color getChargingColor(int index)
        {
            return m_Config.m_ChargingLeds[index];
        }

        public static DS4Color[] CustomColor => m_Config.m_CustomLeds;
        public static DS4Color getCustomColor(int index)
        {
            return m_Config.m_CustomLeds[index];
        }

        public static bool[] UseCustomLed => m_Config.useCustomLeds;
        public static bool getUseCustomLed(int index)
        {
            return m_Config.useCustomLeds[index];
        }

        public static DS4Color[] FlashColor => m_Config.m_FlashLeds;
        public static DS4Color getFlashColor(int index)
        {
            return m_Config.m_FlashLeds[index];
        }

        public static byte[] TapSensitivity => m_Config.tapSensitivity;
        public static byte getTapSensitivity(int index)
        {
            return m_Config.tapSensitivity[index];
        }

        public static bool[] DoubleTap => m_Config.doubleTap;
        public static bool getDoubleTap(int index)
        {
            return m_Config.doubleTap[index];
        }

        public static int[] ScrollSensitivity => m_Config.scrollSensitivity;
        public static int[] getScrollSensitivity()
        {
            return m_Config.scrollSensitivity;
        }
        public static int getScrollSensitivity(int index)
        {
            return m_Config.scrollSensitivity[index];
        }

        public static bool[] LowerRCOn => m_Config.lowerRCOn;
        public static bool[] TouchpadJitterCompensation => m_Config.touchpadJitterCompensation;
        public static bool getTouchpadJitterCompensation(int index)
        {
            return m_Config.touchpadJitterCompensation[index];
        }

        public static int[] TouchpadInvert => m_Config.touchpadInvert;
        public static int getTouchpadInvert(int index)
        {
            return m_Config.touchpadInvert[index];
        }

        public static byte[] L2Deadzone => m_Config.l2Deadzone;
        public static byte getL2Deadzone(int index)
        {
            return m_Config.l2Deadzone[index];
        }

        public static byte[] R2Deadzone => m_Config.r2Deadzone;
        public static byte getR2Deadzone(int index)
        {
            return m_Config.r2Deadzone[index];
        }

        public static double[] SXDeadzone => m_Config.SXDeadzone;
        public static double getSXDeadzone(int index)
        {
            return m_Config.SXDeadzone[index];
        }

        public static double[] SZDeadzone => m_Config.SZDeadzone;
        public static double getSZDeadzone(int index)
        {
            return m_Config.SZDeadzone[index];
        }

        public static int[] LSDeadzone => m_Config.LSDeadzone;
        public static int getLSDeadzone(int index)
        {
            return m_Config.LSDeadzone[index];
        }

        public static int[] RSDeadzone => m_Config.RSDeadzone;
        public static int getRSDeadzone(int index)
        {
            return m_Config.RSDeadzone[index];
        }

        public static int[] LSAntiDeadzone => m_Config.LSAntiDeadzone;
        public static int getLSAntiDeadzone(int index)
        {
            return m_Config.LSAntiDeadzone[index];
        }

        public static int[] RSAntiDeadzone => m_Config.RSAntiDeadzone;
        public static int getRSAntiDeadzone(int index)
        {
            return m_Config.RSAntiDeadzone[index];
        }

        public static double[] SXAntiDeadzone => m_Config.SXAntiDeadzone;
        public static double getSXAntiDeadzone(int index)
        {
            return m_Config.SXAntiDeadzone[index];
        }

        public static double[] SZAntiDeadzone => m_Config.SZAntiDeadzone;
        public static double getSZAntiDeadzone(int index)
        {
            return m_Config.SZAntiDeadzone[index];
        }

        public static int[] LSMaxzone => m_Config.LSMaxzone;
        public static int getLSMaxzone(int index)
        {
            return m_Config.LSMaxzone[index];
        }

        public static int[] RSMaxzone => m_Config.RSMaxzone;
        public static int getRSMaxzone(int index)
        {
            return m_Config.RSMaxzone[index];
        }

        public static double[] SXMaxzone => m_Config.SXMaxzone;
        public static double getSXMaxzone(int index)
        {
            return m_Config.SXMaxzone[index];
        }

        public static double[] SZMaxzone => m_Config.SZMaxzone;
        public static double getSZMaxzone(int index)
        {
            return m_Config.SZMaxzone[index];
        }

        public static int[] L2AntiDeadzone => m_Config.l2AntiDeadzone;
        public static int getL2AntiDeadzone(int index)
        {
            return m_Config.l2AntiDeadzone[index];
        }

        public static int[] R2AntiDeadzone => m_Config.r2AntiDeadzone;
        public static int getR2AntiDeadzone(int index)
        {
            return m_Config.r2AntiDeadzone[index];
        }

        public static int[] L2Maxzone => m_Config.l2Maxzone;
        public static int getL2Maxzone(int index)
        {
            return m_Config.l2Maxzone[index];
        }

        public static int[] R2Maxzone => m_Config.r2Maxzone;
        public static int getR2Maxzone(int index)
        {
            return m_Config.r2Maxzone[index];
        }

        public static int[] LSCurve => m_Config.lsCurve;
        public static int getLSCurve(int index)
        {
            return m_Config.lsCurve[index];
        }

        public static int[] RSCurve => m_Config.rsCurve;
        public static int getRSCurve(int index)
        {
            return m_Config.rsCurve[index];
        }

        public static double[] LSRotation => m_Config.LSRotation;
        public static double getLSRotation(int index)
        {
            return m_Config.LSRotation[index];
        }

        public static double[] RSRotation => m_Config.RSRotation;
        public static double getRSRotation(int index)
        {
            return m_Config.RSRotation[index];
        }

        public static double[] L2Sens => m_Config.l2Sens;
        public static double getL2Sens(int index)
        {
            return m_Config.l2Sens[index];
        }

        public static double[] R2Sens => m_Config.r2Sens;
        public static double getR2Sens(int index)
        {
            return m_Config.r2Sens[index];
        }

        public static double[] SXSens => m_Config.SXSens;
        public static double getSXSens(int index)
        {
            return m_Config.SXSens[index];
        }

        public static double[] SZSens => m_Config.SZSens;
        public static double getSZSens(int index)
        {
            return m_Config.SZSens[index];
        }

        public static double[] LSSens => m_Config.LSSens;
        public static double getLSSens(int index)
        {
            return m_Config.LSSens[index];
        }

        public static double[] RSSens => m_Config.RSSens;
        public static double getRSSens(int index)
        {
            return m_Config.RSSens[index];
        }

        public static bool[] MouseAccel => m_Config.mouseAccel;
        public static bool getMouseAccel(int device)
        {
            return m_Config.mouseAccel[device];
        }

        public static int[] BTPollRate => m_Config.btPollRate;
        public static int getBTPollRate(int index)
        {
            return m_Config.btPollRate[index];
        }

        public static int[] lsOutCurveMode => m_Config.lsOutCurveMode;
        public static int getLsOutCurveMode(int index)
        {
            return m_Config.lsOutCurveMode[index];
        }

        public static int[] rsOutCurveMode => m_Config.rsOutCurveMode;
        public static int getRsOutCurveMode(int index)
        {
            return m_Config.rsOutCurveMode[index];
        }

        public static int[] l2OutCurveMode => m_Config.l2OutCurveMode;
        public static int getL2OutCurveMode(int index)
        {
            return m_Config.l2OutCurveMode[index];
        }

        public static int[] r2OutCurveMode => m_Config.r2OutCurveMode;
        public static int getR2OutCurveMode(int index)
        {
            return m_Config.r2OutCurveMode[index];
        }

        public static int[] sxOutCurveMode => m_Config.sxOutCurveMode;
        public static int getSXOutCurveMode(int index)
        {
            return m_Config.sxOutCurveMode[index];
        }

        public static int[] szOutCurveMode => m_Config.szOutCurveMode;
        public static int getSZOutCurveMode(int index)
        {
            return m_Config.szOutCurveMode[index];
        }

        public static bool[] TrackballMode => m_Config.trackballMode;
        public static bool getTrackballMode(int index)
        {
            return m_Config.trackballMode[index];
        }

        public static double[] TrackballFriction => m_Config.trackballFriction;
        public static double getTrackballFriction(int index)
        {
            return m_Config.trackballFriction[index];
        }

        public static string[] LaunchProgram => m_Config.launchProgram;
        public static string[] ProfilePath => m_Config.profilePath;
        public static string[] OlderProfilePath => m_Config.olderProfilePath;
        public static bool[] DistanceProfiles = m_Config.distanceProfiles;

        public static List<string>[] ProfileActions => m_Config.profileActions;
        public static int getProfileActionCount(int index)
        {
            return m_Config.profileActionCount[index];
        }

        public static void calculateProfileActionCount(int index)
        {
            m_Config.profileActionCount[index] = m_Config.profileActions[index].Count;
        }

        public static List<string> getProfileActions(int index)
        {
            return m_Config.profileActions[index];
        }
        
        public static void UpdateDS4CSetting (int deviceNum, string buttonName, bool shift, object action, string exts, DS4KeyType kt, int trigger = 0)
        {
            m_Config.UpdateDS4CSetting(deviceNum, buttonName, shift, action, exts, kt, trigger);
            m_Config.containsCustomAction[deviceNum] = m_Config.HasCustomActions(deviceNum);
            m_Config.containsCustomExtras[deviceNum] = m_Config.HasCustomExtras(deviceNum);
        }

        public static void UpdateDS4Extra(int deviceNum, string buttonName, bool shift, string exts)
        {
            m_Config.UpdateDS4CExtra(deviceNum, buttonName, shift, exts);
            m_Config.containsCustomAction[deviceNum] = m_Config.HasCustomActions(deviceNum);
            m_Config.containsCustomExtras[deviceNum] = m_Config.HasCustomExtras(deviceNum);
        }

        public static object GetDS4Action(int deviceNum, string buttonName, bool shift) => m_Config.GetDS4Action(deviceNum, buttonName, shift);
        public static object GetDS4Action(int deviceNum, DS4Controls control, bool shift) => m_Config.GetDS4Action(deviceNum, control, shift);
        public static DS4KeyType GetDS4KeyType(int deviceNum, string buttonName, bool shift) => m_Config.GetDS4KeyType(deviceNum, buttonName, shift);
        public static string GetDS4Extra(int deviceNum, string buttonName, bool shift) => m_Config.GetDS4Extra(deviceNum, buttonName, shift);
        public static int GetDS4STrigger(int deviceNum, string buttonName) => m_Config.GetDS4STrigger(deviceNum, buttonName);
        public static int GetDS4STrigger(int deviceNum, DS4Controls control) => m_Config.GetDS4STrigger(deviceNum, control);
        public static List<DS4ControlSettings> getDS4CSettings(int device) => m_Config.ds4settings[device];
        public static DS4ControlSettings getDS4CSetting(int deviceNum, string control) => m_Config.getDS4CSetting(deviceNum, control);
        public static DS4ControlSettings getDS4CSetting(int deviceNum, DS4Controls control) => m_Config.getDS4CSetting(deviceNum, control);
        public static bool HasCustomActions(int deviceNum) => m_Config.HasCustomActions(deviceNum);
        public static bool HasCustomExtras(int deviceNum) => m_Config.HasCustomExtras(deviceNum);

        public static bool containsCustomAction(int deviceNum)
        {
            return m_Config.containsCustomAction[deviceNum];
        }

        public static bool containsCustomExtras(int deviceNum)
        {
            return m_Config.containsCustomExtras[deviceNum];
        }

        public static void SaveAction(string name, string controls, int mode,
            string details, bool edit, string extras = "")
        {
            m_Config.SaveAction(name, controls, mode, details, edit, extras);
            Mapping.actionDone.Add(new Mapping.ActionState());
        }

        public static void RemoveAction(string name)
        {
            m_Config.RemoveAction(name);
        }

        public static bool LoadActions() => m_Config.LoadActions();

        public static List<SpecialAction> GetActions() => m_Config.actions;

        public static int GetActionIndexOf(string name)
        {
            for (int i = 0, actionCount = m_Config.actions.Count; i < actionCount; i++)
            {
                if (m_Config.actions[i].name == name)
                    return i;
            }

            return -1;
        }

        public static int GetProfileActionIndexOf(int device, string name)
        {
            int index = -1;
            m_Config.profileActionIndexDict[device].TryGetValue(name, out index);
            return index;
        }

        public static SpecialAction GetAction(string name)
        {
            //foreach (SpecialAction sA in m_Config.actions)
            for (int i=0, actionCount = m_Config.actions.Count; i < actionCount; i++)
            {
                SpecialAction sA = m_Config.actions[i];
                if (sA.name == name)
                    return sA;
            }

            return new SpecialAction("null", "null", "null", "null");
        }

        public static SpecialAction GetProfileAction(int device, string name)
        {
            SpecialAction sA = null;
            m_Config.profileActionDict[device].TryGetValue(name, out sA);
            return sA;
        }

        public static void calculateProfileActionDicts(int device)
        {
            m_Config.profileActionDict[device].Clear();
            m_Config.profileActionIndexDict[device].Clear();

            foreach (string actionname in m_Config.profileActions[device])
            {
                m_Config.profileActionDict[device][actionname] = GetAction(actionname);
                m_Config.profileActionIndexDict[device][actionname] = GetActionIndexOf(actionname);
            }
        }

        public static void cacheProfileCustomsFlags(int device)
        {
            m_Config.containsCustomAction[device] = HasCustomActions(device);
            m_Config.containsCustomExtras[device] = HasCustomExtras(device);
        }

        public static X360Controls getX360ControlsByName(string key)
        {
            return m_Config.getX360ControlsByName(key);
        }

        public static string getX360ControlString(X360Controls key)
        {
            return m_Config.getX360ControlString(key);
        }

        public static DS4Controls getDS4ControlsByName(string key)
        {
            return m_Config.getDS4ControlsByName(key);
        }

        public static X360Controls getDefaultX360ControlBinding(DS4Controls dc)
        {
            return defaultButtonMapping[(int)dc];
        }

        public static bool containsLinkedProfile(string serial)
        {
            string tempSerial = serial.Replace(":", string.Empty);
            return m_Config.linkedProfiles.ContainsKey(tempSerial);
        }

        public static string getLinkedProfile(string serial)
        {
            string temp = string.Empty;
            string tempSerial = serial.Replace(":", string.Empty);
            if (m_Config.linkedProfiles.ContainsKey(tempSerial))
            {
                temp = m_Config.linkedProfiles[tempSerial];
            }

            return temp;
        }

        public static void changeLinkedProfile(string serial, string profile)
        {
            string tempSerial = serial.Replace(":", string.Empty);
            m_Config.linkedProfiles[tempSerial] = profile;
        }

        public static void removeLinkedProfile(string serial)
        {
            string tempSerial = serial.Replace(":", string.Empty);
            if (m_Config.linkedProfiles.ContainsKey(tempSerial))
            {
                m_Config.linkedProfiles.Remove(tempSerial);
            }
        }

        public static bool Load() => m_Config.Load();
        
        public static void LoadProfile(int device, bool launchprogram, ControlService control,
            bool xinputChange = true, bool postLoad = true)
        {
            m_Config.LoadProfile(device, launchprogram, control, "", xinputChange, postLoad);
            tempprofilename[device] = string.Empty;
            tempprofileDistance[device] = false;
        }

        public static void LoadTempProfile(int device, string name, bool launchprogram,
            ControlService control, bool xinputChange = true)
        {
            m_Config.LoadProfile(device, launchprogram, control, appdatapath + @"\Profiles\" + name + ".xml");
            tempprofilename[device] = name;
            tempprofileDistance[device] = name.ToLower().Contains("distance");
        }

        public static bool Save()
        {
            return m_Config.Save();
        }

        public static void SaveProfile(int device, string propath)
        {
            m_Config.SaveProfile(device, propath);
        }

        public static bool SaveLinkedProfiles()
        {
            return m_Config.SaveLinkedProfiles();
        }

        public static bool LoadLinkedProfiles()
        {
            return m_Config.LoadLinkedProfiles();
        }

        private static byte applyRatio(byte b1, byte b2, double r)
        {
            if (r > 100.0)
                r = 100.0;
            else if (r < 0.0)
                r = 0.0;

            r /= 100.0;
            return (byte)Math.Round((b1 * (1 - r) + b2 * r), 0);
        }

        public static DS4Color getTransitionedColor(DS4Color c1, DS4Color c2, double ratio)
        {
            //Color cs = Color.FromArgb(c1.red, c1.green, c1.blue);
            c1.red = applyRatio(c1.red, c2.red, ratio);
            c1.green = applyRatio(c1.green, c2.green, ratio);
            c1.blue = applyRatio(c1.blue, c2.blue, ratio);
            return c1;
        }

        private static Color applyRatio(Color c1, Color c2, uint r)
        {
            float ratio = r / 100f;
            float hue1 = c1.GetHue();
            float hue2 = c2.GetHue();
            float bri1 = c1.GetBrightness();
            float bri2 = c2.GetBrightness();
            float sat1 = c1.GetSaturation();
            float sat2 = c2.GetSaturation();
            float hr = hue2 - hue1;
            float br = bri2 - bri1;
            float sr = sat2 - sat1;
            Color csR;
            if (bri1 == 0)
                csR = HuetoRGB(hue2,sat2,bri2 - br*ratio);
            else
                csR = HuetoRGB(hue2 - hr * ratio, sat2 - sr * ratio, bri2 - br * ratio);

            return csR;
        }

        public static Color HuetoRGB(float hue, float sat, float bri)
        {
            float C = (1-Math.Abs(2*bri)-1)* sat;
            float X = C * (1 - Math.Abs((hue / 60) % 2 - 1));
            float m = bri - C / 2;
            float R, G, B;
            if (0 <= hue && hue < 60)
            {
                R = C; G = X; B = 0;
            }
            else if (60 <= hue && hue < 120)
            {
                R = X; G = C; B = 0;
            }
            else if (120 <= hue && hue < 180)
            {
                R = 0; G = C; B = X;
            }
            else if (180 <= hue && hue < 240)
            {
                R = 0; G = X; B = C;
            }
            else if (240 <= hue && hue < 300)
            {
                R = X; G = 0; B = C;
            }
            else if (300 <= hue && hue < 360)
            {
                R = C; G = 0; B = X;
            }
            else
            {
                R = 255; G = 0; B = 0;
            }

            R += m; G += m; B += m;
            R *= 255.0f; G *= 255.0f; B *= 255.0f;
            return Color.FromArgb((int)R, (int)G, (int)B);
        }

        public static double Clamp(double min, double value, double max)
        {
            return (value < min) ? min : (value > max) ? max : value;
        }

        private static int ClampInt(int min, int value, int max)
        {
            return (value < min) ? min : (value > max) ? max : value;
        }
    }

    public class BackingStore
    {
        //public String m_Profile = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\DS4Tool" + "\\Profiles.xml";
        public String m_Profile = Directory.GetParent(Assembly.GetExecutingAssembly().Location).FullName + "\\Profiles.xml";
        public String m_Actions = Global.appdatapath + "\\Actions.xml";
        public string m_linkedProfiles = Global.appdatapath + "\\LinkedProfiles.xml";

        protected XmlDocument m_Xdoc = new XmlDocument();
        // fifth value used for options, not fifth controller
        public int[] buttonMouseSensitivity = new int[5] { 25, 25, 25, 25, 25 };

        public bool[] flushHIDQueue = new bool[5] { false, false, false, false, false };
        public bool[] enableTouchToggle = new bool[5] { true, true, true, true, true };
        public int[] idleDisconnectTimeout = new int[5] { 0, 0, 0, 0, 0 };
        public bool[] touchpadJitterCompensation = new bool[5] { true, true, true, true, true };
        public bool[] lowerRCOn = new bool[5] { false, false, false, false, false };
        public bool[] ledAsBattery = new bool[5] { false, false, false, false, false };
        public byte[] flashType = new byte[5] { 0, 0, 0, 0, 0 };
        public string[] profilePath = new string[5] { string.Empty, string.Empty, string.Empty, string.Empty, string.Empty };
        public string[] olderProfilePath = new string[5] { string.Empty, string.Empty, string.Empty, string.Empty, string.Empty };
        public Dictionary<string, string> linkedProfiles = new Dictionary<string, string>();
        // Cache properties instead of performing a string comparison every frame
        public bool[] distanceProfiles = new bool[5] { false, false, false, false, false };
        public Byte[] rumble = new Byte[5] { 100, 100, 100, 100, 100 };
        public Byte[] touchSensitivity = new Byte[5] { 100, 100, 100, 100, 100 };
        public Byte[] l2Deadzone = new Byte[5] { 0, 0, 0, 0, 0 }, r2Deadzone = new Byte[5] { 0, 0, 0, 0, 0 };
        public int[] LSDeadzone = new int[5] { 0, 0, 0, 0, 0 }, RSDeadzone = new int[5] { 0, 0, 0, 0, 0 };
        public int[] LSAntiDeadzone = new int[5] { 0, 0, 0, 0, 0 }, RSAntiDeadzone = new int[5] { 0, 0, 0, 0, 0 };
        public int[] LSMaxzone = new int[5] { 100, 100, 100, 100, 100 }, RSMaxzone = new int[5] { 100, 100, 100, 100, 100 };
        public int[] l2AntiDeadzone = new int[5] { 0, 0, 0, 0, 0 }, r2AntiDeadzone = new int[5] { 0, 0, 0, 0, 0 };
        public int[] l2Maxzone = new int[5] { 100, 100, 100, 100, 100 }, r2Maxzone = new int[5] { 100, 100, 100, 100, 100 };
        public double[] LSRotation = new double[5] { 0.0, 0.0, 0.0, 0.0, 0.0 }, RSRotation = new double[5] { 0.0, 0.0, 0.0, 0.0, 0.0 };
        public double[] SXDeadzone = new double[5] { 0.02, 0.02, 0.02, 0.02, 0.02 }, SZDeadzone = new double[5] { 0.02, 0.02, 0.02, 0.02, 0.02 };
        public double[] SXMaxzone = new double[5] { 1.0, 1.0, 1.0, 1.0, 1.0 },
            SZMaxzone = new double[5] { 1.0, 1.0, 1.0, 1.0, 1.0 };
        public double[] SXAntiDeadzone = new double[5] { 0.0, 0.0, 0.0, 0.0, 0.0 },
            SZAntiDeadzone = new double[5] { 0.0, 0.0, 0.0, 0.0, 0.0 };
        public double[] l2Sens = new double[5] { 1.0, 1.0, 1.0, 1.0, 1.0 }, r2Sens = new double[5] { 1.0, 1.0, 1.0, 1.0, 1.0 };
        public double[] LSSens = new double[5] { 1.0, 1.0, 1.0, 1.0, 1.0 }, RSSens = new double[5] { 1.0, 1.0, 1.0, 1.0, 1.0 };
        public double[] SXSens = new double[5] { 1.0, 1.0, 1.0, 1.0, 1.0 }, SZSens = new double[5] { 1.0, 1.0, 1.0, 1.0, 1.0 };
        public Byte[] tapSensitivity = new Byte[5] { 0, 0, 0, 0, 0 };
        public bool[] doubleTap = new bool[5] { false, false, false, false, false };
        public int[] scrollSensitivity = new int[5] { 0, 0, 0, 0, 0 };
        public int[] touchpadInvert = new int[5] { 0, 0, 0, 0, 0 };
        public double[] rainbow = new double[5] { 0.0, 0.0, 0.0, 0.0, 0.0 };
        public int[] flashAt = new int[5] { 0, 0, 0, 0, 0 };
        public bool[] mouseAccel = new bool[5] { true, true, true, true, true };
        public int[] btPollRate = new int[5] { 4, 4, 4, 4, 4 };
        public int[] lsOutCurveMode = new int[5] { 0, 0, 0, 0, 0 };
        public int[] rsOutCurveMode = new int[5] { 0, 0, 0, 0, 0 };
        public int[] l2OutCurveMode = new int[5] { 0, 0, 0, 0, 0 };
        public int[] r2OutCurveMode = new int[5] { 0, 0, 0, 0, 0 };
        public int[] sxOutCurveMode = new int[5] { 0, 0, 0, 0, 0 };
        public int[] szOutCurveMode = new int[5] { 0, 0, 0, 0, 0 };

        public DS4Color[] m_LowLeds = new DS4Color[5]
        {
            new DS4Color(Color.Black),
            new DS4Color(Color.Black),
            new DS4Color(Color.Black),
            new DS4Color(Color.Black),
            new DS4Color(Color.Black)
        };
        public DS4Color[] m_Leds = new DS4Color[5]
        {
            new DS4Color(Color.Blue),
            new DS4Color(Color.Red),
            new DS4Color(Color.Green),
            new DS4Color(Color.Pink),
            new DS4Color(Color.White)
        };
        public DS4Color[] m_ChargingLeds = new DS4Color[5]
        {
            new DS4Color(Color.Black),
            new DS4Color(Color.Black),
            new DS4Color(Color.Black),
            new DS4Color(Color.Black),
            new DS4Color(Color.Black)
        };
        public DS4Color[] m_FlashLeds = new DS4Color[5]
        {
            new DS4Color(Color.Black),
            new DS4Color(Color.Black),
            new DS4Color(Color.Black),
            new DS4Color(Color.Black),
            new DS4Color(Color.Black)
        };
        public bool[] useCustomLeds = new bool[5] { false, false, false, false, false };
        public DS4Color[] m_CustomLeds = new DS4Color[5]
        {
            new DS4Color(Color.Blue),
            new DS4Color(Color.Blue),
            new DS4Color(Color.Blue),
            new DS4Color(Color.Blue),
            new DS4Color(Color.Blue)
        };

        public int[] chargingType = new int[5] { 0, 0, 0, 0, 0 };
        public string[] launchProgram = new string[5] { string.Empty, string.Empty, string.Empty, string.Empty, string.Empty };
        public bool[] dinputOnly = new bool[5] { false, false, false, false, false };
        public bool[] startTouchpadOff = new bool[5] { false, false, false, false, false };
        public bool[] useTPforControls = new bool[5] { false, false, false, false, false };
        public bool[] useSAforMouse = new bool[5] { false, false, false, false, false };
        public string[] sATriggers = new string[5] { string.Empty, string.Empty, string.Empty, string.Empty, string.Empty };
        public int[][] touchDisInvertTriggers = new int[5][] { new int[1] { -1 }, new int[1] { -1 }, new int[1] { -1 },
            new int[1] { -1 }, new int[1] { -1 } };
        public int[] lsCurve = new int[5] { 0, 0, 0, 0, 0 };
        public int[] rsCurve = new int[5] { 0, 0, 0, 0, 0 };
        public Boolean useExclusiveMode = false;
        public Int32 formWidth = 782;
        public Int32 formHeight = 550;
        public int formLocationX = 0;
        public int formLocationY = 0;
        public Boolean startMinimized = false;
        public Boolean minToTaskbar = false;
        public DateTime lastChecked;
        public int CheckWhen = 1;
        public int notifications = 2;
        public bool disconnectBTAtStop = false;
        public bool swipeProfiles = true;
        public bool ds4Mapping = true;
        public bool quickCharge = false;
        public int firstXinputPort = 1;
        public bool closeMini = false;
        public List<SpecialAction> actions = new List<SpecialAction>();
        public List<DS4ControlSettings>[] ds4settings = new List<DS4ControlSettings>[5]
            { new List<DS4ControlSettings>(), new List<DS4ControlSettings>(), new List<DS4ControlSettings>(),
              new List<DS4ControlSettings>(), new List<DS4ControlSettings>() };

        public List<string>[] profileActions = new List<string>[5] { null, null, null, null, null };
        public int[] profileActionCount = new int[5] { 0, 0, 0, 0, 0 };
        public Dictionary<string, SpecialAction>[] profileActionDict = new Dictionary<string, SpecialAction>[5]
            { new Dictionary<string, SpecialAction>(), new Dictionary<string, SpecialAction>(), new Dictionary<string, SpecialAction>(),
              new Dictionary<string, SpecialAction>(), new Dictionary<string, SpecialAction>() };

        public Dictionary<string, int>[] profileActionIndexDict = new Dictionary<string, int>[5]
            { new Dictionary<string, int>(), new Dictionary<string, int>(), new Dictionary<string, int>(),
              new Dictionary<string, int>(), new Dictionary<string, int>() };

        public string useLang = "";
        public bool downloadLang = true;
        public bool useWhiteIcon;
        public bool flashWhenLate = true;
        public int flashWhenLateAt = 20;
        // Cache whether profile has custom action
        public bool[] containsCustomAction = new bool[5] { false, false, false, false, false };

        // Cache whether profile has custom extras
        public bool[] containsCustomExtras = new bool[5] { false, false, false, false, false };

        public int[] gyroSensitivity = new int[5] { 100, 100, 100, 100, 100 };
        public int[] gyroSensVerticalScale = new int[5] { 100, 100, 100, 100, 100 };
        public int[] gyroInvert = new int[5] { 0, 0, 0, 0, 0 };
        public bool[] gyroTriggerTurns = new bool[5] { true, true, true, true, true };
        public bool[] gyroSmoothing = new bool[5] { false, false, false, false, false };
        public double[] gyroSmoothWeight = new double[5] { 0.5, 0.5, 0.5, 0.5, 0.5 };
        public int[] gyroMouseHorizontalAxis = new int[5] { 0, 0, 0, 0, 0 };
        public bool[] trackballMode = new bool[5] { false, false, false, false, false };
        public double[] trackballFriction = new double[5] { 10.0, 10.0, 10.0, 10.0, 10.0 };

        bool tempBool = false;

        public BackingStore()
        {
            for (int i = 0; i < 5; i++)
            {
                foreach (DS4Controls dc in Enum.GetValues(typeof(DS4Controls)))
                {
                    if (dc != DS4Controls.None)
                        ds4settings[i].Add(new DS4ControlSettings(dc));
                }

                profileActions[i] = new List<string>();
                profileActions[i].Add("Disconnect Controller");
                profileActionCount[i] = profileActions[i].Count;
            }
        }

        private string stickOutputCurveString(int id)
        {
            string result = "linear";
            switch (id)
            {
                case 0: break;
                case 1: result = "enhanced-precision"; break;
                case 2: result = "quadratic"; break;
                case 3: result = "cubic"; break;
                default: break;
            }

            return result;
        }

        private int stickOutputCurveId(string name)
        {
            int id = 0;
            switch (name)
            {
                case "linear": id = 0; break;
                case "enhanced-precision": id = 1; break;
                case "quadratic": id = 2; break;
                case "cubic": id = 3; break;
                default: break;
            }

            return id;
        }

        private string axisOutputCurveString(int id)
        {
            string result = "linear";
            switch (id)
            {
                case 0: break;
                case 1: result = "quadratic"; break;
                case 2: result = "cubic"; break;
                default: break;
            }

            return result;
        }

        private int axisOutputCurveId(string name)
        {
            int id = 0;
            switch (name)
            {
                case "linear": id = 0; break;
                case "quadratic": id = 1; break;
                case "cubic": id = 2; break;
                default: break;
            }

            return id;
        }

        public bool SaveProfile(int device, string propath)
        {
            bool Saved = true;
            string path = Global.appdatapath + @"\Profiles\" + Path.GetFileNameWithoutExtension(propath) + ".xml";
            try
            {
                XmlNode Node;
                XmlNode xmlControls = m_Xdoc.SelectSingleNode("/DS4Windows/Control");
                XmlNode xmlShiftControls = m_Xdoc.SelectSingleNode("/DS4Windows/ShiftControl");
                m_Xdoc.RemoveAll();

                Node = m_Xdoc.CreateXmlDeclaration("1.0", "utf-8", string.Empty);
                m_Xdoc.AppendChild(Node);

                Node = m_Xdoc.CreateComment(string.Format(" DS4Windows Configuration Data. {0} ", DateTime.Now));
                m_Xdoc.AppendChild(Node);

                Node = m_Xdoc.CreateWhitespace("\r\n");
                m_Xdoc.AppendChild(Node);

                Node = m_Xdoc.CreateNode(XmlNodeType.Element, "DS4Windows", null);

                XmlNode xmlFlushHIDQueue = m_Xdoc.CreateNode(XmlNodeType.Element, "flushHIDQueue", null); xmlFlushHIDQueue.InnerText = flushHIDQueue[device].ToString(); Node.AppendChild(xmlFlushHIDQueue);
                XmlNode xmlTouchToggle = m_Xdoc.CreateNode(XmlNodeType.Element, "touchToggle", null); xmlTouchToggle.InnerText = enableTouchToggle[device].ToString(); Node.AppendChild(xmlTouchToggle);
                XmlNode xmlIdleDisconnectTimeout = m_Xdoc.CreateNode(XmlNodeType.Element, "idleDisconnectTimeout", null); xmlIdleDisconnectTimeout.InnerText = idleDisconnectTimeout[device].ToString(); Node.AppendChild(xmlIdleDisconnectTimeout);
                XmlNode xmlColor = m_Xdoc.CreateNode(XmlNodeType.Element, "Color", null);
                xmlColor.InnerText = m_Leds[device].red.ToString() + "," + m_Leds[device].green.ToString() + "," + m_Leds[device].blue.ToString();
                Node.AppendChild(xmlColor);
                XmlNode xmlRumbleBoost = m_Xdoc.CreateNode(XmlNodeType.Element, "RumbleBoost", null); xmlRumbleBoost.InnerText = rumble[device].ToString(); Node.AppendChild(xmlRumbleBoost);
                XmlNode xmlLedAsBatteryIndicator = m_Xdoc.CreateNode(XmlNodeType.Element, "ledAsBatteryIndicator", null); xmlLedAsBatteryIndicator.InnerText = ledAsBattery[device].ToString(); Node.AppendChild(xmlLedAsBatteryIndicator);
                XmlNode xmlLowBatteryFlash = m_Xdoc.CreateNode(XmlNodeType.Element, "FlashType", null); xmlLowBatteryFlash.InnerText = flashType[device].ToString(); Node.AppendChild(xmlLowBatteryFlash);
                XmlNode xmlFlashBatterAt = m_Xdoc.CreateNode(XmlNodeType.Element, "flashBatteryAt", null); xmlFlashBatterAt.InnerText = flashAt[device].ToString(); Node.AppendChild(xmlFlashBatterAt);
                XmlNode xmlTouchSensitivity = m_Xdoc.CreateNode(XmlNodeType.Element, "touchSensitivity", null); xmlTouchSensitivity.InnerText = touchSensitivity[device].ToString(); Node.AppendChild(xmlTouchSensitivity);
                XmlNode xmlLowColor = m_Xdoc.CreateNode(XmlNodeType.Element, "LowColor", null);
                xmlLowColor.InnerText = m_LowLeds[device].red.ToString() + "," + m_LowLeds[device].green.ToString() + "," + m_LowLeds[device].blue.ToString();
                Node.AppendChild(xmlLowColor);
                XmlNode xmlChargingColor = m_Xdoc.CreateNode(XmlNodeType.Element, "ChargingColor", null);
                xmlChargingColor.InnerText = m_ChargingLeds[device].red.ToString() + "," + m_ChargingLeds[device].green.ToString() + "," + m_ChargingLeds[device].blue.ToString();
                Node.AppendChild(xmlChargingColor);
                XmlNode xmlFlashColor = m_Xdoc.CreateNode(XmlNodeType.Element, "FlashColor", null);
                xmlFlashColor.InnerText = m_FlashLeds[device].red.ToString() + "," + m_FlashLeds[device].green.ToString() + "," + m_FlashLeds[device].blue.ToString();
                Node.AppendChild(xmlFlashColor);
                XmlNode xmlTouchpadJitterCompensation = m_Xdoc.CreateNode(XmlNodeType.Element, "touchpadJitterCompensation", null); xmlTouchpadJitterCompensation.InnerText = touchpadJitterCompensation[device].ToString(); Node.AppendChild(xmlTouchpadJitterCompensation);
                XmlNode xmlLowerRCOn = m_Xdoc.CreateNode(XmlNodeType.Element, "lowerRCOn", null); xmlLowerRCOn.InnerText = lowerRCOn[device].ToString(); Node.AppendChild(xmlLowerRCOn);
                XmlNode xmlTapSensitivity = m_Xdoc.CreateNode(XmlNodeType.Element, "tapSensitivity", null); xmlTapSensitivity.InnerText = tapSensitivity[device].ToString(); Node.AppendChild(xmlTapSensitivity);
                XmlNode xmlDouble = m_Xdoc.CreateNode(XmlNodeType.Element, "doubleTap", null); xmlDouble.InnerText = doubleTap[device].ToString(); Node.AppendChild(xmlDouble);
                XmlNode xmlScrollSensitivity = m_Xdoc.CreateNode(XmlNodeType.Element, "scrollSensitivity", null); xmlScrollSensitivity.InnerText = scrollSensitivity[device].ToString(); Node.AppendChild(xmlScrollSensitivity);
                XmlNode xmlLeftTriggerMiddle = m_Xdoc.CreateNode(XmlNodeType.Element, "LeftTriggerMiddle", null); xmlLeftTriggerMiddle.InnerText = l2Deadzone[device].ToString(); Node.AppendChild(xmlLeftTriggerMiddle);
                XmlNode xmlRightTriggerMiddle = m_Xdoc.CreateNode(XmlNodeType.Element, "RightTriggerMiddle", null); xmlRightTriggerMiddle.InnerText = r2Deadzone[device].ToString(); Node.AppendChild(xmlRightTriggerMiddle);
                XmlNode xmlTouchpadInvert = m_Xdoc.CreateNode(XmlNodeType.Element, "TouchpadInvert", null); xmlTouchpadInvert.InnerText = touchpadInvert[device].ToString(); Node.AppendChild(xmlTouchpadInvert);
                XmlNode xmlL2AD = m_Xdoc.CreateNode(XmlNodeType.Element, "L2AntiDeadZone", null); xmlL2AD.InnerText = l2AntiDeadzone[device].ToString(); Node.AppendChild(xmlL2AD);
                XmlNode xmlR2AD = m_Xdoc.CreateNode(XmlNodeType.Element, "R2AntiDeadZone", null); xmlR2AD.InnerText = r2AntiDeadzone[device].ToString(); Node.AppendChild(xmlR2AD);
                XmlNode xmlL2Maxzone = m_Xdoc.CreateNode(XmlNodeType.Element, "L2MaxZone", null); xmlL2Maxzone.InnerText = l2Maxzone[device].ToString(); Node.AppendChild(xmlL2Maxzone);
                XmlNode xmlR2Maxzone = m_Xdoc.CreateNode(XmlNodeType.Element, "R2MaxZone", null); xmlR2Maxzone.InnerText = r2Maxzone[device].ToString(); Node.AppendChild(xmlR2Maxzone);
                XmlNode xmlButtonMouseSensitivity = m_Xdoc.CreateNode(XmlNodeType.Element, "ButtonMouseSensitivity", null); xmlButtonMouseSensitivity.InnerText = buttonMouseSensitivity[device].ToString(); Node.AppendChild(xmlButtonMouseSensitivity);
                XmlNode xmlRainbow = m_Xdoc.CreateNode(XmlNodeType.Element, "Rainbow", null); xmlRainbow.InnerText = rainbow[device].ToString(); Node.AppendChild(xmlRainbow);
                XmlNode xmlLSD = m_Xdoc.CreateNode(XmlNodeType.Element, "LSDeadZone", null); xmlLSD.InnerText = LSDeadzone[device].ToString(); Node.AppendChild(xmlLSD);
                XmlNode xmlRSD = m_Xdoc.CreateNode(XmlNodeType.Element, "RSDeadZone", null); xmlRSD.InnerText = RSDeadzone[device].ToString(); Node.AppendChild(xmlRSD);
                XmlNode xmlLSAD = m_Xdoc.CreateNode(XmlNodeType.Element, "LSAntiDeadZone", null); xmlLSAD.InnerText = LSAntiDeadzone[device].ToString(); Node.AppendChild(xmlLSAD);
                XmlNode xmlRSAD = m_Xdoc.CreateNode(XmlNodeType.Element, "RSAntiDeadZone", null); xmlRSAD.InnerText = RSAntiDeadzone[device].ToString(); Node.AppendChild(xmlRSAD);
                XmlNode xmlLSMaxZone = m_Xdoc.CreateNode(XmlNodeType.Element, "LSMaxZone", null); xmlLSMaxZone.InnerText = LSMaxzone[device].ToString(); Node.AppendChild(xmlLSMaxZone);
                XmlNode xmlRSMaxZone = m_Xdoc.CreateNode(XmlNodeType.Element, "RSMaxZone", null); xmlRSMaxZone.InnerText = RSMaxzone[device].ToString(); Node.AppendChild(xmlRSMaxZone);
                XmlNode xmlLSRotation = m_Xdoc.CreateNode(XmlNodeType.Element, "LSRotation", null); xmlLSRotation.InnerText = Convert.ToInt32(LSRotation[device] * 180.0 / Math.PI).ToString(); Node.AppendChild(xmlLSRotation);
                XmlNode xmlRSRotation = m_Xdoc.CreateNode(XmlNodeType.Element, "RSRotation", null); xmlRSRotation.InnerText = Convert.ToInt32(RSRotation[device] * 180.0 / Math.PI).ToString(); Node.AppendChild(xmlRSRotation);

                XmlNode xmlSXD = m_Xdoc.CreateNode(XmlNodeType.Element, "SXDeadZone", null); xmlSXD.InnerText = SXDeadzone[device].ToString(); Node.AppendChild(xmlSXD);
                XmlNode xmlSZD = m_Xdoc.CreateNode(XmlNodeType.Element, "SZDeadZone", null); xmlSZD.InnerText = SZDeadzone[device].ToString(); Node.AppendChild(xmlSZD);

                XmlNode xmlSXMaxzone = m_Xdoc.CreateNode(XmlNodeType.Element, "SXMaxZone", null); xmlSXMaxzone.InnerText = Convert.ToInt32(SXMaxzone[device] * 100.0).ToString(); Node.AppendChild(xmlSXMaxzone);
                XmlNode xmlSZMaxzone = m_Xdoc.CreateNode(XmlNodeType.Element, "SZMaxZone", null); xmlSZMaxzone.InnerText = Convert.ToInt32(SZMaxzone[device] * 100.0).ToString(); Node.AppendChild(xmlSZMaxzone);

                XmlNode xmlSXAntiDeadzone = m_Xdoc.CreateNode(XmlNodeType.Element, "SXAntiDeadZone", null); xmlSXAntiDeadzone.InnerText = Convert.ToInt32(SXAntiDeadzone[device] * 100.0).ToString(); Node.AppendChild(xmlSXAntiDeadzone);
                XmlNode xmlSZAntiDeadzone = m_Xdoc.CreateNode(XmlNodeType.Element, "SZAntiDeadZone", null); xmlSZAntiDeadzone.InnerText = Convert.ToInt32(SZAntiDeadzone[device] * 100.0).ToString(); Node.AppendChild(xmlSZAntiDeadzone);

                XmlNode xmlSens = m_Xdoc.CreateNode(XmlNodeType.Element, "Sensitivity", null);
                xmlSens.InnerText = $"{LSSens[device]}|{RSSens[device]}|{l2Sens[device]}|{r2Sens[device]}|{SXSens[device]}|{SZSens[device]}";
                Node.AppendChild(xmlSens);

                XmlNode xmlChargingType = m_Xdoc.CreateNode(XmlNodeType.Element, "ChargingType", null); xmlChargingType.InnerText = chargingType[device].ToString(); Node.AppendChild(xmlChargingType);
                XmlNode xmlMouseAccel = m_Xdoc.CreateNode(XmlNodeType.Element, "MouseAcceleration", null); xmlMouseAccel.InnerText = mouseAccel[device].ToString(); Node.AppendChild(xmlMouseAccel);
                //XmlNode xmlShiftMod = m_Xdoc.CreateNode(XmlNodeType.Element, "ShiftModifier", null); xmlShiftMod.InnerText = shiftModifier[device].ToString(); Node.AppendChild(xmlShiftMod);
                XmlNode xmlLaunchProgram = m_Xdoc.CreateNode(XmlNodeType.Element, "LaunchProgram", null); xmlLaunchProgram.InnerText = launchProgram[device].ToString(); Node.AppendChild(xmlLaunchProgram);
                XmlNode xmlDinput = m_Xdoc.CreateNode(XmlNodeType.Element, "DinputOnly", null); xmlDinput.InnerText = dinputOnly[device].ToString(); Node.AppendChild(xmlDinput);
                XmlNode xmlStartTouchpadOff = m_Xdoc.CreateNode(XmlNodeType.Element, "StartTouchpadOff", null); xmlStartTouchpadOff.InnerText = startTouchpadOff[device].ToString(); Node.AppendChild(xmlStartTouchpadOff);
                XmlNode xmlUseTPforControls = m_Xdoc.CreateNode(XmlNodeType.Element, "UseTPforControls", null); xmlUseTPforControls.InnerText = useTPforControls[device].ToString(); Node.AppendChild(xmlUseTPforControls);
                XmlNode xmlUseSAforMouse = m_Xdoc.CreateNode(XmlNodeType.Element, "UseSAforMouse", null); xmlUseSAforMouse.InnerText = useSAforMouse[device].ToString(); Node.AppendChild(xmlUseSAforMouse);
                XmlNode xmlSATriggers = m_Xdoc.CreateNode(XmlNodeType.Element, "SATriggers", null); xmlSATriggers.InnerText = sATriggers[device].ToString(); Node.AppendChild(xmlSATriggers);

                XmlNode xmlTouchDisInvTriggers = m_Xdoc.CreateNode(XmlNodeType.Element, "TouchDisInvTriggers", null);
                string tempTouchDisInv = string.Join(",", touchDisInvertTriggers[device]);
                xmlTouchDisInvTriggers.InnerText = tempTouchDisInv;
                Node.AppendChild(xmlTouchDisInvTriggers);

                XmlNode xmlGyroSensitivity = m_Xdoc.CreateNode(XmlNodeType.Element, "GyroSensitivity", null); xmlGyroSensitivity.InnerText = gyroSensitivity[device].ToString(); Node.AppendChild(xmlGyroSensitivity);
                XmlNode xmlGyroSensVerticalScale = m_Xdoc.CreateNode(XmlNodeType.Element, "GyroSensVerticalScale", null); xmlGyroSensVerticalScale.InnerText = gyroSensVerticalScale[device].ToString(); Node.AppendChild(xmlGyroSensVerticalScale);
                XmlNode xmlGyroInvert = m_Xdoc.CreateNode(XmlNodeType.Element, "GyroInvert", null); xmlGyroInvert.InnerText = gyroInvert[device].ToString(); Node.AppendChild(xmlGyroInvert);
                XmlNode xmlGyroTriggerTurns = m_Xdoc.CreateNode(XmlNodeType.Element, "GyroTriggerTurns", null); xmlGyroTriggerTurns.InnerText = gyroTriggerTurns[device].ToString(); Node.AppendChild(xmlGyroTriggerTurns);
                XmlNode xmlGyroSmoothWeight = m_Xdoc.CreateNode(XmlNodeType.Element, "GyroSmoothingWeight", null); xmlGyroSmoothWeight.InnerText = Convert.ToInt32(gyroSmoothWeight[device] * 100).ToString(); Node.AppendChild(xmlGyroSmoothWeight);
                XmlNode xmlGyroSmoothing = m_Xdoc.CreateNode(XmlNodeType.Element, "GyroSmoothing", null); xmlGyroSmoothing.InnerText = gyroSmoothing[device].ToString(); Node.AppendChild(xmlGyroSmoothing);
                XmlNode xmlGyroMouseHAxis = m_Xdoc.CreateNode(XmlNodeType.Element, "GyroMouseHAxis", null); xmlGyroMouseHAxis.InnerText = gyroMouseHorizontalAxis[device].ToString(); Node.AppendChild(xmlGyroMouseHAxis);
                XmlNode xmlLSC = m_Xdoc.CreateNode(XmlNodeType.Element, "LSCurve", null); xmlLSC.InnerText = lsCurve[device].ToString(); Node.AppendChild(xmlLSC);
                XmlNode xmlRSC = m_Xdoc.CreateNode(XmlNodeType.Element, "RSCurve", null); xmlRSC.InnerText = rsCurve[device].ToString(); Node.AppendChild(xmlRSC);
                XmlNode xmlProfileActions = m_Xdoc.CreateNode(XmlNodeType.Element, "ProfileActions", null); xmlProfileActions.InnerText = string.Join("/", profileActions[device]); Node.AppendChild(xmlProfileActions);
                XmlNode xmlBTPollRate = m_Xdoc.CreateNode(XmlNodeType.Element, "BTPollRate", null); xmlBTPollRate.InnerText = btPollRate[device].ToString(); Node.AppendChild(xmlBTPollRate);
                XmlNode xmlLsOutputCurveMode = m_Xdoc.CreateNode(XmlNodeType.Element, "LSOutputCurveMode", null); xmlLsOutputCurveMode.InnerText = stickOutputCurveString(lsOutCurveMode[device]); Node.AppendChild(xmlLsOutputCurveMode);
                XmlNode xmlRsOutputCurveMode = m_Xdoc.CreateNode(XmlNodeType.Element, "RSOutputCurveMode", null); xmlRsOutputCurveMode.InnerText = stickOutputCurveString(rsOutCurveMode[device]); Node.AppendChild(xmlRsOutputCurveMode);

                XmlNode xmlL2OutputCurveMode = m_Xdoc.CreateNode(XmlNodeType.Element, "L2OutputCurveMode", null); xmlL2OutputCurveMode.InnerText = axisOutputCurveString(l2OutCurveMode[device]); Node.AppendChild(xmlL2OutputCurveMode);
                XmlNode xmlR2OutputCurveMode = m_Xdoc.CreateNode(XmlNodeType.Element, "R2OutputCurveMode", null); xmlR2OutputCurveMode.InnerText = axisOutputCurveString(r2OutCurveMode[device]); Node.AppendChild(xmlR2OutputCurveMode);

                XmlNode xmlSXOutputCurveMode = m_Xdoc.CreateNode(XmlNodeType.Element, "SXOutputCurveMode", null); xmlSXOutputCurveMode.InnerText = axisOutputCurveString(sxOutCurveMode[device]); Node.AppendChild(xmlSXOutputCurveMode);
                XmlNode xmlSZOutputCurveMode = m_Xdoc.CreateNode(XmlNodeType.Element, "SZOutputCurveMode", null); xmlSZOutputCurveMode.InnerText = axisOutputCurveString(szOutCurveMode[device]); Node.AppendChild(xmlSZOutputCurveMode);

                XmlNode xmlTrackBallMode = m_Xdoc.CreateNode(XmlNodeType.Element, "TrackballMode", null); xmlTrackBallMode.InnerText = trackballMode[device].ToString(); Node.AppendChild(xmlTrackBallMode);
                XmlNode xmlTrackBallFriction = m_Xdoc.CreateNode(XmlNodeType.Element, "TrackballFriction", null); xmlTrackBallFriction.InnerText = trackballFriction[device].ToString(); Node.AppendChild(xmlTrackBallFriction);

                XmlNode NodeControl = m_Xdoc.CreateNode(XmlNodeType.Element, "Control", null);
                XmlNode Key = m_Xdoc.CreateNode(XmlNodeType.Element, "Key", null);
                XmlNode Macro = m_Xdoc.CreateNode(XmlNodeType.Element, "Macro", null);
                XmlNode KeyType = m_Xdoc.CreateNode(XmlNodeType.Element, "KeyType", null);
                XmlNode Button = m_Xdoc.CreateNode(XmlNodeType.Element, "Button", null);
                XmlNode Extras = m_Xdoc.CreateNode(XmlNodeType.Element, "Extras", null);

                XmlNode NodeShiftControl = m_Xdoc.CreateNode(XmlNodeType.Element, "ShiftControl", null);

                XmlNode ShiftKey = m_Xdoc.CreateNode(XmlNodeType.Element, "Key", null);
                XmlNode ShiftMacro = m_Xdoc.CreateNode(XmlNodeType.Element, "Macro", null);
                XmlNode ShiftKeyType = m_Xdoc.CreateNode(XmlNodeType.Element, "KeyType", null);
                XmlNode ShiftButton = m_Xdoc.CreateNode(XmlNodeType.Element, "Button", null);
                XmlNode ShiftExtras = m_Xdoc.CreateNode(XmlNodeType.Element, "Extras", null);

                foreach (DS4ControlSettings dcs in ds4settings[device])
                {
                    if (dcs.action != null)
                    {
                        XmlNode buttonNode;
                        string keyType = string.Empty;

                        if (dcs.action is string)
                        {
                            if (dcs.action.ToString() == "Unbound")
                                keyType += DS4KeyType.Unbound;
                        }

                        if (dcs.keyType.HasFlag(DS4KeyType.HoldMacro))
                            keyType += DS4KeyType.HoldMacro;
                        else if (dcs.keyType.HasFlag(DS4KeyType.Macro))
                            keyType += DS4KeyType.Macro;

                        if (dcs.keyType.HasFlag(DS4KeyType.Toggle))
                            keyType += DS4KeyType.Toggle;
                        if (dcs.keyType.HasFlag(DS4KeyType.ScanCode))
                            keyType += DS4KeyType.ScanCode;

                        if (keyType != string.Empty)
                        {
                            buttonNode = m_Xdoc.CreateNode(XmlNodeType.Element, dcs.control.ToString(), null);
                            buttonNode.InnerText = keyType;
                            KeyType.AppendChild(buttonNode);
                        }

                        buttonNode = m_Xdoc.CreateNode(XmlNodeType.Element, dcs.control.ToString(), null);
                        if (dcs.action is IEnumerable<int> || dcs.action is int[] || dcs.action is ushort[])
                        {
                            int[] ii = (int[])dcs.action;
                            buttonNode.InnerText = string.Join("/", ii);
                            Macro.AppendChild(buttonNode);
                        }
                        else if (dcs.action is int || dcs.action is ushort || dcs.action is byte)
                        {
                            buttonNode.InnerText = dcs.action.ToString();
                            Key.AppendChild(buttonNode);
                        }
                        else if (dcs.action is string)
                        {
                            buttonNode.InnerText = dcs.action.ToString();
                            Button.AppendChild(buttonNode);
                        }
                        else if (dcs.action is X360Controls)
                        {
                            buttonNode.InnerText = getX360ControlString((X360Controls)dcs.action);
                            Button.AppendChild(buttonNode);
                        }
                    }

                    bool hasvalue = false;
                    if (!string.IsNullOrEmpty(dcs.extras))
                    {
                        foreach (string s in dcs.extras.Split(','))
                        {
                            if (s != "0")
                            {
                                hasvalue = true;
                                break;
                            }
                        }
                    }

                    if (hasvalue)
                    {
                        XmlNode extraNode = m_Xdoc.CreateNode(XmlNodeType.Element, dcs.control.ToString(), null);
                        extraNode.InnerText = dcs.extras;
                        Extras.AppendChild(extraNode);
                    }

                    if (dcs.shiftAction != null && dcs.shiftTrigger > 0)
                    {
                        XmlElement buttonNode;
                        string keyType = string.Empty;

                        if (dcs.shiftAction is string)
                        {
                            if (dcs.shiftAction.ToString() == "Unbound")
                                keyType += DS4KeyType.Unbound;
                        }

                        if (dcs.shiftKeyType.HasFlag(DS4KeyType.HoldMacro))
                            keyType += DS4KeyType.HoldMacro;
                        if (dcs.shiftKeyType.HasFlag(DS4KeyType.Macro))
                            keyType += DS4KeyType.Macro;
                        if (dcs.shiftKeyType.HasFlag(DS4KeyType.Toggle))
                            keyType += DS4KeyType.Toggle;
                        if (dcs.shiftKeyType.HasFlag(DS4KeyType.ScanCode))
                            keyType += DS4KeyType.ScanCode;

                        if (keyType != string.Empty)
                        {
                            buttonNode = m_Xdoc.CreateElement(dcs.control.ToString());
                            buttonNode.InnerText = keyType;
                            ShiftKeyType.AppendChild(buttonNode);
                        }

                        buttonNode = m_Xdoc.CreateElement(dcs.control.ToString());
                        buttonNode.SetAttribute("Trigger", dcs.shiftTrigger.ToString());
                        if (dcs.shiftAction is IEnumerable<int> || dcs.shiftAction is int[] || dcs.shiftAction is ushort[])
                        {
                            int[] ii = (int[])dcs.shiftAction;
                            buttonNode.InnerText = string.Join("/", ii);
                            ShiftMacro.AppendChild(buttonNode);
                        }
                        else if (dcs.shiftAction is int || dcs.shiftAction is ushort || dcs.shiftAction is byte)
                        {
                            buttonNode.InnerText = dcs.shiftAction.ToString();
                            ShiftKey.AppendChild(buttonNode);
                        }
                        else if (dcs.shiftAction is string || dcs.shiftAction is X360Controls)
                        {
                            buttonNode.InnerText = dcs.shiftAction.ToString();
                            ShiftButton.AppendChild(buttonNode);
                        }
                    }

                    hasvalue = false;
                    if (!string.IsNullOrEmpty(dcs.shiftExtras))
                    {
                        foreach (string s in dcs.shiftExtras.Split(','))
                        {
                            if (s != "0")
                            {
                                hasvalue = true;
                                break;
                            }
                        }
                    }

                    if (hasvalue)
                    {
                        XmlNode extraNode = m_Xdoc.CreateNode(XmlNodeType.Element, dcs.control.ToString(), null);
                        extraNode.InnerText = dcs.shiftExtras;
                        ShiftExtras.AppendChild(extraNode);
                    }
                }

                Node.AppendChild(NodeControl);
                if (Button.HasChildNodes)
                    NodeControl.AppendChild(Button);
                if (Macro.HasChildNodes)
                    NodeControl.AppendChild(Macro);
                if (Key.HasChildNodes)
                    NodeControl.AppendChild(Key);
                if (Extras.HasChildNodes)
                    NodeControl.AppendChild(Extras);
                if (KeyType.HasChildNodes)
                    NodeControl.AppendChild(KeyType);
                if (NodeControl.HasChildNodes)
                    Node.AppendChild(NodeControl);

                Node.AppendChild(NodeShiftControl);
                if (ShiftButton.HasChildNodes)
                    NodeShiftControl.AppendChild(ShiftButton);
                if (ShiftMacro.HasChildNodes)
                    NodeShiftControl.AppendChild(ShiftMacro);
                if (ShiftKey.HasChildNodes)
                    NodeShiftControl.AppendChild(ShiftKey);
                if (ShiftKeyType.HasChildNodes)
                    NodeShiftControl.AppendChild(ShiftKeyType);
                if (ShiftExtras.HasChildNodes)
                    NodeShiftControl.AppendChild(ShiftExtras);
                
                m_Xdoc.AppendChild(Node);
                m_Xdoc.Save(path);
            }
            catch { Saved = false; }
            return Saved;
        }

        public DS4Controls getDS4ControlsByName(string key)
        {
            if (!key.StartsWith("bn"))
                return (DS4Controls)Enum.Parse(typeof(DS4Controls), key, true);

            switch (key)
            {
                case "bnShare": return DS4Controls.Share;
                case "bnL3": return DS4Controls.L3;
                case "bnR3": return DS4Controls.R3;
                case "bnOptions": return DS4Controls.Options;
                case "bnUp": return DS4Controls.DpadUp;
                case "bnRight": return DS4Controls.DpadRight;
                case "bnDown": return DS4Controls.DpadDown;
                case "bnLeft": return DS4Controls.DpadLeft;

                case "bnL1": return DS4Controls.L1;
                case "bnR1": return DS4Controls.R1;
                case "bnTriangle": return DS4Controls.Triangle;
                case "bnCircle": return DS4Controls.Circle;
                case "bnCross": return DS4Controls.Cross;
                case "bnSquare": return DS4Controls.Square;

                case "bnPS": return DS4Controls.PS;
                case "bnLSLeft": return DS4Controls.LXNeg;
                case "bnLSUp": return DS4Controls.LYNeg;
                case "bnRSLeft": return DS4Controls.RXNeg;
                case "bnRSUp": return DS4Controls.RYNeg;

                case "bnLSRight": return DS4Controls.LXPos;
                case "bnLSDown": return DS4Controls.LYPos;
                case "bnRSRight": return DS4Controls.RXPos;
                case "bnRSDown": return DS4Controls.RYPos;
                case "bnL2": return DS4Controls.L2;
                case "bnR2": return DS4Controls.R2;

                case "bnTouchLeft": return DS4Controls.TouchLeft;
                case "bnTouchMulti": return DS4Controls.TouchMulti;
                case "bnTouchUpper": return DS4Controls.TouchUpper;
                case "bnTouchRight": return DS4Controls.TouchRight;
                case "bnGyroXP": return DS4Controls.GyroXPos;
                case "bnGyroXN": return DS4Controls.GyroXNeg;
                case "bnGyroZP": return DS4Controls.GyroZPos;
                case "bnGyroZN": return DS4Controls.GyroZNeg;

                case "bnSwipeUp": return DS4Controls.SwipeUp;
                case "bnSwipeDown": return DS4Controls.SwipeDown;
                case "bnSwipeLeft": return DS4Controls.SwipeLeft;
                case "bnSwipeRight": return DS4Controls.SwipeRight;

                #region OldShiftname
                case "sbnShare": return DS4Controls.Share;
                case "sbnL3": return DS4Controls.L3;
                case "sbnR3": return DS4Controls.R3;
                case "sbnOptions": return DS4Controls.Options;
                case "sbnUp": return DS4Controls.DpadUp;
                case "sbnRight": return DS4Controls.DpadRight;
                case "sbnDown": return DS4Controls.DpadDown;
                case "sbnLeft": return DS4Controls.DpadLeft;

                case "sbnL1": return DS4Controls.L1;
                case "sbnR1": return DS4Controls.R1;
                case "sbnTriangle": return DS4Controls.Triangle;
                case "sbnCircle": return DS4Controls.Circle;
                case "sbnCross": return DS4Controls.Cross;
                case "sbnSquare": return DS4Controls.Square;

                case "sbnPS": return DS4Controls.PS;
                case "sbnLSLeft": return DS4Controls.LXNeg;
                case "sbnLSUp": return DS4Controls.LYNeg;
                case "sbnRSLeft": return DS4Controls.RXNeg;
                case "sbnRSUp": return DS4Controls.RYNeg;

                case "sbnLSRight": return DS4Controls.LXPos;
                case "sbnLSDown": return DS4Controls.LYPos;
                case "sbnRSRight": return DS4Controls.RXPos;
                case "sbnRSDown": return DS4Controls.RYPos;
                case "sbnL2": return DS4Controls.L2;
                case "sbnR2": return DS4Controls.R2;

                case "sbnTouchLeft": return DS4Controls.TouchLeft;
                case "sbnTouchMulti": return DS4Controls.TouchMulti;
                case "sbnTouchUpper": return DS4Controls.TouchUpper;
                case "sbnTouchRight": return DS4Controls.TouchRight;
                case "sbnGsyroXP": return DS4Controls.GyroXPos;
                case "sbnGyroXN": return DS4Controls.GyroXNeg;
                case "sbnGyroZP": return DS4Controls.GyroZPos;
                case "sbnGyroZN": return DS4Controls.GyroZNeg;
                #endregion

                case "bnShiftShare": return DS4Controls.Share;
                case "bnShiftL3": return DS4Controls.L3;
                case "bnShiftR3": return DS4Controls.R3;
                case "bnShiftOptions": return DS4Controls.Options;
                case "bnShiftUp": return DS4Controls.DpadUp;
                case "bnShiftRight": return DS4Controls.DpadRight;
                case "bnShiftDown": return DS4Controls.DpadDown;
                case "bnShiftLeft": return DS4Controls.DpadLeft;

                case "bnShiftL1": return DS4Controls.L1;
                case "bnShiftR1": return DS4Controls.R1;
                case "bnShiftTriangle": return DS4Controls.Triangle;
                case "bnShiftCircle": return DS4Controls.Circle;
                case "bnShiftCross": return DS4Controls.Cross;
                case "bnShiftSquare": return DS4Controls.Square;

                case "bnShiftPS": return DS4Controls.PS;
                case "bnShiftLSLeft": return DS4Controls.LXNeg;
                case "bnShiftLSUp": return DS4Controls.LYNeg;
                case "bnShiftRSLeft": return DS4Controls.RXNeg;
                case "bnShiftRSUp": return DS4Controls.RYNeg;

                case "bnShiftLSRight": return DS4Controls.LXPos;
                case "bnShiftLSDown": return DS4Controls.LYPos;
                case "bnShiftRSRight": return DS4Controls.RXPos;
                case "bnShiftRSDown": return DS4Controls.RYPos;
                case "bnShiftL2": return DS4Controls.L2;
                case "bnShiftR2": return DS4Controls.R2;

                case "bnShiftTouchLeft": return DS4Controls.TouchLeft;
                case "bnShiftTouchMulti": return DS4Controls.TouchMulti;
                case "bnShiftTouchUpper": return DS4Controls.TouchUpper;
                case "bnShiftTouchRight": return DS4Controls.TouchRight;
                case "bnShiftGyroXP": return DS4Controls.GyroXPos;
                case "bnShiftGyroXN": return DS4Controls.GyroXNeg;
                case "bnShiftGyroZP": return DS4Controls.GyroZPos;
                case "bnShiftGyroZN": return DS4Controls.GyroZNeg;

                case "bnShiftSwipeUp": return DS4Controls.SwipeUp;
                case "bnShiftSwipeDown": return DS4Controls.SwipeDown;
                case "bnShiftSwipeLeft": return DS4Controls.SwipeLeft;
                case "bnShiftSwipeRight": return DS4Controls.SwipeRight;
            }

            return 0;
        }

        public X360Controls getX360ControlsByName(string key)
        {
            X360Controls x3c;
            if (Enum.TryParse(key, true, out x3c))
                return x3c;

            switch (key)
            {
                case "Back": return X360Controls.Back;
                case "Left Stick": return X360Controls.LS;
                case "Right Stick": return X360Controls.RS;
                case "Start": return X360Controls.Start;
                case "Up Button": return X360Controls.DpadUp;
                case "Right Button": return X360Controls.DpadRight;
                case "Down Button": return X360Controls.DpadDown;
                case "Left Button": return X360Controls.DpadLeft;

                case "Left Bumper": return X360Controls.LB;
                case "Right Bumper": return X360Controls.RB;
                case "Y Button": return X360Controls.Y;
                case "B Button": return X360Controls.B;
                case "A Button": return X360Controls.A;
                case "X Button": return X360Controls.X;

                case "Guide": return X360Controls.Guide;
                case "Left X-Axis-": return X360Controls.LXNeg;
                case "Left Y-Axis-": return X360Controls.LYNeg;
                case "Right X-Axis-": return X360Controls.RXNeg;
                case "Right Y-Axis-": return X360Controls.RYNeg;

                case "Left X-Axis+": return X360Controls.LXPos;
                case "Left Y-Axis+": return X360Controls.LYPos;
                case "Right X-Axis+": return X360Controls.RXPos;
                case "Right Y-Axis+": return X360Controls.RYPos;
                case "Left Trigger": return X360Controls.LT;
                case "Right Trigger": return X360Controls.RT;

                case "Left Mouse Button": return X360Controls.LeftMouse;
                case "Right Mouse Button": return X360Controls.RightMouse;
                case "Middle Mouse Button": return X360Controls.MiddleMouse;
                case "4th Mouse Button": return X360Controls.FourthMouse;
                case "5th Mouse Button": return X360Controls.FifthMouse;
                case "Mouse Wheel Up": return X360Controls.WUP;
                case "Mouse Wheel Down": return X360Controls.WDOWN;
                case "Mouse Up": return X360Controls.MouseUp;
                case "Mouse Down": return X360Controls.MouseDown;
                case "Mouse Left": return X360Controls.MouseLeft;
                case "Mouse Right": return X360Controls.MouseRight;
                case "Unbound": return X360Controls.Unbound;
            }

            return X360Controls.Unbound;
        }

        public string getX360ControlString(X360Controls key)
        {
            switch (key)
            {
                case X360Controls.Back: return "Back";
                case X360Controls.LS: return "Left Stick";
                case X360Controls.RS: return "Right Stick";
                case X360Controls.Start: return "Start";
                case X360Controls.DpadUp: return "Up Button";
                case X360Controls.DpadRight: return "Right Button";
                case X360Controls.DpadDown: return "Down Button";
                case X360Controls.DpadLeft: return "Left Button";

                case X360Controls.LB: return "Left Bumper";
                case X360Controls.RB: return "Right Bumper";
                case X360Controls.Y: return "Y Button";
                case X360Controls.B: return "B Button";
                case X360Controls.A: return "A Button";
                case X360Controls.X: return "X Button";

                case X360Controls.Guide: return "Guide";
                case X360Controls.LXNeg: return "Left X-Axis-";
                case X360Controls.LYNeg: return "Left Y-Axis-";
                case X360Controls.RXNeg: return "Right X-Axis-";
                case X360Controls.RYNeg: return "Right Y-Axis-";

                case X360Controls.LXPos: return "Left X-Axis+";
                case X360Controls.LYPos: return "Left Y-Axis+";
                case X360Controls.RXPos: return "Right X-Axis+";
                case X360Controls.RYPos: return "Right Y-Axis+";
                case X360Controls.LT: return "Left Trigger";
                case X360Controls.RT: return "Right Trigger";

                case X360Controls.LeftMouse: return "Left Mouse Button";
                case X360Controls.RightMouse: return "Right Mouse Button";
                case X360Controls.MiddleMouse: return "Middle Mouse Button";
                case X360Controls.FourthMouse: return "4th Mouse Button";
                case X360Controls.FifthMouse: return "5th Mouse Button";
                case X360Controls.WUP: return "Mouse Wheel Up";
                case X360Controls.WDOWN: return "Mouse Wheel Down";
                case X360Controls.MouseUp: return "Mouse Up";
                case X360Controls.MouseDown: return "Mouse Down";
                case X360Controls.MouseLeft: return "Mouse Left";
                case X360Controls.MouseRight: return "Mouse Right";
                case X360Controls.Unbound: return "Unbound";
            }

            return "Unbound";
        }

        public bool LoadProfile(int device, bool launchprogram, ControlService control,
            string propath = "", bool xinputChange = true, bool postLoad = true)
        {
            bool Loaded = true;
            Dictionary<DS4Controls, DS4KeyType> customMapKeyTypes = new Dictionary<DS4Controls, DS4KeyType>();
            Dictionary<DS4Controls, UInt16> customMapKeys = new Dictionary<DS4Controls, UInt16>();
            Dictionary<DS4Controls, X360Controls> customMapButtons = new Dictionary<DS4Controls, X360Controls>();
            Dictionary<DS4Controls, String> customMapMacros = new Dictionary<DS4Controls, String>();
            Dictionary<DS4Controls, String> customMapExtras = new Dictionary<DS4Controls, String>();
            Dictionary<DS4Controls, DS4KeyType> shiftCustomMapKeyTypes = new Dictionary<DS4Controls, DS4KeyType>();
            Dictionary<DS4Controls, UInt16> shiftCustomMapKeys = new Dictionary<DS4Controls, UInt16>();
            Dictionary<DS4Controls, X360Controls> shiftCustomMapButtons = new Dictionary<DS4Controls, X360Controls>();
            Dictionary<DS4Controls, String> shiftCustomMapMacros = new Dictionary<DS4Controls, String>();
            Dictionary<DS4Controls, String> shiftCustomMapExtras = new Dictionary<DS4Controls, String>();
            string rootname = "DS4Windows";
            bool missingSetting = false;
            string profilepath;
            if (propath == "")
                profilepath = Global.appdatapath + @"\Profiles\" + profilePath[device] + ".xml";
            else
                profilepath = propath;

            bool xinputPlug = false;
            bool xinputStatus = false;

            if (File.Exists(profilepath))
            {
                XmlNode Item;

                m_Xdoc.Load(profilepath);
                if (m_Xdoc.SelectSingleNode(rootname) == null)
                {
                    rootname = "ScpControl";
                    missingSetting = true;
                }

                if (device < 4)
                {
                    DS4LightBar.forcelight[device] = false;
                    DS4LightBar.forcedFlash[device] = 0;
                }

                // Make sure to reset currently set profile values before parsing
                ResetProfile(device);

                try { Item = m_Xdoc.SelectSingleNode("/" + rootname + "/flushHIDQueue"); Boolean.TryParse(Item.InnerText, out flushHIDQueue[device]); }
                catch { missingSetting = true; }//rootname = }

                try { Item = m_Xdoc.SelectSingleNode("/" + rootname + "/touchToggle"); Boolean.TryParse(Item.InnerText, out enableTouchToggle[device]); }
                catch { missingSetting = true; }

                try { Item = m_Xdoc.SelectSingleNode("/" + rootname + "/idleDisconnectTimeout"); Int32.TryParse(Item.InnerText, out idleDisconnectTimeout[device]); }
                catch { missingSetting = true; }
                //New method for saving color
                try
                {
                    Item = m_Xdoc.SelectSingleNode("/" + rootname + "/Color");
                    string[] colors;
                    if (!string.IsNullOrEmpty(Item.InnerText))
                        colors = Item.InnerText.Split(',');
                    else
                        colors = new string[0];

                    m_Leds[device].red = byte.Parse(colors[0]);
                    m_Leds[device].green = byte.Parse(colors[1]);
                    m_Leds[device].blue = byte.Parse(colors[2]);
                }
                catch { missingSetting = true; }

                if (m_Xdoc.SelectSingleNode("/" + rootname + "/Color") == null)
                {
                    //Old method of color saving
                    try { Item = m_Xdoc.SelectSingleNode("/" + rootname + "/Red"); Byte.TryParse(Item.InnerText, out m_Leds[device].red); }
                    catch { missingSetting = true; }
                    try { Item = m_Xdoc.SelectSingleNode("/" + rootname + "/Green"); Byte.TryParse(Item.InnerText, out m_Leds[device].green); }
                    catch { missingSetting = true; }

                    try { Item = m_Xdoc.SelectSingleNode("/" + rootname + "/Blue"); Byte.TryParse(Item.InnerText, out m_Leds[device].blue); }
                    catch { missingSetting = true; }
                }

                try { Item = m_Xdoc.SelectSingleNode("/" + rootname + "/RumbleBoost"); Byte.TryParse(Item.InnerText, out rumble[device]); }
                catch { missingSetting = true; }

                try { Item = m_Xdoc.SelectSingleNode("/" + rootname + "/ledAsBatteryIndicator"); Boolean.TryParse(Item.InnerText, out ledAsBattery[device]); }
                catch { missingSetting = true; }

                try { Item = m_Xdoc.SelectSingleNode("/" + rootname + "/FlashType"); Byte.TryParse(Item.InnerText, out flashType[device]); }
                catch { missingSetting = true; }

                try { Item = m_Xdoc.SelectSingleNode("/" + rootname + "/flashBatteryAt"); Int32.TryParse(Item.InnerText, out flashAt[device]); }
                catch { missingSetting = true; }

                try { Item = m_Xdoc.SelectSingleNode("/" + rootname + "/touchSensitivity"); Byte.TryParse(Item.InnerText, out touchSensitivity[device]); }
                catch { missingSetting = true; }

                //New method for saving color
                try
                {
                    Item = m_Xdoc.SelectSingleNode("/" + rootname + "/LowColor");
                    string[] colors;
                    if (!string.IsNullOrEmpty(Item.InnerText))
                        colors = Item.InnerText.Split(',');
                    else
                        colors = new string[0];

                    m_LowLeds[device].red = byte.Parse(colors[0]);
                    m_LowLeds[device].green = byte.Parse(colors[1]);
                    m_LowLeds[device].blue = byte.Parse(colors[2]);
                }
                catch { missingSetting = true; }

                if (m_Xdoc.SelectSingleNode("/" + rootname + "/LowColor") == null)
                {
                    //Old method of color saving
                    try { Item = m_Xdoc.SelectSingleNode("/" + rootname + "/LowRed"); byte.TryParse(Item.InnerText, out m_LowLeds[device].red); }
                    catch { missingSetting = true; }
                    try { Item = m_Xdoc.SelectSingleNode("/" + rootname + "/LowGreen"); byte.TryParse(Item.InnerText, out m_LowLeds[device].green); }
                    catch { missingSetting = true; }
                    try { Item = m_Xdoc.SelectSingleNode("/" + rootname + "/LowBlue"); byte.TryParse(Item.InnerText, out m_LowLeds[device].blue); }
                    catch { missingSetting = true; }
                }

                //New method for saving color
                try
                {
                    Item = m_Xdoc.SelectSingleNode("/" + rootname + "/ChargingColor");
                    string[] colors;
                    if (!string.IsNullOrEmpty(Item.InnerText))
                        colors = Item.InnerText.Split(',');
                    else
                        colors = new string[0];

                    m_ChargingLeds[device].red = byte.Parse(colors[0]);
                    m_ChargingLeds[device].green = byte.Parse(colors[1]);
                    m_ChargingLeds[device].blue = byte.Parse(colors[2]);
                }
                catch { missingSetting = true; }

                if (m_Xdoc.SelectSingleNode("/" + rootname + "/ChargingColor") == null)
                {
                    try { Item = m_Xdoc.SelectSingleNode("/" + rootname + "/ChargingRed"); Byte.TryParse(Item.InnerText, out m_ChargingLeds[device].red); }
                    catch { missingSetting = true; }
                    try { Item = m_Xdoc.SelectSingleNode("/" + rootname + "/ChargingGreen"); Byte.TryParse(Item.InnerText, out m_ChargingLeds[device].green); }
                    catch { missingSetting = true; }
                    try { Item = m_Xdoc.SelectSingleNode("/" + rootname + "/ChargingBlue"); Byte.TryParse(Item.InnerText, out m_ChargingLeds[device].blue); }
                    catch { missingSetting = true; }
                }

                try
                {
                    Item = m_Xdoc.SelectSingleNode("/" + rootname + "/FlashColor");
                    string[] colors;
                    if (!string.IsNullOrEmpty(Item.InnerText))
                        colors = Item.InnerText.Split(',');
                    else
                        colors = new string[0];
                    m_FlashLeds[device].red = byte.Parse(colors[0]);
                    m_FlashLeds[device].green = byte.Parse(colors[1]);
                    m_FlashLeds[device].blue = byte.Parse(colors[2]);
                }
                catch { missingSetting = true; }

                try { Item = m_Xdoc.SelectSingleNode("/" + rootname + "/touchpadJitterCompensation"); bool.TryParse(Item.InnerText, out touchpadJitterCompensation[device]); }
                catch { missingSetting = true; }
                try { Item = m_Xdoc.SelectSingleNode("/" + rootname + "/lowerRCOn"); bool.TryParse(Item.InnerText, out lowerRCOn[device]); }
                catch { missingSetting = true; }
                try { Item = m_Xdoc.SelectSingleNode("/" + rootname + "/tapSensitivity"); byte.TryParse(Item.InnerText, out tapSensitivity[device]); }
                catch { missingSetting = true; }
                try { Item = m_Xdoc.SelectSingleNode("/" + rootname + "/doubleTap"); bool.TryParse(Item.InnerText, out doubleTap[device]); }
                catch { missingSetting = true; }
                try { Item = m_Xdoc.SelectSingleNode("/" + rootname + "/scrollSensitivity"); int.TryParse(Item.InnerText, out scrollSensitivity[device]); }
                catch { missingSetting = true; }

                try { Item = m_Xdoc.SelectSingleNode("/" + rootname + "/TouchpadInvert"); int temp = 0; int.TryParse(Item.InnerText, out temp); touchpadInvert[device] = Math.Min(Math.Max(temp, 0), 3); }
                catch { touchpadInvert[device] = 0; missingSetting = true; }

                try { Item = m_Xdoc.SelectSingleNode("/" + rootname + "/LeftTriggerMiddle"); byte.TryParse(Item.InnerText, out l2Deadzone[device]); }
                catch { missingSetting = true; }
                try { Item = m_Xdoc.SelectSingleNode("/" + rootname + "/RightTriggerMiddle"); byte.TryParse(Item.InnerText, out r2Deadzone[device]); }
                catch { missingSetting = true; }

                try { Item = m_Xdoc.SelectSingleNode("/" + rootname + "/L2AntiDeadZone"); int.TryParse(Item.InnerText, out l2AntiDeadzone[device]); }
                catch { l2AntiDeadzone[device] = 0; missingSetting = true; }

                try { Item = m_Xdoc.SelectSingleNode("/" + rootname + "/R2AntiDeadZone"); int.TryParse(Item.InnerText, out r2AntiDeadzone[device]); }
                catch { r2AntiDeadzone[device] = 0; missingSetting = true; }

                try {
                    Item = m_Xdoc.SelectSingleNode("/" + rootname + "/L2MaxZone"); int temp = 100;
                    int.TryParse(Item.InnerText, out temp);
                    l2Maxzone[device] = Math.Min(Math.Max(temp, 0), 100);
                }
                catch { l2Maxzone[device] = 100; missingSetting = true; }

                try {
                    Item = m_Xdoc.SelectSingleNode("/" + rootname + "/R2MaxZone"); int temp = 100;
                    int.TryParse(Item.InnerText, out temp);
                    r2Maxzone[device] = Math.Min(Math.Max(temp, 0), 100);
                }
                catch { r2Maxzone[device] = 100; missingSetting = true; }

                try
                {
                    Item = m_Xdoc.SelectSingleNode("/" + rootname + "/LSRotation"); int temp = 0;
                    int.TryParse(Item.InnerText, out temp);
                    temp = Math.Min(Math.Max(temp, -180), 180);
                    LSRotation[device] = temp * Math.PI / 180.0;
                }
                catch { LSRotation[device] = 0.0; missingSetting = true; }

                try
                {
                    Item = m_Xdoc.SelectSingleNode("/" + rootname + "/RSRotation"); int temp = 0;
                    int.TryParse(Item.InnerText, out temp);
                    temp = Math.Min(Math.Max(temp, -180), 180);
                    RSRotation[device] = temp * Math.PI / 180.0;
                }
                catch { RSRotation[device] = 0.0; missingSetting = true; }

                try { Item = m_Xdoc.SelectSingleNode("/" + rootname + "/ButtonMouseSensitivity"); int.TryParse(Item.InnerText, out buttonMouseSensitivity[device]); }
                catch { missingSetting = true; }
                try { Item = m_Xdoc.SelectSingleNode("/" + rootname + "/Rainbow"); double.TryParse(Item.InnerText, out rainbow[device]); }
                catch { rainbow[device] = 0; missingSetting = true; }
                try { Item = m_Xdoc.SelectSingleNode("/" + rootname + "/LSDeadZone"); int.TryParse(Item.InnerText, out LSDeadzone[device]); }
                catch { LSDeadzone[device] = 0; missingSetting = true; }
                try { Item = m_Xdoc.SelectSingleNode("/" + rootname + "/RSDeadZone"); int.TryParse(Item.InnerText, out RSDeadzone[device]); }
                catch { RSDeadzone[device] = 0; missingSetting = true; }
                try { Item = m_Xdoc.SelectSingleNode("/" + rootname + "/LSAntiDeadZone"); int.TryParse(Item.InnerText, out LSAntiDeadzone[device]); }
                catch { LSAntiDeadzone[device] = 0; missingSetting = true; }
                try { Item = m_Xdoc.SelectSingleNode("/" + rootname + "/RSAntiDeadZone"); int.TryParse(Item.InnerText, out RSAntiDeadzone[device]); }
                catch { RSAntiDeadzone[device] = 0; missingSetting = true; }

                try {
                    Item = m_Xdoc.SelectSingleNode("/" + rootname + "/LSMaxZone"); int temp = 100;
                    int.TryParse(Item.InnerText, out temp);
                    LSMaxzone[device] = Math.Min(Math.Max(temp, 0), 100);
                }
                catch { LSMaxzone[device] = 100; missingSetting = true; }

                try {
                    Item = m_Xdoc.SelectSingleNode("/" + rootname + "/RSMaxZone"); int temp = 100;
                    int.TryParse(Item.InnerText, out temp);
                    RSMaxzone[device] = Math.Min(Math.Max(temp, 0), 100);
                }
                catch { RSMaxzone[device] = 100; missingSetting = true; }

                try { Item = m_Xdoc.SelectSingleNode("/" + rootname + "/SXDeadZone"); double.TryParse(Item.InnerText, out SXDeadzone[device]); }
                catch { SXDeadzone[device] = 0.02; missingSetting = true; }
                try { Item = m_Xdoc.SelectSingleNode("/" + rootname + "/SZDeadZone"); double.TryParse(Item.InnerText, out SZDeadzone[device]); }
                catch { SZDeadzone[device] = 0.02; missingSetting = true; }

                try { Item = m_Xdoc.SelectSingleNode("/" + rootname + "/SXMaxZone");
                    int temp = 0;
                    int.TryParse(Item.InnerText, out temp);
                    SXMaxzone[device] = Math.Min(Math.Max(temp * 0.01, 0.0), 1.0);
                }
                catch { SXMaxzone[device] = 1.0; missingSetting = true; }

                try { Item = m_Xdoc.SelectSingleNode("/" + rootname + "/SZMaxZone");
                    int temp = 0;
                    int.TryParse(Item.InnerText, out temp);
                    SZMaxzone[device] = Math.Min(Math.Max(temp * 0.01, 0.0), 1.0);
                }
                catch { SZMaxzone[device] = 1.0; missingSetting = true; }

                try { Item = m_Xdoc.SelectSingleNode("/" + rootname + "/SXAntiDeadZone");
                    int temp = 0;
                    int.TryParse(Item.InnerText, out temp);
                    SXAntiDeadzone[device] = Math.Min(Math.Max(temp * 0.01, 0.0), 1.0);
                }
                catch { SXAntiDeadzone[device] = 0.0; missingSetting = true; }

                try { Item = m_Xdoc.SelectSingleNode("/" + rootname + "/SZAntiDeadZone");
                    int temp = 0;
                    int.TryParse(Item.InnerText, out temp);
                    SZAntiDeadzone[device] = Math.Min(Math.Max(temp * 0.01, 0.0), 1.0);
                }
                catch { SZAntiDeadzone[device] = 0.0; missingSetting = true; }

                try
                {
                    Item = m_Xdoc.SelectSingleNode("/" + rootname + "/Sensitivity");
                    string[] s = Item.InnerText.Split('|');
                    if (s.Length == 1)
                        s = Item.InnerText.Split(',');
                    if (!double.TryParse(s[0], out LSSens[device]) || LSSens[device] < .5f)
                        LSSens[device] = 1;
                    if (!double.TryParse(s[1], out RSSens[device]) || RSSens[device] < .5f)
                        RSSens[device] = 1;
                    if (!double.TryParse(s[2], out l2Sens[device]) || l2Sens[device] < .1f)
                        l2Sens[device] = 1;
                    if (!double.TryParse(s[3], out r2Sens[device]) || r2Sens[device] < .1f)
                        r2Sens[device] = 1;
                    if (!double.TryParse(s[4], out SXSens[device]) || SXSens[device] < .5f)
                        SXSens[device] = 1;
                    if (!double.TryParse(s[5], out SZSens[device]) || SZSens[device] < .5f)
                        SZSens[device] = 1;
                }
                catch { missingSetting = true; }

                try { Item = m_Xdoc.SelectSingleNode("/" + rootname + "/ChargingType"); int.TryParse(Item.InnerText, out chargingType[device]); }
                catch { missingSetting = true; }
                try { Item = m_Xdoc.SelectSingleNode("/" + rootname + "/MouseAcceleration"); bool.TryParse(Item.InnerText, out mouseAccel[device]); }
                catch { missingSetting = true; }

                int shiftM = 0;
                if (m_Xdoc.SelectSingleNode("/" + rootname + "/ShiftModifier") != null)
                    int.TryParse(m_Xdoc.SelectSingleNode("/" + rootname + "/ShiftModifier").InnerText, out shiftM);

                try
                {
                    Item = m_Xdoc.SelectSingleNode("/" + rootname + "/LaunchProgram");
                    launchProgram[device] = Item.InnerText;
                }
                catch { launchProgram[device] = string.Empty; missingSetting = true; }

                if (launchprogram == true && launchProgram[device] != string.Empty)
                {
                    string programPath = launchProgram[device];
                    System.Diagnostics.Process[] localAll = System.Diagnostics.Process.GetProcesses();
                    bool procFound = false;
                    for (int procInd = 0, procsLen = localAll.Length; !procFound && procInd < procsLen; procInd++)
                    {
                        try
                        {
                            string temp = localAll[procInd].MainModule.FileName;
                            if (temp == programPath)
                            {
                                procFound = true;
                            }
                        }
                        // Ignore any process for which this information
                        // is not exposed
                        catch { }
                    }

                    if (!procFound)
                    {
                        Task processTask = new Task(() =>
                        {
                            Thread.Sleep(5000);
                            System.Diagnostics.Process tempProcess = new System.Diagnostics.Process();
                            tempProcess.StartInfo.FileName = programPath;
                            tempProcess.StartInfo.WorkingDirectory = new FileInfo(programPath).Directory.ToString();
                            //tempProcess.StartInfo.UseShellExecute = false;
                            try { tempProcess.Start(); }
                            catch { }
                        });

                        processTask.Start();
                    }
                }

                try
                {
                    Item = m_Xdoc.SelectSingleNode("/" + rootname + "/DinputOnly");
                    bool.TryParse(Item.InnerText, out dinputOnly[device]);
                }
                catch { dinputOnly[device] = false; missingSetting = true; }

                bool oldUseDInputOnly = Global.useDInputOnly[device];
                Global.useDInputOnly[device] = dinputOnly[device];

                // Only change xinput devices under certain conditions. Avoid
                // performing this upon program startup before loading devices.
                if (xinputChange)
                {
                    if (device < 4)
                    {
                        //bool changed = false;
                        DS4Device tempDevice = control.DS4Controllers[device];
                        bool exists = tempBool = (tempDevice != null);
                        bool synced = tempBool = exists ? tempDevice.isSynced() : false;
                        bool isAlive = tempBool = exists ? tempDevice.IsAlive() : false;
                        if (dinputOnly[device] != oldUseDInputOnly)
                        {
                            if (dinputOnly[device] == true)
                            {
                                Global.useDInputOnly[device] = true;
                                xinputPlug = false;
                                xinputStatus = true;

                                //changed = true;
                            }
                            else if (synced && isAlive)
                            {
                                Global.useDInputOnly[device] = false;
                                xinputPlug = true;
                                xinputStatus = true;

                                //changed = true;
                            }

                            /*if (changed)
                            {
                                System.Threading.Thread.Sleep(Global.XINPUT_UNPLUG_SETTLE_TIME);
                            }
                            */
                        }
                    }
                }

                try
                {
                    Item = m_Xdoc.SelectSingleNode("/" + rootname + "/StartTouchpadOff");
                    bool.TryParse(Item.InnerText, out startTouchpadOff[device]);
                    if (startTouchpadOff[device] == true) control.StartTPOff(device);
                }
                catch { startTouchpadOff[device] = false; missingSetting = true; }

                try { Item = m_Xdoc.SelectSingleNode("/" + rootname + "/UseTPforControls"); bool.TryParse(Item.InnerText, out useTPforControls[device]); }
                catch { useTPforControls[device] = false; missingSetting = true; }

                try { Item = m_Xdoc.SelectSingleNode("/" + rootname + "/UseSAforMouse"); bool.TryParse(Item.InnerText, out useSAforMouse[device]); }
                catch { useSAforMouse[device] = false; missingSetting = true; }

                try { Item = m_Xdoc.SelectSingleNode("/" + rootname + "/SATriggers"); sATriggers[device] = Item.InnerText; }
                catch { sATriggers[device] = ""; missingSetting = true; }

                try {
                    Item = m_Xdoc.SelectSingleNode("/" + rootname + "/TouchDisInvTriggers");
                    string[] triggers = Item.InnerText.Split(',');
                    int temp = -1;
                    List<int> tempIntList = new List<int>();
                    for (int i = 0, arlen = triggers.Length; i < arlen; i++)
                    {
                        if (int.TryParse(triggers[i], out temp))
                            tempIntList.Add(temp);
                    }

                    touchDisInvertTriggers[device] = tempIntList.ToArray();
                }
                catch { touchDisInvertTriggers[device] = new int[1] { -1 }; missingSetting = true; }

                try { Item = m_Xdoc.SelectSingleNode("/" + rootname + "/GyroSensitivity"); int.TryParse(Item.InnerText, out gyroSensitivity[device]); }
                catch { gyroSensitivity[device] = 100; missingSetting = true; }

                try { Item = m_Xdoc.SelectSingleNode("/" + rootname + "/GyroSensVerticalScale"); int.TryParse(Item.InnerText, out gyroSensVerticalScale[device]); }
                catch { gyroSensVerticalScale[device] = 100; missingSetting = true; }

                try { Item = m_Xdoc.SelectSingleNode("/" + rootname + "/GyroInvert"); int.TryParse(Item.InnerText, out gyroInvert[device]); }
                catch { gyroInvert[device] = 0; missingSetting = true; }

                try { Item = m_Xdoc.SelectSingleNode("/" + rootname + "/GyroTriggerTurns"); bool.TryParse(Item.InnerText, out gyroTriggerTurns[device]); }
                catch { gyroTriggerTurns[device] = true; missingSetting = true; }

                try { Item = m_Xdoc.SelectSingleNode("/" + rootname + "/GyroSmoothing"); bool.TryParse(Item.InnerText, out gyroSmoothing[device]); }
                catch { gyroSmoothing[device] = false; missingSetting = true; }

                try { Item = m_Xdoc.SelectSingleNode("/" + rootname + "/GyroSmoothingWeight"); int temp = 0; int.TryParse(Item.InnerText, out temp); gyroSmoothWeight[device] = Math.Min(Math.Max(0.0, Convert.ToDouble(temp * 0.01)), 1.0); }
                catch { gyroSmoothWeight[device] = 0.5; missingSetting = true; }

                try { Item = m_Xdoc.SelectSingleNode("/" + rootname + "/GyroMouseHAxis"); int temp = 0; int.TryParse(Item.InnerText, out temp); gyroMouseHorizontalAxis[device] = Math.Min(Math.Max(0, temp), 1); }
                catch { gyroMouseHorizontalAxis[device] = 0; missingSetting = true; }

                try { Item = m_Xdoc.SelectSingleNode("/" + rootname + "/LSCurve"); int.TryParse(Item.InnerText, out lsCurve[device]); }
                catch { lsCurve[device] = 0; missingSetting = true; }

                try { Item = m_Xdoc.SelectSingleNode("/" + rootname + "/RSCurve"); int.TryParse(Item.InnerText, out rsCurve[device]); }
                catch { rsCurve[device] = 0; missingSetting = true; }

                try {
                    Item = m_Xdoc.SelectSingleNode("/" + rootname + "/BTPollRate");
                    int temp = 0;
                    int.TryParse(Item.InnerText, out temp);
                    btPollRate[device] = (temp >= 0 && temp <= 16) ? temp : 0;
                }
                catch { btPollRate[device] = 4; missingSetting = true; }

                try { Item = m_Xdoc.SelectSingleNode("/" + rootname + "/LSOutputCurveMode"); lsOutCurveMode[device] = stickOutputCurveId(Item.InnerText); }
                catch { lsOutCurveMode[device] = 0; missingSetting = true; }

                try { Item = m_Xdoc.SelectSingleNode("/" + rootname + "/RSOutputCurveMode"); rsOutCurveMode[device] = stickOutputCurveId(Item.InnerText); }
                catch { rsOutCurveMode[device] = 0; missingSetting = true; }

                try { Item = m_Xdoc.SelectSingleNode("/" + rootname + "/L2OutputCurveMode"); l2OutCurveMode[device] = axisOutputCurveId(Item.InnerText); }
                catch { l2OutCurveMode[device] = 0; missingSetting = true; }

                try { Item = m_Xdoc.SelectSingleNode("/" + rootname + "/R2OutputCurveMode"); r2OutCurveMode[device] = axisOutputCurveId(Item.InnerText); }
                catch { r2OutCurveMode[device] = 0; missingSetting = true; }

                try { Item = m_Xdoc.SelectSingleNode("/" + rootname + "/SXOutputCurveMode"); sxOutCurveMode[device] = axisOutputCurveId(Item.InnerText); }
                catch { sxOutCurveMode[device] = 0; missingSetting = true; }

                try { Item = m_Xdoc.SelectSingleNode("/" + rootname + "/SZOutputCurveMode"); szOutCurveMode[device] = axisOutputCurveId(Item.InnerText); }
                catch { szOutCurveMode[device] = 0; missingSetting = true; }

                try { Item = m_Xdoc.SelectSingleNode("/" + rootname + "/TrackballMode"); bool.TryParse(Item.InnerText, out trackballMode[device]); }
                catch { trackballMode[device] = false; missingSetting = true; }

                try { Item = m_Xdoc.SelectSingleNode("/" + rootname + "/TrackballFriction"); double.TryParse(Item.InnerText, out trackballFriction[device]); }
                catch { trackballFriction[device] = 10.0; missingSetting = true; }

                try
                {
                    Item = m_Xdoc.SelectSingleNode("/" + rootname + "/ProfileActions");
                    profileActions[device].Clear();
                    if (!string.IsNullOrEmpty(Item.InnerText))
                    {
                        string[] actionNames = Item.InnerText.Split('/');
                        for (int actIndex = 0, actLen = actionNames.Length; actIndex < actLen; actIndex++)
                        {
                            string tempActionName = actionNames[actIndex];
                            if (!profileActions[device].Contains(tempActionName))
                            {
                                profileActions[device].Add(tempActionName);
                            }
                        }
                    }
                }
                catch { profileActions[device].Clear(); missingSetting = true; }

                foreach (DS4ControlSettings dcs in ds4settings[device])
                    dcs.Reset();

                containsCustomAction[device] = false;
                containsCustomExtras[device] = false;
                profileActionCount[device] = profileActions[device].Count;
                profileActionDict[device].Clear();
                profileActionIndexDict[device].Clear();
                foreach (string actionname in profileActions[device])
                {
                    profileActionDict[device][actionname] = Global.GetAction(actionname);
                    profileActionIndexDict[device][actionname] = Global.GetActionIndexOf(actionname);
                }

                DS4KeyType keyType;
                ushort wvk;

                {
                    XmlNode ParentItem = m_Xdoc.SelectSingleNode("/" + rootname + "/Control/Button");
                    if (ParentItem != null)
                    {
                        foreach (XmlNode item in ParentItem.ChildNodes)
                        {
                            UpdateDS4CSetting(device, item.Name, false, getX360ControlsByName(item.InnerText), "", DS4KeyType.None, 0);
                            customMapButtons.Add(getDS4ControlsByName(item.Name), getX360ControlsByName(item.InnerText));
                        }
                    }

                    ParentItem = m_Xdoc.SelectSingleNode("/" + rootname + "/Control/Macro");
                    if (ParentItem != null)
                    {
                        foreach (XmlNode item in ParentItem.ChildNodes)
                        {
                            customMapMacros.Add(getDS4ControlsByName(item.Name), item.InnerText);
                            string[] skeys;
                            int[] keys;
                            if (!string.IsNullOrEmpty(item.InnerText))
                            {
                                skeys = item.InnerText.Split('/');
                                keys = new int[skeys.Length];
                            }
                            else
                            {
                                skeys = new string[0];
                                keys = new int[0];
                            }

                            for (int i = 0, keylen = keys.Length; i < keylen; i++)
                                keys[i] = int.Parse(skeys[i]);

                            UpdateDS4CSetting(device, item.Name, false, keys, "", DS4KeyType.None, 0);
                        }
                    }

                    ParentItem = m_Xdoc.SelectSingleNode("/" + rootname + "/Control/Key");
                    if (ParentItem != null)
                    {
                        foreach (XmlNode item in ParentItem.ChildNodes)
                        {
                            if (ushort.TryParse(item.InnerText, out wvk))
                            {
                                UpdateDS4CSetting(device, item.Name, false, wvk, "", DS4KeyType.None, 0);
                                customMapKeys.Add(getDS4ControlsByName(item.Name), wvk);
                            }
                        }
                    }

                    ParentItem = m_Xdoc.SelectSingleNode("/" + rootname + "/Control/Extras");
                    if (ParentItem != null)
                    {
                        foreach (XmlNode item in ParentItem.ChildNodes)
                        {
                            if (item.InnerText != string.Empty)
                            {
                                UpdateDS4CExtra(device, item.Name, false, item.InnerText);
                                customMapExtras.Add(getDS4ControlsByName(item.Name), item.InnerText);
                            }
                            else
                                ParentItem.RemoveChild(item);
                        }
                    }

                    ParentItem = m_Xdoc.SelectSingleNode("/" + rootname + "/Control/KeyType");
                    if (ParentItem != null)
                    {
                        foreach (XmlNode item in ParentItem.ChildNodes)
                        {
                            if (item != null)
                            {
                                keyType = DS4KeyType.None;
                                if (item.InnerText.Contains(DS4KeyType.ScanCode.ToString()))
                                    keyType |= DS4KeyType.ScanCode;
                                if (item.InnerText.Contains(DS4KeyType.Toggle.ToString()))
                                    keyType |= DS4KeyType.Toggle;
                                if (item.InnerText.Contains(DS4KeyType.Macro.ToString()))
                                    keyType |= DS4KeyType.Macro;
                                if (item.InnerText.Contains(DS4KeyType.HoldMacro.ToString()))
                                    keyType |= DS4KeyType.HoldMacro;
                                if (item.InnerText.Contains(DS4KeyType.Unbound.ToString()))
                                    keyType |= DS4KeyType.Unbound;
                                if (keyType != DS4KeyType.None)
                                {
                                    UpdateDS4CKeyType(device, item.Name, false, keyType);
                                    customMapKeyTypes.Add(getDS4ControlsByName(item.Name), keyType);
                                }
                            }
                        }
                    }

                    ParentItem = m_Xdoc.SelectSingleNode("/" + rootname + "/ShiftControl/Button");
                    if (ParentItem != null)
                    {
                        foreach (XmlElement item in ParentItem.ChildNodes)
                        {
                            int shiftT = shiftM;
                            if (item.HasAttribute("Trigger"))
                                int.TryParse(item.Attributes["Trigger"].Value, out shiftT);
                            UpdateDS4CSetting(device, item.Name, true, getX360ControlsByName(item.InnerText), "", DS4KeyType.None, shiftT);
                            shiftCustomMapButtons.Add(getDS4ControlsByName(item.Name), getX360ControlsByName(item.InnerText));
                        }
                    }

                    ParentItem = m_Xdoc.SelectSingleNode("/" + rootname + "/ShiftControl/Macro");
                    if (ParentItem != null)
                    {
                        foreach (XmlElement item in ParentItem.ChildNodes)
                        {
                            shiftCustomMapMacros.Add(getDS4ControlsByName(item.Name), item.InnerText);
                            string[] skeys;
                            int[] keys;
                            if (!string.IsNullOrEmpty(item.InnerText))
                            {
                                skeys = item.InnerText.Split('/');
                                keys = new int[skeys.Length];
                            }
                            else
                            {
                                skeys = new string[0];
                                keys = new int[0];
                            }

                            for (int i = 0, keylen = keys.Length; i < keylen; i++)
                                keys[i] = int.Parse(skeys[i]);

                            int shiftT = shiftM;
                            if (item.HasAttribute("Trigger"))
                                int.TryParse(item.Attributes["Trigger"].Value, out shiftT);
                            UpdateDS4CSetting(device, item.Name, true, keys, "", DS4KeyType.None, shiftT);
                        }
                    }

                    ParentItem = m_Xdoc.SelectSingleNode("/" + rootname + "/ShiftControl/Key");
                    if (ParentItem != null)
                    {
                        foreach (XmlElement item in ParentItem.ChildNodes)
                        {
                            if (ushort.TryParse(item.InnerText, out wvk))
                            {
                                int shiftT = shiftM;
                                if (item.HasAttribute("Trigger"))
                                    int.TryParse(item.Attributes["Trigger"].Value, out shiftT);
                                UpdateDS4CSetting(device, item.Name, true, wvk, "", DS4KeyType.None, shiftT);
                                shiftCustomMapKeys.Add(getDS4ControlsByName(item.Name), wvk);
                            }
                        }
                    }

                    ParentItem = m_Xdoc.SelectSingleNode("/" + rootname + "/ShiftControl/Extras");
                    if (ParentItem != null)
                    {
                        foreach (XmlElement item in ParentItem.ChildNodes)
                        {
                            if (item.InnerText != string.Empty)
                            {
                                UpdateDS4CExtra(device, item.Name, true, item.InnerText);
                                shiftCustomMapExtras.Add(getDS4ControlsByName(item.Name), item.InnerText);
                            }
                            else
                                ParentItem.RemoveChild(item);
                        }
                    }

                    ParentItem = m_Xdoc.SelectSingleNode("/" + rootname + "/ShiftControl/KeyType");
                    if (ParentItem != null)
                    {
                        foreach (XmlElement item in ParentItem.ChildNodes)
                        {
                            if (item != null)
                            {
                                keyType = DS4KeyType.None;
                                if (item.InnerText.Contains(DS4KeyType.ScanCode.ToString()))
                                    keyType |= DS4KeyType.ScanCode;
                                if (item.InnerText.Contains(DS4KeyType.Toggle.ToString()))
                                    keyType |= DS4KeyType.Toggle;
                                if (item.InnerText.Contains(DS4KeyType.Macro.ToString()))
                                    keyType |= DS4KeyType.Macro;
                                if (item.InnerText.Contains(DS4KeyType.HoldMacro.ToString()))
                                    keyType |= DS4KeyType.HoldMacro;
                                if (item.InnerText.Contains(DS4KeyType.Unbound.ToString()))
                                    keyType |= DS4KeyType.Unbound;
                                if (keyType != DS4KeyType.None)
                                {
                                    UpdateDS4CKeyType(device, item.Name, true, keyType);
                                    shiftCustomMapKeyTypes.Add(getDS4ControlsByName(item.Name), keyType);
                                }
                            }
                        }
                    }
                }
            }

            // Only add missing settings if the actual load was graceful
            if (missingSetting && Loaded)// && buttons != null)
                SaveProfile(device, profilepath);

            containsCustomAction[device] = HasCustomActions(device);
            containsCustomExtras[device] = HasCustomExtras(device);

            // If a device exists, make sure to transfer relevant profile device
            // options to device instance
            if (postLoad && device < 4)
            {
                DS4Device tempDev = control.DS4Controllers[device];
                if (tempDev != null && tempDev.isSynced())
                {
                    tempDev.queueEvent(() =>
                    {
                        tempDev.setIdleTimeout(idleDisconnectTimeout[device]);
                        tempDev.setBTPollRate(btPollRate[device]);
                        if (xinputStatus && xinputPlug)
                        {
                            bool xinputResult = control.x360Bus.Plugin(device);
                            int xinputIndex = control.x360Bus.FirstController + device;
                            if (xinputResult)
                            {
                                dinputOnly[device] = false;
                                Log.LogToGui("X360 Controller # " + xinputIndex + " connected", false);
                            }
                            else
                            {
                                dinputOnly[device] = true;
                                Log.LogToGui("X360 Controller # " + xinputIndex + " failed. Using DInput only mode", true);
                            }
                        }
                        else if (xinputStatus && !xinputPlug)
                        {
                            bool xinputResult = control.x360Bus.Unplug(device);
                            int xinputIndex = control.x360Bus.FirstController + device;
                            if (xinputResult)
                            {
                                dinputOnly[device] = true;
                                Log.LogToGui("X360 Controller # " + xinputIndex + " unplugged", false);
                            }
                            else
                            {
                                Log.LogToGui("X360 Controller # " + xinputIndex + " failed to unplug", true);
                            }
                        }

                        tempDev.setRumble(0, 0);
                    });

                    Program.rootHub.touchPad[device]?.ResetTrackAccel(trackballFriction[device]);
                }
            }

            return Loaded;
        }

        public bool Load()
        {
            bool Loaded = true;
            bool missingSetting = false;

            try
            {
                if (File.Exists(m_Profile))
                {
                    XmlNode Item;

                    m_Xdoc.Load(m_Profile);

                    try { Item = m_Xdoc.SelectSingleNode("/Profile/useExclusiveMode"); Boolean.TryParse(Item.InnerText, out useExclusiveMode); }
                    catch { missingSetting = true; }
                    try { Item = m_Xdoc.SelectSingleNode("/Profile/startMinimized"); Boolean.TryParse(Item.InnerText, out startMinimized); }
                    catch { missingSetting = true; }
                    try { Item = m_Xdoc.SelectSingleNode("/Profile/minimizeToTaskbar"); Boolean.TryParse(Item.InnerText, out minToTaskbar); }
                    catch { missingSetting = true; }
                    try { Item = m_Xdoc.SelectSingleNode("/Profile/formWidth"); Int32.TryParse(Item.InnerText, out formWidth); }
                    catch { missingSetting = true; }
                    try { Item = m_Xdoc.SelectSingleNode("/Profile/formHeight"); Int32.TryParse(Item.InnerText, out formHeight); }
                    catch { missingSetting = true; }
                    try {
                        int temp = 0;
                        Item = m_Xdoc.SelectSingleNode("/Profile/formLocationX"); Int32.TryParse(Item.InnerText, out temp);
                        formLocationX = Math.Max(temp, 0);
                    }
                    catch { missingSetting = true; }

                    try {
                        int temp = 0;
                        Item = m_Xdoc.SelectSingleNode("/Profile/formLocationY"); Int32.TryParse(Item.InnerText, out temp);
                        formLocationY = Math.Max(temp, 0);
                    }
                    catch { missingSetting = true; }

                    try {
                        Item = m_Xdoc.SelectSingleNode("/Profile/Controller1"); profilePath[0] = Item.InnerText;
                        if (profilePath[0].ToLower().Contains("distance"))
                        {
                            distanceProfiles[0] = true;
                        }

                        olderProfilePath[0] = profilePath[0];
                    }
                    catch { profilePath[0] = olderProfilePath[0] = string.Empty; distanceProfiles[0] = false; missingSetting = true; }
                    try {
                        Item = m_Xdoc.SelectSingleNode("/Profile/Controller2"); profilePath[1] = Item.InnerText;
                        if (profilePath[1].ToLower().Contains("distance"))
                        {
                            distanceProfiles[1] = true;
                        }

                        olderProfilePath[1] = profilePath[1];
                    }
                    catch { profilePath[1] = olderProfilePath[1] = string.Empty; distanceProfiles[1] = false; missingSetting = true; }
                    try {
                        Item = m_Xdoc.SelectSingleNode("/Profile/Controller3"); profilePath[2] = Item.InnerText;
                        if (profilePath[2].ToLower().Contains("distance"))
                        {
                            distanceProfiles[2] = true;
                        }

                        olderProfilePath[2] = profilePath[2];
                    }
                    catch { profilePath[2] = olderProfilePath[2] = string.Empty; distanceProfiles[2] = false; missingSetting = true; }
                    try {
                        Item = m_Xdoc.SelectSingleNode("/Profile/Controller4"); profilePath[3] = Item.InnerText;
                        if (profilePath[3].ToLower().Contains("distance"))
                        {
                            distanceProfiles[3] = true;
                        }

                        olderProfilePath[3] = profilePath[3];
                    }
                    catch { profilePath[3] = olderProfilePath[3] = string.Empty; distanceProfiles[3] = false; missingSetting = true; }

                    try { Item = m_Xdoc.SelectSingleNode("/Profile/LastChecked"); DateTime.TryParse(Item.InnerText, out lastChecked); }
                    catch { missingSetting = true; }
                    try { Item = m_Xdoc.SelectSingleNode("/Profile/CheckWhen"); Int32.TryParse(Item.InnerText, out CheckWhen); }
                    catch { missingSetting = true; }

                    try
                    {
                        Item = m_Xdoc.SelectSingleNode("/Profile/Notifications");
                        if (!int.TryParse(Item.InnerText, out notifications))
                            notifications = (Boolean.Parse(Item.InnerText) ? 2 : 0);
                    }
                    catch { missingSetting = true; }

                    try { Item = m_Xdoc.SelectSingleNode("/Profile/DisconnectBTAtStop"); Boolean.TryParse(Item.InnerText, out disconnectBTAtStop); }
                    catch { missingSetting = true; }
                    try { Item = m_Xdoc.SelectSingleNode("/Profile/SwipeProfiles"); Boolean.TryParse(Item.InnerText, out swipeProfiles); }
                    catch { missingSetting = true; }
                    try { Item = m_Xdoc.SelectSingleNode("/Profile/UseDS4ForMapping"); Boolean.TryParse(Item.InnerText, out ds4Mapping); }
                    catch { missingSetting = true; }
                    try { Item = m_Xdoc.SelectSingleNode("/Profile/QuickCharge"); Boolean.TryParse(Item.InnerText, out quickCharge); }
                    catch { missingSetting = true; }
                    try { Item = m_Xdoc.SelectSingleNode("/Profile/FirstXinputPort"); Int32.TryParse(Item.InnerText, out firstXinputPort); }
                    catch { missingSetting = true; }
                    try { Item = m_Xdoc.SelectSingleNode("/Profile/CloseMinimizes"); Boolean.TryParse(Item.InnerText, out closeMini); }
                    catch { missingSetting = true; }
                    try { Item = m_Xdoc.SelectSingleNode("/Profile/UseLang"); useLang = Item.InnerText; }
                    catch { missingSetting = true; }
                    try { Item = m_Xdoc.SelectSingleNode("/Profile/DownloadLang"); Boolean.TryParse(Item.InnerText, out downloadLang); }
                    catch { missingSetting = true; }
                    try { Item = m_Xdoc.SelectSingleNode("/Profile/FlashWhenLate"); Boolean.TryParse(Item.InnerText, out flashWhenLate); }
                    catch { missingSetting = true; }
                    try { Item = m_Xdoc.SelectSingleNode("/Profile/FlashWhenLateAt"); int.TryParse(Item.InnerText, out flashWhenLateAt); }
                    catch { missingSetting = true; }
                    try { Item = m_Xdoc.SelectSingleNode("/Profile/WhiteIcon"); Boolean.TryParse(Item.InnerText, out useWhiteIcon); }
                    catch { missingSetting = true; }

                    for (int i = 0; i < 4; i++)
                    {
                        try
                        {
                            Item = m_Xdoc.SelectSingleNode("/Profile/CustomLed" + (i + 1));
                            string[] ss = Item.InnerText.Split(':');
                            bool.TryParse(ss[0], out useCustomLeds[i]);
                            DS4Color.TryParse(ss[1], ref m_CustomLeds[i]);
                        }
                        catch { useCustomLeds[i] = false; m_CustomLeds[i] = new DS4Color(Color.Blue); missingSetting = true; }
                    }
                }
            }
            catch { }

            if (missingSetting)
                Save();

            return Loaded;
        }

        public bool Save()
        {
            bool Saved = true;

            XmlNode Node;

            m_Xdoc.RemoveAll();

            Node = m_Xdoc.CreateXmlDeclaration("1.0", "utf-8", String.Empty);
            m_Xdoc.AppendChild(Node);

            Node = m_Xdoc.CreateComment(String.Format(" Profile Configuration Data. {0} ", DateTime.Now));
            m_Xdoc.AppendChild(Node);

            Node = m_Xdoc.CreateWhitespace("\r\n");
            m_Xdoc.AppendChild(Node);

            Node = m_Xdoc.CreateNode(XmlNodeType.Element, "Profile", null);

            XmlNode xmlUseExclNode = m_Xdoc.CreateNode(XmlNodeType.Element, "useExclusiveMode", null); xmlUseExclNode.InnerText = useExclusiveMode.ToString(); Node.AppendChild(xmlUseExclNode);
            XmlNode xmlStartMinimized = m_Xdoc.CreateNode(XmlNodeType.Element, "startMinimized", null); xmlStartMinimized.InnerText = startMinimized.ToString(); Node.AppendChild(xmlStartMinimized);
            XmlNode xmlminToTaskbar = m_Xdoc.CreateNode(XmlNodeType.Element, "minimizeToTaskbar", null); xmlminToTaskbar.InnerText = minToTaskbar.ToString(); Node.AppendChild(xmlminToTaskbar);
            XmlNode xmlFormWidth = m_Xdoc.CreateNode(XmlNodeType.Element, "formWidth", null); xmlFormWidth.InnerText = formWidth.ToString(); Node.AppendChild(xmlFormWidth);
            XmlNode xmlFormHeight = m_Xdoc.CreateNode(XmlNodeType.Element, "formHeight", null); xmlFormHeight.InnerText = formHeight.ToString(); Node.AppendChild(xmlFormHeight);
            XmlNode xmlFormLocationX = m_Xdoc.CreateNode(XmlNodeType.Element, "formLocationX", null); xmlFormLocationX.InnerText = formLocationX.ToString(); Node.AppendChild(xmlFormLocationX);
            XmlNode xmlFormLocationY = m_Xdoc.CreateNode(XmlNodeType.Element, "formLocationY", null); xmlFormLocationY.InnerText = formLocationY.ToString(); Node.AppendChild(xmlFormLocationY);

            XmlNode xmlController1 = m_Xdoc.CreateNode(XmlNodeType.Element, "Controller1", null); xmlController1.InnerText = !Global.linkedProfileCheck[0] ? profilePath[0] : olderProfilePath[0]; Node.AppendChild(xmlController1);
            XmlNode xmlController2 = m_Xdoc.CreateNode(XmlNodeType.Element, "Controller2", null); xmlController2.InnerText = !Global.linkedProfileCheck[1] ? profilePath[1] : olderProfilePath[1]; Node.AppendChild(xmlController2);
            XmlNode xmlController3 = m_Xdoc.CreateNode(XmlNodeType.Element, "Controller3", null); xmlController3.InnerText = !Global.linkedProfileCheck[2] ? profilePath[2] : olderProfilePath[2]; Node.AppendChild(xmlController3);
            XmlNode xmlController4 = m_Xdoc.CreateNode(XmlNodeType.Element, "Controller4", null); xmlController4.InnerText = !Global.linkedProfileCheck[3] ? profilePath[3] : olderProfilePath[3]; Node.AppendChild(xmlController4);

            XmlNode xmlLastChecked = m_Xdoc.CreateNode(XmlNodeType.Element, "LastChecked", null); xmlLastChecked.InnerText = lastChecked.ToString(); Node.AppendChild(xmlLastChecked);
            XmlNode xmlCheckWhen = m_Xdoc.CreateNode(XmlNodeType.Element, "CheckWhen", null); xmlCheckWhen.InnerText = CheckWhen.ToString(); Node.AppendChild(xmlCheckWhen);
            XmlNode xmlNotifications = m_Xdoc.CreateNode(XmlNodeType.Element, "Notifications", null); xmlNotifications.InnerText = notifications.ToString(); Node.AppendChild(xmlNotifications);
            XmlNode xmlDisconnectBT = m_Xdoc.CreateNode(XmlNodeType.Element, "DisconnectBTAtStop", null); xmlDisconnectBT.InnerText = disconnectBTAtStop.ToString(); Node.AppendChild(xmlDisconnectBT);
            XmlNode xmlSwipeProfiles = m_Xdoc.CreateNode(XmlNodeType.Element, "SwipeProfiles", null); xmlSwipeProfiles.InnerText = swipeProfiles.ToString(); Node.AppendChild(xmlSwipeProfiles);
            XmlNode xmlDS4Mapping = m_Xdoc.CreateNode(XmlNodeType.Element, "UseDS4ForMapping", null); xmlDS4Mapping.InnerText = ds4Mapping.ToString(); Node.AppendChild(xmlDS4Mapping);
            XmlNode xmlQuickCharge = m_Xdoc.CreateNode(XmlNodeType.Element, "QuickCharge", null); xmlQuickCharge.InnerText = quickCharge.ToString(); Node.AppendChild(xmlQuickCharge);
            XmlNode xmlFirstXinputPort = m_Xdoc.CreateNode(XmlNodeType.Element, "FirstXinputPort", null); xmlFirstXinputPort.InnerText = firstXinputPort.ToString(); Node.AppendChild(xmlFirstXinputPort);
            XmlNode xmlCloseMini = m_Xdoc.CreateNode(XmlNodeType.Element, "CloseMinimizes", null); xmlCloseMini.InnerText = closeMini.ToString(); Node.AppendChild(xmlCloseMini);
            XmlNode xmlUseLang = m_Xdoc.CreateNode(XmlNodeType.Element, "UseLang", null); xmlUseLang.InnerText = useLang.ToString(); Node.AppendChild(xmlUseLang);
            XmlNode xmlDownloadLang = m_Xdoc.CreateNode(XmlNodeType.Element, "DownloadLang", null); xmlDownloadLang.InnerText = downloadLang.ToString(); Node.AppendChild(xmlDownloadLang);
            XmlNode xmlFlashWhenLate = m_Xdoc.CreateNode(XmlNodeType.Element, "FlashWhenLate", null); xmlFlashWhenLate.InnerText = flashWhenLate.ToString(); Node.AppendChild(xmlFlashWhenLate);
            XmlNode xmlFlashWhenLateAt = m_Xdoc.CreateNode(XmlNodeType.Element, "FlashWhenLateAt", null); xmlFlashWhenLateAt.InnerText = flashWhenLateAt.ToString(); Node.AppendChild(xmlFlashWhenLateAt);
            XmlNode xmlWhiteIcon = m_Xdoc.CreateNode(XmlNodeType.Element, "WhiteIcon", null); xmlWhiteIcon.InnerText = useWhiteIcon.ToString(); Node.AppendChild(xmlWhiteIcon);

            for (int i = 0; i < 4; i++)
            {
                XmlNode xmlCustomLed = m_Xdoc.CreateNode(XmlNodeType.Element, "CustomLed" + (1 + i), null);
                xmlCustomLed.InnerText = useCustomLeds[i] + ":" + m_CustomLeds[i].red + "," + m_CustomLeds[i].green + "," + m_CustomLeds[i].blue;
                Node.AppendChild(xmlCustomLed);
            }

            m_Xdoc.AppendChild(Node);

            try { m_Xdoc.Save(m_Profile); }
            catch (UnauthorizedAccessException) { Saved = false; }
            return Saved;
        }

        private void CreateAction()
        {
            XmlDocument m_Xdoc = new XmlDocument();
            XmlNode Node;

            Node = m_Xdoc.CreateXmlDeclaration("1.0", "utf-8", String.Empty);
            m_Xdoc.AppendChild(Node);

            Node = m_Xdoc.CreateComment(String.Format(" Special Actions Configuration Data. {0} ", DateTime.Now));
            m_Xdoc.AppendChild(Node);

            Node = m_Xdoc.CreateWhitespace("\r\n");
            m_Xdoc.AppendChild(Node);

            Node = m_Xdoc.CreateNode(XmlNodeType.Element, "Actions", "");
            m_Xdoc.AppendChild(Node);
            m_Xdoc.Save(m_Actions);
        }

        public bool SaveAction(string name, string controls, int mode, string details, bool edit, string extras = "")
        {
            bool saved = true;
            if (!File.Exists(m_Actions))
                CreateAction();
            m_Xdoc.Load(m_Actions);
            XmlNode Node;

            Node = m_Xdoc.CreateComment(String.Format(" Special Actions Configuration Data. {0} ", DateTime.Now));
            foreach (XmlNode node in m_Xdoc.SelectNodes("//comment()"))
                node.ParentNode.ReplaceChild(Node, node);

            Node = m_Xdoc.SelectSingleNode("Actions");
            XmlElement el = m_Xdoc.CreateElement("Action");
            el.SetAttribute("Name", name);
            el.AppendChild(m_Xdoc.CreateElement("Trigger")).InnerText = controls;
            switch (mode)
            {
                case 1:
                    el.AppendChild(m_Xdoc.CreateElement("Type")).InnerText = "Macro";
                    el.AppendChild(m_Xdoc.CreateElement("Details")).InnerText = details;
                    if (extras != string.Empty)
                        el.AppendChild(m_Xdoc.CreateElement("Extras")).InnerText = extras;
                    break;
                case 2:
                    el.AppendChild(m_Xdoc.CreateElement("Type")).InnerText = "Program";
                    el.AppendChild(m_Xdoc.CreateElement("Details")).InnerText = details.Split('?')[0];
                    el.AppendChild(m_Xdoc.CreateElement("Arguements")).InnerText = extras;
                    el.AppendChild(m_Xdoc.CreateElement("Delay")).InnerText = details.Split('?')[1];
                    break;
                case 3:
                    el.AppendChild(m_Xdoc.CreateElement("Type")).InnerText = "Profile";
                    el.AppendChild(m_Xdoc.CreateElement("Details")).InnerText = details;
                    el.AppendChild(m_Xdoc.CreateElement("UnloadTrigger")).InnerText = extras;
                    break;
                case 4:
                    el.AppendChild(m_Xdoc.CreateElement("Type")).InnerText = "Key";
                    el.AppendChild(m_Xdoc.CreateElement("Details")).InnerText = details;
                    if (!string.IsNullOrEmpty(extras))
                    {
                        string[] exts = extras.Split('\n');
                        el.AppendChild(m_Xdoc.CreateElement("UnloadTrigger")).InnerText = exts[1];
                        el.AppendChild(m_Xdoc.CreateElement("UnloadStyle")).InnerText = exts[0];
                    }
                    break;
                case 5:
                    el.AppendChild(m_Xdoc.CreateElement("Type")).InnerText = "DisconnectBT";
                    el.AppendChild(m_Xdoc.CreateElement("Details")).InnerText = details;
                    break;
                case 6:
                    el.AppendChild(m_Xdoc.CreateElement("Type")).InnerText = "BatteryCheck";
                    el.AppendChild(m_Xdoc.CreateElement("Details")).InnerText = details;
                    break;
                case 7:
                    el.AppendChild(m_Xdoc.CreateElement("Type")).InnerText = "MultiAction";
                    el.AppendChild(m_Xdoc.CreateElement("Details")).InnerText = details;
                    break;
            }

            if (edit)
            {
                XmlNode oldxmlprocess = m_Xdoc.SelectSingleNode("/Actions/Action[@Name=\"" + name + "\"]");
                Node.ReplaceChild(el, oldxmlprocess);
            }
            else { Node.AppendChild(el); }

            m_Xdoc.AppendChild(Node);
            try { m_Xdoc.Save(m_Actions); }
            catch { saved = false; }
            LoadActions();
            return saved;
        }

        public void RemoveAction(string name)
        {
            m_Xdoc.Load(m_Actions);
            XmlNode Node = m_Xdoc.SelectSingleNode("Actions");
            XmlNode Item = m_Xdoc.SelectSingleNode("/Actions/Action[@Name=\"" + name + "\"]");
            if (Item != null)
                Node.RemoveChild(Item);

            m_Xdoc.AppendChild(Node);
            m_Xdoc.Save(m_Actions);
            LoadActions();
        }

        public bool LoadActions()
        {
            bool saved = true;
            if (!File.Exists(Global.appdatapath + "\\Actions.xml"))
            {
                SaveAction("Disconnect Controller", "PS/Options", 5, "0", false);
                saved = false;
            }

            try
            {
                actions.Clear();
                XmlDocument doc = new XmlDocument();
                doc.Load(Global.appdatapath + "\\Actions.xml");
                XmlNodeList actionslist = doc.SelectNodes("Actions/Action");
                string name, controls, type, details, extras, extras2;
                Mapping.actionDone.Clear();
                foreach (XmlNode x in actionslist)
                {
                    name = x.Attributes["Name"].Value;
                    controls = x.ChildNodes[0].InnerText;
                    type = x.ChildNodes[1].InnerText;
                    details = x.ChildNodes[2].InnerText;
                    Mapping.actionDone.Add(new Mapping.ActionState());
                    if (type == "Profile")
                    {
                        extras = x.ChildNodes[3].InnerText;
                        actions.Add(new SpecialAction(name, controls, type, details, 0, extras));
                    }
                    else if (type == "Macro")
                    {
                        if (x.ChildNodes[3] != null) extras = x.ChildNodes[3].InnerText;
                        else extras = string.Empty;
                        actions.Add(new SpecialAction(name, controls, type, details, 0, extras));
                    }
                    else if (type == "Key")
                    {
                        if (x.ChildNodes[3] != null)
                        {
                            extras = x.ChildNodes[3].InnerText;
                            extras2 = x.ChildNodes[4].InnerText;
                        }
                        else
                        {
                            extras = string.Empty;
                            extras2 = string.Empty;
                        }
                        if (!string.IsNullOrEmpty(extras))
                            actions.Add(new SpecialAction(name, controls, type, details, 0, extras2 + '\n' + extras));
                        else
                            actions.Add(new SpecialAction(name, controls, type, details));
                    }
                    else if (type == "DisconnectBT")
                    {
                        double doub;
                        if (double.TryParse(details, out doub))
                            actions.Add(new SpecialAction(name, controls, type, "", doub));
                        else
                            actions.Add(new SpecialAction(name, controls, type, ""));
                    }
                    else if (type == "BatteryCheck")
                    {
                        double doub;
                        if (double.TryParse(details.Split('|')[0], out doub))
                            actions.Add(new SpecialAction(name, controls, type, details, doub));
                        else if (double.TryParse(details.Split(',')[0], out doub))
                            actions.Add(new SpecialAction(name, controls, type, details, doub));
                        else
                            actions.Add(new SpecialAction(name, controls, type, details));
                    }
                    else if (type == "Program")
                    {
                        double doub;
                        if (x.ChildNodes[3] != null)
                        {
                            extras = x.ChildNodes[3].InnerText;
                            if (double.TryParse(x.ChildNodes[4].InnerText, out doub))
                                actions.Add(new SpecialAction(name, controls, type, details, doub, extras));
                            else
                                actions.Add(new SpecialAction(name, controls, type, details, 0, extras));
                        }
                        else
                        {
                            actions.Add(new SpecialAction(name, controls, type, details));
                        }
                    }
                    else if (type == "XboxGameDVR" || type == "MultiAction")
                    {
                        actions.Add(new SpecialAction(name, controls, type, details));
                    }
                }
            }
            catch { saved = false; }
            return saved;
        }

        public bool createLinkedProfiles()
        {
            bool saved = true;
            XmlDocument m_Xdoc = new XmlDocument();
            XmlNode Node;

            Node = m_Xdoc.CreateXmlDeclaration("1.0", "utf-8", string.Empty);
            m_Xdoc.AppendChild(Node);

            Node = m_Xdoc.CreateComment(string.Format(" Mac Address and Profile Linking Data. {0} ", DateTime.Now));
            m_Xdoc.AppendChild(Node);

            Node = m_Xdoc.CreateWhitespace("\r\n");
            m_Xdoc.AppendChild(Node);

            Node = m_Xdoc.CreateNode(XmlNodeType.Element, "LinkedControllers", "");
            m_Xdoc.AppendChild(Node);

            try { m_Xdoc.Save(m_linkedProfiles); }
            catch (UnauthorizedAccessException) { Log.LogToGui("Unauthorized Access - Save failed to path: " + m_linkedProfiles, false); saved = false; }

            return saved;
        }

        public bool LoadLinkedProfiles()
        {
            bool loaded = true;
            if (File.Exists(m_linkedProfiles))
            {
                XmlDocument linkedXdoc = new XmlDocument();
                XmlNode Node;
                linkedXdoc.Load(m_linkedProfiles);
                linkedProfiles.Clear();

                try
                {
                    Node = linkedXdoc.SelectSingleNode("/LinkedControllers");
                    XmlNodeList links = Node.ChildNodes;
                    for (int i = 0, listLen = links.Count; i < listLen; i++)
                    {
                        XmlNode current = links[i];
                        string serial = current.Name.Replace("MAC", string.Empty);
                        string profile = current.InnerText;
                        linkedProfiles[serial] = profile;
                    }
                }
                catch { loaded = false; }
            }
            else
            {
                Log.LogToGui("LinkedProfiles.xml can't be found.", false);
                loaded = false;
            }

            return loaded;
        }

        public bool SaveLinkedProfiles()
        {
            bool saved = true;
            if (File.Exists(m_linkedProfiles))
            {
                XmlDocument linkedXdoc = new XmlDocument();
                XmlNode Node;

                Node = linkedXdoc.CreateXmlDeclaration("1.0", "utf-8", string.Empty);
                linkedXdoc.AppendChild(Node);

                Node = linkedXdoc.CreateComment(string.Format(" Mac Address and Profile Linking Data. {0} ", DateTime.Now));
                linkedXdoc.AppendChild(Node);

                Node = linkedXdoc.CreateWhitespace("\r\n");
                linkedXdoc.AppendChild(Node);

                Node = linkedXdoc.CreateNode(XmlNodeType.Element, "LinkedControllers", "");
                linkedXdoc.AppendChild(Node);

                Dictionary<string, string>.KeyCollection serials = linkedProfiles.Keys;
                for (int i = 0, itemCount = linkedProfiles.Count; i < itemCount; i++)
                {
                    string serial = serials.ElementAt(i);
                    string profile = linkedProfiles[serial];
                    XmlElement link = linkedXdoc.CreateElement("MAC" + serial);
                    link.InnerText = profile;
                    Node.AppendChild(link);
                }

                try { linkedXdoc.Save(m_linkedProfiles); }
                catch (UnauthorizedAccessException) { Log.LogToGui("Unauthorized Access - Save failed to path: " + m_linkedProfiles, false); saved = false; }
            }
            else
            {
                saved = createLinkedProfiles();
                saved = saved && SaveLinkedProfiles();
            }

            return saved;
        }

        public void UpdateDS4CSetting(int deviceNum, string buttonName, bool shift, object action, string exts, DS4KeyType kt, int trigger = 0)
        {
            DS4Controls dc;
            if (buttonName.StartsWith("bn"))
                dc = getDS4ControlsByName(buttonName);
            else
                dc = (DS4Controls)Enum.Parse(typeof(DS4Controls), buttonName, true);

            List<DS4ControlSettings> ds4settingsList = ds4settings[deviceNum];
            for (int i = 0, settingsLen = ds4settingsList.Count; i < settingsLen; i++)
            {
                DS4ControlSettings dcs = ds4settingsList[i];
                if (dcs.control == dc)
                {
                    dcs.UpdateSettings(shift, action, exts, kt, trigger);
                    break;
                }
            }
        }

        public void UpdateDS4CExtra(int deviceNum, string buttonName, bool shift, string exts)
        {
            DS4Controls dc;
            if (buttonName.StartsWith("bn"))
                dc = getDS4ControlsByName(buttonName);
            else
                dc = (DS4Controls)Enum.Parse(typeof(DS4Controls), buttonName, true);

            List<DS4ControlSettings> ds4settingsList = ds4settings[deviceNum];
            for (int i = 0, settingsLen = ds4settingsList.Count; i < settingsLen; i++)
            {
                DS4ControlSettings dcs = ds4settingsList[i];
                if (dcs.control == dc)
                {
                    if (shift)
                        dcs.shiftExtras = exts;
                    else
                        dcs.extras = exts;

                    break;
                }
            }
        }

        private void UpdateDS4CKeyType(int deviceNum, string buttonName, bool shift, DS4KeyType keyType)
        {
            DS4Controls dc;
            if (buttonName.StartsWith("bn"))
                dc = getDS4ControlsByName(buttonName);
            else
                dc = (DS4Controls)Enum.Parse(typeof(DS4Controls), buttonName, true);

            List<DS4ControlSettings> ds4settingsList = ds4settings[deviceNum];
            for (int i = 0, settingsLen = ds4settingsList.Count; i < settingsLen; i++)
            {
                DS4ControlSettings dcs = ds4settingsList[i];
                if (dcs.control == dc)
                {
                    if (shift)
                        dcs.shiftKeyType = keyType;
                    else
                        dcs.keyType = keyType;

                    break;
                }
            }
        }

        public object GetDS4Action(int deviceNum, string buttonName, bool shift)
        {
            DS4Controls dc;
            if (buttonName.StartsWith("bn"))
                dc = getDS4ControlsByName(buttonName);
            else
                dc = (DS4Controls)Enum.Parse(typeof(DS4Controls), buttonName, true);

            List<DS4ControlSettings> ds4settingsList = ds4settings[deviceNum];
            for (int i = 0, settingsLen = ds4settingsList.Count; i < settingsLen; i++)
            {
                DS4ControlSettings dcs = ds4settingsList[i];
                if (dcs.control == dc)
                {
                    if (shift)
                        return dcs.shiftAction;
                    else
                        return dcs.action;
                }
            }

            return null;
        }

        public object GetDS4Action(int deviceNum, DS4Controls dc, bool shift)
        {
            int temp = (int)dc;
            if (temp > 0)
            {
                int index = temp - 1;
                DS4ControlSettings dcs = ds4settings[deviceNum][index];
                if (shift)
                {
                    return dcs.shiftTrigger;
                }
                else
                {
                    return dcs.action;
                }
            }

            return null;
        }

        public string GetDS4Extra(int deviceNum, string buttonName, bool shift)
        {
            DS4Controls dc;
            if (buttonName.StartsWith("bn"))
                dc = getDS4ControlsByName(buttonName);
            else
                dc = (DS4Controls)Enum.Parse(typeof(DS4Controls), buttonName, true);

            List<DS4ControlSettings> ds4settingsList = ds4settings[deviceNum];
            for (int i = 0, settingsLen = ds4settingsList.Count; i < settingsLen; i++)
            {
                DS4ControlSettings dcs = ds4settingsList[i];
                if (dcs.control == dc)
                {
                    if (shift)
                        return dcs.shiftExtras;
                    else
                        return dcs.extras;
                }
            }

            return null;
        }

        public DS4KeyType GetDS4KeyType(int deviceNum, string buttonName, bool shift)
        {
            DS4Controls dc;
            if (buttonName.StartsWith("bn"))
                dc = getDS4ControlsByName(buttonName);
            else
                dc = (DS4Controls)Enum.Parse(typeof(DS4Controls), buttonName, true);

            List<DS4ControlSettings> ds4settingsList = ds4settings[deviceNum];
            for (int i = 0, settingsLen = ds4settingsList.Count; i < settingsLen; i++)
            {
                DS4ControlSettings dcs = ds4settingsList[i];
                if (dcs.control == dc)
                {
                    if (shift)
                        return dcs.shiftKeyType;
                    else
                        return dcs.keyType;
                }
            }

            return DS4KeyType.None;
        }

        public int GetDS4STrigger(int deviceNum, string buttonName)
        {
            DS4Controls dc;
            if (buttonName.StartsWith("bn"))
                dc = getDS4ControlsByName(buttonName);
            else
                dc = (DS4Controls)Enum.Parse(typeof(DS4Controls), buttonName, true);

            List<DS4ControlSettings> ds4settingsList = ds4settings[deviceNum];
            for (int i = 0, settingsLen = ds4settingsList.Count; i < settingsLen; i++)
            {
                DS4ControlSettings dcs = ds4settingsList[i];
                if (dcs.control == dc)
                    return dcs.shiftTrigger;
            }

            return 0;
        }

        public int GetDS4STrigger(int deviceNum, DS4Controls dc)
        {
            int temp = (int)dc;
            if (temp > 0)
            {
                int index = temp - 1;
                DS4ControlSettings dcs = ds4settings[deviceNum][index];
                return dcs.shiftTrigger;
            }

            return 0;
        }

        public DS4ControlSettings getDS4CSetting(int deviceNum, string buttonName)
        {
            DS4Controls dc;
            if (buttonName.StartsWith("bn"))
                dc = getDS4ControlsByName(buttonName);
            else
                dc = (DS4Controls)Enum.Parse(typeof(DS4Controls), buttonName, true);

            List<DS4ControlSettings> ds4settingsList = ds4settings[deviceNum];
            for (int i = 0, settingsLen = ds4settingsList.Count; i < settingsLen; i++)
            {
                DS4ControlSettings dcs = ds4settingsList[i];
                if (dcs.control == dc)
                    return dcs;
            }

            return null;
        }

        public DS4ControlSettings getDS4CSetting(int deviceNum, DS4Controls dc)
        {
            int temp = (int)dc;
            if (temp > 0)
            {
                int index = temp - 1;
                DS4ControlSettings dcs = ds4settings[deviceNum][index];
                return dcs;
            }

            return null;
        }

        public bool HasCustomActions(int deviceNum)
        {
            List<DS4ControlSettings> ds4settingsList = ds4settings[deviceNum];
            for (int i = 0, settingsLen = ds4settingsList.Count; i < settingsLen; i++)
            {
                DS4ControlSettings dcs = ds4settingsList[i];
                if (dcs.action != null || dcs.shiftAction != null)
                    return true;
            }

            return false;
        }


        public bool HasCustomExtras(int deviceNum)
        {
            List<DS4ControlSettings> ds4settingsList = ds4settings[deviceNum];
            for (int i = 0, settingsLen = ds4settingsList.Count; i < settingsLen; i++)
            {
                DS4ControlSettings dcs = ds4settingsList[i];
                if (dcs.extras != null || dcs.shiftExtras != null)
                    return true;
            }

            return false;
        }

        private void ResetProfile(int device)
        {
            buttonMouseSensitivity[device] = 25;
            flushHIDQueue[device] = false;
            enableTouchToggle[device] = false;
            idleDisconnectTimeout[device] = 0;
            touchpadJitterCompensation[device] = true;
            lowerRCOn[device] = false;
            ledAsBattery[device] = false;
            flashType[device] = 0;
            rumble[device] = 100;
            touchSensitivity[device] = 100;
            l2Deadzone[device] = r2Deadzone[device] = 0;
            LSDeadzone[device] = RSDeadzone[device] = 0;
            LSAntiDeadzone[device] = RSAntiDeadzone[device] = 0;
            LSMaxzone[device] = RSMaxzone[device] = 100;
            l2AntiDeadzone[device] = r2AntiDeadzone[device] = 0;
            l2Maxzone[device] = r2Maxzone[device] = 100;
            LSRotation[device] = 0.0;
            RSRotation[device] = 0.0;
            SXDeadzone[device] = SZDeadzone[device] = 0.02;
            SXMaxzone[device] = SZMaxzone[device] = 1.0;
            SXAntiDeadzone[device] = SZAntiDeadzone[device] = 0.0;
            l2Sens[device] = r2Sens[device] = 1;
            LSSens[device] = RSSens[device] = 1;
            SXSens[device] = SZSens[device] = 1;
            tapSensitivity[device] = 0;
            doubleTap[device] = false;
            scrollSensitivity[device] = 0;
            touchpadInvert[device] = 0;
            rainbow[device] = 0;
            flashAt[device] = 0;
            mouseAccel[device] = true;
            btPollRate[device] = 4;

            m_LowLeds[device] = new DS4Color(Color.Black);

            Color tempColor = Color.Blue;
            switch(device)
            {
                case 0: tempColor = Color.Blue; break;
                case 1: tempColor = Color.Red; break;
                case 2: tempColor = Color.Green; break;
                case 3: tempColor = Color.Pink; break;
                case 4: tempColor = Color.White; break;
                default: tempColor = Color.Blue; break;
            }

            m_Leds[device] = new DS4Color(tempColor);
            m_ChargingLeds[device] = new DS4Color(Color.Black);
            m_FlashLeds[device] = new DS4Color(Color.Black);
            useCustomLeds[device] = false;
            m_CustomLeds[device] = new DS4Color(Color.Blue);

            chargingType[device] = 0;
            launchProgram[device] = string.Empty;
            dinputOnly[device] = false;
            startTouchpadOff[device] = false;
            useTPforControls[device] = false;
            useSAforMouse[device] = false;
            sATriggers[device] = string.Empty;
            touchDisInvertTriggers[device] = new int[1] { -1 };
            lsCurve[device] = rsCurve[device] = 0;
            gyroSensitivity[device] = 100;
            gyroSensVerticalScale[device] = 100;
            gyroInvert[device] = 0;
            gyroTriggerTurns[device] = true;
            gyroSmoothing[device] = false;
            gyroSmoothWeight[device] = 0.5;
            gyroMouseHorizontalAxis[device] = 0;
            lsOutCurveMode[device] = 0;
            rsOutCurveMode[device] = 0;
            l2OutCurveMode[device] = 0;
            r2OutCurveMode[device] = 0;
            sxOutCurveMode[device] = szOutCurveMode[device] = 0;
            trackballMode[device] = false;
            trackballFriction[device] = 10.0;
        }
    }

    public class SpecialAction
    {
        public enum ActionTypeId { None, Key, Program, Profile, Macro, DisconnectBT, BatteryCheck, MultiAction, XboxGameDVR }

        public string name;
        public List<DS4Controls> trigger = new List<DS4Controls>();
        public string type;
        public ActionTypeId typeID;
        public string controls;
        public List<int> macro = new List<int>();
        public string details;
        public List<DS4Controls> uTrigger = new List<DS4Controls>();
        public string ucontrols;
        public double delayTime = 0;
        public string extra;
        public bool pressRelease = false;
        public DS4KeyType keyType;
        public bool tappedOnce = false;
        public bool firstTouch = false;
        public bool secondtouchbegin = false;
        public DateTime pastTime;
        public DateTime firstTap;
        public DateTime TimeofEnd;

        public SpecialAction(string name, string controls, string type, string details, double delay = 0, string extras = "")
        {
            this.name = name;
            this.type = type;
            this.typeID = ActionTypeId.None;
            this.controls = controls;
            delayTime = delay;
            string[] ctrls = controls.Split('/');
            foreach (string s in ctrls)
                trigger.Add(getDS4ControlsByName(s));

            if (type == "Key")
            {
                typeID = ActionTypeId.Key;
                this.details = details.Split(' ')[0];
                if (!string.IsNullOrEmpty(extras))
                {
                    string[] exts = extras.Split('\n');
                    pressRelease = exts[0] == "Release";
                    this.ucontrols = exts[1];
                    string[] uctrls = exts[1].Split('/');
                    foreach (string s in uctrls)
                        uTrigger.Add(getDS4ControlsByName(s));
                }
                if (details.Contains("Scan Code"))
                    keyType |= DS4KeyType.ScanCode;
            }
            else if (type == "Program")
            {
                typeID = ActionTypeId.Program;
                this.details = details;
                if (extras != string.Empty)
                    extra = extras;
            }
            else if (type == "Profile")
            {
                typeID = ActionTypeId.Profile;
                this.details = details;
                if (extras != string.Empty)
                {
                    extra = extras;
                }
            }
            else if (type == "Macro")
            {
                typeID = ActionTypeId.Macro;
                string[] macs = details.Split('/');
                foreach (string s in macs)
                {
                    int v;
                    if (int.TryParse(s, out v))
                        macro.Add(v);
                }
                if (extras.Contains("Scan Code"))
                    keyType |= DS4KeyType.ScanCode;
            }
            else if (type == "DisconnectBT")
            {
                typeID = ActionTypeId.DisconnectBT;
            }
            else if (type == "BatteryCheck")
            {
                typeID = ActionTypeId.BatteryCheck;
                string[] dets = details.Split('|');
                this.details = string.Join(",", dets);
            }
            else if (type == "MultiAction")
            {
                typeID = ActionTypeId.MultiAction;
                this.details = details;
            }
            else if (type == "XboxGameDVR")
            {
                this.typeID = ActionTypeId.XboxGameDVR;
                string[] dets = details.Split(',');
                List<string> macros = new List<string>();
                //string dets = "";
                int typeT = 0;
                for (int i = 0; i < 3; i++)
                {
                    if (int.TryParse(dets[i], out typeT))
                    {
                        switch (typeT)
                        {
                            case 0: macros.Add("91/71/71/91"); break;
                            case 1: macros.Add("91/164/82/82/164/91"); break;
                            case 2: macros.Add("91/164/44/44/164/91"); break;
                            case 3: macros.Add(dets[3] + "/" + dets[3]); break;
                            case 4: macros.Add("91/164/71/71/164/91"); break;
                        }
                    }
                }
                this.type = "MultiAction";
                type = "MultiAction";
                this.details = string.Join(",", macros);
            }
            else
                this.details = details;

            if (type != "Key" && !string.IsNullOrEmpty(extras))
            {
                this.ucontrols = extras;
                string[] uctrls = extras.Split('/');
                foreach (string s in uctrls)
                    uTrigger.Add(getDS4ControlsByName(s));
            }
        }

        private DS4Controls getDS4ControlsByName(string key)
        {
            switch (key)
            {
                case "Share": return DS4Controls.Share;
                case "L3": return DS4Controls.L3;
                case "R3": return DS4Controls.R3;
                case "Options": return DS4Controls.Options;
                case "Up": return DS4Controls.DpadUp;
                case "Right": return DS4Controls.DpadRight;
                case "Down": return DS4Controls.DpadDown;
                case "Left": return DS4Controls.DpadLeft;

                case "L1": return DS4Controls.L1;
                case "R1": return DS4Controls.R1;
                case "Triangle": return DS4Controls.Triangle;
                case "Circle": return DS4Controls.Circle;
                case "Cross": return DS4Controls.Cross;
                case "Square": return DS4Controls.Square;

                case "PS": return DS4Controls.PS;
                case "Left Stick Left": return DS4Controls.LXNeg;
                case "Left Stick Up": return DS4Controls.LYNeg;
                case "Right Stick Left": return DS4Controls.RXNeg;
                case "Right Stick Up": return DS4Controls.RYNeg;

                case "Left Stick Right": return DS4Controls.LXPos;
                case "Left Stick Down": return DS4Controls.LYPos;
                case "Right Stick Right": return DS4Controls.RXPos;
                case "Right Stick Down": return DS4Controls.RYPos;
                case "L2": return DS4Controls.L2;
                case "R2": return DS4Controls.R2;

                case "Left Touch": return DS4Controls.TouchLeft;
                case "Multitouch": return DS4Controls.TouchMulti;
                case "Upper Touch": return DS4Controls.TouchUpper;
                case "Right Touch": return DS4Controls.TouchRight;

                case "Swipe Up": return DS4Controls.SwipeUp;
                case "Swipe Down": return DS4Controls.SwipeDown;
                case "Swipe Left": return DS4Controls.SwipeLeft;
                case "Swipe Right": return DS4Controls.SwipeRight;

                case "Tilt Up": return DS4Controls.GyroZNeg;
                case "Tilt Down": return DS4Controls.GyroZPos;
                case "Tilt Left": return DS4Controls.GyroXPos;
                case "Tilt Right": return DS4Controls.GyroXNeg;
            }

            return 0;
        }
    }
}
