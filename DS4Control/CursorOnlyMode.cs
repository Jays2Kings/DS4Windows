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
        private readonly MouseCursor cursor;
        private readonly MouseWheel wheel;

        public MouseCursorOnly(int deviceID)
        {
            deviceNum = deviceID;
            cursor = new MouseCursor(deviceNum);
            wheel = new MouseWheel(deviceNum);
        }

        public override string ToString()
        {
            return "Cursor Mode";
        }

        public void touchesMoved(object sender, TouchpadEventArgs arg)
        {
            cursor.touchesMoved(arg);
            wheel.touchesMoved(arg);
        }

        public void touchesBegan(object sender, TouchpadEventArgs arg)
        {
            cursor.touchesBegan(arg);
            wheel.touchesBegan(arg);
        }

        public void touchesEnded(object sender, TouchpadEventArgs arg) { }

        public void touchButtonUp(object sender, TouchpadEventArgs arg) { }

        public void touchButtonDown(object sender, TouchpadEventArgs arg) { }

        public void touchUnchanged(object sender, EventArgs unused) { }
    }
}
