﻿using DevePar.Galois;
using DevePar.ParityAlgorithms;
using DevePar.Tests.TestHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace DevePar.Tests.ParityGFAlgorithms
{
    public class ParityGFAlgorithmFacts
    {
        //[Fact]
        //public void RestoresMissingDataForDataLength1()
        //{
        //    int dataBlockCount = 5;
        //    int parityBlockCount = 2;
        //    int dataLength = 1;
        //    RunRepairTest(dataBlockCount, parityBlockCount, dataLength);
        //}

        //[Fact]
        //public void RestoresMissingDataForDataLength5000()
        //{
        //    int dataBlockCount = 5;
        //    int parityBlockCount = 2;
        //    int dataLength = 5000;
        //    RunRepairTest(dataBlockCount, parityBlockCount, dataLength);
        //}

        //[Fact]
        //public void RestoresMissingDataForDataFor3BlocksAnd3Parity()
        //{
        //    int dataBlockCount = 3;
        //    int parityBlockCount = 3;
        //    int dataLength = 1;
        //    RunRepairTest(dataBlockCount, parityBlockCount, dataLength);
        //}

        //[Fact]
        //public void RestoresMissingDataForDataFor4BlocksAnd4Parity()
        //{
        //    int dataBlockCount = 4;
        //    int parityBlockCount = 4;
        //    int dataLength = 1;

        //    RunRepairTest(dataBlockCount, parityBlockCount, dataLength);
        //}

        //[Fact]
        //public void RestoresMissingDataForDataFor5BlocksAnd5Parity()
        //{
        //    int dataBlockCount = 5;
        //    int parityBlockCount = 5;
        //    int dataLength = 1;
        //    RunRepairTest(dataBlockCount, parityBlockCount, dataLength);
        //}

        //[Fact]
        //public void RestoresMissingDataForDataFor5BlocksAnd5Parity()
        //{
        //    int dataBlockCount = 5;
        //    int parityBlockCount = 5;
        //    int dataLength = 5000;
        //    RunRepairTest(dataBlockCount, parityBlockCount, dataLength);
        //}

        //[Fact]
        //public void TestSpecificScenario()
        //{
        //    //This scenario works if using:
        //    //var val = Field.pow(baseList[row], column);

        //    int dataBlockCount = 5;
        //    int parityBlockCount = 5;
        //    int dataLength = 1;


        //    var expectedData = GenerateTestDataHelper.GenerateTestData(dataBlockCount, dataLength);

        //    var data = GenerateTestDataHelper.GenerateTestData(dataBlockCount, dataLength);
        //    var parityData = ParityGFAlgorithm.GenerateParityData(data, parityBlockCount);
        //    var combinedData = data.Concat(parityData).ToList();

        //    combinedData[0].Data = null;
        //    combinedData[3].Data = null;
        //    combinedData[4].Data = null;
        //    combinedData[5].Data = null;


        //    var matrix = ParityGFAlgorithm.CreateParityMatrix(expectedData, parityBlockCount);
        //    Console.WriteLine($"Matrix: {matrix}");
        //    //1 0 0 0 0
        //    //0 1 0 0 0
        //    //0 0 1 0 0
        //    //0 0 0 1 0
        //    //0 0 0 0 1
        //    //1 2 4 8 16
        //    //1 4 16 64 29
        //    //1 16 29 205 76
        //    //1 128 19 117 24
        //    //1 29 76 143 157

        //    var repairedData = ParityGFAlgorithm.RecoverData(data, parityData, parityBlockCount);

        //    //0 1 0 0 0
        //    //0 0 1 0 0
        //    //1 4 16 64 29
        //    //1 16 29 205 76
        //    //1 128 19 117 24

        //    VerifyData(expectedData, repairedData);
        //}

        //[Fact]
        //public void TestSpecificScenario2()
        //{
        //    //This scenario works if using:
        //    //var val = Field.pow(baseList[column], row);

        //    int dataBlockCount = 5;
        //    int parityBlockCount = 5;
        //    int dataLength = 1;


        //    var expectedData = GenerateTestDataHelper.GenerateTestData(dataBlockCount, dataLength);

        //    var data = GenerateTestDataHelper.GenerateTestData(dataBlockCount, dataLength);
        //    var parityData = ParityGFAlgorithm.GenerateParityData(data, parityBlockCount);
        //    var combinedData = data.Concat(parityData).ToList();

        //    combinedData[1].Data = null;
        //    combinedData[2].Data = null;
        //    combinedData[3].Data = null;
        //    combinedData[6].Data = null;
        //    combinedData[7].Data = null;


        //    var matrix = ParityGFAlgorithm.CreateParityMatrix(expectedData, parityBlockCount);
        //    Console.WriteLine($"Matrix: {matrix}");

        //    //1 0 0 0 0
        //    //0 1 0 0 0
        //    //0 0 1 0 0
        //    //0 0 0 1 0
        //    //0 0 0 0 1
        //    //1 1 1 1 1
        //    //2 4 16 128 29
        //    //4 16 29 19 76
        //    //8 64 205 117 143
        //    //16 29 76 24 157

        //    var repairedData = ParityGFAlgorithm.RecoverData(data, parityData, parityBlockCount);

        //    //1 0 0 0 0
        //    //0 0 0 0 1
        //    //1 1 1 1 1
        //    //8 64 205 117 143
        //    //16 29 76 24 157

        //    VerifyData(expectedData, repairedData);
        //}

        [Fact]
        public void TestSpecificScenario3()
        {
            int dataBlockCount = 5;
            int parityBlockCount = 3;
            int dataLength = 1;
            var gfTable = GFTable.GFTable8;


            var expectedData = GenerateTestDataHelper.GenerateTestData(dataBlockCount, dataLength);

            var data = GenerateTestDataHelper.GenerateTestData(dataBlockCount, dataLength);
            var parityData = ParityGFAlgorithm.GenerateParityData(gfTable, data, parityBlockCount);
            var combinedData = data.Concat(parityData).ToList();

            combinedData[0].Data = null;
            combinedData[2].Data = null;
            combinedData[5].Data = null;


            var matrix = ParityGFAlgorithm.CreateParityMatrix(gfTable, expectedData.Count, parityBlockCount);
            Console.WriteLine($"Matrix: {matrix}");

            var repairedData = ParityGFAlgorithm.RecoverData(gfTable, data, parityData, parityBlockCount);

            VerifyData(expectedData, repairedData);
        }

        //private static void RunRepairTest(int dataBlockCount, int parityBlockCount, int dataLength)
        //{
        //    var expectedData = GenerateTestDataHelper.GenerateTestData(dataBlockCount, dataLength);



        //    for (int dataBlocksToDeleteCount = 1; dataBlocksToDeleteCount <= parityBlockCount; dataBlocksToDeleteCount++)
        //    {
        //        var rowsToDelete = DeleteDataHelper.DatasToDelete(dataBlockCount + parityBlockCount, dataBlocksToDeleteCount);

        //        for (int zzz = 0; zzz < rowsToDelete.Count; zzz++)
        //        {
        //            var toDelete = rowsToDelete[zzz];

        //            var data = GenerateTestDataHelper.GenerateTestData(dataBlockCount, dataLength);
        //            var parityData = ParityGFAlgorithm.GenerateParityData(data, parityBlockCount);
        //            var combinedData = data.Concat(parityData).ToList();

        //            foreach (var rowToDelete in toDelete)
        //            {
        //                combinedData[rowToDelete].Data = null;
        //            }

        //            if (dataBlocksToDeleteCount == 2)
        //            {

        //            }

        //            var repairedData = ParityGFAlgorithm.RecoverData(data, parityData, parityBlockCount);

        //            VerifyData(expectedData, repairedData);
        //        }
        //    }
        //}

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

            //    var expectedData = GenerateTestDataHelper.GenerateTestData(dataBlockCount, dataLength);



            //    bool worked = true;
            //    for (int dataBlocksToDeleteCount = 1; dataBlocksToDeleteCount <= parityBlockCount; dataBlocksToDeleteCount++)
            //    {
            //        var rowsToDelete = DeleteDataHelper.DatasToDelete(dataBlockCount + parityBlockCount, dataBlocksToDeleteCount);

            //        for (int zzz = 0; zzz < rowsToDelete.Count; zzz++)
            //        {
            //            var toDelete = rowsToDelete[zzz];

            //            var data = GenerateTestDataHelper.GenerateTestData(dataBlockCount, dataLength);
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
    }
}
