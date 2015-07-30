using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace EAll4Windows
{
    public class EAll4LightBar
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
        public static EAll4Color[] forcedColor = new EAll4Color[4];
        public static byte[] forcedFlash = new byte[4];
        public static void updateLightBar(EAll4Device device, int deviceNum, ControllerState cState, EAll4StateExposed eState, Mouse tp)
        {
            EAll4Color color;
            if (!defualtLight && !forcelight[deviceNum])
            {
                if (Global.ShiftColorOn[deviceNum] && Global.ShiftModifier[deviceNum] > 0 && shiftMod(device, deviceNum, cState, eState, tp))
                {
                    color = Global.ShiftColor[deviceNum];
                }
                else
                {
                    if (Global.Rainbow[deviceNum] > 0)
                    {// Display rainbow
                        DateTime now = DateTime.UtcNow;
                        if (now >= oldnow + TimeSpan.FromMilliseconds(10)) //update by the millisecond that way it's a smooth transtion
                        {
                            oldnow = now;
                            if (device.Charging)
                                counters[deviceNum] -= 1.5 * 3 / Global.Rainbow[deviceNum];
                            else
                                counters[deviceNum] += 1.5 * 3 / Global.Rainbow[deviceNum];
                        }
                        if (counters[deviceNum] < 0)
                            counters[deviceNum] = 180000;
                        if (counters[deviceNum] > 180000)
                            counters[deviceNum] = 0;
                        if (Global.LedAsBatteryIndicator[deviceNum])
                            color = HuetoRGB((float)counters[deviceNum] % 360, (byte)(2.55 * device.Battery));
                        else
                            color = HuetoRGB((float)counters[deviceNum] % 360, 255);

                    }
                    else if (Global.LedAsBatteryIndicator[deviceNum])
                    {
                        //if (device.Charging == false || device.Battery >= 100) // when charged, don't show the charging animation
                        {
                            EAll4Color fullColor = Global.MainColor[deviceNum];
                            EAll4Color lowColor = Global.LowColor[deviceNum];

                            color = Global.getTransitionedColor(lowColor, fullColor, (uint)device.Battery);
                        }
                    }
                    else
                    {
                        color = Global.MainColor[deviceNum];
                    }


                    if (device.Battery <= Global.FlashAt[deviceNum] && !defualtLight && !device.Charging)
                    {
                        if (!(Global.FlashColor[deviceNum].Red == 0 &&
                            Global.FlashColor[deviceNum].Green == 0 &&
                            Global.FlashColor[deviceNum].Blue == 0))
                            color = Global.FlashColor[deviceNum];
                        if (Global.FlashType[deviceNum] == 1)
                        {
                            if (fadetimer[deviceNum] <= 0)
                                fadedirection[deviceNum] = true;
                            else if (fadetimer[deviceNum] >= 100)
                                fadedirection[deviceNum] = false;
                            if (fadedirection[deviceNum])
                                fadetimer[deviceNum] += 1;
                            else
                                fadetimer[deviceNum] -= 1;
                            color = Global.getTransitionedColor(color, new EAll4Color(0, 0, 0), fadetimer[deviceNum]);
                        }
                    }

                    if (Global.IdleDisconnectTimeout[deviceNum] > 0 && Global.LedAsBatteryIndicator[deviceNum] && (!device.Charging || device.Battery >= 100))
                    {//Fade lightbar by idle time
                        TimeSpan timeratio = new TimeSpan(DateTime.UtcNow.Ticks - device.lastActive.Ticks);
                        double botratio = timeratio.TotalMilliseconds;
                        double topratio = TimeSpan.FromSeconds(Global.IdleDisconnectTimeout[deviceNum]).TotalMilliseconds;
                        double ratio = ((botratio / topratio) * 100);
                        if (ratio >= 50 && ratio <= 100)
                            color = Global.getTransitionedColor(color, new EAll4Color(0, 0, 0), (uint)((ratio - 50) * 2));
                        else if (ratio >= 100)
                            color = Global.getTransitionedColor(color, new EAll4Color(0, 0, 0), 100);
                    }
                    if (device.Charging && device.Battery < 100)
                        switch (Global.ChargingType[deviceNum])
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
                                color = Global.getTransitionedColor(color, new EAll4Color(0, 0, 0), fadetimer[deviceNum]);
                                break;
                            case 2:
                                counters[deviceNum] += .167;
                                color = HuetoRGB((float)counters[deviceNum] % 360, 255);
                                break;
                            case 3:
                                color = Global.ChargingColor[deviceNum];
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
                color = new EAll4Color(0, 0, 0);
            else
            {
                if (device.ConnectionType == ConnectionType.BT)
                    color = new EAll4Color(32, 64, 64);
                else
                    color = new EAll4Color(0, 0, 0);
            }
            bool distanceprofile = (Global.ProfilePath[deviceNum].ToLower().Contains("distance") || Global.tempprofilename[deviceNum].ToLower().Contains("distance"));
            if (distanceprofile && !defualtLight)
            { //Thing I did for Distance
                float rumble = device.LeftHeavySlowRumble / 2.55f;
                byte max = Math.Max(color.Red, Math.Max(color.Green, color.Blue));
                if (device.LeftHeavySlowRumble > 100)
                    color = Global.getTransitionedColor(new EAll4Color(max, max, 0), new EAll4Color(255, 0, 0), rumble);
                else
                    color = Global.getTransitionedColor(color, Global.getTransitionedColor(new EAll4Color(max, max, 0), new EAll4Color(255, 0, 0), 39.6078f), device.LeftHeavySlowRumble);
            }
            EAll4HapticState haptics = new EAll4HapticState
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
                else if (device.Battery <= Global.FlashAt[deviceNum] && Global.FlashType[deviceNum] == 0 && !defualtLight && !device.Charging)
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

        public static bool shiftMod(EAll4Device device, int deviceNum, ControllerState cState, EAll4StateExposed eState, Mouse tp)
        {
            bool shift;
            switch (Global.ShiftModifier[deviceNum])
            {
                case 1: shift = Mapping.getBoolMapping(EAll4Controls.Cross, cState, eState, tp); break;
                case 2: shift = Mapping.getBoolMapping(EAll4Controls.Circle, cState, eState, tp); break;
                case 3: shift = Mapping.getBoolMapping(EAll4Controls.Square, cState, eState, tp); break;
                case 4: shift = Mapping.getBoolMapping(EAll4Controls.Triangle, cState, eState, tp); break;
                case 5: shift = Mapping.getBoolMapping(EAll4Controls.Options, cState, eState, tp); break;
                case 6: shift = Mapping.getBoolMapping(EAll4Controls.Share, cState, eState, tp); break;
                case 7: shift = Mapping.getBoolMapping(EAll4Controls.DpadUp, cState, eState, tp); break;
                case 8: shift = Mapping.getBoolMapping(EAll4Controls.DpadDown, cState, eState, tp); break;
                case 9: shift = Mapping.getBoolMapping(EAll4Controls.DpadLeft, cState, eState, tp); break;
                case 10: shift = Mapping.getBoolMapping(EAll4Controls.DpadRight, cState, eState, tp); break;
                case 11: shift = Mapping.getBoolMapping(EAll4Controls.PS, cState, eState, tp); break;
                case 12: shift = Mapping.getBoolMapping(EAll4Controls.L1, cState, eState, tp); break;
                case 13: shift = Mapping.getBoolMapping(EAll4Controls.R1, cState, eState, tp); break;
                case 14: shift = Mapping.getBoolMapping(EAll4Controls.L2, cState, eState, tp); break;
                case 15: shift = Mapping.getBoolMapping(EAll4Controls.R2, cState, eState, tp); break;
                case 16: shift = Mapping.getBoolMapping(EAll4Controls.L3, cState, eState, tp); break;
                case 17: shift = Mapping.getBoolMapping(EAll4Controls.R3, cState, eState, tp); break;
                case 18: shift = Mapping.getBoolMapping(EAll4Controls.TouchLeft, cState, eState, tp); break;
                case 19: shift = Mapping.getBoolMapping(EAll4Controls.TouchUpper, cState, eState, tp); break;
                case 20: shift = Mapping.getBoolMapping(EAll4Controls.TouchMulti, cState, eState, tp); break;
                case 21: shift = Mapping.getBoolMapping(EAll4Controls.TouchRight, cState, eState, tp); break;
                case 22: shift = Mapping.getBoolMapping(EAll4Controls.GyroZNeg, cState, eState, tp); break;
                case 23: shift = Mapping.getBoolMapping(EAll4Controls.GyroZPos, cState, eState, tp); break;
                case 24: shift = Mapping.getBoolMapping(EAll4Controls.GyroXPos, cState, eState, tp); break;
                case 25: shift = Mapping.getBoolMapping(EAll4Controls.GyroXNeg, cState, eState, tp); break;
                case 26: shift = device.getCurrentState().Touch1; break;
                default: shift = false; break;
            }
            return shift;
        }
        public static EAll4Color HuetoRGB(float hue, byte sat)
        {
            byte C = sat;
            int X = (int)((C * (float)(1 - Math.Abs((hue / 60) % 2 - 1))));
            if (0 <= hue && hue < 60)
                return new EAll4Color(C, (byte)X, 0);
            else if (60 <= hue && hue < 120)
                return new EAll4Color((byte)X, C, 0);
            else if (120 <= hue && hue < 180)
                return new EAll4Color(0, C, (byte)X);
            else if (180 <= hue && hue < 240)
                return new EAll4Color(0, (byte)X, C);
            else if (240 <= hue && hue < 300)
                return new EAll4Color((byte)X, 0, C);
            else if (300 <= hue && hue < 360)
                return new EAll4Color(C, 0, (byte)X);
            else
                return new EAll4Color(Color.Red);
        }
    }
}
