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
            this.lnkControllers = new System.Windows.Forms.LinkLabel();
            this.pnlDebug = new System.Windows.Forms.Panel();
            this.pnlStatus = new System.Windows.Forms.Panel();
            this.gpPads = new System.Windows.Forms.GroupBox();
            this.lbLastMessage = new System.Windows.Forms.Label();
            this.startMinimizedCheckBox = new System.Windows.Forms.CheckBox();
            this.hideDS4CheckBox = new System.Windows.Forms.CheckBox();
            this.hotkeysButton = new System.Windows.Forms.Button();
            this.optionsButton = new System.Windows.Forms.Button();
            this.rbPad_4 = new System.Windows.Forms.RadioButton();
            this.rbPad_3 = new System.Windows.Forms.RadioButton();
            this.rbPad_2 = new System.Windows.Forms.RadioButton();
            this.rbPad_1 = new System.Windows.Forms.RadioButton();
            this.notifyIcon1 = new System.Windows.Forms.NotifyIcon(this.components);
            this.pnlButton.SuspendLayout();
            this.pnlDebug.SuspendLayout();
            this.pnlStatus.SuspendLayout();
            this.gpPads.SuspendLayout();
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
            this.lvDebug.Size = new System.Drawing.Size(794, 353);
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
            this.btnStartStop.Location = new System.Drawing.Point(663, 5);
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
            this.btnClear.Location = new System.Drawing.Point(560, 6);
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
            this.btnStop.Location = new System.Drawing.Point(707, 6);
            this.btnStop.Name = "btnStop";
            this.btnStop.Size = new System.Drawing.Size(75, 23);
            this.btnStop.TabIndex = 2;
            this.btnStop.Text = "Stop";
            this.btnStop.UseVisualStyleBackColor = true;
            this.btnStop.Visible = false;
            this.btnStop.Click += new System.EventHandler(this.btnStop_Click);
            // 
            // lnkControllers
            // 
            this.lnkControllers.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lnkControllers.AutoSize = true;
            this.lnkControllers.Location = new System.Drawing.Point(495, 96);
            this.lnkControllers.Name = "lnkControllers";
            this.lnkControllers.Size = new System.Drawing.Size(56, 13);
            this.lnkControllers.TabIndex = 11;
            this.lnkControllers.TabStop = true;
            this.lnkControllers.Text = "Controllers";
            this.lnkControllers.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkControllers_LinkClicked);
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
            this.gpPads.Controls.Add(this.lnkControllers);
            this.gpPads.Controls.Add(this.lbLastMessage);
            this.gpPads.Controls.Add(this.startMinimizedCheckBox);
            this.gpPads.Controls.Add(this.hideDS4CheckBox);
            this.gpPads.Controls.Add(this.hotkeysButton);
            this.gpPads.Controls.Add(this.optionsButton);
            this.gpPads.Controls.Add(this.rbPad_4);
            this.gpPads.Controls.Add(this.rbPad_3);
            this.gpPads.Controls.Add(this.rbPad_2);
            this.gpPads.Controls.Add(this.rbPad_1);
            this.gpPads.Location = new System.Drawing.Point(3, 3);
            this.gpPads.Name = "gpPads";
            this.gpPads.Size = new System.Drawing.Size(791, 117);
            this.gpPads.TabIndex = 1;
            this.gpPads.TabStop = false;
            // 
            // lbLastMessage
            // 
            this.lbLastMessage.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.lbLastMessage.ForeColor = System.Drawing.SystemColors.GrayText;
            this.lbLastMessage.Location = new System.Drawing.Point(58, 94);
            this.lbLastMessage.Name = "lbLastMessage";
            this.lbLastMessage.Size = new System.Drawing.Size(438, 17);
            this.lbLastMessage.TabIndex = 41;
            this.lbLastMessage.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lbLastMessage.Visible = false;
            // 
            // startMinimizedCheckBox
            // 
            this.startMinimizedCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.startMinimizedCheckBox.AutoSize = true;
            this.startMinimizedCheckBox.Location = new System.Drawing.Point(557, 94);
            this.startMinimizedCheckBox.Name = "startMinimizedCheckBox";
            this.startMinimizedCheckBox.Size = new System.Drawing.Size(97, 17);
            this.startMinimizedCheckBox.TabIndex = 40;
            this.startMinimizedCheckBox.Text = "Start Minimized";
            this.startMinimizedCheckBox.UseVisualStyleBackColor = true;
            this.startMinimizedCheckBox.CheckedChanged += new System.EventHandler(this.startMinimizedCheckBox_CheckedChanged);
            // 
            // hideDS4CheckBox
            // 
            this.hideDS4CheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.hideDS4CheckBox.AutoSize = true;
            this.hideDS4CheckBox.Location = new System.Drawing.Point(660, 94);
            this.hideDS4CheckBox.Name = "hideDS4CheckBox";
            this.hideDS4CheckBox.Size = new System.Drawing.Size(119, 17);
            this.hideDS4CheckBox.TabIndex = 13;
            this.hideDS4CheckBox.Text = "Hide DS4 Controller";
            this.hideDS4CheckBox.UseVisualStyleBackColor = true;
            this.hideDS4CheckBox.CheckedChanged += new System.EventHandler(this.hideDS4CheckBox_CheckedChanged);
            // 
            // hotkeysButton
            // 
            this.hotkeysButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.hotkeysButton.Location = new System.Drawing.Point(704, 50);
            this.hotkeysButton.Name = "hotkeysButton";
            this.hotkeysButton.Size = new System.Drawing.Size(75, 23);
            this.hotkeysButton.TabIndex = 12;
            this.hotkeysButton.Text = "Hotkeys";
            this.hotkeysButton.UseVisualStyleBackColor = true;
            this.hotkeysButton.Click += new System.EventHandler(this.hotkeysButton_Click);
            // 
            // optionsButton
            // 
            this.optionsButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.optionsButton.Location = new System.Drawing.Point(704, 10);
            this.optionsButton.Name = "optionsButton";
            this.optionsButton.Size = new System.Drawing.Size(75, 23);
            this.optionsButton.TabIndex = 11;
            this.optionsButton.Text = "Options";
            this.optionsButton.UseVisualStyleBackColor = true;
            this.optionsButton.Click += new System.EventHandler(this.optionsButton_Click);
            // 
            // rbPad_4
            // 
            this.rbPad_4.AutoSize = true;
            this.rbPad_4.Enabled = false;
            this.rbPad_4.Font = new System.Drawing.Font("Lucida Console", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.rbPad_4.Location = new System.Drawing.Point(6, 79);
            this.rbPad_4.Name = "rbPad_4";
            this.rbPad_4.Size = new System.Drawing.Size(185, 17);
            this.rbPad_4.TabIndex = 3;
            this.rbPad_4.TabStop = true;
            this.rbPad_4.Text = "Pad 4 : Disconnected";
            this.rbPad_4.UseVisualStyleBackColor = true;
            // 
            // rbPad_3
            // 
            this.rbPad_3.AutoSize = true;
            this.rbPad_3.Enabled = false;
            this.rbPad_3.Font = new System.Drawing.Font("Lucida Console", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.rbPad_3.Location = new System.Drawing.Point(6, 56);
            this.rbPad_3.Name = "rbPad_3";
            this.rbPad_3.Size = new System.Drawing.Size(185, 17);
            this.rbPad_3.TabIndex = 2;
            this.rbPad_3.TabStop = true;
            this.rbPad_3.Text = "Pad 3 : Disconnected";
            this.rbPad_3.UseVisualStyleBackColor = true;
            // 
            // rbPad_2
            // 
            this.rbPad_2.AutoSize = true;
            this.rbPad_2.Enabled = false;
            this.rbPad_2.Font = new System.Drawing.Font("Lucida Console", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.rbPad_2.Location = new System.Drawing.Point(6, 33);
            this.rbPad_2.Name = "rbPad_2";
            this.rbPad_2.Size = new System.Drawing.Size(185, 17);
            this.rbPad_2.TabIndex = 1;
            this.rbPad_2.TabStop = true;
            this.rbPad_2.Text = "Pad 2 : Disconnected";
            this.rbPad_2.UseVisualStyleBackColor = true;
            // 
            // rbPad_1
            // 
            this.rbPad_1.AutoSize = true;
            this.rbPad_1.Enabled = false;
            this.rbPad_1.Font = new System.Drawing.Font("Lucida Console", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.rbPad_1.Location = new System.Drawing.Point(6, 10);
            this.rbPad_1.Name = "rbPad_1";
            this.rbPad_1.Size = new System.Drawing.Size(185, 17);
            this.rbPad_1.TabIndex = 0;
            this.rbPad_1.TabStop = true;
            this.rbPad_1.Text = "Pad 1 : Disconnected";
            this.rbPad_1.UseVisualStyleBackColor = true;
            // 
            // notifyIcon1
            // 
            this.notifyIcon1.BalloonTipIcon = System.Windows.Forms.ToolTipIcon.Info;
            this.notifyIcon1.BalloonTipTitle = "Scp server";
            this.notifyIcon1.Icon = ((System.Drawing.Icon)(resources.GetObject("notifyIcon1.Icon")));
            this.notifyIcon1.Text = "DS4 Xinput Tool";
            this.notifyIcon1.Click += new System.EventHandler(this.notifyIcon_Click);
            // 
            // ScpForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(794, 512);
            this.Controls.Add(this.pnlDebug);
            this.Controls.Add(this.pnlButton);
            this.Controls.Add(this.pnlStatus);
            this.MinimumSize = new System.Drawing.Size(750, 192);
            this.Name = "ScpForm";
            this.Text = "DS4Windows 1.0 Alpha 2 (2014-03-30.0)";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form_Close);
            this.Load += new System.EventHandler(this.Form_Load);
            this.Resize += new System.EventHandler(this.Form_Resize);
            this.pnlButton.ResumeLayout(false);
            this.pnlDebug.ResumeLayout(false);
            this.pnlStatus.ResumeLayout(false);
            this.gpPads.ResumeLayout(false);
            this.gpPads.PerformLayout();
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
        private System.Windows.Forms.Button optionsButton;
        private System.Windows.Forms.RadioButton rbPad_4;
        private System.Windows.Forms.RadioButton rbPad_3;
        private System.Windows.Forms.RadioButton rbPad_2;
        private System.Windows.Forms.RadioButton rbPad_1;
        private System.Windows.Forms.NotifyIcon notifyIcon1;
        private System.Windows.Forms.Button hotkeysButton;
        private System.Windows.Forms.CheckBox hideDS4CheckBox;
        private System.Windows.Forms.CheckBox startMinimizedCheckBox;
        private System.Windows.Forms.Label lbLastMessage;
        private System.Windows.Forms.LinkLabel lnkControllers;
        private System.Windows.Forms.Button AboutButton;
    }
}

