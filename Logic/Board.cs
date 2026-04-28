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

    private bool[,] GetVision(bool[,] vision)
    {
        return ObstructedVision(vision, board, 2);
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
        return GetVision(vision);
    }

    public bool[,] GetVisionFrom(int row, int col)
    {
        CellState player = board[row, col];
        bool[,] vision = new bool[row, col];
        if (player == CellState.Empty) { return vision; }
        return GetVision(vision);
    }

    private bool[,] ObstructedVision(bool[,] vision, CellState[,]? obstruction = null, int depth = 2)
    {
        if (depth == 0) { return vision; }

        obstruction ??= board;
        CellState[,] obstruction_T = (new Matrix<CellState>(obstruction)).Transpose().GetArray(); 

        bool activity_flag = false;

        bool[,] HorizontalVision(bool[,] vision, CellState[,] obstruction, bool[,] borders)
        {
            bool[,] new_vision = new bool[vision.GetLength(0), vision.GetLength(1)];

            int block_start = 0;
            bool block_active = false;
            for (int i = 0; i < size; i++) //Vertikal
            {
                for (int j = 0; j < size - 1; j++) //Horizontal
                {
                    block_active = vision[i, j] || block_active;

                    bool stop_player = block_active && (obstruction[i, j + 1] != CellState.Empty); //fehlerhaft, wenn j=size
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

        bool[,] Transpose(bool[,] matrix)
        {
            Matrix<bool> M = new Matrix<bool>(matrix);
            M.TransposeInPlace();
            return M.GetArray();
        }
            
        bool[,] horizontal_vision = vision;
        bool[,] vertical_vision = vision;

        int k = 0;
        bool changed = true;
        while (k < depth && changed)
        {
            changed = false;
            k++;

            bool[,] new_horizontal_vision = HorizontalVision(vision, obstruction, vertical_borders);
            changed |= activity_flag;
            activity_flag = false;

            bool[,] new_vertical_vision = Transpose(HorizontalVision(Transpose(vision), obstruction_T, Transpose(horizontal_borders)));
            changed |= activity_flag;
            activity_flag = false;

            vertical_vision = new_horizontal_vision;
            horizontal_vision = new_vertical_vision;
        }

        for (int i = 0; i < size; i++) for (int j = 0; j < size; j++)
        {
            vision[i, j] = horizontal_vision[i, j] || vertical_vision[i, j];
        }
        return vision;
    }

    private bool[,] UnobstructedVision(bool[,] vision)
    { 
        bool[,] new_vision = new bool[size, size];
        CellState[,] obstruction = new CellState[size, size];
        return ObstructedVision(vision, obstruction, size^2);
    }

    public bool IsFinished() //prüft, ob Sichtfelder von Spieler 1 und Spieler 2 komplementär sind
    {
        bool[,] vision1 = GetVision(CellState.Player1);
        bool[,] vision2 = GetVision(CellState.Player2);

        bool finished = true;
        for (int i = 0; i < size; i++) for (int j = 0; j<size; j++)
        {
            finished &= vision1[i, j] && !vision2[i, j];
        }
        return finished;
    }
}
