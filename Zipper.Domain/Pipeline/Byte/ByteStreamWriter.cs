namespace Zipper.Domain.Pipeline.Byte
{
    /// <summary>
    /// Byte stream writer.
    /// </summary>
    public class ByteStreamWriter : IWriter<System.IO.Stream, byte[]>
    {
        /// <summary>
        /// Write bytes to end of a stream.
        /// </summary>
        /// <param name="input">Stream.</param>
        /// <param name="data">Byte array.</param>
        public void Write(System.IO.Stream input, byte[] data)
        {
            input.Write(data, 0, data.Length);
        }
    }
}