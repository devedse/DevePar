using DevePar.Galois;
using DevePar.MathHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DevePar.LinearAlgebra
{
    class BaseGFCalculator
    {
        public static IEnumerable<GField> CalcBase(GFTable gfTable)
        {
            var gfSize = (uint)gfTable.Power;
            if (gfSize > 0 && !IsPowerOfTwo(gfSize))
            {
                throw new ArgumentException("gfSize should be a power of 2", nameof(gfSize));
            }

            var itemsToSkip = CalcItemsToSkip(gfSize).ToList();

            int end = MathHelper.IntPow(2, gfSize);
            uint n = 1;
            uint b = 2;

            while (n < end - 1)
            {
                while (itemsToSkip.Any(t => n % t == 0))
                {
                    n++;
                }

                yield return gfTable.CreateField(gfTable.Pow(b, n));

                n++;
            }
        }

        public static IEnumerable<GField> CalcBase2(GFTable gfTable)
        {
            var gfSize = (uint)gfTable.Power;
            if (gfSize > 0 && !IsPowerOfTwo(gfSize))
            {
                throw new ArgumentException("gfSize should be a power of 2", nameof(gfSize));
            }

            var itemsToSkip = CalcItemsToSkip(gfSize).ToList();

            int end = MathHelper.IntPow(2, gfSize);
            uint n = 0;
            uint b = 2;

            while (n < end - 1)
            {
                yield return gfTable.CreateField(gfTable.Pow(b, n));
                n++;
                while (itemsToSkip.Any(t => n % t == 0))
                {
                    n++;
                }
            }
        }

        public static IEnumerable<int> CalcItemsToSkip(uint gfSize)
        {
            if (gfSize > 0 && !IsPowerOfTwo(gfSize))
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
