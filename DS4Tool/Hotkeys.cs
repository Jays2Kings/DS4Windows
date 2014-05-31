using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using DS4Control;
using System.Net;
using System.IO;

namespace ScpServer
{
    public partial class Hotkeys : Form
    {
        ScpForm form;
        public Hotkeys(ScpForm main)
        {
            form = main;
            InitializeComponent();
            lbAbout.Text += Global.getVersion().ToString() + ")";
            ToolTip tt = new ToolTip();
            tt.SetToolTip(linkUninstall, "To fully remove DS4Windows, You can delete the profiles by the link to the other side");
            if (!System.IO.Directory.Exists(Global.appdatapath + "\\Virtual Bus Driver"))
                linkUninstall.Visible = false;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void linkProfiles_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start(Global.appdatapath + "\\Profiles");
        }

        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://code.google.com/r/jays2kings-ds4tool/source/list?name=jay");
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://code.google.com/r/brianfundakowskifeldman-ds4windows/");
        }

        private void linkInhexSTER_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://code.google.com/p/ds4-tool/");
        }

        private void linkJhebbel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://code.google.com/r/jhebbel-ds4tool/source/browse/");
        }

        private void linkUninstall_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (System.IO.File.Exists(Global.appdatapath + "\\Virtual Bus Driver\\ScpDriver.exe"))
                try { System.Diagnostics.Process.Start(Global.appdatapath + "\\Virtual Bus Driver\\ScpDriver.exe"); }
                catch { System.Diagnostics.Process.Start(Global.appdatapath + "\\Virtual Bus Driver"); }
        }

        private void lLBUpdate_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Uri url = new Uri("https://dl.dropboxusercontent.com/u/16364552/DS4Tool/newest%20version.txt"); //Sorry other devs, gonna have to find your own server
            WebClient wc = new WebClient();
            wc.DownloadFileAsync(url, Global.appdatapath + "\\version.txt");
            wc.DownloadFileCompleted += wc_DownloadFileCompleted;            
        }

        void wc_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            Global.setLastChecked(DateTime.Now);
            double newversion;
            try
            {
                if (double.TryParse(File.ReadAllText(Global.appdatapath + "\\version.txt"), out newversion))
                    if (newversion > Global.getVersion())
                        if (MessageBox.Show("Download now?", "DS4Windows Update Available!", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == System.Windows.Forms.DialogResult.Yes)
                        {
                            if (!File.Exists("Updater.exe"))
                            {
                                Uri url2 = new Uri("https://dl.dropboxusercontent.com/u/16364552/DS4Tool/Updater.exe");
                                WebClient wc2 = new WebClient();
                                wc2.DownloadFile(url2, "Updater.exe");
                            }
                            System.Diagnostics.Process.Start("Updater.exe");
                            form.Close();
                        }
                        else
                            File.Delete(Global.appdatapath + "\\version.txt");
                    else
                    {
                        File.Delete(Global.appdatapath + "\\version.txt");
                        MessageBox.Show("You are up to date", "DS4 Updater");
                    }
                else
                    File.Delete(Global.appdatapath + "\\version.txt");
            }
            catch { };
        }
    }
}
