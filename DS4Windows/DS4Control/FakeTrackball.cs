using System;

namespace DS4WinWPF.DS4Control
{
    public class FakeTrackball
    {
        public const int TRACKBALL_INIT_FICTION = 10;
        public const int TRACKBALL_MASS = 45;
        public const double TRACKBALL_RADIUS = 0.0245;
        private double TRACKBALL_INERTIA = 2.0 * (TRACKBALL_MASS * TRACKBALL_RADIUS * TRACKBALL_RADIUS) / 5.0;
        private double TRACKBALL_SCALE = 0.004;
        private const int TRACKBALL_BUFFER_LEN = 8;

        private double[] trackballXBuffer = new double[TRACKBALL_BUFFER_LEN];
        private double[] trackballYBuffer = new double[TRACKBALL_BUFFER_LEN];
        private int trackballBufferTail = 0;
        private int trackballBufferHead = 0;
        private double trackballAccel = 0.0;
        private double trackballXVel = 0.0;
        private double trackballYVel = 0.0;
        //private bool trackballActive = false;
        private double trackballDXRemain = 0.0;
        private double trackballDYRemain = 0.0;

        public FakeTrackball()
        {
            ResetAccel(TRACKBALL_INIT_FICTION);
        }

        public FakeTrackball(double friction)
        {
            ResetAccel(friction);
        }

        public void ResetAccel(double friction)
        {
            trackballAccel = TRACKBALL_RADIUS * friction / TRACKBALL_INERTIA;
        }

        public void AddData(int x, int y)
        {
            int iIndex = trackballBufferTail;
            // Establish 4 ms as the base
            trackballXBuffer[iIndex] = (x * TRACKBALL_SCALE) / 0.004; // dev.getCurrentStateRef().elapsedTime;
            trackballYBuffer[iIndex] = (y * TRACKBALL_SCALE) / 0.004; // dev.getCurrentStateRef().elapsedTime;
            trackballBufferTail = (iIndex + 1) % TRACKBALL_BUFFER_LEN;
            if (trackballBufferHead == trackballBufferTail)
                trackballBufferHead = (trackballBufferHead + 1) % TRACKBALL_BUFFER_LEN;
        }

        public void AddEmptyTrackballEntry()
        {
            int iIndex = trackballBufferTail;
            trackballXBuffer[iIndex] = 0;
            trackballYBuffer[iIndex] = 0;
            trackballBufferTail = (iIndex + 1) % TRACKBALL_BUFFER_LEN;
            if (trackballBufferHead == trackballBufferTail)
                trackballBufferHead = (trackballBufferHead + 1) % TRACKBALL_BUFFER_LEN;
        }

        public void CalcTrackballInitVelocities()
        {
            double currentWeight = 1.0;
            double finalWeight = 0.0;
            double x_out = 0.0, y_out = 0.0;
            int idx = -1;
            for (int i = 0; i < TRACKBALL_BUFFER_LEN && idx != trackballBufferHead; i++)
            {
                idx = (trackballBufferTail - i - 1 + TRACKBALL_BUFFER_LEN) % TRACKBALL_BUFFER_LEN;
                x_out += trackballXBuffer[idx] * currentWeight;
                y_out += trackballYBuffer[idx] * currentWeight;
                finalWeight += currentWeight;
                currentWeight *= 1.0;
            }

            x_out /= finalWeight;
            trackballXVel = x_out;
            y_out /= finalWeight;
            trackballYVel = y_out;
        }

        public void StartTrackball(double elapsedTime, out int x, out int y)
        {
            CalcTrackballInitVelocities();
            Process(elapsedTime, out x, out y);
        }

        public void Process(double elapsedTime, out int x, out int y)
        {
            x = 0;
            y = 0;

            //trackballActive = true;

            double tempAngle = Math.Atan2(-trackballYVel, trackballXVel);
            double normX = Math.Abs(Math.Cos(tempAngle));
            double normY = Math.Abs(Math.Sin(tempAngle));
            int signX = Math.Sign(trackballXVel);
            int signY = Math.Sign(trackballYVel);

            double trackXvDecay = Math.Min(Math.Abs(trackballXVel), trackballAccel * elapsedTime * normX);
            double trackYvDecay = Math.Min(Math.Abs(trackballYVel), trackballAccel * elapsedTime * normY);
            double xVNew = trackballXVel - (trackXvDecay * signX);
            double yVNew = trackballYVel - (trackYvDecay * signY);
            double xMotion = (xVNew * elapsedTime) / TRACKBALL_SCALE;
            double yMotion = (yVNew * elapsedTime) / TRACKBALL_SCALE;
            if (xMotion != 0.0)
            {
                xMotion += trackballDXRemain;
            }
            else
            {
                trackballDXRemain = 0.0;
            }

            int dx = (int)xMotion;
            trackballDXRemain = xMotion - dx;

            if (yMotion != 0.0)
            {
                yMotion += trackballDYRemain;
            }
            else
            {
                trackballDYRemain = 0.0;
            }

            int dy = (int)yMotion;
            trackballDYRemain = yMotion - dy;

            trackballXVel = xVNew;
            trackballYVel = yVNew;

            x = dx;
            y = dy;

            //if (dx == 0 && dy == 0)
            //{
            //    trackballActive = false;
            //}
            //else
            //{
            //    TouchpadMouseStick(dx, dy);
            //}
        }

        public void ResetBuffers()
        {
            Array.Clear(trackballXBuffer, 0, TRACKBALL_BUFFER_LEN);
            Array.Clear(trackballYBuffer, 0, TRACKBALL_BUFFER_LEN);
            trackballXVel = 0.0;
            trackballYVel = 0.0;
            //trackballActive = false;
            trackballBufferTail = 0;
            trackballBufferHead = 0;
            trackballDXRemain = 0.0;
            trackballDYRemain = 0.0;
        }

        public void Reset()
        {
            ResetBuffers();
            ResetAccel(TRACKBALL_INIT_FICTION);
        }
    }
}
