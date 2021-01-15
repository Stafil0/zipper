using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Zipper.Domain.Lockers;

namespace Zipper.Domain.Collections
{
    internal class BlockingPriorityQueue<T> : IQueue<T>
    {
        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
        private readonly SortedSet<T> _set;
        
        public IEnumerator<T> GetEnumerator() => _set.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public T Peek()
        {
            using (new ReadLockCookie(_lock))
                return PeekItem();
        }

        public bool TryPeek(out T item)
        {
            item = default;
            using (new ReadLockCookie(_lock))
            {
                if (_set.Count == 0)
                    return false;

                item = PeekItem();
                return true;
            }
        }

        private T PeekItem() => _set.Max;

        public T Dequeue()
        {
            using (new WriteLockCookie(_lock))
                return DequeueItem();
        }

        public bool TryDequeue(out T item)
        {
            item = default;
            using (new WriteLockCookie(_lock))
            {
                if (_set.Count == 0)
                    return false;

                item = DequeueItem();
                return true;
            }
        }

        private T DequeueItem()
        {
            var top = _set.Max;
            _set.Remove(top);

            return top;
        }
        
        public bool TryEnqueue(T item)
        {
            using (new WriteLockCookie(_lock))
                return _set.Add(item);
        }

        public BlockingPriorityQueue(IComparer<T> comparer)
        {
            _set = new SortedSet<T>(comparer);
        }

        public BlockingPriorityQueue()
        {
            _set = new SortedSet<T>();
        }
    }
}