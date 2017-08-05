
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
        private Direction horizontalDirection = Direction.Neutral,
            verticalDirection = Direction.Neutral;
        private Direction hDirection = Direction.Neutral, vDirection = Direction.Neutral;

        private double GYRO_MOUSE_COEFFICIENT = 0.0095;
        private int GYRO_MOUSE_DEADZONE = 12;
        private double GYRO_MOUSE_OFFSET = 0.1463;
        private double GYRO_SMOOTH_MOUSE_OFFSET = 0.14698;

        private const int SMOOTH_BUFFER_LEN = 3;
        private double[] xSmoothBuffer = new double[SMOOTH_BUFFER_LEN];
        private double[] ySmoothBuffer = new double[SMOOTH_BUFFER_LEN];
        private int smoothBufferTail = 0;

        double coefficient = 0.0;
        double verticalScale = 0.0;
        bool gyroSmooth = false;

        int tempInt = 0;
        double tempDouble = 0.0;

        public virtual void sixaxisMoved(SixAxisEventArgs arg)
        {
            int deltaX = 0, deltaY = 0;
            deltaX = Global.getGyroMouseHorizontalAxis(deviceNumber) == 0 ? arg.sixAxis.gyroYawFull :
                arg.sixAxis.gyroRollFull;
            deltaY = -arg.sixAxis.gyroPitchFull;
            //tempDouble = arg.sixAxis.elapsed * 0.001 * 200.0; // Base default speed on 5 ms
            tempDouble = arg.sixAxis.elapsed * 200.0; // Base default speed on 5 ms

            gyroSmooth = Global.getGyroSmoothing(deviceNumber);
            double gyroSmoothWeight = 0.0;

            coefficient = (Global.getGyroSensitivity(deviceNumber) * 0.01) * GYRO_MOUSE_COEFFICIENT;
            double offset = GYRO_MOUSE_OFFSET;
            if (gyroSmooth)
            {
                gyroSmoothWeight = Global.getGyroSmoothingWeight(deviceNumber);
                if (gyroSmoothWeight > 0.0)
                {
                    offset = GYRO_SMOOTH_MOUSE_OFFSET;
                }
            }

            double tempAngle = System.Math.Atan2(-deltaY, deltaX);
            double normX = System.Math.Abs(System.Math.Cos(tempAngle));
            double normY = System.Math.Abs(System.Math.Sin(tempAngle));
            int signX = System.Math.Sign(deltaX);
            int signY = System.Math.Sign(deltaY);

            if (deltaX == 0 || (hRemainder > 0 != deltaX > 0))
            {
                hRemainder = 0.0;
            }

            if (deltaY == 0 || (vRemainder > 0 != deltaY > 0))
            {
                vRemainder = 0.0;
            }

            int deadzone = GYRO_MOUSE_DEADZONE;
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

            double xMotion = deltaX != 0 ? coefficient * (deltaX * tempDouble)
                + (normX * (offset * signX)) : 0;

            int xAction = 0;
            if (xMotion != 0.0)
            {
                xMotion += hRemainder;
            }
            else
            {
                hRemainder = 0.0;
            }

            verticalScale = Global.getGyroSensVerticalScale(deviceNumber) * 0.01;
            double yMotion = deltaY != 0 ? (coefficient * verticalScale) * (deltaY * tempDouble)
                + (normY * (offset * signY)) : 0;

            int yAction = 0;
            if (yMotion != 0.0)
            {
                yMotion += vRemainder;
            }
            else
            {
                vRemainder = 0.0;
            }

            if (gyroSmooth)
            {
                int iIndex = smoothBufferTail % SMOOTH_BUFFER_LEN;
                xSmoothBuffer[iIndex] = xMotion;
                ySmoothBuffer[iIndex] = yMotion;
                smoothBufferTail = iIndex + 1;

                double currentWeight = 1.0;
                double finalWeight = 0.0;
                double x_out = 0.0, y_out = 0.0;
                int idx = 0;
                for (int i = 0; i < SMOOTH_BUFFER_LEN; i++)
                {
                    idx = System.Math.Abs(smoothBufferTail - i - 1) % SMOOTH_BUFFER_LEN;
                    x_out += xSmoothBuffer[idx] * currentWeight;
                    y_out += ySmoothBuffer[idx] * currentWeight;
                    finalWeight += currentWeight;
                    currentWeight *= gyroSmoothWeight;
                }

                x_out /= finalWeight;
                xMotion = x_out;
                y_out /= finalWeight;
                yMotion = y_out;
            }

            hRemainder = vRemainder = 0.0;
            if (xMotion != 0.0)
            {
                xAction = (int)xMotion;
                hRemainder = xMotion - xAction;
            }

            if (yMotion != 0.0)
            {
                yAction = (int)yMotion;
                vRemainder = yMotion - yAction;
            }

            int gyroInvert = Global.getGyroInvert(deviceNumber);
            if ((gyroInvert & 0x02) == 2)
                xAction *= -1;

            if ((gyroInvert & 0x01) == 1)
                yAction *= -1;

            if (yAction != 0 || xAction != 0)
                InputMethods.MoveCursorBy(xAction, yAction);

            hDirection = xMotion > 0.0 ? Direction.Positive : xMotion < 0.0 ? Direction.Negative : Direction.Neutral;
            vDirection = yMotion > 0.0 ? Direction.Positive : yMotion < 0.0 ? Direction.Negative : Direction.Neutral;
        }

        public void mouseRemainderReset()
        {
            hRemainder = vRemainder = 0.0;
            int iIndex = smoothBufferTail % SMOOTH_BUFFER_LEN;
            xSmoothBuffer[iIndex] = 0.0;
            ySmoothBuffer[iIndex] = 0.0;
            smoothBufferTail = iIndex + 1;
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
        public void touchesMoved(TouchpadEventArgs arg, bool dragging, bool disableInvert = false)
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

            double coefficient = Global.TouchSensitivity[deviceNumber] * 0.01;
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

            if (disableInvert == false)
            {
                int touchpadInvert = tempInt = Global.getTouchpadInvert(deviceNumber);
                if ((touchpadInvert & 0x02) == 2)
                    xAction *= -1;

                if ((touchpadInvert & 0x01) == 1)
                    yAction *= -1;
            }

            if (yAction != 0 || xAction != 0)
                InputMethods.MoveCursorBy(xAction, yAction);

            horizontalDirection = xMotion > 0.0 ? Direction.Positive : xMotion < 0.0 ? Direction.Negative : Direction.Neutral;
            verticalDirection = yMotion > 0.0 ? Direction.Positive : yMotion < 0.0 ? Direction.Negative : Direction.Neutral;
        }
    }
}
