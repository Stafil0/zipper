namespace Zipper.Domain.Data
{
    public class Blob
    {
        public int Offset { get; set; }
        
        public byte[] Buffer { get; set; }
    }
}