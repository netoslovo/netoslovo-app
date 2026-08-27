using WordoGuessr.API.BuildingBlocks.CQRS;
using WordoGuessr.Game.Dto;

namespace WordoGuessr.Game.App.UseCases.SingleGames.Arcade.GetHistory;

public sealed record GetArcadeGamesHistoryQuery(Guid PlayerId, int Skip, int Take)
    : IQuery<ArcadeGamesHistoryDto>;