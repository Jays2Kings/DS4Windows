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
        protected DateTime pastTime, firstTap, TimeofEnd;
        protected Touch firstTouch, secondTouch;
        private DS4State s = new DS4State();
        protected int deviceNum;
        private DS4Device dev = null;
        private readonly MouseCursor cursor;
        private readonly MouseWheel wheel;
        private bool tappedOnce = false, secondtouchbegin = false;

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
        public bool slideleft, slideright;
        public virtual void touchesMoved(object sender, TouchpadEventArgs arg)
        {
            cursor.touchesMoved(arg);
            wheel.touchesMoved(arg);
            if (Math.Abs(firstTouch.hwY - arg.touches[0].hwY) < 50)
                if (arg.touches.Length == 2)
                    if (arg.touches[0].hwX - firstTouch.hwX > 200 && !slideleft)
                        slideright = true;
                    else if (firstTouch.hwX - arg.touches[0].hwX > 200 && !slideright)
                        slideleft = true;
            dev.getCurrentState(s);
            synthesizeMouseButtons();
            //if (arg.touches.Length == 2)
           // Console.WriteLine("Left " + slideleft + " Right " + slideright);
        }
        public virtual void touchesBegan(object sender, TouchpadEventArgs arg)
        {
            cursor.touchesBegan(arg);
            wheel.touchesBegan(arg);
            pastTime = arg.timeStamp;
            firstTouch = arg.touches[0];
            if (Global.getDoubleTap(deviceNum))
            {
                DateTime test = arg.timeStamp;
                if (test <= (firstTap + TimeSpan.FromMilliseconds((double)Global.getTapSensitivity(deviceNum) * 1.5)) && !arg.touchButtonPressed)
                    secondtouchbegin = true;
            }
            dev.getCurrentState(s);
            synthesizeMouseButtons(); 
            //Console.WriteLine(arg.timeStamp.ToString("O") + " " + "began at " + arg.touches[0].hwX + "," + arg.touches[0].hwY);
        }
        public virtual void touchesEnded(object sender, TouchpadEventArgs arg)
        {
            //Console.WriteLine(arg.timeStamp.ToString("O") + " " + "ended at " + arg.touches[0].hwX + "," + arg.touches[0].hwY);
            slideright = slideleft = false;
            if (Global.getTapSensitivity(deviceNum) != 0)
            {

                if (secondtouchbegin)
                {
                    tappedOnce = false;
                    secondtouchbegin = false;
                }
                DateTime test = arg.timeStamp;
                if (test <= (pastTime + TimeSpan.FromMilliseconds((double)Global.getTapSensitivity(deviceNum) * 2)) && !arg.touchButtonPressed && !tappedOnce)
                    if (Math.Abs(firstTouch.hwX - arg.touches[0].hwX) < 10 && Math.Abs(firstTouch.hwY - arg.touches[0].hwY) < 10)
                    if (Global.getDoubleTap(deviceNum))
                    {
                        tappedOnce = true; 
                        firstTap = arg.timeStamp;
                        TimeofEnd = DateTime.Now; //since arg can't be used in synthesizeMouseButtons
                    }
                    else
                        Mapping.MapClick(deviceNum, Mapping.Click.Left); //this way no delay if disabled
            }
            dev.getCurrentState(s);
            //if (buttonLock)
            synthesizeMouseButtons();
        }

        protected DS4Controls pushed = DS4Controls.None;
        protected Mapping.Click clicked = Mapping.Click.None;

        // touch area stuff
        public bool leftDown, rightDown, upperDown, multiDown;
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
            dev.getCurrentState(s);
            //if (s.Touch1 || s.Touch2 || s.TouchButton)
                synthesizeMouseButtons();
        }

        private DS4State remapped = new DS4State();
        private void synthesizeMouseButtons()
        {
            //Mapping.MapCustom(deviceNum, s, remapped, null);
            if (leftDown)
                Mapping.MapTouchpadButton(deviceNum, DS4Controls.TouchLeft, Mapping.Click.Left, remapped);
            if (upperDown)
                Mapping.MapTouchpadButton(deviceNum, DS4Controls.TouchUpper, Mapping.Click.Middle, remapped);
            if (rightDown)
                Mapping.MapTouchpadButton(deviceNum, DS4Controls.TouchRight, Mapping.Click.Left, remapped);
            if (multiDown)
                Mapping.MapTouchpadButton(deviceNum, DS4Controls.TouchMulti, Mapping.Click.Right, remapped);
            if (tappedOnce)
            {
                DateTime tester = DateTime.Now;
                if (tester > (TimeofEnd + TimeSpan.FromMilliseconds((double)(Global.getTapSensitivity(deviceNum)) * 1.5)))
                    {
                        Mapping.MapClick(deviceNum, Mapping.Click.Left); 
                        tappedOnce = false;
                    }
                //if it fails the method resets, and tries again with a new tester value (gives tap a delay so tap and hold can work)
            }
            if (secondtouchbegin) //if tap and hold (also works as double tap)
               Mapping.MapClick(deviceNum, Mapping.Click.Left);
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
                }
            }
            dev.getCurrentState(s);
            synthesizeMouseButtons();
        }

        public DS4State getDS4State()
        {
            return s;
        }
    }
}
