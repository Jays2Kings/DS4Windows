using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nefarius.ViGEm.Client;
using Nefarius.ViGEm.Client.Targets;
using Nefarius.ViGEm.Client.Targets.Xbox360;

namespace DS4Windows
{
    public class Xbox360OutDevice : OutputDevice
    {
        private const int inputResolution = 127 - (-128);
        private const float reciprocalInputResolution = 1 / (float)inputResolution;
        private const int outputResolution = 32767 - (-32768);
        public const string devType = "X360";

        public Xbox360Controller cont;
        private Xbox360Report report;

        public Xbox360OutDevice(ViGEmClient client)
        {
            cont = new Xbox360Controller(client);
            report = new Xbox360Report();
        }

        public override void ConvertandSendReport(DS4State state, int device)
        {
            Xbox360Buttons tempButtons = 0;

            unchecked
            {
                if (state.Share) tempButtons |= Xbox360Buttons.Back;
                if (state.L3) tempButtons |= Xbox360Buttons.LeftThumb;
                if (state.R3) tempButtons |= Xbox360Buttons.RightThumb;
                if (state.Options) tempButtons |= Xbox360Buttons.Start;

                if (state.DpadUp) tempButtons |= Xbox360Buttons.Up;
                if (state.DpadRight) tempButtons |= Xbox360Buttons.Right;
                if (state.DpadDown) tempButtons |= Xbox360Buttons.Down;
                if (state.DpadLeft) tempButtons |= Xbox360Buttons.Left;

                if (state.L1) tempButtons |= Xbox360Buttons.LeftShoulder;
                if (state.R1) tempButtons |= Xbox360Buttons.RightShoulder;

                if (state.Triangle) tempButtons |= Xbox360Buttons.Y;
                if (state.Circle) tempButtons |= Xbox360Buttons.B;
                if (state.Cross) tempButtons |= Xbox360Buttons.A;
                if (state.Square) tempButtons |= Xbox360Buttons.X;
                if (state.PS) tempButtons |= Xbox360Buttons.Guide;
                report.SetButtonsFull(tempButtons);
            }

            report.LeftTrigger = state.L2;
            report.RightTrigger = state.R2;

            SASteeringWheelEmulationAxisType steeringWheelMappedAxis = Global.GetSASteeringWheelEmulationAxis(device);
            switch (steeringWheelMappedAxis)
            {
                case SASteeringWheelEmulationAxisType.None:
                    report.LeftThumbX = AxisScale(state.LX, false);
                    report.LeftThumbY = AxisScale(state.LY, true);
                    report.RightThumbX = AxisScale(state.RX, false);
                    report.RightThumbY = AxisScale(state.RY, true);
                    break;

                case SASteeringWheelEmulationAxisType.LX:
                    report.LeftThumbX = (short)state.SASteeringWheelEmulationUnit;
                    report.LeftThumbY = AxisScale(state.LY, true);
                    report.RightThumbX = AxisScale(state.RX, false);
                    report.RightThumbY = AxisScale(state.RY, true);
                    break;

                case SASteeringWheelEmulationAxisType.LY:
                    report.LeftThumbX = AxisScale(state.LX, false);
                    report.LeftThumbY = (short)state.SASteeringWheelEmulationUnit;
                    report.RightThumbX = AxisScale(state.RX, false);
                    report.RightThumbY = AxisScale(state.RY, true);
                    break;

                case SASteeringWheelEmulationAxisType.RX:
                    report.LeftThumbX = AxisScale(state.LX, false);
                    report.LeftThumbY = AxisScale(state.LY, true);
                    report.RightThumbX = (short)state.SASteeringWheelEmulationUnit;
                    report.RightThumbY = AxisScale(state.RY, true);
                    break;

                case SASteeringWheelEmulationAxisType.RY:
                    report.LeftThumbX = AxisScale(state.LX, false);
                    report.LeftThumbY = AxisScale(state.LY, true);
                    report.RightThumbX = AxisScale(state.RX, false);
                    report.RightThumbY = (short)state.SASteeringWheelEmulationUnit;
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

        private short AxisScale(Int32 Value, Boolean Flip)
        {
            unchecked
            {
                Value -= 0x80;

                //float temp = (Value - (-128)) / (float)inputResolution;
                float temp = (Value - (-128)) * reciprocalInputResolution;
                if (Flip) temp = (temp - 0.5f) * -1.0f + 0.5f;

                return (short)(temp * outputResolution + (-32768));
            }
        }

        public override void Connect() => cont.Connect();
        public override void Disconnect() => cont.Disconnect();
        public override string GetDeviceType() => devType;
    }
}
