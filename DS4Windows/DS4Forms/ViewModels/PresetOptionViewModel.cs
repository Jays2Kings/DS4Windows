using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DS4WinWPF.DS4Control;

namespace DS4WinWPF.DS4Forms.ViewModels
{
    public class PresetOptionViewModel
    {
        private int presetIndex;

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
        }

        public void ApplyPreset(int index)
        {
            if (presetIndex >= 0)
            {
                PresetOption current = presetList[presetIndex];
                current.ApplyPreset(index);
            }
        }
    }
}
