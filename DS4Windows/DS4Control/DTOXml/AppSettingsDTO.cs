using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using DS4Windows;

namespace DS4WinWPF.DS4Control.DTOXml
{
    [XmlRoot("Profile")]
    public class AppSettingsDTO : IDTO<BackingStore>
    {
        //private XmlDocument tempDoc = new XmlDocument();

        //[XmlAnyElement("ConfigDataComment")]
        //public XmlComment ConfigDataComment
        //{
        //    get
        //    {
        //        return tempDoc.CreateComment(string.Format(" Profile Configuration Data. {0} ", DateTime.Now));
        //    }
        //    set { }
        //}

        //[XmlAnyElement("WrittenWithComment")]
        //public XmlComment WrittenWithComment
        //{
        //    get
        //    {
        //        return tempDoc.CreateComment(string.Format(" Made with DS4Windows version {0} ", Global.exeversion));
        //    }
        //    set { }
        //}

        [XmlAttribute("app_version")]
        public string AppVersion
        {
            get => Global.exeversion;
            set { }
        }

        [XmlAttribute("config_version")]
        public string ConfigVersion
        {
            get => Global.APP_CONFIG_VERSION.ToString();
            set { }
        }


        [XmlElement("useExclusiveMode")]
        public string UseExclusiveModeString
        {
            get => UseExclusiveMode.ToString();
            set
            {
                UseExclusiveMode = XmlDataUtilities.StrToBool(value);
            }
        }

        [XmlIgnore]
        public bool UseExclusiveMode { get; private set; }

        [XmlIgnore]
        public bool StartMinimized { get; private set; }
        [XmlElement("startMinimized")]
        public string StartMinimizedString
        {
            get => StartMinimized.ToString();
            set
            {
                StartMinimized = XmlDataUtilities.StrToBool(value);
            }
        }

        [XmlIgnore]
        public bool MinimizeToTaskbar { get; private set; }

        [XmlElement("minimizeToTaskbar")]
        public string MinimizeToTaskbarString
        {
            get => MinimizeToTaskbar.ToString();
            set => MinimizeToTaskbar = XmlDataUtilities.StrToBool(value);
        }

        [XmlElement("formWidth")]
        public int FormWidth { get; set; } = BackingStore.DEFAULT_FORM_WIDTH;

        [XmlElement("formHeight")]
        public int FormHeight { get; set; } = BackingStore.DEFAULT_FORM_HEIGHT;

        private int _formLocationX;
        [XmlElement("formLocationX")]
        public int FormLocationX
        {
            get => _formLocationX;
            set => _formLocationX = Math.Max(value, 0);
        }

        private int _formLocationY;
        [XmlElement("formLocationY")]
        public int FormLocationY
        {
            get => _formLocationY;
            set => _formLocationY = Math.Max(value, 0);
        }

        [XmlElement("Controller1")]
        public string Controller1CurrentProfile
        {
            get; set;
        }
        public bool ShouldSerializeController1CurrentProfile()
        {
            return !string.IsNullOrEmpty(Controller1CurrentProfile);
        }

        [XmlElement("Controller2")]
        public string Controller2CurrentProfile
        {
            get; set;
        }
        public bool ShouldSerializeController2CurrentProfile()
        {
            return !string.IsNullOrEmpty(Controller2CurrentProfile);
        }

        [XmlElement("Controller3")]
        public string Controller3CurrentProfile
        {
            get; set;
        }
        public bool ShouldSerializeController3CurrentProfile()
        {
            return !string.IsNullOrEmpty(Controller3CurrentProfile);
        }

        [XmlElement("Controller4")]
        public string Controller4CurrentProfile
        {
            get; set;
        }
        public bool ShouldSerializeController4CurrentProfile()
        {
            return !string.IsNullOrEmpty(Controller4CurrentProfile);
        }

        [XmlElement("Controller5")]
        public string Controller5CurrentProfile
        {
            get; set;
        }
        public bool ShouldSerializeController5CurrentProfile()
        {
            return !string.IsNullOrEmpty(Controller5CurrentProfile) &&
                Global.MAX_DS4_CONTROLLER_COUNT >= 5;
        }

        [XmlElement("Controller6")]
        public string Controller6CurrentProfile
        {
            get; set;
        }
        public bool ShouldSerializeController6CurrentProfile()
        {
            return !string.IsNullOrEmpty(Controller6CurrentProfile) &&
                Global.MAX_DS4_CONTROLLER_COUNT >= 6;
        }

        [XmlElement("Controller7")]
        public string Controller7CurrentProfile
        {
            get; set;
        }
        public bool ShouldSerializeController7CurrentProfile()
        {
            return !string.IsNullOrEmpty(Controller7CurrentProfile) &&
                Global.MAX_DS4_CONTROLLER_COUNT >= 7;
        }

        [XmlElement("Controller8")]
        public string Controller8CurrentProfile
        {
            get; set;
        }
        public bool ShouldSerializeController8CurrentProfile()
        {
            return !string.IsNullOrEmpty(Controller8CurrentProfile) &&
                Global.MAX_DS4_CONTROLLER_COUNT >= 8;
        }


        [XmlIgnore]
        public DateTime LastChecked
        {
            get; private set;
        }

        [XmlElement("LastChecked")]
        public string LastCheckString
        {
            get => LastChecked.ToString();
            set
            {
                if (DateTime.TryParse(value, out DateTime temp))
                {
                    LastChecked = temp;
                }
            }
        }

        public int CheckWhen
        {
            get; set;
        } = BackingStore.DEFAULT_CHECK_WHEN;

        public string LastVersionChecked
        {
            get; set;
        } = string.Empty;
        public bool ShouldSerializeLastVersionChecked()
        {
            return !string.IsNullOrEmpty(LastVersionChecked);
        }

        [XmlIgnore]
        public int Notifications
        {
            get; private set;
        } = BackingStore.DEFAULT_NOTIFICATIONS;

        [XmlElement("Notifications")]
        public string NotificationsString
        {
            get => Notifications.ToString();
            set
            {
                if (int.TryParse(value, out int tempNum))
                {
                    Notifications = tempNum;
                }
                else
                {
                    if (bool.TryParse(value, out bool temp))
                    {
                        Notifications = temp ? 2 : 0;
                    }
                }
            }
        }

        [XmlIgnore]
        public bool DisconnectBTAtStop
        {
            get; private set;
        }

        [XmlElement("DisconnectBTAtStop")]
        public string DisconnectBTAtStopString
        {
            get => DisconnectBTAtStop.ToString();
            set
            {
                if (bool.TryParse(value, out bool temp))
                {
                    DisconnectBTAtStop = temp;
                }
            }
        }

        [XmlIgnore]
        public bool SwipeProfiles
        {
            get; private set;
        } = BackingStore.DEFAULT_SWIPE_PROFILES;

        [XmlElement("SwipeProfiles")]
        public string SwipeProfilesString
        {
            get => SwipeProfiles.ToString();
            set
            {
                if (bool.TryParse(value, out bool temp))
                {
                    SwipeProfiles = temp;
                }
            }
        }

        [XmlIgnore]
        public bool QuickCharge
        {
            get; private set;
        }

        [XmlElement("QuickCharge")]
        public string QuickChargeString
        {
            get => QuickCharge.ToString();
            set
            {
                if (bool.TryParse(value, out bool temp))
                {
                    QuickCharge = temp;
                }
            }
        }

        [XmlIgnore]
        public bool CloseMinimizes
        {
            get; private set;
        }

        [XmlElement("CloseMinimizes")]
        public string CloseMinimizesString
        {
            get => CloseMinimizes.ToString();
            set
            {
                if (bool.TryParse(value, out bool temp))
                {
                    CloseMinimizes = temp;
                }
            }
        }

        // Default UseLang to empty string. Not null
        public string UseLang
        {
            get; set;
        } = "";

        [XmlIgnore]
        public bool DownloadLang
        {
            get; private set;
        }

        [XmlElement("DownloadLang")]
        public string DownloadLangString
        {
            get => DownloadLang.ToString();
            set
            {
                if (bool.TryParse(value, out bool temp))
                {
                    DownloadLang = temp;
                }
            }
        }

        [XmlIgnore]
        public bool FlashWhenLate
        {
            get; private set;
        } = BackingStore.DEFAULT_FLASH_WHEN_LATE;

        [XmlElement("FlashWhenLate")]
        public string FlashWhenLateString
        {
            get => FlashWhenLate.ToString();
            set
            {
                if (bool.TryParse(value, out bool temp))
                {
                    FlashWhenLate = temp;
                }
            }
        }

        public int FlashWhenLateAt
        {
            get; set;
        } = BackingStore.DEFAULT_FLASH_WHEN_LATE_AT;

        [XmlIgnore]
        public TrayIconChoice AppIcon
        {
            get; private set;
        }

        [XmlElement("AppIcon")]
        public string AppIconString
        {
            get => AppIcon.ToString();
            set
            {
                if (Enum.TryParse(value, out TrayIconChoice temp))
                {
                    AppIcon = temp;
                }
            }
        }

        [XmlIgnore]
        public AppThemeChoice AppTheme
        {
            get; set;
        }

        [XmlElement("AppTheme")]
        public string AppThemeString
        {
            get => AppTheme.ToString();
            set
            {
                if (Enum.TryParse(value, out AppThemeChoice temp))
                {
                    AppTheme = temp;
                }
            }
        }

        [XmlIgnore]
        public bool UseOSCServer
        {
            get; private set;
        }

        [XmlElement("UseOSCServer")]
        public string UseOSCServerString
        {
            get => UseOSCServer.ToString();
            set
            {
                if (bool.TryParse(value, out bool temp))
                {
                    UseOSCServer = temp;
                }
            }
        }

        public int OSCServerPort
        {
            get; set;
        } = BackingStore.DEFAULT_OSC_SERV_PORT;

        [XmlIgnore]
        public bool UseOSCSender
        {
            get; private set;
        }

        [XmlElement("UseOSCSender")]
        public string UseOSCSenderString
        {
            get => UseOSCSender.ToString();
            set
            {
                if (bool.TryParse(value, out bool temp))
                {
                    UseOSCSender = temp;
                }
            }
        }

        public int OSCSenderPort
        {
            get; set;
        } = BackingStore.DEFAULT_OSC_SEND_PORT;

        public string OSCSenderAddress
        {
            get; set;
        } = BackingStore.DEFAULT_OSC_SEND_ADDRESS;

        [XmlIgnore]
        public bool UseUDPServer
        {
            get; private set;
        }

        [XmlElement("UseUDPServer")]
        public string UseUDPServerString
        {
            get => UseUDPServer.ToString();
            set
            {
                if (bool.TryParse(value, out bool temp))
                {
                    UseUDPServer = temp;
                }
            }
        }

        public int UDPServerPort
        {
            get; set;
        } = BackingStore.DEFAULT_UDP_SERV_PORT;

        public string UDPServerListenAddress
        {
            get; set;
        } = BackingStore.DEFAULT_UDP_SERV_LISTEN_ADDR;

        public UDPSrvSmoothingOptionsGroup UDPServerSmoothingOptions
        {
            get; set;
        }

        [XmlIgnore]
        public bool UseCustomSteamFolder
        {
            get; private set;
        }

        [XmlElement("UseCustomSteamFolder")]
        public string UseCustomSteamFolderString
        {
            get => UseCustomSteamFolder.ToString();
            set
            {
                if (bool.TryParse(value, out bool temp))
                {
                    UseCustomSteamFolder = temp;
                }
            }
        }

        public string CustomSteamFolder
        {
            get; set;
        } = string.Empty;

        [XmlIgnore]
        public bool AutoProfileRevertDefaultProfile
        {
            get; private set;
        } = BackingStore.DEFAULT_AUTO_PROFILE_REVERT_DEFAULT_PROFILE;

        [XmlElement("AutoProfileRevertDefaultProfile")]
        public string AutoProfileRevertDefaultProfileString
        {
            get => AutoProfileRevertDefaultProfile.ToString();
            set
            {
                if (bool.TryParse(value, out bool temp))
                {
                    AutoProfileRevertDefaultProfile = temp;
                }
            }
        }

        [XmlElement("AbsRegionDisplay")]
        public string AbsRegionDisplay
        {
            get; set;
        } = string.Empty;

        public InputDeviceOptions DeviceOptions
        {
            get; set;
        }

        [XmlIgnore]
        public LightbarDS4WinInfo LightbarInfo1
        {
            get; private set;
        }

        [XmlElement("CustomLed1")]
        public string CustomLed1String
        {
            get => BackingStore.CompileCustomLedString(LightbarInfo1);
            set
            {
                BackingStore.ParseCustomLedString(value, LightbarInfo1);
            }
        }

        [XmlIgnore]
        public LightbarDS4WinInfo LightbarInfo2
        {
            get; private set;
        }

        [XmlElement("CustomLed2")]
        public string CustomLed2String
        {
            get => BackingStore.CompileCustomLedString(LightbarInfo2);
            set
            {
                BackingStore.ParseCustomLedString(value, LightbarInfo2);
            }
        }

        [XmlIgnore]
        public LightbarDS4WinInfo LightbarInfo3
        {
            get; private set;
        }

        [XmlElement("CustomLed3")]
        public string CustomLed3String
        {
            get => BackingStore.CompileCustomLedString(LightbarInfo3);
            set
            {
                BackingStore.ParseCustomLedString(value, LightbarInfo3);
            }
        }

        [XmlIgnore]
        public LightbarDS4WinInfo LightbarInfo4
        {
            get; private set;
        }

        [XmlElement("CustomLed4")]
        public string CustomLed4String
        {
            get => BackingStore.CompileCustomLedString(LightbarInfo4);
            set
            {
                BackingStore.ParseCustomLedString(value, LightbarInfo4);
            }
        }

        [XmlIgnore]
        public LightbarDS4WinInfo LightbarInfo5
        {
            get; private set;
        }

        [XmlElement("CustomLed5")]
        public string CustomLed5String
        {
            get => BackingStore.CompileCustomLedString(LightbarInfo5);
            set
            {
                BackingStore.ParseCustomLedString(value, LightbarInfo5);
            }
        }
        public bool ShouldSerializeCustomLed5String()
        {
            return Global.MAX_DS4_CONTROLLER_COUNT >= 5;
        }

        [XmlIgnore]
        public LightbarDS4WinInfo LightbarInfo6
        {
            get; private set;
        }

        [XmlElement("CustomLed6")]
        public string CustomLed6String
        {
            get => BackingStore.CompileCustomLedString(LightbarInfo6);
            set
            {
                BackingStore.ParseCustomLedString(value, LightbarInfo6);
            }
        }
        public bool ShouldSerializeCustomLed6String()
        {
            return Global.MAX_DS4_CONTROLLER_COUNT >= 6;
        }

        [XmlIgnore]
        public LightbarDS4WinInfo LightbarInfo7
        {
            get; private set;
        }

        [XmlElement("CustomLed7")]
        public string CustomLed7String
        {
            get => BackingStore.CompileCustomLedString(LightbarInfo7);
            set
            {
                BackingStore.ParseCustomLedString(value, LightbarInfo7);
            }
        }
        public bool ShouldSerializeCustomLed7String()
        {
            return Global.MAX_DS4_CONTROLLER_COUNT > 7;
        }

        [XmlIgnore]
        public LightbarDS4WinInfo LightbarInfo8
        {
            get; private set;
        }

        [XmlElement("CustomLed8")]
        public string CustomLed8String
        {
            get => BackingStore.CompileCustomLedString(LightbarInfo8);
            set
            {
                BackingStore.ParseCustomLedString(value, LightbarInfo8);
            }
        }
        public bool ShouldSerializeCustomLed8String()
        {
            return Global.MAX_DS4_CONTROLLER_COUNT >= 8;
        }

        public AppSettingsDTO()
        {
            UDPServerSmoothingOptions = new UDPSrvSmoothingOptionsGroup();

            //Controller1CurrentProfile = string.Empty;
            //Controller2CurrentProfile = string.Empty;
            //Controller3CurrentProfile = string.Empty;
            //Controller4CurrentProfile = string.Empty;
            //Controller5CurrentProfile = string.Empty;
            //Controller6CurrentProfile = string.Empty;
            //Controller7CurrentProfile = string.Empty;
            //Controller8CurrentProfile = string.Empty;

            DeviceOptions = new InputDeviceOptions();
            LightbarInfo1 = new LightbarDS4WinInfo();
            LightbarInfo2 = new LightbarDS4WinInfo();
            LightbarInfo3 = new LightbarDS4WinInfo();
            LightbarInfo4 = new LightbarDS4WinInfo();
            LightbarInfo5 = new LightbarDS4WinInfo();
            LightbarInfo6 = new LightbarDS4WinInfo();
            LightbarInfo7 = new LightbarDS4WinInfo();
            LightbarInfo8 = new LightbarDS4WinInfo();
        }

        public void MapFrom(BackingStore source)
        {
            UseExclusiveMode = source.useExclusiveMode;
            StartMinimized = source.startMinimized;
            MinimizeToTaskbar = source.minToTaskbar;
            FormWidth = source.formWidth;
            FormHeight = source.formHeight;
            FormLocationX = source.formLocationX;
            FormLocationY = source.formLocationY;
            Controller1CurrentProfile = source.UsedSavedProfileString(0);
            Controller2CurrentProfile = source.UsedSavedProfileString(1);
            Controller3CurrentProfile = source.UsedSavedProfileString(2);
            Controller4CurrentProfile = source.UsedSavedProfileString(3);
            Controller5CurrentProfile = source.UsedSavedProfileString(4);
            Controller6CurrentProfile = source.UsedSavedProfileString(5);
            Controller7CurrentProfile = source.UsedSavedProfileString(6);
            Controller8CurrentProfile = source.UsedSavedProfileString(7);
            LastChecked = source.lastChecked;
            CheckWhen = source.CheckWhen;
            LastVersionChecked = source.lastVersionChecked;
            Notifications = source.notifications;
            DisconnectBTAtStop = source.disconnectBTAtStop;
            SwipeProfiles = source.swipeProfiles;
            QuickCharge = source.quickCharge;
            CloseMinimizes = source.closeMini;
            UseLang = source.useLang;
            DownloadLang = source.downloadLang;
            FlashWhenLate = source.flashWhenLate;
            FlashWhenLateAt = source.flashWhenLateAt;
            AppIcon = source.useIconChoice;
            AppTheme = source.useCurrentTheme;
            UseOSCServer = source.useOSCServ;
            OSCServerPort = source.oscServPort;
            UseOSCSender = source.useOSCSend;
            OSCSenderPort = source.oscSendPort;
            OSCSenderAddress = source.oscSendAddress;
            UseUDPServer = source.useUDPServ;
            UDPServerPort = source.udpServPort;
            UDPServerListenAddress = source.udpServListenAddress;
            UDPServerSmoothingOptions = new UDPSrvSmoothingOptionsGroup()
            {
                UseSmoothing = source.useUdpSmoothing,
                UdpSmoothMinCutoff = source.udpSmoothingMincutoff,
                UdpSmoothBeta = source.udpSmoothingBeta,
            };
            UseCustomSteamFolder = source.useCustomSteamFolder;
            CustomSteamFolder = source.customSteamFolder;
            AutoProfileRevertDefaultProfile = source.autoProfileRevertDefaultProfile;
            AbsRegionDisplay = source.absDisplayEDID;

            DeviceOptions = new InputDeviceOptions()
            {
                DS4SupportSettings = new DS4SupportSettingsGroup()
                {
                    Enabled = source.deviceOptions.DS4DeviceOpts.Enabled,
                },
                DualSenseSupportSettings = new DualSenseSupportSettings()
                {
                    Enabled = source.deviceOptions.DualSenseOpts.Enabled,
                },
                SwitchProSupportSettings = new SwitchProSupportSettings()
                {
                    Enabled = source.deviceOptions.SwitchProDeviceOpts.Enabled,
                },
                JoyConSupportSettings = new JoyConSupportSettings()
                {
                    Enabled = source.deviceOptions.JoyConDeviceOpts.Enabled,
                    LinkMode = source.deviceOptions.JoyConDeviceOpts.LinkedMode,
                    JoinedGyroProvider = source.deviceOptions.JoyConDeviceOpts.JoinGyroProv,
                }
            };

            LightbarDS4WinInfo[] tempLightArray = new LightbarDS4WinInfo[]
            {
                LightbarInfo1, LightbarInfo2, LightbarInfo3, LightbarInfo4,
                LightbarInfo5, LightbarInfo6, LightbarInfo7, LightbarInfo8,
            };

            for (int i = 0; i < Global.MAX_DS4_CONTROLLER_COUNT; i++)
            {
                LightbarDS4WinInfo lightbarDS4Win = source.ObtainLightbarDS4WinInfo(i);
                LightbarDS4WinInfo tempInstance = tempLightArray[i];
                tempInstance.useCustomLed = lightbarDS4Win.useCustomLed;
                tempInstance.m_CustomLed = lightbarDS4Win.m_CustomLed;
            }
        }

        public void MapTo(BackingStore destination)
        {
            destination.useExclusiveMode = UseExclusiveMode;
            destination.startMinimized = StartMinimized;
            destination.minToTaskbar = MinimizeToTaskbar;
            destination.formWidth = FormWidth;
            destination.formHeight = FormHeight;
            destination.formLocationX = FormLocationX;
            destination.formLocationY = FormLocationY;
            destination.profilePath[0] = destination.olderProfilePath[0] = !string.IsNullOrEmpty(Controller1CurrentProfile) ? Controller1CurrentProfile : string.Empty;
            destination.profilePath[1] = destination.olderProfilePath[1] = !string.IsNullOrEmpty(Controller2CurrentProfile) ? Controller2CurrentProfile : string.Empty;
            destination.profilePath[2] = destination.olderProfilePath[2] = !string.IsNullOrEmpty(Controller3CurrentProfile) ? Controller3CurrentProfile : string.Empty;
            destination.profilePath[3] = destination.olderProfilePath[3] = !string.IsNullOrEmpty(Controller4CurrentProfile) ? Controller4CurrentProfile : string.Empty;
            destination.profilePath[4] = destination.olderProfilePath[4] = !string.IsNullOrEmpty(Controller5CurrentProfile) ? Controller5CurrentProfile : string.Empty;
            destination.profilePath[5] = destination.olderProfilePath[5] = !string.IsNullOrEmpty(Controller6CurrentProfile) ? Controller6CurrentProfile : string.Empty;
            destination.profilePath[6] = destination.olderProfilePath[6] = !string.IsNullOrEmpty(Controller7CurrentProfile) ? Controller7CurrentProfile : string.Empty;
            destination.profilePath[7] = destination.olderProfilePath[7] = !string.IsNullOrEmpty(Controller8CurrentProfile) ? Controller8CurrentProfile : string.Empty;
            destination.lastChecked = LastChecked;
            destination.CheckWhen = CheckWhen;
            destination.lastVersionChecked = LastVersionChecked;
            destination.notifications = Notifications;
            destination.disconnectBTAtStop = DisconnectBTAtStop;
            destination.swipeProfiles = SwipeProfiles;
            destination.quickCharge = QuickCharge;
            destination.closeMini = CloseMinimizes;
            destination.useLang = UseLang;
            destination.downloadLang = DownloadLang;
            destination.flashWhenLate = FlashWhenLate;
            destination.flashWhenLateAt = FlashWhenLateAt;
            destination.useIconChoice = AppIcon;
            destination.useCurrentTheme = AppTheme;
            destination.useOSCServ = UseOSCServer;
            destination.oscServPort = OSCServerPort;
            destination.useOSCSend = UseOSCSender;
            destination.oscSendPort = OSCSenderPort;
            if (!string.IsNullOrEmpty(OSCSenderAddress))
            {
                destination.oscSendAddress = OSCSenderAddress;
            }

            destination.useUDPServ = UseUDPServer;
            destination.udpServPort = UDPServerPort;

            if (!string.IsNullOrEmpty(UDPServerListenAddress))
            {
                destination.udpServListenAddress = UDPServerListenAddress;
            }

            destination.useUdpSmoothing = UDPServerSmoothingOptions.UseSmoothing;
            destination.udpSmoothingMincutoff = UDPServerSmoothingOptions.UdpSmoothMinCutoff;
            destination.udpSmoothingBeta = UDPServerSmoothingOptions.UdpSmoothBeta;

            destination.useCustomSteamFolder = UseCustomSteamFolder;
            destination.customSteamFolder = CustomSteamFolder;
            destination.autoProfileRevertDefaultProfile = AutoProfileRevertDefaultProfile;
            if (!string.IsNullOrEmpty(AbsRegionDisplay))
            {
                destination.absDisplayEDID = AbsRegionDisplay;
            }

            destination.deviceOptions.DS4DeviceOpts.Enabled = DeviceOptions.DS4SupportSettings.Enabled;
            destination.deviceOptions.DualSenseOpts.Enabled = DeviceOptions.DualSenseSupportSettings.Enabled;
            destination.deviceOptions.SwitchProDeviceOpts.Enabled = DeviceOptions.SwitchProSupportSettings.Enabled;
            destination.deviceOptions.JoyConDeviceOpts.Enabled = DeviceOptions.JoyConSupportSettings.Enabled;
            destination.deviceOptions.JoyConDeviceOpts.LinkedMode = DeviceOptions.JoyConSupportSettings.LinkMode;
            destination.deviceOptions.JoyConDeviceOpts.JoinGyroProv = DeviceOptions.JoyConSupportSettings.JoinedGyroProvider;

            LightbarDS4WinInfo[] tempLightArray = new LightbarDS4WinInfo[]
            {
                LightbarInfo1, LightbarInfo2, LightbarInfo3, LightbarInfo4,
                LightbarInfo5, LightbarInfo6, LightbarInfo7, LightbarInfo8,
            };

            for (int i = 0; i < Global.MAX_DS4_CONTROLLER_COUNT; i++)
            {
                LightbarDS4WinInfo tempInstance = tempLightArray[i];
                destination.PopulateLightbarDS4WinInfo(i, tempInstance);
            }
        }
    }

    public class UDPSrvSmoothingOptionsGroup
    {
        [XmlIgnore]
        public bool UseSmoothing
        {
            get; set;
        }

        [XmlElement("UseSmoothing")]
        public string UseSmoothingString
        {
            get => UseSmoothing.ToString();
            set
            {
                if (bool.TryParse(value, out bool temp))
                {
                    UseSmoothing = temp;
                }
            }
        }

        public double UdpSmoothMinCutoff
        {
            get; set;
        } = BackingStore.DEFAULT_UDP_SMOOTH_MINCUTOFF;

        public double UdpSmoothBeta
        {
            get; set;
        } = BackingStore.DEFAULT_UDP_SMOOTH_BETA;
    }

    public class InputDeviceOptions
    {
        public DS4SupportSettingsGroup DS4SupportSettings
        {
            get; set;
        } = new DS4SupportSettingsGroup();

        public DualSenseSupportSettings DualSenseSupportSettings
        {
            get; set;
        } = new DualSenseSupportSettings();

        public SwitchProSupportSettings SwitchProSupportSettings
        {
            get; set;
        } = new SwitchProSupportSettings();

        public JoyConSupportSettings JoyConSupportSettings
        {
            get; set;
        } = new JoyConSupportSettings();
    }

    public abstract class BaseInputDeviceSettingsGroup
    {
        [XmlIgnore]
        public bool Enabled
        {
            get; set;
        }

        [XmlElement("Enabled")]
        public string EnabledString
        {
            get => Enabled.ToString();
            set
            {
                if (bool.TryParse(value, out bool temp))
                {
                    Enabled = temp;
                }
            }
        }
    }

    public class DS4SupportSettingsGroup : BaseInputDeviceSettingsGroup
    {
        public DS4SupportSettingsGroup(): base()
        {
            Enabled = DS4DeviceOptions.DEFAULT_ENABLE;
        }
    }

    public class DualSenseSupportSettings : BaseInputDeviceSettingsGroup
    {
    }

    public class SwitchProSupportSettings : BaseInputDeviceSettingsGroup
    {
    }

    public class JoyConSupportSettings : BaseInputDeviceSettingsGroup
    {
        public JoyConDeviceOptions.LinkMode LinkMode
        {
            get; set;
        }

        public JoyConDeviceOptions.JoinedGyroProvider JoinedGyroProvider
        {
            get; set;
        }
    }
}
