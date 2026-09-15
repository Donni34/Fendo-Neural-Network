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

    public static float VisionPieceBasedEvaluation(BitBoard7x7 board, float active_piece, float lost_piece, float occupied, float visible)
    {
        ulong vision1 = board.GetVision(Player.One);
        ulong vision2 = board.GetVision(Player.Two);
        ulong region1 = board.GetRegion(Player.One);
        ulong region2 = board.GetRegion(Player.Two);
        ulong region_mask = ~(region1 & region2);
        ulong battleground = region1 & region2;

        #region regionscore
        int exclusive1 = BitUtils.NonZeroCount(region1 & ~region2);
        int exclusive2 = BitUtils.NonZeroCount(region2 & ~region1);
        float region_score = occupied * ((float)exclusive1-exclusive2);
        #endregion
        float score;
        if ((region1 & region2) == 0) score = region_score > 0 ? 999999f : -999999f;
        else
        {
            #region piecescore
            int active_pieces_player1 = BitUtils.NonZeroCount(battleground & board.player1);
            int active_pieces_player2 = BitUtils.NonZeroCount(battleground & board.player2);

            int lost_pieces_player1 = BitUtils.NonZeroCount(region_mask & board.player1);
            int lost_pieces_player2 = BitUtils.NonZeroCount(region_mask & board.player2);

            float piece_score = active_piece * ((float)active_pieces_player1 - active_pieces_player2) + lost_piece * ((float)lost_pieces_player1 - lost_pieces_player2);
            #endregion

            #region visionscore 
            int pure_vision1 = BitUtils.NonZeroCount(vision1 & battleground);
            int pure_vision2 = BitUtils.NonZeroCount(vision2 & battleground);
            float vision_score = visible * ((float)pure_vision1 - pure_vision2);
            #endregion
            score = piece_score + region_score + vision_score;
        }
        
        score = board.active_player == Player.One ? score : -score;
        return score;
    }

    public static float EvaluationToyModel(BitBoard7x7 board)
    {
        float active_piece = 1;
        float lost_piece = -3.5f;
        float visible = 1;
        float occupied = 2;
        return VisionPieceBasedEvaluation(board, active_piece, lost_piece, occupied, visible);
    }

    public static float BasicEval(BitBoard7x7 board)
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

        float region_score = (2f * count_region1) - (2f * count_region2);
        float score;

        if ((region1 & region2) == 0)
        {
            score = region_score > 0 ? 999999f : -999999f;
        }
        else
        {
            score = region_score + count_vision1 - count_vision2;
        }

        // Negamax-Anpassung: Score aus Sicht des aktiven Spielers zurückgeben
        return board.active_player == Player.One ? score : -score;
    }

    public static List<Node> TopPercentagePruning(List<(Node node, float score)> scored_nodes, float r)
    {
        scored_nodes.Sort((a, b) => b.score > a.score ? 1 : (b.score < a.score ? -1 : 0));

        int count = (int)Math.Ceiling(scored_nodes.Count * r);
        count = Math.Max(count, scored_nodes.Count > 0 ? 1 : 0);
        count = Math.Min(count, scored_nodes.Count);

        List<Node> nodes = new List<Node>(count);
        for (int i = 0; i < count; i++)
        {
            nodes.Add(scored_nodes[i].node);
        }
        return nodes;
    }

    public static List<Node> GeometricPruning(List<(Node, float)> scored_nodes, float r, float a, int depth)
    {
        return TopPercentagePruning(scored_nodes, r * (float)Math.Pow(a, depth));
    }
}

