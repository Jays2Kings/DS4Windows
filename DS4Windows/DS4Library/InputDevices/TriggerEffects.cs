using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS4Windows.InputDevices
{
    public enum TriggerEffects : ushort
    {
        None,
        Rigid,
        Pulse,
        FullClick,
    }

    public enum TriggerId : ushort
    {
        Trigger0,
        LeftTrigger = Trigger0,
        Trigger1,
        RightTrigger = Trigger1,
    }
}
