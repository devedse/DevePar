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

        public static IEnumerable<GField> CalcBaseGF8(GFTable gfTable)
        {
            for (uint i = 1; i < gfTable.Limit; i++)
            {
                yield return gfTable.CreateField(i);
            }
        }

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

        public static IEnumerable<GField> CalcBase3(GFTable gfTable)
        {
            uint logbase = 0;

            var table = GFTable.GFTable8;
            var limit = table.Limit;

            var allValues = new List<GField>();

            uint count = 500;
            for (uint index = 0; index < count; index++)
            {

                // Determine the next useable base value.
                // Its log must must be relatively prime to 65535
                while (GCD(limit, logbase) != 1)
                {
                    logbase++;
                }
                if (logbase >= limit)
                {
                    return allValues;
                    //throw new Exception("ERRORRR");
                }
                var gfieldValue = table.Alog(logbase++);
                var gfield = table.CreateField(gfieldValue);
                allValues.Add(gfield);
                Console.WriteLine(gfield);
            }
            return null;
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

        public static uint GCD(uint a, uint b)
        {
            if (a != 0 && b != 0)
            {
                while (a != 0 && b != 0)
                {
                    if (a > b)
                    {
                        a = a % b;
                    }
                    else
                    {
                        b = b % a;
                    }
                }

                return a + b;
            }
            else
            {
                return 0;
            }
        }
    }
}
