﻿using DevePar.Galois;
using DevePar.LinearAlgebra;
using DevePar.ParityAlgorithms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace DevePar
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");


            GoParStuff();







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



            var data = new List<Block<byte>>()
            {
                new Block<byte>() { Data = new byte[] { 10 }},
                new Block<byte>() { Data = new byte[] { 5 }},
                new Block<byte>() { Data = new byte[] { 8 }},
                new Block<byte>() { Data = new byte[] { 13 }},
                new Block<byte>() { Data = new byte[] { 2 }},
            };


            var parMatrix = ParityAlgorithm.CreateParityMatrix(data, 2);
            var parMatrixOnly = ParityAlgorithm.CreateParityOnlyMatrix(data, 2);

            Console.WriteLine();
            Console.Write(parMatrix);
            Console.WriteLine();
            Console.Write(parMatrixOnly);

            var recoveryData = ParityAlgorithm.GenerateParityData(data, 2);
            Console.WriteLine(string.Join(Environment.NewLine, recoveryData.Select(t => string.Join(",", t.Data))));
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


    }
}
