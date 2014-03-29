using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using DS4Library;
namespace DS4Control
{
    public class ButtonMouse : ITouchpadBehaviour
    {
        private int deviceNum;
        private bool leftButton, middleButton, rightButton;
        private DS4State s = new DS4State();
        private bool buttonLock; // Toggled with a two-finger touchpad push, we accept and absorb button input without any fingers on a touchpad, helping with drag-and-drop.
        private DS4Device dev = null;
        private readonly MouseCursor cursor;
        private readonly MouseWheel wheel;
        public ButtonMouse(int deviceID, DS4Device d)
        {
            deviceNum = deviceID;
            dev = d;
            cursor = new MouseCursor(deviceNum);
            wheel = new MouseWheel(deviceNum);
        }

        public override string ToString()
        {
            return "Button Mode";
        }

        public void touchesMoved(object sender, TouchpadEventArgs arg)
        {
            cursor.touchesMoved(arg);
            wheel.touchesMoved(arg);
            dev.getCurrentState(s);
            synthesizeMouseButtons(false);
        }

        public void touchUnchanged(object sender, EventArgs unused)
        {
            dev.getCurrentState(s);
            if (buttonLock || s.Touch1 || s.Touch2)
                synthesizeMouseButtons(false);
        }

        public void touchesBegan(object sender, TouchpadEventArgs arg)
        {
            cursor.touchesBegan(arg);
            wheel.touchesBegan(arg);
            dev.getCurrentState(s);
            synthesizeMouseButtons(false);
        }

        public void touchesEnded(object sender, TouchpadEventArgs arg)
        {
            dev.getCurrentState(s);
            if (!buttonLock)
                synthesizeMouseButtons(true);
        }

        private void synthesizeMouseButtons(bool justRelease)
        {
            bool previousLeftButton = leftButton, previousMiddleButton = middleButton, previousRightButton = rightButton;
            if (justRelease)
            {
                leftButton = middleButton = rightButton = false;
            }
            else
            {
                leftButton = s.L1 || s.R1 || s.DpadLeft || s.Square;
                middleButton = s.DpadUp || s.DpadDown || s.Triangle || s.Cross;
                rightButton = s.DpadRight || s.Circle;
                s.L1 = s.R1 = s.DpadLeft = s.Square = s.DpadUp = s.DpadDown = s.Triangle = s.Cross = s.DpadRight = s.Circle = false;
            }
            if (leftButton != previousLeftButton)
                InputMethods.MouseEvent(leftButton ? InputMethods.MOUSEEVENTF_LEFTDOWN : InputMethods.MOUSEEVENTF_LEFTUP);
            if (middleButton != previousMiddleButton)
                InputMethods.MouseEvent(middleButton ? InputMethods.MOUSEEVENTF_MIDDLEDOWN : InputMethods.MOUSEEVENTF_MIDDLEUP);
            if (rightButton != previousRightButton)
                InputMethods.MouseEvent(rightButton ? InputMethods.MOUSEEVENTF_RIGHTDOWN : InputMethods.MOUSEEVENTF_RIGHTUP);

        }

        // touch area stuff
        private bool leftDown, rightDown, upperDown;
        private bool isLeft(Touch t)
        {
            return t.hwX < 1920 * 2 / 5;
        }

        private bool isRight(Touch t)
        {
            return t.hwX >= 1920 * 3 / 5;
        }

        public void touchButtonUp(object sender, TouchpadEventArgs arg)
        {
            if (upperDown)
            {
                if (!mapTouchPad(DS4Controls.TouchUpper, true))
                    InputMethods.MouseEvent(InputMethods.MOUSEEVENTF_MIDDLEUP);
                upperDown = false;
            }
            if (leftDown)
            {
                if (!mapTouchPad(DS4Controls.TouchButton, true))
                    InputMethods.MouseEvent(InputMethods.MOUSEEVENTF_LEFTUP);
                leftDown = false;
            }
            if (rightDown)
            {
                if (!mapTouchPad(DS4Controls.TouchMulti, true))
                    InputMethods.MouseEvent(InputMethods.MOUSEEVENTF_RIGHTUP);
                rightDown = false;
            }
            dev.setRumble(0, 0);
        }

        public void touchButtonDown(object sender, TouchpadEventArgs arg)
        {
            byte leftRumble, rightRumble;
            if (arg.touches == null) //No touches, finger on upper portion of touchpad
            {
                if (!mapTouchPad(DS4Controls.TouchUpper, false))
                    InputMethods.MouseEvent(InputMethods.MOUSEEVENTF_MIDDLEDOWN);
                upperDown = true;
                leftRumble = rightRumble = 127;
            }
            else if (arg.touches.Length == 1)
            {
                if (isLeft(arg.touches[0]))
                {
                    if (!mapTouchPad(DS4Controls.TouchButton, false))
                        InputMethods.MouseEvent(InputMethods.MOUSEEVENTF_LEFTDOWN);
                    leftDown = true;
                    leftRumble = 63;
                    rightRumble = 0;
                }
                else if (isRight(arg.touches[0]))
                {
                    if (!mapTouchPad(DS4Controls.TouchMulti, false))
                        InputMethods.MouseEvent(InputMethods.MOUSEEVENTF_RIGHTDOWN);
                    rightDown = true;
                    leftRumble = 0;
                    rightRumble = 63;
                }
                else
                {
                    leftRumble = rightRumble = 0; // Ignore ambiguous pushes.
                }
            }
            else
            {
                buttonLock = !buttonLock;
                leftRumble = rightRumble = (byte)(buttonLock ? 255 : 63);
            }
            dev.setRumble(rightRumble, leftRumble); // sustain while pressed
        }

        bool mapTouchPad(DS4Controls padControl, bool release)
        {
            ushort key = Global.getCustomKey(padControl);
            if (key == 0)
                return false;
            else
            {
                DS4KeyType keyType = Global.getCustomKeyType(padControl);
                if (!release)
                    if (keyType.HasFlag(DS4KeyType.ScanCode))
                        InputMethods.performSCKeyPress(key);
                    else InputMethods.performKeyPress(key);
                else
                    if (!keyType.HasFlag(DS4KeyType.Repeat))
                        if (keyType.HasFlag(DS4KeyType.ScanCode))
                            InputMethods.performSCKeyRelease(key);
                        else InputMethods.performKeyRelease(key);
                return true;
            }
        }

        public DS4State getDS4State()
        {
            return s;
        }

    }
}

