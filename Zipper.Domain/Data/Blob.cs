namespace Zipper.Domain.Data
{
    public class Blob
    {
        public int Offset { get; set; }

        public byte[] Buffer { get; set; }

        public int Size => Buffer.Length;
    }
}