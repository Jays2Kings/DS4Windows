using System;
using System.Drawing;
using System.Windows.Forms;
using DS4Library;
using DS4Control;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
namespace ScpServer
{
    public partial class Options : Form
    {
        private DS4Control.Control scpDevice;
        private int device;
        private string filename;
        private ScpForm mainWin;
        Byte[] oldLedColor, oldLowLedColor;
        TrackBar tBsixaxisGyroX, tBsixaxisGyroY, tBsixaxisGyroZ,
            tBsixaxisAccelX, tBsixaxisAccelY, tBsixaxisAccelZ;
        Timer sixaxisTimer = new Timer();
        private List<Button> buttons = new List<Button>();
        //private Dictionary<string, string> defaults = new Dictionary<string, string>();
        private Button lastSelected;
        private int alphacolor;
        private Color reg, full;
        private Image colored, greyscale;

        public Options(DS4Control.Control bus_device, int deviceNum, string name, ScpForm mainWindow)
        {
            InitializeComponent();
            device = deviceNum;
            scpDevice = bus_device;
            filename = name;
            mainWin = mainWindow;
            colored = pBRainbow.Image;
            greyscale = GreyscaleImage((Bitmap)pBRainbow.Image);
            if (filename != "")
            {
                tBProfile.Text = filename;
                DS4Color color = Global.loadColor(device);
                redBar.Value = color.red;
                greenBar.Value = color.green;
                blueBar.Value = color.blue;
                
                batteryLed.Checked = DS4Control.Global.getLedAsBatteryIndicator(device);
                DS4Color lowColor = Global.loadLowColor(device);
                lowRedBar.Value = lowColor.red;
                lowGreenBar.Value = lowColor.green;
                lowBlueBar.Value = lowColor.blue;

                rumbleBoostBar.Value = DS4Control.Global.loadRumbleBoost(device);
                rumbleSwap.Checked = Global.getRumbleSwap(device);
                flashLed.Checked = DS4Control.Global.getFlashWhenLowBattery(device);
                numUDTouch.Value = Global.getTouchSensitivity(device);
                numUDScroll.Value = Global.getScrollSensitivity(device);
                numUDTap.Value = Global.getTapSensitivity(device);
                cBTap.Checked = Global.getTap(device);
                cBDoubleTap.Checked = Global.getDoubleTap(device);
                leftTriggerMiddlePoint.Text = Global.getLeftTriggerMiddle(device).ToString();
                rightTriggerMiddlePoint.Text = Global.getRightTriggerMiddle(device).ToString();
                touchpadJitterCompensation.Checked = Global.getTouchpadJitterCompensation(device);
                cBlowerRCOn.Checked = Global.getLowerRCOn(device);
                flushHIDQueue.Checked = Global.getFlushHIDQueue(device);
                idleDisconnectTimeout.Value = Global.getIdleDisconnectTimeout(device);
                tBMouseSens.Value = Global.getButtonMouseSensitivity(device);
                lBMouseSens.Text = tBMouseSens.Value.ToString();
                // Force update of color choosers    
                alphacolor = Math.Max(redBar.Value, Math.Max(greenBar.Value, blueBar.Value));
                reg = Color.FromArgb(color.red, color.green, color.blue);
                full = HuetoRGB(reg.GetHue(), reg.GetSaturation());
                colorChooserButton.BackColor = Color.FromArgb((alphacolor > 205 ? 255 : (alphacolor + 50)), full);
                pBController.BackColor = colorChooserButton.BackColor;

                alphacolor = Math.Max(lowRedBar.Value, Math.Max(greenBar.Value, blueBar.Value));
                reg = Color.FromArgb(lowColor.red, lowColor.green, lowColor.blue);
                full = HuetoRGB(reg.GetHue(), reg.GetSaturation());
                lowColorChooserButton.BackColor = Color.FromArgb((alphacolor > 205 ? 255 : (alphacolor + 50)), full);
                lowRedValLabel.Text = lowColor.red.ToString();
                lowGreenValLabel.Text = lowColor.green.ToString();
                lowBlueValLabel.Text = lowColor.blue.ToString();
                numUDRainbow.Value = (decimal)Global.getRainbow(device);
                if (Global.getRainbow(device) == 0)
                {
                    pBRainbow.Image = greyscale;
                    ToggleRainbow(false);
                }
                else
                {
                    pBRainbow.Image = colored;
                    ToggleRainbow(true);
                }

            }
            else
                Set();
            #region watch sixaxis data
            // Control Positioning
            int horizontalOffset = cbSixaxis.Location.X,
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
                            tabOther.Controls.Add(t);
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
            this.FormClosing += delegate
            {
                if (sixaxisTimer.Enabled)
                    sixaxisTimer.Stop();
                //foreach (CustomMapping cmf in customMappingForms)
                //   if (cmf != null)
                //     cmf.Close();
            };
            if (cbSixaxis.Checked)
                sixaxisTimer.Start();
            #endregion

            foreach (System.Windows.Forms.Control control in tabControls.Controls)
                if (control is Button)
                    if (!((Button)control).Text.Contains("btn"))
                        buttons.Add((Button)control);
            foreach (System.Windows.Forms.Control control in tabTouchPad.Controls)
                if (control is Button)
                    buttons.Add((Button)control);
            foreach (System.Windows.Forms.Control control in tabAnalogSticks.Controls)
                if (control is Button)
                    buttons.Add((Button)control);
            if (filename != "" && filename != "New Profile")
                Global.LoadProfile(device, buttons.ToArray());
            ToolTip tp = new ToolTip();
            tp.SetToolTip(cBlowerRCOn, "Best used with right side as mouse");
            tp.SetToolTip(cBDoubleTap, "Tap and hold to drag, slight delay with one tap");
            advColorDialog.OnUpdateColor += advColorDialog_OnUpdateColor;
            btnLeftStick.Enter += btnSticks_Enter;
            btnRightStick.Enter += btnSticks_Enter;
            btnTouchtab.Enter += btnTouchtab_Enter;
            btnLightbar.Click += btnLightbar_Click;
            UpdateLists();
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

        KBM360 kbm360 = null;

        private void Show_ControlsBn(object sender, EventArgs e)
        {
            lastSelected = (Button)sender;
            kbm360 = new KBM360(scpDevice, device, this, lastSelected, 0);
            kbm360.Icon = this.Icon;
            kbm360.ShowDialog();
            //kbm360.FormClosed += delegate { kbm360 = null; };
            //this.Enabled = false;
        }

        private void Show360Controls(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                lastSelected = (Button)sender;
                kbm360 = new KBM360(scpDevice, device, this, lastSelected, 1);
                kbm360.Icon = this.Icon;
                kbm360.ShowDialog();
            }
        }

        public void ChangeButtonText(string controlname, object tag)
        {
            lastSelected.Text = controlname;
            int value;
            if (Int32.TryParse(tag.ToString(), out value))
                lastSelected.Tag = value;
            else
                lastSelected.Tag = tag.ToString();
        }
        public void ChangeButtonText(string controlname)
        {
            lastSelected.Text = controlname;
            lastSelected.Tag = controlname;
        }
        public void Toggle_Repeat(bool Checked)
        {
            if (lastSelected.Tag is int || lastSelected.Tag is UInt16)
                if (Checked)
                    lastSelected.ForeColor = Color.Red;
                else lastSelected.ForeColor = SystemColors.WindowText;
            else
            {
                //cbRepeat.Checked = false;
                lastSelected.ForeColor = SystemColors.WindowText;
            }
        }
        public void Toggle_ScanCode(bool Checked)
        {
            if (lastSelected.Tag is int || lastSelected.Tag is UInt16)
                if (Checked)
                    lastSelected.Font = new Font(lastSelected.Font, FontStyle.Bold);
                else lastSelected.Font = new Font(lastSelected.Font, FontStyle.Regular);
            else
            {
                //cbScanCode.Checked = false;
                lastSelected.Font = new Font(lastSelected.Font, FontStyle.Regular);
            }
        }
        private void btnSticks_Enter(object sender, EventArgs e)
        {
            tabOptions.SelectTab(1);
        }
        private void btnTouchtab_Enter(object sender, EventArgs e)
        {
            tabOptions.SelectTab(2);
        }
        private void btnLightbar_Click(object sender, EventArgs e)
        {

        }

        private void Set()
        {
            Global.saveColor(device, (byte)redBar.Value, (byte)greenBar.Value, (byte)blueBar.Value);
            Global.saveLowColor(device, (byte)lowRedBar.Value, (byte)lowGreenBar.Value, (byte)lowBlueBar.Value);
            double middle;
            if (Double.TryParse(leftTriggerMiddlePoint.Text, out middle))
                Global.setLeftTriggerMiddle(device, middle);
            if (Double.TryParse(rightTriggerMiddlePoint.Text, out middle))
                Global.setRightTriggerMiddle(device, middle);
            Global.saveRumbleBoost(device, (byte)rumbleBoostBar.Value);
            scpDevice.setRumble((byte)leftMotorBar.Value, (byte)rightMotorBar.Value, device);
            Global.setRumbleSwap(device, rumbleSwap.Checked);
            Global.setTouchSensitivity(device, (byte)numUDTouch.Value);
            Global.setTouchpadJitterCompensation(device, touchpadJitterCompensation.Checked);
            Global.setLowerRCOn(device, cBlowerRCOn.Checked);
            Global.setScrollSensitivity(device, (byte)numUDScroll.Value);
            Global.setDoubleTap(device, cBDoubleTap.Checked);
            Global.setButtonMouseSensitivity(device, tBMouseSens.Value);
            Global.setTapSensitivity(device, (byte)numUDTap.Value);
            Global.setIdleDisconnectTimeout(device, (int)idleDisconnectTimeout.Value);
            Global.setButtonMouseSensitivity(device, tBMouseSens.Value);
            Global.setRainbow(device, (int)numUDRainbow.Value);
            if (numUDRainbow.Value == 0)
                pBRainbow.Image = greyscale;
            else
                pBRainbow.Image = colored;
        }

        private void saveButton_Click(object sender, EventArgs e)
        {
            Set();

            if (tBProfile.Text != null && tBProfile.Text != "" && !tBProfile.Text.Contains("\\") && !tBProfile.Text.Contains("/") && !tBProfile.Text.Contains(":") && !tBProfile.Text.Contains("*") && !tBProfile.Text.Contains("?") && !tBProfile.Text.Contains("\"") && !tBProfile.Text.Contains("<") && !tBProfile.Text.Contains(">") && !tBProfile.Text.Contains("|"))
            {
                Global.setAProfile(device, tBProfile.Text);
                Global.SaveProfile(device, tBProfile.Text, buttons.ToArray());
                Global.Save();
                this.Close();
            }
            else
                MessageBox.Show("Please enter a valid name", "Not valid", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);

        }

        private void redBar_ValueChanged(object sender, EventArgs e)
        {
            alphacolor = Math.Max(redBar.Value, Math.Max(greenBar.Value, blueBar.Value));
            reg = Color.FromArgb(redBar.Value, greenBar.Value, blueBar.Value);
            full = HuetoRGB(reg.GetHue(), reg.GetSaturation());
            colorChooserButton.BackColor = Color.FromArgb((alphacolor > 205 ? 255 : (alphacolor + 50)), full);
            Global.saveColor(device, (byte)redBar.Value, (byte)greenBar.Value, (byte)blueBar.Value);

            pBController.BackColor = colorChooserButton.BackColor;
            redValLabel.Text = redBar.Value.ToString();
        }
        private void greenBar_ValueChanged(object sender, EventArgs e)
        {
            alphacolor = Math.Max(redBar.Value, Math.Max(greenBar.Value, blueBar.Value));
            reg = Color.FromArgb(redBar.Value, greenBar.Value, blueBar.Value);
            full = HuetoRGB(reg.GetHue(), reg.GetSaturation());
            colorChooserButton.BackColor = Color.FromArgb((alphacolor > 205 ? 255 : (alphacolor + 50)), full);
            Global.saveColor(device, (byte)redBar.Value, (byte)greenBar.Value, (byte)blueBar.Value);

            pBController.BackColor = colorChooserButton.BackColor;
            greenValLabel.Text = greenBar.Value.ToString();
        }
        private void blueBar_ValueChanged(object sender, EventArgs e)
        {
            alphacolor = Math.Max(redBar.Value, Math.Max(greenBar.Value, blueBar.Value));
            reg = Color.FromArgb(redBar.Value, greenBar.Value, blueBar.Value);
            full = HuetoRGB(reg.GetHue(), reg.GetSaturation());
            colorChooserButton.BackColor = Color.FromArgb((alphacolor > 205 ? 255 : (alphacolor + 50)), full);
            Global.saveColor(device, (byte)redBar.Value, (byte)greenBar.Value, (byte)blueBar.Value);

            pBController.BackColor = colorChooserButton.BackColor;
            blueValLabel.Text = blueBar.Value.ToString();
        }

        private void lowRedBar_ValueChanged(object sender, EventArgs e)
        {
            alphacolor = Math.Max(lowRedBar.Value, Math.Max(lowGreenBar.Value, lowBlueBar.Value));
            reg = Color.FromArgb(lowRedBar.Value, lowGreenBar.Value, lowBlueBar.Value);
            full = HuetoRGB(reg.GetHue(), reg.GetSaturation());
            lowColorChooserButton.BackColor = Color.FromArgb((alphacolor > 205 ? 255 : (alphacolor + 50)), full);
            Global.saveLowColor(device, (byte)lowRedBar.Value, (byte)lowGreenBar.Value, (byte)lowBlueBar.Value);
            lowRedValLabel.Text = lowRedBar.Value.ToString();
        }

        private void lowGreenBar_ValueChanged(object sender, EventArgs e)
        {
            alphacolor = Math.Max(lowRedBar.Value, Math.Max(lowGreenBar.Value, lowBlueBar.Value));
            reg = Color.FromArgb(lowRedBar.Value, lowGreenBar.Value, lowBlueBar.Value);
            full = HuetoRGB(reg.GetHue(), reg.GetSaturation());
            lowColorChooserButton.BackColor = Color.FromArgb((alphacolor > 205 ? 255 : (alphacolor + 50)), full);
            Global.saveLowColor(device, (byte)lowRedBar.Value, (byte)lowGreenBar.Value, (byte)lowBlueBar.Value);
            lowGreenValLabel.Text = lowGreenBar.Value.ToString();
        }

        private void lowBlueBar_ValueChanged(object sender, EventArgs e)
        {
            alphacolor = Math.Max(lowRedBar.Value, Math.Max(lowGreenBar.Value, lowBlueBar.Value));
            reg = Color.FromArgb(lowRedBar.Value, lowGreenBar.Value, lowBlueBar.Value);
            full = HuetoRGB(reg.GetHue(), reg.GetSaturation());
            lowColorChooserButton.BackColor = Color.FromArgb((alphacolor > 205 ? 255 : (alphacolor + 50)), full);
            Global.saveLowColor(device, (byte)lowRedBar.Value, (byte)lowGreenBar.Value, (byte)lowBlueBar.Value);
            lowBlueValLabel.Text = lowBlueBar.Value.ToString();
        }

        public Color HuetoRGB(float hue, float sat)
        {
            int C = (int)(sat * 255);
            int X = (int)((sat * (float)(1 - Math.Abs((hue / 60) % 2 - 1))) * 255);
            if (sat == 0)
                return Color.FromName("White");
            else if (0 <= hue && hue < 60)
                return Color.FromArgb(C, X, 0);
            else if (60 <= hue && hue < 120)
                return Color.FromArgb(X, C, 0);
            else if (120 <= hue && hue < 180)
                return Color.FromArgb(0, C, X);
            else if (180 <= hue && hue < 240)
                return Color.FromArgb(0, X, C);
            else if (240 <= hue && hue < 300)
                return Color.FromArgb(X, 0, C);
            else if (300 <= hue && hue < 360)
                return Color.FromArgb(C, 0, X);
            else
                return Color.FromName("Black");
        }

        private void rumbleBoostBar_ValueChanged(object sender, EventArgs e)
        {
            rumbleBoostMotorValLabel.Text = rumbleBoostBar.Value.ToString();
            Global.saveRumbleBoost(device, (byte)rumbleBoostBar.Value);
            scpDevice.setRumble((byte)leftMotorBar.Value, (byte)rightMotorBar.Value, device);

        }

        private void leftMotorBar_ValueChanged(object sender, EventArgs e)
        {
            leftMotorValLabel.Text = leftMotorBar.Value.ToString();
            scpDevice.setRumble((byte)leftMotorBar.Value, (byte)rightMotorBar.Value, device);
        }

        private void rightMotorBar_ValueChanged(object sender, EventArgs e)
        {
            rightMotorValLabel.Text = rightMotorBar.Value.ToString();
            scpDevice.setRumble((byte)leftMotorBar.Value, (byte)rightMotorBar.Value, device);
        }

        private void numUDTouch_ValueChanged(object sender, EventArgs e)
        {
            Global.setTouchSensitivity(device, (byte)numUDTouch.Value);
        }

        private void numUDTap_ValueChanged(object sender, EventArgs e)
        {
            Global.setTapSensitivity(device, (byte)numUDTap.Value);
            if (numUDTap.Value == 0)
                cBDoubleTap.Enabled = false;
            else
                cBDoubleTap.Enabled = true;
        }

        private void numUDScroll_ValueChanged(object sender, EventArgs e)
        {
            Global.setScrollSensitivity(device, (byte)numUDScroll.Value);
        }

        private void lowBatteryLed_CheckedChanged(object sender, EventArgs e)
        {
        }
        private void ledAsBatteryIndicator_CheckedChanged(object sender, EventArgs e)
        {
            Global.setLedAsBatteryIndicator(device, batteryLed.Checked);

            // New settings
            if (batteryLed.Checked)
            {
                lowLedPanel.Visible = true;
                //lowLedCheckBox.Visible = true;
                fullLedPanel.Size = new Size(174, 127);
                fullColorLabel.Visible = true;


                Global.setLedAsBatteryIndicator(device, true);
            }
            else
            {
                lowLedPanel.Visible = false;
                //lowLedCheckBox.Visible = false;
                fullLedPanel.Size = new Size(351, 127);
                fullColorLabel.Visible = false;

                Global.setLedAsBatteryIndicator(device, false);
            }
        }
        private void flashWhenLowBattery_CheckedChanged(object sender, EventArgs e)
        {
            Global.setFlashWhenLowBattery(device, flashLed.Checked);
        }
        private void lowerRCOffCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            Global.setLowerRCOn(device, cBlowerRCOn.Checked);
        }

        private void touchpadJitterCompensation_CheckedChanged(object sender, EventArgs e)
        {

            Global.setTouchpadJitterCompensation(device, touchpadJitterCompensation.Checked);
        }

        private void pictureBox_Click(object sender, EventArgs e)
        {
            colorChooserButton_Click(sender, e);
        }
        private void colorChooserButton_Click(object sender, EventArgs e)
        {
            advColorDialog.Color = Color.FromArgb(redBar.Value, greenBar.Value, blueBar.Value);
            advColorDialog_OnUpdateColor(colorChooserButton.BackColor, e);
            if (advColorDialog.ShowDialog() == DialogResult.OK)
            {
                redValLabel.Text = advColorDialog.Color.R.ToString();
                greenValLabel.Text = advColorDialog.Color.G.ToString();
                blueValLabel.Text = advColorDialog.Color.B.ToString();
                colorChooserButton.BackColor = advColorDialog.Color;
                redBar.Value = advColorDialog.Color.R;
                greenBar.Value = advColorDialog.Color.G;
                blueBar.Value = advColorDialog.Color.B;
            }
            else Global.saveColor(device, oldLedColor[0], oldLedColor[1], oldLedColor[2]);
            //Global.saveLowColor(device, oldLowLedColor[0], oldLowLedColor[1], oldLowLedColor[2]);
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
                    lowRedBar.Value = advColorDialog.Color.R;
                    lowGreenBar.Value = advColorDialog.Color.G;
                    lowBlueBar.Value = advColorDialog.Color.B;
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

        private void idleDisconnectTimeout_ValueChanged(object sender, EventArgs e)
        {
            if (idleDisconnectTimeout.Value <= 29 && idleDisconnectTimeout.Value > 15)
            {
                idleDisconnectTimeout.Value = 0;
                Global.setIdleDisconnectTimeout(device, (int)idleDisconnectTimeout.Value);
            }
            else if (idleDisconnectTimeout.Value > 0 && idleDisconnectTimeout.Value <= 15)
            {
                idleDisconnectTimeout.Value = 30;
                Global.setIdleDisconnectTimeout(device, (int)idleDisconnectTimeout.Value);
            }
            else
                Global.setIdleDisconnectTimeout(device, (int)idleDisconnectTimeout.Value);
        }

        private void rumbleSwap_CheckedChanged(object sender, EventArgs e)
        {
            Global.setRumbleSwap(device, rumbleSwap.Checked);
        }

        private void Options_Closed(object sender, FormClosedEventArgs e)
        {
            for (int i = 0; i < 4; i++)
                Global.LoadProfile(i); //Refreshes all profiles in case other controllers are using the same profile
            mainWin.RefreshProfiles();
        }

        private void tBProfile_TextChanged(object sender, EventArgs e)
        {
            if (tBProfile.Text != null && tBProfile.Text != "" && !tBProfile.Text.Contains("\\") && !tBProfile.Text.Contains("/") && !tBProfile.Text.Contains(":") && !tBProfile.Text.Contains("*") && !tBProfile.Text.Contains("?") && !tBProfile.Text.Contains("\"") && !tBProfile.Text.Contains("<") && !tBProfile.Text.Contains(">") && !tBProfile.Text.Contains("|"))
            {
                tBProfile.ForeColor = System.Drawing.SystemColors.WindowText;
            }
            else
                tBProfile.ForeColor = System.Drawing.SystemColors.GrayText;
        }

        private void tBProfile_Enter(object sender, EventArgs e)
        {
            if (tBProfile.Text == "<type profile name here>")
                tBProfile.Text = "";
        }

        private void tBProfile_Leave(object sender, EventArgs e)
        {
            if (tBProfile.Text == "")
                tBProfile.Text = "<type profile name here>";
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (cBSlide.Checked)
                numUDTouch.Value = 100;
            else
                numUDTouch.Value = 0;
            numUDTouch.Enabled = cBSlide.Checked;
        }

        private void cBScroll_CheckedChanged(object sender, EventArgs e)
        {
            if (cBScroll.Checked)
                numUDScroll.Value = 5;
            else
                numUDScroll.Value = 0;
            numUDScroll.Enabled = cBScroll.Checked;
        }

        private void cBTap_CheckedChanged(object sender, EventArgs e)
        {
            if (cBTap.Checked)
                numUDTap.Value = 100;
            else
                numUDTap.Value = 0;
            numUDTap.Enabled = cBTap.Checked;
        }

        private void cBDoubleTap_CheckedChanged(object sender, EventArgs e)
        {
            Global.setDoubleTap(device, cBDoubleTap.Checked);
        }

        private void tbProfile_EnterDown(object sender, KeyEventArgs e)
        {
            if (e.KeyValue == 13)
                saveButton_Click(sender, e);
        }

        private void lBControls_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        public void UpdateLists()
        {
            lBControls.Items[0] = "Cross : " + bnCross.Text;
            lBControls.Items[1] = "Circle : " + bnCircle.Text;
            lBControls.Items[2] = "Sqaure : " + bnSquare.Text;
            lBControls.Items[3] = "Triangle : " + bnTriangle.Text;
            lBControls.Items[4] = "Options : " + bnOptions.Text;
            lBControls.Items[5] = "Share : " + bnShare.Text;
            lBControls.Items[6] = "Up : " + bnUp.Text;
            lBControls.Items[7] = "Down : " + bnDown.Text;
            lBControls.Items[8] = "Left : " + bnLeft.Text;
            lBControls.Items[9] = "Right : " + bnRight.Text;
            lBControls.Items[10] = "PS : " + bnPS.Text;
            lBControls.Items[11] = "L1 : " + bnL1.Text;
            lBControls.Items[12] = "R1 : " + bnR1.Text;
            lBControls.Items[13] = "L2 : " + bnL2.Text;
            lBControls.Items[14] = "R2 : " + bnR2.Text;
            lBControls.Items[15] = "L3 : " + bnL3.Text;
            lBControls.Items[16] = "R3 : " + bnR3.Text;
            lBControls.Items[17] = "Left Stick : " + bnLSUp.Text + ", " + bnLSDown.Text + ", " + bnLSLeft.Text + ", " + bnLSRight.Text;
            lBControls.Items[18] = "Right Stick : " + bnRSUp.Text + ", " + bnRSDown.Text + ", " + bnRSLeft.Text + ", " + bnRSRight.Text;
            lBControls.Items[19] = "Touchpad : " + bnTouchLeft.Text + ", " + bnTouchUpper.Text + ", " + bnTouchMulti.Text + ", " + bnTouchRight.Text;
            lBAnalogSticks.Items[0] = lBControls.Items[15];
            lBAnalogSticks.Items[1] = lBControls.Items[16];
            lBAnalogSticks.Items[2] = "LS Up : " + bnLSUp.Text;
            lBAnalogSticks.Items[3] = "LS Down : " + bnLSDown.Text;
            lBAnalogSticks.Items[4] = "LS Left :" + bnLSLeft.Text;
            lBAnalogSticks.Items[5] = "LS Right : " + bnLSRight.Text;
            lBAnalogSticks.Items[6] = "RS Up : " + bnRSUp.Text;
            lBAnalogSticks.Items[7] = "RS Down : " + bnRSDown.Text;
            lBAnalogSticks.Items[8] = "RS Left : " + bnRSLeft.Text;
            lBAnalogSticks.Items[9] = "RS Right : " + bnRSRight.Text;
            lBTouchControls.Items[0] = "Left Side : " + bnTouchLeft.Text;
            lBTouchControls.Items[1] = "Upperpad : " + bnTouchUpper.Text;
            lBTouchControls.Items[2] = "Multitouch : " + bnTouchMulti.Text;
            lBTouchControls.Items[3] = "Right Side : " + bnTouchRight.Text;
        }

        private void Show_ControlsList(object sender, EventArgs e)
        {
            if (tabOptions.SelectedTab == tabControls)
            {
                if (lBControls.SelectedIndex == 0) Show_ControlsBn(bnCross, e);
                if (lBControls.SelectedIndex == 1) Show_ControlsBn(bnCircle, e);
                if (lBControls.SelectedIndex == 2) Show_ControlsBn(bnSquare, e);
                if (lBControls.SelectedIndex == 3) Show_ControlsBn(bnTriangle, e);
                if (lBControls.SelectedIndex == 4) Show_ControlsBn(bnOptions, e);
                if (lBControls.SelectedIndex == 5) Show_ControlsBn(bnShare, e);
                if (lBControls.SelectedIndex == 6) Show_ControlsBn(bnUp, e);
                if (lBControls.SelectedIndex == 7) Show_ControlsBn(bnDown, e);
                if (lBControls.SelectedIndex == 8) Show_ControlsBn(bnLeft, e);
                if (lBControls.SelectedIndex == 9) Show_ControlsBn(bnRight, e);
                if (lBControls.SelectedIndex == 10) Show_ControlsBn(bnPS, e);
                if (lBControls.SelectedIndex == 11) Show_ControlsBn(bnL1, e);
                if (lBControls.SelectedIndex == 12) Show_ControlsBn(bnR1, e);
                if (lBControls.SelectedIndex == 13) Show_ControlsBn(bnL2, e);
                if (lBControls.SelectedIndex == 14) Show_ControlsBn(bnR2, e);
                if (lBControls.SelectedIndex == 15) Show_ControlsBn(bnL3, e);
                if (lBControls.SelectedIndex == 16) Show_ControlsBn(bnR3, e);
                if (lBControls.SelectedIndex == 17) tabOptions.SelectTab(1);
                if (lBControls.SelectedIndex == 18) tabOptions.SelectTab(1);
                if (lBControls.SelectedIndex == 19) tabOptions.SelectTab(2);
            }
            else if (tabOptions.SelectedTab == tabAnalogSticks)
            {
                if (lBAnalogSticks.SelectedIndex == 0) Show_ControlsBn(bnL3, e);
                if (lBAnalogSticks.SelectedIndex == 1) Show_ControlsBn(bnR3, e);
                if (lBAnalogSticks.SelectedIndex == 2) Show_ControlsBn(bnLSUp, e);
                if (lBAnalogSticks.SelectedIndex == 3) Show_ControlsBn(bnLSDown, e);
                if (lBAnalogSticks.SelectedIndex == 4) Show_ControlsBn(bnLSLeft, e);
                if (lBAnalogSticks.SelectedIndex == 5) Show_ControlsBn(bnLSRight, e);
                if (lBAnalogSticks.SelectedIndex == 6) Show_ControlsBn(bnRSUp, e);
                if (lBAnalogSticks.SelectedIndex == 7) Show_ControlsBn(bnRSDown, e);
                if (lBAnalogSticks.SelectedIndex == 8) Show_ControlsBn(bnRSLeft, e);
                if (lBAnalogSticks.SelectedIndex == 9) Show_ControlsBn(bnRSRight, e);
            }
            else if (tabOptions.SelectedTab == tabTouchPad)
            {
                if (lBTouchControls.SelectedIndex == 0) Show_ControlsBn(bnTouchLeft, e);
                if (lBTouchControls.SelectedIndex == 1) Show_ControlsBn(bnTouchUpper, e);
                if (lBTouchControls.SelectedIndex == 2) Show_ControlsBn(bnTouchMulti, e);
                if (lBTouchControls.SelectedIndex == 3) Show_ControlsBn(bnTouchRight, e);
            }
        }

        private void List_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            Show_ControlsList(sender, e);
        }

        private void List_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyValue == 13)
                Show_ControlsList(sender, e);
        }

        private void tBMouseSens_Scroll(object sender, EventArgs e)
        {
            Global.setButtonMouseSensitivity(device, tBMouseSens.Value);
            lBMouseSens.Text = tBMouseSens.Value.ToString();
        }

        private void numUDRainbow_ValueChanged(object sender, EventArgs e)
        {
            Global.setRainbow(device, (double)numUDRainbow.Value);
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            if (pBRainbow.Image == greyscale)
            {
                pBRainbow.Image = colored;
                ToggleRainbow(true);
                numUDRainbow.Value = 5;
            }
            else
            {
                pBRainbow.Image = greyscale;
                ToggleRainbow(false);
                numUDRainbow.Value = 0;
            }
        }

        private void ToggleRainbow(bool on)
        {
            numUDRainbow.Visible = on;
            if (on)
            {
                pBRainbow.Location = new Point(137, 139);
                batteryLed.Text = "Battery Level Dim";
            }
            else
            {
                pBRainbow.Location = new Point(215, 139);
                batteryLed.Text = "Battery Level Color";
            }
            lBspc.Visible = on;
            fullLedPanel.Enabled = !on;
            lowLedPanel.Enabled = !on;
            flashLed.Enabled = !on;
        }

        private Bitmap GreyscaleImage(Bitmap image)
        {
            Bitmap c = (Bitmap)image;
            Bitmap d = new Bitmap(c.Width, c.Height);

            for (int i = 0; i < c.Width; i++)
            {
                for (int x = 0; x < c.Height; x++)
                {
                    Color oc = c.GetPixel(i, x);
                    int grayScale = (int)((oc.R * 0.3) + (oc.G * 0.59) + (oc.B * 0.11));
                    Color nc = Color.FromArgb(oc.A, grayScale, grayScale, grayScale);
                    d.SetPixel(i, x, nc);
                }
            }
            return d;
        }

        private void fullColorLabel_Click(object sender, EventArgs e)
        {

        }

        private void tabLightBar_Click(object sender, EventArgs e)
        {

        }
    }
}
