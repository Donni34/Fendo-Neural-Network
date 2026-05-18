using Fendo.Engine;
using Fendo.Logic;

// 1. Setup
Console.WriteLine("Fendo Engine v1.0 wird initialisiert...");
Board board = new Board(); // Deine Board-Klasse
Func<int, float> weight_vision = a => (float)a;
Func<int, float> weight_region = a => (float)2 * a;
Func<Board, float> evaluation = board => Heuristics.VisionBasedEvaluation(board, weight_vision, weight_region);
Func<List<(Node, float)>, List<Node>> pruning = scored_nodes => Heuristics.TopPercentagePruning(scored_nodes, 0.3f);

BreadthSearch search = new BreadthSearch(new Board(), evaluation, pruning);

// 2. Suche starten
Console.WriteLine("Suche nach dem besten Zug...");
(float score, Turn best_turn) = search.Evaluate(2);

// 3. Ergebnis ausgeben
Console.WriteLine($"Score ermittelt: {score}");