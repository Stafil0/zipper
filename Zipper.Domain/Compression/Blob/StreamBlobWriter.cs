using System.IO;
using Zipper.Domain.Pipeline;

namespace Zipper.Domain.Compression.Blob
{
    public class StreamBlobWriter : IWriter<Stream, Data.Blob>
    {
        public void Write(Stream input, Data.Blob data)
        {
            input.Write(data.Buffer, 0, data.Buffer.Length);
        }

        public StreamBlobWriter(int bufferSize)
        {
        }
    }
}