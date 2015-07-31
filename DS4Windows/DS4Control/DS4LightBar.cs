using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using static System.Math;
using static DS4Windows.Global;
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
            { 224, 56}, // on 80% of the time at 80, etc.
            { 252, 28 } // on 90% of the time at 90
        };
        static double[] counters = new double[4] { 0, 0, 0, 0 };
        public static double[] fadetimer = new double[4] { 0, 0, 0, 0 };
        static bool[] fadedirection = new bool[4] { false, false, false, false };
        static DateTime oldnow = DateTime.UtcNow;
        public static bool[] forcelight = new bool[4] { false, false, false, false };
        public static DS4Color[] forcedColor = new DS4Color[4];
        public static byte[] forcedFlash = new byte[4];
        public static void updateLightBar(DS4Device device, int deviceNum, DS4State cState, DS4StateExposed eState, Mouse tp)
        {
            DS4Color color;
            if (!defualtLight && !forcelight[deviceNum])
            {
                if (ShiftColorOn[deviceNum] && ShiftModifier[deviceNum] > 0 && shiftMod(device, deviceNum, cState, eState, tp))
                {
                    color = ShiftColor[deviceNum];
                }
                else
                {
                    if (Rainbow[deviceNum] > 0)
                    {// Display rainbow
                        DateTime now = DateTime.UtcNow;
                        if (now >= oldnow + TimeSpan.FromMilliseconds(10)) //update by the millisecond that way it's a smooth transtion
                        {
                            oldnow = now;
                            if (device.Charging)
                                counters[deviceNum] -= 1.5 * 3 / Rainbow[deviceNum];
                            else
                                counters[deviceNum] += 1.5 * 3 / Rainbow[deviceNum];
                        }
                        if (counters[deviceNum] < 0)
                            counters[deviceNum] = 180000;
                        if (counters[deviceNum] > 180000)
                            counters[deviceNum] = 0;
                        if (LedAsBatteryIndicator[deviceNum])
                            color = HuetoRGB((float)counters[deviceNum] % 360, (byte)(2.55 * device.Battery));
                        else
                            color = HuetoRGB((float)counters[deviceNum] % 360, 255);

                    }
                    else if (LedAsBatteryIndicator[deviceNum])
                    {
                        //if (device.Charging == false || device.Battery >= 100) // when charged, don't show the charging animation
                        {
                            DS4Color fullColor = MainColor[deviceNum];
                            DS4Color lowColor =  LowColor[deviceNum];

                            color = getTransitionedColor(lowColor, fullColor, (uint)device.Battery);
                        }
                    }
                    else
                    {
                        color = MainColor[deviceNum];
                    }


                    if (device.Battery <= FlashAt[deviceNum] && !defualtLight && !device.Charging)
                    {
                        if (!(FlashColor[deviceNum].red == 0 &&
                            FlashColor[deviceNum].green == 0 &&
                            FlashColor[deviceNum].blue == 0))
                            color = FlashColor[deviceNum];
                        if (FlashType[deviceNum] == 1)
                        {
                            if (fadetimer[deviceNum] <= 0)
                                fadedirection[deviceNum] = true;
                            else if (fadetimer[deviceNum] >= 100)
                                fadedirection[deviceNum] = false;
                            if (fadedirection[deviceNum])
                                fadetimer[deviceNum] += 1;
                            else
                                fadetimer[deviceNum] -= 1;
                            color = getTransitionedColor(color, new DS4Color(0,0,0), fadetimer[deviceNum]);
                        }
                    }

                    if (IdleDisconnectTimeout[deviceNum] > 0 && LedAsBatteryIndicator[deviceNum] && (!device.Charging || device.Battery >= 100))
                    {//Fade lightbar by idle time
                        TimeSpan timeratio = new TimeSpan(DateTime.UtcNow.Ticks - device.lastActive.Ticks);
                        double botratio = timeratio.TotalMilliseconds;
                        double topratio = TimeSpan.FromSeconds(IdleDisconnectTimeout[deviceNum]).TotalMilliseconds;
                        double ratio = ((botratio / topratio) * 100);
                        if (ratio >= 50 && ratio <= 100)
                            color = getTransitionedColor(color, new DS4Color(0, 0, 0), (uint)((ratio - 50) * 2));
                        else if (ratio >= 100)
                            color = getTransitionedColor(color, new DS4Color(0, 0, 0), 100);
                    }
                    if (device.Charging && device.Battery < 100)
                        switch (ChargingType[deviceNum])
                        {
                            case 1:
                                if (fadetimer[deviceNum] <= 0)
                                    fadedirection[deviceNum] = true;
                                else if (fadetimer[deviceNum] >= 105)
                                    fadedirection[deviceNum] = false;
                                if (fadedirection[deviceNum])
                                    fadetimer[deviceNum] += .1;
                                else
                                    fadetimer[deviceNum] -= .1;
                                color = getTransitionedColor(color, new DS4Color(0, 0, 0), fadetimer[deviceNum]);
                                break;
                            case 2:
                                counters[deviceNum] += .167;
                                color = HuetoRGB((float)counters[deviceNum] % 360, 255);
                                break;
                            case 3:
                                color = ChargingColor[deviceNum];
                                break;
                            default:
                                break;
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
                if (device.ConnectionType == ConnectionType.BT)
                    color = new DS4Color(32, 64, 64);
                else
                    color = new DS4Color(0, 0, 0);
            }
            bool distanceprofile = (ProfilePath[deviceNum].ToLower().Contains("distance") || tempprofilename[deviceNum].ToLower().Contains("distance"));
            if (distanceprofile && !defualtLight)
            { //Thing I did for Distance
                float rumble = device.LeftHeavySlowRumble / 2.55f;
                byte max = Max(color.red, Max(color.green, color.blue));
                if (device.LeftHeavySlowRumble > 100)
                    color = getTransitionedColor(new DS4Color(max, max, 0), new DS4Color(255, 0, 0), rumble);
                else
                    color = getTransitionedColor(color, getTransitionedColor(new DS4Color(max, max, 0), new DS4Color(255, 0, 0), 39.6078f), device.LeftHeavySlowRumble);
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
                else if (device.Battery <= FlashAt[deviceNum] && FlashType[deviceNum] == 0 && !defualtLight && !device.Charging)
                {
                    int level = device.Battery / 10;
                    //if (level >= 10)
                    //level = 0; // all values of ~0% or >~100% are rendered the same
                    haptics.LightBarFlashDurationOn = BatteryIndicatorDurations[level, 0];
                    haptics.LightBarFlashDurationOff = BatteryIndicatorDurations[level, 1];
                }
                else if (distanceprofile && device.LeftHeavySlowRumble > 155) //also part of Distance
                {
                    haptics.LightBarFlashDurationOff = haptics.LightBarFlashDurationOn = (byte)((-device.LeftHeavySlowRumble + 265));
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
            if (device.LightBarOnDuration != haptics.LightBarFlashDurationOn && device.LightBarOnDuration != 1 && haptics.LightBarFlashDurationOn == 0)
                haptics.LightBarFlashDurationOff = haptics.LightBarFlashDurationOn = 1;
            if (device.LightBarOnDuration == 1) //helps better reset the color
                System.Threading.Thread.Sleep(5);
            device.pushHapticState(haptics);
        }

        public static bool defualtLight = false, shuttingdown = false;

        public static bool shiftMod(DS4Device device, int deviceNum, DS4State cState, DS4StateExposed eState, Mouse tp)
        {
            bool shift;
            switch (ShiftModifier[deviceNum])
            {
                case 1: shift = Mapping.getBoolMapping(DS4Controls.Cross, cState, eState, tp); break;
                case 2: shift = Mapping.getBoolMapping(DS4Controls.Circle, cState, eState, tp); break;
                case 3: shift = Mapping.getBoolMapping(DS4Controls.Square, cState, eState, tp); break;
                case 4: shift = Mapping.getBoolMapping(DS4Controls.Triangle, cState, eState, tp); break;
                case 5: shift = Mapping.getBoolMapping(DS4Controls.Options, cState, eState, tp); break;
                case 6: shift = Mapping.getBoolMapping(DS4Controls.Share, cState, eState, tp); break;
                case 7: shift = Mapping.getBoolMapping(DS4Controls.DpadUp, cState, eState, tp); break;
                case 8: shift = Mapping.getBoolMapping(DS4Controls.DpadDown, cState, eState, tp); break;
                case 9: shift = Mapping.getBoolMapping(DS4Controls.DpadLeft, cState, eState, tp); break;
                case 10: shift = Mapping.getBoolMapping(DS4Controls.DpadRight, cState, eState, tp); break;
                case 11: shift = Mapping.getBoolMapping(DS4Controls.PS, cState, eState, tp); break;
                case 12: shift = Mapping.getBoolMapping(DS4Controls.L1, cState, eState, tp); break;
                case 13: shift = Mapping.getBoolMapping(DS4Controls.R1, cState, eState, tp); break;
                case 14: shift = Mapping.getBoolMapping(DS4Controls.L2, cState, eState, tp); break;
                case 15: shift = Mapping.getBoolMapping(DS4Controls.R2, cState, eState, tp); break;
                case 16: shift = Mapping.getBoolMapping(DS4Controls.L3, cState, eState, tp); break;
                case 17: shift = Mapping.getBoolMapping(DS4Controls.R3, cState, eState, tp); break;
                case 18: shift = Mapping.getBoolMapping(DS4Controls.TouchLeft, cState, eState, tp); break;
                case 19: shift = Mapping.getBoolMapping(DS4Controls.TouchUpper, cState, eState, tp); break;
                case 20: shift = Mapping.getBoolMapping(DS4Controls.TouchMulti, cState, eState, tp); break;
                case 21: shift = Mapping.getBoolMapping(DS4Controls.TouchRight, cState, eState, tp); break;
                case 22: shift = Mapping.getBoolMapping(DS4Controls.GyroZNeg, cState, eState, tp); break;
                case 23: shift = Mapping.getBoolMapping(DS4Controls.GyroZPos, cState, eState, tp); break;
                case 24: shift = Mapping.getBoolMapping(DS4Controls.GyroXPos, cState, eState, tp); break;
                case 25: shift = Mapping.getBoolMapping(DS4Controls.GyroXNeg, cState, eState, tp); break;
                case 26: shift = device.getCurrentState().Touch1; break;
                default: shift = false; break;
            }
            return shift;
        }
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
