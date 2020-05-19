using DS4Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS4WinWPF.DS4Control
{
    public class OutSlotDevice
    {
        public enum AttachedStatus : uint
        {
            UnAttached = 0,
            Attached = 1,
        }

        public enum ReserveStatus : uint
        {
            Dynamic = 0,
            Permanent = 1,
        }

        public enum InputBound : uint
        {
            Unbound = 0,
            Bound = 1,
        }

        private AttachedStatus attachedStatus;
        private OutputDevice outputDevice;
        private ReserveStatus reserveStatus;
        private InputBound inputBound;

        public AttachedStatus CurrentAttachedStatus { get => attachedStatus; }
        public OutputDevice OutputDevice { get => outputDevice; }
        public ReserveStatus CurrentReserveStatus
        {
            get => reserveStatus; set => reserveStatus = value;
        }

        public InputBound CurrentInputBound
        {
            get => inputBound; set => inputBound = value;
        }

        public void AttachedDevice(OutputDevice outputDevice)
        {
            this.outputDevice = outputDevice;
            attachedStatus = AttachedStatus.Attached;
        }

        public void DetachDevice()
        {
            if (outputDevice != null)
            {
                outputDevice = null;
                attachedStatus = AttachedStatus.UnAttached;
            }
        }

        ~OutSlotDevice()
        {
            DetachDevice();
        }
    }
}
