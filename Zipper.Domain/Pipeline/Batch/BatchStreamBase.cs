using System;

namespace Zipper.Domain.Pipeline.Batch
{
    public abstract class BatchStreamBase
    {
        protected static readonly Guid MagicGuid = Guid.Parse("8A002D01-A722-4F29-9B9B-0E854CA9E37B");

        protected static readonly byte[] MagicData = MagicGuid.ToByteArray();

        protected static readonly int MagicLength = MagicData.Length;
    }
}