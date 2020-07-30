
namespace DS4Windows.DS4Control
{
    public class SendInputMapping : VirtualKBMMapping
    {
        public override void PopulateConstants()
        {
            MOUSEEVENTF_LEFTDOWN = 2; MOUSEEVENTF_LEFTUP = 4;
            MOUSEEVENTF_RIGHTDOWN = 8; MOUSEEVENTF_RIGHTUP = 16;
            MOUSEEVENTF_MIDDLEDOWN = 32; MOUSEEVENTF_MIDDLEUP = 64;
            MOUSEEVENTF_XBUTTONDOWN = 128; MOUSEEVENTF_XBUTTONUP = 256;
            MOUSEEVENTF_WHEEL = 0x0800; MOUSEEVENTF_HWHEEL = 0x1000;
        }

        /// <summary>
        /// Not really needed as Window keys are the standard being mapped against
        /// </summary>
        public override void PopulateMappings()
        {
        }

        /// <summary>
        /// Not really needed here as Window keys are the standard being mapped against. Just return key
        /// </summary>
        /// <param name="winVkKey">Windows Virtual Key value</param>
        /// <returns>Windows Virtual Key value</returns>
        public override uint GetRealEventKey(uint winVkKey)
        {
            return winVkKey;
        }
    }
}
