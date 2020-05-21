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
using DS4Windows;

namespace DS4WinWPF.DS4Forms
{
    /// <summary>
    /// Interaction logic for PluginOutDevWindow.xaml
    /// </summary>
    public partial class PluginOutDevWindow : Window
    {
        private MessageBoxResult result = MessageBoxResult.Cancel;
        public MessageBoxResult Result { get => result; }

        private DS4Windows.OutContType contType = DS4Windows.OutContType.None;
        public OutContType ContType { get => contType; }

        private DS4Control.OutSlotDevice.ReserveStatus reserveType;

        public PluginOutDevWindow()
        {
            InitializeComponent();
        }

        private void AcceptBtn_Click(object sender, RoutedEventArgs e)
        {
            switch (devTypeCombo.SelectedIndex)
            {
                case 0:
                    contType = OutContType.X360;
                    break;
                case 1:
                    contType = OutContType.DS4;
                    break;
                default:
                    break;
            }

            switch(reserveTypeCombo.SelectedIndex)
            {
                case 0:
                    reserveType = DS4Control.OutSlotDevice.ReserveStatus.Dynamic;
                    break;
                case 1:
                    reserveType = DS4Control.OutSlotDevice.ReserveStatus.Permanent;
                    break;
                default:
                    break;
            }

            if (contType != OutContType.None)
            {
                result = MessageBoxResult.OK;
            }
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            contType = OutContType.None;
            result = MessageBoxResult.Cancel;
        }
    }
}
