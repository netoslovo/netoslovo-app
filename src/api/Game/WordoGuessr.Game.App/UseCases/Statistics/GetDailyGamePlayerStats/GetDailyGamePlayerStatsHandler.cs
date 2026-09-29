using WordoGuessr.API.BuildingBlocks.CQRS;
using WordoGuessr.Common.Domain;
using WordoGuessr.Game.App.Abstractions;
using WordoGuessr.Game.App.Mapping;
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

        return Result<DailyGamePlayerStatsDto, GetDailyGamePlayerStatsError>.Success(result.MapToDto());
    }
}
