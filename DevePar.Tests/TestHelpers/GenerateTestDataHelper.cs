using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DevePar.Tests.TestHelpers
{
    public static class GenerateTestDataHelper
    {
        public static List<Block<uint>> ConvertToUint(List<Block<byte>> data)
        {
            return data.Select(t => new Block<uint>() { Data = t.Data?.Select(z => (uint)z).ToArray() }).ToList();
        }

        public static List<Block<byte>> GenerateTestData(int dataBlocks, int dataLength)
        {
            //The reason this functions fills arrays per data byte is because with this random seed I now get some data I know

            var generatedData = new List<Block<byte>>();
            for (int i = 0; i < dataBlocks; i++)
            {
                generatedData.Add(new Block<byte>() { Data = new byte[dataLength] });
            }

            var r = new Random(8736614);
            for (int u = 0; u < dataLength; u++)
            {
                for (int i = 0; i < dataBlocks; i++)
                {
                    var nextData = (byte)r.Next(256);
                    generatedData[i].Data[u] = nextData;
                }
            }

            return generatedData;
        }
    }
}
