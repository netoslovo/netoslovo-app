using WordoGuessr.Game.Dto;

namespace WordoGuessr.Game.API.UseCases.SingleGames.Daily.GetForDay;

public sealed record GetDailyGameForDayResponse(GameDto? Game);
