using DS4WinWPF.DS4Forms.ViewModels;
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

namespace DS4WinWPF.DS4Forms
{
    /// <summary>
    /// Interaction logic for ChangelogWindow.xaml
    /// </summary>
    public partial class ChangelogWindow : Window
    {
        private ChangelogViewModel changelogVM;

        public ChangelogWindow()
        {
            InitializeComponent();

            changelogVM = new ChangelogViewModel();

            DataContext = changelogVM;

            SetupEvents();

            changelogVM.RetrieveChangelogInfo();

        }

        private void SetupEvents()
        {
            changelogVM.ChangelogDocumentChanged += ChangelogVM_ChangelogDocumentChanged;
        }

        private void ChangelogVM_ChangelogDocumentChanged(object sender, EventArgs e)
        {
            richChangelogTxtBox.Document = changelogVM.ChangelogDocument;
        }

        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
