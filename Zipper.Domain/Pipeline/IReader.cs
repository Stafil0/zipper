namespace Zipper.Domain.Pipeline
{
    public interface IReader<in TIn, out TOut>
    {
        TOut Read(TIn input);
    }
}