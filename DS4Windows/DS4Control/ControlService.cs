using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;
using System.Windows.Threading;
using System.Linq;
using System.Text;
using Sensorit.Base;
using Nefarius.ViGEm.Client;
using SharpOSC;
using static DS4Windows.Global;
using DS4WinWPF.DS4Control;
using DS4Windows.DS4Control;

namespace DS4Windows
{
    public class ControlService
    {
        public ViGEmClient vigemTestClient = null;
        // Might be useful for ScpVBus build
        public const int EXPANDED_CONTROLLER_COUNT = 8;
        public const int MAX_DS4_CONTROLLER_COUNT = Global.MAX_DS4_CONTROLLER_COUNT;
#if FORCE_4_INPUT
        public static int CURRENT_DS4_CONTROLLER_LIMIT = Global.OLD_XINPUT_CONTROLLER_COUNT;
#else
        public static int CURRENT_DS4_CONTROLLER_LIMIT = Global.IsWin8OrGreater() ? MAX_DS4_CONTROLLER_COUNT : Global.OLD_XINPUT_CONTROLLER_COUNT;
#endif
        public static bool USING_MAX_CONTROLLERS = CURRENT_DS4_CONTROLLER_LIMIT == EXPANDED_CONTROLLER_COUNT;
        public DS4Device[] DS4Controllers = new DS4Device[MAX_DS4_CONTROLLER_COUNT];
        public int activeControllers = 0;
        public Mouse[] touchPad = new Mouse[MAX_DS4_CONTROLLER_COUNT];
        public bool running = false;
        public bool loopControllers = true;
        public bool inServiceTask = false;
        private DS4State[] MappedState = new DS4State[MAX_DS4_CONTROLLER_COUNT];
        private DS4State[] CurrentState = new DS4State[MAX_DS4_CONTROLLER_COUNT];
        private DS4State[] PreviousState = new DS4State[MAX_DS4_CONTROLLER_COUNT];
        private DS4State[] TempState = new DS4State[MAX_DS4_CONTROLLER_COUNT];
        public DS4StateExposed[] ExposedState = new DS4StateExposed[MAX_DS4_CONTROLLER_COUNT];
        public ControllerSlotManager slotManager = new ControllerSlotManager();
        public bool recordingMacro = false;
        public event EventHandler<DebugEventArgs> Debug = null;
        bool[] buttonsdown = new bool[MAX_DS4_CONTROLLER_COUNT] { false, false, false, false, false, false, false, false };
        bool[] held = new bool[MAX_DS4_CONTROLLER_COUNT];
        int[] oldmouse = new int[MAX_DS4_CONTROLLER_COUNT] { -1, -1, -1, -1, -1, -1, -1, -1 };
        public OutputDevice[] outputDevices = new OutputDevice[MAX_DS4_CONTROLLER_COUNT] { null, null, null, null, null, null, null, null };
        private OneEuroFilter3D[] udpEuroPairAccel = new OneEuroFilter3D[UdpServer.NUMBER_SLOTS]
        {
            new OneEuroFilter3D(), new OneEuroFilter3D(),
            new OneEuroFilter3D(), new OneEuroFilter3D(),
        };
        private OneEuroFilter3D[] udpEuroPairGyro = new OneEuroFilter3D[UdpServer.NUMBER_SLOTS]
        {
            new OneEuroFilter3D(), new OneEuroFilter3D(),
            new OneEuroFilter3D(), new OneEuroFilter3D(),
        };
        Thread tempThread;
        Thread tempBusThread;
        Thread eventDispatchThread;
        Dispatcher eventDispatcher;
        public bool suspending;

        private UdpServer _udpServer;
        private OutputSlotManager outputslotMan;

        private HashSet<string> hidDeviceHidingAffectedDevs = new HashSet<string>();
        private HashSet<string> hidDeviceHidingExemptedDevs = new HashSet<string>();
        private bool hidDeviceHidingForced = false;
        private bool hidDeviceHidingEnabled = false;

        private ControlServiceDeviceOptions deviceOptions;
        public ControlServiceDeviceOptions DeviceOptions { get => deviceOptions; }

        private DS4WinWPF.ArgumentParser cmdParser;

        public event EventHandler ServiceStarted;
        public event EventHandler PreServiceStop;
        public event EventHandler ServiceStopped;
        public event EventHandler RunningChanged;
        //public event EventHandler HotplugFinished;
        public delegate void HotplugControllerHandler(ControlService sender, DS4Device device, int index);
        public event HotplugControllerHandler HotplugController;

        private byte[][] udpOutBuffers = new byte[UdpServer.NUMBER_SLOTS][]
        {
            new byte[UdpServer.DATA_RSP_PACKET_LEN], new byte[UdpServer.DATA_RSP_PACKET_LEN],
            new byte[UdpServer.DATA_RSP_PACKET_LEN], new byte[UdpServer.DATA_RSP_PACKET_LEN],
        };

        private DS4State[] oscState = new DS4State[MAX_DS4_CONTROLLER_COUNT];
        public HandleOscPacket oscCallback;

        public UDPListener oscListener;
        public UDPSender oscSender;

        void GetPadDetailForIdx(int padIdx, ref DualShockPadMeta meta)
        {
            //meta = new DualShockPadMeta();
            meta.PadId = (byte) padIdx;
            meta.Model = DsModel.DS4;

            var d = DS4Controllers[padIdx];
            if (d == null)
            {
                meta.PadMacAddress = null;
                meta.PadState = DsState.Disconnected;
                meta.ConnectionType = DsConnection.None;
                meta.Model = DsModel.None;
                meta.BatteryStatus = 0;
                meta.IsActive = false;
                return;
                //return meta;
            }

            bool isValidSerial = false;
            string stringMac = d.getMacAddress();
            if (!string.IsNullOrEmpty(stringMac))
            {
                stringMac = string.Join("", stringMac.Split(':'));
                //stringMac = stringMac.Replace(":", "").Trim();
                meta.PadMacAddress = System.Net.NetworkInformation.PhysicalAddress.Parse(stringMac);
                isValidSerial = d.isValidSerial();
            }

            if (!isValidSerial)
            {
                //meta.PadMacAddress = null;
                meta.PadState = DsState.Disconnected;
            }
            else
            {
                if (d.isSynced() || d.IsAlive())
                    meta.PadState = DsState.Connected;
                else
                    meta.PadState = DsState.Reserved;
            }

            meta.ConnectionType = (d.getConnectionType() == ConnectionType.USB) ? DsConnection.Usb : DsConnection.Bluetooth;
            meta.IsActive = !d.isDS4Idle();

            int batteryLevel = d.getBattery();
            if (d.isCharging() && batteryLevel >= 100)
                meta.BatteryStatus = DsBattery.Charged;
            else
            {
                if (batteryLevel >= 95)
                    meta.BatteryStatus = DsBattery.Full;
                else if (batteryLevel >= 70)
                    meta.BatteryStatus = DsBattery.High;
                else if (batteryLevel >= 50)
                    meta.BatteryStatus = DsBattery.Medium;
                else if (batteryLevel >= 20)
                    meta.BatteryStatus = DsBattery.Low;
                else if (batteryLevel >= 5)
                    meta.BatteryStatus = DsBattery.Dying;
                else
                    meta.BatteryStatus = DsBattery.None;
            }

            //return meta;
        }

        private object busThrLck = new object();
        private bool busThrRunning = false;
        private Queue<Action> busEvtQueue = new Queue<Action>();
        private object busEvtQueueLock = new object();
        public ControlService(DS4WinWPF.ArgumentParser cmdParser)
        {
            this.cmdParser = cmdParser;

            Crc32Algorithm.InitializeTable(DS4Device.DefaultPolynomial);
            InitOutputKBMHandler();

            // Cause thread affinity to not be tied to main GUI thread
            tempBusThread = new Thread(() =>
            {
                //_udpServer = new UdpServer(GetPadDetailForIdx);
                busThrRunning = true;

                while (busThrRunning)
                {
                    lock (busEvtQueueLock)
                    {
                        Action tempAct = null;
                        for (int actInd = 0, actLen = busEvtQueue.Count; actInd < actLen; actInd++)
                        {
                            tempAct = busEvtQueue.Dequeue();
                            tempAct.Invoke();
                        }
                    }

                    lock (busThrLck)
                        Monitor.Wait(busThrLck);
                }
            });
            tempBusThread.Priority = ThreadPriority.Normal;
            tempBusThread.IsBackground = true;
            tempBusThread.Start();
            //while (_udpServer == null)
            //{
            //    Thread.SpinWait(500);
            //}

            eventDispatchThread = new Thread(() =>
            {
                Dispatcher currentDis = Dispatcher.CurrentDispatcher;
                eventDispatcher = currentDis;
                Dispatcher.Run();
            });
            eventDispatchThread.IsBackground = true;
            eventDispatchThread.Priority = ThreadPriority.Normal;
            eventDispatchThread.Name = "ControlService Events";
            eventDispatchThread.Start();

            for (int i = 0, arlength = DS4Controllers.Length; i < arlength; i++)
            {
                MappedState[i] = new DS4State();
                CurrentState[i] = new DS4State();
                TempState[i] = new DS4State();
                PreviousState[i] = new DS4State();
                ExposedState[i] = new DS4StateExposed(CurrentState[i]);
                oscState[i] = new DS4State();

                int tempDev = i;
                Global.L2OutputSettings[i].TwoStageModeChanged += (sender, e) =>
                {
                    Mapping.l2TwoStageMappingData[tempDev].Reset();
                };

                Global.R2OutputSettings[i].TwoStageModeChanged += (sender, e) =>
                {
                    Mapping.r2TwoStageMappingData[tempDev].Reset();
                };
            }

            outputslotMan = new OutputSlotManager();
            deviceOptions = Global.DeviceOptions;

            DS4Devices.RequestElevation += DS4Devices_RequestElevation;
            DS4Devices.checkVirtualFunc = CheckForVirtualDevice;
            DS4Devices.PrepareDS4Init = PrepareDS4DeviceInit;
            DS4Devices.PostDS4Init = PostDS4DeviceInit;
            DS4Devices.PreparePendingDevice = CheckForSupportedDevice;
            outputslotMan.ViGEmFailure += OutputslotMan_ViGEmFailure;

            Global.UDPServerSmoothingMincutoffChanged += ChangeUdpSmoothingAttrs;
            Global.UDPServerSmoothingBetaChanged += ChangeUdpSmoothingAttrs;

            CreateOSCCallback();

            //oscListener = new UDPListener(Global.getOSCServerPortNum(), callback: oscCallback);
            //AppLogger.LogToGui("OSC LISTENER STARTED", false);
        }

        private void CreateOSCCallback()
        {
            oscCallback = delegate (OscPacket packet)
            {
                var messageReceived = (OscMessage)packet;

                var command = messageReceived.Address.Split("/");
                //AppLogger.LogToGui("I HEARD SOMETHING " + messageReceived.Address, false);
                if (command[1] != "ds4windows") { return; }

                int stateInd = Convert.ToInt32(command[2]);
                if (command[3] == "battery")
                {
                    if (!isUsingOSCSender())
                    {
                        AppLogger.LogToGui("Battery level requested, but the OSC Sender isn't active. Turn it on in Settings.", false);
                    }
                    else
                    {
                        oscSender.Send(new SharpOSC.OscMessage("/ds4windows/monitor/" + stateInd + "/battery", oscState[stateInd].Battery));
                    }
                }
                if (command[3] == "press")
                {
                    int messageValue = Convert.ToInt32(messageReceived.Arguments[0]);
                    bool buttonBool = messageValue == 1 ? true : false;
                    //AppLogger.LogToGui("OSC BUTTON PRESS " + command[4] + ": " + buttonBool, false);

                    switch (command[4])
                    {
                        case "cross":
                            oscState[stateInd].Cross = buttonBool;
                            break;
                        case "square":
                            oscState[stateInd].Square = buttonBool;
                            break;
                        case "circle":
                            oscState[stateInd].Circle = buttonBool;
                            break;
                        case "triangle":
                            oscState[stateInd].Triangle = buttonBool;
                            break;
                        case "r1":
                            oscState[stateInd].R1 = buttonBool;
                            break;
                        case "r2":
                            oscState[stateInd].R2Btn = buttonBool;
                            break;
                        case "r3":
                            oscState[stateInd].R3 = buttonBool;
                            break;
                        case "l1":
                            oscState[stateInd].L1 = buttonBool;
                            break;
                        case "l2":
                            oscState[stateInd].L2Btn = buttonBool;
                            break;
                        case "l3":
                            oscState[stateInd].L3 = buttonBool;
                            break;
                        case "dup":
                            oscState[stateInd].DpadUp = buttonBool;
                            break;
                        case "ddown":
                            oscState[stateInd].DpadDown = buttonBool;
                            break;
                        case "dleft":
                            oscState[stateInd].DpadLeft = buttonBool;
                            break;
                        case "dright":
                            oscState[stateInd].DpadRight = buttonBool;
                            break;
                        case "options":
                            oscState[stateInd].Options = buttonBool;
                            break;
                        case "share":
                            oscState[stateInd].Share = buttonBool;
                            break;
                    }
                }

                if (command[3] == "stick")
                {
                    //AppLogger.LogToGui("OSC STICK COMMAND " + messageReceived.Arguments[0].GetType(), false);
                    float xValue = Convert.ToSingle(messageReceived.Arguments[0]);
                    float yValue = Convert.ToSingle(messageReceived.Arguments[1]);
                    //AppLogger.LogToGui("OSC STICK " + xValue + ": " + yValue, false);
                    if (command[4] == "left")
                    {
                        oscState[stateInd].LX = Convert.ToByte(xValue * 255);
                        oscState[stateInd].LY = Convert.ToByte(yValue * 255);
                    }
                    else if (command[4] == "right")
                    {
                        oscState[stateInd].RX = Convert.ToByte(xValue * 255);
                        oscState[stateInd].RY = Convert.ToByte(yValue * 255);
                    }
                }
            };
        }

        public void RefreshOutputKBMHandler()
        {
            if (Global.outputKBMHandler != null)
            {
                Global.outputKBMHandler.Disconnect();
                Global.outputKBMHandler = null;
            }

            if (Global.outputKBMMapping != null)
            {
                Global.outputKBMMapping = null;
            }

            InitOutputKBMHandler();
        }

        private void InitOutputKBMHandler()
        {
            string attemptVirtualkbmHandler = cmdParser.VirtualkbmHandler;
            Global.InitOutputKBMHandler(attemptVirtualkbmHandler);

            bool handlerConnected = false;
            try
            {
                handlerConnected = Global.outputKBMHandler.Connect();
            }
            catch { }

            if (!handlerConnected &&
                attemptVirtualkbmHandler != VirtualKBMFactory.GetFallbackHandlerIdentifier())
            {
                Global.outputKBMHandler = VirtualKBMFactory.GetFallbackHandler();
            }
            else
            {
                // Connection was made. Check if version number should get populated
                if (outputKBMHandler.GetIdentifier() == FakerInputHandler.IDENTIFIER)
                {
                    Global.outputKBMHandler.Version = Global.fakerInputVersion;
                }
            }

            Global.InitOutputKBMMapping(Global.outputKBMHandler.GetIdentifier());
            Global.outputKBMMapping.PopulateConstants();
            Global.outputKBMMapping.PopulateMappings();
        }

        private void OutputslotMan_ViGEmFailure(object sender, EventArgs e)
        {
            eventDispatcher.BeginInvoke((Action)(() =>
            {
                loopControllers = false;
                while (inServiceTask)
                    Thread.SpinWait(1000);

                LogDebug(DS4WinWPF.Translations.Strings.ViGEmPluginFailure, true);
                Stop();
            }));
        }

        public void PostDS4DeviceInit(DS4Device device)
        {
            if (device.DeviceType == InputDevices.InputDeviceType.JoyConL ||
                device.DeviceType == InputDevices.InputDeviceType.JoyConR)
            {
                if (deviceOptions.JoyConDeviceOpts.LinkedMode == JoyConDeviceOptions.LinkMode.Joined)
                {
                    InputDevices.JoyConDevice tempJoyDev = device as InputDevices.JoyConDevice;
                    tempJoyDev.PerformStateMerge = true;

                    if (device.DeviceType == InputDevices.InputDeviceType.JoyConL)
                    {
                        tempJoyDev.PrimaryDevice = true;
                        if (deviceOptions.JoyConDeviceOpts.JoinGyroProv == JoyConDeviceOptions.JoinedGyroProvider.JoyConL)
                        {
                            tempJoyDev.OutputMapGyro = true;
                        }
                        else
                        {
                            tempJoyDev.OutputMapGyro = false;
                        }
                    }
                    else
                    {
                        tempJoyDev.PrimaryDevice = false;
                        if (deviceOptions.JoyConDeviceOpts.JoinGyroProv == JoyConDeviceOptions.JoinedGyroProvider.JoyConR)
                        {
                            tempJoyDev.OutputMapGyro = true;
                        }
                        else
                        {
                            tempJoyDev.OutputMapGyro = false;
                        }
                    }
                }
            }
        }

        private void PrepareDS4DeviceSettingHooks(DS4Device device)
        {
            if (device.DeviceType == InputDevices.InputDeviceType.DualSense)
            {
                InputDevices.DualSenseDevice tempDSDev = device as InputDevices.DualSenseDevice;

                DualSenseControllerOptions dSOpts = tempDSDev.NativeOptionsStore;
                dSOpts.LedModeChanged += (sender, e) => { tempDSDev.CheckControllerNumDeviceSettings(activeControllers); };
            }
            else if (device.DeviceType == InputDevices.InputDeviceType.JoyConL ||
                device.DeviceType == InputDevices.InputDeviceType.JoyConR)
            {
            }
        }

        public bool CheckForSupportedDevice(HidDevice device, VidPidInfo metaInfo)
        {
            bool result = false;
            switch (metaInfo.inputDevType)
            {
                case InputDevices.InputDeviceType.DS4:
                    result = deviceOptions.DS4DeviceOpts.Enabled;
                    break;
                case InputDevices.InputDeviceType.DualSense:
                    result = deviceOptions.DualSenseOpts.Enabled;
                    break;
                case InputDevices.InputDeviceType.SwitchPro:
                    result = deviceOptions.SwitchProDeviceOpts.Enabled;
                    break;
                case InputDevices.InputDeviceType.JoyConL:
                case InputDevices.InputDeviceType.JoyConR:
                    result = deviceOptions.JoyConDeviceOpts.Enabled;
                    break;
                default:
                    break;
            }

            return result;
        }

        public void PrepareDS4DeviceInit(DS4Device device)
        {
            // Does nothing now
        }

        public CheckVirtualInfo CheckForVirtualDevice(string deviceInstanceId)
        {
            string temp = Global.GetDeviceProperty(deviceInstanceId,
                NativeMethods.DEVPKEY_Device_UINumber);

            CheckVirtualInfo info = new CheckVirtualInfo()
            {
                PropertyValue = temp,
                DeviceInstanceId = deviceInstanceId,
            };
            return info;
        }

        public void ShutDown()
        {
            DS4Devices.checkVirtualFunc = null;
            outputslotMan.ShutDown();
            OutputSlotPersist.WriteConfig(outputslotMan);

            outputKBMHandler.Disconnect();

            eventDispatcher.InvokeShutdown();
            eventDispatcher = null;

            eventDispatchThread.Join();
            eventDispatchThread = null;
        }

        private void DS4Devices_RequestElevation(RequestElevationArgs args)
        {
            // Launches an elevated child process to re-enable device
            ProcessStartInfo startInfo =
                new ProcessStartInfo(Global.exelocation);
            startInfo.Verb = "runas";
            startInfo.Arguments = "re-enabledevice " + args.InstanceId;
            startInfo.UseShellExecute = true;

            try
            {
                Process child = Process.Start(startInfo);
                if (!child.WaitForExit(30000))
                {
                    child.Kill();
                }
                else
                {
                    args.StatusCode = child.ExitCode;
                }
                child.Dispose();
            }
            catch { }
        }

        public void CheckHidHidePresence()
        {
            if (Global.hidHideInstalled)
            {
                LogDebug("HidHide control device found");
                using (HidHideAPIDevice hidHideDevice = new HidHideAPIDevice())
                {
                    if (!hidHideDevice.IsOpen())
                    {
                        return;
                    }

                    List<string> dosPaths = hidHideDevice.GetWhitelist();

                    int maxPathCheckLength = 512;
                    StringBuilder sb = new StringBuilder(maxPathCheckLength);
                    string driveLetter = Path.GetPathRoot(Global.exelocation).Replace("\\", "");
                    uint _ = NativeMethods.QueryDosDevice(driveLetter, sb, maxPathCheckLength);
                    //int error = Marshal.GetLastWin32Error();

                    string dosDrivePath = sb.ToString();
                    // Strip a possible \??\ prefix.
                    if (dosDrivePath.StartsWith(@"\??\"))
                    {
                        dosDrivePath = dosDrivePath.Remove(0, 4);
                    }

                    string partial = Global.exelocation.Replace(driveLetter, "");
                    // Need to trim starting '\\' from path2 or Path.Combine will
                    // treat it as an absolute path and only return path2
                    string realPath = Path.Combine(dosDrivePath, partial.TrimStart('\\'));
                    bool exists = dosPaths.Contains(realPath);
                    if (!exists)
                    {
                        LogDebug("DS4Windows not found in HidHide whitelist. Adding DS4Windows to list");
                        dosPaths.Add(realPath);
                        hidHideDevice.SetWhitelist(dosPaths);
                    }
                }
            }
        }

        public void LoadPermanentSlotsConfig()
        {
            OutputSlotPersist.ReadConfig(outputslotMan);
        }

        public void UpdateHidHideAttributes()
        {
            if (Global.hidHideInstalled)
            {
                hidDeviceHidingAffectedDevs.Clear();
                hidDeviceHidingExemptedDevs.Clear(); // No known equivalent in HidHide
                hidDeviceHidingForced = false; // No known equivalent in HidHide
                hidDeviceHidingEnabled = false;

                using (HidHideAPIDevice hidHideDevice = new HidHideAPIDevice())
                {
                    if (!hidHideDevice.IsOpen())
                    {
                        return;
                    }

                    bool active = hidHideDevice.GetActiveState();
                    List<string> instances = hidHideDevice.GetBlacklist();

                    hidDeviceHidingEnabled = active;
                    foreach (string instance in instances)
                    {
                        hidDeviceHidingAffectedDevs.Add(instance.ToUpper());
                    }
                }
            }
        }

        public void UpdateHidHiddenAttributes()
        {
            if (Global.hidHideInstalled)
            {
                UpdateHidHideAttributes();
            }
        }

        private bool CheckAffected(DS4Device dev)
        {
            bool result = false;
            if (dev != null && hidDeviceHidingEnabled)
            {
                string deviceInstanceId = DS4Devices.devicePathToInstanceId(dev.HidDevice.DevicePath);
                if (Global.hidHideInstalled)
                {
                    result = Global.CheckHidHideAffectedStatus(deviceInstanceId,
                        hidDeviceHidingAffectedDevs, hidDeviceHidingExemptedDevs, hidDeviceHidingForced);
                }
            }

            return result;
        }

        /// <summary>
        /// Obtain extra mappable controls not on a DS4 that should be added
        /// to the checked inputs list. Keeps Mapping class from having to check
        /// extra Switch Pro and JoyCon buttons for DS4 controllers
        /// </summary>
        /// <param name="dev">Instance of input device</param>
        /// <returns>List of extra controls to check in Mapping class</returns>
        private List<DS4Controls> GetKnownExtraButtons(DS4Device dev)
        {
            List<DS4Controls> result = new List<DS4Controls>();
            switch (dev.DeviceType)
            {
                case InputDevices.InputDeviceType.JoyConL:
                case InputDevices.InputDeviceType.JoyConR:
                    result.AddRange(new DS4Controls[] { DS4Controls.Capture, DS4Controls.SideL, DS4Controls.SideR });
                    break;
                case InputDevices.InputDeviceType.SwitchPro:
                    result.AddRange(new DS4Controls[] { DS4Controls.Capture });
                    break;
                default:
                    break;
            }

            return result;
        }

        private void ChangeExclusiveStatus(DS4Device dev)
        {
            if (Global.hidHideInstalled)
            {
                dev.CurrentExclusiveStatus = DS4Device.ExclusiveStatus.HidHideAffected;
            }
        }

        private void TestQueueBus(Action temp)
        {
            lock (busEvtQueueLock)
            {
                busEvtQueue.Enqueue(temp);
            }

            lock (busThrLck)
                Monitor.Pulse(busThrLck);
        }

        public void ChangeUDPStatus(bool state, bool openPort=true)
        {
            
            if (state && _udpServer == null)
            {
                udpChangeStatus = true;
                TestQueueBus(() =>
                {
                    _udpServer = new UdpServer(GetPadDetailForIdx);
                    if (openPort)
                    {
                        // Change thread affinity of object to have normal priority
                        Task.Run(() =>
                        {
                            var UDP_SERVER_PORT = Global.getUDPServerPortNum();
                            var UDP_SERVER_LISTEN_ADDRESS = Global.getUDPServerListenAddress();

                            try
                            {
                                _udpServer.Start(UDP_SERVER_PORT, UDP_SERVER_LISTEN_ADDRESS);
                                LogDebug($"UDP server listening on address {UDP_SERVER_LISTEN_ADDRESS} port {UDP_SERVER_PORT}");
                            }
                            catch (System.Net.Sockets.SocketException ex)
                            {
                                var errMsg = String.Format("Couldn't start UDP server on address {0}:{1}, outside applications won't be able to access pad data ({2})", UDP_SERVER_LISTEN_ADDRESS, UDP_SERVER_PORT, ex.SocketErrorCode);

                                LogDebug(errMsg, true);
                                AppLogger.LogToTray(errMsg, true, true);
                            }
                        }).Wait();
                    }

                    udpChangeStatus = false;
                });
            }
            else if (!state && _udpServer != null)
            {
                TestQueueBus(() =>
                {
                    udpChangeStatus = true;
                    _udpServer.Stop();
                    _udpServer = null;
                    AppLogger.LogToGui("Closed UDP server", false);
                    udpChangeStatus = false;

                    for (int i = 0; i < UdpServer.NUMBER_SLOTS; i++)
                    {
                        ResetUdpSmoothingFilters(i);
                    }
                });
            }
        }

        public void ChangeOSCListenerStatus(bool state)
        {
            if (state)
            {
                oscListener = new UDPListener(Global.getOSCServerPortNum(), callback: oscCallback);
                
                AppLogger.LogToGui("OSC LISTENER STARTED AT PORT: "+Global.getOSCServerPortNum(), false);
            }
            else
            {
                oscListener.Close();
                oscListener = null;
                AppLogger.LogToGui("OSC LISTENER STOPPED", false);
            }
        }

        public void ChangeOSCSenderStatus(bool state)
        {
            if (state)
            {
                AppLogger.LogToGui("OSC SENDER STARTED AT IP: "+ Global.getOSCSenderAddress()+" PORT: "+ Global.getOSCSenderPortNum(), false);
                oscSender = new UDPSender(Global.getOSCSenderAddress(), Global.getOSCSenderPortNum());
            }
            else
            {
                AppLogger.LogToGui("OSC SENDER STOPPED", false);
                if(oscSender == null) { return; }
                oscSender.Close();
                oscSender = null;
            }
        }

        public void ChangeMotionEventStatus(bool state)
        {
            IEnumerable<DS4Device> devices = DS4Devices.getDS4Controllers();
            if (state)
            {
                int i = 0;
                foreach (DS4Device dev in devices)
                {
                    int tempIdx = i;
                    dev.queueEvent(() =>
                    {
                        if (i < UdpServer.NUMBER_SLOTS)
                        {
                            PrepareDevUDPMotion(dev, tempIdx);
                        }
                    });

                    i++;
                }
            }
            else
            {
                foreach (DS4Device dev in devices)
                {
                    dev.queueEvent(() =>
                    {
                        if (dev.MotionEvent != null)
                        {
                            dev.Report -= dev.MotionEvent;
                            dev.MotionEvent = null;
                        }
                    });
                }
            }
        }

        private bool udpChangeStatus = false;
        public bool changingUDPPort = false;
        public async void UseUDPPort()
        {
            changingUDPPort = true;
            IEnumerable<DS4Device> devices = DS4Devices.getDS4Controllers();
            foreach (DS4Device dev in devices)
            {
                dev.queueEvent(() =>
                {
                    if (dev.MotionEvent != null)
                    {
                        dev.Report -= dev.MotionEvent;
                    }
                });
            }

            await Task.Delay(100);

            var UDP_SERVER_PORT = Global.getUDPServerPortNum();
            var UDP_SERVER_LISTEN_ADDRESS = Global.getUDPServerListenAddress();

            try
            {
                _udpServer.Start(UDP_SERVER_PORT, UDP_SERVER_LISTEN_ADDRESS);
                foreach (DS4Device dev in devices)
                {
                    dev.queueEvent(() =>
                    {
                        if (dev.MotionEvent != null)
                        {
                            dev.Report += dev.MotionEvent;
                        }
                    });
                }
                LogDebug($"UDP server listening on address {UDP_SERVER_LISTEN_ADDRESS} port {UDP_SERVER_PORT}");
            }
            catch (System.Net.Sockets.SocketException ex)
            {
                var errMsg = String.Format("Couldn't start UDP server on address {0}:{1}, outside applications won't be able to access pad data ({2})", UDP_SERVER_LISTEN_ADDRESS, UDP_SERVER_PORT, ex.SocketErrorCode);

                LogDebug(errMsg, true);
                AppLogger.LogToTray(errMsg, true, true);
            }

            changingUDPPort = false;
        }

        private void WarnExclusiveModeFailure(DS4Device device)
        {
            if (DS4Devices.isExclusiveMode && !device.isExclusive())
            {
                string message = DS4WinWPF.Properties.Resources.CouldNotOpenDS4.Replace("*Mac address*", device.getMacAddress()) + " " +
                    DS4WinWPF.Properties.Resources.QuitOtherPrograms;
                LogDebug(message, true);
                AppLogger.LogToTray(message, true);
            }
        }

        private void StartViGEm()
        {
            // Refresh internal ViGEmBus info
            Global.RefreshViGEmBusInfo();
            if (Global.IsRunningSupportedViGEmBus())
            {
                tempThread = new Thread(() =>
                {
                    try
                    {
                        vigemTestClient = new ViGEmClient();
                    }
                    catch {}
                });
                tempThread.Priority = ThreadPriority.AboveNormal;
                tempThread.IsBackground = true;
                tempThread.Start();
                while (tempThread.IsAlive)
                {
                    Thread.SpinWait(500);
                }
            }

            tempThread = null;
        }

        private void StopViGEm()
        {
            if (vigemTestClient != null)
            {
                vigemTestClient.Dispose();
                vigemTestClient = null;
            }
        }

        public void AssignInitialDevices()
        {
            foreach(OutSlotDevice slotDevice in outputslotMan.OutputSlots)
            {
                if (slotDevice.CurrentReserveStatus ==
                    OutSlotDevice.ReserveStatus.Permanent)
                {
                    OutputDevice outDevice = EstablishOutDevice(0, slotDevice.PermanentType);
                    outputslotMan.DeferredPlugin(outDevice, -1, outputDevices, slotDevice.PermanentType);
                }
            }
            /*OutSlotDevice slotDevice =
                outputslotMan.FindExistUnboundSlotType(OutContType.X360);

            if (slotDevice == null)
            {
                slotDevice = outputslotMan.FindOpenSlot();
                slotDevice.CurrentReserveStatus = OutSlotDevice.ReserveStatus.Permanent;
                slotDevice.PermanentType = OutContType.X360;
                OutputDevice outDevice = EstablishOutDevice(0, OutContType.X360);
                Xbox360OutDevice tempXbox = outDevice as Xbox360OutDevice;
                outputslotMan.DeferredPlugin(tempXbox, -1, outputDevices, OutContType.X360);
            }
            */

            /*slotDevice = outputslotMan.FindExistUnboundSlotType(OutContType.X360);
            if (slotDevice == null)
            {
                slotDevice = outputslotMan.FindOpenSlot();
                slotDevice.CurrentReserveStatus = OutSlotDevice.ReserveStatus.Permanent;
                slotDevice.DesiredType = OutContType.X360;
                OutputDevice outDevice = EstablishOutDevice(1, OutContType.X360);
                Xbox360OutDevice tempXbox = outDevice as Xbox360OutDevice;
                outputslotMan.DeferredPlugin(tempXbox, 1, outputDevices);
            }*/
        }

        private OutputDevice EstablishOutDevice(int index, OutContType contType)
        {
            OutputDevice temp = null;
            temp = outputslotMan.AllocateController(contType, vigemTestClient);
            return temp;
        }

        public void EstablishOutFeedback(int index, OutContType contType,
            OutputDevice outDevice, DS4Device device)
        {
            int devIndex = index;

            if (contType == OutContType.X360)
            {
                Xbox360OutDevice tempXbox = outDevice as Xbox360OutDevice;
                Nefarius.ViGEm.Client.Targets.Xbox360FeedbackReceivedEventHandler p = (sender, args) =>
                {
                    //Console.WriteLine("Rumble ({0}, {1}) {2}",
                    //    args.LargeMotor, args.SmallMotor, DateTime.Now.ToString("hh:mm:ss.FFFF"));
                    SetDevRumble(device, args.LargeMotor, args.SmallMotor, devIndex);
                };
                tempXbox.cont.FeedbackReceived += p;
                tempXbox.forceFeedbacksDict.Add(index, p);
            }
            //else if (contType == OutContType.DS4)
            //{
            //    DS4OutDevice tempDS4 = outDevice as DS4OutDevice;
            //    LightbarSettingInfo deviceLightbarSettingsInfo = Global.LightbarSettingsInfo[devIndex];

            //    Nefarius.ViGEm.Client.Targets.DualShock4FeedbackReceivedEventHandler p = (sender, args) =>
            //    {
            //        bool useRumble = false; bool useLight = false;
            //        byte largeMotor = args.LargeMotor;
            //        byte smallMotor = args.SmallMotor;
            //        //SetDevRumble(device, largeMotor, smallMotor, devIndex);
            //        DS4Color color = new DS4Color(args.LightbarColor.Red,
            //                args.LightbarColor.Green,
            //                args.LightbarColor.Blue);

            //        //Console.WriteLine("IN EVENT");
            //        //Console.WriteLine("Rumble ({0}, {1}) | Light ({2}, {3}, {4}) {5}",
            //        //    largeMotor, smallMotor, color.red, color.green, color.blue, DateTime.Now.ToString("hh:mm:ss.FFFF"));

            //        if (largeMotor != 0 || smallMotor != 0)
            //        {
            //            useRumble = true;
            //        }

            //        // Let games to control lightbar only when the mode is Passthru (otherwise DS4Windows controls the light)
            //        if (deviceLightbarSettingsInfo.Mode == LightbarMode.Passthru && (color.red != 0 || color.green != 0 || color.blue != 0))
            //        {
            //            useLight = true;
            //        }

            //        if (!useRumble && !useLight)
            //        {
            //            //Console.WriteLine("Fallback");
            //            if (device.LeftHeavySlowRumble != 0 || device.RightLightFastRumble != 0)
            //            {
            //                useRumble = true;
            //            }
            //            else if (deviceLightbarSettingsInfo.Mode == LightbarMode.Passthru &&
            //                (device.LightBarColor.red != 0 ||
            //                device.LightBarColor.green != 0 ||
            //                device.LightBarColor.blue != 0))
            //            {
            //                useLight = true;
            //            }
            //        }

            //        if (useRumble)
            //        {
            //            //Console.WriteLine("Perform rumble");
            //            SetDevRumble(device, largeMotor, smallMotor, devIndex);
            //        }

            //        if (useLight)
            //        {
            //            //Console.WriteLine("Change lightbar color");
            //            /*DS4HapticState haptics = new DS4HapticState
            //            {
            //                LightBarColor = color,
            //            };
            //            device.SetHapticState(ref haptics);
            //            */

            //            DS4LightbarState lightState = new DS4LightbarState
            //            {
            //                LightBarColor = color,
            //            };
            //            device.SetLightbarState(ref lightState);
            //        }

            //        //Console.WriteLine();
            //    };

            //    tempDS4.cont.FeedbackReceived += p;
            //    tempDS4.forceFeedbacksDict.Add(index, p);
            //}
        }

        public void RemoveOutFeedback(OutContType contType, OutputDevice outDevice, int inIdx)
        {
            if (contType == OutContType.X360)
            {
                Xbox360OutDevice tempXbox = outDevice as Xbox360OutDevice;
                tempXbox.RemoveFeedback(inIdx);
                //tempXbox.cont.FeedbackReceived -= tempXbox.forceFeedbackCall;
                //tempXbox.forceFeedbackCall = null;
            }
            //else if (contType == OutContType.DS4)
            //{
            //    DS4OutDevice tempDS4 = outDevice as DS4OutDevice;
            //    tempDS4.RemoveFeedback(inIdx);
            //    //tempDS4.cont.FeedbackReceived -= tempDS4.forceFeedbackCall;
            //    //tempDS4.forceFeedbackCall = null;
            //}
        }

        public void AttachNewUnboundOutDev(OutContType contType)
        {
            OutSlotDevice slotDevice = outputslotMan.FindOpenSlot();
            if (slotDevice != null &&
                slotDevice.CurrentAttachedStatus == OutSlotDevice.AttachedStatus.UnAttached)
            {
                OutputDevice outDevice = EstablishOutDevice(-1, contType);
                outputslotMan.DeferredPlugin(outDevice, -1, outputDevices, contType);
                LogDebug($"Plugging virtual {contType} Controller");
            }
        }

        public void AttachUnboundOutDev(OutSlotDevice slotDevice, OutContType contType)
        {
            if (slotDevice.CurrentAttachedStatus == OutSlotDevice.AttachedStatus.UnAttached &&
                slotDevice.CurrentInputBound == OutSlotDevice.InputBound.Unbound)
            {
                OutputDevice outDevice = EstablishOutDevice(-1, contType);
                outputslotMan.DeferredPlugin(outDevice, -1, outputDevices, contType);
                LogDebug($"Plugging virtual {contType} Controller");
            }
        }

        public void DetachUnboundOutDev(OutSlotDevice slotDevice)
        {
            if (slotDevice.CurrentInputBound == OutSlotDevice.InputBound.Unbound)
            {
                OutputDevice dev = slotDevice.OutputDevice;
                string tempType = dev.GetDeviceType();
                slotDevice.CurrentInputBound = OutSlotDevice.InputBound.Unbound;
                outputslotMan.DeferredRemoval(dev, -1, outputDevices, false);
                LogDebug($"Unplugging virtual {tempType} Controller");
            }
        }

        public void PluginOutDev(int index, DS4Device device)
        {
            OutContType contType = Global.OutContType[index];

            OutSlotDevice slotDevice = null;
            if (!getDInputOnly(index))
            {
                slotDevice = outputslotMan.FindExistUnboundSlotType(contType);
            }

            if (useDInputOnly[index])
            {
                bool success = false;
                if (contType == OutContType.X360)
                {
                    activeOutDevType[index] = OutContType.X360;

                    if (slotDevice == null)
                    {
                        slotDevice = outputslotMan.FindOpenSlot();
                        if (slotDevice != null)
                        {
                            Xbox360OutDevice tempXbox = EstablishOutDevice(index, OutContType.X360)
                            as Xbox360OutDevice;
                            //outputDevices[index] = tempXbox;
                            
                            // Enable ViGem feedback callback handler only if lightbar/rumble data output is enabled (if those are disabled then no point enabling ViGem callback handler call)
                            if (Global.EnableOutputDataToDS4[index])
                            {
                                EstablishOutFeedback(index, OutContType.X360, tempXbox, device);

                                if (device.JointDeviceSlotNumber != -1)
                                {
                                    DS4Device tempDS4Device = DS4Controllers[device.JointDeviceSlotNumber];
                                    if (tempDS4Device != null)
                                    {
                                        EstablishOutFeedback(device.JointDeviceSlotNumber, OutContType.X360, tempXbox, tempDS4Device);
                                    }
                                }
                            }

                            outputslotMan.DeferredPlugin(tempXbox, index, outputDevices, contType);
                            //slotDevice.CurrentInputBound = OutSlotDevice.InputBound.Bound;

                            LogDebug("Plugging in virtual X360 Controller");
                            success = true;
                        }
                        else
                        {
                            LogDebug("Failed. No open output slot found");
                        }
                    }
                    else
                    {
                        slotDevice.CurrentInputBound = OutSlotDevice.InputBound.Bound;
                        Xbox360OutDevice tempXbox = slotDevice.OutputDevice as Xbox360OutDevice;

                        // Enable ViGem feedback callback handler only if lightbar/rumble data output is enabled (if those are disabled then no point enabling ViGem callback handler call)
                        if (Global.EnableOutputDataToDS4[index])
                        {
                            EstablishOutFeedback(index, OutContType.X360, tempXbox, device);

                            if (device.JointDeviceSlotNumber != -1)
                            {
                                DS4Device tempDS4Device = DS4Controllers[device.JointDeviceSlotNumber];
                                if (tempDS4Device != null)
                                {
                                    EstablishOutFeedback(device.JointDeviceSlotNumber, OutContType.X360, tempXbox, tempDS4Device);
                                }
                            }
                        }

                        outputDevices[index] = tempXbox;
                        slotDevice.CurrentType = contType;
                        success = true;
                    }

                    if (success)
                    {
                        LogDebug($"Associate X360 Controller in{(slotDevice.PermanentType != OutContType.None ? " permanent" : "")} slot #{slotDevice.Index + 1} for input {device.DisplayName} controller #{index + 1}");
                    }

                    //tempXbox.Connect();
                    //LogDebug("X360 Controller #" + (index + 1) + " connected");
                }
                else if (contType == OutContType.DS4)
                {
                    activeOutDevType[index] = OutContType.DS4;
                    if (slotDevice == null)
                    {
                        slotDevice = outputslotMan.FindOpenSlot();
                        if (slotDevice != null)
                        {
                            DS4OutDevice tempDS4 = EstablishOutDevice(index, OutContType.DS4)
                            as DS4OutDevice;

                            // Enable ViGem feedback callback handler only if DS4 lightbar/rumble data output is enabled (if those are disabled then no point enabling ViGem callback handler call)
                            if (Global.EnableOutputDataToDS4[index])
                            {
                                EstablishOutFeedback(index, OutContType.DS4, tempDS4, device);

                                if (device.JointDeviceSlotNumber != -1)
                                {
                                    DS4Device tempDS4Device = DS4Controllers[device.JointDeviceSlotNumber];
                                    if (tempDS4Device != null)
                                    {
                                        EstablishOutFeedback(device.JointDeviceSlotNumber, OutContType.DS4, tempDS4, tempDS4Device);
                                    }
                                }
                            }

                            outputslotMan.DeferredPlugin(tempDS4, index, outputDevices, contType);
                            //slotDevice.CurrentInputBound = OutSlotDevice.InputBound.Bound;

                            LogDebug("Plugging in virtual DS4 Controller");
                            success = true;
                        }
                        else
                        {
                            LogDebug("Failed. No open output slot found");
                        }
                    }
                    else
                    {
                        slotDevice.CurrentInputBound = OutSlotDevice.InputBound.Bound;
                        DS4OutDevice tempDS4 = slotDevice.OutputDevice as DS4OutDevice;

                        // Enable ViGem feedback callback handler only if lightbar/rumble data output is enabled (if those are disabled then no point enabling ViGem callback handler call)
                        if (Global.EnableOutputDataToDS4[index])
                        {
                            EstablishOutFeedback(index, OutContType.DS4, tempDS4, device);

                            if (device.JointDeviceSlotNumber != -1)
                            {
                                DS4Device tempDS4Device = DS4Controllers[device.JointDeviceSlotNumber];
                                if (tempDS4Device != null)
                                {
                                    EstablishOutFeedback(device.JointDeviceSlotNumber, OutContType.DS4, tempDS4, tempDS4Device);
                                }
                            }
                        }

                        outputDevices[index] = tempDS4;
                        slotDevice.CurrentType = contType;
                        success = true;
                    }

                    if (success)
                    {
                        LogDebug($"Associate DS4 Controller in{(slotDevice.PermanentType != OutContType.None ? " permanent" : "")} slot #{slotDevice.Index + 1} for input {device.DisplayName} controller #{index + 1}");
                    }

                    //DS4OutDevice tempDS4 = new DS4OutDevice(vigemTestClient);
                    //DS4OutDevice tempDS4 = outputslotMan.AllocateController(OutContType.DS4, vigemTestClient)
                    //    as DS4OutDevice;
                    //outputDevices[index] = tempDS4;

                    //tempDS4.Connect();
                    //LogDebug("DS4 Controller #" + (index + 1) + " connected");
                }

                if (success)
                {
                    useDInputOnly[index] = false;
                }
            }
        }

        public void UnplugOutDev(int index, DS4Device device, bool immediate = false, bool force = false)
        {
            if (!useDInputOnly[index])
            {
                //OutContType contType = Global.OutContType[index];
                OutputDevice dev = outputDevices[index];
                OutSlotDevice slotDevice = outputslotMan.GetOutSlotDevice(dev);
                if (dev != null && slotDevice != null)
                {
                    string tempType = dev.GetDeviceType();
                    LogDebug($"Disassociate {tempType} Controller from{(slotDevice.CurrentReserveStatus == OutSlotDevice.ReserveStatus.Permanent ? " permanent" : "")} slot #{slotDevice.Index+1} for input {device.DisplayName} controller #{index + 1}", false);

                    OutContType currentType = activeOutDevType[index];
                    outputDevices[index] = null;
                    activeOutDevType[index] = OutContType.None;
                    if ((slotDevice.CurrentAttachedStatus == OutSlotDevice.AttachedStatus.Attached &&
                        slotDevice.CurrentReserveStatus == OutSlotDevice.ReserveStatus.Dynamic) || force)
                    {
                        //slotDevice.CurrentInputBound = OutSlotDevice.InputBound.Unbound;
                        outputslotMan.DeferredRemoval(dev, index, outputDevices, immediate);
                        LogDebug($"Unplugging virtual {tempType} Controller");
                    }
                    else if (slotDevice.CurrentAttachedStatus == OutSlotDevice.AttachedStatus.Attached)
                    {
                        slotDevice.CurrentInputBound = OutSlotDevice.InputBound.Unbound;
                        dev.ResetState();
                        dev.RemoveFeedbacks();
                        //RemoveOutFeedback(currentType, dev);
                    }
                    //dev.Disconnect();
                    //LogDebug(tempType + " Controller # " + (index + 1) + " unplugged");
                }

                useDInputOnly[index] = true;
            }
        }

        public bool Start(bool showlog = true)
        {
            inServiceTask = true;
            StartViGEm();
            if (vigemTestClient != null)
            //if (x360Bus.Open() && x360Bus.Start())
            {
                if (showlog)
                    LogDebug(DS4WinWPF.Properties.Resources.Starting);

                LogDebug($"Using output KB+M handler: {DS4Windows.Global.outputKBMHandler.GetFullDisplayName()}");
                LogDebug($"Connection to ViGEmBus {Global.vigembusVersion} established");

                DS4Devices.isExclusiveMode = getUseExclusiveMode(); //Re-enable Exclusive Mode

                UpdateHidHiddenAttributes();

                if (showlog)
                {
                    LogDebug(DS4WinWPF.Properties.Resources.SearchingController);
                    LogDebug(DS4Devices.isExclusiveMode ? DS4WinWPF.Properties.Resources.UsingExclusive : DS4WinWPF.Properties.Resources.UsingShared);
                }

                if(isUsingOSCServer() && oscListener == null)
                {
                    ChangeOSCListenerStatus(true);
                }

                if(isUsingOSCSender() && oscSender == null)
                {
                    ChangeOSCSenderStatus(true);
                }

                if (isUsingUDPServer() && _udpServer == null)
                {
                    ChangeUDPStatus(true, false);
                    while (udpChangeStatus == true)
                    {
                        Thread.SpinWait(500);
                    }
                }

                try
                {
                    loopControllers = true;
                    AssignInitialDevices();

                    eventDispatcher.Invoke(() =>
                    {
                        DS4Devices.findControllers();
                    });

                    IEnumerable<DS4Device> devices = DS4Devices.getDS4Controllers();
                    int numControllers = new List<DS4Device>(devices).Count;
                    activeControllers = numControllers;
                    //int ind = 0;
                    DS4LightBar.defaultLight = false;
                    //foreach (DS4Device device in devices)
                    //for (int i = 0, devCount = devices.Count(); i < devCount; i++)
                    int i = 0;
                    InputDevices.JoyConDevice tempPrimaryJoyDev = null;
                    for (var devEnum = devices.GetEnumerator(); devEnum.MoveNext() && loopControllers; i++)
                    {
                        DS4Device device = devEnum.Current;
                        if (showlog)
                            LogDebug(DS4WinWPF.Properties.Resources.FoundController + " " + device.getMacAddress() + " (" + device.getConnectionType() + ") (" +
                                device.DisplayName + ")");

                        if (hidDeviceHidingEnabled && CheckAffected(device))
                        {
                            //device.CurrentExclusiveStatus = DS4Device.ExclusiveStatus.HidGuardAffected;
                            ChangeExclusiveStatus(device);
                        }

                        Task task = new Task(() => { Thread.Sleep(5); WarnExclusiveModeFailure(device); });
                        task.Start();

                        PrepareDS4DeviceSettingHooks(device);

                        if (deviceOptions.JoyConDeviceOpts.LinkedMode == JoyConDeviceOptions.LinkMode.Joined)
                        {
                            if ((device.DeviceType == InputDevices.InputDeviceType.JoyConL ||
                                device.DeviceType == InputDevices.InputDeviceType.JoyConR) && device.PerformStateMerge)
                            {
                                if (tempPrimaryJoyDev == null)
                                {
                                    tempPrimaryJoyDev = device as InputDevices.JoyConDevice;
                                }
                                else
                                {
                                    InputDevices.JoyConDevice currentJoyDev = device as InputDevices.JoyConDevice;
                                    tempPrimaryJoyDev.JointDevice = currentJoyDev;
                                    currentJoyDev.JointDevice = tempPrimaryJoyDev;

                                    tempPrimaryJoyDev.JointState = currentJoyDev.JointState;

                                    InputDevices.JoyConDevice parentJoy = tempPrimaryJoyDev;
                                    tempPrimaryJoyDev.Removal += (sender, args) => { currentJoyDev.JointDevice = null; };
                                    currentJoyDev.Removal += (sender, args) => { parentJoy.JointDevice = null; };

                                    tempPrimaryJoyDev = null;
                                }
                            }
                        }

                        DS4Controllers[i] = device;
                        device.DeviceSlotNumber = i;

                        Global.RefreshExtrasButtons(i, GetKnownExtraButtons(device));
                        Global.LoadControllerConfigs(device);
                        device.LoadStoreSettings();
                        device.CheckControllerNumDeviceSettings(numControllers);

                        slotManager.AddController(device, i);
                        if (isUsingOSCSender())
                        {
                            oscSender.Send(new SharpOSC.OscMessage("/ds4windows/monitor/" + i + "/plug", 1));
                        }
                        device.Removal += this.On_DS4Removal;
                        device.Removal += DS4Devices.On_Removal;
                        device.SyncChange += this.On_SyncChange;
                        device.SyncChange += DS4Devices.UpdateSerial;
                        device.SerialChange += this.On_SerialChange;
                        device.ChargingChanged += CheckQuickCharge;

                        touchPad[i] = new Mouse(i, device);
                        bool profileLoaded = false;
                        bool useAutoProfile = useTempProfile[i];
                        if (!useAutoProfile)
                        {
                            if (device.isValidSerial() && containsLinkedProfile(device.getMacAddress()))
                            {
                                ProfilePath[i] = getLinkedProfile(device.getMacAddress());
                                Global.linkedProfileCheck[i] = true;
                            }
                            else
                            {
                                ProfilePath[i] = OlderProfilePath[i];
                                Global.linkedProfileCheck[i] = false;
                            }

                            profileLoaded = LoadProfile(i, false, this, false, false);
                        }

                        if (profileLoaded || useAutoProfile)
                        {
                            device.LightBarColor = getMainColor(i);

                            if (!getDInputOnly(i) && device.isSynced())
                            {
                                if (device.PrimaryDevice)
                                {
                                    PluginOutDev(i, device);
                                }
                                else if (device.JointDeviceSlotNumber != DS4Device.DEFAULT_JOINT_SLOT_NUMBER)
                                {
                                    int otherIdx = device.JointDeviceSlotNumber;
                                    OutputDevice tempOutDev = outputDevices[otherIdx];
                                    if (tempOutDev != null)
                                    {
                                        OutContType tempConType = activeOutDevType[otherIdx];
                                        EstablishOutFeedback(i, tempConType, tempOutDev, device);
                                        outputDevices[i] = tempOutDev;
                                        Global.activeOutDevType[i] = tempConType;
                                    }
                                }
                            }
                            else
                            {
                                useDInputOnly[i] = true;
                                Global.activeOutDevType[i] = OutContType.None;
                            }

                            if (device.PrimaryDevice && device.OutputMapGyro)
                            {
                                TouchPadOn(i, device);
                            }
                            else if (device.JointDeviceSlotNumber != DS4Device.DEFAULT_JOINT_SLOT_NUMBER)
                            {
                                int otherIdx = device.JointDeviceSlotNumber;
                                DS4Device tempDev = DS4Controllers[otherIdx];
                                if (tempDev != null)
                                {
                                    int mappedIdx = tempDev.PrimaryDevice ? otherIdx : i;
                                    DS4Device gyroDev = device.OutputMapGyro ? device : (tempDev.OutputMapGyro ? tempDev : null);
                                    if (gyroDev != null)
                                    {
                                        TouchPadOn(mappedIdx, gyroDev);
                                    }
                                }
                            }

                            CheckProfileOptions(i, device, true);
                            SetupInitialHookEvents(i, device);
                        }

                        int tempIdx = i;
                        device.Report += (sender, e) =>
                        {
                            this.On_Report(sender, e, tempIdx);
                        };

                        if (_udpServer != null && i < UdpServer.NUMBER_SLOTS)
                        {
                            PrepareDevUDPMotion(device, tempIdx);
                        }

                        device.StartUpdate();
                        //string filename = ProfilePath[ind];
                        //ind++;

                        if (i >= CURRENT_DS4_CONTROLLER_LIMIT) // out of Xinput devices!
                            break;
                    }
                }
                catch (Exception e)
                {
                    LogDebug(e.Message, true);
                    AppLogger.LogToTray(e.Message, true);
                }

                running = true;

                if (_udpServer != null)
                {
                    //var UDP_SERVER_PORT = 26760;
                    var UDP_SERVER_PORT = Global.getUDPServerPortNum();
                    var UDP_SERVER_LISTEN_ADDRESS = Global.getUDPServerListenAddress();

                    try
                    {
                        _udpServer.Start(UDP_SERVER_PORT, UDP_SERVER_LISTEN_ADDRESS);
                        LogDebug($"UDP server listening on address {UDP_SERVER_LISTEN_ADDRESS} port {UDP_SERVER_PORT}");
                    }
                    catch (System.Net.Sockets.SocketException ex)
                    {
                        var errMsg = String.Format("Couldn't start UDP server on address {0}:{1}, outside applications won't be able to access pad data ({2})", UDP_SERVER_LISTEN_ADDRESS, UDP_SERVER_PORT, ex.SocketErrorCode);

                        LogDebug(errMsg, true);
                        AppLogger.LogToTray(errMsg, true, true);
                    }
                }
            }
            else
            {
                string logMessage = string.Empty;
                if (!vigemInstalled)
                {
                    logMessage = "ViGEmBus is not installed";
                }
                else if (!Global.IsRunningSupportedViGEmBus())
                {
                    logMessage = string.Format("Unsupported ViGEmBus found ({0}). Please install at least ViGEmBus 1.17.333.0", Global.vigembusVersion);
                }
                else
                {
                    logMessage = "Could not connect to ViGEmBus. Please check the status of the System device in Device Manager and if Visual C++ 2017 Redistributable is installed.";
                }

                LogDebug(logMessage);
                AppLogger.LogToTray(logMessage);
            }

            inServiceTask = false;
            runHotPlug = true;
            ServiceStarted?.Invoke(this, EventArgs.Empty);
            RunningChanged?.Invoke(this, EventArgs.Empty);
            return true;
        }

        private void PrepareDevUDPMotion(DS4Device device, int index)
        {
            int tempIdx = index;
            DS4Device.ReportHandler<EventArgs> tempEvnt = (sender, args) =>
            {
                DualShockPadMeta padDetail = new DualShockPadMeta();
                GetPadDetailForIdx(tempIdx, ref padDetail);
                DS4State stateForUdp = TempState[tempIdx];

                CurrentState[tempIdx].CopyTo(stateForUdp);
                if (Global.IsUsingUDPServerSmoothing())
                {
                    if (stateForUdp.elapsedTime == 0)
                    {
                        // No timestamp was found. Exit out of routine
                        return;
                    }

                    double rate = 1.0 / stateForUdp.elapsedTime;
                    OneEuroFilter3D accelFilter = udpEuroPairAccel[tempIdx];
                    stateForUdp.Motion.accelXG = accelFilter.axis1Filter.Filter(stateForUdp.Motion.accelXG, rate);
                    stateForUdp.Motion.accelYG = accelFilter.axis2Filter.Filter(stateForUdp.Motion.accelYG, rate);
                    stateForUdp.Motion.accelZG = accelFilter.axis3Filter.Filter(stateForUdp.Motion.accelZG, rate);

                    OneEuroFilter3D gyroFilter = udpEuroPairGyro[tempIdx];
                    stateForUdp.Motion.angVelYaw = gyroFilter.axis1Filter.Filter(stateForUdp.Motion.angVelYaw, rate);
                    stateForUdp.Motion.angVelPitch = gyroFilter.axis2Filter.Filter(stateForUdp.Motion.angVelPitch, rate);
                    stateForUdp.Motion.angVelRoll = gyroFilter.axis3Filter.Filter(stateForUdp.Motion.angVelRoll, rate);
                }

                _udpServer.NewReportIncoming(ref padDetail, stateForUdp, udpOutBuffers[tempIdx]);
            };

            device.MotionEvent = tempEvnt;
            device.Report += tempEvnt;
        }

        private void CheckQuickCharge(object sender, EventArgs e)
        {
            DS4Device device = sender as DS4Device;
            if (device.ConnectionType == ConnectionType.BT && getQuickCharge() &&
                device.Charging)
            {
                // Set disconnect flag here. Later Hotplug event will check
                // for presence of flag and remove the device then
                device.ReadyQuickChargeDisconnect = true;
            }
        }

        public void PrepareAbort()
        {
            for (int i = 0, arlength = DS4Controllers.Length; i < arlength; i++)
            {
                DS4Device tempDevice = DS4Controllers[i];
                if (tempDevice != null)
                {
                   tempDevice.PrepareAbort();
                }
            }
        }

        public bool Stop(bool showlog = true, bool immediateUnplug = false)
        {
            if (running)
            {
                running = false;
                runHotPlug = false;
                inServiceTask = true;
                PreServiceStop?.Invoke(this, EventArgs.Empty);

                if (showlog)
                    LogDebug(DS4WinWPF.Properties.Resources.StoppingX360);

                LogDebug("Closing connection to ViGEmBus");

                bool anyUnplugged = false;
                for (int i = 0, arlength = DS4Controllers.Length; i < arlength; i++)
                {
                    DS4Device tempDevice = DS4Controllers[i];
                    if (tempDevice != null)
                    {
                        if ((DCBTatStop && !tempDevice.isCharging()) || suspending)
                        {
                            if (tempDevice.getConnectionType() == ConnectionType.BT)
                            {
                                tempDevice.StopUpdate();
                                tempDevice.DisconnectBT(true);
                            }
                            else if (tempDevice.getConnectionType() == ConnectionType.SONYWA)
                            {
                                tempDevice.StopUpdate();
                                tempDevice.DisconnectDongle(true);
                            }
                            else
                            {
                                tempDevice.StopUpdate();
                            }
                        }
                        else
                        {
                            DS4LightBar.forcelight[i] = false;
                            DS4LightBar.forcedFlash[i] = 0;
                            DS4LightBar.defaultLight = true;
                            DS4LightBar.updateLightBar(DS4Controllers[i], i);
                            tempDevice.IsRemoved = true;
                            tempDevice.StopUpdate();
                            DS4Devices.RemoveDevice(tempDevice);
                            Thread.Sleep(50);
                        }

                        CurrentState[i].Battery = PreviousState[i].Battery = 0; // Reset for the next connection's initial status change.
                        OutputDevice tempout = outputDevices[i];
                        if (tempout != null)
                        {
                            UnplugOutDev(i, tempDevice, immediate: immediateUnplug, force: true);
                            anyUnplugged = true;
                        }

                        //outputDevices[i] = null;
                        //useDInputOnly[i] = true;
                        //Global.activeOutDevType[i] = OutContType.None;
                        useDInputOnly[i] = true;
                        DS4Controllers[i] = null;
                        oscState[i] = new DS4State();
                        touchPad[i] = null;
                        lag[i] = false;
                        inWarnMonitor[i] = false;
                    }
                }

                if (showlog)
                    LogDebug(DS4WinWPF.Properties.Resources.StoppingDS4);

                DS4Devices.stopControllers();
                slotManager.ClearControllerList();

                if(oscListener != null)
                {
                    ChangeOSCListenerStatus(false);
                }

                if(oscSender != null)
                {
                    ChangeOSCSenderStatus(false);
                }

                if (_udpServer != null)
                {
                    ChangeUDPStatus(false);
                }

                if (showlog)
                    LogDebug(DS4WinWPF.Properties.Resources.StoppedDS4Windows);

                while (outputslotMan.RunningQueue)
                {
                    Thread.SpinWait(500);
                }
                outputslotMan.Stop(true);

                if (anyUnplugged)
                {
                    Thread.Sleep(OutputSlotManager.DELAY_TIME);
                }

                StopViGEm();
                inServiceTask = false;
                activeControllers = 0;
            }

            runHotPlug = false;
            ServiceStopped?.Invoke(this, EventArgs.Empty);
            RunningChanged?.Invoke(this, EventArgs.Empty);
            return true;
        }

        public bool HotPlug()
        {
            if (running)
            {
                inServiceTask = true;
                loopControllers = true;
                eventDispatcher.Invoke(() =>
                {
                    DS4Devices.findControllers();
                });

                IEnumerable<DS4Device> devices = DS4Devices.getDS4Controllers();
                int numControllers = new List<DS4Device>(devices).Count;
                activeControllers = numControllers;
                //foreach (DS4Device device in devices)
                //for (int i = 0, devlen = devices.Count(); i < devlen; i++)
                InputDevices.JoyConDevice tempPrimaryJoyDev = null;
                InputDevices.JoyConDevice tempSecondaryJoyDev = null;

                if (deviceOptions.JoyConDeviceOpts.LinkedMode == JoyConDeviceOptions.LinkMode.Joined)
                {
                    tempPrimaryJoyDev = devices.Where(d =>
                        (d.DeviceType == InputDevices.InputDeviceType.JoyConL || d.DeviceType == InputDevices.InputDeviceType.JoyConR)
                         && d.PrimaryDevice && d.JointDeviceSlotNumber == -1).FirstOrDefault() as InputDevices.JoyConDevice;

                    tempSecondaryJoyDev = devices.Where(d =>
                        (d.DeviceType == InputDevices.InputDeviceType.JoyConL || d.DeviceType == InputDevices.InputDeviceType.JoyConR)
                        && !d.PrimaryDevice && d.JointDeviceSlotNumber == -1).FirstOrDefault() as InputDevices.JoyConDevice;
                }

                for (var devEnum = devices.GetEnumerator(); devEnum.MoveNext() && loopControllers;)
                {
                    DS4Device device = devEnum.Current;

                    if (device.isDisconnectingStatus())
                        continue;

                    if (((Func<bool>)delegate
                    {
                        for (Int32 Index = 0, arlength = DS4Controllers.Length; Index < arlength; Index++)
                        {
                            if (DS4Controllers[Index] != null &&
                                DS4Controllers[Index].getMacAddress() == device.getMacAddress())
                            {
                                device.CheckControllerNumDeviceSettings(numControllers);
                                return true;
                            }
                        }

                        return false;
                    })())
                    {
                        continue;
                    }

                    for (Int32 Index = 0, arlength = DS4Controllers.Length; Index < arlength && Index < CURRENT_DS4_CONTROLLER_LIMIT; Index++)
                    {
                        if (DS4Controllers[Index] == null)
                        {
                            //LogDebug(DS4WinWPF.Properties.Resources.FoundController + device.getMacAddress() + " (" + device.getConnectionType() + ")");
                            LogDebug(DS4WinWPF.Properties.Resources.FoundController + " " + device.getMacAddress() + " (" + device.getConnectionType() + ") (" +
                                device.DisplayName + ")");

                            if (hidDeviceHidingEnabled && CheckAffected(device))
                            {
                                //device.CurrentExclusiveStatus = DS4Device.ExclusiveStatus.HidGuardAffected;
                                ChangeExclusiveStatus(device);
                            }

                            Task task = new Task(() => { Thread.Sleep(5); WarnExclusiveModeFailure(device); });
                            task.Start();

                            PrepareDS4DeviceSettingHooks(device);

                            if (deviceOptions.JoyConDeviceOpts.LinkedMode == JoyConDeviceOptions.LinkMode.Joined)
                            {
                                if ((device.DeviceType == InputDevices.InputDeviceType.JoyConL ||
                                    device.DeviceType == InputDevices.InputDeviceType.JoyConR) && device.PerformStateMerge)
                                {
                                    if (device.PrimaryDevice &&
                                        tempSecondaryJoyDev != null)
                                    {
                                        InputDevices.JoyConDevice currentJoyDev = device as InputDevices.JoyConDevice;
                                        tempSecondaryJoyDev.JointDevice = currentJoyDev;
                                        currentJoyDev.JointDevice = tempSecondaryJoyDev;

                                        tempSecondaryJoyDev.JointState = currentJoyDev.JointState;

                                        InputDevices.JoyConDevice secondaryJoy = tempSecondaryJoyDev;
                                        secondaryJoy.Removal += (sender, args) => { currentJoyDev.JointDevice = null; };
                                        currentJoyDev.Removal += (sender, args) => { secondaryJoy.JointDevice = null; };

                                        tempSecondaryJoyDev = null;
                                        tempPrimaryJoyDev = null;
                                    }
                                    else if (!device.PrimaryDevice &&
                                        tempPrimaryJoyDev != null)
                                    {
                                        InputDevices.JoyConDevice currentJoyDev = device as InputDevices.JoyConDevice;
                                        tempPrimaryJoyDev.JointDevice = currentJoyDev;
                                        currentJoyDev.JointDevice = tempPrimaryJoyDev;

                                        tempPrimaryJoyDev.JointState = currentJoyDev.JointState;

                                        InputDevices.JoyConDevice parentJoy = tempPrimaryJoyDev;
                                        tempPrimaryJoyDev.Removal += (sender, args) => { currentJoyDev.JointDevice = null; };
                                        currentJoyDev.Removal += (sender, args) => { parentJoy.JointDevice = null; };

                                        tempPrimaryJoyDev = null;
                                    }
                                }
                            }

                            DS4Controllers[Index] = device;
                            device.DeviceSlotNumber = Index;

                            Global.RefreshExtrasButtons(Index, GetKnownExtraButtons(device));
                            Global.LoadControllerConfigs(device);
                            device.LoadStoreSettings();
                            device.CheckControllerNumDeviceSettings(numControllers);

                            slotManager.AddController(device, Index);
                            if (isUsingOSCSender())
                            {
                                oscSender.Send(new SharpOSC.OscMessage("/ds4windows/monitor/" + Index + "/plug", 1));
                            }
                            device.Removal += this.On_DS4Removal;
                            device.Removal += DS4Devices.On_Removal;
                            device.SyncChange += this.On_SyncChange;
                            device.SyncChange += DS4Devices.UpdateSerial;
                            device.SerialChange += this.On_SerialChange;
                            device.ChargingChanged += CheckQuickCharge;

                            touchPad[Index] = new Mouse(Index, device);
                            bool profileLoaded = false;
                            bool useAutoProfile = useTempProfile[Index];
                            if (!useAutoProfile)
                            {
                                if (device.isValidSerial() && containsLinkedProfile(device.getMacAddress()))
                                {
                                    ProfilePath[Index] = getLinkedProfile(device.getMacAddress());
                                    Global.linkedProfileCheck[Index] = true;
                                }
                                else
                                {
                                    ProfilePath[Index] = OlderProfilePath[Index];
                                    Global.linkedProfileCheck[Index] = false;
                                }

                                profileLoaded = LoadProfile(Index, false, this, false, false);
                            }

                            if (profileLoaded || useAutoProfile)
                            {
                                device.LightBarColor = getMainColor(Index);

                                if (!getDInputOnly(Index) && device.isSynced())
                                {
                                    if (device.PrimaryDevice)
                                    {
                                        PluginOutDev(Index, device);
                                    }
                                    else if (device.JointDeviceSlotNumber != DS4Device.DEFAULT_JOINT_SLOT_NUMBER)
                                    {
                                        int otherIdx = device.JointDeviceSlotNumber;
                                        OutputDevice tempOutDev = outputDevices[otherIdx];
                                        if (tempOutDev != null)
                                        {
                                            OutContType tempConType = activeOutDevType[otherIdx];
                                            EstablishOutFeedback(Index, tempConType, tempOutDev, device);
                                            outputDevices[Index] = tempOutDev;
                                            Global.activeOutDevType[Index] = tempConType;
                                        }
                                    }
                                }
                                else
                                {
                                    useDInputOnly[Index] = true;
                                    Global.activeOutDevType[Index] = OutContType.None;
                                }

                                if (device.PrimaryDevice && device.OutputMapGyro)
                                {
                                    TouchPadOn(Index, device);
                                }
                                else if (device.JointDeviceSlotNumber != DS4Device.DEFAULT_JOINT_SLOT_NUMBER)
                                {
                                    int otherIdx = device.JointDeviceSlotNumber;
                                    DS4Device tempDev = DS4Controllers[otherIdx];
                                    if (tempDev != null)
                                    {
                                        int mappedIdx = tempDev.PrimaryDevice ? otherIdx : Index;
                                        DS4Device gyroDev = device.OutputMapGyro ? device : (tempDev.OutputMapGyro ? tempDev : null);
                                        if (gyroDev != null)
                                        {
                                            TouchPadOn(mappedIdx, gyroDev);
                                        }
                                    }
                                }

                                CheckProfileOptions(Index, device);
                                SetupInitialHookEvents(Index, device);
                            }

                            int tempIdx = Index;
                            device.Report += (sender, e) =>
                            {
                                this.On_Report(sender, e, tempIdx);
                            };

                            if (_udpServer != null && Index < UdpServer.NUMBER_SLOTS)
                            {
                                PrepareDevUDPMotion(device, tempIdx);
                            }

                            device.StartUpdate();
                            HotplugController?.Invoke(this, device, Index);
                            break;
                        }
                    }
                }

                inServiceTask = false;
            }

            return true;
        }

        public void ResetUdpSmoothingFilters(int idx)
        {
            if (idx < UdpServer.NUMBER_SLOTS)
            {
                OneEuroFilter3D temp = udpEuroPairAccel[idx] = new OneEuroFilter3D();
                temp.SetFilterAttrs(Global.UDPServerSmoothingMincutoff, Global.UDPServerSmoothingBeta);

                temp = udpEuroPairGyro[idx] = new OneEuroFilter3D();
                temp.SetFilterAttrs(Global.UDPServerSmoothingMincutoff, Global.UDPServerSmoothingBeta);
            }
        }

        private void ChangeUdpSmoothingAttrs(object sender, EventArgs e)
        {
            for (int i = 0; i < udpEuroPairAccel.Length; i++)
            {
                OneEuroFilter3D temp = udpEuroPairAccel[i];
                temp.SetFilterAttrs(Global.UDPServerSmoothingMincutoff, Global.UDPServerSmoothingBeta);
            }

            for (int i = 0; i < udpEuroPairGyro.Length; i++)
            {
                OneEuroFilter3D temp = udpEuroPairGyro[i];
                temp.SetFilterAttrs(Global.UDPServerSmoothingMincutoff, Global.UDPServerSmoothingBeta);
            }
        }

        public void CheckProfileOptions(int ind, DS4Device device, bool startUp=false)
        {
            device.ModifyFeatureSetFlag(VidPidFeatureSet.NoOutputData, !getEnableOutputDataToDS4(ind));
            if (!getEnableOutputDataToDS4(ind))
                LogDebug("Output data to DS4 disabled. Lightbar and rumble events are not written to DS4 gamepad. If the gamepad is connected over BT then IdleDisconnect option is recommended to let DS4Windows to close the connection after long period of idling.");

            device.setIdleTimeout(getIdleDisconnectTimeout(ind));
            device.setBTPollRate(getBTPollRate(ind));
            touchPad[ind].ResetTrackAccel(getTrackballFriction(ind));
            touchPad[ind].ResetToggleGyroModes();

            // Reset current flick stick progress from previous profile
            Mapping.flickMappingData[ind].Reset();

            Global.L2OutputSettings[ind].TrigEffectSettings.maxValue = (byte)(Math.Max(Global.L2ModInfo[ind].maxOutput, Global.L2ModInfo[ind].maxZone) / 100.0 * 255);
            Global.R2OutputSettings[ind].TrigEffectSettings.maxValue = (byte)(Math.Max(Global.R2ModInfo[ind].maxOutput, Global.R2ModInfo[ind].maxZone) / 100.0 * 255);

            device.PrepareTriggerEffect(InputDevices.TriggerId.LeftTrigger, Global.L2OutputSettings[ind].TriggerEffect,
                Global.L2OutputSettings[ind].TrigEffectSettings);
            device.PrepareTriggerEffect(InputDevices.TriggerId.RightTrigger, Global.R2OutputSettings[ind].TriggerEffect,
                Global.R2OutputSettings[ind].TrigEffectSettings);

            device.RumbleAutostopTime = getRumbleAutostopTime(ind);
            device.setRumble(0, 0);
            device.LightBarColor = Global.getMainColor(ind);

            if (!startUp)
            {
                CheckLauchProfileOption(ind, device);
            }
        }

        private void CheckLauchProfileOption(int ind, DS4Device device)
        {
            string programPath = LaunchProgram[ind];
            if (programPath != string.Empty)
            {
                System.Diagnostics.Process[] localAll = System.Diagnostics.Process.GetProcesses();
                bool procFound = false;
                for (int procInd = 0, procsLen = localAll.Length; !procFound && procInd < procsLen; procInd++)
                {
                    try
                    {
                        string temp = localAll[procInd].MainModule.FileName;
                        if (temp == programPath)
                        {
                            procFound = true;
                        }
                    }
                    // Ignore any process for which this information
                    // is not exposed
                    catch { }
                }

                if (!procFound)
                {
                    Task processTask = new Task(() =>
                    {
                        Thread.Sleep(5000);
                        System.Diagnostics.Process tempProcess = new System.Diagnostics.Process();
                        tempProcess.StartInfo.FileName = programPath;
                        tempProcess.StartInfo.WorkingDirectory = new FileInfo(programPath).Directory.ToString();
                        //tempProcess.StartInfo.UseShellExecute = false;
                        try { tempProcess.Start(); }
                        catch { }
                    });

                    processTask.Start();
                }
            }
        }

        private void SetupInitialHookEvents(int ind, DS4Device device)
        {
            ResetUdpSmoothingFilters(ind);

            // Set up filter for new input device
            OneEuroFilter tempFilter = new OneEuroFilter(OneEuroFilterPair.DEFAULT_WHEEL_CUTOFF,
                OneEuroFilterPair.DEFAULT_WHEEL_BETA);
            Mapping.wheelFilters[ind] = tempFilter;

            // Carry over initial profile wheel smoothing values to filter instances.
            // Set up event hooks to keep values in sync
            SteeringWheelSmoothingInfo wheelSmoothInfo = WheelSmoothInfo[ind];
            wheelSmoothInfo.SetFilterAttrs(tempFilter);
            wheelSmoothInfo.SetRefreshEvents(tempFilter);

            FlickStickSettings flickStickSettings = Global.LSOutputSettings[ind].outputSettings.flickSettings;
            flickStickSettings.RemoveRefreshEvents();
            flickStickSettings.SetRefreshEvents(Mapping.flickMappingData[ind].flickFilter);

            flickStickSettings = Global.RSOutputSettings[ind].outputSettings.flickSettings;
            flickStickSettings.RemoveRefreshEvents();
            flickStickSettings.SetRefreshEvents(Mapping.flickMappingData[ind].flickFilter);

            int tempIdx = ind;
            Global.L2OutputSettings[ind].ResetEvents();
            Global.L2ModInfo[ind].ResetEvents();
            Global.L2OutputSettings[ind].TriggerEffectChanged += (sender, e) =>
            {
                device.PrepareTriggerEffect(InputDevices.TriggerId.LeftTrigger, Global.L2OutputSettings[tempIdx].TriggerEffect,
                    Global.L2OutputSettings[tempIdx].TrigEffectSettings);
            };
            Global.L2ModInfo[ind].MaxOutputChanged += (sender, e) =>
            {
                TriggerDeadZoneZInfo tempInfo = sender as TriggerDeadZoneZInfo;
                L2OutputSettings[tempIdx].TrigEffectSettings.maxValue = (byte)(Math.Max(tempInfo.maxOutput, tempInfo.maxZone) / 100.0 * 255.0);

                // Refresh trigger effect
                device.PrepareTriggerEffect(InputDevices.TriggerId.LeftTrigger, Global.L2OutputSettings[tempIdx].TriggerEffect,
                    Global.L2OutputSettings[tempIdx].TrigEffectSettings);
            };
            Global.L2ModInfo[ind].MaxZoneChanged += (sender, e) =>
            {
                TriggerDeadZoneZInfo tempInfo = sender as TriggerDeadZoneZInfo;
                L2OutputSettings[tempIdx].TrigEffectSettings.maxValue = (byte)(Math.Max(tempInfo.maxOutput, tempInfo.maxZone) / 100.0 * 255.0);

                // Refresh trigger effect
                device.PrepareTriggerEffect(InputDevices.TriggerId.LeftTrigger, Global.L2OutputSettings[tempIdx].TriggerEffect,
                    Global.L2OutputSettings[tempIdx].TrigEffectSettings);
            };

            Global.R2OutputSettings[ind].ResetEvents();
            Global.R2OutputSettings[ind].TriggerEffectChanged += (sender, e) =>
            {
                device.PrepareTriggerEffect(InputDevices.TriggerId.RightTrigger, Global.R2OutputSettings[tempIdx].TriggerEffect,
                    Global.R2OutputSettings[tempIdx].TrigEffectSettings);
            };
            Global.R2ModInfo[ind].MaxOutputChanged += (sender, e) =>
            {
                TriggerDeadZoneZInfo tempInfo = sender as TriggerDeadZoneZInfo;
                R2OutputSettings[tempIdx].TrigEffectSettings.maxValue = (byte)(tempInfo.maxOutput / 100.0 * 255.0);

                // Refresh trigger effect
                device.PrepareTriggerEffect(InputDevices.TriggerId.RightTrigger, Global.R2OutputSettings[tempIdx].TriggerEffect,
                    Global.R2OutputSettings[tempIdx].TrigEffectSettings);
            };
            Global.R2ModInfo[ind].MaxZoneChanged += (sender, e) =>
            {
                TriggerDeadZoneZInfo tempInfo = sender as TriggerDeadZoneZInfo;
                R2OutputSettings[tempIdx].TrigEffectSettings.maxValue = (byte)(tempInfo.maxOutput / 100.0 * 255.0);

                // Refresh trigger effect
                device.PrepareTriggerEffect(InputDevices.TriggerId.RightTrigger, Global.R2OutputSettings[tempIdx].TriggerEffect,
                    Global.R2OutputSettings[tempIdx].TrigEffectSettings);
            };
        }

        public void TouchPadOn(int ind, DS4Device device)
        {
            Mouse tPad = touchPad[ind];
            //ITouchpadBehaviour tPad = touchPad[ind];
            device.Touchpad.TouchButtonDown += tPad.touchButtonDown;
            device.Touchpad.TouchButtonUp += tPad.touchButtonUp;
            device.Touchpad.TouchesBegan += tPad.touchesBegan;
            device.Touchpad.TouchesMoved += tPad.touchesMoved;
            device.Touchpad.TouchesEnded += tPad.touchesEnded;
            device.Touchpad.TouchUnchanged += tPad.touchUnchanged;
            //device.Touchpad.PreTouchProcess += delegate { touchPad[ind].populatePriorButtonStates(); };
            device.Touchpad.PreTouchProcess += (sender, args) => { touchPad[ind].populatePriorButtonStates(); };
            device.SixAxis.SixAccelMoved += tPad.sixaxisMoved;
            //LogDebug("Touchpad mode for " + device.MacAddress + " is now " + tmode.ToString());
            //Log.LogToTray("Touchpad mode for " + device.MacAddress + " is now " + tmode.ToString());
        }

        public string GetDS4Battery(int index)
        {
            DS4Device d = DS4Controllers[index];
            if (d != null)
            {
                string battery;
                if (!d.IsAlive())
                    battery = "...";

                if (d.isCharging())
                {
                    if (d.getBattery() >= 100)
                        battery = DS4WinWPF.Properties.Resources.Full;
                    else
                        battery = d.getBattery() + "%+";
                }
                else
                {
                    battery = d.getBattery() + "%";
                }

                return battery;
            }
            else
                return DS4WinWPF.Properties.Resources.NA;
        }

        protected void On_SerialChange(object sender, EventArgs e)
        {
            DS4Device device = (DS4Device)sender;
            int ind = -1;
            for (int i = 0, arlength = MAX_DS4_CONTROLLER_COUNT; ind == -1 && i < arlength; i++)
            {
                DS4Device tempDev = DS4Controllers[i];
                if (tempDev != null && device == tempDev)
                    ind = i;
            }

            if (ind >= 0)
            {
                OnDeviceSerialChange(this, ind, device.getMacAddress());
            }
        }

        protected void On_SyncChange(object sender, EventArgs e)
        {
            DS4Device device = (DS4Device)sender;
            int ind = -1;
            for (int i = 0, arlength = CURRENT_DS4_CONTROLLER_LIMIT; ind == -1 && i < arlength; i++)
            {
                DS4Device tempDev = DS4Controllers[i];
                if (tempDev != null && device == tempDev)
                    ind = i;
            }

            if (ind >= 0)
            {
                bool synced = device.isSynced();

                if (!synced)
                {
                    if (!useDInputOnly[ind])
                    {
                        Global.activeOutDevType[ind] = OutContType.None;
                        UnplugOutDev(ind, device);
                    }
                }
                else
                {
                    if (!getDInputOnly(ind))
                    {
                        touchPad[ind].ReplaceOneEuroFilterPair();
                        touchPad[ind].ReplaceOneEuroFilterPair();

                        touchPad[ind].Cursor.ReplaceOneEuroFilterPair();
                        touchPad[ind].Cursor.SetupLateOneEuroFilters();
                        PluginOutDev(ind, device);
                    }
                }
            }
        }

        // Called when DS4 is disconnected or timed out
        protected void On_DS4Removal(object sender, EventArgs e)
        {
            DS4Device device = (DS4Device)sender;
            int ind = -1;
            for (int i = 0, arlength = DS4Controllers.Length; ind == -1 && i < arlength; i++)
            {
                if (DS4Controllers[i] != null && device.getMacAddress() == DS4Controllers[i].getMacAddress())
                    ind = i;
            }

            if (ind != -1)
            {
                bool removingStatus = false;
                lock (device.removeLocker)
                {
                    if (!device.IsRemoving)
                    {
                        removingStatus = true;
                        device.IsRemoving = true;
                    }
                }

                if (removingStatus)
                {
                    CurrentState[ind].Battery = PreviousState[ind].Battery = 0; // Reset for the next connection's initial status change.
                    if (!useDInputOnly[ind])
                    {
                        UnplugOutDev(ind, device);
                    }
                    else if (!device.PrimaryDevice)
                    {
                        OutputDevice outDev = outputDevices[ind];
                        if (outDev != null)
                        {
                            outDev.RemoveFeedback(ind);
                            outputDevices[ind] = null;
                        }
                    }

                    // Use Task to reset device synth state and commit it
                    Task.Run(() =>
                    {
                        Mapping.Commit(ind);
                    }).Wait();

                    string removed = DS4WinWPF.Properties.Resources.ControllerWasRemoved.Replace("*Mac address*", (ind + 1).ToString());
                    if (device.getBattery() <= 20 &&
                        device.getConnectionType() == ConnectionType.BT && !device.isCharging())
                    {
                        removed += ". " + DS4WinWPF.Properties.Resources.ChargeController;
                    }

                    LogDebug(removed);
                    AppLogger.LogToTray(removed);
                    /*Stopwatch sw = new Stopwatch();
                    sw.Start();
                    while (sw.ElapsedMilliseconds < XINPUT_UNPLUG_SETTLE_TIME)
                    {
                        // Use SpinWait to keep control of current thread. Using Sleep could potentially
                        // cause other events to get run out of order
                        System.Threading.Thread.SpinWait(500);
                    }
                    sw.Stop();
                    */

                    device.IsRemoved = true;
                    device.Synced = false;
                    DS4Controllers[ind] = null;
                    oscState[ind] = new DS4State();
                    //eventDispatcher.Invoke(() =>
                    //{
                        slotManager.RemoveController(device, ind);
                    if (isUsingOSCSender())
                    {
                        oscSender.Send(new SharpOSC.OscMessage("/ds4windows/monitor/" + ind + "/plug", 0));
                    }
                    //});

                    touchPad[ind] = null;
                    lag[ind] = false;
                    inWarnMonitor[ind] = false;
                    useDInputOnly[ind] = true;
                    Global.activeOutDevType[ind] = OutContType.None;
                    /* Leave up to Auto Profile system to change the following flags? */
                    //Global.useTempProfile[ind] = false;
                    //Global.tempprofilename[ind] = string.Empty;
                    //Global.tempprofileDistance[ind] = false;

                    //Thread.Sleep(XINPUT_UNPLUG_SETTLE_TIME);
                }
            }
        }

        public bool[] lag = new bool[MAX_DS4_CONTROLLER_COUNT] { false, false, false, false, false, false, false, false };
        public bool[] inWarnMonitor = new bool[MAX_DS4_CONTROLLER_COUNT] { false, false, false, false, false, false, false, false };
        private byte[] currentBattery = new byte[MAX_DS4_CONTROLLER_COUNT] { 0, 0, 0, 0, 0, 0, 0, 0 };
        private bool[] charging = new bool[MAX_DS4_CONTROLLER_COUNT] { false, false, false, false, false, false, false, false };
        private string[] tempStrings = new string[MAX_DS4_CONTROLLER_COUNT] { string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty };

        // Called every time a new input report has arrived
        protected void On_Report(DS4Device device, EventArgs e, int ind)
        {
            if (ind != -1)
            {
                string devError = tempStrings[ind] = device.error;
                if (!string.IsNullOrEmpty(devError))
                {
                    LogDebug(devError);
                }

                if (inWarnMonitor[ind])
                {
                    int flashWhenLateAt = getFlashWhenLateAt();
                    if (!lag[ind] && device.Latency >= flashWhenLateAt)
                    {
                        lag[ind] = true;
                        LagFlashWarning(device, ind, true);
                    }
                    else if (lag[ind] && device.Latency < flashWhenLateAt)
                    {
                        lag[ind] = false;
                        LagFlashWarning(device, ind, false);
                    }
                }
                else
                {
                    if (DateTime.UtcNow - device.firstActive > TimeSpan.FromSeconds(5))
                    {
                        inWarnMonitor[ind] = true;
                    }
                }

                DS4State cState, tempControlState;
                if (!device.PerformStateMerge)
                {
                    cState = CurrentState[ind];
                    device.getRawCurrentState(cState);
                    tempControlState = CurrentState[ind];
                }
                else
                {
                    cState = device.JointState;
                    device.MergeStateData(cState);
                    // Need to copy state object info for use in UDP server
                    cState.CopyTo(CurrentState[ind]);
                    tempControlState = CurrentState[ind];
                }

                DS4State pState = device.getPreviousStateRef();
                //device.getPreviousState(PreviousState[ind]);
                //DS4State pState = PreviousState[ind];

                if (device.firstReport && device.isSynced())
                {
                    // Only send Log message when device is considered a primary device
                    if (device.PrimaryDevice)
                    {
                        if (File.Exists(appdatapath + "\\Profiles\\" + ProfilePath[ind] + ".xml"))
                        {
                            string prolog = string.Format(DS4WinWPF.Properties.Resources.UsingProfile, (ind + 1).ToString(), ProfilePath[ind], $"{device.Battery}");
                            LogDebug(prolog);
                            AppLogger.LogToTray(prolog);
                        }
                        else
                        {
                            string prolog = string.Format(DS4WinWPF.Properties.Resources.NotUsingProfile, (ind + 1).ToString(), $"{device.Battery}");
                            LogDebug(prolog);
                            AppLogger.LogToTray(prolog);
                        }
                    }

                    device.firstReport = false;
                }

                if (!device.PrimaryDevice)
                {
                    // Make sure a joined device is still linked
                    int jointInd = device.JointDeviceSlotNumber;
                    if (device.OutputMapGyro &&
                        jointInd != DS4Device.DEFAULT_JOINT_SLOT_NUMBER)
                    {
                        // Output changes from Gyro data early. Seems better to ME... REE
                        GyroOutMode imuOutMode = Global.GetGyroOutMode(device.JointDeviceSlotNumber);
                        if (imuOutMode != GyroOutMode.None)
                        {
                            if (imuOutMode == GyroOutMode.Mouse)
                            {
                                outputKBMHandler.Sync();
                            }
                            else if (imuOutMode == GyroOutMode.MouseJoystick)
                            {
                                // Add new Mapping method and add data to
                                // parent device state
                                DS4State tempMapState = MappedState[jointInd];
                                Mapping.TempMouseJoystick(jointInd, tempMapState);
                                if (!useDInputOnly[jointInd])
                                {
                                    outputDevices[jointInd]?.ConvertandSendReport(tempMapState, jointInd);
                                }
                            }
                        }
                    }
                    else if (!device.OutputMapGyro)
                    {
                        // Copy for use in UDP
                        tempControlState.Motion = device.GetRawCurrentStateRef().Motion;
                    }

                    // Skip mapping routine if part of a joined device
                    return;
                }

                if (getEnableTouchToggle(ind))
                {
                    CheckForTouchToggle(ind, cState, pState);
                }

                cState = Mapping.SetCurveAndDeadzone(ind, cState, TempState[ind]);

                if (!recordingMacro && (useTempProfile[ind] ||
                    containsCustomAction(ind) || containsCustomExtras(ind) ||
                    getProfileActionCount(ind) > 0))
                {
                    DS4State tempMapState = MappedState[ind];
                    DS4State oscMapState = oscState[ind];

                    if (isUsingOSCSender())
                    {
                        OSCPreMappingStep(ind, cState, tempMapState, oscMapState);
                    }

                    Mapping.MapCustom(ind, cState, tempMapState, ExposedState[ind], touchPad[ind], this);

                    // Copy current Touchpad and Gyro data
                    // Might change to use new DS4State.CopyExtrasTo method
                    tempMapState.Motion = cState.Motion;
                    tempMapState.ds4Timestamp = cState.ds4Timestamp;
                    tempMapState.FrameCounter = cState.FrameCounter;
                    tempMapState.TouchPacketCounter = cState.TouchPacketCounter;
                    tempMapState.TrackPadTouch0 = cState.TrackPadTouch0;
                    tempMapState.TrackPadTouch1 = cState.TrackPadTouch1;

                    if (isUsingOSCServer())
                    {
                        OSCPostMappingStep(tempMapState, oscMapState);
                    }

                    cState = tempMapState;
                    
                }

                if (!useDInputOnly[ind])
                {
                    outputDevices[ind]?.ConvertandSendReport(cState, ind);
                    //testNewReport(ref x360reports[ind], cState, ind);
                    //x360controls[ind]?.SendReport(x360reports[ind]);

                    //x360Bus.Parse(cState, processingData[ind].Report, ind);
                    // We push the translated Xinput state, and simultaneously we
                    // pull back any possible rumble data coming from Xinput consumers.
                    /*if (x360Bus.Report(processingData[ind].Report, processingData[ind].Rumble))
                    {
                        byte Big = processingData[ind].Rumble[3];
                        byte Small = processingData[ind].Rumble[4];

                        if (processingData[ind].Rumble[1] == 0x08)
                        {
                            SetDevRumble(device, Big, Small, ind);
                        }
                    }
                    */
                }
                else
                {
                    // UseDInputOnly profile may re-map sixaxis gyro sensor values as a VJoy joystick axis (steering wheel emulation mode using VJoy output device). Handle this option because VJoy output works even in USeDInputOnly mode.
                    // If steering wheel emulation uses LS/RS/R2/L2 output axies then the profile should NOT use UseDInputOnly option at all because those require a virtual output device.
                    SASteeringWheelEmulationAxisType steeringWheelMappedAxis = Global.GetSASteeringWheelEmulationAxis(ind);
                    switch (steeringWheelMappedAxis)
                    {
                        case SASteeringWheelEmulationAxisType.None: break;

                        case SASteeringWheelEmulationAxisType.VJoy1X:
                        case SASteeringWheelEmulationAxisType.VJoy2X:
                            VJoyFeeder.vJoyFeeder.FeedAxisValue(cState.SASteeringWheelEmulationUnit, ((((uint)steeringWheelMappedAxis) - ((uint)SASteeringWheelEmulationAxisType.VJoy1X)) / 3) + 1, VJoyFeeder.HID_USAGES.HID_USAGE_X);
                            break;

                        case SASteeringWheelEmulationAxisType.VJoy1Y:
                        case SASteeringWheelEmulationAxisType.VJoy2Y:
                            VJoyFeeder.vJoyFeeder.FeedAxisValue(cState.SASteeringWheelEmulationUnit, ((((uint)steeringWheelMappedAxis) - ((uint)SASteeringWheelEmulationAxisType.VJoy1X)) / 3) + 1, VJoyFeeder.HID_USAGES.HID_USAGE_Y);
                            break;

                        case SASteeringWheelEmulationAxisType.VJoy1Z:
                        case SASteeringWheelEmulationAxisType.VJoy2Z:
                            VJoyFeeder.vJoyFeeder.FeedAxisValue(cState.SASteeringWheelEmulationUnit, ((((uint)steeringWheelMappedAxis) - ((uint)SASteeringWheelEmulationAxisType.VJoy1X)) / 3) + 1, VJoyFeeder.HID_USAGES.HID_USAGE_Z);
                            break;

                        default: break;
                    }
                }

                // Output any synthetic events.
                Mapping.Commit(ind);

                // Update the Lightbar color
                DS4LightBar.updateLightBar(device, ind);

                if (device.PerformStateMerge)
                {
                    device.PreserveMergedStateData();
                }

                if (device.PerformStateMerge && !device.OutputMapGyro)
                {
                    // Copy for use in UDP
                    tempControlState.Motion = device.GetRawCurrentStateRef().Motion;
                }
            }
        }

        private static void OSCPostMappingStep(DS4State tempMapState, DS4State oscMapState)
        {
            tempMapState.Cross |= oscMapState.Cross;
            tempMapState.Square |= oscMapState.Square;
            tempMapState.Circle |= oscMapState.Circle;
            tempMapState.Triangle |= oscMapState.Triangle;
            tempMapState.R1 |= oscMapState.R1;
            tempMapState.R2Btn |= oscMapState.R2Btn;
            if (oscMapState.R2Btn == true)
            {
                tempMapState.R2 = 255;
            }
            tempMapState.R3 |= oscMapState.R3;
            tempMapState.L1 |= oscMapState.L1;
            tempMapState.L2Btn |= oscMapState.L2Btn;
            if (oscMapState.L2Btn == true)
            {
                tempMapState.L2 = 255;
            }
            tempMapState.L3 |= oscMapState.L3;
            tempMapState.DpadUp |= oscMapState.DpadUp;
            tempMapState.DpadLeft |= oscMapState.DpadLeft;
            tempMapState.DpadRight |= oscMapState.DpadRight;
            tempMapState.DpadDown |= oscMapState.DpadDown;
            tempMapState.Options |= oscMapState.Options;
            tempMapState.Share |= oscMapState.Share;

            tempMapState.LX = oscMapState.LX != 128 ? oscMapState.LX : tempMapState.LX;
            tempMapState.LY = oscMapState.LY != 128 ? oscMapState.LY : tempMapState.LY;
            tempMapState.RX = oscMapState.RX != 128 ? oscMapState.RX : tempMapState.RX;
            tempMapState.RY = oscMapState.RY != 128 ? oscMapState.RY : tempMapState.RY;
        }

        private void OSCPreMappingStep(int ind, DS4State cState, DS4State tempMapState,
            DS4State oscMapState)
        {
            if (cState.Battery != oscMapState.Battery)
            {
                oscSender.Send(new OscMessage("/ds4windows/monitor/" + ind + "/battery", Convert.ToInt32(cState.Battery)));
                oscMapState.Battery = cState.Battery;
            }
            cState.Cross |= oscMapState.Cross;
            cState.Square |= oscMapState.Square;
            cState.Circle |= oscMapState.Circle;
            cState.Triangle |= oscMapState.Triangle;
            cState.R1 |= oscMapState.R1;
            cState.R2Btn |= oscMapState.R2Btn;
            if (oscMapState.R2Btn == true)
            {
                cState.R2 = 255;
            }
            cState.R3 |= oscMapState.R3;
            cState.L1 |= oscMapState.L1;
            cState.L2Btn |= oscMapState.L2Btn;
            if (oscMapState.L2Btn == true)
            {
                cState.L2 = 255;
            }
            cState.L3 |= oscMapState.L3;
            cState.DpadUp |= oscMapState.DpadUp;
            cState.DpadLeft |= oscMapState.DpadLeft;
            cState.DpadRight |= oscMapState.DpadRight;
            cState.DpadDown |= oscMapState.DpadDown;
            cState.Options |= oscMapState.Options;
            cState.Share |= oscMapState.Share;

            cState.LX = oscMapState.LX != 128 ? oscMapState.LX : cState.LX;
            cState.LY = oscMapState.LY != 128 ? oscMapState.LY : cState.LY;
            cState.RX = oscMapState.RX != 128 ? oscMapState.RX : cState.RX;
            cState.RY = oscMapState.RY != 128 ? oscMapState.RY : cState.RY;
            //AppLogger.LogToGui("I HEARD SOMETHING " + pCState.Cross+" : "+tempMapState.Cross, false);
            CompareAndSendChangesToOSC(ind, tempMapState, cState);
        }

        private void CompareAndSendChangesToOSC(int index, DS4State oldState, DS4State newState)
        {
            if(oldState.Square != newState.Square)
            {
                oscSender.Send(new OscMessage("/ds4windows/monitor/" + index + "/square", newState.Square==true?1:0));
            }

            if (oldState.Triangle != newState.Triangle)
            {
                oscSender.Send(new OscMessage("/ds4windows/monitor/" + index + "/triangle", newState.Triangle == true ? 1 : 0));
            }

            if (oldState.Circle != newState.Circle)
            {
                oscSender.Send(new OscMessage("/ds4windows/monitor/" + index + "/circle", newState.Circle == true ? 1 : 0));
            }

            if (oldState.Cross != newState.Cross)
            {
                oscSender.Send(new OscMessage("/ds4windows/monitor/" + index + "/cross", newState.Cross == true ? 1 : 0));
            }

            if (oldState.DpadUp != newState.DpadUp)
            {
                oscSender.Send(new OscMessage("/ds4windows/monitor/" + index + "/dpadup", newState.DpadUp == true ? 1 : 0));
            }

            if (oldState.DpadDown != newState.DpadDown)
            {
                oscSender.Send(new OscMessage("/ds4windows/monitor/" + index + "/dpaddown", newState.DpadDown == true ? 1 : 0));
            }

            if (oldState.DpadLeft != newState.DpadLeft)
            {
                oscSender.Send(new OscMessage("/ds4windows/monitor/" + index + "/dpadleft", newState.DpadLeft == true ? 1 : 0));
            }

            if (oldState.DpadRight != newState.DpadRight)
            {
                oscSender.Send(new OscMessage("/ds4windows/monitor/" + index + "/dpadright", newState.DpadRight == true ? 1 : 0));
            }

            if (oldState.L1 != newState.L1)
            {
                oscSender.Send(new OscMessage("/ds4windows/monitor/" + index + "/l1", newState.L1 == true ? 1 : 0));
            }

            if (oldState.L2 != newState.L2)
            {
                oscSender.Send(new OscMessage("/ds4windows/monitor/" + index + "/l2", Convert.ToInt32(newState.L2)));
            }

            if (oldState.L3 != newState.L3)
            {
                oscSender.Send(new OscMessage("/ds4windows/monitor/" + index + "/l3", newState.L3 == true ? 1 : 0));
            }

            if (oldState.R1 != newState.R1)
            {
                oscSender.Send(new OscMessage("/ds4windows/monitor/" + index + "/r1", newState.R1 == true ? 1 : 0));
            }

            if (oldState.R2 != newState.R2)
            {
                oscSender.Send(new OscMessage("/ds4windows/monitor/" + index + "/r2", Convert.ToInt32(newState.R2)));
            }

            if (oldState.R3 != newState.R3)
            {
                oscSender.Send(new OscMessage("/ds4windows/monitor/" + index + "/r3", newState.R3 == true ? 1 : 0));
            }

            if (oldState.LX != newState.LX)
            {
                oscSender.Send(new OscMessage("/ds4windows/monitor/" + index + "/lx", Convert.ToInt32(newState.LX)));
            }
            if (oldState.LY != newState.LY)
            {
                oscSender.Send(new OscMessage("/ds4windows/monitor/" + index + "/ly", Convert.ToInt32(newState.LY)));
            }
            if (oldState.RX != newState.RX)
            {
                oscSender.Send(new OscMessage("/ds4windows/monitor/" + index + "/rx", Convert.ToInt32(newState.RX)));
            }
            if (oldState.RY != newState.RY)
            {
                oscSender.Send(new OscMessage("/ds4windows/monitor/" + index + "/ry", Convert.ToInt32(newState.RY)));
            }

            if (oldState.Options != newState.Options)
            {
                oscSender.Send(new OscMessage("/ds4windows/monitor/" + index + "/options", newState.Options == true ? 1 : 0));
            }
            if (oldState.Share != newState.Share)
            {
                oscSender.Send(new OscMessage("/ds4windows/monitor/" + index + "/share", newState.Share == true ? 1 : 0));
            }

            if (oldState.PS != newState.PS)
            {
                oscSender.Send(new OscMessage("/ds4windows/monitor/" + index + "/ps", newState.PS == true ? 1 : 0));
            }
            
            /*if (oldState.Battery != newState.Battery)
            {
                AppLogger.LogToGui("BATTERY " + oldState.Battery + " : " + newState.Battery, false);
                oscSender.Send(new SharpOSC.OscMessage("/ds4windows/monitor/" + index + "/battery", Convert.ToInt32(newState.Battery)));
            }*/
        }

        private void LagFlashWarning(DS4Device device, int ind, bool on)
        {
            if (on)
            {
                lag[ind] = true;
                LogDebug(string.Format(DS4WinWPF.Properties.Resources.LatencyOverTen, (ind + 1), device.Latency), true);
                if (getFlashWhenLate())
                {
                    DS4Color color = new DS4Color { red = 50, green = 0, blue = 0 };
                    DS4LightBar.forcedColor[ind] = color;
                    DS4LightBar.forcedFlash[ind] = 2;
                    DS4LightBar.forcelight[ind] = true;
                }
            }
            else
            {
                lag[ind] = false;
                LogDebug(DS4WinWPF.Properties.Resources.LatencyNotOverTen.Replace("*number*", (ind + 1).ToString()));
                DS4LightBar.forcelight[ind] = false;
                DS4LightBar.forcedFlash[ind] = 0;
                device.LightBarColor = getMainColor(ind);
            }
        }

        public DS4Controls GetActiveInputControl(int ind)
        {
            DS4State cState = CurrentState[ind];
            DS4StateExposed eState = ExposedState[ind];
            Mouse tp = touchPad[ind];
            DS4Controls result = DS4Controls.None;

            if (DS4Controllers[ind] != null)
            {
                if (Mapping.getBoolButtonMapping(cState.Cross))
                    result = DS4Controls.Cross;
                else if (Mapping.getBoolButtonMapping(cState.Circle))
                    result = DS4Controls.Circle;
                else if (Mapping.getBoolButtonMapping(cState.Triangle))
                    result = DS4Controls.Triangle;
                else if (Mapping.getBoolButtonMapping(cState.Square))
                    result = DS4Controls.Square;
                else if (Mapping.getBoolButtonMapping(cState.L1))
                    result = DS4Controls.L1;
                else if (Mapping.getBoolTriggerMapping(cState.L2))
                    result = DS4Controls.L2;
                else if (Mapping.getBoolButtonMapping(cState.L3))
                    result = DS4Controls.L3;
                else if (Mapping.getBoolButtonMapping(cState.R1))
                    result = DS4Controls.R1;
                else if (Mapping.getBoolTriggerMapping(cState.R2))
                    result = DS4Controls.R2;
                else if (Mapping.getBoolButtonMapping(cState.R3))
                    result = DS4Controls.R3;
                else if (Mapping.getBoolButtonMapping(cState.DpadUp))
                    result = DS4Controls.DpadUp;
                else if (Mapping.getBoolButtonMapping(cState.DpadDown))
                    result = DS4Controls.DpadDown;
                else if (Mapping.getBoolButtonMapping(cState.DpadLeft))
                    result = DS4Controls.DpadLeft;
                else if (Mapping.getBoolButtonMapping(cState.DpadRight))
                    result = DS4Controls.DpadRight;
                else if (Mapping.getBoolButtonMapping(cState.Share))
                    result = DS4Controls.Share;
                else if (Mapping.getBoolButtonMapping(cState.Options))
                    result = DS4Controls.Options;
                else if (Mapping.getBoolButtonMapping(cState.PS))
                    result = DS4Controls.PS;
                else if (Mapping.getBoolAxisDirMapping(cState.LX, true))
                    result = DS4Controls.LXPos;
                else if (Mapping.getBoolAxisDirMapping(cState.LX, false))
                    result = DS4Controls.LXNeg;
                else if (Mapping.getBoolAxisDirMapping(cState.LY, true))
                    result = DS4Controls.LYPos;
                else if (Mapping.getBoolAxisDirMapping(cState.LY, false))
                    result = DS4Controls.LYNeg;
                else if (Mapping.getBoolAxisDirMapping(cState.RX, true))
                    result = DS4Controls.RXPos;
                else if (Mapping.getBoolAxisDirMapping(cState.RX, false))
                    result = DS4Controls.RXNeg;
                else if (Mapping.getBoolAxisDirMapping(cState.RY, true))
                    result = DS4Controls.RYPos;
                else if (Mapping.getBoolAxisDirMapping(cState.RY, false))
                    result = DS4Controls.RYNeg;
                else if (Mapping.getBoolTouchMapping(tp.leftDown))
                    result = DS4Controls.TouchLeft;
                else if (Mapping.getBoolTouchMapping(tp.rightDown))
                    result = DS4Controls.TouchRight;
                else if (Mapping.getBoolTouchMapping(tp.multiDown))
                    result = DS4Controls.TouchMulti;
                else if (Mapping.getBoolTouchMapping(tp.upperDown))
                    result = DS4Controls.TouchUpper;
            }

            return result;
        }

        public bool[] touchreleased = new bool[MAX_DS4_CONTROLLER_COUNT] { true, true, true, true, true, true, true, true },
            touchslid = new bool[MAX_DS4_CONTROLLER_COUNT] { false, false, false, false, false, false, false, false };

        public Dispatcher EventDispatcher { get => eventDispatcher; }
        public OutputSlotManager OutputslotMan { get => outputslotMan; }

        protected void CheckForTouchToggle(int deviceID, DS4State cState, DS4State pState)
        {
            if (!IsUsingTouchpadForControls(deviceID) && cState.Touch1 && pState.PS)
            {
                if (GetTouchActive(deviceID) && touchreleased[deviceID])
                {
                    TouchActive[deviceID] = false;
                    LogDebug(DS4WinWPF.Properties.Resources.TouchpadMovementOff);
                    AppLogger.LogToTray(DS4WinWPF.Properties.Resources.TouchpadMovementOff);
                    touchreleased[deviceID] = false;
                }
                else if (touchreleased[deviceID])
                {
                    TouchActive[deviceID] = true;
                    LogDebug(DS4WinWPF.Properties.Resources.TouchpadMovementOn);
                    AppLogger.LogToTray(DS4WinWPF.Properties.Resources.TouchpadMovementOn);
                    touchreleased[deviceID] = false;
                }
            }
            else
                touchreleased[deviceID] = true;
        }

        public void StartTPOff(int deviceID)
        {
            if (deviceID < CURRENT_DS4_CONTROLLER_LIMIT)
            {
                TouchActive[deviceID] = false;
            }
        }

        public string TouchpadSlide(int ind)
        {
            DS4State cState = CurrentState[ind];
            string slidedir = "none";
            if (DS4Controllers[ind] != null && cState.Touch2 &&
               !(touchPad[ind].dragging || touchPad[ind].dragging2))
            {
                if (touchPad[ind].slideright && !touchslid[ind])
                {
                    slidedir = "right";
                    touchslid[ind] = true;
                }
                else if (touchPad[ind].slideleft && !touchslid[ind])
                {
                    slidedir = "left";
                    touchslid[ind] = true;
                }
                else if (!touchPad[ind].slideleft && !touchPad[ind].slideright)
                {
                    slidedir = "";
                    touchslid[ind] = false;
                }
            }

            return slidedir;
        }

        public void LogDebug(String Data, bool warning = false)
        {
            //Console.WriteLine(System.DateTime.Now.ToString("G") + "> " + Data);
            if (Debug != null)
            {
                DebugEventArgs args = new DebugEventArgs(Data, warning);
                OnDebug(this, args);
            }
        }

        public void OnDebug(object sender, DebugEventArgs args)
        {
            if (Debug != null)
                Debug(this, args);
        }

        // sets the rumble adjusted with rumble boost. General use method
        public void setRumble(byte heavyMotor, byte lightMotor, int deviceNum)
        {
            if (deviceNum < CURRENT_DS4_CONTROLLER_LIMIT)
            {
                DS4Device device = DS4Controllers[deviceNum];
                if (device != null)
                    SetDevRumble(device, heavyMotor, lightMotor, deviceNum);
                    //device.setRumble((byte)lightBoosted, (byte)heavyBoosted);
            }
        }

        // sets the rumble adjusted with rumble boost. Method more used for
        // report handling. Avoid constant checking for a device.
        public void SetDevRumble(DS4Device device,
            byte heavyMotor, byte lightMotor, int deviceNum)
        {
            byte boost = getRumbleBoost(deviceNum);
            uint lightBoosted = ((uint)lightMotor * (uint)boost) / 100;
            if (lightBoosted > 255)
                lightBoosted = 255;
            uint heavyBoosted = ((uint)heavyMotor * (uint)boost) / 100;
            if (heavyBoosted > 255)
                heavyBoosted = 255;

            device.setRumble((byte)lightBoosted, (byte)heavyBoosted);
        }

        public DS4State getDS4State(int ind)
        {
            return CurrentState[ind];
        }

        public DS4State getDS4StateMapped(int ind)
        {
            return MappedState[ind];
        }

        public DS4State getDS4StateTemp(int ind)
        {
            return TempState[ind];
        }
    }
}
