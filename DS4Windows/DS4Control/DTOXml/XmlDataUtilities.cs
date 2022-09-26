using System;
using System.Collections.Generic;
using System.IO;
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

    /// <summary>
    /// StringWriter derived class used to ensure XML encoding is declared
    /// as utf-8 rather than utf-16
    /// </summary>
    internal sealed class Utf8StringWriter : StringWriter
    {
        public override Encoding Encoding => Encoding.UTF8;
    }
}
