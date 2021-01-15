using System.IO;
using System.IO.Compression;

namespace Zipper.Domain.Compression.GZip
{
    public class GzipCompressor : ICompressor
    {
        public byte[] Compress(byte[] data)
        {
            if (data == null || data.Length == 0)
                return data;

            using (var output = new MemoryStream())
            {
                using (var zip = new GZipStream(output, CompressionMode.Compress))
                    zip.Write(data, 0, data.Length);

                return output.ToArray();
            }
        }
    }
}