namespace Fendo.Logic;

public static class Default
{
    public const ulong player1 = 0;
    public const ulong player2 = 0;
    public const ulong player1_sealed = 0;
    public const ulong player2_sealed = 0;

    public const ulong vertical_borders = ;
    public const ulong horizontal_borders = ;
}

public class BitBoard7x7
{
    public ulong vertical_borders { get; private set; }
    public ulong horizontal_borders { get; private set; }
    public ulong player1 { get; private set; }
    public ulong player2 { get; private set; }
    public ulong player1_sealed { get; private set; }
    public ulong player2_sealed { get; private set; }

    public byte[] pieces { get; private set; } = new byte[2];
    public Player active_player { get; private set; } = Player.One;

    private int? _hash = null;

    public BitBoard7x7(ulong? player1 = null, ulong? player2 = null, ulong? player1_sealed = null, ulong? player2_sealed = null, ulong? horizontal_borders = null, ulong? vertical_borders = null)
    {
        this.player1 = player1 is ulong ? (ulong)player1 : Default.player1;
        this.player2 = player2 is ulong ? (ulong)player2 : Default.player2;
        this.player1_sealed = player1_sealed is ulong ? (ulong)player1_sealed : Default.player1_sealed;
        this.player2_sealed = player2_sealed is ulong ? (ulong)player2_sealed : Default.player2_sealed;
        this.horizontal_borders = (horizontal_borders is ulong ? (ulong)horizontal_borders : 0) | Default.horizontal_borders;
        this.vertical_borders = (vertical_borders is ulong ? (ulong)vertical_borders : 0) | Default.vertical_borders;
    }

    public List<ulong> GetTurns()
    {

    }

    public ulong ObstructedVision(ulong vision, ulong obstruction_board, ulong h_borders, ulong v_borders)
    {

    }
}
