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
    /// Interaction logic for RecordBoxWindow.xaml
    /// </summary>
    public partial class RecordBoxWindow : Window
    {
        public event EventHandler Saved;

        public RecordBoxWindow(int deviceNum, DS4Windows.DS4ControlSettings settings)
        {
            InitializeComponent();

            RecordBox box = new RecordBox(deviceNum, settings, false);
            mainPanel.Children.Add(box);

            box.Save += RecordBox_Save;
            box.Cancel += Box_Cancel;
        }

        private void Box_Cancel(object sender, EventArgs e)
        {
            Close();
        }

        private void RecordBox_Save(object sender, EventArgs e)
        {
            Saved?.Invoke(this, EventArgs.Empty);
            Close();
        }
    }
}
