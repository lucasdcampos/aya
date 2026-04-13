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
        Assert.False(board.WhiteCanCastleKingSide);
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

    [Fact]
    public void LegalMoves_PinnedPiece_CannotMove()
    {
        // White king on e1, White knight on e2, Black rook on e8
        // The knight is pinned and cannot move
        var fen = "4r3/8/8/8/8/8/4N3/4K3 w - - 0 1";
        var board = new Board();
        board.LoadFromFen(fen);
        var generator = new MoveGenerator(board);
        
        var moves = generator.GenerateLegalMoves();
        
        // Knight at e2 (4,1) should have NO legal moves because it's pinned
        var knightMoves = moves.Where(m => m.FromFile == 4 && m.FromRank == 1);
        Assert.Empty(knightMoves);
    }

    [Fact]
    public void LegalMoves_MustEscapeCheck()
    {
        // White king on e1, Black rook on e8
        // White must move the king or block the check (but here only king moves are possible)
        var fen = "4r3/8/8/8/8/8/8/4K3 w - - 0 1";
        var board = new Board();
        board.LoadFromFen(fen);
        var generator = new MoveGenerator(board);
        
        var moves = generator.GenerateLegalMoves();
        
        // King must move to d1, f1, d2, e2, f2 (e2 is blocked by check too, so only d1, f1, d2, f2)
        // Wait, e2 is also under attack by rook.
        foreach (var move in moves)
        {
            board.MakeMove(move);
            var (kFile, kRank) = (move.ToFile, move.ToRank); // King just moved
            var attackerColor = PieceColor.Black;
            Assert.False(generator.IsSquareAttacked(kFile, kRank, attackerColor));
            board.UndoMove(move);
        }
    }
}
