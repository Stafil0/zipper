namespace Zipper.Domain.Pipeline
{
    /// <summary>
    /// Data-reader from object.
    /// </summary>
    /// <typeparam name="TIn">Input object type.</typeparam>
    /// <typeparam name="TOut">Output data type.</typeparam>
    public interface IReader<in TIn, out TOut>
    {
        /// <summary>
        /// Read data from object.
        /// </summary>
        /// <param name="input">Input object.</param>
        /// <returns>Output data.</returns>
        TOut Read(TIn input);
    }
}