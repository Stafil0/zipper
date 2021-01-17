using System.IO;
using System.IO.Compression;
using Zipper.Domain.Exceptions;

namespace Zipper.Domain.Pipeline.GZip
{
    public class GZipBatchDecompressor : IConverter<byte[], byte[]>
    {
        public byte[] Convert(byte[] data)
        {
            if (data == null || data.Length == 0)
                return data;

            try
            {
                using var input = new MemoryStream(data);
                using var output = new MemoryStream();
                using var zip = new GZipStream(input, CompressionMode.Decompress);

                zip.CopyTo(output);

                return output.ToArray();
            }
            catch (InvalidDataException e)
            {
                throw new InvalidFormatException(e);
            }
        }
    }
}