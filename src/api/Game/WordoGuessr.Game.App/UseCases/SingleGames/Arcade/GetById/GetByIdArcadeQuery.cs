using WordoGuessr.API.BuildingBlocks.CQRS;
using WordoGuessr.Game.Dto;

namespace WordoGuessr.Game.App.UseCases.SingleGames.Arcade.GetById;

public sealed record GetByIdArcadeQuery(Guid PlayerId, Guid GameId) : IQuery<GameDto?>;
