using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Zipper.Domain.Data;
using Zipper.Domain.Pipeline;

namespace Zipper.Domain.Compression
{
    public class CompressionPipeline :
        ICompressionPipeline<Stream, IEnumerable<Blob>, Stream, Blob>,
        IDisposable
    {
        private Thread _reader;

        private Thread _writer;

        private int _offset = 0;

        private readonly int _workLimit;

        private readonly int _workersCount;

        private List<Thread> _workers;

        private readonly Queue<Blob> _inputs = new Queue<Blob>();

        private readonly Queue<Blob> _outputs = new Queue<Blob>();

        private IReader<Stream, IEnumerable<Blob>> _inputReader;

        private IWriter<Stream, Blob> _outputWriter;

        public bool IsReading => _reader != null && _reader.IsAlive;

        public bool IsWriting => _writer != null && _writer.IsAlive;

        public bool IsWorking => _workers.Any(x => x.IsAlive);

        private Thread GetReader(Stream input) => new Thread(() =>
        {
            var spinner = new SpinWait();
            using (var enumerator = _inputReader.Read(input).GetEnumerator())
            {
                var next = enumerator.MoveNext();
                while (next)
                {
                    if (_inputs.Count >= _workLimit)
                        spinner.SpinOnce();
                    else
                    {
                        var current = enumerator.Current;
                        _inputs.Enqueue(current);
                        next = enumerator.MoveNext();
                    }
                }
            }
        });

        private List<Thread> GetWorkers(Func<byte[], byte[]> func)
        {
            return Enumerable
                .Range(0, _workersCount)
                .Select(x => new Thread(() =>
                {
                    var spinner = new SpinWait();
                    do
                    {
                        if (!_inputs.Any())
                            spinner.SpinOnce();
                        else
                        {
                            var blob = _inputs.Dequeue();
                            blob.Buffer = func(blob.Buffer);
                            _outputs.Enqueue(blob);
                        }
                    } while (IsReading || _inputs.Any());
                }))
                .ToList();
        }

        private Thread GetWriter(Stream output) => new Thread(() =>
        {
            var spinner = new SpinWait();
            while (IsReading || IsWorking || _outputs.Any())
            {
                if (!_outputs.Any())
                    spinner.SpinOnce();
                else
                {
                    var blob = _outputs.Peek();
                    if (blob.Offset != _offset)
                        spinner.SpinOnce();
                    else
                    {
                        blob = _outputs.Dequeue();
                        _outputWriter.Write(output, blob);
                        Interlocked.Increment(ref _offset);
                    }
                }
            }
        });

        private void Run(Stream input, Stream output, Func<byte[], byte[]> func)
        {
            _reader = GetReader(input);
            _reader.Start();

            _workers = GetWorkers(func);
            _workers.ForEach(t => t.Start());

            _writer = GetWriter(output);
            _writer.Start();

            SpinWait.SpinUntil(() => IsReading || IsWorking || IsWriting);
            Flush();
        }

        private void Flush()
        {
            _inputs.Clear();
            _outputs.Clear();
            _workers.Clear();
            _reader = null;
            _writer = null;
        }

        public IPipeline<Stream, IEnumerable<Blob>, Stream, Blob> Input(IReader<Stream, IEnumerable<Blob>> input)
        {
            _inputReader = input;
            return this;
        }

        public IPipeline<Stream, IEnumerable<Blob>, Stream, Blob> Output(IWriter<Stream, Blob> output)
        {
            _outputWriter = output;
            return this;
        }

        public void Compress(Stream input, Stream output, ICompressor compressor) =>
            Run(input, output, compressor.Compress);

        public void Decompress(Stream input, Stream output, IDecompressor decompressor) =>
            Run(input, output, decompressor.Decompress);

        public void Dispose() => Flush();

        public CompressionPipeline(int threadsCount, int workLimit)
        {
            _workersCount = threadsCount;
            _workLimit = workLimit;
        }
    }
}