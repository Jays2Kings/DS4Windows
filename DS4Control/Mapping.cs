using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DS4Library;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
namespace DS4Control
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
        public static SyntheticState globalState = new SyntheticState();
        public static SyntheticState[] deviceState = { new SyntheticState(), new SyntheticState(), new SyntheticState(), new SyntheticState() };

        // TODO When we disconnect, process a null/dead state to release any keys or buttons.
        public static DateTime oldnow = DateTime.UtcNow;
        private static bool pressagain = false;
        private static int wheel = 0, keyshelddown = 0;
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
                    else if ((gkp.current.toggleCount == 0 && gkp.previous.toggleCount == 0) && gkp.current.vkCount + gkp.current.scanCodeCount == 0 && gkp.previous.vkCount + gkp.previous.scanCodeCount != 0)
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
            switch (ctrl)
            {
                case DS4Controls.Share: return 1;
                case DS4Controls.Options: return 2;
                case DS4Controls.L1: return 3;
                case DS4Controls.R1: return 4;
                case DS4Controls.L3: return 5;
                case DS4Controls.R3: return 6;
                case DS4Controls.DpadUp: return 7;
                case DS4Controls.DpadDown: return 8;
                case DS4Controls.DpadLeft: return 9;
                case DS4Controls.DpadRight: return 10;
                case DS4Controls.PS: return 11;
                case DS4Controls.Cross: return 12;
                case DS4Controls.Square: return 13;
                case DS4Controls.Triangle: return 14;
                case DS4Controls.Circle: return 15;
                case DS4Controls.LXNeg: return 16;
                case DS4Controls.LYNeg: return 17;
                case DS4Controls.RXNeg: return 18;
                case DS4Controls.RYNeg: return 19;
                case DS4Controls.LXPos: return 20;
                case DS4Controls.LYPos: return 21;
                case DS4Controls.RXPos: return 22;
                case DS4Controls.RYPos: return 23;
                case DS4Controls.L2: return 24;
                case DS4Controls.R2: return 25;
                case DS4Controls.TouchMulti: return 26;
                case DS4Controls.TouchLeft: return 27;
                case DS4Controls.TouchRight: return 28;
                case DS4Controls.TouchUpper: return 29;
                case DS4Controls.GyroXNeg: return 30;
                case DS4Controls.GyroXPos: return 31;
                case DS4Controls.GyroZNeg: return 32;
                case DS4Controls.GyroZPos: return 33;
            }
            return 0; 
        }

        static double TValue(double value1, double value2, double percent)
        {
            percent /= 100f;
            return value1 * percent + value2 * (1 - percent);
        }

        public static DS4State SetCurveAndDeadzone(int device, DS4State cState)
        {
            DS4State dState = new DS4State(cState);
            int x;
            int y;
            int curve;
            if (Global.getLSCurve(device) > 0)
            {
                x = cState.LX;
                y = cState.LY;
                float max = x + y;
                double curvex;
                double curvey;
                curve = Global.getLSCurve(device);
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
            if (Global.getRSCurve(device) > 0)
            {
                x = cState.RX;
                y = cState.RY;
                float max = x + y;
                double curvex;
                double curvey;
                curve = Global.getRSCurve(device);
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
            if (Global.getLSDeadzone(device) > 0 &&
                Math.Sqrt(Math.Pow(cState.LX - 127.5f, 2) + Math.Pow(cState.LY - 127.5f, 2)) < Global.getLSDeadzone(device))
            {
                dState.LX = 127;
                dState.LY = 127;
            }
            if (Global.getRSDeadzone(device) > 0
                && Math.Sqrt(Math.Pow(cState.RX - 127.5f, 2) + Math.Pow(cState.RY - 127.5f, 2)) < Global.getLSDeadzone(device))
            {
                dState.RX = 127;
                dState.RY = 127;
            }
            if (Global.getL2Deadzone(device) > 0 && cState.L2 < Global.getL2Deadzone(device))
                dState.L2 = 0;
            if (Global.getR2Deadzone(device) > 0 && cState.R2 < Global.getR2Deadzone(device))
                dState.R2 = 0;
            return dState;
        }

        public static bool[] pressedonce = new bool[261], macrodone = new bool[34];
        public static int test = 0;
        static bool[] macroControl = new bool[25];
        /** Map DS4 Buttons/Axes to other DS4 Buttons/Axes (largely the same as Xinput ones) and to keyboard and mouse buttons. */
        public static async void MapCustom(int device, DS4State cState, DS4State MappedState, DS4StateExposed eState, Mouse tp, Control ctrl)
        {
            bool shift;
            SyntheticState deviceState = Mapping.deviceState[device];
            if (Global.GetActions().Count > 0 && (Global.GetProfileActions(device).Count > 0 || 
                !string.IsNullOrEmpty(Global.tempprofilename[device])))
                MapCustomAction(device, cState, MappedState, eState, tp, ctrl);
            switch (Global.getShiftModifier(device))
            {
                case 1: shift = getBoolMapping(DS4Controls.Cross, cState, eState, tp); break;
                case 2: shift = getBoolMapping(DS4Controls.Circle, cState, eState, tp); break;
                case 3: shift = getBoolMapping(DS4Controls.Square, cState, eState, tp); break;
                case 4: shift = getBoolMapping(DS4Controls.Triangle, cState, eState, tp); break;
                case 5: shift = getBoolMapping(DS4Controls.Options, cState, eState, tp); break;
                case 6: shift = getBoolMapping(DS4Controls.Share, cState, eState, tp); break;
                case 7: shift = getBoolMapping(DS4Controls.DpadUp, cState, eState, tp); break;
                case 8: shift = getBoolMapping(DS4Controls.DpadDown, cState, eState, tp); break;
                case 9: shift = getBoolMapping(DS4Controls.DpadLeft, cState, eState, tp); break;
                case 10: shift = getBoolMapping(DS4Controls.DpadRight, cState, eState, tp); break;
                case 11: shift = getBoolMapping(DS4Controls.PS, cState, eState, tp); break;
                case 12: shift = getBoolMapping(DS4Controls.L1, cState, eState, tp); break;
                case 13: shift = getBoolMapping(DS4Controls.R1, cState, eState, tp); break;
                case 14: shift = getBoolMapping(DS4Controls.L2, cState, eState, tp); break;
                case 15: shift = getBoolMapping(DS4Controls.R2, cState, eState, tp); break;
                case 16: shift = getBoolMapping(DS4Controls.L3, cState, eState, tp); break;
                case 17: shift = getBoolMapping(DS4Controls.R3, cState, eState, tp); break;
                case 18: shift = getBoolMapping(DS4Controls.TouchLeft, cState, eState, tp); break;
                case 19: shift = getBoolMapping(DS4Controls.TouchUpper, cState, eState, tp); break;
                case 20: shift = getBoolMapping(DS4Controls.TouchMulti, cState, eState, tp); break;
                case 21: shift = getBoolMapping(DS4Controls.TouchRight, cState, eState, tp); break;
                case 22: shift = getBoolMapping(DS4Controls.GyroZNeg, cState, eState, tp); break;
                case 23: shift = getBoolMapping(DS4Controls.GyroZPos, cState, eState, tp); break;
                case 24: shift = getBoolMapping(DS4Controls.GyroXPos, cState, eState, tp); break;
                case 25: shift = getBoolMapping(DS4Controls.GyroXNeg, cState, eState, tp); break;
                case 26: shift = cState.Touch1; break;
                default: shift = false; break;
            }
            cState.CopyTo(MappedState);
            if (shift)
                MapShiftCustom(device, cState, MappedState, eState, tp);           
            foreach (KeyValuePair<DS4Controls, string> customKey in Global.getCustomMacros(device))
            {
                if (shift == false ||
                    (Global.getShiftCustomMacro(device, customKey.Key) == "0" &&
                    Global.getShiftCustomKey(device, customKey.Key) == 0 &&
                    Global.getShiftCustomButton(device, customKey.Key) == X360Controls.None))
                {
                    DS4KeyType keyType = Global.getCustomKeyType(device, customKey.Key);
                    if (getBoolMapping(customKey.Key, cState, eState, tp))
                    {
                        resetToDefaultValue(customKey.Key, MappedState);
                        bool LXChanged = (Math.Abs(127 - MappedState.LX) < 5);
                        bool LYChanged = (Math.Abs(127 - MappedState.LY) < 5);
                        bool RXChanged = (Math.Abs(127 - MappedState.RX) < 5);
                        bool RYChanged = (Math.Abs(127 - MappedState.RY) < 5);
                        string[] skeys;
                        int[] keys;
                        if (!string.IsNullOrEmpty(customKey.Value))
                        {
                            skeys = customKey.Value.Split('/');
                            keys = new int[skeys.Length];
                        }
                        else
                        {
                            skeys = new string[0];
                            keys = new int[0];
                        }
                        for (int i = 0; i < keys.Length; i++)
                            keys[i] = ushort.Parse(skeys[i]);
                        bool[] keydown = new bool[286];
                        if (!macrodone[DS4ControltoInt(customKey.Key)])
                        {
                            macrodone[DS4ControltoInt(customKey.Key)] = true;
                            for (int i = 0; i < keys.Length; i++)
                            {
                                if (keys[i] >= 300) //ints over 300 used to delay
                                    await Task.Delay(keys[i] - 300);
                                else if (!keydown[keys[i]])
                                {
                                    if (keys[i] == 256) InputMethods.MouseEvent(InputMethods.MOUSEEVENTF_LEFTDOWN); //anything above 255 is not a keyvalue
                                    else if (keys[i] == 257) InputMethods.MouseEvent(InputMethods.MOUSEEVENTF_RIGHTDOWN);
                                    else if (keys[i] == 258) InputMethods.MouseEvent(InputMethods.MOUSEEVENTF_MIDDLEDOWN);
                                    else if (keys[i] == 259) InputMethods.MouseEvent(InputMethods.MOUSEEVENTF_XBUTTONDOWN, 1);
                                    else if (keys[i] == 260) InputMethods.MouseEvent(InputMethods.MOUSEEVENTF_XBUTTONDOWN, 2);
                                    else if (keys[i] == 261) macroControl[0] = true; 
                                    else if (keys[i] == 262) macroControl[1] = true; 
                                    else if (keys[i] == 263) macroControl[2] = true; 
                                    else if (keys[i] == 264) macroControl[3] = true; 
                                    else if (keys[i] == 265) macroControl[4] = true; 
                                    else if (keys[i] == 266) macroControl[5] = true; 
                                    else if (keys[i] == 267) macroControl[6] = true; 
                                    else if (keys[i] == 268) macroControl[7] = true; 
                                    else if (keys[i] == 269) macroControl[8] = true; 
                                    else if (keys[i] == 270) macroControl[9] = true; 
                                    else if (keys[i] == 271) macroControl[10] = true;
                                    else if (keys[i] == 272) macroControl[11] = true;
                                    else if (keys[i] == 273) macroControl[12] = true;
                                    else if (keys[i] == 274) macroControl[13] = true;
                                    else if (keys[i] == 275) macroControl[14] = true;
                                    else if (keys[i] == 276) macroControl[15] = true;
                                    else if (keys[i] == 277) macroControl[16] = true;
                                    else if (keys[i] == 278) macroControl[17] = true;
                                    else if (keys[i] == 279) macroControl[18] = true;
                                    else if (keys[i] == 280) macroControl[19] = true;
                                    else if (keys[i] == 281) macroControl[20] = true;
                                    else if (keys[i] == 282) macroControl[21] = true;
                                    else if (keys[i] == 283) macroControl[22] = true;
                                    else if (keys[i] == 284) macroControl[23] = true;
                                    else if (keys[i] == 285) macroControl[24] = true;
                                    else if (keyType.HasFlag(DS4KeyType.ScanCode))
                                        InputMethods.performSCKeyPress((ushort)keys[i]);
                                    else
                                        InputMethods.performKeyPress((ushort)keys[i]);
                                    keydown[keys[i]] = true;
                                }
                                else
                                {
                                    if (keys[i] == 256) InputMethods.MouseEvent(InputMethods.MOUSEEVENTF_LEFTUP); //anything above 255 is not a keyvalue
                                    else if (keys[i] == 257) InputMethods.MouseEvent(InputMethods.MOUSEEVENTF_RIGHTUP);
                                    else if (keys[i] == 258) InputMethods.MouseEvent(InputMethods.MOUSEEVENTF_MIDDLEUP);
                                    else if (keys[i] == 259) InputMethods.MouseEvent(InputMethods.MOUSEEVENTF_XBUTTONUP, 1);
                                    else if (keys[i] == 260) InputMethods.MouseEvent(InputMethods.MOUSEEVENTF_XBUTTONUP, 2);
                                    else if (keys[i] == 261) macroControl[0] = false; 
                                    else if (keys[i] == 262) macroControl[1] = false; 
                                    else if (keys[i] == 263) macroControl[2] = false; 
                                    else if (keys[i] == 264) macroControl[3] = false; 
                                    else if (keys[i] == 265) macroControl[4] = false; 
                                    else if (keys[i] == 266) macroControl[5] = false; 
                                    else if (keys[i] == 267) macroControl[6] = false; 
                                    else if (keys[i] == 268) macroControl[7] = false; 
                                    else if (keys[i] == 269) macroControl[8] = false; 
                                    else if (keys[i] == 270) macroControl[9] = false; 
                                    else if (keys[i] == 271) macroControl[10] = false;
                                    else if (keys[i] == 272) macroControl[11] = false;
                                    else if (keys[i] == 273) macroControl[12] = false;
                                    else if (keys[i] == 274) macroControl[13] = false;
                                    else if (keys[i] == 275) macroControl[14] = false;
                                    else if (keys[i] == 276) macroControl[15] = false;
                                    else if (keys[i] == 277) macroControl[16] = false;
                                    else if (keys[i] == 278) macroControl[17] = false;
                                    else if (keys[i] == 279) macroControl[18] = false;
                                    else if (keys[i] == 280) macroControl[19] = false;
                                    else if (keys[i] == 281) macroControl[20] = false;
                                    else if (keys[i] == 282) macroControl[21] = false;
                                    else if (keys[i] == 283) macroControl[22] = false;
                                    else if (keys[i] == 284) macroControl[23] = false;
                                    else if (keys[i] == 285) macroControl[24] = false;
                                    else if (keyType.HasFlag(DS4KeyType.ScanCode))
                                        InputMethods.performSCKeyRelease((ushort)keys[i]);
                                    else
                                        InputMethods.performKeyRelease((ushort)keys[i]);
                                    keydown[keys[i]] = false;
                                }
                            }
                            for (ushort i = 0; i < keydown.Length; i++)
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
                                    else if (keys[i] == 278) macroControl[17] = false;
                                    else if (keys[i] == 279) macroControl[18] = false;
                                    else if (keys[i] == 280) macroControl[19] = false;
                                    else if (keys[i] == 281) macroControl[20] = false;
                                    else if (keys[i] == 282) macroControl[21] = false;
                                    else if (keys[i] == 283) macroControl[22] = false;
                                    else if (keys[i] == 284) macroControl[23] = false;
                                    else if (keys[i] == 285) macroControl[24] = false;
                                    else if (keyType.HasFlag(DS4KeyType.ScanCode))
                                        InputMethods.performSCKeyRelease(i);
                                    else
                                        InputMethods.performKeyRelease(i);
                            }
                            if (keyType.HasFlag(DS4KeyType.HoldMacro))
                            {
                                await Task.Delay(50);
                                macrodone[DS4ControltoInt(customKey.Key)] = false;
                            }
                        }
                    }
                    else if (!getBoolMapping(customKey.Key, cState, eState, tp))
                        macrodone[DS4ControltoInt(customKey.Key)] = false;
                }
            }
            foreach (KeyValuePair<DS4Controls, ushort> customKey in Global.getCustomKeys(device))
            {
                if (shift == false ||
                    (Global.getShiftCustomMacro(device, customKey.Key) == "0" &&
                    Global.getShiftCustomKey(device, customKey.Key) == 0 &&
                    Global.getShiftCustomButton(device, customKey.Key) == X360Controls.None))
                {
                    DS4KeyType keyType = Global.getCustomKeyType(device, customKey.Key);
                    if (getBoolMapping(customKey.Key, cState, eState, tp))
                    {
                        resetToDefaultValue(customKey.Key, MappedState);
                        SyntheticState.KeyPresses kp;
                        if (!deviceState.keyPresses.TryGetValue(customKey.Value, out kp))
                            deviceState.keyPresses[customKey.Value] = kp = new SyntheticState.KeyPresses();
                        if (keyType.HasFlag(DS4KeyType.ScanCode))
                            kp.current.scanCodeCount++;
                        else
                            kp.current.vkCount++;
                        if (keyType.HasFlag(DS4KeyType.Toggle))
                        {
                            if (!pressedonce[customKey.Value])
                            {
                                kp.current.toggle = !kp.current.toggle;
                                pressedonce[customKey.Value] = true;
                            }
                            kp.current.toggleCount++;
                        }
                        kp.current.repeatCount++;
                    }
                    else
                        pressedonce[customKey.Value] = false;
                }
            }

            MappedState.LX = 127;
            MappedState.LY = 127;
            MappedState.RX = 127;
            MappedState.RY = 127;
            int MouseDeltaX = 0;
            int MouseDeltaY = 0;

            //Dictionary<DS4Controls, X360Controls> customButtons = Global.getCustomButtons(device);
            //foreach (KeyValuePair<DS4Controls, X360Controls> customButton in customButtons)
            List<DS4Controls> Cross = new List<DS4Controls>();
            List<DS4Controls> Circle = new List<DS4Controls>();
            List<DS4Controls> Square = new List<DS4Controls>();
            List<DS4Controls> Triangle = new List<DS4Controls>();
            List<DS4Controls> Options = new List<DS4Controls>();
            List<DS4Controls> Share = new List<DS4Controls>();
            List<DS4Controls> DpadUp = new List<DS4Controls>();
            List<DS4Controls> DpadDown = new List<DS4Controls>();
            List<DS4Controls> DpadLeft = new List<DS4Controls>();
            List<DS4Controls> DpadRight = new List<DS4Controls>();
            List<DS4Controls> PS = new List<DS4Controls>();
            List<DS4Controls> L1 = new List<DS4Controls>();
            List<DS4Controls> R1 = new List<DS4Controls>();
            List<DS4Controls> L2 = new List<DS4Controls>();
            List<DS4Controls> R2 = new List<DS4Controls>();
            List<DS4Controls> L3 = new List<DS4Controls>();
            List<DS4Controls> R3 = new List<DS4Controls>();
            List<DS4Controls> LXN = new List<DS4Controls>();
            List<DS4Controls> LXP = new List<DS4Controls>();
            List<DS4Controls> LYN = new List<DS4Controls>();
            List<DS4Controls> LYP = new List<DS4Controls>();
            List<DS4Controls> RXN = new List<DS4Controls>();
            List<DS4Controls> RXP = new List<DS4Controls>();
            List<DS4Controls> RYN = new List<DS4Controls>();
            List<DS4Controls> RYP = new List<DS4Controls>();
            foreach (KeyValuePair<DS4Controls, X360Controls> customButton in Global.getCustomButtons(device))
            {
                if (shift == false ||
                    (Global.getShiftCustomMacro(device, customButton.Key) == "0" &&
                    Global.getShiftCustomKey(device, customButton.Key) == 0 &&
                    Global.getShiftCustomButton(device, customButton.Key) == X360Controls.None))
                {
                    DS4KeyType keyType = Global.getCustomKeyType(device, customButton.Key);
                    int keyvalue = 0;
                    switch (customButton.Value)
                    {
                        case X360Controls.LeftMouse: keyvalue = 256; break;
                        case X360Controls.RightMouse: keyvalue = 257; break;
                        case X360Controls.MiddleMouse: keyvalue = 258; break;
                        case X360Controls.FourthMouse: keyvalue = 259; break;
                        case X360Controls.FifthMouse: keyvalue = 260; break;
                    }
                    if (keyType.HasFlag(DS4KeyType.Toggle))
                    {
                        if (getBoolMapping(customButton.Key, cState, eState, tp))
                        {
                            resetToDefaultValue(customButton.Key, MappedState);
                            if (!pressedonce[keyvalue])
                            {
                                deviceState.currentClicks.toggle = !deviceState.currentClicks.toggle;
                                test++;
                                pressedonce[keyvalue] = true;
                            }
                            deviceState.currentClicks.toggleCount++;
                        }
                        else// if (test = 1) && pressedonce[keyvalue])
                        {
                            pressedonce[keyvalue] = false;
                        }
                    }
                    resetToDefaultValue(customButton.Key, MappedState); // erase default mappings for things that are remapped
                    switch (customButton.Value)
                    {
                        case X360Controls.A: Cross.Add(customButton.Key); break;
                        case X360Controls.B: Circle.Add(customButton.Key); break;
                        case X360Controls.X: Square.Add(customButton.Key); break;
                        case X360Controls.Y: Triangle.Add(customButton.Key); break;
                        case X360Controls.LB: L1.Add(customButton.Key); break;
                        case X360Controls.LS: L3.Add(customButton.Key); break;
                        case X360Controls.RB: R1.Add(customButton.Key); break;
                        case X360Controls.RS: R3.Add(customButton.Key); break;
                        case X360Controls.DpadUp: DpadUp.Add(customButton.Key); break;
                        case X360Controls.DpadDown: DpadDown.Add(customButton.Key); break;
                        case X360Controls.DpadLeft: DpadLeft.Add(customButton.Key); break;
                        case X360Controls.DpadRight: DpadRight.Add(customButton.Key); break;
                        case X360Controls.Start: Options.Add(customButton.Key); break;
                        case X360Controls.Guide: PS.Add(customButton.Key); break;
                        case X360Controls.Back: Share.Add(customButton.Key); break;
                        case X360Controls.LXNeg: LXN.Add(customButton.Key); break;
                        case X360Controls.LYNeg: LYN.Add(customButton.Key); break;
                        case X360Controls.RXNeg: RXN.Add(customButton.Key); break;
                        case X360Controls.RYNeg: RYN.Add(customButton.Key); break;
                        case X360Controls.LXPos: LXP.Add(customButton.Key); break;
                        case X360Controls.LYPos: LYP.Add(customButton.Key); break;
                        case X360Controls.RXPos: RXP.Add(customButton.Key); break;
                        case X360Controls.RYPos: RYP.Add(customButton.Key); break;
                        case X360Controls.LT: L2.Add(customButton.Key); break;
                        case X360Controls.RT: R2.Add(customButton.Key); break;
                        case X360Controls.LeftMouse:
                            if (getBoolMapping(customButton.Key, cState, eState, tp))
                                deviceState.currentClicks.leftCount++;
                            break;
                        case X360Controls.RightMouse:
                            if (getBoolMapping(customButton.Key, cState, eState, tp))
                                deviceState.currentClicks.rightCount++;
                            break;
                        case X360Controls.MiddleMouse:
                            if (getBoolMapping(customButton.Key, cState, eState, tp))
                                deviceState.currentClicks.middleCount++;
                            break;
                        case X360Controls.FourthMouse:
                            if (getBoolMapping(customButton.Key, cState, eState, tp))
                                deviceState.currentClicks.fourthCount++;
                            break;
                        case X360Controls.FifthMouse:
                            if (getBoolMapping(customButton.Key, cState, eState, tp))
                                deviceState.currentClicks.fifthCount++;
                            break;
                        case X360Controls.WUP:
                            if (getBoolMapping(customButton.Key, cState, eState, tp))
                                deviceState.currentClicks.wUpCount++;
                            break;
                        case X360Controls.WDOWN:
                            if (getBoolMapping(customButton.Key, cState, eState, tp))
                                deviceState.currentClicks.wDownCount++;
                            break;
                        case X360Controls.MouseUp:
                            if (MouseDeltaY == 0)
                            {
                                MouseDeltaY = getMouseMapping(device, customButton.Key, cState, eState, 0);
                                MouseDeltaY = -Math.Abs((MouseDeltaY == -2147483648 ? 0 : MouseDeltaY));
                            }
                            break;
                        case X360Controls.MouseDown:
                            if (MouseDeltaY == 0)
                            {
                                MouseDeltaY = getMouseMapping(device, customButton.Key, cState, eState, 1);
                                MouseDeltaY = Math.Abs((MouseDeltaY == -2147483648 ? 0 : MouseDeltaY));
                            }
                            break;
                        case X360Controls.MouseLeft:
                            if (MouseDeltaX == 0)
                            {
                                MouseDeltaX = getMouseMapping(device, customButton.Key, cState, eState, 2);
                                MouseDeltaX = -Math.Abs((MouseDeltaX == -2147483648 ? 0 : MouseDeltaX));
                            }
                            break;
                        case X360Controls.MouseRight:
                            if (MouseDeltaX == 0)
                            {
                                MouseDeltaX = getMouseMapping(device, customButton.Key, cState, eState, 3);
                                MouseDeltaX = Math.Abs((MouseDeltaX == -2147483648 ? 0 : MouseDeltaX));
                            }
                            break;
                    }
                }
            }
            if (macroControl[0]) MappedState.Cross = true;
            if (macroControl[01]) MappedState.Circle = true;
            if (macroControl[02]) MappedState.Square = true;
            if (macroControl[03]) MappedState.Triangle = true;
            if (macroControl[04]) MappedState.Options = true;
            if (macroControl[05]) MappedState.Share = true;
            if (macroControl[06]) MappedState.DpadUp = true;
            if (macroControl[07]) MappedState.DpadDown =true;
            if (macroControl[08]) MappedState.DpadLeft = true;
            if (macroControl[09]) MappedState.DpadRight = true;
            if (macroControl[10]) MappedState.PS = true;
            if (macroControl[11]) MappedState.L1 = true;
            if (macroControl[12]) MappedState.R1 = true;
            if (macroControl[13]) MappedState.L2 = 255;
            if (macroControl[14]) MappedState.R2 = 255;
            if (macroControl[15])  MappedState.L3 = true;
            if (macroControl[16]) MappedState.R3 = true;
            if (macroControl[17]) MappedState.LX = 255;
            if (macroControl[18]) MappedState.LX = 0;
            if (macroControl[19]) MappedState.LY = 255;
            if (macroControl[20]) MappedState.LY = 0;
            if (macroControl[21]) MappedState.RX = 255;
            if (macroControl[22]) MappedState.RX = 0;
            if (macroControl[23]) MappedState.RY = 255;
            if (macroControl[24]) MappedState.RY = 0;
            foreach (DS4Controls dc in Cross)
                if (getBoolMapping(dc, cState, eState, tp))
                    MappedState.Cross = true;
            foreach (DS4Controls dc in Circle)
                if (getBoolMapping(dc, cState, eState, tp))
                    MappedState.Circle = true;
            foreach (DS4Controls dc in Square)
                if (getBoolMapping(dc, cState, eState, tp))
                    MappedState.Square = true;
            foreach (DS4Controls dc in Triangle)
                if (getBoolMapping(dc, cState, eState, tp))
                    MappedState.Triangle = true;
            foreach (DS4Controls dc in L1)
                if (getBoolMapping(dc, cState, eState, tp))
                    MappedState.L1 = true;
            foreach (DS4Controls dc in L2)
                if (getByteMapping(device, dc, cState, eState, tp) > 5)
                    MappedState.L2 = getByteMapping(device, dc, cState, eState, tp);
            foreach (DS4Controls dc in L3)
                if (getBoolMapping(dc, cState, eState, tp))
                    MappedState.L3 = true;
            foreach (DS4Controls dc in R1)
                if (getBoolMapping(dc, cState, eState, tp))
                    MappedState.R1 = true;
            foreach (DS4Controls dc in R2)
                if (getByteMapping(device, dc, cState, eState, tp) > 5)
                    MappedState.R2 = getByteMapping(device, dc, cState, eState, tp);
            foreach (DS4Controls dc in R3)
                if (getBoolMapping(dc, cState, eState, tp))
                    MappedState.R3 = true;
            foreach (DS4Controls dc in DpadUp)
                if (getBoolMapping(dc, cState, eState, tp))
                    MappedState.DpadUp = true;
            foreach (DS4Controls dc in DpadRight)
                if (getBoolMapping(dc, cState, eState, tp))
                    MappedState.DpadRight = true;
            foreach (DS4Controls dc in DpadLeft)
                if (getBoolMapping(dc, cState, eState, tp))
                    MappedState.DpadLeft = true;
            foreach (DS4Controls dc in DpadDown)
                if (getBoolMapping(dc, cState, eState, tp))
                    MappedState.DpadDown = true;
            foreach (DS4Controls dc in Options)
                if (getBoolMapping(dc, cState, eState, tp))
                    MappedState.Options = true;
            foreach (DS4Controls dc in Share)
                if (getBoolMapping(dc, cState, eState, tp))
                    MappedState.Share = true;
            foreach (DS4Controls dc in PS)
                if (getBoolMapping(dc, cState, eState, tp))
                    MappedState.PS = true;

            if (Global.getCustomButton(device, DS4Controls.LXNeg) == X360Controls.None)
                LXN.Add(DS4Controls.LXNeg);
            if (Global.getCustomButton(device, DS4Controls.LXPos) == X360Controls.None)
                LXP.Add(DS4Controls.LXPos);
            if (Global.getCustomButton(device, DS4Controls.LYNeg) == X360Controls.None)
                LYN.Add(DS4Controls.LYNeg);
            if (Global.getCustomButton(device, DS4Controls.LYPos) == X360Controls.None)
                LYP.Add(DS4Controls.LYPos);
            if (Global.getCustomButton(device, DS4Controls.RXNeg) == X360Controls.None)
                RXN.Add(DS4Controls.RXNeg);
            if (Global.getCustomButton(device, DS4Controls.RXPos) == X360Controls.None)
                RXP.Add(DS4Controls.RXPos);
            if (Global.getCustomButton(device, DS4Controls.RYNeg) == X360Controls.None)
                RYN.Add(DS4Controls.RYNeg);
            if (Global.getCustomButton(device, DS4Controls.RYPos) == X360Controls.None)
                RYP.Add(DS4Controls.RYPos);
            if (LXN.Count > 0 || LXP.Count > 0)
            {
                foreach (DS4Controls dc in LXP)
                    if (Math.Abs(127 - getXYAxisMapping(device, dc, cState, eState, tp, true)) > 5)
                        MappedState.LX = getXYAxisMapping(device, dc, cState, eState, tp, true);
                foreach (DS4Controls dc in LXN)
                    if (Math.Abs(127 - getXYAxisMapping(device, dc, cState, eState, tp)) > 5)
                        MappedState.LX = getXYAxisMapping(device, dc, cState, eState, tp);
            }
            else
                MappedState.LX = cState.LX;
            if (LYN.Count > 0 || LYP.Count > 0)
            {
                foreach (DS4Controls dc in LYN)
                    if (Math.Abs(127 - getXYAxisMapping(device, dc, cState, eState, tp)) > 5)
                        MappedState.LY = getXYAxisMapping(device, dc, cState, eState, tp);
                foreach (DS4Controls dc in LYP)
                    if (Math.Abs(127 - getXYAxisMapping(device, dc, cState, eState, tp, true)) > 5)
                        MappedState.LY = getXYAxisMapping(device, dc, cState, eState, tp, true);
            }
            else
                MappedState.LY = cState.LY;
            if (RXN.Count > 0 || RXP.Count > 0)
            {
                foreach (DS4Controls dc in RXN)
                    if (Math.Abs(127 - getXYAxisMapping(device, dc, cState, eState, tp)) > 5)
                        MappedState.RX = getXYAxisMapping(device, dc, cState, eState, tp);
                foreach (DS4Controls dc in RXP)
                    if (Math.Abs(127 - getXYAxisMapping(device, dc, cState, eState, tp, true)) > 5)
                        MappedState.RX = getXYAxisMapping(device, dc, cState, eState, tp, true);
            }
            else
                MappedState.RX = cState.RX;
            if (RYN.Count > 0 || RYP.Count > 0)
            {
                foreach (DS4Controls dc in RYN)
                    if (Math.Abs(127 - getXYAxisMapping(device, dc, cState, eState, tp)) > 5)
                        MappedState.RY = getXYAxisMapping(device, dc, cState, eState, tp);
                foreach (DS4Controls dc in RYP)
                    if (Math.Abs(127 - getXYAxisMapping(device, dc, cState, eState, tp, true)) > 5)
                        MappedState.RY = getXYAxisMapping(device, dc, cState, eState, tp, true);
            }
            else
                MappedState.RY = cState.RY;
            InputMethods.MoveCursorBy(MouseDeltaX, MouseDeltaY);
        }

        public static async void MapShiftCustom(int device, DS4State cState, DS4State MappedState, DS4StateExposed eState, Mouse tp)
        {
            //cState.CopyTo(MappedState);
            SyntheticState deviceState = Mapping.deviceState[device];
            foreach (KeyValuePair<DS4Controls, string> customKey in Global.getShiftCustomMacros(device)) //with delays
            {
                DS4KeyType keyType = Global.getShiftCustomKeyType(device, customKey.Key);
                if (getBoolMapping(customKey.Key, cState, eState, tp))
                {
                    resetToDefaultValue(customKey.Key, MappedState);                    
                    bool LXChanged = (Math.Abs(127 - MappedState.LX) < 5);
                    bool LYChanged = (Math.Abs(127 - MappedState.LY) < 5);
                    bool RXChanged = (Math.Abs(127 - MappedState.RX) < 5);
                    bool RYChanged = (Math.Abs(127 - MappedState.RY) < 5);
                    string[] skeys;
                    int[] keys;
                    if (!string.IsNullOrEmpty(customKey.Value))
                    {
                        skeys = customKey.Value.Split('/');
                        keys = new int[skeys.Length];
                    }
                    else
                    {
                        skeys = new string[0];
                        keys = new int[0];
                    }
                    for (int i = 0; i < keys.Length; i++)
                        keys[i] = ushort.Parse(skeys[i]);
                    bool[] keydown = new bool[286];
                    if (!macrodone[DS4ControltoInt(customKey.Key)])
                    {
                        macrodone[DS4ControltoInt(customKey.Key)] = true;
                        for (int i = 0; i < keys.Length; i++)
                        {
                            if (keys[i] >= 300) //ints over 300 used to delay
                                await Task.Delay(keys[i] - 300);
                            else if (!keydown[keys[i]])
                            {
                                if (keys[i] == 256) InputMethods.MouseEvent(InputMethods.MOUSEEVENTF_LEFTDOWN); //anything above 255 is not a keyvalue
                                else if (keys[i] == 257) InputMethods.MouseEvent(InputMethods.MOUSEEVENTF_RIGHTDOWN);
                                else if (keys[i] == 258) InputMethods.MouseEvent(InputMethods.MOUSEEVENTF_MIDDLEDOWN);
                                else if (keys[i] == 259) InputMethods.MouseEvent(InputMethods.MOUSEEVENTF_XBUTTONDOWN, 1);
                                else if (keys[i] == 260) InputMethods.MouseEvent(InputMethods.MOUSEEVENTF_XBUTTONDOWN, 2);
                                else if (keys[i] == 261) macroControl[0] = true;
                                else if (keys[i] == 262) macroControl[1] = true;
                                else if (keys[i] == 263) macroControl[2] = true;
                                else if (keys[i] == 264) macroControl[3] = true;
                                else if (keys[i] == 265) macroControl[4] = true;
                                else if (keys[i] == 266) macroControl[5] = true;
                                else if (keys[i] == 267) macroControl[6] = true;
                                else if (keys[i] == 268) macroControl[7] = true;
                                else if (keys[i] == 269) macroControl[8] = true;
                                else if (keys[i] == 270) macroControl[9] = true;
                                else if (keys[i] == 271) macroControl[10] = true;
                                else if (keys[i] == 272) macroControl[11] = true;
                                else if (keys[i] == 273) macroControl[12] = true;
                                else if (keys[i] == 274) macroControl[13] = true;
                                else if (keys[i] == 275) macroControl[14] = true;
                                else if (keys[i] == 276) macroControl[15] = true;
                                else if (keys[i] == 277) macroControl[16] = true;
                                else if (keys[i] == 278) macroControl[17] = true;
                                else if (keys[i] == 279) macroControl[18] = true;
                                else if (keys[i] == 280) macroControl[19] = true;
                                else if (keys[i] == 281) macroControl[20] = true;
                                else if (keys[i] == 282) macroControl[21] = true;
                                else if (keys[i] == 283) macroControl[22] = true;
                                else if (keys[i] == 284) macroControl[23] = true;
                                else if (keys[i] == 285) macroControl[24] = true;
                                else if (keyType.HasFlag(DS4KeyType.ScanCode))
                                    InputMethods.performSCKeyPress((ushort)keys[i]);
                                else
                                    InputMethods.performKeyPress((ushort)keys[i]);
                                keydown[keys[i]] = true;
                            }
                            else
                            {
                                if (keys[i] == 256) InputMethods.MouseEvent(InputMethods.MOUSEEVENTF_LEFTUP); //anything above 255 is not a keyvalue
                                else if (keys[i] == 257) InputMethods.MouseEvent(InputMethods.MOUSEEVENTF_RIGHTUP);
                                else if (keys[i] == 258) InputMethods.MouseEvent(InputMethods.MOUSEEVENTF_MIDDLEUP);
                                else if (keys[i] == 259) InputMethods.MouseEvent(InputMethods.MOUSEEVENTF_XBUTTONUP, 1);
                                else if (keys[i] == 260) InputMethods.MouseEvent(InputMethods.MOUSEEVENTF_XBUTTONUP, 2);
                                else if (keys[i] == 261) macroControl[0] = false;
                                else if (keys[i] == 262) macroControl[1] = false;
                                else if (keys[i] == 263) macroControl[2] = false;
                                else if (keys[i] == 264) macroControl[3] = false;
                                else if (keys[i] == 265) macroControl[4] = false;
                                else if (keys[i] == 266) macroControl[5] = false;
                                else if (keys[i] == 267) macroControl[6] = false;
                                else if (keys[i] == 268) macroControl[7] = false;
                                else if (keys[i] == 269) macroControl[8] = false;
                                else if (keys[i] == 270) macroControl[9] = false;
                                else if (keys[i] == 271) macroControl[10] = false;
                                else if (keys[i] == 272) macroControl[11] = false;
                                else if (keys[i] == 273) macroControl[12] = false;
                                else if (keys[i] == 274) macroControl[13] = false;
                                else if (keys[i] == 275) macroControl[14] = false;
                                else if (keys[i] == 276) macroControl[15] = false;
                                else if (keys[i] == 277) macroControl[16] = false;
                                else if (keys[i] == 278) macroControl[17] = false;
                                else if (keys[i] == 279) macroControl[18] = false;
                                else if (keys[i] == 280) macroControl[19] = false;
                                else if (keys[i] == 281) macroControl[20] = false;
                                else if (keys[i] == 282) macroControl[21] = false;
                                else if (keys[i] == 283) macroControl[22] = false;
                                else if (keys[i] == 284) macroControl[23] = false;
                                else if (keys[i] == 285) macroControl[24] = false;
                                else if (keyType.HasFlag(DS4KeyType.ScanCode))
                                    InputMethods.performSCKeyRelease((ushort)keys[i]);
                                else
                                    InputMethods.performKeyRelease((ushort)keys[i]);
                                keydown[keys[i]] = false;
                            }
                        }
                        for (ushort i = 0; i < keydown.Length; i++)
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
                                else if (keys[i] == 278) macroControl[17] = false;
                                else if (keys[i] == 279) macroControl[18] = false;
                                else if (keys[i] == 280) macroControl[19] = false;
                                else if (keys[i] == 281) macroControl[20] = false;
                                else if (keys[i] == 282) macroControl[21] = false;
                                else if (keys[i] == 283) macroControl[22] = false;
                                else if (keys[i] == 284) macroControl[23] = false;
                                else if (keys[i] == 285) macroControl[24] = false;
                                else if (keyType.HasFlag(DS4KeyType.ScanCode))
                                    InputMethods.performSCKeyRelease(i);
                                else
                                    InputMethods.performKeyRelease(i);
                        }
                        if (keyType.HasFlag(DS4KeyType.HoldMacro))
                        {
                            await Task.Delay(50);
                            macrodone[DS4ControltoInt(customKey.Key)] = false;
                        }
                    }
                }
                else if (!getBoolMapping(customKey.Key, cState, eState, tp))
                    macrodone[DS4ControltoInt(customKey.Key)] = false;
            }
            foreach (KeyValuePair<DS4Controls, ushort> customKey in Global.getShiftCustomKeys(device))
            {
                DS4KeyType keyType = Global.getShiftCustomKeyType(device, customKey.Key);
                if (getBoolMapping(customKey.Key, cState, eState, tp))
                {
                    resetToDefaultValue(customKey.Key, MappedState);
                    SyntheticState.KeyPresses kp;
                    if (!deviceState.keyPresses.TryGetValue(customKey.Value, out kp))
                        deviceState.keyPresses[customKey.Value] = kp = new SyntheticState.KeyPresses();
                    if (keyType.HasFlag(DS4KeyType.ScanCode))
                        kp.current.scanCodeCount++;
                    else
                        kp.current.vkCount++;
                    if (keyType.HasFlag(DS4KeyType.Toggle))
                    {
                        if (!pressedonce[customKey.Value])
                        {
                            kp.current.toggle = !kp.current.toggle;
                            pressedonce[customKey.Value] = true;
                        }
                        kp.current.toggleCount++;
                    }
                    kp.current.repeatCount++;
                }
                else
                    pressedonce[customKey.Value] = false;
            }

            MappedState.LX = 127;
            MappedState.LY = 127;
            MappedState.RX = 127;
            MappedState.RY = 127;
            int MouseDeltaX = 0;
            int MouseDeltaY = 0;

            Dictionary<DS4Controls, X360Controls> customButtons = Global.getShiftCustomButtons(device);
            //foreach (KeyValuePair<DS4Controls, X360Controls> customButton in customButtons)
            // resetToDefaultValue(customButton.Key, MappedState); // erase default mappings for things that are remapped

            List<DS4Controls> Cross = new List<DS4Controls>();
            List<DS4Controls> Circle = new List<DS4Controls>();
            List<DS4Controls> Square = new List<DS4Controls>();
            List<DS4Controls> Triangle = new List<DS4Controls>();
            List<DS4Controls> Options = new List<DS4Controls>();
            List<DS4Controls> Share = new List<DS4Controls>();
            List<DS4Controls> DpadUp = new List<DS4Controls>();
            List<DS4Controls> DpadDown = new List<DS4Controls>();
            List<DS4Controls> DpadLeft = new List<DS4Controls>();
            List<DS4Controls> DpadRight = new List<DS4Controls>();
            List<DS4Controls> PS = new List<DS4Controls>();
            List<DS4Controls> L1 = new List<DS4Controls>();
            List<DS4Controls> R1 = new List<DS4Controls>();
            List<DS4Controls> L2 = new List<DS4Controls>();
            List<DS4Controls> R2 = new List<DS4Controls>();
            List<DS4Controls> L3 = new List<DS4Controls>();
            List<DS4Controls> R3 = new List<DS4Controls>();
            List<DS4Controls> LXN = new List<DS4Controls>();
            List<DS4Controls> LXP = new List<DS4Controls>();
            List<DS4Controls> LYN = new List<DS4Controls>();
            List<DS4Controls> LYP = new List<DS4Controls>();
            List<DS4Controls> RXN = new List<DS4Controls>();
            List<DS4Controls> RXP = new List<DS4Controls>();
            List<DS4Controls> RYN = new List<DS4Controls>();
            List<DS4Controls> RYP = new List<DS4Controls>();
            foreach (KeyValuePair<DS4Controls, X360Controls> customButton in customButtons)
            {
                resetToDefaultValue(customButton.Key, MappedState); // erase default mappings for things that are remapped
                DS4KeyType keyType = Global.getShiftCustomKeyType(device, customButton.Key);
                int keyvalue = 0;
                switch (customButton.Value)
                {
                    case X360Controls.LeftMouse: keyvalue = 256; break;
                    case X360Controls.RightMouse: keyvalue = 257; break;
                    case X360Controls.MiddleMouse: keyvalue = 258; break;
                    case X360Controls.FourthMouse: keyvalue = 259; break;
                    case X360Controls.FifthMouse: keyvalue = 260; break;
                }
                if (keyType.HasFlag(DS4KeyType.Toggle))
                {
                    if (getBoolMapping(customButton.Key, cState, eState, tp))
                    {
                        if (!pressedonce[keyvalue])
                        {
                            deviceState.currentClicks.toggle = !deviceState.currentClicks.toggle;
                            test++;
                            pressedonce[keyvalue] = true;
                        }
                        deviceState.currentClicks.toggleCount++;
                    }
                    else// if (test = 1)// && pressedonce[keyvalue])
                    {
                        pressedonce[keyvalue] = false;
                    }
                }
               switch (customButton.Value)
                    {
                        case X360Controls.A: Cross.Add(customButton.Key); break;
                        case X360Controls.B: Circle.Add(customButton.Key); break;
                        case X360Controls.X: Square.Add(customButton.Key); break;
                        case X360Controls.Y: Triangle.Add(customButton.Key); break;
                        case X360Controls.LB: L1.Add(customButton.Key); break;
                        case X360Controls.LS: L3.Add(customButton.Key); break;
                        case X360Controls.RB: R1.Add(customButton.Key); break;
                        case X360Controls.RS: R3.Add(customButton.Key); break;
                        case X360Controls.DpadUp: DpadUp.Add(customButton.Key); break;
                        case X360Controls.DpadDown: DpadDown.Add(customButton.Key); break;
                        case X360Controls.DpadLeft: DpadLeft.Add(customButton.Key); break;
                        case X360Controls.DpadRight: DpadRight.Add(customButton.Key); break;
                        case X360Controls.Start: Options.Add(customButton.Key); break;
                        case X360Controls.Guide: PS.Add(customButton.Key); break;
                        case X360Controls.Back: Share.Add(customButton.Key); break;
                        case X360Controls.LXNeg: LXN.Add(customButton.Key); break;
                        case X360Controls.LYNeg: LYN.Add(customButton.Key); break;
                        case X360Controls.RXNeg: RXN.Add(customButton.Key); break;
                        case X360Controls.RYNeg: RYN.Add(customButton.Key); break;
                        case X360Controls.LXPos: LXP.Add(customButton.Key); break;
                        case X360Controls.LYPos: LYP.Add(customButton.Key); break;
                        case X360Controls.RXPos: RXP.Add(customButton.Key); break;
                        case X360Controls.RYPos: RYP.Add(customButton.Key); break;
                        case X360Controls.LT: L2.Add(customButton.Key); Console.WriteLine("yes"); break;
                        case X360Controls.RT: R2.Add(customButton.Key); break;
                        case X360Controls.LeftMouse:
                            if (getBoolMapping(customButton.Key, cState, eState, tp))
                                deviceState.currentClicks.leftCount++;
                            break;
                        case X360Controls.RightMouse:
                            if (getBoolMapping(customButton.Key, cState, eState, tp))
                                deviceState.currentClicks.rightCount++;
                            break;
                        case X360Controls.MiddleMouse:
                            if (getBoolMapping(customButton.Key, cState, eState, tp))
                                deviceState.currentClicks.middleCount++;
                            break;
                        case X360Controls.FourthMouse:
                            if (getBoolMapping(customButton.Key, cState, eState, tp))
                                deviceState.currentClicks.fourthCount++;
                            break;
                        case X360Controls.FifthMouse:
                            if (getBoolMapping(customButton.Key, cState, eState, tp))
                                deviceState.currentClicks.fifthCount++;
                            break;
                        case X360Controls.WUP:
                            if (getBoolMapping(customButton.Key, cState, eState, tp))
                                deviceState.currentClicks.wUpCount++;
                            break;
                        case X360Controls.WDOWN:
                            if (getBoolMapping(customButton.Key, cState, eState, tp))
                                deviceState.currentClicks.wDownCount++;
                            break;
                        case X360Controls.MouseUp:
                            if (MouseDeltaY == 0)
                            {
                                MouseDeltaY = getMouseMapping(device, customButton.Key, cState, eState, 0);
                                MouseDeltaY = -Math.Abs((MouseDeltaY == -2147483648 ? 0 : MouseDeltaY));
                            }
                            break;
                        case X360Controls.MouseDown:
                            if (MouseDeltaY == 0)
                            {
                                MouseDeltaY = getMouseMapping(device, customButton.Key, cState, eState, 1);
                                MouseDeltaY = Math.Abs((MouseDeltaY == -2147483648 ? 0 : MouseDeltaY));
                            }
                            break;
                        case X360Controls.MouseLeft:
                            if (MouseDeltaX == 0)
                            {
                                MouseDeltaX = getMouseMapping(device, customButton.Key, cState, eState, 2);
                                MouseDeltaX = -Math.Abs((MouseDeltaX == -2147483648 ? 0 : MouseDeltaX));
                            }
                            break;
                        case X360Controls.MouseRight:
                            if (MouseDeltaX == 0)
                            {
                                MouseDeltaX = getMouseMapping(device, customButton.Key, cState, eState, 3);
                                MouseDeltaX = Math.Abs((MouseDeltaX == -2147483648 ? 0 : MouseDeltaX));
                            }
                            break;
                    }
                }
            if (macroControl[0]) MappedState.Cross = true;
            if (macroControl[1]) MappedState.Circle = true;
            if (macroControl[2]) MappedState.Square = true;
            if (macroControl[3]) MappedState.Triangle = true;
            if (macroControl[4]) MappedState.Options = true;
            if (macroControl[5]) MappedState.Share = true;
            if (macroControl[6]) MappedState.DpadUp = true;
            if (macroControl[7]) MappedState.DpadDown = true;
            if (macroControl[8]) MappedState.DpadLeft = true;
            if (macroControl[9]) MappedState.DpadRight = true;
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
            foreach (DS4Controls dc in Cross)
                if (getBoolMapping(dc, cState, eState, tp))
                    MappedState.Cross = true;
            foreach (DS4Controls dc in Circle)
                if (getBoolMapping(dc, cState, eState, tp))
                    MappedState.Circle = true;
            foreach (DS4Controls dc in Square)
                if (getBoolMapping(dc, cState, eState, tp))
                    MappedState.Square = true;
            foreach (DS4Controls dc in Triangle)
                if (getBoolMapping(dc, cState, eState, tp))
                    MappedState.Triangle = true;
            foreach (DS4Controls dc in L1)
                if (getBoolMapping(dc, cState, eState, tp))
                    MappedState.L1 = true;
            foreach (DS4Controls dc in L2)
                if (getByteMapping(device, dc, cState, eState, tp) != 0)
                    MappedState.L2 = getByteMapping(device, dc, cState, eState, tp);
            foreach (DS4Controls dc in L3)
                if (getBoolMapping(dc, cState, eState, tp))
                    MappedState.L3 = true;
            foreach (DS4Controls dc in R1)
                if (getBoolMapping(dc, cState, eState, tp))
                    MappedState.R1 = true;
            foreach (DS4Controls dc in R2)
                  if (getByteMapping(device, dc, cState, eState, tp) != 0)
                    MappedState.R2 = getByteMapping(device, dc, cState, eState, tp);
            foreach (DS4Controls dc in R3)
                if (getBoolMapping(dc, cState, eState, tp))
                    MappedState.R3 = true;
            foreach (DS4Controls dc in DpadUp)
                if (getBoolMapping(dc, cState, eState, tp))
                    MappedState.DpadUp = true;
            foreach (DS4Controls dc in DpadRight)
                if (getBoolMapping(dc, cState, eState, tp))
                    MappedState.DpadRight = true;
            foreach (DS4Controls dc in DpadLeft)
                if (getBoolMapping(dc, cState, eState, tp))
                    MappedState.DpadLeft = true;
            foreach (DS4Controls dc in DpadDown)
                if (getBoolMapping(dc, cState, eState, tp))
                    MappedState.DpadDown = true;
            foreach (DS4Controls dc in Options)
                if (getBoolMapping(dc, cState, eState, tp))
                    MappedState.Options = true;
            foreach (DS4Controls dc in Share)
                if (getBoolMapping(dc, cState, eState, tp))
                    MappedState.Share = true;
            foreach (DS4Controls dc in PS)
                if (getBoolMapping(dc, cState, eState, tp))
                    MappedState.PS = true;

            if (Global.getShiftCustomButton(device, DS4Controls.LXNeg) == X360Controls.None)
                LXN.Add(DS4Controls.LXNeg);
            if (Global.getShiftCustomButton(device, DS4Controls.LXPos) == X360Controls.None)
                LXP.Add(DS4Controls.LXPos);
            if (Global.getShiftCustomButton(device, DS4Controls.LYNeg) == X360Controls.None)
                LYN.Add(DS4Controls.LYNeg);
            if (Global.getShiftCustomButton(device, DS4Controls.LYPos) == X360Controls.None)
                LYP.Add(DS4Controls.LYPos);
            if (Global.getShiftCustomButton(device, DS4Controls.RXNeg) == X360Controls.None)
                RXN.Add(DS4Controls.RXNeg);
            if (Global.getShiftCustomButton(device, DS4Controls.RXPos) == X360Controls.None)
                RXP.Add(DS4Controls.RXPos);
            if (Global.getShiftCustomButton(device, DS4Controls.RYNeg) == X360Controls.None)
                RYN.Add(DS4Controls.RYNeg);
            if (Global.getShiftCustomButton(device, DS4Controls.RYPos) == X360Controls.None)
                RYP.Add(DS4Controls.RYPos);
            if (LXN.Count > 0 || LXP.Count > 0)
            {
                foreach (DS4Controls dc in LXP)
                    if (Math.Abs(127 - getXYAxisMapping(device, dc, cState, eState, tp, true)) > 5)
                        MappedState.LX = getXYAxisMapping(device, dc, cState, eState, tp, true);
                foreach (DS4Controls dc in LXN)
                    if (Math.Abs(127 - getXYAxisMapping(device, dc, cState, eState, tp)) > 5)
                        MappedState.LX = getXYAxisMapping(device, dc, cState, eState, tp);
            }
            else
                MappedState.LX = cState.LX;
            if (LYN.Count > 0 || LYP.Count > 0)
            {
                foreach (DS4Controls dc in LYN)
                    if (Math.Abs(127 - getXYAxisMapping(device, dc, cState, eState, tp)) > 5)
                        MappedState.LY = getXYAxisMapping(device, dc, cState, eState, tp);
                foreach (DS4Controls dc in LYP)
                    if (Math.Abs(127 - getXYAxisMapping(device, dc, cState, eState, tp, true)) > 5)
                        MappedState.LY = getXYAxisMapping(device, dc, cState, eState, tp, true);
            }
            else
                MappedState.LY = cState.LY;
            if (RXN.Count > 0 || RXP.Count > 0)
            {
                foreach (DS4Controls dc in RXN)
                    if (Math.Abs(127 - getXYAxisMapping(device, dc, cState, eState, tp)) > 5)
                        MappedState.RX = getXYAxisMapping(device, dc, cState, eState, tp);
                foreach (DS4Controls dc in RXP)
                    if (Math.Abs(127 - getXYAxisMapping(device, dc, cState, eState, tp, true)) > 5)
                        MappedState.RX = getXYAxisMapping(device, dc, cState, eState, tp, true);
            }
            else
                MappedState.RX = cState.RX;
            if (RYN.Count > 0 || RYP.Count > 0)
            {
                foreach (DS4Controls dc in RYN)
                    if (Math.Abs(127 - getXYAxisMapping(device, dc, cState, eState, tp)) > 5)
                        MappedState.RY = getXYAxisMapping(device, dc, cState, eState, tp);
                foreach (DS4Controls dc in RYP)
                    if (Math.Abs(127 - getXYAxisMapping(device, dc, cState, eState, tp, true)) > 5)
                        MappedState.RY = getXYAxisMapping(device, dc, cState, eState, tp, true);
            }
            else
                MappedState.RY = cState.RY;
            InputMethods.MoveCursorBy(MouseDeltaX, MouseDeltaY);
        }

        public static bool[,] actionDone = new bool[4,50];
        public static SpecialAction[] untriggeraction = new SpecialAction[4];
        public static int[] untriggerindex = new int[4];
        public static async void MapCustomAction(int device, DS4State cState, DS4State MappedState, DS4StateExposed eState, Mouse tp, Control ctrl)
        {
            foreach (string actionname in Global.GetProfileActions(device))
            {
                //DS4KeyType keyType = Global.getShiftCustomKeyType(device, customKey.Key);
                SpecialAction action = Global.GetAction(actionname);
                int index = Global.GetActionIndexOf(actionname);
                if (action.name == "null" || index < 0)
                    return;
                bool triggeractivated = true;
                foreach (DS4Controls dc in action.trigger)
                {
                    if (!getBoolMapping(dc, cState, eState, tp))
                    {
                        triggeractivated = false;
                        break;
                    }
                }
                //if (triggeractivated)
                if (triggeractivated && action.type == "Program")
                {
                    if (!actionDone[device,index])
                    {
                        actionDone[device,index] = true;
                        Process.Start(action.details);
                    }
                }
                else if (triggeractivated && action.type == "Profile")
                {
                    if (!actionDone[device,index] && string.IsNullOrEmpty(Global.tempprofilename[device]))
                    {
                        actionDone[device, index] = true;
                        untriggeraction[device] = action;
                        untriggerindex[device] = index;
                        foreach (DS4Controls dc in action.trigger)
                        {
                            InputMethods.performKeyRelease(Global.getCustomKey(0, dc));
                            string[] skeys = Global.getCustomMacro(0, dc).Split('/');
                            ushort[] keys = new ushort[skeys.Length];
                            for (int i = 0; i < keys.Length; i++)
                            {
                                keys[i] = ushort.Parse(skeys[i]);
                                InputMethods.performKeyRelease(keys[i]);
                            }
                        }
                        Global.LoadTempProfile(device, action.details, true, ctrl);
                        return;
                    }
                }                
                else if (triggeractivated && action.type == "Macro")
                {
                    if (!actionDone[device,index])
                    {
                        actionDone[device,index] = true;
                        foreach (DS4Controls dc in action.trigger)
                            resetToDefaultValue(dc, MappedState);
                        bool[] keydown = new bool[286];
                        //for (int i = 0; i < keys.Length; i++)
                        foreach (int i in action.macro)
                        {
                            if (i >= 300) //ints over 300 used to delay
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
                                //else if (keyType.HasFlag(DS4KeyType.ScanCode))
                                    //InputMethods.performSCKeyPress((ushort)i);
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
                                //else if (keyType.HasFlag(DS4KeyType.ScanCode))
                                    //InputMethods.performSCKeyRelease((ushort)i);
                                else
                                    InputMethods.performKeyRelease((ushort)i);
                                keydown[i] = false;
                            }
                        }
                        for (ushort i = 0; i < keydown.Length; i++)
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
                                //else if (keyType.HasFlag(DS4KeyType.ScanCode))
                                    //InputMethods.performSCKeyRelease(i);
                                else
                                    InputMethods.performKeyRelease(i);
                        }
                        /*if (keyType.HasFlag(DS4KeyType.HoldMacro))
                        {
                            await Task.Delay(50);
                            macrodoneA[index] = false;
                        }*/
                    }
                }
                else
                    actionDone[device,index] = false;
            }
            if (untriggeraction[device] != null)
            {
                SpecialAction action = untriggeraction[device];
                int index = untriggerindex[device];
                bool utriggeractivated = true;
                foreach (DS4Controls dc in action.uTrigger)
                {
                    if (!getBoolMapping(dc, cState, eState, tp))
                    {
                        utriggeractivated = false;
                        break;
                    }
                }

                if (utriggeractivated && action.type == "Profile")
                {
                    if ((action.controls == action.ucontrols && !actionDone[device, index]) || //if trigger and end trigger are the same
                    action.controls != action.ucontrols)
                        if (!string.IsNullOrEmpty(Global.tempprofilename[device]))
                        {
                            foreach (DS4Controls dc in action.uTrigger)
                            {
                                actionDone[device, index] = true;
                                InputMethods.performKeyRelease(Global.getCustomKey(0, dc));
                                string[] skeys = Global.getCustomMacro(0, dc).Split('/');
                                ushort[] keys = new ushort[skeys.Length];
                                for (int i = 0; i < keys.Length; i++)
                                {
                                    keys[i] = ushort.Parse(skeys[i]);
                                    InputMethods.performKeyRelease(keys[i]);
                                }
                            }
                            untriggeraction[device] = null;
                            Global.LoadProfile(device, false, ctrl);
                        }
                }
                else
                    actionDone[device, index] = false;
            }
        }
        public static DateTime[] mousenow = { DateTime.UtcNow, DateTime.UtcNow, DateTime.UtcNow, DateTime.UtcNow };
        public static double mvalue = 0;
        public static int[] mouseaccel = new int[34];
        public static bool[] mousedoublecheck = new bool[34];
        private static int getMouseMapping(int device, DS4Controls control, DS4State cState, DS4StateExposed eState, int mnum)
        {
            int controlnum = DS4ControltoInt(control);
            double SXD = Global.getSXDeadzone(device);
            double SZD = Global.getSZDeadzone(device);
            int deadzoneL = 10;
            int deadzoneR = 10;
            if (Global.getLSDeadzone(device) >= 10)
                deadzoneL = 0;
            if (Global.getRSDeadzone(device) >= 10)
                deadzoneR = 0;
            double value = 0;
            int speed = Global.getButtonMouseSensitivity(device)+15;
            double root = 1.002;
            double divide = 10000d;
            DateTime now = mousenow[mnum];
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
                case DS4Controls.GyroXPos: return (byte)(eState.GyroX > SXD * 7500 ?
                    Math.Pow(root + speed / divide, eState.GyroX / 62) : 0);
                case DS4Controls.GyroXNeg: return (byte)(eState.GyroX < -SXD * 7500 ?
                    Math.Pow(root + speed / divide, -eState.GyroX / 48) : 0);
                case DS4Controls.GyroZPos: return (byte)(eState.GyroZ > SZD * 7500 ?
                    Math.Pow(root + speed / divide, eState.GyroZ / 62) : 0);
                case DS4Controls.GyroZNeg: return (byte)(eState.GyroZ < -SZD * 7500 ?
                    Math.Pow(root + speed / divide, -eState.GyroZ / 62) : 0);
            }
            bool LXChanged = (Math.Abs(127 - cState.LX) < deadzoneL);
            bool LYChanged = (Math.Abs(127 - cState.LY) < deadzoneL);
            bool RXChanged = (Math.Abs(127 - cState.RX) < deadzoneR);
            bool RYChanged = (Math.Abs(127 - cState.RY) < deadzoneR);
            bool contains = (control.ToString().Contains("LX") ||
                control.ToString().Contains("LY") ||
                control.ToString().Contains("RX") ||
                    control.ToString().Contains("RY"));
            if (LXChanged || LYChanged || RXChanged || RYChanged)
                now = DateTime.UtcNow;
            if (Global.getMouseAccel(device))
            {
                if (value > 0)
                    mouseaccel[controlnum]++;
                else if (!mousedoublecheck[controlnum])
                    mouseaccel[controlnum] = 0;
                mousedoublecheck[controlnum] = value != 0;
                if (mouseaccel[controlnum] > 1000)
                    value *= (double)Math.Min(2000, (mouseaccel[controlnum])) / 1000d;
            }
            if (value != 0)
            if (contains && value <= 1 && value != 0)
            {
                if (now >= mousenow[mnum] + TimeSpan.FromMilliseconds((1 - value) * 500))
                {
                    mousenow[mnum] = now;
                    return 1;
                }
                else
                    return 0;
            }
            else
                return (int)value;
            return 0;
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
            double SXD = Global.getSXDeadzone(device);
            double SZD = Global.getSZDeadzone(device);
            switch (control)
            {
                case DS4Controls.Share: return (byte)(cState.Share ? 255 : 0);
                case DS4Controls.Options: return (byte)(cState.Options ? 255 : 0);
                case DS4Controls.L1: return (byte)(cState.L1 ? 255 : 0);
                case DS4Controls.R1: return (byte)(cState.R1 ? 255 : 0);
                case DS4Controls.L3: return (byte)(cState.L3 ? 255 : 0);
                case DS4Controls.R3: return (byte)(cState.R3 ? 255 : 0);
                case DS4Controls.DpadUp: return (byte)(cState.DpadUp ? 255 : 0);
                case DS4Controls.DpadDown: return (byte)(cState.DpadDown ? 255 : 0);
                case DS4Controls.DpadLeft: return (byte)(cState.DpadLeft ? 255 : 0);
                case DS4Controls.DpadRight: return (byte)(cState.DpadRight ? 255 : 0);
                case DS4Controls.PS: return (byte)(cState.PS ? 255 : 0);
                case DS4Controls.Cross: return (byte)(cState.Cross ? 255 : 0);
                case DS4Controls.Square: return (byte)(cState.Square ? 255 : 0);
                case DS4Controls.Triangle: return (byte)(cState.Triangle ? 255 : 0);
                case DS4Controls.Circle: return (byte)(cState.Circle ? 255 : 0);
                case DS4Controls.TouchLeft: return (byte)(tp != null && tp.leftDown ? 255 : 0);
                case DS4Controls.TouchRight: return (byte)(tp != null && tp.rightDown ? 255 : 0);
                case DS4Controls.TouchMulti: return (byte)(tp != null && tp.multiDown ? 255 : 0);
                case DS4Controls.TouchUpper: return (byte)(tp != null && tp.upperDown ? 255 : 0);
                case DS4Controls.LXNeg: return (byte)(cState.LX - 127.5f > 0 ? 0 : -(cState.LX - 127.5f) * 2);
                case DS4Controls.LYNeg: return (byte)(cState.LY - 127.5f > 0 ? 0 : -(cState.LY - 127.5f) * 2);
                case DS4Controls.RXNeg: return (byte)(cState.RX - 127.5f > 0 ? 0 : -(cState.RX - 127.5f) * 2);
                case DS4Controls.RYNeg: return (byte)(cState.RY - 127.5f > 0 ? 0 : -(cState.RY - 127.5f) * 2);
                case DS4Controls.LXPos: return (byte)(cState.LX - 127.5f < 0 ? 0 : (cState.LX - 127.5f) * 2);
                case DS4Controls.LYPos: return (byte)(cState.LY - 127.5f < 0 ? 0 : (cState.LY - 127.5f) * 2);
                case DS4Controls.RXPos: return (byte)(cState.RX - 127.5f < 0 ? 0 : (cState.RX - 127.5f) * 2);
                case DS4Controls.RYPos: return (byte)(cState.RY - 127.5f < 0 ? 0 : (cState.RY - 127.5f) * 2);
                case DS4Controls.L2: return cState.L2;
                case DS4Controls.R2: return cState.R2;
                case DS4Controls.GyroXPos: return (byte)(eState.GyroX > SXD * 7500 ? Math.Min(255, eState.GyroX / 31) : 0);
                case DS4Controls.GyroXNeg: return (byte)(eState.GyroX < -SXD * 7500 ? Math.Min(255, -eState.GyroX / 31) : 0);
                case DS4Controls.GyroZPos: return (byte)(eState.GyroZ > SZD * 7500 ? Math.Min(255, eState.GyroZ / 31) : 0);
                case DS4Controls.GyroZNeg: return (byte)(eState.GyroZ < -SZD * 7500 ? Math.Min(255, -eState.GyroZ / 31) : 0);
                case DS4Controls.SwipeUp: return (byte)(tp != null ? tp.swipeUpB : 0);
                case DS4Controls.SwipeDown: return (byte)(tp != null ? tp.swipeDownB: 0);
                case DS4Controls.SwipeLeft: return (byte)(tp != null ?  tp.swipeLeftB: 0);
                case DS4Controls.SwipeRight: return (byte)(tp != null ? tp.swipeRightB : 0);
            }
            return 0;
        }

        public static bool getBoolMapping(DS4Controls control, DS4State cState, DS4StateExposed eState, Mouse tp)
        {
            //if (control == DS4Controls.Up)
            //Cons
            switch (control)
            {
                case DS4Controls.Share: return cState.Share;
                case DS4Controls.Options: return cState.Options;
                case DS4Controls.L1: return cState.L1;
                case DS4Controls.R1: return cState.R1;
                case DS4Controls.L3: return cState.L3;
                case DS4Controls.R3: return cState.R3;
                case DS4Controls.DpadUp: return cState.DpadUp;
                case DS4Controls.DpadDown: return cState.DpadDown;
                case DS4Controls.DpadLeft: return cState.DpadLeft;
                case DS4Controls.DpadRight: return cState.DpadRight;
                case DS4Controls.PS: return cState.PS;
                case DS4Controls.Cross: return cState.Cross;
                case DS4Controls.Square: return cState.Square;
                case DS4Controls.Triangle: return cState.Triangle;
                case DS4Controls.Circle: return cState.Circle;
                case DS4Controls.TouchLeft: return (tp != null ? tp.leftDown : false);
                case DS4Controls.TouchRight: return (tp != null ? tp.rightDown : false);
                case DS4Controls.TouchMulti: return (tp != null ? tp.multiDown : false);
                case DS4Controls.TouchUpper: return (tp != null ? tp.upperDown : false);
                case DS4Controls.LXNeg: return cState.LX < 127 - 55;
                case DS4Controls.LYNeg: return cState.LY < 127 - 55;
                case DS4Controls.RXNeg: return cState.RX < 127 - 55;
                case DS4Controls.RYNeg: return cState.RY < 127 - 55;
                case DS4Controls.LXPos: return cState.LX > 127 + 55;
                case DS4Controls.LYPos: return cState.LY > 127 + 55;
                case DS4Controls.RXPos: return cState.RX > 127 + 55;
                case DS4Controls.RYPos: return cState.RY > 127 + 55;
                case DS4Controls.L2: return cState.L2 > 100;
                case DS4Controls.R2: return cState.R2 > 100;
                case DS4Controls.GyroXPos: return eState.GyroX > 5000;
                case DS4Controls.GyroXNeg: return eState.GyroX < -5000;
                case DS4Controls.GyroZPos: return eState.GyroZ > 5000;
                case DS4Controls.GyroZNeg: return eState.GyroZ < -5000;
                case DS4Controls.SwipeUp: return (tp != null && tp.swipeUp);
                case DS4Controls.SwipeDown: return (tp != null && tp.swipeDown);
                case DS4Controls.SwipeLeft: return (tp != null && tp.swipeLeft);
                case DS4Controls.SwipeRight: return (tp != null && tp.swipeRight);
            }
            return false;
        }

        public static byte getXYAxisMapping(int device, DS4Controls control, DS4State cState, DS4StateExposed eState, Mouse tp, bool alt = false)
        {
            byte trueVal = 0;
            byte falseVal = 127;
            double SXD = Global.getSXDeadzone(device);
            double SZD = Global.getSZDeadzone(device);
            if (alt)
                trueVal = 255;
            switch (control)
            {
                case DS4Controls.Share: return (byte)(cState.Share ? trueVal : falseVal);
                case DS4Controls.Options: return (byte)(cState.Options ? trueVal : falseVal);
                case DS4Controls.L1: return (byte)(cState.L1 ? trueVal : falseVal);
                case DS4Controls.R1: return (byte)(cState.R1 ? trueVal : falseVal);
                case DS4Controls.L3: return (byte)(cState.L3 ? trueVal : falseVal);
                case DS4Controls.R3: return (byte)(cState.R3 ? trueVal : falseVal);
                case DS4Controls.DpadUp: return (byte)(cState.DpadUp ? trueVal : falseVal);
                case DS4Controls.DpadDown: return (byte)(cState.DpadDown ? trueVal : falseVal);
                case DS4Controls.DpadLeft: return (byte)(cState.DpadLeft ? trueVal : falseVal);
                case DS4Controls.DpadRight: return (byte)(cState.DpadRight ? trueVal : falseVal);
                case DS4Controls.PS: return (byte)(cState.PS ? trueVal : falseVal);
                case DS4Controls.Cross: return (byte)(cState.Cross ? trueVal : falseVal);
                case DS4Controls.Square: return (byte)(cState.Square ? trueVal : falseVal);
                case DS4Controls.Triangle: return (byte)(cState.Triangle ? trueVal : falseVal);
                case DS4Controls.Circle: return (byte)(cState.Circle ? trueVal : falseVal);
                case DS4Controls.TouchLeft: return (byte)(tp != null && tp.leftDown ? trueVal : falseVal);
                case DS4Controls.TouchRight: return (byte)(tp != null && tp.rightDown ? trueVal : falseVal);
                case DS4Controls.TouchMulti: return (byte)(tp != null && tp.multiDown ? trueVal : falseVal);
                case DS4Controls.TouchUpper: return (byte)(tp != null && tp.upperDown ? trueVal : falseVal);
                case DS4Controls.L2: if (alt) return (byte)(127.5f + cState.L2 / 2f); else return (byte)(127.5f - cState.L2 / 2f);
                case DS4Controls.R2: if (alt) return (byte)(127.5f + cState.R2 / 2f); else return (byte)(127.5f - cState.R2 / 2f);
                case DS4Controls.SwipeUp: if (alt) return (byte)(tp != null ? 127.5f + tp.swipeUpB / 2f : 0); else return (byte)(tp != null ? 127.5f - tp.swipeUpB / 2f : 0);
                case DS4Controls.SwipeDown: if (alt) return (byte)(tp != null ? 127.5f + tp.swipeDownB / 2f : 0); else return (byte)(tp != null ? 127.5f - tp.swipeDownB / 2f : 0);
                case DS4Controls.SwipeLeft: if (alt) return (byte)(tp != null ? 127.5f + tp.swipeLeftB / 2f : 0); else return (byte)(tp != null ? 127.5f - tp.swipeLeftB / 2f : 0);
                case DS4Controls.SwipeRight: if (alt) return (byte)(tp != null ? 127.5f + tp.swipeRightB / 2f : 0); else return (byte)(tp != null ? 127.5f - tp.swipeRightB / 2f : 0);
                case DS4Controls.GyroXPos: if (eState.GyroX > SXD * 7500)
                        if (alt) return (byte)Math.Min(255, 127 + eState.GyroX / 62); else return (byte)Math.Max(0, 127 - eState.GyroX / 62);
                    else return falseVal;
                case DS4Controls.GyroXNeg: if (eState.GyroX < -SXD * 7500)
                        if (alt) return (byte)Math.Min(255, 127 + -eState.GyroX / 62); else return (byte)Math.Max(0, 127 - -eState.GyroX / 62);
                    else return falseVal;
                case DS4Controls.GyroZPos: if (eState.GyroZ > SZD * 7500)
                        if (alt) return (byte)Math.Min(255, 127 + eState.GyroZ / 62); else return (byte)Math.Max(0, 127 - eState.GyroZ / 62);
                    else return falseVal;
                case DS4Controls.GyroZNeg: if (eState.GyroZ < -SZD * 7500)
                        if (alt) return (byte)Math.Min(255, 127 + -eState.GyroZ / 62); else return (byte)Math.Max(0, 127 - -eState.GyroZ / 62);
                    else return falseVal;
            }            
            if (!alt)
            {
                switch (control)
                {
                    case DS4Controls.LXNeg: return cState.LX;
                    case DS4Controls.LYNeg: return cState.LY;
                    case DS4Controls.RXNeg: return cState.RX;
                    case DS4Controls.RYNeg: return cState.RY;
                    case DS4Controls.LXPos: return (byte)(255 - cState.LX);
                    case DS4Controls.LYPos: return (byte)(255 - cState.LY);
                    case DS4Controls.RXPos: return (byte)(255 - cState.RX);
                    case DS4Controls.RYPos: return (byte)(255 - cState.RY);
                }
            }
            else
            {
                switch (control)
                {
                    case DS4Controls.LXNeg: return (byte)(255 - cState.LX);
                    case DS4Controls.LYNeg: return (byte)(255 - cState.LY);
                    case DS4Controls.RXNeg: return (byte)(255 - cState.RX);
                    case DS4Controls.RYNeg: return (byte)(255 - cState.RY);
                    case DS4Controls.LXPos: return cState.LX;
                    case DS4Controls.LYPos: return cState.LY;
                    case DS4Controls.RXPos: return cState.RX;
                    case DS4Controls.RYPos: return cState.RY;
                }
            }
            return 0;
        }

        //Returns false for any bool, 
        //if control is one of the xy axis returns 127
        //if its a trigger returns 0
        public static void resetToDefaultValue(DS4Controls control, DS4State cState)
        {
            switch (control)
            {
                case DS4Controls.Share: cState.Share = false; break;
                case DS4Controls.Options: cState.Options = false; break;
                case DS4Controls.L1: cState.L1 = false; break;
                case DS4Controls.R1: cState.R1 = false; break;
                case DS4Controls.L3: cState.L3 = false; break;
                case DS4Controls.R3: cState.R3 = false; break;
                case DS4Controls.DpadUp: cState.DpadUp = false; break;
                case DS4Controls.DpadDown: cState.DpadDown = false; break;
                case DS4Controls.DpadLeft: cState.DpadLeft = false; break;
                case DS4Controls.DpadRight: cState.DpadRight = false; break;
                case DS4Controls.PS: cState.PS = false; break;
                case DS4Controls.Cross: cState.Cross = false; break;
                case DS4Controls.Square: cState.Square = false; break;
                case DS4Controls.Triangle: cState.Triangle = false; break;
                case DS4Controls.Circle: cState.Circle = false; break;
                case DS4Controls.LXNeg: cState.LX = 127; break;
                case DS4Controls.LYNeg: cState.LY = 127; break;
                case DS4Controls.RXNeg: cState.RX = 127; break;
                case DS4Controls.RYNeg: cState.RY = 127; break;
                case DS4Controls.LXPos: cState.LX = 127; break;
                case DS4Controls.LYPos: cState.LY = 127; break;
                case DS4Controls.RXPos: cState.RX = 127; break;
                case DS4Controls.RYPos: cState.RY = 127; break;
                case DS4Controls.L2: cState.L2 = 0; break;
                case DS4Controls.R2: cState.R2 = 0; break;
            }
        }
    }
}
