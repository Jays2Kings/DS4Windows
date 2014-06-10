namespace ScpServer
{
    partial class ScpForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ScpForm));
            this.lvDebug = new System.Windows.Forms.ListView();
            this.chTime = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.chData = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.tmrUpdate = new System.Windows.Forms.Timer(this.components);
            this.pnlButton = new System.Windows.Forms.Panel();
            this.lBTest = new System.Windows.Forms.Label();
            this.btnStartStop = new System.Windows.Forms.Button();
            this.lbLastMessage = new System.Windows.Forms.Label();
            this.llbHelp = new System.Windows.Forms.LinkLabel();
            this.lnkControllers = new System.Windows.Forms.LinkLabel();
            this.StartWindowsCheckBox = new System.Windows.Forms.CheckBox();
            this.startMinimizedCheckBox = new System.Windows.Forms.CheckBox();
            this.hideDS4CheckBox = new System.Windows.Forms.CheckBox();
            this.btnClear = new System.Windows.Forms.Button();
            this.notifyIcon1 = new System.Windows.Forms.NotifyIcon(this.components);
            this.cMTaskbar = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.editProfileForController1ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.editProfileForController2ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.editProfileForController3ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.editProfileForController4ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
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
            this.lBSelectedProfile = new System.Windows.Forms.Label();
            this.lBID = new System.Windows.Forms.Label();
            this.lBStatus = new System.Windows.Forms.Label();
            this.lBBattery = new System.Windows.Forms.Label();
            this.lBBatt1 = new System.Windows.Forms.Label();
            this.lBBatt2 = new System.Windows.Forms.Label();
            this.lBBatt3 = new System.Windows.Forms.Label();
            this.lBBatt4 = new System.Windows.Forms.Label();
            this.pBStatus2 = new System.Windows.Forms.PictureBox();
            this.pBStatus3 = new System.Windows.Forms.PictureBox();
            this.pBStatus4 = new System.Windows.Forms.PictureBox();
            this.tabProfiles = new System.Windows.Forms.TabPage();
            this.tSOptions = new System.Windows.Forms.ToolStrip();
            this.toolStripLabel1 = new System.Windows.Forms.ToolStripLabel();
            this.tSTBProfile = new System.Windows.Forms.ToolStripTextBox();
            this.toolStripButton1 = new System.Windows.Forms.ToolStripButton();
            this.tSBCancel = new System.Windows.Forms.ToolStripButton();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.tsBNewProfle = new System.Windows.Forms.ToolStripButton();
            this.tsBEditProfile = new System.Windows.Forms.ToolStripButton();
            this.tsBDeleteProfile = new System.Windows.Forms.ToolStripButton();
            this.tSBDupProfile = new System.Windows.Forms.ToolStripButton();
            this.tSBImportProfile = new System.Windows.Forms.ToolStripButton();
            this.tSBExportProfile = new System.Windows.Forms.ToolStripButton();
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
            this.tabLog = new System.Windows.Forms.TabPage();
            this.tabAutoProfiles = new System.Windows.Forms.TabPage();
            this.tabSettings = new System.Windows.Forms.TabPage();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.cBNotifications = new System.Windows.Forms.CheckBox();
            this.cBUpdate = new System.Windows.Forms.CheckBox();
            this.pNUpdate = new System.Windows.Forms.Panel();
            this.cBUpdateTime = new System.Windows.Forms.ComboBox();
            this.lBCheckEvery = new System.Windows.Forms.Label();
            this.nUDUpdateTime = new System.Windows.Forms.NumericUpDown();
            this.linkProfiles = new System.Windows.Forms.LinkLabel();
            this.lLBUpdate = new System.Windows.Forms.LinkLabel();
            this.linkUninstall = new System.Windows.Forms.LinkLabel();
            this.lLSetup = new System.Windows.Forms.LinkLabel();
            this.saveProfiles = new System.Windows.Forms.SaveFileDialog();
            this.lLAppDataDelete = new System.Windows.Forms.LinkLabel();
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
            this.tSOptions.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            this.cMProfile.SuspendLayout();
            this.tabLog.SuspendLayout();
            this.tabSettings.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.pNUpdate.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nUDUpdateTime)).BeginInit();
            this.SuspendLayout();
            // 
            // lvDebug
            // 
            this.lvDebug.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.chTime,
            this.chData});
            this.lvDebug.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lvDebug.Font = new System.Drawing.Font("Lucida Console", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lvDebug.FullRowSelect = true;
            this.lvDebug.Location = new System.Drawing.Point(3, 3);
            this.lvDebug.Name = "lvDebug";
            this.lvDebug.Size = new System.Drawing.Size(780, 284);
            this.lvDebug.TabIndex = 0;
            this.lvDebug.UseCompatibleStateImageBehavior = false;
            this.lvDebug.View = System.Windows.Forms.View.Details;
            this.lvDebug.ItemActivate += new System.EventHandler(this.lvDebug_ItemActivate);
            // 
            // chTime
            // 
            this.chTime.Text = "Time";
            this.chTime.Width = 200;
            // 
            // chData
            // 
            this.chData.Text = "Data";
            this.chData.Width = 563;
            // 
            // tmrUpdate
            // 
            this.tmrUpdate.Interval = 1;
            this.tmrUpdate.Tick += new System.EventHandler(this.ControllerStatusChange);
            // 
            // pnlButton
            // 
            this.pnlButton.BackColor = System.Drawing.SystemColors.Control;
            this.pnlButton.Controls.Add(this.lBTest);
            this.pnlButton.Controls.Add(this.btnStartStop);
            this.pnlButton.Controls.Add(this.lbLastMessage);
            this.pnlButton.Controls.Add(this.llbHelp);
            this.pnlButton.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlButton.Location = new System.Drawing.Point(0, 339);
            this.pnlButton.Name = "pnlButton";
            this.pnlButton.Size = new System.Drawing.Size(794, 30);
            this.pnlButton.TabIndex = 10;
            // 
            // lBTest
            // 
            this.lBTest.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.lBTest.AutoSize = true;
            this.lBTest.Location = new System.Drawing.Point(291, 9);
            this.lBTest.Name = "lBTest";
            this.lBTest.Size = new System.Drawing.Size(98, 13);
            this.lBTest.TabIndex = 46;
            this.lBTest.Text = "Used to test values";
            this.lBTest.Visible = false;
            // 
            // btnStartStop
            // 
            this.btnStartStop.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnStartStop.Location = new System.Drawing.Point(729, 4);
            this.btnStartStop.Name = "btnStartStop";
            this.btnStartStop.Size = new System.Drawing.Size(58, 23);
            this.btnStartStop.TabIndex = 1;
            this.btnStartStop.Text = "Start";
            this.btnStartStop.UseVisualStyleBackColor = true;
            this.btnStartStop.Click += new System.EventHandler(this.btnStartStop_Click);
            // 
            // lbLastMessage
            // 
            this.lbLastMessage.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lbLastMessage.ForeColor = System.Drawing.SystemColors.GrayText;
            this.lbLastMessage.Location = new System.Drawing.Point(4, 9);
            this.lbLastMessage.Name = "lbLastMessage";
            this.lbLastMessage.Size = new System.Drawing.Size(634, 18);
            this.lbLastMessage.TabIndex = 41;
            // 
            // llbHelp
            // 
            this.llbHelp.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.llbHelp.AutoSize = true;
            this.llbHelp.Location = new System.Drawing.Point(644, 9);
            this.llbHelp.Name = "llbHelp";
            this.llbHelp.Size = new System.Drawing.Size(79, 13);
            this.llbHelp.TabIndex = 13;
            this.llbHelp.TabStop = true;
            this.llbHelp.Text = "Hotkeys/About";
            this.llbHelp.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.llbHelp_LinkClicked);
            // 
            // lnkControllers
            // 
            this.lnkControllers.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.lnkControllers.AutoSize = true;
            this.lnkControllers.Location = new System.Drawing.Point(269, 30);
            this.lnkControllers.Name = "lnkControllers";
            this.lnkControllers.Size = new System.Drawing.Size(73, 13);
            this.lnkControllers.TabIndex = 11;
            this.lnkControllers.TabStop = true;
            this.lnkControllers.Text = "CP Controllers";
            this.lnkControllers.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkControllers_LinkClicked);
            // 
            // StartWindowsCheckBox
            // 
            this.StartWindowsCheckBox.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.StartWindowsCheckBox.AutoSize = true;
            this.StartWindowsCheckBox.Location = new System.Drawing.Point(3, 3);
            this.StartWindowsCheckBox.Name = "StartWindowsCheckBox";
            this.StartWindowsCheckBox.Size = new System.Drawing.Size(95, 17);
            this.StartWindowsCheckBox.TabIndex = 40;
            this.StartWindowsCheckBox.Text = "Run at Startup";
            this.StartWindowsCheckBox.UseVisualStyleBackColor = true;
            this.StartWindowsCheckBox.CheckedChanged += new System.EventHandler(this.StartWindowsCheckBox_CheckedChanged);
            // 
            // startMinimizedCheckBox
            // 
            this.startMinimizedCheckBox.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.startMinimizedCheckBox.AutoSize = true;
            this.startMinimizedCheckBox.Location = new System.Drawing.Point(104, 3);
            this.startMinimizedCheckBox.Name = "startMinimizedCheckBox";
            this.startMinimizedCheckBox.Size = new System.Drawing.Size(97, 17);
            this.startMinimizedCheckBox.TabIndex = 40;
            this.startMinimizedCheckBox.Text = "Start Minimized";
            this.startMinimizedCheckBox.UseVisualStyleBackColor = true;
            this.startMinimizedCheckBox.CheckedChanged += new System.EventHandler(this.startMinimizedCheckBox_CheckedChanged);
            // 
            // hideDS4CheckBox
            // 
            this.hideDS4CheckBox.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.hideDS4CheckBox.AutoSize = true;
            this.hideDS4CheckBox.Location = new System.Drawing.Point(207, 3);
            this.hideDS4CheckBox.Name = "hideDS4CheckBox";
            this.hideDS4CheckBox.Size = new System.Drawing.Size(119, 17);
            this.hideDS4CheckBox.TabIndex = 13;
            this.hideDS4CheckBox.Text = "Hide DS4 Controller";
            this.hideDS4CheckBox.UseVisualStyleBackColor = true;
            this.hideDS4CheckBox.CheckedChanged += new System.EventHandler(this.hideDS4CheckBox_CheckedChanged);
            // 
            // btnClear
            // 
            this.btnClear.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.btnClear.Enabled = false;
            this.btnClear.Location = new System.Drawing.Point(3, 287);
            this.btnClear.Name = "btnClear";
            this.btnClear.Size = new System.Drawing.Size(780, 23);
            this.btnClear.TabIndex = 9;
            this.btnClear.Text = "Clear";
            this.btnClear.UseVisualStyleBackColor = true;
            this.btnClear.Click += new System.EventHandler(this.btnClear_Click);
            // 
            // notifyIcon1
            // 
            this.notifyIcon1.BalloonTipIcon = System.Windows.Forms.ToolTipIcon.Info;
            this.notifyIcon1.BalloonTipTitle = "Scp server";
            this.notifyIcon1.ContextMenuStrip = this.cMTaskbar;
            this.notifyIcon1.Icon = ((System.Drawing.Icon)(resources.GetObject("notifyIcon1.Icon")));
            this.notifyIcon1.Text = "DS4 Xinput Tool";
            this.notifyIcon1.Visible = true;
            this.notifyIcon1.BalloonTipClicked += new System.EventHandler(this.notifyIcon1_BalloonTipClicked);
            this.notifyIcon1.MouseClick += new System.Windows.Forms.MouseEventHandler(this.notifyIcon1_MouseClick);
            // 
            // cMTaskbar
            // 
            this.cMTaskbar.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.editProfileForController1ToolStripMenuItem,
            this.editProfileForController2ToolStripMenuItem,
            this.editProfileForController3ToolStripMenuItem,
            this.editProfileForController4ToolStripMenuItem,
            this.toolStripSeparator1,
            this.openToolStripMenuItem,
            this.exitToolStripMenuItem});
            this.cMTaskbar.Name = "cMTaskbar";
            this.cMTaskbar.Size = new System.Drawing.Size(215, 142);
            this.cMTaskbar.Tag = "25";
            // 
            // editProfileForController1ToolStripMenuItem
            // 
            this.editProfileForController1ToolStripMenuItem.Name = "editProfileForController1ToolStripMenuItem";
            this.editProfileForController1ToolStripMenuItem.Size = new System.Drawing.Size(214, 22);
            this.editProfileForController1ToolStripMenuItem.Tag = "0";
            this.editProfileForController1ToolStripMenuItem.Text = "Edit Profile for Controller 1";
            this.editProfileForController1ToolStripMenuItem.Click += new System.EventHandler(this.editMenu_Click);
            // 
            // editProfileForController2ToolStripMenuItem
            // 
            this.editProfileForController2ToolStripMenuItem.Name = "editProfileForController2ToolStripMenuItem";
            this.editProfileForController2ToolStripMenuItem.Size = new System.Drawing.Size(214, 22);
            this.editProfileForController2ToolStripMenuItem.Tag = "1";
            this.editProfileForController2ToolStripMenuItem.Text = "Edit Profile for Controller 2";
            this.editProfileForController2ToolStripMenuItem.Click += new System.EventHandler(this.editMenu_Click);
            // 
            // editProfileForController3ToolStripMenuItem
            // 
            this.editProfileForController3ToolStripMenuItem.Name = "editProfileForController3ToolStripMenuItem";
            this.editProfileForController3ToolStripMenuItem.Size = new System.Drawing.Size(214, 22);
            this.editProfileForController3ToolStripMenuItem.Tag = "2";
            this.editProfileForController3ToolStripMenuItem.Text = "Edit Profile for Controller 3";
            this.editProfileForController3ToolStripMenuItem.Click += new System.EventHandler(this.editMenu_Click);
            // 
            // editProfileForController4ToolStripMenuItem
            // 
            this.editProfileForController4ToolStripMenuItem.Name = "editProfileForController4ToolStripMenuItem";
            this.editProfileForController4ToolStripMenuItem.Size = new System.Drawing.Size(214, 22);
            this.editProfileForController4ToolStripMenuItem.Tag = "4";
            this.editProfileForController4ToolStripMenuItem.Text = "Edit Profile for Controller 4";
            this.editProfileForController4ToolStripMenuItem.Click += new System.EventHandler(this.editMenu_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(211, 6);
            // 
            // openToolStripMenuItem
            // 
            this.openToolStripMenuItem.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.openToolStripMenuItem.Name = "openToolStripMenuItem";
            this.openToolStripMenuItem.Size = new System.Drawing.Size(214, 22);
            this.openToolStripMenuItem.Text = "Open";
            this.openToolStripMenuItem.Click += new System.EventHandler(this.openToolStripMenuItem_Click);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(214, 22);
            this.exitToolStripMenuItem.Text = "Exit (Middle Mouse)";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(211, 6);
            // 
            // openProfiles
            // 
            this.openProfiles.Filter = "XML Files (*.xml)|*.xml";
            this.openProfiles.Multiselect = true;
            // 
            // tabMain
            // 
            this.tabMain.Controls.Add(this.tabControllers);
            this.tabMain.Controls.Add(this.tabProfiles);
            this.tabMain.Controls.Add(this.tabLog);
            this.tabMain.Controls.Add(this.tabAutoProfiles);
            this.tabMain.Controls.Add(this.tabSettings);
            this.tabMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabMain.Location = new System.Drawing.Point(0, 0);
            this.tabMain.Name = "tabMain";
            this.tabMain.SelectedIndex = 0;
            this.tabMain.Size = new System.Drawing.Size(794, 339);
            this.tabMain.TabIndex = 12;
            this.tabMain.SelectedIndexChanged += new System.EventHandler(this.tabMain_SelectedIndexChanged);
            // 
            // tabControllers
            // 
            this.tabControllers.Controls.Add(this.tLPControllers);
            this.tabControllers.Location = new System.Drawing.Point(4, 22);
            this.tabControllers.Name = "tabControllers";
            this.tabControllers.Size = new System.Drawing.Size(786, 313);
            this.tabControllers.TabIndex = 3;
            this.tabControllers.Text = "Controllers";
            this.tabControllers.UseVisualStyleBackColor = true;
            // 
            // tLPControllers
            // 
            this.tLPControllers.ColumnCount = 5;
            this.tLPControllers.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 51.34884F));
            this.tLPControllers.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 23.34039F));
            this.tLPControllers.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25.31077F));
            this.tLPControllers.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 117F));
            this.tLPControllers.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 50F));
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
            this.tLPControllers.Controls.Add(this.lBSelectedProfile, 3, 0);
            this.tLPControllers.Controls.Add(this.lBID, 0, 0);
            this.tLPControllers.Controls.Add(this.lBStatus, 1, 0);
            this.tLPControllers.Controls.Add(this.lBBattery, 2, 0);
            this.tLPControllers.Controls.Add(this.lBBatt1, 2, 1);
            this.tLPControllers.Controls.Add(this.lBBatt2, 2, 2);
            this.tLPControllers.Controls.Add(this.lBBatt3, 2, 3);
            this.tLPControllers.Controls.Add(this.lBBatt4, 2, 4);
            this.tLPControllers.Controls.Add(this.pBStatus2, 1, 2);
            this.tLPControllers.Controls.Add(this.pBStatus3, 1, 3);
            this.tLPControllers.Controls.Add(this.pBStatus4, 1, 4);
            this.tLPControllers.Dock = System.Windows.Forms.DockStyle.Top;
            this.tLPControllers.Location = new System.Drawing.Point(0, 0);
            this.tLPControllers.Name = "tLPControllers";
            this.tLPControllers.RowCount = 5;
            this.tLPControllers.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tLPControllers.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tLPControllers.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tLPControllers.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tLPControllers.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tLPControllers.Size = new System.Drawing.Size(786, 130);
            this.tLPControllers.TabIndex = 46;
            // 
            // pBStatus1
            // 
            this.pBStatus1.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.pBStatus1.Image = ((System.Drawing.Image)(resources.GetObject("pBStatus1.Image")));
            this.pBStatus1.InitialImage = global::ScpServer.Properties.Resources.BT;
            this.pBStatus1.Location = new System.Drawing.Point(369, 19);
            this.pBStatus1.Name = "pBStatus1";
            this.pBStatus1.Size = new System.Drawing.Size(39, 20);
            this.pBStatus1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.pBStatus1.TabIndex = 47;
            this.pBStatus1.TabStop = false;
            // 
            // lbPad1
            // 
            this.lbPad1.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lbPad1.AutoSize = true;
            this.lbPad1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbPad1.Location = new System.Drawing.Point(3, 22);
            this.lbPad1.Name = "lbPad1";
            this.lbPad1.Size = new System.Drawing.Size(123, 15);
            this.lbPad1.TabIndex = 44;
            this.lbPad1.Text = "Pad 1 : Disconnected";
            // 
            // lbPad2
            // 
            this.lbPad2.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lbPad2.AutoSize = true;
            this.lbPad2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbPad2.Location = new System.Drawing.Point(3, 51);
            this.lbPad2.Name = "lbPad2";
            this.lbPad2.Size = new System.Drawing.Size(123, 15);
            this.lbPad2.TabIndex = 44;
            this.lbPad2.Text = "Pad 2 : Disconnected";
            // 
            // bnEditC3
            // 
            this.bnEditC3.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.bnEditC3.Location = new System.Drawing.Point(737, 76);
            this.bnEditC3.Name = "bnEditC3";
            this.bnEditC3.Size = new System.Drawing.Size(40, 23);
            this.bnEditC3.TabIndex = 43;
            this.bnEditC3.Tag = "2";
            this.bnEditC3.Text = "Edit";
            this.bnEditC3.UseVisualStyleBackColor = true;
            this.bnEditC3.Click += new System.EventHandler(this.editButtons_Click);
            // 
            // bnEditC4
            // 
            this.bnEditC4.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.bnEditC4.Location = new System.Drawing.Point(737, 105);
            this.bnEditC4.Name = "bnEditC4";
            this.bnEditC4.Size = new System.Drawing.Size(40, 23);
            this.bnEditC4.TabIndex = 43;
            this.bnEditC4.Tag = "3";
            this.bnEditC4.Text = "Edit";
            this.bnEditC4.UseVisualStyleBackColor = true;
            this.bnEditC4.Click += new System.EventHandler(this.editButtons_Click);
            // 
            // lbPad3
            // 
            this.lbPad3.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lbPad3.AutoSize = true;
            this.lbPad3.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbPad3.Location = new System.Drawing.Point(3, 80);
            this.lbPad3.Name = "lbPad3";
            this.lbPad3.Size = new System.Drawing.Size(123, 15);
            this.lbPad3.TabIndex = 44;
            this.lbPad3.Text = "Pad 3 : Disconnected";
            // 
            // lbPad4
            // 
            this.lbPad4.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lbPad4.AutoSize = true;
            this.lbPad4.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbPad4.Location = new System.Drawing.Point(3, 109);
            this.lbPad4.Name = "lbPad4";
            this.lbPad4.Size = new System.Drawing.Size(123, 15);
            this.lbPad4.TabIndex = 44;
            this.lbPad4.Text = "Pad 4 : Disconnected";
            // 
            // cBController1
            // 
            this.cBController1.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.cBController1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cBController1.FormattingEnabled = true;
            this.cBController1.Location = new System.Drawing.Point(620, 19);
            this.cBController1.Name = "cBController1";
            this.cBController1.Size = new System.Drawing.Size(111, 21);
            this.cBController1.TabIndex = 42;
            this.cBController1.Tag = "0";
            this.cBController1.SelectedValueChanged += new System.EventHandler(this.Profile_Changed);
            // 
            // bnEditC2
            // 
            this.bnEditC2.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.bnEditC2.Location = new System.Drawing.Point(737, 47);
            this.bnEditC2.Name = "bnEditC2";
            this.bnEditC2.Size = new System.Drawing.Size(40, 23);
            this.bnEditC2.TabIndex = 43;
            this.bnEditC2.Tag = "1";
            this.bnEditC2.Text = "Edit";
            this.bnEditC2.UseVisualStyleBackColor = true;
            this.bnEditC2.Click += new System.EventHandler(this.editButtons_Click);
            // 
            // cBController2
            // 
            this.cBController2.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.cBController2.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cBController2.FormattingEnabled = true;
            this.cBController2.Location = new System.Drawing.Point(620, 48);
            this.cBController2.Name = "cBController2";
            this.cBController2.Size = new System.Drawing.Size(111, 21);
            this.cBController2.TabIndex = 42;
            this.cBController2.Tag = "1";
            this.cBController2.SelectedValueChanged += new System.EventHandler(this.Profile_Changed);
            // 
            // cBController3
            // 
            this.cBController3.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.cBController3.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cBController3.FormattingEnabled = true;
            this.cBController3.Location = new System.Drawing.Point(620, 77);
            this.cBController3.Name = "cBController3";
            this.cBController3.Size = new System.Drawing.Size(111, 21);
            this.cBController3.TabIndex = 42;
            this.cBController3.Tag = "2";
            this.cBController3.SelectedValueChanged += new System.EventHandler(this.Profile_Changed);
            // 
            // bnEditC1
            // 
            this.bnEditC1.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.bnEditC1.Location = new System.Drawing.Point(737, 18);
            this.bnEditC1.Name = "bnEditC1";
            this.bnEditC1.Size = new System.Drawing.Size(40, 23);
            this.bnEditC1.TabIndex = 43;
            this.bnEditC1.Tag = "0";
            this.bnEditC1.Text = "Edit";
            this.bnEditC1.UseVisualStyleBackColor = true;
            this.bnEditC1.Click += new System.EventHandler(this.editButtons_Click);
            // 
            // cBController4
            // 
            this.cBController4.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.cBController4.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cBController4.FormattingEnabled = true;
            this.cBController4.Location = new System.Drawing.Point(620, 106);
            this.cBController4.Name = "cBController4";
            this.cBController4.Size = new System.Drawing.Size(111, 21);
            this.cBController4.TabIndex = 42;
            this.cBController4.Tag = "3";
            this.cBController4.SelectedValueChanged += new System.EventHandler(this.Profile_Changed);
            // 
            // lBSelectedProfile
            // 
            this.lBSelectedProfile.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.lBSelectedProfile.AutoSize = true;
            this.lBSelectedProfile.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lBSelectedProfile.Location = new System.Drawing.Point(621, 0);
            this.lBSelectedProfile.Name = "lBSelectedProfile";
            this.lBSelectedProfile.Size = new System.Drawing.Size(109, 15);
            this.lBSelectedProfile.TabIndex = 45;
            this.lBSelectedProfile.Text = "Selected Profile";
            // 
            // lBID
            // 
            this.lBID.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lBID.AutoSize = true;
            this.lBID.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lBID.Location = new System.Drawing.Point(3, 0);
            this.lBID.Name = "lBID";
            this.lBID.Size = new System.Drawing.Size(21, 15);
            this.lBID.TabIndex = 45;
            this.lBID.Text = "ID";
            // 
            // lBStatus
            // 
            this.lBStatus.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.lBStatus.AutoSize = true;
            this.lBStatus.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lBStatus.Location = new System.Drawing.Point(365, 0);
            this.lBStatus.Name = "lBStatus";
            this.lBStatus.Size = new System.Drawing.Size(47, 15);
            this.lBStatus.TabIndex = 45;
            this.lBStatus.Text = "Status";
            // 
            // lBBattery
            // 
            this.lBBattery.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.lBBattery.AutoSize = true;
            this.lBBattery.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lBBattery.Location = new System.Drawing.Point(513, 0);
            this.lBBattery.Name = "lBBattery";
            this.lBBattery.Size = new System.Drawing.Size(51, 15);
            this.lBBattery.TabIndex = 45;
            this.lBBattery.Text = "Battery";
            // 
            // lBBatt1
            // 
            this.lBBatt1.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.lBBatt1.AutoSize = true;
            this.lBBatt1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lBBatt1.Location = new System.Drawing.Point(519, 22);
            this.lBBatt1.Name = "lBBatt1";
            this.lBBatt1.Size = new System.Drawing.Size(39, 15);
            this.lBBatt1.TabIndex = 44;
            this.lBBatt1.Text = "100%";
            // 
            // lBBatt2
            // 
            this.lBBatt2.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.lBBatt2.AutoSize = true;
            this.lBBatt2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lBBatt2.Location = new System.Drawing.Point(519, 51);
            this.lBBatt2.Name = "lBBatt2";
            this.lBBatt2.Size = new System.Drawing.Size(39, 15);
            this.lBBatt2.TabIndex = 44;
            this.lBBatt2.Text = "100%";
            // 
            // lBBatt3
            // 
            this.lBBatt3.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.lBBatt3.AutoSize = true;
            this.lBBatt3.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lBBatt3.Location = new System.Drawing.Point(519, 80);
            this.lBBatt3.Name = "lBBatt3";
            this.lBBatt3.Size = new System.Drawing.Size(39, 15);
            this.lBBatt3.TabIndex = 44;
            this.lBBatt3.Text = "100%";
            // 
            // lBBatt4
            // 
            this.lBBatt4.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.lBBatt4.AutoSize = true;
            this.lBBatt4.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lBBatt4.Location = new System.Drawing.Point(519, 109);
            this.lBBatt4.Name = "lBBatt4";
            this.lBBatt4.Size = new System.Drawing.Size(39, 15);
            this.lBBatt4.TabIndex = 44;
            this.lBBatt4.Text = "100%";
            // 
            // pBStatus2
            // 
            this.pBStatus2.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.pBStatus2.Image = ((System.Drawing.Image)(resources.GetObject("pBStatus2.Image")));
            this.pBStatus2.InitialImage = global::ScpServer.Properties.Resources.BT;
            this.pBStatus2.Location = new System.Drawing.Point(369, 48);
            this.pBStatus2.Name = "pBStatus2";
            this.pBStatus2.Size = new System.Drawing.Size(39, 20);
            this.pBStatus2.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.pBStatus2.TabIndex = 47;
            this.pBStatus2.TabStop = false;
            // 
            // pBStatus3
            // 
            this.pBStatus3.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.pBStatus3.Image = ((System.Drawing.Image)(resources.GetObject("pBStatus3.Image")));
            this.pBStatus3.InitialImage = global::ScpServer.Properties.Resources.BT;
            this.pBStatus3.Location = new System.Drawing.Point(369, 77);
            this.pBStatus3.Name = "pBStatus3";
            this.pBStatus3.Size = new System.Drawing.Size(39, 20);
            this.pBStatus3.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.pBStatus3.TabIndex = 47;
            this.pBStatus3.TabStop = false;
            // 
            // pBStatus4
            // 
            this.pBStatus4.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.pBStatus4.Image = ((System.Drawing.Image)(resources.GetObject("pBStatus4.Image")));
            this.pBStatus4.InitialImage = global::ScpServer.Properties.Resources.BT;
            this.pBStatus4.Location = new System.Drawing.Point(369, 106);
            this.pBStatus4.Name = "pBStatus4";
            this.pBStatus4.Size = new System.Drawing.Size(39, 20);
            this.pBStatus4.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.pBStatus4.TabIndex = 47;
            this.pBStatus4.TabStop = false;
            // 
            // tabProfiles
            // 
            this.tabProfiles.Controls.Add(this.tSOptions);
            this.tabProfiles.Controls.Add(this.toolStrip1);
            this.tabProfiles.Controls.Add(this.lBProfiles);
            this.tabProfiles.Location = new System.Drawing.Point(4, 22);
            this.tabProfiles.Name = "tabProfiles";
            this.tabProfiles.Padding = new System.Windows.Forms.Padding(3);
            this.tabProfiles.Size = new System.Drawing.Size(786, 313);
            this.tabProfiles.TabIndex = 0;
            this.tabProfiles.Text = "Profiles";
            this.tabProfiles.UseVisualStyleBackColor = true;
            // 
            // tSOptions
            // 
            this.tSOptions.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.tSOptions.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripLabel1,
            this.tSTBProfile,
            this.toolStripButton1,
            this.tSBCancel});
            this.tSOptions.Location = new System.Drawing.Point(3, 28);
            this.tSOptions.Name = "tSOptions";
            this.tSOptions.Size = new System.Drawing.Size(780, 25);
            this.tSOptions.TabIndex = 2;
            this.tSOptions.Text = "toolStrip2";
            // 
            // toolStripLabel1
            // 
            this.toolStripLabel1.Name = "toolStripLabel1";
            this.toolStripLabel1.Size = new System.Drawing.Size(82, 22);
            this.toolStripLabel1.Text = "Profile Name: ";
            // 
            // tSTBProfile
            // 
            this.tSTBProfile.ForeColor = System.Drawing.SystemColors.GrayText;
            this.tSTBProfile.Name = "tSTBProfile";
            this.tSTBProfile.Size = new System.Drawing.Size(185, 25);
            this.tSTBProfile.Text = "<type profile name here>";
            this.tSTBProfile.Enter += new System.EventHandler(this.tBProfile_Enter);
            this.tSTBProfile.Leave += new System.EventHandler(this.tBProfile_Leave);
            this.tSTBProfile.TextChanged += new System.EventHandler(this.tBProfile_TextChanged);
            // 
            // toolStripButton1
            // 
            this.toolStripButton1.AutoToolTip = false;
            this.toolStripButton1.Image = global::ScpServer.Properties.Resources.saveprofile;
            this.toolStripButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton1.Name = "toolStripButton1";
            this.toolStripButton1.Size = new System.Drawing.Size(88, 22);
            this.toolStripButton1.Text = "Save Profile";
            this.toolStripButton1.Click += new System.EventHandler(this.toolStripButton1_Click);
            // 
            // tSBCancel
            // 
            this.tSBCancel.AutoToolTip = false;
            this.tSBCancel.Image = global::ScpServer.Properties.Resources.delete;
            this.tSBCancel.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tSBCancel.Name = "tSBCancel";
            this.tSBCancel.Size = new System.Drawing.Size(63, 22);
            this.tSBCancel.Text = "Cancel";
            this.tSBCancel.Click += new System.EventHandler(this.tSBCancel_Click);
            // 
            // toolStrip1
            // 
            this.toolStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsBNewProfle,
            this.tsBEditProfile,
            this.tsBDeleteProfile,
            this.tSBDupProfile,
            this.tSBImportProfile,
            this.tSBExportProfile});
            this.toolStrip1.Location = new System.Drawing.Point(3, 3);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(780, 25);
            this.toolStrip1.TabIndex = 1;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // tsBNewProfle
            // 
            this.tsBNewProfle.Image = global::ScpServer.Properties.Resources.newprofile;
            this.tsBNewProfle.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsBNewProfle.Name = "tsBNewProfle";
            this.tsBNewProfle.Size = new System.Drawing.Size(51, 22);
            this.tsBNewProfle.Text = "New";
            this.tsBNewProfle.TextAlign = System.Drawing.ContentAlignment.TopLeft;
            this.tsBNewProfle.ToolTipText = "Make New Profile";
            this.tsBNewProfle.Click += new System.EventHandler(this.tsBNewProfile_Click);
            // 
            // tsBEditProfile
            // 
            this.tsBEditProfile.Image = global::ScpServer.Properties.Resources.edit;
            this.tsBEditProfile.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsBEditProfile.Name = "tsBEditProfile";
            this.tsBEditProfile.Size = new System.Drawing.Size(47, 22);
            this.tsBEditProfile.Text = "Edit";
            this.tsBEditProfile.TextAlign = System.Drawing.ContentAlignment.TopLeft;
            this.tsBEditProfile.ToolTipText = "Edit Selected Profile (Enter)";
            this.tsBEditProfile.Click += new System.EventHandler(this.tsBNEditProfile_Click);
            // 
            // tsBDeleteProfile
            // 
            this.tsBDeleteProfile.Image = ((System.Drawing.Image)(resources.GetObject("tsBDeleteProfile.Image")));
            this.tsBDeleteProfile.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsBDeleteProfile.Name = "tsBDeleteProfile";
            this.tsBDeleteProfile.Size = new System.Drawing.Size(60, 22);
            this.tsBDeleteProfile.Text = "Delete";
            this.tsBDeleteProfile.TextAlign = System.Drawing.ContentAlignment.TopLeft;
            this.tsBDeleteProfile.ToolTipText = "Delete Selected Profle (Delete)";
            this.tsBDeleteProfile.Click += new System.EventHandler(this.tsBDeleteProfle_Click);
            // 
            // tSBDupProfile
            // 
            this.tSBDupProfile.Image = ((System.Drawing.Image)(resources.GetObject("tSBDupProfile.Image")));
            this.tSBDupProfile.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tSBDupProfile.Name = "tSBDupProfile";
            this.tSBDupProfile.Size = new System.Drawing.Size(77, 22);
            this.tSBDupProfile.Text = "Duplicate";
            this.tSBDupProfile.TextAlign = System.Drawing.ContentAlignment.TopLeft;
            this.tSBDupProfile.ToolTipText = "Dupliacate Selected Profile (Ctrl+C)";
            this.tSBDupProfile.Click += new System.EventHandler(this.tSBDupProfile_Click);
            // 
            // tSBImportProfile
            // 
            this.tSBImportProfile.Image = global::ScpServer.Properties.Resources.import;
            this.tSBImportProfile.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tSBImportProfile.Name = "tSBImportProfile";
            this.tSBImportProfile.Size = new System.Drawing.Size(63, 22);
            this.tSBImportProfile.Text = "Import";
            this.tSBImportProfile.ToolTipText = "Import Profile or Profiles";
            this.tSBImportProfile.Click += new System.EventHandler(this.tSBImportProfile_Click);
            // 
            // tSBExportProfile
            // 
            this.tSBExportProfile.Image = ((System.Drawing.Image)(resources.GetObject("tSBExportProfile.Image")));
            this.tSBExportProfile.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tSBExportProfile.Name = "tSBExportProfile";
            this.tSBExportProfile.Size = new System.Drawing.Size(60, 22);
            this.tSBExportProfile.Text = "Export";
            this.tSBExportProfile.ToolTipText = "Export Selected Profile";
            this.tSBExportProfile.Click += new System.EventHandler(this.tSBExportProfile_Click);
            // 
            // lBProfiles
            // 
            this.lBProfiles.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lBProfiles.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.lBProfiles.ContextMenuStrip = this.cMProfile;
            this.lBProfiles.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lBProfiles.FormattingEnabled = true;
            this.lBProfiles.ItemHeight = 16;
            this.lBProfiles.Location = new System.Drawing.Point(3, 29);
            this.lBProfiles.MultiColumn = true;
            this.lBProfiles.Name = "lBProfiles";
            this.lBProfiles.Size = new System.Drawing.Size(780, 272);
            this.lBProfiles.TabIndex = 0;
            this.lBProfiles.KeyDown += new System.Windows.Forms.KeyEventHandler(this.lBProfiles_KeyDown);
            this.lBProfiles.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.lBProfiles_MouseDoubleClick);
            this.lBProfiles.MouseDown += new System.Windows.Forms.MouseEventHandler(this.lBProfiles_MouseDown);
            // 
            // cMProfile
            // 
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
            this.cMProfile.Size = new System.Drawing.Size(189, 224);
            // 
            // editToolStripMenuItem
            // 
            this.editToolStripMenuItem.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.editToolStripMenuItem.Name = "editToolStripMenuItem";
            this.editToolStripMenuItem.Size = new System.Drawing.Size(188, 22);
            this.editToolStripMenuItem.Text = "Edit";
            this.editToolStripMenuItem.Click += new System.EventHandler(this.tsBNEditProfile_Click);
            // 
            // assignToController1ToolStripMenuItem
            // 
            this.assignToController1ToolStripMenuItem.Name = "assignToController1ToolStripMenuItem";
            this.assignToController1ToolStripMenuItem.Size = new System.Drawing.Size(188, 22);
            this.assignToController1ToolStripMenuItem.Text = "Assign to Controller 1";
            this.assignToController1ToolStripMenuItem.Click += new System.EventHandler(this.assignToController1ToolStripMenuItem_Click);
            // 
            // assignToController2ToolStripMenuItem
            // 
            this.assignToController2ToolStripMenuItem.Name = "assignToController2ToolStripMenuItem";
            this.assignToController2ToolStripMenuItem.Size = new System.Drawing.Size(188, 22);
            this.assignToController2ToolStripMenuItem.Text = "Assign to Controller 2";
            this.assignToController2ToolStripMenuItem.Click += new System.EventHandler(this.assignToController2ToolStripMenuItem_Click);
            // 
            // assignToController3ToolStripMenuItem
            // 
            this.assignToController3ToolStripMenuItem.Name = "assignToController3ToolStripMenuItem";
            this.assignToController3ToolStripMenuItem.Size = new System.Drawing.Size(188, 22);
            this.assignToController3ToolStripMenuItem.Text = "Assign to Controller 3";
            this.assignToController3ToolStripMenuItem.Click += new System.EventHandler(this.assignToController3ToolStripMenuItem_Click);
            // 
            // assignToController4ToolStripMenuItem
            // 
            this.assignToController4ToolStripMenuItem.Name = "assignToController4ToolStripMenuItem";
            this.assignToController4ToolStripMenuItem.Size = new System.Drawing.Size(188, 22);
            this.assignToController4ToolStripMenuItem.Text = "Assign to Controller 4";
            this.assignToController4ToolStripMenuItem.Click += new System.EventHandler(this.assignToController4ToolStripMenuItem_Click);
            // 
            // deleteToolStripMenuItem
            // 
            this.deleteToolStripMenuItem.Name = "deleteToolStripMenuItem";
            this.deleteToolStripMenuItem.Size = new System.Drawing.Size(188, 22);
            this.deleteToolStripMenuItem.Text = "Delete (Del)";
            this.deleteToolStripMenuItem.Click += new System.EventHandler(this.tsBDeleteProfle_Click);
            // 
            // duplicateToolStripMenuItem
            // 
            this.duplicateToolStripMenuItem.Name = "duplicateToolStripMenuItem";
            this.duplicateToolStripMenuItem.Size = new System.Drawing.Size(188, 22);
            this.duplicateToolStripMenuItem.Text = "Duplicate (Ctrl+C)";
            this.duplicateToolStripMenuItem.Click += new System.EventHandler(this.tSBDupProfile_Click);
            // 
            // newProfileToolStripMenuItem
            // 
            this.newProfileToolStripMenuItem.Name = "newProfileToolStripMenuItem";
            this.newProfileToolStripMenuItem.Size = new System.Drawing.Size(188, 22);
            this.newProfileToolStripMenuItem.Text = "New Profile";
            this.newProfileToolStripMenuItem.Click += new System.EventHandler(this.tsBNewProfile_Click);
            // 
            // importToolStripMenuItem
            // 
            this.importToolStripMenuItem.Name = "importToolStripMenuItem";
            this.importToolStripMenuItem.Size = new System.Drawing.Size(188, 22);
            this.importToolStripMenuItem.Text = "Import";
            this.importToolStripMenuItem.Click += new System.EventHandler(this.tSBImportProfile_Click);
            // 
            // exportToolStripMenuItem
            // 
            this.exportToolStripMenuItem.Name = "exportToolStripMenuItem";
            this.exportToolStripMenuItem.Size = new System.Drawing.Size(188, 22);
            this.exportToolStripMenuItem.Text = "Export";
            this.exportToolStripMenuItem.Click += new System.EventHandler(this.tSBExportProfile_Click);
            // 
            // tabLog
            // 
            this.tabLog.Controls.Add(this.lvDebug);
            this.tabLog.Controls.Add(this.btnClear);
            this.tabLog.Location = new System.Drawing.Point(4, 22);
            this.tabLog.Name = "tabLog";
            this.tabLog.Padding = new System.Windows.Forms.Padding(3);
            this.tabLog.Size = new System.Drawing.Size(786, 313);
            this.tabLog.TabIndex = 1;
            this.tabLog.Text = "Log";
            this.tabLog.UseVisualStyleBackColor = true;
            // 
            // tabAutoProfiles
            // 
            this.tabAutoProfiles.Location = new System.Drawing.Point(4, 22);
            this.tabAutoProfiles.Name = "tabAutoProfiles";
            this.tabAutoProfiles.Size = new System.Drawing.Size(786, 313);
            this.tabAutoProfiles.TabIndex = 2;
            this.tabAutoProfiles.Text = "Auto Profiles";
            this.tabAutoProfiles.UseVisualStyleBackColor = true;
            // 
            // tabSettings
            // 
            this.tabSettings.Controls.Add(this.flowLayoutPanel1);
            this.tabSettings.Location = new System.Drawing.Point(4, 22);
            this.tabSettings.Name = "tabSettings";
            this.tabSettings.Padding = new System.Windows.Forms.Padding(3);
            this.tabSettings.Size = new System.Drawing.Size(786, 313);
            this.tabSettings.TabIndex = 4;
            this.tabSettings.Text = "Settings";
            this.tabSettings.UseVisualStyleBackColor = true;
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.AutoScroll = true;
            this.flowLayoutPanel1.Controls.Add(this.StartWindowsCheckBox);
            this.flowLayoutPanel1.Controls.Add(this.startMinimizedCheckBox);
            this.flowLayoutPanel1.Controls.Add(this.hideDS4CheckBox);
            this.flowLayoutPanel1.Controls.Add(this.cBNotifications);
            this.flowLayoutPanel1.Controls.Add(this.cBUpdate);
            this.flowLayoutPanel1.Controls.Add(this.pNUpdate);
            this.flowLayoutPanel1.Controls.Add(this.linkProfiles);
            this.flowLayoutPanel1.Controls.Add(this.lnkControllers);
            this.flowLayoutPanel1.Controls.Add(this.lLBUpdate);
            this.flowLayoutPanel1.Controls.Add(this.linkUninstall);
            this.flowLayoutPanel1.Controls.Add(this.lLSetup);
            this.flowLayoutPanel1.Controls.Add(this.lLAppDataDelete);
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(3, 3);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(780, 307);
            this.flowLayoutPanel1.TabIndex = 0;
            // 
            // cBNotifications
            // 
            this.cBNotifications.AutoSize = true;
            this.cBNotifications.Location = new System.Drawing.Point(332, 3);
            this.cBNotifications.Name = "cBNotifications";
            this.cBNotifications.Size = new System.Drawing.Size(114, 17);
            this.cBNotifications.TabIndex = 51;
            this.cBNotifications.Text = "Show Notifications";
            this.cBNotifications.UseVisualStyleBackColor = true;
            this.cBNotifications.CheckedChanged += new System.EventHandler(this.cBNotifications_CheckedChanged);
            // 
            // cBUpdate
            // 
            this.cBUpdate.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.cBUpdate.AutoSize = true;
            this.cBUpdate.Location = new System.Drawing.Point(452, 3);
            this.cBUpdate.Name = "cBUpdate";
            this.cBUpdate.Size = new System.Drawing.Size(164, 17);
            this.cBUpdate.TabIndex = 41;
            this.cBUpdate.Text = "Check for Updates at Startup";
            this.cBUpdate.UseVisualStyleBackColor = true;
            this.cBUpdate.CheckedChanged += new System.EventHandler(this.cBUpdate_CheckedChanged);
            // 
            // pNUpdate
            // 
            this.pNUpdate.Controls.Add(this.cBUpdateTime);
            this.pNUpdate.Controls.Add(this.lBCheckEvery);
            this.pNUpdate.Controls.Add(this.nUDUpdateTime);
            this.pNUpdate.Enabled = false;
            this.pNUpdate.Location = new System.Drawing.Point(3, 26);
            this.pNUpdate.Name = "pNUpdate";
            this.pNUpdate.Size = new System.Drawing.Size(189, 22);
            this.pNUpdate.TabIndex = 43;
            // 
            // cBUpdateTime
            // 
            this.cBUpdateTime.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cBUpdateTime.FormattingEnabled = true;
            this.cBUpdateTime.Items.AddRange(new object[] {
            "hours",
            "days"});
            this.cBUpdateTime.Location = new System.Drawing.Point(126, 0);
            this.cBUpdateTime.Name = "cBUpdateTime";
            this.cBUpdateTime.Size = new System.Drawing.Size(60, 21);
            this.cBUpdateTime.TabIndex = 43;
            this.cBUpdateTime.SelectedIndexChanged += new System.EventHandler(this.cBUpdateTime_SelectedIndexChanged);
            // 
            // lBCheckEvery
            // 
            this.lBCheckEvery.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lBCheckEvery.AutoSize = true;
            this.lBCheckEvery.Location = new System.Drawing.Point(3, 3);
            this.lBCheckEvery.Name = "lBCheckEvery";
            this.lBCheckEvery.Size = new System.Drawing.Size(67, 13);
            this.lBCheckEvery.TabIndex = 0;
            this.lBCheckEvery.Text = "Check every";
            // 
            // nUDUpdateTime
            // 
            this.nUDUpdateTime.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.nUDUpdateTime.Location = new System.Drawing.Point(76, 1);
            this.nUDUpdateTime.Maximum = new decimal(new int[] {
            24,
            0,
            0,
            0});
            this.nUDUpdateTime.Name = "nUDUpdateTime";
            this.nUDUpdateTime.Size = new System.Drawing.Size(44, 20);
            this.nUDUpdateTime.TabIndex = 42;
            this.nUDUpdateTime.ValueChanged += new System.EventHandler(this.nUDUpdateTime_ValueChanged);
            // 
            // linkProfiles
            // 
            this.linkProfiles.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.linkProfiles.AutoSize = true;
            this.linkProfiles.Location = new System.Drawing.Point(198, 30);
            this.linkProfiles.Name = "linkProfiles";
            this.linkProfiles.Size = new System.Drawing.Size(65, 13);
            this.linkProfiles.TabIndex = 50;
            this.linkProfiles.TabStop = true;
            this.linkProfiles.Text = "Profile folder";
            this.linkProfiles.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkProfiles_LinkClicked);
            // 
            // lLBUpdate
            // 
            this.lLBUpdate.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.lLBUpdate.Location = new System.Drawing.Point(348, 25);
            this.lLBUpdate.Name = "lLBUpdate";
            this.lLBUpdate.Size = new System.Drawing.Size(116, 23);
            this.lLBUpdate.TabIndex = 49;
            this.lLBUpdate.TabStop = true;
            this.lLBUpdate.Text = "Check for Update Now";
            this.lLBUpdate.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lLBUpdate.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lLBUpdate_LinkClicked);
            // 
            // linkUninstall
            // 
            this.linkUninstall.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.linkUninstall.Location = new System.Drawing.Point(470, 25);
            this.linkUninstall.Name = "linkUninstall";
            this.linkUninstall.Size = new System.Drawing.Size(106, 23);
            this.linkUninstall.TabIndex = 44;
            this.linkUninstall.TabStop = true;
            this.linkUninstall.Text = "Uninstall VBus Driver";
            this.linkUninstall.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.linkUninstall.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkUninstall_LinkClicked);
            // 
            // lLSetup
            // 
            this.lLSetup.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.lLSetup.AutoSize = true;
            this.lLSetup.Location = new System.Drawing.Point(582, 30);
            this.lLSetup.Name = "lLSetup";
            this.lLSetup.Size = new System.Drawing.Size(82, 13);
            this.lLSetup.TabIndex = 52;
            this.lLSetup.TabStop = true;
            this.lLSetup.Text = "Controller Setup";
            this.lLSetup.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lLSetup_LinkClicked);
            // 
            // saveProfiles
            // 
            this.saveProfiles.Filter = "XML Files (*.xml)|*.xml";
            // 
            // lLAppDataDelete
            // 
            this.lLAppDataDelete.AutoSize = true;
            this.lLAppDataDelete.Location = new System.Drawing.Point(3, 51);
            this.lLAppDataDelete.Name = "lLAppDataDelete";
            this.lLAppDataDelete.Size = new System.Drawing.Size(127, 13);
            this.lLAppDataDelete.TabIndex = 53;
            this.lLAppDataDelete.TabStop = true;
            this.lLAppDataDelete.Text = "Delete Old Settings folder";
            this.lLAppDataDelete.Visible = false;
            this.lLAppDataDelete.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lLAppDataDelete_LinkClicked);
            // 
            // ScpForm
            // 
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(794, 369);
            this.Controls.Add(this.tabMain);
            this.Controls.Add(this.pnlButton);
            this.MinimumSize = new System.Drawing.Size(420, 231);
            this.Name = "ScpForm";
            this.Text = "DS4Windows";
            this.Activated += new System.EventHandler(this.ScpForm_Activated);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form_Close);
            this.Load += new System.EventHandler(this.Form_Load);
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
            this.tSOptions.ResumeLayout(false);
            this.tSOptions.PerformLayout();
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.cMProfile.ResumeLayout(false);
            this.tabLog.ResumeLayout(false);
            this.tabSettings.ResumeLayout(false);
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
            this.pNUpdate.ResumeLayout(false);
            this.pNUpdate.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nUDUpdateTime)).EndInit();
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
        private System.Windows.Forms.CheckBox hideDS4CheckBox;
        private System.Windows.Forms.CheckBox startMinimizedCheckBox;
        private System.Windows.Forms.Label lbLastMessage;
        private System.Windows.Forms.LinkLabel lnkControllers;
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
        private System.Windows.Forms.CheckBox StartWindowsCheckBox;
        private System.Windows.Forms.Label lBTest;
        private System.Windows.Forms.TabControl tabMain;
        private System.Windows.Forms.TabPage tabProfiles;
        private System.Windows.Forms.TabPage tabLog;
        private System.Windows.Forms.ListBox lBProfiles;
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
        private System.Windows.Forms.Label lBSelectedProfile;
        private System.Windows.Forms.Label lBID;
        private System.Windows.Forms.Label lBStatus;
        private System.Windows.Forms.Label lBBattery;
        private System.Windows.Forms.Label lBBatt1;
        private System.Windows.Forms.Label lBBatt2;
        private System.Windows.Forms.Label lBBatt3;
        private System.Windows.Forms.Label lBBatt4;
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
        private System.Windows.Forms.ToolStripButton toolStripButton1;
        private System.Windows.Forms.ToolStripButton tSBCancel;
        private System.Windows.Forms.TabPage tabSettings;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.CheckBox cBUpdate;
        private System.Windows.Forms.NumericUpDown nUDUpdateTime;
        private System.Windows.Forms.Panel pNUpdate;
        private System.Windows.Forms.Label lBCheckEvery;
        private System.Windows.Forms.ComboBox cBUpdateTime;
        private System.Windows.Forms.LinkLabel lLBUpdate;
        private System.Windows.Forms.LinkLabel linkUninstall;
        private System.Windows.Forms.LinkLabel linkProfiles;
        private System.Windows.Forms.CheckBox cBNotifications;
        private System.Windows.Forms.LinkLabel lLSetup;
        private System.Windows.Forms.LinkLabel lLAppDataDelete;
        //private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem2;
    }
}

