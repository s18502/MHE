﻿using System.Collections.Generic;
using System.Numerics;

namespace LongestPathProblem
{
    public static class NumericHelpers
    {
        public static IEnumerable<int> ToArbitrarySystem(this BigInteger decimalNumber, int radix)
        {
            var result = new Stack<int>();

            if (decimalNumber == 0)
                return new[] { 0};

            while (decimalNumber != 0)
            {
                var remainder = (int)(decimalNumber % radix);
                result.Push(remainder);
                decimalNumber = decimalNumber / radix;
            }

            return result;
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