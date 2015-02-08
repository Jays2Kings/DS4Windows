using DS4Library;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace DS4WCF
{
    /// <summary>
    /// Interaction logic for ControllerInputGrid.xaml
    /// </summary>
    public partial class ControllerInputGrid : UserControl, INotifyPropertyChanged
    {
        private void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

        string _batteryLevel;
        public string BatteryLevel 
        { 
            get
            {
                return _batteryLevel;
            }
            set
            {
           
                _batteryLevel = value;
                OnPropertyChanged("BatteryLevel");
            }
        }

        Brush _barColor;
        public Brush BarColor 
        {
            get
            {
                return _barColor;
            }
            set
            {
                _barColor = value;
                OnPropertyChanged("barColor");
            }
        }

        public ControllerInputGrid()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        public void InitializeWithDS4Device(DS4Device device)
        {
            this.BatteryLevel = device.Battery.ToString() + "%";
            this.BarColor = new SolidColorBrush(Color.FromRgb(device.LightBarColor.red, device.LightBarColor.green, device.LightBarColor.blue));
        }


        public event PropertyChangedEventHandler PropertyChanged;
    }
}
