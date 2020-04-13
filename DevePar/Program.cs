using DevePar.ParityAlgorithms;
using System;
using System.Collections.Generic;

namespace DevePar
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");


            int dataLength = 1000000000;
            int seed = 1337;

            int dataBlocks = 3;
            int parityBlocks = 1;


            var expectedData = TestDataHelper.GenerateData(seed, dataBlocks, parityBlocks, dataLength);
            Raid5Fixer.GoFix(expectedData);

            Console.WriteLine("Full data:");
            TestDataHelper.LogData(expectedData);
            Console.WriteLine();

            for (int i = 0; i < expectedData.Count; i++)
            {
                Console.WriteLine($"##### Removing {i} #####");
                var data = TestDataHelper.GenerateData(seed, dataBlocks, parityBlocks, dataLength);
                Raid5Fixer.GoFix(data);

                data[i].Data = null;

                Console.WriteLine("# Before:");
                TestDataHelper.LogData(data);

                Raid5Fixer.GoFix(data);

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
