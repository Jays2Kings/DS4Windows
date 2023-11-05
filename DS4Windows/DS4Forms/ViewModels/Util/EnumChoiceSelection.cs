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

namespace DS4WinWPF.DS4Forms.ViewModels.Util
{
    public class EnumChoiceSelection<T>
    {
        private string displayName;
        public string DisplayName { get => displayName; }

        private T choiceValue;
        public T ChoiceValue
        {
            get => choiceValue;
            set => choiceValue = value;
        }

        public EnumChoiceSelection(string name, T currentValue)
        {
            displayName = name;
            choiceValue = currentValue;
        }
    }
}
