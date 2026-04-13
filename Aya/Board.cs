namespace Aya;

public class Board
{
    private readonly Piece[,] _squares = new Piece[8, 8];

    public Board()
    {
        InitializeStartingPosition();
    }

    public Piece GetPiece(int file, int rank) => _squares[file, rank];

    private void InitializeStartingPosition()
    {
        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                _squares[i, j] = Piece.Empty;
            }
        }

        for (int i = 0; i < 8; i++)
        {
            _squares[i, 1] = new Piece(PieceType.Pawn, PieceColor.White);
            _squares[i, 6] = new Piece(PieceType.Pawn, PieceColor.Black);
        }

        SetupBackRank(0, PieceColor.White);
        SetupBackRank(7, PieceColor.Black);
    }

    private void SetupBackRank(int rank, PieceColor color)
    {
        _squares[0, rank] = new Piece(PieceType.Rook, color);
        _squares[7, rank] = new Piece(PieceType.Rook, color);
        _squares[1, rank] = new Piece(PieceType.Knight, color);
        _squares[6, rank] = new Piece(PieceType.Knight, color);
        _squares[2, rank] = new Piece(PieceType.Bishop, color);
        _squares[5, rank] = new Piece(PieceType.Bishop, color);
        _squares[3, rank] = new Piece(PieceType.Queen, color);
        _squares[4, rank] = new Piece(PieceType.King, color);
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
    }
}
