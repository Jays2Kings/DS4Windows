using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using DS4WinWPF.DS4Forms.ViewModels;
using DS4Windows;

namespace DS4WinWPF.DS4Forms
{
    /// <summary>
    /// Interaction logic for ControllerRegisterOptions.xaml
    /// </summary>
    public partial class ControllerRegisterOptionsWindow : Window
    {
        private ControllerRegDeviceOptsViewModel deviceOptsVM;

        public ControllerRegisterOptionsWindow(ControlServiceDeviceOptions deviceOptions)
        {
            InitializeComponent();

            deviceOptsVM = new ControllerRegDeviceOptsViewModel(deviceOptions);

            devOptionsDockPanel.DataContext = deviceOptsVM;
        }
    }
}
