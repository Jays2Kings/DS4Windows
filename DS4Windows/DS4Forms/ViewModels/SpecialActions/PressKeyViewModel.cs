using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using DS4Windows;
using DS4WinWPF.DS4Forms.ViewModels.Util;

namespace DS4WinWPF.DS4Forms.ViewModels.SpecialActions
{
    public class PressKeyViewModel : NotifyDataErrorBase
    {
        private string describeText;
        private DS4KeyType keyType;
        private int value;
        private int pressReleaseIndex = 0;
        private bool normalTrigger = true;
        public bool IsToggle => (keyType & DS4KeyType.Toggle) != 0;
        public event EventHandler IsToggleChanged;

        public Visibility ShowToggleControls
        {
            get
            {
                return ((keyType & DS4KeyType.Toggle) != 0) ? Visibility.Visible : Visibility.Collapsed;
            }
        }
        public event EventHandler ShowToggleControlsChanged;

        public string DescribeText
        {
            get
            {
                string result = "Select a Key";
                if (!string.IsNullOrEmpty(describeText))
                {
                    result = describeText;
                };

                return result;
            }
        }
        public event EventHandler DescribeTextChanged;
        public DS4KeyType KeyType { get => keyType; set => keyType = value; }
        public int Value { get => value; set => this.value = value; }
        public int PressReleaseIndex { get => pressReleaseIndex; set => pressReleaseIndex = value; }
        public bool NormalTrigger { get => normalTrigger; set => normalTrigger = value; }
        public bool UnloadError
        {
            get => errors.TryGetValue("UnloadError", out _);
        }

        public void LoadAction(SpecialAction action)
        {
            keyType = action.keyType;
            if (!string.IsNullOrEmpty(action.ucontrols))
            {
                keyType |= DS4KeyType.Toggle;
            }

            int.TryParse(action.details, out value);

            if (action.pressRelease)
            {
                pressReleaseIndex = 1;
            }

            UpdateDescribeText();
            UpdateToggleControls();
        }

        public void UpdateDescribeText()
        {
            describeText = KeyInterop.KeyFromVirtualKey(value).ToString() +
                (keyType.HasFlag(DS4KeyType.ScanCode) ? " (SC)" : "") +
                (keyType.HasFlag(DS4KeyType.Toggle) ? " (Toggle)" : "");

            DescribeTextChanged?.Invoke(this, EventArgs.Empty);
        }

        public void UpdateToggleControls()
        {
            IsToggleChanged?.Invoke(this, EventArgs.Empty);
            ShowToggleControlsChanged?.Invoke(this, EventArgs.Empty);
        }

        public DS4ControlSettings PrepareSettings()
        {
            DS4ControlSettings settings = new DS4ControlSettings(DS4Controls.None);
            settings.action.actionKey = value;
            settings.keyType = keyType;
            settings.actionType = DS4ControlSettings.ActionType.Key;
            return settings;
        }

        public void ReadSettings(DS4ControlSettings settings)
        {
            value = (int)settings.action.actionKey;
            keyType = settings.keyType;
        }

        public void SaveAction(SpecialAction action, bool edit = false)
        {
            string uaction = null;
            if (keyType.HasFlag(DS4KeyType.Toggle))
            {
                uaction = "Press";
                if (pressReleaseIndex == 1)
                {
                    uaction = "Release";
                }
            }

            Global.SaveAction(action.name, action.controls, 4,
                $"{value}{(keyType.HasFlag(DS4KeyType.ScanCode) ? " Scan Code" : "")}", edit,
                !string.IsNullOrEmpty(uaction) ? $"{uaction}\n{action.ucontrols}" : "");
        }

        public override bool IsValid(SpecialAction action)
        {
            ClearOldErrors();

            bool valid = true;
            List<string> valueErrors = new List<string>();
            List<string> toggleErrors = new List<string>();

            if (value == 0)
            {
                valueErrors.Add("No key defined");
                errors["Value"] = valueErrors;
                RaiseErrorsChanged("Value");
            }
            if (keyType.HasFlag(DS4KeyType.Toggle) && string.IsNullOrEmpty(action.ucontrols))
            {
                toggleErrors.Add("No unload triggers specified");
                errors["UnloadError"] = toggleErrors;
                RaiseErrorsChanged("UnloadError");
            }

            return valid;
        }

        public override void ClearOldErrors()
        {
            if (errors.Count > 0)
            {
                errors.Clear();
                RaiseErrorsChanged("Value");
                RaiseErrorsChanged("UnloadError");
            }
        }
    }
}
