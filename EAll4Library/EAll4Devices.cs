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
                var typeOfInterface = typeof(IEAll4Device);
                var types = AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(s => s.GetTypes())
                    .Where(p => typeOfInterface.IsAssignableFrom(p) && !p.IsInterface);//.Select(p => (IEAll4Device)p).ToList();
                var hDevices = new List<HidDevice>();
                foreach (var type in types)
                {
                    //Detect DS4 Controllers
                    IEAll4Device helper = (IEAll4Device)Activator.CreateInstance(type);
                    int[] pids = helper.PIDs;
                    int[] vids = helper.VIDs;
                    foreach (var vid in vids)
                    {
                        var devices = HidDevices.Enumerate(vid, pids).ToList();
                        // Sort Bluetooth first in case USB is also connected on the same controller.
                        hDevices = devices.OrderBy(d => ConnectionType(d, helper)).ToList();
                        foreach (var hDevice in hDevices.Where(hDevice => !DevicePaths.Contains(hDevice.DevicePath)))
                        {
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
                            IEAll4Device controller = (IEAll4Device)Activator.CreateInstance(type);
                            controller.Load(hDevice);
                            controller.Removal += On_Removal;
                            Devices.Add(controller.MacAddress, controller);
                            DevicePaths.Add(hDevice.DevicePath);
                            controller.StartUpdate();
                        }
                    }
                }



            }
        }

        private static ConnectionType ConnectionType(HidDevice d, IEAll4Device helper)
        {
            return d.Capabilities.InputReportByteLength == helper.InputReportByteLengthUSB ? EAll4Windows.ConnectionType.USB : EAll4Windows.ConnectionType.BT;
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
                IEAll4Device[] controllers = new IEAll4Device[Devices.Count];
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
