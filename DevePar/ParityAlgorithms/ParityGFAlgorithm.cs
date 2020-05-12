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

        //public static MatrixGField CreateParityMatrix(GFTable gfTable, int dataBlocksCount, int parityBlockCount)
        //{
        //    int totalBlocks = dataBlocksCount + parityBlockCount;
        //    if (totalBlocks > 256)
        //    {
        //        throw new InvalidOperationException("A total of more then 256 blocks is not supported");
        //    }

        //    var identityMatrix = MatrixGField.CreateIdentityMatrix(gfTable, dataBlocksCount);
        //    var parityOnlyMatrix = CreateParityOnlyMatrix(gfTable, dataBlocksCount, parityBlockCount);

        //    var combinedMatrix = identityMatrix.AddRowsAtTheEnd(parityOnlyMatrix);
        //    return combinedMatrix;
        //}


        //public static MatrixGField CreateParityOnlyMatrix(GFTable gfTable, int dataBlocksCount, int parityBlockCount)
        //{
        //    int totalBlocks = dataBlocksCount + parityBlockCount;
        //    if (totalBlocks > 256)
        //    {
        //        throw new InvalidOperationException("A total of more then 256 blocks is not supported");
        //    }

        //    var parityMatrixArray = new GField[parityBlockCount, dataBlocksCount];



        //    var baseList = BaseGFCalculator.CalcBase(gfTable).ToList();
        //    //Copy parity part of the matrix
        //    for (int column = 0; column < parityMatrixArray.GetLength(1); column++)
        //    {
        //        for (uint row = 0; row < parityBlockCount; row++)
        //        {
        //            var val = baseList[column].Pow(row + 1);
        //            parityMatrixArray[row, column] = val;
        //        }
        //    }


        //    return new MatrixGField(parityMatrixArray);
        //}

        public static MatrixGField CreateParityOnlyMatrix2(GFTable gfTable, int dataBlocksCount, int parityBlockCount)
        {
            int totalBlocks = dataBlocksCount + parityBlockCount;
            if (totalBlocks > 256)
            {
                throw new InvalidOperationException("A total of more then 256 blocks is not supported");
            }

            var parityMatrixArray = new GField[totalBlocks, dataBlocksCount];



            var baseList = BaseGFCalculator.CalcBase2(gfTable).ToList();
            //Copy parity part of the matrix
            for (uint row = 0; row < totalBlocks; row++)
            {
                for (int column = 0; column < parityMatrixArray.GetLength(1); column++)
                {
                    var val = baseList[column].Pow(row);
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

        public static List<Block<byte>> GenerateParityData2(GFTable gfTable, List<Block<byte>> dataBlocks, int parityBlockCount)
        {
            int dataLengthInsideBlock = dataBlocks.First().Data.Length;

            var parityMatrix = CreateParityOnlyMatrix2(gfTable, dataBlocks.Count, parityBlockCount);

            var parityDataList = new List<Block<byte>>();
            for (int i = 0; i < parityBlockCount; i++)
            {
                parityDataList.Add(new Block<byte>() { Data = new byte[dataLengthInsideBlock] });
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
                    parityDataList[y].Data[i] = (byte)parityData[y].Value;
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


        public static List<Block<byte>> RecoverData2(GFTable gfTable, List<Block<byte>> dataBlocks, List<Block<byte>> recoveryBlocks, int parityBlockCount)
        {
            var combinedData = dataBlocks.Concat(recoveryBlocks).ToList();
            var combinedDataWithoutMissingData = combinedData.Where(t => t.Data != null).ToList();
            int dataLengthInsideBlock = combinedData.First(t => t.Data != null).Data.Length;


            var parMatrix = CreateParityOnlyMatrix2(gfTable, dataBlocks.Count, parityBlockCount);
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
                    dataBlock.Data = new byte[dataLengthInsideBlock];
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
                    dataBlocks[y].Data[i] = (byte)res[y].Value;
                }
            }


            return dataBlocks;
        }
    }
}
