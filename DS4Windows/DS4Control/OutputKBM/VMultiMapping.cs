using System.Collections.Generic;
using System.Windows.Input;
using VMultiDllWrapper;
using MouseButton = VMultiDllWrapper.MouseButton;

namespace DS4WinWPF.DS4Control
{
    public class VMultiMapping : VirtualKBMMapping
    {
        private Dictionary<ushort, ushort> mappingPairs = new Dictionary<ushort, ushort>();

        public override void PopulateConstants()
        {
            MOUSEEVENTF_LEFTDOWN = (uint)MouseButton.LeftButton; MOUSEEVENTF_LEFTUP = (uint)MouseButton.LeftButton;
            MOUSEEVENTF_RIGHTDOWN = (uint)MouseButton.RightButton; MOUSEEVENTF_RIGHTUP = (uint)MouseButton.RightButton;
            MOUSEEVENTF_MIDDLEDOWN = (uint)MouseButton.MiddleButton; MOUSEEVENTF_MIDDLEUP = (uint)MouseButton.MiddleButton;
            // Buttons not supported in vanilla VMulti
            MOUSEEVENTF_XBUTTONDOWN = 0; MOUSEEVENTF_XBUTTONUP = 0;

            MOUSEEVENTF_WHEEL = 0x0800; MOUSEEVENTF_HWHEEL = 0x1000;
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

                // Map Numpad 1 - 9 keys
                for (int i = 0; i <= KeyboardKey.Keypad9 - KeyboardKey.Keypad1; i++)
                {
                    mappingPairs.Add((ushort)KeyInterop.VirtualKeyFromKey(Key.NumPad1 + i),
                        (ushort)((ushort)KeyboardKey.Keypad1 + i));
                }

                // Map Numpad 0 key
                mappingPairs.Add((ushort)KeyInterop.VirtualKeyFromKey(Key.NumPad0), (ushort)KeyboardKey.Keypad0);

                mappingPairs.Add((ushort)KeyInterop.VirtualKeyFromKey(Key.Escape), (ushort)KeyboardKey.Escape);
                mappingPairs.Add((ushort)KeyInterop.VirtualKeyFromKey(Key.OemTilde), (ushort)KeyboardKey.Tilde);
                mappingPairs.Add((ushort)KeyInterop.VirtualKeyFromKey(Key.Tab), (ushort)KeyboardKey.Tab);
                mappingPairs.Add((ushort)KeyInterop.VirtualKeyFromKey(Key.Back), (ushort)KeyboardKey.Backspace);
                mappingPairs.Add((ushort)KeyInterop.VirtualKeyFromKey(Key.CapsLock), (ushort)KeyboardKey.CapsLock);
                mappingPairs.Add((ushort)KeyInterop.VirtualKeyFromKey(Key.Enter), (ushort)KeyboardKey.Enter);
                mappingPairs.Add((ushort)KeyInterop.VirtualKeyFromKey(Key.Space), (ushort)KeyboardKey.Spacebar);
                mappingPairs.Add((ushort)KeyInterop.VirtualKeyFromKey(Key.Up), (ushort)KeyboardKey.UpArrow);
                mappingPairs.Add((ushort)KeyInterop.VirtualKeyFromKey(Key.Down), (ushort)KeyboardKey.DownArrow);
                mappingPairs.Add((ushort)KeyInterop.VirtualKeyFromKey(Key.Left), (ushort)KeyboardKey.LeftArrow);
                mappingPairs.Add((ushort)KeyInterop.VirtualKeyFromKey(Key.Right), (ushort)KeyboardKey.RightArrow);
                mappingPairs.Add((ushort)KeyInterop.VirtualKeyFromKey(Key.Subtract), (ushort)KeyboardKey.Subtract);
                mappingPairs.Add((ushort)KeyInterop.VirtualKeyFromKey(Key.OemOpenBrackets), (ushort)KeyboardKey.OpenBrace);
                mappingPairs.Add((ushort)KeyInterop.VirtualKeyFromKey(Key.OemCloseBrackets), (ushort)KeyboardKey.CloseBrace);
                mappingPairs.Add((ushort)KeyInterop.VirtualKeyFromKey(Key.OemSemicolon), (ushort)KeyboardKey.Semicolon);
                mappingPairs.Add((ushort)KeyInterop.VirtualKeyFromKey(Key.OemQuotes), (ushort)KeyboardKey.Quote);
                mappingPairs.Add((ushort)KeyInterop.VirtualKeyFromKey(Key.OemComma), (ushort)KeyboardKey.Comma);
                mappingPairs.Add((ushort)KeyInterop.VirtualKeyFromKey(Key.OemPeriod), (ushort)KeyboardKey.Dot);
                mappingPairs.Add((ushort)KeyInterop.VirtualKeyFromKey(Key.OemBackslash), (ushort)KeyboardKey.Backslash);
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

                //mappingPairs.Add((ushort)KeyInterop.VirtualKeyFromKey(Key.Return), (ushort)KeyboardKey.KeypadEnter);
                //mappingPairs.Add((ushort)KeyInterop.VirtualKeyFromKey(Key.LeftCtrl), (ushort)0xE0);
                //mappingPairs.Add((ushort)KeyInterop.VirtualKeyFromKey(Key.RightCtrl), (ushort)0xE4);
            }
        }

        public override uint GetRealEventKey(uint winVkKey)
        {
            mappingPairs.TryGetValue((ushort)winVkKey, out ushort result);
            return result;
        }
    }
}
