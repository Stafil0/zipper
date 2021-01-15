using System.Collections.Generic;

namespace Zipper.Domain.Collections
{
    public interface IQueue<T> : IEnumerable<T> 
    {
        T Peek();

        bool TryPeek(out T item);
        
        T Dequeue();

        bool TryDequeue(out T item);
        
        bool TryEnqueue(T item);
    }
}