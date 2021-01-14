using System.IO;
using System.IO.Compression;

namespace Zipper.Domain.Compression.GZip
{
    public class GzipCompressor : ICompressor
    {
        private readonly int _bufferSize;

        public byte[] Compress(byte[] data)
        {
            if (data == null || data.Length == 0)
                return data;

            using (var output = new MemoryStream())
            {
                using (var buf = new BufferedStream(new GZipStream(output, CompressionMode.Compress), _bufferSize))
                {
                    buf.Write(data, 0, data.Length);
                }

                return output.ToArray();
            }
        }

        public GzipCompressor(int bufferSize)
        {
            _bufferSize = bufferSize;
        }
    }
}