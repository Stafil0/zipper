﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Zipper.Domain.BoundedBuffer.Blobs;
using Zipper.Domain.BoundedBuffer.File;
using Zipper.Domain.Compression.GZip;
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
                    .Reader(new FileStreamReader(1024 * 1024))
                    .Writer(new BlobWriter())
                    .Compress(inputStream, outputStream, new GzipCompressor());
            }
        }

        private static void Decompress(string input, string output)
        {
            using (var inputStream = new FileStream(input, FileMode.Open))
            using (var outputStream = new FileStream(output, FileMode.Create))
            using (var compressor = new CompressionPipeline(16, 16))
            {
                compressor
                    .Reader(new BlobReader())
                    .Writer(new FileStreamWriter())
                    .Decompress(inputStream, outputStream, new GzipDecompressor());
            }
        }
        
        public static void Main(string[] args)
        {
            var sw = new Stopwatch();

            // var completions = new List<long>();
            // for (var i = 0; i < 100; i++)
            // {
            //     sw.Restart();
            //     Compress("input.file", "compressed.file");
            //     sw.Stop();
            //
            //     completions.Add(sw.ElapsedMilliseconds);
            // }
            // Console.WriteLine($"Compress avg = {completions.Sum() / completions.Count} milliseconds");
            //
            // completions = new List<long>();
            // for (var i = 0; i < 100; i++)
            // {
            //     sw.Restart();
            //     Decompress("compressed.file", "decompressed.file");
            //     sw.Stop();
            //
            //     completions.Add(sw.ElapsedMilliseconds);
            // }
            // Console.WriteLine($"Decompress avg = {completions.Sum() / completions.Count} milliseconds");
            
            sw.Restart();
            Compress("input.file", "compressed.file");
            sw.Stop();
            Console.WriteLine($"Compressed by {sw.ElapsedMilliseconds} milliseconds");
            
            sw.Restart();
            Decompress("compressed.file", "decompressed.file");
            sw.Stop();
            Console.WriteLine($"Decompressed by {sw.ElapsedMilliseconds} milliseconds");
        }
    }
}