using System;
using System.Collections.Generic;

namespace Zipper.Domain.Pipeline.File
{
    public class FileStreamReader : IReader<System.IO.Stream, IEnumerable<byte[]>>
    {
        private readonly int _bufferSize;

        public IEnumerable<byte[]> Read(System.IO.Stream input)
        {
            var offset = 0;
            var buffer = new byte[_bufferSize];

            int read;
            while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
            {
                var blob = new byte[read];
                Array.Copy(buffer, blob, read);

                yield return blob;
            }
        }

        public FileStreamReader(int bufferSize)
        {
            _bufferSize = bufferSize;
        }
    }
}