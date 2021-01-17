namespace Zipper.Domain.Pipeline
{
    /// <summary>
    /// Converter of one object to another.
    /// </summary>
    /// <typeparam name="TIn">Input object type.</typeparam>
    /// <typeparam name="TOut">Output object type.</typeparam>
    public interface IConverter<in TIn, out TOut>
    {
        /// <summary>
        /// Convert one object to another.
        /// </summary>
        /// <param name="data">Input object.</param>
        /// <returns>Output object.</returns>
        TOut Convert(TIn data);
    }
}