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