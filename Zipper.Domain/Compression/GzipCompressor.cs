using System.IO;
using System.IO.Compression;
using Zipper.Domain.Pipeline;

namespace Zipper.Domain.Compression
{
    public class GzipCompressor : IConverter<byte[], byte[]>
    {
        public byte[] Convert(byte[] data)
        {
            if (data == null || data.Length == 0)
                return data;

            using var output = new MemoryStream();
            using (var zip = new GZipStream(output, CompressionMode.Compress))
                zip.Write(data, 0, data.Length);

            return output.ToArray();
        }
    }
}