using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using DS4Windows;

namespace DS4WinWPF
{
    [SuppressUnmanagedCodeSecurity]
    public class AutoProfileChecker
    {
        private AutoProfileHolder profileHolder;
        private IntPtr prevForegroundWnd = IntPtr.Zero;
        private uint prevForegroundProcessID;
        private string prevForegroundProcessName = string.Empty;
        private string prevForegroundWndTitleName = string.Empty;
        private StringBuilder autoProfileCheckTextBuilder = new StringBuilder(1000);
        private int autoProfileDebugLogLevel = 0;
        private bool turnOffTemp;
        private AutoProfileEntity tempAutoProfile;
        private bool running;

        public int AutoProfileDebugLogLevel { get => autoProfileDebugLogLevel; set => autoProfileDebugLogLevel = value; }
        public bool Running { get => running; set => running = value; }

        public delegate void ChangeServiceHandler(AutoProfileChecker sender, bool state);
        public event ChangeServiceHandler RequestServiceChange;

        public AutoProfileChecker(AutoProfileHolder holder)
        {
            profileHolder = holder;
        }

        public void Process()
        {
            string topProcessName, topWindowTitle;
            bool turnOffDS4WinApp = false;
            AutoProfileEntity matchedProfileEntity = null;

            if (GetTopWindowName(out topProcessName, out topWindowTitle))
            {
                // Find a profile match based on autoprofile program path and wnd title list.
                // The same program may set different profiles for each of the controllers, so we need an array of newProfileName[controllerIdx] values.
                for (int i = 0, pathsLen = profileHolder.AutoProfileColl.Count; i < pathsLen; i++)
                {
                    AutoProfileEntity tempEntity = profileHolder.AutoProfileColl[i];
                    if (tempEntity.IsMatch(topProcessName, topWindowTitle))
                    {
                        if (autoProfileDebugLogLevel > 0)
                            DS4Windows.AppLogger.LogToGui($"DEBUG: Auto-Profile. Rule#{i + 1}  Path={tempEntity.path}  Title={tempEntity.title}", false);

                        // Matching autoprofile rule found
                        turnOffDS4WinApp = tempEntity.Turnoff;
                        matchedProfileEntity = tempEntity;
                        break;
                    }
                }

                if (matchedProfileEntity != null)
                {
                    // Program match found. Check if the new profile is different than current profile of the controller. Load the new profile only if it is not already loaded.
                    for (int j = 0; j < 4; j++)
                    {
                        string tempname = matchedProfileEntity.ProfileNames[j];
                        if (tempname != string.Empty && tempname != "(none)")
                        {
                            if ((Global.useTempProfile[j] && tempname != Global.tempprofilename[j]) ||
                                (!Global.useTempProfile[j] && tempname != Global.ProfilePath[j]))
                            {
                                if (autoProfileDebugLogLevel > 0)
                                    DS4Windows.AppLogger.LogToGui($"DEBUG: Auto-Profile. LoadProfile Controller {j + 1}={tempname}", false);

                                Global.LoadTempProfile(j, tempname, true, Program.rootHub); // j is controller index, i is filename
                                                                                              //if (LaunchProgram[j] != string.Empty) Process.Start(LaunchProgram[j]);
                            }
                            else
                            {
                                if (autoProfileDebugLogLevel > 0)
                                    DS4Windows.AppLogger.LogToGui($"DEBUG: Auto-Profile. LoadProfile Controller {j + 1}={tempname} (already loaded)", false);
                            }
                        }
                    }

                    if (turnOffDS4WinApp)
                    {
                        turnOffTemp = true;
                        RequestServiceChange?.Invoke(this, false);
                    }

                    tempAutoProfile = matchedProfileEntity;
                }
                else if (tempAutoProfile != null)
                {
                    tempAutoProfile = null;
                    for (int j = 0; j < 4; j++)
                    {
                        if (Global.useTempProfile[j])
                        {
                            if (autoProfileDebugLogLevel > 0)
                                DS4Windows.AppLogger.LogToGui($"DEBUG: Auto-Profile. RestoreProfile Controller {j + 1}={Global.ProfilePath[j]} (default)", false);

                            Global.LoadProfile(j, false, Program.rootHub);
                        }
                    }

                    if (turnOffTemp)
                    {
                        turnOffTemp = false;
                        RequestServiceChange?.Invoke(this, true);
                    }
                }
            }
        }

        private bool GetTopWindowName(out string topProcessName, out string topWndTitleName)
        {
            IntPtr hWnd = GetForegroundWindow();
            if (hWnd == IntPtr.Zero)
            {
                // Top window unknown or cannot acquire a handle. Return FALSE and return unknown process and wndTitle values
                prevForegroundWnd = IntPtr.Zero;
                prevForegroundProcessID = 0;
                topProcessName = topWndTitleName = String.Empty;
                return false;
            }

            //
            // If this function was called from "auto-profile watcher timer" then check cached "previous hWnd handle". If the current hWnd is the same
            // as during the previous check then return cached previous wnd and name values (ie. foreground app and window are assumed to be the same, so no need to re-query names).
            // This should optimize the auto-profile timer check process and causes less burden to .NET GC collector because StringBuffer is not re-allocated every second.
            //
            // Note! hWnd handles may be re-cycled but not during the lifetime of the window. This "cache" optimization still works because when an old window is closed
            // then foreground window changes to something else and the cached prevForgroundWnd variable is updated to store the new hWnd handle.
            // It doesn't matter even when the previously cached handle is recycled by WinOS to represent some other window (it is no longer used as a cached value anyway).
            //
            if (hWnd == prevForegroundWnd)
            {
                // The active window is still the same. Return cached process and wndTitle values and FALSE to indicate caller that no changes since the last call of this method
                topProcessName = prevForegroundProcessName;
                topWndTitleName = prevForegroundWndTitleName;
                return false;
            }

            prevForegroundWnd = hWnd;

            IntPtr hProcess = IntPtr.Zero;
            uint lpdwProcessId = 0;
            GetWindowThreadProcessId(hWnd, out lpdwProcessId);

            if (lpdwProcessId == prevForegroundProcessID)
            {
                topProcessName = prevForegroundProcessName;
            }
            else
            {
                prevForegroundProcessID = lpdwProcessId;

                hProcess = OpenProcess(0x0410, false, lpdwProcessId);
                if (hProcess != IntPtr.Zero) GetModuleFileNameEx(hProcess, IntPtr.Zero, autoProfileCheckTextBuilder, autoProfileCheckTextBuilder.Capacity);
                else autoProfileCheckTextBuilder.Clear();

                prevForegroundProcessName = topProcessName = autoProfileCheckTextBuilder.Replace('/', '\\').ToString().ToLower();
            }

            GetWindowText(hWnd, autoProfileCheckTextBuilder, autoProfileCheckTextBuilder.Capacity);
            prevForegroundWndTitleName = topWndTitleName = autoProfileCheckTextBuilder.ToString().ToLower();


            if (hProcess != IntPtr.Zero) CloseHandle(hProcess);

            if (autoProfileDebugLogLevel > 0)
                DS4Windows.AppLogger.LogToGui($"DEBUG: Auto-Profile. PID={lpdwProcessId}  Path={topProcessName} | WND={hWnd}  Title={topWndTitleName}", false);

            return true;
        }

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport("kernel32.dll")]
        private static extern IntPtr OpenProcess(uint dwDesiredAccess, bool bInheritHandle, uint dwProcessId);

        [DllImport("kernel32.dll")]
        private static extern bool CloseHandle(IntPtr handle);

        [DllImport("psapi.dll")]
        private static extern uint GetModuleFileNameEx(IntPtr hWnd, IntPtr hModule, StringBuilder lpFileName, int nSize);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nSize);
    }
}
