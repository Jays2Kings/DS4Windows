﻿using System;
using System.Windows.Forms;
using System.Runtime.InteropServices;
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
using System.ServiceProcess;

namespace EAll4Windows
{
    public partial class EAll4Form : Form
    {
        public string[] arguements;
        delegate void LogDebugDelegate(DateTime Time, String Data, bool warning);
        protected Label[] Pads, Batteries;
        protected ComboBox[] cbs;
        protected Button[] ebns;
        protected PictureBox[] statPB;
        protected ToolStripMenuItem[] shortcuts;
        WebClient wc = new WebClient();
        Timer test = new Timer(), hotkeysTimer = new Timer();
        string exepath = Directory.GetParent(Assembly.GetExecutingAssembly().Location).FullName;
        string appdatapath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\EAll4Windows";
        string oldappdatapath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\EAll4Tool";
        string tempProfileProgram = "null";
        float dpix, dpiy;
        List<string> profilenames = new List<string>();
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
        public bool mAllowVisible;
        bool contextclose;
        string logFile = Global.appdatapath + @"\EAll4Service.log";
        //StreamWriter logWriter;
        //bool outputlog = false;

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

        public EAll4Form(string[] args)
        {
            //System.Threading.Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo("fr-FR");
            InitializeComponent();
            arguements = args;
            ThemeUtil.SetTheme(lvDebug);
            Pads = new Label[4] { lbPad1, lbPad2, lbPad3, lbPad4 };
            Batteries = new Label[4] { lbBatt1, lbBatt2, lbBatt3, lbBatt4 };
            cbs = new ComboBox[4] { cBController1, cBController2, cBController3, cBController4 };
            ebns = new Button[4] { bnEditC1, bnEditC2, bnEditC3, bnEditC4 };
            statPB = new PictureBox[4] { pBStatus1, pBStatus2, pBStatus3, pBStatus4 };
            shortcuts = new ToolStripMenuItem[4] { (ToolStripMenuItem)notifyIcon1.ContextMenuStrip.Items[0],
                (ToolStripMenuItem)notifyIcon1.ContextMenuStrip.Items[1],
                (ToolStripMenuItem)notifyIcon1.ContextMenuStrip.Items[2],
                (ToolStripMenuItem)notifyIcon1.ContextMenuStrip.Items[3] };
            SystemEvents.PowerModeChanged += OnPowerChange;
            tSOptions.Visible = false;
            bool firstrun = false;
            if (File.Exists(exepath + "\\Auto Profiles.xml")
                && File.Exists(appdatapath + "\\Auto Profiles.xml"))
            {
                firstrun = true;
                new SaveWhere(true).ShowDialog();
            }
            else if (File.Exists(exepath + "\\Auto Profiles.xml"))
                Global.SaveWhere(exepath);
            else if (File.Exists(appdatapath + "\\Auto Profiles.xml"))
                Global.SaveWhere(appdatapath);
            else if (File.Exists(oldappdatapath + "\\Auto Profiles.xml"))
            {
                try
                {
                    if (Directory.Exists(appdatapath))
                        Directory.Move(appdatapath, Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\EAll4Windows Old");
                    Directory.Move(oldappdatapath, appdatapath);
                    Global.SaveWhere(appdatapath);
                }
                catch
                {
                    MessageBox.Show(Properties.Resources.CannotMoveFiles, "EAll4Windows");
                    Process.Start("explorer.exe", @"/select, " + appdatapath);
                    Close();
                    return;
                }
            }
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
            Icon = Properties.Resources.EAll4W;
            notifyIcon1.Icon = Properties.Resources.EAll4W;
            Program.rootHub.Debug += On_Debug;

            Log.GuiLog += On_Debug;
            logFile = Global.appdatapath + "\\EAll4Windows.log";
            //logWriter = File.AppendText(logFile);
            Log.TrayIconLog += ShowNotification;
            // tmrUpdate.Enabled = true; TODO remove tmrUpdate and leave tick()

            Directory.CreateDirectory(Global.appdatapath);
            Global.Load();
            if (!Global.Save()) //if can't write to file
                if (MessageBox.Show("Cannot write at current location\nCopy Settings to appdata?", "EAll4Windows",
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
                    MessageBox.Show("Copy complete, please relaunch EAll4Windows and remove settings from Program Directory", "EAll4Windows");
                    Global.appdatapath = null;
                    Close();
                    return;
                }
                else
                {
                    MessageBox.Show("EAll4Windows cannot edit settings here, This will now close", "EAll4Windows");
                    Global.appdatapath = null;
                    Close();
                    return;
                }
            foreach (ToolStripMenuItem t in shortcuts)
                t.DropDownItemClicked += Profile_Changed_Menu;
            hideEAll4CheckBox.CheckedChanged -= hideEAll4CheckBox_CheckedChanged;
            hideEAll4CheckBox.Checked = Global.UseExclusiveMode;
            hideEAll4CheckBox.CheckedChanged += hideEAll4CheckBox_CheckedChanged;
            cBDisconnectBT.Checked = Global.DCBTatStop;
            cBQuickCharge.Checked = Global.QuickCharge;
            nUDXIPorts.Value = Global.FirstXinputPort;
            Program.rootHub.x360Bus.FirstController = Global.FirstXinputPort;
            // New settings
            this.Width = Global.FormWidth;
            this.Height = Global.FormHeight;
            startMinimizedCheckBox.CheckedChanged -= startMinimizedCheckBox_CheckedChanged;
            startMinimizedCheckBox.Checked = Global.StartMinimized;
            startMinimizedCheckBox.CheckedChanged += startMinimizedCheckBox_CheckedChanged;
            cBCloseMini.Checked = Global.CloseMini;
            string lang = CultureInfo.CurrentCulture.ToString();
            if (lang.StartsWith("en"))
                cBDownloadLangauge.Visible = false;
            cBDownloadLangauge.Checked = Global.DownloadLang;
            cBFlashWhenLate.Checked = Global.FlashWhenLate;
            if (!Global.LoadActions()) //if first no actions have been made yet, create PS+Option to D/C and save it to every profile
            {
                XmlDocument xDoc = new XmlDocument();
                try
                {
                    string[] profiles = Directory.GetFiles(Global.appdatapath + @"\Profiles\");
                    foreach (String s in profiles)
                        if (Path.GetExtension(s) == ".xml")
                        {
                            xDoc.Load(s);
                            XmlNode el = xDoc.SelectSingleNode("EAll4Windows/ProfileActions"); //.CreateElement("Action");
                            if (el != null)
                                if (string.IsNullOrEmpty(el.InnerText))
                                    el.InnerText = "Disconnect Controller";
                                else
                                    el.InnerText += "/Disconnect Controller";
                            else
                            {
                                XmlNode Node = xDoc.SelectSingleNode("EAll4Windows");
                                el = xDoc.CreateElement("ProfileActions");
                                el.InnerText = "Disconnect Controller";
                                Node.AppendChild(el);
                            }
                            xDoc.Save(s);
                            Global.LoadActions();
                        }
                }
                catch { }
            }
            bool start = true;
            bool mini = false;
            for (int i = 0; i < arguements.Length; i++)
            {
                if (arguements[i] == "-stop")
                    start = false;
                if (arguements[i] == "-m")
                    mini = true;
                if (mini && start)
                    break;
            }
            if (!(startMinimizedCheckBox.Checked || mini))
            {
                mAllowVisible = true;
                Show();
            }
            Form_Resize(null, null);
            RefreshProfiles();
            NewVersion();
            for (int i = 0; i < 4; i++)
            {
                Global.LoadProfile(i, true, Program.rootHub);
            }
            LoadP();
            Global.ControllerStatusChange += ControllerStatusChange;
            Enable_Controls(0, false);
            Enable_Controls(1, false);
            Enable_Controls(2, false);
            Enable_Controls(3, false);
            btnStartStop.Text = Properties.Resources.StartText;
            if (btnStartStop.Enabled && start)
                btnStartStop_Clicked();
            startToolStripMenuItem.Text = btnStartStop.Text;
            if (!tLPControllers.Visible)
                tabMain.SelectedIndex = 1;
            cBNotifications.Checked = Global.Notifications;
            cBSwipeProfiles.Checked = Global.SwipeProfiles;
            int checkwhen = Global.CheckWhen;
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
            Uri url = new Uri("http://eall4windows.com/Files/Builds/newest.txt"); //Sorry other devs, gonna have to find your own server


            if (checkwhen > 0 && DateTime.Now >= Global.LastChecked + TimeSpan.FromHours(checkwhen))
            {
                wc.DownloadFileAsync(url, Global.appdatapath + "\\version.txt");
                wc.DownloadFileCompleted += Check_Version;
                Global.LastChecked = DateTime.Now;
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
            if (File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.Startup) + "\\EAll4Windows.lnk"))
            {
                StartWindowsCheckBox.Checked = true;
                string lnkpath = WinProgs.ResolveShortcutAndArgument(Environment.GetFolderPath(Environment.SpecialFolder.Startup) + "\\EAll4Windows.lnk");
                if (!lnkpath.EndsWith("-m"))
                {
                    File.Delete(Environment.GetFolderPath(Environment.SpecialFolder.Startup) + "\\EAll4Windows.lnk");
                    appShortcutToStartup();
                }
            }
        }

        void NewVersion()
        {
            if (File.Exists(exepath + "\\1.4.22.eall4w"))
            {
                bool dcexists = false;
                foreach (SpecialAction action in Global.GetActions())
                {
                    if (action.type == "DisconnectBT")
                    {
                        dcexists = true;
                        break;
                    }
                }
                if (!dcexists)
                {
                    try
                    {
                        XmlDocument xDoc = new XmlDocument();
                        Global.SaveAction("Disconnect Controller", "PS/Options", 5, "0", false);
                        string[] profiles = Directory.GetFiles(Global.appdatapath + @"\Profiles\");
                        foreach (String s in profiles)
                            if (Path.GetExtension(s) == ".xml")
                            {
                                xDoc.Load(s);
                                XmlNode el = xDoc.SelectSingleNode("EAll4Windows/ProfileActions");
                                if (el != null)
                                    if (string.IsNullOrEmpty(el.InnerText))
                                        el.InnerText = "Disconnect Controller";
                                    else
                                        el.InnerText += "/Disconnect Controller";
                                else
                                {
                                    XmlNode Node = xDoc.SelectSingleNode("EAll4Windows");
                                    el = xDoc.CreateElement("ProfileActions");
                                    el.InnerText = "Disconnect Controller";
                                    Node.AppendChild(el);
                                }
                                xDoc.Save(s);
                                Global.LoadActions();
                            }
                    }
                    catch { }
                }
                File.Delete(exepath + "\\1.4.22.eall4w");
            }
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
                        EAll4LightBar.shuttingdown = false;
                        wasrunning = false;
                        btnStartStop_Clicked();
                    }
                    break;
                case PowerModes.Suspend:
                    if (btnStartStop.Text == Properties.Resources.StopText)
                    {
                        EAll4LightBar.shuttingdown = true;
                        btnStartStop_Clicked();
                        wasrunning = true;
                    }
                    break;
            }
        }

        private void test_Tick(object sender, EventArgs e)
        {
            /*testing values
            lbTest.Visible = true;
            lbTest.Text = Program.rootHub.oldtouchvalue[0].ToString();//*/
        }
        void Hotkeys(object sender, EventArgs e)
        {
            if (Global.SwipeProfiles)
                for (int i = 0; i < 4; i++)
                {
                    string slide = Program.rootHub.TouchpadSlide(i);
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
            if (tempProfileProgram == "null")
                for (int i = 0; i < programpaths.Count; i++)
                {
                    string name = programpaths[i].ToLower().Replace('/', '\\');
                    if (name == GetTopWindowName().ToLower().Replace('/', '\\'))
                    {
                        for (int j = 0; j < 4; j++)
                            if (proprofiles[j][i] != "(none)" && proprofiles[j][i] != Properties.Resources.noneProfile)
                            {
                                Global.LoadTempProfile(j, proprofiles[j][i], true, Program.rootHub); //j is controller index, i is filename
                                if (Global.LaunchProgram[j] != string.Empty) Process.Start(Global.LaunchProgram[j]);
                            }
                        tempProfileProgram = name;
                    }
                }
            else
            {
                if (tempProfileProgram != GetTopWindowName().ToLower().Replace('/', '\\'))
                {
                    tempProfileProgram = "null";
                    for (int j = 0; j < 4; j++)
                        Global.LoadProfile(j, false, Program.rootHub);
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
        string originalsettingstext;
        private void CheckDrivers()
        {
            originalsettingstext = tabSettings.Text;
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
                    Process p = new Process();
                    p.StartInfo.FileName = Assembly.GetExecutingAssembly().Location;
                    p.StartInfo.Arguments = "driverinstall";
                    p.StartInfo.Verb = "runas";
                    try { p.Start(); }
                    catch { }
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
                if (MessageBox.Show(Properties.Resources.DownloadVersion.Replace("*number*", newversion), Properties.Resources.EAll4Update, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == System.Windows.Forms.DialogResult.Yes)
                {
                    if (!File.Exists(exepath + "\\EAll4Updater.exe") || (File.Exists(exepath + "\\EAll4Updater.exe")
                        && (FileVersionInfo.GetVersionInfo(exepath + "\\EAll4Updater.exe").FileVersion.CompareTo("1.1.0.0") == -1)))
                    {
                        Uri url2 = new Uri("http://eall4windows.com/Files/EAll4Updater.exe");
                        WebClient wc2 = new WebClient();
                        if (Global.appdatapath == exepath)
                            wc2.DownloadFile(url2, exepath + "\\EAll4Updater.exe");
                        else
                        {
                            MessageBox.Show(Properties.Resources.PleaseDownloadUpdater);
                            Process.Start("http://eall4windows.com/Files/EAll4Updater.exe");
                        }
                    }
                    Process p = new Process();
                    p.StartInfo.FileName = exepath + "\\EAll4Updater.exe";
                    if (!cBDownloadLangauge.Checked)
                        p.StartInfo.Arguments = "-skipLang";
                    if (Global.AdminNeeded())
                        p.StartInfo.Verb = "runas";
                    try { p.Start(); Close(); }
                    catch { }
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
                    Global.ProfilePath[0] = "Default";
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
                        if (cbs[i].Items[j].ToString() == Path.GetFileNameWithoutExtension(Global.ProfilePath[i]))
                        {
                            cbs[i].SelectedIndex = j;
                            ((ToolStripMenuItem)shortcuts[i].DropDownItems[j]).Checked = true;
                            Global.ProfilePath[i] = cbs[i].Text;
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
                Global.ProfilePath[0] = "Default";
                RefreshProfiles();
                return;
            }
            finally
            {
                if (!(cbs[0].Items.Count > 0 && cbs[0].Items[cbs[0].Items.Count - 1].ToString() == "+" + Properties.Resources.PlusNewProfile))
                {
                    for (int i = 0; i < 4; i++)
                    {
                        cbs[i].Items.Add("+" + Properties.Resources.PlusNewProfile);
                        shortcuts[i].DropDownItems.Add("-");
                        shortcuts[i].DropDownItems.Add("+" + Properties.Resources.PlusNewProfile);
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
        protected void LogDebug(DateTime Time, String Data, bool warning)
        {
            if (lvDebug.InvokeRequired)
            {
                LogDebugDelegate d = new LogDebugDelegate(LogDebug);
                try
                {
                    this.Invoke(d, new Object[] { Time, Data, warning });
                }
                catch { }
            }
            else
            {
                String Posted = Time.ToString("G");
                lvDebug.Items.Add(new ListViewItem(new String[] { Posted, Data })).EnsureVisible();
                if (warning) lvDebug.Items[lvDebug.Items.Count - 1].ForeColor = Color.Red;
                //Added alternative
                lbLastMessage.Text = Data;
                lbLastMessage.ForeColor = (warning ? Color.Red : SystemColors.GrayText);
            }
        }

        protected void ShowNotification(object sender, DebugEventArgs args)
        {
            if (Form.ActiveForm != this && (cBNotifications.Checked || sender != null))
            {
                this.notifyIcon1.BalloonTipText = args.Data;
                notifyIcon1.BalloonTipTitle = "EAll4Windows";
                notifyIcon1.ShowBalloonTip(1);
            }
        }

        protected void ShowNotification(object sender, string text)
        {
            if (Form.ActiveForm != this && cBNotifications.Checked)
            {
                this.notifyIcon1.BalloonTipText = text;
                notifyIcon1.BalloonTipTitle = "EAll4Windows";
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
                Program.rootHub.Start(log);
                hotkeysTimer.Start();
                btnStartStop.Text = Properties.Resources.StopText;
            }

            else if (btnStartStop.Text == Properties.Resources.StopText)
            {
                Program.rootHub.Stop(log);
                hotkeysTimer.Stop();
                btnStartStop.Text = Properties.Resources.StartText;
            }
            startToolStripMenuItem.Text = btnStartStop.Text;
        }
        protected void btnClear_Click(object sender, EventArgs e)
        {
            lvDebug.Items.Clear();
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
                        Program.rootHub.HotPlug();
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
            String tooltip = "EAll4Windows v" + FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).FileVersion;
            bool nocontrollers = true;
            for (Int32 Index = 0; Index < Pads.Length; Index++)
            {
                Pads[Index].Text = Program.rootHub.getEAll4MacAddress(Index);
                EAll4Device d = Program.rootHub.EAll4Controllers[Index];
                if (d != null && Global.QuickCharge && d.ConnectionType == ConnectionType.BT && d.Charging)
                {
                    d.DisconnectBT();
                    return;
                }
                switch (Program.rootHub.getEAll4Status(Index))
                {
                    case "USB": statPB[Index].Visible = true; statPB[Index].Image = Properties.Resources.USB; toolTip1.SetToolTip(statPB[Index], ""); break;
                    case "BT": statPB[Index].Visible = true; statPB[Index].Image = Properties.Resources.BT; toolTip1.SetToolTip(statPB[Index], "Right click to disconnect"); break;
                    default: statPB[Index].Visible = false; toolTip1.SetToolTip(statPB[Index], ""); break;
                }
                Batteries[Index].Text = Program.rootHub.getEAll4Battery(Index);
                if (Pads[Index].Text != String.Empty)
                {
                    Pads[Index].Enabled = true;
                    nocontrollers = false;
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
                }
                //if (((Index + 1) + ": " + Program.rootHub.getShortEAll4ControllerInfo(Index)).Length > 50)
                //MessageBox.Show(((Index + 1) + ": " + Program.rootHub.getShortEAll4ControllerInfo(Index)).Length.ToString());
                if (Program.rootHub.getShortEAll4ControllerInfo(Index) != Properties.Resources.NoneText)
                    tooltip += "\n" + (Index + 1) + ": " + Program.rootHub.getShortEAll4ControllerInfo(Index); // Carefully stay under the 63 character limit.
            }
            lbNoControllers.Visible = nocontrollers;
            tLPControllers.Visible = !nocontrollers;
            btnClear.Enabled = lvDebug.Items.Count > 0;
            if (tooltip.Length > 63)
                notifyIcon1.Text = tooltip.Substring(0, 63);
            else
                notifyIcon1.Text = tooltip;
        }

        private void pBStatus_MouseClick(object sender, MouseEventArgs e)
        {
            int i = Int32.Parse(((PictureBox)sender).Tag.ToString());
            if (e.Button == System.Windows.Forms.MouseButtons.Right && Program.rootHub.getEAll4Status(i) == "BT" && !Program.rootHub.EAll4Controllers[i].Charging)
                Program.rootHub.EAll4Controllers[i].DisconnectBT();
        }

        private void Enable_Controls(int device, bool on)
        {
            Pads[device].Visible = on;
            ebns[device].Visible = on;
            cbs[device].Visible = on;
            shortcuts[device].Visible = on;
            Batteries[device].Visible = on;
        }

        void ScpForm_Report(object sender, EventArgs e)
        {
            if (InvokeRequired)
                Invoke(new HotKeysDelegate(Hotkeys), new object[] { sender, e });
            else
                Hotkeys(sender, e);
        }

        protected void On_Debug(object sender, DebugEventArgs e)
        {
            //logWriter.WriteLine(e.Time + ":\t" + e.Data);
            //logWriter.Flush();
            LogDebug(e.Time, e.Data, e.Warning);
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
                DupBox MTB = new DupBox(filename, this);
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
                openProfiles.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\EAll4Tool" + @"\Profiles\";
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
            opt = new Options(devID, profile, this);
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
                tSBKeepSize.Text = Properties.Resources.KeepThisSize;
                tSBKeepSize.Image = Properties.Resources.size;
                tSBKeepSize.Enabled = true;
                tSOptions.Visible = false;
                toolStrip1.Visible = true;
                toolStrip1.Enabled = true;
                lbLastMessage.ForeColor = SystemColors.GrayText;
                lbLastMessage.Text = lvDebug.Items[lvDebug.Items.Count - 1].SubItems[1].Text;
            };
            oldsize = this.Size;
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
            mAllowVisible = true;
            this.Show();
            WindowState = FormWindowState.Normal;
            ToolStripMenuItem em = (ToolStripMenuItem)sender;
            int i = Int32.Parse(em.Tag.ToString());
            if (em.Text == Properties.Resources.ContextNew.Replace("*number*", (i + 1).ToString()))
                ShowOptions(i, "");
            else
                for (int t = 0; t < em.DropDownItems.Count - 2; t++)
                    if (((ToolStripMenuItem)em.DropDownItems[t]).Checked)
                        ShowOptions(i, ((ToolStripMenuItem)em.DropDownItems[t]).Text);
        }

        private void lnkControllers_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("control", "joy.cpl");
        }

        private void hideEAll4CheckBox_CheckedChanged(object sender, EventArgs e)
        {
            // Prevent the Game Controllers window from throwing an error when controllers are un/hidden
            System.Diagnostics.Process[] rundll32 = System.Diagnostics.Process.GetProcessesByName("rundll32");
            foreach (System.Diagnostics.Process rundll32Instance in rundll32)
                foreach (System.Diagnostics.ProcessModule module in rundll32Instance.Modules)
                    if (module.FileName.Contains("joy.cpl"))
                        module.Dispose();

            Global.UseExclusiveMode = hideEAll4CheckBox.Checked;
            btnStartStop_Clicked(false);
            btnStartStop_Clicked(false);
            Global.Save();
        }

        private void startMinimizedCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            Global.StartMinimized = startMinimizedCheckBox.Checked;
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
            if (cb.Items[cb.Items.Count - 1].ToString() == "+" + Properties.Resources.PlusNewProfile)
            {
                if (cb.SelectedIndex < cb.Items.Count - 1)
                {
                    for (int i = 0; i < shortcuts[tdevice].DropDownItems.Count; i++)
                        if (!(shortcuts[tdevice].DropDownItems[i] is ToolStripSeparator))
                            ((ToolStripMenuItem)shortcuts[tdevice].DropDownItems[i]).Checked = false;
                    ((ToolStripMenuItem)shortcuts[tdevice].DropDownItems[cb.SelectedIndex]).Checked = true;
                    LogDebug(DateTime.Now, Properties.Resources.UsingProfile.Replace("*number*", (tdevice + 1).ToString()).Replace("*Profile name*", cb.Text), false);
                    shortcuts[tdevice].Text = Properties.Resources.ContextEdit.Replace("*number*", (tdevice + 1).ToString());
                    Global.ProfilePath[tdevice] = cb.Items[cb.SelectedIndex].ToString();
                    Global.Save();
                    Global.LoadProfile(tdevice, true, Program.rootHub);
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
            contextclose = true;
            this.Close();
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            mAllowVisible = true;
            this.Show();
            Focus();
            WindowState = FormWindowState.Normal;
        }

        private void startToolStripMenuItem_Click(object sender, EventArgs e)
        {
            btnStartStop_Clicked();
        }
        private void notifyIcon1_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Middle)
            {
                contextclose = true;
                this.Close();
            }
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
            hotkeysForm.Text = llbHelp.Text;
            hotkeysForm.ShowDialog();
        }

        private void StartWindowsCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            RegistryKey KeyLoc = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            if (StartWindowsCheckBox.Checked && !File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.Startup) + "\\EAll4Windows.lnk"))
                appShortcutToStartup();
            else if (!StartWindowsCheckBox.Checked)
                File.Delete(Environment.GetFolderPath(Environment.SpecialFolder.Startup) + "\\EAll4Windows.lnk");
            KeyLoc.DeleteValue("EAll4Tool", false);
        }

        private void appShortcutToStartup()
        {
            Type t = Type.GetTypeFromCLSID(new Guid("72C24DD5-D70A-438B-8A42-98424B88AFB8")); //Windows Script Host Shell Object
            dynamic shell = Activator.CreateInstance(t);
            try
            {
                var lnk = shell.CreateShortcut(Environment.GetFolderPath(Environment.SpecialFolder.Startup) + "\\EAll4Windows.lnk");
                try
                {
                    string app = Assembly.GetExecutingAssembly().Location;
                    lnk.TargetPath = Assembly.GetExecutingAssembly().Location;
                    lnk.Arguments = "-m";
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
            lbLastMessage.Visible = tabMain.SelectedTab != tabLog;
            if (tabMain.SelectedTab == tabLog)
                chData.AutoResize(ColumnHeaderAutoResizeStyle.HeaderSize);
            if (tabMain.SelectedTab == tabSettings)
            {
                lbLastMessage.ForeColor = SystemColors.GrayText;
                lbLastMessage.Text = Properties.Resources.HoverOverItems;
                foreach (System.Windows.Forms.Control control in fLPSettings.Controls)
                {
                    if (control.HasChildren)
                        foreach (System.Windows.Forms.Control ctrl in control.Controls)
                            ctrl.MouseHover += Items_MouseHover;
                    control.MouseHover += Items_MouseHover;
                }
            }
            else if (lvDebug.Items.Count > 0)
                lbLastMessage.Text = lbLastMessage.Text = lvDebug.Items[lvDebug.Items.Count - 1].SubItems[1].Text;
            else
                lbLastMessage.Text = "";
            if (opt != null)
                if (tabMain.SelectedIndex != 1)
                    opt.inputtimer.Stop();
                else
                    opt.inputtimer.Start();
            Program.rootHub.eastertime = tabMain.SelectedTab == tabLog;
        }

        private void Items_MouseHover(object sender, EventArgs e)
        {
            switch (((System.Windows.Forms.Control)sender).Name)
            {

                //if (File.Exists(appdatapath + "\\Auto Profiles.xml"))
                case "linkUninstall": lbLastMessage.Text = Properties.Resources.IfRemovingEAll4Windows; break;
                case "cBSwipeProfiles": lbLastMessage.Text = Properties.Resources.TwoFingerSwipe; break;
                case "cBQuickCharge": lbLastMessage.Text = Properties.Resources.QuickCharge; break;
                case "pnlXIPorts": lbLastMessage.Text = Properties.Resources.XinputPorts; break;
                case "lbUseXIPorts": lbLastMessage.Text = Properties.Resources.XinputPorts; break;
                case "nUDXIPorts": lbLastMessage.Text = Properties.Resources.XinputPorts; break;
                case "lbLastXIPort": lbLastMessage.Text = Properties.Resources.XinputPorts; break;
                case "cBCloseMini": lbLastMessage.Text = Properties.Resources.CloseMinimize; break;
                default: lbLastMessage.Text = Properties.Resources.HoverOverItems; break;
            }
            if (lbLastMessage.Text != Properties.Resources.HoverOverItems)
                lbLastMessage.ForeColor = Color.Black;
            else
                lbLastMessage.ForeColor = SystemColors.GrayText;
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
                opt.saving = true;
                opt.Set();

                if (tSTBProfile.Text != null && tSTBProfile.Text != "" && !tSTBProfile.Text.Contains("\\") && !tSTBProfile.Text.Contains("/") && !tSTBProfile.Text.Contains(":") && !tSTBProfile.Text.Contains("*") && !tSTBProfile.Text.Contains("?") && !tSTBProfile.Text.Contains("\"") && !tSTBProfile.Text.Contains("<") && !tSTBProfile.Text.Contains(">") && !tSTBProfile.Text.Contains("|"))
                {
                    System.IO.File.Delete(Global.appdatapath + @"\Profiles\" + opt.filename + ".xml");
                    Global.ProfilePath[opt.device] = tSTBProfile.Text;
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
                Global.CheckWhen = (int)nUDUpdateTime.Value;
            else if (cBUpdateTime.SelectedIndex == 1)
                Global.CheckWhen = (int)nUDUpdateTime.Value * 24;
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
                Global.CheckWhen = (int)nUDUpdateTime.Value;
            else if (cBUpdateTime.SelectedIndex == 1)
                Global.CheckWhen = (int)nUDUpdateTime.Value * 24;
        }

        private void lLBUpdate_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Uri url = new Uri("http://eall4windows.com/Files/Builds/newest.txt"); //Sorry other devs, gonna have to find your own server
            WebClient wct = new WebClient();
            wct.DownloadFileAsync(url, Global.appdatapath + "\\version.txt");
            wct.DownloadFileCompleted += wct_DownloadFileCompleted;
        }

        private void cBDisconnectBT_CheckedChanged(object sender, EventArgs e)
        {
            Global.DCBTatStop = cBDisconnectBT.Checked;
        }

        void wct_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            Global.LastChecked = DateTime.Now;
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location);
            string version2 = fvi.FileVersion;
            string newversion2 = File.ReadAllText(Global.appdatapath + "\\version.txt");
            if (version2.Replace(',', '.').CompareTo(File.ReadAllText(Global.appdatapath + "\\version.txt")) == -1)//CompareVersions();
                if (MessageBox.Show(Properties.Resources.DownloadVersion.Replace("*number*", newversion2), Properties.Resources.EAll4Update, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == System.Windows.Forms.DialogResult.Yes)
                {
                    if (!File.Exists(exepath + "\\EAll4Updater.exe") || (File.Exists(exepath + "\\EAll4Updater.exe")
                         && (FileVersionInfo.GetVersionInfo(exepath + "\\EAll4Updater.exe").FileVersion.CompareTo("1.1.0.0") == -1)))
                    {
                        Uri url2 = new Uri("http://eall4windows.com/Files/EAll4Updater.exe");
                        WebClient wc2 = new WebClient();
                        if (Global.appdatapath == exepath)
                            wc2.DownloadFile(url2, exepath + "\\EAll4Updater.exe");
                        else
                        {
                            MessageBox.Show(Properties.Resources.PleaseDownloadUpdater);
                            Process.Start("http://eall4windows.com/Files/EAll4Updater.exe");
                        }
                    }
                    Process p = new Process();
                    p.StartInfo.FileName = exepath + "\\EAll4Updater.exe";
                    if (!cBDownloadLangauge.Checked)
                        p.StartInfo.Arguments = "-skipLang";
                    if (Global.AdminNeeded())
                        p.StartInfo.Verb = "runas";
                    try { p.Start(); Close(); }
                    catch { }
                }
                else
                    File.Delete(Global.appdatapath + "\\version.txt");
            else
            {
                File.Delete(Global.appdatapath + "\\version.txt");
                MessageBox.Show(Properties.Resources.UpToDate, "EAll4Windows Updater");
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
            Global.Notifications = cBNotifications.Checked;
        }

        private void lLSetup_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process p = new Process();
            p.StartInfo.FileName = Assembly.GetExecutingAssembly().Location;
            p.StartInfo.Arguments = "-driverinstall";
            p.StartInfo.Verb = "runas";
            try { p.Start(); }
            catch { }
            //WelcomeDialog wd = new WelcomeDialog();
            //wd.ShowDialog();
            tabSettings.Text = originalsettingstext;
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
            if (cBCloseMini.Checked && !contextclose)
            {
                this.WindowState = FormWindowState.Minimized;
                e.Cancel = true;
                return;
            }
            if (systemShutdown)
            // Reset the variable because the user might cancel the 
            // shutdown.
            {
                systemShutdown = false;
                EAll4LightBar.shuttingdown = true;
            }
            if (oldsize == new System.Drawing.Size(0, 0))
            {
                Global.FormWidth = this.Width;
                Global.FormHeight = this.Height;
            }
            else
            {
                Global.FormWidth = oldsize.Width;
                Global.FormHeight = oldsize.Height;
            }
            if (!String.IsNullOrEmpty(Global.appdatapath))
            {
                Global.Save();
                Program.rootHub.Stop();
            }
        }

        private void cBSwipeProfiles_CheckedChanged(object sender, EventArgs e)
        {
            Global.SwipeProfiles = cBSwipeProfiles.Checked;
        }

        private void cBQuickCharge_CheckedChanged(object sender, EventArgs e)
        {
            Global.QuickCharge = cBQuickCharge.Checked;
        }

        private void lbLastMessage_MouseHover(object sender, EventArgs e)
        {
            toolTip1.Show(lbLastMessage.Text, lbLastMessage, -3, -3);
        }

        private void pnlButton_MouseLeave(object sender, EventArgs e)
        {
            toolTip1.Hide(lbLastMessage);
        }

        private void pnlXIPorts_MouseEnter(object sender, EventArgs e)
        {
            //oldxiport = (int)Math.Round(nUDXIPorts.Value,0);
        }
        int oldxiport;
        private void pnlXIPorts_MouseLeave(object sender, EventArgs e)
        {

        }

        private void nUDXIPorts_ValueChanged(object sender, EventArgs e)
        {
            lbLastXIPort.Text = "- " + ((int)Math.Round(nUDXIPorts.Value, 0) + 3);
        }

        private void nUDXIPorts_Leave(object sender, EventArgs e)
        {
            if (oldxiport != (int)Math.Round(nUDXIPorts.Value, 0))
            {
                oldxiport = (int)Math.Round(nUDXIPorts.Value, 0);
                Global.FirstXinputPort = oldxiport;
                Program.rootHub.x360Bus.FirstController = oldxiport;
                btnStartStop_Click(sender, e);
                btnStartStop_Click(sender, e);
            }
        }

        private void nUDXIPorts_Enter(object sender, EventArgs e)
        {
            oldxiport = (int)Math.Round(nUDXIPorts.Value, 0);
        }

        private void cBCloseMini_CheckedChanged(object sender, EventArgs e)
        {
            Global.CloseMini = cBCloseMini.Checked;
        }

        private void Pads_MouseHover(object sender, EventArgs e)
        {
            Label lb = (Label)sender;
            int i = Int32.Parse(lb.Tag.ToString());
            if (Program.rootHub.EAll4Controllers[i] != null && Program.rootHub.EAll4Controllers[i].ConnectionType == ConnectionType.BT)
            {
                double latency = Program.rootHub.EAll4Controllers[i].Latency;
                toolTip1.Hide(Pads[i]);
                toolTip1.Show(Properties.Resources.InputDelay.Replace("*number*", latency.ToString()), lb, lb.Size.Width, 0);
            }
        }
        private void Pads_MouseLeave(object sender, EventArgs e)
        {
            toolTip1.Hide((Label)sender);
        }

        private void cBDownloadLangauge_CheckedChanged(object sender, EventArgs e)
        {
            Global.DownloadLang = cBDownloadLangauge.Checked;
        }

        private void cBFlashWhenLate_CheckedChanged(object sender, EventArgs e)
        {
            Global.FlashWhenLate = cBFlashWhenLate.Checked;
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
