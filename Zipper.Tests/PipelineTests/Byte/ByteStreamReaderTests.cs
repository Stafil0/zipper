using System;
using System.IO;
using Xunit;
using Zipper.Domain.Pipeline.Byte;

namespace Zipper.Tests.PipelineTests.Byte
{
    public class ByteStreamReaderTests
    {
        [Fact]
        public void Init_BufferSizeLessThanZero_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => new ByteStreamReader(-1));
        }
        
        [Fact]
        public void Read_StreamEmpty_ReturnEmpty()
        {
            var size = 1;
            var reader = new ByteStreamReader(size);
            using var input = new MemoryStream();
            
            Assert.Empty(reader.Read(input));
        }

        [Fact]
        public void Read_StreamNotEmpty_ReadBytes()
        {
            var size = 4;
            var reader = new ByteStreamReader(size);

            var guid = Guid.NewGuid();
            var bytes = guid.ToByteArray();
            using var input = new MemoryStream(bytes);

            var index = 0;
            var output = new byte[bytes.Length];
            foreach (var read in reader.Read(input))
            foreach (var readByte in read)
            {
                output[index++] = readByte;
            }

            Assert.Equal(guid, new Guid(output));
        }
    }
}