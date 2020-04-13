using DeveCoolLib.Conversion;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace DevePar.ParityAlgorithms
{
    public static class Raid5Fixer
    {
        public static void GoFix(List<Block<int>> data)
        {
            var w = Stopwatch.StartNew();

            //find missing parity block
            var missingBlocks = data.Where(t => t.Data == null);

            //find data valid blocks
            var validBlocks = data.Where(t => t.Data != null).ToList();

            if (missingBlocks.Count() != 1)
            {
                throw new ArgumentException("Too much parity data is missing");
            }

            var missingBlock = missingBlocks.Single();

            var maxLength = validBlocks.Max(t => t.Data.Length);


            var newParityData = new int[maxLength];

            for (int i = 0; i < maxLength; i++)
            {
                int thisData = validBlocks[0].Data[i];
                for (int y = 1; y < validBlocks.Count; y++)
                {
                    thisData = thisData ^ validBlocks[y].Data[i];
                }
                newParityData[i] = thisData;
            }

            missingBlock.Data = newParityData;
            w.Stop();

            Console.WriteLine($"Time taken for Raid 5 repair:{w.Elapsed}  Mb per second: {ValuesToStringHelper.BytesToString((long)(maxLength / w.Elapsed.TotalSeconds) * sizeof(int))}");
        }

        public static void GoVerify(List<Block<int>> data)
        {

        }
    }
}
