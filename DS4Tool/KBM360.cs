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
        public List<string> macros = new List<string>();
        public List<int> macrostag = new List<int>();
        public bool macrorepeat;
        public KBM360(DS4Control.Control bus_device, int deviceNum, Options ooo, Button buton)
        {
            InitializeComponent();
            device = deviceNum;
            scpDevice = bus_device;
            ops = ooo;
            button = buton;
            cbToggle.Checked = button.Font.Italic;
            cbScanCode.Checked = button.Font.Bold;
            //cBMacro.Checked = button.Font.Underline;
            lBMacroOn.Visible = button.Font.Underline;
            if (button.Name.StartsWith("bn"))
                Text = "Select an action for " + button.Name.Substring(2);
            else if (button.Name.StartsWith("sbn"))
            {
                Text = "Select an action for " + button.Name.Substring(3);
                btnUNBOUND2.Text = "Fall Back";
                btnUNBOUND2.Tag = null;
            }
            foreach (System.Windows.Forms.Control control in this.Controls)
                if (control is Button)
                    ((Button)control).Click += anybtn_Click;
            if (button.Name.Contains("Touch"))
            {
                bnMOUSEDOWN.Visible = false;
                bnMOUSELEFT.Visible = false;
                bnMOUSERIGHT.Visible = false;
                bnMOUSEUP.Visible = false;
            }
            ActiveControl = null;
        }
       
        public void anybtn_Click(object sender, EventArgs e)
        {
            if (sender is Button && ((Button)sender).Name != "btnMacro")
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
                    keyname = "Fall back";
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
            if (lBMacroOn.Visible)
                ops.ChangeButtonText("Macro", macrostag.ToArray());
            ops.Toggle_Bn(cbScanCode.Checked, cbToggle.Checked, lBMacroOn.Visible, macrorepeat);
            ops.UpdateLists();
        }

        private void Key_Down_Action(object sender, KeyEventArgs e)
        {
            lBMacroOn.Visible = false;
            ops.ChangeButtonText(e.KeyCode.ToString(), e.KeyValue);
            this.Close();
        }

        private void Key_Press_Action(object sender, KeyEventArgs e)
        {
            lBMacroOn.Visible = false;
            ops.ChangeButtonText(e.KeyCode.ToString(), e.KeyValue);
            this.Close();
        }

        private void cbToggle_CheckedChanged(object sender, EventArgs e)
        {
            if (cbToggle.Checked)
                lBMacroOn.Visible = false;
        }

        private void btnMacro_Click(object sender, EventArgs e)
        {            
            RecordBox rb = new RecordBox(this);
            rb.StartPosition = FormStartPosition.Manual;
            rb.Location = new Point(this.Location.X + 580, this.Location.Y+ 55);
            rb.ShowDialog();
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
