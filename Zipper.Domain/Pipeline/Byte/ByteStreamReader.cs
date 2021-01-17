using System;
using System.Collections.Generic;

namespace Zipper.Domain.Pipeline.Byte
{
    public class ByteStreamReader : IReader<System.IO.Stream, IEnumerable<byte[]>>
    {
        private readonly int _bufferSize;

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

        public ByteStreamReader(int bufferSize)
        {
            if (bufferSize < 0)
                throw new ArgumentException("Buffer size can't be less, than 0.");
            
            _bufferSize = bufferSize;
        }
    }
}