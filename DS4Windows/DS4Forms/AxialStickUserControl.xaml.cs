using System;
using System.Windows.Controls;
using DS4Windows;
using DS4WinWPF.DS4Forms.ViewModels;

namespace DS4WinWPF.DS4Forms
{
    /// <summary>
    /// Interaction logic for AxialStickUserControl.xaml
    /// </summary>
    public partial class AxialStickUserControl : UserControl
    {
        private AxialStickControlViewModel axialVM;
        public AxialStickControlViewModel AxialVM
        {
            get => axialVM;
        }

        public AxialStickUserControl()
        {
            InitializeComponent();
        }

        public void UseDevice(StickDeadZoneInfo stickDeadInfo)
        {
            axialVM = new AxialStickControlViewModel(stickDeadInfo);
            mainGrid.DataContext = axialVM;
        }
    }
}
