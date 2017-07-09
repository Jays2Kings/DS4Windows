
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

        /// <summary> Holds raw DS4 input data from 14 to 19 </summary>
        public byte[] Accel { set { accel = value; } }
        public void setAccel(byte[] value)
        {
            accel = value;
        }

        /// <summary> Holds raw DS4 input data from 20 to 25 </summary>
        public byte[] Gyro { set { gyro = value; } }
        public void setGyro(byte[] value)
        {
            gyro = value;
        }

        /// <summary> Yaw leftward/counter-clockwise/turn to port or larboard side </summary>
        /// <remarks> Add double the previous result to this delta and divide by three.</remarks>
        //public int AccelX { get { return (short)((ushort)(accel[2] << 8) | accel[3]) / 256; } }
        //public int AccelX { get { return (short)((ushort)(accel[1] << 8) | accel[0]) / 64; } }
        public int AccelX { get { return (short)((ushort)(gyro[3] << 8) | gyro[2]) / 256; } }

        /// <summary> Pitch upward/backward </summary>
        /// <remarks> Add double the previous result to this delta and divide by three.</remarks>
        //public int AccelY { get { return (short)((ushort)(accel[0] << 8) | accel[1] ) / 256; } }
        //public int AccelY { get { return (short)((ushort)(accel[3] << 8) | accel[2]) / 64; } }
        public int AccelY { get { return (short)((ushort)(gyro[1] << 8) | gyro[0]) / 256; } }

        /// <summary> roll left/L side of controller down/starboard raising up </summary>
        /// <remarks> Add double the previous result to this delta and divide by three.</remarks>
        //public int AccelZ { get { return (short)((ushort)(accel[4] << 8) | accel[5]) / 256; } }
        //public int AccelZ { get { return (short)((ushort)(accel[5] << 8) | accel[4]) / 64; } }
        public int AccelZ { get { return (short)((ushort)(gyro[5] << 8) | gyro[4]) / 256; } }

        /// <summary> R side of controller upward </summary>
        /// <remarks> Add double the previous result to this delta and divide by three.</remarks>
        //public int GyroX { get { return (short)((ushort)(gyro[0] << 8) | gyro[1]) / 64; } }
        //public int GyroX { get { return (short)((ushort)(gyro[3] << 8) | gyro[2]) / 256; } }
        public int GyroX { get { return (short)((ushort)(accel[1] << 8) | accel[0]) / 64; } }

        public int getGyroX()
        {
            //return (short)((ushort)(gyro[0] << 8) | gyro[1]) / 64;
            //return (short)((ushort)(gyro[3] << 8) | gyro[2]) / 256;
            return (short)((ushort)(accel[1] << 8) | accel[0]) / 64;
        }

        /// <summary> touchpad and button face side of controller upward </summary>
        /// <remarks> Add double the previous result to this delta and divide by three.</remarks>
        //public int GyroY { get { return (short)((ushort)(gyro[2] << 8) | gyro[3]) / 64; } }
        //public int GyroY { get { return (short)((ushort)(gyro[1] << 8) | gyro[0]) / 256; } }
        public int GyroY { get { return (short)((ushort)(accel[3] << 8) | accel[2]) / 64; } }

        public int getGyroY()
        {
            //return (short)((ushort)(gyro[2] << 8) | gyro[3]) / 64;
            //return (short)((ushort)(gyro[1] << 8) | gyro[0]) / 256;
            return (short)((ushort)(accel[3] << 8) | accel[2]) / 64;
        }

        /// <summary> Audio/expansion ports upward and light bar/shoulders/bumpers/USB port downward </summary>
        /// <remarks> Add double the previous result to this delta and divide by three.</remarks>
        //public int GyroZ { get { return (short)((ushort)(gyro[4] << 8) | gyro[5]) / 64; } }
        //public int GyroZ { get { return (short)((ushort)(gyro[5] << 8) | gyro[4]) / 256; } }
        public int GyroZ { get { return (short)((ushort)(accel[5] << 8) | accel[4]) / 64; } }

        public int getGyroZ()
        {
            //return (short)((ushort)(gyro[4] << 8) | gyro[5]) / 64;
            //return (short)((ushort)(gyro[5] << 8) | gyro[4]) / 256;
            return (short)((ushort)(accel[5] << 8) | accel[4]) / 64;
        }
    }
}
