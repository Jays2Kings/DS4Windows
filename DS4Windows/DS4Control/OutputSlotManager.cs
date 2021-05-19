using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using DS4WinWPF.DS4Control;
using Nefarius.ViGEm.Client;

namespace DS4Windows
{
    public class OutputSlotManager
    {
        public const int DELAY_TIME = 500; // measured in ms
        private OutSlotDevice[] outputSlots;/* = new OutSlotDevice[Global.MAX_DS4_CONTROLLER_COUNT]
        {
            new OutSlotDevice(0), new OutSlotDevice(1),
            new OutSlotDevice(2), new OutSlotDevice(3)
        };
        */
        private int lastSlotIndex;

        public int NumAttachedDevices
        {
            get
            {
                int result = 0;
                for (int i = 0; i < outputSlots.Length; i++)
                {
                    OutSlotDevice tmp = outputSlots[i];
                    if (tmp.CurrentAttachedStatus == OutSlotDevice.AttachedStatus.Attached)
                    {
                        result++;
                    }
                }

                return result;
            }
        }

        private Dictionary<int, OutputDevice> deviceDict = new Dictionary<int, OutputDevice>();
        private Dictionary<OutputDevice, int> revDeviceDict = new Dictionary<OutputDevice, int>();
        private OutputDevice[] outputDevices = new OutputDevice[ControlService.CURRENT_DS4_CONTROLLER_LIMIT];

        private int queuedTasks = 0;
        private ReaderWriterLockSlim queueLocker;
        private Thread eventDispatchThread;
        private Dispatcher eventDispatcher;

        public bool RunningQueue { get => queuedTasks > 0; }
        public Dispatcher EventDispatcher { get => eventDispatcher; }
        public OutSlotDevice[] OutputSlots { get => outputSlots; }

        public delegate void SlotAssignedDelegate(OutputSlotManager sender,
            int slotNum, OutSlotDevice outSlotDev);
        public event SlotAssignedDelegate SlotAssigned;

        public delegate void SlotUnassignedDelegate(OutputSlotManager sender,
            int slotNum, OutSlotDevice outSlotDev);
        public event SlotUnassignedDelegate SlotUnassigned;

        public event EventHandler ViGEmFailure;

        public OutputSlotManager()
        {
            outputSlots = new OutSlotDevice[ControlService.CURRENT_DS4_CONTROLLER_LIMIT];
            for (int i = 0; i < ControlService.CURRENT_DS4_CONTROLLER_LIMIT; i++)
            {
                outputSlots[i] = new OutSlotDevice(i);
            }

            lastSlotIndex = outputSlots.Length > 0 ? outputSlots.Length - 1 : 0;

            queueLocker = new ReaderWriterLockSlim();

            eventDispatchThread = new Thread(() =>
            {
                Dispatcher currentDis = Dispatcher.CurrentDispatcher;
                eventDispatcher = currentDis;
                Dispatcher.Run();
            });

            eventDispatchThread.IsBackground = true;
            eventDispatchThread.Name = "OutputSlotManager Events";
            eventDispatchThread.Priority = ThreadPriority.Normal;
            eventDispatchThread.Start();
        }

        public void ShutDown()
        {
            eventDispatcher.InvokeShutdown();
            eventDispatcher = null;

            eventDispatchThread.Join();
            eventDispatchThread = null;
        }

        public void Stop()
        {
            UnplugRemainingControllers();
            deviceDict.Clear();
            revDeviceDict.Clear();
        }

        public OutputDevice AllocateController(OutContType contType, ViGEmClient client)
        {
            OutputDevice outputDevice = null;
            switch(contType)
            {
                case OutContType.X360:
                    outputDevice = new Xbox360OutDevice(client);
                    break;
                case OutContType.DS4:
                    outputDevice = DS4OutDeviceFactory.CreateDS4Device(client, Global.vigemBusVersionInfo);
                    break;
                case OutContType.None:
                default:
                    break;
            }

            return outputDevice;
        }

        private int FindEmptySlot()
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

        public void DeferredPlugin(OutputDevice outputDevice, int inIdx, OutputDevice[] outdevs, OutContType contType)
        {
            Action tempAction = new Action(() =>
            {
                int slot = FindEmptySlot();
                if (slot != -1)
                {
                    try
                    {
                        outputDevice.Connect();
                    }
                    catch (Win32Exception)
                    {
                        // Leave task immediately if connect call failed
                        ViGEmFailure?.Invoke(this, EventArgs.Empty);
                        return;
                    }

                    outputDevices[slot] = outputDevice;
                    deviceDict.Add(slot, outputDevice);
                    revDeviceDict.Add(outputDevice, slot);
                    outputSlots[slot].AttachedDevice(outputDevice, contType);
                    if (inIdx != -1)
                    {
                        outdevs[inIdx] = outputDevice;
                        outputSlots[slot].CurrentInputBound = OutSlotDevice.InputBound.Bound;
                    }
                    SlotAssigned?.Invoke(this, slot, outputSlots[slot]);

                    Task.Delay(DELAY_TIME).Wait();
                }
            });

            queueLocker.EnterWriteLock();
            queuedTasks++;
            queueLocker.ExitWriteLock();

            eventDispatcher.BeginInvoke((Action)(() =>
            {
                tempAction.Invoke();
                queueLocker.EnterWriteLock();
                queuedTasks--;
                queueLocker.ExitWriteLock();
            }));
        }

        public void DeferredRemoval(OutputDevice outputDevice, int inIdx,
            OutputDevice[] outdevs, bool immediate = false)
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
                    if (inIdx != -1)
                    {
                        outdevs[inIdx] = null;
                    }

                    outputSlots[slot].DetachDevice();
                    SlotUnassigned?.Invoke(this, slot, outputSlots[slot]);

                    if (!immediate)
                    {
                        Task.Delay(DELAY_TIME).Wait();
                    }
                }
            });

            queueLocker.EnterWriteLock();
            queuedTasks++;
            queueLocker.ExitWriteLock();

            eventDispatcher.BeginInvoke((Action)(() =>
            {
                tempAction.Invoke();
                queueLocker.EnterWriteLock();
                queuedTasks--;
                queueLocker.ExitWriteLock();
            }));
        }

        public OutSlotDevice FindOpenSlot()
        {
            OutSlotDevice temp = null;
            for (int i = 0; i < outputSlots.Length; i++)
            {
                OutSlotDevice tmp = outputSlots[i];
                if (tmp.CurrentInputBound == OutSlotDevice.InputBound.Unbound &&
                    tmp.CurrentAttachedStatus == OutSlotDevice.AttachedStatus.UnAttached)
                {
                    temp = tmp;
                    break;
                }
            }

            return temp;
        }

        public bool SlotAvailable(int slotNum)
        {
            bool result;
            if (slotNum < 0 && slotNum > lastSlotIndex)
            {
                throw new ArgumentOutOfRangeException("Invalid slot number");
            }

            //slotNum -= 1;
            result = outputSlots[slotNum].CurrentAttachedStatus == OutSlotDevice.AttachedStatus.UnAttached;
            return result;
        }

        public OutSlotDevice GetOutSlotDevice(int slotNum)
        {
            OutSlotDevice temp;
            if (slotNum < 0 && slotNum > lastSlotIndex)
            {
                throw new ArgumentOutOfRangeException("Invalid slot number");
            }

            //slotNum -= 1;
            temp = outputSlots[slotNum];
            return temp;
        }

        public OutSlotDevice GetOutSlotDevice(OutputDevice outputDevice)
        {
            OutSlotDevice temp = null;
            if (outputDevice != null &&
                revDeviceDict.TryGetValue(outputDevice, out int slotNum))
            {
                temp = outputSlots[slotNum];
            }

            return temp;
        }

        public OutSlotDevice FindExistUnboundSlotType(OutContType contType)
        {
            OutSlotDevice temp = null;
            string devtype = contType.ToString();
            for (int i = 0; i < outputSlots.Length; i++)
            {
                OutSlotDevice tmp = outputSlots[i];
                if (tmp.CurrentInputBound == OutSlotDevice.InputBound.Unbound &&
                    (tmp.CurrentAttachedStatus == OutSlotDevice.AttachedStatus.Attached &&
                    (tmp.OutputDevice != null && tmp.OutputDevice.GetDeviceType() == devtype)))
                {
                    temp = tmp;
                    break;
                }
            }

            return temp;
        }

        public void UnplugRemainingControllers()
        {
            Action tempAction = new Action(() =>
            {
                foreach (OutSlotDevice device in outputSlots)
                {
                    if (device.CurrentAttachedStatus ==
                        OutSlotDevice.AttachedStatus.Attached)
                    {
                        device.DetachDevice();
                        Task.Delay(DELAY_TIME).Wait();
                    }
                }
            });

            queueLocker.EnterWriteLock();
            queuedTasks++;
            queueLocker.ExitWriteLock();

            eventDispatcher.BeginInvoke((Action)(() =>
            {
                tempAction.Invoke();
                queueLocker.EnterWriteLock();
                queuedTasks--;
                queueLocker.ExitWriteLock();
            }));
        }
    }
}
