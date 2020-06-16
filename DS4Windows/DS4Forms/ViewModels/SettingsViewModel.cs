using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Media;
using System.Windows.Interop;
using System.Windows;
using System.Windows.Media.Imaging;

namespace DS4WinWPF.DS4Forms.ViewModels
{
    public class SettingsViewModel
    {
        public bool HideDS4Controller { get => DS4Windows.Global.UseExclusiveMode;
            set => DS4Windows.Global.UseExclusiveMode = value; }

        public bool SwipeTouchSwitchProfile { get => DS4Windows.Global.SwipeProfiles;
            set => DS4Windows.Global.SwipeProfiles = value; }

        private bool runAtStartup;
        public bool RunAtStartup
        {
            get => runAtStartup;
            set
            {
                runAtStartup = value;
                RunAtStartupChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler RunAtStartupChanged;

        private bool runStartProg;
        public bool RunStartProg
        {
            get => runStartProg;
            set
            {
                runStartProg = value;
                RunStartProgChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler RunStartProgChanged;

        private bool runStartTask;
        public bool RunStartTask
        {
            get => runStartTask;
            set
            {
                runStartTask = value;
                RunStartTaskChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler RunStartTaskChanged;

        private bool canWriteTask;
        public bool CanWriteTask { get => canWriteTask; }

        public ImageSource uacSource;
        public ImageSource UACSource { get => uacSource; }

        private Visibility showRunStartPanel = Visibility.Collapsed;
        public Visibility ShowRunStartPanel {
            get => showRunStartPanel;
            set
            {
                if (showRunStartPanel == value) return;
                showRunStartPanel = value;
                ShowRunStartPanelChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public event EventHandler ShowRunStartPanelChanged;

        public int ShowNotificationsIndex { get => DS4Windows.Global.Notifications; set => DS4Windows.Global.Notifications = value; }
        public bool DisconnectBTStop { get => DS4Windows.Global.DCBTatStop; set => DS4Windows.Global.DCBTatStop = value; }
        public bool FlashHighLatency { get => DS4Windows.Global.FlashWhenLate; set => DS4Windows.Global.FlashWhenLate = value; }
        public int FlashHighLatencyAt { get => DS4Windows.Global.FlashWhenLateAt; set => DS4Windows.Global.FlashWhenLateAt = value; }
        public bool StartMinimize { get => DS4Windows.Global.StartMinimized; set => DS4Windows.Global.StartMinimized = value; }
        public bool MinimizeToTaskbar { get => DS4Windows.Global.MinToTaskbar; set => DS4Windows.Global.MinToTaskbar = value; }
        public bool CloseMinimizes { get => DS4Windows.Global.CloseMini; set => DS4Windows.Global.CloseMini = value; }
        public bool QuickCharge { get => DS4Windows.Global.QuickCharge; set => DS4Windows.Global.QuickCharge = value; }
        public bool WhiteDS4Icon
        {
            get => DS4Windows.Global.UseWhiteIcon;
            set
            {
                if (DS4Windows.Global.UseWhiteIcon == value) return;
                DS4Windows.Global.UseWhiteIcon = value;
                WhiteDS4IconChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler WhiteDS4IconChanged;

        public bool CheckForUpdates
        {
            get => DS4Windows.Global.CheckWhen > 0;
            set
            {
                DS4Windows.Global.CheckWhen = value ? 24 : 0;
                CheckForNoUpdatesWhen();
            }
        }
        public event EventHandler CheckForUpdatesChanged;

        public int CheckEvery
        {
            get
            {
                int temp = DS4Windows.Global.CheckWhen;
                if (temp > 23)
                {
                    temp = temp / 24;
                }
                return temp;
            }
            set
            {
                int temp;
                if (checkEveryUnitIdx == 0 && value < 24)
                {
                    temp = DS4Windows.Global.CheckWhen;
                    if (temp != value)
                    {
                        DS4Windows.Global.CheckWhen = value;
                        CheckForNoUpdatesWhen();
                    }
                }
                else if (checkEveryUnitIdx == 1)
                {
                    temp = DS4Windows.Global.CheckWhen / 24;
                    if (temp != value)
                    {
                        DS4Windows.Global.CheckWhen = value * 24;
                        CheckForNoUpdatesWhen();
                    }
                }
            }
        }
        public event EventHandler CheckEveryChanged;

        private int checkEveryUnitIdx = 1;
        public int CheckEveryUnit
        {
            get
            {
                return checkEveryUnitIdx;
            }
            set
            {
                if (checkEveryUnitIdx == value) return;
                checkEveryUnitIdx = value;
                CheckEveryUnitChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler CheckEveryUnitChanged;
        public bool UseUDPServer
        {
            get => DS4Windows.Global.isUsingUDPServer();
            set
            {
                if (DS4Windows.Global.isUsingUDPServer() == value) return;
                DS4Windows.Global.setUsingUDPServer(value);
                UseUDPServerChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler UseUDPServerChanged;

        public string UdpIpAddress { get => DS4Windows.Global.getUDPServerListenAddress();
            set => DS4Windows.Global.setUDPServerListenAddress(value); }
        public int UdpPort { get => DS4Windows.Global.getUDPServerPortNum(); set => DS4Windows.Global.setUDPServerPort(value); }
        public bool UseCustomSteamFolder
        {
            get => DS4Windows.Global.UseCustomSteamFolder;
            set
            {
                DS4Windows.Global.UseCustomSteamFolder = value;
                UseCustomSteamFolderChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler UseCustomSteamFolderChanged;

        public string CustomSteamFolder
        {
            get => DS4Windows.Global.CustomSteamFolder;
            set
            {
                string temp = DS4Windows.Global.CustomSteamFolder;
                if (temp == value) return;
                if (Directory.Exists(value) || value == string.Empty)
                {
                    DS4Windows.Global.CustomSteamFolder = value;
                }
            }
        }

        private bool viewEnabled = true;
        public bool ViewEnabled
        {
            get => viewEnabled;
            set
            {
                viewEnabled = value;
                ViewEnabledChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler ViewEnabledChanged;


        public SettingsViewModel()
        {
            checkEveryUnitIdx = 1;

            int checklapse = DS4Windows.Global.CheckWhen;
            if (checklapse < 24 && checklapse > 0)
            {
                checkEveryUnitIdx = 0;
            }

            CheckStartupOptions();

            Icon img = SystemIcons.Shield;
            Bitmap bitmap = img.ToBitmap();
            IntPtr hBitmap = bitmap.GetHbitmap();

            ImageSource wpfBitmap =
                 Imaging.CreateBitmapSourceFromHBitmap(
                      hBitmap, IntPtr.Zero, Int32Rect.Empty,
                      BitmapSizeOptions.FromEmptyOptions());
            uacSource = wpfBitmap;

            runAtStartup = StartupMethods.RunAtStartup();
            runStartProg = StartupMethods.HasStartProgEntry();
            runStartTask = StartupMethods.HasTaskEntry();
            canWriteTask = DS4Windows.Global.IsAdministrator();

            if (!runAtStartup)
            {
                runStartProg = true;
            }
            else if (runStartProg && runStartTask)
            {
                runStartProg = false;
                if (StartupMethods.CanWriteStartEntry())
                {
                    StartupMethods.DeleteStartProgEntry();
                }
            }

            if (runAtStartup && runStartProg)
            {
                bool locChange = StartupMethods.CheckStartupExeLocation();
                if (locChange)
                {
                    if (StartupMethods.CanWriteStartEntry())
                    {
                        StartupMethods.DeleteStartProgEntry();
                        StartupMethods.WriteStartProgEntry();
                    }
                    else
                    {
                        runAtStartup = false;
                        showRunStartPanel = Visibility.Collapsed;
                    }
                }
            }
            else if (runAtStartup && runStartTask)
            {
                if (canWriteTask)
                {
                    StartupMethods.DeleteOldTaskEntry();
                    StartupMethods.WriteTaskEntry();
                }
            }

            if (runAtStartup)
            {
                showRunStartPanel = Visibility.Visible;
            }

            RunAtStartupChanged += SettingsViewModel_RunAtStartupChanged;
            RunStartProgChanged += SettingsViewModel_RunStartProgChanged;
            RunStartTaskChanged += SettingsViewModel_RunStartTaskChanged;

            //CheckForUpdatesChanged += SettingsViewModel_CheckForUpdatesChanged;
        }

        private void SettingsViewModel_RunStartTaskChanged(object sender, EventArgs e)
        {
            if (runStartTask)
            {
                StartupMethods.WriteTaskEntry();
            }
            else
            {
                StartupMethods.DeleteTaskEntry();
            }
        }

        private void SettingsViewModel_RunStartProgChanged(object sender, EventArgs e)
        {
            if (runStartProg)
            {
                StartupMethods.WriteStartProgEntry();
            }
            else
            {
                StartupMethods.DeleteStartProgEntry();
            }
        }

        private void SettingsViewModel_RunAtStartupChanged(object sender, EventArgs e)
        {
            if (runAtStartup)
            {
                RunStartProg = true;
                RunStartTask = false;
            }
            else
            {
                StartupMethods.DeleteStartProgEntry();
                StartupMethods.DeleteTaskEntry();
            }
        }

        private void SettingsViewModel_CheckForUpdatesChanged(object sender, EventArgs e)
        {
            if (!CheckForUpdates)
            {
                CheckEveryChanged?.Invoke(this, EventArgs.Empty);
                CheckEveryUnitChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        private void CheckStartupOptions()
        {
            bool lnkExists = File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.Startup) + "\\DS4Windows.lnk");
            if (lnkExists)
            {
                runAtStartup = true;
            }
            else
            {
                runAtStartup = false;
            }
        }

        private void CheckForNoUpdatesWhen()
        {
            if (DS4Windows.Global.CheckWhen == 0)
            {
                checkEveryUnitIdx = 1;
            }

            CheckForUpdatesChanged?.Invoke(this, EventArgs.Empty);
            CheckEveryChanged?.Invoke(this, EventArgs.Empty);
            CheckEveryUnitChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
