namespace WordoGuessr.Game.Domain;

public sealed class DisplayWord
{
    public IReadOnlyCollection<DisplayWordCell>? Cells { get; }

    public DisplayWord(IReadOnlyCollection<DisplayWordCell>? cells)
    {
        Cells = cells;
    }

    public static DisplayWord UnknownLength() => new DisplayWord(null);
}

public sealed record DisplayWordCell(char? Value, bool Revealed); 