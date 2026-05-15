namespace Fendo.Logic;

public class Matrix<T>
{
    private readonly int n_row;
    private readonly int n_col;

    private bool transposed = false;

    private readonly T[,] matrix;

    public int rows => transposed ? n_col : n_row;
    public int cols => transposed ? n_row : n_col;

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

    public T this[(int i, int j) pos]
    {
        get { return this[pos.i, pos.j]; }
        set { this[pos.i,pos.j] = value; }
    }

    public Matrix<T> Copy()
    {
        Matrix<T> M_copy = new Matrix<T>((T[,]) matrix.Clone());
        if (transposed) M_copy.Transpose();
        return M_copy;
    }

    public bool Equals(Matrix<T>? comparison_matrix)
    {
        if (comparison_matrix is null) return false;
        if (comparison_matrix is not Matrix<T>) return false;
        if (ReferenceEquals(this, comparison_matrix)) return true;
        if (!(comparison_matrix.rows == n_row && comparison_matrix.cols == n_col)) return false;
        for (int i = 0; i < n_row; i++) for (int j = 0; j < n_col; j++)
        {
            if (!this[i, j].Equals(comparison_matrix[i, j])) return false;
        }
        return true;
    }
    public override bool Equals(object? obj) => Equals(obj as Matrix<T>);

    public static bool operator ==(Matrix<T>? left, Matrix<T>? right)
    {
        // Nutzt die statische Equals-Methode, die sicher mit null umgehen kann
        return Equals(left, right);
    }

    public static bool operator !=(Matrix<T>? left, Matrix<T>? right)
    {
        return !Equals(left, right);
    }


    public override int GetHashCode()
    {
        unchecked 
        {
            int hash = 17; 

            for (int i = 0; i < n_row; i++)
            {
                for (int j = 0; j < n_col; j++)
                {
                    int elementHash = EqualityComparer<T>.Default.GetHashCode(this[i, j]!);
                    hash = hash * 31 + elementHash;
                }
            }
            return hash;
        }
    }

    public void Transpose() { transposed = !transposed; }
}

