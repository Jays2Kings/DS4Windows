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

using System;

namespace DS4Windows
{
    interface ITouchpadBehaviour
    {
        void touchesBegan(DS4Touchpad sender, TouchpadEventArgs arg);
        void touchesMoved(DS4Touchpad sender, TouchpadEventArgs arg);
        void touchButtonUp(DS4Touchpad sender, TouchpadEventArgs arg);
        void touchButtonDown(DS4Touchpad sender, TouchpadEventArgs arg);
        void touchesEnded(DS4Touchpad sender, TouchpadEventArgs arg);
        void sixaxisMoved(DS4SixAxis sender, SixAxisEventArgs unused);
        void touchUnchanged(DS4Touchpad sender, EventArgs unused);
    }
}
