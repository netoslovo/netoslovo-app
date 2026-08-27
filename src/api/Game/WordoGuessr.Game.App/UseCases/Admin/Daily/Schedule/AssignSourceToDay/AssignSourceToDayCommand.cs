using WordoGuessr.API.BuildingBlocks.CQRS;
using WordoGuessr.Common.Domain;

namespace WordoGuessr.Game.App.UseCases.Admin.Daily.Schedule.AssignSourceToDay;

public sealed record AssignSourceToDayCommand(DateOnly Day, long ApprovedGameSourceId)
    : ICommand<Result<AssignSourceToDayError>>;
