using WordoGuessr.API.BuildingBlocks.CQRS;
using WordoGuessr.Game.Dto;

namespace WordoGuessr.Game.App.UseCases.Statistics.GetDailyGameCurrentPlayerStreak;

public sealed record GetDailyGameCurrentPlayerStreakQuery(Guid PlayerId)
    : IQuery<DailyGameStreakDto>;
