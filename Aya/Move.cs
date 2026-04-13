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

    public Move(int fromFile, int fromRank, int toFile, int toRank, MoveFlag flag = MoveFlag.None)
    {
        FromFile = fromFile;
        FromRank = fromRank;
        ToFile = toFile;
        ToRank = toRank;
        Flag = flag;
    }

    public override string ToString()
    {
        char fromFileChar = (char)('a' + FromFile);
        char toFileChar = (char)('a' + ToFile);
        return $"{fromFileChar}{FromRank + 1}{toFileChar}{ToRank + 1}";
    }
}
