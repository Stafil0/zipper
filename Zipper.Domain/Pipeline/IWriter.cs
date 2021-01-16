namespace Zipper.Domain.Pipeline
{
    public interface IWriter<in TInput, in TData>
    {
        void Write(TInput input, TData data);
    }
}