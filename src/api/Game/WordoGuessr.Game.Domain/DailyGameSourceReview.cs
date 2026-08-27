namespace WordoGuessr.Game.Domain;

public sealed class DailyGameSourceReview
{
    public long GameSourceId { get; }
    public int WordsVersion { get; private set; }
    public VersionedGameSource VersionedGameSource { get; } = null!;
    public ApprovedDailyGameSource? ApprovedSource { get; private set; }
    public DateTimeOffset CreatedAt { get; }
    public DateTimeOffset UpdatedAt { get; private set; }

    public DailyGameSourceReview(long gameSourceId, int wordsVersion, DateTimeOffset createdAt)
    {
        GameSourceId = gameSourceId;
        WordsVersion = wordsVersion;
        CreatedAt = createdAt;
        UpdatedAt = createdAt;
    }

    private DailyGameSourceReview() { }

    public ApprovedDailyGameSource? Approve(DateTimeOffset at)
    {
        UpdatedAt = at;
        if (ApprovedSource is not null)
        {
            return null;
        }

        ApprovedSource = new ApprovedDailyGameSource(GameSourceId);
        return ApprovedSource;
    }

    public ApprovedDailyGameSource? Reject(DateTimeOffset at)
    {
        UpdatedAt = at;
        if (ApprovedSource is null)
        {
            return null;
        }

        var toRemove = ApprovedSource;
        ApprovedSource = null;
        return toRemove;
    }
}
