using Nefarius.ViGEm.Client;
using Nefarius.ViGEm.Client.Targets.DualShock4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS4Windows
{
    class DS4OutDeviceBasic : DS4OutDevice
    {
        public DS4OutDeviceBasic(ViGEmClient client) : base(client)
        {
        }

        public override void ConvertandSendReport(DS4State state, int device)
        {
            if (!connected) return;

            //cont.ResetReport();
            ushort tempButtons = 0;
            DualShock4DPadDirection tempDPad = DualShock4DPadDirection.None;
            ushort tempSpecial = 0;
            unchecked
            {
                if (state.Share) tempButtons |= DualShock4Button.Share.Value;
                if (state.L3) tempButtons |= DualShock4Button.ThumbLeft.Value;
                if (state.R3) tempButtons |= DualShock4Button.ThumbRight.Value;
                if (state.Options) tempButtons |= DualShock4Button.Options.Value;

                if (state.DpadUp && state.DpadRight) tempDPad = DualShock4DPadDirection.Northeast;
                else if (state.DpadUp && state.DpadLeft) tempDPad = DualShock4DPadDirection.Northwest;
                else if (state.DpadUp) tempDPad = DualShock4DPadDirection.North;
                else if (state.DpadRight && state.DpadDown) tempDPad = DualShock4DPadDirection.Southeast;
                else if (state.DpadRight) tempDPad = DualShock4DPadDirection.East;
                else if (state.DpadDown && state.DpadLeft) tempDPad = DualShock4DPadDirection.Southwest;
                else if (state.DpadDown) tempDPad = DualShock4DPadDirection.South;
                else if (state.DpadLeft) tempDPad = DualShock4DPadDirection.West;

                /*if (state.DpadUp) tempDPad = (state.DpadRight) ? DualShock4DPadValues.Northeast : DualShock4DPadValues.North;
                if (state.DpadRight) tempDPad = (state.DpadDown) ? DualShock4DPadValues.Southeast : DualShock4DPadValues.East;
                if (state.DpadDown) tempDPad = (state.DpadLeft) ? DualShock4DPadValues.Southwest : DualShock4DPadValues.South;
                if (state.DpadLeft) tempDPad = (state.DpadUp) ? DualShock4DPadValues.Northwest : DualShock4DPadValues.West;
                */

                if (state.L1) tempButtons |= DualShock4Button.ShoulderLeft.Value;
                if (state.R1) tempButtons |= DualShock4Button.ShoulderRight.Value;
                //if (state.L2Btn) tempButtons |= DualShock4Buttons.TriggerLeft;
                //if (state.R2Btn) tempButtons |= DualShock4Buttons.TriggerRight;
                if (state.L2 > 0) tempButtons |= DualShock4Button.TriggerLeft.Value;
                if (state.R2 > 0) tempButtons |= DualShock4Button.TriggerRight.Value;

                if (state.Triangle) tempButtons |= DualShock4Button.Triangle.Value;
                if (state.Circle) tempButtons |= DualShock4Button.Circle.Value;
                if (state.Cross) tempButtons |= DualShock4Button.Cross.Value;
                if (state.Square) tempButtons |= DualShock4Button.Square.Value;
                if (state.PS) tempSpecial |= DualShock4SpecialButton.Ps.Value;
                if (state.OutputTouchButton) tempSpecial |= DualShock4SpecialButton.Touchpad.Value;
                cont.SetButtonsFull(tempButtons);
                cont.SetSpecialButtonsFull((byte)tempSpecial);
                cont.SetDPadDirection(tempDPad);
                //report.Buttons = (ushort)tempButtons;
                //report.SpecialButtons = (byte)tempSpecial;
            }

            cont.LeftTrigger = state.L2;
            cont.RightTrigger = state.R2;

            SASteeringWheelEmulationAxisType steeringWheelMappedAxis = Global.GetSASteeringWheelEmulationAxis(device);
            switch (steeringWheelMappedAxis)
            {
                case SASteeringWheelEmulationAxisType.None:
                    cont.LeftThumbX = state.LX;
                    cont.LeftThumbY = state.LY;
                    cont.RightThumbX = state.RX;
                    cont.RightThumbY = state.RY;
                    break;

                case SASteeringWheelEmulationAxisType.LX:
                    cont.LeftThumbX = (byte)state.SASteeringWheelEmulationUnit;
                    cont.LeftThumbY = state.LY;
                    cont.RightThumbX = state.RX;
                    cont.RightThumbY = state.RY;
                    break;

                case SASteeringWheelEmulationAxisType.LY:
                    cont.LeftThumbX = state.LX;
                    cont.LeftThumbY = (byte)state.SASteeringWheelEmulationUnit;
                    cont.RightThumbX = state.RX;
                    cont.RightThumbY = state.RY;
                    break;

                case SASteeringWheelEmulationAxisType.RX:
                    cont.LeftThumbX = state.LX;
                    cont.LeftThumbY = state.LY;
                    cont.RightThumbX = (byte)state.SASteeringWheelEmulationUnit;
                    cont.RightThumbY = state.RY;
                    break;

                case SASteeringWheelEmulationAxisType.RY:
                    cont.LeftThumbX = state.LX;
                    cont.LeftThumbY = state.LY;
                    cont.RightThumbX = state.RX;
                    cont.RightThumbY = (byte)state.SASteeringWheelEmulationUnit;
                    break;

                case SASteeringWheelEmulationAxisType.L2R2:
                    cont.LeftTrigger = cont.RightTrigger = 0;
                    if (state.SASteeringWheelEmulationUnit >= 0) cont.LeftTrigger = (Byte)state.SASteeringWheelEmulationUnit;
                    else cont.RightTrigger = (Byte)state.SASteeringWheelEmulationUnit;
                    goto case SASteeringWheelEmulationAxisType.None;

                case SASteeringWheelEmulationAxisType.VJoy1X:
                case SASteeringWheelEmulationAxisType.VJoy2X:
                    DS4Windows.VJoyFeeder.vJoyFeeder.FeedAxisValue(state.SASteeringWheelEmulationUnit, ((((uint)steeringWheelMappedAxis) - ((uint)SASteeringWheelEmulationAxisType.VJoy1X)) / 3) + 1, DS4Windows.VJoyFeeder.HID_USAGES.HID_USAGE_X);
                    goto case SASteeringWheelEmulationAxisType.None;

                case SASteeringWheelEmulationAxisType.VJoy1Y:
                case SASteeringWheelEmulationAxisType.VJoy2Y:
                    DS4Windows.VJoyFeeder.vJoyFeeder.FeedAxisValue(state.SASteeringWheelEmulationUnit, ((((uint)steeringWheelMappedAxis) - ((uint)SASteeringWheelEmulationAxisType.VJoy1X)) / 3) + 1, DS4Windows.VJoyFeeder.HID_USAGES.HID_USAGE_Y);
                    goto case SASteeringWheelEmulationAxisType.None;

                case SASteeringWheelEmulationAxisType.VJoy1Z:
                case SASteeringWheelEmulationAxisType.VJoy2Z:
                    DS4Windows.VJoyFeeder.vJoyFeeder.FeedAxisValue(state.SASteeringWheelEmulationUnit, ((((uint)steeringWheelMappedAxis) - ((uint)SASteeringWheelEmulationAxisType.VJoy1X)) / 3) + 1, DS4Windows.VJoyFeeder.HID_USAGES.HID_USAGE_Z);
                    goto case SASteeringWheelEmulationAxisType.None;

                default:
                    // Should never come here but just in case use the NONE case as default handler....
                    goto case SASteeringWheelEmulationAxisType.None;
            }

            cont.SubmitReport();
        }

        public override void ResetState(bool submit = true)
        {
            cont.ResetReport();
            if (submit)
            {
                cont.SubmitReport();
            }
        }
    }
}
