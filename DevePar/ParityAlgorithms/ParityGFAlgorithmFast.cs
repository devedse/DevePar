using DevePar.Galois;
using DevePar.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DevePar.ParityAlgorithms
{
    public static class ParityGFAlgorithmFast
    {
        //private const int Base = 2;

        //public static short[][] CreateParityMatrix(GFTable gfTable, int dataBlocksCount, int parityBlockCount)
        //{
        //    int totalBlocks = dataBlocksCount + parityBlockCount;
        //    if (totalBlocks > 256)
        //    {
        //        throw new InvalidOperationException("A total of more then 256 blocks is not supported");
        //    }

        //    var identityMatrix = MatrixGFieldFast.CreateIdentityMatrix(gfTable, dataBlocksCount);
        //    var parityOnlyMatrix = CreateParityOnlyMatrix(gfTable, dataBlocksCount, parityBlockCount);

        //    var combinedMatrix = identityMatrix.AddRowsAtTheEnd(parityOnlyMatrix);
        //    return combinedMatrix;
        //}


        //public static MatrixGFieldFast CreateParityOnlyMatrix(GFTable gfTable, int dataBlocksCount, int parityBlockCount)
        //{
        //    int totalBlocks = dataBlocksCount + parityBlockCount;
        //    if (totalBlocks > 256)
        //    {
        //        throw new InvalidOperationException("A total of more then 256 blocks is not supported");
        //    }

        //    var parityMatrixArray = new GField[parityBlockCount, dataBlocksCount];



        //    var baseList = BaseGFCalculator.CalculateBase(gfTable).ToList();
        //    //Copy parity part of the matrix
        //    for (uint row = 0; row < parityBlockCount; row++)
        //    {
        //        for (int column = 0; column < parityMatrixArray.GetLength(1); column++)
        //        {
        //            var val = baseList[column].Pow(row + 1);
        //            parityMatrixArray[row, column] = val;
        //        }
        //    }


        //    return new MatrixGFieldFast(parityMatrixArray);
        //}

        //public static MatrixGFieldFast CreateParityMatrix2(GFTable gfTable, int dataBlocksCount, int parityBlockCount)
        //{
        //    int totalBlocks = dataBlocksCount + parityBlockCount;
        //    if (totalBlocks > 256)
        //    {
        //        throw new InvalidOperationException("A total of more then 256 blocks is not supported");
        //    }

        //    var parityMatrixArray = new GField[totalBlocks, dataBlocksCount];



        //    var baseList = BaseGFCalculator.CalculateBase(gfTable).ToList();
        //    var res = string.Join(",", baseList);
        //    //Copy parity part of the matrix
        //    for (int row = 0; row < totalBlocks; row++)
        //    {
        //        for (uint column = 0; column < parityMatrixArray.GetLength(1); column++)
        //        {
        //            var val = baseList[row].Pow(column + 1);
        //            //var val = gfTable.CreateField(gfTable.Pow(2, gfTable.Mul(row, column)));
        //            //var val = baseList[row].Pow(gfTable.Mul(row, (uint)column));
        //            parityMatrixArray[row, column] = val;
        //        }
        //    }

        //    var transposedVanDerMondeMatrix = new MatrixGFieldFast(parityMatrixArray);

        //    var dataMatrix = transposedVanDerMondeMatrix.Submatrix(0, dataBlocksCount - 1, 0, dataBlocksCount - 1);
        //    var invertedDataMatrix = dataMatrix.InverseRuben();

        //    var finalMatrix = transposedVanDerMondeMatrix * invertedDataMatrix;

        //    return finalMatrix;
        //}


        public static MatrixGFieldFast CreateParityMatrixForRecovery<T>(GFTable gfTable, List<Block<T>> dataBlocks, List<Block<T>> parityBlocks)
        {



            //                                              parpresent
            //                  datapresent       datamissing         datamissing       parmissing
            //            /                     |             \ /                     |           \
            //parpresent  |           (ppi[row])|             | |           (ppi[row])|           |
            //datamissing |          ^          |      I      | |          ^          |     0     |
            //            |(dpi[col])           |             | |(dmi[col])           |           |
            //            +---------------------+-------------+ +---------------------+-----------+
            //            |           (pmi[row])|             | |           (pmi[row])|           |
            //parmissing  |          ^          |      0      | |          ^          |     I     |
            //            |(dpi[col])           |             | |(dmi[col])           |           |
            //            \                     |             / \                     |           /

            uint datamissing = (uint)dataBlocks.Count(t => t.Data == null);
            uint parmissing = (uint)parityBlocks.Count(t => t.Data == null);
            uint datapresent = (uint)dataBlocks.Count(t => t.Data != null);
            uint parpresent = (uint)parityBlocks.Count(t => t.Data != null);

            uint outcount = datamissing + parmissing;
            uint incount = datapresent + datamissing;

            if (datamissing > parpresent)
            {
                throw new InvalidOperationException("Not enough recovery blocks.");
            }
            else if (outcount == 0)
            {
                throw new InvalidOperationException("No output blocks.");
            }

            int[] datamissingindex = new int[datamissing];
            int[] parmissingindex = new int[parmissing];
            int[] datapresentindex = new int[datapresent];
            int[] parpresentindex = new int[parpresent];

            int counterDataMissing = 0;
            int counterDataPresent = 0;
            for (int i = 0; i < dataBlocks.Count; i++)
            {
                var dataBlock = dataBlocks[i];
                if (dataBlock.Data == null)
                {
                    datamissingindex[counterDataMissing] = i;
                    counterDataMissing++;
                }
                else
                {
                    datapresentindex[counterDataPresent] = i;
                    counterDataPresent++;
                }
            }

            int counterParMissing = 0;
            int counterParPresent = 0;
            for (int i = 0; i < parityBlocks.Count; i++)
            {
                var ParBlock = parityBlocks[i];
                if (ParBlock.Data == null)
                {
                    parmissingindex[counterParMissing] = i;
                    counterParMissing++;
                }
                else
                {
                    parpresentindex[counterParPresent] = i;
                    counterParPresent++;
                }
            }



            var leftmatrix = new ushort[outcount][];
            for (int i = 0; i < outcount; i++)
            {
                leftmatrix[i] = new ushort[incount];
            }

            var rightmatrix = new ushort[outcount][];

            if (datamissing > 0)
            {
                for (int i = 0; i < outcount; i++)
                {
                    rightmatrix[i] = new ushort[outcount];
                }
            }


            var combinedData = dataBlocks.Concat(parityBlocks).ToList();
            int outputrow = 0;

            var database = BaseGFCalculator.CalculateBase(gfTable).Select(t => t.Value).ToList();

            for (uint row = 0; row < datamissing; row++)
            {
                // Get the exponent of the next present recovery block
                while (combinedData[outputrow].Data != null)
                {
                    outputrow++;
                }
                var exponent = (uint)outputrow;

                // One column for each present data block
                for (uint col = 0; col < datapresent; col++)
                {
                    leftmatrix[row][col] = (ushort)gfTable.Pow(database[datapresentindex[col]], (uint)parpresentindex[row]);
                }
                // One column for each each present recovery block that will be used for a missing data block
                for (uint col = 0; col < datamissing; col++)
                {
                    leftmatrix[row][col + datapresent] = (ushort)((row == col) ? 1 : 0);
                }

                if (datamissing > 0)
                {
                    // One column for each missing data block
                    for (uint col = 0; col < datamissing; col++)
                    {
                        rightmatrix[row][col] = (ushort)gfTable.Pow(database[datamissingindex[col]], (uint)parpresentindex[row]);
                    }
                    // One column for each missing recovery block
                    for (uint col = 0; col < parmissing; col++)
                    {
                        rightmatrix[row][col + datamissing] = 0;
                    }
                }

                outputrow++;
            }

            outputrow = 0;

            for (uint row = 0; row < parmissing; row++)
            {
                // Get the exponent of the next missing recovery block
                while (combinedData[outputrow].Data == null)
                {
                    outputrow++;
                }
                var exponent = (uint)outputrow;

                // One column for each present data block
                for (uint col = 0; col < datapresent; col++)
                {
                    leftmatrix[(row + datamissing)][col] = (ushort)gfTable.Pow(database[datapresentindex[col]], (uint)parmissingindex[row]);
                }
                // One column for each each present recovery block that will be used for a missing data block
                for (uint col = 0; col < datamissing; col++)
                {
                    leftmatrix[(row + datamissing)][col + datapresent] = 0;
                }

                if (datamissing > 0)
                {
                    // One column for each missing data block
                    for (uint col = 0; col < datamissing; col++)
                    {
                        rightmatrix[(row + datamissing)][col] = (ushort)gfTable.Pow(database[datamissingindex[col]], (uint)parmissingindex[row]);
                    }
                    // One column for each missing recovery block
                    for (uint col = 0; col < parmissing; col++)
                    {
                        rightmatrix[(row + datamissing)][col + datamissing] = (ushort)((row == col) ? 1 : 0);
                    }
                }

                outputrow++;
            }

            var leftmatrixM = new MatrixGFieldFast(gfTable, leftmatrix);

            if (datamissing > 0)
            {
                // Perform Gaussian Elimination and then delete the right matrix (which 
                // will no longer be required).
                var rightmatrixM = new MatrixGFieldFast(gfTable, rightmatrix);
                GaussElim(gfTable, outcount, incount, leftmatrixM, rightmatrixM, datamissing);

            }

            return leftmatrixM;
        }

        public static void GaussElim(GFTable gfTable, uint rows, uint leftcols, MatrixGFieldFast leftmatrix, MatrixGFieldFast rightmatrix, uint datamissing)
        {
            for (int row = 0; row < datamissing; row++)
            {
                // NB Row and column swapping to find a non zero pivot value or to find the largest value
                // is not necessary due to the nature of the arithmetic and construction of the RS matrix.

                // Get the pivot value.
                var pivotvalue = rightmatrix[row, row];

                if (pivotvalue == 0)
                {
                    throw new InvalidOperationException("RS computation error.");
                }

                // If the pivot value is not 1, then the whole row has to be scaled
                if (pivotvalue != 1)
                {
                    for (int col = 0; col < leftcols; col++)
                    {
                        if (leftmatrix[row, col] != 0)
                        {
                            leftmatrix[row, col] = (ushort)gfTable.Div(leftmatrix[row, col], pivotvalue);
                        }
                    }
                    rightmatrix[row, row] = 1;
                    for (int col = row + 1; col < rows; col++)
                    {
                        if (rightmatrix[row, col] != 0)
                        {
                            rightmatrix[row, col] = (ushort)gfTable.Div(rightmatrix[row, col], pivotvalue);
                        }
                    }
                }

                // For every other row in the matrix
                for (int row2 = 0; row2 < rows; row2++)
                {
                    if (row != row2)
                    {
                        // Get the scaling factor for this row.
                        var scalevalue = rightmatrix[row2, row];

                        if (scalevalue == 1)
                        {
                            // If the scaling factor happens to be 1, just subtract rows
                            for (int col = 0; col < leftcols; col++)
                            {
                                if (leftmatrix[row, col] != 0)
                                {
                                    leftmatrix[row2, col] = (ushort)gfTable.Sub(leftmatrix[row2, col], leftmatrix[row, col]);
                                }
                            }

                            for (int col = row; col < rows; col++)
                            {
                                if (rightmatrix[row, col] != 0)
                                {
                                    rightmatrix[row2, col] = (ushort)gfTable.Sub(rightmatrix[row2, col], rightmatrix[row, col]);
                                }
                            }
                        }
                        else if (scalevalue != 0)
                        {
                            // If the scaling factor is not 0, then compute accordingly.
                            for (int col = 0; col < leftcols; col++)
                            {
                                if (leftmatrix[row, col] != 0)
                                {
                                    leftmatrix[row2, col] = (ushort)gfTable.Sub(leftmatrix[row2, col], gfTable.Mul(leftmatrix[row, col], scalevalue));
                                }
                            }

                            for (int col = row; col < rows; col++)
                            {
                                if (rightmatrix[row, col] != 0)
                                {
                                    rightmatrix[row2, col] = (ushort)gfTable.Sub(rightmatrix[row2, col], gfTable.Mul(rightmatrix[row, col], scalevalue));
                                }
                            }
                        }
                    }
                }
            }
        }


        public static List<Block<uint>> GenerateParityData3(GFTable gfTable, List<Block<uint>> dataBlocks, int parityBlockCount)
        {
            int dataLengthInsideBlock = dataBlocks.First().Data.Length;

            var parityDataList = new List<Block<uint>>();
            for (int i = 0; i < parityBlockCount; i++)
            {
                parityDataList.Add(new Block<uint>() { Data = null });
            }

            var parityMatrix = CreateParityMatrixForRecovery(gfTable, dataBlocks, parityDataList);

            parityDataList = new List<Block<uint>>();
            for (int i = 0; i < parityBlockCount; i++)
            {
                parityDataList.Add(new Block<uint>() { Data = new uint[dataLengthInsideBlock] });
            }

            for (int i = 0; i < dataLengthInsideBlock; i++)
            {
                var data = new List<ushort>();
                foreach (var dataBlock in dataBlocks)
                {
                    data.Add((ushort)dataBlock.Data[i]);
                }

                var toArray = data.ToArray();

                var resultData = parityMatrix.Multiply(toArray);
                var parityData = resultData.ToArray();

                for (int y = 0; y < parityDataList.Count; y++)
                {
                    parityDataList[y].Data[i] = parityData[y];
                }
            }

            return parityDataList;
        }



        public static List<Block<uint>> RecoverData3(GFTable gfTable, List<Block<uint>> dataBlocks, List<Block<uint>> recoveryBlocks, int parityBlockCount)
        {
            var combinedData = dataBlocks.Concat(recoveryBlocks).ToList();
            var combinedDataWithoutMissingData = combinedData.Where(t => t.Data != null).ToList();
            var combinedDataWithMissingData = combinedData.Where(t => t.Data == null).ToList();
            int dataLengthInsideBlock = combinedData.First(t => t.Data != null).Data.Length;

            var recoveryMatrixDing = CreateParityMatrixForRecovery(gfTable, dataBlocks, recoveryBlocks);





            foreach (var block in combinedDataWithMissingData)
            {
                if (block.Data == null)
                {
                    block.Data = new uint[dataLengthInsideBlock];
                }
            }




            for (int i = 0; i < dataLengthInsideBlock; i++)
            {
                var data = new List<ushort>();
                //If there's more repair data then we need, from all the blocks, just take the amount of data blocks
                foreach (var dataBlock in combinedDataWithoutMissingData.Take(dataBlocks.Count))
                {
                    data.Add((ushort)dataBlock.Data[i]);
                }

                var toArray = data.ToArray();
                //var vector = new CoolVectorField(toArray);

                var res = recoveryMatrixDing.Multiply(toArray);

                //Console.WriteLine($"Recovered data:\n\r{res}");
                for (int y = 0; y < res.Length; y++)
                {
                    combinedDataWithMissingData[y].Data[i] = res[y];
                }
            }


            return dataBlocks;
        }
    }
}
