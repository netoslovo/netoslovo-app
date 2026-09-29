using WordoGuessr.API.BuildingBlocks.CQRS;
using WordoGuessr.Common.Domain;
using WordoGuessr.Game.App.Abstractions;
using WordoGuessr.Game.Dto;

namespace WordoGuessr.Game.App.UseCases.Statistics.GetDailyGameStats;

internal sealed class GetDailyGameStatsHandler
    : IQueryHandler<GetDailyGameStatsQuery, Result<DailyGameStatsDto, GetDailyGameStatsError>>
{
    private readonly IGameReadModelStore _gameReadStore;

    public GetDailyGameStatsHandler(IGameReadModelStore gameReadStore)
    {
        _gameReadStore = gameReadStore ?? throw new ArgumentNullException(nameof(gameReadStore));
    }

    public async Task<Result<DailyGameStatsDto, GetDailyGameStatsError>> Handle(
        GetDailyGameStatsQuery query,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);

        var data = await _gameReadStore.GetDailyGameStats(query.Day, ct);
        if (data is null)
        {
            return Result<DailyGameStatsDto, GetDailyGameStatsError>.Failure(GetDailyGameStatsError.GameNotFound);
        }

        if (data.TotalPlays < Const.MinValuableStatsCount)
        {
            return Result<DailyGameStatsDto, GetDailyGameStatsError>.Failure(GetDailyGameStatsError.NotEnoughData);
        }

        var result = new DailyGameStatsDto(
            data.MedianScore!.Value,
            data.MedianAttempts!.Value,
            data.MedianDuration!.Value
        );

        return Result<DailyGameStatsDto, GetDailyGameStatsError>.Success(result);
    }
}
