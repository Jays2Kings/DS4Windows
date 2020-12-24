using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DS4Windows
{
    [StructLayout(LayoutKind.Sequential)]
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

    [StructLayout(LayoutKind.Sequential, Size = 63)]
    unsafe struct DS4_REPORT_EX
    {
        public byte bThumbLX;
        public byte bThumbLY;
        public byte bThumbRX;
        public byte bThumbRY;
        public ushort wButtons;
        public byte bSpecial;
        public byte bTriggerL;
        public byte bTriggerR;
        public ushort wTimestamp;
        public byte bBatteryLvl;
        public short wGyroX;
        public short wGyroY;
        public short wGyroZ;
        public short wAccelX;
        public short wAccelY;
        public short wAccelZ;
        public fixed byte _bUnknown1[5];
        public byte bBatteryLvlSpecial;
        public fixed byte _bUnknown2[2];
        public byte bTouchPacketsN;
        public DS4_TOUCH sCurrentTouch;
        public DS4_TOUCH sPreviousTouch1;
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
