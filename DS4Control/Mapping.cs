using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DS4Library;
namespace DS4Control
{
    public class Mapping
    {
        /*
         * Represent the synthetic keyboard and mouse events.  Maintain counts for each so we don't duplicate events.
         */
        private class SyntheticState
        {
            public struct MouseClick
            {
                public int leftCount, middleCount, rightCount, fourthCount, fifthCount, wUpCount, wDownCount;
            }
            public MouseClick previousClicks, currentClicks;
            public struct KeyPress
            {
                public int vkCount, scanCodeCount, repeatCount; // repeat takes priority over non-, and scancode takes priority over non-
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
                    currentClicks.leftCount = currentClicks.middleCount = currentClicks.rightCount = currentClicks.fourthCount = currentClicks.fifthCount = currentClicks.wUpCount = currentClicks.wDownCount = 0;
                foreach (KeyPresses kp in keyPresses.Values)
                {
                    kp.previous = kp.current;
                    if (performClear)
                        kp.current.repeatCount = kp.current.scanCodeCount = kp.current.vkCount = 0;
                }
            }
        }
        private static SyntheticState globalState = new SyntheticState();
        private static SyntheticState[] deviceState = { new SyntheticState(), new SyntheticState(), new SyntheticState(), new SyntheticState() };

        // TODO When we disconnect, process a null/dead state to release any keys or buttons.
        public static DateTime oldnow = DateTime.Now;
        private static bool pressagain = false;
        private static int wheel = 0, keyshelddown = 0;
        public static void Commit(int device)
        {
            SyntheticState state = deviceState[device];
            lock (globalState)
            {
                globalState.currentClicks.leftCount += state.currentClicks.leftCount - state.previousClicks.leftCount;
                if (globalState.currentClicks.leftCount != 0 && globalState.previousClicks.leftCount == 0)
                    InputMethods.MouseEvent(InputMethods.MOUSEEVENTF_LEFTDOWN);
                else if (globalState.currentClicks.leftCount == 0 && globalState.previousClicks.leftCount != 0)
                    InputMethods.MouseEvent(InputMethods.MOUSEEVENTF_LEFTUP);

                globalState.currentClicks.middleCount += state.currentClicks.middleCount - state.previousClicks.middleCount;
                if (globalState.currentClicks.middleCount != 0 && globalState.previousClicks.middleCount == 0)
                    InputMethods.MouseEvent(InputMethods.MOUSEEVENTF_MIDDLEDOWN);
                else if (globalState.currentClicks.middleCount == 0 && globalState.previousClicks.middleCount != 0)
                    InputMethods.MouseEvent(InputMethods.MOUSEEVENTF_MIDDLEUP);
                
                globalState.currentClicks.rightCount += state.currentClicks.rightCount - state.previousClicks.rightCount;
                if (globalState.currentClicks.rightCount != 0 && globalState.previousClicks.rightCount == 0)
                    InputMethods.MouseEvent(InputMethods.MOUSEEVENTF_RIGHTDOWN);
                else if (globalState.currentClicks.rightCount == 0 && globalState.previousClicks.rightCount != 0)
                    InputMethods.MouseEvent(InputMethods.MOUSEEVENTF_RIGHTUP);

                globalState.currentClicks.fourthCount += state.currentClicks.fourthCount - state.previousClicks.fourthCount;
                if (globalState.currentClicks.fourthCount != 0 && globalState.previousClicks.fourthCount == 0)
                    InputMethods.MouseEvent(InputMethods.MOUSEEVENTF_XBUTTONDOWN, 1);
                else if (globalState.currentClicks.fourthCount == 0 && globalState.previousClicks.fourthCount != 0)
                    InputMethods.MouseEvent(InputMethods.MOUSEEVENTF_XBUTTONUP, 1);

                globalState.currentClicks.fifthCount += state.currentClicks.fifthCount - state.previousClicks.fifthCount;
                if (globalState.currentClicks.fifthCount != 0 && globalState.previousClicks.fifthCount == 0)
                    InputMethods.MouseEvent(InputMethods.MOUSEEVENTF_XBUTTONDOWN, 2);
                else if (globalState.currentClicks.fifthCount == 0 && globalState.previousClicks.fifthCount != 0)
                    InputMethods.MouseEvent(InputMethods.MOUSEEVENTF_XBUTTONUP, 2);

                globalState.currentClicks.wUpCount += state.currentClicks.wUpCount - state.previousClicks.wUpCount;
                if (globalState.currentClicks.wUpCount != 0 && globalState.previousClicks.wUpCount == 0)
                {
                    InputMethods.MouseEvent(InputMethods.MOUSEEVENTF_WHEEL, 100);
                    oldnow = DateTime.Now;
                    wheel = 100;
                }
                else if (globalState.currentClicks.wUpCount == 0 && globalState.previousClicks.wUpCount != 0)
                    wheel = 0;
                
                globalState.currentClicks.wDownCount += state.currentClicks.wDownCount - state.previousClicks.wDownCount;
                if (globalState.currentClicks.wDownCount != 0 && globalState.previousClicks.wDownCount == 0)
                {
                    InputMethods.MouseEvent(InputMethods.MOUSEEVENTF_WHEEL, -100);
                    oldnow = DateTime.Now;
                    wheel = -100;
                }
                if (globalState.currentClicks.wDownCount == 0 && globalState.previousClicks.wDownCount != 0)
                    wheel = 0;

                if (wheel != 0) //Continue mouse wheel movement
                {
                    DateTime now = DateTime.Now;
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
                    }
                    else
                    {
                        gkp = new SyntheticState.KeyPresses();
                        gkp.current = kvp.Value.current;
                        globalState.keyPresses[kvp.Key] = gkp;
                    }

                    if (gkp.current.vkCount + gkp.current.scanCodeCount != 0 && gkp.previous.vkCount + gkp.previous.scanCodeCount == 0)
                    {
                        if (gkp.current.scanCodeCount != 0)
                        {
                            oldnow = DateTime.Now;
                            InputMethods.performSCKeyPress(kvp.Key);
                            pressagain = false;
                            keyshelddown = kvp.Key;
                        }
                        else
                        {
                            oldnow = DateTime.Now;
                            InputMethods.performKeyPress(kvp.Key);
                            pressagain = false;
                            keyshelddown = kvp.Key;
                        }
                    }
                    else if (gkp.current.repeatCount != 0 || // repeat or SC/VK transition
                        ((gkp.previous.scanCodeCount == 0) != (gkp.current.scanCodeCount == 0)))
                    {
                        if (keyshelddown == kvp.Key)
                        {
                            DateTime now = DateTime.Now;
                            if (now >= oldnow + TimeSpan.FromMilliseconds(500) && !pressagain)
                            {
                                oldnow = now;
                                pressagain = true;
                            }
                            if (pressagain && gkp.current.scanCodeCount != 0)
                            {
                                now = DateTime.Now;
                                if (now >= oldnow + TimeSpan.FromMilliseconds(25) && pressagain)
                                {
                                    oldnow = now;
                                    InputMethods.performSCKeyPress(kvp.Key);
                                }                                
                            }
                            else if (pressagain)
                            {
                                now = DateTime.Now;
                                if (now >= oldnow + TimeSpan.FromMilliseconds(25) && pressagain)
                                {
                                    oldnow = now;
                                    InputMethods.performKeyPress(kvp.Key);
                                }
                            }
                        }

                    }
                    else if (gkp.current.vkCount + gkp.current.scanCodeCount == 0 && gkp.previous.vkCount + gkp.previous.scanCodeCount != 0)
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

        /** Map the touchpad button state to mouse or keyboard events. */
        public static void MapTouchpadButton(int device, DS4Controls what, Click mouseEventFallback, DS4State MappedState = null)
        {
            SyntheticState deviceState = Mapping.deviceState[device];
            ushort key = Global.getCustomKey(device, what);
            if (key != 0)
            {
  
                DS4KeyType keyType = Global.getCustomKeyType(device, what);
                SyntheticState.KeyPresses kp;
                if (!deviceState.keyPresses.TryGetValue(key, out kp))
                    deviceState.keyPresses[key] = kp = new SyntheticState.KeyPresses();
                if (keyType.HasFlag(DS4KeyType.ScanCode))
                    kp.current.scanCodeCount++;
                else
                    kp.current.vkCount++;
                if (keyType.HasFlag(DS4KeyType.Repeat))
                    kp.current.repeatCount++;
            } 
            else
            {
                X360Controls button = Global.getCustomButton(device, what);
                switch (button)
                {
                    case X360Controls.None:
                        switch (mouseEventFallback)
                        {
                            case Click.Left:
                                deviceState.currentClicks.leftCount++;
                                return;
                            case Click.Middle:
                                deviceState.currentClicks.middleCount++;
                                return;
                            case Click.Right:
                                deviceState.currentClicks.rightCount++;
                                return;
                            case Click.Fourth:
                                deviceState.currentClicks.fourthCount++;
                                return;
                            case Click.Fifth:
                                deviceState.currentClicks.fifthCount++;
                                return;
                            case Click.WUP:
                                deviceState.currentClicks.wUpCount++;
                                return;
                            case Click.WDOWN:
                                deviceState.currentClicks.wDownCount++;
                                return;
                        }
                        return;
                   case X360Controls.LeftMouse:
                        deviceState.currentClicks.leftCount++;
                        return;
                    case X360Controls.MiddleMouse:
                        deviceState.currentClicks.middleCount++;
                        return;
                    case X360Controls.RightMouse:
                        deviceState.currentClicks.rightCount++;
                        return;
                    case X360Controls.FourthMouse:
                        deviceState.currentClicks.fourthCount++;
                        return;
                    case X360Controls.FifthMouse:
                        deviceState.currentClicks.fifthCount++;
                        return;
                    case X360Controls.WUP:
                        deviceState.currentClicks.wUpCount++;
                        return;
                    case X360Controls.WDOWN:
                        deviceState.currentClicks.wDownCount++;
                        return;                  
                        
                    case X360Controls.A:
                        MappedState.Cross = true;
                        return;
                    case X360Controls.B:
                        MappedState.Circle = true;
                        return;
                    case X360Controls.X:
                        MappedState.Square = true;
                        return;
                    case X360Controls.Y:
                        MappedState.Triangle = true;
                        return;
                    case X360Controls.LB:
                        MappedState.L1 = true;
                        return;
                    case X360Controls.LS:
                        MappedState.L3 = true;
                        return;
                    case X360Controls.RB:
                        MappedState.R1 = true;
                        return;
                    case X360Controls.RS:
                        MappedState.R3 = true;
                        return;
                    case X360Controls.DpadUp:
                        MappedState.DpadUp = true;
                        return;
                    case X360Controls.DpadDown:
                        MappedState.DpadDown = true;
                        return;
                    case X360Controls.DpadLeft:
                        MappedState.DpadLeft = true;
                        return;
                    case X360Controls.DpadRight:
                        MappedState.DpadRight = true;
                        return;
                    case X360Controls.Guide:
                        MappedState.PS = true;
                        return;
                    case X360Controls.Back:
                        MappedState.Share = true;
                        return;
                    case X360Controls.Start:
                        MappedState.Options = true;
                        return;
                    case X360Controls.LT:
                        if (MappedState.L2 == 0)
                            MappedState.L2 = 255;
                        return;
                    case X360Controls.RT:
                        if (MappedState.R2 == 0)
                            MappedState.R2 = 255;
                        return;
                        
                    case X360Controls.Unbound:
                        return;

                    default:
                       if (MappedState == null)
                           return;
                        break; 
                }
            }
        }

        /** Map DS4 Buttons/Axes to other DS4 Buttons/Axes (largely the same as Xinput ones) and to keyboard and mouse buttons. */
        public static void MapCustom(int device, DS4State cState, DS4State MappedState, DS4State pState = null)
        {
            cState.CopyTo(MappedState);
            SyntheticState deviceState = Mapping.deviceState[device];
            foreach (KeyValuePair<DS4Controls, ushort> customKey in Global.getCustomKeys(device))
            {
                DS4KeyType keyType = Global.getCustomKeyType(device, customKey.Key);
                if (getBoolMapping(customKey.Key, cState))
                {
                    resetToDefaultValue(customKey.Key, MappedState);
                    SyntheticState.KeyPresses kp;
                    if (!deviceState.keyPresses.TryGetValue(customKey.Value, out kp))
                        deviceState.keyPresses[customKey.Value] = kp = new SyntheticState.KeyPresses();
                    if (keyType.HasFlag(DS4KeyType.ScanCode))
                        kp.current.scanCodeCount++;
                    else
                        kp.current.vkCount++;
                    //if (keyType.HasFlag(DS4KeyType.Repeat))
                        kp.current.repeatCount++;
                }
            }

            bool LX = false, LY = false, RX = false, RY = false;
            MappedState.LX = 127;
            MappedState.LY = 127;
            MappedState.RX = 127;
            MappedState.RY = 127;
            int MouseDeltaX = 0;
            int MouseDeltaY = 0;

            Dictionary<DS4Controls, X360Controls> customButtons = Global.getCustomButtons(device);
            foreach (KeyValuePair<DS4Controls, X360Controls> customButton in customButtons)
                resetToDefaultValue(customButton.Key, MappedState); // erase default mappings for things that are remapped
            foreach (KeyValuePair<DS4Controls, X360Controls> customButton in customButtons)
            {
                bool LXChanged = MappedState.LX == 127;
                bool LYChanged = MappedState.LY == 127;
                bool RXChanged = MappedState.RX == 127;
                bool RYChanged = MappedState.RY == 127;
                switch (customButton.Value)
                {
                    case X360Controls.A:
                        if (!MappedState.Cross)
                            MappedState.Cross = getBoolMapping(customButton.Key, cState);
                        break;
                    case X360Controls.B:
                        if (!MappedState.Circle)
                            MappedState.Circle = getBoolMapping(customButton.Key, cState);
                        break;
                    case X360Controls.X:
                        if (!MappedState.Square)
                            MappedState.Square = getBoolMapping(customButton.Key, cState);
                        break;
                    case X360Controls.Y:
                        if (!MappedState.Triangle)
                            MappedState.Triangle = getBoolMapping(customButton.Key, cState);
                        break;
                    case X360Controls.LB:
                        if (!MappedState.L1)
                            MappedState.L1 = getBoolMapping(customButton.Key, cState);
                        break;
                    case X360Controls.LS:
                        if (!MappedState.L3)
                            MappedState.L3 = getBoolMapping(customButton.Key, cState);
                        break;
                    case X360Controls.RB:
                        if (!MappedState.R1)
                            MappedState.R1 = getBoolMapping(customButton.Key, cState);
                        break;
                    case X360Controls.RS:
                        if (!MappedState.R3)
                            MappedState.R3 = getBoolMapping(customButton.Key, cState);
                        break;
                    case X360Controls.DpadUp:
                        if (!MappedState.DpadUp)
                            MappedState.DpadUp = getBoolMapping(customButton.Key, cState);
                        break;
                    case X360Controls.DpadDown:
                        if (!MappedState.DpadDown)
                            MappedState.DpadDown = getBoolMapping(customButton.Key, cState);
                        break;
                    case X360Controls.DpadLeft:
                        if (!MappedState.DpadLeft)
                            MappedState.DpadLeft = getBoolMapping(customButton.Key, cState);
                        break;
                    case X360Controls.DpadRight:
                        if (!MappedState.DpadRight)
                            MappedState.DpadRight = getBoolMapping(customButton.Key, cState);
                        break;
                    case X360Controls.Guide:
                        if (!MappedState.PS)
                            MappedState.PS = getBoolMapping(customButton.Key, cState);
                        break;
                    case X360Controls.Back:
                        if (!MappedState.Share)
                            MappedState.Share = getBoolMapping(customButton.Key, cState);
                        break;
                    case X360Controls.Start:
                        if (!MappedState.Options)
                            MappedState.Options = getBoolMapping(customButton.Key, cState);
                        break;
                    case X360Controls.LXNeg:
                        if (LXChanged)
                        {
                            MappedState.LX = getXYAxisMapping(customButton.Key, cState);
                            LX = true;
                        }
                        break;
                    case X360Controls.LYNeg:
                        if (LYChanged)
                        {
                            MappedState.LY = getXYAxisMapping(customButton.Key, cState);
                            LY = true;
                        }
                        break;
                    case X360Controls.RXNeg:
                        if (RXChanged)
                        {
                            MappedState.RX = getXYAxisMapping(customButton.Key, cState);
                            RX = true;
                        }
                        break;
                    case X360Controls.RYNeg:
                        if (RYChanged)
                        {
                            MappedState.RY = getXYAxisMapping(customButton.Key, cState);
                            RY = true;
                        }
                        break;
                    case X360Controls.LXPos:
                        if (LXChanged)
                        {
                            MappedState.LX = getXYAxisMapping(customButton.Key, cState, true);
                            LX = true;
                        }
                        break;
                    case X360Controls.LYPos:
                        if (LYChanged)
                        {
                            MappedState.LY = getXYAxisMapping(customButton.Key, cState, true);
                            LY = true;
                        }
                        break;
                    case X360Controls.RXPos:
                        if (RXChanged)
                        {
                            MappedState.RX = getXYAxisMapping(customButton.Key, cState, true);
                            RX = true;
                        }
                        break;
                    case X360Controls.RYPos:
                        if (RYChanged)
                        {
                            MappedState.RY = getXYAxisMapping(customButton.Key, cState, true);
                            RY = true;
                        }
                        break;
                    case X360Controls.LT:
                        MappedState.L2 = getByteMapping(customButton.Key, cState);
                        break;
                    case X360Controls.RT:
                        MappedState.R2 = getByteMapping(customButton.Key, cState);
                        break;
                    case X360Controls.LeftMouse:
                        if (getBoolMapping(customButton.Key, cState))
                            deviceState.currentClicks.leftCount++;
                        break;
                    case X360Controls.RightMouse:
                        if (getBoolMapping(customButton.Key, cState))
                            deviceState.currentClicks.rightCount++;
                        break;
                    case X360Controls.MiddleMouse:
                        if (getBoolMapping(customButton.Key, cState))
                            deviceState.currentClicks.middleCount++;
                        break;
                    case X360Controls.FourthMouse:
                        if (getBoolMapping(customButton.Key, cState))
                            deviceState.currentClicks.fourthCount++;
                        break;
                    case X360Controls.FifthMouse:
                        if (getBoolMapping(customButton.Key, cState))
                            deviceState.currentClicks.fifthCount++;
                        break;
                    case X360Controls.WUP:
                        if (getBoolMapping(customButton.Key, cState))
                            deviceState.currentClicks.wUpCount++;
                        break;
                    case X360Controls.WDOWN:
                        if (getBoolMapping(customButton.Key, cState))
                            deviceState.currentClicks.wDownCount++;
                        break;
                }
                if (pState != null)
                {
                    switch (customButton.Value)
                    {

                        case X360Controls.MouseUp:
                            if (MouseDeltaY == 0)
                            {
                                MouseDeltaY = calculateRelativeMouseDelta(device, customButton.Key, cState, pState);
                                MouseDeltaY = -Math.Abs(MouseDeltaY);
                            }
                            break;
                        case X360Controls.MouseDown:
                            if (MouseDeltaY == 0)
                            {
                                MouseDeltaY = calculateRelativeMouseDelta(device, customButton.Key, cState, pState);
                                MouseDeltaY = Math.Abs(MouseDeltaY);
                            }
                            break;
                        case X360Controls.MouseLeft:
                            if (MouseDeltaX == 0)
                            {
                                MouseDeltaX = calculateRelativeMouseDelta(device, customButton.Key, cState, pState);
                                MouseDeltaX = -Math.Abs(MouseDeltaX);
                            }
                            break;
                        case X360Controls.MouseRight:
                            if (MouseDeltaX == 0)
                            {
                                MouseDeltaX = calculateRelativeMouseDelta(device, customButton.Key, cState, pState);
                                MouseDeltaX = Math.Abs(MouseDeltaX);
                            }
                            break;
                    }
                }
            }

            if (!LX)
                MappedState.LX = cState.LX;
            if (!LY)
                MappedState.LY = cState.LY;
            if (!RX)
                MappedState.RX = cState.RX;
            if (!RY)
                MappedState.RY = cState.RY;
            InputMethods.MoveCursorBy(MouseDeltaX, MouseDeltaY);
        }

        private static int calculateRelativeMouseDelta(int device, DS4Controls control, DS4State cState, DS4State pState)
        {
            int axisVal = -1;
            int DEAD_ZONE = 10;
            float SPEED_MULTIPLIER = 0.000004f;
            bool positive = false;
            float deltaTime = cState.ReportTimeStamp.Ticks - pState.ReportTimeStamp.Ticks;
            switch (control)
            {
                case DS4Controls.LXNeg:
                    axisVal = cState.LX;
                    break;
                case DS4Controls.LXPos:
                    positive = true;
                    axisVal = cState.LX;
                    break;
                case DS4Controls.RXNeg:
                    axisVal = cState.RX;
                    break;
                case DS4Controls.RXPos:
                    positive = true;
                    axisVal = cState.RX;
                    break;
                case DS4Controls.LYNeg:
                    axisVal = cState.LY;
                    break;
                case DS4Controls.LYPos:
                    positive = true;
                    axisVal = cState.LY;
                    break;
                case DS4Controls.RYNeg:
                    axisVal = cState.RY;
                    break;
                case DS4Controls.RYPos:
                    positive = true;
                    axisVal = cState.RY;
                    break;
                case DS4Controls.Share: axisVal = (byte)(cState.Share ? -Global.getButtonMouseSensitivity(device) + 117 : -1); break;
                case DS4Controls.Options: axisVal = (byte)(cState.Options ? -Global.getButtonMouseSensitivity(device) + 117 : -1); break;
                case DS4Controls.L1: axisVal = (byte)(cState.L1 ? -Global.getButtonMouseSensitivity(device) + 117 : -1); break;
                case DS4Controls.R1: axisVal = (byte)(cState.R1 ? -Global.getButtonMouseSensitivity(device) + 117 : -1); break;
                case DS4Controls.L3: axisVal = (byte)(cState.L3 ? 117 - Global.getButtonMouseSensitivity(device) : -1); break;
                case DS4Controls.R3: axisVal = (byte)(cState.R3 ? 117 - Global.getButtonMouseSensitivity(device) : -1); break;
                case DS4Controls.DpadUp: axisVal = (byte)(cState.DpadUp ? -Global.getButtonMouseSensitivity(device) + 117 : -1); break;
                case DS4Controls.DpadDown: axisVal = (byte)(cState.DpadDown ? -Global.getButtonMouseSensitivity(device) + 117 : -1); break;
                case DS4Controls.DpadLeft: axisVal = (byte)(cState.DpadLeft ? -Global.getButtonMouseSensitivity(device) + 117 : -1); break;
                case DS4Controls.DpadRight: axisVal = (byte)(cState.DpadRight ? -Global.getButtonMouseSensitivity(device) + 117 : -1); break;
                case DS4Controls.PS: axisVal = (byte)(cState.PS ? -Global.getButtonMouseSensitivity(device) + 117 : -1); break;
                case DS4Controls.Cross: axisVal = (byte)(cState.Cross ? -Global.getButtonMouseSensitivity(device) + 117 : -1); break;
                case DS4Controls.Square: axisVal = (byte)(cState.Square ? -Global.getButtonMouseSensitivity(device) + 117 : -1); break;
                case DS4Controls.Triangle: axisVal = (byte)(cState.Triangle ? -Global.getButtonMouseSensitivity(device) + 117 : -1); break;
                case DS4Controls.Circle: axisVal = (byte)(cState.Circle ? -Global.getButtonMouseSensitivity(device) + 117 : -1); break;
                case DS4Controls.L2: positive = true; axisVal = cState.L2; break;
                case DS4Controls.R2: positive = true; axisVal = cState.R2; break;
            }
            axisVal = axisVal - 127;
            int delta = 0;
            if ((!positive && axisVal < -DEAD_ZONE) || (positive && axisVal > DEAD_ZONE))
            {
                delta = (int)(float)(axisVal * SPEED_MULTIPLIER * deltaTime);
            }
            return delta;
        }

        public static bool compare(byte b1, byte b2)
        {
            if (Math.Abs(b1 - b2) > 10)
            {
                return false;
            }
            return true;
        }

        static bool[] touchArea = { true, true, true, true };
        public static byte getByteMapping(DS4Controls control, DS4State cState)
        {
            if (!cState.TouchButton)
                for (int i = 0; i < 4; i++)
                    touchArea[i] = false;
            if (!(touchArea[0] || touchArea[1] || touchArea[2] || touchArea[3]))
            {
                if (cState.Touch2)
                    touchArea[0] = true;
                if (cState.TouchLeft && !cState.Touch2 && cState.Touch1)
                    touchArea[1] = true;
                if (cState.TouchRight && !cState.Touch2 && cState.Touch1)
                    touchArea[2] = true;
                if (!cState.Touch1)
                    touchArea[3] = true;
            }
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
                case DS4Controls.LXNeg: return cState.LX;
                case DS4Controls.LYNeg: return cState.LY;
                case DS4Controls.RXNeg: return cState.RX;
                case DS4Controls.RYNeg: return cState.RY;
                case DS4Controls.LXPos: return (byte)(cState.LX - 127 < 0 ? 0 : (cState.LX - 127));
                case DS4Controls.LYPos: return (byte)(cState.LY - 123 < 0 ? 0 : (cState.LY - 123));
                case DS4Controls.RXPos: return (byte)(cState.RX - 125 < 0 ? 0 : (cState.RX - 125));
                case DS4Controls.RYPos: return (byte)(cState.RY - 127 < 0 ? 0 : (cState.RY - 127));
                case DS4Controls.L2: return cState.L2;
                case DS4Controls.R2: return cState.R2;
            }
            if (cState.TouchButton)
            {
                if (control == DS4Controls.TouchMulti)
                    if (!(touchArea[1] || touchArea[2] || touchArea[3]))
                        return (byte)(touchArea[0] ? 255 : 0);
                if (control == DS4Controls.TouchLeft)
                    if (!(touchArea[0] || touchArea[2] || touchArea[3]))
                        return (byte)(touchArea[1] ? 255 : 0);
                if (control == DS4Controls.TouchRight)
                    if (!(touchArea[0] || touchArea[1] || touchArea[3]))
                        return (byte)(touchArea[2] ? 255 : 0);
                if (control == DS4Controls.TouchUpper)
                    if (!(touchArea[0] || touchArea[1] || touchArea[2]))
                        return (byte)(touchArea[3] ? 255 : 0);
            }
            return 0;
        }
        public static bool getBoolMapping(DS4Controls control, DS4State cState)
        {
            if (!cState.TouchButton)
                for (int i = 0; i < 4; i++)
                    touchArea[i] = false;
            if (!(touchArea[0] || touchArea[1] || touchArea[2] || touchArea[3]))
            {
                if (cState.Touch2)
                    touchArea[0] = true;
                if (cState.TouchLeft && !cState.Touch2 && cState.Touch1)
                    touchArea[1] = true;
                if (cState.TouchRight && !cState.Touch2 && cState.Touch1)
                    touchArea[2] = true;
                if (!cState.Touch1)
                    touchArea[3] = true;
            }
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
                case DS4Controls.LXNeg: return cState.LX < 55;
                case DS4Controls.LYNeg: return cState.LY < 55;
                case DS4Controls.RXNeg: return cState.RX < 55;
                case DS4Controls.RYNeg: return cState.RY < 55;
                case DS4Controls.LXPos: return cState.LX > 200;
                case DS4Controls.LYPos: return cState.LY > 200;
                case DS4Controls.RXPos: return cState.RX > 200;
                case DS4Controls.RYPos: return cState.RY > 200;
                case DS4Controls.L2: return cState.L2 > 100;
                case DS4Controls.R2: return cState.R2 > 100;
               
            }
            if (cState.TouchButton)
            {
                if (control == DS4Controls.TouchMulti)
                    if (!(touchArea[1] || touchArea[2] || touchArea[3]))
                        return touchArea[0];
                if (control == DS4Controls.TouchLeft)
                    if (!(touchArea[0] || touchArea[2] || touchArea[3]))
                        return touchArea[1];
                if (control == DS4Controls.TouchRight)
                    if (!(touchArea[0] || touchArea[1] || touchArea[3]))
                        return touchArea[2];
                if (control == DS4Controls.TouchUpper)
                    if (!(touchArea[0] || touchArea[1] || touchArea[2]))
                        return touchArea[3];
            }
            return false;
        }

        public static byte getXYAxisMapping(DS4Controls control, DS4State cState, bool alt = false)
        {
            byte trueVal = 0;
            byte falseVal = 127;
            if (alt)
            {
                trueVal = 255;
            }
            if (!cState.TouchButton)
                for (int i = 0; i < 4; i++)
                    touchArea[i] = false;
            if (!(touchArea[0] || touchArea[1] || touchArea[2] || touchArea[3]))
            {
                if (cState.Touch2)
                    touchArea[0] = true;
                if (cState.TouchLeft && !cState.Touch2 && cState.Touch1)
                    touchArea[1] = true;
                if (cState.TouchRight && !cState.Touch2 && cState.Touch1)
                    touchArea[2] = true;
                if (!cState.Touch1)
                    touchArea[3] = true;
            }
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
                case DS4Controls.L2: return (byte)(cState.L2 == 255 ? trueVal : falseVal);
                case DS4Controls.R2: return (byte)(cState.R2 == 255 ? trueVal : falseVal);
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
            if (cState.TouchButton)
            {
                if (control == DS4Controls.TouchMulti)
                    if (!(touchArea[1] || touchArea[2] || touchArea[3]))
                        return (byte)(touchArea[0] ? trueVal : falseVal);
                if (control == DS4Controls.TouchLeft)
                    if (!(touchArea[0] || touchArea[2] || touchArea[3]))
                        return (byte)(touchArea[1] ? trueVal : falseVal);
                if (control == DS4Controls.TouchRight)
                    if (!(touchArea[0] || touchArea[1] || touchArea[3]))
                        return (byte)(touchArea[2] ? trueVal : falseVal);
                if (control == DS4Controls.TouchUpper)
                    if (!(touchArea[0] || touchArea[1] || touchArea[2]))
                        return (byte)(touchArea[3] ? trueVal : falseVal);
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
                //case DS4Controls.TouchButton: cState.TouchLeft = false; break;
                //case DS4Controls.TouchMulti: cState.Touch2 = false; break;
                //case DS4Controls.TouchRight: cState.TouchRight = false; break;
                //case DS4Controls.TouchUpper: cState.TouchButton = false; break;
            }
        }


        // Arthritis mode, compensate for cumulative pressure F=kx on the triggers by allowing the user to remap the entire trigger to just the initial portion.
        private static byte[,]  leftTriggerMap = new byte[4,256], rightTriggerMap = new byte[4,256];
        private static double[] leftTriggerMiddle = new double[4], // linear trigger remapping, 0.5 is in the middle of 0 and 255 from the native controller.
            oldLeftTriggerMiddle = new double[4],
            rightTriggerMiddle = new double[4], oldRightTriggerMiddle = new double[4];
        private static void initializeTriggerMap(byte[,] map, double triggerMiddle, int deviceNum)
        {
            double midpoint = 256.0 * triggerMiddle;
            for (uint x = 0; x <= 255; x++)
            {
                double mapped;
                if (x < midpoint) // i.e. with standard 0.5, 0..127->0..127, etc.; with 0.25, 0..63->0..127 and 64..255->128..255\
                    mapped = (x * 0.5 / triggerMiddle);
                else
                    mapped = 128.0 + 128.0 * (x - midpoint) / (256.0 - midpoint);
                map[deviceNum, x] = (byte)mapped;
            }

        }
        public static byte mapLeftTrigger(byte orig, int deviceNum)
        {
            leftTriggerMiddle[deviceNum] = Global.getLeftTriggerMiddle(deviceNum);
            if (leftTriggerMiddle[deviceNum] != oldLeftTriggerMiddle[deviceNum])
            {
                oldLeftTriggerMiddle[deviceNum] = leftTriggerMiddle[deviceNum];
                initializeTriggerMap(leftTriggerMap, leftTriggerMiddle[deviceNum],deviceNum);
            }
            return leftTriggerMap[deviceNum, orig];
        }
        public static byte mapRightTrigger(byte orig, int deviceNum)
        {
            rightTriggerMiddle[deviceNum] = Global.getRightTriggerMiddle(deviceNum);
            if (rightTriggerMiddle[deviceNum] != oldRightTriggerMiddle[deviceNum])
            {
                oldRightTriggerMiddle[deviceNum] = rightTriggerMiddle[deviceNum];
                initializeTriggerMap(rightTriggerMap, rightTriggerMiddle[deviceNum], deviceNum);
            }
            return rightTriggerMap[deviceNum,orig];
        }
    }
}
