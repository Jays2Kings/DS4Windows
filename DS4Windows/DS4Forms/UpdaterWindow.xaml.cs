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

namespace DS4WinWPF.DS4Forms
{
    /// <summary>
    /// Interaction logic for UpdaterWindow.xaml
    /// </summary>
    public partial class UpdaterWindow : Window
    {
        private MessageBoxResult result = MessageBoxResult.No;
        public MessageBoxResult Result { get => result; }

        private UpdaterWindowViewModel updaterWinVM;

        public UpdaterWindow(string newversion)
        {
            InitializeComponent();

            Title = Properties.Resources.DS4Update;
            captionTextBlock.Text = Properties.Resources.DownloadVersion.Replace("*number*",
                newversion);
            updaterWinVM = new UpdaterWindowViewModel(newversion);

            DataContext = updaterWinVM;

            SetupEvents();

            updaterWinVM.RetrieveChangelogInfo();
        }

        private void SetupEvents()
        {
            updaterWinVM.ChangelogDocumentChanged += UpdaterWinVM_ChangelogDocumentChanged;
        }

        private void UpdaterWinVM_ChangelogDocumentChanged(object sender, EventArgs e)
        {
            richChangelogTxtBox.Document = updaterWinVM.ChangelogDocument;
        }

        private void YesBtn_Click(object sender, RoutedEventArgs e)
        {
            result = MessageBoxResult.Yes;
            Close();
        }

        private void NoBtn_Click(object sender, RoutedEventArgs e)
        {
            result = MessageBoxResult.No;
            Close();
        }
    }
}
