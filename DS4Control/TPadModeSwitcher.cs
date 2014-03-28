using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DS4Library;
namespace DS4Control
{
    class TPadModeSwitcher
    {
        List<ITouchpadBehaviour> modes = new List<ITouchpadBehaviour>();
        public event EventHandler<DebugEventArgs> Debug = null;
        private DS4Device device;
        Int32 currentTypeInd = 0;
        public TPadModeSwitcher(DS4Device device, int deviceID)
        {
            this.device = device;
            modes.Add(TouchpadDisabled.singleton);
            modes.Add(new Mouse(deviceID));
            modes.Add(new ButtonMouse(deviceID, device));
            modes.Add(new MouseCursorOnly(deviceID));
            modes.Add(new DragMouse(deviceID));
        }

        public void switchMode(int ind)
        {
            ITouchpadBehaviour currentMode = modes.ElementAt(currentTypeInd);
            device.Touchpad.TouchButtonDown -= currentMode.touchButtonDown;
            device.Touchpad.TouchButtonUp -= currentMode.touchButtonUp;
            device.Touchpad.TouchesBegan -= currentMode.touchesBegan;
            device.Touchpad.TouchesMoved -= currentMode.touchesMoved;
            device.Touchpad.TouchesEnded -= currentMode.touchesEnded;
            setMode(ind);
        }
        
        public void setMode(int ind)
        {
            ITouchpadBehaviour tmode = modes.ElementAt(ind);
            device.Touchpad.TouchButtonDown += tmode.touchButtonDown;
            device.Touchpad.TouchButtonUp += tmode.touchButtonUp;
            device.Touchpad.TouchesBegan += tmode.touchesBegan;
            device.Touchpad.TouchesMoved += tmode.touchesMoved;
            device.Touchpad.TouchesEnded += tmode.touchesEnded;
            currentTypeInd = ind;
            LogDebug("Touchpad mode for " + device.MacAddress + " is now " + tmode.ToString());
            Log.LogToTray("Touchpad mode for " + device.MacAddress + " is now " + tmode.ToString());
        }

        public override string ToString()
        {
            return modes.ElementAt(currentTypeInd).ToString();
        }

        public void previousMode()
        {
            int i = currentTypeInd - 1;
            if (i == -1)
                i = modes.Count - 1;
            switchMode(i);
        }

        public void nextMode()
        {
            int i = currentTypeInd + 1;
            if (i == modes.Count)
                i = 0;
            switchMode(i);
        }

        private void LogDebug(string data)
        {
            if (Debug != null)
                Debug(this, new DebugEventArgs(data));
        }

        public ITouchpadBehaviour getCurrentMode()
        {
            return modes.ElementAt(currentTypeInd);
        }
    }
}
