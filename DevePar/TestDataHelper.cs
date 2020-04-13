using System;
using System.Collections.Generic;
using System.Text;

namespace DevePar
{
    public static class TestDataHelper
    {

        public static void LogData<T>(List<Block<T>> data)
        {
            return;
            for (int i = 0; i < data.Count; i++)
            {
                Console.Write($"{i}:");

                var dataCur = data[i].Data;
                if (dataCur == null)
                {
                    Console.WriteLine($"\tnull");
                }
                else
                {
                    Console.WriteLine($"\t{string.Join("", data[i].Data)}");
                }
            }
        }


        public static List<Block<int>> GenerateData(int seed, int dataBlocks, int emptyParityBlocks, int dataLength)
        {
            Console.Write("Generating data...");

            var random = new Random(seed);

            var blocks = new List<Block<int>>();

            for (int i = 0; i < dataBlocks; i++)
            {
                var data = new int[dataLength];

                for (int y = 0; y < dataLength; y++)
                {
                    data[y] = random.Next();
                }

                blocks.Add(new Block<int>()
                {
                    Data = data
                });
            }

            for (int i = 0; i < emptyParityBlocks; i++)
            {
                //Add parity block which is empty
                blocks.Add(new Block<int>());
            }

            Console.WriteLine(" Done :)");
            return blocks;
        }

        public static bool VerifyIfEqual<T>(List<Block<T>> expected, List<Block<T>> actual)
        {
            if (expected.Count != actual.Count)
            {
                return false;
            }

            for (int i = 0; i < expected.Count; i++)
            {
                var expectedBlock = expected[i];
                var actualBlock = actual[i];

                if (expectedBlock.Data.Length != actualBlock.Data.Length)
                {
                    return false;
                }

                for (int y = 0; y < expectedBlock.Data.Length; y++)
                {
                    var expectedData = expectedBlock.Data[y];
                    var actualData = actualBlock.Data[y];

                    if (!expectedData.Equals(actualData))
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }
}
