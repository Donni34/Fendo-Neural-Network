using Fendo.Logic;

namespace Fendo.Engine;

public class Node
{
    public Board board { get; private set; }
    public readonly Turn? turn;

    Func<Board, float> EvaluationFunction;
    Func<List<(Turn, float)>, List<Turn>> PruningFunction;

    public List<Node> children = new List<Node>();
    private float? score = null;

    public Node(Board board, Turn? turn, Func<Board, float> EvaluationFunction, Func<List<(Turn, float)>, List<Turn>> PruningFunction)
    {
        this.board = board;
        this.turn = turn;

        this.EvaluationFunction = EvaluationFunction;
        this.PruningFunction = PruningFunction;
    }

    public List<Node> MakeChildren()
    {
        List<(Turn turn, float score)> scored_turns = board.GetTurns(board.active_player)
            .Select(t => (t, 0f))
            .ToList();
        List<Turn> turns = PruningFunction(scored_turns);
        List<Node> children = new List<Node>();
        foreach (var turn in turns)
        {
            Board new_board = board.Copy();
            new_board.ForceTurn(turn);
            children.Add(new Node(new_board, turn, EvaluationFunction, PruningFunction));
        }
        this.children = children;
        return children;
    }

    public void ReplaceChild(int index, Node node)
    {
        children[index] = node;
    }

    public float Score()
    {
        if (score is float s) return s;
        if (children.Count == 0) return EvaluationFunction(board);

        float max = -10000;
        foreach (var node in children) max = Math.Max(max, -node.Score());
        score = max;
        return max;
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
        return base.GetHashCode();
    }
    #endregion
}
