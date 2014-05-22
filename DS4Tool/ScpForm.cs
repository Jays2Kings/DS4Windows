using System;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using DS4Control;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using System.Net;
using System.Management;
using Microsoft.Win32;
namespace ScpServer
{
    public partial class ScpForm : Form
    {
        private DS4Control.Control rootHub;
        delegate void LogDebugDelegate(DateTime Time, String Data);
        double version = 7.4;

        protected Label[] Pads;
        protected ComboBox[] cbs;
        protected Button[] ebns;
        protected Button[] dbns;
        protected Label[] protexts;
        protected ToolStripMenuItem[] shortcuts;
        WebClient wc = new WebClient();
        public ScpForm()
        {
            InitializeComponent();

            ThemeUtil.SetTheme(lvDebug);

            Pads = new Label[4] { lbPad1, lbPad2, lbPad3, lbPad4 };
            cbs = new ComboBox[4] { cBController1, cBController2, cBController3, cBController4 };
            ebns = new Button[4] { bnEditC1, bnEditC2, bnEditC3, bnEditC4 };
            dbns = new Button[4] { bnDeleteC1, bnDeleteC2, bnDeleteC3, bnDeleteC4 };
            protexts = new Label[4] { lbSelPro1, lbSelPro2, lbSelPro3, lbSelPro4 };
            
            shortcuts = new ToolStripMenuItem[4] { (ToolStripMenuItem)notifyIcon1.ContextMenuStrip.Items[0],
                (ToolStripMenuItem)notifyIcon1.ContextMenuStrip.Items[1],
                (ToolStripMenuItem)notifyIcon1.ContextMenuStrip.Items[2],
                (ToolStripMenuItem)notifyIcon1.ContextMenuStrip.Items[3] };
            foreach (ToolStripMenuItem t in shortcuts)
                t.DropDownItemClicked += Profile_Changed_Menu;
            CheckDrivers(); 
        }

        private void CheckDrivers()
        {
            bool deriverinstalled = false;
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PnPSignedDriver");
            foreach (ManagementObject obj in searcher.Get())
            {
                try
                {
                    if (obj.GetPropertyValue("DeviceName").ToString() == "Scp Virtual Bus Driver")
                    {
                        deriverinstalled = true;
                        break;
                    }
                }
                catch { }
            }
            if (!deriverinstalled)
            {
                WelcomeDialog wd = new WelcomeDialog();
                wd.ShowDialog();
            }
        }
        
        private void Check_Version(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            double newversion;
            try
            {
                if (double.TryParse(File.ReadAllText(Global.appdatapath + "\\version.txt"), out newversion))
                {
                    if (newversion > version)
                        if (MessageBox.Show("Download now?", "New Version Available!", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == System.Windows.Forms.DialogResult.Yes)
                        {
                            if (!File.Exists("Updater.exe"))
                            {
                                Uri url2 = new Uri("https://dl.dropboxusercontent.com/u/16364552/DS4Tool/Updater.exe");
                                WebClient wc2 = new WebClient();
                                wc2.DownloadFile(url2, "Updater.exe");
                            }
                            System.Diagnostics.Process.Start("Updater.exe");
                            this.Close();
                        }
                        else
                            File.Delete(Global.appdatapath + "\\version.txt");
                    else
                        File.Delete(Global.appdatapath + "\\version.txt");
                }
                else
                    File.Delete(Global.appdatapath + "\\version.txt");
            }
            catch { };
        }

        protected void Form_Load(object sender, EventArgs e)
        {
            Icon = Properties.Resources.DS4;
            notifyIcon1.Icon = Properties.Resources.DS4;
            rootHub = new DS4Control.Control();
            rootHub.Debug += On_Debug;
            Log.GuiLog += On_Debug;
            Log.TrayIconLog += ShowNotification;
            // tmrUpdate.Enabled = true; TODO remove tmrUpdate and leave tick()
            Global.Load();
            Global.setVersion(version);
            Global.Save();
            hideDS4CheckBox.CheckedChanged -= hideDS4CheckBox_CheckedChanged;
            hideDS4CheckBox.Checked = Global.getUseExclusiveMode();
            hideDS4CheckBox.CheckedChanged += hideDS4CheckBox_CheckedChanged;

            // New settings
            this.Width = Global.getFormWidth();
            this.Height = Global.getFormHeight();
            startMinimizedCheckBox.CheckedChanged -= startMinimizedCheckBox_CheckedChanged;
            startMinimizedCheckBox.Checked = Global.getStartMinimized();
            startMinimizedCheckBox.CheckedChanged += startMinimizedCheckBox_CheckedChanged;

            RegistryKey KeyLoc = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", false);
            StartWindowsCheckBox.Checked = (KeyLoc.GetValue("DS4Tool") != null);

            if (startMinimizedCheckBox.Checked)
            {
                this.WindowState = FormWindowState.Minimized;
                Form_Resize(sender, e);
            }
            RefreshProfiles();
            for (int i = 0; i < 4; i++)
                Global.LoadProfile(i);
            Global.ControllerStatusChange += ControllerStatusChange;
            ControllerStatusChanged();
            if (btnStartStop.Enabled)
                btnStartStop_Clicked();
            Uri url = new Uri("https://dl.dropboxusercontent.com/u/16364552/DS4Tool/newest%20version.txt"); //Sorry other devs, gonna have to find your own server
            Directory.CreateDirectory(Global.appdatapath);
            if (DateTime.Now >= Global.getLastChecked() + TimeSpan.FromHours(1))
            {
                wc.DownloadFileAsync(url, Global.appdatapath + "\\version.txt");
                wc.DownloadFileCompleted += Check_Version;
                Global.setLastChecked(DateTime.Now);
            }
        }
        public void RefreshProfiles()
        {
            try
            {
                string[] profiles = Directory.GetFiles(Global.appdatapath + @"\Profiles\");
                List<string> profilenames = new List<string>();
                foreach (String s in profiles)
                    if (s.EndsWith(".xml"))
                        profilenames.Add(Path.GetFileNameWithoutExtension(s));
                for (int i = 0; i < 4; i++)
                {
                    cbs[i].Items.Clear();
                    shortcuts[i].DropDownItems.Clear();
                    cbs[i].Items.AddRange(profilenames.ToArray());
                    foreach (string s in profilenames)
                        shortcuts[i].DropDownItems.Add(Path.GetFileNameWithoutExtension(s));
                    for (int j = 0; j < cbs[i].Items.Count; j++)
                        if (cbs[i].Items[j].ToString() == Path.GetFileNameWithoutExtension(Global.getAProfile(i)))
                        {
                            cbs[i].SelectedIndex = j;
                            ((ToolStripMenuItem)shortcuts[i].DropDownItems[j]).Checked = true;
                            Global.setAProfile(i, cbs[i].Text);
                            shortcuts[i].Text = "Edit Profile for Controller " + (i + 1);
                            ebns[i].Text = "Edit";
                            break;
                        }
                        else
                        {
                            cbs[i].Text = "(No Profile Loaded)";
                            shortcuts[i].Text = "Make Profile for Controller " + (i + 1);
                            ebns[i].Text = "New";
                        }
                    cbs[i].Items.Add("+New Profile");
                    shortcuts[i].DropDownItems.Add("-");
                    shortcuts[i].DropDownItems.Add("+New Profile");
                }
            }
            catch (DirectoryNotFoundException)
            {
                if (Directory.Exists(Directory.GetParent(Assembly.GetExecutingAssembly().Location).FullName + @"\Profiles\"))
                    MessageBox.Show("Please import or make a profile", "Profile Folder Moved");
                Directory.CreateDirectory(Global.appdatapath + @"\Profiles\");
                for (int i = 0; i < 4; i++)
                {
                    cbs[i].Text = "(No Profile Loaded)";
                    shortcuts[i].Text = "Make Profile for Controller " + (i + 1);
                    ebns[i].Text = "New";
                    cbs[i].Items.Add("+New Profile");
                    shortcuts[i].DropDownItems.Add("-");
                    shortcuts[i].DropDownItems.Add("+New Profile");
                }
            }
        }

        protected void LogDebug(DateTime Time, String Data)
        {
            if (lvDebug.InvokeRequired)
            {
                LogDebugDelegate d = new LogDebugDelegate(LogDebug);
                try
                {
                    this.Invoke(d, new Object[] { Time, Data });
                }
                catch { }
            }
            else
            {
                String Posted = Time.ToString("G");

                lvDebug.Items.Add(new ListViewItem(new String[] { Posted, Data })).EnsureVisible();

                //Added alternative
                lbLastMessage.Text = Data;
            }
        }

        protected void ShowNotification(object sender, DebugEventArgs args)
        {
            notifyIcon1.BalloonTipText = args.Data;
            notifyIcon1.BalloonTipTitle = "DS4Windows";
            notifyIcon1.ShowBalloonTip(1);
        }

        protected void Form_Resize(object sender, EventArgs e)
        {
            if (FormWindowState.Minimized == this.WindowState)
            {
                notifyIcon1.Visible = true;
                this.Hide();
                this.ShowInTaskbar = false;
            }

            else if (FormWindowState.Normal == this.WindowState)
            {
                notifyIcon1.Visible = false;
                this.Show();
                this.ShowInTaskbar = true;
            }

            //Added last message alternative

            if (this.Height > 220)
                lbLastMessage.Visible = false;
            else lbLastMessage.Visible = true;
            if (protexts != null)
                for (int i = 0; i < 4; i++)
                    if (this.Width > 665)
                        protexts[i].Visible = true;
                    else
                        protexts[i].Visible = false;
            StartWindowsCheckBox.Visible = (this.Width > 665);
        }

        protected void btnStartStop_Click(object sender, EventArgs e)
        {
            btnStartStop_Clicked();
        }
        protected void btnStartStop_Clicked()
        {
            if (btnStartStop.Text == Properties.Resources.Start
                && rootHub.Start())
                btnStartStop.Text = Properties.Resources.Stop;
            else if (btnStartStop.Text == Properties.Resources.Stop
                && rootHub.Stop())
                btnStartStop.Text = Properties.Resources.Start;
        }
        protected void btnStop_Click(object sender, EventArgs e)
        {
            if (rootHub.Stop())
            {
                btnStartStop.Enabled = true;
                btnStop.Enabled = false;
            }
        }
        protected void btnClear_Click(object sender, EventArgs e)
        {
            lvDebug.Items.Clear();
            //Added alternative
            lbLastMessage.Text = string.Empty;
        }

        protected override void WndProc(ref Message m)
        {
            try
            {
                if (m.Msg == ScpDevice.WM_DEVICECHANGE)
                {
                    Int32 Type = m.WParam.ToInt32();
                    lock (this)
                    {
                        rootHub.HotPlug();
                    }
                }
            }
            catch { }

            base.WndProc(ref m);
        }

        delegate void ControllerStatusChangedDelegate(object sender, EventArgs e);
        protected void ControllerStatusChange(object sender, EventArgs e)
        {
            if (InvokeRequired)
                Invoke(new ControllerStatusChangedDelegate(ControllerStatusChange), new object[] { sender, e });
            else
                ControllerStatusChanged();
        }
        protected void ControllerStatusChanged()
        {
            // If controllers are detected, but not checked, automatically check #1
            //bool checkFirst = true;
            String tooltip = "DS4Windows";
            for (Int32 Index = 0; Index < Pads.Length; Index++)
            {
                Pads[Index].Text = rootHub.getDS4ControllerInfo(Index);
                if (Pads[Index].Text != String.Empty)
                {
                    Pads[Index].Enabled = true;
                    cbs[Index].Enabled = true;
                    ebns[Index].Enabled = true;
                    dbns[Index].Enabled = true;
                    protexts[Index].Enabled = true;
                    shortcuts[Index].Enabled = true;
                    // As above
                    //if (checkFirst && (Pads[Index].Checked && Index != 0))
                    // checkFirst = false;
                }
                else
                {
                    Pads[Index].Text = "Disconnected";
                    Pads[Index].Enabled = false;
                    cbs[Index].Enabled = false;
                    ebns[Index].Enabled = false;
                    dbns[Index].Enabled = false;
                    protexts[Index].Enabled = false;
                    if (OptionsDialog[Index] != null)
                        OptionsDialog[Index].Close();
                    shortcuts[Index].Enabled = false;
                    // As above
                    //if (Index == 0)
                    // checkFirst = false;
                }
                if (rootHub.getShortDS4ControllerInfo(Index) != "None")
                    tooltip += "\n" + (Index + 1) + ": " + rootHub.getShortDS4ControllerInfo(Index); // Carefully stay under the 63 character limit.
            }
            btnClear.Enabled = lvDebug.Items.Count > 0;

            // As above
            //if (checkFirst && btnClear.Enabled)
            // Pads[0].Checked = true;
            notifyIcon1.Text = tooltip;
        }
        protected void On_Debug(object sender, DS4Control.DebugEventArgs e)
        {
            LogDebug(e.Time, e.Data);
        }

        private Options[] OptionsDialog = { null, null, null, null };
        private void ShowOptions(int devID, string profile)
        {
            if (OptionsDialog[devID] == null)
            {
                Options opt;
                opt = OptionsDialog[devID] = new Options(rootHub, devID, profile);
                opt.Text = "Options for Controller " + (devID + 1);
                opt.Icon = this.Icon;
                opt.FormClosed += delegate { OptionsDialog[devID] = null; RefreshProfiles(); };
                opt.Show();
            }
        }
        private void editButtons_Click(object sender, EventArgs e)
        {
            Button bn = (Button)sender;
            int i = Int32.Parse(bn.Tag.ToString());
            if (cbs[i].Text == "(No Profile Loaded)")
                    ShowOptions(i, "");
                else
                    ShowOptions(i, cbs[i].Text);
        }
        private void editMenu_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem em = (ToolStripMenuItem)sender;
            int i = Int32.Parse(em.Tag.ToString());
                if (em.Text == "Make Profile for Controller " + (i + 1))
                    ShowOptions(i, "");
                else
                    for (int t=0; t < em.DropDownItems.Count-2; t++)
                        if (((ToolStripMenuItem)em.DropDownItems[t]).Checked)
                            ShowOptions(i, ((ToolStripMenuItem)em.DropDownItems[t]).Text);
        }
        private void Enable_Controls(int device, bool on)
        {
            ebns[device].Enabled = on;
            dbns[device].Enabled = on;
            cbs[device].Enabled = on;
            shortcuts[device].Enabled = on;
        }
        private void deleteButtons_Click(object sender, EventArgs e)
        {
            Button bn = (Button)sender;
            int tdevice = Int32.Parse(bn.Tag.ToString());
            string filename = cbs[tdevice].Items[cbs[tdevice].SelectedIndex].ToString();
            if (MessageBox.Show("\"" + filename + "\" cannot be restored.", "Delete Profile?", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == System.Windows.Forms.DialogResult.Yes)
            {
                System.IO.File.Delete(Global.appdatapath + "\\Profiles\\" +filename + ".xml");
                Global.setAProfile(tdevice, null);
                RefreshProfiles();
            }
        }

        private void hotkeysButton_Click(object sender, EventArgs e)
        {
            Hotkeys hotkeysForm = new Hotkeys();
            hotkeysForm.Icon = this.Icon;
            hotkeysForm.ShowDialog();
        }
        private void lnkControllers_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("control", "joy.cpl");
        }

        private void hideDS4CheckBox_CheckedChanged(object sender, EventArgs e)
        {
            // Prevent the Game Controllers window from throwing an error when controllers are un/hidden
            System.Diagnostics.Process[] rundll32 = System.Diagnostics.Process.GetProcessesByName("rundll32");
            foreach (System.Diagnostics.Process rundll32Instance in rundll32)
                foreach (System.Diagnostics.ProcessModule module in rundll32Instance.Modules)
                    if (module.FileName.Contains("joy.cpl"))
                        module.Dispose();

            Global.setUseExclusiveMode(hideDS4CheckBox.Checked);
            btnStartStop_Clicked();
            btnStartStop_Clicked();
            Global.Save();
        }
        private void startMinimizedCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            Global.setStartMinimized(startMinimizedCheckBox.Checked);
            Global.Save();
        }

        private void lvDebug_ItemActivate(object sender, EventArgs e)
        {
            MessageBox.Show(((ListView)sender).FocusedItem.SubItems[1].Text, "Log");
        }

        private void Profile_Changed(object sender, EventArgs e)
        {
            ComboBox cb = (ComboBox)sender;
            int tdevice = Int32.Parse(cb.Tag.ToString());
            if (cb.Items[cb.Items.Count - 1].ToString() == "+New Profile")
            {
                if (cb.SelectedIndex < cb.Items.Count - 1)
                {
                    for (int i = 0; i < shortcuts[tdevice].DropDownItems.Count; i++)
                        if (!(shortcuts[tdevice].DropDownItems[i] is ToolStripSeparator))
                            ((ToolStripMenuItem)shortcuts[tdevice].DropDownItems[i]).Checked = false;
                    ((ToolStripMenuItem)shortcuts[tdevice].DropDownItems[cb.SelectedIndex]).Checked = true;
                    LogDebug(DateTime.Now, "Controller " + (tdevice + 1) + " is now using Profile \"" + cb.Text + "\"");
                    shortcuts[tdevice].Text = "Edit Profile for Controller " + (tdevice + 1);
                    Global.setAProfile(tdevice, cb.Items[cb.SelectedIndex].ToString());
                    Global.Save();
                    Global.LoadProfile(tdevice);
                }
                else if (cb.SelectedIndex == cb.Items.Count - 1 && cb.Items.Count > 1) //if +New Profile selected
                    ShowOptions(tdevice, "");
                if (cb.Text == "(No Profile Loaded)")
                    ebns[tdevice].Text = "New";
                else
                    ebns[tdevice].Text = "Edit";
            }
        }

        private void Profile_Changed_Menu(object sender, ToolStripItemClickedEventArgs e)
        {
            ToolStripMenuItem tS = (ToolStripMenuItem)sender;
            int tdevice = Int32.Parse(tS.Tag.ToString());
            if (!(e.ClickedItem is ToolStripSeparator))
                if (e.ClickedItem != tS.DropDownItems[tS.DropDownItems.Count - 1]) //if +New Profile not selected 
                    if (((ToolStripMenuItem)e.ClickedItem).Checked)
                        ShowOptions(tdevice, e.ClickedItem.Text);
                    else
                    {
                        for (int i = 0; i < tS.DropDownItems.Count; i++)
                            if (!(shortcuts[tdevice].DropDownItems[i] is ToolStripSeparator))
                                ((ToolStripMenuItem)tS.DropDownItems[i]).Checked = false;
                        ((ToolStripMenuItem)e.ClickedItem).Checked = true;
                        shortcuts[tdevice].Text = "Edit Profile for Controller " + (tdevice + 1);
                        cbs[tdevice].SelectedIndex = tS.DropDownItems.IndexOf(e.ClickedItem);
                        Global.setAProfile(tdevice, e.ClickedItem.Text);
                        Global.Save();
                        Global.LoadProfile(tdevice);
                    }
                else //if +New Profile selected
                    ShowOptions(tdevice, "");
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            notifyIcon_Click(sender, e);
        }

        private void notifyIcon_Click(object sender, EventArgs e)
        {
            this.Show();
            WindowState = FormWindowState.Normal;
        }

        private void linkProfiles_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            openProfiles.InitialDirectory = Directory.GetParent(Assembly.GetExecutingAssembly().Location).FullName + @"\Profiles\";
            if (openProfiles.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string[] files = openProfiles .FileNames;
                for (int i = 0; i < files.Length; i++)
                {
                    string[] temp = files[i].Split('\\');
                    files[i] = temp[temp.Length-1];
                    File.Copy(openProfiles.FileNames[i], Global.appdatapath + "\\Profiles\\" + files[i], true);
                }
                RefreshProfiles();
            }
        }

        private void llbHelp_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Hotkeys hotkeysForm = new Hotkeys();
            hotkeysForm.Icon = this.Icon;
            hotkeysForm.ShowDialog();
        }

        private void btnImportProfiles_Click(object sender, EventArgs e)
        {
            openProfiles.InitialDirectory = Directory.GetParent(Assembly.GetExecutingAssembly().Location).FullName + @"\Profiles\";
            if (openProfiles.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string[] files = openProfiles.FileNames;
                for (int i = 0; i < files.Length; i++)
                {
                    string[] temp = files[i].Split('\\');
                    files[i] = temp[temp.Length - 1];
                    File.Copy(openProfiles.FileNames[i], Global.appdatapath + "\\Profiles\\" + files[i], true);
                }
                RefreshProfiles();
            }
        }

        protected void Form_Close(object sender, FormClosingEventArgs e)
        {
            Global.setFormWidth(this.Width);
            Global.setFormHeight(this.Height);
            Global.Save();
            rootHub.Stop();
        }

        private void ScpForm_Deactivate(object sender, EventArgs e)
        {
            try { notifyIcon1.Visible = true; }
            catch { }
        }

        private void ScpForm_Activated(object sender, EventArgs e)
        {
            notifyIcon1.Visible = false;

        }

        private void StartWindowsCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            RegistryKey KeyLoc = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            if (StartWindowsCheckBox.Checked)
                KeyLoc.SetValue("DS4Tool", "\"" + Application.ExecutablePath.ToString() + "\"");
            else
                KeyLoc.DeleteValue("DS4Tool", false);
        }

    }

    public class ThemeUtil
    {
        [DllImport("UxTheme", CharSet = CharSet.Unicode, ExactSpelling = true)]
        private static extern int SetWindowTheme(IntPtr hWnd, String appName, String partList);

        public static void SetTheme(ListView lv)
        {
            try
            {
                SetWindowTheme(lv.Handle, "Explorer", null);
            }
            catch { }
        }

        public static void SetTheme(TreeView tv)
        {
            try
            {
                SetWindowTheme(tv.Handle, "Explorer", null);
            }
            catch { }
        }
    }
}
