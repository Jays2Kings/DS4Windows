using System;

namespace DS4Windows
{
    public class DS4State
    {
        public DateTime ReportTimeStamp;
        public bool Square, Triangle, Circle, Cross;
        public bool DpadUp, DpadDown, DpadLeft, DpadRight;
        public bool L1, L3, R1, R3;
        public bool Share, Options, PS, Touch1, Touch2, TouchButton, TouchRight,
            TouchLeft, Touch1Finger, Touch2Fingers;
        public byte Touch1Identifier, Touch2Identifier;
        public byte LX, RX, LY, RY, L2, R2;
        public byte FrameCounter; // 0, 1, 2...62, 63, 0....
        public byte TouchPacketCounter; // we break these out automatically
        public byte Battery; // 0 for charging, 10/20/30/40/50/60/70/80/90/100 for percentage of full
        public double LSAngle; // Calculated bearing of the LS X,Y coordinates
        public double RSAngle; // Calculated bearing of the RS X,Y coordinates
        public double LSAngleRad; // Calculated bearing of the LS X,Y coordinates (in radians)
        public double RSAngleRad; // Calculated bearing of the RS X,Y coordinates (in radians)
        public double LXUnit;
        public double LYUnit;
        public double RXUnit;
        public double RYUnit;
        public double elapsedTime = 0.0;
        public SixAxis Motion = null;
        public static readonly int DEFAULT_AXISDIR_VALUE = 127;

        public DS4State()
        {
            Square = Triangle = Circle = Cross = false;
            DpadUp = DpadDown = DpadLeft = DpadRight = false;
            L1 = L3 = R1 = R3 = false;
            Share = Options = PS = Touch1 = Touch2 = TouchButton = TouchRight = TouchLeft = false;
            Touch1Finger = Touch2Fingers = false;
            LX = RX = LY = RY = 127;
            L2 = R2 = 0;
            FrameCounter = 255; // only actually has 6 bits, so this is a null indicator
            TouchPacketCounter = 255; // 8 bits, no great junk value
            Battery = 0;
            LSAngle = 0.0;
            LSAngleRad = 0.0;
            RSAngle = 0.0;
            RSAngleRad = 0.0;
            LXUnit = 0.0;
            LYUnit = 0.0;
            RXUnit = 0.0;
            RYUnit = 0.0;
            elapsedTime = 0.0;
            Motion = new SixAxis(0, 0, 0, 0, 0, 0, 0.0);
        }

        public DS4State(DS4State state)
        {
            ReportTimeStamp = state.ReportTimeStamp;
            Square = state.Square;
            Triangle = state.Triangle;
            Circle = state.Circle;
            Cross = state.Cross;
            DpadUp = state.DpadUp;
            DpadDown = state.DpadDown;
            DpadLeft = state.DpadLeft;
            DpadRight = state.DpadRight;
            L1 = state.L1;
            L2 = state.L2;
            L3 = state.L3;
            R1 = state.R1;
            R2 = state.R2;
            R3 = state.R3;
            Share = state.Share;
            Options = state.Options;
            PS = state.PS;
            Touch1 = state.Touch1;
            TouchRight = state.TouchRight;
            TouchLeft = state.TouchLeft;
            Touch1Identifier = state.Touch1Identifier;
            Touch2 = state.Touch2;
            Touch2Identifier = state.Touch2Identifier;
            TouchButton = state.TouchButton;
            TouchPacketCounter = state.TouchPacketCounter;
            Touch1Finger = state.Touch1Finger;
            Touch2Fingers = state.Touch2Fingers;
            LX = state.LX;
            RX = state.RX;
            LY = state.LY;
            RY = state.RY;
            FrameCounter = state.FrameCounter;
            Battery = state.Battery;
            LSAngle = state.LSAngle;
            LSAngleRad = state.LSAngleRad;
            RSAngle = state.RSAngle;
            RSAngleRad = state.RSAngleRad;
            LXUnit = state.LXUnit;
            LYUnit = state.LYUnit;
            RXUnit = state.RXUnit;
            RYUnit = state.RYUnit;
            elapsedTime = state.elapsedTime;
            Motion = state.Motion;
        }

        public DS4State Clone()
        {
            return new DS4State(this);
        }

        public void CopyTo(DS4State state)
        {
            state.ReportTimeStamp = ReportTimeStamp;
            state.Square = Square;
            state.Triangle = Triangle;
            state.Circle = Circle;
            state.Cross = Cross;
            state.DpadUp = DpadUp;
            state.DpadDown = DpadDown;
            state.DpadLeft = DpadLeft;
            state.DpadRight = DpadRight;
            state.L1 = L1;
            state.L2 = L2;
            state.L3 = L3;
            state.R1 = R1;
            state.R2 = R2;
            state.R3 = R3;
            state.Share = Share;
            state.Options = Options;
            state.PS = PS;
            state.Touch1 = Touch1;
            state.Touch1Identifier = Touch1Identifier;
            state.Touch2 = Touch2;
            state.Touch2Identifier = Touch2Identifier;
            state.TouchLeft = TouchLeft;
            state.TouchRight = TouchRight;
            state.TouchButton = TouchButton;
            state.TouchPacketCounter = TouchPacketCounter;
            state.Touch1Finger = Touch1Finger;
            state.Touch2Fingers = Touch2Fingers;
            state.LX = LX;
            state.RX = RX;
            state.LY = LY;
            state.RY = RY;
            state.FrameCounter = FrameCounter;
            state.Battery = Battery;
            state.LSAngle = LSAngle;
            state.LSAngleRad = LSAngleRad;
            state.RSAngle = RSAngle;
            state.RSAngleRad = RSAngleRad;
            state.LXUnit = LXUnit;
            state.LYUnit = LYUnit;
            state.RXUnit = RXUnit;
            state.RYUnit = RYUnit;
            state.elapsedTime = elapsedTime;
            state.Motion = Motion;
        }

        public void calculateStickAngles()
        {
            double lsangle = Math.Atan2(-(LY - 127), (LX - 127));
            LSAngleRad = lsangle;
            lsangle = (lsangle >= 0 ? lsangle : (2 * Math.PI + lsangle)) * 180 / Math.PI;
            LSAngle = lsangle;
            LXUnit = Math.Abs(Math.Cos(LSAngleRad));
            LYUnit = Math.Abs(Math.Sin(LSAngleRad));

            double rsangle = Math.Atan2(-(RY - 127), (RX - 127));
            RSAngleRad = rsangle;
            rsangle = (rsangle >= 0 ? rsangle : (2 * Math.PI + rsangle)) * 180 / Math.PI;
            RSAngle = rsangle;
            RXUnit = Math.Abs(Math.Cos(RSAngleRad));
            RYUnit = Math.Abs(Math.Sin(RSAngleRad));
        }

        public void rotateLSCoordinates(double rotation)
        {
            double sinAngle = Math.Sin(rotation), cosAngle = Math.Cos(rotation);
            double tempLX = LX - 127.5, tempLY = LY - 127.5;
            LX = (Byte)(Global.Clamp(-127.5, (tempLX * cosAngle - tempLY * sinAngle), 127.5) + 127.5);
            LY = (Byte)(Global.Clamp(-127.5, (tempLX * sinAngle + tempLY * cosAngle), 127.5) + 127.5);
        }

        public void rotateRSCoordinates(double rotation)
        {
            double sinAngle = Math.Sin(rotation), cosAngle = Math.Cos(rotation);
            double tempRX = RX - 127.5, tempRY = RY - 127.5;
            RX = (Byte)(Global.Clamp(-127.5, (tempRX * cosAngle - tempRY * sinAngle), 127.5) + 127.5);
            RY = (Byte)(Global.Clamp(-127.5, (tempRX * sinAngle + tempRY * cosAngle), 127.5) + 127.5);
        }
    }
}
