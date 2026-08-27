using WordoGuessr.API.BuildingBlocks.CQRS;
using WordoGuessr.Common.Domain;
using WordoGuessr.Game.Dto;

namespace WordoGuessr.Game.App.UseCases.Statistics.GetGlobalPlayerStats;

public sealed record GetGlobalPlayerStatsQuery(Guid PlayerId)
    : IQuery<Result<GlobalPlayerStatsDto, GetGlobalPlayerStatsError>>;
