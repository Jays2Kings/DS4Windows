/*
DS4Windows
Copyright (C) 2023  Travis Nickles

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/

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
