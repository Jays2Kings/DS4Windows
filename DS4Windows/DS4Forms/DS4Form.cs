using System;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using System.Net;
using System.Drawing;
using System.Diagnostics;
using System.Xml;
using System.Text;
using Microsoft.Win32.TaskScheduler;
using System.Security.Principal;
using System.Threading;
using System.Drawing.Drawing2D;
using TaskRunner = System.Threading.Tasks.Task;
using NonFormTimer = System.Timers.Timer;
using static DS4Windows.Global;
using System.Security;
using System.Management;

namespace DS4Windows
{
    [SuppressUnmanagedCodeSecurity]
    public partial class DS4Form : Form
    {
        public string[] cmdArguments;
        delegate void LogDebugDelegate(DateTime Time, String Data, bool warning);
        delegate void NotificationDelegate(object sender, DebugEventArgs args);
        delegate void DeviceStatusChangedDelegate(object sender, DeviceStatusChangeEventArgs args);
        delegate void DeviceSerialChangedDelegate(object sender, SerialChangeArgs args);
        private Label[] Pads, Batteries;
        private ComboBox[] cbs;
        private Button[] ebns;
        private Button[] lights;
        private PictureBox[] statPB;
        private ToolStripMenuItem[] shortcuts;
        private ToolStripMenuItem[] disconnectShortcuts;
        protected CheckBox[] linkedProfileCB;
        NonFormTimer hotkeysTimer = null;// new NonFormTimer();
        NonFormTimer autoProfilesTimer = null;// new NonFormTimer();
        string tempProfileProgram = string.Empty;
        double dpix, dpiy;
        List<string> profilenames = new List<string>();
        List<string> programpaths = new List<string>();
        List<string>[] proprofiles;
        List<bool> turnOffTempProfiles;
        
        private bool systemShutdown = false;
        private bool wasrunning = false;
        Options opt;
        private bool optPop;
        public Size oldsize;
        bool contextclose;
        bool turnOffTemp;
        bool runningBat;
        private bool changingService;
        private IntPtr regHandle = new IntPtr();
        private ManagementEventWatcher managementEvWatcher;
        Dictionary<Control, string> hoverTextDict = new Dictionary<Control, string>();
        // 0 index is used for application version text. 1 - 4 indices are used for controller status
        string[] notifyText = new string[5]
            { "DS4Windows v" + FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).FileVersion,
            string.Empty, string.Empty, string.Empty, string.Empty };

        private const string UPDATER_VERSION = "1.3.1";
        private const int WM_QUERYENDSESSION = 0x11;
        private const int WM_CLOSE = 0x10;
        internal string updaterExe = Environment.Is64BitProcess ? "DS4Updater.exe" : "DS4Updater_x86.exe";

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport("kernel32.dll")]
        private static extern IntPtr OpenProcess(uint dwDesiredAccess, bool bInheritHandle, uint dwProcessId);

        [DllImport("kernel32.dll")]
        private static extern bool CloseHandle(IntPtr handle);

        [DllImport("psapi.dll")]
        private static extern uint GetModuleFileNameEx(IntPtr hWnd, IntPtr hModule, StringBuilder lpFileName, int nSize);

        public DS4Form(string[] args)
        {
            Global.FindConfigLocation();

            if (Global.firstRun)
            {
                new SaveWhere(Global.multisavespots).ShowDialog();
            }

            Global.Load();
            Global.SetCulture(UseLang);

            InitializeComponent();

            bnEditC1.Tag = 0;
            bnEditC2.Tag = 1;
            bnEditC3.Tag = 2;
            bnEditC4.Tag = 3;

            StartWindowsCheckBox.CheckedChanged -= StartWindowsCheckBox_CheckedChanged;

            saveProfiles.Filter = Properties.Resources.XMLFiles + "|*.xml";
            openProfiles.Filter = Properties.Resources.XMLFiles + "|*.xml";
            cmdArguments = args;

            Pads = new Label[4] { lbPad1, lbPad2, lbPad3, lbPad4 };
            Batteries = new Label[4] { lbBatt1, lbBatt2, lbBatt3, lbBatt4 };
            cbs = new ComboBox[4] { cBController1, cBController2, cBController3, cBController4 };
            ebns = new Button[4] { bnEditC1, bnEditC2, bnEditC3, bnEditC4 };
            lights = new Button[4] { bnLight1, bnLight2, bnLight3, bnLight4 };
            statPB = new PictureBox[4] { pBStatus1, pBStatus2, pBStatus3, pBStatus4 };
            shortcuts = new ToolStripMenuItem[4] { (ToolStripMenuItem)notifyIcon1.ContextMenuStrip.Items[0],
                (ToolStripMenuItem)notifyIcon1.ContextMenuStrip.Items[1],
                (ToolStripMenuItem)notifyIcon1.ContextMenuStrip.Items[2],
                (ToolStripMenuItem)notifyIcon1.ContextMenuStrip.Items[3] };
            disconnectShortcuts = new ToolStripMenuItem[4]
            {
                discon1toolStripMenuItem, discon2ToolStripMenuItem,
                discon3ToolStripMenuItem, discon4ToolStripMenuItem
            };

            linkedProfileCB = new CheckBox[4] { linkCB1, linkCB2, linkCB3, linkCB4 };

            WqlEventQuery q = new WqlEventQuery();
            ManagementScope scope = new ManagementScope("root\\CIMV2");
            q.EventClassName = "Win32_PowerManagementEvent";
            managementEvWatcher = new ManagementEventWatcher(scope, q);
            managementEvWatcher.EventArrived += PowerEventArrive;
            managementEvWatcher.Start();

            tSOptions.Visible = false;

            TaskRunner.Run(() => CheckDrivers());

            if (string.IsNullOrEmpty(appdatapath))
            {
                Close();
                return;
            }

            Graphics g = CreateGraphics();
            try
            {
                dpix = g.DpiX / 100f * 1.041666666667f;
                dpiy = g.DpiY / 100f * 1.041666666667f;
            }
            finally
            {
                g.Dispose();
            }

            blankControllerTab();

            Directory.CreateDirectory(appdatapath);
            if (!Save()) //if can't write to file
            {
                if (MessageBox.Show("Cannot write at current location\nCopy Settings to appdata?", "DS4Windows",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                {
                    try
                    {
                        Directory.CreateDirectory(appDataPpath);
                        File.Copy(exepath + "\\Profiles.xml", appDataPpath + "\\Profiles.xml");
                        File.Copy(exepath + "\\Auto Profiles.xml", appDataPpath + "\\Auto Profiles.xml");
                        Directory.CreateDirectory(appDataPpath + "\\Profiles");
                        foreach (string s in Directory.GetFiles(exepath + "\\Profiles"))
                        {
                            File.Copy(s, appDataPpath + "\\Profiles\\" + Path.GetFileName(s));
                        }
                    }
                    catch { }
                    MessageBox.Show("Copy complete, please relaunch DS4Windows and remove settings from Program Directory", "DS4Windows");
                }
                else
                {
                    MessageBox.Show("DS4Windows cannot edit settings here, This will now close", "DS4Windows");
                }

                appdatapath = null;
                Close();
                return;
            }

            cBUseWhiteIcon.Checked = UseWhiteIcon;
            Icon = Properties.Resources.DS4W;
            notifyIcon1.Icon = UseWhiteIcon ? Properties.Resources.DS4W___White : Properties.Resources.DS4W;
            populateNotifyText();
            foreach (ToolStripMenuItem t in shortcuts)
                t.DropDownItemClicked += Profile_Changed_Menu;

            hideDS4CheckBox.CheckedChanged -= hideDS4CheckBox_CheckedChanged;
            hideDS4CheckBox.Checked = UseExclusiveMode;
            hideDS4CheckBox.CheckedChanged += hideDS4CheckBox_CheckedChanged;

            cBDisconnectBT.Checked = DCBTatStop;
            cBQuickCharge.Checked = QuickCharge;
            cBCustomSteam.Checked = UseCustomSteamFolder;
            tBSteamFolder.Text = CustomSteamFolder;
            // New settings
            this.Width = FormWidth;
            this.Height = FormHeight;
            Location = new Point(FormLocationX, FormLocationY);
            startMinimizedCheckBox.CheckedChanged -= startMinimizedCheckBox_CheckedChanged;
            startMinimizedCheckBox.Checked = StartMinimized;
            startMinimizedCheckBox.CheckedChanged += startMinimizedCheckBox_CheckedChanged;

            mintoTaskCheckBox.Checked = Global.MinToTaskbar;
            mintoTaskCheckBox.CheckedChanged += MintoTaskCheckBox_CheckedChanged;

            cBCloseMini.Checked = CloseMini;

            cBFlashWhenLate.Checked = FlashWhenLate;
            nUDLatency.Value = FlashWhenLateAt;

            if (!LoadActions()) //if first no actions have been made yet, create PS+Option to D/C and save it to every profile
            {
                Global.CreateStdActions();
            }

            bool start = true;
            bool mini = false;
            for (int i = 0, argslen = cmdArguments.Length; i < argslen; i++)
            {
                if (cmdArguments[i] == "-stop")
                    start = false;
                else if (cmdArguments[i] == "-m")
                    mini = true;

                if (mini && start)
                    break;
            }

            if (startMinimizedCheckBox.Checked || mini)
            {
                WindowState = FormWindowState.Minimized;
            }

            RefreshProfiles();
            /*opt = new Options(this);
            opt.Icon = this.Icon;
            opt.TopLevel = false;
            opt.Dock = DockStyle.None;
            opt.FormBorderStyle = FormBorderStyle.None;
            */
            //tabProfiles.Controls.Add(opt);

            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location);
            string version = fvi.FileVersion;
            LogDebug(DateTime.Now, "DS4Windows version " + version, false);

            Global.BatteryStatusChange += BatteryStatusUpdate;
            Global.ControllerRemoved += ControllerRemovedChange;
            Global.DeviceStatusChange += DeviceStatusChanged;
            Global.DeviceSerialChange += DeviceSerialChanged;

            Enable_Controls(0, false);
            Enable_Controls(1, false);
            Enable_Controls(2, false);
            Enable_Controls(3, false);
            btnStartStop.Text = Properties.Resources.StartText;

            startToolStripMenuItem.Text = btnStartStop.Text;
            cBoxNotifications.SelectedIndex = Notifications;
            //cBSwipeProfiles.Checked = SwipeProfiles;
            int checkwhen = CheckWhen;
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

            if (File.Exists(exepath + "\\Updater.exe"))
            {
                Thread.Sleep(2000);
                File.Delete(exepath + "\\Updater.exe");
            }

            if (!Directory.Exists(appdatapath + "\\Virtual Bus Driver"))
                linkUninstall.Visible = false;

            bool isElevated = IsAdministrator();
            if (!isElevated)
            {
                Image tempImg = new Bitmap(uacPictureBox.Width, uacPictureBox.Height);
                AddUACShieldToImage(tempImg);
                uacPictureBox.BackgroundImage = tempImg;
                uacPictureBox.Visible = true;
                new ToolTip().SetToolTip(uacPictureBox, Properties.Resources.UACTask);
                runStartTaskRadio.Enabled = false;
            }
            else
            {
                runStartTaskRadio.Enabled = true;
            }

            if (File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.Startup) + "\\DS4Windows.lnk"))
            {
                StartWindowsCheckBox.Checked = true;
                runStartupPanel.Visible = true;

                string lnkpath = WinProgs.ResolveShortcutAndArgument(Environment.GetFolderPath(Environment.SpecialFolder.Startup) + "\\DS4Windows.lnk");
                string onlylnkpath = WinProgs.ResolveShortcut(Environment.GetFolderPath(Environment.SpecialFolder.Startup) + "\\DS4Windows.lnk");
                if (!lnkpath.EndsWith("-runtask"))
                {
                    runStartProgRadio.Checked = true;
                }
                else
                {
                    runStartTaskRadio.Checked = true;
                }

                if (onlylnkpath != Process.GetCurrentProcess().MainModule.FileName)
                {
                    File.Delete(Environment.GetFolderPath(Environment.SpecialFolder.Startup) + "\\DS4Windows.lnk");
                    appShortcutToStartup();
                    changeStartupRoutine();
                }
            }

            StartWindowsCheckBox.CheckedChanged += new EventHandler(StartWindowsCheckBox_CheckedChanged);
            new ToolTip().SetToolTip(StartWindowsCheckBox, Properties.Resources.RunAtStartup);

            ckUdpServ.Checked = nUDUdpPortNum.Enabled = isUsingUDPServer();
            nUDUdpPortNum.Value = getUDPServerPortNum();

            ckUdpServ.CheckedChanged += CkUdpServ_CheckedChanged;
            nUDUdpPortNum.Leave += NUDUdpPortNum_Leave;

            populateHoverTextDict();

            cBController1.KeyPress += CBController_KeyPress;
            cBController2.KeyPress += CBController_KeyPress;
            cBController3.KeyPress += CBController_KeyPress;
            cBController4.KeyPress += CBController_KeyPress;

            foreach (Control control in fLPSettings.Controls)
            {
                string tempst;
                if (control.HasChildren)
                {
                    foreach (Control ctrl in control.Controls)
                    {
                        if (hoverTextDict.TryGetValue(ctrl, out tempst))
                        {
                            ctrl.MouseHover += Items_MouseHover;
                        }
                        else
                        {
                            ctrl.MouseHover += ClearLastMessage;
                        }
                    }
                }
                else
                {
                    if (hoverTextDict.TryGetValue(control, out tempst))
                        control.MouseHover += Items_MouseHover;
                    else
                        control.MouseHover += ClearLastMessage;
                }
            }

            this.Resize += Form_Resize;
            this.LocationChanged += TrackLocationChanged;
            if (!(StartMinimized || mini))
                Form_Resize(null, null);

            Program.rootHub.Debug += On_Debug;

            AppLogger.GuiLog += On_Debug;
            AppLogger.TrayIconLog += ShowNotification;
            LoadLinkedProfiles();

            TaskRunner.Delay(50).ContinueWith((t) =>
            {
                if (checkwhen > 0 && DateTime.Now >= LastChecked + TimeSpan.FromHours(checkwhen))
                {
                    this.BeginInvoke((System.Action)(() =>
                    {
                        // Sorry other devs, gonna have to find your own server
                        Uri url = new Uri("https://raw.githubusercontent.com/Ryochan7/DS4Windows/jay/DS4Windows/newest.txt");
                        WebClient wc = new WebClient();
                        wc.DownloadFileAsync(url, appdatapath + "\\version.txt");
                        wc.DownloadFileCompleted += (sender, e) => { TaskRunner.Run(() => Check_Version(sender, e)); };
                        LastChecked = DateTime.Now;
                    }));
                }

                UpdateTheUpdater();
            });

            if (btnStartStop.Enabled && start)
            {
                TaskRunner.Delay(100).ContinueWith((t) => {
                    this.BeginInvoke((System.Action)(() => BtnStartStop_Clicked()));
                });
            }

            Thread timerThread = new Thread(() =>
            {
                hotkeysTimer = new NonFormTimer();
                //hotkeysTimer.Elapsed += Hotkeys;
                hotkeysTimer.AutoReset = false;
                if (SwipeProfiles)
                {
                    ChangeHotkeysStatus(true);
                    //hotkeysTimer.Start();
                }

                autoProfilesTimer = new NonFormTimer();
                //autoProfilesTimer.Elapsed += CheckAutoProfiles;
                autoProfilesTimer.Interval = 1000;
                autoProfilesTimer.AutoReset = false;

                LoadP();

                this.BeginInvoke((System.Action)(() =>
                {
                    cBSwipeProfiles.Checked = SwipeProfiles;
                }));
            });
            timerThread.IsBackground = true;
            timerThread.Priority = ThreadPriority.Lowest;
            timerThread.Start();
        }

        private void populateHoverTextDict()
        {
            hoverTextDict.Clear();
            hoverTextDict[linkUninstall] = Properties.Resources.IfRemovingDS4Windows;
            hoverTextDict[cBSwipeProfiles] = Properties.Resources.TwoFingerSwipe;
            hoverTextDict[cBQuickCharge] = Properties.Resources.QuickCharge;
            hoverTextDict[cBCloseMini] = Properties.Resources.CloseMinimize;
            hoverTextDict[uacPictureBox] = Properties.Resources.UACTask;
            hoverTextDict[StartWindowsCheckBox] = Properties.Resources.RunAtStartup;
        }

        private void AddUACShieldToImage(Image image)
        {
            Bitmap shield = SystemIcons.Shield.ToBitmap();
            shield.MakeTransparent();

            Graphics g = Graphics.FromImage(image);
            g.CompositingMode = CompositingMode.SourceOver;
            double aspectRatio = shield.Width / (double)shield.Height;
            int finalWidth = Convert.ToInt32(image.Height * aspectRatio);
            int finalHeight = Convert.ToInt32(image.Width / aspectRatio);
            g.DrawImage(shield, new Rectangle(0, 0, finalWidth, finalHeight));
        }

        private void ClearLastMessage(object sender, EventArgs e)
        {
            lbLastMessage.Text = "";
            lbLastMessage.ForeColor = SystemColors.GrayText;
        }

        private void ChangeAutoProfilesStatus(bool state)
        {
            if (state)
            {
                autoProfilesTimer.Elapsed += CheckAutoProfiles;
                autoProfilesTimer.Start();
            }
            else
            {
                autoProfilesTimer.Stop();
                autoProfilesTimer.Elapsed -= CheckAutoProfiles;
            }
        }

        private void ChangeHotkeysStatus(bool state)
        {
            if (state)
            {
                hotkeysTimer.Elapsed += Hotkeys;
                hotkeysTimer.Start();
            }
            else
            {
                hotkeysTimer.Stop();
                hotkeysTimer.Elapsed -= Hotkeys;
            }
        }

        private void blankControllerTab()
        {
            for (int Index = 0, PadsLen = Pads.Length;
                Index < PadsLen; Index++)
            {
                if (Index < ControlService.DS4_CONTROLLER_COUNT)
                {
                    statPB[Index].Visible = false;
                    toolTip1.SetToolTip(statPB[Index], "");
                    Batteries[Index].Text = Properties.Resources.NA;
                    Pads[Index].Text = Properties.Resources.Disconnected;
                    Enable_Controls(Index, false);
                }
            }

            lbNoControllers.Visible = true;
            tLPControllers.Visible = false;
        }

        private void UpdateTheUpdater()
        {
            if (File.Exists(exepath + "\\Update Files\\DS4Updater.exe"))
            {
                Process[] processes = Process.GetProcessesByName("DS4Updater");
                while (processes.Length > 0)
                {
                    Thread.Sleep(500);
                }

                File.Delete(exepath + "\\DS4Updater.exe");
                File.Move(exepath + "\\Update Files\\DS4Updater.exe", exepath + "\\DS4Updater.exe");
                Directory.Delete(exepath + "\\Update Files");
            }
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

        private void PowerEventArrive(object sender, EventArrivedEventArgs e)
        {
            short evType = Convert.ToInt16(e.NewEvent.GetPropertyValue("EventType"));
            switch (evType)
            {
                case 7:
                {
                    if (btnStartStop.Text == Properties.Resources.StartText && wasrunning)
                    {
                        DS4LightBar.shuttingdown = false;
                        wasrunning = false;
                        Program.rootHub.suspending = false;
                        this.Invoke((System.Action)(() => BtnStartStop_Clicked()));
                    }

                    break;
                }
                case 4:
                {
                    if (btnStartStop.Text == Properties.Resources.StopText)
                    {
                        DS4LightBar.shuttingdown = true;
                        Program.rootHub.suspending = true;
                        this.Invoke((System.Action)(() => BtnStartStop_Clicked()));
                        wasrunning = true;
                    }

                    break;
                }
                default:
                    break;
            }
        }

        void Hotkeys(object sender, EventArgs e)
        {
            hotkeysTimer.Stop();

            if (SwipeProfiles)
            {
                for (int i = 0; i < 4; i++)
                {
                    string slide = Program.rootHub.TouchpadSlide(i);
                    if (slide == "left")
                    {
                        int ind = i;
                        this.BeginInvoke((System.Action)(() =>
                        {
                            if (cbs[ind].SelectedIndex <= 0)
                                cbs[ind].SelectedIndex = cbs[ind].Items.Count - 2;
                            else
                                cbs[ind].SelectedIndex--;
                        }));
                    }
                    else if (slide == "right")
                    {
                        int ind = i;
                        this.BeginInvoke((System.Action)(() =>
                        {
                            if (cbs[ind].SelectedIndex == cbs[ind].Items.Count - 2)
                                cbs[ind].SelectedIndex = 0;
                            else
                                cbs[ind].SelectedIndex++;
                        }));
                    }

                    if (slide.Contains("t"))
                    {
                        int ind = i;
                        this.BeginInvoke((System.Action)(() =>
                        {
                            ShowNotification(this, Properties.Resources.UsingProfile.Replace("*number*", (ind + 1).ToString()).Replace("*Profile name*", cbs[ind].Text));
                        }));
                    }
                }
            }

            if (bat != null && bat.HasExited && runningBat)
            {
                Process.Start("explorer.exe");
                bat = null;
                runningBat = false;
            }

            hotkeysTimer.Start();
        }

        private void CheckAutoProfiles(object sender, EventArgs e)
        {
            autoProfilesTimer.Stop();

            //Check for process for auto profiles
            if (string.IsNullOrEmpty(tempProfileProgram))
            {
                string windowName = GetTopWindowName().ToLower().Replace('/', '\\');
                for (int i = 0, pathsLen = programpaths.Count; i < pathsLen; i++)
                {
                    string name = programpaths[i].ToLower().Replace('/', '\\');
                    if (name == windowName)
                    {
                        for (int j = 0; j < 4; j++)
                        {
                            if (proprofiles[j][i] != "(none)" && proprofiles[j][i] != Properties.Resources.noneProfile)
                            {
                                LoadTempProfile(j, proprofiles[j][i], true, Program.rootHub); // j is controller index, i is filename
                                //if (LaunchProgram[j] != string.Empty) Process.Start(LaunchProgram[j]);
                            }
                        }

                        if (turnOffTempProfiles[i])
                        {
                            turnOffTemp = true;
                            if (btnStartStop.Text == Properties.Resources.StopText)
                            {
                                //autoProfilesTimer.Stop();
                                //hotkeysTimer.Stop();
                                ChangeAutoProfilesStatus(false);
                                ChangeHotkeysStatus(false);

                                this.Invoke((System.Action)(() => {
                                    this.changingService = true;
                                    BtnStartStop_Clicked();
                                }));

                                while (this.changingService)
                                {
                                    Thread.SpinWait(500);
                                }

                                this.Invoke((System.Action)(() =>
                                {
                                    //hotkeysTimer.Start();
                                    ChangeHotkeysStatus(true);
                                    ChangeAutoProfilesStatus(true);
                                    //autoProfilesTimer.Start();
                                }));
                            }
                        }

                        tempProfileProgram = name;
                        break;
                    }
                }
            }
            else
            {
                string windowName = GetTopWindowName().ToLower().Replace('/', '\\');
                if (tempProfileProgram != windowName)
                {
                    tempProfileProgram = string.Empty;
                    for (int j = 0; j < 4; j++)
                        LoadProfile(j, false, Program.rootHub);

                    if (turnOffTemp)
                    {
                        turnOffTemp = false;
                        if (btnStartStop.Text == Properties.Resources.StartText)
                        {
                            this.BeginInvoke((System.Action)(() =>
                            {
                                BtnStartStop_Clicked();
                            }));
                        }
                    }
                }
            }

            autoProfilesTimer.Start();
            //GC.Collect();
        }

        public void LoadP()
        {
            XmlDocument doc = new XmlDocument();
            proprofiles = new List<string>[4] { new List<string>(), new List<string>(),
                new List<string>(), new List<string>() };
            turnOffTempProfiles = new List<bool>();
            programpaths.Clear();
            if (!File.Exists(appdatapath + "\\Auto Profiles.xml"))
                return;

            doc.Load(appdatapath + "\\Auto Profiles.xml");
            XmlNodeList programslist = doc.SelectNodes("Programs/Program");
            foreach (XmlNode x in programslist)
                programpaths.Add(x.Attributes["path"].Value);

            foreach (string s in programpaths)
            {
                for (int i = 0; i < 4; i++)
                {
                    proprofiles[i].Add(doc.SelectSingleNode("/Programs/Program[@path=\"" + s + "\"]"
                        + "/Controller" + (i + 1)).InnerText);
                }

                XmlNode item = doc.SelectSingleNode("/Programs/Program[@path=\"" + s + "\"]"
                        + "/TurnOff");
                bool turnOff;
                if (item != null && bool.TryParse(item.InnerText, out turnOff))
                    turnOffTempProfiles.Add(turnOff);
                else
                    turnOffTempProfiles.Add(false);
            }

            int pathCount = programpaths.Count;
            bool timerEnabled = autoProfilesTimer.Enabled;
            if (pathCount > 0 && !timerEnabled)
            {
                ChangeAutoProfilesStatus(true);
                //autoProfilesTimer.Start();
            }
            else if (pathCount == 0 && timerEnabled)
            {
                //autoProfilesTimer.Stop();
                ChangeAutoProfilesStatus(false);
            }
        }

        string originalsettingstext;
        private void CheckDrivers()
        {
            originalsettingstext = tabSettings.Text;
            bool deriverinstalled = false;
            deriverinstalled = Global.IsViGEmBusInstalled();
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

        private void Check_Version(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location);
            string version = fvi.FileVersion;
            string newversion = File.ReadAllText(appdatapath + "\\version.txt").Trim();
            if (!string.IsNullOrWhiteSpace(newversion) && version.Replace(',', '.').CompareTo(newversion) != 0)
            {
                if ((DialogResult)this.Invoke(new Func<DialogResult>(() => {
                    return MessageBox.Show(Properties.Resources.DownloadVersion.Replace("*number*", newversion),
Properties.Resources.DS4Update, MessageBoxButtons.YesNo, MessageBoxIcon.Question); })) == DialogResult.Yes)
                {
                    if (!File.Exists(exepath + "\\DS4Updater.exe") || (File.Exists(exepath + "\\DS4Updater.exe")
                        && (FileVersionInfo.GetVersionInfo(exepath + "\\DS4Updater.exe").FileVersion.CompareTo(UPDATER_VERSION) != 0)))
                    {
                        Uri url2 = new Uri($"https://github.com/Ryochan7/DS4Updater/releases/download/v{UPDATER_VERSION}/{updaterExe}");
                        WebClient wc2 = new WebClient();
                        if (appdatapath == exepath)
                        {
                            wc2.DownloadFile(url2, exepath + "\\DS4Updater.exe");
                            Process p = new Process();
                            p.StartInfo.FileName = exepath + "\\DS4Updater.exe";
                            p.StartInfo.Arguments = "-autolaunch";
                            if (AdminNeeded())
                                p.StartInfo.Verb = "runas";

                            try { p.Start(); Close(); }
                            catch { }
                        }
                        else
                        {
                            this.BeginInvoke((System.Action)(() => MessageBox.Show(Properties.Resources.PleaseDownloadUpdater)));
                            Process.Start($"https://github.com/Ryochan7/DS4Updater/releases/download/v{UPDATER_VERSION}/{updaterExe}");
                        }
                    }
                }
                else
                    File.Delete(appdatapath + "\\version.txt");
            }
            else
                File.Delete(appdatapath + "\\version.txt");
        }

        public void RefreshProfiles()
        {
            try
            {
                profilenames.Clear();
                string[] profiles = Directory.GetFiles(appdatapath + @"\Profiles\");
                foreach (string s in profiles)
                {
                    if (s.EndsWith(".xml"))
                        profilenames.Add(Path.GetFileNameWithoutExtension(s));
                }

                lBProfiles.Items.Clear();
                lBProfiles.Items.AddRange(profilenames.ToArray());
                if (lBProfiles.Items.Count == 0)
                {
                    SaveProfile(0, "Default");
                    ProfilePath[0] = OlderProfilePath[0] = "Default";
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

                    for (int j = 0, itemCount = cbs[i].Items.Count; j < itemCount; j++)
                    {
                        if (cbs[i].Items[j].ToString() == Path.GetFileNameWithoutExtension(ProfilePath[i]))
                        {
                            cbs[i].SelectedIndex = j;
                            ((ToolStripMenuItem)shortcuts[i].DropDownItems[j]).Checked = true;
                            ProfilePath[i] = OlderProfilePath[i] = cbs[i].Text;
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
            }
            catch (DirectoryNotFoundException)
            {
                Directory.CreateDirectory(appdatapath + @"\Profiles\");
                SaveProfile(0, "Default");
                ProfilePath[0] = OlderProfilePath[0] = "Default";
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
            WinProgs WP = new WinProgs(profilenames.ToArray(), this);
            WP.TopLevel = false;
            WP.FormBorderStyle = FormBorderStyle.None;
            WP.Visible = true;
            WP.Dock = DockStyle.Fill;
            tabAutoProfiles.Controls.Add(WP);
        }

        protected void LogDebug(DateTime Time, String Data, bool warning)
        {
            if (this.InvokeRequired)
            {
                LogDebugDelegate d = new LogDebugDelegate(LogDebug);
                try
                {
                    // Make sure to invoke method asynchronously instead of waiting for result
                    this.BeginInvoke(d, new object[] { Time, Data, warning });
                    //this.Invoke(d, new object[] { Time, Data, warning });
                }
                catch { }
            }
            else
            {
                String Posted = Time.ToString("G");
                lvDebug.Items.Add(new ListViewItem(new String[] { Posted, Data })).EnsureVisible();
                if (warning) lvDebug.Items[lvDebug.Items.Count - 1].ForeColor = Color.Red;
                // Added alternative
                lbLastMessage.Text = Data;
                lbLastMessage.ForeColor = (warning ? Color.Red : SystemColors.GrayText);
            }
        }

        protected void ShowNotification(object sender, DebugEventArgs args)
        {
            if (this.InvokeRequired)
            {
                NotificationDelegate d = new NotificationDelegate(ShowNotification);

                try
                {
                    // Make sure to invoke method asynchronously instead of waiting for result
                    this.BeginInvoke(d, new object[] { sender, args });
                }
                catch { }
            }
            else
            {
                if (Form.ActiveForm != this && (Notifications == 2 || (Notifications == 1 && args.Warning) || sender != null))
                {
                    this.notifyIcon1.BalloonTipText = args.Data;
                    notifyIcon1.BalloonTipTitle = "DS4Windows";
                    notifyIcon1.ShowBalloonTip(1);
                }
            }
        }

        protected void ShowNotification(object sender, string text)
        {
            if (Form.ActiveForm != this && Notifications == 2)
            {
                this.notifyIcon1.BalloonTipText = text;
                notifyIcon1.BalloonTipTitle = "DS4Windows";
                notifyIcon1.ShowBalloonTip(1);
            }
        }

        protected void Form_Resize(object sender, EventArgs e)
        {
            bool minToTask = GetMinToTaskbar();
            if (FormWindowState.Minimized == WindowState && !minToTask)
            {
                Hide();
                ShowInTaskbar = false;
                FormBorderStyle = FormBorderStyle.None;
            }

            else if (FormWindowState.Normal == WindowState && !minToTask)
            {
                //mAllowVisible = true;
                Show();
                ShowInTaskbar = true;
                FormBorderStyle = FormBorderStyle.Sizable;
            }

            if (WindowState != FormWindowState.Minimized)
            {
                FormWidth = Width;
                FormHeight = Height;
            }

            chData.AutoResize(ColumnHeaderAutoResizeStyle.HeaderSize);
        }

        private void TrackLocationChanged(object sender, EventArgs e)
        {
            if (WindowState != FormWindowState.Minimized)
            {
                FormLocationX = Location.X;
                FormLocationY = Location.Y;
            }
        }

        private void BtnStartStop_Click(object sender, EventArgs e)
        {
            BtnStartStop_Clicked();
        }

        private void ServiceStartup(bool log)
        {
            var uiContext = SynchronizationContext.Current;
            changingService = true;
            TaskRunner.Run(() =>
            {
                //Thread.CurrentThread.Priority = ThreadPriority.AboveNormal;
                Program.rootHub.Start(uiContext, log);
                Invoke((System.Action)(() => { ServiceStartupFinish(); }));
                changingService = false;
            });
        }

        private void ServiceStartupFinish()
        {
            if (SwipeProfiles && !hotkeysTimer.Enabled)
            {
                ChangeHotkeysStatus(true);
                //hotkeysTimer.Start();
            }

            if (programpaths.Count > 0 && !autoProfilesTimer.Enabled)
            {
                ChangeAutoProfilesStatus(true);
                //autoProfilesTimer.Start();
            }

            startToolStripMenuItem.Text = btnStartStop.Text = Properties.Resources.StopText;
        }

        private void ServiceShutdown(bool log)
        {
            changingService = true;
            TaskRunner.Run(() =>
            {
                Program.rootHub.Stop(log);
                Invoke((System.Action)(() => { ServiceShutdownFinish(); }));
                changingService = false;
            });
        }

        private void ServiceShutdownFinish()
        {
            ChangeAutoProfilesStatus(false);
            ChangeHotkeysStatus(false);
            //hotkeysTimer.Stop();
            //autoProfilesTimer.Stop();
            startToolStripMenuItem.Text = btnStartStop.Text = Properties.Resources.StartText;
            blankControllerTab();
            populateFullNotifyText();
        }

        public void BtnStartStop_Clicked(bool log = true)
        {
            if (btnStartStop.Text == Properties.Resources.StartText)
            {
                ServiceStartup(log);
            }
            else if (btnStartStop.Text == Properties.Resources.StopText)
            {
                blankControllerTab();
                ServiceShutdown(log);
            }
        }

        protected void BtnClear_Click(object sender, EventArgs e)
        {
            lvDebug.Items.Clear();
            lbLastMessage.Text = string.Empty;
        }

        private bool inHotPlug = false;
        private int hotplugCounter = 0;
        private object hotplugCounterLock = new object();
        private const int DBT_DEVNODES_CHANGED = 0x0007;
        private const int DBT_DEVICEARRIVAL = 0x8000;
        private const int DBT_DEVICEREMOVECOMPLETE = 0x8004;
        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case Util.WM_CREATE:
                {
                    Guid hidGuid = new Guid();
                    NativeMethods.HidD_GetHidGuid(ref hidGuid);
                    bool result = Util.RegisterNotify(this.Handle, hidGuid, ref regHandle);
                    if (!result)
                    {
                        ScpForm_Closing(this,
                            new FormClosingEventArgs(CloseReason.ApplicationExitCall, false));
                    }
                    break;
                }
                case Util.WM_DEVICECHANGE:
                {
                    if (runHotPlug)
                    {
                        Int32 Type = m.WParam.ToInt32();
                        if (Type == DBT_DEVICEARRIVAL ||
                            Type == DBT_DEVICEREMOVECOMPLETE)
                        {
                            lock (hotplugCounterLock)
                            {
                                hotplugCounter++;
                            }

                            if (!inHotPlug)
                            {
                                inHotPlug = true;
                                TaskRunner.Run(() => { InnerHotplug2(); });
                            }
                        }
                    }
                    break;
                }
                case WM_QUERYENDSESSION:
                {
                    systemShutdown = true;
                    break;
                }
                default: break;
            }

            // If this is WM_QUERYENDSESSION, the closing event should be
            // raised in the base WndProc.
            base.WndProc(ref m);
        }

        private void InnerHotplug2()
        {
            inHotPlug = true;

            bool loopHotplug = false;
            lock (hotplugCounterLock)
            {
                loopHotplug = hotplugCounter > 0;
            }

            while (loopHotplug == true)
            {
                Thread.Sleep(1500);
                Program.rootHub.HotPlug();
                //TaskRunner.Run(() => { Program.rootHub.HotPlug(uiContext); });
                lock (hotplugCounterLock)
                {
                    hotplugCounter--;
                    loopHotplug = hotplugCounter > 0;
                }
            }

            inHotPlug = false;
        }

        protected void BatteryStatusUpdate(object sender, BatteryReportArgs args)
        {
            string battery;
            int level = args.getLevel();
            bool charging = args.isCharging();
            int Index = args.getIndex();
            if (charging)
            {
                if (level >= 100)
                    battery = Properties.Resources.Full;
                else
                    battery = level + "%+";
            }
            else
            {
                battery = level + "%";
            }

            Batteries[args.getIndex()].Text = battery;

            // Update device battery level display for tray icon
            generateDeviceNotifyText(args.getIndex());
            populateNotifyText();
        }

        protected void populateFullNotifyText()
        {
            for (int i = 0; i < ControlService.DS4_CONTROLLER_COUNT; i++)
            {
                string temp = Program.rootHub.getShortDS4ControllerInfo(i);
                if (temp != Properties.Resources.NoneText)
                {
                    notifyText[i + 1] = (i + 1) + ": " + temp;
                }
                else
                {
                    notifyText[i + 1] = string.Empty;
                }
            }

            populateNotifyText();
        }

        protected void generateDeviceNotifyText(int index)
        {
            string temp = Program.rootHub.getShortDS4ControllerInfo(index);
            if (temp != Properties.Resources.NoneText)
            {
                notifyText[index + 1] = (index + 1) + ": " + temp;
            }
            else
            {
                notifyText[index + 1] = string.Empty;
            }
        }

        protected void populateNotifyText()
        {
            string tooltip = notifyText[0];
            for (int i = 1; i < 5; i++)
            {
                string temp = notifyText[i];
                if (!string.IsNullOrEmpty(temp))
                {
                    tooltip += "\n" + notifyText[i];
                }
            }

            notifyIcon1.Text = tooltip.Length > 63 ? tooltip.Substring(0, 63) : tooltip; // Carefully stay under the 63 character limit.
        }

        protected void DeviceSerialChanged(object sender, SerialChangeArgs args)
        {
            if (this.InvokeRequired)
            {
                DeviceSerialChangedDelegate d = new DeviceSerialChangedDelegate(DeviceSerialChanged);
                this.BeginInvoke(d, new object[] { sender, args });
            }
            else
            {
                int devIndex = args.getIndex();
                string serial = args.getSerial();
                DS4Device device = (devIndex >= 0 && devIndex < ControlService.DS4_CONTROLLER_COUNT) ?
                    Program.rootHub.DS4Controllers[devIndex] : null;
                if (device != null)
                {
                    Pads[devIndex].Text = serial;
                    if (device.isSynced())
                    {
                        linkedProfileCB[devIndex].Enabled = true;
                    }
                    else
                    {
                        linkedProfileCB[devIndex].Enabled = false;
                    }

                    if (device.isValidSerial() && containsLinkedProfile(device.getMacAddress()))
                    {
                        ProfilePath[devIndex] = getLinkedProfile(device.getMacAddress());
                        int profileIndex = cbs[devIndex].FindString(ProfilePath[devIndex]);
                        if (profileIndex >= 0)
                        {
                            cbs[devIndex].SelectedIndex = profileIndex;
                        }
                    }
                    else
                    {
                        ProfilePath[devIndex] = OlderProfilePath[devIndex];                        
                    }

                    linkedProfileCB[devIndex].Checked = false;
                }
            }
        }

        protected void DeviceStatusChanged(object sender, DeviceStatusChangeEventArgs args)
        {
            if (this.InvokeRequired)
            {
                DeviceStatusChangedDelegate d = new DeviceStatusChangedDelegate(DeviceStatusChanged);
                this.BeginInvoke(d, new object[] { sender, args });
            }
            else
            {
                bool nocontrollers = true;
                for (int i = 0, arlen = Program.rootHub.DS4Controllers.Length; nocontrollers && i < arlen; i++)
                {
                    DS4Device dev = Program.rootHub.DS4Controllers[i];
                    if (dev != null)
                    {
                        nocontrollers = false;
                    }
                }

                //string tooltip = "DS4Windows v" + FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).FileVersion;
                int Index = args.getIndex();
                if (Index >= 0 && Index < ControlService.DS4_CONTROLLER_COUNT)
                {
                    Pads[Index].Text = Program.rootHub.getDS4MacAddress(Index);

                    linkedProfileCB[Index].CheckedChanged -= linkCB_CheckedChanged;
                    if (DS4Device.isValidSerial(Pads[Index].Text))
                    {
                        linkedProfileCB[Index].Checked = containsLinkedProfile(Pads[Index].Text);
                        linkedProfileCB[Index].Enabled = true;
                    }
                    else
                    {
                        linkedProfileCB[Index].Checked = false;
                        linkedProfileCB[Index].Enabled = false;
                    }

                    linkedProfileCB[Index].CheckedChanged += linkCB_CheckedChanged;

                    switch (Program.rootHub.getDS4Status(Index))
                    {
                        case "USB": statPB[Index].Visible = true; statPB[Index].Image = Properties.Resources.USB; toolTip1.SetToolTip(statPB[Index], ""); break;
                        case "BT": statPB[Index].Visible = true; statPB[Index].Image = Properties.Resources.BT; toolTip1.SetToolTip(statPB[Index], "Right click to disconnect"); break;
                        case "SONYWA": statPB[Index].Visible = true; statPB[Index].Image = Properties.Resources.BT; toolTip1.SetToolTip(statPB[Index], "Right click to disconnect"); break;
                        default: statPB[Index].Visible = false; toolTip1.SetToolTip(statPB[Index], ""); break;
                    }

                    Batteries[Index].Text = Program.rootHub.getDS4Battery(Index);
                    int profileIndex = cbs[Index].FindString(ProfilePath[Index]);
                    if (profileIndex >= 0)
                    {
                        cbs[Index].SelectedValueChanged -= Profile_Changed;
                        cbs[Index].SelectedIndex = profileIndex;
                        cbs[Index].SelectedValueChanged += Profile_Changed;
                    }

                    if (UseCustomLed[Index])
                        lights[Index].BackColor = CustomColor[Index].ToColorA;
                    else
                        lights[Index].BackColor = MainColor[Index].ToColorA;

                    if (Pads[Index].Text != String.Empty)
                    {
                        if (runningBat)
                        {
                            SendKeys.Send("A");
                            runningBat = false;
                        }

                        Pads[Index].Enabled = true;
                        if (Pads[Index].Text != Properties.Resources.Connecting)
                        {
                            Enable_Controls(Index, true);
                        }
                    }
                    else
                    {
                        Pads[Index].Text = Properties.Resources.Disconnected;
                        Enable_Controls(Index, false);
                    }

                    generateDeviceNotifyText(Index);
                    populateNotifyText();
                }

                lbNoControllers.Visible = nocontrollers;
                tLPControllers.Visible = !nocontrollers;
            }
        }

        protected void ControllerRemovedChange(object sender, ControllerRemovedArgs args)
        {
            int devIndex = args.getIndex();
            Pads[devIndex].Text = Properties.Resources.Disconnected;
            Enable_Controls(devIndex, false);
            statPB[devIndex].Visible = false;
            toolTip1.SetToolTip(statPB[devIndex], "");

            DS4Device[] devices = Program.rootHub.DS4Controllers;
            int controllerLen = devices.Length;
            bool nocontrollers = true;
            for (Int32 i = 0, PadsLen = Pads.Length; nocontrollers && i < PadsLen; i++)
            {
                DS4Device d = devices[i];
                if (d != null)
                {
                    nocontrollers = false;
                }
            }

            lbNoControllers.Visible = nocontrollers;
            tLPControllers.Visible = !nocontrollers;

            // Update device battery level display for tray icon
            generateDeviceNotifyText(devIndex);
            populateNotifyText();
        }

        private void pBStatus_MouseClick(object sender, MouseEventArgs e)
        {
            int i = Convert.ToInt32(((PictureBox)sender).Tag);
            DS4Device d = Program.rootHub.DS4Controllers[i];
            if (d != null)
            {
                if (e.Button == MouseButtons.Right && Program.rootHub.getDS4Status(i) == "BT" && !d.Charging)
                {
                    d.DisconnectBT();
                }
                else if (e.Button == MouseButtons.Right &&
                    Program.rootHub.getDS4Status(i) == "SONYWA" && !d.Charging)
                {
                    d.DisconnectDongle();
                }
            }
        }

        private void Enable_Controls(int device, bool on)
        {
            DS4Device dev = Program.rootHub.DS4Controllers[device];
            ConnectionType conType = ConnectionType.USB;
            if (dev != null)
                conType = dev.ConnectionType;

            Pads[device].Visible = on;
            ebns[device].Visible = on;
            lights[device].Visible = on;
            cbs[device].Visible = on;
            shortcuts[device].Visible = on;
            Batteries[device].Visible = on;
            linkedProfileCB[device].Visible = on;
            disconnectShortcuts[device].Visible = on && conType != ConnectionType.USB;
        }

        protected void On_Debug(object sender, DebugEventArgs e)
        {
            LogDebug(e.Time, e.Data, e.Warning);
        }

        private void lBProfiles_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (lBProfiles.SelectedIndex >= 0)
                ShowOptions(4, lBProfiles.SelectedItem.ToString());
        }


        private void lBProfiles_KeyDown(object sender, KeyEventArgs e)
        {
            if (lBProfiles.SelectedIndex >= 0 && optPop && !opt.Visible)
            {
                if (e.KeyValue == 13)
                    ShowOptions(4, lBProfiles.SelectedItem.ToString());
                else if (e.KeyValue == 46)
                    tsBDeleteProfle_Click(this, e);
                else if (e.KeyValue == 68 && e.Modifiers == Keys.Control)
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
                if (MessageBox.Show(Properties.Resources.ProfileCannotRestore.Replace("*Profile name*", "\"" + filename + "\""),
                    Properties.Resources.DeleteProfile,
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    File.Delete(appdatapath + @"\Profiles\" + filename + ".xml");
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
                MTB.FormBorderStyle = FormBorderStyle.None;
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
            if (appdatapath == Directory.GetParent(Assembly.GetExecutingAssembly().Location).FullName)
                openProfiles.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\DS4Tool" + @"\Profiles\";
            else
                openProfiles.InitialDirectory = Directory.GetParent(Assembly.GetExecutingAssembly().Location).FullName + @"\Profiles\";

            if (openProfiles.ShowDialog() == DialogResult.OK)
            {
                string[] files = openProfiles.FileNames;
                for (int i = 0, arlen = files.Length; i < arlen; i++)
                    File.Copy(openProfiles.FileNames[i], appdatapath + "\\Profiles\\" + Path.GetFileName(files[i]), true);

                RefreshProfiles();
            }
        }

        private void tSBExportProfile_Click(object sender, EventArgs e)
        {
            if (lBProfiles.SelectedIndex >= 0)
            {
                Stream stream;
                Stream profile = new StreamReader(appdatapath + "\\Profiles\\" + lBProfiles.SelectedItem.ToString() + ".xml").BaseStream;                
                if (saveProfiles.ShowDialog() == DialogResult.OK)
                {
                    if ((stream = saveProfiles.OpenFile()) != null)
                    {
                        profile.CopyTo(stream);
                        profile.Close();
                        stream.Close();
                    }
                }
            }
        }

        private void ShowOptions(int devID, string profile)
        {
            Show();

            lBProfiles.Visible = false;

            WindowState = FormWindowState.Normal;
            toolStrip1.Enabled = false;
            tSOptions.Visible = true;
            toolStrip1.Visible = false;
            if (profile != "")
                tSTBProfile.Text = profile;
            else
                tSTBProfile.Text = "<" + Properties.Resources.TypeProfileName + ">";

            opt = new Options(this);
            opt.Icon = this.Icon;
            opt.TopLevel = false;
            opt.Dock = DockStyle.Fill;
            opt.FormBorderStyle = FormBorderStyle.None;

            tabProfiles.Controls.Add(opt);
            optPop = true;
            //opt.Dock = DockStyle.Fill;
            //lBProfiles.SendToBack();
            //toolStrip1.SendToBack();
            //tSOptions.SendToBack();
            opt.BringToFront();
            oldsize = Size;
            {
                if (Size.Height < (int)(90 * dpiy) + Options.mSize.Height)
                    Size = new Size(Size.Width, (int)(90 * dpiy) + Options.mSize.Height);

                if (Size.Width < (int)(20 * dpix) + Options.mSize.Width)
                    Size = new Size((int)(20 * dpix) + Options.mSize.Width, Size.Height);
            }

            opt.Reload(devID, profile);

            opt.inputtimer.Start();
            opt.Visible = true;

            tabMain.SelectedIndex = 1;
            opt.SetFlowAutoScroll();
        }

        public void OptionsClosed()
        {
            RefreshProfiles();

            if (!lbNoControllers.Visible)
                tabMain.SelectedIndex = 0;

            Size = oldsize;
            oldsize = new Size(0, 0);
            tSBKeepSize.Text = Properties.Resources.KeepThisSize;
            tSBKeepSize.Image = Properties.Resources.size;
            tSBKeepSize.Enabled = true;
            tSOptions.Visible = false;
            toolStrip1.Visible = true;
            toolStrip1.Enabled = true;
            lbLastMessage.ForeColor = SystemColors.GrayText;

            opt.Dock = DockStyle.None;
            tabProfiles.Controls.Remove(opt);
            optPop = false; opt = null;

            lBProfiles.Visible = true;
        }

        private void editButtons_Click(object sender, EventArgs e)
        {
            Button bn = (Button)sender;
            //int i = Int32.Parse(bn.Tag.ToString());
            int i = Convert.ToInt32(bn.Tag);
            string profileText = cbs[i].Text;
            if (profileText != "(" + Properties.Resources.NoProfileLoaded + ")")
                ShowOptions(i, profileText);
            else
                ShowOptions(i, "");
        }

        private void editMenu_Click(object sender, EventArgs e)
        {
            Show();
            WindowState = FormWindowState.Normal;
            ToolStripMenuItem em = (ToolStripMenuItem)sender;
            int i = Convert.ToInt32(em.Tag);
            if (em.Text == Properties.Resources.ContextNew.Replace("*number*", (i + 1).ToString()))
                ShowOptions(i, "");
            else
            {
                for (int t = 0, itemCount = em.DropDownItems.Count - 2; t < itemCount; t++)
                {
                    if (((ToolStripMenuItem)em.DropDownItems[t]).Checked)
                        ShowOptions(i, ((ToolStripMenuItem)em.DropDownItems[t]).Text);
                }
            }
        }

        private void lnkControllers_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("control", "joy.cpl");
        }

        private void hideDS4CheckBox_CheckedChanged(object sender, EventArgs e)
        {
            // Prevent the Game Controllers window from throwing an error when controllers are un/hidden
            Process[] rundll64 = Process.GetProcessesByName("rundll64");
            foreach (Process rundll64Instance in rundll64)
            {
                foreach (ProcessModule module in rundll64Instance.Modules)
                {
                    if (module.FileName.Contains("joy.cpl"))
                        module.Dispose();
                }
            }

            bool exclusiveMode = hideDS4CheckBox.Checked;
            UseExclusiveMode = exclusiveMode;

            hideDS4CheckBox.Enabled = false;
            Save();
            BtnStartStop_Clicked(false);
            finishHideDS4Check();
        }

        private async void finishHideDS4Check()
        {
            await TaskRunner.Factory.StartNew(() =>
            {
                while (changingService)
                {
                    Thread.Sleep(10);
                }
            });

            BtnStartStop_Clicked(false);

            await TaskRunner.Factory.StartNew(() =>
            {
                while (changingService)
                {
                    Thread.Sleep(10);
                }
            });

            hideDS4CheckBox.Enabled = true;
        }

        private void startMinimizedCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            StartMinimized = startMinimizedCheckBox.Checked;
            Save();
        }

        private void lvDebug_ItemActivate(object sender, EventArgs e)
        {
            MessageBox.Show(((ListView)sender).FocusedItem.SubItems[1].Text, "Log");
        }

        private void Profile_Changed(object sender, EventArgs e) //cbs[i] changed
        {
            ComboBox cb = (ComboBox)sender;
            int tdevice = Convert.ToInt32(cb.Tag);
            if (cb.Items[cb.Items.Count - 1].ToString() == "+" + Properties.Resources.PlusNewProfile)
            {
                if (cb.SelectedIndex < cb.Items.Count - 1)
                {
                    for (int i = 0, arlen = shortcuts[tdevice].DropDownItems.Count; i < arlen; i++)
                    {
                        if (!(shortcuts[tdevice].DropDownItems[i] is ToolStripSeparator))
                            ((ToolStripMenuItem)shortcuts[tdevice].DropDownItems[i]).Checked = false;
                    }

                    ((ToolStripMenuItem)shortcuts[tdevice].DropDownItems[cb.SelectedIndex]).Checked = true;
                    LogDebug(DateTime.Now, Properties.Resources.UsingProfile.Replace("*number*", (tdevice + 1).ToString()).Replace("*Profile name*", cb.Text), false);
                    shortcuts[tdevice].Text = Properties.Resources.ContextEdit.Replace("*number*", (tdevice + 1).ToString());
                    ProfilePath[tdevice] = cb.Items[cb.SelectedIndex].ToString();
                    Save();
                    LoadProfile(tdevice, true, Program.rootHub);
                    if (UseCustomLed[tdevice])
                        lights[tdevice].BackColor = CustomColor[tdevice].ToColorA;
                    else
                        lights[tdevice].BackColor = MainColor[tdevice].ToColorA;

                    if (linkedProfileCB[tdevice].Checked)
                    {
                        DS4Device device = Program.rootHub.DS4Controllers[tdevice];
                        if (device != null && device.isValidSerial())
                        {
                            changeLinkedProfile(device.getMacAddress(), ProfilePath[tdevice]);
                            SaveLinkedProfiles();
                        }
                    }
                    else
                    {
                        OlderProfilePath[tdevice] = ProfilePath[tdevice];
                    }
                }
                else if (cb.SelectedIndex == cb.Items.Count - 1 && cb.Items.Count > 1) //if +New Profile selected
                    ShowOptions(tdevice, "");

                if (cb.Text == "(" + Properties.Resources.NoProfileLoaded + ")")
                    ebns[tdevice].Text = Properties.Resources.New;
                else
                    ebns[tdevice].Text = Properties.Resources.EditProfile;
            }

            OnDeviceStatusChanged(this, tdevice); //to update profile name in notify icon
        }

        private void Profile_Changed_Menu(object sender, ToolStripItemClickedEventArgs e)
        {
            ToolStripMenuItem tS = (ToolStripMenuItem)sender;
            int tdevice = Convert.ToInt32(tS.Tag);
            if (!(e.ClickedItem is ToolStripSeparator))
            {
                if (e.ClickedItem != tS.DropDownItems[tS.DropDownItems.Count - 1]) //if +New Profile not selected 
                    cbs[tdevice].SelectedIndex = tS.DropDownItems.IndexOf(e.ClickedItem);
                else //if +New Profile selected
                    ShowOptions(tdevice, "");
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            contextclose = true;
            Close();
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Show();
            Focus();
            WindowState = FormWindowState.Normal;
        }

        private void startToolStripMenuItem_Click(object sender, EventArgs e)
        {
            BtnStartStop_Clicked();
        }

        private void notifyIcon1_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Middle)
            {
                contextclose = true;
                Close();
            }
        }

        private void notifyIcon1_BalloonTipClicked(object sender, EventArgs e)
        {
            Show();
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
            bool isChecked = StartWindowsCheckBox.Checked;
            if (isChecked && !File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.Startup) + "\\DS4Windows.lnk"))
            {
                appShortcutToStartup();
            }
            else if (!isChecked)
            {
                File.Delete(Environment.GetFolderPath(Environment.SpecialFolder.Startup) + "\\DS4Windows.lnk");
            }

            if (isChecked)
            {
                runStartupPanel.Visible = true;
            }
            else
            {
                runStartupPanel.Visible = false;
                runStartTaskRadio.Checked = false;
                runStartProgRadio.Checked = true;
            }

            changeStartupRoutine();
        }

        private void appShortcutToStartup()
        {
            Type t = Type.GetTypeFromCLSID(new Guid("72C24DD5-D70A-438B-8A42-98424B88AFB8")); // Windows Script Host Shell Object
            dynamic shell = Activator.CreateInstance(t);
            try
            {
                var lnk = shell.CreateShortcut(Environment.GetFolderPath(Environment.SpecialFolder.Startup) + "\\DS4Windows.lnk");
                try
                {
                    string app = Assembly.GetExecutingAssembly().Location;
                    lnk.TargetPath = Assembly.GetExecutingAssembly().Location;

                    if (runStartProgRadio.Checked)
                    {
                        lnk.Arguments = "-m";
                    }
                    else if (runStartTaskRadio.Checked)
                    {
                        lnk.Arguments = "-runtask";
                    }

                    //lnk.TargetPath = Assembly.GetExecutingAssembly().Location;
                    //lnk.Arguments = "-m";
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
            TabPage currentTab = tabMain.SelectedTab;
            lbLastMessage.Visible = currentTab != tabLog;
            if (currentTab == tabLog)
                chData.AutoResize(ColumnHeaderAutoResizeStyle.HeaderSize);

            if (currentTab == tabSettings)
            {
                lbLastMessage.ForeColor = SystemColors.GrayText;
                lbLastMessage.Text = Properties.Resources.HoverOverItems;
            }
            else if (lvDebug.Items.Count > 0)
                lbLastMessage.Text = lvDebug.Items[lvDebug.Items.Count - 1].SubItems[1].Text;
            else
                lbLastMessage.Text = "";
        }

        private void Items_MouseHover(object sender, EventArgs e)
        {
            string hoverText = Properties.Resources.HoverOverItems;
            string temp = "";
            if (hoverTextDict.TryGetValue((Control)sender, out temp))
            {
                hoverText = temp;
            }

            lbLastMessage.Text = hoverText;
            if (hoverText != Properties.Resources.HoverOverItems)
                lbLastMessage.ForeColor = Color.Black;
            else
                lbLastMessage.ForeColor = SystemColors.GrayText;
        }

        private void lBProfiles_MouseDown(object sender, MouseEventArgs e)
        {
            lBProfiles.SelectedIndex = lBProfiles.IndexFromPoint(e.X, e.Y);
            if (e.Button == MouseButtons.Right)
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

        private void tBProfile_TextChanged(object sender, EventArgs e)
        {
            if (tSTBProfile.Text != null && tSTBProfile.Text != "" &&
                !tSTBProfile.Text.Contains("\\") && !tSTBProfile.Text.Contains("/") &&
                !tSTBProfile.Text.Contains(":") && !tSTBProfile.Text.Contains("*") &&
                !tSTBProfile.Text.Contains("?") && !tSTBProfile.Text.Contains("\"") &&
                !tSTBProfile.Text.Contains("<") && !tSTBProfile.Text.Contains(">") &&
                !tSTBProfile.Text.Contains("|"))
                tSTBProfile.ForeColor = SystemColors.WindowText;
            else
                tSTBProfile.ForeColor = SystemColors.GrayText;
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
            if (optPop && opt.Visible)
                opt.Close();
        }

        private void tSBSaveProfile_Click(object sender, EventArgs e)
        {
            if (optPop && opt.Visible)
            {
                opt.saving = true;
                opt.Set();

                if (tSTBProfile.Text != null && tSTBProfile.Text != "" &&
                    !tSTBProfile.Text.Contains("\\") && !tSTBProfile.Text.Contains("/") &&
                    !tSTBProfile.Text.Contains(":") && !tSTBProfile.Text.Contains("*") &&
                    !tSTBProfile.Text.Contains("?") && !tSTBProfile.Text.Contains("\"") &&
                    !tSTBProfile.Text.Contains("<") && !tSTBProfile.Text.Contains(">") &&
                    !tSTBProfile.Text.Contains("|"))
                {
                    File.Delete(appdatapath + @"\Profiles\" + opt.filename + ".xml");
                    ProfilePath[opt.device] = tSTBProfile.Text;
                    SaveProfile(opt.device, tSTBProfile.Text);
                    cacheProfileCustomsFlags(opt.device);
                    Save();
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

        private void cBCustomSteam_CheckedChanged(object sender, EventArgs e)
        {
            UseCustomSteamFolder = cBCustomSteam.Checked;
            tBSteamFolder.Enabled = cBCustomSteam.Checked;
        }

        private void tBSteamFolder_TextChanged(object sender, EventArgs e)
        {
            CustomSteamFolder = tBSteamFolder.Text;
        }

        private void nUDUpdateTime_ValueChanged(object sender, EventArgs e)
        {
            int currentIndex = cBUpdateTime.SelectedIndex;
            if (currentIndex == 0)
                CheckWhen = (int)nUDUpdateTime.Value;
            else if (currentIndex == 1)
                CheckWhen = (int)nUDUpdateTime.Value * 24;

            if (nUDUpdateTime.Value < 1)
                cBUpdate.Checked = false;

            if (nUDUpdateTime.Value == 1)
            {
                int index = currentIndex;
                cBUpdateTime.BeginUpdate();
                cBUpdateTime.Items.Clear();
                cBUpdateTime.Items.Add(Properties.Resources.Hour);
                cBUpdateTime.Items.Add(Properties.Resources.Day);
                cBUpdateTime.SelectedIndex = index;
                cBUpdateTime.EndUpdate();
            }
            else if (cBUpdateTime.Items[0].ToString() == Properties.Resources.Hour)
            {
                int index = currentIndex;
                cBUpdateTime.BeginUpdate();
                cBUpdateTime.Items.Clear();
                cBUpdateTime.Items.Add(Properties.Resources.Hours);
                cBUpdateTime.Items.Add(Properties.Resources.Days);
                cBUpdateTime.SelectedIndex = index;
                cBUpdateTime.EndUpdate();
            }
        }

        private void cBUpdateTime_SelectedIndexChanged(object sender, EventArgs e)
        {
            int index = cBUpdateTime.SelectedIndex;
            if (index == 0)
                CheckWhen = (int)nUDUpdateTime.Value;
            else if (index == 1)
                CheckWhen = (int)nUDUpdateTime.Value * 24;
        }

        private void lLBUpdate_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            // Sorry other devs, gonna have to find your own server
            Uri url = new Uri("https://raw.githubusercontent.com/Ryochan7/DS4Windows/jay/DS4Windows/newest.txt");
            WebClient wct = new WebClient();
            wct.DownloadFileAsync(url, appdatapath + "\\version.txt");
            wct.DownloadFileCompleted += (sender2, e2) => TaskRunner.Run(() => wct_DownloadFileCompleted(sender2, e2));
        }

        private void cBDisconnectBT_CheckedChanged(object sender, EventArgs e)
        {
            DCBTatStop = cBDisconnectBT.Checked;
        }

        void wct_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            LastChecked = DateTime.Now;
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location);
            string version2 = fvi.FileVersion;
            string newversion2 = File.ReadAllText(appdatapath + "\\version.txt").Trim();
            if (!string.IsNullOrWhiteSpace(newversion2) && version2.Replace(',', '.').CompareTo(newversion2) != 0)
            {
                if ((DialogResult)this.Invoke(new Func<DialogResult>(() =>
                {
                    return MessageBox.Show(Properties.Resources.DownloadVersion.Replace("*number*", newversion2),
                    Properties.Resources.DS4Update, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                })) == DialogResult.Yes)
                {
                    if (!File.Exists(exepath + "\\DS4Updater.exe") || (File.Exists(exepath + "\\DS4Updater.exe")
                         && (FileVersionInfo.GetVersionInfo(exepath + "\\DS4Updater.exe").FileVersion.CompareTo(UPDATER_VERSION) != 0)))
                    {
                        Uri url2 = new Uri($"https://github.com/Ryochan7/DS4Updater/releases/download/v{UPDATER_VERSION}/{updaterExe}");
                        WebClient wc2 = new WebClient();
                        if (appdatapath == exepath)
                            wc2.DownloadFile(url2, exepath + "\\DS4Updater.exe");
                        else
                        {
                            this.BeginInvoke((System.Action)(() => MessageBox.Show(Properties.Resources.PleaseDownloadUpdater)));
                            Process.Start($"https://github.com/Ryochan7/DS4Updater/releases/download/v{UPDATER_VERSION}/{updaterExe}");
                        }
                    }

                    Process p = new Process();
                    p.StartInfo.FileName = exepath + "\\DS4Updater.exe";
                    p.StartInfo.Arguments = "-autolaunch";
                    if (AdminNeeded())
                        p.StartInfo.Verb = "runas";

                    try { p.Start(); Close(); }
                    catch { }
                }
                else
                    File.Delete(appdatapath + "\\version.txt");
            }
            else
            {
                File.Delete(appdatapath + "\\version.txt");
                this.BeginInvoke((System.Action)(() => MessageBox.Show(Properties.Resources.UpToDate, "DS4Windows Updater")));
            }
        }

        private void linkProfiles_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start(appdatapath + "\\Profiles");
        }

        private void linkUninstall_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (File.Exists(appdatapath + "\\Virtual Bus Driver\\ScpDriver.exe"))
            {
                try { Process.Start(appdatapath + "\\Virtual Bus Driver\\ScpDriver.exe"); }
                catch { Process.Start(appdatapath + "\\Virtual Bus Driver"); }
            }
        }

        private void cBoxNotifications_SelectedIndexChanged(object sender, EventArgs e)
        {
            Notifications = cBoxNotifications.SelectedIndex;
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

        bool tempBool = false;
        private void ScpForm_Closing(object sender, FormClosingEventArgs e)
        {
            if (optPop)
            {
                opt.Close();
                e.Cancel = true;
                return;
            }

            bool closeMini = tempBool = cBCloseMini.Checked;
            bool userClosing = e.CloseReason == CloseReason.UserClosing;
            DS4Device d = null;
            bool nocontrollers = tempBool = true;
            //in case user accidentally clicks on the close button whilst "Close Minimizes" checkbox is unchecked
            if (userClosing && !closeMini && !contextclose)
            {
                for (int i = 0, PadsLen = Pads.Length; tempBool && i < PadsLen; i++)
                {
                    d = Program.rootHub.DS4Controllers[i];
                    tempBool = (d != null) ? false : tempBool;
                }

                nocontrollers = tempBool;
                if (!nocontrollers)
                {
                    if (MessageBox.Show(Properties.Resources.CloseConfirm, Properties.Resources.Confirm,
                        MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                    {
                        e.Cancel = true;
                        return;
                    }
                    else
                    {
                        Util.UnregisterNotify(regHandle);
                    }
                }
                else
                {
                    Util.UnregisterNotify(regHandle);
                }
            }
            else if (userClosing && closeMini && !contextclose)
            {
                WindowState = FormWindowState.Minimized;
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

            Global.ControllerRemoved -= ControllerRemovedChange;

            if (!string.IsNullOrEmpty(appdatapath))
            {
                Save();
                blankControllerTab();
            }

            TaskRunner.Run(() => Program.rootHub.Stop()).Wait();
            // Make sure to stop event generation routines. Should fix odd crashes on shutdown
            Application.Exit();
        }

        private void cBSwipeProfiles_CheckedChanged(object sender, EventArgs e)
        {
            bool swipe = false;
            SwipeProfiles = swipe = cBSwipeProfiles.Checked;
            bool timerEnabled = hotkeysTimer.Enabled;
            if (swipe && !timerEnabled)
            {
                ChangeHotkeysStatus(true);
                //hotkeysTimer.Start();
            }
            else if (!swipe && timerEnabled)
            {
                ChangeHotkeysStatus(false);
                //hotkeysTimer.Stop();
            }
        }

        private void cBQuickCharge_CheckedChanged(object sender, EventArgs e)
        {
            QuickCharge = cBQuickCharge.Checked;
        }

        private void lbLastMessage_MouseHover(object sender, EventArgs e)
        {
            toolTip1.Show(lbLastMessage.Text, lbLastMessage, -3, -3);
        }

        private void pnlButton_MouseLeave(object sender, EventArgs e)
        {
            toolTip1.Hide(lbLastMessage);
        }

        private void cBCloseMini_CheckedChanged(object sender, EventArgs e)
        {
            CloseMini = cBCloseMini.Checked;
        }

        private void Pads_MouseHover(object sender, EventArgs e)
        {
            Label lb = (Label)sender;
            int i = Convert.ToInt32(lb.Tag);
            DS4Device d = Program.rootHub.DS4Controllers[i];
            if (d != null)
            {
                double latency = d.Latency;
                toolTip1.Hide(Pads[i]);
                toolTip1.Show(Properties.Resources.InputDelay.Replace("*number*", latency.ToString()), lb, lb.Size.Width, 0);
            }
        }

        private void Pads_MouseLeave(object sender, EventArgs e)
        {
            toolTip1.Hide((Label)sender);
        }

        Process bat;

        int currentCustomLed;
        private void EditCustomLed(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            currentCustomLed = Convert.ToInt32(btn.Tag);
            bool customLedChecked = UseCustomLed[currentCustomLed];
            useCustomColorToolStripMenuItem.Checked = customLedChecked;
            useProfileColorToolStripMenuItem.Checked = !customLedChecked;
            cMCustomLed.Show(btn, new Point(0, btn.Height));
        }

        private void useProfileColorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            UseCustomLed[currentCustomLed] = false;
            lights[currentCustomLed].BackColor = MainColor[currentCustomLed].ToColorA;
            Global.Save();
        }
    
        private void useCustomColorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            advColorDialog.Color = CustomColor[currentCustomLed].ToColor;
            AdvancedColorDialog.ColorUpdateHandler tempDel =
                new AdvancedColorDialog.ColorUpdateHandler(advColor_CustomColorUpdate);

            advColorDialog.OnUpdateColor += tempDel;
            if (advColorDialog.ShowDialog() == DialogResult.OK)
            {
                lights[currentCustomLed].BackColor = new DS4Color(advColorDialog.Color).ToColorA;
                CustomColor[currentCustomLed] = new DS4Color(advColorDialog.Color);
                UseCustomLed[currentCustomLed] = true;
                Global.Save();
            }

            advColorDialog.OnUpdateColor -= tempDel;
            DS4LightBar.forcedFlash[currentCustomLed] = 0;
            DS4LightBar.forcelight[currentCustomLed] = false;
        }

        private void advColor_CustomColorUpdate(Color color, EventArgs e)
        {
            if (currentCustomLed < 4)
            {
                DS4Color dcolor = new DS4Color { red = color.R, green = color.G, blue = color.B };
                DS4LightBar.forcedColor[currentCustomLed] = dcolor;
                DS4LightBar.forcedFlash[currentCustomLed] = 0;
                DS4LightBar.forcelight[currentCustomLed] = true;
            }
        }

        private void cBUseWhiteIcon_CheckedChanged(object sender, EventArgs e)
        {
            UseWhiteIcon = cBUseWhiteIcon.Checked;
            notifyIcon1.Icon = UseWhiteIcon ? Properties.Resources.DS4W___White : Properties.Resources.DS4W;
        }

        private void advColorDialog_OnUpdateColor(object sender, EventArgs e)
        {
            if (sender is Color)
            {
                Color color = (Color)sender;
                DS4Color dcolor = new DS4Color(color);
                Console.WriteLine(dcolor);
                DS4LightBar.forcedColor[currentCustomLed] = dcolor;
                DS4LightBar.forcedFlash[currentCustomLed] = 0;
                DS4LightBar.forcelight[currentCustomLed] = true;
            }
        }

        private void lBProfiles_SelectedIndexChanged(object sender, EventArgs e)
        {
            int index = lBProfiles.SelectedIndex;
            if (index >= 0)
            {
                tsBNewProfle.Enabled = true;
                tsBEditProfile.Enabled = true;
                tsBDeleteProfile.Enabled = true;
                tSBDupProfile.Enabled = true;
                tSBImportProfile.Enabled = true;
                tSBExportProfile.Enabled = true;
            }
            else
            {
                tsBNewProfle.Enabled = true;
                tsBEditProfile.Enabled = false;
                tsBDeleteProfile.Enabled = false;
                tSBDupProfile.Enabled = false;
                tSBImportProfile.Enabled = true;
                tSBExportProfile.Enabled = false;
            }
        }

        private void runStartProgRadio_Click(object sender, EventArgs e)
        {
            appShortcutToStartup();
            changeStartupRoutine();
        }

        private void runStartTaskRadio_Click(object sender, EventArgs e)
        {
            appShortcutToStartup();
            changeStartupRoutine();
        }

        private void changeStartupRoutine()
        {
            if (runStartTaskRadio.Checked)
            {
                WindowsIdentity identity = WindowsIdentity.GetCurrent();
                WindowsPrincipal principal = new WindowsPrincipal(identity);
                if (principal.IsInRole(WindowsBuiltInRole.Administrator))
                {
                    TaskService ts = new TaskService();
                    Task tasker = ts.FindTask("RunDS4Windows");
                    if (tasker != null)
                    {
                        ts.RootFolder.DeleteTask("RunDS4Windows");
                    }

                    TaskDefinition td = ts.NewTask();
                    td.Actions.Add(new ExecAction(@"%windir%\System32\cmd.exe",
                        "/c start \"RunDS4Windows\" \"" + Process.GetCurrentProcess().MainModule.FileName + "\" -m",
                        new FileInfo(Process.GetCurrentProcess().MainModule.FileName).DirectoryName));

                    td.Principal.RunLevel = TaskRunLevel.Highest;
                    ts.RootFolder.RegisterTaskDefinition("RunDS4Windows", td);
                }
            }
            else
            {
                WindowsIdentity identity = WindowsIdentity.GetCurrent();
                WindowsPrincipal principal = new WindowsPrincipal(identity);
                if (principal.IsInRole(WindowsBuiltInRole.Administrator))
                {
                    TaskService ts = new TaskService();
                    Task tasker = ts.FindTask("RunDS4Windows");
                    if (tasker != null)
                    {
                        ts.RootFolder.DeleteTask("RunDS4Windows");
                    }
                }
            }
        }

        private void linkCB_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox linkCb = (CheckBox)sender;
            int i = Convert.ToInt32(linkCb.Tag);
            bool check = linkCb.Checked;
            Global.linkedProfileCheck[i] = check;
            DS4Device device = Program.rootHub.DS4Controllers[i];
            if (device != null && device.isSynced())
            {
                if (check)
                {
                    if (device.isValidSerial())
                    {
                        changeLinkedProfile(device.getMacAddress(), ProfilePath[i]);
                    }
                }
                else
                {
                    removeLinkedProfile(device.getMacAddress());
                    ProfilePath[i] = OlderProfilePath[i];
                    int profileIndex = cbs[i].FindString(ProfilePath[i]);
                    if (profileIndex >= 0)
                    {
                        cbs[i].SelectedIndex = profileIndex;
                    }
                }

                SaveLinkedProfiles();
            }
        }

        private void exportLogTxtBtn_Click(object sender, EventArgs e)
        {
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.AddExtension = true;
            dialog.DefaultExt = ".txt";
            dialog.Filter = "Text Documents (*.txt)|*.txt";
            dialog.Title = "Select Export File";
            dialog.InitialDirectory = Global.appdatapath;
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                string outputFile = dialog.FileName;
                List < string > outputLines = new List<string>();
                ListViewItem item = null;
                for (int i = 0, len = lvDebug.Items.Count; i < len; i++)
                {
                    item = lvDebug.Items[i];
                    outputLines.Add(item.SubItems[0].Text + ": " + item.SubItems[1].Text);
                }

                try
                {
                    StreamWriter stream = new StreamWriter(outputFile);
                    string line = string.Empty;
                    for (int i = 0, len = outputLines.Count; i < len; i++)
                    {
                        line = outputLines[i];
                        stream.WriteLine(line);
                    }
                    stream.Close();
                }
                catch { }
            }
        }

        private void languagePackComboBox1_SelectedValueChanged(object sender, EventArgs e)
        {
            string newValue = ((DS4Forms.LanguagePackComboBox)sender).SelectedValue.ToString();
            if (newValue != UseLang)
            {
                UseLang = newValue;
                Save();
                MessageBox.Show(Properties.Resources.LanguagePackApplyRestartRequired, Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void OpenProgramFolderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process temp = new Process();
            temp.StartInfo.FileName = "explorer.exe";
            temp.StartInfo.Arguments = @"/select, " + Assembly.GetExecutingAssembly().Location;
            try { temp.Start(); }
            catch { }
        }

        private void DiscontoolStripMenuItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem item = (ToolStripMenuItem)sender;
            int i = Convert.ToInt32(item.Tag);
            DS4Device d = Program.rootHub.DS4Controllers[i];
            if (d != null)
            {
                if (d.ConnectionType == ConnectionType.BT && !d.Charging)
                {
                    d.DisconnectBT();
                }
                else if (d.ConnectionType == ConnectionType.SONYWA && !d.Charging)
                {
                    d.DisconnectDongle();
                }
            }
        }

        private void MintoTaskCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            MinToTaskbar = mintoTaskCheckBox.Checked;
            Save();
        }

        private void CBController_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = true;
        }

        private async void CkUdpServ_CheckedChanged(object sender, EventArgs e)
        {
            bool state = ckUdpServ.Checked;
            setUsingUDPServer(state);
            if (!state)
            {
                Program.rootHub.ChangeMotionEventStatus(state);
                await TaskRunner.Delay(100).ContinueWith((t) =>
                {
                    Program.rootHub.ChangeUDPStatus(state);
                });
            }
            else
            {
                Program.rootHub.ChangeUDPStatus(state);
                await TaskRunner.Delay(100).ContinueWith((t) =>
                {
                    Program.rootHub.ChangeMotionEventStatus(state);
                });
            }

            nUDUdpPortNum.Enabled = state;
        }

        private void NUDUdpPortNum_Leave(object sender, EventArgs e)
        {
            int curValue = (int)nUDUdpPortNum.Value;
            if (curValue != getUDPServerPortNum())
            {
                setUDPServerPort(curValue);
                nUDUdpPortNum.Enabled = false;
                WaitUDPPortChange();
            }
        }

        private async void WaitUDPPortChange()
        {
            await TaskRunner.Delay(100);
            if (isUsingUDPServer())
            {
                await TaskRunner.Run(() => Program.rootHub.UseUDPPort());
                nUDUdpPortNum.Enabled = true;
            }
        }

        private void cBFlashWhenLate_CheckedChanged(object sender, EventArgs e)
        {
            FlashWhenLate = cBFlashWhenLate.Checked;
            nUDLatency.Enabled = cBFlashWhenLate.Checked;
            lbMsLatency.Enabled = cBFlashWhenLate.Checked;
        }

        private void nUDLatency_ValueChanged(object sender, EventArgs e)
        {
            FlashWhenLateAt = (int)Math.Round(nUDLatency.Value);
        }
    }
}
