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
    public enum DS4KeyType : byte { None = 0, ScanCode = 1, Repeat = 2, Unbound = 4 }; //Increment by exponents of 2*, starting at 2^0
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

        public static void setLedAsBatteryIndicator(int device, bool ledAsBattery)
        {
            m_Config.ledAsBattery[device] = ledAsBattery;

        }
        public static bool getLedAsBatteryIndicator(int device)
        {
            return m_Config.ledAsBattery[device];
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
        public static void setAProfile(int device, string filepath)
        {
            m_Config.profilePath[device] = Global.appdatapath + @"\Profiles\" + filepath + ".xml";
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
        public static void Save()
        {
            m_Config.Save();
        }

        public static void SaveProfile(int device, string propath, System.Windows.Forms.Control[] buttons)
        {
            m_Config.SaveProfile(device, propath, buttons);
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
        public static DS4Color getTransitionedColor(DS4Color c1, DS4Color c2, uint ratio)
        {;
            c1.red = applyRatio(c1.red, c2.red, ratio);
            c1.green = applyRatio(c1.green, c2.green, ratio);
            c1.blue = applyRatio(c1.blue, c2.blue, ratio);
            return c1;
        }
    }



    public class BackingStore
    {
        protected String m_Profile = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\DS4Tool\\Profiles.xml";
        protected XmlDocument m_Xdoc = new XmlDocument();

        public int[] buttonMouseSensitivity = { 50, 50, 50, 50 };

        public Boolean[] touchpadJitterCompensation = {true, true, true, true};
        public Boolean[] lowerRCOn = { false, false, false, false };
        public Boolean[] ledAsBattery = { false, false, false, false };
        public Boolean[] flashLedLowBattery = { false, false, false, false };
        public double[] m_LeftTriggerMiddle = { 0.5, 0.5, 0.5, 0.5 }, m_RightTriggerMiddle = { 0.5, 0.5, 0.5, 0.5 };
        public String[] profilePath = { String.Empty, String.Empty, String.Empty, String.Empty };
        public Byte[] m_Rumble = { 100, 100, 100, 100 };
        public Byte[] touchSensitivity = { 100, 100, 100, 100 };
        public Byte[] tapSensitivity = {0, 0, 0, 0};
        public bool[] doubleTap = { false, false, false, false };
        public int[] scrollSensitivity = { 0, 0, 0, 0 };
        public double[] rainbow = { 0, 0, 0, 0 };
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
        public int[] idleDisconnectTimeout = { 0, 0, 0, 0 };

        public Boolean useExclusiveMode = false;
        public Int32 formWidth = 782;
        public Int32 formHeight = 550;
        public Boolean startMinimized = false;
        public double version;
        public Dictionary<DS4Controls, DS4KeyType>[] customMapKeyTypes = {null, null, null, null};
        public Dictionary<DS4Controls, UInt16>[] customMapKeys = { null, null, null, null };
        public Dictionary<DS4Controls, X360Controls>[] customMapButtons = { null, null, null, null };
        public BackingStore()
        {
            for (int i = 0; i < 4; i++)
            {
                {
                    customMapKeyTypes[i] = new Dictionary<DS4Controls, DS4KeyType>();
                    customMapKeys[i] = new Dictionary<DS4Controls, UInt16>();
                    customMapButtons[i] = new Dictionary<DS4Controls, X360Controls>();
                }
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
        public DS4KeyType GetCustomKeyType(int device, DS4Controls controlName)
        {
            if (customMapKeyTypes[device].ContainsKey(controlName))
                return customMapKeyTypes[device][controlName];
            else return 0;
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
                XmlNode xmlTouchSensitivity = m_Xdoc.CreateNode(XmlNodeType.Element, "touchSensitivity", null); xmlTouchSensitivity.InnerText = touchSensitivity[device].ToString(); Node.AppendChild(xmlTouchSensitivity);
                XmlNode xmlLowRed = m_Xdoc.CreateNode(XmlNodeType.Element, "LowRed", null); xmlLowRed.InnerText = m_LowLeds[device][0].ToString(); Node.AppendChild(xmlLowRed);
                XmlNode xmlLowGreen = m_Xdoc.CreateNode(XmlNodeType.Element, "LowGreen", null); xmlLowGreen.InnerText = m_LowLeds[device][1].ToString(); Node.AppendChild(xmlLowGreen);
                XmlNode xmlLowBlue = m_Xdoc.CreateNode(XmlNodeType.Element, "LowBlue", null); xmlLowBlue.InnerText = m_LowLeds[device][2].ToString(); Node.AppendChild(xmlLowBlue);
                XmlNode xmlTouchpadJitterCompensation = m_Xdoc.CreateNode(XmlNodeType.Element, "touchpadJitterCompensation", null); xmlTouchpadJitterCompensation.InnerText = touchpadJitterCompensation[device].ToString(); Node.AppendChild(xmlTouchpadJitterCompensation);
                XmlNode xmlLowerRCOn = m_Xdoc.CreateNode(XmlNodeType.Element, "lowerRCOn", null); xmlLowerRCOn.InnerText = lowerRCOn[device].ToString(); Node.AppendChild(xmlLowerRCOn);
                XmlNode xmlTapSensitivity = m_Xdoc.CreateNode(XmlNodeType.Element, "tapSensitivity", null); xmlTapSensitivity.InnerText = tapSensitivity[device].ToString(); Node.AppendChild(xmlTapSensitivity);
                XmlNode xmlDouble = m_Xdoc.CreateNode(XmlNodeType.Element, "doubleTap", null); xmlDouble.InnerText = doubleTap[device].ToString(); Node.AppendChild(xmlDouble);
                XmlNode xmlScrollSensitivity = m_Xdoc.CreateNode(XmlNodeType.Element, "scrollSensitivity", null); xmlScrollSensitivity.InnerText = scrollSensitivity[device].ToString(); Node.AppendChild(xmlScrollSensitivity);
                XmlNode xmlLeftTriggerMiddle = m_Xdoc.CreateNode(XmlNodeType.Element, "LeftTriggerMiddle", null); xmlLeftTriggerMiddle.InnerText = m_LeftTriggerMiddle[device].ToString(); Node.AppendChild(xmlLeftTriggerMiddle);
                XmlNode xmlRightTriggerMiddle = m_Xdoc.CreateNode(XmlNodeType.Element, "RightTriggerMiddle", null); xmlRightTriggerMiddle.InnerText = m_RightTriggerMiddle[device].ToString(); Node.AppendChild(xmlRightTriggerMiddle);
                XmlNode xmlButtonMouseSensitivity = m_Xdoc.CreateNode(XmlNodeType.Element, "ButtonMouseSensitivity", null); xmlButtonMouseSensitivity.InnerText = buttonMouseSensitivity[device].ToString(); Node.AppendChild(xmlButtonMouseSensitivity);
                XmlNode xmlRainbow = m_Xdoc.CreateNode(XmlNodeType.Element, "Rainbow", null); xmlRainbow.InnerText = rainbow[device].ToString(); Node.AppendChild(xmlRainbow);

                XmlNode NodeControl = m_Xdoc.CreateNode(XmlNodeType.Element, "Control", null);

                XmlNode Key = m_Xdoc.CreateNode(XmlNodeType.Element, "Key", null);
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
                                /*if (button.ForeColor == System.Drawing.Color.Red)
                                    keyType += DS4KeyType.Repeat;*/
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
                            if (button.Tag is Int32 || button.Tag is UInt16)
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

                    try { Item = m_Xdoc.SelectSingleNode("/ScpControl/touchSensitivity"); Byte.TryParse(Item.InnerText, out touchSensitivity[device]); }
                    catch { missingSetting = true; }

                    try { Item = m_Xdoc.SelectSingleNode("/ScpControl/LowRed"); Byte.TryParse(Item.InnerText, out m_LowLeds[device][0]); }
                    catch { missingSetting = true; }
                    try { Item = m_Xdoc.SelectSingleNode("/ScpControl/LowGreen"); Byte.TryParse(Item.InnerText, out m_LowLeds[device][1]); }
                    catch { missingSetting = true; }
                    try { Item = m_Xdoc.SelectSingleNode("/ScpControl/LowBlue"); Byte.TryParse(Item.InnerText, out m_LowLeds[device][2]); }
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
                    try { Item = m_Xdoc.SelectSingleNode("/ScpControl/LeftTriggerMiddle"); Double.TryParse(Item.InnerText, out m_LeftTriggerMiddle[device]); }
                    catch { missingSetting = true; }
                    try { Item = m_Xdoc.SelectSingleNode("/ScpControl/RightTriggerMiddle"); Double.TryParse(Item.InnerText, out m_RightTriggerMiddle[device]); }
                    catch { missingSetting = true; }
                    try { Item = m_Xdoc.SelectSingleNode("/ScpControl/ButtonMouseSensitivity"); Int32.TryParse(Item.InnerText, out buttonMouseSensitivity[device]); }
                    catch { missingSetting = true; }
                    try { Item = m_Xdoc.SelectSingleNode("/ScpControl/Rainbow"); Double.TryParse(Item.InnerText, out rainbow[device]); }
                    catch { rainbow[device] = 0; missingSetting = true; }

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
                                    if (Item.InnerText.Contains(DS4KeyType.ScanCode.ToString()))
                                    {
                                        keyType |= DS4KeyType.ScanCode;
                                        button.Font = new System.Drawing.Font(button.Font, System.Drawing.FontStyle.Bold);
                                    }
                                    /*if (Item.InnerText.Contains(DS4KeyType.Repeat.ToString()))
                                    {
                                        keyType |= DS4KeyType.Repeat;
                                        button.ForeColor = System.Drawing.Color.Red;
                                    }*/
                                }
                                if (keyType != DS4KeyType.None)
                                    customMapKeyTypes.Add(getDS4ControlsByName(Item.Name), keyType);
                            }

                            Item = m_Xdoc.SelectSingleNode(String.Format("/ScpControl/Control/Key/{0}", button.Name));
                            if (Item != null)
                            {
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
            }
            // Only add missing settings if the actual load was graceful
            if (missingSetting && Loaded)
                SaveProfile(device, profilePath[device], buttons);

            return Loaded;
        }
        public Boolean LoadProfile(int device)
        {
            Boolean Loaded = true;
            Dictionary<DS4Controls, DS4KeyType> customMapKeyTypes = new Dictionary<DS4Controls, DS4KeyType>();
            Dictionary<DS4Controls, UInt16> customMapKeys = new Dictionary<DS4Controls, UInt16>();
            Dictionary<DS4Controls, X360Controls> customMapButtons = new Dictionary<DS4Controls, X360Controls>();
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

                    try { Item = m_Xdoc.SelectSingleNode("/ScpControl/touchSensitivity"); Byte.TryParse(Item.InnerText, out touchSensitivity[device]); }
                    catch { missingSetting = true; }

                    try { Item = m_Xdoc.SelectSingleNode("/ScpControl/LowRed"); Byte.TryParse(Item.InnerText, out m_LowLeds[device][0]); }
                    catch { missingSetting = true; }
                    try { Item = m_Xdoc.SelectSingleNode("/ScpControl/LowGreen"); Byte.TryParse(Item.InnerText, out m_LowLeds[device][1]); }
                    catch { missingSetting = true; }
                    try { Item = m_Xdoc.SelectSingleNode("/ScpControl/LowBlue"); Byte.TryParse(Item.InnerText, out m_LowLeds[device][2]); }
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
                    try { Item = m_Xdoc.SelectSingleNode("/ScpControl/LeftTriggerMiddle"); Double.TryParse(Item.InnerText, out m_LeftTriggerMiddle[device]); }
                    catch { missingSetting = true; }
                    try { Item = m_Xdoc.SelectSingleNode("/ScpControl/RightTriggerMiddle"); Double.TryParse(Item.InnerText, out m_RightTriggerMiddle[device]); }
                    catch { missingSetting = true; }
                    try { Item = m_Xdoc.SelectSingleNode("/ScpControl/ButtonMouseSensitivity"); Int32.TryParse(Item.InnerText, out buttonMouseSensitivity[device]); }
                    catch { missingSetting = true; }
                    try { Item = m_Xdoc.SelectSingleNode("/ScpControl/Rainbow"); Double.TryParse(Item.InnerText, out rainbow[device]); }
                    catch { rainbow[device] = 0; missingSetting = true; }

                    DS4KeyType keyType;
                    UInt16 wvk;
                    XmlNode ParentItem = m_Xdoc.SelectSingleNode("/ScpControl/Control/Button");
                    if (ParentItem != null)
                        foreach (XmlNode item in ParentItem.ChildNodes)
                            customMapButtons.Add(getDS4ControlsByName(item.Name), getX360ControlsByName(item.InnerText));
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
                                //if (item.InnerText.Contains(DS4KeyType.Repeat.ToString()))
                                    //keyType |= DS4KeyType.Repeat;
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
            }
            // Only add missing settings if the actual load was graceful
            if (missingSetting && Loaded)
                SaveProfile(device, profilePath[device], null);

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
                m_Xdoc.AppendChild(Node);

                m_Xdoc.Save(m_Profile);
            }
            catch { Saved = false; }

            return Saved;
        }
    }
}
