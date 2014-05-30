namespace ScpServer
{
    partial class MessageTextBox
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
            this.button1 = new System.Windows.Forms.Button();
            this.tBProfile = new System.Windows.Forms.TextBox();
            this.button2 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.button1.Location = new System.Drawing.Point(195, 5);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 2;
            this.button1.Text = "Save";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // tBProfile
            // 
            this.tBProfile.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.tBProfile.ForeColor = System.Drawing.SystemColors.GrayText;
            this.tBProfile.Location = new System.Drawing.Point(12, 7);
            this.tBProfile.Name = "tBProfile";
            this.tBProfile.Size = new System.Drawing.Size(177, 20);
            this.tBProfile.TabIndex = 239;
            this.tBProfile.Text = "<type new name here>";
            this.tBProfile.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.tBProfile.TextChanged += new System.EventHandler(this.tBProfile_TextChanged);
            this.tBProfile.Enter += new System.EventHandler(this.tBProfile_Enter);
            this.tBProfile.Leave += new System.EventHandler(this.tBProfile_Leave);
            // 
            // button2
            // 
            this.button2.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.button2.Location = new System.Drawing.Point(276, 5);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 23);
            this.button2.TabIndex = 240;
            this.button2.Text = "Cancel";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // MessageTextBox
            // 
            this.AcceptButton = this.button1;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(242)))), ((int)(((byte)(242)))), ((int)(((byte)(242)))));
            this.ClientSize = new System.Drawing.Size(359, 35);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.tBProfile);
            this.Controls.Add(this.button1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "MessageTextBox";
            this.ShowInTaskbar = false;
            this.Text = "Type name of New Profile";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.TextBox tBProfile;
        private System.Windows.Forms.Button button2;
    }
}