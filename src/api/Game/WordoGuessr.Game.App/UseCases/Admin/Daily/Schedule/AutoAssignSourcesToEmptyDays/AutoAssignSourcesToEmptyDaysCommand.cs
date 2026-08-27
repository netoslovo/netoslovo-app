using WordoGuessr.API.BuildingBlocks.CQRS;
using WordoGuessr.Common.Domain;

namespace WordoGuessr.Game.App.UseCases.Admin.Daily.Schedule.AutoAssignSourcesToEmptyDays;

public sealed record AutoAssignSourcesToEmptyDaysCommand(DateOnly From, DateOnly To)
    : ICommand<Result<AutoAssignSourcesToEmptyDaysError>>;