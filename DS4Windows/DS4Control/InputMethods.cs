using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
namespace DS4Windows
{
    class InputMethods
    {
        private static Interception interception = new Interception();

        public static void MoveCursorBy(int x, int y)
        {
            if (x != 0 || y != 0)
                interception.MoveMouseAbsolute(x, y);
        }

        public static void MouseWheel(int vertical, int horizontal)
        {
            if(vertical != 0)
                interception.MouseWheel((short)vertical);

            if (horizontal != 0)
                interception.MouseHWheel((short)horizontal);
        }

        public static void MouseEvent(uint mouseButton)
        {
            ushort state = (ushort)(mouseButton >> 1);
            if (state > 0 && state < 33)
                interception.MouseClick((InterceptionMouseState)state);
        }

        public static void MouseEvent(uint mouseButton, int type)
        {
            if ((mouseButton & InputMethods.MOUSEEVENTF_XBUTTONUP) != 0)
            {
                if (type == 1)
                    interception.MouseClick(InterceptionMouseState.BUTTON_4_DOWN);
                if (type == 2)
                    interception.MouseClick(InterceptionMouseState.BUTTON_5_DOWN);
            }
            else if ((mouseButton & InputMethods.MOUSEEVENTF_XBUTTONUP) != 0)
            {
                if (type == 1)
                    interception.MouseClick(InterceptionMouseState.BUTTON_4_UP);
                if (type == 2)
                    interception.MouseClick(InterceptionMouseState.BUTTON_5_UP);
            }
        }

        public static void performLeftClick()
        {
            interception.MouseClick(InterceptionMouseState.LEFT_BUTTON_UP | InterceptionMouseState.LEFT_BUTTON_DOWN);
        }

        public static void performRightClick()
        {
            interception.MouseClick(InterceptionMouseState.RIGHT_BUTTON_UP | InterceptionMouseState.RIGHT_BUTTON_DOWN);
        }

        public static void performMiddleClick()
        {
            interception.MouseClick(InterceptionMouseState.MIDDLE_BUTTON_UP | InterceptionMouseState.MIDDLE_BUTTON_DOWN);
        }

        public static void performFourthClick()
        {
            interception.MouseClick(InterceptionMouseState.BUTTON_4_UP | InterceptionMouseState.BUTTON_4_DOWN);
        }
        public static void performSCKeyPress(ushort key)
        {
            interception.KeyDown(key);
        }

        public static void performKeyPress(ushort key)
        {
            interception.KeyDown(MapVirtualKey(key, MAPVK_VK_TO_VSC));
        }

        public static void performSCKeyRelease(ushort key)
        {
            interception.KeyUp(key);
        }

        public static void performKeyRelease(ushort key)
        {
            interception.KeyUp(MapVirtualKey(key, MAPVK_VK_TO_VSC));
        }

        internal const uint MOUSEEVENTF_MOVE = 1, // just apply X/Y (delta due to not setting absolute flag)
            MOUSEEVENTF_LEFTDOWN = 2, MOUSEEVENTF_LEFTUP = 4,
            MOUSEEVENTF_RIGHTDOWN = 8, MOUSEEVENTF_RIGHTUP = 16,
            MOUSEEVENTF_MIDDLEDOWN = 32, MOUSEEVENTF_MIDDLEUP = 64,
            MOUSEEVENTF_XBUTTONDOWN = 128, MOUSEEVENTF_XBUTTONUP = 256,
            KEYEVENTF_EXTENDEDKEY = 1, KEYEVENTF_KEYUP = 2, MOUSEEVENTF_WHEEL = 0x0800, MOUSEEVENTF_HWHEEL = 0x1000,
            MOUSEEVENTF_MIDDLEWDOWN = 0x0020, MOUSEEVENTF_MIDDLEWUP = 0x0040,
            KEYEVENTF_SCANCODE = 0x0008, MAPVK_VK_TO_VSC = 0, KEYEVENTF_UNICODE = 0x0004;

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern ushort MapVirtualKey(uint uCode, uint uMapType);
    }
}