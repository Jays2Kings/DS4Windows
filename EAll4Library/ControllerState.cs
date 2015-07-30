using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EAll4Windows
{
    public class ControllerState
    {
        public DateTime ReportTimeStamp;
        public bool X, Y, B, A;
        public bool DpadUp, DpadDown, DpadLeft, DpadRight;
        public bool LB, LS, RB, RS;
        public bool Back, Start, Guide, Touch1, Touch2, TouchButton, TouchRight, TouchLeft;
        public byte Touch1Identifier, Touch2Identifier;
        public byte LX, RX, LY, RY, LT, RT;
        public byte FrameCounter; // 0, 1, 2...62, 63, 0....
        public byte TouchPacketCounter; // we break these out automatically
        public byte Battery; // 0 for charging, 10/20/30/40/50/60/70/80/90/100 for percentage of full

        public ControllerState()
        {
            X = Y = B = A = false;
            DpadUp = DpadDown = DpadLeft = DpadRight = false;
            LB = LS = RB = RS = false;
            Back = Start = Guide = Touch1 = Touch2 = TouchButton = TouchRight = TouchLeft = false;
            LX = RX = LY = RY = 127;
            LT = RT = 0;
            FrameCounter = 255; // only actually has 6 bits, so this is a null indicator
            TouchPacketCounter = 255; // 8 bits, no great junk value
            Battery = 0;
        }

        public ControllerState(ControllerState state)
        {
            ReportTimeStamp = state.ReportTimeStamp;
            X = state.X;
            Y = state.Y;
            B = state.B;
            A = state.A;
            DpadUp = state.DpadUp;
            DpadDown = state.DpadDown;
            DpadLeft = state.DpadLeft;
            DpadRight = state.DpadRight;
            LB = state.LB;
            LT = state.LT;
            LS = state.LS;
            RB = state.RB;
            RT = state.RT;
            RS = state.RS;
            Back = state.Back;
            Start = state.Start;
            Guide = state.Guide;
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

        public ControllerState Clone()
        {
            return new ControllerState(this);
        }

        public void CopyTo(ControllerState state)
        {
            state.ReportTimeStamp = ReportTimeStamp;
            state.X = X;
            state.Y = Y;
            state.B = B;
            state.A = A;
            state.DpadUp = DpadUp;
            state.DpadDown = DpadDown;
            state.DpadLeft = DpadLeft;
            state.DpadRight = DpadRight;
            state.LB = LB;
            state.LT = LT;
            state.LS = LS;
            state.RB = RB;
            state.RT = RT;
            state.RS = RS;
            state.Back = Back;
            state.Start = Start;
            state.Guide = Guide;
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
