using DevePar.Galois;
using DevePar.ParityAlgorithms;
using DevePar.Tests.TestHelpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace DevePar.Tests.ParityGFAlgorithms
{
    public class ParityGFAlgorithmFacts
    {
        private static readonly GFTable gfTable = GFTable.GFTable16;

        [Fact]
        public void RestoresMissingDataForDataLength1()
        {
            int dataBlockCount = 5;
            int parityBlockCount = 2;
            int dataLength = 1;
            RunRepairTest(gfTable, dataBlockCount, parityBlockCount, dataLength);
        }

        [Fact]
        public void RestoresMissingDataForDataLength5000()
        {
            int dataBlockCount = 5;
            int parityBlockCount = 2;
            int dataLength = 5000;
            RunRepairTest(gfTable, dataBlockCount, parityBlockCount, dataLength);
        }

        [Fact]
        public void RestoresMissingDataForDataFor2BlocksAnd2Parity()
        {
            int dataBlockCount = 2;
            int parityBlockCount = 2;
            int dataLength = 1;
            RunRepairTest(gfTable, dataBlockCount, parityBlockCount, dataLength);
        }

        [Fact]
        public void RestoresMissingDataForDataFor3BlocksAnd3Parity()
        {
            int dataBlockCount = 3;
            int parityBlockCount = 3;
            int dataLength = 1;
            RunRepairTest(gfTable, dataBlockCount, parityBlockCount, dataLength);
        }

        [Fact]
        public void RestoresMissingDataForDataFor4BlocksAnd4Parity()
        {
            int dataBlockCount = 4;
            int parityBlockCount = 4;
            int dataLength = 1;
            RunRepairTest(gfTable, dataBlockCount, parityBlockCount, dataLength);
        }

        [Fact]
        public void RestoresMissingDataForDataFor5BlocksAnd5Parity()
        {
            int dataBlockCount = 5;
            int parityBlockCount = 5;
            int dataLength = 1;
            RunRepairTest(gfTable, dataBlockCount, parityBlockCount, dataLength);
        }

        [Fact]
        public void RestoresMissingDataForDataForUltraTest()
        {
            int dataLength = 1;

            int maxDataBlocksToTest = 8;

            for (int dataBlockCount = 1; dataBlockCount <= maxDataBlocksToTest; dataBlockCount++)
            {
                for (int parityBlockCount = 1; parityBlockCount <= dataBlockCount; parityBlockCount++)
                {
                    RunRepairTest(gfTable, dataBlockCount, parityBlockCount, dataLength);
                }
            }
        }

        //[Fact]
        //public void RestoresMissingDataForDataFor5BlocksAnd5Parity()
        //{
        //    int dataBlockCount = 5;
        //    int parityBlockCount = 5;
        //    int dataLength = 5000;
        //    var gfTable = GFTable.GFTable16;
        //    RunRepairTest(gfTable, dataBlockCount, parityBlockCount, dataLength);
        //}

        [Fact]
        public void TestSpecificScenario()
        {
            //This scenario works if using:
            //var val = Field.pow(baseList[row], column);

            int dataBlockCount = 5;
            int parityBlockCount = 5;
            int dataLength = 1;

            var expectedData = GenerateTestDataHelper.ConvertToUint(GenerateTestDataHelper.GenerateTestData(dataBlockCount, dataLength));

            var data = GenerateTestDataHelper.ConvertToUint(GenerateTestDataHelper.GenerateTestData(dataBlockCount, dataLength));
            var parityData = ParityGFAlgorithm.GenerateParityData3(gfTable, data, parityBlockCount);
            var combinedData = data.Concat(parityData).ToList();

            combinedData[0].Data = null;
            combinedData[3].Data = null;
            combinedData[4].Data = null;
            combinedData[5].Data = null;

            //var matrix = ParityGFAlgorithm.CreateParityMatrix2(gfTable, expectedData.Count, parityBlockCount);
            //Console.WriteLine($"Matrix: {matrix}"); 

            var repairedData = ParityGFAlgorithm.RecoverData3(gfTable, data, parityData, parityBlockCount);

            VerifyData(expectedData, repairedData);
        }

        [Fact]
        public void TestSpecificScenario2()
        {
            //This scenario works if using:
            //var val = Field.pow(baseList[column], row);

            int dataBlockCount = 5;
            int parityBlockCount = 5;
            int dataLength = 1;


            var expectedData = GenerateTestDataHelper.ConvertToUint(GenerateTestDataHelper.GenerateTestData(dataBlockCount, dataLength));

            var data = GenerateTestDataHelper.ConvertToUint(GenerateTestDataHelper.GenerateTestData(dataBlockCount, dataLength));
            var parityData = ParityGFAlgorithm.GenerateParityData3(gfTable, data, parityBlockCount);
            var combinedData = data.Concat(parityData).ToList();

            combinedData[1].Data = null;
            combinedData[2].Data = null;
            combinedData[3].Data = null;
            combinedData[6].Data = null;
            combinedData[7].Data = null;


            //var matrix = ParityGFAlgorithm.CreateParityMatrix2(gfTable, expectedData.Count, parityBlockCount);
            //Console.WriteLine($"Matrix: {matrix}");


            var repairedData = ParityGFAlgorithm.RecoverData3(gfTable, data, parityData, parityBlockCount);


            VerifyData(expectedData, repairedData);
        }

        [Fact]
        public void TestSpecificScenario3()
        {
            int dataBlockCount = 5;
            int parityBlockCount = 3;
            int dataLength = 1;


            var expectedData = GenerateTestDataHelper.ConvertToUint(GenerateTestDataHelper.GenerateTestData(dataBlockCount, dataLength));

            var data = GenerateTestDataHelper.ConvertToUint(GenerateTestDataHelper.GenerateTestData(dataBlockCount, dataLength));
            var parityData = ParityGFAlgorithm.GenerateParityData3(gfTable, data, parityBlockCount);
            var combinedData = data.Concat(parityData).ToList();

            combinedData[0].Data = null;
            combinedData[2].Data = null;
            combinedData[5].Data = null;


            //var matrix = ParityGFAlgorithm.CreateParityMatrix(gfTable, expectedData.Count, parityBlockCount);
            //Console.WriteLine($"Matrix: {matrix}");

            var repairedData = ParityGFAlgorithm.RecoverData3(gfTable, data, parityData, parityBlockCount);

            VerifyData(expectedData, repairedData);
        }

        [Fact]
        public void TestSpecificScenario4()
        {
            int dataBlockCount = 5;
            int parityBlockCount = 5;
            int dataLength = 1;


            var expectedData = GenerateTestDataHelper.ConvertToUint(GenerateTestDataHelper.GenerateTestData(dataBlockCount, dataLength));

            var data = GenerateTestDataHelper.ConvertToUint(GenerateTestDataHelper.GenerateTestData(dataBlockCount, dataLength));
            var parityData = ParityGFAlgorithm.GenerateParityData3(gfTable, data, parityBlockCount);
            var combinedData = data.Concat(parityData).ToList();

            combinedData[0].Data = null;
            combinedData[2].Data = null;
            combinedData[3].Data = null;
            combinedData[5].Data = null;
            combinedData[6].Data = null;


            var repairedData = ParityGFAlgorithm.RecoverData3(gfTable, data, parityData, parityBlockCount);

            VerifyData(expectedData, repairedData);
        }

        [Fact]
        public void TestSpecificScenario5()
        {
            int dataBlockCount = 6;
            int parityBlockCount = 6;
            int dataLength = 1;


            var expectedData = GenerateTestDataHelper.ConvertToUint(GenerateTestDataHelper.GenerateTestData(dataBlockCount, dataLength));

            var data = GenerateTestDataHelper.ConvertToUint(GenerateTestDataHelper.GenerateTestData(dataBlockCount, dataLength));
            var parityData = ParityGFAlgorithm.GenerateParityData3(gfTable, data, parityBlockCount);
            var combinedData = data.Concat(parityData).ToList();

            combinedData[2].Data = null;
            combinedData[3].Data = null;
            combinedData[5].Data = null;
            combinedData[7].Data = null;
            combinedData[8].Data = null;
            combinedData[10].Data = null;


            var repairedData = ParityGFAlgorithm.RecoverData3(gfTable, data, parityData, parityBlockCount);

            VerifyData(expectedData, repairedData);
        }

        [Fact]
        public void TestSpecificScenario6()
        {
            int dataBlockCount = 2;
            int parityBlockCount = 2;
            int dataLength = 1;


            var expectedData = GenerateTestDataHelper.ConvertToUint(GenerateTestDataHelper.GenerateTestData(dataBlockCount, dataLength));

            var data = GenerateTestDataHelper.ConvertToUint(GenerateTestDataHelper.GenerateTestData(dataBlockCount, dataLength));
            var parityData = ParityGFAlgorithm.GenerateParityData3(gfTable, data, parityBlockCount);
            var combinedData = data.Concat(parityData).ToList();

            combinedData[0].Data = null;
            combinedData[1].Data = null;


            //var matrix = ParityGFAlgorithm.CreateParityMatrix(gfTable, expectedData.Count, parityBlockCount);
            //Console.WriteLine($"Matrix: {matrix}");

            var repairedData = ParityGFAlgorithm.RecoverData3(gfTable, data, parityData, parityBlockCount);

            VerifyData(expectedData, repairedData);
        }

        [Fact]
        public void TestSpecificScenario7()
        {
            int dataBlockCount = 4;
            int parityBlockCount = 4;
            int dataLength = 1;


            var expectedData = GenerateTestDataHelper.ConvertToUint(GenerateTestDataHelper.GenerateTestData(dataBlockCount, dataLength));

            var data = GenerateTestDataHelper.ConvertToUint(GenerateTestDataHelper.GenerateTestData(dataBlockCount, dataLength));
            var parityData = ParityGFAlgorithm.GenerateParityData3(gfTable, data, parityBlockCount);
            var combinedData = data.Concat(parityData).ToList();

            combinedData[0].Data = null;
            combinedData[1].Data = null;
            combinedData[2].Data = null;
            combinedData[6].Data = null;


            //var matrix = ParityGFAlgorithm.CreateParityMatrix(gfTable, expectedData.Count, parityBlockCount);
            //Console.WriteLine($"Matrix: {matrix}");

            var repairedData = ParityGFAlgorithm.RecoverData3(gfTable, data, parityData, parityBlockCount);

            VerifyData(expectedData, repairedData);
        }


        [Fact]
        public void TestSpecificScenario8()
        {
            //int dataBlockCount = 2;
            //int parityBlockCount = 2;
            //int dataLength = 1;


            //var expectedData = GenerateTestDataHelper.ConvertToUint(GenerateTestDataHelper.GenerateTestData(dataBlockCount, dataLength));

            //var data = GenerateTestDataHelper.ConvertToUint(GenerateTestDataHelper.GenerateTestData(dataBlockCount, dataLength));
            //var parityData = ParityGFAlgorithm.GenerateParityData3(gfTable, data, parityBlockCount);
            //var parityData2 = ParityGFAlgorithm.GenerateParityData2(gfTable, data, parityBlockCount);
            //var combinedData = data.Concat(parityData).ToList();


            //combinedData[0].Data = null;
            //combinedData[1].Data = null;


            ////var matrix = ParityGFAlgorithm.CreateParityMatrix(gfTable, expectedData.Count, parityBlockCount);
            ////Console.WriteLine($"Matrix: {matrix}");

            //var repairedData = ParityGFAlgorithm.RecoverData3(gfTable, data, parityData, parityBlockCount);
            //var repairedData2 = ParityGFAlgorithm.RecoverData2(gfTable, data, parityData, parityBlockCount);

            //VerifyData(expectedData, repairedData);
            //VerifyData(expectedData, repairedData2);
        }

        [Fact]
        public void TestSpecificScenario9()
        {
            int dataBlockCount = 3;
            int parityBlockCount = 3;
            int dataLength = 1;


            var expectedData = GenerateTestDataHelper.ConvertToUint(GenerateTestDataHelper.GenerateTestData(dataBlockCount, dataLength));

            var data = GenerateTestDataHelper.ConvertToUint(GenerateTestDataHelper.GenerateTestData(dataBlockCount, dataLength));
            var parityData = ParityGFAlgorithm.GenerateParityData3(gfTable, data, parityBlockCount);
            var combinedData = data.Concat(parityData).ToList();

            combinedData[0].Data = null;
            combinedData[1].Data = null;
            combinedData[2].Data = null;


            //var matrix = ParityGFAlgorithm.CreateParityMatrix(gfTable, expectedData.Count, parityBlockCount);
            //Console.WriteLine($"Matrix: {matrix}");

            var repairedData = ParityGFAlgorithm.RecoverData3(gfTable, data, parityData, parityBlockCount);

            VerifyData(expectedData, repairedData);
        }

        [Fact]
        public void TestSpecificScenarioHuge()
        {
            int dataBlockCount = 64;
            int parityBlockCount = 64;
            int dataLength = 1;




            //Parallel.For(0, 1000, new ParallelOptions() { MaxDegreeOfParallelism = 32 }, (i) =>
            for (int i = 0; i < 100; i++)
            {
                var expectedData = GenerateTestDataHelper.ConvertToUint(GenerateTestDataHelper.GenerateTestData(dataBlockCount, dataLength));

                var data = GenerateTestDataHelper.ConvertToUint(GenerateTestDataHelper.GenerateTestData(dataBlockCount, dataLength));
                var parityData = ParityGFAlgorithm.GenerateParityData3(gfTable, data, parityBlockCount);
                var combinedData = data.Concat(parityData).ToList();

                var r = new Random(i);

                combinedData.Shuffle(r);
                for (int y = 0; y < parityBlockCount; y++)
                {
                    combinedData[y].Data = null;
                }


                var repairedData = ParityGFAlgorithm.RecoverData3(gfTable, data, parityData, parityBlockCount);

                VerifyData(expectedData, repairedData);
            }
            //);
        }

        private static void RunRepairTest(GFTable gfTable, int dataBlockCount, int parityBlockCount, int dataLength)
        {
            var expectedData = GenerateTestDataHelper.ConvertToUint(GenerateTestDataHelper.GenerateTestData(dataBlockCount, dataLength));



            for (int dataBlocksToDeleteCount = 1; dataBlocksToDeleteCount <= parityBlockCount; dataBlocksToDeleteCount++)
            {
                var rowsToDelete = DeleteDataHelper.DetermineAllPermutations(dataBlockCount + parityBlockCount, dataBlocksToDeleteCount);

                //for (int zzz = 0; zzz < rowsToDelete.Count; zzz++)
                Parallel.For(0, rowsToDelete.Count, new ParallelOptions() { MaxDegreeOfParallelism = 32 }, (zzz) =>
                {
                    {
                        var toDelete = rowsToDelete[zzz];

                        var data = GenerateTestDataHelper.ConvertToUint(GenerateTestDataHelper.GenerateTestData(dataBlockCount, dataLength));
                        var parityData = ParityGFAlgorithm.GenerateParityData3(gfTable, data, parityBlockCount);
                        var combinedData = data.Concat(parityData).ToList();

                        foreach (var rowToDelete in toDelete)
                        {
                            combinedData[rowToDelete].Data = null;
                        }

                        var repairedData = ParityGFAlgorithm.RecoverData3(gfTable, data, parityData, parityBlockCount);

                        VerifyData(expectedData, repairedData);
                    }
                }
                );
            }
        }

        private static void VerifyData(List<Block<byte>> expectedData, List<Block<byte>> repairedData)
        {
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

        private static void VerifyData(List<Block<uint>> expectedData, List<Block<uint>> repairedData)
        {
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


        [Fact]
        public void TestPoly()
        {
            //This scenario works if using:
            //var val = Field.pow(baseList[row], column);

            //int dataBlockCount = 4;
            //int parityBlockCount = 4;
            //int dataLength = 1;

            //var workingPolynomials = new List<int>();

            //var original = Field.polynomial;

            //for (int i = 0; i < 300; i++)
            //{
            //    Field.polynomial = i;
            //    Field.GenerateExpAndLogTables();

            //    var expectedData = GenerateTestDataHelper.ConvertToUint(GenerateTestDataHelper.GenerateTestData(dataBlockCount, dataLength));



            //    bool worked = true;
            //    for (int dataBlocksToDeleteCount = 1; dataBlocksToDeleteCount <= parityBlockCount; dataBlocksToDeleteCount++)
            //    {
            //        var rowsToDelete = DeleteDataHelper.DatasToDelete(dataBlockCount + parityBlockCount, dataBlocksToDeleteCount);

            //        for (int zzz = 0; zzz < rowsToDelete.Count; zzz++)
            //        {
            //            var toDelete = rowsToDelete[zzz];

            //            var data = GenerateTestDataHelper.ConvertToUint(GenerateTestDataHelper.GenerateTestData(dataBlockCount, dataLength));
            //            var parityData = ParityGFAlgorithm.GenerateParityData(data, parityBlockCount);
            //            var combinedData = data.Concat(parityData).ToList();

            //            foreach (var rowToDelete in toDelete)
            //            {
            //                combinedData[rowToDelete].Data = null;
            //            }

            //            if (dataBlocksToDeleteCount == 2)
            //            {

            //            }

            //            try
            //            {
            //                var repairedData = ParityGFAlgorithm.RecoverData(data, parityData, parityBlockCount);

            //                for (int y = 0; y < expectedData.Count; y++)
            //                {
            //                    var curExpectedData = expectedData[y];
            //                    var curRepairData = repairedData[y];
            //                    for (int z = 0; z < curExpectedData.Data.Length; z++)
            //                    {
            //                        var curExpectedValue = curExpectedData.Data[z];
            //                        var curRepairedValue = curRepairData.Data[z];

            //                        if (curExpectedValue != curRepairedValue)
            //                        {
            //                            worked = false;
            //                        }
            //                    }
            //                }
            //            }
            //            catch (Exception ex)
            //            {
            //                worked = false;
            //            }
            //        }
            //    }

            //    if (worked)
            //    {
            //        workingPolynomials.Add(i);
            //    }
            //}

            //Field.polynomial = original;
            //Field.GenerateExpAndLogTables();

            //var str = string.Join(",", workingPolynomials);
            //Console.WriteLine(str);

            //29,43,45,77,95,99,101,105,113,135,141,169,195,207,231,245,285,299
        }

        [Fact]
        public void WriteTestFiles()
        {
            var data = GenerateTestDataHelper.GenerateTestData(4, 1);
            for (int i = 0; i < data.Count; i++)
            {
                File.WriteAllBytes($"{i}.txt", new byte[] { data[i].Data[0] });
            }
        }

    }
}
