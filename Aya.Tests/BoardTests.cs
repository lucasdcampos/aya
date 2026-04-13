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
        var fen = "rnbqkbnr/ppp1pppp/8/3pP3/8/8/PPPP1PPP/RNBQKBNR w KQkq d6 0 3";
        var board = new Board();
        board.LoadFromFen(fen);
        
        var epMove = new Move(4, 4, 3, 5, MoveFlag.EnPassant);
        board.MakeMove(epMove);
        
        Assert.Equal(PieceType.Pawn, board.GetPiece(3, 5).Type);
        Assert.Equal(PieceType.None, board.GetPiece(3, 4).Type);
    }

    [Fact]
    public void Castling_WhiteKingSide_MovesRook()
    {
        var fen = "r1bqkbnr/pppp1ppp/2n5/4p3/2B1P3/5N2/PPPP1PPP/RNBQK2R w KQkq - 4 4";
        var board = new Board();
        board.LoadFromFen(fen);
        
        var castlingMove = new Move(4, 0, 6, 0, MoveFlag.Castling);
        board.MakeMove(castlingMove);
        
        Assert.Equal(PieceType.King, board.GetPiece(6, 0).Type);
        Assert.Equal(PieceType.Rook, board.GetPiece(5, 0).Type);
        Assert.Equal(PieceType.None, board.GetPiece(4, 0).Type);
        Assert.Equal(PieceType.None, board.GetPiece(7, 0).Type);
        Assert.False(board.WhiteCanCastleKingSide);
    }

    [Fact]
    public void Promotion_PawnToQueen()
    {
        // White pawn on a7, black king on h8
        var fen = "7k/P7/8/8/8/8/8/7K w - - 0 1";
        var board = new Board();
        board.LoadFromFen(fen);
        
        var promotionMove = new Move(0, 6, 0, 7, MoveFlag.Promotion, PieceType.Queen);
        board.MakeMove(promotionMove);
        
        Assert.Equal(PieceType.Queen, board.GetPiece(0, 7).Type);
        Assert.Equal(PieceColor.White, board.GetPiece(0, 7).Color);
        Assert.Equal(PieceType.None, board.GetPiece(0, 6).Type);
    }

    [Fact]
    public void Promotion_UndoRestoresPawn()
    {
        var fen = "7k/P7/8/8/8/8/8/7K w - - 0 1";
        var board = new Board();
        board.LoadFromFen(fen);
        
        var promotionMove = new Move(0, 6, 0, 7, MoveFlag.Promotion, PieceType.Queen);
        board.MakeMove(promotionMove);
        board.UndoMove(promotionMove);
        
        Assert.Equal(PieceType.Pawn, board.GetPiece(0, 6).Type);
        Assert.Equal(PieceType.None, board.GetPiece(0, 7).Type);
        Assert.Equal(PieceColor.White, board.GetPiece(0, 6).Color);
    }
}
