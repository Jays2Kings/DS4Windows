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

namespace DS4Windows
{
    public partial class WinProgs : Form
    {
        ToolTip tp = new ToolTip();
        ComboBox[] cbs;
        public DS4Form form;
        //C:\ProgramData\Microsoft\Windows\Start Menu\Programs
        string steamgamesdir, origingamesdir;
        protected String m_Profile = Global.appdatapath + "\\Auto Profiles.xml";
        protected XmlDocument m_Xdoc = new XmlDocument();
        List<string> programpaths = new List<string>();
        List<string> lodsf = new List<string>();
        bool appsloaded = false;
        const string steamCommx86Loc = @"C:\Program Files (x86)\Steam\steamapps\common";
        const string steamCommLoc = @"C:\Program Files\Steam\steamapps\common";
        const string originx86Loc = @"C:\Program Files (x86)\Origin Games";
        const string originLoc = @"C:\Program Files\Origin Games";

        public WinProgs(string[] oc, DS4Form main)
        {
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

            if (!File.Exists(Global.appdatapath + @"\Auto Profiles.xml"))
                Create();

            LoadP();

            if (Directory.Exists(steamCommx86Loc))
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
        }

        public bool Create()
        {
            bool Saved = true;

            try
            {
                XmlNode Node;

                Node = m_Xdoc.CreateXmlDeclaration("1.0", "utf-8", String.Empty);
                m_Xdoc.AppendChild(Node);

                Node = m_Xdoc.CreateComment(String.Format(" Auto-Profile Configuration Data. {0} ", DateTime.Now));
                m_Xdoc.AppendChild(Node);

                Node = m_Xdoc.CreateWhitespace("\r\n");
                m_Xdoc.AppendChild(Node);

                Node = m_Xdoc.CreateNode(XmlNodeType.Element, "Programs", "");
                m_Xdoc.AppendChild(Node);
                m_Xdoc.Save(m_Profile);
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
            XmlDocument doc = new XmlDocument();
            programpaths.Clear();
            if (!File.Exists(Global.appdatapath + "\\Auto Profiles.xml"))
                return;

            doc.Load(Global.appdatapath + "\\Auto Profiles.xml");
            XmlNodeList programslist = doc.SelectNodes("Programs/Program");
            foreach (XmlNode x in programslist)
                programpaths.Add(x.Attributes["path"].Value);

            lVPrograms.BeginUpdate();
            int index = 0;
            foreach (string st in programpaths)
            {
                if (!string.IsNullOrEmpty(st))
                {
                    if (File.Exists(st))
                    {
                        iLIcons.Images.Add(Icon.ExtractAssociatedIcon(st));
                    }

                    ListViewItem lvi = new ListViewItem(Path.GetFileNameWithoutExtension(st), index);
                    lvi.Checked = true;
                    lvi.ToolTipText = st;
                    lvi.SubItems.Add(st);
                    lVPrograms.Items.Add(lvi);
                }

                index++;
            }

            lVPrograms.EndUpdate();
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

        void AddLoadedApps()
        {
            if (appsloaded)
            {
                bnAddPrograms.Text = Properties.Resources.AddingToList;
                for (int i = lodsf.Count - 1; i >= 0; i--)
                {
                    if (lodsf[i].Contains("etup") || lodsf[i].Contains("dotnet") || lodsf[i].Contains("SETUP")
                        || lodsf[i].Contains("edist") || lodsf[i].Contains("nstall") || String.IsNullOrEmpty(lodsf[i]))
                        lodsf.RemoveAt(i);
                }

                for (int i = lodsf.Count - 1; i >= 0; i--)
                {
                    for (int j = programpaths.Count - 1; j >= 0; j--)
                    {
                        if (lodsf[i].ToLower().Replace('/', '\\') == programpaths[j].ToLower().Replace('/', '\\'))
                            lodsf.RemoveAt(i);
                    }
                }

                lVPrograms.BeginUpdate();
                foreach (string st in lodsf)
                {
                    if (File.Exists(st))
                    {
                        int index = programpaths.IndexOf(st);
                        iLIcons.Images.Add(Icon.ExtractAssociatedIcon(st));
                        ListViewItem lvi = new ListViewItem(Path.GetFileNameWithoutExtension(st), iLIcons.Images.Count + index);
                        lvi.SubItems.Add(st);
                        lvi.ToolTipText = st;
                        lVPrograms.Items.Add(lvi);
                    }
                }
                lVPrograms.EndUpdate();

                bnAddPrograms.Text = Properties.Resources.AddPrograms;
                bnAddPrograms.Enabled = true;
                appsloaded = false;
            }
        }

        public void Save(string name)
        {
            m_Xdoc.Load(m_Profile);
            XmlNode Node;

            Node = m_Xdoc.CreateComment(String.Format(" Auto-Profile Configuration Data. {0} ", DateTime.Now));
            foreach (XmlNode node in m_Xdoc.SelectNodes("//comment()"))
                node.ParentNode.ReplaceChild(Node, node);

            Node = m_Xdoc.SelectSingleNode("Programs");
            string programname;
            programname = Path.GetFileNameWithoutExtension(name);
            XmlElement el = m_Xdoc.CreateElement("Program");
            el.SetAttribute("path", name);
            el.AppendChild(m_Xdoc.CreateElement("Controller1")).InnerText = cBProfile1.Text;
            el.AppendChild(m_Xdoc.CreateElement("Controller2")).InnerText = cBProfile2.Text;
            el.AppendChild(m_Xdoc.CreateElement("Controller3")).InnerText = cBProfile3.Text;
            el.AppendChild(m_Xdoc.CreateElement("Controller4")).InnerText = cBProfile4.Text;
            el.AppendChild(m_Xdoc.CreateElement("TurnOff")).InnerText = cBTurnOffDS4W.Checked.ToString();

            try
            {
                XmlNode oldxmlprocess = m_Xdoc.SelectSingleNode("/Programs/Program[@path=\"" + lBProgramPath.Text + "\"]");
                Node.ReplaceChild(el, oldxmlprocess);
            }
            catch { Node.AppendChild(el); }

            m_Xdoc.AppendChild(Node);
            m_Xdoc.Save(m_Profile);

            if (lVPrograms.SelectedItems.Count > 0)
                lVPrograms.SelectedItems[0].Checked = true;

            form.LoadP();
        }

        public void LoadP(string name)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(m_Profile);
            XmlNodeList programs = doc.SelectNodes("Programs/Program");
            XmlNode Item = doc.SelectSingleNode("/Programs/Program[@path=\"" + name + "\"]");
            if (Item != null)
            {
                for (int i = 0; i < 4; i++)
                {
                    Item = doc.SelectSingleNode("/Programs/Program[@path=\"" + name + "\"]" + "/Controller" + (i + 1));
                    if (Item != null)
                    {
                        for (int j = 0; j < cbs[i].Items.Count; j++)
                        {
                            if (cbs[i].Items[j].ToString() == Item.InnerText)
                            {
                                cbs[i].SelectedIndex = j;
                                bnSave.Enabled = false;
                                break;
                            }
                            else
                                cbs[i].SelectedIndex = cbs[i].Items.Count - 1;
                        }
                    }
                    else
                        cbs[i].SelectedIndex = cbs[i].Items.Count - 1;
                }

                Item = doc.SelectSingleNode("/Programs/Program[@path=\"" + name + "\"]" + "/TurnOff");
                bool turnOff;
                if (Item != null && bool.TryParse(Item.InnerText, out turnOff))
                {
                    cBTurnOffDS4W.Checked = turnOff;
                }
                else
                    cBTurnOffDS4W.Checked = false;
            }
            else
            {
                for (int i = 0; i < 4; i++)
                    cbs[i].SelectedIndex = cbs[i].Items.Count - 1;

                cBTurnOffDS4W.Checked = false;
                bnSave.Enabled = false;
            }
        }

        public void RemoveP(string name, bool uncheck)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(m_Profile);
            XmlNode Node = doc.SelectSingleNode("Programs");
            XmlNode Item = doc.SelectSingleNode("/Programs/Program[@path=\"" + name + "\"]");
            if (Item != null)
                Node.RemoveChild(Item);

            doc.AppendChild(Node);
            doc.Save(m_Profile);
            if (lVPrograms.SelectedItems.Count > 0 && uncheck)
                lVPrograms.SelectedItems[0].Checked = false;

            for (int i = 0; i < 4; i++)
                cbs[i].SelectedIndex = cbs[i].Items.Count - 1;

            bnSave.Enabled = false;
        }

        private void CBProfile_IndexChanged(object sender, EventArgs e)
        {
            int last = cbs[0].Items.Count - 1;
            if (lBProgramPath.Text != string.Empty)
                bnSave.Enabled = true;

            if (cbs[0].SelectedIndex == last && cbs[1].SelectedIndex == last &&
                cbs[2].SelectedIndex == last && cbs[3].SelectedIndex == last && !cBTurnOffDS4W.Checked)
                bnSave.Enabled = false;
        }

        private void bnSave_Click(object sender, EventArgs e)
        {
            if (lBProgramPath.Text != "")
                Save(lBProgramPath.Text);

            bnSave.Enabled = false;
        }

        private void bnAddPrograms_Click(object sender, EventArgs e)
        {
            cMSPrograms.Show(bnAddPrograms, new Point(0, bnAddPrograms.Height));
        }

        private void lBProgramPath_TextChanged(object sender, EventArgs e)
        {
            if (lBProgramPath.Text != "")
                LoadP(lBProgramPath.Text);
            else
            {
                for (int i = 0; i < 4; i++)
                    cbs[i].SelectedIndex = cbs[i].Items.Count - 1;
            }
        }

        private void bnDelete_Click(object sender, EventArgs e)
        {
            RemoveP(lBProgramPath.Text, true);
        }

        private void lBProgramPath_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lVPrograms.SelectedItems.Count > 0)
            {
                if (lVPrograms.SelectedIndices[0] > -1)
                    lBProgramPath.Text = lVPrograms.SelectedItems[0].SubItems[1].Text;
            }
            else
                lBProgramPath.Text = "";
        }

        private void listView1_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (lVPrograms.Items[e.Index].Checked)
                RemoveP(lVPrograms.Items[e.Index].SubItems[1].Text, false);
        }

        private void bnHideUnchecked_Click(object sender, EventArgs e)
        {
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

                lBProgramPath.Text = file;
                iLIcons.Images.Add(Icon.ExtractAssociatedIcon(file));
                ListViewItem lvi = new ListViewItem(Path.GetFileNameWithoutExtension(file), lVPrograms.Items.Count);
                lvi.SubItems.Add(file);
                lVPrograms.Items.Insert(0, lvi);
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
