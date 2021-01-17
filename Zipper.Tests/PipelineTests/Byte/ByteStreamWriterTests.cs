using System;
using System.IO;
using Xunit;
using Zipper.Domain.Pipeline.Byte;

namespace Zipper.Tests.PipelineTests.Byte
{
    public class ByteStreamWriterTests
    {
        [Fact]
        public void Write_EmptyBuffer_WriteNothing()
        {
            var buffer = new byte[0];
            var writer = new ByteStreamWriter();
            using var output = new MemoryStream();
            
            writer.Write(output, buffer);
            
            Assert.Empty(output.ToArray());
        }
        
        [Fact]
        public void Write_Buffer_CorrectResult()
        {
            var guid = Guid.NewGuid();
            var buffer = guid.ToByteArray();
            var writer = new ByteStreamWriter();
            using var output = new MemoryStream();
            
            writer.Write(output, buffer);
            var result = output.ToArray();
            
            Assert.NotEmpty(result);
            Assert.Equal(guid, new Guid(result));
        }
    }
}