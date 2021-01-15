using System;
using System.Collections.Generic;
using System.IO;
using Zipper.Domain.Data;
using Zipper.Domain.Pipeline;

namespace Zipper.Domain.BoundedBuffer.File
{
    public class FileStreamReader : IReader<Stream, IEnumerable<Blob>>
    {
        private readonly int _bufferSize;

        public IEnumerable<Blob> Read(Stream input)
        {
            var offset = 0;
            var buffer = new byte[_bufferSize];

            int read;
            while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
            {
                var blob = new byte[read];
                Array.Copy(buffer, blob, read);

                yield return new Blob { Offset = offset++, Buffer = blob };
            }
        }

        public FileStreamReader(int bufferSize)
        {
            _bufferSize = bufferSize;
        }
    }
}