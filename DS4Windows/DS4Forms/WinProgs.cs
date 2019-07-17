using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;

using System.Xml;
using System.Runtime.InteropServices;
using System.Threading;
using System.Diagnostics;
using System.Threading.Tasks;
using static DS4Windows.Global;

namespace DS4Windows.Forms
{
    public partial class WinProgs : Form
    {
        ComboBox[] cbs;
        public DS4Form form;
        //C:\ProgramData\Microsoft\Windows\Start Menu\Programs
        string steamgamesdir, origingamesdir;
        protected String m_Profile = Global.appdatapath + "\\Auto Profiles.xml";

        ProgramPathItem selectedProgramPathItem = null;
        List<string> lodsf = new List<string>();
        bool appsloaded = false;
        public const string steamCommx86Loc = @"C:\Program Files (x86)\Steam\steamapps\common";
        public const string steamCommLoc = @"C:\Program Files\Steam\steamapps\common";
        const string originx86Loc = @"C:\Program Files (x86)\Origin Games";
        const string originLoc = @"C:\Program Files\Origin Games";

        public WinProgs(string[] oc, DS4Form main)
        {
            ToolTip tp = new ToolTip();
            InitializeComponent();

            openProgram.Filter =  Properties.Resources.Programs+"|*.exe|" + Properties.Resources.Shortcuts + "|*.lnk";
            form = main;
            cbs = new ComboBox[4] { cBProfile1, cBProfile2, cBProfile3, cBProfile4 };
            for (int i = 0; i < 4; i++)
            {
                cbs[i].Items.AddRange(oc);
                cbs[i].Items.Add(Properties.Resources.noneProfile);
                cbs[i].SelectedIndex = cbs[i].Items.Count - 1;
            }

            tp.SetToolTip(cBAutoProfileDebugLog, Properties.Resources.ShowAutoProfileDebugLogTip);
            tp.SetToolTip(tBPath, Properties.Resources.AutoProfilePathAndWindowTitleEditTip);
            tp.SetToolTip(tBWndTitle, Properties.Resources.AutoProfilePathAndWindowTitleEditTip);

            if (!File.Exists(Global.appdatapath + @"\Auto Profiles.xml"))
                Create();

            LoadP();

            if (UseCustomSteamFolder && Directory.Exists(CustomSteamFolder))
                steamgamesdir = CustomSteamFolder;
            else if (Directory.Exists(steamCommx86Loc))
                steamgamesdir = steamCommx86Loc;
            else if (Directory.Exists(steamCommLoc))
                steamgamesdir = steamCommLoc;
            else
                cMSPrograms.Items.Remove(addSteamGamesToolStripMenuItem);

            if (Directory.Exists(originx86Loc))
                origingamesdir = originx86Loc;
            else if (Directory.Exists(originLoc))
                origingamesdir = originLoc;
            else
                cMSPrograms.Items.Remove(addOriginGamesToolStripMenuItem);

            cBAutoProfileDebugLog.Checked = (DS4Form.autoProfileDebugLogLevel > 0);
            selectedProgramPathItem = null;
            UpdateProfileComboListValues(null);
        }

        public bool Create()
        {
            bool Saved = true;

            try
            {
                XmlNode Node;
                XmlDocument doc = new XmlDocument();

                Node = doc.CreateXmlDeclaration("1.0", "utf-8", String.Empty);
                doc.AppendChild(Node);

                Node = doc.CreateComment(String.Format(" Auto-Profile Configuration Data. {0} ", DateTime.Now));
                doc.AppendChild(Node);

                Node = doc.CreateWhitespace("\r\n");
                doc.AppendChild(Node);

                Node = doc.CreateNode(XmlNodeType.Element, "Programs", "");
                doc.AppendChild(Node);
                doc.Save(m_Profile);
            }
            catch { Saved = false; }

            return Saved;
        }

        public void ShowMainWindow()
        {
            form.Show();
            form.WindowState = FormWindowState.Normal;
            form.Focus();
        }

        public void LoadP()
        {
            try
            {
                XmlDocument doc = new XmlDocument();

                iLIcons.Images.Clear();
                if (!File.Exists(Global.appdatapath + "\\Auto Profiles.xml"))
                    return;

                doc.Load(Global.appdatapath + "\\Auto Profiles.xml");
                XmlNodeList programslist = doc.SelectNodes("Programs/Program");

                int index;
                ListViewItem lvi;
                string progPath, progTitle;

                lVPrograms.BeginUpdate();
                foreach (XmlNode progNode in programslist)
                {
                    progPath = progNode.Attributes["path"]?.Value;
                    progTitle = progNode.Attributes["title"]?.Value;

                    if (!string.IsNullOrEmpty(progPath))
                    {
                        index = iLIcons.Images.IndexOfKey(progPath);
                        if (index < 0 && File.Exists(progPath))
                        {
                            iLIcons.Images.Add(progPath, Icon.ExtractAssociatedIcon(progPath));
                            index = iLIcons.Images.Count - 1;
                        }                        

                        if(index >= 0)
                            lvi = new ListViewItem(Path.GetFileNameWithoutExtension(progPath), index);
                        else
                            lvi = new ListViewItem(Path.GetFileNameWithoutExtension(progPath));

                        lvi.Checked = true;
                        lvi.SubItems.Add(progPath);
                        if (!String.IsNullOrEmpty(progTitle))
                            lvi.SubItems.Add(progTitle);

                        lVPrograms.Items.Add(lvi);
                    }
                }
            }
            catch (Exception e)
            {
                // Eat all exceptions while reading auto-profile file because we don't want to crash DS4Win app just because there are some permissions or other issues with the file
                AppLogger.LogToGui($"ERROR. Auto-profile XML file {Global.appdatapath}\\Auto Profiles.xml reading failed. {e.Message}", true);
            }
            finally
            {
                lVPrograms.EndUpdate();
            }
        }

        private void GetApps(string path)
        {
            lodsf.Clear();
            lodsf.AddRange(Directory.GetFiles(path, "*.exe", SearchOption.TopDirectoryOnly));
            foreach (string s in Directory.GetDirectories(path, "*", SearchOption.TopDirectoryOnly))
            {
                try
                {
                    lodsf.AddRange(Directory.GetFiles(s, "*.exe", SearchOption.TopDirectoryOnly));
                    lodsf.AddRange(GetAppsR(s));
                }
                catch { }
            }

            appsloaded = true;
        }

        private List<string> GetAppsR(string path)
        {
            List<string> lods = new List<string>();
            foreach (string s in Directory.GetDirectories(path, "*", SearchOption.TopDirectoryOnly))
            {
                try
                {
                    lods.AddRange(Directory.GetFiles(s, "*.exe", SearchOption.TopDirectoryOnly));
                    lods.AddRange(GetAppsR(s));
                }
                catch { }
            }

            return lods;
        }

        private void GetShortcuts(string path)
        {
            lodsf.Clear();
            lodsf.AddRange(Directory.GetFiles(path, "*.lnk", SearchOption.AllDirectories));
            lodsf.AddRange(Directory.GetFiles(Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu) + "\\Programs", "*.lnk", SearchOption.AllDirectories));
            for (int i = 0; i < lodsf.Count; i++)
                lodsf[i] = GetTargetPath(lodsf[i]);

            appsloaded = true;
        }

        int AddLoadedApps(bool autoremoveSetupApps = true)
        {
            int numOfAppsAdded = 0;

            if (appsloaded)
            {
                bnAddPrograms.Text = Properties.Resources.AddingToList;

                if (autoremoveSetupApps)
                {
                    for (int i = lodsf.Count - 1; i >= 0; i--)
                    {
                        if (lodsf[i].Contains("etup") || lodsf[i].Contains("dotnet") || lodsf[i].Contains("SETUP")
                            || lodsf[i].Contains("edist") || lodsf[i].Contains("nstall") || String.IsNullOrEmpty(lodsf[i]))
                            lodsf.RemoveAt(i);
                    }

                    // Remove existing program entries from a list of new app paths (no need to add the same path twice)
                    for (int i = lodsf.Count - 1; i >= 0; i--)
                    {
                        for (int j = 0; j < lVPrograms.Items.Count; j++)
                        {
                            if(string.Equals(lVPrograms.Items[j].SubItems[1].Text, lodsf[i], StringComparison.OrdinalIgnoreCase))
                            {
                                lodsf.RemoveAt(i);
                                break;
                            }                            
                        }
                    }
                }

                try
                {
                    ListViewItem lvi;
                    int index;

                    lVPrograms.BeginUpdate();
                    foreach (string st in lodsf)
                    {
                        if (File.Exists(st))
                        {
                            index = iLIcons.Images.IndexOfKey(st);
                            if (index < 0)
                            {
                                iLIcons.Images.Add(st, Icon.ExtractAssociatedIcon(st));
                                index = iLIcons.Images.Count - 1;
                            }
                           
                            if(index >= 0)
                                lvi = new ListViewItem(Path.GetFileNameWithoutExtension(st), index);
                            else
                                lvi = new ListViewItem(Path.GetFileNameWithoutExtension(st));

                            lvi.SubItems.Add(st);
                            lvi.ToolTipText = st;
                            lVPrograms.Items.Add(lvi);
                            numOfAppsAdded++;
                        }
                    }
                }
                catch (Exception e)
                {
                    // Eat all exceptions while processing added apps because we don't want to crash DS4Win app just because there are some permissions or other issues with the file
                    AppLogger.LogToGui($"ERROR. Failed to add selected applications to an auto-profile list. {e.Message}", true);
                }
                finally
                {
                    lVPrograms.EndUpdate();

                    bnAddPrograms.Text = Properties.Resources.AddPrograms;
                    bnAddPrograms.Enabled = true;
                    appsloaded = false;
                }
            }

            return numOfAppsAdded;
        }

        private XmlNode FindProgramXMLItem(XmlDocument doc, string programPath, string windowTitle)
        {
            // Try to find a specified programPath+windowTitle program entry from an autoprofile XML file list
            foreach (XmlNode item in doc.SelectNodes("/Programs/Program"))
            {
                XmlAttribute xmlAttrTitle = item.Attributes["title"];
                if(item.Attributes["path"].InnerText == programPath)
                {
                    if (String.IsNullOrEmpty(windowTitle) && (xmlAttrTitle == null || String.IsNullOrEmpty(xmlAttrTitle.InnerText)))
                        return item;
                    else if (!String.IsNullOrEmpty(windowTitle) && xmlAttrTitle != null && xmlAttrTitle.InnerText == windowTitle)
                        return item;
                }
            }

            return null;
        }

        public void Save(ProgramPathItem progPathItem)
        {
            XmlDocument doc = new XmlDocument();
            XmlNode Node;
            string newPath, newTitle;

            newPath = tBPath.Text.Trim();
            newTitle = tBWndTitle.Text;

            if (progPathItem == null || String.IsNullOrEmpty(progPathItem.path) || String.IsNullOrEmpty(newPath))
                return;

            try
            {
                doc.Load(m_Profile);
                Node = doc.CreateComment(String.Format(" Auto-Profile Configuration Data. {0} ", DateTime.Now));
                foreach (XmlNode node in doc.SelectNodes("//comment()"))
                    node.ParentNode.ReplaceChild(Node, node);

                // Find the existing XML program entry using the old path and title value as a search key and replace it with new path and title values (or add a new entry if this was a new program path)
                XmlNode oldxmlprocess = FindProgramXMLItem(doc, progPathItem.path, progPathItem.title);
                Node = doc.SelectSingleNode("Programs");

                XmlElement el = doc.CreateElement("Program");
                el.SetAttribute("path", newPath);
                if (!String.IsNullOrEmpty(newTitle))
                    el.SetAttribute("title", newTitle);

                el.AppendChild(doc.CreateElement("Controller1")).InnerText = cBProfile1.Text;
                el.AppendChild(doc.CreateElement("Controller2")).InnerText = cBProfile2.Text;
                el.AppendChild(doc.CreateElement("Controller3")).InnerText = cBProfile3.Text;
                el.AppendChild(doc.CreateElement("Controller4")).InnerText = cBProfile4.Text;
                el.AppendChild(doc.CreateElement("TurnOff")).InnerText = cBTurnOffDS4W.Checked.ToString();

                if (oldxmlprocess != null)
                    Node.ReplaceChild(el, oldxmlprocess);
                else
                    Node.AppendChild(el);

                doc.AppendChild(Node);
                doc.Save(m_Profile);

                if(selectedProgramPathItem != null)
                {
                    selectedProgramPathItem.path = newPath;
                    selectedProgramPathItem.title = newTitle;
                }

                if (lVPrograms.SelectedItems.Count > 0)
                {
                    lVPrograms.SelectedItems[0].Checked = true;

                    lVPrograms.SelectedItems[0].SubItems[0].Text = Path.GetFileNameWithoutExtension(newPath);
                    lVPrograms.SelectedItems[0].SubItems[1].Text = newPath;
                    if (lVPrograms.SelectedItems[0].SubItems.Count < 3)
                    {
                        if (!String.IsNullOrEmpty(newTitle))
                            lVPrograms.SelectedItems[0].SubItems.Add(newTitle);
                    }
                    else
                    {
                        lVPrograms.SelectedItems[0].SubItems[2].Text = newTitle;
                    }

                    if (!String.IsNullOrEmpty(newTitle))
                        lVPrograms.SelectedItems[0].ToolTipText = $"{newPath}  [{newTitle}]";
                    else
                        lVPrograms.SelectedItems[0].ToolTipText = newPath;
                }

                form.LoadP();
            }
            catch (Exception e)
            {
                // Eat all exceptions while writing auto-profile file because we don't want to crash DS4Win app just because there are some permissions or other issues with the file
                AppLogger.LogToGui($"ERROR. Auto-profile XML file {Global.appdatapath}\\Auto Profiles.xml writing failed. {e.Message}", true);
            }
        }

        public void LoadP(ProgramPathItem loadProgPathItem)
        {           
            if (loadProgPathItem == null)
                return;

            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(m_Profile);

                XmlNode programItem = FindProgramXMLItem(doc, loadProgPathItem.path, loadProgPathItem.title);
                if (programItem != null)
                {
                    XmlNode profileItem;

                    for (int i = 0; i < 4; i++)
                    {
                        profileItem = programItem.SelectSingleNode($".//Controller{i + 1}");
                        if (profileItem != null)
                        {
                            for (int j = 0; j < cbs[i].Items.Count; j++)
                            {
                                if (cbs[i].Items[j].ToString() == profileItem.InnerText)
                                {
                                    cbs[i].SelectedIndex = j;
                                    break;
                                }
                                else
                                    cbs[i].SelectedIndex = cbs[i].Items.Count - 1;
                            }
                        }
                        else
                            cbs[i].SelectedIndex = cbs[i].Items.Count - 1;
                    }

                    bool turnOff;
                    profileItem = programItem.SelectSingleNode($".//TurnOff");
                    if (profileItem != null && bool.TryParse(profileItem.InnerText, out turnOff))
                    {
                        cBTurnOffDS4W.Checked = turnOff;
                    }
                    else
                        cBTurnOffDS4W.Checked = false;
                }

                tBPath.Text = loadProgPathItem.path;
                tBWndTitle.Text = loadProgPathItem.title;
            }
            catch (Exception e)
            {
                // Eat all exceptions while reading auto-profile file because we don't want to crash DS4Win app just because there are some permissions or other issues with the file
                AppLogger.LogToGui($"ERROR. Failed to read {loadProgPathItem.path} {loadProgPathItem.title} XML entry. {e.Message}", true);
            }
            bnSave.Enabled = false;
        }

        public void RemoveP(ProgramPathItem removeProgPathItem, bool uncheck)
        {
            bnSave.Enabled = false;

            if (removeProgPathItem == null)
                return;

            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(m_Profile);

                XmlNode programItem = FindProgramXMLItem(doc, removeProgPathItem.path, removeProgPathItem.title);
                if (programItem != null)
                {
                    XmlNode parentNode = programItem.ParentNode;
                    if (parentNode != null)
                    {
                        parentNode.RemoveChild(programItem);
                        doc.AppendChild(parentNode);
                        doc.Save(m_Profile);
                    }
                }
                if (lVPrograms.SelectedItems.Count > 0 && uncheck)
                    lVPrograms.SelectedItems[0].Checked = false;
            }
            catch (Exception e)
            {
                // Eat all exceptions while updating auto-profile file because we don't want to crash DS4Win app just because there are some permissions or other issues with the file
                AppLogger.LogToGui($"ERROR. Failed to remove {removeProgPathItem.path} {removeProgPathItem.title} XML entry. {e.Message}", true);
            }

            UpdateProfileComboListValues(null); 
        }

        private void UpdateProfileComboListValues(ProgramPathItem progItem)
        {
            if (progItem != null)
            {
                for (int i = 0; i < 4; i++)
                    cbs[i].Enabled = true;

                tBPath.Enabled = tBWndTitle.Enabled = cBTurnOffDS4W.Enabled = true;
                LoadP(progItem);
                bnDelete.Enabled = true;
            }
            else
            {
                int last = cbs[0].Items.Count - 1;

                // Set all profile combox values to "none" value (ie. the last item in a controller combobox list)
                for (int i = 0; i < 4; i++)
                {
                    cbs[i].Enabled = false;
                    if (cbs[i].SelectedIndex != last)
                        cbs[i].SelectedIndex = last;
                }

                bnSave.Enabled = bnDelete.Enabled = false;
                tBPath.Enabled = tBWndTitle.Enabled = cBTurnOffDS4W.Enabled = false;
                tBPath.Text = tBWndTitle.Text = "";
                cBTurnOffDS4W.Checked = false;                
            }
        }

        private void CBProfile_IndexChanged(object sender, EventArgs e)
        {
            if (selectedProgramPathItem != null && lVPrograms.SelectedItems.Count > 0)
                bnSave.Enabled = true;
        }

        private void bnSave_Click(object sender, EventArgs e)
        {
            if (selectedProgramPathItem != null)
            {
                // Path cannot be empty. If user tried to clear it then re-use the original path value
                if (String.IsNullOrEmpty(tBPath.Text))
                    tBPath.Text = selectedProgramPathItem.path;

                Save(selectedProgramPathItem);
            }
            bnSave.Enabled = false;
        }

        private void bnAddPrograms_Click(object sender, EventArgs e)
        {
            cMSPrograms.Show(bnAddPrograms, new Point(0, bnAddPrograms.Height));
        }

        private void bnDelete_Click(object sender, EventArgs e)
        {
            RemoveP(selectedProgramPathItem, true);
            selectedProgramPathItem = null;
            bnSave.Enabled = bnDelete.Enabled = false;
            if (lVPrograms.SelectedItems.Count > 0)
                lVPrograms.SelectedItems[0].Selected = lVPrograms.SelectedItems[0].Focused = false;
        }

        private void lBProgramPath_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lVPrograms.SelectedItems.Count > 0 && lVPrograms.SelectedIndices[0] > -1)
            {
                if (selectedProgramPathItem == null) selectedProgramPathItem = new ProgramPathItem(String.Empty, String.Empty);

                selectedProgramPathItem.path = lVPrograms.SelectedItems[0].SubItems[1].Text;
                if (lVPrograms.SelectedItems[0].SubItems.Count >= 3)
                    selectedProgramPathItem.title = lVPrograms.SelectedItems[0].SubItems[2].Text;
                else
                    selectedProgramPathItem.title = String.Empty;
                    
                UpdateProfileComboListValues(selectedProgramPathItem);
            }
            else
            {
                selectedProgramPathItem = null;
                UpdateProfileComboListValues(null);
            }
        }

        private void bnHideUnchecked_Click(object sender, EventArgs e)
        {
            // Remove all unchecked items from autoprofile XML file and listView list
            foreach (ListViewItem lvi in lVPrograms.Items)
            {
                if (!lvi.Checked)
                {
                    RemoveP(new ProgramPathItem(lvi.SubItems[1]?.Text, (lvi.SubItems.Count >=3 ? lvi.SubItems[2]?.Text : null)), false);
                }
            }

            selectedProgramPathItem = null;
            form.RefreshAutoProfilesPage();
        }

        private async void addSteamGamesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            bnAddPrograms.Text = Properties.Resources.Loading;
            bnAddPrograms.Enabled = false;
            cMSPrograms.Items.Remove(addSteamGamesToolStripMenuItem);
            await Task.Run(() => GetApps(steamgamesdir));
            AddLoadedApps();
        }
        
        private async void addDirectoryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            if (fbd.ShowDialog() == DialogResult.OK)
            {
                bnAddPrograms.Text = Properties.Resources.Loading;
                bnAddPrograms.Enabled = false;
                await Task.Run(() => GetApps(fbd.SelectedPath));
                AddLoadedApps();
            }
        }

        private void browseForOtherProgramsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (openProgram.ShowDialog() == DialogResult.OK)
            {
                string file = openProgram.FileName;
                if (file.EndsWith(".lnk"))
                {
                    file = GetTargetPath(file);
                }

                if (lVPrograms.SelectedItems.Count > 0)
                    lVPrograms.SelectedItems[0].Selected = lVPrograms.SelectedItems[0].Focused = false;

                lodsf.Clear();
                lodsf.Add(file);
                appsloaded = true;

                if (AddLoadedApps(false) > 0 && lVPrograms.Items.Count > 0)
                {
                    lVPrograms.Items[lVPrograms.Items.Count - 1].Focused = true;
                    lVPrograms.Items[lVPrograms.Items.Count - 1].Selected = true;
                }
            }
        }

        private async void addOriginGamesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            bnAddPrograms.Text = Properties.Resources.Loading;
            bnAddPrograms.Enabled = false;
            cMSPrograms.Items.Remove(addOriginGamesToolStripMenuItem);
            await Task.Run(() => GetApps(origingamesdir));
            AddLoadedApps();
        }

        private async void addProgramsFromStartMenuToolStripMenuItem_Click(object sender, EventArgs e)
        {
            bnAddPrograms.Text = Properties.Resources.Loading;
            bnAddPrograms.Enabled = false;
            cMSPrograms.Items.Remove(addProgramsFromStartMenuToolStripMenuItem);
            await Task.Run(() => GetShortcuts(Environment.GetFolderPath(Environment.SpecialFolder.StartMenu) + "\\Programs"));
            AddLoadedApps();
        }

        public static string GetTargetPath(string filePath)
        {
            string targetPath = ResolveMsiShortcut(filePath);
            if (targetPath == null)
            {
                targetPath = ResolveShortcut(filePath);
            }

            return targetPath;
        }

        public static string GetInternetShortcut(string filePath)
        {
            string url = "";

            using (TextReader reader = new StreamReader(filePath))
            {
                string line = "";
                while ((line = reader.ReadLine()) != null)
                {
                    if (line.StartsWith("URL="))
                    {
                        string[] splitLine = line.Split('=');
                        if (splitLine.Length > 0)
                        {
                            url = splitLine[1];
                            break;
                        }
                    }
                }
            }

            return url;
        }

        public static string ResolveShortcut(string filePath)
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

        public static string ResolveShortcutAndArgument(string filePath)
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

        private void cBTurnOffDS4W_CheckedChanged(object sender, EventArgs e)
        {
            CBProfile_IndexChanged(sender, e);
        }

        private void cBAutoProfileDebugLog_CheckedChanged(object sender, EventArgs e)
        {
            DS4Form.autoProfileDebugLogLevel = cBAutoProfileDebugLog.Checked ? 1 : 0;
        }

        private void tBPath_TextChanged(object sender, EventArgs e)
        {
            int last = cbs[0].Items.Count - 1;
            if (cbs[0].SelectedIndex != last || cbs[1].SelectedIndex != last || cbs[2].SelectedIndex != last || cbs[3].SelectedIndex != last || !cBTurnOffDS4W.Checked)
            {
                // Content of path or wndTitle editbox changed. Enable SAVE button if it is disabled at the moment and there is an active selection in a listView
                if (selectedProgramPathItem != null && bnSave.Enabled == false && lVPrograms.SelectedItems.Count > 0)
                    bnSave.Enabled = true;
            }
        }

        public static string ResolveMsiShortcut(string file)
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
    }

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
