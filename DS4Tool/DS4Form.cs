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
using System.Xml;
using System.Text;
using System.Globalization;
using System.Threading.Tasks;
namespace DS4Windows
{
    public partial class DS4Form : Form
    {
        public string[] arguements;
        private DS4Control.Control rootHub;
        delegate void LogDebugDelegate(DateTime Time, String Data);

        protected Label[] Pads, Batteries;
        protected ComboBox[] cbs;
        protected Button[] ebns;
        protected PictureBox[] statPB;
        protected ToolStripMenuItem[] shortcuts;
        WebClient wc = new WebClient();
        Timer test = new Timer(), hotkeysTimer = new Timer();
        string exepath = Directory.GetParent(Assembly.GetExecutingAssembly().Location).FullName;
        string appdatapath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\DS4Tool";
        float dpix, dpiy;
        DateTime oldnow = DateTime.UtcNow;
        string tempprofile = "null";
        List<string> profilenames= new List<string>();
        List<string> programpaths = new List<string>();
        List<string>[] proprofiles;
        private static int WM_QUERYENDSESSION = 0x11;
        private static bool systemShutdown = false;
        private bool wasrunning = false;
        delegate void ControllerStatusChangedDelegate(object sender, EventArgs e);
        delegate void HotKeysDelegate(object sender, EventArgs e);
        Options opt;
        private System.Drawing.Size oldsize;
        WinProgs WP;
        ToolTip tt = new ToolTip();
        public String m_Profile = Directory.GetParent(Assembly.GetExecutingAssembly().Location).FullName + "\\Profiles.xml";
        protected XmlDocument m_Xdoc = new XmlDocument();
        public bool mAllowVisible; 

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport("kernel32.dll")]
        private static extern IntPtr OpenProcess(uint dwDesiredAccess, bool bInheritHandle, uint dwProcessId);

        [DllImport("kernel32.dll")]
        private static extern bool CloseHandle(IntPtr handle);

        [DllImport("psapi.dll")]
        private static extern uint GetModuleBaseName(IntPtr hWnd, IntPtr hModule, StringBuilder lpFileName, int nSize);

        [DllImport("psapi.dll")]
        private static extern uint GetModuleFileNameEx(IntPtr hWnd, IntPtr hModule, StringBuilder lpFileName, int nSize);

        public DS4Form(string[] args)
        {
            InitializeComponent();
            arguements = args;
            ThemeUtil.SetTheme(lvDebug);
            Pads = new Label[4] { lbPad1, lbPad2, lbPad3, lbPad4 };
            Batteries = new Label[4] { lBBatt1, lBBatt2, lBBatt3, lBBatt4 };
            cbs = new ComboBox[4] { cBController1, cBController2, cBController3, cBController4 };
            ebns = new Button[4] { bnEditC1, bnEditC2, bnEditC3, bnEditC4 };
            statPB = new PictureBox[4] { pBStatus1, pBStatus2, pBStatus3, pBStatus4 };

            shortcuts = new ToolStripMenuItem[4] { (ToolStripMenuItem)notifyIcon1.ContextMenuStrip.Items[0],
                (ToolStripMenuItem)notifyIcon1.ContextMenuStrip.Items[1],
                (ToolStripMenuItem)notifyIcon1.ContextMenuStrip.Items[2],
                (ToolStripMenuItem)notifyIcon1.ContextMenuStrip.Items[3] };
            SystemEvents.PowerModeChanged += OnPowerChange;
            tSOptions.Visible = false;
            if (File.Exists(appdatapath + "\\Profiles.xml"))
                tt.SetToolTip(linkUninstall, Properties.Resources.IfRemovingDS4Windows);
            tt.SetToolTip(cBSwipeProfiles, Properties.Resources.TwoFingerSwipe);
            tt.SetToolTip(cBQuickCharge, Properties.Resources.QuickCharge);
            bool firstrun = false;
            if (File.Exists(exepath + "\\Auto Profiles.xml")
                && File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\DS4Tool\\Auto Profiles.xml"))
            {
                firstrun = true;
                new SaveWhere(true).ShowDialog();
            }
            else if (File.Exists(exepath + "\\Auto Profiles.xml"))
                Global.SaveWhere(exepath);
            else if (File.Exists(appdatapath + "\\Auto Profiles.xml"))
                Global.SaveWhere(appdatapath);
            else if (!File.Exists(exepath + "\\Auto Profiles.xml")
                && !File.Exists(appdatapath + "\\Auto Profiles.xml"))
            {
                firstrun = true;
                new SaveWhere(false).ShowDialog();
            }
            if (firstrun)
                CheckDrivers();
            else
            {
                var AppCollectionThread = new System.Threading.Thread(() => CheckDrivers());
                AppCollectionThread.IsBackground = true;
                AppCollectionThread.Start();
            }

            if (String.IsNullOrEmpty(Global.appdatapath))
            {
                Close();
                return;
            }
            Graphics g = this.CreateGraphics();
            try
            {
                dpix = g.DpiX / 100f * 1.041666666667f;
                dpiy = g.DpiY / 100f * 1.041666666667f;
            }
            finally
            {
                g.Dispose();
            }
            Icon = Properties.Resources.DS4W;
            notifyIcon1.Icon = Properties.Resources.DS4W;
            rootHub = new DS4Control.Control();
            rootHub.Debug += On_Debug;
            Log.GuiLog += On_Debug;
            Log.TrayIconLog += ShowNotification;
            // tmrUpdate.Enabled = true; TODO remove tmrUpdate and leave tick()

            Directory.CreateDirectory(Global.appdatapath);
            Global.Load();
            if (!Global.Save()) //if can't write to file
                if (MessageBox.Show("Cannot write at current locataion\nCopy Settings to appdata?", "DS4Windows",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == System.Windows.Forms.DialogResult.Yes)
                {
                    try
                    {
                        Directory.CreateDirectory(appdatapath);
                        File.Copy(exepath + "\\Profiles.xml", appdatapath + "\\Profiles.xml");
                        File.Copy(exepath + "\\Auto Profiles.xml", appdatapath + "\\Auto Profiles.xml");
                        Directory.CreateDirectory(appdatapath + "\\Profiles");
                        foreach (string s in Directory.GetFiles(exepath + "\\Profiles"))
                        {
                            File.Copy(s, appdatapath + "\\Profiles\\" + Path.GetFileName(s));
                        }
                    }
                    catch { }
                    MessageBox.Show("Copy complete, please relaunch DS4Windows and remove settings from Program Directory", "DS4Windows");
                    Global.appdatapath = null;
                    Close();
                    return;
                }
                else
                {
                    MessageBox.Show("DS4Windows cannot edit settings here, This will now close", "DS4Windows");
                    Global.appdatapath = null;
                    Close();
                    return;
                }
            foreach (ToolStripMenuItem t in shortcuts)
                t.DropDownItemClicked += Profile_Changed_Menu;
            hideDS4CheckBox.CheckedChanged -= hideDS4CheckBox_CheckedChanged;
            hideDS4CheckBox.Checked = Global.getUseExclusiveMode();
            hideDS4CheckBox.CheckedChanged += hideDS4CheckBox_CheckedChanged;
            cBDisconnectBT.Checked = Global.getDCBTatStop();
            cBQuickCharge.Checked = Global.getQuickCharge();
            // New settings
            this.Width = Global.getFormWidth();
            this.Height = Global.getFormHeight();
            startMinimizedCheckBox.CheckedChanged -= startMinimizedCheckBox_CheckedChanged;
            startMinimizedCheckBox.Checked = Global.getStartMinimized();
            startMinimizedCheckBox.CheckedChanged += startMinimizedCheckBox_CheckedChanged;
            if (!startMinimizedCheckBox.Checked)
            {
                mAllowVisible = true;
                Show();
            }
                //this.WindowState = FormWindowState.Minimized;
            Form_Resize(null, null);
            RefreshProfiles();
            for (int i = 0; i < 4; i++)
            {
                Global.LoadProfile(i, true, rootHub);
            }
            LoadP();
            Global.ControllerStatusChange += ControllerStatusChange;
            bool start = true;
            foreach (string s in arguements)
                if (s == "stop")
                {
                    start = false;
                    break;
                }
            if (btnStartStop.Enabled && start)
                btnStartStop_Clicked();
            startToolStripMenuItem.Text = btnStartStop.Text;
            cBNotifications.Checked = Global.getNotifications();
            cBSwipeProfiles.Checked = Global.getSwipeProfiles();
            int checkwhen = Global.getCheckWhen();
            cBUpdate.Checked = checkwhen > 0;
            if (checkwhen > 23)
            {
                cBUpdateTime.SelectedIndex = 1;
                nUDUpdateTime.Value = checkwhen / 24;
            }
            else
            {
                cBUpdateTime.SelectedIndex = 0;
                nUDUpdateTime.Value = checkwhen;
            }
            Uri url = new Uri("http://ds4windows.com/Files/Builds/newest.txt"); //Sorry other devs, gonna have to find your own server


            if (checkwhen > 0 && DateTime.Now >= Global.getLastChecked() + TimeSpan.FromHours(checkwhen))
            {
                wc.DownloadFileAsync(url, Global.appdatapath + "\\version.txt");
                wc.DownloadFileCompleted += Check_Version;
                Global.setLastChecked(DateTime.Now);
            }

            if (File.Exists(exepath + "\\Updater.exe"))
            {
                System.Threading.Thread.Sleep(2000);
                File.Delete(exepath + "\\Updater.exe");
            }
            //test.Start();
            hotkeysTimer.Start();
            hotkeysTimer.Tick += Hotkeys;
            test.Tick += test_Tick;
            if (!System.IO.Directory.Exists(Global.appdatapath + "\\Virtual Bus Driver"))
                linkUninstall.Visible = false;
            StartWindowsCheckBox.Checked = File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.Startup) + "\\DS4Windows.lnk");
        }


        protected override void SetVisibleCore(bool value)
        {
            if (!mAllowVisible)
            {
                value = false;
                if (!this.IsHandleCreated) CreateHandle();
            }
            base.SetVisibleCore(value);
        }

        private void showToolStripMenuItem_Click(object sender, EventArgs e)
        {
            mAllowVisible = true;
            Show();
        }

        /*private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            mAllowClose = mAllowVisible = true;
            if (!mLoadFired) Show();
            Close();
        }*/

        public static string GetTopWindowName()
        {
            IntPtr hWnd = GetForegroundWindow();
            uint lpdwProcessId;
            GetWindowThreadProcessId(hWnd, out lpdwProcessId);

            IntPtr hProcess = OpenProcess(0x0410, false, lpdwProcessId);

            StringBuilder text = new StringBuilder(1000);
            GetModuleFileNameEx(hProcess, IntPtr.Zero, text, text.Capacity);

            CloseHandle(hProcess);

            return text.ToString();
        }

        private void OnPowerChange(object s, PowerModeChangedEventArgs e)
        {
            switch (e.Mode)
            {
                case PowerModes.Resume:
                    if (btnStartStop.Text == Properties.Resources.StartText && wasrunning)
                    {
                        DS4LightBar.shuttingdown = false;
                        wasrunning = false;
                        btnStartStop_Clicked();
                    }
                    break;
                case PowerModes.Suspend:
                    if (btnStartStop.Text == Properties.Resources.StopText)
                    {
                        DS4LightBar.shuttingdown = true;
                        btnStartStop_Clicked();
                        wasrunning = true;
                    }
                    break;
            }
        }

        private void test_Tick(object sender, EventArgs e)
        {
            lBTest.Visible = true;
            lBTest.Text = rootHub.oldtouchvalue[0].ToString();
        }
        void Hotkeys(object sender, EventArgs e)
        {
            if (Global.getSwipeProfiles())
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
                        ShowNotification(this, Properties.Resources.UsingProfile.Replace("*number*", (i + 1).ToString()).Replace("*Profile name*", cbs[i].Text));
                }

            //Check for process for auto profiles
            if (tempprofile == "null")
                for (int i = 0; i < programpaths.Count; i++)
                {
                    string name = programpaths[i].ToLower().Replace('/', '\\');
                    if (name == GetTopWindowName().ToLower().Replace('/', '\\'))
                    {
                        for (int j = 0; j < 4; j++)
                            if (proprofiles[j][i] != "(none)" && proprofiles[j][i] != Properties.Resources.noneProfile)
                            {
                                Global.LoadTempProfile(j, proprofiles[j][i], true, rootHub); //j is controller index, i is filename
                                if (Global.getLaunchProgram(j) != string.Empty) Process.Start(Global.getLaunchProgram(j));
                            }
                        tempprofile = name;
                    }
                }
            else
            {
                if (tempprofile != GetTopWindowName().ToLower().Replace('/', '\\'))
                {
                    tempprofile = "null";
                    for (int j = 0; j < 4; j++)
                        Global.LoadProfile(j, false, rootHub);
                }
            }
            GC.Collect();
        }
        
        public void LoadP()
        {
            XmlDocument doc = new XmlDocument();
            proprofiles = new List<string>[4] { new List<string>(), new List<string>(),
                new List<string>(), new List<string>() };
            programpaths.Clear();
            if (!File.Exists(Global.appdatapath + "\\Auto Profiles.xml"))
                return;
            doc.Load(Global.appdatapath + "\\Auto Profiles.xml");
            XmlNodeList programslist = doc.SelectNodes("Programs/Program");
            foreach (XmlNode x in programslist)
                    programpaths.Add(x.Attributes["path"].Value);
            foreach (string s in programpaths)
                for (int i = 0; i < 4; i++)
                {
                    proprofiles[i].Add(doc.SelectSingleNode("/Programs/Program[@path=\"" + s + "\"]"
                        + "/Controller" + (i + 1)).InnerText);
                }
        }

        private void CheckDrivers()
        {
            bool deriverinstalled = false;
            try
            {
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
                    wd.FormClosed += delegate { btnStartStop_Clicked(false); btnStartStop_Clicked(false); };
                }
            }
            catch
            {
                if (!File.Exists(exepath + "\\Auto Profiles.xml") && !File.Exists(appdatapath + "\\Auto Profiles.xml"))
                {
                    linkSetup.LinkColor = Color.Green;
                    tabSettings.Text += " (" + Properties.Resources.InstallDriver + ")";
                }
            }
        }

        private void Check_Version(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location);
            string version = fvi.FileVersion;
            string newversion = File.ReadAllText(Global.appdatapath + "\\version.txt");
            if (version.Replace(',', '.').CompareTo(newversion) == -1)//CompareVersions();
                if (MessageBox.Show(Properties.Resources.DownloadVersion.Replace("*number*", newversion), Properties.Resources.DS4Update, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == System.Windows.Forms.DialogResult.Yes)
                {
                    if (!File.Exists(exepath + "\\DS4Updater.exe") || (File.Exists(exepath + "\\DS4Updater.exe")
                        && (FileVersionInfo.GetVersionInfo(exepath + "\\DS4Updater.exe").FileVersion.CompareTo("1.1.0.0") == -1)))
                    {
                        Uri url2 = new Uri("http://ds4windows.com/Files/DS4Updater.exe");
                        WebClient wc2 = new WebClient();
                        if (Global.appdatapath == exepath)
                            wc2.DownloadFile(url2, exepath + "\\DS4Updater.exe");
                        else
                        {
                            MessageBox.Show(Properties.Resources.PleaseDownloadUpdater);
                            Process.Start("http://ds4windows.com/Files/DS4Updater.exe");
                        }
                    }
                    Process p = new Process();
                    p.StartInfo.FileName = exepath + "\\DS4Updater.exe";
                    if (Global.AdminNeeded())
                        p.StartInfo.Verb = "runas";
                    p.Start();
                    Close();
                }
                else
                    File.Delete(Global.appdatapath + "\\version.txt");
            else
                File.Delete(Global.appdatapath + "\\version.txt");
        }

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
                if (lBProfiles.Items.Count == 0)
                {
                    Global.SaveProfile(0, "Default", null, null);
                    Global.setAProfile(0, "Default");
                    RefreshProfiles();
                    return;
                }
                for (int i = 0; i < 4; i++)
                {
                    cbs[i].Items.Clear();
                    shortcuts[i].DropDownItems.Clear();
                    cbs[i].Items.AddRange(profilenames.ToArray());
                    foreach (string s in profilenames)
                        shortcuts[i].DropDownItems.Add(s);
                    for (int j = 0; j < cbs[i].Items.Count; j++)
                        if (cbs[i].Items[j].ToString() == Path.GetFileNameWithoutExtension(Global.getAProfile(i)))
                        {
                            cbs[i].SelectedIndex = j;
                            ((ToolStripMenuItem)shortcuts[i].DropDownItems[j]).Checked = true;
                            Global.setAProfile(i, cbs[i].Text);
                            shortcuts[i].Text = Properties.Resources.ContextEdit.Replace("*number*", (i + 1).ToString());
                            ebns[i].Text = Properties.Resources.EditProfile;
                            break;
                        }
                        else
                        {
                            cbs[i].Text = "(" + Properties.Resources.NoProfileLoaded + ")";
                            shortcuts[i].Text = Properties.Resources.ContextNew.Replace("*number*", (i + 1).ToString());
                            ebns[i].Text = Properties.Resources.New;
                        }
                }
            }
            catch (DirectoryNotFoundException)
            {
                Directory.CreateDirectory(Global.appdatapath + @"\Profiles\");
                Global.SaveProfile(0, "Default", null, null);
                Global.setAProfile(0, "Default");
                RefreshProfiles();
                return;
            }
            finally
            {
                if (!(cbs[0].Items.Count > 0 && cbs[0].Items[cbs[0].Items.Count - 1].ToString() == "+New Profile"))
                {
                    for (int i = 0; i < 4; i++)
                    {
                        cbs[i].Items.Add("+New Profile");
                        shortcuts[i].DropDownItems.Add("-");
                        shortcuts[i].DropDownItems.Add("+New Profile");
                    }
                    RefreshAutoProfilesPage();
                }
            }
        }


        public void RefreshAutoProfilesPage()
        {
            tabAutoProfiles.Controls.Clear();
            WP = new WinProgs(profilenames.ToArray(), this);
            WP.TopLevel = false;
            WP.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            WP.Visible = true;
            WP.Dock = DockStyle.Fill;
            tabAutoProfiles.Controls.Add(WP);
        }
        protected async void LogDebug(DateTime Time, String Data)
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
                if (Data.StartsWith("Warning"))
                {
                    await System.Threading.Tasks.Task.Delay(5);
                    Time = DateTime.Now;
                }
                String Posted = Time.ToString("G");

                lvDebug.Items.Add(new ListViewItem(new String[] { Posted, Data })).EnsureVisible();

                //Added alternative
                lbLastMessage.Text = Data;
            }
        }

        protected void ShowNotification(object sender, DebugEventArgs args)
        {
            if (Form.ActiveForm != this && cBNotifications.Checked)
            {
                this.notifyIcon1.BalloonTipText = args.Data;
                notifyIcon1.BalloonTipTitle = "DS4Windows";
                notifyIcon1.ShowBalloonTip(1); 
            }
        }

        protected void ShowNotification(object sender, string text)
        {
            if (Form.ActiveForm != this && cBNotifications.Checked)
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
                this.FormBorderStyle = FormBorderStyle.None;
            }

            else if (FormWindowState.Normal == this.WindowState)
            {
                //mAllowVisible = true;
                this.Show();
                this.ShowInTaskbar = true;
                this.FormBorderStyle = FormBorderStyle.Sizable;
            }
            chData.AutoResize(ColumnHeaderAutoResizeStyle.HeaderSize);
        }

        protected void btnStartStop_Click(object sender, EventArgs e)
        {
            btnStartStop_Clicked();
        }
        public void btnStartStop_Clicked(bool log = true)
        {
            if (btnStartStop.Text == Properties.Resources.StartText)
            {
                rootHub.Start(log);
                hotkeysTimer.Start();
                btnStartStop.Text = Properties.Resources.StopText;
            }

            else if (btnStartStop.Text == Properties.Resources.StopText)
            {                
                rootHub.Stop(log);
                hotkeysTimer.Stop();
                btnStartStop.Text = Properties.Resources.StartText;
            }
            startToolStripMenuItem.Text = btnStartStop.Text;
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
            if (m.Msg == WM_QUERYENDSESSION)
                systemShutdown = true;

            // If this is WM_QUERYENDSESSION, the closing event should be
            // raised in the base WndProc.
            base.WndProc(ref m);
        }
        
        protected void ControllerStatusChange(object sender, EventArgs e)
        {
            if (InvokeRequired)
                Invoke(new ControllerStatusChangedDelegate(ControllerStatusChange), new object[] { sender, e });
            else
                ControllerStatusChanged();
        }
        protected void ControllerStatusChanged()
        {
            String tooltip = "DS4Windows v" + FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).FileVersion;
            for (Int32 Index = 0; Index < Pads.Length; Index++)
            {
                Pads[Index].Text = rootHub.getDS4MacAddress(Index);
                DS4Device d = rootHub.DS4Controllers[Index];
                if (d != null && Global.getQuickCharge() && d.ConnectionType == ConnectionType.BT && d.Charging)
                {
                    d.DisconnectBT();
                    return;
                }
                switch (rootHub.getDS4Status(Index))
                {
                    case "USB": statPB[Index].Image = Properties.Resources.USB; tt.SetToolTip(statPB[Index], ""); break;
                    case "BT": statPB[Index].Image = Properties.Resources.BT; tt.SetToolTip(statPB[Index], "Right click to disconnect"); break;
                    default: statPB[Index].Image = Properties.Resources.none; tt.SetToolTip(statPB[Index], ""); break;
                }
                Batteries[Index].Text = rootHub.getDS4Battery(Index);
                if (Pads[Index].Text != String.Empty)
                {
                    Pads[Index].Enabled = true;
                    if (Pads[Index].Text != Properties.Resources.Connecting)
                    {
                        Enable_Controls(Index, true);
                        if (opt != null)
                            opt.inputtimer.Start();
                        //MinimumSize = new Size(MinimumSize.Width, 137 + 29 * Index);
                    }
                    else if (opt != null)
                        opt.inputtimer.Stop();
                }
                else
                {
                    Pads[Index].Text = Properties.Resources.Disconnected;
                    Enable_Controls(Index, false);
                    shortcuts[Index].Enabled = false;
                }
                //if (((Index + 1) + ": " + rootHub.getShortDS4ControllerInfo(Index)).Length > 50)
                //MessageBox.Show(((Index + 1) + ": " + rootHub.getShortDS4ControllerInfo(Index)).Length.ToString());
                if (rootHub.getShortDS4ControllerInfo(Index) != Properties.Resources.NoneText)
                    tooltip += "\n" + (Index + 1) + ": " + rootHub.getShortDS4ControllerInfo(Index); // Carefully stay under the 63 character limit.
            }
            btnClear.Enabled = lvDebug.Items.Count > 0;
            if (tooltip.Length > 63)
                notifyIcon1.Text = tooltip.Substring(0,63);
            else
                notifyIcon1.Text = tooltip;
        }
        
        private void pBStatus_MouseClick(object sender, MouseEventArgs e)
        {
            int i = Int32.Parse(((PictureBox)sender).Tag.ToString());
            if (e.Button == System.Windows.Forms.MouseButtons.Right && rootHub.getDS4Status(i) == "BT" && !rootHub.DS4Controllers[i].Charging)
                rootHub.DS4Controllers[i].DisconnectBT();
        }

        private async void lbPad1_TextChanged(object sender, EventArgs e)
        {
            Label lb = ((Label)sender);
            int i = int.Parse(lb.Name.Substring(lb.Name.Length-1)) - 1;
            if (lb.Text == Properties.Resources.Disconnected && Pads[i].Enabled)
            {
                await Task.Delay(10);
                Enable_Controls(i, false);
            }
        }

        private void Enable_Controls(int device, bool on)
        {
            Pads[device].Enabled = on;
            ebns[device].Enabled = on;
            cbs[device].Enabled = on;
            shortcuts[device].Enabled = on;
            Batteries[device].Enabled = on;
        }
        
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
                if (MessageBox.Show(Properties.Resources.ProfileCannotRestore.Replace("*Profile name*", "\"" + filename + "\""), Properties.Resources.DeleteProfile, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == System.Windows.Forms.DialogResult.Yes)
                {
                    System.IO.File.Delete(Global.appdatapath + @"\Profiles\" + filename + ".xml");
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
            if (Global.appdatapath == Directory.GetParent(Assembly.GetExecutingAssembly().Location).FullName)
                openProfiles.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\DS4Tool" + @"\Profiles\";
            else
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

        private void ShowOptions(int devID, string profile)
        {
            if (opt != null)
                opt.Close();
            Show();
            WindowState = FormWindowState.Normal;
            toolStrip1.Enabled = false;
            tSOptions.Visible = true;
            toolStrip1.Visible = false;
            if (profile != "")
                tSTBProfile.Text = profile;
            else
                tSTBProfile.Text = "<" + Properties.Resources.TypeProfileName + ">";
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
            tSOptions.SendToBack();
            opt.FormClosed += delegate
            {
                opt = null;
                RefreshProfiles();
                this.Size = oldsize;
                oldsize = new System.Drawing.Size(0, 0);
                tSBKeepSize.Text =  Properties.Resources.KeepThisSize;
                tSBKeepSize.Image = Properties.Resources.size;
                tSBKeepSize.Enabled = true;
                tSOptions.Visible = false;
                toolStrip1.Visible = true;
                toolStrip1.Enabled = true;
                lbLastMessage.ForeColor = SystemColors.GrayText;
                lbLastMessage.Text = lvDebug.Items[lvDebug.Items.Count - 1].SubItems[1].Text;
            };
            oldsize = this.Size;
            /*if (dpix == 120)
            {
                if (this.Size.Height < 90 + opt.MaximumSize.Height * 1.25)
                    this.Size = new System.Drawing.Size(this.Size.Width, (int)(90 + opt.MaximumSize.Height * 1.25));
                if (this.Size.Width < 20 + opt.MaximumSize.Width * 1.25)
                    this.Size = new System.Drawing.Size((int)(20 + opt.Size.Width * 1.25), this.Size.Height);
            }
            else*/
            {
                if (this.Size.Height < (int)(90 * dpiy) + opt.MaximumSize.Height)
                    this.Size = new System.Drawing.Size(this.Size.Width, (int)(90 * dpiy) + opt.MaximumSize.Height);
                if (this.Size.Width < (int)(20 * dpix) + opt.MaximumSize.Width)
                    this.Size = new System.Drawing.Size((int)(20 * dpix) + opt.MaximumSize.Width, this.Size.Height);
            }
            tabMain.SelectedIndex = 1;
        }

        private void editButtons_Click(object sender, EventArgs e)
        {
            Button bn = (Button)sender;
            int i = Int32.Parse(bn.Tag.ToString());
            if (cbs[i].Text == "(" + Properties.Resources.NoProfileLoaded + ")")
                    ShowOptions(i, "");
                else
                    ShowOptions(i, cbs[i].Text);
        }

        private void editMenu_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem em = (ToolStripMenuItem)sender;
            int i = Int32.Parse(em.Tag.ToString());
                if (em.Text == Properties.Resources.ContextNew.Replace("*number*", (i + 1).ToString()))
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
            btnStartStop_Clicked(false);
            btnStartStop_Clicked(false);
            Global.Save();
            //if (MessageBox.Show("Restart DS4Windows?", "") == System.Windows.Forms.DialogResult.OK);
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
                    LogDebug(DateTime.Now, Properties.Resources.UsingProfile.Replace("*number*", (tdevice + 1).ToString()).Replace("*Profile name*", cb.Text));
                    shortcuts[tdevice].Text = Properties.Resources.ContextEdit.Replace("*number*", (tdevice + 1).ToString());
                    Global.setAProfile(tdevice, cb.Items[cb.SelectedIndex].ToString());
                    Global.Save();
                    Global.LoadProfile(tdevice, true, rootHub);
                }
                else if (cb.SelectedIndex == cb.Items.Count - 1 && cb.Items.Count > 1) //if +New Profile selected
                    ShowOptions(tdevice, "");
                if (cb.Text == "(" + Properties.Resources.NoProfileLoaded + ")")
                    ebns[tdevice].Text = Properties.Resources.New;
                else
                    ebns[tdevice].Text = Properties.Resources.EditProfile;
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
            mAllowVisible = true;
            this.Show();
            WindowState = FormWindowState.Normal;
        }
        
        private void startToolStripMenuItem_Click(object sender, EventArgs e)
        {
            btnStartStop_Clicked();
        }
        private void notifyIcon1_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                mAllowVisible = true;
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
            Hotkeys hotkeysForm = new Hotkeys();
            hotkeysForm.Icon = this.Icon;
            hotkeysForm.ShowDialog();
        }

        private void StartWindowsCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            RegistryKey KeyLoc = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            if (StartWindowsCheckBox.Checked && !File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.Startup) + "\\DS4Windows.lnk"))
                appShortcutToStartup();
            else if (!StartWindowsCheckBox.Checked)
                File.Delete(Environment.GetFolderPath(Environment.SpecialFolder.Startup) + "\\DS4Windows.lnk");
            KeyLoc.DeleteValue("DS4Tool", false);
        }

        private void appShortcutToStartup()
        {
            Type t = Type.GetTypeFromCLSID(new Guid("72C24DD5-D70A-438B-8A42-98424B88AFB8")); //Windows Script Host Shell Object
            dynamic shell = Activator.CreateInstance(t);
            try
            {
                var lnk = shell.CreateShortcut(Environment.GetFolderPath(Environment.SpecialFolder.Startup) + "\\DS4Windows.lnk");
                try
                {
                    string app = Assembly.GetExecutingAssembly().Location;
                    lnk.TargetPath = Assembly.GetExecutingAssembly().Location;
                    lnk.IconLocation = app.Replace('\\', '/');
                    lnk.Save();
                }
                finally
                {
                    Marshal.FinalReleaseComObject(lnk);
                }
            }
            finally
            {
                Marshal.FinalReleaseComObject(shell);
            }
        }

        private void tabMain_SelectedIndexChanged(object sender, EventArgs e)
        {
            lbLastMessage.Visible = tabMain.SelectedIndex != 4;
            if (tabMain.SelectedIndex == 4)
                chData.AutoResize(ColumnHeaderAutoResizeStyle.HeaderSize);
            if (opt != null)
                if (tabMain.SelectedIndex != 1)
                    opt.inputtimer.Stop();
                else
                    opt.inputtimer.Start();            
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

        

        private void tBProfile_TextChanged(object sender, EventArgs e)
        {
            if (tSTBProfile.Text != null && tSTBProfile.Text != "" && !tSTBProfile.Text.Contains("\\") && !tSTBProfile.Text.Contains("/") && !tSTBProfile.Text.Contains(":") && !tSTBProfile.Text.Contains("*") && !tSTBProfile.Text.Contains("?") && !tSTBProfile.Text.Contains("\"") && !tSTBProfile.Text.Contains("<") && !tSTBProfile.Text.Contains(">") && !tSTBProfile.Text.Contains("|"))
                tSTBProfile.ForeColor = System.Drawing.SystemColors.WindowText;
            else
                tSTBProfile.ForeColor = System.Drawing.SystemColors.GrayText;
        }

        private void tBProfile_Enter(object sender, EventArgs e)
        {
            if (tSTBProfile.Text == "<" + Properties.Resources.TypeProfileName + ">")
                tSTBProfile.Text = "";
        }

        private void tBProfile_Leave(object sender, EventArgs e)
        {
            if (tSTBProfile.Text == "")
                tSTBProfile.Text = "<" + Properties.Resources.TypeProfileName + ">";
        }

        private void tSBCancel_Click(object sender, EventArgs e)
        {
            if (opt != null)
                opt.Close();
        }

        private void tSBSaveProfile_Click(object sender, EventArgs e)
        {
            if (opt != null)
            {
                opt.Set();

                if (tSTBProfile.Text != null && tSTBProfile.Text != "" && !tSTBProfile.Text.Contains("\\") && !tSTBProfile.Text.Contains("/") && !tSTBProfile.Text.Contains(":") && !tSTBProfile.Text.Contains("*") && !tSTBProfile.Text.Contains("?") && !tSTBProfile.Text.Contains("\"") && !tSTBProfile.Text.Contains("<") && !tSTBProfile.Text.Contains(">") && !tSTBProfile.Text.Contains("|"))
                {
                    System.IO.File.Delete(Global.appdatapath + @"\Profiles\" + opt.filename + ".xml");
                    Global.setAProfile(opt.device, tSTBProfile.Text);
                    Global.SaveProfile(opt.device, tSTBProfile.Text, opt.buttons.ToArray(), opt.subbuttons.ToArray());
                    Global.Save();
                    opt.Close();
                }
                else
                    MessageBox.Show(Properties.Resources.ValidName, Properties.Resources.NotValid, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void tSBKeepSize_Click(object sender, EventArgs e)
        {
            oldsize = Size;
            tSBKeepSize.Text = Properties.Resources.WillKeep;
            tSBKeepSize.Image = Properties.Resources._checked;
            tSBKeepSize.Enabled = false;
        }

        private void cBUpdate_CheckedChanged(object sender, EventArgs e)
        {
            if (!cBUpdate.Checked)
            {
                nUDUpdateTime.Value = 0;
                pNUpdate.Enabled = false;
            }
            else
            {
                nUDUpdateTime.Value = 1;
                cBUpdateTime.SelectedIndex = 0;
                pNUpdate.Enabled = true;
            }
        }

        private void nUDUpdateTime_ValueChanged(object sender, EventArgs e)
        {
            if (cBUpdateTime.SelectedIndex == 0)
                Global.setCheckWhen((int)nUDUpdateTime.Value);
            else if (cBUpdateTime.SelectedIndex == 1)
                Global.setCheckWhen((int)nUDUpdateTime.Value * 24);
            if (nUDUpdateTime.Value < 1)
                cBUpdate.Checked = false;
            if (nUDUpdateTime.Value == 1)
            {
                int index = cBUpdateTime.SelectedIndex;
                cBUpdateTime.Items.Clear();
                cBUpdateTime.Items.Add(Properties.Resources.Hour);
                cBUpdateTime.Items.Add(Properties.Resources.Day);
                cBUpdateTime.SelectedIndex = index;
            }
            else if (cBUpdateTime.Items[0].ToString() == Properties.Resources.Hour)
            {
                int index = cBUpdateTime.SelectedIndex;
                cBUpdateTime.Items.Clear();
                cBUpdateTime.Items.Add(Properties.Resources.Hours);
                cBUpdateTime.Items.Add(Properties.Resources.Days);
                cBUpdateTime.SelectedIndex = index;
            }
        }

        private void cBUpdateTime_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cBUpdateTime.SelectedIndex == 0)
                Global.setCheckWhen((int)nUDUpdateTime.Value);
            else if (cBUpdateTime.SelectedIndex == 1)
                Global.setCheckWhen((int)nUDUpdateTime.Value * 24);
        }

        private void lLBUpdate_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Uri url = new Uri("http://ds4windows.com/Files/Builds/newest.txt"); //Sorry other devs, gonna have to find your own server
            WebClient wct = new WebClient();
            wct.DownloadFileAsync(url, Global.appdatapath + "\\version.txt");
            wct.DownloadFileCompleted += wct_DownloadFileCompleted;
        }

        private void cBDisconnectBT_CheckedChanged(object sender, EventArgs e)
        {
            Global.setDCBTatStop(cBDisconnectBT.Checked);
        }

        void wct_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            Global.setLastChecked(DateTime.Now);
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location);
            string version2 = fvi.FileVersion;
            string newversion2 = File.ReadAllText(Global.appdatapath + "\\version.txt");
            if (version2.Replace(',', '.').CompareTo(File.ReadAllText(Global.appdatapath + "\\version.txt")) == -1)//CompareVersions();
                if (MessageBox.Show(Properties.Resources.DownloadVersion.Replace("*number*", newversion2), Properties.Resources.DS4Update, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == System.Windows.Forms.DialogResult.Yes)
                {
                    if (!File.Exists(exepath + "\\DS4Updater.exe") || (File.Exists(exepath + "\\DS4Updater.exe")
                         && (FileVersionInfo.GetVersionInfo(exepath + "\\DS4Updater.exe").FileVersion.CompareTo("1.1.0.0") == -1)))
                    {
                        Uri url2 = new Uri("http://ds4windows.com/Files/DS4Updater.exe");
                        WebClient wc2 = new WebClient();
                        if (Global.appdatapath == exepath)
                            wc2.DownloadFile(url2, exepath + "\\DS4Updater.exe");
                        else
                        {
                            MessageBox.Show(Properties.Resources.PleaseDownloadUpdater);
                            Process.Start("http://ds4windows.com/Files/DS4Updater.exe");
                        }
                    }
                    Process p = new Process();
                    p.StartInfo.FileName = exepath + "\\DS4Updater.exe";
                    if (Global.AdminNeeded())
                        p.StartInfo.Verb = "runas";
                    p.Start();
                    Close();
                }
                else
                    File.Delete(Global.appdatapath + "\\version.txt");
            else
            {
                File.Delete(Global.appdatapath + "\\version.txt");
                MessageBox.Show(Properties.Resources.UpToDate, "DS4Windows Updater");
            }
        }

        private void linkProfiles_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start(Global.appdatapath + "\\Profiles");
        }

        private void linkUninstall_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (File.Exists(Global.appdatapath + "\\Virtual Bus Driver\\ScpDriver.exe"))
                try { System.Diagnostics.Process.Start(Global.appdatapath + "\\Virtual Bus Driver\\ScpDriver.exe"); }
                catch { System.Diagnostics.Process.Start(Global.appdatapath + "\\Virtual Bus Driver"); }
        }

        private void cBNotifications_CheckedChanged(object sender, EventArgs e)
        {
            Global.setNotifications(cBNotifications.Checked);
        }

        private void lLSetup_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            WelcomeDialog wd = new WelcomeDialog();
            wd.ShowDialog();
            tabSettings.Text = "Settings";
            linkSetup.LinkColor = Color.Blue;
        }

        protected void ScpForm_Closing(object sender, FormClosingEventArgs e)
        {
            if (opt != null)
            {
                opt.Close();
                e.Cancel = true;
                return;
            }
            if (systemShutdown)
            // Reset the variable because the user might cancel the 
            // shutdown.
            {
                systemShutdown = false;
                DS4LightBar.shuttingdown = true;
            }
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
            if (!String.IsNullOrEmpty(Global.appdatapath))
            {
                Global.Save();
                rootHub.Stop();
            }
        }

        private void cBSwipeProfiles_CheckedChanged(object sender, EventArgs e)
        {
            Global.setSwipeProfiles(cBSwipeProfiles.Checked);
        }

        private void cBQuickCharge_CheckedChanged(object sender, EventArgs e)
        {
            Global.setQuickCharge(cBQuickCharge.Checked);
        }

        private void lbLastMessage_MouseHover(object sender, EventArgs e)
        {
            tt.Show(lbLastMessage.Text, lbLastMessage, -3, -3);
        }

        private void pnlButton_MouseLeave(object sender, EventArgs e)
        {
            tt.Hide(lbLastMessage);
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
