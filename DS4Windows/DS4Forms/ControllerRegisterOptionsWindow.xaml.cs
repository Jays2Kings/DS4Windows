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

        public ControllerRegisterOptionsWindow(ControlServiceDeviceOptions deviceOptions, ControlService service)
        {
            InitializeComponent();

            deviceOptsVM = new ControllerRegDeviceOptsViewModel(deviceOptions, service);

            devOptionsDockPanel.DataContext = deviceOptsVM;
            deviceOptsVM.ControllerSelectedIndexChanged += ChangeActiveDeviceTab;
        }

        private void ChangeActiveDeviceTab(object sender, EventArgs e)
        {
            TabItem currentTab = deviceSettingsTabControl.SelectedItem as TabItem;
            if (currentTab != null)
            {
                currentTab.DataContext = null;
            }

            int tabIdx = deviceOptsVM.FindTabOptionsIndex();
            if (tabIdx >= 0)
            {
                TabItem pendingTab = deviceSettingsTabControl.Items[tabIdx] as TabItem;
                deviceOptsVM.FindFittingDataContext();
                pendingTab.DataContext = deviceOptsVM.DataContextObject;
            }

            deviceOptsVM.CurrentTabSelectedIndex = tabIdx;
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            deviceOptsVM.SaveControllerConfigs();
        }
    }
}
