﻿using System;
using System.Collections.Generic;

namespace DS4Windows
{
    public class ControllerSlotManager
    {
        private List<DS4Device> controllerColl;
        public List<DS4Device> ControllerColl { get => controllerColl; set => controllerColl = value; }
        
        private Dictionary<int, DS4Device> controllerDict;
        private Dictionary<DS4Device, int> reverseControllerDict;
        public Dictionary<int, DS4Device> ControllerDict { get => controllerDict; }
        public Dictionary<DS4Device, int> ReverseControllerDict { get => reverseControllerDict; }

        public ControllerSlotManager()
        {
            controllerColl = new List<DS4Device>();
            controllerDict = new Dictionary<int, DS4Device>();
            reverseControllerDict = new Dictionary<DS4Device, int>();
        }

        public void AddController(DS4Device device, int slotIdx)
        {
            controllerColl.Add(device);
            controllerDict.Add(slotIdx, device);
            reverseControllerDict.Add(device, slotIdx);
        }

        public void RemoveController(DS4Device device, int slotIdx)
        {
            controllerColl.Remove(device);
            controllerDict.Remove(slotIdx);
            reverseControllerDict.Remove(device);
        }

        public void ClearControllerList()
        {
            controllerColl.Clear();
            controllerDict.Clear();
            reverseControllerDict.Clear();
        }
    }
}
