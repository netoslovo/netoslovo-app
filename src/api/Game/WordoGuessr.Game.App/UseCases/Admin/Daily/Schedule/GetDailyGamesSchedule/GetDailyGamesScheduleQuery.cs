using WordoGuessr.API.BuildingBlocks.CQRS;
using WordoGuessr.Common.Domain;
using WordoGuessr.Game.Dto;

namespace WordoGuessr.Game.App.UseCases.Admin.Daily.Schedule.GetDailyGamesSchedule;

public sealed record GetDailyGamesScheduleQuery(
    DateOnly From,
    DateOnly To,
    int Skip,
    int Take,
    SortDirectionDto? SortDirection,
    bool? AssignedFilter)
    : IQuery<Result<DailyGameSchedulesDto, GetDailyGamesScheduleError>>;
