using Fendo.Logic;

namespace Fendo.Engine;

public static class Heuristics
{
    public static float VisionBasedEvaluation(BitBoard7x7 board, Func<int, float> weight_vision, Func<int, float> weight_region)
    {
        ulong vision1 = board.GetVision(Player.One);
        ulong vision2 = board.GetVision(Player.Two);
        ulong region1 = board.GetRegion(Player.One);
        ulong region2 = board.GetRegion(Player.Two);
        ulong region_mask = ~(region1 & region2);

        int count_vision1 = BitUtils.NonZeroCount(vision1 & region_mask);
        int count_vision2 = BitUtils.NonZeroCount(vision2 & region_mask);
        int count_region1 = BitUtils.NonZeroCount(region1);
        int count_region2 = BitUtils.NonZeroCount(region2);

        float region_score = weight_region(count_region1) - weight_region(count_region2);
        if ((region1 & region2) == 0) return region_score > 0 ? 999999 : -999999;

        float score = region_score + weight_vision(count_vision1) - weight_vision(count_vision2);
        return score;
    }

    public static List<Node> TopPercentagePruning(List<(Node node, float score)> scored_nodes, float r)
    {
        scored_nodes.Sort((a, b) => b.score.CompareTo(a.score));
        List<Node> nodes = scored_nodes.Select(sn => sn.node).ToList();

        int count = (int)Math.Ceiling(scored_nodes.Count * r);
        count = Math.Max(count, nodes.Any() ? 1 : 0);

        nodes = nodes.Take(count).ToList();
        return nodes;
    }

    public static List<Node> GeometricPruning(List<(Node, float)> scored_nodes, float r, float a, int depth)
    {
        return TopPercentagePruning(scored_nodes, r * (float)Math.Pow(a, depth));
    }

    public static float BasicEval(BitBoard7x7 board)
    {
        Func<int, float> weight_vision = a => (float)a;
        Func<int, float> weight_region = a => (float)2 * a;
        return VisionBasedEvaluation(board, weight_vision, weight_region);
    }
}

