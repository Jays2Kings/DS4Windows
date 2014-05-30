using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DS4Control;

namespace ScpServer
{
    public partial class MessageTextBox : Form
    {
        public string oldfilename;
        ScpForm yes;
        public MessageTextBox(string name, ScpForm mainwindow)
        {
            InitializeComponent();
            oldfilename = name;
            yes = mainwindow;
        }
        
        private void button1_Click(object sender, EventArgs e)
        {
            if (tBProfile.Text != null && tBProfile.Text != "" && !tBProfile.Text.Contains("\\") && !tBProfile.Text.Contains("/") && !tBProfile.Text.Contains(":") && !tBProfile.Text.Contains("*") && !tBProfile.Text.Contains("?") && !tBProfile.Text.Contains("\"") && !tBProfile.Text.Contains("<") && !tBProfile.Text.Contains(">") && !tBProfile.Text.Contains("|"))
            {
                System.IO.File.Copy(Global.appdatapath + "\\Profiles\\" + oldfilename + ".xml", Global.appdatapath + "\\Profiles\\" + tBProfile.Text + ".xml", true);
                yes.RefreshProfiles();
                this.Close();
            }
            else
                MessageBox.Show("Please enter a valid name", "Not valid", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);            
        }

        private void tBProfile_TextChanged(object sender, EventArgs e)
        {
            if (tBProfile.Text != null && tBProfile.Text != "" && !tBProfile.Text.Contains("\\") && !tBProfile.Text.Contains("/") && !tBProfile.Text.Contains(":") && !tBProfile.Text.Contains("*") && !tBProfile.Text.Contains("?") && !tBProfile.Text.Contains("\"") && !tBProfile.Text.Contains("<") && !tBProfile.Text.Contains(">") && !tBProfile.Text.Contains("|"))
                tBProfile.ForeColor = System.Drawing.SystemColors.WindowText;
            else
                tBProfile.ForeColor = System.Drawing.SystemColors.GrayText;
        }

        private void tBProfile_Enter(object sender, EventArgs e)
        {
            if (tBProfile.Text == "<type new name here>")
                tBProfile.Text = "";
        }

        private void tBProfile_Leave(object sender, EventArgs e)
        {
            if (tBProfile.Text == "")
                tBProfile.Text = "<type new name here>";
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
