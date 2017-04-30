using System;
using System.Collections.Generic;
using System.ComponentModel;

using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace DS4Windows
{
    public partial class X360Device : ScpDevice
    {
        private const String DS3_BUS_CLASS_GUID = "{F679F562-3164-42CE-A4DB-E7DDBE723909}";
        private const int CONTROLLER_OFFSET = 1; // Device 0 is the virtual USB hub itself, and we leave devices 1-10 available for other software (like the Scarlet.Crush DualShock driver itself)
        private const int inputResolution = 127 - (-128);
        private const float reciprocalInputResolution = 1 / (float)inputResolution;
        private const int outputResolution = 32767 - (-32768);

        private int firstController = 1;
        // Device 0 is the virtual USB hub itself, and we can leave more available for other software (like the Scarlet.Crush DualShock driver)
        public int FirstController
        {
            get { return firstController; }
            set { firstController = value > 0 ? value : 1; }
        }

        protected Int32 Scale(Int32 Value, Boolean Flip)
        {
            Value -= 0x80;

            //float temp = (Value - (-128)) / (float)inputResolution;
            float temp = (Value - (-128)) * reciprocalInputResolution;
            if (Flip) temp = (temp - 0.5f) * -1.0f + 0.5f;

            return (Int32)(temp * outputResolution + (-32768));
        }


        public X360Device()
            : base(DS3_BUS_CLASS_GUID)
        {
            InitializeComponent();
        }

        public X360Device(IContainer container)
            : base(DS3_BUS_CLASS_GUID)
        {
            container.Add(this);

            InitializeComponent();
        }


        /* public override Boolean Open(int Instance = 0)
        {
            if (base.Open(Instance))
            {
            }

            return true;
        } */

        public override Boolean Open(String DevicePath)
        {
            m_Path = DevicePath;
            m_WinUsbHandle = (IntPtr)INVALID_HANDLE_VALUE;

            if (GetDeviceHandle(m_Path))
            {
                m_IsActive = true;
            }

            return true;
        }

        public override Boolean Start()
        {
            if (IsActive)
            {
            }

            return true;
        }

        public override Boolean Stop()
        {
            if (IsActive)
            {
                //Unplug(0);
            }

            return base.Stop();
        }

        public override Boolean Close()
        {
            if (IsActive)
            {
                Unplug(0);
            }

            return base.Close();
        }


        public void Parse(DS4State state, Byte[] Output, int device)
        {
            Output[0] = 0x1C;
            Output[4] = (Byte)(device + firstController);
            Output[9] = 0x14;

            for (int i = 10, outLen = Output.Length; i < outLen; i++)
            {
                Output[i] = 0;
            }

            if (state.Share) Output[10] |= (Byte)(1 << 5); // Back
            if (state.L3) Output[10] |= (Byte)(1 << 6); // Left  Thumb
            if (state.R3) Output[10] |= (Byte)(1 << 7); // Right Thumb
            if (state.Options) Output[10] |= (Byte)(1 << 4); // Start

            if (state.DpadUp) Output[10] |= (Byte)(1 << 0); // Up
            if (state.DpadRight) Output[10] |= (Byte)(1 << 3); // Down
            if (state.DpadDown) Output[10] |= (Byte)(1 << 1); // Right
            if (state.DpadLeft) Output[10] |= (Byte)(1 << 2); // Left

            if (state.L1) Output[11] |= (Byte)(1 << 0); // Left  Shoulder
            if (state.R1) Output[11] |= (Byte)(1 << 1); // Right Shoulder

            if (state.Triangle) Output[11] |= (Byte)(1 << 7); // Y
            if (state.Circle) Output[11] |= (Byte)(1 << 5); // B
            if (state.Cross) Output[11] |= (Byte)(1 << 4); // A
            if (state.Square) Output[11] |= (Byte)(1 << 6); // X

            if (state.PS) Output[11] |= (Byte)(1 << 2); // Guide     

            Output[12] = state.L2; // Left Trigger
            Output[13] = state.R2; // Right Trigger

            Int32 ThumbLX = Scale(state.LX, false);
            Int32 ThumbLY = Scale(state.LY, true);
            Int32 ThumbRX = Scale(state.RX, false);
            Int32 ThumbRY = Scale(state.RY, true);
            Output[14] = (Byte)((ThumbLX >> 0) & 0xFF); // LX
            Output[15] = (Byte)((ThumbLX >> 8) & 0xFF);            
            Output[16] = (Byte)((ThumbLY >> 0) & 0xFF); // LY
            Output[17] = (Byte)((ThumbLY >> 8) & 0xFF);
            Output[18] = (Byte)((ThumbRX >> 0) & 0xFF); // RX
            Output[19] = (Byte)((ThumbRX >> 8) & 0xFF);
            Output[20] = (Byte)((ThumbRY >> 0) & 0xFF); // RY
            Output[21] = (Byte)((ThumbRY >> 8) & 0xFF);
        }

        public Boolean Plugin(Int32 Serial)
        {
            if (IsActive)
            {
                Int32 Transfered = 0;
                Byte[] Buffer = new Byte[16];

                Buffer[0] = 0x10;
                Buffer[1] = 0x00;
                Buffer[2] = 0x00;
                Buffer[3] = 0x00;

                Serial += firstController;
                Buffer[4] = (Byte)((Serial >> 0) & 0xFF);
                Buffer[5] = (Byte)((Serial >> 8) & 0xFF);
                Buffer[6] = (Byte)((Serial >> 16) & 0xFF);
                Buffer[7] = (Byte)((Serial >> 24) & 0xFF);

                return DeviceIoControl(m_FileHandle, 0x2A4000, Buffer, Buffer.Length, null, 0, ref Transfered, IntPtr.Zero);
            }

            return false;
        }

        public Boolean Unplug(Int32 Serial)
        {
            if (IsActive)
            {
                Int32 Transfered = 0;
                Byte[] Buffer = new Byte[16];

                Buffer[0] = 0x10;
                Buffer[1] = 0x00;
                Buffer[2] = 0x00;
                Buffer[3] = 0x00;

                Serial += firstController;
                Buffer[4] = (Byte)((Serial >> 0) & 0xFF);
                Buffer[5] = (Byte)((Serial >> 8) & 0xFF);
                Buffer[6] = (Byte)((Serial >> 16) & 0xFF);
                Buffer[7] = (Byte)((Serial >> 24) & 0xFF);

                return DeviceIoControl(m_FileHandle, 0x2A4004, Buffer, Buffer.Length, null, 0, ref Transfered, IntPtr.Zero);
            }

            return false;
        }

        public Boolean UnplugAll() //not yet implemented, not sure if will
        {
            if (IsActive)
            {
                Int32 Transfered = 0;
                Byte[] Buffer = new Byte[16];

                Buffer[0] = 0x10;
                Buffer[1] = 0x00;
                Buffer[2] = 0x00;
                Buffer[3] = 0x00;

                return DeviceIoControl(m_FileHandle, 0x2A4004, Buffer, Buffer.Length, null, 0, ref Transfered, IntPtr.Zero);
            }

            return false;
        }


        public Boolean Report(Byte[] Input, Byte[] Output)
        {
            if (IsActive)
            {
                Int32 Transfered = 0;

                return DeviceIoControl(m_FileHandle, 0x2A400C, Input, Input.Length, Output, Output.Length, ref Transfered, IntPtr.Zero) && Transfered > 0;
            }

            return false;
        }
    }
}
