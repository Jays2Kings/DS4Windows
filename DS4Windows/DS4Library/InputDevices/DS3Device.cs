using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace DS4Windows.InputDevices
{
    public class DS3Device : DS4Device
    {
        private const byte OUTPUT_REPORT_ID = 0x00;
        private bool timeStampInit = false;
        private uint timeStampPrevious = 0;
        private uint deltaTimeCurrent = 0;
        private bool outputDirty = false;
        private DS4HapticState previousHapticState = new DS4HapticState();
        private byte[] featureReport;

        public override event ReportHandler<EventArgs> Report = null;
        public override event EventHandler BatteryChanged;
        public override event EventHandler ChargingChanged;

        public DS3Device(HidDevice hidDevice, string disName, VidPidFeatureSet featureSet = VidPidFeatureSet.DefaultDS4) :
            base(hidDevice, disName, featureSet)
        {
            synced = true;
        }

        public override void PostInit()
        {
            conType = hDevice.ParentPath.StartsWith("BTHPS3BUS\\") ? ConnectionType.BT : ConnectionType.USB;
            Mac = hDevice.GenerateFakeHwSerial();
            deviceType = InputDeviceType.DS3;
            gyroMouseSensSettings = new GyroMouseSens();
            featureReport = new byte[hDevice.Capabilities.FeatureReportByteLength];
            inputReport = new byte[hDevice.Capabilities.InputReportByteLength];
            outputReport = new byte[hDevice.Capabilities.OutputReportByteLength];
            warnInterval = WARN_INTERVAL_USB;

            if (!hDevice.IsFileStreamOpen())
            {
                hDevice.OpenFileStream(outputReport.Length);
            }
        }

        public static ConnectionType DetermineConnectionType(HidDevice hidDevice)
        {
            ConnectionType result = hidDevice.ParentPath.StartsWith("BTHPS3BUS\\") ?
                ConnectionType.BT : ConnectionType.USB;
            return result;
        }

        public override bool DisconnectBT(bool callRemoval = false)
        {
            return false; // we using fake address
        }

        public override bool DisconnectDongle(bool remove = false)
        {
            return false; // we using fake address
        }

        public override bool DisconnectWireless(bool callRemoval = false)
        {
            return false; // we using fake address
        }

        public override bool IsAlive()
        {
            return synced;
        }

        public override void StartUpdate()
        {
            this.inputReportErrorCount = 0;

            if (ds4Input == null)
            {
                ds4Input = new Thread(ReadInput);
                ds4Input.Priority = ThreadPriority.AboveNormal;
                ds4Input.Name = "DS3 Input thread: " + Mac;
                ds4Input.IsBackground = true;
                ds4Input.Start();
            }
            else
                Console.WriteLine("Thread already running for DS4: " + Mac);
        }

        private unsafe void ReadInput()
        {
            unchecked
            {
                firstActive = DateTime.UtcNow;
                NativeMethods.HidD_SetNumInputBuffers(hDevice.safeReadHandle.DangerousGetHandle(), 3);
                Queue<long> latencyQueue = new Queue<long>(21); // Set capacity at max + 1 to avoid any resizing
                int tempLatencyCount = 0;
                long oldtime = 0;
                string currerror = string.Empty;
                long curtime = 0;
                long testelapsed = 0;
                timeoutEvent = false;
                ds4InactiveFrame = true;
                idleInput = true;
                bool syncWriteReport = conType != ConnectionType.BT;
                //bool forceWrite = false;

                int tempBattery = 0;
                bool tempCharging = charging;
                bool tempFull = false;
                uint tempStamp = 0;
                double elapsedDeltaTime = 0.0;
                uint tempDelta = 0;
                byte tempByte = 0;
                long latencySum = 0;
                int reportOffset = 1;


                // Run continuous calibration on Gyro when starting input loop
                sixAxis.ResetContinuousCalibration();
                standbySw.Start();

                while (!exitInputThread)
                {
                    oldCharging = charging;
                    currerror = string.Empty;

                    if (tempLatencyCount >= 20)
                    {
                        latencySum -= latencyQueue.Dequeue();
                        tempLatencyCount--;
                    }

                    latencySum += this.lastTimeElapsed;
                    latencyQueue.Enqueue(this.lastTimeElapsed);
                    tempLatencyCount++;

                    //Latency = latencyQueue.Average();
                    Latency = latencySum / (double)tempLatencyCount;

                    readWaitEv.Set();

                    Thread.Sleep(1); //aovid high cpu
                    if (!hDevice.readFeatureData(featureReport))
                    {
                        //int winError = Marshal.GetLastWin32Error();
                        //Console.WriteLine(Mac.ToString() + " " + DateTime.UtcNow.ToString("o") + "> disconnect due to read failure: " + winError);
                        //Log.LogToGui(Mac.ToString() + " disconnected due to read failure: " + winError, true);

                        exitInputThread = true;
                        readWaitEv.Reset();
                        StopOutputUpdate();
                        isDisconnecting = true;
                        RunRemoval();

                        timeoutExecuted = true;
                        continue;
                    }

                    readWaitEv.Wait();
                    readWaitEv.Reset();

                    curtime = Stopwatch.GetTimestamp();
                    testelapsed = curtime - oldtime;
                    lastTimeElapsedDouble = testelapsed * (1.0 / Stopwatch.Frequency) * 1000.0;
                    lastTimeElapsed = (long)lastTimeElapsedDouble;
                    oldtime = curtime;

                    utcNow = DateTime.UtcNow; // timestamp with UTC in case system time zone changes

                    cState.PacketCounter = pState.PacketCounter + 1;
                    cState.ReportTimeStamp = utcNow;

                    cState.Share = (featureReport[2 + reportOffset] & 0x01) > 0;
                    cState.L3 = (featureReport[2 + reportOffset] & 0x02) > 0;
                    cState.R3 = (featureReport[2 + reportOffset] & 0x04) > 0;
                    cState.Options = (featureReport[2 + reportOffset] & 0x08) > 0;
                    cState.DpadUp = ((featureReport[2 + reportOffset] & 0x10) > 0) && featureReport[14 + reportOffset] > 0;
                    cState.DpadRight = ((featureReport[2 + reportOffset] & 0x20) > 0) && featureReport[15 + reportOffset] > 0;
                    cState.DpadDown = ((featureReport[2 + reportOffset] & 0x40) > 0) && featureReport[16 + reportOffset] > 0;
                    cState.DpadLeft = ((featureReport[2 + reportOffset] & 0x80) > 0) && featureReport[17 + reportOffset] > 0;
                    cState.L2 = ((featureReport[3 + reportOffset] & 0x01) > 0) ? featureReport[18 + reportOffset] : (byte)0x00;
                    cState.R2 = ((featureReport[3 + reportOffset] & 0x02) > 0) ? featureReport[19 + reportOffset] : (byte)0x00;
                    cState.L1 = ((featureReport[3 + reportOffset] & 0x04) > 0) && featureReport[20 + reportOffset] > 0;
                    cState.R1 = ((featureReport[3 + reportOffset] & 0x08) > 0) && featureReport[21 + reportOffset] > 0;
                    cState.Triangle = ((featureReport[3 + reportOffset] & 0x10) > 0) && featureReport[22 + reportOffset] > 0;
                    cState.Circle = ((featureReport[3 + reportOffset] & 0x20) > 0) && featureReport[23 + reportOffset] > 0;
                    cState.Cross = ((featureReport[3 + reportOffset] & 0x40) > 0) && featureReport[24 + reportOffset] > 0;
                    cState.Square = ((featureReport[3 + reportOffset] & 0x80) > 0) && featureReport[25 + reportOffset] > 0;
                    cState.PS = (featureReport[4 + reportOffset] & 0x01) > 0;

                    cState.L2Btn = cState.L2 > 0;
                    cState.R2Btn = cState.R2 > 0;
                    cState.L2Raw = cState.L2;
                    cState.R2Raw = cState.R2;

                    cState.LX = featureReport[6 + reportOffset];
                    cState.LY = featureReport[7 + reportOffset];
                    cState.RX = featureReport[8 + reportOffset];
                    cState.RY = featureReport[9 + reportOffset];


                    if ((this.featureSet & VidPidFeatureSet.NoBatteryReading) == 0)
                    {
                        tempByte = featureReport[30 + reportOffset];
                        tempCharging = tempByte == 0xEE;
                        if (tempCharging != charging)
                        {
                            charging = tempCharging;
                            ChargingChanged?.Invoke(this, EventArgs.Empty);
                        }

                        tempFull = tempByte == 0xEF; // Check for Full status
                        if (tempFull)
                        {
                            // Full Charge flag found
                            tempBattery = 100;
                        }
                        else if (tempCharging)
                        {
                            // fake it to 50%
                            tempBattery = battery < 50 ? 50 : battery;
                        }
                        else
                        {
                            // Partial charge
                            switch(tempByte)
                            {
                                case 0x01:
                                    tempBattery = 10;
                                    break;
                                case 0x02:
                                    tempBattery = 25;
                                    break;
                                case 0x03:
                                    tempBattery = 50;
                                    break;
                                case 0x04:
                                    tempBattery = 75;
                                    break;
                                case 0x05:
                                    tempBattery = 100;
                                    break;
                            }
                        }

                        if (tempBattery != battery)
                        {
                            battery = tempBattery;
                            BatteryChanged?.Invoke(this, EventArgs.Empty);
                            outputDirty = true;
                        }

                        cState.Battery = (byte)battery;
                        //System.Diagnostics.Debug.WriteLine("CURRENT BATTERY: " + (featureReport[30] & 0x0f) + " | " + tempBattery + " | " + battery);
                    }
                    else
                    {
                        // Some gamepads don't send battery values in DS4 compatible data fields, so use dummy 99% value to avoid constant low battery warnings
                        //priorInputReport30 = 0x0F;
                        battery = 99;
                        cState.Battery = 99;
                    }


                    if (timeStampInit == false)
                    {
                        timeStampInit = true;
                        deltaTimeCurrent = tempStamp * 1u / 3u;
                    }
                    else if (timeStampPrevious > tempStamp)
                    {
                        tempDelta = uint.MaxValue - timeStampPrevious + tempStamp + 1u;
                        deltaTimeCurrent = tempDelta * 1u / 3u;
                    }
                    else
                    {
                        tempDelta = tempStamp - timeStampPrevious;
                        deltaTimeCurrent = tempDelta * 1u / 3u;
                    }

                    //if (tempStamp == timeStampPrevious)
                    //{
                    //    Console.WriteLine("PINEAPPLES");
                    //}

                    // Make sure timestamps don't match
                    if (deltaTimeCurrent != 0)
                    {
                        elapsedDeltaTime = 0.000001 * deltaTimeCurrent; // Convert from microseconds to seconds
                        cState.totalMicroSec = pState.totalMicroSec + deltaTimeCurrent;
                    }
                    else
                    {
                        // Duplicate timestamp. Use system clock for elapsed time instead
                        elapsedDeltaTime = lastTimeElapsedDouble * .001;
                        cState.totalMicroSec = pState.totalMicroSec + (uint)(elapsedDeltaTime * 1000000);
                    }

                    //Console.WriteLine("{0} {1} {2} {3} {4} Diff({5}) TSms({6}) Sys({7})", tempStamp, featureReport[31 + reportOffset], featureReport[30 + reportOffset], featureReport[29 + reportOffset], featureReport[28 + reportOffset], tempStamp - timeStampPrevious, elapsedDeltaTime, lastTimeElapsedDouble * 0.001);

                    cState.elapsedTime = elapsedDeltaTime;
                    cState.ds4Timestamp = (ushort)((tempStamp / 16) % ushort.MaxValue);
                    timeStampPrevious = tempStamp;

                    fixed (byte* pbInput = &featureReport[41+reportOffset], pbGyro = gyro, pbAccel = accel)
                    {
                        for (int i = 0; i < 6; i++)
                        {
                            pbAccel[i] = pbInput[i];
                        }

                        for (int i = 6; i < 8; i++)
                        {
                            pbGyro[i - 6] = pbInput[i];
                        }

                        if (synced)
                        {
                            sixAxis.handleDS3Sixaxis(pbGyro, pbAccel, cState, elapsedDeltaTime);
                        }
                    }

                    /* Debug output of incoming HID data:
                    if (cState.L2 == 0xff && cState.R2 == 0xff)
                    {
                        Console.Write(MacAddress.ToString() + " " + System.DateTime.UtcNow.ToString("o") + ">");
                        for (int i = 0; i < featureReport.Length; i++)
                            Console.Write(" " + featureReport[i].ToString("x2"));
                        Console.WriteLine();
                    }
                    ///*/

                    if (conType == ConnectionType.USB)
                    {
                        if (idleTimeout == 0)
                        {
                            lastActive = utcNow;
                        }
                        else
                        {
                            idleInput = isDS4Idle();
                            if (!idleInput)
                            {
                                lastActive = utcNow;
                            }
                        }
                    }
                    else
                    {
                        bool shouldDisconnect = false;
                        if (!isRemoved && idleTimeout > 0)
                        {
                            idleInput = isDS4Idle();
                            if (idleInput)
                            {
                                DateTime timeout = lastActive + TimeSpan.FromSeconds(idleTimeout);
                                if (!charging)
                                    shouldDisconnect = utcNow >= timeout;
                            }
                            else
                            {
                                lastActive = utcNow;
                            }
                        }
                        else
                        {
                            lastActive = utcNow;
                        }

                        if (shouldDisconnect)
                        {
                            AppLogger.LogToGui(Mac.ToString() + " disconnecting due to idle disconnect", false);

                            if (conType == ConnectionType.BT)
                            {
                                if (DisconnectBT(true))
                                {
                                    exitInputThread = true;
                                    timeoutExecuted = true;
                                    return; // all done
                                }
                            }
                        }
                    }

                    Report?.Invoke(this, EventArgs.Empty);

                    PrepareOutReport();
                    if (outputDirty)
                    {
                        WriteReport();
                        currentHap.dirty = false;
                        previousHapticState = currentHap;
                    }

                    outputDirty = false;
                    currentHap.dirty = false;
                    //forceWrite = false;

                    if (!string.IsNullOrEmpty(currerror))
                        error = currerror;
                    else if (!string.IsNullOrEmpty(error))
                        error = string.Empty;

                    cState.CopyTo(pState);

                    if (hasInputEvts)
                    {
                        lock (eventQueueLock)
                        {
                            Action tempAct = null;
                            for (int actInd = 0, actLen = eventQueue.Count; actInd < actLen; actInd++)
                            {
                                tempAct = eventQueue.Dequeue();
                                tempAct.Invoke();
                            }

                            hasInputEvts = false;
                        }
                    }
                }
            }

            timeoutExecuted = true;
        }

        protected override void StopOutputUpdate()
        {
            SendEmptyOutputReport();
        }

        private void SendEmptyOutputReport()
        {
            Array.Clear(outputReport, 0, outputReport.Length);

            outputReport[0] = OUTPUT_REPORT_ID;
            outputReport[1] = 0x02;
            outputReport[2] = 0x00;
            outputReport[3] = 0x00;

            //struct ds3_rumble
            //{
            //    u8 padding = 0x00;
            //    u8 small_motor_duration = 0xFF; // 0xff means forever
            //    u8 small_motor_on = 0x00; // 0 or 1 (off/on)
            //    u8 large_motor_duration = 0xFF; // 0xff means forever
            //    u8 large_motor_force = 0x00; // 0 to 255
            //};
            outputReport[4] = 0x00;
            outputReport[5] = 0xFF;
            outputReport[6] = 0x00;
            outputReport[7] = 0xFF;
            outputReport[8] = 0x00;

            // padding 4bytes
            outputReport[13] = 0x00; // led_enabled 

            //struct ds3_led
            //{
            //    u8 duration = 0xFF; // total duration, 0xff means forever
            //    u8 interval_duration = 0xFF; // interval duration in deciseconds
            //    u8 enabled = 0x10;
            //    u8 interval_portion_off = 0x00; // in percent (100% = 0xFF)
            //    u8 interval_portion_on = 0xFF; // in percent (100% = 0xFF)
            //};
            // led 1
            outputReport[14] = 0xFF;
            outputReport[15] = 0xFF;
            outputReport[16] = 0x10;
            outputReport[17] = 0x00;
            outputReport[18] = 0xFF;
            // led 2
            outputReport[19] = 0xFF;
            outputReport[20] = 0xFF;
            outputReport[21] = 0x10;
            outputReport[22] = 0x00;
            outputReport[23] = 0xFF;
            // led 3
            outputReport[24] = 0xFF;
            outputReport[25] = 0xFF;
            outputReport[26] = 0x10;
            outputReport[27] = 0x00;
            outputReport[28] = 0xFF;
            // led 4
            outputReport[29] = 0xFF;
            outputReport[30] = 0xFF;
            outputReport[31] = 0x10;
            outputReport[32] = 0x00;
            outputReport[33] = 0xFF;

            WriteReport();
        }

        private unsafe void PrepareOutReport()
        {
            MergeStates();

            bool change = false;
            bool rumbleSet = currentHap.IsRumbleSet();


            outputReport[0] = OUTPUT_REPORT_ID; // Report ID
            outputReport[1] = 0x02;
            outputReport[2] = 0x00;
            outputReport[3] = 0x00;

            //struct ds3_rumble
            //{
            //    u8 padding = 0x00;
            //    u8 small_motor_duration = 0xFF; // 0xff means forever
            //    u8 small_motor_on = 0x00; // 0 or 1 (off/on)
            //    u8 large_motor_duration = 0xFF; // 0xff means forever
            //    u8 large_motor_force = 0x00; // 0 to 255
            //};
            outputReport[4] = 0x00;
            outputReport[5] = 0xFF;
            outputReport[6] = currentHap.rumbleState.RumbleMotorStrengthRightLightFast == 0x00 ? (byte)0x00 : (byte)0x01;
            outputReport[7] = 0xFF;
            outputReport[8] = currentHap.rumbleState.RumbleMotorStrengthLeftHeavySlow;

            // padding 4bytes
            byte led_enable = 0x00;
            if (currentHap.lightbarState.IsLightBarSet())
            {
                if (battery >= 75)
                {
                    led_enable = 0b00011110;
                }
                else if (battery >= 50)
                {
                    led_enable = 0b00001110;
                }
                else if (battery >= 25)
                {
                    led_enable = 0b00000110;
                }
                else
                {
                    led_enable = 0b00000010;
                }
            }
            outputReport[13] = led_enable; // led_enabled 
            //struct ds3_led
            //{
            //    u8 duration = 0xFF; // total duration, 0xff means forever
            //    u8 interval_duration = 0xFF; // interval duration in deciseconds
            //    u8 enabled = 0x10;
            //    u8 interval_portion_off = 0x00; // in percent (100% = 0xFF)
            //    u8 interval_portion_on = 0xFF; // in percent (100% = 0xFF)
            //};
            // led 1
            outputReport[14] = 0xFF;
            outputReport[15] = 0xFF;
            outputReport[16] = 0x10;
            outputReport[17] = 0x00;
            outputReport[18] = 0xFF;
            // led 2
            outputReport[19] = 0xFF;
            outputReport[20] = 0xFF;
            outputReport[21] = 0x10;
            outputReport[22] = 0x00;
            outputReport[23] = 0xFF;
            // led 3
            outputReport[24] = 0xFF;
            outputReport[25] = 0xFF;
            outputReport[26] = 0x10;
            outputReport[27] = 0x00;
            outputReport[28] = 0xFF;
            // led 4
            if (currentHap.lightbarState.IsLightBarSet()
                && currentHap.lightbarState.LightBarFlashDurationOn > 0
                && currentHap.lightbarState.LightBarFlashDurationOff > 0)
            {
                int sum = currentHap.lightbarState.LightBarFlashDurationOn + currentHap.lightbarState.LightBarFlashDurationOff;
                outputReport[29] = 0xFF;
                outputReport[30] = 0x14; //2secs
                outputReport[31] = 0x10;
                outputReport[32] = (byte)(currentHap.lightbarState.LightBarFlashDurationOn * 255 / sum);
                outputReport[33] = (byte)(currentHap.lightbarState.LightBarFlashDurationOff * 255 / sum);
            }
            else
            {
                outputReport[29] = 0xFF;
                outputReport[30] = 0xFF;
                outputReport[31] = 0x10;
                outputReport[32] = 0x00;
                outputReport[33] = 0xFF;
            }

            if (currentHap.dirty || !previousHapticState.Equals(currentHap))
            {
                change = true;
            }

            if (change)
            {
                //Console.WriteLine("DIRTY");
                outputDirty = true;
                if (rumbleSet)
                {
                    standbySw.Restart();
                }
                else
                {
                    standbySw.Reset();
                }

                //outReportBuffer.CopyTo(outputReport, 0);
            }
            else if (rumbleSet && standbySw.ElapsedMilliseconds >= 4000L)
            {
                outputDirty = true;
                standbySw.Restart();
            }
            //bool res = hDevice.WriteOutputReportViaInterrupt(outputReport, READ_STREAM_TIMEOUT);
            //Console.WriteLine("STAUTS: {0}", res);

        }

        private bool WriteReport()
        {
            var result = hDevice.WriteOutputReportViaInterrupt(outputReport, READ_STREAM_TIMEOUT);

            //Console.WriteLine("STAUTS: {0}", result);
            return result;
        }

        private void Detach()
        {
            SendEmptyOutputReport();
        }
    }
}
