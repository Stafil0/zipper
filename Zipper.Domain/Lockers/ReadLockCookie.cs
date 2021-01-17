using System;
using System.Threading;

namespace Zipper.Domain.Lockers
{
    /// <summary>
    /// Read cookie for ReaderWriterLockSlim.
    /// </summary>
    public struct ReadLockCookie : IDisposable
    {
        /// <summary>
        /// ReaderWriterLockSlim instance.
        /// </summary>
        private readonly ReaderWriterLockSlim _lock;

        /// <summary>
        /// Create cookie and enter read lock.
        /// </summary>
        /// <param name="rwl">Instance of ReaderWriterLockSlim.</param>
        public ReadLockCookie(ReaderWriterLockSlim rwl)
        {
            (_lock = rwl)?.EnterReadLock();
        }

        /// <summary>
        /// Release cookie and exit read lock.
        /// </summary>
        public void Dispose()
        {
            _lock?.ExitReadLock();
        }
    }
}