using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security;
using System.Threading.Tasks;
using System.Text;
using System.IO;
using Microsoft.Win32;

namespace DS4Windows
{
    [SuppressUnmanagedCodeSecurity]
    public class Util
    {
        public static Guid sysGuid = Guid.Parse("{4d36e97d-e325-11ce-bfc1-08002be10318}");
        public static Guid fakerInputGuid = Guid.Parse("{ab67b0fa-d0f5-4f60-81f4-346e18fd0805}");

        public enum PROCESS_INFORMATION_CLASS : int
        {
            ProcessBasicInformation = 0,
            ProcessQuotaLimits,
            ProcessIoCounters,
            ProcessVmCounters,
            ProcessTimes,
            ProcessBasePriority,
            ProcessRaisePriority,
            ProcessDebugPort,
            ProcessExceptionPort,
            ProcessAccessToken,
            ProcessLdtInformation,
            ProcessLdtSize,
            ProcessDefaultHardErrorMode,
            ProcessIoPortHandlers,
            ProcessPooledUsageAndLimits,
            ProcessWorkingSetWatch,
            ProcessUserModeIOPL,
            ProcessEnableAlignmentFaultFixup,
            ProcessPriorityClass,
            ProcessWx86Information,
            ProcessHandleCount,
            ProcessAffinityMask,
            ProcessPriorityBoost,
            ProcessDeviceMap,
            ProcessSessionInformation,
            ProcessForegroundInformation,
            ProcessWow64Information,
            ProcessImageFileName,
            ProcessLUIDDeviceMapsEnabled,
            ProcessBreakOnTermination,
            ProcessDebugObjectHandle,
            ProcessDebugFlags,
            ProcessHandleTracing,
            ProcessIoPriority,
            ProcessExecuteFlags,
            ProcessResourceManagement,
            ProcessCookie,
            ProcessImageInformation,
            ProcessCycleTime,
            ProcessPagePriority,
            ProcessInstrumentationCallback,
            ProcessThreadStackAllocation,
            ProcessWorkingSetWatchEx,
            ProcessImageFileNameWin32,
            ProcessImageFileMapping,
            ProcessAffinityUpdateMode,
            ProcessMemoryAllocationMode,
            MaxProcessInfoClass
        }

        [StructLayout(LayoutKind.Sequential)]
        public class DEV_BROADCAST_DEVICEINTERFACE
        {
            internal Int32 dbcc_size;
            internal Int32 dbcc_devicetype;
            internal Int32 dbcc_reserved;
            internal Guid dbcc_classguid;
            internal Int16 dbcc_name;
        }

        public const Int32 DBT_DEVTYP_DEVICEINTERFACE = 0x0005;

        public const Int32 DEVICE_NOTIFY_WINDOW_HANDLE = 0x0000;
        public const Int32 DEVICE_NOTIFY_SERVICE_HANDLE = 0x0001;
        public const Int32 DEVICE_NOTIFY_ALL_INTERFACE_CLASSES = 0x0004;

        public const Int32 WM_CREATE = 0x0001;
        public const Int32 WM_DEVICECHANGE = 0x0219;

        public const Int32 DIGCF_PRESENT = 0x0002;
        public const Int32 DIGCF_DEVICEINTERFACE = 0x0010;

        public const uint EDD_GET_DEVICE_INTERFACE_NAME = 0x00000001;


        [Flags]
        public enum DisplayDeviceStateFlags : int
        {
            /// <summary>The device is part of the desktop.</summary>
            AttachedToDesktop = 0x1,
            MultiDriver = 0x2,
            /// <summary>The device is part of the desktop.</summary>
            PrimaryDevice = 0x4,
            /// <summary>Represents a pseudo device used to mirror application drawing for remoting or other purposes.</summary>
            MirroringDriver = 0x8,
            /// <summary>The device is VGA compatible.</summary>
            VGACompatible = 0x10,
            /// <summary>The device is removable; it cannot be the primary display.</summary>
            Removable = 0x20,
            /// <summary>The device has more display modes than its output devices support.</summary>
            ModesPruned = 0x8000000,
            Remote = 0x4000000,
            Disconnect = 0x2000000
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct DISPLAY_DEVICE
        {
            [MarshalAs(UnmanagedType.U4)]
            public int cb;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string DeviceName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string DeviceString;
            [MarshalAs(UnmanagedType.U4)]
            public DisplayDeviceStateFlags StateFlags;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string DeviceID;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string DeviceKey;
        }

        [DllImport("ntdll.dll", SetLastError = true)]
        public static extern int NtSetInformationProcess(IntPtr processHandle,
           PROCESS_INFORMATION_CLASS processInformationClass, ref IntPtr processInformation, uint processInformationLength);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        protected static extern IntPtr RegisterDeviceNotification(IntPtr hRecipient, IntPtr NotificationFilter, Int32 Flags);

        [DllImport("user32.dll", SetLastError = true)]
        protected static extern Boolean UnregisterDeviceNotification(IntPtr Handle);

        [DllImport("winmm.dll")]
        internal static extern uint timeBeginPeriod(uint period);
        [DllImport("winmm.dll")]
        internal static extern uint timeEndPeriod(uint period);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern bool EnumDisplayDevicesW(string lpDevice, uint iDevNum, ref DISPLAY_DEVICE lpDisplayDevice, uint dwFlags);

        public static Boolean RegisterNotify(IntPtr Form, Guid Class, ref IntPtr Handle, Boolean Window = true)
        {
            IntPtr devBroadcastDeviceInterfaceBuffer = IntPtr.Zero;

            try
            {
                DEV_BROADCAST_DEVICEINTERFACE devBroadcastDeviceInterface = new DEV_BROADCAST_DEVICEINTERFACE();
                Int32 Size = Marshal.SizeOf(devBroadcastDeviceInterface);

                devBroadcastDeviceInterface.dbcc_size = Size;
                devBroadcastDeviceInterface.dbcc_devicetype = DBT_DEVTYP_DEVICEINTERFACE;
                devBroadcastDeviceInterface.dbcc_reserved = 0;
                devBroadcastDeviceInterface.dbcc_classguid = Class;

                devBroadcastDeviceInterfaceBuffer = Marshal.AllocHGlobal(Size);
                Marshal.StructureToPtr(devBroadcastDeviceInterface, devBroadcastDeviceInterfaceBuffer, true);

                Handle = RegisterDeviceNotification(Form, devBroadcastDeviceInterfaceBuffer, Window ? DEVICE_NOTIFY_WINDOW_HANDLE : DEVICE_NOTIFY_SERVICE_HANDLE);

                Marshal.PtrToStructure(devBroadcastDeviceInterfaceBuffer, devBroadcastDeviceInterface);

                return Handle != IntPtr.Zero;
            }
            catch (Exception ex)
            {
                Console.WriteLine("{0} {1}", ex.HelpLink, ex.Message);
                throw;
            }
            finally
            {
                if (devBroadcastDeviceInterfaceBuffer != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(devBroadcastDeviceInterfaceBuffer);
                }
            }
        }

        public static Boolean UnregisterNotify(IntPtr Handle)
        {
            try
            {
                return UnregisterDeviceNotification(Handle);
            }
            catch (Exception ex)
            {
                Console.WriteLine("{0} {1}", ex.HelpLink, ex.Message);
                throw;
            }
        }

        public static void StartProcessHelper(string path)
        {
            if (!Global.IsAdministrator())
            {
                ProcessStartInfo startInfo = new ProcessStartInfo(path);
                startInfo.UseShellExecute = true;
                try
                {
                    using (Process temp = Process.Start(startInfo))
                    {
                    }
                }
                catch { }
            }
            else
            {
                StartProcessInExplorer(path);
            }
        }

        /// <summary>
        /// Launch process in Explorer to de-elevate the process if DS4Windows is running
        /// as under the Admin account
        /// </summary>
        /// <param name="path">Program path or URL</param>
        public static void StartProcessInExplorer(string path)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = "explorer.exe";
            // Need to place Path/URL in double quotes to allow equals sign to not be
            // interpreted as a delimiter
            startInfo.Arguments = $"\"{path}\"";
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.UseShellExecute = true;
            try
            {
                using (Process temp = Process.Start(startInfo)) { }
            }
            catch { }
        }

        public static void LogAssistBackgroundTask(Task task)
        {
            task.ContinueWith((t) =>
            {
                if (t.IsFaulted)
                {
                    AppLogger.LogToGui(t.Exception.ToString(), true);
                }
            });
        }

        public static int ElevatedCopyUpdater(string tmpUpdaterPath, bool deleteUpdatesDir=false)
        {
            int result = -1;
            string tmpPath = Path.Combine(Path.GetTempPath(), "updatercopy.bat");
            //string tmpPath = Path.GetTempFileName();
            // Create temporary bat script that will later be executed
            using (StreamWriter w = new StreamWriter(new FileStream(tmpPath,
                FileMode.Create, FileAccess.Write)))
            {
                w.WriteLine("@echo off"); // Turn off echo
                w.WriteLine("@echo Attempting to replace updater, please wait...");
                // Move temp downloaded file to destination
                w.WriteLine($"@mov /Y \"{tmpUpdaterPath}\" \"{Global.exedirpath}\\DS4Updater.exe\"");
                if (deleteUpdatesDir)
                {
                    w.WriteLine($"@del /S \"{Global.exedirpath}\\Update Files\\DS4Windows\"");
                }

                w.Close();
            }

            // Execute temp batch script with admin privileges
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = tmpPath;
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.Verb = "runas";
            startInfo.UseShellExecute = true;
            startInfo.CreateNoWindow = true;
            try
            {
                // Launch process, wait and then save exit code
                using (Process temp = Process.Start(startInfo))
                {
                    temp.WaitForExit();
                    result = temp.ExitCode;
                }
            }
            catch { }

            return result;
        }

        public static string GetOSProductName()
        {
            string productName =
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion", "ProductName", "").ToString();
            return productName;
        }

        public static string GetOSReleaseId()
        {
            string releaseId =
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion", "ReleaseId", "").ToString();
            return releaseId;
        }

        /// <summary>
        /// DS4Windows and HidHideClient need to be on same drive. Assume default
        /// install path. Don't care if someone has changed the install path.
        /// Return found path or string.Empty if path not found. Good enough for me.
        /// </summary>
        /// <returns></returns>
        public static string GetHidHideClientPath()
        {
            string result = string.Empty;
            string driveLetter = Path.GetPathRoot(Global.exedirpath);
            string[] testPaths = new string[]
            {
                Path.Combine(driveLetter, "Program Files",
                    "Nefarius Software Solutions e.U", "HidHideClient", "HidHideClient.exe"),

                Path.Combine(driveLetter, @"Program Files (x86)",
                    "Nefarius Software Solutions e.U", "HidHideClient", "HidHideClient.exe"),

                Path.Combine(driveLetter, @"Program Files",
                    "Nefarius Software Solutions", "HidHide", "x64", "HidHideClient.exe"),

                Path.Combine(driveLetter, @"Program Files",
                    "Nefarius Software Solutions", "HidHide", "x86", "HidHideClient.exe"),
            };

            foreach(string testPath in testPaths)
            {
                if (File.Exists(testPath))
                {
                    result = testPath;
                    break;
                }
            }

            return result;
        }
    }
}
