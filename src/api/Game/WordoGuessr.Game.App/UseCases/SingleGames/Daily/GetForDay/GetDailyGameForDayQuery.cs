using WordoGuessr.API.BuildingBlocks.CQRS;
using WordoGuessr.Common.Domain;
using WordoGuessr.Game.Dto;

namespace WordoGuessr.Game.App.UseCases.SingleGames.Daily.GetForDay;

public sealed record GetDailyGameForDayQuery(DateOnly Day, Guid PlayerId)
: IQuery<Result<GameDto?, GetDailyError>>;
