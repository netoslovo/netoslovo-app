namespace WordoGuessr.Game.Dto;

public sealed record DailyGameStatsDto(
    int MedianScore,
    int MedianAttempts,
    TimeSpan MedianDuration
);