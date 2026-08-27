using WordoGuessr.API.BuildingBlocks.CQRS;
using WordoGuessr.Game.Dto;

namespace WordoGuessr.Game.App.UseCases.Statistics.GetDailyGameCurrentStreakTop;

public sealed record GetDailyGameCurrentStreakTopQuery(Guid PlayerId, int TopN)
    : IQuery<DailyGameStreakTopDto>;
