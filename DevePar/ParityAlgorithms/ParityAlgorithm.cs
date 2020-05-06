using System;
using System.Collections.Generic;
using System.Text;

namespace DevePar.ParityAlgorithms
{
    public static class ParityAlgorithm
    {
        public static void CalculateParity(List<byte> dataBlocks, int parityBlocks)
        {
            int totalBlocks = dataBlocks.Count + parityBlocks;
            if (totalBlocks > 256)
            {
                throw new InvalidOperationException("A total of more then 256 blocks is not supported");
            }

            var parityData = new List<byte>(parityBlocks);

        }
    }
}
