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
    /// Interaction logic for TouchButtonUserControl.xaml
    /// </summary>
    public partial class TouchButtonUserControl : UserControl
    {
        private TouchButtonUserControlViewModel touchButtonVM;

        public TouchButtonUserControl(int deviceIndex)
        {
            InitializeComponent();

            touchButtonVM = new TouchButtonUserControlViewModel(deviceIndex);
            DataContext = touchButtonVM;
        }
    }
}
