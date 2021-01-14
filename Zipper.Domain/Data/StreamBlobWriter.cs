using System.IO;
using Zipper.Domain.Pipeline;

namespace Zipper.Domain.Data
{
    public class StreamBlobWriter : IWriter<Stream, Blob>
    {
        public void Write(Stream input, Blob data)
        {
            input.Write(data.Buffer, 0, data.Buffer.Length);
        }
    }
}