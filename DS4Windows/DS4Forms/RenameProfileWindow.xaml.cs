using System.Windows;
using DS4WinWPF.DS4Forms.ViewModels;

namespace DS4WinWPF.DS4Forms
{
    /// <summary>
    /// Interaction logic for RenameProfileWindow.xaml
    /// </summary>
    public partial class RenameProfileWindow : Window
    {
        private RenameProfileViewModel renameProfileVM;
        public RenameProfileViewModel RenameProfileVM
        {
            get => renameProfileVM;
        }

        public RenameProfileWindow()
        {
            InitializeComponent();

            renameProfileVM = new RenameProfileViewModel();
            mainDockPanel.DataContext = renameProfileVM;
        }

        /// <summary>
        /// Method used to carry over current name of profile and copy
        /// it to ViewModel
        /// </summary>
        /// <param name="profileName">name of current profile</param>
        public void ChangeProfileName(string profileName)
        {
            renameProfileVM.ProfileName = profileName;
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void ConfirmBtn_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(renameProfileVM.ProfileName))
            {
                bool validChars = renameProfileVM.ProfileName.IndexOfAny(System.IO.Path.GetInvalidFileNameChars()) == -1;
                if (validChars)
                {
                    bool existingFile = renameProfileVM.ProfileFileExists();
                    if (existingFile)
                    {
                        e.Handled = true;
                        MessageBox.Show("Profile with name already exists. Please try again.");
                    }
                    else
                    {
                        DialogResult = true;
                        Close();
                    }
                }
                else
                {
                    e.Handled = true;
                    MessageBox.Show("Invalid characters used in filename. Please change text.");
                }
            }
        }
    }
}
