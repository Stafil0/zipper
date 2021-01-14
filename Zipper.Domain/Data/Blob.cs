namespace Zipper.Domain.Data
{
    public class Blob
    {
        public int Offset { get; }
        
        public byte[] Buffer { get; set; }

        public int Length => Buffer.Length;

        public bool IsEmpty => Buffer == null || Buffer.Length == 0;
    }
}