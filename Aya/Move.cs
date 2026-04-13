namespace Aya;

public enum MoveFlag
{
    None,
    EnPassant,
    Castling,
    Promotion
}

public struct Move
{
    public int FromFile { get; }
    public int FromRank { get; }
    public int ToFile { get; }
    public int ToRank { get; }
    public MoveFlag Flag { get; }
    public PieceType PromotionType { get; }

    public Move(int fromFile, int fromRank, int toFile, int toRank, MoveFlag flag = MoveFlag.None, PieceType promotionType = PieceType.None)
    {
        FromFile = fromFile;
        FromRank = fromRank;
        ToFile = toFile;
        ToRank = toRank;
        Flag = flag;
        PromotionType = promotionType;
    }

    public override string ToString()
    {
        char fromFileChar = (char)('a' + FromFile);
        char toFileChar = (char)('a' + ToFile);
        string promotion = PromotionType != PieceType.None ? char.ToLower(PromotionType.ToString()[0]).ToString() : "";
        return $"{fromFileChar}{FromRank + 1}{toFileChar}{ToRank + 1}{promotion}";
    }
}
