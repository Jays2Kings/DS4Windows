using System;

namespace DS4Windows
{
    public abstract class OutputDevice
    {
        protected bool connected;

        public abstract void ConvertandSendReport(DS4State state, int device);
        public abstract void Connect();
        public abstract void Disconnect();
        public abstract void ResetState(bool submit=true);
        public abstract string GetDeviceType();
    }
}
