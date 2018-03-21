using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using System.IO;
using static DS4Windows.Global;

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
        public bool actionTabSeen = false;
        public static Size mSize { get; private set; }
        private Size settingsSize;
        private Dictionary<Control, int> hoverIndexDict = new Dictionary<Control, int>();
        private Dictionary<Control, Bitmap> hoverImageDict = new Dictionary<Control, Bitmap>();
        private Dictionary<Control, Label> hoverLabelDict = new Dictionary<Control, Label>();
        private int[] touchpadInvertToValue = new int[4] { 0, 2, 1, 3 };
        private Bitmap pnlControllerBgImg;
        private Bitmap btnLightBgImg;
        private Bitmap btnLightBg;

        int tempInt = 0;

        public Options(DS4Form rt)
        {
            InitializeComponent();
            pnlControllerBgImg = (Bitmap)Properties.Resources.DS4_Config.Clone();
            btnLightBg = (Bitmap)Properties.Resources.DS4_lightbar.Clone();
            pnlController.BackgroundImage = null;
            pnlController.BackgroundImageLayout = ImageLayout.None;
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
            {
                if (control is Button && !((Button)control).Name.Contains("btn"))
                    buttons.Add((Button)control);
            }

            foreach (Control control in fLPTouchSwipe.Controls)
            {
                if (control is Button && !((Button)control).Name.Contains("btn"))
                    buttons.Add((Button)control);
            }

            foreach (Control control in fLPTiltControls.Controls)
            {
                if (control is Button && !((Button)control).Name.Contains("btn"))
                    buttons.Add((Button)control);
            }

            foreach (Control control in pnlController.Controls)
            {
                if (control is Button && !((Button)control).Name.Contains("btn"))
                    buttons.Add((Button)control);
            }

            foreach (Button b in buttons)
            {
                defaults.Add(b.Name, b.Text);
                b.Text = "";
            }
            
            foreach (Control control in Controls)
            {
                if (control.HasChildren)
                {
                    foreach (Control ctrl in control.Controls)
                    {
                        if (ctrl.HasChildren)
                        {
                            foreach (Control ctrl2 in ctrl.Controls)
                            {
                                if (ctrl2.HasChildren)
                                {
                                    foreach (Control ctrl3 in ctrl2.Controls)
                                        ctrl3.MouseHover += Items_MouseHover;
                                }

                                ctrl2.MouseHover += Items_MouseHover;
                            }
                        }

                        ctrl.MouseHover += Items_MouseHover;
                    }
                }

                control.MouseHover += Items_MouseHover;
            }
            
            foreach (Button b in buttons)
            {
                b.MouseHover += button_MouseHover;
                b.MouseLeave += button_MouseLeave;
            }

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
            btnLightbar.BackgroundImage = null;
            btnLightbar.BackgroundImageLayout = ImageLayout.None;

            populateHoverIndexDict();
            populateHoverImageDict();
            populateHoverLabelDict();
        }

        private void populateHoverIndexDict()
        {
            hoverIndexDict.Clear();
            hoverIndexDict[bnCross] = 0;
            hoverIndexDict[bnCircle] = 1;
            hoverIndexDict[bnSquare] = 2;
            hoverIndexDict[bnTriangle] = 3;
            hoverIndexDict[bnOptions] = 4;
            hoverIndexDict[bnShare] = 5;
            hoverIndexDict[bnUp] = 6;
            hoverIndexDict[bnDown] = 7;
            hoverIndexDict[bnLeft] = 8;
            hoverIndexDict[bnRight] = 9;
            hoverIndexDict[bnPS] = 10;
            hoverIndexDict[bnL1] = 11;
            hoverIndexDict[bnR1] = 12;
            hoverIndexDict[bnL2] = 13;
            hoverIndexDict[bnR2] = 14;
            hoverIndexDict[bnL3] = 15;
            hoverIndexDict[bnR3] = 16;
            hoverIndexDict[bnTouchLeft] = 17;
            hoverIndexDict[bnTouchRight] = 18;
            hoverIndexDict[bnTouchMulti] = 19;
            hoverIndexDict[bnTouchUpper] = 20;
            hoverIndexDict[bnLSUp] = 21;
            hoverIndexDict[bnLSDown] = 22;
            hoverIndexDict[bnLSLeft] = 23;
            hoverIndexDict[bnLSRight] = 24;
            hoverIndexDict[bnRSUp] = 25;
            hoverIndexDict[bnRSDown] = 26;
            hoverIndexDict[bnRSLeft] = 27;
            hoverIndexDict[bnRSRight] = 28;
            hoverIndexDict[bnGyroZN] = 29;
            hoverIndexDict[bnGyroZP] = 30;
            hoverIndexDict[bnGyroXP] = 31;
            hoverIndexDict[bnGyroXN] = 32;
            hoverIndexDict[bnSwipeUp] = 33;
            hoverIndexDict[bnSwipeDown] = 34;
            hoverIndexDict[bnSwipeLeft] = 35;
            hoverIndexDict[bnSwipeRight] = 36;
        }

        private void populateHoverImageDict()
        {
            hoverImageDict.Clear();
            hoverImageDict[bnCross] = Properties.Resources.DS4_Config_Cross;
            hoverImageDict[bnCircle] = Properties.Resources.DS4_Config_Circle;
            hoverImageDict[bnSquare] = Properties.Resources.DS4_Config_Square;
            hoverImageDict[bnTriangle] = Properties.Resources.DS4_Config_Triangle;
            hoverImageDict[bnOptions] = Properties.Resources.DS4_Config_Options;
            hoverImageDict[bnShare] = Properties.Resources.DS4_Config_Share;
            hoverImageDict[bnUp] = Properties.Resources.DS4_Config_Up;
            hoverImageDict[bnDown] = Properties.Resources.DS4_Config_Down;
            hoverImageDict[bnLeft] = Properties.Resources.DS4_Config_Left;
            hoverImageDict[bnRight] = Properties.Resources.DS4_Config_Right;
            hoverImageDict[bnPS] = Properties.Resources.DS4_Config_PS;
            hoverImageDict[bnL1] = Properties.Resources.DS4_Config_L1;
            hoverImageDict[bnR1] = Properties.Resources.DS4_Config_R1;
            hoverImageDict[bnL2] = Properties.Resources.DS4_Config_L2;
            hoverImageDict[bnR2] = Properties.Resources.DS4_Config_R2;
            hoverImageDict[bnTouchLeft] = Properties.Resources.DS4_Config_TouchLeft;
            hoverImageDict[bnTouchRight] = Properties.Resources.DS4_Config_TouchRight;
            hoverImageDict[bnTouchMulti] = Properties.Resources.DS4_Config_TouchMulti;
            hoverImageDict[bnTouchUpper] = Properties.Resources.DS4_Config_TouchUpper;
            hoverImageDict[bnL3] = Properties.Resources.DS4_Config_LS;
            hoverImageDict[bnLSUp] = Properties.Resources.DS4_Config_LS;
            hoverImageDict[bnLSDown] = Properties.Resources.DS4_Config_LS;
            hoverImageDict[bnLSLeft] = Properties.Resources.DS4_Config_LS;
            hoverImageDict[bnLSRight] = Properties.Resources.DS4_Config_LS;
            hoverImageDict[bnR3] = Properties.Resources.DS4_Config_RS;
            hoverImageDict[bnRSUp] = Properties.Resources.DS4_Config_RS;
            hoverImageDict[bnRSDown] = Properties.Resources.DS4_Config_RS;
            hoverImageDict[bnRSLeft] = Properties.Resources.DS4_Config_RS;
            hoverImageDict[bnRSRight] = Properties.Resources.DS4_Config_RS;
        }

        private void populateHoverLabelDict()
        {
            hoverLabelDict.Clear();

            hoverLabelDict[bnCross] = lbLCross;
            hoverLabelDict[bnCircle] = lbLCircle;
            hoverLabelDict[bnSquare] = lbLSquare;
            hoverLabelDict[bnTriangle] = lbLTriangle;
            hoverLabelDict[bnOptions] = lbLOptions;
            hoverLabelDict[bnShare] = lbLShare;
            hoverLabelDict[bnUp] = lbLUp;
            hoverLabelDict[bnDown] = lbLDown;
            hoverLabelDict[bnLeft] = lbLLeft;
            hoverLabelDict[bnRight] = lbLright;
            hoverLabelDict[bnPS] = lbLPS;
            hoverLabelDict[bnL1] = lbLL1;
            hoverLabelDict[bnR1] = lbLR1;
            hoverLabelDict[bnL2] = lbLL2;
            hoverLabelDict[bnR2] = lbLR2;
            hoverLabelDict[bnTouchLeft] = lbLTouchLM;
            hoverLabelDict[bnTouchRight] = lbLTouchRight;
            hoverLabelDict[bnTouchMulti] = lbLTouchLM;
            hoverLabelDict[bnTouchUpper] = lbLTouchUpper;
            hoverLabelDict[bnL3] = lbLLS;
            hoverLabelDict[bnLSUp] = lbLLS;
            hoverLabelDict[bnLSDown] = lbLLS;
            hoverLabelDict[bnLSLeft] = lbLLS;
            hoverLabelDict[bnLSRight] = lbLLS;
            hoverLabelDict[bnR3] = lbLRS;
            hoverLabelDict[bnRSUp] = lbLRS;
            hoverLabelDict[bnRSDown] = lbLRS;
            hoverLabelDict[bnRSLeft] = lbLRS;
            hoverLabelDict[bnRSRight] = lbLRS;
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

            lVActions.ItemCheck -= this.lVActions_ItemCheck;

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
                
                btnLightBgImg = RecolorImage(btnLightBg, main);
                btnLightbar.Refresh();

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
                byte tempFlashType = FlashType[device];
                if (tempFlashType > cBFlashType.Items.Count - 1)
                    cBFlashType.SelectedIndex = 0;
                else
                    cBFlashType.SelectedIndex = tempFlashType;

                DS4Color fColor = FlashColor[device];
                if (fColor.Equals(new DS4Color { red = 0, green = 0, blue = 0 }))
                {
                    if (Rainbow[device] == 0)
                        btnFlashColor.BackColor = main;
                    else
                        btnFlashColor.BackgroundImage = rainbowImg;
                }
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

                tempInt = TouchpadInvert[device];
                touchpadInvertComboBox.SelectedIndex = touchpadInvertToValue[tempInt];

                cBlowerRCOn.Checked = LowerRCOn[device];
                cBFlushHIDQueue.Checked = FlushHIDQueue[device];
                enableTouchToggleCheckbox.Checked = getEnableTouchToggle(device);
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
                int tempWhileCharging = ChargingType[device];
                if (tempWhileCharging > cBWhileCharging.Items.Count - 1)
                    cBWhileCharging.SelectedIndex = 0;
                else
                    cBWhileCharging.SelectedIndex = tempWhileCharging;

                btPollRateComboBox.SelectedIndex = getBTPollRate(device);
                lsOutCurveComboBox.SelectedIndex = getLsOutCurveMode(device);
                rsOutCurveComboBox.SelectedIndex = getRsOutCurveMode(device);
                cBL2OutputCurve.SelectedIndex = getL2OutCurveMode(device);
                cBR2OutputCurve.SelectedIndex = getR2OutCurveMode(device);
                cBSixaxisXOutputCurve.SelectedIndex = getSXOutCurveMode(device);
                cBSixaxisZOutputCurve.SelectedIndex = getSZOutCurveMode(device);

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
                    nUDL2AntiDead.Value = (decimal)(L2AntiDeadzone[device] / 100d);
                }
                catch
                {
                    nUDL2AntiDead.Value = 0;
                }
                try
                {
                    nUDR2AntiDead.Value = (decimal)(R2AntiDeadzone[device] / 100d);
                }
                catch
                {
                    nUDR2AntiDead.Value = 0;
                }

                try
                {
                    nUDL2Maxzone.Value = (decimal)(L2Maxzone[device] / 100d);
                }
                catch
                {
                    nUDL2Maxzone.Value = 1;
                }
                try
                {
                    nUDR2Maxzone.Value = (decimal)(R2Maxzone[device] / 100d);
                }
                catch
                {
                    nUDR2Maxzone.Value = 1;
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
                    nUDLSAntiDead.Value = (decimal)(LSAntiDeadzone[device] / 100d);
                }
                catch
                {
                    nUDLSAntiDead.Value = 0;
                }
                try
                {
                    nUDRSAntiDead.Value = (decimal)(RSAntiDeadzone[device] / 100d);
                }
                catch
                {
                    nUDRSAntiDead.Value = 0;
                }

                try
                {
                    nUDLSMaxZone.Value = (decimal)(LSMaxzone[device] / 100d);
                }
                catch
                {
                    nUDLSMaxZone.Value = 1;
                }
                try
                {
                    nUDRSMaxZone.Value = (decimal)(RSMaxzone[device] / 100d);
                }
                catch
                {
                    nUDRSMaxZone.Value = 1;
                }

                try
                {
                    nUDLSRotation.Value = (decimal)(LSRotation[device] * 180.0 / Math.PI);
                }
                catch
                {
                    nUDLSRotation.Value = 0.0m;
                }

                try
                {
                    nUDRSRotation.Value = (decimal)(RSRotation[device] * 180.0 / Math.PI);
                }
                catch
                {
                    nUDRSRotation.Value = 0.0m;
                }

                try
                {
                    nUDSX.Value = (decimal)SXDeadzone[device];
                }
                catch
                {
                    nUDSX.Value = 0.02m;
                }

                try
                {
                    nUDSZ.Value = (decimal)SZDeadzone[device];
                }
                catch
                {
                    nUDSZ.Value = 0.02m;
                }

                try
                {
                    nUDSixAxisXMaxZone.Value = (decimal)SXMaxzone[device];
                }
                catch
                {
                    nUDSixAxisXMaxZone.Value = 1.0m;
                }

                try
                {
                    nUDSixAxisZMaxZone.Value = (decimal)SZMaxzone[device];
                }
                catch
                {
                    nUDSixAxisZMaxZone.Value = 1.0m;
                }

                try
                {
                    nUDSixaxisXAntiDead.Value = (decimal)SXAntiDeadzone[device];
                }
                catch
                {
                    nUDSixaxisXAntiDead.Value = 0.0m;
                }

                try
                {
                    nUDSixaxisZAntiDead.Value = (decimal)SZAntiDeadzone[device];
                }
                catch
                {
                    nUDSixaxisZAntiDead.Value = 0.0m;
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
                trackballCk.Checked = TrackballMode[device];
                trackFrictionNUD.Value = (decimal)TrackballFriction[device];
                if (device < 4)
                {
                    Program.rootHub.touchPad[device]?.ResetTrackAccel(TrackballFriction[device]);
                }

                for (int i = 0, arlen = cMGyroTriggers.Items.Count; i < arlen; i++)
                {
                    ((ToolStripMenuItem)cMGyroTriggers.Items[i]).Checked = false;
                }

                for (int i = 0, arlen = cMTouchDisableInvert.Items.Count; i < arlen; i++)
                {
                    ((ToolStripMenuItem)cMTouchDisableInvert.Items[i]).Checked = false;
                }

                string[] satriggers = SATriggers[device].Split(',');
                List<string> s = new List<string>();
                int gyroTriggerCount = cMGyroTriggers.Items.Count;
                for (int i = 0, satrigLen = satriggers.Length; i < satrigLen; i++)
                {
                    int tr = 0;
                    if (int.TryParse(satriggers[i], out tr))
                    {
                        if (tr < gyroTriggerCount && tr > -1)
                        {
                            ((ToolStripMenuItem)cMGyroTriggers.Items[tr]).Checked = true;
                            s.Add(cMGyroTriggers.Items[tr].Text);
                        }
                        else
                        {
                            ((ToolStripMenuItem)cMGyroTriggers.Items[gyroTriggerCount - 1]).Checked = true;
                            s.Add(cMGyroTriggers.Items[gyroTriggerCount - 1].Text);
                            break;
                        }
                    }
                    else
                    {
                        ((ToolStripMenuItem)cMGyroTriggers.Items[gyroTriggerCount - 1]).Checked = true;
                        s.Add(cMGyroTriggers.Items[gyroTriggerCount - 1].Text);
                        break;
                    }
                }

                btnGyroTriggers.Text = string.Join(", ", s);
                s.Clear();

                int[] touchDisInvTriggers = TouchDisInvertTriggers[device];
                int touchDisableInvCount = cMTouchDisableInvert.Items.Count;
                for (int i = 0, trigLen = touchDisInvTriggers.Length; i < trigLen; i++)
                {
                    int tr = touchDisInvTriggers[i];
                    if (tr < touchDisableInvCount && tr > -1)
                    {
                        ToolStripMenuItem current = (ToolStripMenuItem)cMTouchDisableInvert.Items[tr];
                        current.Checked = true;
                        s.Add(current.Text);
                    }
                }

                if (s.Count > 0)
                    touchpadDisInvertButton.Text = string.Join(", ", s);

                nUDGyroSensitivity.Value = GyroSensitivity[device];
                gyroTriggerBehavior.Checked = GyroTriggerTurns[device];
                nUDGyroMouseVertScale.Value = GyroSensVerticalScale[device];
                int invert = GyroInvert[device];
                cBGyroInvertX.Checked = (invert & 0x02) == 2;
                cBGyroInvertY.Checked = (invert & 0x01) == 1;
                if (s.Count > 0)
                    btnGyroTriggers.Text = string.Join(", ", s);

                cBGyroSmooth.Checked = nUDGyroSmoothWeight.Enabled = GyroSmoothing[device];
                nUDGyroSmoothWeight.Value = (decimal)(GyroSmoothingWeight[device]);
                cBGyroMouseXAxis.SelectedIndex = GyroMouseHorizontalAxis[device];
            }
            else
            {
                cBFlashType.SelectedIndex = 0;
                cBWhileCharging.SelectedIndex = 0;
                btPollRateComboBox.SelectedIndex = 4;
                lsOutCurveComboBox.SelectedIndex = 0;
                rsOutCurveComboBox.SelectedIndex = 0;
                cBL2OutputCurve.SelectedIndex = 0;
                cBR2OutputCurve.SelectedIndex = 0;
                cBSixaxisXOutputCurve.SelectedIndex = 0;
                cBSixaxisZOutputCurve.SelectedIndex = 0;
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
                touchpadInvertComboBox.SelectedIndex = 0;
                cBlowerRCOn.Checked = false;
                cBFlushHIDQueue.Checked = false;
                enableTouchToggleCheckbox.Checked = true;
                nUDIdleDisconnect.Value = 5;
                cBIdleDisconnect.Checked = true;
                numUDMouseSens.Value = 25;
                cBMouseAccel.Checked = true;
                pBHoveredButton.Image = null;

                nUDRainbow.Value = 0;
                nUDL2.Value = 0;
                nUDR2.Value = 0;
                nUDL2Maxzone.Value = 1;
                nUDR2Maxzone.Value = 1;
                nUDLS.Value = 0;
                nUDRS.Value = 0;
                nUDLSAntiDead.Value = 0;
                nUDRSAntiDead.Value = 0;
                nUDLSMaxZone.Value = 1;
                nUDRSMaxZone.Value = 1;
                nUDLSRotation.Value = 0;
                nUDRSRotation.Value = 0;
                nUDSX.Value = 0.02m;
                nUDSZ.Value = 0.02m;
                nUDSixAxisXMaxZone.Value = 1.0m;
                nUDSixAxisZMaxZone.Value = 1.0m;
                nUDSixaxisXAntiDead.Value = 0.0m;
                nUDSixaxisZAntiDead.Value = 0.0m;

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
                trackballCk.Checked = false;
                trackFrictionNUD.Value = 10.0m;
                if (device < 4)
                {
                    Program.rootHub.touchPad[device]?.ResetTrackAccel(10.0);
                }

                for (int i = 0, arlen = cMGyroTriggers.Items.Count - 1; i < arlen; i++)
                {
                    ((ToolStripMenuItem)cMGyroTriggers.Items[i]).Checked = false;
                }
                ((ToolStripMenuItem)cMGyroTriggers.Items[cMGyroTriggers.Items.Count - 1]).Checked = true;

                for (int i = 0, arlen = cMTouchDisableInvert.Items.Count; i < arlen; i++)
                {
                    ((ToolStripMenuItem)cMTouchDisableInvert.Items[i]).Checked = false;
                }

                nUDGyroSensitivity.Value = 100;
                nUDGyroMouseVertScale.Value = 100;
                gyroTriggerBehavior.Checked = true;
                cBGyroInvertX.Checked = false;
                cBGyroInvertY.Checked = false;
                cBGyroSmooth.Checked = false;
                nUDGyroSmoothWeight.Value = 0.5m;
                cBGyroMouseXAxis.SelectedIndex = 0;
                Set();
            }
            
            UpdateLists();
            LoadActions(string.IsNullOrEmpty(filename));
            lVActions.ItemCheck += new ItemCheckEventHandler(this.lVActions_ItemCheck);
            loading = false;
            saving = false;
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
                    case "Key": lvi.SubItems.Add(((Keys)Convert.ToInt32(action.details)).ToString() + (action.uTrigger.Count > 0 ? " (Toggle)" : "")); break;
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
                {
                    if (action.type == "DisconnectBT")
                        lvi.Checked = true;
                    else
                        lvi.Checked = false;
                }
                else
                {
                    foreach (string s in pactions)
                    {
                        if (s == action.name)
                        {
                            lvi.Checked = true;
                            break;
                        }
                    }
                }

                lVActions.Items.Add(lvi);
            }
        }

        private void EnableReadings(bool on)
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

        private void ControllerReadout_Tick(object sender, EventArgs e)
        {
            // MEMS gyro data is all calibrated to roughly -1G..1G for values -0x2000..0x1fff
            // Enough additional acceleration and we are no longer mostly measuring Earth's gravity...
            // We should try to indicate setpoints of the calibration when exposing this measurement....
            int tempDeviceNum = (int)nUDSixaxis.Value - 1;
            DS4Device ds = Program.rootHub.DS4Controllers[tempDeviceNum];

            if (ds == null)
            {
                EnableReadings(false);
                lbInputDelay.Text = Properties.Resources.InputDelay.Replace("*number*", Properties.Resources.NA);
                lbInputDelay.BackColor = Color.Transparent;
                lbInputDelay.ForeColor = SystemColors.ControlText;
            }
            else
            {
                EnableReadings(true);

                DS4StateExposed exposeState = Program.rootHub.ExposedState[tempDeviceNum];
                DS4State baseState = Program.rootHub.getDS4State(tempDeviceNum);

                SetDynamicTrackBarValue(tBsixaxisGyroX, (exposeState.getGyroYaw() + tBsixaxisGyroX.Value * 2) / 3);
                SetDynamicTrackBarValue(tBsixaxisGyroY, (exposeState.getGyroPitch() + tBsixaxisGyroY.Value * 2) / 3);
                SetDynamicTrackBarValue(tBsixaxisGyroZ, (exposeState.getGyroRoll() + tBsixaxisGyroZ.Value * 2) / 3);
                SetDynamicTrackBarValue(tBsixaxisAccelX, (exposeState.getAccelX() + tBsixaxisAccelX.Value * 2) / 3);
                SetDynamicTrackBarValue(tBsixaxisAccelY, (exposeState.getAccelY() + tBsixaxisAccelY.Value * 2) / 3);
                SetDynamicTrackBarValue(tBsixaxisAccelZ, (exposeState.getAccelZ() + tBsixaxisAccelZ.Value * 2) / 3);

                int x = baseState.LX;
                int y = baseState.LY;

                double tempLSS = (double)nUDLSS.Value;
                btnLSTrackS.Visible = tempLSS != 1;
                double tempLSCurve = (double)nUDLSCurve.Value;
                if (tempLSCurve > 0.0)
                {
                    float max = x + y;
                    double curvex;
                    double curvey;
                    double multimax = TValue(382.5, max, tempLSCurve);
                    double multimin = TValue(127.5, max, tempLSCurve);
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
                    btnLSTrackS.Visible = tempLSS != 1;
                }

                if (tempLSS != 1)
                {
                    btnLSTrackS.Location = new Point((int)(tempLSS * (btnLSTrack.Location.X - pnlLSTrack.Size.Width / 2f) + pnlLSTrack.Size.Width / 2f),
                        (int)(tempLSS * (btnLSTrack.Location.Y - pnlLSTrack.Size.Height / 2f) + pnlLSTrack.Size.Height / 2f));
                }

                x = baseState.RX;
                y = baseState.RY;

                double tempRSS = (double)nUDRSS.Value;
                btnRSTrackS.Visible = tempRSS != 1;
                double tempRSCurve = (double)nUDRSCurve.Value;
                if (tempRSCurve > 0.0)
                {
                    float max = x + y;
                    double curvex;
                    double curvey;
                    double multimax = TValue(382.5, max, tempRSCurve);
                    double multimin = TValue(127.5, max, tempRSCurve);
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
                    btnRSTrackS.Visible = tempRSS != 1;
                }

                if (tempRSS != 1)
                {
                    btnRSTrackS.Location = new Point((int)(tempRSS * (btnRSTrack.Location.X - pnlRSTrack.Size.Width / 2f) + pnlRSTrack.Size.Width / 2f),
                        (int)(tempRSS * (btnRSTrack.Location.Y - pnlRSTrack.Size.Height / 2f) + pnlRSTrack.Size.Height / 2f));
                }

                x = exposeState.getAccelX() + 127;
                y = exposeState.getAccelZ() + 127;
                btnSATrack.Location = new Point((int)(dpix * Global.Clamp(0, x / 2.09, pnlSATrack.Size.Width)), (int)(dpiy * Global.Clamp(0, y / 2.09, pnlSATrack.Size.Height)));

                double tempSXS = (double)nUDSXS.Value;
                double tempSZS = (double)nUDSZS.Value;
                btnSATrackS.Visible = tempSXS != 1 || tempSZS != 1;
                if (tempSXS != 1 || tempSZS != 1)
                {
                    btnSATrackS.Location = new Point((int)(tempSXS * (btnSATrack.Location.X - pnlSATrack.Size.Width / 2f) + pnlSATrack.Size.Width / 2f),
                        (int)(tempSZS * (btnSATrack.Location.Y - pnlSATrack.Size.Height / 2f) + pnlSATrack.Size.Height / 2f));
                }

                double tempL2 = (double)nUDL2.Value;
                double tempL2S = (double)nUDL2S.Value;
                tBL2.Value = baseState.L2;
                lbL2Track.Location = new Point(tBL2.Location.X - (int)(dpix * 25), 
                    Math.Max((int)(((tBL2.Location.Y + tBL2.Size.Height) - (tBL2.Value * tempL2S) / (tBL2.Size.Height * .0209f / Math.Pow(dpix, 2))) - dpix * 20),
                    (int)(1 * ((tBL2.Location.Y + tBL2.Size.Height) - 255 / (tBL2.Size.Height * .0209f / Math.Pow(dpix, 2))) - dpix * 20)));

                if (tBL2.Value * tempL2S >= 255)
                    lbL2Track.ForeColor = Color.Green;
                else if (tBL2.Value * tempL2S < tempL2 * 255)
                    lbL2Track.ForeColor = Color.Red;
                else
                    lbL2Track.ForeColor = Color.Black;

                double tempR2 = (double)nUDR2.Value;
                double tempR2S = (double)nUDR2S.Value;
                tBR2.Value = baseState.R2;
                lbR2Track.Location = new Point(tBR2.Location.X + (int)(dpix * 25),
                     Math.Max((int)(1 * ((tBR2.Location.Y + tBR2.Size.Height) - (tBR2.Value * tempR2S) / (tBR2.Size.Height * .0209f / Math.Pow(dpix, 2))) - dpix * 20),
                     (int)(1 * ((tBR2.Location.Y + tBR2.Size.Height) - 255 / (tBR2.Size.Height * .0209f / Math.Pow(dpix, 2))) - dpix * 20)));

                if (tBR2.Value * tempR2S >= 255)
                    lbR2Track.ForeColor = Color.Green;
                else if (tBR2.Value * tempR2S < tempR2 * 255)
                    lbR2Track.ForeColor = Color.Red;
                else
                    lbR2Track.ForeColor = Color.Black;

                double latency = ds.Latency;
                int warnInterval = ds.getWarnInterval();
                lbInputDelay.Text = Properties.Resources.InputDelay.Replace("*number*", latency.ToString());
                if (latency > warnInterval)
                {
                    lbInputDelay.BackColor = Color.Red;
                    lbInputDelay.ForeColor = Color.White;
                }
                else if (latency > (warnInterval * 0.5))
                {
                    lbInputDelay.BackColor = Color.Yellow;
                    lbInputDelay.ForeColor = Color.Black;
                }
                else
                {
                    lbInputDelay.BackColor = Color.Transparent;
                    lbInputDelay.ForeColor = SystemColors.ControlText;
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
            {
                int tempDeviceNum = (int)nUDSixaxis.Value - 1;
                switch (Program.rootHub.GetActiveInputControl(tempDeviceNum))
                {
                    case DS4Controls.None: break;
                    case DS4Controls.Cross: Show_ControlsBn(bnCross, e); break;
                    case DS4Controls.Circle: Show_ControlsBn(bnCircle, e); break;
                    case DS4Controls.Square: Show_ControlsBn(bnSquare, e); break;
                    case DS4Controls.Triangle: Show_ControlsBn(bnTriangle, e); break;
                    case DS4Controls.Options: Show_ControlsBn(bnOptions, e); break;
                    case DS4Controls.Share: Show_ControlsBn(bnShare, e); break;
                    case DS4Controls.DpadUp: Show_ControlsBn(bnUp, e); break;
                    case DS4Controls.DpadDown: Show_ControlsBn(bnDown, e); break;
                    case DS4Controls.DpadLeft: Show_ControlsBn(bnLeft, e); break;
                    case DS4Controls.DpadRight: Show_ControlsBn(bnRight, e); break;
                    case DS4Controls.PS: Show_ControlsBn(bnPS, e); break;
                    case DS4Controls.L1: Show_ControlsBn(bnL1, e); break;
                    case DS4Controls.R1: Show_ControlsBn(bnR1, e); break;
                    case DS4Controls.L2: Show_ControlsBn(bnL2, e); break;
                    case DS4Controls.R2: Show_ControlsBn(bnR2, e); break;
                    case DS4Controls.L3: Show_ControlsBn(bnL3, e); break;
                    case DS4Controls.R3: Show_ControlsBn(bnR3, e); break;
                    case DS4Controls.TouchLeft: Show_ControlsBn(bnTouchLeft, e); break;
                    case DS4Controls.TouchRight: Show_ControlsBn(bnTouchRight, e); break;
                    case DS4Controls.TouchMulti: Show_ControlsBn(bnTouchMulti, e); break;
                    case DS4Controls.TouchUpper: Show_ControlsBn(bnTouchUpper, e); break;
                    case DS4Controls.LYNeg: Show_ControlsBn(bnLSUp, e); break;
                    case DS4Controls.LYPos: Show_ControlsBn(bnLSDown, e); break;
                    case DS4Controls.LXNeg: Show_ControlsBn(bnLSLeft, e); break;
                    case DS4Controls.LXPos: Show_ControlsBn(bnLSRight, e); break;
                    case DS4Controls.RYNeg: Show_ControlsBn(bnRSUp, e); break;
                    case DS4Controls.RYPos: Show_ControlsBn(bnRSDown, e); break;
                    case DS4Controls.RXNeg: Show_ControlsBn(bnRSLeft, e); break;
                    case DS4Controls.RXPos: Show_ControlsBn(bnRSRight, e); break;
                    default: break;
                }
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
            Button senderControl = (Button)sender;
            string name = senderControl.Name;
            if (e != null)
            {
                int tempIndex = 0;
                if (hoverIndexDict.TryGetValue(senderControl, out tempIndex))
                {
                    lBControls.SelectedIndex = tempIndex;
                }
            }

            DS4ControlSettings dcs = getDS4CSetting(device, name);
            if (lBControls.SelectedIndex >= 0)
            { 
                string tipText = lBControls.SelectedItem.ToString().Split(':')[0];
                tipText += ": ";
                tipText += UpdateButtonList(senderControl);
                if (GetDS4Action(device, name, true) != null && GetDS4STrigger(device, name) > 0)
                {
                    tipText += "\n Shift: ";
                    tipText += ShiftTrigger(GetDS4STrigger(device, name)) + " -> " + UpdateButtonList(senderControl, true);
                }

                lbControlName.Text = tipText;
            }

            Bitmap tempBit = null;
            if (hoverImageDict.TryGetValue(senderControl, out tempBit))
            {
                pBHoveredButton.Image = tempBit;
            }

            Label tempLabel = null;
            if (hoverLabelDict.TryGetValue(senderControl, out tempLabel))
            {
                pBHoveredButton.Location = tempLabel.Location;
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

            BTPollRate[device] = btPollRateComboBox.SelectedIndex;
            lsOutCurveMode[device] = lsOutCurveComboBox.SelectedIndex;
            rsOutCurveMode[device] = rsOutCurveComboBox.SelectedIndex;
            l2OutCurveMode[device] = cBL2OutputCurve.SelectedIndex;
            r2OutCurveMode[device] = cBR2OutputCurve.SelectedIndex;
            sxOutCurveMode[device] = cBSixaxisXOutputCurve.SelectedIndex;
            szOutCurveMode[device] = cBSixaxisZOutputCurve.SelectedIndex;
            L2Deadzone[device] = (byte)Math.Round((nUDL2.Value * 255), 0);
            R2Deadzone[device] = (byte)Math.Round((nUDR2.Value * 255), 0);
            L2AntiDeadzone[device] = (int)(nUDL2AntiDead.Value * 100);
            R2AntiDeadzone[device] = (int)(nUDR2AntiDead.Value * 100);
            RumbleBoost[device] = (byte)nUDRumbleBoost.Value;
            TouchSensitivity[device] = (byte)nUDTouch.Value;
            TouchpadJitterCompensation[device] = cBTouchpadJitterCompensation.Checked;
            LowerRCOn[device] = cBlowerRCOn.Checked;
            ScrollSensitivity[device] = (int)nUDScroll.Value;
            DoubleTap[device] = cBDoubleTap.Checked;
            TapSensitivity[device] = (byte)nUDTap.Value;

            tempInt = touchpadInvertComboBox.SelectedIndex;
            TouchpadInvert[device] = touchpadInvertToValue[tempInt];

            IdleDisconnectTimeout[device] = (int)(nUDIdleDisconnect.Value * 60);
            Rainbow[device] = (int)nUDRainbow.Value;
            RSDeadzone[device] = (int)Math.Round((nUDRS.Value * 127), 0);
            LSDeadzone[device] = (int)Math.Round((nUDLS.Value * 127), 0);
            LSAntiDeadzone[device] = (int)(nUDLSAntiDead.Value * 100);
            RSAntiDeadzone[device] = (int)(nUDRSAntiDead.Value * 100);
            LSMaxzone[device] = (int)(nUDLSMaxZone.Value * 100);
            RSMaxzone[device] = (int)(nUDRSMaxZone.Value * 100);
            LSRotation[device] = (double)nUDLSRotation.Value * Math.PI / 180.0;
            RSRotation[device] = (double)nUDRSRotation.Value * Math.PI / 180.0;
            ButtonMouseSensitivity[device] = (int)numUDMouseSens.Value;
            FlashAt[device] = (int)nUDflashLED.Value;
            SXDeadzone[device] = (double)nUDSX.Value;
            SZDeadzone[device] = (double)nUDSZ.Value;
            SXMaxzone[device] = (double)nUDSixAxisXMaxZone.Value;
            SZMaxzone[device] = (double)nUDSixAxisZMaxZone.Value;
            SXAntiDeadzone[device] = (double)nUDSixaxisXAntiDead.Value;
            SZAntiDeadzone[device] = (double)nUDSixaxisZAntiDead.Value;
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
            {
                if (lvi.Checked)
                    pactions.Add(lvi.Text);
            }

            ProfileActions[device] = pactions;
            calculateProfileActionCount(device);
            calculateProfileActionDicts(device);
            cacheProfileCustomsFlags(device);
            pnlTPMouse.Visible = rBTPMouse.Checked;
            pnlSAMouse.Visible = rBSAMouse.Checked;
            fLPTiltControls.Visible = rBSAControls.Checked;
            fLPTouchSwipe.Visible = rBTPControls.Checked;
            TrackballMode[device] = trackballCk.Checked;
            TrackballFriction[device] = (double)trackFrictionNUD.Value;
            if (device < 4)
            {
                Program.rootHub.touchPad[device]?.ResetTrackAccel(TrackballFriction[device]);
            }

            GyroSensitivity[device] = (int)Math.Round(nUDGyroSensitivity.Value, 0);
            GyroTriggerTurns[device] = gyroTriggerBehavior.Checked;
            GyroSensVerticalScale[device] = (int)nUDGyroMouseVertScale.Value;
            GyroSmoothing[device] = cBGyroSmooth.Checked;
            GyroSmoothingWeight[device] = (double)nUDGyroSmoothWeight.Value;
            GyroMouseHorizontalAxis[device] = cBGyroMouseXAxis.SelectedIndex;

            int invert = 0;
            if (cBGyroInvertX.Checked)
                invert += 2;

            if (cBGyroInvertY.Checked)
                invert += 1;

            GyroInvert[device] = invert;

            List<int> ints = new List<int>();
            for (int i = 0, trigLen = cMGyroTriggers.Items.Count - 1; i < trigLen; i++)
            {
                if (((ToolStripMenuItem)cMGyroTriggers.Items[i]).Checked)
                    ints.Add(i);
            }

            if (ints.Count == 0)
                ints.Add(-1);

            SATriggers[device] = string.Join(",", ints);

            ints.Clear();
            for (int i = 0, trigLen = cMTouchDisableInvert.Items.Count; i < trigLen; i++)
            {
                if (((ToolStripMenuItem)cMTouchDisableInvert.Items[i]).Checked)
                    ints.Add(i);
            }

            if (ints.Count == 0)
                ints.Add(-1);

            TouchDisInvertTriggers[device] = ints.ToArray();

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

        public void ChangeButtonText(Control ctrl, bool shift, KeyValuePair<object, string> tag,
            bool SC, bool TG, bool MC, bool MR, int sTrigger = 0)
        {
            DS4KeyType kt = DS4KeyType.None;
            if (SC) kt |= DS4KeyType.ScanCode;
            if (TG) kt |= DS4KeyType.Toggle;
            if (MC) kt |= DS4KeyType.Macro;
            if (MR) kt |= DS4KeyType.HoldMacro;
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

        private void btnLightbar_Click(object sender, EventArgs e)
        {
            AdvancedColorDialog.ColorUpdateHandler tempDel =
                new AdvancedColorDialog.ColorUpdateHandler(advColorDialog_OnUpdateColor);

            advColorDialog.OnUpdateColor += tempDel;
            advColorDialog.Color = Color.FromArgb(tBRedBar.Value, tBGreenBar.Value, tBBlueBar.Value);
            advColorDialog_OnUpdateColor(main, e);
            if (advColorDialog.ShowDialog() == DialogResult.OK)
            {
                main = advColorDialog.Color;
                btnLightBgImg = RecolorImage(btnLightBg, main);
                btnLightbar.Refresh();
                if (FlashColor[device].Equals(new DS4Color { red = 0, green = 0, blue = 0 }))
                    btnFlashColor.BackColor = main;

                btnFlashColor.BackgroundImage = nUDRainbow.Enabled ? rainbowImg : null;
                tBRedBar.Value = advColorDialog.Color.R;
                tBGreenBar.Value = advColorDialog.Color.G;
                tBBlueBar.Value = advColorDialog.Color.B;
            }

            advColorDialog.OnUpdateColor -= tempDel;
            if (device < 4)
                DS4LightBar.forcelight[device] = false;
        }
        
        private void lowColorChooserButton_Click(object sender, EventArgs e)
        {
            Color chooserBackColor = lowColorChooserButton.BackColor;
            advColorDialog.Color = chooserBackColor;
            advColorDialog_OnUpdateColor(chooserBackColor, e);
            if (advColorDialog.ShowDialog() == DialogResult.OK)
            {
                lowColorChooserButton.BackColor = chooserBackColor = advColorDialog.Color;
                tBLowRedBar.Value = advColorDialog.Color.R;
                tBLowGreenBar.Value = advColorDialog.Color.G;
                tBLowBlueBar.Value = advColorDialog.Color.B;
            }

            if (device < 4)
                DS4LightBar.forcelight[device] = false;
        }

        private void btnChargingColor_Click(object sender, EventArgs e)
        {
            Color chargingBackColor = btnChargingColor.BackColor;
            advColorDialog.Color = chargingBackColor;
            advColorDialog_OnUpdateColor(chargingBackColor, e);
            if (advColorDialog.ShowDialog() == DialogResult.OK)
            {
                btnChargingColor.BackColor = chargingBackColor = advColorDialog.Color;
            }

            if (device < 4)
                DS4LightBar.forcelight[device] = false;

            ChargingColor[device] = new DS4Color(chargingBackColor);
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

            if (type == 0) // main
            {
                alphacolor = Math.Max(tBRedBar.Value, Math.Max(tBGreenBar.Value, tBBlueBar.Value));
                reg = Color.FromArgb(tBRedBar.Value, tBGreenBar.Value, tBBlueBar.Value);
                full = HuetoRGB(reg.GetHue(), reg.GetBrightness(), reg);
                main = Color.FromArgb((alphacolor > 205 ? 255 : (alphacolor + 50)), full);
                btnLightBgImg = RecolorImage(btnLightBg, main);
                btnLightbar.Refresh();
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
            if (!loading)
            {
                RumbleBoost[device] = (byte)nUDRumbleBoost.Value;
                byte h = (byte)Math.Min(255, (255 * nUDRumbleBoost.Value / 100));
                byte l = (byte)Math.Min(255, (255 * nUDRumbleBoost.Value / 100));
                bool hB = btnRumbleHeavyTest.Text == Properties.Resources.TestLText;
                bool lB = btnRumbleLightTest.Text == Properties.Resources.TestLText;
                Program.rootHub.setRumble((byte)(hB ? h : 0), (byte)(lB ? l : 0), device);
            }
        }

        private void btnRumbleHeavyTest_Click(object sender, EventArgs e)
        {
            int tempDeviceNum = (int)nUDSixaxis.Value - 1;
            DS4Device d = Program.rootHub.DS4Controllers[tempDeviceNum];
            if (d != null)
            {
                if (((Button)sender).Text == Properties.Resources.TestHText)
                {
                    Program.rootHub.setRumble((byte)Math.Min(255, (255 * nUDRumbleBoost.Value / 100)), d.RightLightFastRumble, tempDeviceNum);
                    ((Button)sender).Text = Properties.Resources.StopHText;
                }
                else
                {
                    Program.rootHub.setRumble(0, d.RightLightFastRumble, tempDeviceNum);
                    ((Button)sender).Text = Properties.Resources.TestHText;
                }
            }
        }

        private void btnRumbleLightTest_Click(object sender, EventArgs e)
        {
            int tempDeviceNum = (int)nUDSixaxis.Value - 1;
            DS4Device d = Program.rootHub.DS4Controllers[tempDeviceNum];
            if (d != null)
            {
                if (((Button)sender).Text == Properties.Resources.TestLText)
                {
                    Program.rootHub.setRumble(d.LeftHeavySlowRumble, (byte)Math.Min(255, (255 * nUDRumbleBoost.Value / 100)), tempDeviceNum);
                    ((Button)sender).Text = Properties.Resources.StopLText;
                }
                else
                {
                    Program.rootHub.setRumble(d.LeftHeavySlowRumble, 0, tempDeviceNum);
                    ((Button)sender).Text = Properties.Resources.TestLText;
                }
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
            bool lightByBatteryChecked = cBLightbyBattery.Checked;
            LedAsBatteryIndicator[device] = lightByBatteryChecked;
            pnlLowBattery.Visible = lightByBatteryChecked;
            //pnlFull.Location = new Point(pnlFull.Location.X, (lightByBatteryChecked ? (int)(dpix * 42) : (pnlFull.Location.Y + pnlLowBattery.Location.Y) / 2));
            lbFull.Text = (lightByBatteryChecked ? Properties.Resources.Full + ":" : Properties.Resources.Color + ":");
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
        }

        private void cBIdleDisconnect_CheckedChanged(object sender, EventArgs e)
        {
            bool idleDisconnectChecked = cBIdleDisconnect.Checked;
            if (idleDisconnectChecked)
                nUDIdleDisconnect.Value = 5;
            else
                nUDIdleDisconnect.Value = 0;

            nUDIdleDisconnect.Enabled = idleDisconnectChecked;
        }

        private void Options_FormClosing(object sender, FormClosingEventArgs e)
        {
            for (int i = 0; i < 4; i++)
                LoadProfile(i, false, Program.rootHub); // Refreshes all profiles in case other controllers are using the same profile

            if (btnRumbleHeavyTest.Text == Properties.Resources.StopText)
                Program.rootHub.setRumble(0, 0, (int)nUDSixaxis.Value - 1);

            if (saving)
            {
                if (device < 4)
                {
                    DS4Device tempDev = Program.rootHub.DS4Controllers[device];
                    if (tempDev != null)
                    {
                        int discon = getIdleDisconnectTimeout(device);
                        int btCurrentIndex = btPollRateComboBox.SelectedIndex;

                        tempDev.queueEvent(() =>
                        {
                            tempDev.setIdleTimeout(discon);
                            if (btCurrentIndex >= 0)
                            {
                                tempDev.setBTPollRate(btCurrentIndex);
                            }
                        });
                    }
                }
            }

            inputtimer.Stop();
            sixaxisTimer.Stop();
            root.OptionsClosed();
            lVActions.ItemCheck -= this.lVActions_ItemCheck;
            Visible = false;
            e.Cancel = true;
        }

        private void cBSlide_CheckedChanged(object sender, EventArgs e)
        {
            bool slideChecked = cBSlide.Checked;
            if (slideChecked)
                nUDTouch.Value = 100;
            else
                nUDTouch.Value = 0;

            nUDTouch.Enabled = slideChecked;
        }

        private void cBScroll_CheckedChanged(object sender, EventArgs e)
        {
            bool scrollChecked = cBScroll.Checked;
            if (scrollChecked)
                nUDScroll.Value = 5;
            else
                nUDScroll.Value = 0;

            nUDScroll.Enabled = scrollChecked;
        }

        private void cBTap_CheckedChanged(object sender, EventArgs e)
        {
            bool tapChecked = cBTap.Checked;
            if (tapChecked)
                nUDTap.Value = 100;
            else
                nUDTap.Value = 0;

            nUDTap.Enabled = tapChecked;
            cBDoubleTap.Enabled = tapChecked;
        }

        private void cBDoubleTap_CheckedChanged(object sender, EventArgs e)
        {
            DoubleTap[device] = cBDoubleTap.Checked;
        }

        public void UpdateLists()
        {
            lBControls.BeginUpdate();
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

            lBControls.EndUpdate();

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
                    //return (Keys)int.Parse(tagO.ToString()) + (SC ? " (" + Properties.Resources.ScanCode + ")" : "");
                    return (Keys)Convert.ToInt32(tagO) + (SC ? " (" + Properties.Resources.ScanCode + ")" : "");
                }
                else if (tagO is int[])
                {
                    return Properties.Resources.Macro + (SC ? " (" + Properties.Resources.ScanCode + ")" : "");
                }
                else if (tagO is X360Controls)
                {
                    string tag;
                    tag = KBM360.getX360ControlsByName((X360Controls)tagO);
                    return tag;
                }
                else if (tagO is string)
                {
                    string tag;
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
            int controlSelectedIndex = lBControls.SelectedIndex;

            if (controlSelectedIndex == 0) Show_ControlsBn(bnCross, e);
            else if (controlSelectedIndex == 1) Show_ControlsBn(bnCircle, e);
            else if (controlSelectedIndex == 2) Show_ControlsBn(bnSquare, e);
            else if (controlSelectedIndex == 3) Show_ControlsBn(bnTriangle, e);
            else if (controlSelectedIndex == 4) Show_ControlsBn(bnOptions, e);
            else if (controlSelectedIndex == 5) Show_ControlsBn(bnShare, e);
            else if (controlSelectedIndex == 6) Show_ControlsBn(bnUp, e);
            else if (controlSelectedIndex == 7) Show_ControlsBn(bnDown, e);
            else if (controlSelectedIndex == 8) Show_ControlsBn(bnLeft, e);
            else if (controlSelectedIndex == 9) Show_ControlsBn(bnRight, e);
            else if (controlSelectedIndex == 10) Show_ControlsBn(bnPS, e);
            else if (controlSelectedIndex == 11) Show_ControlsBn(bnL1, e);
            else if (controlSelectedIndex == 12) Show_ControlsBn(bnR1, e);
            else if (controlSelectedIndex == 13) Show_ControlsBn(bnL2, e);
            else if (controlSelectedIndex == 14) Show_ControlsBn(bnR2, e);
            else if (controlSelectedIndex == 15) Show_ControlsBn(bnL3, e);
            else if (controlSelectedIndex == 16) Show_ControlsBn(bnR3, e);

            else if (controlSelectedIndex == 17) Show_ControlsBn(bnTouchLeft, e);
            else if (controlSelectedIndex == 18) Show_ControlsBn(bnTouchRight, e);
            else if (controlSelectedIndex == 19) Show_ControlsBn(bnTouchMulti, e);
            else if (controlSelectedIndex == 20) Show_ControlsBn(bnTouchUpper, e);

            else if (controlSelectedIndex == 21) Show_ControlsBn(bnLSUp, e);
            else if (controlSelectedIndex == 22) Show_ControlsBn(bnLSDown, e);
            else if (controlSelectedIndex == 23) Show_ControlsBn(bnLSLeft, e);
            else if (controlSelectedIndex == 24) Show_ControlsBn(bnLSRight, e);
            else if (controlSelectedIndex == 25) Show_ControlsBn(bnRSUp, e);
            else if (controlSelectedIndex == 26) Show_ControlsBn(bnRSDown, e);
            else if (controlSelectedIndex == 27) Show_ControlsBn(bnRSLeft, e);
            else if (controlSelectedIndex == 28) Show_ControlsBn(bnRSRight, e);

            else if (controlSelectedIndex == 29) Show_ControlsBn(bnGyroZN, e);
            else if (controlSelectedIndex == 30) Show_ControlsBn(bnGyroZP, e);
            else if (controlSelectedIndex == 31) Show_ControlsBn(bnGyroXP, e);
            else if (controlSelectedIndex == 32) Show_ControlsBn(bnGyroXN, e);

            else if (controlSelectedIndex == 33) Show_ControlsBn(bnSwipeUp, e);
            else if (controlSelectedIndex == 34) Show_ControlsBn(bnSwipeDown, e);
            else if (controlSelectedIndex == 35) Show_ControlsBn(bnSwipeLeft, e);
            else if (controlSelectedIndex == 36) Show_ControlsBn(bnSwipeRight, e);
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
            double tempRainbow = (double)nUDRainbow.Value;
            Rainbow[device] = tempRainbow;
            if (tempRainbow <= 0.5)
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
                btnLightBgImg = RecolorImage(btnLightBg, main);
                btnLightbar.Refresh();
                cBLightbyBattery.Text = Properties.Resources.DimByBattery.Replace("*nl*", "\n");
            }
            else
            {
                pnlLowBattery.Enabled = cBLightbyBattery.Checked;
                btnLightBgImg = RecolorImage(btnLightBg, main);
                btnLightbar.Refresh();
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

            for (int i = 0, bitwidth = c.Width; i < bitwidth; i++)
            {
                for (int x = 0, bitheight = c.Height; x < bitheight; x++)
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
            Bitmap c = image;
            Bitmap d = new Bitmap(c.Width, c.Height);
            bool rainEnabled = nUDRainbow.Enabled;

            for (int i = 0, bitwidth = c.Width; i < bitwidth; i++)
            {
                for (int x = 0, bitheight = c.Height; x < bitheight; x++)
                {
                    if (!rainEnabled)
                    {
                        Color col = c.GetPixel(i, x);
                        col = Color.FromArgb((int)(col.A * (color.A / 255f)), color.R, color.G, color.B);
                        d.SetPixel(i, x, col);
                    }
                    else
                    {
                        Color col = HuetoRGB((i / (float)bitwidth) * 360, .5f, Color.Red);
                        d.SetPixel(i, x, Color.FromArgb(c.GetPixel(i, x).A, col));
                    }
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
            int index = tCControls.SelectedIndex;
            if (index == 2)
                sixaxisTimer.Start();
            else
                sixaxisTimer.Stop();

            if (index == 1)
            {
                actionTabSeen = true;
            }
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
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
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

        private void CBDinput_CheckedChanged(object sender, EventArgs e)
        {
            DinputOnly[device] = cBDinput.Checked;
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
                case "btPollRateLabel": root.lbLastMessage.Text = Properties.Resources.BTPollRate; break;
                case "btPollRateComboBox": root.lbLastMessage.Text = Properties.Resources.BTPollRate; break;
                case "nUDSixaxis": root.lbLastMessage.Text = Properties.Resources.UseControllerForMapping; break;
                case "cBControllerInput": root.lbLastMessage.Text = Properties.Resources.UseControllerForMapping; break;
                case "lbUseController": root.lbLastMessage.Text = Properties.Resources.UseControllerForMapping; break;
                case "gyroTriggerBehavior": root.lbLastMessage.Text = Properties.Resources.GyroTriggerBehavior; break;
                default: root.lbLastMessage.Text = Properties.Resources.HoverOverItems; break;
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
                lBControls.BeginUpdate();
                lBControls.Items.RemoveAt(36);
                lBControls.Items.RemoveAt(35);
                lBControls.Items.RemoveAt(34);
                lBControls.Items.RemoveAt(33);
                lBControls.EndUpdate();
            }
        }

        private void cBControllerInput_CheckedChanged(object sender, EventArgs e)
        {
            DS4Mapping = cBControllerInput.Checked;
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
                calculateProfileActionCount(device);
                calculateProfileActionDicts(device);
                cacheProfileCustomsFlags(device);
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

        private void SetPreset(object sender, EventArgs e)
        {
            bool scancode = false;
            KeyValuePair<object, string> tagU;
            KeyValuePair<object, string> tagL;
            KeyValuePair<object, string> tagR;
            KeyValuePair<object, string> tagD;
            KeyValuePair<object, string> tagM = new KeyValuePair<object, string>(null, "0,0,0,0,0,0,0,0");

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
            string toolStripMenuText = controlToolStripMenuItem.Text;
            if (toolStripMenuText == "Dpad")
            {
                button1 = bnUp;
                button2 = bnLeft;
                button3 = bnRight;
                button4 = bnDown;
            }
            else if (toolStripMenuText == "Left Stick")
            {
                button1 = bnLSUp;
                button2 = bnLSLeft;
                button3 = bnLSRight;
                button4 = bnLSDown;
                button5 = bnL3;
            }
            else if (toolStripMenuText == "Right Stick")
            {
                button1 = bnRSUp;
                button2 = bnRSLeft;
                button3 = bnRSRight;
                button4 = bnRSDown;
                button5 = bnR3;
            }
            else if (toolStripMenuText == "Face Buttons")
            {
                button1 = bnTriangle;
                button2 = bnSquare;
                button3 = bnCircle;
                button4 = bnCross;
            }
            else if (toolStripMenuText == "Sixaxis")
            {
                button1 = bnGyroZN;
                button2 = bnGyroXP;
                button3 = bnGyroXN;
                button4 = bnGyroZP;
            }
            else if (toolStripMenuText == "Touchpad Swipes")
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
            Control button = (Control)sender;
            cMGyroTriggers.Show(button, new Point(0, button.Height));
        }

        private void SATrigger_CheckedChanged(object sender, EventArgs e)
        {
            if (loading == false)
            {
                int gyroTriggerCount = cMGyroTriggers.Items.Count;
                if (sender != cMGyroTriggers.Items[gyroTriggerCount - 1] && ((ToolStripMenuItem)sender).Checked)
                    ((ToolStripMenuItem)cMGyroTriggers.Items[gyroTriggerCount - 1]).Checked = false;

                if (((ToolStripMenuItem)cMGyroTriggers.Items[gyroTriggerCount - 1]).Checked) //always on
                {
                    for (int i = 0; i < gyroTriggerCount - 1; i++)
                        ((ToolStripMenuItem)cMGyroTriggers.Items[i]).Checked = false;
                }

                List<int> ints = new List<int>();
                List<string> s = new List<string>();
                for (int i = 0; i < gyroTriggerCount - 1; i++)
                {
                    if (((ToolStripMenuItem)cMGyroTriggers.Items[i]).Checked)
                    {
                        ints.Add(i);
                        s.Add(cMGyroTriggers.Items[i].Text);
                    }
                }

                if (ints.Count == 0)
                {
                    ints.Add(-1);
                    s.Add(cMGyroTriggers.Items[gyroTriggerCount - 1].Text);
                }

                SATriggers[device] = string.Join(",", ints);
                if (s.Count > 0)
                    btnGyroTriggers.Text = string.Join(", ", s);
            }
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

        private void nUDLSAntiDead_ValueChanged(object sender, EventArgs e)
        {
            LSAntiDeadzone[device] = (int)(nUDLSAntiDead.Value * 100);
        }

        private void nUDRSAntiDead_ValueChanged(object sender, EventArgs e)
        {
            RSAntiDeadzone[device] = (int)(nUDRSAntiDead.Value * 100);
        }

        private void nUDL2AntiDead_ValueChanged(object sender, EventArgs e)
        {
            L2AntiDeadzone[device] = (int)(nUDL2AntiDead.Value * 100);
        }

        private void nUDR2AntiDead_ValueChanged(object sender, EventArgs e)
        {
            R2AntiDeadzone[device] = (int)(nUDR2AntiDead.Value * 100);
        }

        private void lVActions_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (actionTabSeen)
            {
                List<string> pactions = new List<string>();
                foreach (ListViewItem lvi in lVActions.Items)
                {
                    if (lvi != null && lvi.Checked)
                        pactions.Add(lvi.Text);
                }

                ProfileActions[device] = pactions;
                calculateProfileActionCount(device);
                calculateProfileActionDicts(device);
                cacheProfileCustomsFlags(device);
            }
        }

        private void enableTouchToggleCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            EnableTouchToggle[device] = enableTouchToggleCheckbox.Checked;
        }

        private void nUDRSMaxZone_ValueChanged(object sender, EventArgs e)
        {
            RSMaxzone[device] = (int)(nUDRSMaxZone.Value * 100);
        }

        private void nUDLSMaxZone_ValueChanged(object sender, EventArgs e)
        {
            LSMaxzone[device] = (int)(nUDLSMaxZone.Value * 100);
        }

        private void nUDL2Maxzone_ValueChanged(object sender, EventArgs e)
        {
            L2Maxzone[device] = (int)(nUDL2Maxzone.Value * 100);
        }

        private void nUDR2Maxzone_ValueChanged(object sender, EventArgs e)
        {
            R2Maxzone[device] = (int)(nUDR2Maxzone.Value * 100);
        }

        private void btPollRateComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            int currentIndex = btPollRateComboBox.SelectedIndex;
            BTPollRate[device] = currentIndex;
        }

        private void nUDL2Sens_ValueChanged(object sender, EventArgs e)
        {
            if (!loading)
            {
                L2Sens[device] = (double)nUDL2S.Value;
            }
        }

        private void nUDLSSens_ValueChanged(object sender, EventArgs e)
        {
            if (!loading)
            {
                LSSens[device] = (double)nUDLSS.Value;
            }
        }

        private void nUDR2Sens_ValueChanged(object sender, EventArgs e)
        {
            if (!loading)
            {
                R2Sens[device] = (double)nUDR2S.Value;
            }
        }

        private void nUDRSSens_ValueChanged(object sender, EventArgs e)
        {
            if (!loading)
            {
                RSSens[device] = (double)nUDRSS.Value;
            }
        }

        private void nUDSXSens_ValueChanged(object sender, EventArgs e)
        {
            if (!loading)
            {
                SXSens[device] = (double)nUDSXS.Value;
            }
        }

        private void nUDSZSens_ValueChanged(object sender, EventArgs e)
        {
            if (!loading)
            {
                SZSens[device] = (double)nUDSZS.Value;
            }
        }

        private void lsOutCurveComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!loading)
            {
                lsOutCurveMode[device] = lsOutCurveComboBox.SelectedIndex;
            }
        }

        private void rsOutCurveComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!loading)
            {
                rsOutCurveMode[device] = rsOutCurveComboBox.SelectedIndex;
            }
        }

        private void gyroTriggerBehavior_CheckedChanged(object sender, EventArgs e)
        {
            if (!loading)
            {
                GyroTriggerTurns[device] = gyroTriggerBehavior.Checked;
            }
        }

        private void nUDGyroMouseVertScale_ValueChanged(object sender, EventArgs e)
        {
            if (!loading)
            {
                GyroSensVerticalScale[device] = (int)nUDGyroMouseVertScale.Value;
            }
        }

        private void nUDGyroSmoothWeight_ValueChanged(object sender, EventArgs e)
        {
            if (!loading)
            {
                GyroSmoothingWeight[device] = (double)nUDGyroSmoothWeight.Value;
            }
        }

        private void cBGyroSmooth_CheckedChanged(object sender, EventArgs e)
        {
            bool value = cBGyroSmooth.Checked;
            nUDGyroSmoothWeight.Enabled = value;
            if (!loading)
            {
                GyroSmoothing[device] = value;
            }
        }

        private void nUDLSRotation_ValueChanged(object sender, EventArgs e)
        {
            if (!loading)
            {
                LSRotation[device] = (double)nUDLSRotation.Value * Math.PI / 180.0;
            }
        }

        private void nUDRSRotation_ValueChanged(object sender, EventArgs e)
        {
            if (!loading)
            {
                RSRotation[device] = (double)nUDRSRotation.Value * Math.PI / 180.0;
            }
        }

        private void touchpadInvertComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!loading)
            {
                tempInt = touchpadInvertToValue[touchpadInvertComboBox.SelectedIndex];
                TouchpadInvert[device] = tempInt;
            }
        }

        private void cBGyroMouseXAxis_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!loading)
            {
                GyroMouseHorizontalAxis[device] = cBGyroMouseXAxis.SelectedIndex;
            }
        }

        private void nUDSixAxisXMaxZone_ValueChanged(object sender, EventArgs e)
        {
            if (!loading)
            {
                SXMaxzone[device] = (double)nUDSixAxisXMaxZone.Value;
            }
        }

        private void nUDSixAxisZMaxZone_ValueChanged(object sender, EventArgs e)
        {
            if (!loading)
            {
                SZMaxzone[device] = (double)nUDSixAxisZMaxZone.Value;
            }
        }

        private void nUDSixaxisXAntiDead_ValueChanged(object sender, EventArgs e)
        {
            if (loading == false)
            {
                SXAntiDeadzone[device] = (double)nUDSixaxisXAntiDead.Value;
            }
        }

        private void nUDSixaxisZAntiDead_ValueChanged(object sender, EventArgs e)
        {
            if (loading == false)
            {
                SZAntiDeadzone[device] = (double)nUDSixaxisZAntiDead.Value;
            }
        }

        private void cBL2OutputCurve_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (loading == false)
            {
                l2OutCurveMode[device] = cBL2OutputCurve.SelectedIndex;
            }
        }

        private void cBR2OutputCurve_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (loading == false)
            {
                r2OutCurveMode[device] = cBR2OutputCurve.SelectedIndex;
            }
        }

        private void cBSixaxisXOutputCurve_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (loading == false)
            {
                sxOutCurveMode[device] = cBSixaxisXOutputCurve.SelectedIndex;
            }
        }

        private void cBSixaxisZOutputCurve_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (loading == false)
            {
                szOutCurveMode[device] = cBSixaxisZOutputCurve.SelectedIndex;
            }
        }

        private void TouchDisableInvert_CheckedChanged(object sender, EventArgs e)
        {
            if (loading == false)
            {
                int touchDisableInvCount = cMTouchDisableInvert.Items.Count;

                List<int> ints = new List<int>();
                List<string> s = new List<string>();
                for (int i = 0; i < touchDisableInvCount; i++)
                {
                    ToolStripMenuItem current = (ToolStripMenuItem)cMTouchDisableInvert.Items[i];
                    if (current.Checked)
                    {
                        ints.Add(i);
                        s.Add(current.Text);
                    }
                }

                if (ints.Count == 0)
                {
                    ints.Add(-1);
                    s.Add("None");
                }

                TouchDisInvertTriggers[device] = ints.ToArray();
                if (s.Count > 0)
                    touchpadDisInvertButton.Text = string.Join(", ", s);
            }
        }

        private void touchpadDisInvertButton_Click(object sender, EventArgs e)
        {
            Control button = (Control)sender;
            cMTouchDisableInvert.Show(button, new Point(0, button.Height));
        }

        private void pnlController_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.DrawImage(pnlControllerBgImg, 0, 0, Convert.ToInt32(pnlController.Width), Convert.ToInt32(pnlController.Height - 1));
        }

        private void trackballCk_CheckedChanged(object sender, EventArgs e)
        {
            if (loading == false)
            {
                TrackballMode[device] = trackballCk.Checked;
            }
        }

        private void trackFrictionNUD_ValueChanged(object sender, EventArgs e)
        {
            if (loading == false)
            {
                TrackballFriction[device] = (double)trackFrictionNUD.Value;
                if (device < 4)
                {
                    Program.rootHub.touchPad[device]?.ResetTrackAccel(TrackballFriction[device]);
                }
            }
        }

        private void btnLightbar_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.DrawImage(btnLightBgImg, new Rectangle(0, -1, Convert.ToInt32(btnLightbar.Width), Convert.ToInt32(btnLightbar.Height - 2)));
        }

        private void Options_Resize(object sender, EventArgs e)
        {
            fLPSettings.AutoScroll = false;
            fLPSettings.AutoScroll = true;
        }

        private void lBControls_SelectedIndexChanged(object sender, EventArgs e)
        {
            int controlSelectedIndex = lBControls.SelectedIndex;

            if (lBControls.SelectedItem != null)
            {
                if (controlSelectedIndex == 0)
                    lbControlName.ForeColor = Color.FromArgb(153, 205, 204);
                else if (controlSelectedIndex == 1)
                    lbControlName.ForeColor = Color.FromArgb(247, 131, 150);
                else if (controlSelectedIndex == 2)
                    lbControlName.ForeColor = Color.FromArgb(237, 170, 217);
                else if (controlSelectedIndex == 3)
                    lbControlName.ForeColor = Color.FromArgb(75, 194, 202);
                else
                    lbControlName.ForeColor = Color.White;
            }

            if (controlSelectedIndex == 0) button_MouseHover(bnCross, null);
            else if (controlSelectedIndex == 1) button_MouseHover(bnCircle, null);
            else if (controlSelectedIndex == 2) button_MouseHover(bnSquare, null);
            else if (controlSelectedIndex == 3) button_MouseHover(bnTriangle, null);
            else if (controlSelectedIndex == 4) button_MouseHover(bnOptions, null);
            else if (controlSelectedIndex == 5) button_MouseHover(bnShare, null);
            else if (controlSelectedIndex == 6) button_MouseHover(bnUp, null);
            else if (controlSelectedIndex == 7) button_MouseHover(bnDown, null);
            else if (controlSelectedIndex == 8) button_MouseHover(bnLeft, null);
            else if (controlSelectedIndex == 9) button_MouseHover(bnRight, null);
            else if (controlSelectedIndex == 10) button_MouseHover(bnPS, null);
            else if (controlSelectedIndex == 11) button_MouseHover(bnL1, null);
            else if (controlSelectedIndex == 12) button_MouseHover(bnR1, null);
            else if (controlSelectedIndex == 13) button_MouseHover(bnL2, null);
            else if (controlSelectedIndex == 14) button_MouseHover(bnR2, null);
            else if (controlSelectedIndex == 15) button_MouseHover(bnL3, null);
            else if (controlSelectedIndex == 16) button_MouseHover(bnR3, null);

            else if (controlSelectedIndex == 17) button_MouseHover(bnTouchLeft, null);
            else if (controlSelectedIndex == 18) button_MouseHover(bnTouchRight, null);
            else if (controlSelectedIndex == 19) button_MouseHover(bnTouchMulti, null);
            else if (controlSelectedIndex == 20) button_MouseHover(bnTouchUpper, null);

            else if (controlSelectedIndex == 21) button_MouseHover(bnLSUp, null);
            else if (controlSelectedIndex == 22) button_MouseHover(bnLSDown, null);
            else if (controlSelectedIndex == 23) button_MouseHover(bnLSLeft, null);
            else if (controlSelectedIndex == 24) button_MouseHover(bnLSRight, null);
            else if (controlSelectedIndex == 25) button_MouseHover(bnRSUp, null);
            else if (controlSelectedIndex == 26) button_MouseHover(bnRSDown, null);
            else if (controlSelectedIndex == 27) button_MouseHover(bnRSLeft, null);
            else if (controlSelectedIndex == 28) button_MouseHover(bnRSRight, null);

            else if (controlSelectedIndex == 29) button_MouseHover(bnGyroZN, null);
            else if (controlSelectedIndex == 30) button_MouseHover(bnGyroZP, null);
            else if (controlSelectedIndex == 31) button_MouseHover(bnGyroXP, null);
            else if (controlSelectedIndex == 32) button_MouseHover(bnGyroXN, null);

            else if (controlSelectedIndex == 33) button_MouseHover(bnSwipeUp, null);
            else if (controlSelectedIndex == 34) button_MouseHover(bnSwipeDown, null);
            else if (controlSelectedIndex == 35) button_MouseHover(bnSwipeLeft, null);
            else if (controlSelectedIndex == 36) button_MouseHover(bnSwipeRight, null);
        }
        
        private void nUDGyroSensitivity_ValueChanged(object sender, EventArgs e)
        {
            GyroSensitivity[device] = (int)Math.Round(nUDGyroSensitivity.Value, 0);
        }

        private void cBFlashType_SelectedIndexChanged(object sender, EventArgs e)
        {
            FlashType[device] = (byte)cBFlashType.SelectedIndex;
        }
    }
}
