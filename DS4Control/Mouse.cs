using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using DS4Library;
namespace DS4Control
{
    public class Mouse : ITouchpadBehaviour
    {
        protected DateTime pastTime;
        protected Touch firstTouch;
        protected int deviceNum;
        private readonly MouseCursor cursor;
        private readonly MouseWheel wheel;
        protected bool rightClick = false;

        public Mouse(int deviceID)
        {
            deviceNum = deviceID;
            cursor = new MouseCursor(deviceNum);
            wheel = new MouseWheel(deviceNum);
        }

        public override string ToString()
        {
            return "Standard Mode";
        }

        public virtual void touchesMoved(object sender, TouchpadEventArgs arg)
        {
            cursor.touchesMoved(arg);
            wheel.touchesMoved(arg);
            //Log.LogToGui("moved to " + arg.touches[0].hwX + "," + arg.touches[0].hwY);
        }

        public virtual void touchesBegan(object sender, TouchpadEventArgs arg)
        {
            cursor.touchesBegan(arg);
            wheel.touchesBegan(arg);
            pastTime = arg.timeStamp;
            firstTouch = arg.touches[0];
            //Log.LogToGui("began at " + arg.touches[0].hwX + "," + arg.touches[0].hwY);
        }

        public virtual void touchesEnded(object sender, TouchpadEventArgs arg)
        {
            //Log.LogToGui("ended at " + arg.touches[0].hwX + "," + arg.touches[0].hwY);
            if (Global.getTapSensitivity(deviceNum) != 0)
            {
                DateTime test = arg.timeStamp;
                if (test <= (pastTime + TimeSpan.FromMilliseconds((double)Global.getTapSensitivity(deviceNum) * 2)) && !arg.touchButtonPressed)
                {
                    if (Math.Abs(firstTouch.hwX - arg.touches[0].hwX) < 10 &&
                        Math.Abs(firstTouch.hwY - arg.touches[0].hwY) < 10)
                        InputMethods.performLeftClick();
                }
            }
        }

        public virtual void touchButtonUp(object sender, TouchpadEventArgs arg)
        {
            if (arg.touches == null)
            {
                //No touches, finger on upper portion of touchpad
                mapTouchPad(DS4Controls.TouchUpper,true);
            }
            else if (arg.touches.Length > 1)
                mapTouchPad(DS4Controls.TouchMulti, true);
            else if (!rightClick && arg.touches.Length == 1 && !mapTouchPad(DS4Controls.TouchButton, true))
            {
                InputMethods.MouseEvent(InputMethods.MOUSEEVENTF_LEFTUP);
            }
        }

        public virtual void touchButtonDown(object sender, TouchpadEventArgs arg)
        {
            if (arg.touches == null)
            {
                //No touches, finger on upper portion of touchpad
                if(!mapTouchPad(DS4Controls.TouchUpper))
                    InputMethods.performMiddleClick();
            }
            else if (!Global.getLowerRCOff(deviceNum) && arg.touches[0].hwX > (1920 * 3)/4
                && arg.touches[0].hwY > (960 * 3)/4)
            {
                rightClick = true;
                InputMethods.performRightClick();
            }
            else if (arg.touches.Length>1 && !mapTouchPad(DS4Controls.TouchMulti))
            {
                rightClick = true;
                InputMethods.performRightClick();
            }
            else if (arg.touches.Length==1 && !mapTouchPad(DS4Controls.TouchButton))
            {
                rightClick = false;
                InputMethods.MouseEvent(InputMethods.MOUSEEVENTF_LEFTDOWN);
            }
        }

        public void touchUnchanged(object sender, EventArgs unused) { }

        protected bool mapTouchPad(DS4Controls padControl, bool release = false)
        {
            ushort key = Global.getCustomKey(padControl);
            if (key == 0)
                return false;
            else
            {
                DS4KeyType keyType = Global.getCustomKeyType(padControl);
                if (!release)
                    if (keyType.HasFlag(DS4KeyType.ScanCode))
                        InputMethods.performSCKeyPress(key);
                    else InputMethods.performKeyPress(key);
                else
                    if (!keyType.HasFlag(DS4KeyType.Repeat))
                        if (keyType.HasFlag(DS4KeyType.ScanCode))
                            InputMethods.performSCKeyRelease(key);
                        else InputMethods.performKeyRelease(key);
                return true;
            }
        }

    }
}
