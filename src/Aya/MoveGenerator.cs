namespace Aya;

public class MoveGenerator
{
    private readonly Board _board;
    private readonly List<Move> _moves = new();

    public MoveGenerator(Board board)
    {
        _board = board;
    }

    public GameStatus GetGameStatus()
    {
        // 1. Check repetition
        if (_board.GetPositionOccurrenceCount(_board.GetPositionKey()) >= 3)
        {
            return GameStatus.DrawByRepetition;
        }

        // 2. Check moves
        var legalMoves = GenerateLegalMoves().ToList();
        if (!legalMoves.Any())
        {
            if (IsInCheck(_board.ActiveColor))
            {
                return GameStatus.Checkmate;
            }
            return GameStatus.Stalemate;
        }

        // 3. Check 50-move rule
        if (_board.HalfmoveClock >= 100)
        {
            return GameStatus.DrawByFiftyMoveRule;
        }

        // 4. Check material
        if (_board.HasInsufficientMaterial())
        {
            return GameStatus.DrawByInsufficientMaterial;
        }

        return GameStatus.InProgress;
    }

    public bool IsInCheck(PieceColor color)
    {
        var (file, rank) = FindKing(color);
        PieceColor opponentColor = color == PieceColor.White ? PieceColor.Black : PieceColor.White;
        return IsSquareAttacked(file, rank, opponentColor);
    }

    public IEnumerable<Move> GenerateLegalMoves()
    {
        var pseudoMoves = GeneratePseudoLegalMoves().ToList();
        var legalMoves = new List<Move>();

        PieceColor movingColor = _board.ActiveColor;
        PieceColor opponentColor = movingColor == PieceColor.White ? PieceColor.Black : PieceColor.White;

        foreach (var move in pseudoMoves)
        {
            _board.MakeMove(move);
            if (!IsInCheck(movingColor))
            {
                legalMoves.Add(move);
            }
            _board.UndoMove(move);
        }

        return legalMoves;
    }

    public (int file, int rank) FindKing(PieceColor color)
    {
        for (int file = 0; file < 8; file++)
        {
            for (int rank = 0; rank < 8; rank++)
            {
                Piece p = _board.GetPiece(file, rank);
                if (p.Type == PieceType.King && p.Color == color) return (file, rank);
            }
        }
        return (-1, -1);
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
        int promotionRank = color == PieceColor.White ? 7 : 0;

        int nextRank = rank + direction;
        if (IsOnBoard(file, nextRank) && _board.GetPiece(file, nextRank).Type == PieceType.None)
        {
            if (nextRank == promotionRank)
            {
                AddPromotionMoves(file, rank, file, nextRank);
            }
            else
            {
                _moves.Add(new Move(file, rank, file, nextRank));
                int doubleNextRank = rank + 2 * direction;
                if (rank == startRank && IsOnBoard(file, doubleNextRank) && _board.GetPiece(file, doubleNextRank).Type == PieceType.None)
                {
                    _moves.Add(new Move(file, rank, file, doubleNextRank));
                }
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
                    if (nextRank == promotionRank)
                    {
                        AddPromotionMoves(file, rank, capFile, nextRank);
                    }
                    else
                    {
                        _moves.Add(new Move(file, rank, capFile, nextRank));
                    }
                }
                else if (capFile == _board.EnPassantFile && nextRank == _board.EnPassantRank)
                {
                    _moves.Add(new Move(file, rank, capFile, nextRank, MoveFlag.EnPassant));
                }
            }
        }
    }

    private void AddPromotionMoves(int fromFile, int fromRank, int toFile, int toRank)
    {
        _moves.Add(new Move(fromFile, fromRank, toFile, toRank, MoveFlag.Promotion, PieceType.Queen));
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

        GenerateCastlingMoves(file, rank, color);
    }

    private void GenerateCastlingMoves(int file, int rank, PieceColor color)
    {
        PieceColor opponentColor = color == PieceColor.White ? PieceColor.Black : PieceColor.White;

        if (color == PieceColor.White)
        {
            if (rank != 0 || file != 4) return;
            
            if (_board.WhiteCanCastleKingSide && 
                _board.GetPiece(5, 0).Type == PieceType.None && 
                _board.GetPiece(6, 0).Type == PieceType.None &&
                !IsSquareAttacked(4, 0, opponentColor) &&
                !IsSquareAttacked(5, 0, opponentColor) &&
                !IsSquareAttacked(6, 0, opponentColor))
            {
                _moves.Add(new Move(4, 0, 6, 0, MoveFlag.Castling));
            }
            if (_board.WhiteCanCastleQueenSide && 
                _board.GetPiece(1, 0).Type == PieceType.None &&
                _board.GetPiece(2, 0).Type == PieceType.None && 
                _board.GetPiece(3, 0).Type == PieceType.None &&
                !IsSquareAttacked(4, 0, opponentColor) &&
                !IsSquareAttacked(3, 0, opponentColor) &&
                !IsSquareAttacked(2, 0, opponentColor))
            {
                _moves.Add(new Move(4, 0, 2, 0, MoveFlag.Castling));
            }
        }
        else
        {
            if (rank != 7 || file != 4) return;

            if (_board.BlackCanCastleKingSide && 
                _board.GetPiece(5, 7).Type == PieceType.None && 
                _board.GetPiece(6, 7).Type == PieceType.None &&
                !IsSquareAttacked(4, 7, opponentColor) &&
                !IsSquareAttacked(5, 7, opponentColor) &&
                !IsSquareAttacked(6, 7, opponentColor))
            {
                _moves.Add(new Move(4, 7, 6, 7, MoveFlag.Castling));
            }
            if (_board.BlackCanCastleQueenSide && 
                _board.GetPiece(1, 7).Type == PieceType.None &&
                _board.GetPiece(2, 7).Type == PieceType.None && 
                _board.GetPiece(3, 7).Type == PieceType.None &&
                !IsSquareAttacked(4, 7, opponentColor) &&
                !IsSquareAttacked(3, 7, opponentColor) &&
                !IsSquareAttacked(2, 7, opponentColor))
            {
                _moves.Add(new Move(4, 7, 2, 7, MoveFlag.Castling));
            }
        }
    }

    public bool IsSquareAttacked(int file, int rank, PieceColor attackerColor)
    {
        int[] knF = { 1, 1, -1, -1, 2, 2, -2, -2 };
        int[] knR = { 2, -2, 2, -2, 1, -1, 1, -1 };
        for (int i = 0; i < 8; i++)
        {
            int f = file + knF[i];
            int r = rank + knR[i];
            if (IsOnBoard(f, r))
            {
                Piece p = _board.GetPiece(f, r);
                if (p.Type == PieceType.Knight && p.Color == attackerColor) return true;
            }
        }

        for (int df = -1; df <= 1; df++)
        {
            for (int dr = -1; dr <= 1; dr++)
            {
                if (df == 0 && dr == 0) continue;
                int f = file + df;
                int r = rank + dr;
                if (IsOnBoard(f, r))
                {
                    Piece p = _board.GetPiece(f, r);
                    if (p.Type == PieceType.King && p.Color == attackerColor) return true;
                }
            }
        }

        int pawnDir = attackerColor == PieceColor.White ? -1 : 1;
        int[] pawnFiles = { file - 1, file + 1 };
        foreach (int f in pawnFiles)
        {
            int r = rank + pawnDir;
            if (IsOnBoard(f, r))
            {
                Piece p = _board.GetPiece(f, r);
                if (p.Type == PieceType.Pawn && p.Color == attackerColor) return true;
            }
        }

        if (IsAttackedBySliding(file, rank, attackerColor, new[] { (1, 0), (-1, 0), (0, 1), (0, -1) }, PieceType.Rook)) return true;
        if (IsAttackedBySliding(file, rank, attackerColor, new[] { (1, 1), (1, -1), (-1, 1), (-1, -1) }, PieceType.Bishop)) return true;

        return false;
    }

    private bool IsAttackedBySliding(int file, int rank, PieceColor color, (int df, int dr)[] directions, PieceType type)
    {
        foreach (var (df, dr) in directions)
        {
            int f = file + df;
            int r = rank + dr;
            while (IsOnBoard(f, r))
            {
                Piece p = _board.GetPiece(f, r);
                if (p.Type != PieceType.None)
                {
                    if (p.Color == color && (p.Type == type || p.Type == PieceType.Queen)) return true;
                    break;
                }
                f += df;
                r += dr;
            }
        }
        return false;
    }

    private bool IsOnBoard(int file, int rank) => file >= 0 && file < 8 && rank >= 0 && rank < 8;
}
