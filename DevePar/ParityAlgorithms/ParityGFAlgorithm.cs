using DevePar.Galois;
using DevePar.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DevePar.ParityAlgorithms
{
    public static class ParityGFAlgorithm
    {
        //private const int Base = 2;

        public static MatrixGField CreateParityMatrix(GFTable gfTable, int dataBlocksCount, int parityBlockCount)
        {
            int totalBlocks = dataBlocksCount + parityBlockCount;
            if (totalBlocks > 256)
            {
                throw new InvalidOperationException("A total of more then 256 blocks is not supported");
            }

            var identityMatrix = MatrixGField.CreateIdentityMatrix(gfTable, dataBlocksCount);
            var parityOnlyMatrix = CreateParityOnlyMatrix(gfTable, dataBlocksCount, parityBlockCount);

            var combinedMatrix = identityMatrix.AddRowsAtTheEnd(parityOnlyMatrix);
            return combinedMatrix;
        }


        public static MatrixGField CreateParityOnlyMatrix(GFTable gfTable, int dataBlocksCount, int parityBlockCount)
        {
            int totalBlocks = dataBlocksCount + parityBlockCount;
            if (totalBlocks > 256)
            {
                throw new InvalidOperationException("A total of more then 256 blocks is not supported");
            }

            var parityMatrixArray = new GField[parityBlockCount, dataBlocksCount];



            var baseList = BaseGFCalculator.CalculateBase(gfTable).ToList();
            //Copy parity part of the matrix
            for (uint row = 0; row < parityBlockCount; row++)
            {
                for (int column = 0; column < parityMatrixArray.GetLength(1); column++)
                {
                    var val = baseList[column].Pow(row + 1);
                    parityMatrixArray[row, column] = val;
                }
            }


            return new MatrixGField(parityMatrixArray);
        }

        public static MatrixGField CreateParityMatrix2(GFTable gfTable, int dataBlocksCount, int parityBlockCount)
        {
            int totalBlocks = dataBlocksCount + parityBlockCount;
            if (totalBlocks > 256)
            {
                throw new InvalidOperationException("A total of more then 256 blocks is not supported");
            }

            var parityMatrixArray = new GField[totalBlocks, dataBlocksCount];



            var baseList = BaseGFCalculator.CalculateBase(gfTable).ToList();
            var res = string.Join(",", baseList);
            //Copy parity part of the matrix
            for (int row = 0; row < totalBlocks; row++)
            {
                for (uint column = 0; column < parityMatrixArray.GetLength(1); column++)
                {
                    var val = baseList[row].Pow(column + 1);
                    //var val = gfTable.CreateField(gfTable.Pow(2, gfTable.Mul(row, column)));
                    //var val = baseList[row].Pow(gfTable.Mul(row, (uint)column));
                    parityMatrixArray[row, column] = val;
                }
            }

            var transposedVanDerMondeMatrix = new MatrixGField(parityMatrixArray);

            var dataMatrix = transposedVanDerMondeMatrix.Submatrix(0, dataBlocksCount - 1, 0, dataBlocksCount - 1);
            var invertedDataMatrix = dataMatrix.InverseRuben();

            var finalMatrix = transposedVanDerMondeMatrix * invertedDataMatrix;

            return finalMatrix;
        }

        //public static List<Block<byte>> GenerateParityData(GFTable gfTable, List<Block<byte>> dataBlocks, int parityBlockCount)
        //{
        //    int dataLengthInsideBlock = dataBlocks.First().Data.Length;

        //    var parityMatrix = CreateParityOnlyMatrix(gfTable, dataBlocks.Count, parityBlockCount);

        //    var parityDataList = new List<Block<byte>>();
        //    for (int i = 0; i < parityBlockCount; i++)
        //    {
        //        parityDataList.Add(new Block<byte>() { Data = new byte[dataLengthInsideBlock] });
        //    }

        //    for (int i = 0; i < dataLengthInsideBlock; i++)
        //    {
        //        var data = new List<GField>();
        //        foreach (var dataBlock in dataBlocks)
        //        {
        //            var newField = gfTable.CreateField(dataBlock.Data[i]);
        //            data.Add(newField);
        //        }

        //        var toArray = data.ToArray();

        //        var parityData = parityMatrix.Multiply(toArray);

        //        for (int y = 0; y < parityDataList.Count; y++)
        //        {
        //            parityDataList[y].Data[i] = (byte)parityData[y].Value;
        //        }
        //    }

        //    return parityDataList;
        //}

        public static MatrixGField CreateParityMatrixForRecovery<T>(GFTable gfTable, List<Block<T>> dataBlocks, List<Block<T>> parityBlocks)
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


            var leftmatrix = new GField[outcount][];
            for (int i = 0; i < outcount; i++)
            {
                leftmatrix[i] = new GField[incount];
            }

            var rightmatrix = new GField[outcount][];
            if (datamissing > 0)
            {
                for (int i = 0; i < outcount; i++)
                {
                    rightmatrix[i] = new GField[outcount];
                }
            }

            var combinedData = dataBlocks.Concat(parityBlocks).ToList();
            int outputrow = 0;

            var database = BaseGFCalculator.CalculateBase(gfTable).ToList();

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
                    leftmatrix[row][col] = database[datapresentindex[col]].Pow((uint)parpresentindex[row]);
                }
                // One column for each each present recovery block that will be used for a missing data block
                for (uint col = 0; col < datamissing; col++)
                {
                    leftmatrix[row][col + datapresent] = gfTable.CreateField((uint)((row == col) ? 1 : 0));
                }

                if (datamissing > 0)
                {
                    // One column for each missing data block
                    for (uint col = 0; col < datamissing; col++)
                    {
                        rightmatrix[row][col] = database[datamissingindex[col]].Pow((uint)parpresentindex[row]);
                    }
                    // One column for each missing recovery block
                    for (uint col = 0; col < parmissing; col++)
                    {
                        rightmatrix[row][col + datamissing] = gfTable.CreateField(0);
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
                    leftmatrix[(row + datamissing)][col] = database[datapresentindex[col]].Pow((uint)parmissingindex[row]);
                }
                // One column for each each present recovery block that will be used for a missing data block
                for (uint col = 0; col < datamissing; col++)
                {
                    leftmatrix[(row + datamissing)][col + datapresent] = gfTable.CreateField(0);
                }

                if (datamissing > 0)
                {
                    // One column for each missing data block
                    for (uint col = 0; col < datamissing; col++)
                    {
                        rightmatrix[(row + datamissing)][col] = database[datamissingindex[col]].Pow((uint)parmissingindex[row]);
                    }
                    // One column for each missing recovery block
                    for (uint col = 0; col < parmissing; col++)
                    {
                        rightmatrix[(row + datamissing)][col + datamissing] = gfTable.CreateField((uint)((row == col) ? 1 : 0));
                    }
                }

                outputrow++;
            }

            var leftmatrixM = new MatrixGField(leftmatrix);

            if (datamissing > 0)
            {
                // Perform Gaussian Elimination and then delete the right matrix (which 
                // will no longer be required).
                var rightmatrixM = new MatrixGField(rightmatrix);
                GaussElim(gfTable, outcount, incount, leftmatrixM, rightmatrixM, datamissing);

            }

            return leftmatrixM;
        }

        public static void GaussElim(GFTable gfTable, uint rows, uint leftcols, MatrixGField leftmatrix, MatrixGField rightmatrix, uint datamissing)
        {
            for (int row = 0; row < datamissing; row++)
            {
                // NB Row and column swapping to find a non zero pivot value or to find the largest value
                // is not necessary due to the nature of the arithmetic and construction of the RS matrix.

                // Get the pivot value.
                var pivotvalue = rightmatrix[row, row];

                if (pivotvalue.Value == 0)
                {
                    throw new InvalidOperationException("RS computation error.");
                }

                // If the pivot value is not 1, then the whole row has to be scaled
                if (pivotvalue.Value != 1)
                {
                    for (int col = 0; col < leftcols; col++)
                    {
                        if (leftmatrix[row, col].Value != 0)
                        {
                            leftmatrix[row, col] /= pivotvalue;
                        }
                    }
                    rightmatrix[row, row] = gfTable.CreateField(1);
                    for (int col = row + 1; col < rows; col++)
                    {
                        if (rightmatrix[row, col].Value != 0)
                        {
                            rightmatrix[row, col] /= pivotvalue;
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

                        if (scalevalue.Value == 1)
                        {
                            // If the scaling factor happens to be 1, just subtract rows
                            for (int col = 0; col < leftcols; col++)
                            {
                                if (leftmatrix[row, col].Value != 0)
                                {
                                    leftmatrix[row2, col] -= leftmatrix[row, col];
                                }
                            }

                            for (int col = row; col < rows; col++)
                            {
                                if (rightmatrix[row, col].Value != 0)
                                {
                                    rightmatrix[row2, col] -= rightmatrix[row, col];
                                }
                            }
                        }
                        else if (scalevalue.Value != 0)
                        {
                            // If the scaling factor is not 0, then compute accordingly.
                            for (int col = 0; col < leftcols; col++)
                            {
                                if (leftmatrix[row, col].Value != 0)
                                {
                                    leftmatrix[row2, col] -= leftmatrix[row, col] * scalevalue;
                                }
                            }

                            for (int col = row; col < rows; col++)
                            {
                                if (rightmatrix[row, col].Value != 0)
                                {
                                    rightmatrix[row2, col] -= rightmatrix[row, col] * scalevalue;
                                }
                            }
                        }
                    }
                }
            }
        }

        public static List<Block<uint>> GenerateParityData2(GFTable gfTable, List<Block<uint>> dataBlocks, int parityBlockCount)
        {
            int dataLengthInsideBlock = dataBlocks.First().Data.Length;

            var parityMatrix = CreateParityMatrix2(gfTable, dataBlocks.Count, parityBlockCount);

            var parityDataList = new List<Block<uint>>();
            for (int i = 0; i < parityBlockCount; i++)
            {
                parityDataList.Add(new Block<uint>() { Data = new uint[dataLengthInsideBlock] });
            }

            for (int i = 0; i < dataLengthInsideBlock; i++)
            {
                var data = new List<GField>();
                foreach (var dataBlock in dataBlocks)
                {
                    var newField = gfTable.CreateField(dataBlock.Data[i]);
                    data.Add(newField);
                }

                var toArray = data.ToArray();

                var resultData = parityMatrix.Multiply(toArray);
                var parityData = resultData.Skip(dataBlocks.Count).ToArray();

                for (int y = 0; y < parityDataList.Count; y++)
                {
                    parityDataList[y].Data[i] = parityData[y].Value;
                }
            }

            return parityDataList;
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
                var data = new List<GField>();
                foreach (var dataBlock in dataBlocks)
                {
                    var newField = gfTable.CreateField(dataBlock.Data[i]);
                    data.Add(newField);
                }

                var toArray = data.ToArray();

                var resultData = parityMatrix.Multiply(toArray);
                var parityData = resultData.ToArray();

                for (int y = 0; y < parityDataList.Count; y++)
                {
                    parityDataList[y].Data[i] = parityData[y].Value;
                }
            }

            return parityDataList;
        }



        //public static List<Block<byte>> RecoverData(GFTable gfTable, List<Block<byte>> dataBlocks, List<Block<byte>> recoveryBlocks, int parityBlockCount)
        //{
        //    var combinedData = dataBlocks.Concat(recoveryBlocks).ToList();
        //    var combinedDataWithoutMissingData = combinedData.Where(t => t.Data != null).ToList();
        //    int dataLengthInsideBlock = combinedData.First(t => t.Data != null).Data.Length;


        //    var parMatrix = CreateParityMatrix(gfTable, dataBlocks.Count, parityBlockCount);
        //    //var parMatrixOnly = CreateParityOnlyMatrix(dataBlocks, parityBlockCount);


        //    var missingDataElements = new List<int>();
        //    var missingRows = new List<GField[]>();
        //    var nonMissingRows = new List<GField[]>();

        //    for (int i = 0; i < combinedData.Count; i++)
        //    {
        //        var dataBlock = combinedData[i];
        //        if (dataBlock.Data == null)
        //        {
        //            missingDataElements.Add(i);
        //            missingRows.Add(parMatrix.Data[i]);
        //        }
        //        else
        //        {
        //            nonMissingRows.Add(parMatrix.Data[i]);
        //        }
        //    }

        //    if (missingDataElements.Count > parityBlockCount)
        //    {
        //        throw new InvalidOperationException("Can't recover this data as too much blocks are damaged");
        //    }






        //    //var subspace = new MatrixGField(new int[,] {
        //    //        { 0, 0, 1, 0, 0 },
        //    //        { 0, 0, 0, 1, 0 },
        //    //    });
        //    //If there's more repair data then we need, from all the blocks, just take the amount of data blocks
        //    var rowsNeeded = nonMissingRows.Take(dataBlocks.Count).ToArray();
        //    var subspace = new MatrixGField(rowsNeeded);
        //    Console.WriteLine($"Subspace:\n\r{subspace}");

        //    var inverse = subspace.InverseRuben();
        //    Console.WriteLine($"Inverse:\n\r{inverse}");



        //    foreach (var dataBlock in dataBlocks)
        //    {
        //        if (dataBlock.Data == null)
        //        {
        //            dataBlock.Data = new byte[dataLengthInsideBlock];
        //        }
        //    }




        //    for (int i = 0; i < dataLengthInsideBlock; i++)
        //    {
        //        var data = new List<GField>();
        //        //If there's more repair data then we need, from all the blocks, just take the amount of data blocks
        //        foreach (var dataBlock in combinedDataWithoutMissingData.Take(dataBlocks.Count))
        //        {
        //            var newField = gfTable.CreateField(dataBlock.Data[i]);
        //            data.Add(newField);
        //        }

        //        var toArray = data.ToArray();
        //        //var vector = new CoolVectorField(toArray);

        //        var res = inverse.Multiply(toArray);


        //        //Console.WriteLine($"Recovered data:\n\r{res}");
        //        for (int y = 0; y < res.Length; y++)
        //        {
        //            dataBlocks[y].Data[i] = (byte)res[y].Value;
        //        }
        //    }


        //    return dataBlocks;
        //}





        public static List<Block<uint>> RecoverData2(GFTable gfTable, List<Block<uint>> dataBlocks, List<Block<uint>> recoveryBlocks, int parityBlockCount)
        {
            var combinedData = dataBlocks.Concat(recoveryBlocks).ToList();
            var combinedDataWithoutMissingData = combinedData.Where(t => t.Data != null).ToList();
            int dataLengthInsideBlock = combinedData.First(t => t.Data != null).Data.Length;


            var parMatrix = CreateParityMatrix2(gfTable, dataBlocks.Count, parityBlockCount);
            //var parMatrixOnly = CreateParityOnlyMatrix(dataBlocks, parityBlockCount);

            var missingDataElements = new List<int>();
            var missingRows = new List<GField[]>();
            var nonMissingRows = new List<GField[]>();

            for (int i = 0; i < combinedData.Count; i++)
            {
                var dataBlock = combinedData[i];
                if (dataBlock.Data == null)
                {
                    missingDataElements.Add(i);
                    missingRows.Add(parMatrix.Data[i]);
                }
                else
                {
                    nonMissingRows.Add(parMatrix.Data[i]);
                }
            }

            if (missingDataElements.Count > parityBlockCount)
            {
                throw new InvalidOperationException("Can't recover this data as too much blocks are damaged");
            }






            //var subspace = new MatrixGField(new int[,] {
            //        { 0, 0, 1, 0, 0 },
            //        { 0, 0, 0, 1, 0 },
            //    });
            //If there's more repair data then we need, from all the blocks, just take the amount of data blocks
            var rowsNeeded = nonMissingRows.Take(dataBlocks.Count).ToArray();
            var subspace = new MatrixGField(rowsNeeded);
            Console.WriteLine($"Subspace:\n\r{subspace}");

            var inverse = subspace.InverseRuben();
            Console.WriteLine($"Inverse:\n\r{inverse}");

            var testje = subspace * inverse;

            var blah = subspace.Data.Select(t => t.Select(z => new Field((byte)z.Value)).ToArray()).ToArray();
            var mf = new MatrixField(blah);
            var inversed = mf.Inverse;

            var realInverseData = inversed.Array.Select(t => t.Select(z => gfTable.CreateField((uint)z)).ToArray()).ToArray();
            var realInverse = new MatrixGField(realInverseData);






            foreach (var dataBlock in dataBlocks)
            {
                if (dataBlock.Data == null)
                {
                    dataBlock.Data = new uint[dataLengthInsideBlock];
                }
            }




            for (int i = 0; i < dataLengthInsideBlock; i++)
            {
                var data = new List<GField>();
                //If there's more repair data then we need, from all the blocks, just take the amount of data blocks
                foreach (var dataBlock in combinedDataWithoutMissingData.Take(dataBlocks.Count))
                {
                    var newField = gfTable.CreateField(dataBlock.Data[i]);
                    data.Add(newField);
                }

                var toArray = data.ToArray();
                //var vector = new CoolVectorField(toArray);

                var res = inverse.Multiply(toArray);


                //Console.WriteLine($"Recovered data:\n\r{res}");
                for (int y = 0; y < res.Length; y++)
                {
                    dataBlocks[y].Data[i] = res[y].Value;
                }
            }


            return dataBlocks;
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
                var data = new List<GField>();
                //If there's more repair data then we need, from all the blocks, just take the amount of data blocks
                foreach (var dataBlock in combinedDataWithoutMissingData.Take(dataBlocks.Count))
                {
                    var newField = gfTable.CreateField(dataBlock.Data[i]);
                    data.Add(newField);
                }

                var toArray = data.ToArray();
                //var vector = new CoolVectorField(toArray);

                var res = recoveryMatrixDing.Multiply(toArray);

                //Console.WriteLine($"Recovered data:\n\r{res}");
                for (int y = 0; y < res.Length; y++)
                {
                    combinedDataWithMissingData[y].Data[i] = res[y].Value;
                }
            }


            return dataBlocks;
        }
    }
}
