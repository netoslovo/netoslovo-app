using Microsoft.EntityFrameworkCore;
using WordoGuessr.Common.App.Exceptions.Persistence;
using WordoGuessr.Game.App.Abstractions;
using WordoGuessr.Game.Domain;
using WordoGuessr.Game.ReadModels.Queries;
using WordoGuessr.Game.ReadModels.Stored;

namespace WordoGuessr.Game.Infra;

public sealed class EfGameReadStore : IGameReadModelStore
{
    private readonly GameDbContext _gameDbContext;

    public EfGameReadStore(GameDbContext gameDbContext)
    {
        _gameDbContext = gameDbContext ?? throw new ArgumentNullException(nameof(gameDbContext));
    }

    public Task AddGameResult(SingleGameResult result, CancellationToken ct = default)
    {
        _gameDbContext.SingleGameResults.Add(result);
        return _gameDbContext.SaveChangesAsync(ct);
    }

    // TODO: raw sql?
    public async Task<DailyGamePlayerStatsData?> GetDailyGamePlayerStats(
        Guid playerId,
        DateOnly day,
        CancellationToken ct = default)
    {
        var result = await _gameDbContext.SingleGameResults
            .AsNoTracking()
            .Where(sg =>
                sg.PlayerId == playerId &&
                sg.Mode == SingleGameMode.Daily &&
                sg.State == SingleGameStateCode.Guessed &&
                sg.DayOfDailyGame == day)
            .Select(gameResult => new DailyGamePlayerStatsData
            (
                Score: gameResult.Score,
                AttemptsCount: gameResult.Attempts,
                Duration: gameResult.Duration,
                OtherPlaysCount: _gameDbContext.SingleGameResults.Count(other =>
                    other.PlayerId != gameResult.PlayerId &&
                    other.Mode == SingleGameMode.Daily &&
                    other.State == SingleGameStateCode.Guessed &&
                    other.DayOfDailyGame == day),

                PlayersWithWorseScore: _gameDbContext.SingleGameResults.Count(other =>
                    other.PlayerId != gameResult.PlayerId &&
                    other.Mode == SingleGameMode.Daily &&
                    other.State == SingleGameStateCode.Guessed &&
                    other.DayOfDailyGame == day &&
                    other.Score > gameResult.Score),

                PlayersWithMoreAttempts: _gameDbContext.SingleGameResults.Count(other =>
                    other.PlayerId != gameResult.PlayerId &&
                    other.Mode == SingleGameMode.Daily &&
                    other.State == SingleGameStateCode.Guessed &&
                    other.DayOfDailyGame == day &&
                    other.Attempts > gameResult.Attempts),

                PlayersWithWorseTime: _gameDbContext.SingleGameResults.Count(other =>
                    other.PlayerId != gameResult.PlayerId &&
                    other.Mode == SingleGameMode.Daily &&
                    other.State == SingleGameStateCode.Guessed &&
                    other.DayOfDailyGame == day &&
                    other.Duration > gameResult.Duration)
            ))
            .FirstOrDefaultAsync(ct);

        return result;
    }

    public async Task<DailyGameStatsData?> GetDailyGameStats(DateOnly day, CancellationToken ct = default)
    {
        var result = await _gameDbContext.Database
            .SqlQuery<DailyGameStatsData>(
            $"""
            SELECT
                count(*) as "TotalPlays",

                percentile_disc(0.5) WITHIN GROUP (
                ORDER BY score) as "MedianScore",

                percentile_disc(0.5) WITHIN GROUP (
                ORDER BY attempts) as "MedianAttempts",

                percentile_cont(0.5) WITHIN GROUP (
                ORDER BY duration) AS "MedianDuration"

            FROM game.single_game_results sgr
            WHERE sgr.mode = 1 AND sgr.state = 2 AND sgr.day_of_daily_game = {day}
            """)
            .SingleOrDefaultAsync(ct);

        return result;
    }

    public Task<GlobalPlayerStatsData?> GetGlobalPlayerStats(Guid playerId, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public async Task RecordDailyGameStreakSuccess(Guid playerId, DateOnly today, CancellationToken ct = default)
    {
        var dailyGameStreakInfo = await _gameDbContext.DailyGameStreaksInfo.SingleOrDefaultAsync(
            si => si.PlayerId == playerId,
            ct);

        if (dailyGameStreakInfo is null)
        {
            dailyGameStreakInfo = new DailyGameStreakInfo(playerId, today);
            _gameDbContext.DailyGameStreaksInfo.Add(dailyGameStreakInfo);
            try
            {
                await _gameDbContext.SaveChangesAsync(ct);
            }
            catch (UniqueConstraintViolationException)
            {
                _gameDbContext.Entry(dailyGameStreakInfo).State = EntityState.Detached;
                dailyGameStreakInfo = await _gameDbContext.DailyGameStreaksInfo.SingleAsync(
                    si => si.PlayerId == playerId,
                    ct);

                dailyGameStreakInfo.RecordStreakSuccess(today);
                await _gameDbContext.SaveChangesAsync(ct);
            }
        }
        else
        {
            dailyGameStreakInfo.RecordStreakSuccess(today);
            await _gameDbContext.SaveChangesAsync(ct);
        }
    }

    public async Task<int> GetDailyGameCurrentPlayerStreakValue(Guid playerId, DateOnly today, CancellationToken ct = default)
    {
        var dailyGameStreakInfo = await _gameDbContext.DailyGameStreaksInfo.SingleOrDefaultAsync(
            si => si.PlayerId == playerId,
            ct);

        if (dailyGameStreakInfo is null) return 0;
        return dailyGameStreakInfo.GetCurrentStreak(today);
    }

    public async Task<DailyGameStreakTop> GetDailyGameCurrentStreakTop(Guid playerId, DateOnly today, int topN, CancellationToken ct = default)
    {
        var yesterday = today.AddDays(-1);
        var rows = await _gameDbContext.Database.SqlQuery<DailyGameStreakTopRow>(
            $"""
            WITH player_row AS (
                SELECT
                    player_id AS "PlayerId",
                    current_streak AS "Streak"
                FROM game.daily_game_streaks_info
                WHERE current_streak <> 0
                    AND last_success_day IN ({today}, {yesterday})
                    AND player_id = {playerId}
                LIMIT 1
            ),
            top_rows AS (
                SELECT
                    player_id AS "PlayerId",
                    current_streak AS "Streak"
                FROM game.daily_game_streaks_info
                WHERE current_streak <> 0
                    AND last_success_day IN ({today}, {yesterday})
                ORDER BY current_streak DESC, player_id
                LIMIT {topN}
            )
            SELECT
                false AS "IsCurrentPlayer",
                top_rows."PlayerId",
                row_number() OVER (
                    ORDER BY top_rows."Streak" DESC, top_rows."PlayerId"
                )::int AS "Place",
                top_rows."Streak"
            FROM top_rows

            UNION ALL

            SELECT
                true AS "IsCurrentPlayer",
                player_row."PlayerId",
                (
                    SELECT 1 + count(*)
                    FROM game.daily_game_streaks_info AS another_streak
                    WHERE another_streak.current_streak <> 0
                        AND another_streak.last_success_day IN ({today}, {yesterday})
                        AND (
                            another_streak.current_streak > player_row."Streak"
                            OR (
                                another_streak.current_streak = player_row."Streak"
                                AND another_streak.player_id < player_row."PlayerId"
                            )
                        )
                )::int AS "Place",
                player_row."Streak"
            FROM player_row

            ORDER BY "IsCurrentPlayer", "Place"
            """)
            .ToArrayAsync(ct);

        var result = MapStreakTopRows(rows, playerId);
        return result;
    }

    public async Task<DailyGameStreakTop> GetDailyGameLongestStreakTop(Guid playerId, int topN, CancellationToken ct = default)
    {
        var rows = await _gameDbContext.Database.SqlQuery<DailyGameStreakTopRow>(
            $"""
            WITH player_row AS (
                SELECT
                    player_id AS "PlayerId",
                    longest_streak AS "Streak"
                FROM game.daily_game_streaks_info
                WHERE longest_streak <> 0
                    AND player_id = {playerId}
                LIMIT 1
            ),
            top_rows AS (
                SELECT
                    player_id AS "PlayerId",
                    longest_streak AS "Streak"
                FROM game.daily_game_streaks_info
                WHERE longest_streak <> 0
                ORDER BY longest_streak DESC, player_id
                LIMIT {topN}
            )
            SELECT
                false AS "IsCurrentPlayer",
                top_rows."PlayerId",
                row_number() OVER (
                    ORDER BY top_rows."Streak" DESC, top_rows."PlayerId"
                )::int AS "Place",
                top_rows."Streak"
            FROM top_rows

            UNION ALL

            SELECT
                true AS "IsCurrentPlayer",
                player_row."PlayerId",
                (
                    SELECT 1 + count(*)
                    FROM game.daily_game_streaks_info AS another_streak
                    WHERE another_streak.longest_streak <> 0
                        AND (
                            another_streak.longest_streak > player_row."Streak"
                            OR (
                                another_streak.longest_streak = player_row."Streak"
                                AND another_streak.player_id < player_row."PlayerId"
                            )
                        )
                )::int AS "Place",
                player_row."Streak"
            FROM player_row

            ORDER BY "IsCurrentPlayer", "Place"
            """)
            .ToArrayAsync(ct);

        var result = MapStreakTopRows(rows, playerId);
        return result;
    }

    public async Task RecordSuccessfulArcadeGame(
        Guid playerId,
        Difficulty difficulty,
        int score,
        int attemptsCount,
        TimeSpan duration,
        CancellationToken ct = default)
    {
        var arcadeGamePlayerStats = await _gameDbContext.ArcadeGameStats
            .SingleOrDefaultAsync(
                x => x.PlayerId == playerId && x.Difficulty == difficulty,
                ct);

        if (arcadeGamePlayerStats is null)
        {
            arcadeGamePlayerStats = new ArcadeGameStats(playerId, difficulty);
            arcadeGamePlayerStats.RecordSuccessfulGame(score, attemptsCount, duration);
            _gameDbContext.ArcadeGameStats.Add(arcadeGamePlayerStats);

            try
            {
                await _gameDbContext.SaveChangesAsync(ct);
            }
            catch (UniqueConstraintViolationException)
            {
                _gameDbContext.Entry(arcadeGamePlayerStats).State = EntityState.Detached;
                arcadeGamePlayerStats = await _gameDbContext.ArcadeGameStats
                    .SingleAsync(
                        x => x.PlayerId == playerId && x.Difficulty == difficulty,
                        ct);

                arcadeGamePlayerStats.RecordSuccessfulGame(score, attemptsCount, duration);
                await _gameDbContext.SaveChangesAsync(ct);
            }
        }
        else
        {
            arcadeGamePlayerStats.RecordSuccessfulGame(score, attemptsCount, duration);
            await _gameDbContext.SaveChangesAsync(ct);
        }
    }

    public async Task<ArcadeGameTopPlayers> GetArcadeGameTopPlayers(Guid playerId, Difficulty difficulty, int topN, CancellationToken ct = default)
    {
        var rows = await _gameDbContext.Database.SqlQuery<ArcadeGameStatsRow>(
            $"""
            WITH player_row AS (
                SELECT 
                    player_id as "PlayerId",
                    guessed_games as "GuessedGames",
                    average_score as "AverageScore",
                    average_duration as "AverageDuration"
                FROM game.arcade_game_stats
                WHERE player_id = {playerId}
                    AND difficulty_code = {difficulty.Code}
                    AND guessed_games <> 0
                LIMIT 1
                ),
            top_rows as (
                SELECT 
                    player_id as "PlayerId",
                    guessed_games as "GuessedGames",
                    average_score as "AverageScore",
                    average_duration as "AverageDuration"
                FROM game.arcade_game_stats
                WHERE difficulty_code = {difficulty.Code}
                    AND guessed_games <> 0
                ORDER BY 
                    guessed_games DESC,
                    average_score ASC,
                    average_duration ASC,
                    player_id ASC
                LIMIT {topN}
                )
            SELECT
                false as "IsCurrentPlayer",
                top_rows."PlayerId",
                row_number() OVER(
                    ORDER BY 
                        top_rows."GuessedGames" DESC,
                        top_rows."AverageScore" ASC,
                        top_rows."AverageDuration" ASC,
                        top_rows."PlayerId" ASC
                )::int as "Place",
                top_rows."GuessedGames",
                top_rows."AverageScore",
                top_rows."AverageDuration"
            FROM top_rows

            UNION ALL

            SELECT
                true as "IsCurrentPlayer",
                player_row."PlayerId",
                (
                    SELECT 1 + count(*)
                    FROM game.arcade_game_stats AS another_stats
                    WHERE
                        another_stats.difficulty_code = {difficulty.Code}
                        AND (
                            another_stats.guessed_games > player_row."GuessedGames"
                            OR (
                                another_stats.guessed_games = player_row."GuessedGames"
                                AND another_stats.average_score < player_row."AverageScore"
                            )
                            OR (
                                another_stats.guessed_games = player_row."GuessedGames"
                                AND another_stats.average_score = player_row."AverageScore"
                                AND another_stats.average_duration < player_row."AverageDuration"
                            )
                            OR (
                                another_stats.guessed_games = player_row."GuessedGames"
                                AND another_stats.average_score = player_row."AverageScore"
                                AND another_stats.average_duration = player_row."AverageDuration"
                                AND another_stats.player_id < player_row."PlayerId"
                            )
                        )
                )::int as "Place",
                player_row."GuessedGames",
                player_row."AverageScore",
                player_row."AverageDuration"
            FROM player_row

            ORDER BY "IsCurrentPlayer", "Place"
            """)
            .ToArrayAsync(ct);

        var result = MapArcadeGameStatsTopRows(rows, playerId);
        return result;
    }

    private static DailyGameStreakTop MapStreakTopRows(DailyGameStreakTopRow[] rows, Guid playerId)
    {
        var top = rows
            .Where(row => !row.IsCurrentPlayer)
            .Select(row => new DailyGameStreakTopEntry(row.PlayerId, row.Place, row.Streak))
            .ToArray();

        var playerRow = rows.SingleOrDefault(row => row.IsCurrentPlayer);
        var playerStreakTopInfo = playerRow is null
            ? DailyGameStreakTopCurrentPlayerEntry.NotRanked(playerId)
            : DailyGameStreakTopCurrentPlayerEntry.Ranked(
                playerRow.PlayerId,
                playerRow.Place,
                playerRow.Streak);

        return new DailyGameStreakTop(top, playerStreakTopInfo);
    }

    private ArcadeGameTopPlayers MapArcadeGameStatsTopRows(ArcadeGameStatsRow[] rows, Guid playerId)
    {
        var top = rows
            .Where(row => !row.IsCurrentPlayer)
            .Select(GetArcadeGameTopEntryFromRow)
            .ToArray();

        var playerRow = rows.SingleOrDefault(row => row.IsCurrentPlayer);
        var playerTopInfo = playerRow is null || playerRow.GuessedGames == 0
            ? ArcadeGameTopPlayersCurrentPlayerEntry.NotRanked(playerId)
            : ArcadeGameTopPlayersCurrentPlayerEntry.Ranked(
                playerRow.PlayerId,
                playerRow.Place,
                playerRow.GuessedGames,
                playerRow.AverageScore,
                playerRow.AverageDuration);

        return new ArcadeGameTopPlayers(top, playerTopInfo);
    }

    private static ArcadeGameTopPlayersEntry GetArcadeGameTopEntryFromRow(ArcadeGameStatsRow row) =>
        new ArcadeGameTopPlayersEntry(
                PlayerId: row.PlayerId,
                Place: row.Place,
                GuessedGames: row.GuessedGames,
                AverageScore: row.AverageScore,
                AverageDuration: row.AverageDuration);

    private sealed record DailyGameStreakTopRow(
        bool IsCurrentPlayer,
        Guid PlayerId,
        int Place,
        int Streak);

    private sealed record ArcadeGameStatsRow(
        bool IsCurrentPlayer,
        Guid PlayerId,
        int Place,
        int GuessedGames,
        decimal AverageScore,
        TimeSpan AverageDuration
    );
}
