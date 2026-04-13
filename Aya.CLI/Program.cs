using Aya;

Console.WriteLine("Aya - Chess Engine (Evaluation Test)");
Console.WriteLine("------------------------------------");

var board = new Board();
var evaluator = new Evaluator();

Console.WriteLine("Initial Position Evaluation:");
board.Print();
Console.WriteLine($"Score: {evaluator.Evaluate(board)} (Positive: White favor, Negative: Black favor)");

// Move Knight to center
var nf3 = new Move(6, 0, 5, 2); // g1 to f3
Console.WriteLine("\nAfter g1f3 (Knight to center):");
board.MakeMove(nf3);
board.Print();
Console.WriteLine($"Score: {evaluator.Evaluate(board)}");

// Capture test
board.LoadFromFen("rnbqkbnr/ppp1pppp/8/3p4/4P3/8/PPPP1PPP/RNBQKBNR w KQkq - 0 2");
Console.WriteLine("\nPosition before capture (e4 d5):");
board.Print();
Console.WriteLine($"Score: {evaluator.Evaluate(board)}");

var exd5 = new Move(4, 3, 3, 4); // e4xd5
Console.WriteLine("\nAfter exd5 (White captures pawn):");
board.MakeMove(exd5);
board.Print();
Console.WriteLine($"Score: {evaluator.Evaluate(board)}");
