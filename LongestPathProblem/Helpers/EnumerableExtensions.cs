using System.Collections.Generic;
using System.Linq;
// ReSharper disable PossibleMultipleEnumeration

namespace LongestPathProblem.Helpers
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<(T, T)> Pairs<T>(this IEnumerable<T> input) => 
            input.Zip(input.Skip(1), (a, b) => (a, b));
    }
}