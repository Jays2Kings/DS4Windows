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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LanguagePackComboBox));
            this.cbCulture = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // cbCulture
            // 
            resources.ApplyResources(this.cbCulture, "cbCulture");
            this.cbCulture.DisplayMember = "Value";
            this.cbCulture.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbCulture.FormattingEnabled = true;
            this.cbCulture.Name = "cbCulture";
            this.cbCulture.ValueMember = "Key";
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            this.label1.SizeChanged += new System.EventHandler(this.LanguagePackComboBox_SizeChanged);
            // 
            // LanguagePackComboBox
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.cbCulture);
            this.Controls.Add(this.label1);
            this.Name = "LanguagePackComboBox";
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
