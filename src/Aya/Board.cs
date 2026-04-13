namespace Aya;

public class Board
{
    private readonly Piece[,] _squares = new Piece[8, 8];
    private readonly Stack<GameState> _history = new();
    private readonly List<string> _positionHistory = new();
    
    public PieceColor ActiveColor { get; private set; }
    public bool WhiteCanCastleKingSide { get; private set; }
    public bool WhiteCanCastleQueenSide { get; private set; }
    public bool BlackCanCastleKingSide { get; private set; }
    public bool BlackCanCastleQueenSide { get; private set; }
    public int EnPassantFile { get; private set; } = -1;
    public int EnPassantRank { get; private set; } = -1;
    public int HalfmoveClock { get; private set; }
    public int FullmoveNumber { get; private set; }

    public const string InitialFen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";

    public Board()
    {
        LoadFromFen(InitialFen);
    }

    public Piece GetPiece(int file, int rank) => _squares[file, rank];

    public void LoadFromFen(string fen)
    {
        string[] parts = fen.Split(' ');
        if (parts.Length < 1) return;

        ParsePiecePlacement(parts[0]);

        if (parts.Length > 1)
        {
            ActiveColor = parts[1] == "w" ? PieceColor.White : PieceColor.Black;
        }

        if (parts.Length > 2)
        {
            string castling = parts[2];
            WhiteCanCastleKingSide = castling.Contains('K');
            WhiteCanCastleQueenSide = castling.Contains('Q');
            BlackCanCastleKingSide = castling.Contains('k');
            BlackCanCastleQueenSide = castling.Contains('q');
        }

        if (parts.Length > 3)
        {
            string ep = parts[3];
            if (ep != "-")
            {
                EnPassantFile = ep[0] - 'a';
                EnPassantRank = ep[1] - '1';
            }
            else
            {
                EnPassantFile = -1;
                EnPassantRank = -1;
            }
        }

        if (parts.Length > 4 && int.TryParse(parts[4], out int halfmove))
        {
            HalfmoveClock = halfmove;
        }

        if (parts.Length > 5 && int.TryParse(parts[5], out int fullmove))
        {
            FullmoveNumber = fullmove;
        }

        _positionHistory.Clear();
        _positionHistory.Add(GetPositionKey());
    }

    public string GetPositionKey()
    {
        var fen = new System.Text.StringBuilder();

        // 1. Piece placement
        for (int rank = 7; rank >= 0; rank--)
        {
            int emptySquares = 0;
            for (int file = 0; file < 8; file++)
            {
                Piece piece = _squares[file, rank];
                if (piece.Type == PieceType.None)
                {
                    emptySquares++;
                }
                else
                {
                    if (emptySquares > 0)
                    {
                        fen.Append(emptySquares);
                        emptySquares = 0;
                    }
                    fen.Append(PieceToChar(piece));
                }
            }
            if (emptySquares > 0) fen.Append(emptySquares);
            if (rank > 0) fen.Append('/');
        }

        // 2. Active color
        fen.Append($" {(ActiveColor == PieceColor.White ? 'w' : 'b')}");

        // 3. Castling rights
        fen.Append(" ");
        string castling = "";
        if (WhiteCanCastleKingSide) castling += "K";
        if (WhiteCanCastleQueenSide) castling += "Q";
        if (BlackCanCastleKingSide) castling += "k";
        if (BlackCanCastleQueenSide) castling += "q";
        fen.Append(string.IsNullOrEmpty(castling) ? "-" : castling);

        // 4. En Passant target
        fen.Append(" ");
        if (EnPassantFile != -1)
        {
            fen.Append((char)('a' + EnPassantFile));
            fen.Append(EnPassantRank + 1);
        }
        else
        {
            fen.Append("-");
        }

        return fen.ToString();
    }

    public int GetPositionOccurrenceCount(string key)
    {
        int count = 0;
        foreach (var p in _positionHistory)
        {
            if (p == key) count++;
        }
        return count;
    }

    public string GetCurrentFen()
    {
        return $"{GetPositionKey()} {HalfmoveClock} {FullmoveNumber}";
    }

    private char PieceToChar(Piece p)
    {
        char c = p.Type switch
        {
            PieceType.Pawn => 'P',
            PieceType.Knight => 'N',
            PieceType.Bishop => 'B',
            PieceType.Rook => 'R',
            PieceType.Queen => 'Q',
            PieceType.King => 'K',
            _ => '?'
        };
        return p.Color == PieceColor.White ? c : char.ToLower(c);
    }

    public bool HasInsufficientMaterial()
    {
        int whitePieces = 0;
        int blackPieces = 0;
        for (int file = 0; file < 8; file++)
        {
            for (int rank = 0; rank < 8; rank++)
            {
                Piece p = _squares[file, rank];
                if (p.Type == PieceType.None || p.Type == PieceType.King) continue;
                if (p.Color == PieceColor.White) whitePieces++;
                else blackPieces++;
                if (p.Type != PieceType.Bishop && p.Type != PieceType.Knight) return false;
            }
        }
        return whitePieces <= 1 && blackPieces <= 1;
    }

    public void MakeMove(Move move)
    {
        Piece piece = _squares[move.FromFile, move.FromRank];
        Piece capturedPiece = _squares[move.ToFile, move.ToRank];

        if (move.Flag == MoveFlag.EnPassant)
        {
            capturedPiece = _squares[move.ToFile, move.FromRank];
        }

        _history.Push(new GameState(
            capturedPiece,
            WhiteCanCastleKingSide, WhiteCanCastleQueenSide,
            BlackCanCastleKingSide, BlackCanCastleQueenSide,
            EnPassantFile, EnPassantRank, HalfmoveClock
        ));

        if (move.Flag == MoveFlag.Promotion)
        {
            piece = new Piece(move.PromotionType, piece.Color);
        }

        _squares[move.ToFile, move.ToRank] = piece;
        _squares[move.FromFile, move.FromRank] = Piece.Empty;

        if (move.Flag == MoveFlag.EnPassant)
        {
            _squares[move.ToFile, move.FromRank] = Piece.Empty;
        }
        else if (move.Flag == MoveFlag.Castling)
        {
            bool isKingSide = move.ToFile == 6;
            int rookFromFile = isKingSide ? 7 : 0;
            int rookToFile = isKingSide ? 5 : 3;
            int rank = move.ToRank;
            
            _squares[rookToFile, rank] = _squares[rookFromFile, rank];
            _squares[rookFromFile, rank] = Piece.Empty;
        }

        UpdateCastlingRights(move, piece, capturedPiece);

        EnPassantFile = -1;
        EnPassantRank = -1;

        if (piece.Type == PieceType.Pawn && Math.Abs(move.ToRank - move.FromRank) == 2)
        {
            EnPassantFile = move.FromFile;
            EnPassantRank = (move.FromRank + move.ToRank) / 2;
        }

        if (piece.Type == PieceType.Pawn || capturedPiece.Type != PieceType.None)
        {
            HalfmoveClock = 0;
        }
        else
        {
            HalfmoveClock++;
        }

        if (ActiveColor == PieceColor.Black)
        {
            FullmoveNumber++;
        }

        ActiveColor = ActiveColor == PieceColor.White ? PieceColor.Black : PieceColor.White;

        // Add to history
        _positionHistory.Add(GetPositionKey());
    }

    private void UpdateCastlingRights(Move move, Piece movedPiece, Piece capturedPiece)
    {
        if (movedPiece.Type == PieceType.King)
        {
            if (movedPiece.Color == PieceColor.White)
            {
                WhiteCanCastleKingSide = false;
                WhiteCanCastleQueenSide = false;
            }
            else
            {
                BlackCanCastleKingSide = false;
                BlackCanCastleQueenSide = false;
            }
        }
        CheckRookSquare(move.FromFile, move.FromRank);
        CheckRookSquare(move.ToFile, move.ToRank);
    }

    private void CheckRookSquare(int file, int rank)
    {
        if (rank == 0)
        {
            if (file == 7) WhiteCanCastleKingSide = false;
            if (file == 0) WhiteCanCastleQueenSide = false;
        }
        else if (rank == 7)
        {
            if (file == 7) BlackCanCastleKingSide = false;
            if (file == 0) BlackCanCastleQueenSide = false;
        }
    }

    public void UndoMove(Move move)
    {
        if (_history.Count == 0) return;
        GameState state = _history.Pop();
        Piece movedPiece = _squares[move.ToFile, move.ToRank];

        // Remove from history
        _positionHistory.RemoveAt(_positionHistory.Count - 1);

        if (move.Flag == MoveFlag.Promotion)
        {
            _squares[move.FromFile, move.FromRank] = new Piece(PieceType.Pawn, movedPiece.Color);
        }
        else
        {
            _squares[move.FromFile, move.FromRank] = movedPiece;
        }

        _squares[move.ToFile, move.ToRank] = state.CapturedPiece;

        if (move.Flag == MoveFlag.EnPassant)
        {
            _squares[move.ToFile, move.FromRank] = state.CapturedPiece;
            _squares[move.ToFile, move.ToRank] = Piece.Empty;
        }
        else if (move.Flag == MoveFlag.Castling)
        {
            bool isKingSide = move.ToFile == 6;
            int rookFromFile = isKingSide ? 7 : 0;
            int rookToFile = isKingSide ? 5 : 3;
            int rank = move.ToRank;
            
            _squares[rookFromFile, rank] = _squares[rookToFile, rank];
            _squares[rookToFile, rank] = Piece.Empty;
        }

        WhiteCanCastleKingSide = state.WhiteCanCastleKingSide;
        WhiteCanCastleQueenSide = state.WhiteCanCastleQueenSide;
        BlackCanCastleKingSide = state.BlackCanCastleKingSide;
        BlackCanCastleQueenSide = state.BlackCanCastleQueenSide;
        EnPassantFile = state.EnPassantFile;
        EnPassantRank = state.EnPassantRank;
        HalfmoveClock = state.HalfmoveClock;

        ActiveColor = ActiveColor == PieceColor.White ? PieceColor.Black : PieceColor.White;

        if (ActiveColor == PieceColor.Black)
        {
            FullmoveNumber--;
        }
    }

    private void ParsePiecePlacement(string placement)
    {
        for (int i = 0; i < 8; i++)
            for (int j = 0; j < 8; j++)
                _squares[i, j] = Piece.Empty;

        string[] ranks = placement.Split('/');
        for (int rank = 0; rank < 8; rank++)
        {
            int file = 0;
            string rankStr = ranks[7 - rank];

            foreach (char c in rankStr)
            {
                if (char.IsDigit(c))
                {
                    file += (int)char.GetNumericValue(c);
                }
                else
                {
                    _squares[file, rank] = CharToPiece(c);
                    file++;
                }
            }
        }
    }

    private Piece CharToPiece(char c)
    {
        PieceColor color = char.IsUpper(c) ? PieceColor.White : PieceColor.Black;
        PieceType type = char.ToLower(c) switch
        {
            'p' => PieceType.Pawn,
            'n' => PieceType.Knight,
            'b' => PieceType.Bishop,
            'r' => PieceType.Rook,
            'q' => PieceType.Queen,
            'k' => PieceType.King,
            _ => PieceType.None
        };
        return new Piece(type, color);
    }

    public void Print()
    {
        for (int rank = 7; rank >= 0; rank--)
        {
            Console.Write($"{rank + 1} ");
            for (int file = 0; file < 8; file++)
            {
                Console.Write($"{_squares[file, rank]} ");
            }
            Console.WriteLine();
        }
        Console.WriteLine("  a b c d e f g h");
        Console.WriteLine($"Turn: {ActiveColor}, Fullmove: {FullmoveNumber}, EP: {(EnPassantFile == -1 ? "-" : $"{(char)('a' + EnPassantFile)}{EnPassantRank + 1}")}");
        Console.WriteLine($"Castling: {(WhiteCanCastleKingSide ? "K" : "")}{(WhiteCanCastleQueenSide ? "Q" : "")}{(BlackCanCastleKingSide ? "k" : "")}{(BlackCanCastleQueenSide ? "q" : "")}");
    }
}
