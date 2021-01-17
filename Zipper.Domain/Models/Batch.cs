namespace Zipper.Domain.Models
{
    /// <summary>
    /// Memory batch.
    /// </summary>
    public class Batch
    {
        /// <summary>
        /// Offset.
        /// </summary>
        public int Offset { get; set; }

        /// <summary>
        /// Byte buffer.
        /// </summary>
        public byte[] Buffer { get; set; }

        /// <summary>
        /// Size of batch.
        /// </summary>
        public int Size => Buffer.Length;
    }
}