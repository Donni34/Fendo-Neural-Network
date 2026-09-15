using Fendo.Logic;

namespace Fendo.Engine;

public enum SearchType : byte
{
    Breadth,
    AlphaBeta
}
public class FendoEngine
{
    SearchType type;
    BitBoard7x7 board;
    public FendoEngine(SearchType type, BitBoard7x7 board)
    {
        this.type = type;
        this.board = board;
    }

    public Turn BestTurn(int depth)
    {
        switch (type)
        {
            case SearchType.Breadth:
                float a = 0.5f;
                float q = (float)Math.Sqrt(2);
                Func<List<(Node, float)>, int, List<Node>> pruning = (scored_nodes, depth) => Heuristics.GeometricPruning(scored_nodes, 1f, q, depth);
                BreadthSearch breadth_search = new BreadthSearch(board, Heuristics.BasicEval, pruning);
                return breadth_search.Evaluate(depth).Item2;
                break;
            case SearchType.AlphaBeta:
                AlphaBetaSearch ab_search = new AlphaBetaSearch(Heuristics.EvaluationToyModel);
                return ab_search.Evaluate(board, depth).Item2;
                break;
            default:
                return new Turn();
        }
    }
}
