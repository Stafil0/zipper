using System.IO;
using Zipper.Domain.Compression.Blob;
using Zipper.Domain.Compression.GZip;
using Zipper.Domain.Pipeline.Compression;

namespace Zipper.CLI
{
    internal class Program
    {
        private static void Compress(string input, string output)
        {
            using (var inputStream = new FileStream(input, FileMode.Open))
            using (var outputStream = new FileStream(output, FileMode.Create))
            using (var compressor = new CompressionPipeline(4, 16))
            {
                compressor
                    .Reader(new StreamBlobReader(1024 * 1024))
                    .Writer(new StreamBlobWriter())
                    .Compress(inputStream, outputStream, new GzipCompressor(1024 * 1024));
            }
        }

        private static void Decompress(string input, string output)
        {
            using (var inputStream = new FileStream(input, FileMode.Open))
            using (var outputStream = new FileStream(output, FileMode.Create))
            using (var compressor = new CompressionPipeline(4, 16))
            {
                compressor
                    .Reader(new StreamBlobReader(1024 * 1024))
                    .Writer(new StreamBlobWriter())
                    .Decompress(inputStream, outputStream, new GzipDecompressor(1024 * 1024));
            }
        }
        
        public static void Main(string[] args)
        {
            Compress("input.file", "compressed.file");
            Decompress("compressed.file", "decompressed.file");
        }
    }
}