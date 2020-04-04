using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DS4Windows;
using DS4WinWPF.DS4Forms.ViewModels.Util;

namespace DS4WinWPF.DS4Forms.ViewModels
{
    public class SpecialActEditorViewModel : NotifyDataErrorBase
    {
        private int deviceNum;
        private int actionTypeIndex = 0;
        private string actionName;
        private SpecialAction savedaction;
        private SpecialAction.ActionTypeId[] typeAssoc = new SpecialAction.ActionTypeId[]
        {
            SpecialAction.ActionTypeId.None, SpecialAction.ActionTypeId.Macro,
            SpecialAction.ActionTypeId.Program, SpecialAction.ActionTypeId.Profile,
            SpecialAction.ActionTypeId.Key, SpecialAction.ActionTypeId.DisconnectBT,
            SpecialAction.ActionTypeId.BatteryCheck, SpecialAction.ActionTypeId.MultiAction,
            SpecialAction.ActionTypeId.SASteeringWheelEmulationCalibrate,
        };

        private List<string> controlTriggerList = new List<string>();
        private List<string> controlUnloadTriggerList = new List<string>();
        private bool editMode;

        public int DeviceNum { get => deviceNum; }
        public int ActionTypeIndex { get => actionTypeIndex; set => actionTypeIndex = value; }
        public string ActionName { get => actionName; set => actionName = value; }
        public SpecialAction.ActionTypeId[] TypeAssoc { get => typeAssoc; }
        public SpecialAction SavedAction { get => savedaction; }
        public List<string> ControlTriggerList { get => controlTriggerList; }
        public List<string> ControlUnloadTriggerList { get => controlUnloadTriggerList; }
        public bool EditMode { get => editMode; }

        public bool TriggerError
        {
            get
            {
                return errors.TryGetValue("TriggerError", out List<string> _);
            }
        }

        public bool ExistingName { get => existingName; }

        private bool existingName;


        public SpecialActEditorViewModel(int deviceNum, SpecialAction action)
        {
            this.deviceNum = deviceNum;
            savedaction = action;
            editMode = savedaction != null;
        }

        public void LoadAction(SpecialAction action)
        {
            foreach (string s in action.controls.Split('/'))
            {
                controlTriggerList.Add(s);
            }

            if (action.ucontrols != null)
            {
                foreach (string s in action.ucontrols.Split('/'))
                {
                    if (s != "AutomaticUntrigger")
                    {
                        controlUnloadTriggerList.Add(s);
                    }
                }
            }

            actionName = action.name;
            for(int i = 0; i < typeAssoc.Length; i++)
            {
                SpecialAction.ActionTypeId type = typeAssoc[i];
                if (type == action.typeID)
                {
                    actionTypeIndex = i;
                    break;
                }
            }
        }

        public void SetAction(SpecialAction action)
        {
            action.name = actionName;
            action.controls = string.Join("/", controlTriggerList.ToArray());
            if (controlUnloadTriggerList.Count > 0)
            {
                action.ucontrols = string.Join("/", controlUnloadTriggerList.ToArray());
            }
            action.typeID = typeAssoc[actionTypeIndex];
        }

        public override bool IsValid(SpecialAction action)
        {
            ClearOldErrors();

            bool valid = true;
            List<string> actionNameErrors = new List<string>();
            List<string> triggerErrors = new List<string>();
            List<string> typeErrors = new List<string>();

            if (string.IsNullOrWhiteSpace(actionName))
            {
                valid = false;
                actionNameErrors.Add("No name provided");
            }
            else if (!editMode || savedaction.name != actionName)
            {
                // Perform existing name check when creating a new action
                // or if the action name has changed
                foreach (SpecialAction sA in Global.GetActions())
                {
                    if (sA.name == actionName)
                    {
                        valid = false;
                        actionNameErrors.Add("Existing action with name already exists");
                        existingName = true;
                        break;
                    }
                }
            }

            if (controlTriggerList.Count == 0)
            {
                valid = false;
                triggerErrors.Add("No triggers provided");
            }

            if (ActionTypeIndex == 0)
            {
                valid = false;
                typeErrors.Add("Specify an action type");
            }

            if (actionNameErrors.Count > 0)
            {
                errors["ActionName"] = actionNameErrors;
                RaiseErrorsChanged("ActionName");
            }
            if (triggerErrors.Count > 0)
            {
                errors["TriggerError"] = triggerErrors;
                RaiseErrorsChanged("TriggerError");
            }
            if (typeErrors.Count > 0)
            {
                errors["ActionTypeIndex"] = typeErrors;
                RaiseErrorsChanged("ActionTypeIndex");
            }

            return valid;
        }

        public override void ClearOldErrors()
        {
            existingName = false;

            if (errors.Count > 0)
            {
                errors.Clear();
                RaiseErrorsChanged("ActionName");
                RaiseErrorsChanged("TriggerError");
                RaiseErrorsChanged("ActionTypeIndex");
            }
        }
    }
}
