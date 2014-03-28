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
            { 255, 255 }, // 0 doesn't happen
            { 28, 252 },
            { 56, 224 },
            { 84, 196 },
            { 112, 168 },
            { 140, 140 },
            { 168, 112 },
            { 196, 84 },
            { 224, 56}, // on 80% of the time at 80, etc.
            { 252, 28 }, // on 90% of the time at 90
            { 0, 0 } // no flash at 100
        };

        public static void updateBatteryStatus(int battery, DS4Device device, int deviceNum)
        {
            if (Global.getLedAsBatteryIndicator(deviceNum))
            {
                byte[] fullColor = { 
                                   Global.loadColor(deviceNum).red, 
                                   Global.loadColor(deviceNum).green, 
                                   Global.loadColor(deviceNum).blue 
                               };

                // New Setting
                DS4Color color = Global.loadLowColor(deviceNum);
                byte[] lowColor = { color.red, color.green, color.blue };

                uint ratio = (uint)battery;
                color = Global.getTransitionedColor(lowColor, fullColor, ratio);
                device.LightBarColor = color;


            }
            else
            {
                DS4Color color = Global.loadColor(deviceNum);
                device.LightBarColor = color;
            }

            if (Global.getFlashWhenLowBattery(deviceNum))
            {
                device.LightBarOnDuration = BatteryIndicatorDurations[battery / 10, 0];
                device.LightBarOffDuration = BatteryIndicatorDurations[battery / 10, 1];
            }
            else
            {
                device.LightBarOffDuration = device.LightBarOnDuration = 0;
            }
        }

    }
}
