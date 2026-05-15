using Fendo.Logic;

namespace Fendo.Engine;

public class BreadthSearch
{
    Board board;
    Func<Board, float> EvaluationFunction;
    Func<List<(Turn, float)>, List<Turn>> PruningFunction;
    BreadthSearch(Board board, Func<Board, float> EvaluationFunction, Func<List<(Turn, float)>, List<Turn>> PruningFunction)
    {
        this.board = board;
        this.EvaluationFunction = EvaluationFunction;
        this.PruningFunction = PruningFunction;
    }

    public Turn BestTurn(int depth)
    {
        List<Node>[] search_layers = new List<Node>[depth+1];
        Node root = new Node(board, null, EvaluationFunction, PruningFunction);
        search_layers[0] = new List<Node> { root };

        for (int i = 0; i < depth; i++)
        {
            List<Node> new_layer = new List<Node>();
            foreach(Node node in new_layer)
            {
                List<Node> new_children = node.MakeChildren();
                for (int j = 0; j < new_children.Count; j++) foreach(Node child in new_layer)
                {
                    if (new_children[j].Equals(child)) node.ReplaceChild(j, child);
                    else new_layer.Add(new_children[j]);
                }
            }
            search_layers[i + 1] = new_layer;
        }
        return root.BestChild().BestChild().turn!;
    }
}