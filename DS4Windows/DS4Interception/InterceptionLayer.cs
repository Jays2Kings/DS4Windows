using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Diagnostics;
using System.Runtime.InteropServices;

namespace DS4Windows
{
    using InterceptionContext = IntPtr;
    using InterceptionDevice = Int32;
    using InterceptionPrecedence = Int32;
    using InterceptionFilter = UInt16;

    public delegate int InterceptionPredicate(InterceptionDevice device);

    [StructLayout(LayoutKind.Sequential)]
    public struct InterceptionMouseStroke
    {
        public InterceptionMouseState state;
        public InterceptionMouseFlag flags;
        public short rolling;
        public int x;
        public int y;
        public uint information;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct InterceptionKeyStroke
    {
        public ushort code;
        public InterceptionKeyState state;
        public uint information;
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct InterceptionStroke
    {
        [FieldOffset(0)] public InterceptionMouseStroke Mouse;
        [FieldOffset(0)] public InterceptionKeyStroke Key;
    }

    public enum InterceptionKeyState : ushort
    {
        DOWN = 0x00,
        UP = 0x01,
        E0 = 0x02,
        E1 = 0x04,
        TERMSRV_SET_LED = 0x08,
        TERMSRV_SHADOW = 0x10,
        TERMSRV_VKPACKET = 0x20
    }
    public enum InterceptionFilterKeyState : ushort
    {
        NONE = 0x0000,
        ALL = 0xFFFF,
        DOWN = InterceptionKeyState.UP,
        UP = InterceptionKeyState.UP << 1,
        E0 = InterceptionKeyState.E0 << 1,
        E1 = InterceptionKeyState.E1 << 1,
        TERMSRV_SET_LED = InterceptionKeyState.TERMSRV_SET_LED << 1,
        TERMSRV_SHADOW = InterceptionKeyState.TERMSRV_SHADOW << 1,
        TERMSRV_VKPACKET = InterceptionKeyState.TERMSRV_VKPACKET << 1
    }
    public enum InterceptionMouseState : ushort
    {
        LEFT_BUTTON_DOWN = 0x001,
        LEFT_BUTTON_UP = 0x002,
        RIGHT_BUTTON_DOWN = 0x004,
        RIGHT_BUTTON_UP = 0x008,
        MIDDLE_BUTTON_DOWN = 0x010,
        MIDDLE_BUTTON_UP = 0x020,

        BUTTON_1_DOWN = LEFT_BUTTON_DOWN,
        BUTTON_1_UP = LEFT_BUTTON_UP,
        BUTTON_2_DOWN = RIGHT_BUTTON_DOWN,
        BUTTON_2_UP = RIGHT_BUTTON_UP,
        BUTTON_3_DOWN = MIDDLE_BUTTON_DOWN,
        BUTTON_3_UP = MIDDLE_BUTTON_UP,

        BUTTON_4_DOWN = 0x040,
        BUTTON_4_UP = 0x080,
        BUTTON_5_DOWN = 0x100,
        BUTTON_5_UP = 0x200,

        WHEEL = 0x400,
        HWHEEL = 0x800
    }
    public enum InterceptionFilterMouseState : ushort
    {
        NONE = 0x0000,
        ALL = 0xFFFF,

        LEFT_BUTTON_DOWN = InterceptionMouseState.LEFT_BUTTON_DOWN,
        LEFT_BUTTON_UP = InterceptionMouseState.LEFT_BUTTON_UP,
        RIGHT_BUTTON_DOWN = InterceptionMouseState.RIGHT_BUTTON_DOWN,
        RIGHT_BUTTON_UP = InterceptionMouseState.RIGHT_BUTTON_UP,
        MIDDLE_BUTTON_DOWN = InterceptionMouseState.MIDDLE_BUTTON_DOWN,
        MIDDLE_BUTTON_UP = InterceptionMouseState.MIDDLE_BUTTON_UP,

        BUTTON_1_DOWN = InterceptionMouseState.BUTTON_1_DOWN,
        BUTTON_1_UP = InterceptionMouseState.BUTTON_1_UP,
        BUTTON_2_DOWN = InterceptionMouseState.BUTTON_2_DOWN,
        BUTTON_2_UP = InterceptionMouseState.BUTTON_2_UP,
        BUTTON_3_DOWN = InterceptionMouseState.BUTTON_3_DOWN,
        BUTTON_3_UP = InterceptionMouseState.BUTTON_3_UP,

        BUTTON_4_DOWN = InterceptionMouseState.BUTTON_4_DOWN,
        BUTTON_4_UP = InterceptionMouseState.BUTTON_4_UP,
        BUTTON_5_DOWN = InterceptionMouseState.BUTTON_5_DOWN,
        BUTTON_5_UP = InterceptionMouseState.BUTTON_5_UP,

        WHEEL = InterceptionMouseState.WHEEL,
        HWHEEL = InterceptionMouseState.HWHEEL,

        MOVE = 0x1000
    }

    public enum InterceptionMouseFlag : ushort
    {
        MOVE_RELATIVE = 0x000,
        MOVE_ABSOLUTE = 0x001,
        VIRTUAL_DESKTOP = 0x002,
        ATTRIBUTES_CHANGED = 0x004,
        MOVE_NOCOALESCE = 0x008,
        TERMSRV_SRC_SHADOW = 0x100
    }

    public class InterceptionLayer
    {
        [DllImport("interception.dll", EntryPoint = "interception_create_context", CallingConvention = CallingConvention.Cdecl)]
        public static extern InterceptionContext CreateContext();

        [DllImport("interception.dll", EntryPoint = "interception_destroy_context", CallingConvention = CallingConvention.Cdecl)]
        public static extern void DestroyContext(InterceptionContext context);

        [DllImport("interception.dll", EntryPoint = "interception_get_precedence", CallingConvention = CallingConvention.Cdecl)]
        public static extern InterceptionPrecedence GetPrecedence(InterceptionContext context, InterceptionDevice device);

        [DllImport("interception.dll", EntryPoint = "interception_set_precedence", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetPrecedence(InterceptionContext context, InterceptionDevice device, InterceptionPrecedence precedence);

        [DllImport("interception.dll", EntryPoint = "interception_get_filter", CallingConvention = CallingConvention.Cdecl)]
        public static extern InterceptionFilter GetFilter(InterceptionContext context, InterceptionDevice device);

        [DllImport("interception.dll", EntryPoint = "interception_set_filter", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetFilter(InterceptionContext context, InterceptionPredicate predicate, InterceptionFilter filter);

        [DllImport("interception.dll", EntryPoint = "interception_wait", CallingConvention = CallingConvention.Cdecl)]
        public static extern InterceptionDevice Wait(InterceptionContext context);

        [DllImport("interception.dll", EntryPoint = "interception_wait_with_timeout", CallingConvention = CallingConvention.Cdecl)]
        public static extern InterceptionDevice WaitWithTimeout(InterceptionContext context, UInt64 milliseconds);

        [DllImport("interception.dll", EntryPoint = "interception_send", CallingConvention = CallingConvention.Cdecl)]
        public static extern Int32 Send(InterceptionContext context, InterceptionDevice device, ref InterceptionStroke stroke, UInt32 numStrokes);

        [DllImport("interception.dll", EntryPoint = "interception_receive", CallingConvention = CallingConvention.Cdecl)]
        public static extern Int32 Receive(InterceptionContext context, InterceptionDevice device, ref InterceptionStroke stroke, UInt32 numStrokes);

        [DllImport("interception.dll", EntryPoint = "interception_get_hardware_id", CallingConvention = CallingConvention.Cdecl)]
        public static extern UInt32 GetHardwareID(InterceptionContext context, InterceptionDevice device, IntPtr hardware_id_buffer, UInt32 buffer_size);

        [DllImport("interception.dll", EntryPoint = "interception_is_invalid", CallingConvention = CallingConvention.Cdecl)]
        public static extern Int32 IsInvalid(InterceptionDevice device);

        [DllImport("interception.dll", EntryPoint = "interception_is_keyboard", CallingConvention = CallingConvention.Cdecl)]
        public static extern Int32 IsKeyboard(InterceptionDevice device);

        [DllImport("interception.dll", EntryPoint = "interception_is_mouse", CallingConvention = CallingConvention.Cdecl)]
        public static extern Int32 IsMouse(InterceptionDevice device);
    }
}