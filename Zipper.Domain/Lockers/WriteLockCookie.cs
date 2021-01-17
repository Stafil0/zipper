using System;
using System.Threading;

namespace Zipper.Domain.Lockers
{
    /// <summary>
    /// Write cookie for ReaderWriterLockSlim.
    /// </summary>
    public struct WriteLockCookie : IDisposable
    {
        /// <summary>
        /// ReaderWriterLockSlim instance.
        /// </summary>
        private readonly ReaderWriterLockSlim _lock;

        /// <summary>
        /// Create cookie and enter write lock.
        /// </summary>
        /// <param name="rwl">Instance of ReaderWriterLockSlim.</param>
        public WriteLockCookie(ReaderWriterLockSlim rwl)
        {
            (_lock = rwl)?.EnterWriteLock();
        }
        
        /// <summary>
        /// Release cookie and exit write lock.
        /// </summary>
        public void Dispose()
        {
            _lock?.ExitWriteLock();
        }
    }
}