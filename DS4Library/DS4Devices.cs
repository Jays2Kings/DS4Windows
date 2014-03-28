using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HidLibrary;
namespace DS4Library
{
    public class DS4Devices
    {
        private static Dictionary<string, DS4Device> Devices = new Dictionary<string, DS4Device>();
        public static bool isExclusiveMode = false;

        //enumerates ds4 controllers in the system
        public static void findControllers()
        {
            lock (Devices)
            {
                int[] pid = { 0x5C4 };
                IEnumerable<HidDevice> hDevices = HidDevices.Enumerate(0x054C, pid);

                foreach (HidDevice hDevice in hDevices)
                {
                    if (!hDevice.IsOpen)
                        hDevice.OpenDevice(isExclusiveMode);
                    if (hDevice.IsOpen)
                    {
                        byte[] buffer = new byte[38];
                        buffer[0] = 0x2;
                        hDevice.readFeatureData(buffer);
                        if (Devices.ContainsKey(hDevice.readSerial()))
                            continue;
                        else
                        {
                            DS4Device ds4Device = new DS4Device(hDevice);
                            ds4Device.StartUpdate();
                            ds4Device.Removal += On_Removal;
                            Devices.Add(ds4Device.MacAddress, ds4Device);
                        }
                    }
                    else
                    {
                        throw new Exception("ERROR: Can't open DS4 Controller. Try quitting other applications like Steam, Uplay, etc.)");
                    }
                }
                
            }
        }

        //allows to get DS4Device by specifying unique MAC address
        //format for MAC address is XX:XX:XX:XX:XX:XX
        public static DS4Device getDS4Controller(string mac)
        { 
            DS4Device device = null;
            try
            {
                Devices.TryGetValue(mac, out device);
            }
            catch (ArgumentNullException) { }
            return device;
        }
        
        //returns DS4 controllers that were found and are running
        public static IEnumerable<DS4Device> getDS4Controllers()
        {
            return Devices.Values;
        }

        public static void stopControllers()
        { 
            IEnumerable<DS4Device> devices = getDS4Controllers();
            foreach (DS4Device device in devices)
            {
                device.StopUpdate();
                device.HidDevice.CloseDevice();
            }
            Devices.Clear();
        }

        //called when devices is diconnected, timed out or has input reading failure
        public static void On_Removal(object sender, EventArgs e)
        {
            lock (Devices)
            {
                DS4Device device = (DS4Device)sender;
                device.HidDevice.CloseDevice();
                Devices.Remove(device.MacAddress);
            }
        }
    }
}
