﻿using System;
using System.Threading;

namespace Zipper.Domain.Lockers
{
    public struct ReadLockCookie : IDisposable
    {
        private readonly ReaderWriterLockSlim _lock;

        public ReadLockCookie(ReaderWriterLockSlim rwl)
        {
            (_lock = rwl)?.EnterReadLock();
        }

        public void Dispose()
        {
            _lock?.ExitReadLock();
        }
    }
}