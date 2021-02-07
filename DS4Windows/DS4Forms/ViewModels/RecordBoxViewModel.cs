using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using DS4Windows;

namespace DS4WinWPF.DS4Forms.ViewModels
{
    public class RecordBoxViewModel
    {
        private Stopwatch sw = new Stopwatch();
        private int deviceNum;
        public int DeviceNum { get => deviceNum; }

        private DS4ControlSettings settings;
        public DS4ControlSettings Settings { get => settings; }

        private bool shift;
        public bool Shift { get => shift; }

        private bool recordDelays;
        public bool RecordDelays { get => recordDelays; set => recordDelays = value; }

        private int macroModeIndex;
        public int MacroModeIndex { get => macroModeIndex; set => macroModeIndex = value; }

        private bool recording;
        public bool Recording { get => recording; set => recording = value; }

        private bool toggleLightbar;
        public bool ToggleLightbar { get => toggleLightbar; set => toggleLightbar = value; }

        private bool toggleRummble;
        public bool ToggleRumble { get => toggleRummble; set => toggleRummble = value; }

        private bool toggle4thMouse;
        private bool toggle5thMouse;
        private int appendIndex = -1;

        private object _colLockobj = new object();
        private ObservableCollection<MacroStepItem> macroSteps =
            new ObservableCollection<MacroStepItem>();
        public ObservableCollection<MacroStepItem> MacroSteps { get => macroSteps; }
        
        private int macroStepIndex;
        public int MacroStepIndex
        {
            get => macroStepIndex;
            set
            {
                if (macroStepIndex == value) return;
                macroStepIndex = value;
                MacroStepIndexChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler MacroStepIndexChanged;
        public Stopwatch Sw { get => sw; }
        public bool Toggle4thMouse { get => toggle4thMouse; set => toggle4thMouse = value; }
        public bool Toggle5thMouse { get => toggle5thMouse; set => toggle5thMouse = value; }
        public int AppendIndex { get => appendIndex; set => appendIndex = value; }
        public int EditMacroIndex { get => editMacroIndex; set => editMacroIndex = value; }
        public Dictionary<int, bool> KeysdownMap { get => keysdownMap; }
        public bool UseScanCode { get => useScanCode; set => useScanCode = value; }
        public static HashSet<int> KeydownOverrides { get => keydownOverrides; }

        private int editMacroIndex = -1;
        private Dictionary<int, bool> keysdownMap = new Dictionary<int, bool>();
        private static HashSet<int> keydownOverrides;

        private bool useScanCode;


        public RecordBoxViewModel(int deviceNum, DS4ControlSettings controlSettings, bool shift)
        {
            if (keydownOverrides == null)
            {
                CreateKeyDownOverrides();
            }

            this.deviceNum = deviceNum;
            settings = controlSettings;
            this.shift = shift;
            if (!shift && settings.keyType.HasFlag(DS4KeyType.HoldMacro))
            {
                macroModeIndex = 1;
            }
            else if (shift && settings.shiftKeyType.HasFlag(DS4KeyType.HoldMacro))
            {
                macroModeIndex = 1;
            }

            if (!shift && settings.keyType.HasFlag(DS4KeyType.ScanCode))
            {
                useScanCode = true;
            }
            else if (shift && settings.shiftKeyType.HasFlag(DS4KeyType.ScanCode))
            {
                useScanCode = true;
            }

            if (!shift && settings.actionType == DS4ControlSettings.ActionType.Macro)
            {
                LoadMacro();
            }
            else if (shift && settings.shiftActionType == DS4ControlSettings.ActionType.Macro)
            {
                LoadMacro();
            }

            BindingOperations.EnableCollectionSynchronization(macroSteps, _colLockobj);
            
            // By default RECORD button appends new steps. User must select (click) an existing step to insert new steps in front of the selected step
            this.MacroStepIndex = -1;

            MacroStepItem.CacheImgLocations();
        }

        private void CreateKeyDownOverrides()
        {
            keydownOverrides = new HashSet<int>()
            {
                44,
            };
        }

        public void LoadMacro()
        {
            int[] macro;
            if (!shift)
            {
                macro = (int[])settings.action.actionMacro;
            }
            else
            {
                macro = (int[])settings.shiftAction.actionMacro;
            }

            MacroParser macroParser = new MacroParser(macro);
            macroParser.LoadMacro();
            foreach(MacroStep step in macroParser.MacroSteps)
            {
                MacroStepItem item = new MacroStepItem(step);
                macroSteps.Add(item);
            }
        }

        public void ExportMacro()
        {
            int[] outmac = new int[macroSteps.Count];
            int index = 0;
            foreach(MacroStepItem step in macroSteps)
            {
                outmac[index] = step.Step.Value;
                index++;
            }

            if (!shift)
            {
                settings.action.actionMacro = outmac;
                settings.actionType = DS4ControlSettings.ActionType.Macro;
                settings.keyType = DS4KeyType.Macro;
                if (macroModeIndex == 1)
                {
                    settings.keyType |= DS4KeyType.HoldMacro;
                }
                if (useScanCode)
                {
                    settings.keyType |= DS4KeyType.ScanCode;
                }
            }
            else
            {
                settings.shiftAction.actionMacro = outmac;
                settings.shiftActionType = DS4ControlSettings.ActionType.Macro;
                settings.shiftKeyType = DS4KeyType.Macro;
                if (macroModeIndex == 1)
                {
                    settings.shiftKeyType |= DS4KeyType.HoldMacro;
                }
                if (useScanCode)
                {
                    settings.shiftKeyType |= DS4KeyType.ScanCode;
                }
            }
        }

        public void WriteCycleProgramsPreset()
        {
            MacroStep step = new MacroStep(18, KeyInterop.KeyFromVirtualKey(18).ToString(),
                MacroStep.StepType.ActDown, MacroStep.StepOutput.Key);
            macroSteps.Add(new MacroStepItem(step));

            step = new MacroStep(9, KeyInterop.KeyFromVirtualKey(9).ToString(),
                MacroStep.StepType.ActDown, MacroStep.StepOutput.Key);
            macroSteps.Add(new MacroStepItem(step));

            step = new MacroStep(9, KeyInterop.KeyFromVirtualKey(9).ToString(),
                MacroStep.StepType.ActUp, MacroStep.StepOutput.Key);
            macroSteps.Add(new MacroStepItem(step));

            step = new MacroStep(18, KeyInterop.KeyFromVirtualKey(18).ToString(),
                MacroStep.StepType.ActUp, MacroStep.StepOutput.Key);
            macroSteps.Add(new MacroStepItem(step));

            step = new MacroStep(1300, $"Wait 1000ms",
                MacroStep.StepType.Wait, MacroStep.StepOutput.None);
            macroSteps.Add(new MacroStepItem(step));
        }

        public void LoadPresetFromFile(string filepath)
        {
            string[] macs = File.ReadAllText(filepath).Split('/');
            List<int> tmpmacro = new List<int>();
            int temp;
            foreach (string s in macs)
            {
                if (int.TryParse(s, out temp))
                    tmpmacro.Add(temp);
            }

            MacroParser macroParser = new MacroParser(tmpmacro.ToArray());
            macroParser.LoadMacro();
            foreach (MacroStep step in macroParser.MacroSteps)
            {
                MacroStepItem item = new MacroStepItem(step);
                macroSteps.Add(item);
            }
        }

        public void SavePreset(string filepath)
        {
            int[] outmac = new int[macroSteps.Count];
            int index = 0;
            foreach (MacroStepItem step in macroSteps)
            {
                outmac[index] = step.Step.Value;
                index++;
            }

            string macro = string.Join("/", outmac);
            StreamWriter sw = new StreamWriter(filepath);
            sw.Write(macro);
            sw.Close();
        }

        public void AddMacroStep(MacroStep step, bool ignoreDelay = false)
        {
            if (recordDelays && macroSteps.Count > 0 && !ignoreDelay)
            {
                int elapsed = (int)sw.ElapsedMilliseconds + 300;
                MacroStep waitstep = new MacroStep(elapsed, $"Wait {elapsed - 300}ms",
                    MacroStep.StepType.Wait, MacroStep.StepOutput.None);
                MacroStepItem waititem = new MacroStepItem(waitstep);
                if (appendIndex == -1)
                {
                    macroSteps.Add(waititem);
                }
                else
                {
                    macroSteps.Insert(appendIndex, waititem);
                    appendIndex++;
                }
            }

            sw.Restart();
            MacroStepItem item = new MacroStepItem(step);
            if (appendIndex == -1)
            {
                macroSteps.Add(item);
            }
            else
            {
                macroSteps.Insert(appendIndex, item);
                appendIndex++;
            }
        }

        public void InsertMacroStep(int index, MacroStep step)
        {
            MacroStepItem item = new MacroStepItem(step);
            macroSteps.Insert(index, item);
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

        public void ProcessDS4Tick()
        {
            if (Program.rootHub.DS4Controllers[0] != null)
            {
                DS4Device dev = Program.rootHub.DS4Controllers[0];
                DS4State cState = dev.getCurrentStateRef();
                for (DS4Controls dc = DS4Controls.LXNeg; dc < DS4Controls.GyroXPos; dc++)
                {
                    // Ignore Touch controls
                    if (dc >= DS4Controls.TouchLeft && dc <= DS4Controls.TouchRight)
                    {
                        continue;
                    }

                    int macroValue = Global.macroDS4Values[dc];
                    keysdownMap.TryGetValue(macroValue, out bool isdown);
                    if (!isdown && Mapping.getBoolMapping(0, dc, cState, null, null))
                    {
                        MacroStep step = new MacroStep(macroValue, MacroParser.macroInputNames[macroValue],
                                MacroStep.StepType.ActDown, MacroStep.StepOutput.Button);
                        AddMacroStep(step);
                        keysdownMap.Add(macroValue, true);
                    }
                    else if (isdown && !Mapping.getBoolMapping(0, dc, cState, null, null))
                    {
                        MacroStep step = new MacroStep(macroValue, MacroParser.macroInputNames[macroValue],
                                MacroStep.StepType.ActUp, MacroStep.StepOutput.Button);
                        AddMacroStep(step);
                        keysdownMap.Remove(macroValue);
                    }
                }
            }
        }
    }

    public class MacroStepItem
    {
        private static string[] imageSources = new string[]
        {
            $"/DS4Windows;component/Resources/{(string)App.Current.FindResource("KeyDownImg")}",
            $"/DS4Windows;component/Resources/{(string)App.Current.FindResource("KeyUpImg")}",
            $"/DS4Windows;component/Resources/{(string)App.Current.FindResource("ClockImg")}",
        };

        public static void CacheImgLocations()
        {
            imageSources = new string[]
            {
                $"/DS4Windows;component/Resources/{(string)App.Current.FindResource("KeyDownImg")}",
                $"/DS4Windows;component/Resources/{(string)App.Current.FindResource("KeyUpImg")}",
                $"/DS4Windows;component/Resources/{(string)App.Current.FindResource("ClockImg")}",
            };
        }

        private MacroStep step;
        private string image;

        public string Image { get => image; }
        public MacroStep Step { get => step; }
        public int DisplayValue
        {
            get
            {
                int result = step.Value;
                if (step.ActType == MacroStep.StepType.Wait)
                {
                    result -= 300;
                }

                return result;
            }
            set
            {
                int result = value;
                if (step.ActType == MacroStep.StepType.Wait)
                {
                    result += 300;
                }

                step.Value = result;
            }
        }

        public int RumbleHeavy
        {
            get
            {
                int result = step.Value;
                result -= 1000000;
                string temp = result.ToString();
                result = int.Parse(temp.Substring(0, 3));
                return result;
            }
            set
            {
                int result = step.Value;
                result -= 1000000;
                int curHeavy = result / 1000;
                int curLight = result - (curHeavy * 1000);
                result = curLight + (value * 1000) + 1000000;
                step.Value = result;
            }
        }

        public int RumbleLight
        {
            get
            {
                int result = step.Value;
                result -= 1000000;
                string temp = result.ToString();
                result = int.Parse(temp.Substring(3, 3));
                return result;
            }
            set
            {
                int result = step.Value;
                result -= 1000000;
                int curHeavy = result / 1000;
                result = value + (curHeavy * 1000) + 1000000;
                step.Value = result;
            }
        }

        public MacroStepItem(MacroStep step)
        {
            this.step = step;
            image = imageSources[(int)step.ActType];
        }

        public void UpdateLightbarValue(Color color)
        {
            step.Value = 1000000000 + (color.R*1000000)+(color.G*1000)+color.B;
        }

        public Color LightbarColorValue()
        {
            int temp = step.Value - 1000000000;
            int r = temp / 1000000;
            temp -= (r * 1000000);
            int g = temp / 1000;
            temp -= (g * 1000);
            int b = temp;
            return new Color() { A = 255, R = (byte)r, G = (byte)g, B = (byte)b };
        }
    }
}
