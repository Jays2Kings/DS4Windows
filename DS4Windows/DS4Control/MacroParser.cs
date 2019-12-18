using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DS4Windows
{
    public class MacroParser
    {
        private bool loaded;
        private List<MacroStep> macroSteps;
        private int[] inputMacro;
        private Dictionary<int, bool> keydown = new Dictionary<int, bool>();
        public static Dictionary<int, string> macroInputNames = new Dictionary<int, string>()
        {
            [256] = "Left Mouse Button", [257] = "Right Mouse Button",
            [258] = "Middle Mouse Button", [259] = "4th Mouse Button",
            [260] = "5th Mouse Button", [261] = "A Button",
            [262] = "B Button", [263] = "X Button",
            [264] = "Y Button", [265] = "Start",
            [266] = "Back", [267] = "Up Button",
            [268] = "Down Button", [269] = "Left Button",
            [270] = "Right Button", [271] = "Guide",
            [272] = "Left Bumper", [273] = "Right Bumper",
            [274] = "Left Trigger", [275] = "Right Trigger",
            [276] = "Left Stick", [277] = "Right Stick",
            [278] = "LS Right", [279] = "LS Left",
            [280] = "LS Down", [281] = "LS Up",
            [282] = "RS Right", [283] = "RS Left",
            [284] = "RS Down", [285] = "RS Up",
        };

        public List<MacroStep> MacroSteps { get => macroSteps; }

        public MacroParser(int[] macro)
        {
            macroSteps = new List<MacroStep>();
            inputMacro = macro;
        }

        public void LoadMacro()
        {
            if (loaded)
            {
                return;
            }

            keydown.Clear();
            for(int i = 0; i < inputMacro.Length; i++)
            {
                int value = inputMacro[i];
                MacroStep step = ParseStep(value);
                macroSteps.Add(step);
            }

            loaded = true;
        }

        public List<string> GetMacroStrings()
        {
            if (!loaded)
            {
                LoadMacro();
            }

            List<string> result = new List<string>();
            foreach(MacroStep step in macroSteps)
            {
                result.Add(step.Name);
            }

            return result;
        }

        private MacroStep ParseStep(int value)
        {
            string name = string.Empty;
            MacroStep.StepType type = MacroStep.StepType.ActDown;
            MacroStep.StepOutput outType = MacroStep.StepOutput.Key;

            if (value >= 1000000000)
            {
                outType = MacroStep.StepOutput.Lightbar;
                if (value > 1000000000)
                {
                    type = MacroStep.StepType.ActDown;
                    string lb = value.ToString().Substring(1);
                    byte r = (byte)(int.Parse(lb[0].ToString()) * 100 + int.Parse(lb[1].ToString()) * 10 + int.Parse(lb[2].ToString()));
                    byte g = (byte)(int.Parse(lb[3].ToString()) * 100 + int.Parse(lb[4].ToString()) * 10 + int.Parse(lb[5].ToString()));
                    byte b = (byte)(int.Parse(lb[6].ToString()) * 100 + int.Parse(lb[7].ToString()) * 10 + int.Parse(lb[8].ToString()));
                    name = $"Lightbar Color: {r},{g},{b}";
                }
                else
                {
                    type = MacroStep.StepType.ActUp;
                    name = "Reset Lightbar";
                }
            }
            else if (value >= 1000000)
            {
                outType = MacroStep.StepOutput.Rumble;
                if (value > 1000000)
                {
                    type = MacroStep.StepType.ActDown;
                    string r = value.ToString().Substring(1);
                    byte heavy = (byte)(int.Parse(r[0].ToString()) * 100 + int.Parse(r[1].ToString()) * 10 + int.Parse(r[2].ToString()));
                    byte light = (byte)(int.Parse(r[3].ToString()) * 100 + int.Parse(r[4].ToString()) * 10 + int.Parse(r[5].ToString()));
                    name = $"Rumble {heavy}, {light} ({Math.Round((heavy * .75f + light * .25f) / 2.55f, 1)}%)";
                }
                else
                {
                    type = MacroStep.StepType.ActUp;
                    name = "Stop Rumble";
                }
            }
            else if (value >= 300) // ints over 300 used to delay
            {
                type = MacroStep.StepType.Wait;
                outType = MacroStep.StepOutput.None;
                name = $"Wait {(value - 300).ToString()} ms";
            }
            else
            {
                // anything above 255 is not a key value
                outType = value <= 255 ? MacroStep.StepOutput.Key : MacroStep.StepOutput.Button;
                keydown.TryGetValue(value, out bool isdown);
                if (!isdown)
                {
                    type = MacroStep.StepType.ActDown;
                    keydown.Add(value, true);
                    if (outType == MacroStep.StepOutput.Key)
                    {
                        name = KeyInterop.KeyFromVirtualKey(value).ToString();
                    }
                    else
                    {
                        macroInputNames.TryGetValue(value, out name);
                    }
                }
                else
                {
                    type = MacroStep.StepType.ActUp;
                    keydown.Remove(value);
                    if (outType == MacroStep.StepOutput.Key)
                    {
                        name = KeyInterop.KeyFromVirtualKey(value).ToString();
                    }
                    else
                    {
                        macroInputNames.TryGetValue(value, out name);
                    }
                }
            }

            MacroStep step = new MacroStep(value, name, type, outType);
            return step;
        }

        public void Reset()
        {
            loaded = false;
        }
    }

    public class MacroStep
    {
        public enum StepType : uint
        {
            ActDown,
            ActUp,
            Wait,
        }

        public enum StepOutput : uint
        {
            None,
            Key,
            Button,
            Rumble,
            Lightbar,
        }

        private string name;
        private int value;
        private StepType actType;
        private StepOutput outputType;

        public string Name
        {
            get => name;
            set
            {
                name = value;
                NameChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler NameChanged;
        public int Value
        {
            get => value;
            set
            {
                this.value = value;
                ValueChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler ValueChanged;
        public StepType ActType { get => actType; }
        public StepOutput OutputType { get => outputType; }

        public MacroStep(int value, string name, StepType act, StepOutput output)
        {
            this.value = value;
            this.name = name;
            actType = act;
            outputType = output;

            ValueChanged += MacroStep_ValueChanged;
        }

        private void MacroStep_ValueChanged(object sender, EventArgs e)
        {
            if (actType == StepType.Wait)
            {
                Name = $"Wait {value-300}ms";
            }
            else if (outputType == StepOutput.Rumble)
            {
                int result = value;
                result -= 1000000;
                int curHeavy = result / 1000;
                int curLight = result - (curHeavy * 1000);
                Name = $"Rumble {curHeavy},{curLight}";
            }
            else if (outputType == StepOutput.Lightbar)
            {
                int temp = value - 1000000000;
                int r = temp / 1000000;
                temp -= (r * 1000000);
                int g = temp / 1000;
                temp -= (g * 1000);
                int b = temp;
                Name = $"Lightbar Color: {r},{g},{b}";
            }
        }
    }
}
