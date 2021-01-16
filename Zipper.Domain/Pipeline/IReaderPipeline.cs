using Zipper.Domain.BoundedBuffer;

namespace Zipper.Domain.Pipeline
{
    public interface IReaderPipeline<TIn, TOut>
    {
        event OnProgressHandler OnRead;

        delegate void OnProgressHandler(object sender, OnProgressEventArgs args);

        IReaderPipeline<TIn, TOut> Reader(IReader<TIn, TOut> inputReader);
    }
}