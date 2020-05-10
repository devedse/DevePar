using DevePar.ParityAlgorithms;
using DevePar.Tests.TestHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace DevePar.Tests.ParityAlgorithms
{
    public class ParityAlgorithmFacts
    {
        [Fact]
        public void RestoresMissingDataForDataLength1()
        {
            int dataBlockCount = 5;
            int parityBlockCount = 2;
            int dataLength = 1;
            RunRepairTest(dataBlockCount, parityBlockCount, dataLength);
        }

        [Fact]
        public void RestoresMissingDataForDataLength5000()
        {
            int dataBlockCount = 5;
            int parityBlockCount = 2;
            int dataLength = 5000;
            RunRepairTest(dataBlockCount, parityBlockCount, dataLength);
        }

        private static void RunRepairTest(int dataBlockCount, int parityBlockCount, int dataLength)
        {
            var expectedData = GenerateTestDataHelper.GenerateTestData(dataBlockCount, dataLength);



            for (int dataBlocksToDeleteCount = 1; dataBlocksToDeleteCount <= parityBlockCount; dataBlocksToDeleteCount++)
            {
                var rowsToDelete = DeleteDataHelper.DatasToDelete(dataBlockCount + parityBlockCount, dataBlocksToDeleteCount);

                foreach (var toDelete in rowsToDelete)
                {
                    var data = GenerateTestDataHelper.GenerateTestData(dataBlockCount, dataLength);
                    var parityData = ParityAlgorithm.GenerateParityData(data, parityBlockCount);
                    var combinedData = data.Concat(parityData).ToList();

                    foreach (var rowToDelete in toDelete)
                    {
                        combinedData[rowToDelete].Data = null;
                    }

                    var repairedData = ParityAlgorithm.RecoverData(data, parityData, parityBlockCount);

                    for (int y = 0; y < expectedData.Count; y++)
                    {
                        var curExpectedData = expectedData[y];
                        var curRepairData = repairedData[y];
                        for (int z = 0; z < curExpectedData.Data.Length; z++)
                        {
                            var curExpectedValue = curExpectedData.Data[z];
                            var curRepairedValue = curRepairData.Data[z];

                            Assert.Equal(curExpectedValue, curRepairedValue);
                        }
                    }
                }
            }
        }
    }
}
