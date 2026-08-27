using WordoGuessr.API.BuildingBlocks.CQRS;
using WordoGuessr.Game.Dto;

namespace WordoGuessr.Game.App.UseCases.SingleGames.Arcade.GetLatestActive;

public sealed record GetLatestActiveArcadeQuery(Guid PlayerId) : IQuery<ArcadeGameInfoDto?>;
