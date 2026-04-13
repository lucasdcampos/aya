namespace Aya;

public class Board
{
    private readonly Piece[,] _squares = new Piece[8, 8];

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
        Console.WriteLine($"Turn: {ActiveColor}, Castling: {(WhiteCanCastleKingSide ? "K" : "")}{(WhiteCanCastleQueenSide ? "Q" : "")}{(BlackCanCastleKingSide ? "k" : "")}{(BlackCanCastleQueenSide ? "q" : "")}");
    }
}
