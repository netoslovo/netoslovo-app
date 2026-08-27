namespace WordoGuessr.Words.App.Abstractions;

public sealed class WordIndex
{
    public int Version { get; }
    public int WordId { get; }
    public string WordText { get; } = null!;

    public WordIndex(int version, int wordId, string wordText)
    {
        Version = version;
        WordId = wordId;
        WordText = wordText;
    }

    private WordIndex() { }
}
