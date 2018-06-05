using System;
using System.Collections.Generic;
using System.Linq;

using System.Threading.Tasks;
using System.Diagnostics;
using static DS4Windows.Global;

namespace DS4Windows
{
    public class Mapping
    {
        /*
         * Represent the synthetic keyboard and mouse events.  Maintain counts for each so we don't duplicate events.
         */
        public class SyntheticState
        {
            public struct MouseClick
            {
                public int leftCount, middleCount, rightCount, fourthCount, fifthCount, wUpCount, wDownCount, toggleCount;
                public bool toggle;
            }
            public MouseClick previousClicks, currentClicks;
            public struct KeyPress
            {
                public int vkCount, scanCodeCount, repeatCount, toggleCount; // repeat takes priority over non-, and scancode takes priority over non-
                public bool toggle;
            }
            public class KeyPresses
            {
                public KeyPress previous, current;
            }
            public Dictionary<UInt16, KeyPresses> keyPresses = new Dictionary<UInt16, KeyPresses>();

            public void SavePrevious(bool performClear)
            {
                previousClicks = currentClicks;
                if (performClear)
                    currentClicks.leftCount = currentClicks.middleCount = currentClicks.rightCount = currentClicks.fourthCount = currentClicks.fifthCount = currentClicks.wUpCount = currentClicks.wDownCount = currentClicks.toggleCount = 0;

                //foreach (KeyPresses kp in keyPresses.Values)
                Dictionary<ushort, KeyPresses>.ValueCollection keyValues = keyPresses.Values;
                for (int i = 0, kpCount = keyValues.Count; i < kpCount; i++)
                {
                    KeyPresses kp = keyValues.ElementAt(i);
                    kp.previous = kp.current;
                    if (performClear)
                    {
                        kp.current.repeatCount = kp.current.scanCodeCount = kp.current.vkCount = kp.current.toggleCount = 0;
                        //kp.current.toggle = false;
                    }
                }
            }
        }

        public class ActionState
        {
            public bool[] dev = new bool[4];
        }

        public static SyntheticState globalState = new SyntheticState();
        public static SyntheticState[] deviceState = new SyntheticState[4]
            { new SyntheticState(), new SyntheticState(), new SyntheticState(),
              new SyntheticState() };

        public static DS4StateFieldMapping[] fieldMappings = new DS4StateFieldMapping[4] {
            new DS4StateFieldMapping(), new DS4StateFieldMapping(), new DS4StateFieldMapping(),
            new DS4StateFieldMapping()
        };
        public static DS4StateFieldMapping[] outputFieldMappings = new DS4StateFieldMapping[4]
        {
            new DS4StateFieldMapping(), new DS4StateFieldMapping(), new DS4StateFieldMapping(),
            new DS4StateFieldMapping()
        };
        public static DS4StateFieldMapping[] previousFieldMappings = new DS4StateFieldMapping[4]
        {
            new DS4StateFieldMapping(), new DS4StateFieldMapping(), new DS4StateFieldMapping(),
            new DS4StateFieldMapping()
        };

        // TODO When we disconnect, process a null/dead state to release any keys or buttons.
        public static DateTime oldnow = DateTime.UtcNow;
        private static bool pressagain = false;
        private static int wheel = 0, keyshelddown = 0;

        //mapcustom
        public static bool[] pressedonce = new bool[261], macrodone = new bool[38];
        static bool[] macroControl = new bool[25];
        static uint macroCount = 0;

        //actions
        public static int[] fadetimer = new int[4] { 0, 0, 0, 0 };
        public static int[] prevFadetimer = new int[4] { 0, 0, 0, 0 };
        public static DS4Color[] lastColor = new DS4Color[4];
        public static List<ActionState> actionDone = new List<ActionState>();
        public static SpecialAction[] untriggeraction = new SpecialAction[4];
        public static DateTime[] nowAction = { DateTime.MinValue, DateTime.MinValue, DateTime.MinValue, DateTime.MinValue };
        public static DateTime[] oldnowAction = { DateTime.MinValue, DateTime.MinValue, DateTime.MinValue, DateTime.MinValue };
        public static int[] untriggerindex = new int[4] { -1, -1, -1, -1 };
        public static DateTime[] oldnowKeyAct = new DateTime[4] { DateTime.MinValue,
            DateTime.MinValue, DateTime.MinValue, DateTime.MinValue };

        private static DS4Controls[] shiftTriggerMapping = new DS4Controls[26] { DS4Controls.None, DS4Controls.Cross, DS4Controls.Circle, DS4Controls.Square,
            DS4Controls.Triangle, DS4Controls.Options, DS4Controls.Share, DS4Controls.DpadUp, DS4Controls.DpadDown,
            DS4Controls.DpadLeft, DS4Controls.DpadRight, DS4Controls.PS, DS4Controls.L1, DS4Controls.R1, DS4Controls.L2,
            DS4Controls.R2, DS4Controls.L3, DS4Controls.R3, DS4Controls.TouchLeft, DS4Controls.TouchUpper, DS4Controls.TouchMulti,
            DS4Controls.TouchRight, DS4Controls.GyroZNeg, DS4Controls.GyroZPos, DS4Controls.GyroXPos, DS4Controls.GyroXNeg,
        };

        private static int[] ds4ControlMapping = new int[38] { 0, // DS4Control.None
            16, // DS4Controls.LXNeg
            20, // DS4Controls.LXPos
            17, // DS4Controls.LYNeg
            21, // DS4Controls.LYPos
            18, // DS4Controls.RXNeg
            22, // DS4Controls.RXPos
            19, // DS4Controls.RYNeg
            23, // DS4Controls.RYPos
            3,  // DS4Controls.L1
            24, // DS4Controls.L2
            5,  // DS4Controls.L3
            4,  // DS4Controls.R1
            25, // DS4Controls.R2
            6,  // DS4Controls.R3
            13, // DS4Controls.Square
            14, // DS4Controls.Triangle
            15, // DS4Controls.Circle
            12, // DS4Controls.Cross
            7,  // DS4Controls.DpadUp
            10, // DS4Controls.DpadRight
            8,  // DS4Controls.DpadDown
            9,  // DS4Controls.DpadLeft
            11, // DS4Controls.PS
            27, // DS4Controls.TouchLeft
            29, // DS4Controls.TouchUpper
            26, // DS4Controls.TouchMulti
            28, // DS4Controls.TouchRight
            1,  // DS4Controls.Share
            2,  // DS4Controls.Options
            31, // DS4Controls.GyroXPos
            30, // DS4Controls.GyroXNeg
            33, // DS4Controls.GyroZPos
            32, // DS4Controls.GyroZNeg
            34, // DS4Controls.SwipeLeft
            35, // DS4Controls.SwipeRight
            36, // DS4Controls.SwipeUp
            37  // DS4Controls.SwipeDown
        };

        // Define here to save some time processing.
        // It is enough to feel a difference during gameplay.
        private static int[] rsOutCurveModeArray = new int[4] { 0, 0, 0, 0 };
        private static int[] lsOutCurveModeArray = new int[4] { 0, 0, 0, 0 };
        static bool tempBool = false;
        private static double[] tempDoubleArray = new double[4] { 0.0, 0.0, 0.0, 0.0 };
        private static int[] tempIntArray = new int[4] { 0, 0, 0, 0 };

        // Special macros
        static bool altTabDone = true;
        static DateTime altTabNow = DateTime.UtcNow,
            oldAltTabNow = DateTime.UtcNow - TimeSpan.FromSeconds(1);

        // Mouse
        public static int mcounter = 34;
        public static int mouseaccel = 0;
        public static int prevmouseaccel = 0;
        private static double horizontalRemainder = 0.0, verticalRemainder = 0.0;
        private const int MOUSESPEEDFACTOR = 48;
        private const double MOUSESTICKOFFSET = 0.032;

        public static void Commit(int device)
        {
            SyntheticState state = deviceState[device];
            lock (globalState)
            {
                globalState.currentClicks.leftCount += state.currentClicks.leftCount - state.previousClicks.leftCount;
                globalState.currentClicks.middleCount += state.currentClicks.middleCount - state.previousClicks.middleCount;
                globalState.currentClicks.rightCount += state.currentClicks.rightCount - state.previousClicks.rightCount;
                globalState.currentClicks.fourthCount += state.currentClicks.fourthCount - state.previousClicks.fourthCount;
                globalState.currentClicks.fifthCount += state.currentClicks.fifthCount - state.previousClicks.fifthCount;
                globalState.currentClicks.wUpCount += state.currentClicks.wUpCount - state.previousClicks.wUpCount;
                globalState.currentClicks.wDownCount += state.currentClicks.wDownCount - state.previousClicks.wDownCount;
                globalState.currentClicks.toggleCount += state.currentClicks.toggleCount - state.previousClicks.toggleCount;
                globalState.currentClicks.toggle = state.currentClicks.toggle;

                if (globalState.currentClicks.toggleCount != 0 && globalState.previousClicks.toggleCount == 0 && globalState.currentClicks.toggle)
                {
                    if (globalState.currentClicks.leftCount != 0 && globalState.previousClicks.leftCount == 0)
                        InputMethods.MouseEvent(InputMethods.MOUSEEVENTF_LEFTDOWN);
                    if (globalState.currentClicks.rightCount != 0 && globalState.previousClicks.rightCount == 0)
                        InputMethods.MouseEvent(InputMethods.MOUSEEVENTF_RIGHTDOWN);
                    if (globalState.currentClicks.middleCount != 0 && globalState.previousClicks.middleCount == 0)
                        InputMethods.MouseEvent(InputMethods.MOUSEEVENTF_MIDDLEDOWN);
                    if (globalState.currentClicks.fourthCount != 0 && globalState.previousClicks.fourthCount == 0)
                        InputMethods.MouseEvent(InputMethods.MOUSEEVENTF_XBUTTONDOWN, 1);
                    if (globalState.currentClicks.fifthCount != 0 && globalState.previousClicks.fifthCount == 0)
                        InputMethods.MouseEvent(InputMethods.MOUSEEVENTF_XBUTTONDOWN, 2);
                }
                else if (globalState.currentClicks.toggleCount != 0 && globalState.previousClicks.toggleCount == 0 && !globalState.currentClicks.toggle)
                {
                    if (globalState.currentClicks.leftCount != 0 && globalState.previousClicks.leftCount == 0)
                        InputMethods.MouseEvent(InputMethods.MOUSEEVENTF_LEFTUP);
                    if (globalState.currentClicks.rightCount != 0 && globalState.previousClicks.rightCount == 0)
                        InputMethods.MouseEvent(InputMethods.MOUSEEVENTF_RIGHTUP);
                    if (globalState.currentClicks.middleCount != 0 && globalState.previousClicks.middleCount == 0)
                        InputMethods.MouseEvent(InputMethods.MOUSEEVENTF_MIDDLEUP);
                    if (globalState.currentClicks.fourthCount != 0 && globalState.previousClicks.fourthCount == 0)
                        InputMethods.MouseEvent(InputMethods.MOUSEEVENTF_XBUTTONUP, 1);
                    if (globalState.currentClicks.fifthCount != 0 && globalState.previousClicks.fifthCount == 0)
                        InputMethods.MouseEvent(InputMethods.MOUSEEVENTF_XBUTTONUP, 2);
                }

                if (globalState.currentClicks.toggleCount == 0 && globalState.previousClicks.toggleCount == 0)
                {
                    if (globalState.currentClicks.leftCount != 0 && globalState.previousClicks.leftCount == 0)
                        InputMethods.MouseEvent(InputMethods.MOUSEEVENTF_LEFTDOWN);
                    else if (globalState.currentClicks.leftCount == 0 && globalState.previousClicks.leftCount != 0)
                        InputMethods.MouseEvent(InputMethods.MOUSEEVENTF_LEFTUP);

                    if (globalState.currentClicks.middleCount != 0 && globalState.previousClicks.middleCount == 0)
                        InputMethods.MouseEvent(InputMethods.MOUSEEVENTF_MIDDLEDOWN);
                    else if (globalState.currentClicks.middleCount == 0 && globalState.previousClicks.middleCount != 0)
                        InputMethods.MouseEvent(InputMethods.MOUSEEVENTF_MIDDLEUP);

                    if (globalState.currentClicks.rightCount != 0 && globalState.previousClicks.rightCount == 0)
                        InputMethods.MouseEvent(InputMethods.MOUSEEVENTF_RIGHTDOWN);
                    else if (globalState.currentClicks.rightCount == 0 && globalState.previousClicks.rightCount != 0)
                        InputMethods.MouseEvent(InputMethods.MOUSEEVENTF_RIGHTUP);

                    if (globalState.currentClicks.fourthCount != 0 && globalState.previousClicks.fourthCount == 0)
                        InputMethods.MouseEvent(InputMethods.MOUSEEVENTF_XBUTTONDOWN, 1);
                    else if (globalState.currentClicks.fourthCount == 0 && globalState.previousClicks.fourthCount != 0)
                        InputMethods.MouseEvent(InputMethods.MOUSEEVENTF_XBUTTONUP, 1);

                    if (globalState.currentClicks.fifthCount != 0 && globalState.previousClicks.fifthCount == 0)
                        InputMethods.MouseEvent(InputMethods.MOUSEEVENTF_XBUTTONDOWN, 2);
                    else if (globalState.currentClicks.fifthCount == 0 && globalState.previousClicks.fifthCount != 0)
                        InputMethods.MouseEvent(InputMethods.MOUSEEVENTF_XBUTTONUP, 2);

                    if (globalState.currentClicks.wUpCount != 0 && globalState.previousClicks.wUpCount == 0)
                    {
                        InputMethods.MouseEvent(InputMethods.MOUSEEVENTF_WHEEL, 120);
                        oldnow = DateTime.UtcNow;
                        wheel = 120;
                    }
                    else if (globalState.currentClicks.wUpCount == 0 && globalState.previousClicks.wUpCount != 0)
                        wheel = 0;

                    if (globalState.currentClicks.wDownCount != 0 && globalState.previousClicks.wDownCount == 0)
                    {
                        InputMethods.MouseEvent(InputMethods.MOUSEEVENTF_WHEEL, -120);
                        oldnow = DateTime.UtcNow;
                        wheel = -120;
                    }
                    if (globalState.currentClicks.wDownCount == 0 && globalState.previousClicks.wDownCount != 0)
                        wheel = 0;
                }
            

                if (wheel != 0) //Continue mouse wheel movement
                {
                    DateTime now = DateTime.UtcNow;
                    if (now >= oldnow + TimeSpan.FromMilliseconds(100) && !pressagain)
                    {
                        oldnow = now;
                        InputMethods.MouseEvent(InputMethods.MOUSEEVENTF_WHEEL, wheel);
                    }
                }

                // Merge and synthesize all key presses/releases that are present in this device's mapping.
                // TODO what about the rest?  e.g. repeat keys really ought to be on some set schedule
                Dictionary<UInt16, SyntheticState.KeyPresses>.KeyCollection kvpKeys = state.keyPresses.Keys;
                //foreach (KeyValuePair<UInt16, SyntheticState.KeyPresses> kvp in state.keyPresses)
                for (int i = 0, keyCount = kvpKeys.Count; i < keyCount; i++)
                {
                    UInt16 kvpKey = kvpKeys.ElementAt(i);
                    SyntheticState.KeyPresses kvpValue = state.keyPresses[kvpKey];

                    SyntheticState.KeyPresses gkp;
                    if (globalState.keyPresses.TryGetValue(kvpKey, out gkp))
                    {
                        gkp.current.vkCount += kvpValue.current.vkCount - kvpValue.previous.vkCount;
                        gkp.current.scanCodeCount += kvpValue.current.scanCodeCount - kvpValue.previous.scanCodeCount;
                        gkp.current.repeatCount += kvpValue.current.repeatCount - kvpValue.previous.repeatCount;
                        gkp.current.toggle = kvpValue.current.toggle;
                        gkp.current.toggleCount += kvpValue.current.toggleCount - kvpValue.previous.toggleCount;
                    }
                    else
                    {
                        gkp = new SyntheticState.KeyPresses();
                        gkp.current = kvpValue.current;
                        globalState.keyPresses[kvpKey] = gkp;
                    }
                    if (gkp.current.toggleCount != 0 && gkp.previous.toggleCount == 0 && gkp.current.toggle)
                    {
                        if (gkp.current.scanCodeCount != 0)
                            InputMethods.performSCKeyPress(kvpKey);
                        else
                            InputMethods.performKeyPress(kvpKey);
                    }
                    else if (gkp.current.toggleCount != 0 && gkp.previous.toggleCount == 0 && !gkp.current.toggle)
                    {
                        if (gkp.previous.scanCodeCount != 0) // use the last type of VK/SC
                            InputMethods.performSCKeyRelease(kvpKey);
                        else
                            InputMethods.performKeyRelease(kvpKey);
                    }
                    else if (gkp.current.vkCount + gkp.current.scanCodeCount != 0 && gkp.previous.vkCount + gkp.previous.scanCodeCount == 0)
                    {
                        if (gkp.current.scanCodeCount != 0)
                        {
                            oldnow = DateTime.UtcNow;
                            InputMethods.performSCKeyPress(kvpKey);
                            pressagain = false;
                            keyshelddown = kvpKey;
                        }
                        else
                        {
                            oldnow = DateTime.UtcNow;
                            InputMethods.performKeyPress(kvpKey);
                            pressagain = false;
                            keyshelddown = kvpKey;
                        }
                    }
                    else if (gkp.current.toggleCount != 0 || gkp.previous.toggleCount != 0 || gkp.current.repeatCount != 0 || // repeat or SC/VK transition
                        ((gkp.previous.scanCodeCount == 0) != (gkp.current.scanCodeCount == 0))) //repeat keystroke after 500ms
                    {
                        if (keyshelddown == kvpKey)
                        {
                            DateTime now = DateTime.UtcNow;
                            if (now >= oldnow + TimeSpan.FromMilliseconds(500) && !pressagain)
                            {
                                oldnow = now;
                                pressagain = true;
                            }
                            if (pressagain && gkp.current.scanCodeCount != 0)
                            {
                                now = DateTime.UtcNow;
                                if (now >= oldnow + TimeSpan.FromMilliseconds(25) && pressagain)
                                {
                                    oldnow = now;
                                    InputMethods.performSCKeyPress(kvpKey);
                                }
                            }
                            else if (pressagain)
                            {
                                now = DateTime.UtcNow;
                                if (now >= oldnow + TimeSpan.FromMilliseconds(25) && pressagain)
                                {
                                    oldnow = now;
                                    InputMethods.performKeyPress(kvpKey);
                                }
                            }
                        }
                    }
                    if ((gkp.current.toggleCount == 0 && gkp.previous.toggleCount == 0) && gkp.current.vkCount + gkp.current.scanCodeCount == 0 && gkp.previous.vkCount + gkp.previous.scanCodeCount != 0)
                    {
                        if (gkp.previous.scanCodeCount != 0) // use the last type of VK/SC
                        {
                            InputMethods.performSCKeyRelease(kvpKey);
                            pressagain = false;
                        }
                        else
                        {
                            InputMethods.performKeyRelease(kvpKey);
                            pressagain = false;
                        }
                    }
                }
                globalState.SavePrevious(false);
            }
            state.SavePrevious(true);
        }

        public enum Click { None, Left, Middle, Right, Fourth, Fifth, WUP, WDOWN };
        public static void MapClick(int device, Click mouseClick)
        {
            switch (mouseClick)
            {
                case Click.Left:
                    deviceState[device].currentClicks.leftCount++;
                    break;
                case Click.Middle:
                    deviceState[device].currentClicks.middleCount++;
                    break;
                case Click.Right:
                    deviceState[device].currentClicks.rightCount++;
                    break;
                case Click.Fourth:
                    deviceState[device].currentClicks.fourthCount++;
                    break;
                case Click.Fifth:
                    deviceState[device].currentClicks.fifthCount++;
                    break;
                case Click.WUP:
                    deviceState[device].currentClicks.wUpCount++;
                    break;
                case Click.WDOWN:
                    deviceState[device].currentClicks.wDownCount++;
                    break;
                default: break;
            }
        }

        public static int DS4ControltoInt(DS4Controls ctrl)
        {
            int result = 0;
            if (ctrl >= DS4Controls.None && ctrl <= DS4Controls.SwipeDown)
            {
                result = ds4ControlMapping[(int)ctrl];
            }

            return result;
        }

        static double TValue(double value1, double value2, double percent)
        {
            percent /= 100f;
            return value1 * percent + value2 * (1 - percent);
        }

        private static int ClampInt(int min, int value, int max)
        {
            return (value < min) ? min : (value > max) ? max : value;
        }

        public static DS4State SetCurveAndDeadzone(int device, DS4State cState, DS4State dState)
        {
            double rotation = tempDoubleArray[device] = getLSRotation(device);
            if (rotation > 0.0 || rotation < 0.0)
                cState.rotateLSCoordinates(rotation);

            double rotationRS = tempDoubleArray[device] = getRSRotation(device);
            if (rotationRS > 0.0 || rotationRS < 0.0)
                cState.rotateRSCoordinates(rotationRS);

            cState.CopyTo(dState);
            //DS4State dState = new DS4State(cState);
            int x;
            int y;
            int curve;

            /* TODO: Look into curve options and make sure maximum axes values are being respected */
            int lsCurve = getLSCurve(device);
            if (lsCurve > 0)
            {
                x = cState.LX;
                y = cState.LY;
                float max = x + y;
                double curvex;
                double curvey;
                curve = lsCurve;
                double multimax = TValue(382.5, max, curve);
                double multimin = TValue(127.5, max, curve);
                if ((x > 127.5f && y > 127.5f) || (x < 127.5f && y < 127.5f))
                {
                    curvex = (x > 127.5f ? Math.Min(x, (x / max) * multimax) : Math.Max(x, (x / max) * multimin));
                    curvey = (y > 127.5f ? Math.Min(y, (y / max) * multimax) : Math.Max(y, (y / max) * multimin));
                }
                else
                {
                    if (x < 127.5f)
                    {
                        curvex = Math.Min(x, (x / max) * multimax);
                        curvey = Math.Min(y, (-(y / max) * multimax + 510));
                    }
                    else
                    {
                        curvex = Math.Min(x, (-(x / max) * multimax + 510));
                        curvey = Math.Min(y, (y / max) * multimax);
                    }
                }

                dState.LX = (byte)Math.Round(curvex, 0);
                dState.LY = (byte)Math.Round(curvey, 0);
            }

            /* TODO: Look into curve options and make sure maximum axes values are being respected */
            int rsCurve = getRSCurve(device);
            if (rsCurve > 0)
            {
                x = cState.RX;
                y = cState.RY;
                float max = x + y;
                double curvex;
                double curvey;
                curve = rsCurve;
                double multimax = TValue(382.5, max, curve);
                double multimin = TValue(127.5, max, curve);
                if ((x > 127.5f && y > 127.5f) || (x < 127.5f && y < 127.5f))
                {
                    curvex = (x > 127.5f ? Math.Min(x, (x / max) * multimax) : Math.Max(x, (x / max) * multimin));
                    curvey = (y > 127.5f ? Math.Min(y, (y / max) * multimax) : Math.Max(y, (y / max) * multimin));
                }
                else
                {
                    if (x < 127.5f)
                    {
                        curvex = Math.Min(x, (x / max) * multimax);
                        curvey = Math.Min(y, (-(y / max) * multimax + 510));
                    }
                    else
                    {
                        curvex = Math.Min(x, (-(x / max) * multimax + 510));
                        curvey = Math.Min(y, (y / max) * multimax);
                    }
                }

                dState.RX = (byte)Math.Round(curvex, 0);
                dState.RY = (byte)Math.Round(curvey, 0);
            }

            int lsDeadzone = getLSDeadzone(device);
            int lsAntiDead = getLSAntiDeadzone(device);
            int lsMaxZone = getLSMaxzone(device);
            if (lsDeadzone > 0 || lsAntiDead > 0 || lsMaxZone != 100)
            {
                double lsSquared = Math.Pow(cState.LX - 127.5f, 2) + Math.Pow(cState.LY - 127.5f, 2);
                double lsDeadzoneSquared = Math.Pow(lsDeadzone, 2);
                if (lsDeadzone > 0 && lsSquared <= lsDeadzoneSquared)
                {
                    dState.LX = 127;
                    dState.LY = 127;
                }
                else if ((lsDeadzone > 0 && lsSquared > lsDeadzoneSquared) || lsAntiDead > 0 || lsMaxZone != 100)
                {
                    double r = Math.Atan2(-(dState.LY - 127.5), (dState.LX - 127.5));
                    double maxXValue = dState.LX >= 127.5 ? 127.5 : -127.5;
                    double maxYValue = dState.LY >= 127.5 ? 127.5 : -127.5;
                    double ratio = lsMaxZone / 100.0;

                    double maxZoneXNegValue = (ratio * -127.5) + 127.5;
                    double maxZoneXPosValue = (ratio * 127.5) + 127.5;
                    double maxZoneYNegValue = maxZoneXNegValue;
                    double maxZoneYPosValue = maxZoneXPosValue;
                    double maxZoneX = dState.LX >= 127.5 ? (maxZoneXPosValue - 127.5) : (maxZoneXNegValue - 127.5);
                    double maxZoneY = dState.LY >= 127.5 ? (maxZoneYPosValue - 127.5) : (maxZoneYNegValue - 127.5);

                    double tempLsXDead = 0.0, tempLsYDead = 0.0;
                    double tempOutputX = 0.0, tempOutputY = 0.0;
                    if (lsDeadzone > 0)
                    {
                        tempLsXDead = Math.Abs(Math.Cos(r)) * (lsDeadzone / 127.0) * maxXValue;
                        tempLsYDead = Math.Abs(Math.Sin(r)) * (lsDeadzone / 127.0) * maxYValue;

                        if (lsSquared > lsDeadzoneSquared)
                        {
                            double currentX = Global.Clamp(maxZoneXNegValue, dState.LX, maxZoneXPosValue);
                            double currentY = Global.Clamp(maxZoneYNegValue, dState.LY, maxZoneYPosValue);
                            tempOutputX = ((currentX - 127.5 - tempLsXDead) / (maxZoneX - tempLsXDead));
                            tempOutputY = ((currentY - 127.5 - tempLsYDead) / (maxZoneY - tempLsYDead));
                        }
                    }
                    else
                    {
                        double currentX = Global.Clamp(maxZoneXNegValue, dState.LX, maxZoneXPosValue);
                        double currentY = Global.Clamp(maxZoneYNegValue, dState.LY, maxZoneYPosValue);
                        tempOutputX = (currentX - 127.5) / maxZoneX;
                        tempOutputY = (currentY - 127.5) / maxZoneY;
                    }

                    double tempLsXAntiDeadPercent = 0.0, tempLsYAntiDeadPercent = 0.0;
                    if (lsAntiDead > 0)
                    {
                        tempLsXAntiDeadPercent = (lsAntiDead * 0.01) * Math.Abs(Math.Cos(r));
                        tempLsYAntiDeadPercent = (lsAntiDead * 0.01) * Math.Abs(Math.Sin(r));
                    }

                    if (tempOutputX > 0.0)
                    {
                        dState.LX = (byte)((((1.0 - tempLsXAntiDeadPercent) * tempOutputX + tempLsXAntiDeadPercent)) * maxXValue + 127.5);
                    }
                    else
                    {
                        dState.LX = 127;
                    }

                    if (tempOutputY > 0.0)
                    {
                        dState.LY = (byte)((((1.0 - tempLsYAntiDeadPercent) * tempOutputY + tempLsYAntiDeadPercent)) * maxYValue + 127.5);
                    }
                    else
                    {
                        dState.LY = 127;
                    }
                }
            }

            int rsDeadzone = getRSDeadzone(device);
            int rsAntiDead = getRSAntiDeadzone(device);
            int rsMaxZone = getRSMaxzone(device);
            if (rsDeadzone > 0 || rsAntiDead > 0 || rsMaxZone != 100)
            {
                double rsSquared = Math.Pow(cState.RX - 127.5, 2) + Math.Pow(cState.RY - 127.5, 2);
                double rsDeadzoneSquared = Math.Pow(rsDeadzone, 2);
                if (rsDeadzone > 0 && rsSquared <= rsDeadzoneSquared)
                {
                    dState.RX = 127;
                    dState.RY = 127;
                }
                else if ((rsDeadzone > 0 && rsSquared > rsDeadzoneSquared) || rsAntiDead > 0 || rsMaxZone != 100)
                {
                    double r = Math.Atan2(-(dState.RY - 127.5), (dState.RX - 127.5));
                    double maxXValue = dState.RX >= 127.5 ? 127.5 : -127.5;
                    double maxYValue = dState.RY >= 127.5 ? 127.5 : -127.5;
                    double ratio = rsMaxZone / 100.0;

                    double maxZoneXNegValue = (ratio * -127.5) + 127.5;
                    double maxZoneXPosValue = (ratio * 127.5) + 127.5;
                    double maxZoneYNegValue = maxZoneXNegValue;
                    double maxZoneYPosValue = maxZoneXPosValue;
                    double maxZoneX = dState.RX >= 127.5 ? (maxZoneXPosValue - 127.5) : (maxZoneXNegValue - 127.5);
                    double maxZoneY = dState.RY >= 127.5 ? (maxZoneYPosValue - 127.5) : (maxZoneYNegValue - 127.5);

                    double tempRsXDead = 0.0, tempRsYDead = 0.0;
                    double tempOutputX = 0.0, tempOutputY = 0.0;
                    if (rsDeadzone > 0)
                    {
                        tempRsXDead = Math.Abs(Math.Cos(r)) * (rsDeadzone / 127.0) * maxXValue;
                        tempRsYDead = Math.Abs(Math.Sin(r)) * (rsDeadzone / 127.0) * maxYValue;

                        if (rsSquared > rsDeadzoneSquared)
                        {
                            double currentX = Global.Clamp(maxZoneXNegValue, dState.RX, maxZoneXPosValue);
                            double currentY = Global.Clamp(maxZoneYNegValue, dState.RY, maxZoneYPosValue);

                            tempOutputX = ((currentX - 127.5 - tempRsXDead) / (maxZoneX - tempRsXDead));
                            tempOutputY = ((currentY - 127.5 - tempRsYDead) / (maxZoneY - tempRsYDead));
                        }
                    }
                    else
                    {
                        double currentX = Global.Clamp(maxZoneXNegValue, dState.RX, maxZoneXPosValue);
                        double currentY = Global.Clamp(maxZoneYNegValue, dState.RY, maxZoneYPosValue);

                        tempOutputX = (currentX - 127.5) / maxZoneX;
                        tempOutputY = (currentY - 127.5) / maxZoneY;
                    }

                    double tempRsXAntiDeadPercent = 0.0, tempRsYAntiDeadPercent = 0.0;
                    if (rsAntiDead > 0)
                    {
                        tempRsXAntiDeadPercent = (rsAntiDead * 0.01) * Math.Abs(Math.Cos(r));
                        tempRsYAntiDeadPercent = (rsAntiDead * 0.01) * Math.Abs(Math.Sin(r));
                    }

                    if (tempOutputX > 0.0)
                    {
                        dState.RX = (byte)((((1.0 - tempRsXAntiDeadPercent) * tempOutputX + tempRsXAntiDeadPercent)) * maxXValue + 127.5);
                    }
                    else
                    {
                        dState.RX = 127;
                    }

                    if (tempOutputY > 0.0)
                    {
                        dState.RY = (byte)((((1.0 - tempRsYAntiDeadPercent) * tempOutputY + tempRsYAntiDeadPercent)) * maxYValue + 127.5);
                    }
                    else
                    {
                        dState.RY = 127;
                    }
                }
            }

            byte l2Deadzone = getL2Deadzone(device);
            int l2AntiDeadzone = getL2AntiDeadzone(device);
            int l2Maxzone = getL2Maxzone(device);
            if (l2Deadzone > 0 || l2AntiDeadzone > 0 || l2Maxzone != 100)
            {
                double tempL2Output = cState.L2 / 255.0;
                double tempL2AntiDead = 0.0;
                double ratio = l2Maxzone / 100.0;
                double maxValue = 255.0 * ratio;

                if (l2Deadzone > 0)
                {
                    if (cState.L2 > l2Deadzone)
                    {
                        double current = Global.Clamp(0, dState.L2, maxValue);
                        tempL2Output = (current - l2Deadzone) / (maxValue - l2Deadzone);
                    }
                    else
                    {
                        tempL2Output = 0.0;
                    }
                }

                if (l2AntiDeadzone > 0)
                {
                    tempL2AntiDead = l2AntiDeadzone * 0.01;
                }

                if (tempL2Output > 0.0)
                {
                    dState.L2 = (byte)(((1.0 - tempL2AntiDead) * tempL2Output + tempL2AntiDead) * 255.0);
                }
                else
                {
                    dState.L2 = 0;
                }
            }

            byte r2Deadzone = getR2Deadzone(device);
            int r2AntiDeadzone = getR2AntiDeadzone(device);
            int r2Maxzone = getR2Maxzone(device);
            if (r2Deadzone > 0 || r2AntiDeadzone > 0 || r2Maxzone != 100)
            {
                double tempR2Output = cState.R2 / 255.0;
                double tempR2AntiDead = 0.0;
                double ratio = r2Maxzone / 100.0;
                double maxValue = 255 * ratio;

                if (r2Deadzone > 0)
                {
                    if (cState.R2 > r2Deadzone)
                    {
                        double current = Global.Clamp(0, dState.R2, maxValue);
                        tempR2Output = (current - r2Deadzone) / (maxValue - r2Deadzone);
                    }
                    else
                    {
                        tempR2Output = 0.0;
                    }
                }

                if (r2AntiDeadzone > 0)
                {
                    tempR2AntiDead = r2AntiDeadzone * 0.01;
                }

                if (tempR2Output > 0.0)
                {
                    dState.R2 = (byte)(((1.0 - tempR2AntiDead) * tempR2Output + tempR2AntiDead) * 255.0);
                }
                else
                {
                    dState.R2 = 0;
                }
            }

            double lsSens = getLSSens(device);
            if (lsSens != 1.0)
            {
                dState.LX = (byte)Global.Clamp(0, lsSens * (dState.LX - 127.5) + 127.5, 255);
                dState.LY = (byte)Global.Clamp(0, lsSens * (dState.LY - 127.5) + 127.5, 255);
            }

            double rsSens = getRSSens(device);
            if (rsSens != 1.0)
            {
                dState.RX = (byte)Global.Clamp(0, rsSens * (dState.RX - 127.5) + 127.5, 255);
                dState.RY = (byte)Global.Clamp(0, rsSens * (dState.RY - 127.5) + 127.5, 255);
            }

            double l2Sens = getL2Sens(device);
            if (l2Sens != 1.0)
                dState.L2 = (byte)Global.Clamp(0, l2Sens * dState.L2, 255);

            double r2Sens = getR2Sens(device);
            if (r2Sens != 1.0)
                dState.R2 = (byte)Global.Clamp(0, r2Sens * dState.R2, 255);

            int lsOutCurveMode = lsOutCurveModeArray[device] = getLsOutCurveMode(device);
            if (lsOutCurveMode > 0)
            {
                double tempX = (dState.LX - 127.5) / 127.5;
                double tempY = (dState.LY - 127.5) / 127.5;
                double signX = tempX >= 0.0 ? 1.0 : -1.0;
                double signY = tempY >= 0.0 ? 1.0 : -1.0;

                if (lsOutCurveMode == 1)
                {
                    double absX = Math.Abs(tempX);
                    double absY = Math.Abs(tempY);
                    double outputX = 0.0;
                    double outputY = 0.0;

                    if (absX <= 0.4)
                    {
                        outputX = 0.414 * absX;
                    }
                    else if (absX <= 0.75)
                    {
                        outputX = absX - 0.2344;
                    }
                    else if (absX > 0.75)
                    {
                        outputX = (absX * 1.9376) - 0.9376;
                    }

                    if (absY <= 0.4)
                    {
                        outputY = 0.414 * absY;
                    }
                    else if (absY <= 0.75)
                    {
                        outputY = absY - 0.2344;
                    }
                    else if (absY > 0.75)
                    {
                        outputY = (absY * 1.9376) - 0.9376;
                    }

                    dState.LX = (byte)(outputX * signX * 127.5 + 127.5);
                    dState.LY = (byte)(outputY * signY * 127.5 + 127.5);
                }
                else if (lsOutCurveMode == 2)
                {
                    double outputX = tempX * tempX;
                    double outputY = tempY * tempY;
                    dState.LX = (byte)(outputX * signX * 127.5 + 127.5);
                    dState.LY = (byte)(outputY * signY * 127.5 + 127.5);
                }
                else if (lsOutCurveMode == 3)
                {
                    double outputX = tempX * tempX * tempX;
                    double outputY = tempY * tempY * tempY;
                    dState.LX = (byte)(outputX * 127.5 + 127.5);
                    dState.LY = (byte)(outputY * 127.5 + 127.5);
                }
            }

            int rsOutCurveMode = rsOutCurveModeArray[device] = getRsOutCurveMode(device);
            if (rsOutCurveMode > 0)
            {
                double tempX = (dState.RX - 127.5) / 127.5;
                double tempY = (dState.RY - 127.5) / 127.5;
                double signX = tempX >= 0.0 ? 1.0 : -1.0;
                double signY = tempY >= 0.0 ? 1.0 : -1.0;

                if (rsOutCurveMode == 1)
                {
                    double absX = Math.Abs(tempX);
                    double absY = Math.Abs(tempY);
                    double outputX = 0.0;
                    double outputY = 0.0;

                    if (absX <= 0.4)
                    {
                        outputX = 0.414 * absX;
                    }
                    else if (absX <= 0.75)
                    {
                        outputX = absX - 0.24;
                    }
                    else if (absX > 0.75)
                    {
                        outputX = (absX * 1.9376) - 0.9376;
                    }

                    if (absY <= 0.4)
                    {
                        outputY = 0.414 * absY;
                    }
                    else if (absY <= 0.75)
                    {
                        outputY = absY - 0.24;
                    }
                    else if (absY > 0.75)
                    {
                        outputY = (absY * 1.9376) - 0.9376;
                    }

                    dState.RX = (byte)(outputX * signX * 127.5 + 127.5);
                    dState.RY = (byte)(outputY * signY * 127.5 + 127.5);
                }
                else if (rsOutCurveMode == 2)
                {
                    double outputX = tempX * tempX;
                    double outputY = tempY * tempY;
                    dState.RX = (byte)(outputX * signX * 127.5 + 127.5);
                    dState.RY = (byte)(outputY * signY * 127.5 + 127.5);
                }
                else if (rsOutCurveMode == 3)
                {
                    double outputX = tempX * tempX * tempX;
                    double outputY = tempY * tempY * tempY;
                    dState.RX = (byte)(outputX * 127.5 + 127.5);
                    dState.RY = (byte)(outputY * 127.5 + 127.5);
                }
            }

            int l2OutCurveMode = tempIntArray[device] = getL2OutCurveMode(device);
            if (l2OutCurveMode > 0)
            {
                double temp = dState.L2 / 255.0;
                if (l2OutCurveMode == 1)
                {
                    double output = temp * temp;
                    dState.L2 = (byte)(output * 255.0);
                }
                else if (l2OutCurveMode == 2)
                {
                    double output = temp * temp * temp;
                    dState.L2 = (byte)(output * 255.0);
                }
            }

            int r2OutCurveMode = tempIntArray[device] = getR2OutCurveMode(device);
            if (r2OutCurveMode > 0)
            {
                double temp = dState.R2 / 255.0;
                if (r2OutCurveMode == 1)
                {
                    double output = temp * temp;
                    dState.R2 = (byte)(output * 255.0);
                }
                else if (r2OutCurveMode == 2)
                {
                    double output = temp * temp * temp;
                    dState.R2 = (byte)(output * 255.0);
                }
            }

            bool sOff = tempBool = isUsingSAforMouse(device);
            if (sOff == false)
            {
                int SXD = (int)(128d * getSXDeadzone(device));
                int SZD = (int)(128d * getSZDeadzone(device));
                double SXMax = getSXMaxzone(device);
                double SZMax = getSZMaxzone(device);
                double sxAntiDead = getSXAntiDeadzone(device);
                double szAntiDead = getSZAntiDeadzone(device);
                double sxsens = getSXSens(device);
                double szsens = getSZSens(device);
                int result = 0;

                int gyroX = cState.Motion.accelX, gyroZ = cState.Motion.accelZ;
                int absx = Math.Abs(gyroX), absz = Math.Abs(gyroZ);

                if (SXD > 0 || SXMax < 1.0 || sxAntiDead > 0)
                {
                    int maxValue = (int)(SXMax * 128d);
                    if (absx > SXD)
                    {
                        double ratioX = absx < maxValue ? (absx - SXD) / (double)(maxValue - SXD) : 1.0;
                        dState.Motion.outputAccelX = Math.Sign(gyroX) *
                            (int)Math.Min(128d, sxsens * 128d * ((1.0 - sxAntiDead) * ratioX + sxAntiDead));
                    }
                    else
                    {
                        dState.Motion.outputAccelX = 0;
                    }
                }
                else
                {
                    dState.Motion.outputAccelX = Math.Sign(gyroX) *
                        (int)Math.Min(128d, sxsens * 128d * (absx / 128d));
                }

                if (SZD > 0 || SZMax < 1.0 || szAntiDead > 0)
                {
                    int maxValue = (int)(SZMax * 128d);
                    if (absz > SZD)
                    {
                        double ratioZ = absz < maxValue ? (absz - SZD) / (double)(maxValue - SZD) : 1.0;
                        dState.Motion.outputAccelZ = Math.Sign(gyroZ) *
                            (int)Math.Min(128d, szsens * 128d * ((1.0 - szAntiDead) * ratioZ + szAntiDead));
                    }
                    else
                    {
                        dState.Motion.outputAccelZ = 0;
                    }
                }
                else
                {
                    dState.Motion.outputAccelZ = Math.Sign(gyroZ) *
                        (int)Math.Min(128d, szsens * 128d * (absz / 128d));
                }

                int sxOutCurveMode = tempIntArray[device] = getSXOutCurveMode(device);
                if (sxOutCurveMode > 0)
                {
                    double temp = dState.Motion.outputAccelX / 128.0;
                    double sign = Math.Sign(temp);
                    if (sxOutCurveMode == 1)
                    {
                        double output = temp * temp;
                        result = (int)(output * sign * 128.0);
                        dState.Motion.outputAccelX = result;
                    }
                    else if (sxOutCurveMode == 2)
                    {
                        double output = temp * temp * temp;
                        result = (int)(output * 128.0);
                        dState.Motion.outputAccelX = result;
                    }
                }

                int szOutCurveMode = tempIntArray[device] = getSZOutCurveMode(device);
                if (szOutCurveMode > 0)
                {
                    double temp = dState.Motion.outputAccelZ / 128.0;
                    double sign = Math.Sign(temp);
                    if (szOutCurveMode == 1)
                    {
                        double output = temp * temp;
                        result = (int)(output * sign * 128.0);
                        dState.Motion.outputAccelZ = result;
                    }
                    else if (szOutCurveMode == 2)
                    {
                        double output = temp * temp * temp;
                        result = (int)(output * 128.0);
                        dState.Motion.outputAccelZ = result;
                    }
                }
            }

            return dState;
        }

        /* TODO: Possibly remove usage of this version of the method */
        private static bool ShiftTrigger(int trigger, int device, DS4State cState, DS4StateExposed eState, Mouse tp)
        {
            bool result = false;
            if (trigger == 0)
            {
                result = false;
            }
            else
            {
                DS4Controls ds = shiftTriggerMapping[trigger];
                result = getBoolMapping(device, ds, cState, eState, tp);
            }

            return result;
        }

        private static bool ShiftTrigger2(int trigger, int device, DS4State cState, DS4StateExposed eState, Mouse tp, DS4StateFieldMapping fieldMapping)
        {
            bool result = false;
            if (trigger == 0)
            {
                result = false;
            }
            else if (trigger < 26)
            {
                DS4Controls ds = shiftTriggerMapping[trigger];
                result = getBoolMapping2(device, ds, cState, eState, tp, fieldMapping);
            }
            else if (trigger == 26)
            {
                result = cState.Touch1Finger;
            }

            return result;
        }

        private static X360Controls getX360ControlsByName(string key)
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
                default: break;
            }

            return X360Controls.Unbound;
        }

        /// <summary>
        /// Map DS4 Buttons/Axes to other DS4 Buttons/Axes (largely the same as Xinput ones) and to keyboard and mouse buttons.
        /// </summary>
        static bool[] held = new bool[4];
        static int[] oldmouse = new int[4] { -1, -1, -1, -1 };
        public static void MapCustom(int device, DS4State cState, DS4State MappedState, DS4StateExposed eState,
            Mouse tp, ControlService ctrl)
        {
            /* TODO: This method is slow sauce. Find ways to speed up action execution */
            MappedState.LX = 127;
            MappedState.LY = 127;
            MappedState.RX = 127;
            MappedState.RY = 127;
            double tempMouseDeltaX = 0.0;
            double tempMouseDeltaY = 0.0;
            int mouseDeltaX = 0;
            int mouseDeltaY = 0;

            cState.calculateStickAngles();
            DS4StateFieldMapping fieldMapping = fieldMappings[device];
            fieldMapping.populateFieldMapping(cState, eState, tp);
            DS4StateFieldMapping outputfieldMapping = outputFieldMappings[device];
            outputfieldMapping.populateFieldMapping(cState, eState, tp);
            //DS4StateFieldMapping fieldMapping = new DS4StateFieldMapping(cState, eState, tp);
            //DS4StateFieldMapping outputfieldMapping = new DS4StateFieldMapping(cState, eState, tp);

            SyntheticState deviceState = Mapping.deviceState[device];
            if (getProfileActionCount(device) > 0 || !string.IsNullOrEmpty(tempprofilename[device]))
                MapCustomAction(device, cState, MappedState, eState, tp, ctrl, fieldMapping, outputfieldMapping);
            if (ctrl.DS4Controllers[device] == null) return;

            cState.CopyTo(MappedState);

            Dictionary<DS4Controls, DS4Controls> tempControlDict = new Dictionary<DS4Controls, DS4Controls>();
            //MultiValueDict<DS4Controls, DS4Controls> tempControlDict = new MultiValueDict<DS4Controls, DS4Controls>();
            DS4Controls usingExtra = DS4Controls.None;
            List<DS4ControlSettings> tempSettingsList = getDS4CSettings(device);
            //foreach (DS4ControlSettings dcs in getDS4CSettings(device))
            for (int settingIndex = 0, arlen = tempSettingsList.Count; settingIndex < arlen; settingIndex++)
            {
                DS4ControlSettings dcs = tempSettingsList[settingIndex];
                object action = null;
                DS4ControlSettings.ActionType actionType = 0;
                DS4KeyType keyType = DS4KeyType.None;
                if (dcs.shiftAction != null && ShiftTrigger2(dcs.shiftTrigger, device, cState, eState, tp, fieldMapping))
                {
                    action = dcs.shiftAction;
                    actionType = dcs.shiftActionType;
                    keyType = dcs.shiftKeyType;
                }
                else if (dcs.action != null)
                {
                    action = dcs.action;
                    actionType = dcs.actionType;
                    keyType = dcs.keyType;
                }

                if (usingExtra == DS4Controls.None || usingExtra == dcs.control)
                {
                    bool shiftE = !string.IsNullOrEmpty(dcs.shiftExtras) && ShiftTrigger2(dcs.shiftTrigger, device, cState, eState, tp, fieldMapping);
                    bool regE = !string.IsNullOrEmpty(dcs.extras);
                    if ((regE || shiftE) && getBoolActionMapping2(device, dcs.control, cState, eState, tp, fieldMapping))
                    {
                        usingExtra = dcs.control;
                        string p;
                        if (shiftE)
                            p = dcs.shiftExtras;
                        else
                            p = dcs.extras;

                        string[] extraS = p.Split(',');
                        int extrasSLen = extraS.Length;
                        int[] extras = new int[extrasSLen];
                        for (int i = 0; i < extrasSLen; i++)
                        {
                            int b;
                            if (int.TryParse(extraS[i], out b))
                                extras[i] = b;
                        }

                        held[device] = true;
                        try
                        {
                            if (!(extras[0] == extras[1] && extras[1] == 0))
                                ctrl.setRumble((byte)extras[0], (byte)extras[1], device);

                            if (extras[2] == 1)
                            {
                                DS4Color color = new DS4Color { red = (byte)extras[3], green = (byte)extras[4], blue = (byte)extras[5] };
                                DS4LightBar.forcedColor[device] = color;
                                DS4LightBar.forcedFlash[device] = (byte)extras[6];
                                DS4LightBar.forcelight[device] = true;
                            }

                            if (extras[7] == 1)
                            {
                                if (oldmouse[device] == -1)
                                    oldmouse[device] = ButtonMouseSensitivity[device];
                                ButtonMouseSensitivity[device] = extras[8];
                            }
                        }
                        catch { }
                    }
                    else if ((regE || shiftE) && held[device])
                    {
                        DS4LightBar.forcelight[device] = false;
                        DS4LightBar.forcedFlash[device] = 0;
                        if (oldmouse[device] != -1)
                        {
                            ButtonMouseSensitivity[device] = oldmouse[device];
                            oldmouse[device] = -1;
                        }

                        ctrl.setRumble(0, 0, device);
                        held[device] = false;
                        usingExtra = DS4Controls.None;
                    }
                }

                if (action != null)
                {
                    if (actionType == DS4ControlSettings.ActionType.Macro)
                    {
                        bool active = getBoolMapping2(device, dcs.control, cState, eState, tp, fieldMapping);
                        if (active)
                        {
                            PlayMacro(device, macroControl, string.Join("/", (int[])action), dcs.control, keyType);
                        }
                        else
                        {
                            EndMacro(device, macroControl, string.Join("/", (int[])action), dcs.control);
                        }

                        // erase default mappings for things that are remapped
                        resetToDefaultValue2(dcs.control, MappedState, outputfieldMapping);
                    }
                    else if (actionType == DS4ControlSettings.ActionType.Key)
                    {
                        ushort value = Convert.ToUInt16(action);
                        if (getBoolActionMapping2(device, dcs.control, cState, eState, tp, fieldMapping))
                        {
                            SyntheticState.KeyPresses kp;
                            if (!deviceState.keyPresses.TryGetValue(value, out kp))
                                deviceState.keyPresses[value] = kp = new SyntheticState.KeyPresses();

                            if (keyType.HasFlag(DS4KeyType.ScanCode))
                                kp.current.scanCodeCount++;
                            else
                                kp.current.vkCount++;

                            if (keyType.HasFlag(DS4KeyType.Toggle))
                            {
                                if (!pressedonce[value])
                                {
                                    kp.current.toggle = !kp.current.toggle;
                                    pressedonce[value] = true;
                                }
                                kp.current.toggleCount++;
                            }
                            kp.current.repeatCount++;
                        }
                        else
                            pressedonce[value] = false;

                        // erase default mappings for things that are remapped
                        resetToDefaultValue2(dcs.control, MappedState, outputfieldMapping);
                    }
                    else if (actionType == DS4ControlSettings.ActionType.Button)
                    {
                        int keyvalue = 0;
                        bool isAnalog = false;

                        if (dcs.control >= DS4Controls.LXNeg && dcs.control <= DS4Controls.RYPos)
                        {
                            isAnalog = true;
                        }
                        else if (dcs.control == DS4Controls.L2 || dcs.control == DS4Controls.R2)
                        {
                            isAnalog = true;
                        }
                        else if (dcs.control >= DS4Controls.GyroXPos && dcs.control <= DS4Controls.GyroZNeg)
                        {
                            isAnalog = true;
                        }

                        X360Controls xboxControl = X360Controls.None;
                        if (action is X360Controls)
                        {
                            xboxControl = (X360Controls)action;
                        }
                        else if (action is string)
                        {
                            xboxControl = getX360ControlsByName(action.ToString());
                        }

                        if (xboxControl >= X360Controls.LXNeg && xboxControl <= X360Controls.Start)
                        {
                            DS4Controls tempDS4Control = reverseX360ButtonMapping[(int)xboxControl];
                            tempControlDict.Add(dcs.control, tempDS4Control);
                        }
                        else if (xboxControl >= X360Controls.LeftMouse && xboxControl <= X360Controls.WDOWN)
                        {
                            switch (xboxControl)
                            {
                                case X360Controls.LeftMouse:
                                {
                                    keyvalue = 256;
                                    if (getBoolActionMapping2(device, dcs.control, cState, eState, tp, fieldMapping))
                                        deviceState.currentClicks.leftCount++;

                                    break;
                                }
                                case X360Controls.RightMouse:
                                {
                                    keyvalue = 257;
                                    if (getBoolActionMapping2(device, dcs.control, cState, eState, tp, fieldMapping))
                                        deviceState.currentClicks.rightCount++;

                                    break;
                                }
                                case X360Controls.MiddleMouse:
                                {
                                    keyvalue = 258;
                                    if (getBoolActionMapping2(device, dcs.control, cState, eState, tp, fieldMapping))
                                        deviceState.currentClicks.middleCount++;

                                    break;
                                }
                                case X360Controls.FourthMouse:
                                {
                                    keyvalue = 259;
                                    if (getBoolActionMapping2(device, dcs.control, cState, eState, tp, fieldMapping))
                                        deviceState.currentClicks.fourthCount++;

                                    break;
                                }
                                case X360Controls.FifthMouse:
                                {
                                    keyvalue = 260;
                                    if (getBoolActionMapping2(device, dcs.control, cState, eState, tp, fieldMapping))
                                        deviceState.currentClicks.fifthCount++;

                                    break;
                                }
                                case X360Controls.WUP:
                                {
                                    if (getBoolActionMapping2(device, dcs.control, cState, eState, tp, fieldMapping))
                                    {
                                        if (isAnalog)
                                            getMouseWheelMapping(device, dcs.control, cState, eState, tp, false);
                                        else
                                            deviceState.currentClicks.wUpCount++;
                                    }

                                    break;
                                }
                                case X360Controls.WDOWN:
                                {
                                    if (getBoolActionMapping2(device, dcs.control, cState, eState, tp, fieldMapping))
                                    {
                                        if (isAnalog)
                                            getMouseWheelMapping(device, dcs.control, cState, eState, tp, true);
                                        else
                                            deviceState.currentClicks.wDownCount++;
                                    }

                                    break;
                                }

                                default: break;
                            }
                        }
                        else if (xboxControl >= X360Controls.MouseUp && xboxControl <= X360Controls.MouseRight)
                        {
                            switch (xboxControl)
                            {
                                case X360Controls.MouseUp:
                                {
                                    if (tempMouseDeltaY == 0)
                                    {
                                        tempMouseDeltaY = getMouseMapping(device, dcs.control, cState, eState, fieldMapping, 0, ctrl);
                                        tempMouseDeltaY = -Math.Abs((tempMouseDeltaY == -2147483648 ? 0 : tempMouseDeltaY));
                                    }

                                    break;
                                }
                                case X360Controls.MouseDown:
                                {
                                    if (tempMouseDeltaY == 0)
                                    {
                                        tempMouseDeltaY = getMouseMapping(device, dcs.control, cState, eState, fieldMapping, 1, ctrl);
                                        tempMouseDeltaY = Math.Abs((tempMouseDeltaY == -2147483648 ? 0 : tempMouseDeltaY));
                                    }

                                    break;
                                }
                                case X360Controls.MouseLeft:
                                {
                                    if (tempMouseDeltaX == 0)
                                    {
                                        tempMouseDeltaX = getMouseMapping(device, dcs.control, cState, eState, fieldMapping, 2, ctrl);
                                        tempMouseDeltaX = -Math.Abs((tempMouseDeltaX == -2147483648 ? 0 : tempMouseDeltaX));
                                    }

                                    break;
                                }
                                case X360Controls.MouseRight:
                                {
                                    if (tempMouseDeltaX == 0)
                                    {
                                        tempMouseDeltaX = getMouseMapping(device, dcs.control, cState, eState, fieldMapping, 3, ctrl);
                                        tempMouseDeltaX = Math.Abs((tempMouseDeltaX == -2147483648 ? 0 : tempMouseDeltaX));
                                    }

                                    break;
                                }

                                default: break;
                            }
                        }

                        if (keyType.HasFlag(DS4KeyType.Toggle))
                        {
                            if (getBoolActionMapping2(device, dcs.control, cState, eState, tp, fieldMapping))
                            {
                                resetToDefaultValue2(dcs.control, MappedState, outputfieldMapping);
                                if (!pressedonce[keyvalue])
                                {
                                    deviceState.currentClicks.toggle = !deviceState.currentClicks.toggle;
                                    pressedonce[keyvalue] = true;
                                }
                                deviceState.currentClicks.toggleCount++;
                            }
                            else
                            {
                                pressedonce[keyvalue] = false;
                            }
                        }

                        // erase default mappings for things that are remapped
                        resetToDefaultValue2(dcs.control, MappedState, outputfieldMapping);
                    }
                }
            }

            outputfieldMapping.populateState(MappedState);

            if (macroCount > 0)
            {
                if (macroControl[00]) MappedState.Cross = true;
                if (macroControl[01]) MappedState.Circle = true;
                if (macroControl[02]) MappedState.Square = true;
                if (macroControl[03]) MappedState.Triangle = true;
                if (macroControl[04]) MappedState.Options = true;
                if (macroControl[05]) MappedState.Share = true;
                if (macroControl[06]) MappedState.DpadUp = true;
                if (macroControl[07]) MappedState.DpadDown = true;
                if (macroControl[08]) MappedState.DpadLeft = true;
                if (macroControl[09]) MappedState.DpadRight = true;
                if (macroControl[10]) MappedState.PS = true;
                if (macroControl[11]) MappedState.L1 = true;
                if (macroControl[12]) MappedState.R1 = true;
                if (macroControl[13]) MappedState.L2 = 255;
                if (macroControl[14]) MappedState.R2 = 255;
                if (macroControl[15]) MappedState.L3 = true;
                if (macroControl[16]) MappedState.R3 = true;
                if (macroControl[17]) MappedState.LX = 255;
                if (macroControl[18]) MappedState.LX = 0;
                if (macroControl[19]) MappedState.LY = 255;
                if (macroControl[20]) MappedState.LY = 0;
                if (macroControl[21]) MappedState.RX = 255;
                if (macroControl[22]) MappedState.RX = 0;
                if (macroControl[23]) MappedState.RY = 255;
                if (macroControl[24]) MappedState.RY = 0;
            }

            if (IfAxisIsNotModified(device, ShiftTrigger2(GetDS4STrigger(device, DS4Controls.LXNeg), device, cState, eState, tp, fieldMapping), DS4Controls.LXNeg))
                tempControlDict[DS4Controls.LXNeg] = DS4Controls.LXNeg;

            if (IfAxisIsNotModified(device, ShiftTrigger2(GetDS4STrigger(device, DS4Controls.LXPos), device, cState, eState, tp, fieldMapping), DS4Controls.LXPos))
                tempControlDict[DS4Controls.LXPos] = DS4Controls.LXPos;

            if (IfAxisIsNotModified(device, ShiftTrigger2(GetDS4STrigger(device, DS4Controls.LYNeg), device, cState, eState, tp, fieldMapping), DS4Controls.LYNeg))
                tempControlDict[DS4Controls.LYNeg] = DS4Controls.LYNeg;

            if (IfAxisIsNotModified(device, ShiftTrigger2(GetDS4STrigger(device, DS4Controls.LYPos), device, cState, eState, tp, fieldMapping), DS4Controls.LYPos))
                tempControlDict[DS4Controls.LYPos] = DS4Controls.LYPos;

            if (IfAxisIsNotModified(device, ShiftTrigger2(GetDS4STrigger(device, DS4Controls.RXNeg), device, cState, eState, tp, fieldMapping), DS4Controls.RXNeg))
                tempControlDict[DS4Controls.RXNeg] = DS4Controls.RXNeg;

            if (IfAxisIsNotModified(device, ShiftTrigger2(GetDS4STrigger(device, DS4Controls.RXPos), device, cState, eState, tp, fieldMapping), DS4Controls.RXPos))
                tempControlDict[DS4Controls.RXPos] = DS4Controls.RXPos;

            if (IfAxisIsNotModified(device, ShiftTrigger2(GetDS4STrigger(device, DS4Controls.RYNeg), device, cState, eState, tp, fieldMapping), DS4Controls.RYNeg))
                tempControlDict[DS4Controls.RYNeg] = DS4Controls.RYNeg;

            if (IfAxisIsNotModified(device, ShiftTrigger2(GetDS4STrigger(device, DS4Controls.RYPos), device, cState, eState, tp, fieldMapping), DS4Controls.RYPos))
                tempControlDict[DS4Controls.RYPos] = DS4Controls.RYPos;

            Dictionary<DS4Controls, DS4Controls>.KeyCollection controlKeys = tempControlDict.Keys;
            //Dictionary<DS4Controls, List<DS4Controls>>.KeyCollection controlKeys = tempControlDict.Keys;

            //foreach (KeyValuePair<DS4Controls, DS4Controls> entry in tempControlDict)
            for (int i = 0, keyCount = controlKeys.Count; i < keyCount; i++)
            {
                DS4Controls key = controlKeys.ElementAt(i);
                DS4Controls dc = tempControlDict[key];
                //DS4Controls key = entry.Key;
                //DS4Controls dc = entry.Value;

                if (getBoolActionMapping2(device, key, cState, eState, tp, fieldMapping, true))
                {
                    if (dc >= DS4Controls.Square && dc <= DS4Controls.Cross)
                    {
                        switch (dc)
                        {
                            case DS4Controls.Cross: MappedState.Cross = true; break;
                            case DS4Controls.Circle: MappedState.Circle = true; break;
                            case DS4Controls.Square: MappedState.Square = true; break;
                            case DS4Controls.Triangle: MappedState.Triangle = true; break;
                            default: break;
                        }
                    }
                    else if (dc >= DS4Controls.L1 && dc <= DS4Controls.R3)
                    {
                        switch (dc)
                        {
                            case DS4Controls.L1: MappedState.L1 = true; break;
                            case DS4Controls.L2: MappedState.L2 = getByteMapping2(device, key, cState, eState, tp, fieldMapping); break;
                            case DS4Controls.L3: MappedState.L3 = true; break;
                            case DS4Controls.R1: MappedState.R1 = true; break;
                            case DS4Controls.R2: MappedState.R2 = getByteMapping2(device, key, cState, eState, tp, fieldMapping); break;
                            case DS4Controls.R3: MappedState.R3 = true; break;
                            default: break;
                        }
                    }
                    else if (dc >= DS4Controls.DpadUp && dc <= DS4Controls.DpadLeft)
                    {
                        switch (dc)
                        {
                            case DS4Controls.DpadUp: MappedState.DpadUp = true; break;
                            case DS4Controls.DpadRight: MappedState.DpadRight = true; break;
                            case DS4Controls.DpadLeft: MappedState.DpadLeft = true; break;
                            case DS4Controls.DpadDown: MappedState.DpadDown = true; break;
                            default: break;
                        }
                    }
                    else if (dc >= DS4Controls.LXNeg && dc <= DS4Controls.RYPos)
                    {
                        switch (dc)
                        {
                            case DS4Controls.LXNeg:
                            case DS4Controls.LXPos:
                            {
                                if (MappedState.LX == 127)
                                {
                                    if (dc == DS4Controls.LXNeg)
                                    {
                                        byte axisMapping = getXYAxisMapping2(device, key, cState, eState, tp, fieldMapping);
                                        MappedState.LX = axisMapping;
                                    }
                                    else
                                    {
                                        byte axisMapping = getXYAxisMapping2(device, key, cState, eState, tp, fieldMapping, true);
                                        MappedState.LX = axisMapping;
                                    }
                                }

                                break;
                            }
                            case DS4Controls.LYNeg:
                            case DS4Controls.LYPos:
                            {
                                if (MappedState.LY == 127)
                                {
                                    if (dc == DS4Controls.LYNeg)
                                    {
                                        byte axisMapping = getXYAxisMapping2(device, key, cState, eState, tp, fieldMapping);
                                        MappedState.LY = axisMapping;
                                    }
                                    else
                                    {
                                        byte axisMapping = getXYAxisMapping2(device, key, cState, eState, tp, fieldMapping, true);
                                        MappedState.LY = axisMapping;
                                    }
                                }

                                break;
                            }
                            case DS4Controls.RXNeg:
                            case DS4Controls.RXPos:
                            {
                                if (MappedState.RX == 127)
                                {
                                    if (dc == DS4Controls.RXNeg)
                                    {
                                        byte axisMapping = getXYAxisMapping2(device, key, cState, eState, tp, fieldMapping);
                                        MappedState.RX = axisMapping;
                                    }
                                    else
                                    {
                                        byte axisMapping = getXYAxisMapping2(device, key, cState, eState, tp, fieldMapping, true);
                                        MappedState.RX = axisMapping;
                                    }
                                }

                                break;
                            }
                            case DS4Controls.RYNeg:
                            case DS4Controls.RYPos:
                            {
                                if (MappedState.RY == 127)
                                {
                                    if (dc == DS4Controls.RYNeg)
                                    {
                                        byte axisMapping = getXYAxisMapping2(device, key, cState, eState, tp, fieldMapping);
                                        MappedState.RY = axisMapping;
                                    }
                                    else
                                    {
                                        byte axisMapping = getXYAxisMapping2(device, key, cState, eState, tp, fieldMapping, true);
                                        MappedState.RY = axisMapping;
                                    }
                                }

                                break;
                            }
                            default: break;
                        }
                    }
                    else
                    {
                        switch (dc)
                        {
                            case DS4Controls.Options: MappedState.Options = true; break;
                            case DS4Controls.Share: MappedState.Share = true; break;
                            case DS4Controls.PS: MappedState.PS = true; break;
                            default: break;
                        }
                    }
                }
            }

            calculateFinalMouseMovement(ref tempMouseDeltaX, ref tempMouseDeltaY,
                out mouseDeltaX, out mouseDeltaY);
            if (mouseDeltaX != 0 || mouseDeltaY != 0)
            {
                InputMethods.MoveCursorBy(mouseDeltaX, mouseDeltaY);
            }
        }

        private static bool IfAxisIsNotModified(int device, bool shift, DS4Controls dc)
        {
            return shift ? false : GetDS4Action(device, dc, false) == null;
        }

        private static async void MapCustomAction(int device, DS4State cState, DS4State MappedState,
            DS4StateExposed eState, Mouse tp, ControlService ctrl, DS4StateFieldMapping fieldMapping, DS4StateFieldMapping outputfieldMapping)
        {
            /* TODO: This method is slow sauce. Find ways to speed up action execution */
            try
            {
                int actionDoneCount = actionDone.Count;
                int totalActionCount = GetActions().Count;
                DS4StateFieldMapping previousFieldMapping = null;
                List<string> profileActions = getProfileActions(device);
                //foreach (string actionname in profileActions)
                for (int actionIndex = 0, profileListLen = profileActions.Count;
                     actionIndex < profileListLen; actionIndex++)
                {
                    //DS4KeyType keyType = getShiftCustomKeyType(device, customKey.Key);
                    //SpecialAction action = GetAction(actionname);
                    //int index = GetActionIndexOf(actionname);
                    string actionname = profileActions[actionIndex];
                    SpecialAction action = GetProfileAction(device, actionname);
                    int index = GetProfileActionIndexOf(device, actionname);

                    if (actionDoneCount < index + 1)
                    {
                        actionDone.Add(new ActionState());
                        actionDoneCount++;
                    }
                    else if (actionDoneCount > totalActionCount)
                    {
                        actionDone.RemoveAt(actionDoneCount - 1);
                        actionDoneCount--;
                    }

                    double time = 0.0;
                    //If a key or button is assigned to the trigger, a key special action is used like
                    //a quick tap to use and hold to use the regular custom button/key
                    bool triggerToBeTapped = action.typeID == SpecialAction.ActionTypeId.None && action.trigger.Count == 1 &&
                            GetDS4Action(device, action.trigger[0], false) == null;
                    if (!(action.typeID == SpecialAction.ActionTypeId.None || index < 0))
                    {
                        bool triggeractivated = true;
                        if (action.delayTime > 0.0)
                        {
                            triggeractivated = false;
                            bool subtriggeractivated = true;
                            //foreach (DS4Controls dc in action.trigger)
                            for (int i = 0, arlen = action.trigger.Count; i < arlen; i++)
                            {
                                DS4Controls dc = action.trigger[i];
                                if (!getBoolMapping2(device, dc, cState, eState, tp, fieldMapping))
                                {
                                    subtriggeractivated = false;
                                    break;
                                }
                            }
                            if (subtriggeractivated)
                            {
                                time = action.delayTime;
                                nowAction[device] = DateTime.UtcNow;
                                if (nowAction[device] >= oldnowAction[device] + TimeSpan.FromSeconds(time))
                                    triggeractivated = true;
                            }
                            else if (nowAction[device] < DateTime.UtcNow - TimeSpan.FromMilliseconds(100))
                                oldnowAction[device] = DateTime.UtcNow;
                        }
                        else if (triggerToBeTapped && oldnowKeyAct[device] == DateTime.MinValue)
                        {
                            triggeractivated = false;
                            bool subtriggeractivated = true;
                            //foreach (DS4Controls dc in action.trigger)
                            for (int i = 0, arlen = action.trigger.Count; i < arlen; i++)
                            {
                                DS4Controls dc = action.trigger[i];
                                if (!getBoolMapping2(device, dc, cState, eState, tp, fieldMapping))
                                {
                                    subtriggeractivated = false;
                                    break;
                                }
                            }
                            if (subtriggeractivated)
                            {
                                oldnowKeyAct[device] = DateTime.UtcNow;
                            }
                        }
                        else if (triggerToBeTapped && oldnowKeyAct[device] != DateTime.MinValue)
                        {
                            triggeractivated = false;
                            bool subtriggeractivated = true;
                            //foreach (DS4Controls dc in action.trigger)
                            for (int i = 0, arlen = action.trigger.Count; i < arlen; i++)
                            {
                                DS4Controls dc = action.trigger[i];
                                if (!getBoolMapping2(device, dc, cState, eState, tp, fieldMapping))
                                {
                                    subtriggeractivated = false;
                                    break;
                                }
                            }
                            DateTime now = DateTime.UtcNow;
                            if (!subtriggeractivated && now <= oldnowKeyAct[device] + TimeSpan.FromMilliseconds(250))
                            {
                                await Task.Delay(3); //if the button is assigned to the same key use a delay so the key down is the last action, not key up
                                triggeractivated = true;
                                oldnowKeyAct[device] = DateTime.MinValue;
                            }
                            else if (!subtriggeractivated)
                                oldnowKeyAct[device] = DateTime.MinValue;
                        }
                        else
                        {
                            //foreach (DS4Controls dc in action.trigger)
                            for (int i = 0, arlen = action.trigger.Count; i < arlen; i++)
                            {
                                DS4Controls dc = action.trigger[i];
                                if (!getBoolMapping2(device, dc, cState, eState, tp, fieldMapping))
                                {
                                    triggeractivated = false;
                                    break;
                                }
                            }
                        }

                        bool utriggeractivated = true;
                        int uTriggerCount = action.uTrigger.Count;
                        if (action.typeID == SpecialAction.ActionTypeId.Key && uTriggerCount > 0)
                        {
                            //foreach (DS4Controls dc in action.uTrigger)
                            for (int i = 0, arlen = action.uTrigger.Count; i < arlen; i++)
                            {
                                DS4Controls dc = action.uTrigger[i];
                                if (!getBoolMapping2(device, dc, cState, eState, tp, fieldMapping))
                                {
                                    utriggeractivated = false;
                                    break;
                                }
                            }
                            if (action.pressRelease) utriggeractivated = !utriggeractivated;
                        }

                        bool actionFound = false;
                        if (triggeractivated)
                        {
                            if (action.typeID == SpecialAction.ActionTypeId.Program)
                            {
                                actionFound = true;

                                if (!actionDone[index].dev[device])
                                {
                                    actionDone[index].dev[device] = true;
                                    if (!string.IsNullOrEmpty(action.extra))
                                        Process.Start(action.details, action.extra);
                                    else
                                        Process.Start(action.details);
                                }
                            }
                            else if (action.typeID == SpecialAction.ActionTypeId.Profile)
                            {
                                actionFound = true;

                                if (!actionDone[index].dev[device] && string.IsNullOrEmpty(tempprofilename[device]))
                                {
                                    actionDone[index].dev[device] = true;
                                    untriggeraction[device] = action;
                                    untriggerindex[device] = index;
                                    //foreach (DS4Controls dc in action.trigger)
                                    for (int i = 0, arlen = action.trigger.Count; i < arlen; i++)
                                    {
                                        DS4Controls dc = action.trigger[i];
                                        DS4ControlSettings dcs = getDS4CSetting(device, dc);
                                        if (dcs.action != null)
                                        {
                                            if (dcs.actionType == DS4ControlSettings.ActionType.Key)
                                                InputMethods.performKeyRelease(ushort.Parse(dcs.action.ToString()));
                                            else if (dcs.actionType == DS4ControlSettings.ActionType.Macro)
                                            {
                                                int[] keys = (int[])dcs.action;
                                                for (int j = 0, keysLen = keys.Length; j < keysLen; j++)
                                                    InputMethods.performKeyRelease((ushort)keys[j]);
                                            }
                                        }
                                    }
                                    LoadTempProfile(device, action.details, true, ctrl);
                                    return;
                                }
                            }
                            else if (action.typeID == SpecialAction.ActionTypeId.Macro)
                            {
                                actionFound = true;

                                if (!actionDone[index].dev[device])
                                {
                                    DS4KeyType keyType = action.keyType;
                                    actionDone[index].dev[device] = true;
                                    //foreach (DS4Controls dc in action.trigger)
                                    for (int i = 0, arlen = action.trigger.Count; i < arlen; i++)
                                    {
                                        DS4Controls dc = action.trigger[i];
                                        resetToDefaultValue2(dc, MappedState, outputfieldMapping);
                                    }

                                    PlayMacro(device, macroControl, String.Join("/", action.macro), DS4Controls.None, keyType);
                                }
                                else
                                    EndMacro(device, macroControl, String.Join("/", action.macro), DS4Controls.None);
                            }
                            else if (action.typeID == SpecialAction.ActionTypeId.Key)
                            {
                                actionFound = true;

                                if (uTriggerCount == 0 || (uTriggerCount > 0 && untriggerindex[device] == -1 && !actionDone[index].dev[device]))
                                {
                                    actionDone[index].dev[device] = true;
                                    untriggerindex[device] = index;
                                    ushort key;
                                    ushort.TryParse(action.details, out key);
                                    if (uTriggerCount == 0)
                                    {
                                        SyntheticState.KeyPresses kp;
                                        if (!deviceState[device].keyPresses.TryGetValue(key, out kp))
                                            deviceState[device].keyPresses[key] = kp = new SyntheticState.KeyPresses();
                                        if (action.keyType.HasFlag(DS4KeyType.ScanCode))
                                            kp.current.scanCodeCount++;
                                        else
                                            kp.current.vkCount++;
                                        kp.current.repeatCount++;
                                    }
                                    else if (action.keyType.HasFlag(DS4KeyType.ScanCode))
                                        InputMethods.performSCKeyPress(key);
                                    else
                                        InputMethods.performKeyPress(key);
                                }
                            }
                            else if (action.typeID == SpecialAction.ActionTypeId.DisconnectBT)
                            {
                                actionFound = true;

                                DS4Device d = ctrl.DS4Controllers[device];
                                bool synced = tempBool = d.isSynced();
                                if (synced && !d.isCharging())
                                {
                                    ConnectionType deviceConn = d.getConnectionType();
                                    bool exclusive = tempBool = d.isExclusive();
                                    if (deviceConn == ConnectionType.BT)
                                    {
                                        d.DisconnectBT();
                                    }
                                    else if (deviceConn == ConnectionType.SONYWA && exclusive)
                                    {
                                        d.DisconnectDongle();
                                    }

                                    //foreach (DS4Controls dc in action.trigger)
                                    for (int i = 0, arlen = action.trigger.Count; i < arlen; i++)
                                    {
                                        DS4Controls dc = action.trigger[i];
                                        DS4ControlSettings dcs = getDS4CSetting(device, dc);
                                        if (dcs.action != null)
                                        {
                                            if (dcs.actionType == DS4ControlSettings.ActionType.Key)
                                                InputMethods.performKeyRelease((ushort)dcs.action);
                                            else if (dcs.actionType == DS4ControlSettings.ActionType.Macro)
                                            {
                                                int[] keys = (int[])dcs.action;
                                                for (int j = 0, keysLen = keys.Length; j < keysLen; j++)
                                                    InputMethods.performKeyRelease((ushort)keys[j]);
                                            }
                                        }
                                    }
                                    return;
                                }
                            }
                            else if (action.typeID == SpecialAction.ActionTypeId.BatteryCheck)
                            {
                                actionFound = true;

                                string[] dets = action.details.Split('|');
                                if (dets.Length == 1)
                                    dets = action.details.Split(',');
                                if (bool.Parse(dets[1]) && !actionDone[index].dev[device])
                                {
                                    Log.LogToTray("Controller " + (device + 1) + ": " +
                                        ctrl.getDS4Battery(device), true);
                                }
                                if (bool.Parse(dets[2]))
                                {
                                    DS4Device d = ctrl.DS4Controllers[device];
                                    if (!actionDone[index].dev[device])
                                    {
                                        lastColor[device] = d.LightBarColor;
                                        DS4LightBar.forcelight[device] = true;
                                    }
                                    DS4Color empty = new DS4Color(byte.Parse(dets[3]), byte.Parse(dets[4]), byte.Parse(dets[5]));
                                    DS4Color full = new DS4Color(byte.Parse(dets[6]), byte.Parse(dets[7]), byte.Parse(dets[8]));
                                    DS4Color trans = getTransitionedColor(empty, full, d.Battery);
                                    if (fadetimer[device] < 100)
                                        DS4LightBar.forcedColor[device] = getTransitionedColor(lastColor[device], trans, fadetimer[device] += 2);
                                }
                                actionDone[index].dev[device] = true;
                            }
                        }
                        else
                        {
                            if (action.typeID == SpecialAction.ActionTypeId.BatteryCheck)
                            {
                                actionFound = true;
                                if (actionDone[index].dev[device])
                                {
                                    fadetimer[device] = 0;
                                    /*if (prevFadetimer[device] == fadetimer[device])
                                    {
                                        prevFadetimer[device] = 0;
                                        fadetimer[device] = 0;
                                    }
                                    else
                                        prevFadetimer[device] = fadetimer[device];*/
                                    DS4LightBar.forcelight[device] = false;
                                    actionDone[index].dev[device] = false;
                                }
                            }
                            else if (action.typeID != SpecialAction.ActionTypeId.Key &&
                                     action.typeID != SpecialAction.ActionTypeId.XboxGameDVR &&
                                     action.typeID != SpecialAction.ActionTypeId.MultiAction)
                            {
                                // Ignore
                                actionFound = true;
                                actionDone[index].dev[device] = false;
                            }
                        }

                        if (!actionFound)
                        {
                            if (uTriggerCount > 0 && utriggeractivated && action.typeID == SpecialAction.ActionTypeId.Key)
                            {
                                actionFound = true;

                                if (untriggerindex[device] > -1 && !actionDone[index].dev[device])
                                {
                                    actionDone[index].dev[device] = true;
                                    untriggerindex[device] = -1;
                                    ushort key;
                                    ushort.TryParse(action.details, out key);
                                    if (action.keyType.HasFlag(DS4KeyType.ScanCode))
                                        InputMethods.performSCKeyRelease(key);
                                    else
                                        InputMethods.performKeyRelease(key);
                                }
                            }
                            else if (action.typeID == SpecialAction.ActionTypeId.XboxGameDVR || action.typeID == SpecialAction.ActionTypeId.MultiAction)
                            {
                                actionFound = true;

                                bool tappedOnce = action.tappedOnce, firstTouch = action.firstTouch,
                                    secondtouchbegin = action.secondtouchbegin;
                                //DateTime pastTime = action.pastTime, firstTap = action.firstTap,
                                //    TimeofEnd = action.TimeofEnd;

                                /*if (getCustomButton(device, action.trigger[0]) != X360Controls.Unbound)
                                    getCustomButtons(device)[action.trigger[0]] = X360Controls.Unbound;
                                if (getCustomMacro(device, action.trigger[0]) != "0")
                                    getCustomMacros(device).Remove(action.trigger[0]);
                                if (getCustomKey(device, action.trigger[0]) != 0)
                                    getCustomMacros(device).Remove(action.trigger[0]);*/
                                string[] dets = action.details.Split(',');
                                DS4Device d = ctrl.DS4Controllers[device];
                                //cus

                                DS4State tempPrevState = d.getPreviousStateRef();
                                // Only create one instance of previous DS4StateFieldMapping in case more than one multi-action
                                // button is assigned
                                if (previousFieldMapping == null)
                                {
                                    previousFieldMapping = previousFieldMappings[device];
                                    previousFieldMapping.populateFieldMapping(tempPrevState, eState, tp, true);
                                    //previousFieldMapping = new DS4StateFieldMapping(tempPrevState, eState, tp, true);
                                }

                                bool activeCur = getBoolMapping2(device, action.trigger[0], cState, eState, tp, fieldMapping);
                                bool activePrev = getBoolMapping2(device, action.trigger[0], tempPrevState, eState, tp, previousFieldMapping);
                                if (activeCur && !activePrev)
                                {
                                    // pressed down
                                    action.pastTime = DateTime.UtcNow;
                                    if (action.pastTime <= (action.firstTap + TimeSpan.FromMilliseconds(100)))
                                    {
                                        action.tappedOnce = tappedOnce = false;
                                        action.secondtouchbegin = secondtouchbegin = true;
                                        //tappedOnce = false;
                                        //secondtouchbegin = true;
                                    }
                                    else
                                        action.firstTouch = firstTouch = true;
                                        //firstTouch = true;
                                }
                                else if (!activeCur && activePrev)
                                {
                                    // released
                                    if (secondtouchbegin)
                                    {
                                        action.firstTouch = firstTouch = false;
                                        action.secondtouchbegin = secondtouchbegin = false;
                                        //firstTouch = false;
                                        //secondtouchbegin = false;
                                    }
                                    else if (firstTouch)
                                    {
                                        action.firstTouch = firstTouch = false;
                                        //firstTouch = false;
                                        if (DateTime.UtcNow <= (action.pastTime + TimeSpan.FromMilliseconds(150)) && !tappedOnce)
                                        {
                                            action.tappedOnce = tappedOnce = true;
                                            //tappedOnce = true;
                                            action.firstTap = DateTime.UtcNow;
                                            action.TimeofEnd = DateTime.UtcNow;
                                        }
                                    }
                                }

                                int type = 0;
                                string macro = "";
                                if (tappedOnce) //single tap
                                {
                                    if (action.typeID == SpecialAction.ActionTypeId.MultiAction)
                                    {
                                        macro = dets[0];
                                    }
                                    else if (int.TryParse(dets[0], out type))
                                    {
                                        switch (type)
                                        {
                                            case 0: macro = "91/71/71/91"; break;
                                            case 1: macro = "91/164/82/82/164/91"; break;
                                            case 2: macro = "91/164/44/44/164/91"; break;
                                            case 3: macro = dets[3] + "/" + dets[3]; break;
                                            case 4: macro = "91/164/71/71/164/91"; break;
                                        }
                                    }

                                    if ((DateTime.UtcNow - action.TimeofEnd) > TimeSpan.FromMilliseconds(150))
                                    {
                                        if (macro != "")
                                            PlayMacro(device, macroControl, macro, DS4Controls.None, DS4KeyType.None);

                                        tappedOnce = false;
                                        action.tappedOnce = false;
                                    }
                                    //if it fails the method resets, and tries again with a new tester value (gives tap a delay so tap and hold can work)
                                }
                                else if (firstTouch && (DateTime.UtcNow - action.pastTime) > TimeSpan.FromMilliseconds(500)) //helddown
                                {
                                    if (action.typeID == SpecialAction.ActionTypeId.MultiAction)
                                    {
                                        macro = dets[1];
                                    }
                                    else if (int.TryParse(dets[1], out type))
                                    {
                                        switch (type)
                                        {
                                            case 0: macro = "91/71/71/91"; break;
                                            case 1: macro = "91/164/82/82/164/91"; break;
                                            case 2: macro = "91/164/44/44/164/91"; break;
                                            case 3: macro = dets[3] + "/" + dets[3]; break;
                                            case 4: macro = "91/164/71/71/164/91"; break;
                                        }
                                    }

                                    if (macro != "")
                                        PlayMacro(device, macroControl, macro, DS4Controls.None, DS4KeyType.None);

                                    firstTouch = false;
                                    action.firstTouch = false;
                                }
                                else if (secondtouchbegin) //if double tap
                                {
                                    if (action.typeID == SpecialAction.ActionTypeId.MultiAction)
                                    {
                                        macro = dets[2];
                                    }
                                    else if (int.TryParse(dets[2], out type))
                                    {
                                        switch (type)
                                        {
                                            case 0: macro = "91/71/71/91"; break;
                                            case 1: macro = "91/164/82/82/164/91"; break;
                                            case 2: macro = "91/164/44/44/164/91"; break;
                                            case 3: macro = dets[3] + "/" + dets[3]; break;
                                            case 4: macro = "91/164/71/71/164/91"; break;
                                        }
                                    }

                                    if (macro != "")
                                        PlayMacro(device, macroControl, macro, DS4Controls.None, DS4KeyType.None);

                                    secondtouchbegin = false;
                                    action.secondtouchbegin = false;
                                }
                            }
                            else
                            {
                                actionDone[index].dev[device] = false;
                            }
                        }
                    }
                }
            }
            catch { return; }

            if (untriggeraction[device] != null)
            {
                SpecialAction action = untriggeraction[device];
                int index = untriggerindex[device];
                bool utriggeractivated = true;
                //foreach (DS4Controls dc in action.uTrigger)
                for (int i = 0, uTrigLen = action.uTrigger.Count; i < uTrigLen; i++)
                {
                    DS4Controls dc = action.uTrigger[i];
                    if (!getBoolMapping2(device, dc, cState, eState, tp, fieldMapping))
                    {
                        utriggeractivated = false;
                        break;
                    }
                }

                if (utriggeractivated && action.typeID == SpecialAction.ActionTypeId.Profile)
                {
                    if ((action.controls == action.ucontrols && !actionDone[index].dev[device]) || //if trigger and end trigger are the same
                    action.controls != action.ucontrols)
                    {
                        if (!string.IsNullOrEmpty(tempprofilename[device]))
                        {
                            //foreach (DS4Controls dc in action.uTrigger)
                            for (int i = 0, arlen = action.uTrigger.Count; i < arlen; i++)
                            {
                                DS4Controls dc = action.uTrigger[i];
                                actionDone[index].dev[device] = true;
                                DS4ControlSettings dcs = getDS4CSetting(device, dc);
                                if (dcs.action != null)
                                {
                                    if (dcs.actionType == DS4ControlSettings.ActionType.Key)
                                        InputMethods.performKeyRelease((ushort)dcs.action);
                                    else if (dcs.actionType == DS4ControlSettings.ActionType.Macro)
                                    {
                                        int[] keys = (int[])dcs.action;
                                        for (int j = 0, keysLen = keys.Length; j < keysLen; j++)
                                            InputMethods.performKeyRelease((ushort)keys[j]);
                                    }
                                }
                            }

                            untriggeraction[device] = null;
                            LoadProfile(device, false, ctrl);
                        }
                    }
                }
                else
                {
                    actionDone[index].dev[device] = false;
                }
            }
        }

        private static async void PlayMacro(int device, bool[] macrocontrol, string macro, DS4Controls control, DS4KeyType keyType)
        {
            if (macro.StartsWith("164/9/9/164") || macro.StartsWith("18/9/9/18"))
            {
                string[] skeys;
                int wait = 1000;
                if (!string.IsNullOrEmpty(macro))
                {
                    skeys = macro.Split('/');
                    ushort delay;
                    if (ushort.TryParse(skeys[skeys.Length - 1], out delay) && delay > 300)
                        wait = delay - 300;
                }
                AltTabSwapping(wait, device);
                if (control != DS4Controls.None)
                    macrodone[DS4ControltoInt(control)] = true;
            }
            else
            {
                string[] skeys;
                int[] keys;
                if (!string.IsNullOrEmpty(macro))
                {
                    skeys = macro.Split('/');
                    keys = new int[skeys.Length];
                }
                else
                {
                    skeys = new string[0];
                    keys = new int[0];
                }
                for (int i = 0; i < keys.Length; i++)
                    keys[i] = int.Parse(skeys[i]);
                bool[] keydown = new bool[286];
                if (control == DS4Controls.None || !macrodone[DS4ControltoInt(control)])
                {
                    if (control != DS4Controls.None)
                        macrodone[DS4ControltoInt(control)] = true;
                    foreach (int i in keys)
                    {
                        if (i >= 1000000000)
                        {
                            string lb = i.ToString().Substring(1);
                            if (i > 1000000000)
                            {
                                byte r = (byte)(int.Parse(lb[0].ToString()) * 100 + int.Parse(lb[1].ToString()) * 10 + int.Parse(lb[2].ToString()));
                                byte g = (byte)(int.Parse(lb[3].ToString()) * 100 + int.Parse(lb[4].ToString()) * 10 + int.Parse(lb[5].ToString()));
                                byte b = (byte)(int.Parse(lb[6].ToString()) * 100 + int.Parse(lb[7].ToString()) * 10 + int.Parse(lb[8].ToString()));
                                DS4LightBar.forcelight[device] = true;
                                DS4LightBar.forcedFlash[device] = 0;
                                DS4LightBar.forcedColor[device] = new DS4Color(r, g, b);
                            }
                            else
                            {
                                DS4LightBar.forcedFlash[device] = 0;
                                DS4LightBar.forcelight[device] = false;
                            }
                        }
                        else if (i >= 1000000)
                        {
                            DS4Device d = Program.rootHub.DS4Controllers[device];
                            string r = i.ToString().Substring(1);
                            byte heavy = (byte)(int.Parse(r[0].ToString()) * 100 + int.Parse(r[1].ToString()) * 10 + int.Parse(r[2].ToString()));
                            byte light = (byte)(int.Parse(r[3].ToString()) * 100 + int.Parse(r[4].ToString()) * 10 + int.Parse(r[5].ToString()));
                            d.setRumble(light, heavy);
                        }
                        else if (i >= 300) //ints over 300 used to delay
                            await Task.Delay(i - 300);
                        else if (!keydown[i])
                        {
                            if (i == 256) InputMethods.MouseEvent(InputMethods.MOUSEEVENTF_LEFTDOWN); //anything above 255 is not a keyvalue
                            else if (i == 257) InputMethods.MouseEvent(InputMethods.MOUSEEVENTF_RIGHTDOWN);
                            else if (i == 258) InputMethods.MouseEvent(InputMethods.MOUSEEVENTF_MIDDLEDOWN);
                            else if (i == 259) InputMethods.MouseEvent(InputMethods.MOUSEEVENTF_XBUTTONDOWN, 1);
                            else if (i == 260) InputMethods.MouseEvent(InputMethods.MOUSEEVENTF_XBUTTONDOWN, 2);
                            else if (i == 261) { macroControl[0] = true; macroCount++; }
                            else if (i == 262) { macroControl[1] = true; macroCount++; }
                            else if (i == 263) { macroControl[2] = true; macroCount++; }
                            else if (i == 264) { macroControl[3] = true; macroCount++; }
                            else if (i == 265) { macroControl[4] = true; macroCount++; }
                            else if (i == 266) { macroControl[5] = true; macroCount++; }
                            else if (i == 267) { macroControl[6] = true; macroCount++; }
                            else if (i == 268) { macroControl[7] = true; macroCount++; }
                            else if (i == 269) { macroControl[8] = true; macroCount++; }
                            else if (i == 270) { macroControl[9] = true; macroCount++; }
                            else if (i == 271) { macroControl[10] = true; macroCount++; }
                            else if (i == 272) { macroControl[11] = true; macroCount++; }
                            else if (i == 273) { macroControl[12] = true; macroCount++; }
                            else if (i == 274) { macroControl[13] = true; macroCount++; }
                            else if (i == 275) { macroControl[14] = true; macroCount++; }
                            else if (i == 276) { macroControl[15] = true; macroCount++; }
                            else if (i == 277) { macroControl[16] = true; macroCount++; }
                            else if (i == 278) { macroControl[17] = true; macroCount++; }
                            else if (i == 279) { macroControl[18] = true; macroCount++; }
                            else if (i == 280) { macroControl[19] = true; macroCount++; }
                            else if (i == 281) { macroControl[20] = true; macroCount++; }
                            else if (i == 282) { macroControl[21] = true; macroCount++; }
                            else if (i == 283) { macroControl[22] = true; macroCount++; }
                            else if (i == 284) { macroControl[23] = true;macroCount++; }
                            else if (i == 285) { macroControl[24] = true; macroCount++; }
                            else if (keyType.HasFlag(DS4KeyType.ScanCode))
                                InputMethods.performSCKeyPress((ushort)i);
                            else
                                InputMethods.performKeyPress((ushort)i);
                            keydown[i] = true;
                        }
                        else
                        {
                            if (i == 256) InputMethods.MouseEvent(InputMethods.MOUSEEVENTF_LEFTUP); //anything above 255 is not a keyvalue
                            else if (i == 257) InputMethods.MouseEvent(InputMethods.MOUSEEVENTF_RIGHTUP);
                            else if (i == 258) InputMethods.MouseEvent(InputMethods.MOUSEEVENTF_MIDDLEUP);
                            else if (i == 259) InputMethods.MouseEvent(InputMethods.MOUSEEVENTF_XBUTTONUP, 1);
                            else if (i == 260) InputMethods.MouseEvent(InputMethods.MOUSEEVENTF_XBUTTONUP, 2);
                            else if (i == 261) { macroControl[0] = false; if (macroCount > 0) macroCount--; }
                            else if (i == 262) { macroControl[1] = false; if (macroCount > 0) macroCount--; }
                            else if (i == 263) { macroControl[2] = false; if (macroCount > 0) macroCount--; }
                            else if (i == 264) { macroControl[3] = false; if (macroCount > 0) macroCount--; }
                            else if (i == 265) { macroControl[4] = false; if (macroCount > 0) macroCount--; }
                            else if (i == 266) { macroControl[5] = false; if (macroCount > 0) macroCount--; }
                            else if (i == 267) { macroControl[6] = false; if (macroCount > 0) macroCount--; }
                            else if (i == 268) { macroControl[7] = false; if (macroCount > 0) macroCount--; }
                            else if (i == 269) { macroControl[8] = false; if (macroCount > 0) macroCount--; }
                            else if (i == 270) { macroControl[9] = false; if (macroCount > 0) macroCount--; }
                            else if (i == 271) { macroControl[10] = false; if (macroCount > 0) macroCount--; }
                            else if (i == 272) { macroControl[11] = false; if (macroCount > 0) macroCount--; }
                            else if (i == 273) { macroControl[12] = false; if (macroCount > 0) macroCount--; }
                            else if (i == 274) { macroControl[13] = false; if (macroCount > 0) macroCount--; }
                            else if (i == 275) { macroControl[14] = false; if (macroCount > 0) macroCount--; }
                            else if (i == 276) { macroControl[15] = false; if (macroCount > 0) macroCount--; }
                            else if (i == 277) { macroControl[16] = false; if (macroCount > 0) macroCount--; }
                            else if (i == 278) { macroControl[17] = false; if (macroCount > 0) macroCount--; }
                            else if (i == 279) { macroControl[18] = false; if (macroCount > 0) macroCount--; }
                            else if (i == 280) { macroControl[19] = false; if (macroCount > 0) macroCount--; }
                            else if (i == 281) { macroControl[20] = false; if (macroCount > 0) macroCount--; }
                            else if (i == 282) { macroControl[21] = false; if (macroCount > 0) macroCount--; }
                            else if (i == 283) { macroControl[22] = false; if (macroCount > 0) macroCount--; }
                            else if (i == 284) { macroControl[23] = false; if (macroCount > 0) macroCount--; }
                            else if (i == 285) { macroControl[24] = false; if (macroCount > 0) macroCount--; }
                            else if (keyType.HasFlag(DS4KeyType.ScanCode))
                                InputMethods.performSCKeyRelease((ushort)i);
                            else
                                InputMethods.performKeyRelease((ushort)i);
                            keydown[i] = false;
                        }
                    }
                    for (int i = 0, arlength = keydown.Length; i < arlength; i++)
                    {
                        if (keydown[i])
                        {
                            if (i == 256) InputMethods.MouseEvent(InputMethods.MOUSEEVENTF_LEFTUP); //anything above 255 is not a keyvalue
                            else if (i == 257) InputMethods.MouseEvent(InputMethods.MOUSEEVENTF_RIGHTUP);
                            else if (i == 258) InputMethods.MouseEvent(InputMethods.MOUSEEVENTF_MIDDLEUP);
                            else if (i == 259) InputMethods.MouseEvent(InputMethods.MOUSEEVENTF_XBUTTONUP, 1);
                            else if (i == 260) InputMethods.MouseEvent(InputMethods.MOUSEEVENTF_XBUTTONUP, 2);
                            else if (i == 261) { macroControl[0] = false; if (macroCount > 0) macroCount--; }
                            else if (i == 262) { macroControl[1] = false; if (macroCount > 0) macroCount--; }
                            else if (i == 263) { macroControl[2] = false; if (macroCount > 0) macroCount--; }
                            else if (i == 264) { macroControl[3] = false; if (macroCount > 0) macroCount--; }
                            else if (i == 265) { macroControl[4] = false; if (macroCount > 0) macroCount--; }
                            else if (i == 266) { macroControl[5] = false; if (macroCount > 0) macroCount--; }
                            else if (i == 267) { macroControl[6] = false; if (macroCount > 0) macroCount--; }
                            else if (i == 268) { macroControl[7] = false; if (macroCount > 0) macroCount--; }
                            else if (i == 269) { macroControl[8] = false; if (macroCount > 0) macroCount--; }
                            else if (i == 270) { macroControl[9] = false; if (macroCount > 0) macroCount--; }
                            else if (i == 271) { macroControl[10] = false; if (macroCount > 0) macroCount--; }
                            else if (i == 272) { macroControl[11] = false; if (macroCount > 0) macroCount--; }
                            else if (i == 273) { macroControl[12] = false; if (macroCount > 0) macroCount--; }
                            else if (i == 274) { macroControl[13] = false; if (macroCount > 0) macroCount--; }
                            else if (i == 275) { macroControl[14] = false; if (macroCount > 0) macroCount--; }
                            else if (i == 276) { macroControl[15] = false; if (macroCount > 0) macroCount--; }
                            else if (i == 277) { macroControl[16] = false; if (macroCount > 0) macroCount--; }
                            else if (i == 278) { macroControl[17] = false; if (macroCount > 0) macroCount--; }
                            else if (i == 279) { macroControl[18] = false; if (macroCount > 0) macroCount--; }
                            else if (i == 280) { macroControl[19] = false; if (macroCount > 0) macroCount--; }
                            else if (i == 281) { macroControl[20] = false; if (macroCount > 0) macroCount--; }
                            else if (i == 282) { macroControl[21] = false; if (macroCount > 0) macroCount--; }
                            else if (i == 283) { macroControl[22] = false; if (macroCount > 0) macroCount--; }
                            else if (i == 284) { macroControl[23] = false; if (macroCount > 0) macroCount--; }
                            else if (i == 285) { macroControl[24] = false; if (macroCount > 0) macroCount--; }
                            else if (keyType.HasFlag(DS4KeyType.ScanCode))
                                InputMethods.performSCKeyRelease((ushort)i);
                            else
                                InputMethods.performKeyRelease((ushort)i);
                        }
                    }

                    DS4LightBar.forcedFlash[device] = 0;
                    DS4LightBar.forcelight[device] = false;
                    Program.rootHub.DS4Controllers[device].setRumble(0, 0);
                    if (keyType.HasFlag(DS4KeyType.HoldMacro))
                    {
                        await Task.Delay(50);
                        if (control != DS4Controls.None)
                            macrodone[DS4ControltoInt(control)] = false;
                    }
                }
            }
        }

        private static void EndMacro(int device, bool[] macrocontrol, string macro, DS4Controls control)
        {
            if ((macro.StartsWith("164/9/9/164") || macro.StartsWith("18/9/9/18")) && !altTabDone)
                AltTabSwappingRelease();

            if (control != DS4Controls.None)
                macrodone[DS4ControltoInt(control)] = false;
        }

        private static void AltTabSwapping(int wait, int device)
        {
            if (altTabDone)
            {
                altTabDone = false;
                InputMethods.performKeyPress(18);
            }
            else
            {
                altTabNow = DateTime.UtcNow;
                if (altTabNow >= oldAltTabNow + TimeSpan.FromMilliseconds(wait))
                {
                    oldAltTabNow = altTabNow;
                    InputMethods.performKeyPress(9);
                    InputMethods.performKeyRelease(9);
                }
            }
        }

        private static void AltTabSwappingRelease()
        {
            if (altTabNow < DateTime.UtcNow - TimeSpan.FromMilliseconds(10)) //in case multiple controls are mapped to alt+tab
            {
                altTabDone = true;
                InputMethods.performKeyRelease(9);
                InputMethods.performKeyRelease(18);
                altTabNow = DateTime.UtcNow;
                oldAltTabNow = DateTime.UtcNow - TimeSpan.FromDays(1);
            }
        }

        private static void getMouseWheelMapping(int device, DS4Controls control, DS4State cState,
            DS4StateExposed eState, Mouse tp, bool down)
        {
            DateTime now = DateTime.UtcNow;
            if (now >= oldnow + TimeSpan.FromMilliseconds(10) && !pressagain)
            {
                oldnow = now;
                InputMethods.MouseWheel((int)(getByteMapping(device, control, cState, eState, tp) / 51f * (down ? -1 : 1)), 0);
            }
        }

        private static double getMouseMapping(int device, DS4Controls control, DS4State cState, DS4StateExposed eState,
            DS4StateFieldMapping fieldMapping, int mnum, ControlService ctrl)
        {
            int controlnum = DS4ControltoInt(control);

            int deadzoneL = 3;
            int deadzoneR = 3;
            if (getLSDeadzone(device) >= 3)
                deadzoneL = 0;
            if (getRSDeadzone(device) >= 3)
                deadzoneR = 0;

            double value = 0.0;
            int speed = ButtonMouseSensitivity[device];
            double root = 1.002;
            double divide = 10000d;
            //DateTime now = mousenow[mnum];

            int controlNum = (int)control;
            DS4StateFieldMapping.ControlType controlType = DS4StateFieldMapping.mappedType[controlNum];
            //long timeElapsed = ctrl.DS4Controllers[device].getLastTimeElapsed();
            double timeElapsed = ctrl.DS4Controllers[device].lastTimeElapsedDouble;
            //double mouseOffset = 0.025;
            double tempMouseOffsetX = 0.0, tempMouseOffsetY = 0.0;

            if (controlType == DS4StateFieldMapping.ControlType.Button)
            {
                bool active = fieldMapping.buttons[controlNum];
                value = (active ? Math.Pow(root + speed / divide, 100) - 1 : 0);
            }
            else if (controlType == DS4StateFieldMapping.ControlType.AxisDir)
            {
                switch (control)
                {
                    case DS4Controls.LXNeg:
                    {
                        if (cState.LX < 127 - deadzoneL)
                        {
                            double diff = -(cState.LX - 127 - deadzoneL) / (double)(0 - 127 - deadzoneL);
                            //tempMouseOffsetX = Math.Abs(Math.Cos(cState.LSAngleRad)) * MOUSESTICKOFFSET;
                            //tempMouseOffsetX = MOUSESTICKOFFSET;
                            tempMouseOffsetX = cState.LXUnit * MOUSESTICKOFFSET;
                            value = ((speed * MOUSESPEEDFACTOR * (timeElapsed * 0.001)) - tempMouseOffsetX) * diff + (tempMouseOffsetX * -1.0);
                            //value = diff * MOUSESPEEDFACTOR * (timeElapsed * 0.001) * speed;
                            //value = -(cState.LX - 127 - deadzoneL) / 2550d * speed;
                        }

                        break;
                    }
                    case DS4Controls.LXPos:
                    {
                        if (cState.LX > 127 + deadzoneL)
                        {
                            double diff = (cState.LX - 127 + deadzoneL) / (double)(255 - 127 + deadzoneL);
                            tempMouseOffsetX = cState.LXUnit * MOUSESTICKOFFSET;
                            //tempMouseOffsetX = Math.Abs(Math.Cos(cState.LSAngleRad)) * MOUSESTICKOFFSET;
                            //tempMouseOffsetX = MOUSESTICKOFFSET;
                            value = ((speed * MOUSESPEEDFACTOR * (timeElapsed * 0.001)) - tempMouseOffsetX) * diff + tempMouseOffsetX;
                            //value = diff * MOUSESPEEDFACTOR * (timeElapsed * 0.001) * speed;
                            //value = (cState.LX - 127 + deadzoneL) / 2550d * speed;
                        }

                        break;
                    }
                    case DS4Controls.RXNeg:
                    {
                        if (cState.RX < 127 - deadzoneR)
                        {
                            double diff = -(cState.RX - 127 - deadzoneR) / (double)(0 - 127 - deadzoneR);
                            tempMouseOffsetX = cState.RXUnit * MOUSESTICKOFFSET;
                            //tempMouseOffsetX = MOUSESTICKOFFSET;
                            //tempMouseOffsetX = Math.Abs(Math.Cos(cState.RSAngleRad)) * MOUSESTICKOFFSET;
                            value = ((speed * MOUSESPEEDFACTOR * (timeElapsed * 0.001)) - tempMouseOffsetX) * diff + (tempMouseOffsetX * -1.0);
                            //value = diff * MOUSESPEEDFACTOR * (timeElapsed * 0.001) * speed;
                            //value = -(cState.RX - 127 - deadzoneR) / 2550d * speed;
                        }

                        break;
                    }
                    case DS4Controls.RXPos:
                    {
                        if (cState.RX > 127 + deadzoneR)
                        {
                            double diff = (cState.RX - 127 + deadzoneR) / (double)(255 - 127 + deadzoneR);
                            tempMouseOffsetX = cState.RXUnit * MOUSESTICKOFFSET;
                            //tempMouseOffsetX = MOUSESTICKOFFSET;
                            //tempMouseOffsetX = Math.Abs(Math.Cos(cState.RSAngleRad)) * MOUSESTICKOFFSET;
                            value = ((speed * MOUSESPEEDFACTOR * (timeElapsed * 0.001)) - tempMouseOffsetX) * diff + tempMouseOffsetX;
                            //value = diff * MOUSESPEEDFACTOR * (timeElapsed * 0.001) * speed;
                            //value = (cState.RX - 127 + deadzoneR) / 2550d * speed;
                        }

                        break;
                    }
                    case DS4Controls.LYNeg:
                    {
                        if (cState.LY < 127 - deadzoneL)
                        {
                            double diff = -(cState.LY - 127 - deadzoneL) / (double)(0 - 127 - deadzoneL);
                            tempMouseOffsetY = cState.LYUnit * MOUSESTICKOFFSET;
                            //tempMouseOffsetY = MOUSESTICKOFFSET;
                            //tempMouseOffsetY = Math.Abs(Math.Sin(cState.LSAngleRad)) * MOUSESTICKOFFSET;
                            value = ((speed * MOUSESPEEDFACTOR * (timeElapsed * 0.001)) - tempMouseOffsetY) * diff + (tempMouseOffsetY * -1.0);
                            //value = diff * MOUSESPEEDFACTOR * (timeElapsed * 0.001) * speed;
                            //value = -(cState.LY - 127 - deadzoneL) / 2550d * speed;
                        }

                        break;
                    }
                    case DS4Controls.LYPos:
                    {
                        if (cState.LY > 127 + deadzoneL)
                        {
                            double diff = (cState.LY - 127 + deadzoneL) / (double)(255 - 127 + deadzoneL);
                            tempMouseOffsetY = cState.LYUnit * MOUSESTICKOFFSET;
                            //tempMouseOffsetY = MOUSESTICKOFFSET;
                            //tempMouseOffsetY = Math.Abs(Math.Sin(cState.LSAngleRad)) * MOUSESTICKOFFSET;
                            value = ((speed * MOUSESPEEDFACTOR * (timeElapsed * 0.001)) - tempMouseOffsetY) * diff + tempMouseOffsetY;
                            //value = diff * MOUSESPEEDFACTOR * (timeElapsed * 0.001) * speed;
                            //value = (cState.LY - 127 + deadzoneL) / 2550d * speed;
                        }

                        break;
                    }
                    case DS4Controls.RYNeg:
                    {
                        if (cState.RY < 127 - deadzoneR)
                        {
                            double diff = -(cState.RY - 127 - deadzoneR) / (double)(0 - 127 - deadzoneR);
                            tempMouseOffsetY = cState.RYUnit * MOUSESTICKOFFSET;
                            //tempMouseOffsetY = MOUSESTICKOFFSET;
                            //tempMouseOffsetY = Math.Abs(Math.Sin(cState.RSAngleRad)) * MOUSESTICKOFFSET;
                            value = ((speed * MOUSESPEEDFACTOR * (timeElapsed * 0.001)) - tempMouseOffsetY) * diff + (tempMouseOffsetY * -1.0);
                            //value = diff * MOUSESPEEDFACTOR * (timeElapsed * 0.001) * speed;
                            //value = -(cState.RY - 127 - deadzoneR) / 2550d * speed;
                        }

                        break;
                    }
                    case DS4Controls.RYPos:
                    {
                        if (cState.RY > 127 + deadzoneR)
                        {
                            double diff = (cState.RY - 127 + deadzoneR) / (double)(255 - 127 + deadzoneR);
                            tempMouseOffsetY = cState.RYUnit * MOUSESTICKOFFSET;
                            //tempMouseOffsetY = MOUSESTICKOFFSET;
                            //tempMouseOffsetY = Math.Abs(Math.Sin(cState.RSAngleRad)) * MOUSESTICKOFFSET;
                            value = ((speed * MOUSESPEEDFACTOR * (timeElapsed * 0.001)) - tempMouseOffsetY) * diff + tempMouseOffsetY;
                            //value = diff * MOUSESPEEDFACTOR * (timeElapsed * 0.001) * speed;
                            //value = (cState.RY - 127 + deadzoneR) / 2550d * speed;
                        }

                        break;
                    }

                    default: break;
                }
            }
            else if (controlType == DS4StateFieldMapping.ControlType.Trigger)
            {
                byte trigger = fieldMapping.triggers[controlNum];
                value = Math.Pow(root + speed / divide, trigger / 2d) - 1;
            }
            else if (controlType == DS4StateFieldMapping.ControlType.GyroDir)
            {
                //double SXD = getSXDeadzone(device);
                //double SZD = getSZDeadzone(device);

                switch (control)
                {
                    case DS4Controls.GyroXPos:
                    {
                        int gyroX = fieldMapping.gryodirs[controlNum];
                        value = (byte)(gyroX > 0 ? Math.Pow(root + speed / divide, gyroX) : 0);
                        break;
                    }
                    case DS4Controls.GyroXNeg:
                    {
                        int gyroX = fieldMapping.gryodirs[controlNum];
                        value = (byte)(gyroX < 0 ? Math.Pow(root + speed / divide, -gyroX) : 0);
                        break;
                    }
                    case DS4Controls.GyroZPos:
                    {
                        int gyroZ = fieldMapping.gryodirs[controlNum];
                        value = (byte)(gyroZ > 0 ? Math.Pow(root + speed / divide, gyroZ) : 0);
                        break;
                    }
                    case DS4Controls.GyroZNeg:
                    {
                        int gyroZ = fieldMapping.gryodirs[controlNum];
                        value = (byte)(gyroZ < 0 ? Math.Pow(root + speed / divide, -gyroZ) : 0);
                        break;
                    }
                    default: break;
                }
            }

            if (getMouseAccel(device))
            {
                if (value > 0)
                {
                    mcounter = 34;
                    mouseaccel++;
                }

                if (mouseaccel == prevmouseaccel)
                {
                    mcounter--;
                }

                if (mcounter <= 0)
                {
                    mouseaccel = 0;
                    mcounter = 34;
                }

                value *= 1 + Math.Min(20000, (mouseaccel)) / 10000d;
                prevmouseaccel = mouseaccel;
            }

            return value;
        }

        private static void calculateFinalMouseMovement(ref double rawMouseX, ref double rawMouseY,
            out int mouseX, out int mouseY)
        {
            if ((rawMouseX > 0.0 && horizontalRemainder > 0.0) || (rawMouseX < 0.0 && horizontalRemainder < 0.0))
            {
                rawMouseX += horizontalRemainder;
            }
            else
            {
                horizontalRemainder = 0.0;
            }

            //double mouseXTemp = rawMouseX - (Math.IEEERemainder(rawMouseX * 1000.0, 1.0) / 1000.0);
            double mouseXTemp = rawMouseX - (remainderCutoff(rawMouseX * 1000.0, 1.0) / 1000.0);
            //double mouseXTemp = rawMouseX - (rawMouseX * 1000.0 - (1.0 * (int)(rawMouseX * 1000.0 / 1.0)));
            mouseX = (int)mouseXTemp;
            horizontalRemainder = mouseXTemp - mouseX;
            //mouseX = (int)rawMouseX;
            //horizontalRemainder = rawMouseX - mouseX;

            if ((rawMouseY > 0.0 && verticalRemainder > 0.0) || (rawMouseY < 0.0 && verticalRemainder < 0.0))
            {
                rawMouseY += verticalRemainder;
            }
            else
            {
                verticalRemainder = 0.0;
            }

            //double mouseYTemp = rawMouseY - (Math.IEEERemainder(rawMouseY * 1000.0, 1.0) / 1000.0);
            double mouseYTemp = rawMouseY - (remainderCutoff(rawMouseY * 1000.0, 1.0) / 1000.0);
            mouseY = (int)mouseYTemp;
            verticalRemainder = mouseYTemp - mouseY;
            //mouseY = (int)rawMouseY;
            //verticalRemainder = rawMouseY - mouseY;
        }

        private static double remainderCutoff(double dividend, double divisor)
        {
            return dividend - (divisor * (int)(dividend / divisor));
        }

        public static bool compare(byte b1, byte b2)
        {
            bool result = true;
            if (Math.Abs(b1 - b2) > 10)
            {
                result = false;
            }

            return result;
        }

        private static byte getByteMapping2(int device, DS4Controls control, DS4State cState, DS4StateExposed eState, Mouse tp,
            DS4StateFieldMapping fieldMap)
        {
            byte result = 0;

            int controlNum = (int)control;
            DS4StateFieldMapping.ControlType controlType = DS4StateFieldMapping.mappedType[controlNum];
            if (controlType == DS4StateFieldMapping.ControlType.Button)
            {
                result = (byte)(fieldMap.buttons[controlNum] ? 255 : 0);
            }
            else if (controlType == DS4StateFieldMapping.ControlType.AxisDir)
            {
                byte axisValue = fieldMap.axisdirs[controlNum];

                switch (control)
                {
                    case DS4Controls.LXNeg: result = (byte)(axisValue - 127.5f > 0 ? 0 : -(axisValue - 127.5f) * 2); break;
                    case DS4Controls.LYNeg: result = (byte)(axisValue - 127.5f > 0 ? 0 : -(axisValue - 127.5f) * 2); break;
                    case DS4Controls.RXNeg: result = (byte)(axisValue - 127.5f > 0 ? 0 : -(axisValue - 127.5f) * 2); break;
                    case DS4Controls.RYNeg: result = (byte)(axisValue - 127.5f > 0 ? 0 : -(axisValue - 127.5f) * 2); break;
                    default: result = (byte)(axisValue - 127.5f < 0 ? 0 : (axisValue - 127.5f) * 2); break;
                }
            }
            else if (controlType == DS4StateFieldMapping.ControlType.Trigger)
            {
                result = fieldMap.triggers[controlNum];
            }
            else if (controlType == DS4StateFieldMapping.ControlType.Touch)
            {
                result = (byte)(tp != null && fieldMap.buttons[controlNum] ? 255 : 0);
            }
            else if (controlType == DS4StateFieldMapping.ControlType.SwipeDir)
            {
                result = (byte)(tp != null ? fieldMap.swipedirs[controlNum] : 0);
            }
            else if (controlType == DS4StateFieldMapping.ControlType.GyroDir)
            {
                bool sOff = isUsingSAforMouse(device);

                switch (control)
                {
                    case DS4Controls.GyroXPos:
                    {
                        int gyroX = fieldMap.gryodirs[controlNum];
                        result = (byte)(sOff == false ? Math.Min(255, gyroX * 2) : 0);
                        break;
                    }
                    case DS4Controls.GyroXNeg:
                    {
                        int gyroX = fieldMap.gryodirs[controlNum];
                        result = (byte)(sOff == false ? Math.Min(255, -gyroX * 2) : 0);
                        break;
                    }
                    case DS4Controls.GyroZPos:
                    {
                        int gyroZ = fieldMap.gryodirs[controlNum];
                        result = (byte)(sOff == false ? Math.Min(255, gyroZ * 2) : 0);
                        break;
                    }
                    case DS4Controls.GyroZNeg:
                    {
                        int gyroZ = fieldMap.gryodirs[controlNum];
                        result = (byte)(sOff == false ? Math.Min(255, -gyroZ * 2) : 0);
                        break;
                    }
                    default: break;
                }
            }

            return result;
        }

        public static byte getByteMapping(int device, DS4Controls control, DS4State cState, DS4StateExposed eState, Mouse tp)
        {
            byte result = 0;

            if (control >= DS4Controls.Square && control <= DS4Controls.Cross)
            {
                switch (control)
                {
                    case DS4Controls.Cross: result = (byte)(cState.Cross ? 255 : 0); break;
                    case DS4Controls.Square: result = (byte)(cState.Square ? 255 : 0); break;
                    case DS4Controls.Triangle: result = (byte)(cState.Triangle ? 255 : 0); break;
                    case DS4Controls.Circle: result = (byte)(cState.Circle ? 255 : 0); break;
                    default: break;
                }
            }
            else if (control >= DS4Controls.L1 && control <= DS4Controls.R3)
            {
                switch (control)
                {
                    case DS4Controls.L1: result = (byte)(cState.L1 ? 255 : 0); break;
                    case DS4Controls.L2: result = cState.L2; break;
                    case DS4Controls.L3: result = (byte)(cState.L3 ? 255 : 0); break;
                    case DS4Controls.R1: result = (byte)(cState.R1 ? 255 : 0); break;
                    case DS4Controls.R2: result = cState.R2; break;
                    case DS4Controls.R3: result = (byte)(cState.R3 ? 255 : 0); break;
                    default: break;
                }
            }
            else if (control >= DS4Controls.DpadUp && control <= DS4Controls.DpadLeft)
            {
                switch (control)
                {
                    case DS4Controls.DpadUp: result = (byte)(cState.DpadUp ? 255 : 0); break;
                    case DS4Controls.DpadDown: result = (byte)(cState.DpadDown ? 255 : 0); break;
                    case DS4Controls.DpadLeft: result = (byte)(cState.DpadLeft ? 255 : 0); break;
                    case DS4Controls.DpadRight: result = (byte)(cState.DpadRight ? 255 : 0); break;
                    default: break;
                }
            }
            else if (control >= DS4Controls.LXNeg && control <= DS4Controls.RYPos)
            {
                switch (control)
                {
                    case DS4Controls.LXNeg: result = (byte)(cState.LX - 127.5f > 0 ? 0 : -(cState.LX - 127.5f) * 2); break;
                    case DS4Controls.LYNeg: result = (byte)(cState.LY - 127.5f > 0 ? 0 : -(cState.LY - 127.5f) * 2); break;
                    case DS4Controls.RXNeg: result = (byte)(cState.RX - 127.5f > 0 ? 0 : -(cState.RX - 127.5f) * 2); break;
                    case DS4Controls.RYNeg: result = (byte)(cState.RY - 127.5f > 0 ? 0 : -(cState.RY - 127.5f) * 2); break;
                    case DS4Controls.LXPos: result = (byte)(cState.LX - 127.5f < 0 ? 0 : (cState.LX - 127.5f) * 2); break;
                    case DS4Controls.LYPos: result = (byte)(cState.LY - 127.5f < 0 ? 0 : (cState.LY - 127.5f) * 2); break;
                    case DS4Controls.RXPos: result = (byte)(cState.RX - 127.5f < 0 ? 0 : (cState.RX - 127.5f) * 2); break;
                    case DS4Controls.RYPos: result = (byte)(cState.RY - 127.5f < 0 ? 0 : (cState.RY - 127.5f) * 2); break;
                    default: break;
                }
            }
            else if (control >= DS4Controls.TouchLeft && control <= DS4Controls.TouchRight)
            {
                switch (control)
                {
                    case DS4Controls.TouchLeft: result = (byte)(tp != null && tp.leftDown ? 255 : 0); break;
                    case DS4Controls.TouchRight: result = (byte)(tp != null && tp.rightDown ? 255 : 0); break;
                    case DS4Controls.TouchMulti: result = (byte)(tp != null && tp.multiDown ? 255 : 0); break;
                    case DS4Controls.TouchUpper: result = (byte)(tp != null && tp.upperDown ? 255 : 0); break;
                    default: break;
                }
            }
            else if (control >= DS4Controls.SwipeLeft && control <= DS4Controls.SwipeDown)
            {
                switch (control)
                {
                    case DS4Controls.SwipeUp: result = (byte)(tp != null ? tp.swipeUpB : 0); break;
                    case DS4Controls.SwipeDown: result = (byte)(tp != null ? tp.swipeDownB : 0); break;
                    case DS4Controls.SwipeLeft: result = (byte)(tp != null ? tp.swipeLeftB : 0); break;
                    case DS4Controls.SwipeRight: result = (byte)(tp != null ? tp.swipeRightB : 0); break;
                    default: break;
                }
            }
            else if (control >= DS4Controls.GyroXPos && control <= DS4Controls.GyroZNeg)
            {
                double SXD = getSXDeadzone(device);
                double SZD = getSZDeadzone(device);
                bool sOff = isUsingSAforMouse(device);
                double sxsens = getSXSens(device);
                double szsens = getSZSens(device);

                switch (control)
                {
                    case DS4Controls.GyroXPos:
                    {
                        int gyroX = -eState.AccelX;
                        result = (byte)(!sOff && sxsens * gyroX > SXD * 10 ? Math.Min(255, sxsens * gyroX * 2) : 0);
                        break;
                    }
                    case DS4Controls.GyroXNeg:
                    {
                        int gyroX = -eState.AccelX;
                        result = (byte)(!sOff && sxsens * gyroX < -SXD * 10 ? Math.Min(255, sxsens * -gyroX * 2) : 0);
                        break;
                    }
                    case DS4Controls.GyroZPos:
                    {
                        int gyroZ = eState.AccelZ;
                        result = (byte)(!sOff && szsens * gyroZ > SZD * 10 ? Math.Min(255, szsens * gyroZ * 2) : 0);
                        break;
                    }
                    case DS4Controls.GyroZNeg:
                    {
                        int gyroZ = eState.AccelZ;
                        result = (byte)(!sOff && szsens * gyroZ < -SZD * 10 ? Math.Min(255, szsens * -gyroZ * 2) : 0);
                        break;
                    }
                    default: break;
                }
            }
            else
            {
                switch (control)
                {
                    case DS4Controls.Share: result = (byte)(cState.Share ? 255 : 0); break;
                    case DS4Controls.Options: result = (byte)(cState.Options ? 255 : 0); break;
                    case DS4Controls.PS: result = (byte)(cState.PS ? 255 : 0); break;
                    default: break;
                }
            }

            return result;
        }

        /* TODO: Possibly remove usage of this version of the method */
        public static bool getBoolMapping(int device, DS4Controls control, DS4State cState, DS4StateExposed eState, Mouse tp)
        {
            bool result = false;

            if (control >= DS4Controls.Square && control <= DS4Controls.Cross)
            {
                switch (control)
                {
                    case DS4Controls.Cross: result = cState.Cross; break;
                    case DS4Controls.Square: result = cState.Square; break;
                    case DS4Controls.Triangle: result = cState.Triangle; break;
                    case DS4Controls.Circle: result = cState.Circle; break;
                    default: break;
                }
            }
            else if (control >= DS4Controls.L1 && control <= DS4Controls.R3)
            {
                switch (control)
                {
                    case DS4Controls.L1: result = cState.L1; break;
                    case DS4Controls.R1: result = cState.R1; break;
                    case DS4Controls.L2: result = cState.L2 > 100; break;
                    case DS4Controls.R2: result = cState.R2 > 100; break;
                    case DS4Controls.L3: result = cState.L3; break;
                    case DS4Controls.R3: result = cState.R3; break;
                    default: break;
                }
            }
            else if (control >= DS4Controls.DpadUp && control <= DS4Controls.DpadLeft)
            {
                switch (control)
                {
                    case DS4Controls.DpadUp: result = cState.DpadUp; break;
                    case DS4Controls.DpadDown: result = cState.DpadDown; break;
                    case DS4Controls.DpadLeft: result = cState.DpadLeft; break;
                    case DS4Controls.DpadRight: result = cState.DpadRight; break;
                    default: break;
                }
            }
            else if (control >= DS4Controls.LXNeg && control <= DS4Controls.RYPos)
            {
                switch (control)
                {
                    case DS4Controls.LXNeg: result = cState.LX < 127 - 55; break;
                    case DS4Controls.LYNeg: result = cState.LY < 127 - 55; break;
                    case DS4Controls.RXNeg: result = cState.RX < 127 - 55; break;
                    case DS4Controls.RYNeg: result = cState.RY < 127 - 55; break;
                    case DS4Controls.LXPos: result = cState.LX > 127 + 55; break;
                    case DS4Controls.LYPos: result = cState.LY > 127 + 55; break;
                    case DS4Controls.RXPos: result = cState.RX > 127 + 55; break;
                    case DS4Controls.RYPos: result = cState.RY > 127 + 55; break;
                    default: break;
                }
            }
            else if (control >= DS4Controls.TouchLeft && control <= DS4Controls.TouchRight)
            {
                switch (control)
                {
                    case DS4Controls.TouchLeft: result = (tp != null ? tp.leftDown : false); break;
                    case DS4Controls.TouchRight: result = (tp != null ? tp.rightDown : false); break;
                    case DS4Controls.TouchMulti: result = (tp != null ? tp.multiDown : false); break;
                    case DS4Controls.TouchUpper: result = (tp != null ? tp.upperDown : false); break;
                    default: break;
                }
            }
            else if (control >= DS4Controls.SwipeLeft && control <= DS4Controls.SwipeDown)
            {
                switch (control)
                {
                    case DS4Controls.SwipeUp: result = (tp != null && tp.swipeUp); break;
                    case DS4Controls.SwipeDown: result = (tp != null && tp.swipeDown); break;
                    case DS4Controls.SwipeLeft: result = (tp != null && tp.swipeLeft); break;
                    case DS4Controls.SwipeRight: result = (tp != null && tp.swipeRight); break;
                    default: break;
                }
            }
            else if (control >= DS4Controls.GyroXPos && control <= DS4Controls.GyroZNeg)
            {
                bool sOff = isUsingSAforMouse(device);

                switch (control)
                {
                    case DS4Controls.GyroXPos: result = !sOff ? SXSens[device] * -eState.AccelX > 67 : false; break;
                    case DS4Controls.GyroXNeg: result = !sOff ? SXSens[device] * -eState.AccelX < -67 : false; break;
                    case DS4Controls.GyroZPos: result = !sOff ? SZSens[device] * eState.AccelZ > 67 : false; break;
                    case DS4Controls.GyroZNeg: result = !sOff ? SZSens[device] * eState.AccelZ < -67 : false; break;
                    default: break;
                }
            }
            else
            {
                switch (control)
                {
                    case DS4Controls.PS: result = cState.PS; break;
                    case DS4Controls.Share: result = cState.Share; break;
                    case DS4Controls.Options: result = cState.Options; break;
                    default: break;
                }
            }

            return result;
        }

        private static bool getBoolMapping2(int device, DS4Controls control,
            DS4State cState, DS4StateExposed eState, Mouse tp, DS4StateFieldMapping fieldMap)
        {
            bool result = false;

            int controlNum = (int)control;
            DS4StateFieldMapping.ControlType controlType = DS4StateFieldMapping.mappedType[controlNum];
            if (controlType == DS4StateFieldMapping.ControlType.Button)
            {
                result = fieldMap.buttons[controlNum];
            }
            else if (controlType == DS4StateFieldMapping.ControlType.AxisDir)
            {
                byte axisValue = fieldMap.axisdirs[controlNum];

                switch (control)
                {
                    case DS4Controls.LXNeg: result = cState.LX < 127 - 55; break;
                    case DS4Controls.LYNeg: result = cState.LY < 127 - 55; break;
                    case DS4Controls.RXNeg: result = cState.RX < 127 - 55; break;
                    case DS4Controls.RYNeg: result = cState.RY < 127 - 55; break;
                    default: result = axisValue > 127 + 55; break;
                }
            }
            else if (controlType == DS4StateFieldMapping.ControlType.Trigger)
            {
                result = fieldMap.triggers[controlNum] > 100;
            }
            else if (controlType == DS4StateFieldMapping.ControlType.Touch)
            {
                result = fieldMap.buttons[controlNum];
            }
            else if (controlType == DS4StateFieldMapping.ControlType.SwipeDir)
            {
                result = fieldMap.swipedirbools[controlNum];
            }
            else if (controlType == DS4StateFieldMapping.ControlType.GyroDir)
            {
                bool sOff = isUsingSAforMouse(device);
                bool safeTest = false;

                switch (control)
                {
                    case DS4Controls.GyroXPos: safeTest = fieldMap.gryodirs[controlNum] > 0; break;
                    case DS4Controls.GyroXNeg: safeTest = fieldMap.gryodirs[controlNum] < -0; break;
                    case DS4Controls.GyroZPos: safeTest = fieldMap.gryodirs[controlNum] > 0; break;
                    case DS4Controls.GyroZNeg: safeTest = fieldMap.gryodirs[controlNum] < -0; break;
                    default: break;
                }

                result = sOff == false ? safeTest : false;
            }

            return result;
        }

        private static bool getBoolActionMapping2(int device, DS4Controls control,
            DS4State cState, DS4StateExposed eState, Mouse tp, DS4StateFieldMapping fieldMap, bool analog = false)
        {
            bool result = false;

            int controlNum = (int)control;
            DS4StateFieldMapping.ControlType controlType = DS4StateFieldMapping.mappedType[controlNum];
            if (controlType == DS4StateFieldMapping.ControlType.Button)
            {
                result = fieldMap.buttons[controlNum];
            }
            else if (controlType == DS4StateFieldMapping.ControlType.AxisDir)
            {
                switch (control)
                {
                    case DS4Controls.LXNeg:
                    {
                        double angle = cState.LSAngle;
                        result = cState.LX < 127 && (angle >= 112.5 && angle <= 247.5);
                        break;
                    }
                    case DS4Controls.LYNeg:
                    {
                        double angle = cState.LSAngle;
                        result = cState.LY < 127 && (angle >= 22.5 && angle <= 157.5);
                        break;
                    }
                    case DS4Controls.RXNeg:
                    {
                        double angle = cState.RSAngle;
                        result = cState.RX < 127 && (angle >= 112.5 && angle <= 247.5);
                        break;
                    }
                    case DS4Controls.RYNeg:
                    {
                        double angle = cState.RSAngle;
                        result = cState.RY < 127 && (angle >= 22.5 && angle <= 157.5);
                        break;
                    }
                    case DS4Controls.LXPos:
                    {
                        double angle = cState.LSAngle;
                        result = cState.LX > 127 && (angle <= 67.5 || angle >= 292.5);
                        break;
                    }
                    case DS4Controls.LYPos:
                    {
                        double angle = cState.LSAngle;
                        result = cState.LY > 127 && (angle >= 202.5 && angle <= 337.5);
                        break;
                    }
                    case DS4Controls.RXPos:
                    {
                        double angle = cState.RSAngle;
                        result = cState.RX > 127 && (angle <= 67.5 || angle >= 292.5);
                        break;
                    }
                    case DS4Controls.RYPos:
                    {
                        double angle = cState.RSAngle;
                        result = cState.RY > 127 && (angle >= 202.5 && angle <= 337.5);
                        break;
                    }
                    default: break;
                }
            }
            else if (controlType == DS4StateFieldMapping.ControlType.Trigger)
            {
                result = fieldMap.triggers[controlNum] > 0;
            }
            else if (controlType == DS4StateFieldMapping.ControlType.Touch)
            {
                result = fieldMap.buttons[controlNum];
            }
            else if (controlType == DS4StateFieldMapping.ControlType.SwipeDir)
            {
                result = fieldMap.swipedirbools[controlNum];
            }
            else if (controlType == DS4StateFieldMapping.ControlType.GyroDir)
            {
                bool sOff = isUsingSAforMouse(device);
                bool safeTest = false;

                switch (control)
                {
                    case DS4Controls.GyroXPos: safeTest = fieldMap.gryodirs[controlNum] > 0; break;
                    case DS4Controls.GyroXNeg: safeTest = fieldMap.gryodirs[controlNum] < 0; break;
                    case DS4Controls.GyroZPos: safeTest = fieldMap.gryodirs[controlNum] > 0; break;
                    case DS4Controls.GyroZNeg: safeTest = fieldMap.gryodirs[controlNum] < 0; break;
                    default: break;
                }

                result = sOff == false ? safeTest : false;
            }

            return result;
        }

        /* TODO: Possibly remove usage of this version of the method */
        public static bool getBoolActionMapping(int device, DS4Controls control,
            DS4State cState, DS4StateExposed eState, Mouse tp, bool analog=false)
        {
            bool result = false;

            if (control >= DS4Controls.Square && control <= DS4Controls.Cross)
            {
                switch (control)
                {
                    case DS4Controls.Cross: result = cState.Cross; break;
                    case DS4Controls.Square: result = cState.Square; break;
                    case DS4Controls.Triangle: result = cState.Triangle; break;
                    case DS4Controls.Circle: result = cState.Circle; break;
                    default: break;
                }
            }
            else if (control >= DS4Controls.L1 && control <= DS4Controls.R3)
            {
                switch (control)
                {
                    case DS4Controls.L1: result = cState.L1; break;
                    case DS4Controls.R1: result = cState.R1; break;
                    case DS4Controls.L2: result = cState.L2 > 0; break;
                    case DS4Controls.R2: result = cState.R2 > 0; break;
                    case DS4Controls.L3: result = cState.L3; break;
                    case DS4Controls.R3: result = cState.R3; break;
                    default: break;
                }
            }
            else if (control >= DS4Controls.DpadUp && control <= DS4Controls.DpadLeft)
            {
                switch (control)
                {
                    case DS4Controls.DpadUp: result = cState.DpadUp; break;
                    case DS4Controls.DpadDown: result = cState.DpadDown; break;
                    case DS4Controls.DpadLeft: result = cState.DpadLeft; break;
                    case DS4Controls.DpadRight: result = cState.DpadRight; break;
                    default: break;
                }
            }
            else if (control >= DS4Controls.LXNeg && control <= DS4Controls.RYPos)
            {
                switch (control)
                {
                    case DS4Controls.LXNeg:
                    {
                        if (!analog)
                        {
                            double angle = cState.LSAngle;
                            result = cState.LX < 127 && (angle >= 112.5 && angle <= 247.5);
                        }
                        else
                        {
                            result = cState.LX < 127;
                        }

                        break;
                    }
                    case DS4Controls.LYNeg:
                    {
                        if (!analog)
                        {
                            double angle = cState.LSAngle;
                            result = cState.LY < 127 && (angle >= 22.5 && angle <= 157.5);
                        }
                        else
                        {
                            result = cState.LY < 127;
                        }

                        break;
                    }
                    case DS4Controls.RXNeg:
                    {
                        if (!analog)
                        {
                            double angle = cState.RSAngle;
                            result = cState.RX < 127 && (angle >= 112.5 && angle <= 247.5);
                        }
                        else
                        {
                            result = cState.RX < 127;
                        }

                        break;
                    }
                    case DS4Controls.RYNeg:
                    {
                        if (!analog)
                        {
                            double angle = cState.RSAngle;
                            result = cState.RY < 127 && (angle >= 22.5 && angle <= 157.5);
                        }
                        else
                        {
                            result = cState.RY < 127;
                        }

                        break;
                    }
                    case DS4Controls.LXPos:
                    {
                        if (!analog)
                        {
                            double angle = cState.LSAngle;
                            result = cState.LX > 127 && (angle <= 67.5 || angle >= 292.5);
                        }
                        else
                        {
                            result = cState.LX > 127;
                        }

                        break;
                    }
                    case DS4Controls.LYPos:
                    {
                        if (!analog)
                        {
                            double angle = cState.LSAngle;
                            result = cState.LY > 127 && (angle >= 202.5 && angle <= 337.5);
                        }
                        else
                        {
                            result = cState.LY > 127;
                        }

                        break;
                    }
                    case DS4Controls.RXPos:
                    {
                        if (!analog)
                        {
                            double angle = cState.RSAngle;
                            result = cState.RX > 127 && (angle <= 67.5 || angle >= 292.5);
                        }
                        else
                        {
                            result = cState.RX > 127;
                        }

                        break;
                    }
                    case DS4Controls.RYPos:
                    {
                        if (!analog)
                        {
                            double angle = cState.RSAngle;
                            result = cState.RY > 127 && (angle >= 202.5 && angle <= 337.5);
                        }
                        else
                        {
                            result = cState.RY > 127;
                        }

                        break;
                    }
                    default: break;
                }
            }
            else if (control >= DS4Controls.TouchLeft && control <= DS4Controls.TouchRight)
            {
                switch (control)
                {
                    case DS4Controls.TouchLeft: result = (tp != null ? tp.leftDown : false); break;
                    case DS4Controls.TouchRight: result = (tp != null ? tp.rightDown : false); break;
                    case DS4Controls.TouchMulti: result = (tp != null ? tp.multiDown : false); break;
                    case DS4Controls.TouchUpper: result = (tp != null ? tp.upperDown : false); break;
                    default: break;
                }
            }
            else if (control >= DS4Controls.SwipeLeft && control <= DS4Controls.SwipeDown)
            {
                switch (control)
                {
                    case DS4Controls.SwipeUp: result = (tp != null && tp.swipeUp); break;
                    case DS4Controls.SwipeDown: result = (tp != null && tp.swipeDown); break;
                    case DS4Controls.SwipeLeft: result = (tp != null && tp.swipeLeft); break;
                    case DS4Controls.SwipeRight: result = (tp != null && tp.swipeRight); break;
                    default: break;
                }
            }
            else if (control >= DS4Controls.GyroXPos && control <= DS4Controls.GyroZNeg)
            {
                bool sOff = isUsingSAforMouse(device);

                switch (control)
                {
                    case DS4Controls.GyroXPos: result = !sOff ? SXSens[device] * eState.AccelX > 67 : false; break;
                    case DS4Controls.GyroXNeg: result = !sOff ? SXSens[device] * eState.AccelX < -67 : false; break;
                    case DS4Controls.GyroZPos: result = !sOff ? SZSens[device] * eState.AccelZ > 67 : false; break;
                    case DS4Controls.GyroZNeg: result = !sOff ? SZSens[device] * eState.AccelZ < -67 : false; break;
                    default: break;
                }
            }
            else
            {
                switch (control)
                {
                    case DS4Controls.PS: result = cState.PS; break;
                    case DS4Controls.Share: result = cState.Share; break;
                    case DS4Controls.Options: result = cState.Options; break;
                    default: break;
                }
            }

            return result;
        }

        public static bool getBoolButtonMapping(bool stateButton)
        {
            return stateButton;
        }

        public static bool getBoolAxisDirMapping(byte stateAxis, bool positive)
        {
            return positive ? stateAxis > 127 + 55 : stateAxis < 127 - 55;
        }

        public static bool getBoolTriggerMapping(byte stateAxis)
        {
            return stateAxis > 100;
        }

        public static bool getBoolTouchMapping(bool touchButton)
        {
            return touchButton;
        }

        private static byte getXYAxisMapping2(int device, DS4Controls control, DS4State cState,
            DS4StateExposed eState, Mouse tp, DS4StateFieldMapping fieldMap, bool alt = false)
        {
            byte result = 0;
            byte trueVal = 0;
            byte falseVal = 127;

            if (alt)
                trueVal = 255;

            int controlNum = (int)control;
            DS4StateFieldMapping.ControlType controlType = DS4StateFieldMapping.mappedType[controlNum];

            if (controlType == DS4StateFieldMapping.ControlType.Button)
            {
                result = fieldMap.buttons[controlNum] ? trueVal : falseVal;
            }
            else if (controlType == DS4StateFieldMapping.ControlType.AxisDir)
            {
                byte axisValue = fieldMap.axisdirs[controlNum];

                switch (control)
                {
                    case DS4Controls.LXNeg: if (!alt) result = cState.LX; else result = (byte)(255 - cState.LX); break;
                    case DS4Controls.LYNeg: if (!alt) result = cState.LY; else result = (byte)(255 - cState.LY); break;
                    case DS4Controls.RXNeg: if (!alt) result = cState.RX; else result = (byte)(255 - cState.RX); break;
                    case DS4Controls.RYNeg: if (!alt) result = cState.RY; else result = (byte)(255 - cState.RY); break;
                    default: if (!alt) result = (byte)(255 - axisValue); else result = axisValue; break;
                }
            }
            else if (controlType == DS4StateFieldMapping.ControlType.Trigger)
            {
                if (alt)
                {
                    result = (byte)(127.5f + fieldMap.triggers[controlNum] / 2f);
                }
                else
                {
                    result = (byte)(127.5f - fieldMap.triggers[controlNum] / 2f);
                }
            }
            else if (controlType == DS4StateFieldMapping.ControlType.Touch)
            {
                result = fieldMap.buttons[controlNum] ? trueVal : falseVal;
            }
            else if (controlType == DS4StateFieldMapping.ControlType.SwipeDir)
            {
                if (alt)
                {
                    result = (byte)(tp != null ? 127.5f + fieldMap.swipedirs[controlNum] / 2f : 0);
                }
                else
                {
                    result = (byte)(tp != null ? 127.5f - fieldMap.swipedirs[controlNum] / 2f : 0);
                }
            }
            else if (controlType == DS4StateFieldMapping.ControlType.GyroDir)
            {
                bool sOff = isUsingSAforMouse(device);

                switch (control)
                {
                    case DS4Controls.GyroXPos:
                    {
                        if (sOff == false && fieldMap.gryodirs[controlNum] > 0)
                        {
                            if (alt) result = (byte)Math.Min(255, 127 + fieldMap.gryodirs[controlNum]); else result = (byte)Math.Max(0, 127 - fieldMap.gryodirs[controlNum]);
                        }
                        else result = falseVal;
                        break;
                    }
                    case DS4Controls.GyroXNeg:
                    {
                        if (sOff == false && fieldMap.gryodirs[controlNum] < 0)
                        {
                            if (alt) result = (byte)Math.Min(255, 127 + -fieldMap.gryodirs[controlNum]); else result = (byte)Math.Max(0, 127 - -fieldMap.gryodirs[controlNum]);
                        }
                        else result = falseVal;
                        break;
                    }
                    case DS4Controls.GyroZPos:
                    {
                        if (sOff == false && fieldMap.gryodirs[controlNum] > 0)
                        {
                            if (alt) result = (byte)Math.Min(255, 127 + fieldMap.gryodirs[controlNum]); else result = (byte)Math.Max(0, 127 - fieldMap.gryodirs[controlNum]);
                        }
                        else return falseVal;
                        break;
                    }
                    case DS4Controls.GyroZNeg:
                    {
                        if (sOff == false && fieldMap.gryodirs[controlNum] < 0)
                        {
                            if (alt) result = (byte)Math.Min(255, 127 + -fieldMap.gryodirs[controlNum]); else result = (byte)Math.Max(0, 127 - -fieldMap.gryodirs[controlNum]);
                        }
                        else result = falseVal;
                        break;
                    }
                    default: break;
                }
            }

            return result;
        }

        public static byte getXYAxisMapping(int device, DS4Controls control, DS4State cState, DS4StateExposed eState, Mouse tp, bool alt = false)
        {
            byte result = 0;
            byte trueVal = 0;
            byte falseVal = 127;

            if (alt)
                trueVal = 255;

            if (control >= DS4Controls.Square && control <= DS4Controls.Cross)
            {
                switch (control)
                {
                    case DS4Controls.Cross: result = (byte)(cState.Cross ? trueVal : falseVal); break;
                    case DS4Controls.Square: result = (byte)(cState.Square ? trueVal : falseVal); break;
                    case DS4Controls.Triangle: result = (byte)(cState.Triangle ? trueVal : falseVal); break;
                    case DS4Controls.Circle: result = (byte)(cState.Circle ? trueVal : falseVal); break;
                    default: break;
                }
            }
            else if (control >= DS4Controls.L1 && control <= DS4Controls.R3)
            {
                switch (control)
                {
                    case DS4Controls.L1: result = (byte)(cState.L1 ? trueVal : falseVal); break;
                    case DS4Controls.L2: if (alt) result = (byte)(127.5f + cState.L2 / 2f); else result = (byte)(127.5f - cState.L2 / 2f); break;
                    case DS4Controls.L3: result = (byte)(cState.L3 ? trueVal : falseVal); break;
                    case DS4Controls.R1: result = (byte)(cState.R1 ? trueVal : falseVal); break;
                    case DS4Controls.R2: if (alt) result = (byte)(127.5f + cState.R2 / 2f); else result = (byte)(127.5f - cState.R2 / 2f); break;
                    case DS4Controls.R3: result = (byte)(cState.R3 ? trueVal : falseVal); break;
                    default: break;
                }
            }
            else if (control >= DS4Controls.DpadUp && control <= DS4Controls.DpadLeft)
            {
                switch (control)
                {
                    case DS4Controls.DpadUp: result = (byte)(cState.DpadUp ? trueVal : falseVal); break;
                    case DS4Controls.DpadDown: result = (byte)(cState.DpadDown ? trueVal : falseVal); break;
                    case DS4Controls.DpadLeft: result = (byte)(cState.DpadLeft ? trueVal : falseVal); break;
                    case DS4Controls.DpadRight: result = (byte)(cState.DpadRight ? trueVal : falseVal); break;
                    default: break;
                }
            }
            else if (control >= DS4Controls.LXNeg && control <= DS4Controls.RYPos)
            {
                switch (control)
                {
                    case DS4Controls.LXNeg: if (!alt) result = cState.LX; else result = (byte)(255 - cState.LX); break;
                    case DS4Controls.LYNeg: if (!alt) result = cState.LY; else result = (byte)(255 - cState.LY); break;
                    case DS4Controls.RXNeg: if (!alt) result = cState.RX; else result = (byte)(255 - cState.RX); break;
                    case DS4Controls.RYNeg: if (!alt) result = cState.RY; else result = (byte)(255 - cState.RY); break;
                    case DS4Controls.LXPos: if (!alt) result = (byte)(255 - cState.LX); else result = cState.LX; break;
                    case DS4Controls.LYPos: if (!alt) result = (byte)(255 - cState.LY); else result = cState.LY; break;
                    case DS4Controls.RXPos: if (!alt) result = (byte)(255 - cState.RX); else result = cState.RX; break;
                    case DS4Controls.RYPos: if (!alt) result = (byte)(255 - cState.RY); else result = cState.RY; break;
                    default: break;
                }
            }
            else if (control >= DS4Controls.TouchLeft && control <= DS4Controls.TouchRight)
            {
                switch (control)
                {
                    case DS4Controls.TouchLeft: result = (byte)(tp != null && tp.leftDown ? trueVal : falseVal); break;
                    case DS4Controls.TouchRight: result = (byte)(tp != null && tp.rightDown ? trueVal : falseVal); break;
                    case DS4Controls.TouchMulti: result = (byte)(tp != null && tp.multiDown ? trueVal : falseVal); break;
                    case DS4Controls.TouchUpper: result = (byte)(tp != null && tp.upperDown ? trueVal : falseVal); break;
                    default: break;
                }
            }
            else if (control >= DS4Controls.SwipeLeft && control <= DS4Controls.SwipeDown)
            {
                switch (control)
                {
                    case DS4Controls.SwipeUp: if (alt) result = (byte)(tp != null ? 127.5f + tp.swipeUpB / 2f : 0); else result = (byte)(tp != null ? 127.5f - tp.swipeUpB / 2f : 0); break;
                    case DS4Controls.SwipeDown: if (alt) result = (byte)(tp != null ? 127.5f + tp.swipeDownB / 2f : 0); else result = (byte)(tp != null ? 127.5f - tp.swipeDownB / 2f : 0); break;
                    case DS4Controls.SwipeLeft: if (alt) result = (byte)(tp != null ? 127.5f + tp.swipeLeftB / 2f : 0); else result = (byte)(tp != null ? 127.5f - tp.swipeLeftB / 2f : 0); break;
                    case DS4Controls.SwipeRight: if (alt) result = (byte)(tp != null ? 127.5f + tp.swipeRightB / 2f : 0); else result = (byte)(tp != null ? 127.5f - tp.swipeRightB / 2f : 0); break;
                    default: break;
                }
            }
            else if (control >= DS4Controls.GyroXPos && control <= DS4Controls.GyroZNeg)
            {
                double SXD = getSXDeadzone(device);
                double SZD = getSZDeadzone(device);
                bool sOff = isUsingSAforMouse(device);

                switch (control)
                {
                    case DS4Controls.GyroXPos:
                    {
                        if (!sOff && -eState.AccelX > SXD * 10)
                        {
                            if (alt) result = (byte)Math.Min(255, 127 + SXSens[device] * -eState.AccelX); else result = (byte)Math.Max(0, 127 - SXSens[device] * -eState.AccelX);
                        }
                        else result = falseVal;
                        break;
                    }
                    case DS4Controls.GyroXNeg:
                    {
                        if (!sOff && -eState.AccelX < -SXD * 10)
                        {
                            if (alt) result = (byte)Math.Min(255, 127 + SXSens[device] * eState.AccelX); else result = (byte)Math.Max(0, 127 - SXSens[device] * eState.AccelX);
                        }
                        else result = falseVal;
                        break;
                    }
                    case DS4Controls.GyroZPos:
                    {
                        if (!sOff && eState.AccelZ > SZD * 10)
                        {
                            if (alt) result = (byte)Math.Min(255, 127 + SZSens[device] * eState.AccelZ); else result = (byte)Math.Max(0, 127 - SZSens[device] * eState.AccelZ);
                        }
                        else return falseVal;
                        break;
                    }
                    case DS4Controls.GyroZNeg:
                    {
                        if (!sOff && eState.AccelZ < -SZD * 10)
                        {
                            if (alt) result = (byte)Math.Min(255, 127 + SZSens[device] * -eState.AccelZ); else result = (byte)Math.Max(0, 127 - SZSens[device] * -eState.AccelZ);
                        }
                        else result = falseVal;
                        break;
                    }
                    default: break;
                }
            }
            else
            {
                switch (control)
                {
                    case DS4Controls.Share: result = (byte)(cState.Share ? trueVal : falseVal); break;
                    case DS4Controls.Options: result = (byte)(cState.Options ? trueVal : falseVal); break;
                    case DS4Controls.PS: result = (byte)(cState.PS ? trueVal : falseVal); break;
                    default: break;
                }
            }

            return result;
        }

        /* TODO: Possibly remove usage of this version of the method */
        //Returns false for any bool, 
        //if control is one of the xy axis returns 127
        //if its a trigger returns 0
        public static void resetToDefaultValue(DS4Controls control, DS4State cState)
        {
            if (control >= DS4Controls.Square && control <= DS4Controls.Cross)
            {
                switch (control)
                {
                    case DS4Controls.Cross: cState.Cross = false; break;
                    case DS4Controls.Square: cState.Square = false; break;
                    case DS4Controls.Triangle: cState.Triangle = false; break;
                    case DS4Controls.Circle: cState.Circle = false; break;
                    default: break;
                }
            }
            else if (control >= DS4Controls.L1 && control <= DS4Controls.R3)
            {
                switch (control)
                {
                    case DS4Controls.L1: cState.L1 = false; break;
                    case DS4Controls.L2: cState.L2 = 0; break;
                    case DS4Controls.L3: cState.L3 = false; break;
                    case DS4Controls.R1: cState.R1 = false; break;
                    case DS4Controls.R2: cState.R2 = 0; break;
                    case DS4Controls.R3: cState.R3 = false; break;
                    default: break;
                }
            }
            else if (control >= DS4Controls.DpadUp && control <= DS4Controls.DpadLeft)
            {
                switch (control)
                {
                    case DS4Controls.DpadUp: cState.DpadUp = false; break;
                    case DS4Controls.DpadDown: cState.DpadDown = false; break;
                    case DS4Controls.DpadLeft: cState.DpadLeft = false; break;
                    case DS4Controls.DpadRight: cState.DpadRight = false; break;
                    default: break;
                }
            }
            else if (control >= DS4Controls.LXNeg && control <= DS4Controls.RYPos)
            {
                switch (control)
                {
                    case DS4Controls.LXNeg: cState.LX = 127; break;
                    case DS4Controls.LYNeg: cState.LY = 127; break;
                    case DS4Controls.RXNeg: cState.RX = 127; break;
                    case DS4Controls.RYNeg: cState.RY = 127; break;
                    case DS4Controls.LXPos: cState.LX = 127; break;
                    case DS4Controls.LYPos: cState.LY = 127; break;
                    case DS4Controls.RXPos: cState.RX = 127; break;
                    case DS4Controls.RYPos: cState.RY = 127; break;
                    default: break;
                }
            }
            else
            {
                switch (control)
                {
                    case DS4Controls.Share: cState.Share = false; break;
                    case DS4Controls.Options: cState.Options = false; break;
                    case DS4Controls.PS: cState.PS = false; break;
                    default: break;
                }
            }
        }

        private static void resetToDefaultValue2(DS4Controls control, DS4State cState,
            DS4StateFieldMapping fieldMap)
        {
            int controlNum = (int)control;
            DS4StateFieldMapping.ControlType controlType = DS4StateFieldMapping.mappedType[controlNum];
            if (controlType == DS4StateFieldMapping.ControlType.Button)
            {
                fieldMap.buttons[controlNum] = false;
            }
            else if (controlType == DS4StateFieldMapping.ControlType.AxisDir)
            {
                fieldMap.axisdirs[controlNum] = 127;
                int controlRelation = (controlNum % 2 == 0 ? controlNum - 1 : controlNum + 1);
                fieldMap.axisdirs[controlRelation] = 127;
            }
            else if (controlType == DS4StateFieldMapping.ControlType.Trigger)
            {
                fieldMap.triggers[controlNum] = 0;
            }
            else if (controlType == DS4StateFieldMapping.ControlType.Touch)
            {
                fieldMap.buttons[controlNum] = false;
            }
        }
    }
}
