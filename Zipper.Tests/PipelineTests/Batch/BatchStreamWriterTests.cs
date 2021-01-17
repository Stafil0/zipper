using System;
using System.IO;
using Xunit;
using Zipper.Domain.Pipeline.Batch;

namespace Zipper.Tests.PipelineTests.Batch
{
    public class BatchStreamWriterTests
    {        
        [Fact]
        public void Write_EmptyBuffer_WriteNothing()
        {
            var buffer = new byte[0];
            var writer = new BatchStreamWriter();
            using var output = new MemoryStream();
            
            writer.Write(output, buffer);
            
            Assert.Empty(output.ToArray());
        }
        
        [Fact]
        public void Write_Buffer_CorrectResult()
        {
            var writer = new BatchStreamWriter();
            var reader = new BatchStreamReader();

            var guid = Guid.NewGuid();
            var bytes = guid.ToByteArray();

            using var input = new MemoryStream();
            writer.Write(input, bytes);
            input.Seek(0, SeekOrigin.Begin);

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