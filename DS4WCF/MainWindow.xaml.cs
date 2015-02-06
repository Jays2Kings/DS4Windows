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

namespace DS4WCF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //Window does not own control
        private DS4Control.Control _control;
        List<ControllerInputGrid> _controllerGrids = new List<ControllerInputGrid>();

        public MainWindow( DS4Control.Control control )
        {
            _control = control;

            InitializeComponent();
            GetAllControllerPanels();
            GetAllControllers();
        }

        private void GetAllControllerPanels()
        {
            _controllerGrids.Add(FindName("ControllerPanel0") as ControllerInputGrid);
            _controllerGrids.Add(FindName("ControllerPanel1") as ControllerInputGrid);
            _controllerGrids.Add(FindName("ControllerPanel2") as ControllerInputGrid);
            _controllerGrids.Add(FindName("ControllerPanel3") as ControllerInputGrid);
        }

        private void GetAllControllers()
        {
            DS4Library.DS4Device[] devices = this._control.DS4Controllers;
            for (int deviceIndex = 0; deviceIndex < devices.Count(); deviceIndex++)
            {
                if (devices[deviceIndex] != null)
                {
                    _controllerGrids[deviceIndex].InitializeWithDS4Device(devices[deviceIndex]);
                }
            }
        }

    }
}
