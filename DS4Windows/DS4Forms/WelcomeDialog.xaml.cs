using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Windows;
using HttpProgress;
using NonFormTimer = System.Timers.Timer;

namespace DS4WinWPF.DS4Forms
{
    /// <summary>
    /// Interaction logic for WelcomeDialog.xaml
    /// </summary>
    public partial class WelcomeDialog : Window
    {
        private const string InstallerDL =
            "https://github.com/ViGEm/ViGEmBus/releases/download/v1.16.112/ViGEmBus_Setup_1.16.115.exe";
        private const string InstFileName = "ViGEmBus_Setup_1.16.115.exe";

        Process monitorProc;
        NonFormTimer monitorTimer;

        public WelcomeDialog(bool loadConfig = false)
        {
            if (loadConfig)
            {
                DS4Windows.Global.FindConfigLocation();
                DS4Windows.Global.Load();
                //DS4Windows.Global.SetCulture(DS4Windows.Global.UseLang);
            }

            InitializeComponent();
        }

        private void FinishedBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void VigemInstallBtn_Click(object sender, RoutedEventArgs e)
        {
            if (File.Exists(DS4Windows.Global.exedirpath + $"\\{InstFileName}"))
            {
                File.Delete(DS4Windows.Global.exedirpath + $"\\{InstFileName}");
            }

            ViGEmDownloadLaunch();

            /*WebClient wb = new WebClient();
            wb.DownloadFileAsync(new Uri(InstallerDL), exepath + $"\\{InstFileName}");

            wb.DownloadProgressChanged += wb_DownloadProgressChanged;
            wb.DownloadFileCompleted += wb_DownloadFileCompleted;
            */
        }

        private async void ViGEmDownloadLaunch()
        {
            Progress<ICopyProgress> progress = new Progress<ICopyProgress>(x => // Please see "Notes on IProgress<T>"
            {
                // This is your progress event!
                // It will fire on every buffer fill so don't do anything expensive.
                // Writing to the console IS expensive, so don't do the following in practice...
                vigemInstallBtn.Content = Properties.Resources.Downloading.Replace("*number*",
                    x.PercentComplete.ToString("P"));
                //Console.WriteLine(x.PercentComplete.ToString("P"));
            });

            string filename = DS4Windows.Global.exedirpath + $"\\{InstFileName}";
            using (var downloadStream = new FileStream(filename, FileMode.CreateNew))
            {
                HttpResponseMessage response = await App.requestClient.GetAsync(InstallerDL, downloadStream, progress);
            }

            if (File.Exists(DS4Windows.Global.exedirpath + $"\\{InstFileName}"))
            {
                //vigemInstallBtn.Content = Properties.Resources.OpeningInstaller;
                monitorProc = Process.Start(DS4Windows.Global.exedirpath + $"\\{InstFileName}");
                vigemInstallBtn.Content = Properties.Resources.Installing;
            }

            monitorTimer = new NonFormTimer();
            monitorTimer.Elapsed += ViGEmInstallTimer_Tick;
            monitorTimer.Start();
        }

        private void ViGEmInstallTimer_Tick(object sender, System.Timers.ElapsedEventArgs e)
        {
            ((NonFormTimer)sender).Stop();
            bool finished = false;
            if (monitorProc != null && monitorProc.HasExited)
            {
                if (DS4Windows.Global.IsViGEmBusInstalled())
                {
                    Dispatcher.BeginInvoke((Action)(() => { vigemInstallBtn.Content = Properties.Resources.InstallComplete; }));
                }
                else
                {
                    Dispatcher.BeginInvoke((Action)(() => { vigemInstallBtn.Content = Properties.Resources.InstallFailed; }), null);
                }

                File.Delete(DS4Windows.Global.exedirpath + $"\\{InstFileName}");
                ((NonFormTimer)sender).Stop();
                finished = true;
            }

            if (!finished)
            {
                ((NonFormTimer)sender).Start();
            }
        }

        private void Step2Btn_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("http://www.microsoft.com/accessories/en-gb/d/xbox-360-controller-for-windows");
        }

        private void BluetoothSetLink_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("control", "bthprops.cpl");
        }
    }
}
