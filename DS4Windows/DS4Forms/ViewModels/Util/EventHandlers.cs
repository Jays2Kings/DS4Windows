using System;

namespace DS4WinWPF.DS4Forms.ViewModels.Util
{
    public delegate void PropertyChangingHandler<TValue>(object sender, TValue oldValue, TValue newValue);
}
