using System;
using System.IO;
using System.IO.Compression;
using Xunit;
using Zipper.Domain.Compression;

namespace Zipper.Tests.CompressionTests
{
    public class GzipCompressorTests
    {
        [Fact]
        public void Compressor_EmptyInput_EmptyOutput()
        {
            var compressor = new GzipCompressor();
            Assert.Null(compressor.Convert(null));
            Assert.Empty(compressor.Convert(new byte[0]));
        }
        
        [Fact]
        public void Compressor_Convert_CanDecompress()
        {
            var data = Guid.NewGuid();
            var bytes = data.ToByteArray();
            var compressor = new GzipCompressor();

            var compressed = compressor.Convert(bytes);
            
            using var input = new MemoryStream(compressed);
            using var output = new MemoryStream();
            using var zip = new GZipStream(input, CompressionMode.Decompress);

            zip.CopyTo(output);
            var decompressed = output.ToArray();
            var guid = new Guid(decompressed);
            
            Assert.Equal(data, guid);
        }
    }
}