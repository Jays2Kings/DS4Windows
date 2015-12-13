using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS4Windows
{
    public class SixAxisEventArgs : EventArgs
    {
        public readonly SixAxis sixAxis;
        public readonly System.DateTime timeStamp;
        public SixAxisEventArgs(System.DateTime utcTimestamp, SixAxis sa)
        {
            sixAxis = sa;
            this.timeStamp = utcTimestamp;
        }
    }

    public class SixAxis
    {
        public readonly int gyroX, gyroY, gyroZ, deltaX, deltaY, deltaZ, accelX, accelY, accelZ;
        public readonly byte touchID;
        public readonly SixAxis previousAxis;
        public SixAxis(int X, int Y, int Z, int aX, int aY, int aZ, SixAxis prevAxis = null)
        {
            gyroX = X;
            gyroY = Y;
            gyroZ = Z;
            accelX = aX;
            accelY = aY;
            accelZ = aZ;
            previousAxis = prevAxis;
            if (previousAxis != null)
            {
                deltaX = X - previousAxis.gyroX;
                deltaY = Y - previousAxis.gyroY;
                deltaZ = Z - previousAxis.gyroZ;
            }
        }
    }

    public class DS4SixAxis
    {
        public event EventHandler<SixAxisEventArgs> SixAxisMoved = null; // deltaX/deltaY are set because one or both fingers were already down on a prior sensor reading
        public event EventHandler<SixAxisEventArgs> SixAccelMoved = null; // no status change for the touchpad itself... but other sensors may have changed, or you may just want to do some processing

        internal int lastGyroX, lastGyroY, lastGyroZ, lastAX, lastAY, lastAZ; // tracks 0, 1 or 2 touches; we maintain touch 1 and 2 separately
        internal byte[] previousPacket = new byte[8];
        

        public void handleSixaxis(byte[] gyro, byte[] accel, DS4State state)
        {
            //bool touchPadIsDown = sensors.TouchButton;
            /*if (!PacketChanged(data, touchPacketOffset) && touchPadIsDown == lastTouchPadIsDown)
            {
                if (SixAxisUnchanged != null)
                    SixAxisUnchanged(this, EventArgs.Empty);
                return;
            }*/
            /* byte touchID1 = (byte)(data[0 + TOUCHPAD_DATA_OFFSET + touchPacketOffset] & 0x7F);
             byte touchID2 = (byte)(data[4 + TOUCHPAD_DATA_OFFSET + touchPacketOffset] & 0x7F);*/
            int currentX = (short)((ushort)(gyro[0] << 8) | gyro[1]) / 64;
            int currentY = (short)((ushort)(gyro[2] << 8) | gyro[3]) / 64;
            int currentZ = (short)((ushort)(gyro[4] << 8) | gyro[5]) / 64;
            int AccelX = (short)((ushort)(accel[2] << 8) | accel[3]) / 256;
            int AccelY = (short)((ushort)(accel[0] << 8) | accel[1]) / 256;
            int AccelZ = (short)((ushort)(accel[4] << 8) | accel[5]) / 256;
            SixAxisEventArgs args;
            //if (sensors.Touch1 || sensors.Touch2)
            {
               /* if (SixAxisMoved != null)
                {
                    SixAxis sPrev, now;
                    sPrev = new SixAxis(lastGyroX, lastGyroY, lastGyroZ, lastAX,lastAY,lastAZ);
                    now = new SixAxis(currentX, currentY, currentZ, AccelX, AccelY, AccelZ, sPrev);
                    args = new SixAxisEventArgs(state.ReportTimeStamp, now);
                    SixAxisMoved(this, args);
                }

                lastGyroX = currentX;
                lastGyroY = currentY;
                lastGyroZ = currentZ;
                lastAX = AccelX;
                lastAY = AccelY;
                lastAZ = AccelZ;*/
            }
            if (AccelX != 0 || AccelY != 0 || AccelZ != 0)
            {
                if (SixAccelMoved != null)
                {
                    SixAxis sPrev, now;
                    sPrev = new SixAxis(lastGyroX, lastGyroY, lastGyroZ, lastAX, lastAY, lastAZ);
                    now = new SixAxis(currentX, currentY, currentZ, AccelX, AccelY, AccelZ, sPrev);
                    args = new SixAxisEventArgs(state.ReportTimeStamp, now);
                    SixAccelMoved(this, args);
                }

                lastGyroX = currentX;
                lastGyroY = currentY;
                lastGyroZ = currentZ;
                lastAX = AccelX;
                lastAY = AccelY;
                lastAZ = AccelZ;
            }
        }
    }
}
