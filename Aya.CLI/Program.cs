using Aya;

Console.WriteLine("Aya - Engine vs Engine");
Console.WriteLine("----------------------");

var board = new Board();
var generator = new MoveGenerator(board);
var search = new Search(board);
int depth = 3;

while (true)
{
    Console.Clear();
    Console.WriteLine($"Aya - Engine vs Engine (Depth: {depth})");
    Console.WriteLine("-------------------------------------");
    board.Print();

    var status = generator.GetGameStatus();
    if (status != GameStatus.InProgress)
    {
        Console.WriteLine($"\nGame Over: {status}");
        break;
    }

    Console.WriteLine($"\nAya is thinking for {board.ActiveColor}...");
    search.StartSearch(depth);

    if (search.BestMove.Equals(default(Move)))
    {
        Console.WriteLine("Error: No moves found!");
        break;
    }

    if (search.UsedBook)
        Console.WriteLine($"[BOOK] Best Move: {search.BestMove}");
    else
        Console.WriteLine($"[SEARCH] Best Move: {search.BestMove} (Nodes: {search.NodesEvaluated})");

    board.MakeMove(search.BestMove);
    
    // Small delay to follow the moves
    //Thread.Sleep(800);
}
