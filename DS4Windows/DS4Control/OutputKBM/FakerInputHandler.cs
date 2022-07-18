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
        // Keys values are under 255
        public const int MODIFIER_MASK = 1 << 9;
        // Can only map to 31 keys with this approach. Underlying key value is a uint
        // (1 << 31). cannot express as uint as bit-shift op converts to int
        public const uint MODIFIER_ENHANCED = 2147483648;

        private const double ABSOLUTE_MOUSE_COOR_MAX = 32767.0;
        private const int MAX_NORMAL_KEY_PRESSED = 6;
        private FakerInput fakerInput = null;
        private RelativeMouseReport mouseReport = new RelativeMouseReport();
        private AbsoluteMouseReport absoluteMouseReport = new AbsoluteMouseReport();
        private KeyboardReport keyReport = new KeyboardReport();
        private KeyboardEnhancedReport mediaKeyReport = new KeyboardEnhancedReport();

        private HashSet<KeyboardModifier> modifiers = new HashSet<KeyboardModifier>();
        private HashSet<KeyboardKey> pressedKeys = new HashSet<KeyboardKey>();

        // Flags that will dictate which output report methods to call in Sync method
        private bool syncKeyboard;
        private bool syncEnhancedKeyboard;
        private bool syncRelativeMouse;
        private bool syncAbsoluteMouse;

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

            //mouseReport.ResetMousePos();
            mouseReport.Reset();
            syncRelativeMouse = true;
            //fakerInput.UpdateRelativeMouse(mouseReport);

            absoluteMouseReport.Reset();
            syncAbsoluteMouse = true;

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

            syncKeyboard = true;
            //fakerInput.UpdateKeyboard(keyReport);

            mediaKeyReport.EnhancedKeys = 0;
            //mediaKeyReport.MediaKeys = 0;
            syncEnhancedKeyboard = true;
            //fakerInput.UpdateKeyboardEnhanced(mediaKeyReport);

            eventLock.ExitWriteLock();

            // Perform sync here after changing report objects
            Sync();
        }

        public override void MoveRelativeMouse(int x, int y)
        {
            const int MOUSE_MIN = -32767;
            const int MOUSE_MAX = 32767;
            //Console.WriteLine("RAW MOUSE {0} {1}", x, y);
            eventLock.EnterWriteLock();

            //mouseReport.ResetMousePos();

            mouseReport.MouseX = (short)(x < MOUSE_MIN ? MOUSE_MIN : (x > MOUSE_MAX) ? MOUSE_MAX : x);
            mouseReport.MouseY = (short)(y < MOUSE_MIN ? MOUSE_MIN : (y > MOUSE_MAX) ? MOUSE_MAX : y);
            //Console.WriteLine("LKJDFSLKJDFSLKJS {0} {1}", mouseReport.MouseX, mouseReport.MouseY);

            syncRelativeMouse = true;
            //fakerInput.UpdateRelativeMouse(mouseReport);

            eventLock.ExitWriteLock();
        }

        /// <summary>
        /// Move the mouse cursor to an absolute position on the virtual desktop
        /// </summary>
        /// <param name="x">X coordinate in range of [0.0, 1.0]. 0.0 for left. 1.0 for far right</param>
        /// <param name="y">Y coordinate in range of [0.0, 1.0]. 0.0 for top. 1.0 for bottom</param>
        public override void MoveAbsoluteMouse(double x, double y)
        {
            eventLock.EnterWriteLock();

            absoluteMouseReport.MouseX = (ushort)(x * ABSOLUTE_MOUSE_COOR_MAX);
            absoluteMouseReport.MouseY = (ushort)(y * ABSOLUTE_MOUSE_COOR_MAX);
            syncAbsoluteMouse = true;

            eventLock.ExitWriteLock();
        }

        public override void PerformKeyPress(uint key)
        {
            //Console.WriteLine("PerformKeyPress {0}", key);
            eventLock.EnterWriteLock();

            if (key < MODIFIER_MASK)
            {
                KeyboardKey temp = (KeyboardKey)key;
                if (!pressedKeys.Contains(temp) && pressedKeys.Count <= MAX_NORMAL_KEY_PRESSED)
                {
                    keyReport.KeyDown(temp);
                    pressedKeys.Add(temp);
                    syncKeyboard = true;
                }
            }
            else if (key < MODIFIER_ENHANCED)
            {
                KeyboardModifier modifier = (KeyboardModifier)(key & ~MODIFIER_MASK);
                if (!modifiers.Contains(modifier))
                {
                    keyReport.KeyDown(modifier);
                    modifiers.Add(modifier);
                    syncKeyboard = true;
                }
            }
            else
            {
                EnhancedKey temp = (EnhancedKey)(key & ~MODIFIER_ENHANCED);
                mediaKeyReport.KeyDown(temp);
                syncEnhancedKeyboard = true;
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
            eventLock.EnterWriteLock();

            if (key < MODIFIER_MASK)
            {
                KeyboardKey temp = (KeyboardKey)key;
                if (!pressedKeys.Contains(temp) && pressedKeys.Count <= MAX_NORMAL_KEY_PRESSED)
                {
                    keyReport.KeyDown(temp);
                    pressedKeys.Add(temp);
                    syncKeyboard = true;
                }
            }
            else if (key < MODIFIER_ENHANCED)
            {
                KeyboardModifier modifier = (KeyboardModifier)(key & ~MODIFIER_MASK);
                if (!modifiers.Contains(modifier))
                {
                    keyReport.KeyDown(modifier);
                    modifiers.Add(modifier);
                    syncKeyboard = true;
                }
            }
            else
            {
                EnhancedKey temp = (EnhancedKey)(key & ~MODIFIER_ENHANCED);
                mediaKeyReport.KeyDown(temp);
                syncEnhancedKeyboard = true;
            }

            eventLock.ExitWriteLock();
        }

        public override void PerformKeyRelease(uint key)
        {
            //Console.WriteLine("PerformKeyRelease {0}", key);
            eventLock.EnterWriteLock();

            if (key < MODIFIER_MASK)
            {
                KeyboardKey temp = (KeyboardKey)key;
                if (pressedKeys.Contains(temp))
                {
                    keyReport.KeyUp(temp);
                    pressedKeys.Remove(temp);
                    syncKeyboard = true;
                }
            }
            else if (key < MODIFIER_ENHANCED)
            {
                KeyboardModifier modifier = (KeyboardModifier)(key & ~MODIFIER_MASK);
                if (modifiers.Contains(modifier))
                {
                    keyReport.KeyUp(modifier);
                    modifiers.Remove(modifier);
                    syncKeyboard = true;
                }
            }
            else
            {
                EnhancedKey temp = (EnhancedKey)(key & ~MODIFIER_ENHANCED);
                mediaKeyReport.KeyUp(temp);
                syncEnhancedKeyboard = true;
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
            eventLock.EnterWriteLock();

            if (key < MODIFIER_MASK)
            {
                KeyboardKey temp = (KeyboardKey)key;
                if (pressedKeys.Contains(temp))
                {
                    keyReport.KeyUp(temp);
                    pressedKeys.Remove(temp);
                    syncKeyboard = true;
                }
            }
            else if (key < MODIFIER_ENHANCED)
            {
                KeyboardModifier modifier = (KeyboardModifier)(key & ~MODIFIER_MASK);
                if (modifiers.Contains(modifier))
                {
                    keyReport.KeyUp(modifier);
                    modifiers.Remove(modifier);
                    syncKeyboard = true;
                }
            }
            else
            {
                EnhancedKey temp = (EnhancedKey)(key & ~MODIFIER_ENHANCED);
                mediaKeyReport.KeyUp(temp);
                syncEnhancedKeyboard = true;
            }

            eventLock.ExitWriteLock();
        }

        public override void PerformMouseButtonEvent(uint mouseButton)
        {
            MouseButton temp = (MouseButton)mouseButton;
            eventLock.EnterWriteLock();

            //mouseReport.ResetMousePos();

            if (!mouseReport.HeldButtons.Contains(temp))
            {
                mouseReport.ButtonDown(temp);
                syncRelativeMouse = true;
            }
            else
            {
                mouseReport.ButtonUp(temp);
                syncRelativeMouse = true;
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
            MouseButton temp = (MouseButton)mouseButton;
            eventLock.EnterWriteLock();

            //mouseReport.ResetMousePos();

            if (!mouseReport.HeldButtons.Contains(temp))
            {
                mouseReport.ButtonDown(temp);
                syncRelativeMouse = true;
            }
            else
            {
                mouseReport.ButtonUp(temp);
                syncRelativeMouse = true;
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
            //mouseReport.ResetMousePos();
            mouseReport.WheelPosition = (byte)vertical;
            mouseReport.HWheelPosition = (byte)horizontal;
            syncRelativeMouse = true;
            //fakerInput.UpdateRelativeMouse(mouseReport);
            eventLock.ExitWriteLock();
        }

        public override string GetDisplayName()
        {
            return DISPLAY_NAME;
        }

        public override string GetFullDisplayName()
        {
            return $"{DISPLAY_NAME} {version}";
        }

        public override string GetIdentifier()
        {
            return IDENTIFIER;
        }

        public override void PerformMouseButtonPress(uint mouseButton)
        {
            eventLock.EnterWriteLock();

            MouseButton tempButton = (MouseButton)mouseButton;
            if (!mouseReport.HeldButtons.Contains(tempButton))
            {
                //mouseReport.ResetMousePos();
                mouseReport.ButtonDown(tempButton);
                syncRelativeMouse = true;
            }

            eventLock.ExitWriteLock();
        }

        public override void PerformMouseButtonRelease(uint mouseButton)
        {
            eventLock.EnterWriteLock();

            MouseButton tempButton = (MouseButton)mouseButton;
            if (mouseReport.HeldButtons.Contains(tempButton))
            {
                //mouseReport.ResetMousePos();
                mouseReport.ButtonUp(tempButton);
                syncRelativeMouse = true;
            }

            eventLock.ExitWriteLock();
        }

        public override void Sync()
        {
            eventLock.EnterWriteLock();

            if (syncRelativeMouse)
            {
                fakerInput.UpdateRelativeMouse(mouseReport);
                mouseReport.ResetMousePos();
                syncRelativeMouse = false;
            }

            if (syncAbsoluteMouse)
            {
                fakerInput.UpdateAbsoluteMouse(absoluteMouseReport);
                absoluteMouseReport.Reset();
                syncAbsoluteMouse = false;
            }

            if (syncKeyboard)
            {
                fakerInput.UpdateKeyboard(keyReport);
                syncKeyboard = false;
            }

            if (syncEnhancedKeyboard)
            {
                fakerInput.UpdateKeyboardEnhanced(mediaKeyReport);
                syncEnhancedKeyboard = false;
            }

            eventLock.ExitWriteLock();
        }
    }
}
