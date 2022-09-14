using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS4WinWPF.DS4Control.DTOXml
{
    internal static class XmlDataUtilities
    {
        public static bool StrToBool(string str)
        {
            bool.TryParse(str, out bool result);
            return result;
        }
    }
}
