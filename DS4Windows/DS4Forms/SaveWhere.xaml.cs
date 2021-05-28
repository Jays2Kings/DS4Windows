using System;
using System.IO;
using System.Windows;


namespace DS4WinWPF.DS4Forms
{
    /// <summary>
    /// Interaction logic for SaveWhere.xaml
    /// </summary>
    public partial class SaveWhere : Window
    {
        private bool multisaves;
        private bool choiceMade = false;

        public SaveWhere(bool multisavespots)
        {
            InitializeComponent();
            multisaves = multisavespots;
            if (!multisavespots)
            {
                multipleSavesDockP.Visibility = Visibility.Collapsed;
                pickWhereTxt.Text += Properties.Resources.OtherFileLocation;
            }

            if (DS4Windows.Global.AdminNeeded())
            {
                progFolderPanel.IsEnabled = false;
            }
        }

        private void ProgFolderBtn_Click(object sender, RoutedEventArgs e)
        {
            DS4Windows.Global.SaveWhere(DS4Windows.Global.exedirpath);
            if (multisaves && dontDeleteCk.IsChecked == false)
            {
                try
                {
                    if (Directory.Exists(DS4Windows.Global.appDataPpath))
                    {
                        Directory.Delete(DS4Windows.Global.appDataPpath, true);
                    }
                }
                catch { }
            }
            else if (!multisaves)
            {
                DS4Windows.Global.SaveDefault(DS4Windows.Global.exedirpath + "\\Profiles.xml");
            }

            choiceMade = true;
            Close();
        }

        private void AppdataBtn_Click(object sender, RoutedEventArgs e)
        {
            if (multisaves && dontDeleteCk.IsChecked == false)
            {
                try
                {
                    Directory.Delete(DS4Windows.Global.exedirpath + "\\Profiles", true);
                    File.Delete(DS4Windows.Global.exedirpath + "\\Profiles.xml");
                    File.Delete(DS4Windows.Global.exedirpath + "\\Auto Profiles.xml");
                }
                catch (UnauthorizedAccessException)
                {
                    MessageBox.Show("Cannot Delete old settings, please manaully delete", "DS4Windows");
                }
            }
            else if (!multisaves)
            {
                DS4Windows.Global.SaveDefault(Path.Combine(DS4Windows.Global.appDataPpath, "Profiles.xml"));
            }

            DS4Windows.Global.SaveWhere(DS4Windows.Global.appDataPpath);
            choiceMade = true;
            Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!choiceMade)
            {
                e.Cancel = true;
            }
        }
    }
}
