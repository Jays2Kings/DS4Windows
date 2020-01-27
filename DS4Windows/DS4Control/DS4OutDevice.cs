using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nefarius.ViGEm.Client;
using Nefarius.ViGEm.Client.Targets;
using Nefarius.ViGEm.Client.Targets.DualShock4;

namespace DS4Windows
{
    class DS4OutDevice : OutputDevice
    {
        public const string devtype = "DS4";

        public DualShock4Controller cont;
        private DualShock4Report report;

        public DS4OutDevice(ViGEmClient client)
        {
            cont = new DualShock4Controller(client);
            report = new DualShock4Report();
        }

        public override void ConvertandSendReport(DS4State state, int device)
        {
            DualShock4Buttons tempButtons = 0;
            DualShock4DPadValues tempDPad = DualShock4DPadValues.None;
            DualShock4SpecialButtons tempSpecial = 0;

            unchecked
            {
                if (state.Share) tempButtons |= DualShock4Buttons.Share;
                if (state.L3) tempButtons |= DualShock4Buttons.ThumbLeft;
                if (state.R3) tempButtons |= DualShock4Buttons.ThumbRight;
                if (state.Options) tempButtons |= DualShock4Buttons.Options;

                if (state.DpadUp && state.DpadRight) tempDPad = DualShock4DPadValues.Northeast;
                else if (state.DpadUp && state.DpadLeft) tempDPad = DualShock4DPadValues.Northwest;
                else if (state.DpadUp) tempDPad = DualShock4DPadValues.North;
                else if (state.DpadRight && state.DpadDown) tempDPad = DualShock4DPadValues.Southeast;
                else if (state.DpadRight) tempDPad = DualShock4DPadValues.East;
                else if (state.DpadDown && state.DpadLeft) tempDPad = DualShock4DPadValues.Southwest;
                else if (state.DpadDown) tempDPad = DualShock4DPadValues.South;
                else if (state.DpadLeft) tempDPad = DualShock4DPadValues.West;

                /*if (state.DpadUp) tempDPad = (state.DpadRight) ? DualShock4DPadValues.Northeast : DualShock4DPadValues.North;
                if (state.DpadRight) tempDPad = (state.DpadDown) ? DualShock4DPadValues.Southeast : DualShock4DPadValues.East;
                if (state.DpadDown) tempDPad = (state.DpadLeft) ? DualShock4DPadValues.Southwest : DualShock4DPadValues.South;
                if (state.DpadLeft) tempDPad = (state.DpadUp) ? DualShock4DPadValues.Northwest : DualShock4DPadValues.West;
                */

                if (state.L1) tempButtons |= DualShock4Buttons.ShoulderLeft;
                if (state.R1) tempButtons |= DualShock4Buttons.ShoulderRight;
                //if (state.L2Btn) tempButtons |= DualShock4Buttons.TriggerLeft;
                //if (state.R2Btn) tempButtons |= DualShock4Buttons.TriggerRight;
                if (state.L2 > 0) tempButtons |= DualShock4Buttons.TriggerLeft;
                if (state.R2 > 0) tempButtons |= DualShock4Buttons.TriggerRight;

                if (state.Triangle) tempButtons |= DualShock4Buttons.Triangle;
                if (state.Circle) tempButtons |= DualShock4Buttons.Circle;
                if (state.Cross) tempButtons |= DualShock4Buttons.Cross;
                if (state.Square) tempButtons |= DualShock4Buttons.Square;
                if (state.PS) tempSpecial |= DualShock4SpecialButtons.Ps;
                if (state.TouchButton) tempSpecial |= DualShock4SpecialButtons.Touchpad;
                //report.SetButtonsFull(tempButtons);
                report.Buttons = (ushort)tempButtons;
                report.SetDPad(tempDPad);
                report.SpecialButtons = (byte)tempSpecial;
            }


            report.LeftTrigger = state.L2;
            report.RightTrigger = state.R2;

            SASteeringWheelEmulationAxisType steeringWheelMappedAxis = Global.GetSASteeringWheelEmulationAxis(device);
            switch (steeringWheelMappedAxis)
            {
                case SASteeringWheelEmulationAxisType.None:
                    report.LeftThumbX = state.LX;
                    report.LeftThumbY = state.LY;
                    report.RightThumbX = state.RX;
                    report.RightThumbY = state.RY;
                    break;

                case SASteeringWheelEmulationAxisType.LX:
                    report.LeftThumbX = (byte)state.SASteeringWheelEmulationUnit;
                    report.LeftThumbY = state.LY;
                    report.RightThumbX = state.RX;
                    report.RightThumbY = state.RY;
                    break;

                case SASteeringWheelEmulationAxisType.LY:
                    report.LeftThumbX = state.LX;
                    report.LeftThumbY = (byte)state.SASteeringWheelEmulationUnit;
                    report.RightThumbX = state.RX;
                    report.RightThumbY = state.RY;
                    break;

                case SASteeringWheelEmulationAxisType.RX:
                    report.LeftThumbX = state.LX;
                    report.LeftThumbY = state.LY;
                    report.RightThumbX = (byte)state.SASteeringWheelEmulationUnit;
                    report.RightThumbY = state.RY;
                    break;

                case SASteeringWheelEmulationAxisType.RY:
                    report.LeftThumbX = state.LX;
                    report.LeftThumbY = state.LY;
                    report.RightThumbX = state.RX;
                    report.RightThumbY = (byte)state.SASteeringWheelEmulationUnit;
                    break;

                case SASteeringWheelEmulationAxisType.L2R2:
                    report.LeftTrigger = report.RightTrigger = 0;
                    if (state.SASteeringWheelEmulationUnit >= 0) report.LeftTrigger = (Byte)state.SASteeringWheelEmulationUnit;
                    else report.RightTrigger = (Byte)state.SASteeringWheelEmulationUnit;
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

            cont.SendReport(report);
        }

        public override void Connect() => cont.Connect();
        public override void Disconnect() => cont.Disconnect();
        public override string GetDeviceType() => devtype;
    }
}
