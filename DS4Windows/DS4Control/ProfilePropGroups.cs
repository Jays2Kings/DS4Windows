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
    }

    public class TriggerDeadZoneZInfo
    {
        public byte deadZone; // Trigger deadzone is expressed in axis units
        public int antiDeadZone;
        public int maxZone = 100;
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
}