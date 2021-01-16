using Zipper.Domain.BoundedBuffer;

namespace Zipper.Domain.Pipeline
{
    public interface IWriterPipeline<TIn, TOut>
    {
        event OnProgressHandler OnWrite;

        delegate void OnProgressHandler(object sender, OnProgressEventArgs args);

        IWriterPipeline<TIn, TOut> Writer(IWriter<TIn, TOut> outputReader);
    }
}