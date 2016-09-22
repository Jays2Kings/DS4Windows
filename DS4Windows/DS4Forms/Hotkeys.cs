using System;
using System.Windows.Forms;

using System.Reflection;
using System.Threading;
using System.Diagnostics;
using System.Drawing;

namespace DS4Windows
{
    public partial class Hotkeys : Form
    {
        public Hotkeys()
        {
            InitializeComponent();
            string s = Thread.CurrentThread.CurrentUICulture.ToString().Split('-')[0];
            
            Control[] ctrls = tLPTranslators.Controls.Find("lb" + s, true);
            if (ctrls.Length > 0)
            {
                ((Label)ctrls[0]).ForeColor = Color.DarkGreen;
                int ind = tLPTranslators.Controls.IndexOf(ctrls[0]) + 1;
                ((Label)tLPTranslators.Controls[ind]).ForeColor = Color.DarkGreen;
            }
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location);
            string version = fvi.FileVersion;
            lbAbout.Text += version + ")";
            if (tPCredits.HasChildren)
                foreach (System.Windows.Forms.Control ctrl in tPCredits.Controls)
                {
                    if (ctrl.HasChildren)
                        foreach (System.Windows.Forms.Control ctrl2 in ctrl.Controls)
                            ctrl2.MouseHover += Items_MouseHover;
                    ctrl.MouseHover += Items_MouseHover;
                }
            tPCredits.MouseHover += Items_MouseHover;
            lbLinkText.Text = string.Empty;
        }

        private void Items_MouseHover(object sender, EventArgs e)
        {
            switch (((System.Windows.Forms.Control)sender).Name)
            {
                //if (File.Exists(appdatapath + "\\Auto Profiles.xml"))
                case "linkJays2Kings": lbLinkText.Text = "http://ds4windows.com"; break;
                case "linkElectro": lbLinkText.Text = "https://code.google.com/r/brianfundakowskifeldman-ds4windows/"; break;
                case "linkInhexSTER": lbLinkText.Text = "https://code.google.com/p/ds4-tool/"; break;
                case "linkJhebbel": lbLinkText.Text = "http://dsdcs.com/index.php/portfolio/software-development/4-ds4windows"; break;
                case "linkSourceCode": lbLinkText.Text = "https://github.com/Jays2Kings/DS4Windows"; break;
                case "linkBoganhobo": lbLinkText.Text = "https://github.com/boganhobo"; break;
                case "linkChamilsaan": lbLinkText.Text = "https://github.com/Chamilsaan"; break;
                case "linkKiliansch": lbLinkText.Text = "https://github.com/kiliansch"; break;
                case "linkTeokp": lbLinkText.Text = "https://github.com/teokp"; break;
                default: lbLinkText.Text = string.Empty; break;
            }
        }

        private void linkJays2Kings_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("http://ds4windows.com");
        }

        private void linkElectro_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://code.google.com/r/brianfundakowskifeldman-ds4windows/");
        }

        private void linkInhexSTER_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://code.google.com/p/ds4-tool/");
        }

        private void linkJhebbel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("http://dsdcs.com/index.php/portfolio/software-development/4-ds4windows");
        }

        private void lLChangelog_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://docs.google.com/document/d/1l4xcgVQkGUskc5CQ0p069yW22Cd5WAH_yE3Fz2hXo0E/edit?usp=sharing");
        }

        private void linkDonate_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://www.paypal.com/cgi-bin/webscr?cmd=_donations&business=2FTZ9BZEHSQ8Q&lc=US&item_name=DS4Windows&currency_code=USD&bn=PP%2dDonationsBF%3abtn_donateCC_LG%2egif%3aNonHosted");
        }

        private void linkSourceCode_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://github.com/Jays2Kings/DS4Windows");
        }

        private void linkBoganhobo_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
           Process.Start("https://github.com/boganhobo");
        }

        private void linkChamilsaan_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://github.com/Chamilsaan");
        }

        private void linkKiliansch_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://github.com/kiliansch");
        }

        private void linkTeokp_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://github.com/teokp");
        }
    }
}
