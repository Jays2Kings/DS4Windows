using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS4WinWPF.DS4Control
{
    public abstract class PresetOption
    {
        protected string name;
        protected string description;

        public string Name { get => name; }
        public string Description { get => description; }

        public abstract void ApplyPreset(int idx);
    }

    public class GamepadPreset : PresetOption
    {
        public GamepadPreset()
        {
            name = Translations.Strings.GamepadPresetName;
            description = Translations.Strings.GamepadPresetDescription;
        }

        public override void ApplyPreset(int idx)
        {
            DS4Windows.Global.LoadBlankDevProfile(idx, false, App.rootHub, false);
        }
    }

    public class GamepadGyroCamera : PresetOption
    {
        public GamepadGyroCamera()
        {
            name = Translations.Strings.GamepadGyroCameraName;
            description = Translations.Strings.GamepadGyroCameraDescription;
        }

        public override void ApplyPreset(int idx)
        {
            DS4Windows.Global.LoadDefaultGamepadGyroProfile(idx, false, App.rootHub, false);
        }
    }

    public class MixedPreset : PresetOption
    {
        public MixedPreset()
        {
            name = Translations.Strings.MixedPresetName;
            description = Translations.Strings.MixedPresetDescription;
        }

        public override void ApplyPreset(int idx)
        {
            DS4Windows.Global.LoadDefaultMixedControlsProfile(idx, false, App.rootHub, false);
        }
    }

    public class MixedGyroMousePreset : PresetOption
    {
        public MixedGyroMousePreset()
        {
            name = Translations.Strings.MixedGyroMousePresetName;
            description = Translations.Strings.MixedGyroMousePresetDescription;
        }

        public override void ApplyPreset(int idx)
        {
            DS4Windows.Global.LoadDefaultMixedGyroMouseProfile(idx, false, App.rootHub, false);
        }
    }

    public class KBMPreset : PresetOption
    {
        public KBMPreset()
        {
            name = Translations.Strings.KBMPresetName;
            description = Translations.Strings.KBMPresetDescription;
        }

        public override void ApplyPreset(int idx)
        {
            DS4Windows.Global.LoadDefaultKBMProfile(idx, false, App.rootHub, false);
        }
    }

    public class KBMGyroMouse : PresetOption
    {
        public KBMGyroMouse()
        {
            name = Translations.Strings.KBMGyroMouseName;
            description = Translations.Strings.KBMGyroMouseDescription;
        }

        public override void ApplyPreset(int idx)
        {
            DS4Windows.Global.LoadDefaultKBMGyroMouseProfile(idx, false, App.rootHub, false);
        }
    }
}
