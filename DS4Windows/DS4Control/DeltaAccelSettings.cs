using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS4WinWPF.DS4Control
{
    public class DeltaAccelSettings
    {
        public bool enabled = true;
        public double multiplier = 4.0;
        public double maxTravel = 0.2;
        public double minTravel = 0.01;
        public double easingDuration = 0.2;
        public double minfactor = 1.2;

        public bool Enabled
        {
            get => enabled;
            set => enabled = value;
        }

        public double Multiplier
        {
            get => multiplier;
            set => multiplier = value;
        }

        public double MaxTravel
        {
            get => maxTravel;
            set => maxTravel = value;
        }

        public double MinTravel
        {
            get => minTravel;
            set => minTravel = value;
        }

        public double EasingDuration
        {
            get => easingDuration;
            set => easingDuration = value;
        }

        public double MinFactor
        {
            get => minfactor;
            set => minfactor = value;
        }

        public DeltaAccelSettings()
        {
        }

        public DeltaAccelSettings(DeltaAccelSettings other)
        {
            enabled = other.enabled;
            multiplier = other.multiplier;
            maxTravel = other.maxTravel;
            minTravel = other.minTravel;
            easingDuration = other.easingDuration;
            minfactor = other.minfactor;
        }

        public void Reset()
        {
            enabled = true;
            multiplier = 4.0;
            maxTravel = 0.2;
            minTravel = 0.01;
            easingDuration = 0.2;
            minfactor = 1.2;
        }
    }
}
