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
using System.Windows.Navigation;
using System.Windows.Shapes;
using DS4WinWPF.DS4Forms.ViewModels;

namespace DS4WinWPF.DS4Forms
{
    /// <summary>
    /// Interaction logic for LanguagePackControl.xaml
    /// </summary>
    public partial class LanguagePackControl : UserControl
    {
        private LanguagePackViewModel langPackVM;

        public LanguagePackControl()
        {
            InitializeComponent();

            langPackVM = new LanguagePackViewModel();
            this.DataContext = null;
            langPackVM.ScanFinished += LangPackVM_ScanFinished;
            langPackVM.SelectedIndexChanged += CheckForCultureChange;

            langPackVM.ScanForLangPacks();
        }

        private void CheckForCultureChange(object sender, EventArgs e)
        {
            if (langPackVM.ChangeLanguagePack())
            {
                MessageBox.Show(Properties.Resources.LanguagePackApplyRestartRequired,
                    "DS4Windows", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void LangPackVM_ScanFinished(object sender, EventArgs e)
        {
            Dispatcher.BeginInvoke((Action)(() =>
            {
                cbCulture.IsEnabled = true;
                this.DataContext = langPackVM;
            }));
        }
    }
}
