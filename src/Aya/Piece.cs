namespace Aya;

public struct Piece
{
    public PieceType Type { get; }
    public PieceColor Color { get; }

    public Piece(PieceType type, PieceColor color)
    {
        Type = type;
        Color = color;
    }

    public static Piece Empty => new Piece(PieceType.None, PieceColor.None);

    public override string ToString()
    {
        if (Type == PieceType.None) return ".";

        char c = Type switch
        {
            PieceType.Pawn => 'P',
            PieceType.Knight => 'N',
            PieceType.Bishop => 'B',
            PieceType.Rook => 'R',
            PieceType.Queen => 'Q',
            PieceType.King => 'K',
            _ => '?'
        };

        return Color == PieceColor.White ? c.ToString() : char.ToLower(c).ToString();
    }
}
