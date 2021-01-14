using System.Collections.Generic;
using System.IO;
using Zipper.Domain.Pipeline;

namespace Zipper.Domain.Compression.Blob
{
    public class StreamBlobReader : IReader<Stream, IEnumerable<Data.Blob>>
    {
        private readonly int _bufferSize;

        public IEnumerable<Data.Blob> Read(Stream input)
        {
            var offset = 0;
            var buffer = new byte[_bufferSize];

            while (input.Read(buffer, 0, buffer.Length) > 0)
            {
                var blobBuffer = new byte[_bufferSize];
                yield return new Data.Blob { Offset = offset++, Buffer = blobBuffer };
            }
        }

        public StreamBlobReader(int bufferSize)
        {
            _bufferSize = bufferSize;
        }
    }
}