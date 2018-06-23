using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace DS4Windows
{
    public partial class KBM360 : Form
    {
        private int device;
        private Button button;
        private Options ops;
        private SpecActions sA;
        private Point oldSC;
        public List<string> macros = new List<string>();
        public List<int> macrostag = new List<int>();
        public bool macrorepeat, newaction;
        RecordBox rb;
        string defaultText;
        string guideText;
        bool loading = true;
        private int alphacolor;
        private Color reg, full;
        int bgc = 240; // Color of the form background, If greyscale color
        private bool extraChanged;
        private Bitmap pnl360BgImage;

        public KBM360(int deviceNum, Options ooo, Button buton)
        {
            InitializeComponent();
            pnl360BgImage = (Bitmap)Properties.Resources._360_map.Clone();
            pnl360Controls.BackgroundImage = null;
            pnl360Controls.BackgroundImageLayout = ImageLayout.None;
            device = deviceNum;
            ops = ooo;
            button = buton;
            DS4ControlSettings dcs = Global.getDS4CSetting(device, button.Name);
            cBToggle.Checked = dcs.keyType.HasFlag(DS4KeyType.Toggle);
            cBScanCode.Checked = dcs.keyType.HasFlag(DS4KeyType.ScanCode);
            oldSC = cBScanCode.Location;
            defaultText = btnDefault.Text;
            if (button.Name.StartsWith("bnShift"))
            {
                Console.Write("shift");
                Text = Properties.Resources.SelectActionTitle.Replace("*action*", button.Name.Substring(7));
                btnDefault.Text = Properties.Resources.FallBack;
            }
            else if (button.Name.StartsWith("bn"))
            {
                Text = Properties.Resources.SelectActionTitle.Replace("*action*", button.Name.Substring(2));
            }

            foreach (Control control in Controls)
            {
                if (control is Button)
                    ((Button)control).Click += anybtn_Click;
            }

            foreach (Control control in pnl360Controls.Controls)
            {
                if (control is Button)
                    ((Button)control).Click += anybtn_Click;
            }

            if (button.Name.Contains("Touch") || button.Name.Contains("Swipe"))
            {
                btnMOUSEDOWN.Visible = false;
                btnMOUSELEFT.Visible = false;
                btnMOUSERIGHT.Visible = false;
                btnMOUSEUP.Visible = false;
            }

            ActiveControl = lBMacroOn;
            guideText = btnGuide.Text;
            btnGuide.Text = "";
            cBShiftButton.SelectedIndex = Global.GetDS4STrigger(device, button.Name);
            cBShiftButton.Items[26] = ops.fingerOnTouchpadToolStripMenuItem.Text;
            rBRegular.Checked = true;
            loading = false;
        }

        public KBM360(SpecActions ooo, Button buton, bool extras)
        {
            InitializeComponent();
            sA = ooo;
            button = buton;
            Size = new Size(btnVolUp.Location.X + btnVolUp.Size.Width * 2,
                btnNUMENTER.Location.Y + btnNUMENTER.Size.Height * 2);

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
            X360Label.Visible = false;
            Text = Properties.Resources.SelectActionTitle.Replace("*action*", "Trigger");
            foreach (Control control in Controls)
            {
                if (control is Button)
                    ((Button)control).Click += anybtn_Click;
            }

            btnMOUSEDOWN.Visible = false;
            btnMOUSELEFT.Visible = false;
            btnMOUSERIGHT.Visible = false;
            btnMOUSEUP.Visible = false;
            rBRegular.Visible = false;
            rBShiftModifer.Visible = false;
            pBMouse.Visible = false;
            btnLEFTMOUSE.Visible = false;
            btn4THMOUSE.Visible = false;
            pnl360Controls.Visible = false;
            ActiveControl = lBMacroOn;
            btnGuide.Text = "";
        }

        public void anybtn_Click(object sender, EventArgs e)
        {
            if (rb == null && sender is Button && ((Button)sender).Name != "bnMacro" && ((Button)sender).Name != "bnTest")
            {
                Button bn = ((Button)sender);
                macrostag.Clear();
                string keyname;
                ushort val;
                /*if (((Button)sender).Text.Contains('↑') || ((Button)sender).Text.Contains('↓') || ((Button)sender).Text.Contains('→') || ((Button)sender).Text.Contains('←') || ((Button)sender).Text.Contains('Ø'))
                    keyname = ((Button)sender).Tag.ToString();
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
                else */
                if (bn.Tag == null)
                    keyname = bn.Text;
                else if (bn.Tag.ToString().Contains("X360"))
                    keyname = bn.Tag.ToString().Substring(4);
                else if (bn.Tag != null && ushort.TryParse(bn.Tag.ToString(), out val))
                    keyname = ((Keys)val).ToString();
                else
                    keyname = bn.Tag.ToString();

                object keytag;
                //ushort val;
                if (bn.Tag != null && bn.Tag.ToString().Contains("X360"))
                {
                    //keytag = ((Button)sender).Tag.ToString().Substring(4);
                    keytag = Global.getX360ControlsByName(bn.Tag.ToString().Substring(4));
                    DS4Controls psButton = Global.getDS4ControlsByName(button.Name);
                    if ((X360Controls)keytag == Global.getDefaultX360ControlBinding(psButton) &&
                        !cBScanCode.Checked && !cBToggle.Checked && !rBShiftModifer.Checked)
                    {
                        // Reset action
                        keytag = null;
                    }
                }
                else if (bn.Tag != null && ushort.TryParse(bn.Tag.ToString(), out val))
                    keytag = val;
                else
                    keytag = bn.Tag;

                lBMacroOn.Visible = false;
                string extras = null;
                if (IsUsingExtras())
                {
                    extras = GetExtras();
                }

                KeyValuePair<object, string> tag = new KeyValuePair<object, string>(keytag, extras);

                newaction = true;
                int value;
                bool tagisint = keytag != null && int.TryParse(keytag.ToString(), out value);
                bool scanavail = tagisint;
                bool toggleavil = tagisint;
                if (ops != null)
                {
                    ops.ChangeButtonText(button, rBShiftModifer.Checked, tag,
                        (scanavail ? cBScanCode.Checked : false), (toggleavil ? cBToggle.Checked : false),
                        false, false, cBShiftButton.SelectedIndex);
                }
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

        private bool IsUsingExtras()
        {
            bool result = false;
            result = result || (nUDHeavy.Value != 0);
            result = result || (nUDLight.Value != 0);
            result = result || (cBLightbar.Checked);
            result = result ||
                (tBRedBar.Value != 255 && tBGreenBar.Value != 255 &&
                tBBlueBar.Value != 255);

            result = result || (nUDLightFlash.Value != 0);
            result = result || (cBMouse.Checked);
            result = result || (nUDMouse.Value != 25);
            return result;
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
                    string extras = null;
                    if (IsUsingExtras())
                    {
                        extras = GetExtras();
                    }

                    KeyValuePair<object, string> tag = new KeyValuePair<object, string>(macrostag.ToArray(), extras);
                    ops.ChangeButtonText(button, rBShiftModifer.Checked, tag, cBScanCode.Checked, false, lBMacroOn.Visible, macrorepeat, cBShiftButton.SelectedIndex);
                }
                else if (!newaction)
                {
                    string extras = null;
                    if (IsUsingExtras())
                    {
                        extras = GetExtras();
                    }

                    int value;
                    object tt = Global.GetDS4Action(device, button.Name, rBShiftModifer.Checked);
                    bool tagisint = tt != null
                        && int.TryParse(tt.ToString(), out value);
                    bool scanavail = tagisint;
                    bool toggleavil = tagisint;
                    KeyValuePair<object, string> tag;
                    if (tt is X360Controls)
                        tag = new KeyValuePair<object, string>(getX360ControlsByName((X360Controls)tt), extras);
                    else
                        tag = new KeyValuePair<object, string>(tt, extras);
                    ops.ChangeButtonText(button, rBShiftModifer.Checked, tag, (scanavail ? cBScanCode.Checked : false), (toggleavil ? cBToggle.Checked : false), lBMacroOn.Visible, macrorepeat, cBShiftButton.SelectedIndex);
                }
                //ops.Toggle_Bn((scanavail ? cBScanCode.Checked : false), (toggleavil ? cBToggle.Checked : false), lBMacroOn.Visible, macrorepeat);
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
                string extras = null;
                if (IsUsingExtras())
                {
                    extras = GetExtras();
                }

                KeyValuePair<object, string> tag = new KeyValuePair<object, string>(e.KeyValue, extras);
                newaction = true;
                if (ops != null)
                {
                    ops.ChangeButtonText(button, rBShiftModifer.Checked, tag,
                        cBScanCode.Checked, cBToggle.Checked, false, false, cBShiftButton.SelectedIndex);
                }
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
                string extras = null;
                if (IsUsingExtras())
                {
                    extras = GetExtras();
                }

                KeyValuePair<object, string> tag = new KeyValuePair<object, string>(e.KeyValue, extras);
                newaction = true;
                if (ops != null)
                {
                    ops.ChangeButtonText(button, rBShiftModifer.Checked, tag,
                        cBScanCode.Checked, cBToggle.Checked, false, false, cBShiftButton.SelectedIndex);
                }
                else if (sA != null)
                {
                    button.Text = e.KeyCode.ToString();
                    button.Tag = e.KeyValue;
                    button.ForeColor = Color.Black;
                }

                this.Close();
            }
        }

        private void btnMacro_Click(object sender, EventArgs e)
        {
            gBExtras.Controls.Add(cBScanCode);
            cBScanCode.Location = new Point(lBTip.Location.X, lBTip.Location.Y + lBTip.Size.Height);
            rb = new RecordBox(this);
            rb.TopLevel = false;
            rb.Dock = DockStyle.Fill;
            rb.Visible = true;
            Controls.Add(rb);
            rb.BringToFront();
            rb.FormClosed += delegate { Controls.Add(cBScanCode); cBScanCode.Location = oldSC; ActiveControl = lBMacroOn; rb = null; };
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

                case (Keys.Shift | Keys.Right):
                case (Keys.Shift | Keys.Left):
                case (Keys.Shift | Keys.Up):
                case (Keys.Shift | Keys.Down):
                    return true;

                default: break;
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
                {
                    if (e.Shift)
                    {

                    }
                    else
                    {
                    }
                    break;
                }
                default: break;
            }
        }

        private void SetColorToolTip(object sender, EventArgs e)
        {
            TrackBar tb = (TrackBar)sender;
            cBLightbar.Checked = true;
            if (tb != null)
            {
                int value = tb.Value;
                int sat = bgc - (value < bgc ? value : bgc);
                int som = bgc + 11 * (int)(value * 0.0039215);
                tb.BackColor = Color.FromArgb(tb.Name.ToLower().Contains("red") ? som : sat, tb.Name.ToLower().Contains("green") ? som : sat, tb.Name.ToLower().Contains("blue") ? som : sat);
            }

            alphacolor = Math.Max(tBRedBar.Value, Math.Max(tBGreenBar.Value, tBBlueBar.Value));
            reg = Color.FromArgb(tBRedBar.Value, tBGreenBar.Value, tBBlueBar.Value);
            full = HuetoRGB(reg.GetHue(), reg.GetBrightness(), reg);
            bnColor.BackColor = Color.FromArgb((alphacolor > 205 ? 255 : (alphacolor + 50)), full);
            ((Label)gBExtras.Controls.Find("lb" + tb.Name.Substring(2, tb.Name.Length - 5) + "V", true)[0]).Text = tb.Value.ToString();
            extraChanged = true;
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
                extraChanged = true;
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

        private void Hightlight_Hover(object sender, EventArgs e)
        {
            pBHighlight.Visible = true;
            lb360Tip.Visible = true;
            Control c = (Control)sender;
            Size s = c.Size;
            Size s2 = pBHighlight.Size;
            Point l = c.Location;
            pBHighlight.Location = new Point(l.X + s.Width / 2 - s2.Width / 2, l.Y + s.Height / 2 - s2.Height / 2);
            Point l2 = pBHighlight.Location;
            lb360Tip.Text = X360ControlName(c.Name.Substring(3));
            lb360Tip.Location = new Point(l2.X + s2.Width / 2 - lb360Tip.Width / 2, l2.Y - 20);
        }

        private string X360ControlName(string v)
        {
            string s = v;
            for (int i = s.Length - 1; i > (s.StartsWith("L") || s.StartsWith("R") ? 1 : 0); i--)
            {
                if (s[i] >= 'A' && s[i] <= 'Z')
                    s = s.Insert(i, " ");
            }

            if (s == "Guide")
                s = guideText;

            return s;
        }

        private void Highlight_Leave(object sender, EventArgs e)
        {
            pBHighlight.Visible = false;
            lb360Tip.Visible = false;
        }

        private void rBShift_CheckedChanged(object sender, EventArgs e)
        {
            if (!loading && extraChanged)
            {
                string strextras = null;
                if (IsUsingExtras())
                {
                    strextras = GetExtras();
                }

                Global.UpdateDS4Extra(device, button.Name, !rBShiftModifer.Checked, strextras);
            }

            object tagO = Global.GetDS4Action(device, button.Name, rBShiftModifer.Checked);
            if (rBShiftModifer.Checked)
                btnDefault.Text = Properties.Resources.FallBack;
            else
                btnDefault.Text = defaultText;

            cBShiftButton.Visible = rBShiftModifer.Checked;
            macrostag.Clear();
            lBMacroOn.Visible = false;
            newaction = false;
            Highlight_Leave(null, null);
            foreach (Control control in Controls)
            {
                if (control is Button)
                    ((Button)control).BackColor = SystemColors.Control;
            }

            if (tagO != null)
            {
                if (tagO is int || tagO is ushort)
                {
                    int tag = int.Parse(tagO.ToString());
                    int i;
                    foreach (Control control in Controls)
                    {
                        if (control is Button)
                        {
                            if (int.TryParse(control.Tag?.ToString(), out i) && i == tag)
                            {
                                ((Button)control).BackColor = Color.LightGreen;
                                break;
                            }
                        }
                    }
                }
                else if (tagO is int[])
                {
                    int[] tag = (int[])tagO;
                    lBMacroOn.Visible = true;
                    foreach (int i in tag)
                        macrostag.Add(i);

                    if (Global.GetDS4KeyType(device, button.Name, rBShiftModifer.Checked).HasFlag(DS4KeyType.HoldMacro))
                        macrorepeat = true;
                }
                else if (tagO is string || tagO is X360Controls)
                {
                    string tag;
                    if (tagO is X360Controls)
                        tag = getX360ControlsByName((X360Controls)tagO);
                    else
                        tag = tagO.ToString();

                    foreach (Control control in Controls)
                    {
                        if (control is Button)
                        {
                            if (control.Tag?.ToString() == tag)
                            {
                                ((Button)control).BackColor = Color.LightGreen;
                                break;
                            }
                        }
                    }

                    foreach (Control control in pnl360Controls.Controls)
                    {
                        if (control is Button)
                        {
                            if (control.Tag?.ToString().Substring(4) == tag)
                            {
                                Hightlight_Hover(((Button)control), null);
                                break;
                            }
                        }
                    }
                }
            }
            else
            {
                btnDefault.BackColor = Color.LightGreen;
                if (rBRegular.Checked && ops.defaults.ContainsKey(button.Name))
                {
                    tagO = ops.defaults[button.Name];
                    string tag;
                    if (tagO is X360Controls)
                        tag = getX360ControlsByName((X360Controls)tagO);
                    else
                        tag = tagO.ToString();

                    foreach (Control control in Controls)
                    {
                        if (control is Button)
                        {
                            if (control.Tag != null && control.Tag.ToString().Contains("X360") ? control.Tag?.ToString().Substring(4) == tag : control.Tag?.ToString() == tag)
                            {
                                ((Button)control).BackColor = Color.LightGreen;
                                break;
                            }
                        }
                    }
   
                    foreach (Control control in pnl360Controls.Controls)
                    {
                        if (control is Button)
                        {
                            if (control.Tag?.ToString().Substring(4) == tag)
                            {
                                Hightlight_Hover(((Button)control), null);
                                break;
                            }
                        }
                    }
                }
            }

            string dcExtras = Global.GetDS4Extra(device, button.Name, rBShiftModifer.Checked);
            string[] extras = null;
            if (!string.IsNullOrEmpty(dcExtras))
            {
                extras = dcExtras.Split(',');
            }

            if (extras != null)
            {
                int b;
                try
                {
                    if (int.TryParse(extras[0], out b)) nUDHeavy.Value = b;
                    if (int.TryParse(extras[1], out b)) nUDLight.Value = b;
                    if (int.TryParse(extras[2], out b))
                    {
                        if (b == 1)
                        {
                            cBLightbar.Checked = true;
                            if (int.TryParse(extras[3], out b)) tBRedBar.Value = b;
                            if (int.TryParse(extras[4], out b)) tBGreenBar.Value = b;
                            if (int.TryParse(extras[5], out b)) tBBlueBar.Value = b;
                            if (int.TryParse(extras[6], out b)) nUDLightFlash.Value = b;
                        }
                        else
                        {

                            tBRedBar.Value = 255;
                            tBGreenBar.Value = 255;
                            tBBlueBar.Value = 255;
                            nUDLightFlash.Value = 0;
                            cBLightbar.Checked = false;
                        }
                    }

                    if (int.TryParse(extras[7], out b))
                    {
                        if (b == 1)
                        {
                            cBMouse.Checked = true;
                            if (int.TryParse(extras[8], out b)) nUDMouse.Value = b;
                        }
                        else
                        {
                            nUDMouse.Value = 25;
                            cBMouse.Checked = false;
                        }
                    }
                }
                catch
                {
                    nUDHeavy.Value = 0;
                    nUDLight.Value = 0;
                    tBRedBar.Value = 255;
                    tBGreenBar.Value = 255;
                    tBBlueBar.Value = 255;
                    nUDLightFlash.Value = 0;
                    cBLightbar.Checked = false;
                    nUDMouse.Value = 25;
                    cBMouse.Checked = false;
                }
            }

            extraChanged = false;
        }

        public static string getX360ControlsByName(X360Controls key)
        {
            switch (key)
            {
                case X360Controls.Back: return "Back";
                case X360Controls.LS: return "Left Stick";
                case X360Controls.RS: return "Right Stick";
                case X360Controls.Start: return "Start";
                case X360Controls.DpadUp: return "Up Button";
                case X360Controls.DpadRight: return "Right Button";
                case X360Controls.DpadDown: return "Down Button";
                case X360Controls.DpadLeft: return "Left Button";

                case X360Controls.LB: return "Left Bumper";
                case X360Controls.RB: return "Right Bumper";
                case X360Controls.Y: return "Y Button";
                case X360Controls.B: return "B Button";
                case X360Controls.A: return "A Button";
                case X360Controls.X: return "X Button";

                case X360Controls.Guide: return "Guide";
                case X360Controls.LXNeg: return "Left X-Axis-";
                case X360Controls.LYNeg: return "Left Y-Axis-";
                case X360Controls.RXNeg: return "Right X-Axis-";
                case X360Controls.RYNeg: return "Right Y-Axis-";

                case X360Controls.LXPos: return "Left X-Axis+";
                case X360Controls.LYPos: return "Left Y-Axis+";
                case X360Controls.RXPos: return "Right X-Axis+";
                case X360Controls.RYPos: return "Right Y-Axis+";
                case X360Controls.LT: return "Left Trigger";
                case X360Controls.RT: return "Right Trigger";

                case X360Controls.LeftMouse: return "Left Mouse Button";
                case X360Controls.RightMouse: return "Right Mouse Button";
                case X360Controls.MiddleMouse: return "Middle Mouse Button";
                case X360Controls.FourthMouse: return "4th Mouse Button";
                case X360Controls.FifthMouse: return "5th Mouse Button";
                case X360Controls.WUP: return "Mouse Wheel Up";
                case X360Controls.WDOWN: return "Mouse Wheel Down";
                case X360Controls.MouseUp: return "Mouse Up";
                case X360Controls.MouseDown: return "Mouse Down";
                case X360Controls.MouseLeft: return "Mouse Left";
                case X360Controls.MouseRight: return "Mouse Right";
                case X360Controls.Unbound: return "Unbound";
                default: break;
            }

            return "Unbound";
        }

        private void bnTest_Click(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            if (device < 4)
            {
                if (btn.Text == Properties.Resources.TestText)
                {
                    Program.rootHub.setRumble((byte)nUDHeavy.Value, (byte)nUDLight.Value, device);
                    btn.Text = Properties.Resources.StopText;
                }
                else
                {
                    Program.rootHub.setRumble(0, 0, device);
                    btn.Text = Properties.Resources.TestText;
                }
            }
            else
            {
                if (btn.Text == Properties.Resources.TestText)
                {
                    Program.rootHub.setRumble((byte)nUDHeavy.Value, (byte)nUDLight.Value, 0);
                    btn.Text = Properties.Resources.StopText;
                }
                else
                {
                    Program.rootHub.setRumble(0, 0, 0);
                    btn.Text = Properties.Resources.TestText;
                }
            }
        }

        private void ExtraChanged(object sender, EventArgs e)
        {
            extraChanged = true;
        }

        private void pnl360Controls_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.DrawImage(pnl360BgImage, new Rectangle(0, 13, Convert.ToInt32(pnl360Controls.Width), Convert.ToInt32(pnl360Controls.Height - 26)), new Rectangle(0, 0, Convert.ToInt32(pnl360BgImage.Width), Convert.ToInt32(pnl360BgImage.Height)), GraphicsUnit.Pixel);
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

            extraChanged = true;
        }

        private void nUDMouse_ValueChanged(object sender, EventArgs e)
        {
            cBMouse.Checked = true;
            extraChanged = true;
        }
    }
}
