namespace WordoGuessr.Game.Domain;

public sealed class ApprovedDailyGameSource
{
    public long GameSourceId { get; }
    public DailyGameSourceReview Review { get; } = null!;
    public SingleGameDailySchedule? Schedule { get; } = null!;

    public ApprovedDailyGameSource(long gameSourceId)
    {
        GameSourceId = gameSourceId;
    }

    private ApprovedDailyGameSource() { }
}
