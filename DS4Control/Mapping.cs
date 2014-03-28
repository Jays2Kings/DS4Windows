using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DS4Library;
namespace DS4Control
{
    class Mapping
    {
        public static void mapButtons(DS4State cState, DS4State prevState, DS4State MappedState)
        {
            foreach (KeyValuePair<DS4Controls, ushort> customKey in Global.getCustomKeys())
            {
                DS4KeyType keyType = Global.getCustomKeyType(customKey.Key);
                bool PrevOn = getBoolMapping(customKey.Key, prevState);
                if (getBoolMapping(customKey.Key, cState))
                {
                    resetToDefaultValue(customKey.Key, cState);
                    if (!PrevOn)
                    {

                        if (keyType.HasFlag(DS4KeyType.ScanCode))
                            InputMethods.performSCKeyPress(customKey.Value);
                        else InputMethods.performKeyPress(customKey.Value);
                    }
                    else if (keyType.HasFlag(DS4KeyType.Repeat))
                        if (keyType.HasFlag(DS4KeyType.ScanCode))
                            InputMethods.performSCKeyPress(customKey.Value);
                        else InputMethods.performKeyPress(customKey.Value);
                }
                else if (PrevOn)
                    if (keyType.HasFlag(DS4KeyType.ScanCode))
                        InputMethods.performSCKeyRelease(customKey.Value);
                    else InputMethods.performKeyRelease(customKey.Value);
            }

            cState.Copy(MappedState);
            bool LX = false, LY = false, RX = false, RY = false;
            MappedState.LX = 127;
            MappedState.LY = 127;
            MappedState.RX = 127;
            MappedState.RY = 127;
            int MouseDeltaX = 0;
            int MouseDeltaY = 0;

            foreach (KeyValuePair<DS4Controls, X360Controls> customButton in Global.getCustomButtons())
            {

                bool LXChanged = MappedState.LX == 127;
                bool LYChanged = MappedState.LY == 127;
                bool RXChanged = MappedState.RX == 127;
                bool RYChanged = MappedState.RY == 127;
                switch (customButton.Value)
                {
                    case X360Controls.A:
                        MappedState.Cross = getBoolMapping(customButton.Key, cState);
                        break;
                    case X360Controls.B:
                        MappedState.Circle = getBoolMapping(customButton.Key, cState);
                        break;
                    case X360Controls.X:
                        MappedState.Square = getBoolMapping(customButton.Key, cState);
                        break;
                    case X360Controls.Y:
                        MappedState.Triangle = getBoolMapping(customButton.Key, cState);
                        break;
                    case X360Controls.LB:
                        MappedState.L1 = getBoolMapping(customButton.Key, cState);
                        break;
                    case X360Controls.LS:
                        MappedState.L3 = getBoolMapping(customButton.Key, cState);
                        break;
                    case X360Controls.RB:
                        MappedState.R1 = getBoolMapping(customButton.Key, cState);
                        break;
                    case X360Controls.RS:
                        MappedState.R3 = getBoolMapping(customButton.Key, cState);
                        break;
                    case X360Controls.DpadUp:
                        MappedState.DpadUp = getBoolMapping(customButton.Key, cState);
                        break;
                    case X360Controls.DpadDown:
                        MappedState.DpadDown = getBoolMapping(customButton.Key, cState);
                        break;
                    case X360Controls.DpadLeft:
                        MappedState.DpadLeft = getBoolMapping(customButton.Key, cState);
                        break;
                    case X360Controls.DpadRight:
                        MappedState.DpadRight = getBoolMapping(customButton.Key, cState);
                        break;
                    case X360Controls.Guide:
                        MappedState.PS = getBoolMapping(customButton.Key, cState);
                        break;
                    case X360Controls.Back:
                        MappedState.Share = getBoolMapping(customButton.Key, cState);
                        break;
                    case X360Controls.Start:
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
                        bool PrevOn = getBoolMapping(customButton.Key, prevState);
                        bool CurOn = getBoolMapping(customButton.Key, cState);
                        if (!PrevOn && CurOn)
                            InputMethods.MouseEvent(InputMethods.MOUSEEVENTF_LEFTDOWN);
                        else if (PrevOn && !CurOn)
                            InputMethods.MouseEvent(InputMethods.MOUSEEVENTF_LEFTUP);
                        break;
                    case X360Controls.RightMouse:
                        PrevOn = getBoolMapping(customButton.Key, prevState);
                        CurOn = getBoolMapping(customButton.Key, cState);
                        if (!PrevOn && CurOn)
                            InputMethods.MouseEvent(InputMethods.MOUSEEVENTF_RIGHTDOWN);
                        else if (PrevOn && !CurOn)
                            InputMethods.MouseEvent(InputMethods.MOUSEEVENTF_RIGHTUP);
                        break;
                    case X360Controls.MiddleMouse:
                        PrevOn = getBoolMapping(customButton.Key, prevState);
                        CurOn = getBoolMapping(customButton.Key, cState);
                        if (!PrevOn && CurOn)
                            InputMethods.MouseEvent(InputMethods.MOUSEEVENTF_MIDDLEDOWN);
                        else if (PrevOn && !CurOn)
                            InputMethods.MouseEvent(InputMethods.MOUSEEVENTF_MIDDLEUP);
                        break;
                    case X360Controls.MouseUp:
                        if (MouseDeltaY == 0)
                        {
                            MouseDeltaY = calculateRelativeMouseDelta(customButton.Key, cState, prevState);
                            MouseDeltaY = -Math.Abs(MouseDeltaY);
                        }
                        break;
                    case X360Controls.MouseDown:
                        if (MouseDeltaY == 0)
                        {
                            MouseDeltaY = calculateRelativeMouseDelta(customButton.Key, cState, prevState);
                            MouseDeltaY = Math.Abs(MouseDeltaY);
                        }
                        break;
                    case X360Controls.MouseLeft:
                        if (MouseDeltaX == 0)
                        {
                            MouseDeltaX = calculateRelativeMouseDelta(customButton.Key, cState, prevState);
                            MouseDeltaX = -Math.Abs(MouseDeltaX);
                        }
                        break;
                    case X360Controls.MouseRight:
                        if (MouseDeltaX == 0)
                        {
                            MouseDeltaX = calculateRelativeMouseDelta(customButton.Key, cState, prevState);
                            MouseDeltaX = Math.Abs(MouseDeltaX);
                        }
                        break;
                    case X360Controls.Unbound:
                        resetToDefaultValue(customButton.Key, MappedState);
                        break;
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

        private static int calculateRelativeMouseDelta(DS4Controls control, DS4State cState, DS4State pState)
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
            }
            axisVal = axisVal - 127;
            int delta = 0;
            if ( (!positive && axisVal < -DEAD_ZONE ) || (positive && axisVal > DEAD_ZONE))
            {
                delta = (int)(float) (axisVal * SPEED_MULTIPLIER * deltaTime);
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
        public static byte getByteMapping(DS4Controls control, DS4State cState)
        {
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
            return 0;
        }

        public static bool getBoolMapping(DS4Controls control, DS4State cState)
        {
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
