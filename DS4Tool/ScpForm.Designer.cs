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
            this.AboutButton = new System.Windows.Forms.Button();
            this.btnStartStop = new System.Windows.Forms.Button();
            this.btnClear = new System.Windows.Forms.Button();
            this.btnStop = new System.Windows.Forms.Button();
            this.hotkeysButton = new System.Windows.Forms.Button();
            this.lnkControllers = new System.Windows.Forms.LinkLabel();
            this.hideDS4CheckBox = new System.Windows.Forms.CheckBox();
            this.startMinimizedCheckBox = new System.Windows.Forms.CheckBox();
            this.pnlDebug = new System.Windows.Forms.Panel();
            this.pnlStatus = new System.Windows.Forms.Panel();
            this.gpPads = new System.Windows.Forms.GroupBox();
            this.lbSelPro4 = new System.Windows.Forms.Label();
            this.lbSelPro3 = new System.Windows.Forms.Label();
            this.lbSelPro2 = new System.Windows.Forms.Label();
            this.lbSelPro1 = new System.Windows.Forms.Label();
            this.lbPad4 = new System.Windows.Forms.Label();
            this.lbPad3 = new System.Windows.Forms.Label();
            this.lbPad2 = new System.Windows.Forms.Label();
            this.lbPad1 = new System.Windows.Forms.Label();
            this.bnEditC4 = new System.Windows.Forms.Button();
            this.bnEditC3 = new System.Windows.Forms.Button();
            this.bnEditC2 = new System.Windows.Forms.Button();
            this.bnDeleteC4 = new System.Windows.Forms.Button();
            this.bnDeleteC3 = new System.Windows.Forms.Button();
            this.bnDeleteC2 = new System.Windows.Forms.Button();
            this.bnDeleteC1 = new System.Windows.Forms.Button();
            this.bnEditC1 = new System.Windows.Forms.Button();
            this.cBController4 = new System.Windows.Forms.ComboBox();
            this.cBController3 = new System.Windows.Forms.ComboBox();
            this.cBController2 = new System.Windows.Forms.ComboBox();
            this.cBController1 = new System.Windows.Forms.ComboBox();
            this.lbLastMessage = new System.Windows.Forms.Label();
            this.notifyIcon1 = new System.Windows.Forms.NotifyIcon(this.components);
            this.cMTaskbar = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.editProfileForController1ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.editProfileForController2ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.editProfileForController3ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.editProfileForController4ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.pnlButton.SuspendLayout();
            this.pnlDebug.SuspendLayout();
            this.pnlStatus.SuspendLayout();
            this.gpPads.SuspendLayout();
            this.cMTaskbar.SuspendLayout();
            this.SuspendLayout();
            // 
            // lvDebug
            // 
            this.lvDebug.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lvDebug.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.chTime,
            this.chData});
            this.lvDebug.Font = new System.Drawing.Font("Lucida Console", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lvDebug.FullRowSelect = true;
            this.lvDebug.Location = new System.Drawing.Point(0, 0);
            this.lvDebug.Name = "lvDebug";
            this.lvDebug.Size = new System.Drawing.Size(797, 353);
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
            this.pnlButton.Controls.Add(this.AboutButton);
            this.pnlButton.Controls.Add(this.btnStartStop);
            this.pnlButton.Controls.Add(this.btnClear);
            this.pnlButton.Controls.Add(this.btnStop);
            this.pnlButton.Controls.Add(this.hotkeysButton);
            this.pnlButton.Controls.Add(this.lnkControllers);
            this.pnlButton.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlButton.Location = new System.Drawing.Point(0, 477);
            this.pnlButton.Name = "pnlButton";
            this.pnlButton.Size = new System.Drawing.Size(794, 35);
            this.pnlButton.TabIndex = 10;
            // 
            // AboutButton
            // 
            this.AboutButton.Location = new System.Drawing.Point(9, 5);
            this.AboutButton.Name = "AboutButton";
            this.AboutButton.Size = new System.Drawing.Size(75, 23);
            this.AboutButton.TabIndex = 11;
            this.AboutButton.Text = "About";
            this.AboutButton.UseVisualStyleBackColor = true;
            this.AboutButton.Click += new System.EventHandler(this.AboutButton_Click);
            // 
            // btnStartStop
            // 
            this.btnStartStop.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnStartStop.Location = new System.Drawing.Point(666, 5);
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
            this.btnClear.Location = new System.Drawing.Point(563, 6);
            this.btnClear.Name = "btnClear";
            this.btnClear.Size = new System.Drawing.Size(97, 23);
            this.btnClear.TabIndex = 9;
            this.btnClear.Text = "Clear";
            this.btnClear.UseVisualStyleBackColor = true;
            this.btnClear.Click += new System.EventHandler(this.btnClear_Click);
            // 
            // btnStop
            // 
            this.btnStop.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnStop.Enabled = false;
            this.btnStop.Location = new System.Drawing.Point(710, 6);
            this.btnStop.Name = "btnStop";
            this.btnStop.Size = new System.Drawing.Size(75, 23);
            this.btnStop.TabIndex = 2;
            this.btnStop.Text = "Stop";
            this.btnStop.UseVisualStyleBackColor = true;
            this.btnStop.Visible = false;
            this.btnStop.Click += new System.EventHandler(this.btnStop_Click);
            // 
            // hotkeysButton
            // 
            this.hotkeysButton.Location = new System.Drawing.Point(90, 5);
            this.hotkeysButton.Name = "hotkeysButton";
            this.hotkeysButton.Size = new System.Drawing.Size(75, 23);
            this.hotkeysButton.TabIndex = 12;
            this.hotkeysButton.Text = "Hotkeys";
            this.hotkeysButton.UseVisualStyleBackColor = true;
            this.hotkeysButton.Click += new System.EventHandler(this.hotkeysButton_Click);
            // 
            // lnkControllers
            // 
            this.lnkControllers.AutoSize = true;
            this.lnkControllers.Location = new System.Drawing.Point(171, 10);
            this.lnkControllers.Name = "lnkControllers";
            this.lnkControllers.Size = new System.Drawing.Size(56, 13);
            this.lnkControllers.TabIndex = 11;
            this.lnkControllers.TabStop = true;
            this.lnkControllers.Text = "Controllers";
            this.lnkControllers.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkControllers_LinkClicked);
            // 
            // hideDS4CheckBox
            // 
            this.hideDS4CheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.hideDS4CheckBox.AutoSize = true;
            this.hideDS4CheckBox.Location = new System.Drawing.Point(675, 109);
            this.hideDS4CheckBox.Name = "hideDS4CheckBox";
            this.hideDS4CheckBox.Size = new System.Drawing.Size(119, 17);
            this.hideDS4CheckBox.TabIndex = 13;
            this.hideDS4CheckBox.Text = "Hide DS4 Controller";
            this.hideDS4CheckBox.UseVisualStyleBackColor = true;
            this.hideDS4CheckBox.CheckedChanged += new System.EventHandler(this.hideDS4CheckBox_CheckedChanged);
            // 
            // startMinimizedCheckBox
            // 
            this.startMinimizedCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.startMinimizedCheckBox.AutoSize = true;
            this.startMinimizedCheckBox.Location = new System.Drawing.Point(572, 109);
            this.startMinimizedCheckBox.Name = "startMinimizedCheckBox";
            this.startMinimizedCheckBox.Size = new System.Drawing.Size(97, 17);
            this.startMinimizedCheckBox.TabIndex = 40;
            this.startMinimizedCheckBox.Text = "Start Minimized";
            this.startMinimizedCheckBox.UseVisualStyleBackColor = true;
            this.startMinimizedCheckBox.CheckedChanged += new System.EventHandler(this.startMinimizedCheckBox_CheckedChanged);
            // 
            // pnlDebug
            // 
            this.pnlDebug.Controls.Add(this.lvDebug);
            this.pnlDebug.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlDebug.Location = new System.Drawing.Point(0, 124);
            this.pnlDebug.Name = "pnlDebug";
            this.pnlDebug.Size = new System.Drawing.Size(794, 353);
            this.pnlDebug.TabIndex = 11;
            // 
            // pnlStatus
            // 
            this.pnlStatus.Controls.Add(this.gpPads);
            this.pnlStatus.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlStatus.Location = new System.Drawing.Point(0, 0);
            this.pnlStatus.Name = "pnlStatus";
            this.pnlStatus.Size = new System.Drawing.Size(794, 124);
            this.pnlStatus.TabIndex = 9;
            // 
            // gpPads
            // 
            this.gpPads.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gpPads.Controls.Add(this.lbSelPro4);
            this.gpPads.Controls.Add(this.lbSelPro3);
            this.gpPads.Controls.Add(this.lbSelPro2);
            this.gpPads.Controls.Add(this.lbSelPro1);
            this.gpPads.Controls.Add(this.lbPad4);
            this.gpPads.Controls.Add(this.lbPad3);
            this.gpPads.Controls.Add(this.lbPad2);
            this.gpPads.Controls.Add(this.lbPad1);
            this.gpPads.Controls.Add(this.bnEditC4);
            this.gpPads.Controls.Add(this.bnEditC3);
            this.gpPads.Controls.Add(this.hideDS4CheckBox);
            this.gpPads.Controls.Add(this.bnEditC2);
            this.gpPads.Controls.Add(this.startMinimizedCheckBox);
            this.gpPads.Controls.Add(this.bnDeleteC4);
            this.gpPads.Controls.Add(this.bnDeleteC3);
            this.gpPads.Controls.Add(this.bnDeleteC2);
            this.gpPads.Controls.Add(this.bnDeleteC1);
            this.gpPads.Controls.Add(this.bnEditC1);
            this.gpPads.Controls.Add(this.cBController4);
            this.gpPads.Controls.Add(this.cBController3);
            this.gpPads.Controls.Add(this.cBController2);
            this.gpPads.Controls.Add(this.cBController1);
            this.gpPads.Controls.Add(this.lbLastMessage);
            this.gpPads.Location = new System.Drawing.Point(-6, -9);
            this.gpPads.Name = "gpPads";
            this.gpPads.Size = new System.Drawing.Size(803, 129);
            this.gpPads.TabIndex = 1;
            this.gpPads.TabStop = false;
            // 
            // lbSelPro4
            // 
            this.lbSelPro4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lbSelPro4.AutoSize = true;
            this.lbSelPro4.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbSelPro4.Location = new System.Drawing.Point(470, 86);
            this.lbSelPro4.Name = "lbSelPro4";
            this.lbSelPro4.Size = new System.Drawing.Size(96, 15);
            this.lbSelPro4.TabIndex = 45;
            this.lbSelPro4.Text = "Selected Profile:";
            // 
            // lbSelPro3
            // 
            this.lbSelPro3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lbSelPro3.AutoSize = true;
            this.lbSelPro3.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbSelPro3.Location = new System.Drawing.Point(470, 62);
            this.lbSelPro3.Name = "lbSelPro3";
            this.lbSelPro3.Size = new System.Drawing.Size(96, 15);
            this.lbSelPro3.TabIndex = 45;
            this.lbSelPro3.Text = "Selected Profile:";
            // 
            // lbSelPro2
            // 
            this.lbSelPro2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lbSelPro2.AutoSize = true;
            this.lbSelPro2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbSelPro2.Location = new System.Drawing.Point(470, 40);
            this.lbSelPro2.Name = "lbSelPro2";
            this.lbSelPro2.Size = new System.Drawing.Size(96, 15);
            this.lbSelPro2.TabIndex = 45;
            this.lbSelPro2.Text = "Selected Profile:";
            // 
            // lbSelPro1
            // 
            this.lbSelPro1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lbSelPro1.AutoSize = true;
            this.lbSelPro1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbSelPro1.Location = new System.Drawing.Point(470, 17);
            this.lbSelPro1.Name = "lbSelPro1";
            this.lbSelPro1.Size = new System.Drawing.Size(96, 15);
            this.lbSelPro1.TabIndex = 45;
            this.lbSelPro1.Text = "Selected Profile:";
            // 
            // lbPad4
            // 
            this.lbPad4.AutoSize = true;
            this.lbPad4.Font = new System.Drawing.Font("Lucida Console", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbPad4.Location = new System.Drawing.Point(12, 89);
            this.lbPad4.Name = "lbPad4";
            this.lbPad4.Size = new System.Drawing.Size(167, 13);
            this.lbPad4.TabIndex = 44;
            this.lbPad4.Text = "Pad 4 : Disconnected";
            // 
            // lbPad3
            // 
            this.lbPad3.AutoSize = true;
            this.lbPad3.Font = new System.Drawing.Font("Lucida Console", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbPad3.Location = new System.Drawing.Point(12, 66);
            this.lbPad3.Name = "lbPad3";
            this.lbPad3.Size = new System.Drawing.Size(167, 13);
            this.lbPad3.TabIndex = 44;
            this.lbPad3.Text = "Pad 3 : Disconnected";
            // 
            // lbPad2
            // 
            this.lbPad2.AutoSize = true;
            this.lbPad2.Font = new System.Drawing.Font("Lucida Console", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbPad2.Location = new System.Drawing.Point(12, 43);
            this.lbPad2.Name = "lbPad2";
            this.lbPad2.Size = new System.Drawing.Size(167, 13);
            this.lbPad2.TabIndex = 44;
            this.lbPad2.Text = "Pad 2 : Disconnected";
            // 
            // lbPad1
            // 
            this.lbPad1.AutoSize = true;
            this.lbPad1.Font = new System.Drawing.Font("Lucida Console", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbPad1.Location = new System.Drawing.Point(12, 19);
            this.lbPad1.Name = "lbPad1";
            this.lbPad1.Size = new System.Drawing.Size(167, 13);
            this.lbPad1.TabIndex = 44;
            this.lbPad1.Text = "Pad 1 : Disconnected";
            // 
            // bnEditC4
            // 
            this.bnEditC4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.bnEditC4.Location = new System.Drawing.Point(698, 83);
            this.bnEditC4.Name = "bnEditC4";
            this.bnEditC4.Size = new System.Drawing.Size(40, 23);
            this.bnEditC4.TabIndex = 43;
            this.bnEditC4.Tag = "3";
            this.bnEditC4.Text = "Edit";
            this.bnEditC4.UseVisualStyleBackColor = true;
            this.bnEditC4.Click += new System.EventHandler(this.editButtons_Click);
            // 
            // bnEditC3
            // 
            this.bnEditC3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.bnEditC3.Location = new System.Drawing.Point(698, 60);
            this.bnEditC3.Name = "bnEditC3";
            this.bnEditC3.Size = new System.Drawing.Size(40, 23);
            this.bnEditC3.TabIndex = 43;
            this.bnEditC3.Tag = "2";
            this.bnEditC3.Text = "Edit";
            this.bnEditC3.UseVisualStyleBackColor = true;
            this.bnEditC3.Click += new System.EventHandler(this.editButtons_Click);
            // 
            // bnEditC2
            // 
            this.bnEditC2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.bnEditC2.Location = new System.Drawing.Point(698, 37);
            this.bnEditC2.Name = "bnEditC2";
            this.bnEditC2.Size = new System.Drawing.Size(40, 23);
            this.bnEditC2.TabIndex = 43;
            this.bnEditC2.Tag = "1";
            this.bnEditC2.Text = "Edit";
            this.bnEditC2.UseVisualStyleBackColor = true;
            this.bnEditC2.Click += new System.EventHandler(this.editButtons_Click);
            // 
            // bnDeleteC4
            // 
            this.bnDeleteC4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.bnDeleteC4.Location = new System.Drawing.Point(744, 83);
            this.bnDeleteC4.Name = "bnDeleteC4";
            this.bnDeleteC4.Size = new System.Drawing.Size(47, 23);
            this.bnDeleteC4.TabIndex = 43;
            this.bnDeleteC4.Tag = "3";
            this.bnDeleteC4.Text = "Delete";
            this.bnDeleteC4.UseVisualStyleBackColor = true;
            this.bnDeleteC4.Click += new System.EventHandler(this.deleteButtons_Click);
            // 
            // bnDeleteC3
            // 
            this.bnDeleteC3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.bnDeleteC3.Location = new System.Drawing.Point(744, 60);
            this.bnDeleteC3.Name = "bnDeleteC3";
            this.bnDeleteC3.Size = new System.Drawing.Size(47, 23);
            this.bnDeleteC3.TabIndex = 43;
            this.bnDeleteC3.Tag = "2";
            this.bnDeleteC3.Text = "Delete";
            this.bnDeleteC3.UseVisualStyleBackColor = true;
            this.bnDeleteC3.Click += new System.EventHandler(this.deleteButtons_Click);
            // 
            // bnDeleteC2
            // 
            this.bnDeleteC2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.bnDeleteC2.Location = new System.Drawing.Point(744, 37);
            this.bnDeleteC2.Name = "bnDeleteC2";
            this.bnDeleteC2.Size = new System.Drawing.Size(47, 23);
            this.bnDeleteC2.TabIndex = 43;
            this.bnDeleteC2.Tag = "1";
            this.bnDeleteC2.Text = "Delete";
            this.bnDeleteC2.UseVisualStyleBackColor = true;
            this.bnDeleteC2.Click += new System.EventHandler(this.deleteButtons_Click);
            // 
            // bnDeleteC1
            // 
            this.bnDeleteC1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.bnDeleteC1.Location = new System.Drawing.Point(744, 13);
            this.bnDeleteC1.Name = "bnDeleteC1";
            this.bnDeleteC1.Size = new System.Drawing.Size(47, 23);
            this.bnDeleteC1.TabIndex = 43;
            this.bnDeleteC1.Tag = "0";
            this.bnDeleteC1.Text = "Delete";
            this.bnDeleteC1.UseVisualStyleBackColor = true;
            this.bnDeleteC1.Click += new System.EventHandler(this.deleteButtons_Click);
            // 
            // bnEditC1
            // 
            this.bnEditC1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.bnEditC1.Location = new System.Drawing.Point(698, 13);
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
            this.cBController4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cBController4.Enabled = false;
            this.cBController4.FormattingEnabled = true;
            this.cBController4.Location = new System.Drawing.Point(571, 84);
            this.cBController4.Name = "cBController4";
            this.cBController4.Size = new System.Drawing.Size(121, 21);
            this.cBController4.TabIndex = 42;
            this.cBController4.Tag = "3";
            this.cBController4.SelectedValueChanged += new System.EventHandler(this.Profile_Changed);
            // 
            // cBController3
            // 
            this.cBController3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cBController3.Enabled = false;
            this.cBController3.FormattingEnabled = true;
            this.cBController3.Location = new System.Drawing.Point(571, 61);
            this.cBController3.Name = "cBController3";
            this.cBController3.Size = new System.Drawing.Size(121, 21);
            this.cBController3.TabIndex = 42;
            this.cBController3.Tag = "2";
            this.cBController3.SelectedValueChanged += new System.EventHandler(this.Profile_Changed);
            // 
            // cBController2
            // 
            this.cBController2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cBController2.Enabled = false;
            this.cBController2.FormattingEnabled = true;
            this.cBController2.Location = new System.Drawing.Point(571, 38);
            this.cBController2.Name = "cBController2";
            this.cBController2.Size = new System.Drawing.Size(121, 21);
            this.cBController2.TabIndex = 42;
            this.cBController2.Tag = "1";
            this.cBController2.SelectedValueChanged += new System.EventHandler(this.Profile_Changed);
            // 
            // cBController1
            // 
            this.cBController1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cBController1.Enabled = false;
            this.cBController1.FormattingEnabled = true;
            this.cBController1.Location = new System.Drawing.Point(571, 15);
            this.cBController1.Name = "cBController1";
            this.cBController1.Size = new System.Drawing.Size(121, 21);
            this.cBController1.TabIndex = 42;
            this.cBController1.Tag = "0";
            this.cBController1.SelectedValueChanged += new System.EventHandler(this.Profile_Changed);
            // 
            // lbLastMessage
            // 
            this.lbLastMessage.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.lbLastMessage.ForeColor = System.Drawing.SystemColors.GrayText;
            this.lbLastMessage.Location = new System.Drawing.Point(15, 107);
            this.lbLastMessage.Name = "lbLastMessage";
            this.lbLastMessage.Size = new System.Drawing.Size(551, 20);
            this.lbLastMessage.TabIndex = 41;
            this.lbLastMessage.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lbLastMessage.Visible = false;
            // 
            // notifyIcon1
            // 
            this.notifyIcon1.BalloonTipIcon = System.Windows.Forms.ToolTipIcon.Info;
            this.notifyIcon1.BalloonTipTitle = "Scp server";
            this.notifyIcon1.ContextMenuStrip = this.cMTaskbar;
            this.notifyIcon1.Icon = ((System.Drawing.Icon)(resources.GetObject("notifyIcon1.Icon")));
            this.notifyIcon1.Text = "DS4 Xinput Tool";
            this.notifyIcon1.DoubleClick += new System.EventHandler(this.notifyIcon_Click);
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
            this.cMTaskbar.Size = new System.Drawing.Size(215, 164);
            this.cMTaskbar.Tag = "25";
            // 
            // openToolStripMenuItem
            // 
            this.openToolStripMenuItem.Name = "openToolStripMenuItem";
            this.openToolStripMenuItem.Size = new System.Drawing.Size(214, 22);
            this.openToolStripMenuItem.Text = "Open";
            this.openToolStripMenuItem.Click += new System.EventHandler(this.openToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(211, 6);
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
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(214, 22);
            this.exitToolStripMenuItem.Text = "Exit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(211, 6);
            // 
            // ScpForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(794, 512);
            this.Controls.Add(this.pnlDebug);
            this.Controls.Add(this.pnlButton);
            this.Controls.Add(this.pnlStatus);
            this.MinimumSize = new System.Drawing.Size(560, 192);
            this.Name = "ScpForm";
            this.Text = "DS4Windows 1.0 Beta J2K Build";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form_Close);
            this.Load += new System.EventHandler(this.Form_Load);
            this.Move += new System.EventHandler(this.ScpForm_Move);
            this.Resize += new System.EventHandler(this.Form_Resize);
            this.pnlButton.ResumeLayout(false);
            this.pnlButton.PerformLayout();
            this.pnlDebug.ResumeLayout(false);
            this.pnlStatus.ResumeLayout(false);
            this.gpPads.ResumeLayout(false);
            this.gpPads.PerformLayout();
            this.cMTaskbar.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListView lvDebug;
        private System.Windows.Forms.ColumnHeader chTime;
        private System.Windows.Forms.ColumnHeader chData;
        private System.Windows.Forms.Timer tmrUpdate;
        private System.Windows.Forms.Panel pnlButton;
        private System.Windows.Forms.Button btnStop;
        private System.Windows.Forms.Button btnStartStop;
        private System.Windows.Forms.Panel pnlDebug;
        private System.Windows.Forms.Button btnClear;
        private System.Windows.Forms.Panel pnlStatus;
        private System.Windows.Forms.GroupBox gpPads;
        private System.Windows.Forms.NotifyIcon notifyIcon1;
        private System.Windows.Forms.Button hotkeysButton;
        private System.Windows.Forms.CheckBox hideDS4CheckBox;
        private System.Windows.Forms.CheckBox startMinimizedCheckBox;
        private System.Windows.Forms.Label lbLastMessage;
        private System.Windows.Forms.LinkLabel lnkControllers;
        private System.Windows.Forms.Button AboutButton;
        private System.Windows.Forms.ComboBox cBController4;
        private System.Windows.Forms.ComboBox cBController3;
        private System.Windows.Forms.ComboBox cBController2;
        private System.Windows.Forms.ComboBox cBController1;
        private System.Windows.Forms.Button bnEditC4;
        private System.Windows.Forms.Button bnEditC3;
        private System.Windows.Forms.Button bnEditC2;
        private System.Windows.Forms.Button bnEditC1;
        private System.Windows.Forms.Button bnDeleteC1;
        private System.Windows.Forms.Button bnDeleteC4;
        private System.Windows.Forms.Button bnDeleteC3;
        private System.Windows.Forms.Button bnDeleteC2;
        private System.Windows.Forms.Label lbPad4;
        private System.Windows.Forms.Label lbPad3;
        private System.Windows.Forms.Label lbPad2;
        private System.Windows.Forms.Label lbPad1;
        private System.Windows.Forms.Label lbSelPro1;
        private System.Windows.Forms.Label lbSelPro4;
        private System.Windows.Forms.Label lbSelPro3;
        private System.Windows.Forms.Label lbSelPro2;
        private System.Windows.Forms.ContextMenuStrip cMTaskbar;
        private System.Windows.Forms.ToolStripMenuItem openToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem editProfileForController1ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem editProfileForController2ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem editProfileForController4ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem editProfileForController3ToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        //private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem2;
    }
}

