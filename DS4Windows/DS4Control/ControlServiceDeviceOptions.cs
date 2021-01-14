using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS4Windows
{
    public class ControlServiceDeviceOptions
    {
        private DS4DeviceOptions dS4DeviceOpts = new DS4DeviceOptions();
        public DS4DeviceOptions DS4DeviceOpts { get => dS4DeviceOpts; }

        private DualSenseDeviceOptions dualSenseOpts =
            new DualSenseDeviceOptions();
        public DualSenseDeviceOptions DualSenseOpts { get => dualSenseOpts; }

        private SwitchProDeviceOptions switchProDeviceOpts = new SwitchProDeviceOptions();
        public SwitchProDeviceOptions SwitchProDeviceOpts { get => switchProDeviceOpts; }

        private JoyConDeviceOptions joyConDeviceOpts = new JoyConDeviceOptions();
        public JoyConDeviceOptions JoyConDeviceOpts { get => joyConDeviceOpts; }
    }

    public class DS4DeviceOptions
    {
        private bool enabled = true;
        public bool Enabled { get => enabled; set => enabled = value; }
    }

    public class DualSenseDeviceOptions
    {
        private bool enabled = true;
        public bool Enabled { get => enabled; set => enabled = value; }

        private bool enableRumble = true;
        public bool EnableRumble { get => enableRumble; set => enableRumble = value; }
    }

    public class SwitchProDeviceOptions
    {
        private bool enabled = true;
        public bool Enabled { get => enabled; set => enabled = value; }

        private bool enableHomeLED = true;
        public bool EnableHomeLED { get => enableHomeLED; set => enableHomeLED = value; }
    }

    public class JoyConDeviceOptions
    {
        private bool enabled = true;
        public bool Enabled { get => enabled; set => enabled = value; }

        private bool enableHomeLED = true;
        public bool EnableHomeLED { get => enableHomeLED; set => enableHomeLED = value; }
    }
}
