using Nefarius.ViGEm.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS4Windows
{
    static class DS4OutDeviceFactory
    {
        // First known ViGEmBus version to allow full output DS4 Ext support
        private static Version extAPIMinVersion = new Version("1.17.333.0");
        // Temp version number for ViGEmBus version with AwaitOutputBuffer API calls
        private static Version outBuffAPIMinVersion = new Version("1.19.0.0");

        public static DS4OutDevice CreateDS4Device(ViGEmClient client,
            Version driverVersion)
        {
            DS4OutDevice result = null;
            if (outBuffAPIMinVersion.CompareTo(driverVersion) <= 0)
            {
                result = new DS4OutDeviceExt(client, useAwaitOutputBuffer: true);
            }
            else if (extAPIMinVersion.CompareTo(driverVersion) <= 0)
            //if (extAPIMinVersion.CompareTo(driverVersion) <= 0)
            {
                result = new DS4OutDeviceExt(client, useAwaitOutputBuffer: false);
            }
            else
            {
                result = new DS4OutDeviceBasic(client);
            }

            return result;
        }
    }
}
