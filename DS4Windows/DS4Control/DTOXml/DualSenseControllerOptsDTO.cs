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

using System;
using System.Xml;
using System.Xml.Serialization;
using DS4Windows;
using DS4Windows.InputDevices;
using static DS4Windows.DualSenseControllerOptions;

namespace DS4WinWPF.DS4Control.DTOXml
{
    [XmlRoot(DualSenseControllerOptions.XML_ELEMENT_NAME)]
    public class DualSenseControllerOptsDTO : IDTO<DualSenseControllerOptions>
    {
        [XmlElement("LEDBarMode")]
        public LEDBarMode LEDMode
        {
            get; set;
        }

        [XmlElement("MuteLEDMode")]
        public MuteLEDMode MuteLedMode
        {
            get; set;
        }

        public void MapFrom(DualSenseControllerOptions source)
        {
            LEDMode = source.LedMode;
            MuteLedMode = source.MuteLedMode;
        }

        public void MapTo(DualSenseControllerOptions destination)
        {
            destination.LedMode = LEDMode;
            destination.MuteLedMode = MuteLedMode;
        }
    }
}
