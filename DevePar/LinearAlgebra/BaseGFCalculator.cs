﻿using DevePar.Galois;
using DevePar.MathHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DevePar.LinearAlgebra
{
    public static class BaseGFCalculator
    {
        public static IEnumerable<GField> CalculateBase(GFTable gfTable)
        {
            if (gfTable.Power == 8)
            {
                return CalcBaseGF8(gfTable);
            }
            else if (gfTable.Power == 16)
            {
                return CalcBase3(gfTable);
            }
            throw new InvalidOperationException("This GFTable type is not supported");
        }


        private static IEnumerable<GField> CalcBaseGF8(GFTable gfTable)
        {
            for (uint i = 1; i < gfTable.Limit; i++)
            {
                yield return gfTable.CreateField(i);
            }
        }

        private static IEnumerable<GField> CalcBase(GFTable gfTable)
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

        private static IEnumerable<GField> CalcBase2(GFTable gfTable)
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

        private static IEnumerable<GField> CalcBase3(GFTable gfTable)
        {
            uint logbase = 0;

            var limit = gfTable.Limit;

            while (true)
            {

                // Determine the next useable base value.
                // Its log must must be relatively prime to 65535
                while (GCD(limit, logbase) != 1)
                {
                    logbase++;
                }
                if (logbase >= limit)
                {
                    break;
                }
                var gfieldValue = gfTable.Alog(logbase++);
                var gfield = gfTable.CreateField(gfieldValue);

                yield return gfield;
            }
        }

        private static IEnumerable<int> CalcItemsToSkip(uint gfSize)
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
