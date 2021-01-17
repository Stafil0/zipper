namespace Zipper.Domain.Pipeline
{
    /// <summary>
    /// Data-writer to object.
    /// </summary>
    /// <typeparam name="TInput">Input object type.</typeparam>
    /// <typeparam name="TData">Write data type.</typeparam>
    public interface IWriter<in TInput, in TData>
    {
        /// <summary>
        /// Write data to object.
        /// </summary>
        /// <param name="input">Input object type.</param>
        /// <param name="data">Write data type.</param>
        void Write(TInput input, TData data);
    }
}