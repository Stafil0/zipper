namespace Zipper.Domain.Compression
{
    public interface ICompressor
    {
        byte[] Compress(byte[] data);
    }
}