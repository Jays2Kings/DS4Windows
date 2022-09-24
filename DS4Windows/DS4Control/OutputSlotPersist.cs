using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using DS4Windows;
using DS4WinWPF.DS4Control.DTOXml;

namespace DS4WinWPF.DS4Control
{
    public static class OutputSlotPersist
    {
        private const string CONFIG_FILENAME = "OutputSlots.xml";

        public static bool ReadConfig(OutputSlotManager slotManager)
        {
            bool result = false;

            string output_path = Path.Combine(Global.appdatapath, CONFIG_FILENAME);
            if (File.Exists(output_path))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(OutputSlotPersistDTO));
                using StreamReader sr = new StreamReader(output_path);
                try
                {
                    OutputSlotPersistDTO dto = serializer.Deserialize(sr) as OutputSlotPersistDTO;
                    dto.MapTo(slotManager);
                    result = true;
                }
                catch (InvalidOperationException)
                {
                    result = false;
                }
            }

            //string output_path = Path.Combine(Global.appdatapath, CONFIG_FILENAME);
            //if (File.Exists(output_path))
            //{
            //    XmlDocument m_Xdoc = new XmlDocument();
            //    try { m_Xdoc.Load(output_path); }
            //    catch (UnauthorizedAccessException) { }
            //    catch (XmlException) { }

            //    XmlElement rootElement = m_Xdoc.DocumentElement;
            //    if (rootElement == null) return false;

            //    foreach(XmlElement element in rootElement.GetElementsByTagName("Slot"))
            //    {
            //        OutSlotDevice tempDev = null;
            //        string temp = element.GetAttribute("idx");
            //        if (int.TryParse(temp, out int idx) && idx >= 0 && idx <= 3)
            //        {
            //            tempDev = slotManager.OutputSlots[idx];
            //        }

            //        if (tempDev != null)
            //        {
            //            tempDev.CurrentReserveStatus = OutSlotDevice.ReserveStatus.Permanent;
            //            XmlNode tempNode = element.SelectSingleNode("DeviceType");
            //            if (tempNode != null && Enum.TryParse(tempNode.InnerText, out OutContType tempType))
            //            {
            //                tempDev.PermanentType = tempType;
            //            }
            //        }
            //    }

            //    result = true;
            //}

            return result;
        }

        public static bool WriteConfig(OutputSlotManager slotManager)
        {
            bool result = false;

            string output_path = Path.Combine(Global.appdatapath, CONFIG_FILENAME);
            string testStr = string.Empty;
            XmlSerializer serializer = new XmlSerializer(typeof(OutputSlotPersistDTO));
            using (StringWriter strWriter = new StringWriter())
            {
                using XmlWriter xmlWriter = XmlWriter.Create(strWriter,
                    new XmlWriterSettings()
                    {
                        Encoding = Encoding.UTF8,
                        Indent = true,
                    });

                // Write header explicitly
                xmlWriter.WriteStartDocument();
                xmlWriter.WriteComment(string.Format(" Made with DS4Windows version {0} ", Global.exeversion));
                xmlWriter.WriteWhitespace("\r\n");
                xmlWriter.WriteWhitespace("\r\n");

                // Write root element and children
                OutputSlotPersistDTO dto = new OutputSlotPersistDTO();
                dto.MapFrom(slotManager);
                // Omit xmlns:xsi and xmlns:xsd from output
                serializer.Serialize(xmlWriter, dto,
                    new XmlSerializerNamespaces(new[] { XmlQualifiedName.Empty }));
                xmlWriter.Flush();
                xmlWriter.Close();

                testStr = strWriter.ToString();
                //Trace.WriteLine("TEST OUTPUT");
                //Trace.WriteLine(testStr);
            }

            try
            {
                using (StreamWriter sw = new StreamWriter(output_path, false))
                {
                    sw.Write(testStr);
                }
            }
            catch (UnauthorizedAccessException)
            {
                result = false;
            }

            return result;

            //XmlDocument m_Xdoc = new XmlDocument();
            //XmlNode rootNode;
            //rootNode = m_Xdoc.CreateXmlDeclaration("1.0", "utf-8", string.Empty);
            //m_Xdoc.AppendChild(rootNode);

            //rootNode = m_Xdoc.CreateComment(string.Format(" Made with DS4Windows version {0} ", Global.exeversion));
            //m_Xdoc.AppendChild(rootNode);

            //rootNode = m_Xdoc.CreateWhitespace("\r\n");
            //m_Xdoc.AppendChild(rootNode);

            //XmlElement baseElement = m_Xdoc.CreateElement("OutputSlots", null);
            //baseElement.SetAttribute("app_version", Global.exeversion);

            //int idx = 0;
            //foreach (OutSlotDevice dev in slotManager.OutputSlots)
            //{
            //    if (dev.CurrentReserveStatus == OutSlotDevice.ReserveStatus.Permanent)
            //    {
            //        XmlElement slotElement = m_Xdoc.CreateElement("Slot");
            //        slotElement.SetAttribute("idx", idx.ToString());

            //        XmlElement propElement;
            //        propElement = m_Xdoc.CreateElement("DeviceType");
            //        propElement.InnerText = dev.PermanentType.ToString();
            //        slotElement.AppendChild(propElement);

            //        baseElement.AppendChild(slotElement);
            //    }

            //    idx++;
            //}

            //m_Xdoc.AppendChild(baseElement);

            //string output_path = Path.Combine(Global.appdatapath, CONFIG_FILENAME);
            //try { m_Xdoc.Save(output_path); result = true; }
            //catch (UnauthorizedAccessException) { result = false; }

            //return result;
        }
    }
}