using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using DS4Windows;

namespace DS4WinWPF.DS4Forms.ViewModels
{
    public class RenameProfileViewModel
    {
        private string profileName;
        public string ProfileName
        {
            get => profileName;
            set
            {
                profileName = value;
                ProfileNameChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler ProfileNameChanged;

        public bool ProfileFileExists()
        {
            string filePath = Path.Combine(Global.appdatapath,
                "Profiles", $"{profileName}.xml");
            return File.Exists(filePath);
        }
    }
}
