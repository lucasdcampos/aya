using Aya;

Console.WriteLine("Aya - Chess Engine");
Console.WriteLine("------------------");

var board = new Board();
board.Print();

Console.WriteLine("\nAvailable pseudo-legal moves:");
var moveGenerator = new MoveGenerator(board);
var moves = moveGenerator.GeneratePseudoLegalMoves();

foreach (var move in moves)
{
    Console.Write($"{move} ");
}
Console.WriteLine($"\nTotal moves: {moves.Count()}");
