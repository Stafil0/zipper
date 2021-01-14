using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using Zipper.Domain.Compression;
using Zipper.Domain.Data;

namespace Zipper.Domain.Pipeline.Compression
{
    public class CompressionPipeline :
        ICompressionPipeline<Stream, IEnumerable<Blob>, Stream, Blob>,
        IDisposable
    {
        private Thread _reader;

        private Thread _writer;

        private int _offset;

        private readonly int _workLimit;

        private readonly int _workersCount;

        private List<Thread> _workers;

        private ConcurrentQueue<Blob> _inputs = new ConcurrentQueue<Blob>();

        private ConcurrentQueue<Blob> _outputs = new ConcurrentQueue<Blob>();

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
                Debug.WriteLine($"Reader:\tstarted. ThreadId = {Thread.CurrentThread.ManagedThreadId}");
                
                var next = enumerator.MoveNext();
                while (next)
                {
                    if (_inputs.Count >= _workLimit)
                    {
                        Debug.WriteLine($"Reader:\tToo many blobs ({_inputs.Count}), spinning while >= {_workLimit}. ThreadId = {Thread.CurrentThread.ManagedThreadId}");
                        spinner.SpinOnce();
                    }
                    else
                    {
                        var current = enumerator.Current;
                        if (current != null)
                        {
                            Debug.WriteLine($"Reader:\tGot blob ({current.Offset}). ThreadId = {Thread.CurrentThread.ManagedThreadId}");
                            _inputs.Enqueue(current);
                        }

                        next = enumerator.MoveNext();
                    }
                }
            }
        });

        private List<Thread> GetWorkers(Func<byte[], byte[]> func) => 
            Enumerable
                .Range(0, _workersCount)
                .Select(x => new Thread(() =>
                {
                    Debug.WriteLine($"Worker:\tstarted. ThreadId = {Thread.CurrentThread.ManagedThreadId}");

                    var spinner = new SpinWait();
                    do
                    {
                        Debug.WriteLine($"Worker\tTrying get new work item. ThreadId = {Thread.CurrentThread.ManagedThreadId}");
                        if (!_inputs.TryDequeue(out var blob))
                        {
                            Debug.WriteLine($"Worker:\tNo work available, spinning. ThreadId = {Thread.CurrentThread.ManagedThreadId}");
                            spinner.SpinOnce();
                        }
                        else
                        {
                            Debug.WriteLine($"Worker:\tWorking with blob ({blob.Offset}). ThreadId = {Thread.CurrentThread.ManagedThreadId}");
                            blob.Buffer = func(blob.Buffer);
                            _outputs.Enqueue(blob);
                        }
                    } while (IsReading || _inputs.Any());
                }))
                .ToList();

        private Thread GetWriter(Stream output) => new Thread(() =>
        {
            var spinner = new SpinWait();
            
            Debug.WriteLine($"Writer:\tstarted. ThreadId = {Thread.CurrentThread.ManagedThreadId}");
            while (IsReading || IsWorking || _outputs.Any())
            {
                if (!_outputs.Any())
                {
                    Debug.WriteLine($"Writer:\tNo blobs available for write, spinning. ThreadId = {Thread.CurrentThread.ManagedThreadId}");
                    spinner.SpinOnce();
                }
                else
                {
                    Debug.WriteLine($"Writer:\tTrying to get new blob to write. ThreadId = {Thread.CurrentThread.ManagedThreadId}");
                    if (!_outputs.TryDequeue(out var blob) || blob.Offset != _offset)
                    {
                        Debug.WriteLine($"Writer:\tBlob not in order ({blob.Offset}), waiting for ({_offset}). ThreadId = {Thread.CurrentThread.ManagedThreadId}");
                        _outputs.Enqueue(blob);
                        spinner.SpinOnce();
                    }
                    else
                    {
                        Debug.WriteLine($"Writer:\tGot blob ({blob.Offset}), writing. ThreadId = {Thread.CurrentThread.ManagedThreadId}");
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

            SpinWait.SpinUntil(() => !(IsReading || IsWorking || IsWriting));
            Flush();
        }

        private void Flush()
        {
            _workers.Clear();
            _inputs = new ConcurrentQueue<Blob>();
            _outputs = new ConcurrentQueue<Blob>();
            _reader = null;
            _writer = null;
        }

        IPipeline<Stream, IEnumerable<Blob>, Stream, Blob> IPipeline<Stream, IEnumerable<Blob>, Stream, Blob>.Reader(IReader<Stream, IEnumerable<Blob>> input) => Reader(input);

        public CompressionPipeline Reader(IReader<Stream, IEnumerable<Blob>> input)
        {
            _inputReader = input;
            return this;
        }

        IPipeline<Stream, IEnumerable<Blob>, Stream, Blob> IPipeline<Stream, IEnumerable<Blob>, Stream, Blob>.Writer(IWriter<Stream, Blob> input) => Writer(input);

        public CompressionPipeline Writer(IWriter<Stream, Blob> output)
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