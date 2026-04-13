namespace Aya;

public class MoveGenerator
{
    private readonly Board _board;
    private readonly List<Move> _moves = new();

    public MoveGenerator(Board board)
    {
        _board = board;
    }

    public IEnumerable<Move> GeneratePseudoLegalMoves()
    {
        _moves.Clear();

        for (int file = 0; file < 8; file++)
        {
            for (int rank = 0; rank < 8; rank++)
            {
                Piece piece = _board.GetPiece(file, rank);
                if (piece.Color == _board.ActiveColor)
                {
                    GeneratePieceMoves(file, rank, piece);
                }
            }
        }

        return _moves;
    }

    private void GeneratePieceMoves(int file, int rank, Piece piece)
    {
        switch (piece.Type)
        {
            case PieceType.Pawn:
                GeneratePawnMoves(file, rank, piece.Color);
                break;
            case PieceType.Knight:
                GenerateKnightMoves(file, rank, piece.Color);
                break;
            case PieceType.Bishop:
                GenerateSlidingMoves(file, rank, piece.Color, new[] { (1, 1), (1, -1), (-1, 1), (-1, -1) });
                break;
            case PieceType.Rook:
                GenerateSlidingMoves(file, rank, piece.Color, new[] { (1, 0), (-1, 0), (0, 1), (0, -1) });
                break;
            case PieceType.Queen:
                GenerateSlidingMoves(file, rank, piece.Color, new[] { (1, 1), (1, -1), (-1, 1), (-1, -1), (1, 0), (-1, 0), (0, 1), (0, -1) });
                break;
            case PieceType.King:
                GenerateKingMoves(file, rank, piece.Color);
                break;
        }
    }

    private void GeneratePawnMoves(int file, int rank, PieceColor color)
    {
        int direction = color == PieceColor.White ? 1 : -1;
        int startRank = color == PieceColor.White ? 1 : 6;

        int nextRank = rank + direction;
        if (IsOnBoard(file, nextRank) && _board.GetPiece(file, nextRank).Type == PieceType.None)
        {
            _moves.Add(new Move(file, rank, file, nextRank));
            
            int doubleNextRank = rank + 2 * direction;
            if (rank == startRank && IsOnBoard(file, doubleNextRank) && _board.GetPiece(file, doubleNextRank).Type == PieceType.None)
            {
                _moves.Add(new Move(file, rank, file, doubleNextRank));
            }
        }

        int[] captureFiles = { file - 1, file + 1 };
        foreach (int capFile in captureFiles)
        {
            if (IsOnBoard(capFile, nextRank))
            {
                Piece target = _board.GetPiece(capFile, nextRank);
                if (target.Type != PieceType.None && target.Color != color)
                {
                    _moves.Add(new Move(file, rank, capFile, nextRank));
                }
            }
        }
    }

    private void GenerateKnightMoves(int file, int rank, PieceColor color)
    {
        int[] df = { 1, 1, -1, -1, 2, 2, -2, -2 };
        int[] dr = { 2, -2, 2, -2, 1, -1, 1, -1 };

        for (int i = 0; i < 8; i++)
        {
            int nextFile = file + df[i];
            int nextRank = rank + dr[i];

            if (IsOnBoard(nextFile, nextRank))
            {
                Piece target = _board.GetPiece(nextFile, nextRank);
                if (target.Color != color)
                {
                    _moves.Add(new Move(file, rank, nextFile, nextRank));
                }
            }
        }
    }

    private void GenerateSlidingMoves(int file, int rank, PieceColor color, (int df, int dr)[] directions)
    {
        foreach (var (df, dr) in directions)
        {
            int nextFile = file + df;
            int nextRank = rank + dr;

            while (IsOnBoard(nextFile, nextRank))
            {
                Piece target = _board.GetPiece(nextFile, nextRank);
                if (target.Type == PieceType.None)
                {
                    _moves.Add(new Move(file, rank, nextFile, nextRank));
                }
                else
                {
                    if (target.Color != color)
                    {
                        _moves.Add(new Move(file, rank, nextFile, nextRank));
                    }
                    break;
                }
                nextFile += df;
                nextRank += dr;
            }
        }
    }

    private void GenerateKingMoves(int file, int rank, PieceColor color)
    {
        for (int df = -1; df <= 1; df++)
        {
            for (int dr = -1; dr <= 1; dr++)
            {
                if (df == 0 && dr == 0) continue;

                int nextFile = file + df;
                int nextRank = rank + dr;

                if (IsOnBoard(nextFile, nextRank))
                {
                    Piece target = _board.GetPiece(nextFile, nextRank);
                    if (target.Color != color)
                    {
                        _moves.Add(new Move(file, rank, nextFile, nextRank));
                    }
                }
            }
        }
    }

    private bool IsOnBoard(int file, int rank) => file >= 0 && file < 8 && rank >= 0 && rank < 8;
}
