using System;
using System.Linq;

namespace Zipper.Domain.Pipeline.Batch
{
    public class BatchStreamWriter :
        BatchStreamBase,
        IWriter<System.IO.Stream, byte[]>
    {
        public void Write(System.IO.Stream input, byte[] data)
        {
            var sized = PrependSize(data);
            var magic = PrependMagic(sized);
            input.Write(magic, 0, magic.Length);
        }

        private static byte[] PrependMagic(byte[] data) => PrependBytes(data, MagicData);

        private static byte[] PrependSize(byte[] data) => PrependBytes(data, BitConverter.GetBytes(data.LongLength));

        private static byte[] PrependBytes(byte[] data, byte[] prepend) => prepend.Concat(data).ToArray();
    }
}