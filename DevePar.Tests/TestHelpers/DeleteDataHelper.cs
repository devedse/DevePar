using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using Xunit;

namespace DevePar.Tests.TestHelpers
{
    public static class DeleteDataHelper
    {
        [Fact]
        public static void ShouldGenerateTheRightDeleteData()
        {
            int totalBlocks = 15;
            int dataToDelete = 6;
         

            var w2 = Stopwatch.StartNew();
            var set2 = DatasToDeleteOld(totalBlocks, dataToDelete).ToList();
            w2.Stop();

            var w1 = Stopwatch.StartNew();
            var set1 = DatasToDelete(totalBlocks, dataToDelete);
            w1.Stop();

            Assert.Equal(set1.Count, set2.Count);

            for (int i = 0; i < set2.Count; i++)
            {
                var cur = set2[i];
                var res = set1.Count(t => t.SequenceEqual(cur));

                Assert.Equal(1, res);
            }
        }

        //public static void DeleteData<T>(IEnumerable<Block<T>> combinedData, int countDataToDelete)
        //{
        //    var datasToDelete = DatasToDelete(combinedData.Count(), countDataToDelete);
        //    for (int i = 0; i < datasToDelete.Count; i++)
        //    {
        //        var res = datasToDelete[i];
        //        Console.WriteLine($"{i}: Deleting: {string.Join(",", res.Select(t => t.ToString()))}");
        //    }
        //}

        private static int IntPow(int x, uint pow)
        {
            int ret = 1;
            while (pow != 0)
            {
                if ((pow & 1) == 1)
                    ret *= x;
                x *= x;
                pow >>= 1;
            }
            return ret;
        }

        public static List<List<int>> DatasToDeleteOld(int totalBlockCount, int countDataToDelete)
        {
            var bitCount = IntPow(2, (uint)totalBlockCount);
            var range = Enumerable.Range(0, bitCount - 1);

            var res = range.Select(t => new BitArray(new int[] { t })).Where(t => CountTrueInBitArray(t) == countDataToDelete).Select(t => CountPosOfTrueBits(t).ToList()).ToList();
            return res;
        }



        public static List<List<int>> DatasToDelete(int totalBlockCount, int countDataToDelete)
        {
            return DatasToDeleteRecursive(totalBlockCount, countDataToDelete, new List<int>()).ToList();
        }

        public static IEnumerable<List<int>> DatasToDeleteRecursive(int totalBlockCount, int countDataToDelete, List<int> cur, int startNumber = 0)
        {
            if (cur.Count == countDataToDelete)
            {
                return new List<List<int>>() { cur };
            }

            var allLists = Enumerable.Empty<List<int>>();
            for (int i = startNumber; i < totalBlockCount - (countDataToDelete - (cur.Count + 1)); i++)
            {
                var cloned = new List<int>(countDataToDelete);
                cloned.AddRange(cur);
                cloned.Add(i);

                allLists = allLists.Concat(DatasToDeleteRecursive(totalBlockCount, countDataToDelete, cloned, i + 1));
            }

            return allLists;
        }

        private static int CountTrueInBitArray(BitArray t)
        {
            int count = 0;
            for (int i = 0; i < t.Count; i++)
            {
                if (t[i])
                {
                    count++;
                }
            }
            return count;
        }

        private static IEnumerable<int> CountPosOfTrueBits(BitArray t)
        {
            for (int i = 0; i < t.Count; i++)
            {
                if (t[i])
                {
                    yield return i;
                }
            }
        }
    }
}
