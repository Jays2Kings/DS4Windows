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
