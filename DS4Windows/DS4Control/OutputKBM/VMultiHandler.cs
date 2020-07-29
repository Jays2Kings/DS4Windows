using System.Collections.Generic;
using VMultiDllWrapper;

namespace DS4Windows.DS4Control
{
    public class VMultiHandler : VirtualKBMBase
    {
        public const string DISPLAY_NAME = "VMulti";
        public const string IDENTIFIER = "vmulti";
        public const int MODIFIER_MASK = 1 << 8;

        private VMulti vMulti = null;
        private RelativeMouseReport mouseReport = new RelativeMouseReport();
        private KeyboardReport keyReport = new KeyboardReport();
        private HashSet<KeyboardModifier> modifiers = new HashSet<KeyboardModifier>();
        private HashSet<KeyboardKey> pressedKeys = new HashSet<KeyboardKey>();

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
            Release();
            vMulti.disconnect();
            return vMulti.isConnected();
        }

        private void Release()
        {
            mouseReport.ResetMousePos();
            vMulti.updateMouse(mouseReport);

            foreach(KeyboardModifier mod in modifiers)
            {
                keyReport.keyUp(mod);
            }
            modifiers.Clear();

            foreach(KeyboardKey key in pressedKeys)
            {
                keyReport.keyUp(key);
            }
            pressedKeys.Clear();

            vMulti.updateKeyboard(keyReport);
        }

        public override void MoveRelativeMouse(int x, int y)
        {
            mouseReport.ResetMousePos();
            mouseReport.MouseX = (byte)(x < -127 ? 127 : (x > 127) ? 127 : x);
            mouseReport.MouseY = (byte)(y < -127 ? 127 : (y > 127) ? 127 : y);

            vMulti.updateMouse(mouseReport);
        }

        public override void PerformKeyPress(uint key)
        {
            if (key < MODIFIER_MASK)
            {
                KeyboardKey temp = (KeyboardKey)key;
                keyReport.keyDown(temp);
                pressedKeys.Add(temp);
            }
            else
            {
                KeyboardModifier modifier = (KeyboardModifier)(key & ~MODIFIER_MASK);
                keyReport.keyDown(modifier);
                modifiers.Add(modifier);
            }

            vMulti.updateKeyboard(keyReport);
        }

        /// <summary>
        /// Just use normal routine
        /// </summary>
        /// <param name="key"></param>
        public override void PerformKeyPressAlt(uint key)
        {
            if (key < MODIFIER_MASK)
            {
                KeyboardKey temp = (KeyboardKey)key;
                keyReport.keyDown(temp);
                pressedKeys.Add(temp);
            }
            else
            {
                KeyboardModifier modifier = (KeyboardModifier)(key & ~MODIFIER_MASK);
                keyReport.keyDown(modifier);
                modifiers.Add(modifier);
            }

            vMulti.updateKeyboard(keyReport);
        }

        public override void PerformKeyRelease(uint key)
        {
            if (key < MODIFIER_MASK)
            {
                KeyboardKey temp = (KeyboardKey)key;
                keyReport.keyUp(temp);
                pressedKeys.Remove(temp);
            }
            else
            {
                KeyboardModifier modifier = (KeyboardModifier)(key & ~MODIFIER_MASK);
                keyReport.keyUp(modifier);
                modifiers.Remove(modifier);
            }

            vMulti.updateKeyboard(keyReport);
        }

        /// <summary>
        /// Just use normal routine
        /// </summary>
        /// <param name="key"></param>
        public override void PerformKeyReleaseAlt(uint key)
        {
            if (key < MODIFIER_MASK)
            {
                KeyboardKey temp = (KeyboardKey)key;
                keyReport.keyUp(temp);
                pressedKeys.Remove(temp);
            }
            else
            {
                KeyboardModifier modifier = (KeyboardModifier)(key & ~MODIFIER_MASK);
                keyReport.keyUp(modifier);
                modifiers.Remove(modifier);
            }

            vMulti.updateKeyboard(keyReport);
        }

        public override void PerformMouseButtonEvent(uint mouseButton)
        {
            mouseReport.ResetMousePos();
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
            mouseReport.ResetMousePos();
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
            mouseReport.ResetMousePos();
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
            mouseReport.ResetMousePos();
            mouseReport.ButtonDown((MouseButton)mouseButton);
            vMulti.updateMouse(mouseReport);
        }

        public override void PerformMouseButtonRelease(uint mouseButton)
        {
            mouseReport.ResetMousePos();
            mouseReport.ButtonUp((MouseButton)mouseButton);
            vMulti.updateMouse(mouseReport);
        }
    }
}
