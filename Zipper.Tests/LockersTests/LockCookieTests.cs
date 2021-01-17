using System.Threading;
using Xunit;
using Zipper.Domain.Lockers;

namespace Zipper.Tests.LockersTests
{
    public class ReadLockCookieTests
    {
        [Fact]
        public void CookieCreated_ReadLockAcquired_UnlockOnDispose()
        {
            var rwl = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
            Assert.False(rwl.IsReadLockHeld);

            using (new ReadLockCookie(rwl))
            {
                Assert.True(rwl.IsReadLockHeld);
            }

            Assert.False(rwl.IsReadLockHeld);
        }
        
        [Fact]
        public void CookieCreated_ReadLockAcquired_TryLockAgain_ThrowLockRecursionException_UnlockOnDispose()
        {
            var rwl = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
            Assert.False(rwl.IsReadLockHeld);

            using (new ReadLockCookie(rwl))
            {
                Assert.True(rwl.IsReadLockHeld);
                Assert.Throws<LockRecursionException>( () => rwl.EnterReadLock());
            }

            Assert.False(rwl.IsReadLockHeld);
        }

        [Fact]
        public void CookieCreated_RecursionReadLockAcquired_TryReadLock_ReadersIncreased_LockedOnDispose()
        {
            var rwl = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
            Assert.False(rwl.IsReadLockHeld);

            using (new ReadLockCookie(rwl))
            {
                Assert.True(rwl.IsReadLockHeld);
                rwl.EnterReadLock();

                Assert.Equal(2, rwl.RecursiveReadCount);
            }

            Assert.True(rwl.IsReadLockHeld);
        }

        [Fact]
        public void CookieCreated_RecursionReadLockAcquired_TryWriteLock_ThrowLockRecursionException_UnlockOnDispose()
        {
            var rwl = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
            Assert.False(rwl.IsReadLockHeld);

            using (new ReadLockCookie(rwl))
            {
                Assert.True(rwl.IsReadLockHeld);
                Assert.Throws<LockRecursionException>(() => rwl.EnterWriteLock());
            }

            Assert.False(rwl.IsReadLockHeld);
        }

        [Fact]
        public void CookieCreated_RecursionReadLockAcquired_TryUpgradeReadLock_ThrowLockRecursionException_UnlockOnDispose()
        {
            var rwl = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
            Assert.False(rwl.IsReadLockHeld);

            using (new ReadLockCookie(rwl))
            {
                Assert.True(rwl.IsReadLockHeld);
                Assert.Throws<LockRecursionException>(() => rwl.EnterUpgradeableReadLock());
            }

            Assert.False(rwl.IsReadLockHeld);
        }
    }
    
    public class WriteLockCookieTests
    {
        [Fact]
        public void CookieCreated_WriteLockAcquired_UnlockOnDispose()
        {
            var rwl = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
            Assert.False(rwl.IsWriteLockHeld);

            using (new WriteLockCookie(rwl))
            {
                Assert.True(rwl.IsWriteLockHeld);
            }

            Assert.False(rwl.IsWriteLockHeld);
        }
        
        [Fact]
        public void CookieCreated_WriteLockAcquired_TryLockAgain_ThrowLockRecursionException_UnlockOnDispose()
        {
            var rwl = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
            Assert.False(rwl.IsWriteLockHeld);

            using (new WriteLockCookie(rwl))
            {
                Assert.True(rwl.IsWriteLockHeld);
                Assert.Throws<LockRecursionException>( () => rwl.EnterWriteLock());
            }

            Assert.False(rwl.IsWriteLockHeld);
        }

        [Fact]
        public void CookieCreated_RecursionWriteLockAcquired_TryLockAgain_WritersCountIncreased_UnlockOnDispose()
        {
            var rwl = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
            Assert.False(rwl.IsWriteLockHeld);

            using (new WriteLockCookie(rwl))
            {
                Assert.True(rwl.IsWriteLockHeld);
                rwl.EnterWriteLock();
                
                Assert.Equal(2, rwl.RecursiveWriteCount);
            }

            Assert.True(rwl.IsWriteLockHeld);
        }

        [Fact]
        public void CookieCreated_RecursionWriteLockAcquired_TryReadLock_ReadersCountIncreased_UnlockOnDispose()
        {
            var rwl = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
            Assert.False(rwl.IsWriteLockHeld);

            using (new WriteLockCookie(rwl))
            {
                Assert.True(rwl.IsWriteLockHeld);
                rwl.EnterReadLock();
                
                Assert.Equal(1, rwl.RecursiveWriteCount);
                Assert.Equal(1, rwl.RecursiveReadCount);
            }

            Assert.False(rwl.IsWriteLockHeld);
        }

        [Fact]
        public void CookieCreated_RecursionWriteLockAcquired_TryUpgradeReadLock_UpgradeableReadersIncreased_UnlockOnDispose()
        {
            var rwl = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
            Assert.False(rwl.IsWriteLockHeld);

            using (new WriteLockCookie(rwl))
            {
                Assert.True(rwl.IsWriteLockHeld);
                rwl.EnterUpgradeableReadLock();
                
                Assert.Equal(1, rwl.RecursiveWriteCount);
                Assert.Equal(1, rwl.RecursiveUpgradeCount);
            }

            Assert.False(rwl.IsWriteLockHeld);
        }
    }
}