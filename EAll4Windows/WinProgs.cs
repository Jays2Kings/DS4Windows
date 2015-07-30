﻿using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;

using System.Xml;
using System.Runtime.InteropServices;

namespace EAll4Windows
{
    public partial class WinProgs : Form
    {
        ToolTip tp = new ToolTip();
        ComboBox[] cbs;
        public EAll4Form form;
        //C:\ProgramData\Microsoft\Windows\Start Menu\Programs
        string steamgamesdir, origingamesdir;
        protected String m_Profile = Global.appdatapath + "\\Auto Profiles.xml";
        protected XmlDocument m_Xdoc = new XmlDocument();
        List<string> programpaths = new List<string>();
        List<string> lodsf = new List<string>();
        bool appsloaded = false;

        public WinProgs(string[] oc, EAll4Form main)
        {
            InitializeComponent();

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

            if (Directory.Exists(@"C:\Program Files (x86)\Steam\steamapps\common"))
                steamgamesdir =  @"C:\Program Files (x86)\Steam\steamapps\common";
            else if (Directory.Exists(@"C:\Program Files\Steam\steamapps\common"))
                steamgamesdir = @"C:\Program Files\Steam\steamapps\common";
            else
                cMSPrograms.Items.Remove(addSteamGamesToolStripMenuItem);

            if (Directory.Exists(@"C:\Program Files (x86)\Origin Games"))
                origingamesdir = @"C:\Program Files (x86)\Origin Games";
            else if (Directory.Exists(@"C:\Program Files\Origin Games"))
                origingamesdir = @"C:\Program Files\Origin Games";
            else
                cMSPrograms.Items.Remove(addOriginGamesToolStripMenuItem);
        }

        public bool Create()
        {
            Boolean Saved = true;

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
            foreach (string st in programpaths)
            {
                if (File.Exists(st))
                {
                    int index = programpaths.IndexOf(st);
                    if (string.Empty != st)
                    {
                        iLIcons.Images.Add(Icon.ExtractAssociatedIcon(st));
                        ListViewItem lvi = new ListViewItem(Path.GetFileNameWithoutExtension(st), index);
                        lvi.SubItems.Add(st);
                        lvi.Checked = true;
                        lvi.ToolTipText = st;
                        lVPrograms.Items.Add(lvi);
                    }
                }
                else
                {
                    RemoveP(st, false, false);
                }
            }
        }


        private void bnLoadSteam_Click(object sender, EventArgs e)
        {

        }


        private void GetApps(string path)
        {
            lodsf.Clear();
            lodsf.AddRange(Directory.GetFiles(path, "*.exe", SearchOption.AllDirectories));
            appsloaded = true;
        }

        private void GetShortcuts(string path)
        {
            lodsf.Clear();
            lodsf.AddRange(Directory.GetFiles(path, "*.lnk", SearchOption.AllDirectories));
            lodsf.AddRange(Directory.GetFiles(@"C:\ProgramData\Microsoft\Windows\Start Menu\Programs", "*.lnk", SearchOption.AllDirectories));
            for (int i = 0; i < lodsf.Count; i++)
                lodsf[i] = GetTargetPath(lodsf[i]);
            appsloaded = true;
        }

        void appstimer_Tick(object sender, EventArgs e)
        {
            if (appsloaded)
            {
                bnAddPrograms.Text = Properties.Resources.AddingToList;
                for (int i = lodsf.Count - 1; i >= 0; i--)
                    if (lodsf[i].Contains("etup") || lodsf[i].Contains("dotnet") || lodsf[i].Contains("SETUP")
                        || lodsf[i].Contains("edist") || lodsf[i].Contains("nstall") || String.IsNullOrEmpty(lodsf[i]))
                        lodsf.RemoveAt(i);
                for (int i = lodsf.Count - 1; i >= 0; i--)
                    for (int j = programpaths.Count - 1; j >= 0; j--)
                        if (lodsf[i].ToLower().Replace('/', '\\') == programpaths[j].ToLower().Replace('/', '\\'))
                            lodsf.RemoveAt(i);
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
                bnAddPrograms.Text = Properties.Resources.AddPrograms;
                bnAddPrograms.Enabled = true;
                appsloaded = false;
                ((Timer)sender).Stop();
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
                    for (int j = 0; j < cbs[i].Items.Count; j++)
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
            {
                for (int i = 0; i < 4; i++)
                    cbs[i].SelectedIndex = cbs[i].Items.Count - 1;
                bnSave.Enabled = false;
            }
        }

        public void RemoveP(string name, bool uncheck, bool reload = true)
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
            if (reload)
            form.LoadP();
        }

        private void CBProfile_IndexChanged(object sender, EventArgs e)
        {
            int last = cbs[0].Items.Count - 1;
            if (lBProgramPath.Text != string.Empty)
                bnSave.Enabled = true;
            if (cbs[0].SelectedIndex == last && cbs[1].SelectedIndex == last &&
                cbs[2].SelectedIndex == last && cbs[3].SelectedIndex == last)
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
                for (int i = 0; i < 4; i++)
                    cbs[i].SelectedIndex = cbs[i].Items.Count - 1;
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


        private void addSteamGamesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                var AppCollectionThread = new System.Threading.Thread(() => GetApps(steamgamesdir));
                AppCollectionThread.IsBackground = true;
                AppCollectionThread.Start();
            }
            catch { }
            bnAddPrograms.Text = Properties.Resources.Loading;
            bnAddPrograms.Enabled = false;
            cMSPrograms.Items.Remove(addSteamGamesToolStripMenuItem);
            Timer appstimer = new Timer();
            appstimer.Start();
            appstimer.Tick += appstimer_Tick;
        }

        private void browseForOtherProgramsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openProgram.Filter =  Properties.Resources.Programs+"|*.exe|" + Properties.Resources.Shortcuts + "|*.lnk";
            if (openProgram.ShowDialog() == System.Windows.Forms.DialogResult.OK)
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

        private void addOriginGamesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                var AppCollectionThread = new System.Threading.Thread(() => GetApps(origingamesdir));
                AppCollectionThread.IsBackground = true;
                AppCollectionThread.Start();
            }
            catch { }
            bnAddPrograms.Text = Properties.Resources.Loading;
            bnAddPrograms.Enabled = false;
            cMSPrograms.Items.Remove(addOriginGamesToolStripMenuItem);
            Timer appstimer = new Timer();
            appstimer.Start();
            appstimer.Tick += appstimer_Tick;
        }

        private void addProgramsFromStartMenuToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //MessageBox.Show(Environment.GetFolderPath(Environment.SpecialFolder.StartMenu) + "\\Programs");
            try
            {
                var AppCollectionThread = new System.Threading.Thread(() => GetShortcuts(Environment.GetFolderPath(Environment.SpecialFolder.StartMenu) + "\\Programs"));
                AppCollectionThread.IsBackground = true;
                AppCollectionThread.Start();
            }
            catch { }
            bnAddPrograms.Text = Properties.Resources.Loading;
            bnAddPrograms.Enabled = false;
            cMSPrograms.Items.Remove(addProgramsFromStartMenuToolStripMenuItem);
            Timer appstimer = new Timer();
            appstimer.Start();
            appstimer.Tick += appstimer_Tick;
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
            // IWshRuntimeLibrary is in the COM library "Windows Script Host Object Model"
            IWshRuntimeLibrary.WshShell shell = new IWshRuntimeLibrary.WshShell();

            try
            {
                IWshRuntimeLibrary.IWshShortcut shortcut = (IWshRuntimeLibrary.IWshShortcut)shell.CreateShortcut(filePath);
                return shortcut.TargetPath;
            }
            catch (COMException)
            {
                // A COMException is thrown if the file is not a valid shortcut (.lnk) file 
                return null;
            }
        }

        public static string ResolveShortcutAndArgument(string filePath)
        {
            // IWshRuntimeLibrary is in the COM library "Windows Script Host Object Model"
            IWshRuntimeLibrary.WshShell shell = new IWshRuntimeLibrary.WshShell();

            try
            {
                IWshRuntimeLibrary.IWshShortcut shortcut = (IWshRuntimeLibrary.IWshShortcut)shell.CreateShortcut(filePath);
                return shortcut.TargetPath + " " + shortcut.Arguments;
            }
            catch (COMException)
            {
                // A COMException is thrown if the file is not a valid shortcut (.lnk) file 
                return null;
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
