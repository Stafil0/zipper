using CommandLine;

namespace Zipper.CLI
{
    [Verb("compress", HelpText = "Compress file.")]
    internal class CompressOptions : BaseOptions
    {
        [Option('b', "buffer", Default = 1024 * 1024, HelpText = "Buffer size.")]
        public int BufferSize { get; set; }
    }

    [Verb("decompress", HelpText = "Decompress file.")]
    internal class DecompressOptions : BaseOptions
    {
    }

    class BaseOptions
    {
        [Value(0, Required = true, HelpText = "Input file to be processed.")]
        public string InputFile { get; set; }

        [Value(0, Required = true, HelpText = "Output file.")]
        public string OutputFile { get; set; }

        [Option('t', "threads", Default = 16, HelpText = "Worker threads count.")]
        public int ThreadsCount { get; set; }

        [Option('m', "max", Default = 16, HelpText = "Max blobs count.")]
        public int MaxBlobs { get; set; }

        [Option('v', "verbose", Default = false, HelpText = "Prints all messages to standard output.")]
        public bool Verbose { get; set; }
    }
}