using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using DS4Control;

namespace ScpServer
{
    public partial class Hotkeys : Form
    {
        public Hotkeys()
        {
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
    }
}
