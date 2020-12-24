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
        private static Version extAPIMinVersion = new Version("1.17.333.0");

        public static DS4OutDevice CreateDS4Device(ViGEmClient client,
            Version driverVersion)
        {
            DS4OutDevice result = null;
            if (extAPIMinVersion.CompareTo(driverVersion) <= 0)
            {
                result = new DS4OutDeviceExt(client);
            }
            else
            {
                result = new DS4OutDeviceBasic(client);
            }

            return result;
        }
    }
}
