using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using DS4Library;
using System.Threading;

namespace DS4Control
{
    class DragMouse: Mouse
    {
        protected bool leftClick = false;
        protected Timer timer;
        private readonly MouseCursor cursor;
        private readonly MouseWheel wheel;

        public DragMouse(int deviceID):base(deviceID)
        {
            timer = new System.Threading.Timer((a) =>
            {
                InputMethods.MouseEvent(InputMethods.MOUSEEVENTF_LEFTUP);
                leftClick = false;
            }, null, 
            System.Threading.Timeout.Infinite, 
            System.Threading.Timeout.Infinite);
            cursor = new MouseCursor(deviceNum);
            wheel = new MouseWheel(deviceNum);
        }

        public override string ToString()
        {
            return "Drag Mode";
        }

        public override void touchesBegan(object sender, TouchpadEventArgs arg)
        {
            base.touchesBegan(sender, arg);
            if (leftClick)
                timer.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
        }

        public override void touchesEnded(object sender, TouchpadEventArgs arg)
        {
            if (Global.getTapSensitivity(deviceNum) != 0)
            {
                DateTime test = arg.timeStamp;
                if (test <= (pastTime + TimeSpan.FromMilliseconds((double)Global.getTapSensitivity(deviceNum) * 2)) && !arg.touchButtonPressed)
                {
                    if (Math.Abs(firstTouch.hwX - arg.touches[0].hwX) < 10 &&
                        Math.Abs(firstTouch.hwY - arg.touches[0].hwY) < 10)
                    {
                        if (leftClick)
                            InputMethods.MouseEvent(InputMethods.MOUSEEVENTF_LEFTUP);

                        leftClick = true;
                        InputMethods.MouseEvent(InputMethods.MOUSEEVENTF_LEFTDOWN);
                        timer.Change(Global.getTapSensitivity(deviceNum) * 2, System.Threading.Timeout.Infinite);
                    }
                }
                else if (leftClick)
                {
                    InputMethods.MouseEvent(InputMethods.MOUSEEVENTF_LEFTUP);
                    leftClick = false;
                }
            }
        }

        public override void touchButtonUp(object sender, TouchpadEventArgs arg)
        {
            if (arg.touches == null)
            {
                //No touches, finger on upper portion of touchpad
                mapTouchPad(DS4Controls.TouchUpper, true);
            }
            else if (arg.touches.Length > 1)
                mapTouchPad(DS4Controls.TouchMulti, true);
            else if (!rightClick && arg.touches.Length == 1 && !mapTouchPad(DS4Controls.TouchButton, true))
            {
                InputMethods.MouseEvent(InputMethods.MOUSEEVENTF_LEFTUP);
                leftClick = false;
            }
        }

        public override void touchButtonDown(object sender, TouchpadEventArgs arg)
        {
            if (arg.touches == null)
            {
                //No touches, finger on upper portion of touchpad
                if (!mapTouchPad(DS4Controls.TouchUpper))
                    InputMethods.performMiddleClick();
            }
            else if (!Global.getLowerRCOff(deviceNum) && arg.touches[0].hwX > (1920 * 3) / 4
                && arg.touches[0].hwY > (960 * 3) / 4)
            {
                rightClick = true;
                InputMethods.performRightClick();
            }
            else if (arg.touches.Length > 1 && !mapTouchPad(DS4Controls.TouchMulti))
            {
                rightClick = true;
                InputMethods.performRightClick();
            }
            else if (arg.touches.Length == 1 && !mapTouchPad(DS4Controls.TouchButton))
            {
                rightClick = false;
                InputMethods.MouseEvent(InputMethods.MOUSEEVENTF_LEFTDOWN);
                leftClick = true;
            }
        }
    }
}
