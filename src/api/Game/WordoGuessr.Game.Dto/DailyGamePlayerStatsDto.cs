namespace WordoGuessr.Game.Dto;

public sealed record DailyGamePlayerStatsDto(
    int Score,
    int AttemptsCount,
    TimeSpan Duration,
    int ScoreBetterThanPercent,
    int AttemptsCountBetterThanPercent,
    int DurationBetterThanPercent
);