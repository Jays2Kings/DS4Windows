using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace DS4Windows
{
    public partial class KBM360 : Form
    {
        private int device;
        private Button button;
        private Options ops;
        private SpecActions sA;
        public List<string> macros = new List<string>();
        public List<int> macrostag = new List<int>();
        public bool macrorepeat, newaction;
        RecordBox rb;
        object oldtag;
        bool scanavail, toggleavil;
        string keyname;
        object keytag;
        public KBM360(int deviceNum, Options ooo, Button buton)
        {
            InitializeComponent();
            device = deviceNum;
            ops = ooo;
            button = buton;
            cBToggle.Checked = button.Font.Italic;
            cBScanCode.Checked = button.Font.Bold;
            if (button.Tag != null)
            {
                string[] extras;
                if (button.Tag is KeyValuePair<int, string>)
                {
                    KeyValuePair<int, string> tag = (KeyValuePair<int, string>)button.Tag;
                    oldtag = tag.Key;
                    keytag = tag.Key;
                    extras = tag.Value.Split(',');
                }
                else if (button.Tag is KeyValuePair<Int32[], string>)
                {
                    KeyValuePair<Int32[], string> tag = (KeyValuePair<Int32[], string>)button.Tag;
                    oldtag = tag.Key;
                    keytag = tag.Key;
                    if (button.Font.Underline)
                    {
                        lBMacroOn.Visible = true;
                        foreach (int i in ((int[])tag.Key))
                            macrostag.Add(i);
                    }
                    if (button.Font.Strikeout)
                        macrorepeat = true;
                    extras = tag.Value.Split(',');
                }
                else if (button.Tag is KeyValuePair<string, string>)
                {
                    KeyValuePair<string, string> tag = (KeyValuePair<string, string>)button.Tag;
                    oldtag = tag.Key;
                    keytag = tag.Key;
                    extras = tag.Value.Split(',');
                }
                else
                {
                    KeyValuePair<object, string> tag = (KeyValuePair<object, string>)button.Tag;
                    extras = tag.Value.Split(',');
                }
                int b;
                try
                {
                    if (int.TryParse(extras[0], out b)) nUDHeavy.Value = b;
                    if (int.TryParse(extras[1], out b)) nUDLight.Value = b;
                    if (int.TryParse(extras[2], out b))
                        if (b == 1)
                        {
                            cBLightbar.Checked = true;
                            if (int.TryParse(extras[3], out b)) tBRedBar.Value = b;
                            if (int.TryParse(extras[4], out b)) tBGreenBar.Value = b;
                            if (int.TryParse(extras[5], out b)) tBBlueBar.Value = b;
                            if (int.TryParse(extras[6], out b)) nUDLightFlash.Value = b;
                        }
                    if (int.TryParse(extras[7], out b))
                        if (b == 1)
                        {
                            cBMouse.Checked = true;
                            if (int.TryParse(extras[8], out b)) nUDMouse.Value = b;
                        }

                }
                catch { }
            }
            if (button.Name.StartsWith("bnShift"))
            {
                Console.Write("shift");
                Text = Properties.Resources.SelectActionTitle.Replace("*action*", button.Name.Substring(7));
                btnDefault.Text = Properties.Resources.FallBack;
            }
            else if (button.Name.StartsWith("bn"))
                Text = Properties.Resources.SelectActionTitle.Replace("*action*", button.Name.Substring(2));
            foreach (System.Windows.Forms.Control control in Controls)
                if (control is Button)
                    ((Button)control).Click += anybtn_Click;
            if (button.Name.Contains("Touch") || button.Name.Contains("Swipe"))
            {
                btnMOUSEDOWN.Visible = false;
                btnMOUSELEFT.Visible = false;
                btnMOUSERIGHT.Visible = false;
                btnMOUSEUP.Visible = false;
            }
            ActiveControl = lBMacroOn;
        }

        public KBM360(SpecActions ooo, Button buton, bool extras)
        {
            InitializeComponent();
            sA = ooo;
            button = buton;
            Size = new System.Drawing.Size(763, 244);
            if (extras)
            {
                cBScanCode.Checked = button.Text.Contains("(SC)");
                cBToggle.Checked = button.Text.Contains("(Toggle)");
            }
            else
            {
                cBScanCode.Visible = false;
                cBToggle.Visible = false;
            }
            gBExtras.Visible = false;
            bnMacro.Visible = false;
            Text = Properties.Resources.SelectActionTitle.Replace("*action*", "Trigger");
            foreach (System.Windows.Forms.Control control in Controls)
                if (control is Button)
                    ((Button)control).Click += anybtn_Click;
            btnMOUSEDOWN.Visible = false;
            btnMOUSELEFT.Visible = false;
            btnMOUSERIGHT.Visible = false;
            btnMOUSEUP.Visible = false;
            ActiveControl = lBMacroOn;
        }

        public void anybtn_Click(object sender, EventArgs e)
        {
            if (rb == null && sender is Button && ((Button)sender).Name != "bnMacro" && ((Button)sender).Name != "bnTest")
            {
                Button bn = ((Button)sender);
                macrostag.Clear();
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
                {
                    keyname = ((Button)sender).Tag.ToString().Substring(4);
                }
                else
                    keyname = ((Button)sender).Text;

                if (((Button)sender).Tag != null && ((Button)sender).Tag.ToString().Contains("X360"))
                    keytag = ((Button)sender).Tag.ToString().Substring(4);
                else
                    keytag = ((Button)sender).Tag;

                lBMacroOn.Visible = false;
                string extras = GetExtras();
                KeyValuePair<object, string> tag = new KeyValuePair<object, string>(keytag, extras);
                newaction = true;
                if (ops != null)
                    ops.ChangeButtonText(keyname, tag);
                else if (sA != null)
                {
                    button.Text = keyname;
                    button.Tag = keytag;
                    button.ForeColor = Color.Black;
                }
                this.Close();
            }
        }

        private string GetExtras()
        {
            string t =(byte)nUDHeavy.Value + "," + (byte)nUDLight.Value + "," +
                (cBLightbar.Checked ? "1" + "," + tBRedBar.Value + "," + tBGreenBar.Value + "," + tBBlueBar.Value + "," + nUDLightFlash.Value: "0,0,0,0,0") + "," +
                   (cBMouse.Checked ? "1" + "," + (byte)nUDMouse.Value : "0,0");
            return t;
        }

        private void finalMeasure(object sender, FormClosedEventArgs e)
        {
            if (ops != null)
            {
                if (rb != null) //if record macro is open
                {
                    if (!rb.saved && rb.macros.Count > 0)
                        if (MessageBox.Show(Properties.Resources.SaveRecordedMacro, "DS4Windows", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == System.Windows.Forms.DialogResult.Yes)
                            rb.btnSave_Click(this, null);
                }
                if (lBMacroOn.Visible)
                {
                    string extras = GetExtras();
                    keytag = null;
                    KeyValuePair<object, string> tag = new KeyValuePair<object, string>(macrostag.ToArray(), extras);
                    ops.ChangeButtonText("Macro", tag);
                    //ops.ChangeButtonText("Macro", macrostag.ToArray());
                }
                else if (!newaction)
                {
                    string extras = GetExtras();
                    KeyValuePair<object, string> tag = new KeyValuePair<object, string>(oldtag, extras);
                    ops.ChangeButtonText(button.Text, tag);
                }
                int value;
                bool tagisint = keytag != null && Int32.TryParse(keytag.ToString(), out value);
                scanavail = lBMacroOn.Visible || tagisint;
                toggleavil = tagisint;
                ops.Toggle_Bn((scanavail ? cBScanCode.Checked : false), (toggleavil ? cBToggle.Checked : false), lBMacroOn.Visible, macrorepeat);
                ops.UpdateLists();
            }
            else if (sA != null)
            {
                if (button.Tag != null)
                {
                    int key;
                    if (int.TryParse(button.Tag.ToString(), out key))
                        button.Text = ((Keys)key).ToString() +
                            (cBScanCode.Checked ? " (SC)" : "") +
                            (cBToggle.Checked ? " (Toggle)" : "");
                }
                //button.Font = new Font(button.Font, (cBScanCode.Checked ? FontStyle.Bold : FontStyle.Regular) | (cBToggle.Checked ? FontStyle.Italic : FontStyle.Regular));
            }
        }

        private void Key_Down_Action(object sender, KeyEventArgs e)
        {
            if (rb == null && !(ActiveControl is NumericUpDown) && !(ActiveControl is TrackBar))
            {
                lBMacroOn.Visible = false;
                string extras = GetExtras();
                KeyValuePair<object, string> tag = new KeyValuePair<object, string>(e.KeyValue, extras);
                newaction = true;
                if (ops != null)
                    ops.ChangeButtonText(keyname, tag);
                else if (sA != null)
                {
                    button.Text = e.KeyCode.ToString();
                    button.Tag = e.KeyValue;
                    button.ForeColor = Color.Black;
                }
                this.Close();
            }
        }

        private void Key_Press_Action(object sender, KeyEventArgs e)
        {
            if (rb == null && !(ActiveControl is NumericUpDown) && !(ActiveControl is TrackBar))
            {
                lBMacroOn.Visible = false;
                string extras = GetExtras();
                KeyValuePair<object, string> tag = new KeyValuePair<object, string>(e.KeyValue, extras);
                newaction = true;
                if (ops != null)
                    ops.ChangeButtonText(keyname, tag);
                else if (sA != null)
                {
                    button.Text = e.KeyCode.ToString();
                    button.Tag = e.KeyValue;
                    button.ForeColor = Color.Black;
                }
                this.Close();
            }
        }

        private void cbToggle_CheckedChanged(object sender, EventArgs e)
        {
         
        }

        private void btnMacro_Click(object sender, EventArgs e)
        {
            gBExtras.Controls.Add(cBScanCode);
            cBScanCode.Location = new Point(20, 300);
            rb = new RecordBox(this);
            rb.TopLevel = false;
            rb.Dock = DockStyle.Fill;
            rb.Visible = true;
            Controls.Add(rb);
            rb.BringToFront();
            rb.FormClosed += delegate { Controls.Add(cBScanCode); cBScanCode.Location = new Point(663, 8); ActiveControl = lBMacroOn; rb = null; };
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

        private int alphacolor;
        private Color reg, full;
        int bgc = 240; //Color of the form background, If greyscale color
        private void redBar_ValueChanged(object sender, EventArgs e)
        {
            cBLightbar.Checked = true;
            int value = ((TrackBar)sender).Value;
            int sat = bgc - (value < bgc ? value : bgc);
            int som = bgc + 11 * (int)(value * 0.0039215);
            ((TrackBar)sender).BackColor = Color.FromArgb(som, sat, sat);
            alphacolor = Math.Max(tBRedBar.Value, Math.Max(tBGreenBar.Value, tBBlueBar.Value));
            reg = Color.FromArgb(tBRedBar.Value, tBGreenBar.Value, tBBlueBar.Value);
            full = HuetoRGB(reg.GetHue(), reg.GetBrightness(), reg);
            bnColor.BackColor = Color.FromArgb((alphacolor > 205 ? 255 : (alphacolor + 50)), full);
            Global.ShiftColor[device] = new DS4Color((byte)tBRedBar.Value, (byte)tBGreenBar.Value, (byte)tBBlueBar.Value);
            lbRedV.Text = ((TrackBar)sender).Value.ToString();
        }
        private void greenBar_ValueChanged(object sender, EventArgs e)
        {
            cBLightbar.Checked = true;
            int value = ((TrackBar)sender).Value;
            int sat = bgc - (value < bgc ? value : bgc);
            int som = bgc + 11 * (int)(value * 0.0039215);
            ((TrackBar)sender).BackColor = Color.FromArgb(sat, som, sat);
            alphacolor = Math.Max(tBRedBar.Value, Math.Max(tBGreenBar.Value, tBBlueBar.Value));
            reg = Color.FromArgb(tBRedBar.Value, tBGreenBar.Value, tBBlueBar.Value);
            full = HuetoRGB(reg.GetHue(), reg.GetBrightness(), reg);
            bnColor.BackColor = Color.FromArgb((alphacolor > 205 ? 255 : (alphacolor + 50)), full);
            Global.ShiftColor[device] = new DS4Color((byte)tBRedBar.Value, (byte)tBGreenBar.Value, (byte)tBBlueBar.Value);
            lbGreenV.Text = ((TrackBar)sender).Value.ToString();
        }
        private void blueBar_ValueChanged(object sender, EventArgs e)
        {
            cBLightbar.Checked = true;
            int value = ((TrackBar)sender).Value;
            int sat = bgc - (value < bgc ? value : bgc);
            int som = bgc + 11 * (int)(value * 0.0039215);
            ((TrackBar)sender).BackColor = Color.FromArgb(sat, sat, som);
            alphacolor = Math.Max(tBRedBar.Value, Math.Max(tBGreenBar.Value, tBBlueBar.Value));
            reg = Color.FromArgb(tBRedBar.Value, tBGreenBar.Value, tBBlueBar.Value);
            full = HuetoRGB(reg.GetHue(), reg.GetBrightness(), reg);
            bnColor.BackColor = Color.FromArgb((alphacolor > 205 ? 255 : (alphacolor + 50)), full);
            Global.ShiftColor[device] = new DS4Color((byte)tBRedBar.Value, (byte)tBGreenBar.Value, (byte)tBBlueBar.Value);
            lbBlueV.Text = ((TrackBar)sender).Value.ToString();
        }



        public Color HuetoRGB(float hue, float light, Color rgb)
        {
            float L = (float)Math.Max(.5, light);
            float C = (1 - Math.Abs(2 * L - 1));
            float X = (C * (1 - Math.Abs((hue / 60) % 2 - 1)));
            float m = L - C / 2;
            float R = 0, G = 0, B = 0;
            if (light == 1) return Color.FromName("White");
            else if (rgb.R == rgb.G && rgb.G == rgb.B) return Color.FromName("White");
            else if (0 <= hue && hue < 60) { R = C; G = X; }
            else if (60 <= hue && hue < 120) { R = X; G = C; }
            else if (120 <= hue && hue < 180) { G = C; B = X; }
            else if (180 <= hue && hue < 240) { G = X; B = C; }
            else if (240 <= hue && hue < 300) { R = X; B = C; }
            else if (300 <= hue && hue < 360) { R = C; B = X; }
            return Color.FromArgb((int)((R + m) * 255), (int)((G + m) * 255), (int)((B + m) * 255));
        }

        private void bnColor_Click(object sender, EventArgs e)
        {
            advColorDialog.Color = bnColor.BackColor;
            if (advColorDialog.ShowDialog() == DialogResult.OK)
            {
                cBLightbar.Checked = true;
                bnColor.BackColor = advColorDialog.Color;
                tBRedBar.Value = advColorDialog.Color.R;
                tBGreenBar.Value = advColorDialog.Color.G;
                tBBlueBar.Value = advColorDialog.Color.B;
            }
            if (device < 4)
                DS4LightBar.forcelight[device] = false;
        }

        private void advColorDialog_OnUpdateColor(object sender, EventArgs e)
        {
            if (sender is Color && device < 4)
            {
                Color color = (Color)sender;
                DS4Color dcolor = new DS4Color { red = color.R, green = color.G, blue = color.B };
                DS4LightBar.forcedColor[device] = dcolor;
                DS4LightBar.forcedFlash[device] = 0;
                DS4LightBar.forcelight[device] = true;
            }
        }

        private void bnTest_Click(object sender, EventArgs e)
        {
            if (device < 4)
                if (((Button)sender).Text == Properties.Resources.TestText)
                {
                    Program.rootHub.setRumble((byte)nUDHeavy.Value, (byte)nUDLight.Value, device);
                    ((Button)sender).Text = Properties.Resources.StopText;
                }
                else
                {
                    Program.rootHub.setRumble(0, 0, device);
                    ((Button)sender).Text = Properties.Resources.TestText;
                }
            else
                if (((Button)sender).Text == Properties.Resources.TestText)
                {
                    Program.rootHub.setRumble((byte)nUDHeavy.Value, (byte)nUDLight.Value, 0);
                    ((Button)sender).Text = Properties.Resources.StopText;
                }
                else
                {
                    Program.rootHub.setRumble(0, 0, 0);
                    ((Button)sender).Text = Properties.Resources.TestText;
                }
        }

        private void nUD_ValueChanged(object sender, EventArgs e)
        {
            if (bnTest.Text != Properties.Resources.TestText)
            {
                if (device < 4)
                    Program.rootHub.setRumble((byte)nUDHeavy.Value, (byte)nUDLight.Value, device);
                else
                    Program.rootHub.setRumble((byte)nUDHeavy.Value, (byte)nUDLight.Value, 0);
            }
        }

        private void nUDMouse_ValueChanged(object sender, EventArgs e)
        {
            cBMouse.Checked = true;
        }
    }
}
