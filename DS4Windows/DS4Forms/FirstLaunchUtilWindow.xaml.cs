using DS4Windows;
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
    /// Interaction logic for FirstLaunchHelper.xaml
    /// </summary>
    public partial class FirstLaunchUtilWindow : Window
    {
        private FirstLauchUtilViewModel firstLaunchUtilVM;

        public FirstLaunchUtilWindow(ControlServiceDeviceOptions serviceDeviceOpts)
        {
            InitializeComponent();

            firstLaunchUtilVM = new FirstLauchUtilViewModel(serviceDeviceOpts);
            pagesTabControl.DataContext = firstLaunchUtilVM;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void DsHidMiniLink_Click(object sender, RoutedEventArgs e)
        {
            Util.StartProcessHelper("https://vigem.org/projects/DsHidMini/");
        }
    }
}
