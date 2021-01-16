namespace Zipper.Domain.BoundedBuffer
{
    public interface IReader<in TIn, out TOut>
    {
        TOut Read(TIn input);
    }
}