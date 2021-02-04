using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using DS4Windows;

namespace DS4WinWPF.DS4Forms.ViewModels
{
    public class ControllerListViewModel
    {
        //private object _colLockobj = new object();
        private ReaderWriterLockSlim _colListLocker = new ReaderWriterLockSlim();
        private ObservableCollection<CompositeDeviceModel> controllerCol =
            new ObservableCollection<CompositeDeviceModel>();
        private Dictionary<int, CompositeDeviceModel> controllerDict =
            new Dictionary<int, CompositeDeviceModel>();

        public ObservableCollection<CompositeDeviceModel> ControllerCol
        { get => controllerCol; set => controllerCol = value; }

        private ProfileList profileListHolder;
        private ControlService controlService;
        private int currentIndex;
        public int CurrentIndex { get => currentIndex; set => currentIndex = value; }
        public CompositeDeviceModel CurrentItem {
            get
            {
                if (currentIndex == -1) return null;
                controllerDict.TryGetValue(currentIndex, out CompositeDeviceModel item);
                return item;
            }
        }

        public Dictionary<int, CompositeDeviceModel> ControllerDict { get => controllerDict; set => controllerDict = value; }

        //public ControllerListViewModel(Tester tester, ProfileList profileListHolder)
        public ControllerListViewModel(ControlService service, ProfileList profileListHolder)
        {
            this.profileListHolder = profileListHolder;
            this.controlService = service;
            service.ServiceStarted += ControllersChanged;
            service.PreServiceStop += ClearControllerList;
            service.HotplugController += Service_HotplugController;
            //tester.StartControllers += ControllersChanged;
            //tester.ControllersRemoved += ClearControllerList;

            int idx = 0;
            foreach (DS4Device currentDev in controlService.slotManager.ControllerColl)
            {
                CompositeDeviceModel temp = new CompositeDeviceModel(currentDev,
                    idx, Global.ProfilePath[idx], profileListHolder);
                controllerCol.Add(temp);
                controllerDict.Add(idx, temp);
                currentDev.Removal += Controller_Removal;
                idx++;
            }

            //BindingOperations.EnableCollectionSynchronization(controllerCol, _colLockobj);
            BindingOperations.EnableCollectionSynchronization(controllerCol, _colListLocker,
                ColLockCallback);
        }

        private void ColLockCallback(IEnumerable collection, object context,
            Action accessMethod, bool writeAccess)
        {
            if (writeAccess)
            {
                using (WriteLocker locker = new WriteLocker(_colListLocker))
                {
                    accessMethod?.Invoke();
                }
            }
            else
            {
                using (ReadLocker locker = new ReadLocker(_colListLocker))
                {
                    accessMethod?.Invoke();
                }
            }
        }

        private void Service_HotplugController(ControlService sender, DS4Device device, int index)
        {
            CompositeDeviceModel temp = new CompositeDeviceModel(device,
                index, Global.ProfilePath[index], profileListHolder);
            _colListLocker.EnterWriteLock();
            controllerCol.Add(temp);
            controllerDict.Add(index, temp);
            _colListLocker.ExitWriteLock();

            device.Removal += Controller_Removal;
        }

        private void ClearControllerList(object sender, EventArgs e)
        {
            _colListLocker.EnterReadLock();
            foreach (CompositeDeviceModel temp in controllerCol)
            {
                temp.Device.Removal -= Controller_Removal;
            }
            _colListLocker.ExitReadLock();

            _colListLocker.EnterWriteLock();
            controllerCol.Clear();
            controllerDict.Clear();
            _colListLocker.ExitWriteLock();
        }

        private void ControllersChanged(object sender, EventArgs e)
        {
            //IEnumerable<DS4Device> devices = DS4Windows.DS4Devices.getDS4Controllers();
            using (ReadLocker locker = new ReadLocker(controlService.slotManager.CollectionLocker))
            {
                foreach (DS4Device currentDev in controlService.slotManager.ControllerColl)
                {
                    bool found = false;
                    _colListLocker.EnterReadLock();
                    foreach (CompositeDeviceModel temp in controllerCol)
                    {
                        if (temp.Device == currentDev)
                        {
                            found = true;
                            break;
                        }
                    }
                    _colListLocker.ExitReadLock();


                    if (!found)
                    {
                        //int idx = controllerCol.Count;
                        _colListLocker.EnterWriteLock();
                        int idx = controlService.slotManager.ReverseControllerDict[currentDev];
                        CompositeDeviceModel temp = new CompositeDeviceModel(currentDev,
                            idx, Global.ProfilePath[idx], profileListHolder);
                        controllerCol.Add(temp);
                        controllerDict.Add(idx, temp);
                        _colListLocker.ExitWriteLock();

                        currentDev.Removal += Controller_Removal;
                    }
                }
            }
        }

        private void Controller_Removal(object sender, EventArgs e)
        {
            DS4Device currentDev = sender as DS4Device;
            CompositeDeviceModel found = null;
            _colListLocker.EnterReadLock();
            foreach (CompositeDeviceModel temp in controllerCol)
            {
                if (temp.Device == currentDev)
                {
                    found = temp;
                    break;
                }
            }
            _colListLocker.ExitReadLock();

            if (found != null)
            {
                _colListLocker.EnterWriteLock();
                controllerCol.Remove(found);
                controllerDict.Remove(found.DevIndex);
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    Global.Save();
                });
                Global.linkedProfileCheck[found.DevIndex] = false;
                _colListLocker.ExitWriteLock();
            }
        }
    }

    public class CompositeDeviceModel
    {
        private DS4Device device;
        private string selectedProfile;
        private ProfileList profileListHolder;
        private ProfileEntity selectedEntity;
        private int selectedIndex = 1;
        private int devIndex;

        public DS4Device Device { get => device; set => device = value; }
        public string SelectedProfile { get => selectedProfile; set => selectedProfile = value; }
        public ProfileList ProfileEntities { get => profileListHolder; set => profileListHolder = value; }
        public ObservableCollection<ProfileEntity> ProfileListCol => profileListHolder.ProfileListCol;

        public string LightColor
        {
            get
            {
                DS4Color color;
                if (Global.LightbarSettingsInfo[devIndex].ds4winSettings.useCustomLed)
                {
                    color = Global.LightbarSettingsInfo[devIndex].ds4winSettings.m_CustomLed; //Global.CustomColor[devIndex];
                }
                else
                {
                    color = Global.LightbarSettingsInfo[devIndex].ds4winSettings.m_Led;
                }
                return $"#FF{color.red.ToString("X2")}{color.green.ToString("X2")}{color.blue.ToString("X2")}";
            }
        }

        public event EventHandler LightColorChanged;

        public Color CustomLightColor
        {
            get
            {
                DS4Color color;
                color = Global.LightbarSettingsInfo[devIndex].ds4winSettings.m_CustomLed;
                return new Color() { R = color.red, G = color.green, B = color.blue, A = 255 };
            }
        }

        public string BatteryState
        {
            get
            {
                string temp = $"{device.Battery}%{(device.Charging ? "+" : "")}";
                return temp;
            }
        }
        public event EventHandler BatteryStateChanged;

        public int SelectedIndex
        {
            get => selectedIndex;
            set
            {
                if (selectedIndex == value) return;
                selectedIndex = value;
                SelectedIndexChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler SelectedIndexChanged;

        public string StatusSource
        {
            get
            {
                string imgName = (string)App.Current.FindResource(device.ConnectionType == ConnectionType.USB ? "UsbImg" : "BtImg");
                string source = $"/DS4Windows;component/Resources/{imgName}";
                return source;
            }
        }

        public string ExclusiveSource
        {
            get
            {
                string imgName = (string)App.Current.FindResource("CancelImg");
                string source = $"/DS4Windows;component/Resources/{imgName}";
                switch(device.CurrentExclusiveStatus)
                {
                    case DS4Device.ExclusiveStatus.Exclusive:
                        imgName = (string)App.Current.FindResource("CheckedImg");
                        source = $"/DS4Windows;component/Resources/{imgName}";
                        break;
                    case DS4Device.ExclusiveStatus.HidGuardAffected:
                        imgName = (string)App.Current.FindResource("KeyImageImg");
                        source = $"/DS4Windows;component/Resources/{imgName}";
                        break;
                    default:
                        break;
                }

                return source;
            }
        }

        public bool LinkedProfile
        {
            get
            {
                return Global.linkedProfileCheck[devIndex];
            }
            set
            {
                bool temp = Global.linkedProfileCheck[devIndex];
                if (temp == value) return;
                Global.linkedProfileCheck[devIndex] = value;
                SaveLinked(value);
            }
        }

        public int DevIndex { get => devIndex; }
        public int DisplayDevIndex { get => devIndex + 1; }

        public string TooltipIDText
        {
            get
            {
                string temp = string.Format(Properties.Resources.InputDelay, device.Latency);
                return temp;
            }
        }

        public event EventHandler TooltipIDTextChanged;

        private bool useCustomColor;
        public bool UseCustomColor { get => useCustomColor; set => useCustomColor = value; }

        private ContextMenu lightContext;
        public ContextMenu LightContext { get => lightContext; set => lightContext = value; }

        public string IdText
        {
            get => $"{device.DisplayName} ({device.MacAddress})";
        }
        public event EventHandler IdTextChanged;

        public string IsExclusiveText
        {
            get
            {
                string temp = "Shared Access";
                switch(device.CurrentExclusiveStatus)
                {
                    case DS4Device.ExclusiveStatus.Exclusive:
                        temp = "Exclusive Access";
                        break;
                    case DS4Device.ExclusiveStatus.HidGuardAffected:
                        temp = "HidGuardian Access";
                        break;
                    default:
                        break;
                }

                return temp;
            }
        }

        public delegate void CustomColorHandler(CompositeDeviceModel sender);
        public event CustomColorHandler RequestColorPicker;

        public CompositeDeviceModel(DS4Device device, int devIndex, string profile,
            ProfileList collection)
        {
            this.device = device;
            device.BatteryChanged += (sender, e) => BatteryStateChanged?.Invoke(this, e);
            device.ChargingChanged += (sender, e) => BatteryStateChanged?.Invoke(this, e);
            device.MacAddressChanged += (sender, e) => IdTextChanged?.Invoke(this, e);
            this.devIndex = devIndex;
            this.selectedProfile = profile;
            profileListHolder = collection;
            if (!string.IsNullOrEmpty(selectedProfile))
            {
                this.selectedEntity = profileListHolder.ProfileListCol.SingleOrDefault(x => x.Name == selectedProfile);
            }

            if (this.selectedEntity != null)
            {
                selectedIndex = profileListHolder.ProfileListCol.IndexOf(this.selectedEntity);
                HookEvents(true);
            }

            useCustomColor = Global.LightbarSettingsInfo[devIndex].ds4winSettings.useCustomLed;
        }

        public void ChangeSelectedProfile()
        {
            if (this.selectedEntity != null)
            {
                HookEvents(false);
            }

            string prof = Global.ProfilePath[devIndex] = ProfileListCol[selectedIndex].Name;
            if (LinkedProfile)
            {
                Global.changeLinkedProfile(device.getMacAddress(), Global.ProfilePath[devIndex]);
                Global.SaveLinkedProfiles();
            }
            else
            {
                Global.OlderProfilePath[devIndex] = Global.ProfilePath[devIndex];
            }

            //Global.Save();
            Global.LoadProfile(devIndex, true, App.rootHub);
            string prolog = string.Format(Properties.Resources.UsingProfile, (devIndex + 1).ToString(), prof, $"{device.Battery}");
            DS4Windows.AppLogger.LogToGui(prolog, false);

            selectedProfile = prof;
            this.selectedEntity = profileListHolder.ProfileListCol.SingleOrDefault(x => x.Name == prof);
            if (this.selectedEntity != null)
            {
                selectedIndex = profileListHolder.ProfileListCol.IndexOf(this.selectedEntity);
                HookEvents(true);
            }

            LightColorChanged?.Invoke(this, EventArgs.Empty);
        }

        private void HookEvents(bool state)
        {
            if (state)
            {
                selectedEntity.ProfileSaved += SelectedEntity_ProfileSaved;
                selectedEntity.ProfileDeleted += SelectedEntity_ProfileDeleted;
            }
            else
            {
                selectedEntity.ProfileSaved -= SelectedEntity_ProfileSaved;
                selectedEntity.ProfileDeleted -= SelectedEntity_ProfileDeleted;
            }
        }

        private void SelectedEntity_ProfileDeleted(object sender, EventArgs e)
        {
            HookEvents(false);
            ProfileEntity entity = profileListHolder.ProfileListCol.FirstOrDefault();
            if (entity != null)
            {
                SelectedIndex = profileListHolder.ProfileListCol.IndexOf(entity);
            }
        }

        private void SelectedEntity_ProfileSaved(object sender, EventArgs e)
        {
            Global.LoadProfile(devIndex, false, App.rootHub);
            LightColorChanged?.Invoke(this, EventArgs.Empty);
        }

        public void RequestUpdatedTooltipID()
        {
            TooltipIDTextChanged?.Invoke(this, EventArgs.Empty);
        }

        private void SaveLinked(bool status)
        {
            if (device != null && device.isSynced())
            {
                if (status)
                {
                    if (device.isValidSerial())
                    {
                        Global.changeLinkedProfile(device.getMacAddress(), Global.ProfilePath[devIndex]);
                    }
                }
                else
                {
                    Global.removeLinkedProfile(device.getMacAddress());
                    Global.ProfilePath[devIndex] = Global.OlderProfilePath[devIndex];
                }

                Global.SaveLinkedProfiles();
            }
        }

        public void AddLightContextItems()
        {
            MenuItem thing = new MenuItem() { Header = "Use Profile Color", IsChecked = !useCustomColor };
            thing.Click += ProfileColorMenuClick;
            lightContext.Items.Add(thing);
            thing = new MenuItem() { Header = "Use Custom Color", IsChecked = useCustomColor };
            thing.Click += CustomColorItemClick;
            lightContext.Items.Add(thing);
        }

        private void ProfileColorMenuClick(object sender, System.Windows.RoutedEventArgs e)
        {
            useCustomColor = false;
            RefreshLightContext();
            Global.LightbarSettingsInfo[devIndex].ds4winSettings.useCustomLed = false;
            LightColorChanged?.Invoke(this, EventArgs.Empty);
        }

        private void CustomColorItemClick(object sender, System.Windows.RoutedEventArgs e)
        {
            useCustomColor = true;
            RefreshLightContext();
            Global.LightbarSettingsInfo[devIndex].ds4winSettings.useCustomLed = true;
            LightColorChanged?.Invoke(this, EventArgs.Empty);
            RequestColorPicker?.Invoke(this);
        }

        private void RefreshLightContext()
        {
            (lightContext.Items[0] as MenuItem).IsChecked = !useCustomColor;
            (lightContext.Items[1] as MenuItem).IsChecked = useCustomColor;
        }

        public void UpdateCustomLightColor(Color color)
        {
            Global.LightbarSettingsInfo[devIndex].ds4winSettings.m_CustomLed = new DS4Color() { red = color.R, green = color.G, blue = color.B };
            LightColorChanged?.Invoke(this, EventArgs.Empty);
        }

        public void ChangeSelectedProfile(string loadprofile)
        {
            ProfileEntity temp = profileListHolder.ProfileListCol.SingleOrDefault(x => x.Name == loadprofile);
            if (temp != null)
            {
                SelectedIndex = profileListHolder.ProfileListCol.IndexOf(temp);
            }
        }

        public void RequestDisconnect()
        {
            if (device.Synced && !device.Charging)
            {
                if (device.ConnectionType == ConnectionType.BT)
                {
                    //device.StopUpdate();
                    device.queueEvent(() =>
                    {
                        device.DisconnectBT();
                    });
                }
                else if (device.ConnectionType == ConnectionType.SONYWA)
                {
                    device.DisconnectDongle();
                }
            }
        }
    }
}
