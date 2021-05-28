using System;
using System.Collections.Generic;
using System.Threading;
using FakerInputWrapper;

namespace DS4Windows.DS4Control
{
    public class FakerInputHandler : VirtualKBMBase
    {
        public const string DISPLAY_NAME = "FakerInput";
        public const string IDENTIFIER = "fakerinput";
        public const int MODIFIER_MASK = 1 << 9;
        public const int MODIFIER_MULTIMEDIA = 1 << 10;
        public const int MODIFIER_ENHANCED = 1 << 11;

        private FakerInput fakerInput = null;
        private RelativeMouseReport mouseReport = new RelativeMouseReport();
        private KeyboardReport keyReport = new KeyboardReport();
        private KeyboardEnhancedReport mediaKeyReport = new KeyboardEnhancedReport();
        private HashSet<KeyboardModifier> modifiers = new HashSet<KeyboardModifier>();
        private HashSet<KeyboardKey> pressedKeys = new HashSet<KeyboardKey>();

        // Used to guard reports and attempt to keep methods thread safe
        private ReaderWriterLockSlim eventLock = new ReaderWriterLockSlim();

        public FakerInputHandler()
        {
            fakerInput = new FakerInput();
        }

        public override bool Connect()
        {
            return fakerInput.Connect();
        }

        public override bool Disconnect()
        {
            Release();
            fakerInput.Disconnect();
            return !fakerInput.IsConnected();
        }

        private void Release()
        {
            eventLock.EnterWriteLock();

            mouseReport.ResetMousePos();
            fakerInput.UpdateRelativeMouse(mouseReport);

            foreach(KeyboardModifier mod in modifiers)
            {
                keyReport.KeyUp(mod);
            }
            modifiers.Clear();

            foreach(KeyboardKey key in pressedKeys)
            {
                keyReport.KeyUp(key);
            }
            pressedKeys.Clear();

            fakerInput.UpdateKeyboard(keyReport);

            mediaKeyReport.EnhancedKeys = 0;
            //mediaKeyReport.MediaKeys = 0;
            fakerInput.UpdateKeyboardEnhanced(mediaKeyReport);

            eventLock.ExitWriteLock();
        }

        public override void MoveRelativeMouse(int x, int y)
        {
            const int MOUSE_MIN = -32767;
            const int MOUSE_MAX = 32767;
            //Console.WriteLine("RAW MOUSE {0} {1}", x, y);
            eventLock.EnterWriteLock();

            mouseReport.ResetMousePos();

            mouseReport.MouseX = (short)(x < MOUSE_MIN ? MOUSE_MIN : (x > MOUSE_MAX) ? MOUSE_MAX : x);
            mouseReport.MouseY = (short)(y < MOUSE_MIN ? MOUSE_MIN : (y > MOUSE_MAX) ? MOUSE_MAX : y);
            //Console.WriteLine("LKJDFSLKJDFSLKJS {0} {1}", mouseReport.MouseX, mouseReport.MouseY);

            fakerInput.UpdateRelativeMouse(mouseReport);

            eventLock.ExitWriteLock();
        }

        public override void PerformKeyPress(uint key)
        {
            //Console.WriteLine("PerformKeyPress {0}", key);
            bool sync = false;
            bool syncEnhanced = false;
            eventLock.EnterWriteLock();

            if (key < MODIFIER_MASK)
            {
                KeyboardKey temp = (KeyboardKey)key;
                if (!pressedKeys.Contains(temp))
                {
                    keyReport.KeyDown(temp);
                    pressedKeys.Add(temp);
                    sync = true;
                }
            }
            else if (key < MODIFIER_MULTIMEDIA)
            {
                KeyboardModifier modifier = (KeyboardModifier)(key & ~MODIFIER_MASK);
                if (!modifiers.Contains(modifier))
                {
                    keyReport.KeyDown(modifier);
                    modifiers.Add(modifier);
                    sync = true;
                }
            }
            else if (key < MODIFIER_ENHANCED)
            {
                EnhancedKey temp = (EnhancedKey)(key & ~MODIFIER_MULTIMEDIA);
                mediaKeyReport.KeyDown(temp);
                syncEnhanced = true;
            }
            else
            {
                EnhancedKey temp = (EnhancedKey)(key & ~MODIFIER_ENHANCED);
                mediaKeyReport.KeyDown(temp);
                syncEnhanced = true;
            }

            if (sync)
            {
                fakerInput.UpdateKeyboard(keyReport);
            }

            if (syncEnhanced)
            {
                fakerInput.UpdateKeyboardEnhanced(mediaKeyReport);
            }

            eventLock.ExitWriteLock();
        }

        /// <summary>
        /// Just use normal routine
        /// </summary>
        /// <param name="key"></param>
        public override void PerformKeyPressAlt(uint key)
        {
            //Console.WriteLine("PerformKeyPressAlt {0}", key);
            bool sync = false;
            bool syncEnhanced = false;
            eventLock.EnterWriteLock();

            if (key < MODIFIER_MASK)
            {
                KeyboardKey temp = (KeyboardKey)key;
                if (!pressedKeys.Contains(temp))
                {
                    keyReport.KeyDown(temp);
                    pressedKeys.Add(temp);
                    sync = true;
                }
            }
            else if (key < MODIFIER_MULTIMEDIA)
            {
                KeyboardModifier modifier = (KeyboardModifier)(key & ~MODIFIER_MASK);
                if (!modifiers.Contains(modifier))
                {
                    keyReport.KeyDown(modifier);
                    modifiers.Add(modifier);
                    sync = true;
                }
            }
            else if (key < MODIFIER_ENHANCED)
            {
                EnhancedKey temp = (EnhancedKey)(key & ~MODIFIER_MULTIMEDIA);
                mediaKeyReport.KeyDown(temp);
                syncEnhanced = true;
            }
            else
            {
                EnhancedKey temp = (EnhancedKey)(key & ~MODIFIER_ENHANCED);
                mediaKeyReport.KeyDown(temp);
                syncEnhanced = true;
            }

            if (sync)
            {
                fakerInput.UpdateKeyboard(keyReport);
            }

            if (syncEnhanced)
            {
                fakerInput.UpdateKeyboardEnhanced(mediaKeyReport);
            }

            eventLock.ExitWriteLock();
        }

        public override void PerformKeyRelease(uint key)
        {
            //Console.WriteLine("PerformKeyRelease {0}", key);
            bool sync = false;
            bool syncEnhanced = false;
            eventLock.EnterWriteLock();

            if (key < MODIFIER_MASK)
            {
                KeyboardKey temp = (KeyboardKey)key;
                if (pressedKeys.Contains(temp))
                {
                    keyReport.KeyUp(temp);
                    pressedKeys.Remove(temp);
                    sync = true;
                }
            }
            else if (key < MODIFIER_MULTIMEDIA)
            {
                KeyboardModifier modifier = (KeyboardModifier)(key & ~MODIFIER_MASK);
                if (modifiers.Contains(modifier))
                {
                    keyReport.KeyUp(modifier);
                    modifiers.Remove(modifier);
                    sync = true;
                }
            }
            else if (key < MODIFIER_ENHANCED)
            {
                EnhancedKey temp = (EnhancedKey)(key & ~MODIFIER_MULTIMEDIA);
                mediaKeyReport.KeyUp(temp);
                syncEnhanced = true;
            }
            else
            {
                EnhancedKey temp = (EnhancedKey)(key & ~MODIFIER_ENHANCED);
                mediaKeyReport.KeyUp(temp);
                syncEnhanced = true;
            }

            if (sync)
            {
                fakerInput.UpdateKeyboard(keyReport);
            }

            if (syncEnhanced)
            {
                fakerInput.UpdateKeyboardEnhanced(mediaKeyReport);
            }

            eventLock.ExitWriteLock();
        }

        /// <summary>
        /// Just use normal routine
        /// </summary>
        /// <param name="key"></param>
        public override void PerformKeyReleaseAlt(uint key)
        {
            //Console.WriteLine("PerformKeyReleaseAlt {0}", key);
            bool sync = false;
            bool syncEnhanced = false;
            eventLock.EnterWriteLock();

            if (key < MODIFIER_MASK)
            {
                KeyboardKey temp = (KeyboardKey)key;
                if (pressedKeys.Contains(temp))
                {
                    keyReport.KeyUp(temp);
                    pressedKeys.Remove(temp);
                    sync = true;
                }
            }
            else if (key < MODIFIER_MULTIMEDIA)
            {
                KeyboardModifier modifier = (KeyboardModifier)(key & ~MODIFIER_MASK);
                if (modifiers.Contains(modifier))
                {
                    keyReport.KeyUp(modifier);
                    modifiers.Remove(modifier);
                    sync = true;
                }
            }
            else if (key < MODIFIER_ENHANCED)
            {
                EnhancedKey temp = (EnhancedKey)(key & ~MODIFIER_MULTIMEDIA);
                mediaKeyReport.KeyUp(temp);
                syncEnhanced = true;
            }
            else
            {
                EnhancedKey temp = (EnhancedKey)(key & ~MODIFIER_ENHANCED);
                mediaKeyReport.KeyUp(temp);
                syncEnhanced = true;
            }

            if (sync)
            {
                fakerInput.UpdateKeyboard(keyReport);
            }

            if (syncEnhanced)
            {
                fakerInput.UpdateKeyboardEnhanced(mediaKeyReport);
            }

            eventLock.ExitWriteLock();
        }

        public override void PerformMouseButtonEvent(uint mouseButton)
        {
            bool sync = false;
            MouseButton temp = (MouseButton)mouseButton;
            eventLock.EnterWriteLock();

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
                fakerInput.UpdateRelativeMouse(mouseReport);
            }

            eventLock.ExitWriteLock();
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
            eventLock.EnterWriteLock();

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
                fakerInput.UpdateRelativeMouse(mouseReport);
            }

            eventLock.ExitWriteLock();
        }

        /// <summary>
        /// No support for horizontal mouse wheel in vmulti
        /// </summary>
        /// <param name="vertical"></param>
        /// <param name="horizontal"></param>
        public override void PerformMouseWheelEvent(int vertical, int horizontal)
        {
            eventLock.EnterWriteLock();
            mouseReport.ResetMousePos();
            mouseReport.WheelPosition = (byte)vertical;
            mouseReport.HWheelPosition = (byte)horizontal;
            fakerInput.UpdateRelativeMouse(mouseReport);
            eventLock.ExitWriteLock();
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
            eventLock.EnterWriteLock();

            MouseButton tempButton = (MouseButton)mouseButton;
            if (!mouseReport.HeldButtons.Contains(tempButton))
            {
                mouseReport.ResetMousePos();
                mouseReport.ButtonDown(tempButton);
                sync = true;
            }

            if (sync)
            {
                fakerInput.UpdateRelativeMouse(mouseReport);
            }

            eventLock.ExitWriteLock();
        }

        public override void PerformMouseButtonRelease(uint mouseButton)
        {
            bool sync = false;
            eventLock.EnterWriteLock();

            MouseButton tempButton = (MouseButton)mouseButton;
            if (mouseReport.HeldButtons.Contains(tempButton))
            {
                mouseReport.ResetMousePos();
                mouseReport.ButtonUp(tempButton);
                sync = true;
            }

            if (sync)
            {
                fakerInput.UpdateRelativeMouse(mouseReport);
            }

            eventLock.ExitWriteLock();
        }
    }
}
