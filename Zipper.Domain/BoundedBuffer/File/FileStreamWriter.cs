using System.IO;
using Zipper.Domain.Data;
using Zipper.Domain.Pipeline;

namespace Zipper.Domain.BoundedBuffer.File
{
    public class FileStreamWriter : IWriter<Stream, Blob>
    {
        public void Write(Stream input, Blob data)
        {
            input.Write(data.Buffer, 0, data.Buffer.Length);
        }
    }
}