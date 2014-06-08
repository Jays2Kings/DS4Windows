namespace ScpServer
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
            this.lBBeta = new System.Windows.Forms.Label();
            this.openProgram = new System.Windows.Forms.OpenFileDialog();
            this.bnDelete = new System.Windows.Forms.Button();
            this.iLIcons = new System.Windows.Forms.ImageList(this.components);
            this.lVPrograms = new System.Windows.Forms.ListView();
            this.nameHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.PathHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.lBTip = new System.Windows.Forms.Label();
            this.pBProfilesTip = new System.Windows.Forms.Label();
            this.bnHideUnchecked = new System.Windows.Forms.Button();
            this.cMSPrograms = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.addProgramsFromStartMenuToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.addSteamGamesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.addOriginGamesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.browseForOtherProgramsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.cMSPrograms.SuspendLayout();
            this.SuspendLayout();
            // 
            // bnAddPrograms
            // 
            this.bnAddPrograms.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.bnAddPrograms.Location = new System.Drawing.Point(516, 195);
            this.bnAddPrograms.Name = "bnAddPrograms";
            this.bnAddPrograms.Size = new System.Drawing.Size(114, 23);
            this.bnAddPrograms.TabIndex = 2;
            this.bnAddPrograms.Text = "Add programs";
            this.bnAddPrograms.UseVisualStyleBackColor = true;
            this.bnAddPrograms.Click += new System.EventHandler(this.bnAddPrograms_Click);
            // 
            // lBProgramPath
            // 
            this.lBProgramPath.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lBProgramPath.Location = new System.Drawing.Point(602, 131);
            this.lBProgramPath.Name = "lBProgramPath";
            this.lBProgramPath.Size = new System.Drawing.Size(47, 18);
            this.lBProgramPath.TabIndex = 3;
            this.lBProgramPath.Visible = false;
            this.lBProgramPath.TextChanged += new System.EventHandler(this.lBProgramPath_TextChanged);
            // 
            // cBProfile1
            // 
            this.cBProfile1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cBProfile1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cBProfile1.FormattingEnabled = true;
            this.cBProfile1.Location = new System.Drawing.Point(609, 18);
            this.cBProfile1.Name = "cBProfile1";
            this.cBProfile1.Size = new System.Drawing.Size(121, 21);
            this.cBProfile1.TabIndex = 6;
            this.cBProfile1.SelectedIndexChanged += new System.EventHandler(this.CBProfile_IndexChanged);
            // 
            // cBProfile2
            // 
            this.cBProfile2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cBProfile2.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cBProfile2.FormattingEnabled = true;
            this.cBProfile2.Location = new System.Drawing.Point(609, 45);
            this.cBProfile2.Name = "cBProfile2";
            this.cBProfile2.Size = new System.Drawing.Size(121, 21);
            this.cBProfile2.TabIndex = 6;
            this.cBProfile2.SelectedIndexChanged += new System.EventHandler(this.CBProfile_IndexChanged);
            // 
            // cBProfile3
            // 
            this.cBProfile3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cBProfile3.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cBProfile3.FormattingEnabled = true;
            this.cBProfile3.Location = new System.Drawing.Point(609, 72);
            this.cBProfile3.Name = "cBProfile3";
            this.cBProfile3.Size = new System.Drawing.Size(121, 21);
            this.cBProfile3.TabIndex = 6;
            this.cBProfile3.SelectedIndexChanged += new System.EventHandler(this.CBProfile_IndexChanged);
            // 
            // cBProfile4
            // 
            this.cBProfile4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cBProfile4.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cBProfile4.FormattingEnabled = true;
            this.cBProfile4.Location = new System.Drawing.Point(609, 99);
            this.cBProfile4.Name = "cBProfile4";
            this.cBProfile4.Size = new System.Drawing.Size(121, 21);
            this.cBProfile4.TabIndex = 6;
            this.cBProfile4.SelectedIndexChanged += new System.EventHandler(this.CBProfile_IndexChanged);
            // 
            // bnSave
            // 
            this.bnSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.bnSave.Enabled = false;
            this.bnSave.Location = new System.Drawing.Point(663, 126);
            this.bnSave.Name = "bnSave";
            this.bnSave.Size = new System.Drawing.Size(67, 23);
            this.bnSave.TabIndex = 2;
            this.bnSave.Text = "Save";
            this.bnSave.UseVisualStyleBackColor = true;
            this.bnSave.Click += new System.EventHandler(this.bnSave_Click);
            // 
            // lBController1
            // 
            this.lBController1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lBController1.AutoSize = true;
            this.lBController1.Location = new System.Drawing.Point(516, 21);
            this.lBController1.Name = "lBController1";
            this.lBController1.Size = new System.Drawing.Size(60, 13);
            this.lBController1.TabIndex = 7;
            this.lBController1.Text = "Controller 1";
            // 
            // lBController2
            // 
            this.lBController2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lBController2.AutoSize = true;
            this.lBController2.Location = new System.Drawing.Point(516, 48);
            this.lBController2.Name = "lBController2";
            this.lBController2.Size = new System.Drawing.Size(60, 13);
            this.lBController2.TabIndex = 7;
            this.lBController2.Text = "Controller 2";
            // 
            // lBController3
            // 
            this.lBController3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lBController3.AutoSize = true;
            this.lBController3.Location = new System.Drawing.Point(516, 75);
            this.lBController3.Name = "lBController3";
            this.lBController3.Size = new System.Drawing.Size(60, 13);
            this.lBController3.TabIndex = 7;
            this.lBController3.Text = "Controller 3";
            // 
            // lBController4
            // 
            this.lBController4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lBController4.AutoSize = true;
            this.lBController4.Location = new System.Drawing.Point(516, 102);
            this.lBController4.Name = "lBController4";
            this.lBController4.Size = new System.Drawing.Size(60, 13);
            this.lBController4.TabIndex = 7;
            this.lBController4.Text = "Controller 4";
            // 
            // lBBeta
            // 
            this.lBBeta.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lBBeta.AutoSize = true;
            this.lBBeta.BackColor = System.Drawing.Color.Transparent;
            this.lBBeta.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lBBeta.ForeColor = System.Drawing.SystemColors.GrayText;
            this.lBBeta.Location = new System.Drawing.Point(606, 175);
            this.lBBeta.Name = "lBBeta";
            this.lBBeta.Size = new System.Drawing.Size(38, 18);
            this.lBBeta.TabIndex = 8;
            this.lBBeta.Text = "Beta";
            this.lBBeta.Visible = false;
            // 
            // openProgram
            // 
            this.openProgram.FileName = "openFileDialog1";
            this.openProgram.Filter = "Programs|*.exe|Shortcuts|*.lnk";
            // 
            // bnDelete
            // 
            this.bnDelete.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.bnDelete.Location = new System.Drawing.Point(519, 126);
            this.bnDelete.Name = "bnDelete";
            this.bnDelete.Size = new System.Drawing.Size(67, 23);
            this.bnDelete.TabIndex = 2;
            this.bnDelete.Text = "Remove";
            this.bnDelete.UseVisualStyleBackColor = true;
            this.bnDelete.Click += new System.EventHandler(this.bnDelete_Click);
            // 
            // iLIcons
            // 
            this.iLIcons.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
            this.iLIcons.ImageSize = new System.Drawing.Size(16, 16);
            this.iLIcons.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // lVPrograms
            // 
            this.lVPrograms.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lVPrograms.CheckBoxes = true;
            this.lVPrograms.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.nameHeader,
            this.PathHeader});
            this.lVPrograms.FullRowSelect = true;
            this.lVPrograms.LargeImageList = this.iLIcons;
            this.lVPrograms.Location = new System.Drawing.Point(5, 6);
            this.lVPrograms.MultiSelect = false;
            this.lVPrograms.Name = "lVPrograms";
            this.lVPrograms.Size = new System.Drawing.Size(505, 212);
            this.lVPrograms.SmallImageList = this.iLIcons;
            this.lVPrograms.TabIndex = 12;
            this.lVPrograms.UseCompatibleStateImageBehavior = false;
            this.lVPrograms.View = System.Windows.Forms.View.Details;
            this.lVPrograms.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.listView1_ItemCheck);
            this.lVPrograms.SelectedIndexChanged += new System.EventHandler(this.lBProgramPath_SelectedIndexChanged);
            // 
            // nameHeader
            // 
            this.nameHeader.Text = "Name";
            this.nameHeader.Width = 140;
            // 
            // PathHeader
            // 
            this.PathHeader.Text = "Path";
            this.PathHeader.Width = 358;
            // 
            // lBTip
            // 
            this.lBTip.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lBTip.AutoSize = true;
            this.lBTip.BackColor = System.Drawing.Color.Transparent;
            this.lBTip.ForeColor = System.Drawing.SystemColors.GrayText;
            this.lBTip.Location = new System.Drawing.Point(529, 152);
            this.lBTip.Name = "lBTip";
            this.lBTip.Size = new System.Drawing.Size(191, 13);
            this.lBTip.TabIndex = 8;
            this.lBTip.Text = "Pick a program, then profiles, and save";
            this.lBTip.Visible = false;
            // 
            // pBProfilesTip
            // 
            this.pBProfilesTip.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.pBProfilesTip.AutoSize = true;
            this.pBProfilesTip.BackColor = System.Drawing.Color.Transparent;
            this.pBProfilesTip.ForeColor = System.Drawing.SystemColors.GrayText;
            this.pBProfilesTip.Location = new System.Drawing.Point(620, 2);
            this.pBProfilesTip.Name = "pBProfilesTip";
            this.pBProfilesTip.Size = new System.Drawing.Size(89, 13);
            this.pBProfilesTip.TabIndex = 8;
            this.pBProfilesTip.Text = "Pick Profiles here";
            // 
            // bnHideUnchecked
            // 
            this.bnHideUnchecked.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.bnHideUnchecked.Location = new System.Drawing.Point(636, 195);
            this.bnHideUnchecked.Name = "bnHideUnchecked";
            this.bnHideUnchecked.Size = new System.Drawing.Size(94, 23);
            this.bnHideUnchecked.TabIndex = 2;
            this.bnHideUnchecked.Text = "Hide unchecked";
            this.bnHideUnchecked.UseVisualStyleBackColor = true;
            this.bnHideUnchecked.Click += new System.EventHandler(this.bnHideUnchecked_Click);
            // 
            // cMSPrograms
            // 
            this.cMSPrograms.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.addProgramsFromStartMenuToolStripMenuItem,
            this.addSteamGamesToolStripMenuItem,
            this.addOriginGamesToolStripMenuItem,
            this.browseForOtherProgramsToolStripMenuItem});
            this.cMSPrograms.Name = "contextMenuStrip1";
            this.cMSPrograms.ShowImageMargin = false;
            this.cMSPrograms.Size = new System.Drawing.Size(193, 92);
            // 
            // addProgramsFromStartMenuToolStripMenuItem
            // 
            this.addProgramsFromStartMenuToolStripMenuItem.Name = "addProgramsFromStartMenuToolStripMenuItem";
            this.addProgramsFromStartMenuToolStripMenuItem.Size = new System.Drawing.Size(192, 22);
            this.addProgramsFromStartMenuToolStripMenuItem.Text = "Add Start Menu Programs";
            this.addProgramsFromStartMenuToolStripMenuItem.Click += new System.EventHandler(this.addProgramsFromStartMenuToolStripMenuItem_Click);
            // 
            // addSteamGamesToolStripMenuItem
            // 
            this.addSteamGamesToolStripMenuItem.Name = "addSteamGamesToolStripMenuItem";
            this.addSteamGamesToolStripMenuItem.Size = new System.Drawing.Size(192, 22);
            this.addSteamGamesToolStripMenuItem.Text = "Add Steam Games";
            this.addSteamGamesToolStripMenuItem.Click += new System.EventHandler(this.addSteamGamesToolStripMenuItem_Click);
            // 
            // addOriginGamesToolStripMenuItem
            // 
            this.addOriginGamesToolStripMenuItem.Name = "addOriginGamesToolStripMenuItem";
            this.addOriginGamesToolStripMenuItem.Size = new System.Drawing.Size(192, 22);
            this.addOriginGamesToolStripMenuItem.Text = "Add Origin Games";
            this.addOriginGamesToolStripMenuItem.Click += new System.EventHandler(this.addOriginGamesToolStripMenuItem_Click);
            // 
            // browseForOtherProgramsToolStripMenuItem
            // 
            this.browseForOtherProgramsToolStripMenuItem.Name = "browseForOtherProgramsToolStripMenuItem";
            this.browseForOtherProgramsToolStripMenuItem.Size = new System.Drawing.Size(192, 22);
            this.browseForOtherProgramsToolStripMenuItem.Text = "Browse for Other Programs";
            this.browseForOtherProgramsToolStripMenuItem.Click += new System.EventHandler(this.browseForOtherProgramsToolStripMenuItem_Click);
            // 
            // WinProgs
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(736, 222);
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
            this.Controls.Add(this.pBProfilesTip);
            this.Controls.Add(this.lBTip);
            this.Controls.Add(this.lBBeta);
            this.Name = "WinProgs";
            this.Text = "Auto-Profiles";
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
        private System.Windows.Forms.Label lBBeta;
        private System.Windows.Forms.OpenFileDialog openProgram;
        private System.Windows.Forms.Button bnDelete;
        private System.Windows.Forms.ImageList iLIcons;
        private System.Windows.Forms.ListView lVPrograms;
        private System.Windows.Forms.ColumnHeader nameHeader;
        private System.Windows.Forms.ColumnHeader PathHeader;
        private System.Windows.Forms.Label lBTip;
        private System.Windows.Forms.Label pBProfilesTip;
        private System.Windows.Forms.Button bnHideUnchecked;
        private System.Windows.Forms.ContextMenuStrip cMSPrograms;
        private System.Windows.Forms.ToolStripMenuItem addProgramsFromStartMenuToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem addSteamGamesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem addOriginGamesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem browseForOtherProgramsToolStripMenuItem;
    }
}