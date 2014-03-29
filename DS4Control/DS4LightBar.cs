using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DS4Library;
namespace DS4Control
{
    class DS4LightBar
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
        static double[] counters = new double[4];

        public static void updateLightBar(DS4Device device, int deviceNum)
        {
            DS4Color color;
            if (Global.getLedAsBatteryIndicator(deviceNum))
            {
                if (device.Charging == false || device.Battery >= 100) // when charged, don't show the charging animation
                {
                    DS4Color fullColor = new DS4Color
                    {
                        red = Global.loadColor(deviceNum).red,
                        green = Global.loadColor(deviceNum).green,
                        blue = Global.loadColor(deviceNum).blue
                    };

                    color = Global.loadLowColor(deviceNum);
                    DS4Color lowColor = new DS4Color
                    {
                        red = color.red,
                        green = color.green,
                        blue = color.blue
                    };

                    color = Global.getTransitionedColor(lowColor, fullColor, (uint)device.Battery);
                }
                else // Display rainbow when charging.
                {
                    counters[deviceNum]++;
                    double theta = Math.PI * 2.0 * counters[deviceNum] / 1800.0;
                    const double brightness = Math.PI; // small brightness numbers (far from max 128.0) mean less light steps and slower output reports; also, the lower the brightness the faster you can charge
                    color = new DS4Color
                    {
                        red = (byte)(brightness * Math.Sin(theta) + brightness - 0.5),
                        green = (byte)(brightness * Math.Sin(theta + (Math.PI * 2.0) / 3.0) + brightness - 0.5),
                        blue = (byte)(brightness * Math.Sin(theta + 2.0 * (Math.PI * 2.0) / 3.0) + brightness - 0.5)
                    };
                }
            }
            else
            {
                color = Global.loadColor(deviceNum);
            }

            DS4HapticState haptics = new DS4HapticState
            {
                LightBarColor = color
            };
            if (haptics.IsLightBarSet())
            {
                if (Global.getFlashWhenLowBattery(deviceNum))
                {
                    int level = device.Battery / 10;
                    if (level >= 10)
                        level = 0; // all values of ~0% or >~100% are rendered the same
                    haptics.LightBarFlashDurationOn = BatteryIndicatorDurations[level, 0];
                    haptics.LightBarFlashDurationOff = BatteryIndicatorDurations[level, 1];
                }
                else
                {
                    haptics.LightBarFlashDurationOff = haptics.LightBarFlashDurationOn = 0;
                }
            }
            else
            {
                haptics.LightBarExplicitlyOff = true;
            }
            device.pushHapticState(haptics);
        }

    }
}
