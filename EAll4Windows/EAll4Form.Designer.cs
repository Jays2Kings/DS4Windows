namespace EAll4Windows
{
    partial class EAll4Form
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EAll4Form));
            this.lvDebug = new System.Windows.Forms.ListView();
            this.chTime = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.chData = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.tmrUpdate = new System.Windows.Forms.Timer(this.components);
            this.pnlButton = new System.Windows.Forms.Panel();
            this.lbTest = new System.Windows.Forms.Label();
            this.btnStartStop = new System.Windows.Forms.Button();
            this.lbLastMessage = new System.Windows.Forms.Label();
            this.llbHelp = new System.Windows.Forms.LinkLabel();
            this.btnClear = new System.Windows.Forms.Button();
            this.notifyIcon1 = new System.Windows.Forms.NotifyIcon(this.components);
            this.cMTaskbar = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.editProfileForController1ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.editProfileForController2ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.editProfileForController3ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.editProfileForController4ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.startToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.openProfiles = new System.Windows.Forms.OpenFileDialog();
            this.tabMain = new System.Windows.Forms.TabControl();
            this.tabControllers = new System.Windows.Forms.TabPage();
            this.tLPControllers = new System.Windows.Forms.TableLayoutPanel();
            this.pBStatus1 = new System.Windows.Forms.PictureBox();
            this.lbPad1 = new System.Windows.Forms.Label();
            this.lbPad2 = new System.Windows.Forms.Label();
            this.bnEditC3 = new System.Windows.Forms.Button();
            this.bnEditC4 = new System.Windows.Forms.Button();
            this.lbPad3 = new System.Windows.Forms.Label();
            this.lbPad4 = new System.Windows.Forms.Label();
            this.cBController1 = new System.Windows.Forms.ComboBox();
            this.bnEditC2 = new System.Windows.Forms.Button();
            this.cBController2 = new System.Windows.Forms.ComboBox();
            this.cBController3 = new System.Windows.Forms.ComboBox();
            this.bnEditC1 = new System.Windows.Forms.Button();
            this.cBController4 = new System.Windows.Forms.ComboBox();
            this.lbSelectedProfile = new System.Windows.Forms.Label();
            this.lbID = new System.Windows.Forms.Label();
            this.lbStatus = new System.Windows.Forms.Label();
            this.lbBattery = new System.Windows.Forms.Label();
            this.lbBatt1 = new System.Windows.Forms.Label();
            this.lbBatt2 = new System.Windows.Forms.Label();
            this.lbBatt3 = new System.Windows.Forms.Label();
            this.lbBatt4 = new System.Windows.Forms.Label();
            this.pBStatus2 = new System.Windows.Forms.PictureBox();
            this.pBStatus3 = new System.Windows.Forms.PictureBox();
            this.pBStatus4 = new System.Windows.Forms.PictureBox();
            this.lbNoControllers = new System.Windows.Forms.Label();
            this.tabProfiles = new System.Windows.Forms.TabPage();
            this.lBProfiles = new System.Windows.Forms.ListBox();
            this.cMProfile = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.editToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.assignToController1ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.assignToController2ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.assignToController3ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.assignToController4ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.duplicateToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.newProfileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.importToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exportToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tSOptions = new System.Windows.Forms.ToolStrip();
            this.toolStripLabel1 = new System.Windows.Forms.ToolStripLabel();
            this.tSTBProfile = new System.Windows.Forms.ToolStripTextBox();
            this.tSBSaveProfile = new System.Windows.Forms.ToolStripButton();
            this.tSBCancel = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.tSBKeepSize = new System.Windows.Forms.ToolStripButton();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.tsBNewProfle = new System.Windows.Forms.ToolStripButton();
            this.tsBEditProfile = new System.Windows.Forms.ToolStripButton();
            this.tsBDeleteProfile = new System.Windows.Forms.ToolStripButton();
            this.tSBDupProfile = new System.Windows.Forms.ToolStripButton();
            this.tSBImportProfile = new System.Windows.Forms.ToolStripButton();
            this.tSBExportProfile = new System.Windows.Forms.ToolStripButton();
            this.tabAutoProfiles = new System.Windows.Forms.TabPage();
            this.tabSettings = new System.Windows.Forms.TabPage();
            this.fLPSettings = new System.Windows.Forms.FlowLayoutPanel();
            this.hideEAll4CheckBox = new System.Windows.Forms.CheckBox();
            this.cBSwipeProfiles = new System.Windows.Forms.CheckBox();
            this.StartWindowsCheckBox = new System.Windows.Forms.CheckBox();
            this.startMinimizedCheckBox = new System.Windows.Forms.CheckBox();
            this.cBNotifications = new System.Windows.Forms.CheckBox();
            this.cBDisconnectBT = new System.Windows.Forms.CheckBox();
            this.cBFlashWhenLate = new System.Windows.Forms.CheckBox();
            this.cBCloseMini = new System.Windows.Forms.CheckBox();
            this.cBQuickCharge = new System.Windows.Forms.CheckBox();
            this.cBUpdate = new System.Windows.Forms.CheckBox();
            this.cBDownloadLangauge = new System.Windows.Forms.CheckBox();
            this.pNUpdate = new System.Windows.Forms.Panel();
            this.cBUpdateTime = new System.Windows.Forms.ComboBox();
            this.lbCheckEvery = new System.Windows.Forms.Label();
            this.nUDUpdateTime = new System.Windows.Forms.NumericUpDown();
            this.pnlXIPorts = new System.Windows.Forms.Panel();
            this.lbUseXIPorts = new System.Windows.Forms.Label();
            this.nUDXIPorts = new System.Windows.Forms.NumericUpDown();
            this.lbLastXIPort = new System.Windows.Forms.Label();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.linkProfiles = new System.Windows.Forms.LinkLabel();
            this.lnkControllers = new System.Windows.Forms.LinkLabel();
            this.linkUninstall = new System.Windows.Forms.LinkLabel();
            this.linkSetup = new System.Windows.Forms.LinkLabel();
            this.lLBUpdate = new System.Windows.Forms.LinkLabel();
            this.tabLog = new System.Windows.Forms.TabPage();
            this.saveProfiles = new System.Windows.Forms.SaveFileDialog();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.pnlButton.SuspendLayout();
            this.cMTaskbar.SuspendLayout();
            this.tabMain.SuspendLayout();
            this.tabControllers.SuspendLayout();
            this.tLPControllers.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pBStatus1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pBStatus2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pBStatus3)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pBStatus4)).BeginInit();
            this.tabProfiles.SuspendLayout();
            this.cMProfile.SuspendLayout();
            this.tSOptions.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            this.tabSettings.SuspendLayout();
            this.fLPSettings.SuspendLayout();
            this.pNUpdate.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nUDUpdateTime)).BeginInit();
            this.pnlXIPorts.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nUDXIPorts)).BeginInit();
            this.flowLayoutPanel1.SuspendLayout();
            this.tabLog.SuspendLayout();
            this.SuspendLayout();
            // 
            // lvDebug
            // 
            resources.ApplyResources(this.lvDebug, "lvDebug");
            this.lvDebug.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.chTime,
            this.chData});
            this.lvDebug.FullRowSelect = true;
            this.lvDebug.Name = "lvDebug";
            this.toolTip1.SetToolTip(this.lvDebug, resources.GetString("lvDebug.ToolTip"));
            this.lvDebug.UseCompatibleStateImageBehavior = false;
            this.lvDebug.View = System.Windows.Forms.View.Details;
            this.lvDebug.ItemActivate += new System.EventHandler(this.lvDebug_ItemActivate);
            // 
            // chTime
            // 
            resources.ApplyResources(this.chTime, "chTime");
            // 
            // chData
            // 
            resources.ApplyResources(this.chData, "chData");
            // 
            // tmrUpdate
            // 
            this.tmrUpdate.Interval = 1;
            this.tmrUpdate.Tick += new System.EventHandler(this.ControllerStatusChange);
            // 
            // pnlButton
            // 
            resources.ApplyResources(this.pnlButton, "pnlButton");
            this.pnlButton.BackColor = System.Drawing.SystemColors.Control;
            this.pnlButton.Controls.Add(this.lbTest);
            this.pnlButton.Controls.Add(this.btnStartStop);
            this.pnlButton.Controls.Add(this.lbLastMessage);
            this.pnlButton.Controls.Add(this.llbHelp);
            this.pnlButton.Name = "pnlButton";
            this.toolTip1.SetToolTip(this.pnlButton, resources.GetString("pnlButton.ToolTip"));
            this.pnlButton.MouseLeave += new System.EventHandler(this.pnlButton_MouseLeave);
            // 
            // lbTest
            // 
            resources.ApplyResources(this.lbTest, "lbTest");
            this.lbTest.Name = "lbTest";
            this.toolTip1.SetToolTip(this.lbTest, resources.GetString("lbTest.ToolTip"));
            // 
            // btnStartStop
            // 
            resources.ApplyResources(this.btnStartStop, "btnStartStop");
            this.btnStartStop.Name = "btnStartStop";
            this.toolTip1.SetToolTip(this.btnStartStop, resources.GetString("btnStartStop.ToolTip"));
            this.btnStartStop.UseVisualStyleBackColor = true;
            this.btnStartStop.Click += new System.EventHandler(this.btnStartStop_Click);
            // 
            // lbLastMessage
            // 
            resources.ApplyResources(this.lbLastMessage, "lbLastMessage");
            this.lbLastMessage.ForeColor = System.Drawing.SystemColors.GrayText;
            this.lbLastMessage.Name = "lbLastMessage";
            this.toolTip1.SetToolTip(this.lbLastMessage, resources.GetString("lbLastMessage.ToolTip"));
            this.lbLastMessage.MouseHover += new System.EventHandler(this.lbLastMessage_MouseHover);
            // 
            // llbHelp
            // 
            resources.ApplyResources(this.llbHelp, "llbHelp");
            this.llbHelp.Name = "llbHelp";
            this.llbHelp.TabStop = true;
            this.toolTip1.SetToolTip(this.llbHelp, resources.GetString("llbHelp.ToolTip"));
            this.llbHelp.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.llbHelp_LinkClicked);
            // 
            // btnClear
            // 
            resources.ApplyResources(this.btnClear, "btnClear");
            this.btnClear.Name = "btnClear";
            this.toolTip1.SetToolTip(this.btnClear, resources.GetString("btnClear.ToolTip"));
            this.btnClear.UseVisualStyleBackColor = true;
            this.btnClear.Click += new System.EventHandler(this.btnClear_Click);
            // 
            // notifyIcon1
            // 
            this.notifyIcon1.BalloonTipIcon = System.Windows.Forms.ToolTipIcon.Info;
            resources.ApplyResources(this.notifyIcon1, "notifyIcon1");
            this.notifyIcon1.ContextMenuStrip = this.cMTaskbar;
            this.notifyIcon1.BalloonTipClicked += new System.EventHandler(this.notifyIcon1_BalloonTipClicked);
            this.notifyIcon1.DoubleClick += new System.EventHandler(this.openToolStripMenuItem_Click);
            this.notifyIcon1.MouseClick += new System.Windows.Forms.MouseEventHandler(this.notifyIcon1_MouseClick);
            // 
            // cMTaskbar
            // 
            resources.ApplyResources(this.cMTaskbar, "cMTaskbar");
            this.cMTaskbar.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.cMTaskbar.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.editProfileForController1ToolStripMenuItem,
            this.editProfileForController2ToolStripMenuItem,
            this.editProfileForController3ToolStripMenuItem,
            this.editProfileForController4ToolStripMenuItem,
            this.toolStripSeparator1,
            this.startToolStripMenuItem,
            this.openToolStripMenuItem,
            this.exitToolStripMenuItem});
            this.cMTaskbar.Name = "cMTaskbar";
            this.cMTaskbar.Tag = "25";
            this.toolTip1.SetToolTip(this.cMTaskbar, resources.GetString("cMTaskbar.ToolTip"));
            // 
            // editProfileForController1ToolStripMenuItem
            // 
            resources.ApplyResources(this.editProfileForController1ToolStripMenuItem, "editProfileForController1ToolStripMenuItem");
            this.editProfileForController1ToolStripMenuItem.Name = "editProfileForController1ToolStripMenuItem";
            this.editProfileForController1ToolStripMenuItem.Tag = "0";
            this.editProfileForController1ToolStripMenuItem.Click += new System.EventHandler(this.editMenu_Click);
            // 
            // editProfileForController2ToolStripMenuItem
            // 
            resources.ApplyResources(this.editProfileForController2ToolStripMenuItem, "editProfileForController2ToolStripMenuItem");
            this.editProfileForController2ToolStripMenuItem.Name = "editProfileForController2ToolStripMenuItem";
            this.editProfileForController2ToolStripMenuItem.Tag = "1";
            this.editProfileForController2ToolStripMenuItem.Click += new System.EventHandler(this.editMenu_Click);
            // 
            // editProfileForController3ToolStripMenuItem
            // 
            resources.ApplyResources(this.editProfileForController3ToolStripMenuItem, "editProfileForController3ToolStripMenuItem");
            this.editProfileForController3ToolStripMenuItem.Name = "editProfileForController3ToolStripMenuItem";
            this.editProfileForController3ToolStripMenuItem.Tag = "2";
            this.editProfileForController3ToolStripMenuItem.Click += new System.EventHandler(this.editMenu_Click);
            // 
            // editProfileForController4ToolStripMenuItem
            // 
            resources.ApplyResources(this.editProfileForController4ToolStripMenuItem, "editProfileForController4ToolStripMenuItem");
            this.editProfileForController4ToolStripMenuItem.Name = "editProfileForController4ToolStripMenuItem";
            this.editProfileForController4ToolStripMenuItem.Tag = "4";
            this.editProfileForController4ToolStripMenuItem.Click += new System.EventHandler(this.editMenu_Click);
            // 
            // toolStripSeparator1
            // 
            resources.ApplyResources(this.toolStripSeparator1, "toolStripSeparator1");
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            // 
            // startToolStripMenuItem
            // 
            resources.ApplyResources(this.startToolStripMenuItem, "startToolStripMenuItem");
            this.startToolStripMenuItem.Name = "startToolStripMenuItem";
            this.startToolStripMenuItem.Click += new System.EventHandler(this.startToolStripMenuItem_Click);
            // 
            // openToolStripMenuItem
            // 
            resources.ApplyResources(this.openToolStripMenuItem, "openToolStripMenuItem");
            this.openToolStripMenuItem.Name = "openToolStripMenuItem";
            this.openToolStripMenuItem.Click += new System.EventHandler(this.openToolStripMenuItem_Click);
            // 
            // exitToolStripMenuItem
            // 
            resources.ApplyResources(this.exitToolStripMenuItem, "exitToolStripMenuItem");
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // toolStripSeparator2
            // 
            resources.ApplyResources(this.toolStripSeparator2, "toolStripSeparator2");
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            // 
            // openProfiles
            // 
            resources.ApplyResources(this.openProfiles, "openProfiles");
            this.openProfiles.Multiselect = true;
            // 
            // tabMain
            // 
            resources.ApplyResources(this.tabMain, "tabMain");
            this.tabMain.Controls.Add(this.tabControllers);
            this.tabMain.Controls.Add(this.tabProfiles);
            this.tabMain.Controls.Add(this.tabAutoProfiles);
            this.tabMain.Controls.Add(this.tabSettings);
            this.tabMain.Controls.Add(this.tabLog);
            this.tabMain.Name = "tabMain";
            this.tabMain.SelectedIndex = 0;
            this.toolTip1.SetToolTip(this.tabMain, resources.GetString("tabMain.ToolTip"));
            this.tabMain.SelectedIndexChanged += new System.EventHandler(this.tabMain_SelectedIndexChanged);
            // 
            // tabControllers
            // 
            resources.ApplyResources(this.tabControllers, "tabControllers");
            this.tabControllers.Controls.Add(this.tLPControllers);
            this.tabControllers.Controls.Add(this.lbNoControllers);
            this.tabControllers.Name = "tabControllers";
            this.toolTip1.SetToolTip(this.tabControllers, resources.GetString("tabControllers.ToolTip"));
            this.tabControllers.UseVisualStyleBackColor = true;
            // 
            // tLPControllers
            // 
            resources.ApplyResources(this.tLPControllers, "tLPControllers");
            this.tLPControllers.Controls.Add(this.pBStatus1, 1, 1);
            this.tLPControllers.Controls.Add(this.lbPad1, 0, 1);
            this.tLPControllers.Controls.Add(this.lbPad2, 0, 2);
            this.tLPControllers.Controls.Add(this.bnEditC3, 4, 3);
            this.tLPControllers.Controls.Add(this.bnEditC4, 4, 4);
            this.tLPControllers.Controls.Add(this.lbPad3, 0, 3);
            this.tLPControllers.Controls.Add(this.lbPad4, 0, 4);
            this.tLPControllers.Controls.Add(this.cBController1, 3, 1);
            this.tLPControllers.Controls.Add(this.bnEditC2, 4, 2);
            this.tLPControllers.Controls.Add(this.cBController2, 3, 2);
            this.tLPControllers.Controls.Add(this.cBController3, 3, 3);
            this.tLPControllers.Controls.Add(this.bnEditC1, 4, 1);
            this.tLPControllers.Controls.Add(this.cBController4, 3, 4);
            this.tLPControllers.Controls.Add(this.lbSelectedProfile, 3, 0);
            this.tLPControllers.Controls.Add(this.lbID, 0, 0);
            this.tLPControllers.Controls.Add(this.lbStatus, 1, 0);
            this.tLPControllers.Controls.Add(this.lbBattery, 2, 0);
            this.tLPControllers.Controls.Add(this.lbBatt1, 2, 1);
            this.tLPControllers.Controls.Add(this.lbBatt2, 2, 2);
            this.tLPControllers.Controls.Add(this.lbBatt3, 2, 3);
            this.tLPControllers.Controls.Add(this.lbBatt4, 2, 4);
            this.tLPControllers.Controls.Add(this.pBStatus2, 1, 2);
            this.tLPControllers.Controls.Add(this.pBStatus3, 1, 3);
            this.tLPControllers.Controls.Add(this.pBStatus4, 1, 4);
            this.tLPControllers.Name = "tLPControllers";
            this.toolTip1.SetToolTip(this.tLPControllers, resources.GetString("tLPControllers.ToolTip"));
            // 
            // pBStatus1
            // 
            resources.ApplyResources(this.pBStatus1, "pBStatus1");
            this.pBStatus1.InitialImage = global::EAll4Windows.Properties.Resources.BT;
            this.pBStatus1.Name = "pBStatus1";
            this.pBStatus1.TabStop = false;
            this.pBStatus1.Tag = "0";
            this.toolTip1.SetToolTip(this.pBStatus1, resources.GetString("pBStatus1.ToolTip"));
            this.pBStatus1.MouseClick += new System.Windows.Forms.MouseEventHandler(this.pBStatus_MouseClick);
            // 
            // lbPad1
            // 
            resources.ApplyResources(this.lbPad1, "lbPad1");
            this.lbPad1.Name = "lbPad1";
            this.lbPad1.Tag = "0";
            this.toolTip1.SetToolTip(this.lbPad1, resources.GetString("lbPad1.ToolTip"));
            this.lbPad1.MouseLeave += new System.EventHandler(this.Pads_MouseLeave);
            this.lbPad1.MouseHover += new System.EventHandler(this.Pads_MouseHover);
            // 
            // lbPad2
            // 
            resources.ApplyResources(this.lbPad2, "lbPad2");
            this.lbPad2.Name = "lbPad2";
            this.lbPad2.Tag = "1";
            this.toolTip1.SetToolTip(this.lbPad2, resources.GetString("lbPad2.ToolTip"));
            this.lbPad2.MouseLeave += new System.EventHandler(this.Pads_MouseLeave);
            this.lbPad2.MouseHover += new System.EventHandler(this.Pads_MouseHover);
            // 
            // bnEditC3
            // 
            resources.ApplyResources(this.bnEditC3, "bnEditC3");
            this.bnEditC3.Name = "bnEditC3";
            this.bnEditC3.Tag = "2";
            this.toolTip1.SetToolTip(this.bnEditC3, resources.GetString("bnEditC3.ToolTip"));
            this.bnEditC3.UseVisualStyleBackColor = true;
            this.bnEditC3.Click += new System.EventHandler(this.editButtons_Click);
            // 
            // bnEditC4
            // 
            resources.ApplyResources(this.bnEditC4, "bnEditC4");
            this.bnEditC4.Name = "bnEditC4";
            this.bnEditC4.Tag = "3";
            this.toolTip1.SetToolTip(this.bnEditC4, resources.GetString("bnEditC4.ToolTip"));
            this.bnEditC4.UseVisualStyleBackColor = true;
            this.bnEditC4.Click += new System.EventHandler(this.editButtons_Click);
            // 
            // lbPad3
            // 
            resources.ApplyResources(this.lbPad3, "lbPad3");
            this.lbPad3.Name = "lbPad3";
            this.lbPad3.Tag = "2";
            this.toolTip1.SetToolTip(this.lbPad3, resources.GetString("lbPad3.ToolTip"));
            this.lbPad3.MouseLeave += new System.EventHandler(this.Pads_MouseLeave);
            this.lbPad3.MouseHover += new System.EventHandler(this.Pads_MouseHover);
            // 
            // lbPad4
            // 
            resources.ApplyResources(this.lbPad4, "lbPad4");
            this.lbPad4.Name = "lbPad4";
            this.lbPad4.Tag = "3";
            this.toolTip1.SetToolTip(this.lbPad4, resources.GetString("lbPad4.ToolTip"));
            this.lbPad4.MouseLeave += new System.EventHandler(this.Pads_MouseLeave);
            this.lbPad4.MouseHover += new System.EventHandler(this.Pads_MouseHover);
            // 
            // cBController1
            // 
            resources.ApplyResources(this.cBController1, "cBController1");
            this.cBController1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cBController1.FormattingEnabled = true;
            this.cBController1.Name = "cBController1";
            this.cBController1.Tag = "0";
            this.toolTip1.SetToolTip(this.cBController1, resources.GetString("cBController1.ToolTip"));
            this.cBController1.SelectedValueChanged += new System.EventHandler(this.Profile_Changed);
            // 
            // bnEditC2
            // 
            resources.ApplyResources(this.bnEditC2, "bnEditC2");
            this.bnEditC2.Name = "bnEditC2";
            this.bnEditC2.Tag = "1";
            this.toolTip1.SetToolTip(this.bnEditC2, resources.GetString("bnEditC2.ToolTip"));
            this.bnEditC2.UseVisualStyleBackColor = true;
            this.bnEditC2.Click += new System.EventHandler(this.editButtons_Click);
            // 
            // cBController2
            // 
            resources.ApplyResources(this.cBController2, "cBController2");
            this.cBController2.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cBController2.FormattingEnabled = true;
            this.cBController2.Name = "cBController2";
            this.cBController2.Tag = "1";
            this.toolTip1.SetToolTip(this.cBController2, resources.GetString("cBController2.ToolTip"));
            this.cBController2.SelectedValueChanged += new System.EventHandler(this.Profile_Changed);
            // 
            // cBController3
            // 
            resources.ApplyResources(this.cBController3, "cBController3");
            this.cBController3.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cBController3.FormattingEnabled = true;
            this.cBController3.Name = "cBController3";
            this.cBController3.Tag = "2";
            this.toolTip1.SetToolTip(this.cBController3, resources.GetString("cBController3.ToolTip"));
            this.cBController3.SelectedValueChanged += new System.EventHandler(this.Profile_Changed);
            // 
            // bnEditC1
            // 
            resources.ApplyResources(this.bnEditC1, "bnEditC1");
            this.bnEditC1.Name = "bnEditC1";
            this.bnEditC1.Tag = "0";
            this.toolTip1.SetToolTip(this.bnEditC1, resources.GetString("bnEditC1.ToolTip"));
            this.bnEditC1.UseVisualStyleBackColor = true;
            this.bnEditC1.Click += new System.EventHandler(this.editButtons_Click);
            // 
            // cBController4
            // 
            resources.ApplyResources(this.cBController4, "cBController4");
            this.cBController4.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cBController4.FormattingEnabled = true;
            this.cBController4.Name = "cBController4";
            this.cBController4.Tag = "3";
            this.toolTip1.SetToolTip(this.cBController4, resources.GetString("cBController4.ToolTip"));
            this.cBController4.SelectedValueChanged += new System.EventHandler(this.Profile_Changed);
            // 
            // lbSelectedProfile
            // 
            resources.ApplyResources(this.lbSelectedProfile, "lbSelectedProfile");
            this.lbSelectedProfile.Name = "lbSelectedProfile";
            this.toolTip1.SetToolTip(this.lbSelectedProfile, resources.GetString("lbSelectedProfile.ToolTip"));
            // 
            // lbID
            // 
            resources.ApplyResources(this.lbID, "lbID");
            this.lbID.Name = "lbID";
            this.toolTip1.SetToolTip(this.lbID, resources.GetString("lbID.ToolTip"));
            // 
            // lbStatus
            // 
            resources.ApplyResources(this.lbStatus, "lbStatus");
            this.lbStatus.Name = "lbStatus";
            this.toolTip1.SetToolTip(this.lbStatus, resources.GetString("lbStatus.ToolTip"));
            // 
            // lbBattery
            // 
            resources.ApplyResources(this.lbBattery, "lbBattery");
            this.lbBattery.Name = "lbBattery";
            this.toolTip1.SetToolTip(this.lbBattery, resources.GetString("lbBattery.ToolTip"));
            // 
            // lbBatt1
            // 
            resources.ApplyResources(this.lbBatt1, "lbBatt1");
            this.lbBatt1.Name = "lbBatt1";
            this.toolTip1.SetToolTip(this.lbBatt1, resources.GetString("lbBatt1.ToolTip"));
            // 
            // lbBatt2
            // 
            resources.ApplyResources(this.lbBatt2, "lbBatt2");
            this.lbBatt2.Name = "lbBatt2";
            this.toolTip1.SetToolTip(this.lbBatt2, resources.GetString("lbBatt2.ToolTip"));
            // 
            // lbBatt3
            // 
            resources.ApplyResources(this.lbBatt3, "lbBatt3");
            this.lbBatt3.Name = "lbBatt3";
            this.toolTip1.SetToolTip(this.lbBatt3, resources.GetString("lbBatt3.ToolTip"));
            // 
            // lbBatt4
            // 
            resources.ApplyResources(this.lbBatt4, "lbBatt4");
            this.lbBatt4.Name = "lbBatt4";
            this.toolTip1.SetToolTip(this.lbBatt4, resources.GetString("lbBatt4.ToolTip"));
            // 
            // pBStatus2
            // 
            resources.ApplyResources(this.pBStatus2, "pBStatus2");
            this.pBStatus2.InitialImage = global::EAll4Windows.Properties.Resources.BT;
            this.pBStatus2.Name = "pBStatus2";
            this.pBStatus2.TabStop = false;
            this.pBStatus2.Tag = "1";
            this.toolTip1.SetToolTip(this.pBStatus2, resources.GetString("pBStatus2.ToolTip"));
            this.pBStatus2.MouseClick += new System.Windows.Forms.MouseEventHandler(this.pBStatus_MouseClick);
            // 
            // pBStatus3
            // 
            resources.ApplyResources(this.pBStatus3, "pBStatus3");
            this.pBStatus3.InitialImage = global::EAll4Windows.Properties.Resources.BT;
            this.pBStatus3.Name = "pBStatus3";
            this.pBStatus3.TabStop = false;
            this.pBStatus3.Tag = "2";
            this.toolTip1.SetToolTip(this.pBStatus3, resources.GetString("pBStatus3.ToolTip"));
            this.pBStatus3.MouseClick += new System.Windows.Forms.MouseEventHandler(this.pBStatus_MouseClick);
            // 
            // pBStatus4
            // 
            resources.ApplyResources(this.pBStatus4, "pBStatus4");
            this.pBStatus4.InitialImage = global::EAll4Windows.Properties.Resources.BT;
            this.pBStatus4.Name = "pBStatus4";
            this.pBStatus4.TabStop = false;
            this.pBStatus4.Tag = "3";
            this.toolTip1.SetToolTip(this.pBStatus4, resources.GetString("pBStatus4.ToolTip"));
            this.pBStatus4.MouseClick += new System.Windows.Forms.MouseEventHandler(this.pBStatus_MouseClick);
            // 
            // lbNoControllers
            // 
            resources.ApplyResources(this.lbNoControllers, "lbNoControllers");
            this.lbNoControllers.Name = "lbNoControllers";
            this.toolTip1.SetToolTip(this.lbNoControllers, resources.GetString("lbNoControllers.ToolTip"));
            // 
            // tabProfiles
            // 
            resources.ApplyResources(this.tabProfiles, "tabProfiles");
            this.tabProfiles.Controls.Add(this.lBProfiles);
            this.tabProfiles.Controls.Add(this.tSOptions);
            this.tabProfiles.Controls.Add(this.toolStrip1);
            this.tabProfiles.Name = "tabProfiles";
            this.toolTip1.SetToolTip(this.tabProfiles, resources.GetString("tabProfiles.ToolTip"));
            this.tabProfiles.UseVisualStyleBackColor = true;
            // 
            // lBProfiles
            // 
            resources.ApplyResources(this.lBProfiles, "lBProfiles");
            this.lBProfiles.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.lBProfiles.ContextMenuStrip = this.cMProfile;
            this.lBProfiles.FormattingEnabled = true;
            this.lBProfiles.MultiColumn = true;
            this.lBProfiles.Name = "lBProfiles";
            this.toolTip1.SetToolTip(this.lBProfiles, resources.GetString("lBProfiles.ToolTip"));
            this.lBProfiles.KeyDown += new System.Windows.Forms.KeyEventHandler(this.lBProfiles_KeyDown);
            this.lBProfiles.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.lBProfiles_MouseDoubleClick);
            this.lBProfiles.MouseDown += new System.Windows.Forms.MouseEventHandler(this.lBProfiles_MouseDown);
            // 
            // cMProfile
            // 
            resources.ApplyResources(this.cMProfile, "cMProfile");
            this.cMProfile.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.cMProfile.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.editToolStripMenuItem,
            this.assignToController1ToolStripMenuItem,
            this.assignToController2ToolStripMenuItem,
            this.assignToController3ToolStripMenuItem,
            this.assignToController4ToolStripMenuItem,
            this.deleteToolStripMenuItem,
            this.duplicateToolStripMenuItem,
            this.newProfileToolStripMenuItem,
            this.importToolStripMenuItem,
            this.exportToolStripMenuItem});
            this.cMProfile.Name = "cMProfile";
            this.toolTip1.SetToolTip(this.cMProfile, resources.GetString("cMProfile.ToolTip"));
            // 
            // editToolStripMenuItem
            // 
            resources.ApplyResources(this.editToolStripMenuItem, "editToolStripMenuItem");
            this.editToolStripMenuItem.Name = "editToolStripMenuItem";
            this.editToolStripMenuItem.Click += new System.EventHandler(this.tsBNEditProfile_Click);
            // 
            // assignToController1ToolStripMenuItem
            // 
            resources.ApplyResources(this.assignToController1ToolStripMenuItem, "assignToController1ToolStripMenuItem");
            this.assignToController1ToolStripMenuItem.Name = "assignToController1ToolStripMenuItem";
            this.assignToController1ToolStripMenuItem.Click += new System.EventHandler(this.assignToController1ToolStripMenuItem_Click);
            // 
            // assignToController2ToolStripMenuItem
            // 
            resources.ApplyResources(this.assignToController2ToolStripMenuItem, "assignToController2ToolStripMenuItem");
            this.assignToController2ToolStripMenuItem.Name = "assignToController2ToolStripMenuItem";
            this.assignToController2ToolStripMenuItem.Click += new System.EventHandler(this.assignToController2ToolStripMenuItem_Click);
            // 
            // assignToController3ToolStripMenuItem
            // 
            resources.ApplyResources(this.assignToController3ToolStripMenuItem, "assignToController3ToolStripMenuItem");
            this.assignToController3ToolStripMenuItem.Name = "assignToController3ToolStripMenuItem";
            this.assignToController3ToolStripMenuItem.Click += new System.EventHandler(this.assignToController3ToolStripMenuItem_Click);
            // 
            // assignToController4ToolStripMenuItem
            // 
            resources.ApplyResources(this.assignToController4ToolStripMenuItem, "assignToController4ToolStripMenuItem");
            this.assignToController4ToolStripMenuItem.Name = "assignToController4ToolStripMenuItem";
            this.assignToController4ToolStripMenuItem.Click += new System.EventHandler(this.assignToController4ToolStripMenuItem_Click);
            // 
            // deleteToolStripMenuItem
            // 
            resources.ApplyResources(this.deleteToolStripMenuItem, "deleteToolStripMenuItem");
            this.deleteToolStripMenuItem.Name = "deleteToolStripMenuItem";
            this.deleteToolStripMenuItem.Click += new System.EventHandler(this.tsBDeleteProfle_Click);
            // 
            // duplicateToolStripMenuItem
            // 
            resources.ApplyResources(this.duplicateToolStripMenuItem, "duplicateToolStripMenuItem");
            this.duplicateToolStripMenuItem.Name = "duplicateToolStripMenuItem";
            this.duplicateToolStripMenuItem.Click += new System.EventHandler(this.tSBDupProfile_Click);
            // 
            // newProfileToolStripMenuItem
            // 
            resources.ApplyResources(this.newProfileToolStripMenuItem, "newProfileToolStripMenuItem");
            this.newProfileToolStripMenuItem.Name = "newProfileToolStripMenuItem";
            this.newProfileToolStripMenuItem.Click += new System.EventHandler(this.tsBNewProfile_Click);
            // 
            // importToolStripMenuItem
            // 
            resources.ApplyResources(this.importToolStripMenuItem, "importToolStripMenuItem");
            this.importToolStripMenuItem.Name = "importToolStripMenuItem";
            this.importToolStripMenuItem.Click += new System.EventHandler(this.tSBImportProfile_Click);
            // 
            // exportToolStripMenuItem
            // 
            resources.ApplyResources(this.exportToolStripMenuItem, "exportToolStripMenuItem");
            this.exportToolStripMenuItem.Name = "exportToolStripMenuItem";
            this.exportToolStripMenuItem.Click += new System.EventHandler(this.tSBExportProfile_Click);
            // 
            // tSOptions
            // 
            resources.ApplyResources(this.tSOptions, "tSOptions");
            this.tSOptions.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.tSOptions.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripLabel1,
            this.tSTBProfile,
            this.tSBSaveProfile,
            this.tSBCancel,
            this.toolStripSeparator3,
            this.tSBKeepSize});
            this.tSOptions.Name = "tSOptions";
            this.tSOptions.ShowItemToolTips = false;
            this.toolTip1.SetToolTip(this.tSOptions, resources.GetString("tSOptions.ToolTip"));
            // 
            // toolStripLabel1
            // 
            resources.ApplyResources(this.toolStripLabel1, "toolStripLabel1");
            this.toolStripLabel1.Name = "toolStripLabel1";
            // 
            // tSTBProfile
            // 
            resources.ApplyResources(this.tSTBProfile, "tSTBProfile");
            this.tSTBProfile.ForeColor = System.Drawing.SystemColors.GrayText;
            this.tSTBProfile.Name = "tSTBProfile";
            this.tSTBProfile.Enter += new System.EventHandler(this.tBProfile_Enter);
            this.tSTBProfile.Leave += new System.EventHandler(this.tBProfile_Leave);
            this.tSTBProfile.TextChanged += new System.EventHandler(this.tBProfile_TextChanged);
            // 
            // tSBSaveProfile
            // 
            resources.ApplyResources(this.tSBSaveProfile, "tSBSaveProfile");
            this.tSBSaveProfile.AutoToolTip = false;
            this.tSBSaveProfile.Image = global::EAll4Windows.Properties.Resources.saveprofile;
            this.tSBSaveProfile.Name = "tSBSaveProfile";
            this.tSBSaveProfile.Click += new System.EventHandler(this.tSBSaveProfile_Click);
            // 
            // tSBCancel
            // 
            resources.ApplyResources(this.tSBCancel, "tSBCancel");
            this.tSBCancel.AutoToolTip = false;
            this.tSBCancel.Image = global::EAll4Windows.Properties.Resources.delete;
            this.tSBCancel.Name = "tSBCancel";
            this.tSBCancel.Click += new System.EventHandler(this.tSBCancel_Click);
            // 
            // toolStripSeparator3
            // 
            resources.ApplyResources(this.toolStripSeparator3, "toolStripSeparator3");
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            // 
            // tSBKeepSize
            // 
            resources.ApplyResources(this.tSBKeepSize, "tSBKeepSize");
            this.tSBKeepSize.Image = global::EAll4Windows.Properties.Resources.size;
            this.tSBKeepSize.Name = "tSBKeepSize";
            this.tSBKeepSize.Click += new System.EventHandler(this.tSBKeepSize_Click);
            // 
            // toolStrip1
            // 
            resources.ApplyResources(this.toolStrip1, "toolStrip1");
            this.toolStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsBNewProfle,
            this.tsBEditProfile,
            this.tsBDeleteProfile,
            this.tSBDupProfile,
            this.tSBImportProfile,
            this.tSBExportProfile});
            this.toolStrip1.Name = "toolStrip1";
            this.toolTip1.SetToolTip(this.toolStrip1, resources.GetString("toolStrip1.ToolTip"));
            // 
            // tsBNewProfle
            // 
            resources.ApplyResources(this.tsBNewProfle, "tsBNewProfle");
            this.tsBNewProfle.Image = global::EAll4Windows.Properties.Resources.newprofile;
            this.tsBNewProfle.Name = "tsBNewProfle";
            this.tsBNewProfle.Click += new System.EventHandler(this.tsBNewProfile_Click);
            // 
            // tsBEditProfile
            // 
            resources.ApplyResources(this.tsBEditProfile, "tsBEditProfile");
            this.tsBEditProfile.Image = global::EAll4Windows.Properties.Resources.edit;
            this.tsBEditProfile.Name = "tsBEditProfile";
            this.tsBEditProfile.Click += new System.EventHandler(this.tsBNEditProfile_Click);
            // 
            // tsBDeleteProfile
            // 
            resources.ApplyResources(this.tsBDeleteProfile, "tsBDeleteProfile");
            this.tsBDeleteProfile.Name = "tsBDeleteProfile";
            this.tsBDeleteProfile.Click += new System.EventHandler(this.tsBDeleteProfle_Click);
            // 
            // tSBDupProfile
            // 
            resources.ApplyResources(this.tSBDupProfile, "tSBDupProfile");
            this.tSBDupProfile.Name = "tSBDupProfile";
            this.tSBDupProfile.Click += new System.EventHandler(this.tSBDupProfile_Click);
            // 
            // tSBImportProfile
            // 
            resources.ApplyResources(this.tSBImportProfile, "tSBImportProfile");
            this.tSBImportProfile.Image = global::EAll4Windows.Properties.Resources.import;
            this.tSBImportProfile.Name = "tSBImportProfile";
            this.tSBImportProfile.Click += new System.EventHandler(this.tSBImportProfile_Click);
            // 
            // tSBExportProfile
            // 
            resources.ApplyResources(this.tSBExportProfile, "tSBExportProfile");
            this.tSBExportProfile.Name = "tSBExportProfile";
            this.tSBExportProfile.Click += new System.EventHandler(this.tSBExportProfile_Click);
            // 
            // tabAutoProfiles
            // 
            resources.ApplyResources(this.tabAutoProfiles, "tabAutoProfiles");
            this.tabAutoProfiles.Name = "tabAutoProfiles";
            this.toolTip1.SetToolTip(this.tabAutoProfiles, resources.GetString("tabAutoProfiles.ToolTip"));
            this.tabAutoProfiles.UseVisualStyleBackColor = true;
            // 
            // tabSettings
            // 
            resources.ApplyResources(this.tabSettings, "tabSettings");
            this.tabSettings.Controls.Add(this.fLPSettings);
            this.tabSettings.Name = "tabSettings";
            this.toolTip1.SetToolTip(this.tabSettings, resources.GetString("tabSettings.ToolTip"));
            this.tabSettings.UseVisualStyleBackColor = true;
            // 
            // fLPSettings
            // 
            resources.ApplyResources(this.fLPSettings, "fLPSettings");
            this.fLPSettings.Controls.Add(this.hideEAll4CheckBox);
            this.fLPSettings.Controls.Add(this.cBSwipeProfiles);
            this.fLPSettings.Controls.Add(this.StartWindowsCheckBox);
            this.fLPSettings.Controls.Add(this.startMinimizedCheckBox);
            this.fLPSettings.Controls.Add(this.cBNotifications);
            this.fLPSettings.Controls.Add(this.cBDisconnectBT);
            this.fLPSettings.Controls.Add(this.cBFlashWhenLate);
            this.fLPSettings.Controls.Add(this.cBCloseMini);
            this.fLPSettings.Controls.Add(this.cBQuickCharge);
            this.fLPSettings.Controls.Add(this.cBUpdate);
            this.fLPSettings.Controls.Add(this.cBDownloadLangauge);
            this.fLPSettings.Controls.Add(this.pNUpdate);
            this.fLPSettings.Controls.Add(this.pnlXIPorts);
            this.fLPSettings.Controls.Add(this.flowLayoutPanel1);
            this.fLPSettings.Name = "fLPSettings";
            this.toolTip1.SetToolTip(this.fLPSettings, resources.GetString("fLPSettings.ToolTip"));
            // 
            // hideEAll4CheckBox
            // 
            resources.ApplyResources(this.hideEAll4CheckBox, "hideEAll4CheckBox");
            this.hideEAll4CheckBox.Name = "hideEAll4CheckBox";
            this.toolTip1.SetToolTip(this.hideEAll4CheckBox, resources.GetString("hideEAll4CheckBox.ToolTip"));
            this.hideEAll4CheckBox.UseVisualStyleBackColor = true;
            this.hideEAll4CheckBox.CheckedChanged += new System.EventHandler(this.hideEAll4CheckBox_CheckedChanged);
            // 
            // cBSwipeProfiles
            // 
            resources.ApplyResources(this.cBSwipeProfiles, "cBSwipeProfiles");
            this.cBSwipeProfiles.Checked = true;
            this.cBSwipeProfiles.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cBSwipeProfiles.Name = "cBSwipeProfiles";
            this.toolTip1.SetToolTip(this.cBSwipeProfiles, resources.GetString("cBSwipeProfiles.ToolTip"));
            this.cBSwipeProfiles.UseVisualStyleBackColor = true;
            this.cBSwipeProfiles.CheckedChanged += new System.EventHandler(this.cBSwipeProfiles_CheckedChanged);
            // 
            // StartWindowsCheckBox
            // 
            resources.ApplyResources(this.StartWindowsCheckBox, "StartWindowsCheckBox");
            this.StartWindowsCheckBox.Name = "StartWindowsCheckBox";
            this.toolTip1.SetToolTip(this.StartWindowsCheckBox, resources.GetString("StartWindowsCheckBox.ToolTip"));
            this.StartWindowsCheckBox.UseVisualStyleBackColor = true;
            this.StartWindowsCheckBox.CheckedChanged += new System.EventHandler(this.StartWindowsCheckBox_CheckedChanged);
            // 
            // startMinimizedCheckBox
            // 
            resources.ApplyResources(this.startMinimizedCheckBox, "startMinimizedCheckBox");
            this.startMinimizedCheckBox.Name = "startMinimizedCheckBox";
            this.toolTip1.SetToolTip(this.startMinimizedCheckBox, resources.GetString("startMinimizedCheckBox.ToolTip"));
            this.startMinimizedCheckBox.UseVisualStyleBackColor = true;
            this.startMinimizedCheckBox.CheckedChanged += new System.EventHandler(this.startMinimizedCheckBox_CheckedChanged);
            // 
            // cBNotifications
            // 
            resources.ApplyResources(this.cBNotifications, "cBNotifications");
            this.cBNotifications.Name = "cBNotifications";
            this.toolTip1.SetToolTip(this.cBNotifications, resources.GetString("cBNotifications.ToolTip"));
            this.cBNotifications.UseVisualStyleBackColor = true;
            this.cBNotifications.CheckedChanged += new System.EventHandler(this.cBNotifications_CheckedChanged);
            // 
            // cBDisconnectBT
            // 
            resources.ApplyResources(this.cBDisconnectBT, "cBDisconnectBT");
            this.cBDisconnectBT.Name = "cBDisconnectBT";
            this.toolTip1.SetToolTip(this.cBDisconnectBT, resources.GetString("cBDisconnectBT.ToolTip"));
            this.cBDisconnectBT.UseVisualStyleBackColor = true;
            this.cBDisconnectBT.CheckedChanged += new System.EventHandler(this.cBDisconnectBT_CheckedChanged);
            // 
            // cBFlashWhenLate
            // 
            resources.ApplyResources(this.cBFlashWhenLate, "cBFlashWhenLate");
            this.cBFlashWhenLate.Checked = true;
            this.cBFlashWhenLate.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cBFlashWhenLate.Name = "cBFlashWhenLate";
            this.toolTip1.SetToolTip(this.cBFlashWhenLate, resources.GetString("cBFlashWhenLate.ToolTip"));
            this.cBFlashWhenLate.UseVisualStyleBackColor = true;
            this.cBFlashWhenLate.CheckedChanged += new System.EventHandler(this.cBFlashWhenLate_CheckedChanged);
            // 
            // cBCloseMini
            // 
            resources.ApplyResources(this.cBCloseMini, "cBCloseMini");
            this.cBCloseMini.Name = "cBCloseMini";
            this.toolTip1.SetToolTip(this.cBCloseMini, resources.GetString("cBCloseMini.ToolTip"));
            this.cBCloseMini.UseVisualStyleBackColor = true;
            this.cBCloseMini.CheckedChanged += new System.EventHandler(this.cBCloseMini_CheckedChanged);
            // 
            // cBQuickCharge
            // 
            resources.ApplyResources(this.cBQuickCharge, "cBQuickCharge");
            this.cBQuickCharge.Name = "cBQuickCharge";
            this.toolTip1.SetToolTip(this.cBQuickCharge, resources.GetString("cBQuickCharge.ToolTip"));
            this.cBQuickCharge.UseVisualStyleBackColor = true;
            this.cBQuickCharge.CheckedChanged += new System.EventHandler(this.cBQuickCharge_CheckedChanged);
            // 
            // cBUpdate
            // 
            resources.ApplyResources(this.cBUpdate, "cBUpdate");
            this.cBUpdate.Name = "cBUpdate";
            this.toolTip1.SetToolTip(this.cBUpdate, resources.GetString("cBUpdate.ToolTip"));
            this.cBUpdate.UseVisualStyleBackColor = true;
            this.cBUpdate.CheckedChanged += new System.EventHandler(this.cBUpdate_CheckedChanged);
            // 
            // cBDownloadLangauge
            // 
            resources.ApplyResources(this.cBDownloadLangauge, "cBDownloadLangauge");
            this.cBDownloadLangauge.Checked = true;
            this.cBDownloadLangauge.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cBDownloadLangauge.Name = "cBDownloadLangauge";
            this.toolTip1.SetToolTip(this.cBDownloadLangauge, resources.GetString("cBDownloadLangauge.ToolTip"));
            this.cBDownloadLangauge.UseVisualStyleBackColor = true;
            this.cBDownloadLangauge.CheckedChanged += new System.EventHandler(this.cBDownloadLangauge_CheckedChanged);
            // 
            // pNUpdate
            // 
            resources.ApplyResources(this.pNUpdate, "pNUpdate");
            this.pNUpdate.Controls.Add(this.cBUpdateTime);
            this.pNUpdate.Controls.Add(this.lbCheckEvery);
            this.pNUpdate.Controls.Add(this.nUDUpdateTime);
            this.pNUpdate.Name = "pNUpdate";
            this.toolTip1.SetToolTip(this.pNUpdate, resources.GetString("pNUpdate.ToolTip"));
            // 
            // cBUpdateTime
            // 
            resources.ApplyResources(this.cBUpdateTime, "cBUpdateTime");
            this.cBUpdateTime.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cBUpdateTime.FormattingEnabled = true;
            this.cBUpdateTime.Items.AddRange(new object[] {
            resources.GetString("cBUpdateTime.Items"),
            resources.GetString("cBUpdateTime.Items1")});
            this.cBUpdateTime.Name = "cBUpdateTime";
            this.toolTip1.SetToolTip(this.cBUpdateTime, resources.GetString("cBUpdateTime.ToolTip"));
            this.cBUpdateTime.SelectedIndexChanged += new System.EventHandler(this.cBUpdateTime_SelectedIndexChanged);
            // 
            // lbCheckEvery
            // 
            resources.ApplyResources(this.lbCheckEvery, "lbCheckEvery");
            this.lbCheckEvery.Name = "lbCheckEvery";
            this.toolTip1.SetToolTip(this.lbCheckEvery, resources.GetString("lbCheckEvery.ToolTip"));
            // 
            // nUDUpdateTime
            // 
            resources.ApplyResources(this.nUDUpdateTime, "nUDUpdateTime");
            this.nUDUpdateTime.Maximum = new decimal(new int[] {
            24,
            0,
            0,
            0});
            this.nUDUpdateTime.Name = "nUDUpdateTime";
            this.toolTip1.SetToolTip(this.nUDUpdateTime, resources.GetString("nUDUpdateTime.ToolTip"));
            this.nUDUpdateTime.ValueChanged += new System.EventHandler(this.nUDUpdateTime_ValueChanged);
            // 
            // pnlXIPorts
            // 
            resources.ApplyResources(this.pnlXIPorts, "pnlXIPorts");
            this.pnlXIPorts.Controls.Add(this.lbUseXIPorts);
            this.pnlXIPorts.Controls.Add(this.nUDXIPorts);
            this.pnlXIPorts.Controls.Add(this.lbLastXIPort);
            this.pnlXIPorts.Name = "pnlXIPorts";
            this.toolTip1.SetToolTip(this.pnlXIPorts, resources.GetString("pnlXIPorts.ToolTip"));
            this.pnlXIPorts.MouseEnter += new System.EventHandler(this.pnlXIPorts_MouseEnter);
            this.pnlXIPorts.MouseLeave += new System.EventHandler(this.pnlXIPorts_MouseLeave);
            // 
            // lbUseXIPorts
            // 
            resources.ApplyResources(this.lbUseXIPorts, "lbUseXIPorts");
            this.lbUseXIPorts.Name = "lbUseXIPorts";
            this.toolTip1.SetToolTip(this.lbUseXIPorts, resources.GetString("lbUseXIPorts.ToolTip"));
            // 
            // nUDXIPorts
            // 
            resources.ApplyResources(this.nUDXIPorts, "nUDXIPorts");
            this.nUDXIPorts.Maximum = new decimal(new int[] {
            11,
            0,
            0,
            0});
            this.nUDXIPorts.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nUDXIPorts.Name = "nUDXIPorts";
            this.toolTip1.SetToolTip(this.nUDXIPorts, resources.GetString("nUDXIPorts.ToolTip"));
            this.nUDXIPorts.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nUDXIPorts.ValueChanged += new System.EventHandler(this.nUDXIPorts_ValueChanged);
            this.nUDXIPorts.Enter += new System.EventHandler(this.nUDXIPorts_Enter);
            this.nUDXIPorts.Leave += new System.EventHandler(this.nUDXIPorts_Leave);
            // 
            // lbLastXIPort
            // 
            resources.ApplyResources(this.lbLastXIPort, "lbLastXIPort");
            this.lbLastXIPort.Name = "lbLastXIPort";
            this.toolTip1.SetToolTip(this.lbLastXIPort, resources.GetString("lbLastXIPort.ToolTip"));
            // 
            // flowLayoutPanel1
            // 
            resources.ApplyResources(this.flowLayoutPanel1, "flowLayoutPanel1");
            this.flowLayoutPanel1.Controls.Add(this.linkProfiles);
            this.flowLayoutPanel1.Controls.Add(this.lnkControllers);
            this.flowLayoutPanel1.Controls.Add(this.linkUninstall);
            this.flowLayoutPanel1.Controls.Add(this.linkSetup);
            this.flowLayoutPanel1.Controls.Add(this.lLBUpdate);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.toolTip1.SetToolTip(this.flowLayoutPanel1, resources.GetString("flowLayoutPanel1.ToolTip"));
            // 
            // linkProfiles
            // 
            resources.ApplyResources(this.linkProfiles, "linkProfiles");
            this.linkProfiles.Name = "linkProfiles";
            this.linkProfiles.TabStop = true;
            this.toolTip1.SetToolTip(this.linkProfiles, resources.GetString("linkProfiles.ToolTip"));
            this.linkProfiles.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkProfiles_LinkClicked);
            // 
            // lnkControllers
            // 
            resources.ApplyResources(this.lnkControllers, "lnkControllers");
            this.lnkControllers.Name = "lnkControllers";
            this.lnkControllers.TabStop = true;
            this.toolTip1.SetToolTip(this.lnkControllers, resources.GetString("lnkControllers.ToolTip"));
            this.lnkControllers.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkControllers_LinkClicked);
            // 
            // linkUninstall
            // 
            resources.ApplyResources(this.linkUninstall, "linkUninstall");
            this.linkUninstall.Name = "linkUninstall";
            this.linkUninstall.TabStop = true;
            this.toolTip1.SetToolTip(this.linkUninstall, resources.GetString("linkUninstall.ToolTip"));
            this.linkUninstall.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkUninstall_LinkClicked);
            // 
            // linkSetup
            // 
            resources.ApplyResources(this.linkSetup, "linkSetup");
            this.linkSetup.Name = "linkSetup";
            this.linkSetup.TabStop = true;
            this.toolTip1.SetToolTip(this.linkSetup, resources.GetString("linkSetup.ToolTip"));
            this.linkSetup.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lLSetup_LinkClicked);
            // 
            // lLBUpdate
            // 
            resources.ApplyResources(this.lLBUpdate, "lLBUpdate");
            this.lLBUpdate.Name = "lLBUpdate";
            this.lLBUpdate.TabStop = true;
            this.toolTip1.SetToolTip(this.lLBUpdate, resources.GetString("lLBUpdate.ToolTip"));
            this.lLBUpdate.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lLBUpdate_LinkClicked);
            // 
            // tabLog
            // 
            resources.ApplyResources(this.tabLog, "tabLog");
            this.tabLog.Controls.Add(this.lvDebug);
            this.tabLog.Controls.Add(this.btnClear);
            this.tabLog.Name = "tabLog";
            this.toolTip1.SetToolTip(this.tabLog, resources.GetString("tabLog.ToolTip"));
            this.tabLog.UseVisualStyleBackColor = true;
            // 
            // saveProfiles
            // 
            resources.ApplyResources(this.saveProfiles, "saveProfiles");
            // 
            // EAll4Form
            // 
            resources.ApplyResources(this, "$this");
            this.AllowDrop = true;
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.BackColor = System.Drawing.Color.White;
            this.Controls.Add(this.tabMain);
            this.Controls.Add(this.pnlButton);
            this.Name = "EAll4Form";
            this.toolTip1.SetToolTip(this, resources.GetString("$this.ToolTip"));
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ScpForm_Closing);
            this.DragDrop += new System.Windows.Forms.DragEventHandler(this.ScpForm_DragDrop);
            this.DragEnter += new System.Windows.Forms.DragEventHandler(this.ScpForm_DragEnter);
            this.Resize += new System.EventHandler(this.Form_Resize);
            this.pnlButton.ResumeLayout(false);
            this.pnlButton.PerformLayout();
            this.cMTaskbar.ResumeLayout(false);
            this.tabMain.ResumeLayout(false);
            this.tabControllers.ResumeLayout(false);
            this.tLPControllers.ResumeLayout(false);
            this.tLPControllers.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pBStatus1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pBStatus2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pBStatus3)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pBStatus4)).EndInit();
            this.tabProfiles.ResumeLayout(false);
            this.tabProfiles.PerformLayout();
            this.cMProfile.ResumeLayout(false);
            this.tSOptions.ResumeLayout(false);
            this.tSOptions.PerformLayout();
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.tabSettings.ResumeLayout(false);
            this.fLPSettings.ResumeLayout(false);
            this.fLPSettings.PerformLayout();
            this.pNUpdate.ResumeLayout(false);
            this.pNUpdate.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nUDUpdateTime)).EndInit();
            this.pnlXIPorts.ResumeLayout(false);
            this.pnlXIPorts.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nUDXIPorts)).EndInit();
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
            this.tabLog.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListView lvDebug;
        private System.Windows.Forms.ColumnHeader chTime;
        private System.Windows.Forms.ColumnHeader chData;
        private System.Windows.Forms.Timer tmrUpdate;
        private System.Windows.Forms.Panel pnlButton;
        private System.Windows.Forms.Button btnStartStop;
        private System.Windows.Forms.Button btnClear;
        private System.Windows.Forms.NotifyIcon notifyIcon1;
        private System.Windows.Forms.ContextMenuStrip cMTaskbar;
        private System.Windows.Forms.ToolStripMenuItem openToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem editProfileForController1ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem editProfileForController2ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem editProfileForController4ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem editProfileForController3ToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.OpenFileDialog openProfiles;
        private System.Windows.Forms.LinkLabel llbHelp;
        private System.Windows.Forms.Label lbTest;
        private System.Windows.Forms.TabControl tabMain;
        private System.Windows.Forms.TabPage tabProfiles;
        private System.Windows.Forms.TabPage tabLog;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton tsBNewProfle;
        private System.Windows.Forms.ToolStripButton tsBEditProfile;
        private System.Windows.Forms.ToolStripButton tsBDeleteProfile;
        private System.Windows.Forms.TabPage tabAutoProfiles;
        private System.Windows.Forms.ToolStripButton tSBDupProfile;
        private System.Windows.Forms.TabPage tabControllers;
        private System.Windows.Forms.TableLayoutPanel tLPControllers;
        private System.Windows.Forms.PictureBox pBStatus1;
        private System.Windows.Forms.Label lbPad1;
        private System.Windows.Forms.Label lbPad2;
        private System.Windows.Forms.Button bnEditC3;
        private System.Windows.Forms.Button bnEditC4;
        private System.Windows.Forms.Label lbPad3;
        private System.Windows.Forms.Label lbPad4;
        private System.Windows.Forms.ComboBox cBController1;
        private System.Windows.Forms.Button bnEditC2;
        private System.Windows.Forms.ComboBox cBController2;
        private System.Windows.Forms.ComboBox cBController3;
        private System.Windows.Forms.Button bnEditC1;
        private System.Windows.Forms.ComboBox cBController4;
        private System.Windows.Forms.Label lbSelectedProfile;
        private System.Windows.Forms.Label lbID;
        private System.Windows.Forms.Label lbStatus;
        private System.Windows.Forms.Label lbBattery;
        private System.Windows.Forms.Label lbBatt1;
        private System.Windows.Forms.Label lbBatt2;
        private System.Windows.Forms.Label lbBatt3;
        private System.Windows.Forms.Label lbBatt4;
        private System.Windows.Forms.PictureBox pBStatus2;
        private System.Windows.Forms.PictureBox pBStatus3;
        private System.Windows.Forms.PictureBox pBStatus4;
        private System.Windows.Forms.ToolStripButton tSBImportProfile;
        private System.Windows.Forms.ToolStripButton tSBExportProfile;
        private System.Windows.Forms.SaveFileDialog saveProfiles;
        private System.Windows.Forms.ContextMenuStrip cMProfile;
        private System.Windows.Forms.ToolStripMenuItem editToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem assignToController1ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem assignToController2ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem assignToController3ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem assignToController4ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem deleteToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem duplicateToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem newProfileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem importToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exportToolStripMenuItem;
        private System.Windows.Forms.ToolStrip tSOptions;
        private System.Windows.Forms.ToolStripLabel toolStripLabel1;
        private System.Windows.Forms.ToolStripTextBox tSTBProfile;
        private System.Windows.Forms.ToolStripButton tSBSaveProfile;
        private System.Windows.Forms.ToolStripButton tSBCancel;
        private System.Windows.Forms.TabPage tabSettings;
        private System.Windows.Forms.ToolStripButton tSBKeepSize;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.FlowLayoutPanel fLPSettings;
        private System.Windows.Forms.LinkLabel lLBUpdate;
        private System.Windows.Forms.LinkLabel linkSetup;
        private System.Windows.Forms.CheckBox hideEAll4CheckBox;
        private System.Windows.Forms.CheckBox cBUpdate;
        private System.Windows.Forms.Panel pNUpdate;
        private System.Windows.Forms.ComboBox cBUpdateTime;
        private System.Windows.Forms.Label lbCheckEvery;
        private System.Windows.Forms.NumericUpDown nUDUpdateTime;
        private System.Windows.Forms.LinkLabel lnkControllers;
        private System.Windows.Forms.CheckBox StartWindowsCheckBox;
        private System.Windows.Forms.CheckBox startMinimizedCheckBox;
        private System.Windows.Forms.LinkLabel linkProfiles;
        private System.Windows.Forms.LinkLabel linkUninstall;
        private System.Windows.Forms.CheckBox cBNotifications;
        private System.Windows.Forms.CheckBox cBDisconnectBT;
        private System.Windows.Forms.CheckBox cBSwipeProfiles;
        private System.Windows.Forms.ToolStripMenuItem startToolStripMenuItem;
        public System.Windows.Forms.Label lbLastMessage;
        private System.Windows.Forms.CheckBox cBQuickCharge;
        private System.Windows.Forms.Panel pnlXIPorts;
        private System.Windows.Forms.Label lbUseXIPorts;
        private System.Windows.Forms.NumericUpDown nUDXIPorts;
        private System.Windows.Forms.Label lbLastXIPort;
        public System.Windows.Forms.ListBox lBProfiles;
        private System.Windows.Forms.CheckBox cBCloseMini;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.Label lbNoControllers;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.CheckBox cBDownloadLangauge;
        private System.Windows.Forms.CheckBox cBFlashWhenLate;
        //private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem2;
    }
}

