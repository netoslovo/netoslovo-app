using WordoGuessr.API.BuildingBlocks.CQRS;
using WordoGuessr.Common.Domain;

namespace WordoGuessr.Game.App.UseCases.Admin.Daily.Schedule.GetUnassignedDaysCount;

public sealed record GetUnassignedDaysCountQuery(DateOnly From, DateOnly To)
    : IQuery<Result<int, GetUnassignedDaysCountError>>;
