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

        private List<DSHapticsChoiceEnum> dsHapticOptions = new List<DSHapticsChoiceEnum>()
        {
            new DSHapticsChoiceEnum("Low", DS4Windows.InputDevices.DualSenseDevice.HapticIntensity.Low),
            new DSHapticsChoiceEnum("Medium", DS4Windows.InputDevices.DualSenseDevice.HapticIntensity.Medium),
            new DSHapticsChoiceEnum("High", DS4Windows.InputDevices.DualSenseDevice.HapticIntensity.High)
        };
        public List<DSHapticsChoiceEnum> DSHapticOptions { get => dsHapticOptions; }

        public ControllerRegDeviceOptsViewModel(ControlServiceDeviceOptions serviceDeviceOpts)
        {
            this.serviceDeviceOpts = serviceDeviceOpts;
        }
    }

    public class DSHapticsChoiceEnum
    {
        private string displayName = string.Empty;
        public string DisplayName { get => displayName; }

        private DS4Windows.InputDevices.DualSenseDevice.HapticIntensity choiceValue;
        public DS4Windows.InputDevices.DualSenseDevice.HapticIntensity ChoiceValue
        {
            get => choiceValue;
            set => choiceValue = value;
        }

        public DSHapticsChoiceEnum(string name,
            DS4Windows.InputDevices.DualSenseDevice.HapticIntensity intensity)
        {
            displayName = name;
            choiceValue = intensity;
        }

        public override string ToString()
        {
            return displayName;
        }
    }
}
