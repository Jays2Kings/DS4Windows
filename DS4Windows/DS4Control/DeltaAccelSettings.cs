/*
DS4Windows
Copyright (C) 2023  Travis Nickles

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS4WinWPF.DS4Control
{
    public class DeltaAccelSettings
    {
        public const bool ENABLED_DEFAULT = false;
        public const double MINFACTOR_DEFAULT = 1.0;
        public const double MULTIPLIER_DEFAULT = 4.0;
        public const double MAX_TRAVEL_DEFAULT = 0.2;
        public const double MIN_TRAVEL_DEFAULT = 0.01;
        public const double EASING_DURATION_DEFAULT = 0.2;

        public bool enabled = ENABLED_DEFAULT;
        public double multiplier = MULTIPLIER_DEFAULT;
        public double maxTravel = MAX_TRAVEL_DEFAULT;
        public double minTravel = MIN_TRAVEL_DEFAULT;
        public double easingDuration = EASING_DURATION_DEFAULT;
        public double minfactor = MINFACTOR_DEFAULT;

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
            enabled = ENABLED_DEFAULT;
            multiplier = MULTIPLIER_DEFAULT;
            maxTravel = MAX_TRAVEL_DEFAULT;
            minTravel = MIN_TRAVEL_DEFAULT;
            easingDuration = EASING_DURATION_DEFAULT;
            minfactor = MINFACTOR_DEFAULT;
        }
    }
}
