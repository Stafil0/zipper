using System;
using Xunit;
using Zipper.Domain.Collections;

namespace Zipper.Tests.CollectionsTests
{
    public class BlockingPriorityQueueTests
    {
        [Fact]
        public void Peek_QueueEmpty_ThrowsException()
        {
            var queue = new BlockingPriorityQueue<int>();
            Assert.Throws<InvalidOperationException>(() => queue.Peek());
        }

        [Fact]
        public void Peek_QueueNotEmpty_GetTop()
        {
            var queue = new BlockingPriorityQueue<int>();
            var value1 = 69;
            var value2 = 42;

            queue.TryEnqueue(value1);
            queue.TryEnqueue(value2);
            
            Assert.Equal(value2, queue.Peek());
        }

        [Fact]
        public void TryPeek_QueueEmpty_ReturnsFalse()
        {
            var queue = new BlockingPriorityQueue<int>();
            Assert.False(queue.TryPeek(out _));
        }

        [Fact]
        public void TryPeek_QueueNotEmpty_GetTop()
        {
            var queue = new BlockingPriorityQueue<int>();
            var value1 = 69;
            var value2 = 42;

            queue.TryEnqueue(value1);
            queue.TryEnqueue(value2);
            
            Assert.True(queue.TryPeek(out var top));
            Assert.Equal(value2, top);
        }

        [Fact]
        public void Dequeue_QueueEmpty_ThrowsException()
        {
            var queue = new BlockingPriorityQueue<int>();
            Assert.Throws<InvalidOperationException>(() => queue.Dequeue());
        }
        
        [Fact]
        public void Dequeue_QueueNotEmpty_DequeueTop()
        {
            var queue = new BlockingPriorityQueue<int>();
            var value1 = 69;
            var value2 = 42;
            
            queue.TryEnqueue(value1);
            queue.TryEnqueue(value2);
            
            Assert.Equal(value2, queue.Dequeue());
            foreach (var value in queue)
                Assert.NotEqual(value2, value);
        }

        [Fact]
        public void TryDequeue_QueueEmpty_ReturnsFalse()
        {
            var queue = new BlockingPriorityQueue<int>();
            Assert.False(queue.TryDequeue(out _));
        }

        [Fact]
        public void TryDequeue_QueueNotEmpty_DequeueTop()
        {
            var queue = new BlockingPriorityQueue<int>();
            var value1 = 69;
            var value2 = 42;
            
            queue.TryEnqueue(value1);
            queue.TryEnqueue(value2);
            
            Assert.True(queue.TryDequeue(out var top));
            Assert.Equal(value2, top);
            foreach (var value in queue)
                Assert.NotEqual(value2, value);
        }

        [Fact]
        public void TryEnqueue_QueueEmpty_EnqueuedNew()
        {
            var queue = new BlockingPriorityQueue<int>();
            var value1 = 42;
            Assert.True(queue.TryEnqueue(value1));
            Assert.Equal(value1, queue.Peek());
        }
        
        [Fact]
        public void TryEnqueue_QueueNotEmpty_EnqueueInOrder()
        {
            var queue = new BlockingPriorityQueue<int>();
            var values = new [] { 42, 69 };

            queue.TryEnqueue(values[1]);
            queue.TryEnqueue(values[0]);

            var index = 0;
            foreach (var value in queue)
                Assert.Equal(values[index++], value);
        }

        [Fact]
        public void TryEnqueue_QueueHasValue_ReturnsFalse()
        {
            var queue = new BlockingPriorityQueue<int>();
            var value1 = 42;
            Assert.True(queue.TryEnqueue(value1));
            Assert.False(queue.TryEnqueue(value1));
        }
    }
}