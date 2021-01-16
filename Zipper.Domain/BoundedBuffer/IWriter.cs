namespace Zipper.Domain.BoundedBuffer
{
    public interface IWriter<in TInput, in TData>
    {
        void Write(TInput input, TData data);
    }
}