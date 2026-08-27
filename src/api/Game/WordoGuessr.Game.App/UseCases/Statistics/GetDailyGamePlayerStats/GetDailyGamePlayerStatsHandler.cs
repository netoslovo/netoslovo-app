using WordoGuessr.API.BuildingBlocks.CQRS;
using WordoGuessr.Common.Domain;
using WordoGuessr.Game.App.Abstractions;
using WordoGuessr.Game.Dto;

namespace WordoGuessr.Game.App.UseCases.Statistics.GetDailyGamePlayerStats;

internal sealed class GetDailyGamePlayerStatsHandler
    : IQueryHandler<GetDailyGamePlayerStatsQuery, Result<DailyGamePlayerStatsDto, GetDailyGamePlayerStatsError>>
{
    private readonly IGameReadModelStore _gameReadStore;

    public GetDailyGamePlayerStatsHandler(IGameReadModelStore gameReadStore)
    {
        _gameReadStore = gameReadStore ?? throw new ArgumentNullException(nameof(gameReadStore));
    }

    public async Task<Result<DailyGamePlayerStatsDto, GetDailyGamePlayerStatsError>> Handle(
        GetDailyGamePlayerStatsQuery query,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);

        var result = await _gameReadStore.GetDailyGamePlayerStats(query.PlayerId, query.Day, ct);

        if (result is null)
        {
            return Result<DailyGamePlayerStatsDto, GetDailyGamePlayerStatsError>
                .Failure(GetDailyGamePlayerStatsError.GameNotFound);
        }

        var anyOtherPlays = result.OtherPlaysCount > 0;
        var percentIfNoAnyOtherGames = 100;

        var scoreBetterThan = anyOtherPlays
            ? (int)(100.0 * result.PlayersWithWorseScore / result.OtherPlaysCount)
            : percentIfNoAnyOtherGames;

        var attemptsCountBetterThan = anyOtherPlays
            ? (int)(100.0 * result.PlayersWithMoreAttempts / result.OtherPlaysCount)
            : percentIfNoAnyOtherGames;

        var durationBetterThan = anyOtherPlays
            ? (int)(100.0 * result.PlayersWithWorseTime / result.OtherPlaysCount)
            : percentIfNoAnyOtherGames;

        var resultDto = new DailyGamePlayerStatsDto(
            Score: result.Score,
            AttemptsCount: result.AttemptsCount,
            Duration: result.Duration,
            ScoreBetterThanPercent: scoreBetterThan,
            AttemptsCountBetterThanPercent: attemptsCountBetterThan,
            DurationBetterThanPercent: durationBetterThan
        );

        return Result<DailyGamePlayerStatsDto, GetDailyGamePlayerStatsError>.Success(resultDto);
    }
}
