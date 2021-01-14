using System.Collections.Generic;

namespace Zipper.Domain.Pipeline
{
    public interface IPipeline<TReadIn, TReadOut, TWriteIn, TWriteOut>
    {
        IPipeline<TReadIn, TReadOut, TWriteIn, TWriteOut> Reader(IReader<TReadIn, TReadOut> inputReader);

        IPipeline<TReadIn, TReadOut, TWriteIn, TWriteOut> Writer(IWriter<TWriteIn, TWriteOut> outputReader);
    }
}