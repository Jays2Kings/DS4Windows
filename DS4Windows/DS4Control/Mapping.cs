using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Threading.Tasks;
using System.Windows.Forms;
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
                foreach (KeyPresses kp in keyPresses.Values)
                {
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
        public static SyntheticState[] deviceState = { new SyntheticState(), new SyntheticState(), new SyntheticState(), new SyntheticState() };

        // TODO When we disconnect, process a null/dead state to release any keys or buttons.
        public static DateTime oldnow = DateTime.UtcNow;
        private static bool pressagain = false;
        private static int wheel = 0, keyshelddown = 0;

        //mapcustom
        public static bool[] pressedonce = new bool[261], macrodone = new bool[34];
        static bool[] macroControl = new bool[25];

        //actions
        public static int[] fadetimer = { 0, 0, 0, 0 };
        public static int[] prevFadetimer = { 0, 0, 0, 0 };
        public static DS4Color[] lastColor = new DS4Color[4];
        public static List<ActionState> actionDone = new List<ActionState>();
        //public static List<bool>[] actionDone = { new List<bool>(), new List<bool>(), new List<bool>(), new List<bool>() };
        //public static bool[,] actionDone = new bool[4, 50];
        public static SpecialAction[] untriggeraction = new SpecialAction[4];
        public static DateTime[] nowAction = { DateTime.MinValue, DateTime.MinValue, DateTime.MinValue, DateTime.MinValue };
        public static DateTime[] oldnowAction = { DateTime.MinValue, DateTime.MinValue, DateTime.MinValue, DateTime.MinValue };
        public static int[] untriggerindex = { -1, -1, -1, -1 };
        public static DateTime[] oldnowKeyAct = { DateTime.MinValue, DateTime.MinValue, DateTime.MinValue, DateTime.MinValue };
        private static bool tappedOnce = false, firstTouch = false, secondtouchbegin = false;
        private static DateTime pastTime, firstTap, TimeofEnd;
        private static DS4Controls[] shiftTriggerMapping = { DS4Controls.None, DS4Controls.Cross, DS4Controls.Circle, DS4Controls.Square,
            DS4Controls.Triangle, DS4Controls.Options, DS4Controls.Share, DS4Controls.DpadUp, DS4Controls.DpadDown,
            DS4Controls.DpadLeft, DS4Controls.DpadRight, DS4Controls.PS, DS4Controls.L1, DS4Controls.R1, DS4Controls.L2,
            DS4Controls.R2, DS4Controls.L3, DS4Controls.R3, DS4Controls.TouchLeft, DS4Controls.TouchUpper, DS4Controls.TouchMulti,
            DS4Controls.TouchRight, DS4Controls.GyroZNeg, DS4Controls.GyroZPos, DS4Controls.GyroXPos, DS4Controls.GyroXNeg
        };
        private static int[] ds4ControlMapping = { 0, // DS4Control.None
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
            32  // DS4Controls.GyroZNeg
        };

        //special macros
        static bool altTabDone = true;
        static DateTime altTabNow = DateTime.UtcNow, oldAltTabNow = DateTime.UtcNow - TimeSpan.FromSeconds(1);

        //mouse
        public static int mcounter = 34;
        public static int mouseaccel = 0;
        public static int prevmouseaccel = 0;
        private static double horizontalRemainder = 0.0, verticalRemainder = 0.0;

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
                        InputMethods.MouseEvent(InputMethods.MOUSEEVENTF_WHEEL, 100);
                        oldnow = DateTime.UtcNow;
                        wheel = 100;
                    }
                    else if (globalState.currentClicks.wUpCount == 0 && globalState.previousClicks.wUpCount != 0)
                        wheel = 0;

                    if (globalState.currentClicks.wDownCount != 0 && globalState.previousClicks.wDownCount == 0)
                    {
                        InputMethods.MouseEvent(InputMethods.MOUSEEVENTF_WHEEL, -100);
                        oldnow = DateTime.UtcNow;
                        wheel = -100;
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
                foreach (KeyValuePair<UInt16, SyntheticState.KeyPresses> kvp in state.keyPresses)
                {
                    SyntheticState.KeyPresses gkp;
                    if (globalState.keyPresses.TryGetValue(kvp.Key, out gkp))
                    {
                        gkp.current.vkCount += kvp.Value.current.vkCount - kvp.Value.previous.vkCount;
                        gkp.current.scanCodeCount += kvp.Value.current.scanCodeCount - kvp.Value.previous.scanCodeCount;
                        gkp.current.repeatCount += kvp.Value.current.repeatCount - kvp.Value.previous.repeatCount;
                        gkp.current.toggle = kvp.Value.current.toggle;
                        gkp.current.toggleCount += kvp.Value.current.toggleCount - kvp.Value.previous.toggleCount;
                    }
                    else
                    {
                        gkp = new SyntheticState.KeyPresses();
                        gkp.current = kvp.Value.current;
                        globalState.keyPresses[kvp.Key] = gkp;
                    }
                    if (gkp.current.toggleCount != 0 && gkp.previous.toggleCount == 0 && gkp.current.toggle)
                    {
                        if (gkp.current.scanCodeCount != 0)
                            InputMethods.performSCKeyPress(kvp.Key);
                        else
                            InputMethods.performKeyPress(kvp.Key);
                    }
                    else if (gkp.current.toggleCount != 0 && gkp.previous.toggleCount == 0 && !gkp.current.toggle)
                    {
                        if (gkp.previous.scanCodeCount != 0) // use the last type of VK/SC
                            InputMethods.performSCKeyRelease(kvp.Key);
                        else
                         InputMethods.performKeyRelease(kvp.Key);
                    }
                    else if (gkp.current.vkCount + gkp.current.scanCodeCount != 0 && gkp.previous.vkCount + gkp.previous.scanCodeCount == 0)
                    {
                        if (gkp.current.scanCodeCount != 0)
                        {
                            oldnow = DateTime.UtcNow;
                            InputMethods.performSCKeyPress(kvp.Key);
                            pressagain = false;
                            keyshelddown = kvp.Key;
                        }
                        else
                        {
                            oldnow = DateTime.UtcNow;
                            InputMethods.performKeyPress(kvp.Key);
                            pressagain = false;
                            keyshelddown = kvp.Key;
                        }
                    }
                    else if (gkp.current.toggleCount != 0 || gkp.previous.toggleCount != 0 || gkp.current.repeatCount != 0 || // repeat or SC/VK transition
                        ((gkp.previous.scanCodeCount == 0) != (gkp.current.scanCodeCount == 0))) //repeat keystroke after 500ms
                    {
                        if (keyshelddown == kvp.Key)
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
                                    InputMethods.performSCKeyPress(kvp.Key);
                                }
                            }
                            else if (pressagain)
                            {
                                now = DateTime.UtcNow;
                                if (now >= oldnow + TimeSpan.FromMilliseconds(25) && pressagain)
                                {
                                    oldnow = now;
                                    InputMethods.performKeyPress(kvp.Key);
                                }
                            }
                        }
                    }
                    if ((gkp.current.toggleCount == 0 && gkp.previous.toggleCount == 0) && gkp.current.vkCount + gkp.current.scanCodeCount == 0 && gkp.previous.vkCount + gkp.previous.scanCodeCount != 0)
                    {
                        if (gkp.previous.scanCodeCount != 0) // use the last type of VK/SC
                        {
                            InputMethods.performSCKeyRelease(kvp.Key);
                            pressagain = false;
                        }
                        else
                        {
                            InputMethods.performKeyRelease(kvp.Key);
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
            }
        }

        public static int DS4ControltoInt(DS4Controls ctrl)
        {
            int result = 0;
            if (ctrl >= DS4Controls.None && ctrl <= DS4Controls.GyroZNeg)
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
        static double Clamp(double min, double value, double max)
        {
            if (value > max)
                return max;
            else if (value < min)
                return min;
            else
                return value;
        }

        public static DS4State SetCurveAndDeadzone(int device, DS4State cState)
        {
            DS4State dState = new DS4State(cState);
            int x;
            int y;
            int curve;
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
                    //btnLSTrack.Location = new Point((int)(dpix * curvex / 2.09 + lbLSTrack.Location.X), (int)(dpiy * curvey / 2.09 + lbLSTrack.Location.Y));
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
            if (lsDeadzone > 0 || lsAntiDead > 0)
            {
                double lsSquared = Math.Pow(cState.LX - 127.5f, 2) + Math.Pow(cState.LY - 127.5f, 2);
                double lsDeadzoneSquared = Math.Pow(lsDeadzone, 2);
                if (lsDeadzone > 0 && lsSquared <= lsDeadzoneSquared)
                {
                    dState.LX = 127;
                    dState.LY = 127;
                }
                else if ((lsDeadzone > 0 && lsSquared > lsDeadzoneSquared) || lsAntiDead > 0)
                {
                    double r = Math.Atan2(-(dState.LY - 127.5f), (dState.LX - 127.5f));
                    double maxXValue = dState.LX >= 127.5 ? 127.5 : -127.5;
                    double maxYValue = dState.LY >= 127.5 ? 127.5 : -127.5;

                    double tempLsXDead = 0.0, tempLsYDead = 0.0;
                    double tempOutputX = 0.0, tempOutputY = 0.0;
                    if (lsDeadzone > 0)
                    {
                        tempLsXDead = Math.Abs(Math.Cos(r)) * (lsDeadzone / 127.0) * maxXValue;
                        tempLsYDead = Math.Abs(Math.Sin(r)) * (lsDeadzone / 127.0) * maxYValue;

                        if (lsSquared > lsDeadzoneSquared)
                        {
                            tempOutputX = ((dState.LX - 127.5f - tempLsXDead) / (double)(maxXValue - tempLsXDead));
                            tempOutputY = ((dState.LY - 127.5f - tempLsYDead) / (double)(maxYValue - tempLsYDead));
                        }
                    }
                    else
                    {
                        tempOutputX = ((dState.LX - 127.5f) / (double)(maxXValue));
                        tempOutputY = ((dState.LY - 127.5f) / (double)(maxYValue));
                    }

                    double tempLsXAntiDeadPercent = 0.0, tempLsYAntiDeadPercent = 0.0;
                    if (lsAntiDead > 0)
                    {
                        tempLsXAntiDeadPercent = (lsAntiDead * 0.01) * Math.Abs(Math.Cos(r));
                        tempLsYAntiDeadPercent = (lsAntiDead * 0.01) * Math.Abs(Math.Sin(r));
                    }

                    if (tempOutputX > 0.0)
                    {
                        dState.LX = (byte)((((1.0 - tempLsXAntiDeadPercent) * tempOutputX + tempLsXAntiDeadPercent)) * maxXValue + 127.5f);
                    }
                    else
                    {
                        dState.LX = 127;
                    }

                    if (tempOutputY > 0.0)
                    {
                        dState.LY = (byte)((((1.0 - tempLsYAntiDeadPercent) * tempOutputY + tempLsYAntiDeadPercent)) * maxYValue + 127.5f);
                    }
                    else
                    {
                        dState.LY = 127;
                    }

                    //dState.LX = (byte)(Math.Cos(r) * (127.5f + LSDeadzone[device]) + 127.5f);
                    //dState.LY = (byte)(Math.Sin(r) * (127.5f + LSDeadzone[device]) + 127.5f);
                }
            }

            int rsDeadzone = getRSDeadzone(device);
            int rsAntiDead = getRSAntiDeadzone(device);
            if (rsDeadzone > 0 || rsAntiDead > 0)
            {
                double rsSquared = Math.Pow(cState.RX - 127.5f, 2) + Math.Pow(cState.RY - 127.5f, 2);
                double rsDeadzoneSquared = Math.Pow(rsDeadzone, 2);
                if (rsDeadzone > 0 && rsSquared <= rsDeadzoneSquared)
                {
                    dState.RX = 127;
                    dState.RY = 127;
                }
                else if ((rsDeadzone > 0 && rsSquared > rsDeadzoneSquared) || rsAntiDead > 0)
                {
                    double r = Math.Atan2(-(dState.RY - 127.5f), (dState.RX - 127.5f));
                    double maxXValue = dState.RX >= 127.5 ? 127.5 : -127.5;
                    double maxYValue = dState.RY >= 127.5 ? 127.5 : -127.5;

                    double tempRsXDead = 0.0, tempRsYDead = 0.0;
                    double tempOutputX = 0.0, tempOutputY = 0.0;
                    if (rsDeadzone > 0)
                    {
                        tempRsXDead = Math.Abs(Math.Cos(r)) * (rsDeadzone / 127.0) * maxXValue;
                        tempRsYDead = Math.Abs(Math.Sin(r)) * (rsDeadzone / 127.0) * maxYValue;

                        if (rsSquared > rsDeadzoneSquared)
                        {
                            tempOutputX = ((dState.RX - 127.5f - tempRsXDead) / (double)(maxXValue - tempRsXDead));
                            tempOutputY = ((dState.RY - 127.5f - tempRsYDead) / (double)(maxYValue - tempRsYDead));
                        }
                    }
                    else
                    {
                        tempOutputX = ((dState.RX - 127.5f) / (double)(maxXValue));
                        tempOutputY = ((dState.RY - 127.5f) / (double)(maxYValue));
                    }

                    double tempRsXAntiDeadPercent = 0.0, tempRsYAntiDeadPercent = 0.0;
                    if (rsAntiDead > 0)
                    {
                        tempRsXAntiDeadPercent = (rsAntiDead * 0.01) * Math.Abs(Math.Cos(r));
                        tempRsYAntiDeadPercent = (rsAntiDead * 0.01) * Math.Abs(Math.Sin(r));
                    }

                    if (tempOutputX > 0.0)
                    {
                        dState.RX = (byte)((((1.0 - tempRsXAntiDeadPercent) * tempOutputX + tempRsXAntiDeadPercent)) * maxXValue + 127.5f);
                    }
                    else
                    {
                        dState.RX = 127;
                    }

                    if (tempOutputY > 0.0)
                    {
                        dState.RY = (byte)((((1.0 - tempRsYAntiDeadPercent) * tempOutputY + tempRsYAntiDeadPercent)) * maxYValue + 127.5f);
                    }
                    else
                    {
                        dState.RY = 127;
                    }

                    //dState.RX = (byte)(((dState.RX - 127.5f - tempRsXDead) / (double)(maxXValue - tempRsXDead)) * maxXValue + 127.5f);
                    //dState.RY = (byte)(((dState.RY - 127.5f - tempRsYDead) / (double)(maxYValue - tempRsYDead)) * maxYValue + 127.5f);
                    //dState.RX = (byte)(Math.Cos(r) * (127.5f + RSDeadzone[device]) + 127.5f);
                    //dState.RY = (byte)(Math.Sin(r) * (127.5f + RSDeadzone[device]) + 127.5f);
                }
            }

            byte l2Deadzone = getL2Deadzone(device);
            int l2AntiDeadzone = getL2AntiDeadzone(device);
            if (l2Deadzone > 0 || l2AntiDeadzone > 0)
            {
                double tempL2Output = (cState.L2 / 255.0);
                double tempL2AntiDead = 0.0;
                if (l2Deadzone > 0)
                {
                    if (cState.L2 > l2Deadzone)
                    {
                        tempL2Output = ((dState.L2 - l2Deadzone) / (double)(255 - l2Deadzone));
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
                    dState.L2 = (byte)(((1.0 - tempL2AntiDead) * tempL2Output + tempL2AntiDead) * 255);
                }
                else
                {
                    dState.L2 = 0;
                }

                /*if (cState.L2 > l2Deadzone)
                {
                    dState.L2 = (byte)(((dState.L2 - l2Deadzone) / (double)(255 - l2Deadzone)) * 255);
                }
                else
                {
                    dState.L2 = 0;
                }
                */
            }

            byte r2Deadzone = getR2Deadzone(device);
            int r2AntiDeadzone = getR2AntiDeadzone(device);
            if (r2Deadzone > 0 || r2AntiDeadzone > 0)
            {
                double tempR2Output = (cState.R2 / 255.0);
                double tempR2AntiDead = 0.0;
                if (r2Deadzone > 0)
                {
                    if (cState.R2 > r2Deadzone)
                    {
                        tempR2Output = ((dState.R2 - r2Deadzone) / (double)(255 - r2Deadzone));
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
                    dState.R2 = (byte)(((1.0 - tempR2AntiDead) * tempR2Output + tempR2AntiDead) * 255);
                }
                else
                {
                    dState.R2 = 0;
                }
            }

            double lsSens = getLSSens(device);
            if (lsSens != 1.0)
            {
                dState.LX = (byte)Clamp(0, lsSens * (dState.LX - 127.5f) + 127.5f, 255);
                dState.LY = (byte)Clamp(0, lsSens * (dState.LY - 127.5f) + 127.5f, 255);
            }

            double rsSens = getRSSens(device);
            if (rsSens != 1.0)
            {
                dState.RX = (byte)Clamp(0, rsSens * (dState.RX - 127.5f) + 127.5f, 255);
                dState.RY = (byte)Clamp(0, rsSens * (dState.RY - 127.5f) + 127.5f, 255);
            }

            double l2Sens = getL2Sens(device);
            if (l2Sens != 1.0)
                dState.L2 = (byte)Clamp(0, l2Sens * dState.L2, 255);

            double r2Sens = getR2Sens(device);
            if (r2Sens != 1.0)
                dState.R2 = (byte)Clamp(0, r2Sens * dState.R2, 255);

            return dState;
        }

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
            int MouseDeltaX = 0;
            int MouseDeltaY = 0;
            
            SyntheticState deviceState = Mapping.deviceState[device];
            if (getProfileActionCount(device) > 0 || !string.IsNullOrEmpty(tempprofilename[device]))
                MapCustomAction(device, cState, MappedState, eState, tp, ctrl);
            if (ctrl.DS4Controllers[device] == null) return;

            cState.CopyTo(MappedState);

            Dictionary<DS4Controls, DS4Controls> tempControlDict = new Dictionary<DS4Controls, DS4Controls>();
            DS4Controls usingExtra = DS4Controls.None;
            List<DS4ControlSettings> tempSettingsList = getDS4CSettings(device);
            //foreach (DS4ControlSettings dcs in getDS4CSettings(device))
            for (int settingIndex = 0, arlen = tempSettingsList.Count; settingIndex < arlen; settingIndex++)
            {
                DS4ControlSettings dcs = tempSettingsList[settingIndex];
                object action = null;
                DS4ControlSettings.ActionType actionType = 0;
                DS4KeyType keyType = DS4KeyType.None;
                if (dcs.shiftAction != null && ShiftTrigger(dcs.shiftTrigger, device, cState, eState, tp))
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

                if (action != null)
                {
                    if (actionType == DS4ControlSettings.ActionType.Macro)
                    {
                        if (getBoolMapping(device, dcs.control, cState, eState, tp))
                        {
                            resetToDefaultValue(dcs.control, MappedState);
                            PlayMacro(device, macroControl, string.Join("/", (int[])action), dcs.control, keyType);
                        }
                        else if (!getBoolMapping(device, dcs.control, cState, eState, tp))
                        {
                            EndMacro(device, macroControl, string.Join("/", (int[])action), dcs.control);
                        }
                    }
                    else if (actionType == DS4ControlSettings.ActionType.Key)
                    {
                        ushort value = ushort.Parse(action.ToString());
                        if (getBoolMapping(device, dcs.control, cState, eState, tp))
                        {
                            resetToDefaultValue(dcs.control, MappedState);
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

                        X360Controls xboxControl = getX360ControlsByName(action.ToString());
                        if (xboxControl >= X360Controls.X && xboxControl <= X360Controls.A)
                        {
                            switch (xboxControl)
                            {
                                case X360Controls.A: tempControlDict.Add(DS4Controls.Cross, dcs.control); break;
                                case X360Controls.B: tempControlDict.Add(DS4Controls.Circle, dcs.control); break;
                                case X360Controls.X: tempControlDict.Add(DS4Controls.Square, dcs.control); break;
                                case X360Controls.Y: tempControlDict.Add(DS4Controls.Triangle, dcs.control); break;
                                default: break;
                            }
                        }
                        else if (xboxControl >= X360Controls.LB && xboxControl <= X360Controls.RS)
                        {
                            switch (xboxControl)
                            {
                                case X360Controls.LB: tempControlDict.Add(DS4Controls.L1, dcs.control); break;
                                case X360Controls.LT: tempControlDict.Add(DS4Controls.L2, dcs.control); break;
                                case X360Controls.LS: tempControlDict.Add(DS4Controls.L3, dcs.control); break;
                                case X360Controls.RB: tempControlDict.Add(DS4Controls.R1, dcs.control); break;
                                case X360Controls.RT: tempControlDict.Add(DS4Controls.R2, dcs.control); break;
                                case X360Controls.RS: tempControlDict.Add(DS4Controls.R3, dcs.control); break;
                                default: break;
                            }
                        }
                        else if (xboxControl >= X360Controls.DpadUp && xboxControl <= X360Controls.DpadLeft)
                        {
                            switch (xboxControl)
                            {
                                case X360Controls.DpadUp: tempControlDict.Add(DS4Controls.DpadUp, dcs.control); break;
                                case X360Controls.DpadDown: tempControlDict.Add(DS4Controls.DpadDown, dcs.control); break;
                                case X360Controls.DpadLeft: tempControlDict.Add(DS4Controls.DpadLeft, dcs.control); break;
                                case X360Controls.DpadRight: tempControlDict.Add(DS4Controls.DpadRight, dcs.control); break;
                                default: break;
                            }
                        }
                        else if (xboxControl >= X360Controls.LXNeg && xboxControl <= X360Controls.RYPos)
                        {
                            switch (xboxControl)
                            {
                                case X360Controls.LXNeg: tempControlDict.Add(DS4Controls.LXNeg, dcs.control); break;
                                case X360Controls.LYNeg: tempControlDict.Add(DS4Controls.LYNeg, dcs.control); break;
                                case X360Controls.RXNeg: tempControlDict.Add(DS4Controls.RXNeg, dcs.control); break;
                                case X360Controls.RYNeg: tempControlDict.Add(DS4Controls.RYNeg, dcs.control); break;
                                case X360Controls.LXPos: tempControlDict.Add(DS4Controls.LXPos, dcs.control); break;
                                case X360Controls.LYPos: tempControlDict.Add(DS4Controls.LYPos, dcs.control); break;
                                case X360Controls.RXPos: tempControlDict.Add(DS4Controls.RXPos, dcs.control); break;
                                case X360Controls.RYPos: tempControlDict.Add(DS4Controls.RYPos, dcs.control); break;
                                default: break;
                            }
                        }
                        else if (xboxControl >= X360Controls.LeftMouse && xboxControl <= X360Controls.WDOWN)
                        {
                            switch (xboxControl)
                            {
                                case X360Controls.LeftMouse:
                                    keyvalue = 256;
                                    if (getBoolMapping(device, dcs.control, cState, eState, tp))
                                        deviceState.currentClicks.leftCount++;
                                    break;
                                case X360Controls.RightMouse:
                                    keyvalue = 257;
                                    if (getBoolMapping(device, dcs.control, cState, eState, tp))
                                        deviceState.currentClicks.rightCount++;
                                    break;
                                case X360Controls.MiddleMouse:
                                    keyvalue = 258;
                                    if (getBoolMapping(device, dcs.control, cState, eState, tp))
                                        deviceState.currentClicks.middleCount++;
                                    break;
                                case X360Controls.FourthMouse:
                                    keyvalue = 259;
                                    if (getBoolMapping(device, dcs.control, cState, eState, tp))
                                        deviceState.currentClicks.fourthCount++;
                                    break;
                                case X360Controls.FifthMouse:
                                    keyvalue = 260;
                                    if (getBoolMapping(device, dcs.control, cState, eState, tp))
                                        deviceState.currentClicks.fifthCount++;
                                    break;
                                case X360Controls.WUP:
                                    if (getBoolMapping(device, dcs.control, cState, eState, tp))
                                        if (isAnalog)
                                            getMouseWheelMapping(device, dcs.control, cState, eState, tp, false);
                                        else
                                            deviceState.currentClicks.wUpCount++;
                                    break;
                                case X360Controls.WDOWN:
                                    if (getBoolMapping(device, dcs.control, cState, eState, tp))
                                        if (isAnalog)
                                            getMouseWheelMapping(device, dcs.control, cState, eState, tp, true);
                                        else
                                            deviceState.currentClicks.wDownCount++;
                                    break;
                                default: break;
                            }
                        }
                        else if (xboxControl >= X360Controls.MouseUp && xboxControl <= X360Controls.MouseRight)
                        {
                            switch (xboxControl)
                            {
                                case X360Controls.MouseUp:
                                    if (MouseDeltaY == 0)
                                    {
                                        MouseDeltaY = getMouseMapping(device, dcs.control, cState, eState, 0);
                                        MouseDeltaY = -Math.Abs((MouseDeltaY == -2147483648 ? 0 : MouseDeltaY));
                                    }
                                    break;
                                case X360Controls.MouseDown:
                                    if (MouseDeltaY == 0)
                                    {
                                        MouseDeltaY = getMouseMapping(device, dcs.control, cState, eState, 1);
                                        MouseDeltaY = Math.Abs((MouseDeltaY == -2147483648 ? 0 : MouseDeltaY));
                                    }
                                    break;
                                case X360Controls.MouseLeft:
                                    if (MouseDeltaX == 0)
                                    {
                                        MouseDeltaX = getMouseMapping(device, dcs.control, cState, eState, 2);
                                        MouseDeltaX = -Math.Abs((MouseDeltaX == -2147483648 ? 0 : MouseDeltaX));
                                    }
                                    break;
                                case X360Controls.MouseRight:
                                    if (MouseDeltaX == 0)
                                    {
                                        MouseDeltaX = getMouseMapping(device, dcs.control, cState, eState, 3);
                                        MouseDeltaX = Math.Abs((MouseDeltaX == -2147483648 ? 0 : MouseDeltaX));
                                    }
                                    break;

                                default: break;
                            }
                        }
                        else
                        {
                            switch (xboxControl)
                            {
                                case X360Controls.Start: tempControlDict.Add(DS4Controls.Options, dcs.control); break;
                                case X360Controls.Guide: tempControlDict.Add(DS4Controls.PS, dcs.control); break;
                                case X360Controls.Back: tempControlDict.Add(DS4Controls.Share, dcs.control); break;
                                default: break;
                            }
                        }

                        if (keyType.HasFlag(DS4KeyType.Toggle))
                        {
                            if (getBoolMapping(device, dcs.control, cState, eState, tp))
                            {
                                resetToDefaultValue(dcs.control, MappedState);
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

                        resetToDefaultValue(dcs.control, MappedState); // erase default mappings for things that are remapped                       
                    }
                }

                if (usingExtra == DS4Controls.None || usingExtra == dcs.control)
                {
                    bool shiftE = !string.IsNullOrEmpty(dcs.shiftExtras) && dcs.shiftExtras != "0,0,0,0,0,0,0,0" && ShiftTrigger(dcs.shiftTrigger, device, cState, eState, tp);
                    bool regE = !string.IsNullOrEmpty(dcs.extras) && dcs.extras != "0,0,0,0,0,0,0,0";
                    if ((regE || shiftE) && getBoolMapping(device, dcs.control, cState, eState, tp))
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
                        ButtonMouseSensitivity[device] = oldmouse[device];
                        oldmouse[device] = -1;
                        ctrl.setRumble(0, 0, device);
                        held[device] = false;
                        usingExtra = DS4Controls.None;
                    }
                }
            }            

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

            if (IfAxisIsNotModified(device, ShiftTrigger(GetDS4STrigger(device, DS4Controls.LXNeg), device, cState, eState, tp), DS4Controls.LXNeg))
                tempControlDict.Add(DS4Controls.LXNeg, DS4Controls.LXNeg);

            if (IfAxisIsNotModified(device, ShiftTrigger(GetDS4STrigger(device, DS4Controls.LXPos), device, cState, eState, tp), DS4Controls.LXPos))
                tempControlDict.Add(DS4Controls.LXPos, DS4Controls.LXPos);

            if (IfAxisIsNotModified(device, ShiftTrigger(GetDS4STrigger(device, DS4Controls.LYNeg), device, cState, eState, tp), DS4Controls.LYNeg))
                tempControlDict.Add(DS4Controls.LYNeg, DS4Controls.LYNeg);

            if (IfAxisIsNotModified(device, ShiftTrigger(GetDS4STrigger(device, DS4Controls.LYPos), device, cState, eState, tp), DS4Controls.LYPos))
                tempControlDict.Add(DS4Controls.LYPos, DS4Controls.LYPos);

            if (IfAxisIsNotModified(device, ShiftTrigger(GetDS4STrigger(device, DS4Controls.RXNeg), device, cState, eState, tp), DS4Controls.RXNeg))
                tempControlDict.Add(DS4Controls.RXNeg, DS4Controls.RXNeg);

            if (IfAxisIsNotModified(device, ShiftTrigger(GetDS4STrigger(device, DS4Controls.RXPos), device, cState, eState, tp), DS4Controls.RXPos))
                tempControlDict.Add(DS4Controls.RXPos, DS4Controls.RXPos);

            if (IfAxisIsNotModified(device, ShiftTrigger(GetDS4STrigger(device, DS4Controls.RYNeg), device, cState, eState, tp), DS4Controls.RYNeg))
                tempControlDict.Add(DS4Controls.RYNeg, DS4Controls.RYNeg);

            if (IfAxisIsNotModified(device, ShiftTrigger(GetDS4STrigger(device, DS4Controls.RYPos), device, cState, eState, tp), DS4Controls.RYPos))
                tempControlDict.Add(DS4Controls.RYPos, DS4Controls.RYPos);

            foreach (KeyValuePair<DS4Controls, DS4Controls> entry in tempControlDict)
            {
                DS4Controls key = entry.Key;
                DS4Controls dc = entry.Value;
                if (getBoolMapping(device, dc, cState, eState, tp))
                {
                    if (key >= DS4Controls.Square && key <= DS4Controls.Cross)
                    {
                        switch (key)
                        {
                            case DS4Controls.Cross: MappedState.Cross = true; break;
                            case DS4Controls.Circle: MappedState.Circle = true; break;
                            case DS4Controls.Square: MappedState.Square = true; break;
                            case DS4Controls.Triangle: MappedState.Triangle = true; break;
                            default: break;
                        }
                    }
                    else if (key >= DS4Controls.L1 && key <= DS4Controls.R3)
                    {
                        switch (key)
                        {
                            case DS4Controls.L1: MappedState.L1 = true; break;
                            case DS4Controls.L2: MappedState.L2 = getByteMapping(device, dc, cState, eState, tp); break;
                            case DS4Controls.L3: MappedState.L3 = true; break;
                            case DS4Controls.R1: MappedState.R1 = true; break;
                            case DS4Controls.R2: MappedState.R2 = getByteMapping(device, dc, cState, eState, tp); break;
                            case DS4Controls.R3: MappedState.R3 = true; break;
                            default: break;
                        }
                    }
                    else if (key >= DS4Controls.DpadUp && key <= DS4Controls.DpadLeft)
                    {
                        switch (key)
                        {
                            case DS4Controls.DpadUp: MappedState.DpadUp = true; break;
                            case DS4Controls.DpadRight: MappedState.DpadRight = true; break;
                            case DS4Controls.DpadLeft: MappedState.DpadLeft = true; break;
                            case DS4Controls.DpadDown: MappedState.DpadDown = true; break;
                            default: break;
                        }
                    }
                    else if (key >= DS4Controls.LXNeg && key <= DS4Controls.RYPos)
                    {
                        switch (key)
                        {
                            case DS4Controls.LXNeg:
                            case DS4Controls.LXPos:
                            {
                                if (Math.Abs(MappedState.LX - 127) < 10)
                                {
                                    if (key == DS4Controls.LXNeg)
                                    {
                                        byte axisMapping = getXYAxisMapping(device, dc, cState, eState, tp, true);
                                        if (Math.Abs(127 - axisMapping) > 5)
                                            MappedState.LX = axisMapping;
                                    }
                                    else
                                    {
                                        byte axisMapping = getXYAxisMapping(device, dc, cState, eState, tp);
                                        if (Math.Abs(127 - axisMapping) > 5)
                                            MappedState.LX = axisMapping;
                                    }
                                }

                                break;
                            }
                            case DS4Controls.LYNeg:
                            case DS4Controls.LYPos:
                            {
                                if (Math.Abs(MappedState.LY - 127) < 10)
                                {
                                    if (key == DS4Controls.LYNeg)
                                    {
                                        byte axisMapping = getXYAxisMapping(device, dc, cState, eState, tp);
                                        if (Math.Abs(127 - axisMapping) > 5)
                                            MappedState.LY = axisMapping;
                                    }
                                    else
                                    {
                                        byte axisMapping = getXYAxisMapping(device, dc, cState, eState, tp, true);
                                        if (Math.Abs(127 - axisMapping) > 5)
                                            MappedState.LY = axisMapping;
                                    }
                                }

                                break;
                            }
                            case DS4Controls.RXNeg:
                            case DS4Controls.RXPos:
                            {
                                if (Math.Abs(MappedState.RX - 127) < 10)
                                {
                                    if (key == DS4Controls.RXNeg)
                                    {
                                        byte axisMapping = getXYAxisMapping(device, dc, cState, eState, tp);
                                        if (Math.Abs(127 - axisMapping) > 5)
                                            MappedState.RX = axisMapping;
                                    }
                                    else
                                    {
                                        byte axisMapping = getXYAxisMapping(device, dc, cState, eState, tp, true);
                                        if (Math.Abs(127 - axisMapping) > 5)
                                            MappedState.RX = axisMapping;
                                    }
                                }

                                break;
                            }
                            case DS4Controls.RYNeg:
                            case DS4Controls.RYPos:
                            {
                                if (Math.Abs(MappedState.RY - 127) < 10)
                                {
                                    if (key == DS4Controls.RYNeg)
                                    {
                                        byte axisMapping = getXYAxisMapping(device, dc, cState, eState, tp);
                                        if (Math.Abs(127 - axisMapping) > 5)
                                            MappedState.RY = axisMapping;
                                    }
                                    else
                                    {
                                        byte axisMapping = getXYAxisMapping(device, dc, cState, eState, tp, true);
                                        if (Math.Abs(127 - axisMapping) > 5)
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
                        switch (key)
                        {
                            case DS4Controls.Options: MappedState.Options = true; break;
                            case DS4Controls.Share: MappedState.Share = true; break;
                            case DS4Controls.PS: MappedState.PS = true; break;
                            default: break;
                        }
                    }
                }
            }

            if (MouseDeltaX != 0 || MouseDeltaY != 0)
            {
                InputMethods.MoveCursorBy(MouseDeltaX, MouseDeltaY);
            }
        }

        private static bool IfAxisIsNotModified(int device, bool shift, DS4Controls dc)
        {
            return shift ? false : GetDS4Action(device, dc, false) == null;
        }
        public static async void MapCustomAction(int device, DS4State cState, DS4State MappedState,
            DS4StateExposed eState, Mouse tp, ControlService ctrl)
        {
            /* TODO: This method is slow sauce. Find ways to speed up action execution */
            try
            {
                int actionDoneCount = actionDone.Count;
                int totalActionCount = GetActions().Count;
                List<string> profileActions = getProfileActions(device);
                foreach (string actionname in profileActions)
                {
                    //DS4KeyType keyType = getShiftCustomKeyType(device, customKey.Key);
                    //SpecialAction action = GetAction(actionname);
                    //int index = GetActionIndexOf(actionname);
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
                                if (!getBoolMapping(device, dc, cState, eState, tp))
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
                                if (!getBoolMapping(device, dc, cState, eState, tp))
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
                                if (!getBoolMapping(device, dc, cState, eState, tp))
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
                                if (!getBoolMapping(device, dc, cState, eState, tp))
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
                                if (!getBoolMapping(device, dc, cState, eState, tp))
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
                                        resetToDefaultValue(dc, MappedState);
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
                                if (!d.isCharging())
                                {
                                    ConnectionType deviceConn = d.ConnectionType;
                                    if (deviceConn == ConnectionType.BT)
                                    {
                                        d.DisconnectBT();
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

                                /*if (getCustomButton(device, action.trigger[0]) != X360Controls.Unbound)
                                    getCustomButtons(device)[action.trigger[0]] = X360Controls.Unbound;
                                if (getCustomMacro(device, action.trigger[0]) != "0")
                                    getCustomMacros(device).Remove(action.trigger[0]);
                                if (getCustomKey(device, action.trigger[0]) != 0)
                                    getCustomMacros(device).Remove(action.trigger[0]);*/
                                string[] dets = action.details.Split(',');
                                DS4Device d = ctrl.DS4Controllers[device];
                                //cus
                                if (getBoolMapping(device, action.trigger[0], cState, eState, tp) && !getBoolMapping(device, action.trigger[0], d.getPreviousState(), eState, tp))
                                {//pressed down
                                    pastTime = DateTime.UtcNow;
                                    if (DateTime.UtcNow <= (firstTap + TimeSpan.FromMilliseconds(150)))
                                    {
                                        tappedOnce = false;
                                        secondtouchbegin = true;
                                    }
                                    else
                                        firstTouch = true;
                                }
                                else if (!getBoolMapping(device, action.trigger[0], cState, eState, tp) && getBoolMapping(device, action.trigger[0], d.getPreviousState(), eState, tp))
                                {//released
                                    if (secondtouchbegin)
                                    {
                                        firstTouch = false;
                                        secondtouchbegin = false;
                                    }
                                    else if (firstTouch)
                                    {
                                        firstTouch = false;
                                        if (DateTime.UtcNow <= (pastTime + TimeSpan.FromMilliseconds(200)) && !tappedOnce)
                                        {
                                            tappedOnce = true;
                                            firstTap = DateTime.UtcNow;
                                            TimeofEnd = DateTime.UtcNow;
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
                                    if ((DateTime.UtcNow - TimeofEnd) > TimeSpan.FromMilliseconds(150))
                                    {
                                        if (macro != "")
                                            PlayMacro(device, macroControl, macro, DS4Controls.None, DS4KeyType.None);
                                        tappedOnce = false;
                                    }
                                    //if it fails the method resets, and tries again with a new tester value (gives tap a delay so tap and hold can work)
                                }
                                else if (firstTouch && (DateTime.UtcNow - pastTime) > TimeSpan.FromMilliseconds(1000)) //helddown
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
                foreach (DS4Controls dc in action.uTrigger)
                {
                    if (!getBoolMapping(device, dc, cState, eState, tp))
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
                            else if (i == 261) macroControl[0] = true;
                            else if (i == 262) macroControl[1] = true;
                            else if (i == 263) macroControl[2] = true;
                            else if (i == 264) macroControl[3] = true;
                            else if (i == 265) macroControl[4] = true;
                            else if (i == 266) macroControl[5] = true;
                            else if (i == 267) macroControl[6] = true;
                            else if (i == 268) macroControl[7] = true;
                            else if (i == 269) macroControl[8] = true;
                            else if (i == 270) macroControl[9] = true;
                            else if (i == 271) macroControl[10] = true;
                            else if (i == 272) macroControl[11] = true;
                            else if (i == 273) macroControl[12] = true;
                            else if (i == 274) macroControl[13] = true;
                            else if (i == 275) macroControl[14] = true;
                            else if (i == 276) macroControl[15] = true;
                            else if (i == 277) macroControl[16] = true;
                            else if (i == 278) macroControl[17] = true;
                            else if (i == 279) macroControl[18] = true;
                            else if (i == 280) macroControl[19] = true;
                            else if (i == 281) macroControl[20] = true;
                            else if (i == 282) macroControl[21] = true;
                            else if (i == 283) macroControl[22] = true;
                            else if (i == 284) macroControl[23] = true;
                            else if (i == 285) macroControl[24] = true;
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
                            else if (i == 261) macroControl[0] = false;
                            else if (i == 262) macroControl[1] = false;
                            else if (i == 263) macroControl[2] = false;
                            else if (i == 264) macroControl[3] = false;
                            else if (i == 265) macroControl[4] = false;
                            else if (i == 266) macroControl[5] = false;
                            else if (i == 267) macroControl[6] = false;
                            else if (i == 268) macroControl[7] = false;
                            else if (i == 269) macroControl[8] = false;
                            else if (i == 270) macroControl[9] = false;
                            else if (i == 271) macroControl[10] = false;
                            else if (i == 272) macroControl[11] = false;
                            else if (i == 273) macroControl[12] = false;
                            else if (i == 274) macroControl[13] = false;
                            else if (i == 275) macroControl[14] = false;
                            else if (i == 276) macroControl[15] = false;
                            else if (i == 277) macroControl[16] = false;
                            else if (i == 278) macroControl[17] = false;
                            else if (i == 279) macroControl[18] = false;
                            else if (i == 280) macroControl[19] = false;
                            else if (i == 281) macroControl[20] = false;
                            else if (i == 282) macroControl[21] = false;
                            else if (i == 283) macroControl[22] = false;
                            else if (i == 284) macroControl[23] = false;
                            else if (i == 285) macroControl[24] = false;
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
                            if (i == 256) InputMethods.MouseEvent(InputMethods.MOUSEEVENTF_LEFTUP); //anything above 255 is not a keyvalue
                            else if (i == 257) InputMethods.MouseEvent(InputMethods.MOUSEEVENTF_RIGHTUP);
                            else if (i == 258) InputMethods.MouseEvent(InputMethods.MOUSEEVENTF_MIDDLEUP);
                            else if (i == 259) InputMethods.MouseEvent(InputMethods.MOUSEEVENTF_XBUTTONUP, 1);
                            else if (i == 260) InputMethods.MouseEvent(InputMethods.MOUSEEVENTF_XBUTTONUP, 2);
                            else if (i == 261) macroControl[0] = false;
                            else if (i == 262) macroControl[1] = false;
                            else if (i == 263) macroControl[2] = false;
                            else if (i == 264) macroControl[3] = false;
                            else if (i == 265) macroControl[4] = false;
                            else if (i == 266) macroControl[5] = false;
                            else if (i == 267) macroControl[6] = false;
                            else if (i == 268) macroControl[7] = false;
                            else if (i == 269) macroControl[8] = false;
                            else if (i == 270) macroControl[9] = false;
                            else if (i == 271) macroControl[10] = false;
                            else if (i == 272) macroControl[11] = false;
                            else if (i == 273) macroControl[12] = false;
                            else if (i == 274) macroControl[13] = false;
                            else if (i == 275) macroControl[14] = false;
                            else if (i == 276) macroControl[15] = false;
                            else if (i == 277) macroControl[16] = false;
                            else if (i == 278) macroControl[17] = false;
                            else if (i == 279) macroControl[18] = false;
                            else if (i == 280) macroControl[19] = false;
                            else if (i == 281) macroControl[20] = false;
                            else if (i == 282) macroControl[21] = false;
                            else if (i == 283) macroControl[22] = false;
                            else if (i == 284) macroControl[23] = false;
                            else if (i == 285) macroControl[24] = false;
                            else if (keyType.HasFlag(DS4KeyType.ScanCode))
                                InputMethods.performSCKeyRelease((ushort)i);
                            else
                                InputMethods.performKeyRelease((ushort)i);
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

        private static void getMouseWheelMapping(int device, DS4Controls control, DS4State cState, DS4StateExposed eState, Mouse tp, bool down)
        {
            DateTime now = DateTime.UtcNow;
            if (now >= oldnow + TimeSpan.FromMilliseconds(10) && !pressagain)
            {
                oldnow = now;
                InputMethods.MouseWheel((int)(getByteMapping(device, control, cState, eState, tp) / 51f * (down ? -1 : 1)), 0);
            }
        }

        private static int getMouseMapping(int device, DS4Controls control, DS4State cState, DS4StateExposed eState, int mnum)
        {
            int controlnum = DS4ControltoInt(control);
            double SXD = SXDeadzone[device];
            double SZD = SZDeadzone[device];
            int deadzoneL = 3;
            int deadzoneR = 3;
            if (LSDeadzone[device] >= 3)
                deadzoneL = 0;
            if (RSDeadzone[device] >= 3)
                deadzoneR = 0;
            double value = 0;
            int speed = ButtonMouseSensitivity[device] + 15;
            double root = 1.002;
            double divide = 10000d;
            //DateTime now = mousenow[mnum];
            switch (control)
            {
                case DS4Controls.LXNeg:
                    if (cState.LX - 127.5f < -deadzoneL)
                        value = -(cState.LX - 127.5f) / 2550d * speed;
                    break;
                case DS4Controls.LXPos:
                    if (cState.LX - 127.5f > deadzoneL)
                        value = (cState.LX - 127.5f) / 2550d * speed;
                    break;
                case DS4Controls.RXNeg:
                    if (cState.RX - 127.5f < -deadzoneR)
                        value = -(cState.RX - 127.5f) / 2550d * speed;
                    break;
                case DS4Controls.RXPos:
                    if (cState.RX - 127.5f > deadzoneR)
                        value = (cState.RX - 127.5f) / 2550d * speed;
                    break;
                case DS4Controls.LYNeg:
                    if (cState.LY - 127.5f < -deadzoneL)
                        value = -(cState.LY - 127.5f) / 2550d * speed;
                    break;
                case DS4Controls.LYPos:
                    if (cState.LY - 127.5f > deadzoneL)
                        value = (cState.LY - 127.5f) / 2550d * speed;
                    break;
                case DS4Controls.RYNeg:
                    if (cState.RY - 127.5f < -deadzoneR)
                        value = -(cState.RY - 127.5f) / 2550d * speed;
                    break;
                case DS4Controls.RYPos:
                    if (cState.RY - 127.5f > deadzoneR)
                        value = (cState.RY - 127.5f) / 2550d * speed;
                    break;
                case DS4Controls.Share: value = (cState.Share ? Math.Pow(root + speed / divide, 100) - 1 : 0); break;
                case DS4Controls.Options: value = (cState.Options ? Math.Pow(root + speed / divide, 100) - 1 : 0); break;
                case DS4Controls.L1: value = (cState.L1 ? Math.Pow(root + speed / divide, 100) - 1 : 0); break;
                case DS4Controls.R1: value = (cState.R1 ? Math.Pow(root + speed / divide, 100) - 1 : 0); break;
                case DS4Controls.L3: value = (cState.L3 ? Math.Pow(root + speed / divide, 100) - 1 : 0); break;
                case DS4Controls.R3: value = (cState.R3 ? Math.Pow(root + speed / divide, 100) - 1 : 0); break;
                case DS4Controls.DpadUp: value = (cState.DpadUp ? Math.Pow(root + speed / divide, 100) - 1 : 0); break;
                case DS4Controls.DpadDown: value = (cState.DpadDown ? Math.Pow(root + speed / divide, 100) - 1 : 0); break;
                case DS4Controls.DpadLeft: value = (cState.DpadLeft ? Math.Pow(root + speed / divide, 100) - 1 : 0); break;
                case DS4Controls.DpadRight: value = (cState.DpadRight ? Math.Pow(root + speed / divide, 100) - 1 : 0); break;
                case DS4Controls.PS: value = (cState.PS ? Math.Pow(root + speed / divide, 100) - 1 : 0); break;
                case DS4Controls.Cross: value = (cState.Cross ? Math.Pow(root + speed / divide, 100) - 1 : 0); break;
                case DS4Controls.Square: value = (cState.Square ? Math.Pow(root + speed / divide, 100) - 1 : 0); break;
                case DS4Controls.Triangle: value = (cState.Triangle ? Math.Pow(root + speed / divide, 100) - 1 : 0); break;
                case DS4Controls.Circle: value = (cState.Circle ? Math.Pow(root + speed / divide, 100) - 1 : 0); break;
                case DS4Controls.L2: value = Math.Pow(root + speed / divide, cState.L2 / 2d) - 1; break;
                case DS4Controls.R2: value = Math.Pow(root + speed / divide, cState.R2 / 2d) - 1; break;
                case DS4Controls.GyroXPos: return (byte)(eState.GyroX > SXD * 10 ?
                    Math.Pow(root + speed / divide, eState.GyroX) : 0);
                case DS4Controls.GyroXNeg: return (byte)(eState.GyroX < -SXD * 10 ?
                    Math.Pow(root + speed / divide, -eState.GyroX) : 0);
                case DS4Controls.GyroZPos: return (byte)(eState.GyroZ > SZD * 10 ?
                    Math.Pow(root + speed / divide, eState.GyroZ) : 0);
                case DS4Controls.GyroZNeg: return (byte)(eState.GyroZ < -SZD * 10 ?
                    Math.Pow(root + speed / divide, -eState.GyroZ) : 0);
            }
            bool LXChanged = (Math.Abs(127 - cState.LX) < deadzoneL);
            bool LYChanged = (Math.Abs(127 - cState.LY) < deadzoneL);
            bool RXChanged = (Math.Abs(127 - cState.RX) < deadzoneR);
            bool RYChanged = (Math.Abs(127 - cState.RY) < deadzoneR);
            bool contains = (control.ToString().Contains("LX") ||
                control.ToString().Contains("LY") ||
                control.ToString().Contains("RX") ||
                    control.ToString().Contains("RY"));
            if (MouseAccel[device])
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
                value *= 1 + (double)Math.Min(20000, (mouseaccel)) / 10000d;
                prevmouseaccel = mouseaccel;
            }
            int intValue;
            if (mnum > 1)
            {
                if ((value > 0.0 && horizontalRemainder > 0.0) || (value < 0.0 && horizontalRemainder < 0.0))
                    value += horizontalRemainder;
                intValue = (int)value;
                horizontalRemainder = value - intValue;
            }
            else
            {
                if ((value > 0.0 && verticalRemainder > 0.0) || (value < 0.0 && verticalRemainder < 0.0))
                    value += verticalRemainder;
                intValue = (int)value;
                verticalRemainder = value - intValue;
            }
            return intValue;
        }

        public static bool compare(byte b1, byte b2)
        {
            if (Math.Abs(b1 - b2) > 10)
            {
                return false;
            }
            return true;
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
                double SXD = SXDeadzone[device];
                double SZD = SZDeadzone[device];
                bool sOff = UseSAforMouse[device];

                switch (control)
                {
                    case DS4Controls.GyroXPos: result = (byte)(!sOff && SXSens[device] * eState.GyroX > SXD * 10 ? Math.Min(255, SXSens[device] * eState.GyroX * 2) : 0); break;
                    case DS4Controls.GyroXNeg: result = (byte)(!sOff && SXSens[device] * eState.GyroX < -SXD * 10 ? Math.Min(255, SXSens[device] * -eState.GyroX * 2) : 0); break;
                    case DS4Controls.GyroZPos: result = (byte)(!sOff && SZSens[device] * eState.GyroZ > SZD * 10 ? Math.Min(255, SZSens[device] * eState.GyroZ * 2) : 0); break;
                    case DS4Controls.GyroZNeg: result = (byte)(!sOff && SZSens[device] * eState.GyroZ < -SZD * 10 ? Math.Min(255, SZSens[device] * -eState.GyroZ * 2) : 0); break;
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
                bool sOff = UseSAforMouse[device];

                switch (control)
                {
                    case DS4Controls.GyroXPos: result = !sOff ? SXSens[device] * eState.GyroX > 67 : false; break;
                    case DS4Controls.GyroXNeg: result = !sOff ? SXSens[device] * eState.GyroX < -67 : false; break;
                    case DS4Controls.GyroZPos: result = !sOff ? SZSens[device] * eState.GyroZ > 67 : false; break;
                    case DS4Controls.GyroZNeg: result = !sOff ? SZSens[device] * eState.GyroZ < -67 : false; break;
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
                double SXD = SXDeadzone[device];
                double SZD = SZDeadzone[device];
                bool sOff = UseSAforMouse[device];

                switch (control)
                {
                    case DS4Controls.GyroXPos:
                    {
                        if (!sOff && eState.GyroX > SXD * 10)
                        {
                            if (alt) result = (byte)Math.Min(255, 127 + SXSens[device] * eState.GyroX); else result = (byte)Math.Max(0, 127 - SXSens[device] * eState.GyroX);
                        }
                        else result = falseVal;
                        break;
                    }
                    case DS4Controls.GyroXNeg:
                    {
                        if (!sOff && eState.GyroX < -SXD * 10)
                        {
                            if (alt) result = (byte)Math.Min(255, 127 + SXSens[device] * -eState.GyroX); else result = (byte)Math.Max(0, 127 - SXSens[device] * -eState.GyroX);
                        }
                        else result = falseVal;
                        break;
                    }
                    case DS4Controls.GyroZPos:
                    {
                        if (!sOff && eState.GyroZ > SZD * 10)
                        {
                            if (alt) result = (byte)Math.Min(255, 127 + SZSens[device] * eState.GyroZ); else result = (byte)Math.Max(0, 127 - SZSens[device] * eState.GyroZ);
                        }
                        else return falseVal;
                        break;
                    }
                    case DS4Controls.GyroZNeg:
                    {
                        if (!sOff && eState.GyroZ < -SZD * 10)
                        {
                            if (alt) result = (byte)Math.Min(255, 127 + SZSens[device] * -eState.GyroZ); else result = (byte)Math.Max(0, 127 - SZSens[device] * -eState.GyroZ);
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
    }
}
