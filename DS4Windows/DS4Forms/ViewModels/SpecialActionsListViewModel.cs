using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using DS4Windows;

namespace DS4WinWPF.DS4Forms.ViewModels
{
    public class SpecialActionsListViewModel
    {
        private int deviceNum;
        private ObservableCollection<SpecialActionItem> actionCol =
            new ObservableCollection<SpecialActionItem>();
        private int specialActionIndex = -1;

        public ObservableCollection<SpecialActionItem> ActionCol { get => actionCol; }
        public int DeviceNum { get => deviceNum; }
        public int SpecialActionIndex { get => specialActionIndex;
            set
            {
                if (specialActionIndex == value) return;
                specialActionIndex = value;
                SpecialActionIndexChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler SpecialActionIndexChanged;
        public bool ItemSelected { get => specialActionIndex >= 0; }
        public event EventHandler ItemSelectedChanged;

        public SpecialActionsListViewModel(int deviceNum)
        {
            this.deviceNum = deviceNum;

            SpecialActionIndexChanged += SpecialActionsListViewModel_SpecialActionIndexChanged;
        }

        private void SpecialActionsListViewModel_SpecialActionIndexChanged(object sender, EventArgs e)
        {
            ItemSelectedChanged?.Invoke(this, EventArgs.Empty);
        }

        public void LoadActions(bool newProfile = false)
        {
            actionCol.Clear();

            List<string> pactions = Global.ProfileActions[deviceNum];
            foreach (SpecialAction action in Global.GetActions())
            {
                string displayName = GetActionDisplayName(action);
                SpecialActionItem item = new SpecialActionItem(action, displayName);

                if (pactions.Contains(action.name))
                {
                    item.Active = true;
                }
                else if (newProfile && action.typeID == SpecialAction.ActionTypeId.DisconnectBT)
                {
                    item.Active = true;
                }

                actionCol.Add(item);
            }
        }

        public SpecialActionItem CreateActionItem(SpecialAction action)
        {
            string displayName = GetActionDisplayName(action);
            SpecialActionItem item = new SpecialActionItem(action, displayName);
            return item;
        }

        public string GetActionDisplayName(SpecialAction action)
        {
            string displayName = string.Empty;
            switch (action.typeID)
            {
                case SpecialAction.ActionTypeId.DisconnectBT:
                    displayName = Properties.Resources.DisconnectBT; break;
                case SpecialAction.ActionTypeId.Macro:
                    displayName = Properties.Resources.Macro + (action.keyType.HasFlag(DS4KeyType.ScanCode) ? " (" + Properties.Resources.ScanCode + ")" : "");
                    break;
                case SpecialAction.ActionTypeId.Program:
                    displayName = Properties.Resources.LaunchProgram.Replace("*program*", Path.GetFileNameWithoutExtension(action.details));
                    break;
                case SpecialAction.ActionTypeId.Profile:
                    displayName = Properties.Resources.LoadProfile.Replace("*profile*", action.details);
                    break;
                case SpecialAction.ActionTypeId.Key:
                    displayName = KeyInterop.KeyFromVirtualKey(int.Parse(action.details)).ToString() +
                         (action.uTrigger.Count > 0 ? " (Toggle)" : "");
                    break;
                case SpecialAction.ActionTypeId.BatteryCheck:
                    displayName = Properties.Resources.CheckBattery;
                    break;
                case SpecialAction.ActionTypeId.XboxGameDVR:
                    displayName = "Xbox Game DVR";
                    break;
                case SpecialAction.ActionTypeId.MultiAction:
                    displayName = Properties.Resources.MultiAction;
                    break;
                case SpecialAction.ActionTypeId.SASteeringWheelEmulationCalibrate:
                    displayName = Properties.Resources.SASteeringWheelEmulationCalibrate;
                    break;
                default: break;
            }

            return displayName;
        }

        public void ExportEnabledActions()
        {
            List<string> pactions = new List<string>();
            foreach(SpecialActionItem item in actionCol)
            {
                if (item.Active)
                {
                    pactions.Add(item.ActionName);
                }
            }

            Global.ProfileActions[deviceNum] = pactions;
            Global.CacheExtraProfileInfo(deviceNum);
        }

        public void RemoveAction(SpecialActionItem item)
        {
            Global.RemoveAction(item.SpecialAction.name);
            actionCol.RemoveAt(specialActionIndex);
            Global.ProfileActions[deviceNum].Remove(item.SpecialAction.name);
            Global.CacheExtraProfileInfo(deviceNum);
        }
    }

    public class SpecialActionItem
    {
        private SpecialAction specialAction;
        private bool active;
        private string typeName;

        public SpecialActionItem(SpecialAction specialAction, string displayName)
        {
            this.specialAction = specialAction;
            this.typeName = displayName;
        }

        public string ActionName
        {
            get => specialAction.name;
            set
            {
                specialAction.name = value;
            }
        }
        public event EventHandler ActionNameChanged;

        public bool Active
        {
            get => active;
            set
            {
                if (active == value) return;
                active = value;
                ActiveChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler ActiveChanged;
        public string Controls { get => specialAction.controls.Replace("/", ", "); }

        public event EventHandler ControlsChanged;

        public string TypeName { get => typeName; }
        public SpecialAction SpecialAction { get => specialAction; }

        public void Refresh()
        {
            ActionNameChanged?.Invoke(this, EventArgs.Empty);
            ControlsChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
