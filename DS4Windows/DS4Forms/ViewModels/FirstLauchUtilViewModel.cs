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
