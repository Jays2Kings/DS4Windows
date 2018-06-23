namespace DS4Windows
{
    partial class WinProgs
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(WinProgs));
            this.bnAddPrograms = new System.Windows.Forms.Button();
            this.lBProgramPath = new System.Windows.Forms.Label();
            this.cBProfile1 = new System.Windows.Forms.ComboBox();
            this.cBProfile2 = new System.Windows.Forms.ComboBox();
            this.cBProfile3 = new System.Windows.Forms.ComboBox();
            this.cBProfile4 = new System.Windows.Forms.ComboBox();
            this.bnSave = new System.Windows.Forms.Button();
            this.lBController1 = new System.Windows.Forms.Label();
            this.lBController2 = new System.Windows.Forms.Label();
            this.lBController3 = new System.Windows.Forms.Label();
            this.lBController4 = new System.Windows.Forms.Label();
            this.openProgram = new System.Windows.Forms.OpenFileDialog();
            this.bnDelete = new System.Windows.Forms.Button();
            this.iLIcons = new System.Windows.Forms.ImageList(this.components);
            this.lVPrograms = new System.Windows.Forms.ListView();
            this.nameHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.PathHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.pBProfilesTip = new System.Windows.Forms.Label();
            this.bnHideUnchecked = new System.Windows.Forms.Button();
            this.cMSPrograms = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.addProgramsFromStartMenuToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.addSteamGamesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.addOriginGamesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.addDirectoryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.browseForOtherProgramsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.cBTurnOffDS4W = new System.Windows.Forms.CheckBox();
            this.cMSPrograms.SuspendLayout();
            this.SuspendLayout();
            // 
            // bnAddPrograms
            // 
            resources.ApplyResources(this.bnAddPrograms, "bnAddPrograms");
            this.bnAddPrograms.Name = "bnAddPrograms";
            this.bnAddPrograms.UseVisualStyleBackColor = true;
            this.bnAddPrograms.Click += new System.EventHandler(this.bnAddPrograms_Click);
            // 
            // lBProgramPath
            // 
            resources.ApplyResources(this.lBProgramPath, "lBProgramPath");
            this.lBProgramPath.Name = "lBProgramPath";
            this.lBProgramPath.TextChanged += new System.EventHandler(this.lBProgramPath_TextChanged);
            // 
            // cBProfile1
            // 
            resources.ApplyResources(this.cBProfile1, "cBProfile1");
            this.cBProfile1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cBProfile1.FormattingEnabled = true;
            this.cBProfile1.Name = "cBProfile1";
            this.cBProfile1.SelectedIndexChanged += new System.EventHandler(this.CBProfile_IndexChanged);
            // 
            // cBProfile2
            // 
            resources.ApplyResources(this.cBProfile2, "cBProfile2");
            this.cBProfile2.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cBProfile2.FormattingEnabled = true;
            this.cBProfile2.Name = "cBProfile2";
            this.cBProfile2.SelectedIndexChanged += new System.EventHandler(this.CBProfile_IndexChanged);
            // 
            // cBProfile3
            // 
            resources.ApplyResources(this.cBProfile3, "cBProfile3");
            this.cBProfile3.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cBProfile3.FormattingEnabled = true;
            this.cBProfile3.Name = "cBProfile3";
            this.cBProfile3.SelectedIndexChanged += new System.EventHandler(this.CBProfile_IndexChanged);
            // 
            // cBProfile4
            // 
            resources.ApplyResources(this.cBProfile4, "cBProfile4");
            this.cBProfile4.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cBProfile4.FormattingEnabled = true;
            this.cBProfile4.Name = "cBProfile4";
            this.cBProfile4.SelectedIndexChanged += new System.EventHandler(this.CBProfile_IndexChanged);
            // 
            // bnSave
            // 
            resources.ApplyResources(this.bnSave, "bnSave");
            this.bnSave.Name = "bnSave";
            this.bnSave.UseVisualStyleBackColor = true;
            this.bnSave.Click += new System.EventHandler(this.bnSave_Click);
            // 
            // lBController1
            // 
            resources.ApplyResources(this.lBController1, "lBController1");
            this.lBController1.Name = "lBController1";
            // 
            // lBController2
            // 
            resources.ApplyResources(this.lBController2, "lBController2");
            this.lBController2.Name = "lBController2";
            // 
            // lBController3
            // 
            resources.ApplyResources(this.lBController3, "lBController3");
            this.lBController3.Name = "lBController3";
            // 
            // lBController4
            // 
            resources.ApplyResources(this.lBController4, "lBController4");
            this.lBController4.Name = "lBController4";
            // 
            // openProgram
            // 
            resources.ApplyResources(this.openProgram, "openProgram");
            // 
            // bnDelete
            // 
            resources.ApplyResources(this.bnDelete, "bnDelete");
            this.bnDelete.Name = "bnDelete";
            this.bnDelete.UseVisualStyleBackColor = true;
            this.bnDelete.Click += new System.EventHandler(this.bnDelete_Click);
            // 
            // iLIcons
            // 
            this.iLIcons.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
            resources.ApplyResources(this.iLIcons, "iLIcons");
            this.iLIcons.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // lVPrograms
            // 
            resources.ApplyResources(this.lVPrograms, "lVPrograms");
            this.lVPrograms.CheckBoxes = true;
            this.lVPrograms.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.nameHeader,
            this.PathHeader});
            this.lVPrograms.FullRowSelect = true;
            this.lVPrograms.HideSelection = false;
            this.lVPrograms.LargeImageList = this.iLIcons;
            this.lVPrograms.MultiSelect = false;
            this.lVPrograms.Name = "lVPrograms";
            this.lVPrograms.ShowItemToolTips = true;
            this.lVPrograms.SmallImageList = this.iLIcons;
            this.lVPrograms.UseCompatibleStateImageBehavior = false;
            this.lVPrograms.View = System.Windows.Forms.View.Details;
            this.lVPrograms.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.listView1_ItemCheck);
            this.lVPrograms.SelectedIndexChanged += new System.EventHandler(this.lBProgramPath_SelectedIndexChanged);
            // 
            // nameHeader
            // 
            resources.ApplyResources(this.nameHeader, "nameHeader");
            // 
            // PathHeader
            // 
            resources.ApplyResources(this.PathHeader, "PathHeader");
            // 
            // pBProfilesTip
            // 
            resources.ApplyResources(this.pBProfilesTip, "pBProfilesTip");
            this.pBProfilesTip.BackColor = System.Drawing.Color.Transparent;
            this.pBProfilesTip.ForeColor = System.Drawing.SystemColors.GrayText;
            this.pBProfilesTip.Name = "pBProfilesTip";
            // 
            // bnHideUnchecked
            // 
            resources.ApplyResources(this.bnHideUnchecked, "bnHideUnchecked");
            this.bnHideUnchecked.Name = "bnHideUnchecked";
            this.bnHideUnchecked.UseVisualStyleBackColor = true;
            this.bnHideUnchecked.Click += new System.EventHandler(this.bnHideUnchecked_Click);
            // 
            // cMSPrograms
            // 
            this.cMSPrograms.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.cMSPrograms.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.addProgramsFromStartMenuToolStripMenuItem,
            this.addSteamGamesToolStripMenuItem,
            this.addOriginGamesToolStripMenuItem,
            this.addDirectoryToolStripMenuItem,
            this.browseForOtherProgramsToolStripMenuItem});
            this.cMSPrograms.Name = "contextMenuStrip1";
            this.cMSPrograms.ShowImageMargin = false;
            resources.ApplyResources(this.cMSPrograms, "cMSPrograms");
            // 
            // addProgramsFromStartMenuToolStripMenuItem
            // 
            this.addProgramsFromStartMenuToolStripMenuItem.Name = "addProgramsFromStartMenuToolStripMenuItem";
            resources.ApplyResources(this.addProgramsFromStartMenuToolStripMenuItem, "addProgramsFromStartMenuToolStripMenuItem");
            this.addProgramsFromStartMenuToolStripMenuItem.Click += new System.EventHandler(this.addProgramsFromStartMenuToolStripMenuItem_Click);
            // 
            // addSteamGamesToolStripMenuItem
            // 
            this.addSteamGamesToolStripMenuItem.Name = "addSteamGamesToolStripMenuItem";
            resources.ApplyResources(this.addSteamGamesToolStripMenuItem, "addSteamGamesToolStripMenuItem");
            this.addSteamGamesToolStripMenuItem.Click += new System.EventHandler(this.addSteamGamesToolStripMenuItem_Click);
            // 
            // addOriginGamesToolStripMenuItem
            // 
            this.addOriginGamesToolStripMenuItem.Name = "addOriginGamesToolStripMenuItem";
            resources.ApplyResources(this.addOriginGamesToolStripMenuItem, "addOriginGamesToolStripMenuItem");
            this.addOriginGamesToolStripMenuItem.Click += new System.EventHandler(this.addOriginGamesToolStripMenuItem_Click);
            // 
            // addDirectoryToolStripMenuItem
            // 
            this.addDirectoryToolStripMenuItem.Name = "addDirectoryToolStripMenuItem";
            resources.ApplyResources(this.addDirectoryToolStripMenuItem, "addDirectoryToolStripMenuItem");
            this.addDirectoryToolStripMenuItem.Click += new System.EventHandler(this.addDirectoryToolStripMenuItem_Click);
            // 
            // browseForOtherProgramsToolStripMenuItem
            // 
            this.browseForOtherProgramsToolStripMenuItem.Name = "browseForOtherProgramsToolStripMenuItem";
            resources.ApplyResources(this.browseForOtherProgramsToolStripMenuItem, "browseForOtherProgramsToolStripMenuItem");
            this.browseForOtherProgramsToolStripMenuItem.Click += new System.EventHandler(this.browseForOtherProgramsToolStripMenuItem_Click);
            // 
            // cBTurnOffDS4W
            // 
            resources.ApplyResources(this.cBTurnOffDS4W, "cBTurnOffDS4W");
            this.cBTurnOffDS4W.Name = "cBTurnOffDS4W";
            this.cBTurnOffDS4W.UseVisualStyleBackColor = true;
            this.cBTurnOffDS4W.CheckedChanged += new System.EventHandler(this.cBTurnOffDS4W_CheckedChanged);
            // 
            // WinProgs
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.Controls.Add(this.cBTurnOffDS4W);
            this.Controls.Add(this.pBProfilesTip);
            this.Controls.Add(this.bnHideUnchecked);
            this.Controls.Add(this.bnAddPrograms);
            this.Controls.Add(this.lVPrograms);
            this.Controls.Add(this.lBController4);
            this.Controls.Add(this.lBController3);
            this.Controls.Add(this.lBController2);
            this.Controls.Add(this.lBController1);
            this.Controls.Add(this.cBProfile4);
            this.Controls.Add(this.cBProfile3);
            this.Controls.Add(this.cBProfile2);
            this.Controls.Add(this.cBProfile1);
            this.Controls.Add(this.lBProgramPath);
            this.Controls.Add(this.bnDelete);
            this.Controls.Add(this.bnSave);
            this.Name = "WinProgs";
            this.cMSPrograms.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button bnAddPrograms;
        private System.Windows.Forms.Label lBProgramPath;
        private System.Windows.Forms.ComboBox cBProfile1;
        private System.Windows.Forms.ComboBox cBProfile2;
        private System.Windows.Forms.ComboBox cBProfile3;
        private System.Windows.Forms.ComboBox cBProfile4;
        private System.Windows.Forms.Button bnSave;
        private System.Windows.Forms.Label lBController1;
        private System.Windows.Forms.Label lBController2;
        private System.Windows.Forms.Label lBController3;
        private System.Windows.Forms.Label lBController4;
        private System.Windows.Forms.OpenFileDialog openProgram;
        private System.Windows.Forms.Button bnDelete;
        private System.Windows.Forms.ImageList iLIcons;
        private System.Windows.Forms.ListView lVPrograms;
        private System.Windows.Forms.ColumnHeader nameHeader;
        private System.Windows.Forms.ColumnHeader PathHeader;
        private System.Windows.Forms.Label pBProfilesTip;
        private System.Windows.Forms.Button bnHideUnchecked;
        private System.Windows.Forms.ContextMenuStrip cMSPrograms;
        private System.Windows.Forms.ToolStripMenuItem addProgramsFromStartMenuToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem addSteamGamesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem addOriginGamesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem browseForOtherProgramsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem addDirectoryToolStripMenuItem;
        private System.Windows.Forms.CheckBox cBTurnOffDS4W;
    }
}