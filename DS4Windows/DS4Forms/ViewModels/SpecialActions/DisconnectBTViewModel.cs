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
using DS4Windows;
using DS4WinWPF.DS4Forms.ViewModels.Util;

namespace DS4WinWPF.DS4Forms.ViewModels.SpecialActions
{
    public class DisconnectBTViewModel : NotifyDataErrorBase
    {
        private double holdInterval;
        public double HoldInterval { get => holdInterval; set => holdInterval = value; }

        public void LoadAction(SpecialAction action)
        {
            holdInterval = action.delayTime;
        }

        public void SaveAction(SpecialAction action, bool edit = false)
        {
            Global.SaveAction(action.name, action.controls, 5, $"{holdInterval.ToString("#.##", Global.configFileDecimalCulture)}", edit, holdInterval);
        }

        public override bool IsValid(SpecialAction action)
        {
            ClearOldErrors();

            bool valid = true;
            List<string> holdIntervalErrors = new List<string>();

            if (holdInterval < 0 || holdInterval > 60)
            {
                holdIntervalErrors.Add("Interval not valid");
                errors["HoldInterval"] = holdIntervalErrors;
                RaiseErrorsChanged("HoldInterval");
            }

            return valid;
        }

        public override void ClearOldErrors()
        {
            if (errors.Count > 0)
            {
                errors.Clear();
                RaiseErrorsChanged("HoldInterval");
            }
        }
    }
}
