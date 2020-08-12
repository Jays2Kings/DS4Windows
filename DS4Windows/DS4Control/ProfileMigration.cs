using System;
using System.IO;
using System.Text;
using System.Xml;

namespace DS4Windows
{
    public class ProfileMigration
    {
        private XmlReader profileReader;
        public XmlReader ProfileReader { get => profileReader; }

        private int configFileVersion;
        private bool usedMigration;
        public bool UsedMigration { get => usedMigration; }
        private string currentMigrationText;

        public ProfileMigration(string filePath)
        {
            FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            StreamReader innerStreamReader = new StreamReader(fileStream);
            currentMigrationText = innerStreamReader.ReadToEnd();
            innerStreamReader.Dispose();

            profileReader = XmlReader.Create(new StringReader(currentMigrationText));
            // Move stream to root element
            profileReader.MoveToContent();
            string temp = profileReader.GetAttribute("config_version");
            if (!string.IsNullOrEmpty(temp))
            {
                int.TryParse(temp, out configFileVersion);
            }

            // config_version not available in file. Assume either version 1 or 2.
            // Try to determine which version
            if (configFileVersion == 0)
            {
                DetermineProfileVersion();
            }
        }

        public bool RequiresMigration()
        {
            bool result = false;
            // Skip configFileVersion == 1 and pass profile XML as is
            if (configFileVersion > 1 && configFileVersion < Global.CONFIG_VERSION)
            {
                result = true;
            }

            return result;
        }

        public void Migrate()
        {
            if (RequiresMigration())
            {
                string migratedText;
                int tempVersion = configFileVersion;
                switch(configFileVersion)
                {
                    case 1:
                        goto default;
                    case 2:
                    case 3:
                        migratedText = Version0004Migration();
                        PrepareReaderMigration(migratedText);
                        tempVersion = 4;
                        goto default;

                    default:
                        break;
                }

                configFileVersion = tempVersion;
            }
        }

        private void PrepareReaderMigration(string migratedText)
        {
            usedMigration = true;
            // Close and flush current XmlReader instance
            profileReader.Close();
            profileReader.Dispose();

            currentMigrationText = migratedText;
            StringReader stringReader = new StringReader(currentMigrationText);
            profileReader = XmlReader.Create(stringReader);
        }

        private void DetermineProfileVersion()
        {
            bool hasAntiDeadLSTag = false;
            //int deadZoneLS = -1;

            // Move stream to root element
            profileReader.MoveToContent();
            // Skip past root element
            profileReader.Read();
            while (profileReader.Read())
            {
                /*if (profileReader.Name == "LSDeadZone" && profileReader.IsStartElement())
                {
                    string weight = profileReader.ReadElementContentAsString();
                    int.TryParse(weight, out deadZoneLS);
                }
                */
                if (profileReader.Name == "LSAntiDeadZone" && profileReader.IsStartElement())
                {
                    hasAntiDeadLSTag = true;
                    profileReader.ReadElementContentAsString();
                }
            }

            // Close and dispose current XmlReader
            profileReader.Close();
            profileReader.Dispose();

            if (hasAntiDeadLSTag)
            {
                configFileVersion = 2;
            }
            else
            {
                configFileVersion = 1;
            }

            // Start reader at zero position
            profileReader = XmlReader.Create(new StringReader(currentMigrationText));
            // Move stream to root element
            profileReader.MoveToContent();
        }

        struct GyroSmoothSettings0004
        {
            public const double DEFAULT_SMOOTH_WEIGHT = 0.5;

            public bool hasSmoothing;
            public bool hasSmoothingWeight;

            public bool useSmoothing;
            public double smoothingWeight;
        }

        private string Version0004Migration()
        {
            GyroSmoothSettings0004 gyroSmoothSettings = new GyroSmoothSettings0004()
            {
                smoothingWeight = GyroSmoothSettings0004.DEFAULT_SMOOTH_WEIGHT,
            };

            void MigrateGyroSmoothingSettings(XmlWriter xmlWriter)
            {
                // <GyroMouseSmoothingSettings>
                xmlWriter.WriteStartElement("GyroMouseSmoothingSettings");

                xmlWriter.WriteStartElement("UseSmoothing");
                xmlWriter.WriteValue(gyroSmoothSettings.useSmoothing.ToString());
                xmlWriter.WriteEndElement();

                if (gyroSmoothSettings.useSmoothing)
                {
                    xmlWriter.WriteStartElement("SmoothingMethod");
                    xmlWriter.WriteValue("weighted-average");
                    xmlWriter.WriteEndElement();
                }

                xmlWriter.WriteStartElement("SmoothingWeight");
                xmlWriter.WriteValue(gyroSmoothSettings.smoothingWeight.ToString());
                xmlWriter.WriteEndElement();

                // </GyroMouseSmoothingSettings>
                xmlWriter.WriteEndElement();
            }

            StringWriter stringWrite = new StringWriter();
            XmlWriter tempWriter = XmlWriter.Create(stringWrite, new XmlWriterSettings()
            {
                Encoding = Encoding.UTF8,
                Indent = true,
            });
            tempWriter.WriteStartDocument();
            // Move stream to root element
            profileReader.MoveToContent();
            // Skip past root element
            profileReader.Read();

            // Write replacement root element in XmlWriter
            tempWriter.WriteStartElement("DS4Windows");
            tempWriter.WriteAttributeString("app_version", Global.exeversion);
            tempWriter.WriteAttributeString("config_version", "4");

            // First pass
            while (profileReader.Read())
            {
                if (profileReader.Name == "GyroSmoothing" && profileReader.IsStartElement())
                {
                    gyroSmoothSettings.hasSmoothing = true;
                    string useSmooth = profileReader.ReadElementContentAsString();
                    bool.TryParse(useSmooth, out gyroSmoothSettings.useSmoothing);
                }
                else if (profileReader.Name == "GyroSmoothingWeight" && profileReader.IsStartElement())
                {
                    gyroSmoothSettings.hasSmoothingWeight = true;
                    string weight = profileReader.ReadElementContentAsString();
                    double.TryParse(weight, out gyroSmoothSettings.smoothingWeight);
                }
            }

            // Close and dispose current XmlReader
            profileReader.Close();
            profileReader.Dispose();

            // Prepare for second pass
            StringReader stringReader = new StringReader(currentMigrationText);
            profileReader = XmlReader.Create(stringReader);
            // Move stream to root element
            profileReader.MoveToContent();
            // Skip past root element
            profileReader.Read();

            // Second pass
            while (profileReader.Read())
            {
                if (profileReader.Name == "GyroSmoothing" && profileReader.IsStartElement())
                {
                    // Place new GyroMouseSmoothingSettings group where GyroSmoothing used to be
                    MigrateGyroSmoothingSettings(tempWriter);
                    // Consume reset of element
                    profileReader.ReadElementContentAsString();
                }
                else if (profileReader.Name == "GyroSmoothingWeight" && profileReader.IsStartElement())
                {
                    // Consume reset of element
                    profileReader.ReadElementContentAsString();
                }
                else
                {
                    tempWriter.WriteNode(profileReader, true);
                }
            }

            // End XML document and flush IO stream
            tempWriter.WriteEndDocument();
            tempWriter.Close();
            return stringWrite.ToString();
        }
    }
}
