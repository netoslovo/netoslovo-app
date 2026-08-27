using WordoGuessr.API.BuildingBlocks.CQRS;
using WordoGuessr.Common.Domain;
using WordoGuessr.Game.Dto;

namespace WordoGuessr.Game.App.UseCases.Statistics.GetDailyGamePlayerStats;

public sealed record GetDailyGamePlayerStatsQuery(Guid PlayerId, DateOnly Day)
    : IQuery<Result<DailyGamePlayerStatsDto, GetDailyGamePlayerStatsError>>;
