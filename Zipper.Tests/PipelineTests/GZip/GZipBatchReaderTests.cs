using System;
using System.IO;
using System.Linq;
using Xunit;
using Zipper.Domain.Exceptions;
using Zipper.Domain.Pipeline.GZip;

namespace Zipper.Tests.PipelineTests.GZip
{
    public class GZipBatchReaderTests
    {
        [Fact]
        public void Read_StreamEmpty_ReturnEmpty()
        {
            var reader = new GZipBatchReader();
            using var input = new MemoryStream();
            
            Assert.Empty(reader.Read(input));
        }

        [Fact]
        public void Read_InvalidStream_ThrowInvalidFormatException()
        {
            var reader = new GZipBatchReader();

            var guid = Guid.NewGuid();
            var bytes = guid.ToByteArray();
            using var input = new MemoryStream(bytes);

            Assert.Throws<InvalidFormatException>(() => reader.Read(input).ToList());
        }
    }
}