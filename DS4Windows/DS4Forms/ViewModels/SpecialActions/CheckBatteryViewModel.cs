using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using DS4Windows;
using DS4WinWPF.DS4Forms.ViewModels.Util;

namespace DS4WinWPF.DS4Forms.ViewModels.SpecialActions
{
    public class CheckBatteryViewModel : NotifyDataErrorBase
    {
        private int delay;
        private bool notification;
        private bool lightbar = true;
        private Color emptyColor
            = new Color() { A = 255, R = 255, G = 0, B = 0 };
        private Color fullColor =
            new Color() { A = 255, R = 0, G = 255, B = 0 };

        public int Delay { get => delay; set => delay = value; }
        public bool Notification { get => notification; set => notification = value; }
        public bool Lightbar { get => lightbar; set => lightbar = value; }
        public Color EmptyColor
        {
            get => emptyColor;
            set
            {
                if (emptyColor == value) return;
                emptyColor = value;
                EmptyColorChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler EmptyColorChanged;
        public Color FullColor
        {
            get => fullColor;
            set
            {
                if (fullColor == value) return;
                fullColor = value;
                FullColorChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler FullColorChanged;

        public void UpdateForcedColor(System.Windows.Media.Color color, int device)
        {
            if (device < ControlService.CURRENT_DS4_CONTROLLER_LIMIT)
            {
                DS4Color dcolor = new DS4Color() { red = color.R, green = color.G, blue = color.B };
                DS4LightBar.forcedColor[device] = dcolor;
                DS4LightBar.forcedFlash[device] = 0;
                DS4LightBar.forcelight[device] = true;
            }
        }

        public void StartForcedColor(System.Windows.Media.Color color, int device)
        {
            if (device < ControlService.CURRENT_DS4_CONTROLLER_LIMIT)
            {
                DS4Color dcolor = new DS4Color() { red = color.R, green = color.G, blue = color.B };
                DS4LightBar.forcedColor[device] = dcolor;
                DS4LightBar.forcedFlash[device] = 0;
                DS4LightBar.forcelight[device] = true;
            }
        }

        public void EndForcedColor(int device)
        {
            if (device < ControlService.CURRENT_DS4_CONTROLLER_LIMIT)
            {
                DS4LightBar.forcedColor[device] = new DS4Color(0, 0, 0);
                DS4LightBar.forcedFlash[device] = 0;
                DS4LightBar.forcelight[device] = false;
            }
        }

        public void LoadAction(SpecialAction action)
        {
            string[] details = action.details.Split(',');
            delay = (int)action.delayTime;
            bool.TryParse(details[1], out notification);
            bool.TryParse(details[2], out lightbar);
            emptyColor = Color.FromArgb(255, byte.Parse(details[3]), byte.Parse(details[4]), byte.Parse(details[5]));
            fullColor = Color.FromArgb(255, byte.Parse(details[6]), byte.Parse(details[7]), byte.Parse(details[8]));
        }

        public void SaveAction(SpecialAction action, bool edit = false)
        {
            string details = $"{delay}|{notification}|{lightbar}|{emptyColor.R}|{emptyColor.G}|{emptyColor.B}|" +
                $"{fullColor.R}|{fullColor.G}|{fullColor.B}";

            Global.SaveAction(action.name, action.controls, 6, details, edit);
        }

        public override bool IsValid(SpecialAction action)
        {
            ClearOldErrors();

            bool valid = true;
            List<string> notificationErrors = new List<string>();
            List<string> lightbarErrors = new List<string>();

            if (!notification && !lightbar)
            {
                notificationErrors.Add("Need status option");
                lightbarErrors.Add("Need status option");
            }
            else if (lightbar)
            {
                if (emptyColor == fullColor)
                {
                    lightbarErrors.Add("Need to set two different colors");
                }
            }

            if (notificationErrors.Count > 0)
            {
                errors["Notification"] = notificationErrors;
                RaiseErrorsChanged("Notification");
            }
            if (lightbarErrors.Count > 0)
            {
                errors["Lightbar"] = lightbarErrors;
                RaiseErrorsChanged("Lightbar");
            }

            return valid;
        }

        public override void ClearOldErrors()
        {
            if (errors.Count > 0)
            {
                errors.Clear();
                RaiseErrorsChanged("Notification");
                RaiseErrorsChanged("Lightbar");
            }
        }
    }
}
