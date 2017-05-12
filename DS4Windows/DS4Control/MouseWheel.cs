using System;

namespace DS4Windows
{
    class MouseWheel
    {
        private readonly int deviceNumber;
        public MouseWheel(int deviceNum)
        {
            deviceNumber = deviceNum;
        }

        // Keep track of remainders when performing scrolls or we lose fractional parts.
        private double horizontalRemainder = 0.0, verticalRemainder = 0.0;

        public void touchesBegan(TouchpadEventArgs arg)
        {
            if (arg.touches.Length == 2)
                horizontalRemainder = verticalRemainder = 0.0;
        }

        public void touchesMoved(TouchpadEventArgs arg, bool dragging)
        {
            if (arg.touches.Length != 2 || dragging)
                return;

            Touch lastT0 = arg.touches[0].previousTouch;
            Touch lastT1 = arg.touches[1].previousTouch;
            Touch T0 = arg.touches[0];
            Touch T1 = arg.touches[1];

            //mouse wheel 120 == 1 wheel click according to Windows API
            double lastMidX = (lastT0.hwX + lastT1.hwX) / 2d, lastMidY = (lastT0.hwY + lastT1.hwY) / 2d,
               currentMidX = (T0.hwX + T1.hwX) / 2d, currentMidY = (T0.hwY + T1.hwY) / 2d;
            double coefficient = Global.ScrollSensitivity[deviceNumber];
            // Adjust for touch distance: "standard" distance is 960 pixels, i.e. half the width.  Scroll farther if fingers are farther apart, and vice versa, in linear proportion.
            double touchXDistance = T1.hwX - T0.hwX, touchYDistance = T1.hwY - T0.hwY, touchDistance = Math.Sqrt(touchXDistance * touchXDistance + touchYDistance * touchYDistance);
            coefficient *= touchDistance / 960.0;

            // Collect rounding errors instead of losing motion.
            double xMotion = coefficient * (currentMidX - lastMidX);
            if ((xMotion > 0.0 &&horizontalRemainder > 0.0) || (xMotion < 0.0 &&horizontalRemainder < 0.0))
                    xMotion += horizontalRemainder;
            int xAction = (int)xMotion;
            horizontalRemainder = xMotion - xAction;

            double yMotion = coefficient * (lastMidY - currentMidY);
            if ((yMotion > 0.0 && verticalRemainder > 0.0) || (yMotion < 0.0 && verticalRemainder < 0.0))
                    yMotion += verticalRemainder;
            int yAction = (int)yMotion;
            verticalRemainder = yMotion - yAction;

            if (yAction != 0 || xAction != 0)
                InputMethods.MouseWheel(yAction, xAction);
        }
    }
}
