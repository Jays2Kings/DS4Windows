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