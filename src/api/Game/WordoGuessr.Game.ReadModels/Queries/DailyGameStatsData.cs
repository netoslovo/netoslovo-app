namespace WordoGuessr.Game.ReadModels.Queries;

public sealed record DailyGameStatsData(
    int TotalPlays,
    int? MedianScore,
    int? MedianAttempts,
    TimeSpan? MedianDuration
);
