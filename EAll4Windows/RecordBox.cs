﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


using System.IO;
using System.Reflection;

namespace EAll4Windows
{
    public partial class RecordBox : Form
    {
        Stopwatch sw = new Stopwatch();
        Timer eall4 = new Timer();
        public List<int> macros = new List<int>(), macrosAfter = new List<int>();
        public List<string> macronames = new List<string>();
        SpecActions sA;
        KBM360 kbm;
        ControllerState cState;
        public bool saved = false;
        List<EAll4Controls> dcs = new List<EAll4Controls>();
        public RecordBox(KBM360 op)
        {
            kbm = op;
            InitializeComponent();
            if (op != null)
            if (kbm.macrorepeat)
                cBStyle.SelectedIndex = 1;
            else
                cBStyle.SelectedIndex = 0;
            AddtoEAll4List();
            eall4.Tick += eall4_Tick;
            eall4.Interval = 1;
            if (kbm.macrostag.Count > 0)
            {
                macros.AddRange(kbm.macrostag);
                LoadMacro();
                saved = true;
            }
        }

        public RecordBox(SpecActions op)
        {
            sA = op;
            InitializeComponent();
            if (sA.macrorepeat)
                cBStyle.SelectedIndex = 1;
            else
                cBStyle.SelectedIndex = 0;
            AddtoEAll4List();
            eall4.Tick += eall4_Tick;
            eall4.Interval = 1;
            lbRecordTip.Visible = false;
            cBStyle.Visible = false;
            pnlMouseButtons.Location = new Point(pnlMouseButtons.Location.X, pnlMouseButtons.Location.Y - 75);
            if (sA.macrostag.Count > 0)
            {
                macros.AddRange(sA.macrostag);
                LoadMacro();
                saved = true;
            }
        }

        void AddtoEAll4List()
        {
            dcs.Add(EAll4Controls.Cross);
            dcs.Add(EAll4Controls.Cross);
            dcs.Add(EAll4Controls.Circle);
            dcs.Add(EAll4Controls.Square);
            dcs.Add(EAll4Controls.Triangle);
            dcs.Add(EAll4Controls.Options);
            dcs.Add(EAll4Controls.Share);
            dcs.Add(EAll4Controls.DpadUp);
            dcs.Add(EAll4Controls.DpadDown);
            dcs.Add(EAll4Controls.DpadLeft);
            dcs.Add(EAll4Controls.DpadRight);
            dcs.Add(EAll4Controls.PS);
            dcs.Add(EAll4Controls.L1);
            dcs.Add(EAll4Controls.R1);
            dcs.Add(EAll4Controls.L2);
            dcs.Add(EAll4Controls.R2);
            dcs.Add(EAll4Controls.L3);
            dcs.Add(EAll4Controls.R3);
            dcs.Add(EAll4Controls.LXPos);
            dcs.Add(EAll4Controls.LXNeg);
            dcs.Add(EAll4Controls.LYPos);
            dcs.Add(EAll4Controls.LYNeg);
            dcs.Add(EAll4Controls.RXPos);
            dcs.Add(EAll4Controls.RXNeg);
            dcs.Add(EAll4Controls.RYPos);
            dcs.Add(EAll4Controls.RYNeg);
        }

        void AddMacroValue(int value)
        {
            if (recordAfter)
                macrosAfter.Add(value);
            else
                macros.Add(value);
        }
        void eall4_Tick(object sender, EventArgs e)
        {
            if (Program.rootHub.EAll4Controllers[0] != null)
            {
                cState = Program.rootHub.getEAll4State(0);
                if (btnRecord.Text == Properties.Resources.StopText)
                    foreach (EAll4Controls dc in dcs)
                        if (Mapping.getBoolMapping(dc, cState, null, null))
                        {
                            int value = EAll4ControltoInt(dc);
                            int count = 0;
                            foreach (int i in macros)
                            {
                                if (i == value)
                                    count++;
                            }
                            if (macros.Count == 0)
                            {
                                AddMacroValue(value);
                                lVMacros.Items.Add(EAll4ControltoX360(dc), 0);
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
                                lVMacros.Items.Add(EAll4ControltoX360(dc), 0);
                            }
                            lVMacros.Items[lVMacros.Items.Count - 1].EnsureVisible();
                        }
                        else if (!Mapping.getBoolMapping(dc, cState, null, null))
                        {
                            if (macros.Count != 0)
                            {
                                int value = EAll4ControltoInt(dc);
                                int count = 0;
                                foreach (int i in macros)
                                {
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
                                    lVMacros.Items.Add(EAll4ControltoX360(dc), 1);
                                    lVMacros.Items[lVMacros.Items.Count - 1].EnsureVisible();
                                }
                            }
                        }
            }
        }
        
        public static int EAll4ControltoInt(EAll4Controls ctrl)
        {
            switch (ctrl)
            {
                case EAll4Controls.Cross: return 261;
                case EAll4Controls.Circle: return 262; 
                case EAll4Controls.Square: return 263; 
                case EAll4Controls.Triangle: return 264; 
                case EAll4Controls.Options: return 265; 
                case EAll4Controls.Share: return 266; 
                case EAll4Controls.DpadUp: return 267; 
                case EAll4Controls.DpadDown: return 268; 
                case EAll4Controls.DpadLeft: return 269; 
                case EAll4Controls.DpadRight: return 270; 
                case EAll4Controls.PS: return 271; 
                case EAll4Controls.L1: return 272; 
                case EAll4Controls.R1: return 273; 
                case EAll4Controls.L2: return 274; 
                case EAll4Controls.R2: return 275; 
                case EAll4Controls.L3: return 276;
                case EAll4Controls.R3: return 277;
                case EAll4Controls.LXPos: return 278;
                case EAll4Controls.LXNeg: return 279;
                case EAll4Controls.LYPos: return 280;
                case EAll4Controls.LYNeg: return 281;
                case EAll4Controls.RXPos: return 282;
                case EAll4Controls.RXNeg: return 283;
                case EAll4Controls.RYPos: return 284;
                case EAll4Controls.RYNeg: return 285;
            }
            return 0;
        }

        public static string EAll4ControltoX360(EAll4Controls ctrl)
        {
            switch (ctrl)
            {
                case EAll4Controls.Cross: return "A Button";
                case EAll4Controls.Circle: return "B Button";
                case EAll4Controls.Square: return "X Button";
                case EAll4Controls.Triangle: return "Y Button";
                case EAll4Controls.Options: return "Start";
                case EAll4Controls.Share: return "Back";
                case EAll4Controls.DpadUp: return "Up Button";
                case EAll4Controls.DpadDown: return "Down Button";
                case EAll4Controls.DpadLeft: return "Left Button";
                case EAll4Controls.DpadRight: return "Right Button";
                case EAll4Controls.PS: return "Guide";
                case EAll4Controls.L1: return "Left Bumper";
                case EAll4Controls.R1: return "Right Bumper";
                case EAll4Controls.L2: return "Left Trigger";
                case EAll4Controls.R2: return "Right Trigger";
                case EAll4Controls.L3: return "Left Stick";
                case EAll4Controls.R3: return "Right Stick";
                case EAll4Controls.LXPos: return "LS Right";
                case EAll4Controls.LXNeg: return "LS Left";
                case EAll4Controls.LYPos: return "LS Down";
                case EAll4Controls.LYNeg: return "LS Up";
                case EAll4Controls.RXPos: return "RS Right";
                case EAll4Controls.RXNeg: return "RS Left";
                case EAll4Controls.RYPos: return "RS Down";
                case EAll4Controls.RYNeg: return "RS Up";
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
                Program.rootHub.recordingMacro = true;
                saved = false;
                eall4.Start();
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
                eall4.Stop();
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
                    foreach (int i in macrosAfter)
                    {
                        if (i == value)
                            count++;
                    }
                else
                foreach (int i in macros)
                {
                    if (i == value)
                        count++;
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
                Close();
            else if (e.KeyCode == Keys.Delete)
                if (lVMacros.SelectedIndices.Count > 0 && lVMacros.SelectedIndices[0] > -1)
                {
                    macros.RemoveAt(lVMacros.SelectedIndices[0]);
                    lVMacros.Items.Remove(lVMacros.SelectedItems[0]);
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
                            if (macrosAfter[i] == 160)
                                return 160;
                            else if (macrosAfter[i] == 161)
                                return 161;
                    }
                    else
                        for (int i = macros.Count - 1; i >= 0; i--)
                            if (macros[i] == 160)
                                return 160;
                            else if (macros[i] == 161)
                                return 161;
                }
                else if (e.KeyCode == Keys.ControlKey)
                {
                    if (recordAfter)
                    {
                        for (int i = macrosAfter.Count - 1; i >= 0; i--)
                            if (macrosAfter[i] == 162)
                                return 162;
                            else if (macrosAfter[i] == 163)
                                return 163;
                    }
                    else
                    for (int i = macros.Count - 1; i >= 0; i--)
                        if (macros[i] == 162)
                            return 162;
                        else if (macros[i] == 163)
                            return 163;
                }
                else if (e.KeyCode == Keys.Menu)
                {
                    if (recordAfter)
                    {
                        for (int i = macrosAfter.Count - 1; i >= 0; i--)
                            if (macrosAfter[i] == 164)
                                return 164;
                            else if (macrosAfter[i] == 165)
                                return 165;
                    }
                    else
                        for (int i = macros.Count - 1; i >= 0; i--)
                            if (macros[i] == 164)
                                return 164;
                            else if (macros[i] == 165)
                                return 165;
                }
                return e.KeyValue;
            }
            else
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
            return e.KeyValue;
        }

        private void anyKeyUp(object sender, KeyEventArgs e)
        {
            if (btnRecord.Text == Properties.Resources.StopText && (macros.Count != 0 || (recordAfter && macrosAfter.Count != 0)))
            {
                int value = WhichKey(e, 1);
                if (cBRecordDelays.Checked)
                {
                    AddMacroValue((int)sw.ElapsedMilliseconds + 300);
                    lVMacros.Items.Add(Properties.Resources.WaitMS.Replace("*number*", sw.ElapsedMilliseconds.ToString()).Replace("*ms*", "ms"), 2);
                    sw.Reset();
                    sw.Start();
                }
                AddMacroValue(value);
                lVMacros.Items.Add(((Keys)value).ToString(), 1);
                lVMacros.Items[lVMacros.Items.Count - 1].EnsureVisible();
            }
        }
        private void anyMouseDown(object sender, MouseEventArgs e)
        {
            if (btnRecord.Text == Properties.Resources.StopText)
            {
                int value;
                switch (e.Button)
                {
                    case System.Windows.Forms.MouseButtons.Left: value = 256; break;
                    case System.Windows.Forms.MouseButtons.Right: value = 257; break;
                    case System.Windows.Forms.MouseButtons.Middle: value = 258; break;
                    case System.Windows.Forms.MouseButtons.XButton1: value = 259; break;
                    case System.Windows.Forms.MouseButtons.XButton2: value = 260; break;
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
                if (e.Button == System.Windows.Forms.MouseButtons.XButton1)
                    lVMacros.Items[lVMacros.Items.Count - 1].Text = "4th Mouse Button";
                if (e.Button == System.Windows.Forms.MouseButtons.XButton2)
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
                    case System.Windows.Forms.MouseButtons.Left: value = 256; break;
                    case System.Windows.Forms.MouseButtons.Right: value = 257; break;
                    case System.Windows.Forms.MouseButtons.Middle: value = 258; break;
                    case System.Windows.Forms.MouseButtons.XButton1: value = 259; break;
                    case System.Windows.Forms.MouseButtons.XButton2: value = 260; break;
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
                if (e.Button == System.Windows.Forms.MouseButtons.XButton1)
                    lVMacros.Items[lVMacros.Items.Count - 1].Text = "4th Mouse Button";
                if (e.Button == System.Windows.Forms.MouseButtons.XButton2)
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


        public void btnSave_Click(object sender, EventArgs e)
        {
            if (macros.Count > 0)
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
                    if (cBStyle.SelectedIndex == 1)
                        kbm.macrorepeat = true;
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
                    if (cBStyle.SelectedIndex == 1)
                        sA.macrorepeat = true;
                    saved = true;
                    //if (sender != sA)
                       // sA.Close();
                    Close();
                }
                else
                    Close();
            }
            else MessageBox.Show(Properties.Resources.NoMacroRecorded, "EAll4Windows", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
                    savePresets.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\EAll4Tool" + @"\Macros\";
                if (!Directory.Exists(savePresets.InitialDirectory))
                {
                    Directory.CreateDirectory(savePresets.InitialDirectory);
                    //savePresets.InitialDirectory = path;
                }
                Console.WriteLine(savePresets.InitialDirectory);
                if (savePresets.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    if ((stream = savePresets.OpenFile()) != null)
                    {
                        string macro = string.Join("/", macros.ToArray());
                        StreamWriter sw = new StreamWriter(stream);
                        sw.Write(macro);
                        sw.Close();
                    }
            }
            else MessageBox.Show(Properties.Resources.NoMacroRecorded, "EAll4Windows", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
                openPresets.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\EAll4Tool" + @"\Macros\";
            if (openPresets.ShowDialog() == System.Windows.Forms.DialogResult.OK)
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
                    if (i >= 300) //ints over 300 used to delay
                        lVMacros.Items.Add(Properties.Resources.WaitMS.Replace("*number*", (i - 300).ToString()).Replace("*ms*", "ms"), 2);
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
                for (ushort i = 0; i < keydown.Length; i++)
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
                if (MessageBox.Show(Properties.Resources.SaveRecordedMacro, "EAll4Windows", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == System.Windows.Forms.DialogResult.Yes)
                    btnSave_Click(null, null);
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
        private void lVMacros_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (lVMacros.SelectedIndices[0] >= 0 && lVMacros.SelectedItems[0].ImageIndex == 2)
            {
                TextBox tb = new TextBox();
                tb.MaxLength = 5;
                tb.KeyDown += nud_KeyDown;
                tb.LostFocus += nud_LostFocus;
                selection = lVMacros.SelectedIndices[0];
                Controls.Add(tb);
                tb.Location = new Point(lVMacros.Location.X + lVMacros.SelectedItems[0].Position.X, lVMacros.Location.Y + lVMacros.SelectedItems[0].Position.Y);
                tb.BringToFront();
                lVMacros.MouseHover -= lVMacros_MouseHover;
                tb.TextChanged += tb_TextChanged;
                tb.Focus();
            }
        }

        void tb_TextChanged(object sender, EventArgs e)
        {
            TextBox tb = (TextBox)sender;
            for (int i = tb.Text.Length - 1; i >= 0; i--)
                  if (!Char.IsDigit(tb.Text[i]))
                      tb.Text = tb.Text.Remove(i, 1);
        }

        void nud_LostFocus(object sender, EventArgs e)
        {
            TextBox tb = (TextBox)sender;
            int i;
            if (!string.IsNullOrEmpty(tb.Text) && int.TryParse(tb.Text, out i))
            {
                lVMacros.Items[selection] = new ListViewItem(Properties.Resources.WaitMS.Replace("*number*", (tb.Text)).Replace("*ms*", "ms"), 2);
                macros[selection] = i + 300;
                saved = false;
            }
            Controls.Remove(tb);
            lVMacros.MouseHover += lVMacros_MouseHover;
        }

        void nud_KeyDown(object sender, KeyEventArgs e)
        {
            TextBox tb = (TextBox)sender;
            if (e.KeyCode == Keys.Enter && !string.IsNullOrEmpty(tb.Text))
            {
                int i;
                if (int.TryParse(tb.Text, out i))
                {
                    lVMacros.Items[selection] = new ListViewItem(Properties.Resources.WaitMS.Replace("*number*", (tb.Text)).Replace("*ms*", "ms"), 2);
                    macros[selection] = i + 300;
                    saved = false;
                    Controls.Remove(tb);
                    lVMacros.MouseHover += lVMacros_MouseHover;
                }
            }
        }

        private void RecordBox_Resize(object sender, EventArgs e)
        {
            cHMacro.AutoResize(ColumnHeaderAutoResizeStyle.HeaderSize);
        }

        private void lVMacros_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (btnRecord.Text != Properties.Resources.StopText)
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
