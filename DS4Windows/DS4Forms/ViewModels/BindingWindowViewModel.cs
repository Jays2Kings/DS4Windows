using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using DS4Windows;

namespace DS4WinWPF.DS4Forms.ViewModels
{
    public class BindingWindowViewModel
    {
        private int deviceNum;
        private bool use360Mode;
        private DS4ControlSettings settings;
        private OutBinding currentOutBind;
        private OutBinding shiftOutBind;
        private OutBinding actionBinding;
        private bool showShift;
        private bool rumbleActive;

        public bool Using360Mode
        {
            get => use360Mode;
        }
        public int DeviceNum { get => deviceNum; }
        public OutBinding CurrentOutBind { get => currentOutBind; }
        public OutBinding ShiftOutBind { get => shiftOutBind; }
        public OutBinding ActionBinding
        {
            get => actionBinding;
            set
            {
                actionBinding = value;
            }
        }

        public bool ShowShift { get => showShift; set => showShift = value; }
        public bool RumbleActive { get => rumbleActive; set => rumbleActive = value; }
        public DS4ControlSettings Settings { get => settings; }

        public BindingWindowViewModel(int deviceNum, DS4ControlSettings settings)
        {
            this.deviceNum = deviceNum;
            use360Mode = Global.outDevTypeTemp[deviceNum] == OutContType.X360;
            this.settings = settings;
            currentOutBind = new OutBinding();
            shiftOutBind = new OutBinding();
            shiftOutBind.shiftBind = true;
            PopulateCurrentBinds();
        }

        public void PopulateCurrentBinds()
        {
            DS4ControlSettings setting = settings;
            bool sc = setting.keyType.HasFlag(DS4KeyType.ScanCode);
            bool toggle = setting.keyType.HasFlag(DS4KeyType.Toggle);
            currentOutBind.input = setting.control;
            shiftOutBind.input = setting.control;
            if (setting.actionType != DS4ControlSettings.ActionType.Default)
            {
                switch(setting.actionType)
                {
                    case DS4ControlSettings.ActionType.Button:
                        currentOutBind.outputType = OutBinding.OutType.Button;
                        currentOutBind.control = (X360Controls)setting.action.actionBtn;
                        break;
                    case DS4ControlSettings.ActionType.Default:
                        currentOutBind.outputType = OutBinding.OutType.Default;
                        break;
                    case DS4ControlSettings.ActionType.Key:
                        currentOutBind.outputType = OutBinding.OutType.Key;
                        currentOutBind.outkey = setting.action.actionKey;
                        currentOutBind.hasScanCode = sc;
                        currentOutBind.toggle = toggle;
                        break;
                    case DS4ControlSettings.ActionType.Macro:
                        currentOutBind.outputType = OutBinding.OutType.Macro;
                        currentOutBind.macro = (int[])setting.action.actionMacro;
                        currentOutBind.macroType = settings.keyType;
                        currentOutBind.hasScanCode = sc;
                        break;
                }
            }
            else
            {
                currentOutBind.outputType = OutBinding.OutType.Default;
            }

            if (!string.IsNullOrEmpty(setting.extras))
            {
                currentOutBind.ParseExtras(setting.extras);
            }

            if (setting.shiftActionType != DS4ControlSettings.ActionType.Default)
            {
                sc = setting.shiftKeyType.HasFlag(DS4KeyType.ScanCode);
                toggle = setting.shiftKeyType.HasFlag(DS4KeyType.Toggle);
                shiftOutBind.shiftTrigger = setting.shiftTrigger;
                switch (setting.shiftActionType)
                {
                    case DS4ControlSettings.ActionType.Button:
                        shiftOutBind.outputType = OutBinding.OutType.Button;
                        shiftOutBind.control = (X360Controls)setting.shiftAction.actionBtn;
                        break;
                    case DS4ControlSettings.ActionType.Default:
                        shiftOutBind.outputType = OutBinding.OutType.Default;
                        break;
                    case DS4ControlSettings.ActionType.Key:
                        shiftOutBind.outputType = OutBinding.OutType.Key;
                        shiftOutBind.outkey = setting.shiftAction.actionKey;
                        shiftOutBind.hasScanCode = sc;
                        shiftOutBind.toggle = toggle;
                        break;
                    case DS4ControlSettings.ActionType.Macro:
                        shiftOutBind.outputType = OutBinding.OutType.Macro;
                        shiftOutBind.macro = (int[])setting.shiftAction.actionMacro;
                        shiftOutBind.macroType = setting.shiftKeyType;
                        shiftOutBind.hasScanCode = sc;
                        break;
                }
            }

            if (!string.IsNullOrEmpty(setting.shiftExtras))
            {
                shiftOutBind.ParseExtras(setting.shiftExtras);
            }
        }

        public void WriteBinds()
        {
            currentOutBind.WriteBind(settings);
            shiftOutBind.WriteBind(settings);
        }

        public void StartForcedColor(Color color)
        {
            if (deviceNum < ControlService.CURRENT_DS4_CONTROLLER_LIMIT)
            {
                DS4Color dcolor = new DS4Color() { red = color.R, green = color.G, blue = color.B };
                DS4LightBar.forcedColor[deviceNum] = dcolor;
                DS4LightBar.forcedFlash[deviceNum] = 0;
                DS4LightBar.forcelight[deviceNum] = true;
            }
        }

        public void EndForcedColor()
        {
            if (deviceNum < ControlService.CURRENT_DS4_CONTROLLER_LIMIT)
            {
                DS4LightBar.forcedColor[deviceNum] = new DS4Color(0, 0, 0);
                DS4LightBar.forcedFlash[deviceNum] = 0;
                DS4LightBar.forcelight[deviceNum] = false;
            }
        }

        public void UpdateForcedColor(Color color)
        {
            if (deviceNum < ControlService.CURRENT_DS4_CONTROLLER_LIMIT)
            {
                DS4Color dcolor = new DS4Color() { red = color.R, green = color.G, blue = color.B };
                DS4LightBar.forcedColor[deviceNum] = dcolor;
                DS4LightBar.forcedFlash[deviceNum] = 0;
                DS4LightBar.forcelight[deviceNum] = true;
            }
        }
    }

    public class BindAssociation
    {
        public enum OutType : uint
        {
            Default,
            Key,
            Button,
            Macro
        }

        public OutType outputType;
        public X360Controls control;
        public int outkey;

        public bool IsMouse()
        {
            return outputType == OutType.Button && (control >= X360Controls.LeftMouse && control < X360Controls.Unbound);
        }

        public static bool IsMouseRange(X360Controls control)
        {
            return control >= X360Controls.LeftMouse && control < X360Controls.Unbound;
        }
    }

    public class OutBinding
    {
        public enum OutType : uint
        {
            Default,
            Key,
            Button,
            Macro
        }

        public DS4Controls input;
        public bool toggle;
        public bool hasScanCode;
        public OutType outputType;
        public int outkey;
        public int[] macro;
        public DS4KeyType macroType;
        public X360Controls control;
        public bool shiftBind;
        public int shiftTrigger;
        private int heavyRumble = 0;
        private int lightRumble = 0;
        private int flashRate;
        private int mouseSens = 25;
        private DS4Color extrasColor = new DS4Color(255,255,255);

        public bool HasScanCode { get => hasScanCode; set => hasScanCode = value; }
        public bool Toggle { get => toggle; set => toggle = value; }
        public int ShiftTrigger { get => shiftTrigger; set => shiftTrigger = value; }
        public int HeavyRumble { get => heavyRumble; set => heavyRumble = value; }
        public int LightRumble { get => lightRumble; set => lightRumble = value; }
        public int FlashRate
        {
            get => flashRate;
            set
            {
                flashRate = value;
                FlashRateChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler FlashRateChanged;

        public int MouseSens
        {
            get => mouseSens;
            set
            {
                mouseSens = value;
                MouseSensChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler MouseSensChanged;

        private bool useMouseSens;
        public bool UseMouseSens
        {
            get
            {
                return useMouseSens;
            }
            set
            {
                useMouseSens = value;
                UseMouseSensChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler UseMouseSensChanged;

        private bool useExtrasColor;
        public bool UseExtrasColor {
            get
            {
                return useExtrasColor;
            }
            set
            {
                useExtrasColor = value;
                UseExtrasColorChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler UseExtrasColorChanged;

        public int ExtrasColorR
        {
            get => extrasColor.red;
            set
            {
                extrasColor.red = (byte)value;
                ExtrasColorRChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler ExtrasColorRChanged;

        public string ExtrasColorRString
        {
            get
            {
                string temp = $"#{extrasColor.red.ToString("X2")}FF0000";
                return temp;
            }
        }
        public event EventHandler ExtrasColorRStringChanged;
        public int ExtrasColorG
        {
            get => extrasColor.green;
            set
            {
                extrasColor.green = (byte)value;
                ExtrasColorGChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler ExtrasColorGChanged;

        public string ExtrasColorGString
        {
            get
            {
                string temp = $"#{ extrasColor.green.ToString("X2")}00FF00";
                return temp;
            }
        }
        public event EventHandler ExtrasColorGStringChanged;

        public int ExtrasColorB
        {
            get => extrasColor.blue;
            set
            {
                extrasColor.blue = (byte)value;
                ExtrasColorBChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler ExtrasColorBChanged;

        public string ExtrasColorBString
        {
            get
            {
                string temp = $"#{extrasColor.blue.ToString("X2")}0000FF";
                return temp;
            }
        }
        public event EventHandler ExtrasColorBStringChanged;

        public string ExtrasColorString
        {
            get => $"#FF{extrasColor.red.ToString("X2")}{extrasColor.green.ToString("X2")}{extrasColor.blue.ToString("X2")}";
        }
        public event EventHandler ExtrasColorStringChanged;

        public Color ExtrasColorMedia
        {
            get
            {
                return new Color()
                {
                    A = 255,
                    R = extrasColor.red,
                    B = extrasColor.blue,
                    G = extrasColor.green
                };
            }
        }

        private int shiftTriggerIndex;
        public int ShiftTriggerIndex { get => shiftTriggerIndex; set => shiftTriggerIndex = value; }

        public string DefaultColor
        {
            get
            {
                string color = string.Empty;
                if (outputType == OutType.Default)
                {
                    color =  Colors.LimeGreen.ToString();
                }
                /*else
                {
                    color = SystemColors.ControlBrush.Color.ToString();
                }
                */

                return color;
            }
        }

        public string UnboundColor
        {
            get
            {
                string color = string.Empty;
                if (outputType == OutType.Button && control == X360Controls.Unbound)
                {
                    color = Colors.LimeGreen.ToString();
                }
                /*else
                {
                    color = SystemColors.ControlBrush.Color.ToString();
                }
                */

                return color;
            }
        }

        public string DefaultBtnString
        {
            get
            {
                string result = "Default";
                if (shiftBind)
                {
                    result = Properties.Resources.FallBack;
                }

                return result;
            }
        }

        public Visibility MacroLbVisible
        {
            get
            {
                return outputType == OutType.Macro ? Visibility.Visible : Visibility.Hidden;
            }
        }

        public OutBinding()
        {
            ExtrasColorRChanged += OutBinding_ExtrasColorRChanged;
            ExtrasColorGChanged += OutBinding_ExtrasColorGChanged;
            ExtrasColorBChanged += OutBinding_ExtrasColorBChanged;
            UseExtrasColorChanged += OutBinding_UseExtrasColorChanged;
        }

        private void OutBinding_ExtrasColorBChanged(object sender, EventArgs e)
        {
            ExtrasColorStringChanged?.Invoke(this, EventArgs.Empty);
            ExtrasColorBStringChanged?.Invoke(this, EventArgs.Empty);
        }

        private void OutBinding_ExtrasColorGChanged(object sender, EventArgs e)
        {
            ExtrasColorStringChanged?.Invoke(this, EventArgs.Empty);
            ExtrasColorGStringChanged?.Invoke(this, EventArgs.Empty);
        }

        private void OutBinding_ExtrasColorRChanged(object sender, EventArgs e)
        {
            ExtrasColorStringChanged?.Invoke(this, EventArgs.Empty);
            ExtrasColorRStringChanged?.Invoke(this, EventArgs.Empty);
        }

        private void OutBinding_UseExtrasColorChanged(object sender, EventArgs e)
        {
            if (!useExtrasColor)
            {
                ExtrasColorR = 255;
                ExtrasColorG = 255;
                ExtrasColorB = 255;
            }
        }

        public bool IsShift()
        {
            return shiftBind;
        }

        public bool IsMouse()
        {
            return outputType == OutType.Button && (control >= X360Controls.LeftMouse && control < X360Controls.Unbound);
        }

        public static bool IsMouseRange(X360Controls control)
        {
            return control >= X360Controls.LeftMouse && control < X360Controls.Unbound;
        }

        public void ParseExtras(string extras)
        {
            string[] temp = extras.Split(',');
            if (temp.Length == 9)
            {
                int.TryParse(temp[0], out heavyRumble);
                int.TryParse(temp[1], out lightRumble);
                int.TryParse(temp[2], out int useColor);
                if (useColor == 1)
                {
                    useExtrasColor = true;
                    byte.TryParse(temp[3], out extrasColor.red);
                    byte.TryParse(temp[4], out extrasColor.green);
                    byte.TryParse(temp[5], out extrasColor.blue);
                    int.TryParse(temp[6], out flashRate);
                }
                else
                {
                    useExtrasColor = false;
                    extrasColor.red = extrasColor.green = extrasColor.blue = 255;
                    flashRate = 0;
                }

                int.TryParse(temp[7], out int useM);
                if (useM == 1)
                {
                    useMouseSens = true;
                    int.TryParse(temp[8], out mouseSens);
                }
                else
                {
                    useMouseSens = false;
                    mouseSens = 25;
                }
            }
        }

        public string CompileExtras()
        {
            string result = $"{heavyRumble},{lightRumble},";
            if (useExtrasColor)
            {
                result += $"1,{extrasColor.red},{extrasColor.green},{extrasColor.blue},{flashRate},";
            }
            else
            {
                result += "0,0,0,0,0,";
            }

            if (useMouseSens)
            {
                result += $"1,{mouseSens}";
            }
            else
            {
                result += "0,0";
            }

            return result;
        }

        public bool IsUsingExtras()
        {
            bool result = false;
            result = result || (heavyRumble != 0);
            result = result || (lightRumble != 0);
            result = result || useExtrasColor;
            result = result ||
                (extrasColor.red != 255 && extrasColor.green != 255 &&
                extrasColor.blue != 255);

            result = result || (flashRate != 0);
            result = result || useMouseSens;
            result = result || (mouseSens != 25);
            return result;
        }

        public void WriteBind(DS4ControlSettings settings)
        {
            if (!shiftBind)
            {
                settings.keyType = DS4KeyType.None;

                if (outputType == OutType.Default)
                {
                    settings.action.actionKey = 0;
                    settings.actionType = DS4ControlSettings.ActionType.Default;
                }
                else if (outputType == OutType.Button)
                {
                    settings.action.actionBtn = control;
                    settings.actionType = DS4ControlSettings.ActionType.Button;
                    if (control == X360Controls.Unbound)
                    {
                        settings.keyType |= DS4KeyType.Unbound;
                    }
                }
                else if (outputType == OutType.Key)
                {
                    settings.action.actionKey = outkey;
                    settings.actionType = DS4ControlSettings.ActionType.Key;
                    if (hasScanCode)
                    {
                        settings.keyType |= DS4KeyType.ScanCode;
                    }

                    if (toggle)
                    {
                        settings.keyType |= DS4KeyType.Toggle;
                    }
                }
                else if (outputType == OutType.Macro)
                {
                    settings.action.actionMacro = macro;
                    settings.actionType = DS4ControlSettings.ActionType.Macro;
                    if (macroType.HasFlag(DS4KeyType.HoldMacro))
                    {
                        settings.keyType |= DS4KeyType.HoldMacro;
                    }
                    else
                    {
                        settings.keyType |= DS4KeyType.Macro;
                    }

                    if (hasScanCode)
                    {
                        settings.keyType |= DS4KeyType.ScanCode;
                    }
                }

                if (IsUsingExtras())
                {
                    settings.extras = CompileExtras();
                }
                else
                {
                    settings.extras = string.Empty;
                }
            }
            else
            {
                settings.shiftKeyType = DS4KeyType.None;
                settings.shiftTrigger = shiftTrigger;

                if (outputType == OutType.Default || shiftTrigger == 0)
                {
                    settings.shiftAction.actionKey = 0;
                    settings.shiftActionType = DS4ControlSettings.ActionType.Default;
                }
                else if (outputType == OutType.Button)
                {
                    settings.shiftAction.actionBtn = control;
                    settings.shiftActionType = DS4ControlSettings.ActionType.Button;
                    if (control == X360Controls.Unbound)
                    {
                        settings.shiftKeyType |= DS4KeyType.Unbound;
                    }
                }
                else if (outputType == OutType.Key)
                {
                    settings.shiftAction.actionKey = outkey;
                    settings.shiftActionType = DS4ControlSettings.ActionType.Key;
                    if (hasScanCode)
                    {
                        settings.shiftKeyType |= DS4KeyType.ScanCode;
                    }

                    if (toggle)
                    {
                        settings.shiftKeyType |= DS4KeyType.Toggle;
                    }
                }
                else if (outputType == OutType.Macro)
                {
                    settings.shiftAction.actionMacro = macro;
                    settings.shiftActionType = DS4ControlSettings.ActionType.Macro;

                    if (macroType.HasFlag(DS4KeyType.HoldMacro))
                    {
                        settings.shiftKeyType |= DS4KeyType.HoldMacro;
                    }
                    else
                    {
                        settings.shiftKeyType |= DS4KeyType.Macro;
                    }

                    if (hasScanCode)
                    {
                        settings.shiftKeyType |= DS4KeyType.ScanCode;
                    }
                }

                if (IsUsingExtras())
                {
                    settings.shiftExtras = CompileExtras();
                }
                else
                {
                    settings.shiftExtras = string.Empty;
                }
            }
        }

        public void UpdateExtrasColor(Color color)
        {
            ExtrasColorR = color.R;
            ExtrasColorG = color.G;
            ExtrasColorB = color.B;
        }
    }
}
