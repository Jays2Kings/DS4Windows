using System;
using System.Xml;
using System.Xml.Serialization;
using DS4Windows;

namespace DS4WinWPF.DS4Control.DTOXml
{
    [XmlRoot(DS4ControllerOptions.XML_ELEMENT_NAME)]
    public class DS4ControllerOptsDTO : IDTO<DS4ControllerOptions>
    {
        [XmlElement("Copycat")]
        public string IsCopyCatString
        {
            get => IsCopyCat.ToString();
            set
            {
                IsCopyCat = XmlDataUtilities.StrToBool(value);
            }
        }

        [XmlIgnore]
        public bool IsCopyCat
        {
            get; set;
        }

        public void MapFrom(DS4ControllerOptions source)
        {
            IsCopyCat = source.IsCopyCat;
        }

        public void MapTo(DS4ControllerOptions destination)
        {
            destination.IsCopyCat = IsCopyCat;
        }
    }
}
