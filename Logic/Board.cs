using System.ComponentModel;

namespace Fendo.Logic;

public class Board
{
    private bool[,] vertical_borders;
    private bool[,] horizontal_borders;
    private CellState[,] board;
    private int size;

    List<(int row, int col)> player1;
    List<(int row, int col)> player2;

    public Board(int size = 7, List<(int row, int col)>? p1 = null, List<(int row, int col)>? p2 = null, bool[,]? v_borders = null, bool[,]? h_borders = null)
    {
        // dimensionen, länge der Listen prüfen

        vertical_borders = v_borders ?? new bool[size, size + 1];
        horizontal_borders = h_borders ?? new bool[size + 1, size]; 
        for (int i = 0; i < size; i++)
        {
            vertical_borders[i, 0] = true;
            vertical_borders[i, size + 1] = true;

            horizontal_borders[i, 0] = true;
            horizontal_borders[i, size + 1] = true;

        }
        board = new CellState[size, size];
        this.size = size;

        p1 ??= [(0, 3)];
        p2 ??= [(6, 3)];

        player1 = p1;
        player2 = p2;

        UpdateBoard();
    }

    public bool ValidateMove(int row1, int col1, CellState player = CellState.Empty, int? row0 = null, int? col0 = null, char? border = null)
    {
        bool is_new = row0 == null || col0 == null;
        if (player == CellState.Empty && is_new) { return false; }
        if (is_new && border != null) { return false; }

        if (is_new)
        {
            bool[,] vision = GetVision(player);
            return vision[row1, col1];
        }
        else
        {
            if (row0 != row1 || col0 != col1) 
            {
                if (board[row1, col1] != CellState.Empty)
                {
                    return false;
                }
                
            }
            bool[,] vision = GetVisionFrom(row0.Value, col0.Value);
            if (!vision[row1, col1]) { return false; }
            switch (border)
            {
                case 'n': return (horizontal_borders[row1 + 1, col1]);
                case 'e': return (vertical_borders[row1, col1 + 1]);
                case 's': return (horizontal_borders[row1, col1]);
                case 'w': return (vertical_borders[row1, col1]);
                default: return false;
            }
        }
        return false;
    }

    public CellState[,] GetBoard()
    {
        return board;
    }
    public void UpdateBoard()
    {
        board = new CellState[size, size];
        foreach (var (row, col) in player1) { board[row, col] = CellState.Player1; }
        foreach (var (row, col) in player2) { board[row, col] = CellState.Player2; }
    }

    

    private bool[,] GetVision(CellState player, bool[,] vision)
    {
        //opponent womöglich redundant, vielleicht auch benötigt für Logik; alles noch nicht getestet
        CellState opponent;
        if (player == CellState.Player1) { opponent = CellState.Player2; }
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
                    block_active = vision[i, j] || block_active;

                    bool stop_player = block_active && (board[i, j + 1] != CellState.Empty); //fehlerhaft, wenn j=size
                    bool stop_other = block_active && (j == size - 1 || borders[i, j + 1]);
                    if (stop_player || stop_other)
                    {
                        for (int k = block_start; k <= j; k++) { new_vision[i, k] = true; }
                        block_active = false;
                        if (stop_other) { block_start = j + 1; }
                        if (stop_player) { block_start = j + 2; }
                    }
                }
            }
            return new_vision;
        }

        bool[,] horizontal_vision = HorizontalVision(vision, vertical_borders);
        bool[,] vertical_vision = Transpose(HorizontalVision(Transpose(vision), Transpose(horizontal_borders)));

        bool[,] vision_hv = Transpose(HorizontalVision(Transpose(horizontal_vision), Transpose(horizontal_borders)));
        bool[,] vision_vh = HorizontalVision(vertical_vision, vertical_borders);

        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                vision[i, j] = vision_hv[i, j] || vision_vh[i, j];
            }
        }
        return vision;
    }

    public bool[,] GetVision(CellState player)
    {
        bool[,] vision = new bool[size, size];
        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                vision[i, j] = board[i, j] == player;
            }
        }
        return GetVision(player, vision);
    }

    public bool[,] GetVisionFrom(int row, int col)
    {
        CellState player = board[row, col];
        bool[,] vision = new bool[row, col];
        if (player == CellState.Empty) { return vision; }
        return GetVision(player, vision);        
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
