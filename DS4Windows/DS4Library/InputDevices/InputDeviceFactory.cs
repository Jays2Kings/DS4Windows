using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS4Windows.InputDevices
{
    public enum InputDeviceType : uint
    {
        DS4,
        SwitchPro,
        JoyConL,
        JoyConR,
        JoyConGrip,
        DualSense,
        DS3
    }

    public abstract class InputDeviceFactory
    {
        public static DS4Device CreateDevice(InputDeviceType tempType,
            HidDevice hidDevice, string disName, VidPidFeatureSet featureSet = VidPidFeatureSet.DefaultDS4)
        {
            DS4Device temp = null;

            switch(tempType)
            {
                case InputDeviceType.DS4:
                    temp = new DS4Device(hidDevice, disName, featureSet);
                    break;
                case InputDeviceType.SwitchPro:
                    temp = new SwitchProDevice(hidDevice, disName, featureSet);
                    break;
                case InputDeviceType.JoyConL:
                case InputDeviceType.JoyConR:
                case InputDeviceType.JoyConGrip:
                    temp = new JoyConDevice(hidDevice, disName, featureSet);
                    break;
                case InputDeviceType.DualSense:
                    temp = new DualSenseDevice(hidDevice, disName, featureSet);
                    break;
                case InputDeviceType.DS3:
                    temp = new DS3Device(hidDevice, disName, featureSet);
                    break;
            }

            return temp;
        }
    }
}
