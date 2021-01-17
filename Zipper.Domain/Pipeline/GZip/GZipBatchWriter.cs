using System;

namespace Zipper.Domain.Pipeline.GZip
{
    public class GZipBatchWriter :
        GZipBatchStreamBase,
        IWriter<System.IO.Stream, byte[]>
    {
        public void Write(System.IO.Stream input, byte[] data)
        {
            if (data == null || data.Length == 0)
                return;

            var size = BitConverter.GetBytes(data.LongLength);

            input.Write(MagicData, 0, MagicData.Length);
            input.Write(size, 0, size.Length);
            input.Write(data, 0, data.Length);
        }
    }
}