using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using HttpProgress;
using DS4Windows;
using System.Collections.Generic;

namespace DS4WinWPF.DS4Forms.ViewModels
{
    public class MainWindowsViewModel
    {
        private bool fullTabsEnabled = true;

        public bool FullTabsEnabled
        {
            get => fullTabsEnabled;
            set
            {
                fullTabsEnabled = value;
                FullTabsEnabledChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler FullTabsEnabledChanged;

        public string updaterExe = Environment.Is64BitProcess ? "DS4Updater.exe" : "DS4Updater_x86.exe";

        private string DownloadUpstreamUpdaterVersion()
        {
            string result = string.Empty;
            // Sorry other devs, gonna have to find your own server
            Uri url = new Uri("https://raw.githubusercontent.com/Ryochan7/DS4Updater/master/Updater2/newest.txt");
            string filename = Path.Combine(Path.GetTempPath(), "DS4Updater_version.txt");
            bool readFile = false;
            using (var downloadStream = new FileStream(filename, FileMode.Create))
            {
                Task<System.Net.Http.HttpResponseMessage> temp = App.requestClient.GetAsync(url.ToString(), downloadStream);
                temp.Wait();

                if (temp.Result.IsSuccessStatusCode) readFile = true;
            }

            if (readFile)
            {
                result = File.ReadAllText(filename).Trim();
                File.Delete(filename);
            }

            return result;
        }

        public bool RunUpdaterCheck(bool launch, out string upstreamVersion)
        {
            string destPath = Path.Combine(Global.exedirpath, "DS4Updater.exe");
            bool updaterExists = File.Exists(destPath);
            upstreamVersion = DownloadUpstreamUpdaterVersion();
            if (!updaterExists ||
                (!string.IsNullOrEmpty(upstreamVersion) && FileVersionInfo.GetVersionInfo(destPath).FileVersion.CompareTo(upstreamVersion) != 0))
            {
                launch = false;
                Uri url2 = new Uri($"https://github.com/Ryochan7/DS4Updater/releases/download/v{upstreamVersion}/{updaterExe}");
                string filename = Path.Combine(Path.GetTempPath(), "DS4Updater.exe");
                using (var downloadStream = new FileStream(filename, FileMode.Create))
                {
                    Task<System.Net.Http.HttpResponseMessage> temp =
                        App.requestClient.GetAsync(url2.ToString(), downloadStream);
                    temp.Wait();
                    if (temp.Result.IsSuccessStatusCode) launch = true;
                }

                if (launch)
                {
                    if (Global.AdminNeeded())
                    {
                        int copyStatus = DS4Windows.Util.ElevatedCopyUpdater(filename);
                        if (copyStatus != 0) launch = false;
                    }
                    else
                    {
                        if (updaterExists) File.Delete(destPath);
                        File.Move(filename, destPath);
                    }
                }
            }

            return launch;
        }

        public void DownloadUpstreamVersionInfo()
        {
            // Sorry other devs, gonna have to find your own server
            Uri url = new Uri("https://raw.githubusercontent.com/Ryochan7/DS4Windows/jay/DS4Windows/newest.txt");
            string filename = Global.appdatapath + "\\version.txt";
            bool success = false;
            using (var downloadStream = new FileStream(filename, FileMode.Create))
            {
                Task<System.Net.Http.HttpResponseMessage> temp = App.requestClient.GetAsync(url.ToString(), downloadStream);
                try
                {
                    temp.Wait();
                    if (temp.Result.IsSuccessStatusCode) success = true;
                }
                catch (AggregateException) { }
            }

            if (!success && File.Exists(filename))
            {
                File.Delete(filename);
            }
        }

        public void CheckDrivers()
        {
            bool deriverinstalled = Global.IsViGEmBusInstalled();
            if (!deriverinstalled || !Global.IsRunningSupportedViGEmBus())
            {
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.FileName = $"{Global.exelocation}";
                startInfo.Arguments = "-driverinstall";
                startInfo.Verb = "runas";
                startInfo.UseShellExecute = true;
                try
                {
                    using (Process temp = Process.Start(startInfo))
                    {
                    }
                }
                catch { }
            }
        }

        public bool LauchDS4Updater()
        {
            bool launch = false;
            using (Process p = new Process())
            {
                p.StartInfo.FileName = Path.Combine(Global.exedirpath, "DS4Updater.exe");
                bool isAdmin = Global.IsAdministrator();
                List<string> argList = new List<string>();
                argList.Add("-autolaunch");
                if (!isAdmin)
                {
                    argList.Add("-user");
                }

                // Specify current exe to have DS4Updater launch
                argList.Add("--launchExe");
                argList.Add(Global.exeFileName);

                p.StartInfo.Arguments = string.Join(" ", argList);
                if (Global.AdminNeeded())
                    p.StartInfo.Verb = "runas";

                try { launch = p.Start(); }
                catch (InvalidOperationException) { }
            }

            return launch;
        }
    }
}
