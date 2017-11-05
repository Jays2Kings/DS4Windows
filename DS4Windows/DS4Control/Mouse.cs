using System;

namespace DS4Windows
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
        public bool swipeLeft, swipeRight, swipeUp, swipeDown;
        public bool priorSwipeLeft, priorSwipeRight, priorSwipeUp, priorSwipeDown;
        public byte swipeLeftB, swipeRightB, swipeUpB, swipeDownB, swipedB;
        public byte priorSwipeLeftB, priorSwipeRightB, priorSwipeUpB, priorSwipeDownB, priorSwipedB;
        public bool slideleft, slideright;
        public bool priorSlideLeft, priorSlideright;
        // touch area stuff
        public bool leftDown, rightDown, upperDown, multiDown;
        public bool priorLeftDown, priorRightDown, priorUpperDown, priorMultiDown;
        protected DS4Controls pushed = DS4Controls.None;
        protected Mapping.Click clicked = Mapping.Click.None;

        public Mouse(int deviceID, DS4Device d)
        {
            deviceNum = deviceID;
            dev = d;
            cursor = new MouseCursor(deviceNum);
            wheel = new MouseWheel(deviceNum);
        }

        bool triggeractivated = false;
        bool useReverseRatchet = false;

        public virtual void sixaxisMoved(object sender, SixAxisEventArgs arg)
        {
            if (Global.isUsingSAforMouse(deviceNum) && Global.getGyroSensitivity(deviceNum) > 0)
            {
                s = dev.getCurrentStateRef();

                triggeractivated = true;
                useReverseRatchet = Global.getGyroTriggerTurns(deviceNum);
                int i = 0;
                string[] ss = Global.getSATriggers(deviceNum).Split(',');
                if (!string.IsNullOrEmpty(ss[0]))
                {
                    string s = string.Empty;
                    for (int index = 0, arlen = ss.Length;
                        triggeractivated && index < arlen; index++)
                    {
                        s = ss[index];
                        if (!(int.TryParse(s, out i) && getDS4ControlsByName(i)))
                        {
                            triggeractivated = false;
                        }
                    }
                }

                if (useReverseRatchet && triggeractivated)
                    cursor.sixaxisMoved(arg);
                else if (!useReverseRatchet && !triggeractivated)
                    cursor.sixaxisMoved(arg);
                else
                    cursor.mouseRemainderReset();
            }
        }

        private bool getDS4ControlsByName(int key)
        {
            switch (key)
            {
                case -1: return true;
                case 0: return s.Cross;
                case 1: return s.Circle;
                case 2: return s.Square;
                case 3: return s.Triangle;
                case 4: return s.L1;
                case 5: return s.L2 > 127;
                case 6: return s.R1;
                case 7: return s.R2 > 127;
                case 8: return s.DpadUp;
                case 9: return s.DpadDown;
                case 10: return s.DpadLeft;
                case 11: return s.DpadRight;
                case 12: return s.L3;
                case 13: return s.R3;
                case 14: return s.Touch1Finger;
                case 15: return s.Touch2Fingers;
                case 16: return s.Options;
                case 17: return s.Share;
                case 18: return s.PS;
                default: break;
            }

            return false;
        }

        private bool tempBool = false;
        public virtual void touchesMoved(object sender, TouchpadEventArgs arg)
        {
            s = dev.getCurrentStateRef();

            if (Global.getUseTPforControls(deviceNum) == false)
            {
                int[] disArray = Global.getTouchDisInvertTriggers(deviceNum);
                tempBool = true;
                for (int i = 0, arlen = disArray.Length; tempBool && i < arlen; i++)
                {
                    if (getDS4ControlsByName(disArray[i]) == false)
                        tempBool = false;
                }

                cursor.touchesMoved(arg, dragging || dragging2, tempBool);
                wheel.touchesMoved(arg, dragging || dragging2);
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
            {
                if (arg.touches[0].hwX - firstTouch.hwX > 200 && !slideleft)
                    slideright = true;
                else if (firstTouch.hwX - arg.touches[0].hwX > 200 && !slideright)
                    slideleft = true;
            }

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

            s = dev.getCurrentStateRef();
            synthesizeMouseButtons();
        }

        public virtual void touchesEnded(object sender, TouchpadEventArgs arg)
        {
            slideright = slideleft = false;
            swipeUp = swipeDown = swipeLeft = swipeRight = false;
            swipeUpB = swipeDownB = swipeLeftB = swipeRightB = 0;
            byte tapSensitivity = Global.TapSensitivity[deviceNum];
            if (tapSensitivity != 0 && !Global.UseTPforControls[deviceNum])
            {
                if (secondtouchbegin)
                {
                    tappedOnce = false;
                    secondtouchbegin = false;
                }

                DateTime test = arg.timeStamp;
                if (test <= (pastTime + TimeSpan.FromMilliseconds((double)tapSensitivity * 2)) && !arg.touchButtonPressed && !tappedOnce)
                {
                    if (Math.Abs(firstTouch.hwX - arg.touches[0].hwX) < 10 && Math.Abs(firstTouch.hwY - arg.touches[0].hwY) < 10)
                    {
                        if (Global.DoubleTap[deviceNum])
                        {
                            tappedOnce = true;
                            firstTap = arg.timeStamp;
                            TimeofEnd = DateTime.Now; //since arg can't be used in synthesizeMouseButtons
                        }
                        else
                            Mapping.MapClick(deviceNum, Mapping.Click.Left); //this way no delay if disabled
                    }
                }
            }

            s = dev.getCurrentStateRef();
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
            s = dev.getCurrentStateRef();
            synthesizeMouseButtons();
        }

        private DS4State remapped = new DS4State();
        public bool dragging, dragging2;
        private void synthesizeMouseButtons()
        {
            if (Global.GetDS4Action(deviceNum, DS4Controls.TouchLeft, false) == null && leftDown)
            {
                Mapping.MapClick(deviceNum, Mapping.Click.Left);
                dragging2 = true;
            }
            else
            {
                dragging2 = false;
            }

            if (Global.GetDS4Action(deviceNum, DS4Controls.TouchUpper, false) == null && upperDown)
                Mapping.MapClick(deviceNum, Mapping.Click.Middle);

            if (Global.GetDS4Action(deviceNum, DS4Controls.TouchRight, false) == null && rightDown)
                Mapping.MapClick(deviceNum, Mapping.Click.Left);

            if (Global.GetDS4Action(deviceNum, DS4Controls.TouchMulti, false) == null && multiDown)
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
                {
                    Mapping.MapClick(deviceNum, Mapping.Click.Left);
                    dragging = true;
                }
                else
                {
                    dragging = false;
                }
            }

            s = remapped;
        }

        public virtual void touchButtonUp(object sender, TouchpadEventArgs arg)
        {
            pushed = DS4Controls.None;
            upperDown = leftDown = rightDown = multiDown = false;
            dev.setRumble(0, 0);
            s = dev.getCurrentStateRef();
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

            s = dev.getCurrentStateRef();
            synthesizeMouseButtons();
        }

        public void populatePriorButtonStates()
        {
            priorUpperDown = upperDown;
            priorLeftDown = leftDown;
            priorRightDown = rightDown;
            priorMultiDown = multiDown;

            priorSwipeLeft = swipeLeft; priorSwipeRight = swipeRight;
            priorSwipeUp = swipeUp; priorSwipeDown = swipeDown;
            priorSwipeLeftB = swipeLeftB; priorSwipeRightB = swipeRightB; priorSwipeUpB = swipeUpB;
            priorSwipeDownB = swipeDownB; priorSwipedB = swipedB;
        }

        public DS4State getDS4State()
        {
            return s;
        }
    }
}
