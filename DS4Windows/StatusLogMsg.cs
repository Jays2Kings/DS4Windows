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
