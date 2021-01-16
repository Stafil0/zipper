using System;
using System.Linq;

namespace Zipper.Domain.Pipeline.Batch
{
    public class BatchStreamWriter : IWriter<System.IO.Stream, byte[]>
    {
        public void Write(System.IO.Stream input, byte[] data)
        {
            var buffer = GetSizedBuffer(data);
            input.Write(buffer, 0, buffer.Length);
        }

        private byte[] GetSizedBuffer(byte[] data) => BitConverter.GetBytes(data.Length).Concat(data).ToArray();
    }
}