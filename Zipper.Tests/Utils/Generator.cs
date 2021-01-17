using System;
using System.Collections.Generic;

namespace Zipper.Tests.Utils
{
    public static class Generators
    {
        public static IEnumerable<T> Generate<T>(int count, Func<T> generator)
        {
            for (var i = 0; i < count; i++)
                yield return generator();
        }
    }
}