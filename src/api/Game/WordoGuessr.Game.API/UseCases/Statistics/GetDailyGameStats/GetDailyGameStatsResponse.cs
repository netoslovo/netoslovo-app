using WordoGuessr.Game.Dto;

namespace WordoGuessr.Game.API.UseCases.Statistics.GetDailyGameStats;

public sealed record GetDailyGameStatsResponse(DailyGameStatsDto? Stats);
