using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using DS4Windows;

namespace DS4WinWPF.DS4Control.DTOXml
{
    [XmlRoot("Programs")]
    public class AutoProfilesDTO : IDTO<AutoProfileHolder>
    {
        [XmlElement("Program")] // Use XmlElement here to skip container element
        public List<AutoProfileEntrySerializer> programEntries;

        public AutoProfilesDTO()
        {
            programEntries = new List<AutoProfileEntrySerializer>();
        }

        public void MapFrom(AutoProfileHolder source)
        {
            foreach(AutoProfileEntity entity in source.AutoProfileColl)
            {
                AutoProfileEntrySerializer temp = new AutoProfileEntrySerializer()
                {
                    Path = entity.Path,
                    Title = entity.Title,
                    Controller1 = entity.ProfileNames[0],
                    Controller2 = entity.ProfileNames[1],
                    Controller3 = entity.ProfileNames[2],
                    TurnOff = entity.Turnoff,
                };

                if (ControlService.USING_MAX_CONTROLLERS)
                {
                    temp.Controller4 = entity.ProfileNames[3];
                    temp.Controller5 = entity.ProfileNames[4];
                    temp.Controller6 = entity.ProfileNames[5];
                    temp.Controller7 = entity.ProfileNames[6];
                    temp.Controller8 = entity.ProfileNames[7];
                }

                programEntries.Add(temp);
            }
        }

        public void MapTo(AutoProfileHolder destination)
        {
            foreach(AutoProfileEntrySerializer serializier in programEntries)
            {
                AutoProfileEntity autoprof = new AutoProfileEntity(serializier.Path, serializier.Title);

                autoprof.ProfileNames[0] = serializier.Controller1;
                autoprof.ProfileNames[1] = serializier.Controller2;
                autoprof.ProfileNames[2] = serializier.Controller3;
                autoprof.ProfileNames[3] = serializier.Controller4;

                if (ControlService.CURRENT_DS4_CONTROLLER_LIMIT >=
                    ControlService.MAX_DS4_CONTROLLER_COUNT)
                {
                    autoprof.ProfileNames[4] = serializier.Controller5;
                    autoprof.ProfileNames[5] = serializier.Controller6;
                    autoprof.ProfileNames[6] = serializier.Controller7;
                    autoprof.ProfileNames[7] = serializier.Controller8;
                }

                autoprof.Turnoff = serializier.TurnOff;
                destination.AutoProfileColl.Add(autoprof);
            }
        }
    }

    public class AutoProfileEntrySerializer
    {
        [XmlAttribute("path")]
        public string Path
        {
            get; set;
        } = string.Empty;

        [XmlAttribute("title")]
        public string Title
        {
            get; set;
        } = string.Empty;

        [XmlElement("Controller1")]
        public string Controller1
        {
            get; set;
        } = string.Empty;

        [XmlElement("Controller2")]
        public string Controller2
        {
            get; set;
        } = string.Empty;

        [XmlElement("Controller3")]
        public string Controller3
        {
            get; set;
        } = string.Empty;

        [XmlElement("Controller4")]
        public string Controller4
        {
            get; set;
        } = string.Empty;

        [XmlElement("Controller5")]
        public string Controller5
        {
            get; set;
        } = string.Empty;
        public bool ShouldSerializeController5()
        {
            return Global.MAX_DS4_CONTROLLER_COUNT >= 5;
        }

        [XmlElement("Controller6")]
        public string Controller6
        {
            get; set;
        } = string.Empty;
        public bool ShouldSerializeController6()
        {
            return Global.MAX_DS4_CONTROLLER_COUNT >= 6;
        }

        [XmlElement("Controller7")]
        public string Controller7
        {
            get; set;
        } = string.Empty;
        public bool ShouldSerializeController7()
        {
            return Global.MAX_DS4_CONTROLLER_COUNT >= 7;
        }

        [XmlElement("Controller8")]
        public string Controller8
        {
            get; set;
        } = string.Empty;
        public bool ShouldSerializeController8()
        {
            return Global.MAX_DS4_CONTROLLER_COUNT >= 8;
        }

        [XmlElement("TurnOff")]
        public string TurnOffString
        {
            get => TurnOff.ToString();
            set
            {
                TurnOff = XmlDataUtilities.StrToBool(value);
            }
        }

        [XmlIgnore]
        public bool TurnOff
        {
            get; set;
        }

        public AutoProfileEntrySerializer()
        {
        }
    }
}
