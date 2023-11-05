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
using System.Collections.Generic;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using DS4Windows;

namespace DS4WinWPF.DS4Control.DTOXml
{
    [XmlRoot("LinkedControllers")]
    public class LinkedProfilesDTO : IDTO<BackingStore>, IXmlSerializable
    {
        private List<LinkedProfileItem> linkedProfileItems;

        public LinkedProfilesDTO()
        {
            linkedProfileItems = new List<LinkedProfileItem>();
        }

        public void MapFrom(BackingStore source)
        {
            foreach(KeyValuePair<string, string> pair in source.linkedProfiles)
            {
                LinkedProfileItem tempItem = new LinkedProfileItem()
                {
                    Serial = pair.Key.Replace("MAC", string.Empty),
                    Profile = pair.Value,
                };

                linkedProfileItems.Add(tempItem);
            }
        }

        public void MapTo(BackingStore destination)
        {
            foreach(LinkedProfileItem item in linkedProfileItems)
            {
                destination.linkedProfiles[item.Serial] = item.Profile;
            }
        }

        public XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
        {
            XmlDocument tempDoc = new XmlDocument();
            tempDoc.Load(reader);

            XmlNode node = tempDoc.SelectSingleNode("/LinkedControllers");
            if (node == null)
            {
                return;
            }

            XmlNodeList links = node.ChildNodes;
            for (int i = 0, listLen = links.Count; i < listLen; i++)
            {
                XmlNode current = links[i];
                if (current.Name.Contains("MAC"))
                {
                    string serial = current.Name.Replace("MAC", string.Empty);
                    string profile = current.InnerText;
                    LinkedProfileItem tempItem = new LinkedProfileItem()
                    {
                        Serial = serial,
                        Profile = profile,
                    };

                    linkedProfileItems.Add(tempItem);
                }
            }
        }

        public void WriteXml(XmlWriter writer)
        {
            int itemCount = linkedProfileItems.Count;
            if (itemCount == 0)
                return;

            foreach (LinkedProfileItem item in linkedProfileItems)
            {
                writer.WriteElementString($"MAC{item.Serial}", item.Profile);
            }
        }
    }

    public class LinkedProfileItem
    {
        [XmlIgnore]
        public string Serial
        {
            get; set;
        }

        [XmlIgnore]
        public string Profile
        {
            get; set;
        }
    }
}
