using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DS4Windows;
using DS4WinWPF.DS4Forms.ViewModels.Util;

namespace DS4WinWPF.DS4Forms.ViewModels.SpecialActions
{
    public class SASteeringWheelViewModel : NotifyDataErrorBase
    {
        private double delay;
        public double Delay { get => delay; set => delay = value; }

        public void LoadAction(SpecialAction action)
        {
            delay = action.delayTime;
        }

        public void SaveAction(SpecialAction action, bool edit = false)
        {
            Global.SaveAction(action.name, action.controls, 8, delay.ToString("#.##", Global.configFileDecimalCulture), edit);
        }

        public override bool IsValid(SpecialAction action)
        {
            ClearOldErrors();

            bool valid = true;
            List<string> delayErrors = new List<string>();

            if (delay < 0 || delay > 60)
            {
                delayErrors.Add("Delay out of range");
                errors["Delay"] = delayErrors;
                RaiseErrorsChanged("Delay");
            }

            return valid;
        }

        public override void ClearOldErrors()
        {
            if (errors.Count > 0)
            {
                errors.Clear();
                RaiseErrorsChanged("Delay");
            }
        }
    }
}
