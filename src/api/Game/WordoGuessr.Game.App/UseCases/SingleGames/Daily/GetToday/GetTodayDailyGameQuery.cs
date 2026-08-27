using WordoGuessr.API.BuildingBlocks.CQRS;
using WordoGuessr.Common.Domain;
using WordoGuessr.Game.Dto;

namespace WordoGuessr.Game.App.UseCases.SingleGames.Daily.GetToday;

public sealed record GetTodayDailyGameQuery(Guid PlayerId)
    : IQuery<Result<DailyGameInfoDto?, GetDailyError>>;
