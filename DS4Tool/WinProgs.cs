using Microsoft.Win32;
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
using DS4Control;
using System.Xml;

namespace ScpServer
{
    public partial class WinProgs : Form
    {
        ToolTip tp = new ToolTip();
        ComboBox[] cbs;
        ScpForm form;
        string steamgamesdir;
        protected String m_Profile = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\DS4Tool\\Auto Profiles.xml";
        protected XmlDocument m_Xdoc = new XmlDocument();
        List<string> programpaths = new List<string>();
        List<string> lodsf = new List<string>();
        bool appsloaded = false;

        public WinProgs(string[] oc, ScpForm main)
        {
            InitializeComponent();
            
            form = main;
            cbs = new ComboBox[4] { cBProfile1, cBProfile2, cBProfile3, cBProfile4 };
            for (int i = 0; i < 4; i++)
            {
                cbs[i].Items.AddRange(oc);
                cbs[i].Items.Add("(none)");
                cbs[i].SelectedIndex = cbs[i].Items.Count - 1;
            }
            if (!File.Exists(Global.appdatapath + @"\Auto Profiles.xml"))
                Create();
            LoadP();
            RegistryKey regKey = Registry.CurrentUser;
            regKey = regKey.OpenSubKey(@"Software\Valve\Steam");
            if (regKey != null)
                steamgamesdir = Path.GetDirectoryName(regKey.GetValue("SourceModInstallPath").ToString()) + @"\common";
            if (!Directory.Exists(steamgamesdir))
            {
                bnLoadSteam.Visible = false;
                lVPrograms.Size = new Size(lVPrograms.Size.Width, lVPrograms.Size.Height + 25);
            }
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
                int index = programpaths.IndexOf(st);
                if (string.Empty != st)
                {
                    iLIcons.Images.Add(Icon.ExtractAssociatedIcon(st));
                    ListViewItem lvi = new ListViewItem(Path.GetFileNameWithoutExtension(st), index);
                    lvi.SubItems.Add(st);
                    lvi.Checked = true;
                    lVPrograms.Items.Add(lvi);
                }
            }
        }


        private void bnLoadSteam_Click(object sender, EventArgs e)
        {
            try
            {
                var AppCollectionThread = new System.Threading.Thread(() => GetApps());
                AppCollectionThread.IsBackground = true;
                AppCollectionThread.Start();
            }
            catch { }
            bnLoadSteam.Text = "Loading...";
            bnLoadSteam.Enabled = false;
            Timer appstimer = new Timer();
            appstimer.Start();
            appstimer.Tick += appstimer_Tick;
        }

        
        private void GetApps()
        {
            lodsf.AddRange(Directory.GetFiles(steamgamesdir, "*.exe", SearchOption.AllDirectories));
            appsloaded = true;
        }        

        void appstimer_Tick(object sender, EventArgs e)
        {
            if (appsloaded)
            {
                bnLoadSteam.Text = "Adding to list...";
                for (int i = lodsf.Count - 1; i >= 0; i--)
                    if (lodsf[i].Contains("etup") || lodsf[i].Contains("dotnet") || lodsf[i].Contains("SETUP")
                        || lodsf[i].Contains("edist") || lodsf[i].Contains("nstall"))
                        lodsf.RemoveAt(i);
                for (int i = lodsf.Count - 1; i >= 0; i--)
                    for (int j = programpaths.Count - 1; j >= 0; j--)
                        if (lodsf[i] == programpaths[j])
                            lodsf.RemoveAt(i);
                foreach (string st in lodsf)
                {
                    int index = programpaths.IndexOf(st);
                    iLIcons.Images.Add(Icon.ExtractAssociatedIcon(st));
                    ListViewItem lvi = new ListViewItem(Path.GetFileNameWithoutExtension(st), iLIcons.Images.Count + index);
                    lvi.SubItems.Add(st);
                    lVPrograms.Items.Add(lvi);
                }
                bnLoadSteam.Visible = false;
                lVPrograms.Size = new Size(lVPrograms.Size.Width, lVPrograms.Size.Height + 25);
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
            if (openProgram.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string file = openProgram.FileName;
                lBProgramPath.Text = file;
                iLIcons.Images.Add(Icon.ExtractAssociatedIcon(file));
                ListViewItem lvi = new ListViewItem(Path.GetFileNameWithoutExtension(file), lVPrograms.Items.Count);
                lvi.SubItems.Add(file);
                lVPrograms.Items.Insert(0, lvi);
            }
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

    }
}
