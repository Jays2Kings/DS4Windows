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
using System.Threading;

namespace DS4Windows
{
    public class ReadLocker : IDisposable
    {
        private ReaderWriterLockSlim _lockerInstance;

        public ReadLocker(ReaderWriterLockSlim lockerInstance)
        {
            _lockerInstance = lockerInstance;
            _lockerInstance.EnterReadLock();
        }

        public void Dispose()
        {
            _lockerInstance.ExitReadLock();
            _lockerInstance = null;
        }
    }

    public class WriteLocker : IDisposable
    {
        private ReaderWriterLockSlim _lockerInstance;
        private bool IsDisposed => _lockerInstance == null;

        public WriteLocker(ReaderWriterLockSlim lockerInstance)
        {
            _lockerInstance = lockerInstance;
            _lockerInstance.EnterWriteLock();
        }

        public void Dispose()
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(this.ToString());
            }

            _lockerInstance.ExitWriteLock();
            _lockerInstance = null;
        }
    }
}