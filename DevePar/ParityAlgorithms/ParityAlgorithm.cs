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
        private const int Base = 2;

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
                    var val = Field.pow(new Field((byte)(column + Base)), row);
                    parityMatrixArray[row + dataBlocks.Count, column] = val;
                }
            }

            return new MatrixField(parityMatrixArray);
        }

        public static MatrixField CreateParityMatrixBig(List<Block<byte>> dataBlocks, int parityBlockCount)
        {
            int totalBlocks = dataBlocks.Count + parityBlockCount;
            if (totalBlocks > 256)
            {
                throw new InvalidOperationException("A total of more then 256 blocks is not supported");
            }

            var theMatrix = MatrixField.CreateIdentityMatrix(dataBlocks.Count + parityBlockCount);


            //Copy parity part of the matrix
            for (byte column = 0; column < dataBlocks.Count; column++)
            {
                for (byte row = 0; row < parityBlockCount; row++)
                {
                    var val = Field.pow(new Field((byte)(column + Base)), row);
                    theMatrix[row + dataBlocks.Count, column] = val;
                }
            }

            return theMatrix;
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
                    var val = Field.pow(new Field((byte)(column + Base)), row);
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
                foreach (var dataBlock in dataBlocks)
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





        public static List<Block<byte>> RecoverData(List<Block<byte>> dataBlocks, List<Block<byte>> recoveryBlocks, int parityBlockCount)
        {
            var combinedData = dataBlocks.Concat(recoveryBlocks).ToList();
            var combinedDataWithoutMissingData = combinedData.Where(t => t.Data != null).ToList();
            int dataLengthInsideBlock = combinedData.First(t => t.Data != null).Data.Length;


            var parMatrix = ParityAlgorithm.CreateParityMatrix(dataBlocks, parityBlockCount);
            //var parMatrixOnly = ParityAlgorithm.CreateParityOnlyMatrix(dataBlocks, parityBlockCount);


            var missingDataElements = new List<int>();
            var missingRows = new List<Field[]>();
            var nonMissingRows = new List<Field[]>();

            for (int i = 0; i < combinedData.Count; i++)
            {
                var dataBlock = combinedData[i];
                if (dataBlock.Data == null)
                {
                    missingDataElements.Add(i);
                    missingRows.Add(parMatrix.Array[i]);
                }
                else
                {
                    nonMissingRows.Add(parMatrix.Array[i]);
                }
            }

            if (missingDataElements.Count > parityBlockCount)
            {
                throw new InvalidOperationException("Can't recover this data as too much blocks are damaged");
            }






            //var subspace = new MatrixField(new int[,] {
            //        { 0, 0, 1, 0, 0 },
            //        { 0, 0, 0, 1, 0 },
            //    });
            //If there's more repair data then we need, from all the blocks, just take the amount of data blocks
            var rowsNeeded = nonMissingRows.Take(dataBlocks.Count).ToArray();
            var subspace = new MatrixField(rowsNeeded);
            Console.WriteLine($"Subspace:\n\r{subspace}");

            var inverse = subspace.InverseRuben();
            var inverse2 = subspace.Inverse;
            Console.WriteLine($"Inverse:\n\r{inverse}");


            if (inverse2 != inverse)
            {
                Console.WriteLine("NU");
            }


            foreach (var dataBlock in dataBlocks)
            {
                if (dataBlock.Data == null)
                {
                    dataBlock.Data = new byte[dataLengthInsideBlock];
                }
            }




            for (int i = 0; i < dataLengthInsideBlock; i++)
            {
                var data = new List<Field>();
                //If there's more repair data then we need, from all the blocks, just take the amount of data blocks
                foreach (var dataBlock in combinedDataWithoutMissingData.Take(dataBlocks.Count))
                {
                    data.Add(new Field(dataBlock.Data[i]));
                }

                var toArray = data.ToArray();
                var vector = new CoolVectorField(toArray);

                var res = inverse * vector;


                //Console.WriteLine($"Recovered data:\n\r{res}");
                for (int y = 0; y < res.Length; y++)
                {
                    dataBlocks[y].Data[i] = res.Data[y].Value;
                }
            }


            return dataBlocks;
        }



        public static List<Block<byte>> RecoverDataV2(List<Block<byte>> dataBlocks, List<Block<byte>> recoveryBlocks, int parityBlockCount)
        {
            var combinedData = dataBlocks.Concat(recoveryBlocks).ToList();
            var combinedDataWithoutMissingData = combinedData.Where(t => t.Data != null).ToList();
            int dataLengthInsideBlock = dataBlocks.First(t => t.Data != null).Data.Length;


            var parMatrix = ParityAlgorithm.CreateParityMatrix(dataBlocks, parityBlockCount);
            var parMatrixOnly = ParityAlgorithm.CreateParityOnlyMatrix(dataBlocks, parityBlockCount);
            var parMatrixBig = ParityAlgorithm.CreateParityMatrixBig(dataBlocks, parityBlockCount);


            var missingDataElements = new List<int>();
            var missingRows = new List<Field[]>();
            var nonMissingRows = new List<Field[]>();

            for (int i = 0; i < combinedData.Count; i++)
            {
                var dataBlock = combinedData[i];
                if (dataBlock.Data == null)
                {
                    missingDataElements.Add(i);
                    missingRows.Add(parMatrix.Array[i]);
                }
                else
                {
                    nonMissingRows.Add(parMatrix.Array[i]);
                }
            }

            if (missingDataElements.Count > parityBlockCount)
            {
                throw new InvalidOperationException("Can't recover this data as too much blocks are damaged");
            }






            //var subspace = new MatrixField(new int[,] {
            //        { 0, 0, 1, 0, 0 },
            //        { 0, 0, 0, 1, 0 },
            //    });
            //var subspace = parMatrix;
            //Console.WriteLine($"parMatrix:\n\r{parMatrixBig}");

            //var inverse = subspace.Inverse;
            //Console.WriteLine($"Inverse:\n\r{inverse}");

            Console.WriteLine($"Parmatrix2:\n\r{parMatrixBig}");

            //for (int i = 0; i < combinedData.Count; i++)
            //{
            //    var dataBlock = combinedData[i];
            //    if (dataBlock.Data != null)
            //    {
            //        for (int y = 0; y < dataBlocks.Count; y++)
            //        {
            //            parMatrixBig.Array[i][y].Value = (byte)((y == i) ? 1 : 0);
            //        }
            //    }
            //}

            //for (int i = dataBlocks.Count + 1; i < combinedData.Count; i++)
            //{
            //    parMatrixBig[i, i].Value = 0;
            //}

            var rowsNeeded = nonMissingRows.Take(dataBlocks.Count).ToArray();
            var theOtherOne = new MatrixField(rowsNeeded);

            Console.WriteLine($"theOtherOne:\n\r{theOtherOne}");

            Console.WriteLine($"theOtherOne2:\n\r{theOtherOne.Inverse}");

            var restoreMatrix = parMatrixBig;
            //var restoreMatrix = parMatrixBig.Inverse;



            //var dingetje = parMatrixBig * parMatrixBig.Inverse;
            //Console.WriteLine($"Ruben wil dit:\n\r{dingetje}");

            restoreMatrix = new MatrixField(new int[,] {
                {1, 0, 0, 0, 0},
                {1, 1, 1, 1, 1},
                {0, 1, 0, 0, 0},
                {0, 0, 1, 0, 0},
                {0, 0, 0, 1, 0},
                {1, 1, 1, 1, 1},
                {1, 2, 3, 4, 5},
            });

            //for (int ggg = 0; ggg < 256; ggg++)
            //{
            //Console.WriteLine($"HALLO: {ggg}");
            restoreMatrix = new MatrixField(new int[,] {
                    {1, 0, 0, 0, 0, 0, 0},
                    {1, 0, 1, 1, 1, 1, 0},
                    {0, 0, 1, 0, 0, 0, 0},
                    {0, 0, 0, 1, 0, 0, 0},
                    {0, 0, 0, 0, 1, 0, 0},
                    {0, 0, 0, 0, 0, 1, 0},
                    {1, 2, 3, 4, 5, 70, 0},
                });

            //restoreMatrix = restoreMatrix.Transpose();
            Console.WriteLine($"Restore matrix:\n\r{restoreMatrix}");


            var resulta = new Field(2) * new Field(5);
            var resultb = new Field(70) * new Field(8);


            for (int i = 0; i < dataLengthInsideBlock; i++)
            {
                var data = new List<Field>();
                foreach (var dataBlock in combinedData)
                {
                    if (dataBlock.Data != null)
                    {
                        data.Add(new Field(dataBlock.Data[i]));
                    }
                    else
                    {
                        data.Add(new Field(0));
                    }
                }

                var toArray = data.ToArray();
                var vector = new CoolVectorField(toArray);

                Console.WriteLine($"Vector:\n\r{vector}");

                var res = restoreMatrix * vector;

                //Console.WriteLine($"Recovered data:\n\r{res}");
            }
            //}


            return combinedData;
        }
    }
}
