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
    /// <summary>
    /// Pipeline to read/write/convert streams.
    /// </summary>
    public class StreamPipeline : IDisposable
    {
        /// <summary>
        /// Reader thread.
        /// </summary>
        private Thread _reader;

        /// <summary>
        /// Writer thread.
        /// </summary>
        private Thread _writer;

        /// <summary>
        /// Current offset for writing batches.
        /// </summary>
        private int _offset;

        /// <summary>
        /// Maximum work limit for workers threads.
        /// </summary>
        private readonly int? _workLimit;

        /// <summary>
        /// Workers threads count.
        /// </summary>
        private readonly int _threads;

        /// <summary>
        /// Collection of workers threads.
        /// </summary>
        private List<Thread> _workers = new List<Thread>();

        /// <summary>
        /// Read data to proceed.
        /// </summary>
        private ConcurrentBag<Batch> _inputs = new ConcurrentBag<Batch>();

        /// <summary>
        /// Proceeded data for output.
        /// </summary>
        private IQueue<Batch> _outputs = new BlockingPriorityQueue<Batch>(new BatchComparer());

        /// <summary>
        /// Reader instance.
        /// </summary>
        private IReader<System.IO.Stream, IEnumerable<byte[]>> _inputReader;

        /// <summary>
        /// Writer instance.
        /// </summary>
        private IWriter<System.IO.Stream, byte[]> _outputWriter;

        /// <summary>
        /// Converter instance.
        /// </summary>
        private IConverter<byte[], byte[]> _converter;

        /// <summary>
        /// Event on read action.
        /// </summary>
        public event OnProgressHandler OnRead;

        /// <summary>
        /// Event on write action.
        /// </summary>
        public event OnProgressHandler OnWrite;

        /// <summary>
        /// Collection of occured exceptions during pipeline execution.
        /// </summary>
        private readonly List<Exception> _exceptions = new List<Exception>();

        /// <summary>
        /// Exception generator.
        /// Returns exception if any occured, else null.
        /// </summary>
        private AggregateException Exception => _exceptions.Any() ? new AggregateException(_exceptions) : null;
        
        /// <summary>
        /// Is reading thread alive and working.
        /// </summary>
        public bool IsReading => _reader != null && _reader.IsAlive;

        /// <summary>
        /// Is writing thread alive and working.
        /// </summary>
        public bool IsWriting => _writer != null && _writer.IsAlive;

        /// <summary>
        /// Is workers thread alive and working.
        /// </summary>
        public bool IsWorking => _workers.Any(x => x.IsAlive);

        /// <summary>
        /// Is any errors during pipeline execution.
        /// </summary>
        public bool IsError => _exceptions.Any();

        /// <summary>
        /// Is pipeline set up correctly.
        /// </summary>
        private bool IsValid => _inputReader != null && _outputWriter != null;

        /// <summary>
        /// Try catch and store any exceptions during action.
        /// </summary>
        /// <param name="action">Action.</param>
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

        /// <summary>
        /// Throws exceptions if any happen during execution.
        /// </summary>
        /// <exception cref="AggregateException">Throws exception that happen during execution.</exception>
        private void ThrowIfException()
        {
            var exception = Exception;
            if (exception != null)
                throw exception;
        }

        /// <summary>
        /// Reads data from input stream.
        /// </summary>
        /// <param name="input">Input stream.</param>
        private void ReadStream(System.IO.Stream input) => TryCatch(() =>
        {
            var size = 0;
            var offset = 0;
            var spinner = new SpinWait();
            using var enumerator = _inputReader.Read(input).GetEnumerator();

            Debug.WriteLine($"Reader:\tstarted. ThreadId = {Thread.CurrentThread.ManagedThreadId}");

            var next = enumerator.MoveNext();
            while (!IsError && next)
            {
                var work = _inputs.Count;
                if (work >= _workLimit)
                {
                    Debug.WriteLine($"Reader:\tToo many batchs in work ({_inputs.Count}), spinning while >= {_workLimit}. ThreadId = {Thread.CurrentThread.ManagedThreadId}");
                    spinner.SpinOnce();
                    continue;
                }

                var current = enumerator.Current;
                if (current != null)
                {
                    Debug.WriteLine($"Reader:\tGot batch ({offset}). ThreadId = {Thread.CurrentThread.ManagedThreadId}");
                    _inputs.Add(new Batch { Offset = offset++, Buffer = current });
                    size += current.Length;

                    OnRead?.Invoke(this, new OnProgressEventArgs(work, size, $"Read:\t{SizeExtensions.GetReadableSize(size)}."));
                }

                next = enumerator.MoveNext();
            }
        });

        /// <summary>
        /// Proceed data from reader.
        /// </summary>
        private void ProceedInput() => TryCatch(() =>
        {
            Debug.WriteLine($"Worker:\tstarted. ThreadId = {Thread.CurrentThread.ManagedThreadId}");
            
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
                if (_converter != null)
                    batch.Buffer = _converter.Convert(batch.Buffer);

                _outputs.TryEnqueue(batch);
            } while (!IsError && (IsReading || _inputs.Any()));
        });

        /// <summary>
        /// Proceed data from converter to output stream.
        /// </summary>
        /// <param name="output">Output stream.</param>
        private void WriteOutput(System.IO.Stream output) => TryCatch(() =>
        {
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

                OnWrite?.Invoke(this, new OnProgressEventArgs(_outputs.Count, size, $"Write:\t{SizeExtensions.GetReadableSize(size)}."));
                Debug.WriteLine($"Writer:\tGot batch ({batch.Offset}), writing. ThreadId = {Thread.CurrentThread.ManagedThreadId}");

                Interlocked.Increment(ref _offset);
            }
        });

        /// <summary>
        /// Set's up reader.
        /// </summary>
        /// <param name="reader">Reader.</param>
        /// <returns>Pipeline instance.</returns>
        public StreamPipeline Reader(IReader<System.IO.Stream, IEnumerable<byte[]>> reader)
        {
            _inputReader = reader;
            return this;
        }

        /// <summary>
        /// Set's up writer.
        /// </summary>
        /// <param name="writer">Writer.</param>
        /// <returns>Pipeline instance.</returns>
        public StreamPipeline Writer(IWriter<System.IO.Stream, byte[]> writer)
        {
            _outputWriter = writer;
            return this;
        }

        /// <summary>
        /// Set's up converter.
        /// </summary>
        /// <param name="converter">Converter.</param>
        /// <returns>Pipeline instance.</returns>
        public StreamPipeline Converter(IConverter<byte[], byte[]> converter)
        {
            _converter = converter;
            return this;
        }

        /// <summary>
        /// Proceed streams.
        /// </summary>
        /// <param name="input">Input stream.</param>
        /// <param name="output">Output stream.</param>
        /// <exception cref="ArgumentException">Throws exception if reader/writer not set or streams are empty.</exception>
        public void Proceed(System.IO.Stream input, System.IO.Stream output)
        {
            if (input == null)
                throw new ArgumentException(nameof(input));
            if (output == null)
                throw new ArgumentException(nameof(output));
            if (!IsValid)
                throw new ArgumentException("Reader or writer is not set up.");

            try
            {
                Debug.WriteLine($"Compression pipeline started. ThreadId = {Thread.CurrentThread.ManagedThreadId}");
                
                _reader = new Thread(() => ReadStream(input));
                _reader.Start();
                
                _workers = Enumerable.Range(0, _threads).Select(x => new Thread(ProceedInput)).ToList();
                _workers.ForEach(t => t.Start());

                _writer = new Thread(() => WriteOutput(output));
                _writer.Start();

                SpinWait.SpinUntil(() => IsError || !(IsReading || IsWorking || IsWriting));
                ThrowIfException();

                Debug.WriteLine($"Compression pipeline completed. ThreadId = {Thread.CurrentThread.ManagedThreadId}");
            }
            finally
            {
                Dispose();
            }
        }

        /// <summary>
        /// Dispose managed resources.
        /// </summary>
        public void Dispose()
        {
            _writer = null;
            _reader = null;
            _workers.Clear();
            
            _inputs = new ConcurrentBag<Batch>();
            _outputs = new BlockingPriorityQueue<Batch>();
            _exceptions.Clear();
            _offset = 0;
        }

        /// <summary>
        /// Initialize instance of pipeline.
        /// </summary>
        /// <param name="threadsCount">Threads count for data conversion.</param>
        /// <param name="workLimit">Reader work limit. Reader will wait if it produced more data than workers/writer can proceed.</param>
        /// <exception cref="ArgumentException">Throws exception if threads count less than 1 or work limit less than 0.</exception>
        public StreamPipeline(int threadsCount = 1, int? workLimit = null)
        {
            if (threadsCount <= 0)
                throw new ArgumentException("Threads count can't be less, than 1.");

            if (workLimit <= 0)
                throw new ArgumentException("Work limit can't be less, than 0.");

            _workLimit = workLimit;
            _threads = threadsCount;
            _offset = 0;
        }
    }
}