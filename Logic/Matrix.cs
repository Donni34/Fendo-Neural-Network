namespace Fendo.Logic;

public class Matrix<T>
{
    private int n_row;
    private int n_col;

    private bool transposed = false;

    private readonly T[,] matrix;

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

    public T this[int i, int j]
    {
        get
        {
            if (transposed) return matrix[j, i];
            return matrix[i, j];
        }
        set
        {
            if (transposed) matrix[j, i] = value;
            else matrix[i, j] = value;
        }
    }

    public Matrix<T> Copy()
    {
        Matrix<T> M_copy = new Matrix<T>((T[,]) matrix.Clone());
        if (transposed) M_copy.Transpose();
        return M_copy;
    }

    public bool Equals(Matrix <T> comparison_matrix)
    {
        return false;
    }

    public void Transpose() { transposed = !transposed; }
}

