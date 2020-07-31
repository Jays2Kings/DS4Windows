using System;
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
            //Console.WriteLine("RAW MOUSE {0} {1}", x, y);
            mouseReport.ResetMousePos();
            mouseReport.MouseX = (byte)(x < -127 ? 127 : (x > 127) ? 127 : x);
            mouseReport.MouseY = (byte)(y < -127 ? 127 : (y > 127) ? 127 : y);

            vMulti.updateMouse(mouseReport);
        }

        public override void PerformKeyPress(uint key)
        {
            //Console.WriteLine("PerformKeyPress {0}", key);
            bool sync = false;

            if (key < MODIFIER_MASK)
            {
                KeyboardKey temp = (KeyboardKey)key;
                if (!pressedKeys.Contains(temp))
                {
                    keyReport.keyDown(temp);
                    pressedKeys.Add(temp);
                    sync = true;
                }
            }
            else
            {
                KeyboardModifier modifier = (KeyboardModifier)(key & ~MODIFIER_MASK);
                if (!modifiers.Contains(modifier))
                {
                    keyReport.keyDown(modifier);
                    modifiers.Add(modifier);
                    sync = true;
                }
            }

            if (sync)
            {
                vMulti.updateKeyboard(keyReport);
            }
        }

        /// <summary>
        /// Just use normal routine
        /// </summary>
        /// <param name="key"></param>
        public override void PerformKeyPressAlt(uint key)
        {
            //Console.WriteLine("PerformKeyPressAlt {0}", key);
            bool sync = false;

            if (key < MODIFIER_MASK)
            {
                KeyboardKey temp = (KeyboardKey)key;
                if (!pressedKeys.Contains(temp))
                {
                    keyReport.keyDown(temp);
                    pressedKeys.Add(temp);
                    sync = true;
                }
            }
            else
            {
                KeyboardModifier modifier = (KeyboardModifier)(key & ~MODIFIER_MASK);
                if (!modifiers.Contains(modifier))
                {
                    keyReport.keyDown(modifier);
                    modifiers.Add(modifier);
                    sync = true;
                }
            }

            if (sync)
            {
                vMulti.updateKeyboard(keyReport);
            }
        }

        public override void PerformKeyRelease(uint key)
        {
            //Console.WriteLine("PerformKeyRelease {0}", key);
            bool sync = false;

            if (key < MODIFIER_MASK)
            {
                KeyboardKey temp = (KeyboardKey)key;
                if (pressedKeys.Contains(temp))
                {
                    keyReport.keyUp(temp);
                    pressedKeys.Remove(temp);
                    sync = true;
                }
            }
            else
            {
                KeyboardModifier modifier = (KeyboardModifier)(key & ~MODIFIER_MASK);
                if (modifiers.Contains(modifier))
                {
                    keyReport.keyUp(modifier);
                    modifiers.Remove(modifier);
                    sync = true;
                }
            }

            if (sync)
            {
                vMulti.updateKeyboard(keyReport);
            }
        }

        /// <summary>
        /// Just use normal routine
        /// </summary>
        /// <param name="key"></param>
        public override void PerformKeyReleaseAlt(uint key)
        {
            //Console.WriteLine("PerformKeyReleaseAlt {0}", key);
            bool sync = false;

            if (key < MODIFIER_MASK)
            {
                KeyboardKey temp = (KeyboardKey)key;
                if (pressedKeys.Contains(temp))
                {
                    keyReport.keyUp(temp);
                    pressedKeys.Remove(temp);
                    sync = true;
                }
            }
            else
            {
                KeyboardModifier modifier = (KeyboardModifier)(key & ~MODIFIER_MASK);
                if (modifiers.Contains(modifier))
                {
                    keyReport.keyUp(modifier);
                    modifiers.Remove(modifier);
                    sync = true;
                }
            }

            if (sync)
            {
                vMulti.updateKeyboard(keyReport);
            }
        }

        public override void PerformMouseButtonEvent(uint mouseButton)
        {
            bool sync = false;
            MouseButton temp = (MouseButton)mouseButton;

            mouseReport.ResetMousePos();

            if (!mouseReport.HeldButtons.Contains(temp))
            {
                mouseReport.ButtonDown(temp);
                sync = true;
            }
            else
            {
                mouseReport.ButtonUp(temp);
                sync = true;
            }

            if (sync)
            {
                vMulti.updateMouse(mouseReport);
            }
        }

        /// <summary>
        /// Just use normal routine
        /// </summary>
        /// <param name="mouseButton"></param>
        /// <param name="type"></param>
        public override void PerformMouseButtonEventAlt(uint mouseButton, int type)
        {
            bool sync = false;
            MouseButton temp = (MouseButton)mouseButton;

            mouseReport.ResetMousePos();

            if (!mouseReport.HeldButtons.Contains(temp))
            {
                mouseReport.ButtonDown(temp);
                sync = true;
            }
            else
            {
                mouseReport.ButtonUp(temp);
                sync = true;
            }

            if (sync)
            {
                vMulti.updateMouse(mouseReport);
            }
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
            bool sync = false;

            MouseButton tempButton = (MouseButton)mouseButton;
            if (!mouseReport.HeldButtons.Contains(tempButton))
            {
                mouseReport.ResetMousePos();
                mouseReport.ButtonDown(tempButton);
                sync = true;
            }

            if (sync)
            {
                vMulti.updateMouse(mouseReport);
            }
        }

        public override void PerformMouseButtonRelease(uint mouseButton)
        {
            bool sync = false;

            MouseButton tempButton = (MouseButton)mouseButton;
            if (mouseReport.HeldButtons.Contains(tempButton))
            {
                mouseReport.ResetMousePos();
                mouseReport.ButtonUp(tempButton);
                sync = true;
            }

            if (sync)
            {
                vMulti.updateMouse(mouseReport);
            }
        }
    }
}
