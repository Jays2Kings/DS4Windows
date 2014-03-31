namespace ScpServer
{
    partial class CustomMapping
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
            this.pictureBox = new System.Windows.Forms.PictureBox();
            this.btnLoad = new System.Windows.Forms.Button();
            this.btnSave = new System.Windows.Forms.Button();
            this.cbRepeat = new System.Windows.Forms.CheckBox();
            this.cbScanCode = new System.Windows.Forms.CheckBox();
            this.bnCross = new System.Windows.Forms.Button();
            this.bnCircle = new System.Windows.Forms.Button();
            this.bnSquare = new System.Windows.Forms.Button();
            this.bnTriangle = new System.Windows.Forms.Button();
            this.bnR1 = new System.Windows.Forms.Button();
            this.bnR2 = new System.Windows.Forms.Button();
            this.bnL1 = new System.Windows.Forms.Button();
            this.bnL2 = new System.Windows.Forms.Button();
            this.bnUp = new System.Windows.Forms.Button();
            this.bnDown = new System.Windows.Forms.Button();
            this.bnRight = new System.Windows.Forms.Button();
            this.bnLeft = new System.Windows.Forms.Button();
            this.bnOptions = new System.Windows.Forms.Button();
            this.bnShare = new System.Windows.Forms.Button();
            this.bnTouchpad = new System.Windows.Forms.Button();
            this.bnPS = new System.Windows.Forms.Button();
            this.bnTouchUpper = new System.Windows.Forms.Button();
            this.bnTouchMulti = new System.Windows.Forms.Button();
            this.bnLY = new System.Windows.Forms.Button();
            this.lbControls = new System.Windows.Forms.ListBox();
            this.bnLY2 = new System.Windows.Forms.Button();
            this.bnRY = new System.Windows.Forms.Button();
            this.bnRY2 = new System.Windows.Forms.Button();
            this.bnLX = new System.Windows.Forms.Button();
            this.bnLX2 = new System.Windows.Forms.Button();
            this.bnRX = new System.Windows.Forms.Button();
            this.bnRX2 = new System.Windows.Forms.Button();
            this.bnL3 = new System.Windows.Forms.Button();
            this.bnR3 = new System.Windows.Forms.Button();
            this.TouchTip = new System.Windows.Forms.Label();
            this.ReapTip = new System.Windows.Forms.Label();
            this.lbMode = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox)).BeginInit();
            this.SuspendLayout();
            // 
            // pictureBox
            // 
            this.pictureBox.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.pictureBox.Enabled = false;
            this.pictureBox.Image = global::ScpServer.Properties.Resources._1;
            this.pictureBox.Location = new System.Drawing.Point(16, 67);
            this.pictureBox.Name = "pictureBox";
            this.pictureBox.Size = new System.Drawing.Size(396, 214);
            this.pictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox.TabIndex = 0;
            this.pictureBox.TabStop = false;
            // 
            // btnLoad
            // 
            this.btnLoad.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnLoad.Location = new System.Drawing.Point(530, 12);
            this.btnLoad.Name = "btnLoad";
            this.btnLoad.Size = new System.Drawing.Size(65, 21);
            this.btnLoad.TabIndex = 24;
            this.btnLoad.Text = "Load";
            this.btnLoad.UseVisualStyleBackColor = true;
            this.btnLoad.Click += new System.EventHandler(this.btnLoad_Click);
            // 
            // btnSave
            // 
            this.btnSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSave.Location = new System.Drawing.Point(601, 12);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(65, 21);
            this.btnSave.TabIndex = 25;
            this.btnSave.Text = "Save";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // cbRepeat
            // 
            this.cbRepeat.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.cbRepeat.AutoSize = true;
            this.cbRepeat.ForeColor = System.Drawing.SystemColors.ControlText;
            this.cbRepeat.Location = new System.Drawing.Point(19, 12);
            this.cbRepeat.Name = "cbRepeat";
            this.cbRepeat.Size = new System.Drawing.Size(61, 17);
            this.cbRepeat.TabIndex = 26;
            this.cbRepeat.Text = "Repeat";
            this.cbRepeat.UseVisualStyleBackColor = true;
            this.cbRepeat.CheckedChanged += new System.EventHandler(this.cbRepeat_CheckedChanged);
            // 
            // cbScanCode
            // 
            this.cbScanCode.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.cbScanCode.AutoSize = true;
            this.cbScanCode.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cbScanCode.Location = new System.Drawing.Point(103, 12);
            this.cbScanCode.Name = "cbScanCode";
            this.cbScanCode.Size = new System.Drawing.Size(79, 17);
            this.cbScanCode.TabIndex = 50;
            this.cbScanCode.Text = "Scan Code";
            this.cbScanCode.UseVisualStyleBackColor = true;
            this.cbScanCode.CheckedChanged += new System.EventHandler(this.cbScanCode_CheckedChanged);
            // 
            // bnCross
            // 
            this.bnCross.BackColor = System.Drawing.Color.Transparent;
            this.bnCross.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.bnCross.Cursor = System.Windows.Forms.Cursors.Default;
            this.bnCross.FlatAppearance.BorderColor = System.Drawing.Color.Red;
            this.bnCross.FlatAppearance.BorderSize = 0;
            this.bnCross.FlatAppearance.MouseDownBackColor = System.Drawing.SystemColors.Control;
            this.bnCross.FlatAppearance.MouseOverBackColor = System.Drawing.SystemColors.Control;
            this.bnCross.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.bnCross.ForeColor = System.Drawing.SystemColors.WindowText;
            this.bnCross.Location = new System.Drawing.Point(323, 190);
            this.bnCross.Name = "bnCross";
            this.bnCross.Size = new System.Drawing.Size(23, 23);
            this.bnCross.TabIndex = 53;
            this.bnCross.Text = "A Button";
            this.bnCross.UseVisualStyleBackColor = false;
            // 
            // bnCircle
            // 
            this.bnCircle.BackColor = System.Drawing.Color.Transparent;
            this.bnCircle.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.bnCircle.Cursor = System.Windows.Forms.Cursors.Default;
            this.bnCircle.FlatAppearance.BorderColor = System.Drawing.Color.Red;
            this.bnCircle.FlatAppearance.BorderSize = 0;
            this.bnCircle.FlatAppearance.MouseDownBackColor = System.Drawing.SystemColors.Control;
            this.bnCircle.FlatAppearance.MouseOverBackColor = System.Drawing.SystemColors.Control;
            this.bnCircle.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.bnCircle.ForeColor = System.Drawing.SystemColors.WindowText;
            this.bnCircle.Location = new System.Drawing.Point(350, 166);
            this.bnCircle.Name = "bnCircle";
            this.bnCircle.Size = new System.Drawing.Size(23, 23);
            this.bnCircle.TabIndex = 53;
            this.bnCircle.Text = "B Button";
            this.bnCircle.UseVisualStyleBackColor = false;
            // 
            // bnSquare
            // 
            this.bnSquare.BackColor = System.Drawing.Color.Transparent;
            this.bnSquare.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.bnSquare.Cursor = System.Windows.Forms.Cursors.Default;
            this.bnSquare.FlatAppearance.BorderColor = System.Drawing.Color.Red;
            this.bnSquare.FlatAppearance.BorderSize = 0;
            this.bnSquare.FlatAppearance.MouseDownBackColor = System.Drawing.SystemColors.Control;
            this.bnSquare.FlatAppearance.MouseOverBackColor = System.Drawing.SystemColors.Control;
            this.bnSquare.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.bnSquare.ForeColor = System.Drawing.SystemColors.WindowText;
            this.bnSquare.Location = new System.Drawing.Point(294, 166);
            this.bnSquare.Name = "bnSquare";
            this.bnSquare.Size = new System.Drawing.Size(23, 23);
            this.bnSquare.TabIndex = 53;
            this.bnSquare.Text = "X Button";
            this.bnSquare.UseVisualStyleBackColor = false;
            // 
            // bnTriangle
            // 
            this.bnTriangle.BackColor = System.Drawing.Color.Transparent;
            this.bnTriangle.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.bnTriangle.Cursor = System.Windows.Forms.Cursors.Default;
            this.bnTriangle.FlatAppearance.BorderColor = System.Drawing.Color.Red;
            this.bnTriangle.FlatAppearance.BorderSize = 0;
            this.bnTriangle.FlatAppearance.MouseDownBackColor = System.Drawing.SystemColors.Control;
            this.bnTriangle.FlatAppearance.MouseOverBackColor = System.Drawing.SystemColors.Control;
            this.bnTriangle.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.bnTriangle.ForeColor = System.Drawing.SystemColors.WindowText;
            this.bnTriangle.Location = new System.Drawing.Point(322, 140);
            this.bnTriangle.Name = "bnTriangle";
            this.bnTriangle.Size = new System.Drawing.Size(23, 23);
            this.bnTriangle.TabIndex = 53;
            this.bnTriangle.Text = "Y Button";
            this.bnTriangle.UseVisualStyleBackColor = false;
            // 
            // bnR1
            // 
            this.bnR1.BackColor = System.Drawing.Color.Transparent;
            this.bnR1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.bnR1.Cursor = System.Windows.Forms.Cursors.Default;
            this.bnR1.FlatAppearance.BorderColor = System.Drawing.Color.Red;
            this.bnR1.FlatAppearance.BorderSize = 0;
            this.bnR1.FlatAppearance.MouseDownBackColor = System.Drawing.SystemColors.Control;
            this.bnR1.FlatAppearance.MouseOverBackColor = System.Drawing.SystemColors.Control;
            this.bnR1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.bnR1.ForeColor = System.Drawing.SystemColors.WindowText;
            this.bnR1.Location = new System.Drawing.Point(310, 90);
            this.bnR1.Name = "bnR1";
            this.bnR1.Size = new System.Drawing.Size(43, 15);
            this.bnR1.TabIndex = 53;
            this.bnR1.Text = "Right Bumper";
            this.bnR1.UseVisualStyleBackColor = false;
            // 
            // bnR2
            // 
            this.bnR2.BackColor = System.Drawing.Color.Transparent;
            this.bnR2.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.bnR2.Cursor = System.Windows.Forms.Cursors.Default;
            this.bnR2.FlatAppearance.BorderColor = System.Drawing.Color.Red;
            this.bnR2.FlatAppearance.BorderSize = 0;
            this.bnR2.FlatAppearance.MouseDownBackColor = System.Drawing.SystemColors.Control;
            this.bnR2.FlatAppearance.MouseOverBackColor = System.Drawing.SystemColors.Control;
            this.bnR2.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.bnR2.ForeColor = System.Drawing.SystemColors.WindowText;
            this.bnR2.Location = new System.Drawing.Point(309, 65);
            this.bnR2.Name = "bnR2";
            this.bnR2.Size = new System.Drawing.Size(43, 20);
            this.bnR2.TabIndex = 53;
            this.bnR2.Text = "Right Trigger";
            this.bnR2.UseVisualStyleBackColor = false;
            // 
            // bnL1
            // 
            this.bnL1.BackColor = System.Drawing.Color.Transparent;
            this.bnL1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.bnL1.Cursor = System.Windows.Forms.Cursors.Default;
            this.bnL1.FlatAppearance.BorderColor = System.Drawing.Color.Red;
            this.bnL1.FlatAppearance.BorderSize = 0;
            this.bnL1.FlatAppearance.MouseDownBackColor = System.Drawing.SystemColors.Control;
            this.bnL1.FlatAppearance.MouseOverBackColor = System.Drawing.SystemColors.Control;
            this.bnL1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.bnL1.ForeColor = System.Drawing.SystemColors.WindowText;
            this.bnL1.Location = new System.Drawing.Point(73, 88);
            this.bnL1.Name = "bnL1";
            this.bnL1.Size = new System.Drawing.Size(43, 15);
            this.bnL1.TabIndex = 53;
            this.bnL1.Text = "Left Bumper";
            this.bnL1.UseVisualStyleBackColor = false;
            // 
            // bnL2
            // 
            this.bnL2.BackColor = System.Drawing.Color.Transparent;
            this.bnL2.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.bnL2.Cursor = System.Windows.Forms.Cursors.Default;
            this.bnL2.FlatAppearance.BorderColor = System.Drawing.Color.Red;
            this.bnL2.FlatAppearance.BorderSize = 0;
            this.bnL2.FlatAppearance.MouseDownBackColor = System.Drawing.SystemColors.Control;
            this.bnL2.FlatAppearance.MouseOverBackColor = System.Drawing.SystemColors.Control;
            this.bnL2.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.bnL2.ForeColor = System.Drawing.SystemColors.WindowText;
            this.bnL2.Location = new System.Drawing.Point(76, 66);
            this.bnL2.Name = "bnL2";
            this.bnL2.Size = new System.Drawing.Size(43, 20);
            this.bnL2.TabIndex = 53;
            this.bnL2.Text = "Left Trigger";
            this.bnL2.UseVisualStyleBackColor = false;
            // 
            // bnUp
            // 
            this.bnUp.BackColor = System.Drawing.Color.Transparent;
            this.bnUp.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.bnUp.Cursor = System.Windows.Forms.Cursors.Default;
            this.bnUp.FlatAppearance.BorderColor = System.Drawing.Color.Red;
            this.bnUp.FlatAppearance.BorderSize = 0;
            this.bnUp.FlatAppearance.MouseDownBackColor = System.Drawing.SystemColors.Control;
            this.bnUp.FlatAppearance.MouseOverBackColor = System.Drawing.SystemColors.Control;
            this.bnUp.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.bnUp.ForeColor = System.Drawing.SystemColors.WindowText;
            this.bnUp.Location = new System.Drawing.Point(84, 142);
            this.bnUp.Name = "bnUp";
            this.bnUp.Size = new System.Drawing.Size(19, 22);
            this.bnUp.TabIndex = 53;
            this.bnUp.Text = "Up Button";
            this.bnUp.UseVisualStyleBackColor = false;
            // 
            // bnDown
            // 
            this.bnDown.BackColor = System.Drawing.Color.Transparent;
            this.bnDown.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.bnDown.Cursor = System.Windows.Forms.Cursors.Default;
            this.bnDown.FlatAppearance.BorderColor = System.Drawing.Color.Red;
            this.bnDown.FlatAppearance.BorderSize = 0;
            this.bnDown.FlatAppearance.MouseDownBackColor = System.Drawing.SystemColors.Control;
            this.bnDown.FlatAppearance.MouseOverBackColor = System.Drawing.SystemColors.Control;
            this.bnDown.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.bnDown.ForeColor = System.Drawing.SystemColors.WindowText;
            this.bnDown.Location = new System.Drawing.Point(85, 184);
            this.bnDown.Name = "bnDown";
            this.bnDown.Size = new System.Drawing.Size(19, 29);
            this.bnDown.TabIndex = 53;
            this.bnDown.Text = "Down Button";
            this.bnDown.UseVisualStyleBackColor = false;
            // 
            // bnRight
            // 
            this.bnRight.BackColor = System.Drawing.Color.Transparent;
            this.bnRight.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.bnRight.Cursor = System.Windows.Forms.Cursors.Default;
            this.bnRight.FlatAppearance.BorderColor = System.Drawing.Color.Red;
            this.bnRight.FlatAppearance.BorderSize = 0;
            this.bnRight.FlatAppearance.MouseDownBackColor = System.Drawing.SystemColors.Control;
            this.bnRight.FlatAppearance.MouseOverBackColor = System.Drawing.SystemColors.Control;
            this.bnRight.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.bnRight.ForeColor = System.Drawing.SystemColors.WindowText;
            this.bnRight.Location = new System.Drawing.Point(106, 164);
            this.bnRight.Name = "bnRight";
            this.bnRight.Size = new System.Drawing.Size(27, 22);
            this.bnRight.TabIndex = 53;
            this.bnRight.Text = "Right Button";
            this.bnRight.UseVisualStyleBackColor = false;
            // 
            // bnLeft
            // 
            this.bnLeft.BackColor = System.Drawing.Color.Transparent;
            this.bnLeft.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.bnLeft.Cursor = System.Windows.Forms.Cursors.Default;
            this.bnLeft.FlatAppearance.BorderColor = System.Drawing.Color.Red;
            this.bnLeft.FlatAppearance.BorderSize = 0;
            this.bnLeft.FlatAppearance.MouseDownBackColor = System.Drawing.SystemColors.Control;
            this.bnLeft.FlatAppearance.MouseOverBackColor = System.Drawing.SystemColors.Control;
            this.bnLeft.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.bnLeft.ForeColor = System.Drawing.SystemColors.WindowText;
            this.bnLeft.Location = new System.Drawing.Point(57, 163);
            this.bnLeft.Name = "bnLeft";
            this.bnLeft.Size = new System.Drawing.Size(26, 23);
            this.bnLeft.TabIndex = 53;
            this.bnLeft.Text = "Left Button";
            this.bnLeft.UseVisualStyleBackColor = false;
            // 
            // bnOptions
            // 
            this.bnOptions.BackColor = System.Drawing.Color.Transparent;
            this.bnOptions.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.bnOptions.Cursor = System.Windows.Forms.Cursors.Default;
            this.bnOptions.FlatAppearance.BorderColor = System.Drawing.Color.Red;
            this.bnOptions.FlatAppearance.BorderSize = 0;
            this.bnOptions.FlatAppearance.MouseDownBackColor = System.Drawing.SystemColors.Control;
            this.bnOptions.FlatAppearance.MouseOverBackColor = System.Drawing.SystemColors.Control;
            this.bnOptions.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.bnOptions.ForeColor = System.Drawing.SystemColors.WindowText;
            this.bnOptions.Location = new System.Drawing.Point(286, 121);
            this.bnOptions.Name = "bnOptions";
            this.bnOptions.Size = new System.Drawing.Size(19, 30);
            this.bnOptions.TabIndex = 53;
            this.bnOptions.Text = "Start";
            this.bnOptions.UseVisualStyleBackColor = false;
            // 
            // bnShare
            // 
            this.bnShare.BackColor = System.Drawing.Color.Transparent;
            this.bnShare.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.bnShare.Cursor = System.Windows.Forms.Cursors.Default;
            this.bnShare.FlatAppearance.BorderColor = System.Drawing.Color.Red;
            this.bnShare.FlatAppearance.BorderSize = 0;
            this.bnShare.FlatAppearance.MouseDownBackColor = System.Drawing.SystemColors.Control;
            this.bnShare.FlatAppearance.MouseOverBackColor = System.Drawing.SystemColors.Control;
            this.bnShare.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.bnShare.ForeColor = System.Drawing.SystemColors.WindowText;
            this.bnShare.Location = new System.Drawing.Point(131, 124);
            this.bnShare.Name = "bnShare";
            this.bnShare.Size = new System.Drawing.Size(14, 29);
            this.bnShare.TabIndex = 53;
            this.bnShare.Text = "Back";
            this.bnShare.UseVisualStyleBackColor = false;
            // 
            // bnTouchpad
            // 
            this.bnTouchpad.BackColor = System.Drawing.Color.Transparent;
            this.bnTouchpad.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.bnTouchpad.Cursor = System.Windows.Forms.Cursors.Default;
            this.bnTouchpad.FlatAppearance.BorderColor = System.Drawing.Color.Red;
            this.bnTouchpad.FlatAppearance.BorderSize = 0;
            this.bnTouchpad.FlatAppearance.MouseDownBackColor = System.Drawing.SystemColors.Control;
            this.bnTouchpad.FlatAppearance.MouseOverBackColor = System.Drawing.SystemColors.Control;
            this.bnTouchpad.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.bnTouchpad.ForeColor = System.Drawing.SystemColors.WindowText;
            this.bnTouchpad.Location = new System.Drawing.Point(151, 135);
            this.bnTouchpad.Name = "bnTouchpad";
            this.bnTouchpad.Size = new System.Drawing.Size(64, 52);
            this.bnTouchpad.TabIndex = 53;
            this.bnTouchpad.Text = "Click";
            this.bnTouchpad.UseVisualStyleBackColor = false;
            // 
            // bnPS
            // 
            this.bnPS.BackColor = System.Drawing.Color.Transparent;
            this.bnPS.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.bnPS.Cursor = System.Windows.Forms.Cursors.Default;
            this.bnPS.FlatAppearance.BorderColor = System.Drawing.Color.Red;
            this.bnPS.FlatAppearance.BorderSize = 0;
            this.bnPS.FlatAppearance.MouseDownBackColor = System.Drawing.SystemColors.Control;
            this.bnPS.FlatAppearance.MouseOverBackColor = System.Drawing.SystemColors.Control;
            this.bnPS.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.bnPS.ForeColor = System.Drawing.SystemColors.WindowText;
            this.bnPS.Location = new System.Drawing.Point(205, 208);
            this.bnPS.Name = "bnPS";
            this.bnPS.Size = new System.Drawing.Size(18, 18);
            this.bnPS.TabIndex = 53;
            this.bnPS.Text = "Guide";
            this.bnPS.UseVisualStyleBackColor = false;
            // 
            // bnTouchUpper
            // 
            this.bnTouchUpper.BackColor = System.Drawing.Color.Transparent;
            this.bnTouchUpper.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.bnTouchUpper.Cursor = System.Windows.Forms.Cursors.Default;
            this.bnTouchUpper.FlatAppearance.BorderColor = System.Drawing.Color.Red;
            this.bnTouchUpper.FlatAppearance.BorderSize = 0;
            this.bnTouchUpper.FlatAppearance.MouseDownBackColor = System.Drawing.SystemColors.Control;
            this.bnTouchUpper.FlatAppearance.MouseOverBackColor = System.Drawing.SystemColors.Control;
            this.bnTouchUpper.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.bnTouchUpper.ForeColor = System.Drawing.SystemColors.WindowText;
            this.bnTouchUpper.Location = new System.Drawing.Point(150, 104);
            this.bnTouchUpper.Name = "bnTouchUpper";
            this.bnTouchUpper.Size = new System.Drawing.Size(129, 32);
            this.bnTouchUpper.TabIndex = 53;
            this.bnTouchUpper.Text = "Middle Click";
            this.bnTouchUpper.UseVisualStyleBackColor = false;
            // 
            // bnTouchMulti
            // 
            this.bnTouchMulti.BackColor = System.Drawing.Color.Transparent;
            this.bnTouchMulti.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.bnTouchMulti.Cursor = System.Windows.Forms.Cursors.Default;
            this.bnTouchMulti.FlatAppearance.BorderColor = System.Drawing.Color.Red;
            this.bnTouchMulti.FlatAppearance.BorderSize = 0;
            this.bnTouchMulti.FlatAppearance.MouseDownBackColor = System.Drawing.SystemColors.Control;
            this.bnTouchMulti.FlatAppearance.MouseOverBackColor = System.Drawing.SystemColors.Control;
            this.bnTouchMulti.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.bnTouchMulti.ForeColor = System.Drawing.SystemColors.WindowText;
            this.bnTouchMulti.Location = new System.Drawing.Point(215, 135);
            this.bnTouchMulti.Name = "bnTouchMulti";
            this.bnTouchMulti.Size = new System.Drawing.Size(64, 52);
            this.bnTouchMulti.TabIndex = 53;
            this.bnTouchMulti.Text = "Right Click";
            this.bnTouchMulti.UseVisualStyleBackColor = false;
            // 
            // bnLY
            // 
            this.bnLY.BackColor = System.Drawing.Color.Transparent;
            this.bnLY.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.bnLY.Cursor = System.Windows.Forms.Cursors.Default;
            this.bnLY.FlatAppearance.BorderColor = System.Drawing.Color.Red;
            this.bnLY.FlatAppearance.BorderSize = 0;
            this.bnLY.FlatAppearance.MouseDownBackColor = System.Drawing.SystemColors.Control;
            this.bnLY.FlatAppearance.MouseOverBackColor = System.Drawing.SystemColors.Control;
            this.bnLY.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.bnLY.ForeColor = System.Drawing.SystemColors.WindowText;
            this.bnLY.Location = new System.Drawing.Point(144, 213);
            this.bnLY.Name = "bnLY";
            this.bnLY.Size = new System.Drawing.Size(21, 10);
            this.bnLY.TabIndex = 53;
            this.bnLY.Text = "Left Y-Axis-";
            this.bnLY.UseVisualStyleBackColor = false;
            // 
            // lbControls
            // 
            this.lbControls.FormattingEnabled = true;
            this.lbControls.Items.AddRange(new object[] {
            "<Click a button, then a key/item to assign>"});
            this.lbControls.Location = new System.Drawing.Point(418, 67);
            this.lbControls.Name = "lbControls";
            this.lbControls.Size = new System.Drawing.Size(248, 212);
            this.lbControls.TabIndex = 54;
            // 
            // bnLY2
            // 
            this.bnLY2.BackColor = System.Drawing.Color.Transparent;
            this.bnLY2.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.bnLY2.Cursor = System.Windows.Forms.Cursors.Default;
            this.bnLY2.FlatAppearance.BorderColor = System.Drawing.Color.Red;
            this.bnLY2.FlatAppearance.BorderSize = 0;
            this.bnLY2.FlatAppearance.MouseDownBackColor = System.Drawing.SystemColors.Control;
            this.bnLY2.FlatAppearance.MouseOverBackColor = System.Drawing.SystemColors.Control;
            this.bnLY2.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.bnLY2.ForeColor = System.Drawing.SystemColors.WindowText;
            this.bnLY2.Location = new System.Drawing.Point(144, 241);
            this.bnLY2.Name = "bnLY2";
            this.bnLY2.Size = new System.Drawing.Size(21, 10);
            this.bnLY2.TabIndex = 53;
            this.bnLY2.Text = "Left Y-Axis+";
            this.bnLY2.UseVisualStyleBackColor = false;
            // 
            // bnRY
            // 
            this.bnRY.BackColor = System.Drawing.Color.Transparent;
            this.bnRY.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.bnRY.Cursor = System.Windows.Forms.Cursors.Default;
            this.bnRY.FlatAppearance.BorderColor = System.Drawing.Color.Red;
            this.bnRY.FlatAppearance.BorderSize = 0;
            this.bnRY.FlatAppearance.MouseDownBackColor = System.Drawing.SystemColors.Control;
            this.bnRY.FlatAppearance.MouseOverBackColor = System.Drawing.SystemColors.Control;
            this.bnRY.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.bnRY.ForeColor = System.Drawing.SystemColors.WindowText;
            this.bnRY.Location = new System.Drawing.Point(265, 213);
            this.bnRY.Name = "bnRY";
            this.bnRY.Size = new System.Drawing.Size(21, 10);
            this.bnRY.TabIndex = 53;
            this.bnRY.Text = "Right Y-Axis-";
            this.bnRY.UseVisualStyleBackColor = false;
            // 
            // bnRY2
            // 
            this.bnRY2.BackColor = System.Drawing.Color.Transparent;
            this.bnRY2.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.bnRY2.Cursor = System.Windows.Forms.Cursors.Default;
            this.bnRY2.FlatAppearance.BorderColor = System.Drawing.Color.Red;
            this.bnRY2.FlatAppearance.BorderSize = 0;
            this.bnRY2.FlatAppearance.MouseDownBackColor = System.Drawing.SystemColors.Control;
            this.bnRY2.FlatAppearance.MouseOverBackColor = System.Drawing.SystemColors.Control;
            this.bnRY2.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.bnRY2.ForeColor = System.Drawing.SystemColors.WindowText;
            this.bnRY2.Location = new System.Drawing.Point(265, 241);
            this.bnRY2.Name = "bnRY2";
            this.bnRY2.Size = new System.Drawing.Size(21, 10);
            this.bnRY2.TabIndex = 53;
            this.bnRY2.Text = "Right Y-Axis+";
            this.bnRY2.UseVisualStyleBackColor = false;
            // 
            // bnLX
            // 
            this.bnLX.BackColor = System.Drawing.Color.Transparent;
            this.bnLX.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.bnLX.Cursor = System.Windows.Forms.Cursors.Default;
            this.bnLX.FlatAppearance.BorderColor = System.Drawing.Color.Red;
            this.bnLX.FlatAppearance.BorderSize = 0;
            this.bnLX.FlatAppearance.MouseDownBackColor = System.Drawing.SystemColors.Control;
            this.bnLX.FlatAppearance.MouseOverBackColor = System.Drawing.SystemColors.Control;
            this.bnLX.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.bnLX.ForeColor = System.Drawing.SystemColors.WindowText;
            this.bnLX.Location = new System.Drawing.Point(131, 222);
            this.bnLX.Name = "bnLX";
            this.bnLX.Size = new System.Drawing.Size(10, 20);
            this.bnLX.TabIndex = 53;
            this.bnLX.Text = "Left X-Axis-";
            this.bnLX.UseVisualStyleBackColor = false;
            // 
            // bnLX2
            // 
            this.bnLX2.BackColor = System.Drawing.Color.Transparent;
            this.bnLX2.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.bnLX2.Cursor = System.Windows.Forms.Cursors.Default;
            this.bnLX2.FlatAppearance.BorderColor = System.Drawing.Color.Red;
            this.bnLX2.FlatAppearance.BorderSize = 0;
            this.bnLX2.FlatAppearance.MouseDownBackColor = System.Drawing.SystemColors.Control;
            this.bnLX2.FlatAppearance.MouseOverBackColor = System.Drawing.SystemColors.Control;
            this.bnLX2.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.bnLX2.ForeColor = System.Drawing.SystemColors.WindowText;
            this.bnLX2.Location = new System.Drawing.Point(167, 222);
            this.bnLX2.Name = "bnLX2";
            this.bnLX2.Size = new System.Drawing.Size(10, 20);
            this.bnLX2.TabIndex = 53;
            this.bnLX2.Text = "Left X-Axis+";
            this.bnLX2.UseVisualStyleBackColor = false;
            // 
            // bnRX
            // 
            this.bnRX.BackColor = System.Drawing.Color.Transparent;
            this.bnRX.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.bnRX.Cursor = System.Windows.Forms.Cursors.Default;
            this.bnRX.FlatAppearance.BorderColor = System.Drawing.Color.Red;
            this.bnRX.FlatAppearance.BorderSize = 0;
            this.bnRX.FlatAppearance.MouseDownBackColor = System.Drawing.SystemColors.Control;
            this.bnRX.FlatAppearance.MouseOverBackColor = System.Drawing.SystemColors.Control;
            this.bnRX.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.bnRX.ForeColor = System.Drawing.SystemColors.WindowText;
            this.bnRX.Location = new System.Drawing.Point(253, 222);
            this.bnRX.Name = "bnRX";
            this.bnRX.Size = new System.Drawing.Size(10, 20);
            this.bnRX.TabIndex = 53;
            this.bnRX.Text = "Right X-Axis-";
            this.bnRX.UseVisualStyleBackColor = false;
            // 
            // bnRX2
            // 
            this.bnRX2.BackColor = System.Drawing.Color.Transparent;
            this.bnRX2.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.bnRX2.Cursor = System.Windows.Forms.Cursors.Default;
            this.bnRX2.FlatAppearance.BorderColor = System.Drawing.Color.Red;
            this.bnRX2.FlatAppearance.BorderSize = 0;
            this.bnRX2.FlatAppearance.MouseDownBackColor = System.Drawing.SystemColors.Control;
            this.bnRX2.FlatAppearance.MouseOverBackColor = System.Drawing.SystemColors.Control;
            this.bnRX2.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.bnRX2.ForeColor = System.Drawing.SystemColors.WindowText;
            this.bnRX2.Location = new System.Drawing.Point(289, 222);
            this.bnRX2.Name = "bnRX2";
            this.bnRX2.Size = new System.Drawing.Size(10, 20);
            this.bnRX2.TabIndex = 53;
            this.bnRX2.Text = "Right X-Axis+";
            this.bnRX2.UseVisualStyleBackColor = false;
            // 
            // bnL3
            // 
            this.bnL3.BackColor = System.Drawing.Color.Transparent;
            this.bnL3.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.bnL3.Cursor = System.Windows.Forms.Cursors.Default;
            this.bnL3.FlatAppearance.BorderColor = System.Drawing.Color.Red;
            this.bnL3.FlatAppearance.BorderSize = 0;
            this.bnL3.FlatAppearance.MouseDownBackColor = System.Drawing.SystemColors.Control;
            this.bnL3.FlatAppearance.MouseOverBackColor = System.Drawing.SystemColors.Control;
            this.bnL3.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.bnL3.ForeColor = System.Drawing.SystemColors.WindowText;
            this.bnL3.Location = new System.Drawing.Point(147, 223);
            this.bnL3.Name = "bnL3";
            this.bnL3.Size = new System.Drawing.Size(16, 17);
            this.bnL3.TabIndex = 53;
            this.bnL3.Text = "Left Stick";
            this.bnL3.UseVisualStyleBackColor = false;
            // 
            // bnR3
            // 
            this.bnR3.BackColor = System.Drawing.Color.Transparent;
            this.bnR3.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.bnR3.Cursor = System.Windows.Forms.Cursors.Default;
            this.bnR3.FlatAppearance.BorderColor = System.Drawing.Color.Red;
            this.bnR3.FlatAppearance.BorderSize = 0;
            this.bnR3.FlatAppearance.MouseDownBackColor = System.Drawing.SystemColors.Control;
            this.bnR3.FlatAppearance.MouseOverBackColor = System.Drawing.SystemColors.Control;
            this.bnR3.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.bnR3.ForeColor = System.Drawing.SystemColors.WindowText;
            this.bnR3.Location = new System.Drawing.Point(267, 224);
            this.bnR3.Name = "bnR3";
            this.bnR3.Size = new System.Drawing.Size(16, 17);
            this.bnR3.TabIndex = 53;
            this.bnR3.Text = "Right Stick";
            this.bnR3.UseVisualStyleBackColor = false;
            // 
            // TouchTip
            // 
            this.TouchTip.AutoSize = true;
            this.TouchTip.Location = new System.Drawing.Point(141, 34);
            this.TouchTip.MaximumSize = new System.Drawing.Size(151, 52);
            this.TouchTip.MinimumSize = new System.Drawing.Size(151, 52);
            this.TouchTip.Name = "TouchTip";
            this.TouchTip.Size = new System.Drawing.Size(151, 52);
            this.TouchTip.TabIndex = 55;
            this.TouchTip.Text = "Touchpad (Standard Mode):\r\nTop: Upper Pad \r\nMiddle: Multi-Touch \r\nBottom: Single " +
    "Touch";
            this.TouchTip.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            this.TouchTip.Visible = false;
            // 
            // ReapTip
            // 
            this.ReapTip.AutoSize = true;
            this.ReapTip.Location = new System.Drawing.Point(134, 59);
            this.ReapTip.Name = "ReapTip";
            this.ReapTip.Size = new System.Drawing.Size(169, 26);
            this.ReapTip.TabIndex = 55;
            this.ReapTip.Text = "Double Tap a key to toggle repeat\r\n(Excludes TouchPad)";
            this.ReapTip.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            this.ReapTip.Visible = false;
            // 
            // lbMode
            // 
            this.lbMode.AutoSize = true;
            this.lbMode.Location = new System.Drawing.Point(418, 51);
            this.lbMode.Name = "lbMode";
            this.lbMode.Size = new System.Drawing.Size(51, 13);
            this.lbMode.TabIndex = 56;
            this.lbMode.Text = "Controller";
            this.lbMode.Visible = false;
            // 
            // CustomMapping
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(244)))), ((int)(((byte)(244)))));
            this.ClientSize = new System.Drawing.Size(684, 310);
            this.Controls.Add(this.TouchTip);
            this.Controls.Add(this.ReapTip);
            this.Controls.Add(this.pictureBox);
            this.Controls.Add(this.lbMode);
            this.Controls.Add(this.lbControls);
            this.Controls.Add(this.cbScanCode);
            this.Controls.Add(this.cbRepeat);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.btnLoad);
            this.Controls.Add(this.bnL2);
            this.Controls.Add(this.bnR2);
            this.Controls.Add(this.bnL1);
            this.Controls.Add(this.bnR1);
            this.Controls.Add(this.bnLeft);
            this.Controls.Add(this.bnRY2);
            this.Controls.Add(this.bnRY);
            this.Controls.Add(this.bnLY2);
            this.Controls.Add(this.bnRX2);
            this.Controls.Add(this.bnLX2);
            this.Controls.Add(this.bnRX);
            this.Controls.Add(this.bnLX);
            this.Controls.Add(this.bnLY);
            this.Controls.Add(this.bnRight);
            this.Controls.Add(this.bnDown);
            this.Controls.Add(this.bnR3);
            this.Controls.Add(this.bnL3);
            this.Controls.Add(this.bnPS);
            this.Controls.Add(this.bnShare);
            this.Controls.Add(this.bnTouchUpper);
            this.Controls.Add(this.bnTouchMulti);
            this.Controls.Add(this.bnTouchpad);
            this.Controls.Add(this.bnOptions);
            this.Controls.Add(this.bnUp);
            this.Controls.Add(this.bnTriangle);
            this.Controls.Add(this.bnSquare);
            this.Controls.Add(this.bnCircle);
            this.Controls.Add(this.bnCross);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(700, 349);
            this.MinimumSize = new System.Drawing.Size(700, 349);
            this.Name = "CustomMapping";
            this.Text = "Custom Mapping";
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox pictureBox;
        private System.Windows.Forms.CheckBox cbRepeat;
        private System.Windows.Forms.Button btnLoad;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.CheckBox cbScanCode;
        private System.Windows.Forms.Button bnCross;
        private System.Windows.Forms.Button bnCircle;
        private System.Windows.Forms.Button bnSquare;
        private System.Windows.Forms.Button bnTriangle;
        private System.Windows.Forms.Button bnR1;
        private System.Windows.Forms.Button bnR2;
        private System.Windows.Forms.Button bnL1;
        private System.Windows.Forms.Button bnL2;
        private System.Windows.Forms.Button bnUp;
        private System.Windows.Forms.Button bnDown;
        private System.Windows.Forms.Button bnRight;
        private System.Windows.Forms.Button bnLeft;
        private System.Windows.Forms.Button bnOptions;
        private System.Windows.Forms.Button bnShare;
        private System.Windows.Forms.Button bnTouchpad;
        private System.Windows.Forms.Button bnPS;
        private System.Windows.Forms.Button bnTouchUpper;
        private System.Windows.Forms.Button bnTouchMulti;
        private System.Windows.Forms.Button bnLY;
        private System.Windows.Forms.ListBox lbControls;
        private System.Windows.Forms.Button bnLY2;
        private System.Windows.Forms.Button bnRY;
        private System.Windows.Forms.Button bnRY2;
        private System.Windows.Forms.Button bnLX;
        private System.Windows.Forms.Button bnLX2;
        private System.Windows.Forms.Button bnRX;
        private System.Windows.Forms.Button bnRX2;
        private System.Windows.Forms.Button bnL3;
        private System.Windows.Forms.Button bnR3;
        private System.Windows.Forms.Label TouchTip;
        private System.Windows.Forms.Label ReapTip;
        private System.Windows.Forms.Label lbMode;
    }
}