using VMultiDllWrapper;
using DS4Windows.DS4Control;

namespace DS4WinWPF.DS4Control
{
    public class VMultiHandler : VirtualKBMBase
    {
        public const string DISPLAY_NAME = "VMulti";
        public const string IDENTIFIER = "vmulti";

        private VMulti vMulti = null;
        private RelativeMouseReport mouseReport = new RelativeMouseReport();
        private KeyboardReport keyReport = new KeyboardReport();

        public VMultiHandler()
        {
            vMulti = new VMulti();
        }

        public override bool Connect()
        {
            return vMulti.connect();
        }

        public override bool Disconnect()
        {
            vMulti.disconnect();
            return vMulti.isConnected();
        }

        public override void MoveRelativeMouse(int x, int y)
        {
            mouseReport.MouseX = (byte)(x < -127 ? 127 : (x > 127) ? 127 : x);
            mouseReport.MouseY = (byte)(y < -127 ? 127 : (y > 127) ? 127 : y);
            vMulti.updateMouse(mouseReport);
        }

        public override void PerformKeyPress(ushort key)
        {
            KeyboardKey temp = (KeyboardKey)key;
            keyReport.keyDown(temp);
            vMulti.updateKeyboard(keyReport);
        }

        /// <summary>
        /// Just use normal routine
        /// </summary>
        /// <param name="key"></param>
        public override void PerformKeyPressAlt(ushort key)
        {
            KeyboardKey temp = (KeyboardKey)key;
            keyReport.keyDown(temp);
            vMulti.updateKeyboard(keyReport);
        }

        public override void PerformKeyRelease(ushort key)
        {
            KeyboardKey temp = (KeyboardKey)key;
            keyReport.keyUp(temp);
            vMulti.updateKeyboard(keyReport);
        }

        /// <summary>
        /// Just use normal routine
        /// </summary>
        /// <param name="key"></param>
        public override void PerformKeyReleaseAlt(ushort key)
        {
            KeyboardKey temp = (KeyboardKey)key;
            keyReport.keyUp(temp);
            vMulti.updateKeyboard(keyReport);
        }

        public override void PerformMouseButtonEvent(uint mouseButton)
        {
            MouseButton temp = (MouseButton)mouseButton;
            if (!mouseReport.HeldButtons.Contains(temp))
            {
                mouseReport.ButtonDown(temp);
            }
            else
            {
                mouseReport.ButtonUp(temp);
            }

            vMulti.updateMouse(mouseReport);
        }

        /// <summary>
        /// Just use normal routine
        /// </summary>
        /// <param name="mouseButton"></param>
        /// <param name="type"></param>
        public override void PerformMouseButtonEventAlt(uint mouseButton, int type)
        {
            MouseButton temp = (MouseButton)mouseButton;
            if (!mouseReport.HeldButtons.Contains(temp))
            {
                mouseReport.ButtonDown(temp);
            }
            else
            {
                mouseReport.ButtonUp(temp);
            }

            vMulti.updateMouse(mouseReport);
        }

        /// <summary>
        /// No support for horizontal mouse wheel in vmulti
        /// </summary>
        /// <param name="vertical"></param>
        /// <param name="horizontal"></param>
        public override void PerformMouseWheelEvent(int vertical, int horizontal)
        {
            mouseReport.WheelPosition = (byte)vertical;
            vMulti.updateMouse(mouseReport);
        }

        public override string GetDisplayName()
        {
            return DISPLAY_NAME;
        }

        public override string GetIdentifier()
        {
            return IDENTIFIER;
        }

        public override void PerformMouseButtonPress(uint mouseButton)
        {
            mouseReport.ButtonDown((MouseButton)mouseButton);
            vMulti.updateMouse(mouseReport);
        }

        public override void PerformMouseButtonRelease(uint mouseButton)
        {
            mouseReport.ButtonUp((MouseButton)mouseButton);
            vMulti.updateMouse(mouseReport);
        }
    }
}
