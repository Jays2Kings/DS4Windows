using System;
using System.Collections.Generic;
using System.Windows.Input;
using FakerInputWrapper;
using MouseButton = FakerInputWrapper.MouseButton;

namespace DS4Windows.DS4Control
{
    public class FakerInputMapping : VirtualKBMMapping
    {
        private Dictionary<ushort, uint> mappingPairs = new Dictionary<ushort, uint>();

        /// <summary>
        /// Use to define keys not available in the VMultiDllWrapper. Might not use
        /// </summary>
        public static class ExpandedKeyboardKeys
        {
            public const byte KEY_NONE = 0x00;
        }

        public override void PopulateConstants()
        {
            MOUSEEVENTF_LEFTDOWN = (uint)MouseButton.LeftButton; MOUSEEVENTF_LEFTUP = (uint)MouseButton.LeftButton;
            MOUSEEVENTF_RIGHTDOWN = (uint)MouseButton.RightButton; MOUSEEVENTF_RIGHTUP = (uint)MouseButton.RightButton;
            MOUSEEVENTF_MIDDLEDOWN = (uint)MouseButton.MiddleButton; MOUSEEVENTF_MIDDLEUP = (uint)MouseButton.MiddleButton;
            // Buttons not supported in vanilla VMulti driver
            MOUSEEVENTF_XBUTTONDOWN = 0; MOUSEEVENTF_XBUTTONUP = 0;

            MOUSEEVENTF_WHEEL = 0x0800; MOUSEEVENTF_HWHEEL = 0x1000;

            KEY_TAB = (uint)KeyboardKey.Tab;
            KEY_LALT = (uint)KeyboardModifier.LAlt | FakerInputHandler.MODIFIER_MASK;
            WHEEL_TICK_DOWN = -1; WHEEL_TICK_UP = 1;
            macroKeyTranslate = true;
        }

        public override void PopulateMappings()
        {
            if (mappingPairs.Count == 0)
            {
                // Map A - Z keys
                for (int i = 0; i <= KeyboardKey.Z - KeyboardKey.A; i++)
                {
                    mappingPairs.Add((ushort)KeyInterop.VirtualKeyFromKey(Key.A + i), (ushort)((ushort)KeyboardKey.A + i));
                }

                // Map 1 - 9 keys
                for (int i = 0; i <= KeyboardKey.Number9 - KeyboardKey.Number1; i++)
                {
                    mappingPairs.Add((ushort)KeyInterop.VirtualKeyFromKey(Key.D1 + i),
                        (ushort)((ushort)KeyboardKey.Number1 + i));
                }

                // Map 0 key
                mappingPairs.Add((ushort)KeyInterop.VirtualKeyFromKey(Key.D0), (ushort)KeyboardKey.Number0);

                // Map F1 - F12 keys
                for (int i = 0; i <= KeyboardKey.F12 - KeyboardKey.F1; i++)
                {
                    mappingPairs.Add((ushort)KeyInterop.VirtualKeyFromKey(Key.F1 + i), (ushort)((ushort)KeyboardKey.F1 + i));
                }

                // Map F13 - F24 keys
                for (int i = 0; i <= KeyboardKey.F24 - KeyboardKey.F13; i++)
                {
                    mappingPairs.Add((ushort)KeyInterop.VirtualKeyFromKey(Key.F13 + i), (ushort)((ushort)KeyboardKey.F13 + i));
                }

                // Map Numpad 1 - 9 keys
                for (int i = 0; i <= KeyboardKey.Keypad9 - KeyboardKey.Keypad1; i++)
                {
                    mappingPairs.Add((ushort)KeyInterop.VirtualKeyFromKey(Key.NumPad1 + i),
                        (ushort)((ushort)KeyboardKey.Keypad1 + i));
                }

                // Map Numpad 0 key
                mappingPairs.Add((ushort)KeyInterop.VirtualKeyFromKey(Key.NumPad0), (ushort)KeyboardKey.Keypad0);

                // Map more Numpad keys
                mappingPairs.Add((ushort)KeyInterop.VirtualKeyFromKey(Key.Divide), (ushort)KeyboardKey.KeypadDivide);
                mappingPairs.Add((ushort)KeyInterop.VirtualKeyFromKey(Key.Multiply), (ushort)KeyboardKey.KeypadMultiply);
                mappingPairs.Add((ushort)KeyInterop.VirtualKeyFromKey(Key.Subtract), (ushort)KeyboardKey.KeypadSubtract);
                mappingPairs.Add((ushort)KeyInterop.VirtualKeyFromKey(Key.Add), (ushort)KeyboardKey.KeypadAdd);
                mappingPairs.Add((ushort)KeyInterop.VirtualKeyFromKey(Key.Separator), (ushort)KeyboardKey.KeypadDecimal);
                //mappingPairs.Add((ushort)KeyInterop.VirtualKeyFromKey(Key.Return), (ushort)KeyboardKey.KeypadEnter);

                mappingPairs.Add((ushort)KeyInterop.VirtualKeyFromKey(Key.Escape), (ushort)KeyboardKey.Escape);
                mappingPairs.Add((ushort)KeyInterop.VirtualKeyFromKey(Key.OemTilde), (ushort)KeyboardKey.Tilde);
                mappingPairs.Add((ushort)KeyInterop.VirtualKeyFromKey(Key.Tab), (ushort)KeyboardKey.Tab);
                mappingPairs.Add((ushort)KeyInterop.VirtualKeyFromKey(Key.OemMinus), (ushort)KeyboardKey.Subtract);
                mappingPairs.Add((ushort)KeyInterop.VirtualKeyFromKey(Key.OemPlus), (ushort)KeyboardKey.Equals);
                //Console.WriteLine("FINISH: {0}", KeyInterop.VirtualKeyFromKey(Key.LeftShift));
                mappingPairs.Add((ushort)KeyInterop.VirtualKeyFromKey(Key.Back), (ushort)KeyboardKey.Backspace);
                mappingPairs.Add((ushort)KeyInterop.VirtualKeyFromKey(Key.CapsLock), (ushort)KeyboardKey.CapsLock);
                mappingPairs.Add((ushort)KeyInterop.VirtualKeyFromKey(Key.Enter), (ushort)KeyboardKey.Enter);
                mappingPairs.Add((ushort)KeyInterop.VirtualKeyFromKey(Key.Space), (ushort)KeyboardKey.Spacebar);
                mappingPairs.Add((ushort)KeyInterop.VirtualKeyFromKey(Key.Up), (ushort)KeyboardKey.UpArrow);
                mappingPairs.Add((ushort)KeyInterop.VirtualKeyFromKey(Key.Down), (ushort)KeyboardKey.DownArrow);
                mappingPairs.Add((ushort)KeyInterop.VirtualKeyFromKey(Key.Left), (ushort)KeyboardKey.LeftArrow);
                mappingPairs.Add((ushort)KeyInterop.VirtualKeyFromKey(Key.Right), (ushort)KeyboardKey.RightArrow);
                mappingPairs.Add((ushort)KeyInterop.VirtualKeyFromKey(Key.OemOpenBrackets), (ushort)KeyboardKey.OpenBrace);
                mappingPairs.Add((ushort)KeyInterop.VirtualKeyFromKey(Key.OemCloseBrackets), (ushort)KeyboardKey.CloseBrace);
                mappingPairs.Add((ushort)KeyInterop.VirtualKeyFromKey(Key.Oem5), (ushort)KeyboardKey.Backslash);
                mappingPairs.Add((ushort)KeyInterop.VirtualKeyFromKey(Key.OemSemicolon), (ushort)KeyboardKey.Semicolon);
                mappingPairs.Add((ushort)KeyInterop.VirtualKeyFromKey(Key.OemQuotes), (ushort)KeyboardKey.Quote);
                mappingPairs.Add((ushort)KeyInterop.VirtualKeyFromKey(Key.OemComma), (ushort)KeyboardKey.Comma);
                mappingPairs.Add((ushort)KeyInterop.VirtualKeyFromKey(Key.OemPeriod), (ushort)KeyboardKey.Dot);
                mappingPairs.Add((ushort)KeyInterop.VirtualKeyFromKey(Key.OemQuestion), (ushort)KeyboardKey.ForwardSlash);
                mappingPairs.Add((ushort)KeyInterop.VirtualKeyFromKey(Key.Insert), (ushort)KeyboardKey.Insert);
                mappingPairs.Add((ushort)KeyInterop.VirtualKeyFromKey(Key.Delete), (ushort)KeyboardKey.Delete);
                mappingPairs.Add((ushort)KeyInterop.VirtualKeyFromKey(Key.Home), (ushort)KeyboardKey.Home);
                mappingPairs.Add((ushort)KeyInterop.VirtualKeyFromKey(Key.End), (ushort)KeyboardKey.End);
                mappingPairs.Add((ushort)KeyInterop.VirtualKeyFromKey(Key.PageUp), (ushort)KeyboardKey.PageUp);
                mappingPairs.Add((ushort)KeyInterop.VirtualKeyFromKey(Key.PageDown), (ushort)KeyboardKey.PageDown);
                mappingPairs.Add((ushort)KeyInterop.VirtualKeyFromKey(Key.PrintScreen), (ushort)KeyboardKey.PrintScreen);
                mappingPairs.Add((ushort)KeyInterop.VirtualKeyFromKey(Key.Scroll), (ushort)KeyboardKey.ScrollLock);
                mappingPairs.Add((ushort)KeyInterop.VirtualKeyFromKey(Key.Pause), (ushort)KeyboardKey.Pause);
                mappingPairs.Add((ushort)KeyInterop.VirtualKeyFromKey(Key.NumLock), (ushort)KeyboardKey.NumLock);

                // Map modifier keys. Need to add a mask to separate modifier key values
                // from normal keyboard keys.
                mappingPairs.Add((ushort)KeyInterop.VirtualKeyFromKey(Key.LeftCtrl),
                    (ushort)KeyboardModifier.LControl | FakerInputHandler.MODIFIER_MASK);
                mappingPairs.Add((ushort)KeyInterop.VirtualKeyFromKey(Key.RightCtrl),
                    (ushort)KeyboardModifier.RControl | FakerInputHandler.MODIFIER_MASK);
                mappingPairs.Add((ushort)KeyInterop.VirtualKeyFromKey(Key.LeftAlt),
                    (ushort)KeyboardModifier.LAlt | FakerInputHandler.MODIFIER_MASK);
                // Bind VK_MENU to LAlt as well
                mappingPairs.Add(0x12,
                    (ushort)KeyboardModifier.LAlt | FakerInputHandler.MODIFIER_MASK);
                mappingPairs.Add((ushort)KeyInterop.VirtualKeyFromKey(Key.RightAlt),
                    (ushort)KeyboardModifier.RAlt | FakerInputHandler.MODIFIER_MASK);
                mappingPairs.Add((ushort)KeyInterop.VirtualKeyFromKey(Key.LeftShift),
                    (ushort)KeyboardModifier.LShift | FakerInputHandler.MODIFIER_MASK);
                // Bind VK_SHIFT to LeftShift as well
                mappingPairs.Add(0x10,
                    (ushort)KeyboardModifier.LShift | FakerInputHandler.MODIFIER_MASK);
                mappingPairs.Add((ushort)KeyInterop.VirtualKeyFromKey(Key.RightShift),
                    (ushort)KeyboardModifier.RShift | FakerInputHandler.MODIFIER_MASK);
                mappingPairs.Add((ushort)KeyInterop.VirtualKeyFromKey(Key.LWin),
                    (ushort)KeyboardModifier.LWin | FakerInputHandler.MODIFIER_MASK);
                mappingPairs.Add((ushort)KeyInterop.VirtualKeyFromKey(Key.RWin),
                    (ushort)KeyboardModifier.RWin | FakerInputHandler.MODIFIER_MASK);

                // Map Enhanced Keys. Need to add a mask to separate modifier key values
                // from normal keyboard keys.
                mappingPairs.Add((ushort)KeyInterop.VirtualKeyFromKey(Key.BrowserFavorites),
                    (uint)EnhancedKey.ACBookmarks | FakerInputHandler.MODIFIER_ENHANCED);
                mappingPairs.Add((ushort)KeyInterop.VirtualKeyFromKey(Key.BrowserSearch),
                    (ushort)EnhancedKey.WWWSearch | FakerInputHandler.MODIFIER_ENHANCED);
                mappingPairs.Add((ushort)KeyInterop.VirtualKeyFromKey(Key.BrowserStop),
                    (uint)EnhancedKey.ACStop | FakerInputHandler.MODIFIER_ENHANCED);
                mappingPairs.Add((ushort)KeyInterop.VirtualKeyFromKey(Key.BrowserBack),
                    (uint)EnhancedKey.WWWBack | FakerInputHandler.MODIFIER_ENHANCED);
                mappingPairs.Add((ushort)KeyInterop.VirtualKeyFromKey(Key.SelectMedia),
                    (uint)EnhancedKey.MediaSelect | FakerInputHandler.MODIFIER_ENHANCED);
                mappingPairs.Add((ushort)KeyInterop.VirtualKeyFromKey(Key.LaunchMail),
                    (uint)EnhancedKey.Mail | FakerInputHandler.MODIFIER_ENHANCED);

                // Map Multimedia Keys. Need to add a mask to separate modifier key values
                // from normal keyboard keys.
                mappingPairs.Add((ushort)KeyInterop.VirtualKeyFromKey(Key.MediaNextTrack),
                    (uint)EnhancedKey.ScanNextTrack | FakerInputHandler.MODIFIER_ENHANCED);
                mappingPairs.Add((ushort)KeyInterop.VirtualKeyFromKey(Key.MediaPreviousTrack),
                    (uint)EnhancedKey.ScanPreviousTrack | FakerInputHandler.MODIFIER_ENHANCED);
                mappingPairs.Add((ushort)KeyInterop.VirtualKeyFromKey(Key.MediaStop),
                    (uint)EnhancedKey.Stop | FakerInputHandler.MODIFIER_ENHANCED);
                mappingPairs.Add((ushort)KeyInterop.VirtualKeyFromKey(Key.MediaPlayPause),
                    (uint)EnhancedKey.PlayPause | FakerInputHandler.MODIFIER_ENHANCED);
                mappingPairs.Add((ushort)KeyInterop.VirtualKeyFromKey(Key.VolumeMute),
                    (uint)EnhancedKey.Mute | FakerInputHandler.MODIFIER_ENHANCED);
                mappingPairs.Add((ushort)KeyInterop.VirtualKeyFromKey(Key.VolumeUp),
                    (uint)EnhancedKey.VolumeUp | FakerInputHandler.MODIFIER_ENHANCED);
                mappingPairs.Add((ushort)KeyInterop.VirtualKeyFromKey(Key.VolumeDown),
                    (uint)EnhancedKey.VolumeDown | FakerInputHandler.MODIFIER_ENHANCED);
            }
        }

        public override uint GetRealEventKey(uint winVkKey)
        {
            mappingPairs.TryGetValue((ushort)winVkKey, out uint result);
            return result;
        }
    }
}
