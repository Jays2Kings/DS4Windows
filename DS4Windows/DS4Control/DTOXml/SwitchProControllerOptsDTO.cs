using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using DS4Windows;

namespace DS4WinWPF.DS4Control.DTOXml
{
    [XmlRoot(SwitchProControllerOptions.XML_ELEMENT_NAME)]
    public class SwitchProControllerOptsDTO : IDTO<SwitchProControllerOptions>
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

        public void MapFrom(SwitchProControllerOptions source)
        {
            EnableHomeLED = source.EnableHomeLED;
        }

        public void MapTo(SwitchProControllerOptions destination)
        {
            destination.EnableHomeLED = EnableHomeLED;
        }
    }
}
