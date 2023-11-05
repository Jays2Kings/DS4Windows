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
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using DS4Windows;
using DS4WinWPF.DS4Forms.ViewModels.Util;

namespace DS4WinWPF.DS4Forms.ViewModels.SpecialActions
{
    public class LaunchProgramViewModel : NotifyDataErrorBase
    {
        private string filepath;
        private double delay;
        private string arguments;

        public string Filepath
        {
            get => filepath;
            set
            {
                if (filepath == value) return;
                filepath = value;
                FilepathChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler FilepathChanged;
        public double Delay { get => delay; set => delay = value; }
        public string Arguments { get => arguments; set => arguments = value; }

        public ImageSource ProgramIcon
        {
            get
            {
                ImageSource exeicon = null;
                string path = filepath;
                if (File.Exists(path) && Path.GetExtension(path).ToLower() == ".exe")
                {
                    using (Icon ico = Icon.ExtractAssociatedIcon(path))
                    {
                        exeicon = Imaging.CreateBitmapSourceFromHIcon(ico.Handle, Int32Rect.Empty,
                            BitmapSizeOptions.FromEmptyOptions());
                        exeicon.Freeze();
                    }
                }

                return exeicon;
            }
        }
        public event EventHandler ProgramIconChanged;

        public string ProgramName
        {
            get
            {
                string temp = "";
                if (!string.IsNullOrEmpty(filepath))
                {
                    temp = Path.GetFileNameWithoutExtension(filepath);
                }

                return temp;
            }
        }
        public event EventHandler ProgramNameChanged;

        public LaunchProgramViewModel()
        {
            FilepathChanged += LaunchProgramViewModel_FilepathChanged;
        }

        private void LaunchProgramViewModel_FilepathChanged(object sender, EventArgs e)
        {
            ProgramIconChanged?.Invoke(this, EventArgs.Empty);
            ProgramNameChanged?.Invoke(this, EventArgs.Empty);
        }

        public void LoadAction(SpecialAction action)
        {
            filepath = action.details;
            delay = action.delayTime;
            arguments = action.extra;
        }

        public void SaveAction(SpecialAction action, bool edit = false)
        {
            Global.SaveAction(action.name, action.controls, 2, $"{filepath}?{delay.ToString("#.##", Global.configFileDecimalCulture)}", edit, delay, arguments);
        }

        public override bool IsValid(SpecialAction action)
        {
            ClearOldErrors();

            bool valid = true;
            List<string> filepathErrors = new List<string>();

            if (filepath.Length == 0)
            {
                filepathErrors.Add("Filepath empty");
            }
            else if (!File.Exists(filepath))
            {
                filepathErrors.Add("File at path does not exist");
            }

            if (filepathErrors.Count > 0)
            {
                errors["Filepath"] = filepathErrors;
                RaiseErrorsChanged("Filepath");
            }

            return valid;
        }

        public override void ClearOldErrors()
        {
            if (errors.Count > 0)
            {
                errors.Clear();
                RaiseErrorsChanged("Filepath");
            }
        }
    }
}
