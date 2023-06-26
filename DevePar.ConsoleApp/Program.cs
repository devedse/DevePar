﻿using DevePar.Galois;
using DevePar.LinearAlgebra;
using DevePar.ParityAlgorithms;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;

namespace DevePar.ConsoleApp
{
    public static class Program
    {
        public static void Main(string[] args)
        {

            //var dataList = new GField[100_000_000];

            //var blah = GFTable.GFTable16;
            //for (uint i = 0; i < dataList.Length; i++)
            //{
            //    var a = new GField(blah, i % 32767);
            //    dataList[i] = a;
            //}


            //var randomShortData = new ushort[100_000_000];

            //for (int yyy = 0; yyy < randomShortData.Length; yyy++)
            //{
            //    randomShortData[yyy] = (ushort)(yyy % 32767);
            //}


            //var w = Stopwatch.StartNew();
            //for (int yy = 0; yy < 10; yy++)
            //{
            //    int superCount = 0;
            //    for (uint i = 0; i < randomShortData.Length; i++)
            //    {
            //        var inp = randomShortData[i];
            //        var res = inp ^ 12345;
            //        if (res == 1234)
            //        {
            //            superCount++;
            //        }

            //    }
            //    Console.WriteLine($"{w.Elapsed} => {superCount}");

            //    w.Restart();
            //}


            TestSpecificScenarioHugeMaxShort();




            //Console.WriteLine(dataList[13000].Value);


            //for (int i = 0; i < 10; i++)
            //{
            //    Console.WriteLine("Waiting..");
            //    Thread.Sleep(1000);
            //}

            Console.WriteLine("Hello World!");

            var baseListTest = BaseCalculator.CalcBase(8).ToList();

            var rrrrrr = string.Join($"{Environment.NewLine}", baseListTest);

            byte aa = 0;
            aa ^= 0x11D & 0xFF;
            byte ding = 0x11D & 0xFF;



            var test1 = GFTable.GFTable8;

            var result = test1.Add(5, 10);
            result = test1.Mul(10, 5);


            var res2 = test1.Add(250, 50);
        }


        public static void TestSpecificScenarioHugeMaxShort()
        {
            int dataBlockCount = 1000;
            int parityBlockCount = 1000;
            int dataLength = 2;


            //Parallel.For(0, 1000, new ParallelOptions() { MaxDegreeOfParallelism = 32 }, (i) =>
            for (int i = 0; i < 1; i++)
            {
                var testData = GenerateTestDataHelper.GenerateTestDataShort(dataBlockCount, dataLength);
                var expectedData = GenerateTestDataHelper.ConvertToUint(testData);

                var data = GenerateTestDataHelper.ConvertToUint(testData);
                var parityData = ParityGFAlgorithmFast.GenerateParityData3(GFTable.GFTable16, data, parityBlockCount);
                var combinedData = data.Concat(parityData).ToList();

                var r = new Random(i);

                //combinedData.Shuffle(r);
                for (int y = 0; y < parityBlockCount; y++)
                {
                    combinedData[y].Data = null;
                }


                var repairedData = ParityGFAlgorithmFast.RecoverData3(GFTable.GFTable16, data, parityData, parityBlockCount);

                //VerifyData(expectedData, repairedData);
            }
            //);
        }


        public static void TestSpecificScenarioHugeMaxShortFast()
        {
            int dataBlockCount = 1000;
            int parityBlockCount = 1000;
            int dataLength = 2;


            //Parallel.For(0, 1000, new ParallelOptions() { MaxDegreeOfParallelism = 32 }, (i) =>
            for (int i = 0; i < 1; i++)
            {
                var testData = GenerateTestDataHelper.GenerateTestDataShort(dataBlockCount, dataLength);
                var expectedData = GenerateTestDataHelper.ConvertToUint(testData);

                var data = GenerateTestDataHelper.ConvertToUint(testData);
                var parityData = ParityGFAlgorithmFast.GenerateParityData3(GFTable.GFTable16, data, parityBlockCount);
                var combinedData = data.Concat(parityData).ToList();

                var r = new Random(i);

                //combinedData.Shuffle(r);
                for (int y = 0; y < parityBlockCount; y++)
                {
                    combinedData[y].Data = null;
                }


                var repairedData = ParityGFAlgorithmFast.RecoverData3(GFTable.GFTable16, data, parityData, parityBlockCount);

                //VerifyData(expectedData, repairedData);
            }
            //);
        }




        public static void GoParStuff()
        {

            //usage example
            Field f1 = new Field(15);
            Field f2 = new Field(8);
            Field f3 = f1 + f2;
            Console.WriteLine(f3);

            var f4 = f3 - f2;

            Console.WriteLine(f4);


            //var v1 = new CoolVector(1, 2, 3);
            //var v2 = new CoolVector(2, 1, 3);

            //var ar2 = new int[,] {
            //    { 1, 2, 3 },
            //    { 4, 5, 6 },
            //    { 7, 8, 9 }
            //};


            //var matrix = new Matrix(ar2);

            //var matrix2 = new Matrix(new int[,] {
            //    { 2 },
            //    { 1 },
            //    { 3 }
            //});






            //var result = matrix * v2;
            //Console.WriteLine(result);

            //var result2 = matrix * matrix2;
            //Console.WriteLine(result2);




            // 10
            // 3

            var ar1 = new int[,] {
                { 1, 0, 0, 0, 0 },
                { 0, 1, 0, 0, 0 },
                { 0, 0, 1, 0, 0 },
                { 0, 0, 0, 1, 0 },
                { 0, 0, 0, 0, 1 },
                { 1, 1, 1, 1, 1 },
                { 1, 2, 3, 4, 5 }
            };


            var matrixField = new MatrixField(ar1);
            var dataForField = new MatrixField(new int[,] {
                { 10 },
                { 5 },
                { 8 },
                { 13 },
                { 2 }
            });
            var dataForField2 = new CoolVectorField(
                10,
                5,
                8,
                13,
                2
                );

            var result3 = matrixField * dataForField2;

            Console.WriteLine(result3);


            int parityBlocks = 2;

            var data = new List<Block<byte>>()
            {
                new Block<byte>() { Data = new byte[] { 10 }},
                new Block<byte>() { Data = new byte[] { 5 }},
                new Block<byte>() { Data = new byte[] { 8 }},
                new Block<byte>() { Data = new byte[] { 13 }},
                new Block<byte>() { Data = new byte[] { 2 }},
            };


            var parMatrix = ParityAlgorithm.CreateParityMatrix(data, parityBlocks);
            var parMatrixOnly = ParityAlgorithm.CreateParityOnlyMatrix(data, parityBlocks);

            Console.WriteLine();
            Console.Write(parMatrix);
            Console.WriteLine();
            Console.Write(parMatrixOnly);

            var recoveryData = ParityAlgorithm.GenerateParityData(data, parityBlocks);
            Console.WriteLine(string.Join(Environment.NewLine, recoveryData.Select(t => string.Join(",", t.Data))));



            //var totalData = data.Concat(recoveryData).ToList();
            //data[0].Data = null;
            data[1].Data = null;
            //data[2].Data = null;
            //data[3].Data = null;
            //data[4].Data = null;
            //recoveryData[0].Data = null;
            recoveryData[1].Data = null;
            ParityAlgorithm.RecoverData(data, recoveryData, parityBlocks);
            //ParityAlgorithm.RecoverDataV2(data, recoveryData, parityBlocks);





            var missing = new MatrixField(new int[,]
            {
                { 1 },
                { 1 },
                { 1 },
                { 1 },
                { 1 },
                { 1 },
                { 1 }
            });


        }

        public static void GoRaid5Stuff()
        {

            int dataLength = 1000000000;
            int seed = 1337;

            int dataBlocks = 3;
            int parityBlocks = 1;


            var expectedData = TestDataHelper.GenerateData(seed, dataBlocks, parityBlocks, dataLength);
            Raid5Algorithm.GoFix(expectedData);

            Console.WriteLine("Full data:");
            TestDataHelper.LogData(expectedData);
            Console.WriteLine();

            for (int i = 0; i < expectedData.Count; i++)
            {
                Console.WriteLine($"##### Removing {i} #####");
                var data = TestDataHelper.GenerateData(seed, dataBlocks, parityBlocks, dataLength);
                Raid5Algorithm.GoFix(data);

                data[i].Data = null;

                Console.WriteLine("# Before:");
                TestDataHelper.LogData(data);

                Raid5Algorithm.GoFix(data);

                var areEqual = TestDataHelper.VerifyIfEqual(expectedData, data);

                Console.WriteLine("# After");
                TestDataHelper.LogData(data);

                Console.WriteLine($"Equality check: {areEqual}");
                Console.WriteLine();
                Console.WriteLine();
            }
        }

        public static void DetermineSeedForRandom()
        {
            var data = new List<Block<byte>>()
            {
                new Block<byte>() { Data = new byte[] { 10 }},
                new Block<byte>() { Data = new byte[] { 5 }},
                new Block<byte>() { Data = new byte[] { 8 }},
                new Block<byte>() { Data = new byte[] { 13 }},
                new Block<byte>() { Data = new byte[] { 2 }},
            };

            int curMax = 0;
            int lowestDiff = 256;


            for (int i = 0; i < int.MaxValue; i++)
            {
                bool found = true;
                var random = new Random(i);
                //var random = new Random(8736614);
                for (int y = 0; y < 5; y++)
                {
                    var newRandom = random.Next(256);
                    var diff = Math.Abs(newRandom - data[y].Data[0]);
                    if (newRandom != data[y].Data[0])
                    {
                        found = false;
                    }

                    if (y + 1 > curMax && found)
                    {
                        Console.WriteLine($"New max: {y + 1}, with seed: {i}");
                        curMax = y + 1;
                        lowestDiff = 256;
                    }
                    else if (y + 1 > curMax && diff < lowestDiff)
                    {
                        Console.WriteLine($"New max: {y + 1}, with seed: {i} with diff: {diff} (Expected: {data[y].Data[0]} Actual: {newRandom})");
                        lowestDiff = diff;
                    }

                    if (found == false)
                    {
                        break;
                    }
                }

                if (found)
                {
                    Console.WriteLine($"Found seed {i}");
                    break;
                }

                if (i % 10000000 == 0)
                {
                    Console.WriteLine(i);
                }
            }

            Console.WriteLine("Done");
        }
    }
}
