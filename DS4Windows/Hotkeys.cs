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
using System.Reflection;
using System.Diagnostics;

namespace DS4Windows
{
    public partial class Hotkeys : Form
    {
        public Hotkeys()
        {
            InitializeComponent();
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
                default: lbLinkText.Text = string.Empty; break;
            }
        }

        private void linkJays2Kings_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("http://ds4windows.com");
        }

        private void linkElectro_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://code.google.com/r/brianfundakowskifeldman-ds4windows/");
        }

        private void linkInhexSTER_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://code.google.com/p/ds4-tool/");
        }

        private void linkJhebbel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("http://dsdcs.com/index.php/portfolio/software-development/4-ds4windows");
        }

        private void lLChangelog_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://docs.google.com/document/d/1l4xcgVQkGUskc5CQ0p069yW22Cd5WAH_yE3Fz2hXo0E/edit?usp=sharing");
        }

        private void linkDonate_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://www.paypal.com/cgi-bin/webscr?cmd=_donations&business=2FTZ9BZEHSQ8Q&lc=US&item_name=DS4Windows&currency_code=USD&bn=PP%2dDonationsBF%3abtn_donateCC_LG%2egif%3aNonHosted");
        }

        private void linkSourceCode_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/Jays2Kings/DS4Windows");
        }

    }
}
