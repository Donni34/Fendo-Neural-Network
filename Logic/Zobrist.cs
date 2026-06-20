namespace Fendo.Logic;

public static class Zobrist
{
    // [Spieler (0 oder 1), Feld (0 bis 63)]
    public static readonly ulong[][] Pieces = [new ulong [64], new ulong [64]];

    // Wände [Feld (0 bis 63)]
    public static readonly ulong[] HorizontalWalls = new ulong[64];
    public static readonly ulong[] VerticalWalls = new ulong[64];

    // Wer ist am Zug?
    public static readonly ulong BlackToMove;

    static Zobrist()
    {
        Random rnd = new Random(12345);
        byte[] buffer = new byte[8]; 

        for (int p = 0; p < 2; p++)
        {
            for (int s = 0; s < 64; s++)
            {
                rnd.NextBytes(buffer);
                Pieces[p][s] = BitConverter.ToUInt64(buffer, 0);
            }
        }
        for (int i = 0; i < 64; i++)
        {
            rnd.NextBytes(buffer);
            HorizontalWalls[i] = BitConverter.ToUInt64(buffer, 0);
            rnd.NextBytes(buffer);
            VerticalWalls[i] = BitConverter.ToUInt64(buffer, 0);
        }
        rnd.NextBytes(buffer);
        BlackToMove = BitConverter.ToUInt64(buffer, 0);
    }

    public static ulong GetInitialHash()
    {
        ulong hash = 0;
        hash ^= Zobrist.Pieces[(int)Player.One][3];
        hash ^= Zobrist.Pieces[(int)Player.Two][51];
        return hash;
    }

    public static ulong Hash(BitBoard7x7 board)
    {
        static ulong Hash(ulong bits, ulong[] table)
        {
            ulong bit_hash = 0;
            while (bits != 0)
            {
                int sq = BitUtils.TrailingZeroCount(bits);
                bit_hash ^= table[sq];
                bits &= bits - 1;
            }
            return bit_hash;
        }

        ulong hash = 0;
        hash ^= Hash(board.player1, Zobrist.Pieces[(int)Player.One]);
        hash ^= Hash(board.player2, Zobrist.Pieces[(int)Player.Two]);
        hash ^= Hash(board.horizontal_walls, Zobrist.HorizontalWalls);
        hash ^= Hash(board.vertical_walls, Zobrist.VerticalWalls);
        if (board.active_player == Player.Two) hash ^= Zobrist.BlackToMove;

        return hash;
    }
}
