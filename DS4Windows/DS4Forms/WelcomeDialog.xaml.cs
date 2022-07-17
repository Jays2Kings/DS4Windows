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
            "https://github.com/ViGEm/ViGEmBus/releases/download/setup-v1.17.333/ViGEmBusSetup_x86.msi";

        private const string InstallerHidHideX64 = "https://github.com/ViGEm/HidHide/releases/download/v1.1.50.0/HidHideMSI.msi";
        private const string InstallerFakerInputX64 = "https://github.com/Ryochan7/FakerInput/releases/download/v0.1.0/FakerInput_0.1.0_x64.msi";
        private const string InstallerFakerInputX86 = "https://github.com/Ryochan7/FakerInput/releases/download/v0.1.0/FakerInput_0.1.0_x86.msi";

        private const string InstFileName1_16 = "ViGEmBus_Setup_1.16.116.exe";
        private const string InstFileNameX64 = "ViGEmBusSetup_x64.msi";
        private const string InstFileNameX86 = "ViGEmBusSetup_x86.msi";
        private string tempInstFileName;

        private const string InstHidHideFileNameX64 = "HidHideMSI.msi";

        private string installFakerInputDL = "";
        private string instFakerInputFileName = "";

        // Default to latest known ViGEmBus installer
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

            // Run checks for compatible version of ViGEmBus
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

            // Run checks for FakerInput driver
            if (DS4Windows.Global.IsWin8OrGreater())
            {
                if (Environment.Is64BitOperatingSystem)
                {
                    installFakerInputDL = InstallerFakerInputX64;
                    instFakerInputFileName = new FileInfo(InstallerFakerInputX64).Name;
                }
                else if (!Environment.Is64BitOperatingSystem)
                {
                    installDL = InstallerDLX86;
                    installFileName = InstFileNameX86;

                    installFakerInputDL = InstallerFakerInputX86;
                    instFakerInputFileName = new FileInfo(InstallerFakerInputX86).Name;
                }
            }
            else
            {
                step5FakerInputPanel.IsEnabled = false;
            }

            tempInstFileName = Path.Combine(Path.GetTempPath(), $"{installFileName}.tmp");

            // HidHide only works on Windows 10 x64
            if (!IsHidHideControlCompatible())
            {
                step4HidHidePanel.IsEnabled = false;
            }
        }

        private bool IsHidHideControlCompatible()
        {
            // HidHide only works on Windows 10 x64
            return DS4Windows.Global.IsWin10OrGreater() &&
                Environment.Is64BitOperatingSystem;
        }

        private bool IsFakerInputControlCompatible()
        {
            // FakerInput works on Windows 8.1 and later. Going to attempt
            // to support x64 and x86 arch
            return DS4Windows.Global.IsWin8OrGreater();
        }

        private void FinishedBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void VigemInstallBtn_Click(object sender, RoutedEventArgs e)
        {
            string tempFilePath = Path.Combine(Path.GetTempPath(), $"{installFileName}");
            if (File.Exists(tempFilePath))
            {
                File.Delete(tempFilePath);
            }

            if (File.Exists(tempInstFileName))
            {
                File.Delete(tempInstFileName);
            }

            EnableControls(false);
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

            string finalFilename = Path.Combine(Path.GetTempPath(), $"{installFileName}");
            bool success = false;
            using (var downloadStream = new FileStream(tempInstFileName, FileMode.CreateNew))
            {
                HttpResponseMessage response = await App.requestClient.GetAsync(installDL,
                    downloadStream, progress);
                success = response.IsSuccessStatusCode;
            }

            if (success)
            {
                File.Move(tempInstFileName, finalFilename);
            }
            success = false; // Reset for later check

            if (File.Exists(finalFilename))
            {
                //vigemInstallBtn.Content = Properties.Resources.OpeningInstaller;
                ProcessStartInfo startInfo = new ProcessStartInfo(finalFilename);
                startInfo.UseShellExecute = true; // Needed to run program as admin
                monitorProc = Process.Start(startInfo);
                vigemInstallBtn.Content = Properties.Resources.Installing;
                success = true;
            }

            if (success)
            {
                EnableControls(false);

                monitorTimer = new NonFormTimer();
                monitorTimer.Elapsed += ViGEmInstallTimer_Tick;
                monitorTimer.Start();
            }
            else
            {
                vigemInstallBtn.Content = Properties.Resources.InstallFailed;
                EnableControls(true);
            }
        }

        private void ViGEmInstallTimer_Tick(object sender, System.Timers.ElapsedEventArgs e)
        {
            ((NonFormTimer)sender).Stop();
            bool finished = false;
            if (monitorProc != null && monitorProc.HasExited)
            {
                // Retrieve info about installed ViGEmBus device if found
                DS4Windows.Global.RefreshViGEmBusInfo();
                if (DS4Windows.Global.IsViGEmBusInstalled())
                {
                    Dispatcher.BeginInvoke((Action)(() =>
                    {
                        vigemInstallBtn.Content = Properties.Resources.InstallComplete;
                        EnableControls(true);
                    }));
                }
                else
                {
                    Dispatcher.BeginInvoke((Action)(() =>
                    {
                        vigemInstallBtn.Content = Properties.Resources.InstallFailed;
                        EnableControls(true);
                    }), null);
                }

                string currentFilePath = Path.Combine(Path.GetTempPath(), $"{installFileName}");
                File.Delete(currentFilePath);
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

        private void HidHideInstall_Click(object sender, RoutedEventArgs e)
        {
            string tempFilePath = Path.Combine(Path.GetTempPath(), $"{InstHidHideFileNameX64}");
            if (File.Exists(tempFilePath))
            {
                File.Delete(tempFilePath);
            }

            string tempInstHidFileName = Path.Combine(Path.GetTempPath(), $"{InstHidHideFileNameX64}.tmp");
            if (File.Exists(tempInstHidFileName))
            {
                File.Delete(tempInstHidFileName);
            }

            EnableControls(false);
            HidHideDownloadLaunch();
        }

        private async void HidHideDownloadLaunch()
        {
            Progress<ICopyProgress> progress = new Progress<ICopyProgress>(x => // Please see "Notes on IProgress<T>"
            {
                // This is your progress event!
                // It will fire on every buffer fill so don't do anything expensive.
                // Writing to the console IS expensive, so don't do the following in practice...
                hidHideInstallBtn.Content = Properties.Resources.Downloading.Replace("*number*%",
                    x.PercentComplete.ToString("P"));
                //Console.WriteLine(x.PercentComplete.ToString("P"));
            });

            string tempInstHidFileName = Path.Combine(Path.GetTempPath(), $"{InstHidHideFileNameX64}.tmp");
            string finalFilename = Path.Combine(Path.GetTempPath(), $"{InstHidHideFileNameX64}");
            bool success = false;
            using (var downloadStream = new FileStream(tempInstHidFileName, FileMode.CreateNew))
            {
                HttpResponseMessage response = await App.requestClient.GetAsync(InstallerHidHideX64,
                    downloadStream, progress);
                success = response.IsSuccessStatusCode;
            }

            if (success)
            {
                File.Move(tempInstHidFileName, finalFilename);
            }
            success = false; // Reset for later check

            if (File.Exists(finalFilename))
            {
                //vigemInstallBtn.Content = Properties.Resources.OpeningInstaller;
                ProcessStartInfo startInfo = new ProcessStartInfo(finalFilename);
                startInfo.UseShellExecute = true; // Needed to run program as admin
                monitorProc = Process.Start(startInfo);
                hidHideInstallBtn.Content = Properties.Resources.Installing;
                success = true;
            }

            if (success)
            {
                monitorTimer = new NonFormTimer();
                monitorTimer.Elapsed += HidHideInstallTimer_Elapsed;
                monitorTimer.Start();
            }
            else
            {
                hidHideInstallBtn.Content = Properties.Resources.InstallFailed;
                EnableControls(true);
            }
        }

        private void HidHideInstallTimer_Elapsed(object sender,
            System.Timers.ElapsedEventArgs e)
        {
            ((NonFormTimer)sender).Stop();
            bool finished = false;
            if (monitorProc != null && monitorProc.HasExited)
            {
                if (DS4Windows.Global.IsHidHideInstalled())
                {
                    Dispatcher.BeginInvoke((Action)(() =>
                    {
                        hidHideInstallBtn.Content = Properties.Resources.InstallComplete;
                        EnableControls(true);
                    }));
                }
                else
                {
                    Dispatcher.BeginInvoke((Action)(() =>
                    {
                        hidHideInstallBtn.Content = Properties.Resources.InstallFailed;
                        EnableControls(true);
                    }), null);
                }

                string currentFilePath = Path.Combine(Path.GetTempPath(), $"{InstHidHideFileNameX64}");
                File.Delete(currentFilePath);
                ((NonFormTimer)sender).Stop();
                finished = true;
            }

            if (!finished)
            {
                ((NonFormTimer)sender).Start();
            }
        }

        private void FakerInputInstallBtn_Click(object sender, RoutedEventArgs e)
        {
            string tempFilePath = Path.Combine(Path.GetTempPath(), $"{installFileName}");
            if (File.Exists(tempFilePath))
            {
                File.Delete(tempFilePath);
            }

            string tempInstFakerInputName = Path.Combine(Path.GetTempPath(), $"{instFakerInputFileName}.tmp");
            if (File.Exists(tempInstFakerInputName))
            {
                File.Delete(tempInstFakerInputName);
            }

            EnableControls(false);
            FakerInputDownloadLaunch();
        }

        private async void FakerInputDownloadLaunch()
        {
            Progress<ICopyProgress> progress = new Progress<ICopyProgress>(x => // Please see "Notes on IProgress<T>"
            {
                // This is your progress event!
                // It will fire on every buffer fill so don't do anything expensive.
                // Writing to the console IS expensive, so don't do the following in practice...
                fakerInputInstallBtn.Content = Properties.Resources.Downloading.Replace("*number*%",
                    x.PercentComplete.ToString("P"));
                //Console.WriteLine(x.PercentComplete.ToString("P"));
            });

            string tempInstFakerInputFileName = Path.Combine(Path.GetTempPath(),
                $"{instFakerInputFileName}.tmp");
            string finalFilename = Path.Combine(Path.GetTempPath(), $"{instFakerInputFileName}");
            bool success = false;
            using (var downloadStream = new FileStream(tempInstFakerInputFileName, FileMode.CreateNew))
            {
                HttpResponseMessage response = await App.requestClient.GetAsync(installFakerInputDL,
                    downloadStream, progress);
                success = response.IsSuccessStatusCode;
            }

            if (success)
            {
                File.Move(tempInstFakerInputFileName, finalFilename);
            }
            success = false; // Reset for later check

            if (File.Exists(finalFilename))
            {
                //vigemInstallBtn.Content = Properties.Resources.OpeningInstaller;
                ProcessStartInfo startInfo = new ProcessStartInfo(finalFilename);
                startInfo.UseShellExecute = true; // Needed to run program as admin
                monitorProc = Process.Start(startInfo);
                fakerInputInstallBtn.Content = Properties.Resources.Installing;
                success = true;
            }

            if (success)
            {
                monitorTimer = new NonFormTimer();
                monitorTimer.Elapsed += FakerInputInstallTimer_Elapsed;
                monitorTimer.Start();
            }
            else
            {
                fakerInputInstallBtn.Content = Properties.Resources.InstallFailed;
                EnableControls(true);
            }
        }

        private void FakerInputInstallTimer_Elapsed(object sender,
            System.Timers.ElapsedEventArgs e)
        {
            ((NonFormTimer)sender).Stop();
            bool finished = false;
            if (monitorProc != null && monitorProc.HasExited)
            {
                if (DS4Windows.Global.IsFakerInputInstalled())
                {
                    Dispatcher.BeginInvoke((Action)(() =>
                    {
                        fakerInputInstallBtn.Content = Properties.Resources.InstallComplete;
                        EnableControls(true);
                    }));
                }
                else
                {
                    Dispatcher.BeginInvoke((Action)(() =>
                    {
                        fakerInputInstallBtn.Content = Properties.Resources.InstallFailed;
                        EnableControls(true);
                    }), null);
                }

                string currentFilePath = Path.Combine(Path.GetTempPath(), $"{instFakerInputFileName}");
                File.Delete(currentFilePath);
                ((NonFormTimer)sender).Stop();
                finished = true;
            }

            if (!finished)
            {
                ((NonFormTimer)sender).Start();
            }
        }

        private void EnableControls(bool on)
        {
            vigemInstallBtn.IsEnabled = on;
            step4HidHidePanel.IsEnabled = on;
            step5FakerInputPanel.IsEnabled = on;

            // Perform compatibility checks for controls that might need
            // to be disabled when on is set to true
            if (on)
            {
                LateControlsCheck();
            }
        }

        /// <summary>
        /// Possibly disable some controls for components that are not compatible
        /// with the installed version of Windows or system configuration
        /// </summary>
        private void LateControlsCheck()
        {
            step4HidHidePanel.IsEnabled = IsHidHideControlCompatible();
            step5FakerInputPanel.IsEnabled = IsFakerInputControlCompatible();
        }
    }
}
