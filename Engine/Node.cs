using Fendo.Logic;

namespace Fendo.Engine;

public class Node
{
    public BitBoard7x7 board { get; private set; }
    public readonly Turn? turn;

    Func<BitBoard7x7, float> EvaluationFunction;
    Func<List<(Node, float)>, int, List<Node>> PruningFunction;

    public readonly int depth;
    public List<Node> children = new List<Node>();
    private float? score = null;

    public Node(BitBoard7x7 board, Turn? turn, Func<BitBoard7x7, float> EvaluationFunction, Func<List<(Node, float)>, int, List<Node>> PruningFunction, int depth = 0)
    {
        this.board = board;
        this.turn = turn;

        this.EvaluationFunction = EvaluationFunction;
        this.PruningFunction = PruningFunction;
        this.depth = depth;
    }

    public List<Node> MakeChildren()
    {
        // Kapazität vorab reservieren verhindert teures Array-Resizing unter der Haube
        List<Turn> turns = board.GenerateLegalTurns(new List<Turn>(40));
        List<(Node n, float s)> scored_nodes = new List<(Node n, float s)>(turns.Count);

        foreach (var turn in turns)
        {
            BitBoard7x7 new_board = board.Copy();
            new_board.MakeMove(turn);

            // Kein neues Lambda mehr! Die Funktion wird direkt durchgereicht.
            Node n = new Node(new_board, turn, EvaluationFunction, PruningFunction, depth: depth + 1);
            scored_nodes.Add((n, n.Score()));
        }

        // vorher war hier List<Node> children
        children = PruningFunction(scored_nodes, depth);
        score = null;
        return children;
    }

    public void ReplaceChild(int index, Node node)
    {
        children[index] = node;
    }

    public float Score()
    {
        if (score is float s) return s;
        if (children.Count == 0) score = EvaluationFunction(board); 
        else
        {
            float max = -99999;
            foreach (var node in children) max = Math.Max(max, -node.Score());
            score = max;
        }
        return (float)score;
    }

    public Node BestChild()
    {
        children.Sort((a, b) => (-b.Score()).CompareTo(-a.Score()));
        return children[0];
    }

    #region Verwaltung
    public override bool Equals(object? obj)
    {
        if (obj is not Node node) return false;
        else return (EqualTo(node));
    }

    private bool EqualTo(Node other)
    {
        return board.Equals(other.board);
    }

    public override int GetHashCode()
    {
        return board.GetHashCode();
    }
    #endregion
}