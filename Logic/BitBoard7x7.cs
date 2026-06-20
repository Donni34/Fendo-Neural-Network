using System.Numerics;
namespace Fendo.Logic;

public static class BoardMasks
{
    public const ulong ValidArea = 0x007F_7F7F_7F7F_7F7F;
    public const ulong LeftBorder = 0x0001_0101_0101_0101;
    public const ulong RightBorder = 0x0080_8080_8080_8080;
    public const ulong TopBorder = 0x0000_0000_0000_007F;
    public const ulong BottomBorder = 0x7F00_0000_0000_0000;
}

public class BitBoard7x7
{
    public ulong vertical_borders { get; private set; }
    public ulong horizontal_borders { get; private set; }
    public ulong player1 { get; private set; }
    public ulong player2 { get; private set; }

    public ulong AllPieces => player1 | player2;

    public Player active_player { get; private set; }

    private static readonly Border[] AllDirections = [Border.North, Border.South, Border.East, Border.West];

    public BitBoard7x7(ulong p1 = 0, ulong p2 = 0, ulong hBorders = 0, ulong vBorders = 0)
    {
        player1 = p1;
        player2 = p2;
        horizontal_borders = hBorders;
        vertical_borders = vBorders;
    }

    #region Basics
    public static bool HasBit(ulong board, int x, int y)
        => (board & (1UL << (y * 8 + x))) != 0;

    public static bool HasBit(ulong board, int index)
        => (board & (1UL << index)) != 0;

    public static ulong SetBit(ulong board, int x, int y)
        => board | (1UL << (y * 8 + x));

    public static ulong Bit(int x, int y) 
        => 1UL << (y * 8 + x);
    public static ulong Bit(int index)
        => 1UL << index;

    public bool HasWall(int x, int y, Border direction)
    {
        return direction switch
        {
            Border.West => HasBit(vertical_borders, x, y),       
            Border.East => HasBit(vertical_borders, x + 1, y),  
            Border.North => HasBit(horizontal_borders, x, y),     
            Border.South => HasBit(horizontal_borders, x, y + 1), 
            _ => false
        };
    }

    public bool HasWall(int index, Border direction)
    {
        return direction switch
        {
            Border.West => (vertical_borders & (1UL << index)) != 0,
            Border.East => (vertical_borders & (1UL << (index + 1))) != 0,
            Border.North => (horizontal_borders & (1UL << index)) != 0,
            Border.South => (horizontal_borders & (1UL << (index + 8))) != 0,
            _ => false
        };
    }

    public void PlaceWall(int index, Border direction)
    {
        switch (direction)
        {
            case Border.North:
                horizontal_borders |= (1UL << index);
                break;
            case Border.South:
                horizontal_borders |= (1UL << (index + 8));
                break;
            case Border.West:
                vertical_borders |= (1UL << (index + 1));
                break;
            case Border.East:
                vertical_borders |= (1UL << index);
                break;
        }
    }

    public void RemoveWall(int index, Border direction)
    {
        switch (direction)
        {
            case Border.North:
                horizontal_borders |= (1UL << index);
                break;
            case Border.South:
                horizontal_borders |= (1UL << (index + 8));
                break;
            case Border.West:
                vertical_borders |= (1UL << (index + 1));
                break;
            case Border.East:
                vertical_borders |= (1UL << index);
                break;
        }
    }

    public static int Index(int x, int y) => y * 8 + x;

    public ulong PlayerToPieces(Player player) => player switch
    {
        Player.One => player1,
        Player.Two => player2,
        _ => 0,
    };

    public ulong ShiftRight(ulong pieces) => (pieces << 1) & ~vertical_borders;
    public ulong ShiftLeft(ulong pieces) => (pieces & ~vertical_borders) >> 1;
    public ulong ShiftDown(ulong pieces) => (pieces << 8) & ~horizontal_borders;
    public ulong ShiftUp(ulong pieces) => (pieces & ~horizontal_borders) >> 8;

    public static ulong ShiftRight(ulong pieces, ulong borders) => (pieces << 1) & ~borders;
    public static ulong ShiftLeft(ulong pieces, ulong borders) => (pieces & ~borders) >> 1;
    public static ulong ShiftDown(ulong pieces, ulong borders) => (pieces << 8) & ~borders;
    public static ulong ShiftUp(ulong pieces, ulong borders) => (pieces & ~borders) >> 8;
    #endregion 

    #region Vision
    private ulong GetVisionTo(ulong origin, ulong obstructions, ulong borders, Border b)
    {
        ulong vision = 0;
        ulong ray = origin;
        for (int i = 0; i < 6; i++)
        {
            ray = b switch
            {
                Border.North => ShiftUp(ray, borders),
                Border.South => ShiftDown(ray, borders),
                Border.West => ShiftLeft(ray, borders),
                Border.East => ShiftRight(ray, borders),
                _ => 0,
            };
            if (ray == 0) break;
            vision |= ray;
            ray &= ~obstructions;
        }
        return vision;
    }

    public ulong GetVision(ulong origin, ulong obstructions, ulong hborders, ulong vborders, int depth = 2)
    {
        if (depth <= 0) return origin;
        ulong horizontal_vision = GetVisionTo(origin, obstructions, vborders, Border.East) | GetVisionTo(origin, obstructions, vborders, Border.West);
        ulong vertical_vision = GetVisionTo(origin, obstructions, hborders, Border.North) | GetVisionTo(origin, obstructions, hborders, Border.South);
        ulong vision = horizontal_vision | vertical_vision;
        return vision == origin ? vision : GetVision(vision, obstructions, hborders, vborders, depth - 1);
    }
    #endregion

    #region Regions
    public ulong GetRegion(ulong origin, ulong hborders, ulong vborders)
    {
        return GetVision(origin, 0UL, hborders, vborders, 49);
    }

    public ulong GetRegionFrom(int x, int y, ulong obstructions, ulong hborders, ulong vborders)
    {
        return GetRegion(Bit(x, y), hborders, vborders);
    }
    #endregion

    #region Turns
    public bool ValidateTurn(Turn turn) 
    {
        switch (turn.Type)
        {
            case Border.North | Border.East | Border.South | Border.West:
                bool pos0_valid = HasBit(PlayerToPieces(active_player), turn.To);
                bool pos1_valid;
                if (turn.To == turn.From) pos1_valid = true;
                else pos1_valid = HasBit(AllPieces, turn.To);
                bool wall_valid = HasWall(turn.To, turn.Type);
                return pos0_valid && pos1_valid && wall_valid;
            default: return HasBit(AllPieces, turn.To);
        }
    }

    //public bool ValidatePlace(Player player, int row1, int col1, int row0, int col0, Border border)
    //{
    //    bool pos0_valid = HasBit(PlayerToPieces(player), row1, col1);
    //    bool pos1_valid;
    //    if (row1==row0 && col1 == col0) pos1_valid = true;
    //    else pos1_valid = !HasBit(AllPieces, row1, col1);
    //    bool wall_valid = HasWall(row1, col1, border);
    //    return pos0_valid && pos1_valid && wall_valid;
    //}

    //public bool ValidateMove(Player player, int row, int col) 
    //{ 
    //    return HasBit(AllPieces, row, col) && active_player==player; 
    //}

    public List<Turn> GenerateTurns(List<Turn>? turns = null)
    {
        turns ??= new List<Turn>();
        ulong pieces = PlayerToPieces(active_player);
        ulong obstructions = AllPieces;
        ulong vision = GetVision(pieces, obstructions, horizontal_borders, vertical_borders);
        #region Place
        ulong possible_places = vision & ~obstructions;
        while (possible_places != 0)
        {
            int i = BitUtils.TrailingZeroCount(possible_places);
            turns.Add(new Turn(Border.NaB, 0, i));
            possible_places &= possible_places - 1;
        }
        #endregion
        #region Move
        while (pieces != 0)
        {
            int i = BitUtils.TrailingZeroCount(pieces);
            possible_places = GetVision(Bit(i), obstructions, horizontal_borders, vertical_borders);
            while (possible_places != 0)
            {
                int j = BitUtils.TrailingZeroCount(possible_places);
                foreach (Border direction in AllDirections)
                {
                    if (!HasWall(j, direction)) turns.Add(new Turn(direction, j, i));
                }
                possible_places &= possible_places - 1;
            }
            pieces &= pieces - 1;
        }
        #endregion
        return turns;
    }

    public void MakeMove(Turn turn)
    {
        if (turn.Type == Border.NaB)
        {
            switch (active_player)
            {
                case Player.One:
                    player1 |= Bit(turn.To);
                    break;
                case Player.Two:
                    player2 |= Bit(turn.To);
                    break;
            }
        }
        else
        {
            ulong pos0 = Bit(turn.From);
            ulong pos1 = Bit(turn.To);
            switch (active_player)
            {
                case Player.One:
                    player1 ^= (pos0 ^ pos1);
                    break;
                case Player.Two:
                    player2 ^= (pos0 ^ pos1);
                    break;
            }
            PlaceWall(turn.To, turn.Type);
        }
        active_player = active_player.Opponent();
    }

    public void UndoMove(Turn turn)
    {
        active_player = active_player.Opponent();
        if (turn.Type == Border.NaB)
        {
            switch (active_player)
            {
                case Player.One:
                    player1 ^= Bit(turn.To);
                    break;
                case Player.Two:
                    player2 ^= Bit(turn.To);
                    break;
            }
        }
        else
        {
            ulong pos0 = Bit(turn.From);
            ulong pos1 = Bit(turn.To);
            switch (active_player)
            {
                case Player.One:
                    player1 ^= (pos0 ^ pos1);
                    break;
                case Player.Two:
                    player2 ^= (pos0 ^ pos1);
                    break;
            }
            RemoveWall(turn.To, turn.Type);
        }
        active_player = active_player;
    }
    #endregion
}