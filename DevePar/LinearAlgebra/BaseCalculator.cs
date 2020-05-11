using DevePar.Galois;
using DevePar.MathHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DevePar.LinearAlgebra
{
    public static class BaseCalculator
    {
        public static IEnumerable<Field> CalcBase(int gfSize)
        {
            if (gfSize > 0 && !IsPowerOfTwo((uint)gfSize))
            {
                throw new ArgumentException("gfSize should be a power of 2", nameof(gfSize));
            }

            var itemsToSkip = CalcItemsToSkip(gfSize).ToList();

            int end = MathHelper.IntPow(2, (uint)gfSize);
            int n = 1;
            var b = new Field(2);

            while (n < end - 1)
            {
                while (itemsToSkip.Any(t => n % t == 0))
                {
                    n++;
                }

                yield return Field.pow(b, (byte)n);

                n++;
            }
        }

        public static IEnumerable<int> CalcItemsToSkip(int gfSize)
        {
            if (gfSize > 0 && !IsPowerOfTwo((uint)gfSize))
            {
                throw new ArgumentException("gfSize should be a power of 2", nameof(gfSize));
            }

            uint i = 1;
            while (i < gfSize)
            {
                var data = (MathHelper.IntPow(2, i) + 1);
                yield return data;
                i *= 2;
            }
        }

        private static bool IsPowerOfTwo(uint x)
        {
            return (x != 0) && ((x & (x - 1)) == 0);
        }
    }
}
