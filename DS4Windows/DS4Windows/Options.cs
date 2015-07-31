using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml;
using static DS4Windows.Global;

namespace DS4Windows
{
    public partial class Options : Form
    {
        public int device;
        public string filename;
        public Timer inputtimer = new Timer(), sixaxisTimer = new Timer();
        public List<Button> buttons = new List<Button>(), subbuttons = new List<Button>();
        private Button lastSelected;
        private int alphacolor;
        private Color reg, full;
        private Image colored, greyscale;
        ToolTip tp = new ToolTip();
        public DS4Form root;
        bool olddinputcheck = false;
        Image L = Properties.Resources.LeftTouch;
        Image R = Properties.Resources.RightTouch;
        Image M = Properties.Resources.MultiTouch;
        Image U = Properties.Resources.UpperTouch;
        private float dpix;
        private float dpiy;
        public Dictionary<string, string> defaults = new Dictionary<string, string>();
        public bool saving;
        public Options(int deviceNum, string name, DS4Form rt)
        {
            InitializeComponent();
            btnRumbleHeavyTest.Text = Properties.Resources.TestHText;
            btnRumbleLightTest.Text = Properties.Resources.TestLText;
            device = deviceNum;
            filename = name;
            colored = pBRainbow.Image;
            root = rt;
            Graphics g = this.CreateGraphics();
            try
            {
                dpix = g.DpiX / 100f * 1.041666666667f;
                dpiy = g.DpiY / 100f * 1.041666666667f;
            }
            finally
            {
                g.Dispose();
            }

            greyscale = GreyscaleImage((Bitmap)pBRainbow.Image);
            foreach (System.Windows.Forms.Control control in pnlMain.Controls)
                if (control is Button && !((Button)control).Name.Contains("btn"))
                        buttons.Add((Button)control);
            foreach (System.Windows.Forms.Control control in pnlSticks.Controls)
                if (control is Button && !((Button)control).Name.Contains("btn"))
                        buttons.Add((Button)control);
            foreach (Button b in buttons)
                defaults.Add(b.Name, b.Text);
            foreach (System.Windows.Forms.Control control in fLPTiltControls.Controls)
                if (control is Button && !((Button)control).Name.Contains("btn"))
                        buttons.Add((Button)control);
            foreach (System.Windows.Forms.Control control in fLPTouchSwipe.Controls)
                if (control is Button && !((Button)control).Name.Contains("btn"))
                    buttons.Add((Button)control);
            foreach (System.Windows.Forms.Control control in pnlShiftMain.Controls)
                if (control is Button && !((Button)control).Name.Contains("btnShift"))
                        subbuttons.Add((Button)control);
            foreach (System.Windows.Forms.Control control in pnlShiftSticks.Controls)
                if (control is Button && !((Button)control).Name.Contains("btnShift"))
                        subbuttons.Add((Button)control);
            foreach (System.Windows.Forms.Control control in fLPShiftTiltControls.Controls)
                if (control is Button && !((Button)control).Name.Contains("btnShift"))
                    subbuttons.Add((Button)control);
            foreach (System.Windows.Forms.Control control in fLPShiftTouchSwipe.Controls)
                if (control is Button && !((Button)control).Name.Contains("btn"))
                    subbuttons.Add((Button)control);
            //string butts = "";
                //butts += "\n" + b.Name;
            //MessageBox.Show(butts);

            root.lbLastMessage.ForeColor = Color.Black;
            root.lbLastMessage.Text = "Hover over items to see description or more about";
            foreach (System.Windows.Forms.Control control in Controls)
            {
                if (control.HasChildren)
                    foreach (System.Windows.Forms.Control ctrl in control.Controls)
                    {
                        if (ctrl.HasChildren)
                            foreach (System.Windows.Forms.Control ctrl2 in ctrl.Controls)
                            {
                                if (ctrl2.HasChildren)
                                    foreach (System.Windows.Forms.Control ctrl3 in ctrl2.Controls)
                                        ctrl3.MouseHover += Items_MouseHover;
                                ctrl2.MouseHover += Items_MouseHover;
                            }
                        ctrl.MouseHover += Items_MouseHover;
                    }
                control.MouseHover += Items_MouseHover;
            }

            if (device < 4)
            nUDSixaxis.Value = deviceNum + 1;
            if (filename != "")
            {
                if (device == 4) //if temp device is called
                    ProfilePath[4] = name;
                LoadProfile(device, buttons.ToArray(), subbuttons.ToArray(), false, Program.rootHub);
                DS4Color color = MainColor[device];
                tBRedBar.Value = color.red;
                tBGreenBar.Value = color.green;
                tBBlueBar.Value = color.blue;

                cBLightbyBattery.Checked = LedAsBatteryIndicator[device];
                nUDflashLED.Value = FlashAt[device];
                pnlLowBattery.Visible = cBLightbyBattery.Checked;
                lbFull.Text = (cBLightbyBattery.Checked ? "Full:" : "Color:");
                pnlFull.Location = new Point(pnlFull.Location.X, (cBLightbyBattery.Checked ? (int)(dpix * 42) : (pnlFull.Location.Y + pnlLowBattery.Location.Y) / 2));
                DS4Color lowColor = LowColor[device];
                tBLowRedBar.Value = lowColor.red;
                tBLowGreenBar.Value = lowColor.green;
                tBLowBlueBar.Value = lowColor.blue;

                DS4Color shiftColor = ShiftColor[device];
                tBShiftRedBar.Value = shiftColor.red;
                tBShiftGreenBar.Value = shiftColor.green;
                tBShiftBlueBar.Value = shiftColor.blue;
                cBShiftLight.Checked = ShiftColorOn[device];

                DS4Color cColor = ChargingColor[device];
                btnChargingColor.BackColor = Color.FromArgb(cColor.red, cColor.green, cColor.blue);
                if (FlashType[device] > cBFlashType.Items.Count - 1)
                    cBFlashType.SelectedIndex = 0;
                else
                    cBFlashType.SelectedIndex = FlashType[device];
                DS4Color fColor = FlashColor[device];
                if (fColor.Equals(new DS4Color { red = 0, green = 0, blue = 0 }))
                    if (Rainbow[device] == 0)
                        btnFlashColor.BackColor = pBController.BackColor;
                    else
                        btnFlashColor.BackgroundImage = pBController.BackgroundImage;
                else
                    btnFlashColor.BackColor = Color.FromArgb(fColor.red, fColor.green, fColor.blue);
                nUDRumbleBoost.Value = RumbleBoost[device];
                nUDTouch.Value = TouchSensitivity[device];
                cBSlide.Checked = TouchSensitivity[device] > 0;
                nUDScroll.Value = ScrollSensitivity[device];
                cBScroll.Checked = ScrollSensitivity[device] > 0;
                nUDTap.Value = TapSensitivity[device];
                cBTap.Checked = TapSensitivity[device] > 0;
                cBDoubleTap.Checked = DoubleTap[device];
                nUDL2.Value = Math.Round((decimal)L2Deadzone[device] / 255, 2);
                nUDR2.Value = Math.Round((decimal)R2Deadzone[device] / 255, 2);
                cBTouchpadJitterCompensation.Checked = TouchpadJitterCompensation[device];
                cBlowerRCOn.Checked = LowerRCOn[device];
                cBFlushHIDQueue.Checked = FlushHIDQueue[device];
                nUDIdleDisconnect.Value = Math.Round((decimal)(IdleDisconnectTimeout[device] / 60d), 1);
                cBIdleDisconnect.Checked = IdleDisconnectTimeout[device] > 0;
                numUDMouseSens.Value = ButtonMouseSensitivity[device];
                cBMouseAccel.Checked = MouseAccel[device];
                // Force update of color choosers    
                alphacolor = Math.Max(tBRedBar.Value, Math.Max(tBGreenBar.Value, tBBlueBar.Value));
                reg = Color.FromArgb(color.red, color.green, color.blue);
                full = HuetoRGB(reg.GetHue(), reg.GetBrightness(), reg);
                pBController.BackColor = Color.FromArgb((alphacolor > 205 ? 255 : (alphacolor + 50)), full);

                alphacolor = Math.Max(tBLowRedBar.Value, Math.Max(tBGreenBar.Value, tBBlueBar.Value));
                reg = Color.FromArgb(lowColor.red, lowColor.green, lowColor.blue);
                full = HuetoRGB(reg.GetHue(), reg.GetBrightness(), reg);
                lowColorChooserButton.BackColor = Color.FromArgb((alphacolor > 205 ? 255 : (alphacolor + 50)), full);
                nUDRainbow.Value = (decimal)Rainbow[device];
                if (ChargingType[device] > cBWhileCharging.Items.Count - 1)
                    cBWhileCharging.SelectedIndex = 0; 
                else
                    cBWhileCharging.SelectedIndex = ChargingType[device]; 
                if (Rainbow[device] == 0)
                {
                    pBRainbow.Image = greyscale;
                    ToggleRainbow(false);
                }
                else
                {
                    pBRainbow.Image = colored;
                    ToggleRainbow(true);
                }
                nUDLS.Value = Math.Round((decimal)(LSDeadzone[device] / 127d), 3);
                nUDRS.Value = Math.Round((decimal)(RSDeadzone[device] / 127d), 3);
                nUDSX.Value = (decimal)SXDeadzone[device];
                nUDSZ.Value = (decimal)SZDeadzone[device];
                cBShiftControl.SelectedIndex = ShiftModifier[device];
                if (LaunchProgram[device] != string.Empty)
                {
                    cBLaunchProgram.Checked = true;
                    pBProgram.Image = Icon.ExtractAssociatedIcon(LaunchProgram[device]).ToBitmap();
                    btnBrowse.Text = Path.GetFileNameWithoutExtension(LaunchProgram[device]);
                }
                cBDinput.Checked = DinputOnly[device];
                olddinputcheck = cBDinput.Checked;
                cbStartTouchpadOff.Checked = StartTouchpadOff[device];
                cBTPforControls.Checked = UseTPforControls[device];
                nUDLSCurve.Value = LSCurve[device];
                nUDRSCurve.Value = RSCurve[device];
                cBControllerInput.Checked = DS4Mapping;
            }
            else
            {
                cBFlashType.SelectedIndex = 0;
                cBWhileCharging.SelectedIndex = 0;
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
            btnShiftLeftStick.Enter += btnShiftSticks_Enter;
            btnShiftRightStick.Enter += btnShiftSticks_Enter;
            UpdateLists();
            inputtimer.Start();
            inputtimer.Tick += InputDS4;
            sixaxisTimer.Tick += ControllerReadout_Tick;
            sixaxisTimer.Interval = 1000 / 60;
            LoadActions(string.IsNullOrEmpty(filename));
        }

        public void LoadActions(bool newp)
        {
            List<string> pactions = ProfileActions[device];
            foreach (SpecialAction action in GetActions())
            {
                ListViewItem lvi = new ListViewItem(action.name);
                lvi.SubItems.Add(action.controls.Replace("/", ", "));
                //string type = action.type;
                switch (action.type)
                {
                    case "Macro": lvi.SubItems.Add(Properties.Resources.Macro + (action.keyType.HasFlag(DS4KeyType.ScanCode) ? " (" + Properties.Resources.ScanCode + ")" : "")); break;
                    case "Program": lvi.SubItems.Add(Properties.Resources.LaunchProgram.Replace("*program*", Path.GetFileNameWithoutExtension(action.details))); break;
                    case "Profile": lvi.SubItems.Add(Properties.Resources.LoadProfile.Replace("*profile*", action.details)); break;
                    case "Key": lvi.SubItems.Add(((Keys)int.Parse(action.details)).ToString() + (action.uTrigger.Count > 0 ? " (Toggle)" : "")); break;
                    case "DisconnectBT":
                        lvi.SubItems.Add(Properties.Resources.DisconnectBT);
                        break;
                    case "BatteryCheck":
                        lvi.SubItems.Add(Properties.Resources.CheckBattery);
                        break;
                    case "XboxGameDVR":
                        lvi.SubItems.Add("Xbox Game DVR");
                        break;
                }
                if (newp && action.type == "DisconnectBT")
                    lvi.Checked = true;
                foreach (string s in pactions)
                    if (s == action.name)
                    {
                        lvi.Checked = true;
                        break;
                    }
                lVActions.Items.Add(lvi);
            }
        }

        public double Clamp(double min, double value, double max)
        {
            if (value > max)
                return max;
            else if (value < min)
                return min;
            else
                return value;
        }
        void ControllerReadout_Tick(object sender, EventArgs e)
        {          
            // MEMS gyro data is all calibrated to roughly -1G..1G for values -0x2000..0x1fff
            // Enough additional acceleration and we are no longer mostly measuring Earth's gravity...
            // We should try to indicate setpoints of the calibration when exposing this measurement....
            if (Program.rootHub.DS4Controllers[(int)nUDSixaxis.Value - 1] == null)
            {
                tPController.Enabled = false;
                lbInputDelay.Text = Properties.Resources.InputDelay.Replace("*number*", Properties.Resources.NA);
                pBDelayTracker.BackColor = Color.Transparent;
            }
            else
            {
                tPController.Enabled = true;
                SetDynamicTrackBarValue(tBsixaxisGyroX, (Program.rootHub.ExposedState[(int)nUDSixaxis.Value - 1].GyroX + tBsixaxisGyroX.Value * 2) / 3);
                SetDynamicTrackBarValue(tBsixaxisGyroY, (Program.rootHub.ExposedState[(int)nUDSixaxis.Value - 1].GyroY + tBsixaxisGyroY.Value * 2) / 3);
                SetDynamicTrackBarValue(tBsixaxisGyroZ, (Program.rootHub.ExposedState[(int)nUDSixaxis.Value - 1].GyroZ + tBsixaxisGyroZ.Value * 2) / 3);
                SetDynamicTrackBarValue(tBsixaxisAccelX, (Program.rootHub.ExposedState[(int)nUDSixaxis.Value - 1].AccelX + tBsixaxisAccelX.Value * 2) / 3);
                SetDynamicTrackBarValue(tBsixaxisAccelY, (Program.rootHub.ExposedState[(int)nUDSixaxis.Value - 1].AccelY + tBsixaxisAccelY.Value * 2) / 3);
                SetDynamicTrackBarValue(tBsixaxisAccelZ, (Program.rootHub.ExposedState[(int)nUDSixaxis.Value - 1].AccelZ + tBsixaxisAccelZ.Value * 2) / 3);

                int x = Program.rootHub.getDS4State((int)nUDSixaxis.Value - 1).LX;
                int y = Program.rootHub.getDS4State((int)nUDSixaxis.Value - 1).LY;
                //else
                //double hypot = Math.Min(127.5f, Math.Sqrt(Math.Pow(x - 127.5f, 2) + Math.Pow(y - 127.5f, 2)));
                if (nUDLSCurve.Value > 0)
                {
                    float max = x + y;
                    double curvex;
                    double curvey;
                    double multimax = TValue(382.5, max, (double)nUDLSCurve.Value);
                    double multimin = TValue(127.5, max, (double)nUDLSCurve.Value);
                    if ((x > 127.5f && y > 127.5f) || (x < 127.5f && y < 127.5f))
                    {
                        curvex = (x > 127.5f ? Math.Min(x, (x / max) * multimax) : Math.Max(x, (x / max) * multimin));
                        curvey = (y > 127.5f ? Math.Min(y, (y / max) * multimax) : Math.Max(y, (y / max) * multimin));
                        btnLSTrack.Location = new Point((int)(dpix * curvex / 2.09 + lbLSTrack.Location.X), (int)(dpiy * curvey / 2.09 + lbLSTrack.Location.Y));
                    }
                    else
                    {
                        if (x < 127.5f)
                        {
                            curvex = Math.Min(x, (x / max) * multimax);
                            curvey = Math.Min(y, (-(y / max) * multimax + 510));
                        }
                        else
                        {
                            curvex = Math.Min(x, (-(x / max) * multimax + 510));
                            curvey = Math.Min(y, (y / max) * multimax);
                        }
                        btnLSTrack.Location = new Point((int)(dpix * curvex / 2.09 + lbLSTrack.Location.X), (int)(dpiy * curvey / 2.09 + lbLSTrack.Location.Y));
                    }
                }
                else
                btnLSTrack.Location = new Point((int)(dpix * x / 2.09 + lbLSTrack.Location.X), (int)(dpiy * y / 2.09 + lbLSTrack.Location.Y));
                //*/
                x = Program.rootHub.getDS4State((int)nUDSixaxis.Value - 1).RX;
                y = Program.rootHub.getDS4State((int)nUDSixaxis.Value - 1).RY;
                if (nUDRSCurve.Value > 0)
                {
                    float max = x + y;
                    double curvex;
                    double curvey;
                    double multimax = TValue(382.5, max, (double)nUDRSCurve.Value);
                    double multimin = TValue(127.5, max, (double)nUDRSCurve.Value);
                    if ((x > 127.5f && y > 127.5f) || (x < 127.5f && y < 127.5f))
                    {
                        curvex = (x > 127.5f ? Math.Min(x, (x / max) * multimax) : Math.Max(x, (x / max) * multimin));
                        curvey = (y > 127.5f ? Math.Min(y, (y / max) * multimax) : Math.Max(y, (y / max) * multimin));
                        btnRSTrack.Location = new Point((int)(dpix * curvex / 2.09 + lbRSTrack.Location.X), (int)(dpiy * curvey / 2.09 + lbRSTrack.Location.Y));
                    }
                    else
                    {
                        if (x < 127.5f)
                        {
                            curvex = Math.Min(x, (x / max) * multimax);
                            curvey = Math.Min(y, (-(y / max) * multimax + 510));
                        }
                        else
                        {
                            curvex = Math.Min(x, (-(x / max) * multimax + 510));
                            curvey = Math.Min(y, (y / max) * multimax);
                        }
                        btnRSTrack.Location = new Point((int)(dpix * curvex / 2.09 + lbRSTrack.Location.X), (int)(dpiy * curvey / 2.09 + lbRSTrack.Location.Y));
                    }
                }
                else
                    btnRSTrack.Location = new Point((int)(dpix * x / 2.09 + lbRSTrack.Location.X), (int)(dpiy * y / 2.09 + lbRSTrack.Location.Y));
                x = -Program.rootHub.ExposedState[(int)nUDSixaxis.Value - 1].GyroX / 62 + 127;
                y = Program.rootHub.ExposedState[(int)nUDSixaxis.Value - 1].GyroZ / 62 + 127;
                btnSATrack.Location = new Point((int)(dpix * Clamp(0,x / 2.09,lbSATrack.Size.Width) + lbSATrack.Location.X), (int)(dpiy * Clamp(0,y / 2.09,lbSATrack.Size.Height)  + lbSATrack.Location.Y));


                tBL2.Value = Program.rootHub.getDS4State((int)nUDSixaxis.Value - 1).L2;
                lbL2Track.Location = new Point(tBL2.Location.X - (int)(dpix * 15), (int)((dpix * (24 - tBL2.Value / 10.625) + 10)));
                if (tBL2.Value == 255)
                    lbL2Track.ForeColor = Color.Green;
                else if (tBL2.Value < (double)nUDL2.Value * 255)
                    lbL2Track.ForeColor = Color.Red;
                else
                    lbL2Track.ForeColor = Color.Black;

                tBR2.Value = Program.rootHub.getDS4State((int)nUDSixaxis.Value - 1).R2;
                lbR2Track.Location = new Point(tBR2.Location.X + (int)(dpix * 20), (int)((dpix * (24 - tBR2.Value / 10.625) + 10)));
                if (tBR2.Value == 255)
                    lbR2Track.ForeColor = Color.Green;
                else if (tBR2.Value < (double)nUDR2.Value * 255)
                    lbR2Track.ForeColor = Color.Red;
                else
                    lbR2Track.ForeColor = Color.Black;


                double latency = Program.rootHub.DS4Controllers[(int)nUDSixaxis.Value - 1].Latency;
                lbInputDelay.Text = Properties.Resources.InputDelay.Replace("*number*", latency.ToString());
                if (latency > 10)
                    pBDelayTracker.BackColor = Color.Red;
                else if (latency > 5)
                    pBDelayTracker.BackColor = Color.Yellow;
                else
                    pBDelayTracker.BackColor = Color.Green;
            }
        }

        double TValue(double value1, double value2, double percent)
        {
            percent /= 100f;
            return value1 * percent + value2 * (1 - percent);
        }
        private void InputDS4(object sender, EventArgs e)
        {
            if (Form.ActiveForm == root && cBControllerInput.Checked && tabControls.SelectedIndex < 2)
            switch (Program.rootHub.GetInputkeys((int)nUDSixaxis.Value - 1))
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
        }
        private void button_MouseHover(object sender, EventArgs e)
        {
            switch (((Button)sender).Name)
            {
                #region
                case "bnCross": lBControls.SelectedIndex = 0; break;
                case "bnCircle": lBControls.SelectedIndex = 1; break;
                case "bnSquare": lBControls.SelectedIndex = 2; break;
                case "bnTriangle": lBControls.SelectedIndex = 3; break;
                case "bnOptions": lBControls.SelectedIndex = 4; break;
                case "bnShare": lBControls.SelectedIndex = 5; break;
                case "bnUp": lBControls.SelectedIndex = 6; break;
                case "bnDown": lBControls.SelectedIndex = 7; break;
                case "bnLeft": lBControls.SelectedIndex = 8; break;
                case "bnRight": lBControls.SelectedIndex = 9; break;
                case "bnPS": lBControls.SelectedIndex = 10; break;
                case "bnL1": lBControls.SelectedIndex = 11; break;
                case "bnR1": lBControls.SelectedIndex = 12; break;
                case "bnL2": lBControls.SelectedIndex = 13; break;
                case "bnR2": lBControls.SelectedIndex = 14; break;
                case "bnL3": lBControls.SelectedIndex = 15; break;
                case "bnR3": lBControls.SelectedIndex = 16; break;
                case "bnTouchLeft": lBControls.SelectedIndex = 17; break;
                case "bnTouchRight": lBControls.SelectedIndex = 18; break;
                case "bnTouchMulti": lBControls.SelectedIndex = 19; break;
                case "bnTouchUpper": lBControls.SelectedIndex = 20; break;
                case "bnLSUp": lBControls.SelectedIndex = 21; break;
                case "bnLSDown": lBControls.SelectedIndex = 22; break;
                case "bnLSLeft": lBControls.SelectedIndex = 23; break;
                case "bnLSRight": lBControls.SelectedIndex = 24; break;
                case "bnRSUp": lBControls.SelectedIndex = 25; break;
                case "bnRSDown": lBControls.SelectedIndex = 26; break;
                case "bnRSLeft": lBControls.SelectedIndex = 27; break;
                case "bnRSRight": lBControls.SelectedIndex = 28; break;
                case "bnGyroZN": lBControls.SelectedIndex = 29; break;
                case "bnGyroZP": lBControls.SelectedIndex = 30; break;
                case "bnGyroXP": lBControls.SelectedIndex = 31; break;
                case "bnGyroXN": lBControls.SelectedIndex = 32; break;

                case "bnSwipeUp": lBControls.SelectedIndex = 33; break;
                case "bnSwipeDown": lBControls.SelectedIndex = 34; break;
                case "bnSwipeLeft": lBControls.SelectedIndex = 35; break;
                case "bnSwipeRight": lBControls.SelectedIndex = 36; break;

                case "bnShiftCross": lBShiftControls.SelectedIndex = 0; break;
                case "bnShiftCircle": lBShiftControls.SelectedIndex = 1; break;
                case "bnShiftSquare": lBShiftControls.SelectedIndex = 2; break;
                case "bnShiftTriangle": lBShiftControls.SelectedIndex = 3; break;
                case "bnShiftOptions": lBShiftControls.SelectedIndex = 4; break;
                case "bnShiftShare": lBShiftControls.SelectedIndex = 5; break;
                case "bnShiftUp": lBShiftControls.SelectedIndex = 6; break;
                case "bnShiftDown": lBShiftControls.SelectedIndex = 7; break;
                case "bnShiftLeft": lBShiftControls.SelectedIndex = 8; break;
                case "bnShiftRight": lBShiftControls.SelectedIndex = 9; break;
                case "bnShiftPS": lBShiftControls.SelectedIndex = 10; break;
                case "bnShiftL1": lBShiftControls.SelectedIndex = 11; break;
                case "bnShiftR1": lBShiftControls.SelectedIndex = 12; break;
                case "bnShiftL2": lBShiftControls.SelectedIndex = 13; break;
                case "bnShiftR2": lBShiftControls.SelectedIndex = 14; break;
                case "bnShiftL3": lBShiftControls.SelectedIndex = 15; break;
                case "bnShiftR3": lBShiftControls.SelectedIndex = 16; break;
                case "bnShiftTouchLeft": lBShiftControls.SelectedIndex = 17; break;
                case "bnShiftTouchRight": lBShiftControls.SelectedIndex = 18; break;
                case "bnShiftTouchMulti": lBShiftControls.SelectedIndex = 19; break;
                case "bnShiftTouchUpper": lBShiftControls.SelectedIndex = 20; break;
                case "bnShiftLSUp": lBShiftControls.SelectedIndex = 21; break;
                case "bnShiftLSDown": lBShiftControls.SelectedIndex = 22; break;
                case "bnShiftLSLeft": lBShiftControls.SelectedIndex = 23; break;
                case "bnShiftLSRight": lBShiftControls.SelectedIndex = 24; break;
                case "bnShiftRSUp": lBShiftControls.SelectedIndex = 25; break;
                case "bnShiftRSDown": lBShiftControls.SelectedIndex = 26; break;
                case "bnShiftRSLeft": lBShiftControls.SelectedIndex = 27; break;
                case "bnShiftRSRight": lBShiftControls.SelectedIndex = 28; break;
                case "bnShiftGyroZN": lBShiftControls.SelectedIndex = 29; break;
                case "bnShiftGyroZP": lBShiftControls.SelectedIndex = 30; break;
                case "bnShiftGyroXP": lBShiftControls.SelectedIndex = 31; break;
                case "bnShiftGyroXN": lBShiftControls.SelectedIndex = 32; break;

                case "bnShiftSwipeUp": lBShiftControls.SelectedIndex = 33; break;
                case "bnShiftSwipeDown": lBShiftControls.SelectedIndex = 34; break;
                case "bnShiftSwipeLeft": lBShiftControls.SelectedIndex = 35; break;
                case "bnShiftSwipeRight": lBShiftControls.SelectedIndex = 36; break;
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
            pnlLowBattery.Visible = cBLightbyBattery.Checked;
            lbFull.Text = (cBLightbyBattery.Checked ? Properties.Resources.Full + ":": Properties.Resources.Color + ":");
            pnlFull.Location = new Point(pnlFull.Location.X, (cBLightbyBattery.Checked ? (int)(dpix * 42) : (pnlFull.Location.Y + pnlLowBattery.Location.Y) / 2));
            MainColor[device] = new DS4Color((byte)tBRedBar.Value, (byte)tBGreenBar.Value, (byte)tBBlueBar.Value);
            LowColor[device] = new DS4Color((byte)tBLowRedBar.Value, (byte)tBLowGreenBar.Value, (byte)tBLowBlueBar.Value);
            ShiftColor[device] = new DS4Color((byte)tBShiftRedBar.Value, (byte)tBShiftGreenBar.Value, (byte)tBShiftBlueBar.Value);
            ChargingColor[device] = new DS4Color(btnChargingColor.BackColor);
            FlashType[device] = (byte)cBFlashType.SelectedIndex;
            if (btnFlashColor.BackColor != pBController.BackColor)
                FlashColor[device] = new DS4Color(btnFlashColor.BackColor);
            else
                FlashColor[device] = new DS4Color(Color.Black);
            L2Deadzone[device] = (byte)Math.Round((nUDL2.Value * 255), 0);
            R2Deadzone[device] = (byte)Math.Round((nUDR2.Value * 255), 0);
            RumbleBoost[device] = (byte)nUDRumbleBoost.Value;
            TouchSensitivity[device] = (byte)nUDTouch.Value;
            TouchpadJitterCompensation[device] = cBTouchpadJitterCompensation.Checked;
            LowerRCOn[device] = cBlowerRCOn.Checked;
            ScrollSensitivity[device] = (byte)nUDScroll.Value;
            DoubleTap[device] = cBDoubleTap.Checked;
            TapSensitivity[device] = (byte)nUDTap.Value;
            IdleDisconnectTimeout[device] = (int)(nUDIdleDisconnect.Value * 60);
            Rainbow[device] = (int)nUDRainbow.Value;
            RSDeadzone[device] = (int)Math.Round((nUDRS.Value * 127), 0);
            LSDeadzone[device] = (int)Math.Round((nUDLS.Value * 127), 0);
            ButtonMouseSensitivity[device] = (int)numUDMouseSens.Value;
            FlashAt[device] = (int)nUDflashLED.Value;
            SXDeadzone[device] = (double)nUDSX.Value;
            SZDeadzone[device] = (double)nUDSZ.Value;
            MouseAccel[device] = cBMouseAccel.Checked;
            ShiftModifier[device] = cBShiftControl.SelectedIndex;
            DinputOnly[device] = cBDinput.Checked;
            StartTouchpadOff[device] = cbStartTouchpadOff.Checked;
            UseTPforControls[device] = cBTPforControls.Checked;
            DS4Mapping = cBControllerInput.Checked;
            LSCurve[device] = (int)Math.Round(nUDLSCurve.Value, 0);
            RSCurve[device] = (int)Math.Round(nUDRSCurve.Value, 0);
            List<string> pactions = new List<string>();
            foreach (ListViewItem lvi in lVActions.Items)
                if (lvi.Checked)
                    pactions.Add(lvi.Text);
            ProfileActions[device] = pactions;
            gBTouchpad.Enabled = !cBTPforControls.Checked;
            if (cBTPforControls.Checked)
                tabControls.Size = new Size(tabControls.Size.Width, (int)(282 * dpiy));
            else
                tabControls.Size = new Size(tabControls.Size.Width, (int)(242 * dpiy));
            if (nUDRainbow.Value == 0) pBRainbow.Image = greyscale;
            else pBRainbow.Image = colored;
        }

        KBM360 kbm360 = null;

        private void Show_ControlsBn(object sender, EventArgs e)
        {
            lastSelected = (Button)sender;
            kbm360 = new KBM360(device, this, lastSelected);
            kbm360.Icon = this.Icon;
            kbm360.ShowDialog();
        }

        public void ChangeButtonText(string controlname, KeyValuePair<object, string> tag)
        {
            lastSelected.Text = controlname;
            int value;
            if (tag.Key == null)
                lastSelected.Tag = tag;
            else if (Int32.TryParse(tag.Key.ToString(), out value))
                lastSelected.Tag = new KeyValuePair<int, string>(value, tag.Value);
            else if (tag.Key is Int32[])
                lastSelected.Tag = new KeyValuePair<Int32[], string>((Int32[])tag.Key, tag.Value);
            else
                lastSelected.Tag = new KeyValuePair<string, string>(tag.Key.ToString(), tag.Value);    
        }
        public void ChangeButtonText(string controlname, KeyValuePair<object, string> tag, System.Windows.Forms.Control ctrl)
        {
            if (ctrl is Button)
            {
                Button btn = (Button)ctrl;
                btn.Text = controlname;
                int value;
                if (tag.Key == null)
                    btn.Tag = tag;
                else if (Int32.TryParse(tag.Key.ToString(), out value))
                    btn.Tag = new KeyValuePair<int, string>(value, tag.Value);
                else if (tag.Key is Int32[])
                    btn.Tag = new KeyValuePair<Int32[], string>((Int32[])tag.Key, tag.Value);
                else
                    btn.Tag = new KeyValuePair<string, string>(tag.Key.ToString(), tag.Value);
            }
        }
        public void ChangeButtonText(string controlname)
        {
            lastSelected.Text = controlname;
            lastSelected.Tag = controlname;
        }

        public void Toggle_Bn(bool SC, bool TG, bool MC,  bool MR)
        {
            if (lastSelected.Tag is KeyValuePair<int, string> || lastSelected.Tag is KeyValuePair<UInt16, string> || lastSelected.Tag is KeyValuePair<int[], string>)
                lastSelected.Font = new Font(lastSelected.Font, 
                    (SC ? FontStyle.Bold : FontStyle.Regular) | (TG ? FontStyle.Italic : FontStyle.Regular) | 
                    (MC ? FontStyle.Underline : FontStyle.Regular) | (MR ? FontStyle.Strikeout : FontStyle.Regular));
            else if (lastSelected.Tag is KeyValuePair<string, string>)
                if (lastSelected.Tag.ToString().Contains("Mouse Button"))
                    lastSelected.Font = new Font(lastSelected.Font, TG ? FontStyle.Italic : FontStyle.Regular);
            else
                lastSelected.Font = new Font(lastSelected.Font, FontStyle.Regular);
        }
        public void Toggle_Bn(bool SC, bool TG, bool MC, bool MR, System.Windows.Forms.Control ctrl)
        {
            if (ctrl is Button)
            {
                Button btn = (Button)ctrl;
                if (btn.Tag is KeyValuePair<int, string> || btn.Tag is KeyValuePair<UInt16, string> || btn.Tag is KeyValuePair<int[], string>)
                    btn.Font = new Font(btn.Font,
                        (SC ? FontStyle.Bold : FontStyle.Regular) | (TG ? FontStyle.Italic : FontStyle.Regular) |
                        (MC ? FontStyle.Underline : FontStyle.Regular) | (MR ? FontStyle.Strikeout : FontStyle.Regular));
                else if (btn.Tag is KeyValuePair<string, string>)
                    if (btn.Tag.ToString().Contains("Mouse Button"))
                        btn.Font = new Font(btn.Font, TG ? FontStyle.Italic : FontStyle.Regular);
                    else
                        btn.Font = new Font(btn.Font, FontStyle.Regular);
            }
        }

        private void btnSticks_Enter(object sender, EventArgs e)
        {
            pnlSticks.Visible = true;
            pnlMain.Visible = false;
        }

        private void btnFullView_Click(object sender, EventArgs e)
        {
            pnlSticks.Visible = false;
            pnlMain.Visible = true;
        }

        private void btnShiftSticks_Enter(object sender, EventArgs e)
        {
            pnlShiftSticks.Visible = true;
            pnlShiftMain.Visible = false;
        }

        private void btnShiftFullView_Click(object sender, EventArgs e)
        {
            pnlShiftSticks.Visible = false;
            pnlShiftMain.Visible = true;
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
            if (device < 4)
                DS4LightBar.forcelight[device] = false;
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
            if (device < 4)
                DS4LightBar.forcelight[device] = false;
        }


        private void btnChargingColor_Click(object sender, EventArgs e)
        {
            advColorDialog.Color = btnChargingColor.BackColor;
            advColorDialog_OnUpdateColor(btnChargingColor.BackColor, e);
            if (advColorDialog.ShowDialog() == DialogResult.OK)
            {
                btnChargingColor.BackColor = advColorDialog.Color;
            }
            if (device < 4)
                DS4LightBar.forcelight[device] = false;
            ChargingColor[device] = new DS4Color(btnChargingColor.BackColor);
        }
        private void advColorDialog_OnUpdateColor(object sender, EventArgs e)
        {
            if (sender is Color && device < 4)
            {
                Color color = (Color)sender;
                DS4Color dcolor = new DS4Color { red = color.R, green = color.G, blue = color.B };
                DS4LightBar.forcedColor[device] = dcolor;
                DS4LightBar.forcedFlash[device] = 0;
                DS4LightBar.forcelight[device] = true;
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
            MainColor[device] = new DS4Color((byte)tBRedBar.Value, (byte)tBGreenBar.Value, (byte)tBBlueBar.Value);
            if (!saving)
                tp.Show(((TrackBar)sender).Value.ToString(), ((TrackBar)sender), (int)(dpix * 100), 0, 2000);
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
            MainColor[device] = new DS4Color((byte)tBRedBar.Value, (byte)tBGreenBar.Value, (byte)tBBlueBar.Value);
            if (!saving)
                tp.Show(((TrackBar)sender).Value.ToString(), ((TrackBar)sender), (int)(100*dpix), 0, 2000);
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
            MainColor[device] = new DS4Color((byte)tBRedBar.Value, (byte)tBGreenBar.Value, (byte)tBBlueBar.Value);
            if (!saving)
                tp.Show(((TrackBar)sender).Value.ToString(), ((TrackBar)sender), (int)(100 * dpix), 0, 2000);
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
            LowColor[device] = new DS4Color((byte)tBLowRedBar.Value, (byte)tBLowGreenBar.Value, (byte)tBLowBlueBar.Value);
            if (!saving)
                tp.Show(((TrackBar)sender).Value.ToString(), ((TrackBar)sender), (int)(100 * dpix), 0, 2000);
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
            LowColor[device] = new DS4Color((byte)tBLowRedBar.Value, (byte)tBLowGreenBar.Value, (byte)tBLowBlueBar.Value);
            if (!saving)
                tp.Show(((TrackBar)sender).Value.ToString(), ((TrackBar)sender), (int)(100 * dpix), 0, 2000);
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
            LowColor[device] = new DS4Color((byte)tBLowRedBar.Value, (byte)tBLowGreenBar.Value, (byte)tBLowBlueBar.Value);
            if (!saving)
                tp.Show(((TrackBar)sender).Value.ToString(), ((TrackBar)sender), (int)(100 * dpix), 0, 2000);
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
            pBShiftController.BackColor = Color.FromArgb((alphacolor > 205 ? 255 : (alphacolor + 50)), full);
            ShiftColor[device] = new DS4Color((byte)tBShiftRedBar.Value, (byte)tBShiftGreenBar.Value, (byte)tBShiftBlueBar.Value);
            if (!saving)
                tp.Show(((TrackBar)sender).Value.ToString(), ((TrackBar)sender), (int)(100 * dpix), 0, 2000);
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
            pBShiftController.BackColor = Color.FromArgb((alphacolor > 205 ? 255 : (alphacolor + 50)), full);
            ShiftColor[device] = new DS4Color((byte)tBShiftRedBar.Value, (byte)tBShiftGreenBar.Value, (byte)tBShiftBlueBar.Value);
            if (!saving)
                tp.Show(((TrackBar)sender).Value.ToString(), ((TrackBar)sender), (int)(100 * dpix), 0, 2000);
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
            pBShiftController.BackColor = Color.FromArgb((alphacolor > 205 ? 255 : (alphacolor + 50)), full);
            ShiftColor[device] = new DS4Color((byte)tBShiftRedBar.Value, (byte)tBShiftGreenBar.Value, (byte)tBShiftBlueBar.Value);
            if (!saving)
                tp.Show(((TrackBar)sender).Value.ToString(), ((TrackBar)sender), (int)(100 * dpix), 0, 2000);
        }

        public Color HuetoRGB(float hue, float light, Color rgb)
        {
            float L = (float)Math.Max(.5, light);
            float C = (1 - Math.Abs(2 * L - 1));
            float X = (C * (1 - Math.Abs((hue / 60) % 2 - 1)));
            float m = L - C / 2;
            float R =0, G=0, B=0;
            if (light == 1) return Color.White;
            else if (rgb.R == rgb.G && rgb.G == rgb.B) return Color.White;
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
            RumbleBoost[device] = (byte)nUDRumbleBoost.Value;
            byte h = (byte)Math.Min(255, (255 * nUDRumbleBoost.Value / 100));
            byte l = (byte)Math.Min(255, (255 * nUDRumbleBoost.Value / 100));
            bool hB = btnRumbleHeavyTest.Text == Properties.Resources.TestLText;
            bool lB = btnRumbleLightTest.Text == Properties.Resources.TestLText;
            Program.rootHub.setRumble((byte)(hB ? h : 0), (byte)(lB ? l : 0), device);
        }

        private void btnRumbleHeavyTest_Click(object sender, EventArgs e)
        {
            DS4Device d = Program.rootHub.DS4Controllers[(int)nUDSixaxis.Value - 1];
            if (d != null)
                if (((Button)sender).Text == Properties.Resources.TestHText)
                {
                    Program.rootHub.setRumble((byte)Math.Min(255, (255 * nUDRumbleBoost.Value / 100)), d.RightLightFastRumble, (int)nUDSixaxis.Value - 1);
                    ((Button)sender).Text = Properties.Resources.StopHText;
                }
                else
                {
                    Program.rootHub.setRumble(0, d.RightLightFastRumble, (int)nUDSixaxis.Value - 1);
                    ((Button)sender).Text = Properties.Resources.TestHText;
                }
        }

        private void btnRumbleLightTest_Click(object sender, EventArgs e)
        {
            DS4Device d = Program.rootHub.DS4Controllers[(int)nUDSixaxis.Value - 1];
            if (d != null)
                if (((Button)sender).Text == Properties.Resources.TestLText)
                {
                    Program.rootHub.setRumble(d.LeftHeavySlowRumble, (byte)Math.Min(255, (255 * nUDRumbleBoost.Value / 100)), (int)nUDSixaxis.Value - 1);
                    ((Button)sender).Text = Properties.Resources.StopLText;
                }
                else
                {
                    Program.rootHub.setRumble(d.LeftHeavySlowRumble, 0, (int)nUDSixaxis.Value - 1);
                    ((Button)sender).Text = Properties.Resources.TestLText;
                }
        }

        private void numUDTouch_ValueChanged(object sender, EventArgs e)
        {
            TouchSensitivity[device] = (byte)nUDTouch.Value;
        }

        private void numUDTap_ValueChanged(object sender, EventArgs e)
        {
            TapSensitivity[device] = (byte)nUDTap.Value;
        }

        private void numUDScroll_ValueChanged(object sender, EventArgs e)
        {
            ScrollSensitivity[device] = (int)nUDScroll.Value;
        }
        private void ledAsBatteryIndicator_CheckedChanged(object sender, EventArgs e)
        {
            LedAsBatteryIndicator[device] = cBLightbyBattery.Checked;
            pnlLowBattery.Visible = cBLightbyBattery.Checked;
            pnlFull.Location = new Point(pnlFull.Location.X, (cBLightbyBattery.Checked ? (int)(dpix * 42) : (pnlFull.Location.Y + pnlLowBattery.Location.Y) / 2));
            lbFull.Text = (cBLightbyBattery.Checked ? Properties.Resources.Full + ":" : Properties.Resources.Color + ":");
        }

        private void lowerRCOffCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            LowerRCOn[device] = cBlowerRCOn.Checked;
        }

        private void touchpadJitterCompensation_CheckedChanged(object sender, EventArgs e)
        {
            TouchpadJitterCompensation[device] = cBTouchpadJitterCompensation.Checked;
        }
        
        private void flushHIDQueue_CheckedChanged(object sender, EventArgs e)
        {
            FlushHIDQueue[device] = cBFlushHIDQueue.Checked;
        }

        private void nUDIdleDisconnect_ValueChanged(object sender, EventArgs e)
        {
            IdleDisconnectTimeout[device] = (int)(nUDIdleDisconnect.Value * 60);
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
                LoadProfile(i, false, Program.rootHub); //Refreshes all profiles in case other controllers are using the same profile
            if (olddinputcheck != cBDinput.Checked)
            {
                root.btnStartStop_Clicked(false);
                root.btnStartStop_Clicked(false);
            }
            if (btnRumbleHeavyTest.Text == Properties.Resources.StopText)
                Program.rootHub.setRumble(0, 0, (int)nUDSixaxis.Value - 1);
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
            DoubleTap[device] = cBDoubleTap.Checked;
        }

        public void UpdateLists()
        {
            lBControls.Items[0] = "Cross : " + UpdateRegButtonList(bnCross);
            lBControls.Items[1] = "Circle : " + UpdateRegButtonList(bnCircle);
            lBControls.Items[2] = "Square : " + UpdateRegButtonList(bnSquare);
            lBControls.Items[3] = "Triangle : " + UpdateRegButtonList(bnTriangle);
            lBControls.Items[4] = "Options : " + UpdateRegButtonList(bnOptions);
            lBControls.Items[5] = "Share : " + UpdateRegButtonList(bnShare);
            lBControls.Items[6] = "Up : " + UpdateRegButtonList(bnUp);
            lBControls.Items[7] = "Down : " + UpdateRegButtonList(bnDown);
            lBControls.Items[8] = "Left : " + UpdateRegButtonList(bnLeft);
            lBControls.Items[9] = "Right : " + UpdateRegButtonList(bnRight);
            lBControls.Items[10] = "PS : " + UpdateRegButtonList(bnPS);
            lBControls.Items[11] = "L1 : " + UpdateRegButtonList(bnL1);
            lBControls.Items[12] = "R1 : " + UpdateRegButtonList(bnR1);
            lBControls.Items[13] = "L2 : " + UpdateRegButtonList(bnL2);
            lBControls.Items[14] = "R2 : " + UpdateRegButtonList(bnR2);
            lBControls.Items[15] = "L3 : " + UpdateRegButtonList(bnL3);
            lBControls.Items[16] = "R3 : " + UpdateRegButtonList(bnR3);
            lBControls.Items[17] = "Left Touch : " + UpdateRegButtonList(bnTouchLeft);
            lBControls.Items[18] = "Right Touch : " + UpdateRegButtonList(bnTouchRight);
            lBControls.Items[19] = "Multitouch : " + UpdateRegButtonList(bnTouchMulti);
            lBControls.Items[20] = "Upper Touch : " + UpdateRegButtonList(bnTouchUpper);
            lBControls.Items[21] = "LS Up : " + UpdateRegButtonList(bnLSUp);
            lBControls.Items[22] = "LS Down : " + UpdateRegButtonList(bnLSDown);
            lBControls.Items[23] = "LS Left : " + UpdateRegButtonList(bnLSLeft);
            lBControls.Items[24] = "LS Right : " + UpdateRegButtonList(bnLSRight);
            lBControls.Items[25] = "RS Up : " + UpdateRegButtonList(bnRSUp);
            lBControls.Items[26] = "RS Down : " + UpdateRegButtonList(bnRSDown);
            lBControls.Items[27] = "RS Left : " + UpdateRegButtonList(bnRSLeft);
            lBControls.Items[28] = "RS Right : " + UpdateRegButtonList(bnRSRight);
            lBControls.Items[29] = Properties.Resources.TiltUp + " : " + UpdateRegButtonList(bnGyroZN);
            lBControls.Items[30] = Properties.Resources.TiltDown + " : " + UpdateRegButtonList(bnGyroZP);
            lBControls.Items[31] = Properties.Resources.TiltLeft + " : " + UpdateRegButtonList(bnGyroXP);
            lBControls.Items[32] = Properties.Resources.TiltRight + " : " + UpdateRegButtonList(bnGyroXN);
            bnGyroZN.Text = Properties.Resources.TiltUp;
            bnGyroZP.Text = Properties.Resources.TiltDown;
            bnGyroXP.Text = Properties.Resources.TiltLeft;
            bnGyroXN.Text = Properties.Resources.TiltRight;
            if (lBControls.Items.Count > 33)
            {
                lBControls.Items[33] = Properties.Resources.SwipeUp + " : " + UpdateRegButtonList(bnSwipeUp);
                lBControls.Items[34] = Properties.Resources.SwipeDown + " : " + UpdateRegButtonList(bnSwipeDown);
                lBControls.Items[35] = Properties.Resources.SwipeLeft + " : " + UpdateRegButtonList(bnSwipeLeft);
                lBControls.Items[36] = Properties.Resources.SwipeRight + " : " + UpdateRegButtonList(bnSwipeRight);
                bnSwipeUp.Text = Properties.Resources.SwipeUp;
                bnSwipeDown.Text = Properties.Resources.SwipeDown;
                bnSwipeLeft.Text = Properties.Resources.SwipeLeft;
                bnSwipeRight.Text = Properties.Resources.SwipeRight;
            }

            /*foreach (Button b in subbuttons)
                if (b.Tag == null)
                    b.Text = "Fall Back to " + ((Button)Controls.Find(b.Name.Remove(2,5), true)[0]).Text;*/
            lBShiftControls.Items[0] = "Cross : " + UpdateRegButtonList(bnShiftCross);
            lBShiftControls.Items[1] = "Circle : " + UpdateRegButtonList(bnShiftCircle);
            lBShiftControls.Items[2] = "Square : " + UpdateRegButtonList(bnShiftSquare);
            lBShiftControls.Items[3] = "Triangle : " + UpdateRegButtonList(bnShiftTriangle);
            lBShiftControls.Items[4] = "Options : " + UpdateRegButtonList(bnShiftOptions);
            lBShiftControls.Items[5] = "Share : " + UpdateRegButtonList(bnShiftShare);
            lBShiftControls.Items[6] = "Up : " + UpdateRegButtonList(bnShiftUp);
            lBShiftControls.Items[7] = "Down : " + UpdateRegButtonList(bnShiftDown);
            lBShiftControls.Items[8] = "Left : " + UpdateRegButtonList(bnShiftLeft);
            lBShiftControls.Items[9] = "Right : " + UpdateRegButtonList(bnShiftRight);
            lBShiftControls.Items[10] = "PS : " + UpdateRegButtonList(bnShiftPS);
            lBShiftControls.Items[11] = "L1 : " + UpdateRegButtonList(bnShiftL1);
            lBShiftControls.Items[12] = "R1 : " + UpdateRegButtonList(bnShiftR1);
            lBShiftControls.Items[13] = "L2 : " + UpdateRegButtonList(bnShiftL2);
            lBShiftControls.Items[14] = "R2 : " + UpdateRegButtonList(bnShiftR2);
            lBShiftControls.Items[15] = "L3 : " + UpdateRegButtonList(bnShiftL3);
            lBShiftControls.Items[16] = "R3 : " + UpdateRegButtonList(bnShiftR3);
            lBShiftControls.Items[17] = "Left Touch : " + UpdateRegButtonList(bnShiftTouchLeft);
            lBShiftControls.Items[18] = "Right Touch : " + UpdateRegButtonList(bnShiftTouchRight);
            lBShiftControls.Items[19] = "Multitouch : " + UpdateRegButtonList(bnShiftTouchMulti);
            lBShiftControls.Items[20] = "Upper Touch : " + UpdateRegButtonList(bnShiftTouchUpper);
            lBShiftControls.Items[21] = "LS Up : " + UpdateRegButtonList(bnShiftLSUp);
            lBShiftControls.Items[22] = "LS Down : " + UpdateRegButtonList(bnShiftLSDown);
            lBShiftControls.Items[23] = "LS Left : " + UpdateRegButtonList(bnShiftLSLeft);
            lBShiftControls.Items[24] = "LS Right : " + UpdateRegButtonList(bnShiftLSRight);
            lBShiftControls.Items[25] = "RS Up : " + UpdateRegButtonList(bnShiftRSUp);
            lBShiftControls.Items[26] = "RS Down : " + UpdateRegButtonList(bnShiftRSDown);
            lBShiftControls.Items[27] = "RS Left : " + UpdateRegButtonList(bnShiftRSLeft);
            lBShiftControls.Items[28] = "RS Right : " + UpdateRegButtonList(bnShiftRSRight);
            lBShiftControls.Items[29] = Properties.Resources.TiltUp + " : " + UpdateRegButtonList(bnShiftGyroZN);
            lBShiftControls.Items[30] = Properties.Resources.TiltDown + " : " + UpdateRegButtonList(bnShiftGyroZP);
            lBShiftControls.Items[31] = Properties.Resources.TiltLeft + " : " + UpdateRegButtonList(bnShiftGyroXP);
            lBShiftControls.Items[32] = Properties.Resources.TiltRight + " : " + UpdateRegButtonList(bnShiftGyroXN);
            bnShiftGyroZN.Text = Properties.Resources.TiltUp;
            bnShiftGyroZP.Text = Properties.Resources.TiltDown;
            bnShiftGyroXP.Text = Properties.Resources.TiltLeft;
            bnShiftGyroXN.Text = Properties.Resources.TiltRight;
            if (lBShiftControls.Items.Count > 33)
            {
                lBShiftControls.Items[33] = Properties.Resources.SwipeUp + " : " + UpdateRegButtonList(bnShiftSwipeUp);
                lBShiftControls.Items[34] = Properties.Resources.SwipeDown + " : " + UpdateRegButtonList(bnShiftSwipeDown);
                lBShiftControls.Items[35] = Properties.Resources.SwipeLeft + " : " + UpdateRegButtonList(bnShiftSwipeLeft);
                lBShiftControls.Items[36] = Properties.Resources.SwipeRight + " : " + UpdateRegButtonList(bnShiftSwipeRight);
                bnShiftSwipeUp.Text = Properties.Resources.SwipeUp;
                bnShiftSwipeDown.Text = Properties.Resources.SwipeDown;
                bnShiftSwipeLeft.Text = Properties.Resources.SwipeLeft;
                bnShiftSwipeRight.Text = Properties.Resources.SwipeRight;
            }
        }

        private string UpdateRegButtonList(Button button)
        {
            Button regbutton = null;
            bool shift = button.Name.Contains("Shift");
            if (shift)
                regbutton = ((Button)Controls.Find(button.Name.Remove(2, 5), true)[0]);
            bool extracontrol = button.Name.Contains("Gyro") || button.Name.Contains("Swipe");
            if (button.Tag is String && (String)button.Tag == "Unbound")
                return "Unbound";
            else if (button.Tag is KeyValuePair<Int32[], string>)
                return Properties.Resources.Macro  + (button.Font.Bold ? " " + Properties.Resources.ScanCode : "");
            else if (button.Tag is KeyValuePair<int, string>)
                return ((Keys)((KeyValuePair<int, string>)button.Tag).Key).ToString() + (button.Font.Bold ? " " + Properties.Resources.ScanCode : "");
            else if (button.Tag is KeyValuePair<UInt16, string>)
                return ((Keys)((KeyValuePair<UInt16, string>)button.Tag).Key).ToString() + (button.Font.Bold ? " " + Properties.Resources.ScanCode : "");
            else if (button.Tag is KeyValuePair<string, string>)
                return ((KeyValuePair<string, string>)button.Tag).Key;
            else if (shift && extracontrol && !(regbutton.Tag is KeyValuePair<object, string>) 
                && (button.Tag == null ||((KeyValuePair<object, string>)button.Tag).Key == null))
                return Properties.Resources.FallBackTo.Replace("*button*", UpdateRegButtonList(regbutton));
            else if (shift && !extracontrol && (button.Tag == null || ((KeyValuePair<object, string>)button.Tag).Key == null))
                return Properties.Resources.FallBackTo.Replace("button*", UpdateRegButtonList(regbutton));
            else if (!shift && !extracontrol)
                return defaults[button.Name];
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

            if (lBControls.SelectedIndex == 33) Show_ControlsBn(bnSwipeUp, e);
            if (lBControls.SelectedIndex == 34) Show_ControlsBn(bnSwipeDown, e);
            if (lBControls.SelectedIndex == 35) Show_ControlsBn(bnSwipeLeft, e);
            if (lBControls.SelectedIndex == 36) Show_ControlsBn(bnSwipeRight, e);
        }

        private void Show_ShiftControlsList(object sender, EventArgs e)
        {
            if (lBShiftControls.SelectedIndex == 0) Show_ControlsBn(bnShiftCross, e);
            if (lBShiftControls.SelectedIndex == 1) Show_ControlsBn(bnShiftCircle, e);
            if (lBShiftControls.SelectedIndex == 2) Show_ControlsBn(bnShiftSquare, e);
            if (lBShiftControls.SelectedIndex == 3) Show_ControlsBn(bnShiftTriangle, e);
            if (lBShiftControls.SelectedIndex == 4) Show_ControlsBn(bnShiftOptions, e);
            if (lBShiftControls.SelectedIndex == 5) Show_ControlsBn(bnShiftShare, e);
            if (lBShiftControls.SelectedIndex == 6) Show_ControlsBn(bnShiftUp, e);
            if (lBShiftControls.SelectedIndex == 7) Show_ControlsBn(bnShiftDown, e);
            if (lBShiftControls.SelectedIndex == 8) Show_ControlsBn(bnShiftLeft, e);
            if (lBShiftControls.SelectedIndex == 9) Show_ControlsBn(bnShiftRight, e);
            if (lBShiftControls.SelectedIndex == 10) Show_ControlsBn(bnShiftPS, e);
            if (lBShiftControls.SelectedIndex == 11) Show_ControlsBn(bnShiftL1, e);
            if (lBShiftControls.SelectedIndex == 12) Show_ControlsBn(bnShiftR1, e);
            if (lBShiftControls.SelectedIndex == 13) Show_ControlsBn(bnShiftL2, e);
            if (lBShiftControls.SelectedIndex == 14) Show_ControlsBn(bnShiftR2, e);
            if (lBShiftControls.SelectedIndex == 15) Show_ControlsBn(bnShiftL3, e);
            if (lBShiftControls.SelectedIndex == 16) Show_ControlsBn(bnShiftR3, e);

            if (lBShiftControls.SelectedIndex == 17) Show_ControlsBn(bnShiftTouchLeft, e);
            if (lBShiftControls.SelectedIndex == 18) Show_ControlsBn(bnShiftTouchRight, e);
            if (lBShiftControls.SelectedIndex == 19) Show_ControlsBn(bnShiftTouchMulti, e);
            if (lBShiftControls.SelectedIndex == 20) Show_ControlsBn(bnShiftTouchUpper, e);

            if (lBShiftControls.SelectedIndex == 21) Show_ControlsBn(bnShiftLSUp, e);
            if (lBShiftControls.SelectedIndex == 22) Show_ControlsBn(bnShiftLSDown, e);
            if (lBShiftControls.SelectedIndex == 23) Show_ControlsBn(bnShiftLSLeft, e);
            if (lBShiftControls.SelectedIndex == 24) Show_ControlsBn(bnShiftLSRight, e);
            if (lBShiftControls.SelectedIndex == 25) Show_ControlsBn(bnShiftRSUp, e);
            if (lBShiftControls.SelectedIndex == 26) Show_ControlsBn(bnShiftRSDown, e);
            if (lBShiftControls.SelectedIndex == 27) Show_ControlsBn(bnShiftRSLeft, e);
            if (lBShiftControls.SelectedIndex == 28) Show_ControlsBn(bnShiftRSRight, e);

            if (lBShiftControls.SelectedIndex == 29) Show_ControlsBn(bnShiftGyroZN, e);
            if (lBShiftControls.SelectedIndex == 30) Show_ControlsBn(bnShiftGyroZP, e);
            if (lBShiftControls.SelectedIndex == 31) Show_ControlsBn(bnShiftGyroXP, e);
            if (lBShiftControls.SelectedIndex == 32) Show_ControlsBn(bnShiftGyroXN, e);


            if (lBShiftControls.SelectedIndex == 33) Show_ControlsBn(bnShiftSwipeUp, e);
            if (lBShiftControls.SelectedIndex == 34) Show_ControlsBn(bnShiftSwipeDown, e);
            if (lBShiftControls.SelectedIndex == 35) Show_ControlsBn(bnShiftSwipeLeft, e);
            if (lBShiftControls.SelectedIndex == 36) Show_ControlsBn(bnShiftSwipeRight, e);
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
            Rainbow[device]= (double)nUDRainbow.Value;
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
                pnlLowBattery.Enabled = cBLightbyBattery.Checked;
                //pBRainbow.Location = new Point(216, pBRainbow.Location.Y);
                pBController.BackgroundImage = null;
                cBLightbyBattery.Text = Properties.Resources.ColorByBattery.Replace("*nl*", "\n");
            }
            lbspc.Enabled = on;
            pnlLowBattery.Enabled = !on;
            pnlFull.Enabled = !on;
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
            L2Deadzone[device] = (byte)(nUDL2.Value * 255);
        }

        private void numUDR2_ValueChanged(object sender, EventArgs e)
        {
            R2Deadzone[device] = (byte)(nUDR2.Value * 255);
        }

        private void nUDSX_ValueChanged(object sender, EventArgs e)
        {
            SXDeadzone[device] = (double)nUDSX.Value;
            lbSATrack.Refresh();            
        }

        private void nUDSZ_ValueChanged(object sender, EventArgs e)
        {
            SZDeadzone[device] = (double)nUDSZ.Value;
            lbSATrack.Refresh();
        }

        private void lbSATrack_Paint(object sender, PaintEventArgs e)
        {
            if (nUDSX.Value > 0 || nUDSZ.Value > 0)
            {
                e.Graphics.FillEllipse(Brushes.Red,
                    (int)(dpix * 63) - (int)(nUDSX.Value * 125) / 2,
                    (int)(dpix * 63) - (int)(nUDSZ.Value * 125) / 2,
                    (int)(nUDSX.Value * 125), (int)(nUDSZ.Value * 125));
            }
        }

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
            nUDRS.Value = Math.Round(nUDRS.Value, 2);
            RSDeadzone[device] = (int)Math.Round((nUDRS.Value * 127),0);
            lbRSTrack.BackColor = nUDRS.Value >= 0 ? Color.White : Color.Red;
            lbRSTrack.Refresh();
        }

        private void lbRSTrack_Paint(object sender, PaintEventArgs e)
        {
            if (nUDRS.Value > 0)
            {
                int value = (int)(nUDRS.Value * 125);
                e.Graphics.FillEllipse(Brushes.Red,
                    (int)(dpix * 63) - value / 2,
                    (int)(dpix * 63) - value / 2,
                    value, value);
            }
            else if (nUDRS.Value < 0)
            {
                int value = (int)((1 + nUDRS.Value) * 125);
                e.Graphics.FillEllipse(Brushes.White,
                    (int)(dpix * 63) - value / 2,
                    (int)(dpix * 63) - value / 2,
                    value, value);
            }
        }

        private void numUDLS_ValueChanged(object sender, EventArgs e)
        {
            nUDLS.Value = Math.Round(nUDLS.Value, 2);
            LSDeadzone[device] = (int)Math.Round((nUDLS.Value * 127), 0);
            lbLSTrack.BackColor = nUDLS.Value >= 0 ? Color.White : Color.Red;
            lbLSTrack.Refresh();
        }

        private void lbLSTrack_Paint(object sender, PaintEventArgs e)
        {
            if (nUDLS.Value > 0)
            {
                int value = (int)(nUDLS.Value * 125);
                e.Graphics.FillEllipse(Brushes.Red,
                    (int)(dpix * 63) - value / 2,
                    (int)(dpix * 63) - value / 2,
                    value, value);
            }
            else if (nUDLS.Value < 0)
            {
                int value = (int)((1 + nUDLS.Value) * 125);
                e.Graphics.FillEllipse(Brushes.White,
                    (int)(dpix * 63) - value / 2,
                    (int)(dpix * 63) - value / 2,
                    value, value);
            }
        }

        private void numUDMouseSens_ValueChanged(object sender, EventArgs e)
        {
            ButtonMouseSensitivity[device] = (int)numUDMouseSens.Value;
            //ButtonMouseSensitivity(device, (int)numUDMouseSens.Value);
        }

        private void LightBar_MouseDown(object sender, MouseEventArgs e)
        {
            if (!saving)
                tp.Show(((TrackBar)sender).Value.ToString(), ((TrackBar)sender), (int)(100 * dpix), 0, 2000);
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
            FlashAt[device] = (int)nUDflashLED.Value;
        }

        private void cBMouseAccel_CheckedChanged(object sender, EventArgs e)
        {
            MouseAccel[device] = cBMouseAccel.Checked;
        }

        private void cBShiftControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            ShiftModifier[device] = cBShiftControl.SelectedIndex;
            if (cBShiftControl.SelectedIndex < 1)
                cBShiftLight.Checked = false;
        }

        private void tabControls_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabControls.SelectedIndex == 3)
                sixaxisTimer.Start();
            else
                sixaxisTimer.Stop();
            if (tabControls.SelectedIndex == 1)
                pnlShift.Visible = true;
            else
                pnlShift.Visible = false;
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
            pnlSixaxis.Visible = !pnlSixaxis.Visible;
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
            if (ShiftModifier[device] < 1)
                cBShiftLight.Checked = false;
            if (!cBShiftLight.Checked)
            {
                pBShiftController.BackColor = pBController.BackColor;
                pBShiftController.BackgroundImage = pBController.BackgroundImage;
            }
            else
            {
                alphacolor = Math.Max(tBShiftRedBar.Value, Math.Max(tBShiftGreenBar.Value, tBShiftBlueBar.Value));
                reg = Color.FromArgb(tBShiftRedBar.Value, tBShiftGreenBar.Value, tBShiftBlueBar.Value);
                full = HuetoRGB(reg.GetHue(), reg.GetBrightness(), reg);
                pBShiftController.BackColor = Color.FromArgb((alphacolor > 205 ? 255 : (alphacolor + 50)), full);
            }
            ShiftColorOn[device]= cBShiftLight.Checked;
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
                LaunchProgram[device] = openFileDialog1.FileName;
                pBProgram.Image = Icon.ExtractAssociatedIcon(openFileDialog1.FileName).ToBitmap();
                btnBrowse.Text = Path.GetFileNameWithoutExtension(openFileDialog1.FileName);
            }
        }

        private void cBLaunchProgram_CheckedChanged(object sender, EventArgs e)
        {
            if (!cBLaunchProgram.Checked)
            {
                LaunchProgram[device] = string.Empty;
                pBProgram.Image = null;
                btnBrowse.Text = Properties.Resources.Browse;
            }
        }

        private void cBDinput_CheckedChanged(object sender, EventArgs e)
        {
            DinputOnly[device] = cBDinput.Checked;
            if (device < 4)
            {
                root.btnStartStop_Clicked(false);
                root.btnStartStop_Clicked(false);
            }
        }

        private void cbStartTouchpadOff_CheckedChanged(object sender, EventArgs e)
        {
            StartTouchpadOff[device] = cbStartTouchpadOff.Checked;
        }
        
        private void Items_MouseHover(object sender, EventArgs e)
        {
            switch (((System.Windows.Forms.Control)sender).Name)
            {
                case "cBlowerRCOn": root.lbLastMessage.Text = Properties.Resources.BestUsedRightSide; break;
                case "cBDoubleTap": root.lbLastMessage.Text = Properties.Resources.TapAndHold; break;
                case "lbControlTip": root.lbLastMessage.Text = Properties.Resources.UseControllerForMapping; break;
                case "cBTouchpadJitterCompensation": root.lbLastMessage.Text = Properties.Resources.Jitter; break;
                case "pBRainbow": root.lbLastMessage.Text = Properties.Resources.AlwaysRainbow; break;
                case "cBFlushHIDQueue": root.lbLastMessage.Text = Properties.Resources.FlushHIDTip; break;
                case "cBLightbyBattery": root.lbLastMessage.Text = Properties.Resources.LightByBatteryTip; break;
                case "lbGryo": root.lbLastMessage.Text = Properties.Resources.GyroReadout; break;
                case "tBsixaxisGyroX": root.lbLastMessage.Text = Properties.Resources.GyroX; break;
                case "tBsixaxisGyroY": root.lbLastMessage.Text = Properties.Resources.GyroY; break;
                case "tBsixaxisGyroZ": root.lbLastMessage.Text = Properties.Resources.GyroZ; break;
                case "tBsixaxisAccelX": root.lbLastMessage.Text = "AccelX"; break;
                case "tBsixaxisAccelY": root.lbLastMessage.Text = "AccelY"; break;
                case "tBsixaxisAccelZ": root.lbLastMessage.Text = "AccelZ"; break;
                case "lbEmpty": root.lbLastMessage.Text = Properties.Resources.CopyFullColor; break;
                case "lbShift": root.lbLastMessage.Text = Properties.Resources.CopyFullColor; break;
                case "lbSATip": root.lbLastMessage.Text = Properties.Resources.SixAxisReading; break;
                case "cBDinput": root.lbLastMessage.Text = Properties.Resources.DinputOnly; break;
                case "btnFlashColor": root.lbLastMessage.Text = Properties.Resources.FlashAtTip; break;
                case "cbStartTouchpadOff": root.lbLastMessage.Text = Properties.Resources.TouchpadOffTip; break;
                case "cBTPforControls": root.lbLastMessage.Text = Properties.Resources.UsingTPSwipes; break;
                default: root.lbLastMessage.Text = Properties.Resources.HoverOverItems; break;

                case "bnUp": root.lbLastMessage.Text = Properties.Resources.RightClickPresets; break;
                case "bnLeft": root.lbLastMessage.Text = Properties.Resources.RightClickPresets; break;
                case "bnRight": root.lbLastMessage.Text = Properties.Resources.RightClickPresets; break;
                case "bnDown": root.lbLastMessage.Text = Properties.Resources.RightClickPresets; break;
                case "btnLeftStick": root.lbLastMessage.Text = Properties.Resources.RightClickPresets; break;
                case "btnRightStick": root.lbLastMessage.Text = Properties.Resources.RightClickPresets; break;                        
                case "bnCross": root.lbLastMessage.Text = Properties.Resources.RightClickPresets; break;
                case "bnCircle": root.lbLastMessage.Text = Properties.Resources.RightClickPresets; break;
                case "bnSquare": root.lbLastMessage.Text = Properties.Resources.RightClickPresets; break;
                case "bnTriangle": root.lbLastMessage.Text = Properties.Resources.RightClickPresets; break;
                case "lbGyro": root.lbLastMessage.Text = Properties.Resources.RightClickPresets; break;
                case "bnGyroZN": root.lbLastMessage.Text = Properties.Resources.RightClickPresets; break;
                case "bnGyroZP": root.lbLastMessage.Text = Properties.Resources.RightClickPresets; break;
                case "bnGyroXN": root.lbLastMessage.Text = Properties.Resources.RightClickPresets; break;
                case "bnGyroXP": root.lbLastMessage.Text = Properties.Resources.RightClickPresets; break;
                case "lbTPSwipes": root.lbLastMessage.Text = Properties.Resources.RightClickPresets; break;
                case "bnSwipeUp": root.lbLastMessage.Text = Properties.Resources.RightClickPresets; break;
                case "bnSwipeLeft": root.lbLastMessage.Text = Properties.Resources.RightClickPresets; break;
                case "bnSwipeRight": root.lbLastMessage.Text = Properties.Resources.RightClickPresets; break;
                case "bnSwipeDown": root.lbLastMessage.Text = Properties.Resources.RightClickPresets; break;

                case "bnShiftUp": root.lbLastMessage.Text = Properties.Resources.RightClickPresets; break;
                case "bnShiftLeft": root.lbLastMessage.Text = Properties.Resources.RightClickPresets; break;
                case "bnShiftRight": root.lbLastMessage.Text = Properties.Resources.RightClickPresets; break;
                case "bnShiftDown": root.lbLastMessage.Text = Properties.Resources.RightClickPresets; break;
                case "btnShiftLeftStick": root.lbLastMessage.Text = Properties.Resources.RightClickPresets; break;
                case "btnShiftRightStick": root.lbLastMessage.Text = Properties.Resources.RightClickPresets; break;
                case "bnShiftCross": root.lbLastMessage.Text = Properties.Resources.RightClickPresets; break;
                case "bnShiftCircle": root.lbLastMessage.Text = Properties.Resources.RightClickPresets; break;
                case "bnShiftSquare": root.lbLastMessage.Text = Properties.Resources.RightClickPresets; break;
                case "bnShiftTriangle": root.lbLastMessage.Text = Properties.Resources.RightClickPresets; break;
                case "lbShiftGyro": root.lbLastMessage.Text = Properties.Resources.RightClickPresets; break;
                case "bnShiftGyroZN": root.lbLastMessage.Text = Properties.Resources.RightClickPresets; break;
                case "bnShiftGyroZP": root.lbLastMessage.Text = Properties.Resources.RightClickPresets; break;
                case "bnShiftGyroXN": root.lbLastMessage.Text = Properties.Resources.RightClickPresets; break;
                case "bnShiftGyroXP": root.lbLastMessage.Text = Properties.Resources.RightClickPresets; break;
                case "lbShiftTPSwipes": root.lbLastMessage.Text = Properties.Resources.RightClickPresets; break;
                case "bnShiftSwipeUp": root.lbLastMessage.Text = Properties.Resources.RightClickPresets; break;
                case "bnShiftSwipeLeft": root.lbLastMessage.Text = Properties.Resources.RightClickPresets; break;
                case "bnShiftSwipeRight": root.lbLastMessage.Text = Properties.Resources.RightClickPresets; break;
                case "bnShiftSwipeDown": root.lbLastMessage.Text = Properties.Resources.RightClickPresets; break;
            }
            if (root.lbLastMessage.Text != Properties.Resources.HoverOverItems)
                root.lbLastMessage.ForeColor = Color.Black;
            else
                root.lbLastMessage.ForeColor = SystemColors.GrayText;
        }

        private void cBTPforControls_CheckedChanged(object sender, EventArgs e)
        {
            UseTPforControls[device] = cBTPforControls.Checked;
            gBTouchpad.Enabled = !cBTPforControls.Checked;
            if (cBTPforControls.Checked)
            {
                tabControls.Size = new Size(tabControls.Size.Width, (int)(282 * dpiy));
                lBControls.Items.Add(Properties.Resources.SwipeUp + " : " + UpdateRegButtonList(bnSwipeUp));
                lBControls.Items.Add(Properties.Resources.SwipeDown + " : " + UpdateRegButtonList(bnSwipeDown));
                lBControls.Items.Add(Properties.Resources.SwipeLeft + " : " + UpdateRegButtonList(bnSwipeLeft));
                lBControls.Items.Add(Properties.Resources.SwipeRight + " : " + UpdateRegButtonList(bnSwipeRight));
                bnSwipeUp.Text = Properties.Resources.SwipeUp;
                bnSwipeDown.Text = Properties.Resources.SwipeDown;
                bnSwipeLeft.Text = Properties.Resources.SwipeLeft;
                bnSwipeRight.Text = Properties.Resources.SwipeRight;
                lBShiftControls.Items.Add(Properties.Resources.SwipeUp + " : " + UpdateRegButtonList(bnShiftSwipeUp));
                lBShiftControls.Items.Add(Properties.Resources.SwipeDown + " : " + UpdateRegButtonList(bnShiftSwipeDown));
                lBShiftControls.Items.Add(Properties.Resources.SwipeLeft + " : " + UpdateRegButtonList(bnShiftSwipeLeft));
                lBShiftControls.Items.Add(Properties.Resources.SwipeRight + " : " + UpdateRegButtonList(bnShiftSwipeRight));
                bnShiftSwipeUp.Text = Properties.Resources.SwipeUp;
                bnShiftSwipeDown.Text = Properties.Resources.SwipeDown;
                bnShiftSwipeLeft.Text = Properties.Resources.SwipeLeft;
                bnShiftSwipeRight.Text = Properties.Resources.SwipeRight;
            }
            else
            {
                tabControls.Size = new Size(tabControls.Size.Width, (int)(242 * dpiy));
                lBControls.Items.RemoveAt(36);
                lBControls.Items.RemoveAt(35);
                lBControls.Items.RemoveAt(34);
                lBControls.Items.RemoveAt(33);
                lBShiftControls.Items.RemoveAt(36);
                lBShiftControls.Items.RemoveAt(35);
                lBShiftControls.Items.RemoveAt(34);
                lBShiftControls.Items.RemoveAt(33);
            }
        }

        private void cBControllerInput_CheckedChanged(object sender, EventArgs e)
        {
            DS4Mapping=cBControllerInput.Checked;
        }

        private void btnAddAction_Click(object sender, EventArgs e)
        {
            SpecActions sA = new SpecActions(this);
            sA.TopLevel = false;
            sA.Dock = DockStyle.Fill;
            sA.Visible = true;
            tPSpecial.Controls.Add(sA);
            sA.BringToFront();
        }

        private void btnEditAction_Click(object sender, EventArgs e)
        {
            if (lVActions.SelectedIndices.Count > 0 && lVActions.SelectedIndices[0] >= 0)
            {
                SpecActions sA = new SpecActions(this, lVActions.SelectedItems[0].Text, lVActions.SelectedIndices[0]);
                sA.TopLevel = false;
                sA.Dock = DockStyle.Fill;
                sA.Visible = true;
                tPSpecial.Controls.Add(sA);
                sA.BringToFront();
            }
        }

        private void btnRemAction_Click(object sender, EventArgs e)
        {
            if (lVActions.SelectedIndices.Count > 0 && lVActions.SelectedIndices[0] >= 0)
            {
                RemoveAction(lVActions.SelectedItems[0].Text);
                lVActions.Items.Remove(lVActions.SelectedItems[0]);
            }
        }

        private void lVActions_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            List<string> pactions = new List<string>();
            foreach (ListViewItem lvi in lVActions.Items)
                if (lvi.Checked)
                    pactions.Add(lvi.Text);
            ProfileActions[device] = pactions;
            if (lVActions.Items.Count >= 50)
            {
                btnNewAction.Enabled = false;
            }
        }

        private void nUDLSCurve_ValueChanged(object sender, EventArgs e)
        {
            LSCurve[device] = (int)Math.Round(nUDLSCurve.Value, 0);
        }

        private void nUDRSCurve_ValueChanged(object sender, EventArgs e)
        {
            RSCurve[device] = (int)Math.Round(nUDRSCurve.Value, 0);
        }
        
        private void cMSPresets_Opened(object sender, EventArgs e)
        {
            string name = cMSPresets.SourceControl.Name;
            if (name == "bnUp" || name == "bnLeft" || name == "bnRight" || name == "bnDown")
                controlToolStripMenuItem.Text = "Dpad";
            else if (name == "btnLeftStick")
                controlToolStripMenuItem.Text = "Left Stick";
            else if (name == "btnRightStick")
                controlToolStripMenuItem.Text = "Right Stick";
            else if (name == "bnCross" || name == "bnCircle" || name == "bnSquare" || name == "bnTriangle")
                controlToolStripMenuItem.Text = "Face Buttons";
            else if (name == "lbGyro" || name.StartsWith("bnGyro"))
                controlToolStripMenuItem.Text = "Sixaxis";
            else if (name == "lbTPSwipes" || name.StartsWith("bnSwipe"))
                controlToolStripMenuItem.Text = "Touchpad Swipes";
            else if (name == "bnShiftUp" || name == "bnShiftLeft" || name == "bnShiftRight" || name == "bnShiftDown")
                controlToolStripMenuItem.Text = "Dpad (Shift)";
            else if (name == "btnShiftLeftStick")
                controlToolStripMenuItem.Text = "Left Stick (Shift)";
            else if (name == "btnShiftRightStick")
                controlToolStripMenuItem.Text = "Right Stick (Shift)";
            else if (name == "bnShiftCross" || name == "bnShiftCircle" || name == "bnShiftSquare" || name == "bnShiftTriangle")
                controlToolStripMenuItem.Text = "Face Buttons (Shift)";
            else if (name == "lbShiftGyro" || name.StartsWith("bnShiftGyro"))
                controlToolStripMenuItem.Text = "Sixaxis (Shift)";
            else if (name == "lbShiftTPSwipes" || name.StartsWith("bnShiftSwipe"))
                controlToolStripMenuItem.Text = "Touchpad Swipes (Shift)";
            else
                controlToolStripMenuItem.Text = "Select another control";
            MouseToolStripMenuItem.Visible = !(name == "lbTPSwipes" || name.StartsWith("bnSwipe") || name == "lbShiftTPSwipes" || name.StartsWith("bnShiftSwipe"));
        }

        private void BatchToggle_Bn(bool scancode, Button button1, Button button2, Button button3, Button button4)
        {
            Toggle_Bn(scancode, false, false, false, button1);
            Toggle_Bn(scancode, false, false, false, button2);
            Toggle_Bn(scancode, false, false, false, button3);
            Toggle_Bn(scancode, false, false, false, button4);
        }


        private void SetPreset(object sender, EventArgs e)
        {
            bool scancode = false;
            KeyValuePair<object, string> tagU;
            KeyValuePair<object, string> tagL;
            KeyValuePair<object, string> tagR;
            KeyValuePair<object, string> tagD;
            string name = ((ToolStripMenuItem)sender).Name;
            if (name.Contains("Dpad") || name.Contains("DPad"))
            {
                if (name.Contains("InvertedX"))
                {
                    tagU = new KeyValuePair<object, string>("Up Button", "0,0,0,0,0,0,0,0");
                    tagL = new KeyValuePair<object, string>("Right Button", "0,0,0,0,0,0,0,0");
                    tagR = new KeyValuePair<object, string>("Left Button", "0,0,0,0,0,0,0,0");
                    tagD = new KeyValuePair<object, string>("Down Button", "0,0,0,0,0,0,0,0");
                }
                else if (name.Contains("InvertedY"))
                {
                    tagU = new KeyValuePair<object, string>("Down Button", "0,0,0,0,0,0,0,0");
                    tagL = new KeyValuePair<object, string>("Left Button", "0,0,0,0,0,0,0,0");
                    tagR = new KeyValuePair<object, string>("Right Button", "0,0,0,0,0,0,0,0");
                    tagD = new KeyValuePair<object, string>("Up Button", "0,0,0,0,0,0,0,0");
                }
                else if (name.Contains("Inverted"))
                {
                    tagU = new KeyValuePair<object, string>("Down Button", "0,0,0,0,0,0,0,0");
                    tagL = new KeyValuePair<object, string>("Right Button", "0,0,0,0,0,0,0,0");
                    tagR = new KeyValuePair<object, string>("Left Button", "0,0,0,0,0,0,0,0");
                    tagD = new KeyValuePair<object, string>("Up Button", "0,0,0,0,0,0,0,0");
                }
                else
                {
                    tagU = new KeyValuePair<object, string>("Up Button", "0,0,0,0,0,0,0,0");
                    tagL = new KeyValuePair<object, string>("Left Button", "0,0,0,0,0,0,0,0");
                    tagR = new KeyValuePair<object, string>("Right Button", "0,0,0,0,0,0,0,0");
                    tagD = new KeyValuePair<object, string>("Down Button", "0,0,0,0,0,0,0,0");
                }
            }
            else if (name.Contains("LS"))
            {

                if (name.Contains("InvertedX"))
                {
                    tagU = new KeyValuePair<object, string>("Left Y-Axis-", "0,0,0,0,0,0,0,0");
                    tagL = new KeyValuePair<object, string>("Left X-Axis+", "0,0,0,0,0,0,0,0");
                    tagR = new KeyValuePair<object, string>("Left X-Axis-", "0,0,0,0,0,0,0,0");
                    tagD = new KeyValuePair<object, string>("Left Y-Axis+", "0,0,0,0,0,0,0,0");
                }
                else if (name.Contains("InvertedY"))
                {
                    tagU = new KeyValuePair<object, string>("Left Y-Axis+", "0,0,0,0,0,0,0,0");
                    tagL = new KeyValuePair<object, string>("Left X-Axis-", "0,0,0,0,0,0,0,0");
                    tagR = new KeyValuePair<object, string>("Left X-Axis+", "0,0,0,0,0,0,0,0");
                    tagD = new KeyValuePair<object, string>("Left Y-Axis-", "0,0,0,0,0,0,0,0");
                }
                else if (name.Contains("Inverted"))
                {
                    tagU = new KeyValuePair<object, string>("Left Y-Axis+", "0,0,0,0,0,0,0,0");
                    tagL = new KeyValuePair<object, string>("Left X-Axis+", "0,0,0,0,0,0,0,0");
                    tagR = new KeyValuePair<object, string>("Left X-Axis-", "0,0,0,0,0,0,0,0");
                    tagD = new KeyValuePair<object, string>("Left Y-Axis-", "0,0,0,0,0,0,0,0");
                }
                else
                {
                    tagU = new KeyValuePair<object, string>("Left Y-Axis-", "0,0,0,0,0,0,0,0");
                    tagL = new KeyValuePair<object, string>("Left X-Axis-", "0,0,0,0,0,0,0,0");
                    tagR = new KeyValuePair<object, string>("Left X-Axis+", "0,0,0,0,0,0,0,0");
                    tagD = new KeyValuePair<object, string>("Left Y-Axis+", "0,0,0,0,0,0,0,0");
                }
            }
            else if (name.Contains("RS"))
            {
                if (name.Contains("InvertedX"))
                {
                    tagU = new KeyValuePair<object, string>("Right Y-Axis-", "0,0,0,0,0,0,0,0");
                    tagL = new KeyValuePair<object, string>("Right X-Axis+", "0,0,0,0,0,0,0,0");
                    tagR = new KeyValuePair<object, string>("Right X-Axis-", "0,0,0,0,0,0,0,0");
                    tagD = new KeyValuePair<object, string>("Right Y-Axis+", "0,0,0,0,0,0,0,0");
                }
                else if (name.Contains("InvertedY"))
                {
                    tagU = new KeyValuePair<object, string>("Right Y-Axis+", "0,0,0,0,0,0,0,0");
                    tagL = new KeyValuePair<object, string>("Right X-Axis-", "0,0,0,0,0,0,0,0");
                    tagR = new KeyValuePair<object, string>("Right X-Axis+", "0,0,0,0,0,0,0,0");
                    tagD = new KeyValuePair<object, string>("Right Y-Axis-", "0,0,0,0,0,0,0,0");
                }
                else if (name.Contains("Inverted"))
                {
                    tagU = new KeyValuePair<object, string>("Right Y-Axis+", "0,0,0,0,0,0,0,0");
                    tagL = new KeyValuePair<object, string>("Right X-Axis+", "0,0,0,0,0,0,0,0");
                    tagR = new KeyValuePair<object, string>("Right X-Axis-", "0,0,0,0,0,0,0,0");
                    tagD = new KeyValuePair<object, string>("Right Y-Axis-", "0,0,0,0,0,0,0,0");
                }
                else
                {
                    tagU = new KeyValuePair<object, string>("Right Y-Axis-", "0,0,0,0,0,0,0,0");
                    tagL = new KeyValuePair<object, string>("Right X-Axis-", "0,0,0,0,0,0,0,0");
                    tagR = new KeyValuePair<object, string>("Right X-Axis+", "0,0,0,0,0,0,0,0");
                    tagD = new KeyValuePair<object, string>("Right Y-Axis+", "0,0,0,0,0,0,0,0");
                }
            }
            else if (name.Contains("ABXY"))
            {
                tagU = new KeyValuePair<object, string>("Y Button", "0,0,0,0,0,0,0,0");
                tagL = new KeyValuePair<object, string>("X Button", "0,0,0,0,0,0,0,0");
                tagR = new KeyValuePair<object, string>("B Button", "0,0,0,0,0,0,0,0");
                tagD = new KeyValuePair<object, string>("A Button", "0,0,0,0,0,0,0,0");
            }
            else if (name.Contains("WASD"))
            {
                if (name.Contains("ScanCode"))
                    scancode = true;
                tagU = new KeyValuePair<object, string>((int)Keys.W, "0,0,0,0,0,0,0,0");
                tagL = new KeyValuePair<object, string>((int)Keys.A, "0,0,0,0,0,0,0,0");
                tagR = new KeyValuePair<object, string>((int)Keys.D, "0,0,0,0,0,0,0,0");
                tagD = new KeyValuePair<object, string>((int)Keys.S, "0,0,0,0,0,0,0,0");
            }
            else if (name.Contains("ArrowKeys"))
            {
                if (name.Contains("ScanCode"))
                    scancode = true;
                tagU = new KeyValuePair<object, string>((int)Keys.Up, "0,0,0,0,0,0,0,0");
                tagL = new KeyValuePair<object, string>((int)Keys.Left, "0,0,0,0,0,0,0,0");
                tagR = new KeyValuePair<object, string>((int)Keys.Right, "0,0,0,0,0,0,0,0");
                tagD = new KeyValuePair<object, string>((int)Keys.Down, "0,0,0,0,0,0,0,0");
            }
            else if (name.Contains("Mouse"))
            {

                if (name.Contains("InvertedX"))
                {
                    tagU = new KeyValuePair<object, string>("Mouse Up", "0,0,0,0,0,0,0,0");
                    tagL = new KeyValuePair<object, string>("Mouse Right", "0,0,0,0,0,0,0,0");
                    tagR = new KeyValuePair<object, string>("Mouse Left", "0,0,0,0,0,0,0,0");
                    tagD = new KeyValuePair<object, string>("Mouse Down", "0,0,0,0,0,0,0,0");
                }
                else if (name.Contains("InvertedY"))
                {
                    tagU = new KeyValuePair<object, string>("Mouse Down", "0,0,0,0,0,0,0,0");
                    tagL = new KeyValuePair<object, string>("Mouse Left", "0,0,0,0,0,0,0,0");
                    tagR = new KeyValuePair<object, string>("Mouse Right", "0,0,0,0,0,0,0,0");
                    tagD = new KeyValuePair<object, string>("Mouse Up", "0,0,0,0,0,0,0,0");
                }
                else if (name.Contains("Inverted"))
                {
                    tagU = new KeyValuePair<object, string>("Mouse Down", "0,0,0,0,0,0,0,0");
                    tagL = new KeyValuePair<object, string>("Mouse Right", "0,0,0,0,0,0,0,0");
                    tagR = new KeyValuePair<object, string>("Mouse Left", "0,0,0,0,0,0,0,0");
                    tagD = new KeyValuePair<object, string>("Mouse Up", "0,0,0,0,0,0,0,0");
                }
                else
                {
                    tagU = new KeyValuePair<object, string>("Mouse Up", "0,0,0,0,0,0,0,0");
                    tagL = new KeyValuePair<object, string>("Mouse Left", "0,0,0,0,0,0,0,0");
                    tagR = new KeyValuePair<object, string>("Mouse Right", "0,0,0,0,0,0,0,0");
                    tagD = new KeyValuePair<object, string>("Mouse Down", "0,0,0,0,0,0,0,0");
                }
            }
            else //default
            {
                tagU = new KeyValuePair<object, string>(null, "0,0,0,0,0,0,0,0");
                tagL = new KeyValuePair<object, string>(null, "0,0,0,0,0,0,0,0");
                tagR = new KeyValuePair<object, string>(null, "0,0,0,0,0,0,0,0");
                tagD = new KeyValuePair<object, string>(null, "0,0,0,0,0,0,0,0");
            }

            Button button1, button2, button3, button4;
            if (controlToolStripMenuItem.Text == "Dpad")
            {
                button1 = bnUp;
                button2 = bnLeft;
                button3 = bnRight;
                button4 = bnDown;
            }
            else if (controlToolStripMenuItem.Text == "Left Stick")
            {
                button1 = bnLSUp;
                button2 = bnLSLeft;
                button3 = bnLSRight;
                button4 = bnLSDown;
            }
            else if (controlToolStripMenuItem.Text == "Right Stick")
            {
                button1 = bnRSUp;
                button2 = bnRSLeft;
                button3 = bnRSRight;
                button4 = bnRSDown;
            }
            else if (controlToolStripMenuItem.Text == "Face Buttons")
            {
                button1 = bnTriangle;
                button2 = bnSquare;
                button3 = bnCircle;
                button4 = bnCross;
            }
            else if (controlToolStripMenuItem.Text == "Sixaxis")
            {
                button1 = bnGyroZN;
                button2 = bnGyroXP;
                button3 = bnGyroXN;
                button4 = bnGyroZP;
            }
            else if (controlToolStripMenuItem.Text == "Touchpad Swipes")
            {
                button1 = bnSwipeUp;
                button2 = bnSwipeLeft;
                button3 = bnSwipeRight;
                button4 = bnSwipeDown;
            }
            else if (controlToolStripMenuItem.Text == "Dpad (Shift)")
            {
                button1 = bnShiftUp;
                button2 = bnShiftLeft;
                button3 = bnShiftRight;
                button4 = bnShiftDown;
            }
            else if (controlToolStripMenuItem.Text == "Left Stick (Shift)")
            {
                button1 = bnShiftLSUp;
                button2 = bnShiftLSLeft;
                button3 = bnShiftLSRight;
                button4 = bnShiftLSDown;
            }
            else if (controlToolStripMenuItem.Text == "Right Stick (Shift)")
            {
                button1 = bnShiftRSUp;
                button2 = bnShiftRSLeft;
                button3 = bnShiftRSRight;
                button4 = bnShiftRSDown;
            }
            else if (controlToolStripMenuItem.Text == "Face Buttons (Shift)")
            {
                button1 = bnShiftTriangle;
                button2 = bnShiftSquare;
                button3 = bnShiftCircle;
                button4 = bnShiftCross;
            }
            else if (controlToolStripMenuItem.Text == "Sixaxis (Shift)")
            {
                button1 = bnShiftGyroZN;
                button2 = bnShiftGyroXP;
                button3 = bnShiftGyroXN;
                button4 = bnShiftGyroZP;
            }
            else if (controlToolStripMenuItem.Text == "Touchpad Swipes (Shift)")
            {
                button1 = bnShiftSwipeUp;
                button2 = bnShiftSwipeLeft;
                button3 = bnShiftSwipeRight;
                button4 = bnShiftSwipeDown;
            }
            else
                button1 = button2 = button3 = button4 = null;
            ChangeButtonText("Up Button", tagU, button1);
            ChangeButtonText("Left Button", tagL, button2);
            ChangeButtonText("Right Button", tagR, button3);
            ChangeButtonText("Down Button", tagD, button4);
            BatchToggle_Bn(scancode, button1, button2, button3, button4);

            UpdateLists();
            cMSPresets.Hide();
        }

        private void cBWhileCharging_SelectedIndexChanged(object sender, EventArgs e)
        {
            ChargingType[device] = cBWhileCharging.SelectedIndex;
            btnChargingColor.Visible = cBWhileCharging.SelectedIndex == 3;
        }

        private void btnFlashColor_Click(object sender, EventArgs e)
        {
            if (btnFlashColor.BackColor != pBController.BackColor)
                advColorDialog.Color = btnFlashColor.BackColor;
            else
                advColorDialog.Color = Color.Black;
            advColorDialog_OnUpdateColor(lbPercentFlashBar.ForeColor, e);
            if (advColorDialog.ShowDialog() == DialogResult.OK)
            {
                if (advColorDialog.Color.GetBrightness() > 0)
                    btnFlashColor.BackColor = advColorDialog.Color;
                else
                    btnFlashColor.BackColor = pBController.BackColor;
                FlashColor[device] = new DS4Color(advColorDialog.Color);
            }
            if (device < 4)
                DS4LightBar.forcelight[device] = false;
        }

        private void pBController_BackColorChanged(object sender, EventArgs e)
        {
            if (FlashColor[device].Equals(new DS4Color { red = 0, green = 0, blue = 0 }))
            {
                btnFlashColor.BackColor = pBController.BackColor;
            }
        }

        private void pBController_BackgroundImageChanged(object sender, EventArgs e)
        {
            btnFlashColor.BackgroundImage = pBController.BackgroundImage;
        }

        private void cBFlashType_SelectedIndexChanged(object sender, EventArgs e)
        {
            FlashType[device]= (byte)cBFlashType.SelectedIndex;
        }

        private void nUDRainbowB_ValueChanged(object sender, EventArgs e)
        {
            //
        }
    }
}
