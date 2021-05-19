using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS4WinWPF.DS4Forms.ViewModels
{
    public class MainWindowsViewModel
    {
        private bool fullTabsEnabled = true;

        public bool FullTabsEnabled
        {
            get => fullTabsEnabled;
            set
            {
                fullTabsEnabled = value;
                FullTabsEnabledChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler FullTabsEnabledChanged;

        public string updaterExe = Environment.Is64BitProcess ? "DS4Updater.exe" : "DS4Updater_x86.exe";
    }
}
