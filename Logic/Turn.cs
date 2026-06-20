using Fendo.Logic;

namespace Fendo.Logic;

//public class Turn
//{
//    public readonly int row1;
//    public readonly int col1;
//    public readonly Player player;
//    public readonly CellState cellstate;

//    public Turn(int row1, int col1, Player player)
//    {
//        this.row1 = row1;
//        this.col1 = col1;
//        this.player = player;
//    }
//}

//public class Move : Turn
//{
//    public readonly int row0;
//    public readonly int col0;
//    public readonly Border border;
//    public Move(int row0, int col0, int row1, int col1, Player player, Border border)
//        : base(row1, col1, player)
//    {
//        this.row0 = row0;
//        this.col0 = col0;
//        this.border = border;
//    }
//}

//public class Place : Turn
//{
//    public Place(int row1, int col1, Player player) : base(row1, col1, player) { }
//}

public readonly struct Turn
{
    public readonly ushort Value;

    public Turn(Border border, int from, int to)
    {
        Value = (ushort)(from | (to << 6) | ((int)border << 12));
    }

    public int From => Value & 0x3F;

    public int To => (Value >> 6) & 0x3F;

    public Border Type => (Border)((Value >> 12) & 0x7);

    public override bool Equals(object? obj) => obj is Turn other && this.Value == other.Value;
    public override int GetHashCode() => Value;
    public static bool operator ==(Turn left, Turn right) => left.Value == right.Value;
    public static bool operator !=(Turn left, Turn right) => left.Value != right.Value;

    //public override string ToString()
    //{
    //    return Type switch
    //    {
    //        TurnType.Move => $"Move: {IndexToCoord(From)} -> {IndexToCoord(To)}",
    //        TurnType.PlaceVerticalWall => $"Wall (V) at {IndexToCoord(To)}",
    //        TurnType.PlaceHorizontalWall => $"Wall (H) at {IndexToCoord(To)}",
    //        _ => "Unknown"
    //    };
    //}

    private static string IndexToCoord(int index) => $"{(char)('A' + (index % 8))}{index / 8 + 1}";
}