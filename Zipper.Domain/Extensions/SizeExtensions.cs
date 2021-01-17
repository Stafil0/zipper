using System;

namespace Zipper.Domain.Extensions
{
    /// <summary>
    /// Extensions for measuring size of objects.
    /// </summary>
    public static class SizeExtensions
    {
        /// <summary>
        /// Memory size formats.
        /// </summary>
        private static readonly string[] Sizes = {"B", "KB", "MB", "GB"}; 

        /// <summary>
        /// Convert <see cref="long"/>, that represents memory size to readable string.
        /// </summary>
        /// <param name="i">Memory size.</param>
        /// <returns>Memory size in readable format.</returns>
        /// <exception cref="ArgumentException">Throws exception if size less than zero</exception>
        public static string GetReadableSize(long i)
        {
            if (i < 0)
                throw new ArgumentException("Size can't be less than 0.");
            
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