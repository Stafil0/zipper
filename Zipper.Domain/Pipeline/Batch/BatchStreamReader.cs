using System;
using System.Collections.Generic;
using System.Linq;
using Zipper.Domain.Exceptions;

namespace Zipper.Domain.Pipeline.Batch
{
    public class BatchStreamReader :
        BatchStreamBase,
        IReader<System.IO.Stream, IEnumerable<byte[]>>
    {
        public IEnumerable<byte[]> Read(System.IO.Stream input)
        {
            while (TryRead(input, out var buffer))
            {
                yield return buffer;
            }
        }

        private static bool TryRead(System.IO.Stream stream, out byte[] data)
        {
            data = default;
            
            var buffer = new byte[MagicLength];
            if (stream.Read(buffer, 0, buffer.Length) == 0)
                return false;

            if (!MagicData.SequenceEqual(buffer))
                throw new InvalidFormatException();

            const int length = sizeof(long);
            buffer = new byte[length];
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