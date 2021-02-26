using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DS4Windows;
using DS4WinWPF.DS4Forms.ViewModels.Util;

namespace DS4WinWPF.DS4Forms.ViewModels.SpecialActions
{
    public class MacroViewModel : NotifyDataErrorBase
    {
        private bool useScanCode;
        private bool runTriggerRelease;
        private bool syncRun;
        private bool keepKeyState;
        private bool repeatHeld;
        private List<int> macro = new List<int>(1);
        private string macrostring;

        public bool UseScanCode { get => useScanCode; set => useScanCode = value; }
        public bool RunTriggerRelease { get => runTriggerRelease; set => runTriggerRelease = value; }
        public bool SyncRun { get => syncRun; set => syncRun = value; }
        public bool KeepKeyState { get => keepKeyState; set => keepKeyState = value; }
        public bool RepeatHeld { get => repeatHeld; set => repeatHeld = value; }
        public List<int> Macro { get => macro; set => macro = value; }
        public string Macrostring { get => macrostring;
            set
            {
                macrostring = value;
                MacrostringChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public event EventHandler MacrostringChanged;

        public void LoadAction(SpecialAction action)
        {
            macro = action.macro;
            if (action.macro.Count > 0)
            {
                MacroParser macroParser = new MacroParser(action.macro.ToArray());
                macroParser.LoadMacro();
                macrostring = string.Join(", ", macroParser.GetMacroStrings());
            }

            useScanCode = action.keyType.HasFlag(DS4KeyType.ScanCode);
            runTriggerRelease = action.pressRelease;
            syncRun = action.synchronized;
            keepKeyState = action.keepKeyState;
            repeatHeld = action.keyType.HasFlag(DS4KeyType.RepeatMacro);
        }

        public DS4ControlSettings PrepareSettings()
        {
            DS4ControlSettings settings = new DS4ControlSettings(DS4Controls.None);
            settings.action.actionMacro = macro.ToArray();
            settings.actionType = DS4ControlSettings.ActionType.Macro;
            settings.keyType = DS4KeyType.Macro;
            if (repeatHeld)
            {
                settings.keyType |= DS4KeyType.RepeatMacro;
            }

            return settings;
        }

        public void SaveAction(SpecialAction action, bool edit = false)
        {
            List<string> extrasList = new List<string>();
            extrasList.Add(useScanCode ? "Scan Code" : null);
            extrasList.Add(runTriggerRelease ? "RunOnRelease" : null);
            extrasList.Add(syncRun ? "Sync" : null);
            extrasList.Add(keepKeyState ? "KeepKeyState" : null);
            extrasList.Add(repeatHeld ? "Repeat" : null);
            Global.SaveAction(action.name, action.controls, 1, string.Join("/", macro), edit,
                string.Join("/", extrasList.Where(s => !string.IsNullOrEmpty(s))));
        }

        public void UpdateMacroString()
        {
            string temp = "";
            if (macro.Count > 0)
            {
                MacroParser macroParser = new MacroParser(macro.ToArray());
                macroParser.LoadMacro();
                temp = string.Join(", ", macroParser.GetMacroStrings());
            }

            Macrostring = temp;
        }

        public override bool IsValid(SpecialAction action)
        {
            ClearOldErrors();

            bool valid = true;
            List<string> macroErrors = new List<string>();

            if (macro.Count == 0)
            {
                valid = false;
                macroErrors.Add("No macro defined");
                errors["Macro"] = macroErrors;
                RaiseErrorsChanged("Macro");
            }

            return valid;
        }

        public override void ClearOldErrors()
        {
            if (errors.Count > 0)
            {
                errors.Clear();
                RaiseErrorsChanged("Macro");
            }
        }
    }
}
