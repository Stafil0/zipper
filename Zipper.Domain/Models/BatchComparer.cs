using System.Collections.Generic;

namespace Zipper.Domain.Models
{
    public class BatchComparer : IComparer<Batch>
    {
        public int Compare(Batch x, Batch y)
        {
            if (ReferenceEquals(x, y))
                return 0;
            
            if (ReferenceEquals(null, y))
                return 1;
            
            if (ReferenceEquals(null, x))
                return -1;
            
            return y.Offset.CompareTo(x.Offset);
        }
    }
}