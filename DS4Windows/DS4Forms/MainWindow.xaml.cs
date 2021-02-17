using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using System.Windows.Interop;
using System.Diagnostics;
using System.IO;
using System.Management;
using NonFormTimer = System.Timers.Timer;
using System.Runtime.InteropServices;
using System.ComponentModel;
using HttpProgress;

using DS4WinWPF.DS4Forms.ViewModels;
using DS4Windows;
using DS4WinWPF.Translations;

namespace DS4WinWPF.DS4Forms
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    [System.Security.SuppressUnmanagedCodeSecurity]
    public partial class MainWindow : Window
    {
        private const int DEFAULT_PROFILE_EDITOR_WIDTH = 1000;
        private const int DEFAULT_PROFILE_EDITOR_HEIGHT = 650;

        private MainWindowsViewModel mainWinVM;
        private StatusLogMsg lastLogMsg = new StatusLogMsg();
        private ProfileList profileListHolder = new ProfileList();
        private LogViewModel logvm;
        private ControllerListViewModel conLvViewModel;
        private TrayIconViewModel trayIconVM;
        private SettingsViewModel settingsWrapVM;
        private IntPtr regHandle = new IntPtr();
        private bool showAppInTaskbar = false;
        private ManagementEventWatcher managementEvWatcher;
        private bool wasrunning = false;
        private AutoProfileHolder autoProfileHolder;
        private NonFormTimer hotkeysTimer;
        private NonFormTimer autoProfilesTimer;
        private AutoProfileChecker autoprofileChecker;
        private ProfileEditor editor;
        private bool preserveSize = true;
        private Size oldSize;
        private bool contextclose;

        public ProfileList ProfileListHolder { get => profileListHolder; }

        public MainWindow(ArgumentParser parser)
        {
            InitializeComponent();

            mainWinVM = new MainWindowsViewModel();
            DataContext = mainWinVM;

            App root = Application.Current as App;
            settingsWrapVM = new SettingsViewModel();
            settingsTab.DataContext = settingsWrapVM;
            logvm = new LogViewModel(App.rootHub);
            //logListView.ItemsSource = logvm.LogItems;
            logListView.DataContext = logvm;
            lastMsgLb.DataContext = lastLogMsg;

            profileListHolder.Refresh();
            profilesListBox.ItemsSource = profileListHolder.ProfileListCol;

            StartStopBtn.Content = App.rootHub.running ? Translations.Strings.StopText :
                Translations.Strings.StartText;

            conLvViewModel = new ControllerListViewModel(App.rootHub, profileListHolder);
            controllerLV.DataContext = conLvViewModel;
            controllerLV.ItemsSource = conLvViewModel.ControllerCol;
            ChangeControllerPanel();
            // Sort device by input slot number
            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(controllerLV.ItemsSource);
            view.SortDescriptions.Clear();
            view.SortDescriptions.Add(new SortDescription("DevIndex", ListSortDirection.Ascending));
            view.Refresh();

            trayIconVM = new TrayIconViewModel(App.rootHub, profileListHolder);
            notifyIcon.DataContext = trayIconVM;

            if (Global.StartMinimized || parser.Mini)
            {
                WindowState = WindowState.Minimized;
            }

            bool isElevated = Global.IsAdministrator();
            if (isElevated)
            {
                uacImg.Visibility = Visibility.Collapsed;
            }

            this.Width = Global.FormWidth;
            this.Height = Global.FormHeight;
            WindowStartupLocation = WindowStartupLocation.Manual;
            Left = Global.FormLocationX;
            Top = Global.FormLocationY;
            noContLb.Content = string.Format(Strings.NoControllersConnected,
                ControlService.CURRENT_DS4_CONTROLLER_LIMIT);

            autoProfileHolder = autoProfControl.AutoProfileHolder;
            autoProfControl.SetupDataContext(profileListHolder);

            autoprofileChecker = new AutoProfileChecker(autoProfileHolder);

            slotManControl.SetupDataContext(controlService: App.rootHub,
                App.rootHub.OutputslotMan);

            SetupEvents();

            Thread timerThread = new Thread(() =>
            {
                hotkeysTimer = new NonFormTimer();
                hotkeysTimer.Interval = 20;
                hotkeysTimer.AutoReset = false;

                autoProfilesTimer = new NonFormTimer();
                autoProfilesTimer.Interval = 1000;
                autoProfilesTimer.AutoReset = false;
            });
            timerThread.IsBackground = true;
            timerThread.Priority = ThreadPriority.Lowest;
            timerThread.Start();
            timerThread.Join();
        }

        public void LateChecks(ArgumentParser parser)
        {
            Task tempTask = Task.Run(() =>
            {
                CheckDrivers();
                if (!parser.Stop)
                {
                    Dispatcher.BeginInvoke((Action)(() =>
                    {
                        StartStopBtn.IsEnabled = false;
                    }));
                    Thread.Sleep(1000);
                    App.rootHub.Start();
                    //root.rootHubtest.Start();
                }

                //UpdateTheUpdater();
            });

            Util.LogAssistBackgroundTask(tempTask);

            tempTask = Task.Delay(100).ContinueWith((t) =>
            {
                int checkwhen = Global.CheckWhen;
                if (checkwhen > 0 && DateTime.Now >= Global.LastChecked + TimeSpan.FromHours(checkwhen))
                {
                    DownloadUpstreamVersionInfo();
                    Check_Version();

                    Global.LastChecked = DateTime.Now;
                }
            });
            Util.LogAssistBackgroundTask(tempTask);
        }

        private void DownloadUpstreamVersionInfo()
        {
            // Sorry other devs, gonna have to find your own server
            Uri url = new Uri("https://raw.githubusercontent.com/Ryochan7/DS4Windows/jay/DS4Windows/newest.txt");
            string filename = Global.appdatapath + "\\version.txt";
            bool success = false;
            using (var downloadStream = new FileStream(filename, FileMode.Create))
            {
                Task<System.Net.Http.HttpResponseMessage> temp = App.requestClient.GetAsync(url.ToString(), downloadStream);
                try
                {
                    temp.Wait();
                    if (temp.Result.IsSuccessStatusCode) success = true;
                }
                catch (AggregateException) { }
            }

            if (!success && File.Exists(filename))
            {
                File.Delete(filename);
            }
        }

        private string DownloadUpstreamUpdaterVersion()
        {
            string result = string.Empty;
            // Sorry other devs, gonna have to find your own server
            Uri url = new Uri("https://raw.githubusercontent.com/Ryochan7/DS4Updater/master/Updater2/newest.txt");
            string filename = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "DS4Updater_version.txt");
            bool readFile = false;
            using (var downloadStream = new FileStream(filename, FileMode.Create))
            {
                Task<System.Net.Http.HttpResponseMessage> temp = App.requestClient.GetAsync(url.ToString(), downloadStream);
                temp.Wait();

                if (temp.Result.IsSuccessStatusCode) readFile = true;
            }

            if (readFile)
            {
                result = File.ReadAllText(filename).Trim();
                File.Delete(filename);
            }

            return result;
        }

        private void Check_Version(bool showstatus = false)
        {
            string version = Global.exeversion;
            string newversion = string.Empty;
            string versionFilePath = Global.appdatapath + "\\version.txt";
            ulong lastVersionNum = Global.LastVersionCheckedNum;
            //ulong lastVersion = Global.CompileVersionNumberFromString("2.1.1");

            bool versionFileExists = File.Exists(versionFilePath);
            if (versionFileExists)
            {
                newversion = File.ReadAllText(versionFilePath).Trim();
                //newversion = "2.1.3";
            }

            ulong newversionNum = !string.IsNullOrEmpty(newversion) ?
                Global.CompileVersionNumberFromString(newversion) : 0;

            if (!string.IsNullOrWhiteSpace(newversion) && version.CompareTo(newversion) != 0 &&
                lastVersionNum < newversionNum)
            {
                MessageBoxResult result = MessageBoxResult.No;
                Dispatcher.Invoke(() =>
                {
                    UpdaterWindow updaterWin = new UpdaterWindow(newversion);
                    updaterWin.ShowDialog();
                    result = updaterWin.Result;
                });

                if (result == MessageBoxResult.Yes)
                {
                    bool launch = true;
                    launch = RunUpdaterCheck(launch);

                    if (launch)
                    {
                        using (Process p = new Process())
                        {
                            p.StartInfo.FileName = System.IO.Path.Combine(Global.exedirpath, "DS4Updater.exe");
                            bool isAdmin = Global.IsAdministrator();
                            List<string> argList = new List<string>();
                            argList.Add("-autolaunch");
                            if (!isAdmin)
                            {
                                argList.Add("-user");
                            }

                            // Specify current exe to have DS4Updater launch
                            argList.Add("--launchExe");
                            argList.Add(Global.exeFileName);

                            p.StartInfo.Arguments = string.Join(" ", argList);
                            if (Global.AdminNeeded())
                                p.StartInfo.Verb = "runas";

                            try { launch = p.Start(); }
                            catch (InvalidOperationException) { }
                        }
                    }

                    if (launch)
                    {
                        Dispatcher.BeginInvoke((Action)(() =>
                        {
                            contextclose = true;
                            Close();
                        }));
                    }
                    else
                    {
                        Dispatcher.Invoke(() =>
                        {
                            MessageBox.Show(Properties.Resources.PleaseDownloadUpdater);
                        });
                        //Process.Start($"https://github.com/Ryochan7/DS4Updater/releases/download/v{UPDATER_VERSION}/{updaterExe}");
                    }
                }
                else
                {
                    if (versionFileExists)
                        File.Delete(Global.appdatapath + "\\version.txt");
                }
            }
            else
            {
                if (versionFileExists)
                    File.Delete(Global.appdatapath + "\\version.txt");

                if (showstatus)
                {
                    Dispatcher.Invoke(() => MessageBox.Show(Properties.Resources.UpToDate, "DS4Windows Updater"));
                }
            }
        }

        private bool RunUpdaterCheck(bool launch)
        {
            string destPath = Global.exedirpath + "\\DS4Updater.exe";
            bool updaterExists = File.Exists(destPath);
            string version = DownloadUpstreamUpdaterVersion();
            if (!updaterExists ||
                (!string.IsNullOrEmpty(version) && FileVersionInfo.GetVersionInfo(destPath).FileVersion.CompareTo(version) != 0))
            {
                launch = false;
                Uri url2 = new Uri($"https://github.com/Ryochan7/DS4Updater/releases/download/v{version}/{mainWinVM.updaterExe}");
                string filename = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "DS4Updater.exe");
                using (var downloadStream = new FileStream(filename, FileMode.Create))
                {
                    Task<System.Net.Http.HttpResponseMessage> temp =
                        App.requestClient.GetAsync(url2.ToString(), downloadStream);
                    temp.Wait();
                    if (temp.Result.IsSuccessStatusCode) launch = true;
                }

                if (launch)
                {
                    if (Global.AdminNeeded())
                    {
                        int copyStatus = Util.ElevatedCopyUpdater(filename);
                        if (copyStatus != 0) launch = false;
                    }
                    else
                    {
                        if (updaterExists) File.Delete(destPath);
                        File.Move(filename, destPath);
                    }
                }
            }

            return launch;
        }

        private void TrayIconVM_RequestMinimize(object sender, EventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void TrayIconVM_ProfileSelected(TrayIconViewModel sender,
            ControllerHolder item, string profile)
        {
            int idx = item.Index;
            CompositeDeviceModel devitem = conLvViewModel.ControllerDict[idx];
            if (devitem != null)
            {
                devitem.ChangeSelectedProfile(profile);
            }
        }

        private void ShowNotification(object sender, DS4Windows.DebugEventArgs e)
        {
            Dispatcher.BeginInvoke((Action)(() =>
            {

                if (!IsActive && (Global.Notifications == 2 ||
                    (Global.Notifications == 1 && e.Warning)))
                {
                    notifyIcon.ShowBalloonTip(TrayIconViewModel.ballonTitle,
                    e.Data, !e.Warning ? Hardcodet.Wpf.TaskbarNotification.BalloonIcon.Info :
                    Hardcodet.Wpf.TaskbarNotification.BalloonIcon.Warning);
                }
            }));
        }

        private void SetupEvents()
        {
            App root = Application.Current as App;
            App.rootHub.ServiceStarted += ControlServiceStarted;
            App.rootHub.RunningChanged += ControlServiceChanged;
            App.rootHub.PreServiceStop += PrepareForServiceStop;
            //root.rootHubtest.RunningChanged += ControlServiceChanged;
            conLvViewModel.ControllerCol.CollectionChanged += ControllerCol_CollectionChanged;
            AppLogger.TrayIconLog += ShowNotification;
            AppLogger.GuiLog += UpdateLastStatusMessage;
            logvm.LogItems.CollectionChanged += LogItems_CollectionChanged;
            App.rootHub.Debug += UpdateLastStatusMessage;
            trayIconVM.RequestShutdown += TrayIconVM_RequestShutdown;
            trayIconVM.ProfileSelected += TrayIconVM_ProfileSelected;
            trayIconVM.RequestMinimize += TrayIconVM_RequestMinimize;
            trayIconVM.RequestOpen += TrayIconVM_RequestOpen;
            trayIconVM.RequestServiceChange += TrayIconVM_RequestServiceChange;
            settingsWrapVM.IconChoiceIndexChanged += SettingsWrapVM_IconChoiceIndexChanged;
            settingsWrapVM.AppChoiceIndexChanged += SettingsWrapVM_AppChoiceIndexChanged;

            autoProfControl.AutoDebugChanged += AutoProfControl_AutoDebugChanged;
            autoprofileChecker.RequestServiceChange += AutoprofileChecker_RequestServiceChange;
            autoProfileHolder.AutoProfileColl.CollectionChanged += AutoProfileColl_CollectionChanged;
            //autoProfControl.AutoProfVM.AutoProfileSystemChange += AutoProfVM_AutoProfileSystemChange;
            mainWinVM.FullTabsEnabledChanged += MainWinVM_FullTabsEnabledChanged;

            bool wmiConnected = false;
            WqlEventQuery q = new WqlEventQuery();
            ManagementScope scope = new ManagementScope("root\\CIMV2");
            q.EventClassName = "Win32_PowerManagementEvent";

            try
            {
                scope.Connect();
            }
            catch (COMException) { }

            if (scope.IsConnected)
            {
                wmiConnected = true;
                managementEvWatcher = new ManagementEventWatcher(scope, q);
                managementEvWatcher.EventArrived += PowerEventArrive;
                try
                {
                    managementEvWatcher.Start();
                }
                catch (ManagementException) { wmiConnected = false; }
            }

            if (!wmiConnected)
            {
                AppLogger.LogToGui(@"Could not connect to Windows Management Instrumentation service.
Suspend support not enabled.", true);
            }
        }

        private void SettingsWrapVM_AppChoiceIndexChanged(object sender, EventArgs e)
        {
            AppThemeChoice choice = Global.UseCurrentTheme;
            App current = App.Current as App;
            current.ChangeTheme(choice);
            trayIconVM.PopulateContextMenu();
        }

        private void SettingsWrapVM_IconChoiceIndexChanged(object sender, EventArgs e)
        {
            trayIconVM.IconSource = Global.iconChoiceResources[Global.UseIconChoice];
        }

        private void MainWinVM_FullTabsEnabledChanged(object sender, EventArgs e)
        {
            settingsWrapVM.ViewEnabled = mainWinVM.FullTabsEnabled;
        }

        private void TrayIconVM_RequestServiceChange(object sender, EventArgs e)
        {
            ChangeService();
        }

        private void LogItems_CollectionChanged(object sender,
            System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                Dispatcher.BeginInvoke((Action)(() =>
                {
                    int count = logListView.Items.Count;
                    if (count > 0)
                    {
                        logListView.ScrollIntoView(logvm.LogItems[count - 1]);
                    }
                }));
            }
        }

        private void ControlServiceStarted(object sender, EventArgs e)
        {
            if (Global.SwipeProfiles)
            {
                ChangeHotkeysStatus(true);
            }

            CheckAutoProfileStatus();
        }

        private void AutoprofileChecker_RequestServiceChange(AutoProfileChecker sender, bool state)
        {
            Dispatcher.BeginInvoke((Action)(() =>
            {
                ChangeService();
            }));
        }

        private void AutoProfVM_AutoProfileSystemChange(AutoProfilesViewModel sender, bool state)
        {
            if (state)
            {
                ChangeAutoProfilesStatus(true);
                autoProfileHolder.AutoProfileColl.CollectionChanged += AutoProfileColl_CollectionChanged;
            }
            else
            {
                ChangeAutoProfilesStatus(false);
                autoProfileHolder.AutoProfileColl.CollectionChanged -= AutoProfileColl_CollectionChanged;
            }
        }

        private void AutoProfileColl_CollectionChanged(object sender,
            System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            CheckAutoProfileStatus();
        }

        private void AutoProfControl_AutoDebugChanged(object sender, EventArgs e)
        {
            autoprofileChecker.AutoProfileDebugLogLevel = autoProfControl.AutoDebug == true ? 1 : 0;
        }

        private void PowerEventArrive(object sender, EventArrivedEventArgs e)
        {
            short evType = Convert.ToInt16(e.NewEvent.GetPropertyValue("EventType"));
            switch (evType)
            {
                // Wakeup from Suspend
                case 7:
                    DS4LightBar.shuttingdown = false;
                    App.rootHub.suspending = false;

                    if (wasrunning)
                    {
                        wasrunning = false;
                        Thread.Sleep(16000);
                        Dispatcher.BeginInvoke((Action)(() =>
                        {
                            StartStopBtn.IsEnabled = false;
                        }));

                        App.rootHub.Start();
                    }

                    break;
                // Entering Suspend
                case 4:
                    DS4LightBar.shuttingdown = true;
                    Program.rootHub.suspending = true;

                    if (App.rootHub.running)
                    {
                        Dispatcher.BeginInvoke((Action)(() =>
                        {
                            StartStopBtn.IsEnabled = false;
                        }));

                        App.rootHub.Stop();
                        wasrunning = true;
                    }

                    break;

                default: break;
            }
        }

        private void UpdateTheUpdater()
        {
            if (File.Exists(Global.exedirpath + "\\Update Files\\DS4Windows\\DS4Updater.exe"))
            {
                Process[] processes = Process.GetProcessesByName("DS4Updater");
                while (processes.Length > 0)
                {
                    Thread.Sleep(500);
                    processes = Process.GetProcessesByName("DS4Updater");
                }

                if (!Global.AdminNeeded())
                {
                    File.Delete(Global.exedirpath + "\\DS4Updater.exe");
                    File.Move(Global.exedirpath + "\\Update Files\\DS4Windows\\DS4Updater.exe",
                        Global.exedirpath + "\\DS4Updater.exe");
                    Directory.Delete(Global.exedirpath + "\\Update Files", true);
                }
                else
                {
                    Util.ElevatedCopyUpdater(Global.exedirpath + "\\Update Files\\DS4Windows\\DS4Updater.exe", true);
                }
            }
        }

        private void ChangeHotkeysStatus(bool state)
        {
            if (state)
            {
                hotkeysTimer.Elapsed += HotkeysTimer_Elapsed;
                hotkeysTimer.Start();
            }
            else
            {
                hotkeysTimer.Stop();
                hotkeysTimer.Elapsed -= HotkeysTimer_Elapsed;
            }
        }

        private void HotkeysTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            hotkeysTimer.Stop();

            if (Global.SwipeProfiles)
            {
                foreach (CompositeDeviceModel item in conLvViewModel.ControllerCol)
                //for (int i = 0; i < 4; i++)
                {
                    string slide = App.rootHub.TouchpadSlide(item.DevIndex);
                    if (slide == "left")
                    {
                        //int ind = i;
                        Dispatcher.BeginInvoke((Action)(() =>
                        {
                            if (item.SelectedIndex <= 0)
                            {
                                item.SelectedIndex = item.ProfileListCol.Count - 1;
                            }
                            else
                            {
                                item.SelectedIndex--;
                            }
                        }));
                    }
                    else if (slide == "right")
                    {
                        //int ind = i;
                        Dispatcher.BeginInvoke((Action)(() =>
                        {
                            if (item.SelectedIndex == (item.ProfileListCol.Count - 1))
                            {
                                item.SelectedIndex = 0;
                            }
                            else
                            {
                                item.SelectedIndex++;
                            }
                        }));
                    }

                    if (slide.Contains("t"))
                    {
                        //int ind = i;
                        Dispatcher.BeginInvoke((Action)(() =>
                        {
                            string temp = string.Format(Properties.Resources.UsingProfile, (item.DevIndex + 1).ToString(), item.SelectedProfile, $"{item.Device.Battery}");
                            ShowHotkeyNotification(temp);
                        }));
                    }
                }
            }

            hotkeysTimer.Start();
        }

        private void ShowHotkeyNotification(string message)
        {
            if (!IsActive && (Global.Notifications == 2))
            {
                notifyIcon.ShowBalloonTip(TrayIconViewModel.ballonTitle,
                message, Hardcodet.Wpf.TaskbarNotification.BalloonIcon.Info);
            }
        }

        private void PrepareForServiceStop(object sender, EventArgs e)
        {
            Dispatcher.BeginInvoke((Action)(() =>
            {
                trayIconVM.ClearContextMenu();
            }));

            ChangeHotkeysStatus(false);
        }

        private void TrayIconVM_RequestOpen(object sender, EventArgs e)
        {
            if (!showAppInTaskbar)
            {
                Show();
            }

            WindowState = WindowState.Normal;
        }

        private void TrayIconVM_RequestShutdown(object sender, EventArgs e)
        {
            contextclose = true;
            this.Close();
        }

        private void UpdateLastStatusMessage(object sender, DS4Windows.DebugEventArgs e)
        {
            lastLogMsg.Message = e.Data;
            lastLogMsg.Warning = e.Warning;
        }

        private void ChangeControllerPanel()
        {
            if (conLvViewModel.ControllerCol.Count == 0)
            {
                controllerLV.Visibility = Visibility.Hidden;
                noContLb.Visibility = Visibility.Visible;
            }
            else
            {
                controllerLV.Visibility = Visibility.Visible;
                noContLb.Visibility = Visibility.Hidden;
            }
        }

        private void ChangeAutoProfilesStatus(bool state)
        {
            if (state)
            {
                autoProfilesTimer.Elapsed += AutoProfilesTimer_Elapsed;
                autoProfilesTimer.Start();
                autoprofileChecker.Running = true;
            }
            else
            {
                autoProfilesTimer.Stop();
                autoProfilesTimer.Elapsed -= AutoProfilesTimer_Elapsed;
                autoprofileChecker.Running = false;
            }
        }

        private void CheckAutoProfileStatus()
        {
            int pathCount = autoProfileHolder.AutoProfileColl.Count;
            bool timerEnabled = autoprofileChecker.Running;
            if (pathCount > 0 && !timerEnabled)
            {
                ChangeAutoProfilesStatus(true);
            }
            else if (pathCount == 0 && timerEnabled)
            {
                ChangeAutoProfilesStatus(false);
            }
        }

        private void AutoProfilesTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            autoProfilesTimer.Stop();
            //Console.WriteLine("Event triggered");
            autoprofileChecker.Process();

            if (autoprofileChecker.Running)
            {
                autoProfilesTimer.Start();
            }
        }

        private void ControllerCol_CollectionChanged(object sender,
            System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            Dispatcher.BeginInvoke((Action)(() =>
            {
                ChangeControllerPanel();
                System.Collections.IList newitems = e.NewItems;
                if (newitems != null)
                {
                    foreach (CompositeDeviceModel item in newitems)
                    {
                        item.LightContext = new ContextMenu();
                        item.AddLightContextItems();
                        item.Device.SyncChange += DS4Device_SyncChange;
                        item.RequestColorPicker += Item_RequestColorPicker;
                        //item.LightContext.Items.Add(new MenuItem() { Header = "Use Profile Color", IsChecked = !item.UseCustomColor });
                        //item.LightContext.Items.Add(new MenuItem() { Header = "Use Custom Color", IsChecked = item.UseCustomColor });
                    }
                }

                if (App.rootHub.running)
                    trayIconVM.PopulateContextMenu();
            }));
        }

        private void Item_RequestColorPicker(CompositeDeviceModel sender)
        {
            ColorPickerWindow dialog = new ColorPickerWindow();
            dialog.Owner = this;
            dialog.colorPicker.SelectedColor = sender.CustomLightColor;
            dialog.ColorChanged += (sender2, color) =>
            {
                sender.UpdateCustomLightColor(color);
            };
            dialog.ShowDialog();
        }

        private void DS4Device_SyncChange(object sender, EventArgs e)
        {
            Dispatcher.BeginInvoke((Action)(() =>
            {
                trayIconVM.PopulateContextMenu();
            }));
        }

        private void ControlServiceChanged(object sender, EventArgs e)
        {
            //Tester service = sender as Tester;
            ControlService service = sender as ControlService;
            Dispatcher.BeginInvoke((Action)(() =>
            {
                if (service.running)
                {
                    StartStopBtn.Content = Translations.Strings.StopText;
                }
                else
                {
                    StartStopBtn.Content = Translations.Strings.StartText;
                }

                StartStopBtn.IsEnabled = true;
                slotManControl.IsEnabled = service.running;
            }));
        }

        private void AboutBtn_Click(object sender, RoutedEventArgs e)
        {
            About aboutWin = new About();
            aboutWin.Owner = this;
            aboutWin.ShowDialog();
        }

        private void StartStopBtn_Click(object sender, RoutedEventArgs e)
        {
            ChangeService();
        }

        private async void ChangeService()
        {
            StartStopBtn.IsEnabled = false;
            App root = Application.Current as App;
            //Tester service = root.rootHubtest;
            ControlService service = App.rootHub;
            await Task.Run(() =>
            {
                if (service.running)
                    service.Stop();
                else
                    service.Start();
            });
        }

        private void LogListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            int idx = logListView.SelectedIndex;
            if (idx > -1)
            {
                LogItem temp = logvm.LogItems[idx];
                LogMessageDisplay msgBox = new LogMessageDisplay(temp.Message);
                msgBox.Owner = this;
                msgBox.ShowDialog();
                //MessageBox.Show(temp.Message, "Log");
            }
        }

        private void ClearLogBtn_Click(object sender, RoutedEventArgs e)
        {
            logvm.LogItems.Clear();
        }

        private void MainTabCon_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (mainTabCon.SelectedIndex == 4)
            {
                lastMsgLb.Visibility = Visibility.Hidden;
            }
            else
            {
                lastMsgLb.Visibility = Visibility.Visible;
            }
        }

        private void ProfilesListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            newProfListBtn.IsEnabled = true;
            editProfBtn.IsEnabled = true;
            deleteProfBtn.IsEnabled = true;
            dupProfBtn.IsEnabled = true;
            importProfBtn.IsEnabled = true;
            exportProfBtn.IsEnabled = true;
        }

        private void RunAtStartCk_Click(object sender, RoutedEventArgs e)
        {
            settingsWrapVM.ShowRunStartPanel = runAtStartCk.IsChecked == true ? Visibility.Visible :
                Visibility.Collapsed;
        }

        private void ContStatusImg_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            Image img = sender as Image;
            int tag = Convert.ToInt32(img.Tag);
            CompositeDeviceModel item = conLvViewModel.CurrentItem;
            //CompositeDeviceModel item = conLvViewModel.ControllerDict[tag];
            if (item != null)
            {
                item.RequestDisconnect();
            }
        }

        private void ExportLogBtn_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.AddExtension = true;
            dialog.DefaultExt = ".txt";
            dialog.Filter = "Text Documents (*.txt)|*.txt";
            dialog.Title = "Select Export File";
            // TODO: Expose config dir
            dialog.InitialDirectory = Global.appdatapath;
            if (dialog.ShowDialog() == true)
            {
                LogWriter logWriter = new LogWriter(dialog.FileName, logvm.LogItems.ToList());
                logWriter.Process();
            }
        }

        private void IdColumnTxtB_ToolTipOpening(object sender, ToolTipEventArgs e)
        {
            TextBlock statusBk = sender as TextBlock;
            int idx = Convert.ToInt32(statusBk.Tag);
            if (idx >= 0)
            {
                CompositeDeviceModel item = conLvViewModel.ControllerDict[idx];
                item.RequestUpdatedTooltipID();
            }
        }

        /// <summary>
        /// Clear and re-populate tray context menu
        /// </summary>
        private void NotifyIcon_TrayRightMouseUp(object sender, RoutedEventArgs e)
        {
            notifyIcon.ContextMenu = trayIconVM.ContextMenu;
        }

        /// <summary>
        /// Change profile based on selection
        /// </summary>
        private void SelectProfCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox box = sender as ComboBox;
            int idx = Convert.ToInt32(box.Tag);
            if (idx > -1 && conLvViewModel.ControllerDict.ContainsKey(idx))
            {
                CompositeDeviceModel item = conLvViewModel.ControllerDict[idx];
                if (item.SelectedIndex > -1)
                {
                    item.ChangeSelectedProfile();
                    trayIconVM.PopulateContextMenu();
                }
            }
        }

        private void CustomColorPick_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {

        }

        private void LightColorBtn_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            int idx = Convert.ToInt32(button.Tag);
            CompositeDeviceModel item = conLvViewModel.ControllerDict[idx];
            //(button.ContextMenu.Items[0] as MenuItem).IsChecked = conLvViewModel.ControllerCol[idx].UseCustomColor;
            //(button.ContextMenu.Items[1] as MenuItem).IsChecked = !conLvViewModel.ControllerCol[idx].UseCustomColor;
            button.ContextMenu = item.LightContext;
            button.ContextMenu.IsOpen = true;
        }

        private void MainDS4Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (editor != null)
            {
                editor.Close();
                e.Cancel = true;
                return;
            }
            else if (contextclose)
            {
                return;
            }
            else if (Global.CloseMini)
            {
                WindowState = WindowState.Minimized;
                e.Cancel = true;
                return;
            }

            // If this method was called directly without sender object then skip the confirmation dialogbox
            if (sender != null && conLvViewModel.ControllerCol.Count > 0)
            {
                MessageBoxResult result = MessageBox.Show(Properties.Resources.CloseConfirm, Properties.Resources.Confirm,
                        MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.No)
                {
                    e.Cancel = true;
                    return;
                }
            }
        }

        private void MainDS4Window_Closed(object sender, EventArgs e)
        {
            hotkeysTimer.Stop();
            autoProfilesTimer.Stop();
            //autoProfileHolder.Save();
            Util.UnregisterNotify(regHandle);
            Application.Current.Shutdown();
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            HwndSource source = PresentationSource.FromVisual(this) as HwndSource;
            HookWindowMessages(source);
            source.AddHook(WndProc);
        }

        private bool inHotPlug = false;
        private int hotplugCounter = 0;
        private object hotplugCounterLock = new object();
        private const int DBT_DEVNODES_CHANGED = 0x0007;
        private const int DBT_DEVICEARRIVAL = 0x8000;
        private const int DBT_DEVICEREMOVECOMPLETE = 0x8004;
        public const int WM_COPYDATA = 0x004A;
        private const int HOTPLUG_CHECK_DELAY = 2000;

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam,
            IntPtr lParam, ref bool handled)
        {
            // Handle messages...
            switch (msg)
            {
                case Util.WM_DEVICECHANGE:
                {
                    if (Global.runHotPlug)
                    {
                        Int32 Type = wParam.ToInt32();
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
                                Task.Run(() => { InnerHotplug2(); });
                            }
                        }
                    }
                    break;
                }
                case WM_COPYDATA:
                {
                    // Received InterProcessCommunication (IPC) message. DS4Win command is embedded as a string value in lpData buffer
                    try
                    {
                        App.COPYDATASTRUCT cds = (App.COPYDATASTRUCT)Marshal.PtrToStructure(lParam, typeof(App.COPYDATASTRUCT));
                        if (cds.cbData >= 4 && cds.cbData <= 256)
                        {
                            int tdevice = -1;

                            byte[] buffer = new byte[cds.cbData];
                            Marshal.Copy(cds.lpData, buffer, 0, cds.cbData);
                            string[] strData = Encoding.ASCII.GetString(buffer).Split('.');

                            if (strData.Length >= 1)
                            {
                                strData[0] = strData[0].ToLower();

                                if (strData[0] == "start")
                                { 
                                    if(!Program.rootHub.running) 
                                        ChangeService();
                                }
                                else if (strData[0] == "stop")
                                {    
                                    if (Program.rootHub.running)
                                        ChangeService();
                                }
                                else if (strData[0] == "cycle")
                                {
                                    ChangeService();
                                }
                                else if (strData[0] == "shutdown")
                                {
                                    // Force disconnect all gamepads before closing the app to avoid "Are you sure you want to close the app" messagebox
                                    if (Program.rootHub.running)
                                        ChangeService();

                                    // Call closing method and let it to close editor wnd (if it is open) before proceeding to the actual "app closed" handler
                                    MainDS4Window_Closing(null, new System.ComponentModel.CancelEventArgs());
                                    MainDS4Window_Closed(this, new System.EventArgs());
                                }
                                else if ((strData[0] == "loadprofile" || strData[0] == "loadtempprofile") && strData.Length >= 3)
                                {
                                    // Command syntax: LoadProfile.device#.profileName (fex LoadProfile.1.GameSnake or LoadTempProfile.1.WebBrowserSet)
                                    if (int.TryParse(strData[1], out tdevice)) tdevice--;

                                    if (tdevice >= 0 && tdevice < ControlService.MAX_DS4_CONTROLLER_COUNT &&
                                            File.Exists(Global.appdatapath + "\\Profiles\\" + strData[2] + ".xml"))
                                    {
                                        if (strData[0] == "loadprofile")
                                        {
                                            int idx = profileListHolder.ProfileListCol.Select((item, index) => new { item, index }).
                                                    Where(x => x.item.Name == strData[2]).Select(x => x.index).DefaultIfEmpty(-1).First();

                                            if (idx >= 0 && tdevice < conLvViewModel.ControllerCol.Count)
                                            {
                                                conLvViewModel.ControllerCol[tdevice].ChangeSelectedProfile(strData[2]);
                                            }
                                            else
                                            {
                                                // Preset profile name for later loading
                                                Global.ProfilePath[tdevice] = strData[2];
                                                //Global.LoadProfile(tdevice, true, Program.rootHub);
                                            }
                                        }
                                        else
                                        {
                                            Global.LoadTempProfile(tdevice, strData[2], true, Program.rootHub);
                                        }

                                        DS4Device device = conLvViewModel.ControllerCol[tdevice].Device;
                                        if (device != null)
                                        {
                                            string prolog = string.Format(Properties.Resources.UsingProfile, (tdevice + 1).ToString(), strData[2], $"{device.Battery}");
                                            Program.rootHub.LogDebug(prolog);
                                        }
                                    }
                                }
                                else if (strData[0] == "outputslot" && strData.Length >= 3)
                                {
                                    // Command syntax: 
                                    //    OutputSlot.slot#.Unplug
                                    //    OutputSlot.slot#.PlugDS4
                                    //    OutputSlot.slot#.PlugX360
                                    if (int.TryParse(strData[1], out tdevice))
                                        tdevice--;

                                    if (tdevice >= 0 && tdevice < ControlService.MAX_DS4_CONTROLLER_COUNT)
                                    {
                                        strData[2] = strData[2].ToLower();
                                        DS4Control.OutSlotDevice slotDevice = Program.rootHub.OutputslotMan.OutputSlots[tdevice];
                                        if (strData[2] == "unplug")
                                            Program.rootHub.DetachUnboundOutDev(slotDevice);
                                        else if (strData[2] == "plugds4")
                                            Program.rootHub.AttachUnboundOutDev(slotDevice, OutContType.DS4);
                                        else if (strData[2] == "plugx360")
                                            Program.rootHub.AttachUnboundOutDev(slotDevice, OutContType.X360);
                                    }
                                }
                                else if (strData[0] == "query" && strData.Length >= 3)
                                {
                                    string propName;
                                    string propValue = String.Empty;

                                    // Command syntax: QueryProfile.device#.Name (fex "Query.1.ProfileName" would print out the name of the active profile in controller 1)
                                    if (int.TryParse(strData[1], out tdevice))
                                        tdevice--;

                                    if (tdevice >= 0 && tdevice < ControlService.MAX_DS4_CONTROLLER_COUNT)
                                    {
                                        // Name of the property to query from a profile or DS4Windows app engine
                                        propName = strData[2].ToLower();

                                            if (propName == "profilename")
                                            {
                                                if (Global.useTempProfile[tdevice])
                                                    propValue = Global.tempprofilename[tdevice];
                                                else
                                                    propValue = Global.ProfilePath[tdevice];
                                            }
                                            else if (propName == "outconttype")
                                                propValue = Global.OutContType[tdevice].ToString();
                                            else if (propName == "activeoutdevtype")
                                                propValue = Global.activeOutDevType[tdevice].ToString();
                                            else if (propName == "usedinputonly")
                                                propValue = Global.useDInputOnly[tdevice].ToString();

                                            else if (propName == "devicevidpid" && App.rootHub.DS4Controllers[tdevice] != null)
                                                propValue = $"VID={App.rootHub.DS4Controllers[tdevice].HidDevice.Attributes.VendorHexId}, PID={App.rootHub.DS4Controllers[tdevice].HidDevice.Attributes.ProductHexId}";
                                            else if (propName == "devicepath" && App.rootHub.DS4Controllers[tdevice] != null)
                                                propValue = App.rootHub.DS4Controllers[tdevice].HidDevice.DevicePath;
                                            else if (propName == "macaddress" && App.rootHub.DS4Controllers[tdevice] != null)
                                                propValue = App.rootHub.DS4Controllers[tdevice].MacAddress;
                                            else if (propName == "displayname" && App.rootHub.DS4Controllers[tdevice] != null)
                                                propValue = App.rootHub.DS4Controllers[tdevice].DisplayName;
                                            else if (propName == "conntype" && App.rootHub.DS4Controllers[tdevice] != null)
                                                propValue = App.rootHub.DS4Controllers[tdevice].ConnectionType.ToString();
                                            else if (propName == "exclusivestatus" && App.rootHub.DS4Controllers[tdevice] != null)
                                                propValue = App.rootHub.DS4Controllers[tdevice].CurrentExclusiveStatus.ToString();
                                            else if (propName == "battery" && App.rootHub.DS4Controllers[tdevice] != null)
                                                propValue = App.rootHub.DS4Controllers[tdevice].Battery.ToString();
                                            else if (propName == "charging" && App.rootHub.DS4Controllers[tdevice] != null)
                                                propValue = App.rootHub.DS4Controllers[tdevice].Charging.ToString();
                                            else if (propName == "outputslottype")
                                                propValue = App.rootHub.OutputslotMan.OutputSlots[tdevice].CurrentType.ToString();
                                            else if (propName == "outputslotpermanenttype")
                                                propValue = App.rootHub.OutputslotMan.OutputSlots[tdevice].PermanentType.ToString();
                                            else if (propName == "outputslotattachedstatus")
                                                propValue = App.rootHub.OutputslotMan.OutputSlots[tdevice].CurrentAttachedStatus.ToString();
                                            else if (propName == "outputslotinputbound")
                                                propValue = App.rootHub.OutputslotMan.OutputSlots[tdevice].CurrentInputBound.ToString();

                                            else if (propName == "apprunning")
                                                propValue = App.rootHub.running.ToString(); // Controller idx value is ignored, but it still needs to be in 1..4 range in a cmdline call
                                    }

                                    // Write out the property value to MMF result data file and notify a client process that the data is available
                                    ((Application.Current) as App).WriteIPCResultDataMMF(propValue);
                                }
                            }
                        }
                    }
                    catch
                    {
                        // Eat all exceptions in WM_COPYDATA because exceptions here are not fatal for DS4Windows background app
                    }
                    break;
                }
                default: break;
            }

            return IntPtr.Zero;
        }

        private void InnerHotplug2()
        {
            inHotPlug = true;

            bool loopHotplug = false;
            lock (hotplugCounterLock)
            {
                loopHotplug = hotplugCounter > 0;
            }

            Program.rootHub.UpdateHidGuardAttributes();
            while (loopHotplug == true)
            {
                Thread.Sleep(HOTPLUG_CHECK_DELAY);
                Program.rootHub.HotPlug();
                lock (hotplugCounterLock)
                {
                    hotplugCounter--;
                    loopHotplug = hotplugCounter > 0;
                }
            }

            inHotPlug = false;
        }

        private void HookWindowMessages(HwndSource source)
        {
            Guid hidGuid = new Guid();
            NativeMethods.HidD_GetHidGuid(ref hidGuid);
            bool result = Util.RegisterNotify(source.Handle, hidGuid, ref regHandle);
            if (!result)
            {
                App.Current.Shutdown();
            }
        }

        private void ProfEditSBtn_Click(object sender, RoutedEventArgs e)
        {
            Control temp = sender as Control;
            int idx = Convert.ToInt32(temp.Tag);
            controllerLV.SelectedIndex = idx;
            CompositeDeviceModel item = conLvViewModel.CurrentItem;

            if (item != null)
            {
                ProfileEntity entity = profileListHolder.ProfileListCol[item.SelectedIndex];
                ShowProfileEditor(idx, entity);
                mainTabCon.SelectedIndex = 1;
            }
        }

        private void NewProfBtn_Click(object sender, RoutedEventArgs e)
        {
            Control temp = sender as Control;
            int idx = Convert.ToInt32(temp.Tag);
            controllerLV.SelectedIndex = idx;
            ShowProfileEditor(idx, null);
            mainTabCon.SelectedIndex = 1;
            //controllerLV.Focus();
        }
        // Ex Mode Re-Enable
        private async void HideDS4ContCk_Click(object sender, RoutedEventArgs e)
        {
            if (DS4Windows.Global.UseExclusiveMode == true) { MessageBox.Show("This feature is depreciated and no longer supported. Exclusive mode usage is provided mearly as a legacy feature. Do NOT ask for help for this feature, you will not recieve any.", "Feature no longer supported"); }
            StartStopBtn.IsEnabled = false;
            //bool checkStatus = hideDS4ContCk.IsChecked == true;
            hideDS4ContCk.IsEnabled = false;
            await Task.Run(() =>
            {
                App.rootHub.Stop();
                App.rootHub.Start();
            });

            hideDS4ContCk.IsEnabled = true;
            StartStopBtn.IsEnabled = true;
        }
        private async void UseUdpServerCk_Click(object sender, RoutedEventArgs e)
        {
            bool status = useUdpServerCk.IsChecked == true;
            if (!status)
            {
                App.rootHub.ChangeMotionEventStatus(status);
                await Task.Delay(100).ContinueWith((t) =>
                {
                    App.rootHub.ChangeUDPStatus(status);
                });
            }
            else
            {
                Program.rootHub.ChangeUDPStatus(status);
                await Task.Delay(100).ContinueWith((t) =>
                {
                    App.rootHub.ChangeMotionEventStatus(status);
                });
            }
        }

        private void ProfFolderBtn_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(Global.appdatapath + "\\Profiles");
        }

        private void ControlPanelBtn_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("control", "joy.cpl");
        }

        private async void DriverSetupBtn_Click(object sender, RoutedEventArgs e)
        {
            StartStopBtn.IsEnabled = false;
            await Task.Run(() =>
            {
                if (App.rootHub.running)
                    App.rootHub.Stop();
            });

            StartStopBtn.IsEnabled = true;
            Process p = new Process();
            p.StartInfo.FileName = Global.exelocation;
            p.StartInfo.Arguments = "-driverinstall";
            p.StartInfo.Verb = "runas";
            try { p.Start(); }
            catch { }
        }

        private void CheckUpdatesBtn_Click(object sender, RoutedEventArgs e)
        {
            Task.Run(() =>
            {
                DownloadUpstreamVersionInfo();
                Check_Version(true);
            });
        }

        private void CheckDrivers()
        {
            bool deriverinstalled = Global.IsViGEmBusInstalled();
            if (!deriverinstalled)
            {
                Process p = new Process();
                p.StartInfo.FileName = $"{Global.exelocation}";
                p.StartInfo.Arguments = "-driverinstall";
                p.StartInfo.Verb = "runas";
                try { p.Start(); }
                catch { }
            }
        }

        private void ImportProfBtn_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.AddExtension = true;
            dialog.DefaultExt = ".xml";
            dialog.Filter = "DS4Windows Profile (*.xml)|*.xml";
            dialog.Title = "Select Profile to Import File";
            if (Global.appdatapath != Global.exedirpath)
                dialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\DS4Windows" + @"\Profiles\";
            else
                dialog.InitialDirectory = Global.exedirpath + @"\Profiles\";

            if (dialog.ShowDialog() == true)
            {
                string[] files = dialog.FileNames;
                for (int i = 0, arlen = files.Length; i < arlen; i++)
                {
                    string profilename = System.IO.Path.GetFileName(files[i]);
                    string basename = System.IO.Path.GetFileNameWithoutExtension(files[i]);
                    File.Copy(dialog.FileNames[i], Global.appdatapath + "\\Profiles\\" + profilename, true);
                    profileListHolder.AddProfileSort(basename);
                }
            }
        }

        private void ExportProfBtn_Click(object sender, RoutedEventArgs e)
        {
            if (profilesListBox.SelectedIndex >= 0)
            {
                SaveFileDialog dialog = new SaveFileDialog();
                dialog.AddExtension = true;
                dialog.DefaultExt = ".xml";
                dialog.Filter = "DS4Windows Profile (*.xml)|*.xml";
                dialog.Title = "Select Profile to Export File";
                Stream stream;
                int idx = profilesListBox.SelectedIndex;
                Stream profile = new StreamReader(Global.appdatapath + "\\Profiles\\" + profileListHolder.ProfileListCol[idx].Name + ".xml").BaseStream;
                if (dialog.ShowDialog() == true)
                {
                    if ((stream = dialog.OpenFile()) != null)
                    {
                        profile.CopyTo(stream);
                        profile.Close();
                        stream.Close();
                    }
                }
            }
        }

        private void DupProfBtn_Click(object sender, RoutedEventArgs e)
        {
            string filename = "";
            if (profilesListBox.SelectedIndex >= 0)
            {
                int idx = profilesListBox.SelectedIndex;
                filename = profileListHolder.ProfileListCol[idx].Name;
                dupBox.OldFilename = filename;
                dupBoxBar.Visibility = Visibility.Visible;
                dupBox.Save -= DupBox_Save;
                dupBox.Cancel -= DupBox_Cancel;
                dupBox.Save += DupBox_Save;
                dupBox.Cancel += DupBox_Cancel;
            }
        }

        private void DupBox_Cancel(object sender, EventArgs e)
        {
            dupBoxBar.Visibility = Visibility.Collapsed;
        }

        private void DupBox_Save(DupBox sender, string profilename)
        {
            profileListHolder.AddProfileSort(profilename);
            dupBoxBar.Visibility = Visibility.Collapsed;
        }

        private void DeleteProfBtn_Click(object sender, RoutedEventArgs e)
        {
            if (profilesListBox.SelectedIndex >= 0)
            {
                int idx = profilesListBox.SelectedIndex;
                ProfileEntity entity = profileListHolder.ProfileListCol[idx];
                string filename = entity.Name;
                if (MessageBox.Show(Properties.Resources.ProfileCannotRestore.Replace("*Profile name*", "\"" + filename + "\""),
                    Properties.Resources.DeleteProfile,
                    MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    entity.DeleteFile();
                    profileListHolder.ProfileListCol.RemoveAt(idx);
                }
            }
        }

        private void SelectProfCombo_KeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = true;
        }

        private void MainDS4Window_StateChanged(object _sender, EventArgs _e)
        {
            CheckMinStatus();
        }

        public void CheckMinStatus()
        {
            bool minToTask = Global.MinToTaskbar;
            if (WindowState == WindowState.Minimized && !minToTask)
            {
                Hide();
                showAppInTaskbar = false;
            }
            else if (WindowState == WindowState.Normal && !minToTask)
            {
                Show();
                showAppInTaskbar = true;
            }
        }

        private void MainDS4Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (WindowState != WindowState.Minimized && preserveSize)
            {
                Global.FormWidth = Convert.ToInt32(Width);
                Global.FormHeight = Convert.ToInt32(Height);
            }
        }

        private void MainDS4Window_LocationChanged(object sender, EventArgs e)
        {
            int left = Convert.ToInt32(Left), top = Convert.ToInt32(Top);
            if (left >= 0 && top >= 0)
            {
                Global.FormLocationX = left;
                Global.FormLocationY = top;
            }
        }

        private void NotifyIcon_TrayMiddleMouseDown(object sender, RoutedEventArgs e)
        {
            contextclose = true;
            Close();
        }

        private void SwipeTouchCk_Click(object sender, RoutedEventArgs e)
        {
            bool status = swipeTouchCk.IsChecked == true;
            ChangeHotkeysStatus(status);
        }

        private void EditProfBtn_Click(object sender, RoutedEventArgs e)
        {
            if (profilesListBox.SelectedIndex >= 0)
            {
                ProfileEntity entity = profileListHolder.ProfileListCol[profilesListBox.SelectedIndex];
                ShowProfileEditor(Global.TEST_PROFILE_INDEX, entity);
            }
        }

        private void ProfileEditor_Closed(object sender, EventArgs e)
        {
            profDockPanel.Children.Remove(editor);
            profOptsToolbar.Visibility = Visibility.Visible;
            profilesListBox.Visibility = Visibility.Visible;
            preserveSize = true;
            if (!editor.Keepsize)
            {
                this.Width = oldSize.Width;
                this.Height = oldSize.Height;
            }
            else
            {
                oldSize = new Size(Width, Height);
            }

            editor = null;
            mainTabCon.SelectedIndex = 0;
            mainWinVM.FullTabsEnabled = true;
            //Task.Run(() => GC.Collect(0, GCCollectionMode.Forced, false));
        }

        private void NewProfListBtn_Click(object sender, RoutedEventArgs e)
        {
            ShowProfileEditor(Global.TEST_PROFILE_INDEX, null);
        }

        private void ShowProfileEditor(int device, ProfileEntity entity = null)
        {
            if (editor == null)
            {
                profOptsToolbar.Visibility = Visibility.Collapsed;
                profilesListBox.Visibility = Visibility.Collapsed;
                mainWinVM.FullTabsEnabled = false;

                preserveSize = false;
                oldSize.Width = Width;
                oldSize.Height = Height;
                if (this.Width < DEFAULT_PROFILE_EDITOR_WIDTH)
                {
                    this.Width = DEFAULT_PROFILE_EDITOR_WIDTH;
                }

                if (this.Height < DEFAULT_PROFILE_EDITOR_HEIGHT)
                {
                    this.Height = DEFAULT_PROFILE_EDITOR_HEIGHT;
                }

                editor = new ProfileEditor(device);
                editor.CreatedProfile += Editor_CreatedProfile;
                editor.Closed += ProfileEditor_Closed;
                profDockPanel.Children.Add(editor);
                editor.Reload(device, entity);
            }
            
        }

        private void Editor_CreatedProfile(ProfileEditor sender, string profile)
        {
            profileListHolder.AddProfileSort(profile);
            int devnum = sender.DeviceNum;
            if (devnum >= 0 && devnum+1 <= conLvViewModel.ControllerCol.Count)
            {
                conLvViewModel.ControllerCol[devnum].ChangeSelectedProfile(profile);
            }
        }

        private void NotifyIcon_TrayMouseDoubleClick(object sender, RoutedEventArgs e)
        {
            if (!showAppInTaskbar)
            {
                Show();
            }

            WindowState = WindowState.Normal;
        }

        private void ProfilesListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (profilesListBox.SelectedIndex >= 0)
            {
                ProfileEntity entity = profileListHolder.ProfileListCol[profilesListBox.SelectedIndex];
                ShowProfileEditor(Global.TEST_PROFILE_INDEX, entity);
            }
        }

        private void Html5GameBtn_Click(object sender, RoutedEventArgs e)
        {
            Util.StartProcessHelper("https://gamepad-tester.com/");
        }

        private void HidNinjaBtn_Click(object sender, RoutedEventArgs e)
        {
            string path = System.IO.Path.Combine(Global.exedirpath, "Tools",
                "HidNinja", "HidNinja.exe");

            if (File.Exists(path))
            {
                try
                {
                    using (Process proc = Process.Start(path)) { }
                }
                catch { }
            }
        }

        private void FakeExeNameExplainBtn_Click(object sender, RoutedEventArgs e)
        {
            string message = Translations.Strings.CustomExeNameInfo;
            MessageBox.Show(message, "Custom Exe Name Info", MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private void XinputCheckerBtn_Click(object sender, RoutedEventArgs e)
        {
            string path = System.IO.Path.Combine(Global.exedirpath, "Tools",
                "XInputChecker", "XInputChecker.exe");

            if (File.Exists(path))
            {
                try
                {
                    using (Process proc = Process.Start(path)) { }
                }
                catch { }
            }
        }

        private void ChecklogViewBtn_Click(object sender, RoutedEventArgs e)
        {
            ChangelogWindow changelogWin = new ChangelogWindow();
            changelogWin.ShowDialog();
        }

        private void DeviceOptionSettingsBtn_Click(object sender, RoutedEventArgs e)
        {
            ControllerRegisterOptionsWindow optsWindow =
                new ControllerRegisterOptionsWindow(Program.rootHub.DeviceOptions, Program.rootHub);

            optsWindow.Owner = this;
            optsWindow.Show();
        }
    }

    public class ImageLocationPaths
    {
        public string NewProfile { get => $"/DS4Windows;component/Resources/{App.Current.FindResource("NewProfileImg")}"; }
        public event EventHandler NewProfileChanged;

        public string EditProfile { get => $"/DS4Windows;component/Resources/{App.Current.FindResource("EditImg")}"; }
        public event EventHandler EditProfileChanged;

        public string DeleteProfile { get => $"/DS4Windows;component/Resources/{App.Current.FindResource("DeleteImg")}"; }
        public event EventHandler DeleteProfileChanged;

        public string DuplicateProfile { get => $"/DS4Windows;component/Resources/{App.Current.FindResource("CopyImg")}"; }
        public event EventHandler DuplicateProfileChanged;

        public string ExportProfile { get => $"/DS4Windows;component/Resources/{App.Current.FindResource("ExportImg")}"; }
        public event EventHandler ExportProfileChanged;

        public string ImportProfile { get => $"/DS4Windows;component/Resources/{App.Current.FindResource("ImportImg")}"; }
        public event EventHandler ImportProfileChanged;

        public ImageLocationPaths()
        {
            App current = App.Current as App;
            if (current != null)
            {
                current.ThemeChanged += Current_ThemeChanged;
            }
        }

        private void Current_ThemeChanged(object sender, EventArgs e)
        {
            NewProfileChanged?.Invoke(this, EventArgs.Empty);
            EditProfileChanged?.Invoke(this, EventArgs.Empty);
            DeleteProfileChanged?.Invoke(this, EventArgs.Empty);
            DuplicateProfileChanged?.Invoke(this, EventArgs.Empty);
            ExportProfileChanged?.Invoke(this, EventArgs.Empty);
            ImportProfileChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
