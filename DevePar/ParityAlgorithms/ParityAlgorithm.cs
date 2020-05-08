using DevePar.Galois;
using DevePar.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DevePar.ParityAlgorithms
{
    public static class ParityAlgorithm
    {
        public static MatrixField CreateParityMatrix(List<Block<byte>> dataBlocks, int parityBlockCount)
        {
            int totalBlocks = dataBlocks.Count + parityBlockCount;
            if (totalBlocks > 256)
            {
                throw new InvalidOperationException("A total of more then 256 blocks is not supported");
            }

            var identityMatrix = MatrixField.CreateIdentityMatrix(dataBlocks.Count);
            var parityMatrixArray = new Field[parityBlockCount + dataBlocks.Count, dataBlocks.Count];

            //Copy identity matrix first
            for (byte column = 0; column < identityMatrix.Columns; column++)
            {
                for (byte row = 0; row < identityMatrix.Rows; row++)
                {
                    parityMatrixArray[row, column] = identityMatrix.Array[row][column];
                }
            }

            //Copy parity part of the matrix
            for (byte column = 0; column < parityMatrixArray.GetLength(1); column++)
            {
                for (byte row = 0; row < parityBlockCount; row++)
                {
                    var val = Field.pow(new Field((byte)(column + 1)), row);
                    parityMatrixArray[row + identityMatrix.Rows, column] = val;
                }
            }

            return new MatrixField(parityMatrixArray);
        }

        public static MatrixField CreateParityOnlyMatrix(List<Block<byte>> dataBlocks, int parityBlockCount)
        {
            int totalBlocks = dataBlocks.Count + parityBlockCount;
            if (totalBlocks > 256)
            {
                throw new InvalidOperationException("A total of more then 256 blocks is not supported");
            }

            var parityMatrixArray = new Field[parityBlockCount, dataBlocks.Count];

            //Copy parity part of the matrix
            for (byte column = 0; column < parityMatrixArray.GetLength(1); column++)
            {
                for (byte row = 0; row < parityBlockCount; row++)
                {
                    var val = Field.pow(new Field((byte)(column + 1)), row);
                    parityMatrixArray[row, column] = val;
                }
            }


            return new MatrixField(parityMatrixArray);
        }

        public static List<Block<byte>> GenerateParityData(List<Block<byte>> dataBlocks, int parityBlockCount)
        {
            int dataLengthInsideBlock = dataBlocks.First().Data.Length;

            var parityMatrix = CreateParityOnlyMatrix(dataBlocks, parityBlockCount);

            var parityDataList = new List<Block<byte>>();
            for (int i = 0; i < parityBlockCount; i++)
            {
                parityDataList.Add(new Block<byte>() { Data = new byte[dataLengthInsideBlock] });
            }

            for (int i = 0; i < dataLengthInsideBlock; i++)
            {
                var data = new List<Field>();
                foreach(var dataBlock in dataBlocks)
                {
                    data.Add(new Field(dataBlock.Data[i]));
                }

                var toArray = data.ToArray();
                var vector = new CoolVectorField(toArray);

                var parityData = parityMatrix * vector;

                for (int y = 0; y < parityDataList.Count; y++)
                {
                    parityDataList[y].Data[i] = parityData.Data[y].Value;
                }
            }

            return parityDataList;
        }
    }
}
