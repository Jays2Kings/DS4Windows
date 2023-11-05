/*
DS4Windows
Copyright (C) 2023  Travis Nickles

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/

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
