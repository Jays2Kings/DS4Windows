using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DS4WinWPF.DS4Control;

namespace DS4WinWPF.DS4Forms.ViewModels
{
    public class PermanentOutDevViewModel
    {
        private DS4Windows.OutputSlotManager outSlotManager;
        private List<PermanentSlotDeviceEntry> slotDeviceEntries;

        public List<PermanentSlotDeviceEntry> SlotDeviceEntries
        {
            get => slotDeviceEntries;
        }

        public PermanentOutDevViewModel(DS4Windows.ControlService controlService,
            DS4Windows.OutputSlotManager outputMan)
        {
            outSlotManager = outputMan;
            foreach(OutSlotDevice tempDev in outputMan.OutputSlots)
            {
                //slotDeviceEntries.Add(new PermanentSlotDeviceEntry(tempDev));
            }

            outSlotManager.SlotAssigned += OutSlotManager_SlotAssigned;
            outSlotManager.SlotUnassigned += OutSlotManager_SlotUnassigned;
        }

        private void OutSlotManager_SlotUnassigned(DS4Windows.OutputSlotManager sender,
            int slotNum, OutSlotDevice _)
        {
            slotDeviceEntries[slotNum].UpdateDevice();
        }

        private void OutSlotManager_SlotAssigned(DS4Windows.OutputSlotManager sender,
            int slotNum, OutSlotDevice _)
        {
            slotDeviceEntries[slotNum].UpdateDevice();
        }
    }

    public class PermanentSlotDeviceEntry
    {
        private OutSlotDevice slotDevice;
        public OutSlotDevice SlotDevice { get => slotDevice; }

        public PermanentSlotDeviceEntry(OutSlotDevice slotDevice)
        {
            this.slotDevice = slotDevice;
        }

        public void UpdateDevice()
        {

        }
    }
}
