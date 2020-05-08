using System;

namespace DS4Windows
{
    public class SquareStickInfo
    {
        public bool lsMode;
        public bool rsMode;
        public double lsRoundness = 5.0;
        public double rsRoundness = 5.0;
    }

    public class StickDeadZoneInfo
    {
        public int deadZone;
        public int antiDeadZone;
        public int maxZone = 100;
        public double maxOutput = 100.0;
    }

    public class TriggerDeadZoneZInfo
    {
        public byte deadZone; // Trigger deadzone is expressed in axis units
        public int antiDeadZone;
        public int maxZone = 100;
        public double maxOutput = 100.0;
    }

    public class GyroMouseInfo
    {

    }

    public class GyroMouseStickInfo
    {
        public int deadZone;
        public int maxZone;
        public double antiDeadX;
        public double antiDeadY;
        public int vertScale;
        // Flags representing invert axis choices
        public uint inverted;
        public bool useSmoothing;
        public double smoothWeight;
    }

    public class ButtonMouseInfo
    {
        //public const double MOUSESTICKANTIOFFSET = 0.0128;
        public const double MOUSESTICKANTIOFFSET = 0.005;

        public int buttonSensitivity = 25;
        public bool mouseAccel;
        public int activeButtonSensitivity = 25;
        public int tempButtonSensitivity = -1;
        public double mouseVelocityOffset = MOUSESTICKANTIOFFSET;

        public void SetActiveButtonSensitivity(int sens)
        {
            activeButtonSensitivity = sens;
        }
    }

    public enum LightbarMode : uint
    {
        None,
        DS4Win,
        Passthru,
    }

    public class LightbarDS4WinInfo
    {
        public bool useCustomLed;
        public bool ledAsBattery;
        public DS4Color m_CustomLed = new DS4Color(0, 0, 255);
        public DS4Color m_Led;
        public DS4Color m_LowLed;
        public DS4Color m_ChargingLed;
        public DS4Color m_FlashLed;
        public double rainbow;
        public double maxRainbowSat = 1.0;
        public int flashAt; // Battery % when flashing occurs. <0 means disabled
        public byte flashType;
        public int chargingType;
    }

    public class LightbarSettingInfo
    {
        public LightbarMode mode = LightbarMode.DS4Win;
        public LightbarDS4WinInfo ds4winSettings = new LightbarDS4WinInfo();
        public LightbarMode Mode
        {
            get => mode;
            set
            {
                if (mode == value) return;
                mode = value;
                ChangedMode?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler ChangedMode;

        public LightbarSettingInfo()
        {
            /*ChangedMode += (sender, e) =>
            {
                if (mode != LightbarMode.DS4Win)
                {
                    ds4winSettings = null;
                }
            };
            */
        }
    }
}