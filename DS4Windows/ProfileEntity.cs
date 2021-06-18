using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS4WinWPF
{
    public class ProfileEntity
    {
        private string name;
        public string Name
        {
            get => name;
            set
            {
                if (name == value) return;
                name = value;
                NameChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public event EventHandler NameChanged;
        public event EventHandler ProfileSaved;
        public event EventHandler ProfileDeleted;

        public void DeleteFile()
        {
            if (!string.IsNullOrWhiteSpace(name))
            {
                string filepath = DS4Windows.Global.appdatapath + @"\Profiles\" + name + ".xml";
                if (File.Exists(filepath))
                {
                    File.Delete(filepath);
                    ProfileDeleted?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public void SaveProfile(int deviceNum)
        {
            if (!string.IsNullOrWhiteSpace(name))
            {
                DS4Windows.Global.SaveProfile(deviceNum, name);
                DS4Windows.Global.CacheExtraProfileInfo(deviceNum);
            }
        }

        public void FireSaved()
        {
            ProfileSaved?.Invoke(this, EventArgs.Empty);
        }

        public void RenameProfile(string newProfileName)
        {
            string oldFilePath = Path.Combine(DS4Windows.Global.appdatapath,
                "Profiles", $"{name}.xml");

            string newFilePath = Path.Combine(DS4Windows.Global.appdatapath,
                "Profiles", $"{newProfileName}.xml");

            if (File.Exists(oldFilePath) && !File.Exists(newFilePath))
            {
                File.Move(oldFilePath, newFilePath);
                // Send NameChanged event so controls get updated with new name
                Name = newProfileName;
            }
        }
    }
}
