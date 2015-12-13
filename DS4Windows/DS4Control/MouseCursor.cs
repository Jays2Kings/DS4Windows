using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace DS4Windows
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
        private double hRemainder = 0.0, vRemainder = 0.0;
        /** Indicate x/y direction for doing jitter compensation, etc. */
        public enum Direction { Negative, Neutral, Positive }
        // Track direction vector separately and very trivially for now.
        private Direction horizontalDirection = Direction.Neutral, verticalDirection = Direction.Neutral;
        private Direction hDirection = Direction.Neutral, vDirection = Direction.Neutral;

        public virtual void sixaxisMoved(SixAxisEventArgs arg)
        {
            int deltaX = 0, deltaY = 0;
            deltaX = -arg.sixAxis.accelX;
            deltaY = -arg.sixAxis.accelY;
            //Console.WriteLine(arg.sixAxis.deltaX);

            double coefficient = Global.GyroSensitivity[deviceNumber] / 100f;
            //Collect rounding errors instead of losing motion.
            double xMotion = coefficient * deltaX;
            xMotion += hRemainder;
            int xAction = (int)xMotion;
            hRemainder += xMotion - xAction;
            hRemainder -= (int)hRemainder;
            double yMotion = coefficient * deltaY;
            yMotion += vRemainder;
            int yAction = (int)yMotion;
            vRemainder += yMotion - yAction;
            vRemainder -= (int)vRemainder;
            if (Global.GyroInvert[deviceNumber] == 2 || Global.GyroInvert[deviceNumber] == 3)
                xAction *= -1;
            if (Global.GyroInvert[deviceNumber] == 1 || Global.GyroInvert[deviceNumber] == 3)
                yAction *= -1;
            if (yAction != 0 || xAction != 0)
                InputMethods.MoveCursorBy(xAction, yAction);

            hDirection = xMotion > 0.0 ? Direction.Positive : xMotion < 0.0 ? Direction.Negative : Direction.Neutral;
            vDirection = yMotion > 0.0 ? Direction.Positive : yMotion < 0.0 ? Direction.Negative : Direction.Neutral;
        }

        public void touchesBegan(TouchpadEventArgs arg)
        {
            if (arg.touches.Length == 1)
            {
                horizontalRemainder = verticalRemainder = 0.0;
                horizontalDirection = verticalDirection = Direction.Neutral;
            }
        }

        private byte lastTouchID;
        public void touchesMoved(TouchpadEventArgs arg, bool dragging)
        {
            if ((!dragging && arg.touches.Length != 1) || (dragging && arg.touches.Length < 1))
                return;
            int deltaX, deltaY;
            if (arg.touches[0].touchID != lastTouchID)
            {
                deltaX = deltaY = 0;
                horizontalRemainder = verticalRemainder = 0.0;
                horizontalDirection = verticalDirection = Direction.Neutral;
                lastTouchID = arg.touches[0].touchID;
            }
            else if (Global.TouchpadJitterCompensation[deviceNumber])
            {
                // Often the DS4's internal jitter compensation kicks in and starts hiding changes, ironically creating jitter...

                if (dragging && arg.touches.Length > 1)
                {
                    deltaX = arg.touches[1].deltaX;
                    deltaY = arg.touches[1].deltaY;
                }
                else
                {
                    deltaX = arg.touches[0].deltaX;
                    deltaY = arg.touches[0].deltaY;
                }
                // allow only very fine, slow motions, when changing direction, even from neutral
                // TODO maybe just consume it completely?
                if (deltaX <= -1)
                {
                    if (horizontalDirection != Direction.Negative)
                    {
                        deltaX = -1;
                        horizontalRemainder = 0.0;
                    }
                }
                else if (deltaX >= 1)
                {
                    if (horizontalDirection != Direction.Positive)
                    {
                        deltaX = 1;
                        horizontalRemainder = 0.0;
                    }
                }

                if (deltaY <= -1)
                {
                    if (verticalDirection != Direction.Negative)
                    {
                        deltaY = -1;
                        verticalRemainder = 0.0;
                    }
                }
                else if (deltaY >= 1)
                {
                    if (verticalDirection != Direction.Positive)
                    {
                        deltaY = 1;
                        verticalRemainder = 0.0;
                    }
                }
            }
            else
            {
                if (dragging && arg.touches.Length > 1)
                {
                    deltaX = arg.touches[1].deltaX;
                    deltaY = arg.touches[1].deltaY;
                }
                else
                {
                    deltaX = arg.touches[0].deltaX;
                    deltaY = arg.touches[0].deltaY;
                }
            }

            double coefficient = Global.TouchSensitivity[deviceNumber] / 100.0;
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

            horizontalDirection = xMotion > 0.0 ? Direction.Positive : xMotion < 0.0 ? Direction.Negative : Direction.Neutral;
            verticalDirection = yMotion > 0.0 ? Direction.Positive : yMotion < 0.0 ? Direction.Negative : Direction.Neutral;
        }
    }
}
