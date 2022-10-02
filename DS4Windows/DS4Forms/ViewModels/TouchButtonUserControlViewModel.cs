using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DS4WinWPF.DS4Forms.ViewModels.Util;
using DS4Windows;
using static DS4Windows.Mouse;

namespace DS4WinWPF.DS4Forms.ViewModels
{
    public class TouchButtonUserControlViewModel
    {
        private int deviceIndex;
        public int DeviceIndex
        {
            get => deviceIndex;
            set => deviceIndex = value;
        }

        private EnumChoiceSelection<TouchButtonActivationMode>[] touchButtonModes = new EnumChoiceSelection<TouchButtonActivationMode>[]
        {
            new EnumChoiceSelection<TouchButtonActivationMode>("Click", TouchButtonActivationMode.Click),
            new EnumChoiceSelection<TouchButtonActivationMode>("Touch", TouchButtonActivationMode.Touch),
            new EnumChoiceSelection<TouchButtonActivationMode>("Release", TouchButtonActivationMode.Release),
        };
        public EnumChoiceSelection<TouchButtonActivationMode>[] TouchButtonModes => touchButtonModes;

        public TouchButtonActivationMode CurrentMode
        {
            get => Global.TouchpadButtonMode[deviceIndex];
            set => Global.TouchpadButtonMode[deviceIndex] = value;
        }

        public TouchButtonUserControlViewModel(int deviceIndex)
        {
            this.deviceIndex = deviceIndex;
        }
    }
}
