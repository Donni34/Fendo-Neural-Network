namespace Fendo.Logic;
public class Board
{
    private Matrix<bool> vertical_borders;
    private Matrix<bool> horizontal_borders;
    private Matrix<CellState> board;
    private int size;

    List<(int row, int col)> player1;
    List<(int row, int col)> player2;

    public Board(int size = 7, List<(int row, int col)>? p1 = null, List<(int row, int col)>? p2 = null, Matrix<bool>? v_borders = null, Matrix<bool>? h_borders = null)
    {
        // dimensionen, länge der Listen prüfen

        vertical_borders = v_borders ?? new Matrix<bool>(size,size+1);
        horizontal_borders = h_borders ?? new Matrix<bool>(size+1,size); 
        for (int i = 0; i < size; i++)
        {
            vertical_borders[i, 0] = true;
            vertical_borders[i, size + 1] = true;

            horizontal_borders[i, 0] = true;
            horizontal_borders[i, size + 1] = true;

        }
        board = new Matrix<CellState>(size, size);
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
            Matrix<bool> vision = GetVision(player);
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
            Matrix<bool> vision = GetVisionFrom(row0.Value, col0.Value);
            if (!vision[row1, col1]) { return false; }

            Matrix<bool> region;
            Matrix<bool> complementary_region;
            switch (border)
            {
                case 'n':
                    if (horizontal_borders[row1 + 1, col1]) return false;
                    else
                    {
                        horizontal_borders[row1 + 1, col1] = true;
                        region = GetRegionFrom(row1, col1, vertical_borders, horizontal_borders);
                        complementary_region = GetRegionFrom(row1 + 1, col1, vertical_borders, horizontal_borders);
                        horizontal_borders[row1 + 1, col1] = false;
                    }
                    break;
                case 'e':
                    if (vertical_borders[row1, col1 + 1]) return false;
                    else
                    {
                        vertical_borders[row1, col1 + 1] = true;
                        region = GetRegionFrom(row1, col1, vertical_borders, horizontal_borders);
                        complementary_region = GetRegionFrom(row1, col1 + 1, vertical_borders, horizontal_borders);
                        vertical_borders[row1, col1 + 1] = false;
                    }
                    break;
                case 's':
                    if (horizontal_borders[row1, col1]) return false;
                    else
                    {
                        horizontal_borders[row1, col1] = true;
                        region = GetRegionFrom(row1, col1, vertical_borders, horizontal_borders);
                        complementary_region = GetRegionFrom(row1 - 1, col1, vertical_borders, horizontal_borders);
                        horizontal_borders[row1, col1] = false;
                    }
                    break;
                case 'w':
                    if (vertical_borders[row1, col1]) return false;
                    else
                    {
                        vertical_borders[row1, col1] = true;
                        region = GetRegionFrom(row1, col1, vertical_borders, horizontal_borders);
                        complementary_region = GetRegionFrom(row1, col1 - 1, vertical_borders, horizontal_borders);
                        vertical_borders[row1, col1] = false;
                    }
                    break;
                default: return false;
            }
            bool same_region = true;
            bool lone_ranger = true;
            for (int i = 0; i < size; i++) for (int j = 0; j < size; j++)
            {
                same_region &= region[i, j] && complementary_region[i, j];
                if (i != row0 && j != col0) lone_ranger &= (board[i, j] == CellState.Empty) && region[i, j];
                if (!same_region || !lone_ranger) return false;
            }
            return true;
        }
    }

    public Matrix<CellState> GetBoard()
    {
        return board;
    }
    
    public void UpdateBoard()
    {
        board = new Matrix<CellState>(size, size);
        foreach (var (row, col) in player1) { board[row, col] = CellState.Player1; }
        foreach (var (row, col) in player2) { board[row, col] = CellState.Player2; }
    }

    private Matrix<bool> GetVision(Matrix<bool> vision)
    {
        return ObstructedVision(vision, board, 2);
    }

    public Matrix<bool> GetVision(CellState player)
    {
        Matrix<bool> vision = new Matrix<bool>(size, size);
        for (int i = 0; i < size; i++) for (int j = 0; j < size; j++)
        {
            vision[i, j] = board[i, j] == player;
        }     
        return GetVision(vision);
    }

    public Matrix<bool> GetVisionFrom(int row, int col)
    {
        CellState player = board[row, col];
        Matrix<bool> vision = new Matrix<bool>(row, col);
        if (player == CellState.Empty) { return vision; }
        return GetVision(vision);
    }

    private Matrix<bool> ObstructedVision(Matrix<bool> vision, Matrix<CellState>? obstruction = null, int depth = 2, Matrix<bool>? v_borders = null, Matrix<bool>? h_borders = null)
    {
        if (depth == 0) { return vision; }

        obstruction ??= board;
        v_borders ??= vertical_borders;
        h_borders ??= horizontal_borders;

        bool activity_flag = false;

        void HorizontalVision(Matrix<bool> vision, Matrix<CellState> obstruction, Matrix<bool> borders)
        {
            int block_start = 0;
            bool block_active = false;
            for (int i = 0; i < size; i++) for (int j = 0; j < size - 1; j++) //Horizontal
            {
                block_active = vision[i, j] || block_active;

                bool stop_player = block_active && (obstruction[i, j + 1] != CellState.Empty); //fehlerhaft, wenn j=size
                bool stop_other = block_active && (j == size - 1 || borders[i, j + 1]);
                if (stop_player || stop_other)
                {
                    for (int k = block_start; k <= j; k++) { vision[i, k] = true; }
                    block_active = false;
                    if (stop_other) { block_start = j + 1; }
                    if (stop_player) { block_start = j + 2; }
                }
            }
        }

        Matrix<bool> horizontal_vision = vision.Copy();
        Matrix<bool> vertical_vision = vision.Copy();

        int k = 0;
        bool changed = true;
        while (k < depth && changed)
        {
            changed = false;
            k++;

            HorizontalVision(vision, obstruction, vertical_borders);
            changed |= activity_flag;
            activity_flag = false;

            vision.Transpose();
            h_borders.Transpose();
            obstruction.Transpose();

            HorizontalVision(vision, obstruction, horizontal_borders);
            changed |= activity_flag;
            activity_flag = false;

            vision.Transpose();
            h_borders.Transpose();
            obstruction.Transpose();

            (vertical_vision, horizontal_vision) = (horizontal_vision, vertical_vision);
        }

        for (int i = 0; i < size; i++) for (int j = 0; j < size; j++)
        {
            vision[i, j] = horizontal_vision[i, j] || vertical_vision[i, j];
        }
        return vision;
    }

    private Matrix<bool> UnobstructedVision(Matrix<bool> vision, Matrix<bool>? v_borders = null, Matrix<bool>? h_borders = null)
    {
        v_borders ??= vertical_borders;
        h_borders ??= horizontal_borders;
        Matrix<CellState> obstruction = new Matrix<CellState>(size, size);
        return ObstructedVision(vision, obstruction, size^2, v_borders, h_borders);
    }

    private Matrix<bool> GetRegionFrom(int row, int col, Matrix<bool>? v_borders = null, Matrix<bool>? h_borders = null)
    {
        v_borders ??= vertical_borders;
        h_borders ??= horizontal_borders;
        Matrix<bool> vision = new Matrix<bool>(size, size);
        vision[row, col] = true;

        Matrix<bool> region = UnobstructedVision(vision, v_borders, h_borders);
        return region;
    }

    public bool IsFinished() //prüft, ob Sichtfelder von Spieler 1 und Spieler 2 komplementär sind
    {
        Matrix<bool> vision1 = GetVision(CellState.Player1);
        Matrix<bool> vision2 = GetVision(CellState.Player2);

        bool finished = true;
        for (int i = 0; i < size; i++) for (int j = 0; j<size; j++)
        {
            finished &= vision1[i, j] && !vision2[i, j];
        }
        return finished;
    }
}
