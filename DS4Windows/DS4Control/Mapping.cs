using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using static DS4Windows.Global;
using System.Drawing; // Point struct
using Sensorit.Base;

namespace DS4Windows
{
    public class Mapping
    {
        /*
         * Represent the synthetic keyboard and mouse events.  Maintain counts for each so we don't duplicate events.
         */
        public class SyntheticState
        {
            public struct MouseClick
            {
                public int leftCount, middleCount, rightCount, fourthCount, fifthCount, wUpCount, wDownCount, toggleCount;
                public bool toggle;
            }
            public MouseClick previousClicks, currentClicks;
            public struct KeyPress
            {
                public int vkCount, scanCodeCount, repeatCount, toggleCount; // repeat takes priority over non-, and scancode takes priority over non-
                public bool toggle;
            }
            public class KeyPresses
            {
                public KeyPress previous, current;
            }
            public Dictionary<UInt16, KeyPresses> keyPresses = new Dictionary<UInt16, KeyPresses>();

            public void SaveToPrevious(bool performClear)
            {
                previousClicks = currentClicks;
                if (performClear)
                    currentClicks.leftCount = currentClicks.middleCount = currentClicks.rightCount = currentClicks.fourthCount = currentClicks.fifthCount = currentClicks.wUpCount = currentClicks.wDownCount = currentClicks.toggleCount = 0;

                //foreach (KeyPresses kp in keyPresses.Values)
                Dictionary<ushort, KeyPresses>.ValueCollection keyValues = keyPresses.Values;
                for (var keyEnum = keyValues.GetEnumerator(); keyEnum.MoveNext();)
                //for (int i = 0, kpCount = keyValues.Count; i < kpCount; i++)
                {
                    //KeyPresses kp = keyValues.ElementAt(i);
                    KeyPresses kp = keyEnum.Current;
                    kp.previous = kp.current;
                    if (performClear)
                    {
                        kp.current.repeatCount = kp.current.scanCodeCount = kp.current.vkCount = kp.current.toggleCount = 0;
                        //kp.current.toggle = false;
                    }
                }
            }
        }

        public class ActionState
        {
            public bool[] dev = new bool[Global.MAX_DS4_CONTROLLER_COUNT];
        }

        struct ControlToXInput
        {
            public DS4Controls ds4input;
            public DS4Controls xoutput;

            public ControlToXInput(DS4Controls input, DS4Controls output)
            {
                ds4input = input; xoutput = output;
            }
        }

        static Queue<ControlToXInput>[] customMapQueue = new Queue<ControlToXInput>[Global.MAX_DS4_CONTROLLER_COUNT]
        {
            new Queue<ControlToXInput>(), new Queue<ControlToXInput>(),
            new Queue<ControlToXInput>(), new Queue<ControlToXInput>(),
            new Queue<ControlToXInput>(), new Queue<ControlToXInput>(),
            new Queue<ControlToXInput>(), new Queue<ControlToXInput>(),
        };

        struct DS4Vector2
        {
            public double x;
            public double y;

            public DS4Vector2(double x, double y)
            {
                this.x = x;
                this.y = y;
            }
        }

        class DS4SquareStick
        {
            public DS4Vector2 current;
            public DS4Vector2 squared;

            public DS4SquareStick()
            {
                current = new DS4Vector2(0.0, 0.0);
                squared = new DS4Vector2(0.0, 0.0);
            }

            // Modification of squared stick routine documented
            // at http://theinstructionlimit.com/squaring-the-thumbsticks
            public void CircleToSquare(double roundness)
            {
                const double PiOverFour = Math.PI / 4.0;

                // Determine the theta angle
                double angle = Math.Atan2(current.y, -current.x);
                angle += Math.PI;
                double cosAng = Math.Cos(angle);
                // Scale according to which wall we're clamping to
                // X+ wall
                if (angle <= PiOverFour || angle > 7.0 * PiOverFour)
                {
                    double tempVal = 1.0 / cosAng;
                    //Console.WriteLine("1 ANG: {0} | TEMP: {1}", angle, tempVal);
                    squared.x = current.x * tempVal;
                    squared.y = current.y * tempVal;
                }
                // Y+ wall
                else if (angle > PiOverFour && angle <= 3.0 * PiOverFour)
                {
                    double tempVal = 1.0 / Math.Sin(angle);
                    //Console.WriteLine("2 ANG: {0} | TEMP: {1}", angle, tempVal);
                    squared.x = current.x * tempVal;
                    squared.y = current.y * tempVal;
                }
                // X- wall
                else if (angle > 3.0 * PiOverFour && angle <= 5.0 * PiOverFour)
                {
                    double tempVal = -1.0 / cosAng;
                    //Console.WriteLine("3 ANG: {0} | TEMP: {1}", angle, tempVal);
                    squared.x = current.x * tempVal;
                    squared.y = current.y * tempVal;
                }
                // Y- wall
                else if (angle > 5.0 * PiOverFour && angle <= 7.0 * PiOverFour)
                {
                    double tempVal = -1.0 / Math.Sin(angle);
                    //Console.WriteLine("4 ANG: {0} | TEMP: {1}", angle, tempVal);
                    squared.x = current.x * tempVal;
                    squared.y = current.y * tempVal;
                }
                else return;

                //double lengthOld = Math.Sqrt((x * x) + (y * y));
                double length = current.x / cosAng;
                //Console.WriteLine("LENGTH TEST ({0}) ({1}) {2}", lengthOld, length, (lengthOld == length).ToString());
                double factor = Math.Pow(length, roundness);
                //double ogX = current.x, ogY = current.y;
                current.x += (squared.x - current.x) * factor;
                current.y += (squared.y - current.y) * factor;
                //Console.WriteLine("INPUT: {0} {1} | {2} {3} | {4} {5} | {6} {7}",
                //    ogX, ogY, current.x, current.y, squared.x, squared.y, length, factor);
            }
        }

        private static DS4SquareStick[] outSqrStk = new DS4SquareStick[Global.TEST_PROFILE_ITEM_COUNT]
        {
            new DS4SquareStick(), new DS4SquareStick(), new DS4SquareStick(), new DS4SquareStick(),
            new DS4SquareStick(), new DS4SquareStick(), new DS4SquareStick(), new DS4SquareStick(),
            new DS4SquareStick(),
        };

        public static byte[] gyroStickX = new byte[Global.MAX_DS4_CONTROLLER_COUNT] { 128, 128, 128, 128, 128, 128, 128, 128 };
        public static byte[] gyroStickY = new byte[Global.MAX_DS4_CONTROLLER_COUNT] { 128, 128, 128, 128, 128, 128, 128, 128 };

        // [<Device>][<AxisId>]. LX = 0, LY = 1, RX = 2, RY = 3
        public static byte[][] lastStickAxisValues = new byte[Global.MAX_DS4_CONTROLLER_COUNT][]
        {
            new byte[4] {128, 128, 128, 128}, new byte[4] {128, 128, 128, 128},
            new byte[4] {128, 128, 128, 128}, new byte[4] {128, 128, 128, 128},
            new byte[4] {128, 128, 128, 128}, new byte[4] {128, 128, 128, 128},
            new byte[4] {128, 128, 128, 128}, new byte[4] {128, 128, 128, 128},
        };

        private class LastWheelGyroCoord
        {
            public int gyroX;
            public int gyroZ;
        }

        private static LastWheelGyroCoord[] lastWheelGyroValues = new LastWheelGyroCoord[Global.MAX_DS4_CONTROLLER_COUNT]
        {
            new LastWheelGyroCoord(), new LastWheelGyroCoord(), new LastWheelGyroCoord(), new LastWheelGyroCoord(),
            new LastWheelGyroCoord(), new LastWheelGyroCoord(), new LastWheelGyroCoord(), new LastWheelGyroCoord()
        };
        //static int lastGyroX = 0;
        //static int lastGyroZ = 0;

        //private static OneEuroFilter filterX = new OneEuroFilter(minCutoff: 1, beta: 0);
        //private static OneEuroFilter filterZ = new OneEuroFilter(minCutoff: 1, beta: 0);
        //private static OneEuroFilter filterX = new OneEuroFilter(minCutoff: 0.0001, beta: 0.001);
        //private static OneEuroFilter filterZ = new OneEuroFilter(minCutoff: 0.0001, beta: 0.001);
        //private static OneEuroFilter wheel360FilterX = new OneEuroFilter(minCutoff: 0.1, beta: 0.02);
        //private static OneEuroFilter wheel360FilterZ = new OneEuroFilter(minCutoff: 0.1, beta: 0.02);

        public static OneEuroFilter[] wheelFilters = new OneEuroFilter[ControlService.MAX_DS4_CONTROLLER_COUNT];

        public class FlickStickMappingData
        {
            public const double DEFAULT_MINCUTOFF = 0.4;
            public const double DEFAULT_BETA = 0.4;

            public const double DEFAULT_FLICK_PROGRESS = 0.0;
            public const double DEFAULT_FLICK_SIZE = 0.0;
            public const double DEFAULT_FLICK_ANGLE_REMAINDER = 0.0;

            public OneEuroFilter flickFilter = new OneEuroFilter(DEFAULT_MINCUTOFF, DEFAULT_BETA);
            public double flickProgress = DEFAULT_FLICK_PROGRESS;
            public double flickSize = DEFAULT_FLICK_SIZE;
            public double flickAngleRemainder = DEFAULT_FLICK_ANGLE_REMAINDER;

            public void Reset()
            {
                flickFilter = new OneEuroFilter(DEFAULT_MINCUTOFF, DEFAULT_BETA);
                flickProgress = DEFAULT_FLICK_PROGRESS;
                flickSize = DEFAULT_FLICK_SIZE;
                flickAngleRemainder = DEFAULT_FLICK_ANGLE_REMAINDER;
            }
        }
        public static FlickStickMappingData[] flickMappingData = new FlickStickMappingData[Global.MAX_DS4_CONTROLLER_COUNT]
        {
            new FlickStickMappingData(), new FlickStickMappingData(), new FlickStickMappingData(),
            new FlickStickMappingData(), new FlickStickMappingData(), new FlickStickMappingData(),
            new FlickStickMappingData(), new FlickStickMappingData(),
        };

        public class TwoStageTriggerMappingData
        {
            public enum EngageButtonsMode : uint
            {
                None,
                SoftPullOnly,
                FullPullOnly,
                Both,
            }

            [Flags]
            public enum ActiveZoneButtons : ushort
            {
                None,
                SoftPull,
                FullPull
            }

            public bool startCheck;
            public DateTime checkTime;
            public bool outputActive;
            public bool softPullActActive;
            public bool fullPullActActive;
            public EngageButtonsMode actionStateMode = EngageButtonsMode.Both;
            public ActiveZoneButtons previousActiveButtons = ActiveZoneButtons.None;

            public void StartProcessing()
            {
                startCheck = true;
                checkTime = DateTime.Now;
                outputActive = false;
                softPullActActive = false;
                fullPullActActive = false;
                actionStateMode = EngageButtonsMode.Both;
                previousActiveButtons = ActiveZoneButtons.None;
            }

            public void Reset()
            {
                checkTime = DateTime.Now;
                startCheck = false;
                outputActive = false;
                softPullActActive = false;
                fullPullActActive = false;
                actionStateMode = EngageButtonsMode.Both;
                previousActiveButtons = ActiveZoneButtons.None;
            }
        }

        public static TwoStageTriggerMappingData[] l2TwoStageMappingData = new TwoStageTriggerMappingData[Global.MAX_DS4_CONTROLLER_COUNT]
        {
            new TwoStageTriggerMappingData(), new TwoStageTriggerMappingData(), new TwoStageTriggerMappingData(),
            new TwoStageTriggerMappingData(), new TwoStageTriggerMappingData(), new TwoStageTriggerMappingData(), 
            new TwoStageTriggerMappingData(), new TwoStageTriggerMappingData(),
        };

        public static TwoStageTriggerMappingData[] r2TwoStageMappingData = new TwoStageTriggerMappingData[Global.MAX_DS4_CONTROLLER_COUNT]
        {
            new TwoStageTriggerMappingData(), new TwoStageTriggerMappingData(), new TwoStageTriggerMappingData(),
            new TwoStageTriggerMappingData(), new TwoStageTriggerMappingData(), new TwoStageTriggerMappingData(),
            new TwoStageTriggerMappingData(), new TwoStageTriggerMappingData(),
        };

        static ReaderWriterLockSlim syncStateLock = new ReaderWriterLockSlim();

        public static SyntheticState globalState = new SyntheticState();
        public static SyntheticState[] deviceState = new SyntheticState[Global.MAX_DS4_CONTROLLER_COUNT]
            { new SyntheticState(), new SyntheticState(), new SyntheticState(),
              new SyntheticState(), new SyntheticState(), new SyntheticState(), new SyntheticState(), new SyntheticState() };

        public static DS4StateFieldMapping[] fieldMappings = new DS4StateFieldMapping[Global.MAX_DS4_CONTROLLER_COUNT] {
            new DS4StateFieldMapping(), new DS4StateFieldMapping(), new DS4StateFieldMapping(),
            new DS4StateFieldMapping(), new DS4StateFieldMapping(), new DS4StateFieldMapping(),
            new DS4StateFieldMapping(), new DS4StateFieldMapping(),
        };
        public static DS4StateFieldMapping[] outputFieldMappings = new DS4StateFieldMapping[Global.MAX_DS4_CONTROLLER_COUNT]
        {
            new DS4StateFieldMapping(), new DS4StateFieldMapping(), new DS4StateFieldMapping(),
            new DS4StateFieldMapping(), new DS4StateFieldMapping(), new DS4StateFieldMapping(),
            new DS4StateFieldMapping(), new DS4StateFieldMapping(),
        };
        public static DS4StateFieldMapping[] previousFieldMappings = new DS4StateFieldMapping[Global.MAX_DS4_CONTROLLER_COUNT]
        {
            new DS4StateFieldMapping(), new DS4StateFieldMapping(), new DS4StateFieldMapping(),
            new DS4StateFieldMapping(), new DS4StateFieldMapping(), new DS4StateFieldMapping(),
            new DS4StateFieldMapping(), new DS4StateFieldMapping(),
        };

        // TODO When we disconnect, process a null/dead state to release any keys or buttons.
        public static DateTime oldnow = DateTime.UtcNow;
        private static bool pressagain = false;
        private static int wheel = 0, keyshelddown = 0;

        //mapcustom
        public static bool[] pressedonce = new bool[261], macrodone = new bool[39];
        static bool[] macroControl = new bool[25];
        static uint macroCount = 0;
        static Dictionary<string, Task>[] macroTaskQueue = new Dictionary<string, Task>[Global.MAX_DS4_CONTROLLER_COUNT] { new Dictionary<string, Task>(), new Dictionary<string, Task>(), new Dictionary<string, Task>(), new Dictionary<string, Task>(), new Dictionary<string, Task>(), new Dictionary<string, Task>(), new Dictionary<string, Task>(), new Dictionary<string, Task>() };

        //actions
        public static bool[] extrasRumbleActive = new bool[Global.MAX_DS4_CONTROLLER_COUNT];
        public static int[] fadetimer = new int[Global.MAX_DS4_CONTROLLER_COUNT] { 0, 0, 0, 0, 0, 0, 0, 0 };
        public static int[] prevFadetimer = new int[Global.MAX_DS4_CONTROLLER_COUNT] { 0, 0, 0, 0, 0, 0, 0, 0 };
        public static DS4Color[] lastColor = new DS4Color[Global.MAX_DS4_CONTROLLER_COUNT];
        public static List<ActionState> actionDone = new List<ActionState>();
        public static SpecialAction[] untriggeraction = new SpecialAction[Global.MAX_DS4_CONTROLLER_COUNT];
        public static DateTime[] nowAction = { DateTime.MinValue, DateTime.MinValue, DateTime.MinValue, DateTime.MinValue };
        public static DateTime[] oldnowAction = { DateTime.MinValue, DateTime.MinValue, DateTime.MinValue, DateTime.MinValue };
        public static int[] untriggerindex = new int[Global.MAX_DS4_CONTROLLER_COUNT] { -1, -1, -1, -1, -1, -1, -1, -1 };
        public static DateTime[] oldnowKeyAct = new DateTime[Global.MAX_DS4_CONTROLLER_COUNT] { DateTime.MinValue,
            DateTime.MinValue, DateTime.MinValue, DateTime.MinValue, DateTime.MinValue, DateTime.MinValue, DateTime.MinValue, DateTime.MinValue };

        private static DS4Controls[] shiftTriggerMapping = new DS4Controls[28] { DS4Controls.None, DS4Controls.Cross, DS4Controls.Circle, DS4Controls.Square,
            DS4Controls.Triangle, DS4Controls.Options, DS4Controls.Share, DS4Controls.DpadUp, DS4Controls.DpadDown,
            DS4Controls.DpadLeft, DS4Controls.DpadRight, DS4Controls.PS, DS4Controls.L1, DS4Controls.R1, DS4Controls.L2,
            DS4Controls.R2, DS4Controls.L3, DS4Controls.R3, DS4Controls.TouchLeft, DS4Controls.TouchUpper, DS4Controls.TouchMulti,
            DS4Controls.TouchRight, DS4Controls.GyroZNeg, DS4Controls.GyroZPos, DS4Controls.GyroXPos, DS4Controls.GyroXNeg,
            DS4Controls.None, DS4Controls.Mute,
        };

        // Button to index mapping used for macrodone array. Not even sure this
        // is needed. This was originally made to replace a switch test used in the DS4ControlToInt method.
        private static int[] ds4ControlMapping = new int[39] { 0, // DS4Control.None
            16, // DS4Controls.LXNeg
            20, // DS4Controls.LXPos
            17, // DS4Controls.LYNeg
            21, // DS4Controls.LYPos
            18, // DS4Controls.RXNeg
            22, // DS4Controls.RXPos
            19, // DS4Controls.RYNeg
            23, // DS4Controls.RYPos
            3,  // DS4Controls.L1
            24, // DS4Controls.L2
            5,  // DS4Controls.L3
            4,  // DS4Controls.R1
            25, // DS4Controls.R2
            6,  // DS4Controls.R3
            13, // DS4Controls.Square
            14, // DS4Controls.Triangle
            15, // DS4Controls.Circle
            12, // DS4Controls.Cross
            7,  // DS4Controls.DpadUp
            10, // DS4Controls.DpadRight
            8,  // DS4Controls.DpadDown
            9,  // DS4Controls.DpadLeft
            11, // DS4Controls.PS
            27, // DS4Controls.TouchLeft
            29, // DS4Controls.TouchUpper
            26, // DS4Controls.TouchMulti
            28, // DS4Controls.TouchRight
            1,  // DS4Controls.Share
            2,  // DS4Controls.Options
            30, // DS4Controls.Mute
            32, // DS4Controls.GyroXPos
            31, // DS4Controls.GyroXNeg
            34, // DS4Controls.GyroZPos
            33, // DS4Controls.GyroZNeg
            35, // DS4Controls.SwipeLeft
            36, // DS4Controls.SwipeRight
            37, // DS4Controls.SwipeUp
            38  // DS4Controls.SwipeDown
        };

        // Define here to save some time processing.
        // It is enough to feel a difference during gameplay.
        // 201907: Commented out these temp variables because those were not actually used anymore (value was assigned but it was never used anywhere)
        //private static int[] rsOutCurveModeArray = new int[4] { 0, 0, 0, 0 };
        //private static int[] lsOutCurveModeArray = new int[4] { 0, 0, 0, 0 };
        //static bool tempBool = false;
        //private static double[] tempDoubleArray = new double[4] { 0.0, 0.0, 0.0, 0.0 };
        //private static int[] tempIntArray = new int[4] { 0, 0, 0, 0 };

        // Special macros
        static bool altTabDone = true;
        static DateTime altTabNow = DateTime.UtcNow,
            oldAltTabNow = DateTime.UtcNow - TimeSpan.FromSeconds(1);

        // Mouse
        public static int mcounter = 34;
        public static int mouseaccel = 0;
        public static int prevmouseaccel = 0;
        private static double horizontalRemainder = 0.0, verticalRemainder = 0.0;
        public const int MOUSESPEEDFACTOR = 48;
        private const double MOUSESTICKANTIOFFSET = 0.0128;
        private const double MOUSESTICKMINVELOCITY = 67.5;
        //private const double MOUSESTICKMINVELOCITY = 40.0;

        public static void Commit(int device)
        {
            SyntheticState state = deviceState[device];
            syncStateLock.EnterWriteLock();

            globalState.currentClicks.leftCount += state.currentClicks.leftCount - state.previousClicks.leftCount;
            globalState.currentClicks.middleCount += state.currentClicks.middleCount - state.previousClicks.middleCount;
            globalState.currentClicks.rightCount += state.currentClicks.rightCount - state.previousClicks.rightCount;
            globalState.currentClicks.fourthCount += state.currentClicks.fourthCount - state.previousClicks.fourthCount;
            globalState.currentClicks.fifthCount += state.currentClicks.fifthCount - state.previousClicks.fifthCount;
            globalState.currentClicks.wUpCount += state.currentClicks.wUpCount - state.previousClicks.wUpCount;
            globalState.currentClicks.wDownCount += state.currentClicks.wDownCount - state.previousClicks.wDownCount;
            globalState.currentClicks.toggleCount += state.currentClicks.toggleCount - state.previousClicks.toggleCount;
            globalState.currentClicks.toggle = state.currentClicks.toggle;

            if (globalState.currentClicks.toggleCount != 0 && globalState.previousClicks.toggleCount == 0 && globalState.currentClicks.toggle)
            {
                if (globalState.currentClicks.leftCount != 0 && globalState.previousClicks.leftCount == 0)
                    InputMethods.MouseEvent(InputMethods.MOUSEEVENTF_LEFTDOWN);
                if (globalState.currentClicks.rightCount != 0 && globalState.previousClicks.rightCount == 0)
                    InputMethods.MouseEvent(InputMethods.MOUSEEVENTF_RIGHTDOWN);
                if (globalState.currentClicks.middleCount != 0 && globalState.previousClicks.middleCount == 0)
                    InputMethods.MouseEvent(InputMethods.MOUSEEVENTF_MIDDLEDOWN);
                if (globalState.currentClicks.fourthCount != 0 && globalState.previousClicks.fourthCount == 0)
                    InputMethods.MouseEvent(InputMethods.MOUSEEVENTF_XBUTTONDOWN, 1);
                if (globalState.currentClicks.fifthCount != 0 && globalState.previousClicks.fifthCount == 0)
                    InputMethods.MouseEvent(InputMethods.MOUSEEVENTF_XBUTTONDOWN, 2);
            }
            else if (globalState.currentClicks.toggleCount != 0 && globalState.previousClicks.toggleCount == 0 && !globalState.currentClicks.toggle)
            {
                if (globalState.currentClicks.leftCount != 0 && globalState.previousClicks.leftCount == 0)
                    InputMethods.MouseEvent(InputMethods.MOUSEEVENTF_LEFTUP);
                if (globalState.currentClicks.rightCount != 0 && globalState.previousClicks.rightCount == 0)
                    InputMethods.MouseEvent(InputMethods.MOUSEEVENTF_RIGHTUP);
                if (globalState.currentClicks.middleCount != 0 && globalState.previousClicks.middleCount == 0)
                    InputMethods.MouseEvent(InputMethods.MOUSEEVENTF_MIDDLEUP);
                if (globalState.currentClicks.fourthCount != 0 && globalState.previousClicks.fourthCount == 0)
                    InputMethods.MouseEvent(InputMethods.MOUSEEVENTF_XBUTTONUP, 1);
                if (globalState.currentClicks.fifthCount != 0 && globalState.previousClicks.fifthCount == 0)
                    InputMethods.MouseEvent(InputMethods.MOUSEEVENTF_XBUTTONUP, 2);
            }

            if (globalState.currentClicks.toggleCount == 0 && globalState.previousClicks.toggleCount == 0)
            {
                if (globalState.currentClicks.leftCount != 0 && globalState.previousClicks.leftCount == 0)
                    InputMethods.MouseEvent(InputMethods.MOUSEEVENTF_LEFTDOWN);
                else if (globalState.currentClicks.leftCount == 0 && globalState.previousClicks.leftCount != 0)
                    InputMethods.MouseEvent(InputMethods.MOUSEEVENTF_LEFTUP);

                if (globalState.currentClicks.middleCount != 0 && globalState.previousClicks.middleCount == 0)
                    InputMethods.MouseEvent(InputMethods.MOUSEEVENTF_MIDDLEDOWN);
                else if (globalState.currentClicks.middleCount == 0 && globalState.previousClicks.middleCount != 0)
                    InputMethods.MouseEvent(InputMethods.MOUSEEVENTF_MIDDLEUP);

                if (globalState.currentClicks.rightCount != 0 && globalState.previousClicks.rightCount == 0)
                    InputMethods.MouseEvent(InputMethods.MOUSEEVENTF_RIGHTDOWN);
                else if (globalState.currentClicks.rightCount == 0 && globalState.previousClicks.rightCount != 0)
                    InputMethods.MouseEvent(InputMethods.MOUSEEVENTF_RIGHTUP);

                if (globalState.currentClicks.fourthCount != 0 && globalState.previousClicks.fourthCount == 0)
                    InputMethods.MouseEvent(InputMethods.MOUSEEVENTF_XBUTTONDOWN, 1);
                else if (globalState.currentClicks.fourthCount == 0 && globalState.previousClicks.fourthCount != 0)
                    InputMethods.MouseEvent(InputMethods.MOUSEEVENTF_XBUTTONUP, 1);

                if (globalState.currentClicks.fifthCount != 0 && globalState.previousClicks.fifthCount == 0)
                    InputMethods.MouseEvent(InputMethods.MOUSEEVENTF_XBUTTONDOWN, 2);
                else if (globalState.currentClicks.fifthCount == 0 && globalState.previousClicks.fifthCount != 0)
                    InputMethods.MouseEvent(InputMethods.MOUSEEVENTF_XBUTTONUP, 2);

                if (globalState.currentClicks.wUpCount != 0 && globalState.previousClicks.wUpCount == 0)
                {
                    InputMethods.MouseEvent(InputMethods.MOUSEEVENTF_WHEEL, 120);
                    oldnow = DateTime.UtcNow;
                    wheel = 120;
                }
                else if (globalState.currentClicks.wUpCount == 0 && globalState.previousClicks.wUpCount != 0)
                    wheel = 0;

                if (globalState.currentClicks.wDownCount != 0 && globalState.previousClicks.wDownCount == 0)
                {
                    InputMethods.MouseEvent(InputMethods.MOUSEEVENTF_WHEEL, -120);
                    oldnow = DateTime.UtcNow;
                    wheel = -120;
                }
                if (globalState.currentClicks.wDownCount == 0 && globalState.previousClicks.wDownCount != 0)
                    wheel = 0;
            }
            

            if (wheel != 0) //Continue mouse wheel movement
            {
                DateTime now = DateTime.UtcNow;
                if (now >= oldnow + TimeSpan.FromMilliseconds(100) && !pressagain)
                {
                    oldnow = now;
                    InputMethods.MouseEvent(InputMethods.MOUSEEVENTF_WHEEL, wheel);
                }
            }

            // Merge and synthesize all key presses/releases that are present in this device's mapping.
            // TODO what about the rest?  e.g. repeat keys really ought to be on some set schedule
            Dictionary<UInt16, SyntheticState.KeyPresses>.KeyCollection kvpKeys = state.keyPresses.Keys;
            //foreach (KeyValuePair<UInt16, SyntheticState.KeyPresses> kvp in state.keyPresses)
            //for (int i = 0, keyCount = kvpKeys.Count; i < keyCount; i++)
            for (var keyEnum = kvpKeys.GetEnumerator(); keyEnum.MoveNext();)
            {
                //UInt16 kvpKey = kvpKeys.ElementAt(i);
                UInt16 kvpKey = keyEnum.Current;
                SyntheticState.KeyPresses kvpValue = state.keyPresses[kvpKey];

                SyntheticState.KeyPresses gkp;
                if (globalState.keyPresses.TryGetValue(kvpKey, out gkp))
                {
                    gkp.current.vkCount += kvpValue.current.vkCount - kvpValue.previous.vkCount;
                    gkp.current.scanCodeCount += kvpValue.current.scanCodeCount - kvpValue.previous.scanCodeCount;
                    gkp.current.repeatCount += kvpValue.current.repeatCount - kvpValue.previous.repeatCount;
                    gkp.current.toggle = kvpValue.current.toggle;
                    gkp.current.toggleCount += kvpValue.current.toggleCount - kvpValue.previous.toggleCount;
                }
                else
                {
                    gkp = new SyntheticState.KeyPresses();
                    gkp.current = kvpValue.current;
                    globalState.keyPresses[kvpKey] = gkp;
                }
                if (gkp.current.toggleCount != 0 && gkp.previous.toggleCount == 0 && gkp.current.toggle)
                {
                    if (gkp.current.scanCodeCount != 0)
                        InputMethods.performSCKeyPress(kvpKey);
                    else
                        InputMethods.performKeyPress(kvpKey);
                }
                else if (gkp.current.toggleCount != 0 && gkp.previous.toggleCount == 0 && !gkp.current.toggle)
                {
                    if (gkp.previous.scanCodeCount != 0) // use the last type of VK/SC
                        InputMethods.performSCKeyRelease(kvpKey);
                    else
                        InputMethods.performKeyRelease(kvpKey);
                }
                else if (gkp.current.vkCount + gkp.current.scanCodeCount != 0 && gkp.previous.vkCount + gkp.previous.scanCodeCount == 0)
                {
                    if (gkp.current.scanCodeCount != 0)
                    {
                        oldnow = DateTime.UtcNow;
                        InputMethods.performSCKeyPress(kvpKey);
                        pressagain = false;
                        keyshelddown = kvpKey;
                    }
                    else
                    {
                        oldnow = DateTime.UtcNow;
                        InputMethods.performKeyPress(kvpKey);
                        pressagain = false;
                        keyshelddown = kvpKey;
                    }
                }
                else if (gkp.current.toggleCount != 0 || gkp.previous.toggleCount != 0 || gkp.current.repeatCount != 0 || // repeat or SC/VK transition
                    ((gkp.previous.scanCodeCount == 0) != (gkp.current.scanCodeCount == 0))) //repeat keystroke after 500ms
                {
                    if (keyshelddown == kvpKey)
                    {
                        DateTime now = DateTime.UtcNow;
                        if (now >= oldnow + TimeSpan.FromMilliseconds(500) && !pressagain)
                        {
                            oldnow = now;
                            pressagain = true;
                        }
                        if (pressagain && gkp.current.scanCodeCount != 0)
                        {
                            now = DateTime.UtcNow;
                            if (now >= oldnow + TimeSpan.FromMilliseconds(25) && pressagain)
                            {
                                oldnow = now;
                                InputMethods.performSCKeyPress(kvpKey);
                            }
                        }
                        else if (pressagain)
                        {
                            now = DateTime.UtcNow;
                            if (now >= oldnow + TimeSpan.FromMilliseconds(25) && pressagain)
                            {
                                oldnow = now;
                                InputMethods.performKeyPress(kvpKey);
                            }
                        }
                    }
                }
                if ((gkp.current.toggleCount == 0 && gkp.previous.toggleCount == 0) && gkp.current.vkCount + gkp.current.scanCodeCount == 0 && gkp.previous.vkCount + gkp.previous.scanCodeCount != 0)
                {
                    if (gkp.previous.scanCodeCount != 0) // use the last type of VK/SC
                    {
                        InputMethods.performSCKeyRelease(kvpKey);
                        pressagain = false;
                    }
                    else
                    {
                        InputMethods.performKeyRelease(kvpKey);
                        pressagain = false;
                    }
                }
            }
            globalState.SaveToPrevious(false);

            syncStateLock.ExitWriteLock();
            state.SaveToPrevious(true);
        }

        public enum Click { None, Left, Middle, Right, Fourth, Fifth, WUP, WDOWN };
        public static void MapClick(int device, Click mouseClick)
        {
            switch (mouseClick)
            {
                case Click.Left:
                    deviceState[device].currentClicks.leftCount++;
                    break;
                case Click.Middle:
                    deviceState[device].currentClicks.middleCount++;
                    break;
                case Click.Right:
                    deviceState[device].currentClicks.rightCount++;
                    break;
                case Click.Fourth:
                    deviceState[device].currentClicks.fourthCount++;
                    break;
                case Click.Fifth:
                    deviceState[device].currentClicks.fifthCount++;
                    break;
                case Click.WUP:
                    deviceState[device].currentClicks.wUpCount++;
                    break;
                case Click.WDOWN:
                    deviceState[device].currentClicks.wDownCount++;
                    break;
                default: break;
            }
        }

        public static int DS4ControltoInt(DS4Controls ctrl)
        {
            int result = 0;
            if (ctrl >= DS4Controls.None && ctrl <= DS4Controls.SwipeDown)
            {
                result = ds4ControlMapping[(int)ctrl];
            }

            return result;
        }

        static double TValue(double value1, double value2, double percent)
        {
            percent /= 100f;
            return value1 * percent + value2 * (1 - percent);
        }

        private static int ClampInt(int min, int value, int max)
        {
            return (value < min) ? min : (value > max) ? max : value;
        }

        public static DS4State SetCurveAndDeadzone(int device, DS4State cState, DS4State dState)
        {
            double rotation = /*tempDoubleArray[device] =*/  getLSRotation(device);
            if (rotation > 0.0 || rotation < 0.0)
                cState.rotateLSCoordinates(rotation);

            double rotationRS = /*tempDoubleArray[device] =*/ getRSRotation(device);
            if (rotationRS > 0.0 || rotationRS < 0.0)
                cState.rotateRSCoordinates(rotationRS);

            StickDeadZoneInfo lsMod = GetLSDeadInfo(device);
            StickDeadZoneInfo rsMod = GetRSDeadInfo(device);

            if (lsMod.fuzz > 0)
            {
                CalcStickAxisFuzz(device, 0, lsMod.fuzz, cState.LX, cState.LY, out cState.LX, out cState.LY);
            }

            if (rsMod.fuzz > 0)
            {
                CalcStickAxisFuzz(device, 1, rsMod.fuzz, cState.RX, cState.RY, out cState.RX, out cState.RY);
            }

            cState.CopyTo(dState);
            //DS4State dState = new DS4State(cState);
            int x;
            int y;
            int curve;

            /* TODO: Look into curve options and make sure maximum axes values are being respected */
            int lsCurve = getLSCurve(device);
            if (lsCurve > 0)
            {
                x = cState.LX;
                y = cState.LY;
                float max = x + y;
                double curvex;
                double curvey;
                curve = lsCurve;
                double multimax = TValue(382.5, max, curve);
                double multimin = TValue(127.5, max, curve);
                if ((x > 127.5f && y > 127.5f) || (x < 127.5f && y < 127.5f))
                {
                    curvex = (x > 127.5f ? Math.Min(x, (x / max) * multimax) : Math.Max(x, (x / max) * multimin));
                    curvey = (y > 127.5f ? Math.Min(y, (y / max) * multimax) : Math.Max(y, (y / max) * multimin));
                }
                else
                {
                    if (x < 127.5f)
                    {
                        curvex = Math.Min(x, (x / max) * multimax);
                        curvey = Math.Min(y, (-(y / max) * multimax + 510));
                    }
                    else
                    {
                        curvex = Math.Min(x, (-(x / max) * multimax + 510));
                        curvey = Math.Min(y, (y / max) * multimax);
                    }
                }

                dState.LX = (byte)Math.Round(curvex, 0);
                dState.LY = (byte)Math.Round(curvey, 0);
            }

            /* TODO: Look into curve options and make sure maximum axes values are being respected */
            int rsCurve = getRSCurve(device);
            if (rsCurve > 0)
            {
                x = cState.RX;
                y = cState.RY;
                float max = x + y;
                double curvex;
                double curvey;
                curve = rsCurve;
                double multimax = TValue(382.5, max, curve);
                double multimin = TValue(127.5, max, curve);
                if ((x > 127.5f && y > 127.5f) || (x < 127.5f && y < 127.5f))
                {
                    curvex = (x > 127.5f ? Math.Min(x, (x / max) * multimax) : Math.Max(x, (x / max) * multimin));
                    curvey = (y > 127.5f ? Math.Min(y, (y / max) * multimax) : Math.Max(y, (y / max) * multimin));
                }
                else
                {
                    if (x < 127.5f)
                    {
                        curvex = Math.Min(x, (x / max) * multimax);
                        curvey = Math.Min(y, (-(y / max) * multimax + 510));
                    }
                    else
                    {
                        curvex = Math.Min(x, (-(x / max) * multimax + 510));
                        curvey = Math.Min(y, (y / max) * multimax);
                    }
                }

                dState.RX = (byte)Math.Round(curvex, 0);
                dState.RY = (byte)Math.Round(curvey, 0);
            }

            /*int lsDeadzone = getLSDeadzone(device);
            int lsAntiDead = getLSAntiDeadzone(device);
            int lsMaxZone = getLSMaxzone(device);
            */
            int lsDeadzone = lsMod.deadZone;
            int lsAntiDead = lsMod.antiDeadZone;
            int lsMaxZone = lsMod.maxZone;
            double lsMaxOutput = lsMod.maxOutput;

            if (lsDeadzone > 0 || lsAntiDead > 0 || lsMaxZone != 100 || lsMaxOutput != 100.0)
            {
                double lsSquared = Math.Pow(cState.LX - 128f, 2) + Math.Pow(cState.LY - 128f, 2);
                double lsDeadzoneSquared = Math.Pow(lsDeadzone, 2);
                if (lsDeadzone > 0 && lsSquared <= lsDeadzoneSquared)
                {
                    dState.LX = 128;
                    dState.LY = 128;
                }
                else if ((lsDeadzone > 0 && lsSquared > lsDeadzoneSquared) || lsAntiDead > 0 || lsMaxZone != 100 || lsMaxOutput != 100.0)
                {
                    double r = Math.Atan2(-(dState.LY - 128.0), (dState.LX - 128.0));
                    double maxXValue = dState.LX >= 128.0 ? 127.0 : -128;
                    double maxYValue = dState.LY >= 128.0 ? 127.0 : -128;
                    double ratio = lsMaxZone / 100.0;
                    double maxOutRatio = lsMaxOutput / 100.0;

                    double maxZoneXNegValue = (ratio * -128) + 128;
                    double maxZoneXPosValue = (ratio * 127) + 128;
                    double maxZoneYNegValue = maxZoneXNegValue;
                    double maxZoneYPosValue = maxZoneXPosValue;
                    double maxZoneX = dState.LX >= 128.0 ? (maxZoneXPosValue - 128.0) : (maxZoneXNegValue - 128.0);
                    double maxZoneY = dState.LY >= 128.0 ? (maxZoneYPosValue - 128.0) : (maxZoneYNegValue - 128.0);

                    double tempLsXDead = 0.0, tempLsYDead = 0.0;
                    double tempOutputX = 0.0, tempOutputY = 0.0;
                    if (lsDeadzone > 0)
                    {
                        tempLsXDead = Math.Abs(Math.Cos(r)) * (lsDeadzone / 127.0) * maxXValue;
                        tempLsYDead = Math.Abs(Math.Sin(r)) * (lsDeadzone / 127.0) * maxYValue;

                        if (lsSquared > lsDeadzoneSquared)
                        {
                            double currentX = Global.Clamp(maxZoneXNegValue, dState.LX, maxZoneXPosValue);
                            double currentY = Global.Clamp(maxZoneYNegValue, dState.LY, maxZoneYPosValue);
                            tempOutputX = ((currentX - 128.0 - tempLsXDead) / (maxZoneX - tempLsXDead));
                            tempOutputY = ((currentY - 128.0 - tempLsYDead) / (maxZoneY - tempLsYDead));
                        }
                    }
                    else
                    {
                        double currentX = Global.Clamp(maxZoneXNegValue, dState.LX, maxZoneXPosValue);
                        double currentY = Global.Clamp(maxZoneYNegValue, dState.LY, maxZoneYPosValue);
                        tempOutputX = (currentX - 128.0) / maxZoneX;
                        tempOutputY = (currentY - 128.0) / maxZoneY;
                    }

                    if (lsMaxOutput != 100.0)
                    {
                        double maxOutXRatio = Math.Abs(Math.Cos(r)) * maxOutRatio;
                        double maxOutYRatio = Math.Abs(Math.Sin(r)) * maxOutRatio;
                        tempOutputX = Math.Min(Math.Max(tempOutputX, 0.0), maxOutXRatio);
                        tempOutputY = Math.Min(Math.Max(tempOutputY, 0.0), maxOutYRatio);
                    }

                    double tempLsXAntiDeadPercent = 0.0, tempLsYAntiDeadPercent = 0.0;
                    if (lsAntiDead > 0)
                    {
                        tempLsXAntiDeadPercent = (lsAntiDead * 0.01) * Math.Abs(Math.Cos(r));
                        tempLsYAntiDeadPercent = (lsAntiDead * 0.01) * Math.Abs(Math.Sin(r));
                    }

                    if (tempOutputX > 0.0)
                    {
                        dState.LX = (byte)((((1.0 - tempLsXAntiDeadPercent) * tempOutputX + tempLsXAntiDeadPercent)) * maxXValue + 128.0);
                    }
                    else
                    {
                        dState.LX = 128;
                    }

                    if (tempOutputY > 0.0)
                    {
                        dState.LY = (byte)((((1.0 - tempLsYAntiDeadPercent) * tempOutputY + tempLsYAntiDeadPercent)) * maxYValue + 128.0);
                    }
                    else
                    {
                        dState.LY = 128;
                    }
                }
            }

            /*int rsDeadzone = getRSDeadzone(device);
            int rsAntiDead = getRSAntiDeadzone(device);
            int rsMaxZone = getRSMaxzone(device);
            */
            int rsDeadzone = rsMod.deadZone;
            int rsAntiDead = rsMod.antiDeadZone;
            int rsMaxZone = rsMod.maxZone;
            double rsMaxOutput = rsMod.maxOutput;

            if (rsDeadzone > 0 || rsAntiDead > 0 || rsMaxZone != 100 || rsMaxOutput != 100.0)
            {
                double rsSquared = Math.Pow(cState.RX - 128.0, 2) + Math.Pow(cState.RY - 128.0, 2);
                double rsDeadzoneSquared = Math.Pow(rsDeadzone, 2);
                if (rsDeadzone > 0 && rsSquared <= rsDeadzoneSquared)
                {
                    dState.RX = 128;
                    dState.RY = 128;
                }
                else if ((rsDeadzone > 0 && rsSquared > rsDeadzoneSquared) || rsAntiDead > 0 || rsMaxZone != 100 || rsMaxOutput != 100.0)
                {
                    double r = Math.Atan2(-(dState.RY - 128.0), (dState.RX - 128.0));
                    double maxXValue = dState.RX >= 128.0 ? 127 : -128;
                    double maxYValue = dState.RY >= 128.0 ? 127 : -128;
                    double ratio = rsMaxZone / 100.0;
                    double maxOutRatio = rsMaxOutput / 100.0;

                    double maxZoneXNegValue = (ratio * -128.0) + 128.0;
                    double maxZoneXPosValue = (ratio * 127.0) + 128.0;
                    double maxZoneYNegValue = maxZoneXNegValue;
                    double maxZoneYPosValue = maxZoneXPosValue;
                    double maxZoneX = dState.RX >= 128.0 ? (maxZoneXPosValue - 128.0) : (maxZoneXNegValue - 128.0);
                    double maxZoneY = dState.RY >= 128.0 ? (maxZoneYPosValue - 128.0) : (maxZoneYNegValue - 128.0);

                    double tempRsXDead = 0.0, tempRsYDead = 0.0;
                    double tempOutputX = 0.0, tempOutputY = 0.0;
                    if (rsDeadzone > 0)
                    {
                        tempRsXDead = Math.Abs(Math.Cos(r)) * (rsDeadzone / 127.0) * maxXValue;
                        tempRsYDead = Math.Abs(Math.Sin(r)) * (rsDeadzone / 127.0) * maxYValue;

                        if (rsSquared > rsDeadzoneSquared)
                        {
                            double currentX = Global.Clamp(maxZoneXNegValue, dState.RX, maxZoneXPosValue);
                            double currentY = Global.Clamp(maxZoneYNegValue, dState.RY, maxZoneYPosValue);

                            tempOutputX = ((currentX - 128.0 - tempRsXDead) / (maxZoneX - tempRsXDead));
                            tempOutputY = ((currentY - 128.0 - tempRsYDead) / (maxZoneY - tempRsYDead));
                        }
                    }
                    else
                    {
                        double currentX = Global.Clamp(maxZoneXNegValue, dState.RX, maxZoneXPosValue);
                        double currentY = Global.Clamp(maxZoneYNegValue, dState.RY, maxZoneYPosValue);

                        tempOutputX = (currentX - 128.0) / maxZoneX;
                        tempOutputY = (currentY - 128.0) / maxZoneY;
                    }

                    if (rsMaxOutput != 100.0)
                    {
                        double maxOutXRatio = Math.Abs(Math.Cos(r)) * maxOutRatio;
                        double maxOutYRatio = Math.Abs(Math.Sin(r)) * maxOutRatio;
                        tempOutputX = Math.Min(Math.Max(tempOutputX, 0.0), maxOutXRatio);
                        tempOutputY = Math.Min(Math.Max(tempOutputY, 0.0), maxOutYRatio);
                    }

                    double tempRsXAntiDeadPercent = 0.0, tempRsYAntiDeadPercent = 0.0;
                    if (rsAntiDead > 0)
                    {
                        tempRsXAntiDeadPercent = (rsAntiDead * 0.01) * Math.Abs(Math.Cos(r));
                        tempRsYAntiDeadPercent = (rsAntiDead * 0.01) * Math.Abs(Math.Sin(r));
                    }

                    if (tempOutputX > 0.0)
                    {
                        dState.RX = (byte)((((1.0 - tempRsXAntiDeadPercent) * tempOutputX + tempRsXAntiDeadPercent)) * maxXValue + 128.0);
                    }
                    else
                    {
                        dState.RX = 128;
                    }

                    if (tempOutputY > 0.0)
                    {
                        dState.RY = (byte)((((1.0 - tempRsYAntiDeadPercent) * tempOutputY + tempRsYAntiDeadPercent)) * maxYValue + 128.0);
                    }
                    else
                    {
                        dState.RY = 128;
                    }
                }
            }

            /*byte l2Deadzone = getL2Deadzone(device);
            int l2AntiDeadzone = getL2AntiDeadzone(device);
            int l2Maxzone = getL2Maxzone(device);
            */

            TriggerDeadZoneZInfo l2ModInfo = GetL2ModInfo(device);
            byte l2Deadzone = l2ModInfo.deadZone;
            int l2AntiDeadzone = l2ModInfo.antiDeadZone;
            int l2Maxzone = l2ModInfo.maxZone;
            double l2MaxOutput = l2ModInfo.maxOutput;
            if (l2Deadzone > 0 || l2AntiDeadzone > 0 || l2Maxzone != 100 || l2MaxOutput != 100.0)
            {
                double tempL2Output = cState.L2 / 255.0;
                double tempL2AntiDead = 0.0;
                double ratio = l2Maxzone / 100.0;
                double maxValue = 255.0 * ratio;

                if (l2Deadzone > 0)
                {
                    if (cState.L2 > l2Deadzone)
                    {
                        double current = Global.Clamp(0, dState.L2, maxValue);
                        tempL2Output = (current - l2Deadzone) / (maxValue - l2Deadzone);
                    }
                    else
                    {
                        tempL2Output = 0.0;
                    }
                }
                else
                {
                    double current = Global.Clamp(0, dState.L2, maxValue);
                    tempL2Output = current / maxValue;
                }

                if (l2MaxOutput != 100.0)
                {
                    double maxOutRatio = l2MaxOutput / 100.0;
                    tempL2Output = Math.Min(Math.Max(tempL2Output, 0.0), maxOutRatio);
                }

                if (l2AntiDeadzone > 0)
                {
                    tempL2AntiDead = l2AntiDeadzone * 0.01;
                }

                if (tempL2Output > 0.0)
                {
                    dState.L2 = (byte)(((1.0 - tempL2AntiDead) * tempL2Output + tempL2AntiDead) * 255.0);
                }
                else
                {
                    dState.L2 = 0;
                }
            }

            /*byte r2Deadzone = getR2Deadzone(device);
            int r2AntiDeadzone = getR2AntiDeadzone(device);
            int r2Maxzone = getR2Maxzone(device);
            */
            TriggerDeadZoneZInfo r2ModInfo = GetR2ModInfo(device);
            byte r2Deadzone = r2ModInfo.deadZone;
            int r2AntiDeadzone = r2ModInfo.antiDeadZone;
            int r2Maxzone = r2ModInfo.maxZone;
            double r2MaxOutput = r2ModInfo.maxOutput;
            if (r2Deadzone > 0 || r2AntiDeadzone > 0 || r2Maxzone != 100 || r2MaxOutput != 100.0)
            {
                double tempR2Output = cState.R2 / 255.0;
                double tempR2AntiDead = 0.0;
                double ratio = r2Maxzone / 100.0;
                double maxValue = 255 * ratio;

                if (r2Deadzone > 0)
                {
                    if (cState.R2 > r2Deadzone)
                    {
                        double current = Global.Clamp(0, dState.R2, maxValue);
                        tempR2Output = (current - r2Deadzone) / (maxValue - r2Deadzone);
                    }
                    else
                    {
                        tempR2Output = 0.0;
                    }
                }
                else
                {
                    double current = Global.Clamp(0, dState.R2, maxValue);
                    tempR2Output = current / maxValue;
                }

                if (r2MaxOutput != 100.0)
                {
                    double maxOutRatio = r2MaxOutput / 100.0;
                    tempR2Output = Math.Min(Math.Max(tempR2Output, 0.0), maxOutRatio);
                }

                if (r2AntiDeadzone > 0)
                {
                    tempR2AntiDead = r2AntiDeadzone * 0.01;
                }

                if (tempR2Output > 0.0)
                {
                    dState.R2 = (byte)(((1.0 - tempR2AntiDead) * tempR2Output + tempR2AntiDead) * 255.0);
                }
                else
                {
                    dState.R2 = 0;
                }
            }

            double lsSens = getLSSens(device);
            if (lsSens != 1.0)
            {
                dState.LX = (byte)Global.Clamp(0, lsSens * (dState.LX - 128.0) + 128.0, 255);
                dState.LY = (byte)Global.Clamp(0, lsSens * (dState.LY - 128.0) + 128.0, 255);
            }

            double rsSens = getRSSens(device);
            if (rsSens != 1.0)
            {
                dState.RX = (byte)Global.Clamp(0, rsSens * (dState.RX - 128.0) + 128.0, 255);
                dState.RY = (byte)Global.Clamp(0, rsSens * (dState.RY - 128.0) + 128.0, 255);
            }

            double l2Sens = getL2Sens(device);
            if (l2Sens != 1.0)
                dState.L2 = (byte)Global.Clamp(0, l2Sens * dState.L2, 255);

            double r2Sens = getR2Sens(device);
            if (r2Sens != 1.0)
                dState.R2 = (byte)Global.Clamp(0, r2Sens * dState.R2, 255);

            SquareStickInfo squStk = GetSquareStickInfo(device);
            if (squStk.lsMode && (dState.LX != 128 || dState.LY != 128))
            {
                double capX = dState.LX >= 128 ? 127.0 : 128.0;
                double capY = dState.LY >= 128 ? 127.0 : 128.0;
                double tempX = (dState.LX - 128.0) / capX;
                double tempY = (dState.LY - 128.0) / capY;
                DS4SquareStick sqstick = outSqrStk[device];
                sqstick.current.x = tempX; sqstick.current.y = tempY;
                sqstick.CircleToSquare(squStk.lsRoundness);
                //Console.WriteLine("Input ({0}) | Output ({1})", tempY, sqstick.current.y);
                tempX = sqstick.current.x < -1.0 ? -1.0 : sqstick.current.x > 1.0
                    ? 1.0 : sqstick.current.x;
                tempY = sqstick.current.y < -1.0 ? -1.0 : sqstick.current.y > 1.0
                    ? 1.0 : sqstick.current.y;
                dState.LX = (byte)(tempX * capX + 128.0);
                dState.LY = (byte)(tempY * capY + 128.0);
            }

            int lsOutCurveMode = getLsOutCurveMode(device);
            if (lsOutCurveMode > 0 && (dState.LX != 128 || dState.LY != 128))
            {
                double r = Math.Atan2(-(dState.LY - 128.0), (dState.LX - 128.0));
                double maxOutXRatio = Math.Abs(Math.Cos(r));
                double maxOutYRatio = Math.Abs(Math.Sin(r));
                double sideX = dState.LX - 128; double sideY = dState.LY - 128.0;
                double capX = dState.LX >= 128 ? maxOutXRatio * 127.0 : maxOutXRatio * 128.0;
                double capY = dState.LY >= 128 ? maxOutYRatio * 127.0 : maxOutYRatio * 128.0;
                double absSideX = Math.Abs(sideX); double absSideY = Math.Abs(sideY);
                if (absSideX > capX) capX = absSideX;
                if (absSideY > capY) capY = absSideY;
                double tempRatioX = capX > 0 ? (dState.LX - 128.0) / capX : 0;
                double tempRatioY = capY > 0 ? (dState.LY - 128.0) / capY : 0;
                double signX = tempRatioX >= 0.0 ? 1.0 : -1.0;
                double signY = tempRatioY >= 0.0 ? 1.0 : -1.0;

                if (lsOutCurveMode == 1)
                {
                    double absX = Math.Abs(tempRatioX);
                    double absY = Math.Abs(tempRatioY);
                    double outputX = 0.0;
                    double outputY = 0.0;

                    if (absX <= 0.4)
                    {
                        outputX = 0.8 * absX;
                    }
                    else if (absX <= 0.75)
                    {
                        outputX = absX - 0.08;
                    }
                    else if (absX > 0.75)
                    {
                        outputX = (absX * 1.32) - 0.32;
                    }

                    if (absY <= 0.4)
                    {
                        outputY = 0.8 * absY;
                    }
                    else if (absY <= 0.75)
                    {
                        outputY = absY - 0.08;
                    }
                    else if (absY > 0.75)
                    {
                        outputY = (absY * 1.32) - 0.32;
                    }

                    dState.LX = (byte)(outputX * signX * capX + 128.0);
                    dState.LY = (byte)(outputY * signY * capY + 128.0);
                }
                else if (lsOutCurveMode == 2)
                {
                    double outputX = tempRatioX * tempRatioX;
                    double outputY = tempRatioY * tempRatioY;
                    dState.LX = (byte)(outputX * signX * capX + 128.0);
                    dState.LY = (byte)(outputY * signY * capY + 128.0);
                }
                else if (lsOutCurveMode == 3)
                {
                    double outputX = tempRatioX * tempRatioX * tempRatioX;
                    double outputY = tempRatioY * tempRatioY * tempRatioY;
                    dState.LX = (byte)(outputX * capX + 128.0);
                    dState.LY = (byte)(outputY * capY + 128.0);
                }
                else if (lsOutCurveMode == 4)
                {
                    double absX = Math.Abs(tempRatioX);
                    double absY = Math.Abs(tempRatioY);
                    double outputX = absX * (absX - 2.0);
                    double outputY = absY * (absY - 2.0);
                    dState.LX = (byte)(-1.0 * outputX * signX * capX + 128.0);
                    dState.LY = (byte)(-1.0 * outputY * signY * capY + 128.0);
                }
                else if (lsOutCurveMode == 5)
                {
                    double innerX = Math.Abs(tempRatioX) - 1.0;
                    double innerY = Math.Abs(tempRatioY) - 1.0;
                    double outputX = innerX * innerX * innerX + 1.0;
                    double outputY = innerY * innerY * innerY + 1.0;
                    dState.LX = (byte)(1.0 * outputX * signX * capX + 128.0);
                    dState.LY = (byte)(1.0 * outputY * signY * capY + 128.0);
                }
                else if (lsOutCurveMode == 6)
                {
                    // Get max values and circular distance of axes
                    double maxX = (dState.LX >= 128 ? 127 : 128);
                    double maxY = (dState.LY >= 128 ? 127 : 128);
                    byte tempOutX = (byte)(tempRatioX * maxX + 128.0);
                    byte tempOutY = (byte)(tempRatioY * maxY + 128.0);

                    // Perform curve based on byte values from vector
                    byte tempX = lsOutBezierCurveObj[device].arrayBezierLUT[tempOutX];
                    byte tempY = lsOutBezierCurveObj[device].arrayBezierLUT[tempOutY];

                    // Calculate new ratio
                    double tempRatioOutX = (tempX - 128.0) / maxX;
                    double tempRatioOutY = (tempY - 128.0) / maxY;

                    // Map back to stick coordinates
                    dState.LX = (byte)(tempRatioOutX * capX + 128);
                    dState.LY = (byte)(tempRatioOutY * capY + 128);
                    //Console.WriteLine("X(I){0} X(O){1} {2} {3}", tempOutX, dState.LX, tempOutY, dState.LY);
                }
            }
            
            if (squStk.rsMode && (dState.RX != 128 || dState.RY != 128))
            {
                double capX = dState.RX >= 128 ? 127.0 : 128.0;
                double capY = dState.RY >= 128 ? 127.0 : 128.0;
                double tempX = (dState.RX - 128.0) / capX;
                double tempY = (dState.RY - 128.0) / capY;
                DS4SquareStick sqstick = outSqrStk[device];
                sqstick.current.x = tempX; sqstick.current.y = tempY;
                sqstick.CircleToSquare(squStk.rsRoundness);
                tempX = sqstick.current.x < -1.0 ? -1.0 : sqstick.current.x > 1.0
                    ? 1.0 : sqstick.current.x;
                tempY = sqstick.current.y < -1.0 ? -1.0 : sqstick.current.y > 1.0
                    ? 1.0 : sqstick.current.y;
                //Console.WriteLine("Input ({0}) | Output ({1})", tempY, sqstick.current.y);
                dState.RX = (byte)(tempX * capX + 128.0);
                dState.RY = (byte)(tempY * capY + 128.0);
            }

            int rsOutCurveMode = getRsOutCurveMode(device);
            if (rsOutCurveMode > 0 && (dState.RX != 128 || dState.RY != 128))
            {
                double r = Math.Atan2(-(dState.RY - 128.0), (dState.RX - 128.0));
                double maxOutXRatio = Math.Abs(Math.Cos(r));
                double maxOutYRatio = Math.Abs(Math.Sin(r));
                double sideX = dState.RX - 128; double sideY = dState.RY - 128.0;
                double capX = dState.RX >= 128 ? maxOutXRatio * 127.0 : maxOutXRatio * 128.0;
                double capY = dState.RY >= 128 ? maxOutYRatio * 127.0 : maxOutYRatio * 128.0;
                double absSideX = Math.Abs(sideX); double absSideY = Math.Abs(sideY);
                if (absSideX > capX) capX = absSideX;
                if (absSideY > capY) capY = absSideY;
                double tempRatioX = capX > 0 ? (dState.RX - 128.0) / capX : 0;
                double tempRatioY = capY > 0 ? (dState.RY - 128.0) / capY : 0;
                double signX = tempRatioX >= 0.0 ? 1.0 : -1.0;
                double signY = tempRatioY >= 0.0 ? 1.0 : -1.0;

                if (rsOutCurveMode == 1)
                {
                    double absX = Math.Abs(tempRatioX);
                    double absY = Math.Abs(tempRatioY);
                    double outputX = 0.0;
                    double outputY = 0.0;

                    if (absX <= 0.4)
                    {
                        outputX = 0.8 * absX;
                    }
                    else if (absX <= 0.75)
                    {
                        outputX = absX - 0.08;
                    }
                    else if (absX > 0.75)
                    {
                        outputX = (absX * 1.32) - 0.32;
                    }

                    if (absY <= 0.4)
                    {
                        outputY = 0.8 * absY;
                    }
                    else if (absY <= 0.75)
                    {
                        outputY = absY - 0.08;
                    }
                    else if (absY > 0.75)
                    {
                        outputY = (absY * 1.32) - 0.32;
                    }

                    dState.RX = (byte)(outputX * signX * capX + 128.0);
                    dState.RY = (byte)(outputY * signY * capY + 128.0);
                }
                else if (rsOutCurveMode == 2)
                {
                    double outputX = tempRatioX * tempRatioX;
                    double outputY = tempRatioY * tempRatioY;
                    dState.RX = (byte)(outputX * signX * capX + 128.0);
                    dState.RY = (byte)(outputY * signY * capY + 128.0);
                }
                else if (rsOutCurveMode == 3)
                {
                    double outputX = tempRatioX * tempRatioX * tempRatioX;
                    double outputY = tempRatioY * tempRatioY * tempRatioY;
                    dState.RX = (byte)(outputX * capX + 128.0);
                    dState.RY = (byte)(outputY * capY + 128.0);
                }
                else if (rsOutCurveMode == 4)
                {
                    double absX = Math.Abs(tempRatioX);
                    double absY = Math.Abs(tempRatioY);
                    double outputX = absX * (absX - 2.0);
                    double outputY = absY * (absY - 2.0);
                    dState.RX = (byte)(-1.0 * outputX * signX * capX + 128.0);
                    dState.RY = (byte)(-1.0 * outputY * signY * capY + 128.0);
                }
                else if (rsOutCurveMode == 5)
                {
                    double innerX = Math.Abs(tempRatioX) - 1.0;
                    double innerY = Math.Abs(tempRatioY) - 1.0;
                    double outputX = innerX * innerX * innerX + 1.0;
                    double outputY = innerY * innerY * innerY + 1.0;
                    dState.RX = (byte)(1.0 * outputX * signX * capX + 128.0);
                    dState.RY = (byte)(1.0 * outputY * signY * capY + 128.0);
                }
                else if (rsOutCurveMode == 6)
                {
                    // Get max values and circular distance of axes
                    double maxX = (dState.RX >= 128 ? 127 : 128);
                    double maxY = (dState.RY >= 128 ? 127 : 128);
                    byte tempOutX = (byte)(tempRatioX * maxX + 128.0);
                    byte tempOutY = (byte)(tempRatioY * maxY + 128.0);

                    // Perform curve based on byte values from vector
                    byte tempX = rsOutBezierCurveObj[device].arrayBezierLUT[tempOutX];
                    byte tempY = rsOutBezierCurveObj[device].arrayBezierLUT[tempOutY];

                    // Calculate new ratio
                    double tempRatioOutX = (tempX - 128.0) / maxX;
                    double tempRatioOutY = (tempY - 128.0) / maxY;

                    // Map back to stick coordinates
                    dState.RX = (byte)(tempRatioOutX * capX + 128);
                    dState.RY = (byte)(tempRatioOutY * capY + 128);
                }
            }

            int l2OutCurveMode = getL2OutCurveMode(device);
            if (l2OutCurveMode > 0 && dState.L2 != 0)
            {
                double temp = dState.L2 / 255.0;
                if (l2OutCurveMode == 1)
                {
                    double output;

                    if (temp <= 0.4)
                        output = 0.55 * temp;
                    else if (temp <= 0.75)
                        output = temp - 0.18;
                    else // if (temp > 0.75)
                        output = (temp * 1.72) - 0.72;
                    dState.L2 = (byte)(output * 255.0);
                }
                else if (l2OutCurveMode == 2)
                {
                    double output = temp * temp;
                    dState.L2 = (byte)(output * 255.0);
                }
                else if (l2OutCurveMode == 3)
                {
                    double output = temp * temp * temp;
                    dState.L2 = (byte)(output * 255.0);
                }
                else if (l2OutCurveMode == 4)
                {
                    double output = temp * (temp - 2.0);
                    dState.L2 = (byte)(-1.0 * output * 255.0);
                }
                else if (l2OutCurveMode == 5)
                {
                    double inner = Math.Abs(temp) - 1.0;
                    double output = inner * inner * inner + 1.0;
                    dState.L2 = (byte)(-1.0 * output * 255.0);
                }
                else if (l2OutCurveMode == 6)
                {
                    dState.L2 = l2OutBezierCurveObj[device].arrayBezierLUT[dState.L2];
                }
            }

            int r2OutCurveMode = getR2OutCurveMode(device);
            if (r2OutCurveMode > 0 && dState.R2 != 0)
            {
                double temp = dState.R2 / 255.0;
                if (r2OutCurveMode == 1)
                {
                    double output;

                    if (temp <= 0.4)
                        output = 0.55 * temp;
                    else if (temp <= 0.75)
                        output = temp - 0.18;
                    else // if (temp > 0.75)
                        output = (temp * 1.72) - 0.72;
                    dState.R2 = (byte)(output * 255.0);
                }
                else if (r2OutCurveMode == 2)
                {
                    double output = temp * temp;
                    dState.R2 = (byte)(output * 255.0);
                }
                else if (r2OutCurveMode == 3)
                {
                    double output = temp * temp * temp;
                    dState.R2 = (byte)(output * 255.0);
                }
                else if (r2OutCurveMode == 4)
                {
                    double output = temp * (temp - 2.0);
                    dState.R2 = (byte)(-1.0 * output * 255.0);
                }
                else if (r2OutCurveMode == 5)
                {
                    double inner = Math.Abs(temp) - 1.0;
                    double output = inner * inner * inner + 1.0;
                    dState.R2 = (byte)(-1.0 * output * 255.0);
                }
                else if (r2OutCurveMode == 6)
                {
                    dState.R2 = r2OutBezierCurveObj[device].arrayBezierLUT[dState.R2];
                }
            }
                

            bool saControls = IsUsingSAForControls(device);
            if (saControls && dState.Motion.outputGyroControls)
            {
                int SXD = (int)(128d * getSXDeadzone(device));
                int SZD = (int)(128d * getSZDeadzone(device));
                double SXMax = getSXMaxzone(device);
                double SZMax = getSZMaxzone(device);
                double sxAntiDead = getSXAntiDeadzone(device);
                double szAntiDead = getSZAntiDeadzone(device);
                double sxsens = getSXSens(device);
                double szsens = getSZSens(device);
                int result = 0;

                int gyroX = cState.Motion.accelX, gyroZ = cState.Motion.accelZ;
                int absx = Math.Abs(gyroX), absz = Math.Abs(gyroZ);

                if (SXD > 0 || SXMax < 1.0 || sxAntiDead > 0)
                {
                    int maxValue = (int)(SXMax * 128d);
                    if (absx > SXD)
                    {
                        double ratioX = absx < maxValue ? (absx - SXD) / (double)(maxValue - SXD) : 1.0;
                        dState.Motion.outputAccelX = Math.Sign(gyroX) *
                            (int)Math.Min(128d, sxsens * 128d * ((1.0 - sxAntiDead) * ratioX + sxAntiDead));
                    }
                    else
                    {
                        dState.Motion.outputAccelX = 0;
                    }
                }
                else
                {
                    dState.Motion.outputAccelX = Math.Sign(gyroX) *
                        (int)Math.Min(128d, sxsens * 128d * (absx / 128d));
                }

                if (SZD > 0 || SZMax < 1.0 || szAntiDead > 0)
                {
                    int maxValue = (int)(SZMax * 128d);
                    if (absz > SZD)
                    {
                        double ratioZ = absz < maxValue ? (absz - SZD) / (double)(maxValue - SZD) : 1.0;
                        dState.Motion.outputAccelZ = Math.Sign(gyroZ) *
                            (int)Math.Min(128d, szsens * 128d * ((1.0 - szAntiDead) * ratioZ + szAntiDead));
                    }
                    else
                    {
                        dState.Motion.outputAccelZ = 0;
                    }
                }
                else
                {
                    dState.Motion.outputAccelZ = Math.Sign(gyroZ) *
                        (int)Math.Min(128d, szsens * 128d * (absz / 128d));
                }

                int sxOutCurveMode = getSXOutCurveMode(device);
                if (sxOutCurveMode > 0)
                {
                    double temp = dState.Motion.outputAccelX / 128.0;
                    double sign = Math.Sign(temp);
                    if (sxOutCurveMode == 1)
                    {
                        double output;
                        double abs = Math.Abs(temp);

                        if (abs <= 0.4)
                            output = 0.55 * abs;
                        else if (abs <= 0.75)
                            output = abs - 0.18;
                        else // if (abs > 0.75)
                            output = (abs * 1.72) - 0.72;
                        dState.Motion.outputAccelX = (int)(output * sign * 128.0);
                    }
                    else if (sxOutCurveMode == 2)
                    {
                        double output = temp * temp;
                        result = (int)(output * sign * 128.0);
                        dState.Motion.outputAccelX = result;
                    }
                    else if (sxOutCurveMode == 3)
                    {
                        double output = temp * temp * temp;
                        result = (int)(output * 128.0);
                        dState.Motion.outputAccelX = result;
                    }
                    else if (sxOutCurveMode == 4)
                    {
                        double abs = Math.Abs(temp);
                        double output = abs * (abs - 2.0);
                        dState.Motion.outputAccelX = (int)(-1.0 * output *
                            sign * 128.0);
                    }
                    else if (sxOutCurveMode == 5)
                    {
                        double inner = Math.Abs(temp) - 1.0;
                        double output = inner * inner * inner + 1.0;
                        dState.Motion.outputAccelX = (int)(output *
                            sign * 128.0);
                    }
                    else if (sxOutCurveMode == 6)
                    {
                        int signSA = Math.Sign(dState.Motion.outputAccelX);
                        dState.Motion.outputAccelX = sxOutBezierCurveObj[device].arrayBezierLUT[Math.Min(Math.Abs(dState.Motion.outputAccelX), 128)] * signSA;
                    }
                }

                int szOutCurveMode = getSZOutCurveMode(device);
                if (szOutCurveMode > 0 && dState.Motion.outputAccelZ != 0)
                {
                    double temp = dState.Motion.outputAccelZ / 128.0;
                    double sign = Math.Sign(temp);
                    if (szOutCurveMode == 1)
                    {
                        double output;
                        double abs = Math.Abs(temp);

                        if (abs <= 0.4)
                            output = 0.55 * abs;
                        else if (abs <= 0.75)
                            output = abs - 0.18;
                        else // if (abs > 0.75)
                            output = (abs * 1.72) - 0.72;
                        dState.Motion.outputAccelZ = (int)(output * sign * 128.0);
                    }
                    else if (szOutCurveMode == 2)
                    {
                        double output = temp * temp;
                        result = (int)(output * sign * 128.0);
                        dState.Motion.outputAccelZ = result;
                    }
                    else if (szOutCurveMode == 3)
                    {
                        double output = temp * temp * temp;
                        result = (int)(output * 128.0);
                        dState.Motion.outputAccelZ = result;
                    }
                    else if (szOutCurveMode == 4)
                    {
                        double abs = Math.Abs(temp);
                        double output = abs * (abs - 2.0);
                        dState.Motion.outputAccelZ = (int)(-1.0 * output *
                            sign * 128.0);
                    }
                    else if (szOutCurveMode == 5)
                    {
                        double inner = Math.Abs(temp) - 1.0;
                        double output = inner * inner * inner + 1.0;
                        dState.Motion.outputAccelZ = (int)(output *
                            sign * 128.0);
                    }
                    else if (szOutCurveMode == 6)
                    {
                        int signSA = Math.Sign(dState.Motion.outputAccelZ);
                        dState.Motion.outputAccelZ = szOutBezierCurveObj[device].arrayBezierLUT[Math.Min(Math.Abs(dState.Motion.outputAccelZ), 128)] * signSA;
                    }
                }
            }

            return dState;
        }

        /* TODO: Possibly remove usage of this version of the method */
        private static bool ShiftTrigger(int trigger, int device, DS4State cState, DS4StateExposed eState, Mouse tp)
        {
            bool result = false;
            if (trigger == 0)
            {
                result = false;
            }
            else
            {
                DS4Controls ds = shiftTriggerMapping[trigger];
                result = getBoolMapping(device, ds, cState, eState, tp);
            }

            return result;
        }

        private static bool ShiftTrigger2(int trigger, int device, DS4State cState, DS4StateExposed eState, Mouse tp, DS4StateFieldMapping fieldMapping)
        {
            bool result = false;
            if (trigger == 0)
            {
                result = false;
            }
            else if (trigger < 28 && trigger != 26)
            {
                DS4Controls ds = shiftTriggerMapping[trigger];
                result = getBoolMapping2(device, ds, cState, eState, tp, fieldMapping);
            }
            // 26 is a special case. It does not correlate to a direct DS4Controls value
            else if (trigger == 26)
            {
                result = cState.Touch1Finger;
            }

            return result;
        }

        private static X360Controls getX360ControlsByName(string key)
        {
            X360Controls x3c;
            if (Enum.TryParse(key, true, out x3c))
                return x3c;

            switch (key)
            {
                case "Back": return X360Controls.Back;
                case "Left Stick": return X360Controls.LS;
                case "Right Stick": return X360Controls.RS;
                case "Start": return X360Controls.Start;
                case "Up Button": return X360Controls.DpadUp;
                case "Right Button": return X360Controls.DpadRight;
                case "Down Button": return X360Controls.DpadDown;
                case "Left Button": return X360Controls.DpadLeft;

                case "Left Bumper": return X360Controls.LB;
                case "Right Bumper": return X360Controls.RB;
                case "Y Button": return X360Controls.Y;
                case "B Button": return X360Controls.B;
                case "A Button": return X360Controls.A;
                case "X Button": return X360Controls.X;

                case "Guide": return X360Controls.Guide;
                case "Left X-Axis-": return X360Controls.LXNeg;
                case "Left Y-Axis-": return X360Controls.LYNeg;
                case "Right X-Axis-": return X360Controls.RXNeg;
                case "Right Y-Axis-": return X360Controls.RYNeg;

                case "Left X-Axis+": return X360Controls.LXPos;
                case "Left Y-Axis+": return X360Controls.LYPos;
                case "Right X-Axis+": return X360Controls.RXPos;
                case "Right Y-Axis+": return X360Controls.RYPos;
                case "Left Trigger": return X360Controls.LT;
                case "Right Trigger": return X360Controls.RT;

                case "Left Mouse Button": return X360Controls.LeftMouse;
                case "Right Mouse Button": return X360Controls.RightMouse;
                case "Middle Mouse Button": return X360Controls.MiddleMouse;
                case "4th Mouse Button": return X360Controls.FourthMouse;
                case "5th Mouse Button": return X360Controls.FifthMouse;
                case "Mouse Wheel Up": return X360Controls.WUP;
                case "Mouse Wheel Down": return X360Controls.WDOWN;
                case "Mouse Up": return X360Controls.MouseUp;
                case "Mouse Down": return X360Controls.MouseDown;
                case "Mouse Left": return X360Controls.MouseLeft;
                case "Mouse Right": return X360Controls.MouseRight;
                case "Touchpad Click": return X360Controls.TouchpadClick;
                case "Unbound": return X360Controls.Unbound;
                default: break;
            }

            return X360Controls.Unbound;
        }

        /// <summary>
        /// Map DS4 Buttons/Axes to other DS4 Buttons/Axes (largely the same as Xinput ones) and to keyboard and mouse buttons.
        /// </summary>
        static bool[] held = new bool[Global.MAX_DS4_CONTROLLER_COUNT];
        public static void MapCustom(int device, DS4State cState, DS4State MappedState, DS4StateExposed eState,
            Mouse tp, ControlService ctrl)
        {
            /* TODO: This method is slow sauce. Find ways to speed up action execution */
            double tempMouseDeltaX = 0.0;
            double tempMouseDeltaY = 0.0;
            int mouseDeltaX = 0;
            int mouseDeltaY = 0;

            cState.calculateStickAngles();
            DS4StateFieldMapping fieldMapping = fieldMappings[device];
            fieldMapping.populateFieldMapping(cState, eState, tp);
            DS4StateFieldMapping outputfieldMapping = outputFieldMappings[device];
            outputfieldMapping.populateFieldMapping(cState, eState, tp);
            //DS4StateFieldMapping fieldMapping = new DS4StateFieldMapping(cState, eState, tp);
            //DS4StateFieldMapping outputfieldMapping = new DS4StateFieldMapping(cState, eState, tp);

            SyntheticState deviceState = Mapping.deviceState[device];
            if (getProfileActionCount(device) > 0 || useTempProfile[device])
                MapCustomAction(device, cState, MappedState, eState, tp, ctrl, fieldMapping, outputfieldMapping);
            //if (ctrl.DS4Controllers[device] == null) return;

            //cState.CopyTo(MappedState);

            //Dictionary<DS4Controls, DS4Controls> tempControlDict = new Dictionary<DS4Controls, DS4Controls>();
            //MultiValueDict<DS4Controls, DS4Controls> tempControlDict = new MultiValueDict<DS4Controls, DS4Controls>();
            
            //List<DS4ControlSettings> tempSettingsList = getDS4CSettings(device);
            //foreach (DS4ControlSettings dcs in getDS4CSettings(device))
            //for (int settingIndex = 0, arlen = tempSettingsList.Count; settingIndex < arlen; settingIndex++)

            ControlSettingsGroup controlSetGroup = GetControlSettingsGroup(device);
            StickOutputSetting stickSettings = Global.LSOutputSettings[device];
            if (stickSettings.mode == StickMode.Controls)
            {
                for (var settingEnum = controlSetGroup.LS.GetEnumerator(); settingEnum.MoveNext();)
                {
                    DS4ControlSettings dcs = settingEnum.Current;
                    ProcessControlSettingAction(dcs, device, cState, MappedState, eState,
                        tp, fieldMapping, outputfieldMapping, deviceState, ref tempMouseDeltaX,
                        ref tempMouseDeltaY, ctrl);
                }
            }
            else
            {
                outputfieldMapping.axisdirs[(int)DS4Controls.LXNeg] = 128;
                outputfieldMapping.axisdirs[(int)DS4Controls.LXPos] = 128;
                outputfieldMapping.axisdirs[(int)DS4Controls.LYNeg] = 128;
                outputfieldMapping.axisdirs[(int)DS4Controls.LYPos] = 128;

                switch (stickSettings.mode)
                {
                    case StickMode.None:
                        break;
                    case StickMode.FlickStick:
                        DS4Device d = ctrl.DS4Controllers[device];
                        DS4State cRawState = d.getCurrentStateRef();
                        DS4State pState = d.getPreviousStateRef();

                        ProcessFlickStick(device, cRawState, cRawState.LX, cRawState.LY, pState.LX, pState.LY, ctrl,
                            stickSettings.outputSettings.flickSettings, ref tempMouseDeltaX);
                        break;
                    default:
                        break;
                }
            }

            stickSettings = Global.RSOutputSettings[device];
            if (stickSettings.mode == StickMode.Controls)
            {
                for (var settingEnum = controlSetGroup.RS.GetEnumerator(); settingEnum.MoveNext();)
                {
                    DS4ControlSettings dcs = settingEnum.Current;
                    ProcessControlSettingAction(dcs, device, cState, MappedState, eState,
                        tp, fieldMapping, outputfieldMapping, deviceState, ref tempMouseDeltaX,
                        ref tempMouseDeltaY, ctrl);
                }
            }
            else
            {
                outputfieldMapping.axisdirs[(int)DS4Controls.RXNeg] = 128;
                outputfieldMapping.axisdirs[(int)DS4Controls.RXPos] = 128;
                outputfieldMapping.axisdirs[(int)DS4Controls.RYNeg] = 128;
                outputfieldMapping.axisdirs[(int)DS4Controls.RYPos] = 128;

                switch (stickSettings.mode)
                {
                    case StickMode.None:
                        break;
                    case StickMode.FlickStick:
                        DS4Device d = ctrl.DS4Controllers[device];
                        DS4State cRawState = d.getCurrentStateRef();
                        DS4State pState = d.getPreviousStateRef();

                        ProcessFlickStick(device, cRawState, cRawState.RX, cRawState.RY, pState.RX, pState.RY, ctrl,
                            stickSettings.outputSettings.flickSettings, ref tempMouseDeltaX);
                        break;
                    default:
                        break;
                }
            }

            TriggerOutputSettings l2TriggerSettings = Global.L2OutputSettings[device];
            DS4ControlSettings dcsTemp = controlSetGroup.L2;
            if (l2TriggerSettings.twoStageMode == TwoStageTriggerMode.Disabled)
            {
                ProcessControlSettingAction(dcsTemp, device, cState, MappedState, eState,
                    tp, fieldMapping, outputfieldMapping, deviceState, ref tempMouseDeltaX,
                    ref tempMouseDeltaY, ctrl);
            }
            else
            {
                outputfieldMapping.triggers[(int)DS4Controls.L2] = 0;

                DS4ControlSettings l2FullPull = controlSetGroup.L2FullPull;
                TwoStageTriggerMappingData l2TwoStageData = l2TwoStageMappingData[device];
                ProcessTwoStageTrigger(device, cState, cState.L2, ref dcsTemp, ref l2FullPull,
                    l2TriggerSettings, l2TwoStageData, out DS4ControlSettings outputSoftPull, out DS4ControlSettings outputFullPull);

                TwoStageTriggerMappingData.ActiveZoneButtons tempButtons = TwoStageTriggerMappingData.ActiveZoneButtons.None;
                // Check for Soft Pull activation
                if (outputSoftPull != null ||
                    (l2TwoStageData.previousActiveButtons & TwoStageTriggerMappingData.ActiveZoneButtons.SoftPull) != 0)
                {
                    if (outputSoftPull != null)
                    {
                        tempButtons |= TwoStageTriggerMappingData.ActiveZoneButtons.SoftPull;
                    }
                    else
                    {
                        // Need to reset input state so output binding is not activated.
                        // Used to de-activate Extras
                        fieldMapping.triggers[(int)DS4Controls.L2] = 0;
                    }

                    ProcessControlSettingAction(dcsTemp, device, cState, MappedState, eState,
                        tp, fieldMapping, outputfieldMapping, deviceState, ref tempMouseDeltaX,
                        ref tempMouseDeltaY, ctrl);
                }

                // Check for Full Pull activation
                if (outputFullPull != null ||
                    (l2TwoStageData.previousActiveButtons & TwoStageTriggerMappingData.ActiveZoneButtons.FullPull) != 0)
                {
                    if (outputFullPull != null)
                    {
                        tempButtons |= TwoStageTriggerMappingData.ActiveZoneButtons.FullPull;
                    }
                    else
                    {
                        // Need to reset input state so output binding is not activated.
                        // Used to de-activate Extras
                        fieldMapping.buttons[(int)DS4Controls.L2FullPull] = false;
                    }

                    ProcessControlSettingAction(l2FullPull, device, cState, MappedState, eState,
                        tp, fieldMapping, outputfieldMapping, deviceState, ref tempMouseDeltaX,
                        ref tempMouseDeltaY, ctrl);
                }

                // Store active buttons state
                l2TwoStageData.previousActiveButtons = tempButtons;
            }

            TriggerOutputSettings r2TriggerSettings = Global.R2OutputSettings[device];
            dcsTemp = controlSetGroup.R2;
            if (r2TriggerSettings.twoStageMode == TwoStageTriggerMode.Disabled)
            {
                ProcessControlSettingAction(dcsTemp, device, cState, MappedState, eState,
                    tp, fieldMapping, outputfieldMapping, deviceState, ref tempMouseDeltaX,
                    ref tempMouseDeltaY, ctrl);
            }
            else
            {
                outputfieldMapping.triggers[(int)DS4Controls.R2] = 0;

                DS4ControlSettings r2FullPull = controlSetGroup.R2FullPull;
                TwoStageTriggerMappingData r2TwoStageData = r2TwoStageMappingData[device];
                ProcessTwoStageTrigger(device, cState, cState.R2, ref dcsTemp, ref r2FullPull,
                    r2TriggerSettings, r2TwoStageData, out DS4ControlSettings outputSoftPull, out DS4ControlSettings outputFullPull);

                TwoStageTriggerMappingData.ActiveZoneButtons tempButtons = TwoStageTriggerMappingData.ActiveZoneButtons.None;
                // Check for Soft Pull activation
                if (outputSoftPull != null ||
                    (r2TwoStageData.previousActiveButtons & TwoStageTriggerMappingData.ActiveZoneButtons.SoftPull) != 0)
                {
                    if (outputSoftPull != null)
                    {
                        tempButtons |= TwoStageTriggerMappingData.ActiveZoneButtons.SoftPull;
                    }
                    else
                    {
                        // Need to reset input state so output binding is not activated.
                        // Used to de-activate Extras
                        fieldMapping.triggers[(int)DS4Controls.R2] = 0;
                    }

                    ProcessControlSettingAction(dcsTemp, device, cState, MappedState, eState,
                        tp, fieldMapping, outputfieldMapping, deviceState, ref tempMouseDeltaX,
                        ref tempMouseDeltaY, ctrl);
                }

                // Check for Full Pull activation
                if (outputFullPull != null ||
                    (r2TwoStageData.previousActiveButtons & TwoStageTriggerMappingData.ActiveZoneButtons.FullPull) != 0)
                {
                    if (outputFullPull != null)
                    {
                        tempButtons |= TwoStageTriggerMappingData.ActiveZoneButtons.FullPull;
                    }
                    else
                    {
                        // Need to reset input state so output binding is not activated.
                        // Used to de-activate Extras
                        fieldMapping.buttons[(int)DS4Controls.R2FullPull] = false;
                    }

                    ProcessControlSettingAction(r2FullPull, device, cState, MappedState, eState,
                        tp, fieldMapping, outputfieldMapping, deviceState, ref tempMouseDeltaX,
                        ref tempMouseDeltaY, ctrl);
                }

                // Store active buttons state
                r2TwoStageData.previousActiveButtons = tempButtons;
            }

            for (var settingEnum = controlSetGroup.ControlButtons.GetEnumerator(); settingEnum.MoveNext();)
            {
                DS4ControlSettings dcs = settingEnum.Current;
                ProcessControlSettingAction(dcs, device, cState, MappedState, eState,
                    tp, fieldMapping, outputfieldMapping, deviceState, ref tempMouseDeltaX,
                    ref tempMouseDeltaY, ctrl);
            }

            if (GyroOutputMode[device] == GyroOutMode.DirectionalSwipe)
            {
                DS4ControlSettings gyroSwipeXDcs = null;
                DS4ControlSettings gyroSwipeYDcs = null;
                DS4ControlSettings previousGyroSwipeXDcs = null;
                DS4ControlSettings previousGyroSwipeYDcs = null;

                if (tp.gyroSwipe.swipeLeft)
                {
                    gyroSwipeXDcs = controlSetGroup.GyroSwipeLeft;
                }
                else if (tp.gyroSwipe.swipeRight)
                {
                    gyroSwipeXDcs = controlSetGroup.GyroSwipeRight;
                }

                if (tp.gyroSwipe.previousSwipeLeft && !tp.gyroSwipe.swipeLeft)
                {
                    previousGyroSwipeXDcs = controlSetGroup.GyroSwipeLeft;
                }
                else if (tp.gyroSwipe.previousSwipeRight && !tp.gyroSwipe.swipeRight)
                {
                    previousGyroSwipeXDcs = controlSetGroup.GyroSwipeRight;
                }

                if (tp.gyroSwipe.swipeUp)
                {
                    gyroSwipeYDcs = controlSetGroup.GyroSwipeUp;
                }
                else if (tp.gyroSwipe.swipeDown)
                {
                    gyroSwipeYDcs = controlSetGroup.GyroSwipeDown;
                }

                if (tp.gyroSwipe.previousSwipeUp && !tp.gyroSwipe.swipeUp)
                {
                    previousGyroSwipeYDcs = controlSetGroup.GyroSwipeUp;
                }
                else if (tp.gyroSwipe.previousSwipeDown && !tp.gyroSwipe.swipeDown)
                {
                    previousGyroSwipeYDcs = controlSetGroup.GyroSwipeDown;
                }

                // Disable previous button before possibly activating current button
                if (previousGyroSwipeXDcs != null)
                {
                    ProcessControlSettingAction(previousGyroSwipeXDcs, device, cState, MappedState, eState,
                        tp, fieldMapping, outputfieldMapping, deviceState, ref tempMouseDeltaX,
                        ref tempMouseDeltaY, ctrl);
                }

                if (gyroSwipeXDcs != null)
                {
                    ProcessControlSettingAction(gyroSwipeXDcs, device, cState, MappedState, eState,
                        tp, fieldMapping, outputfieldMapping, deviceState, ref tempMouseDeltaX,
                        ref tempMouseDeltaY, ctrl);
                }

                // Disable previous button before possibly activating current button
                if (previousGyroSwipeYDcs != null)
                {
                    ProcessControlSettingAction(previousGyroSwipeYDcs, device, cState, MappedState, eState,
                        tp, fieldMapping, outputfieldMapping, deviceState, ref tempMouseDeltaX,
                        ref tempMouseDeltaY, ctrl);
                }

                if (gyroSwipeYDcs != null)
                {
                    ProcessControlSettingAction(gyroSwipeYDcs, device, cState, MappedState, eState,
                        tp, fieldMapping, outputfieldMapping, deviceState, ref tempMouseDeltaX,
                        ref tempMouseDeltaY, ctrl);
                }
            }

            Queue<ControlToXInput> tempControl = customMapQueue[device];
            unchecked
            {
                for (int i = 0, len = tempControl.Count; i < len; i++)
                //while(tempControl.Any())
                {
                    ControlToXInput tempMap = tempControl.Dequeue();
                    int controlNum = (int)tempMap.ds4input;
                    int tempOutControl = (int)tempMap.xoutput;
                    if (tempMap.xoutput >= DS4Controls.LXNeg && tempMap.xoutput <= DS4Controls.RYPos)
                    {
                        const byte axisDead = 128;
                        DS4StateFieldMapping.ControlType controlType = DS4StateFieldMapping.mappedType[tempOutControl];
                        bool alt = controlType == DS4StateFieldMapping.ControlType.AxisDir && tempOutControl % 2 == 0 ? true : false;
                        byte axisMapping = getXYAxisMapping2(device, tempMap.ds4input, cState, eState, tp, fieldMapping, alt);
                        if (axisMapping != axisDead)
                        {
                            int controlRelation = tempOutControl % 2 == 0 ? tempOutControl - 1 : tempOutControl + 1;
                            outputfieldMapping.axisdirs[tempOutControl] = axisMapping;
                            outputfieldMapping.axisdirs[controlRelation] = axisMapping;
                        }
                    }
                    else
                    {
                        if (tempMap.xoutput == DS4Controls.L2 || tempMap.xoutput == DS4Controls.R2)
                        {
                            const byte axisZero = 0;
                            byte axisMapping = getByteMapping2(device, tempMap.ds4input, cState, eState, tp, fieldMapping);
                            if (axisMapping != axisZero)
                                outputfieldMapping.triggers[tempOutControl] = axisMapping;
                        }
                        else
                        {
                            bool value = getBoolMapping2(device, tempMap.ds4input, cState, eState, tp, fieldMapping);
                            if (value)
                                outputfieldMapping.buttons[tempOutControl] = value;
                        }
                    }
                }
            }

            outputfieldMapping.populateState(MappedState);

            if (macroCount > 0)
            {
                if (macroControl[00]) MappedState.Cross = true;
                if (macroControl[01]) MappedState.Circle = true;
                if (macroControl[02]) MappedState.Square = true;
                if (macroControl[03]) MappedState.Triangle = true;
                if (macroControl[04]) MappedState.Options = true;
                if (macroControl[05]) MappedState.Share = true;
                if (macroControl[06]) MappedState.DpadUp = true;
                if (macroControl[07]) MappedState.DpadDown = true;
                if (macroControl[08]) MappedState.DpadLeft = true;
                if (macroControl[09]) MappedState.DpadRight = true;
                if (macroControl[10]) MappedState.PS = true;
                if (macroControl[11]) MappedState.L1 = true;
                if (macroControl[12]) MappedState.R1 = true;
                if (macroControl[13]) MappedState.L2 = 255;
                if (macroControl[14]) MappedState.R2 = 255;
                if (macroControl[15]) MappedState.L3 = true;
                if (macroControl[16]) MappedState.R3 = true;
                if (macroControl[17]) MappedState.LX = 255;
                if (macroControl[18]) MappedState.LX = 0;
                if (macroControl[19]) MappedState.LY = 255;
                if (macroControl[20]) MappedState.LY = 0;
                if (macroControl[21]) MappedState.RX = 255;
                if (macroControl[22]) MappedState.RX = 0;
                if (macroControl[23]) MappedState.RY = 255;
                if (macroControl[24]) MappedState.RY = 0;
            }

            if (GetSASteeringWheelEmulationAxis(device) != SASteeringWheelEmulationAxisType.None)
            {
                MappedState.SASteeringWheelEmulationUnit = Mapping.Scale360degreeGyroAxis(device, eState, ctrl);
            }

            ref byte gyroTempX = ref gyroStickX[device];
            if (gyroTempX != 128)
            {
                if (MappedState.RX != 128)
                    MappedState.RX = Math.Abs(gyroTempX - 128) > Math.Abs(MappedState.RX - 128) ?
                        gyroTempX : MappedState.RX;
                else
                    MappedState.RX = gyroTempX;
            }

            ref byte gyroTempY = ref gyroStickY[device];
            if (gyroTempY != 128)
            {
                if (MappedState.RY != 128)
                    MappedState.RY = Math.Abs(gyroTempY - 128) > Math.Abs(MappedState.RY - 128) ?
                        gyroTempY : MappedState.RY;
                else
                    MappedState.RY = gyroTempY;
            }

            gyroTempX = gyroTempY = 128;

            calculateFinalMouseMovement(ref tempMouseDeltaX, ref tempMouseDeltaY,
                out mouseDeltaX, out mouseDeltaY);
            if (mouseDeltaX != 0 || mouseDeltaY != 0)
            {
                InputMethods.MoveCursorBy(mouseDeltaX, mouseDeltaY);
            }
        }

        private static void ProcessTwoStageTrigger(int device, DS4State cState, byte triggerValue,
            ref DS4ControlSettings inputSoftPull, ref DS4ControlSettings inputFullPull, TriggerOutputSettings outputSettings,
            TwoStageTriggerMappingData twoStageData, out DS4ControlSettings outputSoftPull, out DS4ControlSettings outputFullPull)
        {
            DS4ControlSettings dcsTemp = inputSoftPull;
            DS4ControlSettings dcsFullPull = null;
            TwoStageTriggerMappingData triggerData = twoStageData;

            switch (outputSettings.twoStageMode)
            {
                case TwoStageTriggerMode.Normal:
                    if (triggerValue == 255)
                    {
                        dcsFullPull = inputFullPull;
                    }

                    break;
                case TwoStageTriggerMode.ExclusiveButtons:
                    if (triggerValue == 255)
                    {
                        dcsFullPull = inputFullPull;
                        dcsTemp = null;
                        triggerData.actionStateMode =
                            TwoStageTriggerMappingData.EngageButtonsMode.FullPullOnly;
                        triggerData.outputActive = true;
                    }
                    else if (triggerValue != 0 && triggerData.actionStateMode !=
                        TwoStageTriggerMappingData.EngageButtonsMode.FullPullOnly)
                    {
                        triggerData.actionStateMode =
                            TwoStageTriggerMappingData.EngageButtonsMode.Both;
                        triggerData.outputActive = true;
                    }
                    else if (triggerValue == 0 && triggerData.outputActive)
                    {
                        triggerData.Reset();
                    }

                    break;
                case TwoStageTriggerMode.HairTrigger:
                    dcsTemp = null;

                    triggerData.actionStateMode = TwoStageTriggerMappingData.EngageButtonsMode.Both;

                    if (triggerValue == 255)
                    {
                        // Full pull now activates both. Soft pull action
                        // no longer engaged with threshold
                        dcsTemp = inputSoftPull;
                        dcsFullPull = inputFullPull;
                        triggerData.softPullActActive = true;
                        triggerData.fullPullActActive = true;
                        triggerData.outputActive = true;
                    }
                    else if (triggerValue != 0 && !triggerData.fullPullActActive)
                    {
                        // Full pull not engaged yet. Activate Soft pull action.
                        dcsTemp = inputSoftPull;
                        triggerData.softPullActActive = true;
                        triggerData.outputActive = true;
                    }
                    else if (triggerValue == 0 && triggerData.outputActive)
                    {
                        triggerData.Reset();
                    }
                    //else if (triggerData.outputActive)
                    //{
                    //    Console.WriteLine(triggerValue);
                    //}

                    break;

                case TwoStageTriggerMode.HipFire:
                    dcsTemp = null;

                    if (triggerValue != 0 && !triggerData.startCheck)
                    {
                        triggerData.StartProcessing();
                        dcsTemp = null;
                    }
                    else if (triggerValue != 0 && !triggerData.outputActive)
                    {
                        bool outputActive = triggerData.checkTime +
                            TimeSpan.FromMilliseconds(outputSettings.hipFireMS) < DateTime.Now;
                        if (outputActive)
                        {
                            triggerData.outputActive = true;

                            if (triggerValue == 255)
                            {
                                dcsFullPull = inputFullPull;
                                triggerData.fullPullActActive = true;
                                triggerData.actionStateMode = TwoStageTriggerMappingData.EngageButtonsMode.FullPullOnly;
                            }
                            else if (triggerValue != 0)
                            {
                                dcsTemp = inputSoftPull;
                                triggerData.softPullActActive = true;
                                triggerData.actionStateMode = TwoStageTriggerMappingData.EngageButtonsMode.Both;
                            }
                        }
                    }
                    else if (triggerData.outputActive)
                    {
                        //DS4State pState = d.getPreviousStateRef();
                        if (triggerValue == 255)
                        {
                            dcsFullPull = inputFullPull;
                            triggerData.fullPullActActive = true;
                            if (triggerData.actionStateMode == TwoStageTriggerMappingData.EngageButtonsMode.Both)
                            {
                                dcsTemp = inputSoftPull;
                            }
                        }
                        else if (triggerValue != 0 && triggerData.actionStateMode ==
                            TwoStageTriggerMappingData.EngageButtonsMode.Both)
                        {
                            triggerData.fullPullActActive = false;

                            dcsTemp = inputSoftPull;
                            triggerData.softPullActActive = true;
                        }
                        else if (triggerValue == 0)
                        {
                            triggerData.Reset();
                        }
                    }
                    else if (triggerData.startCheck)
                    {
                        triggerData.Reset();
                    }

                    break;
                case TwoStageTriggerMode.HipFireExclusiveButtons:
                    dcsTemp = null;

                    if (triggerValue != 0 && !triggerData.startCheck)
                    {
                        triggerData.StartProcessing();
                        dcsTemp = null;
                    }
                    else if (triggerValue != 0 && !triggerData.outputActive)
                    {
                        bool outputActive = triggerData.checkTime + TimeSpan.FromMilliseconds(outputSettings.hipFireMS) < DateTime.Now;
                        if (outputActive)
                        {
                            triggerData.outputActive = true;

                            if (triggerValue == 255)
                            {
                                dcsFullPull = inputFullPull;
                                triggerData.fullPullActActive = true;
                                triggerData.actionStateMode =
                                    TwoStageTriggerMappingData.EngageButtonsMode.FullPullOnly;
                            }
                            else if (triggerValue != 0)
                            {
                                dcsTemp = inputSoftPull;
                                triggerData.softPullActActive = true;
                                triggerData.actionStateMode =
                                    TwoStageTriggerMappingData.EngageButtonsMode.SoftPullOnly;
                            }
                        }
                    }
                    else if (triggerData.outputActive)
                    {
                        //DS4State pState = d.getPreviousStateRef();
                        if (triggerValue == 255 &&
                            triggerData.actionStateMode == TwoStageTriggerMappingData.EngageButtonsMode.FullPullOnly)
                        {
                            dcsFullPull = inputFullPull;
                        }
                        else if (triggerValue != 0 && triggerData.actionStateMode ==
                            TwoStageTriggerMappingData.EngageButtonsMode.SoftPullOnly)
                        {
                            dcsTemp = inputSoftPull;
                        }
                        else if (triggerValue == 0)
                        {
                            triggerData.Reset();
                        }
                    }
                    else if (triggerData.startCheck)
                    {
                        triggerData.Reset();
                    }

                    break;
                default:
                    break;
            }

            outputSoftPull = dcsTemp;
            outputFullPull = dcsFullPull;
        }

        private static void ProcessFlickStick(int device, DS4State cRawState, byte stickX, byte stickY, byte prevStickX, byte prevStickY, ControlService ctrl, FlickStickSettings flickSettings, ref double tempMouseDeltaX)
        {
            FlickStickMappingData tempFlickData = flickMappingData[device];
            double angleChange = HandleFlickStickAngle(cRawState, stickX, stickY, prevStickX, prevStickY,
                tempFlickData, flickSettings);
            //angleChange = flickFilter.Filter(angleChange, cState.elapsedTime);
            //Console.WriteLine(angleChange);
            //if (angleChange != 0.0)
            double lsangle = angleChange * 180.0 / Math.PI;
            if (lsangle == 0.0)
            {
                tempFlickData.flickAngleRemainder = 0.0;
            }
            else if (lsangle >= 0.0 && tempFlickData.flickAngleRemainder >= 0.0)
            {
                lsangle += tempFlickData.flickAngleRemainder;
            }

            tempFlickData.flickAngleRemainder = 0.0;
            //Console.WriteLine(lsangle);
            //if (angleChange != 0.0)
            if (flickSettings.minAngleThreshold == 0.0 && lsangle != 0.0)
            //if (Math.Abs(lsangle) >= 0.5)
            {
                tempFlickData.flickAngleRemainder = 0.0;
                //flickAngleRemainder = lsangle - (int)lsangle;
                //lsangle = (int)lsangle;
                tempMouseDeltaX += lsangle * flickSettings.realWorldCalibration;
            }
            else if (Math.Abs(lsangle) >= flickSettings.minAngleThreshold)
            {
                tempFlickData.flickAngleRemainder = 0.0;
                //flickAngleRemainder = lsangle - (int)lsangle;
                //lsangle = (int)lsangle;
                tempMouseDeltaX += lsangle * flickSettings.realWorldCalibration;
            }
            else
            {
                tempFlickData.flickAngleRemainder = lsangle;
            }
        }

        private static double HandleFlickStickAngle(DS4State cState, byte stickX, byte stickY, byte prevStickX, byte prevStickY,
            FlickStickMappingData flickData, FlickStickSettings flickSettings)
        {
            double result = 0.0;

            double lastXMax = prevStickX >= 128 ? 127.0 : -128.0;
            double lastTestX = (prevStickX - 128) / lastXMax;
            double lastYMax = prevStickY >= 128 ? 127.0 : -128.0;
            double lastTestY = (prevStickY - 128) / lastYMax;

            double currentXMax = stickX >= 128 ? 127.0 : -128.0;
            double currentTestX = (stickX - 128) / currentXMax;
            double currentYMax = stickY >= 128 ? 127.0 : -128.0;
            double currentTestY = (stickY - 128) / currentYMax;

            double lastLength = (lastTestX * lastTestX) + (lastTestY * lastTestY);
            double length = (currentTestX * currentTestX) + (currentTestY * currentTestY);
            double testLength = flickSettings.flickThreshold * flickSettings.flickThreshold;

            if (length >= testLength)
            {
                if (lastLength < testLength)
                {
                    // Start new Flick
                    flickData.flickProgress = 0.0; // Reset Flick progress
                    flickData.flickSize = Math.Atan2((stickX - 128), -(stickY - 128));
                    //flickData.flickFilter.Filter(0.0, cState.elapsedTime);
                }
                else
                {
                    // Turn camera
                    double stickAngle = Math.Atan2((stickX - 128), -(stickY - 128));
                    double lastStickAngle = Math.Atan2((prevStickX - 128), -(prevStickY - 128));
                    double angleChange = (stickAngle - lastStickAngle);
                    double rawAngleChange = angleChange;
                    angleChange = (angleChange+Math.PI) % (2* Math.PI);
                    if (angleChange < 0)
                    {
                        angleChange += 2 * Math.PI;
                    }
                    angleChange -= Math.PI;
                    //Console.WriteLine("ANGLE CHANGE: {0} {1} {2}", stickAngle, lastStickAngle, rawAngleChange);
                    //Console.WriteLine("{0} {1} | {2} {3}", cState.RX, pState.RX, cState.RY, pState.RY);
                    //angleChange = flickData.flickFilter.Filter(angleChange, cState.elapsedTime);
                    result += angleChange;
                }
            }
            else
            {
                // Cleanup
                //flickData.flickFilter.Filter(0.0, cState.elapsedTime);
                result = 0.0;
            }

            // Continue Flick motion
            double lastFlickProgress = flickData.flickProgress;
            double flickTime = flickSettings.flickTime;
            if (lastFlickProgress < flickTime)
            {
                flickData.flickProgress = Math.Min(flickData.flickProgress + cState.elapsedTime, flickTime);
    
                double lastPerOne = lastFlickProgress / flickTime;
                double thisPerOne = flickData.flickProgress / flickTime;
    
                double warpedLastPerOne = WarpEaseOut(lastPerOne);
                double warpedThisPerone = WarpEaseOut(thisPerOne);
                //Console.WriteLine("{0} {1}", warpedThisPerone, warpedLastPerOne);

                result += (warpedThisPerone - warpedLastPerOne) * flickData.flickSize;
            }

            return result;
        }

        private static double WarpEaseOut(double input)
        {
            double flipped = 1.0 - input;
            return 1.0 - flipped * flipped;
        }

        //private static OneEuroFilter flickFilter = new OneEuroFilter(0.4, 0.4);
        //private static double flickProgress = 0.0;
        //private static double flickSize = 0.0;
        //private static double flickAngleRemainder = 0.0;

        //private const double REAL_WORLD_CALIBRATION = 10;

        //private static double FlickThreshold = 0.9;
        //private static double FlickTime = 0.1;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void ProcessControlSettingAction(DS4ControlSettings dcs, int device, DS4State cState, DS4State MappedState, DS4StateExposed eState,
            Mouse tp, DS4StateFieldMapping fieldMapping, DS4StateFieldMapping outputfieldMapping, SyntheticState deviceState, ref double tempMouseDeltaX, ref double tempMouseDeltaY,
            ControlService ctrl)
        {
            //DS4ControlSettings dcs = tempSettingsList[settingIndex];

            //object action = null;
            ControlActionData action = null;
            DS4ControlSettings.ActionType actionType = 0;
            DS4KeyType keyType = DS4KeyType.None;
            DS4Controls usingExtra = DS4Controls.None;
            if (dcs.shiftActionType != DS4ControlSettings.ActionType.Default && ShiftTrigger2(dcs.shiftTrigger, device, cState, eState, tp, fieldMapping))
            {
                action = dcs.shiftAction;
                actionType = dcs.shiftActionType;
                keyType = dcs.shiftKeyType;
            }
            else if (dcs.actionType != DS4ControlSettings.ActionType.Default)
            {
                action = dcs.action;
                actionType = dcs.actionType;
                keyType = dcs.keyType;
            }

            if (usingExtra == DS4Controls.None || usingExtra == dcs.control)
            {
                bool shiftE = !string.IsNullOrEmpty(dcs.shiftExtras) && ShiftTrigger2(dcs.shiftTrigger, device, cState, eState, tp, fieldMapping);
                bool regE = !string.IsNullOrEmpty(dcs.extras);
                if ((regE || shiftE) && getBoolActionMapping2(device, dcs.control, cState, eState, tp, fieldMapping))
                {
                    usingExtra = dcs.control;
                    string p;
                    if (shiftE)
                        p = dcs.shiftExtras;
                    else
                        p = dcs.extras;

                    string[] extraS = p.Split(',');
                    int extrasSLen = extraS.Length;
                    int[] extras = new int[extrasSLen];
                    for (int i = 0; i < extrasSLen; i++)
                    {
                        int b;
                        if (int.TryParse(extraS[i], out b))
                            extras[i] = b;
                    }

                    held[device] = true;
                    try
                    {
                        if (!(extras[0] == extras[1] && extras[1] == 0))
                        {
                            ctrl.setRumble((byte)extras[0], (byte)extras[1], device);
                            extrasRumbleActive[device] = true;
                        }

                        if (extras[2] == 1)
                        {
                            DS4Color color = new DS4Color { red = (byte)extras[3], green = (byte)extras[4], blue = (byte)extras[5] };
                            DS4LightBar.forcedColor[device] = color;
                            DS4LightBar.forcedFlash[device] = (byte)extras[6];
                            DS4LightBar.forcelight[device] = true;
                        }

                        if (extras[7] == 1)
                        {
                            ButtonMouseInfo tempMouseInfo = ButtonMouseInfos[device];
                            if (tempMouseInfo.tempButtonSensitivity == -1)
                            {
                                tempMouseInfo.tempButtonSensitivity = extras[8];
                                tempMouseInfo.SetActiveButtonSensitivity(extras[8]);
                            }
                        }
                    }
                    catch { }
                }
                else if ((regE || shiftE) && held[device])
                {
                    DS4LightBar.forcelight[device] = false;
                    DS4LightBar.forcedFlash[device] = 0;
                    ButtonMouseInfo tempMouseInfo = ButtonMouseInfos[device];
                    if (tempMouseInfo.tempButtonSensitivity != -1)
                    {
                        tempMouseInfo.SetActiveButtonSensitivity(tempMouseInfo.buttonSensitivity);
                        tempMouseInfo.tempButtonSensitivity = -1;
                    }

                    if (extrasRumbleActive[device])
                    {
                        ctrl.setRumble(0, 0, device);
                        extrasRumbleActive[device] = false;
                    }

                    held[device] = false;
                    usingExtra = DS4Controls.None;
                }
            }

            if (actionType != DS4ControlSettings.ActionType.Default)
            {
                if (actionType == DS4ControlSettings.ActionType.Macro)
                {
                    bool active = getBoolMapping2(device, dcs.control, cState, eState, tp, fieldMapping);
                    if (active)
                    {
                        PlayMacro(device, macroControl, string.Empty, null, action.actionMacro, dcs.control, keyType);
                    }
                    else
                    {
                        EndMacro(device, macroControl, action.actionMacro, dcs.control);
                    }

                    // erase default mappings for things that are remapped
                    resetToDefaultValue2(dcs.control, MappedState, outputfieldMapping);
                }
                else if (actionType == DS4ControlSettings.ActionType.Key)
                {
                    ushort value = Convert.ToUInt16(action.actionKey);
                    if (getBoolActionMapping2(device, dcs.control, cState, eState, tp, fieldMapping))
                    {
                        SyntheticState.KeyPresses kp;
                        if (!deviceState.keyPresses.TryGetValue(value, out kp))
                            deviceState.keyPresses[value] = kp = new SyntheticState.KeyPresses();

                        if (keyType.HasFlag(DS4KeyType.ScanCode))
                            kp.current.scanCodeCount++;
                        else
                            kp.current.vkCount++;

                        if (keyType.HasFlag(DS4KeyType.Toggle))
                        {
                            if (!pressedonce[value])
                            {
                                kp.current.toggle = !kp.current.toggle;
                                pressedonce[value] = true;
                            }
                            kp.current.toggleCount++;
                        }
                        kp.current.repeatCount++;
                    }
                    else
                        pressedonce[value] = false;

                    // erase default mappings for things that are remapped
                    resetToDefaultValue2(dcs.control, MappedState, outputfieldMapping);
                }
                else if (actionType == DS4ControlSettings.ActionType.Button)
                {
                    int keyvalue = 0;
                    bool isAnalog = false;

                    if (dcs.control >= DS4Controls.LXNeg && dcs.control <= DS4Controls.RYPos)
                    {
                        isAnalog = true;
                    }
                    else if (dcs.control == DS4Controls.L2 || dcs.control == DS4Controls.R2)
                    {
                        isAnalog = true;
                    }
                    else if (dcs.control >= DS4Controls.GyroXPos && dcs.control <= DS4Controls.GyroZNeg)
                    {
                        isAnalog = true;
                    }

                    X360Controls xboxControl = X360Controls.None;
                    xboxControl = (X360Controls)action.actionBtn;
                    if (xboxControl >= X360Controls.LXNeg && xboxControl <= X360Controls.Start)
                    {
                        DS4Controls tempDS4Control = reverseX360ButtonMapping[(int)xboxControl];
                        customMapQueue[device].Enqueue(new ControlToXInput(dcs.control, tempDS4Control));
                        //tempControlDict.Add(dcs.control, tempDS4Control);
                    }
                    else if (xboxControl == X360Controls.TouchpadClick)
                    {
                        bool value = getBoolMapping2(device, dcs.control, cState, eState, tp, fieldMapping);
                        if (value)
                            outputfieldMapping.touchButton = value;
                    }
                    else if (xboxControl >= X360Controls.LeftMouse && xboxControl <= X360Controls.WDOWN)
                    {
                        switch (xboxControl)
                        {
                            case X360Controls.LeftMouse:
                                {
                                    keyvalue = 256;
                                    if (getBoolActionMapping2(device, dcs.control, cState, eState, tp, fieldMapping))
                                        deviceState.currentClicks.leftCount++;

                                    break;
                                }
                            case X360Controls.RightMouse:
                                {
                                    keyvalue = 257;
                                    if (getBoolActionMapping2(device, dcs.control, cState, eState, tp, fieldMapping))
                                        deviceState.currentClicks.rightCount++;

                                    break;
                                }
                            case X360Controls.MiddleMouse:
                                {
                                    keyvalue = 258;
                                    if (getBoolActionMapping2(device, dcs.control, cState, eState, tp, fieldMapping))
                                        deviceState.currentClicks.middleCount++;

                                    break;
                                }
                            case X360Controls.FourthMouse:
                                {
                                    keyvalue = 259;
                                    if (getBoolActionMapping2(device, dcs.control, cState, eState, tp, fieldMapping))
                                        deviceState.currentClicks.fourthCount++;

                                    break;
                                }
                            case X360Controls.FifthMouse:
                                {
                                    keyvalue = 260;
                                    if (getBoolActionMapping2(device, dcs.control, cState, eState, tp, fieldMapping))
                                        deviceState.currentClicks.fifthCount++;

                                    break;
                                }
                            case X360Controls.WUP:
                                {
                                    if (getBoolActionMapping2(device, dcs.control, cState, eState, tp, fieldMapping))
                                    {
                                        if (isAnalog)
                                            getMouseWheelMapping(device, dcs.control, cState, eState, tp, false);
                                        else
                                            deviceState.currentClicks.wUpCount++;
                                    }

                                    break;
                                }
                            case X360Controls.WDOWN:
                                {
                                    if (getBoolActionMapping2(device, dcs.control, cState, eState, tp, fieldMapping))
                                    {
                                        if (isAnalog)
                                            getMouseWheelMapping(device, dcs.control, cState, eState, tp, true);
                                        else
                                            deviceState.currentClicks.wDownCount++;
                                    }

                                    break;
                                }

                            default: break;
                        }
                    }
                    else if (xboxControl >= X360Controls.MouseUp && xboxControl <= X360Controls.MouseRight)
                    {
                        switch (xboxControl)
                        {
                            case X360Controls.MouseUp:
                                {
                                    if (tempMouseDeltaY == 0)
                                    {
                                        tempMouseDeltaY = getMouseMapping(device, dcs.control, cState, eState, fieldMapping, 0, ctrl);
                                        tempMouseDeltaY = -Math.Abs((tempMouseDeltaY == -2147483648 ? 0 : tempMouseDeltaY));
                                    }

                                    break;
                                }
                            case X360Controls.MouseDown:
                                {
                                    if (tempMouseDeltaY == 0)
                                    {
                                        tempMouseDeltaY = getMouseMapping(device, dcs.control, cState, eState, fieldMapping, 1, ctrl);
                                        tempMouseDeltaY = Math.Abs((tempMouseDeltaY == -2147483648 ? 0 : tempMouseDeltaY));
                                    }

                                    break;
                                }
                            case X360Controls.MouseLeft:
                                {
                                    if (tempMouseDeltaX == 0)
                                    {
                                        tempMouseDeltaX = getMouseMapping(device, dcs.control, cState, eState, fieldMapping, 2, ctrl);
                                        tempMouseDeltaX = -Math.Abs((tempMouseDeltaX == -2147483648 ? 0 : tempMouseDeltaX));
                                    }

                                    break;
                                }
                            case X360Controls.MouseRight:
                                {
                                    if (tempMouseDeltaX == 0)
                                    {
                                        tempMouseDeltaX = getMouseMapping(device, dcs.control, cState, eState, fieldMapping, 3, ctrl);
                                        tempMouseDeltaX = Math.Abs((tempMouseDeltaX == -2147483648 ? 0 : tempMouseDeltaX));
                                    }

                                    break;
                                }

                            default: break;
                        }
                    }

                    if (keyType.HasFlag(DS4KeyType.Toggle))
                    {
                        if (getBoolActionMapping2(device, dcs.control, cState, eState, tp, fieldMapping))
                        {
                            if (!pressedonce[keyvalue])
                            {
                                deviceState.currentClicks.toggle = !deviceState.currentClicks.toggle;
                                pressedonce[keyvalue] = true;
                            }
                            deviceState.currentClicks.toggleCount++;
                        }
                        else
                        {
                            pressedonce[keyvalue] = false;
                        }
                    }

                    // erase default mappings for things that are remapped
                    resetToDefaultValue2(dcs.control, MappedState, outputfieldMapping);
                }
            }
            else
            {
                DS4StateFieldMapping.ControlType controlType = DS4StateFieldMapping.mappedType[(int)dcs.control];
                if (controlType == DS4StateFieldMapping.ControlType.AxisDir)
                //if (dcs.control > DS4Controls.None && dcs.control < DS4Controls.L1)
                {
                    //int current = (int)dcs.control;
                    //outputfieldMapping.axisdirs[current] = fieldMapping.axisdirs[current];
                    customMapQueue[device].Enqueue(new ControlToXInput(dcs.control, dcs.control));
                }
            }
        }

        private static bool IfAxisIsNotModified(int device, bool shift, DS4Controls dc)
        {
            return shift ? false : GetDS4CSetting(device, dc).actionType == DS4ControlSettings.ActionType.Default;
        }

        private static async void MapCustomAction(int device, DS4State cState, DS4State MappedState,
            DS4StateExposed eState, Mouse tp, ControlService ctrl, DS4StateFieldMapping fieldMapping, DS4StateFieldMapping outputfieldMapping)
        {
            /* TODO: This method is slow sauce. Find ways to speed up action execution */
            try
            {
                int actionDoneCount = actionDone.Count;
                int totalActionCount = GetActions().Count;
                DS4StateFieldMapping previousFieldMapping = null;
                List<string> profileActions = getProfileActions(device);
                //foreach (string actionname in profileActions)
                for (int actionIndex = 0, profileListLen = profileActions.Count;
                     actionIndex < profileListLen; actionIndex++)
                {
                    //DS4KeyType keyType = getShiftCustomKeyType(device, customKey.Key);
                    //SpecialAction action = GetAction(actionname);
                    //int index = GetActionIndexOf(actionname);
                    string actionname = profileActions[actionIndex];
                    SpecialAction action = GetProfileAction(device, actionname);
                    int index = GetProfileActionIndexOf(device, actionname);

                    if (actionDoneCount < index + 1)
                    {
                        actionDone.Add(new ActionState());
                        actionDoneCount++;
                    }
                    else if (actionDoneCount > totalActionCount)
                    {
                        actionDone.RemoveAt(actionDoneCount - 1);
                        actionDoneCount--;
                    }

                    if (action == null)
                    {
                        continue;
                    }

                    double time = 0.0;
                    //If a key or button is assigned to the trigger, a key special action is used like
                    //a quick tap to use and hold to use the regular custom button/key
                    bool triggerToBeTapped = action.typeID == SpecialAction.ActionTypeId.None && action.trigger.Count == 1 &&
                            GetDS4CSetting(device, action.trigger[0]).IsDefault;
                    if (!(action.typeID == SpecialAction.ActionTypeId.None || index < 0))
                    {
                        bool triggeractivated = true;
                        if (action.delayTime > 0.0)
                        {
                            triggeractivated = false;
                            bool subtriggeractivated = true;
                            //foreach (DS4Controls dc in action.trigger)
                            for (int i = 0, arlen = action.trigger.Count; i < arlen; i++)
                            {
                                DS4Controls dc = action.trigger[i];
                                if (!getBoolSpecialActionMapping(device, dc, cState, eState, tp, fieldMapping))
                                {
                                    subtriggeractivated = false;
                                    break;
                                }
                            }
                            if (subtriggeractivated)
                            {
                                time = action.delayTime;
                                nowAction[device] = DateTime.UtcNow;
                                if (nowAction[device] >= oldnowAction[device] + TimeSpan.FromSeconds(time))
                                    triggeractivated = true;
                            }
                            else if (nowAction[device] < DateTime.UtcNow - TimeSpan.FromMilliseconds(100))
                                oldnowAction[device] = DateTime.UtcNow;
                        }
                        else if (triggerToBeTapped && oldnowKeyAct[device] == DateTime.MinValue)
                        {
                            triggeractivated = false;
                            bool subtriggeractivated = true;
                            //foreach (DS4Controls dc in action.trigger)
                            for (int i = 0, arlen = action.trigger.Count; i < arlen; i++)
                            {
                                DS4Controls dc = action.trigger[i];
                                if (!getBoolSpecialActionMapping(device, dc, cState, eState, tp, fieldMapping))
                                {
                                    subtriggeractivated = false;
                                    break;
                                }
                            }
                            if (subtriggeractivated)
                            {
                                oldnowKeyAct[device] = DateTime.UtcNow;
                            }
                        }
                        else if (triggerToBeTapped && oldnowKeyAct[device] != DateTime.MinValue)
                        {
                            triggeractivated = false;
                            bool subtriggeractivated = true;
                            //foreach (DS4Controls dc in action.trigger)
                            for (int i = 0, arlen = action.trigger.Count; i < arlen; i++)
                            {
                                DS4Controls dc = action.trigger[i];
                                if (!getBoolSpecialActionMapping(device, dc, cState, eState, tp, fieldMapping))
                                {
                                    subtriggeractivated = false;
                                    break;
                                }
                            }
                            DateTime now = DateTime.UtcNow;
                            if (!subtriggeractivated && now <= oldnowKeyAct[device] + TimeSpan.FromMilliseconds(250))
                            {
                                await Task.Delay(3); //if the button is assigned to the same key use a delay so the key down is the last action, not key up
                                triggeractivated = true;
                                oldnowKeyAct[device] = DateTime.MinValue;
                            }
                            else if (!subtriggeractivated)
                                oldnowKeyAct[device] = DateTime.MinValue;
                        }
                        else
                        {
                            //foreach (DS4Controls dc in action.trigger)
                            for (int i = 0, arlen = action.trigger.Count; i < arlen; i++)
                            {
                                DS4Controls dc = action.trigger[i];
                                if (!getBoolSpecialActionMapping(device, dc, cState, eState, tp, fieldMapping))
                                {
                                    triggeractivated = false;
                                    break;
                                }
                            }

                            // If special action macro is set to run on key release then activate the trigger status only when the trigger key is released
                            if (action.typeID == SpecialAction.ActionTypeId.Macro && action.pressRelease && action.firstTouch)
                                triggeractivated = !triggeractivated;
                        }

                        bool utriggeractivated = true;
                        int uTriggerCount = action.uTrigger.Count;
                        if (action.typeID == SpecialAction.ActionTypeId.Key && uTriggerCount > 0)
                        {
                            //foreach (DS4Controls dc in action.uTrigger)
                            for (int i = 0, arlen = action.uTrigger.Count; i < arlen; i++)
                            {
                                DS4Controls dc = action.uTrigger[i];
                                if (!getBoolSpecialActionMapping(device, dc, cState, eState, tp, fieldMapping))
                                {
                                    utriggeractivated = false;
                                    break;
                                }
                            }
                            if (action.pressRelease) utriggeractivated = !utriggeractivated;
                        }

                        bool actionFound = false;
                        if (triggeractivated)
                        {
                            for (int i = 0, arlen = action.trigger.Count; i < arlen; i++)
                            {
                                DS4Controls dc = action.trigger[i];
                                resetToDefaultValue2(dc, MappedState, outputfieldMapping);
                            }

                            if (action.typeID == SpecialAction.ActionTypeId.Program)
                            {
                                actionFound = true;

                                if (!actionDone[index].dev[device])
                                {
                                    actionDone[index].dev[device] = true;
                                    if (!string.IsNullOrEmpty(action.extra))
                                    {
                                        int pos = action.extra.IndexOf("$hidden", StringComparison.OrdinalIgnoreCase);
                                        if (pos >= 0)
                                        {
                                            System.Diagnostics.Process specActionLaunchProc = new System.Diagnostics.Process();

                                            // LaunchProgram specAction has $hidden argument to indicate that the child process window should be hidden (especially useful when launching .bat/.cmd batch files).
                                            // Removes the first occurence of $hidden substring from extra argument because it was a special action modifier keyword
                                            string cmdArgs = specActionLaunchProc.StartInfo.Arguments = action.extra.Remove(pos, 7);
                                            string cmdExt = Path.GetExtension(action.details).ToLower();

                                            if (cmdExt == ".bat" || cmdExt == ".cmd")
                                            {
                                                // Launch batch script using the default command shell cmd (COMSPEC env variable)
                                                specActionLaunchProc.StartInfo.FileName = System.Environment.GetEnvironmentVariable("COMSPEC");
                                                specActionLaunchProc.StartInfo.Arguments = "/C \"" + action.details + "\" " + cmdArgs;
                                            }
                                            else
                                            {
                                                // Normal EXE executable app (action.details) with optional cmdline arguments (action.extra)
                                                specActionLaunchProc.StartInfo.FileName = action.details;
                                                specActionLaunchProc.StartInfo.Arguments = cmdArgs;
                                            }

                                            // Launch child process using hidden wnd option (the child process should probably do something and then close itself unless you want it to remain hidden in background)
                                            specActionLaunchProc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                                            specActionLaunchProc.StartInfo.CreateNoWindow = true;
                                            specActionLaunchProc.Start();
                                        }                                            
                                        else
                                            // No special process modifiers (ie. $hidden wnd keyword). Launch the child process using the default WinOS settings
                                            Process.Start(action.details, action.extra);
                                    }
                                    else
                                        Process.Start(action.details);
                                }
                            }
                            else if (action.typeID == SpecialAction.ActionTypeId.Profile)
                            {
                                actionFound = true;

                                if (!actionDone[index].dev[device] && (!useTempProfile[device] || untriggeraction[device] == null || untriggeraction[device].typeID != SpecialAction.ActionTypeId.Profile) )
                                {
                                    actionDone[index].dev[device] = true;
                                    // If Loadprofile special action doesn't have untrigger keys or automatic untrigger option is not set then don't set untrigger status. This way the new loaded profile allows yet another loadProfile action key event.
                                    if (action.uTrigger.Count > 0 || action.automaticUntrigger)
                                    {
                                        untriggeraction[device] = action;
                                        untriggerindex[device] = index;

                                        // If the existing profile is a temp profile then store its name, because automaticUntrigger needs to know where to go back (empty name goes back to default regular profile)
                                        untriggeraction[device].prevProfileName = (useTempProfile[device] ? tempprofilename[device] : string.Empty);
                                    }
                                    //foreach (DS4Controls dc in action.trigger)
                                    for (int i = 0, arlen = action.trigger.Count; i < arlen; i++)
                                    {
                                        DS4Controls dc = action.trigger[i];
                                        DS4ControlSettings dcs = GetDS4CSetting(device, dc);
                                        if (dcs.actionType != DS4ControlSettings.ActionType.Default)
                                        {
                                            if (dcs.actionType == DS4ControlSettings.ActionType.Key)
                                                InputMethods.performKeyRelease(ushort.Parse(dcs.action.ToString()));
                                            else if (dcs.actionType == DS4ControlSettings.ActionType.Macro)
                                            {
                                                int[] keys = (int[])dcs.action.actionMacro;
                                                for (int j = 0, keysLen = keys.Length; j < keysLen; j++)
                                                    InputMethods.performKeyRelease((ushort)keys[j]);
                                            }
                                        }
                                    }

                                    DS4Device d = ctrl.DS4Controllers[device];
                                    string prolog = string.Format(DS4WinWPF.Properties.Resources.UsingProfile,
                                        (device + 1).ToString(), action.details, $"{d.Battery}");

                                    AppLogger.LogToGui(prolog, false);
                                    LoadTempProfile(device, action.details, true, ctrl);
                                    //LoadProfile(device, false, ctrl);

                                    if (action.uTrigger.Count == 0 && !action.automaticUntrigger)
                                    {
                                        // If the new profile has any actions with the same action key (controls) than this action (which doesn't have untrigger keys) then set status of those actions to wait for the release of the existing action key. 
                                        List<string> profileActionsNext = getProfileActions(device);
                                        for (int actionIndexNext = 0, profileListLenNext = profileActionsNext.Count; actionIndexNext < profileListLenNext; actionIndexNext++)
                                        {
                                            string actionnameNext = profileActionsNext[actionIndexNext];
                                            SpecialAction actionNext = GetProfileAction(device, actionnameNext);
                                            int indexNext = GetProfileActionIndexOf(device, actionnameNext);

                                            if (actionNext.controls == action.controls)
                                                actionDone[indexNext].dev[device] = true;
                                        }
                                    }

                                    return;
                                }
                            }
                            else if (action.typeID == SpecialAction.ActionTypeId.Macro)
                            {
                                actionFound = true;
                                if (!action.pressRelease)
                                {
                                    // Macro run when trigger keys are pressed down (the default behaviour)
                                    if (!actionDone[index].dev[device])
                                    {
                                        DS4KeyType keyType = action.keyType;
                                        actionDone[index].dev[device] = true;
                                        /*for (int i = 0, arlen = action.trigger.Count; i < arlen; i++)
                                        {
                                            DS4Controls dc = action.trigger[i];
                                            resetToDefaultValue2(dc, MappedState, outputfieldMapping);
                                        }
                                        */

                                        PlayMacro(device, macroControl, String.Empty, action.macro, null, DS4Controls.None, keyType, action, actionDone[index]);
                                    }
                                    else
                                    {
                                        if (!action.keyType.HasFlag(DS4KeyType.RepeatMacro))
                                            EndMacro(device, macroControl, action.macro, DS4Controls.None);
                                    }
                                }
                                else 
                                {
                                    // Macro is run when trigger keys are released (optional behaviour of macro special action))
                                    if (action.firstTouch)
                                    {
                                        action.firstTouch = false;
                                        if (!actionDone[index].dev[device])
                                        {
                                            DS4KeyType keyType = action.keyType;
                                            actionDone[index].dev[device] = true;
                                            /*for (int i = 0, arlen = action.trigger.Count; i < arlen; i++)
                                            {
                                                DS4Controls dc = action.trigger[i];
                                                resetToDefaultValue2(dc, MappedState, outputfieldMapping);
                                            }
                                            */

                                            PlayMacro(device, macroControl, String.Empty, action.macro, null, DS4Controls.None, keyType, action, null);
                                        }
                                    }
                                    else
                                        action.firstTouch = true;
                                }
                            }
                            else if (action.typeID == SpecialAction.ActionTypeId.Key)
                            {
                                actionFound = true;

                                if (uTriggerCount == 0 || (uTriggerCount > 0 && untriggerindex[device] == -1 && !actionDone[index].dev[device]))
                                {
                                    actionDone[index].dev[device] = true;
                                    untriggerindex[device] = index;
                                    ushort key;
                                    ushort.TryParse(action.details, out key);
                                    if (uTriggerCount == 0)
                                    {
                                        SyntheticState.KeyPresses kp;
                                        if (!deviceState[device].keyPresses.TryGetValue(key, out kp))
                                            deviceState[device].keyPresses[key] = kp = new SyntheticState.KeyPresses();
                                        if (action.keyType.HasFlag(DS4KeyType.ScanCode))
                                            kp.current.scanCodeCount++;
                                        else
                                            kp.current.vkCount++;
                                        kp.current.repeatCount++;
                                    }
                                    else if (action.keyType.HasFlag(DS4KeyType.ScanCode))
                                        InputMethods.performSCKeyPress(key);
                                    else
                                        InputMethods.performKeyPress(key);
                                }
                            }
                            else if (action.typeID == SpecialAction.ActionTypeId.DisconnectBT)
                            {
                                actionFound = true;

                                DS4Device d = ctrl.DS4Controllers[device];
                                bool synced = /*tempBool =*/ d.isSynced();
                                if (synced && !d.isCharging())
                                {
                                    ConnectionType deviceConn = d.getConnectionType();
                                    //bool exclusive = /*tempBool =*/ d.isExclusive();
                                    if (deviceConn == ConnectionType.BT)
                                    {
                                        d.DisconnectBT();
                                        ReleaseActionKeys(action, device);
                                        return;
                                    }
                                    else if (deviceConn == ConnectionType.SONYWA)
                                    {
                                        action.pressRelease = true;
                                    }
                                }
                            }
                            else if (action.typeID == SpecialAction.ActionTypeId.BatteryCheck)
                            {
                                actionFound = true;

                                string[] dets = action.details.Split('|');
                                if (dets.Length == 1)
                                    dets = action.details.Split(',');
                                if (bool.Parse(dets[1]) && !actionDone[index].dev[device])
                                {
                                    AppLogger.LogToTray("Controller " + (device + 1) + ": " +
                                        ctrl.getDS4Battery(device), true);
                                }
                                if (bool.Parse(dets[2]))
                                {
                                    DS4Device d = ctrl.DS4Controllers[device];
                                    if (!actionDone[index].dev[device])
                                    {
                                        lastColor[device] = d.LightBarColor;
                                        DS4LightBar.forcelight[device] = true;
                                    }
                                    DS4Color empty = new DS4Color(byte.Parse(dets[3]), byte.Parse(dets[4]), byte.Parse(dets[5]));
                                    DS4Color full = new DS4Color(byte.Parse(dets[6]), byte.Parse(dets[7]), byte.Parse(dets[8]));
                                    DS4Color trans = getTransitionedColor(ref empty, ref full, d.Battery);
                                    if (fadetimer[device] < 100)
                                        DS4LightBar.forcedColor[device] = getTransitionedColor(ref lastColor[device], ref trans, fadetimer[device] += 2);
                                }
                                actionDone[index].dev[device] = true;
                            }
                            else if (action.typeID == SpecialAction.ActionTypeId.SASteeringWheelEmulationCalibrate)
                            {
                                actionFound = true;

                                DS4Device d = ctrl.DS4Controllers[device];
                                // If controller is not already in SASteeringWheelCalibration state then enable it now. If calibration is active then complete it (commit calibration values)
                                if (d.WheelRecalibrateActiveState == 0 && DateTime.UtcNow > (action.firstTap + TimeSpan.FromMilliseconds(3000)))
                                {
                                    action.firstTap = DateTime.UtcNow;
                                    d.WheelRecalibrateActiveState = 1;  // Start calibration process
                                }
                                else if (d.WheelRecalibrateActiveState == 2 && DateTime.UtcNow > (action.firstTap + TimeSpan.FromMilliseconds(3000)))
                                {
                                    action.firstTap = DateTime.UtcNow;
                                    d.WheelRecalibrateActiveState = 3;  // Complete calibration process
                                }

                                actionDone[index].dev[device] = true;
                            }
                        }
                        else
                        {
                            if (action.typeID == SpecialAction.ActionTypeId.BatteryCheck)
                            {
                                actionFound = true;
                                if (actionDone[index].dev[device])
                                {
                                    fadetimer[device] = 0;
                                    /*if (prevFadetimer[device] == fadetimer[device])
                                    {
                                        prevFadetimer[device] = 0;
                                        fadetimer[device] = 0;
                                    }
                                    else
                                        prevFadetimer[device] = fadetimer[device];*/
                                    DS4LightBar.forcelight[device] = false;
                                    actionDone[index].dev[device] = false;
                                }
                            }
                            else if (action.typeID == SpecialAction.ActionTypeId.DisconnectBT && action.pressRelease)
                            {
                                actionFound = true;
                                DS4Device d = ctrl.DS4Controllers[device];
                                ConnectionType deviceConn = d.getConnectionType();
                                if (deviceConn == ConnectionType.SONYWA && d.isSynced())
                                {
                                    if (d.isDS4Idle())
                                    {
                                        d.DisconnectDongle();
                                        ReleaseActionKeys(action, device);
                                        actionDone[index].dev[device] = false;
                                        action.pressRelease = false;
                                    }
                                }
                            }
                            else if (action.typeID != SpecialAction.ActionTypeId.Key &&
                                     action.typeID != SpecialAction.ActionTypeId.XboxGameDVR &&
                                     action.typeID != SpecialAction.ActionTypeId.MultiAction)
                            {
                                // Ignore
                                actionFound = true;
                                actionDone[index].dev[device] = false;
                            }
                        }

                        if (!actionFound)
                        {
                            if (uTriggerCount > 0 && utriggeractivated && action.typeID == SpecialAction.ActionTypeId.Key)
                            {
                                actionFound = true;

                                if (untriggerindex[device] > -1 && !actionDone[index].dev[device])
                                {
                                    actionDone[index].dev[device] = true;
                                    untriggerindex[device] = -1;
                                    ushort key;
                                    ushort.TryParse(action.details, out key);
                                    if (action.keyType.HasFlag(DS4KeyType.ScanCode))
                                        InputMethods.performSCKeyRelease(key);
                                    else
                                        InputMethods.performKeyRelease(key);
                                }
                            }
                            else if (action.typeID == SpecialAction.ActionTypeId.XboxGameDVR || action.typeID == SpecialAction.ActionTypeId.MultiAction)
                            {
                                actionFound = true;

                                bool tappedOnce = action.tappedOnce, firstTouch = action.firstTouch,
                                    secondtouchbegin = action.secondtouchbegin;
                                //DateTime pastTime = action.pastTime, firstTap = action.firstTap,
                                //    TimeofEnd = action.TimeofEnd;

                                /*if (getCustomButton(device, action.trigger[0]) != X360Controls.Unbound)
                                    getCustomButtons(device)[action.trigger[0]] = X360Controls.Unbound;
                                if (getCustomMacro(device, action.trigger[0]) != "0")
                                    getCustomMacros(device).Remove(action.trigger[0]);
                                if (getCustomKey(device, action.trigger[0]) != 0)
                                    getCustomMacros(device).Remove(action.trigger[0]);*/
                                string[] dets = action.details.Split(',');
                                DS4Device d = ctrl.DS4Controllers[device];
                                //cus

                                DS4State tempPrevState = d.getPreviousStateRef();
                                // Only create one instance of previous DS4StateFieldMapping in case more than one multi-action
                                // button is assigned
                                if (previousFieldMapping == null)
                                {
                                    previousFieldMapping = previousFieldMappings[device];
                                    previousFieldMapping.populateFieldMapping(tempPrevState, eState, tp, true);
                                    //previousFieldMapping = new DS4StateFieldMapping(tempPrevState, eState, tp, true);
                                }

                                bool activeCur = getBoolSpecialActionMapping(device, action.trigger[0], cState, eState, tp, fieldMapping);
                                bool activePrev = getBoolSpecialActionMapping(device, action.trigger[0], tempPrevState, eState, tp, previousFieldMapping);
                                if (activeCur && !activePrev)
                                {
                                    // pressed down
                                    action.pastTime = DateTime.UtcNow;
                                    if (action.pastTime <= (action.firstTap + TimeSpan.FromMilliseconds(150)))
                                    {
                                        action.tappedOnce = tappedOnce = false;
                                        action.secondtouchbegin = secondtouchbegin = true;
                                        //tappedOnce = false;
                                        //secondtouchbegin = true;
                                    }
                                    else
                                        action.firstTouch = firstTouch = true;
                                        //firstTouch = true;
                                }
                                else if (!activeCur && activePrev)
                                {
                                    // released
                                    if (secondtouchbegin)
                                    {
                                        action.firstTouch = firstTouch = false;
                                        action.secondtouchbegin = secondtouchbegin = false;
                                        //firstTouch = false;
                                        //secondtouchbegin = false;
                                    }
                                    else if (firstTouch)
                                    {
                                        action.firstTouch = firstTouch = false;
                                        //firstTouch = false;
                                        if (DateTime.UtcNow <= (action.pastTime + TimeSpan.FromMilliseconds(150)) && !tappedOnce)
                                        {
                                            action.tappedOnce = tappedOnce = true;
                                            //tappedOnce = true;
                                            action.firstTap = DateTime.UtcNow;
                                            action.TimeofEnd = DateTime.UtcNow;
                                        }
                                    }
                                }

                                int type = 0;
                                string macro = "";
                                if (tappedOnce) //single tap
                                {
                                    if (action.typeID == SpecialAction.ActionTypeId.MultiAction)
                                    {
                                        macro = dets[0];
                                    }
                                    else if (int.TryParse(dets[0], out type))
                                    {
                                        switch (type)
                                        {
                                            case 0: macro = "91/71/71/91"; break;
                                            case 1: macro = "91/164/82/82/164/91"; break;
                                            case 2: macro = "91/164/44/44/164/91"; break;
                                            case 3: macro = dets[3] + "/" + dets[3]; break;
                                            case 4: macro = "91/164/71/71/164/91"; break;
                                        }
                                    }

                                    if ((DateTime.UtcNow - action.TimeofEnd) > TimeSpan.FromMilliseconds(150))
                                    {
                                        if (macro != "")
                                            PlayMacro(device, macroControl, macro, null, null, DS4Controls.None, DS4KeyType.None);

                                        tappedOnce = false;
                                        action.tappedOnce = false;
                                    }
                                    //if it fails the method resets, and tries again with a new tester value (gives tap a delay so tap and hold can work)
                                }
                                else if (firstTouch && (DateTime.UtcNow - action.pastTime) > TimeSpan.FromMilliseconds(500)) //helddown
                                {
                                    if (action.typeID == SpecialAction.ActionTypeId.MultiAction)
                                    {
                                        macro = dets[1];
                                    }
                                    else if (int.TryParse(dets[1], out type))
                                    {
                                        switch (type)
                                        {
                                            case 0: macro = "91/71/71/91"; break;
                                            case 1: macro = "91/164/82/82/164/91"; break;
                                            case 2: macro = "91/164/44/44/164/91"; break;
                                            case 3: macro = dets[3] + "/" + dets[3]; break;
                                            case 4: macro = "91/164/71/71/164/91"; break;
                                        }
                                    }

                                    if (macro != "")
                                        PlayMacro(device, macroControl, macro, null, null, DS4Controls.None, DS4KeyType.None);

                                    firstTouch = false;
                                    action.firstTouch = false;
                                }
                                else if (secondtouchbegin) //if double tap
                                {
                                    if (action.typeID == SpecialAction.ActionTypeId.MultiAction)
                                    {
                                        macro = dets[2];
                                    }
                                    else if (int.TryParse(dets[2], out type))
                                    {
                                        switch (type)
                                        {
                                            case 0: macro = "91/71/71/91"; break;
                                            case 1: macro = "91/164/82/82/164/91"; break;
                                            case 2: macro = "91/164/44/44/164/91"; break;
                                            case 3: macro = dets[3] + "/" + dets[3]; break;
                                            case 4: macro = "91/164/71/71/164/91"; break;
                                        }
                                    }

                                    if (macro != "")
                                        PlayMacro(device, macroControl, macro, null, null, DS4Controls.None, DS4KeyType.None);

                                    secondtouchbegin = false;
                                    action.secondtouchbegin = false;
                                }
                            }
                            else
                            {
                                actionDone[index].dev[device] = false;
                            }
                        }
                    }
                }
            }
            catch { return; }

            if (untriggeraction[device] != null)
            {
                SpecialAction action = untriggeraction[device];
                int index = untriggerindex[device];
                bool utriggeractivated;

                if (!action.automaticUntrigger)
                {
                    // Untrigger keys defined and auto-untrigger (=unload) profile option is NOT set. Unload a temporary profile only when specified untrigger keys have been triggered.
                    utriggeractivated = true;

                    //foreach (DS4Controls dc in action.uTrigger)
                    for (int i = 0, uTrigLen = action.uTrigger.Count; i < uTrigLen; i++)
                    {
                        DS4Controls dc = action.uTrigger[i];
                        if (!getBoolSpecialActionMapping(device, dc, cState, eState, tp, fieldMapping))
                        {
                            utriggeractivated = false;
                            break;
                        }
                    }
                }
                else
                {
                    // Untrigger as soon any of the defined regular trigger keys have been released. 
                    utriggeractivated = false;

                    for (int i = 0, trigLen = action.trigger.Count; i < trigLen; i++)
                    {
                        DS4Controls dc = action.trigger[i];
                        if (!getBoolSpecialActionMapping(device, dc, cState, eState, tp, fieldMapping))
                        {
                            utriggeractivated = true;
                            break;
                        }
                    }
                }

                if (utriggeractivated && action.typeID == SpecialAction.ActionTypeId.Profile)
                {
                    if ((action.controls == action.ucontrols && !actionDone[index].dev[device]) || //if trigger and end trigger are the same
                    action.controls != action.ucontrols)
                    {
                        if (useTempProfile[device])
                        {
                            //foreach (DS4Controls dc in action.uTrigger)
                            for (int i = 0, arlen = action.uTrigger.Count; i < arlen; i++)
                            {
                                DS4Controls dc = action.uTrigger[i];
                                actionDone[index].dev[device] = true;
                                DS4ControlSettings dcs = GetDS4CSetting(device, dc);
                                if (dcs.actionType != DS4ControlSettings.ActionType.Default)
                                {
                                    if (dcs.actionType == DS4ControlSettings.ActionType.Key)
                                        InputMethods.performKeyRelease((ushort)dcs.action.actionKey);
                                    else if (dcs.actionType == DS4ControlSettings.ActionType.Macro)
                                    {
                                        int[] keys = dcs.action.actionMacro;
                                        for (int j = 0, keysLen = keys.Length; j < keysLen; j++)
                                            InputMethods.performKeyRelease((ushort)keys[j]);
                                    }
                                }
                            }

                            string profileName = untriggeraction[device].prevProfileName;
                            DS4Device d = ctrl.DS4Controllers[device];
                            string prolog = string.Format(DS4WinWPF.Properties.Resources.UsingProfile,
                                (device + 1).ToString(), (profileName == string.Empty ? ProfilePath[device] : profileName), $"{d.Battery}");

                            AppLogger.LogToGui(prolog, false);

                            untriggeraction[device] = null;

                            if (profileName == string.Empty)
                                LoadProfile(device, false, ctrl); // Previous profile was a regular default profile of a controller
                            else
                                LoadTempProfile(device, profileName, true, ctrl); // Previous profile was a temporary profile, so re-load it as a temp profile
                        }
                    }
                }
                else
                {
                    actionDone[index].dev[device] = false;
                }
            }
        }

        private static void ReleaseActionKeys(SpecialAction action, int device)
        {
            //foreach (DS4Controls dc in action.trigger)
            for (int i = 0, arlen = action.trigger.Count; i < arlen; i++)
            {
                DS4Controls dc = action.trigger[i];
                DS4ControlSettings dcs = GetDS4CSetting(device, dc);
                if (dcs.actionType != DS4ControlSettings.ActionType.Default)
                {
                    if (dcs.actionType == DS4ControlSettings.ActionType.Key)
                        InputMethods.performKeyRelease((ushort)dcs.action.actionKey);
                    else if (dcs.actionType == DS4ControlSettings.ActionType.Macro)
                    {
                        int[] keys = dcs.action.actionMacro;
                        for (int j = 0, keysLen = keys.Length; j < keysLen; j++)
                            InputMethods.performKeyRelease((ushort)keys[j]);
                    }
                }
            }
        }

        // Play macro as a background task. Optionally the new macro play waits for completion of a previous macro execution (synchronized macro special action). 
        // Macro steps are defined either as macrostr string value, macroLst list<int> object or as macroArr integer array. Only one of these should have a valid macro definition when this method is called.
        // If the macro definition is a macroStr string value then it will be converted as integer array on the fl. If steps are already defined as list or array of integers then there is no need to do type cast conversion.
        private static void PlayMacro(int device, bool[] macrocontrol, string macroStr, List<int> macroLst, int[] macroArr, DS4Controls control, DS4KeyType keyType, SpecialAction action = null, ActionState actionDoneState = null)
        {
            if (action != null && action.synchronized)
            {
                // Run special action macros in synchronized order (ie. FirstIn-FirstOut). The trigger control name string is the execution queue identifier (ie. each unique trigger combination has an own synchronization queue).
                if (!macroTaskQueue[device].TryGetValue(action.controls, out Task prevTask))
                    macroTaskQueue[device].Add(action.controls, (Task.Factory.StartNew(() => PlayMacroTask(device, macroControl, macroStr, macroLst, macroArr, control, keyType, action, actionDoneState))) );
                else
                    macroTaskQueue[device][action.controls] = prevTask.ContinueWith((x) => PlayMacroTask(device, macroControl, macroStr, macroLst, macroArr, control, keyType, action, actionDoneState));                       
            }
            else
                // Run macro as "fire and forget" background task. No need to wait for completion of any of the other macros. 
                // If the same trigger macro is re-launched while previous macro is still running then the order of parallel macros is not guaranteed.
                Task.Factory.StartNew(() => PlayMacroTask(device, macroControl, macroStr, macroLst, macroArr, control, keyType, action, actionDoneState));
        }

        // Play through a macro. The macro steps are defined either as string, List or Array object (always only one of those parameters is set to a valid value)
        private static void PlayMacroTask(int device, bool[] macrocontrol, string macroStr, List<int> macroLst, int[] macroArr, DS4Controls control, DS4KeyType keyType, SpecialAction action, ActionState actionDoneState)
        {
            if(!String.IsNullOrEmpty(macroStr))
            {
                string[] skeys;

                skeys = macroStr.Split('/');
                macroArr = new int[skeys.Length];
                for (int i = 0; i < macroArr.Length; i++)
                    macroArr[i] = int.Parse(skeys[i]);
            }

            // macro.StartsWith("164/9/9/164") || macro.StartsWith("18/9/9/18")
            if ( (macroLst != null && macroLst.Count >= 4 && ((macroLst[0] == 164 && macroLst[1] == 9 && macroLst[2] == 9 && macroLst[3] == 164) || (macroLst[0] == 18 && macroLst[1] == 9 && macroLst[2] == 9 && macroLst[3] == 18))) 
              || (macroArr != null && macroArr.Length>= 4 && ((macroArr[0] == 164 && macroArr[1] == 9 && macroArr[2] == 9 && macroArr[3] == 164) || (macroArr[0] == 18 && macroArr[1] == 9 && macroArr[2] == 9 && macroArr[3] == 18)))
            )
            {
                int wait;
                if(macroLst != null)
                    wait = macroLst[macroLst.Count - 1];
                else
                    wait = macroArr[macroArr.Length - 1];

                if (wait <= 300 || wait > ushort.MaxValue)
                    wait = 1000;
                else
                    wait -= 300;

                AltTabSwapping(wait, device);
                if (control != DS4Controls.None)
                    macrodone[DS4ControltoInt(control)] = true;
            }
            else if(control == DS4Controls.None || !macrodone[DS4ControltoInt(control)])
            {
                int macroCodeValue;
                bool[] keydown = new bool[286];

                if (control != DS4Controls.None)
                    macrodone[DS4ControltoInt(control)] = true;

                // Play macro codes and simulate key down/up events (note! The same key may go through several up and down events during the same macro).
                // If the return value is TRUE then this method should do a asynchronized delay (the usual Thread.Sleep doesnt work here because it would block the main gamepad reading thread).
                if (macroLst != null)
                {
                    for (int i = 0; i < macroLst.Count; i++)
                    {
                        macroCodeValue = macroLst[i];
                        if (PlayMacroCodeValue(device, macrocontrol, keyType, macroCodeValue, keydown))
                            Task.Delay(macroCodeValue - 300).Wait();
                    }
                }
                else
                {
                    for (int i = 0; i < macroArr.Length; i++)
                    {
                        macroCodeValue = macroArr[i];
                        if (PlayMacroCodeValue(device, macrocontrol, keyType, macroCodeValue, keydown))
                            Task.Delay(macroCodeValue - 300).Wait();
                    }
                }

                // The macro is finished. If any of the keys is still in down state then release a key state (ie. simulate key up event) unless special action specified to keep the last state as it is left in a macro
                if (action == null || !action.keepKeyState)
                {
                    for (int i = 0, arlength = keydown.Length; i < arlength; i++)
                    {
                        if (keydown[i])
                            PlayMacroCodeValue(device, macrocontrol, keyType, i, keydown);
                    }

                    // Reset lightbar back to a default value (if the macro modified the color) because keepKeyState macro option was not set
                    DS4LightBar.forcedFlash[device] = 0;
                    DS4LightBar.forcelight[device] = false;
                }

                // Commented out rumble reset. No need to zero out rumble after a macro because it may conflict with a game generated rumble events (ie. macro would stop a game generated rumble effect).
                // If macro generates rumble effects then the macro can stop the rumble as a last step or wait for rumble watchdog timer to do it after few seconds.
                //Program.rootHub.DS4Controllers[device].setRumble(0, 0);

                if (keyType.HasFlag(DS4KeyType.HoldMacro))
                {
                    Task.Delay(50).Wait();
                    if (control != DS4Controls.None)
                        macrodone[DS4ControltoInt(control)] = false;
                }
            }

            // If a special action type of Macro has "Repeat while held" option and actionDoneState object is defined then reset the action back to "not done" status in order to re-fire it if the trigger key is still held down
            if (actionDoneState != null && keyType.HasFlag(DS4KeyType.RepeatMacro))
                actionDoneState.dev[device] = false;
        }

        private static bool PlayMacroCodeValue(int device, bool[] macrocontrol, DS4KeyType keyType, int macroCodeValue, bool[] keydown)
        {
            bool doDelayOnCaller = false;
            if (macroCodeValue >= 261 && macroCodeValue <= 285)
            {
                // Gamepad button up or down macro event. macroCodeValue index value is the button identifier (codeValue-261 = idx in 0..24 range)
                if (!keydown[macroCodeValue])
                {
                    macroControl[macroCodeValue - 261] = keydown[macroCodeValue] = true;
                    macroCount++;
                }
                else
                {
                    macroControl[macroCodeValue - 261] = keydown[macroCodeValue] = false;
                    if (macroCount > 0) macroCount--;
                }
            }
            else if (macroCodeValue < 300)
            {
                // Keyboard key or mouse button macro event
                if (!keydown[macroCodeValue])
                {
                    switch (macroCodeValue)
                    {
                        //anything above 255 is not a keyvalue
                        case 256: InputMethods.MouseEvent(InputMethods.MOUSEEVENTF_LEFTDOWN); break;
                        case 257: InputMethods.MouseEvent(InputMethods.MOUSEEVENTF_RIGHTDOWN); break;
                        case 258: InputMethods.MouseEvent(InputMethods.MOUSEEVENTF_MIDDLEDOWN); break;
                        case 259: InputMethods.MouseEvent(InputMethods.MOUSEEVENTF_XBUTTONDOWN, 1); break;
                        case 260: InputMethods.MouseEvent(InputMethods.MOUSEEVENTF_XBUTTONDOWN, 2); break;

                        default:
                            if (keyType.HasFlag(DS4KeyType.ScanCode)) InputMethods.performSCKeyPress((ushort)macroCodeValue);
                            else InputMethods.performKeyPress((ushort)macroCodeValue);
                            break;
                    }
                    keydown[macroCodeValue] = true;
                }
                else
                {
                    switch (macroCodeValue)
                    {
                        //anything above 255 is not a keyvalue
                        case 256: InputMethods.MouseEvent(InputMethods.MOUSEEVENTF_LEFTUP); break;
                        case 257: InputMethods.MouseEvent(InputMethods.MOUSEEVENTF_RIGHTUP); break;
                        case 258: InputMethods.MouseEvent(InputMethods.MOUSEEVENTF_MIDDLEUP); break;
                        case 259: InputMethods.MouseEvent(InputMethods.MOUSEEVENTF_XBUTTONUP, 1); break;
                        case 260: InputMethods.MouseEvent(InputMethods.MOUSEEVENTF_XBUTTONUP, 2); break;

                        default:
                            if (keyType.HasFlag(DS4KeyType.ScanCode)) InputMethods.performSCKeyRelease((ushort)macroCodeValue);
                            else InputMethods.performKeyRelease((ushort)macroCodeValue);
                            break;
                    }
                    keydown[macroCodeValue] = false;
                }
            }
            else if (macroCodeValue >= 1000000000)
            {
                // Lightbar color event
                if (macroCodeValue > 1000000000)
                {
                    string lb = macroCodeValue.ToString().Substring(1);
                    byte r = (byte)(int.Parse(lb[0].ToString()) * 100 + int.Parse(lb[1].ToString()) * 10 + int.Parse(lb[2].ToString()));
                    byte g = (byte)(int.Parse(lb[3].ToString()) * 100 + int.Parse(lb[4].ToString()) * 10 + int.Parse(lb[5].ToString()));
                    byte b = (byte)(int.Parse(lb[6].ToString()) * 100 + int.Parse(lb[7].ToString()) * 10 + int.Parse(lb[8].ToString()));
                    DS4LightBar.forcelight[device] = true;
                    DS4LightBar.forcedFlash[device] = 0;
                    DS4LightBar.forcedColor[device] = new DS4Color(r, g, b);
                }
                else
                {
                    DS4LightBar.forcedFlash[device] = 0;
                    DS4LightBar.forcelight[device] = false;
                }
            }
            else if (macroCodeValue >= 1000000)
            {
                // Rumble event
                DS4Device d = Program.rootHub.DS4Controllers[device];
                string r = macroCodeValue.ToString().Substring(1);
                byte heavy = (byte)(int.Parse(r[0].ToString()) * 100 + int.Parse(r[1].ToString()) * 10 + int.Parse(r[2].ToString()));
                byte light = (byte)(int.Parse(r[3].ToString()) * 100 + int.Parse(r[4].ToString()) * 10 + int.Parse(r[5].ToString()));
                d.setRumble(light, heavy);
            }
            else
            {
                // Delay specification. Indicate to caller that it should do a delay of macroCodeValue-300 msecs
                doDelayOnCaller = true;
            }

            return doDelayOnCaller;
        }

        private static void EndMacro(int device, bool[] macrocontrol, string macro, DS4Controls control)
        {
            if ((macro.StartsWith("164/9/9/164") || macro.StartsWith("18/9/9/18")) && !altTabDone)
                AltTabSwappingRelease();

            if (control != DS4Controls.None)
                macrodone[DS4ControltoInt(control)] = false;
        }

        private static void EndMacro(int device, bool[] macrocontrol, List<int> macro, DS4Controls control)
        {
            if(macro.Count >= 4 && ((macro[0] == 164 && macro[1] == 9 && macro[2] == 9 && macro[3] == 164) || (macro[0] == 18 && macro[1] == 9 && macro[2] == 9 && macro[3] == 18)) && !altTabDone)
                AltTabSwappingRelease();

            if (control != DS4Controls.None)
                macrodone[DS4ControltoInt(control)] = false;
        }

        private static void EndMacro(int device, bool[] macrocontrol, int[] macro, DS4Controls control)
        {
            if (macro.Length >= 4 && ((macro[0] == 164 && macro[1] == 9 && macro[2] == 9 && macro[3] == 164) || (macro[0] == 18 && macro[1] == 9 && macro[2] == 9 && macro[3] == 18)) && !altTabDone)
                AltTabSwappingRelease();

            if (control != DS4Controls.None)
                macrodone[DS4ControltoInt(control)] = false;
        }

        private static void AltTabSwapping(int wait, int device)
        {
            if (altTabDone)
            {
                altTabDone = false;
                InputMethods.performKeyPress(18);
            }
            else
            {
                altTabNow = DateTime.UtcNow;
                if (altTabNow >= oldAltTabNow + TimeSpan.FromMilliseconds(wait))
                {
                    oldAltTabNow = altTabNow;
                    InputMethods.performKeyPress(9);
                    InputMethods.performKeyRelease(9);
                }
            }
        }

        private static void AltTabSwappingRelease()
        {
            if (altTabNow < DateTime.UtcNow - TimeSpan.FromMilliseconds(10)) //in case multiple controls are mapped to alt+tab
            {
                altTabDone = true;
                InputMethods.performKeyRelease(9);
                InputMethods.performKeyRelease(18);
                altTabNow = DateTime.UtcNow;
                oldAltTabNow = DateTime.UtcNow - TimeSpan.FromDays(1);
            }
        }

        private static void getMouseWheelMapping(int device, DS4Controls control, DS4State cState,
            DS4StateExposed eState, Mouse tp, bool down)
        {
            DateTime now = DateTime.UtcNow;
            if (now >= oldnow + TimeSpan.FromMilliseconds(10) && !pressagain)
            {
                oldnow = now;
                InputMethods.MouseWheel((int)(getByteMapping(device, control, cState, eState, tp) / 8.0f * (down ? -1 : 1)), 0);
            }
        }

        private static double getMouseMapping(int device, DS4Controls control, DS4State cState, DS4StateExposed eState,
            DS4StateFieldMapping fieldMapping, int mnum, ControlService ctrl)
        {
            int deadzoneL = 0;
            int deadzoneR = 0;
            if (getLSDeadzone(device) == 0)
                deadzoneL = 3;
            if (getRSDeadzone(device) == 0)
                deadzoneR = 3;

            double value = 0.0;
            ButtonMouseInfo buttonMouseInfo = ButtonMouseInfos[device];
            int speed = buttonMouseInfo.activeButtonSensitivity;
            const double root = 1.002;
            const double divide = 10000d;

            int controlNum = (int)control;
            DS4StateFieldMapping.ControlType controlType = DS4StateFieldMapping.mappedType[controlNum];
            //long timeElapsed = ctrl.DS4Controllers[device].getLastTimeElapsed();
            double timeElapsed = ctrl.DS4Controllers[device].lastTimeElapsedDouble;
            //double mouseOffset = 0.025;
            double tempMouseOffsetX = 0.0, tempMouseOffsetY = 0.0;
            double mouseVerticalScale = 1.0;
            bool verticalDir = false;
            // 0 = MouseUp, 1 = MouseDown
            if (mnum == 0 || mnum == 1)
            {
                verticalDir = true;
                mouseVerticalScale = buttonMouseInfo.buttonVerticalScale;
            }

            if (controlType == DS4StateFieldMapping.ControlType.Button)
            {
                bool active = fieldMapping.buttons[controlNum];
                value = (active ? Math.Pow(root + speed / divide, 100) - 1 : 0);
                if (verticalDir) value *= mouseVerticalScale;
            }
            else if (controlType == DS4StateFieldMapping.ControlType.AxisDir)
            {
                double timeDelta = timeElapsed * 0.001;
                int mouseVelocity = speed * MOUSESPEEDFACTOR;
                double mouseOffset = buttonMouseInfo.mouseVelocityOffset * mouseVelocity;
                if (verticalDir) mouseVelocity = (int)(mouseVelocity * mouseVerticalScale);

                //double mouseOffset = MOUSESTICKANTIOFFSET * mouseVelocity;
                // Cap mouse offset to final mouse velocity
                //double mouseOffset = mouseVelocity >= MOUSESTICKMINVELOCITY ? MOUSESTICKMINVELOCITY : mouseVelocity;

                switch (control)
                {
                    case DS4Controls.LXNeg:
                    {
                        if (cState.LX < 128 - deadzoneL)
                        {
                            double diff = -(cState.LX - 128 - deadzoneL) / (double)(0 - 128 - deadzoneL);
                            //tempMouseOffsetX = Math.Abs(Math.Cos(cState.LSAngleRad)) * MOUSESTICKOFFSET;
                            //tempMouseOffsetX = MOUSESTICKOFFSET;
                            tempMouseOffsetX = cState.LXUnit * mouseOffset;
                            value = (mouseVelocity - tempMouseOffsetX) * timeDelta * diff + (tempMouseOffsetX * -1.0 * timeDelta);
                            //value = diff * MOUSESPEEDFACTOR * (timeElapsed * 0.001) * speed;
                            //value = -(cState.LX - 127 - deadzoneL) / 2550d * speed;
                        }

                        break;
                    }
                    case DS4Controls.LXPos:
                    {
                        if (cState.LX > 128 + deadzoneL)
                        {
                            double diff = (cState.LX - 128 + deadzoneL) / (double)(255 - 128 + deadzoneL);
                            tempMouseOffsetX = cState.LXUnit * mouseOffset;
                            //tempMouseOffsetX = Math.Abs(Math.Cos(cState.LSAngleRad)) * MOUSESTICKOFFSET;
                            //tempMouseOffsetX = MOUSESTICKOFFSET;
                            value = (mouseVelocity - tempMouseOffsetX) * timeDelta * diff + (tempMouseOffsetX * timeDelta);
                            //value = diff * MOUSESPEEDFACTOR * (timeElapsed * 0.001) * speed;
                            //value = (cState.LX - 127 + deadzoneL) / 2550d * speed;
                        }

                        break;
                    }
                    case DS4Controls.RXNeg:
                    {
                        if (cState.RX < 128 - deadzoneR)
                        {
                            double diff = -(cState.RX - 128 - deadzoneR) / (double)(0 - 128 - deadzoneR);
                            tempMouseOffsetX = cState.RXUnit * mouseOffset;
                            //tempMouseOffsetX = MOUSESTICKOFFSET;
                            //tempMouseOffsetX = Math.Abs(Math.Cos(cState.RSAngleRad)) * MOUSESTICKOFFSET;
                            value = (mouseVelocity - tempMouseOffsetX) * timeDelta * diff + (tempMouseOffsetX * -1.0 * timeDelta);
                            //value = diff * MOUSESPEEDFACTOR * (timeElapsed * 0.001) * speed;
                            //value = -(cState.RX - 127 - deadzoneR) / 2550d * speed;
                        }

                        break;
                    }
                    case DS4Controls.RXPos:
                    {
                        if (cState.RX > 128 + deadzoneR)
                        {
                            double diff = (cState.RX - 128 + deadzoneR) / (double)(255 - 128 + deadzoneR);
                            tempMouseOffsetX = cState.RXUnit * mouseOffset;
                            //tempMouseOffsetX = MOUSESTICKOFFSET;
                            //tempMouseOffsetX = Math.Abs(Math.Cos(cState.RSAngleRad)) * MOUSESTICKOFFSET;
                            value = (mouseVelocity - tempMouseOffsetX) * timeDelta * diff + (tempMouseOffsetX * timeDelta);
                            //value = diff * MOUSESPEEDFACTOR * (timeElapsed * 0.001) * speed;
                            //value = (cState.RX - 127 + deadzoneR) / 2550d * speed;
                        }

                        break;
                    }
                    case DS4Controls.LYNeg:
                    {
                        if (cState.LY < 128 - deadzoneL)
                        {
                            double diff = -(cState.LY - 128 - deadzoneL) / (double)(0 - 128 - deadzoneL);
                            tempMouseOffsetY = cState.LYUnit * mouseOffset;
                            //tempMouseOffsetY = MOUSESTICKOFFSET;
                            //tempMouseOffsetY = Math.Abs(Math.Sin(cState.LSAngleRad)) * MOUSESTICKOFFSET;
                            value = (mouseVelocity - tempMouseOffsetY) * timeDelta * diff + (tempMouseOffsetY * -1.0 * timeDelta);
                            //value = diff * MOUSESPEEDFACTOR * (timeElapsed * 0.001) * speed;
                            //value = -(cState.LY - 127 - deadzoneL) / 2550d * speed;
                        }

                        break;
                    }
                    case DS4Controls.LYPos:
                    {
                        if (cState.LY > 128 + deadzoneL)
                        {
                            double diff = (cState.LY - 128 + deadzoneL) / (double)(255 - 128 + deadzoneL);
                            tempMouseOffsetY = cState.LYUnit * mouseOffset;
                            //tempMouseOffsetY = MOUSESTICKOFFSET;
                            //tempMouseOffsetY = Math.Abs(Math.Sin(cState.LSAngleRad)) * MOUSESTICKOFFSET;
                            value = (mouseVelocity - tempMouseOffsetY) * timeDelta * diff + (tempMouseOffsetY * timeDelta);
                            //value = diff * MOUSESPEEDFACTOR * (timeElapsed * 0.001) * speed;
                            //value = (cState.LY - 127 + deadzoneL) / 2550d * speed;
                        }

                        break;
                    }
                    case DS4Controls.RYNeg:
                    {
                        if (cState.RY < 128 - deadzoneR)
                        {
                            double diff = -(cState.RY - 128 - deadzoneR) / (double)(0 - 128 - deadzoneR);
                            tempMouseOffsetY = cState.RYUnit * mouseOffset;
                            //tempMouseOffsetY = MOUSESTICKOFFSET;
                            //tempMouseOffsetY = Math.Abs(Math.Sin(cState.RSAngleRad)) * MOUSESTICKOFFSET;
                            value = (mouseVelocity - tempMouseOffsetY) * timeDelta * diff + (tempMouseOffsetY * -1.0 * timeDelta);
                            //value = diff * MOUSESPEEDFACTOR * (timeElapsed * 0.001) * speed;
                            //value = -(cState.RY - 127 - deadzoneR) / 2550d * speed;
                        }

                        break;
                    }
                    case DS4Controls.RYPos:
                    {
                        if (cState.RY > 128 + deadzoneR)
                        {
                            double diff = (cState.RY - 128 + deadzoneR) / (double)(255 - 128 + deadzoneR);
                            tempMouseOffsetY = cState.RYUnit * mouseOffset;
                            //tempMouseOffsetY = MOUSESTICKOFFSET;
                            //tempMouseOffsetY = Math.Abs(Math.Sin(cState.RSAngleRad)) * MOUSESTICKOFFSET;
                            value = (mouseVelocity - tempMouseOffsetY) * timeDelta * diff + (tempMouseOffsetY * timeDelta);
                            //value = diff * MOUSESPEEDFACTOR * (timeElapsed * 0.001) * speed;
                            //value = (cState.RY - 127 + deadzoneR) / 2550d * speed;
                        }

                        break;
                    }

                    default: break;
                }
            }
            else if (controlType == DS4StateFieldMapping.ControlType.Trigger)
            {
                byte trigger = fieldMapping.triggers[controlNum];
                value = Math.Pow(root + speed / divide, trigger / 2d) - 1;
                if (verticalDir) value *= mouseVerticalScale;
            }
            else if (controlType == DS4StateFieldMapping.ControlType.GyroDir)
            {
                //double SXD = getSXDeadzone(device);
                //double SZD = getSZDeadzone(device);

                switch (control)
                {
                    case DS4Controls.GyroXPos:
                    {
                        int gyroX = fieldMapping.gryodirs[controlNum];
                        value = (byte)(gyroX > 0 ? Math.Pow(root + speed / divide, gyroX) : 0);
                        if (verticalDir) value *= mouseVerticalScale;
                        break;
                    }
                    case DS4Controls.GyroXNeg:
                    {
                        int gyroX = fieldMapping.gryodirs[controlNum];
                        value = (byte)(gyroX < 0 ? Math.Pow(root + speed / divide, -gyroX) : 0);
                        if (verticalDir) value *= mouseVerticalScale;
                        break;
                    }
                    case DS4Controls.GyroZPos:
                    {
                        int gyroZ = fieldMapping.gryodirs[controlNum];
                        value = (byte)(gyroZ > 0 ? Math.Pow(root + speed / divide, gyroZ) : 0);
                        if (verticalDir) value *= mouseVerticalScale;
                        break;
                    }
                    case DS4Controls.GyroZNeg:
                    {
                        int gyroZ = fieldMapping.gryodirs[controlNum];
                        value = (byte)(gyroZ < 0 ? Math.Pow(root + speed / divide, -gyroZ) : 0);
                        if (verticalDir) value *= mouseVerticalScale;
                        break;
                    }
                    default: break;
                }
            }

            if (buttonMouseInfo.mouseAccel)
            {
                if (value > 0)
                {
                    mcounter = 34;
                    mouseaccel++;
                }

                if (mouseaccel == prevmouseaccel)
                {
                    mcounter--;
                }

                if (mcounter <= 0)
                {
                    mouseaccel = 0;
                    mcounter = 34;
                }

                value *= 1 + Math.Min(20000, (mouseaccel)) / 10000d;
                prevmouseaccel = mouseaccel;
            }

            return value;
        }

        private static void calculateFinalMouseMovement(ref double rawMouseX, ref double rawMouseY,
            out int mouseX, out int mouseY)
        {
            if ((rawMouseX > 0.0 && horizontalRemainder > 0.0) || (rawMouseX < 0.0 && horizontalRemainder < 0.0))
            {
                rawMouseX += horizontalRemainder;
            }
            else
            {
                horizontalRemainder = 0.0;
            }

            //double mouseXTemp = rawMouseX - (Math.IEEERemainder(rawMouseX * 1000.0, 1.0) / 1000.0);
            double mouseXTemp = rawMouseX - (remainderCutoff(rawMouseX * 1000.0, 1.0) / 1000.0);
            //double mouseXTemp = rawMouseX - (rawMouseX * 1000.0 - (1.0 * (int)(rawMouseX * 1000.0 / 1.0)));
            mouseX = (int)mouseXTemp;
            horizontalRemainder = mouseXTemp - mouseX;
            //mouseX = (int)rawMouseX;
            //horizontalRemainder = rawMouseX - mouseX;

            if ((rawMouseY > 0.0 && verticalRemainder > 0.0) || (rawMouseY < 0.0 && verticalRemainder < 0.0))
            {
                rawMouseY += verticalRemainder;
            }
            else
            {
                verticalRemainder = 0.0;
            }

            //double mouseYTemp = rawMouseY - (Math.IEEERemainder(rawMouseY * 1000.0, 1.0) / 1000.0);
            double mouseYTemp = rawMouseY - (remainderCutoff(rawMouseY * 1000.0, 1.0) / 1000.0);
            mouseY = (int)mouseYTemp;
            verticalRemainder = mouseYTemp - mouseY;
            //mouseY = (int)rawMouseY;
            //verticalRemainder = rawMouseY - mouseY;
        }

        private static double remainderCutoff(double dividend, double divisor)
        {
            return dividend - (divisor * (int)(dividend / divisor));
        }

        public static bool compare(byte b1, byte b2)
        {
            bool result = true;
            if (Math.Abs(b1 - b2) > 10)
            {
                result = false;
            }

            return result;
        }

        private static byte getByteMapping2(int device, DS4Controls control, DS4State cState, DS4StateExposed eState, Mouse tp,
            DS4StateFieldMapping fieldMap)
        {
            byte result = 0;

            int controlNum = (int)control;
            DS4StateFieldMapping.ControlType controlType = DS4StateFieldMapping.mappedType[controlNum];
            if (controlType == DS4StateFieldMapping.ControlType.Button)
            {
                result = (byte)(fieldMap.buttons[controlNum] ? 255 : 0);
            }
            else if (controlType == DS4StateFieldMapping.ControlType.AxisDir)
            {
                byte axisValue = fieldMap.axisdirs[controlNum];

                switch (control)
                {
                    case DS4Controls.LXNeg: result = (byte)(axisValue - 128.0f >= 0 ? 0 : -(axisValue - 128.0f) * 1.9921875f); break;
                    case DS4Controls.LYNeg: result = (byte)(axisValue - 128.0f >= 0 ? 0 : -(axisValue - 128.0f) * 1.9921875f); break;
                    case DS4Controls.RXNeg: result = (byte)(axisValue - 128.0f >= 0 ? 0 : -(axisValue - 128.0f) * 1.9921875f); break;
                    case DS4Controls.RYNeg: result = (byte)(axisValue - 128.0f >= 0 ? 0 : -(axisValue - 128.0f) * 1.9921875f); break;
                    default: result = (byte)(axisValue - 128.0f < 0 ? 0 : (axisValue - 128.0f) * 2.0078740157480315f); break;
                }
            }
            else if (controlType == DS4StateFieldMapping.ControlType.Trigger)
            {
                result = fieldMap.triggers[controlNum];
            }
            else if (controlType == DS4StateFieldMapping.ControlType.Touch)
            {
                result = (byte)(tp != null && fieldMap.buttons[controlNum] ? 255 : 0);
            }
            else if (controlType == DS4StateFieldMapping.ControlType.SwipeDir)
            {
                result = (byte)(tp != null ? fieldMap.swipedirs[controlNum] : 0);
            }
            else if (controlType == DS4StateFieldMapping.ControlType.GyroDir)
            {
                bool saControls = IsUsingSAForControls(device);

                switch (control)
                {
                    case DS4Controls.GyroXPos:
                    {
                        int gyroX = fieldMap.gryodirs[controlNum];
                        result = (byte)(saControls ? Math.Min(255, gyroX * 2) : 0);
                        break;
                    }
                    case DS4Controls.GyroXNeg:
                    {
                        int gyroX = fieldMap.gryodirs[controlNum];
                        result = (byte)(saControls ? Math.Min(255, -gyroX * 2) : 0);
                        break;
                    }
                    case DS4Controls.GyroZPos:
                    {
                        int gyroZ = fieldMap.gryodirs[controlNum];
                        result = (byte)(saControls ? Math.Min(255, gyroZ * 2) : 0);
                        break;
                    }
                    case DS4Controls.GyroZNeg:
                    {
                        int gyroZ = fieldMap.gryodirs[controlNum];
                        result = (byte)(saControls ? Math.Min(255, -gyroZ * 2) : 0);
                        break;
                    }
                    default: break;
                }
            }

            return result;
        }

        public static byte getByteMapping(int device, DS4Controls control, DS4State cState, DS4StateExposed eState, Mouse tp)
        {
            byte result = 0;

            if (control >= DS4Controls.Square && control <= DS4Controls.Cross)
            {
                switch (control)
                {
                    case DS4Controls.Cross: result = (byte)(cState.Cross ? 255 : 0); break;
                    case DS4Controls.Square: result = (byte)(cState.Square ? 255 : 0); break;
                    case DS4Controls.Triangle: result = (byte)(cState.Triangle ? 255 : 0); break;
                    case DS4Controls.Circle: result = (byte)(cState.Circle ? 255 : 0); break;
                    default: break;
                }
            }
            else if (control >= DS4Controls.L1 && control <= DS4Controls.R3)
            {
                switch (control)
                {
                    case DS4Controls.L1: result = (byte)(cState.L1 ? 255 : 0); break;
                    case DS4Controls.L2: result = cState.L2; break;
                    case DS4Controls.L3: result = (byte)(cState.L3 ? 255 : 0); break;
                    case DS4Controls.R1: result = (byte)(cState.R1 ? 255 : 0); break;
                    case DS4Controls.R2: result = cState.R2; break;
                    case DS4Controls.R3: result = (byte)(cState.R3 ? 255 : 0); break;
                    default: break;
                }
            }
            else if (control >= DS4Controls.DpadUp && control <= DS4Controls.DpadLeft)
            {
                switch (control)
                {
                    case DS4Controls.DpadUp: result = (byte)(cState.DpadUp ? 255 : 0); break;
                    case DS4Controls.DpadDown: result = (byte)(cState.DpadDown ? 255 : 0); break;
                    case DS4Controls.DpadLeft: result = (byte)(cState.DpadLeft ? 255 : 0); break;
                    case DS4Controls.DpadRight: result = (byte)(cState.DpadRight ? 255 : 0); break;
                    default: break;
                }
            }
            else if (control >= DS4Controls.LXNeg && control <= DS4Controls.RYPos)
            {
                switch (control)
                {
                    case DS4Controls.LXNeg: result = (byte)(cState.LX - 128.0f >= 0 ? 0 : -(cState.LX - 128.0f) * 1.9921875f); break;
                    case DS4Controls.LYNeg: result = (byte)(cState.LY - 128.0f >= 0 ? 0 : -(cState.LY - 128.0f) * 1.9921875f); break;
                    case DS4Controls.RXNeg: result = (byte)(cState.RX - 128.0f >= 0 ? 0 : -(cState.RX - 128.0f) * 1.9921875f); break;
                    case DS4Controls.RYNeg: result = (byte)(cState.RY - 128.0f >= 0 ? 0 : -(cState.RY - 128.0f) * 1.9921875f); break;
                    case DS4Controls.LXPos: result = (byte)(cState.LX - 128.0f < 0 ? 0 : (cState.LX - 128.0f) * 2.0078740157480315f); break;
                    case DS4Controls.LYPos: result = (byte)(cState.LY - 128.0f < 0 ? 0 : (cState.LY - 128.0f) * 2.0078740157480315f); break;
                    case DS4Controls.RXPos: result = (byte)(cState.RX - 128.0f < 0 ? 0 : (cState.RX - 128.0f) * 2.0078740157480315f); break;
                    case DS4Controls.RYPos: result = (byte)(cState.RY - 128.0f < 0 ? 0 : (cState.RY - 128.0f) * 2.0078740157480315f); break;
                    default: break;
                }
            }
            else if (control >= DS4Controls.TouchLeft && control <= DS4Controls.TouchRight)
            {
                switch (control)
                {
                    case DS4Controls.TouchLeft: result = (byte)(tp != null && tp.leftDown ? 255 : 0); break;
                    case DS4Controls.TouchRight: result = (byte)(tp != null && tp.rightDown ? 255 : 0); break;
                    case DS4Controls.TouchMulti: result = (byte)(tp != null && tp.multiDown ? 255 : 0); break;
                    case DS4Controls.TouchUpper: result = (byte)(tp != null && tp.upperDown ? 255 : 0); break;
                    default: break;
                }
            }
            else if (control >= DS4Controls.SwipeLeft && control <= DS4Controls.SwipeDown)
            {
                switch (control)
                {
                    case DS4Controls.SwipeUp: result = (byte)(tp != null ? tp.swipeUpB : 0); break;
                    case DS4Controls.SwipeDown: result = (byte)(tp != null ? tp.swipeDownB : 0); break;
                    case DS4Controls.SwipeLeft: result = (byte)(tp != null ? tp.swipeLeftB : 0); break;
                    case DS4Controls.SwipeRight: result = (byte)(tp != null ? tp.swipeRightB : 0); break;
                    default: break;
                }
            }
            else if (control >= DS4Controls.GyroXPos && control <= DS4Controls.GyroZNeg)
            {
                double SXD = getSXDeadzone(device);
                double SZD = getSZDeadzone(device);
                bool saControls = IsUsingSAForControls(device);
                double sxsens = getSXSens(device);
                double szsens = getSZSens(device);

                switch (control)
                {
                    case DS4Controls.GyroXPos:
                    {
                        int gyroX = -eState.AccelX;
                        result = (byte)(saControls && sxsens * gyroX > SXD * 10 ? Math.Min(255, sxsens * gyroX * 2) : 0);
                        break;
                    }
                    case DS4Controls.GyroXNeg:
                    {
                        int gyroX = -eState.AccelX;
                        result = (byte)(saControls && sxsens * gyroX < -SXD * 10 ? Math.Min(255, sxsens * -gyroX * 2) : 0);
                        break;
                    }
                    case DS4Controls.GyroZPos:
                    {
                        int gyroZ = eState.AccelZ;
                        result = (byte)(saControls && szsens * gyroZ > SZD * 10 ? Math.Min(255, szsens * gyroZ * 2) : 0);
                        break;
                    }
                    case DS4Controls.GyroZNeg:
                    {
                        int gyroZ = eState.AccelZ;
                        result = (byte)(saControls && szsens * gyroZ < -SZD * 10 ? Math.Min(255, szsens * -gyroZ * 2) : 0);
                        break;
                    }
                    default: break;
                }
            }
            else
            {
                switch (control)
                {
                    case DS4Controls.Share: result = (byte)(cState.Share ? 255 : 0); break;
                    case DS4Controls.Options: result = (byte)(cState.Options ? 255 : 0); break;
                    case DS4Controls.PS: result = (byte)(cState.PS ? 255 : 0); break;
                    default: break;
                }
            }

            return result;
        }

        /* TODO: Possibly remove usage of this version of the method */
        public static bool getBoolMapping(int device, DS4Controls control, DS4State cState, DS4StateExposed eState, Mouse tp)
        {
            bool result = false;

            if (control >= DS4Controls.Square && control <= DS4Controls.Cross)
            {
                switch (control)
                {
                    case DS4Controls.Cross: result = cState.Cross; break;
                    case DS4Controls.Square: result = cState.Square; break;
                    case DS4Controls.Triangle: result = cState.Triangle; break;
                    case DS4Controls.Circle: result = cState.Circle; break;
                    default: break;
                }
            }
            else if (control >= DS4Controls.L1 && control <= DS4Controls.R3)
            {
                switch (control)
                {
                    case DS4Controls.L1: result = cState.L1; break;
                    case DS4Controls.R1: result = cState.R1; break;
                    case DS4Controls.L2: result = cState.L2 > 100; break;
                    case DS4Controls.R2: result = cState.R2 > 100; break;
                    case DS4Controls.L3: result = cState.L3; break;
                    case DS4Controls.R3: result = cState.R3; break;
                    default: break;
                }
            }
            else if (control >= DS4Controls.DpadUp && control <= DS4Controls.DpadLeft)
            {
                switch (control)
                {
                    case DS4Controls.DpadUp: result = cState.DpadUp; break;
                    case DS4Controls.DpadDown: result = cState.DpadDown; break;
                    case DS4Controls.DpadLeft: result = cState.DpadLeft; break;
                    case DS4Controls.DpadRight: result = cState.DpadRight; break;
                    default: break;
                }
            }
            else if (control >= DS4Controls.LXNeg && control <= DS4Controls.RYPos)
            {
                switch (control)
                {
                    case DS4Controls.LXNeg: result = cState.LX < 128 - 55; break;
                    case DS4Controls.LYNeg: result = cState.LY < 128 - 55; break;
                    case DS4Controls.RXNeg: result = cState.RX < 128 - 55; break;
                    case DS4Controls.RYNeg: result = cState.RY < 128 - 55; break;
                    case DS4Controls.LXPos: result = cState.LX > 128 + 55; break;
                    case DS4Controls.LYPos: result = cState.LY > 128 + 55; break;
                    case DS4Controls.RXPos: result = cState.RX > 128 + 55; break;
                    case DS4Controls.RYPos: result = cState.RY > 128 + 55; break;
                    default: break;
                }
            }
            else if (control >= DS4Controls.TouchLeft && control <= DS4Controls.TouchRight)
            {
                switch (control)
                {
                    case DS4Controls.TouchLeft: result = (tp != null ? tp.leftDown : false); break;
                    case DS4Controls.TouchRight: result = (tp != null ? tp.rightDown : false); break;
                    case DS4Controls.TouchMulti: result = (tp != null ? tp.multiDown : false); break;
                    case DS4Controls.TouchUpper: result = (tp != null ? tp.upperDown : false); break;
                    default: break;
                }
            }
            else if (control >= DS4Controls.SwipeLeft && control <= DS4Controls.SwipeDown)
            {
                switch (control)
                {
                    case DS4Controls.SwipeUp: result = (tp != null && tp.swipeUp); break;
                    case DS4Controls.SwipeDown: result = (tp != null && tp.swipeDown); break;
                    case DS4Controls.SwipeLeft: result = (tp != null && tp.swipeLeft); break;
                    case DS4Controls.SwipeRight: result = (tp != null && tp.swipeRight); break;
                    default: break;
                }
            }
            else if (control >= DS4Controls.GyroXPos && control <= DS4Controls.GyroZNeg)
            {
                bool saControls = IsUsingSAForControls(device);

                switch (control)
                {
                    case DS4Controls.GyroXPos: result = saControls ? SXSens[device] * -eState.AccelX > 67 : false; break;
                    case DS4Controls.GyroXNeg: result = saControls ? SXSens[device] * -eState.AccelX < -67 : false; break;
                    case DS4Controls.GyroZPos: result = saControls ? SZSens[device] * eState.AccelZ > 67 : false; break;
                    case DS4Controls.GyroZNeg: result = saControls ? SZSens[device] * eState.AccelZ < -67 : false; break;
                    default: break;
                }
            }
            else
            {
                switch (control)
                {
                    case DS4Controls.PS: result = cState.PS; break;
                    case DS4Controls.Share: result = cState.Share; break;
                    case DS4Controls.Options: result = cState.Options; break;
                    default: break;
                }
            }

            return result;
        }

        private static bool getBoolMapping2(int device, DS4Controls control,
            DS4State cState, DS4StateExposed eState, Mouse tp, DS4StateFieldMapping fieldMap)
        {
            bool result = false;

            int controlNum = (int)control;
            DS4StateFieldMapping.ControlType controlType = DS4StateFieldMapping.mappedType[controlNum];
            if (controlType == DS4StateFieldMapping.ControlType.Button)
            {
                result = fieldMap.buttons[controlNum];
            }
            else if (controlType == DS4StateFieldMapping.ControlType.AxisDir)
            {
                byte axisValue = fieldMap.axisdirs[controlNum];

                switch (control)
                {
                    case DS4Controls.LXNeg: result = cState.LX < 128 - 55; break;
                    case DS4Controls.LYNeg: result = cState.LY < 128 - 55; break;
                    case DS4Controls.RXNeg: result = cState.RX < 128 - 55; break;
                    case DS4Controls.RYNeg: result = cState.RY < 128 - 55; break;
                    default: result = axisValue > 128 + 55; break;
                }
            }
            else if (controlType == DS4StateFieldMapping.ControlType.Trigger)
            {
                result = fieldMap.triggers[controlNum] > 100;
            }
            else if (controlType == DS4StateFieldMapping.ControlType.Touch)
            {
                result = fieldMap.buttons[controlNum];
            }
            else if (controlType == DS4StateFieldMapping.ControlType.SwipeDir)
            {
                result = fieldMap.swipedirbools[controlNum];
            }
            else if (controlType == DS4StateFieldMapping.ControlType.GyroDir)
            {
                bool saControls = IsUsingSAForControls(device);
                bool safeTest = false;

                switch (control)
                {
                    case DS4Controls.GyroXPos: safeTest = fieldMap.gryodirs[controlNum] > 0; break;
                    case DS4Controls.GyroXNeg: safeTest = fieldMap.gryodirs[controlNum] < -0; break;
                    case DS4Controls.GyroZPos: safeTest = fieldMap.gryodirs[controlNum] > 0; break;
                    case DS4Controls.GyroZNeg: safeTest = fieldMap.gryodirs[controlNum] < -0; break;
                    default: break;
                }

                result = saControls ? safeTest : false;
            }

            return result;
        }

        private static bool getBoolSpecialActionMapping(int device, DS4Controls control,
            DS4State cState, DS4StateExposed eState, Mouse tp, DS4StateFieldMapping fieldMap)
        {
            bool result = false;

            int controlNum = (int)control;
            DS4StateFieldMapping.ControlType controlType = DS4StateFieldMapping.mappedType[controlNum];
            if (controlType == DS4StateFieldMapping.ControlType.Button)
            {
                result = fieldMap.buttons[controlNum];
            }
            else if (controlType == DS4StateFieldMapping.ControlType.AxisDir)
            {
                byte axisValue = fieldMap.axisdirs[controlNum];

                switch (control)
                {
                    case DS4Controls.LXNeg: result = cState.LX < 128 - 55; break;
                    case DS4Controls.LYNeg: result = cState.LY < 128 - 55; break;
                    case DS4Controls.RXNeg: result = cState.RX < 128 - 55; break;
                    case DS4Controls.RYNeg: result = cState.RY < 128 - 55; break;
                    default: result = axisValue > 128 + 55; break;
                }
            }
            else if (controlType == DS4StateFieldMapping.ControlType.Trigger)
            {
                result = fieldMap.triggers[controlNum] > 100;
            }
            else if (controlType == DS4StateFieldMapping.ControlType.Touch)
            {
                result = fieldMap.buttons[controlNum];
            }
            else if (controlType == DS4StateFieldMapping.ControlType.SwipeDir)
            {
                result = fieldMap.swipedirbools[controlNum];
            }
            else if (controlType == DS4StateFieldMapping.ControlType.GyroDir)
            {
                bool saControls = IsUsingSAForControls(device);
                bool safeTest = false;

                switch (control)
                {
                    case DS4Controls.GyroXPos: safeTest = fieldMap.gryodirs[controlNum] > 67; break;
                    case DS4Controls.GyroXNeg: safeTest = fieldMap.gryodirs[controlNum] < -67; break;
                    case DS4Controls.GyroZPos: safeTest = fieldMap.gryodirs[controlNum] > 67; break;
                    case DS4Controls.GyroZNeg: safeTest = fieldMap.gryodirs[controlNum] < -67; break;
                    default: break;
                }

                result = saControls ? safeTest : false;
            }

            return result;
        }

        private static bool getBoolActionMapping2(int device, DS4Controls control,
            DS4State cState, DS4StateExposed eState, Mouse tp, DS4StateFieldMapping fieldMap, bool analog = false)
        {
            bool result = false;

            int controlNum = (int)control;
            DS4StateFieldMapping.ControlType controlType = DS4StateFieldMapping.mappedType[controlNum];
            if (controlType == DS4StateFieldMapping.ControlType.Button)
            {
                result = fieldMap.buttons[controlNum];
            }
            else if (controlType == DS4StateFieldMapping.ControlType.AxisDir)
            {
                switch (control)
                {
                    case DS4Controls.LXNeg:
                    {
                        double angle = cState.LSAngle;
                        result = cState.LX < 128 && (angle >= 112.5 && angle <= 247.5);
                        break;
                    }
                    case DS4Controls.LYNeg:
                    {
                        double angle = cState.LSAngle;
                        result = cState.LY < 128 && (angle >= 22.5 && angle <= 157.5);
                        break;
                    }
                    case DS4Controls.RXNeg:
                    {
                        double angle = cState.RSAngle;
                        result = cState.RX < 128 && (angle >= 112.5 && angle <= 247.5);
                        break;
                    }
                    case DS4Controls.RYNeg:
                    {
                        double angle = cState.RSAngle;
                        result = cState.RY < 128 && (angle >= 22.5 && angle <= 157.5);
                        break;
                    }
                    case DS4Controls.LXPos:
                    {
                        double angle = cState.LSAngle;
                        result = cState.LX > 128 && (angle <= 67.5 || angle >= 292.5);
                        break;
                    }
                    case DS4Controls.LYPos:
                    {
                        double angle = cState.LSAngle;
                        result = cState.LY > 128 && (angle >= 202.5 && angle <= 337.5);
                        break;
                    }
                    case DS4Controls.RXPos:
                    {
                        double angle = cState.RSAngle;
                        result = cState.RX > 128 && (angle <= 67.5 || angle >= 292.5);
                        break;
                    }
                    case DS4Controls.RYPos:
                    {
                        double angle = cState.RSAngle;
                        result = cState.RY > 128 && (angle >= 202.5 && angle <= 337.5);
                        break;
                    }
                    default: break;
                }
            }
            else if (controlType == DS4StateFieldMapping.ControlType.Trigger)
            {
                result = fieldMap.triggers[controlNum] > 0;
            }
            else if (controlType == DS4StateFieldMapping.ControlType.Touch)
            {
                result = fieldMap.buttons[controlNum];
            }
            else if (controlType == DS4StateFieldMapping.ControlType.SwipeDir)
            {
                result = fieldMap.swipedirbools[controlNum];
            }
            else if (controlType == DS4StateFieldMapping.ControlType.GyroDir)
            {
                bool saControls = IsUsingSAForControls(device);
                bool safeTest = false;

                switch (control)
                {
                    case DS4Controls.GyroXPos: safeTest = fieldMap.gryodirs[controlNum] > 0; break;
                    case DS4Controls.GyroXNeg: safeTest = fieldMap.gryodirs[controlNum] < 0; break;
                    case DS4Controls.GyroZPos: safeTest = fieldMap.gryodirs[controlNum] > 0; break;
                    case DS4Controls.GyroZNeg: safeTest = fieldMap.gryodirs[controlNum] < 0; break;
                    default: break;
                }

                result = saControls ? safeTest : false;
            }

            return result;
        }

        public static bool getBoolButtonMapping(bool stateButton)
        {
            return stateButton;
        }

        public static bool getBoolAxisDirMapping(byte stateAxis, bool positive)
        {
            return positive ? stateAxis > 128 + 55 : stateAxis < 128 - 55;
        }

        public static bool getBoolTriggerMapping(byte stateAxis)
        {
            return stateAxis > 100;
        }

        public static bool getBoolTouchMapping(bool touchButton)
        {
            return touchButton;
        }

        private static byte getXYAxisMapping2(int device, DS4Controls control, DS4State cState,
            DS4StateExposed eState, Mouse tp, DS4StateFieldMapping fieldMap, bool alt = false)
        {
            const byte falseVal = 128;
            byte result = 0;
            byte trueVal = 0;

            if (alt)
                trueVal = 255;

            int controlNum = (int)control;
            DS4StateFieldMapping.ControlType controlType = DS4StateFieldMapping.mappedType[controlNum];

            if (controlType == DS4StateFieldMapping.ControlType.Button)
            {
                result = fieldMap.buttons[controlNum] ? trueVal : falseVal;
            }
            else if (controlType == DS4StateFieldMapping.ControlType.AxisDir)
            {
                byte axisValue = fieldMap.axisdirs[controlNum];

                switch (control)
                {
                    case DS4Controls.LXNeg: if (!alt) result = axisValue < falseVal ? axisValue : falseVal; else result = axisValue < falseVal ? (byte)(255 - axisValue) : falseVal; break;
                    case DS4Controls.LYNeg: if (!alt) result = axisValue < falseVal ? axisValue : falseVal; else result = axisValue < falseVal ? (byte)(255 - axisValue) : falseVal; break;
                    case DS4Controls.RXNeg: if (!alt) result = axisValue < falseVal ? axisValue : falseVal; else result = axisValue < falseVal ? (byte)(255 - axisValue) : falseVal; break;
                    case DS4Controls.RYNeg: if (!alt) result = axisValue < falseVal ? axisValue : falseVal; else result = axisValue < falseVal ? (byte)(255 - axisValue) : falseVal; break;
                    default: if (!alt) result = axisValue > falseVal ? (byte)(255 - axisValue) : falseVal; else result = axisValue > falseVal ? axisValue : falseVal; break;
                }
            }
            else if (controlType == DS4StateFieldMapping.ControlType.Trigger)
            {
                if (alt)
                {
                    result = (byte)(128.0f + fieldMap.triggers[controlNum] / 2.0078740157480315f);
                }
                else
                {
                    result = (byte)(128.0f - fieldMap.triggers[controlNum] / 2.0078740157480315f);
                }
            }
            else if (controlType == DS4StateFieldMapping.ControlType.Touch)
            {
                result = fieldMap.buttons[controlNum] ? trueVal : falseVal;
            }
            else if (controlType == DS4StateFieldMapping.ControlType.SwipeDir)
            {
                if (alt)
                {
                    result = (byte)(tp != null ? 128.0f + fieldMap.swipedirs[controlNum] / 2f : 0);
                }
                else
                {
                    result = (byte)(tp != null ? 128.0f - fieldMap.swipedirs[controlNum] / 2f : 0);
                }
            }
            else if (controlType == DS4StateFieldMapping.ControlType.GyroDir)
            {
                bool saControls = IsUsingSAForControls(device);

                switch (control)
                {
                    case DS4Controls.GyroXPos:
                    {
                        if (saControls && fieldMap.gryodirs[controlNum] > 0)
                        {
                            if (alt) result = (byte)Math.Min(255, 128 + fieldMap.gryodirs[controlNum]); else result = (byte)Math.Max(0, 128 - fieldMap.gryodirs[controlNum]);
                        }
                        else result = falseVal;
                        break;
                    }
                    case DS4Controls.GyroXNeg:
                    {
                        if (saControls && fieldMap.gryodirs[controlNum] < 0)
                        {
                            if (alt) result = (byte)Math.Min(255, 128 + -fieldMap.gryodirs[controlNum]); else result = (byte)Math.Max(0, 128 - -fieldMap.gryodirs[controlNum]);
                        }
                        else result = falseVal;
                        break;
                    }
                    case DS4Controls.GyroZPos:
                    {
                        if (saControls && fieldMap.gryodirs[controlNum] > 0)
                        {
                            if (alt) result = (byte)Math.Min(255, 128 + fieldMap.gryodirs[controlNum]); else result = (byte)Math.Max(0, 128 - fieldMap.gryodirs[controlNum]);
                        }
                        else return falseVal;
                        break;
                    }
                    case DS4Controls.GyroZNeg:
                    {
                        if (saControls && fieldMap.gryodirs[controlNum] < 0)
                        {
                            if (alt) result = (byte)Math.Min(255, 128 + -fieldMap.gryodirs[controlNum]); else result = (byte)Math.Max(0, 128 - -fieldMap.gryodirs[controlNum]);
                        }
                        else result = falseVal;
                        break;
                    }
                    default: break;
                }
            }

            return result;
        }

        /* TODO: Possibly remove usage of this version of the method */
        public static byte getXYAxisMapping(int device, DS4Controls control, DS4State cState, DS4StateExposed eState, Mouse tp, bool alt = false)
        {
            byte result = 0;
            byte trueVal = 0;
            byte falseVal = 127;

            if (alt)
                trueVal = 255;

            if (control >= DS4Controls.Square && control <= DS4Controls.Cross)
            {
                switch (control)
                {
                    case DS4Controls.Cross: result = (byte)(cState.Cross ? trueVal : falseVal); break;
                    case DS4Controls.Square: result = (byte)(cState.Square ? trueVal : falseVal); break;
                    case DS4Controls.Triangle: result = (byte)(cState.Triangle ? trueVal : falseVal); break;
                    case DS4Controls.Circle: result = (byte)(cState.Circle ? trueVal : falseVal); break;
                    default: break;
                }
            }
            else if (control >= DS4Controls.L1 && control <= DS4Controls.R3)
            {
                switch (control)
                {
                    case DS4Controls.L1: result = (byte)(cState.L1 ? trueVal : falseVal); break;
                    case DS4Controls.L2: if (alt) result = (byte)(128.0f + cState.L2 / 2.0078740157480315f); else result = (byte)(128.0f - cState.L2 / 2.0078740157480315f); break;
                    case DS4Controls.L3: result = (byte)(cState.L3 ? trueVal : falseVal); break;
                    case DS4Controls.R1: result = (byte)(cState.R1 ? trueVal : falseVal); break;
                    case DS4Controls.R2: if (alt) result = (byte)(128.0f + cState.R2 / 2.0078740157480315f); else result = (byte)(128.0f - cState.R2 / 2.0078740157480315f); break;
                    case DS4Controls.R3: result = (byte)(cState.R3 ? trueVal : falseVal); break;
                    default: break;
                }
            }
            else if (control >= DS4Controls.DpadUp && control <= DS4Controls.DpadLeft)
            {
                switch (control)
                {
                    case DS4Controls.DpadUp: result = (byte)(cState.DpadUp ? trueVal : falseVal); break;
                    case DS4Controls.DpadDown: result = (byte)(cState.DpadDown ? trueVal : falseVal); break;
                    case DS4Controls.DpadLeft: result = (byte)(cState.DpadLeft ? trueVal : falseVal); break;
                    case DS4Controls.DpadRight: result = (byte)(cState.DpadRight ? trueVal : falseVal); break;
                    default: break;
                }
            }
            else if (control >= DS4Controls.LXNeg && control <= DS4Controls.RYPos)
            {
                switch (control)
                {
                    case DS4Controls.LXNeg: if (!alt) result = cState.LX; else result = (byte)(255 - cState.LX); break;
                    case DS4Controls.LYNeg: if (!alt) result = cState.LY; else result = (byte)(255 - cState.LY); break;
                    case DS4Controls.RXNeg: if (!alt) result = cState.RX; else result = (byte)(255 - cState.RX); break;
                    case DS4Controls.RYNeg: if (!alt) result = cState.RY; else result = (byte)(255 - cState.RY); break;
                    case DS4Controls.LXPos: if (!alt) result = (byte)(255 - cState.LX); else result = cState.LX; break;
                    case DS4Controls.LYPos: if (!alt) result = (byte)(255 - cState.LY); else result = cState.LY; break;
                    case DS4Controls.RXPos: if (!alt) result = (byte)(255 - cState.RX); else result = cState.RX; break;
                    case DS4Controls.RYPos: if (!alt) result = (byte)(255 - cState.RY); else result = cState.RY; break;
                    default: break;
                }
            }
            else if (control >= DS4Controls.TouchLeft && control <= DS4Controls.TouchRight)
            {
                switch (control)
                {
                    case DS4Controls.TouchLeft: result = (byte)(tp != null && tp.leftDown ? trueVal : falseVal); break;
                    case DS4Controls.TouchRight: result = (byte)(tp != null && tp.rightDown ? trueVal : falseVal); break;
                    case DS4Controls.TouchMulti: result = (byte)(tp != null && tp.multiDown ? trueVal : falseVal); break;
                    case DS4Controls.TouchUpper: result = (byte)(tp != null && tp.upperDown ? trueVal : falseVal); break;
                    default: break;
                }
            }
            else if (control >= DS4Controls.SwipeLeft && control <= DS4Controls.SwipeDown)
            {
                switch (control)
                {
                    case DS4Controls.SwipeUp: if (alt) result = (byte)(tp != null ? 128.0f + tp.swipeUpB / 2f : 0); else result = (byte)(tp != null ? 128.0f - tp.swipeUpB / 2f : 0); break;
                    case DS4Controls.SwipeDown: if (alt) result = (byte)(tp != null ? 128.0f + tp.swipeDownB / 2f : 0); else result = (byte)(tp != null ? 128.0f - tp.swipeDownB / 2f : 0); break;
                    case DS4Controls.SwipeLeft: if (alt) result = (byte)(tp != null ? 128.0f + tp.swipeLeftB / 2f : 0); else result = (byte)(tp != null ? 128.0f - tp.swipeLeftB / 2f : 0); break;
                    case DS4Controls.SwipeRight: if (alt) result = (byte)(tp != null ? 128.0f + tp.swipeRightB / 2f : 0); else result = (byte)(tp != null ? 128.0f - tp.swipeRightB / 2f : 0); break;
                    default: break;
                }
            }
            else if (control >= DS4Controls.GyroXPos && control <= DS4Controls.GyroZNeg)
            {
                double SXD = getSXDeadzone(device);
                double SZD = getSZDeadzone(device);
                bool saControls = IsUsingSAForControls(device);

                switch (control)
                {
                    case DS4Controls.GyroXPos:
                    {
                        if (saControls && -eState.AccelX > SXD * 10)
                        {
                            if (alt) result = (byte)Math.Min(255, 127 + SXSens[device] * -eState.AccelX); else result = (byte)Math.Max(0, 127 - SXSens[device] * -eState.AccelX);
                        }
                        else result = falseVal;
                        break;
                    }
                    case DS4Controls.GyroXNeg:
                    {
                        if (saControls && -eState.AccelX < -SXD * 10)
                        {
                            if (alt) result = (byte)Math.Min(255, 127 + SXSens[device] * eState.AccelX); else result = (byte)Math.Max(0, 127 - SXSens[device] * eState.AccelX);
                        }
                        else result = falseVal;
                        break;
                    }
                    case DS4Controls.GyroZPos:
                    {
                        if (saControls && eState.AccelZ > SZD * 10)
                        {
                            if (alt) result = (byte)Math.Min(255, 127 + SZSens[device] * eState.AccelZ); else result = (byte)Math.Max(0, 127 - SZSens[device] * eState.AccelZ);
                        }
                        else return falseVal;
                        break;
                    }
                    case DS4Controls.GyroZNeg:
                    {
                        if (saControls && eState.AccelZ < -SZD * 10)
                        {
                            if (alt) result = (byte)Math.Min(255, 127 + SZSens[device] * -eState.AccelZ); else result = (byte)Math.Max(0, 127 - SZSens[device] * -eState.AccelZ);
                        }
                        else result = falseVal;
                        break;
                    }
                    default: break;
                }
            }
            else
            {
                switch (control)
                {
                    case DS4Controls.Share: result = (byte)(cState.Share ? trueVal : falseVal); break;
                    case DS4Controls.Options: result = (byte)(cState.Options ? trueVal : falseVal); break;
                    case DS4Controls.PS: result = (byte)(cState.PS ? trueVal : falseVal); break;
                    default: break;
                }
            }

            return result;
        }

        private static void resetToDefaultValue2(DS4Controls control, DS4State cState,
            DS4StateFieldMapping fieldMap)
        {
            int controlNum = (int)control;
            DS4StateFieldMapping.ControlType controlType = DS4StateFieldMapping.mappedType[controlNum];
            if (controlType == DS4StateFieldMapping.ControlType.Button)
            {
                fieldMap.buttons[controlNum] = false;
            }
            else if (controlType == DS4StateFieldMapping.ControlType.AxisDir)
            {
                fieldMap.axisdirs[controlNum] = 128;
                int controlRelation = (controlNum % 2 == 0 ? controlNum - 1 : controlNum + 1);
                fieldMap.axisdirs[controlRelation] = 128;
            }
            else if (controlType == DS4StateFieldMapping.ControlType.Trigger)
            {
                fieldMap.triggers[controlNum] = 0;
            }
            else if (controlType == DS4StateFieldMapping.ControlType.Touch)
            {
                fieldMap.buttons[controlNum] = false;
            }
        }


        // SA steering wheel emulation mapping

        private const int C_WHEEL_ANGLE_PRECISION = 10; // Precision of SA angle in 1/10 of degrees
        
        private static readonly DS4Color calibrationColor_0 = new DS4Color { red = 0xA0, green = 0x00, blue = 0x00 };
        private static readonly DS4Color calibrationColor_1 = new DS4Color { red = 0xFF, green = 0xFF, blue = 0x00 };
        private static readonly DS4Color calibrationColor_2 = new DS4Color { red = 0x00, green = 0x50, blue = 0x50 };
        private static readonly DS4Color calibrationColor_3 = new DS4Color { red = 0x00, green = 0xC0, blue = 0x00 };

        private static DateTime latestDebugMsgTime;
        private static string latestDebugData;
        private static void LogToGuiSACalibrationDebugMsg(string data, bool forceOutput = false)
        {
            // Print debug calibration log messages only once per 2 secs to avoid flooding the log receiver
            DateTime curTime = DateTime.Now;
            if (forceOutput || ((TimeSpan)(curTime - latestDebugMsgTime)).TotalSeconds > 2)
            {
                latestDebugMsgTime = curTime;
                if (data != latestDebugData)
                {
                    AppLogger.LogToGui(data, false);
                    latestDebugData = data;
                }
            }
        }

        // Return number of bits set in a value
        protected static int CountNumOfSetBits(int bitValue)
        {
            int count = 0;
            while (bitValue != 0)
            {
                count++;
                bitValue &= (bitValue - 1);
            }
            return count;
        }

        // Calculate and return the angle of the controller as -180...0...+180 value.
        private static Int32 CalculateControllerAngle(int gyroAccelX, int gyroAccelZ, DS4Device controller)
        {
            Int32 result;

            if (gyroAccelX == controller.wheelCenterPoint.X && Math.Abs(gyroAccelZ - controller.wheelCenterPoint.Y) <= 1)
            {
                // When the current gyro position is "close enough" the wheel center point then no need to go through the hassle of calculating an angle
                result = 0;
            }
            else
            {
                // Calculate two vectors based on "circle center" (ie. circle represents the 360 degree wheel turn and wheelCenterPoint and currentPosition vectors both start from circle center).
                // To improve accuracy both left and right turns use a decicated calibration "circle" because DS4 gyro and DoItYourselfWheelRig may return slightly different SA sensor values depending on the tilt direction (well, only one or two degree difference so nothing major).
                Point vectorAB;
                Point vectorCD;

                if (gyroAccelX >= controller.wheelCenterPoint.X)
                {
                    // "DS4 gyro wheel" tilted to right
                    vectorAB = new Point(controller.wheelCenterPoint.X - controller.wheelCircleCenterPointRight.X, controller.wheelCenterPoint.Y - controller.wheelCircleCenterPointRight.Y);
                    vectorCD = new Point(gyroAccelX - controller.wheelCircleCenterPointRight.X, gyroAccelZ - controller.wheelCircleCenterPointRight.Y);
                }
                else
                {
                    // "DS4 gyro wheel" tilted to left
                    vectorAB = new Point(controller.wheelCenterPoint.X - controller.wheelCircleCenterPointLeft.X, controller.wheelCenterPoint.Y - controller.wheelCircleCenterPointLeft.Y);
                    vectorCD = new Point(gyroAccelX - controller.wheelCircleCenterPointLeft.X, gyroAccelZ - controller.wheelCircleCenterPointLeft.Y);
                }

                // Calculate dot product and magnitude of vectors (center vector and the current tilt vector)
                double dotProduct = vectorAB.X * vectorCD.X + vectorAB.Y * vectorCD.Y;
                double magAB = Math.Sqrt(vectorAB.X * vectorAB.X + vectorAB.Y * vectorAB.Y);
                double magCD = Math.Sqrt(vectorCD.X * vectorCD.X + vectorCD.Y * vectorCD.Y);

                // Calculate angle between vectors and convert radian to degrees
                if (magAB == 0 || magCD == 0)
                {
                    result = 0;
                }
                else
                {
                    double angle = Math.Acos(dotProduct / (magAB * magCD));
                    result = Convert.ToInt32(Global.Clamp(
                            -180.0 * C_WHEEL_ANGLE_PRECISION,
                            Math.Round((angle * (180.0 / Math.PI)), 1) * C_WHEEL_ANGLE_PRECISION,
                            180.0 * C_WHEEL_ANGLE_PRECISION)
                         );
                }

                // Left turn is -180..0 and right turn 0..180 degrees
                if (gyroAccelX < controller.wheelCenterPoint.X) result = -result;
            }

            return result;
        }

        // Calibrate sixaxis steering wheel emulation. Use DS4Windows configuration screen to start a calibration or press a special action key (if defined)
        private static void SAWheelEmulationCalibration(int device, DS4StateExposed exposedState, ControlService ctrl, DS4State currentDeviceState, DS4Device controller)
        {
            int gyroAccelX, gyroAccelZ;
            int result;

            gyroAccelX = exposedState.getAccelX();
            gyroAccelZ = exposedState.getAccelZ();

            // State 0=Normal mode (ie. calibration process is not running), 1=Activating calibration, 2=Calibration process running, 3=Completing calibration, 4=Cancelling calibration
            if (controller.WheelRecalibrateActiveState == 1)
            {
                AppLogger.LogToGui($"Controller {1 + device} activated re-calibration of SA steering wheel emulation", false);

                controller.WheelRecalibrateActiveState = 2;

                controller.wheelPrevPhysicalAngle = 0;
                controller.wheelPrevFullAngle = 0;
                controller.wheelFullTurnCount = 0;

                // Clear existing calibration value and use current position as "center" point.
                // This initial center value may be off-center because of shaking the controller while button was pressed. The value will be overriden with correct value once controller is stabilized and hold still few secs at the center point
                controller.wheelCenterPoint.X = gyroAccelX;
                controller.wheelCenterPoint.Y = gyroAccelZ;
                controller.wheel90DegPointRight.X = gyroAccelX + 20;
                controller.wheel90DegPointLeft.X = gyroAccelX - 20;

                // Clear bitmask for calibration points. All three calibration points need to be set before re-calibration process is valid
                controller.wheelCalibratedAxisBitmask = DS4Device.WheelCalibrationPoint.None;

                controller.wheelPrevRecalibrateTime = new DateTime(2500, 1, 1);
            }
            else if (controller.WheelRecalibrateActiveState == 3)
            {
                AppLogger.LogToGui($"Controller {1 + device} completed the calibration of SA steering wheel emulation. center=({controller.wheelCenterPoint.X}, {controller.wheelCenterPoint.Y})  90L=({controller.wheel90DegPointLeft.X}, {controller.wheel90DegPointLeft.Y})  90R=({controller.wheel90DegPointRight.X}, {controller.wheel90DegPointRight.Y})", false);

                // If any of the calibration points (center, left 90deg, right 90deg) are missing then reset back to default calibration values
                if (((controller.wheelCalibratedAxisBitmask & DS4Device.WheelCalibrationPoint.All) == DS4Device.WheelCalibrationPoint.All))
                    Global.SaveControllerConfigs(controller);
                else
                    controller.wheelCenterPoint.X = controller.wheelCenterPoint.Y = 0;

                controller.WheelRecalibrateActiveState = 0;
                controller.wheelPrevRecalibrateTime = DateTime.Now;
            }
            else if (controller.WheelRecalibrateActiveState == 4)
            {
                AppLogger.LogToGui($"Controller {1 + device} cancelled the calibration of SA steering wheel emulation.", false);

                controller.WheelRecalibrateActiveState = 0;
                controller.wheelPrevRecalibrateTime = DateTime.Now;
            }

            if (controller.WheelRecalibrateActiveState > 0)
            {
                // Cross "X" key pressed. Set calibration point when the key is released and controller hold steady for a few seconds
                if (currentDeviceState.Cross == true) controller.wheelPrevRecalibrateTime = DateTime.Now;

                // Make sure controller is hold steady (velocity of gyro axis) to avoid misaligments and set calibration few secs after the "X" key was released
                if (Math.Abs(currentDeviceState.Motion.angVelPitch) < 0.5 && Math.Abs(currentDeviceState.Motion.angVelYaw) < 0.5 && Math.Abs(currentDeviceState.Motion.angVelRoll) < 0.5
                    && ((TimeSpan)(DateTime.Now - controller.wheelPrevRecalibrateTime)).TotalSeconds > 1)
                {
                    controller.wheelPrevRecalibrateTime = new DateTime(2500, 1, 1);

                    if (controller.wheelCalibratedAxisBitmask == DS4Device.WheelCalibrationPoint.None)
                    {
                        controller.wheelCenterPoint.X = gyroAccelX;
                        controller.wheelCenterPoint.Y = gyroAccelZ;

                        controller.wheelCalibratedAxisBitmask |= DS4Device.WheelCalibrationPoint.Center;
                    }
                    else if (controller.wheel90DegPointRight.X < gyroAccelX)
                    {
                        controller.wheel90DegPointRight.X = gyroAccelX;
                        controller.wheel90DegPointRight.Y = gyroAccelZ;
                        controller.wheelCircleCenterPointRight.X = controller.wheelCenterPoint.X;
                        controller.wheelCircleCenterPointRight.Y = controller.wheel90DegPointRight.Y;

                        controller.wheelCalibratedAxisBitmask |= DS4Device.WheelCalibrationPoint.Right90;
                    }
                    else if (controller.wheel90DegPointLeft.X > gyroAccelX)
                    {
                        controller.wheel90DegPointLeft.X = gyroAccelX;
                        controller.wheel90DegPointLeft.Y = gyroAccelZ;
                        controller.wheelCircleCenterPointLeft.X = controller.wheelCenterPoint.X;
                        controller.wheelCircleCenterPointLeft.Y = controller.wheel90DegPointLeft.Y;

                        controller.wheelCalibratedAxisBitmask |= DS4Device.WheelCalibrationPoint.Left90;
                    }
                }

                // Show lightbar color feedback how the calibration process is proceeding.
                //  red / yellow / blue / green = No calibration anchors/one anchor/two anchors/all three anchors calibrated when color turns to green (center, 90DegLeft, 90DegRight).
                int bitsSet = CountNumOfSetBits((int)controller.wheelCalibratedAxisBitmask);
                if (bitsSet >= 3) DS4LightBar.forcedColor[device] = calibrationColor_3;
                else if (bitsSet == 2) DS4LightBar.forcedColor[device] = calibrationColor_2;
                else if (bitsSet == 1) DS4LightBar.forcedColor[device] = calibrationColor_1;
                else DS4LightBar.forcedColor[device] = calibrationColor_0;

                result = CalculateControllerAngle(gyroAccelX, gyroAccelZ, controller);

                // Force lightbar flashing when controller is currently at calibration point (user can verify the calibration before accepting it by looking at flashing lightbar)
                if (((controller.wheelCalibratedAxisBitmask & DS4Device.WheelCalibrationPoint.Center) != 0 && Math.Abs(result) <= 1 * C_WHEEL_ANGLE_PRECISION)
                 || ((controller.wheelCalibratedAxisBitmask & DS4Device.WheelCalibrationPoint.Left90) != 0 && result <= -89 * C_WHEEL_ANGLE_PRECISION && result >= -91 * C_WHEEL_ANGLE_PRECISION)
                 || ((controller.wheelCalibratedAxisBitmask & DS4Device.WheelCalibrationPoint.Right90) != 0 && result >= 89 * C_WHEEL_ANGLE_PRECISION && result <= 91 * C_WHEEL_ANGLE_PRECISION)
                 || ((controller.wheelCalibratedAxisBitmask & DS4Device.WheelCalibrationPoint.Left90) != 0 && Math.Abs(result) >= 179 * C_WHEEL_ANGLE_PRECISION))
                    DS4LightBar.forcedFlash[device] = 2;
                else
                    DS4LightBar.forcedFlash[device] = 0;

                DS4LightBar.forcelight[device] = true;

                LogToGuiSACalibrationDebugMsg($"Calibration values ({gyroAccelX}, {gyroAccelZ})  angle={result / (1.0 * C_WHEEL_ANGLE_PRECISION)}\n");
            }
            else
            {
                // Re-calibration completed or cancelled. Set lightbar color back to normal color
                DS4LightBar.forcedFlash[device] = 0;
                DS4LightBar.forcedColor[device] = Global.getMainColor(device);
                DS4LightBar.forcelight[device] = false;
                DS4LightBar.updateLightBar(controller, device);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void CalcStickAxisFuzz(int device,
            int stickId, int delta, byte axisXValue, byte axisYValue,
            out byte useAxisX, out byte useAxisY)
        {
            if (stickId < 0 || stickId > 2)
            {
                throw new ArgumentOutOfRangeException("Stick ID has to be either 0 or 1");
            }

            int xIdX = stickId == 0 ? 0 : 2;
            int yIdX = stickId == 1 ? 1 : 3;
            ref byte lastXVal = ref lastStickAxisValues[device][xIdX];
            ref byte lastYVal = ref lastStickAxisValues[device][yIdX];
            useAxisX = lastXVal;
            useAxisY = lastYVal;

            int deltaX = axisXValue - lastXVal;
            int deltaY = axisYValue - lastYVal;
            int magSqu = (deltaX * deltaX) + (deltaY * deltaY);
            int deltaSqu = delta * delta;
            //if (stickId == 0)
            //    Console.WriteLine("DELTA MAG SQU: {0} {1}", magSqu, deltaSqu);

            if (axisXValue == 0 || axisXValue == 255 || magSqu > deltaSqu)
            {
                useAxisX = axisXValue;
                lastXVal = axisXValue;
            }

            if (axisYValue == 0 || axisYValue == 255 || magSqu > deltaSqu)
            {
                useAxisY = axisYValue;
                lastYVal = axisYValue;
            }
        }

        private static void CalcWheelFuzz(int gyroX, int gyroZ, int lastGyroX, int lastGyroZ,
            int delta, out int useGyroX, out int useGyroZ)
        {
            useGyroX = lastGyroX;
            if (gyroX == 0 || gyroX == 128 || gyroX == -128 || Math.Abs(gyroX - lastGyroX) > delta)
            {
                useGyroX = gyroX;
            }

            useGyroZ = lastGyroZ;
            if (gyroZ == 0 || gyroZ == 128 || gyroZ == -128 || Math.Abs(gyroZ - lastGyroZ) > delta)
            {
                useGyroZ = gyroZ;
            }
        }

        protected static Int32 Scale360degreeGyroAxis(int device, DS4StateExposed exposedState, ControlService ctrl)
        {
            unchecked
            {
                DS4Device controller;
                DS4State currentDeviceState;

                int gyroAccelX, gyroAccelZ;
                int result;

                controller = ctrl.DS4Controllers[device];
                if (controller == null) return 0;

                currentDeviceState = controller.getCurrentStateRef();

                // If calibration is active then do the calibration process instead of the normal "angle calculation"
                if (controller.WheelRecalibrateActiveState > 0)
                {
                    SAWheelEmulationCalibration(device, exposedState, ctrl, currentDeviceState, controller);

                    // Return center wheel position while SA wheel emuation is being calibrated
                    return 0;
                }

                // Do nothing if connection is active but the actual DS4 controller is still missing or not yet synchronized
                if (!controller.Synced)
                    return 0;

                gyroAccelX = exposedState.getAccelX();
                gyroAccelZ = exposedState.getAccelZ();

                // If calibration values are missing then use "educated guesses" about good starting values
                if (controller.wheelCenterPoint.IsEmpty)
                {
                    if (!Global.LoadControllerConfigs(controller))
                    {
                        AppLogger.LogToGui($"Controller {1 + device} sixaxis steering wheel calibration data missing. It is recommended to run steering wheel calibration process by pressing SASteeringWheelEmulationCalibration special action key. Using estimated values until the controller is calibrated at least once.", false);

                        // Use current controller position as "center point". Assume DS4Windows was started while controller was hold in center position (yes, dangerous assumption but can't do much until controller is calibrated)
                        controller.wheelCenterPoint.X = gyroAccelX;
                        controller.wheelCenterPoint.Y = gyroAccelZ;

                        controller.wheel90DegPointRight.X = controller.wheelCenterPoint.X + 113;
                        controller.wheel90DegPointRight.Y = controller.wheelCenterPoint.Y + 110;

                        controller.wheel90DegPointLeft.X = controller.wheelCenterPoint.X - 127;
                        controller.wheel90DegPointLeft.Y = controller.wheel90DegPointRight.Y;
                    }

                    controller.wheelCircleCenterPointRight.X = controller.wheelCenterPoint.X;
                    controller.wheelCircleCenterPointRight.Y = controller.wheel90DegPointRight.Y;
                    controller.wheelCircleCenterPointLeft.X = controller.wheelCenterPoint.X;
                    controller.wheelCircleCenterPointLeft.Y = controller.wheel90DegPointLeft.Y;

                    AppLogger.LogToGui($"Controller {1 + device} steering wheel emulation calibration values. Center=({controller.wheelCenterPoint.X}, {controller.wheelCenterPoint.Y})  90L=({controller.wheel90DegPointLeft.X}, {controller.wheel90DegPointLeft.Y})  90R=({controller.wheel90DegPointRight.X}, {controller.wheel90DegPointRight.Y})  Range={Global.GetSASteeringWheelEmulationRange(device)}", false);
                    controller.wheelPrevRecalibrateTime = DateTime.Now;
                }


                int maxRangeRight = Global.GetSASteeringWheelEmulationRange(device) / 2 * C_WHEEL_ANGLE_PRECISION;
                int maxRangeLeft = -maxRangeRight;

                //Console.WriteLine("Values {0} {1}", gyroAccelX, gyroAccelZ);

                //gyroAccelX = (int)(wheel360FilterX.Filter(gyroAccelX, currentRate));
                //gyroAccelZ = (int)(wheel360FilterZ.Filter(gyroAccelZ, currentRate));

                int wheelFuzz = SAWheelFuzzValues[device];
                if (wheelFuzz != 0)
                {
                    //int currentValueX = gyroAccelX;
                    LastWheelGyroCoord lastWheelGyro = lastWheelGyroValues[device];
                    CalcWheelFuzz(gyroAccelX, gyroAccelZ, lastWheelGyro.gyroX, lastWheelGyro.gyroZ,
                        wheelFuzz, out gyroAccelX, out gyroAccelZ);
                    lastWheelGyro.gyroX = gyroAccelX; lastWheelGyro.gyroZ = gyroAccelZ;
                    //lastGyroX = gyroAccelX; lastGyroZ = gyroAccelZ;
                }

                result = CalculateControllerAngle(gyroAccelX, gyroAccelZ, controller);

                // Apply deadzone (SA X-deadzone value). This code assumes that 20deg is the max deadzone anyone ever might wanna use (in practice effective deadzone 
                // is probably just few degrees by using SXDeadZone values 0.01...0.05)
                double sxDead = getSXDeadzone(device);
                if (sxDead > 0)
                {
                    int sxDeadInt = Convert.ToInt32(20.0 * C_WHEEL_ANGLE_PRECISION * sxDead);
                    if (Math.Abs(result) <= sxDeadInt)
                    {
                        result = 0;
                    }
                    else
                    {
                        // Smooth steering angle based on deadzone range instead of just clipping the deadzone gap
                        result -= (result < 0 ? -sxDeadInt : sxDeadInt);
                    }
                }

                // If wrapped around from +180 to -180 side (or vice versa) then SA steering wheel keeps on turning beyond 360 degrees (if range is >360).
                // Keep track of how many times the steering wheel has been turned beyond the full 360 circle and clip the result to max range.
                int wheelFullTurnCount = controller.wheelFullTurnCount;
                if (controller.wheelPrevPhysicalAngle < 0 && result > 0)
                {
                    if ((result - controller.wheelPrevPhysicalAngle) > 180 * C_WHEEL_ANGLE_PRECISION)
                    {
                        if (maxRangeRight > 360/2 * C_WHEEL_ANGLE_PRECISION)
                            wheelFullTurnCount--;
                        else
                            result = maxRangeLeft;
                    }
                }
                else if (controller.wheelPrevPhysicalAngle > 0 && result < 0)
                {
                    if ((controller.wheelPrevPhysicalAngle - result) > 180 * C_WHEEL_ANGLE_PRECISION)
                    {
                        if (maxRangeRight > 360/2 * C_WHEEL_ANGLE_PRECISION)
                            wheelFullTurnCount++;
                        else
                            result = maxRangeRight;
                    }
                }
                controller.wheelPrevPhysicalAngle = result;

                if (wheelFullTurnCount != 0)
                {
                    // Adjust value of result (steering wheel angle) based on num of full 360 turn counts
                    result += (wheelFullTurnCount * 180 * C_WHEEL_ANGLE_PRECISION * 2);
                }

                // If the new angle is more than 180 degrees further away then this is probably bogus value (controller shaking too much and gyro and velocity sensors went crazy).
                // Accept the new angle only when the new angle is within a "stability threshold", otherwise use the previous full angle value and wait for controller to be stabilized.
                if (Math.Abs(result - controller.wheelPrevFullAngle) <= 180 * C_WHEEL_ANGLE_PRECISION)
                {
                    controller.wheelPrevFullAngle = result;
                    controller.wheelFullTurnCount = wheelFullTurnCount;
                }
                else
                {
                    result = controller.wheelPrevFullAngle;
                }

                result = Mapping.ClampInt(maxRangeLeft, result, maxRangeRight);
                if (WheelSmoothInfo[device].enabled)
                {
                    double currentRate = 1.0 / currentDeviceState.elapsedTime; // Need to express poll time in Hz
                    OneEuroFilter wheelFilter = wheelFilters[device];
                    result = (int)(wheelFilter.Filter(result, currentRate));
                }

                // Debug log output of SA sensor values
                //LogToGuiSACalibrationDebugMsg($"DBG gyro=({gyroAccelX}, {gyroAccelZ})  output=({exposedState.OutputAccelX}, {exposedState.OutputAccelZ})  PitRolYaw=({currentDeviceState.Motion.gyroPitch}, {currentDeviceState.Motion.gyroRoll}, {currentDeviceState.Motion.gyroYaw})  VelPitRolYaw=({currentDeviceState.Motion.angVelPitch}, {currentDeviceState.Motion.angVelRoll}, {currentDeviceState.Motion.angVelYaw})  angle={result / (1.0 * C_WHEEL_ANGLE_PRECISION)}  fullTurns={controller.wheelFullTurnCount}", false);

                // Apply anti-deadzone (SA X-antideadzone value)
                double sxAntiDead = getSXAntiDeadzone(device);

                int outputAxisMax, outputAxisMin, outputAxisZero;
                if ( Global.OutContType[device] == OutContType.DS4 )
                {
                    // DS4 analog stick axis supports only 0...255 output value range (not the best one for steering wheel usage)
                    outputAxisMax = 255;
                    outputAxisMin = 0;
                    outputAxisZero = 128;
                }
                else
                {
                    // x360 (xinput) analog stick axis supports -32768...32767 output value range (more than enough for steering wheel usage)
                    outputAxisMax = 32767;
                    outputAxisMin = -32768;
                    outputAxisZero = 0;
                }

                switch (Global.GetSASteeringWheelEmulationAxis(device))
                {
                    case SASteeringWheelEmulationAxisType.LX:
                    case SASteeringWheelEmulationAxisType.LY:
                    case SASteeringWheelEmulationAxisType.RX:
                    case SASteeringWheelEmulationAxisType.RY:
                        // DS4 thumbstick axis output (-32768..32767 raw value range)
                        //return (((result - maxRangeLeft) * (32767 - (-32768))) / (maxRangeRight - maxRangeLeft)) + (-32768);
                        if (result == 0) return outputAxisZero;

                        if (sxAntiDead > 0)
                        {
                            sxAntiDead *= (outputAxisMax - outputAxisZero);
                            if (result < 0) return (((result - maxRangeLeft) * (outputAxisZero - Convert.ToInt32(sxAntiDead) - (outputAxisMin))) / (0 - maxRangeLeft)) + (outputAxisMin);
                            else return (((result - 0) * (outputAxisMax - (outputAxisZero + Convert.ToInt32(sxAntiDead)))) / (maxRangeRight - 0)) + (outputAxisZero + Convert.ToInt32(sxAntiDead));
                        }
                        else
                        {
                            return (((result - maxRangeLeft) * (outputAxisMax - (outputAxisMin))) / (maxRangeRight - maxRangeLeft)) + (outputAxisMin);
                        }
                        
                    case SASteeringWheelEmulationAxisType.L2R2:
                        // DS4 Trigger axis output. L2+R2 triggers share the same axis in x360 xInput/DInput controller, 
                        // so L2+R2 steering output supports only 360 turn range (-255..255 raw value range in the shared trigger axis)
                        if (result == 0) return 0;

                        result = Convert.ToInt32(Math.Round(result / (1.0 * C_WHEEL_ANGLE_PRECISION)));
                        if (result < 0) result = -181 - result;

                        if (sxAntiDead > 0)
                        {
                            sxAntiDead *= 255;
                            if (result < 0) return (((result - (-180)) * (-Convert.ToInt32(sxAntiDead) - (-255))) / (0 - (-180))) + (-255);
                            else return (((result - (0)) * (255 - (Convert.ToInt32(sxAntiDead)))) / (180 - (0))) + (Convert.ToInt32(sxAntiDead));
                        }
                        else
                        {
                            return (((result - (-180)) * (255 - (-255))) / (180 - (-180))) + (-255);
                        }

                    case SASteeringWheelEmulationAxisType.VJoy1X:
                    case SASteeringWheelEmulationAxisType.VJoy1Y:
                    case SASteeringWheelEmulationAxisType.VJoy1Z:
                    case SASteeringWheelEmulationAxisType.VJoy2X:
                    case SASteeringWheelEmulationAxisType.VJoy2Y:
                    case SASteeringWheelEmulationAxisType.VJoy2Z:
                        // SASteeringWheelEmulationAxisType.VJoy1X/VJoy1Y/VJoy1Z/VJoy2X/VJoy2Y/VJoy2Z VJoy axis output (0..32767 raw value range by default)
                        if (result == 0) return 16384;

                        if (sxAntiDead > 0)
                        {
                            sxAntiDead *= 16384;
                            if (result < 0) return (((result - maxRangeLeft) * (16384 - Convert.ToInt32(sxAntiDead) - (-0))) / (0 - maxRangeLeft)) + (-0);
                            else return (((result - 0) * (32767 - (16384 + Convert.ToInt32(sxAntiDead)))) / (maxRangeRight - 0)) + (16384 + Convert.ToInt32(sxAntiDead));
                        }
                        else
                        {
                            return (((result - maxRangeLeft) * (32767 - (-0))) / (maxRangeRight - maxRangeLeft)) + (-0);
                        }

                    default:
                        // Should never come here, but C# case statement syntax requires DEFAULT handler
                        return 0;
                }
            }
        }

    }
}
