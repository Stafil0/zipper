using Zipper.Domain.Compression;

namespace Zipper.Domain.Pipeline.Compression
{
    public interface ICompressionPipeline<TReadIn, TReadOut, TWriteIn, TWriteOut> : IPipeline<TReadIn, TReadOut, TWriteIn, TWriteOut>
    {
        void Compress(TReadIn input, TWriteIn output, ICompressor compressor);

        void Decompress(TWriteIn input, TWriteIn output, IDecompressor decompressor);
    }
}