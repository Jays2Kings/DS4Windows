using System;
using System.Runtime.InteropServices;
using System.Security;

namespace DS4Windows
{
    [SuppressUnmanagedCodeSecurity]
    class InputMethods
    {
        public static void MoveCursorBy(int x, int y)
        {
            if (x != 0 || y != 0)
            {
                INPUT[] tempInput = new INPUT[1];
                ref INPUT temp = ref tempInput[0];
                temp.Type = INPUT_MOUSE;
                temp.Data.Mouse.ExtraInfo = IntPtr.Zero;
                temp.Data.Mouse.Flags = MOUSEEVENTF_MOVE;
                temp.Data.Mouse.MouseData = 0;
                temp.Data.Mouse.Time = 0;
                temp.Data.Mouse.X = x;
                temp.Data.Mouse.Y = y;
                uint result = SendInput(1, tempInput, Marshal.SizeOf(tempInput[0]));
            }
        }

        private const double ABSOLUTE_MOUSE_COOR_MAX = 65535.0;

        /// <summary>
        /// Move the mouse cursor to an absolute position on the virtual desktop
        /// </summary>
        /// <param name="x">X coordinate in range of [0.0, 1.0]. 0.0 for left. 1.0 for far right</param>
        /// <param name="y">Y coordinate in range of [0.0, 1.0]. 0.0 for top. 1.0 for bottom</param>
        public static void MoveAbsoluteMouse(double x, double y)
        {
            INPUT[] tempInput = new INPUT[1];
            ref INPUT temp = ref tempInput[0];
            temp.Type = INPUT_MOUSE;
            temp.Data.Mouse.ExtraInfo = IntPtr.Zero;
            temp.Data.Mouse.Flags = MOUSEEVENTF_MOVE | MOUSEEVENTF_VIRTUALDESK | MOUSEEVENTF_ABSOLUTE;
            temp.Data.Mouse.MouseData = 0;
            temp.Data.Mouse.Time = 0;
            temp.Data.Mouse.X = (int)(x * ABSOLUTE_MOUSE_COOR_MAX);
            temp.Data.Mouse.Y = (int)(y * ABSOLUTE_MOUSE_COOR_MAX);
            uint result = SendInput(1, tempInput, Marshal.SizeOf(tempInput[0]));
        }

        public static void MouseWheel(int vertical, int horizontal)
        {
            INPUT[] tempInput = new INPUT[2];
            uint inputs = 0;
            ref INPUT temp = ref tempInput[inputs];
            if (vertical != 0)
            {
                temp.Type = INPUT_MOUSE;
                temp.Data.Mouse.ExtraInfo = IntPtr.Zero;
                temp.Data.Mouse.Flags = MOUSEEVENTF_WHEEL;
                temp.Data.Mouse.MouseData = (uint)vertical;
                temp.Data.Mouse.Time = 0;
                temp.Data.Mouse.X = 0;
                temp.Data.Mouse.Y = 0;
                inputs++;
            }

            if (horizontal != 0)
            {
                temp = ref tempInput[inputs];
                temp.Type = INPUT_MOUSE;
                temp.Data.Mouse.ExtraInfo = IntPtr.Zero;
                temp.Data.Mouse.Flags = MOUSEEVENTF_HWHEEL;
                temp.Data.Mouse.MouseData = (uint)horizontal;
                temp.Data.Mouse.Time = 0;
                temp.Data.Mouse.X = 0;
                temp.Data.Mouse.Y = 0;
                inputs++;
            }

            SendInput(inputs, tempInput, (int)inputs * Marshal.SizeOf(tempInput[0]));
        }

        public static void MouseEvent(uint mouseButton)
        {
            INPUT[] tempInput = new INPUT[1];
            ref INPUT temp = ref tempInput[0];
            temp.Type = INPUT_MOUSE;
            temp.Data.Mouse.ExtraInfo = IntPtr.Zero;
            temp.Data.Mouse.Flags = mouseButton;
            temp.Data.Mouse.MouseData = 0;
            temp.Data.Mouse.Time = 0;
            temp.Data.Mouse.X = 0;
            temp.Data.Mouse.Y = 0;
            uint result = SendInput(1, tempInput, Marshal.SizeOf(tempInput[0]));
        }

        public static void MouseEvent(uint mouseButton, int type)
        {
            INPUT[] tempInput = new INPUT[1];
            ref INPUT temp = ref tempInput[0];
            temp.Type = INPUT_MOUSE;
            temp.Data.Mouse.ExtraInfo = IntPtr.Zero;
            temp.Data.Mouse.Flags = mouseButton;
            temp.Data.Mouse.MouseData = (uint)type;
            temp.Data.Mouse.Time = 0;
            temp.Data.Mouse.X = 0;
            temp.Data.Mouse.Y = 0;
            uint result = SendInput(1, tempInput, Marshal.SizeOf(tempInput[0]));
        }

        public static void performSCKeyPress(ushort key)
        {
            INPUT[] tempInput = new INPUT[1];
            ref INPUT temp = ref tempInput[0];
            ushort scancode = scancodeFromVK(key);
            bool extended = (scancode & 0x100) != 0;
            uint curflags = extended ? KEYEVENTF_EXTENDEDKEY : 0;

            temp.Type = INPUT_KEYBOARD;
            temp.Data.Keyboard.ExtraInfo = IntPtr.Zero;
            temp.Data.Keyboard.Flags = KEYEVENTF_SCANCODE | curflags;
            temp.Data.Keyboard.Scan = scancode;
            temp.Data.Keyboard.Time = 0;
            temp.Data.Keyboard.Vk = key;
            uint result = SendInput(1, tempInput, Marshal.SizeOf(tempInput[0]));
        }

        public static void performKeyPress(ushort key)
        {
            INPUT[] tempInput = new INPUT[1];
            ref INPUT temp = ref tempInput[0];
            ushort scancode = scancodeFromVK(key);
            bool extended = (scancode & 0x100) != 0;
            uint curflags = extended ? KEYEVENTF_EXTENDEDKEY : 0;

            temp.Type = INPUT_KEYBOARD;
            temp.Data.Keyboard.ExtraInfo = IntPtr.Zero;
            temp.Data.Keyboard.Flags = curflags;
            temp.Data.Keyboard.Scan = scancode;
            //sendInputs[0].Data.Keyboard.Flags = 1;
            //sendInputs[0].Data.Keyboard.Scan = 0;
            temp.Data.Keyboard.Time = 0;
            temp.Data.Keyboard.Vk = key;
            uint result = SendInput(1, tempInput, Marshal.SizeOf(tempInput[0]));
        }

        public static void performSCKeyRelease(ushort key)
        {
            INPUT[] tempInput = new INPUT[1];
            ref INPUT temp = ref tempInput[0];
            ushort scancode = scancodeFromVK(key);
            bool extended = (scancode & 0x100) != 0;
            uint curflags = extended ? KEYEVENTF_EXTENDEDKEY : 0;

            temp.Type = INPUT_KEYBOARD;
            temp.Data.Keyboard.ExtraInfo = IntPtr.Zero;
            temp.Data.Keyboard.Flags = KEYEVENTF_SCANCODE | KEYEVENTF_KEYUP | curflags;
            temp.Data.Keyboard.Scan = scancode;
            temp.Data.Keyboard.Time = 0;
            //sendInputs[0].Data.Keyboard.Vk = MapVirtualKey(key, MAPVK_VK_TO_VSC);
            uint result = SendInput(1, tempInput, Marshal.SizeOf(tempInput[0]));
        }

        public static void performKeyRelease(ushort key)
        {
            INPUT[] tempInput = new INPUT[1];
            ref INPUT temp = ref tempInput[0];
            ushort scancode = scancodeFromVK(key);
            bool extended = (scancode & 0x100) != 0;
            uint curflags = extended ? KEYEVENTF_EXTENDEDKEY : 0;

            temp.Type = INPUT_KEYBOARD;
            temp.Data.Keyboard.ExtraInfo = IntPtr.Zero;
            temp.Data.Keyboard.Flags = curflags | KEYEVENTF_KEYUP;
            temp.Data.Keyboard.Scan = scancode;
            //sendInputs[0].Data.Keyboard.Flags = KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP;
            //sendInputs[0].Data.Keyboard.Scan = 0;
            temp.Data.Keyboard.Time = 0;
            temp.Data.Keyboard.Vk = key;
            uint result = SendInput(1, tempInput, Marshal.SizeOf(tempInput[0]));
        }

        private static ushort scancodeFromVK(uint vkey)
        {
            ushort scancode = 0;
            if (vkey == VK_PAUSE)
            {
                // MapVirtualKey does not work with VK_PAUSE
                scancode = 0x45;
            }
            else
            {
                scancode = MapVirtualKey(vkey, MAPVK_VK_TO_VSC);
            }

            switch (vkey)
            {
                case VK_LEFT:
                case VK_UP:
                case VK_RIGHT:
                case VK_DOWN:
                case VK_PRIOR:
                case VK_NEXT:
                case VK_END:
                case VK_HOME:
                case VK_INSERT:
                case VK_DELETE:
                case VK_DIVIDE:
                case VK_NUMLOCK:
                case VK_RCONTROL:
                case VK_RMENU:
                case VK_VOLUME_MUTE:
                case VK_VOLUME_DOWN:
                case VK_VOLUME_UP:
                case VK_MEDIA_NEXT_TRACK:
                case VK_MEDIA_PREV_TRACK:
                case VK_LAUNCH_MEDIA_SELECT:
                case VK_BROWSER_HOME:
                case VK_LAUNCH_MAIL:
                case VK_LAUNCH_APP1:
                case VK_LAUNCH_APP2:
                case VK_APPS:
                {
                    scancode |= (ushort)EXTENDED_FLAG; // set extended bit
                    break;
                }
            }

            return scancode;
        }

        /// <summary>
        /// http://msdn.microsoft.com/en-us/library/windows/desktop/ms646270(v=vs.85).aspx
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        internal struct INPUT
        {
            public uint Type;
            public MOUSEKEYBDHARDWAREINPUT Data;
        }
        internal const uint INPUT_MOUSE = 0, INPUT_KEYBOARD = 1, INPUT_HARDWARE = 2;

        /// <summary>
        /// http://social.msdn.microsoft.com/Forums/en/csharplanguage/thread/f0e82d6e-4999-4d22-b3d3-32b25f61fb2a
        /// </summary>
        [StructLayout(LayoutKind.Explicit)]
        internal struct MOUSEKEYBDHARDWAREINPUT
        {
            [FieldOffset(0)]
            public HARDWAREINPUT Hardware;
            [FieldOffset(0)]
            public KEYBDINPUT Keyboard;
            [FieldOffset(0)]
            public MOUSEINPUT Mouse;
        }

        /// <summary>
        /// http://msdn.microsoft.com/en-us/library/windows/desktop/ms646310(v=vs.85).aspx
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        internal struct HARDWAREINPUT
        {
            public uint Msg;
            public ushort ParamL;
            public ushort ParamH;
        }

        /// <summary>
        /// http://msdn.microsoft.com/en-us/library/windows/desktop/ms646310(v=vs.85).aspx
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        internal struct KEYBDINPUT
        {
            public ushort Vk;
            public ushort Scan;
            public uint Flags;
            public uint Time;
            public IntPtr ExtraInfo;
        }

        /// <summary>
        /// http://social.msdn.microsoft.com/forums/en-US/netfxbcl/thread/2abc6be8-c593-4686-93d2-89785232dacd
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        internal struct MOUSEINPUT
        {
            public int X;
            public int Y;
            public uint MouseData;
            public uint Flags;
            public uint Time;
            public IntPtr ExtraInfo;
        }

        internal const uint MOUSEEVENTF_MOVE = 1, // just apply X/Y (delta due to not setting absolute flag)
            MOUSEEVENTF_LEFTDOWN = 2, MOUSEEVENTF_LEFTUP = 4,
            MOUSEEVENTF_RIGHTDOWN = 8, MOUSEEVENTF_RIGHTUP = 16,
            MOUSEEVENTF_MIDDLEDOWN = 32, MOUSEEVENTF_MIDDLEUP = 64,
            MOUSEEVENTF_XBUTTONDOWN = 128, MOUSEEVENTF_XBUTTONUP = 256,
            MOUSEEVENTF_ABSOLUTE = 0x8000, MOUSEEVENTF_VIRTUALDESK = 0x4000,
            KEYEVENTF_EXTENDEDKEY = 1, KEYEVENTF_KEYUP = 2, MOUSEEVENTF_WHEEL = 0x0800, MOUSEEVENTF_HWHEEL = 0x1000,
            MOUSEEVENTF_MIDDLEWDOWN = 0x0020, MOUSEEVENTF_MIDDLEWUP = 0x0040,
            KEYEVENTF_SCANCODE = 0x0008, MAPVK_VK_TO_VSC = 0, KEYEVENTF_UNICODE = 0x0004, EXTENDED_FLAG = 0x100;

        internal const uint VK_PAUSE = 0x13, VK_LEFT = 0x25, VK_UP = 0x26, VK_RIGHT = 0x27, VK_DOWN = 0x28,
            VK_PRIOR = 0x21, VK_NEXT = 0x22, VK_END = 0x23, VK_HOME = 0x24, VK_INSERT = 0x2D, VK_DELETE = 0x2E, VK_APPS = 0x5D,
            VK_DIVIDE = 0x6F, VK_NUMLOCK = 0x90, VK_RCONTROL = 0xA3, VK_RMENU = 0xA5, VK_BROWSER_HOME = 0xAC,
            VK_VOLUME_MUTE = 0xAD, VK_VOLUME_DOWN = 0xAE, VK_VOLUME_UP = 0xAF,
            VK_MEDIA_NEXT_TRACK = 0xB0, VK_MEDIA_PREV_TRACK = 0xB1, VK_MEDIA_STOP = 0xB2, VK_MEDIA_PLAY_PAUSE = 0xB3,
            VK_LAUNCH_MAIL = 0xB4, VK_LAUNCH_MEDIA_SELECT = 0xB5, VK_LAUNCH_APP1 = 0xB6, VK_LAUNCH_APP2 = 0xB7;

        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint SendInput(uint numberOfInputs, INPUT[] inputs, int sizeOfInputs);
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern ushort MapVirtualKey(uint uCode, uint uMapType);
        [DllImport("user32.dll", SetLastError = true)]
        static extern void keybd_event(byte bVk, byte bScan, int dwFlags, int dwExtraInfo);

        //Not used, just here
        public static void DownKeys(ushort key)
        {
            keybd_event((byte)key, 0, (int)0, 0);
        }

        public static void PressKeys(ushort key)
        {
            keybd_event((byte)key, 0, (int)KEYEVENTF_EXTENDEDKEY, 0);
        }

        public static void ReleaseKeys(ushort key)
        {
            keybd_event((byte)key, 0, (int)KEYEVENTF_KEYUP, 0);
        }
    }
}

