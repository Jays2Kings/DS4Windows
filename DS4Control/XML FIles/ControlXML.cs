using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS4Control.XML_FIles
{
    struct Key
    {
        public string name { get; set; }
        public int type { get; set; }
    }

    public class ControlXML
    {
        public string Name { get; set; }

        public int Key { get; set; }
        public int KeyType { get; set; }
        public List<int> Macro { get; set; }
        public string Button { get; set; }
        public string Extras { get; set; }
    }
}
