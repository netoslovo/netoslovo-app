namespace WordoGuessr.Game.ReadModels.Queries;

public sealed record DailyGameStreakTop(
    IReadOnlyList<DailyGameStreakTopEntry> Top,
    DailyGameStreakTopCurrentPlayerEntry PlayerStreakTopInfo);
