using DS4Control.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Xml;

namespace DS4Windows
{
    public class AutoProfileXML : XmlDocument
    {
        public AutoProfileXML(string location)
            : base()
        {
            //Is it ok run exception potential code in the constructor in C#?
            base.Load(location);
        }

        public List<string> ProgramPaths
        {
            get
            {
                List<string> paths = new List<string>();

                XmlNodeList programslist = this.SelectNodes("Programs/Program");
                foreach (XmlNode x in programslist)
                    paths.Add(x.Attributes["path"].Value);

                return paths;
            }
        }

        public List<string>[] Profiles
        {
            get
            {
                var proprofiles = new List<string>[4]
                { 
                    new List<string>(), new List<string>(),
                    new List<string>(), new List<string>() 
                };

                foreach (string s in this.ProgramPaths)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        proprofiles[i].Add(this.SelectSingleNode("/Programs/Program[@path=\"" + s + "\"]"
                            + "/Controller" + (i + 1)).InnerText);
                    }
                }

                return proprofiles;
            }
        }
    }
}
