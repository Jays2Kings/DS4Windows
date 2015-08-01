using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;
using System.Reflection;
using System.Media;
using System.Threading.Tasks;
namespace EAll4Windows
{
    public class ControlService
    {
        public X360Device x360Bus;
        public List<IEAll4Device> controllers = new List<IEAll4Device>(4);
        public Mouse[] touchPad = new Mouse[4];
        private bool running = false;
        private ControllerState[] MappedState = new ControllerState[4];
        private ControllerState[] CurrentState = new ControllerState[4];
        private ControllerState[] PreviousState = new ControllerState[4];
        public EAll4StateExposed[] ExposedState = new EAll4StateExposed[4];
        public bool recordingMacro = false;
        public event EventHandler<DebugEventArgs> Debug = null;
        public bool eastertime = false;
        private int eCode = 0;
        bool[] buttonsdown = { false, false, false, false };
        List<GenericControls> dcs = new List<GenericControls>();
        bool[] held = new bool[4];
        int[] oldmouse = new int[4] { -1, -1, -1, -1 };
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
            AddtoEAll4List();
            for (int i = 0; i < controllers.Count; i++)
            {
                processingData[i] = new X360Data();
                MappedState[i] = new ControllerState();
                CurrentState[i] = new ControllerState();
                PreviousState[i] = new ControllerState();
                ExposedState[i] = new EAll4StateExposed(CurrentState[i]);
            }
        }

        void AddtoEAll4List()
        {
            dcs.Add(GenericControls.A);
            dcs.Add(GenericControls.B);
            dcs.Add(GenericControls.X);
            dcs.Add(GenericControls.Y);
            dcs.Add(GenericControls.Start);
            dcs.Add(GenericControls.Back);
            dcs.Add(GenericControls.DpadUp);
            dcs.Add(GenericControls.DpadDown);
            dcs.Add(GenericControls.DpadLeft);
            dcs.Add(GenericControls.DpadRight);
            dcs.Add(GenericControls.Guide);
            dcs.Add(GenericControls.LB);
            dcs.Add(GenericControls.RB);
            dcs.Add(GenericControls.LT);
            dcs.Add(GenericControls.RT);
            dcs.Add(GenericControls.LS);
            dcs.Add(GenericControls.RS);
            dcs.Add(GenericControls.LXPos);
            dcs.Add(GenericControls.LXNeg);
            dcs.Add(GenericControls.LYPos);
            dcs.Add(GenericControls.LYNeg);
            dcs.Add(GenericControls.RXPos);
            dcs.Add(GenericControls.RXNeg);
            dcs.Add(GenericControls.RYPos);
            dcs.Add(GenericControls.RYNeg);
            dcs.Add(GenericControls.SwipeUp);
            dcs.Add(GenericControls.SwipeDown);
            dcs.Add(GenericControls.SwipeLeft);
            dcs.Add(GenericControls.SwipeRight);
        }

        private async void WarnExclusiveModeFailure(IEAll4Device ieAll4Device)
        {
            if (EAll4Devices.isExclusiveMode && !ieAll4Device.IsExclusive)
            {
                await System.Threading.Tasks.Task.Delay(5);
                String message = Properties.Resources.CouldNotOpenEAll4.Replace("*Mac address*", ieAll4Device.MacAddress) + " " + Properties.Resources.QuitOtherPrograms;
                LogDebug(message, true);
                Log.LogToTray(message);
            }
        }
        public bool Start(bool showlog = true)
        {
            if (x360Bus.Open() && x360Bus.Start())
            {
                if (showlog)
                    LogDebug(Properties.Resources.Starting);
                EAll4Devices.isExclusiveMode = Global.UseExclusiveMode;
                if (showlog)
                {
                    LogDebug(Properties.Resources.SearchingController);
                    LogDebug(EAll4Devices.isExclusiveMode ? Properties.Resources.UsingExclusive : Properties.Resources.UsingShared);
                }
                try
                {
                    EAll4Devices.findControllers();
                    IEnumerable<IEAll4Device> devices = EAll4Devices.getEAll4Controllers();
                    int ind = 0;
                    EAll4LightBar.defaultLight = false;
                    foreach (var device in devices)
                    {
                        if (showlog)
                            LogDebug(Properties.Resources.FoundController + device.MacAddress + " (" + device.ConnectionType + ")");
                        WarnExclusiveModeFailure(device);
                        controllers[ind] = device;
                        device.Removal -= EAll4Devices.On_Removal;
                        device.Removal += this.On_EAll4Removal;
                        device.Removal += EAll4Devices.On_Removal;
                        touchPad[ind] = new Mouse(ind, device);
                        device.LightBarColor = Global.MainColor[ind];
                        if (!Global.DinputOnly[ind])
                            x360Bus.Plugin(ind);
                        device.Report += this.On_Report;
                        TouchPadOn(ind, device);
                        //string filename = Global.ProfilePath[ind];
                        ind++;
                        if (showlog)
                            if (System.IO.File.Exists(Global.appdatapath + "\\Profiles\\" + Global.ProfilePath[ind - 1] + ".xml"))
                            {
                                string prolog = Properties.Resources.UsingProfile.Replace("*number*", ind.ToString()).Replace("*Profile name*", Global.ProfilePath[ind - 1]);
                                LogDebug(prolog);
                                Log.LogToTray(prolog);
                            }
                            else
                            {
                                string prolog = Properties.Resources.NotUsingProfile.Replace("*number*", (ind).ToString());
                                LogDebug(prolog);
                                Log.LogToTray(prolog);
                            }
                        if (ind >= 4) // out of Xinput devices!
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
            return true;
        }

        public bool Stop(bool showlog = true)
        {
            if (running)
            {
                running = false;
                if (showlog)
                    LogDebug(Properties.Resources.StoppingX360);
                bool anyUnplugged = false;
                for (int i = 0; i < controllers.Count; i++)
                {
                    if (controllers[i] != null)
                    {
                        if (Global.DCBTatStop && !controllers[i].Charging && showlog)
                            controllers[i].DisconnectBT();
                        else
                        {
                            EAll4LightBar.forcelight[i] = false;
                            EAll4LightBar.forcedFlash[i] = 0;
                            EAll4LightBar.defaultLight = true;
                            EAll4LightBar.updateLightBar(controllers[i], i, CurrentState[i], ExposedState[i], touchPad[i]);
                            System.Threading.Thread.Sleep(50);
                        }
                        CurrentState[i].Battery = PreviousState[i].Battery = 0; // Reset for the next connection's initial status change.
                        x360Bus.Unplug(i);
                        anyUnplugged = true;
                        controllers[i] = null;
                        touchPad[i] = null;
                    }
                }
                if (anyUnplugged)
                    System.Threading.Thread.Sleep(XINPUT_UNPLUG_SETTLE_TIME);
                x360Bus.UnplugAll();
                x360Bus.Stop();
                if (showlog)
                    LogDebug(Properties.Resources.StoppingEAll4);
                EAll4Devices.stopControllers();
                if (showlog)
                    LogDebug(Properties.Resources.StoppedEAll4Windows);
                Global.ControllerStatusChanged(this);
            }
            return true;
        }

        public bool HotPlug()
        {
            if (running)
            {
                EAll4Devices.findControllers();
                IEnumerable<IEAll4Device> devices = EAll4Devices.getEAll4Controllers();
                foreach (Ds4Device device in devices)
                {
                    if (device.IsDisconnecting)
                        continue;
                    if (((Func<bool>)delegate
                    {
                        for (Int32 Index = 0; Index < controllers.Count; Index++)
                            if (controllers[Index] != null && controllers[Index].MacAddress == device.MacAddress)
                                return true;
                        return false;
                    })())
                        continue;
                    for (Int32 Index = 0; Index < controllers.Count; Index++)
                        if (controllers[Index] == null)
                        {
                            LogDebug(Properties.Resources.FoundController + device.MacAddress + " (" + device.ConnectionType + ")");
                            WarnExclusiveModeFailure(device);
                            controllers[Index] = device;
                            device.Removal -= EAll4Devices.On_Removal;
                            device.Removal += this.On_EAll4Removal;
                            device.Removal += EAll4Devices.On_Removal;
                            touchPad[Index] = new Mouse(Index, device);
                            device.LightBarColor = Global.MainColor[Index];
                            device.Report += this.On_Report;
                            if (!Global.DinputOnly[Index])
                                x360Bus.Plugin(Index);
                            TouchPadOn(Index, device);
                            //string filename = Path.GetFileName(Global.ProfilePath[Index]);
                            if (System.IO.File.Exists(Global.appdatapath + "\\Profiles\\" + Global.ProfilePath[Index] + ".xml"))
                            {
                                string prolog = Properties.Resources.UsingProfile.Replace("*number*", (Index + 1).ToString()).Replace("*Profile name*", Global.ProfilePath[Index]);
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
            return true;
        }

        public void TouchPadOn(int ind, IEAll4Device ieAll4Device)
        {
            ITouchpadBehaviour tPad = touchPad[ind];
            ieAll4Device.Touchpad.TouchButtonDown += tPad.touchButtonDown;
            ieAll4Device.Touchpad.TouchButtonUp += tPad.touchButtonUp;
            ieAll4Device.Touchpad.TouchesBegan += tPad.touchesBegan;
            ieAll4Device.Touchpad.TouchesMoved += tPad.touchesMoved;
            ieAll4Device.Touchpad.TouchesEnded += tPad.touchesEnded;
            ieAll4Device.Touchpad.TouchUnchanged += tPad.touchUnchanged;
            //LogDebug("Touchpad mode for " + device.MacAddress + " is now " + tmode.ToString());
            //Log.LogToTray("Touchpad mode for " + device.MacAddress + " is now " + tmode.ToString());
            Global.ControllerStatusChanged(this);
        }

        public void TimeoutConnection(IEAll4Device d)
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

        public string getEAll4ControllerInfo(int index)
        {
            if (controllers[index] != null)
            {
                IEAll4Device d = controllers[index];
                if (!d.IsAlive())
                //return "Connecting..."; // awaiting the first battery charge indication
                {
                    var TimeoutThread = new System.Threading.Thread(() => TimeoutConnection(d));
                    TimeoutThread.IsBackground = true;
                    TimeoutThread.Name = "TimeoutFor" + d.MacAddress.ToString();
                    TimeoutThread.Start();
                    return Properties.Resources.Connecting;
                }
                String battery;
                if (d.Charging)
                {
                    if (d.Battery >= 100)
                        battery = Properties.Resources.Charged;
                    else
                        battery = Properties.Resources.Charging.Replace("*number*", d.Battery.ToString());
                }
                else
                {
                    battery = Properties.Resources.Battery.Replace("*number*", d.Battery.ToString());
                }
                return d.MacAddress + " (" + d.ConnectionType + "), " + battery;
                //return d.MacAddress + " (" + d.ConnectionType + "), Battery is " + battery + ", Touchpad in " + modeSwitcher[index].ToString();
            }
            else
                return String.Empty;
        }

        public string getEAll4MacAddress(int index)
        {
            if (controllers[index] == null) return String.Empty;
            var d = controllers[index];
            if (d.IsAlive()) return d.MacAddress;
            var timeoutThread = new System.Threading.Thread(() => TimeoutConnection(d))
            {
                IsBackground = true,
                Name = "TimeoutFor" + d.MacAddress.ToString()
            };
            timeoutThread.Start();
            return Properties.Resources.Connecting;
        }

        public string getShortEAll4ControllerInfo(int index)
        {
            if (controllers[index] != null)
            {
                IEAll4Device d = controllers[index];
                String battery;
                if (!d.IsAlive())
                    battery = "...";
                if (d.Charging)
                {
                    if (d.Battery >= 100)
                        battery = Properties.Resources.Full;
                    else
                        battery = d.Battery + "%+";
                }
                else
                {
                    battery = d.Battery + "%";
                }
                return (d.ConnectionType + " " + battery);
            }
            else
                return Properties.Resources.NoneText;
        }

        public string getEAll4Battery(int index)
        {
            if (controllers[index] != null)
            {
                var d = controllers[index];
                String battery;
                if (!d.IsAlive())
                    battery = "...";
                if (d.Charging)
                {
                    if (d.Battery >= 100)
                        battery = Properties.Resources.Full;
                    else
                        battery = d.Battery + "%+";
                }
                else
                {
                    battery = d.Battery + "%";
                }
                return battery;
            }
            else
                return Properties.Resources.NA;
        }

        public string getEAll4Status(int index)
        {
            if (controllers[index] == null)
                return Properties.Resources.NoneText;
            var d = controllers[index];
            return d.ConnectionType + "";
        }


        private int XINPUT_UNPLUG_SETTLE_TIME = 250; // Inhibit races that occur with the asynchronous teardown of ScpVBus -> X360 driver instance.
        //Called when EAll4 is disconnected or timed out
        protected virtual void On_EAll4Removal(object sender, EventArgs e)
        {
            Ds4Device ieAll4Device = (Ds4Device)sender;
            int ind = -1;
            for (int i = 0; i < controllers.Count; i++)
                if (controllers[i] != null && ieAll4Device.MacAddress == controllers[i].MacAddress)
                    ind = i;
            if (ind != -1)
            {
                CurrentState[ind].Battery = PreviousState[ind].Battery = 0; // Reset for the next connection's initial status change.
                x360Bus.Unplug(ind);
                LogDebug(Properties.Resources.ControllerWasRemoved.Replace("*Mac address*", ieAll4Device.MacAddress));
                Log.LogToTray(Properties.Resources.ControllerWasRemoved.Replace("*Mac address*", ieAll4Device.MacAddress));
                System.Threading.Thread.Sleep(XINPUT_UNPLUG_SETTLE_TIME);
                controllers[ind] = null;
                touchPad[ind] = null;
                Global.ControllerStatusChanged(this);
            }
        }
        public bool[] lag = { false, false, false, false };
        //Called every time the new input report has arrived
        protected virtual void On_Report(object sender, EventArgs e)
        {

            Ds4Device ieAll4Device = (Ds4Device)sender;

            int ind = -1;
            for (int i = 0; i < controllers.Count; i++)
                if (ieAll4Device == controllers[i])
                    ind = i;

            if (ind != -1)
            {
                if (Global.FlushHIDQueue[ind])
                    ieAll4Device.FlushHID();
                if (!string.IsNullOrEmpty(ieAll4Device.error))
                {
                    LogDebug(ieAll4Device.error);
                }
                if (DateTime.UtcNow - ieAll4Device.firstActive > TimeSpan.FromSeconds(5))
                {
                    if (ieAll4Device.Latency >= 10 && !lag[ind])
                        LagFlashWarning(ind, true);
                    else if (ieAll4Device.Latency < 10 && lag[ind])
                        LagFlashWarning(ind, false);
                }
                ieAll4Device.getExposedState(ExposedState[ind], CurrentState[ind]);
                ControllerState cState = CurrentState[ind];
                ieAll4Device.getPreviousState(PreviousState[ind]);
                ControllerState pState = PreviousState[ind];
                if (pState.Battery != cState.Battery)
                    Global.ControllerStatusChanged(this);
                CheckForHotkeys(ind, cState, pState);
                if (eastertime)
                    EasterTime(ind);
                GetInputkeys(ind);
                if (Global.LSCurve[ind] != 0 || Global.RSCurve[ind] != 0 || Global.LSDeadzone[ind] != 0 || Global.RSDeadzone[ind] != 0 ||
                    Global.L2Deadzone[ind] != 0 || Global.R2Deadzone[ind] != 0) //if a curve or deadzone is in place
                    cState = Mapping.SetCurveAndDeadzone(ind, cState);
                if (!recordingMacro && (!string.IsNullOrEmpty(Global.tempprofilename[ind]) ||
                    Global.getHasCustomKeysorButtons(ind) || Global.getHasShiftCustomKeysorButtons(ind) || Global.ProfileActions[ind].Count > 0))
                {
                    Mapping.MapCustom(ind, cState, MappedState[ind], ExposedState[ind], touchPad[ind], this);
                    cState = MappedState[ind];
                }
                if (Global.getHasCustomExtras(ind))
                    DoExtras(ind);

                // Update the GUI/whatever.
                EAll4LightBar.updateLightBar(ieAll4Device, ind, cState, ExposedState[ind], touchPad[ind]);

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
                ieAll4Device.IdleTimeout = Global.IdleDisconnectTimeout[ind];
            }
        }

        public void LagFlashWarning(int ind, bool on)
        {
            if (on)
            {
                lag[ind] = true;
                LogDebug(Properties.Resources.LatencyOverTen.Replace("*number*", (ind + 1).ToString()), true);
                if (Global.FlashWhenLate)
                {
                    EAll4Color color = new EAll4Color { Red = 50, Green = 0, Blue = 0 };
                    EAll4LightBar.forcedColor[ind] = color;
                    EAll4LightBar.forcedFlash[ind] = 2;
                    EAll4LightBar.forcelight[ind] = true;
                }
            }
            else
            {
                lag[ind] = false;
                LogDebug(Properties.Resources.LatencyNotOverTen.Replace("*number*", (ind + 1).ToString()));
                EAll4LightBar.forcelight[ind] = false;
                EAll4LightBar.forcedFlash[ind] = 0;
            }
        }

        private void DoExtras(int ind)
        {
            ControllerState cState = CurrentState[ind];
            EAll4StateExposed eState = ExposedState[ind];
            Mouse tp = touchPad[ind];
            GenericControls helddown = GenericControls.None;
            foreach (KeyValuePair<GenericControls, string> p in Global.getCustomExtras(ind))
            {
                if (Mapping.getBoolMapping(p.Key, cState, eState, tp))
                {
                    helddown = p.Key;
                    break;
                }
            }
            if (helddown != GenericControls.None)
            {
                string p = Global.getCustomExtras(ind)[helddown];
                string[] extraS = p.Split(',');
                int[] extras = new int[extraS.Length];
                for (int i = 0; i < extraS.Length; i++)
                {
                    int b;
                    if (int.TryParse(extraS[i], out b))
                        extras[i] = b;
                }
                held[ind] = true;
                try
                {
                    if (!(extras[0] == extras[1] && extras[1] == 0))
                        setRumble((byte)extras[0], (byte)extras[1], ind);
                    if (extras[2] == 1)
                    {
                        EAll4Color color = new EAll4Color { Red = (byte)extras[3], Green = (byte)extras[4], Blue = (byte)extras[5] };
                        EAll4LightBar.forcedColor[ind] = color;
                        EAll4LightBar.forcedFlash[ind] = (byte)extras[6];
                        EAll4LightBar.forcelight[ind] = true;
                    }
                    if (extras[7] == 1)
                    {
                        if (oldmouse[ind] == -1)
                            oldmouse[ind] = Global.ButtonMouseSensitivity[ind];
                        Global.ButtonMouseSensitivity[ind] = extras[8];
                    }
                }
                catch { }
            }
            else if (held[ind])
            {
                EAll4LightBar.forcelight[ind] = false;
                EAll4LightBar.forcedFlash[ind] = 0;
                Global.ButtonMouseSensitivity[ind] = oldmouse[ind];
                oldmouse[ind] = -1;
                setRumble(0, 0, ind);
                held[ind] = false;
            }
        }



        public void EasterTime(int ind)
        {
            ControllerState cState = CurrentState[ind];
            EAll4StateExposed eState = ExposedState[ind];
            Mouse tp = touchPad[ind];

            bool pb = false;
            foreach (GenericControls dc in dcs)
            {
                if (Mapping.getBoolMapping(dc, cState, eState, tp))
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
                if (cState.A && eCode == 9)
                    eCode++;
                else if (!cState.A && eCode == 9)
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
                else if (cState.B && eCode == 8)
                    eCode++;
                else if (!cState.B && eCode == 8)
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
            ControllerState cState = CurrentState[ind];
            EAll4StateExposed eState = ExposedState[ind];
            Mouse tp = touchPad[ind];
            if (controllers[ind] != null)
                if (Mapping.getBoolMapping(GenericControls.A, cState, eState, tp)) return "A";
                else if (Mapping.getBoolMapping(GenericControls.B, cState, eState, tp)) return "B";
                else if (Mapping.getBoolMapping(GenericControls.Y, cState, eState, tp)) return "Y";
                else if (Mapping.getBoolMapping(GenericControls.X, cState, eState, tp)) return "X";
                else if (Mapping.getBoolMapping(GenericControls.LB, cState, eState, tp)) return "LB";
                else if (Mapping.getBoolMapping(GenericControls.RB, cState, eState, tp)) return "RB";
                else if (Mapping.getBoolMapping(GenericControls.LT, cState, eState, tp)) return "LT";
                else if (Mapping.getBoolMapping(GenericControls.RT, cState, eState, tp)) return "RT";
                else if (Mapping.getBoolMapping(GenericControls.LS, cState, eState, tp)) return "LS";
                else if (Mapping.getBoolMapping(GenericControls.RS, cState, eState, tp)) return "RS";
                else if (Mapping.getBoolMapping(GenericControls.DpadUp, cState, eState, tp)) return "Up";
                else if (Mapping.getBoolMapping(GenericControls.DpadDown, cState, eState, tp)) return "Down";
                else if (Mapping.getBoolMapping(GenericControls.DpadLeft, cState, eState, tp)) return "Left";
                else if (Mapping.getBoolMapping(GenericControls.DpadRight, cState, eState, tp)) return "Right";
                else if (Mapping.getBoolMapping(GenericControls.Back, cState, eState, tp)) return "Back";
                else if (Mapping.getBoolMapping(GenericControls.Start, cState, eState, tp)) return "Start";
                else if (Mapping.getBoolMapping(GenericControls.Guide, cState, eState, tp)) return "Guide";
                else if (Mapping.getBoolMapping(GenericControls.LXPos, cState, eState, tp)) return "LS Right";
                else if (Mapping.getBoolMapping(GenericControls.LXNeg, cState, eState, tp)) return "LS Left";
                else if (Mapping.getBoolMapping(GenericControls.LYPos, cState, eState, tp)) return "LS Down";
                else if (Mapping.getBoolMapping(GenericControls.LYNeg, cState, eState, tp)) return "LS Up";
                else if (Mapping.getBoolMapping(GenericControls.RXPos, cState, eState, tp)) return "RS Right";
                else if (Mapping.getBoolMapping(GenericControls.RXNeg, cState, eState, tp)) return "RS Left";
                else if (Mapping.getBoolMapping(GenericControls.RYPos, cState, eState, tp)) return "RS Down";
                else if (Mapping.getBoolMapping(GenericControls.RYNeg, cState, eState, tp)) return "RS Up";
                else if (Mapping.getBoolMapping(GenericControls.TouchLeft, cState, eState, tp)) return "Touch Left";
                else if (Mapping.getBoolMapping(GenericControls.TouchRight, cState, eState, tp)) return "Touch Right";
                else if (Mapping.getBoolMapping(GenericControls.TouchMulti, cState, eState, tp)) return "Touch Multi";
                else if (Mapping.getBoolMapping(GenericControls.TouchUpper, cState, eState, tp)) return "Touch Upper";
            return "nothing";
        }

        public GenericControls GetInputkeysEAll4(int ind)
        {
            ControllerState cState = CurrentState[ind];
            EAll4StateExposed eState = ExposedState[ind];
            Mouse tp = touchPad[ind];
            if (controllers[ind] != null)
                if (Mapping.getBoolMapping(GenericControls.A, cState, eState, tp)) return GenericControls.A;
                else if (Mapping.getBoolMapping(GenericControls.B, cState, eState, tp)) return GenericControls.B;
                else if (Mapping.getBoolMapping(GenericControls.Y, cState, eState, tp)) return GenericControls.Y;
                else if (Mapping.getBoolMapping(GenericControls.X, cState, eState, tp)) return GenericControls.X;
                else if (Mapping.getBoolMapping(GenericControls.LB, cState, eState, tp)) return GenericControls.LB;
                else if (Mapping.getBoolMapping(GenericControls.RB, cState, eState, tp)) return GenericControls.RB;
                else if (Mapping.getBoolMapping(GenericControls.LT, cState, eState, tp)) return GenericControls.LT;
                else if (Mapping.getBoolMapping(GenericControls.RT, cState, eState, tp)) return GenericControls.RT;
                else if (Mapping.getBoolMapping(GenericControls.LS, cState, eState, tp)) return GenericControls.LS;
                else if (Mapping.getBoolMapping(GenericControls.RS, cState, eState, tp)) return GenericControls.RS;
                else if (Mapping.getBoolMapping(GenericControls.DpadUp, cState, eState, tp)) return GenericControls.DpadUp;
                else if (Mapping.getBoolMapping(GenericControls.DpadDown, cState, eState, tp)) return GenericControls.DpadDown;
                else if (Mapping.getBoolMapping(GenericControls.DpadLeft, cState, eState, tp)) return GenericControls.DpadLeft;
                else if (Mapping.getBoolMapping(GenericControls.DpadRight, cState, eState, tp)) return GenericControls.DpadRight;
                else if (Mapping.getBoolMapping(GenericControls.Back, cState, eState, tp)) return GenericControls.Back;
                else if (Mapping.getBoolMapping(GenericControls.Start, cState, eState, tp)) return GenericControls.Start;
                else if (Mapping.getBoolMapping(GenericControls.Guide, cState, eState, tp)) return GenericControls.Guide;
                else if (Mapping.getBoolMapping(GenericControls.LXPos, cState, eState, tp)) return GenericControls.LXPos;
                else if (Mapping.getBoolMapping(GenericControls.LXNeg, cState, eState, tp)) return GenericControls.LXNeg;
                else if (Mapping.getBoolMapping(GenericControls.LYPos, cState, eState, tp)) return GenericControls.LYPos;
                else if (Mapping.getBoolMapping(GenericControls.LYNeg, cState, eState, tp)) return GenericControls.LYNeg;
                else if (Mapping.getBoolMapping(GenericControls.RXPos, cState, eState, tp)) return GenericControls.RXPos;
                else if (Mapping.getBoolMapping(GenericControls.RXNeg, cState, eState, tp)) return GenericControls.RXNeg;
                else if (Mapping.getBoolMapping(GenericControls.RYPos, cState, eState, tp)) return GenericControls.RYPos;
                else if (Mapping.getBoolMapping(GenericControls.RYNeg, cState, eState, tp)) return GenericControls.RYNeg;
                else if (Mapping.getBoolMapping(GenericControls.TouchLeft, cState, eState, tp)) return GenericControls.TouchLeft;
                else if (Mapping.getBoolMapping(GenericControls.TouchRight, cState, eState, tp)) return GenericControls.TouchRight;
                else if (Mapping.getBoolMapping(GenericControls.TouchMulti, cState, eState, tp)) return GenericControls.TouchMulti;
                else if (Mapping.getBoolMapping(GenericControls.TouchUpper, cState, eState, tp)) return GenericControls.TouchUpper;
            return GenericControls.None;
        }

        public bool[] touchreleased = { true, true, true, true }, touchslid = { false, false, false, false };
        public byte[] oldtouchvalue = { 0, 0, 0, 0 };
        public int[] oldscrollvalue = { 0, 0, 0, 0 };
        protected virtual void CheckForHotkeys(int deviceID, ControllerState cState, ControllerState pState)
        {
            if (!Global.UseTPforControls[deviceID] && cState.Touch1 && pState.Guide)
            {
                if (Global.TouchSensitivity[deviceID] > 0 && touchreleased[deviceID])
                {
                    oldtouchvalue[deviceID] = Global.TouchSensitivity[deviceID];
                    oldscrollvalue[deviceID] = Global.ScrollSensitivity[deviceID];
                    Global.TouchSensitivity[deviceID] = 0;
                    Global.ScrollSensitivity[deviceID] = 0;
                    LogDebug(Global.TouchSensitivity[deviceID] > 0 ? Properties.Resources.TouchpadMovementOn : Properties.Resources.TouchpadMovementOff);
                    Log.LogToTray(Global.TouchSensitivity[deviceID] > 0 ? Properties.Resources.TouchpadMovementOn : Properties.Resources.TouchpadMovementOff);
                    touchreleased[deviceID] = false;
                }
                else if (touchreleased[deviceID])
                {
                    Global.TouchSensitivity[deviceID] = oldtouchvalue[deviceID];
                    Global.ScrollSensitivity[deviceID] = oldscrollvalue[deviceID];
                    LogDebug(Global.TouchSensitivity[deviceID] > 0 ? Properties.Resources.TouchpadMovementOn : Properties.Resources.TouchpadMovementOff);
                    Log.LogToTray(Global.TouchSensitivity[deviceID] > 0 ? Properties.Resources.TouchpadMovementOn : Properties.Resources.TouchpadMovementOff);
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
                oldtouchvalue[deviceID] = Global.TouchSensitivity[deviceID];
                oldscrollvalue[deviceID] = Global.ScrollSensitivity[deviceID];
                Global.TouchSensitivity[deviceID] = 0;
                Global.ScrollSensitivity[deviceID] = 0;
            }
        }

        public virtual string TouchpadSlide(int ind)
        {
            ControllerState cState = CurrentState[ind];
            string slidedir = "none";
            if (controllers[ind] != null)
                if (cState.Touch2)
                    if (controllers[ind] != null)
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
            byte boost = Global.RumbleBoost[deviceNum];
            uint lightBoosted = ((uint)lightMotor * (uint)boost) / 100;
            if (lightBoosted > 255)
                lightBoosted = 255;
            uint heavyBoosted = ((uint)heavyMotor * (uint)boost) / 100;
            if (heavyBoosted > 255)
                heavyBoosted = 255;
            if (deviceNum < 4)
                if (controllers[deviceNum] != null)
                    controllers[deviceNum].setRumble((byte)lightBoosted, (byte)heavyBoosted);
        }

        public ControllerState getEAll4State(int ind)
        {
            return CurrentState[ind];
        }
        public ControllerState getEAll4StateMapped(int ind)
        {
            return MappedState[ind];
        }
    }
}
