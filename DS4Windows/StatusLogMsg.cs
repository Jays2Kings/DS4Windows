using System;

namespace DS4WinWPF
{
    public class StatusLogMsg
    {
        private string message;
        private bool warning;
        public string Message
        {
            get => message;
            set
            {
                if (message == value) return;
                message = value;
                MessageChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public event EventHandler MessageChanged;

        public bool Warning { get => warning;
            set
            {
                if (warning == value) return;
                warning = value;
                WarningChanged?.Invoke(this, EventArgs.Empty);
                ColorChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public event EventHandler WarningChanged;

        public string Color
        {
            get
            {
                return warning ? "Red" : "#FF696969";
            }
        }

        public event EventHandler ColorChanged;
    }
}
