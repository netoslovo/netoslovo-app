namespace WordoGuessr.Game.ReadModels.Queries;

public sealed class DailyGameStreakTopCurrentPlayerEntry
{
    public Guid PlayerId { get; }
    public int? Place { get; }
    public int Streak { get; }

    private DailyGameStreakTopCurrentPlayerEntry()
    {
    }

    private DailyGameStreakTopCurrentPlayerEntry(Guid playerId, int? place, int streak)
    {
        PlayerId = playerId;
        Place = place;
        Streak = streak;
    }

    public static DailyGameStreakTopCurrentPlayerEntry Ranked(Guid playerId, int place, int streak) =>
        new DailyGameStreakTopCurrentPlayerEntry(playerId, place, streak);

    public static DailyGameStreakTopCurrentPlayerEntry NotRanked(Guid playerId) =>
        new DailyGameStreakTopCurrentPlayerEntry(playerId, null, 0);
}
