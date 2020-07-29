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

        public abstract bool Connect();
        public abstract bool Disconnect();

        public abstract void MoveRelativeMouse(int x, int y);

        public void MoveAbsoluteMouse(int x, int y, int screen)
        {

        }

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

        public abstract string GetDisplayName();
        public abstract string GetIdentifier();
    }
}
