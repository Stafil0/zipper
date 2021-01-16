using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Zipper.Domain.Collections;
using Zipper.Domain.Extensions;
using Zipper.Domain.Models;

namespace Zipper.Domain.Pipeline.Stream
{
    public class StreamPipeline : IDisposable
    {
        private Thread _reader;

        private Thread _writer;

        private int _offset;

        private readonly int? _workLimit;

        private readonly List<Thread> _workers;

        private ConcurrentBag<Models.Batch> _inputs = new ConcurrentBag<Models.Batch>();

        private BlockingPriorityQueue<Models.Batch> _outputs = new BlockingPriorityQueue<Models.Batch>(new BatchComparer());

        private IReader<System.IO.Stream, IEnumerable<byte[]>> _inputReader;

        private IWriter<System.IO.Stream, byte[]> _outputWriter;
        
        private IConverter<byte[], byte[]> _converter;
        
        public event OnProgressHandler OnRead;

        public event OnProgressHandler OnWrite;

        public delegate void OnProgressHandler(object sender, OnProgressEventArgs args);

        private readonly List<Exception> _exceptions = new List<Exception>();

        private AggregateException Exception => _exceptions.Any() ? new AggregateException(_exceptions) : null;
        
        public bool IsReading => _reader != null && _reader.IsAlive;

        public bool IsWriting => _writer != null && _writer.IsAlive;

        public bool IsWorking => _workers.Any(x => x.IsAlive);

        public bool IsError => _exceptions.Any();

        private bool IsValid => _inputReader != null && _outputWriter != null;

        private void TryCatch(Action action)
        {
            try
            {
                action.Invoke();
            }
            catch (Exception ex)
            {
                _exceptions.Add(ex);
            }
        }

        private void ThrowIfException()
        {
            var exception = Exception;
            if (exception != null)
                throw exception;
        }

        private void ReadStream(object obj) => TryCatch(() =>
        {
            if (!(obj is System.IO.Stream input))
                throw new ArgumentException(nameof(obj));

            var size = 0;
            var offset = 0;
            var spinner = new SpinWait();
            using var enumerator = _inputReader.Read(input).GetEnumerator();

            Debug.WriteLine($"Reader:\tstarted. ThreadId = {Thread.CurrentThread.ManagedThreadId}");

            var next = enumerator.MoveNext();
            while (!IsError && next)
            {
                if (_workLimit.HasValue && _inputs.Count >= _workLimit)
                {
                    Debug.WriteLine($"Reader:\tToo many batchs in work ({_inputs.Count}), spinning while >= {_workLimit}. ThreadId = {Thread.CurrentThread.ManagedThreadId}");
                    spinner.SpinOnce();
                    continue;
                }

                var current = enumerator.Current;
                if (current != null)
                {
                    Debug.WriteLine($"Reader:\tGot batch ({offset}). ThreadId = {Thread.CurrentThread.ManagedThreadId}");
                    _inputs.Add(new Models.Batch { Offset = offset++, Buffer = current });
                    size += current.Length;

                    OnRead?.Invoke(this, new OnProgressEventArgs(size, $"Read:\t{SizeExtensions.GetReadableSize(size)}."));
                }

                next = enumerator.MoveNext();
            }
        });

        private void ProceedInput(object obj) => TryCatch(() =>
        {
            Debug.WriteLine($"Worker:\tstarted. ThreadId = {Thread.CurrentThread.ManagedThreadId}");

            var converter = obj as IConverter<byte[], byte[]>;
            var spinner = new SpinWait();
            do
            {
                Debug.WriteLine($"Worker:\tTrying get new work item. ThreadId = {Thread.CurrentThread.ManagedThreadId}");
                if (!_inputs.TryTake(out var batch))
                {
                    Debug.WriteLine($"Worker:\tNo work available, spinning. ThreadId = {Thread.CurrentThread.ManagedThreadId}");
                    spinner.SpinOnce();
                    continue;
                }

                Debug.WriteLine($"Worker:\tWorking with batch ({batch.Offset}). ThreadId = {Thread.CurrentThread.ManagedThreadId}");
                if (converter != null)
                    batch.Buffer = converter.Convert(batch.Buffer);

                _outputs.TryEnqueue(batch);
            } while (!IsError && (IsReading || _inputs.Any()));
        });

        private void WriteOutput(object obj) => TryCatch(() =>
        {
            if (!(obj is System.IO.Stream output))
                throw new ArgumentException(nameof(obj));

            var spinner = new SpinWait();
            var size = 0;

            Debug.WriteLine($"Writer:\tstarted. ThreadId = {Thread.CurrentThread.ManagedThreadId}");
            while (!IsError && (IsReading || IsWorking || _outputs.Any()))
            {
                if (!_outputs.TryPeek(out var batch))
                {
                    Debug.WriteLine($"Writer:\tNo batchs available to write, spinning. ThreadId = {Thread.CurrentThread.ManagedThreadId}");
                    spinner.SpinOnce();
                    continue;
                }

                Debug.WriteLine($"Writer:\tTrying to get new batch to write. ThreadId = {Thread.CurrentThread.ManagedThreadId}");
                if (batch.Offset != _offset)
                {
                    Debug.WriteLine($"Writer:\tbatch not in order ({batch.Offset}), waiting for ({_offset}). ThreadId = {Thread.CurrentThread.ManagedThreadId}");
                    spinner.SpinOnce();
                    continue;
                }

                batch = _outputs.Dequeue();
                _outputWriter.Write(output, batch.Buffer);
                size += batch.Size;

                OnWrite?.Invoke(this, new OnProgressEventArgs(size, $"Write:\t{SizeExtensions.GetReadableSize(size)}."));
                Debug.WriteLine($"Writer:\tGot batch ({batch.Offset}), writing. ThreadId = {Thread.CurrentThread.ManagedThreadId}");

                Interlocked.Increment(ref _offset);
            }
        });

        public StreamPipeline Reader(IReader<System.IO.Stream, IEnumerable<byte[]>> input)
        {
            _inputReader = input;
            return this;
        }

        public StreamPipeline Writer(IWriter<System.IO.Stream, byte[]> output)
        {
            _outputWriter = output;
            return this;
        }

        public StreamPipeline Converter(IConverter<byte[], byte[]> converter)
        {
            _converter = converter;
            return this;
        }

        public void Proceed(System.IO.Stream input, System.IO.Stream output)
        {
            try
            {
                if (!IsValid)
                    throw new ArgumentException("Reader or writer is not set up.");

                Debug.WriteLine($"Compression pipeline started. ThreadId = {Thread.CurrentThread.ManagedThreadId}");
                _reader.Start(input);
                _workers.ForEach(t => t.Start(_converter));
                _writer.Start(output);

                SpinWait.SpinUntil(() => IsError || !(IsReading || IsWorking || IsWriting));
                ThrowIfException();

                Debug.WriteLine($"Compression pipeline completed. ThreadId = {Thread.CurrentThread.ManagedThreadId}");
            }
            finally
            {
                Dispose(false);
            }
        }

        public void Dispose() => Dispose(true);

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                _workers.ForEach(t => t.Abort());
                _workers.Clear();

                _reader?.Abort();
                _reader = null;

                _writer?.Abort();
                _writer = null;
            }

            _exceptions.Clear();
            _inputs = new ConcurrentBag<Models.Batch>();
            _outputs = new BlockingPriorityQueue<Models.Batch>();
        }

        public StreamPipeline(int threadsCount = 1, int? workLimit = null)
        {
            if (threadsCount <= 0)
                throw new ArgumentException("Threads count can't be less, than 1.");

            if (workLimit <= 0)
                throw new ArgumentException("Work limit can't be less, than 0.");

            _workLimit = workLimit;
            _reader = new Thread(ReadStream);
            _writer = new Thread(WriteOutput);
            _workers = Enumerable.Range(0, threadsCount).Select(x => new Thread(ProceedInput)).ToList();
        }
    }
}