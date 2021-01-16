using System.IO;
using System.IO.Compression;
using Zipper.Domain.Exceptions;
using Zipper.Domain.Pipeline;

namespace Zipper.Domain.Compression
{
    public class GzipDecompressor : IConverter<byte[], byte[]>
    {
        public byte[] Convert(byte[] data)
        {
            if (data == null || data.Length == 0)
                return data;

            try
            {
                using var output = new MemoryStream();
                using (var input = new MemoryStream(data))
                using (var zip = new GZipStream(input, CompressionMode.Decompress))
                {
                    zip.CopyTo(output);
                }
                
                return output.ToArray();
            }
            catch (InvalidDataException e)
            {
                throw new InvalidCompressionFormatException(e);
            }
        }
    }
}