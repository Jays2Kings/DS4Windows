using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DS4Windows;

namespace DS4Windows.InputDevices
{
    public class DualSenseDevice : DS4Device
    {
        public class GyroMouseSensDualSense : GyroMouseSens
        {
            private const double MOUSE_COEFFICIENT = 0.009;
            private const double MOUSE_OFFSET = 0.15;
            private const double SMOOTH_MOUSE_OFFSET = 0.15;

            public GyroMouseSensDualSense() : base()
            {
                mouseCoefficient = MOUSE_COEFFICIENT;
                mouseOffset = MOUSE_OFFSET;
                mouseSmoothOffset = SMOOTH_MOUSE_OFFSET;
            }
        }

        public abstract class InputReportDataBytes
        {
            public const int REPORT_OFFSET = 0;

            public const int REPORT_ID = 0;
            public const int LX = 1;
            public const int LY = 2;
        }

        public class InputReportDataBytesUSB : InputReportDataBytes
        {
        }

        public class InputReportDataBytesBT : InputReportDataBytesUSB
        {
            public new const int REPORT_OFFSET = 2;

            public new const int REPORT_ID = InputReportDataBytes.REPORT_ID;
            public new const int LX = InputReportDataBytes.LX + REPORT_OFFSET;
            public new const int LY = InputReportDataBytes.LY + REPORT_OFFSET;
        }

        public struct TriggerEffectData
        {
            public byte triggerMotorMode;
            public byte triggerStartResistance;
            public byte triggerEffectForce;
            public byte triggerRangeForce;
            public byte triggerNearReleaseStrength;
            public byte triggerNearMiddleStrength;
            public byte triggerPressedStrength;
            public byte triggerActuationFrequency;

            public void ChangeData(TriggerEffects effect)
            {
                switch (effect)
                {
                    case TriggerEffects.None:
                        triggerMotorMode = triggerStartResistance = triggerEffectForce =
                            triggerRangeForce = triggerNearReleaseStrength = triggerNearMiddleStrength =
                            triggerPressedStrength = triggerActuationFrequency = 0;
                        break;
                    case TriggerEffects.FullClick:
                        triggerMotorMode = 0x02;
                        triggerStartResistance = 0xA4;
                        triggerEffectForce = 0xB4;
                        triggerRangeForce = 0xFF;
                        triggerNearReleaseStrength = 0x00;
                        triggerNearMiddleStrength = 0x00;
                        triggerPressedStrength = 0x00;
                        triggerActuationFrequency = 0x00;
                        break;
                    case TriggerEffects.Rigid:
                        triggerMotorMode = 0x01;
                        triggerStartResistance = 0x00;
                        triggerEffectForce = 0x00;
                        triggerRangeForce = 0x00;
                        triggerNearReleaseStrength = 0x00;
                        triggerNearMiddleStrength = 0x00;
                        triggerPressedStrength = 0x00;
                        triggerActuationFrequency = 0x00;
                        break;
                    case TriggerEffects.Pulse:
                        triggerMotorMode = 0x02;
                        triggerStartResistance = 0x00;
                        triggerEffectForce = 0x00;
                        triggerRangeForce = 0x00;
                        triggerNearReleaseStrength = 0x00;
                        triggerNearMiddleStrength = 0x00;
                        triggerPressedStrength = 0x00;
                        triggerActuationFrequency = 0x00;
                        break;
                    default:
                        break;
                }
            }
        }

        public enum HapticIntensity : uint
        {
            Low,
            Medium,
            High,
        }

        private const int BT_REPORT_OFFSET = 2;
        private InputReportDataBytes dataBytes;
        protected new const int BT_OUTPUT_REPORT_LENGTH = 78;
        private new const int BT_INPUT_REPORT_LENGTH = 78;
        protected const int TOUCHPAD_DATA_OFFSET = 33;
        private new const int BATTERY_MAX = 8;

        public new const byte SERIAL_FEATURE_ID = 9;
        public override byte SerialReportID { get => SERIAL_FEATURE_ID; }

        private const byte OUTPUT_REPORT_ID_USB = 0x02;
        private const byte OUTPUT_REPORT_ID_BT = 0x31;
        private const byte OUTPUT_REPORT_ID_DATA = 0x02;
        private new const byte USB_OUTPUT_CHANGE_LENGTH = 48;
        private const int OUTPUT_MIN_COUNT_BT = 20;
        private const byte LED_PLAYER_BAR_TOGGLE = 0x10;
        private bool timeStampInit = false;
        private uint timeStampPrevious = 0;
        private uint deltaTimeCurrent = 0;
        private bool outputDirty = false;
        private DS4HapticState previousHapticState = new DS4HapticState();
        private byte[] outputBTCrc32Head = new byte[] { 0xA2 };
        //private byte outputPendCount = 0;
        private new GyroMouseSensDualSense gyroMouseSensSettings;
        public override GyroMouseSens GyroMouseSensSettings { get => gyroMouseSensSettings; }

        private byte activePlayerLEDMask = 0x00;

        private byte hapticsIntensityByte = 0x02;
        public HapticIntensity HapticChoice {
            set
            {
                switch (value)
                {
                    case HapticIntensity.Low:
                        hapticsIntensityByte = 0x05;
                        break;
                    case HapticIntensity.High:
                        hapticsIntensityByte = 0x00;
                        break;
                    case HapticIntensity.Medium:
                    default:
                        hapticsIntensityByte = 0x02;
                        break;
                }
            }
        }

        private TriggerEffectData l2EffectData;
        private TriggerEffectData r2EffectData;

        private byte muteLEDByte = 0x00;

        private DualSenseControllerOptions nativeOptionsStore;
        public DualSenseControllerOptions NativeOptionsStore { get => nativeOptionsStore; }

        public override event ReportHandler<EventArgs> Report = null;
        public override event EventHandler BatteryChanged;
        public override event EventHandler ChargingChanged;

        public DualSenseDevice(HidDevice hidDevice, string disName, VidPidFeatureSet featureSet = VidPidFeatureSet.DefaultDS4) :
            base(hidDevice, disName, featureSet)
        {
            synced = true;
            DeviceSlotNumberChanged += (sender, e) => {
                CalculateDeviceSlotMask();
            };

            BatteryChanged += (sender, e) =>
            {
                PreparePlayerLEDBarByte();
            };
        }

        public override void PostInit()
        {
            HidDevice hidDevice = hDevice;
            deviceType = InputDeviceType.DualSense;
            gyroMouseSensSettings = new GyroMouseSensDualSense();
            optionsStore = nativeOptionsStore = new DualSenseControllerOptions(deviceType);
            SetupOptionsEvents();

            conType = DetermineConnectionType(hDevice);

            if (conType == ConnectionType.USB)
            {
                dataBytes = new InputReportDataBytesUSB();

                inputReport = new byte[64];
                outputReport = new byte[hDevice.Capabilities.OutputReportByteLength];
                outReportBuffer = new byte[hDevice.Capabilities.OutputReportByteLength];

                warnInterval = WARN_INTERVAL_USB;
            }
            else
            {
                //btInputReport = new byte[BT_INPUT_REPORT_LENGTH];
                //inputReport = new byte[BT_INPUT_REPORT_LENGTH - 2];
                // Only plan to use one input report array. Avoid copying data
                inputReport = new byte[BT_INPUT_REPORT_LENGTH];
                // Default DS4 logic while writing data to gamepad
                outputReport = new byte[BT_OUTPUT_REPORT_LENGTH];
                outReportBuffer = new byte[BT_OUTPUT_REPORT_LENGTH];

                warnInterval = WARN_INTERVAL_BT;
                synced = isValidSerial();
            }

            if (runCalib)
                RefreshCalibration();

            if (!hDevice.IsFileStreamOpen())
            {
                hDevice.OpenFileStream(outputReport.Length);
            }

            // Need to blank LED lights so lightbar will change colors
            // as requested
            if (conType == ConnectionType.BT)
            {
                SendInitialBTOutputReport();
            }
        }

        public static ConnectionType DetermineConnectionType(HidDevice hidDevice)
        {
            ConnectionType result;
            if (hidDevice.Capabilities.InputReportByteLength == 64)
            {
                result = ConnectionType.USB;
            }
            else
            {
                result = ConnectionType.BT;
            }

            return result;
        }

        public override bool DisconnectBT(bool callRemoval = false)
        {
            return base.DisconnectBT(callRemoval);
        }

        public override bool DisconnectDongle(bool remove = false)
        {
            // Do Nothing
            return true;
        }

        public override bool DisconnectWireless(bool callRemoval = false)
        {
            return base.DisconnectWireless(callRemoval);
        }

        public override bool IsAlive()
        {
            return synced;
        }

        public override void RefreshCalibration()
        {
            byte[] calibration = new byte[41];
            calibration[0] = conType == ConnectionType.BT ? (byte)0x05 : (byte)0x05;

            if (conType == ConnectionType.BT)
            {
                bool found = false;
                for (int tries = 0; !found && tries < 5; tries++)
                {
                    hDevice.readFeatureData(calibration);
                    uint recvCrc32 = calibration[DS4_FEATURE_REPORT_5_CRC32_POS] |
                                (uint)(calibration[DS4_FEATURE_REPORT_5_CRC32_POS + 1] << 8) |
                                (uint)(calibration[DS4_FEATURE_REPORT_5_CRC32_POS + 2] << 16) |
                                (uint)(calibration[DS4_FEATURE_REPORT_5_CRC32_POS + 3] << 24);

                    uint calcCrc32 = ~Crc32Algorithm.Compute(new byte[] { 0xA3 });
                    calcCrc32 = ~Crc32Algorithm.CalculateBasicHash(ref calcCrc32, ref calibration, 0, DS4_FEATURE_REPORT_5_LEN - 4);
                    bool validCrc = recvCrc32 == calcCrc32;
                    if (!validCrc && tries >= 5)
                    {
                        AppLogger.LogToGui("Gyro Calibration Failed", true);
                        continue;
                    }
                    else if (validCrc)
                    {
                        found = true;
                    }
                }

                sixAxis.setCalibrationData(ref calibration, true);
            }
            else
            {
                hDevice.readFeatureData(calibration);
                sixAxis.setCalibrationData(ref calibration, true);
            }
        }

        public override void StartUpdate()
        {
            this.inputReportErrorCount = 0;

            if (ds4Input == null)
            {
                if (conType == ConnectionType.BT)
                {
                    //ds4Output = new Thread(performDs4Output);
                    //ds4Output.Priority = ThreadPriority.Normal;
                    //ds4Output.Name = "DS4 Output thread: " + Mac;
                    //ds4Output.IsBackground = true;
                    //ds4Output.Start();

                    timeoutCheckThread = new Thread(TimeoutTestThread);
                    timeoutCheckThread.Priority = ThreadPriority.BelowNormal;
                    timeoutCheckThread.Name = "DualSense Timeout thread: " + Mac;
                    timeoutCheckThread.IsBackground = true;
                    timeoutCheckThread.Start();
                }
                //else
                //{
                //    ds4Output = new Thread(OutReportCopy);
                //    ds4Output.Priority = ThreadPriority.Normal;
                //    ds4Output.Name = "DS4 Arr Copy thread: " + Mac;
                //    ds4Output.IsBackground = true;
                //    ds4Output.Start();
                //}

                ds4Input = new Thread(ReadInput);
                ds4Input.Priority = ThreadPriority.AboveNormal;
                ds4Input.Name = "DualSense Input thread: " + Mac;
                ds4Input.IsBackground = true;
                ds4Input.Start();
            }
            else
                Console.WriteLine("Thread already running for DS4: " + Mac);
        }

        private void TimeoutTestThread()
        {
            while (!timeoutExecuted)
            {
                if (timeoutEvent)
                {
                    timeoutExecuted = true;

                    // Request serial feature report data. Causes Windows to notice the dead
                    // device.
                    byte[] tmpFeatureData = new byte[64];
                    tmpFeatureData[0] = SERIAL_FEATURE_ID;
                    hDevice.readFeatureData(tmpFeatureData); // Kick Windows into noticing the disconnection.
                }
                else
                {
                    timeoutEvent = true;
                    Thread.Sleep(READ_STREAM_TIMEOUT);
                }
            }
        }

        private unsafe void ReadInput()
        {
            unchecked
            {
                firstActive = DateTime.UtcNow;
                NativeMethods.HidD_SetNumInputBuffers(hDevice.safeReadHandle.DangerousGetHandle(), 2);
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
                bool forceWrite = false;

                int maxBatteryValue = 0;
                int tempBattery = 0;
                bool tempCharging = charging;
                bool tempFull = false;
                uint tempStamp = 0;
                double elapsedDeltaTime = 0.0;
                uint tempDelta = 0;
                byte tempByte = 0;
                int CRC32_POS_1 = BT_INPUT_REPORT_CRC32_POS + 1,
                    CRC32_POS_2 = BT_INPUT_REPORT_CRC32_POS + 2,
                    CRC32_POS_3 = BT_INPUT_REPORT_CRC32_POS + 3;
                int crcpos = BT_INPUT_REPORT_CRC32_POS;
                int crcoffset = 0;
                long latencySum = 0;
                int reportOffset = conType == ConnectionType.BT ? 1 : 0;
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

                    if (conType == ConnectionType.BT)
                    {
                        timeoutEvent = false;
                        HidDevice.ReadStatus res = hDevice.ReadWithFileStream(inputReport);
                        if (res == HidDevice.ReadStatus.Success)
                        {
                            uint recvCrc32 = inputReport[BT_INPUT_REPORT_CRC32_POS] |
                                (uint)(inputReport[CRC32_POS_1] << 8) |
                                (uint)(inputReport[CRC32_POS_2] << 16) |
                                (uint)(inputReport[CRC32_POS_3] << 24);

                            uint calcCrc32 = ~Crc32Algorithm.CalculateFasterBT78Hash(ref HamSeed, ref inputReport, ref crcoffset, ref crcpos);
                            if (recvCrc32 != calcCrc32)
                            {
                                cState.PacketCounter = pState.PacketCounter + 1; //still increase so we know there were lost packets
                                if (this.inputReportErrorCount >= 10)
                                {
                                    exitInputThread = true;

                                    readWaitEv.Reset();
                                    //sendOutputReport(true, true); // Kick Windows into noticing the disconnection.
                                    StopOutputUpdate();
                                    isDisconnecting = true;
                                    RunRemoval();

                                    timeoutExecuted = true;
                                    continue;
                                }
                                else
                                {
                                    this.inputReportErrorCount++;
                                }

                                readWaitEv.Reset();
                                continue;
                            }
                        }
                        else
                        {
                            if (res == HidDevice.ReadStatus.WaitTimedOut)
                            {
                                AppLogger.LogToGui(Mac.ToString() + " disconnected due to timeout", true);
                            }
                            else
                            {
                                int winError = Marshal.GetLastWin32Error();
                                Console.WriteLine(Mac.ToString() + " " + DateTime.UtcNow.ToString("o") + "> disconnect due to read failure: " + winError);
                                //Log.LogToGui(Mac.ToString() + " disconnected due to read failure: " + winError, true);
                            }

                            exitInputThread = true;
                            readWaitEv.Reset();
                            //SendEmptyOutputReport();
                            //sendOutputReport(true, true); // Kick Windows into noticing the disconnection.
                            StopOutputUpdate();
                            isDisconnecting = true;
                            RunRemoval();

                            timeoutExecuted = true;
                            continue;
                        }
                    }
                    else
                    {
                        HidDevice.ReadStatus res = hDevice.ReadWithFileStream(inputReport);
                        if (res != HidDevice.ReadStatus.Success)
                        {
                            if (res == HidDevice.ReadStatus.WaitTimedOut)
                            {
                                AppLogger.LogToGui(Mac.ToString() + " disconnected due to timeout", true);
                            }
                            else
                            {
                                int winError = Marshal.GetLastWin32Error();
                                Console.WriteLine(Mac.ToString() + " " + DateTime.UtcNow.ToString("o") + "> disconnect due to read failure: " + winError);
                                //Log.LogToGui(Mac.ToString() + " disconnected due to read failure: " + winError, true);
                            }

                            exitInputThread = true;
                            readWaitEv.Reset();
                            StopOutputUpdate();
                            isDisconnecting = true;
                            RunRemoval();

                            timeoutExecuted = true;
                            continue;
                        }
                    }

                    readWaitEv.Wait();
                    readWaitEv.Reset();

                    curtime = Stopwatch.GetTimestamp();
                    testelapsed = curtime - oldtime;
                    lastTimeElapsedDouble = testelapsed * (1.0 / Stopwatch.Frequency) * 1000.0;
                    lastTimeElapsed = (long)lastTimeElapsedDouble;
                    oldtime = curtime;

                    if (conType == ConnectionType.BT && inputReport[0] != 0x31)
                    {
                        // Received incorrect report, skip it
                        continue;
                    }

                    utcNow = DateTime.UtcNow; // timestamp with UTC in case system time zone changes

                    cState.PacketCounter = pState.PacketCounter + 1;
                    cState.ReportTimeStamp = utcNow;
                    cState.LX = inputReport[1 + reportOffset];
                    cState.LY = inputReport[2 + reportOffset];
                    cState.RX = inputReport[3 + reportOffset];
                    cState.RY = inputReport[4 + reportOffset];
                    cState.L2 = inputReport[5 + reportOffset];
                    cState.R2 = inputReport[6 + reportOffset];

                    tempByte = inputReport[8 + reportOffset];
                    cState.Triangle = (tempByte & (1 << 7)) != 0;
                    cState.Circle = (tempByte & (1 << 6)) != 0;
                    cState.Cross = (tempByte & (1 << 5)) != 0;
                    cState.Square = (tempByte & (1 << 4)) != 0;

                    // First 4 bits denote dpad state. Clock representation
                    // with 8 meaning centered and 0 meaning DpadUp.
                    byte dpad_state = (byte)(tempByte & 0x0F);

                    switch (dpad_state)
                    {
                        case 0: cState.DpadUp = true; cState.DpadDown = false; cState.DpadLeft = false; cState.DpadRight = false; break;
                        case 1: cState.DpadUp = true; cState.DpadDown = false; cState.DpadLeft = false; cState.DpadRight = true; break;
                        case 2: cState.DpadUp = false; cState.DpadDown = false; cState.DpadLeft = false; cState.DpadRight = true; break;
                        case 3: cState.DpadUp = false; cState.DpadDown = true; cState.DpadLeft = false; cState.DpadRight = true; break;
                        case 4: cState.DpadUp = false; cState.DpadDown = true; cState.DpadLeft = false; cState.DpadRight = false; break;
                        case 5: cState.DpadUp = false; cState.DpadDown = true; cState.DpadLeft = true; cState.DpadRight = false; break;
                        case 6: cState.DpadUp = false; cState.DpadDown = false; cState.DpadLeft = true; cState.DpadRight = false; break;
                        case 7: cState.DpadUp = true; cState.DpadDown = false; cState.DpadLeft = true; cState.DpadRight = false; break;
                        case 8:
                        default: cState.DpadUp = false; cState.DpadDown = false; cState.DpadLeft = false; cState.DpadRight = false; break;
                    }

                    tempByte = inputReport[9 + reportOffset];
                    cState.R3 = (tempByte & (1 << 7)) != 0;
                    cState.L3 = (tempByte & (1 << 6)) != 0;
                    cState.Options = (tempByte & (1 << 5)) != 0;
                    cState.Share = (tempByte & (1 << 4)) != 0;
                    cState.R2Btn = (tempByte & (1 << 3)) != 0;
                    cState.L2Btn = (tempByte & (1 << 2)) != 0;
                    cState.R1 = (tempByte & (1 << 1)) != 0;
                    cState.L1 = (tempByte & (1 << 0)) != 0;

                    tempByte = inputReport[10 + reportOffset];
                    cState.PS = (tempByte & (1 << 0)) != 0;
                    cState.TouchButton = (tempByte & 0x02) != 0;
                    cState.OutputTouchButton = cState.TouchButton;
                    cState.Mute = (tempByte & (1 << 2)) != 0;
                    //cState.FrameCounter = (byte)(tempByte >> 2);

                    if ((this.featureSet & VidPidFeatureSet.NoBatteryReading) == 0)
                    {
                        tempByte = inputReport[54 + reportOffset];
                        tempCharging = (tempByte & 0x08) != 0;
                        if (tempCharging != charging)
                        {
                            charging = tempCharging;
                            ChargingChanged?.Invoke(this, EventArgs.Empty);
                        }

                        tempByte = inputReport[53 + reportOffset];
                        tempFull = (tempByte & 0x20) != 0; // Check for Full status
                        maxBatteryValue = BATTERY_MAX;
                        if (tempFull)
                        {
                            // Full Charge flag found
                            tempBattery = 100;
                        }
                        else
                        {
                            // Partial charge
                            tempBattery = (tempByte & 0x0F) * 100 / maxBatteryValue;
                            tempBattery = Math.Min(tempBattery, 100);
                        }

                        if (tempBattery != battery)
                        {
                            battery = tempBattery;
                            BatteryChanged?.Invoke(this, EventArgs.Empty);
                        }

                        cState.Battery = (byte)battery;
                        //System.Diagnostics.Debug.WriteLine("CURRENT BATTERY: " + (inputReport[30] & 0x0f) + " | " + tempBattery + " | " + battery);
                    }
                    else
                    {
                        // Some gamepads don't send battery values in DS4 compatible data fields, so use dummy 99% value to avoid constant low battery warnings
                        //priorInputReport30 = 0x0F;
                        battery = 99;
                        cState.Battery = 99;
                    }

                    tempStamp = inputReport[28+reportOffset] |
                                (uint)(inputReport[29+reportOffset] << 8) |
                                (uint)(inputReport[30+reportOffset] << 16) |
                                (uint)(inputReport[31+reportOffset] << 24);

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

                    //Console.WriteLine("{0} {1} {2} {3} {4} Diff({5}) TSms({6}) Sys({7})", tempStamp, inputReport[31 + reportOffset], inputReport[30 + reportOffset], inputReport[29 + reportOffset], inputReport[28 + reportOffset], tempStamp - timeStampPrevious, elapsedDeltaTime, lastTimeElapsedDouble * 0.001);

                    cState.elapsedTime = elapsedDeltaTime;
                    cState.ds4Timestamp = (ushort)((tempStamp / 16) % ushort.MaxValue);
                    timeStampPrevious = tempStamp;

                    //elapsedDeltaTime = lastTimeElapsedDouble * .001;
                    //cState.elapsedTime = elapsedDeltaTime;
                    //cState.totalMicroSec = pState.totalMicroSec + (uint)(elapsedDeltaTime * 1000000);

                    // Simpler touch storing
                    cState.TrackPadTouch0.RawTrackingNum = inputReport[33+reportOffset];
                    cState.TrackPadTouch0.Id = (byte)(inputReport[33+reportOffset] & 0x7f);
                    cState.TrackPadTouch0.IsActive = (inputReport[33+reportOffset] & 0x80) == 0;
                    cState.TrackPadTouch0.X = (short)(((ushort)(inputReport[35+reportOffset] & 0x0f) << 8) | (ushort)(inputReport[34+reportOffset]));
                    cState.TrackPadTouch0.Y = (short)(((ushort)(inputReport[36+reportOffset]) << 4) | ((ushort)(inputReport[35+reportOffset] & 0xf0) >> 4));

                    cState.TrackPadTouch0.RawTrackingNum = inputReport[37+reportOffset];
                    cState.TrackPadTouch1.Id = (byte)(inputReport[37+reportOffset] & 0x7f);
                    cState.TrackPadTouch1.IsActive = (inputReport[37+reportOffset] & 0x80) == 0;
                    cState.TrackPadTouch1.X = (short)(((ushort)(inputReport[39+reportOffset] & 0x0f) << 8) | (ushort)(inputReport[38+reportOffset]));
                    cState.TrackPadTouch1.Y = (short)(((ushort)(inputReport[40+reportOffset]) << 4) | ((ushort)(inputReport[39+reportOffset] & 0xf0) >> 4));

                    // XXX DS4State mapping needs fixup, turn touches into an array[4] of structs.  And include the touchpad details there instead.
                    try
                    {
                        // Only care if one touch packet is detected. Other touch packets
                        // don't seem to contain relevant data. ds4drv does not use them either.
                        int touchOffset = 0;

                        cState.TouchPacketCounter = inputReport[-1 + TOUCHPAD_DATA_OFFSET + reportOffset + touchOffset];
                        cState.Touch1 = (inputReport[0 + TOUCHPAD_DATA_OFFSET + reportOffset + touchOffset] >> 7) != 0 ? false : true; // finger 1 detected
                        cState.Touch1Identifier = (byte)(inputReport[0 + TOUCHPAD_DATA_OFFSET + reportOffset + touchOffset] & 0x7f);
                        cState.Touch2 = (inputReport[4 + TOUCHPAD_DATA_OFFSET + reportOffset + touchOffset] >> 7) != 0 ? false : true; // finger 2 detected
                        cState.Touch2Identifier = (byte)(inputReport[4 + TOUCHPAD_DATA_OFFSET + reportOffset + touchOffset] & 0x7f);
                        cState.Touch1Finger = cState.Touch1 || cState.Touch2; // >= 1 touch detected
                        cState.Touch2Fingers = cState.Touch1 && cState.Touch2; // 2 touches detected
                        int touchX = (((inputReport[2 + TOUCHPAD_DATA_OFFSET + reportOffset + touchOffset] & 0xF) << 8) | inputReport[1 + TOUCHPAD_DATA_OFFSET + reportOffset + touchOffset]);
                        cState.TouchLeft = touchX >= DS4Touchpad.RESOLUTION_X_MAX * 2 / 5 ? false : true;
                        cState.TouchRight = touchX < DS4Touchpad.RESOLUTION_X_MAX * 2 / 5 ? false : true;
                        // Even when idling there is still a touch packet indicating no touch 1 or 2
                        if (synced)
                        {
                            touchpad.handleTouchpad(inputReport, cState, TOUCHPAD_DATA_OFFSET + reportOffset, touchOffset);
                        }
                    }
                    catch (Exception ex) { currerror = $"Touchpad: {ex.Message}"; }

                    fixed (byte* pbInput = &inputReport[16+reportOffset], pbGyro = gyro, pbAccel = accel)
                    {
                        for (int i = 0; i < 6; i++)
                        {
                            pbGyro[i] = pbInput[i];
                        }

                        for (int i = 6; i < 12; i++)
                        {
                            pbAccel[i - 6] = pbInput[i];
                        }

                        if (synced)
                        {
                            sixAxis.handleSixaxis(pbGyro, pbAccel, cState, elapsedDeltaTime);
                        }
                    }

                    /* Debug output of incoming HID data:
                    if (cState.L2 == 0xff && cState.R2 == 0xff)
                    {
                        Console.Write(MacAddress.ToString() + " " + System.DateTime.UtcNow.ToString("o") + ">");
                        for (int i = 0; i < inputReport.Length; i++)
                            Console.Write(" " + inputReport[i].ToString("x2"));
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
                        previousHapticState = currentHap;
                    }

                    outputDirty = false;
                    forceWrite = false;

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
            int reportOffset = conType == ConnectionType.BT ? 1 : 0;
            Array.Clear(outputReport, 0, outputReport.Length);

            outputReport[0] = conType == ConnectionType.USB ? OUTPUT_REPORT_ID_USB :
                OUTPUT_REPORT_ID_BT;

            // Disable haptics and trigger motors
            outputReport[1 + reportOffset] = useRumble ? (byte)0x0F : (byte)0x0C;
            outputReport[2 + reportOffset] = 0x15; // Toggle all LED lights. 0x01 | 0x04 | 0x10

            if (conType == ConnectionType.BT)
            {
                outputReport[1] = OUTPUT_REPORT_ID_DATA;

                // Need to calculate and populate CRC32 data so controller will accept the report
                uint calcCrc32 = ~Crc32Algorithm.Compute(outputBTCrc32Head);
                calcCrc32 = ~Crc32Algorithm.CalculateBasicHash(ref calcCrc32, ref outputReport, 0, BT_OUTPUT_REPORT_LENGTH - 4);
                outputReport[74] = (byte)calcCrc32;
                outputReport[75] = (byte)(calcCrc32 >> 8);
                outputReport[76] = (byte)(calcCrc32 >> 16);
                outputReport[77] = (byte)(calcCrc32 >> 24);
            }

            WriteReport();
            //hDevice.fileStream.Flush();
        }

        private void SendInitialBTOutputReport()
        {
            Array.Clear(outputReport, 0, outputReport.Length);

            outputReport[0] = OUTPUT_REPORT_ID_BT; // Report ID
            outputReport[1] = OUTPUT_REPORT_ID_DATA;
            outputReport[3] = 0x15; // Toggle all LED lights. 0x01 | 0x04 | 0x10

            // Need to calculate and populate CRC32 data so controller will accept the report
            uint calcCrc32 = ~Crc32Algorithm.Compute(outputBTCrc32Head);
            calcCrc32 = ~Crc32Algorithm.CalculateBasicHash(ref calcCrc32, ref outputReport, 0, BT_OUTPUT_REPORT_LENGTH - 4);
            outputReport[74] = (byte)calcCrc32;
            outputReport[75] = (byte)(calcCrc32 >> 8);
            outputReport[76] = (byte)(calcCrc32 >> 16);
            outputReport[77] = (byte)(calcCrc32 >> 24);

            WriteReport();
        }

        private unsafe void PrepareOutReport()
        {
            MergeStates();

            bool change = false;
            bool rumbleSet = currentHap.IsRumbleSet();

            if (conType == ConnectionType.USB)
            {
                outputReport[0] = OUTPUT_REPORT_ID_USB; // Report ID
                // 0x01 Set the main motors (also requires flag 0x02)
                // 0x02 Set the main motors (also requires flag 0x01)
                // 0x04 Set the right trigger motor
                // 0x08 Set the left trigger motor
                // 0x10 Enable modification of audio volume
                // 0x20 Enable internal speaker (even while headset is connected)
                // 0x40 Enable modification of microphone volume
                // 0x80 Enable internal mic (even while headset is connected)
                outputReport[1] = useRumble ? (byte)0x0F : (byte)0x0C; // 0x02 | 0x01 | 0x04 | 0x08;

                // 0x01 Toggling microphone LED, 0x02 Toggling Audio/Mic Mute
                // 0x04 Toggling LED strips on the sides of the Touchpad, 0x08 Turn off all LED lights
                // 0x10 Toggle player LED lights below Touchpad, 0x20 ???
                // 0x40 Adjust overall motor/effect power, 0x80 ???
                outputReport[2] = 0x55; // 0x04 | 0x01 | 0x10 | 0x40

                if (useRumble)
                {
                    // Right? High Freq Motor
                    outputReport[3] = currentHap.rumbleState.RumbleMotorStrengthRightLightFast;
                    // Left? Low Freq Motor
                    outputReport[4] = currentHap.rumbleState.RumbleMotorStrengthLeftHeavySlow;
                }

                /*
                // Headphone volume
                outputReport[5] = 0x00;
                outputReport[5] = Convert.ToByte(audio.getVolume()); // Left and Right
                // Internal speaker volume
                outputReport[6] = 0x00;
                // Internal microphone volume
                outputReport[7] = 0x00;
                outputReport[7] = Convert.ToByte(micAudio.getVolume());
                // 0x01 Enable internal microphone, 0x10 Disable attached headphones (must set 0x20 as well)
                // 0x20 Enable internal speaker
                outputReport[8] = 0x00;
                //*/

                // Mute button LED. 0x01 = Solid. 0x02 = Pulsating
                outputReport[9] = muteLEDByte;

                // audio settings requiring mute toggling flags
                //outputReport[10] = 0x00; // 0x10 microphone mute, 0x40 audio mute

                /* TRIGGER MOTORS  */
                // R2 Effects
                outputReport[11] = r2EffectData.triggerMotorMode; // right trigger motor mode (0 = no resistance, 1 = continuous resistance, 2 = section resistance, 0x20 and 0x04 enable additional effects together with 1 and 2 (configuration yet unknown), 252 = likely a calibration program* / PS Remote Play defaults this to 5; bit 4 only disables the motor?)
                outputReport[12] = r2EffectData.triggerStartResistance; // right trigger start of resistance section 0-255 (0 = released state; 0xb0 roughly matches trigger value 0xff); in mode 26 this field has something to do with motor re-extension after a press-release-cycle (0 = no re-extension)
                outputReport[13] = r2EffectData.triggerEffectForce; // right trigger
                                         // (mode1) amount of force exerted; 0-255
                                         // (mode2) end of resistance section (>= begin of resistance section is enforced); 0xff makes it behave like mode1
                                         // (supplemental mode 4+20) flag(s?) 0x02 = do not pause effect when fully pressed
                outputReport[14] = r2EffectData.triggerRangeForce; // right trigger force exerted in range (mode2), 0-255
                outputReport[15] = r2EffectData.triggerNearReleaseStrength; // strength of effect near release state (requires supplement modes 4 and 20)
                outputReport[16] = r2EffectData.triggerNearMiddleStrength; // strength of effect near middle (requires supplement modes 4 and 20)
                outputReport[17] = r2EffectData.triggerPressedStrength; // strength of effect at pressed state (requires supplement modes 4 and 20)
                outputReport[20] = r2EffectData.triggerActuationFrequency; // effect actuation frequency in Hz (requires supplement modes 4 and 20)


                // L2 Effects
                outputReport[22] = l2EffectData.triggerMotorMode; // left trigger motor mode (0 = no resistance, 1 = continuous resistance, 2 = section resistance, 0x20 and 0x04 enable additional effects together with 1 and 2 (configuration yet unknown), 252 = likely a calibration program* / PS Remote Play defaults this to 5; bit 4 only disables the motor?)
                outputReport[23] = l2EffectData.triggerStartResistance; // left trigger start of resistance section 0-255 (0 = released state; 0xb0 roughly matches trigger value 0xff); in mode 26 this field has something to do with motor re-extension after a press-release-cycle (0 = no re-extension)
                outputReport[24] = l2EffectData.triggerEffectForce; // left trigger
                                         // (mode1) amount of force exerted; 0-255
                                         // (mode2) end of resistance section (>= begin of resistance section is enforced); 0xff makes it behave like mode1
                                         // (supplemental mode 4+20) flag(s?) 0x02 = do not pause effect when fully pressed
                outputReport[25] = l2EffectData.triggerRangeForce; // left trigger: (mode2) amount of force exerted within range; 0-255
                outputReport[26] = l2EffectData.triggerNearReleaseStrength; // strength of effect near release state (requires supplement modes 4 and 20)
                outputReport[27] = l2EffectData.triggerNearMiddleStrength; // strength of effect near middle (requires supplement modes 4 and 20)
                outputReport[28] = l2EffectData.triggerPressedStrength; // strength of effect at pressed state (requires supplement modes 4 and 20)
                outputReport[31] = l2EffectData.triggerActuationFrequency; // effect actuation frequency in Hz (requires supplement modes 4 and 20)

                // (lower nibble: main motor; upper nibble trigger effects) 0x00 to 0x07 - reduce overall power of the respective motors/effects by 12.5% per increment (this does not affect the regular trigger motor settings, just the automatically repeating trigger effects)
                outputReport[37] = hapticsIntensityByte;
                // Volume of internal speaker (0-7; ties in with index 6. The PS5 default appears to be set a 4)
                //outputReport[38] = 0x00;

                /* Player LED section */
                // 0x01 Enabled LED brightness (value in index 43)
                // 0x02 Uninterruptable blue LED pulse (action in index 42)
                outputReport[39] = 0x02;
                // 0x01 Slowly (2s?) fade to blue (scheduled to when the regular LED settings are active)
                // 0x02 Slowly (2s?) fade out (scheduled after fade-in completion) with eventual switch back to configured LED color; only a fade-out can cancel the pulse (neither index 2, 0x08, nor turning this off will cancel it!)
                outputReport[42] = 0x02;
                // 0x00 High Brightness, 0x01 Medium Brightness, 0x02 Low Brightness
                outputReport[43] = 0x02;
                // 5 player LED lights below Touchpad.
                // Bitmask 0x00-0x1F from left to right with 0x04 being the center LED. Bit 0x20 sets the brightness immediately with no fade in
                outputReport[44] = activePlayerLEDMask;

                /* Lightbar colors */
                outputReport[45] = currentHap.lightbarState.LightBarColor.red;
                outputReport[46] = currentHap.lightbarState.LightBarColor.green;
                outputReport[47] = currentHap.lightbarState.LightBarColor.blue;

                if (!previousHapticState.Equals(currentHap))
                {
                    change = true;
                }
                /*fixed (byte* bytePrevBuff = outputReport, byteTmpBuff = outReportBuffer)
                {
                    for (int i = 0, arlen = USB_OUTPUT_CHANGE_LENGTH; !change && i < arlen; i++)
                        change = bytePrevBuff[i] != byteTmpBuff[i];
                }
                */

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
            else
            {
                //outReportBuffer[0] = OUTPUT_REPORT_ID_BT; // Report ID
                outputReport[0] = OUTPUT_REPORT_ID_BT; // Report ID
                outputReport[1] = OUTPUT_REPORT_ID_DATA;

                // 0x01 Set the main motors (also requires flag 0x02)
                // 0x02 Set the main motors (also requires flag 0x01)
                // 0x04 Set the right trigger motor
                // 0x08 Set the left trigger motor
                // 0x10 Enable modification of audio volume
                // 0x20 Enable internal speaker (even while headset is connected)
                // 0x40 Enable modification of microphone volume
                // 0x80 Enable internal mic (even while headset is connected)
                outputReport[2] = useRumble ? (byte)0x0F : (byte)0x0C; // 0x02 | 0x01 | 0x04 | 0x08;

                // 0x01 Toggling microphone LED, 0x02 Toggling Audio/Mic Mute
                // 0x04 Toggling LED strips on the sides of the Touchpad, 0x08 Turn off all LED lights
                // 0x10 Toggle player LED lights below Touchpad, 0x20 ???
                // 0x40 Adjust overall motor/effect power, 0x80 ???
                outputReport[3] = 0x55; // 0x04 | 0x01 | 0x10 | 0x40

                if (useRumble)
                {
                    // Right? High Freq Motor
                    outputReport[4] = currentHap.rumbleState.RumbleMotorStrengthRightLightFast;
                    // Left? Low Freq Motor
                    outputReport[5] = currentHap.rumbleState.RumbleMotorStrengthLeftHeavySlow;
                }

                /*
                // Headphone volume
                outputReport[6] = 0x00;
                outputReport[6] = Convert.ToByte(audio.getVolume()); // Left and Right
                // Internal speaker volume
                outputReport[7] = 0x00;
                // Internal microphone volume
                outputReport[8] = 0x00;
                outputReport[8] = Convert.ToByte(micAudio.getVolume());
                // 0x01 Enable internal microphone, 0x10 Disable attached headphones (must set 0x20 as well)
                // 0x20 Enable internal speaker
                outputReport[9] = 0x00;
                //*/

                // Mute button LED. 0x01 = Solid. 0x02 = Pulsating
                outputReport[10] = muteLEDByte;

                // audio settings requiring mute toggling flags
                //outputReport[11] = 0x00; // 0x10 microphone mute, 0x40 audio mute

                /* TRIGGER MOTORS  */
                // R2 Effects
                outputReport[12] = r2EffectData.triggerMotorMode; // right trigger motor mode (0 = no resistance, 1 = continuous resistance, 2 = section resistance, 0x20 and 0x04 enable additional effects together with 1 and 2 (configuration yet unknown), 252 = likely a calibration program* / PS Remote Play defaults this to 5; bit 4 only disables the motor?)
                outputReport[13] = r2EffectData.triggerStartResistance; // right trigger start of resistance section 0-255 (0 = released state; 0xb0 roughly matches trigger value 0xff); in mode 26 this field has something to do with motor re-extension after a press-release-cycle (0 = no re-extension)
                outputReport[14] = r2EffectData.triggerEffectForce; // right trigger
                                                                    // (mode1) amount of force exerted; 0-255
                                                                    // (mode2) end of resistance section (>= begin of resistance section is enforced); 0xff makes it behave like mode1
                                                                    // (supplemental mode 4+20) flag(s?) 0x02 = do not pause effect when fully pressed
                outputReport[15] = r2EffectData.triggerRangeForce; // right trigger force exerted in range (mode2), 0-255
                outputReport[16] = r2EffectData.triggerNearReleaseStrength; // strength of effect near release state (requires supplement modes 4 and 20)
                outputReport[17] = r2EffectData.triggerNearMiddleStrength; // strength of effect near middle (requires supplement modes 4 and 20)
                outputReport[18] = r2EffectData.triggerPressedStrength; // strength of effect at pressed state (requires supplement modes 4 and 20)
                outputReport[21] = r2EffectData.triggerActuationFrequency; // effect actuation frequency in Hz (requires supplement modes 4 and 20)


                // L2 Effects
                outputReport[23] = l2EffectData.triggerMotorMode; // left trigger motor mode (0 = no resistance, 1 = continuous resistance, 2 = section resistance, 0x20 and 0x04 enable additional effects together with 1 and 2 (configuration yet unknown), 252 = likely a calibration program* / PS Remote Play defaults this to 5; bit 4 only disables the motor?)
                outputReport[24] = l2EffectData.triggerStartResistance; // left trigger start of resistance section 0-255 (0 = released state; 0xb0 roughly matches trigger value 0xff); in mode 26 this field has something to do with motor re-extension after a press-release-cycle (0 = no re-extension)
                outputReport[25] = l2EffectData.triggerEffectForce; // left trigger
                                                                    // (mode1) amount of force exerted; 0-255
                                                                    // (mode2) end of resistance section (>= begin of resistance section is enforced); 0xff makes it behave like mode1
                                                                    // (supplemental mode 4+20) flag(s?) 0x02 = do not pause effect when fully pressed
                outputReport[26] = l2EffectData.triggerRangeForce; // left trigger: (mode2) amount of force exerted within range; 0-255
                outputReport[27] = l2EffectData.triggerNearReleaseStrength; // strength of effect near release state (requires supplement modes 4 and 20)
                outputReport[28] = l2EffectData.triggerNearMiddleStrength; // strength of effect near middle (requires supplement modes 4 and 20)
                outputReport[29] = l2EffectData.triggerPressedStrength; // strength of effect at pressed state (requires supplement modes 4 and 20)
                outputReport[32] = l2EffectData.triggerActuationFrequency; // effect actuation frequency in Hz (requires supplement modes 4 and 20)

                // (lower nibble: main motor; upper nibble trigger effects) 0x00 to 0x07 - reduce overall power of the respective motors/effects by 12.5% per increment (this does not affect the regular trigger motor settings, just the automatically repeating trigger effects)
                outputReport[38] = hapticsIntensityByte;
                // Volume of internal speaker (0-7; ties in with index 6. The PS5 default appears to be set a 4)
                //outputReport[39] = 0x00;

                /* Player LED section */
                // 0x01 Enabled LED brightness (value in index 43)
                // 0x02 Uninterruptable blue LED pulse (action in index 42)
                outputReport[40] = 0x02;
                // 0x01 Slowly (2s?) fade to blue (scheduled to when the regular LED settings are active)
                // 0x02 Slowly (2s?) fade out (scheduled after fade-in completion) with eventual switch back to configured LED color; only a fade-out can cancel the pulse (neither index 2, 0x08, nor turning this off will cancel it!)
                outputReport[43] = 0x02;
                // 0x00 High Brightness, 0x01 Medium Brightness, 0x02 Low Brightness
                outputReport[44] = 0x02;
                // 5 player LED lights below Touchpad.
                // Bitmask 0x00-0x1F from left to right with 0x04 being the center LED. Bit 0x20 sets the brightness immediately with no fade in
                outputReport[45] = activePlayerLEDMask;

                /* Lightbar colors */
                outputReport[46] = currentHap.lightbarState.LightBarColor.red;
                outputReport[47] = currentHap.lightbarState.LightBarColor.green;
                outputReport[48] = currentHap.lightbarState.LightBarColor.blue;

                change = !previousHapticState.Equals(currentHap);

                // Need to calculate and populate CRC32 data so controller will accept the report
                uint calcCrc32 = 0;
                if (change)
                //if (outputPendCount >= 1 || change)
                //if (!previousHapticState.Equals(currentHap))
                {
                    //change = true;
                    outputDirty = true;

                    if (rumbleSet)
                    {
                        standbySw.Restart();
                    }
                    else
                    {
                        standbySw.Reset();
                    }
                }
                else if (rumbleSet && standbySw.ElapsedMilliseconds >= 4000L)
                {
                    outputDirty = true;
                    standbySw.Restart();
                }

                if (outputDirty)
                {
                    int crcOffset = 0;
                    int crcpos = BT_OUTPUT_REPORT_LENGTH - 4;
                    calcCrc32 = ~Crc32Algorithm.Compute(outputBTCrc32Head);
                    //calcCrc32 = ~Crc32Algorithm.CalculateBasicHash(ref calcCrc32, ref outputReport, 0, BT_OUTPUT_REPORT_LENGTH-4);
                    calcCrc32 = ~Crc32Algorithm.CalculateFasterBT78Hash(ref calcCrc32, ref outputReport, ref crcOffset, ref crcpos);
                }

                outputReport[74] = (byte)calcCrc32;
                outputReport[75] = (byte)(calcCrc32 >> 8);
                outputReport[76] = (byte)(calcCrc32 >> 16);
                outputReport[77] = (byte)(calcCrc32 >> 24);

                /*fixed (byte* bytePrevBuff = outputReport, byteTmpBuff = outReportBuffer)
                {
                    for (int i = 0, arlen = BT_OUTPUT_CHANGE_LENGTH; !change && i < arlen; i++)
                        change = bytePrevBuff[i] != byteTmpBuff[i];
                }
                */

                /*if (change)
                {
                    outputPendCount = OUTPUT_MIN_COUNT_BT;
                    //Console.WriteLine("DIRTY");
                    outputDirty = true;
                    
                    //outReportBuffer.CopyTo(outputReport, 0);
                }
                else if (outputPendCount >= 1)
                {
                    Console.WriteLine("CURRENT: {0}", outputPendCount);
                    outputPendCount--;
                    outputDirty = outputPendCount >= 1;
                }
                */

                //outputDirty = true;

                //bool res = hDevice.WriteOutputReportViaControl(outputReport);
                //Console.WriteLine("STAUTS: {0}", res);
            }
        }

        private bool WriteReport()
        {
            bool result;
            if (conType == ConnectionType.BT)
            {
                // DualSense seems to only accept output data via the Interrupt endpoint
                result = hDevice.WriteOutputReportViaInterrupt(outputReport, READ_STREAM_TIMEOUT);
                //result = hDevice.WriteOutputReportViaControl(outputReport);
            }
            else
            {
                result = hDevice.WriteOutputReportViaInterrupt(outputReport, READ_STREAM_TIMEOUT);
            }

            //Console.WriteLine("STAUTS: {0}", result);
            return result;
        }

        private void Detach()
        {
            SendEmptyOutputReport();
        }

        private void CalculateDeviceSlotMask()
        {
            // Map 1-8 to a symmetrical LED array from a set of
            // 5 LED lights
            switch (deviceSlotNumber)
            {
                case 0:
                    deviceSlotMask = 0x04;
                    break;
                case 1:
                    deviceSlotMask = 0x02 | 0x08;
                    break;
                case 2:
                    deviceSlotMask = 0x01 | 0x04 | 0x10;
                    break;
                case 3:
                    deviceSlotMask = 0x02 | 0x04 | 0x08;
                    break;
                case 4:
                    deviceSlotMask = 0x01 | 0x10;
                    break;
                case 5:
                    deviceSlotMask = 0x01 | 0x02 | 0x08 | 0x10;
                    break;
                case 6:
                    deviceSlotMask = 0x01 | 0x02 | 0x04 | 0x08 | 0x10;
                    break;
                case 7:
                default:
                    deviceSlotMask = 0x00;
                    break;
            }
        }

        private void PrepareMuteLEDByte()
        {
            if (nativeOptionsStore != null)
            {
                switch (nativeOptionsStore.MuteLedMode)
                {
                    case DualSenseControllerOptions.MuteLEDMode.Off:
                        muteLEDByte = 0x00;
                        break;
                    case DualSenseControllerOptions.MuteLEDMode.On:
                        muteLEDByte = 0x01;
                        break;
                    case DualSenseControllerOptions.MuteLEDMode.Pulse:
                        muteLEDByte = 0x02;
                        break;
                    default:
                        muteLEDByte = 0x00;
                        break;
                }
            }
        }

        private void PreparePlayerLEDBarByte()
        {
            if (nativeOptionsStore != null)
            {
                if (nativeOptionsStore.LedMode == DualSenseControllerOptions.LEDBarMode.Off)
                {
                    activePlayerLEDMask = 0x00;
                }
                else if (nativeOptionsStore.LedMode == DualSenseControllerOptions.LEDBarMode.On)
                {
                    activePlayerLEDMask = deviceSlotMask;
                }
                else if (nativeOptionsStore.LedMode == DualSenseControllerOptions.LEDBarMode.BatteryPercentage)
                {
                    activePlayerLEDMask = DeviceBatteryLinearMask(battery);
                }
            }
        }

        public override void PrepareTriggerEffect(TriggerId trigger, TriggerEffects effect)
        {
            if (trigger == TriggerId.LeftTrigger)
            {
                l2EffectData.ChangeData(effect);
            }
            else if (trigger == TriggerId.RightTrigger)
            {
                r2EffectData.ChangeData(effect);
            }
            else
            {
                throw new ArgumentOutOfRangeException("Invalid Trigger Id");
            }

            queueEvent(() =>
            {
                outputDirty = true;
                PrepareOutReport();
            });
        }

        private byte DeviceBatteryLinearMask(int deviceBattery)
        {
            byte batteryMask;
            if (deviceBattery >= 95)
                batteryMask = 0x01 | 0x02 | 0x08 | 0x10;
            else if (deviceBattery >= 70)
                batteryMask = 0x01 | 0x02 | 0x08;
            else if (deviceBattery >= 50)
                batteryMask = 0x01 | 0x02;
            else if (deviceBattery >= 20)
                batteryMask = 0x01;
            else if (deviceBattery >= 5)
                batteryMask = 0x01 | 0x02 | 0x04;
            else
                batteryMask = 0x00;

            return batteryMask;
        }

        public override void CheckControllerNumDeviceSettings(int numControllers)
        {
            if (nativeOptionsStore != null)
            {
                if (nativeOptionsStore.LedMode ==
                    DualSenseControllerOptions.LEDBarMode.MultipleControllers)
                {
                    if (numControllers > 1)
                    {
                        activePlayerLEDMask = deviceSlotMask;
                    }
                    else
                    {
                        activePlayerLEDMask = 0x00;
                    }
                }
            }

            queueEvent(() =>
            {
                outputDirty = true;
                //PrepareOutReport();
            });
        }

        private void SetupOptionsEvents()
        {
            if (nativeOptionsStore != null)
            {
                nativeOptionsStore.EnableRumbleChanged += (sender, e) =>
                {
                    UseRumble = nativeOptionsStore.EnableRumble;
                    queueEvent(() => { outputDirty = true; });
                };
                nativeOptionsStore.HapticIntensityChanged += (sender, e) =>
                {
                    HapticChoice = nativeOptionsStore.HapticIntensity;
                    queueEvent(() => { outputDirty = true; });
                };

                nativeOptionsStore.MuteLedModeChanged += (sender, e) =>
                {
                    PrepareMuteLEDByte();
                    queueEvent(() => { outputDirty = true; });
                };

                nativeOptionsStore.LedModeChanged += (sender, e) =>
                {
                    PreparePlayerLEDBarByte();
                    queueEvent(() => { outputDirty = true; });
                };
            }
        }

        public override void LoadStoreSettings()
        {
            if (nativeOptionsStore != null)
            {
                UseRumble = nativeOptionsStore.EnableRumble;
                HapticChoice = nativeOptionsStore.HapticIntensity;
                PrepareMuteLEDByte();
                PreparePlayerLEDBarByte();
            }
        }
    }
}
