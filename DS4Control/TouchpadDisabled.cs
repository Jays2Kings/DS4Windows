using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DS4Library;

namespace DS4Control
{
    class TouchpadDisabled : ITouchpadBehaviour
    {
        public override string ToString()
        {
            return "Disabled";
        }

        public static readonly TouchpadDisabled singleton = new TouchpadDisabled();

        public void touchesMoved(object sender, TouchpadEventArgs arg) { }

        public void touchesBegan(object sender, TouchpadEventArgs arg) { }

        public void touchesEnded(object sender, TouchpadEventArgs arg) { }

        public void touchButtonUp(object sender, TouchpadEventArgs arg) { }

        public void touchButtonDown(object sender, TouchpadEventArgs arg) { }

        public void untouched(object sender, TouchpadEventArgs nullUnused) { }
    }
}
