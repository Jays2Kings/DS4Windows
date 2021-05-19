using System;
using System.Collections.Generic;
using DS4WinWPF.DS4Control;
using DS4WinWPF.DS4Forms.ViewModels.Util;

namespace DS4WinWPF.DS4Forms.ViewModels
{
    public class PresetOptionViewModel
    {
        private int presetIndex;
        private PresetOption.OutputContChoice controllerChoice =
            PresetOption.OutputContChoice.Xbox360;

        public int PresetIndex
        {
            get => presetIndex;
            set
            {
                if (presetIndex == value) return;
                presetIndex = value;
                PresetIndexChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler PresetIndexChanged;

        private List<PresetOption> presetList;
        public List<PresetOption> PresetsList { get => presetList; }

        public string PresetDescription
        {
            get => presetList[presetIndex].Description;
        }
        public event EventHandler PresetDescriptionChanged;

        public bool PresetDisplayOutputCont
        {
            get => presetList[presetIndex].OutputControllerChoice;
        }
        public event EventHandler PresetDisplayOutputContChanged;

        public PresetOption.OutputContChoice ControllerChoice
        {
            get => controllerChoice;
            set => controllerChoice = value;
        }

        private List<EnumChoiceSelection<PresetOption.OutputContChoice>> outputChoices =
            new List<EnumChoiceSelection<PresetOption.OutputContChoice>>()
            {
                new EnumChoiceSelection<PresetOption.OutputContChoice>("Xbox 360", PresetOption.OutputContChoice.Xbox360),
                new EnumChoiceSelection<PresetOption.OutputContChoice>("DualShock 4", PresetOption.OutputContChoice.DualShock4),
            };

        public List<EnumChoiceSelection<PresetOption.OutputContChoice>> OutputChoices { get => outputChoices; }

        public PresetOptionViewModel()
        {
            presetList = new List<PresetOption>();
            presetList.Add(new GamepadPreset());
            presetList.Add(new GamepadGyroCamera());
            presetList.Add(new MixedPreset());
            presetList.Add(new MixedGyroMousePreset());
            presetList.Add(new KBMPreset());
            presetList.Add(new KBMGyroMouse());

            PresetIndexChanged += PresetOptionViewModel_PresetIndexChanged;
        }

        private void PresetOptionViewModel_PresetIndexChanged(object sender, EventArgs e)
        {
            PresetDescriptionChanged?.Invoke(this, EventArgs.Empty);
            PresetDisplayOutputContChanged?.Invoke(this, EventArgs.Empty);
        }

        public void ApplyPreset(int index)
        {
            if (presetIndex >= 0)
            {
                PresetOption current = presetList[presetIndex];
                if (current.OutputControllerChoice &&
                    controllerChoice != PresetOption.OutputContChoice.None)
                {
                    current.OutputCont = controllerChoice;
                }

                current.ApplyPreset(index);
            }
        }
    }
}
