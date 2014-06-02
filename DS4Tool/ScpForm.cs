using System;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using DS4Control;
using DS4Library;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using System.Net;
using System.Management;
using System.Drawing;
using Microsoft.Win32;
using System.Diagnostics;
namespace ScpServer
{
    public partial class ScpForm : Form
    {
        double version = 9.2;
        private DS4Control.Control rootHub;
        delegate void LogDebugDelegate(DateTime Time, String Data);

        protected Label[] Pads, Batteries;
        protected ComboBox[] cbs;
        protected Button[] ebns;
        protected PictureBox[] statPB;
        protected ToolStripMenuItem[] shortcuts;
        WebClient wc = new WebClient();
        Timer test = new Timer(), hotkeystimer = new Timer();
        #region Aero
        /*[StructLayout(LayoutKind.Sequential)]
        public struct MARGINS
        {
            public int Left;
            public int Right;
            public int Top;
            public int Bottom;
        }

        [DllImport("dwmapi.dll")]
        public static extern int DwmExtendFrameIntoClientArea(IntPtr hWnd, ref MARGINS pMargins);
        /// <summary>
        /// Determins whether the Desktop Windows Manager is enabled
        /// and can therefore display Aero 
        /// </summary>
        [DllImport("dwmapi.dll", PreserveSig = false)]
        public static extern bool DwmIsCompositionEnabled();

              
        /// <summary>
        /// Override the OnPaintBackground method, to draw the desired
        /// Glass regions black and display as Glass
        /// </summary>
        protected override void OnPaintBackground(PaintEventArgs e)
        {
            base.OnPaint(e);
            if (DwmIsCompositionEnabled())
            {
                e.Graphics.Clear(Color.Black);
                // put back the original form background for non-glass area
                Rectangle clientArea = new Rectangle(
                marg.Left,
                marg.Top,
                this.ClientRectangle.Width - marg.Left - marg.Right,
                this.ClientRectangle.Height - marg.Top - marg.Bottom);
                Brush b = new SolidBrush(this.BackColor);
                e.Graphics.FillRectangle(b, clientArea);
            }
        }

        MARGINS marg = new MARGINS() { Left = 0, Right = 0, Top = 0, Bottom = 0 };
        /// <summary>
        /// Use the form padding values to define a Glass margin
        /// </summary>
        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            this.Padding = new Padding(this.trackBar1.Value);
            int value = (int)trackBar1.Value;
            marg = new MARGINS() { Left = value, Right = value, Top = value, Bottom = value };
            DwmExtendFrameIntoClientArea(this.Handle, ref marg);
            //SetGlassRegion();
            //Invalidate();
        }*/
        #endregion

        protected void SetupArrays()
        {
            Pads = new Label[4] { lbPad1, lbPad2, lbPad3, lbPad4 };
            Batteries = new Label[4] { lBBatt1, lBBatt2, lBBatt3, lBBatt4 };
            cbs = new ComboBox[4] { cBController1, cBController2, cBController3, cBController4 };
            ebns = new Button[4] { bnEditC1, bnEditC2, bnEditC3, bnEditC4 };
            statPB = new PictureBox[4] { pBStatus1, pBStatus2, pBStatus3, pBStatus4 };

            shortcuts = new ToolStripMenuItem[4] { (ToolStripMenuItem)notifyIcon1.ContextMenuStrip.Items[0],
                (ToolStripMenuItem)notifyIcon1.ContextMenuStrip.Items[1],
                (ToolStripMenuItem)notifyIcon1.ContextMenuStrip.Items[2],
                (ToolStripMenuItem)notifyIcon1.ContextMenuStrip.Items[3] };
        }
        public ScpForm()
        {
            InitializeComponent();

            ThemeUtil.SetTheme(lvDebug);

            SetupArrays();
            CheckDrivers();                 
        }

        protected void Form_Load(object sender, EventArgs e)
        {
            Icon = Properties.Resources.DS4;
            notifyIcon1.Icon = Properties.Resources.DS4;
            rootHub = new DS4Control.Control();
            rootHub.Debug += On_Debug;
            Log.GuiLog += On_Debug;
            Log.TrayIconLog += ShowNotification;
            // tmrUpdate.Enabled = true; TODO remove tmrUpdate and leave tick()
            Global.Load();
            Global.setVersion(version);
            Global.Save();

            foreach (ToolStripMenuItem t in shortcuts)
                t.DropDownItemClicked += Profile_Changed_Menu;
            hideDS4CheckBox.CheckedChanged -= hideDS4CheckBox_CheckedChanged;
            hideDS4CheckBox.Checked = Global.getUseExclusiveMode();
            hideDS4CheckBox.CheckedChanged += hideDS4CheckBox_CheckedChanged;

            // New settings
            this.Width = Global.getFormWidth();
            this.Height = Global.getFormHeight();
            startMinimizedCheckBox.CheckedChanged -= startMinimizedCheckBox_CheckedChanged;
            startMinimizedCheckBox.Checked = Global.getStartMinimized();
            startMinimizedCheckBox.CheckedChanged += startMinimizedCheckBox_CheckedChanged;

            RegistryKey KeyLoc = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", false);
            StartWindowsCheckBox.Checked = (KeyLoc.GetValue("DS4Tool") != null);

            SetupArrays();
            if (startMinimizedCheckBox.Checked)
            {
                this.WindowState = FormWindowState.Minimized;
                Form_Resize(sender, e);
            }
            RefreshProfiles();
            for (int i = 0; i < 4; i++)
                Global.LoadProfile(i);
            Global.ControllerStatusChange += ControllerStatusChange;
            ControllerStatusChanged();
            if (btnStartStop.Enabled)
                btnStartStop_Clicked();
            Uri url = new Uri("https://dl.dropboxusercontent.com/u/16364552/DS4Tool/newest%20version.txt"); //Sorry other devs, gonna have to find your own server
            Directory.CreateDirectory(Global.appdatapath);
            if (DateTime.Now >= Global.getLastChecked() + TimeSpan.FromHours(1))
            {
                wc.DownloadFileAsync(url, Global.appdatapath + "\\version.txt");
                wc.DownloadFileCompleted += Check_Version;
                Global.setLastChecked(DateTime.Now);
            }
            WinProgs WP = new WinProgs(profilenames.ToArray());
            WP.TopLevel = false;
            WP.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            WP.Visible = true;
            WP.Dock = DockStyle.Fill;
            WP.Enabled = false;
            tabAutoProfiles.Controls.Add(WP);
            //test.Start();
            hotkeystimer.Start();
            hotkeystimer.Tick += Hotkeys;
            test.Tick += test_Tick;    
        }


        void Hotkeys(object sender, EventArgs e)
        {
            for (int i = 0; i < 4; i++)
            {
                string slide = rootHub.TouchpadSlide(i);
                if (slide == "left")
                    if (cbs[i].SelectedIndex <= 0)
                        cbs[i].SelectedIndex = cbs[i].Items.Count - 2;
                    else
                        cbs[i].SelectedIndex--;
                else if (slide == "right")
                    if (cbs[i].SelectedIndex == cbs[i].Items.Count - 2)
                        cbs[i].SelectedIndex = 0;
                    else
                        cbs[i].SelectedIndex++;
                if (slide.Contains("t"))
                    ShowNotification(this, "Controller " + (i + 1) + " is now using Profile \"" + cbs[i].Text + "\"");
            }
        }

        private void test_Tick(object sender, EventArgs e)
        {
            label1.Visible = true;
            int speed = Global.getButtonMouseSensitivity(0);
            label1.Text = (((rootHub.getDS4State(0).RX - 127) / 127d) * speed).ToString() + "and " + Mapping.mvalue;
            /*label1.Text = Mapping.globalState.currentClicks.toggle.ToString() + " Left is " + 
                Mapping.getBoolMapping(DS4Controls.DpadLeft, rootHub.getDS4State(0)) + 
                " Toggle is " + Mapping.pressedonce[256] +
                Mapping.test;*/
        }

        private void CheckDrivers()
        {
            bool deriverinstalled = false;
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PnPSignedDriver");
            foreach (ManagementObject obj in searcher.Get())
            {
                try
                {
                    if (obj.GetPropertyValue("DeviceName").ToString() == "Scp Virtual Bus Driver")
                    {
                        deriverinstalled = true;
                        break;
                    }
                }
                catch { }
            }
            if (!deriverinstalled)
            {
                WelcomeDialog wd = new WelcomeDialog();
                wd.ShowDialog();
            }
        }

        private void linkLabel1_LinkClicked_1(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Uri url = new Uri("https://dl.dropboxusercontent.com/u/16364552/DS4Tool/newest%20version.txt"); //Sorry other devs, gonna have to find your own server
            wc.DownloadFile(url, Global.appdatapath + "\\version.txt");
            Global.setLastChecked(DateTime.Now);
            double newversion;
            try
            {
                if (double.TryParse(File.ReadAllText(Global.appdatapath + "\\version.txt"), out newversion))
                    if (newversion > version)
                        if (MessageBox.Show("Download now?", "DS4Windows Update Available!", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == System.Windows.Forms.DialogResult.Yes)
                        {
                            if (!File.Exists("Updater.exe"))
                            {
                                Uri url2 = new Uri("https://dl.dropboxusercontent.com/u/16364552/DS4Tool/Updater.exe");
                                WebClient wc2 = new WebClient();
                                wc2.DownloadFile(url2, "Updater.exe");
                            }
                            System.Diagnostics.Process.Start("Updater.exe");
                            this.Close();
                        }
                        else
                            File.Delete(Global.appdatapath + "\\version.txt");
                    else
                    {
                        File.Delete(Global.appdatapath + "\\version.txt");
                        MessageBox.Show("No new version", "You're up to date");
                    }
                else
                    File.Delete(Global.appdatapath + "\\version.txt");
            }
            catch { };
        }

        private void Check_Version(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            double newversion;
            try
            {
                if (double.TryParse(File.ReadAllText(Global.appdatapath + "\\version.txt"), out newversion))
                    if (newversion > version)
                        if (MessageBox.Show("Download now?", "DS4Windows Update Available!", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == System.Windows.Forms.DialogResult.Yes)
                        {
                            if (!File.Exists("Updater.exe"))
                            {
                                Uri url2 = new Uri("https://dl.dropboxusercontent.com/u/16364552/DS4Tool/Updater.exe");
                                WebClient wc2 = new WebClient();
                                wc2.DownloadFile(url2, "Updater.exe");
                            }
                            System.Diagnostics.Process.Start("Updater.exe");
                            this.Close();
                        }
                        else
                            File.Delete(Global.appdatapath + "\\version.txt");
                    else
                        File.Delete(Global.appdatapath + "\\version.txt");
                else
                    File.Delete(Global.appdatapath + "\\version.txt");
            }
            catch { };
        }
        
        List<string> profilenames = new List<string>();
        public void RefreshProfiles()
        {
            try
            {
                profilenames.Clear();
                string[] profiles = Directory.GetFiles(Global.appdatapath + @"\Profiles\");                
                foreach (String s in profiles)
                    if (s.EndsWith(".xml"))
                        profilenames.Add(Path.GetFileNameWithoutExtension(s));
                lBProfiles.Items.Clear();
                lBProfiles.Items.AddRange(profilenames.ToArray());
                for (int i = 0; i < 4; i++)
                {
                    cbs[i].Items.Clear();
                    shortcuts[i].DropDownItems.Clear();
                    cbs[i].Items.AddRange(profilenames.ToArray());
                    foreach (string s in profilenames)
                        shortcuts[i].DropDownItems.Add(Path.GetFileNameWithoutExtension(s));
                    for (int j = 0; j < cbs[i].Items.Count; j++)
                        if (cbs[i].Items[j].ToString() == Path.GetFileNameWithoutExtension(Global.getAProfile(i)))
                        {
                            cbs[i].SelectedIndex = j;
                            ((ToolStripMenuItem)shortcuts[i].DropDownItems[j]).Checked = true;
                            Global.setAProfile(i, cbs[i].Text);
                            shortcuts[i].Text = "Edit Profile for Controller " + (i + 1);
                            ebns[i].Text = "Edit";
                            break;
                        }
                        else
                        {
                            cbs[i].Text = "(No Profile Loaded)";
                            shortcuts[i].Text = "Make Profile for Controller " + (i + 1);
                            ebns[i].Text = "New";
                        }
                }
            }
            catch (DirectoryNotFoundException)
            {
                if (Directory.Exists(Directory.GetParent(Assembly.GetExecutingAssembly().Location).FullName + @"\Profiles\"))
                    MessageBox.Show("Please import or make a profile", "Profile Folder Moved");
                Directory.CreateDirectory(Global.appdatapath + @"\Profiles\");
                for (int i = 0; i < 4; i++)
                {
                    cbs[i].Text = "(No Profile Loaded)";
                    shortcuts[i].Text = "Make Profile for Controller " + (i + 1);
                    ebns[i].Text = "New";
                }
            }
            finally
            {
                for (int i = 0; i < 4; i++)
                {
                    cbs[i].Items.Add("+New Profile");
                    shortcuts[i].DropDownItems.Add("-");
                    shortcuts[i].DropDownItems.Add("+New Profile");
                }
            }
        }

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
                String Posted = Time.ToString("G");

                lvDebug.Items.Add(new ListViewItem(new String[] { Posted, Data })).EnsureVisible();

                //Added alternative
                lbLastMessage.Text = Data;
            }
        }

        protected void ShowNotification(object sender, DebugEventArgs args)
        {
            if (Form.ActiveForm != this)
            {
                this.notifyIcon1.BalloonTipText = args.Data;
                notifyIcon1.BalloonTipTitle = "DS4Windows";
                notifyIcon1.ShowBalloonTip(1); 
            }
        }

        protected void ShowNotification(object sender, string text)
        {
            if (Form.ActiveForm != this)
            {
                this.notifyIcon1.BalloonTipText = text;
                notifyIcon1.BalloonTipTitle = "DS4Windows";
                notifyIcon1.ShowBalloonTip(1);
            }
        }

        protected void Form_Resize(object sender, EventArgs e)
        {
            if (FormWindowState.Minimized == this.WindowState)
            {
                this.Hide();
                this.ShowInTaskbar = false;
            }

            else if (FormWindowState.Normal == this.WindowState)
            {
                this.Show();
                this.ShowInTaskbar = true;
            }

            //Added last message alternative

            if (this.Height > 220)
                lbLastMessage.Visible = tabMain.SelectedIndex != 2;
            else lbLastMessage.Visible = true;
        }

        protected void btnStartStop_Click(object sender, EventArgs e)
        {
            btnStartStop_Clicked();
        }
        protected void btnStartStop_Clicked()
        {
            if (btnStartStop.Text == "Start")
            {
                rootHub.Start();
                btnStartStop.Text = "Stop";
            }

            else if (btnStartStop.Text == "Stop")
            {                
                rootHub.Stop();
                btnStartStop.Text = "Start";
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

        delegate void ControllerStatusChangedDelegate(object sender, EventArgs e);
        protected void ControllerStatusChange(object sender, EventArgs e)
        {
            if (InvokeRequired)
                Invoke(new ControllerStatusChangedDelegate(ControllerStatusChange), new object[] { sender, e });
            else
                ControllerStatusChanged();
        }
        protected void ControllerStatusChanged()
        {
            String tooltip = "DS4Windows v" + version;
            for (Int32 Index = 0; Index < Pads.Length; Index++)
            {
                Pads[Index].Text = rootHub.getDS4MacAddress(Index);
                switch (rootHub.getDS4Status(Index))
                {
                    case "USB": statPB[Index].Image = Properties.Resources.USB; break;
                    case "BT": statPB[Index].Image = Properties.Resources.BT; break;
                    default: statPB[Index].Image = Properties.Resources.none; break;
                }
                Batteries[Index].Text = rootHub.getDS4Battery(Index);
                if (Pads[Index].Text != String.Empty)
                {
                    Pads[Index].Enabled = true;
                    if (Pads[Index].Text != "Connecting...")
                    {
                        Enable_Controls(Index, true);
                        MinimumSize = new Size(MinimumSize.Width, 161 + 29 * Index);
                    }
                }
                else
                {
                    Pads[Index].Text = "Disconnected";
                    Enable_Controls(Index, false);
                    shortcuts[Index].Enabled = false;
                }
                if (rootHub.getShortDS4ControllerInfo(Index) != "None")
                    tooltip += "\n" + (Index + 1) + ": " + rootHub.getShortDS4ControllerInfo(Index); // Carefully stay under the 63 character limit.
            }
            btnClear.Enabled = lvDebug.Items.Count > 0;
            notifyIcon1.Text = tooltip;
        }


        private void Enable_Controls(int device, bool on)
        {
            Pads[device].Enabled = on;
            ebns[device].Enabled = on;
            cbs[device].Enabled = on;
            shortcuts[device].Enabled = on;
            Batteries[device].Enabled = on;
        }
        delegate void HotKeysDelegate(object sender, EventArgs e);
        void ScpForm_Report(object sender, EventArgs e)
        {
            if (InvokeRequired)
                Invoke(new HotKeysDelegate(Hotkeys), new object[] { sender, e });
            else
                Hotkeys(sender, e);
        }

        protected void On_Debug(object sender, DS4Control.DebugEventArgs e)
        {
            LogDebug(e.Time, e.Data);
        }


        private void lBProfiles_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (lBProfiles.SelectedIndex >= 0)
            ShowOptions(4, lBProfiles.SelectedItem.ToString());
        }


        private void lBProfiles_KeyDown(object sender, KeyEventArgs e)
        {
            if (lBProfiles.SelectedIndex >= 0 && opt == null)
            {
                if (e.KeyValue == 13)
                    ShowOptions(4, lBProfiles.SelectedItem.ToString());
                if (e.KeyValue == 46)
                    tsBDeleteProfle_Click(this, e);
                if (e.KeyValue == 67 && e.Modifiers == Keys.Control)
                    tSBDupProfile_Click(this, e);
            }

        }

        private void assignToController1ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            cbs[0].SelectedIndex = lBProfiles.SelectedIndex;
        }

        private void assignToController2ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            cbs[1].SelectedIndex = lBProfiles.SelectedIndex;
        }

        private void assignToController3ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            cbs[2].SelectedIndex = lBProfiles.SelectedIndex;
        }

        private void assignToController4ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            cbs[3].SelectedIndex = lBProfiles.SelectedIndex;
        }

        private void tsBNewProfile_Click(object sender, EventArgs e) //Also used for context menu
        {
            ShowOptions(4, "");
        }


        private void tsBNEditProfile_Click(object sender, EventArgs e)
        {
            if (lBProfiles.SelectedIndex >= 0)
                ShowOptions(4, lBProfiles.SelectedItem.ToString());
        }

        private void tsBDeleteProfle_Click(object sender, EventArgs e)
        {
            if (lBProfiles.SelectedIndex >= 0)
            {
                string filename = lBProfiles.SelectedItem.ToString();
                if (MessageBox.Show("\"" + filename + "\" cannot be restored.", "Delete Profile?", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == System.Windows.Forms.DialogResult.Yes)
                {
                    System.IO.File.Delete(Global.appdatapath + "\\Profiles\\" + filename + ".xml");
                    RefreshProfiles();
                }
            }
        }

        
        private void tSBDupProfile_Click(object sender, EventArgs e)
        {
            string filename = "";
            if (lBProfiles.SelectedIndex >= 0)
            { 
                filename = lBProfiles.SelectedItem.ToString();
                MessageTextBox MTB = new MessageTextBox(filename, this);
                MTB.TopLevel = false;
                MTB.Dock = DockStyle.Top;
                MTB.Visible = true;
                MTB.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
                tabProfiles.Controls.Add(MTB);
                lBProfiles.SendToBack();
                toolStrip1.SendToBack();
                toolStrip1.Enabled = false;
                lBProfiles.Enabled = false;
                MTB.FormClosed += delegate { toolStrip1.Enabled = true; lBProfiles.Enabled = true; };
            }
        }



        private void tSBImportProfile_Click(object sender, EventArgs e)
        {
            openProfiles.InitialDirectory = Directory.GetParent(Assembly.GetExecutingAssembly().Location).FullName + @"\Profiles\";
            if (openProfiles.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string[] files = openProfiles.FileNames;
                for (int i = 0; i < files.Length; i++)
                    File.Copy(openProfiles.FileNames[i], Global.appdatapath + "\\Profiles\\" + Path.GetFileName(files[i]), true);
                RefreshProfiles();
            }
        }

        private void tSBExportProfile_Click(object sender, EventArgs e)
        {
            if (lBProfiles.SelectedIndex >= 0)
            {
                Stream stream;
                Stream profile = new StreamReader(Global.appdatapath + "\\Profiles\\" + lBProfiles.SelectedItem.ToString() + ".xml").BaseStream;
                if (saveProfiles.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    if ((stream = saveProfiles.OpenFile()) != null)
                    {
                        profile.CopyTo(stream);
                        profile.Close();
                        stream.Close();
                    }
            }
        }

        Options opt;
        private System.Drawing.Size oldsize;
        private void ShowOptions(int devID, string profile)
        {
            if (opt == null)
            {
                this.Show();
                WindowState = FormWindowState.Normal;
                toolStrip1.Enabled = false;
                opt = new Options(rootHub, devID, profile, this);
                opt.Text = "Options for Controller " + (devID + 1);
                opt.Icon = this.Icon;
                opt.TopLevel = false;
                opt.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
                opt.Visible = true;
                opt.Dock = DockStyle.Fill;
                tabProfiles.Controls.Add(opt);
                lBProfiles.SendToBack();
                toolStrip1.SendToBack();
                FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
                opt.FormClosed += delegate 
                { 
                    opt = null;
                    RefreshProfiles();
                    FormBorderStyle = System.Windows.Forms.FormBorderStyle.Sizable;
                    this.Size = oldsize;
                    oldsize = new System.Drawing.Size(0, 0);
                    toolStrip1.Enabled = true;
                };
                oldsize = this.Size;
                if (this.Size.Height < 442)
                    this.Size = new System.Drawing.Size(this.Size.Width, 442);
                if (this.Size.Width < 910)
                    this.Size = new System.Drawing.Size(910, this.Size.Height);
                tabMain.SelectedIndex = 1;
            }
        }
        private void editButtons_Click(object sender, EventArgs e)
        {
            Button bn = (Button)sender;
            int i = Int32.Parse(bn.Tag.ToString());
            if (cbs[i].Text == "(No Profile Loaded)")
                    ShowOptions(i, "");
                else
                    ShowOptions(i, cbs[i].Text);
        }
        private void editMenu_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem em = (ToolStripMenuItem)sender;
            int i = Int32.Parse(em.Tag.ToString());
                if (em.Text == "Make Profile for Controller " + (i + 1))
                    ShowOptions(i, "");
                else
                    for (int t=0; t < em.DropDownItems.Count-2; t++)
                        if (((ToolStripMenuItem)em.DropDownItems[t]).Checked)
                            ShowOptions(i, ((ToolStripMenuItem)em.DropDownItems[t]).Text);
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
            btnStartStop_Clicked();
            btnStartStop_Clicked();
            Global.Save();
        }
        private void startMinimizedCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            Global.setStartMinimized(startMinimizedCheckBox.Checked);
            Global.Save();
        }

        private void lvDebug_ItemActivate(object sender, EventArgs e)
        {
            MessageBox.Show(((ListView)sender).FocusedItem.SubItems[1].Text, "Log");
        }

        private void Profile_Changed(object sender, EventArgs e) //cbs[i] changed
        {
            ComboBox cb = (ComboBox)sender;
            int tdevice = Int32.Parse(cb.Tag.ToString());
            if (cb.Items[cb.Items.Count - 1].ToString() == "+New Profile")
            {
                if (cb.SelectedIndex < cb.Items.Count - 1)
                {
                    for (int i = 0; i < shortcuts[tdevice].DropDownItems.Count; i++)
                        if (!(shortcuts[tdevice].DropDownItems[i] is ToolStripSeparator))
                            ((ToolStripMenuItem)shortcuts[tdevice].DropDownItems[i]).Checked = false;
                    ((ToolStripMenuItem)shortcuts[tdevice].DropDownItems[cb.SelectedIndex]).Checked = true;
                    LogDebug(DateTime.Now, "Controller " + (tdevice + 1) + " is now using Profile \"" + cb.Text + "\"");
                    shortcuts[tdevice].Text = "Edit Profile for Controller " + (tdevice + 1);
                    Global.setAProfile(tdevice, cb.Items[cb.SelectedIndex].ToString());
                    Global.Save();
                    Global.LoadProfile(tdevice);
                }
                else if (cb.SelectedIndex == cb.Items.Count - 1 && cb.Items.Count > 1) //if +New Profile selected
                    ShowOptions(tdevice, "");
                if (cb.Text == "(No Profile Loaded)")
                    ebns[tdevice].Text = "New";
                else
                    ebns[tdevice].Text = "Edit";
            }
            ControllerStatusChanged(); //to update profile name in notify icon
        }

        private void Profile_Changed_Menu(object sender, ToolStripItemClickedEventArgs e)
        {
            ToolStripMenuItem tS = (ToolStripMenuItem)sender;
            int tdevice = Int32.Parse(tS.Tag.ToString());
            if (!(e.ClickedItem is ToolStripSeparator))
                if (e.ClickedItem != tS.DropDownItems[tS.DropDownItems.Count - 1]) //if +New Profile not selected 
                    cbs[tdevice].SelectedIndex = tS.DropDownItems.IndexOf(e.ClickedItem);
                else //if +New Profile selected
                    ShowOptions(tdevice, "");
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Show();
            WindowState = FormWindowState.Normal;
        }        

        private void notifyIcon1_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                this.Show();
                WindowState = FormWindowState.Normal;
            }
            else if (e.Button == System.Windows.Forms.MouseButtons.Middle)
                this.Close();
        }
        private void notifyIcon1_BalloonTipClicked(object sender, EventArgs e)
        {
            this.Show();
            WindowState = FormWindowState.Normal;
        }

        private void llbHelp_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Hotkeys hotkeysForm = new Hotkeys(this);
            hotkeysForm.Icon = this.Icon;
            hotkeysForm.ShowDialog();
        }

        private void StartWindowsCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            RegistryKey KeyLoc = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            if (StartWindowsCheckBox.Checked)
                KeyLoc.SetValue("DS4Tool", "\"" + Application.ExecutablePath.ToString() + "\"");
            else
                KeyLoc.DeleteValue("DS4Tool", false);
        }

        private void tabMain_SelectedIndexChanged(object sender, EventArgs e)
        {
            lbLastMessage.Visible = tabMain.SelectedIndex != 2;
            if (tabMain.SelectedIndex == 3 && opt == null)
            {
                if (this.Size.Width < 755 || this.Size.Height < 340)
                    oldsize = Size;
                if (this.Size.Height < 340)
                    this.Size = new System.Drawing.Size(this.Size.Width, 340);
                if (this.Size.Width < 755)
                    this.Size = new System.Drawing.Size(755, this.Size.Height);
                
            }
            else if (oldsize != new System.Drawing.Size(0, 0) && opt == null)
            {
                Size = oldsize;
                oldsize = new System.Drawing.Size(0, 0);
            }

        }

        private void lBProfiles_MouseDown(object sender, MouseEventArgs e)
        {
            lBProfiles.SelectedIndex = lBProfiles.IndexFromPoint(e.X, e.Y);
            if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                if (lBProfiles.SelectedIndex < 0)
                {
                    cMProfile.ShowImageMargin = false;
                    assignToController1ToolStripMenuItem.Visible = false;
                    assignToController2ToolStripMenuItem.Visible = false;
                    assignToController3ToolStripMenuItem.Visible = false;
                    assignToController4ToolStripMenuItem.Visible = false;
                    deleteToolStripMenuItem.Visible = false;
                    editToolStripMenuItem.Visible = false;
                    duplicateToolStripMenuItem.Visible = false;
                    exportToolStripMenuItem.Visible = false;
                }
                else
                {
                    cMProfile.ShowImageMargin = true;
                    assignToController1ToolStripMenuItem.Visible = true;
                    assignToController2ToolStripMenuItem.Visible = true;
                    assignToController3ToolStripMenuItem.Visible = true;
                    assignToController4ToolStripMenuItem.Visible = true;
                    ToolStripMenuItem[] assigns = { assignToController1ToolStripMenuItem, 
                                                      assignToController2ToolStripMenuItem,
                                                      assignToController3ToolStripMenuItem, 
                                                      assignToController4ToolStripMenuItem };
                    for (int i = 0; i < 4; i++)
                    {
                        if (lBProfiles.SelectedIndex == cbs[i].SelectedIndex)
                            assigns[i].Checked = true;
                        else
                            assigns[i].Checked = false;
                    }
                    deleteToolStripMenuItem.Visible = true;
                    editToolStripMenuItem.Visible = true;
                    duplicateToolStripMenuItem.Visible = true;
                    exportToolStripMenuItem.Visible = true;                
                }
            }
        }

        private void ScpForm_DragDrop(object sender, DragEventArgs e)
        {
            bool therewasanxml = false;
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop, false);
            for (int i = 0; i < files.Length; i++)
                if (files[i].EndsWith(".xml"))
                {
                    File.Copy(files[i], Global.appdatapath + "\\Profiles\\" + Path.GetFileName(files[i]), true);
                    therewasanxml = true;
                }
            if (therewasanxml)
                RefreshProfiles();
        }

        private void ScpForm_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.Copy; // Okay
            else
                e.Effect = DragDropEffects.None; // Unknown data, ignore it
        }

        protected void Form_Close(object sender, FormClosingEventArgs e)
        {
            if (oldsize == new System.Drawing.Size(0, 0))
            {
                Global.setFormWidth(this.Width);
                Global.setFormHeight(this.Height);
            }
            else
            {
                Global.setFormWidth(oldsize.Width);
                Global.setFormHeight(oldsize.Height);
            }
            Global.Save();
            rootHub.Stop();
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
