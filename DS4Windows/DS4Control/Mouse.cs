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
        public int CursorGyroDead { get => cursor.GyroCursorDeadZone; set => cursor.GyroCursorDeadZone = value; }


        internal const int TRACKBALL_INIT_FICTION = 10;
        internal const int TRACKBALL_MASS = 45;
        internal const double TRACKBALL_RADIUS = 0.0245;

        private double TRACKBALL_INERTIA = 2.0 * (TRACKBALL_MASS * TRACKBALL_RADIUS * TRACKBALL_RADIUS) / 5.0;
        private double TRACKBALL_SCALE = 0.004;
        private const int TRACKBALL_BUFFER_LEN = 8;
        private double[] trackballXBuffer = new double[TRACKBALL_BUFFER_LEN];
        private double[] trackballYBuffer = new double[TRACKBALL_BUFFER_LEN];
        private int trackballBufferTail = 0;
        private int trackballBufferHead = 0;
        private double trackballAccel = 0.0;
        private double trackballXVel = 0.0;
        private double trackballYVel = 0.0;
        private bool trackballActive = false;
        private double trackballDXRemain = 0.0;
        private double trackballDYRemain = 0.0;

        public struct GyroSwipeData
        {
            public bool swipeLeft, swipeRight, swipeUp, swipeDown;
            public bool previousSwipeLeft, previousSwipeRight, previousSwipeUp, previousSwipeDown;
            public enum XDir : ushort { None, Left, Right }
            public enum YDir : ushort { None, Up, Down }

            public XDir currentXDir;
            public YDir currentYDir;
            public bool xActive;
            public bool yActive;

            public DateTime initialTimeX;
            public DateTime initialTimeY;
        }

        public GyroSwipeData gyroSwipe;

        public Mouse(int deviceID, DS4Device d)
        {
            deviceNum = deviceID;
            dev = d;
            cursor = new MouseCursor(deviceNum, d.GyroMouseSensSettings);
            wheel = new MouseWheel(deviceNum);
            trackballAccel = TRACKBALL_RADIUS * TRACKBALL_INIT_FICTION / TRACKBALL_INERTIA;
            firstTouch = new Touch(0, 0, 0, null);

            filterPair.axis1Filter.MinCutoff = filterPair.axis2Filter.MinCutoff = GyroMouseStickInfo.DEFAULT_MINCUTOFF;
            filterPair.axis1Filter.Beta = filterPair.axis2Filter.Beta = GyroMouseStickInfo.DEFAULT_BETA;
            Global.GyroMouseStickInf[deviceNum].SetRefreshEvents(filterPair.axis1Filter);
            Global.GyroMouseStickInf[deviceNum].SetRefreshEvents(filterPair.axis2Filter);
        }

        public void ResetTrackAccel(double friction)
        {
            trackballAccel = TRACKBALL_RADIUS * friction / TRACKBALL_INERTIA;
        }

        public void ResetToggleGyroModes()
        {
            currentToggleGyroControls = false;
            currentToggleGyroM = false;
            currentToggleGyroStick = false;

            previousTriggerActivated = false;
            triggeractivated = false;
        }

        bool triggeractivated = false;
        bool previousTriggerActivated = false;
        bool useReverseRatchet = false;

        private bool toggleGyroControls = true;
        public bool ToggleGyroControls
        {
            get => toggleGyroControls;
            set
            {
                toggleGyroControls = value;
                ResetToggleGyroModes();
            }
        }

        private bool toggleGyroMouse = true;
        public bool ToggleGyroMouse
        {
            get => toggleGyroMouse;
            set
            {
                toggleGyroMouse = value;
                ResetToggleGyroModes();
            }
        }

        private bool toggleGyroStick = true;
        public bool ToggleGyroStick
        {
            get => toggleGyroStick;
            set
            {
                toggleGyroStick = value;
                ResetToggleGyroModes();
            }
        }

        public MouseCursor Cursor => cursor;

        bool currentToggleGyroControls = false;
        bool currentToggleGyroM = false;
        bool currentToggleGyroStick = false;

        public virtual void sixaxisMoved(DS4SixAxis sender, SixAxisEventArgs arg)
        {
            GyroOutMode outMode = Global.GetGyroOutMode(deviceNum);
            if (outMode == GyroOutMode.Controls)
            {
                s = dev.getCurrentStateRef();

                GyroControlsInfo controlsMapInfo = Global.GetGyroControlsInfo(deviceNum);
                useReverseRatchet = controlsMapInfo.triggerTurns;
                int i = 0;
                string[] ss = controlsMapInfo.triggers.Split(',');
                bool andCond = controlsMapInfo.triggerCond;
                triggeractivated = andCond ? true : false;
                if (!string.IsNullOrEmpty(ss[0]))
                {
                    string s = string.Empty;
                    for (int index = 0, arlen = ss.Length; index < arlen; index++)
                    {
                        s = ss[index];
                        if (andCond && !(int.TryParse(s, out i) && getDS4ControlsByName(i)))
                        {
                            triggeractivated = false;
                            break;
                        }
                        else if (!andCond && int.TryParse(s, out i) && getDS4ControlsByName(i))
                        {
                            triggeractivated = true;
                            break;
                        }
                    }
                }

                if (toggleGyroControls)
                {
                    if (triggeractivated && triggeractivated != previousTriggerActivated)
                    {
                        currentToggleGyroStick = !currentToggleGyroStick;
                    }

                    previousTriggerActivated = triggeractivated;
                    triggeractivated = currentToggleGyroStick;
                }
                else
                {
                    previousTriggerActivated = triggeractivated;
                }

                if (useReverseRatchet && triggeractivated)
                {
                    s.Motion.outputGyroControls = true;
                }
                else if (!useReverseRatchet && !triggeractivated)
                {
                    s.Motion.outputGyroControls = true;
                }
                else
                {
                    s.Motion.outputGyroControls = false;
                }
            }
            else if (outMode == GyroOutMode.Mouse && Global.getGyroSensitivity(deviceNum) > 0)
            {
                s = dev.getCurrentStateRef();

                useReverseRatchet = Global.getGyroTriggerTurns(deviceNum);
                int i = 0;
                string[] ss = Global.getSATriggers(deviceNum).Split(',');
                bool andCond = Global.getSATriggerCond(deviceNum);
                triggeractivated = andCond ? true : false;
                if (!string.IsNullOrEmpty(ss[0]))
                {
                    string s = string.Empty;
                    for (int index = 0, arlen = ss.Length; index < arlen; index++)
                    {
                        s = ss[index];
                        if (andCond && !(int.TryParse(s, out i) && getDS4ControlsByName(i)))
                        {
                            triggeractivated = false;
                            break;
                        }
                        else if (!andCond && int.TryParse(s, out i) && getDS4ControlsByName(i))
                        {
                            triggeractivated = true;
                            break;
                        }
                    }
                }

                if (toggleGyroMouse)
                {
                    if (triggeractivated && triggeractivated != previousTriggerActivated)
                    {
                        currentToggleGyroControls = !currentToggleGyroControls;
                    }

                    previousTriggerActivated = triggeractivated;
                    triggeractivated = currentToggleGyroControls;
                }
                else
                {
                    previousTriggerActivated = triggeractivated;
                }

                if (useReverseRatchet && triggeractivated)
                    cursor.sixaxisMoved(arg);
                else if (!useReverseRatchet && !triggeractivated)
                    cursor.sixaxisMoved(arg);
                else
                    cursor.mouseRemainderReset(arg);

            }
            else if (outMode == GyroOutMode.MouseJoystick)
            {
                s = dev.getCurrentStateRef();

                useReverseRatchet = Global.GetGyroMouseStickTriggerTurns(deviceNum);
                int i = 0;
                string[] ss = Global.GetSAMouseStickTriggers(deviceNum).Split(',');
                bool andCond = Global.GetSAMouseStickTriggerCond(deviceNum);
                triggeractivated = andCond ? true : false;
                if (!string.IsNullOrEmpty(ss[0]))
                {
                    string s = string.Empty;
                    for (int index = 0, arlen = ss.Length; index < arlen; index++)
                    {
                        s = ss[index];
                        if (andCond && !(int.TryParse(s, out i) && getDS4ControlsByName(i)))
                        {
                            triggeractivated = false;
                            break;
                        }
                        else if (!andCond && int.TryParse(s, out i) && getDS4ControlsByName(i))
                        {
                            triggeractivated = true;
                            break;
                        }
                    }
                }

                if (toggleGyroStick)
                {
                    if (triggeractivated && triggeractivated != previousTriggerActivated)
                    {
                        currentToggleGyroM = !currentToggleGyroM;
                    }

                    previousTriggerActivated = triggeractivated;
                    triggeractivated = currentToggleGyroM;
                }
                else
                {
                    previousTriggerActivated = triggeractivated;
                }

                if (useReverseRatchet && triggeractivated)
                    SixMouseStick(arg);
                else if (!useReverseRatchet && !triggeractivated)
                    SixMouseStick(arg);
                else
                    SixMouseReset(arg);
            }
            else if (outMode == GyroOutMode.DirectionalSwipe)
            {
                s = dev.getCurrentStateRef();

                GyroDirectionalSwipeInfo swipeMapInfo = Global.GetGyroSwipeInfo(deviceNum);
                useReverseRatchet = swipeMapInfo.triggerTurns;
                int i = 0;
                string[] ss = swipeMapInfo.triggers.Split(',');
                bool andCond = swipeMapInfo.triggerCond;
                triggeractivated = andCond ? true : false;
                if (!string.IsNullOrEmpty(ss[0]))
                {
                    string s = string.Empty;
                    for (int index = 0, arlen = ss.Length; index < arlen; index++)
                    {
                        s = ss[index];
                        if (andCond && !(int.TryParse(s, out i) && getDS4ControlsByName(i)))
                        {
                            triggeractivated = false;
                            break;
                        }
                        else if (!andCond && int.TryParse(s, out i) && getDS4ControlsByName(i))
                        {
                            triggeractivated = true;
                            break;
                        }
                    }
                }

                gyroSwipe.previousSwipeLeft = gyroSwipe.swipeLeft;
                gyroSwipe.previousSwipeRight = gyroSwipe.swipeRight;
                gyroSwipe.previousSwipeUp = gyroSwipe.swipeUp;
                gyroSwipe.previousSwipeDown = gyroSwipe.swipeDown;

                if (useReverseRatchet && triggeractivated)
                {
                    SixDirectionalSwipe(arg, swipeMapInfo);
                }
                else if (!useReverseRatchet && !triggeractivated)
                {
                    SixDirectionalSwipe(arg, swipeMapInfo);
                }
                else
                {
                    gyroSwipe.swipeLeft = gyroSwipe.swipeRight =
                        gyroSwipe.swipeUp = gyroSwipe.swipeDown = false;
                }
            }
        }

        private OneEuroFilterPair filterPair = new OneEuroFilterPair();

        public void ReplaceOneEuroFilterPair()
        {
            Global.GyroMouseStickInf[deviceNum].RemoveRefreshEvents();
            filterPair = new OneEuroFilterPair();
        }

        public void SetupLateOneEuroFilters()
        {
            filterPair.axis1Filter.MinCutoff = filterPair.axis2Filter.MinCutoff = Global.GyroMouseStickInf[deviceNum].MinCutoff;
            filterPair.axis1Filter.Beta = filterPair.axis2Filter.Beta = Global.GyroMouseStickInf[deviceNum].Beta;
            Global.GyroMouseStickInf[deviceNum].SetRefreshEvents(filterPair.axis1Filter);
            Global.GyroMouseStickInf[deviceNum].SetRefreshEvents(filterPair.axis2Filter);
        }

        private const int SMOOTH_BUFFER_LEN = 3;
        private int[] xSmoothBuffer = new int[SMOOTH_BUFFER_LEN];
        private int[] ySmoothBuffer = new int[SMOOTH_BUFFER_LEN];
        private int smoothBufferTail = 0;

        private void SixMouseReset(SixAxisEventArgs args)
        {
            int iIndex = smoothBufferTail % SMOOTH_BUFFER_LEN;
            xSmoothBuffer[iIndex] = 0;
            ySmoothBuffer[iIndex] = 0;
            smoothBufferTail = iIndex + 1;

            GyroMouseStickInfo msinfo = Global.GetGyroMouseStickInfo(deviceNum);
            if (msinfo.smoothingMethod == GyroMouseStickInfo.SmoothingMethod.OneEuro)
            {
                double currentRate = 1.0 / args.sixAxis.elapsed;
                filterPair.axis1Filter.Filter(0.0, currentRate);
                filterPair.axis2Filter.Filter(0.0, currentRate);
            }
        }

        private void SixMouseStick(SixAxisEventArgs arg)
        {
            int deltaX = 0, deltaY = 0;
            deltaX = Global.getGyroMouseStickHorizontalAxis(0) == 0 ? arg.sixAxis.gyroYawFull :
                arg.sixAxis.gyroRollFull;
            deltaY = -arg.sixAxis.gyroPitchFull;
            //int inputX = deltaX, inputY = deltaY;
            int maxDirX = deltaX >= 0 ? 127 : -128;
            int maxDirY = deltaY >= 0 ? 127 : -128;

            GyroMouseStickInfo msinfo = Global.GetGyroMouseStickInfo(deviceNum);

            double tempDouble = arg.sixAxis.elapsed * 250.0; // Base default speed on 4 ms
            double tempAngle = Math.Atan2(-deltaY, deltaX);
            double normX = Math.Abs(Math.Cos(tempAngle));
            double normY = Math.Abs(Math.Sin(tempAngle));
            int signX = Math.Sign(deltaX);
            int signY = Math.Sign(deltaY);

            int deadzoneX = (int)Math.Abs(normX * msinfo.deadZone);
            int deadzoneY = (int)Math.Abs(normY * msinfo.deadZone);

            int maxValX = signX * msinfo.maxZone;
            int maxValY = signY * msinfo.maxZone;

            double xratio = 0.0, yratio = 0.0;
            double antiX = msinfo.antiDeadX * normX;
            double antiY = msinfo.antiDeadY * normY;

            if (Math.Abs(deltaX) > deadzoneX)
            {
                deltaX -= signX * deadzoneX;
                //deltaX = (int)(deltaX * tempDouble);
                deltaX = (deltaX < 0 && deltaX < maxValX) ? maxValX :
                    (deltaX > 0 && deltaX > maxValX) ? maxValX : deltaX;
                //if (deltaX != maxValX) deltaX -= deltaX % (signX * GyroMouseFuzz);
            }
            else
            {
                deltaX = 0;
            }

            if (Math.Abs(deltaY) > deadzoneY)
            {
                deltaY -= signY * deadzoneY;
                //deltaY = (int)(deltaY * tempDouble);
                deltaY = (deltaY < 0 && deltaY < maxValY) ? maxValY :
                    (deltaY > 0 && deltaY > maxValY) ? maxValY : deltaY;
                //if (deltaY != maxValY) deltaY -= deltaY % (signY * GyroMouseFuzz);
            }
            else
            {
                deltaY = 0;
            }

            if (msinfo.useSmoothing)
            {
                if (msinfo.smoothingMethod == GyroMouseStickInfo.SmoothingMethod.OneEuro)
                {
                    double currentRate = 1.0 / arg.sixAxis.elapsed;
                    deltaX = (int)(filterPair.axis1Filter.Filter(deltaX, currentRate));
                    deltaY = (int)(filterPair.axis2Filter.Filter(deltaY, currentRate));
                }
                else if (msinfo.smoothingMethod == GyroMouseStickInfo.SmoothingMethod.WeightedAverage)
                {
                    int iIndex = smoothBufferTail % SMOOTH_BUFFER_LEN;
                    xSmoothBuffer[iIndex] = deltaX;
                    ySmoothBuffer[iIndex] = deltaY;
                    smoothBufferTail = iIndex + 1;

                    double currentWeight = 1.0;
                    double finalWeight = 0.0;
                    double x_out = 0.0, y_out = 0.0;
                    int idx = 0;
                    for (int i = 0; i < SMOOTH_BUFFER_LEN; i++)
                    {
                        idx = (smoothBufferTail - i - 1 + SMOOTH_BUFFER_LEN) % SMOOTH_BUFFER_LEN;
                        x_out += xSmoothBuffer[idx] * currentWeight;
                        y_out += ySmoothBuffer[idx] * currentWeight;
                        finalWeight += currentWeight;
                        currentWeight *= msinfo.smoothWeight;
                    }

                    x_out /= finalWeight;
                    deltaX = (int)x_out;
                    y_out /= finalWeight;
                    deltaY = (int)y_out;
                }

                maxValX = deltaX < 0 ? -msinfo.maxZone : msinfo.maxZone;
                maxValY = deltaY < 0 ? -msinfo.maxZone : msinfo.maxZone;
                maxDirX = deltaX >= 0 ? 127 : -128;
                maxDirY = deltaY >= 0 ? 127 : -128;
            }

            if (msinfo.vertScale != 100)
            {
                double verticalScale = msinfo.vertScale * 0.01;
                deltaY = (int)(deltaY * verticalScale);
                deltaY = (deltaY < 0 && deltaY < maxValY) ? maxValY :
                    (deltaY > 0 && deltaY > maxValY) ? maxValY : deltaY;
            }

            if (deltaX != 0) xratio = deltaX / (double)maxValX;
            if (deltaY != 0) yratio = deltaY / (double)maxValY;

            if (msinfo.maxOutputEnabled)
            {
                double maxOutRatio = msinfo.maxOutput / 100.0;
                double maxOutXRatio = normX * maxOutRatio;
                double maxOutYRatio = normY * maxOutRatio;
                xratio = Math.Min(Math.Max(xratio, 0.0), maxOutXRatio);
                yratio = Math.Min(Math.Max(yratio, 0.0), maxOutYRatio);
            }

            double xNorm = 0.0, yNorm = 0.0;
            if (xratio != 0.0)
            {
                xNorm = (1.0 - antiX) * xratio + antiX;
            }

            if (yratio != 0.0)
            {
                yNorm = (1.0 - antiY) * yratio + antiY;
            }

            if (msinfo.inverted != 0)
            {
                if ((msinfo.inverted & 1) == 1)
                {
                    // Invert max dir value
                    maxDirX = deltaX >= 0 ? -128 : 127;
                }

                if ((msinfo.inverted & 2) == 2)
                {
                    // Invert max dir value
                    maxDirY = deltaY >= 0 ? -128 : 127;
                }
            }

            byte axisXOut = (byte)(xNorm * maxDirX + 128.0);
            byte axisYOut = (byte)(yNorm * maxDirY + 128.0);
            Mapping.gyroStickX[deviceNum] = axisXOut;
            Mapping.gyroStickY[deviceNum] = axisYOut;
        }

        private void SixDirectionalSwipe(SixAxisEventArgs arg, GyroDirectionalSwipeInfo swipeInfo)
        {
            double velX = swipeInfo.xAxis == GyroDirectionalSwipeInfo.XAxisSwipe.Yaw ?
                arg.sixAxis.angVelYaw : arg.sixAxis.angVelRoll;
            double velY = arg.sixAxis.angVelPitch;
            int delayTime = swipeInfo.delayTime;

            int deadzoneX = (int)Math.Abs(swipeInfo.deadzoneX);
            int deadzoneY = (int)Math.Abs(swipeInfo.deadzoneY);

            gyroSwipe.swipeLeft = gyroSwipe.swipeRight = false;
            if (Math.Abs(velX) > deadzoneX)
            {
                if (velX > 0)
                {
                    if (gyroSwipe.currentXDir != GyroSwipeData.XDir.Right)
                    {
                        gyroSwipe.initialTimeX = DateTime.Now;
                        gyroSwipe.currentXDir = GyroSwipeData.XDir.Right;
                        gyroSwipe.xActive = delayTime == 0;
                    }

                    if (gyroSwipe.xActive || (gyroSwipe.xActive = gyroSwipe.initialTimeX + TimeSpan.FromMilliseconds(delayTime) < DateTime.Now))
                    {
                        gyroSwipe.swipeRight = true;
                    }
                }
                else
                {
                    if (gyroSwipe.currentXDir != GyroSwipeData.XDir.Left)
                    {
                        gyroSwipe.initialTimeX = DateTime.Now;
                        gyroSwipe.currentXDir = GyroSwipeData.XDir.Left;
                        gyroSwipe.xActive = delayTime == 0;
                    }

                    if (gyroSwipe.xActive || (gyroSwipe.xActive = gyroSwipe.initialTimeX + TimeSpan.FromMilliseconds(delayTime) < DateTime.Now))
                    {
                        gyroSwipe.swipeLeft = true;
                    }
                }
            }
            else
            {
                gyroSwipe.currentXDir = GyroSwipeData.XDir.None;
            }

            gyroSwipe.swipeUp = gyroSwipe.swipeDown = false;
            if (Math.Abs(velY) > deadzoneY)
            {
                if (velY > 0)
                {
                    if (gyroSwipe.currentYDir != GyroSwipeData.YDir.Up)
                    {
                        gyroSwipe.initialTimeY = DateTime.Now;
                        gyroSwipe.currentYDir = GyroSwipeData.YDir.Up;
                        gyroSwipe.yActive = delayTime == 0;
                    }

                    if (gyroSwipe.yActive || (gyroSwipe.yActive = gyroSwipe.initialTimeY + TimeSpan.FromMilliseconds(delayTime) < DateTime.Now))
                    {
                        gyroSwipe.swipeUp = true;
                    }
                }
                else
                {
                    if (gyroSwipe.currentYDir != GyroSwipeData.YDir.Down)
                    {
                        gyroSwipe.initialTimeY = DateTime.Now;
                        gyroSwipe.currentYDir = GyroSwipeData.YDir.Down;
                        gyroSwipe.yActive = delayTime == 0;
                    }

                    if (gyroSwipe.yActive || (gyroSwipe.yActive = gyroSwipe.initialTimeY + TimeSpan.FromMilliseconds(delayTime) < DateTime.Now))
                    {
                        gyroSwipe.swipeDown = true;
                    }
                }
            }
            else
            {
                gyroSwipe.currentYDir = GyroSwipeData.YDir.None;
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
                case 5: return s.L2 > 128;
                case 6: return s.R1;
                case 7: return s.R2 > 128;
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
                case 19: return s.TouchButton;
                case 20: return s.Mute;
                default: break;
            }

            return false;
        }

        private bool tempBool = false;
        public virtual void touchesMoved(DS4Touchpad sender, TouchpadEventArgs arg)
        {
            s = dev.getCurrentStateRef();

            TouchpadOutMode tempMode = Global.TouchOutMode[deviceNum];
            if (tempMode == TouchpadOutMode.Mouse)
            {
                if (Global.GetTouchActive(deviceNum))
                {
                    int[] disArray = Global.getTouchDisInvertTriggers(deviceNum);
                    tempBool = true;
                    for (int i = 0, arlen = disArray.Length; tempBool && i < arlen; i++)
                    {
                        if (getDS4ControlsByName(disArray[i]) == false)
                            tempBool = false;
                    }

                    if (Global.getTrackballMode(deviceNum))
                    {
                        int iIndex = trackballBufferTail;
                        trackballXBuffer[iIndex] = (arg.touches[0].deltaX * TRACKBALL_SCALE) / dev.getCurrentStateRef().elapsedTime;
                        trackballYBuffer[iIndex] = (arg.touches[0].deltaY * TRACKBALL_SCALE) / dev.getCurrentStateRef().elapsedTime;
                        trackballBufferTail = (iIndex + 1) % TRACKBALL_BUFFER_LEN;
                        if (trackballBufferHead == trackballBufferTail)
                            trackballBufferHead = (trackballBufferHead + 1) % TRACKBALL_BUFFER_LEN;
                    }

                    cursor.touchesMoved(arg, dragging || dragging2, tempBool);
                    wheel.touchesMoved(arg, dragging || dragging2);
                }
                else
                {
                    if (Global.getTrackballMode(deviceNum))
                    {
                        int iIndex = trackballBufferTail;
                        trackballXBuffer[iIndex] = 0;
                        trackballYBuffer[iIndex] = 0;
                        trackballBufferTail = (iIndex + 1) % TRACKBALL_BUFFER_LEN;
                        if (trackballBufferHead == trackballBufferTail)
                            trackballBufferHead = (trackballBufferHead + 1) % TRACKBALL_BUFFER_LEN;
                    }
                }
            }
            else if (tempMode == TouchpadOutMode.Controls)
            {
                if (!(swipeUp || swipeDown || swipeLeft || swipeRight) && arg.touches.Length == 1)
                {
                    if (arg.touches[0].hwX - firstTouch.hwX > 300) swipeRight = true;
                    if (arg.touches[0].hwX - firstTouch.hwX < -300) swipeLeft = true;
                    if (arg.touches[0].hwY - firstTouch.hwY > 300) swipeDown = true;
                    if (arg.touches[0].hwY - firstTouch.hwY < -300) swipeUp = true;
                }

                swipeUpB = (byte)Math.Min(255, Math.Max(0, (firstTouch.hwY - arg.touches[0].hwY) * 1.5f));
                swipeDownB = (byte)Math.Min(255, Math.Max(0, (arg.touches[0].hwY - firstTouch.hwY) * 1.5f));
                swipeLeftB = (byte)Math.Min(255, Math.Max(0, firstTouch.hwX - arg.touches[0].hwX));
                swipeRightB = (byte)Math.Min(255, Math.Max(0, arg.touches[0].hwX - firstTouch.hwX));
            }
            else if (tempMode == TouchpadOutMode.AbsoluteMouse)
            {
                if (Global.GetTouchActive(deviceNum))
                {
                    cursor.TouchesMovedAbsolute(arg);
                }
            }

            // Slide flags needed for possible profile switching from Touchpad swipes
            if (Math.Abs(firstTouch.hwY - arg.touches[0].hwY) < 50 && arg.touches.Length == 2)
            {
                if (arg.touches[0].hwX - firstTouch.hwX > 200 && !slideleft)
                    slideright = true;
                else if (firstTouch.hwX - arg.touches[0].hwX > 200 && !slideright)
                    slideleft = true;
            }

            synthesizeMouseButtons();
        }

        public virtual void touchesBegan(DS4Touchpad sender, TouchpadEventArgs arg)
        {
            TouchpadOutMode tempMode = Global.TouchOutMode[deviceNum];
            bool mouseMode = tempMode == TouchpadOutMode.Mouse;
            if (mouseMode)
            {
                Array.Clear(trackballXBuffer, 0, TRACKBALL_BUFFER_LEN);
                Array.Clear(trackballYBuffer, 0, TRACKBALL_BUFFER_LEN);
                trackballXVel = 0.0;
                trackballYVel = 0.0;
                trackballActive = false;
                trackballBufferTail = 0;
                trackballBufferHead = 0;
                trackballDXRemain = 0.0;
                trackballDYRemain = 0.0;

                cursor.touchesBegan(arg);
                wheel.touchesBegan(arg);
            }

            pastTime = arg.timeStamp;
            firstTouch.populate(arg.touches[0].hwX, arg.touches[0].hwY, arg.touches[0].touchID,
                arg.touches[0].previousTouch);

            if (mouseMode && Global.getDoubleTap(deviceNum))
            {
                DateTime test = arg.timeStamp;
                if (test <= (firstTap + TimeSpan.FromMilliseconds((double)Global.TapSensitivity[deviceNum] * 1.5)) && !arg.touchButtonPressed)
                    secondtouchbegin = true;
            }

            s = dev.getCurrentStateRef();
            synthesizeMouseButtons();
        }

        public virtual void touchesEnded(DS4Touchpad sender, TouchpadEventArgs arg)
        {
            s = dev.getCurrentStateRef();
            slideright = slideleft = false;
            swipeUp = swipeDown = swipeLeft = swipeRight = false;
            swipeUpB = swipeDownB = swipeLeftB = swipeRightB = 0;
            byte tapSensitivity = Global.getTapSensitivity(deviceNum);
            if (tapSensitivity != 0 && Global.TouchOutMode[deviceNum] == TouchpadOutMode.Mouse)
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
                        if (Global.getDoubleTap(deviceNum))
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
            else
            {
                TouchpadOutMode tempMode = Global.TouchOutMode[deviceNum];
                if (tempMode == TouchpadOutMode.Mouse)
                {
                    int[] disArray = Global.getTouchDisInvertTriggers(deviceNum);
                    tempBool = true;
                    for (int i = 0, arlen = disArray.Length; tempBool && i < arlen; i++)
                    {
                        if (getDS4ControlsByName(disArray[i]) == false)
                            tempBool = false;
                    }

                    if (Global.getTrackballMode(deviceNum))
                    {
                        if (!trackballActive)
                        {
                            double currentWeight = 1.0;
                            double finalWeight = 0.0;
                            double x_out = 0.0, y_out = 0.0;
                            int idx = -1;
                            for (int i = 0; i < TRACKBALL_BUFFER_LEN && idx != trackballBufferHead; i++)
                            {
                                idx = (trackballBufferTail - i - 1 + TRACKBALL_BUFFER_LEN) % TRACKBALL_BUFFER_LEN;
                                x_out += trackballXBuffer[idx] * currentWeight;
                                y_out += trackballYBuffer[idx] * currentWeight;
                                finalWeight += currentWeight;
                                currentWeight *= 1.0;
                            }

                            x_out /= finalWeight;
                            trackballXVel = x_out;
                            y_out /= finalWeight;
                            trackballYVel = y_out;

                            trackballActive = true;
                        }

                        double tempAngle = Math.Atan2(-trackballYVel, trackballXVel);
                        double normX = Math.Abs(Math.Cos(tempAngle));
                        double normY = Math.Abs(Math.Sin(tempAngle));
                        int signX = Math.Sign(trackballXVel);
                        int signY = Math.Sign(trackballYVel);
                        
                        double trackXvDecay = Math.Min(Math.Abs(trackballXVel), trackballAccel * s.elapsedTime * normX);
                        double trackYvDecay = Math.Min(Math.Abs(trackballYVel), trackballAccel * s.elapsedTime * normY);
                        double xVNew = trackballXVel - (trackXvDecay * signX);
                        double yVNew = trackballYVel - (trackYvDecay * signY);
                        double xMotion = (xVNew * s.elapsedTime) / TRACKBALL_SCALE;
                        double yMotion = (yVNew * s.elapsedTime) / TRACKBALL_SCALE;
                        if (xMotion != 0.0)
                        {
                            xMotion += trackballDXRemain;
                        }
                        else
                        {
                            trackballDXRemain = 0.0;
                        }

                        int dx = (int)xMotion;
                        trackballDXRemain = xMotion - dx;

                        if (yMotion != 0.0)
                        {
                            yMotion += trackballDYRemain;
                        }
                        else
                        {
                            trackballDYRemain = 0.0;
                        }

                        int dy = (int)yMotion;
                        trackballDYRemain = yMotion - dy;

                        trackballXVel = xVNew;
                        trackballYVel = yVNew;

                        if (dx == 0 && dy == 0)
                        {
                            trackballActive = false;
                        }
                        else
                        {
                            cursor.TouchMoveCursor(dx, dy, tempBool);
                        }
                    }
                }
                else if (tempMode == TouchpadOutMode.AbsoluteMouse)
                {
                    TouchpadAbsMouseSettings absMouseSettings = Global.TouchAbsMouse[deviceNum];
                    if (Global.GetTouchActive(deviceNum) && absMouseSettings.snapToCenter)
                    {
                        cursor.TouchCenterAbsolute();
                    }
                }
            }

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

        public virtual void touchUnchanged(DS4Touchpad sender, EventArgs unused)
        {
            s = dev.getCurrentStateRef();

            if (trackballActive)
            {
                if (Global.TouchOutMode[deviceNum] == TouchpadOutMode.Mouse)
                {
                    int[] disArray = Global.getTouchDisInvertTriggers(deviceNum);
                    tempBool = true;
                    for (int i = 0, arlen = disArray.Length; tempBool && i < arlen; i++)
                    {
                        if (getDS4ControlsByName(disArray[i]) == false)
                            tempBool = false;
                    }

                    double tempAngle = Math.Atan2(-trackballYVel, trackballXVel);
                    double normX = Math.Abs(Math.Cos(tempAngle));
                    double normY = Math.Abs(Math.Sin(tempAngle));
                    int signX = Math.Sign(trackballXVel);
                    int signY = Math.Sign(trackballYVel);
                    double trackXvDecay = Math.Min(Math.Abs(trackballXVel), trackballAccel * s.elapsedTime * normX);
                    double trackYvDecay = Math.Min(Math.Abs(trackballYVel), trackballAccel * s.elapsedTime * normY);
                    double xVNew = trackballXVel - (trackXvDecay * signX);
                    double yVNew = trackballYVel - (trackYvDecay * signY);
                    double xMotion = (xVNew * s.elapsedTime) / TRACKBALL_SCALE;
                    double yMotion = (yVNew * s.elapsedTime) / TRACKBALL_SCALE;
                    if (xMotion != 0.0)
                    {
                        xMotion += trackballDXRemain;
                    }
                    else
                    {
                        trackballDXRemain = 0.0;
                    }

                    int dx = (int)xMotion;
                    trackballDXRemain = xMotion - dx;

                    if (yMotion != 0.0)
                    {
                        yMotion += trackballDYRemain;
                    }
                    else
                    {
                        trackballDYRemain = 0.0;
                    }

                    int dy = (int)yMotion;
                    trackballDYRemain = yMotion - dy;

                    trackballXVel = xVNew;
                    trackballYVel = yVNew;

                    if (dx == 0 && dy == 0)
                    {
                        trackballActive = false;
                    }
                    else
                    {
                        cursor.TouchMoveCursor(dx, dy, tempBool);
                    }
                }
            }

            if (s.Touch1Finger || s.TouchButton)
                synthesizeMouseButtons();
        }

        public bool dragging, dragging2;

        private void synthesizeMouseButtons()
        {
            TouchpadOutMode tempMode = Global.TouchOutMode[deviceNum];
            if (tempMode != TouchpadOutMode.Passthru)
            {
                bool touchClickPass = Global.TouchClickPassthru[deviceNum];
                if (!touchClickPass)
                {
                    // Reset output Touchpad click button
                    s.OutputTouchButton = false;
                }
            }
            else
            {
                // Don't allow virtual buttons for Passthru mode
                return;
            }

            if (Global.GetDS4CSetting(deviceNum, DS4Controls.TouchLeft).IsDefault &&
                leftDown)
            {
                Mapping.MapClick(deviceNum, Mapping.Click.Left);
                dragging2 = true;
            }
            else
            {
                dragging2 = false;
            }

            if (Global.GetDS4CSetting(deviceNum, DS4Controls.TouchUpper).IsDefault &&
                upperDown)
            {
                Mapping.MapClick(deviceNum, Mapping.Click.Middle);
            }

            if (Global.GetDS4CSetting(deviceNum, DS4Controls.TouchRight).IsDefault &&
                rightDown)
            {
                Mapping.MapClick(deviceNum, Mapping.Click.Left);
            }

            if (Global.GetDS4CSetting(deviceNum, DS4Controls.TouchMulti).IsDefault &&
                multiDown)
            {
                Mapping.MapClick(deviceNum, Mapping.Click.Right);
            }

            if (Global.TouchOutMode[deviceNum] == TouchpadOutMode.Mouse)
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
        }

        public virtual void touchButtonUp(DS4Touchpad sender, TouchpadEventArgs arg)
        {
            pushed = DS4Controls.None;
            upperDown = leftDown = rightDown = multiDown = false;
            s = dev.getCurrentStateRef();
            if (s.Touch1 || s.Touch2)
                synthesizeMouseButtons();
        }

        public virtual void touchButtonDown(DS4Touchpad sender, TouchpadEventArgs arg)
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
