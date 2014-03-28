using System;
using System.Threading;

namespace HidLibrary
{
    internal class HidDeviceEventMonitor
    {
        public event RemovedEventHandler Removed;

        public delegate void InsertedEventHandler();
        public delegate void RemovedEventHandler();

        private readonly HidDevice _device;

        public HidDeviceEventMonitor(HidDevice device)
        {
            _device = device;
        }

        public void Init()
        {
            var eventMonitor = new Action(DeviceEventMonitor);
            eventMonitor.BeginInvoke(DisposeDeviceEventMonitor, eventMonitor);
        }

        private void DeviceEventMonitor()
        {
            _device.Tick();
            if (_device.IsTimedOut && Removed != null)
                Removed();

            Thread.Sleep(500);
            if (_device.MonitorDeviceEvents) Init();
        }

        private static void DisposeDeviceEventMonitor(IAsyncResult ar)
        {
            ((Action)ar.AsyncState).EndInvoke(ar);
        }
    }
}
