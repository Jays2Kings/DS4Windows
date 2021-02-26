using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using NonFormTimer = System.Timers.Timer;
using DS4WinWPF.DS4Forms.ViewModels;
using DS4Windows;

namespace DS4WinWPF.DS4Forms
{
    /// <summary>
    /// Interaction logic for ProfileEditor.xaml
    /// </summary>
    public partial class ProfileEditor : UserControl
    {
        private class HoverImageInfo
        {
            public Point point;
            public Size size;
        }

        private int deviceNum;
        private ProfileSettingsViewModel profileSettingsVM;
        private MappingListViewModel mappingListVM;
        private ProfileEntity currentProfile;
        private SpecialActionsListViewModel specialActionsVM;

        public event EventHandler Closed;
        public delegate void CreatedProfileHandler(ProfileEditor sender, string profile);
        public event CreatedProfileHandler CreatedProfile;

        private Dictionary<Button, ImageBrush> hoverImages =
            new Dictionary<Button, ImageBrush>();
        private Dictionary<Button, HoverImageInfo> hoverLocations = new Dictionary<Button, HoverImageInfo>();
        private Dictionary<Button, int> hoverIndexes = new Dictionary<Button, int>();
        private Dictionary<int, Button> reverseHoverIndexes = new Dictionary<int, Button>();

        private StackPanel activeGyroModePanel;
        private bool keepsize;
        private bool controllerReadingsTabActive = false;
        public bool Keepsize { get => keepsize; }
        public int DeviceNum { get => deviceNum; }

        private NonFormTimer inputTimer;

        public ProfileEditor(int device)
        {
            InitializeComponent();

            deviceNum = device;
            emptyColorGB.Visibility = Visibility.Collapsed;
            profileSettingsVM = new ProfileSettingsViewModel(device);
            picBoxHover.Visibility = Visibility.Hidden;
            picBoxHover2.Visibility = Visibility.Hidden;

            activeGyroModePanel = gyroControlsPanel;

            mappingListVM = new MappingListViewModel(deviceNum, profileSettingsVM.ContType);
            specialActionsVM = new SpecialActionsListViewModel(device);

            RemoveHoverBtnText();
            PopulateHoverImages();
            PopulateHoverLocations();
            PopulateHoverIndexes();
            PopulateReverseHoverIndexes();

            ColorByBatteryPerCheck();
            AssignTiltAssociation();
            AssignSwipeAssociation();
            AssignTriggerFullPullAssociation();
            AssignGyroSwipeAssociation();

            inputTimer = new NonFormTimer(100);
            inputTimer.Elapsed += InputDS4;
            SetupEvents();
        }

        private void SetupEvents()
        {
            gyroOutModeCombo.SelectionChanged += GyroOutModeCombo_SelectionChanged;
            outConTypeCombo.SelectionChanged += OutConTypeCombo_SelectionChanged;
            mappingListBox.SelectionChanged += MappingListBox_SelectionChanged;
            Closed += ProfileEditor_Closed;
            profileSettingsVM.LSDeadZoneChanged += UpdateReadingsLsDeadZone;
            profileSettingsVM.RSDeadZoneChanged += UpdateReadingsRsDeadZone;
            profileSettingsVM.L2DeadZoneChanged += UpdateReadingsL2DeadZone;
            profileSettingsVM.R2DeadZoneChanged += UpdateReadingsR2DeadZone;
            profileSettingsVM.SXDeadZoneChanged += UpdateReadingsSXDeadZone;
            profileSettingsVM.SZDeadZoneChanged += UpdateReadingsSZDeadZone;
        }

        private void SetupGyroPanel()
        {
            switch (profileSettingsVM.GyroOutModeIndex)
            {
                case 0:
                    activeGyroModePanel = gyroControlsPanel; break;
                case 1:
                    activeGyroModePanel = gyroMousePanel; break;
                case 2:
                    activeGyroModePanel = gyroMouseJoystickPanel; break;
                case 3:
                    activeGyroModePanel = gyroDirSwipePanel; break;
                case 4:
                    activeGyroModePanel = passthruPanel; break;
                default:
                    activeGyroModePanel = gyroControlsPanel; break;
            }

            //activeGyroModePanel = gyroControlsPanel;
            gyroControlsPanel.Visibility = Visibility.Collapsed;
            gyroMousePanel.Visibility = Visibility.Collapsed;
            gyroMouseJoystickPanel.Visibility = Visibility.Collapsed;
            gyroDirSwipePanel.Visibility = Visibility.Collapsed;
            passthruPanel.Visibility = Visibility.Collapsed;
            activeGyroModePanel.Visibility = Visibility.Visible;
        }

        private void UpdateReadingsSZDeadZone(object sender, EventArgs e)
        {
            conReadingsUserCon.SixAxisZDead = profileSettingsVM.SZDeadZone;
        }

        private void UpdateReadingsSXDeadZone(object sender, EventArgs e)
        {
            conReadingsUserCon.SixAxisXDead = profileSettingsVM.SXDeadZone;
        }

        private void UpdateReadingsR2DeadZone(object sender, EventArgs e)
        {
            conReadingsUserCon.R2Dead = profileSettingsVM.R2DeadZone;
        }

        private void UpdateReadingsL2DeadZone(object sender, EventArgs e)
        {
            conReadingsUserCon.L2Dead = profileSettingsVM.L2DeadZone;
        }

        private void UpdateReadingsRsDeadZone(object sender, EventArgs e)
        {
            conReadingsUserCon.RsDead = profileSettingsVM.RSDeadZone;
        }

        private void UpdateReadingsLsDeadZone(object sender, EventArgs e)
        {
            conReadingsUserCon.LsDead = profileSettingsVM.LSDeadZone;
        }

        private void AssignTiltAssociation()
        {
            gyroZNLb.DataContext = mappingListVM.ControlMap[DS4Windows.DS4Controls.GyroZNeg];
            gyroZPLb.DataContext = mappingListVM.ControlMap[DS4Windows.DS4Controls.GyroZPos];
            gyroXNLb.DataContext = mappingListVM.ControlMap[DS4Windows.DS4Controls.GyroXNeg];
            gyroXLb.DataContext = mappingListVM.ControlMap[DS4Windows.DS4Controls.GyroXPos];
        }

        private void AssignSwipeAssociation()
        {
            swipeUpLb.DataContext = mappingListVM.ControlMap[DS4Windows.DS4Controls.SwipeUp];
            swipeDownLb.DataContext = mappingListVM.ControlMap[DS4Windows.DS4Controls.SwipeDown];
            swipeLeftLb.DataContext = mappingListVM.ControlMap[DS4Windows.DS4Controls.SwipeLeft];
            swipeRightLb.DataContext = mappingListVM.ControlMap[DS4Windows.DS4Controls.SwipeRight];
        }

        private void AssignTriggerFullPullAssociation()
        {
            l2FullPullLb.DataContext = mappingListVM.ControlMap[DS4Controls.L2FullPull];
            r2FullPullLb.DataContext = mappingListVM.ControlMap[DS4Controls.R2FullPull];
        }

        private void AssignGyroSwipeAssociation()
        {
            gyroSwipeLeftLb.DataContext = mappingListVM.ControlMap[DS4Controls.GyroSwipeLeft];
            gyroSwipeRightLb.DataContext = mappingListVM.ControlMap[DS4Controls.GyroSwipeRight];
            gyroSwipeUpLb.DataContext = mappingListVM.ControlMap[DS4Controls.GyroSwipeUp];
            gyroSwipeDownLb.DataContext = mappingListVM.ControlMap[DS4Controls.GyroSwipeDown];
        }

        private void MappingListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (mappingListVM.SelectedIndex >= 0)
            {
                if (reverseHoverIndexes.TryGetValue(mappingListVM.SelectedIndex, out Button tempBtn))
                {
                    InputControlHighlight(tempBtn);
                }
            }

        }

        private void PopulateReverseHoverIndexes()
        {
            foreach(KeyValuePair<Button, int> pair in hoverIndexes)
            {
                reverseHoverIndexes.Add(pair.Value, pair.Key);
            }
        }

        private void PopulateHoverIndexes()
        {
            hoverIndexes[crossConBtn] = 0;
            hoverIndexes[circleConBtn] = 1;
            hoverIndexes[squareConBtn] = 2;
            hoverIndexes[triangleConBtn] = 3;
            hoverIndexes[optionsConBtn] = 4;
            hoverIndexes[shareConBtn] = 5;
            hoverIndexes[upConBtn] = 6;
            hoverIndexes[downConBtn] = 7;
            hoverIndexes[leftConBtn] = 8;
            hoverIndexes[rightConBtn] = 9;
            hoverIndexes[guideConBtn] = 10;
            hoverIndexes[muteConBtn] = 11;
            hoverIndexes[l1ConBtn] = 12;
            hoverIndexes[r1ConBtn] = 13;
            hoverIndexes[l2ConBtn] = 14;
            hoverIndexes[r2ConBtn] = 15;
            hoverIndexes[l3ConBtn] = 16;
            hoverIndexes[r3ConBtn] = 17;
            hoverIndexes[leftTouchConBtn] = 18;
            hoverIndexes[rightTouchConBtn] = 19;
            hoverIndexes[multiTouchConBtn] = 20;
            hoverIndexes[topTouchConBtn] = 21;

            hoverIndexes[lsuConBtn] = 22;
            hoverIndexes[lsdConBtn] = 23;
            hoverIndexes[lslConBtn] = 24;
            hoverIndexes[lsrConBtn] = 25;

            hoverIndexes[rsuConBtn] = 26;
            hoverIndexes[rsdConBtn] = 27;
            hoverIndexes[rslConBtn] = 28;
            hoverIndexes[rsrConBtn] = 29;

            hoverIndexes[gyroZNBtn] = 30;
            hoverIndexes[gyroZPBtn] = 31;
            hoverIndexes[gyroXNBtn] = 32;
            hoverIndexes[gyroXPBtn] = 33;

            hoverIndexes[swipeUpBtn] = 34;
            hoverIndexes[swipeDownBtn] = 35;
            hoverIndexes[swipeLeftBtn] = 36;
            hoverIndexes[swipeRightBtn] = 37;
        }

        private void PopulateHoverLocations()
        {
            hoverLocations[crossConBtn] = new HoverImageInfo() { point = new Point(Canvas.GetLeft(crossConBtn), Canvas.GetTop(crossConBtn)),
                size = new Size(crossConBtn.Width, crossConBtn.Height) };
            hoverLocations[circleConBtn] = new HoverImageInfo() { point = new Point(Canvas.GetLeft(circleConBtn), Canvas.GetTop(circleConBtn)),
                size = new Size(circleConBtn.Width, circleConBtn.Height) };
            hoverLocations[squareConBtn] = new HoverImageInfo() { point = new Point(Canvas.GetLeft(squareConBtn), Canvas.GetTop(squareConBtn)),
                size = new Size(squareConBtn.Width, squareConBtn.Height) };
            hoverLocations[triangleConBtn] = new HoverImageInfo() { point = new Point(Canvas.GetLeft(triangleConBtn), Canvas.GetTop(triangleConBtn)),
                size = new Size(triangleConBtn.Width, triangleConBtn.Height) };
            hoverLocations[l1ConBtn] = new HoverImageInfo() { point = new Point(Canvas.GetLeft(l1ConBtn), Canvas.GetTop(l1ConBtn)),
                size = new Size(l1ConBtn.Width, l1ConBtn.Height) };
            hoverLocations[r1ConBtn] = new HoverImageInfo() { point = new Point(Canvas.GetLeft(r1ConBtn), Canvas.GetTop(r1ConBtn)),
                size = new Size(r1ConBtn.Width, r1ConBtn.Height) };
            hoverLocations[l2ConBtn] = new HoverImageInfo() { point = new Point(Canvas.GetLeft(l2ConBtn), Canvas.GetTop(l2ConBtn)),
                size = new Size(l2ConBtn.Width, l2ConBtn.Height) };
            hoverLocations[r2ConBtn] = new HoverImageInfo() { point = new Point(Canvas.GetLeft(r2ConBtn), Canvas.GetTop(r2ConBtn)),
                size = new Size(r2ConBtn.Width, r2ConBtn.Height) };
            hoverLocations[shareConBtn] = new HoverImageInfo() { point = new Point(Canvas.GetLeft(shareConBtn), Canvas.GetTop(shareConBtn)),
                size = new Size(shareConBtn.Width, shareConBtn.Height) };
            hoverLocations[optionsConBtn] = new HoverImageInfo() { point = new Point(Canvas.GetLeft(optionsConBtn), Canvas.GetTop(optionsConBtn)),
                size = new Size(optionsConBtn.Width, optionsConBtn.Height) };
            hoverLocations[guideConBtn] = new HoverImageInfo() { point = new Point(Canvas.GetLeft(guideConBtn), Canvas.GetTop(guideConBtn)),
                size = new Size(guideConBtn.Width, guideConBtn.Height) };
            hoverLocations[muteConBtn] = new HoverImageInfo()
            {
                point = new Point(Canvas.GetLeft(muteConBtn), Canvas.GetTop(muteConBtn)),
                size = new Size(muteConBtn.Width, muteConBtn.Height)
            };

            hoverLocations[leftTouchConBtn] = new HoverImageInfo() { point = new Point(144, 44), size = new Size(140, 98) };
            hoverLocations[multiTouchConBtn] = new HoverImageInfo() { point = new Point(143, 42), size = new Size(158, 100) };
            hoverLocations[rightTouchConBtn] = new HoverImageInfo() { point = new Point(156, 47), size = new Size(146, 94) };
            hoverLocations[topTouchConBtn] = new HoverImageInfo() { point = new Point(155, 6), size = new Size(153, 114) };

            hoverLocations[l3ConBtn] = new HoverImageInfo() { point = new Point(Canvas.GetLeft(l3ConBtn), Canvas.GetTop(l3ConBtn)),
                size = new Size(l3ConBtn.Width, l3ConBtn.Height) };
            hoverLocations[lsuConBtn] = new HoverImageInfo() { point = new Point(Canvas.GetLeft(l3ConBtn), Canvas.GetTop(l3ConBtn)),
                size = new Size(l3ConBtn.Width, l3ConBtn.Height) };
            hoverLocations[lsrConBtn] = new HoverImageInfo() { point = new Point(Canvas.GetLeft(l3ConBtn), Canvas.GetTop(l3ConBtn)),
                size = new Size(l3ConBtn.Width, l3ConBtn.Height) };
            hoverLocations[lsdConBtn] = new HoverImageInfo() { point = new Point(Canvas.GetLeft(l3ConBtn), Canvas.GetTop(l3ConBtn)),
                size = new Size(l3ConBtn.Width, l3ConBtn.Height) };
            hoverLocations[lslConBtn] = new HoverImageInfo() { point = new Point(Canvas.GetLeft(l3ConBtn), Canvas.GetTop(l3ConBtn)),
                size = new Size(l3ConBtn.Width, l3ConBtn.Height) };

            hoverLocations[r3ConBtn] = new HoverImageInfo() { point = new Point(Canvas.GetLeft(r3ConBtn), Canvas.GetTop(r3ConBtn)),
                size = new Size(r3ConBtn.Width, r3ConBtn.Height) };
            hoverLocations[rsuConBtn] = new HoverImageInfo() { point = new Point(Canvas.GetLeft(r3ConBtn), Canvas.GetTop(r3ConBtn)),
                size = new Size(r3ConBtn.Width, r3ConBtn.Height) };
            hoverLocations[rsrConBtn] = new HoverImageInfo() { point = new Point(Canvas.GetLeft(r3ConBtn), Canvas.GetTop(r3ConBtn)),
                size = new Size(r3ConBtn.Width, r3ConBtn.Height) };
            hoverLocations[rsdConBtn] = new HoverImageInfo() { point = new Point(Canvas.GetLeft(r3ConBtn), Canvas.GetTop(r3ConBtn)),
                size = new Size(r3ConBtn.Width, r3ConBtn.Height) };
            hoverLocations[rslConBtn] = new HoverImageInfo() { point = new Point(Canvas.GetLeft(r3ConBtn), Canvas.GetTop(r3ConBtn)),
                size = new Size(r3ConBtn.Width, r3ConBtn.Height) };

            hoverLocations[upConBtn] = new HoverImageInfo() { point = new Point(Canvas.GetLeft(upConBtn), Canvas.GetTop(upConBtn)),
                size = new Size(upConBtn.Width, upConBtn.Height) };
            hoverLocations[rightConBtn] = new HoverImageInfo() { point = new Point(Canvas.GetLeft(rightConBtn), Canvas.GetTop(rightConBtn)),
                size = new Size(rightConBtn.Width, rightConBtn.Height) };
            hoverLocations[downConBtn] = new HoverImageInfo() { point = new Point(Canvas.GetLeft(downConBtn), Canvas.GetTop(downConBtn)),
                size = new Size(downConBtn.Width, downConBtn.Height) };
            hoverLocations[leftConBtn] = new HoverImageInfo() { point = new Point(Canvas.GetLeft(leftConBtn), Canvas.GetTop(leftConBtn)),
                size = new Size(leftConBtn.Width, leftConBtn.Height) };
        }

        private void RemoveHoverBtnText()
        {
            crossConBtn.Content = "";
            circleConBtn.Content = "";
            squareConBtn.Content = "";
            triangleConBtn.Content = "";
            l1ConBtn.Content = "";
            r1ConBtn.Content = "";
            l2ConBtn.Content = "";
            r2ConBtn.Content = "";
            shareConBtn.Content = "";
            optionsConBtn.Content = "";
            guideConBtn.Content = "";
            muteConBtn.Content = "";
            leftTouchConBtn.Content = "";
            multiTouchConBtn.Content = "";
            rightTouchConBtn.Content = "";
            topTouchConBtn.Content = "";

            l3ConBtn.Content = "";
            lsuConBtn.Content = "";
            lsrConBtn.Content = "";
            lsdConBtn.Content = "";
            lslConBtn.Content = "";

            r3ConBtn.Content = "";
            rsuConBtn.Content = "";
            rsrConBtn.Content = "";
            rsdConBtn.Content = "";
            rslConBtn.Content = "";

            upConBtn.Content = "";
            rightConBtn.Content = "";
            downConBtn.Content = "";
            leftConBtn.Content = "";
        }

        private void PopulateHoverImages()
        {
            ImageSourceConverter sourceConverter = new ImageSourceConverter();

            ImageSource temp = sourceConverter.
                ConvertFromString($"{Global.ASSEMBLY_RESOURCE_PREFIX}component/Resources/DS4-Config_Cross.png") as ImageSource;
            ImageBrush crossHover = new ImageBrush(temp);

            temp = sourceConverter.
                ConvertFromString($"{Global.ASSEMBLY_RESOURCE_PREFIX}component/Resources/DS4-Config_Circle.png") as ImageSource;
            ImageBrush circleHover = new ImageBrush(temp);

            temp = sourceConverter.
                ConvertFromString($"{Global.ASSEMBLY_RESOURCE_PREFIX}component/Resources/DS4-Config_Square.png") as ImageSource;
            ImageBrush squareHover = new ImageBrush(temp);

            temp = sourceConverter.
                ConvertFromString($"{Global.ASSEMBLY_RESOURCE_PREFIX}component/Resources/DS4-Config_Triangle.png") as ImageSource;
            ImageBrush triangleHover = new ImageBrush(temp);

            temp = sourceConverter.
                ConvertFromString($"{Global.ASSEMBLY_RESOURCE_PREFIX}component/Resources/DS4-Config_L1.png") as ImageSource;
            ImageBrush l1Hover = new ImageBrush(temp);

            temp = sourceConverter.
                ConvertFromString($"{Global.ASSEMBLY_RESOURCE_PREFIX}component/Resources/DS4-Config_R1.png") as ImageSource;
            ImageBrush r1Hover = new ImageBrush(temp);

            temp = sourceConverter.
                ConvertFromString($"{Global.ASSEMBLY_RESOURCE_PREFIX}component/Resources/DS4-Config_L2.png") as ImageSource;
            ImageBrush l2Hover = new ImageBrush(temp);

            temp = sourceConverter.
                ConvertFromString($"{Global.ASSEMBLY_RESOURCE_PREFIX}component/Resources/DS4-Config_R2.png") as ImageSource;
            ImageBrush r2Hover = new ImageBrush(temp);

            temp = sourceConverter.
                ConvertFromString($"{Global.ASSEMBLY_RESOURCE_PREFIX}component/Resources/DS4-Config_Share.png") as ImageSource;
            ImageBrush shareHover = new ImageBrush(temp);

            temp = sourceConverter.
                ConvertFromString($"{Global.ASSEMBLY_RESOURCE_PREFIX}component/Resources/DS4-Config_options.png") as ImageSource;
            ImageBrush optionsHover = new ImageBrush(temp);

            temp = sourceConverter.
                ConvertFromString($"{Global.ASSEMBLY_RESOURCE_PREFIX}component/Resources/DS4-Config_PS.png") as ImageSource;
            ImageBrush guideHover = new ImageBrush(temp);

            temp = sourceConverter.
                ConvertFromString($"{Global.ASSEMBLY_RESOURCE_PREFIX}component/Resources/DS4-Config_TouchLeft.png") as ImageSource;
            ImageBrush leftTouchHover = new ImageBrush(temp);

            temp = sourceConverter.
                ConvertFromString($"{Global.ASSEMBLY_RESOURCE_PREFIX}component/Resources/DS4-Config_TouchMulti.png") as ImageSource;
            ImageBrush multiTouchTouchHover = new ImageBrush(temp);

            temp = sourceConverter.
                ConvertFromString($"{Global.ASSEMBLY_RESOURCE_PREFIX}component/Resources/DS4-Config_TouchRight.png") as ImageSource;
            ImageBrush rightTouchHover = new ImageBrush(temp);

            temp = sourceConverter.
                ConvertFromString($"{Global.ASSEMBLY_RESOURCE_PREFIX}component/Resources/DS4-Config_TouchUpper.png") as ImageSource;
            ImageBrush topTouchHover = new ImageBrush(temp);


            temp = sourceConverter.
                ConvertFromString($"{Global.ASSEMBLY_RESOURCE_PREFIX}component/Resources/DS4-Config_LS.png") as ImageSource;
            ImageBrush l3Hover = new ImageBrush(temp);

            temp = sourceConverter.
                ConvertFromString($"{Global.ASSEMBLY_RESOURCE_PREFIX}component/Resources/DS4-Config_LS.png") as ImageSource;
            ImageBrush lsuHover = new ImageBrush(temp);

            temp = sourceConverter.
                ConvertFromString($"{Global.ASSEMBLY_RESOURCE_PREFIX}component/Resources/DS4-Config_LS.png") as ImageSource;
            ImageBrush lsrHover = new ImageBrush(temp);

            temp = sourceConverter.
                ConvertFromString($"{Global.ASSEMBLY_RESOURCE_PREFIX}component/Resources/DS4-Config_LS.png") as ImageSource;
            ImageBrush lsdHover = new ImageBrush(temp);

            temp = sourceConverter.
                ConvertFromString($"{Global.ASSEMBLY_RESOURCE_PREFIX}component/Resources/DS4-Config_LS.png") as ImageSource;
            ImageBrush lslHover = new ImageBrush(temp);


            temp = sourceConverter.
                ConvertFromString($"{Global.ASSEMBLY_RESOURCE_PREFIX}component/Resources/DS4-Config_RS.png") as ImageSource;
            ImageBrush r3Hover = new ImageBrush(temp);

            temp = sourceConverter.
                ConvertFromString($"{Global.ASSEMBLY_RESOURCE_PREFIX}component/Resources/DS4-Config_RS.png") as ImageSource;
            ImageBrush rsuHover = new ImageBrush(temp);

            temp = sourceConverter.
                ConvertFromString($"{Global.ASSEMBLY_RESOURCE_PREFIX}component/Resources/DS4-Config_RS.png") as ImageSource;
            ImageBrush rsrHover = new ImageBrush(temp);

            temp = sourceConverter.
                ConvertFromString($"{Global.ASSEMBLY_RESOURCE_PREFIX}component/Resources/DS4-Config_RS.png") as ImageSource;
            ImageBrush rsdHover = new ImageBrush(temp);

            temp = sourceConverter.
                ConvertFromString($"{Global.ASSEMBLY_RESOURCE_PREFIX}component/Resources/DS4-Config_RS.png") as ImageSource;
            ImageBrush rslHover = new ImageBrush(temp);


            temp = sourceConverter.
                ConvertFromString($"{Global.ASSEMBLY_RESOURCE_PREFIX}component/Resources/DS4-Config_Up.png") as ImageSource;
            ImageBrush upHover = new ImageBrush(temp);

            temp = sourceConverter.
                ConvertFromString($"{Global.ASSEMBLY_RESOURCE_PREFIX}component/Resources/DS4-Config_Right.png") as ImageSource;
            ImageBrush rightHover = new ImageBrush(temp);

            temp = sourceConverter.
                ConvertFromString($"{Global.ASSEMBLY_RESOURCE_PREFIX}component/Resources/DS4-Config_Down.png") as ImageSource;
            ImageBrush downHover = new ImageBrush(temp);

            temp = sourceConverter.
                ConvertFromString($"{Global.ASSEMBLY_RESOURCE_PREFIX}component/Resources/DS4-Config_Left.png") as ImageSource;
            ImageBrush leftHover = new ImageBrush(temp);

            hoverImages[crossConBtn] = crossHover;
            hoverImages[circleConBtn] = circleHover;
            hoverImages[squareConBtn] = squareHover;
            hoverImages[triangleConBtn] = triangleHover;
            hoverImages[l1ConBtn] = l1Hover;
            hoverImages[r1ConBtn] = r1Hover;
            hoverImages[l2ConBtn] = l2Hover;
            hoverImages[r2ConBtn] = r2Hover;
            hoverImages[shareConBtn] = shareHover;
            hoverImages[optionsConBtn] = optionsHover;
            hoverImages[guideConBtn] = guideHover;
            hoverImages[muteConBtn] = guideHover;

            hoverImages[leftTouchConBtn] = leftTouchHover;
            hoverImages[multiTouchConBtn] = multiTouchTouchHover;
            hoverImages[rightTouchConBtn] = rightTouchHover;
            hoverImages[topTouchConBtn] = topTouchHover;
            hoverImages[l3ConBtn] = l3Hover;
            hoverImages[lsuConBtn] = lsuHover;
            hoverImages[lsrConBtn] = lsrHover;
            hoverImages[lsdConBtn] = lsdHover;
            hoverImages[lslConBtn] = lslHover;
            hoverImages[r3ConBtn] = r3Hover;
            hoverImages[rsuConBtn] = rsuHover;
            hoverImages[rsrConBtn] = rsrHover;
            hoverImages[rsdConBtn] = rsdHover;
            hoverImages[rslConBtn] = rslHover;

            hoverImages[upConBtn] = upHover;
            hoverImages[rightConBtn] = rightHover;
            hoverImages[downConBtn] = downHover;
            hoverImages[leftConBtn] = leftHover;
        }

        public void Reload(int device, ProfileEntity profile = null)
        {
            profileSettingsTabCon.DataContext = null;
            mappingListBox.DataContext = null;
            specialActionsTab.DataContext = null;
            lightbarRect.DataContext = null;

            deviceNum = device;
            if (profile != null)
            {
                currentProfile = profile;
                if (device == Global.TEST_PROFILE_INDEX)
                {
                    Global.ProfilePath[Global.TEST_PROFILE_INDEX] = profile.Name;
                }

                Global.LoadProfile(device, false, App.rootHub, false);
                profileNameTxt.Text = profile.Name;
                profileNameTxt.IsEnabled = false;
                applyBtn.IsEnabled = true;
            }
            else
            {
                currentProfile = null;
                PresetOptionWindow presetWin = new PresetOptionWindow();
                presetWin.SetupData(deviceNum);
                presetWin.ShowDialog();
                if (presetWin.Result == MessageBoxResult.Cancel)
                {
                    Global.LoadBlankDevProfile(device, false, App.rootHub, false);
                }
            }

            if (device < Global.TEST_PROFILE_INDEX)
            {
                useControllerUD.Value = device + 1;
                conReadingsUserCon.UseDevice(device, device);
                contReadingsTab.IsEnabled = true;
            }
            else
            {
                useControllerUD.Value = 1;
                conReadingsUserCon.UseDevice(0, Global.TEST_PROFILE_INDEX);
                contReadingsTab.IsEnabled = true;
            }

            conReadingsUserCon.EnableControl(false);

            specialActionsVM.LoadActions(currentProfile == null);
            mappingListVM.UpdateMappings();
            profileSettingsVM.UpdateLateProperties();
            profileSettingsVM.PopulateTouchDisInver(touchDisInvertBtn.ContextMenu);
            profileSettingsVM.PopulateGyroMouseTrig(gyroMouseTrigBtn.ContextMenu);
            profileSettingsVM.PopulateGyroMouseStickTrig(gyroMouseStickTrigBtn.ContextMenu);
            profileSettingsVM.PopulateGyroSwipeTrig(gyroSwipeTrigBtn.ContextMenu);
            profileSettingsVM.PopulateGyroControlsTrig(gyroControlsTrigBtn.ContextMenu);
            profileSettingsTabCon.DataContext = profileSettingsVM;
            mappingListBox.DataContext = mappingListVM;
            specialActionsTab.DataContext = specialActionsVM;
            lightbarRect.DataContext = profileSettingsVM;
            SetupGyroPanel();

            conReadingsUserCon.LsDead = profileSettingsVM.LSDeadZone;
            conReadingsUserCon.RsDead = profileSettingsVM.RSDeadZone;
            conReadingsUserCon.L2Dead = profileSettingsVM.L2DeadZone;
            conReadingsUserCon.R2Dead = profileSettingsVM.R2DeadZone;
            conReadingsUserCon.SixAxisXDead = profileSettingsVM.SXDeadZone;
            conReadingsUserCon.SixAxisZDead = profileSettingsVM.SZDeadZone;

            if (profileSettingsVM.UseControllerReadout)
            {
                inputTimer.Start();
            }
        }

        private void StopEditorBindings()
        {
            profileSettingsTabCon.DataContext = null;
            mappingListBox.DataContext = null;
            specialActionsTab.DataContext = null;
            lightbarRect.DataContext = null;
        }

        private void RefreshEditorBindings()
        {
            specialActionsVM.LoadActions(currentProfile == null);
            mappingListVM.UpdateMappings();
            profileSettingsVM.UpdateLateProperties();
            profileSettingsVM.PopulateTouchDisInver(touchDisInvertBtn.ContextMenu);
            profileSettingsVM.PopulateGyroMouseTrig(gyroMouseTrigBtn.ContextMenu);
            profileSettingsVM.PopulateGyroMouseStickTrig(gyroMouseStickTrigBtn.ContextMenu);
            profileSettingsVM.PopulateGyroSwipeTrig(gyroSwipeTrigBtn.ContextMenu);
            profileSettingsVM.PopulateGyroControlsTrig(gyroControlsTrigBtn.ContextMenu);
            profileSettingsTabCon.DataContext = profileSettingsVM;
            mappingListBox.DataContext = mappingListVM;
            specialActionsTab.DataContext = specialActionsVM;
            lightbarRect.DataContext = profileSettingsVM;
            SetupGyroPanel();

            conReadingsUserCon.LsDead = profileSettingsVM.LSDeadZone;
            conReadingsUserCon.RsDead = profileSettingsVM.RSDeadZone;
            conReadingsUserCon.L2Dead = profileSettingsVM.L2DeadZone;
            conReadingsUserCon.R2Dead = profileSettingsVM.R2DeadZone;
            conReadingsUserCon.SixAxisXDead = profileSettingsVM.SXDeadZone;
            conReadingsUserCon.SixAxisZDead = profileSettingsVM.SZDeadZone;
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            if (profileSettingsVM.FuncDevNum < ControlService.CURRENT_DS4_CONTROLLER_LIMIT)
            {
                App.rootHub.setRumble(0, 0, profileSettingsVM.FuncDevNum);
            }
            Global.outDevTypeTemp[deviceNum] = OutContType.X360;
            Global.LoadProfile(deviceNum, false, App.rootHub);
            Closed?.Invoke(this, EventArgs.Empty);
        }

        private void HoverConBtn_Click(object sender, RoutedEventArgs e)
        {
            MappedControl mpControl = mappingListVM.Mappings[mappingListVM.SelectedIndex];
            BindingWindow window = new BindingWindow(deviceNum, mpControl.Setting);
            window.Owner = App.Current.MainWindow;
            window.ShowDialog();
            mpControl.UpdateMappingName();
            UpdateHighlightLabel(mpControl);
            Global.CacheProfileCustomsFlags(profileSettingsVM.Device);
        }

        private void InputControlHighlight(Button control)
        {
            if (hoverImages.TryGetValue(control, out ImageBrush tempBrush))
            {
                picBoxHover.Source = tempBrush.ImageSource;
                //picBoxHover.Width = tempBrush.ImageSource.Width * .8;
                //picBoxHover.Height = tempBrush.ImageSource.Height * .8;
                //control.Background = tempBrush;
                //control.Background = new SolidColorBrush(Colors.Green);
                //control.Width = tempBrush.ImageSource.Width;
                //control.Height = tempBrush.ImageSource.Height;
            }

            if (hoverLocations.TryGetValue(control, out HoverImageInfo tempInfo))
            {
                Canvas.SetLeft(picBoxHover, tempInfo.point.X);
                Canvas.SetTop(picBoxHover, tempInfo.point.Y);
                picBoxHover.Width = tempInfo.size.Width;
                picBoxHover.Height = tempInfo.size.Height;
                picBoxHover.Stretch = Stretch.Fill;
                picBoxHover.Visibility = Visibility.Visible;
            }

            if (hoverIndexes.TryGetValue(control, out int tempIndex))
            {
                mappingListVM.SelectedIndex = tempIndex;
                mappingListBox.ScrollIntoView(mappingListBox.SelectedItem);
                MappedControl mapped = mappingListVM.Mappings[tempIndex];
                UpdateHighlightLabel(mapped);
            }
        }

        private void UpdateHighlightLabel(MappedControl mapped)
        {
            string display = $"{mapped.ControlName}: {mapped.MappingName}";
            if (mapped.HasShiftAction())
            {
                display += "\nShift: ";
                display += mapped.ShiftMappingName;
            }

            highlightControlDisplayLb.Content = display;
        }

        private void ContBtn_MouseEnter(object sender, MouseEventArgs e)
        {
            Button control = sender as Button;
            InputControlHighlight(control);
        }

        private void ContBtn_MouseLeave(object sender, MouseEventArgs e)
        {
            //Button control = sender as Button;
            //control.Background = new SolidColorBrush(Colors.Transparent);
            Canvas.SetLeft(picBoxHover, 0);
            Canvas.SetTop(picBoxHover, 0);
            picBoxHover.Visibility = Visibility.Hidden;
        }

        private void GyroOutModeCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int idx = gyroOutModeCombo.SelectedIndex;
            if (idx >= 0)
            {
                activeGyroModePanel.Visibility = Visibility.Collapsed;

                if (idx == 0)
                {
                    activeGyroModePanel = gyroControlsPanel;
                }
                else if (idx == 1)
                {
                    activeGyroModePanel = gyroMousePanel;
                }
                else if (idx == 2)
                {
                    activeGyroModePanel = gyroMouseJoystickPanel;
                }
                else if (idx == 3)
                {
                    activeGyroModePanel = gyroDirSwipePanel;
                }
                else if (idx == 4)
                {
                    activeGyroModePanel = passthruPanel;
                }

                activeGyroModePanel.Visibility = Visibility.Visible;

                if (deviceNum < ControlService.CURRENT_DS4_CONTROLLER_LIMIT)
                {
                    App.rootHub.touchPad[deviceNum]?.ResetToggleGyroModes();
                }
            }
        }

        private void SetLateProperties(bool fullSave = true)
        {
            Global.BTPollRate[deviceNum] = profileSettingsVM.TempBTPollRateIndex;
            Global.OutContType[deviceNum] = profileSettingsVM.TempConType;
            if (fullSave)
            {
                Global.outDevTypeTemp[deviceNum] = OutContType.X360;
            }
        }

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            bool saved = ApplyProfileStep(false);
            if (saved)
            {
                Closed?.Invoke(this, EventArgs.Empty);
            }
        }

        private bool ApplyProfileStep(bool fullSave = true)
        {
            bool result = false;
            if (profileSettingsVM.FuncDevNum < ControlService.CURRENT_DS4_CONTROLLER_LIMIT)
            {
                App.rootHub.setRumble(0, 0, profileSettingsVM.FuncDevNum);
            }

            string temp = profileNameTxt.Text;
            if (!string.IsNullOrWhiteSpace(temp) &&
                temp.IndexOfAny(System.IO.Path.GetInvalidFileNameChars()) == -1)
            {
                SetLateProperties(false);
                DS4Windows.Global.ProfilePath[deviceNum] =
                    DS4Windows.Global.OlderProfilePath[deviceNum] = temp;

                if (currentProfile != null)
                {
                    if (temp != currentProfile.Name)
                    {
                        //File.Delete(DS4Windows.Global.appdatapath + @"\Profiles\" + currentProfile.Name + ".xml");
                        currentProfile.DeleteFile();
                        currentProfile.Name = temp;
                    }
                }

                if (currentProfile != null)
                {
                    currentProfile.SaveProfile(deviceNum);
                    currentProfile.FireSaved();
                    result = true;
                }
                else
                {
                    string tempprof = Global.appdatapath + @"\Profiles\" + temp + ".xml";
                    if (!File.Exists(tempprof))
                    {
                        Global.SaveProfile(deviceNum, temp);
                        CreatedProfile?.Invoke(this, temp);
                        result = true;
                    }
                    else
                    {
                        MessageBox.Show(Properties.Resources.ValidName, Properties.Resources.NotValid,
                            MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    }
                }
            }
            else
            {
                MessageBox.Show(Properties.Resources.ValidName, Properties.Resources.NotValid,
                    MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }

            return result;
        }

        private void KeepSizeBtn_Click(object sender, RoutedEventArgs e)
        {
            keepsize = true;
            ImageSourceConverter c = new ImageSourceConverter();
            sizeImage.Source = c.ConvertFromString($"{Global.ASSEMBLY_RESOURCE_PREFIX}component/Resources/checked.png") as ImageSource;
        }

        public void Close()
        {
            if (profileSettingsVM.FuncDevNum < ControlService.CURRENT_DS4_CONTROLLER_LIMIT)
            {
                App.rootHub.setRumble(0, 0, profileSettingsVM.FuncDevNum);
            }

            Closed?.Invoke(this, EventArgs.Empty);
        }

        private void ColorByBatteryPerCk_Click(object sender, RoutedEventArgs e)
        {
            ColorByBatteryPerCheck();
        }

        private void ColorByBatteryPerCheck()
        {
            bool state = profileSettingsVM.ColorBatteryPercent;
            if (state)
            {
                colorGB.Header = Translations.Strings.Full;
                emptyColorGB.Visibility = Visibility.Visible;
            }
            else
            {
                colorGB.Header = Translations.Strings.Color;
                emptyColorGB.Visibility = Visibility.Hidden;
            }
        }

        private void FlashColorBtn_Click(object sender, RoutedEventArgs e)
        {
            ColorPickerWindow dialog = new ColorPickerWindow();
            dialog.Owner = Application.Current.MainWindow;
            Color tempcolor = profileSettingsVM.FlashColorMedia;
            dialog.colorPicker.SelectedColor = tempcolor;
            profileSettingsVM.StartForcedColor(tempcolor);
            dialog.ColorChanged += (sender2, color) =>
            {
                profileSettingsVM.UpdateForcedColor(color);
            };
            dialog.ShowDialog();
            profileSettingsVM.EndForcedColor();
            profileSettingsVM.UpdateFlashColor(dialog.colorPicker.SelectedColor.GetValueOrDefault());
        }

        private void LowColorBtn_Click(object sender, RoutedEventArgs e)
        {
            ColorPickerWindow dialog = new ColorPickerWindow();
            dialog.Owner = Application.Current.MainWindow;
            Color tempcolor = profileSettingsVM.LowColorMedia;
            dialog.colorPicker.SelectedColor = tempcolor;
            profileSettingsVM.StartForcedColor(tempcolor);
            dialog.ColorChanged += (sender2, color) =>
            {
                profileSettingsVM.UpdateForcedColor(color);
            };
            dialog.ShowDialog();
            profileSettingsVM.EndForcedColor();
            profileSettingsVM.UpdateLowColor(dialog.colorPicker.SelectedColor.GetValueOrDefault());
        }

        private void HeavyRumbleTestBtn_Click(object sender, RoutedEventArgs e)
        {
            int deviceNum = profileSettingsVM.FuncDevNum;
            if (deviceNum < ControlService.CURRENT_DS4_CONTROLLER_LIMIT)
            {
                DS4Device d = App.rootHub.DS4Controllers[deviceNum];
                if (d != null)
                {
                    bool rumbleActive = profileSettingsVM.HeavyRumbleActive;
                    if (!rumbleActive)
                    {
                        profileSettingsVM.HeavyRumbleActive = true;
                        d.setRumble(d.RightLightFastRumble,
                            (byte)Math.Min(255, 255 * profileSettingsVM.RumbleBoost / 100));
                        heavyRumbleTestBtn.Content = Properties.Resources.StopHText;
                    }
                    else
                    {
                        profileSettingsVM.HeavyRumbleActive = false;
                        d.setRumble(d.RightLightFastRumble, 0);
                        heavyRumbleTestBtn.Content = Properties.Resources.TestHText;
                    }
                }
            }
        }

        private void LightRumbleTestBtn_Click(object sender, RoutedEventArgs e)
        {
            int deviceNum = profileSettingsVM.FuncDevNum;
            if (deviceNum < ControlService.CURRENT_DS4_CONTROLLER_LIMIT)
            {
                DS4Device d = App.rootHub.DS4Controllers[deviceNum];
                if (d != null)
                {
                    bool rumbleActive = profileSettingsVM.LightRumbleActive;
                    if (!rumbleActive)
                    {
                        profileSettingsVM.LightRumbleActive = true;
                        d.setRumble((byte)Math.Min(255, 255 * profileSettingsVM.RumbleBoost / 100),
                            d.LeftHeavySlowRumble);
                        lightRumbleTestBtn.Content = Properties.Resources.StopLText;
                    }
                    else
                    {
                        profileSettingsVM.LightRumbleActive = false;
                        d.setRumble(0, d.LeftHeavySlowRumble);
                        lightRumbleTestBtn.Content = Properties.Resources.TestLText;
                    }
                }
            }
        }

        private void CustomEditorBtn_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            string tag = btn.Tag.ToString();
            if (tag == "LS") LaunchCurveEditor(profileSettingsVM.LSCustomCurve);
            else if (tag == "RS") LaunchCurveEditor(profileSettingsVM.RSCustomCurve);
            else if (tag == "L2") LaunchCurveEditor(profileSettingsVM.L2CustomCurve);
            else if (tag == "R2") LaunchCurveEditor(profileSettingsVM.R2CustomCurve);
            else if (tag == "SX") LaunchCurveEditor(profileSettingsVM.SXCustomCurve);
            else if (tag == "SZ") LaunchCurveEditor(profileSettingsVM.SZCustomCurve);
        }

        private void LaunchCurveEditor(string customDefinition)
        {
            profileSettingsVM.LaunchCurveEditor(customDefinition);
        }

        private void LaunchProgBrowseBtn_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Multiselect = false;
            dialog.AddExtension = true;
            dialog.DefaultExt = ".exe";
            dialog.Filter = "Program (*.exe)|*.exe";
            dialog.Title = "Select Program";

            dialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            if (dialog.ShowDialog() == true)
            {
                profileSettingsVM.UpdateLaunchProgram(dialog.FileName);
            }
        }

        private void FrictionUD_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (deviceNum < ControlService.CURRENT_DS4_CONTROLLER_LIMIT)
            {
                App.rootHub.touchPad[deviceNum]?.ResetTrackAccel(frictionUD.Value.GetValueOrDefault());
            }
        }

        private void RainbowBtn_Click(object sender, RoutedEventArgs e)
        {
            bool active = profileSettingsVM.Rainbow != 0.0;
            if (active)
            {
                profileSettingsVM.Rainbow = 0.0;
                colorByBatteryPerCk.Content = Properties.Resources.ColorByBattery;
                colorGB.IsEnabled = true;
                emptyColorGB.IsEnabled = true;
            }
            else
            {
                profileSettingsVM.Rainbow = 5.0;
                colorByBatteryPerCk.Content = Properties.Resources.DimByBattery;
                colorGB.IsEnabled = false;
                emptyColorGB.IsEnabled = false;
            }
        }

        private void ChargingColorBtn_Click(object sender, RoutedEventArgs e)
        {
            ColorPickerWindow dialog = new ColorPickerWindow();
            dialog.Owner = Application.Current.MainWindow;
            Color tempcolor = profileSettingsVM.ChargingColorMedia;
            dialog.colorPicker.SelectedColor = tempcolor;
            profileSettingsVM.StartForcedColor(tempcolor);
            dialog.ColorChanged += (sender2, color) =>
            {
                profileSettingsVM.UpdateForcedColor(color);
            };
            dialog.ShowDialog();
            profileSettingsVM.EndForcedColor();
            profileSettingsVM.UpdateChargingColor(dialog.colorPicker.SelectedColor.GetValueOrDefault());
        }

        private void SteeringWheelEmulationCalibrateBtn_Click(object sender, RoutedEventArgs e)
        {
            if (profileSettingsVM.SASteeringWheelEmulationAxisIndex > 0)
            {
                DS4Windows.DS4Device d = App.rootHub.DS4Controllers[profileSettingsVM.FuncDevNum];
                if (d != null)
                {
                    System.Drawing.Point origWheelCenterPoint = new System.Drawing.Point(d.wheelCenterPoint.X, d.wheelCenterPoint.Y);
                    System.Drawing.Point origWheel90DegPointLeft = new System.Drawing.Point(d.wheel90DegPointLeft.X, d.wheel90DegPointLeft.Y);
                    System.Drawing.Point origWheel90DegPointRight = new System.Drawing.Point(d.wheel90DegPointRight.X, d.wheel90DegPointRight.Y);

                    d.WheelRecalibrateActiveState = 1;

                    MessageBoxResult result = MessageBox.Show($"{Properties.Resources.SASteeringWheelEmulationCalibrate}.\n\n" +
                            $"{Properties.Resources.SASteeringWheelEmulationCalibrateInstruction1}.\n" +
                            $"{Properties.Resources.SASteeringWheelEmulationCalibrateInstruction2}.\n" +
                            $"{Properties.Resources.SASteeringWheelEmulationCalibrateInstruction3}.\n\n" +
                            $"{Properties.Resources.SASteeringWheelEmulationCalibrateInstruction}.\n",
                        Properties.Resources.SASteeringWheelEmulationCalibrate, MessageBoxButton.OKCancel, MessageBoxImage.Information, MessageBoxResult.OK);

                    if (result == MessageBoxResult.OK)
                    {
                        // Accept new calibration values (State 3 is "Complete calibration" state)
                        d.WheelRecalibrateActiveState = 3;
                    }
                    else
                    {
                        // Cancel calibration and reset back to original calibration values
                        d.WheelRecalibrateActiveState = 4;

                        d.wheelFullTurnCount = 0;
                        d.wheelCenterPoint = origWheelCenterPoint;
                        d.wheel90DegPointLeft = origWheel90DegPointLeft;
                        d.wheel90DegPointRight = origWheel90DegPointRight;
                    }
                }
                else
                {
                    MessageBox.Show($"{Properties.Resources.SASteeringWheelEmulationCalibrateNoControllerError}.");
                }
            }
        }

        private void TouchDisInvertBtn_Click(object sender, RoutedEventArgs e)
        {
            touchDisInvertBtn.ContextMenu.IsOpen = true;
        }

        private void TouchDisInvertMenuItem_Click(object sender, RoutedEventArgs e)
        {
            profileSettingsVM.UpdateTouchDisInvert(touchDisInvertBtn.ContextMenu);
        }

        private void GyroMouseTrigMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ContextMenu menu = gyroMouseTrigBtn.ContextMenu;
            int itemCount = menu.Items.Count;
            MenuItem alwaysOnItem = menu.Items[itemCount - 1] as MenuItem;

            profileSettingsVM.UpdateGyroMouseTrig(menu, e.OriginalSource == alwaysOnItem);
        }

        private void GyroMouseStickTrigMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ContextMenu menu = gyroMouseStickTrigBtn.ContextMenu;
            int itemCount = menu.Items.Count;
            MenuItem alwaysOnItem = menu.Items[itemCount - 1] as MenuItem;

            profileSettingsVM.UpdateGyroMouseStickTrig(menu, e.OriginalSource == alwaysOnItem);
        }

        private void GyroMouseTrigBtn_Click(object sender, RoutedEventArgs e)
        {
            gyroMouseTrigBtn.ContextMenu.IsOpen = true;
        }

        private void GyroMouseStickTrigBtn_Click(object sender, RoutedEventArgs e)
        {
            gyroMouseStickTrigBtn.ContextMenu.IsOpen = true;
        }

        private void OutConTypeCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int index = outConTypeCombo.SelectedIndex;
            if (index >= 0)
            {
                mappingListVM.UpdateMappingDevType(profileSettingsVM.TempConType);
            }
        }

        private void NewActionBtn_Click(object sender, RoutedEventArgs e)
        {
            baseSpeActPanel.Visibility = Visibility.Collapsed;
            ProfileList profList = (Application.Current.MainWindow as MainWindow).ProfileListHolder;
            SpecialActionEditor actEditor = new SpecialActionEditor(deviceNum, profList, null);
            specialActionDockPanel.Children.Add(actEditor);
            actEditor.Visibility = Visibility.Visible;
            actEditor.Cancel += (sender2, args) =>
            {
                specialActionDockPanel.Children.Remove(actEditor);
                baseSpeActPanel.Visibility = Visibility.Visible;
            };
            actEditor.Saved += (sender2, actionName) =>
            {
                SpecialAction action = Global.GetAction(actionName);
                SpecialActionItem newitem = specialActionsVM.CreateActionItem(action);
                newitem.Active = true;
                specialActionsVM.ActionCol.Add(newitem);
                specialActionDockPanel.Children.Remove(actEditor);
                baseSpeActPanel.Visibility = Visibility.Visible;

                specialActionsVM.ExportEnabledActions();
                Global.CacheExtraProfileInfo(profileSettingsVM.Device);
            };
        }

        private void EditActionBtn_Click(object sender, RoutedEventArgs e)
        {
            if (specialActionsVM.SpecialActionIndex >= 0)
            {
                int currentIndex = specialActionsVM.SpecialActionIndex;
                SpecialActionItem item = specialActionsVM.ActionCol[specialActionsVM.SpecialActionIndex];
                baseSpeActPanel.Visibility = Visibility.Collapsed;
                ProfileList profList = (Application.Current.MainWindow as MainWindow).ProfileListHolder;
                SpecialActionEditor actEditor = new SpecialActionEditor(deviceNum, profList, item.SpecialAction);
                specialActionDockPanel.Children.Add(actEditor);
                actEditor.Visibility = Visibility.Visible;
                actEditor.Cancel += (sender2, args) =>
                {
                    specialActionDockPanel.Children.Remove(actEditor);
                    baseSpeActPanel.Visibility = Visibility.Visible;
                };
                actEditor.Saved += (sender2, actionName) =>
                {
                    DS4Windows.SpecialAction action = DS4Windows.Global.GetAction(actionName);
                    SpecialActionItem newitem = specialActionsVM.CreateActionItem(action);
                    newitem.Active = item.Active;
                    specialActionsVM.ActionCol.RemoveAt(currentIndex);
                    specialActionsVM.ActionCol.Insert(currentIndex, newitem);
                    specialActionDockPanel.Children.Remove(actEditor);
                    baseSpeActPanel.Visibility = Visibility.Visible;
                    Global.CacheExtraProfileInfo(profileSettingsVM.Device);
                };
            }
        }

        private void RemoveActionBtn_Click(object sender, RoutedEventArgs e)
        {
            if (specialActionsVM.SpecialActionIndex >= 0)
            {
                SpecialActionItem item = specialActionsVM.ActionCol[specialActionsVM.SpecialActionIndex];
                specialActionsVM.RemoveAction(item);
                Global.CacheExtraProfileInfo(profileSettingsVM.Device);
            }
        }

        private void SpecialActionCheckBox_Click(object sender, RoutedEventArgs e)
        {
            specialActionsVM.ExportEnabledActions();
        }

        private void Ds4LightbarColorBtn_MouseEnter(object sender, MouseEventArgs e)
        {
            highlightControlDisplayLb.Content = "Click the lightbar for color picker";
        }

        private void Ds4LightbarColorBtn_MouseLeave(object sender, MouseEventArgs e)
        {
            highlightControlDisplayLb.Content = "";
        }

        private void Ds4LightbarColorBtn_Click(object sender, RoutedEventArgs e)
        {
            ColorPickerWindow dialog = new ColorPickerWindow();
            dialog.Owner = Application.Current.MainWindow;
            Color tempcolor = profileSettingsVM.MainColor;
            dialog.colorPicker.SelectedColor = tempcolor;
            profileSettingsVM.StartForcedColor(tempcolor);
            dialog.ColorChanged += (sender2, color) =>
            {
                profileSettingsVM.UpdateForcedColor(color);
            };
            dialog.ShowDialog();
            profileSettingsVM.EndForcedColor();
            profileSettingsVM.UpdateMainColor(dialog.colorPicker.SelectedColor.GetValueOrDefault());
        }

        private void InputDS4(object sender, System.Timers.ElapsedEventArgs e)
        {
            inputTimer.Stop();

            bool activeWin = false;
            int tempDeviceNum = 0;
            Dispatcher.Invoke(() =>
            {
                activeWin = Application.Current.MainWindow.IsActive;
                tempDeviceNum = profileSettingsVM.FuncDevNum;
            });

            if (activeWin && profileSettingsVM.UseControllerReadout)
            {
                int index = -1;
                switch(Program.rootHub.GetActiveInputControl(tempDeviceNum))
                {
                    case DS4Controls.None: break;
                    case DS4Controls.Cross: index = 0; break;
                    case DS4Controls.Circle: index = 1; break;
                    case DS4Controls.Square: index = 2; break;
                    case DS4Controls.Triangle: index = 3; break;
                    case DS4Controls.Options: index = 4; break;
                    case DS4Controls.Share: index = 5; break;
                    case DS4Controls.DpadUp: index = 6; break;
                    case DS4Controls.DpadDown: index = 7; break;
                    case DS4Controls.DpadLeft: index = 8; break;
                    case DS4Controls.DpadRight: index = 9; break;
                    case DS4Controls.PS: index = 10; break;
                    case DS4Controls.L1: index = 11; break;
                    case DS4Controls.R1: index = 12; break;
                    case DS4Controls.L2: index = 13; break;
                    case DS4Controls.R2: index = 14; break;
                    case DS4Controls.L3: index = 15; break;
                    case DS4Controls.R3: index = 16; break;
                    case DS4Controls.TouchLeft: index = 17; break;
                    case DS4Controls.TouchRight: index = 18; break;
                    case DS4Controls.TouchMulti: index = 19; break;
                    case DS4Controls.TouchUpper: index = 20; break;
                    case DS4Controls.LYNeg: index = 21; break;
                    case DS4Controls.LYPos: index = 22; break;
                    case DS4Controls.LXNeg: index = 23; break;
                    case DS4Controls.LXPos: index = 24; break;
                    case DS4Controls.RYNeg: index = 25; break;
                    case DS4Controls.RYPos: index = 26; break;
                    case DS4Controls.RXNeg: index = 27; break;
                    case DS4Controls.RXPos: index = 28; break;
                    default: break;
                }

                if (index >= 0)
                {
                    Dispatcher.BeginInvoke((Action)(() =>
                    {
                        mappingListVM.SelectedIndex = index;
                        ShowControlBindingWindow();
                    }));
                }
            }

            if (profileSettingsVM.UseControllerReadout)
            {
                inputTimer.Start();
            }
        }
        private void ProfileEditor_Closed(object sender, EventArgs e)
        {
            profileSettingsVM.UseControllerReadout = false;
            inputTimer.Stop();
            conReadingsUserCon.EnableControl(false);
            Global.CacheExtraProfileInfo(profileSettingsVM.Device);
        }

        private void UseControllerReadoutCk_Click(object sender, RoutedEventArgs e)
        {
            if (profileSettingsVM.UseControllerReadout && profileSettingsVM.Device < ControlService.CURRENT_DS4_CONTROLLER_LIMIT)
            {
                inputTimer.Start();
            }
            else
            {
                inputTimer.Stop();
            }
        }

        private void ShowControlBindingWindow()
        {
            MappedControl mpControl = mappingListVM.Mappings[mappingListVM.SelectedIndex];
            BindingWindow window = new BindingWindow(deviceNum, mpControl.Setting);
            window.Owner = App.Current.MainWindow;
            window.ShowDialog();
            mpControl.UpdateMappingName();
            UpdateHighlightLabel(mpControl);
            Global.CacheProfileCustomsFlags(profileSettingsVM.Device);
        }

        private void MappingListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (mappingListVM.SelectedIndex >= 0)
            {
                ShowControlBindingWindow();
            }
        }

        private void SidebarTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sidebarTabControl.SelectedItem == contReadingsTab)
            {
                controllerReadingsTabActive = true;
                conReadingsUserCon.EnableControl(true);
            }
            else if (controllerReadingsTabActive)
            {
                controllerReadingsTabActive = false;
                conReadingsUserCon.EnableControl(false);
            }
        }

        private void TiltControlsButton_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            DS4Controls control = (DS4Controls)Convert.ToInt32(btn.Tag);
            MappedControl mpControl = mappingListVM.ControlMap[control];
            BindingWindow window = new BindingWindow(deviceNum, mpControl.Setting);
            window.Owner = App.Current.MainWindow;
            window.ShowDialog();
            mpControl.UpdateMappingName();
            UpdateHighlightLabel(mpControl);
            Global.CacheProfileCustomsFlags(profileSettingsVM.Device);
        }

        private void SwipeControlsButton_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            DS4Controls control = (DS4Controls)Convert.ToInt32(btn.Tag);
            MappedControl mpControl = mappingListVM.ControlMap[control];
            BindingWindow window = new BindingWindow(deviceNum, mpControl.Setting);
            window.Owner = App.Current.MainWindow;
            window.ShowDialog();
            mpControl.UpdateMappingName();
            UpdateHighlightLabel(mpControl);
            Global.CacheProfileCustomsFlags(profileSettingsVM.Device);
        }

        private void ConBtn_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            Button btn = sender as Button;
            MappedControl mpControl = mappingListVM.Mappings[mappingListVM.SelectedIndex];
            profileSettingsVM.PresetMenuUtil.SetHighlightControl(mpControl.Control);
            ContextMenu cm = conCanvas.FindResource("presetMenu") as ContextMenu;
            MenuItem temp = cm.Items[0] as MenuItem;
            temp.Header = profileSettingsVM.PresetMenuUtil.PresetInputLabel;
            cm.PlacementTarget = btn;
            cm.IsOpen = true;
        }

        private void PresetMenuItem_Click(object sender, RoutedEventArgs e)
        {
            MenuItem item = sender as MenuItem;
            int baseTag = Convert.ToInt32(item.Tag);
            int subTag = Convert.ToInt32(item.CommandParameter);
            if (baseTag >= 0 && subTag >= 0)
            {
                List<DS4Controls> controls =
                    profileSettingsVM.PresetMenuUtil.ModifySettingWithPreset(baseTag, subTag);
                foreach(DS4Controls control in controls)
                {
                    MappedControl mpControl = mappingListVM.ControlMap[control];
                    mpControl.UpdateMappingName();
                }

                Global.CacheProfileCustomsFlags(profileSettingsVM.Device);
                highlightControlDisplayLb.Content = "";
            }
        }

        private void PresetBtn_Click(object sender, RoutedEventArgs e)
        {
            sidebarTabControl.SelectedIndex = 0;

            PresetOptionWindow presetWin = new PresetOptionWindow();
            presetWin.SetupData(deviceNum);
            presetWin.ToPresetsScreen();
            presetWin.DelayPresetApply = true;
            presetWin.ShowDialog();

            if (presetWin.Result == MessageBoxResult.OK)
            {
                StopEditorBindings();
                presetWin.ApplyPreset();
                RefreshEditorBindings();
            }
        }

        private void ApplyBtn_Click(object sender, RoutedEventArgs e)
        {
            ApplyProfileStep();
        }

        private void TriggerFullPullBtn_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            int tag = Convert.ToInt32(btn.Tag);
            DS4Controls ds4control = (DS4Controls)tag;
            if (ds4control == DS4Controls.None)
            {
                return;
            }

            //DS4ControlSettings setting = Global.getDS4CSetting(tag, ds4control);
            MappedControl mpControl = mappingListVM.ControlMap[ds4control];
            BindingWindow window = new BindingWindow(deviceNum, mpControl.Setting);
            window.Owner = App.Current.MainWindow;
            window.ShowDialog();
            mpControl.UpdateMappingName();
            Global.CacheProfileCustomsFlags(profileSettingsVM.Device);
		}

        private void GyroCalibration_Click(object sender, RoutedEventArgs e)
        {
            int deviceNum = profileSettingsVM.FuncDevNum;
            if (deviceNum < ControlService.CURRENT_DS4_CONTROLLER_LIMIT)
            {
                DS4Device d = App.rootHub.DS4Controllers[deviceNum];
                d.SixAxis.ResetContinuousCalibration();
            }
        }

        private void GyroSwipeTrigBtn_Click(object sender, RoutedEventArgs e)
        {
            gyroSwipeTrigBtn.ContextMenu.IsOpen = true;
        }

        private void GyroSwipeTrigMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ContextMenu menu = gyroSwipeTrigBtn.ContextMenu;
            int itemCount = menu.Items.Count;
            MenuItem alwaysOnItem = menu.Items[itemCount - 1] as MenuItem;

            profileSettingsVM.UpdateGyroSwipeTrig(menu, e.OriginalSource == alwaysOnItem);
        }

        private void GyroSwipeControlsBtn_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            DS4Controls control = (DS4Controls)Convert.ToInt32(btn.Tag);
            MappedControl mpControl = mappingListVM.ControlMap[control];
            BindingWindow window = new BindingWindow(deviceNum, mpControl.Setting);
            window.Owner = App.Current.MainWindow;
            window.ShowDialog();
            mpControl.UpdateMappingName();
            Global.CacheProfileCustomsFlags(profileSettingsVM.Device);
        }

        private void GyroControlsTrigBtn_Click(object sender, RoutedEventArgs e)
        {
            gyroControlsTrigBtn.ContextMenu.IsOpen = true;
        }

        private void GyroControlsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ContextMenu menu = gyroControlsTrigBtn.ContextMenu;
            int itemCount = menu.Items.Count;
            MenuItem alwaysOnItem = menu.Items[itemCount - 1] as MenuItem;

            profileSettingsVM.UpdateGyroControlsTrig(menu, e.OriginalSource == alwaysOnItem);
        }
    }

    public class ControlIndexCheck
    {
        public int TiltUp { get => (int)DS4Controls.GyroZNeg; }
        public int TiltDown { get => (int)DS4Controls.GyroZPos; }
        public int TiltLeft { get => (int)DS4Controls.GyroXPos; }
        public int TiltRight { get => (int)DS4Controls.GyroXNeg; }

        public int SwipeUp { get => (int)DS4Controls.SwipeUp; }
        public int SwipeDown { get => (int)DS4Controls.SwipeDown; }
        public int SwipeLeft { get => (int)DS4Controls.SwipeLeft; }
        public int SwipeRight { get => (int)DS4Controls.SwipeRight; }
        public int L2FullPull { get => (int)DS4Controls.L2FullPull; }
        public int R2FullPull { get => (int)DS4Controls.R2FullPull; }

        public int GyroSwipeLeft { get => (int)DS4Controls.GyroSwipeLeft; }
        public int GyroSwipeRight { get => (int)DS4Controls.GyroSwipeRight; }
        public int GyroSwipeUp { get => (int)DS4Controls.GyroSwipeUp; }
        public int GyroSwipeDown { get => (int)DS4Controls.GyroSwipeDown; }
    }
}
