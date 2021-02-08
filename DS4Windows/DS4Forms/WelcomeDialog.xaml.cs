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
        private const string InstallerDL1_16 =
            "https://github.com/ViGEm/ViGEmBus/releases/download/setup-v1.16.116/ViGEmBus_Setup_1.16.116.exe";
        private const string InstallerDLX64 =
            "https://github.com/ViGEm/ViGEmBus/releases/download/setup-v1.17.333/ViGEmBusSetup_x64.msi";
        private const string InstallerDLX86 =
            "https://github.com/ViGEm/ViGEmBus/releases/download/setup-v1.17.333/ViGEmBusSetup_x64.msi";

        private const string InstFileName1_16 = "ViGEmBus_Setup_1.16.116.exe";
        private const string InstFileNameX64 = "ViGEmBusSetup_x64.msi";
        private const string InstFileNameX86 = "ViGEmBusSetup_x86.msi";
        private string tempInstFileName;

        private string installDL = InstallerDLX64;
        private string installFileName = InstFileNameX64;

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

            if (!DS4Windows.Global.IsWin10OrGreater())
            {
                installDL = InstallerDL1_16;
                installFileName = InstFileName1_16;
            }
            else if (!Environment.Is64BitOperatingSystem)
            {
                installDL = InstallerDLX86;
                installFileName = InstFileNameX86;
            }

            tempInstFileName = DS4Windows.Global.exedirpath + $"\\{installFileName}.tmp";

            // Disable Xbox 360 driver installer button if running on Windows 8 or greater.
            // Driver come pre-installed on a standed OS install
            if (DS4Windows.Global.IsWin8OrGreater())
            {
                step2Btn.IsEnabled = false;
            }
        }

        private void FinishedBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void VigemInstallBtn_Click(object sender, RoutedEventArgs e)
        {
            if (File.Exists(DS4Windows.Global.exedirpath + $"\\{installFileName}"))
            {
                File.Delete(DS4Windows.Global.exedirpath + $"\\{installFileName}");
            }

            if (File.Exists(tempInstFileName))
            {
                File.Delete(tempInstFileName);
            }

            vigemInstallBtn.IsEnabled = false;
            ViGEmDownloadLaunch();

            /*WebClient wb = new WebClient();
            wb.DownloadFileAsync(new Uri(InstallerDL), exepath + $"\\{installFileName}");

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
                vigemInstallBtn.Content = Properties.Resources.Downloading.Replace("*number*%",
                    x.PercentComplete.ToString("P"));
                //Console.WriteLine(x.PercentComplete.ToString("P"));
            });

            string filename = DS4Windows.Global.exedirpath + $"\\{installFileName}";
            bool success = false;
            using (var downloadStream = new FileStream(tempInstFileName, FileMode.CreateNew))
            {
                HttpResponseMessage response = await App.requestClient.GetAsync(installDL,
                    downloadStream, progress);
                success = response.IsSuccessStatusCode;
            }

            if (success)
            {
                File.Move(tempInstFileName, filename);
            }
            success = false; // Reset for later check

            if (File.Exists(DS4Windows.Global.exedirpath + $"\\{installFileName}"))
            {
                //vigemInstallBtn.Content = Properties.Resources.OpeningInstaller;
                monitorProc = Process.Start(DS4Windows.Global.exedirpath + $"\\{installFileName}");
                vigemInstallBtn.Content = Properties.Resources.Installing;
                success = true;
            }

            if (success)
            {
                vigemInstallBtn.IsEnabled = false;

                monitorTimer = new NonFormTimer();
                monitorTimer.Elapsed += ViGEmInstallTimer_Tick;
                monitorTimer.Start();
            }
            else
            {
                vigemInstallBtn.Content = Properties.Resources.InstallFailed;
                vigemInstallBtn.IsEnabled = true;
            }
        }

        private void ViGEmInstallTimer_Tick(object sender, System.Timers.ElapsedEventArgs e)
        {
            ((NonFormTimer)sender).Stop();
            bool finished = false;
            if (monitorProc != null && monitorProc.HasExited)
            {
                if (DS4Windows.Global.IsViGEmBusInstalled())
                {
                    Dispatcher.BeginInvoke((Action)(() =>
                    {
                        vigemInstallBtn.Content = Properties.Resources.InstallComplete;
                        vigemInstallBtn.IsEnabled = true;
                    }));
                }
                else
                {
                    Dispatcher.BeginInvoke((Action)(() =>
                    {
                        vigemInstallBtn.Content = Properties.Resources.InstallFailed;
                        vigemInstallBtn.IsEnabled = true;
                    }), null);
                }

                File.Delete(DS4Windows.Global.exedirpath + $"\\{installFileName}");
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
