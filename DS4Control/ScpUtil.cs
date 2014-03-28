using System;
using System.Collections.Generic;
using System.Text;

using System.IO;
using System.Reflection;
using System.Xml;
using DS4Library;
namespace DS4Control
{
    [Flags]
    public enum DS4KeyType : byte { None = 0, ScanCode = 1, Repeat = 2 }; //Increment by exponents of 2*, starting at 2^0
    public enum Ds3PadId : byte { None = 0xFF, One = 0x00, Two = 0x01, Three = 0x02, Four = 0x03, All = 0x04 };
    public enum DS4Controls : byte { LXNeg, LXPos, LYNeg, LYPos, RXNeg, RXPos, RYNeg, RYPos, L1, L2, L3, R1, R2, R3, Square, Triangle, Circle, Cross, DpadUp, DpadRight, DpadDown, DpadLeft, PS, TouchButton, TouchUpper, TouchMulti, Share, Options };
    public enum X360Controls : byte { LXNeg, LXPos, LYNeg, LYPos, RXNeg, RXPos, RYNeg, RYPos, LB, LT, LS, RB, RT, RS, X, Y, B, A, DpadUp, DpadRight, DpadDown, DpadLeft, Guide, Back, Start, LeftMouse, RightMouse, MiddleMouse, Unbound,
    MouseLeft, MouseRight, MouseDown, MouseUp};

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

        public static bool getFlushHIDQueue(int device)
        {
            return m_Config.flushHIDQueue[device];
        }
        public static void setFlushHIDQueue(int device, bool setting)
        {
            m_Config.flushHIDQueue[device] = setting;
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

        public static void setLedAsBatteryIndicator(int device, bool ledAsBattery)
        {
            m_Config.ledAsBattery[device] = ledAsBattery;

        }
        public static bool getLedAsBatteryIndicator(int device)
        {
            return m_Config.ledAsBattery[device];
        }

        public static void setTouchEnabled(int device, bool touchEnabled)
        {
            m_Config.touchEnabled[device] = touchEnabled;

        }
        public static bool getTouchEnabled(int device)
        {
            return m_Config.touchEnabled[device];

        }

        public static void setUseExclusiveMode(bool exclusive)
        {
            m_Config.useExclusiveMode = exclusive;
        }
        public static bool getUseExclusiveMode()
        {
            return m_Config.useExclusiveMode;
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
        public static void setTapSensitivity(int device, byte sen)
        {
            m_Config.tapSensitivity[device] = sen;
        }
        public static byte getTapSensitivity(int device)
        {
            return m_Config.tapSensitivity[device];
        }
        public static void setScrollSensitivity(int device, int sen)
        {
            m_Config.scrollSensitivity[device] = sen;
        }
        public static int getScrollSensitivity(int device)
        {
            return m_Config.scrollSensitivity[device];
        }
        public static void setLowerRCOff(int device, bool twoFingerRC)
        {
            m_Config.lowerRCOff[device] = twoFingerRC;
        }
        public static bool getLowerRCOff(int device)
        {
            return m_Config.lowerRCOff[device];
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
        public static void setLeftTriggerMiddle(int device, double value)
        {
            m_Config.m_LeftTriggerMiddle[device] = value;
        }

        public static double getRightTriggerMiddle(int device)
        {
            return m_Config.m_RightTriggerMiddle[device];
        }
        public static void setRightTriggerMiddle(int device, double value)
        {
            m_Config.m_RightTriggerMiddle[device] = value;
        }

        public static void setCustomMap(int device, string customMap)
        {
            m_Config.customMapPath[device] = customMap;
        }
        public static string getCustomMap(int device)
        {
            return m_Config.customMapPath[device];
        }
        public static bool saveCustomMapping(string customMapPath, System.Windows.Forms.Control[] buttons)
        {
            return m_Config.SaveCustomMapping(customMapPath, buttons);
        }
        public static bool loadCustomMapping(string customMapPath, System.Windows.Forms.Control[] buttons)
        {
            return m_Config.LoadCustomMapping(customMapPath, buttons);
        }
        public static bool loadCustomMapping(int device)
        {
            return m_Config.LoadCustomMapping(getCustomMap(device));
        }
        public static X360Controls getCustomButton(DS4Controls controlName)
        {
            return m_Config.GetCustomButton(controlName);
        }
        public static ushort getCustomKey(DS4Controls controlName)
        {
            return m_Config.GetCustomKey(controlName);
        }
        public static DS4KeyType getCustomKeyType(DS4Controls controlName)
        {
            return m_Config.GetCustomKeyType(controlName);
        }
        public static bool getHasCustomKeysorButtons(int device)
        {
            return m_Config.customMapButtons.Count > 0
                || m_Config.customMapKeys.Count > 0;
        }
        public static Dictionary<DS4Controls, X360Controls> getCustomButtons()
        {
            return m_Config.customMapButtons;
        }
        public static Dictionary<DS4Controls, ushort> getCustomKeys()
        {
            return m_Config.customMapKeys;
        }
        public static Dictionary<DS4Controls, DS4KeyType> getCustomKeyTypes()
        {
            return m_Config.customMapKeyTypes;
        }

        public static void Load()
        {
            m_Config.Load();
        }
        public static void Save()
        {
            m_Config.Save();
        }

        private static byte applyRatio(byte b1, byte b2, uint r)
        {
            uint ratio = r;
            if (b1 > b2)
            {
                ratio = 100 - r;
            }
            byte bmax = Math.Max(b1, b2);
            byte bmin = Math.Min(b1, b2);
            byte bdif = (byte)(bmax - bmin);
            return (byte)(bmin + (bdif * ratio / 100));
        }
        public static DS4Color getTransitionedColor(byte[] c1, byte[] c2, uint ratio)
        {
            DS4Color color = new DS4Color();
            color.red = 255;
            color.green = 255;
            color.blue = 255;
            uint r = ratio % 101;
            if (c1.Length != 3 || c2.Length != 3 || ratio < 0)
            {
                return color;
            }
            color.red = applyRatio(c1[0], c2[0], ratio);
            color.green = applyRatio(c1[1], c2[1], ratio);
            color.blue = applyRatio(c1[2], c2[2], ratio);

            return color;
        }
    }



    public class BackingStore
    {
        protected String m_File = Directory.GetParent(Assembly.GetExecutingAssembly().Location).FullName + @"\ScpControl.xml";
        protected XmlDocument m_Xdoc = new XmlDocument();

        public Boolean[] touchpadJitterCompensation = { true, true, true, true };
        public Boolean[] lowerRCOff = { false, false, false, false };
        public Boolean[] ledAsBattery = { false, false, false, false };
        public Boolean[] flashLedLowBattery = { false, false, false, false };
        public Boolean[] touchEnabled = { false, false, false, false };
        public double[] m_LeftTriggerMiddle = { 0.5, 0.5, 0.5, 0.5 }, m_RightTriggerMiddle = { 0.5, 0.5, 0.5, 0.5 };
        public String[] customMapPath = { String.Empty, String.Empty, String.Empty, String.Empty };
        public Byte[] m_Rumble = { 100, 100, 100, 100 };
        public Byte[] touchSensitivity = { 100, 100, 100, 100 };
        public Byte[] tapSensitivity = { 0, 0, 0, 0 };
        public int[] scrollSensitivity = { 0, 0, 0, 0 };
        public Byte[][] m_LowLeds = new Byte[][]
        {
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
        };
        public bool[] flushHIDQueue = { true, true, true, true };

        public Boolean useExclusiveMode = false;
        public Int32 formWidth = 782;
        public Int32 formHeight = 550;
        public Boolean startMinimized = false;

        public Dictionary<DS4Controls, DS4KeyType> customMapKeyTypes = new Dictionary<DS4Controls, DS4KeyType>();
        public Dictionary<DS4Controls, UInt16> customMapKeys = new Dictionary<DS4Controls, UInt16>();
        public Dictionary<DS4Controls, X360Controls> customMapButtons = new Dictionary<DS4Controls, X360Controls>();
        public X360Controls GetCustomButton(DS4Controls controlName)
        {
            if (customMapButtons.ContainsKey(controlName))
                return customMapButtons[controlName];
            else return 0;
        }
        public UInt16 GetCustomKey(DS4Controls controlName)
        {
            if (customMapKeys.ContainsKey(controlName))
                return customMapKeys[controlName];
            else return 0;
        }
        public DS4KeyType GetCustomKeyType(DS4Controls controlName)
        {
            if (customMapKeyTypes.ContainsKey(controlName))
                return customMapKeyTypes[controlName];
            else return 0;
        }

        public Boolean LoadCustomMapping(String customMapPath)
        {
            Boolean Loaded = true;
            customMapButtons.Clear();
            customMapKeys.Clear();
            customMapKeyTypes.Clear();
            try
            {
                if (customMapPath != string.Empty && File.Exists(customMapPath))
                {
                    m_Xdoc.Load(customMapPath);
                    DS4KeyType keyType;
                    UInt16 wvk;
                    XmlNode ParentItem = m_Xdoc.SelectSingleNode("/Control/Button");
                    if (ParentItem != null)
                        foreach (XmlNode Item in ParentItem.ChildNodes)
                            customMapButtons.Add(getDS4ControlsByName(Item.Name), getX360ControlsByName(Item.InnerText));
                    ParentItem = m_Xdoc.SelectSingleNode("/Control/Key");
                    if (ParentItem != null)
                        foreach (XmlNode Item in ParentItem.ChildNodes)
                            if (UInt16.TryParse(Item.InnerText, out wvk))
                                customMapKeys.Add(getDS4ControlsByName(Item.Name), wvk);
                    ParentItem = m_Xdoc.SelectSingleNode("/Control/KeyType");
                    if (ParentItem != null)
                        foreach (XmlNode Item in ParentItem.ChildNodes)
                            if (Item != null)
                            {
                                keyType = DS4KeyType.None;
                                if (Item.InnerText.Contains(DS4KeyType.ScanCode.ToString()))
                                    keyType |= DS4KeyType.ScanCode;
                                if (Item.InnerText.Contains(DS4KeyType.Repeat.ToString()))
                                    keyType |= DS4KeyType.Repeat;
                                if (keyType != DS4KeyType.None)
                                    customMapKeyTypes.Add(getDS4ControlsByName(Item.Name), keyType);
                            }
                }
            }
            catch (Exception)
            {
                Loaded = false;
            }
            return Loaded;
        }
        public Boolean LoadCustomMapping(String customMapPath, System.Windows.Forms.Control[] buttons)
        {
            Boolean Loaded = true;
            customMapButtons.Clear();
            customMapKeys.Clear();
            customMapKeyTypes.Clear();
            try
            {
                if (customMapPath != string.Empty && File.Exists(customMapPath))
                {
                    XmlNode Item;
                    m_Xdoc.Load(customMapPath);
                    DS4KeyType keyType;
                    UInt16 wvk;
                    foreach (var button in buttons)
                        try
                        {
                            Item = m_Xdoc.SelectSingleNode(String.Format("/Control/Key/{0}", button.Name));
                            if (Item != null)
                            {
                                if (UInt16.TryParse(Item.InnerText, out wvk))
                                {
                                    customMapKeys.Add(getDS4ControlsByName(Item.Name), wvk);
                                    button.Tag = wvk;
                                    button.Text = ((System.Windows.Forms.Keys)wvk).ToString();

                                    Item = m_Xdoc.SelectSingleNode(String.Format("/Control/KeyType/{0}", button.Name));
                                    if (Item != null)
                                    {
                                        keyType = DS4KeyType.None;
                                        if (Item.InnerText.Contains(DS4KeyType.ScanCode.ToString()))
                                        {
                                            keyType |= DS4KeyType.ScanCode;
                                            button.Font = new System.Drawing.Font(button.Font, System.Drawing.FontStyle.Bold);
                                        }
                                        if (Item.InnerText.Contains(DS4KeyType.Repeat.ToString()))
                                        {
                                            keyType |= DS4KeyType.Repeat;
                                            button.ForeColor = System.Drawing.Color.Red;
                                        }
                                        if (keyType != DS4KeyType.None)
                                            customMapKeyTypes.Add(getDS4ControlsByName(Item.Name), keyType);
                                    }
                                }
                            }
                            else
                            {
                                Item = m_Xdoc.SelectSingleNode(String.Format("/Control/Button/{0}", button.Name));
                                if (Item != null)
                                {
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
            catch
            {
                Loaded = false;
            }
            return Loaded;
        }
        public Boolean SaveCustomMapping(String customMapPath, System.Windows.Forms.Control[] buttons)
        {
            Boolean Saved = true;
            try
            {
                XmlNode Node;
                m_Xdoc.RemoveAll();
                Node = m_Xdoc.CreateXmlDeclaration("1.0", "utf-8", String.Empty);
                m_Xdoc.AppendChild(Node);
                Node = m_Xdoc.CreateComment(String.Format(" Custom Control Mapping Data. {0} ", DateTime.Now));
                m_Xdoc.AppendChild(Node);
                Node = m_Xdoc.CreateWhitespace("\r\n");
                m_Xdoc.AppendChild(Node);
                Node = m_Xdoc.CreateNode(XmlNodeType.Element, "Control", null);

                XmlNode Key = m_Xdoc.CreateNode(XmlNodeType.Element, "Key", null);
                XmlNode KeyType = m_Xdoc.CreateNode(XmlNodeType.Element, "KeyType", null);
                XmlNode Button = m_Xdoc.CreateNode(XmlNodeType.Element, "Button", null);

                foreach (var button in buttons)
                    try
                    {
                        // Save even if string (for xbox controller buttons)
                        if (button.Tag != null)
                        {
                            XmlNode buttonNode;
                            string keyType = String.Empty;
                            if (button.ForeColor == System.Drawing.Color.Red)
                                keyType += DS4KeyType.Repeat;
                            if (button.Font.Bold)
                                keyType += DS4KeyType.ScanCode;
                            if (keyType != String.Empty)
                            {
                                buttonNode = m_Xdoc.CreateNode(XmlNodeType.Element, button.Name, null);
                                buttonNode.InnerText = keyType;
                                KeyType.AppendChild(buttonNode);
                            }
                            buttonNode = m_Xdoc.CreateNode(XmlNodeType.Element, button.Name, null);
                            buttonNode.InnerText = button.Tag.ToString();
                            if (button.Tag is Int32 || button.Tag is UInt16)
                                Key.AppendChild(buttonNode);
                            else Button.AppendChild(buttonNode);
                        }
                    }
                    catch
                    {
                        Saved = false;
                    }
                m_Xdoc.AppendChild(Node);
                if (Button.HasChildNodes)
                    Node.AppendChild(Button);
                if (Key.HasChildNodes)
                    Node.AppendChild(Key);
                if (KeyType.HasChildNodes)
                    Node.AppendChild(KeyType);
                m_Xdoc.Save(customMapPath);
            }
            catch (Exception)
            {
                Saved = false;
            }
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
                case "bnLX": return DS4Controls.LXNeg;
                case "bnLY": return DS4Controls.LYNeg;
                case "bnRX": return DS4Controls.RXNeg;
                case "bnRY": return DS4Controls.RYNeg;
                case "bnLX2": return DS4Controls.LXPos;
                case "bnLY2": return DS4Controls.LYPos;
                case "bnRX2": return DS4Controls.RXPos;
                case "bnRY2": return DS4Controls.RYPos;
                case "bnL2": return DS4Controls.L2;
                case "bnR2": return DS4Controls.R2;

                case "bnTouchpad": return DS4Controls.TouchButton;
                case "bnTouchMulti": return DS4Controls.TouchMulti;
                case "bnTouchUpper": return DS4Controls.TouchUpper;
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
                case "Click": return X360Controls.LeftMouse;
                case "Right Click": return X360Controls.RightMouse;
                case "Middle Click": return X360Controls.MiddleMouse;
                case "Mouse Up": return X360Controls.MouseUp;
                case "Mouse Down": return X360Controls.MouseDown;
                case "Mouse Left": return X360Controls.MouseLeft;
                case "Mouse Right": return X360Controls.MouseRight;
                case "(Unbound)": return X360Controls.Unbound;

            }
            return X360Controls.Unbound;
        }
        public Boolean Load()
        {
            Boolean Loaded = true;
            Boolean missingSetting = false;

            try
            {
                if (File.Exists(m_File))
                {
                    XmlNode Item;

                    m_Xdoc.Load(m_File);


                    for (int i = 0; i < 4; i++)
                    {
                        try { Item = m_Xdoc.SelectSingleNode("/ScpControl/Controller" + (i + 1) + "/flushHIDQueue"); Boolean.TryParse(Item.InnerText, out flushHIDQueue[i]); }
                        catch { missingSetting = true; }

                        try { Item = m_Xdoc.SelectSingleNode("/ScpControl/Controller" + (i + 1) + "/Red"); Byte.TryParse(Item.InnerText, out m_Leds[i][0]); }
                        catch { missingSetting = true; }

                        try { Item = m_Xdoc.SelectSingleNode("/ScpControl/Controller" + (i + 1) + "/Green"); Byte.TryParse(Item.InnerText, out m_Leds[i][1]); }
                        catch { missingSetting = true; }

                        try { Item = m_Xdoc.SelectSingleNode("/ScpControl/Controller" + (i + 1) + "/Blue"); Byte.TryParse(Item.InnerText, out m_Leds[i][2]); }
                        catch { missingSetting = true; }

                        try { Item = m_Xdoc.SelectSingleNode("/ScpControl/Controller" + (i + 1) + "/RumbleBoost"); Byte.TryParse(Item.InnerText, out m_Rumble[i]); }
                        catch { missingSetting = true; }

                        try { Item = m_Xdoc.SelectSingleNode("/ScpControl/Controller" + (i + 1) + "/ledAsBatteryIndicator"); Boolean.TryParse(Item.InnerText, out ledAsBattery[i]); }
                        catch { missingSetting = true; }

                        try { Item = m_Xdoc.SelectSingleNode("/ScpControl/Controller" + (i + 1) + "/lowBatteryFlash"); Boolean.TryParse(Item.InnerText, out flashLedLowBattery[i]); }
                        catch { missingSetting = true; }

                        try { Item = m_Xdoc.SelectSingleNode("/ScpControl/Controller" + (i + 1) + "/touchSensitivity"); Byte.TryParse(Item.InnerText, out touchSensitivity[i]); }
                        catch { missingSetting = true; }

                        try { Item = m_Xdoc.SelectSingleNode("/ScpControl/Controller" + (i + 1) + "/touchEnabled"); Boolean.TryParse(Item.InnerText, out touchEnabled[i]); }
                        catch { missingSetting = true; }

                        try { Item = m_Xdoc.SelectSingleNode("/ScpControl/Controller" + (i + 1) + "/LowRed"); Byte.TryParse(Item.InnerText, out m_LowLeds[i][0]); }
                        catch { missingSetting = true; }
                        try { Item = m_Xdoc.SelectSingleNode("/ScpControl/Controller" + (i + 1) + "/LowGreen"); Byte.TryParse(Item.InnerText, out m_LowLeds[i][1]); }
                        catch { missingSetting = true; }
                        try { Item = m_Xdoc.SelectSingleNode("/ScpControl/Controller" + (i + 1) + "/LowBlue"); Byte.TryParse(Item.InnerText, out m_LowLeds[i][2]); }
                        catch { missingSetting = true; }
                        try { Item = m_Xdoc.SelectSingleNode("/ScpControl/Controller" + (i + 1) + "/touchpadJitterCompensation"); Boolean.TryParse(Item.InnerText, out touchpadJitterCompensation[i]); }
                        catch { missingSetting = true; }
                        try { Item = m_Xdoc.SelectSingleNode("/ScpControl/Controller" + (i + 1) + "/lowerRCOff"); Boolean.TryParse(Item.InnerText, out lowerRCOff[i]); }
                        catch { missingSetting = true; }
                        try { Item = m_Xdoc.SelectSingleNode("/ScpControl/Controller" + (i + 1) + "/tapSensitivity"); Byte.TryParse(Item.InnerText, out tapSensitivity[i]); }
                        catch { missingSetting = true; }
                        try { Item = m_Xdoc.SelectSingleNode("/ScpControl/Controller" + (i + 1) + "/scrollSensitivity"); Int32.TryParse(Item.InnerText, out scrollSensitivity[i]); }
                        catch { missingSetting = true; }
                        // XXX This sucks, let's do better at removing old values that are no longer valid....
                        if (scrollSensitivity[i] > 10)
                            scrollSensitivity[i] = 5;
                        try { Item = m_Xdoc.SelectSingleNode("/ScpControl/Controller" + (i + 1) + "/customMapPath"); customMapPath[i] = Item.InnerText; }
                        catch { missingSetting = true; }
                        try { Item = m_Xdoc.SelectSingleNode("/ScpControl/Controller" + (i + 1) + "/LeftTriggerMiddle"); Double.TryParse(Item.InnerText, out m_LeftTriggerMiddle[i]); }
                        catch { missingSetting = true; }
                        try { Item = m_Xdoc.SelectSingleNode("/ScpControl/Controller" + (i + 1) + "/RightTriggerMiddle"); Double.TryParse(Item.InnerText, out m_RightTriggerMiddle[i]); }
                        catch { missingSetting = true; }
                    }

                    try { Item = m_Xdoc.SelectSingleNode("/ScpControl/useExclusiveMode"); Boolean.TryParse(Item.InnerText, out useExclusiveMode); }
                    catch { missingSetting = true; }
                    try { Item = m_Xdoc.SelectSingleNode("/ScpControl/startMinimized"); Boolean.TryParse(Item.InnerText, out startMinimized); }
                    catch { missingSetting = true; }
                    try { Item = m_Xdoc.SelectSingleNode("/ScpControl/formWidth"); Int32.TryParse(Item.InnerText, out formWidth); }
                    catch { missingSetting = true; }
                    try { Item = m_Xdoc.SelectSingleNode("/ScpControl/formHeight"); Int32.TryParse(Item.InnerText, out formHeight); }
                    catch { missingSetting = true; }
                }
            }
            catch { Loaded = false; }

            // Only add missing settings if the actual load was graceful
            if (missingSetting && Loaded)
                Save();

            return Loaded;
        }
        public Boolean Save()
        {
            Boolean Saved = true;

            try
            {
                XmlNode Node, Entry;

                m_Xdoc.RemoveAll();

                Node = m_Xdoc.CreateXmlDeclaration("1.0", "utf-8", String.Empty);
                m_Xdoc.AppendChild(Node);

                Node = m_Xdoc.CreateComment(String.Format(" ScpControl Configuration Data. {0} ", DateTime.Now));
                m_Xdoc.AppendChild(Node);

                Node = m_Xdoc.CreateWhitespace("\r\n");
                m_Xdoc.AppendChild(Node);

                Node = m_Xdoc.CreateNode(XmlNodeType.Element, "ScpControl", null);

                XmlNode xmlUseExclNode = m_Xdoc.CreateNode(XmlNodeType.Element, "useExclusiveMode", null); xmlUseExclNode.InnerText = useExclusiveMode.ToString(); Node.AppendChild(xmlUseExclNode);
                XmlNode xmlStartMinimized = m_Xdoc.CreateNode(XmlNodeType.Element, "startMinimized", null); xmlStartMinimized.InnerText = startMinimized.ToString(); Node.AppendChild(xmlStartMinimized);
                XmlNode xmlFormWidth = m_Xdoc.CreateNode(XmlNodeType.Element, "formWidth", null); xmlFormWidth.InnerText = formWidth.ToString(); Node.AppendChild(xmlFormWidth);
                XmlNode xmlFormHeight = m_Xdoc.CreateNode(XmlNodeType.Element, "formHeight", null); xmlFormHeight.InnerText = formHeight.ToString(); Node.AppendChild(xmlFormHeight);

                XmlNode cNode1 = m_Xdoc.CreateNode(XmlNodeType.Element, "Controller1", null); Node.AppendChild(cNode1);
                XmlNode cNode2 = m_Xdoc.CreateNode(XmlNodeType.Element, "Controller2", null); Node.AppendChild(cNode2);
                XmlNode cNode3 = m_Xdoc.CreateNode(XmlNodeType.Element, "Controller3", null); Node.AppendChild(cNode3);
                XmlNode cNode4 = m_Xdoc.CreateNode(XmlNodeType.Element, "Controller4", null); Node.AppendChild(cNode4);

                XmlNode[] cNodes = { cNode1, cNode2, cNode3, cNode4 };

                for (int i = 0; i < 4; i++)
                {
                    Entry = m_Xdoc.CreateNode(XmlNodeType.Element, "flushHIDQueue", null); Entry.InnerText = flushHIDQueue[i].ToString(); cNodes[i].AppendChild(Entry);
                    Entry = m_Xdoc.CreateNode(XmlNodeType.Element, "Red", null); Entry.InnerText = m_Leds[i][0].ToString(); cNodes[i].AppendChild(Entry);
                    Entry = m_Xdoc.CreateNode(XmlNodeType.Element, "Green", null); Entry.InnerText = m_Leds[i][1].ToString(); cNodes[i].AppendChild(Entry);
                    Entry = m_Xdoc.CreateNode(XmlNodeType.Element, "Blue", null); Entry.InnerText = m_Leds[i][2].ToString(); cNodes[i].AppendChild(Entry);
                    Entry = m_Xdoc.CreateNode(XmlNodeType.Element, "RumbleBoost", null); Entry.InnerText = m_Rumble[i].ToString(); cNodes[i].AppendChild(Entry);
                    Entry = m_Xdoc.CreateNode(XmlNodeType.Element, "ledAsBatteryIndicator", null); Entry.InnerText = ledAsBattery[i].ToString(); cNodes[i].AppendChild(Entry);
                    Entry = m_Xdoc.CreateNode(XmlNodeType.Element, "lowBatteryFlash", null); Entry.InnerText = flashLedLowBattery[i].ToString(); cNodes[i].AppendChild(Entry);
                    Entry = m_Xdoc.CreateNode(XmlNodeType.Element, "touchSensitivity", null); Entry.InnerText = touchSensitivity[i].ToString(); cNodes[i].AppendChild(Entry);
                    Entry = m_Xdoc.CreateNode(XmlNodeType.Element, "touchEnabled", null); Entry.InnerText = touchEnabled[i].ToString(); cNodes[i].AppendChild(Entry);
                    Entry = m_Xdoc.CreateNode(XmlNodeType.Element, "LowRed", null); Entry.InnerText = m_LowLeds[i][0].ToString(); cNodes[i].AppendChild(Entry);
                    Entry = m_Xdoc.CreateNode(XmlNodeType.Element, "LowGreen", null); Entry.InnerText = m_LowLeds[i][1].ToString(); cNodes[i].AppendChild(Entry);
                    Entry = m_Xdoc.CreateNode(XmlNodeType.Element, "LowBlue", null); Entry.InnerText = m_LowLeds[i][2].ToString(); cNodes[i].AppendChild(Entry);
                    Entry = m_Xdoc.CreateNode(XmlNodeType.Element, "touchpadJitterCompensation", null); Entry.InnerText = touchpadJitterCompensation[i].ToString(); cNodes[i].AppendChild(Entry);
                    Entry = m_Xdoc.CreateNode(XmlNodeType.Element, "lowerRCOff", null); Entry.InnerText = lowerRCOff[i].ToString(); cNodes[i].AppendChild(Entry);
                    Entry = m_Xdoc.CreateNode(XmlNodeType.Element, "tapSensitivity", null); Entry.InnerText = tapSensitivity[i].ToString(); cNodes[i].AppendChild(Entry);
                    Entry = m_Xdoc.CreateNode(XmlNodeType.Element, "scrollSensitivity", null); Entry.InnerText = scrollSensitivity[i].ToString(); cNodes[i].AppendChild(Entry);
                    Entry = m_Xdoc.CreateNode(XmlNodeType.Element, "customMapPath", null); Entry.InnerText = customMapPath[i]; cNodes[i].AppendChild(Entry);
                    Entry = m_Xdoc.CreateNode(XmlNodeType.Element, "LeftTriggerMiddle", null); Entry.InnerText = m_LeftTriggerMiddle[i].ToString(); cNodes[i].AppendChild(Entry);
                    Entry = m_Xdoc.CreateNode(XmlNodeType.Element, "RightTriggerMiddle", null); Entry.InnerText = m_RightTriggerMiddle[i].ToString(); cNodes[i].AppendChild(Entry);
                }
                m_Xdoc.AppendChild(Node);

                m_Xdoc.Save(m_File);
            }
            catch { Saved = false; }

            return Saved;
        }
    }
}
