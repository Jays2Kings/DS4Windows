using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DS4Library;

namespace DS4Control
{
    class MouseCursor
    {
        private readonly int deviceNumber;
        public MouseCursor(int deviceNum)
        {
            deviceNumber = deviceNum;
        }

        // Keep track of remainders when performing moves or we lose fractional parts.
        private double horizontalRemainder = 0.0, verticalRemainder = 0.0;

        public void touchesBegan(TouchpadEventArgs arg)
        {
            if (arg.touches.Length == 1)
                horizontalRemainder = verticalRemainder = 0.0;
        }

        private byte lastTouchID;
        public void touchesMoved(TouchpadEventArgs arg)
        {
            if (arg.touches.Length != 1)
                return;
            int deltaX, deltaY;
            if (arg.touches[0].touchID != lastTouchID)
            {
                deltaX = deltaY = 0;
                horizontalRemainder = verticalRemainder = 0.0;
                lastTouchID = arg.touches[0].touchID;
            }
            else if (Global.getTouchpadJitterCompensation(deviceNumber))
            {
                // Often the DS4's internal jitter compensation kicks in and starts hiding changes, ironically creating jitter...
                deltaX = arg.touches[0].deltaX;
                deltaY = arg.touches[0].deltaY;
                // allow only very fine, slow motions, when changing direction
                if (deltaX < -1)
                {
                    if (horizontalRemainder >= 0.0)
                    {
                        deltaX = -1;
                        horizontalRemainder = 0.0;
                    }
                }
                else if (deltaX > 1)
                {
                    if (horizontalRemainder <= 0.0)
                    {
                        deltaX = 1;
                        horizontalRemainder = 0.0;
                    }
                }

                if (deltaY < -1)
                {
                    if (verticalRemainder >= 0.0)
                    {
                        deltaY = -1;
                        verticalRemainder = 0.0;
                    }
                }
                else if (deltaY > 1)
                {
                    if (verticalRemainder <= 0.0)
                    {
                        deltaY = 1;
                        verticalRemainder = 0.0;
                    }
                }
            }
            else
            {
                deltaX = arg.touches[0].deltaX;
                deltaY = arg.touches[0].deltaY;
            }

            double coefficient = Global.getTouchSensitivity(deviceNumber) / 100.0;
            // Collect rounding errors instead of losing motion.
            double xMotion = coefficient * deltaX;
            if (xMotion > 0.0)
            {
                if (horizontalRemainder > 0.0)
                    xMotion += horizontalRemainder;
            }
            else if (xMotion < 0.0)
            {
                if (horizontalRemainder < 0.0)
                    xMotion += horizontalRemainder;
            }
            int xAction = (int)xMotion;
            horizontalRemainder = xMotion - xAction;

            double yMotion = coefficient * deltaY;
            if (yMotion > 0.0)
            {
                if (verticalRemainder > 0.0)
                    yMotion += verticalRemainder;
            }
            else if (yMotion < 0.0)
            {
                if (verticalRemainder < 0.0)
                    yMotion += verticalRemainder;
            }
            int yAction = (int)yMotion;
            verticalRemainder = yMotion - yAction;

            if (yAction != 0 || xAction != 0)
                InputMethods.MoveCursorBy(xAction, yAction);
        }
    }
}
