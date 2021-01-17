using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Zipper.Domain.Lockers;

namespace Zipper.Domain.Collections
{
    /// <summary>
    /// Priority queue (heap) with read-write lock.
    /// </summary>
    /// <typeparam name="T">Type of objects, stored in heap.</typeparam>
    internal class BlockingPriorityQueue<T> : IQueue<T>
    {
        /// <summary>
        /// Read-write lock.
        /// </summary>
        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
        
        /// <summary>
        /// Objects storage.
        /// </summary>
        private readonly SortedSet<T> _set;

        /// <inheritdoc />
        public int Count => _set.Count;

        /// <inheritdoc />
        public bool IsReadOnly => false;
        
        /// <inheritdoc />
        public IEnumerator<T> GetEnumerator() => _set.GetEnumerator();

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <inheritdoc />
        public T Peek()
        {
            using (new ReadLockCookie(_lock))
                return PeekItem();
        }

        /// <inheritdoc />
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

        /// <summary>
        /// Get top-priority element from heap.
        /// </summary>
        /// <returns>Top element.</returns>
        /// <exception cref="InvalidOperationException">Throws exception if queue empty.</exception>
        private T PeekItem()
        {
            if (!_set.Any())
                throw new InvalidOperationException("Queue is empty.");

            return _set.Min;
        }

        /// <inhertidoc />
        public T Dequeue()
        {
            using (new WriteLockCookie(_lock))
                return DequeueItem();
        }

        /// <inhertidoc />
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

        /// <summary>
        /// Dequeue top-priority element from heap.
        /// </summary>
        /// <returns>Top-priority element.</returns>
        private T DequeueItem()
        {
            var top = PeekItem();
            _set.Remove(top);

            return top;
        }

        /// <inhertidoc />
        public void Add(T item) => TryEnqueue(item);

        /// <inhertidoc />
        public bool TryEnqueue(T item)
        {
            using (new WriteLockCookie(_lock))
                return _set.Add(item);
        }

        /// <inheritdoc />
        public void Clear()
        {
            using (new WriteLockCookie(_lock))
                _set.Clear();
        }

        /// <inheritdoc />
        public bool Contains(T item)
        {
            using (new ReadLockCookie(_lock))
                return _set.Contains(item ?? throw new ArgumentNullException(nameof(item)));
        }

        /// <inheritdoc />
        public void CopyTo(T[] array, int arrayIndex)
        {
            using (new ReadLockCookie(_lock))
                _set.CopyTo(array, arrayIndex);
        }
        
        /// <inhertidoc />
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

        /// <summary>
        /// Initialize heap with custom comparer.
        /// </summary>
        /// <param name="comparer">Comparer.</param>
        public BlockingPriorityQueue(IComparer<T> comparer)
        {
            _set = new SortedSet<T>(comparer);
        }

        /// <summary>
        /// Initialize heap.
        /// </summary>
        public BlockingPriorityQueue()
        {
            _set = new SortedSet<T>();
        }
    }
}