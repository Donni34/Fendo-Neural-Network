using Fendo.Logic.enums;

namespace Fendo.Logic;

public class Turn
{
    public readonly int row1;
    public readonly int col1;
    public readonly CellState player;

    public Turn(int row1, int col1, CellState player)
    {
        this.row1 = row1;
        this.col1 = col1;
        this.player = player;
    }
}

public class Turn : Turn
{
    public readonly int row0;
    public readonly int col0;
    public readonly Border border;
    public Turn(int row0, int col0, int row1, int col1, CellState player, Border border) 
        : base(row1, col1, player)
    {
        this.row0 = row0;
        this.col0 = col0;
        this.border = border;
    }
}

public class Place : Turn
{
    public Place(int row1, int col1, CellState player) : base(row1, col1, player) { }
}
