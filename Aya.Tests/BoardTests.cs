using Aya;
using Xunit;

namespace Aya.Tests;

public class BoardTests
{
    [Fact]
    public void InitialPosition_MoveCount_IsTwenty()
    {
        var board = new Board();
        var generator = new MoveGenerator(board);
        var moves = generator.GenerateLegalMoves();
        
        Assert.Equal(20, moves.Count());
    }

    [Fact]
    public void ThreefoldRepetition_IsDetected()
    {
        var board = new Board();
        var generator = new MoveGenerator(board);

        // Moves: Nf3 Nc6 Ng1 Nb8 Nf3 Nc6 Ng1 Nb8 (Repetition)
        var nf3 = new Move(6, 0, 5, 2);
        var nc6 = new Move(1, 7, 2, 5);
        var ng1 = new Move(5, 2, 6, 0);
        var nb8 = new Move(2, 5, 1, 7);

        board.MakeMove(nf3);
        board.MakeMove(nc6);
        board.MakeMove(ng1);
        board.MakeMove(nb8); // 2nd time initial pos occurs
        
        board.MakeMove(nf3);
        board.MakeMove(nc6);
        board.MakeMove(ng1);
        board.MakeMove(nb8); // 3rd time initial pos occurs

        Assert.Equal(GameStatus.DrawByRepetition, generator.GetGameStatus());
    }

    [Fact]
    public void InsufficientMaterial_KvsK_IsDraw()
    {
        var board = new Board();
        board.LoadFromFen("k7/8/8/8/8/8/8/K7 w - - 0 1");
        var generator = new MoveGenerator(board);
        
        Assert.Equal(GameStatus.DrawByInsufficientMaterial, generator.GetGameStatus());
    }

    [Fact]
    public void EnPassant_CaptureRemovesEnemyPawn()
    {
        var fen = "rnbqkbnr/ppp1pppp/8/3pP3/8/8/PPPP1PPP/RNBQKBNR w KQkq d6 0 3";
        var board = new Board();
        board.LoadFromFen(fen);
        
        var epMove = new Move(4, 4, 3, 5, MoveFlag.EnPassant);
        board.MakeMove(epMove);
        
        Assert.Equal(PieceType.Pawn, board.GetPiece(3, 5).Type);
        Assert.Equal(PieceType.None, board.GetPiece(3, 4).Type);
    }

    [Fact]
    public void Promotion_PawnToQueen()
    {
        var fen = "7k/P7/8/8/8/8/8/7K w - - 0 1";
        var board = new Board();
        board.LoadFromFen(fen);
        
        var promotionMove = new Move(0, 6, 0, 7, MoveFlag.Promotion, PieceType.Queen);
        board.MakeMove(promotionMove);
        
        Assert.Equal(PieceType.Queen, board.GetPiece(0, 7).Type);
    }
}
