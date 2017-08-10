using System;

namespace DS4Windows
{
    public class SixAxisEventArgs : EventArgs
    {
        public readonly SixAxis sixAxis;
        public readonly DateTime timeStamp;
        public SixAxisEventArgs(DateTime utcTimestamp, SixAxis sa)
        {
            sixAxis = sa;
            timeStamp = utcTimestamp;
        }
    }

    public class SixAxis
    {
        public const int ACC_RES_PER_G = 8192;
        private const float F_ACC_RES_PER_G = ACC_RES_PER_G;
        public const int GYRO_RES_IN_DEG_SEC = 16;
        private const float F_GYRO_RES_IN_DEG_SEC = GYRO_RES_IN_DEG_SEC;

        public int gyroYaw, gyroPitch, gyroRoll, accelX, accelY, accelZ;
        public int outputAccelX, outputAccelY, outputAccelZ;
        public double accelXG, accelYG, accelZG;
        public double angVelYaw, angVelPitch, angVelRoll;
        public readonly int gyroYawFull, gyroPitchFull, gyroRollFull;
        public readonly int accelXFull, accelYFull, accelZFull;
        public readonly byte touchID;
        public readonly double elapsed;
        public readonly SixAxis previousAxis = null;

        double recip = 1d / 8192d;
        double tempDouble = 0d;

        public SixAxis(int X, int Y, int Z,
            int aX, int aY, int aZ,
            double elapsedDelta, SixAxis prevAxis = null)
        {
            gyroYaw = -X / 256;
            gyroPitch = Y / 256;
            gyroRoll = -Z / 256;

            gyroYawFull = -X; gyroPitchFull = Y; gyroRollFull = -Z;
            accelXFull = -aX; accelYFull = -aY; accelZFull = aZ;

            angVelYaw = tempDouble = gyroYawFull / F_GYRO_RES_IN_DEG_SEC;
            angVelPitch = tempDouble = gyroPitchFull / F_GYRO_RES_IN_DEG_SEC;
            angVelRoll = tempDouble = gyroRollFull / F_GYRO_RES_IN_DEG_SEC;

            accelXG = tempDouble = accelXFull / F_ACC_RES_PER_G;
            accelYG = tempDouble = accelYFull / F_ACC_RES_PER_G;
            accelZG = tempDouble = accelZFull / F_ACC_RES_PER_G;

            // Put accel ranges between 0 - 128 abs
            accelX = -aX / 64;
            accelY = -aY / 64;
            accelZ = aZ / 64;
            outputAccelX = accelX;
            outputAccelY = accelY;
            outputAccelZ = accelZ;

            elapsed = elapsedDelta;
            previousAxis = prevAxis;
        }
    }

    public class DS4SixAxis
    {
        public event EventHandler<SixAxisEventArgs> SixAccelMoved = null;

        private int lastGyroYaw, lastGyroPitch, lastGyroRoll,
            lastAX, lastAY, lastAZ;

        private double lastElapsedDelta;
        private byte[] previousPacket = new byte[8];

        public void handleSixaxis(byte[] gyro, byte[] accel, DS4State state,
            double elapsedDelta)
        {
            int currentYaw = (short)((ushort)(gyro[3] << 8) | gyro[2]);
            int currentPitch = (short)((ushort)(gyro[1] << 8) | gyro[0]);
            int currentRoll = (short)((ushort)(gyro[5] << 8) | gyro[4]);
            int AccelX = (short)((ushort)(accel[1] << 8) | accel[0]);
            int AccelY = (short)((ushort)(accel[3] << 8) | accel[2]);
            int AccelZ = (short)((ushort)(accel[5] << 8) | accel[4]);

            SixAxisEventArgs args = null;
            if (AccelX != 0 || AccelY != 0 || AccelZ != 0)
            {
                if (SixAccelMoved != null)
                {
                    SixAxis sPrev = null, now = null;
                    sPrev = new SixAxis(lastGyroYaw, lastGyroPitch, lastGyroRoll,
                        lastAX, lastAY, lastAZ, lastElapsedDelta);

                    now = new SixAxis(currentYaw, currentPitch, currentRoll,
                        AccelX, AccelY, AccelZ, elapsedDelta, sPrev);

                    args = new SixAxisEventArgs(state.ReportTimeStamp, now);
                    state.Motion = now;
                    SixAccelMoved(this, args);
                }

                lastGyroYaw = currentYaw;
                lastGyroPitch = currentPitch;
                lastGyroRoll = currentRoll;
                lastAX = AccelX;
                lastAY = AccelY;
                lastAZ = AccelZ;
                lastElapsedDelta = elapsedDelta;
            }
        }
    }
}
