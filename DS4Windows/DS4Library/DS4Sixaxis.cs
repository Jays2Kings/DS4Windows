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
        public int gyroYaw, gyroPitch, gyroRoll, accelX, accelY, accelZ;
        public readonly int gyroYawFull, gyroPitchFull, gyroRollFull;
        public int accelXFull, accelYFull, accelZFull;
        public readonly byte touchID;
        public readonly double elapsed;
        public readonly SixAxis previousAxis = null;
        public SixAxis(int X, int Y, int Z, int aX, int aY, int aZ,
            double milliseconds, SixAxis prevAxis = null)
        {
            gyroYaw = -X / 256;
            gyroPitch = Y / 256;
            gyroRoll = -Z / 256;
            gyroYawFull = -X;
            gyroPitchFull = Y;
            gyroRollFull = -Z;

            // Put accel ranges between 0 - 128 abs
            accelX = -aX / 64;
            accelY = -aY / 64;
            accelZ = aZ / 64;

            accelXFull = -aX;
            accelYFull = -aY;
            accelZFull = aZ;
            elapsed = milliseconds;

            previousAxis = prevAxis;
        }
    }

    public class DS4SixAxis
    {
        public event EventHandler<SixAxisEventArgs> SixAccelMoved = null;

        internal int lastGyroYaw, lastGyroPitch, lastGyroRoll,
            lastAX, lastAY, lastAZ;

        internal double lastMilliseconds;
        internal byte[] previousPacket = new byte[8];

        public void handleSixaxis(byte[] gyro, byte[] accel, DS4State state, double milliseconds)
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
                        lastAX, lastAY, lastAZ, lastMilliseconds);

                    now = new SixAxis(currentYaw, currentPitch, currentRoll,
                        AccelX, AccelY, AccelZ, milliseconds, sPrev);

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
                lastMilliseconds = milliseconds;
            }
        }
    }
}
