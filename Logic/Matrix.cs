namespace Fendo.Logic
{
    internal class Matrix<T>
    {
        int n_row;
        int n_col;

        T[,] matrix;

        public Matrix(int n_row, int n_col)
        {
            matrix = new T[n_row, n_col];
        }

        public Matrix(T[,] m)
        {
            matrix = m;
            n_row = m.GetLength(0);
            n_col = m.GetLength(1);
        }

        public T[,] GetArray() { return matrix; }

        public void SetArray(T[,] matrix)
        {
            if (matrix.GetLength(0) == n_row && matrix.GetLength(1) == n_col) this.matrix = matrix;
            else return;
        }

        public Matrix<T> Transpose()
        {
            T[,] matrix_T = new T[n_col, n_row];
            for (int i = 0;  i < n_row; i++) for (int j = 0; j < n_col; j++)
            {
                matrix_T[j, i] = matrix[i, j];
            }

            return new Matrix<T>(matrix_T);
        }

        public void TransposeInPlace()
        {
            //quadratische Matrix
            if (n_row == n_col) for (int i = 0; i < n_row; i++) for (int j = i + 1; j < n_col; j++)
            {
                (matrix[i, j], matrix[j, i]) = (matrix[j, i], matrix[i, j]);
            }
            //nicht quadratische Matrix
            else
            {
                matrix = Transpose().GetArray();
                (n_row, n_col) = (n_col, n_row);
            }
        }
    }
}
