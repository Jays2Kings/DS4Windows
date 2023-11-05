/*
DS4Windows
Copyright (C) 2023  Travis Nickles

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/

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
        private const uint IOCTL_GET_WL_INVERT = 0x80016018;
        private const uint IOCTL_SET_WL_INVERT = 0x8001601C;

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

        public bool GetWhiteListInverseState()
        {
            bool result = false;

            unsafe
            {
                int bytesReturned = 0;
                NativeMethods.DeviceIoControl(hidHideHandle.DangerousGetHandle(),
                    HidHideAPIDevice.IOCTL_GET_WL_INVERT,
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

        public bool SetWhitelistInverseState(bool state)
        {
            bool result = false;

            unsafe
            {
                int bytesReturned = 0;
                NativeMethods.DeviceIoControl(hidHideHandle.DangerousGetHandle(),
                    HidHideAPIDevice.IOCTL_SET_WL_INVERT,
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

            // Return IntPtr to caller. Caller MUST free data when finished with it
            return buffer;
        }
    }
}
