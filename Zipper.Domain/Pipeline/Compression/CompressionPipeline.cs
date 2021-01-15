using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using Zipper.Domain.Collections;
using Zipper.Domain.Compression;
using Zipper.Domain.Data;
using Zipper.Domain.Extensions;

namespace Zipper.Domain.Pipeline.Compression
{
    public class CompressionPipeline :
        ICompressionPipeline<Stream, IEnumerable<Blob>, Stream, Blob>,
        IDisposable
    {
        private Thread _reader;

        private Thread _writer;

        private int _offset;

        private readonly int? _workLimit;

        private readonly int _workersCount;

        private List<Thread> _workers;

        private ConcurrentBag<Blob> _inputs = new ConcurrentBag<Blob>();

        private BlockingPriorityQueue<Blob> _outputs = new BlockingPriorityQueue<Blob>(new BlobComparer());

        private IReader<Stream, IEnumerable<Blob>> _inputReader;

        private IWriter<Stream, Blob> _outputWriter;

        private readonly List<Exception> _exceptions = new List<Exception>();

        private AggregateException Exception => _exceptions.Any() ? new AggregateException(_exceptions) : null;

        public event IPipeline<Stream, IEnumerable<Blob>, Stream, Blob>.OnProgressHandler OnRead;

        public event IPipeline<Stream, IEnumerable<Blob>, Stream, Blob>.OnProgressHandler OnWrite;

        public bool IsReading => _reader != null && _reader.IsAlive;

        public bool IsWriting => _writer != null && _writer.IsAlive;

        public bool IsWorking => _workers.Any(x => x.IsAlive);

        private void OnExceptionHandler(Exception e) => _exceptions.Add(e);

        private Thread GetReader(Stream input) => new Thread(() =>
        {
            try
            {
                var size = 0;
                var spinner = new SpinWait();
                using (var enumerator = _inputReader.Read(input).GetEnumerator())
                {
                    Debug.WriteLine($"Reader:\tstarted. ThreadId = {Thread.CurrentThread.ManagedThreadId}");

                    var next = enumerator.MoveNext();
                    while (next)
                    {
                        if (_workLimit.HasValue && _inputs.Count >= _workLimit)
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
                                _inputs.Add(current);
                                size += current.Size;

                                OnRead?.Invoke(this, new OnProgressEventArgs(size, $"Read:\t{SizeExtensions.GetReadableSize(size)}."));
                            }

                            next = enumerator.MoveNext();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                OnExceptionHandler(e);
            }
        });

        private List<Thread> GetWorkers(Func<byte[], byte[]> func) => 
            Enumerable
                .Range(0, _workersCount)
                .Select(x => new Thread(() =>
                {
                    try
                    {
                        Debug.WriteLine($"Worker:\tstarted. ThreadId = {Thread.CurrentThread.ManagedThreadId}");

                        var spinner = new SpinWait();
                        do
                        {
                            Debug.WriteLine($"Worker:\tTrying get new work item. ThreadId = {Thread.CurrentThread.ManagedThreadId}");
                            if (!_inputs.TryTake(out var blob))
                            {
                                Debug.WriteLine($"Worker:\tNo work available, spinning. ThreadId = {Thread.CurrentThread.ManagedThreadId}");
                                spinner.SpinOnce();
                            }
                            else
                            {
                                Debug.WriteLine($"Worker:\tWorking with blob ({blob.Offset}). ThreadId = {Thread.CurrentThread.ManagedThreadId}");
                                blob.Buffer = func(blob.Buffer);
                                _outputs.TryEnqueue(blob);
                            }
                        } while (IsReading || _inputs.Any());
                    }
                    catch (Exception e)
                    {
                        OnExceptionHandler(e);
                    }
                }))
                .ToList();

        private Thread GetWriter(Stream output) => new Thread(() =>
        {
            try
            {
                var spinner = new SpinWait();
                var size = 0;

                Debug.WriteLine($"Writer:\tstarted. ThreadId = {Thread.CurrentThread.ManagedThreadId}");
                while (IsReading || IsWorking || _outputs.Any())
                {
                    if (!_outputs.TryPeek(out var blob))
                    {
                        Debug.WriteLine($"Writer:\tNo blobs available to write, spinning. ThreadId = {Thread.CurrentThread.ManagedThreadId}");
                        spinner.SpinOnce();
                    }
                    else
                    {
                        Debug.WriteLine($"Writer:\tTrying to get new blob to write. ThreadId = {Thread.CurrentThread.ManagedThreadId}");
                        if (blob.Offset != _offset)
                        {
                            Debug.WriteLine($"Writer:\tBlob not in order ({blob.Offset}), waiting for ({_offset}). ThreadId = {Thread.CurrentThread.ManagedThreadId}");
                            spinner.SpinOnce();
                        }
                        else
                        {
                            blob = _outputs.Dequeue();
                            _outputWriter.Write(output, blob);
                            size += blob.Size;

                            OnWrite?.Invoke(this, new OnProgressEventArgs(size, $"Write:\t{SizeExtensions.GetReadableSize(size)}."));
                            Debug.WriteLine($"Writer:\tGot blob ({blob.Offset}), writing. ThreadId = {Thread.CurrentThread.ManagedThreadId}");
                            
                            Interlocked.Increment(ref _offset);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                OnExceptionHandler(e);
            }
        });

        private void Run(Stream input, Stream output, Func<byte[], byte[]> func)
        {
            Debug.WriteLine($"Compression pipeline started. ThreadId = {Thread.CurrentThread.ManagedThreadId}");

            _reader = GetReader(input);
            _reader.Start();

            _workers = GetWorkers(func);
            _workers.ForEach(t => t.Start());

            _writer = GetWriter(output);
            _writer.Start();

            SpinWait.SpinUntil(() => !(IsReading || IsWorking || IsWriting));
            
            Debug.WriteLine($"Compression pipeline completed. ThreadId = {Thread.CurrentThread.ManagedThreadId}");
            Flush();
        }

        private void Flush()
        {
            _reader = null;
            _writer = null;
            _workers.Clear();
            _inputs = new ConcurrentBag<Blob>();
            _outputs = new BlockingPriorityQueue<Blob>();
            _exceptions.Clear();
        }

        IPipeline<Stream, IEnumerable<Blob>, Stream, Blob> IPipeline<Stream, IEnumerable<Blob>, Stream, Blob>
            .Reader(IReader<Stream, IEnumerable<Blob>> input) => Reader(input);

        public CompressionPipeline Reader(IReader<Stream, IEnumerable<Blob>> input)
        {
            _inputReader = input;
            return this;
        }

        IPipeline<Stream, IEnumerable<Blob>, Stream, Blob> IPipeline<Stream, IEnumerable<Blob>, Stream, Blob>
            .Writer(IWriter<Stream, Blob> input) => Writer(input);

        public CompressionPipeline Writer(IWriter<Stream, Blob> output)
        {
            _outputWriter = output;
            return this;
        }

        public void Compress(Stream input, Stream output, ICompressor compressor) =>
            Run(input, output, compressor.Compress);

        public void Decompress(Stream input, Stream output, IDecompressor decompressor) =>
            Run(input, output, decompressor.Decompress);

        public void ThrowIfException()
        {
            if (Exception != null)
                throw Exception;
        }

        public void Dispose() => Flush();

        public CompressionPipeline(int threadsCount, int? workLimit)
        {
            _workersCount = threadsCount;
            _workLimit = workLimit;
        }
    }
}