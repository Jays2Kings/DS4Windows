using System;
using Sensorit.Base;

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
        public const int DEFAULT_MAXZONE = 100;
        public const double DEFAULT_MAXOUTPUT = 100.0;
        public const int DEFAULT_FUZZ = 0;

        public int deadZone;
        public int antiDeadZone;
        public int maxZone = 100;
        public double maxOutput = 100.0;
        public int fuzz = DEFAULT_FUZZ;
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
        public enum SmoothingMethod : byte
        {
            None,
            OneEuro,
            WeightedAverage,
        }

        public const double DEFAULT_MINCUTOFF = 1.0;
        public const double DEFAULT_BETA = 0.7;
        public const string DEFAULT_SMOOTH_TECHNIQUE = "one-euro";

        public double minCutoff = DEFAULT_MINCUTOFF;
        public double beta = DEFAULT_BETA;
        public bool useWeightedAverageSmooth = false;
        public bool useOneEuroSmooth = false;
        public bool enableSmoothing = false;
        public double smoothingWeight = 0.5;

        public delegate void GyroMouseInfoEventHandler(GyroMouseInfo sender, EventArgs args);

        public double MinCutoff
        {
            get => minCutoff;
            set
            {
                if (minCutoff == value) return;
                minCutoff = value;
                MinCutoffChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event GyroMouseInfoEventHandler MinCutoffChanged;

        public double Beta
        {
            get => beta;
            set
            {
                if (beta == value) return;
                beta = value;
                BetaChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event GyroMouseInfoEventHandler BetaChanged;

        public void Reset()
        {
            minCutoff = DEFAULT_MINCUTOFF;
            beta = DEFAULT_BETA;
            useWeightedAverageSmooth = false;
            useOneEuroSmooth = false;
            enableSmoothing = false;
            smoothingWeight = 0.5;
        }

        public void ResetSmoothing()
        {
            enableSmoothing = false;
            ResetSmoothingMethods();
        }

        public void ResetSmoothingMethods()
        {
            useOneEuroSmooth = false;
            useWeightedAverageSmooth = false;
        }

        public void DetermineSmoothMethod(string identier)
        {
            ResetSmoothingMethods();

            switch (identier)
            {
                case "weighted-average":
                    useWeightedAverageSmooth = true;
                    break;
                case "one-euro":
                    useOneEuroSmooth = true;
                    break;
                default:
                    useWeightedAverageSmooth = false;
                    break;
            }
        }

        public string SmoothMethodIdentifier()
        {
            string result = "weighted-average";
            if (useOneEuroSmooth)
            {
                result = "one-euro";
            }
            else if (useWeightedAverageSmooth)
            {
                result = "weighted-average";
            }

            return result;
        }

        public void SetRefreshEvents(OneEuroFilter euroFilter)
        {
            BetaChanged += (sender, args) =>
            {
                euroFilter.Beta = beta;
            };

            MinCutoffChanged += (sender, args) =>
            {
                euroFilter.MinCutoff = minCutoff;
            };
        }

        public void RemoveRefreshEvents()
        {
            BetaChanged = null;
            MinCutoffChanged = null;
        }
    }

    public class GyroMouseStickInfo
    {
        public enum SmoothingMethod : byte
        {
            None,
            OneEuro,
            WeightedAverage,
        }

        public const double DEFAULT_MINCUTOFF = 0.4;
        public const double DEFAULT_BETA = 0.7;
        public const string DEFAULT_SMOOTH_TECHNIQUE = "one-euro";

        public int deadZone;
        public int maxZone;
        public double antiDeadX;
        public double antiDeadY;
        public int vertScale;
        public bool maxOutputEnabled;
        public double maxOutput = 100.0;
        // Flags representing invert axis choices
        public uint inverted;
        public bool useSmoothing;
        public double smoothWeight;
        public SmoothingMethod smoothingMethod;
        public double minCutoff = DEFAULT_MINCUTOFF;
        public double beta = DEFAULT_BETA;

        public delegate void GyroMouseStickInfoEventHandler(GyroMouseStickInfo sender,
            EventArgs args);


        public double MinCutoff
        {
            get => minCutoff;
            set
            {
                if (minCutoff == value) return;
                minCutoff = value;
                MinCutoffChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event GyroMouseStickInfoEventHandler MinCutoffChanged;

        public double Beta
        {
            get => beta;
            set
            {
                if (beta == value) return;
                beta = value;
                BetaChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event GyroMouseStickInfoEventHandler BetaChanged;

        public void Reset()
        {
            deadZone = 30; maxZone = 830;
            antiDeadX = 0.4; antiDeadY = 0.4;
            inverted = 0; vertScale = 100;
            maxOutputEnabled = false; maxOutput = 100.0;

            minCutoff = DEFAULT_MINCUTOFF;
            beta = DEFAULT_BETA;
            smoothingMethod = SmoothingMethod.None;
            useSmoothing = false;
            smoothWeight = 0.5;
        }

        public void ResetSmoothing()
        {
            useSmoothing = false;
            ResetSmoothingMethods();
        }

        public void ResetSmoothingMethods()
        {
            smoothingMethod = SmoothingMethod.None;
        }

        public void DetermineSmoothMethod(string identier)
        {
            ResetSmoothingMethods();

            switch (identier)
            {
                case "weighted-average":
                    smoothingMethod = SmoothingMethod.WeightedAverage;
                    break;
                case "one-euro":
                    smoothingMethod = SmoothingMethod.OneEuro;
                    break;
                default:
                    smoothingMethod = SmoothingMethod.None;
                    break;
            }
        }

        public string SmoothMethodIdentifier()
        {
            string result = "none";
            switch (smoothingMethod)
            {
                case SmoothingMethod.WeightedAverage:
                    result = "weighted-average";
                    break;
                case SmoothingMethod.OneEuro:
                    result = "one-euro";
                    break;
                default:
                    break;
            }

            return result;
        }

        public void SetRefreshEvents(OneEuroFilter euroFilter)
        {
            BetaChanged += (sender, args) =>
            {
                euroFilter.Beta = beta;
            };

            MinCutoffChanged += (sender, args) =>
            {
                euroFilter.MinCutoff = minCutoff;
            };
        }

        public void RemoveRefreshEvents()
        {
            BetaChanged = null;
            MinCutoffChanged = null;
        }
    }

    public class ButtonMouseInfo
    {
        //public const double MOUSESTICKANTIOFFSET = 0.0128;
        public const double MOUSESTICKANTIOFFSET = 0.008;

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
                ModeChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler ModeChanged;

        public LightbarSettingInfo()
        {
            /*ModeChanged += (sender, e) =>
            {
                if (mode != LightbarMode.DS4Win)
                {
                    ds4winSettings = null;
                }
            };
            */
        }
    }

    public class SteeringWheelSmoothingInfo
    {
        private double minCutoff = OneEuroFilterPair.DEFAULT_WHEEL_CUTOFF;
        private double beta = OneEuroFilterPair.DEFAULT_WHEEL_BETA;
        public bool enabled = false;

        public delegate void SmoothingInfoEventHandler(SteeringWheelSmoothingInfo sender, EventArgs args);

        public double MinCutoff
        {
            get => minCutoff;
            set
            {
                if (minCutoff == value) return;
                minCutoff = value;
                MinCutoffChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event SmoothingInfoEventHandler MinCutoffChanged;

        public double Beta
        {
            get => beta;
            set
            {
                if (beta == value) return;
                beta = value;
                BetaChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event SmoothingInfoEventHandler BetaChanged;

        public bool Enabled
        {
            get => enabled;
            set => enabled = value;
        }

        public void Reset()
        {
            MinCutoff = OneEuroFilterPair.DEFAULT_WHEEL_CUTOFF;
            Beta = OneEuroFilterPair.DEFAULT_WHEEL_BETA;
            enabled = false;
        }

        public void SetFilterAttrs(OneEuroFilter euroFilter)
        {
            euroFilter.MinCutoff = minCutoff;
            euroFilter.Beta = beta;
        }

        public void SetRefreshEvents(OneEuroFilter euroFilter)
        {
            BetaChanged += (sender, args) =>
            {
                euroFilter.Beta = beta;
            };

            MinCutoffChanged += (sender, args) =>
            {
                euroFilter.MinCutoff = minCutoff;
            };
        }
    }
}