namespace Fendo.Logic;
public enum CellState : byte
{
    Empty,
    Player1,
    Player2,
    Obstruction
}

public enum Player : byte
{
    One = 0,
    Two = 1,
}

public static class PlayerExtensions
{
    public static Player ToPlayer(this  CellState state)
    {
        return state switch
        {
            CellState.Player1 => Player.One,
            CellState.Player2 => Player.Two,
            _ => throw new System.ArgumentException("Zellzustand ist kein Spieler!")
        };
    }

    public static Player? AsPlayer(this CellState state)
    {
        return state switch
        {
            CellState.Player1 => Player.One,
            CellState.Player2 => Player.Two,
            _ => null,
        };
    }
    public static CellState ToCellState(this Player player)
    {
        return player switch
        {
            Player.One => CellState.Player1,
            Player.Two => CellState.Player2,
            _ => CellState.Empty,
        };
    }

    public static CellState? AsCellState(this Player player)
    {
        return player switch
        {
            Player.One => CellState.Player1,
            Player.Two => CellState.Player2,
            _ => null,
        };
    }

    public static Player GetOpponent(this Player player)
    {
        return player == Player.One ? Player.Two : Player.One;
    }

    public static bool IsPlayer(this CellState state)
    {
        return (state == CellState.Player1 || state == CellState.Player2);
    }

    public static bool TryGetPlayer(this CellState state, out Player player)
    {
        if (state == CellState.Player1 || state == CellState.Player2)
        {
            player = state.ToPlayer();
            return true;
        }
        player = default; 
        return false;
    }
}

public enum Border : byte
{
    North,
    South,
    East,
    West,
    NaB,
}

