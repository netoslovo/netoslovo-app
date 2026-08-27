namespace WordoGuessr.Game.ReadModels.Stored;

public sealed class DailyGameStreakInfo
{
    public Guid PlayerId { get; }
    public int CurrentStreak { get; private set; }
    public int LongestStreak { get; private set; }
    public DateOnly LastSuccessDay { get; private set; }

    private DailyGameStreakInfo() { }

    public DailyGameStreakInfo(Guid playerId, DateOnly lastSuccessDay)
    {
        PlayerId = playerId;
        LastSuccessDay = lastSuccessDay;
        CurrentStreak = 1;
        LongestStreak = 1;
    }

    public void RecordStreakSuccess(DateOnly today)
    {
        var alreadyRecorded = LastSuccessDay == today;
        if (alreadyRecorded) return;

        var yesterday = today.AddDays(-1);
        var isStreakActive = LastSuccessDay == yesterday;

        CurrentStreak = isStreakActive
            ? CurrentStreak + 1
            : 1;

        LastSuccessDay = today;

        if (CurrentStreak > LongestStreak)
        {
            LongestStreak = CurrentStreak;
        }
    }

    public int GetCurrentStreak(DateOnly today)
    {
        var yesterday = today.AddDays(-1);
        var currentStreakIsActive = LastSuccessDay == today || LastSuccessDay == yesterday;
        return currentStreakIsActive
            ? CurrentStreak
            : 0;
    }
}
