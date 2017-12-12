namespace DS4Windows.DS4Forms
{
    partial class LanguagePackComboBox
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

        #region Код, автоматически созданный конструктором компонентов

        /// <summary> 
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.cbCulture = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // cbCulture
            // 
            this.cbCulture.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.cbCulture.DisplayMember = "Value";
            this.cbCulture.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbCulture.FormattingEnabled = true;
            this.cbCulture.Location = new System.Drawing.Point(112, 3);
            this.cbCulture.Name = "cbCulture";
            this.cbCulture.Size = new System.Drawing.Size(145, 21);
            this.cbCulture.TabIndex = 61;
            this.cbCulture.ValueMember = "Key";
            // 
            // label1
            // 
            this.label1.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 6);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(100, 13);
            this.label1.TabIndex = 62;
            this.label1.Text = "Use language pack";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.label1.SizeChanged += new System.EventHandler(this.LanguagePackComboBox_SizeChanged);
            // 
            // LanguagePackComboBox
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.cbCulture);
            this.Controls.Add(this.label1);
            this.Name = "LanguagePackComboBox";
            this.Size = new System.Drawing.Size(260, 27);
            this.SizeChanged += new System.EventHandler(this.LanguagePackComboBox_SizeChanged);
            this.Resize += new System.EventHandler(this.LanguagePackComboBox_SizeChanged);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox cbCulture;
        private System.Windows.Forms.Label label1;
    }
}
