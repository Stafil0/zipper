using System;
using System.Collections.Generic;
using Zipper.Domain.Data;
using Zipper.Domain.Pipeline;

namespace Zipper.Domain.BoundedBuffer.Blobs
{
    public class BlobReader : IReader<System.IO.Stream, IEnumerable<Blob>>
    {
        public IEnumerable<Blob> Read(System.IO.Stream input)
        {
            var offset = 0;
            while (TryRead(input, out var buffer))
            {
                yield return new Blob { Offset = offset++, Buffer = buffer };
            }
        }

        private static bool TryRead(System.IO.Stream stream, out byte[] data)
        {
            data = default;

            const int length = sizeof(int);
            var buffer = new byte[length];

            if (stream.Read(buffer, 0, buffer.Length) == 0)
                return false;

            var size = BitConverter.ToInt32(buffer, 0);
            buffer = new byte[size];

            if (stream.Read(buffer, 0, buffer.Length) == 0)
                return false;

            data = buffer;
            return true;
        }
    }
}