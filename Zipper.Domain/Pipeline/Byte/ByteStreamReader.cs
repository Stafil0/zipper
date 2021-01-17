using System;
using System.Collections.Generic;

namespace Zipper.Domain.Pipeline.Byte
{
    /// <summary>
    /// Byte-batch stream reader.
    /// </summary>
    public class ByteStreamReader : IReader<System.IO.Stream, IEnumerable<byte[]>>
    {
        /// <summary>
        /// Buffer size for read operations.
        /// </summary>
        private readonly int _bufferSize;

        /// <summary>
        /// Read bytes from stream by batches.
        /// </summary>
        /// <param name="input">Stream.</param>
        /// <returns>Collection of bytes array.</returns>
        public IEnumerable<byte[]> Read(System.IO.Stream input)
        {
            var buffer = new byte[_bufferSize];

            int read;
            while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
            {
                var blob = new byte[read];
                Array.Copy(buffer, blob, read);

                yield return blob;
            }
        }

        /// <summary>
        /// Initialize new instance of stream reader.
        /// </summary>
        /// <param name="bufferSize">Buffer size for read operations.</param>
        /// <exception cref="ArgumentException">Throws exception if buffer size less than zero.</exception>
        public ByteStreamReader(int bufferSize)
        {
            if (bufferSize < 0)
                throw new ArgumentException("Buffer size can't be less than 0.");
            
            _bufferSize = bufferSize;
        }
    }
}