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

        public void UnregisterDataContext()
        {
            mainGrid.DataContext = null;
        }
    }
}
