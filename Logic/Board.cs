namespace Logic;

public class Board
{
    private bool[,] vertical_borders;
    private bool[,] horizontal_borders;
    private CellState[,] board;
    private int size;

    public Board(int size=7)
    {
        vertical_borders = new bool[size, size-1]; // in den vertikalen Rinnen
        horizontal_borders = new bool[size-1, size]; // in den horizontalen Rinnen
        board = new CellState[size, size];
        this.size = size;
    }

    bool[,] GetVision(CellState player)
    {
        CellState opponent;
        if (player==CellState.Player1) { opponent = CellState.Player2; }
        else { opponent = CellState.Player1; }

        bool[,] HorizontalVision(bool[,] vision, bool[,] borders)
        {
            bool[,] new_vision = (bool[,])vision.Clone();

            int block_start = 0;
            bool block_active = false;
            for (int i = 0; i < this.size; i++) //Vertikal
            {
                for (int j = 0; j < this.size - 1; j++) //Horizontal
                {
                    if (vision[i,j]) { block_active = true; }
                    bool stop_opponent = block_active && (board[i, j + 1] == opponent);
                    bool stop_other = block_active && (j == size - 1 || borders[i, j]);
                    if (stop_opponent || stop_other)
                    {
                        for (int k = block_start; k<=j; k++) {new_vision[i, k] = true;}
                        block_active = false;
                        if (stop_other) { block_start = j + 1; }
                        if (stop_opponent) { block_start = j + 2; }
                    }
                }
            }
            return new_vision;
        }


        bool[,] vision = new bool[size, size];
        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                vision[i, j] = board[i, j] == player;
            }
        }


        bool[,] horizontal_vision = HorizontalVision(vision, vertical_borders);
        bool[,] vertical_vision = Transpose(HorizontalVision(Transpose(vision), Transpose(horizontal_borders)));

        bool[,] vision_hv = Transpose(HorizontalVision(Transpose(horizontal_vision), Transpose(horizontal_borders)));
        bool[,] vision_vh = HorizontalVision(vertical_vision, vertical_borders);

        for (int i = 0; i<size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                vision[i, j] = vision_hv[i, j] || vision_vh[i, j];
            }
        }
        return vision;
    }

    public bool[,] Transpose(bool[,] matrix)
    {
        int n_line = matrix.GetLength(0); 
        int n_col = matrix.GetLength(1);

        bool[,] matrix_T = new bool[n_col, n_line];

        for (int i = 0; i < n_line; i++)
        {
            for (int j = 0; j < n_col; j++)
            {
                matrix_T[j, i] = matrix[i, j];
            }
        }
        return matrix_T;
    }
}
