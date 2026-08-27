using WordoGuessr.API.BuildingBlocks.CQRS;
using WordoGuessr.Common.Domain;
using WordoGuessr.Game.Dto;

namespace WordoGuessr.Game.App.UseCases.Statistics.GetGlobalPlayerStats;

internal sealed class GetGlobalPlayerStatsHandler
    : IQueryHandler<GetGlobalPlayerStatsQuery, Result<GlobalPlayerStatsDto, GetGlobalPlayerStatsError>>
{
    public Task<Result<GlobalPlayerStatsDto, GetGlobalPlayerStatsError>> Handle(
        GetGlobalPlayerStatsQuery query,
        CancellationToken ct)
    {
        throw new NotImplementedException();
    }
}
