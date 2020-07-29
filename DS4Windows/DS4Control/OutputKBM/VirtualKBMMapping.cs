
namespace DS4WinWPF.DS4Control
{
    public abstract class VirtualKBMMapping
    {
        public uint MOUSEEVENTF_LEFTDOWN = 2, MOUSEEVENTF_LEFTUP = 4,
            MOUSEEVENTF_RIGHTDOWN = 8, MOUSEEVENTF_RIGHTUP = 16,
            MOUSEEVENTF_MIDDLEDOWN = 32, MOUSEEVENTF_MIDDLEUP = 64,
            MOUSEEVENTF_XBUTTONDOWN = 128, MOUSEEVENTF_XBUTTONUP = 256,
            MOUSEEVENTF_WHEEL = 0x0800, MOUSEEVENTF_HWHEEL = 0x1000;

        public abstract void PopulateConstants();
        public abstract void PopulateMappings();
        public abstract uint GetRealEventKey(uint winVkKey);
    }
}
