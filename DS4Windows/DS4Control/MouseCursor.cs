using System;

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

        private const double GYRO_MOUSE_COEFFICIENT = 0.0095;
        private const int GYRO_MOUSE_DEADZONE = 10;
        private const double GYRO_MOUSE_OFFSET = 0.1463;
        private const double GYRO_SMOOTH_MOUSE_OFFSET = 0.14698;
        private const double TOUCHPAD_MOUSE_OFFSET = 0.015;

        private const int SMOOTH_BUFFER_LEN = 3;
        private double[] xSmoothBuffer = new double[SMOOTH_BUFFER_LEN];
        private double[] ySmoothBuffer = new double[SMOOTH_BUFFER_LEN];
        private int smoothBufferTail = 0;

        

        double coefficient = 0.0;
        double verticalScale = 0.0;
        bool gyroSmooth = false;

        int tempInt = 0;
        double tempDouble = 0.0;
        bool tempBool = false;

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

            double tempAngle = Math.Atan2(-deltaY, deltaX);
            double normX = Math.Abs(Math.Cos(tempAngle));
            double normY = Math.Abs(Math.Sin(tempAngle));
            int signX = Math.Sign(deltaX);
            int signY = Math.Sign(deltaY);

            if (deltaX == 0 || (hRemainder > 0 != deltaX > 0))
            {
                hRemainder = 0.0;
            }

            if (deltaY == 0 || (vRemainder > 0 != deltaY > 0))
            {
                vRemainder = 0.0;
            }

            int deadzoneX = (int)Math.Abs(normX * GYRO_MOUSE_DEADZONE);
            int deadzoneY = (int)Math.Abs(normY * GYRO_MOUSE_DEADZONE);

            if (Math.Abs(deltaX) > deadzoneX)
            {
                deltaX -= signX * deadzoneX;
            }
            else
            {
                deltaX = 0;
            }

            if (Math.Abs(deltaY) > deadzoneY)
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
                    idx = (smoothBufferTail - i - 1 + SMOOTH_BUFFER_LEN) % SMOOTH_BUFFER_LEN;
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

            int deltaX = 0, deltaY = 0;
            if (arg.touches[0].touchID != lastTouchID)
            {
                deltaX = deltaY = 0;
                horizontalRemainder = verticalRemainder = 0.0;
                horizontalDirection = verticalDirection = Direction.Neutral;
                lastTouchID = arg.touches[0].touchID;
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

            TouchMoveCursor(deltaX, deltaY, disableInvert);
        }

        public void TouchMoveCursor(int dx, int dy, bool disableInvert = false)
        {
            double tempAngle = Math.Atan2(-dy, dx);
            double normX = Math.Abs(Math.Cos(tempAngle));
            double normY = Math.Abs(Math.Sin(tempAngle));
            int signX = Math.Sign(dx);
            int signY = Math.Sign(dy);
            double coefficient = Global.getTouchSensitivity(deviceNumber) * 0.01;
            bool jitterCompenstation = Global.getTouchpadJitterCompensation(deviceNumber);

            double xMotion = dx != 0 ?
                coefficient * dx + (normX * (TOUCHPAD_MOUSE_OFFSET * signX)) : 0.0;

            double yMotion = dy != 0 ?
                coefficient * dy + (normY * (TOUCHPAD_MOUSE_OFFSET * signY)) : 0.0;

            if (jitterCompenstation)
            {
                double absX = Math.Abs(xMotion);
                if (absX <= normX * 0.15)
                {
                    xMotion = signX * Math.Pow(absX / 0.15f, 1.408) * 0.15;
                }

                double absY = Math.Abs(yMotion);
                if (absY <= normY * 0.15)
                {
                    yMotion = signY * Math.Pow(absY / 0.15f, 1.408) * 0.15;
                }
            }

            // Collect rounding errors instead of losing motion.
            if (xMotion > 0.0 && horizontalRemainder > 0.0)
            {
                xMotion += horizontalRemainder;
            }
            else if (xMotion < 0.0 && horizontalRemainder < 0.0)
            {
                xMotion += horizontalRemainder;
            }
            int xAction = (int)xMotion;
            horizontalRemainder = xMotion - xAction;

            if (yMotion > 0.0 && verticalRemainder > 0.0)
            {
                yMotion += verticalRemainder;
            }
            else if (yMotion < 0.0 && verticalRemainder < 0.0)
            {
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
