using System.Collections.Generic;

namespace Zipper.Domain.Pipeline
{
    public interface IPipeline<TReadIn, TReadOut, TWriteIn, TWriteOut>
    {
        IPipeline<TReadIn, TReadOut, TWriteIn, TWriteOut> Input(IReader<TReadIn, TReadOut> inputReader);

        IPipeline<TReadIn, TReadOut, TWriteIn, TWriteOut> Output(IWriter<TWriteIn, TWriteOut> outputReader);
    }
}