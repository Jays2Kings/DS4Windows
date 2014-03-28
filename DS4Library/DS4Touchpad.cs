using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
namespace DS4Library
{
    public class TouchpadEventArgs : EventArgs
    {
        public readonly Touch[] touches = null;
        public readonly bool touchButtonPressed;
        public readonly DateTime timeStamp;
        public TouchpadEventArgs(DateTime timeStamp, bool tButtonDown, Touch t0, Touch t1 = null)
        {
            this.timeStamp = timeStamp;
            if (t1 != null)
            {
                touches = new Touch[2];
                touches[0] = t0;
                touches[1] = t1;
            }
            else if (t0 != null)
            {
                touches = new Touch[1];
                touches[0] = t0;
            }
            touchButtonPressed = tButtonDown;
        }
    }

    public class Touch
    {
        public readonly int hwX, hwY, deltaX, deltaY;
        public readonly byte touchID;
        public readonly Touch previousTouch;
        public Touch(int X, int Y,  byte tID, Touch prevTouch = null)
        {
            hwX = X;
            hwY = Y;
            touchID = tID;
            previousTouch = prevTouch;
            if (previousTouch != null)
            {
                deltaX = X - previousTouch.hwX;
                deltaY = Y - previousTouch.hwY;
            }
        }
    }

    public class DS4Touchpad
    {
        public event EventHandler<TouchpadEventArgs> TouchesBegan = null;
        public event EventHandler<TouchpadEventArgs> TouchesMoved = null;
        public event EventHandler<TouchpadEventArgs> TouchesEnded = null;
        public event EventHandler<TouchpadEventArgs> TouchButtonDown = null;
        public event EventHandler<TouchpadEventArgs> TouchButtonUp = null;
        public readonly static int TOUCHPAD_DATA_OFFSET = 35;
        internal static int lastTouchPadX, lastTouchPadY,
            lastTouchPadX2, lastTouchPadY2; // tracks 0, 1 or 2 touches; we maintain touch 1 and 2 separately
        internal static bool lastTouchPadIsDown;
        internal static bool lastIsActive;
        internal static bool lastIsActive2;
        internal static byte lastTouchID, lastTouchID2;

        public void handleTouchpad(byte[] data, DS4State sensors)
        {
            bool touchPadIsDown = sensors.TouchButton;
            byte touchID = (byte)(data[0 + TOUCHPAD_DATA_OFFSET] & 0x7F);
            byte touchID2 = (byte)(data[4 + TOUCHPAD_DATA_OFFSET] & 0x7F);
            int currentX = data[1 + TOUCHPAD_DATA_OFFSET] + ((data[2 + TOUCHPAD_DATA_OFFSET] & 0xF) * 255);
            int currentY = ((data[2 + TOUCHPAD_DATA_OFFSET] & 0xF0) >> 4) + (data[3 + TOUCHPAD_DATA_OFFSET] * 16);
            int currentX2 = data[5 + TOUCHPAD_DATA_OFFSET] + ((data[6 + TOUCHPAD_DATA_OFFSET] & 0xF) * 255);
            int currentY2 = ((data[6 + TOUCHPAD_DATA_OFFSET] & 0xF0) >> 4) + (data[7 + TOUCHPAD_DATA_OFFSET] * 16);

            if (sensors.Touch1)
            {
                if (!lastTouchPadIsDown && touchPadIsDown && TouchButtonDown != null)
                {
                    TouchpadEventArgs args = null;
                    Touch t0 = new Touch(currentX, currentY, touchID);
                    if (sensors.Touch2)
                    {
                        Touch t1 = new Touch(currentX2, currentY2, touchID2);
                        args = new TouchpadEventArgs(sensors.ReportTimeStamp, sensors.TouchButton, t0, t1);
                    }
                    else
                        args = new TouchpadEventArgs(sensors.ReportTimeStamp,  sensors.TouchButton, t0);
                    TouchButtonDown(this, args);
                }
                else if (lastTouchPadIsDown && !touchPadIsDown && TouchButtonUp != null)
                {
                    TouchpadEventArgs args = null;
                    Touch t0 = new Touch(currentX, currentY, touchID);
                    if (sensors.Touch2)
                    {
                        Touch t1 = new Touch(currentX2, currentY2, touchID2);
                        args = new TouchpadEventArgs(sensors.ReportTimeStamp,  sensors.TouchButton, t0, t1);
                    }
                    else
                        args = new TouchpadEventArgs(sensors.ReportTimeStamp,  sensors.TouchButton, t0);
                    TouchButtonUp(this, args);
                }

                if (!lastIsActive || (sensors.Touch2 && !lastIsActive2))
                {
                    TouchpadEventArgs args = null;
                    Touch t0 = new Touch(currentX, currentY, touchID);
                    if (sensors.Touch2 && !lastIsActive2)
                    {
                        Touch t1 = new Touch(currentX2, currentY2, touchID2);
                        args = new TouchpadEventArgs(sensors.ReportTimeStamp,  sensors.TouchButton, t0, t1);
                    }
                    else
                        args = new TouchpadEventArgs(sensors.ReportTimeStamp,  sensors.TouchButton, t0);
                    if (TouchesBegan != null)
                        TouchesBegan(this, args);
                }
                else if (lastIsActive)
                {
                    if (TouchesMoved != null)
                    {
                        TouchpadEventArgs args = null;

                        Touch t0Prev = new Touch(lastTouchPadX, lastTouchPadY, lastTouchID);
                        Touch t0 = new Touch(currentX, currentY, touchID, t0Prev);
                        if (sensors.Touch1 && sensors.Touch2)
                        {
                            Touch t1Prev = new Touch(lastTouchPadX2, lastTouchPadY2, lastTouchID2);
                            Touch t1 = new Touch(currentX2, currentY2, touchID2, t1Prev);
                            args = new TouchpadEventArgs(sensors.ReportTimeStamp,  sensors.TouchButton, t0, t1);
                        }
                        else
                            args = new TouchpadEventArgs(sensors.ReportTimeStamp,  sensors.TouchButton, t0);
                        TouchesMoved(this, args);
                    }
                }

                lastTouchPadX = currentX;
                lastTouchPadY = currentY;
                lastTouchPadX2 = currentX2;
                lastTouchPadY2 = currentY2;
                lastTouchPadIsDown = touchPadIsDown;
            }
            else
            {
                if (lastIsActive)
                {
                    TouchpadEventArgs args = null;
                    Touch t0 = new Touch(currentX, currentY, touchID);
                    if (lastIsActive2)
                    {
                        Touch t1 = new Touch(currentX2, currentY2, touchID2);
                        args = new TouchpadEventArgs(sensors.ReportTimeStamp,  sensors.TouchButton, t0, t1);
                    }
                    else
                        args = new TouchpadEventArgs(sensors.ReportTimeStamp, sensors.TouchButton, t0);
                    if (TouchesEnded != null)
                        TouchesEnded(this, args);
                }

                if (touchPadIsDown && !lastTouchPadIsDown && TouchButtonDown != null)
                {
                    TouchButtonDown(this, new TouchpadEventArgs(sensors.ReportTimeStamp, sensors.TouchButton, null, null));
                }
                else if (!touchPadIsDown && lastTouchPadIsDown && TouchButtonUp != null)
                {
                    TouchButtonUp(this, new TouchpadEventArgs(sensors.ReportTimeStamp,  sensors.TouchButton, null, null));
                }
            }

            lastIsActive = sensors.Touch1;
            lastIsActive2 = sensors.Touch2;
            lastTouchID = touchID;
            lastTouchID2 = touchID2;
            lastTouchPadIsDown = touchPadIsDown;
        }
    }
}
