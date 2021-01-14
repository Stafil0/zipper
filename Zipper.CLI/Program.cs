using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Zipper.Domain.Compression.GZip;
using Zipper.Domain.Data;
using Zipper.Domain.Pipeline.Compression;

namespace Zipper.CLI
{
    internal class Program
    {
        private static void GzipCompress(string input, string output)
        {
            using (var inputStream = new FileStream(input, FileMode.Open))
            using (var outputStream = new FileStream(output, FileMode.Create))
            using (var compressionStream = new GZipStream(outputStream, CompressionMode.Compress))
            {
                inputStream.CopyTo(compressionStream);
            }
        }
        
        private static void GzipDecompress(string input, string output)
        {
            using (var inputStream = new FileStream(input, FileMode.Open))
            using (var outputStream = new FileStream(output, FileMode.Create))
            using (var decompressionStream = new GZipStream(inputStream, CompressionMode.Decompress))
            {
                decompressionStream.CopyTo(outputStream);
            }
        }

        private static void Compress(string input, string output)
        {
            using (var inputStream = new FileStream(input, FileMode.Open))
            using (var outputStream = new FileStream(output, FileMode.Create))
            using (var compressor = new CompressionPipeline(16, 16))
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
            using (var compressor = new CompressionPipeline(16, 16))
            {
                compressor
                    .Reader(new StreamBlobReader(1024 * 1024))
                    .Writer(new StreamBlobWriter())
                    .Decompress(inputStream, outputStream, new GzipDecompressor(1024 * 1024));
            }
        }
        
        public static void Main(string[] args)
        {
            var sw = new Stopwatch();
            var completions = new List<long>();

            for (var i = 0; i < 100; i++)
            {
                sw.Restart();
                GzipCompress("input.file", "compressed.file");
                sw.Stop();
                
                completions.Add(sw.ElapsedMilliseconds);
                // Console.WriteLine($"Compressed by {sw.ElapsedMilliseconds} milliseconds");
            }
            
            Console.WriteLine($"Avg = {completions.Sum() / completions.Count} milliseconds");

            // sw.Restart();
            // Decompress("compressed.file", "decompressed.file");
            // sw.Stop();
            // Console.WriteLine($"Decompressed by {sw.ElapsedMilliseconds} milliseconds");
        }
    }
}