using WordoGuessr.Game.Domain;
using WordoGuessr.Game.ReadModels.Queries;
using WordoGuessr.Game.ReadModels.Stored;

namespace WordoGuessr.Game.App.Abstractions;

public interface IGameReadModelStore
{
    Task AddGameResult(SingleGameResult result, CancellationToken ct = default);

    Task<DailyGamePlayerStatsData?> GetDailyGamePlayerStats(Guid playerId, DateOnly day, CancellationToken ct = default);
    Task<DailyGameStatsData?> GetDailyGameStats(DateOnly day, CancellationToken ct = default);
    Task<GlobalPlayerStatsData?> GetGlobalPlayerStats(Guid playerId, CancellationToken ct = default);

    Task RecordDailyGameStreakSuccess(Guid playerId, DateOnly today, CancellationToken ct = default);
    Task<int> GetDailyGameCurrentPlayerStreakValue(Guid playerId, DateOnly today, CancellationToken ct = default);
    Task<DailyGameStreakTop> GetDailyGameCurrentStreakTop(Guid playerId, DateOnly today, int topN, CancellationToken ct = default);
    Task<DailyGameStreakTop> GetDailyGameLongestStreakTop(Guid playerId, int topN, CancellationToken ct = default);

    Task RecordSuccessfulArcadeGame(
        Guid playerId,
        Difficulty difficulty,
        int score,
        int attemptsCount,
        TimeSpan duration,
        CancellationToken ct = default);

    Task<ArcadeGameTopPlayers> GetArcadeGameTopPlayers(Guid playerId, Difficulty difficulty, int topN, CancellationToken ct = default);
}
