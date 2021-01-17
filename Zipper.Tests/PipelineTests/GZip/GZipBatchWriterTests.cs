using System;
using System.IO;
using Xunit;
using Zipper.Domain.Pipeline.GZip;

namespace Zipper.Tests.PipelineTests.GZip
{
    public class GZipBatchWriterTests
    {        
        [Fact]
        public void Write_EmptyBuffer_WriteNothing()
        {
            var buffer = new byte[0];
            var writer = new GZipBatchWriter();
            using var output = new MemoryStream();
            
            writer.Write(output, buffer);
            
            Assert.Empty(output.ToArray());
        }
        
        [Fact]
        public void Write_Buffer_CorrectResult()
        {
            var writer = new GZipBatchWriter();
            var reader = new GZipBatchReader();

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