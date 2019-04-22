using System;
using System.ComponentModel;
using System.Windows.Forms;
using System.Net;

using System.IO;
using System.IO.Compression;
using System.Diagnostics;
//using NonFormTimer = System.Threading.Timer;
using NonFormTimer = System.Timers.Timer;
using System.Threading.Tasks;
using static DS4Windows.Global;

namespace DS4Windows
{
    public partial class WelcomeDialog : Form
    {
        private const string InstallerDL =
            "https://github.com/ViGEm/ViGEmBus/releases/download/v1.16.112/ViGEmBus_Setup_1.16.112.exe";
        private const string InstFileName = "ViGEmBus_Setup_1.16.112.exe";

        public WelcomeDialog(bool loadConfig=false)
        {
            if (loadConfig)
            {
                Global.FindConfigLocation();
                Global.Load();
                Global.SetCulture(Global.UseLang);
            }

            InitializeComponent();
            Icon = Properties.Resources.DS4;
        }

        private void bnFinish_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void linkBluetoothSettings_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("control", "bthprops.cpl");
        }

        private void bnStep1_Click(object sender, EventArgs e)
        {
            if (File.Exists(exepath + $"\\{InstFileName}"))
            {
                File.Delete(exepath + $"\\{InstFileName}");
            }

            WebClient wb = new WebClient();
            wb.DownloadFileAsync(new Uri(InstallerDL), exepath + $"\\{InstFileName}");

            wb.DownloadProgressChanged += wb_DownloadProgressChanged;
            wb.DownloadFileCompleted += wb_DownloadFileCompleted;
        }

        private void wb_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            bnStep1.Text = Properties.Resources.Downloading.Replace("*number*", e.ProgressPercentage.ToString());
        }

        private void wb_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            if (Directory.Exists(exepath + "\\ViGEmBusInstaller"))
            {
                Directory.Delete(exepath + "\\ViGEmBusInstaller", true);
            }

            if (File.Exists(exepath + $"\\{InstFileName}"))
            {
                bnStep1.Text = Properties.Resources.OpeningInstaller;
                Process.Start(exepath + $"\\{InstFileName}", "/quiet");
                bnStep1.Text = Properties.Resources.Installing;
            }

            NonFormTimer timer = new NonFormTimer();
            timer.Elapsed += timer_Tick;
            timer.Start();
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            Process[] processes = Process.GetProcessesByName("ViGEmBus_Setup_1.16.112");
            if (processes.Length < 1)
            {
                if (Global.IsViGEmBusInstalled())
                {
                    this.BeginInvoke((Action)(() => { bnStep1.Text = Properties.Resources.InstallComplete; }));
                }
                else
                {
                    this.BeginInvoke((Action)(() => { bnStep1.Text = Properties.Resources.InstallFailed; }), null);
                }

                File.Delete(exepath + $"\\{InstFileName}");
                ((NonFormTimer)sender).Stop();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
             Process.Start("http://www.microsoft.com/accessories/en-gb/d/xbox-360-controller-for-windows");
        }
    }
}
