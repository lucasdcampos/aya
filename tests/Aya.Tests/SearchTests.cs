using Aya;
using Xunit;

namespace Aya.Tests;

public class SearchTests
{
    [Fact]
    public void OpeningBook_InitialPosition_ReturnsE2E4()
    {
        var board = new Board();
        var search = new Search(board);
        
        search.StartSearch(1);
        
        Assert.True(search.UsedBook);
        Assert.Equal("e2e4", search.BestMove.ToString());
    }

    [Fact]
    public void Search_FindsMateInOne()
    {
        // Black is one move away from being mated (Qf7#)
        var fen = "r1bqk1nr/pppp1ppp/2n5/2b1p3/2B1P3/5Q2/PPPP1PPP/RNB1K1NR w KQkq - 0 1";
        var board = new Board();
        board.LoadFromFen(fen);
        var search = new Search(board);
        
        search.StartSearch(2);
        
        Assert.Equal("f3f7", search.BestMove.ToString());
    }

    [Fact]
    public void Search_AvoidsImmediateMate()
    {
        // White threatens mate on f7. Black must defend.
        var fen = "r1bqkbnr/pppp1ppp/2n5/4p3/2B1P3/5Q2/PPPP1PPP/RNB1K1NR b KQkq - 0 1";
        var board = new Board();
        board.LoadFromFen(fen);
        var search = new Search(board);
        
        search.StartSearch(3);
        
        // Legal defensive moves: Nf6, Qf6, Qe7
        string moveStr = search.BestMove.ToString();
        Assert.True(moveStr == "g8f6" || moveStr == "d8f6" || moveStr == "d8e7");
    }
}
