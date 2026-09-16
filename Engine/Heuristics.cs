using Fendo.Logic;

namespace Fendo.Engine;

public static class Heuristics
{
    #region Evaluation Functions
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
        float region_score = occupied * ((float)exclusive1 - exclusive2);
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
    
    public static float EvaluationWeakTerritory(BitBoard7x7 board, float weight_s_ter, float weight_w_ter, float weight_fov, float weight_act_piece, float weight_lost_piece, float weight_piece_fov)
    {
        ulong v_walls = board.vertical_walls;
        ulong h_walls = board.horizontal_walls;
        ulong player1 = board.PlayerToPieces(Player.One);
        ulong player2 = board.PlayerToPieces(Player.Two);


        #region partition board
        ulong fov1 = board.GetVision(Player.One);
        ulong fov2 = board.GetVision(Player.Two);

        ulong region1 = board.GetRegion(Player.One);
        ulong region2 = board.GetRegion(Player.Two);
        ulong strong_ter1 = region1 & ~region2;
        ulong strong_ter2 = region2 & ~region1;

        ulong region_obstr1 = board.GetRegionObstructed(player1, h_walls, v_walls, player2);
        ulong region_obstr2 = board.GetRegionObstructed(player2, h_walls, v_walls, player1);
        ulong weak_ter1 = region_obstr1 & ~region_obstr2;
        ulong weak_ter2 = region_obstr2 & ~region_obstr1;

        ulong battleground = region1 & region2;
        #endregion


        float score;
        if ((region1 & region2) == 0) score = BitUtils.NonZeroCount(region1)-BitUtils.NonZeroCount(region2) > 0 ? 999999f : -999999f;
        else
        {
            #region piecescore
            ulong active_pieces1 = player1 & battleground;
            ulong active_pieces2 = player2 & battleground;

            static int piecewise_vision_score(ulong pieces, BitBoard7x7 b, ulong hw, ulong vw)
            {
                int s = 0;
                while (pieces != 0)
                {
                    int index = BitUtils.TrailingZeroCount(pieces);
                    ulong single_piece = 1UL << index;
                    ulong single_vision = b.GetVision(single_piece, b.AllPieces, hw, vw);
                    s += BitUtils.NonZeroCount(single_vision);
                    pieces &= pieces - 1;
                }
                return s;
            }

            int score_piecewise_vision1 = piecewise_vision_score(active_pieces1, board, h_walls, v_walls);
            int score_piecewise_vision2 = piecewise_vision_score(active_pieces2, board, h_walls, v_walls);
            int score_active_pieces1 = BitUtils.NonZeroCount(active_pieces1);
            int score_active_pieces2 = BitUtils.NonZeroCount(active_pieces2);
            int score_lost_pieces1 = BitUtils.NonZeroCount(strong_ter1 & player1);
            int score_lost_pieces2 = BitUtils.NonZeroCount(strong_ter2 & player2);

            float score_pieces = weight_piece_fov * ((float)score_piecewise_vision1 - score_piecewise_vision2)
                + weight_act_piece * ((float)score_active_pieces1 - score_active_pieces2)
                + weight_lost_piece * ((float)score_lost_pieces1 - score_lost_pieces2);
            #endregion

            #region territory_score
            float score_territory = weight_s_ter * ((float)BitUtils.NonZeroCount(strong_ter1) - BitUtils.NonZeroCount(strong_ter2))
                + weight_w_ter * ((float)BitUtils.NonZeroCount(weak_ter1) - BitUtils.NonZeroCount(weak_ter2));
            #endregion

            #region fov score
            int score_fov1 = BitUtils.NonZeroCount(fov1 & battleground);
            int score_fov2 = BitUtils.NonZeroCount(fov2 & battleground);
            float score_fov = weight_fov * ((float)score_fov1 - score_fov2);
            #endregion

            score = score_fov + score_territory + score_pieces;
        }
        score = board.active_player == Player.One ? score : -score;
        return score;
    }

    public static float EvaluationToyModel(BitBoard7x7 board)
    {
        //float active_piece = 1;
        //float lost_piece = -3.5f;
        //float visible = 1;
        //float occupied = 2;
        //return VisionPieceBasedEvaluation(board, active_piece, lost_piece, occupied, visible);

        float weight_s_ter = 4;
        float weight_w_ter = 3;
        float weight_fov = 1.4f; // one new fov is worth as much as 7 pieces seeing the same tile
        float weight_act_piece = -3f;
        float weight_lost_piece = -14f; //-3.5 * strong territory
        float weight_piece_fov = 0.2f;

        return EvaluationWeakTerritory(board, weight_s_ter, weight_w_ter, weight_fov, weight_act_piece, weight_lost_piece, weight_piece_fov);
    }
    #endregion

    #region Pruning
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
    #endregion
}

