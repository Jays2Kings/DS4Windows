using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Xml;
using System.Xml.Serialization;
using DS4Windows;
using DS4WinWPF.DS4Control.DTOXml;

namespace DS4WinWPF
{
    public class AutoProfileHolder
    {
        private object _colLockobj = new object();
        private ObservableCollection<AutoProfileEntity> autoProfileColl;
        public ObservableCollection<AutoProfileEntity> AutoProfileColl { get => autoProfileColl; }
        //public Dictionary<string, AutoProfileEntity> AutoProfileDict { get => autoProfileDict; }

        //private Dictionary<string, AutoProfileEntity> autoProfileDict;

        public AutoProfileHolder()
        {
            autoProfileColl = new ObservableCollection<AutoProfileEntity>();
            //autoProfileDict = new Dictionary<string, AutoProfileEntity>();
            Load();

            BindingOperations.EnableCollectionSynchronization(autoProfileColl, _colLockobj);
        }

        private void Load()
        {
            string configFile = Path.Combine(Global.appdatapath, "Auto Profiles.xml");
            if (!File.Exists(configFile))
                return;

            XmlSerializer serializer = new XmlSerializer(typeof(AutoProfilesDTO));
            using StreamReader sr = new StreamReader(configFile);
            try
            {
                AutoProfilesDTO dto = serializer.Deserialize(sr) as AutoProfilesDTO;
                dto.MapTo(this);
            }
            catch (InvalidOperationException) {}
            catch (XmlException) {}
        }

        public bool Save(string m_Profile)
        {
            bool saved = true;

            string output_path = m_Profile;
            string testStr = string.Empty;
            XmlSerializer serializer = new XmlSerializer(typeof(AutoProfilesDTO));
            using (Utf8StringWriter strWriter = new Utf8StringWriter())
            {
                using XmlWriter xmlWriter = XmlWriter.Create(strWriter,
                    new XmlWriterSettings()
                    {
                        Encoding = Encoding.UTF8,
                        Indent = true,
                    });

                // Write header explicitly
                //xmlWriter.WriteStartDocument();
                xmlWriter.WriteComment(string.Format(" Auto-Profile Configuration Data. {0} ", DateTime.Now));
                xmlWriter.WriteWhitespace("\r\n");
                xmlWriter.WriteWhitespace("\r\n");

                // Write root element and children
                AutoProfilesDTO dto = new AutoProfilesDTO();
                dto.MapFrom(this);
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
                saved = false;
            }

            return saved;
        }

        public void Remove(AutoProfileEntity item)
        {
            //autoProfileDict.Remove(item.Path);
            autoProfileColl.Remove(item);
        }
    }

    public class AutoProfileEntity
    {
        public string path = string.Empty;
        public string title = string.Empty;
        private string path_lowercase;
        private string title_lowercase;
        private bool turnoff;
        private string[] profileNames = new string[DS4Windows.Global.MAX_DS4_CONTROLLER_COUNT] { string.Empty, string.Empty,
            string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty };
        public const string NONE_STRING = "(none)";

        public string Path { get => path; set => SetSearchPath(value); }
        public string Title { get => title; set => SetSearchTitle(value); }
        public bool Turnoff { get => turnoff; set => turnoff = value; }
        public string[] ProfileNames { get => profileNames; set => profileNames = value; }

        public AutoProfileEntity(string pathStr, string titleStr)
        {
            // Initialize autoprofile search keywords(xxx_tolower).To improve performance the search keyword is pre - calculated in xxx_tolower variables,
            // so autoprofile timer thread doesn't have to create substrings/replace/tolower string instances every second over and over again.
            SetSearchPath(pathStr);
            SetSearchTitle(titleStr);
        }

        public bool IsMatch(string searchPath, string searchTitle)
        {
            bool bPathMatched = true;
            bool bTitleMwatched = true;

            if (!string.IsNullOrEmpty(path_lowercase))
            {
                bPathMatched = (path_lowercase == searchPath
                    || (path[0] == '^' && searchPath.StartsWith(path_lowercase))
                    || (path[path.Length - 1] == '$' && searchPath.EndsWith(path_lowercase))
                    || (path[0] == '*' && searchPath.Contains(path_lowercase))
                   );
            }

            if (bPathMatched && !string.IsNullOrEmpty(title_lowercase))
            {
                bTitleMwatched = (title_lowercase == searchTitle
                    || (title[0] == '^' && searchTitle.StartsWith(title_lowercase))
                    || (title[title.Length - 1] == '$' && searchTitle.EndsWith(title_lowercase))
                    || (title[0] == '*' && searchTitle.Contains(title_lowercase))
                   );
            }

            // If both path and title defined in autoprofile entry then do AND condition (ie. both path and title should match)
            return bPathMatched && bTitleMwatched;
        }

        private void SetSearchPath(string pathStr)
        {
            if (!string.IsNullOrEmpty(pathStr))
            {
                path = pathStr;
                path_lowercase = path.ToLower().Replace('/', '\\');

                if (path.Length >= 2)
                {
                    if (path[0] == '^') path_lowercase = path_lowercase.Substring(1);
                    else if (path[path.Length - 1] == '$') path_lowercase = path_lowercase.Substring(0, path_lowercase.Length - 1);
                    else if (path[0] == '*') path_lowercase = path_lowercase.Substring(1);
                }
            }
            else path = path_lowercase = string.Empty;
        }

        private void SetSearchTitle(string titleStr)
        {
            if (!string.IsNullOrEmpty(titleStr))
            {
                title = titleStr;
                title_lowercase = title.ToLower();

                if (title.Length >= 2)
                {
                    if (title[0] == '^') title_lowercase = title_lowercase.Substring(1);
                    else if (title[title.Length - 1] == '$') title_lowercase = title_lowercase.Substring(0, title_lowercase.Length - 1);
                    else if (title[0] == '*') title_lowercase = title_lowercase.Substring(1);
                }
            }
            else title = title_lowercase = string.Empty;
        }
    }
}
