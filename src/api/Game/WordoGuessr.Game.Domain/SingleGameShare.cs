using WordoGuessr.Common.Domain;

namespace WordoGuessr.Game.Domain;

public sealed class SingleGameShare : DomainEntity<Guid>
{
    public Guid PublicId { get; }

    public SingleGame SingleGame { get; } = null!;

    public bool ShowGuessWords { get; private set; }

    public DateTimeOffset CreatedAt { get; }

    public DateTimeOffset UpdatedAt { get; private set; }

    public SingleGameShare(
        Guid gameId,
        DateTimeOffset createdAt,
        bool showGuessesWords)
        : base(gameId)
    {
        PublicId = Guid.NewGuid();
        ShowGuessWords = showGuessesWords;
        CreatedAt = createdAt;
        UpdatedAt = createdAt;
    }

    private SingleGameShare() { }

    public void SetWordsVisibility(bool showGuessesWords, DateTimeOffset at)
    {
        ShowGuessWords = showGuessesWords;
        UpdatedAt = at;
    }
}
