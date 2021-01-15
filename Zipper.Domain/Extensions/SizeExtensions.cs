using System;

namespace Zipper.Domain.Extensions
{
    internal static class SizeExtensions
    {
        private static readonly string[] Sizes = {"B", "KB", "MB", "GB", "TB"}; 

        public static string GetReadableSize(long i)
        {
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