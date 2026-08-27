using WordoGuessr.API.BuildingBlocks.CQRS;
using WordoGuessr.Common.Domain;
using WordoGuessr.Game.Dto;

namespace WordoGuessr.Game.App.UseCases.Statistics.GetDailyGameStats;

public sealed record GetDailyGameStatsQuery(DateOnly Day)
    : IQuery<Result<DailyGameStatsDto, GetDailyGameStatsError>>;
