namespace DS4Windows
{
    partial class SpecActions
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SpecActions));
            this.lVTrigger = new System.Windows.Forms.ListView();
            this.cHTrigger = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.btnRecordMacro = new System.Windows.Forms.Button();
            this.btnBrowse = new System.Windows.Forms.Button();
            this.cBProfiles = new System.Windows.Forms.ComboBox();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.lbName = new System.Windows.Forms.Label();
            this.tBName = new System.Windows.Forms.TextBox();
            this.cBActions = new System.Windows.Forms.ComboBox();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.pBProgram = new System.Windows.Forms.PictureBox();
            this.lbProgram = new System.Windows.Forms.Label();
            this.btnBorder = new System.Windows.Forms.Button();
            this.btnSetUTriggerProfile = new System.Windows.Forms.Button();
            this.lVUnloadTrigger = new System.Windows.Forms.ListView();
            this.cHUnloadTrigger = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.pnlProgram = new System.Windows.Forms.Panel();
            this.pnlMacro = new System.Windows.Forms.Panel();
            this.cBMacroScanCode = new System.Windows.Forms.CheckBox();
            this.lbMacroRecorded = new System.Windows.Forms.Label();
            this.pnlProfile = new System.Windows.Forms.Panel();
            this.lbUnloadTip = new System.Windows.Forms.Label();
            this.pnlDisconnectBT = new System.Windows.Forms.Panel();
            this.nUDDCBT = new System.Windows.Forms.NumericUpDown();
            this.lbHoldFor = new System.Windows.Forms.Label();
            this.lbSecs = new System.Windows.Forms.Label();
            this.pnlKeys = new System.Windows.Forms.Panel();
            this.btnSelectKey = new System.Windows.Forms.Button();
            this.cBPressRelease = new System.Windows.Forms.ComboBox();
            this.btnSetUTriggerKeys = new System.Windows.Forms.Button();
            this.lbUnloadTipKey = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.pBProgram)).BeginInit();
            this.pnlProgram.SuspendLayout();
            this.pnlMacro.SuspendLayout();
            this.pnlProfile.SuspendLayout();
            this.pnlDisconnectBT.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nUDDCBT)).BeginInit();
            this.pnlKeys.SuspendLayout();
            this.SuspendLayout();
            // 
            // lVTrigger
            // 
            resources.ApplyResources(this.lVTrigger, "lVTrigger");
            this.lVTrigger.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.lVTrigger.CheckBoxes = true;
            this.lVTrigger.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.cHTrigger});
            this.lVTrigger.FullRowSelect = true;
            this.lVTrigger.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.lVTrigger.HideSelection = false;
            this.lVTrigger.Items.AddRange(new System.Windows.Forms.ListViewItem[] {
            ((System.Windows.Forms.ListViewItem)(resources.GetObject("lVTrigger.Items"))),
            ((System.Windows.Forms.ListViewItem)(resources.GetObject("lVTrigger.Items1"))),
            ((System.Windows.Forms.ListViewItem)(resources.GetObject("lVTrigger.Items2"))),
            ((System.Windows.Forms.ListViewItem)(resources.GetObject("lVTrigger.Items3"))),
            ((System.Windows.Forms.ListViewItem)(resources.GetObject("lVTrigger.Items4"))),
            ((System.Windows.Forms.ListViewItem)(resources.GetObject("lVTrigger.Items5"))),
            ((System.Windows.Forms.ListViewItem)(resources.GetObject("lVTrigger.Items6"))),
            ((System.Windows.Forms.ListViewItem)(resources.GetObject("lVTrigger.Items7"))),
            ((System.Windows.Forms.ListViewItem)(resources.GetObject("lVTrigger.Items8"))),
            ((System.Windows.Forms.ListViewItem)(resources.GetObject("lVTrigger.Items9"))),
            ((System.Windows.Forms.ListViewItem)(resources.GetObject("lVTrigger.Items10"))),
            ((System.Windows.Forms.ListViewItem)(resources.GetObject("lVTrigger.Items11"))),
            ((System.Windows.Forms.ListViewItem)(resources.GetObject("lVTrigger.Items12"))),
            ((System.Windows.Forms.ListViewItem)(resources.GetObject("lVTrigger.Items13"))),
            ((System.Windows.Forms.ListViewItem)(resources.GetObject("lVTrigger.Items14"))),
            ((System.Windows.Forms.ListViewItem)(resources.GetObject("lVTrigger.Items15"))),
            ((System.Windows.Forms.ListViewItem)(resources.GetObject("lVTrigger.Items16"))),
            ((System.Windows.Forms.ListViewItem)(resources.GetObject("lVTrigger.Items17"))),
            ((System.Windows.Forms.ListViewItem)(resources.GetObject("lVTrigger.Items18"))),
            ((System.Windows.Forms.ListViewItem)(resources.GetObject("lVTrigger.Items19"))),
            ((System.Windows.Forms.ListViewItem)(resources.GetObject("lVTrigger.Items20"))),
            ((System.Windows.Forms.ListViewItem)(resources.GetObject("lVTrigger.Items21"))),
            ((System.Windows.Forms.ListViewItem)(resources.GetObject("lVTrigger.Items22"))),
            ((System.Windows.Forms.ListViewItem)(resources.GetObject("lVTrigger.Items23"))),
            ((System.Windows.Forms.ListViewItem)(resources.GetObject("lVTrigger.Items24"))),
            ((System.Windows.Forms.ListViewItem)(resources.GetObject("lVTrigger.Items25"))),
            ((System.Windows.Forms.ListViewItem)(resources.GetObject("lVTrigger.Items26"))),
            ((System.Windows.Forms.ListViewItem)(resources.GetObject("lVTrigger.Items27"))),
            ((System.Windows.Forms.ListViewItem)(resources.GetObject("lVTrigger.Items28"))),
            ((System.Windows.Forms.ListViewItem)(resources.GetObject("lVTrigger.Items29"))),
            ((System.Windows.Forms.ListViewItem)(resources.GetObject("lVTrigger.Items30"))),
            ((System.Windows.Forms.ListViewItem)(resources.GetObject("lVTrigger.Items31"))),
            ((System.Windows.Forms.ListViewItem)(resources.GetObject("lVTrigger.Items32")))});
            this.lVTrigger.MultiSelect = false;
            this.lVTrigger.Name = "lVTrigger";
            this.lVTrigger.ShowGroups = false;
            this.lVTrigger.ShowItemToolTips = true;
            this.lVTrigger.UseCompatibleStateImageBehavior = false;
            this.lVTrigger.View = System.Windows.Forms.View.Details;
            this.lVTrigger.ItemChecked += new System.Windows.Forms.ItemCheckedEventHandler(this.lVTrigger_ItemChecked);
            // 
            // cHTrigger
            // 
            resources.ApplyResources(this.cHTrigger, "cHTrigger");
            // 
            // btnRecordMacro
            // 
            resources.ApplyResources(this.btnRecordMacro, "btnRecordMacro");
            this.btnRecordMacro.Name = "btnRecordMacro";
            this.btnRecordMacro.UseVisualStyleBackColor = true;
            this.btnRecordMacro.Click += new System.EventHandler(this.btnRecordMacro_Click);
            // 
            // btnBrowse
            // 
            resources.ApplyResources(this.btnBrowse, "btnBrowse");
            this.btnBrowse.Name = "btnBrowse";
            this.btnBrowse.UseVisualStyleBackColor = true;
            this.btnBrowse.Click += new System.EventHandler(this.btnBroswe_Click);
            // 
            // cBProfiles
            // 
            resources.ApplyResources(this.cBProfiles, "cBProfiles");
            this.cBProfiles.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cBProfiles.FormattingEnabled = true;
            this.cBProfiles.Name = "cBProfiles";
            this.cBProfiles.SelectedIndexChanged += new System.EventHandler(this.lVUnloadTrigger_SelectedIndexChanged);
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
            // lbName
            // 
            resources.ApplyResources(this.lbName, "lbName");
            this.lbName.Name = "lbName";
            // 
            // tBName
            // 
            resources.ApplyResources(this.tBName, "tBName");
            this.tBName.Name = "tBName";
            this.tBName.TextChanged += new System.EventHandler(this.tBName_TextChanged);
            // 
            // cBActions
            // 
            resources.ApplyResources(this.cBActions, "cBActions");
            this.cBActions.Cursor = System.Windows.Forms.Cursors.Default;
            this.cBActions.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cBActions.FormattingEnabled = true;
            this.cBActions.Items.AddRange(new object[] {
            resources.GetString("cBActions.Items"),
            resources.GetString("cBActions.Items1"),
            resources.GetString("cBActions.Items2"),
            resources.GetString("cBActions.Items3"),
            resources.GetString("cBActions.Items4"),
            resources.GetString("cBActions.Items5")});
            this.cBActions.Name = "cBActions";
            this.cBActions.SelectedIndexChanged += new System.EventHandler(this.cBActions_SelectedIndexChanged);
            // 
            // openFileDialog1
            // 
            resources.ApplyResources(this.openFileDialog1, "openFileDialog1");
            // 
            // imageList1
            // 
            this.imageList1.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
            resources.ApplyResources(this.imageList1, "imageList1");
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // pBProgram
            // 
            resources.ApplyResources(this.pBProgram, "pBProgram");
            this.pBProgram.Name = "pBProgram";
            this.pBProgram.TabStop = false;
            // 
            // lbProgram
            // 
            resources.ApplyResources(this.lbProgram, "lbProgram");
            this.lbProgram.Name = "lbProgram";
            // 
            // btnBorder
            // 
            resources.ApplyResources(this.btnBorder, "btnBorder");
            this.btnBorder.BackColor = System.Drawing.SystemColors.ControlDark;
            this.btnBorder.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.btnBorder.Name = "btnBorder";
            this.btnBorder.UseVisualStyleBackColor = false;
            // 
            // btnSetUTriggerProfile
            // 
            resources.ApplyResources(this.btnSetUTriggerProfile, "btnSetUTriggerProfile");
            this.btnSetUTriggerProfile.Name = "btnSetUTriggerProfile";
            this.btnSetUTriggerProfile.UseVisualStyleBackColor = true;
            this.btnSetUTriggerProfile.Click += new System.EventHandler(this.btnSetUTrigger_Click);
            // 
            // lVUnloadTrigger
            // 
            resources.ApplyResources(this.lVUnloadTrigger, "lVUnloadTrigger");
            this.lVUnloadTrigger.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.lVUnloadTrigger.CheckBoxes = true;
            this.lVUnloadTrigger.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.cHUnloadTrigger});
            this.lVUnloadTrigger.FullRowSelect = true;
            this.lVUnloadTrigger.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.lVUnloadTrigger.HideSelection = false;
            this.lVUnloadTrigger.Items.AddRange(new System.Windows.Forms.ListViewItem[] {
            ((System.Windows.Forms.ListViewItem)(resources.GetObject("lVUnloadTrigger.Items"))),
            ((System.Windows.Forms.ListViewItem)(resources.GetObject("lVUnloadTrigger.Items1"))),
            ((System.Windows.Forms.ListViewItem)(resources.GetObject("lVUnloadTrigger.Items2"))),
            ((System.Windows.Forms.ListViewItem)(resources.GetObject("lVUnloadTrigger.Items3"))),
            ((System.Windows.Forms.ListViewItem)(resources.GetObject("lVUnloadTrigger.Items4"))),
            ((System.Windows.Forms.ListViewItem)(resources.GetObject("lVUnloadTrigger.Items5"))),
            ((System.Windows.Forms.ListViewItem)(resources.GetObject("lVUnloadTrigger.Items6"))),
            ((System.Windows.Forms.ListViewItem)(resources.GetObject("lVUnloadTrigger.Items7"))),
            ((System.Windows.Forms.ListViewItem)(resources.GetObject("lVUnloadTrigger.Items8"))),
            ((System.Windows.Forms.ListViewItem)(resources.GetObject("lVUnloadTrigger.Items9"))),
            ((System.Windows.Forms.ListViewItem)(resources.GetObject("lVUnloadTrigger.Items10"))),
            ((System.Windows.Forms.ListViewItem)(resources.GetObject("lVUnloadTrigger.Items11"))),
            ((System.Windows.Forms.ListViewItem)(resources.GetObject("lVUnloadTrigger.Items12"))),
            ((System.Windows.Forms.ListViewItem)(resources.GetObject("lVUnloadTrigger.Items13"))),
            ((System.Windows.Forms.ListViewItem)(resources.GetObject("lVUnloadTrigger.Items14"))),
            ((System.Windows.Forms.ListViewItem)(resources.GetObject("lVUnloadTrigger.Items15"))),
            ((System.Windows.Forms.ListViewItem)(resources.GetObject("lVUnloadTrigger.Items16"))),
            ((System.Windows.Forms.ListViewItem)(resources.GetObject("lVUnloadTrigger.Items17"))),
            ((System.Windows.Forms.ListViewItem)(resources.GetObject("lVUnloadTrigger.Items18"))),
            ((System.Windows.Forms.ListViewItem)(resources.GetObject("lVUnloadTrigger.Items19"))),
            ((System.Windows.Forms.ListViewItem)(resources.GetObject("lVUnloadTrigger.Items20"))),
            ((System.Windows.Forms.ListViewItem)(resources.GetObject("lVUnloadTrigger.Items21"))),
            ((System.Windows.Forms.ListViewItem)(resources.GetObject("lVUnloadTrigger.Items22"))),
            ((System.Windows.Forms.ListViewItem)(resources.GetObject("lVUnloadTrigger.Items23"))),
            ((System.Windows.Forms.ListViewItem)(resources.GetObject("lVUnloadTrigger.Items24"))),
            ((System.Windows.Forms.ListViewItem)(resources.GetObject("lVUnloadTrigger.Items25"))),
            ((System.Windows.Forms.ListViewItem)(resources.GetObject("lVUnloadTrigger.Items26"))),
            ((System.Windows.Forms.ListViewItem)(resources.GetObject("lVUnloadTrigger.Items27"))),
            ((System.Windows.Forms.ListViewItem)(resources.GetObject("lVUnloadTrigger.Items28"))),
            ((System.Windows.Forms.ListViewItem)(resources.GetObject("lVUnloadTrigger.Items29"))),
            ((System.Windows.Forms.ListViewItem)(resources.GetObject("lVUnloadTrigger.Items30"))),
            ((System.Windows.Forms.ListViewItem)(resources.GetObject("lVUnloadTrigger.Items31"))),
            ((System.Windows.Forms.ListViewItem)(resources.GetObject("lVUnloadTrigger.Items32")))});
            this.lVUnloadTrigger.MultiSelect = false;
            this.lVUnloadTrigger.Name = "lVUnloadTrigger";
            this.lVUnloadTrigger.ShowGroups = false;
            this.lVUnloadTrigger.ShowItemToolTips = true;
            this.lVUnloadTrigger.UseCompatibleStateImageBehavior = false;
            this.lVUnloadTrigger.View = System.Windows.Forms.View.Details;
            this.lVUnloadTrigger.ItemChecked += new System.Windows.Forms.ItemCheckedEventHandler(this.lVUnloadTrigger_ItemChecked);
            // 
            // cHUnloadTrigger
            // 
            resources.ApplyResources(this.cHUnloadTrigger, "cHUnloadTrigger");
            // 
            // pnlProgram
            // 
            resources.ApplyResources(this.pnlProgram, "pnlProgram");
            this.pnlProgram.Controls.Add(this.btnBrowse);
            this.pnlProgram.Controls.Add(this.lbProgram);
            this.pnlProgram.Controls.Add(this.pBProgram);
            this.pnlProgram.Name = "pnlProgram";
            // 
            // pnlMacro
            // 
            resources.ApplyResources(this.pnlMacro, "pnlMacro");
            this.pnlMacro.Controls.Add(this.cBMacroScanCode);
            this.pnlMacro.Controls.Add(this.btnRecordMacro);
            this.pnlMacro.Controls.Add(this.lbMacroRecorded);
            this.pnlMacro.Name = "pnlMacro";
            // 
            // cBMacroScanCode
            // 
            resources.ApplyResources(this.cBMacroScanCode, "cBMacroScanCode");
            this.cBMacroScanCode.Name = "cBMacroScanCode";
            this.cBMacroScanCode.UseVisualStyleBackColor = true;
            // 
            // lbMacroRecorded
            // 
            resources.ApplyResources(this.lbMacroRecorded, "lbMacroRecorded");
            this.lbMacroRecorded.Name = "lbMacroRecorded";
            // 
            // pnlProfile
            // 
            resources.ApplyResources(this.pnlProfile, "pnlProfile");
            this.pnlProfile.Controls.Add(this.lbUnloadTip);
            this.pnlProfile.Controls.Add(this.cBProfiles);
            this.pnlProfile.Controls.Add(this.btnSetUTriggerProfile);
            this.pnlProfile.Name = "pnlProfile";
            // 
            // lbUnloadTip
            // 
            resources.ApplyResources(this.lbUnloadTip, "lbUnloadTip");
            this.lbUnloadTip.Name = "lbUnloadTip";
            // 
            // pnlDisconnectBT
            // 
            resources.ApplyResources(this.pnlDisconnectBT, "pnlDisconnectBT");
            this.pnlDisconnectBT.Controls.Add(this.nUDDCBT);
            this.pnlDisconnectBT.Controls.Add(this.lbHoldFor);
            this.pnlDisconnectBT.Controls.Add(this.lbSecs);
            this.pnlDisconnectBT.Name = "pnlDisconnectBT";
            // 
            // nUDDCBT
            // 
            resources.ApplyResources(this.nUDDCBT, "nUDDCBT");
            this.nUDDCBT.DecimalPlaces = 1;
            this.nUDDCBT.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.nUDDCBT.Name = "nUDDCBT";
            // 
            // lbHoldFor
            // 
            resources.ApplyResources(this.lbHoldFor, "lbHoldFor");
            this.lbHoldFor.Name = "lbHoldFor";
            // 
            // lbSecs
            // 
            resources.ApplyResources(this.lbSecs, "lbSecs");
            this.lbSecs.Name = "lbSecs";
            // 
            // pnlKeys
            // 
            resources.ApplyResources(this.pnlKeys, "pnlKeys");
            this.pnlKeys.Controls.Add(this.btnSelectKey);
            this.pnlKeys.Controls.Add(this.cBPressRelease);
            this.pnlKeys.Controls.Add(this.btnSetUTriggerKeys);
            this.pnlKeys.Controls.Add(this.lbUnloadTipKey);
            this.pnlKeys.Name = "pnlKeys";
            // 
            // btnSelectKey
            // 
            resources.ApplyResources(this.btnSelectKey, "btnSelectKey");
            this.btnSelectKey.Name = "btnSelectKey";
            this.btnSelectKey.UseVisualStyleBackColor = true;
            this.btnSelectKey.TextChanged += new System.EventHandler(this.btnSelectKey_TextChanged);
            this.btnSelectKey.Click += new System.EventHandler(this.btnSelectKey_Click);
            // 
            // cBPressRelease
            // 
            resources.ApplyResources(this.cBPressRelease, "cBPressRelease");
            this.cBPressRelease.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cBPressRelease.FormattingEnabled = true;
            this.cBPressRelease.Items.AddRange(new object[] {
            resources.GetString("cBPressRelease.Items"),
            resources.GetString("cBPressRelease.Items1")});
            this.cBPressRelease.Name = "cBPressRelease";
            // 
            // btnSetUTriggerKeys
            // 
            resources.ApplyResources(this.btnSetUTriggerKeys, "btnSetUTriggerKeys");
            this.btnSetUTriggerKeys.Name = "btnSetUTriggerKeys";
            this.btnSetUTriggerKeys.UseVisualStyleBackColor = true;
            this.btnSetUTriggerKeys.Click += new System.EventHandler(this.btnSetUTrigger_Click);
            // 
            // lbUnloadTipKey
            // 
            resources.ApplyResources(this.lbUnloadTipKey, "lbUnloadTipKey");
            this.lbUnloadTipKey.Name = "lbUnloadTipKey";
            // 
            // SpecActions
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.Controls.Add(this.tBName);
            this.Controls.Add(this.cBActions);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.lbName);
            this.Controls.Add(this.lVTrigger);
            this.Controls.Add(this.lVUnloadTrigger);
            this.Controls.Add(this.btnBorder);
            this.Controls.Add(this.pnlKeys);
            this.Controls.Add(this.pnlProgram);
            this.Controls.Add(this.pnlDisconnectBT);
            this.Controls.Add(this.pnlMacro);
            this.Controls.Add(this.pnlProfile);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "SpecActions";
            ((System.ComponentModel.ISupportInitialize)(this.pBProgram)).EndInit();
            this.pnlProgram.ResumeLayout(false);
            this.pnlMacro.ResumeLayout(false);
            this.pnlMacro.PerformLayout();
            this.pnlProfile.ResumeLayout(false);
            this.pnlDisconnectBT.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.nUDDCBT)).EndInit();
            this.pnlKeys.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListView lVTrigger;
        private System.Windows.Forms.ColumnHeader cHTrigger;
        private System.Windows.Forms.Button btnRecordMacro;
        private System.Windows.Forms.Button btnBrowse;
        private System.Windows.Forms.ComboBox cBProfiles;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Label lbName;
        private System.Windows.Forms.TextBox tBName;
        private System.Windows.Forms.ComboBox cBActions;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.ImageList imageList1;
        private System.Windows.Forms.PictureBox pBProgram;
        private System.Windows.Forms.Label lbProgram;
        private System.Windows.Forms.Button btnBorder;
        private System.Windows.Forms.Button btnSetUTriggerProfile;
        private System.Windows.Forms.ListView lVUnloadTrigger;
        private System.Windows.Forms.ColumnHeader cHUnloadTrigger;
        private System.Windows.Forms.Panel pnlProgram;
        private System.Windows.Forms.Panel pnlMacro;
        private System.Windows.Forms.Panel pnlProfile;
        public System.Windows.Forms.Label lbMacroRecorded;
        private System.Windows.Forms.Label lbUnloadTip;
        private System.Windows.Forms.CheckBox cBMacroScanCode;
        private System.Windows.Forms.Panel pnlDisconnectBT;
        private System.Windows.Forms.NumericUpDown nUDDCBT;
        private System.Windows.Forms.Label lbSecs;
        private System.Windows.Forms.Label lbHoldFor;
        private System.Windows.Forms.Panel pnlKeys;
        private System.Windows.Forms.Label lbUnloadTipKey;
        private System.Windows.Forms.Button btnSetUTriggerKeys;
        private System.Windows.Forms.Button btnSelectKey;
        private System.Windows.Forms.ComboBox cBPressRelease;
    }
}