using Fendo.Engine;
using Fendo.Logic;

// 1. Setup
Console.WriteLine("Fendo Engine v1.0 wird initialisiert...");
Func<int, float> weight_vision = a => (float)a;
Func<int, float> weight_region = a => (float)2 * a;
Func<BitBoard7x7, float> evaluation = board => Heuristics.VisionBasedEvaluation(board, weight_vision, weight_region);
float q = (float)Math.Sqrt(2);
Func<List<(Node, float)>, int, List<Node>> pruning = (scored_nodes, depth) => Heuristics.GeometricPruning(scored_nodes, 1f, q, depth);

BreadthSearch search = new BreadthSearch(new BitBoard7x7 (), evaluation, pruning);

// 2. Suche starten
Console.WriteLine("Suche nach dem besten Zug...");
(float score, Turn best_turn) = search.Evaluate(3);

// 3. Ergebnis ausgeben
Console.WriteLine($"Score ermittelt: {score}");