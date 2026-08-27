using WordoGuessr.API.BuildingBlocks.CQRS;
using WordoGuessr.Game.Dto;

namespace WordoGuessr.Game.App.UseCases.SingleGames.Daily.GetHistory;

public sealed record GetDailyGamesHistoryQuery(Guid PlayerId, int Skip, int Take) : IQuery<DailyGamesHistoryDto>;