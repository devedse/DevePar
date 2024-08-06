using DevePar.Galois;
using DevePar.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace DevePar.ParityAlgorithms
{
    public static class ParityGFAlgorithm
    {
        //private const int Base = 2;
        public const bool DebugLogging = false;

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
                if (DebugLogging)
                {
                    Console.WriteLine($"Seeding GField array leftMatrix {i}...");
                }
                leftmatrix[i] = new GField[incount];
            }

            var rightmatrix = new GField[outcount][];
            if (datamissing > 0)
            {
                for (int i = 0; i < outcount; i++)
                {
                    if (DebugLogging)
                    {
                        Console.WriteLine($"Seeding GField array rightMatrix {i}...");
                    }
                    rightmatrix[i] = new GField[outcount];
                }
            }

            var combinedData = dataBlocks.Concat(parityBlocks).ToList();
            int outputrow = 0;

            var database = BaseGFCalculator.CalculateBase(gfTable).ToList();

            for (uint row = 0; row < datamissing; row++)
            {
                if (DebugLogging)
                {
                    Console.WriteLine($"Calculating matrix rows for datamissing: {row}");
                }

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
                if (DebugLogging)
                {
                    Console.WriteLine($"Calculating matrix rows for parmissing: {row}");
                }

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
                var dataExists = dataBlocks.Select(t => t.Data != null).ToArray();

                //GaussElim5(gfTable, outcount, incount, leftmatrixM, rightmatrixM, datamissing);
                GaussElim(gfTable, outcount, incount, leftmatrixM, rightmatrixM, datamissing);
                //return rightmatrixM;
            }

            return leftmatrixM;
        }

        public static GField[] CreateBaseConstants<T>(GFTable gfTable, List<Block<T>> dataBlocks)
        {
            var source_num = dataBlocks.Count;

            GField[] constant = new GField[source_num];

            int n = 0;
            uint temp = 1;
            for (int i = 0; i < source_num; i++)
            {
                while (n <= ushort.MaxValue)
                {
                    temp = gfTable.Mul_Fix(temp, 1);
                    n++;
                    if ((n % 3 != 0) && (n % 5 != 0) && (n % 17 != 0) && (n % 257 != 0))
                        break;
                }
                constant[i] = gfTable.CreateField(temp);
            }

            return constant;
        }

        public static MatrixGField CreateParityMatrixForEncode<T>(GFTable gfTable, List<Block<T>> dataBlocks, List<Block<T>> parityBlocks)
        {
            var baseConstants = CreateBaseConstants(gfTable, dataBlocks);
            var source_num = dataBlocks.Count;
            var parity_num = parityBlocks.Count;


            // Generate the matrix for encoding
            GField[][] constantMatrix = new GField[parity_num][];
            for (uint j = 0; j < parity_num; j++)
            {
                constantMatrix[j] = new GField[source_num];
                for (int i = 0; i < source_num; i++)
                {
                    // Assume that galois_power() is implemented in gfTable.Power()
                    constantMatrix[j][i] = baseConstants[i].Pow(j);
                }
            }

            var encodeMatrix = new MatrixGField(constantMatrix);

            return encodeMatrix;
        }

        public static MatrixGField CreateParityMatrixForRecovery2<T>(GFTable gfTable, List<Block<T>> dataBlocks, List<Block<T>> parityBlocks)
        {
            var dataExists = dataBlocks.Select(t => t.Data != null).ToArray();
            Dictionary<int, int> columnMapping = CreateColumnMapping(dataExists);
            var parityExists = parityBlocks.Select(t => t.Data != null).ToArray();

            //block_lost
            var block_lost = dataBlocks.Where(t => t.Data == null).Count();
            //source_num
            var source_num = dataBlocks.Count;
            var availableParityBlocks = parityBlocks.Where(t => t.Data != null).Count();
            var parityBlocksCount = parityBlocks.Count;

            var superMatrix = new GField[block_lost][];
            for (int i = 0; i < block_lost; i++)
            {
                superMatrix[i] = new GField[source_num];
            }

            //which parity block to substitute for missing source block
            ushort[] id = new ushort[availableParityBlocks];

            int j = 0;
            for (int i = 0; (i < parityBlocksCount) && (j < block_lost); i++)
            {
                // ignore blocks marked as unavailable
                if (parityExists[i])
                {
                    id[j++] = (ushort)i;
                }
            }

            if (j < block_lost)
            {
                throw new InvalidOperationException("Not enough recovery blocks.");
            }



            // create a matrix of only parity blocks that exist and are used
            int n = 0;
            uint constant = 1;
            for (int i = 0; i < source_num; i++)
            {
                //Set values vertically row by row
                while (n <= 65535)
                {
                    constant = gfTable.Mul_Fix(constant, 1);
                    n++;
                    if ((n % 3 != 0) && (n % 5 != 0) && (n % 17 != 0) && (n % 257 != 0))
                        break;
                }

                int k = 0;
                for (j = 0; j < source_num; j++)
                {
                    // row j, column i
                    if (!dataExists[j])
                    {   // If the corresponding part is supplemented with a parity block
                        superMatrix[k][i] = gfTable.CreateField(gfTable.Pow(constant, id[k]));
                        k++;
                    }
                }
            }

            var superDuperMatrix = new MatrixGField(superMatrix);

            GaussElim6(gfTable, superDuperMatrix, block_lost, source_num, dataExists);

            //ColumnSwapper(superDuperMatrix, dataExists);

            return superDuperMatrix;
        }

        private static Dictionary<int, int> CreateColumnMapping(bool[] dataExists)
        {
            int numColumns = dataExists.Length;

            // Count the number of existing data columns
            int numDataExists = dataExists.Count(x => x);

            Dictionary<int, int> columnMapping = new Dictionary<int, int>();
            int indexExists = 0, indexMissing = numDataExists;

            for (int i = 0; i < numColumns; i++)
            {
                if (dataExists[i])
                {
                    columnMapping[i] = indexExists++;
                }
                else
                {
                    columnMapping[i] = indexMissing++;
                }
            }

            return columnMapping;
        }

        private static Dictionary<int, int> CreateColumnMappingInverted(bool[] dataExists)
        {
            var columnMapping = CreateColumnMapping(dataExists);

            var aaa = new Dictionary<int, int>();
            foreach (var kvp in columnMapping)
            {
                aaa.Add(kvp.Value, kvp.Key);
            }

            return aaa;
        }


        private static void ColumnSwapper(MatrixGField superDuperMatrix, bool[] dataExists)
        {
            Dictionary<int, int> columnMapping = CreateColumnMapping(dataExists);

            //var tempColumn = new GField[superDuperMatrix.Rows];
            //void SwapColumns(int from, int to)
            //{
            //    for (int row = 0; row < superDuperMatrix.Rows; row++)
            //    {
                    
            //    }
            //}

            // Bubble sort based on the created column mapping
            for (int i = 0; i < superDuperMatrix.Columns - 1; i++)
            {
                for (int j = 0; j < superDuperMatrix.Columns - i - 1; j++)
                {
                    if (columnMapping[j] > columnMapping[j + 1])
                    {
                        // Swap columns j and j+1
                        var tempColumn = new GField[superDuperMatrix.Rows];
                        for (int row = 0; row < superDuperMatrix.Rows; row++)
                        {
                            tempColumn[row] = superDuperMatrix[row, j];
                            superDuperMatrix[row, j] = superDuperMatrix[row, j + 1];
                            superDuperMatrix[row, j + 1] = tempColumn[row];
                        }

                        // Swap column mappings
                        int tempMapping = columnMapping[j];
                        columnMapping[j] = columnMapping[j + 1];
                        columnMapping[j + 1] = tempMapping;
                    }
                }
            }
        }

        public static void GaussElim6(GFTable gfTable, MatrixGField mat, int rows, int columns, bool[] dataExists)
        {
            int pivot = 0;

            var columnMapping = CreateColumnMapping(dataExists);

            for (int i = 0; i < rows; i++)
            {
                while ((pivot < columns) && dataExists[pivot])
                {
                    pivot++;
                }

                // Divide the row by element i,pivot
                var factor = mat[i, pivot]; //mat(j, pivot) should be non-zero


                if (factor.Value > 1)
                {
                    // If factor is greater than 1, divide by factor to make it 1
                    mat[i, pivot] = gfTable.CreateField(1); //This is a way to finish the queue with one
                    DivideRow(mat, i, factor);
                }
                else if (factor.Value == 0)
                {
                    // If factor = 0, you can't compute the inverse of the matrix
                    throw new InvalidOperationException("RS computation error.");
                }


                // if the same pivot column in another row is non-zero, to make it 0
                // XOR multiplied by row i
                for (int j = rows - 1; j >= 0; j--)
                {
                    if (j == i)
                        continue;   // skip the same line

                    // pivot column value in row j
                    factor = mat[j, pivot];

                    // Due to the previous calculation, the value of the pivot column in row i is always 1
                    // so this factor is the multiplier
                    mat[j, pivot] = gfTable.CreateField(0);

                    GaloisRegionMultiply(mat.Data[i], mat.Data[j], (uint)mat.Columns, factor);
                }

                pivot++;
            }
        }

        private static void DivideRow(MatrixGField mat, int row, GField divisor)
        {
            var reciprocal = divisor.Reciprocal();
            for (int i = 0; i < mat.Columns; i++)
            {
                mat[row, i] /= divisor;
            }
        }

        public static void GaloisRegionMultiply(GField[] region1, GField[] region2, uint count, GField factor)
        {
            for (int i = 0; i < count; i++)
            {
                var blah = region1[i] * factor;
                region2[i] = region2[i] - blah;
            }
        }

        public static void GaussElim5(GFTable gfTable, uint rows, uint leftcols, MatrixGField leftmatrix, MatrixGField rightmatrix, uint datamissing)
        {
            var w = Stopwatch.StartNew();
            int pivot = 0;
            for (int i = 0; i < datamissing; i++)
            {
                if (DebugLogging)
                {
                    Console.WriteLine($"{w.Elapsed} GaussElim Outer loop > Gaus row: {i}");
                    w.Restart();
                }

                while ((pivot < rows) && rightmatrix[i, pivot].Value == 0)
                    pivot++;

                var pivotValue = rightmatrix[i, pivot];
                if (pivotValue.Value == 0)
                {
                    throw new InvalidOperationException("RS computation error.");
                }

                // Make the pivot value 1 if it is not already
                if (pivotValue.Value > 1)
                {
                    rightmatrix[i, pivot] = gfTable.CreateField(1);
                    for (int j = i + 1; j < rows; j++)
                    {
                        if (rightmatrix[i, j].Value != 0)
                        {
                            rightmatrix[i, j] /= pivotValue;
                        }
                    }
                }

                for (int j = 0; j < rows; j++)
                {
                    if (i != j)
                    {
                        var factor = rightmatrix[j, pivot];
                        rightmatrix[j, pivot] = gfTable.CreateField(0);
                        for (int k = pivot + 1; k < rows; k++)
                        {
                            if (rightmatrix[i, k].Value != 0)
                            {
                                rightmatrix[j, k] -= rightmatrix[i, k] * factor;
                            }
                        }
                    }
                }
                pivot++;
            }
        }

        public static void GaussElim4(GFTable gfTable, uint rows, uint leftcols, MatrixGField leftmatrix, MatrixGField rightmatrix, uint datamissing, bool[] dataExists)
        {
            int pivot = 0;
            for (int i = 0; i < rows; i++)
            {
                //while (pivot < rightmatrix.Columns && dataExists[pivot])
                //    pivot++;

                var factor = rightmatrix[i, i];

                if (factor.Value > 1)
                {
                    rightmatrix[i, i] = gfTable.CreateField(1);
                    DivideRow(rightmatrix, i, factor);
                }
                else if (factor.Value == 0)
                {
                    throw new InvalidOperationException($"RS computation error at pivot {pivot}");
                }

                for (int j = (int)rows - 1; j >= 0; j--)
                {
                    if (j == i)
                        continue;

                    //int row_start2 = rightmatrix.Columns * j;
                    factor = rightmatrix[j, i];
                    rightmatrix[j, i] = gfTable.CreateField(0);

                    GaloisRegionMultiply(rightmatrix.Data[i], rightmatrix.Data[j], (uint)rightmatrix.Columns, factor);
                }
                pivot++;
            }
        }



        public static void GaussElim3(GFTable gfTable, uint rows, uint leftcols, MatrixGField leftmatrix, MatrixGField rightmatrix, uint datamissing)
        {
            var rowsInt = (int)rows;
            var leftColsInt = (int)leftcols;

            var w = Stopwatch.StartNew();
            for (int row = 0; row < datamissing; row++)
            {
                if (DebugLogging)
                {
                    Console.WriteLine($"{w.Elapsed} GaussElim Outer loop > Gaus row: {row}");
                    w.Restart();
                }
                var pivotValue = rightmatrix[row, row];

                if (pivotValue.Value == 0)
                {
                    throw new InvalidOperationException("RS computation error.");
                }

                if (pivotValue.Value != 1)
                {
                    var reciprocalPivotValue = pivotValue.Reciprocal();
                    Parallel.For(0, leftColsInt, col =>
                    {
                        if (leftmatrix[row, col].Value != 0)
                        {
                            leftmatrix[row, col] *= reciprocalPivotValue;
                        }
                    });

                    rightmatrix[row, row] = gfTable.CreateField(1);

                    Parallel.For(row + 1, rowsInt, col =>
                    {
                        if (rightmatrix[row, col].Value != 0)
                        {
                            rightmatrix[row, col] *= reciprocalPivotValue;
                        }
                    });
                }

                Parallel.For(0, rowsInt, row2 =>
                {
                    if (row != row2)
                    {
                        var scaleValue = rightmatrix[row2, row];
                        if (scaleValue.Value != 0)
                        {
                            Parallel.For(0, leftColsInt, col =>
                            {
                                var value = leftmatrix[row, col].Value;
                                if (value != 0)
                                {
                                    var leftValue = gfTable.CreateField(value);
                                    leftmatrix[row2, col] -= leftValue * scaleValue;
                                }
                            });

                            Parallel.For(row, rowsInt, col =>
                            {
                                var value = rightmatrix[row, col].Value;
                                if (value != 0)
                                {
                                    var rightValue = gfTable.CreateField(value);
                                    rightmatrix[row2, col] -= rightValue * scaleValue;
                                }
                            });
                        }
                    }
                });
            }
        }


        public static void GaussElim2(GFTable gfTable, uint rows, uint leftcols, MatrixGField leftmatrix, MatrixGField rightmatrix, uint datamissing)
        {
            int numPivots = 0;
            for (int j = 0; j < rows && numPivots < datamissing; j++)
            {
                // Find a pivot row for this column
                int pivotRow = numPivots;
                while (pivotRow < rows && rightmatrix[pivotRow, j].Value == 0)
                    pivotRow++;

                if (pivotRow == rows)
                    continue;  // Cannot eliminate on this column

                SwapRows(leftmatrix, numPivots, pivotRow);
                pivotRow = numPivots;
                numPivots++;

                // Simplify the pivot row
                MultiplyRow(leftmatrix, pivotRow, rightmatrix[pivotRow, j].Reciprocal());

                // Eliminate rows below
                for (int i = pivotRow + 1; i < rows; i++)
                    AddRows(leftmatrix, pivotRow, i, rightmatrix[i, j].Negate());
            }

            // Compute reduced row echelon form (RREF)
            for (int i = numPivots - 1; i >= 0; i--)
            {
                // Find pivot
                int pivotCol = 0;
                while (pivotCol < leftcols && rightmatrix[i, pivotCol].Value == 0)
                    pivotCol++;

                if (pivotCol == leftcols)
                    continue;  // Skip this all-zero row

                // Eliminate rows above
                for (int j = i - 1; j >= 0; j--)
                    AddRows(leftmatrix, i, j, rightmatrix[j, pivotCol].Negate());
            }
        }

        private static void SwapRows(MatrixGField matrix, int row1, int row2)
        {
            var temp = matrix.Data[row1];
            matrix.Data[row1] = matrix.Data[row2];
            matrix.Data[row2] = temp;
        }

        private static void MultiplyRow(MatrixGField matrix, int row, GField reciprocal)
        {
            for (int i = 0; i < matrix.Columns; i++)
            {
                matrix[row, i] *= reciprocal;
            }
        }

        private static void AddRows(MatrixGField matrix, int row1, int row2, GField negate)
        {
            for (int i = 0; i < matrix.Columns; i++)
            {
                matrix[row2, i] += matrix[row1, i] * negate;
            }
        }

        public static void GaussElim(GFTable gfTable, uint rows, uint leftcols, MatrixGField leftmatrix, MatrixGField rightmatrix, uint datamissing)
        {
            var w = Stopwatch.StartNew();
            for (int row = 0; row < datamissing; row++)
            {
                if (DebugLogging)
                {
                    Console.WriteLine($"{w.Elapsed} GaussElim Outer loop > Gaus row: {row}");
                    w.Restart();
                }

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
            var parityMatrix2 = CreateParityMatrixForEncode(gfTable, dataBlocks, parityDataList);

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
                var resultData2 = parityMatrix2.Multiply(toArray);
                var parityData = resultData.ToArray();
                var parityData2 = resultData2.ToArray();

                for (int y = 0; y < parityDataList.Count; y++)
                {
                    //parityDataList[y].Data[i] = parityData[y].Value;
                    parityDataList[y].Data[i] = parityData2[y].Value;
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

        public static void MatrixCompareColumns(List<Block<uint>> dataBlocks, List<Block<uint>> recoveryBlocks, MatrixGField goodMatrix, MatrixGField badMatrix)
        {
            if (goodMatrix.Columns != badMatrix.Columns)
            {
                throw new InvalidOperationException("MAG NIET");
            }


            Dictionary<int, int> columnMapping = new Dictionary<int, int>();

            for (int i = 0; i < Math.Min(goodMatrix.Rows, badMatrix.Rows); i++)
            {
                for (int j = 0; j < goodMatrix.Columns; j++)
                {
                    for (int k = 0; k < badMatrix.Columns; k++)
                    {
                        if (goodMatrix[i, j] == badMatrix[i, k])
                        {
                            if (!columnMapping.ContainsKey(j))
                            {
                                columnMapping.Add(j, k);
                            }
                            break;
                        }
                    }
                }
            }

            StringBuilder sb = new StringBuilder();

            // Getting missing data blocks
            var missingDataBlocks = dataBlocks
                .Select((block, index) => new { Block = block, Index = index })
                .Where(x => x.Block.Data == null)
                .Select(x => x.Index);

            string dataMessage = missingDataBlocks.Any()
                ? $"Data: {string.Join(", ", missingDataBlocks)}"
                : string.Empty;

            // Getting missing recovery blocks
            var missingRecoveryBlocks = recoveryBlocks
                .Select((block, index) => new { Block = block, Index = index })
                .Where(x => x.Block.Data == null)
                .Select(x => x.Index);

            string parityMessage = missingRecoveryBlocks.Any()
                ? $"Parity: {string.Join(", ", missingRecoveryBlocks)}"
                : string.Empty;

            sb.AppendLine($"({dataBlocks.Count},{recoveryBlocks.Count}) {dataMessage} {parityMessage}");

            if (columnMapping.Values.All(t => t == 0))
            {
                return;
            }

            foreach (KeyValuePair<int, int> pair in columnMapping)
            {
                var same = pair.Key == pair.Value ? " same" : " DIFFERENT";
                sb.AppendLine($"{pair.Key} => {pair.Value}{same}");
            }

            var resultString = sb.ToString();
            Console.WriteLine(resultString);
            Trace.WriteLine(resultString);
            Debug.WriteLine(resultString);

            File.AppendAllText("outputje.txt", resultString + Environment.NewLine + Environment.NewLine);
        }

        public static List<Block<uint>> RecoverData3(GFTable gfTable, List<Block<uint>> dataBlocks, List<Block<uint>> recoveryBlocks, int parityBlockCount)
        {
            var combinedData = dataBlocks.Concat(recoveryBlocks).ToList();

            var dataExists = dataBlocks.Select(t => t.Data != null).ToArray();
            Dictionary<int, int> columnMappingInverted = CreateColumnMappingInverted(dataExists);

            var combinedDataWithoutMissingData = combinedData.Where(t => t.Data != null).ToList();
            var combinedDataWithMissingData = combinedData.Where(t => t.Data == null).ToList();
            int dataLengthInsideBlock = combinedData.First(t => t.Data != null).Data.Length;


            var ww = Stopwatch.StartNew();
            var recoveryMatrixDing = CreateParityMatrixForRecovery(gfTable, dataBlocks, recoveryBlocks);
            var el1 = ww.Elapsed;

            ww.Restart();
            var recoveryMatrixDing2 = CreateParityMatrixForRecovery2(gfTable, dataBlocks, recoveryBlocks);
            var el2 = ww.Elapsed;

            //MatrixCompareColumns(dataBlocks, recoveryBlocks, recoveryMatrixDing, recoveryMatrixDing2);


            var dataBlocksWithMissingData = dataBlocks.Where(t => t.Data == null).ToList();

            Console.WriteLine($"Parity matrix 1: {el1}");
            Console.WriteLine($"Parity matrix 2: {el2}");

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

                //Dit is een smerige super coole re-order van de veldjes
                var reorderToArray = toArray.Select((field, i) => new { field, i }).OrderBy(t => columnMappingInverted[t.i]).Select(t => t.field).ToArray();

                var res2 = recoveryMatrixDing2.Multiply(reorderToArray);

                //Console.WriteLine($"Recovered data:\n\r{res}");
                for (int y = 0; y < res2.Length; y++)
                {
                    combinedDataWithMissingData[y].Data[i] = res2[y].Value;
                }

                //for (int y = 0; y < res.Length; y++)
                //{
                //    dataBlocks[y].Data[i] = res[y].Value;
                //}
            }


            return dataBlocks;
        }
    }
}
