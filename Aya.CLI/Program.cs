using Aya;

Console.WriteLine("Aya - Chess Engine (Game Status Test)");
Console.WriteLine("-------------------------------------");

var board = new Board();
var generator = new MoveGenerator(board);

// 1. Checkmate test (Scholar's Mate)
Console.WriteLine("\nTesting Checkmate (Scholar's Mate):");
board.LoadFromFen("r1bqk1nr/pppp1ppp/2n5/2b1p3/2B1P3/5Q2/PPPP1PPP/RNB1K1NR w KQkq - 4 4");
var mateMove = new Move(5, 2, 5, 6); // f3 to f7
board.MakeMove(mateMove);
board.Print();
Console.WriteLine($"Status: {generator.GetGameStatus()}");

// 2. Stalemate test (Rei em a8, Dama em c7, Rei em a6)
Console.WriteLine("\nTesting Stalemate:");
board.LoadFromFen("k7/2Q5/K7/8/8/8/8/8 b - - 0 1");
board.Print();
Console.WriteLine($"Status: {generator.GetGameStatus()}");

// 3. Insufficient Material test
Console.WriteLine("\nTesting Insufficient Material:");
board.LoadFromFen("k7/8/K7/8/8/8/8/8 w - - 0 1");
board.Print();
Console.WriteLine($"Status: {generator.GetGameStatus()}");
