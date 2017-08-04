using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Principal;

namespace DS4Windows
{
    public class DS4Devices
    {
        private static Dictionary<string, DS4Device> Devices = new Dictionary<string, DS4Device>();
        private static HashSet<string> DevicePaths = new HashSet<string>();
        // Keep instance of opened exclusive mode devices not in use (Charging while using BT connection)
        private static List<HidDevice> DisabledDevices = new List<HidDevice>();
        private static Stopwatch sw = new Stopwatch();
        public static bool isExclusiveMode = false;

        private static string devicePathToInstanceId(string devicePath)
        {
            string deviceInstanceId = devicePath;
            deviceInstanceId = deviceInstanceId.Remove(0, deviceInstanceId.LastIndexOf('\\') + 1);
            deviceInstanceId = deviceInstanceId.Remove(deviceInstanceId.LastIndexOf('{'));
            deviceInstanceId = deviceInstanceId.Replace('#', '\\');
            if (deviceInstanceId.EndsWith("\\"))
            {
                deviceInstanceId = deviceInstanceId.Remove(deviceInstanceId.Length - 1);
            }

            return deviceInstanceId;
        }

        // Enumerates ds4 controllers in the system
        public static void findControllers()
        {
            lock (Devices)
            {
                int[] pid = { 0xBA0, 0x5C4, 0x09CC };
                IEnumerable<HidDevice> hDevices = HidDevices.Enumerate(0x054C, pid);
                // Sort Bluetooth first in case USB is also connected on the same controller.
                hDevices = hDevices.OrderBy<HidDevice, ConnectionType>((HidDevice d) => { return DS4Device.HidConnectionType(d); });

                List<HidDevice> tempList = hDevices.ToList();
                purgeHiddenExclusiveDevices();
                tempList.AddRange(DisabledDevices);
                int devCount = tempList.Count();
                string devicePlural = "device" + (devCount == 0 || devCount > 1 ? "s" : "");
                //Log.LogToGui("Found " + devCount + " possible " + devicePlural + ". Examining " + devicePlural + ".", false);

                for (int i = 0; i < devCount; i++)
                //foreach (HidDevice hDevice in hDevices)
                {
                    HidDevice hDevice = tempList[i];
                    if (DevicePaths.Contains(hDevice.DevicePath))
                        continue; // BT/USB endpoint already open once

                    if (!hDevice.IsOpen)
                    {
                        hDevice.OpenDevice(isExclusiveMode);
                        if (!hDevice.IsOpen && isExclusiveMode)
                        {
                            try
                            {
                                WindowsIdentity identity = WindowsIdentity.GetCurrent();
                                WindowsPrincipal principal = new WindowsPrincipal(identity);
                                bool elevated = principal.IsInRole(WindowsBuiltInRole.Administrator);

                                if (!elevated)
                                {
                                    // Launches an elevated child process to re-enable device
                                    string exeName = Process.GetCurrentProcess().MainModule.FileName;
                                    ProcessStartInfo startInfo = new ProcessStartInfo(exeName);
                                    startInfo.Verb = "runas";
                                    startInfo.Arguments = "re-enabledevice " + devicePathToInstanceId(hDevice.DevicePath);
                                    Process child = Process.Start(startInfo);

                                    if (!child.WaitForExit(5000))
                                    {
                                        child.Kill();
                                    }
                                    else if (child.ExitCode == 0)
                                    {
                                        hDevice.OpenDevice(isExclusiveMode);
                                    }
                                }
                                else
                                {
                                    reEnableDevice(devicePathToInstanceId(hDevice.DevicePath));
                                    hDevice.OpenDevice(isExclusiveMode);
                                }
                            }
                            catch (Exception) { }
                        }
                        
                        // TODO in exclusive mode, try to hold both open when both are connected
                        if (isExclusiveMode && !hDevice.IsOpen)
                            hDevice.OpenDevice(false);
                    }

                    if (hDevice.IsOpen)
                    {
                        string serial = hDevice.readSerial();
                        bool validSerial = !serial.Equals(DS4Device.blankSerial);
                        if (Devices.ContainsKey(serial))
                        {
                            // happens when the BT endpoint already is open and the USB is plugged into the same host
                            if (isExclusiveMode && hDevice.IsExclusive &&
                                !DisabledDevices.Contains(hDevice))
                            {
                                // Grab reference to exclusively opened HidDevice so device
                                // stays hidden to other processes
                                DisabledDevices.Add(hDevice);
                                //DevicePaths.Add(hDevice.DevicePath);
                            }

                            continue;
                        }
                        else
                        {
                            DS4Device ds4Device = new DS4Device(hDevice);
                            //ds4Device.Removal += On_Removal;
                            Devices.Add(ds4Device.MacAddress, ds4Device);
                            DevicePaths.Add(hDevice.DevicePath);
                        }
                    }
                }
            }
        }

        //allows to get DS4Device by specifying unique MAC address
        //format for MAC address is XX:XX:XX:XX:XX:XX
        public static DS4Device getDS4Controller(string mac)
        {
            lock (Devices)
            {
                DS4Device device = null;
                try
                {
                    Devices.TryGetValue(mac, out device);
                }
                catch (ArgumentNullException) { }
                return device;
            }
        }
        
        // Returns DS4 controllers that were found and are running
        public static IEnumerable<DS4Device> getDS4Controllers()
        {
            lock (Devices)
            {
                DS4Device[] controllers = new DS4Device[Devices.Count];
                Devices.Values.CopyTo(controllers, 0);
                return controllers;
            }
        }

        public static void stopControllers()
        {
            lock (Devices)
            {
                IEnumerable<DS4Device> devices = getDS4Controllers();
                //foreach (DS4Device device in devices)
                for (int i = 0, devCount = devices.Count(); i < devCount; i++)
                {
                    DS4Device device = devices.ElementAt(i);
                    device.StopUpdate();
                    //device.runRemoval();
                    device.HidDevice.CloseDevice();
                }

                Devices.Clear();
                DevicePaths.Clear();
                DisabledDevices.Clear();
            }
        }

        // Called when devices is diconnected, timed out or has input reading failure
        public static void On_Removal(object sender, EventArgs e)
        {
            lock (Devices)
            {
                DS4Device device = (DS4Device)sender;
                if (device != null)
                {
                    device.HidDevice.CloseDevice();
                    Devices.Remove(device.MacAddress);
                    DevicePaths.Remove(device.HidDevice.DevicePath);
                    //purgeHiddenExclusiveDevices();
                }
            }
        }

        public static void UpdateSerial(object sender, EventArgs e)
        {
            lock (Devices)
            {
                DS4Device device = (DS4Device)sender;
                if (device != null)
                {
                    string serial = device.getMacAddress();
                    if (Devices.ContainsKey(serial))
                    {
                        Devices.Remove(serial);
                        device.updateSerial();
                        serial = device.getMacAddress();
                        Devices.Add(serial, device);
                    }
                }
            }
        }

        private static void purgeHiddenExclusiveDevices()
        {
            int disabledDevCount = DisabledDevices.Count;
            if (disabledDevCount > 0)
            {
                List<HidDevice> disabledDevList = new List<HidDevice>();
                for (int i = 0, arlen = disabledDevCount; i < arlen; i++)
                {
                    HidDevice tempDev = DisabledDevices.ElementAt(i);
                    if (tempDev != null)
                    {
                        if (tempDev.IsOpen && tempDev.IsConnected)
                        {
                            disabledDevList.Add(tempDev);
                        }
                        else if (tempDev.IsOpen)
                        {
                            if (!tempDev.IsConnected)
                            {
                                try
                                {
                                    tempDev.CloseDevice();
                                }
                                catch { }
                            }

                            if (DevicePaths.Contains(tempDev.DevicePath))
                            {
                                DevicePaths.Remove(tempDev.DevicePath);
                            }
                        }
                    }
                }

                DisabledDevices.Clear();
                DisabledDevices.AddRange(disabledDevList);
            }
        }

        public static void reEnableDevice(string deviceInstanceId)
        {
            bool success;
            Guid hidGuid = new Guid();
            NativeMethods.HidD_GetHidGuid(ref hidGuid);
            IntPtr deviceInfoSet = NativeMethods.SetupDiGetClassDevs(ref hidGuid, deviceInstanceId, 0, NativeMethods.DIGCF_PRESENT | NativeMethods.DIGCF_DEVICEINTERFACE);
            NativeMethods.SP_DEVINFO_DATA deviceInfoData = new NativeMethods.SP_DEVINFO_DATA();
            deviceInfoData.cbSize = Marshal.SizeOf(deviceInfoData);
            success = NativeMethods.SetupDiEnumDeviceInfo(deviceInfoSet, 0, ref deviceInfoData);
            if (!success)
            {
                throw new Exception("Error getting device info data, error code = " + Marshal.GetLastWin32Error());
            }
            success = NativeMethods.SetupDiEnumDeviceInfo(deviceInfoSet, 1, ref deviceInfoData); // Checks that we have a unique device
            if (success)
            {
                throw new Exception("Can't find unique device");
            }

            NativeMethods.SP_PROPCHANGE_PARAMS propChangeParams = new NativeMethods.SP_PROPCHANGE_PARAMS();
            propChangeParams.classInstallHeader.cbSize = Marshal.SizeOf(propChangeParams.classInstallHeader);
            propChangeParams.classInstallHeader.installFunction = NativeMethods.DIF_PROPERTYCHANGE;
            propChangeParams.stateChange = NativeMethods.DICS_DISABLE;
            propChangeParams.scope = NativeMethods.DICS_FLAG_GLOBAL;
            propChangeParams.hwProfile = 0;
            success = NativeMethods.SetupDiSetClassInstallParams(deviceInfoSet, ref deviceInfoData, ref propChangeParams, Marshal.SizeOf(propChangeParams));
            if (!success)
            {
                throw new Exception("Error setting class install params, error code = " + Marshal.GetLastWin32Error());
            }
            success = NativeMethods.SetupDiCallClassInstaller(NativeMethods.DIF_PROPERTYCHANGE, deviceInfoSet, ref deviceInfoData);
            if (!success)
            {
                throw new Exception("Error disabling device, error code = " + Marshal.GetLastWin32Error());
            }

            //System.Threading.Thread.Sleep(50);
            sw.Restart();
            while (sw.ElapsedMilliseconds < 50)
            {
                // Use SpinWait to keep control of current thread. Using Sleep could potentially
                // cause other events to get run out of order
                System.Threading.Thread.SpinWait(100);
            }
            sw.Stop();

            propChangeParams.stateChange = NativeMethods.DICS_ENABLE;
            success = NativeMethods.SetupDiSetClassInstallParams(deviceInfoSet, ref deviceInfoData, ref propChangeParams, Marshal.SizeOf(propChangeParams));
            if (!success)
            {
                throw new Exception("Error setting class install params, error code = " + Marshal.GetLastWin32Error());
            }
            success = NativeMethods.SetupDiCallClassInstaller(NativeMethods.DIF_PROPERTYCHANGE, deviceInfoSet, ref deviceInfoData);
            if (!success)
            {
                throw new Exception("Error enabling device, error code = " + Marshal.GetLastWin32Error());
            }

            NativeMethods.SetupDiDestroyDeviceInfoList(deviceInfoSet);
        }
    }
}
