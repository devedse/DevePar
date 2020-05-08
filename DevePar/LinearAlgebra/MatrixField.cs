using DevePar.Galois;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace DevePar.LinearAlgebra
{
    /// <summary>Matrix provides the fundamental operations of numerical linear algebra.</summary>
    public class MatrixField
    {
        private readonly Field[][] data;
        private readonly int rows;
        private readonly int columns;

        private static readonly Random random = new Random();


        /// <summary>Constructs an empty matrix of the given size.</summary>
        /// <param name="rows">Number of rows.</param>
        /// <param name="columns">Number of columns.</param>
        public MatrixField(int rows, int columns)
        {
            this.rows = rows;
            this.columns = columns;
            data = new Field[rows][];
            for (int i = 0; i < rows; i++)
            {
                data[i] = new Field[columns];
            }
        }

        /// <summary>Constructs a matrix of the given size and assigns a given value to all diagonal elements.</summary>
        /// <param name="rows">Number of rows.</param>
        /// <param name="columns">Number of columns.</param>
        /// <param name="value">Value to assign to the diagnoal elements.</param>
        public MatrixField(int rows, int columns, Field value)
        {
            this.rows = rows;
            this.columns = columns;
            data = new Field[rows][];

            for (int i = 0; i < rows; i++)
            {
                data[i] = new Field[columns];
            }

            for (int i = 0; i < rows; i++)
            {
                data[i][i] = value.Clone();
            }
        }

        /// <summary>Constructs a matrix from the given array.</summary>
        /// <param name="value">The array the matrix gets constructed from.</param>
        [CLSCompliant(false)]
        public MatrixField(Field[][] value)
        {
            rows = value.Length;
            columns = value[0].Length;

            for (int i = 0; i < rows; i++)
            {
                if (value[i].Length != columns)
                {
                    throw new ArgumentException("Argument out of range.");
                }
            }

            data = value;
        }

        /// <summary>Constructs a matrix from the given array.</summary>
        /// <param name="value">The array the matrix gets constructed from.</param>
        [CLSCompliant(false)]
        public MatrixField(Field[,] value) : this(value.GetLength(0), value.GetLength(1))
        {
            for (int i = 0; i < rows; i++)
            {
                for (int y = 0; y < columns; y++)
                {
                    data[i][y] = value[i, y];
                }
            }
        }

        /// <summary>Constructs a matrix from the given array.</summary>
        /// <param name="value">The array the matrix gets constructed from.</param>
        [CLSCompliant(false)]
        public MatrixField(int[,] value) : this(value.GetLength(0), value.GetLength(1))
        {
            for (int i = 0; i < rows; i++)
            {
                for (int y = 0; y < columns; y++)
                {
                    data[i][y] = new Field((byte)value[i, y]);
                }
            }
        }

        public static MatrixField CreateIdentityMatrix(int size)
        {
            int[,] values = new int[size, size];
            for (int i = 0; i < size; i++)
            {
                values[i, i] = 1;
            }

            return new MatrixField(values);
        }

        /// <summary>Determines weather two instances are equal.</summary>
        public override bool Equals(object obj)
        {
            return Equals(this, (Matrix)obj);
        }

        /// <summary>Determines weather two instances are equal.</summary>
        public static bool Equals(Matrix left, Matrix right)
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

        internal Field[][] Array => data;

        /// <summary>Returns the number of columns.</summary>
        public int Rows => rows;

        /// <summary>Returns the number of columns.</summary>
        public int Columns => columns;

        /// <summary>Return <see langword="true"/> if the matrix is a square matrix.</summary>
        public bool Square => (rows == columns);

        /// <summary>Returns <see langword="true"/> if the matrix is symmetric.</summary>
        public bool Symmetric
        {
            get
            {
                if (Square)
                {
                    for (int i = 0; i < rows; i++)
                    {
                        for (int j = 0; j <= i; j++)
                        {
                            if (data[i][j] != data[j][i])
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
        public Field this[int row, int column]
        {
            set => data[row][column] = value;

            get => data[row][column];
        }

        /// <summary>Returns a sub matrix extracted from the current matrix.</summary>
        /// <param name="startRow">Start row index</param>
        /// <param name="endRow">End row index</param>
        /// <param name="startColumn">Start column index</param>
        /// <param name="endColumn">End column index</param>
        public MatrixField Submatrix(int startRow, int endRow, int startColumn, int endColumn)
        {
            if ((startRow > endRow) || (startColumn > endColumn) || (startRow < 0) || (startRow >= rows) || (endRow < 0) || (endRow >= rows) || (startColumn < 0) || (startColumn >= columns) || (endColumn < 0) || (endColumn >= columns))
            {
                throw new ArgumentException("Argument out of range.");
            }

            MatrixField X = new MatrixField(endRow - startRow + 1, endColumn - startColumn + 1);
            Field[][] x = X.Array;
            for (int i = startRow; i <= endRow; i++)
            {
                for (int j = startColumn; j <= endColumn; j++)
                {
                    x[i - startRow][j - startColumn] = data[i][j];
                }
            }

            return X;
        }

        /// <summary>Returns a sub matrix extracted from the current matrix.</summary>
        /// <param name="rowIndexes">Array of row indices</param>
        /// <param name="columnIndexes">Array of column indices</param>
        public MatrixField Submatrix(int[] rowIndexes, int[] columnIndexes)
        {
            MatrixField X = new MatrixField(rowIndexes.Length, columnIndexes.Length);
            Field[][] x = X.Array;
            for (int i = 0; i < rowIndexes.Length; i++)
            {
                for (int j = 0; j < columnIndexes.Length; j++)
                {
                    if ((rowIndexes[i] < 0) || (rowIndexes[i] >= rows) || (columnIndexes[j] < 0) || (columnIndexes[j] >= columns))
                    {
                        throw new ArgumentException("Argument out of range.");
                    }

                    x[i][j] = data[rowIndexes[i]][columnIndexes[j]];
                }
            }

            return X;
        }

        /// <summary>Returns a sub matrix extracted from the current matrix.</summary>
        /// <param name="i0">Starttial row index</param>
        /// <param name="i1">End row index</param>
        /// <param name="c">Array of row indices</param>
        public MatrixField Submatrix(int i0, int i1, int[] c)
        {
            if ((i0 > i1) || (i0 < 0) || (i0 >= rows) || (i1 < 0) || (i1 >= rows))
            {
                throw new ArgumentException("Argument out of range.");
            }

            MatrixField X = new MatrixField(i1 - i0 + 1, c.Length);
            Field[][] x = X.Array;
            for (int i = i0; i <= i1; i++)
            {
                for (int j = 0; j < c.Length; j++)
                {
                    if ((c[j] < 0) || (c[j] >= columns))
                    {
                        throw new ArgumentException("Argument out of range.");
                    }

                    x[i - i0][j] = data[i][c[j]];
                }
            }

            return X;
        }

        /// <summary>Returns a sub matrix extracted from the current matrix.</summary>
        /// <param name="r">Array of row indices</param>
        /// <param name="j0">Start column index</param>
        /// <param name="j1">End column index</param>
        public MatrixField Submatrix(int[] r, int j0, int j1)
        {
            if ((j0 > j1) || (j0 < 0) || (j0 >= columns) || (j1 < 0) || (j1 >= columns))
            {
                throw new ArgumentException("Argument out of range.");
            }

            MatrixField X = new MatrixField(r.Length, j1 - j0 + 1);
            Field[][] x = X.Array;
            for (int i = 0; i < r.Length; i++)
            {
                for (int j = j0; j <= j1; j++)
                {
                    if ((r[i] < 0) || (r[i] >= rows))
                    {
                        throw new ArgumentException("Argument out of range.");
                    }

                    x[i][j - j0] = data[r[i]][j];
                }
            }

            return X;
        }

        /// <summary>Creates a copy of the matrix.</summary>
        public MatrixField Clone()
        {
            MatrixField X = new MatrixField(rows, columns);
            Field[][] x = X.Array;
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < columns; j++)
                {
                    x[i][j] = data[i][j];
                }
            }

            return X;
        }

        /// <summary>Returns the transposed matrix.</summary>
        public MatrixField Transpose()
        {
            MatrixField X = new MatrixField(columns, rows);
            Field[][] x = X.Array;
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < columns; j++)
                {
                    x[j][i] = data[i][j];
                }
            }

            return X;
        }

        ///// <summary>Returns the One Norm for the matrix.</summary>
        ///// <value>The maximum column sum.</value>
        //public int Norm1
        //{
        //    get
        //    {
        //        int f = 0;
        //        for (int j = 0; j < columns; j++)
        //        {
        //            Field s = new Field(0);
        //            for (int i = 0; i < rows; i++)
        //            {
        //                s += data[i][j];
        //            }

        //            f = Math.Max(f, s);
        //        }
        //        return f;
        //    }
        //}

        ///// <summary>Returns the Infinity Norm for the matrix.</summary>
        ///// <value>The maximum row sum.</value>
        //public int InfinityNorm
        //{
        //    get
        //    {
        //        int f = 0;
        //        for (int i = 0; i < rows; i++)
        //        {
        //            int s = 0;
        //            for (int j = 0; j < columns; j++)
        //            {
        //                s += Math.Abs(data[i][j]);
        //            }

        //            f = Math.Max(f, s);
        //        }
        //        return f;
        //    }
        //}

        ///// <summary>Returns the Frobenius Norm for the matrix.</summary>
        ///// <value>The square root of sum of squares of all elements.</value>
        //public double FrobeniusNorm
        //{
        //    get
        //    {
        //        double f = 0;
        //        for (int i = 0; i < rows; i++)
        //        {
        //            for (int j = 0; j < columns; j++)
        //            {
        //                f = Hypotenuse(f, data[i][j]);
        //            }
        //        }

        //        return f;
        //    }
        //}

        ///// <summary>Unary minus.</summary>
        //public static MatrixField Negate(MatrixField value)
        //{
        //    if (value == null)
        //    {
        //        throw new ArgumentNullException("value");
        //    }

        //    int rows = value.Rows;
        //    int columns = value.Columns;
        //    Field[][] data = value.Array;

        //    MatrixField X = new MatrixField(rows, columns);
        //    Field[][] x = X.Array;
        //    for (int i = 0; i < rows; i++)
        //    {
        //        for (int j = 0; j < columns; j++)
        //        {
        //            x[i][j] = -data[i][j];
        //        }
        //    }

        //    return X;
        //}

        ///// <summary>Unary minus.</summary>
        //public static MatrixField operator -(MatrixField value)
        //{
        //    if (value == null)
        //    {
        //        throw new ArgumentNullException("value");
        //    }

        //    return Negate(value);
        //}

        /// <summary>Matrix equality.</summary>
        public static bool operator ==(MatrixField left, MatrixField right)
        {
            return Equals(left, right);
        }

        /// <summary>Matrix inequality.</summary>
        public static bool operator !=(MatrixField left, MatrixField right)
        {
            return !Equals(left, right);
        }

        /// <summary>Matrix addition.</summary>
        public static MatrixField Add(MatrixField left, MatrixField right)
        {
            if (left == null)
            {
                throw new ArgumentNullException("left");
            }

            if (right == null)
            {
                throw new ArgumentNullException("right");
            }

            int rows = left.Rows;
            int columns = left.Columns;
            Field[][] data = left.Array;

            if ((rows != right.Rows) || (columns != right.Columns))
            {
                throw new ArgumentException("MatrixField dimension do not match.");
            }

            MatrixField X = new MatrixField(rows, columns);
            Field[][] x = X.Array;
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
        public static MatrixField operator +(MatrixField left, MatrixField right)
        {
            if (left == null)
            {
                throw new ArgumentNullException("left");
            }

            if (right == null)
            {
                throw new ArgumentNullException("right");
            }

            return Add(left, right);
        }

        /// <summary>Matrix subtraction.</summary>
        public static MatrixField Subtract(MatrixField left, MatrixField right)
        {
            if (left == null)
            {
                throw new ArgumentNullException("left");
            }

            if (right == null)
            {
                throw new ArgumentNullException("right");
            }

            int rows = left.Rows;
            int columns = left.Columns;
            Field[][] data = left.Array;

            if ((rows != right.Rows) || (columns != right.Columns))
            {
                throw new ArgumentException("Matrix dimension do not match.");
            }

            MatrixField X = new MatrixField(rows, columns);
            Field[][] x = X.Array;
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
        public static MatrixField operator -(MatrixField left, MatrixField right)
        {
            if (left == null)
            {
                throw new ArgumentNullException("left");
            }

            if (right == null)
            {
                throw new ArgumentNullException("right");
            }

            return Subtract(left, right);
        }

        /// <summary>Matrix-scalar multiplication.</summary>
        public static MatrixField Multiply(MatrixField left, Field right)
        {
            if (left == null)
            {
                throw new ArgumentNullException("left");
            }

            int rows = left.Rows;
            int columns = left.Columns;
            Field[][] data = left.Array;

            MatrixField X = new MatrixField(rows, columns);

            Field[][] x = X.Array;
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
        public static MatrixField operator *(MatrixField left, Field right)
        {
            if (left == null)
            {
                throw new ArgumentNullException("left");
            }

            return Multiply(left, right);
        }

        /// <summary>Matrix-matrix multiplication.</summary>
        public static MatrixField Multiply(MatrixField left, MatrixField right)
        {
            if (left == null)
            {
                throw new ArgumentNullException("left");
            }

            if (right == null)
            {
                throw new ArgumentNullException("right");
            }

            int rows = left.Rows;
            Field[][] data = left.Array;

            if (right.Rows != left.columns)
            {
                throw new ArgumentException("Matrix dimensions are not valid.");
            }

            int columns = right.Columns;
            MatrixField X = new MatrixField(rows, columns);
            Field[][] x = X.Array;

            int size = left.columns;
            Field[] column = new Field[size];
            for (int j = 0; j < columns; j++)
            {
                for (int k = 0; k < size; k++)
                {
                    column[k] = right[k, j];
                }
                for (int i = 0; i < rows; i++)
                {
                    Field[] row = data[i];
                    Field s = new Field(0);
                    for (int k = 0; k < size; k++)
                    {
                        s += row[k] * column[k];
                    }
                    x[i][j] = s;
                }
            }

            return X;
        }

        /// <summary>Matrix-matrix multiplication.</summary>
        public static MatrixField operator *(MatrixField left, MatrixField right)
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

        /// <summary>Returns the LHS solution vetor if the matrix is square or the least squares solution otherwise.</summary>
        public MatrixField Solve(MatrixField rightHandSide)
        {
            return (rows == columns) ? new LuDecompositionField(this).Solve(rightHandSide) : new QrDecompositionField(this).Solve(rightHandSide);
        }

        /// <summary>Inverse of the matrix if matrix is square, pseudoinverse otherwise.</summary>
        public MatrixField Inverse => Solve(Diagonal(rows, rows, new Field(1)));

        //public MatrixField InverseGausianJordan()
        //{
        //    if (rows != columns)
        //    {
        //        throw new InvalidOperationException("A matrix can only be inversed if rows == columns");
        //    }

        //    var inverse = CreateIdentityMatrix(rows);

        //    var rowsWithOneNonZeroElement = new List<Field[]>();
        //    var rowsWithoutZeroElement = new List<Field[]>();

        //    for (int i = 0; i < rows; i++)
        //    {
        //        var nonZeroElementCount = this.Array[i].Count(t => t.Value != 0);

        //        if (nonZeroElementCount == 1)
        //        {
        //            rowsWithOneNonZeroElement.Add(this.Array[i]);
        //        }
        //        else if (nonZeroElementCount == 0)
        //        {
        //            rowsWithoutZeroElement.Add(this.Array[i]);
        //        }
        //        else
        //        {
        //            throw new InvalidOperationException("Can't deal with this shit");
        //        }
        //    }

        //    foreach (var nonZeroRow in rowsWithoutZeroElement)
        //    {
        //        foreach (var unitRow in rowsWithOneNonZeroElement)
        //        {
        //            var index = unitRow.FindIndex(t => t.Value != 0);
        //            while (nonZeroRow[index] != 0) {
        //                nonZeroRow -= unitRow;
        //                inverse[indexNonZeroRow]
        //            }
        //        }
        //    }

        //    return inverse;
        //}

        /// <summary>Returns the trace of the matrix.</summary>
        /// <returns>Sum of the diagonal elements.</returns>
        public Field Trace
        {
            get
            {
                Field trace = new Field(0);
                for (int i = 0; i < Math.Min(rows, columns); i++)
                {
                    trace += data[i][i];
                }
                return trace;
            }
        }

        /// <summary>Returns a matrix filled with random values.</summary>
        public static MatrixField Random(int rows, int columns)
        {
            MatrixField X = new MatrixField(rows, columns);
            Field[][] x = X.Array;
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < columns; j++)
                {
                    x[i][j] = new Field((byte)random.Next(256));
                }
            }
            return X;
        }

        /// <summary>Returns a diagonal matrix of the given size.</summary>
        public static MatrixField Diagonal(int rows, int columns, Field value)
        {
            MatrixField X = new MatrixField(rows, columns);
            Field[][] x = X.Array;
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < columns; j++)
                {
                    x[i][j] = ((i == j) ? value.Clone() : new Field(0));
                }
            }
            return X;
        }

        /// <summary>Returns the matrix in a textual form.</summary>
        public override string ToString()
        {
            using (StringWriter writer = new StringWriter(CultureInfo.InvariantCulture))
            {
                for (int i = 0; i < rows; i++)
                {
                    for (int j = 0; j < columns; j++)
                    {
                        writer.Write(data[i][j] + " ");
                    }

                    writer.WriteLine();
                }

                return writer.ToString();
            }
        }

        private static double Hypotenuse(double a, double b)
        {
            if (Math.Abs(a) > Math.Abs(b))
            {
                double r = b / a;
                return Math.Abs(a) * Math.Sqrt(1 + r * r);
            }

            if (b != 0)
            {
                double r = a / b;
                return Math.Abs(b) * Math.Sqrt(1 + r * r);
            }

            return 0.0;
        }

        public static CoolVectorField operator *(MatrixField left, CoolVectorField right)
        {
            return left.Multiply(right);
        }

        public CoolVectorField Multiply(CoolVectorField vector)
        {
            if (this.columns != vector.Length)
            {
                throw new ArgumentException($"Vector length should be the same as matrix columns. Matrix columns: {this.columns} Vector length: {vector.Length}");
            }

            var newData = new Field[this.rows];
            for (int i = 0; i < this.rows; i++)
            {
                var result = new Field(0);
                for (int y = 0; y < this.columns; y++)
                {
                    result += data[i][y] * vector.Data[y];
                }
                newData[i] = result;
            }

            return new CoolVectorField(newData);
        }
    }
}
