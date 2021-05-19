using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DS4Windows
{
    [StructLayout(LayoutKind.Sequential, Size = 9)]
    unsafe struct DS4_TOUCH
    {
        public byte bPacketCounter;
        public byte bIsUpTrackingNum1;
        public fixed byte bTouchData1[3];
        public byte bIsUpTrackingNum2;
        public fixed byte bTouchData2[3];
    }

    [StructLayout(LayoutKind.Explicit)]
    unsafe struct DS4_REPORT_UNION
    {
        [FieldOffset(0)]
        public DS4_REPORT_EX reportStruct;

        [FieldOffset(0)]
        public fixed byte Report[63];
    }

    [StructLayout(LayoutKind.Explicit, Size = 63)]
    unsafe struct DS4_REPORT_EX
    {
        [FieldOffset(0)]
        public byte bThumbLX;
        [FieldOffset(1)]
        public byte bThumbLY;
        [FieldOffset(2)]
        public byte bThumbRX;
        [FieldOffset(3)]
        public byte bThumbRY;
        [FieldOffset(4)]
        public ushort wButtons;
        [FieldOffset(6)]
        public byte bSpecial;
        [FieldOffset(7)]
        public byte bTriggerL;
        [FieldOffset(8)]
        public byte bTriggerR;
        [FieldOffset(9)]
        public ushort wTimestamp;
        [FieldOffset(11)]
        public byte bBatteryLvl;
        [FieldOffset(12)]
        public short wGyroX;
        [FieldOffset(14)]
        public short wGyroY;
        [FieldOffset(16)]
        public short wGyroZ;
        [FieldOffset(18)]
        public short wAccelX;
        [FieldOffset(20)]
        public short wAccelY;
        [FieldOffset(22)]
        public short wAccelZ;
        [FieldOffset(24)]
        public fixed byte _bUnknown1[5];
        [FieldOffset(29)]
        public byte bBatteryLvlSpecial;
        [FieldOffset(30)]
        public fixed byte _bUnknown2[2];
        [FieldOffset(32)]
        public byte bTouchPacketsN;
        [FieldOffset(33)]
        public DS4_TOUCH sCurrentTouch;
        [FieldOffset(42)]
        public DS4_TOUCH sPreviousTouch1;
        [FieldOffset(51)]
        public DS4_TOUCH sPreviousTouch2;
    }

    internal static class DS4OutDeviceExtras
    {
        public static void CopyBytes(ref DS4_REPORT_EX outReport, byte[] outBuffer)
        {
            GCHandle h = GCHandle.Alloc(outReport, GCHandleType.Pinned);
            Marshal.Copy(h.AddrOfPinnedObject(), outBuffer, 0, 63);
            h.Free();
        }
    }
}
