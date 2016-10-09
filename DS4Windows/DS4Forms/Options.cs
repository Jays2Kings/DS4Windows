using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml;
using static DS4Windows.Global;
using System.Runtime.InteropServices;

namespace DS4Windows
{
    public partial class Options : Form
    {
        public int device;
        public string filename;
        public Timer inputtimer = new Timer(), sixaxisTimer = new Timer();
        public List<Button> buttons = new List<Button>();
        private int alphacolor;
        private Color reg, full, main;
        private Image colored, greyscale;
        private Image rainbowImg = Properties.Resources.rainbow;
        ToolTip tp = new ToolTip();
        public DS4Form root;
        bool olddinputcheck = false;
        private float dpix;
        private float dpiy;
        public Dictionary<string, string> defaults = new Dictionary<string, string>();
        public bool saving, loading;
        public static Size mSize { get; private set; }
        private Size settingsSize;
        public Options(DS4Form rt)
        {
            InitializeComponent();
            mSize = MaximumSize;
            settingsSize = fLPSettings.Size;
            MaximumSize = new Size(0, 0);
            root = rt;
            btnRumbleHeavyTest.Text = Properties.Resources.TestHText;
            btnRumbleLightTest.Text = Properties.Resources.TestLText;
            rBTPControls.Text = rBSAControls.Text;
            rBTPMouse.Text = rBSAMouse.Text;
            rBTPControls.Location = rBSAControls.Location;
            rBTPMouse.Location = rBSAMouse.Location;
            Visible = false;
            colored = btnRainbow.Image;
            greyscale = GreyscaleImage((Bitmap)btnRainbow.Image);
            fLPSettings.FlowDirection = FlowDirection.TopDown;
            foreach (Control control in tPControls.Controls)
                if (control is Button && !((Button)control).Name.Contains("btn"))
                    buttons.Add((Button)control);
            foreach (Control control in fLPTouchSwipe.Controls)
                if (control is Button && !((Button)control).Name.Contains("btn"))
                    buttons.Add((Button)control);
            foreach (Control control in fLPTiltControls.Controls)
                if (control is Button && !((Button)control).Name.Contains("btn"))
                    buttons.Add((Button)control);
            foreach (Control control in pnlController.Controls)
                if (control is Button && !((Button)control).Name.Contains("btn"))
                    buttons.Add((Button)control);
            foreach (Button b in buttons)
            {
                defaults.Add(b.Name, b.Text);
                b.Text = "";
            }
            
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
            
            foreach (Button b in buttons)
            {
                b.MouseHover += button_MouseHover;
                b.MouseLeave += button_MouseLeave;
            }
            advColorDialog.OnUpdateColor += advColorDialog_OnUpdateColor;
            inputtimer.Tick += InputDS4;
            sixaxisTimer.Tick += ControllerReadout_Tick;
            sixaxisTimer.Interval = 1000 / 60;
            bnGyroZN.Text = Properties.Resources.TiltUp;
            bnGyroZP.Text = Properties.Resources.TiltDown;
            bnGyroXP.Text = Properties.Resources.TiltLeft;
            bnGyroXN.Text = Properties.Resources.TiltRight;
            bnSwipeUp.Text = Properties.Resources.SwipeUp;
            bnSwipeDown.Text = Properties.Resources.SwipeDown;
            bnSwipeLeft.Text = Properties.Resources.SwipeLeft;
            bnSwipeRight.Text = Properties.Resources.SwipeRight;
        }        

        public void Reload(int deviceNum, string name)
        {
            loading = true;
            device = deviceNum;
            filename = name;
            lBControls.SelectedIndex = -1;
            lbControlName.Text = "";

            tCControls.SelectedIndex = 0;
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

            //butts += "\n" + b.Name;
            //MessageBox.Show(butts);

            root.lbLastMessage.ForeColor = Color.Black;
            root.lbLastMessage.Text = "Hover over items to see description or more about";
            if (device < 4)
                nUDSixaxis.Value = deviceNum + 1;
            if (filename != "")
            {
                if (device == 4) //if temp device is called
                    ProfilePath[4] = name;
                LoadProfile(device, false, Program.rootHub);

                if (Rainbow[device] == 0)
                {
                    btnRainbow.Image = greyscale;
                    ToggleRainbow(false);
                }
                else
                {
                    btnRainbow.Image = colored;
                    ToggleRainbow(true);
                }
                DS4Color color = MainColor[device];
                tBRedBar.Value = color.red;
                tBGreenBar.Value = color.green;
                tBBlueBar.Value = color.blue;

                alphacolor = Math.Max(tBRedBar.Value, Math.Max(tBGreenBar.Value, tBBlueBar.Value));
                reg = Color.FromArgb(color.red, color.green, color.blue);
                full = HuetoRGB(reg.GetHue(), reg.GetBrightness(), reg);
                main = Color.FromArgb((alphacolor > 205 ? 255 : (alphacolor + 50)), full);
                btnLightbar.BackgroundImage = RecolorImage((Bitmap)btnLightbar.BackgroundImage, main);

                cBLightbyBattery.Checked = LedAsBatteryIndicator[device];
                nUDflashLED.Value = FlashAt[device];
                pnlLowBattery.Visible = cBLightbyBattery.Checked;
                lbFull.Text = (cBLightbyBattery.Checked ? "Full:" : "Color:");
                //pnlFull.Location = new Point(pnlFull.Location.X, (cBLightbyBattery.Checked ? (int)(dpix * 42) : (pnlFull.Location.Y + pnlLowBattery.Location.Y) / 2));
                DS4Color lowColor = LowColor[device];
                tBLowRedBar.Value = lowColor.red;
                tBLowGreenBar.Value = lowColor.green;
                tBLowBlueBar.Value = lowColor.blue;

                DS4Color cColor = ChargingColor[device];
                btnChargingColor.BackColor = Color.FromArgb(cColor.red, cColor.green, cColor.blue);
                if (FlashType[device] > cBFlashType.Items.Count - 1)
                    cBFlashType.SelectedIndex = 0;
                else
                    cBFlashType.SelectedIndex = FlashType[device];
                DS4Color fColor = FlashColor[device];
                if (fColor.Equals(new DS4Color { red = 0, green = 0, blue = 0 }))
                    if (Rainbow[device] == 0)
                        btnFlashColor.BackColor = main;
                    else
                        btnFlashColor.BackgroundImage = rainbowImg;
                else
                    btnFlashColor.BackColor = Color.FromArgb(fColor.red, fColor.green, fColor.blue);
                nUDRumbleBoost.Value = RumbleBoost[device];
                nUDTouch.Value = TouchSensitivity[device];
                cBSlide.Checked = TouchSensitivity[device] > 0;
                nUDScroll.Value = ScrollSensitivity[device];
                cBScroll.Checked = ScrollSensitivity[device] != 0;
                nUDTap.Value = TapSensitivity[device];
                cBTap.Checked = TapSensitivity[device] > 0;
                cBDoubleTap.Checked = DoubleTap[device];
                cBTouchpadJitterCompensation.Checked = TouchpadJitterCompensation[device];
                cBlowerRCOn.Checked = LowerRCOn[device];
                cBFlushHIDQueue.Checked = FlushHIDQueue[device];
                nUDIdleDisconnect.Value = Math.Round((decimal)(IdleDisconnectTimeout[device] / 60d), 1);
                cBIdleDisconnect.Checked = IdleDisconnectTimeout[device] > 0;
                numUDMouseSens.Value = ButtonMouseSensitivity[device];
                cBMouseAccel.Checked = MouseAccel[device];
                pBHoveredButton.Image = null;

                alphacolor = Math.Max(tBLowRedBar.Value, Math.Max(tBGreenBar.Value, tBBlueBar.Value));
                reg = Color.FromArgb(lowColor.red, lowColor.green, lowColor.blue);
                full = HuetoRGB(reg.GetHue(), reg.GetBrightness(), reg);
                lowColorChooserButton.BackColor = Color.FromArgb((alphacolor > 205 ? 255 : (alphacolor + 50)), full);
                nUDRainbow.Value = (decimal)Rainbow[device];
                if (ChargingType[device] > cBWhileCharging.Items.Count - 1)
                    cBWhileCharging.SelectedIndex = 0;
                else
                    cBWhileCharging.SelectedIndex = ChargingType[device];
                try
                {
                    nUDL2.Value = Math.Round((decimal)L2Deadzone[device] / 255, 2);
                }
                catch
                {
                    nUDL2.Value = 0;
                }
                try
                {
                    nUDR2.Value = Math.Round((decimal)R2Deadzone[device] / 255, 2);
                }
                catch
                {
                    nUDR2.Value = 0;
                }
                try
                {
                    nUDLS.Value = Math.Round((decimal)(LSDeadzone[device] / 127d), 3);
                }
                catch
                {
                    nUDLS.Value = 0;
                }
                try
                {
                    nUDRS.Value = Math.Round((decimal)(RSDeadzone[device] / 127d), 3);
                }
                catch
                {
                    nUDRS.Value = 0;
                }
                try
                {
                    nUDSX.Value = (decimal)SXDeadzone[device];
                }
                catch
                {
                    nUDSX.Value = 0.25m;
                }
                try
                {
                    nUDSZ.Value = (decimal)SZDeadzone[device];
                }
                catch
                {
                    nUDSZ.Value = 0.25m;
                }
                try
                {
                    nUDL2S.Value = Math.Round((decimal)L2Sens[device], 2);
                }
                catch
                {
                    nUDL2S.Value = 1;
                }
                try
                {
                    nUDR2S.Value = Math.Round((decimal)R2Sens[device], 2);
                }
                catch
                {
                    nUDR2S.Value = 1;
                }
                try
                {
                    nUDLSS.Value = Math.Round((decimal)LSSens[device], 2);
                }
                catch
                {
                    nUDLSS.Value = 1;
                }
                try
                {
                    nUDRSS.Value = Math.Round((decimal)RSSens[device], 2);
                }
                catch
                {
                    nUDRSS.Value = 1;
                }
                try
                {
                    nUDSXS.Value = Math.Round((decimal)SXSens[device], 2);
                }
                catch
                {
                    nUDSXS.Value = 1;
                }
                try
                {
                    nUDSZS.Value = Math.Round((decimal)SZSens[device], 2);
                }
                catch
                {
                    nUDSZS.Value = 1;
                }
                if (LaunchProgram[device] != string.Empty)
                {
                    cBLaunchProgram.Checked = true;
                    pBProgram.Image = Icon.ExtractAssociatedIcon(LaunchProgram[device]).ToBitmap();
                    btnBrowse.Text = Path.GetFileNameWithoutExtension(LaunchProgram[device]);
                }
                cBDinput.Checked = DinputOnly[device];
                olddinputcheck = cBDinput.Checked;
                cbStartTouchpadOff.Checked = StartTouchpadOff[device];
                rBTPControls.Checked = UseTPforControls[device];
                rBTPMouse.Checked = !UseTPforControls[device];
                rBSAMouse.Checked = UseSAforMouse[device];
                rBSAControls.Checked = !UseSAforMouse[device];
                nUDLSCurve.Value = LSCurve[device];
                nUDRSCurve.Value = RSCurve[device];
                cBControllerInput.Checked = DS4Mapping;

                string[] satriggers = SATriggers[device].Split(',');
                List<string> s = new List<string>();
                for (int i = 0; i < satriggers.Length; i++)
                {
                    int tr;
                    if (int.TryParse(satriggers[i], out tr))
                    {
                        if (tr < cMGyroTriggers.Items.Count && tr > -1)
                        {
                            ((ToolStripMenuItem)cMGyroTriggers.Items[tr]).Checked = true;
                            s.Add(cMGyroTriggers.Items[tr].Text);
                        }
                        else
                        {
                            ((ToolStripMenuItem)cMGyroTriggers.Items[cMGyroTriggers.Items.Count - 1]).Checked = true;
                            s.Add(cMGyroTriggers.Items[cMGyroTriggers.Items.Count - 1].Text);
                            break;
                        }
                    }
                    else
                    {
                        ((ToolStripMenuItem)cMGyroTriggers.Items[cMGyroTriggers.Items.Count - 1]).Checked = true;
                        s.Add(cMGyroTriggers.Items[cMGyroTriggers.Items.Count - 1].Text);
                        break;
                    }
                }
                nUDGyroSensitivity.Value = GyroSensitivity[device];
                int invert = GyroInvert[device];
                cBGyroInvertX.Checked = invert == 2 || invert == 3;
                cBGyroInvertY.Checked = invert == 1 || invert == 3;
                if (s.Count > 0)
                    btnGyroTriggers.Text = string.Join(", ", s);
            }
            else
            {
                cBFlashType.SelectedIndex = 0;
                cBWhileCharging.SelectedIndex = 0;
                rBTPMouse.Checked = true;
                rBSAControls.Checked = true;
                ToggleRainbow(false);
                cBDinput.Checked = false;
                cbStartTouchpadOff.Checked = false;
                rBSAControls.Checked = true;
                rBTPMouse.Checked = true;
                switch (device)
                {
                    case 0: tBRedBar.Value = 0; tBGreenBar.Value = 0; tBBlueBar.Value = 255; break;
                    case 1: tBRedBar.Value = 255; tBGreenBar.Value = 0; tBBlueBar.Value = 0; break;
                    case 2: tBRedBar.Value = 0; tBGreenBar.Value = 255; tBBlueBar.Value = 0; break;
                    case 3: tBRedBar.Value = 255; tBGreenBar.Value = 0; tBBlueBar.Value = 255; break;
                    case 4: tBRedBar.Value = 255; tBGreenBar.Value = 255; tBBlueBar.Value = 255; break;
                }
                tBLowBlueBar.Value = 0; tBLowGreenBar.Value = 0; tBLowBlueBar.Value = 0;

                cBLightbyBattery.Checked = false;
                nUDflashLED.Value = 0;
                lbFull.Text = (cBLightbyBattery.Checked ? "Full:" : "Color:");

                cBFlashType.SelectedIndex = 0;
                nUDRumbleBoost.Value = 100;
                nUDTouch.Value = 100;
                cBSlide.Checked = true;
                nUDScroll.Value = 0;
                cBScroll.Checked = false;
                nUDTap.Value = 0;
                cBTap.Checked = false;
                cBDoubleTap.Checked = false;
                cBTouchpadJitterCompensation.Checked = true;
                cBlowerRCOn.Checked = false;
                cBFlushHIDQueue.Checked = true;
                nUDIdleDisconnect.Value = 5;
                cBIdleDisconnect.Checked = true;
                numUDMouseSens.Value = 25;
                cBMouseAccel.Checked = true;
                pBHoveredButton.Image = null;

                nUDRainbow.Value = 0;
                nUDL2.Value = 0;
                nUDR2.Value = 0;
                nUDLS.Value = 0;
                nUDRS.Value = 0;
                nUDSX.Value = .25m;
                nUDSZ.Value = .25m;

                nUDL2S.Value = 1;
                nUDR2S.Value = 1;
                nUDLSS.Value = 1;
                nUDRSS.Value = 1;
                nUDSXS.Value = 1;
                nUDSZS.Value = 1;

                cBLaunchProgram.Checked = false;
                pBProgram.Image = null;
                btnBrowse.Text = Properties.Resources.Browse;
                cBDinput.Checked = false;
                olddinputcheck = false;
                cbStartTouchpadOff.Checked = false;
                nUDLSCurve.Value = 0;
                nUDRSCurve.Value = 0;
                cBControllerInput.Checked = DS4Mapping;
                ((ToolStripMenuItem)cMGyroTriggers.Items[cMGyroTriggers.Items.Count - 1]).Checked = true;
                nUDGyroSensitivity.Value = 100;
                cBGyroInvertX.Checked = false;
                cBGyroInvertY.Checked = false;
                Set();
            }
            
            UpdateLists();
            inputtimer.Start();
            LoadActions(string.IsNullOrEmpty(filename));
            loading = false;
            saving = false;
        }

        private string getDS4ControlsByName(DS4Controls key)
        {
            switch (key)
            {
                case DS4Controls.Share: return "bnShare";
                case DS4Controls.L3: return "bnL3";
                case DS4Controls.R3: return "bnR3";
                case DS4Controls.Options: return "bnOptions";
                case DS4Controls.DpadUp: return "bnUp";
                case DS4Controls.DpadRight: return "bnRight";
                case DS4Controls.DpadDown: return "bnDown";
                case DS4Controls.DpadLeft: return "bnLeft";

                case DS4Controls.L1: return "bnL1";
                case DS4Controls.R1: return "bnR1";
                case DS4Controls.Triangle: return "bnTriangle";
                case DS4Controls.Circle: return "bnCircle";
                case DS4Controls.Cross: return "bnCross";
                case DS4Controls.Square: return "bnSquare";

                case DS4Controls.PS: return "bnPS";
                case DS4Controls.LXNeg: return "bnLSLeft";
                case DS4Controls.LYNeg: return "bnLSUp";
                case DS4Controls.RXNeg: return "bnRSLeft";
                case DS4Controls.RYNeg: return "bnRSUp";

                case DS4Controls.LXPos: return "bnLSRight";
                case DS4Controls.LYPos: return "bnLSDown";
                case DS4Controls.RXPos: return "bnRSRight";
                case DS4Controls.RYPos: return "bnRSDown";
                case DS4Controls.L2: return "bnL2";
                case DS4Controls.R2: return "bnR2";

                case DS4Controls.TouchLeft: return "bnTouchLeft";
                case DS4Controls.TouchMulti: return "bnTouchMulti";
                case DS4Controls.TouchUpper: return "bnTouchUpper";
                case DS4Controls.TouchRight: return "bnTouchRight";
                case DS4Controls.GyroXPos: return "bnGyroXP";
                case DS4Controls.GyroXNeg: return "bnGyroXN";
                case DS4Controls.GyroZPos: return "bnGyroZP";
                case DS4Controls.GyroZNeg: return "bnGyroZN";

                case DS4Controls.SwipeUp: return "bnSwipeUp";
                case DS4Controls.SwipeDown: return "bnSwipeDown";
                case DS4Controls.SwipeLeft: return "bnSwipeLeft";
                case DS4Controls.SwipeRight: return "bnSwipeRight";
            }
            return "";
        }

        public void LoadActions(bool newp)
        {
            lVActions.Items.Clear();
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
                    case "MultiAction":
                        lvi.SubItems.Add(Properties.Resources.MultiAction);
                        break;
                }
                if (newp)
                    if (action.type == "DisconnectBT")
                        lvi.Checked = true;
                    else
                        lvi.Checked = false;
                else
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
        void EnableReadings(bool on)
        {
            lbL2Track.Enabled = on;
            lbR2Track.Enabled = on;
            pnlLSTrack.Enabled = on;
            pnlRSTrack.Enabled = on;
            pnlSATrack.Enabled = on;
            btnLSTrack.Visible = on;
            btnLSTrackS.Visible = on;
            btnRSTrack.Visible = on;
            btnRSTrackS.Visible = on;
            btnSATrack.Visible = on;
            btnSATrackS.Visible = on;
        }
        void ControllerReadout_Tick(object sender, EventArgs e)
        {
            // MEMS gyro data is all calibrated to roughly -1G..1G for values -0x2000..0x1fff
            // Enough additional acceleration and we are no longer mostly measuring Earth's gravity...
            // We should try to indicate setpoints of the calibration when exposing this measurement....
            if (Program.rootHub.DS4Controllers[(int)nUDSixaxis.Value - 1] == null)
            {
                EnableReadings(false);
                lbInputDelay.Text = Properties.Resources.InputDelay.Replace("*number*", Properties.Resources.NA);
                lbInputDelay.BackColor = Color.Transparent;
                lbInputDelay.ForeColor = Color.Black;
            }
            else
            {
                EnableReadings(true);
                SetDynamicTrackBarValue(tBsixaxisGyroX, (Program.rootHub.ExposedState[(int)nUDSixaxis.Value - 1].GyroX + tBsixaxisGyroX.Value * 2) / 3);
                SetDynamicTrackBarValue(tBsixaxisGyroY, (Program.rootHub.ExposedState[(int)nUDSixaxis.Value - 1].GyroY + tBsixaxisGyroY.Value * 2) / 3);
                SetDynamicTrackBarValue(tBsixaxisGyroZ, (Program.rootHub.ExposedState[(int)nUDSixaxis.Value - 1].GyroZ + tBsixaxisGyroZ.Value * 2) / 3);
                SetDynamicTrackBarValue(tBsixaxisAccelX, (int)(Program.rootHub.ExposedState[(int)nUDSixaxis.Value - 1].AccelX + tBsixaxisAccelX.Value * 2) / 3);
                SetDynamicTrackBarValue(tBsixaxisAccelY, (int)(Program.rootHub.ExposedState[(int)nUDSixaxis.Value - 1].AccelY + tBsixaxisAccelY.Value * 2) / 3);
                SetDynamicTrackBarValue(tBsixaxisAccelZ, (int)(Program.rootHub.ExposedState[(int)nUDSixaxis.Value - 1].AccelZ + tBsixaxisAccelZ.Value * 2) / 3);

                int x = Program.rootHub.getDS4State((int)nUDSixaxis.Value - 1).LX;
                int y = Program.rootHub.getDS4State((int)nUDSixaxis.Value - 1).LY;
                btnLSTrackS.Visible = nUDLSS.Value != 1;
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
                    }
                    btnLSTrack.Location = new Point((int)(dpix * curvex / 2.09), (int)(dpiy * curvey / 2.09));
                }
                else
                {
                    btnLSTrack.Location = new Point((int)(dpix * x / 2.09), (int)(dpiy * y / 2.09));
                    btnLSTrackS.Visible = nUDLSS.Value != 1;
                }
                if (nUDLSS.Value != 1)
                    btnLSTrackS.Location = new Point((int)((float)nUDLSS.Value * (btnLSTrack.Location.X - pnlLSTrack.Size.Width / 2f) + pnlLSTrack.Size.Width / 2f),
                        (int)((float)nUDLSS.Value * (btnLSTrack.Location.Y - pnlLSTrack.Size.Height / 2f) + pnlLSTrack.Size.Height / 2f));

                x = Program.rootHub.getDS4State((int)nUDSixaxis.Value - 1).RX;
                y = Program.rootHub.getDS4State((int)nUDSixaxis.Value - 1).RY;
                    btnRSTrackS.Visible = nUDRSS.Value != 1;
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
                    }
                    btnRSTrack.Location = new Point((int)(dpix * curvex / 2.09), (int)(dpiy * curvey / 2.09));
                }
                else
                {
                    btnRSTrack.Location = new Point((int)(dpix * x / 2.09), (int)(dpiy * y / 2.09));
                    btnRSTrackS.Visible = nUDRSS.Value != 1;
                }
                if (nUDRSS.Value != 1)
                    btnRSTrackS.Location = new Point((int)((float)nUDRSS.Value * (btnRSTrack.Location.X - pnlRSTrack.Size.Width / 2f) + pnlRSTrack.Size.Width / 2f),
                        (int)((float)nUDRSS.Value * (btnRSTrack.Location.Y - pnlRSTrack.Size.Height / 2f) + pnlRSTrack.Size.Height / 2f));

                x = -Program.rootHub.ExposedState[(int)nUDSixaxis.Value - 1].GyroX + 127;
                y = Program.rootHub.ExposedState[(int)nUDSixaxis.Value - 1].GyroZ + 127;
                btnSATrack.Location = new Point((int)(dpix * Clamp(0, x / 2.09, pnlSATrack.Size.Width)), (int)(dpiy * Clamp(0, y / 2.09, pnlSATrack.Size.Height)));
                btnSATrackS.Visible = nUDSXS.Value != 1 || nUDSZS.Value != 1;
                if (nUDSXS.Value != 1 || nUDSZS.Value != 1)
                    btnSATrackS.Location = new Point((int)((float)nUDSXS.Value * (btnSATrack.Location.X - pnlSATrack.Size.Width / 2f) + pnlSATrack.Size.Width / 2f),
                        (int)((float)nUDSZS.Value * (btnSATrack.Location.Y - pnlSATrack.Size.Height / 2f) + pnlSATrack.Size.Height / 2f));

                tBL2.Value = Program.rootHub.getDS4State((int)nUDSixaxis.Value - 1).L2;
                lbL2Track.Location = new Point(tBL2.Location.X - (int)(dpix * 25), 
                    Math.Max((int)(((tBL2.Location.Y + tBL2.Size.Height) - (tBL2.Value * (float)nUDL2S.Value) / (tBL2.Size.Height * .0209f / Math.Pow(dpix, 2))) - dpix * 20),
                    (int)(1 * ((tBL2.Location.Y + tBL2.Size.Height) - 255 / (tBL2.Size.Height * .0209f / Math.Pow(dpix, 2))) - dpix * 20)));
                if (tBL2.Value * (float)nUDL2S.Value >= 255)
                    lbL2Track.ForeColor = Color.Green;
                else if (tBL2.Value * (float)nUDL2S.Value < (double)nUDL2.Value * 255)
                    lbL2Track.ForeColor = Color.Red;
                else
                    lbL2Track.ForeColor = Color.Black;

                tBR2.Value = Program.rootHub.getDS4State((int)nUDSixaxis.Value - 1).R2;
                lbR2Track.Location = new Point(tBR2.Location.X + (int)(dpix * 25),
                     Math.Max((int)(1 * ((tBR2.Location.Y + tBR2.Size.Height) - (tBR2.Value * (float)nUDR2S.Value) / (tBR2.Size.Height * .0209f / Math.Pow(dpix, 2))) - dpix * 20),
                     (int)(1 * ((tBR2.Location.Y + tBR2.Size.Height) - 255 / (tBR2.Size.Height * .0209f / Math.Pow(dpix, 2))) - dpix * 20)));
                if (tBR2.Value * (float)nUDR2S.Value >= 255)
                    lbR2Track.ForeColor = Color.Green;
                else if (tBR2.Value * (float)nUDR2S.Value < (double)nUDR2.Value * 255)
                    lbR2Track.ForeColor = Color.Red;
                else
                    lbR2Track.ForeColor = Color.Black;


                double latency = Program.rootHub.DS4Controllers[(int)nUDSixaxis.Value - 1].Latency;
                lbInputDelay.Text = Properties.Resources.InputDelay.Replace("*number*", latency.ToString());
                if (latency > 10)
                {
                    lbInputDelay.BackColor = Color.Red;
                    lbInputDelay.ForeColor = Color.White;
                }
                else if (latency > 5)
                {
                    lbInputDelay.BackColor = Color.Yellow;
                    lbInputDelay.ForeColor = Color.Black;
                }
                else
                {
                    lbInputDelay.BackColor = Color.Transparent;
                    lbInputDelay.ForeColor = Color.Black;
                }
            }
        }

        double TValue(double value1, double value2, double percent)
        {
            percent /= 100f;
            return value1 * percent + value2 * (1 - percent);
        }
        private void InputDS4(object sender, EventArgs e)
        {
            if (Form.ActiveForm == root && cBControllerInput.Checked && tCControls.SelectedIndex < 1)
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


        private void button_MouseHoverB(object sender, EventArgs e)
        {
            Control[] b = Controls.Find(((Button)sender).Name.Remove(1, 1), true);
            if (b != null && b.Length == 1)
                button_MouseHover(b[0], e);
        }

        private string ShiftTrigger(int trigger)
        {
            switch (trigger)
            {
                case 1: return "Cross";
                case 2: return "Circle";
                case 3: return "Square";
                case 4: return "Triangle";
                case 5: return "Options";
                case 6: return "Share";
                case 7: return "Dpad Up";
                case 8: return "Dpad Down";
                case 9: return "Dpad Left";
                case 10: return"Dpad Right";
                case 11: return "PS";
                case 12: return "L1";
                case 13: return "R1";
                case 14: return "L2";
                case 15: return "R2";
                case 16: return "L3";
                case 17: return "R3";
                case 18: return "Left Touch";
                case 19: return "Upper Touch";
                case 20: return "Multi Touch";
                case 21: return "Right Touch";
                case 22: return Properties.Resources.TiltUp; 
                case 23: return Properties.Resources.TiltDown;
                case 24: return Properties.Resources.TiltLeft;
                case 25: return Properties.Resources.TiltRight;
                case 26: return fingerOnTouchpadToolStripMenuItem.Text;
                default: return "";
            }
        }

        private void button_MouseHover(object sender, EventArgs e)
        {
            bool swipesOn = lBControls.Items.Count > 33;
            string name = ((Button)sender).Name;
            if (e != null)
            {
                switch (name)
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
                        #endregion
                }
                if (swipesOn)
                    switch (name)
                    {
                        case "bnSwipeUp": if (swipesOn) lBControls.SelectedIndex = 33; break;
                        case "bnSwipeDown": if (swipesOn) lBControls.SelectedIndex = 34; break;
                        case "bnSwipeLeft": if (swipesOn) lBControls.SelectedIndex = 35; break;
                        case "bnSwipeRight": if (swipesOn) lBControls.SelectedIndex = 36; break;
                    }
            }
            DS4ControlSettings dcs = getDS4CSetting(device, name);
            if (lBControls.SelectedIndex >= 0)
            { 
                string tipText = lBControls.SelectedItem.ToString().Split(':')[0];
                tipText += ": ";
                tipText += UpdateButtonList(((Button)sender));
                if (GetDS4Action(device, name, true) != null && GetDS4STrigger(device, name) > 0)
                {
                    tipText += "\n Shift: ";
                    tipText += ShiftTrigger(GetDS4STrigger(device, name)) + " -> " + UpdateButtonList(((Button)sender), true);
                }
                lbControlName.Text = tipText;
            }
            switch (name)
            {
                #region
                case "bnCross":
                    pBHoveredButton.Image = Properties.Resources.DS4_Config_Cross;
                    pBHoveredButton.Location = lbLCross.Location;
                    break;
                case "bnCircle":
                    pBHoveredButton.Image = Properties.Resources.DS4_Config_Circle;
                    pBHoveredButton.Location = lbLCircle.Location;
                    break;
                case "bnSquare":
                    pBHoveredButton.Image = Properties.Resources.DS4_Config_Square;
                    pBHoveredButton.Location = lbLSquare.Location;
                    break;
                case "bnTriangle":
                    pBHoveredButton.Image = Properties.Resources.DS4_Config_Triangle;
                    pBHoveredButton.Location = lbLTriangle.Location;
                    break;
                case "bnOptions":
                    pBHoveredButton.Image = Properties.Resources.DS4_Config_Options;
                    pBHoveredButton.Location = lbLOptions.Location;
                    break;
                case "bnShare":
                    pBHoveredButton.Image = Properties.Resources.DS4_Config_Share;
                    pBHoveredButton.Location = lbLShare.Location;
                    break;
                case "bnUp":
                    pBHoveredButton.Image = Properties.Resources.DS4_Config_Up;
                    pBHoveredButton.Location = lbLUp.Location;
                    break;
                case "bnDown":
                    pBHoveredButton.Image = Properties.Resources.DS4_Config_Down;
                    pBHoveredButton.Location = lbLDown.Location;
                    break;
                case "bnLeft":
                    pBHoveredButton.Image = Properties.Resources.DS4_Config_Left;
                    pBHoveredButton.Location = lbLLeft.Location;
                    break;
                case "bnRight":
                    pBHoveredButton.Image = Properties.Resources.DS4_Config_Right;
                    pBHoveredButton.Location = lbLright.Location;
                    break;
                case "bnPS":
                    pBHoveredButton.Image = Properties.Resources.DS4_Config_PS;
                    pBHoveredButton.Location = lbLPS.Location;
                    break;
                case "bnL1":
                    pBHoveredButton.Image = Properties.Resources.DS4_Config_L1;
                    pBHoveredButton.Location = lbLL1.Location;
                    break;
                case "bnR1":
                    pBHoveredButton.Image = Properties.Resources.DS4_Config_R1;
                    pBHoveredButton.Location = lbLR1.Location;
                    break;
                case "bnL2":
                    pBHoveredButton.Image = Properties.Resources.DS4_Config_L2;
                    pBHoveredButton.Location = lbLL2.Location;
                    break;
                case "bnR2":
                    pBHoveredButton.Image = Properties.Resources.DS4_Config_R2;
                    pBHoveredButton.Location = lbLR2.Location;
                    break;
                case "bnTouchLeft":
                    pBHoveredButton.Image = Properties.Resources.DS4_Config_TouchLeft;
                    pBHoveredButton.Location = lbLTouchLM.Location;
                    break;
                case "bnTouchRight":
                    pBHoveredButton.Image = Properties.Resources.DS4_Config_TouchRight;
                    pBHoveredButton.Location = lbLTouchRight.Location;
                    break;
                case "bnTouchMulti":
                    pBHoveredButton.Image = Properties.Resources.DS4_Config_TouchMulti;
                    pBHoveredButton.Location = lbLTouchLM.Location;
                    break;
                case "bnTouchUpper":
                    pBHoveredButton.Image = Properties.Resources.DS4_Config_TouchUpper;
                    pBHoveredButton.Location = lbLTouchUpper.Location;
                    break;
                case "bnL3":
                    pBHoveredButton.Image = Properties.Resources.DS4_Config_LS;
                    pBHoveredButton.Location = lbLLS.Location;
                    break;
                case "bnLSUp":
                    pBHoveredButton.Image = Properties.Resources.DS4_Config_LS;
                    pBHoveredButton.Location = lbLLS.Location;
                    break;
                case "bnLSDown":
                    pBHoveredButton.Image = Properties.Resources.DS4_Config_LS;
                    pBHoveredButton.Location = lbLLS.Location;
                    break;
                case "bnLSLeft":
                    pBHoveredButton.Image = Properties.Resources.DS4_Config_LS;
                    pBHoveredButton.Location = lbLLS.Location;
                    break;
                case "bnLSRight":
                    pBHoveredButton.Image = Properties.Resources.DS4_Config_LS;
                    pBHoveredButton.Location = lbLLS.Location;
                    break;
                case "bnR3":
                    pBHoveredButton.Image = Properties.Resources.DS4_Config_RS;
                    pBHoveredButton.Location = lbLRS.Location;
                    break;
                case "bnRSUp":
                    pBHoveredButton.Image = Properties.Resources.DS4_Config_RS;
                    pBHoveredButton.Location = lbLRS.Location;
                    break;
                case "bnRSDown":
                    pBHoveredButton.Image = Properties.Resources.DS4_Config_RS;
                    pBHoveredButton.Location = lbLRS.Location;
                    break;
                case "bnRSLeft":
                    pBHoveredButton.Image = Properties.Resources.DS4_Config_RS;
                    pBHoveredButton.Location = lbLRS.Location;
                    break;
                case "bnRSRight":
                    pBHoveredButton.Image = Properties.Resources.DS4_Config_RS;
                    pBHoveredButton.Location = lbLRS.Location;
                    break;                    
                    #endregion
            }
            if (pBHoveredButton.Image != null)
                pBHoveredButton.Size = new Size((int)(pBHoveredButton.Image.Size.Width * (dpix / 1.25f)), (int)(pBHoveredButton.Image.Size.Height * (dpix / 1.25f)));
        }


        private void button_MouseLeave(object sender, EventArgs e)
        {
            pBHoveredButton.Image = null;
            pBHoveredButton.Location = new Point(0, 0);
            pBHoveredButton.Size = new Size(0, 0);
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
            MainColor[device] = new DS4Color((byte)tBRedBar.Value, (byte)tBGreenBar.Value, (byte)tBBlueBar.Value);
            LowColor[device] = new DS4Color((byte)tBLowRedBar.Value, (byte)tBLowGreenBar.Value, (byte)tBLowBlueBar.Value);
            ChargingColor[device] = new DS4Color(btnChargingColor.BackColor);
            FlashType[device] = (byte)cBFlashType.SelectedIndex;
            if (btnFlashColor.BackColor != main)
                FlashColor[device] = new DS4Color(btnFlashColor.BackColor);
            else
                FlashColor[device] = new DS4Color(Color.Black);
            L2Deadzone[device] = (byte)Math.Round((nUDL2.Value * 255), 0);
            R2Deadzone[device] = (byte)Math.Round((nUDR2.Value * 255), 0);
            RumbleBoost[device] = (byte)nUDRumbleBoost.Value;
            TouchSensitivity[device] = (byte)nUDTouch.Value;
            TouchpadJitterCompensation[device] = cBTouchpadJitterCompensation.Checked;
            LowerRCOn[device] = cBlowerRCOn.Checked;
            ScrollSensitivity[device] = (int)nUDScroll.Value;
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
            DinputOnly[device] = cBDinput.Checked;
            StartTouchpadOff[device] = cbStartTouchpadOff.Checked;
            UseTPforControls[device] = rBTPControls.Checked;
            UseSAforMouse[device] = rBSAMouse.Checked;
            DS4Mapping = cBControllerInput.Checked;
            LSCurve[device] = (int)Math.Round(nUDLSCurve.Value, 0);
            RSCurve[device] = (int)Math.Round(nUDRSCurve.Value, 0);
            List<string> pactions = new List<string>();
            foreach (ListViewItem lvi in lVActions.Items)
                if (lvi.Checked)
                    pactions.Add(lvi.Text);
            ProfileActions[device] = pactions;
            pnlTPMouse.Visible = rBTPMouse.Checked;
            pnlSAMouse.Visible = rBSAMouse.Checked;
            fLPTiltControls.Visible = rBSAControls.Checked;
            fLPTouchSwipe.Visible = rBTPControls.Checked;

            GyroSensitivity[device] = (int)Math.Round(nUDGyroSensitivity.Value, 0);
            int invert = 0;
            if (cBGyroInvertX.Checked)
                invert += 2;
            if (cBGyroInvertY.Checked)
                invert += 1;
            GyroInvert[device] = invert;

            List<int> ints = new List<int>();
            for (int i = 0; i < cMGyroTriggers.Items.Count - 1; i++)
                if (((ToolStripMenuItem)cMGyroTriggers.Items[i]).Checked)
                    ints.Add(i);
            if (ints.Count == 0)
                ints.Add(-1);
            SATriggers[device] = string.Join(",", ints);
            if (nUDRainbow.Value == 0) btnRainbow.Image = greyscale;
            else btnRainbow.Image = colored;
        }

        private void Show_ControlsBtn(object sender, EventArgs e)
        {
            Control[] b = Controls.Find(((Button)sender).Name.Remove(1, 1), true);
            if (b != null && b.Length == 1)
                Show_ControlsBn(b[0], e);
        }

        private void Show_ControlsBn(object sender, EventArgs e)
        {
            KBM360 kbm360 = new KBM360(device, this, (Button)sender);
            kbm360.Icon = Icon;
            kbm360.ShowDialog();
        }        

        public void ChangeButtonText(Control ctrl, bool shift, KeyValuePair<object, string> tag, bool SC, bool TG, bool MC, bool MR, int sTrigger = 0)
        {
            DS4KeyType kt = DS4KeyType.None;
            if (SC) kt |= DS4KeyType.ScanCode;
            if (TG) kt |= DS4KeyType.Toggle;
            if (MC) kt |= DS4KeyType.Macro;
            if (MR) kt |= DS4KeyType.RepeatMacro;
            UpdateDS4CSetting(device, ctrl.Name, shift, tag.Key, tag.Value, kt, sTrigger);
        }

         public void ChangeButtonText(KeyValuePair<object, string> tag, Control ctrl, bool SC)
         {
             if (ctrl is Button)
             {
                DS4KeyType kt = DS4KeyType.None;
                if (SC) kt |= DS4KeyType.ScanCode;
                UpdateDS4CSetting(device, ctrl.Name, false, tag.Key, tag.Value, kt);
            }
         }

        /*public void Toggle_Bn(bool SC, bool TG, bool MC,  bool MR)
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
        }*/


        private void btnLightbar_Click(object sender, EventArgs e)
        {
            advColorDialog.Color = Color.FromArgb(tBRedBar.Value, tBGreenBar.Value, tBBlueBar.Value);
            advColorDialog_OnUpdateColor(main, e);
            if (advColorDialog.ShowDialog() == DialogResult.OK)
            {
                main = advColorDialog.Color;
                btnLightbar.BackgroundImage = RecolorImage((Bitmap)btnLightbar.BackgroundImage, main);
                if (FlashColor[device].Equals(new DS4Color { red = 0, green = 0, blue = 0 }))
                    btnFlashColor.BackColor = main;
                btnFlashColor.BackgroundImage = nUDRainbow.Enabled ? rainbowImg : null;
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
        private void SetColorToolTip(TrackBar tb, int type)
        {
            if (tb != null)
            {
                int value = tb.Value;
                int sat = bgc - (value < bgc ? value : bgc);
                int som = bgc + 11 * (int)(value * 0.0039215);
                tb.BackColor = Color.FromArgb(tb.Name.ToLower().Contains("red") ? som : sat, tb.Name.ToLower().Contains("green") ? som : sat, tb.Name.ToLower().Contains("blue") ? som : sat);
            }
            if (type == 0) //main
            {
                alphacolor = Math.Max(tBRedBar.Value, Math.Max(tBGreenBar.Value, tBBlueBar.Value));
                reg = Color.FromArgb(tBRedBar.Value, tBGreenBar.Value, tBBlueBar.Value);
                full = HuetoRGB(reg.GetHue(), reg.GetBrightness(), reg);
                main = Color.FromArgb((alphacolor > 205 ? 255 : (alphacolor + 50)), full);
                btnLightbar.BackgroundImage = RecolorImage((Bitmap)btnLightbar.BackgroundImage, main);
                if (FlashColor[device].Equals(new DS4Color { red = 0, green = 0, blue = 0 }))
                    btnFlashColor.BackColor = main;
                btnFlashColor.BackgroundImage = nUDRainbow.Enabled ? rainbowImg : null;
                MainColor[device] = new DS4Color((byte)tBRedBar.Value, (byte)tBGreenBar.Value, (byte)tBBlueBar.Value);
            }
            else if (type == 1)
            {
                alphacolor = Math.Max(tBLowRedBar.Value, Math.Max(tBLowGreenBar.Value, tBLowBlueBar.Value));
                reg = Color.FromArgb(tBLowRedBar.Value, tBLowGreenBar.Value, tBLowBlueBar.Value);
                full = HuetoRGB(reg.GetHue(), reg.GetBrightness(), reg);
                lowColorChooserButton.BackColor = Color.FromArgb((alphacolor > 205 ? 255 : (alphacolor + 50)), full);
                LowColor[device] = new DS4Color((byte)tBLowRedBar.Value, (byte)tBLowGreenBar.Value, (byte)tBLowBlueBar.Value);
            }
            if (!saving && !loading && tb != null)
                tp.Show(tb.Value.ToString(), tb, (int)(dpix * 100), 0, 2000);
        }

        int bgc = 245; //Color of the form background, If greyscale color
        private void MainBar_ValueChanged(object sender, EventArgs e)
        {
            SetColorToolTip((TrackBar)sender, 0);
        }

        private void LowBar_ValueChanged(object sender, EventArgs e)
        {
            SetColorToolTip((TrackBar)sender, 1);
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
            //pnlFull.Location = new Point(pnlFull.Location.X, (cBLightbyBattery.Checked ? (int)(dpix * 42) : (pnlFull.Location.Y + pnlLowBattery.Location.Y) / 2));
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

        private void Options_FormClosing(object sender, FormClosingEventArgs e)
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
            root.OptionsClosed();
            Visible = false;
            e.Cancel = true;
        }

        private void Options_Closed(object sender, FormClosedEventArgs e)
        {
            /*for (int i = 0; i < 4; i++)
                LoadProfile(i, false, Program.rootHub); //Refreshes all profiles in case other controllers are using the same profile
            if (olddinputcheck != cBDinput.Checked)
            {
                root.btnStartStop_Clicked(false);
                root.btnStartStop_Clicked(false);
            }
            if (btnRumbleHeavyTest.Text == Properties.Resources.StopText)
                Program.rootHub.setRumble(0, 0, (int)nUDSixaxis.Value - 1);
            inputtimer.Stop();
            sixaxisTimer.Stop();*/
            //e.c
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
            lBControls.Items[0] = "Cross : " + UpdateButtonList(bnCross);
            lBControls.Items[1] = "Circle : " + UpdateButtonList(bnCircle);
            lBControls.Items[2] = "Square : " + UpdateButtonList(bnSquare);
            lBControls.Items[3] = "Triangle : " + UpdateButtonList(bnTriangle);
            lBControls.Items[4] = "Options : " + UpdateButtonList(bnOptions);
            lBControls.Items[5] = "Share : " + UpdateButtonList(bnShare);
            lBControls.Items[6] = "Up : " + UpdateButtonList(bnUp);
            lBControls.Items[7] = "Down : " + UpdateButtonList(bnDown);
            lBControls.Items[8] = "Left : " + UpdateButtonList(bnLeft);
            lBControls.Items[9] = "Right : " + UpdateButtonList(bnRight);
            lBControls.Items[10] = "PS : " + UpdateButtonList(bnPS);
            lBControls.Items[11] = "L1 : " + UpdateButtonList(bnL1);
            lBControls.Items[12] = "R1 : " + UpdateButtonList(bnR1);
            lBControls.Items[13] = "L2 : " + UpdateButtonList(bnL2);
            lBControls.Items[14] = "R2 : " + UpdateButtonList(bnR2);
            lBControls.Items[15] = "L3 : " + UpdateButtonList(bnL3);
            lBControls.Items[16] = "R3 : " + UpdateButtonList(bnR3);
            lBControls.Items[17] = "Left Touch : " + UpdateButtonList(bnTouchLeft);
            lBControls.Items[18] = "Right Touch : " + UpdateButtonList(bnTouchRight);
            lBControls.Items[19] = "Multitouch : " + UpdateButtonList(bnTouchMulti);
            lBControls.Items[20] = "Upper Touch : " + UpdateButtonList(bnTouchUpper);
            lBControls.Items[21] = "LS Up : " + UpdateButtonList(bnLSUp);
            lBControls.Items[22] = "LS Down : " + UpdateButtonList(bnLSDown);
            lBControls.Items[23] = "LS Left : " + UpdateButtonList(bnLSLeft);
            lBControls.Items[24] = "LS Right : " + UpdateButtonList(bnLSRight);
            lBControls.Items[25] = "RS Up : " + UpdateButtonList(bnRSUp);
            lBControls.Items[26] = "RS Down : " + UpdateButtonList(bnRSDown);
            lBControls.Items[27] = "RS Left : " + UpdateButtonList(bnRSLeft);
            lBControls.Items[28] = "RS Right : " + UpdateButtonList(bnRSRight);
            lBControls.Items[29] = Properties.Resources.TiltUp + " : " + UpdateButtonList(bnGyroZN);
            lBControls.Items[30] = Properties.Resources.TiltDown + " : " + UpdateButtonList(bnGyroZP);
            lBControls.Items[31] = Properties.Resources.TiltLeft + " : " + UpdateButtonList(bnGyroXP);
            lBControls.Items[32] = Properties.Resources.TiltRight + " : " + UpdateButtonList(bnGyroXN);
            if (lBControls.Items.Count > 33)
            {
                lBControls.Items[33] = Properties.Resources.SwipeUp + " : " + UpdateButtonList(bnSwipeUp);
                lBControls.Items[34] = Properties.Resources.SwipeDown + " : " + UpdateButtonList(bnSwipeDown);
                lBControls.Items[35] = Properties.Resources.SwipeLeft + " : " + UpdateButtonList(bnSwipeLeft);
                lBControls.Items[36] = Properties.Resources.SwipeRight + " : " + UpdateButtonList(bnSwipeRight);

                lbSwipeUp.Text = UpdateButtonList(bnSwipeUp);
                lbSwipeDown.Text = UpdateButtonList(bnSwipeDown);
                lbSwipeLeft.Text = UpdateButtonList(bnSwipeLeft);
                lbSwipeRight.Text = UpdateButtonList(bnSwipeRight);
            }

            lbGyroXN.Text = UpdateButtonList(bnGyroXN);
            lbGyroZN.Text = UpdateButtonList(bnGyroZN);
            lbGyroZP.Text = UpdateButtonList(bnGyroZP);
            lbGyroXP.Text = UpdateButtonList(bnGyroXP);                        
        }

        private string UpdateButtonList(Button button, bool shift =false)
        {
            object tagO = GetDS4Action(device, button.Name, shift);
            bool SC = GetDS4KeyType(device, button.Name, false).HasFlag(DS4KeyType.ScanCode);
            bool extracontrol = button.Name.Contains("Gyro") || button.Name.Contains("Swipe");
            if (tagO != null)
            {
                if (tagO is int || tagO is ushort)
                {
                    return (Keys)int.Parse(tagO.ToString()) + (SC ? " (" + Properties.Resources.ScanCode + ")" : "");
                }
                else if (tagO is int[])
                {
                    return Properties.Resources.Macro + (SC ? " (" + Properties.Resources.ScanCode + ")" : "");
                }
                else if (tagO is string || tagO is X360Controls)
                {
                    string tag;
                    if (tagO is X360Controls)
                    {
                        tag = KBM360.getX360ControlsByName((X360Controls)tagO);
                    }
                    else
                        tag = tagO.ToString();
                    return tag;
                }
                else
                    return defaults[button.Name];
            }
            else if (!extracontrol && !shift && defaults.ContainsKey(button.Name))
                return defaults[button.Name];
            else if (shift)
                return "";
            else
                return Properties.Resources.Unassigned;
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

        private void List_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            Show_ControlsList(sender, e);
        }

        private void List_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyValue == 13)
                Show_ControlsList(sender, e);
        }

        private void numUDRainbow_ValueChanged(object sender, EventArgs e)
        {
            Rainbow[device]= (double)nUDRainbow.Value;
            if ((double)nUDRainbow.Value <= 0.5)
            {
                btnRainbow.Image = greyscale;
                ToggleRainbow(false);
                nUDRainbow.Value = 0;
            }
        }

        private void btnRainbow_Click(object sender, EventArgs e)
        {
            if (btnRainbow.Image == greyscale)
            {
                btnRainbow.Image = colored;
                ToggleRainbow(true);
                nUDRainbow.Value = 5;
            }
            else
            {
                btnRainbow.Image = greyscale;
                ToggleRainbow(false);
                nUDRainbow.Value = 0;
            }
        }

        private void ToggleRainbow(bool on)
        {
            nUDRainbow.Enabled = on;
            if (on)
            {
                btnLightbar.BackgroundImage = RecolorImage((Bitmap)btnLightbar.BackgroundImage, main);
                cBLightbyBattery.Text = Properties.Resources.DimByBattery.Replace("*nl*", "\n");
            }
            else
            {
                pnlLowBattery.Enabled = cBLightbyBattery.Checked;
                btnLightbar.BackgroundImage = RecolorImage((Bitmap)btnLightbar.BackgroundImage, main);
                cBLightbyBattery.Text = Properties.Resources.ColorByBattery.Replace("*nl*", "\n");
            }
            if (FlashColor[device].Equals(new DS4Color { red = 0, green = 0, blue = 0 }))
                btnFlashColor.BackColor = main;
            btnFlashColor.BackgroundImage = nUDRainbow.Enabled ? rainbowImg : null;
            lbspc.Enabled = on;
            pnlLowBattery.Enabled = !on;
            pnlFull.Enabled = !on;
        }

        private Bitmap GreyscaleImage(Bitmap image)
        {
            Bitmap c = image;
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

        private Bitmap RecolorImage(Bitmap image, Color color)
        {
            Bitmap c = Properties.Resources.DS4_lightbar;
            Bitmap d = new Bitmap(c.Width, c.Height);

            for (int i = 0; i < c.Width; i++)
                for (int x = 0; x < c.Height; x++)
                    if (!nUDRainbow.Enabled)
                    {
                        Color col = c.GetPixel(i, x);
                        col = Color.FromArgb((int)(col.A * (color.A / 255f)), color.R, color.G, color.B);
                        d.SetPixel(i, x, col);
                    }
                    else
                    {
                        Color col = HuetoRGB((i / (float)c.Width) * 360, .5f, Color.Red);
                        d.SetPixel(i, x, Color.FromArgb(c.GetPixel(i, x).A, col));
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
            pnlSATrack.Refresh();            
        }

        private void nUDSZ_ValueChanged(object sender, EventArgs e)
        {
            SZDeadzone[device] = (double)nUDSZ.Value;
            pnlSATrack.Refresh();
        }

        private void pnlSATrack_Paint(object sender, PaintEventArgs e)
        {
            if (nUDSX.Value > 0 || nUDSZ.Value > 0)
            {
                e.Graphics.FillEllipse(Brushes.Red,
                    (int)(dpix * 63) - (int)(nUDSX.Value * 125) / 2,
                    (int)(dpix * 63) - (int)(nUDSZ.Value * 125) / 2,
                    (int)(nUDSX.Value * 125), (int)(nUDSZ.Value * 125));
            }
        }

        private void numUDRS_ValueChanged(object sender, EventArgs e)
        {
            nUDRS.Value = Math.Round(nUDRS.Value, 2);
            RSDeadzone[device] = (int)Math.Round((nUDRS.Value * 127),0);
            pnlRSTrack.BackColor = nUDRS.Value >= 0 ? Color.White : Color.Red;
            pnlRSTrack.Refresh();
        }

        private void pnlRSTrack_Paint(object sender, PaintEventArgs e)
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
            pnlLSTrack.BackColor = nUDLS.Value >= 0 ? Color.White : Color.Red;
            pnlLSTrack.Refresh();
        }

        private void pnlLSTrack_Paint(object sender, PaintEventArgs e)
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
            if (!saving && !loading)
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
        
        private void tabControls_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tCControls.SelectedIndex == 2)
                sixaxisTimer.Start();
            else
                sixaxisTimer.Stop();
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

        private void lbSATip_Click(object sender, EventArgs e)
        {
            //pnlSixaxis.Visible = !pnlSixaxis.Visible;
            //btnSATrack.Visible = !btnSATrack.Visible;
        }

        private void SixaxisPanel_Click(object sender, EventArgs e)
        {
            lbSATip_Click(sender, e);
        }

        private void lbSATrack_Click(object sender, EventArgs e)
        {
            lbSATip_Click(sender, e);
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
            string name = ((Control)sender).Name;
            if (name.Contains("btn") && !name.Contains("Flash") && !name.Contains("Stick") && !name.Contains("Rainbow"))
                name = name.Remove(1, 1);
            switch (name)
            {
                case "cBlowerRCOn": root.lbLastMessage.Text = Properties.Resources.BestUsedRightSide; break;
                case "cBDoubleTap": root.lbLastMessage.Text = Properties.Resources.TapAndHold; break;
                case "lbControlTip": root.lbLastMessage.Text = Properties.Resources.UseControllerForMapping; break;
                case "cBTouchpadJitterCompensation": root.lbLastMessage.Text = Properties.Resources.Jitter; break;
                case "btnRainbow": root.lbLastMessage.Text = Properties.Resources.AlwaysRainbow; break;
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
                case "btnLS": root.lbLastMessage.Text = Properties.Resources.RightClickPresets; break;
                case "btnRS": root.lbLastMessage.Text = Properties.Resources.RightClickPresets; break;
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
                case "bnL3": root.lbLastMessage.Text = Properties.Resources.RightClickPresets; break;                    
                case "bnR3": root.lbLastMessage.Text = Properties.Resources.RightClickPresets; break;                    
            }
            if (name.Contains("bnLS") || name.Contains("bnRS"))
                root.lbLastMessage.Text = Properties.Resources.RightClickPresets;
            if (root.lbLastMessage.Text != Properties.Resources.HoverOverItems)
                root.lbLastMessage.ForeColor = Color.Black;
            else
                root.lbLastMessage.ForeColor = SystemColors.GrayText;
        }

        private void cBTPforControls_CheckedChanged(object sender, EventArgs e)
        {
            UseTPforControls[device] = rBTPControls.Checked;
            pnlTPMouse.Visible = rBTPMouse.Checked;
            fLPTouchSwipe.Visible = rBTPControls.Checked;
            if (rBTPControls.Checked && lBControls.Items.Count <= 33)
            {
                lBControls.Items.AddRange(new string[4] { "t", "t", "t", "t" });
                UpdateLists();
            }
            else if (rBTPMouse.Checked && lBControls.Items.Count > 33)
            {
                lBControls.Items.RemoveAt(36);
                lBControls.Items.RemoveAt(35);
                lBControls.Items.RemoveAt(34);
                lBControls.Items.RemoveAt(33);
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
                if (lvi != null && lvi.Checked)
                    pactions.Add(lvi.Text);
            ProfileActions[device] = pactions;
            /*if (lVActions.Items.Count >= 50)
            {
                btnNewAction.Enabled = false;
            }*/
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
            if (name.Contains("btn") && !name.Contains("Stick"))
                name = name.Remove(1, 1);
            if (name == "bnUp" || name == "bnLeft" || name == "bnRight" || name == "bnDown")
                controlToolStripMenuItem.Text = "Dpad";
            else if (name == "btnLeftStick" || name.Contains("bnLS") || name.Contains("bnL3"))
                controlToolStripMenuItem.Text = "Left Stick";
            else if (name == "btnRightStick" || name.Contains("bnRS") || name.Contains("bnR3"))
                controlToolStripMenuItem.Text = "Right Stick";
            else if (name == "bnCross" || name == "bnCircle" || name == "bnSquare" || name == "bnTriangle")
                controlToolStripMenuItem.Text = "Face Buttons";
            else if (name == "lbGyro" || name.StartsWith("bnGyro"))
                controlToolStripMenuItem.Text = "Sixaxis";
            else if (name == "lbTPSwipes" || name.StartsWith("bnSwipe"))
                controlToolStripMenuItem.Text = "Touchpad Swipes";
            else
                controlToolStripMenuItem.Text = "Select another control";
            MouseToolStripMenuItem.Visible = !(name == "lbTPSwipes" || name.StartsWith("bnSwipe"));
        }

        /*private void BatchToggle_Bn(bool scancode, Button button1, Button button2, Button button3, Button button4)
        {
            Toggle_Bn(scancode, false, false, false, button1);
            Toggle_Bn(scancode, false, false, false, button2);
            Toggle_Bn(scancode, false, false, false, button3);
            Toggle_Bn(scancode, false, false, false, button4);
        }*/


        private void SetPreset(object sender, EventArgs e)
        {
            bool scancode = false;
            KeyValuePair<object, string> tagU;
            KeyValuePair<object, string> tagL;
            KeyValuePair<object, string> tagR;
            KeyValuePair<object, string> tagD;
            KeyValuePair<object, string> tagM = new KeyValuePair<object, string>(null, "0,0,0,0,0,0,0,0"); ;
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
                tagM = new KeyValuePair<object, string>("Left Stick", "0,0,0,0,0,0,0,0");
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
                tagM = new KeyValuePair<object, string>("Right Stick", "0,0,0,0,0,0,0,0");
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

            Button button1, button2, button3, button4, button5 = null;
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
                button5 = bnL3;
            }
            else if (controlToolStripMenuItem.Text == "Right Stick")
            {
                button1 = bnRSUp;
                button2 = bnRSLeft;
                button3 = bnRSRight;
                button4 = bnRSDown;
                button5 = bnR3;
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
            else
                button1 = button2 = button3 = button4 = null;
            ChangeButtonText(tagU, button1, scancode);
            ChangeButtonText(tagL, button2, scancode);
            ChangeButtonText(tagR, button3, scancode);
            ChangeButtonText(tagD, button4, scancode);
            if (tagM.Key != null && button5 != null)
                ChangeButtonText(tagM, button5, scancode);
            //BatchToggle_Bn(scancode, button1, button2, button3, button4);

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
            if (btnFlashColor.BackColor != main)
                advColorDialog.Color = btnFlashColor.BackColor;
            else
                advColorDialog.Color = Color.Black;
            advColorDialog_OnUpdateColor(lbPercentFlashBar.ForeColor, e);
            if (advColorDialog.ShowDialog() == DialogResult.OK)
            {
                if (advColorDialog.Color.GetBrightness() > 0)
                    btnFlashColor.BackColor = advColorDialog.Color;
                else
                    btnFlashColor.BackColor = main;
                FlashColor[device] = new DS4Color(advColorDialog.Color);
            }
            if (device < 4)
                DS4LightBar.forcelight[device] = false;
        }

        private void useSAforMouse_CheckedChanged(object sender, EventArgs e)
        {
            UseSAforMouse[device] = rBSAMouse.Checked;
            pnlSAMouse.Visible = rBSAMouse.Checked;
            fLPTiltControls.Visible = rBSAControls.Checked;
        }

        private void btnGyroTriggers_Click(object sender, EventArgs e)
        {
            cMGyroTriggers.Show((Control)sender, new Point(0, ((Control)sender).Height));
        }

        private void SATrigger_CheckedChanged(object sender, EventArgs e)
        {
            if (sender != cMGyroTriggers.Items[cMGyroTriggers.Items.Count - 1] && ((ToolStripMenuItem)sender).Checked)
                ((ToolStripMenuItem)cMGyroTriggers.Items[cMGyroTriggers.Items.Count - 1]).Checked = false;
            if (((ToolStripMenuItem)cMGyroTriggers.Items[cMGyroTriggers.Items.Count - 1]).Checked) //always on
                for (int i = 0; i < cMGyroTriggers.Items.Count - 1; i++)
                    ((ToolStripMenuItem)cMGyroTriggers.Items[i]).Checked = false;

            List <int> ints = new List<int>();
            List<string> s = new List<string>();
            for (int i = 0; i < cMGyroTriggers.Items.Count - 1; i++)
                if (((ToolStripMenuItem)cMGyroTriggers.Items[i]).Checked)
                {
                    ints.Add(i);
                    s.Add(cMGyroTriggers.Items[i].Text);
                }
            if (ints.Count == 0)
            {
                ints.Add(-1);
                s.Add(cMGyroTriggers.Items[cMGyroTriggers.Items.Count - 1].Text);
            }
            SATriggers[device] = string.Join(",", ints);
            if (s.Count > 0)
                btnGyroTriggers.Text = string.Join(", ", s);
        }

        private void cBGyroInvert_CheckChanged(object sender, EventArgs e)
        {
            int invert = 0;
            if (cBGyroInvertX.Checked)
                invert += 2;
            if (cBGyroInvertY.Checked)
                invert += 1;
            GyroInvert[device] = invert;
        }

        private void btnLightbar_MouseHover(object sender, EventArgs e)
        {
            lbControlName.Text = lbControlTip.Text;
        }

        private void nUDSens_ValueChanged(object sender, EventArgs e)
        {
            if (!loading)
            {
                L2Sens[device] = (double)nUDL2S.Value;
                R2Sens[device] = (double)nUDR2S.Value;
                LSSens[device] = (double)nUDLSS.Value;
                RSSens[device] = (double)nUDRSS.Value;
                SXSens[device] = (double)nUDSXS.Value;
                SZSens[device] = (double)nUDSZS.Value;
            }
        }

        private void Options_Resize(object sender, EventArgs e)
        {
            fLPSettings.AutoScroll = false;
            fLPSettings.AutoScroll = true;
        }

        private void lBControls_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lBControls.SelectedItem != null)
            {
                //lbControlName.Text = lBControls.SelectedItem.ToString();
                if (lBControls.SelectedIndex == 0)
                    lbControlName.ForeColor = Color.FromArgb(153, 205, 204);
                else if (lBControls.SelectedIndex == 1)
                    lbControlName.ForeColor = Color.FromArgb(247, 131, 150);
                else if (lBControls.SelectedIndex == 2)
                    lbControlName.ForeColor = Color.FromArgb(237, 170, 217);
                else if (lBControls.SelectedIndex == 3)
                    lbControlName.ForeColor = Color.FromArgb(75, 194, 202);
                else
                    lbControlName.ForeColor = Color.White;
            }
            if (lBControls.SelectedIndex == 0) button_MouseHover(bnCross, null);
            if (lBControls.SelectedIndex == 1) button_MouseHover(bnCircle, null);
            if (lBControls.SelectedIndex == 2) button_MouseHover(bnSquare, null);
            if (lBControls.SelectedIndex == 3) button_MouseHover(bnTriangle, null);
            if (lBControls.SelectedIndex == 4) button_MouseHover(bnOptions, null);
            if (lBControls.SelectedIndex == 5) button_MouseHover(bnShare, null);
            if (lBControls.SelectedIndex == 6) button_MouseHover(bnUp, null);
            if (lBControls.SelectedIndex == 7) button_MouseHover(bnDown, null);
            if (lBControls.SelectedIndex == 8) button_MouseHover(bnLeft, null);
            if (lBControls.SelectedIndex == 9) button_MouseHover(bnRight, null);
            if (lBControls.SelectedIndex == 10) button_MouseHover(bnPS, null);
            if (lBControls.SelectedIndex == 11) button_MouseHover(bnL1, null);
            if (lBControls.SelectedIndex == 12) button_MouseHover(bnR1, null);
            if (lBControls.SelectedIndex == 13) button_MouseHover(bnL2, null);
            if (lBControls.SelectedIndex == 14) button_MouseHover(bnR2, null);
            if (lBControls.SelectedIndex == 15) button_MouseHover(bnL3, null);
            if (lBControls.SelectedIndex == 16) button_MouseHover(bnR3, null);

            if (lBControls.SelectedIndex == 17) button_MouseHover(bnTouchLeft, null);
            if (lBControls.SelectedIndex == 18) button_MouseHover(bnTouchRight, null);
            if (lBControls.SelectedIndex == 19) button_MouseHover(bnTouchMulti, null);
            if (lBControls.SelectedIndex == 20) button_MouseHover(bnTouchUpper, null);

            if (lBControls.SelectedIndex == 21) button_MouseHover(bnLSUp, null);
            if (lBControls.SelectedIndex == 22) button_MouseHover(bnLSDown, null);
            if (lBControls.SelectedIndex == 23) button_MouseHover(bnLSLeft, null);
            if (lBControls.SelectedIndex == 24) button_MouseHover(bnLSRight, null);
            if (lBControls.SelectedIndex == 25) button_MouseHover(bnRSUp, null);
            if (lBControls.SelectedIndex == 26) button_MouseHover(bnRSDown, null);
            if (lBControls.SelectedIndex == 27) button_MouseHover(bnRSLeft, null);
            if (lBControls.SelectedIndex == 28) button_MouseHover(bnRSRight, null);

            if (lBControls.SelectedIndex == 29) button_MouseHover(bnGyroZN, null);
            if (lBControls.SelectedIndex == 30) button_MouseHover(bnGyroZP, null);
            if (lBControls.SelectedIndex == 31) button_MouseHover(bnGyroXP, null);
            if (lBControls.SelectedIndex == 32) button_MouseHover(bnGyroXN, null);

            if (lBControls.SelectedIndex == 33) button_MouseHover(bnSwipeUp, null);
            if (lBControls.SelectedIndex == 34) button_MouseHover(bnSwipeDown, null);
            if (lBControls.SelectedIndex == 35) button_MouseHover(bnSwipeLeft, null);
            if (lBControls.SelectedIndex == 36) button_MouseHover(bnSwipeRight, null);
        }
        
        private void nUDGyroSensitivity_ValueChanged(object sender, EventArgs e)
        {
            GyroSensitivity[device] = (int)Math.Round(nUDGyroSensitivity.Value, 0);
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
