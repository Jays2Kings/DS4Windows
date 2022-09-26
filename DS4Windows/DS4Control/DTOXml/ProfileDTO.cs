using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using DS4Windows;

namespace DS4WinWPF.DS4Control.DTOXml
{
    [XmlRoot("DS4Windows")]
    public class ProfileDTO : IDTO<BackingStore>
    {
        [XmlAttribute("app_version")]
        public string AppVersion
        {
            get => Global.exeversion;
            set { }
        }

        [XmlAttribute("config_version")]
        public string ConfigVersion
        {
            get => Global.CONFIG_VERSION.ToString();
            set { }
        }

        [XmlIgnore]
        public bool TouchToggle
        {
            get; private set;
        }

        [XmlElement("touchToggle")]
        public string TouchToggleString
        {
            get => TouchToggle.ToString();
            set => TouchToggle = XmlDataUtilities.StrToBool(value);
        }

        [XmlElement("idleDisconnectTimeout")]
        public int IdleDisconnect
        {
            get; set;
        }

        [XmlIgnore]
        public bool OutputDataToDS4
        {
            get; private set;
        }

        [XmlElement("outputDataToDS4")]
        public string OutputDataToDS4String
        {
            get => OutputDataToDS4.ToString();
            set => OutputDataToDS4 = XmlDataUtilities.StrToBool(value);
        }

        [XmlElement("LightbarMode")]
        public LightbarMode LightbarMode
        {
            get; set;
        }

        [XmlElement("Color")]
        public string ColorString
        {
            get; set;
        }

        [XmlElement("Red")]
        public string RedColorString
        {
            get; set;
        }

        [XmlElement("Green")]
        public string GreenColorString
        {
            get; set;
        }

        [XmlElement("Blue")]
        public string BlueColorString
        {
            get; set;
        }

        [XmlElement("RumbleBoost")]
        public byte RumbleBoost
        {
            get; set;
        }

        [XmlElement("RumbleAutostopTime")]
        public int RumbleAutostopTime
        {
            get; set;
        }

        [XmlIgnore]
        public bool LedAsBatteryIndicator
        {
            get; private set;
        }

        [XmlElement("ledAsBatteryIndicator")]
        public string LedAsBatteryIndicatorString
        {
            get => LedAsBatteryIndicator.ToString();
            set => LedAsBatteryIndicator = XmlDataUtilities.StrToBool(value);
        }

        [XmlElement("FlashType")]
        public byte FlashType
        {
            get; set;
        }

        [XmlElement("flashBatteryAt")]
        public int FlashBatteryAt
        {
            get; set;
        }

        [XmlElement("touchSensitivity")]
        public byte TouchSensitivity
        {
            get; set;
        }

        [XmlElement("LowColor")]
        public string LowColorString
        {
            get; set;
        }

        [XmlElement("LowRed")]
        public string LowRedColorString
        {
            get; set;
        }

        [XmlElement("LowGreen")]
        public string LowGreenColorString
        {
            get; set;
        }

        [XmlElement("LowBlue")]
        public string LowBlueColorString
        {
            get; set;
        }


        [XmlElement("ChargingColor")]
        public string ChargingColorString
        {
            get; set;
        }

        [XmlElement("ChargingRed")]
        public string ChargingRedString
        {
            get; set;
        }

        [XmlElement("ChargingGreen")]
        public string ChargingGreenString
        {
            get; set;
        }

        [XmlElement("ChargingBlue")]
        public string ChargingBlueString
        {
            get; set;
        }

        [XmlElement("FlashColor")]
        public string FlashColorString
        {
            get; set;
        }

        [XmlIgnore]
        public bool TouchpadJitterCompensation
        {
            get; private set;
        }

        [XmlElement("touchpadJitterCompensation")]
        public string TouchpadJitterCompensationString
        {
            get => TouchpadJitterCompensation.ToString();
            set => TouchpadJitterCompensation = XmlDataUtilities.StrToBool(value);
        }

        [XmlIgnore]
        public bool LowerRCOn
        {
            get; private set;
        }

        [XmlElement("lowerRCOn")]
        public string LowerRCOnString
        {
            get => LowerRCOn.ToString();
            set => LowerRCOn = XmlDataUtilities.StrToBool(value);
        }

        [XmlElement("tapSensitivity")]
        public byte TapSensitivity
        {
            get; set;
        }

        [XmlIgnore]
        public bool DoubleTap
        {
            get; private set;
        }

        [XmlElement("doubleTap")]
        public string DoubleTapString
        {
            get => DoubleTap.ToString();
            set => DoubleTap = XmlDataUtilities.StrToBool(value);
        }

        [XmlElement("scrollSensitivity")]
        public int ScrollSensitivity
        {
            get; set;
        }

        private int _touchpadInvert;
        [XmlElement("TouchpadInvert")]
        public int TouchpadInvert
        {
            get => _touchpadInvert;
            set => _touchpadInvert = Math.Clamp(value, 0, 3);
        }

        [XmlIgnore]
        public bool TouchpadClickPassthru
        {
            get; private set;
        }

        [XmlElement("TouchpadClickPassthru")]
        public string TouchpadClickPassthruString
        {
            get => TouchpadClickPassthru.ToString();
            set => TouchpadClickPassthru = XmlDataUtilities.StrToBool(value);
        }

        [XmlElement("LeftTriggerMiddle")]
        public byte LeftTriggerMiddle
        {
            get; set;
        }

        [XmlElement("RightTriggerMiddle")]
        public byte RightTriggerMiddle
        {
            get; set;
        }

        [XmlElement("L2AntiDeadZone")]
        public int L2AntiDeadZone
        {
            get; set;
        }

        [XmlElement("R2AntiDeadZone")]
        public int R2AntiDeadZone
        {
            get; set;
        }

        private int _l2MaxZone;
        [XmlElement("L2MaxZone")]
        public int L2MaxZone
        {
            get => _l2MaxZone;
            set => _l2MaxZone = Math.Clamp(value, 0, 100);
        }

        private int _r2MaxZone;
        [XmlElement("R2MaxZone")]
        public int R2MaxZone
        {
            get => _r2MaxZone;
            set => _r2MaxZone = Math.Clamp(value, 0, 100);
        }

        private double _l2MaxOutput;
        [XmlElement("L2MaxOutput")]
        public double L2MaxOutput
        {
            get => _l2MaxOutput;
            set => _l2MaxOutput = Math.Clamp(value, 0.0, 100.0);
        }

        private double _r2MaxOutput;
        [XmlElement("R2MaxOutput")]
        public double R2MaxOutput
        {
            get => _r2MaxOutput;
            set => _r2MaxOutput = Math.Clamp(value, 0.0, 100.0);
        }

        private double _lSRotation;
        [XmlElement("LSRotation")]
        public double LSRotation
        {
            get => _lSRotation;
            set
            {
                double tempDegrees = Math.Clamp(value, -180.0, 180.0);
                _lSRotation = tempDegrees * Math.PI / 180.0;
            }
        }

        private double _rSRotation;
        [XmlElement("RSRotation")]
        public double RSRotation
        {
            get => _rSRotation;
            set
            {
                double tempDegrees = Math.Clamp(value, -180.0, 180.0);
                _rSRotation = tempDegrees * Math.PI / 180.0;
            }
        }

        private int _lSFuzz;
        [XmlElement("LSFuzz")]
        public int LSFuzz
        {
            get => _lSFuzz;
            set => _lSFuzz = Math.Clamp(value, 0, 100);
        }

        private int _rSFuzz;
        [XmlElement("RSFuzz")]
        public int RSFuzz
        {
            get => _rSFuzz;
            set => _rSFuzz = Math.Clamp(value, 0, 100);
        }

        [XmlElement("ButtonMouseSensitivity")]
        public int ButtonMouseSensitivity
        {
            get; set;
        }

        [XmlElement("ButtonMouseOffset")]
        public double ButtonMouseOffset
        {
            get; set;
        }

        private double _buttonMouseVerticalScale;
        [XmlElement("ButtonMouseVerticalScale")]
        public double ButtonMouseVerticalScale
        {
            get => _buttonMouseVerticalScale;
            set => _buttonMouseVerticalScale = Math.Clamp(value, 0.0, 500.0) * 0.01;
        }

        [XmlElement("Rainbow")]
        public double Rainbow
        {
            get; set;
        }

        private double _maxSatRainbow;
        [XmlElement("MaxSatRainbow")]
        public double MaxSatRainbow
        {
            get => _maxSatRainbow;
            set => _maxSatRainbow = Math.Clamp(value, 0.0, 100.0) / 100.0;
        }

        private int _lSDeadZone = 10;
        [XmlElement("LSDeadZone")]
        public int LSDeadZone
        {
            get => _lSDeadZone;
            set => _lSDeadZone = Math.Clamp(value, 0, 127);
        }

        private int _rSDeadZone = 10;
        [XmlElement("RSDeadZone")]
        public int RSDeadZone
        {
            get => _rSDeadZone;
            set => _rSDeadZone = Math.Clamp(value, 0, 127);
        }

        private int _lSAntiDeadZone = 20;
        [XmlElement("LSAntiDeadZone")]
        public int LSAntiDeadZone
        {
            get => _lSAntiDeadZone;
            set => _lSAntiDeadZone = value;
        }

        private int _rSAntiDeadZone = 20;
        [XmlElement("RSAntiDeadZone")]
        public int RSAntiDeadZone
        {
            get => _rSAntiDeadZone;
            set => _rSAntiDeadZone = value;
        }

        private int _lSMaxZone = 100;
        [XmlElement("LSMaxZone")]
        public int LSMaxZone
        {
            get => _lSMaxZone;
            set => _lSMaxZone = Math.Clamp(value, 0, 100);
        }

        private int _rSMaxZone = 100;
        [XmlElement("RSMaxZone")]
        public int RSMaxZone
        {
            get => _rSMaxZone;
            set => _rSMaxZone = Math.Clamp(value, 0, 100);
        }

        private double _lSVerticalScale = StickDeadZoneInfo.DEFAULT_VERTICAL_SCALE;
        [XmlElement("LSVerticalScale")]
        public double LSVerticalScale
        {
            get => _lSVerticalScale;
            set => _lSVerticalScale = Math.Clamp(value, 0.0, 100.0);
        }

        private double _lSMaxOutput = 100.0;
        [XmlElement("LSMaxOutput")]
        public double LSMaxOutput
        {
            get => _lSMaxOutput;
            set => _lSMaxOutput = Math.Clamp(value, 0.0, 100.0);
        }

        private bool _lsMaxOutputForce = StickDeadZoneInfo.DEFAULT_MAXOUTPUT_FORCE;
        [XmlElement("LSMaxOutputForce")]
        public string LSMaxOutputForceString
        {
            get => _lsMaxOutputForce.ToString();
            set => _lsMaxOutputForce = XmlDataUtilities.StrToBool(value);
        }

        private double _rSVerticalScale = StickDeadZoneInfo.DEFAULT_VERTICAL_SCALE;
        [XmlElement("RSVerticalScale")]
        public double RSVerticalScale
        {
            get => _rSVerticalScale;
            set => _rSVerticalScale = Math.Clamp(value, 0.0, 100.0);
        }

        private double _rSMaxOutput = 100.0;
        [XmlElement("RSMaxOutput")]
        public double RSMaxOutput
        {
            get => _rSMaxOutput;
            set => _rSMaxOutput = Math.Clamp(value, 0.0, 100.0);
        }

        private bool _rsMaxOutputForce = StickDeadZoneInfo.DEFAULT_MAXOUTPUT_FORCE;
        [XmlElement("RSMaxOutputForce")]
        public string RSMaxOutputForceString
        {
            get => _rsMaxOutputForce.ToString();
            set => _rsMaxOutputForce = XmlDataUtilities.StrToBool(value);
        }

        private double _lSOuterBindDead = StickDeadZoneInfo.DEFAULT_OUTER_BIND_DEAD;
        [XmlElement("LSOuterBindDead")]
        public double LSOuterBindDead
        {
            get => _lSOuterBindDead;
            set => _lSOuterBindDead = Math.Clamp(value, 0.0, 100.0);
        }

        private double _rSOuterBindDead = StickDeadZoneInfo.DEFAULT_OUTER_BIND_DEAD;
        [XmlElement("RSOuterBindDead")]
        public double RSOuterBindDead
        {
            get => _rSOuterBindDead;
            set => _rSOuterBindDead = Math.Clamp(value, 0.0, 100.0);
        }

        private bool _lSOuterBindInvert = StickDeadZoneInfo.DEFAULT_OUTER_BIND_INVERT;
        [XmlElement("LSOuterBindInvert")]
        public string LSOuterBindInvertString
        {
            get => _lSOuterBindInvert.ToString();
            set => _lSOuterBindInvert = XmlDataUtilities.StrToBool(value);
        }

        private bool _rSOuterBindInvert = StickDeadZoneInfo.DEFAULT_OUTER_BIND_INVERT;
        [XmlElement("RSOuterBindInvert")]
        public string RSOuterBindInvertString
        {
            get => _rSOuterBindInvert.ToString();
            set => _rSOuterBindInvert = XmlDataUtilities.StrToBool(value);
        }

        [XmlElement("LSDeadZoneType")]
        public StickDeadZoneInfo.DeadZoneType LSDeadZoneType
        {
            get; set;
        }

        [XmlElement("LSAxialDeadOptions")]
        public StickAxialDeadOptionsSerializer LSAxialDeadOptions
        {
            get; set;
        }

        [XmlElement("LSDeltaAccelSettings")]
        public StickDeltaAccelSettings LSDeltaAccelSettings
        {
            get; set;
        }

        [XmlElement("RSDeadZoneType")]
        public StickDeadZoneInfo.DeadZoneType RSDeadZoneType
        {
            get; set;
        }

        [XmlElement("RSAxialDeadOptions")]
        public StickAxialDeadOptionsSerializer RSAxialDeadOptions
        {
            get; set;
        }

        [XmlElement("RSDeltaAccelSettings")]
        public StickDeltaAccelSettings RSDeltaAccelSettings
        {
            get; set;
        }

        private double _sXDeadZone = BackingStore.DEFAULT_SX_TILT_DEADZONE;
        [XmlElement("SXDeadZone")]
        public double SXDeadZone
        {
            get => _sXDeadZone;
            set => _sXDeadZone = value;
        }

        private double _sZDeadZone = BackingStore.DEFAULT_SX_TILT_DEADZONE;
        [XmlElement("SZDeadZone")]
        public double SZDeadZone
        {
            get => _sZDeadZone;
            set => _sZDeadZone = value;
        }

        private double _sxMaxZone = 1.0;
        [XmlElement("SXMaxZone")]
        public double SXMaxZone
        {
            get => _sxMaxZone;
            set => _sxMaxZone = Math.Clamp(value * 0.01, 0.0, 1.0);
        }

        private double _szMaxZone = 1.0;
        [XmlElement("SZMaxZone")]
        public double SZMaxZone
        {
            get => _szMaxZone;
            set => _szMaxZone = Math.Clamp(value * 0.01, 0.0, 1.0);
        }

        private double _sxAntiDeadZone;
        [XmlElement("SXAntiDeadZone")]
        public double SXAntiDeadZone
        {
            get => _sxAntiDeadZone;
            set => _sxAntiDeadZone = Math.Clamp(value * 0.01, 0.0, 1.0);
        }

        private double _szAntiDeadZone;
        [XmlElement("SZAntiDeadZone")]
        public double SZAntiDeadZone
        {
            get => _szAntiDeadZone;
            set => _szAntiDeadZone = Math.Clamp(value * 0.01, 0.0, 1.0);
        }

        [XmlElement("Sensitivity")]
        public string Sensitivity
        {
            get; set;
        }

        [XmlElement("ChargingType")]
        public int ChargingType
        {
            get; set;
        }

        private bool _mouseAcceleration;
        [XmlElement("MouseAcceleration")]
        public string MouseAccelerationString
        {
            get => _mouseAcceleration.ToString();
            set => _mouseAcceleration = XmlDataUtilities.StrToBool(value);
        }

        [XmlElement("ShiftModifier")]
        public int ShiftModifier
        {
            get; set;
        }

        [XmlElement("LaunchProgram")]
        public string LaunchProgram
        {
            get; set;
        }

        private bool _dinputOnly;
        [XmlElement("DinputOnly")]
        public string DinputOnlyString
        {
            get => _dinputOnly.ToString();
            set => _dinputOnly = XmlDataUtilities.StrToBool(value);
        }

        private bool _startTouchpadOff;
        [XmlElement("StartTouchpadOff")]
        public string StartTouchpadOffString
        {
            get => _startTouchpadOff.ToString();
            set => _startTouchpadOff = XmlDataUtilities.StrToBool(value);
        }

        private bool _useTPforControls = false;
        [XmlElement("UseTPforControls")]
        public string UseTPforControlsString
        {
            get => _useTPforControls.ToString();
            set => _useTPforControls = XmlDataUtilities.StrToBool(value);
        }

        private bool _useSAforMouse;
        [XmlElement("UseSAforMouse")]
        public string UseSAforMouseString
        {
            get => _useSAforMouse.ToString();
            set => _useSAforMouse = XmlDataUtilities.StrToBool(value);
        }

        [XmlElement("SATriggers")]
        public string SATriggers
        {
            get; set;
        }

        private bool _sATriggerCond = true;
        [XmlElement("SATriggerCond")]
        public string SATriggerCondString
        {
            get => BackingStore.SaTriggerCondString(_sATriggerCond);
            set => _sATriggerCond = BackingStore.SaTriggerCondValue(value);
        }

        private SASteeringWheelEmulationAxisType _sASteeringWheelEmulationAxis = SASteeringWheelEmulationAxisType.None;
        [XmlElement("SASteeringWheelEmulationAxis")]
        public SASteeringWheelEmulationAxisType SASteeringWheelEmulationAxis
        {
            get => _sASteeringWheelEmulationAxis;
            set => _sASteeringWheelEmulationAxis = value;
        }

        private int _sASteeringWheelEmulationRange = 360;
        [XmlElement("SASteeringWheelEmulationRange")]
        public int SASteeringWheelEmulationRange
        {
            get => _sASteeringWheelEmulationRange;
            set => _sASteeringWheelEmulationRange = value;
        }

        [XmlElement("SASteeringWheelSmoothingOptions")]
        public SASteeringWheelSmoothingOptions SASteeringWheelSmoothingOptions
        {
            get; set;
        }

        private int _sASteeringWheelFuzz = 0;
        [XmlElement("SASteeringWheelFuzz")]
        public int SASteeringWheelFuzz
        {
            get => _sASteeringWheelFuzz;
            set => _sASteeringWheelFuzz = Math.Clamp(value, 0, 100);
        }

        [XmlElement("GyroOutputMode")]
        public GyroOutMode GyroOutputMode
        {
            get; set;
        }

        [XmlElement("GyroControlsSettings")]
        public GyroControlsSettings GyroControlsSettings
        {
            get; set;
        }

        private string _gyroMouseStickTriggers = "-1";
        [XmlElement("GyroMouseStickTriggers")]
        public string GyroMouseStickTriggers
        {
            get => _gyroMouseStickTriggers;
            set => _gyroMouseStickTriggers = value;
        }

        private bool _gyroMouseStickTriggerCond = true;
        [XmlElement("GyroMouseStickTriggerCond")]
        public string GyroMouseStickTriggerCondString
        {
            get => BackingStore.SaTriggerCondString(_gyroMouseStickTriggerCond);
            set => BackingStore.SaTriggerCondValue(value);
        }

        private bool _gyroMouseStickTriggerTurns = true;
        [XmlElement("GyroMouseStickTriggerTurns")]
        public string GyroMouseStickTriggerTurnsString
        {
            get => _gyroMouseStickTriggerTurns.ToString();
            set => _gyroMouseStickTriggerTurns = XmlDataUtilities.StrToBool(value);
        }

        private int _gyroMouseStickHAxis;
        [XmlElement("GyroMouseStickHAxis")]
        public int GyroMouseStickHAxis
        {
            get => _gyroMouseStickHAxis;
            set => _gyroMouseStickHAxis = Math.Clamp(value, 0, 1);
        }

        private int _gyroMouseStickDeadZone = 30;
        [XmlElement("GyroMouseStickDeadZone")]
        public int GyroMouseStickDeadZone
        {
            get => _gyroMouseStickDeadZone;
            set => _gyroMouseStickDeadZone = value;
        }

        private int _gyroMouseStickMaxZone = 830;
        [XmlElement("GyroMouseStickMaxZone")]
        public int GyroMouseStickMaxZone
        {
            get => _gyroMouseStickMaxZone;
            set => _gyroMouseStickMaxZone = Math.Max(1, value);
        }

        [XmlElement("GyroMouseStickOutputStick")]
        public GyroMouseStickInfo.OutputStick GyroMouseStickOutputStick
        {
            get; set;
        }

        [XmlElement("GyroMouseStickOutputStickAxes")]
        public GyroMouseStickInfo.OutputStickAxes GyroMouseStickOutputStickAxes
        {
            get; set;
        }

        private double _gyroMouseStickAntiDeadX = 0.40;
        [XmlElement("GyroMouseStickAntiDeadX")]
        public double GyroMouseStickAntiDeadX
        {
            get => _gyroMouseStickAntiDeadX;
            set => _gyroMouseStickAntiDeadX = Math.Clamp(value, 0.0, 1.0);
        }

        private double _gyroMouseStickAntiDeadY = 0.40;
        [XmlElement("GyroMouseStickAntiDeadY")]
        public double GyroMouseStickAntiDeadY
        {
            get => _gyroMouseStickAntiDeadY;
            set => _gyroMouseStickAntiDeadY = Math.Clamp(value, 0.0, 1.0);
        }

        private uint _gyroMouseStickInvert = 0;
        [XmlElement("GyroMouseStickInvert")]
        public uint GyroMouseStickInvert
        {
            get => _gyroMouseStickInvert;
            set => _gyroMouseStickInvert = value;
        }

        private bool _gyroMouseStickToggle;
        [XmlElement("GyroMouseStickToggle")]
        public string GyroMouseStickToggleString
        {
            get => _gyroMouseStickToggle.ToString();
            set => _gyroMouseStickToggle = XmlDataUtilities.StrToBool(value);
        }

        private double _gyroMouseStickMaxOutput = 100.0;
        [XmlElement("GyroMouseStickMaxOutput")]
        public double GyroMouseStickMaxOutput
        {
            get => _gyroMouseStickMaxOutput;
            set => _gyroMouseStickMaxOutput = Math.Clamp(value, 0.0, 100.0);
        }

        private bool _gyroMouseStickMaxOutputEnabled;
        [XmlElement("GyroMouseStickMaxOutputEnabled")]
        public string GyroMouseStickMaxOutputEnabledString
        {
            get => _gyroMouseStickMaxOutputEnabled.ToString();
            set => _gyroMouseStickMaxOutputEnabled = XmlDataUtilities.StrToBool(value);
        }

        private int _gyroMouseStickVerticalScale = 100;
        [XmlElement("GyroMouseStickVerticalScale")]
        public int GyroMouseStickVerticalScale
        {
            get => _gyroMouseStickVerticalScale;
            set => _gyroMouseStickVerticalScale = value;
        }

        [XmlElement("GyroMouseStickSmoothingSettings")]
        public GyroMouseStickSmoothingSettings GyroMouseStickSmoothingSettings
        {
            get; set;
        }

        [XmlElement("GyroSwipeSettings")]
        public GyroSwipeSettings GyroSwipeSettings
        {
            get; set;
        }

        [XmlElement("TouchpadOutputMode")]
        public TouchpadOutMode TouchpadOutputMode
        {
            get; set;
        }

        private int[] _touchDisInvTriggers;
        [XmlElement("TouchDisInvTriggers")]
        public string TouchDisInvTriggers
        {
            get; set;
        }

        private int _gyroSensitivity = 100;
        [XmlElement("GyroSensitivity")]
        public int GyroSensitivity
        {
            get => _gyroSensitivity;
            set => _gyroSensitivity = value;
        }

        private int _gyroSensVerticalScale = 100;
        [XmlElement("GyroSensVerticalScale")]
        public int GyroSensVerticalScale
        {
            get => _gyroSensVerticalScale;
            set => _gyroSensVerticalScale = value;
        }

        [XmlElement("GyroInvert")]
        public int GyroInvert
        {
            get; set;
        }

        private bool _gyroTriggerTurns;
        [XmlElement("GyroTriggerTurns")]
        public string GyroTriggerTurnsString
        {
            get => _gyroTriggerTurns.ToString();
            set => _gyroTriggerTurns = XmlDataUtilities.StrToBool(value);
        }

        [XmlElement("GyroMouseSmoothingSettings")]
        public GyroMouseSmoothingSettings GyroMouseSmoothingSettings
        {
            get; set;
        }

        private int _gyroMouseHAxis;
        [XmlElement("GyroMouseHAxis")]
        public int GyroMouseHAxis
        {
            get => _gyroMouseHAxis;
            set => _gyroMouseHAxis = Math.Clamp(value, 0, 1);
        }

        private int _gyroMouseDeadZone = MouseCursor.GYRO_MOUSE_DEADZONE;
        [XmlElement("GyroMouseDeadZone")]
        public int GyroMouseDeadZone
        {
            get => _gyroMouseDeadZone;
            set => _gyroMouseDeadZone = value;
        }

        private double _gyroMouseMinThreshold = GyroMouseInfo.DEFAULT_MIN_THRESHOLD;
        [XmlElement("GyroMouseMinThreshold")]
        public double GyroMouseMinThreshold
        {
            get => _gyroMouseMinThreshold;
            set => _gyroMouseMinThreshold = Math.Clamp(value, 1.0, 40.0);
        }

        private bool _gyroMouseToggle;
        [XmlElement("GyroMouseToggle")]
        public string GyroMouseToggleString
        {
            get => _gyroMouseToggle.ToString();
            set => _gyroMouseToggle = XmlDataUtilities.StrToBool(value);
        }

        private int _bTPollRate = 4;
        [XmlElement("BTPollRate")]
        public int BTPollRateString
        {
            get => _bTPollRate;
            set => _bTPollRate = Math.Clamp(value, 1, 16);
        }

        [XmlElement("LSOutputCurveCustom")]
        public string LSOutputCurveCustom
        {
            get; set;
        }

        [XmlElement("LSOutputCurveMode")]
        public string LSOutputCurveMode
        {
            get; set;
        }

        [XmlElement("RSOutputCurveCustom")]
        public string RSOutputCurveCustom
        {
            get; set;
        }

        [XmlElement("RSOutputCurveMode")]
        public string RSOutputCurveMode
        {
            get; set;
        }

        private bool _lsSquareStick;
        [XmlElement("LSSquareStick")]
        public string LSSquareStickString
        {
            get => _lsSquareStick.ToString();
            set => _lsSquareStick = XmlDataUtilities.StrToBool(value);
        }

        private double _squareStickRoundness = 5.0;
        [XmlElement("SquareStickRoundness")]
        public double SquareStickRoundness
        {
            get => _squareStickRoundness;
            set => _squareStickRoundness = value;
        }

        private double _squareRStickRoundness = 5.0;
        [XmlElement("SquareRStickRoundness")]
        public double SquareRStickRoundness
        {
            get => _squareRStickRoundness;
            set => _squareRStickRoundness = value;
        }

        private bool _rsSquareStick;
        [XmlElement("RSSquareStick")]
        public string RSSquareStickString
        {
            get => _rsSquareStick.ToString();
            set => _rsSquareStick = XmlDataUtilities.StrToBool(value);
        }

        private bool _lsAntiSnapback = StickAntiSnapbackInfo.DEFAULT_ENABLED;
        [XmlElement("LSAntiSnapback")]
        public string LSAntiSnapbackString
        {
            get => _lsAntiSnapback.ToString();
            set => _lsAntiSnapback = XmlDataUtilities.StrToBool(value);
        }

        private bool _rsAntiSnapback = StickAntiSnapbackInfo.DEFAULT_ENABLED;
        [XmlElement("RSAntiSnapback")]
        public string RSAntiSnapbackString
        {
            get => _rsAntiSnapback.ToString();
            set => _rsAntiSnapback = XmlDataUtilities.StrToBool(value);
        }

        private double _lsAntiSnapbackDelta = StickAntiSnapbackInfo.DEFAULT_DELTA;
        [XmlElement("LSAntiSnapbackDelta")]
        public double LSAntiSnapbackDelta
        {
            get => _lsAntiSnapbackDelta;
            set => _lsAntiSnapbackDelta = value;
        }

        private double _rsAntiSnapbackDelta = StickAntiSnapbackInfo.DEFAULT_DELTA;
        [XmlElement("RSAntiSnapbackDelta")]
        public double RSAntiSnapbackDelta
        {
            get => _rsAntiSnapbackDelta;
            set => _rsAntiSnapbackDelta = value;
        }

        private int _lsAntiSnapbackTimeout = StickAntiSnapbackInfo.DEFAULT_TIMEOUT;
        [XmlElement("LSAntiSnapbackTimeout")]
        public int LSAntiSnapbackTimeout
        {
            get => _lsAntiSnapbackTimeout;
            set => _lsAntiSnapbackTimeout = value;
        }

        private int _rsAntiSnapbackTimeout = StickAntiSnapbackInfo.DEFAULT_TIMEOUT;
        [XmlElement("RSAntiSnapbackTimeout")]
        public int RSAntiSnapbackTimeout
        {
            get => _rsAntiSnapbackTimeout;
            set => _rsAntiSnapbackTimeout = value;
        }

        [XmlElement("LSOutputMode")]
        public StickOutputSetting LSOutputMode
        {
            get; set;
        }

        [XmlElement("RSOutputMode")]
        public StickOutputSetting RSOutputMode
        {
            get; set;
        }

        [XmlElement("LSOutputSettings")]
        public StickModeOutputSettings LSOutputSettings
        {
            get; set;
        }

        [XmlElement("RSOutputSettings")]
        public StickModeOutputSettings RSOutputSettings
        {
            get; set;
        }

        [XmlElement("L2OutputCurveCustom")]
        public string L2OutputCurveCustom
        {
            get; set;
        }

        [XmlElement("L2OutputCurveMode")]
        public string L2OutputCurveMode
        {
            get; set;
        }

        [XmlElement("L2TwoStageMode")]
        public TwoStageTriggerMode L2TwoStageMode
        {
            get; set;
        }

        private int _l2HipFireTime;
        [XmlElement("L2HipFireTime")]
        public int L2HipFireTime
        {
            get => _l2HipFireTime;
            set => _l2HipFireTime = Math.Clamp(value, 0, 5000);
        }

        [XmlElement("L2TriggerEffect")]
        public DS4Windows.InputDevices.TriggerEffects L2TriggerEffect
        {
            get; set;
        }


        [XmlElement("R2OutputCurveCustom")]
        public string R2OutputCurveCustom
        {
            get; set;
        }

        [XmlElement("R2OutputCurveMode")]
        public string R2OutputCurveMode
        {
            get; set;
        }

        [XmlElement("R2TwoStageMode")]
        public TwoStageTriggerMode R2TwoStageMode
        {
            get; set;
        }

        private int _r2HipFireTime;
        [XmlElement("R2HipFireTime")]
        public int R2HipFireTime
        {
            get => _r2HipFireTime;
            set => _r2HipFireTime = Math.Clamp(value, 0, 5000);
        }

        [XmlElement("R2TriggerEffect")]
        public DS4Windows.InputDevices.TriggerEffects R2TriggerEffect
        {
            get; set;
        }


        [XmlElement("SXOutputCurveCustom")]
        public string SXOutputCurveCustom
        {
            get; set;
        }

        [XmlElement("SXOutputCurveMode")]
        public string SXOutputCurveMode
        {
            get; set;
        }

        [XmlElement("SZOutputCurveCustom")]
        public string SZOutputCurveCustom
        {
            get; set;
        }

        [XmlElement("SZOutputCurveMode")]
        public string SZOutputCurveMode
        {
            get; set;
        }

        private bool _trackballMode;
        [XmlElement("TrackballMode")]
        public string TrackballModeString
        {
            get => _trackballMode.ToString();
            set => _trackballMode = XmlDataUtilities.StrToBool(value);
        }

        private double _trackballFriction = 10.0;
        [XmlElement("TrackballFriction")]
        public double TrackballFriction
        {
            get => _trackballFriction;
            set => _trackballFriction = value;
        }

        private double _touchRelMouseRotation;
        [XmlElement("TouchRelMouseRotation")]
        public double TouchRelMouseRotation
        {
            get => _touchRelMouseRotation;
            set
            {
                double temp = Math.Clamp(value, -180.0, 180.0);
                _touchRelMouseRotation = temp * Math.PI / 180.0;
            }
        }

        private double _touchRelMouseMinThreshold = TouchpadRelMouseSettings.DEFAULT_MIN_THRESHOLD;
        [XmlElement("TouchRelMouseMinThreshold")]
        public double TouchRelMouseMinThreshold
        {
            get => _touchRelMouseMinThreshold;
            set
            {
                double temp = Math.Clamp(value, 1.0, 40.0);
                _touchRelMouseMinThreshold = temp;
            }
        }

        [XmlElement("TouchpadAbsMouseSettings")]
        public TouchpadAbsMouseSettings TouchpadAbsMouseSettings
        {
            get; set;
        }

        [XmlElement("OutputContDevice")]
        public OutContType OutputContDevice
        {
            get; set;
        }

        public ProfileDTO()
        {
            LSAxialDeadOptions = new StickAxialDeadOptionsSerializer();
            LSDeltaAccelSettings = new StickDeltaAccelSettings();
            RSAxialDeadOptions = new StickAxialDeadOptionsSerializer();
            RSDeltaAccelSettings = new StickDeltaAccelSettings();
            LaunchProgram = string.Empty;
            SATriggers = "-1";
            SASteeringWheelSmoothingOptions = new SASteeringWheelSmoothingOptions();
            GyroControlsSettings = new GyroControlsSettings();
            GyroMouseStickSmoothingSettings = new GyroMouseStickSmoothingSettings();
            GyroSwipeSettings = new GyroSwipeSettings();
            GyroMouseSmoothingSettings = new GyroMouseSmoothingSettings();
            LSOutputSettings = new StickModeOutputSettings();
            TouchpadAbsMouseSettings = new TouchpadAbsMouseSettings();
        }

        public void MapFrom(BackingStore source)
        {
            throw new NotImplementedException();
        }

        public void MapTo(BackingStore destination)
        {
            PostProcessXml();

            throw new NotImplementedException();
        }

        public void PostProcessXml()
        {
            //LowColor
            //ChargingColor
            //Sensitivity
            //LaunchProgram
            //StartTouchpadOff
            //UseTPforControls
            //GyroOutputMode
            //GyroControlsSettings/Toggle
            //GyroMouseStickToggle
            //GyroMouseStickSmoothingSettings/SmoothingMethod
            //TouchpadOutputMode
            //TouchDisInvTriggers
            //GyroMouseDeadZone
            //LSOutputCurveMode
            //RSOutputCurveMode
            //L2OutputCurveMode
            //R2OutputCurveMode
            //SXOutputCurveMode
            //SZOutputCurveMode
            //OutputContDevice
        }
    }

    public class StickAxialDeadOptionsSerializer
    {
        private int _deadZoneX;
        [XmlElement("DeadZoneX")]
        public int DeadZoneX
        {
            get => _deadZoneX;
            set => _deadZoneX = Math.Clamp(value, 0, 127);
        }

        private int _deadZoneY;
        [XmlElement("DeadZoneY")]
        public int DeadZoneY
        {
            get => _deadZoneY;
            set => _deadZoneY = Math.Clamp(value, 0, 127);
        }

        private int _maxZoneX;
        [XmlElement("MaxZoneX")]
        public int MaxZoneX
        {
            get => _maxZoneX;
            set => _maxZoneX = Math.Clamp(value, 0, 100);
        }

        private int _maxZoneY;
        [XmlElement("MaxZoneY")]
        public int MaxZoneY
        {
            get => _maxZoneY;
            set => _maxZoneY = Math.Clamp(value, 0, 100);
        }

        private int _antiDeadZoneX;
        [XmlElement("AntiDeadZoneX")]
        public int AntiDeadZoneX
        {
            get => _antiDeadZoneX;
            set => _antiDeadZoneX = Math.Clamp(value, 0, 100);
        }

        private int _antiDeadZoneY;
        [XmlElement("AntiDeadZoneY")]
        public int AntiDeadZoneY
        {
            get => _antiDeadZoneY;
            set => _antiDeadZoneY = Math.Clamp(value, 0, 100);
        }

        private double _maxOutputX;
        [XmlElement("MaxOutputX")]
        public double MaxOutputX
        {
            get => _maxOutputX;
            set => _maxOutputX = Math.Clamp(value, 0.0, 100.0);
        }

        private double _maxOutputY;
        [XmlElement("MaxOutputY")]
        public double MaxOutputY
        {
            get => _maxOutputY;
            set => _maxOutputY = Math.Clamp(value, 0.0, 100.0);
        }
    }

    public class StickDeltaAccelSettings
    {
        private bool _enabled;
        [XmlElement("Enabled")]
        public string EnabledString
        {
            get => _enabled.ToString();
            set => _enabled = XmlDataUtilities.StrToBool(value);
        }

        private double _multiplier;
        [XmlElement("Multiplier")]
        public double Multiplier
        {
            get => _multiplier;
            set => _multiplier = Math.Clamp(value, 0.0, 10.0);
        }

        private double _maxTravel;
        [XmlElement("MaxTravel")]
        public double MaxTravel
        {
            get => _maxTravel;
            set => _maxTravel = Math.Clamp(value, 0.0, 1.0);
        }

        private double _minTravel;
        [XmlElement("MinTravel")]
        public double MinTravel
        {
            get => _minTravel;
            set => _minTravel = Math.Clamp(value, 0.0, 1.0);
        }

        private double _easingDuration;
        [XmlElement("EasingDuration")]
        public double EasingDuration
        {
            get => _easingDuration;
            set => _easingDuration = Math.Clamp(value, 0.0, 600.0);
        }

        private double _minFactor = 1.0;
        [XmlElement("MinFactor")]
        public double MinFactor
        {
            get => _minFactor;
            set => _minFactor = Math.Clamp(value, 1.0, 10.0);
        }
    }

    public class SASteeringWheelSmoothingOptions
    {
        private bool _sASteeringWheelUseSmoothing;
        [XmlElement("SASteeringWheelUseSmoothing")]
        public string SASteeringWheelUseSmoothingString
        {
            get => _sASteeringWheelUseSmoothing.ToString();
            set => _sASteeringWheelUseSmoothing = XmlDataUtilities.StrToBool(value);
        }

        private double _sASteeringWheelSmoothMinCutoff = OneEuroFilterPair.DEFAULT_WHEEL_CUTOFF;
        [XmlElement("SASteeringWheelSmoothMinCutoff")]
        public double SASteeringWheelSmoothMinCutoff
        {
            get => _sASteeringWheelSmoothMinCutoff;
            set => _sASteeringWheelSmoothMinCutoff = value;
        }

        private double _sASteeringWheelSmoothBeta = OneEuroFilterPair.DEFAULT_WHEEL_BETA;
        [XmlElement("SASteeringWheelSmoothBeta")]
        public double SASteeringWheelSmoothBeta
        {
            get => _sASteeringWheelSmoothBeta;
            set => _sASteeringWheelSmoothBeta = value;
        }
    }

    public class GyroControlsSettings
    {
        [XmlElement("Triggers")]
        public string Triggers
        {
            get; set;
        }

        private bool _triggerCond;
        [XmlElement("TriggerCond")]
        public string TriggerCondString
        {
            get => BackingStore.SaTriggerCondString(_triggerCond);
            set => _triggerCond = BackingStore.SaTriggerCondValue(value);
        }

        private bool _triggerTurns;
        [XmlElement("TriggerTurns")]
        public string TriggerTurnsString
        {
            get => _triggerTurns.ToString();
            set => _triggerTurns = XmlDataUtilities.StrToBool(value);
        }

        private bool _toggle;
        [XmlElement("Toggle")]
        public string ToggleString
        {
            get => _toggle.ToString();
            set => _toggle = XmlDataUtilities.StrToBool(value);
        }
    }

    public class GyroMouseStickSmoothingSettings
    {
        protected bool _useSmoothing;
        [XmlElement("UseSmoothing")]
        public string UseSmoothingString
        {
            get => _useSmoothing.ToString();
            set => _useSmoothing = XmlDataUtilities.StrToBool(value);
        }

        protected string _smoothingMethod;
        [XmlElement("SmoothingMethod")]
        public string SmoothingMethod
        {
            get => _smoothingMethod;
            set => _smoothingMethod = value;
        }

        protected double _smoothingWeight = 0.5;
        [XmlElement("SmoothingWeight")]
        public double SmoothingWeight
        {
            get => _smoothingWeight;
            set => _smoothingWeight = Math.Clamp(value * 0.01, 0.0, 1.0);
        }

        protected double _smoothingMinCutoff = GyroMouseStickInfo.DEFAULT_MINCUTOFF;
        [XmlElement("SmoothingMinCutoff")]
        public double SmoothingMinCutoff
        {
            get => _smoothingMinCutoff;
            set => _smoothingMinCutoff = Math.Clamp(value, 0.0, 100.0);
        }

        protected double _smoothingBeta = GyroMouseStickInfo.DEFAULT_BETA;
        [XmlElement("SmoothingBeta")]
        public double SmoothingBeta
        {
            get => _smoothingBeta;
            set => _smoothingBeta = Math.Clamp(value, 0.0, 1.0);
        }
    }

    public class GyroMouseSmoothingSettings : GyroMouseStickSmoothingSettings
    {
    }

    public class GyroSwipeSettings
    {
        [XmlElement("DeadZoneX")]
        public int DeadZoneX
        {
            get; set;
        }

        [XmlElement("DeadZoneY")]
        public int DeadZoneY
        {
            get; set;
        }

        [XmlElement("Triggers")]
        public string Triggers
        {
            get; set;
        }

        private bool _triggerCond;
        [XmlElement("TriggerCond")]
        public string TriggerCondString
        {
            get => BackingStore.SaTriggerCondString(_triggerCond);
            set => _triggerCond = BackingStore.SaTriggerCondValue(value);
        }

        private bool _triggerTurns;
        [XmlElement("TriggerTurns")]
        public string TriggerTurnsString
        {
            get => _triggerTurns.ToString();
            set => _triggerTurns = XmlDataUtilities.StrToBool(value);
        }

        [XmlElement("XAxis")]
        public GyroDirectionalSwipeInfo.XAxisSwipe XAxis
        {
            get; set;
        }

        [XmlElement("DelayTime")]
        public int DelayTime
        {
            get; set;
        }
    }

    public class StickModeOutputSettings
    {
        [XmlElement("FlickStickSettings")]
        public FlickStickSettings FlickStickSettings
        {
            get; set;
        }

        public StickModeOutputSettings()
        {
            FlickStickSettings = new FlickStickSettings();
        }
    }

    public class FlickStickSettings
    {
        [XmlElement("RealWorldCalibration")]
        public double RealWorldCalibration
        {
            get; set;
        }

        [XmlElement("FlickThreshold")]
        public double FlickThreshold
        {
            get; set;
        }

        [XmlElement("FlickTime")]
        public double FlickTime
        {
            get; set;
        }

        [XmlElement("MinAngleThreshold")]
        public double MinAngleThreshold
        {
            get; set;
        }
    }

    public class TouchpadAbsMouseSettingsSerialize
    {
        private int _maxZoneX = TouchpadAbsMouseSettings.DEFAULT_MAXZONE_X;
        [XmlElement("MaxZoneX")]
        public int MaxZoneX
        {
            get => _maxZoneX;
            set => _maxZoneX = value;
        }

        private int _maxZoneY = TouchpadAbsMouseSettings.DEFAULT_MAXZONE_Y;
        [XmlElement("MaxZoneY")]
        public int MaxZoneY
        {
            get => _maxZoneY;
            set => _maxZoneY = value;
        }

        private bool _snapToCenter = TouchpadAbsMouseSettings.DEFAULT_SNAP_CENTER;
        [XmlElement("SnapToCenter")]
        public string SnapToCenterString
        {
            get => _snapToCenter.ToString();
            set => _snapToCenter = XmlDataUtilities.StrToBool(value);
        }
    }
}
