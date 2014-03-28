using System;
using System.Drawing;
using System.Windows.Forms;
using DS4Library;
using DS4Control;
namespace ScpServer
{
    public partial class Options : Form
    {
        private DS4Control.Control scpDevice;
        private int device;

        Byte[] oldLedColor, oldLowLedColor;
        TrackBar tBsixaxisGyroX, tBsixaxisGyroY, tBsixaxisGyroZ,
            tBsixaxisAccelX, tBsixaxisAccelY, tBsixaxisAccelZ;
        Timer sixaxisTimer = new Timer();

        public Options(DS4Control.Control bus_device, int deviceNum)
        {
            InitializeComponent();
            device = deviceNum;
            scpDevice = bus_device;
            DS4Color color = Global.loadColor(device);
            redBar.Value = color.red;
            greenBar.Value = color.green;
            blueBar.Value = color.blue;
            rumbleBoostBar.Value = DS4Control.Global.loadRumbleBoost(device);
            batteryLed.Checked = DS4Control.Global.getLedAsBatteryIndicator(device);
            flashLed.Checked = DS4Control.Global.getFlashWhenLowBattery(device);
            touchCheckBox.Checked = Global.getTouchEnabled(device);
            touchSensitivityBar.Value = Global.getTouchSensitivity(device);
            leftTriggerMiddlePoint.Text = Global.getLeftTriggerMiddle(device).ToString();
            rightTriggerMiddlePoint.Text = Global.getRightTriggerMiddle(device).ToString();
            DS4Color lowColor = Global.loadLowColor(device);
            touchpadJitterCompensation.Checked = Global.getTouchpadJitterCompensation(device);
            lowerRCOffCheckBox.Checked = Global.getLowerRCOff(device);
            tapSensitivityBar.Value = Global.getTapSensitivity(device);
            scrollSensitivityBar.Value = Global.getScrollSensitivity(device);
            flushHIDQueue.Checked = Global.getFlushHIDQueue(device);
            advColorDialog.OnUpdateColor += advColorDialog_OnUpdateColor;

            // Force update of color choosers
            colorChooserButton.BackColor = Color.FromArgb(color.red, color.green, color.blue);
            lowColorChooserButton.BackColor = Color.FromArgb(lowColor.red, lowColor.green, lowColor.blue);
            pictureBox.BackColor = colorChooserButton.BackColor;
            lowRedValLabel.Text = lowColor.red.ToString();
            lowGreenValLabel.Text = lowColor.green.ToString();
            lowBlueValLabel.Text = lowColor.blue.ToString();

            #region watch sixaxis data
            // Control Positioning
            int horizontalOffset = cbSixaxis.Location.X - 50, 
                verticalOffset = cbSixaxis.Location.Y + cbSixaxis.Height + 5,
                tWidth = 100, tHeight = 19, 
                horizontalMargin = 10 + tWidth,
                verticalMargin = 1 + tHeight;

            sixaxisTimer.Tick +=
            (delegate
                {
                    if (tBsixaxisGyroX == null)
                    {
                        tBsixaxisGyroX = new TrackBar();
                        tBsixaxisGyroY = new TrackBar();
                        tBsixaxisGyroZ = new TrackBar();
                        tBsixaxisAccelX = new TrackBar();
                        tBsixaxisAccelY = new TrackBar();
                        tBsixaxisAccelZ = new TrackBar();
                        TrackBar[] allSixAxes = { tBsixaxisGyroX, tBsixaxisGyroY, tBsixaxisGyroZ,
                                                tBsixaxisAccelX, tBsixaxisAccelY, tBsixaxisAccelZ};
                        foreach (TrackBar t in allSixAxes)
                        {
                            ((System.ComponentModel.ISupportInitialize)(t)).BeginInit();
                            t.Anchor = AnchorStyles.Top | AnchorStyles.Right;
                            t.BackColor = SystemColors.ControlLightLight;
                            t.AutoSize = false;
                            t.Enabled = false;
                            t.Minimum = -0x8000;
                            t.Maximum = 0x7fff;
                            t.Size = new Size(tWidth, tHeight);
                            t.TickFrequency = 0x2000; // calibrated to ~1G
                        }
                        // tBsixaxisGyroX
                        tBsixaxisGyroX.Location = new Point(horizontalOffset, verticalOffset);
                        tBsixaxisGyroX.Name = "tBsixaxisGyroX";
                        // tBsixaxisGyroY
                        tBsixaxisGyroY.Location = new Point(horizontalOffset, verticalOffset + verticalMargin);
                        tBsixaxisGyroY.Name = "tBsixaxisGyroY";
                        // tBsixaxisGyroZ
                        tBsixaxisGyroZ.Location = new Point(horizontalOffset, verticalOffset + verticalMargin * 2);
                        tBsixaxisGyroZ.Name = "tBsixaxisGyroZ";
                        // tBsixaxisAccelX
                        tBsixaxisAccelX.Location = new Point(horizontalOffset + horizontalMargin, verticalOffset);
                        tBsixaxisAccelX.Name = "tBsixaxisAccelX";
                        // tBsixaxisAccelY
                        tBsixaxisAccelY.Location = new Point(horizontalOffset + horizontalMargin, verticalOffset + verticalMargin);
                        tBsixaxisAccelY.Name = "tBsixaxisAccelY";
                        // tBsixaxisAccelZ
                        tBsixaxisAccelZ.Location = new Point(horizontalOffset + horizontalMargin, verticalOffset + verticalMargin * 2);
                        tBsixaxisAccelZ.Name = "tBsixaxisAccelZ";
                        foreach (TrackBar t in allSixAxes)
                        {
                            tabTuning.Controls.Add(t);
                            ((System.ComponentModel.ISupportInitialize)(t)).EndInit();
                        }
                    }
                    //byte[] inputData = null;// scpDevice.GetInputData(device);
                    //if (inputData != null)
                    {
                        // MEMS gyro data is all calibrated to roughly -1G..1G for values -0x2000..0x1fff
                        // Enough additional acceleration and we are no longer mostly measuring Earth's gravity...
                        // We should try to indicate setpoints of the calibration when exposing this measurement....
                        SetDynamicTrackBarValue(tBsixaxisGyroX, (scpDevice.ExposedState[device].GyroX + tBsixaxisGyroX.Value * 2) / 3);
                        SetDynamicTrackBarValue(tBsixaxisGyroY, (scpDevice.ExposedState[device].GyroY + tBsixaxisGyroY.Value * 2) / 3);
                        SetDynamicTrackBarValue(tBsixaxisGyroZ, (scpDevice.ExposedState[device].GyroZ + tBsixaxisGyroZ.Value * 2) / 3);
                        SetDynamicTrackBarValue(tBsixaxisAccelX, (scpDevice.ExposedState[device].AccelX + tBsixaxisAccelX.Value * 2) / 3);
                        SetDynamicTrackBarValue(tBsixaxisAccelY, (scpDevice.ExposedState[device].AccelY + tBsixaxisAccelY.Value * 2) / 3);
                        SetDynamicTrackBarValue(tBsixaxisAccelZ, (scpDevice.ExposedState[device].AccelZ + tBsixaxisAccelZ.Value * 2) / 3);
                    }
                });
            sixaxisTimer.Interval = 1000 / 60;
            this.FormClosing += delegate { if (sixaxisTimer.Enabled) sixaxisTimer.Stop(); };
            if (cbSixaxis.Checked)
                sixaxisTimer.Start();
            #endregion
        }

        private void cbSixaxis_CheckedChanged(object sender, EventArgs e)
        {
            if (cbSixaxis.Checked)
            {
                sixaxisTimer.Start();
                if (tBsixaxisGyroX != null)
                {
                    tBsixaxisGyroX.Visible = true;
                    tBsixaxisGyroY.Visible = true;
                    tBsixaxisGyroZ.Visible = true;
                    tBsixaxisAccelX.Visible = true;
                    tBsixaxisAccelY.Visible = true;
                    tBsixaxisAccelZ.Visible = true;
                }
            }
            else
            {
                sixaxisTimer.Stop();
                tBsixaxisGyroX.Visible = false;
                tBsixaxisGyroY.Visible = false;
                tBsixaxisGyroZ.Visible = false;
                tBsixaxisAccelX.Visible = false;
                tBsixaxisAccelY.Visible = false;
                tBsixaxisAccelZ.Visible = false;
            }
        }

        private void SetDynamicTrackBarValue(TrackBar trackBar, int value)
        {
            if (trackBar.Maximum < value)
                trackBar.Maximum = value;
            else if (trackBar.Minimum > value)
                trackBar.Minimum = value;
            trackBar.Value = value;
        }

        private void CustomMappingButton_Click(object sender, EventArgs e)
        {
            // open a custom mapping form
            CustomMapping cmForm = new CustomMapping(scpDevice, device);
            cmForm.Icon = this.Icon;
            cmForm.Show();
        }

        private void setButton_Click(object sender, EventArgs e)
        {
            Global.saveColor(device, 
                colorChooserButton.BackColor.R, 
                colorChooserButton.BackColor.G, 
                colorChooserButton.BackColor.B);
            Global.saveLowColor(device,
                lowColorChooserButton.BackColor.R, 
                lowColorChooserButton.BackColor.G, 
                lowColorChooserButton.BackColor.B);
            double middle;
            if (Double.TryParse(leftTriggerMiddlePoint.Text, out middle))
                Global.setLeftTriggerMiddle(device, middle);
            if (Double.TryParse(rightTriggerMiddlePoint.Text, out middle))
                Global.setRightTriggerMiddle(device, middle);
            Global.saveRumbleBoost(device,(byte)rumbleBoostBar.Value);
            scpDevice.setRumble((byte)leftMotorBar.Value, (byte)rightMotorBar.Value,device);
            Global.setTouchSensitivity(device, (byte)touchSensitivityBar.Value);
            Global.setTouchpadJitterCompensation(device, touchpadJitterCompensation.Checked);
            Global.setLowerRCOff(device, !lowerRCOffCheckBox.Checked);
            Global.setTapSensitivity(device, (byte)tapSensitivityBar.Value);
            Global.setScrollSensitivity(device, scrollSensitivityBar.Value);
        }

        private void saveButton_Click(object sender, EventArgs e)
        {
            setButton_Click(null, null);
            Global.Save();
            this.Close();
        }

        private void redBar_ValueChanged(object sender, EventArgs e)
        {
            // New settings
            if (lowLedCheckBox.Checked)
            {
                lowRedValLabel.Text = redBar.Value.ToString();
                lowColorChooserButton.BackColor = Color.FromArgb(
                    redBar.Value,
                    lowColorChooserButton.BackColor.G,
                    lowColorChooserButton.BackColor.B);
                pictureBox.BackColor = Color.FromArgb(
                    redBar.Value,
                    lowColorChooserButton.BackColor.G,
                    lowColorChooserButton.BackColor.B);
                if (realTimeChangesCheckBox.Checked)
                    Global.saveLowColor(device, (byte)redBar.Value,
                        lowColorChooserButton.BackColor.G, 
                        lowColorChooserButton.BackColor.B);
            }
            else
            {
                colorChooserButton.BackColor = Color.FromArgb(
                    redBar.Value,
                    colorChooserButton.BackColor.G,
                    colorChooserButton.BackColor.B);
                pictureBox.BackColor = Color.FromArgb(
                    redBar.Value,
                    colorChooserButton.BackColor.G,
                    colorChooserButton.BackColor.B);
                if (realTimeChangesCheckBox.Checked)
                    Global.saveColor(device, (byte)redBar.Value,
                        colorChooserButton.BackColor.G,
                        colorChooserButton.BackColor.B);

                // Previous implementation
                redValLabel.Text = redBar.Value.ToString();
            }
        }
        private void greenBar_ValueChanged(object sender, EventArgs e)
        {
            // New settings
            if (lowLedCheckBox.Checked)
            {
                lowGreenValLabel.Text = greenBar.Value.ToString();
                lowColorChooserButton.BackColor = Color.FromArgb(
                    lowColorChooserButton.BackColor.R,
                    greenBar.Value,
                    lowColorChooserButton.BackColor.B);
                pictureBox.BackColor = Color.FromArgb(
                    lowColorChooserButton.BackColor.R,
                    greenBar.Value,
                    lowColorChooserButton.BackColor.B); 
                if (realTimeChangesCheckBox.Checked)
                    Global.saveLowColor(device,
                        lowColorChooserButton.BackColor.R,
                        (byte)greenBar.Value,
                        lowColorChooserButton.BackColor.B);
            }
            else
            {
                colorChooserButton.BackColor = Color.FromArgb(
                    colorChooserButton.BackColor.R,
                    greenBar.Value,
                    colorChooserButton.BackColor.B);
                pictureBox.BackColor = Color.FromArgb(
                    colorChooserButton.BackColor.R,
                    greenBar.Value,
                    colorChooserButton.BackColor.B);
                if (realTimeChangesCheckBox.Checked)
                    Global.saveColor(device,
                        colorChooserButton.BackColor.R,
                        (byte)greenBar.Value,
                        colorChooserButton.BackColor.B);

                // Previous implementation
                greenValLabel.Text = greenBar.Value.ToString();
            }
        }
        private void blueBar_ValueChanged(object sender, EventArgs e)
        {
            // New settings
            if (lowLedCheckBox.Checked)
            {
                lowBlueValLabel.Text = blueBar.Value.ToString();
                lowColorChooserButton.BackColor = Color.FromArgb(
                    lowColorChooserButton.BackColor.R,
                    lowColorChooserButton.BackColor.G,
                    blueBar.Value);
                pictureBox.BackColor = Color.FromArgb(
                    lowColorChooserButton.BackColor.R,
                    lowColorChooserButton.BackColor.G,
                    blueBar.Value);
                if (realTimeChangesCheckBox.Checked)
                    Global.saveLowColor(device,
                        lowColorChooserButton.BackColor.R,
                        lowColorChooserButton.BackColor.G,
                        (byte)blueBar.Value);
            }
            else
            {
                colorChooserButton.BackColor = Color.FromArgb(
                    colorChooserButton.BackColor.R,
                    colorChooserButton.BackColor.G,
                    blueBar.Value);
                pictureBox.BackColor = Color.FromArgb(
                    colorChooserButton.BackColor.R,
                    colorChooserButton.BackColor.G,
                    blueBar.Value);
                if (realTimeChangesCheckBox.Checked)
                    Global.saveColor(device,
                        colorChooserButton.BackColor.R,
                        colorChooserButton.BackColor.G,
                        (byte)blueBar.Value);

                // Previous implementation
                blueValLabel.Text = blueBar.Value.ToString();
            }
        }

        private void rumbleBoostBar_ValueChanged(object sender, EventArgs e)
        {
            rumbleBoostMotorValLabel.Text = rumbleBoostBar.Value.ToString();

            if (realTimeChangesCheckBox.Checked)
            {
                Global.saveRumbleBoost(device, (byte)rumbleBoostBar.Value);
                scpDevice.setRumble((byte)leftMotorBar.Value, (byte)rightMotorBar.Value, device);
            }
        }
        
        private void leftMotorBar_ValueChanged(object sender, EventArgs e)
        {
            leftMotorValLabel.Text = leftMotorBar.Value.ToString();

            if (realTimeChangesCheckBox.Checked)
                scpDevice.setRumble((byte)leftMotorBar.Value, (byte)rightMotorBar.Value, device);
        }

        private void rightMotorBar_ValueChanged(object sender, EventArgs e)
        {
            rightMotorValLabel.Text = rightMotorBar.Value.ToString();

            if (realTimeChangesCheckBox.Checked)
                 scpDevice.setRumble((byte)leftMotorBar.Value, (byte)rightMotorBar.Value, device);
        }

        private void touchSensitivityBar_ValueChanged(object sender, EventArgs e)
        {
            sensitivityValLabel.Text = touchSensitivityBar.Value.ToString();

            if (realTimeChangesCheckBox.Checked)
                Global.setTouchSensitivity(device, (byte)touchSensitivityBar.Value);
        }
        
        private void tapSensitivityBar_ValueChanged(object sender, EventArgs e)
        {
            tapSensitivityValLabel.Text = tapSensitivityBar.Value.ToString();
            if (tapSensitivityValLabel.Text == "0")
                tapSensitivityValLabel.Text = "Off";

            if (realTimeChangesCheckBox.Checked)
                Global.setTapSensitivity(device, (byte)tapSensitivityBar.Value);
        }
        
        private void scrollSensitivityBar_ValueChanged(object sender, EventArgs e)
        {
            scrollSensitivityValLabel.Text = scrollSensitivityBar.Value.ToString();
            if (scrollSensitivityValLabel.Text == "0")
                scrollSensitivityValLabel.Text = "Off";

            if (realTimeChangesCheckBox.Checked)
                Global.setScrollSensitivity(device, (byte)scrollSensitivityBar.Value);
        }

        private void lowBatteryLed_CheckedChanged(object sender, EventArgs e)
        {
            if (lowLedCheckBox.Checked)
            {
                fullLedPanel.Enabled = false;
                redBar.Value = int.Parse(lowRedValLabel.Text);
                greenBar.Value = int.Parse(lowGreenValLabel.Text);
                blueBar.Value = int.Parse(lowBlueValLabel.Text);
                pictureBox.BackColor = lowColorChooserButton.BackColor;
                if (realTimeChangesCheckBox.Checked)
                    Global.saveLowColor(device,
                        lowColorChooserButton.BackColor.R,
                        lowColorChooserButton.BackColor.G,
                        lowColorChooserButton.BackColor.B);
            }
            else
            {
                fullLedPanel.Enabled = true;
                redBar.Value = int.Parse(redValLabel.Text);
                greenBar.Value = int.Parse(greenValLabel.Text);
                blueBar.Value = int.Parse(blueValLabel.Text);
                pictureBox.BackColor = colorChooserButton.BackColor;
                if (realTimeChangesCheckBox.Checked)
                    Global.saveColor(device,
                        colorChooserButton.BackColor.R,
                        colorChooserButton.BackColor.G,
                        colorChooserButton.BackColor.B);
            }
        }
        private void ledAsBatteryIndicator_CheckedChanged(object sender, EventArgs e)
        {
            Global.setLedAsBatteryIndicator(device, batteryLed.Checked);

            // New settings
            if (batteryLed.Checked)
            {
                lowLedPanel.Visible = true;
                lowLedCheckBox.Visible = true;
                if (realTimeChangesCheckBox.Checked)
                    Global.setLedAsBatteryIndicator(device, true);
            }
            else 
            {
                lowLedPanel.Visible = false;
                lowLedCheckBox.Visible = false;
                if (realTimeChangesCheckBox.Checked)
                    Global.setLedAsBatteryIndicator(device, false);
            }
        }
        private void flashWhenLowBattery_CheckedChanged(object sender, EventArgs e)
        {
            Global.setFlashWhenLowBattery(device, flashLed.Checked);
        }
        private void touchAtStartCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            Global.setTouchEnabled(device,touchCheckBox.Checked);
        }
        private void lowerRCOffCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (realTimeChangesCheckBox.Checked)
                Global.setLowerRCOff(device, !lowerRCOffCheckBox.Checked);
        }

        private void touchpadJitterCompensation_CheckedChanged(object sender, EventArgs e)
        {
            if (realTimeChangesCheckBox.Checked)
                Global.setTouchpadJitterCompensation(device, touchpadJitterCompensation.Checked);
        }
        private void realTimeChangesCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (realTimeChangesCheckBox.Checked)
            {
                setButton.Visible = false;
            }
            else
            {
                setButton.Visible = true;
            }
        }
        
        private void pictureBox_Click(object sender, EventArgs e)
        {
            if (lowLedCheckBox.Checked)
                lowColorChooserButton_Click(sender, e);
            else colorChooserButton_Click(sender, e);
        }
        private void colorChooserButton_Click(object sender, EventArgs e)
        {
            advColorDialog.Color = colorChooserButton.BackColor;
            advColorDialog_OnUpdateColor(colorChooserButton.BackColor, e);
            if (advColorDialog.ShowDialog() == DialogResult.OK)
            {
                redValLabel.Text = advColorDialog.Color.R.ToString();
                greenValLabel.Text = advColorDialog.Color.G.ToString();
                blueValLabel.Text = advColorDialog.Color.B.ToString();
                colorChooserButton.BackColor = advColorDialog.Color;
                pictureBox.BackColor = advColorDialog.Color;
                if (!lowLedCheckBox.Checked)
                {
                    redBar.Value = advColorDialog.Color.R;
                    greenBar.Value = advColorDialog.Color.G;
                    blueBar.Value = advColorDialog.Color.B;
                }
            }
            else Global.saveColor(device, oldLedColor[0], oldLedColor[1], oldLedColor[2]);
            Global.saveLowColor(device, oldLowLedColor[0], oldLowLedColor[1], oldLowLedColor[2]);
            oldLedColor = null;
            oldLowLedColor = null;
        }
        private void lowColorChooserButton_Click(object sender, EventArgs e)
        {
            advColorDialog.Color = lowColorChooserButton.BackColor;
            advColorDialog_OnUpdateColor(lowColorChooserButton.BackColor, e);
            if (advColorDialog.ShowDialog() == DialogResult.OK)
            {
                lowRedValLabel.Text = advColorDialog.Color.R.ToString();
                lowGreenValLabel.Text = advColorDialog.Color.G.ToString();
                lowBlueValLabel.Text = advColorDialog.Color.B.ToString();
                lowColorChooserButton.BackColor = advColorDialog.Color;
                pictureBox.BackColor = advColorDialog.Color;
                if (lowLedCheckBox.Checked)
                {
                    redBar.Value = advColorDialog.Color.R;
                    greenBar.Value = advColorDialog.Color.G;
                    blueBar.Value = advColorDialog.Color.B;
                }
            }
            else Global.saveLowColor(device, oldLowLedColor[0], oldLowLedColor[1], oldLowLedColor[2]);
            Global.saveColor(device, oldLedColor[0], oldLedColor[1], oldLedColor[2]);
            oldLedColor = null;
            oldLowLedColor = null;
        }
        private void advColorDialog_OnUpdateColor(object sender, EventArgs e)
        {
            if (oldLedColor == null || oldLowLedColor == null)
            {
                DS4Color color = Global.loadColor(device);
                oldLedColor = new Byte[] { color.red, color.green, color.blue };
                color = Global.loadLowColor(device);
                oldLowLedColor = new Byte[] { color.red, color.green, color.blue };
            }
            if (sender is Color)
            {
                Color color = (Color)sender;
                Global.saveColor(device, color.R, color.G, color.B);
                Global.saveLowColor(device, color.R, color.G, color.B);
            }
        }

        private void flushHIDQueue_CheckedChanged(object sender, EventArgs e)
        {
            Global.setFlushHIDQueue(device, flushHIDQueue.Checked);
        }


    }

    
}
