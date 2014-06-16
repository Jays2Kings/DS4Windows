namespace ScpServer
{
    partial class RecordBox
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RecordBox));
            this.btnRecord = new System.Windows.Forms.Button();
            this.cBRecordDelays = new System.Windows.Forms.CheckBox();
            this.lVMacros = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.iLKeys = new System.Windows.Forms.ImageList(this.components);
            this.cBStyle = new System.Windows.Forms.ComboBox();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.cB360Controls = new System.Windows.Forms.ComboBox();
            this.lBHoldX360 = new System.Windows.Forms.Label();
            this.pnlSettings = new System.Windows.Forms.Panel();
            this.pnlMouseButtons = new System.Windows.Forms.Panel();
            this.btn5th = new System.Windows.Forms.Button();
            this.btn4th = new System.Windows.Forms.Button();
            this.pnlSettings.SuspendLayout();
            this.pnlMouseButtons.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnRecord
            // 
            this.btnRecord.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnRecord.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnRecord.Location = new System.Drawing.Point(3, 2);
            this.btnRecord.Name = "btnRecord";
            this.btnRecord.Size = new System.Drawing.Size(268, 23);
            this.btnRecord.TabIndex = 322;
            this.btnRecord.TabStop = false;
            this.btnRecord.Text = "Record";
            this.btnRecord.UseVisualStyleBackColor = true;
            this.btnRecord.Click += new System.EventHandler(this.btnRecord_Click);
            this.btnRecord.KeyDown += new System.Windows.Forms.KeyEventHandler(this.anyKeyDown);
            this.btnRecord.KeyUp += new System.Windows.Forms.KeyEventHandler(this.anyKeyUp);
            // 
            // cBRecordDelays
            // 
            this.cBRecordDelays.AutoSize = true;
            this.cBRecordDelays.Location = new System.Drawing.Point(0, 5);
            this.cBRecordDelays.Name = "cBRecordDelays";
            this.cBRecordDelays.Size = new System.Drawing.Size(96, 17);
            this.cBRecordDelays.TabIndex = 324;
            this.cBRecordDelays.TabStop = false;
            this.cBRecordDelays.Text = "Record Delays";
            this.cBRecordDelays.UseVisualStyleBackColor = true;
            this.cBRecordDelays.KeyDown += new System.Windows.Forms.KeyEventHandler(this.anyKeyDown);
            this.cBRecordDelays.KeyUp += new System.Windows.Forms.KeyEventHandler(this.anyKeyUp);
            this.cBRecordDelays.MouseDown += new System.Windows.Forms.MouseEventHandler(this.anyMouseDown);
            this.cBRecordDelays.MouseUp += new System.Windows.Forms.MouseEventHandler(this.anyMouseUp);
            // 
            // lVMacros
            // 
            this.lVMacros.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lVMacros.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1});
            this.lVMacros.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.lVMacros.LargeImageList = this.iLKeys;
            this.lVMacros.Location = new System.Drawing.Point(3, 29);
            this.lVMacros.Name = "lVMacros";
            this.lVMacros.Size = new System.Drawing.Size(268, 200);
            this.lVMacros.SmallImageList = this.iLKeys;
            this.lVMacros.TabIndex = 326;
            this.lVMacros.UseCompatibleStateImageBehavior = false;
            this.lVMacros.View = System.Windows.Forms.View.Details;
            this.lVMacros.KeyDown += new System.Windows.Forms.KeyEventHandler(this.anyKeyDown);
            this.lVMacros.KeyUp += new System.Windows.Forms.KeyEventHandler(this.anyKeyUp);
            this.lVMacros.MouseDown += new System.Windows.Forms.MouseEventHandler(this.anyMouseDown);
            this.lVMacros.MouseMove += new System.Windows.Forms.MouseEventHandler(this.lVMacros_MouseMove);
            this.lVMacros.MouseUp += new System.Windows.Forms.MouseEventHandler(this.anyMouseUp);
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Macro Order";
            this.columnHeader1.Width = 150;
            // 
            // iLKeys
            // 
            this.iLKeys.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("iLKeys.ImageStream")));
            this.iLKeys.TransparentColor = System.Drawing.Color.Transparent;
            this.iLKeys.Images.SetKeyName(0, "keydown.png");
            this.iLKeys.Images.SetKeyName(1, "keyup.png");
            this.iLKeys.Images.SetKeyName(2, "Clock.png");
            // 
            // cBStyle
            // 
            this.cBStyle.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cBStyle.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cBStyle.FormattingEnabled = true;
            this.cBStyle.Items.AddRange(new object[] {
            "Play once",
            "Repeat while held"});
            this.cBStyle.Location = new System.Drawing.Point(147, 3);
            this.cBStyle.Name = "cBStyle";
            this.cBStyle.Size = new System.Drawing.Size(121, 21);
            this.cBStyle.TabIndex = 327;
            // 
            // btnSave
            // 
            this.btnSave.Location = new System.Drawing.Point(0, 51);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(75, 23);
            this.btnSave.TabIndex = 328;
            this.btnSave.Text = "Save";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.Location = new System.Drawing.Point(193, 51);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 328;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // cB360Controls
            // 
            this.cB360Controls.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cB360Controls.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cB360Controls.FormattingEnabled = true;
            this.cB360Controls.Items.AddRange(new object[] {
            "(none)",
            "A Button",
            "B Button",
            "X Button",
            "Y Button",
            "Up Button",
            "Down Button",
            "Left Button",
            "Right Button",
            "Start",
            "Back",
            "Guide",
            "Left Bumper",
            "Right Bumper",
            "Left Trigger",
            "Right Trigger",
            "Left Stick",
            "Right Click",
            "Left Y-Axis+",
            "Left Y-Axis-",
            "Left X-Axis+",
            "Left X-Axis-",
            "Right Y-Axis+",
            "Right Y-Axis-",
            "Right X-Axis+",
            "Right X-Axis-"});
            this.cB360Controls.Location = new System.Drawing.Point(147, 27);
            this.cB360Controls.Name = "cB360Controls";
            this.cB360Controls.Size = new System.Drawing.Size(121, 21);
            this.cB360Controls.TabIndex = 329;
            // 
            // lBHoldX360
            // 
            this.lBHoldX360.AutoSize = true;
            this.lBHoldX360.Location = new System.Drawing.Point(-3, 30);
            this.lBHoldX360.Name = "lBHoldX360";
            this.lBHoldX360.Size = new System.Drawing.Size(125, 13);
            this.lBHoldX360.TabIndex = 330;
            this.lBHoldX360.Text = "Also hold a X360 control:";
            // 
            // pnlSettings
            // 
            this.pnlSettings.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pnlSettings.Controls.Add(this.lBHoldX360);
            this.pnlSettings.Controls.Add(this.cBStyle);
            this.pnlSettings.Controls.Add(this.cB360Controls);
            this.pnlSettings.Controls.Add(this.cBRecordDelays);
            this.pnlSettings.Controls.Add(this.btnCancel);
            this.pnlSettings.Controls.Add(this.btnSave);
            this.pnlSettings.Location = new System.Drawing.Point(3, 229);
            this.pnlSettings.Name = "pnlSettings";
            this.pnlSettings.Size = new System.Drawing.Size(272, 77);
            this.pnlSettings.TabIndex = 331;
            this.pnlSettings.MouseDown += new System.Windows.Forms.MouseEventHandler(this.anyMouseDown);
            this.pnlSettings.MouseUp += new System.Windows.Forms.MouseEventHandler(this.anyMouseUp);
            // 
            // pnlMouseButtons
            // 
            this.pnlMouseButtons.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pnlMouseButtons.Controls.Add(this.btn5th);
            this.pnlMouseButtons.Controls.Add(this.btn4th);
            this.pnlMouseButtons.Location = new System.Drawing.Point(3, 232);
            this.pnlMouseButtons.Name = "pnlMouseButtons";
            this.pnlMouseButtons.Size = new System.Drawing.Size(272, 78);
            this.pnlMouseButtons.TabIndex = 331;
            this.pnlMouseButtons.Visible = false;
            this.pnlMouseButtons.MouseDown += new System.Windows.Forms.MouseEventHandler(this.anyMouseDown);
            this.pnlMouseButtons.MouseUp += new System.Windows.Forms.MouseEventHandler(this.anyMouseUp);
            // 
            // btn5th
            // 
            this.btn5th.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.btn5th.Location = new System.Drawing.Point(76, 42);
            this.btn5th.Name = "btn5th";
            this.btn5th.Size = new System.Drawing.Size(131, 23);
            this.btn5th.TabIndex = 0;
            this.btn5th.Text = "5th Mouse Button Down";
            this.btn5th.UseVisualStyleBackColor = true;
            this.btn5th.Click += new System.EventHandler(this.btn5th_Click);
            this.btn5th.KeyDown += new System.Windows.Forms.KeyEventHandler(this.anyKeyDown);
            this.btn5th.KeyUp += new System.Windows.Forms.KeyEventHandler(this.anyKeyUp);
            // 
            // btn4th
            // 
            this.btn4th.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.btn4th.Location = new System.Drawing.Point(76, 13);
            this.btn4th.Name = "btn4th";
            this.btn4th.Size = new System.Drawing.Size(131, 23);
            this.btn4th.TabIndex = 0;
            this.btn4th.Text = "4th Mouse Button Down";
            this.btn4th.UseVisualStyleBackColor = true;
            this.btn4th.Click += new System.EventHandler(this.btn4th_Click);
            this.btn4th.KeyDown += new System.Windows.Forms.KeyEventHandler(this.anyKeyDown);
            this.btn4th.KeyUp += new System.Windows.Forms.KeyEventHandler(this.anyKeyUp);
            // 
            // RecordBox
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.ClientSize = new System.Drawing.Size(276, 306);
            this.Controls.Add(this.btnRecord);
            this.Controls.Add(this.lVMacros);
            this.Controls.Add(this.pnlSettings);
            this.Controls.Add(this.pnlMouseButtons);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.MinimumSize = new System.Drawing.Size(260, 205);
            this.Name = "RecordBox";
            this.ShowInTaskbar = false;
            this.Text = "Record a Macro";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.RecordBox_FormClosing);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.anyKeyDown);
            this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.anyKeyUp);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.anyMouseDown);
            this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.anyMouseUp);
            this.pnlSettings.ResumeLayout(false);
            this.pnlSettings.PerformLayout();
            this.pnlMouseButtons.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnRecord;
        private System.Windows.Forms.CheckBox cBRecordDelays;
        private System.Windows.Forms.ListView lVMacros;
        private System.Windows.Forms.ImageList iLKeys;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ComboBox cBStyle;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.ComboBox cB360Controls;
        private System.Windows.Forms.Label lBHoldX360;
        private System.Windows.Forms.Panel pnlSettings;
        private System.Windows.Forms.Panel pnlMouseButtons;
        private System.Windows.Forms.Button btn5th;
        private System.Windows.Forms.Button btn4th;
    }
}