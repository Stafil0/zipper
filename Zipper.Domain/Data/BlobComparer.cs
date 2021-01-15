using System.Collections.Generic;

namespace Zipper.Domain.Data
{
    public class BlobComparer : IComparer<Blob>
    {
        public int Compare(Blob x, Blob y)
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