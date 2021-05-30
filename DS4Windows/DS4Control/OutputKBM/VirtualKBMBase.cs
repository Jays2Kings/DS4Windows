using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS4Windows.DS4Control
{
    public abstract class VirtualKBMBase
    {
        protected string errorMessage = string.Empty;
        public string ErrorMessage { get => errorMessage; }

        public bool fakeKeyRepeat = false;

        public Version version = new Version("0.0.0.0");
        public string Version
        {
            get => version.ToString();
            set => version = new Version(value);
        }

        public abstract bool Connect();
        public abstract bool Disconnect();

        public abstract void MoveRelativeMouse(int x, int y);

        public abstract void MoveAbsoluteMouse(double x, double y);

        public abstract void PerformMouseWheelEvent(int vertical, int horizontal);
        public abstract void PerformMouseButtonEvent(uint mouseButton);
        public virtual void PerformMouseButtonEventAlt(uint mouseButton, int type)
        {

        }

        public abstract void PerformMouseButtonPress(uint mouseButton);
        public abstract void PerformMouseButtonRelease(uint mouseButton);

        public abstract void PerformKeyPress(uint key);
        public abstract void PerformKeyPressAlt(uint key);
        public abstract void PerformKeyRelease(uint key);
        public abstract void PerformKeyReleaseAlt(uint key);

        public virtual void Sync()
        {
        }

        public abstract string GetDisplayName();
        public abstract string GetIdentifier();

        public abstract string GetFullDisplayName();
    }
}
