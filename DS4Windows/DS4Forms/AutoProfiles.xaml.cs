using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Ookii.Dialogs.Wpf;
using DS4WinWPF.DS4Forms.ViewModels;
using Microsoft.Win32;

namespace DS4WinWPF.DS4Forms
{
    /// <summary>
    /// Interaction logic for AutoProfiles.xaml
    /// </summary>
    public partial class AutoProfiles : UserControl
    {
        protected String m_Profile = DS4Windows.Global.appdatapath + "\\Auto Profiles.xml";
        public const string steamCommx86Loc = @"C:\Program Files (x86)\Steam\steamapps\common";
        public const string steamCommLoc = @"C:\Program Files\Steam\steamapps\common";
        private string steamgamesdir;
        private AutoProfilesViewModel autoProfVM;
        private AutoProfileHolder autoProfileHolder;
        private ProfileList profileList;
        private bool autoDebug;

        public AutoProfileHolder AutoProfileHolder { get => autoProfileHolder;
            set => autoProfileHolder = value; }
        public AutoProfilesViewModel AutoProfVM { get => autoProfVM; }
        public bool AutoDebug { get => autoDebug; }
        public event EventHandler AutoDebugChanged;

        public AutoProfiles()
        {
            InitializeComponent();

            if (!File.Exists(DS4Windows.Global.appdatapath + @"\Auto Profiles.xml"))
                DS4Windows.Global.CreateAutoProfiles(m_Profile);

            //LoadP();

            if (DS4Windows.Global.UseCustomSteamFolder &&
                Directory.Exists(DS4Windows.Global.CustomSteamFolder))
                steamgamesdir = DS4Windows.Global.CustomSteamFolder;
            else if (Directory.Exists(steamCommx86Loc))
                steamgamesdir = steamCommx86Loc;
            else if (Directory.Exists(steamCommLoc))
                steamgamesdir = steamCommLoc;
            else
                addProgramsBtn.ContextMenu.Items.Remove(steamMenuItem);

            autoProfileHolder = new AutoProfileHolder();
        }

        public void SetupDataContext(ProfileList profileList)
        {
            autoProfVM = new AutoProfilesViewModel(autoProfileHolder, profileList);
            programListLV.DataContext = autoProfVM;
            programListLV.ItemsSource = autoProfVM.ProgramColl;

            autoProfVM.SearchFinished += AutoProfVM_SearchFinished;
            autoProfVM.CurrentItemChange += AutoProfVM_CurrentItemChange;

            this.profileList = profileList;
            cont1AutoProfCol.Collection = profileList.ProfileListCol;
            cont2AutoProfCol.Collection = profileList.ProfileListCol;
            cont3AutoProfCol.Collection = profileList.ProfileListCol;
            cont4AutoProfCol.Collection = profileList.ProfileListCol;
        }

        private void AutoProfVM_CurrentItemChange(AutoProfilesViewModel sender, ProgramItem item)
        {
            if (item != null)
            {
                if (item.MatchedAutoProfile != null)
                {
                    ProfileEntity tempProf = null;
                    string profileName = item.MatchedAutoProfile.ProfileNames[0];
                    if (!string.IsNullOrEmpty(profileName) && profileName != "(none)")
                    {
                        tempProf = profileList.ProfileListCol.SingleOrDefault(x => x.Name == profileName);
                        if (tempProf != null)
                        {
                            item.SelectedIndexCon1 = profileList.ProfileListCol.IndexOf(tempProf) + 1;
                        }
                    }
                    else
                    {
                        item.SelectedIndexCon1 = 0;
                    }

                    profileName = item.MatchedAutoProfile.ProfileNames[1];
                    if (!string.IsNullOrEmpty(profileName) && profileName != "(none)")
                    {
                        tempProf = profileList.ProfileListCol.SingleOrDefault(x => x.Name == profileName);
                        if (tempProf != null)
                        {
                            item.SelectedIndexCon2 = profileList.ProfileListCol.IndexOf(tempProf) + 1;
                        }
                    }
                    else
                    {
                        item.SelectedIndexCon2 = 0;
                    }

                    profileName = item.MatchedAutoProfile.ProfileNames[2];
                    if (!string.IsNullOrEmpty(profileName) && profileName != "(none)")
                    {
                        tempProf = profileList.ProfileListCol.SingleOrDefault(x => x.Name == profileName);
                        if (tempProf != null)
                        {
                            item.SelectedIndexCon3 = profileList.ProfileListCol.IndexOf(tempProf) + 1;
                        }
                    }
                    else
                    {
                        item.SelectedIndexCon3 = 0;
                    }

                    profileName = item.MatchedAutoProfile.ProfileNames[3];
                    if (!string.IsNullOrEmpty(profileName) && profileName != "(none)")
                    {
                        tempProf = profileList.ProfileListCol.SingleOrDefault(x => x.Name == profileName);
                        if (tempProf != null)
                        {
                            item.SelectedIndexCon4 = profileList.ProfileListCol.IndexOf(tempProf) + 1;
                        }
                    }
                    else
                    {
                        item.SelectedIndexCon4 = 0;
                    }
                }

                editControlsPanel.DataContext = item;
                editControlsPanel.IsEnabled = true;
            }
            else
            {
                editControlsPanel.DataContext = null;
                editControlsPanel.IsEnabled = false;

                cont1AutoProf.SelectedIndex = 0;
                cont2AutoProf.SelectedIndex = 0;
                cont3AutoProf.SelectedIndex = 0;
                cont4AutoProf.SelectedIndex = 0;
            }
        }

        private void AutoProfVM_SearchFinished(object sender, EventArgs e)
        {
            IsEnabled = true;
        }

        private void SteamMenuItem_Click(object sender, RoutedEventArgs e)
        {
            this.IsEnabled = false;
            steamMenuItem.Visibility = Visibility.Collapsed;
            programListLV.ItemsSource = null;
            autoProfVM.SearchFinished += AppsSearchFinished;
            autoProfVM.AddProgramsFromSteam(steamgamesdir);
        }

        private void BrowseProgsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            this.IsEnabled = false;
            VistaFolderBrowserDialog dialog = new VistaFolderBrowserDialog();
            if (dialog.ShowDialog() == true)
            {
                //browseProgsMenuItem.Visibility = Visibility.Collapsed;
                programListLV.ItemsSource = null;
                autoProfVM.SearchFinished += AppsSearchFinished;
                autoProfVM.AddProgramsFromDir(dialog.SelectedPath);
            }
            else
            {
                this.IsEnabled = true;
            }
        }

        private void StartMenuItem_Click(object sender, RoutedEventArgs e)
        {
            this.IsEnabled = false;
            startMenuItem.Visibility = Visibility.Collapsed;
            programListLV.ItemsSource = null;
            autoProfVM.SearchFinished += AppsSearchFinished;
            autoProfVM.AddProgramsFromStartMenu();
        }

        private void AppsSearchFinished(object sender, EventArgs e)
        {
            autoProfVM.SearchFinished -= AppsSearchFinished;
            programListLV.ItemsSource = autoProfVM.ProgramColl;
        }

        private void AddProgramsBtn_Click(object sender, RoutedEventArgs e)
        {
            addProgramsBtn.ContextMenu.IsOpen = true;
            e.Handled = true;
        }

        private void HideUncheckedBtn_Click(object sender, RoutedEventArgs e)
        {
            programListLV.ItemsSource = null;
            autoProfVM.RemoveUnchecked();
            steamMenuItem.Visibility = Visibility.Visible;
            startMenuItem.Visibility = Visibility.Visible;
            browseProgsMenuItem.Visibility = Visibility.Visible;
            programListLV.ItemsSource = autoProfVM.ProgramColl;
        }

        private void ShowAutoDebugCk_Click(object sender, RoutedEventArgs e)
        {
            bool state = showAutoDebugCk.IsChecked == true;
            autoDebug = state;
            AutoDebugChanged?.Invoke(this, EventArgs.Empty);
        }

        private void RemoveAutoBtn_Click(object sender, RoutedEventArgs e)
        {
            if (autoProfVM.SelectedItem != null)
            {
                editControlsPanel.DataContext = null;
                autoProfVM.RemoveAutoProfileEntry(autoProfVM.SelectedItem);
                autoProfVM.AutoProfileHolder.Save(DS4Windows.Global.appdatapath + @"\Auto Profiles.xml");
                autoProfVM.SelectedItem = null;
            }
        }

        private void SaveAutoBtn_Click(object sender, RoutedEventArgs e)
        {
            if (autoProfVM.SelectedItem != null)
            {
                if (autoProfVM.SelectedItem.MatchedAutoProfile == null)
                {
                    autoProfVM.CreateAutoProfileEntry(autoProfVM.SelectedItem);
                }
                else
                {
                    autoProfVM.PersistAutoProfileEntry(autoProfVM.SelectedItem);
                }

                autoProfVM.AutoProfileHolder.Save(DS4Windows.Global.appdatapath + @"\Auto Profiles.xml");
            }
        }

        private void BrowseAddProgMenuItem_Click(object sender, RoutedEventArgs e)
        {
            this.IsEnabled = false;
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Multiselect = false;
            dialog.AddExtension = true;
            dialog.DefaultExt = ".exe";
            dialog.Filter = "Program (*.exe)|*.exe";
            dialog.Title = "Select Program";

            dialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            if (dialog.ShowDialog() == true)
            {
                programListLV.ItemsSource = null;
                autoProfVM.SearchFinished += AppsSearchFinished;
                autoProfVM.AddProgramExeLocation(dialog.FileName);
            }
            else
            {
                this.IsEnabled = true;
            }
        }
    }
}
