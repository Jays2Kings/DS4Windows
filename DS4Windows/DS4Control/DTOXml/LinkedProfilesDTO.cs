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
