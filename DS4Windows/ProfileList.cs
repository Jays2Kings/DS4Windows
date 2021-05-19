using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace DS4WinWPF
{
    public class ProfileList
    {
        private object _proLockobj = new object();
        private ObservableCollection<ProfileEntity> profileListCol =
            new ObservableCollection<ProfileEntity>();

        public ObservableCollection<ProfileEntity> ProfileListCol { get => profileListCol; set => profileListCol = value; }

        public ProfileList()
        {
            BindingOperations.EnableCollectionSynchronization(profileListCol, _proLockobj);
        }

        public void Refresh()
        {
            profileListCol.Clear();
            string[] profiles = Directory.GetFiles(DS4Windows.Global.appdatapath + @"\Profiles\");
            foreach (string s in profiles)
            {
                if (s.EndsWith(".xml"))
                {
                    ProfileEntity item = new ProfileEntity()
                    {
                        Name = Path.GetFileNameWithoutExtension(s)
                    };

                    profileListCol.Add(item);
                }
            }
        }

        public void AddProfileSort(string profilename)
        {
            int idx = 0;
            bool inserted = false;
            foreach (ProfileEntity entry in profileListCol)
            {
                if (entry.Name.CompareTo(profilename) > 0)
                {
                    profileListCol.Insert(idx, new ProfileEntity() { Name = profilename });
                    inserted = true;
                    break;
                }
                idx++;
            }

            if (!inserted)
            {
                profileListCol.Add(new ProfileEntity() { Name = profilename });
            }
        }

        public void RemoveProfile(string profile)
        {
            var selectedEntity = profileListCol.SingleOrDefault(x => x.Name == profile);
            if (selectedEntity != null)
            {
                int selectedIndex = profileListCol.IndexOf(selectedEntity);
                profileListCol.RemoveAt(selectedIndex);
            }
        }
    }
}
