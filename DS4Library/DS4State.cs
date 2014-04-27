using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DS4Library
{
    public class DS4State
    {
        public DateTime ReportTimeStamp;
        public bool Square, Triangle, Circle, Cross;
        public bool DpadUp, DpadDown, DpadLeft, DpadRight;
        public bool L1, L3, R1, R3;
        public bool Share, Options, PS, Touch1, Touch2, TouchButton, TouchRight, TouchLeft;
        public byte Touch1Identifier, Touch2Identifier;
        public byte LX, RX, LY, RY, L2, R2;
        public byte FrameCounter; // 0, 1, 2...62, 63, 0....
        public byte TouchPacketCounter; // we break these out automatically
        public byte Battery; // 0 for charging, 10/20/30/40/50/60/70/80/90/100 for percentage of full

        public DS4State()
        {
            Square = Triangle = Circle = Cross = false;
            DpadUp = DpadDown = DpadLeft = DpadRight = false;
            L1 = L3 = R1 = R3 = false;
            Share = Options = PS = Touch1 = Touch2 = TouchButton =  TouchRight = TouchLeft = false;
            LX = RX = LY = RY = 127;
            L2 = R2 = 0;
            FrameCounter = 255; // only actually has 6 bits, so this is a null indicator
            TouchPacketCounter = 255; // 8 bits, no great junk value
            Battery = 0;
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
            LX = state.LX;
            RX = state.RX;
            LY = state.LY;
            RY = state.RY;
            FrameCounter = state.FrameCounter;
            Battery = state.Battery;
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
            state.LX = LX;
            state.RX = RX;
            state.LY = LY;
            state.RY = RY;
            state.FrameCounter = FrameCounter;
            state.Battery = Battery;
        }

    }
}
