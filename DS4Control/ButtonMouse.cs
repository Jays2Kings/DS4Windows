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
        public ButtonMouse(int deviceID, DS4Device d)
        {
            deviceNum = deviceID;
            dev = d;
        }

        public override string ToString()
        {
            return "Button Mode";
        }

        public void touchesMoved(object sender, TouchpadEventArgs arg)
        {
            if (arg.touches.Length == 1)
            {
                double sensitivity = Global.getTouchSensitivity(deviceNum) / 100.0;
                int mouseDeltaX = (int)(sensitivity * (arg.touches[0].deltaX));
                int mouseDeltaY = (int)(sensitivity * (arg.touches[0].deltaY));
                InputMethods.MoveCursorBy(mouseDeltaX, mouseDeltaY);
            }
            else if (arg.touches.Length == 2)
            {
                Touch lastT0 = arg.touches[0].previousTouch;
                Touch lastT1 = arg.touches[1].previousTouch;
                Touch T0 = arg.touches[0];
                Touch T1 = arg.touches[1];

                //mouse wheel 120 == 1 wheel click according to Windows API
                int lastMidX = (lastT0.hwX + lastT1.hwX) / 2, lastMidY = (lastT0.hwY + lastT1.hwY) / 2,
                    currentMidX = (T0.hwX + T1.hwX) / 2, currentMidY = (T0.hwY + T1.hwY) / 2; // XXX Will controller swap touch IDs?
                double coefficient = Global.getScrollSensitivity(deviceNum);
                // Adjust for touch distance: "standard" distance is 960 pixels, i.e. half the width.  Scroll farther if fingers are farther apart, and vice versa, in linear proportion.
                double touchXDistance = T1.hwX - T0.hwX, touchYDistance = T1.hwY - T0.hwY, touchDistance = Math.Sqrt(touchXDistance * touchXDistance + touchYDistance * touchYDistance);
                coefficient *= touchDistance / 960.0;
                InputMethods.MouseWheel((int)(coefficient * (lastMidY - currentMidY)), (int)(coefficient * (currentMidX - lastMidX)));
            }
            synthesizeMouseButtons(false);
        }

        public void untouched()
        {
            if (buttonLock)
                synthesizeMouseButtons(false);
            else
                dev.getCurrentState(s);
        }

        public void touchesBegan(object sender, TouchpadEventArgs arg)
        {
            synthesizeMouseButtons(false);
        }

        public void touchesEnded(object sender, TouchpadEventArgs arg)
        {
            if (!buttonLock)
                synthesizeMouseButtons(true);
            else
                dev.getCurrentState(s);
        }

        private void synthesizeMouseButtons(bool justRelease)
        {
            dev.getCurrentState(s);
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
                mapTouchPad(DS4Controls.TouchUpper, true);
                upperDown = false;
            }
            if (leftDown)
            {
                mapTouchPad(DS4Controls.TouchButton, true);
                leftDown = false;
            }
            if (rightDown)
            {
                mapTouchPad(DS4Controls.TouchMulti, true);
                rightDown = false;
            }
            dev.setRumble(0, 0);
        }

        public void touchButtonDown(object sender, TouchpadEventArgs arg)
        {
            byte leftRumble, rightRumble;
            if (arg.touches == null) //No touches, finger on upper portion of touchpad
            {
                mapTouchPad(DS4Controls.TouchUpper, false);
                upperDown = true;
                leftRumble = rightRumble = 127;
            }
            else if (arg.touches.Length == 1)
            {
                if (isLeft(arg.touches[0]))
                {
                    mapTouchPad(DS4Controls.TouchButton, false);
                    leftDown = true;
                    leftRumble = 63;
                    rightRumble = 0;
                }
                else if (isRight(arg.touches[0]))
                {
                    mapTouchPad(DS4Controls.TouchMulti, false);
                    rightDown = true;
                    leftRumble = 0;
                    rightRumble = 63;
                }
                else
                {
                    mapTouchPad(DS4Controls.TouchUpper, false); // ambiguous = same as upper
                    upperDown = true;
                    leftRumble = rightRumble = 127;
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

