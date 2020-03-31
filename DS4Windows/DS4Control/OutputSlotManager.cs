using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nefarius.ViGEm.Client;

namespace DS4Windows
{
    public class OutputSlotManager
    {
        public const int DELAY_TIME = 500; // measured in ms

        private Dictionary<int, OutputDevice> deviceDict = new Dictionary<int, OutputDevice>();
        private Dictionary<OutputDevice, int> revDeviceDict = new Dictionary<OutputDevice, int>();
        private OutputDevice[] outputDevices = new OutputDevice[4];
        private Queue<Action> actions = new Queue<Action>();
        private object actionLock = new object();
        private bool runningQueue;

        public bool RunningQueue { get => runningQueue; }

        public OutputDevice AllocateController(OutContType contType, ViGEmClient client)
        {
            OutputDevice outputDevice = null;
            switch(contType)
            {
                case OutContType.X360:
                    outputDevice = new Xbox360OutDevice(client);
                    break;
                case OutContType.DS4:
                    outputDevice = new DS4OutDevice(client);
                    break;
                case OutContType.None:
                default:
                    break;
            }

            return outputDevice;
        }

        private int FindSlot()
        {
            int result = -1;
            for (int i = 0; i < outputDevices.Length && result == -1; i++)
            {
                OutputDevice tempdev = outputDevices[i];
                if (tempdev == null)
                {
                    result = i;
                }
            }

            return result;
        }

        private void LaunchEvents()
        {
            bool hasItems = false;
            Action act = null;
            lock (actionLock)
            {
                hasItems = actions.Count > 0;
            }

            while (hasItems)
            {
                lock (actionLock)
                {
                    act = actions.Dequeue();
                }

                act.Invoke();

                lock (actionLock)
                {
                    hasItems = actions.Count > 0;
                }
            }
        }

        private void PrepareEventTask()
        {
            if (!runningQueue)
            {
                runningQueue = true;
                Task.Run(() =>
                {
                    LaunchEvents();
                    runningQueue = false;
                });
            }
        }

        public void DeferredPlugin(OutputDevice outputDevice, int inIdx, OutputDevice[] outdevs)
        {
            Action tempAction = new Action(() =>
            {
                int slot = FindSlot();
                if (slot != -1)
                {
                    outputDevice.Connect();
                    outputDevices[slot] = outputDevice;
                    deviceDict.Add(slot, outputDevice);
                    revDeviceDict.Add(outputDevice, slot);
                    Task.Delay(DELAY_TIME).Wait();
                    outdevs[inIdx] = outputDevice;
                }
            });

            lock (actionLock)
            {
                actions.Enqueue(tempAction);
            }

            PrepareEventTask();
        }

        public void DeferredRemoval(OutputDevice outputDevice, int inIdx, OutputDevice[] outdevs, bool immediate = false)
        {
            Action tempAction = new Action(() =>
            {
                if (revDeviceDict.ContainsKey(outputDevice))
                {
                    int slot = revDeviceDict[outputDevice];
                    outputDevices[slot] = null;
                    deviceDict.Remove(slot);
                    revDeviceDict.Remove(outputDevice);
                    outputDevice.Disconnect();
                    outdevs[inIdx] = null;
                    if (!immediate)
                    {
                        Task.Delay(DELAY_TIME).Wait();
                    }
                }
            });

            lock (actionLock)
            {
                actions.Enqueue(tempAction);
            }

            PrepareEventTask();
        }
    }
}
