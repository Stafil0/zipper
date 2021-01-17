using System;
using System.IO;
using System.Linq;
using Xunit;
using Zipper.Domain.Exceptions;
using Zipper.Domain.Pipeline.Batch;

namespace Zipper.Tests.PipelineTests.Batch
{
    public class BatchStreamReaderTests
    {
        [Fact]
        public void Read_StreamEmpty_ReturnEmpty()
        {
            var reader = new BatchStreamReader();
            using var input = new MemoryStream();
            
            Assert.Empty(reader.Read(input));
        }

        [Fact]
        public void Read_InvalidStream_ThrowInvalidFormatException()
        {
            var reader = new BatchStreamReader();

            var guid = Guid.NewGuid();
            var bytes = guid.ToByteArray();
            using var input = new MemoryStream(bytes);

            Assert.Throws<InvalidFormatException>(() => reader.Read(input).ToList());
        }
    }
}