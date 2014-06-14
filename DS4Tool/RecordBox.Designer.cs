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
            this.SuspendLayout();
            // 
            // btnRecord
            // 
            this.btnRecord.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnRecord.Location = new System.Drawing.Point(13, 2);
            this.btnRecord.Name = "btnRecord";
            this.btnRecord.Size = new System.Drawing.Size(258, 23);
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
            this.cBRecordDelays.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.cBRecordDelays.AutoSize = true;
            this.cBRecordDelays.Location = new System.Drawing.Point(12, 219);
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
            this.lVMacros.LargeImageList = this.iLKeys;
            this.lVMacros.Location = new System.Drawing.Point(13, 29);
            this.lVMacros.Name = "lVMacros";
            this.lVMacros.Size = new System.Drawing.Size(258, 182);
            this.lVMacros.SmallImageList = this.iLKeys;
            this.lVMacros.TabIndex = 326;
            this.lVMacros.UseCompatibleStateImageBehavior = false;
            this.lVMacros.View = System.Windows.Forms.View.Details;
            this.lVMacros.KeyDown += new System.Windows.Forms.KeyEventHandler(this.anyKeyDown);
            this.lVMacros.KeyUp += new System.Windows.Forms.KeyEventHandler(this.anyKeyUp);
            this.lVMacros.MouseDown += new System.Windows.Forms.MouseEventHandler(this.anyMouseDown);
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
            this.cBStyle.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cBStyle.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cBStyle.FormattingEnabled = true;
            this.cBStyle.Items.AddRange(new object[] {
            "Play once",
            "Repeat while held"});
            this.cBStyle.Location = new System.Drawing.Point(150, 217);
            this.cBStyle.Name = "cBStyle";
            this.cBStyle.Size = new System.Drawing.Size(121, 21);
            this.cBStyle.TabIndex = 327;
            // 
            // btnSave
            // 
            this.btnSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnSave.Location = new System.Drawing.Point(12, 242);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(75, 23);
            this.btnSave.TabIndex = 328;
            this.btnSave.Text = "Save";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.Location = new System.Drawing.Point(196, 243);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 328;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // RecordBox
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.ClientSize = new System.Drawing.Size(276, 268);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.cBStyle);
            this.Controls.Add(this.lVMacros);
            this.Controls.Add(this.cBRecordDelays);
            this.Controls.Add(this.btnRecord);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.Name = "RecordBox";
            this.ShowInTaskbar = false;
            this.Text = "Record a Macro";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.RecordBox_FormClosing);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.anyKeyDown);
            this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.anyKeyUp);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.anyMouseDown);
            this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.anyMouseUp);
            this.ResumeLayout(false);
            this.PerformLayout();

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
    }
}