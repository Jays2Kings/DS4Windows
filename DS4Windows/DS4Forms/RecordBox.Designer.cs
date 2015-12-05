namespace DS4Windows
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
            this.cHMacro = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.iLKeys = new System.Windows.Forms.ImageList(this.components);
            this.cBStyle = new System.Windows.Forms.ComboBox();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnSaveP = new System.Windows.Forms.Button();
            this.lbRecordTip = new System.Windows.Forms.Label();
            this.pnlMouseButtons = new System.Windows.Forms.Panel();
            this.pBRtouch = new System.Windows.Forms.PictureBox();
            this.pBLtouch = new System.Windows.Forms.PictureBox();
            this.btnLightbar = new System.Windows.Forms.Button();
            this.btnRumble = new System.Windows.Forms.Button();
            this.btn5th = new System.Windows.Forms.Button();
            this.btn4th = new System.Windows.Forms.Button();
            this.btnLoadP = new System.Windows.Forms.Button();
            this.savePresets = new System.Windows.Forms.SaveFileDialog();
            this.openPresets = new System.Windows.Forms.OpenFileDialog();
            this.lbMacroOrder = new System.Windows.Forms.Label();
            this.lbDelayTip = new System.Windows.Forms.Label();
            this.cMSLoadPresets = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.altTabToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.fromFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pnlMouseButtons.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pBRtouch)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pBLtouch)).BeginInit();
            this.cMSLoadPresets.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnRecord
            // 
            resources.ApplyResources(this.btnRecord, "btnRecord");
            this.btnRecord.Name = "btnRecord";
            this.btnRecord.TabStop = false;
            this.btnRecord.UseVisualStyleBackColor = true;
            this.btnRecord.Click += new System.EventHandler(this.btnRecord_Click);
            this.btnRecord.KeyDown += new System.Windows.Forms.KeyEventHandler(this.anyKeyDown);
            this.btnRecord.KeyUp += new System.Windows.Forms.KeyEventHandler(this.anyKeyUp);
            // 
            // cBRecordDelays
            // 
            resources.ApplyResources(this.cBRecordDelays, "cBRecordDelays");
            this.cBRecordDelays.Name = "cBRecordDelays";
            this.cBRecordDelays.TabStop = false;
            this.cBRecordDelays.UseVisualStyleBackColor = true;
            this.cBRecordDelays.KeyDown += new System.Windows.Forms.KeyEventHandler(this.anyKeyDown);
            this.cBRecordDelays.KeyUp += new System.Windows.Forms.KeyEventHandler(this.anyKeyUp);
            this.cBRecordDelays.MouseDown += new System.Windows.Forms.MouseEventHandler(this.anyMouseDown);
            this.cBRecordDelays.MouseUp += new System.Windows.Forms.MouseEventHandler(this.anyMouseUp);
            // 
            // lVMacros
            // 
            resources.ApplyResources(this.lVMacros, "lVMacros");
            this.lVMacros.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.cHMacro});
            this.lVMacros.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.lVMacros.LargeImageList = this.iLKeys;
            this.lVMacros.MultiSelect = false;
            this.lVMacros.Name = "lVMacros";
            this.lVMacros.SmallImageList = this.iLKeys;
            this.lVMacros.TileSize = new System.Drawing.Size(180, 30);
            this.lVMacros.UseCompatibleStateImageBehavior = false;
            this.lVMacros.View = System.Windows.Forms.View.Details;
            this.lVMacros.SelectedIndexChanged += new System.EventHandler(this.lVMacros_SelectedIndexChanged);
            this.lVMacros.KeyDown += new System.Windows.Forms.KeyEventHandler(this.anyKeyDown);
            this.lVMacros.KeyUp += new System.Windows.Forms.KeyEventHandler(this.anyKeyUp);
            this.lVMacros.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.lVMacros_MouseDoubleClick);
            this.lVMacros.MouseDown += new System.Windows.Forms.MouseEventHandler(this.anyMouseDown);
            this.lVMacros.MouseHover += new System.EventHandler(this.lVMacros_MouseHover);
            this.lVMacros.MouseUp += new System.Windows.Forms.MouseEventHandler(this.anyMouseUp);
            // 
            // cHMacro
            // 
            resources.ApplyResources(this.cHMacro, "cHMacro");
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
            resources.ApplyResources(this.cBStyle, "cBStyle");
            this.cBStyle.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cBStyle.FormattingEnabled = true;
            this.cBStyle.Items.AddRange(new object[] {
            resources.GetString("cBStyle.Items"),
            resources.GetString("cBStyle.Items1")});
            this.cBStyle.Name = "cBStyle";
            // 
            // btnSave
            // 
            resources.ApplyResources(this.btnSave, "btnSave");
            this.btnSave.Name = "btnSave";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // btnCancel
            // 
            resources.ApplyResources(this.btnCancel, "btnCancel");
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnSaveP
            // 
            resources.ApplyResources(this.btnSaveP, "btnSaveP");
            this.btnSaveP.Name = "btnSaveP";
            this.btnSaveP.UseVisualStyleBackColor = true;
            this.btnSaveP.Click += new System.EventHandler(this.btnSaveP_Click);
            // 
            // lbRecordTip
            // 
            resources.ApplyResources(this.lbRecordTip, "lbRecordTip");
            this.lbRecordTip.Name = "lbRecordTip";
            // 
            // pnlMouseButtons
            // 
            resources.ApplyResources(this.pnlMouseButtons, "pnlMouseButtons");
            this.pnlMouseButtons.Controls.Add(this.pBRtouch);
            this.pnlMouseButtons.Controls.Add(this.pBLtouch);
            this.pnlMouseButtons.Controls.Add(this.btnLightbar);
            this.pnlMouseButtons.Controls.Add(this.btnRumble);
            this.pnlMouseButtons.Controls.Add(this.btn5th);
            this.pnlMouseButtons.Controls.Add(this.btn4th);
            this.pnlMouseButtons.Name = "pnlMouseButtons";
            this.pnlMouseButtons.MouseDown += new System.Windows.Forms.MouseEventHandler(this.anyMouseDown);
            this.pnlMouseButtons.MouseUp += new System.Windows.Forms.MouseEventHandler(this.anyMouseUp);
            // 
            // pBRtouch
            // 
            resources.ApplyResources(this.pBRtouch, "pBRtouch");
            this.pBRtouch.Image = global::DS4Windows.Properties.Resources.right_touch;
            this.pBRtouch.Name = "pBRtouch";
            this.pBRtouch.TabStop = false;
            // 
            // pBLtouch
            // 
            resources.ApplyResources(this.pBLtouch, "pBLtouch");
            this.pBLtouch.Image = global::DS4Windows.Properties.Resources.left_touch;
            this.pBLtouch.Name = "pBLtouch";
            this.pBLtouch.TabStop = false;
            // 
            // btnLightbar
            // 
            resources.ApplyResources(this.btnLightbar, "btnLightbar");
            this.btnLightbar.Name = "btnLightbar";
            this.btnLightbar.UseVisualStyleBackColor = true;
            this.btnLightbar.Click += new System.EventHandler(this.btnLightbar_Click);
            this.btnLightbar.KeyDown += new System.Windows.Forms.KeyEventHandler(this.anyKeyDown);
            this.btnLightbar.KeyUp += new System.Windows.Forms.KeyEventHandler(this.anyKeyUp);
            // 
            // btnRumble
            // 
            resources.ApplyResources(this.btnRumble, "btnRumble");
            this.btnRumble.Name = "btnRumble";
            this.btnRumble.UseVisualStyleBackColor = true;
            this.btnRumble.Click += new System.EventHandler(this.btnRumble_Click);
            this.btnRumble.KeyDown += new System.Windows.Forms.KeyEventHandler(this.anyKeyDown);
            this.btnRumble.KeyUp += new System.Windows.Forms.KeyEventHandler(this.anyKeyUp);
            // 
            // btn5th
            // 
            resources.ApplyResources(this.btn5th, "btn5th");
            this.btn5th.Name = "btn5th";
            this.btn5th.UseVisualStyleBackColor = true;
            this.btn5th.Click += new System.EventHandler(this.btn5th_Click);
            this.btn5th.KeyDown += new System.Windows.Forms.KeyEventHandler(this.anyKeyDown);
            this.btn5th.KeyUp += new System.Windows.Forms.KeyEventHandler(this.anyKeyUp);
            // 
            // btn4th
            // 
            resources.ApplyResources(this.btn4th, "btn4th");
            this.btn4th.Name = "btn4th";
            this.btn4th.UseVisualStyleBackColor = true;
            this.btn4th.Click += new System.EventHandler(this.btn4th_Click);
            this.btn4th.KeyDown += new System.Windows.Forms.KeyEventHandler(this.anyKeyDown);
            this.btn4th.KeyUp += new System.Windows.Forms.KeyEventHandler(this.anyKeyUp);
            // 
            // btnLoadP
            // 
            resources.ApplyResources(this.btnLoadP, "btnLoadP");
            this.btnLoadP.Name = "btnLoadP";
            this.btnLoadP.UseVisualStyleBackColor = true;
            this.btnLoadP.Click += new System.EventHandler(this.btnLoadP_Click);
            // 
            // savePresets
            // 
            resources.ApplyResources(this.savePresets, "savePresets");
            // 
            // openPresets
            // 
            this.openPresets.FileName = "openFileDialog1";
            resources.ApplyResources(this.openPresets, "openPresets");
            // 
            // lbMacroOrder
            // 
            resources.ApplyResources(this.lbMacroOrder, "lbMacroOrder");
            this.lbMacroOrder.Name = "lbMacroOrder";
            // 
            // lbDelayTip
            // 
            resources.ApplyResources(this.lbDelayTip, "lbDelayTip");
            this.lbDelayTip.Name = "lbDelayTip";
            // 
            // cMSLoadPresets
            // 
            resources.ApplyResources(this.cMSLoadPresets, "cMSLoadPresets");
            this.cMSLoadPresets.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.cMSLoadPresets.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.altTabToolStripMenuItem,
            this.fromFileToolStripMenuItem});
            this.cMSLoadPresets.Name = "cMSLoadPresets";
            this.cMSLoadPresets.ShowImageMargin = false;
            // 
            // altTabToolStripMenuItem
            // 
            resources.ApplyResources(this.altTabToolStripMenuItem, "altTabToolStripMenuItem");
            this.altTabToolStripMenuItem.Name = "altTabToolStripMenuItem";
            this.altTabToolStripMenuItem.Click += new System.EventHandler(this.altTabToolStripMenuItem_Click);
            // 
            // fromFileToolStripMenuItem
            // 
            resources.ApplyResources(this.fromFileToolStripMenuItem, "fromFileToolStripMenuItem");
            this.fromFileToolStripMenuItem.Name = "fromFileToolStripMenuItem";
            this.fromFileToolStripMenuItem.Click += new System.EventHandler(this.fromFileToolStripMenuItem_Click);
            // 
            // RecordBox
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.Controls.Add(this.lbMacroOrder);
            this.Controls.Add(this.cBStyle);
            this.Controls.Add(this.btnSaveP);
            this.Controls.Add(this.cBRecordDelays);
            this.Controls.Add(this.btnLoadP);
            this.Controls.Add(this.lbDelayTip);
            this.Controls.Add(this.lbRecordTip);
            this.Controls.Add(this.btnRecord);
            this.Controls.Add(this.lVMacros);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.pnlMouseButtons);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "RecordBox";
            this.ShowInTaskbar = false;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.RecordBox_FormClosing);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.anyKeyDown);
            this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.anyKeyUp);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.anyMouseDown);
            this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.anyMouseUp);
            this.Resize += new System.EventHandler(this.RecordBox_Resize);
            this.pnlMouseButtons.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pBRtouch)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pBLtouch)).EndInit();
            this.cMSLoadPresets.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnRecord;
        private System.Windows.Forms.CheckBox cBRecordDelays;
        private System.Windows.Forms.ListView lVMacros;
        private System.Windows.Forms.ImageList iLKeys;
        private System.Windows.Forms.ColumnHeader cHMacro;
        private System.Windows.Forms.ComboBox cBStyle;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Panel pnlMouseButtons;
        private System.Windows.Forms.Button btn5th;
        private System.Windows.Forms.Button btn4th;
        private System.Windows.Forms.Label lbRecordTip;
        private System.Windows.Forms.Button btnSaveP;
        private System.Windows.Forms.Button btnLoadP;
        private System.Windows.Forms.SaveFileDialog savePresets;
        private System.Windows.Forms.OpenFileDialog openPresets;
        private System.Windows.Forms.Label lbMacroOrder;
        private System.Windows.Forms.Label lbDelayTip;
        private System.Windows.Forms.ContextMenuStrip cMSLoadPresets;
        private System.Windows.Forms.ToolStripMenuItem altTabToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem fromFileToolStripMenuItem;
        private System.Windows.Forms.Button btnLightbar;
        private System.Windows.Forms.Button btnRumble;
        private System.Windows.Forms.PictureBox pBRtouch;
        private System.Windows.Forms.PictureBox pBLtouch;
    }
}