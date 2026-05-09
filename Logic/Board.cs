using Fendo.Logic.enums;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
namespace Fendo.Logic;
public class Board
{
    public Matrix<bool> vertical_borders { get; private set; }
    public Matrix<bool> horizontal_borders { get; private set; }
    public Matrix<CellState> board { get; private set; }
    private readonly int size;

    public Board(int size = 7, List<(int row, int col)>? p1 = null, List<(int row, int col)>? p2 = null, Matrix<bool>? v_borders = null, Matrix<bool>? h_borders = null)
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
        board = new Matrix<CellState>(size, size);
        this.size = size;

        p1 ??= [(0, 3)];
        p2 ??= [(6, 3)];

        foreach (var p in p1) { board[p] = CellState.Player1; }
        foreach (var p in p2) { board[p] = CellState.Player2; }
    }


    public bool ValidateTurn(Turn turn)
    {
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
        CellState player = move.player;

        //check elementary conditions (fast)
        if (board[row0, col0] != player) return false;
        if (board[row1, col1] != CellState.Empty) return false;
        switch (border) {
            case Border.North:
                if (horizontal_borders[row1 + 1, col1]) return false;
                break;
            case Border.East:
                if (vertical_borders[row1, col1 + 1]) return false;
                break;
            case Border.South:
                if (horizontal_borders[row1, col1]) return false;
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
        CellState player = move.player;

        Matrix<bool> region;
        Matrix<bool> complementary_region;
        switch (border)
        {
            case Border.North:
                if (horizontal_borders[row1 + 1, col1]) return false;
                else
                {
                    horizontal_borders[row1 + 1, col1] = true;
                    region = GetRegionFrom(row1, col1, vertical_borders, horizontal_borders);
                    complementary_region = GetRegionFrom(row1 + 1, col1, vertical_borders, horizontal_borders);
                    horizontal_borders[row1 + 1, col1] = false;
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
                if (horizontal_borders[row1, col1]) return false;
                else
                {
                    horizontal_borders[row1, col1] = true;
                    region = GetRegionFrom(row1, col1, vertical_borders, horizontal_borders);
                    complementary_region = GetRegionFrom(row1 - 1, col1, vertical_borders, horizontal_borders);
                    horizontal_borders[row1, col1] = false;
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

    private bool ValidatePlace(Place place)
    {
        int row1 = place.row1;
        int col1 = place.col1;
        CellState player = place.player;

        //check elementary conditions (fast)
        if (board[row1, col1] != CellState.Empty) return false;

        //check complicated conditions (slow)
        Matrix<bool> vision = GetVision(player);
        return vision[row1, col1];
    }


    public List<Turn> GetTurns(CellState player)
    {
        List<Turn> turns = new List<Turn>();
        turns.AddRange(GetMoves(player));
        turns.AddRange(GetPlaces(player));
        return turns;
    }

    public List<Turn> GetMoves(CellState player)
    {
        List<Turn> moves = new List<Turn>();
        if (player != CellState.Player1 || player != CellState.Player2) return moves;

        for (int i = 0; i < size; i++) for (int j = 0; j < size; j++)
        {
            if (board[i, j] == player) moves.AddRange(GetMovesFrom(i, j));
        }
        return moves;
    }

    public List<Turn> GetMovesFrom(int row, int col)
    {
        List<Turn> moves = new List<Turn>();
        Matrix<bool> vision = GetVisionFrom(row, col);
        CellState player = board[row, col];
        if (player != CellState.Player1 || player != CellState.Player2) return moves;

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

    public List<Place> GetPlaces(CellState player)
    {
        List<Place> places = new List<Place>();
        if (player != CellState.Player1 || player != CellState.Player2) return places;

        Matrix<bool> vision = GetVision(player);
        for (int i = 0; i < size; i++) for (int j = 0; j < size; j++)
        {
            if (vision[i, j]) places.Append(new Place(i, j, player));
        }
        return places;
    }


    public void MakeTurn(Turn turn)
    {
        if (ValidateTurn(turn)) ForceTurn(turn);
    }

    public void ForceTurn(Turn turn)
    {
        switch (turn) {
            case Move m: 
                ForceMove(m);
                break;
            case Place p:
                ForcePlace(p);
                break;
        }
    }

    private void ForceMove(Move move)
    {
        int row1 = move.row1;
        int col1 = move.col1;
        board[move.row0, move.col0] = CellState.Empty;
        board[row1, col1] = move.player;
        switch (move.border)
        {
            case Border.North:
                horizontal_borders[row1 + 1, col1] = true;
                break;
            case Border.East:
                vertical_borders[row1, col1 + 1] = true;
                break;
            case Border.South:
                horizontal_borders[row1, col1] = true;
                break;
            case Border.West:
                vertical_borders[row1, col1] = true;
                break;
        }
    }

    private void ForcePlace(Place place)
    {
        board[place.row1, place.col1] = place.player;
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
