using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using DS4Windows;
using DS4Windows.InputDevices;
using DS4Windows.StickModifiers;
using static DS4Windows.Mouse;

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

        private DS4Color _ledColor = new DS4Color();
        [XmlElement("Color")]
        public string ColorString
        {
            get; set;
        }

        /// <summary>
        /// Older capatibility property
        /// </summary>
        [XmlElement("Red")]
        public string RedColorString
        {
            get; set;
        }

        /// <summary>
        /// Older capatibility property
        /// </summary>
        [XmlElement("Green")]
        public string GreenColorString
        {
            get; set;
        }

        /// <summary>
        /// Older capatibility property
        /// </summary>
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

        [XmlElement("LightbarMode")]
        public LightbarMode LightbarMode
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

        private DS4Color _lowColor = new DS4Color();
        [XmlElement("LowColor")]
        public string LowColorString
        {
            get; set;
        }

        /// <summary>
        /// Older capatibility property
        /// </summary>
        [XmlElement("LowRed")]
        public string LowRedColorString
        {
            get; set;
        }

        /// <summary>
        /// Older capatibility property
        /// </summary>
        [XmlElement("LowGreen")]
        public string LowGreenColorString
        {
            get; set;
        }

        /// <summary>
        /// Older capatibility property
        /// </summary>
        [XmlElement("LowBlue")]
        public string LowBlueColorString
        {
            get; set;
        }

        private DS4Color _chargingColor = new DS4Color();
        [XmlElement("ChargingColor")]
        public string ChargingColorString
        {
            get; set;
        }

        /// <summary>
        /// Older capatibility property
        /// </summary>
        [XmlElement("ChargingRed")]
        public string ChargingRedString
        {
            get; set;
        }

        /// <summary>
        /// Older capatibility property
        /// </summary>
        [XmlElement("ChargingGreen")]
        public string ChargingGreenString
        {
            get; set;
        }

        /// <summary>
        /// Older capatibility property
        /// </summary>
        [XmlElement("ChargingBlue")]
        public string ChargingBlueString
        {
            get; set;
        }

        private DS4Color _flashColor = new DS4Color();
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

        [XmlElement("Rainbow")]
        public double Rainbow
        {
            get; set;
        }

        private double _maxSatRainbow;
        [XmlElement("MaxSatRainbow")]
        public double MaxSatRainbow
        {
            get => _maxSatRainbow * 100.0;
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
            set => _lSVerticalScale = Math.Clamp(value, 0.0, 400.0);
        }

        private double _rSVerticalScale = StickDeadZoneInfo.DEFAULT_VERTICAL_SCALE;
        [XmlElement("RSVerticalScale")]
        public double RSVerticalScale
        {
            get => _rSVerticalScale;
            set => _rSVerticalScale = Math.Clamp(value, 0.0, 400.0);
        }

        private double _lSMaxOutput = 100.0;
        [XmlElement("LSMaxOutput")]
        public double LSMaxOutput
        {
            get => _lSMaxOutput;
            set => _lSMaxOutput = Math.Clamp(value, 0.0, 100.0);
        }

        private double _rSMaxOutput = 100.0;
        [XmlElement("RSMaxOutput")]
        public double RSMaxOutput
        {
            get => _rSMaxOutput;
            set => _rSMaxOutput = Math.Clamp(value, 0.0, 100.0);
        }

        private bool _lsMaxOutputForce = StickDeadZoneInfo.DEFAULT_MAXOUTPUT_FORCE;
        [XmlElement("LSMaxOutputForce")]
        public string LSMaxOutputForceString
        {
            get => _lsMaxOutputForce.ToString();
            set => _lsMaxOutputForce = XmlDataUtilities.StrToBool(value);
        }

        private bool _rsMaxOutputForce = StickDeadZoneInfo.DEFAULT_MAXOUTPUT_FORCE;
        [XmlElement("RSMaxOutputForce")]
        public string RSMaxOutputForceString
        {
            get => _rsMaxOutputForce.ToString();
            set => _rsMaxOutputForce = XmlDataUtilities.StrToBool(value);
        }

        [XmlElement("LSDeadZoneType")]
        public StickDeadZoneInfo.DeadZoneType LSDeadZoneType
        {
            get; set;
        }

        [XmlElement("RSDeadZoneType")]
        public StickDeadZoneInfo.DeadZoneType RSDeadZoneType
        {
            get; set;
        }

        [XmlElement("LSAxialDeadOptions")]
        public StickAxialDeadOptionsSerializer LSAxialDeadOptions
        {
            get; set;
        }

        [XmlElement("RSAxialDeadOptions")]
        public StickAxialDeadOptionsSerializer RSAxialDeadOptions
        {
            get; set;
        }


        private double _lSRotation;
        [XmlElement("LSRotation")]
        public double LSRotation
        {
            get => _lSRotation * 180.0 / Math.PI;
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
            get => _rSRotation * 180.0 / Math.PI;
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

        [XmlElement("LSDeltaAccelSettings")]
        public StickDeltaAccelSettings LSDeltaAccelSettings
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
            get => _sxMaxZone * 100.0;
            set => _sxMaxZone = Math.Clamp(value * 0.01, 0.0, 1.0);
        }

        private double _szMaxZone = 1.0;
        [XmlElement("SZMaxZone")]
        public double SZMaxZone
        {
            get => _szMaxZone * 100.0;
            set => _szMaxZone = Math.Clamp(value * 0.01, 0.0, 1.0);
        }

        private double _sxAntiDeadZone;
        [XmlElement("SXAntiDeadZone")]
        public double SXAntiDeadZone
        {
            get => _sxAntiDeadZone * 100.0;
            set => _sxAntiDeadZone = Math.Clamp(value * 0.01, 0.0, 1.0);
        }

        private double _szAntiDeadZone;
        [XmlElement("SZAntiDeadZone")]
        public double SZAntiDeadZone
        {
            get => _szAntiDeadZone * 100.0;
            set => _szAntiDeadZone = Math.Clamp(value * 0.01, 0.0, 1.0);
        }

        /// <summary>
        /// Older capatibility property
        /// </summary>
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

        private double _buttonMouseVerticalScale;
        [XmlElement("ButtonMouseVerticalScale")]
        public double ButtonMouseVerticalScale
        {
            get => _buttonMouseVerticalScale * 100.0;
            set => _buttonMouseVerticalScale = Math.Clamp(value, 0.0, 500.0) * 0.01;
        }

        /// <summary>
        /// WTF. Some local var?
        /// </summary>
        [XmlElement("ShiftModifier")]
        public int ShiftModifier
        {
            get; set;
        } = -1;
        public bool ShouldSerializeShiftModifier()
        {
            return ShiftModifier != -1;
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

        private bool _hasUseTPforControls;
        private bool _useTPforControls = false;
        [XmlElement("UseTPforControls")]
        public string UseTPforControlsString
        {
            get => _useTPforControls.ToString();
            set
            {
                _useTPforControls = XmlDataUtilities.StrToBool(value);
                _hasUseTPforControls = true;
            }
        }
        public bool ShouldSerializeUseTPforControlsString()
        {
            return false;
        }

        private bool _hasSAforMouse;
        private bool _useSAforMouse;
        [XmlElement("UseSAforMouse")]
        public string UseSAforMouseString
        {
            get => _useSAforMouse.ToString();
            set
            {
                _useSAforMouse = XmlDataUtilities.StrToBool(value);
                _hasSAforMouse = true;
            }
        }
        public bool ShouldSerializeUseSAforMouseString()
        {
            return false;
        }

        [XmlElement("TouchpadOutputMode")]
        public TouchpadOutMode TouchpadOutputMode
        {
            get; set;
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

        private int _sASteeringWheelFuzz = 0;
        [XmlElement("SASteeringWheelFuzz")]
        public int SASteeringWheelFuzz
        {
            get => _sASteeringWheelFuzz;
            set => _sASteeringWheelFuzz = Math.Clamp(value, 0, 100);
        }

        [XmlElement("SASteeringWheelSmoothingOptions")]
        public SASteeringWheelSmoothingOptions SASteeringWheelSmoothingOptions
        {
            get; set;
        }

        private int[] _touchDisInvTriggers = new int[1] { -1 };
        [XmlElement("TouchDisInvTriggers")]
        public string TouchDisInvTriggersString
        {
            get => string.Join(",", _touchDisInvTriggers);
            set
            {
                string[] triggers = value.Split(',');
                int temp = -1;
                List<int> tempIntList = new List<int>();
                for (int i = 0, arlen = triggers.Length; i < arlen; i++)
                {
                    if (int.TryParse(triggers[i], out temp))
                        tempIntList.Add(temp);
                }

                _touchDisInvTriggers = tempIntList.ToArray();
            }
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

        [XmlElement("GyroControlsSettings")]
        public GyroControlsSettings GyroControlsSettings
        {
            get; set;
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

        [XmlElement("GyroOutputMode")]
        public GyroOutMode GyroOutputMode
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
            set => _gyroMouseStickTriggerCond = BackingStore.SaTriggerCondValue(value);
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

        private int _bTPollRate = 4;
        [XmlElement("BTPollRate")]
        public int BTPollRateString
        {
            get => _bTPollRate;
            set => _bTPollRate = Math.Clamp(value, 1, 16);
        }

        [XmlElement("LSOutputCurveMode")]
        public string LSOutputCurveMode
        {
            get; set;
        }

        [XmlElement("LSOutputCurveCustom")]
        public string LSOutputCurveCustom
        {
            get; set;
        }

        [XmlElement("RSOutputCurveMode")]
        public string RSOutputCurveMode
        {
            get; set;
        }

        [XmlElement("RSOutputCurveCustom")]
        public string RSOutputCurveCustom
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

        private bool _rsSquareStick;
        [XmlElement("RSSquareStick")]
        public string RSSquareStickString
        {
            get => _rsSquareStick.ToString();
            set => _rsSquareStick = XmlDataUtilities.StrToBool(value);
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
        public StickMode LSOutputMode
        {
            get; set;
        }

        [XmlElement("RSOutputMode")]
        public StickMode RSOutputMode
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

        [XmlElement("DualSenseControllerSettings")]
        public DualSenseControllerSettings DualSenseControllerSettings
        {
            get; set;
        }

        [XmlElement("L2OutputCurveMode")]
        public string L2OutputCurveMode
        {
            get; set;
        }

        [XmlElement("L2OutputCurveCustom")]
        public string L2OutputCurveCustom
        {
            get; set;
        }

        [XmlElement("L2TwoStageMode")]
        public TwoStageTriggerMode L2TwoStageMode
        {
            get; set;
        }

        [XmlElement("R2TwoStageMode")]
        public TwoStageTriggerMode R2TwoStageMode
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

        private int _r2HipFireTime;
        [XmlElement("R2HipFireTime")]
        public int R2HipFireTime
        {
            get => _r2HipFireTime;
            set => _r2HipFireTime = Math.Clamp(value, 0, 5000);
        }

        [XmlElement("L2TriggerEffect")]
        public DS4Windows.InputDevices.TriggerEffects L2TriggerEffect
        {
            get; set;
        }

        [XmlElement("R2TriggerEffect")]
        public DS4Windows.InputDevices.TriggerEffects R2TriggerEffect
        {
            get; set;
        }

        [XmlElement("R2OutputCurveMode")]
        public string R2OutputCurveMode
        {
            get; set;
        }

        [XmlElement("R2OutputCurveCustom")]
        public string R2OutputCurveCustom
        {
            get; set;
        }

        [XmlElement("SXOutputCurveMode")]
        public string SXOutputCurveMode
        {
            get; set;
        }


        [XmlElement("SXOutputCurveCustom")]
        public string SXOutputCurveCustom
        {
            get; set;
        }

        [XmlElement("SZOutputCurveMode")]
        public string SZOutputCurveMode
        {
            get; set;
        }

        [XmlElement("SZOutputCurveCustom")]
        public string SZOutputCurveCustom
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
        public TouchpadAbsMouseSettingsSerialize TouchpadAbsMouseSettings
        {
            get; set;
        }

        [XmlElement("TouchpadMouseStick")]
        public TouchpadMouseStickSerializer TouchpadMouseStickSettings
        {
            get; set;
        }

        [XmlElement("TouchpadButtonMode")]
        public TouchButtonActivationMode TouchpadButtonMode
        {
            get; set;
        }

        [XmlElement("AbsMouseRegionSettings")]
        public AbsMouseRegionSettingsSerializer AbsMouseRegionSettings
        {
            get; set;
        }

        [XmlElement("OutputContDevice")]
        public OutContType OutputContDevice
        {
            get; set;
        }

        [XmlElement("ProfileActions")]
        public string ProfileActions
        {
            get; set;
        }

        [XmlElement("Control")]
        public DS4ControlAssignementSerializer Control
        {
            get; set;
        }

        [XmlElement("ShiftControl")]
        public DS4ControlAssignementSerializer ShiftControl
        {
            get; set;
        }

        private int deviceIndex = -1;
        [XmlIgnore]
        public int DeviceIndex
        {
            get => deviceIndex;
            set => deviceIndex = value;
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
            RSOutputSettings = new StickModeOutputSettings();
            DualSenseControllerSettings = new DualSenseControllerSettings();
            TouchpadAbsMouseSettings = new TouchpadAbsMouseSettingsSerialize();
            AbsMouseRegionSettings = new AbsMouseRegionSettingsSerializer();
            Control = new DS4ControlAssignementSerializer();
            ShiftControl = new DS4ControlAssignementSerializer();
        }

        public void MapFrom(BackingStore source)
        {
            if (deviceIndex == -1)
            {
                throw new ArgumentOutOfRangeException("Device Index must be set");
            }

            LightbarSettingInfo lightbarSettings = source.lightbarSettingInfo[deviceIndex];
            LightbarDS4WinInfo lightInfo = lightbarSettings.ds4winSettings;

            TouchToggle = source.enableTouchToggle[deviceIndex];
            IdleDisconnect = source.idleDisconnectTimeout[deviceIndex];
            OutputDataToDS4 = source.enableOutputDataToDS4[deviceIndex];
            LightbarMode = source.lightbarSettingInfo[deviceIndex].mode;
            ColorString = $"{lightInfo.m_Led.red},{lightInfo.m_Led.green},{lightInfo.m_Led.blue}";
            _ledColor = new DS4Color(lightInfo.m_Led.red, lightInfo.m_Led.green, lightInfo.m_Led.blue);
            RumbleBoost = source.rumble[deviceIndex];
            RumbleAutostopTime = source.rumbleAutostopTime[deviceIndex];
            LedAsBatteryIndicator = lightInfo.ledAsBattery;
            FlashType = lightInfo.flashType;
            FlashBatteryAt = lightInfo.flashAt;
            TouchSensitivity = source.touchSensitivity[deviceIndex];
            _lowColor = new DS4Color(lightInfo.m_LowLed.red, lightInfo.m_LowLed.green, lightInfo.m_LowLed.blue);
            LowColorString = $"{lightInfo.m_LowLed.red},{lightInfo.m_LowLed.green},{lightInfo.m_LowLed.blue}";

            _chargingColor = new DS4Color(lightInfo.m_ChargingLed.red, lightInfo.m_ChargingLed.green, lightInfo.m_ChargingLed.blue);
            ChargingColorString = $"{lightInfo.m_ChargingLed.red},{lightInfo.m_ChargingLed.green},{lightInfo.m_ChargingLed.blue}";

            _flashColor = new DS4Color(lightInfo.m_FlashLed.red, lightInfo.m_FlashLed.green, lightInfo.m_FlashLed.blue);
            FlashColorString = $"{lightInfo.m_FlashLed.red},{lightInfo.m_FlashLed.green},{lightInfo.m_FlashLed.blue}";

            TouchpadJitterCompensation = source.touchpadJitterCompensation[deviceIndex];
            LowerRCOn = source.lowerRCOn[deviceIndex];
            TapSensitivity = source.tapSensitivity[deviceIndex];
            DoubleTap = source.doubleTap[deviceIndex];
            ScrollSensitivity = source.scrollSensitivity[deviceIndex];
            TouchpadInvert = source.touchpadInvert[deviceIndex];
            TouchpadClickPassthru = source.touchClickPassthru[deviceIndex];
            LeftTriggerMiddle = source.l2ModInfo[deviceIndex].deadZone;
            RightTriggerMiddle = source.r2ModInfo[deviceIndex].deadZone;
            L2AntiDeadZone = source.l2ModInfo[deviceIndex].antiDeadZone;
            R2AntiDeadZone = source.r2ModInfo[deviceIndex].antiDeadZone;
            L2MaxZone = source.l2ModInfo[deviceIndex].maxZone;
            R2MaxZone = source.r2ModInfo[deviceIndex].maxZone;
            L2MaxOutput = source.l2ModInfo[deviceIndex].maxOutput;
            R2MaxOutput = source.r2ModInfo[deviceIndex].maxOutput;
            _lSRotation = source.LSRotation[deviceIndex];
            _rSRotation = source.RSRotation[deviceIndex];
            LSFuzz = source.lsModInfo[deviceIndex].fuzz;
            RSFuzz = source.rsModInfo[deviceIndex].fuzz;
            ButtonMouseSensitivity = source.buttonMouseInfos[deviceIndex].buttonSensitivity;
            ButtonMouseOffset = source.buttonMouseInfos[deviceIndex].mouseVelocityOffset;
            _buttonMouseVerticalScale = source.buttonMouseInfos[deviceIndex].buttonVerticalScale;
            Rainbow = lightInfo.rainbow;
            _maxSatRainbow = lightInfo.maxRainbowSat;
            LSDeadZone = source.lsModInfo[deviceIndex].deadZone;
            RSDeadZone = source.rsModInfo[deviceIndex].deadZone;
            LSAntiDeadZone = source.lsModInfo[deviceIndex].antiDeadZone;
            RSAntiDeadZone = source.rsModInfo[deviceIndex].antiDeadZone;
            LSMaxZone = source.lsModInfo[deviceIndex].maxZone;
            RSMaxZone = source.rsModInfo[deviceIndex].maxZone;
            LSVerticalScale = source.lsModInfo[deviceIndex].verticalScale;
            LSMaxOutput = source.lsModInfo[deviceIndex].maxOutput;
            _lsMaxOutputForce = source.lsModInfo[deviceIndex].maxOutputForce;

            RSVerticalScale = source.rsModInfo[deviceIndex].verticalScale;
            RSMaxOutput = source.rsModInfo[deviceIndex].maxOutput;
            _rsMaxOutputForce = source.rsModInfo[deviceIndex].maxOutputForce;

            _lSOuterBindDead = source.lsModInfo[deviceIndex].outerBindDeadZone;
            _rSOuterBindDead = source.rsModInfo[deviceIndex].outerBindDeadZone;

            _lSOuterBindInvert = source.lsModInfo[deviceIndex].outerBindInvert;
            _rSOuterBindInvert = source.rsModInfo[deviceIndex].outerBindInvert;

            LSDeadZoneType = source.lsModInfo[deviceIndex].deadzoneType;
            LSAxialDeadOptions = new StickAxialDeadOptionsSerializer()
            {
                DeadZoneX = source.lsModInfo[deviceIndex].xAxisDeadInfo.deadZone,
                DeadZoneY = source.lsModInfo[deviceIndex].yAxisDeadInfo.deadZone,
                MaxZoneX = source.lsModInfo[deviceIndex].xAxisDeadInfo.maxZone,
                MaxZoneY = source.lsModInfo[deviceIndex].yAxisDeadInfo.maxZone,
                AntiDeadZoneX = source.lsModInfo[deviceIndex].xAxisDeadInfo.antiDeadZone,
                AntiDeadZoneY = source.lsModInfo[deviceIndex].yAxisDeadInfo.antiDeadZone,
                MaxOutputX = source.lsModInfo[deviceIndex].xAxisDeadInfo.maxOutput,
                MaxOutputY = source.lsModInfo[deviceIndex].yAxisDeadInfo.maxOutput,
            };
            LSDeltaAccelSettings = new StickDeltaAccelSettings()
            {
                Enabled = source.lsOutputSettings[deviceIndex].outputSettings.controlSettings.deltaAccelSettings.enabled,
                Multiplier = source.lsOutputSettings[deviceIndex].outputSettings.controlSettings.deltaAccelSettings.multiplier,
                MaxTravel = source.lsOutputSettings[deviceIndex].outputSettings.controlSettings.deltaAccelSettings.maxTravel,
                MinTravel = source.lsOutputSettings[deviceIndex].outputSettings.controlSettings.deltaAccelSettings.minTravel,
                EasingDuration = source.lsOutputSettings[deviceIndex].outputSettings.controlSettings.deltaAccelSettings.easingDuration,
                MinFactor = source.lsOutputSettings[deviceIndex].outputSettings.controlSettings.deltaAccelSettings.minfactor,
            };

            RSDeadZoneType = source.rsModInfo[deviceIndex].deadzoneType;
            RSAxialDeadOptions = new StickAxialDeadOptionsSerializer()
            {
                DeadZoneX = source.rsModInfo[deviceIndex].xAxisDeadInfo.deadZone,
                DeadZoneY = source.rsModInfo[deviceIndex].yAxisDeadInfo.deadZone,
                MaxZoneX = source.rsModInfo[deviceIndex].xAxisDeadInfo.maxZone,
                MaxZoneY = source.rsModInfo[deviceIndex].yAxisDeadInfo.maxZone,
                AntiDeadZoneX = source.rsModInfo[deviceIndex].xAxisDeadInfo.antiDeadZone,
                AntiDeadZoneY = source.rsModInfo[deviceIndex].yAxisDeadInfo.antiDeadZone,
                MaxOutputX = source.rsModInfo[deviceIndex].xAxisDeadInfo.maxOutput,
                MaxOutputY = source.rsModInfo[deviceIndex].yAxisDeadInfo.maxOutput,
            };
            RSDeltaAccelSettings = new StickDeltaAccelSettings()
            {
                Enabled = source.rsOutputSettings[deviceIndex].outputSettings.controlSettings.deltaAccelSettings.enabled,
                Multiplier = source.rsOutputSettings[deviceIndex].outputSettings.controlSettings.deltaAccelSettings.multiplier,
                MaxTravel = source.rsOutputSettings[deviceIndex].outputSettings.controlSettings.deltaAccelSettings.maxTravel,
                MinTravel = source.rsOutputSettings[deviceIndex].outputSettings.controlSettings.deltaAccelSettings.minTravel,
                EasingDuration = source.rsOutputSettings[deviceIndex].outputSettings.controlSettings.deltaAccelSettings.easingDuration,
                MinFactor = source.rsOutputSettings[deviceIndex].outputSettings.controlSettings.deltaAccelSettings.minfactor,
            };

            SXDeadZone = source.SXDeadzone[deviceIndex];
            SZDeadZone = source.SZDeadzone[deviceIndex];
            _sxMaxZone = source.SXMaxzone[deviceIndex];
            _szMaxZone = source.SZMaxzone[deviceIndex];
            _sxAntiDeadZone = source.SXAntiDeadzone[deviceIndex];
            _szAntiDeadZone = source.SZAntiDeadzone[deviceIndex];

            Sensitivity = $"{source.LSSens[deviceIndex]}|{source.RSSens[deviceIndex]}|{source.l2Sens[deviceIndex]}|{source.r2Sens[deviceIndex]}|{source.SXSens[deviceIndex]}|{source.SZSens[deviceIndex]}";

            ChargingType = lightInfo.chargingType;
            _mouseAcceleration = source.buttonMouseInfos[deviceIndex].mouseAccel;
            //ShiftModifier = source.
            LaunchProgram = source.launchProgram[deviceIndex];
            _dinputOnly = source.dinputOnly[deviceIndex];
            _startTouchpadOff = source.startTouchpadOff[deviceIndex];
            _useTPforControls = source.touchOutMode[deviceIndex] == TouchpadOutMode.Controls;
            _useSAforMouse = source.gyroOutMode[deviceIndex] == GyroOutMode.Mouse;
            SATriggers = source.sATriggers[deviceIndex];
            _sATriggerCond = source.sATriggerCond[deviceIndex];
            SASteeringWheelEmulationAxis = source.sASteeringWheelEmulationAxis[deviceIndex];
            SASteeringWheelEmulationRange = source.sASteeringWheelEmulationRange[deviceIndex];
            SASteeringWheelSmoothingOptions = new SASteeringWheelSmoothingOptions()
            {
                SASteeringWheelUseSmoothing = source.wheelSmoothInfo[deviceIndex].enabled,
                SASteeringWheelSmoothMinCutoff = source.wheelSmoothInfo[deviceIndex].MinCutoff,
                SASteeringWheelSmoothBeta = source.wheelSmoothInfo[deviceIndex].Beta,
            };

            SASteeringWheelFuzz = source.saWheelFuzzValues[deviceIndex];
            GyroOutputMode = source.gyroOutMode[deviceIndex];
            GyroControlsSettings = new GyroControlsSettings()
            {
                Triggers = source.gyroControlsInf[deviceIndex].triggers,
                TriggerCond = source.gyroControlsInf[deviceIndex].triggerCond,
                TriggerTurns = source.gyroControlsInf[deviceIndex].triggerTurns,
                Toggle = source.gyroControlsInf[deviceIndex].triggerToggle,
            };

            _gyroMouseStickTriggers = source.sAMouseStickTriggers[deviceIndex];
            _gyroMouseStickTriggerCond = source.sAMouseStickTriggerCond[deviceIndex];
            _gyroMouseStickTriggerTurns = source.gyroMouseStickTriggerTurns[deviceIndex];
            GyroMouseStickHAxis = source.gyroMouseStickHorizontalAxis[deviceIndex];
            GyroMouseStickDeadZone = source.gyroMStickInfo[deviceIndex].deadZone;
            GyroMouseStickMaxZone = source.gyroMStickInfo[deviceIndex].maxZone;
            GyroMouseStickOutputStick = source.gyroMStickInfo[deviceIndex].outputStick;
            GyroMouseStickOutputStickAxes = source.gyroMStickInfo[deviceIndex].outputStickDir;
            GyroMouseStickAntiDeadX = source.gyroMStickInfo[deviceIndex].antiDeadX;
            GyroMouseStickAntiDeadY = source.gyroMStickInfo[deviceIndex].antiDeadY;
            GyroMouseStickInvert = source.gyroMStickInfo[deviceIndex].inverted;
            _gyroMouseStickToggle = source.gyroMouseStickToggle[deviceIndex];
            GyroMouseStickMaxOutput = source.gyroMStickInfo[deviceIndex].maxOutput;
            _gyroMouseStickMaxOutputEnabled = source.gyroMStickInfo[deviceIndex].maxOutputEnabled;
            GyroMouseStickVerticalScale = source.gyroMStickInfo[deviceIndex].vertScale;
            GyroMouseStickSmoothingSettings = new GyroMouseStickSmoothingSettings()
            {
                UseSmoothing = source.gyroMStickInfo[deviceIndex].useSmoothing,
                SmoothingMethod = source.gyroMStickInfo[deviceIndex].SmoothMethodIdentifier(),
                SmoothingWeightRaw = source.gyroMStickInfo[deviceIndex].smoothWeight,
                SmoothingMinCutoff = source.gyroMStickInfo[deviceIndex].minCutoff,
                SmoothingBeta = source.gyroMStickInfo[deviceIndex].beta,
            };
            GyroSwipeSettings = new GyroSwipeSettings()
            {
                DeadZoneX = source.gyroSwipeInfo[deviceIndex].deadzoneX,
                DeadZoneY = source.gyroSwipeInfo[deviceIndex].deadzoneY,
                Triggers = source.gyroSwipeInfo[deviceIndex].triggers,
                TriggerCond = source.gyroSwipeInfo[deviceIndex].triggerCond,
                TriggerTurns = source.gyroSwipeInfo[deviceIndex].triggerTurns,
                XAxis = source.gyroSwipeInfo[deviceIndex].xAxis,
                DelayTime = source.gyroSwipeInfo[deviceIndex].delayTime,
            };
            TouchpadOutputMode = source.touchOutMode[deviceIndex];
            _touchDisInvTriggers = source.touchDisInvertTriggers[deviceIndex];
            GyroSensitivity = source.gyroSensitivity[deviceIndex];
            GyroSensVerticalScale = source.gyroSensVerticalScale[deviceIndex];
            GyroInvert = source.gyroInvert[deviceIndex];
            _gyroTriggerTurns = source.gyroTriggerTurns[deviceIndex];
            GyroMouseSmoothingSettings = new GyroMouseSmoothingSettings()
            {
                UseSmoothing = source.gyroMouseInfo[deviceIndex].enableSmoothing,
                SmoothingMethod = source.gyroMouseInfo[deviceIndex].SmoothMethodIdentifier(),
                SmoothingWeightRaw = source.gyroMouseInfo[deviceIndex].smoothingWeight,
                SmoothingMinCutoff = source.gyroMouseInfo[deviceIndex].minCutoff,
                SmoothingBeta = source.gyroMouseInfo[deviceIndex].beta,
            };
            GyroMouseHAxis = source.gyroMouseHorizontalAxis[deviceIndex];
            GyroMouseDeadZone = source.gyroMouseDZ[deviceIndex];
            GyroMouseMinThreshold = source.gyroMouseInfo[deviceIndex].minThreshold;
            _gyroMouseToggle = source.gyroMouseToggle[deviceIndex];
            _bTPollRate = source.btPollRate[deviceIndex];

            LSOutputCurveCustom = source.lsOutBezierCurveObj[deviceIndex].CustomDefinition;
            LSOutputCurveMode = source.stickOutputCurveString(source.getLsOutCurveMode(deviceIndex));
            RSOutputCurveCustom = source.rsOutBezierCurveObj[deviceIndex].CustomDefinition;
            RSOutputCurveMode = source.stickOutputCurveString(source.getRsOutCurveMode(deviceIndex));
            _lsSquareStick = source.squStickInfo[deviceIndex].lsMode;
            SquareStickRoundness = source.squStickInfo[deviceIndex].lsRoundness;
            _rsSquareStick = source.squStickInfo[deviceIndex].rsMode;
            SquareRStickRoundness = source.squStickInfo[deviceIndex].rsRoundness;
            _lsAntiSnapback = source.lsAntiSnapbackInfo[deviceIndex].enabled;
            _rsAntiSnapback = source.rsAntiSnapbackInfo[deviceIndex].enabled;
            LSAntiSnapbackDelta = source.lsAntiSnapbackInfo[deviceIndex].delta;
            RSAntiSnapbackDelta = source.rsAntiSnapbackInfo[deviceIndex].delta;
            LSAntiSnapbackTimeout = source.lsAntiSnapbackInfo[deviceIndex].timeout;
            RSAntiSnapbackTimeout = source.rsAntiSnapbackInfo[deviceIndex].timeout;
            LSOutputMode = source.lsOutputSettings[deviceIndex].mode;
            RSOutputMode = source.rsOutputSettings[deviceIndex].mode;
            LSOutputSettings = new StickModeOutputSettings()
            {
                FlickStickSettings = new FlickStickSettings()
                {
                    RealWorldCalibration = source.lsOutputSettings[deviceIndex].outputSettings.flickSettings.realWorldCalibration,
                    FlickThreshold = source.lsOutputSettings[deviceIndex].outputSettings.flickSettings.flickThreshold,
                    FlickTime = source.lsOutputSettings[deviceIndex].outputSettings.flickSettings.flickTime,
                    MinAngleThreshold = source.lsOutputSettings[deviceIndex].outputSettings.flickSettings.minAngleThreshold,
                },
            };
            RSOutputSettings = new StickModeOutputSettings()
            {
                FlickStickSettings = new FlickStickSettings()
                {
                    RealWorldCalibration = source.rsOutputSettings[deviceIndex].outputSettings.flickSettings.realWorldCalibration,
                    FlickThreshold = source.rsOutputSettings[deviceIndex].outputSettings.flickSettings.flickThreshold,
                    FlickTime = source.rsOutputSettings[deviceIndex].outputSettings.flickSettings.flickTime,
                    MinAngleThreshold = source.rsOutputSettings[deviceIndex].outputSettings.flickSettings.minAngleThreshold,
                },
            };

            DualSenseControllerSettings = new DualSenseControllerSettings()
            {
                RumbleSettingsGroup = new DualSenseControllerSettings.RumbleSettings()
                {
                    EmulationMode = source.dualSenseRumbleEmulationMode[deviceIndex],
                    EnableGenericRumbleRescale = source.useGenericRumbleRescaleForDualSenses[deviceIndex],
                    HapticPowerLevel = source.dualSenseHapticPowerLevel[deviceIndex],
                }
            };

            L2OutputCurveCustom = source.l2OutBezierCurveObj[deviceIndex].CustomDefinition;
            L2OutputCurveMode = source.stickOutputCurveString(source.getL2OutCurveMode(deviceIndex));
            L2TwoStageMode = source.l2OutputSettings[deviceIndex].twoStageMode;
            L2HipFireTime = source.l2OutputSettings[deviceIndex].hipFireMS;
            L2TriggerEffect = source.l2OutputSettings[deviceIndex].triggerEffect;

            R2OutputCurveCustom = source.r2OutBezierCurveObj[deviceIndex].CustomDefinition;
            R2OutputCurveMode = source.stickOutputCurveString(source.getR2OutCurveMode(deviceIndex));
            R2TwoStageMode = source.r2OutputSettings[deviceIndex].twoStageMode;
            R2HipFireTime = source.r2OutputSettings[deviceIndex].hipFireMS;
            R2TriggerEffect = source.r2OutputSettings[deviceIndex].triggerEffect;

            SXOutputCurveCustom = source.sxOutBezierCurveObj[deviceIndex].CustomDefinition;
            SXOutputCurveMode = source.stickOutputCurveString(source.getSXOutCurveMode(deviceIndex));
            SZOutputCurveCustom = source.szOutBezierCurveObj[deviceIndex].CustomDefinition;
            SZOutputCurveMode = source.stickOutputCurveString(source.getSZOutCurveMode(deviceIndex));
            _trackballMode = source.trackballMode[deviceIndex];
            TrackballFriction = source.trackballFriction[deviceIndex];
            TouchRelMouseRotation = source.touchpadRelMouse[deviceIndex].rotation;
            TouchRelMouseMinThreshold = source.touchpadRelMouse[deviceIndex].minThreshold;
            TouchpadAbsMouseSettings = new TouchpadAbsMouseSettingsSerialize()
            {
                MaxZoneX = source.touchpadAbsMouse[deviceIndex].maxZoneX,
                MaxZoneY = source.touchpadAbsMouse[deviceIndex].maxZoneY,
                SnapToCenter = source.touchpadAbsMouse[deviceIndex].snapToCenter,
            };

            TouchpadMouseStickSettings = new TouchpadMouseStickSerializer()
            {
                DeadZone = source.touchMStickInfo[deviceIndex].deadZone,
                MaxZone = source.touchMStickInfo[deviceIndex].maxZone,
                OutputStick = source.touchMStickInfo[deviceIndex].outputStick,
                OutputStickAxes = source.touchMStickInfo[deviceIndex].outputStickDir,
                AntiDeadX = source.touchMStickInfo[deviceIndex].antiDeadX,
                AntiDeadY = source.touchMStickInfo[deviceIndex].antiDeadY,
                Invert = source.touchMStickInfo[deviceIndex].inverted,
                MaxOutput = source.touchMStickInfo[deviceIndex].maxOutput,
                MaxOutputEnabled = source.touchMStickInfo[deviceIndex].maxOutputEnabled,
                VerticalScale = source.touchMStickInfo[deviceIndex].vertScale,
                OutputCurve = source.touchMStickInfo[deviceIndex].outputCurve,
                RotationRad = source.touchMStickInfo[deviceIndex].rotationRad,
                SmoothingSettings = new TouchpadMouseStickSerializer.SmoothingGroupSerializer()
                {
                    SmoothingMethod = source.touchMStickInfo[deviceIndex].smoothingMethod,
                    SmoothingMinCutoff = source.touchMStickInfo[deviceIndex].minCutoff,
                    SmoothingBeta = source.touchMStickInfo[deviceIndex].beta,
                },
            };

            TouchpadButtonMode = source.touchpadButtonMode[deviceIndex];

            AbsMouseRegionSettings = new AbsMouseRegionSettingsSerializer()
            {
                AbsWidth = source.buttonAbsMouseInfos[deviceIndex].width,
                AbsHeight = source.buttonAbsMouseInfos[deviceIndex].height,
                AbsXCenter = source.buttonAbsMouseInfos[deviceIndex].xcenter,
                AbsYCenter = source.buttonAbsMouseInfos[deviceIndex].ycenter,
                AntiRadius = source.buttonAbsMouseInfos[deviceIndex].antiRadius,
                SnapToCenter = source.buttonAbsMouseInfos[deviceIndex].snapToCenter,
            };

            OutputContDevice = source.outputDevType[deviceIndex];
            ProfileActions = string.Join("/", source.profileActions[deviceIndex]);
            Control = new DS4ControlAssignementSerializer();
            ShiftControl = new DS4ControlAssignementSerializer();

            DS4ControlButtonAssignmentSerializer buttonSerializer = new DS4ControlButtonAssignmentSerializer();
            DS4ControlKeyAssignmentSerializer keySerializer = new DS4ControlKeyAssignmentSerializer();
            DS4ControlKeyTypeAssignmentSerializer keyTypeSerializer = new DS4ControlKeyTypeAssignmentSerializer();
            DS4ControlMacroAssignmentSerializer macroSerializer = new DS4ControlMacroAssignmentSerializer();
            DS4ControlExtrasAssignmentSerializer extrasSerializer = new DS4ControlExtrasAssignmentSerializer();

            DS4ControlButtonAssignmentSerializer shiftButtonSerializer = new DS4ControlButtonAssignmentSerializer();
            DS4ControlKeyAssignmentSerializer shiftKeySerializer = new DS4ControlKeyAssignmentSerializer();
            DS4ControlKeyTypeAssignmentSerializer shiftKeyTypeSerializer = new DS4ControlKeyTypeAssignmentSerializer();
            DS4ControlMacroAssignmentSerializer shiftMacroSerializer = new DS4ControlMacroAssignmentSerializer();
            DS4ControlExtrasAssignmentSerializer shiftExtrasSerializer = new DS4ControlExtrasAssignmentSerializer();

            foreach (DS4ControlSettings dcs in source.ds4settings[deviceIndex])
            {
                if (dcs.actionType != DS4ControlSettings.ActionType.Default)
                {
                    if (dcs.keyType != DS4KeyType.None)
                    {
                        keyTypeSerializer.CustomMapKeyTypes.Add(dcs.control, dcs.keyType);
                    }

                    if (dcs.actionType == DS4ControlSettings.ActionType.Button)
                    {
                        if (dcs.action.actionBtn == X360Controls.Unbound &&
                            !dcs.keyType.HasFlag(DS4KeyType.Unbound))
                        {
                            DS4KeyType tempFlags = DS4KeyType.Unbound;
                            if (keyTypeSerializer.CustomMapKeyTypes.ContainsKey(dcs.control))
                            {
                                tempFlags = keyTypeSerializer.CustomMapKeyTypes[dcs.control];
                                tempFlags |= DS4KeyType.Unbound;
                                keyTypeSerializer.CustomMapKeyTypes[dcs.control] = tempFlags;
                            }
                            else
                            {
                                keyTypeSerializer.CustomMapKeyTypes.Add(dcs.control, tempFlags);
                            }
                        }

                        buttonSerializer.CustomMapButtons.Add(dcs.control, dcs.action.actionBtn);
                    }
                    else if (dcs.actionType == DS4ControlSettings.ActionType.Key)
                    {
                        keySerializer.CustomMapKeys.Add(dcs.control, (ushort)dcs.action.actionKey);
                    }
                    else if (dcs.actionType == DS4ControlSettings.ActionType.Macro)
                    {
                        macroSerializer.CustomMapMacros.Add(dcs.control,
                            string.Join("/", dcs.action.actionMacro));
                    }
                }

                bool hasExtrasValue = false;
                if (!string.IsNullOrEmpty(dcs.extras))
                {
                    foreach (string s in dcs.extras.Split(','))
                    {
                        if (s != "0")
                        {
                            hasExtrasValue = true;
                            break;
                        }
                    }
                }

                if (hasExtrasValue)
                {
                    extrasSerializer.CustomMapExtras.Add(dcs.control, dcs.extras);
                }

                if (dcs.shiftActionType != DS4ControlSettings.ActionType.Default && dcs.shiftTrigger > 0)
                {
                    if (dcs.shiftKeyType != DS4KeyType.None)
                    {
                        shiftKeyTypeSerializer.CustomMapKeyTypes.Add(dcs.control, dcs.shiftKeyType);
                    }

                    if (dcs.shiftActionType == DS4ControlSettings.ActionType.Button)
                    {
                        if (dcs.shiftAction.actionBtn == X360Controls.Unbound &&
                            !dcs.shiftKeyType.HasFlag(DS4KeyType.Unbound))
                        {
                            DS4KeyType tempFlags = DS4KeyType.Unbound;
                            if (shiftKeyTypeSerializer.CustomMapKeyTypes.ContainsKey(dcs.control))
                            {
                                tempFlags = shiftKeyTypeSerializer.CustomMapKeyTypes[dcs.control];
                                tempFlags |= DS4KeyType.Unbound;
                                shiftKeyTypeSerializer.CustomMapKeyTypes[dcs.control] = tempFlags;
                            }
                            else
                            {
                                shiftKeyTypeSerializer.CustomMapKeyTypes.Add(dcs.control, tempFlags);
                            }
                        }

                        shiftButtonSerializer.CustomMapButtons.Add(dcs.control, dcs.shiftAction.actionBtn);
                        shiftButtonSerializer.ShiftTriggers.TryAdd(dcs.control, dcs.shiftTrigger);
                    }
                    else if (dcs.shiftActionType == DS4ControlSettings.ActionType.Key)
                    {
                        shiftKeySerializer.CustomMapKeys.Add(dcs.control, (ushort)dcs.shiftAction.actionKey);
                        shiftKeySerializer.ShiftTriggers.TryAdd(dcs.control, dcs.shiftTrigger);
                    }
                    else if (dcs.shiftActionType == DS4ControlSettings.ActionType.Macro)
                    {
                        shiftMacroSerializer.CustomMapMacros.Add(dcs.control,
                            string.Join("/", dcs.shiftAction.actionMacro));
                        shiftMacroSerializer.ShiftTriggers.TryAdd(dcs.control, dcs.shiftTrigger);
                    }
                }

                hasExtrasValue = false;
                if (!string.IsNullOrEmpty(dcs.shiftExtras))
                {
                    foreach (string s in dcs.shiftExtras.Split(','))
                    {
                        if (s != "0")
                        {
                            hasExtrasValue = true;
                            break;
                        }
                    }
                }
            }

            if (buttonSerializer.CustomMapButtons.Count > 0)
            {
                Control.Button = buttonSerializer;
            }

            if (keySerializer.CustomMapKeys.Count > 0)
            {
                Control.Key = keySerializer;
            }

            if (keyTypeSerializer.CustomMapKeyTypes.Count > 0)
            {
                Control.KeyType = keyTypeSerializer;
            }

            if (macroSerializer.CustomMapMacros.Count > 0)
            {
                Control.Macro = macroSerializer;
            }

            if (extrasSerializer.CustomMapExtras.Count > 0)
            {
                Control.Extras = extrasSerializer;
            }


            if (shiftButtonSerializer.CustomMapButtons.Count > 0)
            {
                ShiftControl.Button = shiftButtonSerializer;
            }

            if (shiftKeySerializer.CustomMapKeys.Count > 0)
            {
                ShiftControl.Key = shiftKeySerializer;
            }

            if (shiftKeyTypeSerializer.CustomMapKeyTypes.Count > 0)
            {
                ShiftControl.KeyType = shiftKeyTypeSerializer;
            }

            if (shiftMacroSerializer.CustomMapMacros.Count > 0)
            {
                ShiftControl.Macro = shiftMacroSerializer;
            }

            if (shiftExtrasSerializer.CustomMapExtras.Count > 0)
            {
                ShiftControl.Extras = shiftExtrasSerializer;
            }
        }

        public void MapTo(BackingStore destination)
        {
            if (deviceIndex == -1)
            {
                throw new ArgumentOutOfRangeException("Device Index must be set");
            }

            PostProcessXml();

            LightbarSettingInfo lightbarSettings = destination.lightbarSettingInfo[deviceIndex];
            LightbarDS4WinInfo lightInfo = lightbarSettings.ds4winSettings;

            destination.enableTouchToggle[deviceIndex] = TouchToggle;
            destination.idleDisconnectTimeout[deviceIndex] = IdleDisconnect;
            destination.enableOutputDataToDS4[deviceIndex] = OutputDataToDS4;
            destination.lightbarSettingInfo[deviceIndex].mode = LightbarMode;
            lightInfo.m_Led = _ledColor;

            destination.rumble[deviceIndex] = RumbleBoost;
            destination.rumbleAutostopTime[deviceIndex] = RumbleAutostopTime;
            lightInfo.ledAsBattery = LedAsBatteryIndicator;
            lightInfo.flashType = FlashType;
            lightInfo.flashAt = FlashBatteryAt;
            destination.touchSensitivity[deviceIndex] = TouchSensitivity;
            lightInfo.m_LowLed = _lowColor;
            lightInfo.m_ChargingLed = _chargingColor;
            lightInfo.m_FlashLed = _flashColor;

            destination.touchpadJitterCompensation[deviceIndex] = TouchpadJitterCompensation;
            destination.lowerRCOn[deviceIndex] = LowerRCOn;
            destination.tapSensitivity[deviceIndex] = TapSensitivity;
            destination.doubleTap[deviceIndex] = DoubleTap;
            destination.scrollSensitivity[deviceIndex] = ScrollSensitivity;
            destination.touchpadInvert[deviceIndex] = TouchpadInvert;
            destination.touchClickPassthru[deviceIndex] = TouchpadClickPassthru;
            destination.l2ModInfo[deviceIndex].deadZone = LeftTriggerMiddle;
            destination.r2ModInfo[deviceIndex].deadZone = RightTriggerMiddle;
            destination.l2ModInfo[deviceIndex].antiDeadZone = L2AntiDeadZone;
            destination.r2ModInfo[deviceIndex].antiDeadZone = R2AntiDeadZone;
            destination.l2ModInfo[deviceIndex].maxZone = L2MaxZone;
            destination.r2ModInfo[deviceIndex].maxZone = R2MaxZone;
            destination.l2ModInfo[deviceIndex].maxOutput = L2MaxOutput;
            destination.r2ModInfo[deviceIndex].maxOutput = R2MaxOutput;
            destination.LSRotation[deviceIndex] = _lSRotation;
            destination.RSRotation[deviceIndex] = _rSRotation;
            destination.lsModInfo[deviceIndex].fuzz = LSFuzz;
            destination.rsModInfo[deviceIndex].fuzz = RSFuzz;
            destination.buttonMouseInfos[deviceIndex].buttonSensitivity = ButtonMouseSensitivity;
            destination.buttonMouseInfos[deviceIndex].mouseVelocityOffset = ButtonMouseOffset;
            destination.buttonMouseInfos[deviceIndex].buttonVerticalScale = _buttonMouseVerticalScale;
            lightInfo.rainbow = Rainbow;
            lightInfo.maxRainbowSat = _maxSatRainbow;
            destination.lsModInfo[deviceIndex].deadZone = LSDeadZone;
            destination.rsModInfo[deviceIndex].deadZone = RSDeadZone;
            destination.lsModInfo[deviceIndex].antiDeadZone = LSAntiDeadZone;
            destination.rsModInfo[deviceIndex].antiDeadZone = RSAntiDeadZone;
            destination.lsModInfo[deviceIndex].maxZone = LSMaxZone;
            destination.rsModInfo[deviceIndex].maxZone = RSMaxZone;
            destination.lsModInfo[deviceIndex].verticalScale = LSVerticalScale;
            destination.lsModInfo[deviceIndex].maxOutput = LSMaxOutput;
            destination.lsModInfo[deviceIndex].maxOutputForce = _lsMaxOutputForce;

            destination.rsModInfo[deviceIndex].verticalScale = RSVerticalScale;
            destination.rsModInfo[deviceIndex].maxOutput = RSMaxOutput;
            destination.rsModInfo[deviceIndex].maxOutputForce = _rsMaxOutputForce;

            destination.lsModInfo[deviceIndex].outerBindDeadZone = _lSOuterBindDead;
            destination.rsModInfo[deviceIndex].outerBindDeadZone = _rSOuterBindDead;

            destination.lsModInfo[deviceIndex].outerBindInvert = _lSOuterBindInvert;
            destination.rsModInfo[deviceIndex].outerBindInvert = _rSOuterBindInvert;

            destination.lsModInfo[deviceIndex].deadzoneType = LSDeadZoneType;

            if (LSAxialDeadOptions != null)
            {
                destination.lsModInfo[deviceIndex].xAxisDeadInfo.deadZone = LSAxialDeadOptions.DeadZoneX;
                destination.lsModInfo[deviceIndex].yAxisDeadInfo.deadZone = LSAxialDeadOptions.DeadZoneY;
                destination.lsModInfo[deviceIndex].xAxisDeadInfo.maxZone = LSAxialDeadOptions.MaxZoneX;
                destination.lsModInfo[deviceIndex].yAxisDeadInfo.maxZone = LSAxialDeadOptions.MaxZoneY;
                destination.lsModInfo[deviceIndex].xAxisDeadInfo.antiDeadZone = LSAxialDeadOptions.AntiDeadZoneX;
                destination.lsModInfo[deviceIndex].yAxisDeadInfo.antiDeadZone = LSAxialDeadOptions.AntiDeadZoneY;
                destination.lsModInfo[deviceIndex].xAxisDeadInfo.maxOutput = LSAxialDeadOptions.MaxOutputX;
                destination.lsModInfo[deviceIndex].yAxisDeadInfo.maxOutput = LSAxialDeadOptions.MaxOutputY;
            }

            if (LSDeltaAccelSettings != null)
            {
                destination.lsOutputSettings[deviceIndex].outputSettings.controlSettings.deltaAccelSettings.enabled = LSDeltaAccelSettings.Enabled;
                destination.lsOutputSettings[deviceIndex].outputSettings.controlSettings.deltaAccelSettings.multiplier = LSDeltaAccelSettings.Multiplier;
                destination.lsOutputSettings[deviceIndex].outputSettings.controlSettings.deltaAccelSettings.maxTravel = LSDeltaAccelSettings.MaxTravel;
                destination.lsOutputSettings[deviceIndex].outputSettings.controlSettings.deltaAccelSettings.minTravel = LSDeltaAccelSettings.MinTravel;
                destination.lsOutputSettings[deviceIndex].outputSettings.controlSettings.deltaAccelSettings.easingDuration = LSDeltaAccelSettings.EasingDuration;
                destination.lsOutputSettings[deviceIndex].outputSettings.controlSettings.deltaAccelSettings.minfactor = LSDeltaAccelSettings.MinFactor;
            }

            destination.rsModInfo[deviceIndex].deadzoneType = RSDeadZoneType;
            if (RSAxialDeadOptions != null)
            {
                destination.rsModInfo[deviceIndex].xAxisDeadInfo.deadZone = RSAxialDeadOptions.DeadZoneX;
                destination.rsModInfo[deviceIndex].yAxisDeadInfo.deadZone = RSAxialDeadOptions.DeadZoneY;
                destination.rsModInfo[deviceIndex].xAxisDeadInfo.maxZone = RSAxialDeadOptions.MaxZoneX;
                destination.rsModInfo[deviceIndex].yAxisDeadInfo.maxZone = RSAxialDeadOptions.MaxZoneY;
                destination.rsModInfo[deviceIndex].xAxisDeadInfo.antiDeadZone = RSAxialDeadOptions.AntiDeadZoneX;
                destination.rsModInfo[deviceIndex].yAxisDeadInfo.antiDeadZone = RSAxialDeadOptions.AntiDeadZoneY;
                destination.rsModInfo[deviceIndex].xAxisDeadInfo.maxOutput = RSAxialDeadOptions.MaxOutputX;
                destination.rsModInfo[deviceIndex].yAxisDeadInfo.maxOutput = RSAxialDeadOptions.MaxOutputY;
            }

            if (RSDeltaAccelSettings != null)
            {
                destination.rsOutputSettings[deviceIndex].outputSettings.controlSettings.deltaAccelSettings.enabled = RSDeltaAccelSettings.Enabled;
                destination.rsOutputSettings[deviceIndex].outputSettings.controlSettings.deltaAccelSettings.multiplier = RSDeltaAccelSettings.Multiplier;
                destination.rsOutputSettings[deviceIndex].outputSettings.controlSettings.deltaAccelSettings.maxTravel = RSDeltaAccelSettings.MaxTravel;
                destination.rsOutputSettings[deviceIndex].outputSettings.controlSettings.deltaAccelSettings.minTravel = RSDeltaAccelSettings.MinTravel;
                destination.rsOutputSettings[deviceIndex].outputSettings.controlSettings.deltaAccelSettings.easingDuration = RSDeltaAccelSettings.EasingDuration;
                destination.rsOutputSettings[deviceIndex].outputSettings.controlSettings.deltaAccelSettings.minfactor = RSDeltaAccelSettings.MinFactor;
            }

            destination.SXDeadzone[deviceIndex] = SXDeadZone;
            destination.SZDeadzone[deviceIndex] = SZDeadZone;
            destination.SXMaxzone[deviceIndex] = _sxMaxZone;
            destination.SZMaxzone[deviceIndex] = _szMaxZone;
            destination.SXAntiDeadzone[deviceIndex] = _sxAntiDeadZone;
            destination.SZAntiDeadzone[deviceIndex] = _szAntiDeadZone;

            if (!string.IsNullOrEmpty(Sensitivity))
            {
                string[] s = Sensitivity.Split('|');
                if (s.Length == 1)
                    s = Sensitivity.Split(',');

                try
                {
                    if (double.TryParse(s[0], out double temp))
                    {
                        destination.LSSens[deviceIndex] = Math.Clamp(temp, 0.1, 5.0);
                    }
                    else
                    {
                        destination.LSSens[deviceIndex] = 1.0;
                    }

                    if (double.TryParse(s[1], out double tempRSSens))
                    {
                        destination.RSSens[deviceIndex] = Math.Clamp(tempRSSens, 0.1, 5.0);
                    }
                    else
                    {
                        destination.RSSens[deviceIndex] = 1.0;
                    }

                    if (double.TryParse(s[2], out double tempL2Sens))
                    {
                        destination.l2Sens[deviceIndex] = Math.Clamp(tempL2Sens, 0.1, 10.0);
                    }
                    else
                    {
                        destination.l2Sens[deviceIndex] = 1.0;
                    }

                    if (double.TryParse(s[3], out double tempR2Sens))
                    {
                        destination.r2Sens[deviceIndex] = Math.Clamp(tempR2Sens, 0.1, 10.0);
                    }
                    else
                    {
                        destination.r2Sens[deviceIndex] = 1.0;
                    }

                    if (double.TryParse(s[4], out double tempSXSens))
                    {
                        destination.SXSens[deviceIndex] = Math.Clamp(tempSXSens, 0.0, 5.0);
                    }
                    else
                    {
                        destination.SXSens[deviceIndex] = 1.0;
                    }

                    if (double.TryParse(s[5], out double tempSZSens))
                    {
                        destination.SZSens[deviceIndex] = Math.Clamp(tempSZSens, 0.0, 5.0);
                    }
                    else
                    {
                        destination.SZSens[deviceIndex] = 1.0;
                    }
                }
                catch { }
            }

            lightInfo.chargingType = ChargingType;
            destination.buttonMouseInfos[deviceIndex].mouseAccel = _mouseAcceleration;
            //destination. = ShiftModifier 
            destination.launchProgram[deviceIndex] = LaunchProgram;
            destination.dinputOnly[deviceIndex] = _dinputOnly;
            destination.startTouchpadOff[deviceIndex] = _startTouchpadOff;
            destination.sATriggers[deviceIndex] = SATriggers;
            destination.sATriggerCond[deviceIndex] = _sATriggerCond;
            destination.sASteeringWheelEmulationAxis[deviceIndex] = SASteeringWheelEmulationAxis;
            destination.sASteeringWheelEmulationRange[deviceIndex] = SASteeringWheelEmulationRange;

            if (SASteeringWheelSmoothingOptions != null)
            {
                destination.wheelSmoothInfo[deviceIndex].enabled = SASteeringWheelSmoothingOptions.SASteeringWheelUseSmoothing;
                destination.wheelSmoothInfo[deviceIndex].MinCutoff = SASteeringWheelSmoothingOptions.SASteeringWheelSmoothMinCutoff;
                destination.wheelSmoothInfo[deviceIndex].Beta = SASteeringWheelSmoothingOptions.SASteeringWheelSmoothBeta;
            }

            destination.saWheelFuzzValues[deviceIndex] = SASteeringWheelFuzz;
            if (_hasSAforMouse)
            {
                destination.gyroOutMode[deviceIndex] =
                    _useSAforMouse ? GyroOutMode.Mouse : GyroOutMode.Controls;
            }
            else
            {
                destination.gyroOutMode[deviceIndex] = GyroOutputMode;
            }

            if (GyroControlsSettings != null)
            {
                destination.gyroControlsInf[deviceIndex].triggers = GyroControlsSettings.Triggers;
                destination.gyroControlsInf[deviceIndex].triggerCond = GyroControlsSettings.TriggerCond;
                destination.gyroControlsInf[deviceIndex].triggerTurns = GyroControlsSettings.TriggerTurns;
                destination.gyroControlsInf[deviceIndex].triggerToggle = GyroControlsSettings.Toggle;
            }

            destination.sAMouseStickTriggers[deviceIndex] = _gyroMouseStickTriggers;
            destination.sAMouseStickTriggerCond[deviceIndex] = _gyroMouseStickTriggerCond;
            destination.gyroMouseStickTriggerTurns[deviceIndex] = _gyroMouseStickTriggerTurns;
            destination.gyroMouseStickHorizontalAxis[deviceIndex] = GyroMouseStickHAxis;
            destination.gyroMStickInfo[deviceIndex].deadZone = GyroMouseStickDeadZone;
            destination.gyroMStickInfo[deviceIndex].maxZone = GyroMouseStickMaxZone;
            destination.gyroMStickInfo[deviceIndex].outputStick = GyroMouseStickOutputStick;
            destination.gyroMStickInfo[deviceIndex].outputStickDir = GyroMouseStickOutputStickAxes;
            destination.gyroMStickInfo[deviceIndex].antiDeadX = GyroMouseStickAntiDeadX;
            destination.gyroMStickInfo[deviceIndex].antiDeadY = GyroMouseStickAntiDeadY;
            destination.gyroMStickInfo[deviceIndex].inverted = GyroMouseStickInvert;
            destination.gyroMouseStickToggle[deviceIndex] = _gyroMouseStickToggle;
            destination.gyroMStickInfo[deviceIndex].maxOutput = GyroMouseStickMaxOutput;
            destination.gyroMStickInfo[deviceIndex].maxOutputEnabled = _gyroMouseStickMaxOutputEnabled;
            destination.gyroMStickInfo[deviceIndex].vertScale = GyroMouseStickVerticalScale;

            if (GyroMouseStickSmoothingSettings != null)
            {
                destination.gyroMStickInfo[deviceIndex].useSmoothing = GyroMouseStickSmoothingSettings.UseSmoothing;
                destination.gyroMStickInfo[deviceIndex].smoothingMethod = GyroMouseStickInfo.SmoothingMethodParse(GyroMouseStickSmoothingSettings.SmoothingMethod);
                destination.gyroMStickInfo[deviceIndex].smoothWeight = GyroMouseStickSmoothingSettings.SmoothingWeightRaw;
                destination.gyroMStickInfo[deviceIndex].minCutoff = GyroMouseStickSmoothingSettings.SmoothingMinCutoff;
                destination.gyroMStickInfo[deviceIndex].beta = GyroMouseStickSmoothingSettings.SmoothingBeta;
            }

            if (GyroSwipeSettings != null)
            {
                destination.gyroSwipeInfo[deviceIndex].deadzoneX = GyroSwipeSettings.DeadZoneX;
                destination.gyroSwipeInfo[deviceIndex].deadzoneY = GyroSwipeSettings.DeadZoneY;
                destination.gyroSwipeInfo[deviceIndex].triggers = GyroSwipeSettings.Triggers;
                destination.gyroSwipeInfo[deviceIndex].triggerCond = GyroSwipeSettings.TriggerCond;
                destination.gyroSwipeInfo[deviceIndex].triggerTurns = GyroSwipeSettings.TriggerTurns;
                destination.gyroSwipeInfo[deviceIndex].xAxis = GyroSwipeSettings.XAxis;
                destination.gyroSwipeInfo[deviceIndex].delayTime = GyroSwipeSettings.DelayTime;
            }

            if (_hasUseTPforControls)
            {
                destination.touchOutMode[deviceIndex] =
                    _useTPforControls ? TouchpadOutMode.Controls : TouchpadOutMode.Mouse;
            }
            else
            {
                destination.touchOutMode[deviceIndex] = TouchpadOutputMode;
            }

            destination.touchDisInvertTriggers[deviceIndex] = _touchDisInvTriggers;
            destination.gyroSensitivity[deviceIndex] = GyroSensitivity;
            destination.gyroSensVerticalScale[deviceIndex] = GyroSensVerticalScale;
            destination.gyroInvert[deviceIndex] = GyroInvert;
            destination.gyroTriggerTurns[deviceIndex] = _gyroTriggerTurns;

            if (GyroMouseSmoothingSettings != null)
            {
                destination.gyroMouseInfo[deviceIndex].enableSmoothing = GyroMouseSmoothingSettings.UseSmoothing;
                destination.gyroMouseInfo[deviceIndex].smoothingMethod = GyroMouseInfo.SmoothingMethodParse(GyroMouseSmoothingSettings.SmoothingMethod);
                destination.gyroMouseInfo[deviceIndex].smoothingWeight = GyroMouseSmoothingSettings.SmoothingWeightRaw;
                destination.gyroMouseInfo[deviceIndex].minCutoff = GyroMouseSmoothingSettings.SmoothingMinCutoff;
                destination.gyroMouseInfo[deviceIndex].beta = GyroMouseSmoothingSettings.SmoothingBeta;
            }

            destination.gyroMouseHorizontalAxis[deviceIndex] = GyroMouseHAxis;
            destination.gyroMouseDZ[deviceIndex] = GyroMouseDeadZone;
            destination.gyroMouseInfo[deviceIndex].minThreshold = GyroMouseMinThreshold;
            destination.gyroMouseToggle[deviceIndex] = _gyroMouseToggle;
            destination.btPollRate[deviceIndex] = _bTPollRate;

            destination.lsOutBezierCurveObj[deviceIndex].CustomDefinition = LSOutputCurveCustom;
            destination.setLsOutCurveMode(deviceIndex, destination.stickOutputCurveId(LSOutputCurveMode));
            destination.rsOutBezierCurveObj[deviceIndex].CustomDefinition = RSOutputCurveCustom;
            destination.setRsOutCurveMode(deviceIndex, destination.stickOutputCurveId(RSOutputCurveMode));
            destination.squStickInfo[deviceIndex].lsMode = _lsSquareStick;
            destination.squStickInfo[deviceIndex].lsRoundness = SquareStickRoundness;
            destination.squStickInfo[deviceIndex].rsMode = _rsSquareStick;
            destination.squStickInfo[deviceIndex].rsRoundness = SquareRStickRoundness;
            destination.lsAntiSnapbackInfo[deviceIndex].enabled = _lsAntiSnapback;
            destination.rsAntiSnapbackInfo[deviceIndex].enabled = _rsAntiSnapback;
            destination.lsAntiSnapbackInfo[deviceIndex].delta = LSAntiSnapbackDelta;
            destination.rsAntiSnapbackInfo[deviceIndex].delta = RSAntiSnapbackDelta;
            destination.lsAntiSnapbackInfo[deviceIndex].timeout = LSAntiSnapbackTimeout;
            destination.rsAntiSnapbackInfo[deviceIndex].timeout = RSAntiSnapbackTimeout;
            destination.lsOutputSettings[deviceIndex].mode = LSOutputMode;
            destination.rsOutputSettings[deviceIndex].mode = RSOutputMode;

            if (LSOutputSettings != null)
            {
                if (LSOutputSettings.FlickStickSettings != null)
                {
                    destination.lsOutputSettings[deviceIndex].outputSettings.flickSettings.realWorldCalibration = LSOutputSettings.FlickStickSettings.RealWorldCalibration;
                    destination.lsOutputSettings[deviceIndex].outputSettings.flickSettings.flickThreshold = LSOutputSettings.FlickStickSettings.FlickThreshold;
                    destination.lsOutputSettings[deviceIndex].outputSettings.flickSettings.flickTime = LSOutputSettings.FlickStickSettings.FlickTime;
                    destination.lsOutputSettings[deviceIndex].outputSettings.flickSettings.minAngleThreshold = LSOutputSettings.FlickStickSettings.MinAngleThreshold;
                }
            }

            if (RSOutputSettings != null)
            {
                if (RSOutputSettings.FlickStickSettings != null)
                {
                    destination.rsOutputSettings[deviceIndex].outputSettings.flickSettings.realWorldCalibration = RSOutputSettings.FlickStickSettings.RealWorldCalibration;
                    destination.rsOutputSettings[deviceIndex].outputSettings.flickSettings.flickThreshold = RSOutputSettings.FlickStickSettings.FlickThreshold;
                    destination.rsOutputSettings[deviceIndex].outputSettings.flickSettings.flickTime = RSOutputSettings.FlickStickSettings.FlickTime;
                    destination.rsOutputSettings[deviceIndex].outputSettings.flickSettings.minAngleThreshold = RSOutputSettings.FlickStickSettings.MinAngleThreshold;
                }
            }

            if (DualSenseControllerSettings != null)
            {
                if (DualSenseControllerSettings.RumbleSettingsGroup != null)
                {
                    destination.dualSenseRumbleEmulationMode[deviceIndex] = DualSenseControllerSettings.RumbleSettingsGroup.EmulationMode;
                    destination.useGenericRumbleRescaleForDualSenses[deviceIndex] = DualSenseControllerSettings.RumbleSettingsGroup.EnableGenericRumbleRescale;
                    destination.dualSenseHapticPowerLevel[deviceIndex] = DualSenseControllerSettings.RumbleSettingsGroup.HapticPowerLevel;
                }
            }

            destination.l2OutBezierCurveObj[deviceIndex].CustomDefinition = L2OutputCurveCustom;
            destination.setL2OutCurveMode(deviceIndex, destination.stickOutputCurveId(L2OutputCurveMode));
            destination.l2OutputSettings[deviceIndex].twoStageMode = L2TwoStageMode;
            destination.l2OutputSettings[deviceIndex].hipFireMS = L2HipFireTime;
            destination.l2OutputSettings[deviceIndex].triggerEffect = L2TriggerEffect;

            destination.r2OutBezierCurveObj[deviceIndex].CustomDefinition = R2OutputCurveCustom;
            destination.setR2OutCurveMode(deviceIndex, destination.stickOutputCurveId(R2OutputCurveMode));
            destination.r2OutputSettings[deviceIndex].twoStageMode = R2TwoStageMode;
            destination.r2OutputSettings[deviceIndex].hipFireMS = R2HipFireTime;
            destination.r2OutputSettings[deviceIndex].triggerEffect = R2TriggerEffect;

            destination.sxOutBezierCurveObj[deviceIndex].CustomDefinition = SXOutputCurveCustom;
            destination.setSXOutCurveMode(deviceIndex, destination.stickOutputCurveId(SXOutputCurveMode));
            destination.szOutBezierCurveObj[deviceIndex].CustomDefinition = SZOutputCurveCustom;
            destination.setSZOutCurveMode(deviceIndex, destination.stickOutputCurveId(SZOutputCurveMode));
            destination.trackballMode[deviceIndex] = _trackballMode;
            destination.trackballFriction[deviceIndex] = TrackballFriction;
            destination.touchpadRelMouse[deviceIndex].rotation = TouchRelMouseRotation;
            destination.touchpadRelMouse[deviceIndex].minThreshold = TouchRelMouseMinThreshold;

            if (TouchpadAbsMouseSettings != null)
            {
                destination.touchpadAbsMouse[deviceIndex].maxZoneX = TouchpadAbsMouseSettings.MaxZoneX;
                destination.touchpadAbsMouse[deviceIndex].maxZoneY = TouchpadAbsMouseSettings.MaxZoneY;
                destination.touchpadAbsMouse[deviceIndex].snapToCenter = TouchpadAbsMouseSettings.SnapToCenter;
            }

            if (TouchpadMouseStickSettings != null)
            {
                destination.touchMStickInfo[deviceIndex].deadZone = TouchpadMouseStickSettings.DeadZone;
                destination.touchMStickInfo[deviceIndex].maxZone = TouchpadMouseStickSettings.MaxZone;
                destination.touchMStickInfo[deviceIndex].outputStick = TouchpadMouseStickSettings.OutputStick;
                destination.touchMStickInfo[deviceIndex].outputStickDir = TouchpadMouseStickSettings.OutputStickAxes;
                destination.touchMStickInfo[deviceIndex].antiDeadX = TouchpadMouseStickSettings.AntiDeadX;
                destination.touchMStickInfo[deviceIndex].antiDeadY = TouchpadMouseStickSettings.AntiDeadY;
                destination.touchMStickInfo[deviceIndex].inverted = TouchpadMouseStickSettings.Invert;
                destination.touchMStickInfo[deviceIndex].maxOutput = TouchpadMouseStickSettings.MaxOutput;
                destination.touchMStickInfo[deviceIndex].maxOutputEnabled = TouchpadMouseStickSettings.MaxOutputEnabled;
                destination.touchMStickInfo[deviceIndex].vertScale = TouchpadMouseStickSettings.VerticalScale;
                destination.touchMStickInfo[deviceIndex].outputCurve = TouchpadMouseStickSettings.OutputCurve;
                destination.touchMStickInfo[deviceIndex].rotationRad = TouchpadMouseStickSettings.RotationRad;
                if (TouchpadMouseStickSettings.SmoothingSettings != null)
                {
                    TouchpadMouseStickSerializer.SmoothingGroupSerializer tempSmoothSettings =
                        TouchpadMouseStickSettings.SmoothingSettings;

                    destination.touchMStickInfo[deviceIndex].smoothingMethod = tempSmoothSettings.SmoothingMethod;
                    destination.touchMStickInfo[deviceIndex].minCutoff = tempSmoothSettings.SmoothingMinCutoff;
                    destination.touchMStickInfo[deviceIndex].beta = tempSmoothSettings.SmoothingBeta;
                }
            }

            destination.touchpadButtonMode[deviceIndex] = TouchpadButtonMode;

            if (AbsMouseRegionSettings != null)
            {
                destination.buttonAbsMouseInfos[deviceIndex].width = AbsMouseRegionSettings.AbsWidth;
                destination.buttonAbsMouseInfos[deviceIndex].height = AbsMouseRegionSettings.AbsHeight;
                destination.buttonAbsMouseInfos[deviceIndex].xcenter = AbsMouseRegionSettings.AbsXCenter;
                destination.buttonAbsMouseInfos[deviceIndex].ycenter = AbsMouseRegionSettings.AbsYCenter;
                destination.buttonAbsMouseInfos[deviceIndex].antiRadius = AbsMouseRegionSettings.AntiRadius;
                destination.buttonAbsMouseInfos[deviceIndex].snapToCenter = AbsMouseRegionSettings.SnapToCenter;
            };


            destination.outputDevType[deviceIndex] = OutputContDevice;
            if (!string.IsNullOrEmpty(ProfileActions))
            {
                string[] actionNames = ProfileActions.Split('/');
                for (int actIndex = 0, actLen = actionNames.Length; actIndex < actLen; actIndex++)
                {
                    string tempActionName = actionNames[actIndex];
                    if (!destination.profileActions[deviceIndex].Contains(tempActionName))
                    {
                        destination.profileActions[deviceIndex].Add(tempActionName);
                    }
                }
            }

            if (Control != null)
            {
                if (Control.Button != null && Control.Button.CustomMapButtons.Count > 0)
                {
                    foreach(KeyValuePair<DS4Controls, X360Controls> pair in Control.Button.CustomMapButtons)
                    {
                        destination.UpdateDS4CSetting(deviceIndex,
                            pair.Key.ToString(), false, pair.Value, "", DS4KeyType.None, 0);
                    }
                }

                if (Control.Key != null && Control.Key.CustomMapKeys.Count > 0)
                {
                    foreach(KeyValuePair<DS4Controls, ushort> pair in Control.Key.CustomMapKeys)
                    {
                        destination.UpdateDS4CSetting(deviceIndex,
                            pair.Key.ToString(), false, pair.Value, "", DS4KeyType.None, 0);
                    }
                }

                if (Control.Macro != null && Control.Macro.CustomMapMacros.Count > 0)
                {
                    foreach (KeyValuePair<DS4Controls, string> pair in Control.Macro.CustomMapMacros)
                    {
                        string[] skeys;
                        int[] keys;
                        if (!string.IsNullOrEmpty(pair.Value))
                        {
                            skeys = pair.Value.Split('/');
                            keys = new int[skeys.Length];
                        }
                        else
                        {
                            skeys = new string[0];
                            keys = new int[0];
                        }

                        for (int i = 0, keylen = keys.Length; i < keylen; i++)
                            keys[i] = int.Parse(skeys[i]);

                        destination.UpdateDS4CSetting(deviceIndex,
                            pair.Key.ToString(), false, keys, "", DS4KeyType.None, 0);
                    }
                }

                if (Control.Extras != null && Control.Extras.CustomMapExtras.Count > 0)
                {
                    foreach (KeyValuePair<DS4Controls, string> pair in Control.Extras.CustomMapExtras)
                    {
                        destination.UpdateDS4CExtra(deviceIndex,
                            pair.Key.ToString(), false, pair.Value);
                    }
                }

                if (Control.KeyType != null && Control.KeyType.CustomMapKeyTypes.Count > 0)
                {
                    foreach (KeyValuePair<DS4Controls, DS4KeyType> pair in Control.KeyType.CustomMapKeyTypes)
                    {
                        destination.UpdateDS4CKeyType(deviceIndex, pair.Key.ToString(), false, pair.Value);
                    }
                }
            }

            if (ShiftControl != null)
            {
                if (ShiftControl.Button != null && ShiftControl.Button.CustomMapButtons.Count > 0)
                {
                    foreach (KeyValuePair<DS4Controls, X360Controls> pair in ShiftControl.Button.CustomMapButtons)
                    {
                        if (ShiftControl.Button.ShiftTriggers.TryGetValue(pair.Key, out int shiftT) &&
                            shiftT > 0)
                        {
                            destination.UpdateDS4CSetting(deviceIndex,
                                pair.Key.ToString(), true, pair.Value, "", DS4KeyType.None, shiftT);
                        }
                    }
                }

                if (ShiftControl.Key != null && ShiftControl.Key.CustomMapKeys.Count > 0)
                {
                    foreach (KeyValuePair<DS4Controls, ushort> pair in ShiftControl.Key.CustomMapKeys)
                    {
                        if (ShiftControl.Key.ShiftTriggers.TryGetValue(pair.Key, out int shiftT) &&
                            shiftT > 0)
                        {
                            destination.UpdateDS4CSetting(deviceIndex,
                                pair.Key.ToString(), true, pair.Value, "", DS4KeyType.None, shiftT);
                        }
                    }
                }

                if (ShiftControl.Macro != null && ShiftControl.Macro.CustomMapMacros.Count > 0)
                {
                    foreach (KeyValuePair<DS4Controls, string> pair in ShiftControl.Macro.CustomMapMacros)
                    {
                        string[] skeys;
                        int[] keys;
                        if (!string.IsNullOrEmpty(pair.Value))
                        {
                            skeys = pair.Value.Split('/');
                            keys = new int[skeys.Length];
                        }
                        else
                        {
                            skeys = new string[0];
                            keys = new int[0];
                        }

                        for (int i = 0, keylen = keys.Length; i < keylen; i++)
                            keys[i] = int.Parse(skeys[i]);

                        if (ShiftControl.Macro.ShiftTriggers.TryGetValue(pair.Key, out int shiftT) &&
                            shiftT > 0)
                        {
                            destination.UpdateDS4CSetting(deviceIndex,
                                pair.Key.ToString(), true, keys, "", DS4KeyType.None, shiftT);
                        }
                    }
                }

                if (ShiftControl.Extras != null && ShiftControl.Extras.CustomMapExtras.Count > 0)
                {
                    foreach (KeyValuePair<DS4Controls, string> pair in ShiftControl.Extras.CustomMapExtras)
                    {
                        destination.UpdateDS4CExtra(deviceIndex,
                            pair.Key.ToString(), false, pair.Value);
                    }
                }

                if (ShiftControl.KeyType != null && ShiftControl.KeyType.CustomMapKeyTypes.Count > 0)
                {
                    foreach (KeyValuePair<DS4Controls, DS4KeyType> pair in ShiftControl.KeyType.CustomMapKeyTypes)
                    {
                        destination.UpdateDS4CKeyType(deviceIndex, pair.Key.ToString(), true, pair.Value);
                    }
                }
            }
        }

        public void PostProcessXml()
        {
            if (!string.IsNullOrEmpty(ColorString))
            {
                string[] tempColors = ColorString.Split(',');
                if (tempColors.Length == 3)
                {
                    _ledColor = new DS4Color(byte.Parse(tempColors[0]),
                        byte.Parse(tempColors[1]), byte.Parse(tempColors[2]));
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(RedColorString))
                {
                    byte.TryParse(RedColorString, out _ledColor.red);
                }

                if (!string.IsNullOrEmpty(GreenColorString))
                {
                    byte.TryParse(RedColorString, out _ledColor.green);
                }

                if (!string.IsNullOrEmpty(BlueColorString))
                {
                    byte.TryParse(RedColorString, out _ledColor.blue);
                }
            }

            if (!string.IsNullOrEmpty(LowColorString))
            {
                string[] tempColors = LowColorString.Split(',');
                if (tempColors.Length == 3)
                {
                    _lowColor = new DS4Color(byte.Parse(tempColors[0]),
                        byte.Parse(tempColors[1]), byte.Parse(tempColors[2]));
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(LowRedColorString))
                {
                    byte.TryParse(LowRedColorString, out _lowColor.red);
                }

                if (!string.IsNullOrEmpty(LowGreenColorString))
                {
                    byte.TryParse(LowGreenColorString, out _lowColor.green);
                }

                if (!string.IsNullOrEmpty(LowBlueColorString))
                {
                    byte.TryParse(LowBlueColorString, out _lowColor.blue);
                }
            }

            if (!string.IsNullOrEmpty(ChargingColorString))
            {
                string[] tempColors = ChargingColorString.Split(',');
                if (tempColors.Length == 3)
                {
                    _chargingColor = new DS4Color(byte.Parse(tempColors[0]),
                        byte.Parse(tempColors[1]), byte.Parse(tempColors[2]));
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(ChargingRedString))
                {
                    byte.TryParse(ChargingRedString, out _chargingColor.red);
                }

                if (!string.IsNullOrEmpty(ChargingGreenString))
                {
                    byte.TryParse(ChargingGreenString, out _chargingColor.green);
                }

                if (!string.IsNullOrEmpty(ChargingBlueString))
                {
                    byte.TryParse(ChargingBlueString, out _chargingColor.blue);
                }
            }

            if (!string.IsNullOrEmpty(FlashColorString))
            {
                string[] tempColors = FlashColorString.Split(',');
                if (tempColors.Length == 3)
                {
                    _flashColor = new DS4Color(byte.Parse(tempColors[0]),
                        byte.Parse(tempColors[1]), byte.Parse(tempColors[2]));
                }
            }

            //LowColor +Done
            //ChargingColor +Done
            //Sensitivity +Done
            //LaunchProgram +Done
            //StartTouchpadOff +Done
            //UseTPforControls +Done
            //GyroOutputMode +Done
            //GyroControlsSettings/Toggle +Done
            //GyroMouseStickToggle + Done
            //GyroMouseStickSmoothingSettings/SmoothingMethod +Done
            //TouchpadOutputMode +Done
            //TouchDisInvTriggers +Done
            //GyroMouseDeadZone +Done
            //LSOutputCurveMode +Done
            //RSOutputCurveMode +Done
            //L2OutputCurveMode +Done
            //R2OutputCurveMode +Done
            //SXOutputCurveMode +Done
            //SZOutputCurveMode +Done
            //OutputContDevice +Done
        }

        public static XmlAttributeOverrides GetAttributeOverrides()
        {
            XmlAttributeOverrides xmlOverrides = new XmlAttributeOverrides();

            XmlAttributes xmlAttribs = new XmlAttributes();
            xmlAttribs.XmlType = new XmlTypeAttribute("TouchMouseStickInfo.OutputStick");
            xmlOverrides.Add(typeof(TouchMouseStickInfo.OutputStick), xmlAttribs);

            xmlAttribs = new XmlAttributes();
            xmlAttribs.XmlType = new XmlTypeAttribute("TouchMouseStickInfo.OutputStickAxes");
            xmlOverrides.Add(typeof(TouchMouseStickInfo.OutputStickAxes), xmlAttribs);
            return xmlOverrides;
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
        [XmlIgnore]
        public bool Enabled
        {
            get => _enabled;
            set => _enabled = value;
        }
        [XmlElement("Enabled")]
        public string EnabledString
        {
            get => _enabled.ToString();
            set => _enabled = XmlDataUtilities.StrToBool(value);
        }

        private double _multiplier = DeltaAccelSettings.MULTIPLIER_DEFAULT;
        [XmlElement("Multiplier")]
        public double Multiplier
        {
            get => _multiplier;
            set => _multiplier = Math.Clamp(value, 0.0, 10.0);
        }

        private double _maxTravel = DeltaAccelSettings.MAX_TRAVEL_DEFAULT;
        [XmlElement("MaxTravel")]
        public double MaxTravel
        {
            get => _maxTravel;
            set => _maxTravel = Math.Clamp(value, 0.0, 1.0);
        }

        private double _minTravel = DeltaAccelSettings.MIN_TRAVEL_DEFAULT;
        [XmlElement("MinTravel")]
        public double MinTravel
        {
            get => _minTravel;
            set => _minTravel = Math.Clamp(value, 0.0, 1.0);
        }

        private double _easingDuration = DeltaAccelSettings.EASING_DURATION_DEFAULT;
        [XmlElement("EasingDuration")]
        public double EasingDuration
        {
            get => _easingDuration;
            set => _easingDuration = Math.Clamp(value, 0.0, 600.0);
        }

        private double _minFactor = DeltaAccelSettings.MINFACTOR_DEFAULT;
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
        [XmlIgnore]
        public bool SASteeringWheelUseSmoothing
        {
            get => _sASteeringWheelUseSmoothing;
            set => _sASteeringWheelUseSmoothing = value;
        }

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
        [XmlIgnore]
        public bool TriggerCond
        {
            get => _triggerCond;
            set => _triggerCond = value;
        }

        [XmlElement("TriggerCond")]
        public string TriggerCondString
        {
            get => BackingStore.SaTriggerCondString(_triggerCond);
            set => _triggerCond = BackingStore.SaTriggerCondValue(value);
        }

        private bool _triggerTurns;
        [XmlIgnore]
        public bool TriggerTurns
        {
            get => _triggerTurns;
            set => _triggerTurns = value;
        }
        [XmlElement("TriggerTurns")]
        public string TriggerTurnsString
        {
            get => _triggerTurns.ToString();
            set => _triggerTurns = XmlDataUtilities.StrToBool(value);
        }

        private bool _toggle;
        [XmlIgnore]
        public bool Toggle
        {
            get => _toggle;
            set => _toggle = value;
        }

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
        [XmlIgnore]
        public bool UseSmoothing
        {
            get => _useSmoothing;
            set => _useSmoothing = value;
        }

        [XmlElement("UseSmoothing")]
        public string UseSmoothingString
        {
            get => _useSmoothing.ToString();
            set => _useSmoothing = XmlDataUtilities.StrToBool(value);
        }

        protected string _smoothingMethod = GyroMouseStickInfo.DEFAULT_SMOOTH_TECHNIQUE;
        [XmlElement("SmoothingMethod")]
        public string SmoothingMethod
        {
            get => _smoothingMethod;
            set => _smoothingMethod = value;
        }

        protected double _smoothingWeight = GyroMouseStickInfo.SMOOTHING_WEIGHT_DEFAULT;
        [XmlIgnore]
        public double SmoothingWeightRaw
        {
            get => _smoothingWeight;
            set => _smoothingWeight = value;
        }

        [XmlElement("SmoothingWeight")]
        public double SmoothingWeight
        {
            get => _smoothingWeight * 100.0;
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
        [XmlIgnore]
        public bool TriggerCond
        {
            get => _triggerCond;
            set => _triggerCond = value;
        }

        [XmlElement("TriggerCond")]
        public string TriggerCondString
        {
            get => BackingStore.SaTriggerCondString(_triggerCond);
            set => _triggerCond = BackingStore.SaTriggerCondValue(value);
        }

        private bool _triggerTurns;
        [XmlIgnore]
        public bool TriggerTurns
        {
            get => _triggerTurns;
            set => _triggerTurns = value;
        }
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

    public class DualSenseControllerSettings
    {
        public class RumbleSettings
        {
            [XmlElement("EmulationMode")]
            public DualSenseDevice.RumbleEmulationMode EmulationMode
            {
                get; set;
            }

            private bool _enableGenericRumbleRescale;
            [XmlIgnore]
            public bool EnableGenericRumbleRescale
            {
                get => _enableGenericRumbleRescale;
                set => _enableGenericRumbleRescale = value;
            }

            [XmlElement("EnableGenericRumbleRescale")]
            public string EnableGenericRumbleRescaleString
            {
                get => _enableGenericRumbleRescale.ToString();
                set => _enableGenericRumbleRescale = XmlDataUtilities.StrToBool(value);
            }

            [XmlElement("HapticPowerLevel")]
            public byte HapticPowerLevel
            {
                get; set;
            }
        }

        [XmlElement("RumbleSettings")]
        public RumbleSettings RumbleSettingsGroup
        {
            get; set;
        }

        public DualSenseControllerSettings()
        {
            RumbleSettingsGroup = new RumbleSettings();
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
        [XmlIgnore]
        public bool SnapToCenter
        {
            get => _snapToCenter;
            set => _snapToCenter = value;
        }

        [XmlElement("SnapToCenter")]
        public string SnapToCenterString
        {
            get => _snapToCenter.ToString();
            set => _snapToCenter = XmlDataUtilities.StrToBool(value);
        }
    }

    public class TouchpadMouseStickSerializer
    {
        public class SmoothingGroupSerializer
        {
            private TouchMouseStickInfo.SmoothingMethod _smoothMethod =
                TouchMouseStickInfo.SmoothingMethod.None;
            [XmlElement("SmoothingMethod")]
            public TouchMouseStickInfo.SmoothingMethod SmoothingMethod
            {
                get => _smoothMethod;
                set => _smoothMethod = value;
            }

            private double _smoothingMinCutoff =
                TouchMouseStickInfo.DEFAULT_MINCUTOFF;
            [XmlElement("SmoothingMinCutoff")]
            public double SmoothingMinCutoff
            {
                get => _smoothingMinCutoff;
                set => _smoothingMinCutoff = value;
            }

            private double _smoothingBeta =
                TouchMouseStickInfo.DEFAULT_BETA;
            [XmlElement("SmoothingBeta")]
            public double SmoothingBeta
            {
                get => _smoothingBeta;
                set => _smoothingBeta = value;
            }
        }

        private int _deadZone = 0;
        [XmlElement("DeadZone")]
        public int DeadZone
        {
            get => _deadZone;
            set => _deadZone = value;
        }

        private int _maxZone = TouchMouseStickInfo.MAX_ZONE_DEFAULT;
        [XmlElement("MaxZone")]
        public int MaxZone
        {
            get => _maxZone;
            set => _maxZone = value;
        }

        private TouchMouseStickInfo.OutputStick _outputStick =
            TouchMouseStickInfo.DEFAULT_OUTPUT_STICK;
        [XmlElement("OutputStick")]
        public TouchMouseStickInfo.OutputStick OutputStick
        {
            get => _outputStick;
            set => _outputStick = value;
        }

        private TouchMouseStickInfo.OutputStickAxes _outputStickAxes =
            TouchMouseStickInfo.DEFAULT_OUTPUT_STICK_AXES;
        [XmlElement("OutputStickAxes")]
        public TouchMouseStickInfo.OutputStickAxes OutputStickAxes
        {
            get => _outputStickAxes;
            set => _outputStickAxes = value;
        }

        private double _antiDeadZoneX = TouchMouseStickInfo.ANTI_DEADZONE_DEFAULT;
        [XmlElement("AntiDeadX")]
        public double AntiDeadX
        {
            get => _antiDeadZoneX;
            set => _antiDeadZoneX = value;
        }

        private double _antiDeadZoneY = TouchMouseStickInfo.ANTI_DEADZONE_DEFAULT;
        [XmlElement("AntiDeadY")]
        public double AntiDeadY
        {
            get => _antiDeadZoneY;
            set => _antiDeadZoneY = value;
        }

        private uint _invert;
        [XmlElement("Invert")]
        public uint Invert
        {
            get => _invert;
            set => _invert = value;
        }

        private double _maxOutput = TouchMouseStickInfo.MAX_ZONE_DEFAULT;
        [XmlElement("MaxOutput")]
        public double MaxOutput
        {
            get => _maxOutput;
            set => _maxOutput = value;
        }

        private bool _maxOutputEnabled;
        [XmlIgnore]
        public bool MaxOutputEnabled
        {
            get => _maxOutputEnabled;
            set => _maxOutputEnabled = value;
        }

        [XmlElement("MaxOutputEnabled")]
        public string MaxOutputEnabledString
        {
            get => _maxOutputEnabled.ToString();
            set => _maxOutputEnabled = XmlDataUtilities.StrToBool(value);
        }

        private int _verticalScale = 100;
        [XmlElement("VerticalScale")]
        public int VerticalScale
        {
            get => _verticalScale;
            set => _verticalScale = value;
        }

        private StickOutCurve.Curve _outputCurve = TouchMouseStickInfo.OUTPUT_CURVE_DEFAULT;
        [XmlElement("OutputCurve")]
        public StickOutCurve.Curve OutputCurve
        {
            get => _outputCurve;
            set => _outputCurve = value;
        }

        private double _rotationRad = TouchMouseStickInfo.ANG_RAD_DEFAULT;
        [XmlElement("Rotation")]
        public int Rotation
        {
            get => (int)(_rotationRad / 180 * Math.PI);
            set => _rotationRad = value * Math.PI / 180.0;
        }

        [XmlIgnore]
        public double RotationRad
        {
            get => _rotationRad;
            set => _rotationRad = value;
        }

        [XmlElement("SmoothingSettings")]
        public SmoothingGroupSerializer SmoothingSettings
        {
            get; set;
        }
        public bool ShouldSerializeSmoothingSettings()
        {
            return SmoothingSettings != null;
        }
    }

    public class AbsMouseRegionSettingsSerializer
    {
        private double _absWidth = ButtonAbsMouseInfo.WIDTH_DEFAULT;
        [XmlElement("AbsWidth")]
        public double AbsWidth
        {
            get => _absWidth;
            set => _absWidth = Math.Clamp(value, 0.0, 1.0);
        }

        private double _absHeight = ButtonAbsMouseInfo.HEIGHT_DEFAULT;
        [XmlElement("AbsHeight")]
        public double AbsHeight
        {
            get => _absHeight;
            set => _absHeight = Math.Clamp(value, 0.0, 1.0);
        }

        private double _absXCenter = ButtonAbsMouseInfo.XCENTER_DEFAULT;
        [XmlElement("AbsXCenter")]
        public double AbsXCenter
        {
            get => _absXCenter;
            set => _absXCenter = Math.Clamp(value, 0.0, 1.0);
        }

        private double _absYCenter = ButtonAbsMouseInfo.YCENTER_DEFAULT;
        [XmlElement("AbsYCenter")]
        public double AbsYCenter
        {
            get => _absYCenter;
            set => _absYCenter = Math.Clamp(value, 0.0, 1.0);
        }

        private double _antiRadius = ButtonAbsMouseInfo.ANTI_RADIUS_DEFAULT;
        [XmlElement("AntiRadius")]
        public double AntiRadius
        {
            get => _antiRadius;
            set => _antiRadius = Math.Clamp(value, 0.0, 1.0);
        }

        private bool _snapToCenter = ButtonAbsMouseInfo.SNAP_CENTER_DEFAULT;
        [XmlIgnore]
        public bool SnapToCenter
        {
            get => _snapToCenter;
            set => _snapToCenter = value;
        }

        [XmlElement("SnapToCenter")]
        public string SnapToCenterString
        {
            get => _snapToCenter.ToString();
            set => _snapToCenter = XmlDataUtilities.StrToBool(value);
        }
    }

    public class DS4ControlAssignementSerializer
    {
        [XmlElement("Button")]
        public DS4ControlButtonAssignmentSerializer Button
        {
            get; set;
        }
        public bool ShouldSerializeButton()
        {
            return Button != null && Button.CustomMapButtons.Count > 0;
        }

        [XmlElement("Key")]
        public DS4ControlKeyAssignmentSerializer Key
        {
            get; set;
        }
        public bool ShouldSerializeKey()
        {
            return Key != null && Key.CustomMapKeys.Count > 0;
        }

        [XmlElement("Macro")]
        public DS4ControlMacroAssignmentSerializer Macro
        {
            get; set;
        }
        public bool ShouldSerializeMacro()
        {
            return Macro != null && Macro.CustomMapMacros.Count > 0;
        }

        [XmlElement("Extras")]
        public DS4ControlExtrasAssignmentSerializer Extras
        {
            get; set;
        }
        public bool ShouldSerializeExtras()
        {
            return Extras != null && Extras.CustomMapExtras.Count > 0;
        }

        [XmlElement("KeyType")]
        public DS4ControlKeyTypeAssignmentSerializer KeyType
        {
            get; set;
        }
        public bool ShouldSerializeKeyType()
        {
            return KeyType != null && KeyType.CustomMapKeyTypes.Count > 0;
        }

        public DS4ControlAssignementSerializer()
        {
        }
    }

    public class DS4ControlAssignmentSerializerBase
    {
        protected Dictionary<DS4Controls, int> shiftTriggers =
            new Dictionary<DS4Controls, int>();
        public Dictionary<DS4Controls, int> ShiftTriggers => shiftTriggers;

        //protected int _trigger = -1;
        //[XmlAttribute("Trigger")]
        //public int Trigger
        //{
        //    get => _trigger;
        //    set => _trigger = value;
        //}
        //public bool ShouldSerializeTrigger()
        //{
        //    return _trigger != -1;
        //}
    }

    public class DS4ControlButtonAssignmentSerializer : DS4ControlAssignmentSerializerBase, IXmlSerializable
    {
        private Dictionary<DS4Controls, X360Controls> customMapButtons
            = new Dictionary<DS4Controls, X360Controls>();
        [XmlIgnore]
        public Dictionary<DS4Controls, X360Controls> CustomMapButtons => customMapButtons;

        public XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
        {
            XmlDocument tempDoc = new XmlDocument();
            string tempXml = reader.ReadOuterXml();
            XmlReader tempXmlReader = XmlReader.Create(new StringReader(tempXml));
            tempDoc.Load(tempXmlReader);
            XmlNode parentNode = tempDoc.SelectSingleNode("Button");
            if (parentNode != null)
            {
                foreach (XmlNode item in parentNode.ChildNodes)
                {
                    if (Enum.TryParse(item.Name, out DS4Controls currentControl))
                    {
                        if (item.Attributes["Trigger"] != null)
                        {
                            int.TryParse(item.Attributes["Trigger"].Value, out int shiftT);
                            shiftTriggers.TryAdd(currentControl, shiftT);
                        }

                        //UpdateDS4CSetting(device, item.Name, false, getX360ControlsByName(item.InnerText), "", DS4KeyType.None, 0);
                        customMapButtons.Add(Global.getDS4ControlsByName(item.Name),
                            Global.getX360ControlsByName(item.InnerText));
                    }
                }
            }
        }

        public void WriteXml(XmlWriter writer)
        {
            foreach(KeyValuePair<DS4Controls, X360Controls> pair in customMapButtons)
            {
                writer.WriteStartElement(pair.Key.ToString());
                if (shiftTriggers.TryGetValue(pair.Key, out int shiftTrigger) &&
                    shiftTrigger > 0)
                {
                    writer.WriteAttributeString("Trigger", shiftTrigger.ToString());
                }

                writer.WriteValue(Global.getX360ControlString(pair.Value));
                writer.WriteEndElement();
            }
        }
    }

    public class DS4ControlKeyAssignmentSerializer : DS4ControlAssignmentSerializerBase, IXmlSerializable
    {
        private Dictionary<DS4Controls, ushort> customMapKeys
            = new Dictionary<DS4Controls, ushort>();
        [XmlIgnore]
        public Dictionary<DS4Controls, ushort> CustomMapKeys => customMapKeys;

        public XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
        {
            XmlDocument tempDoc = new XmlDocument();
            string tempXml = reader.ReadOuterXml();
            XmlReader tempXmlReader = XmlReader.Create(new StringReader(tempXml));
            tempDoc.Load(tempXmlReader);
            XmlNode parentNode = tempDoc.SelectSingleNode("Key");
            if (parentNode != null)
            {
                foreach (XmlNode item in parentNode.ChildNodes)
                {
                    if (ushort.TryParse(item.InnerText, out ushort wvk) &&
                        Enum.TryParse(item.Name, out DS4Controls currentControl))
                    {
                        if (item.Attributes["Trigger"] != null)
                        {
                            int.TryParse(item.Attributes["Trigger"].Value, out int shiftT);
                            shiftTriggers.TryAdd(currentControl, shiftT);
                        }

                        //UpdateDS4CSetting(device, item.Name, false, wvk, "", DS4KeyType.None, 0);
                        customMapKeys.Add(Global.getDS4ControlsByName(item.Name), wvk);
                    }
                }
            }
        }

        public void WriteXml(XmlWriter writer)
        {
            foreach (KeyValuePair<DS4Controls, UInt16> pair in customMapKeys)
            {
                writer.WriteStartElement(pair.Key.ToString());
                if (shiftTriggers.TryGetValue(pair.Key, out int shiftTrigger) &&
                    shiftTrigger > 0)
                {
                    writer.WriteAttributeString("Trigger", shiftTrigger.ToString());
                }

                writer.WriteValue(pair.Value.ToString());
                writer.WriteEndElement();
            }
        }
    }

    public class DS4ControlMacroAssignmentSerializer : DS4ControlAssignmentSerializerBase, IXmlSerializable
    {
        private Dictionary<DS4Controls, string> customMapMacros
            = new Dictionary<DS4Controls, string>();
        [XmlIgnore]
        public Dictionary<DS4Controls, string> CustomMapMacros => customMapMacros;

        public XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
        {
            XmlDocument tempDoc = new XmlDocument();
            string tempXml = reader.ReadOuterXml();
            XmlReader tempXmlReader = XmlReader.Create(new StringReader(tempXml));
            tempDoc.Load(tempXmlReader);
            XmlNode parentNode = tempDoc.SelectSingleNode("Macro");
            if (parentNode != null)
            {
                foreach (XmlNode item in parentNode.ChildNodes)
                {
                    if (Enum.TryParse(item.Name, out DS4Controls currentControl))
                    {
                        if (item.Attributes["Trigger"] != null)
                        {
                            int.TryParse(item.Attributes["Trigger"].Value, out int shiftT);
                            shiftTriggers.TryAdd(currentControl, shiftT);
                        }

                        customMapMacros.Add(Global.getDS4ControlsByName(item.Name), item.InnerText);
                    }

                    //string[] skeys;
                    //int[] keys;
                    //if (!string.IsNullOrEmpty(item.InnerText))
                    //{
                    //    skeys = item.InnerText.Split('/');
                    //    keys = new int[skeys.Length];
                    //}
                    //else
                    //{
                    //    skeys = new string[0];
                    //    keys = new int[0];
                    //}

                    //for (int i = 0, keylen = keys.Length; i < keylen; i++)
                    //    keys[i] = int.Parse(skeys[i]);

                    //if (Enum.TryParse(item.Name, out DS4Controls currentControl))
                    //{
                    //    UpdateDS4CSetting(device, item.Name, false, keys, "", DS4KeyType.None, 0);
                    //}
                }
            }
        }

        public void WriteXml(XmlWriter writer)
        {
            foreach (KeyValuePair<DS4Controls, string> pair in customMapMacros)
            {
                writer.WriteStartElement(pair.Key.ToString());
                if (shiftTriggers.TryGetValue(pair.Key, out int shiftTrigger) &&
                    shiftTrigger > 0)
                {
                    writer.WriteAttributeString("Trigger", shiftTrigger.ToString());
                }

                writer.WriteValue(pair.Value.ToString());
                writer.WriteEndElement();
            }
        }
    }

    public class DS4ControlKeyTypeAssignmentSerializer : DS4ControlAssignmentSerializerBase, IXmlSerializable
    {
        private Dictionary<DS4Controls, DS4KeyType> customMapKeyTypes
            = new Dictionary<DS4Controls, DS4KeyType>();
        [XmlIgnore]
        public Dictionary<DS4Controls, DS4KeyType> CustomMapKeyTypes => customMapKeyTypes;

        public XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
        {
            XmlDocument tempDoc = new XmlDocument();
            string tempXml = reader.ReadOuterXml();
            XmlReader tempXmlReader = XmlReader.Create(new StringReader(tempXml));
            tempDoc.Load(tempXmlReader);
            XmlNode parentNode = tempDoc.SelectSingleNode("KeyType");
            if (parentNode != null)
            {
                foreach (XmlNode item in parentNode.ChildNodes)
                {
                    if (item != null)
                    {
                        DS4KeyType keyType = DS4KeyType.None;
                        string[] ds4KeyNames = Enum.GetNames(typeof(DS4KeyType));
                        foreach(string keyName in ds4KeyNames)
                        {
                            if (item.InnerText.Contains(keyName) &&
                                Enum.TryParse(keyName, out DS4KeyType tempKey))
                            {
                                keyType |= tempKey;
                            }
                        }

                        if (keyType != DS4KeyType.None &&
                            Enum.TryParse(item.Name, out DS4Controls currentControl))
                        {

                            if (item.Attributes["Trigger"] != null)
                            {
                                int.TryParse(item.Attributes["Trigger"].Value, out int shiftT);
                                shiftTriggers.TryAdd(currentControl, shiftT);
                            }

                            //UpdateDS4CKeyType(device, item.Name, false, keyType);
                            customMapKeyTypes.Add(Global.getDS4ControlsByName(item.Name), keyType);
                        }
                    }
                }
            }
        }

        public void WriteXml(XmlWriter writer)
        {
            foreach (KeyValuePair<DS4Controls, DS4KeyType> pair in customMapKeyTypes)
            {
                string tempKey = string.Empty;
                DS4KeyType[] tempArray = (DS4KeyType[])Enum.GetValues(typeof(DS4KeyType));
                for (int i = 0; i < tempArray.Length; i++)
                {
                    DS4KeyType testFlag = tempArray[i];
                    if (testFlag != DS4KeyType.None &&
                        pair.Value.HasFlag(testFlag))
                    {
                        tempKey += testFlag;
                    }
                }

                if (!string.IsNullOrEmpty(tempKey))
                {
                    writer.WriteStartElement(pair.Key.ToString());
                    if (shiftTriggers.TryGetValue(pair.Key, out int shiftTrigger) &&
                        shiftTrigger > 0)
                    {
                        writer.WriteAttributeString("Trigger", shiftTrigger.ToString());
                    }

                    writer.WriteValue(tempKey);
                    writer.WriteEndElement();
                }
            }
        }
    }

    public class DS4ControlExtrasAssignmentSerializer : DS4ControlAssignmentSerializerBase, IXmlSerializable
    {
        private Dictionary<DS4Controls, string> customMapExtras
            = new Dictionary<DS4Controls, string>();
        [XmlIgnore]
        public Dictionary<DS4Controls, string> CustomMapExtras => customMapExtras;

        public XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
        {
            XmlDocument tempDoc = new XmlDocument();
            string tempXml = reader.ReadOuterXml();
            XmlReader tempXmlReader = XmlReader.Create(new StringReader(tempXml));
            tempDoc.Load(tempXmlReader);
            XmlNode parentNode = tempDoc.SelectSingleNode("Extras");
            if (parentNode != null)
            {
                foreach (XmlNode item in parentNode.ChildNodes)
                {
                    if (item.InnerText != string.Empty &&
                        Enum.TryParse(item.Name, out DS4Controls currentControl))
                    {
                        if (item.Attributes["Trigger"] != null)
                        {
                            int.TryParse(item.Attributes["Trigger"].Value, out int shiftT);
                            shiftTriggers.TryAdd(currentControl, shiftT);
                        }

                        //UpdateDS4CExtra(device, item.Name, false, item.InnerText);
                        customMapExtras.Add(Global.getDS4ControlsByName(item.Name), item.InnerText);
                    }
                }
            }
        }

        public void WriteXml(XmlWriter writer)
        {
            foreach (KeyValuePair<DS4Controls, string> pair in customMapExtras)
            {
                writer.WriteStartElement(pair.Key.ToString());
                if (shiftTriggers.TryGetValue(pair.Key, out int shiftTrigger) &&
                    shiftTrigger > 0)
                {
                    writer.WriteAttributeString("Trigger", shiftTrigger.ToString());
                }

                writer.WriteValue(pair.Value.ToString());
                writer.WriteEndElement();
            }
        }
    }
}
