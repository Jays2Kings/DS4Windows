using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using DS4Library;
namespace DS4Control
{
    public class MouseCursorOnly : ITouchpadBehaviour
    {
        private int deviceNum;
        public MouseCursorOnly(int deviceID)
        {
            deviceNum = deviceID;
        }

        public override string ToString()
        {
            return "Cursor Mode";
        }

        public void touchesMoved(object sender, TouchpadEventArgs arg)
        {
            if (arg.touches.Length == 1)
            {
                double sensitivity = Global.getTouchSensitivity(deviceNum) / 100.0;
                int mouseDeltaX = (int)(sensitivity * (arg.touches[0].deltaX));
                int mouseDeltaY = (int)(sensitivity * (arg.touches[0].deltaY));
                InputMethods.MoveCursorBy(mouseDeltaX, mouseDeltaY);
            }
        }

        public void touchesBegan(object sender, TouchpadEventArgs arg) { }

        public void touchesEnded(object sender, TouchpadEventArgs arg) { }

        public void touchButtonUp(object sender, TouchpadEventArgs arg) { }

        public void touchButtonDown(object sender, TouchpadEventArgs arg) { }

        public void untouched(object sender, TouchpadEventArgs nullUnused) { }
    }
}
