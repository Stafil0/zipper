using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using Zipper.Domain.BoundedBuffer;
using Zipper.Domain.Collections;
using Zipper.Domain.Compression;
using Zipper.Domain.Data;
using Zipper.Domain.Extensions;

namespace Zipper.Domain.Pipeline.Compression
{
    public class CompressionPipeline : ICompressionPipeline, IDisposable
    {
        private Thread _reader;

        private Thread _writer;

        private int _offset;

        private readonly int? _workLimit;

        private readonly List<Thread> _workers;

        private ConcurrentBag<Blob> _inputs = new ConcurrentBag<Blob>();

        private BlockingPriorityQueue<Blob> _outputs = new BlockingPriorityQueue<Blob>(new BlobComparer());

        private IReader<Stream, IEnumerable<Blob>> _inputReader;

        private IWriter<Stream, Blob> _outputWriter;

        private readonly List<Exception> _exceptions = new List<Exception>();

        private AggregateException Exception => _exceptions.Any() ? new AggregateException(_exceptions) : null;

        public event IReaderPipeline<Stream, IEnumerable<Blob>>.OnProgressHandler OnRead;

        public event IWriterPipeline<Stream, Blob>.OnProgressHandler OnWrite;

        public bool IsReading => _reader != null && _reader.IsAlive;

        public bool IsWriting => _writer != null && _writer.IsAlive;

        public bool IsWorking => _workers.Any(x => x.IsAlive);

        private void OnExceptionHandler(Exception e) => _exceptions.Add(e);

        private void ReadStream(object obj)
        {
            try
            {
                if (!(obj is Stream input))
                    return;

                var size = 0;
                var spinner = new SpinWait();
                using var enumerator = _inputReader.Read(input).GetEnumerator();

                Debug.WriteLine($"Reader:\tstarted. ThreadId = {Thread.CurrentThread.ManagedThreadId}");

                var next = enumerator.MoveNext();
                while (next)
                {
                    if (_workLimit.HasValue && _inputs.Count >= _workLimit)
                    {
                        Debug.WriteLine($"Reader:\tToo many blobs ({_inputs.Count}), spinning while >= {_workLimit}. ThreadId = {Thread.CurrentThread.ManagedThreadId}");
                        spinner.SpinOnce();
                        continue;
                    }

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
            catch (Exception e)
            {
                OnExceptionHandler(e);
            }
        }

        private void ProceedInput(object obj)
        {
            try
            {
                if (!(obj is Func<byte[], byte[]> func))
                    throw new ArgumentException(nameof(obj));
                
                Debug.WriteLine($"Worker:\tstarted. ThreadId = {Thread.CurrentThread.ManagedThreadId}");

                var spinner = new SpinWait();
                do
                {
                    Debug.WriteLine($"Worker:\tTrying get new work item. ThreadId = {Thread.CurrentThread.ManagedThreadId}");
                    if (!_inputs.TryTake(out var blob))
                    {
                        Debug.WriteLine($"Worker:\tNo work available, spinning. ThreadId = {Thread.CurrentThread.ManagedThreadId}");
                        spinner.SpinOnce();
                        continue;
                    }

                    Debug.WriteLine($"Worker:\tWorking with blob ({blob.Offset}). ThreadId = {Thread.CurrentThread.ManagedThreadId}");
                    blob.Buffer = func(blob.Buffer);
                    _outputs.TryEnqueue(blob);
                } while (IsReading || _inputs.Any());
            }
            catch (Exception e)
            {
                OnExceptionHandler(e);
            }
        }

        private void WriteOutput(object obj)
        {
            try
            {
                if (!(obj is Stream output))
                    throw new ArgumentException(nameof(obj));
                
                var spinner = new SpinWait();
                var size = 0;

                Debug.WriteLine($"Writer:\tstarted. ThreadId = {Thread.CurrentThread.ManagedThreadId}");
                while (IsReading || IsWorking || _outputs.Any())
                {
                    if (!_outputs.TryPeek(out var blob))
                    {
                        Debug.WriteLine($"Writer:\tNo blobs available to write, spinning. ThreadId = {Thread.CurrentThread.ManagedThreadId}");
                        spinner.SpinOnce();
                        continue;
                    }

                    Debug.WriteLine($"Writer:\tTrying to get new blob to write. ThreadId = {Thread.CurrentThread.ManagedThreadId}");
                    if (blob.Offset != _offset)
                    {
                        Debug.WriteLine($"Writer:\tBlob not in order ({blob.Offset}), waiting for ({_offset}). ThreadId = {Thread.CurrentThread.ManagedThreadId}");
                        spinner.SpinOnce();
                        continue;
                    }

                    blob = _outputs.Dequeue();
                    _outputWriter.Write(output, blob);
                    size += blob.Size;

                    OnWrite?.Invoke(this, new OnProgressEventArgs(size, $"Write:\t{SizeExtensions.GetReadableSize(size)}."));
                    Debug.WriteLine($"Writer:\tGot blob ({blob.Offset}), writing. ThreadId = {Thread.CurrentThread.ManagedThreadId}");

                    Interlocked.Increment(ref _offset);
                }
            }
            catch (Exception e)
            {
                OnExceptionHandler(e);
            }
        }

        private void Run(Stream input, Stream output, Func<byte[], byte[]> func)
        {
            Debug.WriteLine($"Compression pipeline started. ThreadId = {Thread.CurrentThread.ManagedThreadId}");
            _reader.Start(input);
            _workers.ForEach(t => t.Start(func));
            _writer.Start(output);

            SpinWait.SpinUntil(() => !(IsReading || IsWorking || IsWriting));

            Debug.WriteLine($"Compression pipeline completed. ThreadId = {Thread.CurrentThread.ManagedThreadId}");
            Dispose(false);
        }

        IReaderPipeline<Stream, IEnumerable<Blob>> IReaderPipeline<Stream, IEnumerable<Blob>>.Reader(IReader<Stream, IEnumerable<Blob>> input) =>
            Reader(input);

        IWriterPipeline<Stream, Blob> IWriterPipeline<Stream, Blob>.Writer(IWriter<Stream, Blob> input) =>
            Writer(input);

        public CompressionPipeline Reader(IReader<Stream, IEnumerable<Blob>> input)
        {
            _inputReader = input;
            return this;
        }

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

        public void Dispose() => Dispose(true);

        private void Dispose(bool disposing)
        {
            _workers.ForEach(t => t.Abort());
            _reader?.Abort();
            _writer?.Abort();

            if (disposing)
            {
                _workers.Clear();
                _reader = null;
                _writer = null;
            }

            _inputs = new ConcurrentBag<Blob>();
            _outputs = new BlockingPriorityQueue<Blob>();
            _exceptions.Clear();
        }

        public CompressionPipeline(int threadsCount = 1, int? workLimit = null)
        {
            _workLimit = workLimit;
            _reader = new Thread(ReadStream);
            _writer = new Thread(WriteOutput);
            _workers = Enumerable.Range(0, threadsCount).Select(x => new Thread(ProceedInput)).ToList();
        }
    }
}