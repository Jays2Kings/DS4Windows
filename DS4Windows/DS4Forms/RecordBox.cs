using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using NonFormTimer = System.Timers.Timer;

using System.IO;
using System.Reflection;

namespace DS4Windows
{
    public partial class RecordBox : Form
    {
        Stopwatch sw = new Stopwatch();
        //Timer ds4 = new Timer();
        NonFormTimer ds4 = new NonFormTimer();
        public List<int> macros = new List<int>(), macrosAfter = new List<int>();
        public List<string> macronames = new List<string>();
        SpecActions sA;
        int sAButton = -1;
        KBM360 kbm;
        DS4State cState;
        public bool saved = false;
        List<DS4Controls> dcs = new List<DS4Controls>();
        TextBox tb1, tb2;
        public RecordBox(KBM360 op)
        {
            kbm = op;
            InitializeComponent();
            openPresets.Filter = Properties.Resources.TextDocs + "|*.txt";
            savePresets.Filter = Properties.Resources.TextDocs + "|*.txt";
            if (op != null)
            {
                if (kbm.macrorepeat)
                    cBStyle.SelectedIndex = 1;
                else
                    cBStyle.SelectedIndex = 0;
            }

            AddtoDS4List();
            //ds4.Tick += ds4_Tick;
            ds4.Elapsed += Ds4_Tick;
            ds4.Interval = 1;
            if (kbm.macrostag.Count > 0)
            {
                macros.AddRange(kbm.macrostag);
                LoadMacro();
                saved = true;
            }
        }

        public RecordBox(SpecActions op, int button = -1)
        {
            sA = op;
            InitializeComponent();
            if (sA.macrorepeat)
                cBStyle.SelectedIndex = 1;
            else
                cBStyle.SelectedIndex = 0;

            AddtoDS4List();
            if (button > -1)
                sAButton = button;

            //ds4.Tick += ds4_Tick;
            ds4.Elapsed += Ds4_Tick;
            ds4.Interval = 1;
            lbRecordTip.Visible = false;
            cBStyle.Visible = false;
            pnlMouseButtons.Location = new Point(pnlMouseButtons.Location.X, pnlMouseButtons.Location.Y - 75);
            if (sAButton > -1)
            {
                if (sA.multiMacrostag[sAButton].Count > 0)
                {
                    macros.AddRange(sA.multiMacrostag[sAButton]);
                    LoadMacro();
                    saved = true;
                }
            }
            else if (sA.macrostag.Count > 0)
            {
                macros.AddRange(sA.macrostag);
                LoadMacro();
                saved = true;
            }
        }

        void AddtoDS4List()
        {
            dcs.Add(DS4Controls.Cross);
            dcs.Add(DS4Controls.Circle);
            dcs.Add(DS4Controls.Square);
            dcs.Add(DS4Controls.Triangle);
            dcs.Add(DS4Controls.Options);
            dcs.Add(DS4Controls.Share);
            dcs.Add(DS4Controls.DpadUp);
            dcs.Add(DS4Controls.DpadDown);
            dcs.Add(DS4Controls.DpadLeft);
            dcs.Add(DS4Controls.DpadRight);
            dcs.Add(DS4Controls.PS);
            dcs.Add(DS4Controls.L1);
            dcs.Add(DS4Controls.R1);
            dcs.Add(DS4Controls.L2);
            dcs.Add(DS4Controls.R2);
            dcs.Add(DS4Controls.L3);
            dcs.Add(DS4Controls.R3);
            dcs.Add(DS4Controls.LXPos);
            dcs.Add(DS4Controls.LXNeg);
            dcs.Add(DS4Controls.LYPos);
            dcs.Add(DS4Controls.LYNeg);
            dcs.Add(DS4Controls.RXPos);
            dcs.Add(DS4Controls.RXNeg);
            dcs.Add(DS4Controls.RYPos);
            dcs.Add(DS4Controls.RYNeg);
        }

        void AddMacroValue(int value)
        {
            if (recordAfter)
                macrosAfter.Add(value);
            else
                macros.Add(value);
        }

        bool[] pTP = new bool[4];

        private void Ds4_Tick(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (Program.rootHub.DS4Controllers[0] != null)
            {
                cState = Program.rootHub.getDS4State(0);
                this.BeginInvoke((Action)(() =>
                {
                    if (btnRecord.Text == Properties.Resources.StopText)
                    {
                        if (cBRecordDelays.Checked)
                        {
                            Mouse tP = Program.rootHub.touchPad[0];
                            if (tP.leftDown && !pTP[0])
                                if (!btnRumble.Text.Contains("Stop"))
                                    btnRumble_Click(sender, e);
                                else if (!tP.leftDown && pTP[0])
                                    if (btnRumble.Text.Contains("Stop"))
                                        btnRumble_Click(sender, e);
                            if (tP.rightDown && !pTP[1])
                                if (!btnLightbar.Text.Contains("Reset"))
                                    btnLightbar_Click(sender, e);
                                else if (!tP.rightDown && pTP[1])
                                    if (btnLightbar.Text.Contains("Reset"))
                                        btnLightbar_Click(sender, e);
                            pTP[0] = tP.leftDown;
                            pTP[1] = tP.rightDown;
                        }

                        //foreach (DS4Controls dc in dcs)
                        for (int controlIndex = 0, dcsLen = dcs.Count; controlIndex < dcsLen; controlIndex++)
                        {
                            DS4Controls dc = dcs[controlIndex];
                            if (Mapping.getBoolMapping(0, dc, cState, null, null))
                            {
                                int value = DS4ControltoInt(dc);
                                int count = 0;
                                int macroLen = macros.Count;
                                //foreach (int i in macros)
                                for (int macroIndex = 0; macroIndex < macroLen; macroIndex++)
                                {
                                    int i = macros[macroIndex];
                                    if (i == value)
                                        count++;
                                }

                                if (macroLen == 0)
                                {
                                    AddMacroValue(value);
                                    lVMacros.Items.Add(DS4ControltoX360(dc), 0);
                                    if (cBRecordDelays.Checked)
                                    {
                                        sw.Reset();
                                        sw.Start();
                                    }
                                }
                                else if (count % 2 == 0)
                                {
                                    if (cBRecordDelays.Checked)
                                    {
                                        AddMacroValue((int)sw.ElapsedMilliseconds + 300);
                                        lVMacros.Items.Add(Properties.Resources.WaitMS.Replace("*number*", sw.ElapsedMilliseconds.ToString()).Replace("*ms*", "ms"), 2);
                                        sw.Reset();
                                        sw.Start();
                                    }
                                    AddMacroValue(value);
                                    lVMacros.Items.Add(DS4ControltoX360(dc), 0);
                                }

                                lVMacros.Items[lVMacros.Items.Count - 1].EnsureVisible();
                            }
                            else if (!Mapping.getBoolMapping(0, dc, cState, null, null))
                            {
                                int macroLen = macros.Count;
                                if (macroLen != 0)
                                {
                                    int value = DS4ControltoInt(dc);
                                    int count = 0;
                                    //foreach (int i in macros)
                                    for (int macroIndex = 0; macroIndex < macroLen; macroIndex++)
                                    {
                                        int i = macros[macroIndex];
                                        if (i == value)
                                            count++;
                                    }

                                    /*for (int i = macros.Count - 1; i >= 0; i--)
                                        if (macros.Count == 261)
                                            count++;*/

                                    if (count % 2 == 1)
                                    {
                                        if (cBRecordDelays.Checked)
                                        {
                                            AddMacroValue((int)sw.ElapsedMilliseconds + 300);
                                            lVMacros.Items.Add(Properties.Resources.WaitMS.Replace("*number*", sw.ElapsedMilliseconds.ToString()).Replace("*ms*", "ms"), 2);
                                            sw.Reset();
                                            sw.Start();
                                        }

                                        AddMacroValue(value);
                                        lVMacros.Items.Add(DS4ControltoX360(dc), 1);
                                        lVMacros.Items[lVMacros.Items.Count - 1].EnsureVisible();
                                    }
                                }
                            }
                        }
                    }
                }));
            }
        }
        
        public static int DS4ControltoInt(DS4Controls ctrl)
        {
            switch (ctrl)
            {
                case DS4Controls.Cross: return 261;
                case DS4Controls.Circle: return 262; 
                case DS4Controls.Square: return 263; 
                case DS4Controls.Triangle: return 264; 
                case DS4Controls.Options: return 265; 
                case DS4Controls.Share: return 266; 
                case DS4Controls.DpadUp: return 267; 
                case DS4Controls.DpadDown: return 268; 
                case DS4Controls.DpadLeft: return 269; 
                case DS4Controls.DpadRight: return 270; 
                case DS4Controls.PS: return 271; 
                case DS4Controls.L1: return 272; 
                case DS4Controls.R1: return 273; 
                case DS4Controls.L2: return 274; 
                case DS4Controls.R2: return 275; 
                case DS4Controls.L3: return 276;
                case DS4Controls.R3: return 277;
                case DS4Controls.LXPos: return 278;
                case DS4Controls.LXNeg: return 279;
                case DS4Controls.LYPos: return 280;
                case DS4Controls.LYNeg: return 281;
                case DS4Controls.RXPos: return 282;
                case DS4Controls.RXNeg: return 283;
                case DS4Controls.RYPos: return 284;
                case DS4Controls.RYNeg: return 285;
            }
            return 0;
        }

        public static string DS4ControltoX360(DS4Controls ctrl)
        {
            switch (ctrl)
            {
                case DS4Controls.Cross: return "A Button";
                case DS4Controls.Circle: return "B Button";
                case DS4Controls.Square: return "X Button";
                case DS4Controls.Triangle: return "Y Button";
                case DS4Controls.Options: return "Start";
                case DS4Controls.Share: return "Back";
                case DS4Controls.DpadUp: return "Up Button";
                case DS4Controls.DpadDown: return "Down Button";
                case DS4Controls.DpadLeft: return "Left Button";
                case DS4Controls.DpadRight: return "Right Button";
                case DS4Controls.PS: return "Guide";
                case DS4Controls.L1: return "Left Bumper";
                case DS4Controls.R1: return "Right Bumper";
                case DS4Controls.L2: return "Left Trigger";
                case DS4Controls.R2: return "Right Trigger";
                case DS4Controls.L3: return "Left Stick";
                case DS4Controls.R3: return "Right Stick";
                case DS4Controls.LXPos: return "LS Right";
                case DS4Controls.LXNeg: return "LS Left";
                case DS4Controls.LYPos: return "LS Down";
                case DS4Controls.LYNeg: return "LS Up";
                case DS4Controls.RXPos: return "RS Right";
                case DS4Controls.RXNeg: return "RS Left";
                case DS4Controls.RYPos: return "RS Down";
                case DS4Controls.RYNeg: return "RS Up";
            }

            return "None";
        }

        bool recordAfter = false;
        int recordAfterInt = 0;

        private void btnRecord_Click(object sender, EventArgs e)
        {
            if (btnRecord.Text != Properties.Resources.StopText)
            {
                if (cBRecordDelays.Checked)
                    sw.Start();

                btnRumble.Visible = cBRecordDelays.Checked;
                btnLightbar.Visible = cBRecordDelays.Checked;
                pBLtouch.Visible = cBRecordDelays.Checked;
                pBRtouch.Visible = cBRecordDelays.Checked;
                Program.rootHub.recordingMacro = true;
                saved = false;
                ds4.Start();
                if (!recordAfter)
                    macros.Clear();

                lVMacros.Items.Clear();
                btnRecord.Text = Properties.Resources.StopText;
                EnableControls(false);
                ActiveControl = null;
                lVMacros.Focus();
            }
            else
            {
                Program.rootHub.recordingMacro = false;
                ds4.Stop();
                if (recordAfter)
                {
                    lVMacros.Items.Clear();
                    macros.InsertRange(recordAfterInt, macrosAfter);
                    macrosAfter.Clear();
                    recordAfter = false;
                    LoadMacro();
                }

                if (btn4th.Text.Contains(Properties.Resources.UpText))
                    btn4th_Click(sender, e);

                if (btn5th.Text.Contains(Properties.Resources.UpText))
                    btn5th_Click(sender, e);

                if (cBRecordDelays.Checked)
                {
                    if (btnRumble.Text.Contains("Stop"))
                        btnRumble_Click(sender, e);

                    if (btnLightbar.Text.Contains("Reset"))
                        btnLightbar_Click(sender, e);
                }

                if (cBRecordDelays.Checked)
                    sw.Reset();

                if (cBRecordDelays.Checked)
                    lbDelayTip.Visible = true;

                btnRecord.Text = Properties.Resources.RecordText;
                EnableControls(true);                
            }
        }

        private void EnableControls(bool on)
        {
            lVMacros.Enabled = on;
            cBRecordDelays.Enabled = on;
            cBStyle.Enabled = on;
            pnlMouseButtons.Visible = !on;
        }

        [DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(Keys key);

        private void anyKeyDown(object sender, KeyEventArgs e)
        {
            if (btnRecord.Text == Properties.Resources.StopText)
            {
                int value = WhichKey(e, 0);
                int count = 0;
                if (recordAfter)
                {
                    foreach (int i in macrosAfter)
                    {
                        if (i == value)
                            count++;
                    }
                }
                else
                {
                    foreach (int i in macros)
                    {
                        if (i == value)
                            count++;
                    }
                }

                if (macros.Count == 0 || (recordAfter && macrosAfter.Count == 0))
                {
                    AddMacroValue(value);
                    lVMacros.Items.Add(((Keys)value).ToString(), 0);
                    if (cBRecordDelays.Checked)
                    {
                        sw.Reset();
                        sw.Start();
                    }
                }
                else if (count % 2 == 0)
                {
                    if (cBRecordDelays.Checked)
                    {
                        AddMacroValue((int)sw.ElapsedMilliseconds + 300);
                        lVMacros.Items.Add(Properties.Resources.WaitMS.Replace("*number*", sw.ElapsedMilliseconds.ToString()).Replace("*ms*", "ms"), 2);
                        sw.Reset();
                        sw.Start();
                    }

                    AddMacroValue(value);
                    lVMacros.Items.Add(((Keys)value).ToString(), 0);
                }

                lVMacros.Items[lVMacros.Items.Count - 1].EnsureVisible();
            }
            else if (e.KeyValue == 27)
            {
                Close();
            }
            else if (e.KeyCode == Keys.Delete)
            {
                if (lVMacros.SelectedIndices.Count > 0 && lVMacros.SelectedIndices[0] > -1)
                {
                    macros.RemoveAt(lVMacros.SelectedIndices[0]);
                    lVMacros.Items.Remove(lVMacros.SelectedItems[0]);
                }
            }
        }

        private int WhichKey(KeyEventArgs e, int keystate)
        {
            if (keystate == 1)
            {
                if (e.KeyCode == Keys.ShiftKey)
                {
                    if (recordAfter)
                    {
                        for (int i = macrosAfter.Count - 1; i >= 0; i--)
                        {
                            if (macrosAfter[i] == 160)
                                return 160;
                            else if (macrosAfter[i] == 161)
                                return 161;
                        }
                    }
                    else
                    {
                        for (int i = macros.Count - 1; i >= 0; i--)
                        {
                            if (macros[i] == 160)
                                return 160;
                            else if (macros[i] == 161)
                                return 161;
                        }
                    }
                }
                else if (e.KeyCode == Keys.ControlKey)
                {
                    if (recordAfter)
                    {
                        for (int i = macrosAfter.Count - 1; i >= 0; i--)
                        {
                            if (macrosAfter[i] == 162)
                                return 162;
                            else if (macrosAfter[i] == 163)
                                return 163;
                        }
                    }
                    else
                    {
                        for (int i = macros.Count - 1; i >= 0; i--)
                        {
                            if (macros[i] == 162)
                                return 162;
                            else if (macros[i] == 163)
                                return 163;
                        }
                    }
                }
                else if (e.KeyCode == Keys.Menu)
                {
                    if (recordAfter)
                    {
                        for (int i = macrosAfter.Count - 1; i >= 0; i--)
                        {
                            if (macrosAfter[i] == 164)
                                return 164;
                            else if (macrosAfter[i] == 165)
                                return 165;
                        }
                    }
                    else
                    {
                        for (int i = macros.Count - 1; i >= 0; i--)
                        {
                            if (macros[i] == 164)
                                return 164;
                            else if (macros[i] == 165)
                                return 165;
                        }
                    }
                }

                return e.KeyValue;
            }
            else
            {
                if (e.KeyCode == Keys.ShiftKey)
                {
                    if (Convert.ToBoolean(GetAsyncKeyState(Keys.LShiftKey)))
                        return 160;
                    if (Convert.ToBoolean(GetAsyncKeyState(Keys.RShiftKey)))
                        return 161;
                }
                else if (e.KeyCode == Keys.ControlKey)
                {
                    if (Convert.ToBoolean(GetAsyncKeyState(Keys.LControlKey)))
                        return 162;
                    if (Convert.ToBoolean(GetAsyncKeyState(Keys.RControlKey)))
                        return 163;
                }
                else if (e.KeyCode == Keys.Menu)
                {
                    e.Handled = true;
                    if (Convert.ToBoolean(GetAsyncKeyState(Keys.LMenu)))
                        return 164;
                    if (Convert.ToBoolean(GetAsyncKeyState(Keys.RMenu)))
                        return 165;
                }
            }

            return e.KeyValue;
        }

        private void AnyKeyUp(object sender, KeyEventArgs e)
        {
            if (btnRecord.Text == Properties.Resources.StopText && (macros.Count != 0 || (recordAfter && macrosAfter.Count != 0)))
            {
                lVMacros.BeginUpdate();
                int value = WhichKey(e, 1);
                if (cBRecordDelays.Checked)
                {
                    AddMacroValue((int)sw.ElapsedMilliseconds + 300);
                    lVMacros.Items.Add(Properties.Resources.WaitMS.Replace("*number*", sw.ElapsedMilliseconds.ToString()).Replace("*ms*", "ms"), 2);
                    sw.Reset();
                    sw.Start();
                }

                if (e.KeyCode == Keys.PrintScreen)
                {
                    int tempvalue = WhichKey(e, 0);
                    AddMacroValue(tempvalue);
                    lVMacros.Items.Add(((Keys)value).ToString(), 0);
                    lVMacros.Items[lVMacros.Items.Count - 1].EnsureVisible();
                }

                AddMacroValue(value);
                lVMacros.Items.Add(((Keys)value).ToString(), 1);
                lVMacros.Items[lVMacros.Items.Count - 1].EnsureVisible();
                lVMacros.EndUpdate();
            }
        }
        private void anyMouseDown(object sender, MouseEventArgs e)
        {
            if (btnRecord.Text == Properties.Resources.StopText)
            {
                int value;
                switch (e.Button)
                {
                    case MouseButtons.Left: value = 256; break;
                    case MouseButtons.Right: value = 257; break;
                    case MouseButtons.Middle: value = 258; break;
                    case MouseButtons.XButton1: value = 259; break;
                    case MouseButtons.XButton2: value = 260; break;
                    default: value = 0; break;
                }

                if (macros.Count == 0 || (recordAfter && macrosAfter.Count == 0))
                {
                    AddMacroValue(value);
                    lVMacros.Items.Add(e.Button.ToString() + " Mouse Button", 0);
                    if (cBRecordDelays.Checked)
                    {
                        sw.Reset();
                        sw.Start();
                    }
                }
                else if (macros.Count > 0 || (recordAfter && macrosAfter.Count > 0))
                {
                    if (cBRecordDelays.Checked)
                    {
                        AddMacroValue((int)sw.ElapsedMilliseconds + 300);
                        lVMacros.Items.Add(Properties.Resources.WaitMS.Replace("*number*", sw.ElapsedMilliseconds.ToString()).Replace("*ms*", "ms"), 2);
                        sw.Reset();
                        sw.Start();
                    }

                    AddMacroValue(value);
                    lVMacros.Items.Add(e.Button.ToString() + " Mouse Button", 0);
                }

                if (e.Button == MouseButtons.XButton1)
                    lVMacros.Items[lVMacros.Items.Count - 1].Text = "4th Mouse Button";

                if (e.Button == MouseButtons.XButton2)
                    lVMacros.Items[lVMacros.Items.Count - 1].Text = "5th Mouse Button";

                lVMacros.Items[lVMacros.Items.Count - 1].EnsureVisible();
            }
        }

        private void anyMouseUp(object sender, MouseEventArgs e)
        {
            if (btnRecord.Text == Properties.Resources.StopText && (macros.Count != 0 || (recordAfter && macrosAfter.Count != 0)))
            {
                int value;
                switch (e.Button)
                {
                    case MouseButtons.Left: value = 256; break;
                    case MouseButtons.Right: value = 257; break;
                    case MouseButtons.Middle: value = 258; break;
                    case MouseButtons.XButton1: value = 259; break;
                    case MouseButtons.XButton2: value = 260; break;
                    default: value = 0; break;
                }

                if (cBRecordDelays.Checked)
                {
                    AddMacroValue((int)sw.ElapsedMilliseconds + 300);
                    lVMacros.Items.Add("Wait " + sw.ElapsedMilliseconds + "ms", 2);
                    sw.Reset();
                    sw.Start();
                }

                AddMacroValue(value);
                lVMacros.Items.Add(e.Button.ToString() + " Mouse Button", 1);

                if (e.Button == MouseButtons.XButton1)
                    lVMacros.Items[lVMacros.Items.Count - 1].Text = "4th Mouse Button";

                if (e.Button == MouseButtons.XButton2)
                    lVMacros.Items[lVMacros.Items.Count - 1].Text = "5th Mouse Button";

                lVMacros.Items[lVMacros.Items.Count - 1].EnsureVisible();
            }
        }

        private void btn4th_Click(object sender, EventArgs e)
        {
            int value = 259;
            if (btn4th.Text.Contains(Properties.Resources.DownText))
            {
                if (macros.Count == 0 || (recordAfter && macrosAfter.Count == 0))
                {
                    AddMacroValue(value);
                    lVMacros.Items.Add("4th Mouse Button", 0);
                    if (cBRecordDelays.Checked)
                    {
                        sw.Reset();
                        sw.Start();
                    }
                }
                else if (macros.Count > 0 || (recordAfter && macrosAfter.Count >0))
                {
                    if (cBRecordDelays.Checked)
                    {
                        AddMacroValue((int)sw.ElapsedMilliseconds + 300);
                        lVMacros.Items.Add(Properties.Resources.WaitMS.Replace("*number*", sw.ElapsedMilliseconds.ToString()).Replace("*ms*", "ms"), 2);
                        sw.Reset();
                        sw.Start();
                    }

                    AddMacroValue(value);
                    lVMacros.Items.Add("4th Mouse Button", 0);
                }

                btn4th.Text = Properties.Resources.FourthMouseUp;
            }
            else
            {
                if (cBRecordDelays.Checked)
                {
                    AddMacroValue((int)sw.ElapsedMilliseconds + 300);
                    lVMacros.Items.Add(Properties.Resources.WaitMS.Replace("*number*", sw.ElapsedMilliseconds.ToString()).Replace("*ms*", "ms"), 2);
                    sw.Reset();
                    sw.Start();
                }

                AddMacroValue(value);
                lVMacros.Items.Add("4th Mouse Button", 1);
                btn4th.Text = Properties.Resources.FourthMouseDown;
            }

            lVMacros.Items[lVMacros.Items.Count - 1].EnsureVisible();
        }

        private void btn5th_Click(object sender, EventArgs e)
        {
            int value = 260;
            if (btn5th.Text.Contains(Properties.Resources.DownText))
            {
                if (macros.Count == 0 || (recordAfter && macrosAfter.Count == 0))
                {
                    AddMacroValue(value);
                    lVMacros.Items.Add("5th Mouse Button", 0);
                    if (cBRecordDelays.Checked)
                    {
                        sw.Reset();
                        sw.Start();
                    }
                }
                else if (macros.Count > 0 || (recordAfter && macrosAfter.Count > 0))
                {
                    if (cBRecordDelays.Checked)
                    {
                        AddMacroValue((int)sw.ElapsedMilliseconds + 300);
                        lVMacros.Items.Add(Properties.Resources.WaitMS.Replace("*number*", sw.ElapsedMilliseconds.ToString()).Replace("*ms*", "ms"), 2);
                        sw.Reset();
                        sw.Start();
                    }

                    AddMacroValue(value);
                    lVMacros.Items.Add("5th Mouse Button", 0);
                }

                btn5th.Text = Properties.Resources.FifthMouseUp;
            }
            else
            {
                if (cBRecordDelays.Checked)
                {
                    AddMacroValue((int)sw.ElapsedMilliseconds + 300);
                    lVMacros.Items.Add(Properties.Resources.WaitMS.Replace("*number*", sw.ElapsedMilliseconds.ToString()).Replace("*ms*", "ms"), 2);
                    sw.Reset();
                    sw.Start();
                }

                AddMacroValue(value);
                lVMacros.Items.Add("5th Mouse Button", 1);
                btn5th.Text = Properties.Resources.FifthMouseDown;
            }

            lVMacros.Items[lVMacros.Items.Count - 1].EnsureVisible();
        }


        private void btnRumble_Click(object sender, EventArgs e)
        {
            int value = 1255255;
            if (btnRumble.Text.Contains("Add"))
            {
                if (macros.Count == 0 || (recordAfter && macrosAfter.Count == 0))
                {
                    AddMacroValue(value);
                    lVMacros.Items.Add("Rumble 255,255 (100%)", 0);
                    if (cBRecordDelays.Checked)
                    {
                        sw.Reset();
                        sw.Start();
                    }
                }
                else if (macros.Count > 0 || (recordAfter && macrosAfter.Count > 0))
                {
                    if (cBRecordDelays.Checked)
                    {
                        AddMacroValue((int)sw.ElapsedMilliseconds + 300);
                        lVMacros.Items.Add(Properties.Resources.WaitMS.Replace("*number*", sw.ElapsedMilliseconds.ToString()).Replace("*ms*", "ms"), 2);
                        sw.Reset();
                        sw.Start();
                    }

                    AddMacroValue(value);
                    lVMacros.Items.Add("Rumble 255,255 (100%)", 0);
                }

                btnRumble.Text = "Stop Rumble";
            }
            else
            {
                value = 1000000;
                if (cBRecordDelays.Checked)
                {
                    AddMacroValue((int)sw.ElapsedMilliseconds + 300);
                    lVMacros.Items.Add(Properties.Resources.WaitMS.Replace("*number*", sw.ElapsedMilliseconds.ToString()).Replace("*ms*", "ms"), 2);
                    sw.Reset();
                    sw.Start();
                }

                AddMacroValue(value);
                lVMacros.Items.Add("Stop Rumble", 1);
                btnRumble.Text = "Add Rumble";
            }

            lVMacros.Items[lVMacros.Items.Count - 1].EnsureVisible();
        }

        private void btnLightbar_Click(object sender, EventArgs e)
        {
            int value = 1255255255;
            if (btnLightbar.Text.Contains("Change"))
            {
                if (macros.Count == 0 || (recordAfter && macrosAfter.Count == 0))
                {
                    AddMacroValue(value);
                    lVMacros.Items.Add("Lightbar Color: 255,255,255", 0);
                    if (cBRecordDelays.Checked)
                    {
                        sw.Reset();
                        sw.Start();
                    }
                }
                else if (macros.Count > 0 || (recordAfter && macrosAfter.Count > 0))
                {
                    if (cBRecordDelays.Checked)
                    {
                        AddMacroValue((int)sw.ElapsedMilliseconds + 300);
                        lVMacros.Items.Add(Properties.Resources.WaitMS.Replace("*number*", sw.ElapsedMilliseconds.ToString()).Replace("*ms*", "ms"), 2);
                        sw.Reset();
                        sw.Start();
                    }

                    AddMacroValue(value);
                    lVMacros.Items.Add("Lightbar Color: 255,255,255", 0);
                }

                btnLightbar.Text = "Reset Lightbar Color";
            }
            else
            {
                value = 1000000000;
                if (cBRecordDelays.Checked)
                {
                    AddMacroValue((int)sw.ElapsedMilliseconds + 300);
                    lVMacros.Items.Add(Properties.Resources.WaitMS.Replace("*number*", sw.ElapsedMilliseconds.ToString()).Replace("*ms*", "ms"), 2);
                    sw.Reset();
                    sw.Start();
                }

                AddMacroValue(value);
                lVMacros.Items.Add("Reset Lightbar", 1);
                btnLightbar.Text = "Change Lightbar Color";
            }

            lVMacros.Items[lVMacros.Items.Count - 1].EnsureVisible();
        }

        public void btnSave_Click(object sender, EventArgs e)
        {
            if (sA != null && sAButton > -1)
            {
                sA.multiMacrostag[sAButton] = macros;
                switch (sAButton)
                {
                    case 0: sA.btnSTapT.Text = macros.Count > 0 ? Properties.Resources.MacroRecorded : Properties.Resources.SelectMacro; break;
                    case 1: sA.btnHoldT.Text = macros.Count > 0 ? Properties.Resources.MacroRecorded : Properties.Resources.SelectMacro; break;
                    case 2: sA.btnDTapT.Text = macros.Count > 0 ? Properties.Resources.MacroRecorded : Properties.Resources.SelectMacro; break;
                }

                saved = true;
                Close();
            }
            else if (macros.Count > 0)
            {
                macronames.Clear();
                foreach (ListViewItem lvi in lVMacros.Items)
                {
                    macronames.Add(lvi.Text);
                }

                string macro = string.Join(", ", macronames.ToArray());
                if (kbm != null)
                {
                    kbm.macrostag = macros;
                    kbm.macros = macronames;
                    kbm.lBMacroOn.Visible = true;
                    kbm.macrorepeat = cBStyle.SelectedIndex == 1;
                    saved = true;
                    if (sender != kbm)
                        kbm.Close();
                }
                else if (sA != null)
                {
                    sA.macrostag = macros;
                    sA.macros = macronames;
                    sA.lbMacroRecorded.Text = string.Join(", ", macronames);
                    //kbm.lBMacroOn.Visible = true;
                    sA.macrorepeat = cBStyle.SelectedIndex == 1;
                    saved = true;
                    //if (sender != sA)
                    // sA.Close();
                    Close();
                }
                else
                    Close();
            }
            else
            {
                MessageBox.Show(Properties.Resources.NoMacroRecorded, "DS4Windows", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnSaveP_Click(object sender, EventArgs e)
        {
            if (macros.Count > 0)
            {
                Stream stream;
                Console.WriteLine(Directory.GetParent(Assembly.GetExecutingAssembly().Location).FullName);
                Console.WriteLine(Global.appdatapath);
                //string path;
                if (Global.appdatapath == Directory.GetParent(Assembly.GetExecutingAssembly().Location).FullName)
                    savePresets.InitialDirectory = Directory.GetParent(Assembly.GetExecutingAssembly().Location).FullName + @"\Macros\";
                else
                    savePresets.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\DS4Tool" + @"\Macros\";

                if (!Directory.Exists(savePresets.InitialDirectory))
                {
                    Directory.CreateDirectory(savePresets.InitialDirectory);
                    //savePresets.InitialDirectory = path;
                }

                Console.WriteLine(savePresets.InitialDirectory);
                if (savePresets.ShowDialog() == DialogResult.OK)
                {
                    if ((stream = savePresets.OpenFile()) != null)
                    {
                        string macro = string.Join("/", macros.ToArray());
                        StreamWriter sw = new StreamWriter(stream);
                        sw.Write(macro);
                        sw.Close();
                    }
                }
            }
            else
            {
                MessageBox.Show(Properties.Resources.NoMacroRecorded, "DS4Windows", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnLoadP_Click(object sender, EventArgs e)
        {
            cMSLoadPresets.Show(btnLoadP, new Point(0, btnLoadP.Height));           
        }

        private void altTabToolStripMenuItem_Click(object sender, EventArgs e)
        {
            macros.Clear();
            lVMacros.Items.Clear();
            macros.Add(18);
            macros.Add(9);
            macros.Add(9);
            macros.Add(18);
            macros.Add(1300);
            LoadMacro();
        }

        private void fromFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Global.appdatapath == Directory.GetParent(Assembly.GetExecutingAssembly().Location).FullName)
                openPresets.InitialDirectory = Directory.GetParent(Assembly.GetExecutingAssembly().Location).FullName + @"\Macros\";
            else
                openPresets.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\DS4Tool" + @"\Macros\";

            if (openPresets.ShowDialog() == DialogResult.OK)
            {
                string file = openPresets.FileName;
                macros.Clear();
                lVMacros.Items.Clear();
                StreamReader sr = new StreamReader(file);
                string[] macs = File.ReadAllText(file).Split('/');
                int temp;
                foreach (string s in macs)
                {
                    if (int.TryParse(s, out temp))
                        macros.Add(temp);
                }

                LoadMacro();
                sr.Close();
            }
        }

        private void lVMacros_MouseHover(object sender, EventArgs e)
        {
            lVMacros.Focus();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            saved = true;
            Close();
        }        
       
        void LoadMacro()
        {
            if (macros.Count > 0)
            {
                bool[] keydown = new bool[286];
                foreach (int i in macros)
                {
                    if (i >= 1000000000)
                    {
                        if (i > 1000000000)
                        {
                            string lb = i.ToString().Substring(1);
                            byte r = (byte)(int.Parse(lb[0].ToString()) * 100 + int.Parse(lb[1].ToString()) * 10 + int.Parse(lb[2].ToString()));
                            byte g = (byte)(int.Parse(lb[3].ToString()) * 100 + int.Parse(lb[4].ToString()) * 10 + int.Parse(lb[5].ToString()));
                            byte b = (byte)(int.Parse(lb[6].ToString()) * 100 + int.Parse(lb[7].ToString()) * 10 + int.Parse(lb[8].ToString()));
                            lVMacros.Items.Add($"Lightbar Color: {r},{g},{b}", 0);
                        }
                        else
                        {
                            lVMacros.Items.Add("Reset Lightbar", 1);
                        }
                    }
                    else if (i >= 1000000)
                    {
                        if (i > 1000000)
                        {
                            string r = i.ToString().Substring(1);
                            byte heavy = (byte)(int.Parse(r[0].ToString()) * 100 + int.Parse(r[1].ToString()) * 10 + int.Parse(r[2].ToString()));
                            byte light = (byte)(int.Parse(r[3].ToString()) * 100 + int.Parse(r[4].ToString()) * 10 + int.Parse(r[5].ToString()));
                            lVMacros.Items.Add($"Rumble {heavy}, {light} ({Math.Round((heavy * .75f + light * .25f) / 2.55f, 1)}%)", 0);
                        }
                        else
                            lVMacros.Items.Add("Stop Rumble", 1);
                    }
                    else if (i >= 300) //ints over 300 used to delay
                    {
                        lVMacros.Items.Add(Properties.Resources.WaitMS.Replace("*number*", (i - 300).ToString()).Replace("*ms*", "ms"), 2);
                    }
                    else if (!keydown[i])
                    {
                        //anything above 255 is not a keyvalue
                        if (i == 256) lVMacros.Items.Add("Left Mouse Button", 0);
                        else if (i == 257) lVMacros.Items.Add("Right Mouse Button", 0);
                        else if (i == 258) lVMacros.Items.Add("Middle Mouse Button", 0);
                        else if (i == 259) lVMacros.Items.Add("4th Mouse Button", 0);
                        else if (i == 260) lVMacros.Items.Add("5th Mouse Button", 0);
                        else if (i == 261) lVMacros.Items.Add("A Button", 0);
                        else if (i == 262) lVMacros.Items.Add("B Button", 0);
                        else if (i == 263) lVMacros.Items.Add("X Button", 0);
                        else if (i == 264) lVMacros.Items.Add("Y Button", 0);
                        else if (i == 265) lVMacros.Items.Add("Start", 0);
                        else if (i == 266) lVMacros.Items.Add("Back", 0);
                        else if (i == 267) lVMacros.Items.Add("Up Button", 0);
                        else if (i == 268) lVMacros.Items.Add("Down Button", 0);
                        else if (i == 269) lVMacros.Items.Add("Left Button", 0);
                        else if (i == 270) lVMacros.Items.Add("Right Button", 0);
                        else if (i == 271) lVMacros.Items.Add("Guide", 0);
                        else if (i == 272) lVMacros.Items.Add("Left Bumper", 0);
                        else if (i == 273) lVMacros.Items.Add("Right Bumper", 0);
                        else if (i == 274) lVMacros.Items.Add("Left Trigger", 0);
                        else if (i == 275) lVMacros.Items.Add("Right Trigger", 0);
                        else if (i == 276) lVMacros.Items.Add("Left Stick", 0);
                        else if (i == 277) lVMacros.Items.Add("Right Stick", 0);
                        else if (i == 278) lVMacros.Items.Add("LS Right", 0);
                        else if (i == 279) lVMacros.Items.Add("LS Left", 0);
                        else if (i == 280) lVMacros.Items.Add("LS Down", 0);
                        else if (i == 281) lVMacros.Items.Add("LS Up", 0);
                        else if (i == 282) lVMacros.Items.Add("RS Right", 0);
                        else if (i == 283) lVMacros.Items.Add("RS Left", 0);
                        else if (i == 284) lVMacros.Items.Add("RS Down", 0);
                        else if (i == 285) lVMacros.Items.Add("RS Up", 0);
                        else lVMacros.Items.Add(((Keys)i).ToString(), 0);
                        keydown[i] = true;
                    }
                    else
                    {
                        if (i == 256) lVMacros.Items.Add("Left Mouse Button", 1);
                        else if (i == 257) lVMacros.Items.Add("Right Mouse Button", 1);
                        else if (i == 258) lVMacros.Items.Add("Middle Mouse Button", 1);
                        else if (i == 259) lVMacros.Items.Add("4th Mouse Button", 1);
                        else if (i == 260) lVMacros.Items.Add("5th Mouse Button", 1);
                        else if (i == 261) lVMacros.Items.Add("A Button", 1);
                        else if (i == 262) lVMacros.Items.Add("B Button", 1);
                        else if (i == 263) lVMacros.Items.Add("X Button", 1);
                        else if (i == 264) lVMacros.Items.Add("Y Button", 1);
                        else if (i == 265) lVMacros.Items.Add("Start", 1);
                        else if (i == 266) lVMacros.Items.Add("Back", 1);
                        else if (i == 267) lVMacros.Items.Add("Up Button", 1);
                        else if (i == 268) lVMacros.Items.Add("Down Button", 1);
                        else if (i == 269) lVMacros.Items.Add("Left Button", 1);
                        else if (i == 270) lVMacros.Items.Add("Right Button", 1);
                        else if (i == 271) lVMacros.Items.Add("Guide", 1);
                        else if (i == 272) lVMacros.Items.Add("Left Bumper", 1);
                        else if (i == 273) lVMacros.Items.Add("Right Bumper", 1);
                        else if (i == 274) lVMacros.Items.Add("Left Trigger", 1);
                        else if (i == 275) lVMacros.Items.Add("Right Trigger", 1);
                        else if (i == 276) lVMacros.Items.Add("Left Stick", 1);
                        else if (i == 277) lVMacros.Items.Add("Right Stick", 1);
                        else if (i == 278) lVMacros.Items.Add("LS Right", 1);
                        else if (i == 279) lVMacros.Items.Add("LS Left", 1);
                        else if (i == 280) lVMacros.Items.Add("LS Down", 1);
                        else if (i == 281) lVMacros.Items.Add("LS Up", 1);
                        else if (i == 282) lVMacros.Items.Add("RS Right", 1);
                        else if (i == 283) lVMacros.Items.Add("RS Left", 1);
                        else if (i == 284) lVMacros.Items.Add("RS Down", 1);
                        else if (i == 285) lVMacros.Items.Add("RS Up", 1);
                        else lVMacros.Items.Add(((Keys)i).ToString(), 1);
                        keydown[i] = false;
                    }
                }

                for (int i = 0; i < keydown.Length; i++)
                {
                    if (keydown[i])
                    {
                        if (i == 256) lVMacros.Items.Add("Left Mouse Button", 1);
                        else if (i == 257) lVMacros.Items.Add("Right Mouse Button", 1);
                        else if (i == 258) lVMacros.Items.Add("Middle Mouse Button", 1);
                        else if (i == 259) lVMacros.Items.Add("4th Mouse Button", 1);
                        else if (i == 260) lVMacros.Items.Add("5th Mouse Button", 1);
                        else if (i == 261) lVMacros.Items.Add("A Button", 1);
                        else if (i == 262) lVMacros.Items.Add("B Button", 1);
                        else if (i == 263) lVMacros.Items.Add("X Button", 1);
                        else if (i == 264) lVMacros.Items.Add("Y Button", 1);
                        else if (i == 265) lVMacros.Items.Add("Start", 1);
                        else if (i == 266) lVMacros.Items.Add("Back", 1);
                        else if (i == 267) lVMacros.Items.Add("Up Button", 1);
                        else if (i == 268) lVMacros.Items.Add("Down Button", 1);
                        else if (i == 269) lVMacros.Items.Add("Left Button", 1);
                        else if (i == 270) lVMacros.Items.Add("Right Button", 1);
                        else if (i == 271) lVMacros.Items.Add("Guide", 1);
                        else if (i == 272) lVMacros.Items.Add("Left Bumper", 1);
                        else if (i == 273) lVMacros.Items.Add("Right Bumper", 1);
                        else if (i == 274) lVMacros.Items.Add("Left Trigger", 1);
                        else if (i == 275) lVMacros.Items.Add("Right Trigger", 1);
                        else if (i == 276) lVMacros.Items.Add("Left Stick", 1);
                        else if (i == 277) lVMacros.Items.Add("Right Stick", 1);
                        else if (i == 278) lVMacros.Items.Add("LS Right", 1);
                        else if (i == 279) lVMacros.Items.Add("LS Left", 1);
                        else if (i == 280) lVMacros.Items.Add("LS Down", 1);
                        else if (i == 281) lVMacros.Items.Add("LS Up", 1);
                        else if (i == 282) lVMacros.Items.Add("RS Right", 1);
                        else if (i == 283) lVMacros.Items.Add("RS Left", 1);
                        else if (i == 284) lVMacros.Items.Add("RS Down", 1);
                        else if (i == 285) lVMacros.Items.Add("RS Up", 1);
                        else lVMacros.Items.Add(((Keys)i).ToString(), 1);
                        macros.Add(i);
                    }
                }
            }
        }

        private void RecordBox_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!saved && macros.Count > 0)
            {
                if (MessageBox.Show(Properties.Resources.SaveRecordedMacro, "DS4Windows", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    btnSave_Click(null, null);
            }

            Program.rootHub.recordingMacro = false;
        }

        protected override bool IsInputKey(Keys keyData)
        {
            switch (keyData)
            {
                case Keys.Right:
                case Keys.Left:
                case Keys.Up:
                case Keys.Down:
                case Keys.Tab:
                case Keys.MediaPlayPause:
                case Keys.MediaPreviousTrack:
                case Keys.MediaNextTrack:
                    return true;
                case Keys.Shift | Keys.Right:
                case Keys.Shift | Keys.Left:
                case Keys.Shift | Keys.Up:
                case Keys.Shift | Keys.Down:
                case Keys.Shift | Keys.Tab:
                case Keys.Shift | Keys.MediaPlayPause:
                case Keys.Shift | Keys.MediaPreviousTrack:
                case Keys.Shift | Keys.MediaNextTrack:
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
                case Keys.Tab:
                case Keys.MediaPlayPause:
                case Keys.MediaPreviousTrack:
                case Keys.MediaNextTrack:
                    if (e.Shift)
                    {

                    }
                    else
                    {
                    }
                    break;
            }
        }

        private int selection;
        private bool changingDelay = false;

        private void lVMacros_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (lVMacros.SelectedIndices[0] >= 0)
            {
                if (lVMacros.SelectedItems[0].ImageIndex == 2)
                {
                    TextBox tb = new TextBox();
                    tb.MaxLength = 5;
                    tb.KeyDown += nud_KeyDown;
                    tb.LostFocus += nud_LostFocus;
                    selection = lVMacros.SelectedIndices[0];
                    Controls.Add(tb);
                    changingDelay = true;
                    tb.Location = new Point(lVMacros.Location.X + lVMacros.SelectedItems[0].Position.X, lVMacros.Location.Y + lVMacros.SelectedItems[0].Position.Y);
                    tb.BringToFront();
                    lVMacros.MouseHover -= lVMacros_MouseHover;
                    tb.TextChanged += tb_TextChanged;
                    tb.Focus();
                }
                else if (macros[lVMacros.SelectedIndices[0]] > 1000000000)
                {
                    selection = lVMacros.SelectedIndices[0];
                    string lb = macros[lVMacros.SelectedIndices[0]].ToString().Substring(1);
                    byte r = (byte)(int.Parse(lb[0].ToString()) * 100 + int.Parse(lb[1].ToString()) * 10 + int.Parse(lb[2].ToString()));
                    byte g = (byte)(int.Parse(lb[3].ToString()) * 100 + int.Parse(lb[4].ToString()) * 10 + int.Parse(lb[5].ToString()));
                    byte b = (byte)(int.Parse(lb[6].ToString()) * 100 + int.Parse(lb[7].ToString()) * 10 + int.Parse(lb[8].ToString()));
                    AdvancedColorDialog advColorDialog = new AdvancedColorDialog();
                    advColorDialog.Color = Color.FromArgb(r, g, b);
                    advColorDialog.OnUpdateColor += advColorDialog_OnUpdateColor;
                    if (advColorDialog.ShowDialog() == DialogResult.OK)
                    {
                        macros[selection] = 1000000000 + advColorDialog.Color.R * 1000000 + advColorDialog.Color.G * 1000 + advColorDialog.Color.B;
                    }
                    lVMacros.Items[selection].Text = ($"Lightbar Color: {advColorDialog.Color.R},{advColorDialog.Color.G},{advColorDialog.Color.B}");
                }
                else if (macros[lVMacros.SelectedIndices[0]] > 1000000 && macros[lVMacros.SelectedIndices[0]] != 1000000000)
                {

                    lVMacros.MouseHover -= lVMacros_MouseHover;
                    string r = macros[lVMacros.SelectedIndices[0]].ToString().Substring(1);
                    byte heavy = (byte)(int.Parse(r[0].ToString()) * 100 + int.Parse(r[1].ToString()) * 10 + int.Parse(r[2].ToString()));
                    byte light = (byte)(int.Parse(r[3].ToString()) * 100 + int.Parse(r[4].ToString()) * 10 + int.Parse(r[5].ToString()));
                    selection = lVMacros.SelectedIndices[0];
                    tb1 = new TextBox();
                    tb2 = new TextBox();
                    tb1.Name = "tBHeavy";
                    tb1.Name = "tBLight";
                    tb1.MaxLength = 3;
                    tb2.MaxLength = 3;
                    tb1.KeyDown += nud_KeyDown;
                    tb2.KeyDown += nud_KeyDown;
                    Controls.Add(tb1);
                    Controls.Add(tb2);
                    changingDelay = false;
                    tb1.Location = new Point(lVMacros.Location.X + lVMacros.SelectedItems[0].Position.X, lVMacros.Location.Y + lVMacros.SelectedItems[0].Position.Y);
                    tb1.Size = new Size(tb1.Size.Width / 2, tb1.Size.Height);
                    tb2.Location = new Point(lVMacros.Location.X + lVMacros.SelectedItems[0].Position.X + tb1.Size.Width, lVMacros.Location.Y + lVMacros.SelectedItems[0].Position.Y);
                    tb2.Size = tb1.Size;
                    tb1.BringToFront();
                    tb2.BringToFront();
                    tb1.Text = heavy.ToString();
                    tb2.Text = light.ToString();
                    tb1.TextChanged += tb_TextChanged;
                    tb2.TextChanged += tb_TextChanged;
                    tb1.Focus();
                }
            }
        }

        void tb_TextChanged(object sender, EventArgs e)
        {
            TextBox tb = (TextBox)sender;
            //if (changingDelay)
            {
                for (int i = tb.Text.Length - 1; i >= 0; i--)
                {
                    if (!char.IsDigit(tb.Text[i]))
                        tb.Text = tb.Text.Remove(i, 1);
                }
            }
        }

        void nud_LostFocus(object sender, EventArgs e)
        {
            SaveMacroChange((TextBox)sender);
        }

        void nud_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                SaveMacroChange((TextBox)sender);
        }

        private void SaveMacroChange(TextBox tb)
        {
            int i, j;
            if (!string.IsNullOrEmpty(tb.Text))
            {
                if (changingDelay && int.TryParse(tb.Text, out i))
                {
                    lVMacros.Items[selection] = new ListViewItem(Properties.Resources.WaitMS.Replace("*number*", (tb.Text)).Replace("*ms*", "ms"), 2);
                    macros[selection] = i + 300;
                    Controls.Remove(tb);
                    saved = false;
                }
                else if (!changingDelay)
                {
                    if (int.TryParse(tb1.Text, out i) && int.TryParse(tb2.Text, out j))
                    {
                        if (i + j > 0)
                        {
                            byte heavy = (byte)i;
                            byte light = (byte)j;
                            lVMacros.Items[selection].Text = ($"Rumble {heavy}, {light} ({Math.Round((heavy * .75f + light * .25f) / 2.55f, 1)}%)");
                            macros[selection] = 1000000 + heavy * 1000 + light;
                            saved = false;
                            Controls.Remove(tb1);
                            Controls.Remove(tb2);
                            tb1 = null;
                            tb2 = null;
                        }
                    }
                }
            }

            lVMacros.MouseHover += lVMacros_MouseHover;
        }

        private void RecordBox_Resize(object sender, EventArgs e)
        {
            cHMacro.AutoResize(ColumnHeaderAutoResizeStyle.HeaderSize);
        }

        private void advColorDialog_OnUpdateColor(object sender, EventArgs e)
        {
            if (sender is Color && Program.rootHub.DS4Controllers[0] != null)
            {
                Color color = (Color)sender;
                DS4Color dcolor = new DS4Color { red = color.R, green = color.G, blue = color.B };
                DS4LightBar.forcedColor[0] = dcolor;
                DS4LightBar.forcedFlash[0] = 0;
                DS4LightBar.forcelight[0] = true;
            }
        }

        private void lVMacros_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (btnRecord.Text != Properties.Resources.StopText)
            {
                if (lVMacros.SelectedIndices.Count > 0 && lVMacros.SelectedIndices[0] > -1)
                {
                    recordAfter = true;
                    recordAfterInt = lVMacros.SelectedIndices[0];
                    btnRecord.Text = "Record Before " + lVMacros.SelectedItems[0].Text;
                }
                else
                {
                    recordAfter = false;
                    btnRecord.Text = "Record";
                }
            }
        }
    }
}
