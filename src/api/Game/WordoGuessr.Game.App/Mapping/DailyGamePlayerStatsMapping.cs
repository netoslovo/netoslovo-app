using WordoGuessr.Game.Dto;
using WordoGuessr.Game.ReadModels.Queries;

namespace WordoGuessr.Game.App.Mapping;

internal static class DailyGamePlayerStatsMapping
{
    public static DailyGamePlayerStatsDto MapToDto(this DailyGamePlayerStatsData data)
    {
        const int percentIfNoOtherGames = 100;
        var anyOtherPlays = data.OtherPlaysCount > 0;

        var scoreBetterThan = anyOtherPlays
            ? (int)(100.0 * data.PlayersWithWorseScore / data.OtherPlaysCount)
            : percentIfNoOtherGames;

        var attemptsCountBetterThan = anyOtherPlays
            ? (int)(100.0 * data.PlayersWithMoreAttempts / data.OtherPlaysCount)
            : percentIfNoOtherGames;

        var durationBetterThan = anyOtherPlays
            ? (int)(100.0 * data.PlayersWithWorseTime / data.OtherPlaysCount)
            : percentIfNoOtherGames;

        return new DailyGamePlayerStatsDto(
            Score: data.Score,
            AttemptsCount: data.AttemptsCount,
            Duration: data.Duration,
            ScoreBetterThanPercent: scoreBetterThan,
            AttemptsCountBetterThanPercent: attemptsCountBetterThan,
            DurationBetterThanPercent: durationBetterThan);
    }
}
