namespace ScpServer
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
            this.bnStep1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.labelBluetooth = new System.Windows.Forms.Label();
            this.labelUSB = new System.Windows.Forms.Label();
            this.labelBluetooth2 = new System.Windows.Forms.Label();
            this.linkBluetoothSettings = new System.Windows.Forms.LinkLabel();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.bnFinish = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // bnStep1
            // 
            this.bnStep1.Location = new System.Drawing.Point(82, 8);
            this.bnStep1.Name = "bnStep1";
            this.bnStep1.Size = new System.Drawing.Size(155, 23);
            this.bnStep1.TabIndex = 1;
            this.bnStep1.Text = "Step 1: Install the DS4 Driver";
            this.bnStep1.UseVisualStyleBackColor = true;
            this.bnStep1.Click += new System.EventHandler(this.bnStep1_Click);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(29, 57);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(259, 23);
            this.button2.TabIndex = 1;
            this.button2.Text = "Step 2: If on Windows 7 or below, Install 360 Driver";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = global::ScpServer.Properties.Resources.Pairmode;
            this.pictureBox1.InitialImage = null;
            this.pictureBox1.Location = new System.Drawing.Point(36, 197);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(245, 132);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox1.TabIndex = 2;
            this.pictureBox1.TabStop = false;
            // 
            // labelBluetooth
            // 
            this.labelBluetooth.Location = new System.Drawing.Point(1, 155);
            this.labelBluetooth.Name = "labelBluetooth";
            this.labelBluetooth.Size = new System.Drawing.Size(312, 39);
            this.labelBluetooth.TabIndex = 3;
            this.labelBluetooth.Text = "To set up bluetooth (optional):\r\nHold the PS Button and Share for 3 seconds\r\nThe " +
    "lightbar will begin to double flash";
            this.labelBluetooth.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // labelUSB
            // 
            this.labelUSB.Location = new System.Drawing.Point(0, 106);
            this.labelUSB.Name = "labelUSB";
            this.labelUSB.Size = new System.Drawing.Size(313, 39);
            this.labelUSB.TabIndex = 3;
            this.labelUSB.Text = "Step 3: Connecting the DualShock 4 controller\r\nTo set up wired/usb:\r\nSimply plug " +
    "a micro usb into your PC and DualShock 4";
            this.labelUSB.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // labelBluetooth2
            // 
            this.labelBluetooth2.Location = new System.Drawing.Point(1, 332);
            this.labelBluetooth2.Name = "labelBluetooth2";
            this.labelBluetooth2.Size = new System.Drawing.Size(312, 64);
            this.labelBluetooth2.TabIndex = 3;
            this.labelBluetooth2.Text = "Once flashing go to your Bluetooth Settings\r\nand Connect to \"Wireless Controller\"" +
    "\r\n\r\nOnce paired, you\'re ready. Have fun!";
            this.labelBluetooth2.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // linkBluetoothSettings
            // 
            this.linkBluetoothSettings.AutoSize = true;
            this.linkBluetoothSettings.Location = new System.Drawing.Point(169, 332);
            this.linkBluetoothSettings.Name = "linkBluetoothSettings";
            this.linkBluetoothSettings.Size = new System.Drawing.Size(93, 13);
            this.linkBluetoothSettings.TabIndex = 4;
            this.linkBluetoothSettings.TabStop = true;
            this.linkBluetoothSettings.Text = "Bluetooth Settings";
            this.linkBluetoothSettings.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkBluetoothSettings_LinkClicked);
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(1, 32);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(314, 22);
            this.label1.TabIndex = 3;
            this.label1.Text = "Make sure to check Force Install";
            this.label1.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(0, 82);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(315, 22);
            this.label2.TabIndex = 3;
            this.label2.Text = "If you\'ve used a 360 Controller on this PC, you can skip this";
            this.label2.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // bnFinish
            // 
            this.bnFinish.Location = new System.Drawing.Point(117, 394);
            this.bnFinish.Name = "bnFinish";
            this.bnFinish.Size = new System.Drawing.Size(75, 23);
            this.bnFinish.TabIndex = 0;
            this.bnFinish.Text = "Finish";
            this.bnFinish.UseVisualStyleBackColor = true;
            this.bnFinish.Click += new System.EventHandler(this.bnFinish_Click);
            // 
            // WelcomeDialog
            // 
            this.AcceptButton = this.bnFinish;
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(315, 421);
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
            this.Text = "Welcome to DS4Windows";
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
        private System.Windows.Forms.LinkLabel linkBluetoothSettings;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button bnFinish;
    }
}