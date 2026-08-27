using WordoGuessr.Game.Dto;

namespace WordoGuessr.Game.API.UseCases.SingleGames.Arcade.GetLatestActive;

public sealed record GetLatestActiveArcadeSingleGameResponse(ArcadeGameInfoDto? Game);
