using System;
using System.IO;
using System.Linq;
using CommandLine;
using Zipper.Domain.Compression;
using Zipper.Domain.Extensions;
using Zipper.Domain.Pipeline;
using Zipper.Domain.Pipeline.Batch;
using Zipper.Domain.Pipeline.Byte;
using Zipper.Domain.Pipeline.Stream;

namespace Zipper.CLI
{
    internal class Program
    {
        private static int Compress(CompressOptions options)
        {
            try
            {
                using (var inputStream = new FileStream(options.InputFile, FileMode.Open))
                using (var outputStream = new FileStream(options.OutputFile, FileMode.Create))
                using (var pipeline = new StreamPipeline(options.ThreadsCount, options.WorkLimit))
                {
                    if (options.Verbose)
                    {
                        pipeline.OnRead += (sender, args) => PrintReadProgress(inputStream.Length, args);
                        pipeline.OnWrite += (sender, args) => PrintCompressionLevel(inputStream.Length, args);
                    }

                    pipeline
                        .Reader(new ByteStreamReader(options.BufferSize))
                        .Writer(new BatchStreamWriter())
                        .Converter(new GzipCompressor())
                        .Proceed(inputStream, outputStream);
                }
            }
            catch (AggregateException e)
            {
                var messages = string.Join(";", e.InnerExceptions.Select(x => x.Message));
                Console.WriteLine($"One or more errors occurred while compressing file \"{options.InputFile}\" to \"{options.OutputFile}\": {messages}");
                return 1;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error occured while compressing file \"{options.InputFile}\" to \"{options.OutputFile}\": {e.Message}");
                return 1;
            }

            return 0;
        }

        private static int Decompress(DecompressOptions options)
        {
            try
            {
                using (var inputStream = new FileStream(options.InputFile, FileMode.Open))
                using (var outputStream = new FileStream(options.OutputFile, FileMode.Create))
                using (var pipeline = new StreamPipeline(options.ThreadsCount, options.WorkLimit))
                {
                    if (options.Verbose)
                    {
                        pipeline.OnRead += (sender, args) => PrintReadProgress(inputStream.Length, args);
                        pipeline.OnWrite += (sender, args) => PrintDecompressionSize(args);
                    }

                    pipeline
                        .Reader(new BatchStreamReader())
                        .Writer(new ByteStreamWriter())
                        .Converter(new GzipDecompressor())
                        .Proceed(inputStream, outputStream);
                }
            }
            catch (AggregateException e)
            {
                var messages = string.Join(";", e.InnerExceptions.Select(x => x.Message));
                Console.WriteLine($"One or more errors occurred while decompressing file \"{options.InputFile}\" to \"{options.OutputFile}\": {messages}");
                return 1;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error occurred while decompressing file \"{options.InputFile}\" to \"{options.OutputFile}\": {e.Message}");
                return 1;
            }

            return 0;
        }

        private static void PrintReadProgress(long total, OnProgressEventArgs args)
        {
            var progress = (float) args.Progress / total * 100;
            Console.WriteLine($"{args.Message}\tTotal read progression: {progress:0.##}%");
        }

        private static void PrintCompressionLevel(long original, OnProgressEventArgs args)
        {
            var compression = original / (float) args.Progress;
            Console.WriteLine($"{args.Message}\tCompression level: {compression:0.##}%");
        }

        private static void PrintDecompressionSize(OnProgressEventArgs args)
        {
            Console.WriteLine($"Decompressed file size:\t{SizeExtensions.GetReadableSize(args.Progress)}");
        }

        public static int Main(string[] args)
        {
            return Parser.Default
                .ParseArguments<CompressOptions, DecompressOptions>(args)
                .MapResult(
                    (CompressOptions options) => Compress(options),
                    (DecompressOptions options) => Decompress(options),
                    errors =>
                    {
                        var err = errors.ToArray();
                        if (err.Any(x =>
                            x.Tag == ErrorType.HelpRequestedError ||
                            x.Tag == ErrorType.HelpVerbRequestedError || 
                            x.Tag == ErrorType.VersionRequestedError))
                            return 0;

                        Console.WriteLine("Errors while parsing options:");
                        foreach (var error in err)
                            Console.WriteLine(error);

                        return 1;
                    });
        }
    }
}