using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DS4Library
{
    public class DS4State
    {
        public bool Square, Triangle, Circle, Cross;
        public bool DpadUp, DpadDown, DpadLeft, DpadRight;
        public bool L1, L3, R1, R3;
        public bool Share, Options, PS, Touch1, Touch2, TouchButton;
        public byte LX, RX, LY, RY, L2, R2;
        public int Battery;
        public DateTime ReportTimeStamp;
        public DS4State()
        {
            ReportTimeStamp = DateTime.UtcNow;
            Square = Triangle = Circle = Cross = false;
            DpadUp = DpadDown = DpadLeft = DpadRight = false;
            L1 = L3 = R1 = R3 = false;
            Share = Options = PS = Touch1 = Touch2 = TouchButton = false;
            LX = RX = LY = RY = 127;
            L2 = R2 = 0;
            Battery = 0;
        }

        public DS4State(DS4State state)
        {
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
            Touch2 = state.Touch2;
            TouchButton = state.TouchButton;
            LX = state.LX;
            RX = state.RX;
            LY = state.LY;
            RY = state.RY;
            Battery = state.Battery;
            ReportTimeStamp = state.ReportTimeStamp;
        }

        public DS4State Clone()
        {
            return new DS4State(this);
        }

        public void Copy(DS4State state)
        {
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
            state.Touch2 = Touch2;
            state.TouchButton = TouchButton;
            state.LX = LX;
            state.RX = RX;
            state.LY = LY;
            state.RY = RY;
            state.Battery = Battery;
            state.ReportTimeStamp = ReportTimeStamp;
        }

    }
}
