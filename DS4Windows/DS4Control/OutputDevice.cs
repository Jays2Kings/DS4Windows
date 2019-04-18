using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS4Windows
{
    public abstract class OutputDevice
    {
        public abstract void ConvertandSendReport(DS4State state, int device);
        public abstract void Connect();
        public abstract void Disconnect();
    }
}
