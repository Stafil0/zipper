namespace Zipper.Domain.Pipeline.File
{
    public class FileStreamWriter : IWriter<System.IO.Stream, byte[]>
    {
        public void Write(System.IO.Stream input, byte[] data)
        {
            input.Write(data, 0, data.Length);
        }
    }
}