using System.Data;
using System.Numerics;
namespace Fendo.Logic;

public static class BoardMasks
{
    public const ulong ValidArea = 0x007F_7F7F_7F7F_7F7F;
    public const ulong LeftBorder = 0x0001_0101_0101_0101;
    public const ulong RightBorder = 0x0080_8080_8080_8080;
    public const ulong TopBorder = 0x0000_0000_0000_007F;
    public const ulong BottomBorder = 0x7F00_0000_0000_0000;

    public const ulong InitialPlayer1 = 1UL << 3;
    public const ulong InitialPlayer2 = 1UL << (3 + 6 * 8);
}

public class BitBoard7x7
{
    #region Initialisierungen und Konstruktor
    public ulong vertical_walls { get; private set; }
    public ulong horizontal_walls { get; private set; }
    public ulong player1 { get; private set; }
    public ulong player2 { get; private set; }

    public ulong AllPieces => player1 | player2;

    public Player active_player { get; private set; } = Player.One;

    public ulong Hash { get; private set;  }

    private static readonly Border[] AllDirections = [Border.North, Border.South, Border.East, Border.West];

    public BitBoard7x7(ulong p1, ulong p2, ulong hwalls, ulong vwalls, Player player)
    {
        player1 = p1;
        player2 = p2;
        horizontal_walls = hwalls;
        vertical_walls = vwalls;
        active_player = player;
        Hash = Zobrist.Hash(this);
    }

    public BitBoard7x7()
    {
        player1 = BoardMasks.InitialPlayer1;
        player2 = BoardMasks.InitialPlayer2;
        horizontal_walls = 0;
        vertical_walls = 0;
        Hash = Zobrist.GetInitialHash();
    }

    public override int GetHashCode() => (int)Hash;

    public override bool Equals(object obj) => obj is BitBoard7x7 board ? Equals(board) : false;

    public bool Equals(BitBoard7x7 board)
    {
        if (this.GetHashCode() != board.GetHashCode()) return false;
        return this.player1 == board.player1
            && this.player2 == board.player2
            && this.vertical_walls == board.vertical_walls
            && this.horizontal_walls == board.horizontal_walls
            && this.active_player == board.active_player;
    }

    public BitBoard7x7 Copy() => new BitBoard7x7(player1, player2, horizontal_walls, vertical_walls, active_player);
    #endregion

    #region Basics

    public static int Index(int r, int c) => r * 8 + c;
    public static int WallIndex(int r, int c, Border dir) => dir switch
    {
        Border.West => Index(r, c),
        Border.East => Index(r, c + 1),
        Border.North => Index(r, c),
        Border.South => Index(r + 1, c),
        _ => 0,
    };

    public static bool HasBit(ulong board, int r, int c)
        => (board & (1UL << (r * 8 + c))) != 0;

    public static bool HasBit(ulong board, int index)
        => (board & (1UL << index)) != 0;

    public static ulong SetBit(ulong board, int r, int c)
        => board | (1UL << (r * 8 + c));

    public static ulong Bit(int r, int c) 
        => 1UL << (r * 8 + c);
    public static ulong Bit(int index)
        => 1UL << index;

    public bool HasWall(int r, int c, Border direction) => direction switch
    {
        Border.West or Border.East => HasBit(vertical_walls, WallIndex(r, c, direction)),
        Border.North or Border.South => HasBit(horizontal_walls, WallIndex(r, c, direction)),
        _ => false
    };


    public bool HasWall(int index, Border direction) => direction switch
    {
        Border.West => (vertical_walls & (1UL << index)) != 0,
        Border.East => (vertical_walls & (1UL << (index + 1))) != 0,
        Border.North => (horizontal_walls & (1UL << index)) != 0,
        Border.South => (horizontal_walls & (1UL << (index + 8))) != 0,
        _ => false
    };

    public void PlaceWall(int index, Border direction)
    {
        switch (direction)
        {
            case Border.North:
                horizontal_walls |= (1UL << index);
                Hash ^= Zobrist.HorizontalWalls[index];
                break;
            case Border.South:
                horizontal_walls |= (1UL << (index + 8));
                Hash ^= Zobrist.HorizontalWalls[index + 8];
                break;
            case Border.East:
                vertical_walls |= (1UL << (index + 1));
                Hash ^= Zobrist.VerticalWalls[index + 1];
                break;
            case Border.West:
                vertical_walls |= (1UL << index);
                Hash ^= Zobrist.VerticalWalls[index];
                break;
        }
    }

    public void RemoveWall(int index, Border direction)
    {
        switch (direction)
        {
            case Border.North:
                horizontal_walls &= ~(1UL << index);
                Hash ^= Zobrist.HorizontalWalls[index];
                break;
            case Border.South:
                horizontal_walls &= ~(1UL << (index + 8));
                Hash ^= Zobrist.HorizontalWalls[index + 8];
                break;
            case Border.East:
                vertical_walls &= ~(1UL << (index + 1));
                Hash ^= Zobrist.VerticalWalls[index + 1];
                break;
            case Border.West:
                vertical_walls &= ~(1UL << index);
                Hash ^= Zobrist.VerticalWalls[index];
                break;
        }
    }

    public ulong PlayerToPieces(Player player) => player switch
    {
        Player.One => player1,
        Player.Two => player2,
        _ => 0,
    };

    public ulong ShiftRight(ulong pieces) => ((pieces << 1) & ~vertical_walls) & BoardMasks.ValidArea;
    public ulong ShiftLeft(ulong pieces) => ((pieces & ~vertical_walls) >> 1) & BoardMasks.ValidArea;
    public ulong ShiftDown(ulong pieces) => ((pieces << 8) & ~horizontal_walls) & BoardMasks.ValidArea;
    public ulong ShiftUp(ulong pieces) => ((pieces & ~horizontal_walls) >> 8) & BoardMasks.ValidArea;

    public static ulong ShiftRight(ulong pieces, ulong walls) => ((pieces << 1) & ~walls) & BoardMasks.ValidArea;
    public static ulong ShiftLeft(ulong pieces, ulong walls) => ((pieces & ~walls) >> 1) & BoardMasks.ValidArea;
    public static ulong ShiftDown(ulong pieces, ulong walls) => ((pieces << 8) & ~walls) & BoardMasks.ValidArea;
    public static ulong ShiftUp(ulong pieces, ulong walls) => ((pieces & ~walls) >> 8) & BoardMasks.ValidArea;
    #endregion 

    #region Vision
    private ulong GetVisionTo(ulong origin, ulong obstructions, ulong walls, Border b)
    {
        ulong vision = 0;
        ulong ray = origin;
        for (int i = 0; i < 6; i++)
        {
            ray = b switch
            {
                Border.North => ShiftUp(ray, walls),
                Border.South => ShiftDown(ray, walls),
                Border.West => ShiftLeft(ray, walls),
                Border.East => ShiftRight(ray, walls),
                _ => 0,
            };
            ray &= ~obstructions;
            if (ray == 0) break;
            vision |= ray;
        }
        return vision;
    }

    public ulong GetVision(ulong origin, ulong obstructions, ulong hwalls, ulong vwalls, int depth = 2)
    {
        if (depth <= 0) return origin;
        ulong horizontal_vision = GetVisionTo(origin, obstructions, vwalls, Border.East) | GetVisionTo(origin, obstructions, vwalls, Border.West);
        ulong vertical_vision = GetVisionTo(origin, obstructions, hwalls, Border.North) | GetVisionTo(origin, obstructions, hwalls, Border.South);
        ulong vision = horizontal_vision | vertical_vision | origin;
        return vision == origin ? vision : GetVision(vision, obstructions, hwalls, vwalls, depth - 1);
    }

    public ulong GetVisionFrom(int row, int col)
    {
        return GetVision(Bit(row, col), AllPieces, horizontal_walls, vertical_walls);
    }
    public ulong GetVisionFrom((int, int) pos)
    {
        return GetVisionFrom(pos.Item1, pos.Item2);
    }
    public ulong GetVision(Player player)
    {
        return GetVision(PlayerToPieces(player), AllPieces, horizontal_walls, vertical_walls);
    }
    #endregion

    #region Regions
    public ulong GetRegion(ulong origin, ulong hwalls, ulong vwalls)
    {
        return GetVision(origin, 0UL, hwalls, vwalls, 49);
    }

    public ulong GetRegion(Player p) => GetRegion(PlayerToPieces(p), horizontal_walls, vertical_walls);

    public ulong GetRegionFrom(int x, int y, ulong obstructions, ulong hwalls, ulong vwalls)
    {
        return GetRegion(Bit(x, y), hwalls, vwalls);
    }
    #endregion

    #region Turns
    public bool ValidateTurn(Turn turn) 
    {
        ulong vision = GetVision(Bit(turn.From), AllPieces, horizontal_walls, vertical_walls);
        switch (turn.Type)
        {
            case Border.North or Border.East or Border.South or Border.West:
                bool pos0_valid = HasBit(PlayerToPieces(active_player), turn.From);
                bool pos1_valid;
                if (turn.To == turn.From) pos1_valid = true;
                else pos1_valid = HasBit(~AllPieces & vision, turn.To);
                bool wall_valid = ValidateWallPlacement(turn);
                return pos0_valid && pos1_valid && wall_valid;
            default:
                if (BitUtils.NonZeroCount(PlayerToPieces(active_player)) >= 6) return false;
                return !HasBit(AllPieces, turn.To);
        }
    }

    public bool ValidateWallPlacement(Turn turn)
    {
        // 1. Wenn nur eine Figur gesetzt wird, gibt es keine Wand zu prüfen
        if (turn.Type == Border.NaB) return true;

        ulong temp_h = horizontal_walls;
        ulong temp_v = vertical_walls;
        ulong r1 = 0, r2 = 0; // r1 = Ziel-Region der Figur, r2 = Abgespaltene Region
        int t = turn.To;

        // 2. Wände temporär setzen und Out-Of-Bounds verhindern
        switch (turn.Type)
        {
            case Border.North:
                if (t < 8 || HasBit(temp_h, t)) return false; // Nicht am oberen Rand oder auf anderer Wand
                temp_h |= Bit(t);
                r1 = GetRegion(Bit(t), temp_h, temp_v);
                r2 = GetRegion(Bit(t - 8), temp_h, temp_v);
                break;
            case Border.South:
                if (t >= 48 || HasBit(temp_h, t + 8)) return false; // 48 = Start der untersten Reihe
                temp_h |= Bit(t + 8);
                r1 = GetRegion(Bit(t), temp_h, temp_v);
                r2 = GetRegion(Bit(t + 8), temp_h, temp_v);
                break;
            case Border.West:
                if (t % 8 == 0 || HasBit(temp_v, t)) return false; // Nicht am linken Rand
                temp_v |= Bit(t);
                r1 = GetRegion(Bit(t), temp_h, temp_v);
                r2 = GetRegion(Bit(t - 1), temp_h, temp_v);
                break;
            case Border.East:
                if (t % 8 == 6 || HasBit(temp_v, t + 1)) return false; // Nicht am rechten Rand
                temp_v |= Bit(t + 1);
                r1 = GetRegion(Bit(t), temp_h, temp_v);
                r2 = GetRegion(Bit(t + 1), temp_h, temp_v);
                break;
        }

        // 3. same_region Check: Wenn sich r1 und r2 überschneiden, wurde das Gebiet nicht gespalten
        if ((r1 & r2) != 0) return true;
        if ((r1 & AllPieces) == 0 || (r2 & AllPieces) == 0) return false;

        // 4. Figuren temporär simulieren (Die ziehende Figur befindet sich nun im Geiste auf turn.To)
        ulong myPieces = PlayerToPieces(active_player);
        ulong oppPieces = PlayerToPieces(active_player.Opponent());

        myPieces ^= Bit(turn.From) | Bit(turn.To); // Figur bewegen

        // 5. single_region Check: Befinden sich in der Ankunfts-Region KEINE gegnerischen Figuren?
        bool single_r1 = (r1 & oppPieces) == 0;

        // 6. single_compl Check: Gehört das abgetrennte Gebiet exakt einem Spieler?
        bool compl_has_me = (r2 & myPieces) != 0;
        bool compl_has_opp = (r2 & oppPieces) != 0;

        // Ein XOR (^) prüft, ob exakt EINE der beiden Variablen true ist (also weder 0 Spieler noch 2 Spieler)
        bool single_compl = compl_has_me ^ compl_has_opp;

        return single_r1 || single_compl;
    }

    public List<Turn> GenerateTurns(List<Turn>? turns = null)
    {
        turns ??= new List<Turn>();
        turns.Clear();
        ulong current_pieces = PlayerToPieces(active_player);
        ulong obstructions = AllPieces;
        ulong vision = GetVision(current_pieces, obstructions, horizontal_walls, vertical_walls);
        ulong possible_places;
        #region Place
        if (BitUtils.NonZeroCount(PlayerToPieces(active_player)) < 7)
        {
            possible_places = vision & ~obstructions;
            while (possible_places != 0)
            {
                int i = BitUtils.TrailingZeroCount(possible_places);
                turns.Add(new Turn(Border.NaB, 0, i));
                possible_places &= possible_places - 1;
            }
        }
        
        #endregion
        #region Move
        while (current_pieces != 0)
        {
            int i = BitUtils.TrailingZeroCount(current_pieces);
            possible_places = GetVision(Bit(i), obstructions, horizontal_walls, vertical_walls);
            while (possible_places != 0)
            {
                int j = BitUtils.TrailingZeroCount(possible_places);
                foreach (Border direction in AllDirections)
                {
                    Turn turn = new Turn(direction, i, j);
                    if (ValidateWallPlacement(turn)) turns.Add(turn);
                }
                possible_places &= possible_places - 1;
            }
            current_pieces &= current_pieces - 1;
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
            Hash ^= Zobrist.Pieces[(int)active_player][turn.To];
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
            Hash ^= Zobrist.Pieces[(int)active_player][turn.From];
            Hash ^= Zobrist.Pieces[(int)active_player][turn.To];
        }
        active_player = active_player.Opponent();
        Hash ^= Zobrist.BlackToMove;
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
            Hash ^= Zobrist.Pieces[(int)active_player][turn.To];
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
            Hash ^= Zobrist.Pieces[(int)active_player][turn.From];
            Hash ^= Zobrist.Pieces[(int)active_player][turn.To];
        }
        Hash ^= Zobrist.BlackToMove;
    }

    public bool IsFinished()
    {
        ulong region1 = GetRegion(player1, horizontal_walls, vertical_walls);
        ulong region2 = GetRegion(player2, horizontal_walls, vertical_walls);
        return (region1 & region2) == 0;
    }
    #endregion
}