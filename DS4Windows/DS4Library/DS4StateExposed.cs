
namespace DS4Windows
{
    public class DS4StateExposed
    {
        private DS4State _state;
        private byte[] accel = new byte[] { 0, 0, 0, 0, 0, 0 },
            gyro = new byte[] { 0, 0, 0, 0, 0, 0 };

        public DS4StateExposed()
        {
            _state = new DS4State();
        }
        public DS4StateExposed(DS4State state)
        {
            _state = state;
        }

        bool Square { get { return _state.Square; } }  
        bool Triangle { get { return _state.Triangle; } }  
        bool Circle { get { return _state.Circle; } }  
        bool Cross { get { return _state.Cross; } } 
        bool DpadUp { get { return _state.DpadUp; } }  
        bool DpadDown { get { return _state.DpadDown; } }  
        bool DpadLeft { get { return _state.DpadLeft; } }  
        bool DpadRight { get { return _state.DpadRight; } } 
        bool L1 { get { return _state.L1; } }  
        bool L3 { get { return _state.L3; } }  
        bool R1 { get { return _state.R1; } }  
        bool R3 { get { return _state.R3; } } 
        bool Share { get { return _state.Share; } }  
        bool Options { get { return _state.Options; } }  
        bool PS { get { return _state.PS; } }  
        bool Touch1 { get { return _state.Touch1; } }  
        bool Touch2 { get { return _state.Touch2; } }  
        bool TouchButton { get { return _state.TouchButton; } } 
        bool Touch1Finger { get { return _state.Touch1Finger;  } }
        bool Touch2Fingers { get { return _state.Touch2Fingers; } }
        byte LX { get { return _state.LX; } }  
        byte RX { get { return _state.RX; } }  
        byte LY { get { return _state.LY; } }  
        byte RY { get { return _state.RY; } }  
        byte L2 { get { return _state.L2; } }  
        byte R2 { get { return _state.R2; } } 
        int Battery { get { return _state.Battery; } }

        public byte[] Accel { set { accel = value; } }
        public void setAccel(byte[] value)
        {
            accel = value;
        }

        public byte[] Gyro { set { gyro = value; } }
        public void setGyro(byte[] value)
        {
            gyro = value;
        }

        public int GyroYaw { get { return -(short)((ushort)(gyro[3] << 8) | gyro[2]) / 256; } }
        public int GyroPitch { get { return (short)((ushort)(gyro[1] << 8) | gyro[0]) / 256; } }
        public int GyroRoll { get { return -(short)((ushort)(gyro[5] << 8) | gyro[4]) / 256; } }

        public int AccelX { get { return (short)((ushort)(accel[1] << 8) | accel[0]) / 64; } }
        public int getAccelX()
        {
            return (short)((ushort)(accel[1] << 8) | accel[0]) / 64;
        }

        public int AccelY { get { return (short)((ushort)(accel[3] << 8) | accel[2]) / 64; } }
        public int getAccelY()
        {
            return (short)((ushort)(accel[3] << 8) | accel[2]) / 64;
        }

        public int AccelZ { get { return (short)((ushort)(accel[5] << 8) | accel[4]) / 64; } }
        public int getAccelZ()
        {
            return (short)((ushort)(accel[5] << 8) | accel[4]) / 64;
        }
    }
}
