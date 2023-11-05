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

using DS4Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS4WinWPF.DS4Forms.ViewModels
{
    public class FirstLauchUtilViewModel
    {
        private ControlServiceDeviceOptions serviceDeviceOpts;

        public bool EnableDS4
        {
            get => serviceDeviceOpts.DS4DeviceOpts.Enabled;
            set => serviceDeviceOpts.DS4DeviceOpts.Enabled = value;
        }

        public bool EnableDualSense
        {
            get => serviceDeviceOpts.DualSenseOpts.Enabled;
            set => serviceDeviceOpts.DualSenseOpts.Enabled = value;
        }

        public bool EnableSwitchPro
        {
            get => serviceDeviceOpts.SwitchProDeviceOpts.Enabled;
            set => serviceDeviceOpts.SwitchProDeviceOpts.Enabled = value;
        }

        public bool EnableJoyCon
        {
            get => serviceDeviceOpts.JoyConDeviceOpts.Enabled;
            set => serviceDeviceOpts.JoyConDeviceOpts.Enabled = value;
        }

        public bool EnableDS3
        {
            get => serviceDeviceOpts.DS3DeviceOpts.Enabled;
            set => serviceDeviceOpts.DS3DeviceOpts.Enabled = value;
        }

        public FirstLauchUtilViewModel(ControlServiceDeviceOptions serviceDeviceOpts)
        {
            this.serviceDeviceOpts = serviceDeviceOpts;
        }
    }
}
