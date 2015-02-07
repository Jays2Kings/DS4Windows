using DS4Control.XML_FIles;
using DS4Library;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace DS4Windows.XML_Files
{
    [Serializable]
    public class ProfileXML
    {
        public bool flushHIDQueue { get; set; }
        public int idleDisconnectTimeout { get; set; }
        public DS4Color color { get; set; }
        public int RumbleBoost { get; set; }
        public bool ledAsBatteryIndicator { get; set; }
        public bool lowBatteryFlash { get; set; }
        public int flashBatteryAt { get; set; }
        public int touchSensitivity { get; set; }
        public DS4Color LowColor { get; set; }
        public DS4Color ChargingColor { get; set; }
        public DS4Color ShiftColor { get; set; }
        public bool ShiftColorOn { get; set; }
        public DS4Color FlashColor { get; set; }
        public bool touchpadJitterCompensation { get; set; }
        public bool lowerRCOn { get; set; }
        public int tapSensitivity { get; set; }
        public bool doubleTap { get; set; }
        public int scrollSensitivity { get; set; }
        public int LeftTriggerMiddle { get; set; }
        public int RightTriggerMiddle { get; set; }
        public int ButtonMouseSensitivity { get; set; }
        public double Rainbow { get; set; }
        public double LSDeadZone { get; set; }
        public double RSDeadZone { get; set; }
        public double SXDeadZone { get; set; }
        public double SZDeadZone { get; set; }
        public double ChargingType { get; set; }
        public bool MouseAcceleration { get; set; }
        public double ShiftModifier { get; set; }
        public bool DinputOnly { get; set; }
        public bool StartTouchpadOff { get; set; }
        public bool UseTPforControls { get; set; }
        public double LSCurve { get; set; }
        public double RSCurve { get; set; }
        public List<string> ProfileActions { get; set; }

        public string LaunchProgram { get; set; }

        public List<ControlXML> Controls;
    }
}
