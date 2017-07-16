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
        public readonly int gyroX, gyroY, gyroZ, deltaX, deltaY, deltaZ, accelX, accelY, accelZ;
        public readonly int gyroYawFull, gyroPitchFull, gyroRollFull;
        public readonly int accelXFull, accelYFull, accelZFull;
        public readonly byte touchID;
        public readonly double elapsed;
        public readonly SixAxis previousAxis = null;
        public SixAxis(int X, int Y, int Z, int aX, int aY, int aZ, double milliseconds, SixAxis prevAxis = null)
        {
            gyroX = -X / 256;
            gyroY = Y / 256;
            gyroZ = -Z / 256;
            gyroYawFull = -X;
            gyroPitchFull = Y;
            gyroRollFull = -Z;

            accelX = -aX / 64;
            accelY = -aY / 64;
            accelZ = aZ / 64;
            accelXFull = -aX;
            accelYFull = -aY;
            accelZFull = aZ;
            elapsed = milliseconds;

            previousAxis = prevAxis;
            if (previousAxis != null)
            {
                deltaX = gyroX - previousAxis.gyroX;
                deltaY = gyroY - previousAxis.gyroY;
                deltaZ = gyroZ - previousAxis.gyroZ;
            }
        }
    }

    public class DS4SixAxis
    {
        public event EventHandler<SixAxisEventArgs> SixAccelMoved = null; // no status change for the touchpad itself... but other sensors may have changed, or you may just want to do some processing

        internal int lastGyroX, lastGyroY, lastGyroZ, lastAX, lastAY, lastAZ; // tracks 0, 1 or 2 touches; we maintain touch 1 and 2 separately
        internal double lastMilliseconds;
        internal byte[] previousPacket = new byte[8];

        public void handleSixaxis(byte[] gyro, byte[] accel, DS4State state, double milliseconds)
        {
            int currentX = (short)((ushort)(gyro[3] << 8) | gyro[2]); // Gyro Yaw
            int currentY = (short)((ushort)(gyro[1] << 8) | gyro[0]); // Gyro Pitch
            int currentZ = (short)((ushort)(gyro[5] << 8) | gyro[4]); // Gyro Roll
            int AccelX = (short)((ushort)(accel[1] << 8) | accel[0]);
            int AccelY = (short)((ushort)(accel[3] << 8) | accel[2]);
            int AccelZ = (short)((ushort)(accel[5] << 8) | accel[4]);

            SixAxisEventArgs args = null;
            if (AccelX != 0 || AccelY != 0 || AccelZ != 0)
            {
                if (SixAccelMoved != null)
                {
                    SixAxis sPrev = null, now = null;
                    sPrev = new SixAxis(lastGyroX, lastGyroY, lastGyroZ, lastAX, lastAY, lastAZ, lastMilliseconds);
                    now = new SixAxis(currentX, currentY, currentZ, AccelX, AccelY, AccelZ, milliseconds, sPrev);
                    args = new SixAxisEventArgs(state.ReportTimeStamp, now);
                    state.Motion = now;
                    SixAccelMoved(this, args);
                }

                lastGyroX = currentX;
                lastGyroY = currentY;
                lastGyroZ = currentZ;
                lastAX = AccelX;
                lastAY = AccelY;
                lastAZ = AccelZ;
                lastMilliseconds = milliseconds;
            }
        }
    }
}
