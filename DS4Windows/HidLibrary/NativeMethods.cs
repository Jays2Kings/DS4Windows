using System;
using System.Runtime.InteropServices;
using System.Security;
using Microsoft.Win32.SafeHandles; 
namespace DS4Windows
{
    [SuppressUnmanagedCodeSecurity]
    internal static class NativeMethods
    {
        [StructLayout(LayoutKind.Sequential)]
        internal struct BLUETOOTH_FIND_RADIO_PARAMS
        {
            [MarshalAs(UnmanagedType.U4)]
            public int dwSize;
        }

        [DllImport("bthprops.cpl", CharSet = CharSet.Auto)]
        internal extern static IntPtr BluetoothFindFirstRadio(ref BLUETOOTH_FIND_RADIO_PARAMS pbtfrp, ref IntPtr phRadio);

        [DllImport("bthprops.cpl", CharSet = CharSet.Auto)]
        internal extern static bool BluetoothFindNextRadio(IntPtr hFind, ref IntPtr phRadio);

        [DllImport("bthprops.cpl", CharSet = CharSet.Auto)]
        internal extern static bool BluetoothFindRadioClose(IntPtr hFind);

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern Boolean DeviceIoControl(IntPtr DeviceHandle, Int32 IoControlCode, ref long InBuffer, Int32 InBufferSize, IntPtr OutBuffer, Int32 OutBufferSize, ref Int32 BytesReturned, IntPtr Overlapped);

        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true, CharSet = CharSet.Auto)]
        internal static extern bool CloseHandle(IntPtr hObject);

	    internal const int FILE_FLAG_OVERLAPPED = 0x40000000;
	    internal const uint FILE_FLAG_NO_BUFFERING = 0x20000000;
	    internal const uint FILE_FLAG_WRITE_THROUGH = 0x80000000;
	    internal const uint FILE_ATTRIBUTE_TEMPORARY = 0x100;

	    internal const short FILE_SHARE_READ = 0x1;
	    internal const short FILE_SHARE_WRITE = 0x2;
	    internal const uint GENERIC_READ = 0x80000000;
	    internal const uint GENERIC_WRITE = 0x40000000;
        internal const Int32 FileShareRead = 1;
        internal const Int32 FileShareWrite = 2;
        internal const Int32 OpenExisting = 3;
	    internal const int ACCESS_NONE = 0;
	    internal const int INVALID_HANDLE_VALUE = -1;
	    internal const short OPEN_EXISTING = 3;
	    internal const int WAIT_TIMEOUT = 0x102;
	    internal const uint WAIT_OBJECT_0 = 0;
	    internal const uint WAIT_FAILED = 0xffffffff;

	    internal const int WAIT_INFINITE = 0xffff;
	    [StructLayout(LayoutKind.Sequential)]
	    internal struct OVERLAPPED
	    {
		    public int Internal;
		    public int InternalHigh;
		    public int Offset;
		    public int OffsetHigh;
		    public int hEvent;
	    }

	    [StructLayout(LayoutKind.Sequential)]
	    internal struct SECURITY_ATTRIBUTES
	    {
		    public int nLength;
		    public IntPtr lpSecurityDescriptor;
		    public bool bInheritHandle;
	    }

        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true, CharSet = CharSet.Auto)]
        static internal extern bool CancelIo(IntPtr hFile);

        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true, CharSet = CharSet.Auto)]
        static internal extern bool CancelIoEx(IntPtr hFile, IntPtr lpOverlapped);

        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true, CharSet = CharSet.Auto)]
        static internal extern bool CancelSynchronousIo(IntPtr hObject);

	    [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
	    static internal extern IntPtr CreateEvent(ref SECURITY_ATTRIBUTES securityAttributes, int bManualReset, int bInitialState, string lpName);

	    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	    static internal extern IntPtr CreateFile(string lpFileName, uint dwDesiredAccess, int dwShareMode, ref SECURITY_ATTRIBUTES lpSecurityAttributes, int dwCreationDisposition, uint dwFlagsAndAttributes, int hTemplateFile);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern SafeFileHandle CreateFile(String lpFileName, UInt32 dwDesiredAccess, Int32 dwShareMode, IntPtr lpSecurityAttributes, Int32 dwCreationDisposition, UInt32 dwFlagsAndAttributes, Int32 hTemplateFile);
        [DllImport("kernel32.dll", SetLastError = true)]
        static internal extern bool ReadFile(IntPtr hFile, [Out] byte[] lpBuffer, uint nNumberOfBytesToRead, out uint lpNumberOfBytesRead, IntPtr lpOverlapped);

	    [DllImport("kernel32.dll")]
	    static internal extern uint WaitForSingleObject(IntPtr hHandle, int dwMilliseconds);

        [DllImport("kernel32.dll")]
        static internal extern bool WriteFile(IntPtr hFile, byte[] lpBuffer, uint nNumberOfBytesToWrite, out uint lpNumberOfBytesWritten, [In] ref System.Threading.NativeOverlapped lpOverlapped);

	    internal const int DBT_DEVICEARRIVAL = 0x8000;
	    internal const int DBT_DEVICEREMOVECOMPLETE = 0x8004;
	    internal const int DBT_DEVTYP_DEVICEINTERFACE = 5;
	    internal const int DBT_DEVTYP_HANDLE = 6;
	    internal const int DEVICE_NOTIFY_ALL_INTERFACE_CLASSES = 4;
	    internal const int DEVICE_NOTIFY_SERVICE_HANDLE = 1;
	    internal const int DEVICE_NOTIFY_WINDOW_HANDLE = 0;
	    internal const int WM_DEVICECHANGE = 0x219;
	    internal const short DIGCF_PRESENT = 0x2;
	    internal const short DIGCF_DEVICEINTERFACE = 0x10;
	    internal const int DIGCF_ALLCLASSES = 0x4;
        internal const int DICS_ENABLE = 1;
        internal const int DICS_DISABLE = 2;
        internal const int DICS_FLAG_GLOBAL = 1;
        internal const int DIF_PROPERTYCHANGE = 0x12;

        internal const int MAX_DEV_LEN = 1000;
	    internal const int SPDRP_ADDRESS = 0x1c;
	    internal const int SPDRP_BUSNUMBER = 0x15;
	    internal const int SPDRP_BUSTYPEGUID = 0x13;
	    internal const int SPDRP_CAPABILITIES = 0xf;
	    internal const int SPDRP_CHARACTERISTICS = 0x1b;
	    internal const int SPDRP_CLASS = 7;
	    internal const int SPDRP_CLASSGUID = 8;
	    internal const int SPDRP_COMPATIBLEIDS = 2;
	    internal const int SPDRP_CONFIGFLAGS = 0xa;
	    internal const int SPDRP_DEVICE_POWER_DATA = 0x1e;
	    internal const int SPDRP_DEVICEDESC = 0;
	    internal const int SPDRP_DEVTYPE = 0x19;
	    internal const int SPDRP_DRIVER = 9;
	    internal const int SPDRP_ENUMERATOR_NAME = 0x16;
	    internal const int SPDRP_EXCLUSIVE = 0x1a;
	    internal const int SPDRP_FRIENDLYNAME = 0xc;
	    internal const int SPDRP_HARDWAREID = 1;
	    internal const int SPDRP_LEGACYBUSTYPE = 0x14;
	    internal const int SPDRP_LOCATION_INFORMATION = 0xd;
	    internal const int SPDRP_LOWERFILTERS = 0x12;
	    internal const int SPDRP_MFG = 0xb;
	    internal const int SPDRP_PHYSICAL_DEVICE_OBJECT_NAME = 0xe;
	    internal const int SPDRP_REMOVAL_POLICY = 0x1f;
	    internal const int SPDRP_REMOVAL_POLICY_HW_DEFAULT = 0x20;
	    internal const int SPDRP_REMOVAL_POLICY_OVERRIDE = 0x21;
	    internal const int SPDRP_SECURITY = 0x17;
	    internal const int SPDRP_SECURITY_SDS = 0x18;
	    internal const int SPDRP_SERVICE = 4;
	    internal const int SPDRP_UI_NUMBER = 0x10;
	    internal const int SPDRP_UI_NUMBER_DESC_FORMAT = 0x1d;

        internal const int SPDRP_UPPERFILTERS = 0x11;

	    [StructLayout(LayoutKind.Sequential)]
	    internal class DEV_BROADCAST_DEVICEINTERFACE
	    {
		    internal int dbcc_size;
		    internal int dbcc_devicetype;
		    internal int dbcc_reserved;
		    internal Guid dbcc_classguid;
		    internal short dbcc_name;
	    }

	    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	    internal class DEV_BROADCAST_DEVICEINTERFACE_1
	    {
		    internal int dbcc_size;
		    internal int dbcc_devicetype;
		    internal int dbcc_reserved;
		    [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 16)]
		    internal byte[] dbcc_classguid;
		    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 255)]
		    internal char[] dbcc_name;
	    }

	    [StructLayout(LayoutKind.Sequential)]
	    internal class DEV_BROADCAST_HANDLE
	    {
		    internal int dbch_size;
		    internal int dbch_devicetype;
		    internal int dbch_reserved;
		    internal int dbch_handle;
		    internal int dbch_hdevnotify;
	    }

	    [StructLayout(LayoutKind.Sequential)]
	    internal class DEV_BROADCAST_HDR
	    {
		    internal int dbch_size;
		    internal int dbch_devicetype;
		    internal int dbch_reserved;
	    }

	    [StructLayout(LayoutKind.Sequential)]
	    internal struct SP_DEVICE_INTERFACE_DATA
	    {
		    internal int cbSize;
		    internal System.Guid InterfaceClassGuid;
		    internal int Flags;
		    internal IntPtr Reserved;
	    }

	    [StructLayout(LayoutKind.Sequential)]
	    internal struct SP_DEVINFO_DATA
	    {
		    internal int cbSize;
		    internal Guid ClassGuid;
		    internal int DevInst;
		    internal IntPtr Reserved;
	    }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)]
        internal struct SP_DEVICE_INTERFACE_DETAIL_DATA
        {
            internal int Size;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
            internal string DevicePath;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct DEVPROPKEY
        {
            public Guid fmtid;
            public ulong pid;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct SP_CLASSINSTALL_HEADER
        {
            internal int cbSize;
            internal int installFunction;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct SP_PROPCHANGE_PARAMS
        {
            internal SP_CLASSINSTALL_HEADER classInstallHeader;
            internal int stateChange;
            internal int scope;
            internal int hwProfile;
        }

        internal static DEVPROPKEY DEVPKEY_Device_BusReportedDeviceDesc = 
            new DEVPROPKEY { fmtid = new Guid(0x540b947e, 0x8b40, 0x45bc, 0xa8, 0xa2, 0x6a, 0x0b, 0x89, 0x4c, 0xbd, 0xa2), pid = 4 };

        internal static DEVPROPKEY DEVPKEY_Device_HardwareIds =
            new DEVPROPKEY { fmtid = new Guid(0xa45c254e, 0xdf1c, 0x4efd, 0x80, 0x20, 0x67, 0xd1, 0x46, 0xa8, 0x50, 0xe0), pid = 3 };

        internal static DEVPROPKEY DEVPKEY_Device_UINumber =
            new DEVPROPKEY { fmtid = new Guid(0xa45c254e, 0xdf1c, 0x4efd, 0x80, 0x20, 0x67, 0xd1, 0x46, 0xa8, 0x50, 0xe0), pid = 18 };

        internal static DEVPROPKEY DEVPKEY_Device_DriverVersion =
            new DEVPROPKEY { fmtid = new Guid(0xa8b865dd, 0x2e3d, 0x4094, 0xad, 0x97, 0xe5, 0x93, 0xa7, 0xc, 0x75, 0xd6), pid = 3 };

		internal static DEVPROPKEY DEVPKEY_Device_Parent =
			new DEVPROPKEY { fmtid = new Guid(0x4340a6c5, 0x93fa, 0x4706, 0x97, 0x2c, 0x7b, 0x64, 0x80, 0x08, 0xa5, 0xa7), pid = 8 };

		internal static DEVPROPKEY DEVPKEY_Device_Siblings =
			new DEVPROPKEY { fmtid = new Guid(0x4340a6c5, 0x93fa, 0x4706, 0x97, 0x2c, 0x7b, 0x64, 0x80, 0x08, 0xa5, 0xa7), pid = 10 };

		internal static DEVPROPKEY DEVPKEY_Device_InstanceId =
			new DEVPROPKEY { fmtid = new Guid(0x78c34fc8, 0x104a, 0x4aca, 0x9e, 0xa4, 0x52, 0x4d, 0x52, 0x99, 0x6e, 0x57), pid = 256 };

		[DllImport("setupapi.dll", EntryPoint = "SetupDiGetDeviceRegistryProperty")]
        public static extern bool SetupDiGetDeviceRegistryProperty(IntPtr deviceInfoSet, ref SP_DEVINFO_DATA deviceInfoData, int propertyVal, ref int propertyRegDataType, byte[] propertyBuffer, int propertyBufferSize, ref int requiredSize);
	
        [DllImport("setupapi.dll", EntryPoint = "SetupDiGetDevicePropertyW", SetLastError = true)]
        public static extern bool SetupDiGetDeviceProperty(IntPtr deviceInfo, ref SP_DEVINFO_DATA deviceInfoData, ref DEVPROPKEY propkey, ref ulong propertyDataType, byte[] propertyBuffer, int propertyBufferSize, ref int requiredSize, uint flags);

	    [DllImport("setupapi.dll")]
	    static internal extern bool SetupDiEnumDeviceInfo(IntPtr deviceInfoSet, int memberIndex, ref SP_DEVINFO_DATA deviceInfoData);

	    [DllImport("user32.dll", CharSet = CharSet.Auto)]
	    static internal extern IntPtr RegisterDeviceNotification(IntPtr hRecipient, IntPtr notificationFilter, Int32 flags);

        [DllImport("setupapi.dll")]
        internal static extern int SetupDiCreateDeviceInfoList(ref Guid classGuid, int hwndParent);

	    [DllImport("setupapi.dll")]
	    static internal extern int SetupDiDestroyDeviceInfoList(IntPtr deviceInfoSet);

	    [DllImport("setupapi.dll")]
        static internal extern bool SetupDiEnumDeviceInterfaces(IntPtr deviceInfoSet, ref SP_DEVINFO_DATA deviceInfoData, ref Guid interfaceClassGuid, int memberIndex, ref SP_DEVICE_INTERFACE_DATA deviceInterfaceData);

	    [DllImport("setupapi.dll", CharSet = CharSet.Auto)]
        static internal extern IntPtr SetupDiGetClassDevs(ref System.Guid classGuid, string enumerator, int hwndParent, int flags);

        [DllImport("setupapi.dll", CharSet = CharSet.Auto, EntryPoint = "SetupDiGetDeviceInterfaceDetail")]
        static internal extern bool SetupDiGetDeviceInterfaceDetailBuffer(IntPtr deviceInfoSet, ref SP_DEVICE_INTERFACE_DATA deviceInterfaceData, IntPtr deviceInterfaceDetailData, int deviceInterfaceDetailDataSize, ref int requiredSize, IntPtr deviceInfoData);

	    [DllImport("setupapi.dll", CharSet = CharSet.Auto)]
	    static internal extern bool SetupDiGetDeviceInterfaceDetail(IntPtr deviceInfoSet, ref SP_DEVICE_INTERFACE_DATA deviceInterfaceData, ref SP_DEVICE_INTERFACE_DETAIL_DATA deviceInterfaceDetailData, int deviceInterfaceDetailDataSize, ref int requiredSize, IntPtr deviceInfoData);

        [DllImport("setupapi.dll", CharSet = CharSet.Auto)]
        static internal extern bool SetupDiSetClassInstallParams(IntPtr deviceInfoSet, ref SP_DEVINFO_DATA deviceInfoData, ref SP_PROPCHANGE_PARAMS classInstallParams, int classInstallParamsSize);

        [DllImport("setupapi.dll", CharSet = CharSet.Auto)]
        static internal extern bool SetupDiCallClassInstaller(int installFunction, IntPtr deviceInfoSet, ref SP_DEVINFO_DATA deviceInfoData);

        [DllImport("setupapi.dll", CharSet = CharSet.Auto)]
        static internal extern bool SetupDiGetDeviceInstanceId(IntPtr deviceInfoSet, ref SP_DEVINFO_DATA deviceInfoData, char[] deviceInstanceId, Int32 deviceInstanceIdSize, ref int requiredSize);

        [DllImport("user32.dll")]
	    static internal extern bool UnregisterDeviceNotification(IntPtr handle);

	    internal const short HIDP_INPUT = 0;
	    internal const short HIDP_OUTPUT = 1;

	    internal const short HIDP_FEATURE = 2;
	    [StructLayout(LayoutKind.Sequential)]
	    internal struct HIDD_ATTRIBUTES
	    {
		    internal int Size;
		    internal ushort VendorID;
		    internal ushort ProductID;
		    internal short VersionNumber;
	    }

	    [StructLayout(LayoutKind.Sequential)]
	    internal struct HIDP_CAPS
	    {
		    internal short Usage;
		    internal short UsagePage;
		    internal short InputReportByteLength;
		    internal short OutputReportByteLength;
		    internal short FeatureReportByteLength;
		    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 17)]
		    internal short[] Reserved;
		    internal short NumberLinkCollectionNodes;
		    internal short NumberInputButtonCaps;
		    internal short NumberInputValueCaps;
		    internal short NumberInputDataIndices;
		    internal short NumberOutputButtonCaps;
		    internal short NumberOutputValueCaps;
		    internal short NumberOutputDataIndices;
		    internal short NumberFeatureButtonCaps;
		    internal short NumberFeatureValueCaps;
		    internal short NumberFeatureDataIndices;
	    }

	    [StructLayout(LayoutKind.Sequential)]
	    internal struct HIDP_VALUE_CAPS
	    {
		    internal short UsagePage;
		    internal byte ReportID;
		    internal int IsAlias;
		    internal short BitField;
		    internal short LinkCollection;
		    internal short LinkUsage;
		    internal short LinkUsagePage;
		    internal int IsRange;
		    internal int IsStringRange;
		    internal int IsDesignatorRange;
		    internal int IsAbsolute;
		    internal int HasNull;
		    internal byte Reserved;
		    internal short BitSize;
		    internal short ReportCount;
		    internal short Reserved2;
		    internal short Reserved3;
		    internal short Reserved4;
		    internal short Reserved5;
		    internal short Reserved6;
		    internal int LogicalMin;
		    internal int LogicalMax;
		    internal int PhysicalMin;
		    internal int PhysicalMax;
		    internal short UsageMin;
		    internal short UsageMax;
		    internal short StringMin;
		    internal short StringMax;
		    internal short DesignatorMin;
		    internal short DesignatorMax;
		    internal short DataIndexMin;
		    internal short DataIndexMax;
	    }

	    [DllImport("hid.dll")]
	    static internal extern bool HidD_FlushQueue(IntPtr hidDeviceObject);

        [DllImport("hid.dll")]
        static internal extern bool HidD_FlushQueue(SafeFileHandle hidDeviceObject);

	    [DllImport("hid.dll")]
	    static internal extern bool HidD_GetAttributes(IntPtr hidDeviceObject, ref HIDD_ATTRIBUTES attributes);

	    [DllImport("hid.dll")]
	    static internal extern bool HidD_GetFeature(IntPtr hidDeviceObject, byte[] lpReportBuffer, int reportBufferLength);

        [DllImport("hid.dll", SetLastError = true)]
        internal static extern Boolean HidD_GetInputReport(SafeFileHandle HidDeviceObject, Byte[] lpReportBuffer, Int32 ReportBufferLength);        

	    [DllImport("hid.dll")]
	    static internal extern void HidD_GetHidGuid(ref Guid hidGuid);

	    [DllImport("hid.dll")]
	    static internal extern bool HidD_GetNumInputBuffers(IntPtr hidDeviceObject, ref int numberBuffers);

	    [DllImport("hid.dll")]
	    static internal extern bool HidD_GetPreparsedData(IntPtr hidDeviceObject, ref IntPtr preparsedData);

	    [DllImport("hid.dll")]
	    static internal extern bool HidD_FreePreparsedData(IntPtr preparsedData);

	    [DllImport("hid.dll")]
        static internal extern bool HidD_SetFeature(IntPtr hidDeviceObject, byte[] lpReportBuffer, int reportBufferLength);

        [DllImport("hid.dll")]
        static internal extern bool HidD_SetFeature(SafeFileHandle hidDeviceObject, byte[] lpReportBuffer, int reportBufferLength);

	    [DllImport("hid.dll")]
	    static internal extern bool HidD_SetNumInputBuffers(IntPtr hidDeviceObject, int numberBuffers);
        
        [DllImport("hid.dll")]
        static internal extern bool HidD_SetOutputReport(IntPtr hidDeviceObject, byte[] lpReportBuffer, int reportBufferLength);

        [DllImport("hid.dll", SetLastError = true)]
        static internal extern bool HidD_SetOutputReport(SafeFileHandle hidDeviceObject, byte[] lpReportBuffer, int reportBufferLength);

	    [DllImport("hid.dll")]
	    static internal extern int HidP_GetCaps(IntPtr preparsedData, ref HIDP_CAPS capabilities);

	    [DllImport("hid.dll")]
	    static internal extern int HidP_GetValueCaps(short reportType, ref byte valueCaps, ref short valueCapsLength, IntPtr preparsedData);

#if WIN64
        [DllImport("hid.dll")]
        static internal extern bool HidD_GetSerialNumberString(IntPtr HidDeviceObject, byte[] Buffer, ulong BufferLength);
#else
        [DllImport("hid.dll")]
        static internal extern bool HidD_GetSerialNumberString(IntPtr HidDeviceObject, byte[] Buffer, uint BufferLength);
#endif
    }
}
