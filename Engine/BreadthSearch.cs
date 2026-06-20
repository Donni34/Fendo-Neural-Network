using Fendo.Logic;
using System.Diagnostics;
using System.Threading.Tasks.Sources;

namespace Fendo.Engine;

public class BreadthSearch
{
    BitBoard7x7 board;
    Func<BitBoard7x7, float> EvaluationFunction;
    Func<List<(Node, float)>, int, List<Node>> PruningFunction;
    public BreadthSearch(BitBoard7x7 board, Func<BitBoard7x7, float> EvaluationFunction, Func<List<(Node, float)>, int, List<Node>> PruningFunction)
    {
        this.board = board;
        this.EvaluationFunction = EvaluationFunction;
        this.PruningFunction = PruningFunction;
    }

    public (float, Turn) Evaluate(int depth)
    {
        List<Node>[] search_layers = new List<Node>[depth+1];
        Func<BitBoard7x7, float> eval = b => (float)Math.Pow(-1, (byte)board.active_player) * EvaluationFunction(b);
        Node root = new Node(board, null, EvaluationFunction, PruningFunction, depth: 0);
        List<Node> current_layer = new List<Node>() { root };

        for (int i = 0; i < depth; i++)
        {
            int estimatedSize = current_layer.Count * 20;
            Dictionary<BitBoard7x7, Node> uniqueNodes = new(estimatedSize);

            foreach (Node node in current_layer)
            {
                List<Node> new_children = node.MakeChildren();

                for (int j = 0; j < new_children.Count; j++)
                {
                    Node child = new_children[j];

                    if (uniqueNodes.TryGetValue(child.board, out Node existingNode))
                    {
                        node.ReplaceChild(j, existingNode);
                    }
                    else
                    {
                        uniqueNodes.Add(child.board, child);
                    }
                }
            }
            current_layer = new List<Node>(uniqueNodes.Values);
            Console.WriteLine($" -> Layer {i + 1} has {current_layer.Count} Nodes.");
        }
        Console.WriteLine("All layers built. Start computing score:");
        return (root.Score(), root.BestChild().turn.Value);
    }
}