using WordoGuessr.Game.Dto;

namespace WordoGuessr.Game.API.UseCases.SingleGames.Daily.GetShare;

public sealed record GetShareResponse(DailyGameShareDto? Share);
