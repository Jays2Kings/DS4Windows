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
        private int delay;
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
        public int Delay { get => delay; set => delay = value; }
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
            delay = (int)action.delayTime;
            arguments = action.extra;
        }

        public void SaveAction(SpecialAction action, bool edit = false)
        {
            Global.SaveAction(action.name, action.controls, 2, $"{filepath}?{delay}", edit, arguments);
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
