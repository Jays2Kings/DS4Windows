/*
DS4Windows
Copyright (C) 2023  Travis Nickles

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/

using DS4Windows;
using DS4WinWPF.DS4Forms;
using System;
using System.Runtime.InteropServices;
using System.Security;
using System.Windows.Interop;

namespace DS4WinWPF.DS4Control
{
    [SuppressUnmanagedCodeSecurity]
    internal static class WindowPlacementHelper
    {
        #region Interop

        /// <summary>
        /// The RECT structure defines the coordinates of the upper-left and lower-right corners of a rectangle.
        /// See: https://www.pinvoke.net/default.aspx/Structures/RECT.html
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left, Top, Right, Bottom;

            public RECT(int left, int top, int right, int bottom)
            {
                Left = left;
                Top = top;
                Right = right;
                Bottom = bottom;
            }

            public override readonly string ToString()
            {
                return string.Format(System.Globalization.CultureInfo.CurrentCulture, "{{Left={0},Top={1},Right={2},Bottom={3}}}", Left, Top, Right, Bottom);
            }
        }

        /// <summary>
        /// The POINT structure defines the x- and y-coordinates of a point.
        /// See: https://www.pinvoke.net/default.aspx/Structures/POINT.html
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int X;
            public int Y;

            public POINT(int x, int y)
            {
                X = x;
                Y = y;
            }

            public override readonly string ToString()
            {
                return $"X: {X}, Y: {Y}";
            }
        }

        /// <summary>
        /// Contains information about the placement of a window on the screen.
        /// </summary>
        [Serializable]
        [StructLayout(LayoutKind.Sequential)]
        internal struct WINDOWPLACEMENT
        {
            /// <summary>
            /// The length of the structure, in bytes. Before calling the SetWindowPlacement, set this member to sizeof(WINDOWPLACEMENT).
            /// </summary>
            public int Length;

            /// <summary>
            /// Specifies flags that control the position of the minimized window and the method by which the window is restored.
            /// </summary>
            public int Flags;

            /// <summary>
            /// The current show state of the window.
            /// </summary>
            public int ShowCmd;

            /// <summary>
            /// The coordinates of the window's upper-left corner when the window is minimized.
            /// </summary>
            public POINT MinPosition;

            /// <summary>
            /// The coordinates of the window's upper-left corner when the window is maximized.
            /// </summary>
            public POINT MaxPosition;

            /// <summary>
            /// The window's coordinates when the window is in the restored position.
            /// </summary>
            public RECT NormalPosition;
        }

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetWindowPlacement(IntPtr hWnd, [In] ref WINDOWPLACEMENT lpwndpl);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetWindowPlacement(IntPtr hWnd, out WINDOWPLACEMENT lpwndpl);

        private const int SW_SHOWNORMAL = 1;
        private const int SW_SHOWMINIMIZED = 2;

        #endregion

        private static IntPtr mainWindowHandle = IntPtr.Zero;

        internal static void ApplyPlacement(MainWindow mainWindow, bool startMinimized)
        {
            if(mainWindowHandle == IntPtr.Zero)
            {
                mainWindowHandle = new WindowInteropHelper(mainWindow).Handle;
            }

            var placement = new WINDOWPLACEMENT()
            {
                Flags = 0,
                Length = Marshal.SizeOf(typeof(WINDOWPLACEMENT)),
                ShowCmd = (startMinimized ? SW_SHOWMINIMIZED : SW_SHOWNORMAL),
                MaxPosition = new POINT(-1, -1),
                MinPosition = new POINT(-1, -1),
                NormalPosition = new RECT(Global.FormLocationX, Global.FormLocationY, 
                    Global.FormLocationX + Global.FormWidth, Global.FormLocationY + Global.FormHeight)
            };

            try
            {
                SetWindowPlacement(mainWindowHandle, ref placement);
            }
            catch(Exception ex)
            {
                AppLogger.LogToGui($"Failed to apply window placement: {ex.Message}", true);
            }
        }

        internal static RECT GetPlacement(MainWindow mainWindow)
        {
            if (mainWindowHandle == IntPtr.Zero)
            {
                mainWindowHandle = new WindowInteropHelper(mainWindow).Handle;
            }

            GetWindowPlacement(mainWindowHandle, out var placement);
            return placement.NormalPosition;
        }
    }
}