using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using DS4Windows;

namespace DS4WinWPF.DS4Forms.ViewModels
{
    public class MappingListViewModel
    {
        private int devIndex;
        private ObservableCollection<MappedControl> mappings = new ObservableCollection<MappedControl>();
        public ObservableCollection<MappedControl> Mappings { get => mappings; }

        private int selectedIndex = -1;
        public int SelectedIndex
        {
            get => selectedIndex;
            set
            {
                if (selectedIndex == value) return;
                selectedIndex = value;
                SelectedIndexChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler SelectedIndexChanged;

        private Dictionary<DS4Controls, MappedControl> controlMap = new Dictionary<DS4Controls, MappedControl>();
        public Dictionary<DS4Controls, MappedControl> ControlMap { get => controlMap; }

        private MappedControl l2FullPullControl;
        public MappedControl L2FullPullControl { get => l2FullPullControl; }

        private MappedControl r2FullPullControl;
        public MappedControl R2FullPullControl { get => r2FullPullControl; }

        private MappedControl gyroSwipeLeftControl;
        private MappedControl gyroSwipeRightControl;
        private MappedControl gyroSwipeUpControl;
        private MappedControl gyroSwipeDownControl;

        private List<MappedControl> extraControls = new List<MappedControl>();

        public MappingListViewModel(int devIndex, OutContType devType)
        {
            mappings.Add(new MappedControl(devIndex, DS4Controls.Cross, "Cross",  devType));
            mappings.Add(new MappedControl(devIndex, DS4Controls.Circle, "Circle", devType));
            mappings.Add(new MappedControl(devIndex, DS4Controls.Square, "Square", devType));
            mappings.Add(new MappedControl(devIndex, DS4Controls.Triangle, "Triangle", devType));
            mappings.Add(new MappedControl(devIndex, DS4Controls.Options, "Options", devType));
            mappings.Add(new MappedControl(devIndex, DS4Controls.Share, "Share", devType));
            mappings.Add(new MappedControl(devIndex, DS4Controls.DpadUp, "Up", devType));
            mappings.Add(new MappedControl(devIndex, DS4Controls.DpadDown, "Down", devType));
            mappings.Add(new MappedControl(devIndex, DS4Controls.DpadLeft, "Left", devType));
            mappings.Add(new MappedControl(devIndex, DS4Controls.DpadRight, "Right", devType));
            mappings.Add(new MappedControl(devIndex, DS4Controls.PS, "PS", devType));
            mappings.Add(new MappedControl(devIndex, DS4Controls.Mute, "Mute", devType));
            mappings.Add(new MappedControl(devIndex, DS4Controls.L1, "L1", devType));
            mappings.Add(new MappedControl(devIndex, DS4Controls.R1, "R1", devType));
            mappings.Add(new MappedControl(devIndex, DS4Controls.L2, "L2", devType));
            mappings.Add(new MappedControl(devIndex, DS4Controls.R2, "R2", devType));
            mappings.Add(new MappedControl(devIndex, DS4Controls.L3, "L3", devType));
            mappings.Add(new MappedControl(devIndex, DS4Controls.R3, "R3", devType));
            mappings.Add(new MappedControl(devIndex, DS4Controls.TouchLeft, "Left Touch", devType));
            mappings.Add(new MappedControl(devIndex, DS4Controls.TouchRight, "Right Touch", devType));
            mappings.Add(new MappedControl(devIndex, DS4Controls.TouchMulti, "Multitouch", devType));
            mappings.Add(new MappedControl(devIndex, DS4Controls.TouchUpper, "Upper Touch", devType));

            mappings.Add(new MappedControl(devIndex, DS4Controls.LYNeg, "LS Up", devType));
            mappings.Add(new MappedControl(devIndex, DS4Controls.LYPos, "LS Down", devType));
            mappings.Add(new MappedControl(devIndex, DS4Controls.LXNeg, "LS Left", devType));
            mappings.Add(new MappedControl(devIndex, DS4Controls.LXPos, "LS Right", devType));

            mappings.Add(new MappedControl(devIndex, DS4Controls.RYNeg, "RS Up", devType));
            mappings.Add(new MappedControl(devIndex, DS4Controls.RYPos, "RS Down", devType));
            mappings.Add(new MappedControl(devIndex, DS4Controls.RXNeg, "RS Left", devType));
            mappings.Add(new MappedControl(devIndex, DS4Controls.RXPos, "RS Right", devType));

            mappings.Add(new MappedControl(devIndex, DS4Controls.GyroZNeg, "Tilt Up", devType));
            mappings.Add(new MappedControl(devIndex, DS4Controls.GyroZPos, "Tilt Down", devType));
            mappings.Add(new MappedControl(devIndex, DS4Controls.GyroXPos, "Tilt Left", devType));
            mappings.Add(new MappedControl(devIndex, DS4Controls.GyroXNeg, "Tilt Right", devType));

            mappings.Add(new MappedControl(devIndex, DS4Controls.SwipeUp, "Swipe Up", devType));
            mappings.Add(new MappedControl(devIndex, DS4Controls.SwipeDown, "Swipe Down", devType));
            mappings.Add(new MappedControl(devIndex, DS4Controls.SwipeLeft, "Swipe Left", devType));
            mappings.Add(new MappedControl(devIndex, DS4Controls.SwipeRight, "Swipe Right", devType));

            foreach (MappedControl mapped in mappings)
            {
                controlMap.Add(mapped.Control, mapped);
            }

            l2FullPullControl = new MappedControl(devIndex, DS4Controls.L2FullPull, "L2 Full Pull", devType);
            r2FullPullControl = new MappedControl(devIndex, DS4Controls.R2FullPull, "R2 Full Pull", devType);

            gyroSwipeLeftControl = new MappedControl(devIndex, DS4Controls.GyroSwipeLeft, "Gyro Swipe Left", devType);
            gyroSwipeRightControl = new MappedControl(devIndex, DS4Controls.GyroSwipeRight, "Gyro Swipe Right", devType);
            gyroSwipeUpControl = new MappedControl(devIndex, DS4Controls.GyroSwipeUp, "Gyro Swipe Up", devType);
            gyroSwipeDownControl = new MappedControl(devIndex, DS4Controls.GyroSwipeDown, "Gyro Swipe Down", devType);

            extraControls.Add(l2FullPullControl);
            extraControls.Add(r2FullPullControl);
            extraControls.Add(gyroSwipeLeftControl);
            extraControls.Add(gyroSwipeRightControl);
            extraControls.Add(gyroSwipeUpControl);
            extraControls.Add(gyroSwipeDownControl);

            controlMap.Add(DS4Controls.L2FullPull, l2FullPullControl);
            controlMap.Add(DS4Controls.R2FullPull, r2FullPullControl);
            controlMap.Add(DS4Controls.GyroSwipeLeft, gyroSwipeLeftControl);
            controlMap.Add(DS4Controls.GyroSwipeRight, gyroSwipeRightControl);
            controlMap.Add(DS4Controls.GyroSwipeUp, gyroSwipeUpControl);
            controlMap.Add(DS4Controls.GyroSwipeDown, gyroSwipeDownControl);
        }

        public void UpdateMappingDevType(OutContType devType)
        {
            foreach(MappedControl mapped in mappings)
            {
                mapped.DevType = devType;
            }

            foreach(MappedControl mapped in extraControls)
            {
                mapped.DevType = devType;
            }
        }

        public void UpdateMappings()
        {
            foreach (MappedControl mapped in mappings)
            {
                mapped.UpdateMappingName();
            }

            foreach (MappedControl mapped in extraControls)
            {
                mapped.UpdateMappingName();
            }
        }
    }

    public class MappedControl
    {
        private int devIndex;
        private OutContType devType;
        private DS4Controls control;
        private DS4ControlSettings setting;
        private string controlName;
        private string mappingName;
        private string shiftMappingName;

        public int DevIndex { get => devIndex; }
        public DS4Controls Control { get => control; }
        public DS4ControlSettings Setting { get => setting; }
        public string ControlName { get => controlName; }
        public string MappingName { get => mappingName; }
        public OutContType DevType
        {
            get => devType;
            set
            {
                devType = value;
                DevTypeChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public string ShiftMappingName { get => shiftMappingName; set => shiftMappingName = value; }

        public event EventHandler DevTypeChanged;

        public event EventHandler MappingNameChanged;

        public MappedControl(int devIndex, DS4Controls control, string controlName,
            OutContType devType, bool initMap=false)
        {
            this.devIndex = devIndex;
            this.devType = devType;
            this.control = control;
            this.controlName = controlName;
            setting = Global.GetDS4CSetting(devIndex, control);
            //mappingName = "?";
            if (initMap)
            {
                mappingName = GetMappingString();
                if (HasShiftAction())
                {
                    shiftMappingName = ShiftTrigger(setting.shiftTrigger) + " -> " + GetMappingString(true);
                }
            }
            
            DevTypeChanged += MappedControl_DevTypeChanged;
        }

        private void MappedControl_DevTypeChanged(object sender, EventArgs e)
        {
            UpdateMappingName();
        }

        public void UpdateMappingName()
        {
            mappingName = GetMappingString();
            if (HasShiftAction())
            {
                shiftMappingName = ShiftTrigger(setting.shiftTrigger) + " -> " + GetMappingString(true);
            }
            else
            {
                shiftMappingName = "";
            }

            MappingNameChanged?.Invoke(this, EventArgs.Empty);
        }

        public string GetMappingString(bool shift = false)
        {
            string temp = Properties.Resources.Unassigned;
            ControlActionData action = !shift ? setting.action : setting.shiftAction;
            bool sc = !shift ? setting.keyType.HasFlag(DS4KeyType.ScanCode) :
                setting.shiftKeyType.HasFlag(DS4KeyType.ScanCode);
            bool extra = control >= DS4Controls.GyroXPos && control <= DS4Controls.SwipeDown;
            DS4ControlSettings.ActionType actionType = !shift ? setting.actionType : setting.shiftActionType;
            if (actionType != DS4ControlSettings.ActionType.Default)
            {
                if (actionType == DS4ControlSettings.ActionType.Key)
                {
                    //return (Keys)int.Parse(action.ToString()) + (sc ? " (" + Properties.Resources.ScanCode + ")" : "");
                    temp = KeyInterop.KeyFromVirtualKey(action.actionKey) + (sc ? " (" + Properties.Resources.ScanCode + ")" : "");
                }
                else if (actionType == DS4ControlSettings.ActionType.Macro)
                {
                    temp = Properties.Resources.Macro + (sc ? " (" + Properties.Resources.ScanCode + ")" : "");
                }
                else if (actionType == DS4ControlSettings.ActionType.Button)
                {
                    string tag;
                    tag = Global.getX360ControlString((X360Controls)action.actionBtn, devType);
                    temp = tag;
                }
                else
                {
                    temp = Global.getX360ControlString(Global.defaultButtonMapping[(int)control], devType);
                }
            }
            else if (!extra && !shift)
            {
                X360Controls tempOutControl = Global.defaultButtonMapping[(int)control];
                if (tempOutControl != X360Controls.None)
                {
                    temp = Global.getX360ControlString(tempOutControl, devType);
                }
            }
            else if (shift)
                temp = "";

            return temp;
        }

        public bool HasShiftAction()
        {
            return setting.shiftActionType != DS4ControlSettings.ActionType.Default;
        }

        private static string ShiftTrigger(int trigger)
        {
            switch (trigger)
            {
                case 1: return "Cross";
                case 2: return "Circle";
                case 3: return "Square";
                case 4: return "Triangle";
                case 5: return "Options";
                case 6: return "Share";
                case 7: return "Dpad Up";
                case 8: return "Dpad Down";
                case 9: return "Dpad Left";
                case 10: return "Dpad Right";
                case 11: return "PS";
                case 12: return "L1";
                case 13: return "R1";
                case 14: return "L2";
                case 15: return "R2";
                case 16: return "L3";
                case 17: return "R3";
                case 18: return "Left Touch";
                case 19: return "Upper Touch";
                case 20: return "Multi Touch";
                case 21: return "Right Touch";
                case 22: return Properties.Resources.TiltUp;
                case 23: return Properties.Resources.TiltDown;
                case 24: return Properties.Resources.TiltLeft;
                case 25: return Properties.Resources.TiltRight;
                case 26: return "Finger on Touchpad";
                default: return "";
            }
        }
    }
}
