using System;
using System.Collections.Generic;
using System.Linq;
using Zipper.Domain.Exceptions;

namespace Zipper.Domain.Pipeline.GZip
{
    /// <summary>
    /// Reader for gzipped data, stored in batches.
    /// </summary>
    public class GZipBatchReader :
        GZipBatchStreamBase,
        IReader<System.IO.Stream, IEnumerable<byte[]>>
    {
        /// <summary>
        /// Read gzip batches from stream.
        /// </summary>
        /// <param name="input">Stream.</param>
        /// <returns>Collection of gzipped batches (without magic).</returns>
        public IEnumerable<byte[]> Read(System.IO.Stream input)
        {
            while (TryRead(input, out var buffer))
            {
                yield return buffer;
            }
        }

        /// <summary>
        /// Try read gzip batches from stream.
        /// </summary>
        /// <param name="stream">Stream.</param>
        /// <param name="data">GZip batch.</param>
        /// <returns>True, if read successful, else false.</returns>
        /// <exception cref="InvalidFormatException">Throws exception if stream not contains magic data.</exception>
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