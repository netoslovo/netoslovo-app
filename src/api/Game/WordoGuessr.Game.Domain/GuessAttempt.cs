using WordoGuessr.Common.Domain.ValueObjects;

namespace WordoGuessr.Game.Domain;

public sealed class GuessAttempt
{
    public GuessSource Source { get; }
    public Word Word { get; }
    public int Distance { get; }
    public DateTimeOffset CreatedAt { get; }

    private GuessAttempt(GuessSource source, Word word, int distance, DateTimeOffset createdAt)
    {
        Source = source;
        Word = word;
        Distance = distance;
        CreatedAt = createdAt;
    }

    public static GuessAttempt FromPlayer(Word word, int distance, DateTimeOffset createdAt) =>
        new GuessAttempt(GuessSource.Player, word, distance, createdAt);

    public static GuessAttempt FromHint(Word word, int distance, DateTimeOffset createdAt) =>
        new GuessAttempt(GuessSource.Hint, word, distance, createdAt);
}
