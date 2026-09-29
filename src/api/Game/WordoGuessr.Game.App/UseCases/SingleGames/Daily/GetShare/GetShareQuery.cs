using WordoGuessr.API.BuildingBlocks.CQRS;
using WordoGuessr.Game.Dto;

namespace WordoGuessr.Game.App.UseCases.SingleGames.Daily.GetShare;

public sealed record GetShareQuery(Guid PlayerId, Guid GameId)
    : IQuery<DailyGameShareDto?>;
