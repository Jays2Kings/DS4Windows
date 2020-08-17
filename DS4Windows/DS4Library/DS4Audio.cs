using System;
using System.Runtime.InteropServices;
using DS4Windows.DS4Library.CoreAudio;
using System.Text.RegularExpressions;

namespace DS4Windows.DS4Library
{
    public class DS4Audio : IAudioEndpointVolumeCallback
    {
        private IAudioEndpointVolume endpointVolume;
    
        private static Guid IID_IAudioEndpointVolume = new Guid("5CDF2C82-841E-4546-9722-0CF74078229A");
        private static readonly PropertyKey PKEY_Device_FriendlyName =
            new PropertyKey(new Guid(unchecked((int)0xa45c254e), unchecked((short)0xdf1c), 0x4efd, 0x80, 0x20, 0x67, 0xd1, 0x46, 0xa8, 0x50, 0xe0), 14);

        private static readonly Guid MediaGuid = new Guid("{4d36e96c-e325-11ce-bfc1-08002be10318}");

        private static readonly PropertyKey PKEY_Device_Siblings =
            new PropertyKey(new Guid("{b3f8fa53-0004-438e-9003-51a46e139bfc}"), 2);

        public uint vol;
        public uint Volume
        {
            get
            {
                return vol;
            }
        }

        public uint getVolume()
        {
            return vol;
        }

        private DataFlow instAudioFlags = DataFlow.Render;
        private Regex regex = new Regex(@"^\{\d+\}\.{1}(?<parent>(?:\S+)*\\VID_(?:[0-9]|[A-F])+&PID_(?:[0-9A-F]+)(?:\S+)){1}$",
            RegexOptions.Compiled);

        public void RefreshVolume()
        {
            const float HALFPI = (float)Math.PI / 2.0f;
            const int MAX_RENDER_VOLUME_LVL = 80;
            const int MIN_RENDER_VOLUME_LVL = 40;
            const int MAX_CAPTURE_VOLUME_LVL = 60;
            const int MIN_CAPTURE_VOLUME_LVL = 0;
            float pfLevel = 0;

            if (endpointVolume != null)
                endpointVolume.GetMasterVolumeLevelScalar(out pfLevel);

            if (instAudioFlags == DataFlow.Render)
                // Use QuadraticEaseOut curve for headphone volume level
                //vol = pfLevel != 0.0 ? Convert.ToUInt32((MAX_RENDER_VOLUME_LVL - MIN_RENDER_VOLUME_LVL) * -(pfLevel * (pfLevel - 2.0)) + MIN_RENDER_VOLUME_LVL) : 0;
                // Use SineEaseOut curve for headphone volume level
                vol = pfLevel != 0.0 ? Convert.ToUInt32((MAX_RENDER_VOLUME_LVL - MIN_RENDER_VOLUME_LVL) * Math.Sin(pfLevel * HALFPI) + MIN_RENDER_VOLUME_LVL) : 0;
                // Use CubicEaseOut curve for headphone volume level
                //vol = pfLevel != 0.0 ?  Convert.ToUInt32((MAX_RENDER_VOLUME_LVL - MIN_RENDER_VOLUME_LVL) * (--pfLevel * pfLevel * pfLevel + 1) + MIN_RENDER_VOLUME_LVL) : 0;
                // Use Linear curve for headphone volume level
                //vol = pfLevel != 0.0 ? Convert.ToUInt32((MAX_RENDER_VOLUME_LVL - MIN_RENDER_VOLUME_LVL) * pfLevel + MIN_RENDER_VOLUME_LVL) : 0;
            else if (instAudioFlags == DataFlow.Capture)
                vol = Convert.ToUInt32((MAX_CAPTURE_VOLUME_LVL - MIN_CAPTURE_VOLUME_LVL) * pfLevel + 0);
        }

        public void OnNotify(IntPtr pNotify)
        {
            RefreshVolume();
        }

        public DS4Audio(DataFlow audioFlags = DataFlow.Render,
            string searchDeviceInstance = "")
        {
            var audioEnumerator = new MMDeviceEnumeratorComObject() as IMMDeviceEnumerator;
            IMMDeviceCollection audioDevices;
            audioEnumerator.EnumAudioEndpoints(audioFlags, DeviceState.Active, out audioDevices);

            int numAudioDevices;
            Marshal.ThrowExceptionForHR(audioDevices.GetCount(out numAudioDevices));

            for (int deviceNumber = 0; deviceNumber < numAudioDevices; ++deviceNumber)
            {
                IMMDevice audioDevice;
                Marshal.ThrowExceptionForHR(audioDevices.Item(deviceNumber, out audioDevice));
                string deviceName = GetAudioDeviceName(ref audioDevice);

                if (deviceName == searchDeviceInstance)
                {
                    object interfacePointer;
                    Marshal.ThrowExceptionForHR(audioDevice.Activate(ref IID_IAudioEndpointVolume, ClsCtx.ALL, IntPtr.Zero, out interfacePointer));
                    instAudioFlags = audioFlags;
                    endpointVolume = interfacePointer as IAudioEndpointVolume;
                    endpointVolume.RegisterControlChangeNotify(this);
                    RefreshVolume();
                }

                Marshal.ReleaseComObject(audioDevice);
            }

            Marshal.ReleaseComObject(audioDevices);
            Marshal.ReleaseComObject(audioEnumerator);
        }

        ~DS4Audio()
        {
            if (endpointVolume != null)
            {
                endpointVolume.UnregisterControlChangeNotify(this);
                Marshal.ReleaseComObject(endpointVolume);
                endpointVolume = null;
            }
        }

        private string GetAudioDeviceName(ref IMMDevice audioDevice)
        {
            IPropertyStore propertyStore;
            Marshal.ThrowExceptionForHR(audioDevice.OpenPropertyStore(StorageAccessMode.Read, out propertyStore));

            int numProperties;
            Marshal.ThrowExceptionForHR(propertyStore.GetCount(out numProperties));

            string tempDeviceInstance = string.Empty;
            string devicePath = string.Empty;

            for (int propertyNum = 0; propertyNum < numProperties; ++propertyNum)
            {
                PropertyKey propertyKey;
                Marshal.ThrowExceptionForHR(propertyStore.GetAt(propertyNum, out propertyKey));

                if ((propertyKey.formatId == PKEY_Device_Siblings.formatId) && (propertyKey.propertyId == PKEY_Device_Siblings.propertyId))
                {
                    PropVariant propertyValue;
                    Marshal.ThrowExceptionForHR(propertyStore.GetValue(ref propertyKey, out propertyValue));
                    tempDeviceInstance = Marshal.PtrToStringUni(propertyValue.innerData.pointerValue) ?? "";

                    if (regex.IsMatch(tempDeviceInstance))
                    {
                        MatchCollection collection = regex.Matches(tempDeviceInstance);
                        GroupCollection groupCollection = collection[0].Groups;
                        devicePath = groupCollection["parent"].Value;
                        devicePath = GetSibling(devicePath);
                    }
                    else
                    {
                        devicePath = "";
                    }

                    break;
                }
            }

            Marshal.ReleaseComObject(propertyStore);
            return devicePath;
        }

        private string GetSibling(string devicePath)
        {
            string result = string.Empty;

            var requiredSize = 0;
            ulong propertyType = 0;

            Guid tempGuid = MediaGuid;
            IntPtr deviceInfoSet = NativeMethods.SetupDiGetClassDevs(ref tempGuid, null, 0, NativeMethods.DIGCF_PRESENT);

            NativeMethods.SP_DEVINFO_DATA devinfoData = new NativeMethods.SP_DEVINFO_DATA();
            devinfoData.cbSize = Marshal.SizeOf(devinfoData);

            var deviceIndex = 0;
            while (NativeMethods.SetupDiEnumDeviceInfo(deviceInfoSet, deviceIndex, ref devinfoData))
            {
                deviceIndex += 1;
                string tmpDeviceInst = string.Empty;

                NativeMethods.SetupDiGetDeviceProperty(deviceInfoSet, ref devinfoData,
                                                    ref NativeMethods.DEVPKEY_Device_InstanceId, ref propertyType,
                                                    null, 0,
                                                    ref requiredSize, 0);

                if (requiredSize > 0)
                {
                    var descriptionBuffer = new byte[requiredSize];
                    NativeMethods.SetupDiGetDeviceProperty(deviceInfoSet, ref devinfoData,
                                                            ref NativeMethods.DEVPKEY_Device_InstanceId, ref propertyType,
                                                            descriptionBuffer, descriptionBuffer.Length,
                                                            ref requiredSize, 0);

                    tmpDeviceInst = System.Text.Encoding.Unicode.GetString(descriptionBuffer);
                    if (tmpDeviceInst.EndsWith("\0"))
                    {
                        tmpDeviceInst = tmpDeviceInst.Remove(tmpDeviceInst.Length - 1);
                    }
                }

                if (tmpDeviceInst == devicePath)
                {
                    string tmp = string.Empty;
                    NativeMethods.SetupDiGetDeviceProperty(deviceInfoSet, ref devinfoData,
                        ref NativeMethods.DEVPKEY_Device_Siblings, ref propertyType,
                        null, 0,
                        ref requiredSize, 0);

                    if (requiredSize > 0)
                    {
                        var descriptionBuffer = new byte[requiredSize];
                        NativeMethods.SetupDiGetDeviceProperty(deviceInfoSet, ref devinfoData,
                            ref NativeMethods.DEVPKEY_Device_Siblings, ref propertyType,
                            descriptionBuffer, descriptionBuffer.Length,
                            ref requiredSize, 0);

                        tmp = System.Text.Encoding.Unicode.GetString(descriptionBuffer);
                        tmp = tmp.Remove(tmp.IndexOf("\0\0"));
                        result = tmp;
                        break;
                     }
                }
            }

            NativeMethods.SetupDiDestroyDeviceInfoList(deviceInfoSet);
            return result;
         }
    }
}

namespace DS4Windows.DS4Library.CoreAudio
{
    public enum DataFlow
    {
        Render,
        Capture,
        All
    };

    [Flags]
    public enum DeviceState
    {
        Active = 0x00000001,
        Disabled = 0x00000002,
        NotPresent = 0x00000004,
        Unplugged = 0x00000008,
        All = 0x0000000F
    }

    enum StorageAccessMode
    {
        Read,
        Write,
        ReadWrite
    }

    [Flags]
    public enum ClsCtx
    {
        INPROC_SERVER = 0x1,
        INPROC_HANDLER = 0x2,
        LOCAL_SERVER = 0x4,
        INPROC_SERVER16 = 0x8,
        REMOTE_SERVER = 0x10,
        INPROC_HANDLER16 = 0x20,
        NO_CODE_DOWNLOAD = 0x400,
        NO_CUSTOM_MARSHAL = 0x1000,
        ENABLE_CODE_DOWNLOAD = 0x2000,
        NO_FAILURE_LOG = 0x4000,
        DISABLE_AAA = 0x8000,
        ENABLE_AAA = 0x10000,
        FROM_DEFAULT_CONTEXT = 0x20000,
        ACTIVATE_32_BIT_SERVER = 0x40000,
        ACTIVATE_64_BIT_SERVER = 0x80000,
        ENABLE_CLOAKING = 0x100000,
        PS_DLL = unchecked((int)0x80000000),
        INPROC = INPROC_SERVER | INPROC_HANDLER,
        SERVER = INPROC_SERVER | LOCAL_SERVER | REMOTE_SERVER,
        ALL = SERVER | INPROC_HANDLER
    }

    public struct PropertyKey
    {
        public Guid formatId;
        public int propertyId;
        public PropertyKey(Guid formatId, int propertyId)
        {
            this.formatId = formatId;
            this.propertyId = propertyId;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct PropVariant
    {
        public short vt;
        public short wReserved1;
        public short wReserved2;
        public short wReserved3;
        public PropVariantData innerData;
    }

    // Size 16 is the minimum required size to work in 64 bit mode. Size has to be
    // be explicitly specified for 64 bit. Still works in 32 bit mode
    [StructLayout(LayoutKind.Explicit, CharSet = CharSet.Unicode, Size = 16)]
    public struct PropVariantData
    {
        [FieldOffset(0)] public IntPtr pointerValue;
    }

    [Guid("886d8eeb-8cf2-4446-8d02-cdba1dbdcf99"),
       InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    interface IPropertyStore
    {
        int GetCount(out int propCount);
        int GetAt(int property, out PropertyKey key);
        int GetValue(ref PropertyKey key, out PropVariant value);
        int SetValue(ref PropertyKey key, ref PropVariant value);
        int Commit();
    }

    [Guid("D666063F-1587-4E43-81F1-B948E807363F"),
        InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    interface IMMDevice
    {
        int Activate(ref Guid id, ClsCtx clsCtx, IntPtr activationParams,
            [MarshalAs(UnmanagedType.IUnknown)] out object interfacePointer);

        int OpenPropertyStore(StorageAccessMode stgmAccess, out IPropertyStore properties);

        int GetId([MarshalAs(UnmanagedType.LPWStr)] out string id);
    }

    [Guid("0BD7A1BE-7A1A-44DB-8397-CC5392387B5E"),
        InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    interface IMMDeviceCollection
    {
        int GetCount(out int numDevices);
        int Item(int deviceNumber, out IMMDevice device);
    }

    [Guid("A95664D2-9614-4F35-A746-DE8DB63617E6"), 
        InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    interface IMMDeviceEnumerator
    {
        int EnumAudioEndpoints(DataFlow dataFlow, DeviceState stateMask, out IMMDeviceCollection devices);
    }

    [ComImport, Guid("BCDE0395-E52F-467C-8E3D-C4579291692E")]
    class MMDeviceEnumeratorComObject
    {
    }

    [Guid("657804FA-D6AD-4496-8A60-352752AF4F89"),
        InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IAudioEndpointVolumeCallback
    {
        void OnNotify(IntPtr notifyData);
    };

    [Guid("5CDF2C82-841E-4546-9722-0CF74078229A"),
        InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IAudioEndpointVolume
    {
        int RegisterControlChangeNotify(IAudioEndpointVolumeCallback pNotify);
        int UnregisterControlChangeNotify(IAudioEndpointVolumeCallback pNotify);
        int GetChannelCount(out int pnChannelCount);
        int SetMasterVolumeLevel(float fLevelDB, ref Guid pguidEventContext);
        int SetMasterVolumeLevelScalar(float fLevel, ref Guid pguidEventContext);
        int GetMasterVolumeLevel(out float pfLevelDB);
        int GetMasterVolumeLevelScalar(out float pfLevel);
        int SetChannelVolumeLevel(uint nChannel, float fLevelDB, ref Guid pguidEventContext);
        int SetChannelVolumeLevelScalar(uint nChannel, float fLevel, ref Guid pguidEventContext);
        int GetChannelVolumeLevel(uint nChannel, out float pfLevelDB);
        int GetChannelVolumeLevelScalar(uint nChannel, out float pfLevel);
        int SetMute([MarshalAs(UnmanagedType.Bool)] Boolean bMute, ref Guid pguidEventContext);
        int GetMute(out bool pbMute);
        int GetVolumeStepInfo(out uint pnStep, out uint pnStepCount);
        int VolumeStepUp(ref Guid pguidEventContext);
        int VolumeStepDown(ref Guid pguidEventContext);
        int QueryHardwareSupport(out uint pdwHardwareSupportMask);
        int GetVolumeRange(out float pflVolumeMindB, out float pflVolumeMaxdB, out float pflVolumeIncrementdB);
    }
}