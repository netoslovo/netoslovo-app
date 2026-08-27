using WordoGuessr.Game.Dto;

namespace WordoGuessr.Game.API.UseCases.SingleGames.Arcade.GetById;

public sealed record GetByIdArcadeSingleGameResponse(GameDto? Game);
