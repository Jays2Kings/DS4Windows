using System;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using DS4Control;
using System.Threading;
namespace ScpServer
{
    public partial class ScpForm : Form
    {
        private DS4Control.Control rootHub;
        delegate void LogDebugDelegate(DateTime Time, String Data);

        protected void LogDebug(DateTime Time, String Data)
        {
            if (lvDebug.InvokeRequired)
            {
                LogDebugDelegate d = new LogDebugDelegate(LogDebug);
                try
                {
                    this.Invoke(d, new Object[] { Time, Data });
                }
                catch { }
            }
            else
            {
                String Posted = Time.ToString() + "." + Time.Millisecond.ToString("000");

                lvDebug.Items.Add(new ListViewItem(new String[] { Posted, Data })).EnsureVisible();

                //Added alternative
                lbLastMessage.Text = Data;
            }
        }

        protected void ShowNotification(object sender, DebugEventArgs args)
        {
            notifyIcon1.BalloonTipText = args.Data;
            notifyIcon1.BalloonTipTitle = "DS4 Tool";
            notifyIcon1.ShowBalloonTip(1);
        }

        protected void Form_Resize(object sender, EventArgs e)
        {
            if (FormWindowState.Minimized == this.WindowState)
            {
                notifyIcon1.Visible = true;
                this.Hide();
                //hide in taskbar
                this.ShowInTaskbar = false;
            }
            else if (FormWindowState.Normal == this.WindowState)
            {
                notifyIcon1.Visible = false;
                //show in taskbar
                this.ShowInTaskbar = true;

            }
            //Added last message alternative
            if (this.Height > 220)
                lbLastMessage.Visible = false;
            else lbLastMessage.Visible = true;
        }

        protected RadioButton[] Pad = new RadioButton[4];

        public ScpForm()
        {
            InitializeComponent();

            ThemeUtil.SetTheme(lvDebug);

            Pad[0] = rbPad_1;
            Pad[1] = rbPad_2;
            Pad[2] = rbPad_3;
            Pad[3] = rbPad_4;
        }

        protected void Form_Load(object sender, EventArgs e)
        {
            Icon = Properties.Resources.DS4;
            notifyIcon1.Icon = Properties.Resources.DS4;
            rootHub = new DS4Control.Control();
            rootHub.Debug += On_Debug;
            Log.GuiLog += On_Debug;
            Log.TrayIconLog += ShowNotification;
            tmrUpdate.Enabled = true;
            Global.Load();
            hideDS4CheckBox.CheckedChanged -= hideDS4CheckBox_CheckedChanged;
            hideDS4CheckBox.Checked = Global.getUseExclusiveMode();
            hideDS4CheckBox.CheckedChanged += hideDS4CheckBox_CheckedChanged;
            if (btnStartStop.Enabled)
                btnStartStop_Click(sender, e);

            // New settings
            this.Width = Global.getFormWidth();
            this.Height = Global.getFormHeight();
            startMinimizedCheckBox.CheckedChanged -= startMinimizedCheckBox_CheckedChanged;
            startMinimizedCheckBox.Checked = Global.getStartMinimized();
            startMinimizedCheckBox.CheckedChanged += startMinimizedCheckBox_CheckedChanged;

            if (startMinimizedCheckBox.Checked)
            {
                this.WindowState = FormWindowState.Minimized;
                Form_Resize(sender, e);
            }
            Global.loadCustomMapping(0);


        }
        protected void Form_Close(object sender, FormClosingEventArgs e)
        {
            Global.setFormWidth(this.Width);
            Global.setFormHeight(this.Height);
            Global.Save();
            rootHub.Stop();
        }

        protected void btnStartStop_Click(object sender, EventArgs e)
        {
            if (btnStartStop.Text == Properties.Resources.Start
                && rootHub.Start())
                btnStartStop.Text = Properties.Resources.Stop;
            else if (btnStartStop.Text == Properties.Resources.Stop
                && rootHub.Stop())
                btnStartStop.Text = Properties.Resources.Start;
        }
        protected void btnStop_Click(object sender, EventArgs e)
        {
            if (rootHub.Stop())
            {
                btnStartStop.Enabled = true;
                btnStop.Enabled = false;
            }
        }
        protected void btnClear_Click(object sender, EventArgs e)
        {
            lvDebug.Items.Clear();
            //Added alternative
            lbLastMessage.Text = string.Empty;
        }

        protected override void WndProc(ref Message m)
        {
            try
            {
                if (m.Msg == ScpDevice.WM_DEVICECHANGE)
                {
                    Int32 Type = m.WParam.ToInt32();
                    lock (this)
                    {
                        rootHub.HotPlug();
                    }
                }
            }
            catch { }

            base.WndProc(ref m);
        }
        protected void tmrUpdate_Tick(object sender, EventArgs e)
        {

            // If controllers are detected, but not checked, automatically check #1
            bool checkFirst = true;
            bool optionsEnabled = false;
            for (Int32 Index = 0; Index < Pad.Length; Index++)
            {
                string contollerInfo =  rootHub.getDS4ControllerInfo(Index);

                Pad[Index].Text = contollerInfo;
                if (Pad[Index].Text != null && Pad[Index].Text != "")
                {
                    Pad[Index].Enabled = true;
                    optionsEnabled = true;
                    // As above
                    if (checkFirst && (Pad[Index].Checked && Index != 0))
                        checkFirst = false;
                }
                else
                {
                    Pad[Index].Text = "Disconnected";
                    Pad[Index].Enabled = false;
                    Pad[Index].Checked = false;

                    // As above
                    if (Index == 0)
                        checkFirst = false;
                }
            }
            btnClear.Enabled = lvDebug.Items.Count > 0;

            // As above
            if (checkFirst && btnClear.Enabled)
                Pad[0].Checked = true;
            optionsButton.Enabled = optionsEnabled;
        }
        protected void On_Debug(object sender, DS4Control.DebugEventArgs e)
        {
            LogDebug(e.Time, e.Data);
        }

        private void optionsButton_Click(object sender, EventArgs e)
        {
            for (Int32 Index = 0; Index < Pad.Length; Index++)
            {
                if (Pad[Index].Checked)
                {
                    Options opt = new Options(rootHub, Index);
                    opt.Text = "Options for Controller " + (Index + 1);
                    opt.Icon = this.Icon;
                    opt.ShowDialog();
                }
            }
        }
        private void notifyIcon_Click(object sender, EventArgs e)
        {
            this.Show();
            WindowState = FormWindowState.Normal;
        }
        private void hotkeysButton_Click(object sender, EventArgs e)
        {
            Hotkeys hotkeysForm = new Hotkeys();
            hotkeysForm.Icon = this.Icon;
            hotkeysForm.ShowDialog();
        }
        private void lnkControllers_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("control", "joy.cpl");
        }

        private void hideDS4CheckBox_CheckedChanged(object sender, EventArgs e)
        {
            // Prevent the Game Controllers window from throwing an error when controllers are un/hidden
            System.Diagnostics.Process[] rundll32 = System.Diagnostics.Process.GetProcessesByName("rundll32");
            foreach (System.Diagnostics.Process rundll32Instance in rundll32)
                foreach (System.Diagnostics.ProcessModule module in rundll32Instance.Modules)
                    if (module.FileName.Contains("joy.cpl"))
                        module.Dispose();

            Global.setUseExclusiveMode(hideDS4CheckBox.Checked);
            btnStartStop_Click(sender, e);
            btnStartStop_Click(sender, e);
            Global.Save();
        }
        private void startMinimizedCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            Global.setStartMinimized(startMinimizedCheckBox.Checked);
            Global.Save();
        }

        private void AboutButton_Click(object sender, EventArgs e)
        {
            AboutBox box = new AboutBox();
            box.Icon = this.Icon;
            box.ShowDialog();
        }

        private void lvDebug_ItemActivate(object sender, EventArgs e)
        {
            MessageBox.Show(((ListView)sender).FocusedItem.SubItems[1].Text,"Log");
        }

    }

    public class ThemeUtil
    {
        [DllImport("UxTheme", CharSet = CharSet.Unicode, ExactSpelling = true)]
        private static extern int SetWindowTheme(IntPtr hWnd, String appName, String partList);

        public static void SetTheme(ListView lv)
        {
            try
            {
                SetWindowTheme(lv.Handle, "Explorer", null);
            }
            catch { }
        }

        public static void SetTheme(TreeView tv)
        {
            try
            {
                SetWindowTheme(tv.Handle, "Explorer", null);
            }
            catch { }
        }
    }
}
