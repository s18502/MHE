using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using LongestPathProblem.Helpers;

namespace LongestPathProblem
{
    public static class NumericHelpers
    {
        private static readonly ObjectPool<Stack<int>> StackPool = new(() => new Stack<int>());
        
        public static IEnumerable<int> ToArbitrarySystem(this BigInteger decimalNumber, int radix)
        {
            var result = StackPool.Get();
            try
            {
                result.Clear();

                if (decimalNumber == 0)
                    return new[] { 0};

                while (decimalNumber != 0)
                {
                    var remainder = (int)(decimalNumber % radix);
                    result.Push(remainder);
                    decimalNumber /= radix;
                }

                return result.AsEnumerable();
            }
            finally
            {
                StackPool.Return(result);
            }
        }

        public static BigInteger ToDecimalArbitrarySystem(this IEnumerable<int> number, int radix)
        {
            BigInteger multiplier = 1;
            BigInteger result = 0;
	
            var numbers = new Stack<int>(number);
	
            while(numbers.TryPop(out var x)) {
                result += x * multiplier;
                multiplier *= radix;
            }
	
            return result;
        }
    }
}