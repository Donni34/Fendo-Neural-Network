namespace Fendo.Logic;
public class Board
{
    #region Deklarationen
    public Matrix<bool> vertical_borders { get; private set; }
    public Matrix<bool> horizontal_borders { get; private set; }
    public Matrix<CellState> board { get; private set; }
    public readonly int size;
    public byte[] pieces { get; private set; } = new byte[2];
    public Player active_player { get; private set; } = Player.One;

    private int? _hash = null;
    public int Hash { get => _hash ??= board.GetHashCode(); }

    #endregion

    public Board(int size = 7, List<(int row, int col)>? p1 = null, List<(int row, int col)>? p2 = null, Matrix<CellState>? b = null, Matrix<bool>? v_borders = null, Matrix<bool>? h_borders = null)
    {
        // dimensionen, länge der Listen prüfen

        vertical_borders = v_borders ?? new Matrix<bool>(size,size+1);
        horizontal_borders = h_borders ?? new Matrix<bool>(size+1,size); 
        for (int i = 0; i < size; i++)
        {
            vertical_borders[i, 0] = true;
            vertical_borders[i, size] = true;

            horizontal_borders[0, i] = true;
            horizontal_borders[size, i] = true;

        }
        board = b ?? new Matrix<CellState>(size, size);
        this.size = size;

        p1 ??= [(0, 3)];
        p2 ??= [(6, 3)];

        foreach (var p in p1) 
        { 
            board[p] = CellState.Player1; 
            pieces[(int)Player.One]++;
        }
        foreach (var p in p2) 
        { 
            board[p] = CellState.Player2; 
            pieces[(int)Player.Two]++;
        }
    }

    #region Verwaltung
    public Board Copy() { return new Board(size, b: this.board, v_borders: vertical_borders, h_borders: horizontal_borders); }

    public bool EqualTo(Board other_board)
    {
        return Hash == other_board.Hash && board==other_board.board;
    }

    public override bool Equals(object? obj) => obj is Board other && this.EqualTo(other);

    public static bool operator ==(Board? left, Board? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(Board? left, Board? right)
    {
        return !Equals(left, right);
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }

    #endregion

    #region Turns
    public bool ValidateTurn(Turn turn)
    {
        if (turn.player != active_player) return false;
        switch (turn) {
            case Move m:
                return ValidateMove(m);
            case Place p:
                return ValidatePlace(p);
        }
        return false;
    }

    private bool ValidateMove(Move move)
    {
        int row0 = move.row0;
        int row1 = move.row1;
        int col0 = move.col0;
        int col1 = move.col1;
        Border border = move.border;
        Player player = move.player;

        //check elementary conditions (fast)
        if (board[row0, col0] != player.ToCellState()) return false;
        if (board[row1, col1] != CellState.Empty && !(row0 == row1 && col0 == col1)) return false;
        switch (border) {
            case Border.North:
                if (horizontal_borders[row1, col1]) return false;
                break;
            case Border.East:
                if (vertical_borders[row1, col1 + 1]) return false;
                break;
            case Border.South:
                if (horizontal_borders[row1 + 1, col1]) return false;
                break;
            case Border.West:
                if (vertical_borders[row1, col1]) return false;
                break;
            default: return false;
        }

        //check complicated conditions (slow)
        Matrix<bool> vision = GetVisionFrom(row0, col0);
        if (!vision[row1, col1]) { return false; }

        return ValidateBorderPlacement(move);
    }

    public bool ValidateBorderPlacement(Move move) 
    {
        int row0 = move.row0;
        int row1 = move.row1;
        int col0 = move.col0;
        int col1 = move.col1;
        Border border = move.border;
        Player player = move.player;

        Matrix<bool> region;
        Matrix<bool> complementary_region;
        switch (border)
        {
            case Border.North:
                if (horizontal_borders[row1, col1]) return false;
                else
                {
                    horizontal_borders[row1, col1] = true;
                    region = GetRegionFrom(row1, col1, vertical_borders, horizontal_borders);
                    complementary_region = GetRegionFrom(row1 - 1, col1, vertical_borders, horizontal_borders);
                    horizontal_borders[row1, col1] = false;
                }
                break;
            case Border.East:
                if (vertical_borders[row1, col1 + 1]) return false;
                else
                {
                    vertical_borders[row1, col1 + 1] = true;
                    region = GetRegionFrom(row1, col1, vertical_borders, horizontal_borders);
                    complementary_region = GetRegionFrom(row1, col1 + 1, vertical_borders, horizontal_borders);
                    vertical_borders[row1, col1 + 1] = false;
                }
                break;
            case Border.South:
                if (horizontal_borders[row1 + 1, col1]) return false;
                else
                {
                    horizontal_borders[row1 + 1, col1] = true;
                    region = GetRegionFrom(row1, col1, vertical_borders, horizontal_borders);
                    complementary_region = GetRegionFrom(row1 + 1, col1, vertical_borders, horizontal_borders);
                    horizontal_borders[row1 + 1, col1] = false;
                }
                break;
            case Border.West:
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
        //bool same_region = true;
        //byte inhabitants_region = 0;
        //byte inhabitants_compl = 0;
        //bool proper_move = !(row0 == row1 && col0 == col1);
        //for (int i = 0; i < size; i++) for (int j = 0; j < size; j++)
        //{
        //    if (!(proper_move && i == row0 && j == col0) && 
        //        (board[i, j] != CellState.Empty || (i == row1 && j == col1)))
        //    {
        //        if (region[i, j]) inhabitants_region += 1;
        //        if (complementary_region[i ,j]) inhabitants_compl += 1;
        //    }
        //    same_region &= region[i, j] == complementary_region[i, j];
        //    if (!(same_region || inhabitants_region <= 1 || inhabitants_compl <= 1)) return false;
        //}
        //return same_region || inhabitants_region == 1 || inhabitants_compl == 1;

        bool proper_move = !(row0 == row1 && col0 == col1);
        bool same_region = true;
        bool single_region = true;
        Player owner_region = player;
        bool single_compl = true;
        Player? owner_compl = null;
        for (int i = 0; i < size; i++) for (int j = 0; j < size; j++)
        {
            if (!(proper_move && i == row0 && j == col0) &&
                (board[i, j] != CellState.Empty || (i == row1 && j == col1)))
            {
                if (region[i, j] && board[i, j] == player.Opponent().ToCellState()) single_region = false;
                if (complementary_region[i,j])
                {
                    if (owner_compl is Player p) single_compl &= p.ToCellState() == board[i, j];
                    else owner_compl = board[i, j].AsPlayer();
                }
            }
            same_region &= region[i, j] == complementary_region[i, j];
            if (!(same_region || single_region || single_compl)) return false;
        }
        return same_region || (single_region) || (single_compl && owner_compl != null);
    }

    private bool ValidatePlace(Place place)
    {
        //check elementary conditions (fast)
        if (board[place.row1, place.col1] != CellState.Empty) return false;
        if (pieces[(int)place.player] >= size) return false;

        //check complicated conditions (slow)
        Matrix<bool> vision = GetVision(place.player);
        return vision[place.row1, place.col1];
    }
 

    public List<Turn> GetTurns(Player player)
    {
        List<Turn> turns = new List<Turn>();
        turns.AddRange(GetMoves(player));
        turns.AddRange(GetPlaces(player));
        return turns;
    }

    public List<Move> GetMoves(Player player)
    {
        List<Move> moves = new List<Move>();
        CellState player_state = player.ToCellState();
        for (int i = 0; i < size; i++) for (int j = 0; j < size; j++)
        {
            if (board[i, j] == player_state) moves.AddRange(GetMovesFrom(i, j));
        }
        return moves;
    }

    public List<Move> GetMovesFrom(int row, int col)
    {
        List<Move> moves = new List<Move>();
        if (!board[row, col].TryGetPlayer(out Player player)) return moves;

        Matrix<bool> vision = GetVisionFrom(row, col);
        for (int i = 0; i < size; i++) for (int j = 0; j < size; j++)
        {
            if (vision[i,j]) foreach (Border border in Enum.GetValues(typeof(Border)))
            {
                Move move = new Move(row, col, i, j, player, border);
                if (ValidateBorderPlacement(move)) moves.Append(move);
            }
        }
        return moves;
    }

    public List<Place> GetPlaces(Player player)
    {
        List<Place> places = new List<Place>();
        Matrix<bool> vision = GetVision(player);
        for (int i = 0; i < size; i++) for (int j = 0; j < size; j++)
        {
            if (vision[i, j]) places.Append(new Place(i, j, player));
        }
        return places;
    }


    public bool MakeTurn(Turn turn)
    {
        if (ValidateTurn(turn))
        {
            ForceTurn(turn);
            return true;
        }
        return false;
    }

    public void ForceTurn(Turn turn)
    {
        active_player = turn.player.Opponent();
        switch (turn) {
            case Move move:
                int row1 = move.row1;
                int col1 = move.col1;
                board[move.row0, move.col0] = CellState.Empty;
                board[row1, col1] = move.player.ToCellState();
                switch (move.border)
                {
                    case Border.North:
                        horizontal_borders[row1, col1] = true;
                        break;
                    case Border.East:
                        vertical_borders[row1, col1 + 1] = true;
                        break;
                    case Border.South:
                        horizontal_borders[row1 + 1, col1] = true;
                        break;
                    case Border.West:
                        vertical_borders[row1, col1] = true;
                        break;
                }
                break;
            case Place place:
                board[place.row1, place.col1] = place.player.ToCellState();
                pieces[(int)place.player]++;
                break;
        }

        //cleanup
        _hash = null;
    }
    #endregion

    #region Vision
    private Matrix<bool> GetVision(Matrix<bool> vision)
    {
        return ObstructedVision(vision, board, 2);
    }

    public Matrix<bool> GetVision(Player player) { return GetVision(PlayerToVisionBoard(player)); }

    public Matrix<bool> GetVisionFrom(int row, int col)
    {
        Matrix<bool> vision = new Matrix<bool>(size, size);
        if (!board[row, col].TryGetPlayer(out Player player)) return vision;
        vision[row, col] = true;
        return GetVision(vision);
    }

    private Matrix<bool> ObstructedVision(Matrix<bool> vision, Matrix<CellState>? obstruction = null, int depth = 2, Matrix<bool>? v_borders = null, Matrix<bool>? h_borders = null)
    {
        if (depth == 0) { return vision; }

        obstruction ??= board;
        v_borders ??= vertical_borders;
        h_borders ??= horizontal_borders;

        bool activity_flag = false;

        void HorizontalVision(Matrix<bool> vision, Matrix<CellState> obstruction, Matrix<bool> borders) //muss optimiert werden, funktioniert aber glaube
        {
            for (int i = 0; i < size; i++)
            { 
                int? block_start = null;
                bool block_active = false;
                for (int j = 0; j < size; j++)
                {
                    block_active = vision[i, j] || block_active;
                    if (block_start == null)
                    {
                        if (obstruction[i, j] != CellState.Empty) continue;
                        else block_start = j;
                    }
                    bool stop_player = j<size-1 && (obstruction[i, j + 1] != CellState.Empty && !vision[i, j + 1]); 
                    bool stop_other = (j == size - 1 || borders[i, j + 1]);
                    if (stop_player || stop_other)
                    {
                        if (block_active) for (int k = (int)block_start; k <= j; k++) { vision[i, k] = true; }
                        block_active = false;
                        if (stop_other) { block_start = j + 1; }
                        if (stop_player) { block_start = j + 2; }
                        activity_flag = true;
                    }
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

            HorizontalVision(horizontal_vision, obstruction, v_borders);
            changed |= activity_flag;
            activity_flag = false;

            vertical_vision.Transpose();
            h_borders.Transpose();
            obstruction.Transpose();

            HorizontalVision(vertical_vision, obstruction, h_borders);
            changed |= activity_flag;
            activity_flag = false;

            vertical_vision.Transpose();
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
        return ObstructedVision(vision, obstruction, size*size, v_borders, h_borders);
    }


    public Matrix<bool> GetRegion(Player player) { return UnobstructedVision(PlayerToVisionBoard(player)); }

    private Matrix<bool> GetRegionFrom(int row, int col, Matrix<bool>? v_borders = null, Matrix<bool>? h_borders = null)
    {
        v_borders ??= vertical_borders;
        h_borders ??= horizontal_borders;
        Matrix<bool> vision = new Matrix<bool>(size, size);
        vision[row, col] = true;

        Matrix<bool> region = UnobstructedVision(vision, v_borders, h_borders);
        return region;
    }


    private Matrix<bool> PlayerToVisionBoard(Player player)
    {
        Matrix<bool> vision = new Matrix<bool>(size ,size);
        CellState player_state = player.ToCellState();
        for (int i = 0; i < size; i++) for (int j = 0; j < size; j++)
        {
            if (board[i, j] == player_state) vision[i, j] = true;
        }
        return vision;
    }

    public bool IsFinished() //prüft, ob Sichtfelder von Spieler 1 und Spieler 2 komplementär sind
    {
        Matrix<bool> vision1 = PlayerToVisionBoard(Player.One);
        Matrix<bool> vision2 = PlayerToVisionBoard(Player.Two);

        vision1 = UnobstructedVision(vision1, vertical_borders, horizontal_borders);
        vision2 = UnobstructedVision(vision2, vertical_borders, horizontal_borders);

        bool finished = true;
        for (int i = 0; i < size; i++) for (int j = 0; j<size; j++)
        {
            finished &= vision1[i, j] ^ vision2[i, j];
        }
        return finished;
    }
    #endregion
}
