namespace Aya;

public class Evaluator
{
    private const int PawnValue = 100;
    private const int KnightValue = 320;
    private const int BishopValue = 330;
    private const int RookValue = 500;
    private const int QueenValue = 900;
    private const int KingValue = 20000;

    private const int MaxPhaseMaterial = 2 * (1 * 900 + 2 * 500 + 2 * 330 + 2 * 320);

    private static readonly int[] PawnOpeningPST = {
        0,  0,  0,  0,  0,  0,  0,  0,
        50, 50, 50, 50, 50, 50, 50, 50,
        10, 10, 20, 30, 30, 20, 10, 10,
         5,  5, 15, 30, 30, 15,  5,  5,
         0,  0, 10, 25, 25, 10,  0,  0,
         5, -5,-10,  0,  0,-10, -5,  5,
         5, 10, 10,-20,-20, 10, 10,  5,
         0,  0,  0,  0,  0,  0,  0,  0
    };

    private static readonly int[] PawnEndgamePST = {
        0,   0,   0,   0,   0,   0,   0,   0,
        100, 100, 100, 100, 100, 100, 100, 100, // Near promotion!
        60,  60,  60,  60,  60,  60,  60,  60,
        40,  40,  40,  40,  40,  40,  40,  40,
        25,  25,  25,  25,  25,  25,  25,  25,
        15,  15,  15,  15,  15,  15,  15,  15,
        5,   5,   5,   5,   5,   5,   5,   5,
        0,   0,   0,   0,   0,   0,   0,   0
    };

    private static readonly int[] KnightPST = {
        -50,-40,-30,-30,-30,-30,-40,-50,
        -40,-20,  0,  5,  5,  0,-20,-40,
        -30,  5, 10, 15, 15, 10,  5,-30,
        -30,  0, 15, 20, 20, 15,  0,-30,
        -30,  5, 15, 20, 20, 15,  5,-30,
        -30,  0, 10, 15, 15, 10,  0,-30,
        -40,-20,  0,  0,  0,  0,-20,-40,
        -50,-40,-30,-30,-30,-30,-40,-50
    };

    private static readonly int[] BishopPST = {
        -20,-10,-10,-10,-10,-10,-10,-20,
        -10,  5,  0,  0,  0,  0,  5,-10,
        -10, 10, 10, 10, 10, 10, 10,-10,
        -10,  0, 10, 10, 10, 10,  0,-10,
        -10,  5,  5, 10, 10,  5,  5,-10,
        -10,  0,  5, 10, 10,  5,  0,-10,
        -10,  0,  0,  0,  0,  0,  0,-10,
        -20,-10,-10,-10,-10,-10,-10,-20
    };

    private static readonly int[] RookPST = {
         0,  0,  0,  5,  5,  0,  0,  0,
        -5,  0,  0,  0,  0,  0,  0, -5,
        -5,  0,  0,  0,  0,  0,  0, -5,
        -5,  0,  0,  0,  0,  0,  0, -5,
        -5,  0,  0,  0,  0,  0,  0, -5,
        -5,  0,  0,  0,  0,  0,  0, -5,
         5, 10, 10, 10, 10, 10, 10,  5,
         0,  0,  0,  0,  0,  0,  0,  0
    };

    private static readonly int[] QueenPST = {
        -20,-10,-10, -5, -5,-10,-10,-20,
        -10,  0,  0,  0,  0,  0,  0,-10,
        -10,  5,  5,  5,  5,  5,  0,-10,
          0,  0,  5,  5,  5,  5,  0, -5,
         -5,  0,  5,  5,  5,  5,  0, -5,
        -10,  0,  5,  5,  5,  5,  0,-10,
        -10,  0,  0,  0,  0,  0,  0,-10,
        -20,-10,-10, -5, -5,-10,-10,-20
    };

    private static readonly int[] KingOpeningPST = {
        -30,-40,-40,-50,-50,-40,-40,-30,
        -30,-40,-40,-50,-50,-40,-40,-30,
        -30,-40,-40,-50,-50,-40,-40,-30,
        -30,-40,-40,-50,-50,-40,-40,-30,
        -20,-30,-30,-40,-40,-30,-30,-20,
        -10,-20,-20,-20,-20,-20,-20,-10,
         20, 20,  0,  0,  0,  0, 20, 20,
         20, 30, 10,  0,  0, 10, 30, 20
    };

    private static readonly int[] KingEndgamePST = {
        -50,-40,-30,-20,-20,-30,-40,-50,
        -30,-20,-10,  0,  0,-10,-20,-30,
        -30,-10, 20, 30, 30, 20,-10,-30,
        -30,-10, 30, 40, 40, 30,-10,-30,
        -30,-10, 30, 40, 40, 30,-10,-30,
        -30,-10, 20, 30, 30, 20,-10,-30,
        -30,-30,  0,  0,  0,  0,-30,-30,
        -50,-30,-30,-30,-30,-30,-30,-50
    };

    public int Evaluate(Board board)
    {
        int currentMaterialPhase = 0;
        
        // First pass: Calculate Material Phase
        for (int file = 0; file < 8; file++)
        {
            for (int rank = 0; rank < 8; rank++)
            {
                Piece piece = board.GetPiece(file, rank);
                if (piece.Type != PieceType.None && piece.Type != PieceType.King)
                {
                    currentMaterialPhase += GetPieceValue(piece.Type);
                }
            }
        }

        float phase = (float)currentMaterialPhase / MaxPhaseMaterial;
        if (phase > 1.0f) phase = 1.0f;

        int whiteScore = 0;
        int blackScore = 0;

        // Second pass: Full Evaluation
        for (int file = 0; file < 8; file++)
        {
            for (int rank = 0; rank < 8; rank++)
            {
                Piece piece = board.GetPiece(file, rank);
                if (piece.Type == PieceType.None) continue;

                int score = GetPieceValue(piece.Type);
                score += GetPositionValue(piece.Type, piece.Color, file, rank, phase);

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
            _ => 0
        };
    }

    private int GetPositionValue(PieceType type, PieceColor color, int file, int rank, float phase)
    {
        int index = rank * 8 + file;
        if (color == PieceColor.Black) index = (7 - rank) * 8 + file;

        return type switch
        {
            PieceType.Pawn => (int)(PawnOpeningPST[index] * phase + PawnEndgamePST[index] * (1.0f - phase)),
            PieceType.Knight => KnightPST[index],
            PieceType.Bishop => BishopPST[index],
            PieceType.Rook => RookPST[index],
            PieceType.Queen => QueenPST[index],
            PieceType.King => (int)(KingOpeningPST[index] * phase + KingEndgamePST[index] * (1.0f - phase)),
            _ => 0
        };
    }
}
