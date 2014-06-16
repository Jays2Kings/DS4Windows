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
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
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

        private void lLChangelog_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://docs.google.com/document/d/1l4xcgVQkGUskc5CQ0p069yW22Cd5WAH_yE3Fz2hXo0E/edit?usp=sharing");
        }

    }
}
