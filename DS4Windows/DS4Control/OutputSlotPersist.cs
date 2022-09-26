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

            return result;
        }

        public static bool WriteConfig(OutputSlotManager slotManager)
        {
            bool result = false;

            string output_path = Path.Combine(Global.appdatapath, CONFIG_FILENAME);
            string testStr = string.Empty;
            XmlSerializer serializer = new XmlSerializer(typeof(OutputSlotPersistDTO));
            using (Utf8StringWriter strWriter = new Utf8StringWriter())
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
        }
    }
}