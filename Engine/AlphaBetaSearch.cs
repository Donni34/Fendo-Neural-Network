using Fendo.Logic;
using System;
using System.Collections.Generic;

namespace Fendo.Engine;

public enum HashFlag : byte { Exact, Alpha, Beta }

public struct TTEntry
{
    public ulong Hash; 
    public float Score;
    public int Depth;
    public HashFlag Flag;
    public Turn BestTurn;
}

public class AlphaBetaSearch
{
    private Func<BitBoard7x7, float> EvaluationFunction;
    private const int TTMask = 0x3FFFFF;
    private TTEntry[] transpositionTable = new TTEntry[TTMask + 1];

    // NEU: Ein Array von Listen. Für jede Suchtiefe genau eine Liste.
    private List<Turn>[] turnBuffers;

    public AlphaBetaSearch(Func<BitBoard7x7, float> evaluationFunction)
    {
        this.EvaluationFunction = evaluationFunction;
        //this.transpositionTable = new Dictionary<ulong, TTEntry>(1000000);

        // 50 Schichten reichen für Alpha-Beta-Suchen bei diesem Spiel völlig aus.
        this.turnBuffers = new List<Turn>[50];
        for (int i = 0; i < turnBuffers.Length; i++)
        {
            // Einmalig allokieren!
            turnBuffers[i] = new List<Turn>(40);
        }
    }

    public (float, Turn) Evaluate(BitBoard7x7 board, int maxDepth)
    {
        //transpositionTable.Clear();

        float score = 0;
        Turn bestTurn = new Turn();

        // Iterative Deepening: Wir suchen erst Tiefe 1, dann 2, 3...
        // Das füllt die Transposition Table mit perfekten Vorhersagen für die nächste Tiefe.
        for (int currentDepth = 1; currentDepth <= maxDepth; currentDepth++)
        {
            (score, bestTurn) = Search(board, currentDepth, -float.MaxValue, float.MaxValue);
        }

        return (score, bestTurn);
    }

    private (float, Turn) Search(in BitBoard7x7 currentBoard, int depth, float alpha, float beta)
    {
        float alphaOriginal = alpha;
        Turn ttBestTurn = new Turn(); // Merkt sich den besten Zug aus dem Cache

        //// --- 1. TT Lookup ---
        //if (transpositionTable.TryGetValue(currentBoard.Hash, out TTEntry entry))
        //{
        //    ttBestTurn = entry.BestTurn; // Selbst wenn die Tiefe nicht reicht, ist der Zug extrem wertvoll!

        //    if (entry.Depth >= depth)
        //    {
        //        if (entry.Flag == HashFlag.Exact) return (entry.Score, ttBestTurn);

        //        if (entry.Flag == HashFlag.Alpha) alpha = Math.Max(alpha, entry.Score);
        //        else if (entry.Flag == HashFlag.Beta) beta = Math.Min(beta, entry.Score);

        //        if (alpha >= beta) return (entry.Score, ttBestTurn);
        //    }
        //}

        // --- 1. TT Lookup ---
        int ttIndex = (int)(currentBoard.Hash & TTMask);
        ref TTEntry entry = ref transpositionTable[ttIndex]; // 'ref' = Zero Allocation!

        // Gehört dieser Array-Eintrag wirklich zu unserem aktuellen Brett?
        if (entry.Hash == currentBoard.Hash)
        {
            ttBestTurn = entry.BestTurn;

            if (entry.Depth >= depth)
            {
                if (entry.Flag == HashFlag.Exact) return (entry.Score, ttBestTurn);

                if (entry.Flag == HashFlag.Alpha) alpha = Math.Max(alpha, entry.Score);
                else if (entry.Flag == HashFlag.Beta) beta = Math.Min(beta, entry.Score);

                if (alpha >= beta) return (entry.Score, ttBestTurn);
            }
        }

        if (depth == 0 || currentBoard.IsFinished())
        {
            return (EvaluationFunction(currentBoard), new Turn());
        }

        List<Turn> turns = turnBuffers[depth];
        currentBoard.GeneratePseudoLegalTurns(turns);

        if (turns.Count == 0) return (EvaluationFunction(currentBoard), new Turn());

        // --- 2. MOVE ORDERING ---
        bool hasTtTurn = false; // Merkt sich, ob wir einen gültigen TT-Zug gefunden haben

        // Hier nutzen wir direkt dein bit-gepacktes 'Value' (falls 0 ein leerer Zug ist)
        if (ttBestTurn.Value != 0)
        {
            int index = -1;
            for (int i = 0; i < turns.Count; i++)
            {
                // MEGA-BOOST: Wir vergleichen einfach den ushort statt 3 Properties!
                if (turns[i].Value == ttBestTurn.Value)
                {
                    index = i;
                    break;
                }
            }

            if (index >= 0)
            {
                Turn temp = turns[0];
                turns[0] = ttBestTurn;
                turns[index] = temp;

                hasTtTurn = true; // Bingo! Auf Index 0 liegt jetzt ein zu 100 % verifizierter Zug.
            }
        }

        float bestScore = -float.MaxValue;
        Turn bestTurn = turns[0];
        bool valid_turn_exists = false;

        // --- 3. LAZY VALIDATION (mit for-Schleife statt foreach) ---
        for (int i = 0; i < turns.Count; i++)
        {
            Turn turn = turns[i];

            // Wir wissen: Wenn hasTtTurn true ist, ist exakt der 0-te Durchlauf garantiert legal.
            bool isVerified = (i == 0 && hasTtTurn);

            if (!isVerified && turn.Type != Border.NaB)
            {
                if (!currentBoard.ValidateWallPlacement(turn)) continue;
            }

            valid_turn_exists = true;

            BitBoard7x7 nextBoard = currentBoard.Copy();
            nextBoard.MakeMove(turn);

            (float childScore, _) = Search(nextBoard, depth - 1, -beta, -alpha);
            childScore = -childScore;

            if (childScore > bestScore)
            {
                bestScore = childScore;
                bestTurn = turn;
            }

            if (bestScore > alpha) alpha = bestScore;
            if (alpha >= beta) break;
        }
        if (!valid_turn_exists) return (EvaluationFunction(currentBoard), new Turn());


        // --- 3. TT Save ---
        HashFlag flag = HashFlag.Exact;
        if (bestScore <= alphaOriginal) flag = HashFlag.Alpha;
        else if (bestScore >= beta) flag = HashFlag.Beta;



        //transpositionTable[currentBoard.Hash] = new TTEntry
        //{
        //    Score = bestScore,
        //    Depth = depth,
        //    Flag = flag,
        //    BestTurn = bestTurn // NEU: Zug im Cache ablegen
        //};

        transpositionTable[ttIndex] = new TTEntry
        {
            Hash = currentBoard.Hash, // Den Hash des Brettes ablegen!
            Score = bestScore,
            Depth = depth,
            Flag = flag,
            BestTurn = bestTurn
        };

        return (bestScore, bestTurn);
    }
}