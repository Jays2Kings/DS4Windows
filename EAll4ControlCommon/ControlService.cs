﻿using System;
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
        public EAll4Device[] EAll4Controllers = new EAll4Device[4];
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
        List<EAll4Controls> dcs = new List<EAll4Controls>();
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
            for (int i = 0; i < EAll4Controllers.Length; i++)
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
            dcs.Add(EAll4Controls.Cross);
            dcs.Add(EAll4Controls.Cross);
            dcs.Add(EAll4Controls.Circle);
            dcs.Add(EAll4Controls.Square);
            dcs.Add(EAll4Controls.Triangle);
            dcs.Add(EAll4Controls.Options);
            dcs.Add(EAll4Controls.Share);
            dcs.Add(EAll4Controls.DpadUp);
            dcs.Add(EAll4Controls.DpadDown);
            dcs.Add(EAll4Controls.DpadLeft);
            dcs.Add(EAll4Controls.DpadRight);
            dcs.Add(EAll4Controls.PS);
            dcs.Add(EAll4Controls.L1);
            dcs.Add(EAll4Controls.R1);
            dcs.Add(EAll4Controls.L2);
            dcs.Add(EAll4Controls.R2);
            dcs.Add(EAll4Controls.L3);
            dcs.Add(EAll4Controls.R3);
            dcs.Add(EAll4Controls.LXPos);
            dcs.Add(EAll4Controls.LXNeg);
            dcs.Add(EAll4Controls.LYPos);
            dcs.Add(EAll4Controls.LYNeg);
            dcs.Add(EAll4Controls.RXPos);
            dcs.Add(EAll4Controls.RXNeg);
            dcs.Add(EAll4Controls.RYPos);
            dcs.Add(EAll4Controls.RYNeg);
            dcs.Add(EAll4Controls.SwipeUp);
            dcs.Add(EAll4Controls.SwipeDown);
            dcs.Add(EAll4Controls.SwipeLeft);
            dcs.Add(EAll4Controls.SwipeRight);
        }

        private async void WarnExclusiveModeFailure(EAll4Device device)
        {
            if (EAll4Devices.isExclusiveMode && !device.IsExclusive)
            {
                await System.Threading.Tasks.Task.Delay(5);
                String message = Properties.Resources.CouldNotOpenEAll4.Replace("*Mac address*", device.MacAddress) + " " + Properties.Resources.QuitOtherPrograms;
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
                    IEnumerable<EAll4Device> devices = EAll4Devices.getEAll4Controllers();
                    int ind = 0;
                    EAll4LightBar.defualtLight = false;
                    foreach (EAll4Device device in devices)
                    {
                        if (showlog)
                            LogDebug(Properties.Resources.FoundController + device.MacAddress + " (" + device.ConnectionType + ")");
                        WarnExclusiveModeFailure(device);
                        EAll4Controllers[ind] = device;
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
                for (int i = 0; i < EAll4Controllers.Length; i++)
                {
                    if (EAll4Controllers[i] != null)
                    {
                        if (Global.DCBTatStop && !EAll4Controllers[i].Charging && showlog)
                            EAll4Controllers[i].DisconnectBT();
                        else
                        {
                            EAll4LightBar.forcelight[i] = false;
                            EAll4LightBar.forcedFlash[i] = 0;
                            EAll4LightBar.defualtLight = true;
                            EAll4LightBar.updateLightBar(EAll4Controllers[i], i, CurrentState[i], ExposedState[i], touchPad[i]);
                            System.Threading.Thread.Sleep(50);
                        }
                        CurrentState[i].Battery = PreviousState[i].Battery = 0; // Reset for the next connection's initial status change.
                        x360Bus.Unplug(i);
                        anyUnplugged = true;
                        EAll4Controllers[i] = null;
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
                IEnumerable<EAll4Device> devices = EAll4Devices.getEAll4Controllers();
                foreach (EAll4Device device in devices)
                {
                    if (device.IsDisconnecting)
                        continue;
                    if (((Func<bool>)delegate
                    {
                        for (Int32 Index = 0; Index < EAll4Controllers.Length; Index++)
                            if (EAll4Controllers[Index] != null && EAll4Controllers[Index].MacAddress == device.MacAddress)
                                return true;
                        return false;
                    })())
                        continue;
                    for (Int32 Index = 0; Index < EAll4Controllers.Length; Index++)
                        if (EAll4Controllers[Index] == null)
                        {
                            LogDebug(Properties.Resources.FoundController + device.MacAddress + " (" + device.ConnectionType + ")");
                            WarnExclusiveModeFailure(device);
                            EAll4Controllers[Index] = device;
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

        public void TouchPadOn(int ind, EAll4Device device)
        {
            ITouchpadBehaviour tPad = touchPad[ind];
            device.Touchpad.TouchButtonDown += tPad.touchButtonDown;
            device.Touchpad.TouchButtonUp += tPad.touchButtonUp;
            device.Touchpad.TouchesBegan += tPad.touchesBegan;
            device.Touchpad.TouchesMoved += tPad.touchesMoved;
            device.Touchpad.TouchesEnded += tPad.touchesEnded;
            device.Touchpad.TouchUnchanged += tPad.touchUnchanged;
            //LogDebug("Touchpad mode for " + device.MacAddress + " is now " + tmode.ToString());
            //Log.LogToTray("Touchpad mode for " + device.MacAddress + " is now " + tmode.ToString());
            Global.ControllerStatusChanged(this);
        }

        public void TimeoutConnection(EAll4Device d)
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
            if (EAll4Controllers[index] != null)
            {
                EAll4Device d = EAll4Controllers[index];
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
            if (EAll4Controllers[index] != null)
            {
                EAll4Device d = EAll4Controllers[index];
                if (!d.IsAlive())
                //return "Connecting..."; // awaiting the first battery charge indication
                {
                    var TimeoutThread = new System.Threading.Thread(() => TimeoutConnection(d));
                    TimeoutThread.IsBackground = true;
                    TimeoutThread.Name = "TimeoutFor" + d.MacAddress.ToString();
                    TimeoutThread.Start();
                    return Properties.Resources.Connecting;
                }
                return d.MacAddress;
            }
            else
                return String.Empty;
        }

        public string getShortEAll4ControllerInfo(int index)
        {
            if (EAll4Controllers[index] != null)
            {
                EAll4Device d = EAll4Controllers[index];
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
            if (EAll4Controllers[index] != null)
            {
                EAll4Device d = EAll4Controllers[index];
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
            if (EAll4Controllers[index] != null)
            {
                EAll4Device d = EAll4Controllers[index];
                return d.ConnectionType + "";
            }
            else
                return Properties.Resources.NoneText;
        }


        private int XINPUT_UNPLUG_SETTLE_TIME = 250; // Inhibit races that occur with the asynchronous teardown of ScpVBus -> X360 driver instance.
        //Called when EAll4 is disconnected or timed out
        protected virtual void On_EAll4Removal(object sender, EventArgs e)
        {
            EAll4Device device = (EAll4Device)sender;
            int ind = -1;
            for (int i = 0; i < EAll4Controllers.Length; i++)
                if (EAll4Controllers[i] != null && device.MacAddress == EAll4Controllers[i].MacAddress)
                    ind = i;
            if (ind != -1)
            {
                CurrentState[ind].Battery = PreviousState[ind].Battery = 0; // Reset for the next connection's initial status change.
                x360Bus.Unplug(ind);
                LogDebug(Properties.Resources.ControllerWasRemoved.Replace("*Mac address*", device.MacAddress));
                Log.LogToTray(Properties.Resources.ControllerWasRemoved.Replace("*Mac address*", device.MacAddress));
                System.Threading.Thread.Sleep(XINPUT_UNPLUG_SETTLE_TIME);
                EAll4Controllers[ind] = null;
                touchPad[ind] = null;
                Global.ControllerStatusChanged(this);
            }
        }
        public bool[] lag = { false, false, false, false };
        //Called every time the new input report has arrived
        protected virtual void On_Report(object sender, EventArgs e)
        {

            EAll4Device device = (EAll4Device)sender;

            int ind = -1;
            for (int i = 0; i < EAll4Controllers.Length; i++)
                if (device == EAll4Controllers[i])
                    ind = i;

            if (ind != -1)
            {
                if (Global.FlushHIDQueue[ind])
                    device.FlushHID();
                if (!string.IsNullOrEmpty(device.error))
                {
                    LogDebug(device.error);
                }
                if (DateTime.UtcNow - device.firstActive > TimeSpan.FromSeconds(5))
                {
                    if (device.Latency >= 10 && !lag[ind])
                        LagFlashWarning(ind, true);
                    else if (device.Latency < 10 && lag[ind])
                        LagFlashWarning(ind, false);
                }
                device.getExposedState(ExposedState[ind], CurrentState[ind]);
                ControllerState cState = CurrentState[ind];
                device.getPreviousState(PreviousState[ind]);
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
                EAll4LightBar.updateLightBar(device, ind, cState, ExposedState[ind], touchPad[ind]);

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
                device.IdleTimeout = Global.IdleDisconnectTimeout[ind];
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
            EAll4Controls helddown = EAll4Controls.None;
            foreach (KeyValuePair<EAll4Controls, string> p in Global.getCustomExtras(ind))
            {
                if (Mapping.getBoolMapping(p.Key, cState, eState, tp))
                {
                    helddown = p.Key;
                    break;
                }
            }
            if (helddown != EAll4Controls.None)
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
            foreach (EAll4Controls dc in dcs)
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
            if (EAll4Controllers[ind] != null)
                if (Mapping.getBoolMapping(EAll4Controls.Cross, cState, eState, tp)) return "Cross";
                else if (Mapping.getBoolMapping(EAll4Controls.Circle, cState, eState, tp)) return "Circle";
                else if (Mapping.getBoolMapping(EAll4Controls.Triangle, cState, eState, tp)) return "Triangle";
                else if (Mapping.getBoolMapping(EAll4Controls.Square, cState, eState, tp)) return "Square";
                else if (Mapping.getBoolMapping(EAll4Controls.L1, cState, eState, tp)) return "L1";
                else if (Mapping.getBoolMapping(EAll4Controls.R1, cState, eState, tp)) return "R1";
                else if (Mapping.getBoolMapping(EAll4Controls.L2, cState, eState, tp)) return "L2";
                else if (Mapping.getBoolMapping(EAll4Controls.R2, cState, eState, tp)) return "R2";
                else if (Mapping.getBoolMapping(EAll4Controls.L3, cState, eState, tp)) return "L3";
                else if (Mapping.getBoolMapping(EAll4Controls.R3, cState, eState, tp)) return "R3";
                else if (Mapping.getBoolMapping(EAll4Controls.DpadUp, cState, eState, tp)) return "Up";
                else if (Mapping.getBoolMapping(EAll4Controls.DpadDown, cState, eState, tp)) return "Down";
                else if (Mapping.getBoolMapping(EAll4Controls.DpadLeft, cState, eState, tp)) return "Left";
                else if (Mapping.getBoolMapping(EAll4Controls.DpadRight, cState, eState, tp)) return "Right";
                else if (Mapping.getBoolMapping(EAll4Controls.Share, cState, eState, tp)) return "Share";
                else if (Mapping.getBoolMapping(EAll4Controls.Options, cState, eState, tp)) return "Options";
                else if (Mapping.getBoolMapping(EAll4Controls.PS, cState, eState, tp)) return "PS";
                else if (Mapping.getBoolMapping(EAll4Controls.LXPos, cState, eState, tp)) return "LS Right";
                else if (Mapping.getBoolMapping(EAll4Controls.LXNeg, cState, eState, tp)) return "LS Left";
                else if (Mapping.getBoolMapping(EAll4Controls.LYPos, cState, eState, tp)) return "LS Down";
                else if (Mapping.getBoolMapping(EAll4Controls.LYNeg, cState, eState, tp)) return "LS Up";
                else if (Mapping.getBoolMapping(EAll4Controls.RXPos, cState, eState, tp)) return "RS Right";
                else if (Mapping.getBoolMapping(EAll4Controls.RXNeg, cState, eState, tp)) return "RS Left";
                else if (Mapping.getBoolMapping(EAll4Controls.RYPos, cState, eState, tp)) return "RS Down";
                else if (Mapping.getBoolMapping(EAll4Controls.RYNeg, cState, eState, tp)) return "RS Up";
                else if (Mapping.getBoolMapping(EAll4Controls.TouchLeft, cState, eState, tp)) return "Touch Left";
                else if (Mapping.getBoolMapping(EAll4Controls.TouchRight, cState, eState, tp)) return "Touch Right";
                else if (Mapping.getBoolMapping(EAll4Controls.TouchMulti, cState, eState, tp)) return "Touch Multi";
                else if (Mapping.getBoolMapping(EAll4Controls.TouchUpper, cState, eState, tp)) return "Touch Upper";
            return "nothing";
        }

        public EAll4Controls GetInputkeysEAll4(int ind)
        {
            ControllerState cState = CurrentState[ind];
            EAll4StateExposed eState = ExposedState[ind];
            Mouse tp = touchPad[ind];
            if (EAll4Controllers[ind] != null)
                if (Mapping.getBoolMapping(EAll4Controls.Cross, cState, eState, tp)) return EAll4Controls.Cross;
                else if (Mapping.getBoolMapping(EAll4Controls.Circle, cState, eState, tp)) return EAll4Controls.Circle;
                else if (Mapping.getBoolMapping(EAll4Controls.Triangle, cState, eState, tp)) return EAll4Controls.Triangle;
                else if (Mapping.getBoolMapping(EAll4Controls.Square, cState, eState, tp)) return EAll4Controls.Square;
                else if (Mapping.getBoolMapping(EAll4Controls.L1, cState, eState, tp)) return EAll4Controls.L1;
                else if (Mapping.getBoolMapping(EAll4Controls.R1, cState, eState, tp)) return EAll4Controls.R1;
                else if (Mapping.getBoolMapping(EAll4Controls.L2, cState, eState, tp)) return EAll4Controls.L2;
                else if (Mapping.getBoolMapping(EAll4Controls.R2, cState, eState, tp)) return EAll4Controls.R2;
                else if (Mapping.getBoolMapping(EAll4Controls.L3, cState, eState, tp)) return EAll4Controls.L3;
                else if (Mapping.getBoolMapping(EAll4Controls.R3, cState, eState, tp)) return EAll4Controls.R3;
                else if (Mapping.getBoolMapping(EAll4Controls.DpadUp, cState, eState, tp)) return EAll4Controls.DpadUp;
                else if (Mapping.getBoolMapping(EAll4Controls.DpadDown, cState, eState, tp)) return EAll4Controls.DpadDown;
                else if (Mapping.getBoolMapping(EAll4Controls.DpadLeft, cState, eState, tp)) return EAll4Controls.DpadLeft;
                else if (Mapping.getBoolMapping(EAll4Controls.DpadRight, cState, eState, tp)) return EAll4Controls.DpadRight;
                else if (Mapping.getBoolMapping(EAll4Controls.Share, cState, eState, tp)) return EAll4Controls.Share;
                else if (Mapping.getBoolMapping(EAll4Controls.Options, cState, eState, tp)) return EAll4Controls.Options;
                else if (Mapping.getBoolMapping(EAll4Controls.PS, cState, eState, tp)) return EAll4Controls.PS;
                else if (Mapping.getBoolMapping(EAll4Controls.LXPos, cState, eState, tp)) return EAll4Controls.LXPos;
                else if (Mapping.getBoolMapping(EAll4Controls.LXNeg, cState, eState, tp)) return EAll4Controls.LXNeg;
                else if (Mapping.getBoolMapping(EAll4Controls.LYPos, cState, eState, tp)) return EAll4Controls.LYPos;
                else if (Mapping.getBoolMapping(EAll4Controls.LYNeg, cState, eState, tp)) return EAll4Controls.LYNeg;
                else if (Mapping.getBoolMapping(EAll4Controls.RXPos, cState, eState, tp)) return EAll4Controls.RXPos;
                else if (Mapping.getBoolMapping(EAll4Controls.RXNeg, cState, eState, tp)) return EAll4Controls.RXNeg;
                else if (Mapping.getBoolMapping(EAll4Controls.RYPos, cState, eState, tp)) return EAll4Controls.RYPos;
                else if (Mapping.getBoolMapping(EAll4Controls.RYNeg, cState, eState, tp)) return EAll4Controls.RYNeg;
                else if (Mapping.getBoolMapping(EAll4Controls.TouchLeft, cState, eState, tp)) return EAll4Controls.TouchLeft;
                else if (Mapping.getBoolMapping(EAll4Controls.TouchRight, cState, eState, tp)) return EAll4Controls.TouchRight;
                else if (Mapping.getBoolMapping(EAll4Controls.TouchMulti, cState, eState, tp)) return EAll4Controls.TouchMulti;
                else if (Mapping.getBoolMapping(EAll4Controls.TouchUpper, cState, eState, tp)) return EAll4Controls.TouchUpper;
            return EAll4Controls.None;
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
            if (EAll4Controllers[ind] != null)
                if (cState.Touch2)
                    if (EAll4Controllers[ind] != null)
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
                if (EAll4Controllers[deviceNum] != null)
                    EAll4Controllers[deviceNum].setRumble((byte)lightBoosted, (byte)heavyBoosted);
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
