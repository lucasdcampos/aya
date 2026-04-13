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
        var moves = generator.GeneratePseudoLegalMoves();
        
        Assert.Equal(20, moves.Count());
    }

    [Fact]
    public void MakeMove_UpdatesPosition()
    {
        var board = new Board();
        var move = new Move(4, 1, 4, 3); // e2e4
        
        board.MakeMove(move);
        
        Assert.Equal(PieceType.Pawn, board.GetPiece(4, 3).Type);
        Assert.Equal(PieceColor.White, board.GetPiece(4, 3).Color);
        Assert.Equal(PieceType.None, board.GetPiece(4, 1).Type);
        Assert.Equal(PieceColor.Black, board.ActiveColor);
    }

    [Fact]
    public void UndoMove_RestoresPosition()
    {
        var board = new Board();
        var move = new Move(4, 1, 4, 3); // e2e4
        
        board.MakeMove(move);
        board.UndoMove(move);
        
        Assert.Equal(PieceType.Pawn, board.GetPiece(4, 1).Type);
        Assert.Equal(PieceType.None, board.GetPiece(4, 3).Type);
        Assert.Equal(PieceColor.White, board.ActiveColor);
        Assert.Equal(1, board.FullmoveNumber);
    }

    [Fact]
    public void EnPassant_CaptureRemovesEnemyPawn()
    {
        // Position where white pawn on e5 can capture black pawn on d5 via en passant
        var fen = "rnbqkbnr/ppp1pppp/8/3pP3/8/8/PPPP1PPP/RNBQKBNR w KQkq d6 0 3";
        var board = new Board();
        board.LoadFromFen(fen);
        
        // e5 (4,4) to d6 (3,5)
        var epMove = new Move(4, 4, 3, 5, MoveFlag.EnPassant);
        
        board.MakeMove(epMove);
        
        Assert.Equal(PieceType.Pawn, board.GetPiece(3, 5).Type); // Pawn now on d6
        Assert.Equal(PieceType.None, board.GetPiece(3, 4).Type); // Black pawn on d5 should be gone
    }

    [Fact]
    public void EnPassant_UndoRestoresEnemyPawn()
    {
        var fen = "rnbqkbnr/ppp1pppp/8/3pP3/8/8/PPPP1PPP/RNBQKBNR w KQkq d6 0 3";
        var board = new Board();
        board.LoadFromFen(fen);
        
        var epMove = new Move(4, 4, 3, 5, MoveFlag.EnPassant);
        
        board.MakeMove(epMove);
        board.UndoMove(epMove);
        
        Assert.Equal(PieceType.Pawn, board.GetPiece(4, 4).Type); // White pawn back to e5
        Assert.Equal(PieceType.Pawn, board.GetPiece(3, 4).Type); // Black pawn back to d5
        Assert.Equal(3, board.EnPassantFile); // EP target square d6 (file 3)
        Assert.Equal(5, board.EnPassantRank); // EP target square d6 (rank 5)
    }
}
