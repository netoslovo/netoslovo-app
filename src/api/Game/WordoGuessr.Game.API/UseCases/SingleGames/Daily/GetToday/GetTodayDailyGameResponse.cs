using WordoGuessr.Game.Dto;

namespace WordoGuessr.Game.API.UseCases.SingleGames.Daily.GetToday;

public sealed record GetTodayDailyGameResponse(DailyGameInfoDto? Game);
