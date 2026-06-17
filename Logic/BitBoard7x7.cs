using System.Diagnostics.CodeAnalysis;

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

    public static ulong SetBit(ulong board, int x, int y)
        => board | (1UL << (y * 8 + x));

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
    public ulong GetRegion(ulong origin, ulong obstructions, ulong hborders, ulong vborders)
    {
        return GetVision(origin, obstructions, hborders, vborders, 49);
    }
    #endregion

    #region Turns

    #endregion
}