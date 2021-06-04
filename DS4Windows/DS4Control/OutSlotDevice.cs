using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DS4Windows;

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
        private OutContType permanentType;
        private OutContType currentType;
        private int index;
        public int Index => index;

        /// <summary>
        /// Connection status of virtual output controller
        /// </summary>
        public AttachedStatus CurrentAttachedStatus { get => attachedStatus; }

        /// <summary>
        /// Reference to output controller
        /// </summary>
        public OutputDevice OutputDevice { get => outputDevice; }

        /// <summary>
        /// Flag stating the connection preference of an output controller
        /// </summary>
        public ReserveStatus CurrentReserveStatus
        {
            get => reserveStatus;
            set
            {
                if (reserveStatus == value) return;
                reserveStatus = value;
                CurrentReserveStatusChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler CurrentReserveStatusChanged;

        /// <summary>
        /// Whether an input controller is associated with the slot
        /// </summary>
        public InputBound CurrentInputBound
        {
            get => inputBound;
            set
            {
                if (inputBound == value) return;
                inputBound = value;
                CurrentInputBoundChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler CurrentInputBoundChanged;

        /// <summary>
        /// Desired device type for a permanently connected slot
        /// </summary>
        public OutContType PermanentType
        {
            get => permanentType;
            set
            {
                if (permanentType == value) return;

                if(value != OutContType.None)
                    AppLogger.LogToGui($"Output slot #{this.index+1} has permanent type {value}", false);

                permanentType = value;
                PermanentTypeChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler PermanentTypeChanged;

        /// <summary>
        /// Device type of the current output controller
        /// </summary>
        public OutContType CurrentType { get => currentType; set => currentType = value; }

        public OutSlotDevice(int idx)
        {
            this.index = idx;
            CurrentReserveStatusChanged += OutSlotDevice_CurrentReserveStatusChanged;
        }

        private void OutSlotDevice_CurrentReserveStatusChanged(object sender, EventArgs e)
        {
            if (reserveStatus == ReserveStatus.Dynamic)
            {
                PermanentType = OutContType.None;
            }
            else if (currentType != OutContType.None)
            {
                PermanentType = currentType;
            }
        }

        public void AttachedDevice(OutputDevice outputDevice, OutContType contType)
        {
            this.outputDevice = outputDevice;
            attachedStatus = AttachedStatus.Attached;
            currentType = contType;
            //desiredType = contType;
        }

        public void DetachDevice()
        {
            if (outputDevice != null)
            {
                outputDevice = null;
                attachedStatus = AttachedStatus.UnAttached;
                currentType = OutContType.None;
                CurrentInputBound = InputBound.Unbound;
                if (reserveStatus == ReserveStatus.Dynamic)
                {
                    PermanentType = OutContType.None;
                }
            }
        }

        ~OutSlotDevice()
        {
            DetachDevice();
        }
    }
}
