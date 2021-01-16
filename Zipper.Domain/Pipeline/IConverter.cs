namespace Zipper.Domain.Pipeline
{
    public interface IConverter<in TIn, out TOut>
    {
        TOut Convert(TIn data);
    }
}