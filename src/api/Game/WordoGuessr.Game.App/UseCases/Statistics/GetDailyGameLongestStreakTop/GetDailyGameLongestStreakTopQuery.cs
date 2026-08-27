using WordoGuessr.API.BuildingBlocks.CQRS;
using WordoGuessr.Game.Dto;

namespace WordoGuessr.Game.App.UseCases.Statistics.GetDailyGameLongestStreakTop;

public sealed record GetDailyGameLongestStreakTopQuery(Guid PlayerId, int TopN)
    : IQuery<DailyGameStreakTopDto>;
