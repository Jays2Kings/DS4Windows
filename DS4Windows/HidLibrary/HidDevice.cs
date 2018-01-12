using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Win32.SafeHandles;
namespace DS4Windows
{
    public class HidDevice : IDisposable
    {
        public enum ReadStatus
        {
            Success = 0,
            WaitTimedOut = 1,
            WaitFail = 2,
            NoDataRead = 3,
            ReadError = 4,
            NotConnected = 5
        }

        private readonly string _description;
        private readonly string _devicePath;
        private readonly HidDeviceAttributes _deviceAttributes;

        private readonly HidDeviceCapabilities _deviceCapabilities;
        private bool _monitorDeviceEvents;
        private string serial = null;
        internal HidDevice(string devicePath, string description = null)
        {
            _devicePath = devicePath;
            _description = description;

            try
            {
                var hidHandle = OpenHandle(_devicePath, false);

                _deviceAttributes = GetDeviceAttributes(hidHandle);
                _deviceCapabilities = GetDeviceCapabilities(hidHandle);

                hidHandle.Close();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
                throw new Exception(string.Format("Error querying HID device '{0}'.", devicePath), exception);
            }
        }

        public SafeFileHandle safeReadHandle { get; private set; }
        public FileStream fileStream { get; private set; }
        public bool IsOpen { get; private set; }
        public bool IsExclusive { get; private set; }
        public bool IsConnected { get { return HidDevices.IsConnected(_devicePath); } }
        public string Description { get { return _description; } }
        public HidDeviceCapabilities Capabilities { get { return _deviceCapabilities; } }
        public HidDeviceAttributes Attributes { get { return _deviceAttributes; } }
        public string DevicePath { get { return _devicePath; } }

        public override string ToString()
        {
            return string.Format("VendorID={0}, ProductID={1}, Version={2}, DevicePath={3}",
                                _deviceAttributes.VendorHexId,
                                _deviceAttributes.ProductHexId,
                                _deviceAttributes.Version,
                                _devicePath);
        }

        public void OpenDevice(bool isExclusive)
        {
            if (IsOpen) return;
            try
            {
                if (safeReadHandle == null || safeReadHandle.IsInvalid)
                    safeReadHandle = OpenHandle(_devicePath, isExclusive);
            }
            catch (Exception exception)
            {
                IsOpen = false;
                throw new Exception("Error opening HID device.", exception);
            }

            IsOpen = !safeReadHandle.IsInvalid;
            IsExclusive = isExclusive;
        }

        public void OpenFileStream(int reportSize)
        {
            if (fileStream == null && !safeReadHandle.IsInvalid)
            {
                fileStream = new FileStream(safeReadHandle, FileAccess.ReadWrite, reportSize, true);
            }
        }

        public bool IsFileStreamOpen()
        {
            bool result = false;
            if (fileStream != null)
            {
                result = !fileStream.SafeFileHandle.IsInvalid && !fileStream.SafeFileHandle.IsClosed;
            }

            return result;
        }

        public void CloseDevice()
        {
            if (!IsOpen) return;
            closeFileStreamIO();

            IsOpen = false;
        }

        public void Dispose()
        {
            CancelIO();
            CloseDevice();
        }

        public void CancelIO()
        {
            if (IsOpen)
                NativeMethods.CancelIoEx(safeReadHandle.DangerousGetHandle(), IntPtr.Zero);
        }

        public bool ReadInputReport(byte[] data)
        {
            if (safeReadHandle == null)
                safeReadHandle = OpenHandle(_devicePath, true);
            return NativeMethods.HidD_GetInputReport(safeReadHandle, data, data.Length);
        }

        public bool WriteFeatureReport(byte[] data)
        {
            bool result = false;
            if (IsOpen && safeReadHandle != null)
            {
                result = NativeMethods.HidD_SetFeature(safeReadHandle, data, data.Length);
            }

            return result;
        }


        private static HidDeviceAttributes GetDeviceAttributes(SafeFileHandle hidHandle)
        {
            var deviceAttributes = default(NativeMethods.HIDD_ATTRIBUTES);
            deviceAttributes.Size = Marshal.SizeOf(deviceAttributes);
            NativeMethods.HidD_GetAttributes(hidHandle.DangerousGetHandle(), ref deviceAttributes);
            return new HidDeviceAttributes(deviceAttributes);
        }

        private static HidDeviceCapabilities GetDeviceCapabilities(SafeFileHandle hidHandle)
        {
            var capabilities = default(NativeMethods.HIDP_CAPS);
            var preparsedDataPointer = default(IntPtr);

            if (NativeMethods.HidD_GetPreparsedData(hidHandle.DangerousGetHandle(), ref preparsedDataPointer))
            {
                NativeMethods.HidP_GetCaps(preparsedDataPointer, ref capabilities);
                NativeMethods.HidD_FreePreparsedData(preparsedDataPointer);
            }

            return new HidDeviceCapabilities(capabilities);
        }

        private void closeFileStreamIO()
        {
            if (fileStream != null)
            {
                try
                {
                    fileStream.Close();
                }
                catch (IOException) { }
                catch (OperationCanceledException) { }
            }

            fileStream = null;
            Console.WriteLine("Close fs");
            if (safeReadHandle != null && !safeReadHandle.IsInvalid)
            {
                try
                {
                    if (!safeReadHandle.IsClosed)
                    {
                        safeReadHandle.Close();
                        Console.WriteLine("Close sh");
                    }
                }
                catch (IOException) { }
            }

            safeReadHandle = null;
        }

        public void flush_Queue()
        {
            if (safeReadHandle != null)
            {
                NativeMethods.HidD_FlushQueue(safeReadHandle);
            }
        }

        private ReadStatus ReadWithFileStreamTask(byte[] inputBuffer)
        {
            try
            {
                if (fileStream.Read(inputBuffer, 0, inputBuffer.Length) > 0)
                {
                    return ReadStatus.Success;
                }
                else
                {
                    return ReadStatus.NoDataRead;
                }
            }
            catch (Exception)
            {
                return ReadStatus.ReadError;
            }
        }

        public ReadStatus ReadFile(byte[] inputBuffer)
        {
            if (safeReadHandle == null)
                safeReadHandle = OpenHandle(_devicePath, true);
            try
            {
                uint bytesRead;
                if (NativeMethods.ReadFile(safeReadHandle.DangerousGetHandle(), inputBuffer, (uint)inputBuffer.Length, out bytesRead, IntPtr.Zero))
                {
                    return ReadStatus.Success;
                }
                else
                {
                    return ReadStatus.NoDataRead;
                }
            }
            catch (Exception)
            {
                return ReadStatus.ReadError;
            }
        }

        public ReadStatus ReadWithFileStream(byte[] inputBuffer)
        {
            try
            {
                if (fileStream.Read(inputBuffer, 0, inputBuffer.Length) > 0)
                {
                    return ReadStatus.Success;
                }
                else
                {
                    return ReadStatus.NoDataRead;
                }
            }
            catch (Exception)
            {
                return ReadStatus.ReadError;
            }
        }

        public ReadStatus ReadWithFileStream(byte[] inputBuffer, int timeout)
        {
            try
            {
                if (safeReadHandle == null)
                    safeReadHandle = OpenHandle(_devicePath, true);
                if (fileStream == null && !safeReadHandle.IsInvalid)
                    fileStream = new FileStream(safeReadHandle, FileAccess.ReadWrite, inputBuffer.Length, true);

                if (!safeReadHandle.IsInvalid && fileStream.CanRead)
                {
                    Task<ReadStatus> readFileTask = new Task<ReadStatus>(() => ReadWithFileStreamTask(inputBuffer));
                    readFileTask.Start();
                    bool success = readFileTask.Wait(timeout);
                    if (success)
                    {
                        if (readFileTask.Result == ReadStatus.Success)
                        {
                            return ReadStatus.Success;
                        }
                        else if (readFileTask.Result == ReadStatus.ReadError)
                        {
                            return ReadStatus.ReadError;
                        }
                        else if (readFileTask.Result == ReadStatus.NoDataRead)
                        {
                            return ReadStatus.NoDataRead;
                        }
                    }
                    else
                        return ReadStatus.WaitTimedOut;
                }

            }
            catch (Exception e)
            {
                if (e is AggregateException)
                {
                    Console.WriteLine(e.Message);
                    return ReadStatus.WaitFail;
                }
                else
                {
                    return ReadStatus.ReadError;
                }
            }

            return ReadStatus.ReadError;
        }

        public ReadStatus ReadAsyncWithFileStream(byte[] inputBuffer, int timeout)
        {
            try
            {
                if (safeReadHandle == null)
                    safeReadHandle = OpenHandle(_devicePath, true);
                if (fileStream == null && !safeReadHandle.IsInvalid)
                    fileStream = new FileStream(safeReadHandle, FileAccess.ReadWrite, inputBuffer.Length, true);

                if (!safeReadHandle.IsInvalid && fileStream.CanRead)
                {
                    Task<int> readTask = fileStream.ReadAsync(inputBuffer, 0, inputBuffer.Length);
                    bool success = readTask.Wait(timeout);
                    if (success)
                    {
                        if (readTask.Result > 0)
                        {
                            return ReadStatus.Success;
                        }
                        else
                        {
                            return ReadStatus.NoDataRead;
                        }
                    }
                    else
                    {
                        return ReadStatus.WaitTimedOut;
                    }
                }

            }
            catch (Exception e)
            {
                if (e is AggregateException)
                {
                    Console.WriteLine(e.Message);
                    return ReadStatus.WaitFail;
                }
                else
                {
                    return ReadStatus.ReadError;
                }
            }

            return ReadStatus.ReadError;
        }

        public bool WriteOutputReportViaControl(byte[] outputBuffer)
        {
            if (safeReadHandle == null)
            {
                safeReadHandle = OpenHandle(_devicePath, true);
            }

            if (NativeMethods.HidD_SetOutputReport(safeReadHandle, outputBuffer, outputBuffer.Length))
                return true;
            else
                return false;
        }

        private bool WriteOutputReportViaInterruptTask(byte[] outputBuffer)
        {
            try
            {
                fileStream.Write(outputBuffer, 0, outputBuffer.Length);
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
        }

        public bool WriteOutputReportViaInterrupt(byte[] outputBuffer, int timeout)
        {
            try
            {
                if (safeReadHandle == null)
                {
                    safeReadHandle = OpenHandle(_devicePath, true);
                }
                if (fileStream == null && !safeReadHandle.IsInvalid)
                {
                    fileStream = new FileStream(safeReadHandle, FileAccess.ReadWrite, outputBuffer.Length, true);
                }
                if (fileStream != null && fileStream.CanWrite && !safeReadHandle.IsInvalid)
                {
                    fileStream.Write(outputBuffer, 0, outputBuffer.Length);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception)
            {
                return false;
            }

        }

        public bool WriteAsyncOutputReportViaInterrupt(byte[] outputBuffer)
        {
            try
            {
                if (safeReadHandle == null)
                {
                    safeReadHandle = OpenHandle(_devicePath, true);
                }
                if (fileStream == null && !safeReadHandle.IsInvalid)
                {
                    fileStream = new FileStream(safeReadHandle, FileAccess.ReadWrite, outputBuffer.Length, true);
                }
                if (fileStream != null && fileStream.CanWrite && !safeReadHandle.IsInvalid)
                {
                    Task writeTask = fileStream.WriteAsync(outputBuffer, 0, outputBuffer.Length);
                    //fileStream.Write(outputBuffer, 0, outputBuffer.Length);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception)
            {
                return false;
            }

        }

        private SafeFileHandle OpenHandle(String devicePathName, Boolean isExclusive)
        {
            SafeFileHandle hidHandle;

            if (isExclusive)
            {
                hidHandle = NativeMethods.CreateFile(devicePathName, NativeMethods.GENERIC_READ | NativeMethods.GENERIC_WRITE, 0, IntPtr.Zero, NativeMethods.OpenExisting, 0x20000000 | 0x80000000 | NativeMethods.FILE_FLAG_OVERLAPPED, 0);
            }
            else
            {
                hidHandle = NativeMethods.CreateFile(devicePathName, NativeMethods.GENERIC_READ | NativeMethods.GENERIC_WRITE, NativeMethods.FILE_SHARE_READ | NativeMethods.FILE_SHARE_WRITE, IntPtr.Zero, NativeMethods.OpenExisting, 0x20000000 | 0x80000000 | NativeMethods.FILE_FLAG_OVERLAPPED, 0);
            }

            return hidHandle;
        }

        public bool readFeatureData(byte[] inputBuffer)
        {
            return NativeMethods.HidD_GetFeature(safeReadHandle.DangerousGetHandle(), inputBuffer, inputBuffer.Length);
        }

        public void resetSerial()
        {
            serial = null;
        }

        public string readSerial()
        {
            if (serial != null)
                return serial;

            if (Capabilities.InputReportByteLength == 64)
            {
                byte[] buffer = new byte[16];
                buffer[0] = 18;
                readFeatureData(buffer);                
                serial =  String.Format("{0:X02}:{1:X02}:{2:X02}:{3:X02}:{4:X02}:{5:X02}", buffer[6], buffer[5], buffer[4], buffer[3], buffer[2], buffer[1]);
                return serial;
            }
            else
            {
                byte[] buffer = new byte[126];
#if WIN64
                ulong bufferLen = 126;
#else
                uint bufferLen = 126;
#endif
                NativeMethods.HidD_GetSerialNumberString(safeReadHandle.DangerousGetHandle(), buffer, bufferLen);
                string MACAddr = System.Text.Encoding.Unicode.GetString(buffer).Replace("\0", string.Empty).ToUpper();
                MACAddr = $"{MACAddr[0]}{MACAddr[1]}:{MACAddr[2]}{MACAddr[3]}:{MACAddr[4]}{MACAddr[5]}:{MACAddr[6]}{MACAddr[7]}:{MACAddr[8]}{MACAddr[9]}:{MACAddr[10]}{MACAddr[11]}";
                serial = MACAddr;
                return serial;
            }
        }
    }
}
