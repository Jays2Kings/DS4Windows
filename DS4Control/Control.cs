﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DS4Library;
using System.IO;
using System.Reflection;
using System.Media;
using DS4Control.Enums;
namespace DS4Control
{
    public class Control
    {
        public X360Device x360Bus;
        public DS4Device[] DS4Controllers = new DS4Device[4];
        //TPadModeSwitcher[] modeSwitcher = new TPadModeSwitcher[4];
        public Mouse[] touchPad = new Mouse[4];
        private bool running = false;
        private DS4State[] MappedState = new DS4State[4];
        private DS4State[] CurrentState = new DS4State[4];
        private DS4State[] PreviousState = new DS4State[4];
        public DS4StateExposed[] ExposedState = new DS4StateExposed[4];
        public bool recordingMacro = false;
        public event EventHandler<DebugEventArgs> Debug = null;
        public bool eastertime = false;
        private int eCode = 0;
        bool[] buttonsdown = { false, false, false, false };
        List<DS4Controls> dcs = new List<DS4Controls>();
        bool[] held = new bool[4];
        int[] oldmouse = new int[4] { -1, -1, -1, -1 };
        SoundPlayer sp = new SoundPlayer();

        private class X360Data
        {
            public byte[] Report = new byte[28];
            public byte[] Rumble = new byte[8];
        }
        private X360Data[] processingData = new X360Data[4];

        public Control()
        {
            sp.Stream = Properties.Resources.EE;
            x360Bus = new X360Device();
            AddtoDS4List();
            for (int i = 0; i < DS4Controllers.Length; i++)
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
            if (DS4Devices.isExclusiveMode && !device.IsExclusive)
            {
                await System.Threading.Tasks.Task.Delay(5);
                String message = Properties.Resources.CouldNotOpenDS4.Replace("*Mac address*", device.MacAddress) + " " + Properties.Resources.QuitOtherPrograms;
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
                DS4Devices.isExclusiveMode = Global.getUseExclusiveMode();
                if (showlog)
                {
                    LogDebug(Properties.Resources.SearchingController);
                    LogDebug(DS4Devices.isExclusiveMode ?  Properties.Resources.UsingExclusive: Properties.Resources.UsingShared);
                }
                try
                {
                    DS4Devices.findControllers();
                    IEnumerable<DS4Device> devices = DS4Devices.getDS4Controllers();
                    int ind = 0;
                    DS4LightBar.defualtLight = false;
                    foreach (DS4Device device in devices)
                    {
                        if (showlog)
                            LogDebug(Properties.Resources.FoundController + device.MacAddress + " (" + device.ConnectionType + ")");
                        WarnExclusiveModeFailure(device);
                        DS4Controllers[ind] = device;
                        device.Removal -= DS4Devices.On_Removal;
                        device.Removal += this.On_DS4Removal;
                        device.Removal += DS4Devices.On_Removal;
                        touchPad[ind] = new Mouse(ind, device);
                        DS4Color color = Global.loadColor(ind);
                        device.LightBarColor = color;
                        if (!Global.getDinputOnly(ind))
                            x360Bus.Plugin(ind);
                        device.Report += this.On_Report;
                        TouchPadOn(ind, device);
                        string filename = Path.GetFileName(Global.getAProfile(ind));
                        ind++;
                        if (showlog)
                            if (System.IO.File.Exists(Global.appdatapath + "\\Profiles\\" + filename))
                            {
                                string prolog = Properties.Resources.UsingProfile.Replace("*number*", ind.ToString()).Replace("*Profile name*", filename.Substring(0, filename.Length - 4));
                                LogDebug(prolog);
                                Log.LogToTray(prolog);
                            }
                            else
                            {
                                LogDebug("Controller " + ind + " is not using a profile");
                                Log.LogToTray("Controller " + ind + " is not using a profile");
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
                for (int i = 0; i < DS4Controllers.Length; i++)
                {
                    if (DS4Controllers[i] != null)
                    {
                        if (Global.getDCBTatStop() && !DS4Controllers[i].Charging && showlog)
                            DS4Controllers[i].DisconnectBT();
                        else
                        {
                            DS4LightBar.defualtLight = true;
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
                Global.ControllerStatusChanged(this);                
            }
            return true;
        }

        public bool HotPlug()
        {
            if (running)
            {
                DS4Devices.findControllers();
                IEnumerable<DS4Device> devices = DS4Devices.getDS4Controllers();
                foreach (DS4Device device in devices)
                {
                    if (device.IsDisconnecting)
                        continue;
                    if (((Func<bool>)delegate
                    {
                        for (Int32 Index = 0; Index < DS4Controllers.Length; Index++)
                            if (DS4Controllers[Index] != null && DS4Controllers[Index].MacAddress == device.MacAddress)
                                return true;
                        return false;
                    })())
                        continue;
                    for (Int32 Index = 0; Index < DS4Controllers.Length; Index++)
                        if (DS4Controllers[Index] == null)
                        {
                            LogDebug(Properties.Resources.FoundController + device.MacAddress + " (" + device.ConnectionType + ")");
                            WarnExclusiveModeFailure(device);
                            DS4Controllers[Index] = device;
                            device.Removal -= DS4Devices.On_Removal;
                            device.Removal += this.On_DS4Removal;
                            device.Removal += DS4Devices.On_Removal;
                            touchPad[Index] = new Mouse(Index, device);
                            device.LightBarColor = Global.loadColor(Index);
                            device.Report += this.On_Report;
                            if (!Global.getDinputOnly(Index))
                                x360Bus.Plugin(Index);
                            TouchPadOn(Index, device);
                            string filename = Path.GetFileName(Global.getAProfile(Index));
                            if (System.IO.File.Exists(Global.appdatapath + "\\Profiles\\" + filename))
                            {
                                string prolog = Properties.Resources.UsingProfile.Replace("*number*", (Index + 1).ToString()).Replace("*Profile name*", filename.Substring(0, filename.Length - 4));
                                LogDebug(prolog);
                                Log.LogToTray(prolog);
                            }
                            else
                            {
                                LogDebug("Controller " + (Index + 1) + " is not using a profile");
                                Log.LogToTray("Controller " + (Index + 1) + " is not using a profile");
                            }
                        
                            break;
                        }
                }
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
            //LogDebug("Touchpad mode for " + device.MacAddress + " is now " + tmode.ToString());
            //Log.LogToTray("Touchpad mode for " + device.MacAddress + " is now " + tmode.ToString());
            Global.ControllerStatusChanged(this);
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
                    TimeoutThread.Name = "TimeoutFor" + d.MacAddress.ToString();
                    TimeoutThread.Start();
                    return Properties.Resources.Connecting;
                }
                return d.MacAddress;
            }
            else
                return String.Empty;
        }

        public string getShortDS4ControllerInfo(int index)
        {
            if (DS4Controllers[index] != null)
            {
                DS4Device d = DS4Controllers[index];
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

        public string getDS4Battery(int index)
        {
            if (DS4Controllers[index] != null)
            {
                DS4Device d = DS4Controllers[index];
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

        public string getDS4Status(int index)
        {
            if (DS4Controllers[index] != null)
            {
                DS4Device d = DS4Controllers[index];
                return d.ConnectionType+"";
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
            for (int i = 0; i < DS4Controllers.Length; i++)
                if (DS4Controllers[i] != null && device.MacAddress == DS4Controllers[i].MacAddress)
                    ind = i;
            if (ind != -1)
            {
                CurrentState[ind].Battery = PreviousState[ind].Battery = 0; // Reset for the next connection's initial status change.
                x360Bus.Unplug(ind);
                LogDebug(Properties.Resources.ControllerWasRemoved.Replace("*Mac address*", device.MacAddress));
                Log.LogToTray(Properties.Resources.ControllerWasRemoved.Replace("*Mac address*", device.MacAddress));
                System.Threading.Thread.Sleep(XINPUT_UNPLUG_SETTLE_TIME);
                DS4Controllers[ind] = null;
                touchPad[ind] = null;
                Global.ControllerStatusChanged(this);
            }
        }

        //Called every time the new input report has arrived
        protected virtual void On_Report(object sender, EventArgs e)
        {

            DS4Device device = (DS4Device)sender;

            int ind = -1;
            for (int i = 0; i < DS4Controllers.Length; i++)
                if (device == DS4Controllers[i])
                    ind = i;

            if (ind != -1)
            {
                if (Global.getFlushHIDQueue(ind))
                    device.FlushHID();

                //This shouldn't be altering the collection from outside.
                //something to fix
                while (device.currentErrors.Count() > 0)
                {
                    LogDebug(device.currentErrors[0]);
                    device.currentErrors.RemoveAt(0);
                }

                device.getExposedState(ExposedState[ind], CurrentState[ind]);
                DS4State cState = CurrentState[ind];
                device.getPreviousState(PreviousState[ind]);
                DS4State pState = PreviousState[ind];

                if (pState.Battery != cState.Battery)
                {
                    Global.ControllerStatusChanged(this);
                }

                CheckForHotkeys(ind, cState, pState);
                if (eastertime)
                {
                    EasterTime(ind);
                }

                GetInputkeys(ind);

                if (Global.getLSCurve(ind) + Global.getRSCurve(ind) + Global.getLSDeadzone(ind) + Global.getRSDeadzone(ind) +
                    Global.getL2Deadzone(ind) + Global.getR2Deadzone(ind) > 0)
                {
                    //if a curve or deadzone is in place
                    cState = Mapping.SetCurveAndDeadzone(ind, cState);
                }

                if (!recordingMacro && (!string.IsNullOrEmpty(Global.tempprofilename[ind]) ||
                    Global.getHasCustomKeysorButtons(ind) || Global.getHasShiftCustomKeysorButtons(ind) || Global.GetProfileActions(ind).Count > 0))
                {
                    Mapping.MapCustom(ind, cState, MappedState[ind], ExposedState[ind], touchPad[ind], this);
                    cState = MappedState[ind];
                }
                if (Global.getHasCustomExtras(ind))
                    DoExtras(ind);

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
                device.IdleTimeout = Global.getIdleDisconnectTimeout(ind);
            }
        }

        private void DoExtras(int ind)
        {
            DS4State cState = CurrentState[ind];
            DS4StateExposed eState = ExposedState[ind];
            Mouse tp = touchPad[ind];
            DS4Controls helddown = DS4Controls.None;
            foreach (KeyValuePair<DS4Controls, string> p in Global.getCustomExtras(ind))
            {
                if (Mapping.getBoolMapping(p.Key, cState, eState, tp))
                {
                    helddown = p.Key;
                    break;
                }
            }
            if (helddown != DS4Controls.None)
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
                        DS4Color color = new DS4Color { red = (byte)extras[3], green = (byte)extras[4], blue = (byte)extras[5] };
                        DS4LightBar.forcedColor[ind] = color;
                        DS4LightBar.forcedFlash[ind] = (byte)extras[6];
                        DS4LightBar.forcelight[ind] = true;
                    }
                    if (extras[7] == 1)
                    {
                        if (oldmouse[ind] == -1)
                            oldmouse[ind] = Global.getButtonMouseSensitivity(ind);
                        Global.setButtonMouseSensitivity(ind, extras[8]);
                    }
                }
                catch { }
            }
            else if (held[ind])
            {
                DS4LightBar.forcelight[ind] = false;
                DS4LightBar.forcedFlash[ind] = 0;
                //Console.WriteLine(p.Key + " is done");                
                Global.setButtonMouseSensitivity(ind, oldmouse[ind]);
                oldmouse[ind] = -1;
                setRumble(0, 0, ind);
                held[ind] = false;
            }
        }



        public void EasterTime(int ind)
        {
            DS4State cState = CurrentState[ind];
            DS4StateExposed eState = ExposedState[ind];
            Mouse tp = touchPad[ind];

            bool pb = false;
            foreach (DS4Controls dc in dcs)
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
            if (DS4Controllers[ind] != null)
                if (Mapping.getBoolMapping(DS4Controls.Cross, cState, eState, tp)) return "Cross";
                else if (Mapping.getBoolMapping(DS4Controls.Circle, cState, eState, tp)) return "Circle";
                else if (Mapping.getBoolMapping(DS4Controls.Triangle, cState, eState, tp)) return "Triangle";
                else if (Mapping.getBoolMapping(DS4Controls.Square, cState, eState, tp)) return "Square";
                else if (Mapping.getBoolMapping(DS4Controls.L1, cState, eState, tp)) return "L1";
                else if (Mapping.getBoolMapping(DS4Controls.R1, cState, eState, tp)) return "R1";
                else if (Mapping.getBoolMapping(DS4Controls.L2, cState, eState, tp)) return "L2";
                else if (Mapping.getBoolMapping(DS4Controls.R2, cState, eState, tp)) return "R2";
                else if (Mapping.getBoolMapping(DS4Controls.L3, cState, eState, tp)) return "L3";
                else if (Mapping.getBoolMapping(DS4Controls.R3, cState, eState, tp)) return "R3";
                else if (Mapping.getBoolMapping(DS4Controls.DpadUp, cState, eState, tp)) return "Up";
                else if (Mapping.getBoolMapping(DS4Controls.DpadDown, cState, eState, tp)) return "Down";
                else if (Mapping.getBoolMapping(DS4Controls.DpadLeft, cState, eState, tp)) return "Left";
                else if (Mapping.getBoolMapping(DS4Controls.DpadRight, cState, eState, tp)) return "Right";
                else if (Mapping.getBoolMapping(DS4Controls.Share, cState, eState, tp)) return "Share";
                else if (Mapping.getBoolMapping(DS4Controls.Options, cState, eState, tp)) return "Options";
                else if (Mapping.getBoolMapping(DS4Controls.PS, cState, eState, tp)) return "PS";
                else if (Mapping.getBoolMapping(DS4Controls.LXPos, cState, eState, tp)) return "LS Right";
                else if (Mapping.getBoolMapping(DS4Controls.LXNeg, cState, eState, tp)) return "LS Left";
                else if (Mapping.getBoolMapping(DS4Controls.LYPos, cState, eState, tp)) return "LS Down";
                else if (Mapping.getBoolMapping(DS4Controls.LYNeg, cState, eState, tp)) return "LS Up";
                else if (Mapping.getBoolMapping(DS4Controls.RXPos, cState, eState, tp)) return "RS Right";
                else if (Mapping.getBoolMapping(DS4Controls.RXNeg, cState, eState, tp)) return "RS Left";
                else if (Mapping.getBoolMapping(DS4Controls.RYPos, cState, eState, tp)) return "RS Down";
                else if (Mapping.getBoolMapping(DS4Controls.RYNeg, cState, eState, tp)) return "RS Up";
                else if (Mapping.getBoolMapping(DS4Controls.TouchLeft, cState, eState, tp)) return "Touch Left";
                else if (Mapping.getBoolMapping(DS4Controls.TouchRight, cState, eState, tp)) return "Touch Right";
                else if (Mapping.getBoolMapping(DS4Controls.TouchMulti, cState, eState, tp)) return "Touch Multi";
                else if (Mapping.getBoolMapping(DS4Controls.TouchUpper, cState, eState, tp)) return "Touch Upper";
            return "nothing";
        }

        public DS4Controls GetInputkeysDS4(int ind)
        {
            DS4State cState = CurrentState[ind];
            DS4StateExposed eState = ExposedState[ind];
            Mouse tp = touchPad[ind];
            if (DS4Controllers[ind] != null)
                if (Mapping.getBoolMapping(DS4Controls.Cross, cState, eState, tp)) return DS4Controls.Cross;
                else if (Mapping.getBoolMapping(DS4Controls.Circle, cState, eState, tp)) return DS4Controls.Circle;
                else if (Mapping.getBoolMapping(DS4Controls.Triangle, cState, eState, tp)) return DS4Controls.Triangle;
                else if (Mapping.getBoolMapping(DS4Controls.Square, cState, eState, tp)) return DS4Controls.Square;
                else if (Mapping.getBoolMapping(DS4Controls.L1, cState, eState, tp)) return DS4Controls.L1;
                else if (Mapping.getBoolMapping(DS4Controls.R1, cState, eState, tp)) return DS4Controls.R1;
                else if (Mapping.getBoolMapping(DS4Controls.L2, cState, eState, tp)) return DS4Controls.L2;
                else if (Mapping.getBoolMapping(DS4Controls.R2, cState, eState, tp)) return DS4Controls.R2;
                else if (Mapping.getBoolMapping(DS4Controls.L3, cState, eState, tp)) return DS4Controls.L3;
                else if (Mapping.getBoolMapping(DS4Controls.R3, cState, eState, tp)) return DS4Controls.R3;
                else if (Mapping.getBoolMapping(DS4Controls.DpadUp, cState, eState, tp)) return DS4Controls.DpadUp;
                else if (Mapping.getBoolMapping(DS4Controls.DpadDown, cState, eState, tp)) return DS4Controls.DpadDown;
                else if (Mapping.getBoolMapping(DS4Controls.DpadLeft, cState, eState, tp)) return DS4Controls.DpadLeft;
                else if (Mapping.getBoolMapping(DS4Controls.DpadRight, cState, eState, tp)) return DS4Controls.DpadRight;
                else if (Mapping.getBoolMapping(DS4Controls.Share, cState, eState, tp)) return DS4Controls.Share;
                else if (Mapping.getBoolMapping(DS4Controls.Options, cState, eState, tp)) return DS4Controls.Options;
                else if (Mapping.getBoolMapping(DS4Controls.PS, cState, eState, tp)) return DS4Controls.PS;
                else if (Mapping.getBoolMapping(DS4Controls.LXPos, cState, eState, tp)) return DS4Controls.LXPos;
                else if (Mapping.getBoolMapping(DS4Controls.LXNeg, cState, eState, tp)) return DS4Controls.LXNeg;
                else if (Mapping.getBoolMapping(DS4Controls.LYPos, cState, eState, tp)) return DS4Controls.LYPos;
                else if (Mapping.getBoolMapping(DS4Controls.LYNeg, cState, eState, tp)) return DS4Controls.LYNeg;
                else if (Mapping.getBoolMapping(DS4Controls.RXPos, cState, eState, tp)) return DS4Controls.RXPos;
                else if (Mapping.getBoolMapping(DS4Controls.RXNeg, cState, eState, tp)) return DS4Controls.RXNeg;
                else if (Mapping.getBoolMapping(DS4Controls.RYPos, cState, eState, tp)) return DS4Controls.RYPos;
                else if (Mapping.getBoolMapping(DS4Controls.RYNeg, cState, eState, tp)) return DS4Controls.RYNeg;
                else if (Mapping.getBoolMapping(DS4Controls.TouchLeft, cState, eState, tp)) return DS4Controls.TouchLeft;
                else if (Mapping.getBoolMapping(DS4Controls.TouchRight, cState, eState, tp)) return DS4Controls.TouchRight;
                else if (Mapping.getBoolMapping(DS4Controls.TouchMulti, cState, eState, tp)) return DS4Controls.TouchMulti;
                else if (Mapping.getBoolMapping(DS4Controls.TouchUpper, cState, eState, tp)) return DS4Controls.TouchUpper;
            return DS4Controls.None;
        }

        public bool[] touchreleased = { true, true, true, true }, touchslid = { false, false, false, false };
        public byte[] oldtouchvalue = { 0, 0, 0, 0 };
        public int[] oldscrollvalue = { 0, 0, 0, 0 };
        protected virtual void CheckForHotkeys(int deviceID, DS4State cState, DS4State pState)
        {
            if (!Global.getUseTPforControls(deviceID) && cState.Touch1 && pState.PS)
            {
                if (Global.getTouchSensitivity(deviceID) > 0 && touchreleased[deviceID])
                {
                    oldtouchvalue[deviceID] = Global.getTouchSensitivity(deviceID);
                    oldscrollvalue[deviceID] = Global.getScrollSensitivity(deviceID);
                    Global.setTouchSensitivity(deviceID, 0);
                    Global.setScrollSensitivity(deviceID, 0);
                    LogDebug(Global.getTouchSensitivity(deviceID) > 0 ? Properties.Resources.TouchpadMovementOn : Properties.Resources.TouchpadMovementOff);
                    Log.LogToTray(Global.getTouchSensitivity(deviceID) > 0 ? Properties.Resources.TouchpadMovementOn : Properties.Resources.TouchpadMovementOff);
                    touchreleased[deviceID] = false;
                }
                else if (touchreleased[deviceID])
                {
                    Global.setTouchSensitivity(deviceID, oldtouchvalue[deviceID]);
                    Global.setScrollSensitivity(deviceID, oldscrollvalue[deviceID]);
                    LogDebug(Global.getTouchSensitivity(deviceID) > 0 ? Properties.Resources.TouchpadMovementOn : Properties.Resources.TouchpadMovementOff);
                    Log.LogToTray(Global.getTouchSensitivity(deviceID) > 0 ? Properties.Resources.TouchpadMovementOn : Properties.Resources.TouchpadMovementOff);
                    touchreleased[deviceID] = false;
                }
            }
            else
            {
                touchreleased[deviceID] = true;
            }
        }

        public virtual void StartTPOff(int deviceID)
        {
            if (deviceID > 4)
            {
                oldtouchvalue[deviceID] = Global.getTouchSensitivity(deviceID);
                oldscrollvalue[deviceID] = Global.getScrollSensitivity(deviceID);
                Global.setTouchSensitivity(deviceID, 0);
                Global.setScrollSensitivity(deviceID, 0);
            }
        }
  
        public virtual string TouchpadSlideString(int ind)
        {
            DS4State cState = CurrentState[ind];
            string slidedir = "none";
            if (DS4Controllers[ind] != null)
                if (cState.Touch2)
                    if (DS4Controllers[ind] != null)
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

        public virtual TouchpadSlideDirections TouchpadSlideEnum(int ind)
        {
            DS4State cState = CurrentState[ind];
            TouchpadSlideDirections slidedir = TouchpadSlideDirections.none;
            if (DS4Controllers[ind] != null)
                if (cState.Touch2)
                    if (DS4Controllers[ind] != null)
                        if (touchPad[ind].slideright && !touchslid[ind])
                        {
                            slidedir = TouchpadSlideDirections.right;
                            touchslid[ind] = true;
                        }
                        else if (touchPad[ind].slideleft && !touchslid[ind])
                        {
                            slidedir = TouchpadSlideDirections.left;
                            touchslid[ind] = true;
                        }
                        else if (!touchPad[ind].slideleft && !touchPad[ind].slideright)
                        {
                            slidedir = TouchpadSlideDirections.none;
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
            byte boost = Global.loadRumbleBoost(deviceNum);
            uint lightBoosted = ((uint)lightMotor * (uint)boost) / 100;
            if (lightBoosted > 255)
                lightBoosted = 255;
            uint heavyBoosted = ((uint)heavyMotor * (uint)boost) / 100;
            if (heavyBoosted > 255)
                heavyBoosted = 255;
            if (deviceNum < 4)
                if (DS4Controllers[deviceNum] != null)
                    DS4Controllers[deviceNum].setRumble((byte)lightBoosted, (byte)heavyBoosted);
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
