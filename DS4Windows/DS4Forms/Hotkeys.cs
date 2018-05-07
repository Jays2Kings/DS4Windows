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
            {
                foreach (Control ctrl in tPCredits.Controls)
                {
                    if (ctrl.HasChildren)
                    {
                        foreach (Control ctrl2 in ctrl.Controls)
                            ctrl2.MouseHover += Items_MouseHover;
                    }

                    ctrl.MouseHover += Items_MouseHover;
                }
            }

            tPCredits.MouseHover += Items_MouseHover;
            lbLinkText.Text = string.Empty;
        }

        private void Items_MouseHover(object sender, EventArgs e)
        {
            switch (((Control)sender).Name)
            {
                case "linkJays2Kings": lbLinkText.Text = "http://ds4windows.com"; break;
                case "linkElectro": lbLinkText.Text = "https://code.google.com/r/brianfundakowskifeldman-ds4windows/"; break;
                case "linkInhexSTER": lbLinkText.Text = "https://code.google.com/p/ds4-tool/"; break;
                case "linkJhebbel": lbLinkText.Text = "http://dsdcs.com/index.php/portfolio/software-development/4-ds4windows"; break;
                case "linkCurrentSite": lbLinkText.Text = "https://ryochan7.github.io/ds4windows-site/"; break;
                case "linkSourceCode": lbLinkText.Text = "https://github.com/Ryochan7/DS4Windows"; break;
                case "linkBoganhobo": lbLinkText.Text = "https://github.com/boganhobo"; break;
                case "linkChamilsaan": lbLinkText.Text = "https://github.com/Chamilsaan"; break;
                case "linkKiliansch": lbLinkText.Text = "https://github.com/kiliansch"; break;
                case "linkTeokp": lbLinkText.Text = "https://github.com/teokp"; break;
                default: lbLinkText.Text = string.Empty; break;
            }
        }

        private void LinkJays2Kings_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://github.com/Jays2Kings/");
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

        private void LLChangelog_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://docs.google.com/document/d/1CovpH08fbPSXrC6TmEprzgPwCe0tTjQ_HTFfDotpmxk/edit?usp=sharing");
        }

        private void LinkDonate_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://paypal.me/ryochan7");
        }

        private void LinkSourceCode_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://github.com/Ryochan7/DS4Windows");
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

        private void LinkCurrentSite_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://ryochan7.github.io/ds4windows-site/");
        }
    }
}
