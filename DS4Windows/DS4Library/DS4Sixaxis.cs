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
        public int gyroYawFull, gyroPitchFull, gyroRollFull;
        public int accelXFull, accelYFull, accelZFull;
        public double elapsed;
        public SixAxis previousAxis = null;

        private double tempDouble = 0d;

        public SixAxis(int X, int Y, int Z,
            int aX, int aY, int aZ,
            double elapsedDelta, SixAxis prevAxis = null)
        {
            populate(X, Y, Z, aX, aY, aZ, elapsedDelta, prevAxis);
        }

        public void copy(SixAxis src)
        {
            gyroYaw = src.gyroYaw;
            gyroPitch = src.gyroPitch;
            gyroRoll = src.gyroRoll;

            gyroYawFull = src.gyroYawFull;
            accelXFull = src.accelXFull; accelYFull = src.accelYFull; accelZFull = src.accelZFull;

            angVelYaw = src.angVelYaw;
            angVelPitch = src.angVelPitch;
            angVelRoll = src.angVelRoll;

            accelXG = src.accelXG;
            accelYG = src.accelYG;
            accelZG = src.accelZG;

            // Put accel ranges between 0 - 128 abs
            accelX = src.accelX;
            accelY = src.accelY;
            accelZ = src.accelZ;
            outputAccelX = accelX;
            outputAccelY = accelY;
            outputAccelZ = accelZ;

            elapsed = src.elapsed;
            previousAxis = src.previousAxis;
        }

        public void populate(int X, int Y, int Z,
            int aX, int aY, int aZ,
            double elapsedDelta, SixAxis prevAxis = null)
        {
            gyroYaw = -X / 256;
            gyroPitch = Y / 256;
            gyroRoll = -Z / 256;

            gyroYawFull = -X; gyroPitchFull = Y; gyroRollFull = -Z;
            accelXFull = -aX; accelYFull = -aY; accelZFull = aZ;

            angVelYaw = gyroYawFull / F_GYRO_RES_IN_DEG_SEC;
            angVelPitch = gyroPitchFull / F_GYRO_RES_IN_DEG_SEC;
            angVelRoll = gyroRollFull / F_GYRO_RES_IN_DEG_SEC;

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

    internal class CalibData
    {
        public int bias;
        public int sensNumer;
        public int sensDenom;
        public const int GyroPitchIdx = 0, GyroYawIdx = 1, GyroRollIdx = 2,
        AccelXIdx = 3, AccelYIdx = 4, AccelZIdx = 5;
    }

    public class DS4SixAxis
    {
        public event EventHandler<SixAxisEventArgs> SixAccelMoved = null;
        private SixAxis sPrev = null, now = null;
        private CalibData[] calibrationData = new CalibData[6] { new CalibData(), new CalibData(),
            new CalibData(), new CalibData(), new CalibData(), new CalibData()
        };

        public DS4SixAxis()
        {
            sPrev = new SixAxis(0, 0, 0, 0, 0, 0, 0.0);
            now = new SixAxis(0, 0, 0, 0, 0, 0, 0.0);
        }

        int temInt = 0;
        public void setCalibrationData(ref byte[] calibData, bool fromUSB)
        {
            int pitchPlus, pitchMinus, yawPlus, yawMinus, rollPlus, rollMinus,
                accelXPlus, accelXMinus, accelYPlus, accelYMinus, accelZPlus, accelZMinus,
                gyroSpeedPlus, gyroSpeedMinus;

            calibrationData[0].bias = (short)((ushort)(calibData[2] << 8) | calibData[1]);
            calibrationData[1].bias = (short)((ushort)(calibData[4] << 8) | calibData[3]);
            calibrationData[2].bias = (short)((ushort)(calibData[6] << 8) | calibData[5]);

            if (!fromUSB)
            {
                pitchPlus = temInt = (short)((ushort)(calibData[8] << 8) | calibData[7]);
                yawPlus = temInt = (short)((ushort)(calibData[10] << 8) | calibData[9]);
                rollPlus = temInt = (short)((ushort)(calibData[12] << 8) | calibData[11]);
                pitchMinus = temInt = (short)((ushort)(calibData[14] << 8) | calibData[13]);
                yawMinus = temInt = (short)((ushort)(calibData[16] << 8) | calibData[15]);
                rollMinus = temInt = (short)((ushort)(calibData[18] << 8) | calibData[17]);
            }
            else
            {
                pitchPlus = temInt = (short)((ushort)(calibData[8] << 8) | calibData[7]);
                pitchMinus = temInt = (short)((ushort)(calibData[10] << 8) | calibData[9]);
                yawPlus = temInt = (short)((ushort)(calibData[12] << 8) | calibData[11]);
                yawMinus = temInt = (short)((ushort)(calibData[14] << 8) | calibData[13]);
                rollPlus = temInt = (short)((ushort)(calibData[16] << 8) | calibData[15]);
                rollMinus = temInt = (short)((ushort)(calibData[18] << 8) | calibData[17]);
            }

            gyroSpeedPlus = temInt = (short)((ushort)(calibData[20] << 8) | calibData[19]);
            gyroSpeedMinus = temInt = (short)((ushort)(calibData[22] << 8) | calibData[21]);
            accelXPlus = temInt = (short)((ushort)(calibData[24] << 8) | calibData[23]);
            accelXMinus = temInt = (short)((ushort)(calibData[26] << 8) | calibData[25]);

            accelYPlus = temInt = (short)((ushort)(calibData[28] << 8) | calibData[27]);
            accelYMinus = temInt = (short)((ushort)(calibData[30] << 8) | calibData[29]);

            accelZPlus = temInt = (short)((ushort)(calibData[32] << 8) | calibData[31]);
            accelZMinus = temInt = (short)((ushort)(calibData[34] << 8) | calibData[33]);

            int gyroSpeed2x = temInt = (gyroSpeedPlus + gyroSpeedMinus);
            calibrationData[0].sensNumer = gyroSpeed2x* SixAxis.GYRO_RES_IN_DEG_SEC;
            calibrationData[0].sensDenom = pitchPlus - pitchMinus;

            calibrationData[1].sensNumer = gyroSpeed2x* SixAxis.GYRO_RES_IN_DEG_SEC;
            calibrationData[1].sensDenom = yawPlus - yawMinus;

            calibrationData[2].sensNumer = gyroSpeed2x* SixAxis.GYRO_RES_IN_DEG_SEC;
            calibrationData[2].sensDenom = rollPlus - rollMinus;

            int accelRange = temInt = accelXPlus - accelXMinus;
            calibrationData[3].bias = accelXPlus - accelRange / 2;
            calibrationData[3].sensNumer = 2 * SixAxis.ACC_RES_PER_G;
            calibrationData[3].sensDenom = accelRange;

            accelRange = temInt = accelYPlus - accelYMinus;
            calibrationData[4].bias = accelYPlus - accelRange / 2;
            calibrationData[4].sensNumer = 2 * SixAxis.ACC_RES_PER_G;
            calibrationData[4].sensDenom = accelRange;

            accelRange = temInt = accelZPlus - accelZMinus;
            calibrationData[5].bias = accelZPlus - accelRange / 2;
            calibrationData[5].sensNumer = 2 * SixAxis.ACC_RES_PER_G;
            calibrationData[5].sensDenom = accelRange;
        }

        private void applyCalibs(ref int yaw, ref int pitch, ref int roll,
            ref int accelX, ref int accelY, ref int accelZ)
        {
            CalibData current = calibrationData[0];
            temInt = pitch - current.bias;
            pitch = temInt = (int)(temInt * (current.sensNumer / (float)current.sensDenom));

            current = calibrationData[1];
            temInt = yaw - current.bias;
            yaw = temInt = (int)(temInt * (current.sensNumer / (float)current.sensDenom));

            current = calibrationData[2];
            temInt = roll - current.bias;
            roll = temInt = (int)(temInt * (current.sensNumer / (float)current.sensDenom));

            current = calibrationData[3];
            temInt = accelX - current.bias;
            accelX = temInt = (int)(temInt * (current.sensNumer / (float)current.sensDenom));

            current = calibrationData[4];
            temInt = accelY - current.bias;
            accelY = temInt = (int)(temInt * (current.sensNumer / (float)current.sensDenom));

            current = calibrationData[5];
            temInt = accelZ - current.bias;
            accelZ = temInt = (int)(temInt * (current.sensNumer / (float)current.sensDenom));
        }

        public void handleSixaxis(byte[] gyro, byte[] accel, DS4State state,
            double elapsedDelta)
        {
            int currentYaw = (short)((ushort)(gyro[3] << 8) | gyro[2]);
            int currentPitch = (short)((ushort)(gyro[1] << 8) | gyro[0]);
            int currentRoll = (short)((ushort)(gyro[5] << 8) | gyro[4]);
            int AccelX = (short)((ushort)(accel[1] << 8) | accel[0]);
            int AccelY = (short)((ushort)(accel[3] << 8) | accel[2]);
            int AccelZ = (short)((ushort)(accel[5] << 8) | accel[4]);

            applyCalibs(ref currentYaw, ref currentPitch, ref currentRoll, ref AccelX, ref AccelY, ref AccelZ);

            SixAxisEventArgs args = null;
            if (AccelX != 0 || AccelY != 0 || AccelZ != 0)
            {
                if (SixAccelMoved != null)
                {
                    sPrev.copy(now);
                    now.populate(currentYaw, currentPitch, currentRoll,
                        AccelX, AccelY, AccelZ, elapsedDelta, sPrev);

                    args = new SixAxisEventArgs(state.ReportTimeStamp, now);
                    state.Motion = now;
                    SixAccelMoved(this, args);
                }
            }
        }
    }
}
