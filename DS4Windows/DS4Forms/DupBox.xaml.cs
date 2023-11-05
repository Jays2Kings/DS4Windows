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

namespace DS4WinWPF.DS4Forms
{
    /// <summary>
    /// Interaction logic for DupBox.xaml
    /// </summary>
    public partial class DupBox : UserControl
    {
        private string oldfilename;
        public string OldFilename { get => oldfilename; set => oldfilename = value; }

        public event EventHandler Cancel;
        public delegate void SaveHandler(DupBox sender, string profilename);
        public event SaveHandler Save;

        public DupBox()
        {
            InitializeComponent();
        }

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            string profile = profileTxt.Text;
            if (!string.IsNullOrWhiteSpace(profile) &&
                profile.IndexOfAny(System.IO.Path.GetInvalidFileNameChars()) == -1)
            {
                System.IO.File.Copy(DS4Windows.Global.appdatapath + "\\Profiles\\" + oldfilename + ".xml",
                DS4Windows.Global.appdatapath + "\\Profiles\\" + profile + ".xml", true);
                Save?.Invoke(this, profile);
            }
            else
            {
                MessageBox.Show(Properties.Resources.ValidName, Properties.Resources.NotValid,
                    MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            Cancel?.Invoke(this, EventArgs.Empty);
        }
    }
}
