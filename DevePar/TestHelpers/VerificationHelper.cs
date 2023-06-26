using System.Collections.Generic;

namespace DevePar.TestHelpers
{
    public static class VerificationHelper
    {
        public static bool VerifyData(List<Block<uint>> expectedData, List<Block<uint>> repairedData)
        {
            for (int y = 0; y < expectedData.Count; y++)
            {
                var curExpectedData = expectedData[y];
                var curRepairData = repairedData[y];
                for (int z = 0; z < curExpectedData.Data.Length; z++)
                {
                    var curExpectedValue = curExpectedData.Data[z];
                    var curRepairedValue = curRepairData.Data[z];

                    if (curExpectedValue != curRepairedValue)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public static bool VerifyData(List<Block<byte>> expectedData, List<Block<byte>> repairedData)
        {
            for (int y = 0; y < expectedData.Count; y++)
            {
                var curExpectedData = expectedData[y];
                var curRepairData = repairedData[y];
                for (int z = 0; z < curExpectedData.Data.Length; z++)
                {
                    var curExpectedValue = curExpectedData.Data[z];
                    var curRepairedValue = curRepairData.Data[z];

                    if (curExpectedValue != curRepairedValue)
                    {
                        return false;
                    }
                }
            }
            return true;
        }
    }
}
