namespace WordoGuessr.Game.Domain;

public sealed class Hint
{
    public HintType Type { get; }
    public DateTimeOffset UsedAt { get; }

    private Hint(HintType type, DateTimeOffset usedAt)
    {
        Type = type;
        UsedAt = usedAt;
    }

    private Hint() { }

    public static Hint RevealHalfwayWord(DateTimeOffset usedAt) =>
        new Hint(HintType.RevealHalfwayWord, usedAt);

    public static Hint RevealLength(DateTimeOffset usedAt) =>
        new Hint(HintType.RevealLength, usedAt);

    public static Hint RevealLetter(DateTimeOffset usedAt) =>
        new Hint(HintType.RevealLetter, usedAt);
}
