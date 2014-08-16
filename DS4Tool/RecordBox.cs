using System;
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

namespace ScpServer
{
    public partial class RecordBox : Form
    {
        Stopwatch sw = new Stopwatch();
        public List<int> macros = new List<int>();
        public List<string> macronames = new List<string>();
        KBM360 kbm;
        public RecordBox(KBM360 op)
        {
            kbm = op;
            InitializeComponent();
            if (op != null)
            if (kbm.macrorepeat)
                cBStyle.SelectedIndex = 1;
            else
                cBStyle.SelectedIndex = 0;
            cB360Controls.SelectedIndex = 0;
        }

        private void btnRecord_Click(object sender, EventArgs e)
        {
            if (btnRecord.Text == Properties.Resources.RecordText)
            {
                if (cBRecordDelays.Checked)
                    sw.Start();
                macros.Clear();
                lVMacros.Items.Clear();
                btnRecord.Text = Properties.Resources.StopText;
                EnableControls(false);
                ActiveControl = null;
                lVMacros.Focus();
            }
            else
            {
                if (btn4th.Text.Contains(Properties.Resources.UpText))
                    btn4th_Click(sender, e);
                if (btn5th.Text.Contains(Properties.Resources.UpText))
                    btn5th_Click(sender, e);
                if (cBRecordDelays.Checked)
                    sw.Reset();
                btnRecord.Text = Properties.Resources.RecordText;
                EnableControls(true);                
            }
        }

        private void EnableControls(bool on)
        {
            lVMacros.Enabled = on;
            pnlSettings.Visible = on;
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
                foreach (int i in macros)
                {
                    if (i == value)
                        count++;
                }
                if (macros.Count == 0)
                {
                    macros.Add(value);
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
                        macros.Add((int)sw.ElapsedMilliseconds + 300);
                        lVMacros.Items.Add(Properties.Resources.WaitMS.Replace("*number*", sw.ElapsedMilliseconds.ToString()).Replace("*ms*", "ms"), 2);
                        sw.Reset();
                        sw.Start();
                    }
                    macros.Add(value);
                    lVMacros.Items.Add(((Keys)value).ToString(), 0);
                }
                lVMacros.Items[lVMacros.Items.Count - 1].EnsureVisible();
            }
            else if (e.KeyValue == 27)
                Close();
        }

        private int WhichKey(KeyEventArgs e, int keystate)
        {
            if (keystate == 1)
            {
                if (e.KeyCode == Keys.ShiftKey)
                {
                    for (int i = macros.Count - 1; i >= 0; i--)
                        if (macros[i] == 160)
                            return 160;
                        else if (macros[i] == 161)
                            return 161;
                }
                else if (e.KeyCode == Keys.ControlKey)
                {
                    for (int i = macros.Count - 1; i >= 0; i--)
                        if (macros[i] == 162)
                            return 162;
                        else if (macros[i] == 163)
                            return 163;
                }
                else if (e.KeyCode == Keys.Menu)
                {
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
            if (btnRecord.Text == Properties.Resources.StopText && macros.Count != 0)
            {
                int value = WhichKey(e, 1);
                if (cBRecordDelays.Checked)
                {
                    macros.Add((int)sw.ElapsedMilliseconds + 300);
                    lVMacros.Items.Add(Properties.Resources.WaitMS.Replace("*number*", sw.ElapsedMilliseconds.ToString()).Replace("*ms*", "ms"), 2);
                    sw.Reset();
                    sw.Start();
                }
                macros.Add(value);
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
                if (macros.Count == 0)
                {
                    macros.Add(value);
                    lVMacros.Items.Add(e.Button.ToString() + " Mouse Button", 0);
                    if (cBRecordDelays.Checked)
                    {
                        sw.Reset();
                        sw.Start();
                    }
                }
                else
                {
                    if (cBRecordDelays.Checked)
                    {
                        macros.Add((int)sw.ElapsedMilliseconds + 300);
                        lVMacros.Items.Add(Properties.Resources.WaitMS.Replace("*number*", sw.ElapsedMilliseconds.ToString()).Replace("*ms*", "ms"), 2);
                        sw.Reset();
                        sw.Start();
                    }
                    macros.Add(value);
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
            if (btnRecord.Text == Properties.Resources.StopText && macros.Count != 0)
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
                    macros.Add((int)sw.ElapsedMilliseconds + 300);
                    lVMacros.Items.Add("Wait " + sw.ElapsedMilliseconds + "ms", 2);
                    sw.Reset();
                    sw.Start();
                }
                macros.Add(value);
                lVMacros.Items.Add(e.Button.ToString() + " Mouse Button", 1);
                if (e.Button == System.Windows.Forms.MouseButtons.XButton1)
                    lVMacros.Items[lVMacros.Items.Count - 1].Text = "4th Mouse Button";
                if (e.Button == System.Windows.Forms.MouseButtons.XButton2)
                    lVMacros.Items[lVMacros.Items.Count - 1].Text = "5th Mouse Button";
                lVMacros.Items[lVMacros.Items.Count - 1].EnsureVisible();
            }
        }

        bool saved = false;
        private void btnSave_Click(object sender, EventArgs e)
        {
            if (macros.Count > 0)
            {
                if (cB360Controls.SelectedIndex > 0)
                    macros.Insert(0, cB360Controls.SelectedIndex + 260);
                kbm.macrostag = macros;
                foreach (ListViewItem lvi in lVMacros.Items)
                {
                    macronames.Add(lvi.Text);
                }
                kbm.macros = macronames;
                string macro = string.Join(", ", macronames.ToArray());
                kbm.lBMacroOn.Visible = true;
                if (cBStyle.SelectedIndex == 1)
                    kbm.macrorepeat = true;
                saved = true;
                Close();
            }
            else MessageBox.Show(Properties.Resources.NoMacroRecorded, "DS4Windows", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        
        private void lVMacros_MouseMove(object sender, MouseEventArgs e)
        {
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            saved = true;
            Close();
        }



        private void RecordBox_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!saved && macros.Count > 0)
                if (MessageBox.Show(Properties.Resources.SaveRecordedMacro, "DS4Windows", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == System.Windows.Forms.DialogResult.Yes)
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

        private void btn4th_Click(object sender, EventArgs e)
        {
            int value = 259;
            if (btn4th.Text.Contains(Properties.Resources.DownText))
            {
                if (macros.Count == 0)
                {
                    macros.Add(value);
                    lVMacros.Items.Add("4th Mouse Button", 0);
                    if (cBRecordDelays.Checked)
                    {
                        sw.Reset();
                        sw.Start();
                    }
                }
                else
                {
                    if (cBRecordDelays.Checked)
                    {
                        macros.Add((int)sw.ElapsedMilliseconds + 300);
                        lVMacros.Items.Add(Properties.Resources.WaitMS.Replace("*number*", sw.ElapsedMilliseconds.ToString()).Replace("*ms*", "ms"), 2);
                        sw.Reset();
                        sw.Start();
                    }
                    macros.Add(value);
                    lVMacros.Items.Add("4th Mouse Button", 0);
                }
                btn4th.Text = Properties.Resources.FourthMouseUp;
            }
            else
            {
                if (cBRecordDelays.Checked)
                {
                    macros.Add((int)sw.ElapsedMilliseconds + 300);
                    lVMacros.Items.Add(Properties.Resources.WaitMS.Replace("*number*", sw.ElapsedMilliseconds.ToString()).Replace("*ms*", "ms"), 2);
                    sw.Reset();
                    sw.Start();
                }
                macros.Add(value);
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
                if (macros.Count == 0)
                {
                    macros.Add(value);
                    lVMacros.Items.Add("5th Mouse Button", 0);
                    if (cBRecordDelays.Checked)
                    {
                        sw.Reset();
                        sw.Start();
                    }
                }
                else
                {
                    if (cBRecordDelays.Checked)
                    {
                        macros.Add((int)sw.ElapsedMilliseconds + 300);
                        lVMacros.Items.Add(Properties.Resources.WaitMS.Replace("*number*", sw.ElapsedMilliseconds.ToString()).Replace("*ms*", "ms"), 2);
                        sw.Reset();
                        sw.Start();
                    }
                    macros.Add(value);
                    lVMacros.Items.Add("5th Mouse Button", 0);
                }
                btn5th.Text = Properties.Resources.FifthMouseUp;
            }
            else
            {
                if (cBRecordDelays.Checked)
                {
                    macros.Add((int)sw.ElapsedMilliseconds + 300);
                    lVMacros.Items.Add(Properties.Resources.WaitMS.Replace("*number*", sw.ElapsedMilliseconds.ToString()).Replace("*ms*", "ms"), 2);
                    sw.Reset();
                    sw.Start();
                }
                macros.Add(value);
                lVMacros.Items.Add("5th Mouse Button", 1);
                btn5th.Text = Properties.Resources.FifthMouseDown;
            }
            lVMacros.Items[lVMacros.Items.Count - 1].EnsureVisible();
        }
    }
}
