using System;

namespace Zipper.Domain.Pipeline.GZip
{
    /// <summary>
    /// Base class for GZip-batch reader/writer.
    /// </summary>
    public abstract class GZipBatchStreamBase
    {
        /// <summary>
        /// Magic guid to identify blocks of zipped data.
        /// </summary>
        protected static readonly Guid MagicGuid = Guid.Parse("8A002D01-A722-4F29-9B9B-0E854CA9E37B");

        /// <summary>
        /// Magic data to identify blocks of zipped data.
        /// </summary>
        protected static readonly byte[] MagicData = MagicGuid.ToByteArray();

        /// <summary>
        /// Magic data length to identify blocks of zipped data.
        /// </summary>
        protected static readonly int MagicLength = MagicData.Length;
    }
}