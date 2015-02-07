using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS4Windows.XML_Files
{
    class ProfilesDirectory : List<string>
    {
        public ProfilesDirectory(string directory)
        {
            this.AddRange(Directory.GetFiles(directory)
                .Where(x => x.EndsWith(".xml"))
                .Select(x => Path.GetFileNameWithoutExtension(x)));
        }

        public void CreateDefaultProfile()
        {

        }

    }
}
