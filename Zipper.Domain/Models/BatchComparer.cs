using System.Collections.Generic;

namespace Zipper.Domain.Models
{
    /// <summary>
    /// Class to compare batches by offset.
    /// </summary>
    public class BatchComparer : IComparer<Batch>
    {
        /// <summary>
        /// Compare batches by offset.
        /// </summary>
        /// <param name="x">First batch.</param>
        /// <param name="y">Second batch.</param>
        /// <returns>
        /// 0 if batches equals by offset or reference,
        /// -1 if first offset less, than second,
        /// 1 if second offset less, than first.</returns>
        public int Compare(Batch x, Batch y)
        {
            if (ReferenceEquals(x, y))
                return 0;
            
            if (ReferenceEquals(null, y))
                return 1;
            
            if (ReferenceEquals(null, x))
                return -1;
            
            return x.Offset.CompareTo(y.Offset);
        }
    }
}