using WordoGuessr.Common.Domain;

namespace WordoGuessr.Game.Domain;

public sealed class DailyGameShare : DomainEntity<Guid>
{
    public Guid PublicId { get; }

    public SingleGame SingleGame { get; } = null!;

    public DateTimeOffset CreatedAt { get; }

    public DailyGameShare(
        Guid gameId,
        DateTimeOffset createdAt)
        : base(gameId)
    {
        PublicId = Guid.NewGuid();
        CreatedAt = createdAt;
    }

    private DailyGameShare() { }
}
