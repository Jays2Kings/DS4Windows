using System;

namespace DS4Windows
{
    interface ITouchpadBehaviour
    {
        void touchesBegan(DS4Touchpad sender, TouchpadEventArgs arg);
        void touchesMoved(DS4Touchpad sender, TouchpadEventArgs arg);
        void touchButtonUp(DS4Touchpad sender, TouchpadEventArgs arg);
        void touchButtonDown(DS4Touchpad sender, TouchpadEventArgs arg);
        void touchesEnded(DS4Touchpad sender, TouchpadEventArgs arg);
        void sixaxisMoved(DS4SixAxis sender, SixAxisEventArgs unused);
        void touchUnchanged(DS4Touchpad sender, EventArgs unused);
    }
}
