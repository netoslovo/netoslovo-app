using WordoGuessr.API.BuildingBlocks.CQRS;
using WordoGuessr.Common.Domain;

namespace WordoGuessr.Game.App.UseCases.Admin.Daily.Schedule.UnassignSourceFromDay;

public sealed record UnassignSourceFromDayCommand(DateOnly Day)
    : ICommand<Result<UnassignSourceFromDayError>>;
