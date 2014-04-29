using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ScpServer
{
    public partial class KBM360 : Form
    {
        private DS4Control.Control scpDevice;
        private int device;
        private Button button;
        private Options ops;
        public KBM360(DS4Control.Control bus_device, int deviceNum, Options ooo, Button buton, int tabStart)
        {
            InitializeComponent();
            device = deviceNum;
            scpDevice = bus_device;
            ops = ooo;
            button = buton;
            if (button.ForeColor == Color.Red)
                cbRepeat.Checked = true;
            if (button.Font.Bold)
                cbScanCode.Checked = true;
            Text = "Select an action for " + button.Name.Substring(2);
            foreach (System.Windows.Forms.Control control in tabKBM.Controls)
                if (control is Button)
                    ((Button)control).Click += new System.EventHandler(anybtn_Click);
            foreach (System.Windows.Forms.Control control in tab360.Controls)
                if (control is Button)
                    ((Button)control).Click += new System.EventHandler(anybtn_Click360);
            if (button.Name.Contains("Touch"))
            {
                bnMOUSEDOWN.Visible = false;
                bnMOUSELEFT.Visible = false;
                bnMOUSERIGHT.Visible = false;
                bnMOUSEUP.Visible = false;                    
            }
            if (tabStart < 2)
                tabControl1.SelectedIndex = tabStart;
            else
                tabControl1.SelectedIndex = 0;
            ReFocus();
        }

        public void anybtn_Click(object sender, EventArgs e)
        {
            if (sender is Button)
            {
                string keyname;
                if (((Button)sender).Text.Contains('↑') || ((Button)sender).Text.Contains('↓') || ((Button)sender).Text.Contains('→') || ((Button)sender).Text.Contains('←') || ((Button)sender).Text.Contains('Ø'))
                    keyname = ((Button)sender).Text.Substring(1);
                else if (((Button)sender).Font.Name == "Webdings")
                {
                    if (((Button)sender).Text == "9")
                        keyname = "Previous Track";
                    else if (((Button)sender).Text == "<")
                        keyname = "Stop Track";
                    else if (((Button)sender).Text == "4")
                        keyname = "Play/Pause";
                    else if (((Button)sender).Text == ":")
                        keyname = "Next Track";
                    else
                        keyname = "How did you get here?";
                }
                else
                    keyname = ((Button)sender).Text;
                object keytag = ((Button)sender).Tag;
                ops.ChangeButtonText(keyname, keytag);
                this.Close();
            }
        }

        private void ReFocus()
        {
            if (ops.Focused)
                this.Enabled = false;
        }
        public void anybtn_Click360(object sender, EventArgs e)
        {
            if (sender is Button)
            {
                string keyname = ((Button)sender).Text;
                ops.ChangeButtonText(keyname);
                this.Close();
            }
        }

        private void finalMeasure(object sender, FormClosedEventArgs e)
        {
           ops.Toggle_Repeat(cbRepeat.Checked);
           ops.Toggle_ScanCode(cbScanCode.Checked);
           ops.UpdateLists(); 
        }

        private void Key_Down_Action(object sender, KeyEventArgs e)
        {
            ops.ChangeButtonText(e.KeyCode.ToString(), e.KeyValue);
            this.Close();
        }

    }
}
