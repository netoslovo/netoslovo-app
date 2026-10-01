using WordoGuessr.Common.Domain.ValueObjects;

namespace WordoGuessr.Game.Domain;

public sealed class DisplayWordView
{
    public IReadOnlyCollection<DisplayWordCellView>? Cells { get; }

    public DisplayWordView(IReadOnlyCollection<DisplayWordCellView>? cells)
    {
        Cells = cells;
    }

    public static DisplayWordView UnknownLength() => new DisplayWordView(null);

    public static DisplayWordView FromWordRevealed(Word word)
    {
        var cells = word.Text
            .Select(w => DisplayWordCellView.CreateRevealed(w))
            .ToArray();

        var view = new DisplayWordView(cells);
        return view;
    }
}

public sealed record DisplayWordCellView(char? Value, bool Revealed)
{
    public static DisplayWordCellView CreateRevealed(char value)
        => new DisplayWordCellView(value, true);
};

public sealed class DisplayWord
{
    private List<DisplayWordCell> _cells = [];
    private readonly string _word = null!;

    public DisplayWord(Word word)
    {
        _word = word.Text;
    }

    private DisplayWord() { }

    public void InitCells()
    {
        if (_cells.Count == 0)
        {
            _cells = _word
                .Select(c => new DisplayWordCell(c))
                .ToList();
        }
    }

    public void RevealAt(int index)
    {
        _cells[index].Reveal();
    }
}

public sealed class DisplayWordCell
{
    public char? Value => Revealed ? _internalValue : null;

    public bool Revealed { get; private set; }

    private readonly char _internalValue;

    private DisplayWordCell() { }

    public DisplayWordCell(char value)
    {
        _internalValue = value;
    }

    public void Reveal()
    {
        if (Revealed) return;
        Revealed = true;
    }

    public DisplayWordCellView ToView() =>
        new DisplayWordCellView(Value, Revealed);
}
