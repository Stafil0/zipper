using System.Collections.Generic;
using System.IO;
using Zipper.Domain.Compression;
using Zipper.Domain.Data;

namespace Zipper.Domain.Pipeline.Compression
{
    public interface ICompressionPipeline :
        IReaderPipeline<Stream, IEnumerable<Blob>>,
        IWriterPipeline<Stream, Blob>
    {
        void Compress(Stream input, Stream output, ICompressor compressor);

        void Decompress(Stream input, Stream output, IDecompressor decompressor);
    }
}