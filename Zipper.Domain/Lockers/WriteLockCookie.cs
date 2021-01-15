using System;
using System.Threading;

namespace Zipper.Domain.Lockers
{
    public struct WriteLockCookie : IDisposable
    {
        private readonly ReaderWriterLockSlim _lock;

        public WriteLockCookie(ReaderWriterLockSlim rwl)
        {
            (_lock = rwl)?.EnterWriteLock();
        }
        
        public void Dispose()
        {
            _lock?.ExitWriteLock();
        }
    }
}