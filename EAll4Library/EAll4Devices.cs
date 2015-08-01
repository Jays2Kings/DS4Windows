using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EAll4Windows
{
    public class EAll4Devices
    {
        private static Dictionary<string, IEAll4Device> Devices = new Dictionary<string, IEAll4Device>();
        private static HashSet<String> DevicePaths = new HashSet<String>();
        public static bool isExclusiveMode = false;

        //enumerates eall4 controllers in the system
        public static void findControllers()
        {
            lock (Devices)
            {
                //TODO Move to frontend to allow any controller
                //Detect DS4 Controllers
                int[] pid = { 0x5C4 };
                List<HidDevice> hDevices = HidDevices.Enumerate(0x054C, pid).ToList();
                // Sort Bluetooth first in case USB is also connected on the same controller.
                hDevices = hDevices.OrderBy<HidDevice, ConnectionType>((HidDevice d) => { return Ds4Device.HidConnectionType(d); }).ToList();
                //Detect Miui Controllers
                hDevices.AddRange(HidDevices.Enumerate(0x2717, 0x3144));
                //Detect iPega Controllers 
                hDevices.AddRange(HidDevices.Enumerate(0x1949, 0x0402));

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
                    if (!hDevice.IsOpen) continue;
                    if (Devices.ContainsKey(hDevice.readSerial()))
                        continue; // happens when the BT endpoint already is open and the USB is plugged into the same host

                    IEAll4Device controller = new Ds4Device(); //TODO Load appropriate Device
                    controller.Load(hDevice);
                    controller.Removal += On_Removal;
                    Devices.Add(controller.MacAddress, controller);
                    DevicePaths.Add(hDevice.DevicePath);
                    controller.StartUpdate();
                }

            }
        }

        //allows to get EAll4Device by specifying unique MAC address
        //format for MAC address is XX:XX:XX:XX:XX:XX
        public static IEAll4Device getEAll4Controller(string mac)
        {
            lock (Devices)
            {
                IEAll4Device ieAll4Device = null;
                try
                {
                    Devices.TryGetValue(mac, out ieAll4Device);
                }
                catch (ArgumentNullException) { }
                return ieAll4Device;
            }
        }

        //returns EAll4 controllers that were found and are running
        public static IEnumerable<IEAll4Device> getEAll4Controllers()
        {
            lock (Devices)
            {
                IEAll4Device[] controllers = new Ds4Device[Devices.Count]; //TODO Load appropriate device
                Devices.Values.CopyTo(controllers, 0);
                return controllers;
            }
        }

        public static void stopControllers()
        {
            lock (Devices)
            {
                IEnumerable<IEAll4Device> devices = getEAll4Controllers();
                foreach (var device in devices)
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
                IEAll4Device ieAll4Device = (IEAll4Device)sender;
                ieAll4Device.HidDevice.CloseDevice();
                Devices.Remove(ieAll4Device.MacAddress);
                DevicePaths.Remove(ieAll4Device.HidDevice.DevicePath);
            }
        }
    }
}
