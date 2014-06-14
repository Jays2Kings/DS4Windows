using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
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
            if (kbm.macrorepeat)
                cBStyle.SelectedIndex = 1;
            else
                cBStyle.SelectedIndex = 0;
        }

        private void btnRecord_Click(object sender, EventArgs e)
        {
            if (btnRecord.Text == "Record")
            {
                if (cBRecordDelays.Checked)
                    sw.Start();
                macros.Clear();
                lVMacros.Items.Clear();
                btnRecord.Text = "Stop";
                EnableControls(false);
                ActiveControl = null;
                lVMacros.Focus();
            }
            else
            {
                if (cBRecordDelays.Checked)
                    sw.Reset();
                btnRecord.Text = "Record";
                EnableControls(true);
            }
        }

        private void EnableControls(bool on)
        {
            cBRecordDelays.Enabled = on;
            lVMacros.Enabled = on;
            cBStyle.Enabled = on;
            btnCancel.Enabled = on;
            btnSave.Enabled = on;
        }

        private void anyKeyDown(object sender, KeyEventArgs e)
        {
            if (btnRecord.Text == "Stop")
            {
                int count = 0;
                foreach (int i in macros)
                {
                    if (i == e.KeyValue)
                        count++;
                }
                if (macros.Count == 0)
                {
                    macros.Add(e.KeyValue);
                    lVMacros.Items.Add(e.KeyCode.ToString(), 0);
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
                        lVMacros.Items.Add("Wait " + sw.ElapsedMilliseconds + "ms", 2);
                        sw.Reset();
                        sw.Start();
                    }
                    macros.Add(e.KeyValue);
                    lVMacros.Items.Add(e.KeyCode.ToString(), 0);
                }
            }
            else
            {
                if (e.KeyValue == 27)
                    Close();
            }
        }

        private void anyKeyUp(object sender, KeyEventArgs e)
        {
            if (btnRecord.Text == "Stop" && macros.Count != 0 && !e.KeyCode.ToString().Contains("Media"))
            {
                if (cBRecordDelays.Checked)
                {
                    macros.Add((int)sw.ElapsedMilliseconds + 300);
                    lVMacros.Items.Add("Wait " + sw.ElapsedMilliseconds + "ms", 2);
                    macros.Add(e.KeyValue);
                    sw.Reset();
                    sw.Start();
                }
                else
                {
                    macros.Add(e.KeyValue);
                }
                lVMacros.Items.Add(e.KeyCode.ToString(), 1);
            }
        }

        private void anyMouseDown(object sender, MouseEventArgs e)
        {
            if (btnRecord.Text == "Stop")
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
                        lVMacros.Items.Add("Wait " + sw.ElapsedMilliseconds + "ms", 2);
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
            }
        }

        private void anyMouseUp(object sender, MouseEventArgs e)
        {
            if (btnRecord.Text == "Stop" && macros.Count != 0)
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
            }
        }
        bool saved = false;
        private void btnSave_Click(object sender, EventArgs e)
        {
            if (macros.Count > 0)
            {
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
            else MessageBox.Show("No macro was recorded", "DS4Windows", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            saved = true;
            Close();
        }



        private void RecordBox_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!saved && macros.Count > 0)
                if (MessageBox.Show("Save Recorded Macro?", "DS4Windows", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == System.Windows.Forms.DialogResult.Yes)
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
