
namespace DS4Windows.DS4Control
{
    public abstract class VirtualKBMMapping
    {
        public uint MOUSEEVENTF_LEFTDOWN = 2, MOUSEEVENTF_LEFTUP = 4,
            MOUSEEVENTF_RIGHTDOWN = 8, MOUSEEVENTF_RIGHTUP = 16,
            MOUSEEVENTF_MIDDLEDOWN = 32, MOUSEEVENTF_MIDDLEUP = 64,
            MOUSEEVENTF_XBUTTONDOWN = 128, MOUSEEVENTF_XBUTTONUP = 256,
            MOUSEEVENTF_WHEEL = 0x0800, MOUSEEVENTF_HWHEEL = 0x1000;

        public uint KEY_TAB = 0x09, KEY_LALT = 0x12;
        public int WHEEL_TICK_DOWN = -120, WHEEL_TICK_UP = 120;
        public bool macroKeyTranslate = false;

        public abstract void PopulateConstants();
        public abstract void PopulateMappings();
        public abstract uint GetRealEventKey(uint winVkKey);
    }
}
