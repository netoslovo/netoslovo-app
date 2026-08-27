using WordoGuessr.API.BuildingBlocks.CQRS;
using WordoGuessr.Common.Domain;
using WordoGuessr.Game.Dto;

namespace WordoGuessr.Game.App.UseCases.SingleGames.Daily.StartForDay;

public sealed record StartForDayDailyCommand(DateOnly Day, Guid PlayerId)
    : ICommand<Result<GameDto, StartDailyError>>;
