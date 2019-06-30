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
}