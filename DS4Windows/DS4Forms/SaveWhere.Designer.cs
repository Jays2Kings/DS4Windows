namespace DS4Windows
{
    partial class SaveWhere
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SaveWhere));
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.bnAppdataFolder = new System.Windows.Forms.Button();
            this.bnPrgmFolder = new System.Windows.Forms.Button();
            this.lbPickWhere = new System.Windows.Forms.Label();
            this.lbMultiSaves = new System.Windows.Forms.Label();
            this.cBDeleteOther = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // label4
            // 
            this.label4.ForeColor = System.Drawing.SystemColors.GrayText;
            resources.ApplyResources(this.label4, "label4");
            this.label4.Name = "label4";
            // 
            // label3
            // 
            this.label3.ForeColor = System.Drawing.SystemColors.GrayText;
            resources.ApplyResources(this.label3, "label3");
            this.label3.Name = "label3";
            // 
            // bnAppdataFolder
            // 
            resources.ApplyResources(this.bnAppdataFolder, "bnAppdataFolder");
            this.bnAppdataFolder.Name = "bnAppdataFolder";
            this.bnAppdataFolder.UseVisualStyleBackColor = true;
            this.bnAppdataFolder.Click += new System.EventHandler(this.bnAppdataFolder_Click);
            // 
            // bnPrgmFolder
            // 
            resources.ApplyResources(this.bnPrgmFolder, "bnPrgmFolder");
            this.bnPrgmFolder.Name = "bnPrgmFolder";
            this.bnPrgmFolder.UseVisualStyleBackColor = true;
            this.bnPrgmFolder.Click += new System.EventHandler(this.bnPrgmFolder_Click);
            // 
            // lbPickWhere
            // 
            resources.ApplyResources(this.lbPickWhere, "lbPickWhere");
            this.lbPickWhere.ForeColor = System.Drawing.SystemColors.ControlText;
            this.lbPickWhere.Name = "lbPickWhere";
            // 
            // lbMultiSaves
            // 
            resources.ApplyResources(this.lbMultiSaves, "lbMultiSaves");
            this.lbMultiSaves.ForeColor = System.Drawing.SystemColors.ControlText;
            this.lbMultiSaves.Name = "lbMultiSaves";
            // 
            // cBDeleteOther
            // 
            resources.ApplyResources(this.cBDeleteOther, "cBDeleteOther");
            this.cBDeleteOther.Name = "cBDeleteOther";
            this.cBDeleteOther.UseVisualStyleBackColor = true;
            // 
            // SaveWhere
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.Controls.Add(this.cBDeleteOther);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.lbMultiSaves);
            this.Controls.Add(this.lbPickWhere);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.bnAppdataFolder);
            this.Controls.Add(this.bnPrgmFolder);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SaveWhere";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.SaveWhere_FormClosing);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button bnAppdataFolder;
        private System.Windows.Forms.Button bnPrgmFolder;
        private System.Windows.Forms.Label lbPickWhere;
        private System.Windows.Forms.Label lbMultiSaves;
        private System.Windows.Forms.CheckBox cBDeleteOther;
    }
}