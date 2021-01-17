using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Zipper.Domain.Lockers;

namespace Zipper.Domain.Collections
{
    internal class BlockingPriorityQueue<T> : IQueue<T>
    {
        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
        
        private readonly SortedSet<T> _set;

        public int Count => _set.Count;

        public bool IsReadOnly => false;
        
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

        private T PeekItem()
        {
            if (!_set.Any())
                throw new InvalidOperationException("Queue is empty.");

            return _set.Min;
        }

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
            var top = PeekItem();
            _set.Remove(top);

            return top;
        }

        public void Add(T item) => TryEnqueue(item);


        public bool TryEnqueue(T item)
        {
            using (new WriteLockCookie(_lock))
                return _set.Add(item);
        }

        public void Clear()
        {
            using (new WriteLockCookie(_lock))
                _set.Clear();
        }

        public bool Contains(T item)
        {
            using (new ReadLockCookie(_lock))
                return _set.Contains(item ?? throw new ArgumentNullException(nameof(item)));
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            using (new ReadLockCookie(_lock))
                _set.CopyTo(array, arrayIndex);
        }

        public bool Remove(T item)
        {
            if (item == null)
                return false;

            using (new WriteLockCookie(_lock))
            {
                var top = PeekItem();
                if (!item.Equals(top))
                    return false;

                DequeueItem();
                return true;
            }
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