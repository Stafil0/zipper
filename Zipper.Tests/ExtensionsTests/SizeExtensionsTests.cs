using System;
using Xunit;
using Zipper.Domain.Extensions;

namespace Zipper.Tests.ExtensionsTests
{
    public class SizeExtensionsTests
    {
        [Fact]
        public void GetReadableSize_SizeLessThanZero_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => SizeExtensions.GetReadableSize(-1));
        }

        [Theory]
        [InlineData(0, "0 B")]
        [InlineData(512, "512 B")]
        [InlineData(1024, "1 KB")]
        [InlineData(1024 * 1024, "1 MB")]
        [InlineData(1024 * 1024 * 1024, "1 GB")]
        public void GetReadableSize_InputBytes_GetReadableString(long bytes, string result)
        {
            var size = SizeExtensions.GetReadableSize(bytes);
            Assert.Equal(result, size);
        }
    }
}