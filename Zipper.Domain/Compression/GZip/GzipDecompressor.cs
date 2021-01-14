using System;
using System.IO;
using System.IO.Compression;

namespace Zipper.Domain.Compression.GZip
{
    public class GzipDecompressor : IDecompressor
    {
        private readonly int _bufferSize;

        public byte[] Decompress(byte[] data)
        {
            if (data == null || data.Length == 0)
                return data;

            var buf = new byte[_bufferSize];
            using (var output = new MemoryStream())
            {
                using (var input = new MemoryStream(data))
                {
                    using (var gzs = new BufferedStream(new GZipStream(input, CompressionMode.Decompress), buf.Length))
                    {
                        int count;
                        while ((count = gzs.Read(buf, 0, buf.Length)) != 0)
                        {
                            Array.Resize(ref buf, count);
                            output.Write(buf, 0, count);
                        }
                    }
                }

                return output.ToArray();
            }
        }

        public GzipDecompressor(int bufferSize)
        {
            _bufferSize = bufferSize;
        }
    }
}