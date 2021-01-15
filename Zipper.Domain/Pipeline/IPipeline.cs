using System;

namespace Zipper.Domain.Pipeline
{
    public interface IPipeline<TReadIn, TReadOut, TWriteIn, TWriteOut>
    {
        event OnProgressHandler OnRead;

        event OnProgressHandler OnWrite;

        delegate void OnProgressHandler(object sender, OnProgressEventArgs args);

        IPipeline<TReadIn, TReadOut, TWriteIn, TWriteOut> Reader(IReader<TReadIn, TReadOut> inputReader);

        IPipeline<TReadIn, TReadOut, TWriteIn, TWriteOut> Writer(IWriter<TWriteIn, TWriteOut> outputReader);
    }
}