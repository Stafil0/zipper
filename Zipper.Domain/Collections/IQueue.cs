using System.Collections.Generic;

namespace Zipper.Domain.Collections
{
    /// <summary>
    /// Queue.
    /// </summary>
    /// <typeparam name="T">Type of objects, stored in queue</typeparam>
    internal interface IQueue<T> : ICollection<T>
    {
        /// <summary>
        /// Get first element from queue.
        /// </summary>
        /// <returns>First element in queue.</returns>
        T Peek();

        /// <summary>
        /// Try get first element from queue.
        /// </summary>
        /// <param name="item">First element in queue, else default.</param>
        /// <returns>True, peek is successful, else - false.</returns>
        bool TryPeek(out T item);
        
        /// <summary>
        /// Get and remove first element from queue.
        /// </summary>
        /// <returns>First element in queue.</returns>
        T Dequeue();

        /// <summary>
        /// Try get and remove first element from queue.
        /// </summary>
        /// <param name="item">First element in queue, else default.</param>
        /// <returns>True, dequeue is successful, else - false.</returns>
        bool TryDequeue(out T item);
        
        /// <summary>
        /// Add new element in queue.
        /// </summary>
        /// <param name="item">New element.</param>
        /// <returns>True, added successfully, else - false.</returns>
        bool TryEnqueue(T item);
    }
}