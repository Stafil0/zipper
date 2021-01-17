using System.Collections;
using System.Collections.Generic;

namespace Zipper.Domain.Collections
{
    internal interface IQueue<T> : ICollection<T>
    {
        T Peek();

        bool TryPeek(out T item);
        
        T Dequeue();

        bool TryDequeue(out T item);
        
        bool TryEnqueue(T item);
    }
}