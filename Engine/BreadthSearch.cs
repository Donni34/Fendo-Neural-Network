using Fendo.Logic;
using System.Diagnostics;
using System.Threading.Tasks.Sources;

namespace Fendo.Engine;

public class BreadthSearch
{
    Board board;
    Func<Board, float> EvaluationFunction;
    Func<List<(Node, float)>, List<Node>> PruningFunction;
    public BreadthSearch(Board board, Func<Board, float> EvaluationFunction, Func<List<(Node, float)>, List<Node>> PruningFunction)
    {
        this.board = board;
        this.EvaluationFunction = EvaluationFunction;
        this.PruningFunction = PruningFunction;
    }

    public (float, Turn) Evaluate(int depth)
    {
        List<Node>[] search_layers = new List<Node>[depth+1];
        Func<Board, float> eval = b => (float)Math.Pow(-1, (byte)board.active_player) * EvaluationFunction(b);
        Node root = new Node(board, null, EvaluationFunction, PruningFunction, depth: 0);
        search_layers[0] = new List<Node> { root };

        for (int i = 0; i < depth; i++)
        {
            Console.WriteLine($"Building layer {i + 1}");
            List<Node> new_layer = new();
            foreach(Node node in search_layers[i])
            {
                List<Node> new_children = node.MakeChildren();
                if (new_layer.Count == 0)
                {
                    new_layer = new_children;
                    continue;
                }
                bool copy;
                for (int j = 0; j < new_children.Count; j++)
                {
                    copy = false;
                    foreach (Node child in new_layer) if (new_children[j].Equals(child))
                    {
                        node.ReplaceChild(j, child);
                        copy = true;
                        break;
                    }
                    if (!copy) new_layer.Add(new_children[j]);
                }
            }
            search_layers[i + 1] = new_layer;
            Console.WriteLine($" -> Layer {i + 1} has {new_layer.Count} Nodes.");
        }
        Console.WriteLine("All layers built. Start computing score:");
        return (root.Score(), root.BestChild().turn!);
    }
}