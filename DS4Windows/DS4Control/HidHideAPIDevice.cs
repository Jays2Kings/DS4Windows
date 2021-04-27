using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using DS4Windows;

namespace DS4WinWPF.DS4Control
{
    class HidHideAPIDevice : IDisposable
    {
        private const uint IOCTL_GET_WHITELIST = 0x80016000;
        private const uint IOCTL_SET_WHITELIST = 0x80016004;
        private const uint IOCTL_GET_BLACKLIST = 0x80016008;
        private const uint IOCTL_SET_BLACKLIST = 0x8001600C;
        private const uint IOCTL_GET_ACTIVE = 0x80016010;
        private const uint IOCTL_SET_ACTIVE = 0x80016014;

        private const string CONTROL_DEVICE_FILENAME = "\\\\.\\HidHide";

        private SafeHandle hidHideHandle;

        public HidHideAPIDevice()
        {
            hidHideHandle = NativeMethods.CreateFile(CONTROL_DEVICE_FILENAME,
                    NativeMethods.GENERIC_READ,
                    NativeMethods.FILE_SHARE_READ | NativeMethods.FILE_SHARE_WRITE,
                    IntPtr.Zero,
                    NativeMethods.OpenExisting,
                    NativeMethods.FILE_ATTRIBUTE_NORMAL, 0);
        }

        public bool GetActiveState()
        {
            bool result = false;

            unsafe
            {
                int bytesReturned = 0;
                NativeMethods.DeviceIoControl(hidHideHandle.DangerousGetHandle(),
                    HidHideAPIDevice.IOCTL_GET_ACTIVE,
                    IntPtr.Zero,
                    0,
                    new IntPtr(&result),
                    1,
                    ref bytesReturned,
                    IntPtr.Zero);

                //int error = Marshal.GetLastWin32Error();
            }

            return result;
        }

        public bool SetActiveState(bool state)
        {
            bool result = false;

            unsafe
            {
                int bytesReturned = 0;
                NativeMethods.DeviceIoControl(hidHideHandle.DangerousGetHandle(),
                    HidHideAPIDevice.IOCTL_SET_ACTIVE,
                    new IntPtr(&state),
                    1,
                    IntPtr.Zero,
                    0,
                    ref bytesReturned,
                    IntPtr.Zero);

                //int error = Marshal.GetLastWin32Error();
            }

            return result;
        }

        public List<string> GetBlacklist()
        {
            List<string> instances = new List<string>();

            int bytesReturned = 0;
            bool result = NativeMethods.DeviceIoControl(hidHideHandle.DangerousGetHandle(),
                HidHideAPIDevice.IOCTL_GET_BLACKLIST,
                IntPtr.Zero,
                0,
                IntPtr.Zero,
                0,
                ref bytesReturned,
                IntPtr.Zero);

            if (bytesReturned > 0)
            {
                byte[] dataBuffer = new byte[bytesReturned];
                int requiredBytes = bytesReturned;
                bytesReturned = 0;

                IntPtr buffer = Marshal.AllocHGlobal(requiredBytes);

                result = NativeMethods.DeviceIoControl(hidHideHandle.DangerousGetHandle(),
                    HidHideAPIDevice.IOCTL_GET_BLACKLIST,
                    IntPtr.Zero,
                    0,
                    buffer,
                    requiredBytes,
                    ref bytesReturned,
                    IntPtr.Zero);

                //int error = Marshal.GetLastWin32Error();
                Marshal.Copy(buffer, dataBuffer, 0, requiredBytes);
                string tempstring = Encoding.Unicode.GetString(dataBuffer).TrimEnd(char.MinValue);
                instances = tempstring.Split(char.MinValue).ToList();

                Marshal.FreeHGlobal(buffer);
            }

            return instances;
        }

        public bool SetBlacklist(List<string> instances)
        {
            bool result = false;
            int bytesReturned = 0;
            IntPtr inBuffer =
                StringListToMultiSzPointer(instances, out int inBufferLength);

            result = NativeMethods.DeviceIoControl(hidHideHandle.DangerousGetHandle(),
                IOCTL_SET_BLACKLIST,
                inBuffer,
                inBufferLength,
                IntPtr.Zero,
                0,
                ref bytesReturned,
                IntPtr.Zero);

            //int error = Marshal.GetLastWin32Error();
            // Free buffer returned from StringListToMultiSzPointer
            Marshal.FreeHGlobal(inBuffer);

            return result;
        }

        public List<string> GetWhitelist()
        {
            List<string> instances = new List<string>();

            int bytesReturned = 0;
            bool result = NativeMethods.DeviceIoControl(hidHideHandle.DangerousGetHandle(),
                IOCTL_GET_WHITELIST,
                IntPtr.Zero,
                0,
                IntPtr.Zero,
                0,
                ref bytesReturned,
                IntPtr.Zero);

            if (bytesReturned > 0)
            {
                byte[] dataBuffer = new byte[bytesReturned];
                int requiredBytes = bytesReturned;
                bytesReturned = 0;

                IntPtr buffer = Marshal.AllocHGlobal(requiredBytes);

                result = NativeMethods.DeviceIoControl(hidHideHandle.DangerousGetHandle(),
                    IOCTL_GET_WHITELIST,
                    IntPtr.Zero,
                    0,
                    buffer,
                    requiredBytes,
                    ref bytesReturned,
                    IntPtr.Zero);

                //int error = Marshal.GetLastWin32Error();
                Marshal.Copy(buffer, dataBuffer, 0, requiredBytes);
                string tempstring = Encoding.Unicode.GetString(dataBuffer).TrimEnd(char.MinValue);
                instances = tempstring.Split(char.MinValue).ToList();

                Marshal.FreeHGlobal(buffer);
            }

            return instances;
        }

        public bool SetWhitelist(List<string> instances)
        {
            bool result = false;
            int bytesReturned = 0;
            IntPtr inBuffer =
                StringListToMultiSzPointer(instances, out int inBufferLength);

            result = NativeMethods.DeviceIoControl(hidHideHandle.DangerousGetHandle(),
                IOCTL_SET_WHITELIST,
                inBuffer,
                inBufferLength,
                IntPtr.Zero,
                0,
                ref bytesReturned,
                IntPtr.Zero);

            //int error = Marshal.GetLastWin32Error();
            // Free buffer returned from StringListToMultiSzPointer
            Marshal.FreeHGlobal(inBuffer);

            return result;
        }

        public bool IsOpen()
        {
            return hidHideHandle != null && (!hidHideHandle.IsClosed && !hidHideHandle.IsInvalid);
        }

        public void Close()
        {
            if (IsOpen())
            {
                hidHideHandle.Close();
                hidHideHandle.Dispose();
                hidHideHandle = null;
            }
        }

        public void Dispose()
        {
            Close();
        }

        private IntPtr StringListToMultiSzPointer(List<string> strList,
            out int length)
        {
            // Temporary byte list
            IEnumerable<byte> multiSz = new List<byte>();

            // Convert each string into wide multi-byte and add NULL-terminator in between
            multiSz = strList.Aggregate(multiSz,
                (current, entry) =>
                {
                    return current.Concat(Encoding.Unicode.GetBytes(entry))
                                    .Concat(Encoding.Unicode.GetBytes(new[] { char.MinValue }));
                });

            // Add another NULL-terminator to signal end of list
            multiSz = multiSz.Concat(Encoding.Unicode.GetBytes(new[] { char.MinValue }));

            // Convert list to array
            byte[] multiSzArray = multiSz.ToArray();

            // Copy array content to allocated buffer
            length = multiSzArray.Length;
            IntPtr buffer = Marshal.AllocHGlobal(length);
            Marshal.Copy(multiSzArray, 0, buffer, length);

            // Return IntPtr to caller. Caller MUST free data when finsihed with it
            return buffer;
        }
    }
}
