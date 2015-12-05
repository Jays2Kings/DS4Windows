using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net;

using System.IO;
using System.IO.Compression;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Security.Permissions;
using System.Reflection;

namespace DS4Windows
{
    public partial class WelcomeDialog : Form
    {
        public WelcomeDialog()
        {
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
        bool driverinstalling = false;
        private void bnStep1_Click(object sender, EventArgs e)
        {
            WebClient wb = new WebClient();
            if (!driverinstalling)
            {
                wb.DownloadFileAsync(new Uri("http://ds4windows.com/Files/Virtual Bus Driver.zip"), exepath + "\\VBus.zip");
                wb.DownloadProgressChanged += wb_DownloadProgressChanged;
                wb.DownloadFileCompleted += wb_DownloadFileCompleted;
                driverinstalling = true;
            }
        }

        private void wb_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            bnStep1.Text = Properties.Resources.Downloading.Replace("*number*", e.ProgressPercentage.ToString());
        }

        string exepath = Directory.GetParent(Assembly.GetExecutingAssembly().Location).FullName;
        private void wb_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            bnStep1.Text = Properties.Resources.OpeningInstaller;
            try
            {
                File.Delete(exepath + "\\ScpDriver.exe");
                File.Delete(exepath + "\\ScpDriver.log");
                Directory.Delete(exepath + "\\System", true);
                Directory.Delete(exepath + "\\DIFxAPI", true);
            }
            catch { }
            Directory.CreateDirectory(exepath + "\\Virtual Bus Driver");
            try { ZipFile.ExtractToDirectory(exepath + "\\VBus.zip", exepath + "\\Virtual Bus Driver"); } //Saved so the user can uninstall later
            catch { }
            try { ZipFile.ExtractToDirectory(exepath + "\\VBus.zip", exepath); }
            //Made here as starting the scpdriver.exe via process.start, the program looks for file from where it was called, not where the exe is
            catch { }
            if (File.Exists(exepath + "\\ScpDriver.exe"))
                try
                {
                    Process.Start(exepath + "\\ScpDriver.exe", "si");
                    bnStep1.Text = Properties.Resources.Installing;
                }
                catch { Process.Start(exepath + "\\Virtual Bus Driver"); }

            Timer timer = new Timer();
            timer.Start();
            timer.Tick += timer_Tick;
        }
        bool waitForFile;
        DateTime waitFileCheck;
        private void timer_Tick(object sender, EventArgs e)
        {
            Process[] processes = Process.GetProcessesByName("ScpDriver");
            if (processes.Length < 1)
            {
                if (!File.Exists(exepath + "\\ScpDriver.log") && !waitForFile)
                {
                    waitForFile = true;
                    waitFileCheck = DateTime.UtcNow;
                    return;
                }
                if (waitForFile && waitFileCheck + TimeSpan.FromMinutes(2) < DateTime.UtcNow)
                {
                    bnStep1.Text = Properties.Resources.InstallFailed;
                    Process.Start(exepath + "\\Virtual Bus Driver");
                    File.Delete(exepath + "\\VBus.zip");
                    ((Timer)sender).Stop();
                    return;
                }
                else if (waitForFile)
                    return;
                string log = File.ReadAllText(exepath + "\\ScpDriver.log");
                if (log.Contains("Install Succeeded"))
                    bnStep1.Text = Properties.Resources.InstallComplete;
                else
                {
                    bnStep1.Text = Properties.Resources.InstallFailed;
                    Process.Start(exepath + "\\Virtual Bus Driver");
                }
                try
                {
                    File.Delete(exepath + "\\ScpDriver.exe");
                    File.Delete(exepath + "\\ScpDriver.log");
                    Directory.Delete(exepath + "\\System", true);
                    Directory.Delete(exepath + "\\DIFxAPI", true);
                }
                catch { }
                File.Delete(exepath + "\\VBus.zip");
                ((Timer)sender).Stop();
            }
        }


        private void button2_Click(object sender, EventArgs e)
        {
             Process.Start("http://www.microsoft.com/hardware/en-us/d/xbox-360-controller-for-windows");
        }
    }
}
