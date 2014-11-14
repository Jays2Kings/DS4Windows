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
    public partial class KBM360 : Form
    {
        private DS4Control.Control scpDevice;
        private int device;
        private Button button;
        private Options ops;
        public List<string> macros = new List<string>();
        public List<int> macrostag = new List<int>();
        public bool macrorepeat;
        RecordBox rb;
        public KBM360(DS4Control.Control bus_device, int deviceNum, Options ooo, Button buton)
        {
            InitializeComponent();
            device = deviceNum;
            scpDevice = bus_device;
            ops = ooo;
            button = buton;
            cbToggle.Checked = button.Font.Italic;
            cbScanCode.Checked = button.Font.Bold;
            if (button.Font.Underline)
            {
                lBMacroOn.Visible = true;
                foreach (int i in ((int[])button.Tag))
                    macrostag.Add(i);
            }
            if (button.Name.StartsWith("bn"))
                Text = Properties.Resources.SelectActionTitle.Replace("*action*", button.Name.Substring(2));
            else if (button.Name.StartsWith("bnHold"))
            {
                Text = Properties.Resources.SelectActionTitle.Replace("*action*", button.Name.Substring(6));
                btnFallBack.Text = "Disable";
            }
            else if (button.Name.StartsWith("bnShift"))
            {
                Text = Properties.Resources.SelectActionTitle.Replace("*action*", button.Name.Substring(7));
                btnFallBack.Text = "Fall Back";
            }
            foreach (System.Windows.Forms.Control control in this.Controls)
                if (control is Button)
                    ((Button)control).Click += anybtn_Click;
            if (button.Name.Contains("Touch"))
            {
                btnMOUSEDOWN.Visible = false;
                btnMOUSELEFT.Visible = false;
                btnMOUSERIGHT.Visible = false;
                btnMOUSEUP.Visible = false;
            }
            ActiveControl = null;
        }

        public void anybtn_Click(object sender, EventArgs e)
        {
            if (rb == null && sender is Button && ((Button)sender).Name != "bnMacro")
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
                else if (((Button)sender).Tag == null)
                    keyname = ((Button)sender).Text;
                else if (((Button)sender).Tag.ToString().Contains("X360"))
                    keyname = ((Button)sender).Tag.ToString().Substring(4);
                else
                    keyname = ((Button)sender).Text;

                object keytag;
                if (((Button)sender).Tag != null && ((Button)sender).Tag.ToString().Contains("X360"))
                    keytag = ((Button)sender).Tag.ToString().Substring(4);
                else
                    keytag = ((Button)sender).Tag;
                lBMacroOn.Visible = false;
                ops.ChangeButtonText(keyname, keytag);
                this.Close();
            }
        }

        private void finalMeasure(object sender, FormClosedEventArgs e)
        {
            if (rb != null)
            {
                if (!rb.saved && rb.macros.Count > 0)
                    if (MessageBox.Show(Properties.Resources.SaveRecordedMacro, "DS4Windows", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == System.Windows.Forms.DialogResult.Yes)
                        rb.btnSave_Click(this, null);
            }
            if (lBMacroOn.Visible)
                ops.ChangeButtonText("Macro", macrostag.ToArray());
            ops.Toggle_Bn(cbScanCode.Checked, cbToggle.Checked, lBMacroOn.Visible, macrorepeat);
            ops.UpdateLists();
        }

        private void Key_Down_Action(object sender, KeyEventArgs e)
        {
            if (rb == null)
            {
                lBMacroOn.Visible = false;
                ops.ChangeButtonText(e.KeyCode.ToString(), e.KeyValue);
                this.Close();
            }
        }

        private void Key_Press_Action(object sender, KeyEventArgs e)
        {
            if (rb == null)
            {
                lBMacroOn.Visible = false;
                ops.ChangeButtonText(e.KeyCode.ToString(), e.KeyValue);
                this.Close();
            }
        }

        private void cbToggle_CheckedChanged(object sender, EventArgs e)
        {
            if (cbToggle.Checked)
                lBMacroOn.Visible = false;
        }

        private void btnMacro_Click(object sender, EventArgs e)
        {
            rb = new RecordBox(this, scpDevice);
            rb.TopLevel = false;
            rb.Dock = DockStyle.Fill;
            rb.Visible = true;
            Controls.Add(rb);
            rb.BringToFront();
            //rb.StartPosition = FormStartPosition.Manual;
            //rb.Location = new Point(this.Location.X + 580, this.Location.Y+ 55);
            //rb.Show();
        }

        protected override bool IsInputKey(Keys keyData)
        {
            switch (keyData)
            {
                case Keys.Right:
                case Keys.Left:
                case Keys.Up:
                case Keys.Down:
                    return true;
                case Keys.Shift | Keys.Right:
                case Keys.Shift | Keys.Left:
                case Keys.Shift | Keys.Up:
                case Keys.Shift | Keys.Down:
                    return true;
            }
            return base.IsInputKey(keyData);
        }
        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            switch (e.KeyCode)
            {
                case Keys.Left:
                case Keys.Right:
                case Keys.Up:
                case Keys.Down:
                    if (e.Shift)
                    {

                    }
                    else
                    {
                    }
                    break;
            }
        }
    }
}
