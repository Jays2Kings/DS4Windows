using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using System.Media;
using System.Threading.Tasks;
using static DS4Windows.Global;
using System.Threading;
using System.Diagnostics;

namespace DS4Windows
{
    public class ControlService
    {
        public X360Device x360Bus = null;
        public const int DS4_CONTROLLER_COUNT = 4;
        public DS4Device[] DS4Controllers = new DS4Device[DS4_CONTROLLER_COUNT];
        public Mouse[] touchPad = new Mouse[DS4_CONTROLLER_COUNT];
        private bool running = false;
        private DS4State[] MappedState = new DS4State[DS4_CONTROLLER_COUNT];
        private DS4State[] CurrentState = new DS4State[DS4_CONTROLLER_COUNT];
        private DS4State[] PreviousState = new DS4State[DS4_CONTROLLER_COUNT];
        private DS4State[] TempState = new DS4State[DS4_CONTROLLER_COUNT];
        public DS4StateExposed[] ExposedState = new DS4StateExposed[DS4_CONTROLLER_COUNT];
        public bool recordingMacro = false;
        public event EventHandler<DebugEventArgs> Debug = null;
        bool[] buttonsdown = new bool[4] { false, false, false, false };
        bool[] held = new bool[DS4_CONTROLLER_COUNT];
        int[] oldmouse = new int[DS4_CONTROLLER_COUNT] { -1, -1, -1, -1 };
        Thread tempThread;
        public List<string> affectedDevs = new List<string>()
        {
            @"HID\VID_054C&PID_05C4",
            @"HID\VID_054C&PID_09CC&MI_03",
            @"HID\VID_054C&PID_0BA0&MI_03",
            @"HID\{00001124-0000-1000-8000-00805f9b34fb}_VID&0002054c_PID&05c4",
            @"HID\{00001124-0000-1000-8000-00805f9b34fb}_VID&0002054c_PID&09cc",
        };
        public bool suspending;
        //SoundPlayer sp = new SoundPlayer();
        private UdpServer _udpServer;

        private class X360Data
        {
            public byte[] Report = new byte[28];
            public byte[] Rumble = new byte[8];
        }

        private X360Data[] processingData = new X360Data[4];

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
            //if (d.isValidSerial())
            //{
                string stringMac = d.getMacAddress();
                if (!string.IsNullOrEmpty(stringMac))
                {
                    stringMac = string.Join("", stringMac.Split(':'));
                    //stringMac = stringMac.Replace(":", "").Trim();
                    meta.PadMacAddress = System.Net.NetworkInformation.PhysicalAddress.Parse(stringMac);
                    isValidSerial = d.isValidSerial();
                }
            //}

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

            if (d.isCharging() && d.getBattery() >= 100)
                meta.BatteryStatus = DsBattery.Charged;
            else
            {
                if (d.getBattery() >= 95)
                    meta.BatteryStatus = DsBattery.Full;
                else if (d.getBattery() >= 70)
                    meta.BatteryStatus = DsBattery.High;
                else if (d.getBattery() >= 50)
                    meta.BatteryStatus = DsBattery.Medium;
                else if (d.getBattery() >= 20)
                    meta.BatteryStatus = DsBattery.Low;
                else if (d.getBattery() >= 5)
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
        public ControlService()
        {
            //sp.Stream = Properties.Resources.EE;
            // Cause thread affinity to not be tied to main GUI thread
            tempThread = new Thread(() => {
                x360Bus = new X360Device();
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
            tempThread.Priority = ThreadPriority.Normal;
            tempThread.IsBackground = true;
            tempThread.Start();
            //while (_udpServer == null)
            //{
            //    Thread.SpinWait(500);
            //}

            for (int i = 0, arlength = DS4Controllers.Length; i < arlength; i++)
            {
                processingData[i] = new X360Data();
                MappedState[i] = new DS4State();
                CurrentState[i] = new DS4State();
                TempState[i] = new DS4State();
                PreviousState[i] = new DS4State();
                ExposedState[i] = new DS4StateExposed(CurrentState[i]);
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

                            try
                            {
                                _udpServer.Start(UDP_SERVER_PORT);
                                LogDebug("UDP server listening on port " + UDP_SERVER_PORT);
                            }
                            catch (System.Net.Sockets.SocketException ex)
                            {
                                var errMsg = String.Format("Couldn't start UDP server on port {0}, outside applications won't be able to access pad data ({1})", UDP_SERVER_PORT, ex.SocketErrorCode);

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
                });
            }
        }

        public void ChangeMotionEventStatus(bool state)
        {
            IEnumerable<DS4Device> devices = DS4Devices.getDS4Controllers();
            if (state)
            {
                foreach (DS4Device dev in devices)
                {
                    dev.queueEvent(() =>
                    {
                        dev.Report += dev.MotionEvent;
                    });
                }
            }
            else
            {
                foreach (DS4Device dev in devices)
                {
                    dev.queueEvent(() =>
                    {
                        dev.Report -= dev.MotionEvent;
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
                    dev.Report -= dev.MotionEvent;
                });
            }

            await Task.Delay(100);

            var UDP_SERVER_PORT = Global.getUDPServerPortNum();
            try
            {
                _udpServer.Start(UDP_SERVER_PORT);
                foreach (DS4Device dev in devices)
                {
                    dev.queueEvent(() =>
                    {
                        dev.Report += dev.MotionEvent;
                    });
                }
                LogDebug("UDP server listening on port " + UDP_SERVER_PORT);
            }
            catch (System.Net.Sockets.SocketException ex)
            {
                var errMsg = String.Format("Couldn't start UDP server on port {0}, outside applications won't be able to access pad data ({1})", UDP_SERVER_PORT, ex.SocketErrorCode);

                LogDebug(errMsg, true);
                AppLogger.LogToTray(errMsg, true, true);
            }

            changingUDPPort = false;
        }

        private void WarnExclusiveModeFailure(DS4Device device)
        {
            if (DS4Devices.isExclusiveMode && !device.isExclusive())
            {
                string message = Properties.Resources.CouldNotOpenDS4.Replace("*Mac address*", device.getMacAddress()) + " " +
                    Properties.Resources.QuitOtherPrograms;
                LogDebug(message, true);
                AppLogger.LogToTray(message, true);
            }
        }

        private SynchronizationContext uiContext = null;
        public bool Start(object tempui, bool showlog = true)
        {
            if (x360Bus.Open() && x360Bus.Start())
            {
                if (showlog)
                    LogDebug(Properties.Resources.Starting);

                LogDebug("Connection to Scp Virtual Bus established");

                DS4Devices.isExclusiveMode = getUseExclusiveMode();
                uiContext = tempui as SynchronizationContext;
                if (showlog)
                {
                    LogDebug(Properties.Resources.SearchingController);
                    LogDebug(DS4Devices.isExclusiveMode ? Properties.Resources.UsingExclusive : Properties.Resources.UsingShared);
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
                    DS4Devices.findControllers();
                    IEnumerable<DS4Device> devices = DS4Devices.getDS4Controllers();
                    //int ind = 0;
                    DS4LightBar.defaultLight = false;
                    //foreach (DS4Device device in devices)

                    for (int i = 0, devCount = devices.Count(); i < devCount; i++)
                    {
                        DS4Device device = devices.ElementAt(i);
                        if (showlog)
                            LogDebug(Properties.Resources.FoundController + device.getMacAddress() + " (" + device.getConnectionType() + ")");

                        Task task = new Task(() => { Thread.Sleep(5); WarnExclusiveModeFailure(device); });
                        task.Start();

                        DS4Controllers[i] = device;
                        device.Removal += this.On_DS4Removal;
                        device.Removal += DS4Devices.On_Removal;
                        device.SyncChange += this.On_SyncChange;
                        device.SyncChange += DS4Devices.UpdateSerial;
                        device.SerialChange += this.On_SerialChange;
                        if (device.isValidSerial() && containsLinkedProfile(device.getMacAddress()))
                        {
                            ProfilePath[i] = getLinkedProfile(device.getMacAddress());
                        }
                        else
                        {
                            ProfilePath[i] = OlderProfilePath[i];
                        }
                        LoadProfile(i, false, this, false, false);
                        touchPad[i] = new Mouse(i, device);
                        device.LightBarColor = getMainColor(i);

                        if (!getDInputOnly(i) && device.isSynced())
                        {
                            int xinputIndex = x360Bus.FirstController + i;
                            LogDebug("Plugging in X360 Controller #" + xinputIndex);
                            bool xinputResult = x360Bus.Plugin(i);
                            if (xinputResult)
                            {
                                useDInputOnly[i] = false;
                                LogDebug("X360 Controller # " + xinputIndex + " connected");
                            }
                            else
                            {
                                useDInputOnly[i] = true;
                                LogDebug("X360 Controller # " + xinputIndex + " failed. Using DInput only mode");
                            }
                        }
                        else
                        {
                            useDInputOnly[i] = true;
                        }

                        int tempIdx = i;
                        device.Report += (sender, e) =>
                        {
                            this.On_Report(sender, e, tempIdx);
                        };

                        EventHandler<EventArgs> tempEvnt = (sender, args) =>
                        {
                            DualShockPadMeta padDetail = new DualShockPadMeta();
                            GetPadDetailForIdx(tempIdx, ref padDetail);
                            _udpServer.NewReportIncoming(ref padDetail, CurrentState[tempIdx]);
                        };
                        device.MotionEvent = tempEvnt;

                        if (_udpServer != null)
                        {
                            device.Report += tempEvnt;
                        }

                        TouchPadOn(i, device);
                        CheckProfileOptions(i, device, true);
                        device.StartUpdate();
                        //string filename = ProfilePath[ind];
                        //ind++;
                        if (showlog)
                        {
                            if (File.Exists(appdatapath + "\\Profiles\\" + ProfilePath[i] + ".xml"))
                            {
                                string prolog = Properties.Resources.UsingProfile.Replace("*number*", (i + 1).ToString()).Replace("*Profile name*", ProfilePath[i]);
                                LogDebug(prolog);
                                AppLogger.LogToTray(prolog);
                            }
                            else
                            {
                                string prolog = Properties.Resources.NotUsingProfile.Replace("*number*", (i + 1).ToString());
                                LogDebug(prolog);
                                AppLogger.LogToTray(prolog);
                            }
                        }

                        if (i >= 4) // out of Xinput devices!
                            break;
                    }
                }
                catch (Exception e)
                {
                    LogDebug(e.Message);
                    AppLogger.LogToTray(e.Message);
                }

                running = true;

                if (_udpServer != null)
                {
                    //var UDP_SERVER_PORT = 26760;
                    var UDP_SERVER_PORT = Global.getUDPServerPortNum();

                    try
                    {
                        _udpServer.Start(UDP_SERVER_PORT);
                        LogDebug("UDP server listening on port " + UDP_SERVER_PORT);
                    }
                    catch (System.Net.Sockets.SocketException ex)
                    {
                        var errMsg = String.Format("Couldn't start UDP server on port {0}, outside applications won't be able to access pad data ({1})", UDP_SERVER_PORT, ex.SocketErrorCode);

                        LogDebug(errMsg, true);
                        AppLogger.LogToTray(errMsg, true, true);
                    }
                }
            }
            else
            {
                string logMessage = "Could not connect to Scp Virtual Bus Driver. Please check the status of the System device in Device Manager";
                LogDebug(logMessage);
                AppLogger.LogToTray(logMessage);
            }

            runHotPlug = true;
            return true;
        }

        public bool Stop(bool showlog = true)
        {
            if (running)
            {
                running = false;
                runHotPlug = false;

                if (showlog)
                    LogDebug(Properties.Resources.StoppingX360);

                LogDebug("Closing connection to Scp Virtual Bus");

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
                        x360Bus.Unplug(i);
                        useDInputOnly[i] = true;
                        anyUnplugged = true;
                        DS4Controllers[i] = null;
                        touchPad[i] = null;
                        lag[i] = false;
                        inWarnMonitor[i] = false;
                    }
                }

                if (anyUnplugged)
                    Thread.Sleep(XINPUT_UNPLUG_SETTLE_TIME);

                x360Bus.UnplugAll();
                x360Bus.Stop();

                if (showlog)
                    LogDebug(Properties.Resources.StoppingDS4);

                DS4Devices.stopControllers();

                if (_udpServer != null)
                    ChangeUDPStatus(false);
                    //_udpServer.Stop();

                if (showlog)
                    LogDebug(Properties.Resources.StoppedDS4Windows);
            }

            runHotPlug = false;
            return true;
        }

        public bool HotPlug()
        {
            if (running)
            {
                DS4Devices.findControllers();
                IEnumerable<DS4Device> devices = DS4Devices.getDS4Controllers();
                //foreach (DS4Device device in devices)
                for (int i = 0, devlen = devices.Count(); i < devlen; i++)
                {
                    DS4Device device = devices.ElementAt(i);

                    if (device.isDisconnectingStatus())
                        continue;

                    if (((Func<bool>)delegate
                    {
                        for (Int32 Index = 0, arlength = DS4Controllers.Length; Index < arlength; Index++)
                        {
                            if (DS4Controllers[Index] != null &&
                                DS4Controllers[Index].getMacAddress() == device.getMacAddress())
                                return true;
                        }

                        return false;
                    })())
                    {
                        continue;
                    }

                    for (Int32 Index = 0, arlength = DS4Controllers.Length; Index < arlength; Index++)
                    {
                        if (DS4Controllers[Index] == null)
                        {
                            LogDebug(Properties.Resources.FoundController + device.getMacAddress() + " (" + device.getConnectionType() + ")");
                            Task task = new Task(() => { Thread.Sleep(5); WarnExclusiveModeFailure(device); });
                            task.Start();
                            DS4Controllers[Index] = device;
                            device.Removal += this.On_DS4Removal;
                            device.Removal += DS4Devices.On_Removal;
                            device.SyncChange += this.On_SyncChange;
                            device.SyncChange += DS4Devices.UpdateSerial;
                            device.SerialChange += this.On_SerialChange;
                            if (device.isValidSerial() && containsLinkedProfile(device.getMacAddress()))
                            {
                                ProfilePath[Index] = getLinkedProfile(device.getMacAddress());
                            }
                            else
                            {
                                ProfilePath[Index] = OlderProfilePath[Index];
                            }

                            LoadProfile(Index, false, this, false, false);
                            touchPad[Index] = new Mouse(Index, device);
                            device.LightBarColor = getMainColor(Index);

                            int tempIdx = Index;
                            device.Report += (sender, e) =>
                            {
                                this.On_Report(sender, e, tempIdx);
                            };

                            EventHandler<EventArgs> tempEvnt = (sender, args) =>
                            {
                                DualShockPadMeta padDetail = new DualShockPadMeta();
                                GetPadDetailForIdx(tempIdx, ref padDetail);
                                _udpServer.NewReportIncoming(ref padDetail, CurrentState[tempIdx]);
                            };
                            device.MotionEvent = tempEvnt;

                            if (_udpServer != null)
                            {
                                device.Report += tempEvnt;
                            }

                            if (!getDInputOnly(Index) && device.isSynced())
                            {
                                int xinputIndex = x360Bus.FirstController + Index;
                                LogDebug("Plugging in X360 Controller #" + xinputIndex);
                                bool xinputResult = x360Bus.Plugin(Index);
                                if (xinputResult)
                                {
                                    useDInputOnly[Index] = false;
                                    LogDebug("X360 Controller # " + xinputIndex + " connected");
                                }
                                else
                                {
                                    useDInputOnly[Index] = true;
                                    LogDebug("X360 Controller # " + xinputIndex + " failed. Using DInput only mode");
                                }
                            }
                            else
                            {
                                useDInputOnly[Index] = true;
                            }

                            TouchPadOn(Index, device);
                            CheckProfileOptions(Index, device);
                            device.StartUpdate();

                            //string filename = Path.GetFileName(ProfilePath[Index]);
                            if (File.Exists(appdatapath + "\\Profiles\\" + ProfilePath[Index] + ".xml"))
                            {
                                string prolog = Properties.Resources.UsingProfile.Replace("*number*", (Index + 1).ToString()).Replace("*Profile name*", ProfilePath[Index]);
                                LogDebug(prolog);
                                AppLogger.LogToTray(prolog);
                            }
                            else
                            {
                                string prolog = Properties.Resources.NotUsingProfile.Replace("*number*", (Index + 1).ToString());
                                LogDebug(prolog);
                                AppLogger.LogToTray(prolog);
                            }

                            break;
                        }
                    }
                }
            }

            return true;
        }

        private void CheckProfileOptions(int ind, DS4Device device, bool startUp=false)
        {
            device.setIdleTimeout(getIdleDisconnectTimeout(ind));
            device.setBTPollRate(getBTPollRate(ind));
            touchPad[ind].ResetTrackAccel(getTrackballFriction(ind));

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

        public void TouchPadOn(int ind, DS4Device device)
        {
            ITouchpadBehaviour tPad = touchPad[ind];
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

        public string getDS4ControllerInfo(int index)
        {
            DS4Device d = DS4Controllers[index];
            if (d != null)
            {
                if (!d.IsAlive())
                {
                    return Properties.Resources.Connecting;
                }

                string battery;
                if (d.isCharging())
                {
                    if (d.getBattery() >= 100)
                        battery = Properties.Resources.Charged;
                    else
                        battery = Properties.Resources.Charging.Replace("*number*", d.getBattery().ToString());
                }
                else
                {
                    battery = Properties.Resources.Battery.Replace("*number*", d.getBattery().ToString());
                }

                return d.getMacAddress() + " (" + d.getConnectionType() + "), " + battery;
                //return d.MacAddress + " (" + d.ConnectionType + "), Battery is " + battery + ", Touchpad in " + modeSwitcher[index].ToString();
            }
            else
                return string.Empty;
        }

        public string getDS4MacAddress(int index)
        {
            DS4Device d = DS4Controllers[index];
            if (d != null)
            {
                if (!d.IsAlive())
                {
                    return Properties.Resources.Connecting;
                }

                return d.getMacAddress();
            }
            else
                return string.Empty;
        }

        public string getShortDS4ControllerInfo(int index)
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
                        battery = Properties.Resources.Full;
                    else
                        battery = d.getBattery() + "%+";
                }
                else
                {
                    battery = d.getBattery() + "%";
                }

                return (d.getConnectionType() + " " + battery);
            }
            else
                return Properties.Resources.NoneText;
        }

        public string getDS4Battery(int index)
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
                        battery = Properties.Resources.Full;
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
                return Properties.Resources.NA;
        }

        public string getDS4Status(int index)
        {
            DS4Device d = DS4Controllers[index];
            if (d != null)
            {
                return d.getConnectionType() + "";
            }
            else
                return Properties.Resources.NoneText;
        }

        protected void On_SerialChange(object sender, EventArgs e)
        {
            DS4Device device = (DS4Device)sender;
            int ind = -1;
            for (int i = 0, arlength = DS4_CONTROLLER_COUNT; ind == -1 && i < arlength; i++)
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
            for (int i = 0, arlength = DS4_CONTROLLER_COUNT; ind == -1 && i < arlength; i++)
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
                        bool unplugResult = x360Bus.Unplug(ind);
                        int xinputIndex = x360Bus.FirstController + ind;
                        if (unplugResult)
                        {
                            useDInputOnly[ind] = true;
                            LogDebug("X360 Controller # " + xinputIndex + " unplugged");
                        }
                        else
                        {
                            LogDebug("X360 Controller # " + xinputIndex + " failed to unplug");
                        }
                    }
                }
                else
                {
                    if (!getDInputOnly(ind))
                    {
                        int xinputIndex = x360Bus.FirstController + ind;
                        LogDebug("Plugging in X360 Controller #" + xinputIndex);
                        bool xinputResult = x360Bus.Plugin(ind);
                        if (xinputResult)
                        {
                            useDInputOnly[ind] = false;
                            LogDebug("X360 Controller # " + xinputIndex + " connected");
                        }
                        else
                        {
                            useDInputOnly[ind] = true;
                            LogDebug("X360 Controller # " + xinputIndex + " failed. Using DInput only mode");
                        }
                    }
                }
            }
        }

        //Called when DS4 is disconnected or timed out
        protected virtual void On_DS4Removal(object sender, EventArgs e)
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
                        bool unplugResult = x360Bus.Unplug(ind);
                        int xinputIndex = x360Bus.FirstController + ind;
                        LogDebug("X360 Controller # " + xinputIndex + " unplugged");
                    }

                    // Use Task to reset device synth state and commit it
                    Task.Run(() =>
                    {
                        Mapping.Commit(ind);
                    }).Wait();

                    string removed = Properties.Resources.ControllerWasRemoved.Replace("*Mac address*", (ind + 1).ToString());
                    if (device.getBattery() <= 20 &&
                        device.getConnectionType() == ConnectionType.BT && !device.isCharging())
                    {
                        removed += ". " + Properties.Resources.ChargeController;
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
                    touchPad[ind] = null;
                    lag[ind] = false;
                    inWarnMonitor[ind] = false;
                    useDInputOnly[ind] = true;
                    uiContext?.Post(new SendOrPostCallback((state) =>
                    {
                        OnControllerRemoved(this, ind);
                    }), null);
                    //Thread.Sleep(XINPUT_UNPLUG_SETTLE_TIME);
                }
            }
        }

        public bool[] lag = new bool[4] { false, false, false, false };
        public bool[] inWarnMonitor = new bool[4] { false, false, false, false };
        private byte[] currentBattery = new byte[4] { 0, 0, 0, 0 };
        private bool[] charging = new bool[4] { false, false, false, false };
        private string[] tempStrings = new string[4] { string.Empty, string.Empty, string.Empty, string.Empty };

        // Called every time a new input report has arrived
        protected virtual void On_Report(object sender, EventArgs e, int ind)
        {
            DS4Device device = (DS4Device)sender;

            if (ind != -1)
            {
                if (getFlushHIDQueue(ind))
                    device.FlushHID();

                string devError = tempStrings[ind] = device.error;
                if (!string.IsNullOrEmpty(devError))
                {
                    uiContext?.Post(new SendOrPostCallback(delegate (object state)
                    {
                        LogDebug(devError);
                    }), null);
                }

                if (inWarnMonitor[ind])
                {
                    int flashWhenLateAt = getFlashWhenLateAt();
                    if (!lag[ind] && device.Latency >= flashWhenLateAt)
                    {
                        lag[ind] = true;
                        uiContext?.Post(new SendOrPostCallback(delegate (object state)
                        {
                            LagFlashWarning(ind, true);
                        }), null);
                    }
                    else if (lag[ind] && device.Latency < flashWhenLateAt)
                    {
                        lag[ind] = false;
                        uiContext?.Post(new SendOrPostCallback(delegate (object state)
                        {
                            LagFlashWarning(ind, false);
                        }), null);
                    }
                }
                else
                {
                    if (DateTime.UtcNow - device.firstActive > TimeSpan.FromSeconds(5))
                    {
                        inWarnMonitor[ind] = true;
                    }
                }

                device.getCurrentState(CurrentState[ind]);
                DS4State cState = CurrentState[ind];
                DS4State pState = device.getPreviousStateRef();
                //device.getPreviousState(PreviousState[ind]);
                //DS4State pState = PreviousState[ind];

                if (device.firstReport && device.IsAlive())
                {
                    device.firstReport = false;
                    uiContext?.Post(new SendOrPostCallback(delegate (object state)
                    {
                        OnDeviceStatusChanged(this, ind);
                    }), null);
                }
                else if (pState.Battery != cState.Battery || device.oldCharging != device.isCharging())
                {
                    byte tempBattery = currentBattery[ind] = cState.Battery;
                    bool tempCharging = charging[ind] = device.isCharging();
                    uiContext?.Post(new SendOrPostCallback(delegate (object state)
                    {
                        OnBatteryStatusChange(this, ind, tempBattery, tempCharging);
                    }), null);
                }

                if (getEnableTouchToggle(ind))
                    CheckForTouchToggle(ind, cState, pState);

                cState = Mapping.SetCurveAndDeadzone(ind, cState, TempState[ind]);

                if (!recordingMacro && (!string.IsNullOrEmpty(tempprofilename[ind]) ||
                    containsCustomAction(ind) || containsCustomExtras(ind) ||
                    getProfileActionCount(ind) > 0))
                {
                    Mapping.MapCustom(ind, cState, MappedState[ind], ExposedState[ind], touchPad[ind], this);
                    cState = MappedState[ind];
                }

                if (!useDInputOnly[ind])
                {
                    x360Bus.Parse(cState, processingData[ind].Report, ind);
                    // We push the translated Xinput state, and simultaneously we
                    // pull back any possible rumble data coming from Xinput consumers.
                    if (x360Bus.Report(processingData[ind].Report, processingData[ind].Rumble))
                    {
                        byte Big = processingData[ind].Rumble[3];
                        byte Small = processingData[ind].Rumble[4];

                        if (processingData[ind].Rumble[1] == 0x08)
                        {
                            setRumble(Big, Small, ind);
                        }
                    }
                }

                // Output any synthetic events.
                Mapping.Commit(ind);

                // Update the GUI/whatever.
                DS4LightBar.updateLightBar(device, ind);
            }
        }

        public void LagFlashWarning(int ind, bool on)
        {
            if (on)
            {
                lag[ind] = true;
                LogDebug(Properties.Resources.LatencyOverTen.Replace("*number*", (ind + 1).ToString()), true);
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
                LogDebug(Properties.Resources.LatencyNotOverTen.Replace("*number*", (ind + 1).ToString()));
                DS4LightBar.forcelight[ind] = false;
                DS4LightBar.forcedFlash[ind] = 0;
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

        public bool[] touchreleased = new bool[4] { true, true, true, true },
            touchslid = new bool[4] { false, false, false, false };

        public byte[] oldtouchvalue = new byte[4] { 0, 0, 0, 0 };
        public int[] oldscrollvalue = new int[4] { 0, 0, 0, 0 };

        protected virtual void CheckForTouchToggle(int deviceID, DS4State cState, DS4State pState)
        {
            if (!getUseTPforControls(deviceID) && cState.Touch1 && pState.PS)
            {
                if (getTouchSensitivity(deviceID) > 0 && touchreleased[deviceID])
                {
                    oldtouchvalue[deviceID] = getTouchSensitivity(deviceID);
                    oldscrollvalue[deviceID] = getScrollSensitivity(deviceID);
                    getTouchSensitivity()[deviceID] = 0;
                    getScrollSensitivity()[deviceID] = 0;
                    LogDebug(Properties.Resources.TouchpadMovementOff);
                    AppLogger.LogToTray(Properties.Resources.TouchpadMovementOff);
                    touchreleased[deviceID] = false;
                }
                else if (touchreleased[deviceID])
                {
                    getTouchSensitivity()[deviceID] = oldtouchvalue[deviceID];
                    getScrollSensitivity()[deviceID] = oldscrollvalue[deviceID];
                    LogDebug(Properties.Resources.TouchpadMovementOn);
                    AppLogger.LogToTray(Properties.Resources.TouchpadMovementOn);
                    touchreleased[deviceID] = false;
                }
            }
            else
                touchreleased[deviceID] = true;
        }

        public virtual void StartTPOff(int deviceID)
        {
            if (deviceID < 4)
            {
                oldtouchvalue[deviceID] = getTouchSensitivity(deviceID);
                oldscrollvalue[deviceID] = getScrollSensitivity(deviceID);
                TouchSensitivity[deviceID] = 0;
                ScrollSensitivity[deviceID] = 0;
            }
        }

        public virtual string TouchpadSlide(int ind)
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

        public virtual void LogDebug(String Data, bool warning = false)
        {
            //Console.WriteLine(System.DateTime.Now.ToString("G") + "> " + Data);
            if (Debug != null)
            {
                DebugEventArgs args = new DebugEventArgs(Data, warning);
                OnDebug(this, args);
            }
        }

        public virtual void OnDebug(object sender, DebugEventArgs args)
        {
            if (Debug != null)
                Debug(this, args);
        }

        //sets the rumble adjusted with rumble boost
        public virtual void setRumble(byte heavyMotor, byte lightMotor, int deviceNum)
        {
            byte boost = getRumbleBoost(deviceNum);
            uint lightBoosted = ((uint)lightMotor * (uint)boost) / 100;
            if (lightBoosted > 255)
                lightBoosted = 255;
            uint heavyBoosted = ((uint)heavyMotor * (uint)boost) / 100;
            if (heavyBoosted > 255)
                heavyBoosted = 255;

            if (deviceNum < 4)
            {
                DS4Device device = DS4Controllers[deviceNum];
                if (device != null)
                    device.setRumble((byte)lightBoosted, (byte)heavyBoosted);
            }
        }

        public DS4State getDS4State(int ind)
        {
            return CurrentState[ind];
        }

        public DS4State getDS4StateMapped(int ind)
        {
            return MappedState[ind];
        }        
    }
}
