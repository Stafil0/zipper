using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Moq;
using Xunit;
using Xunit.Abstractions;
using Zipper.Domain.Pipeline;
using Zipper.Domain.Pipeline.Byte;
using Zipper.Domain.Pipeline.GZip;
using Zipper.Domain.Pipeline.Stream;
using Zipper.Tests.Utils;

namespace Zipper.Tests.PipelineTests.Stream
{
    public class StreamPipelineTests
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public StreamPipelineTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        [Fact]
        public void Init_ThreadsLessThanZero_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => new StreamPipeline(threadsCount: -1));
        }

        [Fact]
        public void Init_WorkersLessThanZero_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => new StreamPipeline(workLimit: -1));
        }

        [Fact]
        public void Proceed_EmptyStreams_ThrowsArgumentException()
        {
            var stream = new Mock<System.IO.Stream>();
            var writer = new Mock<IWriter<System.IO.Stream, byte[]>>();
            var reader = new Mock<IReader<System.IO.Stream, IEnumerable<byte[]>>>();

            using var pipeline = new StreamPipeline()
                .Reader(reader.Object)
                .Writer(writer.Object);

            Assert.Throws<ArgumentException>(() => pipeline.Proceed(null, null));
            Assert.Throws<ArgumentException>(() => pipeline.Proceed(stream.Object, null));
            Assert.Throws<ArgumentException>(() => pipeline.Proceed(null, stream.Object));
        }

        [Fact]
        public void Proceed_NotSetUp_ThrowsArgumentException()
        {
            var input = new Mock<System.IO.Stream>();
            var output = new Mock<System.IO.Stream>();

            using var pipeline = new StreamPipeline();

            Assert.Throws<ArgumentException>(() => pipeline.Proceed(input.Object, output.Object));
        }

        [Fact]
        public void Proceed_ReaderNotSet_ThrowsArgumentException()
        {
            var input = new Mock<System.IO.Stream>();
            var output = new Mock<System.IO.Stream>();
            var writer = new Mock<IWriter<System.IO.Stream, byte[]>>();

            using var pipeline = new StreamPipeline();
            pipeline.Writer(writer.Object);

            Assert.Throws<ArgumentException>(() => pipeline.Proceed(input.Object, output.Object));
        }

        [Fact]
        public void Proceed_WriterNotSet_ThrowsArgumentException()
        {
            var input = new Mock<System.IO.Stream>();
            var output = new Mock<System.IO.Stream>();
            var reader = new Mock<IReader<System.IO.Stream, IEnumerable<byte[]>>>();

            using var pipeline = new StreamPipeline();
            pipeline.Reader(reader.Object);

            Assert.Throws<ArgumentException>(() => pipeline.Proceed(input.Object, output.Object));
        }

        [Theory]
        [InlineData(1, 1)]
        [InlineData(4, 2)]
        [InlineData(8, 4)]
        [InlineData(16, 8)]
        [InlineData(32, 16)]
        [InlineData(64, null)]
        public void Proceed_PipelineSetUp_ReadConvertWriteData(int threadsCount, int? workLimit)
        {
            var outputs = 100;
            var input = new Mock<MemoryStream>();
            var output = new Mock<MemoryStream>();
            var data = Generators.Generate(outputs, () => Guid.NewGuid().ToByteArray()).ToArray();

            var reader = new Mock<IReader<System.IO.Stream, IEnumerable<byte[]>>>();
            reader
                .Setup(x => x.Read(input.Object))
                .Returns(data);

            var writer = new Mock<IWriter<System.IO.Stream, byte[]>>();
            foreach (var datum in data)
                writer.Setup(x => x.Write(output.Object, datum));

            var converter = new Mock<IConverter<byte[], byte[]>>();
            foreach (var datum in data)
                converter
                    .Setup(x => x.Convert(datum))
                    .Returns<byte[]>(x => x);

            using var pipeline = new StreamPipeline(threadsCount: threadsCount, workLimit: workLimit);
            pipeline
                .Reader(reader.Object)
                .Writer(writer.Object)
                .Converter(converter.Object);

            if (workLimit.HasValue)
                pipeline.OnRead += (sender, args) => Assert.True(args.Current <= workLimit);

            pipeline.Proceed(input.Object, output.Object);

            reader.Verify(x => x.Read(input.Object), Times.Once);

            converter.Verify(x => x.Convert(It.IsAny<byte[]>()), Times.Exactly(outputs));
            foreach (var datum in data)
                converter.Verify(x => x.Convert(datum), Times.Once);

            writer.Verify(x => x.Write(output.Object, It.IsAny<byte[]>()), Times.Exactly(outputs));
            foreach (var datum in data)
                writer.Verify(x => x.Write(output.Object, datum), Times.Once);
        }
    }

    public class FileStreamPipelineTests
    {
        [Theory]
        [InlineData(1, 1)]
        [InlineData(4, 2)]
        [InlineData(8, 4)]
        public void Proceed_CompressDecompressBigFile_TotalMemoryLessThanFileSize(int threadsCount, int? workLimit)
        {
            using var input = new TempFile();
            using var output = new TempFile();

            var gigabyte = 1024 * 1024 * 1024.0;
            var size = Guid.Empty.ToByteArray().Length;

            var generator = Generators.Generate((int) (gigabyte / size), () => Guid.NewGuid().ToByteArray());
            using (var fs = new FileStream(input.FullPath, FileMode.OpenOrCreate))
            {
                foreach (var bytes in generator)
                    fs.Write(bytes, 0, bytes.Length);
            }

            using var compressionPipeline = new StreamPipeline(threadsCount: threadsCount, workLimit: workLimit)
                .Reader(new ByteStreamReader(1024 * 1024))
                .Writer(new GZipBatchWriter())
                .Converter(new GZipBatchCompressor());

            compressionPipeline.OnRead += (sender, args) => Assert.True(GC.GetTotalMemory(true) < gigabyte * 0.1);
            compressionPipeline.OnWrite += (sender, args) => Assert.True(GC.GetTotalMemory(true) < gigabyte * 0.1);

            using (var inputStream = new FileStream(input.FullPath, FileMode.Open))
            using (var outputStream = new FileStream(output.FullPath, FileMode.OpenOrCreate))
            {
                compressionPipeline.Proceed(inputStream, outputStream);
            }

            using var decompressionPipeline = new StreamPipeline(threadsCount: threadsCount, workLimit: workLimit)
                .Reader(new GZipBatchReader())
                .Writer(new ByteStreamWriter())
                .Converter(new GZipBatchDecompressor());

            decompressionPipeline.OnRead += (sender, args) => Assert.True(GC.GetTotalMemory(true) < gigabyte * 0.1);
            decompressionPipeline.OnWrite +=
                (sender, args) => Assert.True(GC.GetTotalMemory(true) < gigabyte * 0.1);

            using (var inputStream = new FileStream(output.FullPath, FileMode.Open))
            using (var outputStream = new FileStream(input.FullPath, FileMode.OpenOrCreate))
            {
                decompressionPipeline.Proceed(inputStream, outputStream);
            }
        }
    }
}