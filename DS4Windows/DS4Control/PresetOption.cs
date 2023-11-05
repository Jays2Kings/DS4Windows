/*
DS4Windows
Copyright (C) 2023  Travis Nickles

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS4WinWPF.DS4Control
{
    public abstract class PresetOption
    {
        public enum OutputContChoice : ushort
        {
            None,
            Xbox360,
            DualShock4,
        }

        protected string name;
        protected string description;
        protected bool outputControllerChoice;
        protected OutputContChoice outputCont;

        public string Name { get => name; }
        public string Description { get => description; }
        public bool OutputControllerChoice { get => outputControllerChoice; }
        public OutputContChoice OutputCont
        {
            get => outputCont;
            set => outputCont = value;
        }

        public abstract void ApplyPreset(int idx);
    }

    public class GamepadPreset : PresetOption
    {
        public GamepadPreset()
        {
            name = Translations.Strings.GamepadPresetName;
            description = Translations.Strings.GamepadPresetDescription;
            outputControllerChoice = true;
            outputCont = OutputContChoice.Xbox360;
        }

        public override void ApplyPreset(int idx)
        {
            if (outputCont == OutputContChoice.Xbox360)
            {
                DS4Windows.Global.LoadBlankDevProfile(idx, false, App.rootHub, false);
            }
            else if (outputCont == OutputContChoice.DualShock4)
            {
                DS4Windows.Global.LoadBlankDS4Profile(idx, false, App.rootHub, false);
            }
        }
    }

    public class GamepadGyroCamera : PresetOption
    {
        public GamepadGyroCamera()
        {
            name = Translations.Strings.GamepadGyroCameraName;
            description = Translations.Strings.GamepadGyroCameraDescription;
            outputControllerChoice = true;
            outputCont = OutputContChoice.Xbox360;
        }

        public override void ApplyPreset(int idx)
        {
            if (outputCont == OutputContChoice.Xbox360)
            {
                DS4Windows.Global.LoadDefaultGamepadGyroProfile(idx, false, App.rootHub, false);
            }
            else if (outputCont == OutputContChoice.DualShock4)
            {
                DS4Windows.Global.LoadDefaultDS4GamepadGyroProfile(idx, false, App.rootHub, false);
            }
        }
    }

    public class MixedPreset : PresetOption
    {
        public MixedPreset()
        {
            name = Translations.Strings.MixedPresetName;
            description = Translations.Strings.MixedPresetDescription;
            outputControllerChoice = true;
            outputCont = OutputContChoice.Xbox360;
        }

        public override void ApplyPreset(int idx)
        {
            if (outputCont == OutputContChoice.Xbox360)
            {
                DS4Windows.Global.LoadDefaultMixedControlsProfile(idx, false, App.rootHub, false);
            }
            else if (outputCont == OutputContChoice.DualShock4)
            {
                DS4Windows.Global.LoadDefaultDS4MixedControlsProfile(idx, false, App.rootHub, false);
            }
        }
    }

    public class MixedGyroMousePreset : PresetOption
    {
        public MixedGyroMousePreset()
        {
            name = Translations.Strings.MixedGyroMousePresetName;
            description = Translations.Strings.MixedGyroMousePresetDescription;
            outputControllerChoice = true;
            outputCont = OutputContChoice.Xbox360;
        }

        public override void ApplyPreset(int idx)
        {
            if (outputCont == OutputContChoice.Xbox360)
            {
                DS4Windows.Global.LoadDefaultMixedGyroMouseProfile(idx, false, App.rootHub, false);
            }
            else if (outputCont == OutputContChoice.DualShock4)
            {
                DS4Windows.Global.LoadDefaultDS4MixedGyroMouseProfile(idx, false, App.rootHub, false);
            }
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
