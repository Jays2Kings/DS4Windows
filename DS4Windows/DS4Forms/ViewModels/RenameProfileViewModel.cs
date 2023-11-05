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
using System.IO;
using DS4Windows;

namespace DS4WinWPF.DS4Forms.ViewModels
{
    public class RenameProfileViewModel
    {
        private string profileName;
        public string ProfileName
        {
            get => profileName;
            set
            {
                profileName = value;
                ProfileNameChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler ProfileNameChanged;

        public bool ProfileFileExists()
        {
            string filePath = Path.Combine(Global.appdatapath,
                "Profiles", $"{profileName}.xml");
            return File.Exists(filePath);
        }
    }
}
