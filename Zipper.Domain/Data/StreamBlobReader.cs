using System.Collections.Generic;
using System.IO;
using Zipper.Domain.Pipeline;

namespace Zipper.Domain.Data
{
    public class StreamBlobReader : IReader<Stream, IEnumerable<Blob>>
    {
        private readonly int _bufferSize;

        public IEnumerable<Blob> Read(Stream input)
        {
            var offset = 0;
            var buffer = new byte[_bufferSize];

            while (input.Read(buffer, 0, buffer.Length) > 0)
            {
                var blobBuffer = new byte[_bufferSize];
                yield return new Blob { Offset = offset++, Buffer = blobBuffer };
            }
        }

        public StreamBlobReader(int bufferSize)
        {
            _bufferSize = bufferSize;
        }
    }
}