﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace EAll4Windows
{
    public class Mouse : ITouchpadBehaviour
    {
        protected DateTime pastTime, firstTap, TimeofEnd;
        protected Touch firstTouch, secondTouch;
        private ControllerState s = new ControllerState();
        protected int deviceNum;
        private EAll4Device dev = null;
        private readonly MouseCursor cursor;
        private readonly MouseWheel wheel;
        private bool tappedOnce = false, secondtouchbegin = false;
        public bool swipeLeft, swipeRight, swipeUp, swipeDown;
        public byte swipeLeftB, swipeRightB, swipeUpB, swipeDownB, swipedB;
        public bool slideleft, slideright;
        // touch area stuff
        public bool leftDown, rightDown, upperDown, multiDown;
        protected GenericControls pushed = GenericControls.None;
        protected Mapping.Click clicked = Mapping.Click.None;

        public Mouse(int deviceID, EAll4Device d)
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

        public virtual void touchesMoved(object sender, TouchpadEventArgs arg)
        {
            if (!Global.UseTPforControls[deviceNum])
            {
                cursor.touchesMoved(arg);
                wheel.touchesMoved(arg);
            }
            else
            {
                if (!(swipeUp || swipeDown || swipeLeft || swipeRight) && arg.touches.Length == 1)
                {
                    if (arg.touches[0].hwX - firstTouch.hwX > 400) swipeRight = true;
                    if (arg.touches[0].hwX - firstTouch.hwX < -400) swipeLeft = true;
                    if (arg.touches[0].hwY - firstTouch.hwY > 300) swipeDown = true;
                    if (arg.touches[0].hwY - firstTouch.hwY < -300) swipeUp = true;
                }
                swipeUpB = (byte)Math.Min(255, Math.Max(0, (firstTouch.hwY - arg.touches[0].hwY) * 1.5f));
                swipeDownB = (byte)Math.Min(255, Math.Max(0, (arg.touches[0].hwY - firstTouch.hwY) * 1.5f));
                swipeLeftB = (byte)Math.Min(255, Math.Max(0, firstTouch.hwX - arg.touches[0].hwX));
                swipeRightB = (byte)Math.Min(255, Math.Max(0, arg.touches[0].hwX - firstTouch.hwX));
            }
            if (Math.Abs(firstTouch.hwY - arg.touches[0].hwY) < 50 && arg.touches.Length == 2)
                if (arg.touches[0].hwX - firstTouch.hwX > 200 && !slideleft)
                    slideright = true;
                else if (firstTouch.hwX - arg.touches[0].hwX > 200 && !slideright)
                    slideleft = true;
            dev.getCurrentState(s);
            synthesizeMouseButtons();
        }
        public virtual void touchesBegan(object sender, TouchpadEventArgs arg)
        {
            if (!Global.UseTPforControls[deviceNum])
            {
                cursor.touchesBegan(arg);
                wheel.touchesBegan(arg);
            }
            pastTime = arg.timeStamp;
            firstTouch = arg.touches[0];
            if (Global.DoubleTap[deviceNum])
            {
                DateTime test = arg.timeStamp;
                if (test <= (firstTap + TimeSpan.FromMilliseconds((double)Global.TapSensitivity[deviceNum] * 1.5)) && !arg.touchButtonPressed)
                    secondtouchbegin = true;
            }
            dev.getCurrentState(s);
            synthesizeMouseButtons();
        }
        public virtual void touchesEnded(object sender, TouchpadEventArgs arg)
        {
            slideright = slideleft = false;
            swipeUp = swipeDown = swipeLeft = swipeRight = false;
            swipeUpB = swipeDownB = swipeLeftB = swipeRightB = 0;
            if (Global.TapSensitivity[deviceNum] != 0 && !Global.UseTPforControls[deviceNum])
            {

                if (secondtouchbegin)
                {
                    tappedOnce = false;
                    secondtouchbegin = false;
                }
                DateTime test = arg.timeStamp;
                if (test <= (pastTime + TimeSpan.FromMilliseconds((double)Global.TapSensitivity[deviceNum] * 2)) && !arg.touchButtonPressed && !tappedOnce)
                    if (Math.Abs(firstTouch.hwX - arg.touches[0].hwX) < 10 && Math.Abs(firstTouch.hwY - arg.touches[0].hwY) < 10)
                        if (Global.DoubleTap[deviceNum])
                        {
                            tappedOnce = true;
                            firstTap = arg.timeStamp;
                            TimeofEnd = DateTime.Now; //since arg can't be used in synthesizeMouseButtons
                        }
                        else
                            Mapping.MapClick(deviceNum, Mapping.Click.Left); //this way no delay if disabled
            }
            dev.getCurrentState(s);
            synthesizeMouseButtons();
        }

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

        private ControllerState remapped = new ControllerState();
        private void synthesizeMouseButtons()
        {
            if (Global.getCustomButton(deviceNum, GenericControls.TouchLeft) == X360Controls.None &&
                Global.getCustomMacro(deviceNum, GenericControls.TouchLeft) == "0" &&
                    Global.getCustomKey(deviceNum, GenericControls.TouchLeft) == 0 &&
                leftDown)
                Mapping.MapClick(deviceNum, Mapping.Click.Left);
            if (Global.getCustomButton(deviceNum, GenericControls.TouchUpper) == X360Controls.None &&
                Global.getCustomMacro(deviceNum, GenericControls.TouchUpper) == "0" &&
                    Global.getCustomKey(deviceNum, GenericControls.TouchUpper) == 0 &&
                upperDown)
                Mapping.MapClick(deviceNum, Mapping.Click.Middle);
            if (Global.getCustomButton(deviceNum, GenericControls.TouchRight) == X360Controls.None &&
                Global.getCustomMacro(deviceNum, GenericControls.TouchRight) == "0" &&
                    Global.getCustomKey(deviceNum, GenericControls.TouchRight) == 0 &&
                rightDown)
                Mapping.MapClick(deviceNum, Mapping.Click.Left);
            if (Global.getCustomButton(deviceNum, GenericControls.TouchMulti) == X360Controls.None &&
                Global.getCustomMacro(deviceNum, GenericControls.TouchMulti) == "0" &&
                    Global.getCustomKey(deviceNum, GenericControls.TouchMulti) == 0 &&
                multiDown)
                Mapping.MapClick(deviceNum, Mapping.Click.Right);
            if (!Global.UseTPforControls[deviceNum])
            {
                if (tappedOnce)
                {
                    DateTime tester = DateTime.Now;
                    if (tester > (TimeofEnd + TimeSpan.FromMilliseconds((double)(Global.TapSensitivity[deviceNum]) * 1.5)))
                    {
                        Mapping.MapClick(deviceNum, Mapping.Click.Left);
                        tappedOnce = false;
                    }
                    //if it fails the method resets, and tries again with a new tester value (gives tap a delay so tap and hold can work)
                }
                if (secondtouchbegin) //if tap and hold (also works as double tap)
                    Mapping.MapClick(deviceNum, Mapping.Click.Left);
            }
            s = remapped;
            //remapped.CopyTo(s);
        }

        public virtual void touchButtonUp(object sender, TouchpadEventArgs arg)
        {
            pushed = GenericControls.None;
            upperDown = leftDown = rightDown = multiDown = false;
            dev.setRumble(0, 0);
            dev.getCurrentState(s);
            if (s.Touch1 || s.Touch2)
                synthesizeMouseButtons();
        }

        public virtual void touchButtonDown(object sender, TouchpadEventArgs arg)
        {
            if (arg.touches == null)
                upperDown = true;
            else if (arg.touches.Length > 1)
                multiDown = true;
            else
            {
                if ((Global.LowerRCOn[deviceNum] && arg.touches[0].hwX > (1920 * 3) / 4 && arg.touches[0].hwY > (960 * 3) / 4))
                    Mapping.MapClick(deviceNum, Mapping.Click.Right);
                if (isLeft(arg.touches[0]))
                    leftDown = true;
                else if (isRight(arg.touches[0]))
                    rightDown = true;
            }
            dev.getCurrentState(s);
            synthesizeMouseButtons();
        }

        public ControllerState getEAll4State()
        {
            return s;
        }
    }
}
