
namespace DS4Windows
{
    public class DS4StateFieldMapping
    {
        public enum ControlType: int { Unknown = 0, Button, AxisDir, Trigger, Touch, GyroDir, SwipeDir }

        public bool[] buttons = new bool[(int)DS4Controls.GyroSwipeDown + 1];
        public byte[] axisdirs = new byte[(int)DS4Controls.GyroSwipeDown + 1];
        public byte[] triggers = new byte[(int)DS4Controls.GyroSwipeDown + 1];
        public int[] gryodirs = new int[(int)DS4Controls.GyroSwipeDown + 1];
        public byte[] swipedirs = new byte[(int)DS4Controls.GyroSwipeDown + 1];
        public bool[] swipedirbools = new bool[(int)DS4Controls.GyroSwipeDown + 1];
        public bool touchButton = false;
        public bool outputTouchButton = false;

        public static ControlType[] mappedType = new ControlType[45] { ControlType.Unknown, // DS4Controls.None
            ControlType.AxisDir, // DS4Controls.LXNeg
            ControlType.AxisDir, // DS4Controls.LXPos
            ControlType.AxisDir, // DS4Controls.LYNeg
            ControlType.AxisDir, // DS4Controls.LYPos
            ControlType.AxisDir, // DS4Controls.RXNeg
            ControlType.AxisDir, // DS4Controls.RXPos
            ControlType.AxisDir, // DS4Controls.RYNeg
            ControlType.AxisDir, // DS4Controls.RYPos
            ControlType.Button, // DS4Controls.L1
            ControlType.Trigger, // DS4Controls.L2
            ControlType.Button, // DS4Controls.L3
            ControlType.Button, // DS4Controls.R1
            ControlType.Trigger, // DS4Controls.R2
            ControlType.Button, // DS4Controls.R3
            ControlType.Button, // DS4Controls.Square
            ControlType.Button, // DS4Controls.Triangle
            ControlType.Button, // DS4Controls.Circle
            ControlType.Button, // DS4Controls.Cross
            ControlType.Button, // DS4Controls.DpadUp
            ControlType.Button, // DS4Controls.DpadRight
            ControlType.Button, // DS4Controls.DpadDown
            ControlType.Button, // DS4Controls.DpadLeft
            ControlType.Button, // DS4Controls.PS
            ControlType.Touch, // DS4Controls.TouchLeft
            ControlType.Touch, // DS4Controls.TouchUpper
            ControlType.Touch, // DS4Controls.TouchMulti
            ControlType.Touch, // DS4Controls.TouchRight
            ControlType.Button, // DS4Controls.Share
            ControlType.Button, // DS4Controls.Options
            ControlType.Button, // DS4Controls.Mute
            ControlType.GyroDir, // DS4Controls.GyroXPos
            ControlType.GyroDir, // DS4Controls.GyroXNeg
            ControlType.GyroDir, // DS4Controls.GyroZPos
            ControlType.GyroDir, // DS4Controls.GyroZNeg
            ControlType.SwipeDir, // DS4Controls.SwipeLeft
            ControlType.SwipeDir, // DS4Controls.SwipeRight
            ControlType.SwipeDir, // DS4Controls.SwipeUp
            ControlType.SwipeDir, // DS4Controls.SwipeDown
            ControlType.Button, // DS4Controls.L2FullPull
            ControlType.Button, // DS4Controls.R2FullPull
            ControlType.Button, // DS4Controls.GyroSwipeLeft
            ControlType.Button, // DS4Controls.GyroSwipeRight
            ControlType.Button, // DS4Controls.GyroSwipeUp
            ControlType.Button, // DS4Controls.GyroSwipeDown
        };

        public DS4StateFieldMapping()
        {
        }

        public DS4StateFieldMapping(DS4State cState, DS4StateExposed exposeState, Mouse tp, bool priorMouse=false)
        {
            populateFieldMapping(cState, exposeState, tp, priorMouse);
        }

        public void populateFieldMapping(DS4State cState, DS4StateExposed exposeState, Mouse tp, bool priorMouse = false)
        {
            unchecked
            {
                axisdirs[(int)DS4Controls.LXNeg] = cState.LX;
                axisdirs[(int)DS4Controls.LXPos] = cState.LX;
                axisdirs[(int)DS4Controls.LYNeg] = cState.LY;
                axisdirs[(int)DS4Controls.LYPos] = cState.LY;

                axisdirs[(int)DS4Controls.RXNeg] = cState.RX;
                axisdirs[(int)DS4Controls.RXPos] = cState.RX;
                axisdirs[(int)DS4Controls.RYNeg] = cState.RY;
                axisdirs[(int)DS4Controls.RYPos] = cState.RY;

                triggers[(int)DS4Controls.L2] = cState.L2;
                triggers[(int)DS4Controls.R2] = cState.R2;

                buttons[(int)DS4Controls.L1] = cState.L1;
                buttons[(int)DS4Controls.L2FullPull] = cState.L2 == 255;
                buttons[(int)DS4Controls.L3] = cState.L3;
                buttons[(int)DS4Controls.R1] = cState.R1;
                buttons[(int)DS4Controls.R2FullPull] = cState.R2 == 255;
                buttons[(int)DS4Controls.R3] = cState.R3;

                buttons[(int)DS4Controls.Cross] = cState.Cross;
                buttons[(int)DS4Controls.Triangle] = cState.Triangle;
                buttons[(int)DS4Controls.Circle] = cState.Circle;
                buttons[(int)DS4Controls.Square] = cState.Square;
                buttons[(int)DS4Controls.PS] = cState.PS;
                buttons[(int)DS4Controls.Options] = cState.Options;
                buttons[(int)DS4Controls.Share] = cState.Share;
                buttons[(int)DS4Controls.Mute] = cState.Mute;

                buttons[(int)DS4Controls.DpadUp] = cState.DpadUp;
                buttons[(int)DS4Controls.DpadRight] = cState.DpadRight;
                buttons[(int)DS4Controls.DpadDown] = cState.DpadDown;
                buttons[(int)DS4Controls.DpadLeft] = cState.DpadLeft;

                buttons[(int)DS4Controls.TouchLeft] = tp != null ? (!priorMouse ? tp.leftDown : tp.priorLeftDown) : false;
                buttons[(int)DS4Controls.TouchRight] = tp != null ? (!priorMouse ? tp.rightDown : tp.priorRightDown) : false;
                buttons[(int)DS4Controls.TouchUpper] = tp != null ? (!priorMouse ? tp.upperDown : tp.priorUpperDown) : false;
                buttons[(int)DS4Controls.TouchMulti] = tp != null ? (!priorMouse ? tp.multiDown : tp.priorMultiDown) : false;

                int sixAxisX = -exposeState.getOutputAccelX();
                gryodirs[(int)DS4Controls.GyroXPos] = sixAxisX > 0 ? sixAxisX : 0;
                gryodirs[(int)DS4Controls.GyroXNeg] = sixAxisX < 0 ? sixAxisX : 0;

                int sixAxisZ = exposeState.getOutputAccelZ();
                gryodirs[(int)DS4Controls.GyroZPos] = sixAxisZ > 0 ? sixAxisZ : 0;
                gryodirs[(int)DS4Controls.GyroZNeg] = sixAxisZ < 0 ? sixAxisZ : 0;

                swipedirs[(int)DS4Controls.SwipeLeft] = tp != null ? (!priorMouse ? tp.swipeLeftB : tp.priorSwipeLeftB) : (byte)0;
                swipedirs[(int)DS4Controls.SwipeRight] = tp != null ? (!priorMouse ? tp.swipeRightB : tp.priorSwipeRightB) : (byte)0;
                swipedirs[(int)DS4Controls.SwipeUp] = tp != null ? (!priorMouse ? tp.swipeUpB : tp.priorSwipeUpB) : (byte)0;
                swipedirs[(int)DS4Controls.SwipeDown] = tp != null ? (!priorMouse ? tp.swipeDownB : tp.priorSwipeDownB) : (byte)0;

                swipedirbools[(int)DS4Controls.SwipeLeft] = tp != null ? (!priorMouse ? tp.swipeLeft : tp.priorSwipeLeft) : false;
                swipedirbools[(int)DS4Controls.SwipeRight] = tp != null ? (!priorMouse ? tp.swipeRight : tp.priorSwipeRight) : false;
                swipedirbools[(int)DS4Controls.SwipeUp] = tp != null ? (!priorMouse ? tp.swipeUp : tp.priorSwipeUp) : false;
                swipedirbools[(int)DS4Controls.SwipeDown] = tp != null ? (!priorMouse ? tp.swipeDown : tp.priorSwipeDown) : false;

                buttons[(int)DS4Controls.GyroSwipeLeft] = tp != null ? tp.gyroSwipe.swipeLeft : false;
                buttons[(int)DS4Controls.GyroSwipeRight] = tp != null ? tp.gyroSwipe.swipeRight : false;
                buttons[(int)DS4Controls.GyroSwipeUp] = tp != null ? tp.gyroSwipe.swipeUp : false;
                buttons[(int)DS4Controls.GyroSwipeDown] = tp != null ? tp.gyroSwipe.swipeDown : false;

                touchButton = cState.TouchButton;
                outputTouchButton = cState.OutputTouchButton;
            }
        }

        public void populateState(DS4State state)
        {
            unchecked
            {
                state.LX = axisdirs[(int)DS4Controls.LXNeg];
                state.LX = axisdirs[(int)DS4Controls.LXPos];
                state.LY = axisdirs[(int)DS4Controls.LYNeg];
                state.LY = axisdirs[(int)DS4Controls.LYPos];

                state.RX = axisdirs[(int)DS4Controls.RXNeg];
                state.RX = axisdirs[(int)DS4Controls.RXPos];
                state.RY = axisdirs[(int)DS4Controls.RYNeg];
                state.RY = axisdirs[(int)DS4Controls.RYPos];

                state.L2 = triggers[(int)DS4Controls.L2];
                state.R2 = triggers[(int)DS4Controls.R2];

                state.L1 = buttons[(int)DS4Controls.L1];
                state.L3 = buttons[(int)DS4Controls.L3];
                state.R1 = buttons[(int)DS4Controls.R1];
                state.R3 = buttons[(int)DS4Controls.R3];

                state.Cross = buttons[(int)DS4Controls.Cross];
                state.Triangle = buttons[(int)DS4Controls.Triangle];
                state.Circle = buttons[(int)DS4Controls.Circle];
                state.Square = buttons[(int)DS4Controls.Square];
                state.PS = buttons[(int)DS4Controls.PS];
                state.Options = buttons[(int)DS4Controls.Options];
                state.Share = buttons[(int)DS4Controls.Share];
                state.Mute = buttons[(int)DS4Controls.Mute];

                state.DpadUp = buttons[(int)DS4Controls.DpadUp];
                state.DpadRight = buttons[(int)DS4Controls.DpadRight];
                state.DpadDown = buttons[(int)DS4Controls.DpadDown];
                state.DpadLeft = buttons[(int)DS4Controls.DpadLeft];
                state.TouchButton = touchButton;
                state.OutputTouchButton = outputTouchButton;
            }
        }
    }
}
