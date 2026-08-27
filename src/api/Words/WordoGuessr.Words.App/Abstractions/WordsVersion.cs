namespace WordoGuessr.Words.App.Abstractions;

public sealed class WordsVersion
{
    public int Version { get; }

    public WordsVersionState State { get; set; }
    public DateTimeOffset CreatedAt { get; }
    public DateTimeOffset? ActivatedAt { get; set; }
    public DateTimeOffset? RetiredAt { get; set; }

    private WordsVersion() { }

    public WordsVersion(int version, DateTimeOffset at)
    {
        Version = version;
        CreatedAt = at;
        State = WordsVersionState.Created;
    }
}
