namespace Aya;

public class Evaluator
{
    private const int PawnValue = 100;
    private const int KnightValue = 320;
    private const int BishopValue = 330;
    private const int RookValue = 500;
    private const int QueenValue = 900;
    private const int KingValue = 20000;

    // Piece-Square Tables (PST) - Bonus for piece positioning
    // Higher values mean the piece "prefers" that square.
    // Tables are from White's perspective (Rank 1 to 8).
    
    private static readonly int[] PawnPST = {
        0,  0,  0,  0,  0,  0,  0,  0,
        50, 50, 50, 50, 50, 50, 50, 50,
        10, 10, 20, 30, 30, 20, 10, 10,
         5,  5, 10, 25, 25, 10,  5,  5,
         0,  0,  0, 20, 20,  0,  0,  0,
         5, -5,-10,  0,  0,-10, -5,  5,
         5, 10, 10,-20,-20, 10, 10,  5,
         0,  0,  0,  0,  0,  0,  0,  0
    };

    private static readonly int[] KnightPST = {
        -50,-40,-30,-30,-30,-30,-40,-50,
        -40,-20,  0,  0,  0,  0,-20,-40,
        -30,  0, 10, 15, 15, 10,  0,-30,
        -30,  5, 15, 20, 20, 15,  5,-30,
        -30,  0, 15, 20, 20, 15,  0,-30,
        -30,  5, 10, 15, 15, 10,  5,-30,
        -40,-20,  0,  5,  5,  0,-20,-40,
        -50,-40,-30,-30,-30,-30,-40,-50
    };

    public int Evaluate(Board board)
    {
        int whiteScore = 0;
        int blackScore = 0;

        for (int file = 0; file < 8; file++)
        {
            for (int rank = 0; rank < 8; rank++)
            {
                Piece piece = board.GetPiece(file, rank);
                if (piece.Type == PieceType.None) continue;

                int score = GetPieceValue(piece.Type);
                score += GetPositionValue(piece.Type, piece.Color, file, rank);

                if (piece.Color == PieceColor.White)
                    whiteScore += score;
                else
                    blackScore += score;
            }
        }

        return whiteScore - blackScore;
    }

    private int GetPieceValue(PieceType type)
    {
        return type switch
        {
            PieceType.Pawn => PawnValue,
            PieceType.Knight => KnightValue,
            PieceType.Bishop => BishopValue,
            PieceType.Rook => RookValue,
            PieceType.Queen => QueenValue,
            PieceType.King => KingValue,
            _ => 0
        };
    }

    private int GetPositionValue(PieceType type, PieceColor color, int file, int rank)
    {
        int index = rank * 8 + file;
        
        // If Black, flip the index to use the same table
        if (color == PieceColor.Black)
        {
            index = (7 - rank) * 8 + file;
        }

        return type switch
        {
            PieceType.Pawn => PawnPST[index],
            PieceType.Knight => KnightPST[index],
            _ => 0
        };
    }
}
