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
using System.Xml.Serialization;
using DS4Windows;

namespace DS4WinWPF.DS4Control.DTOXml
{
    [XmlRoot(JoyConControllerOptions.XML_ELEMENT_NAME)]
    public class JoyConControllerOptsDTO : IDTO<JoyConControllerOptions>
    {
        [XmlElement("EnableHomeLED")]
        public string EnableHomeLEDString
        {
            get => EnableHomeLED.ToString();
            set
            {
                EnableHomeLED = XmlDataUtilities.StrToBool(value);
            }
        }

        [XmlIgnore]
        public bool EnableHomeLED
        {
            get; set;
        }

        public void MapFrom(JoyConControllerOptions source)
        {
            EnableHomeLED = source.EnableHomeLED;
        }

        public void MapTo(JoyConControllerOptions destination)
        {
            destination.EnableHomeLED = EnableHomeLED;
        }
    }
}
