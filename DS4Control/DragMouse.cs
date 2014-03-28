using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using DS4Library;
using System.Threading;

namespace DS4Control
{
    class DragMouse: Mouse, IDisposable
    {
        protected bool leftClick = false;
        protected Timer timer;

        public DragMouse(int deviceID):base(deviceID)
        {
            timer = new System.Threading.Timer((a) =>
            {
                InputMethods.MouseEvent(InputMethods.MOUSEEVENTF_LEFTUP);
                leftClick = false;
            }, null, 
            System.Threading.Timeout.Infinite, 
            System.Threading.Timeout.Infinite); 
        }

        public override string ToString()
        {
            return "Drag Mode";
        }

        public override void touchesMoved(object sender, TouchpadEventArgs arg)
        {
            if (arg.touches.Length == 1)
            {
                double sensitivity = Global.getTouchSensitivity(deviceNum) / 100.0;
                int mouseDeltaX = (int)(sensitivity * (arg.touches[0].deltaX));
                int mouseDeltaY = (int)(sensitivity * (arg.touches[0].deltaY));
                InputMethods.MoveCursorBy(mouseDeltaX, mouseDeltaY);
            }
            else if (arg.touches.Length == 2 && !leftClick)
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
            else
            {
                double sensitivity = Global.getTouchSensitivity(deviceNum) / 100.0;
                int mouseDeltaX = (int)(sensitivity * (arg.touches[1].deltaX));
                int mouseDeltaY = (int)(sensitivity * (arg.touches[1].deltaY));
                InputMethods.MoveCursorBy(mouseDeltaX, mouseDeltaY);
            }
        }

        public override void touchesBegan(object sender, TouchpadEventArgs arg)
        {
            pastTime = arg.timeStamp;
            firstTouch = arg.touches[0];
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

        //CA1001 TypesThatOwnDisposableFieldsShouldBeDisposable
        public void Dispose()
        {
            timer.Dispose();
        }
    }
}
