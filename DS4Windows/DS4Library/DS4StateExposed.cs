
namespace DS4Windows
{
    public class DS4StateExposed
    {
        private DS4State _state;

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
        bool Touch1Finger { get { return _state.Touch1Finger; } }
        bool Touch2Fingers { get { return _state.Touch2Fingers; } }
        byte LX { get { return _state.LX; } }
        byte RX { get { return _state.RX; } }
        byte LY { get { return _state.LY; } }
        byte RY { get { return _state.RY; } }
        byte L2 { get { return _state.L2; } }
        byte R2 { get { return _state.R2; } }
        int Battery { get { return _state.Battery; } }

        public SixAxis Motion
        {
            get => _state.Motion;
        }

        public int GyroYaw { get { return _state.Motion.gyroYaw; } }
        public int getGyroYaw()
        {
            return _state.Motion.gyroYaw;
        }

        public int GyroPitch { get { return _state.Motion.gyroPitch; } }
        public int getGyroPitch()
        {
            return _state.Motion.gyroPitch;
        }

        public int GyroRoll { get { return _state.Motion.gyroRoll; } }
        public int getGyroRoll()
        {
            return _state.Motion.gyroRoll;
        }

        public int AccelX { get { return _state.Motion.accelX; } }
        public int getAccelX()
        {
            return _state.Motion.accelX;
        }

        public int AccelY { get { return _state.Motion.accelY; } }
        public int getAccelY()
        {
            return _state.Motion.accelY;
        }

        public int AccelZ { get { return _state.Motion.accelZ; } }
        public int getAccelZ()
        {
            return _state.Motion.accelZ;
        }

        public int OutputAccelX { get { return _state.Motion.outputAccelX; } }
        public int getOutputAccelX()
        {
            return _state.Motion.outputAccelX;
        }

        public int OutputAccelY { get { return _state.Motion.outputAccelY; } }
        public int getOutputAccelY()
        {
            return _state.Motion.outputAccelY;
        }

        public int OutputAccelZ { get { return _state.Motion.outputAccelZ; } }
        public int getOutputAccelZ()
        {
            return _state.Motion.outputAccelZ;
        }
    }
}
