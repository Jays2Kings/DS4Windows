using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using Nefarius.ViGEm.Client;

namespace DS4Windows
{
    public class OutputSlotManager
    {
        public const int DELAY_TIME = 500; // measured in ms

        private Dictionary<int, OutputDevice> deviceDict = new Dictionary<int, OutputDevice>();
        private Dictionary<OutputDevice, int> revDeviceDict = new Dictionary<OutputDevice, int>();
        private OutputDevice[] outputDevices = new OutputDevice[4];
        //private Queue<Action> actions = new Queue<Action>();
        private int queuedTasks = 0;
        private ReaderWriterLockSlim queueLocker;
        private Thread eventDispatchThread;
        private Dispatcher eventDispatcher;

        public bool RunningQueue { get => queuedTasks > 0; }
        public Dispatcher EventDispatcher { get => eventDispatcher; }

        public OutputSlotManager()
        {
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
