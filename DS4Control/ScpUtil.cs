using System;
using System.Collections.Generic;
using System.Text;

using System.IO;
using System.Reflection;
using System.Xml;
using System.Drawing;
using DS4Library;
namespace DS4Control
{
    [Flags]
    public enum DS4KeyType : byte { None = 0, ScanCode = 1, Toggle = 2, Unbound = 4, Macro = 8 }; //Increment by exponents of 2*, starting at 2^0
    public enum Ds3PadId : byte { None = 0xFF, One = 0x00, Two = 0x01, Three = 0x02, Four = 0x03, All = 0x04 };
    public enum DS4Controls : byte { None, LXNeg, LXPos, LYNeg, LYPos, RXNeg, RXPos, RYNeg, RYPos, L1, L2, L3, R1, R2, R3, Square, Triangle, Circle, Cross, DpadUp, DpadRight, DpadDown, DpadLeft, PS, TouchLeft, TouchUpper, TouchMulti, TouchRight, Share, Options };
    public enum X360Controls : byte { None, LXNeg, LXPos, LYNeg, LYPos, RXNeg, RXPos, RYNeg, RYPos, LB, LT, LS, RB, RT, RS, X, Y, B, A, DpadUp, DpadRight, DpadDown, DpadLeft, Guide, Back, Start, LeftMouse, RightMouse, MiddleMouse, FourthMouse, FifthMouse, WUP, WDOWN, MouseUp, MouseDown, MouseLeft, MouseRight, Unbound };

    public class DebugEventArgs : EventArgs
    {
        protected DateTime m_Time = DateTime.Now;
        protected String m_Data = String.Empty;

        public DebugEventArgs(String Data)
        {
            m_Data = Data;
        }

        public DateTime Time
        {
            get { return m_Time; }
        }

        public String Data
        {
            get { return m_Data; }
        }
    }

    public class MappingDoneEventArgs : EventArgs
    {
        protected int deviceNum = -1;

        public MappingDoneEventArgs(int DeviceID)
        {
            deviceNum = DeviceID;
        }

        public int DeviceID
        {
            get { return deviceNum; }
        }
    }

    public class ReportEventArgs : EventArgs
    {
        protected Ds3PadId m_Pad = Ds3PadId.None;
        protected Byte[] m_Report = new Byte[64];

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

    public class Global
    {
        protected static BackingStore m_Config = new BackingStore();
        protected static Int32 m_IdleTimeout = 600000;
        //public static string appdatapath = Directory.GetParent(Assembly.GetExecutingAssembly().Location).FullName;
        public static string appdatapath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\DS4Tool";
        public static event EventHandler<EventArgs> ControllerStatusChange; // called when a controller is added/removed/battery or touchpad mode changes/etc.
        public static void ControllerStatusChanged(object sender)
        {
            if (ControllerStatusChange != null)
                ControllerStatusChange(sender, EventArgs.Empty);
        }
        public static void setButtonMouseSensitivity(int device, int data)
        {
            m_Config.buttonMouseSensitivity[device] = data;
        }
        public static int getButtonMouseSensitivity(int device)
        {
            return m_Config.buttonMouseSensitivity[device];
        }
        public static DS4Color loadColor(int device)
        {
            DS4Color color = new DS4Color();
            color.red = m_Config.m_Leds[device][0];
            color.green = m_Config.m_Leds[device][1];
            color.blue = m_Config.m_Leds[device][2];
            return color;
        }
        public static void saveColor(int device, byte red, byte green, byte blue)
        {
            m_Config.m_Leds[device][0] = red;
            m_Config.m_Leds[device][1] = green;
            m_Config.m_Leds[device][2] = blue;
        }

        public static byte loadRumbleBoost(int device)
        {
            return m_Config.m_Rumble[device];
        }
        public static void saveRumbleBoost(int device, byte boost)
        {
            m_Config.m_Rumble[device] = boost;

        }
        public static double getRainbow(int device)
        {
            return m_Config.rainbow[device];
        }
        public static void setRainbow(int device, double speed)
        {
            m_Config.rainbow[device] = speed;
        }
        public static bool getFlushHIDQueue(int device)
        {
            return m_Config.flushHIDQueue[device];
        }
        public static void setFlushHIDQueue(int device, bool setting)
        {
            m_Config.flushHIDQueue[device] = setting;
        }

        public static int getIdleDisconnectTimeout(int device)
        {
            return m_Config.idleDisconnectTimeout[device];
        }
        public static void setIdleDisconnectTimeout(int device, int seconds)
        {
            m_Config.idleDisconnectTimeout[device] = seconds;
        }

        public static byte getTouchSensitivity(int device)
        {
            return m_Config.touchSensitivity[device];
        }
        public static void setTouchSensitivity(int device, byte sen)
        {
            m_Config.touchSensitivity[device] = sen;
        }       

        public static void setFlashWhenLowBattery(int device, bool flash)
        {
            m_Config.flashLedLowBattery[device] = flash;

        }
        public static bool getFlashWhenLowBattery(int device)
        {
            return m_Config.flashLedLowBattery[device];

        }

        public static void setFlashAt(int device, int when)
        {
            m_Config.flashAt[device] = when;

        }
        public static int getFlashAt(int device)
        {
            return m_Config.flashAt[device];

        }

        public static void setLedAsBatteryIndicator(int device, bool ledAsBattery)
        {
            m_Config.ledAsBattery[device] = ledAsBattery;

        }
        public static bool getLedAsBatteryIndicator(int device)
        {
            return m_Config.ledAsBattery[device];
        }

        public static void setChargingType(int device, int type)
        {
            m_Config.chargingType[device] = type;

        }
        public static int getChargingType(int device)
        {
            return m_Config.chargingType[device];

        }

        public static void setUseExclusiveMode(bool exclusive)
        {
            m_Config.useExclusiveMode = exclusive;
        }
        public static bool getUseExclusiveMode()
        {
            return m_Config.useExclusiveMode;
        }

        public static void setVersion(double data)
        {
            m_Config.version = data;
        }

        public static double getVersion()
        {
            return m_Config.version;
        }

        public static void setLastChecked(DateTime data)
        {
            m_Config.lastChecked = data;
        }

        public static DateTime getLastChecked()
        {
            return m_Config.lastChecked;
        }

        public static void setCheckWhen(int data)
        {
            m_Config.CheckWhen = data;
        }

        public static int getCheckWhen()
        {
            return m_Config.CheckWhen;
        }

        public static void setNotifications(bool data)
        {
            m_Config.notifications = data;
        }

        public static bool getNotifications()
        {
            return m_Config.notifications;
        }
        // New settings
        public static void saveLowColor(int device, byte red, byte green, byte blue)
        {
            m_Config.m_LowLeds[device][0] = red;
            m_Config.m_LowLeds[device][1] = green;
            m_Config.m_LowLeds[device][2] = blue;
        }
        public static DS4Color loadLowColor(int device)
        {
            DS4Color color = new DS4Color();
            color.red = m_Config.m_LowLeds[device][0];
            color.green = m_Config.m_LowLeds[device][1];
            color.blue = m_Config.m_LowLeds[device][2];
            return color;
        }
        public static void saveChargingColor(int device, byte red, byte green, byte blue)
        {
            m_Config.m_ChargingLeds[device][0] = red;
            m_Config.m_ChargingLeds[device][1] = green;
            m_Config.m_ChargingLeds[device][2] = blue;
        }
        public static DS4Color loadChargingColor(int device)
        {
            DS4Color color = new DS4Color();
            color.red = m_Config.m_ChargingLeds[device][0];
            color.green = m_Config.m_ChargingLeds[device][1];
            color.blue = m_Config.m_ChargingLeds[device][2];
            return color;
        }
        public static void setTapSensitivity(int device, byte sen)
        {
            m_Config.tapSensitivity[device] = sen;
        }
        public static byte getTapSensitivity(int device)
        {
            return m_Config.tapSensitivity[device];
        }
        public static void setDoubleTap(int device, bool on)
        {
            m_Config.doubleTap[device] = on;
        }
        public static bool getDoubleTap(int device)
        {
            return m_Config.doubleTap[device];
        }
        public static bool getTap(int device)
        {
            if (m_Config.tapSensitivity[device] == 0)
                return false;
            else
                return true;
        }
        public static void setScrollSensitivity(int device, int sen)
        {
            m_Config.scrollSensitivity[device] = sen;
        }
        public static int getScrollSensitivity(int device)
        {
            return m_Config.scrollSensitivity[device];
        }
        public static void setLowerRCOn(int device, bool twoFingerRC)
        {
            m_Config.lowerRCOn[device] = twoFingerRC;
        }
        public static bool getLowerRCOn(int device)
        {
            return m_Config.lowerRCOn[device];
        }
        public static void setTouchpadJitterCompensation(int device, bool enabled)
        {
            m_Config.touchpadJitterCompensation[device] = enabled;
        }
        public static bool getTouchpadJitterCompensation(int device)
        {
            return m_Config.touchpadJitterCompensation[device];
        }
        public static void setStartMinimized(bool startMinimized)
        {
            m_Config.startMinimized = startMinimized;
        }
        public static bool getStartMinimized()
        {
            return m_Config.startMinimized;
        }
        public static void setFormWidth(int size)
        {
            m_Config.formWidth = size;
        }
        public static int getFormWidth()
        {
            return m_Config.formWidth;
        }
        public static void setFormHeight(int size)
        {
            m_Config.formHeight = size;
        }
        public static int getFormHeight()
        {
            return m_Config.formHeight;
        }

        public static double getLeftTriggerMiddle(int device)
        {
            return m_Config.m_LeftTriggerMiddle[device];
        }
        public static void setLeftTriggerMiddle(int device, byte value)
        {
            m_Config.m_LeftTriggerMiddle[device] = value;
        }
        public static double getRightTriggerMiddle(int device)
        {
            return m_Config.m_RightTriggerMiddle[device];
        }
        public static void setRightTriggerMiddle(int device, byte value)
        {
            m_Config.m_RightTriggerMiddle[device] = value;
        }
        public static byte getLSDeadzone(int device)
        {
            return m_Config.LSDeadzone[device];
        }
        public static void setLSDeadzone(int device, byte value)
        {
            m_Config.LSDeadzone[device] = value;
        }
        public static byte getRSDeadzone(int device)
        {
            return m_Config.RSDeadzone[device];
        }
        public static void setRSDeadzone(int device, byte value)
        {
            m_Config.RSDeadzone[device] = value;
        }
        public static void setAProfile(int device, string filepath)
        {
            m_Config.profilePath[device] = appdatapath + @"\Profiles\" + filepath + ".xml";
        }
        public static string getAProfile(int device)
        {
            return m_Config.profilePath[device];
        }
        public static X360Controls getCustomButton(int device, DS4Controls controlName)
        {
            return m_Config.GetCustomButton(device, controlName);
        }
        public static ushort getCustomKey(int device, DS4Controls controlName)
        {
            return m_Config.GetCustomKey(device, controlName);
        }
        public static string getCustomMacro(int device, DS4Controls controlName)
        {
            return m_Config.GetCustomMacro(device, controlName);
        }
        public static DS4KeyType getCustomKeyType(int device, DS4Controls controlName)
        {
            return m_Config.GetCustomKeyType(device, controlName);
        }
        public static bool getHasCustomKeysorButtons(int device)
        {
            return m_Config.customMapButtons[device].Count > 0
                || m_Config.customMapKeys[device].Count > 0;
        }
        public static Dictionary<DS4Controls, X360Controls> getCustomButtons(int device)
        {
            return m_Config.customMapButtons[device];
        }
        public static Dictionary<DS4Controls, ushort> getCustomKeys(int device)
        {
            return m_Config.customMapKeys[device];
        }
        public static Dictionary<DS4Controls, string> getCustomMacros(int device)
        {
            return m_Config.customMapMacros[device];
        }
        public static Dictionary<DS4Controls, DS4KeyType> getCustomKeyTypes(int device)
        {
            return m_Config.customMapKeyTypes[device];
        }
        public static bool Load()
        {
            return m_Config.Load();
        }
        public static void LoadProfile(int device, System.Windows.Forms.Control[] buttons)
        {
            m_Config.LoadProfile(device, buttons);
        }
        public static void LoadProfile(int device)
        {
            m_Config.LoadProfile(device);
        }
        public static void LoadTempProfile(int device, string name)
        {
            m_Config.LoadProfile(device, appdatapath + @"\Profiles\" + name + ".xml");
        }
        public static void Save()
        {
            m_Config.Save();
        }

        public static void SaveProfile(int device, string propath, System.Windows.Forms.Control[] buttons)
        {
            m_Config.SaveProfile(device, propath, buttons);
        }

        private static byte applyRatio(byte b1, byte b2, double r)
        {
            if (r > 100)
                r = 100;
            else if (r < 0)
                r = 0;
            uint ratio = (uint)r;
            if (b1 > b2)
            {
                ratio = 100 - (uint)r;
            }
            byte bmax = Math.Max(b1, b2);
            byte bmin = Math.Min(b1, b2);
            byte bdif = (byte)(bmax - bmin);
            return (byte)(bmin + (bdif * ratio / 100));
        }
        public static DS4Color getTransitionedColor(DS4Color c1, DS4Color c2, double ratio)
        {;
        Color cs = Color.FromArgb(c1.red, c1.green, c1.blue);
            c1.red = applyRatio(c1.red, c2.red, ratio);
            c1.green = applyRatio(c1.green, c2.green, ratio);
            c1.blue = applyRatio(c1.blue, c2.blue, ratio);
            return c1;
        }

        /*public static DS4Color getTransitionedColor(DS4Color lowcolor, DS4Color maxcolor, uint ratio)
        {
            Color cs = Color.FromArgb(lowcolor.red, lowcolor.green, lowcolor.blue);
            Color cs2 = Color.FromArgb(maxcolor.red, maxcolor.green, maxcolor.blue);
            Color csR = applyRatio(cs, cs2, ratio);
            DS4Color dr = new DS4Color { red = csR.R, green = csR.G, blue = csR.B };
            return dr;
        }*/
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
            {   R = C; G = X; B = 0;}
            else if (60 <= hue && hue < 120)
            {R = X; G = C; B = 0; }
            else if (120 <= hue && hue < 180)
            { R = 0; G = C; B = X; }
            else if (180 <= hue && hue < 240)
            { R = 0; G = X; B = C; }
            else if (240 <= hue && hue < 300)
                { R = X; G = 0; B = C; }
            else if (300 <= hue && hue < 360)
                { R = C; G = 0; B = X; }
            else
                { R = 255; G = 0; B = 0; }
            R += m; G += m; B += m;
            R *= 255; G *= 255; B *= 255;
            return Color.FromArgb((int)R, (int)G, (int)B);
        }
    }



    public class BackingStore
    {
        protected String m_Profile = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\DS4Tool" + "\\Profiles.xml";
        protected XmlDocument m_Xdoc = new XmlDocument();
        //fifth value used to for options, not fifth controller
        public int[] buttonMouseSensitivity = { 25, 25, 25, 25, 25 };
        
        public Boolean[] touchpadJitterCompensation = {true, true, true, true, true};
        public Boolean[] lowerRCOn = { false, false, false, false, false };
        public Boolean[] ledAsBattery = { false, false, false, false, false };
        public Boolean[] flashLedLowBattery = { false, false, false, false, false };
        public Byte[] m_LeftTriggerMiddle = { 0, 0, 0, 0, 0}, m_RightTriggerMiddle = { 0, 0, 0, 0, 0};
        public String[] profilePath = { String.Empty, String.Empty, String.Empty, String.Empty, String.Empty };
        public Byte[] m_Rumble = { 100, 100, 100, 100, 100 };
        public Byte[] touchSensitivity = { 100, 100, 100, 100, 100 };
        public Byte[] LSDeadzone = { 0, 0, 0, 0, 0 }, RSDeadzone = { 0, 0, 0, 0, 0};
        public Byte[] tapSensitivity = { 0, 0, 0, 0, 0 };
        public bool[] doubleTap = { false, false, false, false, false };
        public int[] scrollSensitivity = { 0, 0, 0, 0, 0 };
        public double[] rainbow = { 0, 0, 0, 0, 0 };
        public int[] flashAt = { 30, 30, 30, 30, 30 };
        public Byte[][] m_LowLeds = new Byte[][]
        {
            new Byte[] {0,0,0},
            new Byte[] {0,0,0},
            new Byte[] {0,0,0},
            new Byte[] {0,0,0},
            new Byte[] {0,0,0}
        };
        public Byte[][] m_Leds = new Byte[][]
        {
            new Byte[] {0,0,255},
            new Byte[] {255,0,0},
            new Byte[] {0,255,0},
            new Byte[] {255,0,255},
            new Byte[] {255,255,255}
        };
        public Byte[][] m_ChargingLeds = new Byte[][]
        {
            new Byte[] {0,0,0},
            new Byte[] {0,0,0},
            new Byte[] {0,0,0},
            new Byte[] {0,0,0},
            new Byte[] {0,0,0}
        };
        public int[] chargingType = { 0, 0, 0, 0, 0 };
        public bool[] flushHIDQueue = { true, true, true, true, true };
        public int[] idleDisconnectTimeout = { 0, 0, 0, 0, 0 };

        public Boolean useExclusiveMode = false;
        public Int32 formWidth = 782;
        public Int32 formHeight = 550;
        public Boolean startMinimized = false;
        public double version;
        public DateTime lastChecked;
        public int CheckWhen = 1;
        public bool notifications = true;
        public Dictionary<DS4Controls, DS4KeyType>[] customMapKeyTypes = { null, null, null, null, null };
        public Dictionary<DS4Controls, UInt16>[] customMapKeys = { null, null, null, null, null };
        public Dictionary<DS4Controls, String>[] customMapMacros = { null, null, null, null, null };
        public Dictionary<DS4Controls, X360Controls>[] customMapButtons = { null, null, null, null, null };
        public BackingStore()
        {
            for (int i = 0; i < 5; i++)
            {
                customMapKeyTypes[i] = new Dictionary<DS4Controls, DS4KeyType>();
                customMapKeys[i] = new Dictionary<DS4Controls, UInt16>();
                customMapMacros[i] = new Dictionary<DS4Controls, String>();
                customMapButtons[i] = new Dictionary<DS4Controls, X360Controls>();
            }
        }

        public X360Controls GetCustomButton(int device, DS4Controls controlName)
        {
            if (customMapButtons[device].ContainsKey(controlName))
                return customMapButtons[device][controlName];
            else return X360Controls.None;
        }
        public UInt16 GetCustomKey(int device, DS4Controls controlName)
        {
            if (customMapKeys[device].ContainsKey(controlName))
                return customMapKeys[device][controlName];
            else return 0;
        }
        public string GetCustomMacro(int device, DS4Controls controlName)
        {
            if (customMapMacros[device].ContainsKey(controlName))
                return customMapMacros[device][controlName];
            else return "0";
        }
        public DS4KeyType GetCustomKeyType(int device, DS4Controls controlName)
        {
            try
            {
                if (customMapKeyTypes[device].ContainsKey(controlName))
                    return customMapKeyTypes[device][controlName];
                else return 0;
            }
            catch { return 0; }
        }


        public Boolean SaveProfile(int device, String propath, System.Windows.Forms.Control[] buttons)
        {
            Boolean Saved = true;
            String path = Global.appdatapath + @"\Profiles\" + propath + ".xml";
            try
            {
                XmlNode Node;

                m_Xdoc.RemoveAll();

                Node = m_Xdoc.CreateXmlDeclaration("1.0", "utf-8", String.Empty);
                m_Xdoc.AppendChild(Node);

                Node = m_Xdoc.CreateComment(String.Format(" ScpControl Configuration Data. {0} ", DateTime.Now));
                m_Xdoc.AppendChild(Node);

                Node = m_Xdoc.CreateWhitespace("\r\n");
                m_Xdoc.AppendChild(Node);

                Node = m_Xdoc.CreateNode(XmlNodeType.Element, "ScpControl", null);

                XmlNode xmlFlushHIDQueue = m_Xdoc.CreateNode(XmlNodeType.Element, "flushHIDQueue", null); xmlFlushHIDQueue.InnerText = flushHIDQueue[device].ToString(); Node.AppendChild(xmlFlushHIDQueue);
                XmlNode xmlIdleDisconnectTimeout = m_Xdoc.CreateNode(XmlNodeType.Element, "idleDisconnectTimeout", null); xmlIdleDisconnectTimeout.InnerText = idleDisconnectTimeout[device].ToString(); Node.AppendChild(xmlIdleDisconnectTimeout);
                XmlNode xmlRed = m_Xdoc.CreateNode(XmlNodeType.Element, "Red", null); xmlRed.InnerText = m_Leds[device][0].ToString(); Node.AppendChild(xmlRed);
                XmlNode xmlGreen = m_Xdoc.CreateNode(XmlNodeType.Element, "Green", null); xmlGreen.InnerText = m_Leds[device][1].ToString(); Node.AppendChild(xmlGreen);
                XmlNode xmlBlue = m_Xdoc.CreateNode(XmlNodeType.Element, "Blue", null); xmlBlue.InnerText = m_Leds[device][2].ToString(); Node.AppendChild(xmlBlue);
                XmlNode xmlRumbleBoost = m_Xdoc.CreateNode(XmlNodeType.Element, "RumbleBoost", null); xmlRumbleBoost.InnerText = m_Rumble[device].ToString(); Node.AppendChild(xmlRumbleBoost);
                XmlNode xmlLedAsBatteryIndicator = m_Xdoc.CreateNode(XmlNodeType.Element, "ledAsBatteryIndicator", null); xmlLedAsBatteryIndicator.InnerText = ledAsBattery[device].ToString(); Node.AppendChild(xmlLedAsBatteryIndicator);
                XmlNode xmlLowBatteryFlash = m_Xdoc.CreateNode(XmlNodeType.Element, "lowBatteryFlash", null); xmlLowBatteryFlash.InnerText = flashLedLowBattery[device].ToString(); Node.AppendChild(xmlLowBatteryFlash);
                XmlNode xmlFlashBatterAt = m_Xdoc.CreateNode(XmlNodeType.Element, "flashBatteryAt", null); xmlFlashBatterAt.InnerText = flashAt[device].ToString(); Node.AppendChild(xmlFlashBatterAt);
                XmlNode xmlTouchSensitivity = m_Xdoc.CreateNode(XmlNodeType.Element, "touchSensitivity", null); xmlTouchSensitivity.InnerText = touchSensitivity[device].ToString(); Node.AppendChild(xmlTouchSensitivity);
                XmlNode xmlLowRed = m_Xdoc.CreateNode(XmlNodeType.Element, "LowRed", null); xmlLowRed.InnerText = m_LowLeds[device][0].ToString(); Node.AppendChild(xmlLowRed);
                XmlNode xmlLowGreen = m_Xdoc.CreateNode(XmlNodeType.Element, "LowGreen", null); xmlLowGreen.InnerText = m_LowLeds[device][1].ToString(); Node.AppendChild(xmlLowGreen);
                XmlNode xmlLowBlue = m_Xdoc.CreateNode(XmlNodeType.Element, "LowBlue", null); xmlLowBlue.InnerText = m_LowLeds[device][2].ToString(); Node.AppendChild(xmlLowBlue);
                XmlNode xmlChargingRed = m_Xdoc.CreateNode(XmlNodeType.Element, "ChargingRed", null); xmlChargingRed.InnerText = m_ChargingLeds[device][0].ToString(); Node.AppendChild(xmlChargingRed);
                XmlNode xmlChargingGreen = m_Xdoc.CreateNode(XmlNodeType.Element, "ChargingGreen", null); xmlChargingGreen.InnerText = m_ChargingLeds[device][1].ToString(); Node.AppendChild(xmlChargingGreen);
                XmlNode xmlChargingBlue = m_Xdoc.CreateNode(XmlNodeType.Element, "ChargingBlue", null); xmlChargingBlue.InnerText = m_ChargingLeds[device][2].ToString(); Node.AppendChild(xmlChargingBlue);
                XmlNode xmlTouchpadJitterCompensation = m_Xdoc.CreateNode(XmlNodeType.Element, "touchpadJitterCompensation", null); xmlTouchpadJitterCompensation.InnerText = touchpadJitterCompensation[device].ToString(); Node.AppendChild(xmlTouchpadJitterCompensation);
                XmlNode xmlLowerRCOn = m_Xdoc.CreateNode(XmlNodeType.Element, "lowerRCOn", null); xmlLowerRCOn.InnerText = lowerRCOn[device].ToString(); Node.AppendChild(xmlLowerRCOn);
                XmlNode xmlTapSensitivity = m_Xdoc.CreateNode(XmlNodeType.Element, "tapSensitivity", null); xmlTapSensitivity.InnerText = tapSensitivity[device].ToString(); Node.AppendChild(xmlTapSensitivity);
                XmlNode xmlDouble = m_Xdoc.CreateNode(XmlNodeType.Element, "doubleTap", null); xmlDouble.InnerText = doubleTap[device].ToString(); Node.AppendChild(xmlDouble);
                XmlNode xmlScrollSensitivity = m_Xdoc.CreateNode(XmlNodeType.Element, "scrollSensitivity", null); xmlScrollSensitivity.InnerText = scrollSensitivity[device].ToString(); Node.AppendChild(xmlScrollSensitivity);
                XmlNode xmlLeftTriggerMiddle = m_Xdoc.CreateNode(XmlNodeType.Element, "LeftTriggerMiddle", null); xmlLeftTriggerMiddle.InnerText = m_LeftTriggerMiddle[device].ToString(); Node.AppendChild(xmlLeftTriggerMiddle);
                XmlNode xmlRightTriggerMiddle = m_Xdoc.CreateNode(XmlNodeType.Element, "RightTriggerMiddle", null); xmlRightTriggerMiddle.InnerText = m_RightTriggerMiddle[device].ToString(); Node.AppendChild(xmlRightTriggerMiddle);
                XmlNode xmlButtonMouseSensitivity = m_Xdoc.CreateNode(XmlNodeType.Element, "ButtonMouseSensitivity", null); xmlButtonMouseSensitivity.InnerText = buttonMouseSensitivity[device].ToString(); Node.AppendChild(xmlButtonMouseSensitivity);
                XmlNode xmlRainbow = m_Xdoc.CreateNode(XmlNodeType.Element, "Rainbow", null); xmlRainbow.InnerText = rainbow[device].ToString(); Node.AppendChild(xmlRainbow);
                XmlNode xmlLSD = m_Xdoc.CreateNode(XmlNodeType.Element, "LSDeadZone", null); xmlLSD.InnerText = LSDeadzone[device].ToString(); Node.AppendChild(xmlLSD);
                XmlNode xmlRSD = m_Xdoc.CreateNode(XmlNodeType.Element, "RSDeadZone", null); xmlRSD.InnerText = RSDeadzone[device].ToString(); Node.AppendChild(xmlRSD);
                XmlNode xmlChargingType = m_Xdoc.CreateNode(XmlNodeType.Element, "ChargingType", null); xmlChargingType.InnerText = chargingType[device].ToString(); Node.AppendChild(xmlChargingType);
                XmlNode NodeControl = m_Xdoc.CreateNode(XmlNodeType.Element, "Control", null);

                XmlNode Key = m_Xdoc.CreateNode(XmlNodeType.Element, "Key", null);
                XmlNode Macro = m_Xdoc.CreateNode(XmlNodeType.Element, "Macro", null);
                XmlNode KeyType = m_Xdoc.CreateNode(XmlNodeType.Element, "KeyType", null);
                XmlNode Button = m_Xdoc.CreateNode(XmlNodeType.Element, "Button", null);

                foreach (var button in buttons)
                   // try
                    {
                        // Save even if string (for xbox controller buttons)
                        if (button.Tag != null)
                        {
                            XmlNode buttonNode;
                            string keyType = String.Empty;
                            if (button.Tag is String && (String)button.Tag == "Unbound")
                            {
                                keyType += DS4KeyType.Unbound;
                            }
                            {
                                if (button.Font.Underline)
                                    keyType += DS4KeyType.Macro;
                                if (button.Font.Italic)
                                    keyType += DS4KeyType.Toggle;
                                if (button.Font.Bold)
                                    keyType += DS4KeyType.ScanCode;
                            }
                            if (keyType != String.Empty)
                            {
                                buttonNode = m_Xdoc.CreateNode(XmlNodeType.Element, button.Name, null);
                                buttonNode.InnerText = keyType;
                                KeyType.AppendChild(buttonNode);
                            }
                            buttonNode = m_Xdoc.CreateNode(XmlNodeType.Element, button.Name, null);
                            buttonNode.InnerText = button.Tag.ToString();
                            if (button.Tag is IEnumerable<int> || button.Tag is Int32[] || button.Tag is UInt16[])
                            {
                                buttonNode.InnerText = string.Join("/", (int[])button.Tag);
                                Macro.AppendChild(buttonNode);
                            }
                            else if (button.Tag is Int32 || button.Tag is UInt16)
                                Key.AppendChild(buttonNode);
                            else Button.AppendChild(buttonNode);
                        }
                    }
                    //catch
                   // {
                    //    NodeControl.InnerText = "";
                    //}
                Node.AppendChild(NodeControl);
                if (Button.HasChildNodes)
                    NodeControl.AppendChild(Button);
                if (Macro.HasChildNodes)
                    NodeControl.AppendChild(Macro);
                if (Key.HasChildNodes)
                    NodeControl.AppendChild(Key);
                if (KeyType.HasChildNodes)
                    NodeControl.AppendChild(KeyType);
                m_Xdoc.AppendChild(Node);
                if (NodeControl.HasChildNodes)
                    Node.AppendChild(NodeControl);
                m_Xdoc.Save(path);
            }
            catch { Saved = false; }

            return Saved;
        }
        private DS4Controls getDS4ControlsByName(string key)
        {
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
            }
            return 0;
        }
        private X360Controls getX360ControlsByName(string key)
        {
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
        public Boolean LoadProfile(int device, System.Windows.Forms.Control[] buttons)
        {
            Boolean Loaded = true;
            Dictionary<DS4Controls, DS4KeyType> customMapKeyTypes = new Dictionary<DS4Controls, DS4KeyType>();
            Dictionary<DS4Controls, UInt16> customMapKeys = new Dictionary<DS4Controls, UInt16>();
            Dictionary<DS4Controls, X360Controls> customMapButtons = new Dictionary<DS4Controls, X360Controls>();
            Dictionary<DS4Controls, String> customMapMacros = new Dictionary<DS4Controls, String>();
            Boolean missingSetting = false;

            try
            {
                if (File.Exists(profilePath[device]))
                {
                    XmlNode Item;

                    m_Xdoc.Load(profilePath[device]);

                    try { Item = m_Xdoc.SelectSingleNode("/ScpControl/flushHIDQueue"); Boolean.TryParse(Item.InnerText, out flushHIDQueue[device]); }
                    catch { missingSetting = true; }

                    try { Item = m_Xdoc.SelectSingleNode("/ScpControl/idleDisconnectTimeout"); Int32.TryParse(Item.InnerText, out idleDisconnectTimeout[device]); }
                    catch { missingSetting = true; }

                    try { Item = m_Xdoc.SelectSingleNode("/ScpControl/Red"); Byte.TryParse(Item.InnerText, out m_Leds[device][0]); }
                    catch { missingSetting = true; }

                    try { Item = m_Xdoc.SelectSingleNode("/ScpControl/Green"); Byte.TryParse(Item.InnerText, out m_Leds[device][1]); }
                    catch { missingSetting = true; }

                    try { Item = m_Xdoc.SelectSingleNode("/ScpControl/Blue"); Byte.TryParse(Item.InnerText, out m_Leds[device][2]); }
                    catch { missingSetting = true; }

                    try { Item = m_Xdoc.SelectSingleNode("/ScpControl/RumbleBoost"); Byte.TryParse(Item.InnerText, out m_Rumble[device]); }
                    catch { missingSetting = true; }

                    try { Item = m_Xdoc.SelectSingleNode("/ScpControl/ledAsBatteryIndicator"); Boolean.TryParse(Item.InnerText, out ledAsBattery[device]); }
                    catch { missingSetting = true; }

                    try { Item = m_Xdoc.SelectSingleNode("/ScpControl/lowBatteryFlash"); Boolean.TryParse(Item.InnerText, out flashLedLowBattery[device]); }
                    catch { missingSetting = true; }

                    try { Item = m_Xdoc.SelectSingleNode("/ScpControl/flashBatteryAt"); Int32.TryParse(Item.InnerText, out flashAt[device]); }
                    catch { missingSetting = true; }

                    try { Item = m_Xdoc.SelectSingleNode("/ScpControl/touchSensitivity"); Byte.TryParse(Item.InnerText, out touchSensitivity[device]); }
                    catch { missingSetting = true; }

                    try { Item = m_Xdoc.SelectSingleNode("/ScpControl/LowRed"); Byte.TryParse(Item.InnerText, out m_LowLeds[device][0]); }
                    catch { missingSetting = true; }
                    try { Item = m_Xdoc.SelectSingleNode("/ScpControl/LowGreen"); Byte.TryParse(Item.InnerText, out m_LowLeds[device][1]); }
                    catch { missingSetting = true; }
                    try { Item = m_Xdoc.SelectSingleNode("/ScpControl/LowBlue"); Byte.TryParse(Item.InnerText, out m_LowLeds[device][2]); }
                    catch { missingSetting = true; }
                    try { Item = m_Xdoc.SelectSingleNode("/ScpControl/ChargingRed"); Byte.TryParse(Item.InnerText, out m_ChargingLeds[device][0]); }
                    catch { missingSetting = true; }
                    try { Item = m_Xdoc.SelectSingleNode("/ScpControl/ChargingGreen"); Byte.TryParse(Item.InnerText, out m_ChargingLeds[device][1]); }
                    catch { missingSetting = true; }
                    try { Item = m_Xdoc.SelectSingleNode("/ScpControl/ChargingBlue"); Byte.TryParse(Item.InnerText, out m_ChargingLeds[device][2]); }
                    catch { missingSetting = true; }
                    try { Item = m_Xdoc.SelectSingleNode("/ScpControl/touchpadJitterCompensation"); Boolean.TryParse(Item.InnerText, out touchpadJitterCompensation[device]); }
                    catch { missingSetting = true; }
                    try { Item = m_Xdoc.SelectSingleNode("/ScpControl/lowerRCOn"); Boolean.TryParse(Item.InnerText, out lowerRCOn[device]); }
                    catch { missingSetting = true; }
                    try { Item = m_Xdoc.SelectSingleNode("/ScpControl/tapSensitivity"); Byte.TryParse(Item.InnerText, out tapSensitivity[device]); }
                    catch { missingSetting = true; }
                    try { Item = m_Xdoc.SelectSingleNode("/ScpControl/doubleTap"); Boolean.TryParse(Item.InnerText, out doubleTap[device]); }
                    catch { missingSetting = true; }
                    try { Item = m_Xdoc.SelectSingleNode("/ScpControl/scrollSensitivity"); Int32.TryParse(Item.InnerText, out scrollSensitivity[device]); }
                    catch { missingSetting = true; }
                    try { Item = m_Xdoc.SelectSingleNode("/ScpControl/LeftTriggerMiddle"); Byte.TryParse(Item.InnerText, out m_LeftTriggerMiddle[device]); }
                    catch { missingSetting = true; }
                    try { Item = m_Xdoc.SelectSingleNode("/ScpControl/RightTriggerMiddle"); Byte.TryParse(Item.InnerText, out m_RightTriggerMiddle[device]); }
                    catch { missingSetting = true; }
                    try { Item = m_Xdoc.SelectSingleNode("/ScpControl/ButtonMouseSensitivity"); Int32.TryParse(Item.InnerText, out buttonMouseSensitivity[device]); }
                    catch { missingSetting = true; }
                    try { Item = m_Xdoc.SelectSingleNode("/ScpControl/Rainbow"); Double.TryParse(Item.InnerText, out rainbow[device]); }
                    catch { rainbow[device] = 0; missingSetting = true; }
                    try { Item = m_Xdoc.SelectSingleNode("/ScpControl/LSDeadZone"); Byte.TryParse(Item.InnerText, out LSDeadzone[device]); }
                    catch { missingSetting = true; }
                    try { Item = m_Xdoc.SelectSingleNode("/ScpControl/RSDeadZone"); Byte.TryParse(Item.InnerText, out RSDeadzone[device]); }
                    catch { missingSetting = true; }
                    try { Item = m_Xdoc.SelectSingleNode("/ScpControl/ChargingType"); Int32.TryParse(Item.InnerText, out chargingType[device]); }
                    catch { missingSetting = true; }

                    DS4KeyType keyType;
                    UInt16 wvk;
                    foreach (var button in buttons)
                        try
                        {
                            //bool foundBinding = false;
                            Item = m_Xdoc.SelectSingleNode(String.Format("/ScpControl/Control/KeyType/{0}", button.Name));
                            if (Item != null)
                            {
                                //foundBinding = true;
                                keyType = DS4KeyType.None;
                                if (Item.InnerText.Contains(DS4KeyType.Unbound.ToString()))
                                {
                                    keyType = DS4KeyType.Unbound;
                                    button.Tag = "Unbound";
                                    button.Text = "Unbound";
                                }
                                else
                                {
                                    bool SC = Item.InnerText.Contains(DS4KeyType.ScanCode.ToString());
                                    bool TG = Item.InnerText.Contains(DS4KeyType.Toggle.ToString());
                                    bool MC = Item.InnerText.Contains(DS4KeyType.Macro.ToString());
                                    button.Font = new Font(button.Font, (SC ? FontStyle.Bold : FontStyle.Regular) | 
                                        (TG ? FontStyle.Italic : FontStyle.Regular) | (MC ? FontStyle.Underline : FontStyle.Regular));
                                    if (Item.InnerText.Contains(DS4KeyType.ScanCode.ToString()))
                                        keyType |= DS4KeyType.ScanCode;
                                    if (Item.InnerText.Contains(DS4KeyType.Toggle.ToString()))
                                        keyType |= DS4KeyType.Toggle;
                                    if (Item.InnerText.Contains(DS4KeyType.Macro.ToString()))
                                        keyType |= DS4KeyType.Macro;
                                }
                                if (keyType != DS4KeyType.None)
                                    customMapKeyTypes.Add(getDS4ControlsByName(Item.Name), keyType);
                            }

                            Item = m_Xdoc.SelectSingleNode(String.Format("/ScpControl/Control/Macro/{0}", button.Name));
                            if (Item != null)
                            {
                                string[] splitter = Item.InnerText.Split('/');
                                int[] keys = new int[splitter.Length];
                                for (int i = 0; i < keys.Length; i++)
                                {
                                    keys[i] = int.Parse(splitter[i]);
                                    if (keys[i] < 255) splitter[i] = ((System.Windows.Forms.Keys)keys[i]).ToString();
                                    else if (keys[i] == 256) splitter[i] = "Left Mouse Button";
                                    else if (keys[i] == 257) splitter[i] = "Right Mouse Button";
                                    else if (keys[i] == 258) splitter[i] = "Middle Mouse Button";
                                    else if (keys[i] == 259) splitter[i] = "4th Mouse Button";
                                    else if (keys[i] == 260) splitter[i] = "5th Mouse Button";
                                }
                                button.Text = string.Join(", ", splitter);                                
                                button.Tag = keys;
                                customMapMacros.Add(getDS4ControlsByName(button.Name), Item.InnerText);
                            }
                            else if (m_Xdoc.SelectSingleNode(String.Format("/ScpControl/Control/Key/{0}", button.Name)) != null)
                            {
                                Item = m_Xdoc.SelectSingleNode(String.Format("/ScpControl/Control/Key/{0}", button.Name));
                                if (UInt16.TryParse(Item.InnerText, out wvk))
                                {
                                    //foundBinding = true;
                                    customMapKeys.Add(getDS4ControlsByName(Item.Name), wvk);
                                    button.Tag = wvk;
                                    button.Text = ((System.Windows.Forms.Keys)wvk).ToString();
                                }
                            }
                            else
                            {
                                Item = m_Xdoc.SelectSingleNode(String.Format("/ScpControl/Control/Button/{0}", button.Name));
                                if (Item != null)
                                {
                                    //foundBinding = true;
                                    button.Tag = Item.InnerText;
                                    button.Text = Item.InnerText;
                                    customMapButtons.Add(getDS4ControlsByName(button.Name), getX360ControlsByName(Item.InnerText));
                                }
                            }
                        }
                        catch
                        {

                        }
                }
            }
            catch { Loaded = false; }

            if (Loaded)
            {
                this.customMapButtons[device] = customMapButtons;
                this.customMapKeys[device] = customMapKeys;
                this.customMapKeyTypes[device] = customMapKeyTypes;
                this.customMapMacros[device] = customMapMacros;
            }
            // Only add missing settings if the actual load was graceful
            if (missingSetting && Loaded)
                SaveProfile(device, profilePath[device], buttons);

            return Loaded;
        }
        public Boolean LoadProfile(int device, string propath = "")
        {
            Boolean Loaded = true;
            Dictionary<DS4Controls, DS4KeyType> customMapKeyTypes = new Dictionary<DS4Controls, DS4KeyType>();
            Dictionary<DS4Controls, UInt16> customMapKeys = new Dictionary<DS4Controls, UInt16>();
            Dictionary<DS4Controls, X360Controls> customMapButtons = new Dictionary<DS4Controls, X360Controls>();
            Dictionary<DS4Controls, String> customMapMacros = new Dictionary<DS4Controls, String>();
            Boolean missingSetting = false;
            string profilepath;
            if (propath == "")
                profilepath = profilePath[device];
            else
                profilepath = propath;
            try
            {
                if (File.Exists(profilepath))
                {
                    XmlNode Item;

                    m_Xdoc.Load(profilepath);

                    try { Item = m_Xdoc.SelectSingleNode("/ScpControl/flushHIDQueue"); Boolean.TryParse(Item.InnerText, out flushHIDQueue[device]); }
                    catch { missingSetting = true; }

                    try { Item = m_Xdoc.SelectSingleNode("/ScpControl/idleDisconnectTimeout"); Int32.TryParse(Item.InnerText, out idleDisconnectTimeout[device]); }
                    catch { missingSetting = true; }

                    try { Item = m_Xdoc.SelectSingleNode("/ScpControl/Red"); Byte.TryParse(Item.InnerText, out m_Leds[device][0]); }
                    catch { missingSetting = true; }

                    try { Item = m_Xdoc.SelectSingleNode("/ScpControl/Green"); Byte.TryParse(Item.InnerText, out m_Leds[device][1]); }
                    catch { missingSetting = true; }

                    try { Item = m_Xdoc.SelectSingleNode("/ScpControl/Blue"); Byte.TryParse(Item.InnerText, out m_Leds[device][2]); }
                    catch { missingSetting = true; }

                    try { Item = m_Xdoc.SelectSingleNode("/ScpControl/RumbleBoost"); Byte.TryParse(Item.InnerText, out m_Rumble[device]); }
                    catch { missingSetting = true; }

                    try { Item = m_Xdoc.SelectSingleNode("/ScpControl/ledAsBatteryIndicator"); Boolean.TryParse(Item.InnerText, out ledAsBattery[device]); }
                    catch { missingSetting = true; }

                    try { Item = m_Xdoc.SelectSingleNode("/ScpControl/lowBatteryFlash"); Boolean.TryParse(Item.InnerText, out flashLedLowBattery[device]); }
                    catch { missingSetting = true; }

                    try { Item = m_Xdoc.SelectSingleNode("/ScpControl/flashBatteryAt"); Int32.TryParse(Item.InnerText, out flashAt[device]); }
                    catch { missingSetting = true; }

                    try { Item = m_Xdoc.SelectSingleNode("/ScpControl/touchSensitivity"); Byte.TryParse(Item.InnerText, out touchSensitivity[device]); }
                    catch { missingSetting = true; }

                    try { Item = m_Xdoc.SelectSingleNode("/ScpControl/LowRed"); Byte.TryParse(Item.InnerText, out m_LowLeds[device][0]); }
                    catch { missingSetting = true; }
                    try { Item = m_Xdoc.SelectSingleNode("/ScpControl/LowGreen"); Byte.TryParse(Item.InnerText, out m_LowLeds[device][1]); }
                    catch { missingSetting = true; }
                    try { Item = m_Xdoc.SelectSingleNode("/ScpControl/LowBlue"); Byte.TryParse(Item.InnerText, out m_LowLeds[device][2]); }
                    catch { missingSetting = true; }
                    try { Item = m_Xdoc.SelectSingleNode("/ScpControl/ChargingRed"); Byte.TryParse(Item.InnerText, out m_ChargingLeds[device][0]); }
                    catch { missingSetting = true; }
                    try { Item = m_Xdoc.SelectSingleNode("/ScpControl/ChargingGreen"); Byte.TryParse(Item.InnerText, out m_ChargingLeds[device][1]); }
                    catch { missingSetting = true; }
                    try { Item = m_Xdoc.SelectSingleNode("/ScpControl/ChargingBlue"); Byte.TryParse(Item.InnerText, out m_ChargingLeds[device][2]); }
                    catch { missingSetting = true; }
                    try { Item = m_Xdoc.SelectSingleNode("/ScpControl/touchpadJitterCompensation"); Boolean.TryParse(Item.InnerText, out touchpadJitterCompensation[device]); }
                    catch { missingSetting = true; }
                    try { Item = m_Xdoc.SelectSingleNode("/ScpControl/lowerRCOn"); Boolean.TryParse(Item.InnerText, out lowerRCOn[device]); }
                    catch { missingSetting = true; }
                    try { Item = m_Xdoc.SelectSingleNode("/ScpControl/tapSensitivity"); Byte.TryParse(Item.InnerText, out tapSensitivity[device]); }
                    catch { missingSetting = true; }
                    try { Item = m_Xdoc.SelectSingleNode("/ScpControl/doubleTap"); Boolean.TryParse(Item.InnerText, out doubleTap[device]); }
                    catch { missingSetting = true; }
                    try { Item = m_Xdoc.SelectSingleNode("/ScpControl/scrollSensitivity"); Int32.TryParse(Item.InnerText, out scrollSensitivity[device]); }
                    catch { missingSetting = true; }
                    try { Item = m_Xdoc.SelectSingleNode("/ScpControl/LeftTriggerMiddle"); Byte.TryParse(Item.InnerText, out m_LeftTriggerMiddle[device]); }
                    catch { missingSetting = true; }
                    try { Item = m_Xdoc.SelectSingleNode("/ScpControl/RightTriggerMiddle"); Byte.TryParse(Item.InnerText, out m_RightTriggerMiddle[device]); }
                    catch { missingSetting = true; }
                    try { Item = m_Xdoc.SelectSingleNode("/ScpControl/ButtonMouseSensitivity"); Int32.TryParse(Item.InnerText, out buttonMouseSensitivity[device]); }
                    catch { missingSetting = true; }
                    try { Item = m_Xdoc.SelectSingleNode("/ScpControl/Rainbow"); Double.TryParse(Item.InnerText, out rainbow[device]); }
                    catch { rainbow[device] = 0; missingSetting = true; }
                    try { Item = m_Xdoc.SelectSingleNode("/ScpControl/LSDeadZone"); Byte.TryParse(Item.InnerText, out LSDeadzone[device]); }
                    catch { missingSetting = true; }
                    try { Item = m_Xdoc.SelectSingleNode("/ScpControl/RSDeadZone"); Byte.TryParse(Item.InnerText, out RSDeadzone[device]); }
                    catch { missingSetting = true; }
                    try { Item = m_Xdoc.SelectSingleNode("/ScpControl/ChargingType"); Int32.TryParse(Item.InnerText, out chargingType[device]); }
                    catch { missingSetting = true; }

                    DS4KeyType keyType;
                    UInt16 wvk;
                    XmlNode ParentItem = m_Xdoc.SelectSingleNode("/ScpControl/Control/Button");
                    if (ParentItem != null)
                        foreach (XmlNode item in ParentItem.ChildNodes)
                            customMapButtons.Add(getDS4ControlsByName(item.Name), getX360ControlsByName(item.InnerText));
                    ParentItem = m_Xdoc.SelectSingleNode("/ScpControl/Control/Macro");
                    if (ParentItem != null)
                        foreach (XmlNode item in ParentItem.ChildNodes)
                                customMapMacros.Add(getDS4ControlsByName(item.Name), item.InnerText);
                    ParentItem = m_Xdoc.SelectSingleNode("/ScpControl/Control/Key");
                    if (ParentItem != null)
                        foreach (XmlNode item in ParentItem.ChildNodes)
                            if (UInt16.TryParse(item.InnerText, out wvk))
                                customMapKeys.Add(getDS4ControlsByName(item.Name), wvk);
                    ParentItem = m_Xdoc.SelectSingleNode("/ScpControl/Control/KeyType");
                    if (ParentItem != null)
                        foreach (XmlNode item in ParentItem.ChildNodes)
                            if (item != null)
                            {
                                keyType = DS4KeyType.None;
                                if (item.InnerText.Contains(DS4KeyType.ScanCode.ToString()))
                                    keyType |= DS4KeyType.ScanCode;
                                if (item.InnerText.Contains(DS4KeyType.Toggle.ToString()))
                                    keyType |= DS4KeyType.Toggle;
                                if (item.InnerText.Contains(DS4KeyType.Macro.ToString()))
                                    keyType |= DS4KeyType.Macro;
                                if (item.InnerText.Contains(DS4KeyType.Unbound.ToString()))
                                    keyType |= DS4KeyType.Unbound;
                                if (keyType != DS4KeyType.None)
                                    customMapKeyTypes.Add(getDS4ControlsByName(item.Name), keyType);
                            }
                }
            }
            catch { Loaded = false; }

            if (Loaded)
            {
                this.customMapButtons[device] = customMapButtons;
                this.customMapKeys[device] = customMapKeys;
                this.customMapKeyTypes[device] = customMapKeyTypes;
                this.customMapMacros[device] = customMapMacros;
            }
            // Only add missing settings if the actual load was graceful
            if (missingSetting && Loaded)
                SaveProfile(device, profilepath, null);

            return Loaded;
        }

        public bool Load()
        {
            Boolean Loaded = true;
            Boolean missingSetting = false;

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
                    try { Item = m_Xdoc.SelectSingleNode("/Profile/formWidth"); Int32.TryParse(Item.InnerText, out formWidth); }
                    catch { missingSetting = true; }
                    try { Item = m_Xdoc.SelectSingleNode("/Profile/formHeight"); Int32.TryParse(Item.InnerText, out formHeight); }
                    catch { missingSetting = true; }
                    try { Item = m_Xdoc.SelectSingleNode("/Profile/Controller1"); profilePath[0] = Item.InnerText; }
                    catch { missingSetting = true; }
                    try { Item = m_Xdoc.SelectSingleNode("/Profile/Controller2"); profilePath[1] = Item.InnerText; }
                    catch { missingSetting = true; }
                    try { Item = m_Xdoc.SelectSingleNode("/Profile/Controller3"); profilePath[2] = Item.InnerText; }
                    catch { missingSetting = true; }
                    try { Item = m_Xdoc.SelectSingleNode("/Profile/Controller4"); profilePath[3] = Item.InnerText; }
                    catch { missingSetting = true; }
                    try { Item = m_Xdoc.SelectSingleNode("/Profile/DS4Version"); Double.TryParse(Item.InnerText, out version); }
                    catch { missingSetting = true; }
                    try { Item = m_Xdoc.SelectSingleNode("/Profile/LastChecked"); DateTime.TryParse(Item.InnerText, out lastChecked); }
                    catch { missingSetting = true; }
                    try { Item = m_Xdoc.SelectSingleNode("/Profile/CheckWhen"); Int32.TryParse(Item.InnerText, out CheckWhen); }
                    catch { missingSetting = true; }
                    try { Item = m_Xdoc.SelectSingleNode("/Profile/Notifications"); Boolean.TryParse(Item.InnerText, out notifications); }
                    catch { missingSetting = true; }
                }
            }
            catch { }
            if (missingSetting)
                Save();
            return Loaded; 
        }
        public bool Save()
        {
            Boolean Saved = true;

            try
            {
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
                XmlNode xmlFormWidth = m_Xdoc.CreateNode(XmlNodeType.Element, "formWidth", null); xmlFormWidth.InnerText = formWidth.ToString(); Node.AppendChild(xmlFormWidth);
                XmlNode xmlFormHeight = m_Xdoc.CreateNode(XmlNodeType.Element, "formHeight", null); xmlFormHeight.InnerText = formHeight.ToString(); Node.AppendChild(xmlFormHeight);
                
                XmlNode xmlController1 = m_Xdoc.CreateNode(XmlNodeType.Element, "Controller1", null); xmlController1.InnerText = profilePath[0]; Node.AppendChild(xmlController1);
                XmlNode xmlController2 = m_Xdoc.CreateNode(XmlNodeType.Element, "Controller2", null); xmlController2.InnerText = profilePath[1]; Node.AppendChild(xmlController2);
                XmlNode xmlController3 = m_Xdoc.CreateNode(XmlNodeType.Element, "Controller3", null); xmlController3.InnerText = profilePath[2]; Node.AppendChild(xmlController3);
                XmlNode xmlController4 = m_Xdoc.CreateNode(XmlNodeType.Element, "Controller4", null); xmlController4.InnerText = profilePath[3]; Node.AppendChild(xmlController4);

                XmlNode xmlVersion = m_Xdoc.CreateNode(XmlNodeType.Element, "DS4Version", null); xmlVersion.InnerText = version.ToString(); Node.AppendChild(xmlVersion);
                XmlNode xmlLastChecked = m_Xdoc.CreateNode(XmlNodeType.Element, "LastChecked", null); xmlLastChecked.InnerText = lastChecked.ToString(); Node.AppendChild(xmlLastChecked);
                XmlNode xmlCheckWhen = m_Xdoc.CreateNode(XmlNodeType.Element, "CheckWhen", null); xmlCheckWhen.InnerText = CheckWhen.ToString(); Node.AppendChild(xmlCheckWhen);
                XmlNode xmlNotifications = m_Xdoc.CreateNode(XmlNodeType.Element, "Notifications", null); xmlNotifications.InnerText = notifications.ToString(); Node.AppendChild(xmlNotifications);
                m_Xdoc.AppendChild(Node);

                m_Xdoc.Save(m_Profile);
            }
            catch { Saved = false; }

            return Saved;
        }
    }
}
