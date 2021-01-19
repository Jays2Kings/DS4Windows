using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nefarius.ViGEm.Client;
using Nefarius.ViGEm.Client.Targets;
using Nefarius.ViGEm.Client.Targets.DualShock4;

namespace DS4Windows
{
    abstract class DS4OutDevice : OutputDevice
    {
        public const string devtype = "DS4";

        public IDualShock4Controller cont;
        public DualShock4FeedbackReceivedEventHandler forceFeedbackCall;

        public DS4OutDevice(ViGEmClient client)
        {
            cont = client.CreateDualShock4Controller();
            //cont = client.CreateDualShock4Controller(0x054C, 0x09CC);
            cont.AutoSubmitReport = false;
        }

        public override void Connect()
        {
            cont.Connect();
            connected = true;
        }
        public override void Disconnect()
        {
            if (forceFeedbackCall != null)
            {
                cont.FeedbackReceived -= forceFeedbackCall;
                forceFeedbackCall = null;
            }

            connected = false;
            cont.Disconnect();
            //cont.Dispose();
            cont = null;
        }
        public override string GetDeviceType() => devtype;
    }
}
