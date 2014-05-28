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
        public KBM360(DS4Control.Control bus_device, int deviceNum, Options ooo, Button buton)
        {
            InitializeComponent();
            device = deviceNum;
            scpDevice = bus_device;
            ops = ooo;
            button = buton;
            cbToggle.Checked = button.Font.Italic;
            cbScanCode.Checked = button.Font.Bold;
            cBMacro.Checked = button.Font.Underline;
            if (cBMacro.Checked)
                lBMacroOrder.Text += button.Text;
            Text = "Select an action for " + button.Name.Substring(2);
            foreach (System.Windows.Forms.Control control in this.Controls)
                if (control is Button)
                    ((Button)control).Click += new System.EventHandler(anybtn_Click);
            if (button.Name.Contains("Touch"))
            {
                bnMOUSEDOWN.Visible = false;
                bnMOUSELEFT.Visible = false;
                bnMOUSERIGHT.Visible = false;
                bnMOUSEUP.Visible = false;
            }
            ToolTip tp = new ToolTip();
            tp.SetToolTip(cBMacro, "Max 5 actions");
        }
        List<string> macros = new List<string>();
        List<int> macrostag = new List<int>();
        public void anybtn_Click(object sender, EventArgs e)
        {
            if (sender is Button)
            {
                Button bn = ((Button)sender);
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

                if (!cBMacro.Checked)
                {
                    object keytag;
                    if (((Button)sender).Tag.ToString() == "X360")
                        keytag = ((Button)sender).Text;
                    else
                        keytag = ((Button)sender).Tag;
                    ops.ChangeButtonText(keyname, keytag);
                    this.Close();
                }
                else
                {
                    if (!bn.Font.Bold && bn.Tag.ToString() != "X360" && macrostag.Count < 5 && (bn.Text.Contains("Mouse") ^ !bn.Text.Contains("Button"))) //end is xor to remove mouse movement and wheel from macro
                    {
                        bn.Font = new Font(bn.Font, FontStyle.Bold);
                        macros.Add(keyname);
                        int value;
                        if (int.TryParse(bn.Tag.ToString(), out value))
                            macrostag.Add(value);
                        else
                        {
                            if (bn.Text == "Left Mouse Button") macrostag.Add(256);
                            if (bn.Text == "Right Mouse Button") macrostag.Add(257);
                            if (bn.Text == "Middle Mouse Button") macrostag.Add(258);
                            if (bn.Text == "4th Mouse Button") macrostag.Add(259);
                            if (bn.Text == "5th Mouse Button") macrostag.Add(260);
                        }
                    }
                    else if (bn.Tag.ToString() != "X360")
                    {
                        bn.Font = new Font(bn.Font, FontStyle.Regular);
                        macros.Remove(keyname);
                        int value;
                        if (int.TryParse(bn.Tag.ToString(), out value))
                            macrostag.Remove(value);
                        else
                        {
                            if (bn.Text == "Left Mouse Button") macrostag.Remove(256);
                            if (bn.Text == "Right Mouse Button") macrostag.Remove(257);
                            if (bn.Text == "Middle Mouse Button") macrostag.Remove(258);
                            if (bn.Text == "4th Mouse Button") macrostag.Remove(259);
                            if (bn.Text == "5th Mouse Button") macrostag.Remove(260);
                        }
                    }
                    string macro = string.Join(", ", macros.ToArray());
                    lBMacroOrder.Text = "Macro Order: " + macro;
                }
            }
        }

        private void finalMeasure(object sender, FormClosedEventArgs e)
        {
            if (cBMacro.Checked && macrostag.Count > 0)
            {
                ops.ChangeButtonText(string.Join(", ", macros), macrostag.ToArray());
            }
            ops.Toggle_Bn(cbScanCode.Checked, cbToggle.Checked, cBMacro.Checked);
            ops.UpdateLists();
        }

        private void Key_Down_Action(object sender, KeyEventArgs e)
        {
            ops.ChangeButtonText(e.KeyCode.ToString(), e.KeyValue);
            this.Close();
        }

        private void cBMacro_CheckedChanged(object sender, EventArgs e)
        {
            lBMacroOrder.Visible = cBMacro.Checked;
            if (cBMacro.Checked)
                cbToggle.Checked = false;
        }

        private void cbToggle_CheckedChanged(object sender, EventArgs e)
        {
            if (cbToggle.Checked)
                cBMacro.Checked = false;
        }

    }
}
