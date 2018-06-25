namespace DS4Windows
{
    partial class DS4Form
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DS4Form));
            this.lvDebug = new System.Windows.Forms.ListView();
            this.chTime = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.chData = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.pnlButton = new System.Windows.Forms.Panel();
            this.llbHelp = new System.Windows.Forms.LinkLabel();
            this.lbTest = new System.Windows.Forms.Label();
            this.btnStartStop = new System.Windows.Forms.Button();
            this.lbLastMessage = new System.Windows.Forms.Label();
            this.notifyIcon1 = new System.Windows.Forms.NotifyIcon(this.components);
            this.cMTaskbar = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.editProfileForController1ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.editProfileForController2ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.editProfileForController3ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.editProfileForController4ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.disconToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.discon1toolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.discon2ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.discon3ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.discon4ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.startToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openProgramFolderToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.openProfiles = new System.Windows.Forms.OpenFileDialog();
            this.tabMain = new System.Windows.Forms.TabControl();
            this.tabControllers = new System.Windows.Forms.TabPage();
            this.tLPControllers = new System.Windows.Forms.TableLayoutPanel();
            this.bnLight3 = new System.Windows.Forms.Button();
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
            this.bnLight1 = new System.Windows.Forms.Button();
            this.bnLight2 = new System.Windows.Forms.Button();
            this.bnLight4 = new System.Windows.Forms.Button();
            this.lbLinkProfile = new System.Windows.Forms.Label();
            this.linkCB1 = new System.Windows.Forms.CheckBox();
            this.linkCB2 = new System.Windows.Forms.CheckBox();
            this.linkCB3 = new System.Windows.Forms.CheckBox();
            this.linkCB4 = new System.Windows.Forms.CheckBox();
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
            this.hideDS4CheckBox = new System.Windows.Forms.CheckBox();
            this.cBSwipeProfiles = new System.Windows.Forms.CheckBox();
            this.StartWindowsCheckBox = new System.Windows.Forms.CheckBox();
            this.runStartupPanel = new System.Windows.Forms.Panel();
            this.uacPictureBox = new System.Windows.Forms.PictureBox();
            this.runStartTaskRadio = new System.Windows.Forms.RadioButton();
            this.label1 = new System.Windows.Forms.Label();
            this.runStartProgRadio = new System.Windows.Forms.RadioButton();
            this.panel1 = new System.Windows.Forms.Panel();
            this.lbNotifications = new System.Windows.Forms.Label();
            this.cBoxNotifications = new System.Windows.Forms.ComboBox();
            this.cBDisconnectBT = new System.Windows.Forms.CheckBox();
            this.panel2 = new System.Windows.Forms.Panel();
            this.nUDLatency = new System.Windows.Forms.NumericUpDown();
            this.lbMsLatency = new System.Windows.Forms.Label();
            this.cBFlashWhenLate = new System.Windows.Forms.CheckBox();
            this.startMinimizedCheckBox = new System.Windows.Forms.CheckBox();
            this.mintoTaskCheckBox = new System.Windows.Forms.CheckBox();
            this.cBCloseMini = new System.Windows.Forms.CheckBox();
            this.cBQuickCharge = new System.Windows.Forms.CheckBox();
            this.cBUseWhiteIcon = new System.Windows.Forms.CheckBox();
            this.cBUpdate = new System.Windows.Forms.CheckBox();
            this.pNUpdate = new System.Windows.Forms.Panel();
            this.cBUpdateTime = new System.Windows.Forms.ComboBox();
            this.lbCheckEvery = new System.Windows.Forms.Label();
            this.nUDUpdateTime = new System.Windows.Forms.NumericUpDown();
            this.pnlXIPorts = new System.Windows.Forms.Panel();
            this.lbUseXIPorts = new System.Windows.Forms.Label();
            this.nUDXIPorts = new System.Windows.Forms.NumericUpDown();
            this.lbLastXIPort = new System.Windows.Forms.Label();
            this.languagePackComboBox1 = new DS4Windows.DS4Forms.LanguagePackComboBox();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.linkProfiles = new System.Windows.Forms.LinkLabel();
            this.lnkControllers = new System.Windows.Forms.LinkLabel();
            this.linkUninstall = new System.Windows.Forms.LinkLabel();
            this.linkSetup = new System.Windows.Forms.LinkLabel();
            this.lLBUpdate = new System.Windows.Forms.LinkLabel();
            this.linkSplitLabel = new System.Windows.Forms.Label();
            this.hidGuardWhiteList = new System.Windows.Forms.LinkLabel();
            this.clrHidGuardWlistLinkLabel = new System.Windows.Forms.LinkLabel();
            this.hidGuardRegLinkLabel = new System.Windows.Forms.LinkLabel();
            this.tabLog = new System.Windows.Forms.TabPage();
            this.panel3 = new System.Windows.Forms.Panel();
            this.exportLogTxtBtn = new System.Windows.Forms.Button();
            this.btnClear = new System.Windows.Forms.Button();
            this.saveProfiles = new System.Windows.Forms.SaveFileDialog();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.cMCustomLed = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.useProfileColorToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.useCustomColorToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.advColorDialog = new DS4Windows.AdvancedColorDialog();
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
            this.runStartupPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.uacPictureBox)).BeginInit();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nUDLatency)).BeginInit();
            this.pNUpdate.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nUDUpdateTime)).BeginInit();
            this.pnlXIPorts.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nUDXIPorts)).BeginInit();
            this.flowLayoutPanel1.SuspendLayout();
            this.tabLog.SuspendLayout();
            this.panel3.SuspendLayout();
            this.cMCustomLed.SuspendLayout();
            this.SuspendLayout();
            // 
            // lvDebug
            // 
            this.lvDebug.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.chTime,
            this.chData});
            resources.ApplyResources(this.lvDebug, "lvDebug");
            this.lvDebug.FullRowSelect = true;
            this.lvDebug.Name = "lvDebug";
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
            // pnlButton
            // 
            this.pnlButton.BackColor = System.Drawing.SystemColors.Control;
            this.pnlButton.Controls.Add(this.llbHelp);
            this.pnlButton.Controls.Add(this.lbTest);
            this.pnlButton.Controls.Add(this.btnStartStop);
            this.pnlButton.Controls.Add(this.lbLastMessage);
            resources.ApplyResources(this.pnlButton, "pnlButton");
            this.pnlButton.Name = "pnlButton";
            this.pnlButton.MouseLeave += new System.EventHandler(this.pnlButton_MouseLeave);
            // 
            // llbHelp
            // 
            resources.ApplyResources(this.llbHelp, "llbHelp");
            this.llbHelp.Name = "llbHelp";
            this.llbHelp.TabStop = true;
            this.llbHelp.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.llbHelp_LinkClicked);
            // 
            // lbTest
            // 
            resources.ApplyResources(this.lbTest, "lbTest");
            this.lbTest.Name = "lbTest";
            // 
            // btnStartStop
            // 
            resources.ApplyResources(this.btnStartStop, "btnStartStop");
            this.btnStartStop.Name = "btnStartStop";
            this.btnStartStop.UseVisualStyleBackColor = true;
            this.btnStartStop.Click += new System.EventHandler(this.BtnStartStop_Click);
            // 
            // lbLastMessage
            // 
            resources.ApplyResources(this.lbLastMessage, "lbLastMessage");
            this.lbLastMessage.ForeColor = System.Drawing.SystemColors.GrayText;
            this.lbLastMessage.Name = "lbLastMessage";
            this.lbLastMessage.MouseHover += new System.EventHandler(this.lbLastMessage_MouseHover);
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
            this.cMTaskbar.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.cMTaskbar.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.editProfileForController1ToolStripMenuItem,
            this.editProfileForController2ToolStripMenuItem,
            this.editProfileForController3ToolStripMenuItem,
            this.editProfileForController4ToolStripMenuItem,
            this.toolStripSeparator4,
            this.disconToolStripMenuItem,
            this.toolStripSeparator1,
            this.startToolStripMenuItem,
            this.openToolStripMenuItem,
            this.openProgramFolderToolStripMenuItem,
            this.exitToolStripMenuItem});
            this.cMTaskbar.Name = "cMTaskbar";
            resources.ApplyResources(this.cMTaskbar, "cMTaskbar");
            this.cMTaskbar.Tag = "25";
            // 
            // editProfileForController1ToolStripMenuItem
            // 
            this.editProfileForController1ToolStripMenuItem.Name = "editProfileForController1ToolStripMenuItem";
            resources.ApplyResources(this.editProfileForController1ToolStripMenuItem, "editProfileForController1ToolStripMenuItem");
            this.editProfileForController1ToolStripMenuItem.Tag = "0";
            this.editProfileForController1ToolStripMenuItem.Click += new System.EventHandler(this.editMenu_Click);
            // 
            // editProfileForController2ToolStripMenuItem
            // 
            this.editProfileForController2ToolStripMenuItem.Name = "editProfileForController2ToolStripMenuItem";
            resources.ApplyResources(this.editProfileForController2ToolStripMenuItem, "editProfileForController2ToolStripMenuItem");
            this.editProfileForController2ToolStripMenuItem.Tag = "1";
            this.editProfileForController2ToolStripMenuItem.Click += new System.EventHandler(this.editMenu_Click);
            // 
            // editProfileForController3ToolStripMenuItem
            // 
            this.editProfileForController3ToolStripMenuItem.Name = "editProfileForController3ToolStripMenuItem";
            resources.ApplyResources(this.editProfileForController3ToolStripMenuItem, "editProfileForController3ToolStripMenuItem");
            this.editProfileForController3ToolStripMenuItem.Tag = "2";
            this.editProfileForController3ToolStripMenuItem.Click += new System.EventHandler(this.editMenu_Click);
            // 
            // editProfileForController4ToolStripMenuItem
            // 
            this.editProfileForController4ToolStripMenuItem.Name = "editProfileForController4ToolStripMenuItem";
            resources.ApplyResources(this.editProfileForController4ToolStripMenuItem, "editProfileForController4ToolStripMenuItem");
            this.editProfileForController4ToolStripMenuItem.Tag = "4";
            this.editProfileForController4ToolStripMenuItem.Click += new System.EventHandler(this.editMenu_Click);
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            resources.ApplyResources(this.toolStripSeparator4, "toolStripSeparator4");
            // 
            // disconToolStripMenuItem
            // 
            this.disconToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.discon1toolStripMenuItem,
            this.discon2ToolStripMenuItem,
            this.discon3ToolStripMenuItem,
            this.discon4ToolStripMenuItem});
            this.disconToolStripMenuItem.Name = "disconToolStripMenuItem";
            resources.ApplyResources(this.disconToolStripMenuItem, "disconToolStripMenuItem");
            // 
            // discon1toolStripMenuItem
            // 
            this.discon1toolStripMenuItem.Name = "discon1toolStripMenuItem";
            resources.ApplyResources(this.discon1toolStripMenuItem, "discon1toolStripMenuItem");
            this.discon1toolStripMenuItem.Tag = "0";
            this.discon1toolStripMenuItem.Click += new System.EventHandler(this.DiscontoolStripMenuItem_Click);
            // 
            // discon2ToolStripMenuItem
            // 
            this.discon2ToolStripMenuItem.Name = "discon2ToolStripMenuItem";
            resources.ApplyResources(this.discon2ToolStripMenuItem, "discon2ToolStripMenuItem");
            this.discon2ToolStripMenuItem.Tag = "1";
            this.discon2ToolStripMenuItem.Click += new System.EventHandler(this.DiscontoolStripMenuItem_Click);
            // 
            // discon3ToolStripMenuItem
            // 
            this.discon3ToolStripMenuItem.Name = "discon3ToolStripMenuItem";
            resources.ApplyResources(this.discon3ToolStripMenuItem, "discon3ToolStripMenuItem");
            this.discon3ToolStripMenuItem.Tag = "2";
            this.discon3ToolStripMenuItem.Click += new System.EventHandler(this.DiscontoolStripMenuItem_Click);
            // 
            // discon4ToolStripMenuItem
            // 
            this.discon4ToolStripMenuItem.Name = "discon4ToolStripMenuItem";
            resources.ApplyResources(this.discon4ToolStripMenuItem, "discon4ToolStripMenuItem");
            this.discon4ToolStripMenuItem.Tag = "3";
            this.discon4ToolStripMenuItem.Click += new System.EventHandler(this.DiscontoolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            resources.ApplyResources(this.toolStripSeparator1, "toolStripSeparator1");
            // 
            // startToolStripMenuItem
            // 
            this.startToolStripMenuItem.Name = "startToolStripMenuItem";
            resources.ApplyResources(this.startToolStripMenuItem, "startToolStripMenuItem");
            this.startToolStripMenuItem.Click += new System.EventHandler(this.startToolStripMenuItem_Click);
            // 
            // openToolStripMenuItem
            // 
            resources.ApplyResources(this.openToolStripMenuItem, "openToolStripMenuItem");
            this.openToolStripMenuItem.Name = "openToolStripMenuItem";
            this.openToolStripMenuItem.Click += new System.EventHandler(this.openToolStripMenuItem_Click);
            // 
            // openProgramFolderToolStripMenuItem
            // 
            this.openProgramFolderToolStripMenuItem.Name = "openProgramFolderToolStripMenuItem";
            resources.ApplyResources(this.openProgramFolderToolStripMenuItem, "openProgramFolderToolStripMenuItem");
            this.openProgramFolderToolStripMenuItem.Click += new System.EventHandler(this.OpenProgramFolderToolStripMenuItem_Click);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            resources.ApplyResources(this.exitToolStripMenuItem, "exitToolStripMenuItem");
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            resources.ApplyResources(this.toolStripSeparator2, "toolStripSeparator2");
            // 
            // openProfiles
            // 
            resources.ApplyResources(this.openProfiles, "openProfiles");
            this.openProfiles.Multiselect = true;
            // 
            // tabMain
            // 
            this.tabMain.Controls.Add(this.tabControllers);
            this.tabMain.Controls.Add(this.tabProfiles);
            this.tabMain.Controls.Add(this.tabAutoProfiles);
            this.tabMain.Controls.Add(this.tabSettings);
            this.tabMain.Controls.Add(this.tabLog);
            resources.ApplyResources(this.tabMain, "tabMain");
            this.tabMain.Name = "tabMain";
            this.tabMain.SelectedIndex = 0;
            this.tabMain.SelectedIndexChanged += new System.EventHandler(this.tabMain_SelectedIndexChanged);
            // 
            // tabControllers
            // 
            this.tabControllers.Controls.Add(this.tLPControllers);
            this.tabControllers.Controls.Add(this.lbNoControllers);
            resources.ApplyResources(this.tabControllers, "tabControllers");
            this.tabControllers.Name = "tabControllers";
            this.tabControllers.UseVisualStyleBackColor = true;
            // 
            // tLPControllers
            // 
            resources.ApplyResources(this.tLPControllers, "tLPControllers");
            this.tLPControllers.Controls.Add(this.bnLight3, 6, 3);
            this.tLPControllers.Controls.Add(this.pBStatus1, 1, 1);
            this.tLPControllers.Controls.Add(this.lbPad1, 0, 1);
            this.tLPControllers.Controls.Add(this.lbPad2, 0, 2);
            this.tLPControllers.Controls.Add(this.bnEditC3, 5, 3);
            this.tLPControllers.Controls.Add(this.bnEditC4, 5, 4);
            this.tLPControllers.Controls.Add(this.lbPad3, 0, 3);
            this.tLPControllers.Controls.Add(this.lbPad4, 0, 4);
            this.tLPControllers.Controls.Add(this.cBController1, 4, 1);
            this.tLPControllers.Controls.Add(this.bnEditC2, 5, 2);
            this.tLPControllers.Controls.Add(this.cBController2, 4, 2);
            this.tLPControllers.Controls.Add(this.cBController3, 4, 3);
            this.tLPControllers.Controls.Add(this.bnEditC1, 5, 1);
            this.tLPControllers.Controls.Add(this.cBController4, 4, 4);
            this.tLPControllers.Controls.Add(this.lbSelectedProfile, 4, 0);
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
            this.tLPControllers.Controls.Add(this.bnLight1, 6, 1);
            this.tLPControllers.Controls.Add(this.bnLight2, 6, 2);
            this.tLPControllers.Controls.Add(this.bnLight4, 6, 4);
            this.tLPControllers.Controls.Add(this.lbLinkProfile, 3, 0);
            this.tLPControllers.Controls.Add(this.linkCB1, 3, 1);
            this.tLPControllers.Controls.Add(this.linkCB2, 3, 2);
            this.tLPControllers.Controls.Add(this.linkCB3, 3, 3);
            this.tLPControllers.Controls.Add(this.linkCB4, 3, 4);
            this.tLPControllers.Name = "tLPControllers";
            // 
            // bnLight3
            // 
            this.bnLight3.BackColor = System.Drawing.Color.White;
            resources.ApplyResources(this.bnLight3, "bnLight3");
            this.bnLight3.Name = "bnLight3";
            this.bnLight3.Tag = "2";
            this.bnLight3.UseVisualStyleBackColor = false;
            this.bnLight3.Click += new System.EventHandler(this.EditCustomLed);
            // 
            // pBStatus1
            // 
            resources.ApplyResources(this.pBStatus1, "pBStatus1");
            this.pBStatus1.Image = global::DS4Windows.Properties.Resources.none;
            this.pBStatus1.InitialImage = global::DS4Windows.Properties.Resources.BT;
            this.pBStatus1.Name = "pBStatus1";
            this.pBStatus1.TabStop = false;
            this.pBStatus1.Tag = "0";
            this.pBStatus1.MouseClick += new System.Windows.Forms.MouseEventHandler(this.pBStatus_MouseClick);
            // 
            // lbPad1
            // 
            resources.ApplyResources(this.lbPad1, "lbPad1");
            this.lbPad1.Name = "lbPad1";
            this.lbPad1.Tag = "0";
            this.lbPad1.MouseLeave += new System.EventHandler(this.Pads_MouseLeave);
            this.lbPad1.MouseHover += new System.EventHandler(this.Pads_MouseHover);
            // 
            // lbPad2
            // 
            resources.ApplyResources(this.lbPad2, "lbPad2");
            this.lbPad2.Name = "lbPad2";
            this.lbPad2.Tag = "1";
            this.lbPad2.MouseLeave += new System.EventHandler(this.Pads_MouseLeave);
            this.lbPad2.MouseHover += new System.EventHandler(this.Pads_MouseHover);
            // 
            // bnEditC3
            // 
            resources.ApplyResources(this.bnEditC3, "bnEditC3");
            this.bnEditC3.Name = "bnEditC3";
            this.bnEditC3.Tag = "";
            this.bnEditC3.UseVisualStyleBackColor = true;
            this.bnEditC3.Click += new System.EventHandler(this.editButtons_Click);
            // 
            // bnEditC4
            // 
            resources.ApplyResources(this.bnEditC4, "bnEditC4");
            this.bnEditC4.Name = "bnEditC4";
            this.bnEditC4.Tag = "";
            this.bnEditC4.UseVisualStyleBackColor = true;
            this.bnEditC4.Click += new System.EventHandler(this.editButtons_Click);
            // 
            // lbPad3
            // 
            resources.ApplyResources(this.lbPad3, "lbPad3");
            this.lbPad3.Name = "lbPad3";
            this.lbPad3.Tag = "2";
            this.lbPad3.MouseLeave += new System.EventHandler(this.Pads_MouseLeave);
            this.lbPad3.MouseHover += new System.EventHandler(this.Pads_MouseHover);
            // 
            // lbPad4
            // 
            resources.ApplyResources(this.lbPad4, "lbPad4");
            this.lbPad4.Name = "lbPad4";
            this.lbPad4.Tag = "3";
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
            this.cBController1.SelectedValueChanged += new System.EventHandler(this.Profile_Changed);
            // 
            // bnEditC2
            // 
            resources.ApplyResources(this.bnEditC2, "bnEditC2");
            this.bnEditC2.Name = "bnEditC2";
            this.bnEditC2.Tag = "";
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
            this.cBController2.SelectedValueChanged += new System.EventHandler(this.Profile_Changed);
            // 
            // cBController3
            // 
            resources.ApplyResources(this.cBController3, "cBController3");
            this.cBController3.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cBController3.FormattingEnabled = true;
            this.cBController3.Name = "cBController3";
            this.cBController3.Tag = "2";
            this.cBController3.SelectedValueChanged += new System.EventHandler(this.Profile_Changed);
            // 
            // bnEditC1
            // 
            resources.ApplyResources(this.bnEditC1, "bnEditC1");
            this.bnEditC1.Name = "bnEditC1";
            this.bnEditC1.Tag = "";
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
            this.cBController4.SelectedValueChanged += new System.EventHandler(this.Profile_Changed);
            // 
            // lbSelectedProfile
            // 
            resources.ApplyResources(this.lbSelectedProfile, "lbSelectedProfile");
            this.lbSelectedProfile.Name = "lbSelectedProfile";
            // 
            // lbID
            // 
            resources.ApplyResources(this.lbID, "lbID");
            this.lbID.Name = "lbID";
            // 
            // lbStatus
            // 
            resources.ApplyResources(this.lbStatus, "lbStatus");
            this.lbStatus.Name = "lbStatus";
            // 
            // lbBattery
            // 
            resources.ApplyResources(this.lbBattery, "lbBattery");
            this.lbBattery.Name = "lbBattery";
            // 
            // lbBatt1
            // 
            resources.ApplyResources(this.lbBatt1, "lbBatt1");
            this.lbBatt1.Name = "lbBatt1";
            // 
            // lbBatt2
            // 
            resources.ApplyResources(this.lbBatt2, "lbBatt2");
            this.lbBatt2.Name = "lbBatt2";
            // 
            // lbBatt3
            // 
            resources.ApplyResources(this.lbBatt3, "lbBatt3");
            this.lbBatt3.Name = "lbBatt3";
            // 
            // lbBatt4
            // 
            resources.ApplyResources(this.lbBatt4, "lbBatt4");
            this.lbBatt4.Name = "lbBatt4";
            // 
            // pBStatus2
            // 
            resources.ApplyResources(this.pBStatus2, "pBStatus2");
            this.pBStatus2.Image = global::DS4Windows.Properties.Resources.none;
            this.pBStatus2.InitialImage = global::DS4Windows.Properties.Resources.BT;
            this.pBStatus2.Name = "pBStatus2";
            this.pBStatus2.TabStop = false;
            this.pBStatus2.Tag = "1";
            this.pBStatus2.MouseClick += new System.Windows.Forms.MouseEventHandler(this.pBStatus_MouseClick);
            // 
            // pBStatus3
            // 
            resources.ApplyResources(this.pBStatus3, "pBStatus3");
            this.pBStatus3.Image = global::DS4Windows.Properties.Resources.none;
            this.pBStatus3.InitialImage = global::DS4Windows.Properties.Resources.BT;
            this.pBStatus3.Name = "pBStatus3";
            this.pBStatus3.TabStop = false;
            this.pBStatus3.Tag = "2";
            this.pBStatus3.MouseClick += new System.Windows.Forms.MouseEventHandler(this.pBStatus_MouseClick);
            // 
            // pBStatus4
            // 
            resources.ApplyResources(this.pBStatus4, "pBStatus4");
            this.pBStatus4.Image = global::DS4Windows.Properties.Resources.none;
            this.pBStatus4.InitialImage = global::DS4Windows.Properties.Resources.BT;
            this.pBStatus4.Name = "pBStatus4";
            this.pBStatus4.TabStop = false;
            this.pBStatus4.Tag = "3";
            this.pBStatus4.MouseClick += new System.Windows.Forms.MouseEventHandler(this.pBStatus_MouseClick);
            // 
            // bnLight1
            // 
            this.bnLight1.BackColor = System.Drawing.Color.White;
            resources.ApplyResources(this.bnLight1, "bnLight1");
            this.bnLight1.Name = "bnLight1";
            this.bnLight1.Tag = "0";
            this.bnLight1.UseVisualStyleBackColor = false;
            this.bnLight1.Click += new System.EventHandler(this.EditCustomLed);
            // 
            // bnLight2
            // 
            this.bnLight2.BackColor = System.Drawing.Color.White;
            resources.ApplyResources(this.bnLight2, "bnLight2");
            this.bnLight2.Name = "bnLight2";
            this.bnLight2.Tag = "1";
            this.bnLight2.UseVisualStyleBackColor = false;
            this.bnLight2.Click += new System.EventHandler(this.EditCustomLed);
            // 
            // bnLight4
            // 
            this.bnLight4.BackColor = System.Drawing.Color.White;
            resources.ApplyResources(this.bnLight4, "bnLight4");
            this.bnLight4.Name = "bnLight4";
            this.bnLight4.Tag = "3";
            this.bnLight4.UseVisualStyleBackColor = false;
            this.bnLight4.Click += new System.EventHandler(this.EditCustomLed);
            // 
            // lbLinkProfile
            // 
            resources.ApplyResources(this.lbLinkProfile, "lbLinkProfile");
            this.lbLinkProfile.Name = "lbLinkProfile";
            // 
            // linkCB1
            // 
            resources.ApplyResources(this.linkCB1, "linkCB1");
            this.linkCB1.Name = "linkCB1";
            this.linkCB1.Tag = "0";
            this.linkCB1.UseVisualStyleBackColor = true;
            this.linkCB1.CheckedChanged += new System.EventHandler(this.linkCB_CheckedChanged);
            // 
            // linkCB2
            // 
            resources.ApplyResources(this.linkCB2, "linkCB2");
            this.linkCB2.Name = "linkCB2";
            this.linkCB2.Tag = "1";
            this.linkCB2.UseVisualStyleBackColor = true;
            this.linkCB2.CheckedChanged += new System.EventHandler(this.linkCB_CheckedChanged);
            // 
            // linkCB3
            // 
            resources.ApplyResources(this.linkCB3, "linkCB3");
            this.linkCB3.Name = "linkCB3";
            this.linkCB3.Tag = "2";
            this.linkCB3.UseVisualStyleBackColor = true;
            this.linkCB3.CheckedChanged += new System.EventHandler(this.linkCB_CheckedChanged);
            // 
            // linkCB4
            // 
            resources.ApplyResources(this.linkCB4, "linkCB4");
            this.linkCB4.Name = "linkCB4";
            this.linkCB4.Tag = "3";
            this.linkCB4.UseVisualStyleBackColor = true;
            this.linkCB4.CheckedChanged += new System.EventHandler(this.linkCB_CheckedChanged);
            // 
            // lbNoControllers
            // 
            resources.ApplyResources(this.lbNoControllers, "lbNoControllers");
            this.lbNoControllers.Name = "lbNoControllers";
            // 
            // tabProfiles
            // 
            this.tabProfiles.Controls.Add(this.lBProfiles);
            this.tabProfiles.Controls.Add(this.tSOptions);
            this.tabProfiles.Controls.Add(this.toolStrip1);
            resources.ApplyResources(this.tabProfiles, "tabProfiles");
            this.tabProfiles.Name = "tabProfiles";
            this.tabProfiles.UseVisualStyleBackColor = true;
            // 
            // lBProfiles
            // 
            this.lBProfiles.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.lBProfiles.ContextMenuStrip = this.cMProfile;
            resources.ApplyResources(this.lBProfiles, "lBProfiles");
            this.lBProfiles.FormattingEnabled = true;
            this.lBProfiles.Name = "lBProfiles";
            this.lBProfiles.SelectedIndexChanged += new System.EventHandler(this.lBProfiles_SelectedIndexChanged);
            this.lBProfiles.KeyDown += new System.Windows.Forms.KeyEventHandler(this.lBProfiles_KeyDown);
            this.lBProfiles.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.lBProfiles_MouseDoubleClick);
            this.lBProfiles.MouseDown += new System.Windows.Forms.MouseEventHandler(this.lBProfiles_MouseDown);
            // 
            // cMProfile
            // 
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
            resources.ApplyResources(this.cMProfile, "cMProfile");
            // 
            // editToolStripMenuItem
            // 
            resources.ApplyResources(this.editToolStripMenuItem, "editToolStripMenuItem");
            this.editToolStripMenuItem.Name = "editToolStripMenuItem";
            this.editToolStripMenuItem.Click += new System.EventHandler(this.tsBNEditProfile_Click);
            // 
            // assignToController1ToolStripMenuItem
            // 
            this.assignToController1ToolStripMenuItem.Name = "assignToController1ToolStripMenuItem";
            resources.ApplyResources(this.assignToController1ToolStripMenuItem, "assignToController1ToolStripMenuItem");
            this.assignToController1ToolStripMenuItem.Click += new System.EventHandler(this.assignToController1ToolStripMenuItem_Click);
            // 
            // assignToController2ToolStripMenuItem
            // 
            this.assignToController2ToolStripMenuItem.Name = "assignToController2ToolStripMenuItem";
            resources.ApplyResources(this.assignToController2ToolStripMenuItem, "assignToController2ToolStripMenuItem");
            this.assignToController2ToolStripMenuItem.Click += new System.EventHandler(this.assignToController2ToolStripMenuItem_Click);
            // 
            // assignToController3ToolStripMenuItem
            // 
            this.assignToController3ToolStripMenuItem.Name = "assignToController3ToolStripMenuItem";
            resources.ApplyResources(this.assignToController3ToolStripMenuItem, "assignToController3ToolStripMenuItem");
            this.assignToController3ToolStripMenuItem.Click += new System.EventHandler(this.assignToController3ToolStripMenuItem_Click);
            // 
            // assignToController4ToolStripMenuItem
            // 
            this.assignToController4ToolStripMenuItem.Name = "assignToController4ToolStripMenuItem";
            resources.ApplyResources(this.assignToController4ToolStripMenuItem, "assignToController4ToolStripMenuItem");
            this.assignToController4ToolStripMenuItem.Click += new System.EventHandler(this.assignToController4ToolStripMenuItem_Click);
            // 
            // deleteToolStripMenuItem
            // 
            this.deleteToolStripMenuItem.Name = "deleteToolStripMenuItem";
            resources.ApplyResources(this.deleteToolStripMenuItem, "deleteToolStripMenuItem");
            this.deleteToolStripMenuItem.Click += new System.EventHandler(this.tsBDeleteProfle_Click);
            // 
            // duplicateToolStripMenuItem
            // 
            this.duplicateToolStripMenuItem.Name = "duplicateToolStripMenuItem";
            resources.ApplyResources(this.duplicateToolStripMenuItem, "duplicateToolStripMenuItem");
            this.duplicateToolStripMenuItem.Click += new System.EventHandler(this.tSBDupProfile_Click);
            // 
            // newProfileToolStripMenuItem
            // 
            this.newProfileToolStripMenuItem.Name = "newProfileToolStripMenuItem";
            resources.ApplyResources(this.newProfileToolStripMenuItem, "newProfileToolStripMenuItem");
            this.newProfileToolStripMenuItem.Click += new System.EventHandler(this.tsBNewProfile_Click);
            // 
            // importToolStripMenuItem
            // 
            this.importToolStripMenuItem.Name = "importToolStripMenuItem";
            resources.ApplyResources(this.importToolStripMenuItem, "importToolStripMenuItem");
            this.importToolStripMenuItem.Click += new System.EventHandler(this.tSBImportProfile_Click);
            // 
            // exportToolStripMenuItem
            // 
            this.exportToolStripMenuItem.Name = "exportToolStripMenuItem";
            resources.ApplyResources(this.exportToolStripMenuItem, "exportToolStripMenuItem");
            this.exportToolStripMenuItem.Click += new System.EventHandler(this.tSBExportProfile_Click);
            // 
            // tSOptions
            // 
            this.tSOptions.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.tSOptions.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.tSOptions.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripLabel1,
            this.tSTBProfile,
            this.tSBSaveProfile,
            this.tSBCancel,
            this.toolStripSeparator3,
            this.tSBKeepSize});
            resources.ApplyResources(this.tSOptions, "tSOptions");
            this.tSOptions.Name = "tSOptions";
            this.tSOptions.ShowItemToolTips = false;
            // 
            // toolStripLabel1
            // 
            this.toolStripLabel1.Name = "toolStripLabel1";
            resources.ApplyResources(this.toolStripLabel1, "toolStripLabel1");
            // 
            // tSTBProfile
            // 
            this.tSTBProfile.ForeColor = System.Drawing.SystemColors.GrayText;
            this.tSTBProfile.Name = "tSTBProfile";
            resources.ApplyResources(this.tSTBProfile, "tSTBProfile");
            this.tSTBProfile.Enter += new System.EventHandler(this.tBProfile_Enter);
            this.tSTBProfile.Leave += new System.EventHandler(this.tBProfile_Leave);
            this.tSTBProfile.TextChanged += new System.EventHandler(this.tBProfile_TextChanged);
            // 
            // tSBSaveProfile
            // 
            this.tSBSaveProfile.AutoToolTip = false;
            this.tSBSaveProfile.Image = global::DS4Windows.Properties.Resources.saveprofile;
            resources.ApplyResources(this.tSBSaveProfile, "tSBSaveProfile");
            this.tSBSaveProfile.Name = "tSBSaveProfile";
            this.tSBSaveProfile.Click += new System.EventHandler(this.tSBSaveProfile_Click);
            // 
            // tSBCancel
            // 
            this.tSBCancel.AutoToolTip = false;
            this.tSBCancel.Image = global::DS4Windows.Properties.Resources.delete;
            resources.ApplyResources(this.tSBCancel, "tSBCancel");
            this.tSBCancel.Name = "tSBCancel";
            this.tSBCancel.Click += new System.EventHandler(this.tSBCancel_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            resources.ApplyResources(this.toolStripSeparator3, "toolStripSeparator3");
            // 
            // tSBKeepSize
            // 
            this.tSBKeepSize.Image = global::DS4Windows.Properties.Resources.size;
            resources.ApplyResources(this.tSBKeepSize, "tSBKeepSize");
            this.tSBKeepSize.Name = "tSBKeepSize";
            this.tSBKeepSize.Click += new System.EventHandler(this.tSBKeepSize_Click);
            // 
            // toolStrip1
            // 
            this.toolStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsBNewProfle,
            this.tsBEditProfile,
            this.tsBDeleteProfile,
            this.tSBDupProfile,
            this.tSBImportProfile,
            this.tSBExportProfile});
            resources.ApplyResources(this.toolStrip1, "toolStrip1");
            this.toolStrip1.Name = "toolStrip1";
            // 
            // tsBNewProfle
            // 
            this.tsBNewProfle.Image = global::DS4Windows.Properties.Resources.newprofile;
            resources.ApplyResources(this.tsBNewProfle, "tsBNewProfle");
            this.tsBNewProfle.Name = "tsBNewProfle";
            this.tsBNewProfle.Click += new System.EventHandler(this.tsBNewProfile_Click);
            // 
            // tsBEditProfile
            // 
            resources.ApplyResources(this.tsBEditProfile, "tsBEditProfile");
            this.tsBEditProfile.Image = global::DS4Windows.Properties.Resources.edit;
            this.tsBEditProfile.Name = "tsBEditProfile";
            this.tsBEditProfile.Click += new System.EventHandler(this.tsBNEditProfile_Click);
            // 
            // tsBDeleteProfile
            // 
            resources.ApplyResources(this.tsBDeleteProfile, "tsBDeleteProfile");
            this.tsBDeleteProfile.Image = global::DS4Windows.Properties.Resources.delete;
            this.tsBDeleteProfile.Name = "tsBDeleteProfile";
            this.tsBDeleteProfile.Click += new System.EventHandler(this.tsBDeleteProfle_Click);
            // 
            // tSBDupProfile
            // 
            resources.ApplyResources(this.tSBDupProfile, "tSBDupProfile");
            this.tSBDupProfile.Image = global::DS4Windows.Properties.Resources.copy;
            this.tSBDupProfile.Name = "tSBDupProfile";
            this.tSBDupProfile.Click += new System.EventHandler(this.tSBDupProfile_Click);
            // 
            // tSBImportProfile
            // 
            this.tSBImportProfile.Image = global::DS4Windows.Properties.Resources.import;
            resources.ApplyResources(this.tSBImportProfile, "tSBImportProfile");
            this.tSBImportProfile.Name = "tSBImportProfile";
            this.tSBImportProfile.Click += new System.EventHandler(this.tSBImportProfile_Click);
            // 
            // tSBExportProfile
            // 
            resources.ApplyResources(this.tSBExportProfile, "tSBExportProfile");
            this.tSBExportProfile.Image = global::DS4Windows.Properties.Resources.export;
            this.tSBExportProfile.Name = "tSBExportProfile";
            this.tSBExportProfile.Click += new System.EventHandler(this.tSBExportProfile_Click);
            // 
            // tabAutoProfiles
            // 
            resources.ApplyResources(this.tabAutoProfiles, "tabAutoProfiles");
            this.tabAutoProfiles.Name = "tabAutoProfiles";
            this.tabAutoProfiles.UseVisualStyleBackColor = true;
            // 
            // tabSettings
            // 
            this.tabSettings.Controls.Add(this.fLPSettings);
            resources.ApplyResources(this.tabSettings, "tabSettings");
            this.tabSettings.Name = "tabSettings";
            this.tabSettings.UseVisualStyleBackColor = true;
            // 
            // fLPSettings
            // 
            resources.ApplyResources(this.fLPSettings, "fLPSettings");
            this.fLPSettings.Controls.Add(this.hideDS4CheckBox);
            this.fLPSettings.Controls.Add(this.cBSwipeProfiles);
            this.fLPSettings.Controls.Add(this.StartWindowsCheckBox);
            this.fLPSettings.Controls.Add(this.runStartupPanel);
            this.fLPSettings.Controls.Add(this.panel1);
            this.fLPSettings.Controls.Add(this.cBDisconnectBT);
            this.fLPSettings.Controls.Add(this.panel2);
            this.fLPSettings.Controls.Add(this.startMinimizedCheckBox);
            this.fLPSettings.Controls.Add(this.mintoTaskCheckBox);
            this.fLPSettings.Controls.Add(this.cBCloseMini);
            this.fLPSettings.Controls.Add(this.cBQuickCharge);
            this.fLPSettings.Controls.Add(this.cBUseWhiteIcon);
            this.fLPSettings.Controls.Add(this.cBUpdate);
            this.fLPSettings.Controls.Add(this.pNUpdate);
            this.fLPSettings.Controls.Add(this.pnlXIPorts);
            this.fLPSettings.Controls.Add(this.languagePackComboBox1);
            this.fLPSettings.Controls.Add(this.flowLayoutPanel1);
            this.fLPSettings.Name = "fLPSettings";
            // 
            // hideDS4CheckBox
            // 
            resources.ApplyResources(this.hideDS4CheckBox, "hideDS4CheckBox");
            this.hideDS4CheckBox.Name = "hideDS4CheckBox";
            this.hideDS4CheckBox.UseVisualStyleBackColor = false;
            this.hideDS4CheckBox.CheckedChanged += new System.EventHandler(this.hideDS4CheckBox_CheckedChanged);
            // 
            // cBSwipeProfiles
            // 
            resources.ApplyResources(this.cBSwipeProfiles, "cBSwipeProfiles");
            this.cBSwipeProfiles.Checked = true;
            this.cBSwipeProfiles.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cBSwipeProfiles.Name = "cBSwipeProfiles";
            this.cBSwipeProfiles.UseVisualStyleBackColor = false;
            this.cBSwipeProfiles.CheckedChanged += new System.EventHandler(this.cBSwipeProfiles_CheckedChanged);
            // 
            // StartWindowsCheckBox
            // 
            resources.ApplyResources(this.StartWindowsCheckBox, "StartWindowsCheckBox");
            this.StartWindowsCheckBox.Name = "StartWindowsCheckBox";
            this.StartWindowsCheckBox.UseVisualStyleBackColor = true;
            this.StartWindowsCheckBox.CheckedChanged += new System.EventHandler(this.StartWindowsCheckBox_CheckedChanged);
            // 
            // runStartupPanel
            // 
            this.runStartupPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.runStartupPanel.Controls.Add(this.uacPictureBox);
            this.runStartupPanel.Controls.Add(this.runStartTaskRadio);
            this.runStartupPanel.Controls.Add(this.label1);
            this.runStartupPanel.Controls.Add(this.runStartProgRadio);
            resources.ApplyResources(this.runStartupPanel, "runStartupPanel");
            this.runStartupPanel.Name = "runStartupPanel";
            // 
            // uacPictureBox
            // 
            resources.ApplyResources(this.uacPictureBox, "uacPictureBox");
            this.uacPictureBox.Name = "uacPictureBox";
            this.uacPictureBox.TabStop = false;
            // 
            // runStartTaskRadio
            // 
            resources.ApplyResources(this.runStartTaskRadio, "runStartTaskRadio");
            this.runStartTaskRadio.Name = "runStartTaskRadio";
            this.runStartTaskRadio.TabStop = true;
            this.runStartTaskRadio.UseVisualStyleBackColor = false;
            this.runStartTaskRadio.Click += new System.EventHandler(this.runStartTaskRadio_Click);
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            // 
            // runStartProgRadio
            // 
            resources.ApplyResources(this.runStartProgRadio, "runStartProgRadio");
            this.runStartProgRadio.Checked = true;
            this.runStartProgRadio.Name = "runStartProgRadio";
            this.runStartProgRadio.TabStop = true;
            this.runStartProgRadio.UseVisualStyleBackColor = false;
            this.runStartProgRadio.Click += new System.EventHandler(this.runStartProgRadio_Click);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.lbNotifications);
            this.panel1.Controls.Add(this.cBoxNotifications);
            resources.ApplyResources(this.panel1, "panel1");
            this.panel1.Name = "panel1";
            // 
            // lbNotifications
            // 
            resources.ApplyResources(this.lbNotifications, "lbNotifications");
            this.lbNotifications.Name = "lbNotifications";
            // 
            // cBoxNotifications
            // 
            resources.ApplyResources(this.cBoxNotifications, "cBoxNotifications");
            this.cBoxNotifications.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cBoxNotifications.FormattingEnabled = true;
            this.cBoxNotifications.Items.AddRange(new object[] {
            resources.GetString("cBoxNotifications.Items"),
            resources.GetString("cBoxNotifications.Items1"),
            resources.GetString("cBoxNotifications.Items2")});
            this.cBoxNotifications.Name = "cBoxNotifications";
            this.cBoxNotifications.SelectedIndexChanged += new System.EventHandler(this.cBoxNotifications_SelectedIndexChanged);
            // 
            // cBDisconnectBT
            // 
            resources.ApplyResources(this.cBDisconnectBT, "cBDisconnectBT");
            this.cBDisconnectBT.Name = "cBDisconnectBT";
            this.cBDisconnectBT.UseVisualStyleBackColor = true;
            this.cBDisconnectBT.CheckedChanged += new System.EventHandler(this.cBDisconnectBT_CheckedChanged);
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.nUDLatency);
            this.panel2.Controls.Add(this.lbMsLatency);
            this.panel2.Controls.Add(this.cBFlashWhenLate);
            resources.ApplyResources(this.panel2, "panel2");
            this.panel2.Name = "panel2";
            // 
            // nUDLatency
            // 
            resources.ApplyResources(this.nUDLatency, "nUDLatency");
            this.nUDLatency.Minimum = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.nUDLatency.Name = "nUDLatency";
            this.nUDLatency.Value = new decimal(new int[] {
            20,
            0,
            0,
            0});
            this.nUDLatency.ValueChanged += new System.EventHandler(this.nUDLatency_ValueChanged);
            // 
            // lbMsLatency
            // 
            resources.ApplyResources(this.lbMsLatency, "lbMsLatency");
            this.lbMsLatency.Name = "lbMsLatency";
            // 
            // cBFlashWhenLate
            // 
            resources.ApplyResources(this.cBFlashWhenLate, "cBFlashWhenLate");
            this.cBFlashWhenLate.Checked = true;
            this.cBFlashWhenLate.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cBFlashWhenLate.Name = "cBFlashWhenLate";
            this.cBFlashWhenLate.UseVisualStyleBackColor = false;
            this.cBFlashWhenLate.CheckedChanged += new System.EventHandler(this.cBFlashWhenLate_CheckedChanged);
            // 
            // startMinimizedCheckBox
            // 
            resources.ApplyResources(this.startMinimizedCheckBox, "startMinimizedCheckBox");
            this.startMinimizedCheckBox.Name = "startMinimizedCheckBox";
            this.startMinimizedCheckBox.UseVisualStyleBackColor = true;
            this.startMinimizedCheckBox.CheckedChanged += new System.EventHandler(this.startMinimizedCheckBox_CheckedChanged);
            // 
            // mintoTaskCheckBox
            // 
            resources.ApplyResources(this.mintoTaskCheckBox, "mintoTaskCheckBox");
            this.mintoTaskCheckBox.Name = "mintoTaskCheckBox";
            this.mintoTaskCheckBox.UseVisualStyleBackColor = true;
            // 
            // cBCloseMini
            // 
            resources.ApplyResources(this.cBCloseMini, "cBCloseMini");
            this.cBCloseMini.Name = "cBCloseMini";
            this.cBCloseMini.UseVisualStyleBackColor = true;
            this.cBCloseMini.CheckedChanged += new System.EventHandler(this.cBCloseMini_CheckedChanged);
            // 
            // cBQuickCharge
            // 
            resources.ApplyResources(this.cBQuickCharge, "cBQuickCharge");
            this.cBQuickCharge.Name = "cBQuickCharge";
            this.cBQuickCharge.UseVisualStyleBackColor = true;
            this.cBQuickCharge.CheckedChanged += new System.EventHandler(this.cBQuickCharge_CheckedChanged);
            // 
            // cBUseWhiteIcon
            // 
            resources.ApplyResources(this.cBUseWhiteIcon, "cBUseWhiteIcon");
            this.cBUseWhiteIcon.Name = "cBUseWhiteIcon";
            this.cBUseWhiteIcon.UseVisualStyleBackColor = true;
            this.cBUseWhiteIcon.CheckedChanged += new System.EventHandler(this.cBUseWhiteIcon_CheckedChanged);
            // 
            // cBUpdate
            // 
            resources.ApplyResources(this.cBUpdate, "cBUpdate");
            this.cBUpdate.Name = "cBUpdate";
            this.cBUpdate.UseVisualStyleBackColor = false;
            this.cBUpdate.CheckedChanged += new System.EventHandler(this.cBUpdate_CheckedChanged);
            // 
            // pNUpdate
            // 
            this.pNUpdate.Controls.Add(this.cBUpdateTime);
            this.pNUpdate.Controls.Add(this.lbCheckEvery);
            this.pNUpdate.Controls.Add(this.nUDUpdateTime);
            resources.ApplyResources(this.pNUpdate, "pNUpdate");
            this.pNUpdate.Name = "pNUpdate";
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
            this.cBUpdateTime.SelectedIndexChanged += new System.EventHandler(this.cBUpdateTime_SelectedIndexChanged);
            // 
            // lbCheckEvery
            // 
            resources.ApplyResources(this.lbCheckEvery, "lbCheckEvery");
            this.lbCheckEvery.Name = "lbCheckEvery";
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
            this.nUDUpdateTime.ValueChanged += new System.EventHandler(this.nUDUpdateTime_ValueChanged);
            // 
            // pnlXIPorts
            // 
            this.pnlXIPorts.Controls.Add(this.lbUseXIPorts);
            this.pnlXIPorts.Controls.Add(this.nUDXIPorts);
            this.pnlXIPorts.Controls.Add(this.lbLastXIPort);
            resources.ApplyResources(this.pnlXIPorts, "pnlXIPorts");
            this.pnlXIPorts.Name = "pnlXIPorts";
            // 
            // lbUseXIPorts
            // 
            resources.ApplyResources(this.lbUseXIPorts, "lbUseXIPorts");
            this.lbUseXIPorts.Name = "lbUseXIPorts";
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
            // 
            // languagePackComboBox1
            // 
            resources.ApplyResources(this.languagePackComboBox1, "languagePackComboBox1");
            this.languagePackComboBox1.Name = "languagePackComboBox1";
            this.languagePackComboBox1.SelectedValueChanged += new System.EventHandler(this.languagePackComboBox1_SelectedValueChanged);
            // 
            // flowLayoutPanel1
            // 
            resources.ApplyResources(this.flowLayoutPanel1, "flowLayoutPanel1");
            this.flowLayoutPanel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.flowLayoutPanel1.Controls.Add(this.linkProfiles);
            this.flowLayoutPanel1.Controls.Add(this.lnkControllers);
            this.flowLayoutPanel1.Controls.Add(this.linkUninstall);
            this.flowLayoutPanel1.Controls.Add(this.linkSetup);
            this.flowLayoutPanel1.Controls.Add(this.lLBUpdate);
            this.flowLayoutPanel1.Controls.Add(this.linkSplitLabel);
            this.flowLayoutPanel1.Controls.Add(this.hidGuardWhiteList);
            this.flowLayoutPanel1.Controls.Add(this.clrHidGuardWlistLinkLabel);
            this.flowLayoutPanel1.Controls.Add(this.hidGuardRegLinkLabel);
            this.flowLayoutPanel1.ForeColor = System.Drawing.SystemColors.ControlText;
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            // 
            // linkProfiles
            // 
            resources.ApplyResources(this.linkProfiles, "linkProfiles");
            this.linkProfiles.Name = "linkProfiles";
            this.linkProfiles.TabStop = true;
            this.linkProfiles.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkProfiles_LinkClicked);
            // 
            // lnkControllers
            // 
            resources.ApplyResources(this.lnkControllers, "lnkControllers");
            this.lnkControllers.Name = "lnkControllers";
            this.lnkControllers.TabStop = true;
            this.lnkControllers.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkControllers_LinkClicked);
            // 
            // linkUninstall
            // 
            resources.ApplyResources(this.linkUninstall, "linkUninstall");
            this.linkUninstall.Name = "linkUninstall";
            this.linkUninstall.TabStop = true;
            this.linkUninstall.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkUninstall_LinkClicked);
            // 
            // linkSetup
            // 
            resources.ApplyResources(this.linkSetup, "linkSetup");
            this.linkSetup.Name = "linkSetup";
            this.linkSetup.TabStop = true;
            this.linkSetup.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lLSetup_LinkClicked);
            // 
            // lLBUpdate
            // 
            resources.ApplyResources(this.lLBUpdate, "lLBUpdate");
            this.lLBUpdate.Name = "lLBUpdate";
            this.lLBUpdate.TabStop = true;
            this.lLBUpdate.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lLBUpdate_LinkClicked);
            // 
            // linkSplitLabel
            // 
            resources.ApplyResources(this.linkSplitLabel, "linkSplitLabel");
            this.linkSplitLabel.BackColor = System.Drawing.SystemColors.ActiveBorder;
            this.linkSplitLabel.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.linkSplitLabel.ForeColor = System.Drawing.SystemColors.ControlText;
            this.linkSplitLabel.Name = "linkSplitLabel";
            // 
            // hidGuardWhiteList
            // 
            resources.ApplyResources(this.hidGuardWhiteList, "hidGuardWhiteList");
            this.hidGuardWhiteList.Name = "hidGuardWhiteList";
            this.hidGuardWhiteList.TabStop = true;
            this.hidGuardWhiteList.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.HidGuardWhiteList_LinkClicked);
            // 
            // clrHidGuardWlistLinkLabel
            // 
            resources.ApplyResources(this.clrHidGuardWlistLinkLabel, "clrHidGuardWlistLinkLabel");
            this.clrHidGuardWlistLinkLabel.Name = "clrHidGuardWlistLinkLabel";
            this.clrHidGuardWlistLinkLabel.TabStop = true;
            this.clrHidGuardWlistLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.ClrHidGuardWlistLinkLabel_LinkClicked);
            // 
            // hidGuardRegLinkLabel
            // 
            resources.ApplyResources(this.hidGuardRegLinkLabel, "hidGuardRegLinkLabel");
            this.hidGuardRegLinkLabel.Name = "hidGuardRegLinkLabel";
            this.hidGuardRegLinkLabel.TabStop = true;
            this.hidGuardRegLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.HidGuardRegLinkLabel_LinkClicked);
            // 
            // tabLog
            // 
            this.tabLog.Controls.Add(this.lvDebug);
            this.tabLog.Controls.Add(this.panel3);
            resources.ApplyResources(this.tabLog, "tabLog");
            this.tabLog.Name = "tabLog";
            this.tabLog.UseVisualStyleBackColor = true;
            // 
            // panel3
            // 
            this.panel3.Controls.Add(this.exportLogTxtBtn);
            this.panel3.Controls.Add(this.btnClear);
            resources.ApplyResources(this.panel3, "panel3");
            this.panel3.Name = "panel3";
            // 
            // exportLogTxtBtn
            // 
            resources.ApplyResources(this.exportLogTxtBtn, "exportLogTxtBtn");
            this.exportLogTxtBtn.Name = "exportLogTxtBtn";
            this.exportLogTxtBtn.UseVisualStyleBackColor = true;
            this.exportLogTxtBtn.Click += new System.EventHandler(this.exportLogTxtBtn_Click);
            // 
            // btnClear
            // 
            resources.ApplyResources(this.btnClear, "btnClear");
            this.btnClear.Name = "btnClear";
            this.btnClear.UseVisualStyleBackColor = true;
            this.btnClear.Click += new System.EventHandler(this.BtnClear_Click);
            // 
            // saveProfiles
            // 
            resources.ApplyResources(this.saveProfiles, "saveProfiles");
            // 
            // cMCustomLed
            // 
            this.cMCustomLed.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.cMCustomLed.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.useProfileColorToolStripMenuItem,
            this.useCustomColorToolStripMenuItem});
            this.cMCustomLed.Name = "cMCustomLed";
            this.cMCustomLed.ShowCheckMargin = true;
            this.cMCustomLed.ShowImageMargin = false;
            resources.ApplyResources(this.cMCustomLed, "cMCustomLed");
            // 
            // useProfileColorToolStripMenuItem
            // 
            this.useProfileColorToolStripMenuItem.Checked = true;
            this.useProfileColorToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.useProfileColorToolStripMenuItem.Name = "useProfileColorToolStripMenuItem";
            resources.ApplyResources(this.useProfileColorToolStripMenuItem, "useProfileColorToolStripMenuItem");
            this.useProfileColorToolStripMenuItem.Click += new System.EventHandler(this.useProfileColorToolStripMenuItem_Click);
            // 
            // useCustomColorToolStripMenuItem
            // 
            this.useCustomColorToolStripMenuItem.Name = "useCustomColorToolStripMenuItem";
            resources.ApplyResources(this.useCustomColorToolStripMenuItem, "useCustomColorToolStripMenuItem");
            this.useCustomColorToolStripMenuItem.Click += new System.EventHandler(this.useCustomColorToolStripMenuItem_Click);
            // 
            // DS4Form
            // 
            this.AllowDrop = true;
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.BackColor = System.Drawing.SystemColors.Window;
            this.Controls.Add(this.tabMain);
            this.Controls.Add(this.pnlButton);
            this.Name = "DS4Form";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ScpForm_Closing);
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
            this.runStartupPanel.ResumeLayout(false);
            this.runStartupPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.uacPictureBox)).EndInit();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nUDLatency)).EndInit();
            this.pNUpdate.ResumeLayout(false);
            this.pNUpdate.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nUDUpdateTime)).EndInit();
            this.pnlXIPorts.ResumeLayout(false);
            this.pnlXIPorts.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nUDXIPorts)).EndInit();
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
            this.tabLog.ResumeLayout(false);
            this.panel3.ResumeLayout(false);
            this.cMCustomLed.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListView lvDebug;
        private System.Windows.Forms.ColumnHeader chTime;
        private System.Windows.Forms.ColumnHeader chData;
        private System.Windows.Forms.Panel pnlButton;
        private System.Windows.Forms.Button btnStartStop;
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
        private System.Windows.Forms.CheckBox hideDS4CheckBox;
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
        private System.Windows.Forms.CheckBox cBFlashWhenLate;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label lbNotifications;
        private System.Windows.Forms.ComboBox cBoxNotifications;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.NumericUpDown nUDLatency;
        private System.Windows.Forms.Label lbMsLatency;
        private System.Windows.Forms.Button bnLight3;
        private System.Windows.Forms.Button bnLight1;
        private System.Windows.Forms.Button bnLight2;
        private System.Windows.Forms.Button bnLight4;
        private System.Windows.Forms.ContextMenuStrip cMCustomLed;
        private System.Windows.Forms.ToolStripMenuItem useProfileColorToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem useCustomColorToolStripMenuItem;
        private AdvancedColorDialog advColorDialog;
        private System.Windows.Forms.CheckBox cBUseWhiteIcon;
        private System.Windows.Forms.Panel runStartupPanel;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.RadioButton runStartProgRadio;
        private System.Windows.Forms.RadioButton runStartTaskRadio;
        private System.Windows.Forms.PictureBox uacPictureBox;
        private System.Windows.Forms.Label lbBattery;
        private System.Windows.Forms.Label lbLinkProfile;
        private System.Windows.Forms.CheckBox linkCB1;
        private System.Windows.Forms.CheckBox linkCB2;
        private System.Windows.Forms.CheckBox linkCB3;
        private System.Windows.Forms.CheckBox linkCB4;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Button exportLogTxtBtn;
        private System.Windows.Forms.Button btnClear;
        private DS4Forms.LanguagePackComboBox languagePackComboBox1;
        private System.Windows.Forms.LinkLabel hidGuardWhiteList;
        private System.Windows.Forms.LinkLabel clrHidGuardWlistLinkLabel;
        private System.Windows.Forms.LinkLabel hidGuardRegLinkLabel;
        private System.Windows.Forms.Label linkSplitLabel;
        private System.Windows.Forms.ToolStripMenuItem openProgramFolderToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripMenuItem disconToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem discon1toolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem discon2ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem discon3ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem discon4ToolStripMenuItem;
        private System.Windows.Forms.CheckBox mintoTaskCheckBox;
        //private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem2;
    }
}

