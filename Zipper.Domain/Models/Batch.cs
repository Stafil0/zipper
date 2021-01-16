namespace Zipper.Domain.Models
{
    public class Batch
    {
        public int Offset { get; set; }

        public byte[] Buffer { get; set; }

        public int Size => Buffer.Length;
    }
}