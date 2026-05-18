using Fendo.Logic;

namespace Fendo.Engine;

public class Node
{
    public Board board { get; private set; }
    public readonly Turn? turn;

    Func<Board, float> EvaluationFunction;
    Func<List<(Node, float)>, int, List<Node>> PruningFunction;

    public readonly int depth;
    public List<Node> children = new List<Node>();
    private float? score = null;

    public Node(Board board, Turn? turn, Func<Board, float> EvaluationFunction, Func<List<(Node, float)>, int, List<Node>> PruningFunction, int depth = 0)
    {
        this.board = board;
        this.turn = turn;

        this.EvaluationFunction = EvaluationFunction;
        this.PruningFunction = PruningFunction;
        this.depth = depth;
    }

    public List<Node> MakeChildren()
    {
        List<Turn> turns = board.GetTurns(board.active_player);
        List<(Node n, float s)> scored_nodes = new List<(Node n, float s)>();
        Func<Board, float> child_eval = board => -EvaluationFunction(board);
        foreach (var turn in turns)
        {
            Board new_board = board.Copy();
            new_board.ForceTurn(turn);
            Node n = new Node(new_board, turn, child_eval, PruningFunction, depth: depth+1);
            float s = n.Score();
            scored_nodes.Add((n, s));
        }
        List<Node> children = PruningFunction(scored_nodes, depth);
        this.children = children;
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
            float max = -10000;
            foreach (var node in children) max = Math.Max(max, node.Score());
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