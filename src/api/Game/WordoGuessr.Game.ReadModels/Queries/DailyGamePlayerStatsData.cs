namespace WordoGuessr.Game.ReadModels.Queries;

public sealed record DailyGamePlayerStatsData(
    int Score,
    int AttemptsCount,
    TimeSpan Duration,
    int OtherPlaysCount,
    int PlayersWithWorseScore,
    int PlayersWithMoreAttempts,
    int PlayersWithWorseTime);
