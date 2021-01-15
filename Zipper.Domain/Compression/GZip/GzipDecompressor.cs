using System;
using System.IO;
using System.IO.Compression;

namespace Zipper.Domain.Compression.GZip
{
    public class GzipDecompressor : IDecompressor
    {
        public byte[] Decompress(byte[] data)
        {
            if (data == null || data.Length == 0)
                return data;

            using (var output = new MemoryStream())
            {
                using (var input = new MemoryStream(data))
                using (var zip = new GZipStream(input, CompressionMode.Decompress))
                {
                    zip.CopyTo(output);
                }

                return output.ToArray();
            }
        }
    }
}