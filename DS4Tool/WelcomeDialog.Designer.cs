namespace DS4Windows
{
    partial class WelcomeDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(WelcomeDialog));
            this.bnStep1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.labelBluetooth = new System.Windows.Forms.Label();
            this.labelUSB = new System.Windows.Forms.Label();
            this.labelBluetooth2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.bnFinish = new System.Windows.Forms.Button();
            this.linkBluetoothSettings = new System.Windows.Forms.LinkLabel();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // bnStep1
            // 
            resources.ApplyResources(this.bnStep1, "bnStep1");
            this.bnStep1.Name = "bnStep1";
            this.bnStep1.UseVisualStyleBackColor = true;
            this.bnStep1.Click += new System.EventHandler(this.bnStep1_Click);
            // 
            // button2
            // 
            resources.ApplyResources(this.button2, "button2");
            this.button2.Name = "button2";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // pictureBox1
            // 
            resources.ApplyResources(this.pictureBox1, "pictureBox1");
            this.pictureBox1.Image = global::DS4Windows.Properties.Resources.Pairmode;
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.TabStop = false;
            // 
            // labelBluetooth
            // 
            resources.ApplyResources(this.labelBluetooth, "labelBluetooth");
            this.labelBluetooth.Name = "labelBluetooth";
            // 
            // labelUSB
            // 
            resources.ApplyResources(this.labelUSB, "labelUSB");
            this.labelUSB.Name = "labelUSB";
            // 
            // labelBluetooth2
            // 
            resources.ApplyResources(this.labelBluetooth2, "labelBluetooth2");
            this.labelBluetooth2.Name = "labelBluetooth2";
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            // 
            // label2
            // 
            resources.ApplyResources(this.label2, "label2");
            this.label2.Name = "label2";
            // 
            // bnFinish
            // 
            resources.ApplyResources(this.bnFinish, "bnFinish");
            this.bnFinish.Name = "bnFinish";
            this.bnFinish.UseVisualStyleBackColor = true;
            this.bnFinish.Click += new System.EventHandler(this.bnFinish_Click);
            // 
            // linkBluetoothSettings
            // 
            resources.ApplyResources(this.linkBluetoothSettings, "linkBluetoothSettings");
            this.linkBluetoothSettings.Name = "linkBluetoothSettings";
            this.linkBluetoothSettings.TabStop = true;
            this.linkBluetoothSettings.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkBluetoothSettings_LinkClicked);
            // 
            // WelcomeDialog
            // 
            this.AcceptButton = this.bnFinish;
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.Controls.Add(this.linkBluetoothSettings);
            this.Controls.Add(this.labelBluetooth);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.bnStep1);
            this.Controls.Add(this.bnFinish);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.labelUSB);
            this.Controls.Add(this.labelBluetooth2);
            this.Name = "WelcomeDialog";
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button bnStep1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Label labelBluetooth;
        private System.Windows.Forms.Label labelUSB;
        private System.Windows.Forms.Label labelBluetooth2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button bnFinish;
        private System.Windows.Forms.LinkLabel linkBluetoothSettings;
    }
}