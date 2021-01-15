using System;
using System.Linq;
using Zipper.Domain.Data;
using Zipper.Domain.Pipeline;

namespace Zipper.Domain.BoundedBuffer.Blobs
{
    public class BlobWriter : IWriter<System.IO.Stream, Blob>
    {
        public void Write(System.IO.Stream input, Blob data)
        {
            var buffer = GetSizedBuffer(data);
            input.Write(buffer, 0, buffer.Length);
        }
        
        private byte[] GetSizedBuffer(Blob blob) => BitConverter.GetBytes(blob.Size).Concat(blob.Buffer).ToArray();
    }
}