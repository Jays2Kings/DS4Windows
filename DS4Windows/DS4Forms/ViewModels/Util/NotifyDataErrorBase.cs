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
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;

namespace DS4WinWPF.DS4Forms.ViewModels.Util
{
    public abstract class NotifyDataErrorBase : INotifyDataErrorInfo
    {
        protected Dictionary<string, List<string>> errors =
            new Dictionary<string, List<string>>();

        public bool HasErrors => errors.Count > 0;

        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        public IEnumerable GetErrors(string propertyName)
        {
            errors.TryGetValue(propertyName, out List<string> errorsForName);
            return errorsForName;
        }

        public void RaiseErrorsChanged(string propertyName)
        {
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        }

        public abstract bool IsValid(DS4Windows.SpecialAction action);
        public abstract void ClearOldErrors();
    }
}
