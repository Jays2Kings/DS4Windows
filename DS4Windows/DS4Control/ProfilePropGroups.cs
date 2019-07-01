using System;

namespace DS4Windows
{
    public class SquareStickInfo
    {
        public bool lsMode;
        public bool rsMode;
        public double roundness = 5.0;
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
}