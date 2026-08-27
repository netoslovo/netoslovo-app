namespace WordoGuessr.Game.ReadModels.Queries;

public sealed record DailyGameStreakTopEntry(Guid PlayerId, int Place, int Streak);