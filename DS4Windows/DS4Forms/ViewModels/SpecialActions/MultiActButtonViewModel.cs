using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DS4Windows;
using DS4WinWPF.DS4Forms.ViewModels.Util;

namespace DS4WinWPF.DS4Forms.ViewModels.SpecialActions
{
    public class MultiActButtonViewModel : NotifyDataErrorBase
    {
        private List<int> tapMacro = new List<int>();
        private List<int> holdMacro = new List<int>();
        private List<int> doubleTapMacro = new List<int>();
        private List<int>[] loadAccessArray;

        public List<int> TapMacro { get => tapMacro; }
        public List<int> HoldMacro { get => holdMacro; }
        public List<int> DoubleTapMacro { get => doubleTapMacro; }

        public string TapMacroText
        {
            get
            {
                string result = Properties.Resources.SelectMacro;
                if (tapMacro.Count > 0)
                {
                    result = Properties.Resources.MacroRecorded;
                }

                return result;
            }
        }
        public event EventHandler TapMacroTextChanged;

        public string HoldMacroText
        {
            get
            {
                string result = Properties.Resources.SelectMacro;
                if (holdMacro.Count > 0)
                {
                    result = Properties.Resources.MacroRecorded;
                }

                return result;
            }
        }
        public event EventHandler HoldMacroTextChanged;

        public string DoubleTapMacroText
        {
            get
            {
                string result = Properties.Resources.SelectMacro;
                if (doubleTapMacro.Count > 0)
                {
                    result = Properties.Resources.MacroRecorded;
                }

                return result;
            }
        }
        public event EventHandler DoubleTapMacroTextChanged;

        public MultiActButtonViewModel()
        {
            loadAccessArray = new List<int>[3] { tapMacro, holdMacro, doubleTapMacro };
        }

        public void LoadAction(SpecialAction action)
        {
            string[] dets = action.details.Split(',');
            for (int i = 0; i < 3; i++)
            {
                string[] macs = dets[i].Split('/');
                foreach (string s in macs)
                {
                    if (int.TryParse(s, out int v))
                        loadAccessArray[i].Add(v);
                }
            }
        }

        public void UpdateTapDisplayText()
        {
            TapMacroTextChanged?.Invoke(this, EventArgs.Empty);
        }

        public void UpdateHoldDisplayText()
        {
            HoldMacroTextChanged?.Invoke(this, EventArgs.Empty);
        }

        public void UpdateDoubleTapDisplayText()
        {
            DoubleTapMacroTextChanged?.Invoke(this, EventArgs.Empty);
        }

        public DS4ControlSettings PrepareTapSettings()
        {
            DS4ControlSettings settings = new DS4ControlSettings(DS4Controls.None);
            settings.action.actionMacro = tapMacro.ToArray();
            settings.actionType = DS4ControlSettings.ActionType.Macro;
            settings.keyType = DS4KeyType.Macro;
            return settings;
        }

        public DS4ControlSettings PrepareHoldSettings()
        {
            DS4ControlSettings settings = new DS4ControlSettings(DS4Controls.None);
            settings.action.actionMacro = holdMacro.ToArray();
            settings.actionType = DS4ControlSettings.ActionType.Macro;
            settings.keyType = DS4KeyType.Macro;
            return settings;
        }

        public DS4ControlSettings PrepareDoubleTapSettings()
        {
            DS4ControlSettings settings = new DS4ControlSettings(DS4Controls.None);
            settings.action.actionMacro = doubleTapMacro.ToArray();
            settings.actionType = DS4ControlSettings.ActionType.Macro;
            settings.keyType = DS4KeyType.Macro;
            return settings;
        }

        public void SaveAction(SpecialAction action, bool edit = false)
        {
            string details = string.Join("/", tapMacro) + "," +
                string.Join("/", holdMacro) + "," +
                string.Join("/", doubleTapMacro);
            Global.SaveAction(action.name, action.controls, 7, details, edit);
        }

        public override bool IsValid(SpecialAction action)
        {
            ClearOldErrors();

            bool valid = true;
            List<string> tapMacroErrors = new List<string>();
            List<string> holdMacroErrors = new List<string>();
            List<string> doubleTapMacroErrors = new List<string>();

            if (tapMacro.Count == 0)
            {
                tapMacroErrors.Add("No tap macro defined");
                errors["TapMacro"] = tapMacroErrors;
                RaiseErrorsChanged("TapMacro");
            }
            if (holdMacro.Count == 0)
            {
                holdMacroErrors.Add("No hold macro defined");
                errors["HoldMacro"] = holdMacroErrors;
                RaiseErrorsChanged("HoldMacro");
            }
            if (doubleTapMacro.Count == 0)
            {
                doubleTapMacroErrors.Add("No double tap macro defined");
                errors["DoubleTapMacro"] = doubleTapMacroErrors;
                RaiseErrorsChanged("DoubleTapMacro");
            }

            return valid;
        }

        public override void ClearOldErrors()
        {
            if (errors.Count > 0)
            {
                errors.Clear();
                RaiseErrorsChanged("TapMacro");
                RaiseErrorsChanged("HoldMacro");
                RaiseErrorsChanged("DoubleTapMacro");
            }
        }
    }
}
