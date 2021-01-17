using System.IO;
using System.IO.Compression;

namespace Zipper.Domain.Pipeline.GZip
{
    /// <summary>
    /// GZip data compressor.
    /// </summary>
    public class GZipBatchCompressor : IConverter<byte[], byte[]>
    {
        /// <summary>
        /// Compresses byte array using gzip.
        /// </summary>
        /// <param name="data">Byte array.</param>
        /// <returns>Compressed data.</returns>
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