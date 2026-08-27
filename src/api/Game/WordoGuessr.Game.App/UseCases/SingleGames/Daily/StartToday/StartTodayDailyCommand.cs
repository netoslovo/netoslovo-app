using WordoGuessr.API.BuildingBlocks.CQRS;
using WordoGuessr.Common.Domain;
using WordoGuessr.Game.Dto;

namespace WordoGuessr.Game.App.UseCases.SingleGames.Daily.StartToday;

public sealed record StartTodayDailyCommand(Guid PlayerId)
    : ICommand<Result<GameDto, StartDailyError>>;
