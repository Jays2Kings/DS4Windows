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
