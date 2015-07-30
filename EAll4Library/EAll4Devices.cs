using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EAll4Windows
{
    public class EAll4Devices
    {
        private static Dictionary<string, EAll4Device> Devices = new Dictionary<string, EAll4Device>();
        private static HashSet<String> DevicePaths = new HashSet<String>();
        public static bool isExclusiveMode = false;

        //enumerates eall4 controllers in the system
        public static void findControllers()
        {
            lock (Devices)
            {
                int[] pid = { 0x5C4 };
                IEnumerable<HidDevice> hDevices = HidDevices.Enumerate(0x054C, pid);
                // Sort Bluetooth first in case USB is also connected on the same controller.
                hDevices = hDevices.OrderBy<HidDevice, ConnectionType>((HidDevice d) => { return EAll4Device.HidConnectionType(d); });

                foreach (HidDevice hDevice in hDevices)
                {
                    if (DevicePaths.Contains(hDevice.DevicePath))
                        continue; // BT/USB endpoint already open once
                    if (!hDevice.IsOpen)
                    {
                        hDevice.OpenDevice(isExclusiveMode);
                        // TODO in exclusive mode, try to hold both open when both are connected
                        if (isExclusiveMode && !hDevice.IsOpen)
                            hDevice.OpenDevice(false);
                    }
                    if (hDevice.IsOpen)
                    {
                        if (Devices.ContainsKey(hDevice.readSerial()))
                            continue; // happens when the BT endpoint already is open and the USB is plugged into the same host
                        else
                        {
                            EAll4Device eall4Device = new EAll4Device(hDevice);
                            eall4Device.Removal += On_Removal;
                            Devices.Add(eall4Device.MacAddress, eall4Device);
                            DevicePaths.Add(hDevice.DevicePath);
                            eall4Device.StartUpdate();
                        }
                    }
                }
                
            }
        }

        //allows to get EAll4Device by specifying unique MAC address
        //format for MAC address is XX:XX:XX:XX:XX:XX
        public static EAll4Device getEAll4Controller(string mac)
        {
            lock (Devices)
            {
                EAll4Device device = null;
                try
                {
                    Devices.TryGetValue(mac, out device);
                }
                catch (ArgumentNullException) { }
                return device;
            }
        }
        
        //returns EAll4 controllers that were found and are running
        public static IEnumerable<EAll4Device> getEAll4Controllers()
        {
            lock (Devices)
            {
                EAll4Device[] controllers = new EAll4Device[Devices.Count];
                Devices.Values.CopyTo(controllers, 0);
                return controllers;
            }
        }

        public static void stopControllers()
        {
            lock (Devices)
            {
                IEnumerable<EAll4Device> devices = getEAll4Controllers();
                foreach (EAll4Device device in devices)
                {
                    device.StopUpdate();
                    device.HidDevice.CloseDevice();
                }
                Devices.Clear();
                DevicePaths.Clear();
            }
        }

        //called when devices is diconnected, timed out or has input reading failure
        public static void On_Removal(object sender, EventArgs e)
        {
            lock (Devices)
            {
                EAll4Device device = (EAll4Device)sender;
                device.HidDevice.CloseDevice();
                Devices.Remove(device.MacAddress);
                DevicePaths.Remove(device.HidDevice.DevicePath);
            }
        }
    }
}
