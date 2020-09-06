using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using DS4Windows;

namespace DS4WinWPF.DS4Forms.ViewModels
{
    public class TrayIconViewModel
    {
        private string tooltipText = "DS4Windows";
        private string iconSource;
        public const string ballonTitle = "DS4Windows";
        public static string trayTitle = $"DS4Windows v{Global.exeversion}";
        private ContextMenu contextMenu;
        private MenuItem changeServiceItem;
        private MenuItem openItem;
        private MenuItem minimizeItem;
        private MenuItem openProgramItem;
        private MenuItem closeItem;


        public string TooltipText { get => tooltipText;
            set
            {
                string temp = value;
                if (value.Length > 63) temp = value.Substring(0, 63);
                if (tooltipText == temp) return;
                tooltipText = temp;
                TooltipTextChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler TooltipTextChanged;

        public string IconSource { get => iconSource;
            set
            {
                if (iconSource == value) return;
                iconSource = value;
                IconSourceChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public ContextMenu ContextMenu { get => contextMenu; }

        public event EventHandler IconSourceChanged;
        public event EventHandler RequestShutdown;
        public event EventHandler RequestOpen;
        public event EventHandler RequestMinimize;
        public event EventHandler RequestServiceChange;

        private ReaderWriterLockSlim _colLocker = new ReaderWriterLockSlim();
        private List<ControllerHolder> controllerList = new List<ControllerHolder>();
        private ProfileList profileListHolder;
        private ControlService controlService;

        public delegate void ProfileSelectedHandler(TrayIconViewModel sender,
            ControllerHolder item, string profile);
        public event ProfileSelectedHandler ProfileSelected;

        //public TrayIconViewModel(Tester tester)
        public TrayIconViewModel(ControlService service, ProfileList profileListHolder)
        {
            this.profileListHolder = profileListHolder;
            this.controlService = service;
            contextMenu = new ContextMenu();
            iconSource = Global.iconChoiceResources[Global.UseIconChoice];
            changeServiceItem = new MenuItem() { Header = "Start" };
            changeServiceItem.Click += ChangeControlServiceItem_Click;
            changeServiceItem.IsEnabled = false;

            openItem = new MenuItem() { Header = "Open",
                FontWeight = FontWeights.Bold };
            openItem.Click += OpenMenuItem_Click;
            minimizeItem = new MenuItem() { Header = "Minimize" };
            minimizeItem.Click += MinimizeMenuItem_Click;
            openProgramItem = new MenuItem() { Header = "Open Program Folder" };
            openProgramItem.Click += OpenProgramFolderItem_Click;
            closeItem = new MenuItem() { Header = "Exit (Middle Mouse)" }; ;
            closeItem.Click += ExitMenuItem_Click;

            PopulateControllerList();
            PopulateToolText();
            PopulateContextMenu();
            SetupEvents();
            profileListHolder.ProfileListCol.CollectionChanged += ProfileListCol_CollectionChanged;

            service.ServiceStarted += BuildControllerList;
            service.ServiceStarted += HookEvents;
            service.ServiceStarted += StartPopulateText;
            service.PreServiceStop += ClearToolText;
            service.PreServiceStop += UnhookEvents;
            service.PreServiceStop += ClearControllerList;
            service.RunningChanged += Service_RunningChanged;
            service.HotplugController += Service_HotplugController;
            /*tester.StartControllers += HookBatteryUpdate;
            tester.StartControllers += StartPopulateText;
            tester.PreRemoveControllers += ClearToolText;
            tester.HotplugControllers += HookBatteryUpdate;
            tester.HotplugControllers += StartPopulateText;
            */
        }

        private void Service_RunningChanged(object sender, EventArgs e)
        {
            string temp = controlService.running ? "Stop" : "Start";
            App.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                changeServiceItem.Header = temp;
                changeServiceItem.IsEnabled = true;
            }));
        }

        private void ClearControllerList(object sender, EventArgs e)
        {
            _colLocker.EnterWriteLock();
            controllerList.Clear();
            _colLocker.ExitWriteLock();
        }

        private void UnhookEvents(object sender, EventArgs e)
        {
            _colLocker.EnterReadLock();
            foreach (ControllerHolder holder in controllerList)
            {
                DS4Device currentDev = holder.Device;
                RemoveDeviceEvents(currentDev);
            }
            _colLocker.ExitReadLock();
        }

        private void Service_HotplugController(ControlService sender, DS4Device device, int index)
        {
            SetupDeviceEvents(device);
            _colLocker.EnterWriteLock();
            controllerList.Add(new ControllerHolder(device, index));
            _colLocker.ExitWriteLock();
        }

        private void ProfileListCol_CollectionChanged(object sender,
            System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            PopulateContextMenu();
        }

        private void BuildControllerList(object sender, EventArgs e)
        {
            PopulateControllerList();
        }

        public void PopulateContextMenu()
        {
            contextMenu.Items.Clear();
            ItemCollection items = contextMenu.Items;
            MenuItem item;
            int idx = 0;

            using (ReadLocker locker = new ReadLocker(_colLocker))
            {
                foreach (ControllerHolder holder in controllerList)
                {
                    DS4Device currentDev = holder.Device;
                    item = new MenuItem() { Header = $"Controller {idx + 1}" };
                    item.Tag = idx;
                    //item.ContextMenu = new ContextMenu();
                    ItemCollection subitems = item.Items;
                    string currentProfile = Global.ProfilePath[idx];
                    foreach (ProfileEntity entry in profileListHolder.ProfileListCol)
                    {
                        // Need to escape profile name to disable Access Keys for control
                        string name = entry.Name;
                        name = Regex.Replace(name, "_{1}", "__");
                        MenuItem temp = new MenuItem() { Header = name };
                        temp.Tag = idx;
                        temp.Click += ProfileItem_Click;
                        if (entry.Name == currentProfile)
                        {
                            temp.IsChecked = true;
                        }

                        subitems.Add(temp);
                    }

                    items.Add(item);
                    idx++;
                }

                item = new MenuItem() { Header = "Disconnect Menu" };
                idx = 0;
                foreach (ControllerHolder holder in controllerList)
                {
                    DS4Device tempDev = holder.Device;
                    if (tempDev.Synced && !tempDev.Charging)
                    {
                        MenuItem subitem = new MenuItem() { Header = $"Disconnect Controller {idx + 1}" };
                        subitem.Click += DisconnectMenuItem_Click;
                        subitem.Tag = idx;
                        item.Items.Add(subitem);
                    }

                    idx++;
                }

                if (idx == 0)
                {
                    item.IsEnabled = false;
                }
            }

            items.Add(item);
            items.Add(new Separator());
            PopulateStaticItems();
        }

        private void ChangeControlServiceItem_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            changeServiceItem.IsEnabled = false;
            RequestServiceChange?.Invoke(this, EventArgs.Empty);
        }

        private void OpenProgramFolderItem_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Process.Start(Global.exedirpath);
        }

        private void OpenMenuItem_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            RequestOpen?.Invoke(this, EventArgs.Empty);
        }

        private void MinimizeMenuItem_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            RequestMinimize?.Invoke(this, EventArgs.Empty);
        }

        private void ProfileItem_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            MenuItem item = sender as MenuItem;
            int idx = Convert.ToInt32(item.Tag);
            ControllerHolder holder = controllerList[idx];
            ProfileSelected?.Invoke(this, holder, item.Header.ToString());
        }

        private void DisconnectMenuItem_Click(object sender,
            System.Windows.RoutedEventArgs e)
        {
            MenuItem item = sender as MenuItem;
            int idx = Convert.ToInt32(item.Tag);
            ControllerHolder holder = controllerList[idx];
            DS4Device tempDev = holder?.Device;
            if (tempDev != null && tempDev.Synced && !tempDev.Charging)
            {
                if (tempDev.ConnectionType == ConnectionType.BT)
                {
                    //tempDev.StopUpdate();
                    tempDev.DisconnectBT();
                }
                else if (tempDev.ConnectionType == ConnectionType.SONYWA)
                {
                    tempDev.DisconnectDongle();
                }
            }

            //controllerList[idx] = null;
        }

        private void PopulateControllerList()
        {
            //IEnumerable<DS4Device> devices = DS4Devices.getDS4Controllers();
            int idx = 0;
            _colLocker.EnterWriteLock();
            foreach (DS4Device currentDev in controlService.slotManager.ControllerColl)
            {
                controllerList.Add(new ControllerHolder(currentDev, idx));
                idx++;
            }
            _colLocker.ExitWriteLock();
        }

        private void StartPopulateText(object sender, EventArgs e)
        {
            PopulateToolText();
            //PopulateContextMenu();
        }

        private void PopulateToolText()
        {
            List<string> items = new List<string>();
            items.Add(trayTitle);
            //IEnumerable<DS4Device> devices = DS4Devices.getDS4Controllers();
            int idx = 1;
            //foreach (DS4Device currentDev in devices)
            _colLocker.EnterReadLock();
            foreach (ControllerHolder holder in controllerList)
            {
                DS4Device currentDev = holder.Device;
                items.Add($"{idx}: {currentDev.ConnectionType} {currentDev.Battery}%{(currentDev.Charging ? "+" : "")}");
                idx++;
            }
            _colLocker.ExitReadLock();

            TooltipText = string.Join("\n", items);
        }

        private void SetupEvents()
        {
            //IEnumerable<DS4Device> devices = DS4Devices.getDS4Controllers();
            //foreach (DS4Device currentDev in devices)
            _colLocker.EnterReadLock();
            foreach (ControllerHolder holder in controllerList)
            {
                DS4Device currentDev = holder.Device;
                SetupDeviceEvents(currentDev);
            }
            _colLocker.ExitReadLock();
        }

        private void SetupDeviceEvents(DS4Device device)
        {
            device.BatteryChanged += UpdateForBattery;
            device.ChargingChanged += UpdateForBattery;
            device.Removal += CurrentDev_Removal;
        }

        private void RemoveDeviceEvents(DS4Device device)
        {
            device.BatteryChanged -= UpdateForBattery;
            device.ChargingChanged -= UpdateForBattery;
            device.Removal -= CurrentDev_Removal;
        }

        private void CurrentDev_Removal(object sender, EventArgs e)
        {
            DS4Device currentDev = sender as DS4Device;
            ControllerHolder item = null;
            int idx = 0;

            using (WriteLocker locker = new WriteLocker(_colLocker))
            {
                foreach (ControllerHolder holder in controllerList)
                {
                    if (currentDev == holder.Device)
                    {
                        item = holder;
                        break;
                    }

                    idx++;
                }

                if (item != null)
                {
                    controllerList.RemoveAt(idx);
                    RemoveDeviceEvents(currentDev);
                }
            }

            PopulateToolText();
        }

        private void HookEvents(object sender, EventArgs e)
        {
            SetupEvents();
        }

        private void UpdateForBattery(object sender, EventArgs e)
        {
            PopulateToolText();
        }

        private void ClearToolText(object sender, EventArgs e)
        {
            TooltipText = "DS4Windows";
            //contextMenu.Items.Clear();
        }

        private void PopulateStaticItems()
        {
            ItemCollection items = contextMenu.Items;
            items.Add(changeServiceItem);
            items.Add(openItem);
            items.Add(minimizeItem);
            items.Add(openProgramItem);
            items.Add(new Separator());
            items.Add(closeItem);
        }

        public void ClearContextMenu()
        {
            contextMenu.Items.Clear();
            PopulateStaticItems();
        }

        private void ExitMenuItem_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            RequestShutdown?.Invoke(this, EventArgs.Empty);
        }
    }

    public class ControllerHolder
    {
        private DS4Device device;
        private int index;
        public DS4Device Device { get => device; }
        public int Index { get => index; }

        public ControllerHolder(DS4Device device, int index)
        {
            this.device = device;
            this.index = index;
        }
    }
}
