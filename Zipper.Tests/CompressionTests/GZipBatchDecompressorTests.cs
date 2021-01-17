using System;
using System.IO;
using System.IO.Compression;
using Xunit;
using Zipper.Domain.Exceptions;
using Zipper.Domain.Pipeline.GZip;

namespace Zipper.Tests.CompressionTests
{
    public class GZipBatchDecompressorTests
    {        
        [Fact]
        public void Decompressor_EmptyInput_EmptyOutput()
        {
            var decompressor = new GZipBatchDecompressor();
            Assert.Null(decompressor.Convert(null));
            Assert.Empty(decompressor.Convert(new byte[0]));
        }
        
        [Fact]
        public void Decompressor_ConvertCompressed_CanRead()
        {
            var data = Guid.NewGuid();
            var bytes = data.ToByteArray();
            
            using var output = new MemoryStream();
            using (var zip = new GZipStream(output, CompressionMode.Compress))
                zip.Write(bytes, 0, bytes.Length);

            var compressed = output.ToArray();

            var decompressor = new GZipBatchDecompressor();
            var decompressed = decompressor.Convert(compressed);

            var guid = new Guid(decompressed);
            Assert.Equal(data, guid);
        }
        
        [Fact]
        public void Decompressor_ConvertNotCompressed_CatchInvalidFormatException()
        {
            var data = Guid.NewGuid();
            var bytes = data.ToByteArray();
            
            using var input = new MemoryStream(bytes);
            
            var decompressor = new GZipBatchDecompressor();
            Assert.Throws<InvalidFormatException>(() => decompressor.Convert(input.ToArray()));
        }
    }
}