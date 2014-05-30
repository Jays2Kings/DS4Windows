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
            this.label1 = new System.Windows.Forms.Label();
            this.btnImportProfiles = new System.Windows.Forms.Button();
            this.llbHelp = new System.Windows.Forms.LinkLabel();
            this.btnStartStop = new System.Windows.Forms.Button();
            this.btnClear = new System.Windows.Forms.Button();
            this.lnkControllers = new System.Windows.Forms.LinkLabel();
            this.StartWindowsCheckBox = new System.Windows.Forms.CheckBox();
            this.lbLastMessage = new System.Windows.Forms.Label();
            this.startMinimizedCheckBox = new System.Windows.Forms.CheckBox();
            this.hideDS4CheckBox = new System.Windows.Forms.CheckBox();
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
            this.tabProfiles = new System.Windows.Forms.TabPage();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.tsBNewProfle = new System.Windows.Forms.ToolStripButton();
            this.tsBEditProfile = new System.Windows.Forms.ToolStripButton();
            this.tsBDeleteProfile = new System.Windows.Forms.ToolStripButton();
            this.tSBDupProfile = new System.Windows.Forms.ToolStripButton();
            this.lBProfiles = new System.Windows.Forms.ListBox();
            this.tabLog = new System.Windows.Forms.TabPage();
            this.tabAutoProfiles = new System.Windows.Forms.TabPage();
            this.pBStatus4 = new System.Windows.Forms.PictureBox();
            this.pBStatus3 = new System.Windows.Forms.PictureBox();
            this.pBStatus2 = new System.Windows.Forms.PictureBox();
            this.lBBatt4 = new System.Windows.Forms.Label();
            this.lBBatt3 = new System.Windows.Forms.Label();
            this.lBBatt2 = new System.Windows.Forms.Label();
            this.lBBatt1 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.cBController4 = new System.Windows.Forms.ComboBox();
            this.bnEditC1 = new System.Windows.Forms.Button();
            this.cBController3 = new System.Windows.Forms.ComboBox();
            this.cBController2 = new System.Windows.Forms.ComboBox();
            this.bnEditC2 = new System.Windows.Forms.Button();
            this.cBController1 = new System.Windows.Forms.ComboBox();
            this.lbPad4 = new System.Windows.Forms.Label();
            this.lbPad3 = new System.Windows.Forms.Label();
            this.bnEditC4 = new System.Windows.Forms.Button();
            this.bnEditC3 = new System.Windows.Forms.Button();
            this.lbPad2 = new System.Windows.Forms.Label();
            this.lbPad1 = new System.Windows.Forms.Label();
            this.pBStatus1 = new System.Windows.Forms.PictureBox();
            this.tLPControllers = new System.Windows.Forms.TableLayoutPanel();
            this.pnlButton.SuspendLayout();
            this.cMTaskbar.SuspendLayout();
            this.tabMain.SuspendLayout();
            this.tabControllers.SuspendLayout();
            this.tabProfiles.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            this.tabLog.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pBStatus4)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pBStatus3)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pBStatus2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pBStatus1)).BeginInit();
            this.tLPControllers.SuspendLayout();
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
            this.lvDebug.Size = new System.Drawing.Size(780, 281);
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
            this.pnlButton.Controls.Add(this.label1);
            this.pnlButton.Controls.Add(this.btnImportProfiles);
            this.pnlButton.Controls.Add(this.llbHelp);
            this.pnlButton.Controls.Add(this.btnStartStop);
            this.pnlButton.Controls.Add(this.btnClear);
            this.pnlButton.Controls.Add(this.lnkControllers);
            this.pnlButton.Controls.Add(this.StartWindowsCheckBox);
            this.pnlButton.Controls.Add(this.lbLastMessage);
            this.pnlButton.Controls.Add(this.startMinimizedCheckBox);
            this.pnlButton.Controls.Add(this.hideDS4CheckBox);
            this.pnlButton.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlButton.Location = new System.Drawing.Point(0, 313);
            this.pnlButton.Name = "pnlButton";
            this.pnlButton.Size = new System.Drawing.Size(794, 80);
            this.pnlButton.TabIndex = 10;
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(290, 58);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(98, 13);
            this.label1.TabIndex = 46;
            this.label1.Text = "Used to test values";
            this.label1.Visible = false;
            // 
            // btnImportProfiles
            // 
            this.btnImportProfiles.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnImportProfiles.Location = new System.Drawing.Point(7, 51);
            this.btnImportProfiles.Name = "btnImportProfiles";
            this.btnImportProfiles.Size = new System.Drawing.Size(87, 23);
            this.btnImportProfiles.TabIndex = 14;
            this.btnImportProfiles.Text = "Import Profile(s)";
            this.btnImportProfiles.UseVisualStyleBackColor = true;
            this.btnImportProfiles.Click += new System.EventHandler(this.btnImportProfiles_Click);
            // 
            // llbHelp
            // 
            this.llbHelp.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.llbHelp.AutoSize = true;
            this.llbHelp.Location = new System.Drawing.Point(641, 8);
            this.llbHelp.Name = "llbHelp";
            this.llbHelp.Size = new System.Drawing.Size(79, 13);
            this.llbHelp.TabIndex = 13;
            this.llbHelp.TabStop = true;
            this.llbHelp.Text = "Hotkeys/About";
            this.llbHelp.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.llbHelp_LinkClicked);
            // 
            // btnStartStop
            // 
            this.btnStartStop.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnStartStop.Location = new System.Drawing.Point(666, 51);
            this.btnStartStop.Name = "btnStartStop";
            this.btnStartStop.Size = new System.Drawing.Size(119, 23);
            this.btnStartStop.TabIndex = 1;
            this.btnStartStop.Text = "Start";
            this.btnStartStop.UseVisualStyleBackColor = true;
            this.btnStartStop.Click += new System.EventHandler(this.btnStartStop_Click);
            // 
            // btnClear
            // 
            this.btnClear.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClear.Enabled = false;
            this.btnClear.Location = new System.Drawing.Point(563, 51);
            this.btnClear.Name = "btnClear";
            this.btnClear.Size = new System.Drawing.Size(97, 23);
            this.btnClear.TabIndex = 9;
            this.btnClear.Text = "Clear";
            this.btnClear.UseVisualStyleBackColor = true;
            this.btnClear.Click += new System.EventHandler(this.btnClear_Click);
            // 
            // lnkControllers
            // 
            this.lnkControllers.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.lnkControllers.AutoSize = true;
            this.lnkControllers.Location = new System.Drawing.Point(726, 8);
            this.lnkControllers.Name = "lnkControllers";
            this.lnkControllers.Size = new System.Drawing.Size(56, 13);
            this.lnkControllers.TabIndex = 11;
            this.lnkControllers.TabStop = true;
            this.lnkControllers.Text = "Controllers";
            this.lnkControllers.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkControllers_LinkClicked);
            // 
            // StartWindowsCheckBox
            // 
            this.StartWindowsCheckBox.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.StartWindowsCheckBox.AutoSize = true;
            this.StartWindowsCheckBox.Location = new System.Drawing.Point(228, 28);
            this.StartWindowsCheckBox.Name = "StartWindowsCheckBox";
            this.StartWindowsCheckBox.Size = new System.Drawing.Size(117, 17);
            this.StartWindowsCheckBox.TabIndex = 40;
            this.StartWindowsCheckBox.Text = "Start with Windows";
            this.StartWindowsCheckBox.UseVisualStyleBackColor = true;
            this.StartWindowsCheckBox.CheckedChanged += new System.EventHandler(this.StartWindowsCheckBox_CheckedChanged);
            // 
            // lbLastMessage
            // 
            this.lbLastMessage.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lbLastMessage.ForeColor = System.Drawing.SystemColors.GrayText;
            this.lbLastMessage.Location = new System.Drawing.Point(7, 8);
            this.lbLastMessage.Name = "lbLastMessage";
            this.lbLastMessage.Size = new System.Drawing.Size(628, 17);
            this.lbLastMessage.TabIndex = 41;
            // 
            // startMinimizedCheckBox
            // 
            this.startMinimizedCheckBox.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.startMinimizedCheckBox.AutoSize = true;
            this.startMinimizedCheckBox.Location = new System.Drawing.Point(348, 28);
            this.startMinimizedCheckBox.Name = "startMinimizedCheckBox";
            this.startMinimizedCheckBox.Size = new System.Drawing.Size(97, 17);
            this.startMinimizedCheckBox.TabIndex = 40;
            this.startMinimizedCheckBox.Text = "Start Minimized";
            this.startMinimizedCheckBox.UseVisualStyleBackColor = true;
            this.startMinimizedCheckBox.CheckedChanged += new System.EventHandler(this.startMinimizedCheckBox_CheckedChanged);
            // 
            // hideDS4CheckBox
            // 
            this.hideDS4CheckBox.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.hideDS4CheckBox.AutoSize = true;
            this.hideDS4CheckBox.Location = new System.Drawing.Point(451, 28);
            this.hideDS4CheckBox.Name = "hideDS4CheckBox";
            this.hideDS4CheckBox.Size = new System.Drawing.Size(119, 17);
            this.hideDS4CheckBox.TabIndex = 13;
            this.hideDS4CheckBox.Text = "Hide DS4 Controller";
            this.hideDS4CheckBox.UseVisualStyleBackColor = true;
            this.hideDS4CheckBox.CheckedChanged += new System.EventHandler(this.hideDS4CheckBox_CheckedChanged);
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
            this.tabMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabMain.Location = new System.Drawing.Point(0, 0);
            this.tabMain.Name = "tabMain";
            this.tabMain.SelectedIndex = 0;
            this.tabMain.Size = new System.Drawing.Size(794, 313);
            this.tabMain.TabIndex = 12;
            this.tabMain.SelectedIndexChanged += new System.EventHandler(this.tabControl1_SelectedIndexChanged);
            // 
            // tabControllers
            // 
            this.tabControllers.Controls.Add(this.tLPControllers);
            this.tabControllers.Location = new System.Drawing.Point(4, 22);
            this.tabControllers.Name = "tabControllers";
            this.tabControllers.Size = new System.Drawing.Size(786, 287);
            this.tabControllers.TabIndex = 3;
            this.tabControllers.Text = "Controllers";
            this.tabControllers.UseVisualStyleBackColor = true;
            // 
            // tabProfiles
            // 
            this.tabProfiles.Controls.Add(this.toolStrip1);
            this.tabProfiles.Controls.Add(this.lBProfiles);
            this.tabProfiles.Location = new System.Drawing.Point(4, 22);
            this.tabProfiles.Name = "tabProfiles";
            this.tabProfiles.Padding = new System.Windows.Forms.Padding(3);
            this.tabProfiles.Size = new System.Drawing.Size(786, 287);
            this.tabProfiles.TabIndex = 0;
            this.tabProfiles.Text = "Profiles";
            this.tabProfiles.UseVisualStyleBackColor = true;
            // 
            // toolStrip1
            // 
            this.toolStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsBNewProfle,
            this.tsBEditProfile,
            this.tsBDeleteProfile,
            this.tSBDupProfile});
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
            this.tsBNewProfle.Size = new System.Drawing.Size(88, 22);
            this.tsBNewProfle.Text = "New Profile";
            this.tsBNewProfle.TextAlign = System.Drawing.ContentAlignment.TopLeft;
            this.tsBNewProfle.Click += new System.EventHandler(this.tsBNewProfile_Click);
            // 
            // tsBEditProfile
            // 
            this.tsBEditProfile.Image = global::ScpServer.Properties.Resources.edit;
            this.tsBEditProfile.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsBEditProfile.Name = "tsBEditProfile";
            this.tsBEditProfile.Size = new System.Drawing.Size(84, 22);
            this.tsBEditProfile.Text = "Edit Profile";
            this.tsBEditProfile.TextAlign = System.Drawing.ContentAlignment.TopLeft;
            this.tsBEditProfile.Click += new System.EventHandler(this.tsBNEditProfile_Click);
            // 
            // tsBDeleteProfile
            // 
            this.tsBDeleteProfile.Image = ((System.Drawing.Image)(resources.GetObject("tsBDeleteProfile.Image")));
            this.tsBDeleteProfile.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsBDeleteProfile.Name = "tsBDeleteProfile";
            this.tsBDeleteProfile.Size = new System.Drawing.Size(97, 22);
            this.tsBDeleteProfile.Text = "Delete Profile";
            this.tsBDeleteProfile.TextAlign = System.Drawing.ContentAlignment.TopLeft;
            this.tsBDeleteProfile.Click += new System.EventHandler(this.tsBDeleteProfle_Click);
            // 
            // tSBDupProfile
            // 
            this.tSBDupProfile.Image = ((System.Drawing.Image)(resources.GetObject("tSBDupProfile.Image")));
            this.tSBDupProfile.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tSBDupProfile.Name = "tSBDupProfile";
            this.tSBDupProfile.Size = new System.Drawing.Size(120, 22);
            this.tSBDupProfile.Text = "Dupliacate Profile";
            this.tSBDupProfile.TextAlign = System.Drawing.ContentAlignment.TopLeft;
            this.tSBDupProfile.Click += new System.EventHandler(this.tSBDupProfile_Click);
            // 
            // lBProfiles
            // 
            this.lBProfiles.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lBProfiles.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.lBProfiles.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lBProfiles.FormattingEnabled = true;
            this.lBProfiles.ItemHeight = 16;
            this.lBProfiles.Location = new System.Drawing.Point(3, 29);
            this.lBProfiles.MultiColumn = true;
            this.lBProfiles.Name = "lBProfiles";
            this.lBProfiles.Size = new System.Drawing.Size(780, 256);
            this.lBProfiles.TabIndex = 0;
            this.lBProfiles.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.lBProfiles_MouseDoubleClick);
            // 
            // tabLog
            // 
            this.tabLog.Controls.Add(this.lvDebug);
            this.tabLog.Location = new System.Drawing.Point(4, 22);
            this.tabLog.Name = "tabLog";
            this.tabLog.Padding = new System.Windows.Forms.Padding(3);
            this.tabLog.Size = new System.Drawing.Size(786, 287);
            this.tabLog.TabIndex = 1;
            this.tabLog.Text = "Log";
            this.tabLog.UseVisualStyleBackColor = true;
            // 
            // tabAutoProfiles
            // 
            this.tabAutoProfiles.Location = new System.Drawing.Point(4, 22);
            this.tabAutoProfiles.Name = "tabAutoProfiles";
            this.tabAutoProfiles.Size = new System.Drawing.Size(786, 287);
            this.tabAutoProfiles.TabIndex = 2;
            this.tabAutoProfiles.Text = "Auto Profiles (Alpha)";
            this.tabAutoProfiles.UseVisualStyleBackColor = true;
            // 
            // pBStatus4
            // 
            this.pBStatus4.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.pBStatus4.Image = ((System.Drawing.Image)(resources.GetObject("pBStatus4.Image")));
            this.pBStatus4.InitialImage = global::ScpServer.Properties.Resources.BT;
            this.pBStatus4.Location = new System.Drawing.Point(370, 106);
            this.pBStatus4.Name = "pBStatus4";
            this.pBStatus4.Size = new System.Drawing.Size(39, 20);
            this.pBStatus4.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.pBStatus4.TabIndex = 47;
            this.pBStatus4.TabStop = false;
            // 
            // pBStatus3
            // 
            this.pBStatus3.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.pBStatus3.Image = ((System.Drawing.Image)(resources.GetObject("pBStatus3.Image")));
            this.pBStatus3.InitialImage = global::ScpServer.Properties.Resources.BT;
            this.pBStatus3.Location = new System.Drawing.Point(370, 77);
            this.pBStatus3.Name = "pBStatus3";
            this.pBStatus3.Size = new System.Drawing.Size(39, 20);
            this.pBStatus3.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.pBStatus3.TabIndex = 47;
            this.pBStatus3.TabStop = false;
            // 
            // pBStatus2
            // 
            this.pBStatus2.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.pBStatus2.Image = ((System.Drawing.Image)(resources.GetObject("pBStatus2.Image")));
            this.pBStatus2.InitialImage = global::ScpServer.Properties.Resources.BT;
            this.pBStatus2.Location = new System.Drawing.Point(370, 48);
            this.pBStatus2.Name = "pBStatus2";
            this.pBStatus2.Size = new System.Drawing.Size(39, 20);
            this.pBStatus2.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.pBStatus2.TabIndex = 47;
            this.pBStatus2.TabStop = false;
            // 
            // lBBatt4
            // 
            this.lBBatt4.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.lBBatt4.AutoSize = true;
            this.lBBatt4.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lBBatt4.Location = new System.Drawing.Point(521, 109);
            this.lBBatt4.Name = "lBBatt4";
            this.lBBatt4.Size = new System.Drawing.Size(39, 15);
            this.lBBatt4.TabIndex = 44;
            this.lBBatt4.Text = "100%";
            // 
            // lBBatt3
            // 
            this.lBBatt3.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.lBBatt3.AutoSize = true;
            this.lBBatt3.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lBBatt3.Location = new System.Drawing.Point(521, 80);
            this.lBBatt3.Name = "lBBatt3";
            this.lBBatt3.Size = new System.Drawing.Size(39, 15);
            this.lBBatt3.TabIndex = 44;
            this.lBBatt3.Text = "100%";
            // 
            // lBBatt2
            // 
            this.lBBatt2.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.lBBatt2.AutoSize = true;
            this.lBBatt2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lBBatt2.Location = new System.Drawing.Point(521, 51);
            this.lBBatt2.Name = "lBBatt2";
            this.lBBatt2.Size = new System.Drawing.Size(39, 15);
            this.lBBatt2.TabIndex = 44;
            this.lBBatt2.Text = "100%";
            // 
            // lBBatt1
            // 
            this.lBBatt1.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.lBBatt1.AutoSize = true;
            this.lBBatt1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lBBatt1.Location = new System.Drawing.Point(521, 22);
            this.lBBatt1.Name = "lBBatt1";
            this.lBBatt1.Size = new System.Drawing.Size(39, 15);
            this.lBBatt1.TabIndex = 44;
            this.lBBatt1.Text = "100%";
            // 
            // label5
            // 
            this.label5.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.Location = new System.Drawing.Point(515, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(51, 15);
            this.label5.TabIndex = 45;
            this.label5.Text = "Battery";
            // 
            // label4
            // 
            this.label4.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(366, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(47, 15);
            this.label4.TabIndex = 45;
            this.label4.Text = "Status";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(3, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(21, 15);
            this.label3.TabIndex = 45;
            this.label3.Text = "ID";
            // 
            // label2
            // 
            this.label2.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(624, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(109, 15);
            this.label2.TabIndex = 45;
            this.label2.Text = "Selected Profile";
            // 
            // cBController4
            // 
            this.cBController4.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.cBController4.FormattingEnabled = true;
            this.cBController4.Location = new System.Drawing.Point(622, 106);
            this.cBController4.Name = "cBController4";
            this.cBController4.Size = new System.Drawing.Size(114, 21);
            this.cBController4.TabIndex = 42;
            this.cBController4.Tag = "3";
            this.cBController4.SelectedValueChanged += new System.EventHandler(this.Profile_Changed);
            // 
            // bnEditC1
            // 
            this.bnEditC1.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.bnEditC1.Location = new System.Drawing.Point(742, 18);
            this.bnEditC1.Name = "bnEditC1";
            this.bnEditC1.Size = new System.Drawing.Size(41, 23);
            this.bnEditC1.TabIndex = 43;
            this.bnEditC1.Tag = "0";
            this.bnEditC1.Text = "Edit";
            this.bnEditC1.UseVisualStyleBackColor = true;
            this.bnEditC1.Click += new System.EventHandler(this.editButtons_Click);
            // 
            // cBController3
            // 
            this.cBController3.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.cBController3.FormattingEnabled = true;
            this.cBController3.Location = new System.Drawing.Point(622, 77);
            this.cBController3.Name = "cBController3";
            this.cBController3.Size = new System.Drawing.Size(114, 21);
            this.cBController3.TabIndex = 42;
            this.cBController3.Tag = "2";
            this.cBController3.SelectedValueChanged += new System.EventHandler(this.Profile_Changed);
            // 
            // cBController2
            // 
            this.cBController2.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.cBController2.FormattingEnabled = true;
            this.cBController2.Location = new System.Drawing.Point(622, 48);
            this.cBController2.Name = "cBController2";
            this.cBController2.Size = new System.Drawing.Size(114, 21);
            this.cBController2.TabIndex = 42;
            this.cBController2.Tag = "1";
            this.cBController2.SelectedValueChanged += new System.EventHandler(this.Profile_Changed);
            // 
            // bnEditC2
            // 
            this.bnEditC2.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.bnEditC2.Location = new System.Drawing.Point(742, 47);
            this.bnEditC2.Name = "bnEditC2";
            this.bnEditC2.Size = new System.Drawing.Size(41, 23);
            this.bnEditC2.TabIndex = 43;
            this.bnEditC2.Tag = "1";
            this.bnEditC2.Text = "Edit";
            this.bnEditC2.UseVisualStyleBackColor = true;
            this.bnEditC2.Click += new System.EventHandler(this.editButtons_Click);
            // 
            // cBController1
            // 
            this.cBController1.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.cBController1.FormattingEnabled = true;
            this.cBController1.Location = new System.Drawing.Point(622, 19);
            this.cBController1.Name = "cBController1";
            this.cBController1.Size = new System.Drawing.Size(114, 21);
            this.cBController1.TabIndex = 42;
            this.cBController1.Tag = "0";
            this.cBController1.SelectedValueChanged += new System.EventHandler(this.Profile_Changed);
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
            // bnEditC4
            // 
            this.bnEditC4.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.bnEditC4.Location = new System.Drawing.Point(742, 105);
            this.bnEditC4.Name = "bnEditC4";
            this.bnEditC4.Size = new System.Drawing.Size(41, 23);
            this.bnEditC4.TabIndex = 43;
            this.bnEditC4.Tag = "3";
            this.bnEditC4.Text = "Edit";
            this.bnEditC4.UseVisualStyleBackColor = true;
            this.bnEditC4.Click += new System.EventHandler(this.editButtons_Click);
            // 
            // bnEditC3
            // 
            this.bnEditC3.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.bnEditC3.Location = new System.Drawing.Point(742, 76);
            this.bnEditC3.Name = "bnEditC3";
            this.bnEditC3.Size = new System.Drawing.Size(41, 23);
            this.bnEditC3.TabIndex = 43;
            this.bnEditC3.Tag = "2";
            this.bnEditC3.Text = "Edit";
            this.bnEditC3.UseVisualStyleBackColor = true;
            this.bnEditC3.Click += new System.EventHandler(this.editButtons_Click);
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
            // pBStatus1
            // 
            this.pBStatus1.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.pBStatus1.Image = ((System.Drawing.Image)(resources.GetObject("pBStatus1.Image")));
            this.pBStatus1.InitialImage = global::ScpServer.Properties.Resources.BT;
            this.pBStatus1.Location = new System.Drawing.Point(370, 19);
            this.pBStatus1.Name = "pBStatus1";
            this.pBStatus1.Size = new System.Drawing.Size(39, 20);
            this.pBStatus1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.pBStatus1.TabIndex = 47;
            this.pBStatus1.TabStop = false;
            // 
            // tLPControllers
            // 
            this.tLPControllers.ColumnCount = 5;
            this.tLPControllers.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 51.34884F));
            this.tLPControllers.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 23.34039F));
            this.tLPControllers.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25.31077F));
            this.tLPControllers.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 120F));
            this.tLPControllers.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 45F));
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
            this.tLPControllers.Controls.Add(this.label2, 3, 0);
            this.tLPControllers.Controls.Add(this.label3, 0, 0);
            this.tLPControllers.Controls.Add(this.label4, 1, 0);
            this.tLPControllers.Controls.Add(this.label5, 2, 0);
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
            // ScpForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(794, 393);
            this.Controls.Add(this.tabMain);
            this.Controls.Add(this.pnlButton);
            this.MinimumSize = new System.Drawing.Size(420, 190);
            this.Name = "ScpForm";
            this.Text = "DS4Windows 1.0 Beta J2K Build";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form_Close);
            this.Load += new System.EventHandler(this.Form_Load);
            this.Resize += new System.EventHandler(this.Form_Resize);
            this.pnlButton.ResumeLayout(false);
            this.pnlButton.PerformLayout();
            this.cMTaskbar.ResumeLayout(false);
            this.tabMain.ResumeLayout(false);
            this.tabControllers.ResumeLayout(false);
            this.tabProfiles.ResumeLayout(false);
            this.tabProfiles.PerformLayout();
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.tabLog.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pBStatus4)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pBStatus3)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pBStatus2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pBStatus1)).EndInit();
            this.tLPControllers.ResumeLayout(false);
            this.tLPControllers.PerformLayout();
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
        private System.Windows.Forms.Button btnImportProfiles;
        private System.Windows.Forms.CheckBox StartWindowsCheckBox;
        private System.Windows.Forms.Label label1;
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
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label lBBatt1;
        private System.Windows.Forms.Label lBBatt2;
        private System.Windows.Forms.Label lBBatt3;
        private System.Windows.Forms.Label lBBatt4;
        private System.Windows.Forms.PictureBox pBStatus2;
        private System.Windows.Forms.PictureBox pBStatus3;
        private System.Windows.Forms.PictureBox pBStatus4;
        //private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem2;
    }
}

