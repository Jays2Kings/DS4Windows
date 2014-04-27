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
        private DS4State s = new DS4State();
        protected int deviceNum;
        private DS4Device dev = null;
        private readonly MouseCursor cursor;
        private readonly MouseWheel wheel;

        public Mouse(int deviceID, DS4Device d)
        {
            deviceNum = deviceID;
            dev = d;
            cursor = new MouseCursor(deviceNum);
            wheel = new MouseWheel(deviceNum);
        }

        public override string ToString()
        {
            return "Standard Mode";
        }

        protected virtual void MapClicks()
        {
            if (pushed != DS4Controls.None)
                Mapping.MapTouchpadButton(deviceNum, pushed, clicked);
        }

        public virtual void touchesMoved(object sender, TouchpadEventArgs arg)
        {
            cursor.touchesMoved(arg);
            wheel.touchesMoved(arg);
            //MapClicks();
            dev.getCurrentState(s);
            synthesizeMouseButtons();
            //Console.WriteLine(arg.timeStamp.ToString("O") + " " + "moved to " + arg.touches[0].hwX + "," + arg.touches[0].hwY);
        }

        public virtual void touchesBegan(object sender, TouchpadEventArgs arg)
        {
            cursor.touchesBegan(arg);
            wheel.touchesBegan(arg);
            pastTime = arg.timeStamp;
            firstTouch = arg.touches[0];
            dev.getCurrentState(s);
            synthesizeMouseButtons();
            //MapClicks();
            //Console.WriteLine(arg.timeStamp.ToString("O") + " " + "began at " + arg.touches[0].hwX + "," + arg.touches[0].hwY);
        }

        public virtual void touchesEnded(object sender, TouchpadEventArgs arg)
        {
            //Console.WriteLine(arg.timeStamp.ToString("O") + " " + "ended at " + arg.touches[0].hwX + "," + arg.touches[0].hwY);
            if (Global.getTapSensitivity(deviceNum) != 0)
            {
                DateTime test = arg.timeStamp;
                if (test <= (pastTime + TimeSpan.FromMilliseconds((double)Global.getTapSensitivity(deviceNum) * 2)) && !arg.touchButtonPressed)
                {
                    if (Math.Abs(firstTouch.hwX - arg.touches[0].hwX) < 10 && Math.Abs(firstTouch.hwY - arg.touches[0].hwY) < 10)
                       Mapping.MapClick(deviceNum, Mapping.Click.Left);
                }
            }
            dev.getCurrentState(s);
            //if (buttonLock)
            synthesizeMouseButtons();
            //MapClicks();
        }

        protected DS4Controls pushed = DS4Controls.None;
        protected Mapping.Click clicked = Mapping.Click.None;

        // touch area stuff
        public bool leftDown, rightDown, upperDown, multiDown, lowerRDown;
        private bool isLeft(Touch t)
        {
            return t.hwX < 1920 * 2 / 5;
        }

        private bool isRight(Touch t)
        {
            return t.hwX >= 1920 * 2 / 5;
        }

        public virtual void touchUnchanged(object sender, EventArgs unused)
        {
            //MapClicks();
            dev.getCurrentState(s);
            if (s.Touch1 || s.Touch2 || s.TouchButton)
                synthesizeMouseButtons();
        }

        private DS4State remapped = new DS4State();
        private void synthesizeMouseButtons()
        {
            //Mapping.MapCustom(deviceNum, s, remapped);
            if (leftDown)
                Mapping.MapTouchpadButton(deviceNum, DS4Controls.TouchLeft, Mapping.Click.Left, remapped);
            if (upperDown)
                Mapping.MapTouchpadButton(deviceNum, DS4Controls.TouchUpper, Mapping.Click.Middle, remapped);
            if (rightDown)
                Mapping.MapTouchpadButton(deviceNum, DS4Controls.TouchRight, Mapping.Click.Left, remapped);
            if (multiDown)
                Mapping.MapTouchpadButton(deviceNum, DS4Controls.TouchMulti, Mapping.Click.Right, remapped);
            if (lowerRDown)
                Mapping.MapClick(deviceNum, Mapping.Click.Right);
            s = remapped;
            //remapped.CopyTo(s);
        }

        public virtual void touchButtonUp(object sender, TouchpadEventArgs arg)
        {
            pushed = DS4Controls.None;
            upperDown = leftDown = rightDown = multiDown = false;
            dev.setRumble(0, 0);
            dev.getCurrentState(s);
            if (s.Touch1 || s.Touch2)
                synthesizeMouseButtons();
        }

        public virtual void touchButtonDown(object sender, TouchpadEventArgs arg)
        {
            //byte leftRumble, rightRumble;
            if (arg.touches == null)
            {
                //No touches, finger on upper portion of touchpad
                //leftRumble = rightRumble = 0;
                upperDown = true;
            }
            else if (arg.touches.Length > 1 )//|| (Global.getLowerRCOn(deviceNum) && arg.touches[0].hwX > (1920 * 3) / 4 && arg.touches[0].hwY > (960 * 3) / 4))
            {
                //leftRumble = rightRumble = 150;
                multiDown = true;
            }
            else
            {
                if ((Global.getLowerRCOn(deviceNum) && arg.touches[0].hwX > (1920 * 3) / 4 && arg.touches[0].hwY > (960 * 3) / 4))
                    Mapping.MapClick(deviceNum, Mapping.Click.Right);
                if (isLeft(arg.touches[0]))
                {
                    leftDown = true;
                    //leftRumble = 25;
                    //rightRumble = 0;
                }
                else if (isRight(arg.touches[0]))
                {
                    rightDown = true;
                    //leftRumble = 0;
                    //rightRumble = 25;
                }
                else
                {
                    //leftRumble = rightRumble = 0; // Ignore ambiguous pushes.
                }
            }
            //dev.setRumble(rightRumble, leftRumble); // sustain while pressed
            dev.getCurrentState(s);
            synthesizeMouseButtons();
        }

        public DS4State getDS4State()
        {
            return s;
        }
    }
}
