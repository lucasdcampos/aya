using Aya;

Console.WriteLine("Aya - Chess Engine");
Console.WriteLine("------------------");

var board = new Board();
var moveGenerator = new MoveGenerator(board);

Console.WriteLine("Initial Position:");
board.Print();

// Define some moves manually for testing
// e2 (4,1) to e4 (4,3)
var e2e4 = new Move(4, 1, 4, 3);
// e7 (4,6) to e5 (4,4)
var e7e5 = new Move(4, 6, 4, 4);

Console.WriteLine("\nMaking move: e2e4");
board.MakeMove(e2e4);
board.Print();

Console.WriteLine("\nMaking move: e7e5");
board.MakeMove(e7e5);
board.Print();

Console.WriteLine("\nUndoing move: e7e5");
board.UndoMove(e7e5);
board.Print();

Console.WriteLine("\nUndoing move: e2e4");
board.UndoMove(e2e4);
board.Print();

Console.WriteLine("\nFinal check - Position should be back to initial.");
