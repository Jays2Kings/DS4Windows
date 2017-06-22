
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
            deltaX = -arg.sixAxis.gyroXFull;
            deltaY = -arg.sixAxis.gyroYFull;
            //Console.WriteLine(arg.sixAxis.deltaX);

            double coefficient = Global.GyroSensitivity[deviceNumber] / 100f * 0.008;
            double offset = 0.1;
            double tempAngle = System.Math.Atan2(-deltaY, deltaX);
            double normX = System.Math.Abs(System.Math.Cos(tempAngle));
            double normY = System.Math.Abs(System.Math.Sin(tempAngle));
            int signX = System.Math.Sign(deltaX);
            int signY = System.Math.Sign(deltaY);

            if ((hRemainder > 0) != (deltaX > 0))
            {
                hRemainder = 0.0;
            }

            if ((vRemainder > 0) != (deltaY > 0))
            {
                vRemainder = 0.0;
            }

            int deadzone = 14;
            //int deadzone = 0;
            int deadzoneX = (int)System.Math.Abs(normX * deadzone);
            int deadzoneY = (int)System.Math.Abs(normY * deadzone);

            if (System.Math.Abs(deltaX) > deadzoneX)
            {
                deltaX -= signX * deadzoneX;
            }
            else
            {
                deltaX = 0;
            }

            if (System.Math.Abs(deltaY) > deadzoneY)
            {
                deltaY -= signY * deadzoneY;
            }
            else
            {
                deltaY = 0;
            }

            double xMotion = deltaX != 0 ? coefficient * deltaX + (normX * (offset * signX)) : 0;
            int xAction = 0;
            if (xMotion != 0.0)
            {
                xMotion += hRemainder;
                xAction = (int)xMotion;
                hRemainder = xMotion - xAction;
            }
            else
            {
                hRemainder = 0.0;
            }

            //hRemainder -= (int)hRemainder;
            double yMotion = deltaY != 0 ? coefficient * deltaY + (normY * (offset * signY)) : 0;
            int yAction = 0;
            if (yMotion != 0.0)
            {
                yMotion += vRemainder;
                yAction = (int)yMotion;
                vRemainder = yMotion - yAction;
            }
            else
            {
                vRemainder = 0.0;
            }

            //vRemainder -= (int)vRemainder;

            int gyroInvert = Global.GyroInvert[deviceNumber];
            if (gyroInvert == 2 || gyroInvert == 3)
                xAction *= -1;

            if (gyroInvert == 1 || gyroInvert == 3)
                yAction *= -1;

            if (yAction != 0 || xAction != 0)
                InputMethods.MoveCursorBy(xAction, yAction);

            hDirection = xMotion > 0.0 ? Direction.Positive : xMotion < 0.0 ? Direction.Negative : Direction.Neutral;
            vDirection = yMotion > 0.0 ? Direction.Positive : yMotion < 0.0 ? Direction.Negative : Direction.Neutral;
        }

        public void mouseRemainderReset()
        {
            hRemainder = vRemainder = 0.0;
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
            int touchesLen = arg.touches.Length;
            if ((!dragging && touchesLen != 1) || (dragging && touchesLen < 1))
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
                if (dragging && touchesLen > 1)
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
                if (dragging && touchesLen > 1)
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
