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

            devOptionsDockPanel.DataContext = null;
        }
    }
}
