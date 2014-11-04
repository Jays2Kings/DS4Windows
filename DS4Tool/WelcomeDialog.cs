using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net;
using DS4Control;
using System.IO;
using System.IO.Compression;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Security.Permissions;
using System.Reflection;

namespace ScpServer
{
    public partial class WelcomeDialog : Form
    {
        public WelcomeDialog()
        {
            InitializeComponent();
            this.Icon = Properties.Resources.DS4;
            
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
            WebClient wb = new WebClient();
            if (bnStep1.Text == Properties.Resources.Step1)
            {
                wb.DownloadFileAsync(new Uri("http://ds4windows.com/Files/Virtual Bus Driver.zip"), Global.appdatapath + "\\VBus.zip");
                wb.DownloadProgressChanged += wb_DownloadProgressChanged;
                wb.DownloadFileCompleted += wb_DownloadFileCompleted;               
            }  
        }

        private void wb_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            bnStep1.Text = Properties.Resources.Downloading.Replace("*number*", e.ProgressPercentage.ToString());
        }

        string exepath = Directory.GetParent(Assembly.GetExecutingAssembly().Location).FullName;
        private void wb_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            if (!Global.AdminNeeded())
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
                Directory.CreateDirectory(Global.appdatapath + "\\Virtual Bus Driver");
                try { ZipFile.ExtractToDirectory(Global.appdatapath + "\\VBus.zip", Global.appdatapath + "\\Virtual Bus Driver"); } //Saved so the user can uninstall later
                catch { }
                try { ZipFile.ExtractToDirectory(Global.appdatapath + "\\VBus.zip", exepath); }
                //Made here as starting the scpdriver.exe via process.start, the program looks for file from where it was called, not where the exe is
                catch { }
                if (File.Exists(exepath + "\\ScpDriver.exe"))
                    try
                    {
                        Process.Start(exepath + "\\ScpDriver.exe", "si");
                        bnStep1.Text = Properties.Resources.Installing;
                    }
                    catch { Process.Start(Global.appdatapath + "\\Virtual Bus Driver"); }
                
                Timer timer = new Timer();
                timer.Start();
                timer.Tick += timer_Tick;
            }
            else
            {
                bnStep1.Text = Properties.Resources.OpenScpDriver;
                Directory.CreateDirectory(Global.appdatapath + "\\Virtual Bus Driver");
                try { ZipFile.ExtractToDirectory(Global.appdatapath + "\\VBus.zip", Global.appdatapath + "\\Virtual Bus Driver"); }
                catch { }
                Process.Start(Global.appdatapath + "\\Virtual Bus Driver");
                Timer timer = new Timer();
                timer.Start();
                timer.Tick += timer_Tick;
            }
        }

        bool running = false;
        private void timer_Tick(object sender, EventArgs e)
        {
            Process[] processes = Process.GetProcessesByName("ScpDriver");
            if (!Global.AdminNeeded())
            {
                if (processes.Length < 1)
                {
                    bnStep1.Text = Properties.Resources.InstallComplete;
                    
                    try
                    {
                        File.Delete(exepath + "\\ScpDriver.exe");
                        File.Delete(exepath + "\\ScpDriver.log");
                        Directory.Delete(exepath + "\\System", true);
                        Directory.Delete(exepath + "\\DIFxAPI", true);
                    }
                    catch { }
                    File.Delete(Global.appdatapath + "\\VBus.zip");
                    ((Timer)sender).Stop();
                }
            }
            else
            {
                if (processes.Length > 0)
                    running = true;
                if (running)
                    if (processes.Length < 1)
                    {
                        bnStep1.Text = Properties.Resources.InstallComplete;
                        File.Delete(Global.appdatapath + "\\VBus.zip");
                        ((Timer)sender).Stop();
                    }
            }
        }


        private void button2_Click(object sender, EventArgs e)
        {
             Process.Start("http://www.microsoft.com/hardware/en-us/d/xbox-360-controller-for-windows");
        }
    }
}
