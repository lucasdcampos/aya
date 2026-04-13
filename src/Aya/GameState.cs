namespace Aya;

public struct GameState
{
    public Piece CapturedPiece { get; }
    public bool WhiteCanCastleKingSide { get; }
    public bool WhiteCanCastleQueenSide { get; }
    public bool BlackCanCastleKingSide { get; }
    public bool BlackCanCastleQueenSide { get; }
    public int EnPassantFile { get; }
    public int EnPassantRank { get; }
    public int HalfmoveClock { get; }

    public GameState(Piece capturedPiece, bool wK, bool wQ, bool bK, bool bQ, int epFile, int epRank, int halfmove)
    {
        CapturedPiece = capturedPiece;
        WhiteCanCastleKingSide = wK;
        WhiteCanCastleQueenSide = wQ;
        BlackCanCastleKingSide = bK;
        BlackCanCastleQueenSide = bQ;
        EnPassantFile = epFile;
        EnPassantRank = epRank;
        HalfmoveClock = halfmove;
    }
}
