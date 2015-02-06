using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace DS4WCF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        DS4Control.Control _control;

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            _control = new DS4Control.Control();
            _control.Start();

            MainWindow window = new DS4WCF.MainWindow(_control);
            window.Show();
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            _control.Stop();
        }


    }
}
