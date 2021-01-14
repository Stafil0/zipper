namespace Zipper.Domain.Compression
{
    public interface IDecompressor
    {
        byte[] Decompress(byte[] data);
    }
}