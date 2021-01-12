using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DS4Windows;

namespace DS4WinWPF.DS4Forms.ViewModels
{
    public class ControllerRegDeviceOptsViewModel
    {
        private ControlServiceDeviceOptions serviceDeviceOpts;

        public bool EnableDS4 { get => serviceDeviceOpts.DS4DeviceOpts.Enabled; }

        public bool EnableDualSense { get => serviceDeviceOpts.DualSenseOpts.Enabled; }

        public bool EnableSwitchPro { get => serviceDeviceOpts.SwitchProDeviceOpts.Enabled; }

        public bool EnableJoyCon { get => serviceDeviceOpts.JoyConDeviceOpts.Enabled; }

        public DS4DeviceOptions DS4DeviceOpts { get => serviceDeviceOpts.DS4DeviceOpts; }
        public DualSenseDeviceOptions DSDeviceOpts { get => serviceDeviceOpts.DualSenseOpts; }
        public SwitchProDeviceOptions SwitchProDeviceOpts { get => serviceDeviceOpts.SwitchProDeviceOpts; }
        public JoyConDeviceOptions JoyConDeviceOpts { get => serviceDeviceOpts.JoyConDeviceOpts; }

        public ControllerRegDeviceOptsViewModel(ControlServiceDeviceOptions serviceDeviceOpts)
        {
            this.serviceDeviceOpts = serviceDeviceOpts;
        }
    }
}
