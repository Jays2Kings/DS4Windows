namespace ScpServer
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
            this.label4.AutoSize = true;
            this.label4.ForeColor = System.Drawing.SystemColors.GrayText;
            this.label4.Location = new System.Drawing.Point(206, 83);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(177, 39);
            this.label4.TabIndex = 9;
            this.label4.Text = "For those who prefer a regular install\r\nSettings saved at \r\n%appdata%/ds4tool";
            this.label4.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.ForeColor = System.Drawing.SystemColors.GrayText;
            this.label3.Location = new System.Drawing.Point(5, 83);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(195, 39);
            this.label3.TabIndex = 10;
            this.label3.Text = "For those who prefer a portable program\r\nNote: this option does not\r\n work if in " +
    "an admin folder w/o UAC";
            this.label3.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // bnAppdataFolder
            // 
            this.bnAppdataFolder.Location = new System.Drawing.Point(222, 57);
            this.bnAppdataFolder.Name = "bnAppdataFolder";
            this.bnAppdataFolder.Size = new System.Drawing.Size(140, 23);
            this.bnAppdataFolder.TabIndex = 7;
            this.bnAppdataFolder.Text = "Appdata";
            this.bnAppdataFolder.UseVisualStyleBackColor = true;
            this.bnAppdataFolder.Click += new System.EventHandler(this.bnAppdataFolder_Click);
            // 
            // bnPrgmFolder
            // 
            this.bnPrgmFolder.Location = new System.Drawing.Point(32, 57);
            this.bnPrgmFolder.Name = "bnPrgmFolder";
            this.bnPrgmFolder.Size = new System.Drawing.Size(140, 23);
            this.bnPrgmFolder.TabIndex = 8;
            this.bnPrgmFolder.Text = "Program Folder";
            this.bnPrgmFolder.UseVisualStyleBackColor = true;
            this.bnPrgmFolder.Click += new System.EventHandler(this.bnPrgmFolder_Click);
            // 
            // lbPickWhere
            // 
            this.lbPickWhere.ForeColor = System.Drawing.SystemColors.ControlText;
            this.lbPickWhere.Location = new System.Drawing.Point(1, 27);
            this.lbPickWhere.Name = "lbPickWhere";
            this.lbPickWhere.Size = new System.Drawing.Size(386, 27);
            this.lbPickWhere.TabIndex = 10;
            this.lbPickWhere.Text = "Pick where you want settings and profiles to be saved";
            this.lbPickWhere.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // lbMultiSaves
            // 
            this.lbMultiSaves.AutoSize = true;
            this.lbMultiSaves.ForeColor = System.Drawing.SystemColors.ControlText;
            this.lbMultiSaves.Location = new System.Drawing.Point(9, 8);
            this.lbMultiSaves.Name = "lbMultiSaves";
            this.lbMultiSaves.Size = new System.Drawing.Size(159, 13);
            this.lbMultiSaves.TabIndex = 10;
            this.lbMultiSaves.Text = "Multiple save locations detected\r\n";
            this.lbMultiSaves.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            this.lbMultiSaves.Visible = false;
            // 
            // cBDeleteOther
            // 
            this.cBDeleteOther.AutoSize = true;
            this.cBDeleteOther.Location = new System.Drawing.Point(201, 7);
            this.cBDeleteOther.Name = "cBDeleteOther";
            this.cBDeleteOther.Size = new System.Drawing.Size(186, 17);
            this.cBDeleteOther.TabIndex = 11;
            this.cBDeleteOther.Text = "Don\'t Delete the other settings yet";
            this.cBDeleteOther.UseVisualStyleBackColor = true;
            this.cBDeleteOther.Visible = false;
            // 
            // SaveWhere
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.ClientSize = new System.Drawing.Size(389, 128);
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
            this.Text = "DS4Windows";
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