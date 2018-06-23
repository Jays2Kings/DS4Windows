using System;
using System.Drawing;
using static System.Math;
using static DS4Windows.Global;
using System.Diagnostics;

namespace DS4Windows
{
    public class DS4LightBar
    {
        private readonly static byte[/* Light On duration */, /* Light Off duration */] BatteryIndicatorDurations =
        {
            { 0, 0 }, // 0 is for "charging" OR anything sufficiently-"charged"
            { 28, 252 },
            { 56, 224 },
            { 84, 196 },
            { 112, 168 },
            { 140, 140 },
            { 168, 112 },
            { 196, 84 },
            { 224, 56 }, // on 80% of the time at 80, etc.
            { 252, 28 } // on 90% of the time at 90
        };

        static double[] counters = new double[4] { 0, 0, 0, 0 };
        public static Stopwatch[] fadewatches = new Stopwatch[4]
            { new Stopwatch(), new Stopwatch(), new Stopwatch(), new Stopwatch() };

        static bool[] fadedirection = new bool[4] { false, false, false, false };
        static DateTime[] oldnow = new DateTime[4]
            { DateTime.UtcNow, DateTime.UtcNow, DateTime.UtcNow, DateTime.UtcNow };

        public static bool[] forcelight = new bool[4] { false, false, false, false };
        public static DS4Color[] forcedColor = new DS4Color[4];
        public static byte[] forcedFlash = new byte[4];
        internal const int PULSE_FLASH_DURATION = 2000;
        internal const double PULSE_FLASH_SEGMENTS = PULSE_FLASH_DURATION / 40;
        internal const int PULSE_CHARGING_DURATION = 4000;
        internal const double PULSE_CHARGING_SEGMENTS = (PULSE_CHARGING_DURATION / 40) - 2;

        public static void updateLightBar(DS4Device device, int deviceNum)
        {
            DS4Color color;
            if (!defaultLight && !forcelight[deviceNum])
            {
                if (getUseCustomLed(deviceNum))
                {
                    if (getLedAsBatteryIndicator(deviceNum))
                    {
                        DS4Color fullColor = getCustomColor(deviceNum);
                        DS4Color lowColor = getLowColor(deviceNum);
                        color = getTransitionedColor(lowColor, fullColor, device.getBattery());
                    }
                    else
                        color = getCustomColor(deviceNum);
                }
                else
                {
                    double rainbow = getRainbow(deviceNum);
                    if (rainbow > 0)
                    {
                        // Display rainbow
                        DateTime now = DateTime.UtcNow;
                        if (now >= oldnow[deviceNum] + TimeSpan.FromMilliseconds(10)) //update by the millisecond that way it's a smooth transtion
                        {
                            oldnow[deviceNum] = now;
                            if (device.isCharging())
                                counters[deviceNum] -= 1.5 * 3 / rainbow;
                            else
                                counters[deviceNum] += 1.5 * 3 / rainbow;
                        }

                        if (counters[deviceNum] < 0)
                            counters[deviceNum] = 180000;
                        else if (counters[deviceNum] > 180000)
                            counters[deviceNum] = 0;

                        if (getLedAsBatteryIndicator(deviceNum))
                            color = HuetoRGB((float)counters[deviceNum] % 360, (byte)(device.getBattery() * 2.55));
                        else
                            color = HuetoRGB((float)counters[deviceNum] % 360, 255);

                    }
                    else if (getLedAsBatteryIndicator(deviceNum))
                    {
                        DS4Color fullColor = getMainColor(deviceNum);
                        DS4Color lowColor = getLowColor(deviceNum);
                        color = getTransitionedColor(lowColor, fullColor, device.getBattery());
                    }
                    else
                    {
                        color = getMainColor(deviceNum);
                    }
                }

                if (device.getBattery() <= getFlashAt(deviceNum) && !defaultLight && !device.isCharging())
                {
                    DS4Color flashColor = getFlashColor(deviceNum);
                    if (!(flashColor.red == 0 &&
                        flashColor.green == 0 &&
                        flashColor.blue == 0))
                        color = flashColor;

                    if (getFlashType(deviceNum) == 1)
                    {
                        double ratio = 0.0;

                        if (!fadewatches[deviceNum].IsRunning)
                        {
                            bool temp = fadedirection[deviceNum];
                            fadedirection[deviceNum] = !temp;
                            fadewatches[deviceNum].Restart();
                            ratio = temp ? 100.0 : 0.0;
                        }
                        else
                        {
                            long elapsed = fadewatches[deviceNum].ElapsedMilliseconds;

                            if (fadedirection[deviceNum])
                            {
                                if (elapsed < PULSE_FLASH_DURATION)
                                {
                                    elapsed = elapsed / 40;
                                    ratio = 100.0 * (elapsed / PULSE_FLASH_SEGMENTS);
                                }
                                else
                                {
                                    ratio = 100.0;
                                    fadewatches[deviceNum].Stop();
                                }
                            }
                            else
                            {
                                if (elapsed < PULSE_FLASH_DURATION)
                                {
                                    elapsed = elapsed / 40;
                                    ratio = (0 - 100.0) * (elapsed / PULSE_FLASH_SEGMENTS) + 100.0;
                                }
                                else
                                {
                                    ratio = 0.0;
                                    fadewatches[deviceNum].Stop();
                                }
                            }
                        }

                        color = getTransitionedColor(color, new DS4Color(0, 0, 0), ratio);
                    }
                }

                int idleDisconnectTimeout = getIdleDisconnectTimeout(deviceNum);
                if (idleDisconnectTimeout > 0 && getLedAsBatteryIndicator(deviceNum) &&
                    (!device.isCharging() || device.getBattery() >= 100))
                {
                    //Fade lightbar by idle time
                    TimeSpan timeratio = new TimeSpan(DateTime.UtcNow.Ticks - device.lastActive.Ticks);
                    double botratio = timeratio.TotalMilliseconds;
                    double topratio = TimeSpan.FromSeconds(idleDisconnectTimeout).TotalMilliseconds;
                    double ratio = 100.0 * (botratio / topratio), elapsed = ratio;
                    if (ratio >= 50.0 && ratio < 100.0)
                    {
                        color = getTransitionedColor(color, new DS4Color(0, 0, 0),
                            (uint)(-100.0 * (elapsed = 0.02 * (ratio - 50.0)) * (elapsed - 2.0)));
                    }
                    else if (ratio >= 100.0)
                        color = getTransitionedColor(color, new DS4Color(0, 0, 0), 100.0);
                }

                if (device.isCharging() && device.getBattery() < 100)
                {
                    switch (getChargingType(deviceNum))
                    {
                        case 1:
                        {
                            double ratio = 0.0;

                            if (!fadewatches[deviceNum].IsRunning)
                            {
                                bool temp = fadedirection[deviceNum];
                                fadedirection[deviceNum] = !temp;
                                fadewatches[deviceNum].Restart();
                                ratio = temp ? 100.0 : 0.0;
                            }
                            else
                            {
                                long elapsed = fadewatches[deviceNum].ElapsedMilliseconds;

                                if (fadedirection[deviceNum])
                                {
                                    if (elapsed < PULSE_CHARGING_DURATION)
                                    {
                                        elapsed = elapsed / 40;
                                        if (elapsed > PULSE_CHARGING_SEGMENTS)
                                            elapsed = (long)PULSE_CHARGING_SEGMENTS;
                                        ratio = 100.0 * (elapsed / PULSE_CHARGING_SEGMENTS);
                                    }
                                    else
                                    {
                                        ratio = 100.0;
                                        fadewatches[deviceNum].Stop();
                                    }
                                }
                                else
                                {
                                    if (elapsed < PULSE_CHARGING_DURATION)
                                    {
                                        elapsed = elapsed / 40;
                                        if (elapsed > PULSE_CHARGING_SEGMENTS)
                                            elapsed = (long)PULSE_CHARGING_SEGMENTS;
                                        ratio = (0 - 100.0) * (elapsed / PULSE_CHARGING_SEGMENTS) + 100.0;
                                    }
                                    else
                                    {
                                        ratio = 0.0;
                                        fadewatches[deviceNum].Stop();
                                    }
                                }
                            }

                            color = getTransitionedColor(color, new DS4Color(0, 0, 0), ratio);
                            break;
                        }
                        case 2:
                        {
                            counters[deviceNum] += 0.167;
                            color = HuetoRGB((float)counters[deviceNum] % 360, 255);
                            break;
                        }
                        case 3:
                        {
                            color = getChargingColor(deviceNum);
                            break;
                        }
                        default: break;
                    }
                }
            }
            else if (forcelight[deviceNum])
            {
                color = forcedColor[deviceNum];
            }
            else if (shuttingdown)
                color = new DS4Color(0, 0, 0);
            else
            {
                if (device.getConnectionType() == ConnectionType.BT)
                    color = new DS4Color(32, 64, 64);
                else
                    color = new DS4Color(0, 0, 0);
            }

            bool distanceprofile = DistanceProfiles[deviceNum] || tempprofileDistance[deviceNum];
            //distanceprofile = (ProfilePath[deviceNum].ToLower().Contains("distance") || tempprofilename[deviceNum].ToLower().Contains("distance"));
            if (distanceprofile && !defaultLight)
            {
                // Thing I did for Distance
                float rumble = device.getLeftHeavySlowRumble() / 2.55f;
                byte max = Max(color.red, Max(color.green, color.blue));
                if (device.getLeftHeavySlowRumble() > 100)
                    color = getTransitionedColor(new DS4Color(max, max, 0), new DS4Color(255, 0, 0), rumble);
                else
                    color = getTransitionedColor(color, getTransitionedColor(new DS4Color(max, max, 0), new DS4Color(255, 0, 0), 39.6078f), device.getLeftHeavySlowRumble());
            }

            DS4HapticState haptics = new DS4HapticState
            {
                LightBarColor = color
            };

            if (haptics.IsLightBarSet())
            {
                if (forcelight[deviceNum] && forcedFlash[deviceNum] > 0)
                {
                    haptics.LightBarFlashDurationOff = haptics.LightBarFlashDurationOn = (byte)(25 - forcedFlash[deviceNum]);
                    haptics.LightBarExplicitlyOff = true;
                }
                else if (device.getBattery() <= getFlashAt(deviceNum) && getFlashType(deviceNum) == 0 && !defaultLight && !device.isCharging())
                {
                    int level = device.getBattery() / 10;
                    if (level >= 10)
                        level = 0; // all values of ~0% or >~100% are rendered the same

                    haptics.LightBarFlashDurationOn = BatteryIndicatorDurations[level, 0];
                    haptics.LightBarFlashDurationOff = BatteryIndicatorDurations[level, 1];
                }
                else if (distanceprofile && device.getLeftHeavySlowRumble() > 155) //also part of Distance
                {
                    haptics.LightBarFlashDurationOff = haptics.LightBarFlashDurationOn = (byte)((-device.getLeftHeavySlowRumble() + 265));
                    haptics.LightBarExplicitlyOff = true;
                }
                else
                {
                    //haptics.LightBarFlashDurationOff = haptics.LightBarFlashDurationOn = 1;
                    haptics.LightBarFlashDurationOff = haptics.LightBarFlashDurationOn = 0;
                    haptics.LightBarExplicitlyOff = true;
                }
            }
            else
            {
                haptics.LightBarExplicitlyOff = true;
            }

            byte tempLightBarOnDuration = device.getLightBarOnDuration();
            if (tempLightBarOnDuration != haptics.LightBarFlashDurationOn && tempLightBarOnDuration != 1 && haptics.LightBarFlashDurationOn == 0)
                haptics.LightBarFlashDurationOff = haptics.LightBarFlashDurationOn = 1;

            device.pushHapticState(ref haptics);
        }

        public static bool defaultLight = false, shuttingdown = false;
      
        public static DS4Color HuetoRGB(float hue, byte sat)
        {
            byte C = sat;
            int X = (int)((C * (float)(1 - Math.Abs((hue / 60) % 2 - 1))));
            if (0 <= hue && hue < 60)
                return new DS4Color(C, (byte)X, 0);
            else if (60 <= hue && hue < 120)
                return new DS4Color((byte)X, C, 0);
            else if (120 <= hue && hue < 180)
                return new DS4Color(0, C, (byte)X);
            else if (180 <= hue && hue < 240)
                return new DS4Color(0, (byte)X, C);
            else if (240 <= hue && hue < 300)
                return new DS4Color((byte)X, 0, C);
            else if (300 <= hue && hue < 360)
                return new DS4Color(C, 0, (byte)X);
            else
                return new DS4Color(Color.Red);
        }
    }
}
