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
        public int device;
        public string filename;
        Byte[] oldLedColor, oldLowLedColor, oldChargingColor;
        public Timer inputtimer = new Timer(), sixaxisTimer = new Timer();
        public List<Button> buttons = new List<Button>(), subbuttons = new List<Button>();
        private Button lastSelected;
        private int alphacolor;
        private Color reg, full;
        private Image colored, greyscale;
        ToolTip tp = new ToolTip();
        Graphics g;
        ScpForm root;
        bool olddinputcheck = false;
        public Options(DS4Control.Control bus_device, int deviceNum, string name, ScpForm rt)
        {
            InitializeComponent();
            device = deviceNum;
            scpDevice = bus_device;
            filename = name;
            colored = pBRainbow.Image;
            root = rt;
            g = CreateGraphics();
            greyscale = GreyscaleImage((Bitmap)pBRainbow.Image);
            foreach (System.Windows.Forms.Control control in SticksPanel.Controls)
                if (control is Button)
                    if (!((Button)control).Name.Contains("btn"))
                        buttons.Add((Button)control);
            foreach (System.Windows.Forms.Control control in MainPanel.Controls)
                if (control is Button)
                    if (!((Button)control).Name.Contains("btn"))
                        buttons.Add((Button)control);
            foreach (System.Windows.Forms.Control control in fLPTiltControls.Controls)
                if (control is Button)
                    if (!((Button)control).Name.Contains("btn"))
                        buttons.Add((Button)control);
            foreach (System.Windows.Forms.Control control in ShiftMainPanel.Controls)
                if (control is Button)
                    if (!((Button)control).Name.Contains("sbtn"))
                        subbuttons.Add((Button)control);
            foreach (System.Windows.Forms.Control control in ShiftSticksPanel.Controls)
                if (control is Button)
                    if (!((Button)control).Name.Contains("sbtn"))
                        subbuttons.Add((Button)control);
            foreach (System.Windows.Forms.Control control in sfLPTiltControls.Controls)
                if (control is Button)
                    if (!((Button)control).Name.Contains("sbtn"))
                        subbuttons.Add((Button)control);
            string butts = "";
            foreach (Button b in buttons)
                butts += "\n" + b.Name;
            //MessageBox.Show(butts);
            root.lbLastMessage.ForeColor = Color.Black;
            root.lbLastMessage.Text = "Hover over items to see description or more about";
            foreach (System.Windows.Forms.Control control in Controls)
                if (control.HasChildren)
                    foreach (System.Windows.Forms.Control ctrl in control.Controls)
                        if (ctrl.HasChildren)
                            foreach (System.Windows.Forms.Control ctrl2 in ctrl.Controls)
                                if (ctrl2.HasChildren)
                                    foreach (System.Windows.Forms.Control ctrl3 in ctrl2.Controls)
                                        ctrl3.MouseHover += Items_MouseHover;
                                else
                                    ctrl2.MouseHover += Items_MouseHover;
                        else
                            ctrl.MouseHover += Items_MouseHover;
                else
                    control.MouseHover += Items_MouseHover;
            if (device < 4)
            nUDSixaxis.Value = deviceNum + 1;
            if (filename != "")
            {
                if (device == 4)
                    Global.setAProfile(4, name);
                Global.LoadProfile(device, buttons.ToArray(), subbuttons.ToArray(), false, scpDevice);
                DS4Color color = Global.loadColor(device);
                tBRedBar.Value = color.red;
                tBGreenBar.Value = color.green;
                tBBlueBar.Value = color.blue;

                cBLightbyBattery.Checked = Global.getLedAsBatteryIndicator(device);
                nUDflashLED.Value = Global.getFlashAt(device);
                lowBatteryPanel.Visible = cBLightbyBattery.Checked;
                lbFull.Text = (cBLightbyBattery.Checked ? "Full:" : "Color:");
                FullPanel.Location = (cBLightbyBattery.Checked ? new Point(FullPanel.Location.X, 42) : new Point(FullPanel.Location.X, 48));

                DS4Color lowColor = Global.loadLowColor(device);
                tBLowRedBar.Value = lowColor.red;
                tBLowGreenBar.Value = lowColor.green;
                tBLowBlueBar.Value = lowColor.blue;

                DS4Color shiftColor = Global.loadShiftColor(device);
                tBShiftRedBar.Value = shiftColor.red;
                tBShiftGreenBar.Value = shiftColor.green;
                tBShiftBlueBar.Value = shiftColor.blue;
                cBShiftLight.Checked = Global.getShiftColorOn(device);

                DS4Color cColor = Global.loadChargingColor(device);
                btnChargingColor.BackColor = Color.FromArgb(cColor.red, cColor.green, cColor.blue);

                DS4Color fColor = Global.loadFlashColor(device);
                lbFlashAt.ForeColor = Color.FromArgb(fColor.red, fColor.green, fColor.blue);
                if (lbFlashAt.ForeColor.GetBrightness() > .5f)
                    lbFlashAt.BackColor = Color.Black;
                lbPercentFlashBar.ForeColor = lbFlashAt.ForeColor;
                lbPercentFlashBar.BackColor = lbFlashAt.BackColor;
                nUDRumbleBoost.Value = Global.loadRumbleBoost(device);
                nUDTouch.Value = Global.getTouchSensitivity(device);
                cBSlide.Checked = Global.getTouchSensitivity(device) > 0;
                nUDScroll.Value = Global.getScrollSensitivity(device);
                cBScroll.Checked = Global.getScrollSensitivity(device) > 0;
                nUDTap.Value = Global.getTapSensitivity(device);
                cBTap.Checked = Global.getTapSensitivity(device) > 0;
                cBDoubleTap.Checked = Global.getDoubleTap(device);
                nUDL2.Value = (decimal)Global.getLeftTriggerMiddle(device) / 255;
                nUDR2.Value = (decimal)Global.getRightTriggerMiddle(device) / 255;
                cBTouchpadJitterCompensation.Checked = Global.getTouchpadJitterCompensation(device);
                cBlowerRCOn.Checked = Global.getLowerRCOn(device);
                cBFlushHIDQueue.Checked = Global.getFlushHIDQueue(device);
                nUDIdleDisconnect.Value = Math.Round((decimal)(Global.getIdleDisconnectTimeout(device) / 60d), 1);
                cBIdleDisconnect.Checked = Global.getIdleDisconnectTimeout(device) > 0;
                numUDMouseSens.Value = Global.getButtonMouseSensitivity(device);
                cBMouseAccel.Checked = Global.getMouseAccel(device);
                // Force update of color choosers    
                alphacolor = Math.Max(tBRedBar.Value, Math.Max(tBGreenBar.Value, tBBlueBar.Value));
                reg = Color.FromArgb(color.red, color.green, color.blue);
                full = HuetoRGB(reg.GetHue(), reg.GetBrightness(), reg);
                pBController.BackColor = Color.FromArgb((alphacolor > 205 ? 255 : (alphacolor + 50)), full);

                alphacolor = Math.Max(tBLowRedBar.Value, Math.Max(tBGreenBar.Value, tBBlueBar.Value));
                reg = Color.FromArgb(lowColor.red, lowColor.green, lowColor.blue);
                full = HuetoRGB(reg.GetHue(), reg.GetBrightness(), reg);
                lowColorChooserButton.BackColor = Color.FromArgb((alphacolor > 205 ? 255 : (alphacolor + 50)), full);
                nUDRainbow.Value = (decimal)Global.getRainbow(device);
                switch (Global.getChargingType(deviceNum))
                {
                    case 1: rBFade.Checked = true; break;
                    case 2: rBRainbow.Checked = true; break;
                    case 3: rBColor.Checked = true; break;
                    default: rBNormal.Checked = true; break;
                }
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
                nUDLS.Value = Math.Round((decimal)(Global.getLSDeadzone(device) / 127d), 3);
                nUDRS.Value = Math.Round((decimal)(Global.getRSDeadzone(device) / 127d), 3);
                nUDSX.Value = (decimal)Global.getSXDeadzone(device);
                nUDSZ.Value = (decimal)Global.getSZDeadzone(device);
                cBShiftControl.SelectedIndex = Global.getShiftModifier(device);
                if (Global.getLaunchProgram(device) != string.Empty)
                {
                    cBLaunchProgram.Checked = true;
                    pBProgram.Image = Icon.ExtractAssociatedIcon(Global.getLaunchProgram(device)).ToBitmap();
                    btnBrowse.Text = Path.GetFileNameWithoutExtension(Global.getLaunchProgram(device));
                }
                cBDinput.Checked = Global.getDinputOnly(device);
                olddinputcheck = cBDinput.Checked;
                cbStartTouchpadOff.Checked = Global.getStartTouchpadOff(device);
            }
            else
            {
                Set();
                switch (device)
                {
                    case 0: tBRedBar.Value = 0; tBGreenBar.Value = 0; break;
                    case 1: tBGreenBar.Value = 0; tBBlueBar.Value = 0; break;
                    case 2: tBRedBar.Value = 0; tBBlueBar.Value = 0; break;
                    case 3: tBGreenBar.Value = 0; break;
                    case 4: tBRedBar.Value = 0; tBGreenBar.Value = 0; break;
                }
            }
            foreach (Button b in buttons)
                b.MouseHover += button_MouseHover;
            foreach (Button b in subbuttons)
                b.MouseHover += button_MouseHover;
           
            
            advColorDialog.OnUpdateColor += advColorDialog_OnUpdateColor;
            btnLeftStick.Enter += btnSticks_Enter;
            btnRightStick.Enter += btnSticks_Enter;
            sbtnLeftStick.Enter += sbtnSticks_Enter;
            sbtnRightStick.Enter += sbtnSticks_Enter;
            UpdateLists();
            inputtimer.Start();
            inputtimer.Tick += InputDS4;
            sixaxisTimer.Tick += sixaxisTimer_Tick;
            sixaxisTimer.Interval = 1000 / 60;
            
        }

        void sixaxisTimer_Tick(object sender, EventArgs e)
        {            
            // MEMS gyro data is all calibrated to roughly -1G..1G for values -0x2000..0x1fff
            // Enough additional acceleration and we are no longer mostly measuring Earth's gravity...
            // We should try to indicate setpoints of the calibration when exposing this measurement....
            if (scpDevice.DS4Controllers[(int)nUDSixaxis.Value - 1] == null)
            {
                tPController.Enabled = false;
                lbInputDelay.Text = Properties.Resources.InputDelay.Replace("*number*", Properties.Resources.NA).Replace("*ms*", "ms");
                pBDelayTracker.BackColor = Color.Transparent;
            }
            else
            {
                tPController.Enabled = true;
                SetDynamicTrackBarValue(tBsixaxisGyroX, (scpDevice.ExposedState[(int)nUDSixaxis.Value - 1].GyroX + tBsixaxisGyroX.Value * 2) / 3);
                SetDynamicTrackBarValue(tBsixaxisGyroY, (scpDevice.ExposedState[(int)nUDSixaxis.Value - 1].GyroY + tBsixaxisGyroY.Value * 2) / 3);
                SetDynamicTrackBarValue(tBsixaxisGyroZ, (scpDevice.ExposedState[(int)nUDSixaxis.Value - 1].GyroZ + tBsixaxisGyroZ.Value * 2) / 3);
                SetDynamicTrackBarValue(tBsixaxisAccelX, (scpDevice.ExposedState[(int)nUDSixaxis.Value - 1].AccelX + tBsixaxisAccelX.Value * 2) / 3);
                SetDynamicTrackBarValue(tBsixaxisAccelY, (scpDevice.ExposedState[(int)nUDSixaxis.Value - 1].AccelY + tBsixaxisAccelY.Value * 2) / 3);
                SetDynamicTrackBarValue(tBsixaxisAccelZ, (scpDevice.ExposedState[(int)nUDSixaxis.Value - 1].AccelZ + tBsixaxisAccelZ.Value * 2) / 3);
                int x = scpDevice.getDS4State((int)nUDSixaxis.Value - 1).LX;
                int y = scpDevice.getDS4State((int)nUDSixaxis.Value - 1).LY;
                btnLSTrack.Location = new Point((int)(x / 2.09 + lbLSTrack.Location.X), (int)(y / 2.09 + lbLSTrack.Location.Y));
                x = scpDevice.getDS4State((int)nUDSixaxis.Value - 1).RX;
                y = scpDevice.getDS4State((int)nUDSixaxis.Value - 1).RY;
                btnRSTrack.Location = new Point((int)(x / 2.09 + lbRSTrack.Location.X), (int)(y / 2.09 + lbRSTrack.Location.Y));
                x = -scpDevice.ExposedState[(int)nUDSixaxis.Value - 1].GyroX / 62 + 127;
                y = scpDevice.ExposedState[(int)nUDSixaxis.Value - 1].GyroZ / 62 + 127;
                btnSATrack.Location = new Point((int)(x / 2.09 + lbSATrack.Location.X), (int)(y / 2.09 + lbSATrack.Location.Y));
                tBL2.Value = scpDevice.getDS4State((int)nUDSixaxis.Value - 1).L2;
                lbL2Track.Location = new Point(tBL2.Location.X - 15, (int)(24 - tBL2.Value / 10.625) + 10);
                if (tBL2.Value == 255)
                    lbL2Track.ForeColor = Color.Green;
                else if (tBL2.Value < (double)nUDL2.Value * 255)
                    lbL2Track.ForeColor = Color.Red;
                else
                    lbL2Track.ForeColor = Color.Black;
                tBR2.Value = scpDevice.getDS4State((int)nUDSixaxis.Value - 1).R2;
                lbR2Track.Location = new Point(tBR2.Location.X + 20, (int)(24 - tBR2.Value / 10.625) + 10);
                if (tBR2.Value == 255)
                    lbR2Track.ForeColor = Color.Green;
                else if (tBR2.Value < (double)nUDR2.Value * 255)
                    lbR2Track.ForeColor = Color.Red;
                else
                    lbR2Track.ForeColor = Color.Black;
                double latency = scpDevice.DS4Controllers[(int)nUDSixaxis.Value - 1].Latency;
                lbInputDelay.Text = Properties.Resources.InputDelay.Replace("*number*", latency.ToString()).Replace("*ms*", "ms");
                if (latency > 10)
                    pBDelayTracker.BackColor = Color.Red;
                else if (latency > 5)
                    pBDelayTracker.BackColor = Color.Yellow;
                else
                    pBDelayTracker.BackColor = Color.Green;
            }
        }
        private void InputDS4(object sender, EventArgs e)
        {
            #region DS4Input
            if (Form.ActiveForm == root && cBControllerInput.Checked && tabControls.SelectedIndex != 2)
            switch (scpDevice.GetInputkeys((int)nUDSixaxis.Value - 1))
                {
                    case ("Cross"): Show_ControlsBn(bnCross, e); break;
                    case ("Circle"): Show_ControlsBn(bnCircle, e); break;
                    case ("Square"): Show_ControlsBn(bnSquare, e); break;
                    case ("Triangle"): Show_ControlsBn(bnTriangle, e); break;
                    case ("Options"): Show_ControlsBn(bnOptions, e); break;
                    case ("Share"): Show_ControlsBn(bnShare, e); break;
                    case ("Up"): Show_ControlsBn(bnUp, e); break;
                    case ("Down"): Show_ControlsBn(bnDown, e); break;
                    case ("Left"): Show_ControlsBn(bnLeft, e); break;
                    case ("Right"): Show_ControlsBn(bnRight, e); break;
                    case ("PS"): Show_ControlsBn(bnPS, e); break;
                    case ("L1"): Show_ControlsBn(bnL1, e); break;
                    case ("R1"): Show_ControlsBn(bnR1, e); break;
                    case ("L2"): Show_ControlsBn(bnL2, e); break;
                    case ("R2"): Show_ControlsBn(bnR2, e); break;
                    case ("L3"): Show_ControlsBn(bnL3, e); break;
                    case ("R3"): Show_ControlsBn(bnR3, e); break;
                    case ("Touch Left"): Show_ControlsBn(bnTouchLeft, e); break;
                    case ("Touch Right"): Show_ControlsBn(bnTouchRight, e); break;
                    case ("Touch Multi"): Show_ControlsBn(bnTouchMulti, e); break;
                    case ("Touch Upper"): Show_ControlsBn(bnTouchUpper, e); break;
                    case ("LS Up"): Show_ControlsBn(bnLSUp, e); break;
                    case ("LS Down"): Show_ControlsBn(bnLSDown, e); break;
                    case ("LS Left"): Show_ControlsBn(bnLSLeft, e); break;
                    case ("LS Right"): Show_ControlsBn(bnLSRight, e); break;
                    case ("RS Up"): Show_ControlsBn(bnRSUp, e); break;
                    case ("RS Down"): Show_ControlsBn(bnRSDown, e); break;
                    case ("RS Left"): Show_ControlsBn(bnRSLeft, e); break;
                    case ("RS Right"): Show_ControlsBn(bnRSRight, e); break;
                    case ("GyroXP"): Show_ControlsBn(bnGyroXP, e); break;
                    case ("GyroXN"): Show_ControlsBn(bnGyroXN, e); break;
                    case ("GyroZP"): Show_ControlsBn(bnGyroZP, e); break;
                    case ("GyroZN"): Show_ControlsBn(bnGyroZN, e); break;
                }
            #endregion
        }
        private void button_MouseHover(object sender, EventArgs e)
        {
            switch (((Button)sender).Name)
            {
                #region
                case ("bnCross"): lBControls.SelectedIndex = 0; break;
                case ("bnCircle"): lBControls.SelectedIndex = 1; break;
                case ("bnSquare"): lBControls.SelectedIndex = 2; break;
                case ("bnTriangle"): lBControls.SelectedIndex = 3; break;
                case ("bnOptions"): lBControls.SelectedIndex = 4; break;
                case ("bnShare"): lBControls.SelectedIndex = 5; break;
                case ("bnUp"): lBControls.SelectedIndex = 6; break;
                case ("bnDown"): lBControls.SelectedIndex = 7; break;
                case ("bnLeft"): lBControls.SelectedIndex = 8; break;
                case ("bnRight"): lBControls.SelectedIndex = 9; break;
                case ("bnPS"): lBControls.SelectedIndex = 10; break;
                case ("bnL1"): lBControls.SelectedIndex = 11; break;
                case ("bnR1"): lBControls.SelectedIndex = 12; break;
                case ("bnL2"): lBControls.SelectedIndex = 13; break;
                case ("bnR2"): lBControls.SelectedIndex = 14; break;
                case ("bnL3"): lBControls.SelectedIndex = 15; break;
                case ("bnR3"): lBControls.SelectedIndex = 16; break;
                case ("bnTouchLeft"): lBControls.SelectedIndex = 17; break;
                case ("bnTouchRight"): lBControls.SelectedIndex = 18; break;
                case ("bnTouchMulti"): lBControls.SelectedIndex = 19; break;
                case ("bnTouchUpper"): lBControls.SelectedIndex = 20; break;
                case ("bnLSUp"): lBControls.SelectedIndex = 21; break;
                case ("bnLSDown"): lBControls.SelectedIndex = 22; break;
                case ("bnLSLeft"): lBControls.SelectedIndex = 23; break;
                case ("bnLSRight"): lBControls.SelectedIndex = 24; break;
                case ("bnRSUp"): lBControls.SelectedIndex = 25; break;
                case ("bnRSDown"): lBControls.SelectedIndex = 26; break;
                case ("bnRSLeft"): lBControls.SelectedIndex = 27; break;
                case ("bnRSRight"): lBControls.SelectedIndex = 28; break;
                case ("bnGyroZN"): lBControls.SelectedIndex = 29; break;
                case ("bnGyroZP"): lBControls.SelectedIndex = 30; break;
                case ("bnGyroXP"): lBControls.SelectedIndex = 31; break;
                case ("bnGyroXN"): lBControls.SelectedIndex = 32; break;

                case ("sbnCross"): lBShiftControls.SelectedIndex = 0; break;
                case ("sbnCircle"): lBShiftControls.SelectedIndex = 1; break;
                case ("sbnSquare"): lBShiftControls.SelectedIndex = 2; break;
                case ("sbnTriangle"): lBShiftControls.SelectedIndex = 3; break;
                case ("sbnOptions"): lBShiftControls.SelectedIndex = 4; break;
                case ("sbnShare"): lBShiftControls.SelectedIndex = 5; break;
                case ("sbnUp"): lBShiftControls.SelectedIndex = 6; break;
                case ("sbnDown"): lBShiftControls.SelectedIndex = 7; break;
                case ("sbnLeft"): lBShiftControls.SelectedIndex = 8; break;
                case ("sbnRight"): lBShiftControls.SelectedIndex = 9; break;
                case ("sbnPS"): lBShiftControls.SelectedIndex = 10; break;
                case ("sbnL1"): lBShiftControls.SelectedIndex = 11; break;
                case ("sbnR1"): lBShiftControls.SelectedIndex = 12; break;
                case ("sbnL2"): lBShiftControls.SelectedIndex = 13; break;
                case ("sbnR2"): lBShiftControls.SelectedIndex = 14; break;
                case ("sbnL3"): lBShiftControls.SelectedIndex = 15; break;
                case ("sbnR3"): lBShiftControls.SelectedIndex = 16; break;
                case ("sbnTouchLeft"): lBShiftControls.SelectedIndex = 17; break;
                case ("sbnTouchRight"): lBShiftControls.SelectedIndex = 18; break;
                case ("sbnTouchMulti"): lBShiftControls.SelectedIndex = 19; break;
                case ("sbnTouchUpper"): lBShiftControls.SelectedIndex = 20; break;
                case ("sbnLSUp"): lBShiftControls.SelectedIndex = 21; break;
                case ("sbnLSDown"): lBShiftControls.SelectedIndex = 22; break;
                case ("sbnLSLeft"): lBShiftControls.SelectedIndex = 23; break;
                case ("sbnLSRight"): lBShiftControls.SelectedIndex = 24; break;
                case ("sbnRSUp"): lBShiftControls.SelectedIndex = 25; break;
                case ("sbnRSDown"): lBShiftControls.SelectedIndex = 26; break;
                case ("sbnRSLeft"): lBShiftControls.SelectedIndex = 27; break;
                case ("sbnRSRight"): lBShiftControls.SelectedIndex = 28; break;
                case ("sbnGyroZN"): lBShiftControls.SelectedIndex = 29; break;
                case ("sbnGyroZP"): lBShiftControls.SelectedIndex = 30; break;
                case ("sbnGyroXP"): lBShiftControls.SelectedIndex = 31; break;
                case ("sbnGyroXN"): lBShiftControls.SelectedIndex = 32; break;
                #endregion
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

        public void Set()
        {
            lowBatteryPanel.Visible = cBLightbyBattery.Checked;
            lbFull.Text = (cBLightbyBattery.Checked ? Properties.Resources.Full + ":": Properties.Resources.Color + ":");
            FullPanel.Location = (cBLightbyBattery.Checked ? new Point(FullPanel.Location.X, 42) : new Point(FullPanel.Location.X, 48));
            Global.saveColor(device, (byte)tBRedBar.Value, (byte)tBGreenBar.Value, (byte)tBBlueBar.Value);
            Global.saveLowColor(device, (byte)tBLowRedBar.Value, (byte)tBLowGreenBar.Value, (byte)tBLowBlueBar.Value);
            Global.saveShiftColor(device, (byte)tBShiftRedBar.Value, (byte)tBShiftGreenBar.Value, (byte)tBShiftBlueBar.Value);
            Global.saveFlashColor(device, lbFlashAt.ForeColor.R, lbFlashAt.ForeColor.G, lbFlashAt.ForeColor.B);
            Global.setLeftTriggerMiddle(device, (byte)Math.Round((nUDL2.Value * 255), 0));
            Global.setRightTriggerMiddle(device, (byte)Math.Round((nUDR2.Value * 255), 0));
            Global.saveRumbleBoost(device, (byte)nUDRumbleBoost.Value);
            Global.setTouchSensitivity(device, (byte)nUDTouch.Value);
            Global.setTouchpadJitterCompensation(device, cBTouchpadJitterCompensation.Checked);
            Global.setLowerRCOn(device, cBlowerRCOn.Checked);
            Global.setScrollSensitivity(device, (byte)nUDScroll.Value);
            Global.setDoubleTap(device, cBDoubleTap.Checked);
            Global.setTapSensitivity(device, (byte)nUDTap.Value);
            Global.setIdleDisconnectTimeout(device, (int)(nUDIdleDisconnect.Value * 60));
            Global.setRainbow(device, (int)nUDRainbow.Value);
            Global.setRSDeadzone(device, (byte)Math.Round((nUDRS.Value * 127), 0));
            Global.setLSDeadzone(device, (byte)Math.Round((nUDLS.Value * 127), 0));
            Global.setButtonMouseSensitivity(device, (int)numUDMouseSens.Value);
            Global.setFlashAt(device, (int)nUDflashLED.Value);
            Global.setSXDeadzone(device, (double)nUDSX.Value);
            Global.setSZDeadzone(device, (double)nUDSZ.Value);
            Global.setMouseAccel(device, cBMouseAccel.Checked);
            Global.setShiftModifier(device, cBShiftControl.SelectedIndex);
            Global.setDinputOnly(device, cBDinput.Checked);
            Global.setStartTouchpadOff(device, cbStartTouchpadOff.Checked);

            if (nUDRainbow.Value == 0) pBRainbow.Image = greyscale;
            else pBRainbow.Image = colored;
        }

        KBM360 kbm360 = null;

        private void Show_ControlsBn(object sender, EventArgs e)
        {
            lastSelected = (Button)sender;
            kbm360 = new KBM360(scpDevice, device, this, lastSelected);
            kbm360.Icon = this.Icon;
            kbm360.ShowDialog();
        }

        public void ChangeButtonText(string controlname, object tag)
        {
            lastSelected.Text = controlname;
            int value;
            if (tag == null)
                lastSelected.Tag = tag;
            else if (Int32.TryParse(tag.ToString(), out value))
                lastSelected.Tag = value;
            else if (tag is Int32[])
                lastSelected.Tag = tag;
            else
                lastSelected.Tag = tag.ToString();
        }
        public void ChangeButtonText(string controlname)
        {
            lastSelected.Text = controlname;
            lastSelected.Tag = controlname;
        }
        public void Toggle_Bn(bool SC, bool TG, bool MC,  bool MR)
        {
            if (lastSelected.Tag is int || lastSelected.Tag is UInt16 || lastSelected.Tag is int[])
                lastSelected.Font = new Font(lastSelected.Font, 
                    (SC ? FontStyle.Bold : FontStyle.Regular) | (TG ? FontStyle.Italic : FontStyle.Regular) | 
                    (MC ? FontStyle.Underline : FontStyle.Regular) | (MR ? FontStyle.Strikeout : FontStyle.Regular));
            else if (lastSelected.Tag is string)
                if (lastSelected.Tag.ToString().Contains("Mouse Button"))
                    lastSelected.Font = new Font(lastSelected.Font, TG ? FontStyle.Italic : FontStyle.Regular);
            else
                lastSelected.Font = new Font(lastSelected.Font, FontStyle.Regular);
        }
        private void btnSticks_Enter(object sender, EventArgs e)
        {
            SticksPanel.Visible = true;
            MainPanel.Visible = false;
        }

        private void btnFullView_Click(object sender, EventArgs e)
        {
            SticksPanel.Visible = false;
            MainPanel.Visible = true;
        }

        private void sbtnSticks_Enter(object sender, EventArgs e)
        {
            ShiftSticksPanel.Visible = true;
            ShiftMainPanel.Visible = false;
        }

        private void sbtnFullView_Click(object sender, EventArgs e)
        {
            ShiftSticksPanel.Visible = false;
            ShiftMainPanel.Visible = true;
        }
        private void btnLightbar_Click(object sender, EventArgs e)
        {
            advColorDialog.Color = Color.FromArgb(tBRedBar.Value, tBGreenBar.Value, tBBlueBar.Value);
            advColorDialog_OnUpdateColor(pBController.BackColor, e);
            if (advColorDialog.ShowDialog() == DialogResult.OK)
            {
                pBController.BackColor = advColorDialog.Color;
                tBRedBar.Value = advColorDialog.Color.R;
                tBGreenBar.Value = advColorDialog.Color.G;
                tBBlueBar.Value = advColorDialog.Color.B;
            }
            else Global.saveColor(device, oldLedColor[0], oldLedColor[1], oldLedColor[2]);
            Global.saveChargingColor(device, oldChargingColor[0], oldChargingColor[1], oldChargingColor[2]);
            Global.saveLowColor(device, oldLowLedColor[0], oldLowLedColor[1], oldLowLedColor[2]);
            oldChargingColor = null;
            oldLedColor = null;
            oldLowLedColor = null;
        }
        private void lowColorChooserButton_Click(object sender, EventArgs e)
        {
            advColorDialog.Color = lowColorChooserButton.BackColor;
            advColorDialog_OnUpdateColor(lowColorChooserButton.BackColor, e);
            if (advColorDialog.ShowDialog() == DialogResult.OK)
            {
                lowColorChooserButton.BackColor = advColorDialog.Color;
                tBLowRedBar.Value = advColorDialog.Color.R;
                tBLowGreenBar.Value = advColorDialog.Color.G;
                tBLowBlueBar.Value = advColorDialog.Color.B;
            }
            else Global.saveLowColor(device, oldLowLedColor[0], oldLowLedColor[1], oldLowLedColor[2]);
            Global.saveChargingColor(device, oldChargingColor[0], oldChargingColor[1], oldChargingColor[2]);
            Global.saveColor(device, oldLedColor[0], oldLedColor[1], oldLedColor[2]);
            oldChargingColor = null;
            oldLedColor = null;
            oldLowLedColor = null;
        }


        private void btnChargingColor_Click(object sender, EventArgs e)
        {
            advColorDialog.Color = btnChargingColor.BackColor;
            advColorDialog_OnUpdateColor(btnChargingColor.BackColor, e);
            if (advColorDialog.ShowDialog() == DialogResult.OK)
            {
                btnChargingColor.BackColor = advColorDialog.Color;
            }
            else Global.saveChargingColor(device, oldChargingColor[0], oldChargingColor[1], oldChargingColor[2]);
            Global.saveLowColor(device, oldLowLedColor[0], oldLowLedColor[1], oldLowLedColor[2]);
            Global.saveColor(device, oldLedColor[0], oldLedColor[1], oldLedColor[2]);
            oldChargingColor = null;
            oldLedColor = null;
            oldLowLedColor = null;
        }
        private void advColorDialog_OnUpdateColor(object sender, EventArgs e)
        {
            if (oldLedColor == null || oldLowLedColor == null || oldChargingColor == null)
            {
                DS4Color color = Global.loadColor(device);
                oldLedColor = new Byte[] { color.red, color.green, color.blue };
                color = Global.loadLowColor(device);
                oldLowLedColor = new Byte[] { color.red, color.green, color.blue };
                color = Global.loadChargingColor(device);
                oldChargingColor = new Byte[] { color.red, color.green, color.blue };
            }
            if (sender is Color)
            {
                Color color = (Color)sender;
                Global.saveColor(device, color.R, color.G, color.B);
                Global.saveLowColor(device, color.R, color.G, color.B);
                Global.saveChargingColor(device, color.R, color.G, color.B);
            }
        }
        int bgc = 255; //Color of the form background, If greyscale color
        private void redBar_ValueChanged(object sender, EventArgs e)
        {
            int value = ((TrackBar)sender).Value;
            int sat = bgc - (value < bgc ? value : bgc);
            int som = bgc + 11 * (int)(value * 0.0039215);
            ((TrackBar)sender).BackColor = Color.FromArgb(som, sat, sat);
            alphacolor = Math.Max(tBRedBar.Value, Math.Max(tBGreenBar.Value, tBBlueBar.Value));
            reg = Color.FromArgb(tBRedBar.Value, tBGreenBar.Value, tBBlueBar.Value);
            full = HuetoRGB(reg.GetHue(), reg.GetBrightness(), reg);
            pBController.BackColor = Color.FromArgb((alphacolor > 205 ? 255 : (alphacolor + 50)), full);
            Global.saveColor(device, (byte)tBRedBar.Value, (byte)tBGreenBar.Value, (byte)tBBlueBar.Value);
            if (g.DpiX == 120)
                tp.Show(((TrackBar)sender).Value.ToString(), ((TrackBar)sender), 125, 0, 2000);
            else
                tp.Show(((TrackBar)sender).Value.ToString(), ((TrackBar)sender), 100, 0, 2000);
        }
        private void greenBar_ValueChanged(object sender, EventArgs e)
        {
            int value = ((TrackBar)sender).Value;
            int sat = bgc - (value < bgc ? value : bgc);
            int som = bgc + 11 * (int)(value * 0.0039215);
            ((TrackBar)sender).BackColor = Color.FromArgb(sat, som, sat);
            alphacolor = Math.Max(tBRedBar.Value, Math.Max(tBGreenBar.Value, tBBlueBar.Value));
            reg = Color.FromArgb(tBRedBar.Value, tBGreenBar.Value, tBBlueBar.Value);
            full = HuetoRGB(reg.GetHue(), reg.GetBrightness(), reg);
            pBController.BackColor = Color.FromArgb((alphacolor > 205 ? 255 : (alphacolor + 50)), full);
            Global.saveColor(device, (byte)tBRedBar.Value, (byte)tBGreenBar.Value, (byte)tBBlueBar.Value);
            if (g.DpiX == 120)
                tp.Show(((TrackBar)sender).Value.ToString(), ((TrackBar)sender), 125, 0, 2000);
            else
                tp.Show(((TrackBar)sender).Value.ToString(), ((TrackBar)sender), 100, 0, 2000);
        }
        private void blueBar_ValueChanged(object sender, EventArgs e)
        {
            int value = ((TrackBar)sender).Value;
            int sat = bgc - (value < bgc ? value : bgc);
            int som = bgc + 11 * (int)(value * 0.0039215);
            ((TrackBar)sender).BackColor = Color.FromArgb(sat, sat, som);
            alphacolor = Math.Max(tBRedBar.Value, Math.Max(tBGreenBar.Value, tBBlueBar.Value));
            reg = Color.FromArgb(tBRedBar.Value, tBGreenBar.Value, tBBlueBar.Value);
            full = HuetoRGB(reg.GetHue(), reg.GetBrightness(), reg);
            pBController.BackColor = Color.FromArgb((alphacolor > 205 ? 255 : (alphacolor + 50)), full);
            Global.saveColor(device, (byte)tBRedBar.Value, (byte)tBGreenBar.Value, (byte)tBBlueBar.Value);
            if (g.DpiX == 120)
                tp.Show(((TrackBar)sender).Value.ToString(), ((TrackBar)sender), 125, 0, 2000);
            else
                tp.Show(((TrackBar)sender).Value.ToString(), ((TrackBar)sender), 100, 0, 2000);
        }

        private void lowRedBar_ValueChanged(object sender, EventArgs e)
        {
            int value = ((TrackBar)sender).Value;
            int sat = bgc - (value < bgc ? value : bgc);
            int som = bgc + 11 * (int)(value * 0.0039215);
            ((TrackBar)sender).BackColor = Color.FromArgb(som, sat, sat);
            alphacolor = Math.Max(tBLowRedBar.Value, Math.Max(tBLowGreenBar.Value, tBLowBlueBar.Value));
            reg = Color.FromArgb(tBLowRedBar.Value, tBLowGreenBar.Value, tBLowBlueBar.Value);
            full = HuetoRGB(reg.GetHue(), reg.GetBrightness(), reg);
            lowColorChooserButton.BackColor = Color.FromArgb((alphacolor > 205 ? 255 : (alphacolor + 50)), full);
            Global.saveLowColor(device, (byte)tBLowRedBar.Value, (byte)tBLowGreenBar.Value, (byte)tBLowBlueBar.Value);
            if (g.DpiX == 120)
                tp.Show(((TrackBar)sender).Value.ToString(), ((TrackBar)sender), 125, 0, 2000);
            else
                tp.Show(((TrackBar)sender).Value.ToString(), ((TrackBar)sender), 100, 0, 2000);
        }

        private void lowGreenBar_ValueChanged(object sender, EventArgs e)
        {
            int value = ((TrackBar)sender).Value;
            int sat = bgc - (value < bgc ? value : bgc);
            int som = bgc + 11 * (int)(value * 0.0039215);
            ((TrackBar)sender).BackColor = Color.FromArgb(sat, som, sat);
            alphacolor = Math.Max(tBLowRedBar.Value, Math.Max(tBLowGreenBar.Value, tBLowBlueBar.Value));
            reg = Color.FromArgb(tBLowRedBar.Value, tBLowGreenBar.Value, tBLowBlueBar.Value);
            full = HuetoRGB(reg.GetHue(), reg.GetBrightness(), reg);
            lowColorChooserButton.BackColor = Color.FromArgb((alphacolor > 205 ? 255 : (alphacolor + 50)), full);
            Global.saveLowColor(device, (byte)tBLowRedBar.Value, (byte)tBLowGreenBar.Value, (byte)tBLowBlueBar.Value);
            if (g.DpiX == 120)
                tp.Show(((TrackBar)sender).Value.ToString(), ((TrackBar)sender), 125, 0, 2000);
            else
                tp.Show(((TrackBar)sender).Value.ToString(), ((TrackBar)sender), 100, 0, 2000);
        }

        private void lowBlueBar_ValueChanged(object sender, EventArgs e)
        {
            int value = ((TrackBar)sender).Value;
            int sat = bgc - (value < bgc ? value : bgc);
            int som = bgc + 11 * (int)(value * 0.0039215);
            ((TrackBar)sender).BackColor = Color.FromArgb(sat, sat, som);
            alphacolor = Math.Max(tBLowRedBar.Value, Math.Max(tBLowGreenBar.Value, tBLowBlueBar.Value));
            reg = Color.FromArgb(tBLowRedBar.Value, tBLowGreenBar.Value, tBLowBlueBar.Value);
            full = HuetoRGB(reg.GetHue(), reg.GetBrightness(), reg);
            lowColorChooserButton.BackColor = Color.FromArgb((alphacolor > 205 ? 255 : (alphacolor + 50)), full);
            Global.saveLowColor(device, (byte)tBLowRedBar.Value, (byte)tBLowGreenBar.Value, (byte)tBLowBlueBar.Value);
            if (g.DpiX == 120)
                tp.Show(((TrackBar)sender).Value.ToString(), ((TrackBar)sender), 125, 0, 2000);
            else
                tp.Show(((TrackBar)sender).Value.ToString(), ((TrackBar)sender), 100, 0, 2000);
        }

        private void shiftRedBar_ValueChanged(object sender, EventArgs e)
        {
            int value = ((TrackBar)sender).Value;
            int sat = bgc - (value < bgc ? value : bgc);
            int som = bgc + 11 * (int)(value * 0.0039215);
            ((TrackBar)sender).BackColor = Color.FromArgb(som, sat, sat);
            alphacolor = Math.Max(tBShiftRedBar.Value, Math.Max(tBShiftGreenBar.Value, tBShiftBlueBar.Value));
            reg = Color.FromArgb(tBShiftRedBar.Value, tBShiftGreenBar.Value, tBShiftBlueBar.Value);
            full = HuetoRGB(reg.GetHue(), reg.GetBrightness(), reg);
            spBController.BackColor = Color.FromArgb((alphacolor > 205 ? 255 : (alphacolor + 50)), full);
            Global.saveShiftColor(device, (byte)tBShiftRedBar.Value, (byte)tBShiftGreenBar.Value, (byte)tBShiftBlueBar.Value);
            if (g.DpiX == 120)
                tp.Show(((TrackBar)sender).Value.ToString(), ((TrackBar)sender), 125, 0, 2000);
            else
                tp.Show(((TrackBar)sender).Value.ToString(), ((TrackBar)sender), 100, 0, 2000);
        }

        private void shiftGreenBar_ValueChanged(object sender, EventArgs e)
        {
            int value = ((TrackBar)sender).Value;
            int sat = bgc - (value < bgc ? value : bgc);
            int som = bgc + 11 * (int)(value * 0.0039215);
            ((TrackBar)sender).BackColor = Color.FromArgb(sat, som, sat);
            alphacolor = Math.Max(tBShiftRedBar.Value, Math.Max(tBShiftGreenBar.Value, tBShiftBlueBar.Value));
            reg = Color.FromArgb(tBShiftRedBar.Value, tBShiftGreenBar.Value, tBShiftBlueBar.Value);
            full = HuetoRGB(reg.GetHue(), reg.GetBrightness(), reg);
            spBController.BackColor = Color.FromArgb((alphacolor > 205 ? 255 : (alphacolor + 50)), full);
            Global.saveShiftColor(device, (byte)tBShiftRedBar.Value, (byte)tBShiftGreenBar.Value, (byte)tBShiftBlueBar.Value);
            if (g.DpiX == 120)
                tp.Show(((TrackBar)sender).Value.ToString(), ((TrackBar)sender), 125, 0, 2000);
            else
                tp.Show(((TrackBar)sender).Value.ToString(), ((TrackBar)sender), 100, 0, 2000);
        }

        private void shiftBlueBar_ValueChanged(object sender, EventArgs e)
        {
            int value = ((TrackBar)sender).Value;
            int sat = bgc - (value < bgc ? value : bgc);
            int som = bgc + 11 * (int)(value * 0.0039215);
            ((TrackBar)sender).BackColor = Color.FromArgb(sat, sat, som);
            alphacolor = Math.Max(tBShiftRedBar.Value, Math.Max(tBShiftGreenBar.Value, tBShiftBlueBar.Value));
            reg = Color.FromArgb(tBShiftRedBar.Value, tBShiftGreenBar.Value, tBShiftBlueBar.Value);
            full = HuetoRGB(reg.GetHue(), reg.GetBrightness(), reg);
            spBController.BackColor = Color.FromArgb((alphacolor > 205 ? 255 : (alphacolor + 50)), full);
            Global.saveShiftColor(device, (byte)tBShiftRedBar.Value, (byte)tBShiftGreenBar.Value, (byte)tBShiftBlueBar.Value);
            if (g.DpiX == 120)
                tp.Show(((TrackBar)sender).Value.ToString(), ((TrackBar)sender), 125, 0, 2000);
            else
                tp.Show(((TrackBar)sender).Value.ToString(), ((TrackBar)sender), 100, 0, 2000);
        }

        public Color HuetoRGB(float hue, float light, Color rgb)
        {
            float L = (float)Math.Max(.5, light);
            float C = (1 - Math.Abs(2 * L - 1));
            float X = (C * (1 - Math.Abs((hue / 60) % 2 - 1)));
            float m = L - C / 2;
            float R =0, G=0, B=0;
            if (light == 1) return Color.FromName("White");
            else if (rgb.R == rgb.G && rgb.G == rgb.B) return Color.FromName("White");
            else if (0 <= hue && hue < 60)    { R = C; G = X; }
            else if (60 <= hue && hue < 120)  { R = X; G = C; }
            else if (120 <= hue && hue < 180) { G = C; B = X; }
            else if (180 <= hue && hue < 240) { G = X; B = C; }
            else if (240 <= hue && hue < 300) { R = X; B = C; }
            else if (300 <= hue && hue < 360) { R = C; B = X; }
            return Color.FromArgb((int)((R + m) * 255), (int)((G + m) * 255), (int)((B + m) * 255));
        }
        private void rumbleBoostBar_ValueChanged(object sender, EventArgs e)
        {
            Global.saveRumbleBoost(device, (byte)nUDRumbleBoost.Value);
            if (btnRumbleTest.Text == Properties.Resources.StopText)
                scpDevice.setRumble(255, 255, device);
        }

        private void btnRumbleTest_Click(object sender, EventArgs e)
        {
            if (((Button)sender).Text == Properties.Resources.TestText)
            {
                scpDevice.setRumble((byte)Math.Min(255, (255 * nUDRumbleBoost.Value / 100)), (byte)Math.Min(255, (255 * nUDRumbleBoost.Value / 100)), (int)nUDSixaxis.Value - 1);
                ((Button)sender).Text = Properties.Resources.StopText;
            }
            else
            {
                scpDevice.setRumble(0, 0, (int)nUDSixaxis.Value - 1);
                ((Button)sender).Text = Properties.Resources.TestText;
            }                
        }

        private void numUDTouch_ValueChanged(object sender, EventArgs e)
        {
            Global.setTouchSensitivity(device, (byte)nUDTouch.Value);
        }

        private void numUDTap_ValueChanged(object sender, EventArgs e)
        {
            Global.setTapSensitivity(device, (byte)nUDTap.Value);
        }

        private void numUDScroll_ValueChanged(object sender, EventArgs e)
        {
            Global.setScrollSensitivity(device, (int)nUDScroll.Value);
        }
        private void ledAsBatteryIndicator_CheckedChanged(object sender, EventArgs e)
        {
            Global.setLedAsBatteryIndicator(device, cBLightbyBattery.Checked);
            lowBatteryPanel.Visible = cBLightbyBattery.Checked;
            FullPanel.Location = (cBLightbyBattery.Checked ? new Point(FullPanel.Location.X, 42) : new Point(FullPanel.Location.X, 48));
            lbFull.Text = (cBLightbyBattery.Checked ? Properties.Resources.Full + ":" : Properties.Resources.Color + ":");
        }

        private void lowerRCOffCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            Global.setLowerRCOn(device, cBlowerRCOn.Checked);
        }

        private void touchpadJitterCompensation_CheckedChanged(object sender, EventArgs e)
        {
            Global.setTouchpadJitterCompensation(device, cBTouchpadJitterCompensation.Checked);
        }
        
        private void flushHIDQueue_CheckedChanged(object sender, EventArgs e)
        {
            Global.setFlushHIDQueue(device, cBFlushHIDQueue.Checked);
        }

        private void nUDIdleDisconnect_ValueChanged(object sender, EventArgs e)
        {
            Global.setIdleDisconnectTimeout(device, (int)(nUDIdleDisconnect.Value * 60));
            //if (nUDIdleDisconnect.Value == 0)
                //cBIdleDisconnect.Checked = false;
        }

        private void cBIdleDisconnect_CheckedChanged(object sender, EventArgs e)
        {
            if (cBIdleDisconnect.Checked)
                nUDIdleDisconnect.Value = 5;
            else
                nUDIdleDisconnect.Value = 0;
            nUDIdleDisconnect.Enabled = cBIdleDisconnect.Checked;
        }

        private void Options_Closed(object sender, FormClosedEventArgs e)
        {
            for (int i = 0; i < 4; i++)
                Global.LoadProfile(i, false, scpDevice); //Refreshes all profiles in case other controllers are using the same profile
            if (olddinputcheck != cBDinput.Checked)
            {
                root.btnStartStop_Clicked(false);
                root.btnStartStop_Clicked(false);
            }
            if (btnRumbleTest.Text == Properties.Resources.StopText)
                scpDevice.setRumble(0, 0, (int)nUDSixaxis.Value - 1);
            inputtimer.Stop();
            sixaxisTimer.Stop();
        }

        private void cBSlide_CheckedChanged(object sender, EventArgs e)
        {
            if (cBSlide.Checked)
                nUDTouch.Value = 100;
            else
                nUDTouch.Value = 0;
            nUDTouch.Enabled = cBSlide.Checked;
        }

        private void cBScroll_CheckedChanged(object sender, EventArgs e)
        {
            if (cBScroll.Checked)
                nUDScroll.Value = 5;
            else
                nUDScroll.Value = 0;
            nUDScroll.Enabled = cBScroll.Checked;
        }

        private void cBTap_CheckedChanged(object sender, EventArgs e)
        {
            if (cBTap.Checked)
                nUDTap.Value = 100;
            else
                nUDTap.Value = 0;
            nUDTap.Enabled = cBTap.Checked;
            cBDoubleTap.Enabled = cBTap.Checked;
        }

        private void cBDoubleTap_CheckedChanged(object sender, EventArgs e)
        {
            Global.setDoubleTap(device, cBDoubleTap.Checked);
        }

        public void UpdateLists()
        {
            lBControls.Items[0] = "Cross : " + bnCross.Text;
            lBControls.Items[1] = "Circle : " + bnCircle.Text;
            lBControls.Items[2] = "Square : " + bnSquare.Text;
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
            lBControls.Items[17] = "Left Touch : " + bnTouchLeft.Text;
            lBControls.Items[18] = "Right Touch : " + bnTouchRight.Text;
            lBControls.Items[19] = "Multitouch : " + bnTouchMulti.Text;
            lBControls.Items[20] = "Upper Touch : " + bnTouchUpper.Text;
            lBControls.Items[21] = "LS Up : " + bnLSUp.Text;
            lBControls.Items[22] = "LS Down : " + bnLSDown.Text;
            lBControls.Items[23] = "LS Left : " + bnLSLeft.Text;
            lBControls.Items[24] = "LS Right : " + bnLSRight.Text;
            lBControls.Items[25] = "RS Up : " + bnRSUp.Text;
            lBControls.Items[26] = "RS Down : " + bnRSDown.Text;
            lBControls.Items[27] = "RS Left : " + bnRSLeft.Text;
            lBControls.Items[28] = "RS Right : " + bnRSRight.Text;
            lBControls.Items[29] = Properties.Resources.TiltUp + " : " + UpdateGyroList(bnGyroZN);
            lBControls.Items[30] = Properties.Resources.TiltDown + " : " + UpdateGyroList(bnGyroZP);
            lBControls.Items[31] = Properties.Resources.TiltLeft + " : " + UpdateGyroList(bnGyroXP);
            lBControls.Items[32] = Properties.Resources.TiltRight + " : " + UpdateGyroList(bnGyroXN);
            bnGyroZN.Text = Properties.Resources.TiltUp;
            bnGyroZP.Text = Properties.Resources.TiltDown;
            bnGyroXP.Text = Properties.Resources.TiltLeft;
            bnGyroXN.Text = Properties.Resources.TiltRight;

            foreach (Button b in subbuttons)
                if (b.Tag == null)
                    b.Text = "Fall Back to " + buttons[subbuttons.IndexOf(b)].Text;
            lBShiftControls.Items[0] = "Cross : " + sbnCross.Text;
            lBShiftControls.Items[1] = "Circle : " + sbnCircle.Text;
            lBShiftControls.Items[2] = "Square : " + sbnSquare.Text;
            lBShiftControls.Items[3] = "Triangle : " + sbnTriangle.Text;
            lBShiftControls.Items[4] = "Options : " + sbnOptions.Text;
            lBShiftControls.Items[5] = "Share : " + sbnShare.Text;
            lBShiftControls.Items[6] = "Up : " + sbnUp.Text;
            lBShiftControls.Items[7] = "Down : " + sbnDown.Text;
            lBShiftControls.Items[8] = "Left : " + sbnLeft.Text;
            lBShiftControls.Items[9] = "Right : " + sbnRight.Text;
            lBShiftControls.Items[10] = "PS : " + sbnPS.Text;
            lBShiftControls.Items[11] = "L1 : " + sbnL1.Text;
            lBShiftControls.Items[12] = "R1 : " + sbnR1.Text;
            lBShiftControls.Items[13] = "L2 : " + sbnL2.Text;
            lBShiftControls.Items[14] = "R2 : " + sbnR2.Text;
            lBShiftControls.Items[15] = "L3 : " + sbnL3.Text;
            lBShiftControls.Items[16] = "R3 : " + sbnR3.Text;
            lBShiftControls.Items[17] = "Left Touch : " + sbnTouchLeft.Text;
            lBShiftControls.Items[18] = "Right Touch : " + sbnTouchRight.Text;
            lBShiftControls.Items[19] = "Multitouch : " + sbnTouchMulti.Text;
            lBShiftControls.Items[20] = "Upper Touch : " + sbnTouchUpper.Text;
            lBShiftControls.Items[21] = "LS Up : " + sbnLSUp.Text;
            lBShiftControls.Items[22] = "LS Down : " + sbnLSDown.Text;
            lBShiftControls.Items[23] = "LS Left : " + sbnLSLeft.Text;
            lBShiftControls.Items[24] = "LS Right : " + sbnLSRight.Text;
            lBShiftControls.Items[25] = "RS Up : " + sbnRSUp.Text;
            lBShiftControls.Items[26] = "RS Down : " + sbnRSDown.Text;
            lBShiftControls.Items[27] = "RS Left : " + sbnRSLeft.Text;
            lBShiftControls.Items[28] = "RS Right : " + sbnRSRight.Text;
            lBShiftControls.Items[29] = Properties.Resources.TiltUp + " : " + UpdateGyroList(sbnGyroZN);
            lBShiftControls.Items[30] = Properties.Resources.TiltDown + " : " + UpdateGyroList(sbnGyroZP);
            lBShiftControls.Items[31] = Properties.Resources.TiltLeft + " : " + UpdateGyroList(sbnGyroXP);
            lBShiftControls.Items[32] = Properties.Resources.TiltRight + " : " + UpdateGyroList(sbnGyroXN);
            sbnGyroZN.Text = Properties.Resources.TiltUp;
            sbnGyroZP.Text = Properties.Resources.TiltDown;
            sbnGyroXP.Text = Properties.Resources.TiltLeft;
            sbnGyroXN.Text = Properties.Resources.TiltRight;
        }

        private string UpdateGyroList(Button button)
        {
            if (button.Tag is String && (String)button.Tag == "Unbound")
                return "Unbound";
            else if (button.Tag is IEnumerable<int> || button.Tag is Int32[] || button.Tag is UInt16[])
                return "Macro";
            else if (button.Tag is Int32)
                return ((Keys)(Int32)button.Tag).ToString();
            else if (button.Tag is UInt16)
                return ((Keys)(UInt16)button.Tag).ToString();
            else if (button.Tag is string)
                return button.Tag.ToString();
            else if (button.Name.StartsWith("s") && buttons[subbuttons.IndexOf(button)].Tag != null && button.Tag == null)
                return "Fall Back to " + UpdateGyroList(buttons[subbuttons.IndexOf(button)]);
            else
                return string.Empty;
        }
        private void Show_ControlsList(object sender, EventArgs e)
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

            if (lBControls.SelectedIndex == 17) Show_ControlsBn(bnTouchLeft, e);
            if (lBControls.SelectedIndex == 18) Show_ControlsBn(bnTouchRight, e);
            if (lBControls.SelectedIndex == 19) Show_ControlsBn(bnTouchMulti, e);
            if (lBControls.SelectedIndex == 20) Show_ControlsBn(bnTouchUpper, e);

            if (lBControls.SelectedIndex == 21) Show_ControlsBn(bnLSUp, e);
            if (lBControls.SelectedIndex == 22) Show_ControlsBn(bnLSDown, e);
            if (lBControls.SelectedIndex == 23) Show_ControlsBn(bnLSLeft, e);
            if (lBControls.SelectedIndex == 24) Show_ControlsBn(bnLSRight, e);
            if (lBControls.SelectedIndex == 25) Show_ControlsBn(bnRSUp, e);
            if (lBControls.SelectedIndex == 26) Show_ControlsBn(bnRSDown, e);
            if (lBControls.SelectedIndex == 27) Show_ControlsBn(bnRSLeft, e);
            if (lBControls.SelectedIndex == 28) Show_ControlsBn(bnRSRight, e);

            if (lBControls.SelectedIndex == 29) Show_ControlsBn(bnGyroZN, e);
            if (lBControls.SelectedIndex == 30) Show_ControlsBn(bnGyroZP, e);
            if (lBControls.SelectedIndex == 31) Show_ControlsBn(bnGyroXP, e);
            if (lBControls.SelectedIndex == 32) Show_ControlsBn(bnGyroXN, e);
        }

        private void Show_ShiftControlsList(object sender, EventArgs e)
        {
            if (lBShiftControls.SelectedIndex == 0) Show_ControlsBn(sbnCross, e);
            if (lBShiftControls.SelectedIndex == 1) Show_ControlsBn(sbnCircle, e);
            if (lBShiftControls.SelectedIndex == 2) Show_ControlsBn(sbnSquare, e);
            if (lBShiftControls.SelectedIndex == 3) Show_ControlsBn(sbnTriangle, e);
            if (lBShiftControls.SelectedIndex == 4) Show_ControlsBn(sbnOptions, e);
            if (lBShiftControls.SelectedIndex == 5) Show_ControlsBn(sbnShare, e);
            if (lBShiftControls.SelectedIndex == 6) Show_ControlsBn(sbnUp, e);
            if (lBShiftControls.SelectedIndex == 7) Show_ControlsBn(sbnDown, e);
            if (lBShiftControls.SelectedIndex == 8) Show_ControlsBn(sbnLeft, e);
            if (lBShiftControls.SelectedIndex == 9) Show_ControlsBn(sbnRight, e);
            if (lBShiftControls.SelectedIndex == 10) Show_ControlsBn(sbnPS, e);
            if (lBShiftControls.SelectedIndex == 11) Show_ControlsBn(sbnL1, e);
            if (lBShiftControls.SelectedIndex == 12) Show_ControlsBn(sbnR1, e);
            if (lBShiftControls.SelectedIndex == 13) Show_ControlsBn(sbnL2, e);
            if (lBShiftControls.SelectedIndex == 14) Show_ControlsBn(sbnR2, e);
            if (lBShiftControls.SelectedIndex == 15) Show_ControlsBn(sbnL3, e);
            if (lBShiftControls.SelectedIndex == 16) Show_ControlsBn(sbnR3, e);

            if (lBShiftControls.SelectedIndex == 17) Show_ControlsBn(sbnTouchLeft, e);
            if (lBShiftControls.SelectedIndex == 18) Show_ControlsBn(sbnTouchRight, e);
            if (lBShiftControls.SelectedIndex == 19) Show_ControlsBn(sbnTouchMulti, e);
            if (lBShiftControls.SelectedIndex == 20) Show_ControlsBn(sbnTouchUpper, e);

            if (lBShiftControls.SelectedIndex == 21) Show_ControlsBn(sbnLSUp, e);
            if (lBShiftControls.SelectedIndex == 22) Show_ControlsBn(sbnLSDown, e);
            if (lBShiftControls.SelectedIndex == 23) Show_ControlsBn(sbnLSLeft, e);
            if (lBShiftControls.SelectedIndex == 24) Show_ControlsBn(sbnLSRight, e);
            if (lBShiftControls.SelectedIndex == 25) Show_ControlsBn(sbnRSUp, e);
            if (lBShiftControls.SelectedIndex == 26) Show_ControlsBn(sbnRSDown, e);
            if (lBShiftControls.SelectedIndex == 27) Show_ControlsBn(sbnRSLeft, e);
            if (lBShiftControls.SelectedIndex == 28) Show_ControlsBn(sbnRSRight, e);

            if (lBShiftControls.SelectedIndex == 29) Show_ControlsBn(sbnGyroZN, e);
            if (lBShiftControls.SelectedIndex == 30) Show_ControlsBn(sbnGyroZP, e);
            if (lBShiftControls.SelectedIndex == 31) Show_ControlsBn(sbnGyroXP, e);
            if (lBShiftControls.SelectedIndex == 32) Show_ControlsBn(sbnGyroXN, e);
        }

        private void List_MouseDoubleClick(object sender, MouseEventArgs e)
        {
           if (((ListBox)sender).Name.Contains("Shift"))
               Show_ShiftControlsList(sender, e);
           else
            Show_ControlsList(sender, e);
        }

        private void List_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyValue == 13)
                if (((ListBox)sender).Name.Contains("Shift"))
                    Show_ShiftControlsList(sender, e);
                else
                    Show_ControlsList(sender, e);
        }

        private void numUDRainbow_ValueChanged(object sender, EventArgs e)
        {
            Global.setRainbow(device, (double)nUDRainbow.Value);
            if ((double)nUDRainbow.Value <= 0.5)
            {
                pBRainbow.Image = greyscale;
                ToggleRainbow(false);
                nUDRainbow.Value = 0;
            }
        }

        private void pbRainbow_Click(object sender, EventArgs e)
        {
            if (pBRainbow.Image == greyscale)
            {
                pBRainbow.Image = colored;
                ToggleRainbow(true);
                nUDRainbow.Value = 5;
            }
            else
            {
                pBRainbow.Image = greyscale;
                ToggleRainbow(false);
                nUDRainbow.Value = 0;
            }
        }

        private void ToggleRainbow(bool on)
        {
            nUDRainbow.Enabled = on;
            if (on)
            {
                //pBRainbow.Location = new Point(216 - 78, pBRainbow.Location.Y);
                pBController.BackgroundImage = Properties.Resources.rainbowC;
                cBLightbyBattery.Text = Properties.Resources.DimByBattery.Replace("*nl*", "\n");
            }
            else
            {
                lowBatteryPanel.Enabled = cBLightbyBattery.Checked;
                //pBRainbow.Location = new Point(216, pBRainbow.Location.Y);
                pBController.BackgroundImage = null;
                cBLightbyBattery.Text = Properties.Resources.ColorByBattery.Replace("*nl*", "\n");
            }
            lbspc.Enabled = on;
            lowBatteryPanel.Enabled = !on;
            FullPanel.Enabled = !on;
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

        private void numUDL2_ValueChanged(object sender, EventArgs e)
        {
            Global.setLeftTriggerMiddle(device, (byte)(nUDL2.Value * 255));
        }

        private void numUDR2_ValueChanged(object sender, EventArgs e)
        {
            Global.setRightTriggerMiddle(device, (byte)(nUDR2.Value * 255));
        }

        private void nUDSX_ValueChanged(object sender, EventArgs e)
        {
            Global.setSXDeadzone(device, (double)nUDSX.Value);
            if (nUDSX.Value <= 0 && nUDSZ.Value <= 0)
                pBSADeadzone.Visible = false;
            else
            {
                pBSADeadzone.Visible = true;
                pBSADeadzone.Size = new Size((int)(nUDSX.Value * 125), (int)(nUDSZ.Value * 125));
                pBSADeadzone.Location = new Point(lbSATrack.Location.X + 63 - pBSADeadzone.Size.Width / 2, lbSATrack.Location.Y + 63 - pBSADeadzone.Size.Height / 2);
            }
        }

        private void nUDSZ_ValueChanged(object sender, EventArgs e)
        {
            Global.setSZDeadzone(device, (double)nUDSZ.Value);
            if (nUDSX.Value <= 0 && nUDSZ.Value <= 0)
                pBSADeadzone.Visible = false;
            else
            {
                pBSADeadzone.Visible = true;
                pBSADeadzone.Size = new Size((int)(nUDSX.Value * 125), (int)(nUDSZ.Value * 125));
                pBSADeadzone.Location = new Point(lbSATrack.Location.X + 63 - pBSADeadzone.Size.Width / 2, lbSATrack.Location.Y + 63 - pBSADeadzone.Size.Height / 2);
            }
        }

        Image L = Properties.Resources.LeftTouch;
        Image R = Properties.Resources.RightTouch;
        Image M = Properties.Resources.MultiTouch;
        Image U = Properties.Resources.UpperTouch;
        private void bnTouchLeft_MouseHover(object sender, EventArgs e)
        {
            pBController.Image = L;       
        }

        private void bnTouchMulti_MouseHover(object sender, EventArgs e)
        {
            pBController.Image = M;
        }

        private void bnTouchRight_MouseHover(object sender, EventArgs e)
        {
            pBController.Image = R;
        }

        private void bnTouchUpper_MouseHover(object sender, EventArgs e)
        {
            pBController.Image = U;
        }

        private void Toucpad_Leave(object sender, EventArgs e)
        {
            pBController.Image = Properties.Resources.DS4_Controller;
        }

        private void numUDRS_ValueChanged(object sender, EventArgs e)
        {
            Global.setRSDeadzone(device, (byte)Math.Round((nUDRS.Value * 127),0));
            if (nUDRS.Value <= 0)
                pBRSDeadzone.Visible = false;
            else
            {
                pBRSDeadzone.Visible = true;
                pBRSDeadzone.Size = new Size((int)(nUDRS.Value * 125), (int)(nUDRS.Value * 125));
                pBRSDeadzone.Location = new Point(lbRSTrack.Location.X + 63 - pBRSDeadzone.Size.Width / 2, lbRSTrack.Location.Y + 63 - pBRSDeadzone.Size.Width / 2);
            }
        }

        private void numUDLS_ValueChanged(object sender, EventArgs e)
        {
            Global.setLSDeadzone(device, (byte)Math.Round((nUDLS.Value * 127),0));
            if (nUDLS.Value <= 0)
                pBLSDeadzone.Visible = false;
            else
            {
                pBLSDeadzone.Visible = true;
                pBLSDeadzone.Size = new Size((int)(nUDLS.Value*125), (int)(nUDLS.Value*125));
                pBLSDeadzone.Location = new Point(lbLSTrack.Location.X + 63 - pBLSDeadzone.Size.Width / 2, lbLSTrack.Location.Y + 63 - pBLSDeadzone.Size.Width / 2);
            }
        }

        private void numUDMouseSens_ValueChanged(object sender, EventArgs e)
        {
            Global.setButtonMouseSensitivity(device, (int)numUDMouseSens.Value);
        }

        private void LightBar_MouseDown(object sender, MouseEventArgs e)
        {
            if (g.DpiX == 120)
                tp.Show(((TrackBar)sender).Value.ToString(), ((TrackBar)sender), 125, 0, 2000);
            else
                tp.Show(((TrackBar)sender).Value.ToString(), ((TrackBar)sender), 100, 0, 2000);
        }

        private void Lightbar_MouseUp(object sender, MouseEventArgs e)
        {
            tp.Hide(((TrackBar)sender));
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void nUDflashLED_ValueChanged(object sender, EventArgs e)
        {
            if (nUDflashLED.Value % 10 != 0)
                nUDflashLED.Value = Math.Round(nUDflashLED.Value / 10, 0) * 10;
            Global.setFlashAt(device, (int)nUDflashLED.Value);
        }

        private void rBNormal_CheckedChanged(object sender, EventArgs e)
        {
            Global.setChargingType(device, 0);
            btnChargingColor.Visible = false;
        }

        private void rBFade_CheckedChanged(object sender, EventArgs e)
        {
            Global.setChargingType(device, 1);
            btnChargingColor.Visible = false;
        }

        private void rBRainbow_CheckedChanged(object sender, EventArgs e)
        {
            Global.setChargingType(device, 2);
            btnChargingColor.Visible = false;
        }

        private void rBColor_CheckedChanged(object sender, EventArgs e)
        {
            Global.setChargingType(device, 3);
            btnChargingColor.Visible = true;
        }

        private void cBMouseAccel_CheckedChanged(object sender, EventArgs e)
        {
            Global.setMouseAccel(device, cBMouseAccel.Checked);
        }

        private void cBShiftControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            Global.setShiftModifier(device, cBShiftControl.SelectedIndex);
            if (cBShiftControl.SelectedIndex < 1)
                cBShiftLight.Checked = false;
        }

        private void tabControls_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabControls.SelectedIndex == 2)
                sixaxisTimer.Start();
            else
                sixaxisTimer.Stop();
            if (tabControls.SelectedIndex == 1)
                ShiftPanel.Visible = true;
            else
                ShiftPanel.Visible = false;
        }

        private void DrawCircle(object sender, PaintEventArgs e)
        {
            // Create pen.
            Pen blackPen = new Pen(Color.Red);

            // Create rectangle for ellipse.
            Rectangle rect = new Rectangle(0, 0, ((PictureBox)sender).Size.Width, ((PictureBox)sender).Size.Height);

            // Draw ellipse to screen.
            e.Graphics.DrawEllipse(blackPen, rect);
        }

        private void lbEmpty_Click(object sender, EventArgs e)
        {
            tBLowRedBar.Value = tBRedBar.Value;
            tBLowGreenBar.Value = tBGreenBar.Value;
            tBLowBlueBar.Value = tBBlueBar.Value;
        }

        private void lbShift_Click(object sender, EventArgs e)
        {
            tBShiftRedBar.Value = tBRedBar.Value;
            tBShiftGreenBar.Value = tBGreenBar.Value;
            tBShiftBlueBar.Value = tBBlueBar.Value;
        }

        private void lbSATip_Click(object sender, EventArgs e)
        {
            SixaxisPanel.Visible = !SixaxisPanel.Visible;
            pBSADeadzone.Visible = !pBSADeadzone.Visible;
            btnSATrack.Visible = !btnSATrack.Visible;
        }

        private void SixaxisPanel_Click(object sender, EventArgs e)
        {
            lbSATip_Click(sender, e);
        }

        private void lbSATrack_Click(object sender, EventArgs e)
        {
            lbSATip_Click(sender, e);
        }

        private void cBShiftLight_CheckedChanged(object sender, EventArgs e)
        {
            if (Global.getShiftModifier(device) < 1)
                cBShiftLight.Checked = false;
            if (!cBShiftLight.Checked)
            {
                spBController.BackColor = pBController.BackColor;
                spBController.BackgroundImage = pBController.BackgroundImage;
            }
            else
            {
                alphacolor = Math.Max(tBShiftRedBar.Value, Math.Max(tBShiftGreenBar.Value, tBShiftBlueBar.Value));
                reg = Color.FromArgb(tBShiftRedBar.Value, tBShiftGreenBar.Value, tBShiftBlueBar.Value);
                full = HuetoRGB(reg.GetHue(), reg.GetBrightness(), reg);
                spBController.BackColor = Color.FromArgb((alphacolor > 205 ? 255 : (alphacolor + 50)), full);
            }
            Global.setShiftColorOn(device, cBShiftLight.Checked);
            lbShift.Enabled = cBShiftLight.Checked;
            lbShiftRed.Enabled = cBShiftLight.Checked;
            lbShiftGreen.Enabled = cBShiftLight.Checked;
            lbShiftBlue.Enabled = cBShiftLight.Checked;
            tBShiftRedBar.Enabled = cBShiftLight.Checked;
            tBShiftGreenBar.Enabled = cBShiftLight.Checked;
            tBShiftBlueBar.Enabled = cBShiftLight.Checked;
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            if( openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                cBLaunchProgram.Checked = true;
                Global.setLaunchProgram(device, openFileDialog1.FileName);
                pBProgram.Image = Icon.ExtractAssociatedIcon(openFileDialog1.FileName).ToBitmap();
                btnBrowse.Text = Path.GetFileNameWithoutExtension(openFileDialog1.FileName);
            }
        }

        private void cBLaunchProgram_CheckedChanged(object sender, EventArgs e)
        {
            if (!cBLaunchProgram.Checked)
            {
                Global.setLaunchProgram(device, string.Empty);
                pBProgram.Image = null;
                btnBrowse.Text = Properties.Resources.Browse;
            }
        }

        private void cBDinput_CheckedChanged(object sender, EventArgs e)
        {
            Global.setDinputOnly(device, cBDinput.Checked);
            if (device > 4)
            {
                root.btnStartStop_Clicked(false);
                root.btnStartStop_Clicked(false);
            }
        }

        private void lbFlashAt_Click(object sender, EventArgs e)
        {
            advColorDialog.Color = lbFlashAt.ForeColor;
            advColorDialog.Color = lbPercentFlashBar.ForeColor;
            advColorDialog_OnUpdateColor(lbPercentFlashBar.ForeColor, e);
            if (advColorDialog.ShowDialog() == DialogResult.OK)
            {
                lbFlashAt.ForeColor = advColorDialog.Color; 
                lbPercentFlashBar.ForeColor = advColorDialog.Color;
                Global.saveFlashColor(device, advColorDialog.Color.R, advColorDialog.Color.G, advColorDialog.Color.B);
            }
            //else Global.saveChargingColor(device, oldChargingColor[0], oldChargingColor[1], oldChargingColor[2]);
            //Global.saveLowColor(device, oldLowLedColor[0], oldLowLedColor[1], oldLowLedColor[2]);
            Global.saveColor(device, oldLedColor[0], oldLedColor[1], oldLedColor[2]);
            oldLedColor = null;
            if (lbFlashAt.ForeColor.GetBrightness() > .5f)
            {
                lbFlashAt.BackColor = Color.Black;
                lbPercentFlashBar.BackColor = Color.Black;
            }
            else
            {
                lbFlashAt.BackColor = Color.White;
                lbPercentFlashBar.BackColor = Color.White;
            }
            //oldChargingColor = null;
            //oldLowLedColor = null;
        }

        private void cbStartTouchpadOff_CheckedChanged(object sender, EventArgs e)
        {
            Global.setStartTouchpadOff(device, cbStartTouchpadOff.Checked);
        }

        private void cBDinput_MouseHover(object sender, EventArgs e)
        {
            root.lbLastMessage.Text = Properties.Resources.DinputOnly;
        }

        private void Items_MouseHover(object sender, EventArgs e)
        {
            switch (((System.Windows.Forms.Control)sender).Name)
            {
                case "cBlowerRCOn": root.lbLastMessage.Text = Properties.Resources.BestUsedRightSide; break;
                case "cBDoubleTap": root.lbLastMessage.Text = Properties.Resources.TapAndHold; break;
                case "lbControlTip": root.lbLastMessage.Text = Properties.Resources.UseControllerForMapping; break;
                case "cBTouchpadJitterCompensation": root.lbLastMessage.Text = "Use Sixaxis to help calculate touchpad movement"; break;
                case "pBRainbow": root.lbLastMessage.Text = Properties.Resources.AlwaysRainbow; break;
                case "cBFlushHIDQueue": root.lbLastMessage.Text = "Flush HID Queue after each reading"; break;
                case "cBLightbyBattery": root.lbLastMessage.Text = "Also dim light by idle timeout if on"; break;
                case "lbGryo": root.lbLastMessage.Text = "Click to see readout of Sixaxis Gyro"; break;
                case "tBsixaxisGyroX": root.lbLastMessage.Text = "GyroX, Left and Right Tilt"; break;
                case "tBsixaxisGyroY": root.lbLastMessage.Text = "GyroY, Forward and Back Tilt"; break;
                case "tBsixaxisGyroZ": root.lbLastMessage.Text = "GyroZ, Up and Down Tilt"; break;
                case "tBsixaxisAccelX": root.lbLastMessage.Text = "AccelX"; break;
                case "tBsixaxisAccelY": root.lbLastMessage.Text = "AccelY"; break;
                case "tBsixaxisAccelZ": root.lbLastMessage.Text = "AccelZ"; break;
                case "lbEmpty": root.lbLastMessage.Text = Properties.Resources.CopyFullColor; break;
                case "lbShift": root.lbLastMessage.Text = Properties.Resources.CopyFullColor; break;
                case "lbSATip": root.lbLastMessage.Text = "Click for advanced Sixaxis reading"; break;
                case "cBDinput": root.lbLastMessage.Text = Properties.Resources.DinputOnly; break;
                case "lbFlashAt": root.lbLastMessage.Text = "Click to change flash color. Black = default color"; break;
                case "cbStartTouchpadOff": root.lbLastMessage.Text = "Re-enable by pressing PS+Touchpad"; break;
                default: root.lbLastMessage.Text = "Hover over items to see description or more about"; break;
            }
            if (root.lbLastMessage.Text != "Hover over items to see description or more about")
                root.lbLastMessage.ForeColor = Color.Black;
            else
                root.lbLastMessage.ForeColor = SystemColors.GrayText;
        }
    }
}
