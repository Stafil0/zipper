namespace Zipper.Domain.Pipeline.Byte
{
    public class ByteStreamWriter : IWriter<System.IO.Stream, byte[]>
    {
        public void Write(System.IO.Stream input, byte[] data)
        {
            input.Write(data, 0, data.Length);
        }
    }
}