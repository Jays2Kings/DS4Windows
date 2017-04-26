using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;
using System.Reflection;
using System.Media;
using System.Threading.Tasks;
using static DS4Windows.Global;
namespace DS4Windows
{
    public class ControlService
    {
        public X360Device x360Bus;
        public const int DS4_CONTROLLER_COUNT = 4;
        public DS4Device[] DS4Controllers = new DS4Device[DS4_CONTROLLER_COUNT];
        public Mouse[] touchPad = new Mouse[DS4_CONTROLLER_COUNT];
        private bool running = false;
        private DS4State[] MappedState = new DS4State[DS4_CONTROLLER_COUNT];
        private DS4State[] CurrentState = new DS4State[DS4_CONTROLLER_COUNT];
        private DS4State[] PreviousState = new DS4State[DS4_CONTROLLER_COUNT];
        public DS4StateExposed[] ExposedState = new DS4StateExposed[DS4_CONTROLLER_COUNT];
        public bool recordingMacro = false;
        public event EventHandler<DebugEventArgs> Debug = null;
        public bool eastertime = false;
        private int eCode = 0;
        bool[] buttonsdown = { false, false, false, false };
        List<DS4Controls> dcs = new List<DS4Controls>();
        bool[] held = new bool[DS4_CONTROLLER_COUNT];
        int[] oldmouse = new int[DS4_CONTROLLER_COUNT] { -1, -1, -1, -1 };
        SoundPlayer sp = new SoundPlayer();

        private class X360Data
        {
            public byte[] Report = new byte[28];
            public byte[] Rumble = new byte[8];
        }

        private X360Data[] processingData = new X360Data[4];

        public ControlService()
        {
            sp.Stream = Properties.Resources.EE;
            x360Bus = new X360Device();
            AddtoDS4List();

            for (int i = 0, arlength = DS4Controllers.Length; i < arlength; i++)
            {
                processingData[i] = new X360Data();
                MappedState[i] = new DS4State();
                CurrentState[i] = new DS4State();
                PreviousState[i] = new DS4State();
                ExposedState[i] = new DS4StateExposed(CurrentState[i]);
            }
        }

        void AddtoDS4List()
        {
            dcs.Add(DS4Controls.Cross);
            dcs.Add(DS4Controls.Circle);
            dcs.Add(DS4Controls.Square);
            dcs.Add(DS4Controls.Triangle);
            dcs.Add(DS4Controls.Options);
            dcs.Add(DS4Controls.Share);
            dcs.Add(DS4Controls.DpadUp);
            dcs.Add(DS4Controls.DpadDown);
            dcs.Add(DS4Controls.DpadLeft);
            dcs.Add(DS4Controls.DpadRight);
            dcs.Add(DS4Controls.PS);
            dcs.Add(DS4Controls.L1);
            dcs.Add(DS4Controls.R1);
            dcs.Add(DS4Controls.L2);
            dcs.Add(DS4Controls.R2);
            dcs.Add(DS4Controls.L3);
            dcs.Add(DS4Controls.R3);
            dcs.Add(DS4Controls.LXPos);
            dcs.Add(DS4Controls.LXNeg);
            dcs.Add(DS4Controls.LYPos);
            dcs.Add(DS4Controls.LYNeg);
            dcs.Add(DS4Controls.RXPos);
            dcs.Add(DS4Controls.RXNeg);
            dcs.Add(DS4Controls.RYPos);
            dcs.Add(DS4Controls.RYNeg);
            dcs.Add(DS4Controls.SwipeUp);
            dcs.Add(DS4Controls.SwipeDown);
            dcs.Add(DS4Controls.SwipeLeft);
            dcs.Add(DS4Controls.SwipeRight);
        }

        private async void WarnExclusiveModeFailure(DS4Device device)
        {
            if (DS4Devices.isExclusiveMode && !device.isExclusive())
            {
                await System.Threading.Tasks.Task.Delay(5);
                String message = Properties.Resources.CouldNotOpenDS4.Replace("*Mac address*", device.getMacAddress()) + " " + Properties.Resources.QuitOtherPrograms;
                LogDebug(message, true);
                Log.LogToTray(message, true);
            }
        }

        public bool Start(bool showlog = true)
        {
            if (x360Bus.Open() && x360Bus.Start())
            {
                if (showlog)
                    LogDebug(Properties.Resources.Starting);

                DS4Devices.isExclusiveMode = getUseExclusiveMode();
                if (showlog)
                {
                    LogDebug(Properties.Resources.SearchingController);
                    LogDebug(DS4Devices.isExclusiveMode ? Properties.Resources.UsingExclusive : Properties.Resources.UsingShared);
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
                        DS4Device device = devices.ElementAt<DS4Device>(i);
                        if (showlog)
                            LogDebug(Properties.Resources.FoundController + device.getMacAddress() + " (" + device.getConnectionType() + ")");

                        WarnExclusiveModeFailure(device);
                        DS4Controllers[i] = device;
                        device.Removal -= DS4Devices.On_Removal;
                        device.Removal += this.On_DS4Removal;
                        device.Removal += DS4Devices.On_Removal;
                        touchPad[i] = new Mouse(i, device);
                        device.LightBarColor = getMainColor(i);

                        if (!getDInputOnly(i))
                            x360Bus.Plugin(i);

                        device.Report += this.On_Report;
                        TouchPadOn(i, device);
                        //string filename = ProfilePath[ind];
                        //ind++;
                        if (showlog)
                        {
                            if (System.IO.File.Exists(appdatapath + "\\Profiles\\" + ProfilePath[i] + ".xml"))
                            {
                                string prolog = Properties.Resources.UsingProfile.Replace("*number*", (i + 1).ToString()).Replace("*Profile name*", ProfilePath[i]);
                                LogDebug(prolog);
                                Log.LogToTray(prolog);
                            }
                            else
                            {
                                string prolog = Properties.Resources.NotUsingProfile.Replace("*number*", (i + 1).ToString());
                                LogDebug(prolog);
                                Log.LogToTray(prolog);
                            }
                        }

                        if (i >= 4) // out of Xinput devices!
                            break;
                    }
                }
                catch (Exception e)
                {
                    LogDebug(e.Message);
                    Log.LogToTray(e.Message);
                }

                running = true;
            }
            else
            {
                string logMessage = "Could not connect to Scp Virtual Bus Driver. Please check the status of the System device in Device Manager";
                LogDebug(logMessage);
                Log.LogToTray(logMessage);
            }

            ControllerStatusChanged(this);
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

                bool anyUnplugged = false;                
                for (int i = 0, arlength = DS4Controllers.Length; i < arlength; i++)
                {
                    if (DS4Controllers[i] != null)
                    {
                        if (DCBTatStop && !DS4Controllers[i].isCharging() && showlog)
                        {
                            if (DS4Controllers[i].getConnectionType() == ConnectionType.BT)
                            {
                                DS4Controllers[i].DisconnectBT();
                            }
                            else if (DS4Controllers[i].getConnectionType() == ConnectionType.SONYWA)
                            {
                                DS4Controllers[i].DisconnectDongle(true);
                            }
                        }
                        else
                        {
                            DS4LightBar.forcelight[i] = false;
                            DS4LightBar.forcedFlash[i] = 0;
                            DS4LightBar.defaultLight = true;
                            DS4LightBar.updateLightBar(DS4Controllers[i], i, CurrentState[i], ExposedState[i], touchPad[i]);
                            System.Threading.Thread.Sleep(50);
                        }
                        CurrentState[i].Battery = PreviousState[i].Battery = 0; // Reset for the next connection's initial status change.
                        x360Bus.Unplug(i);
                        anyUnplugged = true;
                        DS4Controllers[i] = null;
                        touchPad[i] = null;
                    }
                }

                if (anyUnplugged)
                    System.Threading.Thread.Sleep(XINPUT_UNPLUG_SETTLE_TIME);

                x360Bus.UnplugAll();
                x360Bus.Stop();

                if (showlog)
                    LogDebug(Properties.Resources.StoppingDS4);

                DS4Devices.stopControllers();
                if (showlog)
                    LogDebug(Properties.Resources.StoppedDS4Windows);

                ControllerStatusChanged(this);
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
                    DS4Device device = devices.ElementAt<DS4Device>(i);

                    if (device.isDisconnectingStatus())
                        continue;

                    if (getQuickCharge() && device?.getConnectionType() == ConnectionType.BT &&
                        (bool)device?.isCharging())
                    {
                        device.DisconnectBT();
                        continue;
                    }

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
                            WarnExclusiveModeFailure(device);
                            DS4Controllers[Index] = device;
                            device.Removal -= DS4Devices.On_Removal;
                            device.Removal += this.On_DS4Removal;
                            device.Removal += DS4Devices.On_Removal;
                            touchPad[Index] = new Mouse(Index, device);
                            device.LightBarColor = getMainColor(Index);
                            device.Report += this.On_Report;
                            if (!getDInputOnly(Index))
                                x360Bus.Plugin(Index);
                            TouchPadOn(Index, device);

                            //string filename = Path.GetFileName(ProfilePath[Index]);
                            if (System.IO.File.Exists(appdatapath + "\\Profiles\\" + ProfilePath[Index] + ".xml"))
                            {
                                string prolog = Properties.Resources.UsingProfile.Replace("*number*", (Index + 1).ToString()).Replace("*Profile name*", ProfilePath[Index]);
                                LogDebug(prolog);
                                Log.LogToTray(prolog);
                            }
                            else
                            {
                                string prolog = Properties.Resources.NotUsingProfile.Replace("*number*", (Index + 1).ToString());
                                LogDebug(prolog);
                                Log.LogToTray(prolog);
                            }

                            break;
                        }
                    }
                }

                ControllerStatusChanged(this);
            }

            return true;
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
            device.SixAxis.SixAccelMoved += tPad.sixaxisMoved;
            //LogDebug("Touchpad mode for " + device.MacAddress + " is now " + tmode.ToString());
            //Log.LogToTray("Touchpad mode for " + device.MacAddress + " is now " + tmode.ToString());
            ControllerStatusChanged(this);
        }

        public void TimeoutConnection(DS4Device d)
        {
            try
            {
                System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
                sw.Start();
                while (!d.IsAlive())
                {
                    if (sw.ElapsedMilliseconds < 1000)
                        System.Threading.Thread.SpinWait(500); 
                        //If weve been waiting less than 1 second let the thread keep its processing chunk
                    else
                        System.Threading.Thread.Sleep(500); 
                    //If weve been waiting more than 1 second give up some resources

                    if (sw.ElapsedMilliseconds > 5000) throw new TimeoutException(); //Weve waited long enough
                }
                sw.Reset();
            }
            catch (TimeoutException)
            {
                Stop(false);
                Start(false);
            }
        }

        public string getDS4ControllerInfo(int index)
        {
            if (DS4Controllers[index] != null)
            {
                DS4Device d = DS4Controllers[index];
                if (!d.IsAlive())
                    //return "Connecting..."; // awaiting the first battery charge indication
                {
                    var TimeoutThread = new System.Threading.Thread(() => TimeoutConnection(d));
                    TimeoutThread.IsBackground = true;
                    TimeoutThread.Name = "TimeoutFor" + d.getMacAddress().ToString();
                    TimeoutThread.Start();
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

                return d.getMacAddress() + " (" + d.ConnectionType + "), " + battery;
                //return d.MacAddress + " (" + d.ConnectionType + "), Battery is " + battery + ", Touchpad in " + modeSwitcher[index].ToString();
            }
            else
                return string.Empty;
        }

        public string getDS4MacAddress(int index)
        {
            if (DS4Controllers[index] != null)
            {
                DS4Device d = DS4Controllers[index];
                if (!d.IsAlive())
                //return "Connecting..."; // awaiting the first battery charge indication
                {
                    var TimeoutThread = new System.Threading.Thread(() => TimeoutConnection(d));
                    TimeoutThread.IsBackground = true;
                    TimeoutThread.Name = "TimeoutFor" + d.getMacAddress().ToString();
                    TimeoutThread.Start();
                    return Properties.Resources.Connecting;
                }
                return d.getMacAddress();
            }
            else
                return string.Empty;
        }

        public string getShortDS4ControllerInfo(int index)
        {
            if (DS4Controllers[index] != null)
            {
                DS4Device d = DS4Controllers[index];
                String battery;
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
            if (DS4Controllers[index] != null)
            {
                DS4Device d = DS4Controllers[index];
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
            if (DS4Controllers[index] != null)
            {
                DS4Device d = DS4Controllers[index];
                return d.getConnectionType() + "";
            }
            else
                return Properties.Resources.NoneText;
        }

        private int XINPUT_UNPLUG_SETTLE_TIME = 250; // Inhibit races that occur with the asynchronous teardown of ScpVBus -> X360 driver instance.
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
                    x360Bus.Unplug(ind);
                    string removed = Properties.Resources.ControllerWasRemoved.Replace("*Mac address*", (ind + 1).ToString());
                    if (device.getBattery() <= 20 &&
                        device.getConnectionType() == ConnectionType.BT && !device.isCharging())
                        removed += ". " + Properties.Resources.ChargeController;

                    LogDebug(removed);
                    Log.LogToTray(removed);
                    System.Threading.Thread.Sleep(XINPUT_UNPLUG_SETTLE_TIME);
                    device.IsRemoved = true;
                    DS4Controllers[ind] = null;
                    touchPad[ind] = null;
                    lag[ind] = false;
                    inWarnMonitor[ind] = false;
                    OnControllerRemoved(this, ind);
                    //ControllerStatusChanged(this);
                }
            }
        }
        public bool[] lag = { false, false, false, false };
        public bool[] inWarnMonitor = { false, false, false, false };
        //Called every time the new input report has arrived
        protected virtual void On_Report(object sender, EventArgs e)
        {
            DS4Device device = (DS4Device)sender;

            int ind = -1;
            for (int i = 0, arlength = DS4_CONTROLLER_COUNT; ind == -1 && i < arlength; i++)
            {
                DS4Device tempDev = DS4Controllers[i];
                if (tempDev != null && device == tempDev)
                    ind = i;
            }

            if (ind != -1)
            {
                if (getFlushHIDQueue(ind))
                    device.FlushHID();

                if (!string.IsNullOrEmpty(device.error))
                {
                    LogDebug(device.error);
                }

                if (inWarnMonitor[ind])
                {
                    int flashWhenLateAt = getFlashWhenLateAt();
                    if (!lag[ind] && device.Latency >= flashWhenLateAt)
                        LagFlashWarning(ind, true);
                    else if (lag[ind] && device.Latency < flashWhenLateAt)
                        LagFlashWarning(ind, false);
                }
                else
                {
                    if (DateTime.UtcNow - device.firstActive > TimeSpan.FromSeconds(5))
                    {
                        inWarnMonitor[ind] = true;
                    }
                }

                device.getExposedState(ExposedState[ind], CurrentState[ind]);
                DS4State cState = CurrentState[ind];
                device.getPreviousState(PreviousState[ind]);
                DS4State pState = PreviousState[ind];

                if (pState.Battery != cState.Battery)
                    OnBatteryStatusChange(this, ind, cState.Battery);
                    //ControllerStatusChanged(this);

                CheckForHotkeys(ind, cState, pState);

                // Temporarily disable easter time routine
                //if (eastertime)
                //    EasterTime(ind);

                cState = Mapping.SetCurveAndDeadzone(ind, cState);

                if (!recordingMacro && (!string.IsNullOrEmpty(tempprofilename[ind]) ||
                    containsCustomAction(ind) || containsCustomExtras(ind) ||
                    getProfileActionCount(ind) > 0))
                {
                    Mapping.MapCustom(ind, cState, MappedState[ind], ExposedState[ind], touchPad[ind], this);
                    cState = MappedState[ind];
                }

                // Update the GUI/whatever.
                DS4LightBar.updateLightBar(device, ind, cState, ExposedState[ind], touchPad[ind]);

                x360Bus.Parse(cState, processingData[ind].Report, ind);
                // We push the translated Xinput state, and simultaneously we
                // pull back any possible rumble data coming from Xinput consumers.
                if (x360Bus.Report(processingData[ind].Report, processingData[ind].Rumble))
                {
                    Byte Big = (Byte)(processingData[ind].Rumble[3]);
                    Byte Small = (Byte)(processingData[ind].Rumble[4]);

                    if (processingData[ind].Rumble[1] == 0x08)
                    {
                        setRumble(Big, Small, ind);
                    }
                }

                // Output any synthetic events.
                Mapping.Commit(ind);
                // Pull settings updates.
                device.IdleTimeout = getIdleDisconnectTimeout(ind);
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

        public void EasterTime(int ind)
        {
            DS4State cState = CurrentState[ind];
            DS4StateExposed eState = ExposedState[ind];
            Mouse tp = touchPad[ind];

            bool pb = false;
            //foreach (DS4Controls dc in dcs)
            for (int i = 0, dcslen = dcs.Count; i < dcslen; i++)
            {
                DS4Controls dc = dcs[i];
                if (Mapping.getBoolMapping(ind, dc, cState, eState, tp))
                {
                    pb = true;
                    break;
                }
            }

            int temp = eCode;
            //Looks like you found the easter egg code, since you're already cheating,
            //I scrambled the code for you :)
            if (pb && !buttonsdown[ind])
            {
                if (cState.Cross && eCode == 9)
                    eCode++;
                else if (!cState.Cross && eCode == 9)
                    eCode = 0;
                else if (cState.DpadLeft && eCode == 6)
                    eCode++;
                else if (!cState.DpadLeft && eCode == 6)
                    eCode = 0;
                else if (cState.DpadRight && eCode == 7)
                    eCode++;
                else if (!cState.DpadRight && eCode == 7)
                    eCode = 0;
                else if (cState.DpadLeft && eCode == 4)
                    eCode++;
                else if (!cState.DpadLeft && eCode == 4)
                    eCode = 0;
                else if (cState.DpadDown && eCode == 2)
                    eCode++;
                else if (!cState.DpadDown && eCode == 2)
                    eCode = 0;
                else if (cState.DpadRight && eCode == 5)
                    eCode++;
                else if (!cState.DpadRight && eCode == 5)
                    eCode = 0;
                else if (cState.DpadUp && eCode == 1)
                    eCode++;
                else if (!cState.DpadUp && eCode == 1)
                    eCode = 0;
                else if (cState.DpadDown && eCode == 3)
                    eCode++;
                else if (!cState.DpadDown && eCode == 3)
                    eCode = 0;
                else if (cState.Circle && eCode == 8)
                    eCode++;
                else if (!cState.Circle && eCode == 8)
                    eCode = 0;

                if (cState.DpadUp && eCode == 0)
                    eCode++;

                if (eCode == 10)
                {
                    string message = "(!)";
                    sp.Play();
                    LogDebug(message, true);
                    eCode = 0;
                }

                if (temp != eCode)
                    Console.WriteLine(eCode);

                buttonsdown[ind] = true;
            }
            else if (!pb)
                buttonsdown[ind] = false;
        }

        public string GetInputkeys(int ind)
        {
            DS4State cState = CurrentState[ind];
            DS4StateExposed eState = ExposedState[ind];
            Mouse tp = touchPad[ind];
            string result = "nothing";

            if (DS4Controllers[ind] != null)
            {
                if (Mapping.getBoolButtonMapping(cState.Cross))
                {
                    result = "Cross";
                }
                else if (Mapping.getBoolButtonMapping(cState.Circle))
                {
                    result = "Circle";
                }
                else if (Mapping.getBoolButtonMapping(cState.Triangle))
                {
                    result = "Triangle";
                }
                else if (Mapping.getBoolButtonMapping(cState.Square))
                {
                    result = "Triangle";
                }
                else if (Mapping.getBoolButtonMapping(cState.L1))
                {
                    result = "L1";
                }
                else if (Mapping.getBoolTriggerMapping(cState.L2))
                {
                    result = "L2";
                }
                else if (Mapping.getBoolButtonMapping(cState.L3))
                {
                    result = "L3";
                }
                else if (Mapping.getBoolButtonMapping(cState.R1))
                {
                    result = "R1";
                }
                else if (Mapping.getBoolTriggerMapping(cState.R2))
                {
                    result = "R2";
                }
                else if (Mapping.getBoolButtonMapping(cState.R3))
                {
                    result = "R3";
                }
                else if (Mapping.getBoolButtonMapping(cState.DpadUp))
                {
                    result = "Up";
                }
                else if (Mapping.getBoolButtonMapping(cState.DpadDown))
                {
                    result = "Down";
                }
                else if (Mapping.getBoolButtonMapping(cState.DpadLeft))
                {
                    result = "Left";
                }
                else if (Mapping.getBoolButtonMapping(cState.DpadRight))
                {
                    result = "DpadRight";
                }
                else if (Mapping.getBoolButtonMapping(cState.Share))
                {
                    result = "Share";
                }
                else if (Mapping.getBoolButtonMapping(cState.Options))
                {
                    result = "Options";
                }
                else if (Mapping.getBoolButtonMapping(cState.PS))
                {
                    result = "PS";
                }
                else if (Mapping.getBoolAxisDirMapping(cState.LX, true))
                {
                    result = "LS Right";
                }
                else if (Mapping.getBoolAxisDirMapping(cState.LX, false))
                {
                    result = "LS Left";
                }
                else if (Mapping.getBoolAxisDirMapping(cState.LY, true))
                {
                    result = "LS Down";
                }
                else if (Mapping.getBoolAxisDirMapping(cState.LY, false))
                {
                    result = "LS Up";
                }
                else if (Mapping.getBoolAxisDirMapping(cState.RX, true))
                {
                    result = "RS Right";
                }
                else if (Mapping.getBoolAxisDirMapping(cState.RX, false))
                {
                    result = "RS Left";
                }
                else if (Mapping.getBoolAxisDirMapping(cState.RY, true))
                {
                    result = "RS Down";
                }
                else if (Mapping.getBoolAxisDirMapping(cState.RY, false))
                {
                    result = "RS Up";
                }
                else if (Mapping.getBoolTouchMapping(tp.leftDown))
                {
                    result = "Touch Left";
                }
                else if (Mapping.getBoolTouchMapping(tp.rightDown))
                {
                    result = "Touch Right";
                }
                else if (Mapping.getBoolTouchMapping(tp.multiDown))
                {
                    result = "Touch Multi";
                }
                else if (Mapping.getBoolTouchMapping(tp.upperDown))
                {
                    result = "Touch Upper";
                }
            }

            return result;
        }

        public DS4Controls GetInputkeysDS4(int ind)
        {
            DS4State cState = CurrentState[ind];
            DS4StateExposed eState = ExposedState[ind];
            Mouse tp = touchPad[ind];
            DS4Controls result = DS4Controls.None;

            if (DS4Controllers[ind] != null)
            {
                if (Mapping.getBoolButtonMapping(cState.Cross))
                {
                    result = DS4Controls.Cross;
                }
                else if (Mapping.getBoolButtonMapping(cState.Circle))
                {
                    result = DS4Controls.Circle;
                }
                else if (Mapping.getBoolButtonMapping(cState.Triangle))
                {
                    result = DS4Controls.Triangle;
                }
                else if (Mapping.getBoolButtonMapping(cState.Square))
                {
                    result = DS4Controls.Square;
                }
                else if (Mapping.getBoolButtonMapping(cState.L1))
                {
                    result = DS4Controls.L1;
                }
                else if (Mapping.getBoolTriggerMapping(cState.L2))
                {
                    result = DS4Controls.L2;
                }
                else if (Mapping.getBoolButtonMapping(cState.L3))
                {
                    result = DS4Controls.L3;
                }
                else if (Mapping.getBoolButtonMapping(cState.R1))
                {
                    result = DS4Controls.R1;
                }
                else if (Mapping.getBoolTriggerMapping(cState.R2))
                {
                    result = DS4Controls.R2;
                }
                else if (Mapping.getBoolButtonMapping(cState.R3))
                {
                    result = DS4Controls.R3;
                }
                else if (Mapping.getBoolButtonMapping(cState.DpadUp))
                {
                    result = DS4Controls.DpadUp;
                }
                else if (Mapping.getBoolButtonMapping(cState.DpadDown))
                {
                    result = DS4Controls.DpadDown;
                }
                else if (Mapping.getBoolButtonMapping(cState.DpadLeft))
                {
                    result = DS4Controls.DpadLeft;
                }
                else if (Mapping.getBoolButtonMapping(cState.DpadRight))
                {
                    result = DS4Controls.DpadRight;
                }
                else if (Mapping.getBoolButtonMapping(cState.Share))
                {
                    result = DS4Controls.Share;
                }
                else if (Mapping.getBoolButtonMapping(cState.Options))
                {
                    result = DS4Controls.Options;
                }
                else if (Mapping.getBoolButtonMapping(cState.PS))
                {
                    result = DS4Controls.PS;
                }
                else if (Mapping.getBoolAxisDirMapping(cState.LX, true))
                {
                    result = DS4Controls.LXPos;
                }
                else if (Mapping.getBoolAxisDirMapping(cState.LX, false))
                {
                    result = DS4Controls.LXNeg;
                }
                else if (Mapping.getBoolAxisDirMapping(cState.LY, true))
                {
                    result = DS4Controls.LYPos;
                }
                else if (Mapping.getBoolAxisDirMapping(cState.LY, false))
                {
                    result = DS4Controls.LYNeg;
                }
                else if (Mapping.getBoolAxisDirMapping(cState.RX, true))
                {
                    result = DS4Controls.RXPos;
                }
                else if (Mapping.getBoolAxisDirMapping(cState.RX, false))
                {
                    result = DS4Controls.RXNeg;
                }
                else if (Mapping.getBoolAxisDirMapping(cState.RY, true))
                {
                    result = DS4Controls.RYPos;
                }
                else if (Mapping.getBoolAxisDirMapping(cState.RY, false))
                {
                    result = DS4Controls.RYNeg;
                }
                else if (Mapping.getBoolTouchMapping(tp.leftDown))
                {
                    result = DS4Controls.TouchLeft;
                }
                else if (Mapping.getBoolTouchMapping(tp.rightDown))
                {
                    result = DS4Controls.TouchRight;
                }
                else if (Mapping.getBoolTouchMapping(tp.multiDown))
                {
                    result = DS4Controls.TouchMulti;
                }
                else if (Mapping.getBoolTouchMapping(tp.upperDown))
                {
                    result = DS4Controls.TouchUpper;
                }
            }

            return result;
        }

        public bool[] touchreleased = { true, true, true, true }, touchslid = { false, false, false, false };
        public byte[] oldtouchvalue = { 0, 0, 0, 0 };
        public int[] oldscrollvalue = { 0, 0, 0, 0 };

        protected virtual void CheckForHotkeys(int deviceID, DS4State cState, DS4State pState)
        {
            if (!getUseTPforControls(deviceID) && cState.Touch1 && pState.PS)
            {
                if (getTouchSensitivity(deviceID) > 0 && touchreleased[deviceID])
                {
                    oldtouchvalue[deviceID] = getTouchSensitivity(deviceID);
                    oldscrollvalue[deviceID] = getScrollSensitivity(deviceID);
                    getTouchSensitivity()[deviceID] = 0;
                    getScrollSensitivity()[deviceID] = 0;
                    LogDebug(getTouchSensitivity(deviceID) > 0 ? Properties.Resources.TouchpadMovementOn : Properties.Resources.TouchpadMovementOff);
                    Log.LogToTray(getTouchSensitivity(deviceID) > 0 ? Properties.Resources.TouchpadMovementOn : Properties.Resources.TouchpadMovementOff);
                    touchreleased[deviceID] = false;
                }
                else if (touchreleased[deviceID])
                {
                    getTouchSensitivity()[deviceID] = oldtouchvalue[deviceID];
                    getScrollSensitivity()[deviceID] = oldscrollvalue[deviceID];
                    LogDebug(getTouchSensitivity(deviceID) > 0 ? Properties.Resources.TouchpadMovementOn : Properties.Resources.TouchpadMovementOff);
                    Log.LogToTray(getTouchSensitivity(deviceID) > 0 ? Properties.Resources.TouchpadMovementOn : Properties.Resources.TouchpadMovementOff);
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
            Console.WriteLine(System.DateTime.Now.ToString("G") + "> " + Data);
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
                if (DS4Controllers[deviceNum] != null)
                    DS4Controllers[deviceNum].setRumble((byte)lightBoosted, (byte)heavyBoosted);
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
