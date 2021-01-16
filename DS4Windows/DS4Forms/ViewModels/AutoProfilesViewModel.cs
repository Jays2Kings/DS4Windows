using DS4Windows;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace DS4WinWPF.DS4Forms.ViewModels
{
    public class AutoProfilesViewModel
    {
        private object _colLockobj = new object();
        private ObservableCollection<ProgramItem> programColl;
        private AutoProfileHolder autoProfileHolder;
        private ProfileList profileList;
        private int selectedIndex = -1;
        private ProgramItem selectedItem;
        private HashSet<string> existingapps;

        public ObservableCollection<ProgramItem> ProgramColl { get => programColl; }
        
        public AutoProfileHolder AutoProfileHolder { get => autoProfileHolder; }

        public int SelectedIndex { get => selectedIndex; set => selectedIndex = value; }
        public ProgramItem SelectedItem
        {
            get => selectedItem;
            set
            {
                selectedItem = value;
                CurrentItemChange?.Invoke(this, value);
            }
        }

        public ProfileList ProfileList { get => profileList; }

        public delegate void CurrentItemChangeHandler(AutoProfilesViewModel sender, ProgramItem item);
        public event CurrentItemChangeHandler CurrentItemChange;

        public event EventHandler SearchFinished;
        public delegate void AutoProfileStateHandler(AutoProfilesViewModel sender, bool state);
        public event AutoProfileStateHandler AutoProfileSystemChange;

        public bool RevertDefaultProfileOnUnknown
        {
            get => DS4Windows.Global.AutoProfileRevertDefaultProfile;
            set => DS4Windows.Global.AutoProfileRevertDefaultProfile = value;
        }

        public bool UsingExpandedControllers
        {
            get => ControlService.USING_MAX_CONTROLLERS;
        }

        public Visibility ExpandedControllersVisible
        {
            get
            {
                Visibility temp = Visibility.Visible;
                if (!ControlService.USING_MAX_CONTROLLERS)
                {
                    temp = Visibility.Collapsed;
                }

                return temp;
            }
        }

        public AutoProfilesViewModel(AutoProfileHolder autoProfileHolder, ProfileList profileList)
        {
            programColl = new ObservableCollection<ProgramItem>();
            existingapps = new HashSet<string>();
            this.autoProfileHolder = autoProfileHolder;
            this.profileList = profileList;
            PopulateCurrentEntries();

            BindingOperations.EnableCollectionSynchronization(programColl, _colLockobj);
        }

        private void PopulateCurrentEntries()
        {
            foreach(AutoProfileEntity entry in autoProfileHolder.AutoProfileColl)
            {
                ProgramItem item = new ProgramItem(entry.Path, entry);

                programColl.Add(item);
                existingapps.Add(entry.Path);
            }
        }

        public void RemoveUnchecked()
        {
            AutoProfileSystemChange?.Invoke(this, false);
            programColl.Clear();
            existingapps.Clear();
            PopulateCurrentEntries();
            AutoProfileSystemChange?.Invoke(this, true);
        }

        public async void AddProgramsFromStartMenu()
        {
            AutoProfileSystemChange?.Invoke(this, false);
            await Task.Run(() =>
            {
                AddFromStartMenu(Environment.GetFolderPath(Environment.SpecialFolder.StartMenu) + "\\Programs");
            });

            SearchFinished?.Invoke(this, EventArgs.Empty);
            AutoProfileSystemChange?.Invoke(this, true);
        }

        public async void AddProgramsFromSteam(string location)
        {
            AutoProfileSystemChange?.Invoke(this, false);
            await Task.Run(() =>
            {
                AddAppsFromLocation(location);
            });

            SearchFinished?.Invoke(this, EventArgs.Empty);
            AutoProfileSystemChange?.Invoke(this, true);
        }

        public async void AddProgramsFromDir(string location)
        {
            AutoProfileSystemChange?.Invoke(this, false);
            await Task.Run(() =>
            {
                AddAppsFromLocation(location);
            });

            SearchFinished?.Invoke(this, EventArgs.Empty);
            AutoProfileSystemChange?.Invoke(this, true);
        }

        public async void AddProgramExeLocation(string location)
        {
            AutoProfileSystemChange?.Invoke(this, false);
            await Task.Run(() =>
            {
                AddAppExeLocation(location);
            });

            SearchFinished?.Invoke(this, EventArgs.Empty);
            AutoProfileSystemChange?.Invoke(this, true);
        }

        private void AddFromStartMenu(string path)
        {
            List<string> lnkpaths = new List<string>();
            lnkpaths.AddRange(Directory.GetFiles(path, "*.lnk", SearchOption.AllDirectories));
            lnkpaths.AddRange(Directory.GetFiles(Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu) + "\\Programs", "*.lnk", SearchOption.AllDirectories));
            List<string> exepaths = new List<string>();
            foreach(string link in lnkpaths)
            {
                string target = GetTargetPath(link);
                exepaths.Add(target);
            }

            ScanApps(exepaths);
        }

        private void AddAppsFromLocation(string path)
        {
            List<string> exepaths = new List<string>();
            exepaths.AddRange(Directory.GetFiles(path, "*.exe", SearchOption.AllDirectories));
            ScanApps(exepaths);
        }

        private void AddAppExeLocation(string path)
        {
            List<string> exepaths = new List<string>();
            exepaths.Add(path);
            ScanApps(exepaths, checkexisting: false, skipsetupapps: false);
        }

        private void ScanApps(List<string> exepaths, bool checkexisting = true,
            bool skipsetupapps = true)
        {
            foreach (string target in exepaths)
            {
                bool skip = !File.Exists(target) || Path.GetExtension(target).ToLower() != ".exe";
                skip = skip || (skipsetupapps && (target.Contains("etup") || target.Contains("dotnet") || target.Contains("SETUP")
                    || target.Contains("edist") || target.Contains("nstall") || string.IsNullOrEmpty(target)));
                skip = skip || (checkexisting && existingapps.Contains(target));
                if (!skip)
                {
                    ProgramItem item = new ProgramItem(target);
                    /*if (autoProfileHolder.AutoProfileDict.TryGetValue(target, out AutoProfileEntity autoEntity))
                    {
                        item.MatchedAutoProfile = autoEntity;
                    }
                    */

                    programColl.Add(item);
                    existingapps.Add(target);
                }
            }
        }

        public void CreateAutoProfileEntry(ProgramItem item)
        {
            if (item.MatchedAutoProfile == null)
            {
                AutoProfileEntity tempEntry = new AutoProfileEntity(item.Path, item.Title);
                tempEntry.Turnoff = item.Turnoff;
                int tempindex = item.SelectedIndexCon1;
                tempEntry.ProfileNames[0] = tempindex > 0 ? profileList.ProfileListCol[tempindex - 1].Name :
                    AutoProfileEntity.NONE_STRING;

                tempindex = item.SelectedIndexCon2;
                tempEntry.ProfileNames[1] = tempindex > 0 ? profileList.ProfileListCol[tempindex - 1].Name :
                    AutoProfileEntity.NONE_STRING;

                tempindex = item.SelectedIndexCon3;
                tempEntry.ProfileNames[2] = tempindex > 0 ? profileList.ProfileListCol[tempindex - 1].Name :
                    AutoProfileEntity.NONE_STRING;

                tempindex = item.SelectedIndexCon4;
                tempEntry.ProfileNames[3] = tempindex > 0 ? profileList.ProfileListCol[tempindex - 1].Name :
                    AutoProfileEntity.NONE_STRING;

                if (UsingExpandedControllers)
                {
                    tempindex = item.SelectedIndexCon5;
                    tempEntry.ProfileNames[4] = tempindex > 0 ? profileList.ProfileListCol[tempindex - 1].Name :
                        AutoProfileEntity.NONE_STRING;

                    tempindex = item.SelectedIndexCon6;
                    tempEntry.ProfileNames[5] = tempindex > 0 ? profileList.ProfileListCol[tempindex - 1].Name :
                        AutoProfileEntity.NONE_STRING;

                    tempindex = item.SelectedIndexCon7;
                    tempEntry.ProfileNames[6] = tempindex > 0 ? profileList.ProfileListCol[tempindex - 1].Name :
                        AutoProfileEntity.NONE_STRING;

                    tempindex = item.SelectedIndexCon8;
                    tempEntry.ProfileNames[7] = tempindex > 0 ? profileList.ProfileListCol[tempindex - 1].Name :
                        AutoProfileEntity.NONE_STRING;
                }

                item.MatchedAutoProfile = tempEntry;
                autoProfileHolder.AutoProfileColl.Add(item.MatchedAutoProfile);
            }
        }

        public void PersistAutoProfileEntry(ProgramItem item)
        {
            if (item.MatchedAutoProfile != null)
            {
                AutoProfileEntity tempEntry = item.MatchedAutoProfile;
                int tempindex = item.SelectedIndexCon1;
                tempEntry.ProfileNames[0] = tempindex > 0 ? profileList.ProfileListCol[tempindex - 1].Name :
                    AutoProfileEntity.NONE_STRING;

                tempindex = item.SelectedIndexCon2;
                tempEntry.ProfileNames[1] = tempindex > 0 ? profileList.ProfileListCol[tempindex - 1].Name :
                    AutoProfileEntity.NONE_STRING;

                tempindex = item.SelectedIndexCon3;
                tempEntry.ProfileNames[2] = tempindex > 0 ? profileList.ProfileListCol[tempindex - 1].Name :
                    AutoProfileEntity.NONE_STRING;

                tempindex = item.SelectedIndexCon4;
                tempEntry.ProfileNames[3] = tempindex > 0 ? profileList.ProfileListCol[tempindex - 1].Name :
                    AutoProfileEntity.NONE_STRING;

                if (UsingExpandedControllers)
                {
                    tempindex = item.SelectedIndexCon5;
                    tempEntry.ProfileNames[4] = tempindex > 0 ? profileList.ProfileListCol[tempindex - 1].Name :
                        AutoProfileEntity.NONE_STRING;

                    tempindex = item.SelectedIndexCon6;
                    tempEntry.ProfileNames[5] = tempindex > 0 ? profileList.ProfileListCol[tempindex - 1].Name :
                        AutoProfileEntity.NONE_STRING;

                    tempindex = item.SelectedIndexCon7;
                    tempEntry.ProfileNames[6] = tempindex > 0 ? profileList.ProfileListCol[tempindex - 1].Name :
                        AutoProfileEntity.NONE_STRING;

                    tempindex = item.SelectedIndexCon8;
                    tempEntry.ProfileNames[7] = tempindex > 0 ? profileList.ProfileListCol[tempindex - 1].Name :
                        AutoProfileEntity.NONE_STRING;
                }
            }
        }

        public void RemoveAutoProfileEntry(ProgramItem item)
        {
            autoProfileHolder.AutoProfileColl.Remove(item.MatchedAutoProfile);
            item.MatchedAutoProfile = null;
        }

        private string GetTargetPath(string filePath)
        {
            string targetPath = ResolveMsiShortcut(filePath);
            if (targetPath == null)
            {
                targetPath = ResolveShortcut(filePath);
            }

            return targetPath;
        }

        public string ResolveShortcutAndArgument(string filePath)
        {
            Type t = Type.GetTypeFromCLSID(new Guid("72C24DD5-D70A-438B-8A42-98424B88AFB8")); // Windows Script Host Shell Object
            dynamic shell = Activator.CreateInstance(t);
            string result;

            try
            {
                var shortcut = shell.CreateShortcut(filePath);
                result = shortcut.TargetPath + " " + shortcut.Arguments;
                Marshal.FinalReleaseComObject(shortcut);
            }
            catch (COMException)
            {
                // A COMException is thrown if the file is not a valid shortcut (.lnk) file 
                result = null;
            }
            finally
            {
                Marshal.FinalReleaseComObject(shell);
            }

            return result;
        }

        public string ResolveMsiShortcut(string file)
        {
            StringBuilder product = new StringBuilder(NativeMethods2.MaxGuidLength + 1);
            StringBuilder feature = new StringBuilder(NativeMethods2.MaxFeatureLength + 1);
            StringBuilder component = new StringBuilder(NativeMethods2.MaxGuidLength + 1);

            NativeMethods2.MsiGetShortcutTarget(file, product, feature, component);

            int pathLength = NativeMethods2.MaxPathLength;
            StringBuilder path = new StringBuilder(pathLength);

            NativeMethods2.InstallState installState = NativeMethods2.MsiGetComponentPath(product.ToString(), component.ToString(), path, ref pathLength);
            if (installState == NativeMethods2.InstallState.Local)
            {
                return path.ToString();
            }
            else
            {
                return null;
            }
        }

        public string ResolveShortcut(string filePath)
        {
            Type t = Type.GetTypeFromCLSID(new Guid("72C24DD5-D70A-438B-8A42-98424B88AFB8")); // Windows Script Host Shell Object
            dynamic shell = Activator.CreateInstance(t);
            string result;

            try
            {
                var shortcut = shell.CreateShortcut(filePath);
                result = shortcut.TargetPath;
                Marshal.FinalReleaseComObject(shortcut);
            }
            catch (COMException)
            {
                // A COMException is thrown if the file is not a valid shortcut (.lnk) file 
                result = null;
            }
            finally
            {
                Marshal.FinalReleaseComObject(shell);
            }

            return result;
        }

        public bool MoveItemUpDown(ProgramItem item, int moveDirection)
        {
            // Move autoprofile item up (-1) or down (1) both in listView (programColl) and in autoProfileHolder data structure (will be written into AutoProfiles.xml file)
            bool itemMoved = true;
            int oldIdx = programColl.IndexOf(item);

            if (moveDirection == -1 && oldIdx > 0 && oldIdx < autoProfileHolder.AutoProfileColl.Count)
            {
                programColl.Move(oldIdx, oldIdx - 1);
                autoProfileHolder.AutoProfileColl.Move(oldIdx, oldIdx - 1);
            }
            else if (moveDirection == 1 && oldIdx >= 0 && oldIdx < programColl.Count - 1 && oldIdx < autoProfileHolder.AutoProfileColl.Count - 1)
            {    
                programColl.Move(oldIdx, oldIdx + 1);
                autoProfileHolder.AutoProfileColl.Move(oldIdx, oldIdx + 1);
            }
            else
                itemMoved = false;

            return itemMoved;
        }
    }

    public class ProgramItem
    {
        private string path;
        private string path_lowercase;
        private string filename;
        private string title;
        private string title_lowercase;
        private AutoProfileEntity matchedAutoProfile;
        private ImageSource exeicon;
        private bool turnoff;

        public string Path { get => path;
            set
            {
                if (path == value) return;
                path = value;
                if (matchedAutoProfile != null)
                {
                    matchedAutoProfile.Path = value;
                }
            }
        }

        public string Title { get => title;
            set
            {
                if (title == value) return;
                title = value;
                if (matchedAutoProfile != null)
                {
                    matchedAutoProfile.Title = value;
                }

                TitleChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public event EventHandler TitleChanged;
        public AutoProfileEntity MatchedAutoProfile
        {
            get => matchedAutoProfile;
            set
            {
                matchedAutoProfile = value;
                if (matchedAutoProfile != null)
                {
                    title = matchedAutoProfile.Title ?? string.Empty;
                    title_lowercase = title.ToLower();
                }

                MatchedAutoProfileChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler MatchedAutoProfileChanged;
        public delegate void AutoProfileHandler(ProgramItem sender, bool added);
        public event AutoProfileHandler AutoProfileAction;
        public string Filename { get => filename;  }
        public ImageSource Exeicon { get => exeicon; }

        public bool Turnoff
        {
            get
            {
                bool result = turnoff;
                if (matchedAutoProfile != null)
                {
                    result = matchedAutoProfile.Turnoff;
                }

                return result;
            }
            set
            {
                turnoff = value;
                if (matchedAutoProfile != null)
                {
                    matchedAutoProfile.Turnoff = value;
                }

                TurnoffChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler TurnoffChanged;

        public bool Exists
        {
            get => matchedAutoProfile != null;
        }
        public event EventHandler ExistsChanged;

        private int selectedIndexCon1 = 0;
        private int selectedIndexCon2 = 0;
        private int selectedIndexCon3 = 0;
        private int selectedIndexCon4 = 0;
        private int selectedIndexCon5 = 0;
        private int selectedIndexCon6 = 0;
        private int selectedIndexCon7 = 0;
        private int selectedIndexCon8 = 0;

        public int SelectedIndexCon1
        {
            get => selectedIndexCon1;
            set
            {
                if (selectedIndexCon1 == value) return;
                selectedIndexCon1 = value;
            }
        }

        public int SelectedIndexCon2
        {
            get => selectedIndexCon2;
            set
            {
                if (selectedIndexCon2 == value) return;
                selectedIndexCon2 = value;
            }
        }
        public int SelectedIndexCon3
        {
            get => selectedIndexCon3;
            set
            {
                if (selectedIndexCon3 == value) return;
                selectedIndexCon3 = value;
            }
        }
        public int SelectedIndexCon4
        {
            get => selectedIndexCon4;
            set
            {
                if (selectedIndexCon4 == value) return;
                selectedIndexCon4 = value;
            }
        }

        public int SelectedIndexCon5
        {
            get => selectedIndexCon5;
            set
            {
                if (selectedIndexCon5 == value) return;
                selectedIndexCon5 = value;
            }
        }

        public int SelectedIndexCon6
        {
            get => selectedIndexCon6;
            set
            {
                if (selectedIndexCon6 == value) return;
                selectedIndexCon6 = value;
            }
        }

        public int SelectedIndexCon7
        {
            get => selectedIndexCon7;
            set
            {
                if (selectedIndexCon7 == value) return;
                selectedIndexCon7 = value;
            }
        }

        public int SelectedIndexCon8
        {
            get => selectedIndexCon8;
            set
            {
                if (selectedIndexCon8 == value) return;
                selectedIndexCon8 = value;
            }
        }

        public ProgramItem(string path, AutoProfileEntity autoProfileEntity = null)
        {
            this.path = path;
            this.path_lowercase = path.ToLower();
            filename = System.IO.Path.GetFileNameWithoutExtension(path);
            this.matchedAutoProfile = autoProfileEntity;
            if (autoProfileEntity != null)
            {
                title = autoProfileEntity.Title;
                title_lowercase = title.ToLower();
                turnoff = autoProfileEntity.Turnoff;
            }

            if (File.Exists(path))
            {
                using (Icon ico = Icon.ExtractAssociatedIcon(path))
                {
                    exeicon = Imaging.CreateBitmapSourceFromHIcon(ico.Handle, Int32Rect.Empty,
                        BitmapSizeOptions.FromEmptyOptions());
                    exeicon.Freeze();
                }
            }

            MatchedAutoProfileChanged += ProgramItem_MatchedAutoProfileChanged;
        }

        private void ProgramItem_MatchedAutoProfileChanged(object sender, EventArgs e)
        {
            if (matchedAutoProfile == null)
            {
                selectedIndexCon1 = 0;
                selectedIndexCon2 = 0;
                selectedIndexCon3 = 0;
                selectedIndexCon4 = 0;
                selectedIndexCon5 = 0;
                selectedIndexCon6 = 0;
                selectedIndexCon7 = 0;
                selectedIndexCon8 = 0;
            }

            ExistsChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    [SuppressUnmanagedCodeSecurity]
    class NativeMethods2
    {
        [DllImport("msi.dll", CharSet = CharSet.Auto)]
        public static extern uint MsiGetShortcutTarget(string targetFile, StringBuilder productCode, StringBuilder featureID, StringBuilder componentCode);

        [DllImport("msi.dll", CharSet = CharSet.Auto)]
        public static extern InstallState MsiGetComponentPath(string productCode, string componentCode, StringBuilder componentPath, ref int componentPathBufferSize);

        public const int MaxFeatureLength = 38;
        public const int MaxGuidLength = 38;
        public const int MaxPathLength = 1024;

        public enum InstallState
        {
            NotUsed = -7,
            BadConfig = -6,
            Incomplete = -5,
            SourceAbsent = -4,
            MoreData = -3,
            InvalidArg = -2,
            Unknown = -1,
            Broken = 0,
            Advertised = 1,
            Removed = 1,
            Absent = 2,
            Local = 3,
            Source = 4,
            Default = 5
        }
    }
}
