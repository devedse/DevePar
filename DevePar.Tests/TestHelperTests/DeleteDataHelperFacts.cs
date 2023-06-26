using DevePar.TestHelpers;
using System;
using System.Diagnostics;
using System.Linq;
using Xunit;

namespace DevePar.Tests.TestHelperTests
{
    public class DeleteDataHelperFacts
    {
        [Fact]
        public static void ShouldGenerateTheRightDeleteData()
        {
            int totalBlocks = 15;
            int dataToDelete = 6;


            var w2 = Stopwatch.StartNew();
            var set2 = DeleteDataHelper.DetermineAllPermutationsOld(totalBlocks, dataToDelete).ToList();
            w2.Stop();

            var w1 = Stopwatch.StartNew();
            var set1 = DeleteDataHelper.DetermineAllPermutations(totalBlocks, dataToDelete);
            w1.Stop();

            Assert.Equal(set1.Count, set2.Count);

            for (int i = 0; i < set2.Count; i++)
            {
                var cur = set2[i];
                var res = set1.Count(t => t.SequenceEqual(cur));

                Assert.Equal(1, res);
            }
        }
    }
}
