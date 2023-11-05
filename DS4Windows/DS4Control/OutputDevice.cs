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
    public abstract class OutputDevice
    {
        protected bool connected;

        public abstract void ConvertandSendReport(DS4State state, int device);
        public abstract void Connect();
        public abstract void Disconnect();
        public abstract void ResetState(bool submit=true);
        public abstract string GetDeviceType();

        public abstract void RemoveFeedbacks();

        public abstract void RemoveFeedback(int inIdx);
    }
}
