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
