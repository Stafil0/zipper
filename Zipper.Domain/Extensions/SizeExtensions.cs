using System;

namespace Zipper.Domain.Extensions
{
    public static class SizeExtensions
    {
        private static readonly string[] Sizes = {"B", "KB", "MB", "GB"}; 

        public static string GetReadableSize(long i)
        {
            if (i < 0)
                throw new ArgumentException("Size can't be less, than 0.");
            
            var len = Math.Abs(i);
            var order = 0;
            while (len >= 1024 && order < Sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }

            return $"{len:0.##} {Sizes[order]}";
        }
    }
}