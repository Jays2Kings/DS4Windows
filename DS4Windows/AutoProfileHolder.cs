using DS4Windows;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Xml;

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
            try
            {
                XmlDocument doc = new XmlDocument();

                if (!File.Exists(DS4Windows.Global.appdatapath + "\\Auto Profiles.xml"))
                    return;

                doc.Load(DS4Windows.Global.appdatapath + "\\Auto Profiles.xml");
                XmlNodeList programslist = doc.SelectNodes("Programs/Program");
                foreach (XmlNode x in programslist)
                {
                    string path = x.Attributes["path"]?.Value ?? string.Empty;
                    string title = x.Attributes["title"]?.Value ?? string.Empty;
                    AutoProfileEntity autoprof = new AutoProfileEntity(path, title);

                    XmlNode item;
                    for (int i = 0; i < ControlService.CURRENT_DS4_CONTROLLER_LIMIT; i++)
                    {
                        item = x.SelectSingleNode($"Controller{i+1}");
                        if (item != null)
                        {
                            autoprof.ProfileNames[i] = item.InnerText;
                        }
                    }

                    item = x.SelectSingleNode($"TurnOff");
                    if (item != null && bool.TryParse(item.InnerText, out bool turnoff))
                    {
                        autoprof.Turnoff = turnoff;
                    }

                    autoProfileColl.Add(autoprof);
                    //autoProfileDict.Add(path, autoprof);
                }
            }
            catch (Exception) { }
        }

        public bool Save(string m_Profile)
        {
            XmlDocument doc = new XmlDocument();
            XmlNode Node;
            bool saved = true;
            try
            {
                Node = doc.CreateXmlDeclaration("1.0", "utf-8", string.Empty);
                doc.AppendChild(Node);

                Node = doc.CreateComment(string.Format(" Auto-Profile Configuration Data. {0} ", DateTime.Now));
                doc.AppendChild(Node);

                Node = doc.CreateWhitespace("\r\n");
                doc.AppendChild(Node);

                Node = doc.CreateNode(XmlNodeType.Element, "Programs", "");
                doc.AppendChild(Node);
                foreach (AutoProfileEntity entity in autoProfileColl)
                {
                    XmlElement el = doc.CreateElement("Program");
                    el.SetAttribute("path", entity.Path);
                    if (!string.IsNullOrEmpty(entity.Title))
                    {
                        el.SetAttribute("title", entity.Title);
                    }

                    el.AppendChild(doc.CreateElement("Controller1")).InnerText = entity.ProfileNames[0];
                    el.AppendChild(doc.CreateElement("Controller2")).InnerText = entity.ProfileNames[1];
                    el.AppendChild(doc.CreateElement("Controller3")).InnerText = entity.ProfileNames[2];
                    el.AppendChild(doc.CreateElement("Controller4")).InnerText = entity.ProfileNames[3];
                    if (ControlService.USING_MAX_CONTROLLERS)
                    {
                        el.AppendChild(doc.CreateElement("Controller5")).InnerText = entity.ProfileNames[4];
                        el.AppendChild(doc.CreateElement("Controller6")).InnerText = entity.ProfileNames[5];
                        el.AppendChild(doc.CreateElement("Controller7")).InnerText = entity.ProfileNames[6];
                        el.AppendChild(doc.CreateElement("Controller8")).InnerText = entity.ProfileNames[7];
                    }
                    el.AppendChild(doc.CreateElement("TurnOff")).InnerText = entity.Turnoff.ToString();

                    Node.AppendChild(el);
                }

                doc.Save(m_Profile);
            }
            catch (Exception) { saved = false; }
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
