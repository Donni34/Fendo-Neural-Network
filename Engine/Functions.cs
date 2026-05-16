using Fendo.Logic;

namespace Fendo.Engine;

public static class Heuristics
{
    public static float VisionBasedEvaluation(Board board, float weight_vision, float weight_region)
    {
        Matrix<bool> vision1 = board.GetVision(Player.One);
        Matrix<bool> vision2 = board.GetVision(Player.Two);
        Matrix<bool> region1 = board.GetRegion(Player.One);
        Matrix<bool> region2 = board.GetRegion(Player.Two);

        float count_vision1 = 0;
        float count_vision2 = 0;
        float count_region1 = 0;
        float count_region2 = 0;

        for (int i = 0; i < board.size; i++) for (int j = 0; j < board.size; j++)
        {
            if (region1[i, j]) count_region1++;
            else if (vision1[i, j]) count_vision1++;
            if (region2[i, j]) count_region2++;
            else if (vision2[i, j]) count_vision2++;
        }

        float score = weight_vision*(count_vision1 - count_vision2) + weight_region*(count_region1 - count_region2);
        return score;
    }

    public static List<Node> TopPercentagePruning(List<(Node node, float score)> scored_nodes, float percentage)
    {
        scored_nodes.Sort((a, b) => b.score.CompareTo(a.score));
        List<Node> nodes = scored_nodes.Select(sn => sn.node).ToList();

        int count = (int)Math.Ceiling(scored_nodes.Count * percentage);
        count = Math.Max(count, nodes.Any() ? 1 : 0);

        nodes = nodes.Take(count).ToList();
        return nodes;
    }
}

