using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DS4Windows;
using DS4WinWPF.DS4Control;

namespace DS4WinWPF.DS4Forms.ViewModels
{
    public class CurrentOutDeviceViewModel
    {
        private int selectedIndex = 0;
        public int SelectedIndex
        {
            get => selectedIndex;
            set => selectedIndex = value;
        }

        private DS4Windows.OutputSlotManager outSlotManager;
        private List<SlotDeviceEntry> slotDeviceEntries;

        public List<SlotDeviceEntry> SlotDeviceEntries { get => slotDeviceEntries; }

        private ControlService controlService;

        public CurrentOutDeviceViewModel(ControlService controlService,
            OutputSlotManager outputMan)
        {
            outSlotManager = outputMan;
            slotDeviceEntries = new List<SlotDeviceEntry>(4);
            foreach(OutSlotDevice tempDev in outputMan.OutputSlots)
            {
                SlotDeviceEntry tempEntry = new SlotDeviceEntry(tempDev);
                tempEntry.PluginRequest += OutSlot_PluginRequest;
                tempEntry.UnplugRequest += OutSlot_UnplugRequest;
                slotDeviceEntries.Add(tempEntry);

            }

            this.controlService = controlService;

            outSlotManager.SlotAssigned += OutSlotManager_SlotAssigned;
            outSlotManager.SlotUnassigned += OutSlotManager_SlotUnassigned;
        }

        private void OutSlot_UnplugRequest(object sender, EventArgs e)
        {
            SlotDeviceEntry entry = sender as SlotDeviceEntry;
            controlService.EventDispatcher.BeginInvoke((Action)(() =>
            {
                controlService.DetachUnboundOutDev(entry.OutSlotDevice);
            }));
        }

        private void OutSlot_PluginRequest(object sender, EventArgs e)
        {
            SlotDeviceEntry entry = sender as SlotDeviceEntry;
            controlService.EventDispatcher.BeginInvoke((Action)(() =>
            {
                controlService.AttachUnboundOutDev(entry.OutSlotDevice, entry.OutSlotDevice.CurrentType);
            }));
        }

        private void OutSlotManager_SlotUnassigned(OutputSlotManager sender,
            int slotNum, OutSlotDevice _)
        {
            slotDeviceEntries[slotNum].RemovedDevice();
        }

        private void OutSlotManager_SlotAssigned(OutputSlotManager sender,
            int slotNum, OutSlotDevice _)
        {
            slotDeviceEntries[slotNum].AssignedDevice();
        }
    }

    public class SlotDeviceEntry
    {
        private OutSlotDevice outSlotDevice;
        public OutSlotDevice OutSlotDevice { get => outSlotDevice; }

        public string CurrentType
        {
            get
            {
                string temp = "Empty";
                if (outSlotDevice.OutputDevice != null)
                {
                    temp = outSlotDevice.OutputDevice.GetDeviceType();
                }

                return temp;
            }
        }
        public event EventHandler CurrentTypeChanged;

        public string DesiredType
        {
            get
            {
                string temp = "Dynamic";
                if (outSlotDevice.CurrentReserveStatus ==
                    OutSlotDevice.ReserveStatus.Permanent)
                {
                    temp = outSlotDevice.DesiredType.ToString();
                }

                return temp;
            }
        }
        public event EventHandler DesiredTypeChanged;

        public event EventHandler PluginRequest;
        public event EventHandler UnplugRequest;

        public SlotDeviceEntry(OutSlotDevice outSlotDevice)
        {
            this.outSlotDevice = outSlotDevice;
        }

        public void AssignedDevice()
        {
            Refresh();
        }

        public void RemovedDevice()
        {
            Refresh();
        }

        private void Refresh()
        {
            CurrentTypeChanged?.Invoke(this, EventArgs.Empty);
            DesiredTypeChanged?.Invoke(this, EventArgs.Empty);
        }

        public void RequestPlugin()
        {
            PluginRequest?.Invoke(this, EventArgs.Empty);
        }

        public void RequestUnplug()
        {
            UnplugRequest?.Invoke(this, EventArgs.Empty);
        }
    }
}
