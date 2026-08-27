using WordoGuessr.Common.Domain;
using WordoGuessr.Common.Domain.ValueObjects;

namespace WordoGuessr.Game.Domain;

public sealed class Guess : DomainEntity<long>
{
    public Word Word { get; } = null!;
    public int Distance { get; private set; }
    public DateTimeOffset CreatedAt { get; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public GuessSource Source { get; }

    internal Guess(GuessAttempt attempt)
    {
        Word = attempt.Word;
        Distance = attempt.Distance;
        CreatedAt = attempt.CreatedAt;
        UpdatedAt = attempt.CreatedAt;
        Source = attempt.Source;
    }

    private Guess() { }

    public void UpdateDistance(int distance, DateTimeOffset at)
    {
        Distance = distance;
        UpdatedAt = at;
    }
}
