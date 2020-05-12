using DevePar.Galois;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DevePar.LinearAlgebra
{
    public class MatrixGField
    {
        public GField[][] Data { get; }

        public int Rows { get; }
        public int Columns { get; }

        private GFTable GFTable => Data[0][0].Table;

        /// <summary>Constructs an empty matrix of the given size.</summary>
        /// <param name="rows">Number of rows.</param>
        /// <param name="columns">Number of columns.</param>
        private MatrixGField(int rows, int columns)
        {
            this.Rows = rows;
            this.Columns = columns;
            Data = new GField[rows][];
            for (int i = 0; i < rows; i++)
            {
                Data[i] = new GField[columns];
            }
        }

        /// <summary>Constructs a matrix of the given size and assigns a given value to all diagonal elements.</summary>
        /// <param name="rows">Number of rows.</param>
        /// <param name="columns">Number of columns.</param>
        /// <param name="value">Value to assign to the diagnoal elements.</param>
        public MatrixGField(int rows, int columns, GField value) : this(rows, columns)
        {
            for (int i = 0; i < rows; i++)
            {
                Data[i][i] = value;
            }
        }

        /// <summary>Constructs a matrix from the given array.</summary>
        /// <param name="value">The array the matrix gets constructed from.</param>
        public MatrixGField(GField[][] value)
        {
            Rows = value.Length;
            Columns = value[0].Length;

            for (int i = 0; i < Rows; i++)
            {
                if (value[i].Length != Columns)
                {
                    throw new ArgumentException("Argument out of range.");
                }
            }

            Data = value;
        }

        /// <summary>Constructs a matrix from the given array.</summary>
        /// <param name="value">The array the matrix gets constructed from.</param>
        public MatrixGField(GField[,] value) : this(value.GetLength(0), value.GetLength(1))
        {
            for (int i = 0; i < Rows; i++)
            {
                for (int y = 0; y < Columns; y++)
                {
                    Data[i][y] = value[i, y];
                }
            }
        }

        /// <summary>Constructs a matrix from the given array.</summary>
        /// <param name="value">The array the matrix gets constructed from.</param>
        public MatrixGField(GFTable table, uint[,] value) : this(value.GetLength(0), value.GetLength(1))
        {
            for (int i = 0; i < Rows; i++)
            {
                for (int y = 0; y < Columns; y++)
                {
                    Data[i][y] = table.CreateField(value[i, y]);
                }
            }
        }

        public static MatrixGField CreateIdentityMatrix(GFTable table, int size)
        {
            uint[,] values = new uint[size, size];
            for (int i = 0; i < size; i++)
            {
                values[i, i] = 1;
            }

            return new MatrixGField(table, values);
        }

        /// <summary>Determines weather two instances are equal.</summary>
        public override bool Equals(object obj)
        {
            return Equals(this, (MatrixGField)obj);
        }

        /// <summary>Determines weather two instances are equal.</summary>
        public static bool Equals(MatrixGField left, MatrixGField right)
        {
            if (left == ((object)right))
            {
                return true;
            }

            if ((((object)left) == null) || (((object)right) == null))
            {
                return false;
            }

            if ((left.Rows != right.Rows) || (left.Columns != right.Columns))
            {
                return false;
            }

            for (int i = 0; i < left.Rows; i++)
            {
                for (int j = 0; j < left.Columns; j++)
                {
                    if (left[i, j] != right[i, j])
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        /// <summary>Serves as a hash function for a particular type, suitable for use in hashing algorithms and data structures like a hash table.</summary>
        public override int GetHashCode()
        {
            return (Rows + Columns);
        }

        /// <summary>Return <see langword="true"/> if the matrix is a square matrix.</summary>
        public bool Square => (Rows == Columns);

        /// <summary>Returns <see langword="true"/> if the matrix is symmetric.</summary>
        public bool Symmetric
        {
            get
            {
                if (Square)
                {
                    for (int i = 0; i < Rows; i++)
                    {
                        for (int j = 0; j <= i; j++)
                        {
                            if (Data[i][j] != Data[j][i])
                            {
                                return false;
                            }
                        }
                    }

                    return true;
                }

                return false;
            }
        }

        /// <summary>Access the value at the given location.</summary>
        public GField this[int row, int column]
        {
            set => Data[row][column] = value;

            get => Data[row][column];
        }

        /// <summary>Returns a sub matrix extracted from the current matrix.</summary>
        /// <param name="startRow">Start row index</param>
        /// <param name="endRow">End row index</param>
        /// <param name="startColumn">Start column index</param>
        /// <param name="endColumn">End column index</param>
        public MatrixGField Submatrix(int startRow, int endRow, int startColumn, int endColumn)
        {
            if ((startRow > endRow) || (startColumn > endColumn) || (startRow < 0) || (startRow >= Rows) || (endRow < 0) || (endRow >= Rows) || (startColumn < 0) || (startColumn >= Columns) || (endColumn < 0) || (endColumn >= Columns))
            {
                throw new ArgumentException("Argument out of range.");
            }

            MatrixGField X = new MatrixGField(endRow - startRow + 1, endColumn - startColumn + 1);
            var x = X.Data;
            for (int i = startRow; i <= endRow; i++)
            {
                for (int j = startColumn; j <= endColumn; j++)
                {
                    x[i - startRow][j - startColumn] = Data[i][j];
                }
            }

            return X;
        }

        /// <summary>Returns a sub matrix extracted from the current matrix.</summary>
        /// <param name="rowIndexes">Array of row indices</param>
        /// <param name="columnIndexes">Array of column indices</param>
        public MatrixGField Submatrix(int[] rowIndexes, int[] columnIndexes)
        {
            MatrixGField X = new MatrixGField(rowIndexes.Length, columnIndexes.Length);
            var x = X.Data;
            for (int i = 0; i < rowIndexes.Length; i++)
            {
                for (int j = 0; j < columnIndexes.Length; j++)
                {
                    if ((rowIndexes[i] < 0) || (rowIndexes[i] >= Rows) || (columnIndexes[j] < 0) || (columnIndexes[j] >= Columns))
                    {
                        throw new ArgumentException("Argument out of range.");
                    }

                    x[i][j] = Data[rowIndexes[i]][columnIndexes[j]];
                }
            }

            return X;
        }

        /// <summary>Returns a sub matrix extracted from the current matrix.</summary>
        /// <param name="i0">Starttial row index</param>
        /// <param name="i1">End row index</param>
        /// <param name="c">Array of row indices</param>
        public MatrixGField Submatrix(int i0, int i1, int[] c)
        {
            if ((i0 > i1) || (i0 < 0) || (i0 >= Rows) || (i1 < 0) || (i1 >= Rows))
            {
                throw new ArgumentException("Argument out of range.");
            }

            MatrixGField X = new MatrixGField(i1 - i0 + 1, c.Length);
            var x = X.Data;
            for (int i = i0; i <= i1; i++)
            {
                for (int j = 0; j < c.Length; j++)
                {
                    if ((c[j] < 0) || (c[j] >= Columns))
                    {
                        throw new ArgumentException("Argument out of range.");
                    }

                    x[i - i0][j] = Data[i][c[j]];
                }
            }

            return X;
        }

        /// <summary>Returns a sub matrix extracted from the current matrix.</summary>
        /// <param name="r">Array of row indices</param>
        /// <param name="j0">Start column index</param>
        /// <param name="j1">End column index</param>
        public MatrixGField Submatrix(int[] r, int j0, int j1)
        {
            if ((j0 > j1) || (j0 < 0) || (j0 >= Columns) || (j1 < 0) || (j1 >= Columns))
            {
                throw new ArgumentException("Argument out of range.");
            }

            MatrixGField X = new MatrixGField(r.Length, j1 - j0 + 1);
            var x = X.Data;
            for (int i = 0; i < r.Length; i++)
            {
                for (int j = j0; j <= j1; j++)
                {
                    if ((r[i] < 0) || (r[i] >= Rows))
                    {
                        throw new ArgumentException("Argument out of range.");
                    }

                    x[i][j - j0] = Data[r[i]][j];
                }
            }

            return X;
        }

        public MatrixGField AddRowsAtTheEnd(MatrixGField right)
        {
            if (Columns != right.Columns)
            {
                throw new InvalidOperationException("Columns should be equal");
            }

            var newMatrixRows = Data.Concat(right.Data).ToArray();
            return new MatrixGField(newMatrixRows);
        }

        /// <summary>Creates a copy of the matrix.</summary>
        public MatrixGField Clone()
        {
            MatrixGField X = new MatrixGField(Rows, Columns);
            var x = X.Data;
            for (int i = 0; i < Rows; i++)
            {
                for (int j = 0; j < Columns; j++)
                {
                    x[i][j] = Data[i][j];
                }
            }

            return X;
        }

        /// <summary>Returns the transposed matrix.</summary>
        public MatrixGField Transpose()
        {
            MatrixGField X = new MatrixGField(Columns, Rows);
            var x = X.Data;
            for (int i = 0; i < Rows; i++)
            {
                for (int j = 0; j < Columns; j++)
                {
                    x[j][i] = Data[i][j];
                }
            }

            return X;
        }

        public static void ThrowIfLeftOrRightIsNull(MatrixGField left, MatrixGField right)
        {
            if (left == null)
            {
                throw new ArgumentNullException("left");
            }

            if (right == null)
            {
                throw new ArgumentNullException("right");
            }
        }

        /// <summary>Matrix equality.</summary>
        public static bool operator ==(MatrixGField left, MatrixGField right)
        {
            return Equals(left, right);
        }

        /// <summary>Matrix inequality.</summary>
        public static bool operator !=(MatrixGField left, MatrixGField right)
        {
            return !Equals(left, right);
        }

        /// <summary>Matrix addition.</summary>
        public static MatrixGField Add(MatrixGField left, MatrixGField right)
        {
            ThrowIfLeftOrRightIsNull(left, right);

            int rows = left.Rows;
            int columns = left.Columns;
            var data = left.Data;

            if ((rows != right.Rows) || (columns != right.Columns))
            {
                throw new ArgumentException("MatrixGField dimension do not match.");
            }

            MatrixGField X = new MatrixGField(rows, columns);
            var x = X.Data;
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < columns; j++)
                {
                    x[i][j] = data[i][j] + right[i, j];
                }
            }
            return X;
        }

        /// <summary>Matrix addition.</summary>
        public static MatrixGField operator +(MatrixGField left, MatrixGField right)
        {
            ThrowIfLeftOrRightIsNull(left, right);

            return Add(left, right);
        }

        /// <summary>Matrix subtraction.</summary>
        public static MatrixGField Subtract(MatrixGField left, MatrixGField right)
        {
            ThrowIfLeftOrRightIsNull(left, right);

            int rows = left.Rows;
            int columns = left.Columns;
            var data = left.Data;

            if ((rows != right.Rows) || (columns != right.Columns))
            {
                throw new ArgumentException("Matrix dimension do not match.");
            }

            MatrixGField X = new MatrixGField(rows, columns);
            var x = X.Data;
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < columns; j++)
                {
                    x[i][j] = data[i][j] - right[i, j];
                }
            }
            return X;
        }

        /// <summary>Matrix subtraction.</summary>
        public static MatrixGField operator -(MatrixGField left, MatrixGField right)
        {
            ThrowIfLeftOrRightIsNull(left, right);

            return Subtract(left, right);
        }

        /// <summary>Matrix-scalar multiplication.</summary>
        public static MatrixGField Multiply(MatrixGField left, GField right)
        {
            if (left == null)
            {
                throw new ArgumentNullException("left");
            }

            int rows = left.Rows;
            int columns = left.Columns;
            var data = left.Data;

            MatrixGField X = new MatrixGField(rows, columns);

            var x = X.Data;
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < columns; j++)
                {
                    x[i][j] = data[i][j] * right;
                }
            }

            return X;
        }

        /// <summary>Matrix-scalar multiplication.</summary>
        public static MatrixGField operator *(MatrixGField left, GField right)
        {
            if (left == null)
            {
                throw new ArgumentNullException("left");
            }

            return Multiply(left, right);
        }

        /// <summary>Matrix-matrix multiplication.</summary>
        public static MatrixGField Multiply(MatrixGField left, MatrixGField right)
        {
            ThrowIfLeftOrRightIsNull(left, right);

            int rows = left.Rows;
            var data = left.Data;

            if (right.Rows != left.Columns)
            {
                throw new ArgumentException("Matrix dimensions are not valid.");
            }

            int columns = right.Columns;
            var X = new MatrixGField(rows, columns);
            var x = X.Data;

            int size = left.Columns;
            var column = new GField[size];
            for (int j = 0; j < columns; j++)
            {
                for (int k = 0; k < size; k++)
                {
                    column[k] = right[k, j];
                }
                for (int i = 0; i < rows; i++)
                {
                    var row = data[i];
                    var s = row[0] * column[0];
                    for (int k = 1; k < size; k++)
                    {
                        s += row[k] * column[k];
                    }
                    x[i][j] = s;
                }
            }

            return X;
        }

        /// <summary>Matrix-matrix multiplication.</summary>
        public static MatrixGField operator *(MatrixGField left, MatrixGField right)
        {
            if (left == null)
            {
                throw new ArgumentNullException("left");
            }

            if (right == null)
            {
                throw new ArgumentNullException("right");
            }

            return Multiply(left, right);
        }

        public MatrixGField InverseRuben()
        {
            if (Rows != Columns)
            {
                throw new InvalidOperationException("A matrix can only be inversed if rows == columns");
            }

            var identity = MatrixGField.CreateIdentityMatrix(GFTable, Rows);

            var cloned = this.Clone();

            SwapRowsWithZeroDiagonal(cloned, identity, Rows);
            CleanBottomLeft(cloned, identity, Rows);
            CleanTopRight(cloned, identity, Rows);
            NormalizeMatrixTheRubenWay(cloned, identity, Rows);

            return identity;
        }

        private static void SwapRowsWithZeroDiagonal(MatrixGField M, MatrixGField I, int n)
        {
            for (var r = 0; r < n; r++)
            {
                // swap
                if (M[r, r].Value == 0)
                {
                    for (var swaprow = r + 1; swaprow < n; swaprow++)
                    {
                        if (M[swaprow, r].Value != 0)
                        {
                            for (var c = 0; c < n; c++)
                            {
                                var tempM = M[r, c];
                                var tempI = I[r, c];
                                M[r, c] = M[swaprow, c];
                                I[r, c] = I[swaprow, c];
                                M[swaprow, c] = tempM;
                                I[swaprow, c] = tempI;
                            }

                            break;
                        }
                        if (swaprow == n - 1)
                        {
                            throw new Exception("Could not find non-zero diagonal element");
                        }
                    }
                }
            }
        }

        private static void CleanTopRight(MatrixGField M, MatrixGField I, int n)
        {
            for (var r1 = n - 1; r1 >= 0; r1--)
            {
                for (var r2 = r1 - 1; r2 >= 0; r2--)
                {
                    var subtractMultiplier = M[r2, r1] / M[r1, r1];
                    for (var c = 0; c < n; c++)
                    {
                        M[r2, c] -= subtractMultiplier * M[r1, c];
                        I[r2, c] -= subtractMultiplier * I[r1, c];
                    }
                }
            }
        }

        private static void CleanBottomLeft(MatrixGField M, MatrixGField I, int n)
        {
            for (var r1 = 0; r1 < n; r1++)
            {
                for (var r2 = r1 + 1; r2 < n; r2++)
                {
                    var subtractMultiplier = M[r2, r1] / M[r1, r1];
                    for (var c = 0; c < n; c++)
                    {
                        M[r2, c] -= subtractMultiplier * M[r1, c];
                        I[r2, c] -= subtractMultiplier * I[r1, c];
                    }
                }
            }
        }

        private static void NormalizeMatrixTheRubenWay(MatrixGField M, MatrixGField I, int n)
        {
            for (var r = 0; r < n; r++)
            {
                var factor = M[r, r];
                for (var c = 0; c < n; c++)
                {
                    M[r, c] /= factor;
                    I[r, c] /= factor;
                }
            }
        }


        /// <summary>Returns the trace of the matrix.</summary>
        /// <returns>Sum of the diagonal elements.</returns>
        public GField Trace
        {
            get
            {
                var trace = Data[0][0];
                for (int i = 1; i < Math.Min(Rows, Columns); i++)
                {
                    trace += Data[i][i];
                }
                return trace;
            }
        }

        /// <summary>Returns a matrix filled with random values.</summary>
        public static MatrixGField Random(Random random, GFTable table, int rows, int columns)
        {
            int size = (int)table.Size;
            MatrixGField X = new MatrixGField(rows, columns);
            var x = X.Data;
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < columns; j++)
                {
                    x[i][j] = table.CreateField((uint)random.Next(size));
                }
            }
            return X;
        }

        /// <summary>Returns a diagonal matrix of the given size.</summary>
        public static MatrixGField Diagonal(GFTable table, int rows, int columns, GField value)
        {
            MatrixGField X = new MatrixGField(rows, columns);
            var x = X.Data;
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < columns; j++)
                {
                    x[i][j] = ((i == j) ? value : table.CreateField(0));
                }
            }
            return X;
        }

        /// <summary>Returns the matrix in a textual form.</summary>
        public override string ToString()
        {
            var sb = new StringBuilder();

            for (int i = 0; i < Rows; i++)
            {
                for (int j = 0; j < Columns; j++)
                {
                    sb.Append(Data[i][j] + " ");
                }

                sb.AppendLine();
            }

            return sb.ToString();
        }

        public static CoolVectorGField operator *(MatrixGField left, CoolVectorGField right)
        {
            return new CoolVectorGField(left.Multiply(right.Data));
        }

        public GField[] Multiply(GField[] right)
        {
            if (this.Columns != right.Length)
            {
                throw new ArgumentException($"Vector length should be the same as matrix columns. Matrix columns: {this.Columns} Vector length: {right.Length}");
            }

            var newData = new GField[this.Rows];
            for (int i = 0; i < this.Rows; i++)
            {
                var result = Data[i][0] * right[0];
                for (int y = 1; y < this.Columns; y++)
                {
                    result += Data[i][y] * right[y];
                }
                newData[i] = result;
            }

            return newData;
        }
    }
}
