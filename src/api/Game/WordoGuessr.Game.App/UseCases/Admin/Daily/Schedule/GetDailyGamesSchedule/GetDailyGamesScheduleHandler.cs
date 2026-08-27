using Microsoft.EntityFrameworkCore;
using WordoGuessr.API.BuildingBlocks.CQRS;
using WordoGuessr.Common.Domain;
using WordoGuessr.Game.App.Abstractions;
using WordoGuessr.Game.App.Services;
using WordoGuessr.Game.Dto;

namespace WordoGuessr.Game.App.UseCases.Admin.Daily.Schedule.GetDailyGamesSchedule;

internal sealed class GetDailyGamesScheduleHandler
    : IQueryHandler<
        GetDailyGamesScheduleQuery,
        Result<DailyGameSchedulesDto, GetDailyGamesScheduleError>>
{
    private const int MaxIntervalDays = 365 * 5;
    private readonly IGameStore _dbContext;
    private readonly TimeProvider _timeProvider;

    public GetDailyGamesScheduleHandler(IGameStore dbContext, TimeProvider timeProvider)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
    }

    public async Task<Result<DailyGameSchedulesDto, GetDailyGamesScheduleError>> Handle(
        GetDailyGamesScheduleQuery query,
        CancellationToken ct)
    {
        var now = _timeProvider.GetUtcNow();
        var today = DailyGameClock.GetDateOnly(now);

        var intervalLength = query.To.DayNumber - query.From.DayNumber + 1;
        if (intervalLength > MaxIntervalDays)
        {
            return Result<DailyGameSchedulesDto, GetDailyGamesScheduleError>
                .Failure(GetDailyGamesScheduleError.TooWideInterval);
        }

        var allDays = Enumerable
            .Range(query.From.DayNumber, intervalLength)
            .Select(DateOnly.FromDayNumber);

        var sortDirection = query.SortDirection ?? SortDirectionDto.Desc;

        return query.AssignedFilter switch
        {
            true => await GetAssignedSchedules(query, today, sortDirection, ct),
            false => await GetUnassignedSchedules(query, allDays, sortDirection, ct),
            null => await GetAllSchedules(query, today, allDays, sortDirection, ct)
        };
    }

    private async Task<Result<DailyGameSchedulesDto, GetDailyGamesScheduleError>> GetAllSchedules(
        GetDailyGamesScheduleQuery query,
        DateOnly today,
        IEnumerable<DateOnly> allDays,
        SortDirectionDto sortDirection,
        CancellationToken ct)
    {
        var sortedAllDays = SortDays(allDays, sortDirection);

        var daysIntervalPlusOne = sortedAllDays
            .Skip(query.Skip)
            .Take(query.Take + 1);

        var schedulesByDay = await _dbContext.SingleGamesDailySchedules
            .AsNoTracking()
            .Where(schedule => daysIntervalPlusOne.Contains(schedule.Day))
            .Select(schedule => new DailyGameScheduleDto(
                schedule.Day,
                new GameSourceDto(
                    schedule.ApprovedGameSource.Review.VersionedGameSource.GameSourceId,
                    schedule.ApprovedGameSource.Review.VersionedGameSource.GameSource.Word.Text,
                    new DifficultyDto(
                        schedule.ApprovedGameSource.Review.VersionedGameSource.Difficulty.Code,
                        schedule.ApprovedGameSource.Review.VersionedGameSource.Difficulty.Name)),
                    schedule.Day <= today))
            .ToDictionaryAsync(schedule => schedule.Day, ct);

        var schedulesPlusOne = daysIntervalPlusOne
            .Select(day => schedulesByDay.GetValueOrDefault(day)
                ?? new DailyGameScheduleDto(day, null, false))
            .ToArray();

        return Result<DailyGameSchedulesDto, GetDailyGamesScheduleError>
            .Success(CreatePage(schedulesPlusOne, query.Take));
    }

    private async Task<Result<DailyGameSchedulesDto, GetDailyGamesScheduleError>> GetUnassignedSchedules(
        GetDailyGamesScheduleQuery query,
        IEnumerable<DateOnly> allDays,
        SortDirectionDto sortDirection,
        CancellationToken ct)
    {
        var assignedDays = await _dbContext.SingleGamesDailySchedules
            .AsNoTracking()
            .Where(s => s.Day >= query.From && s.Day <= query.To)
            .Select(s => s.Day)
            .ToHashSetAsync(ct);

        var sortedAllDays = SortDays(allDays, sortDirection);

        var schedulesPlusOne = sortedAllDays
            .Where(s => !assignedDays.Contains(s))
            .Skip(query.Skip)
            .Take(query.Take + 1)
            .Select(s => new DailyGameScheduleDto(s, null, false))
            .ToArray();

        return Result<DailyGameSchedulesDto, GetDailyGamesScheduleError>
            .Success(CreatePage(schedulesPlusOne, query.Take));
    }

    private async Task<Result<DailyGameSchedulesDto, GetDailyGamesScheduleError>> GetAssignedSchedules(
        GetDailyGamesScheduleQuery query,
        DateOnly today,
        SortDirectionDto sortDirection,
        CancellationToken ct)
    {
        var schedulesQuery = _dbContext.SingleGamesDailySchedules
            .Where(s => s.Day >= query.From && s.Day <= query.To);

        schedulesQuery = sortDirection switch
        {
            SortDirectionDto.Asc => schedulesQuery.OrderBy(s => s.Day),
            SortDirectionDto.Desc => schedulesQuery.OrderByDescending(s => s.Day),
            _ => schedulesQuery.OrderBy(s => s.Day)
        };

        var schedulesFromDb = await schedulesQuery
            .AsNoTracking()
            .Skip(query.Skip)
            .Take(query.Take + 1)
            .Select(schedule => new DailyGameScheduleDto(
                schedule.Day,
                new GameSourceDto(
                    schedule.ApprovedGameSource.Review.VersionedGameSource.GameSourceId,
                    schedule.ApprovedGameSource.Review.VersionedGameSource.GameSource.Word.Text,
                    new DifficultyDto(
                        schedule.ApprovedGameSource.Review.VersionedGameSource.Difficulty.Code,
                        schedule.ApprovedGameSource.Review.VersionedGameSource.Difficulty.Name)),
                schedule.Day <= today))
            .ToArrayAsync(ct);

        return Result<DailyGameSchedulesDto, GetDailyGamesScheduleError>
            .Success(CreatePage(schedulesFromDb, query.Take));
    }

    private static IEnumerable<DateOnly> SortDays(IEnumerable<DateOnly> days, SortDirectionDto sortDirection)
    {
        return sortDirection switch
        {
            SortDirectionDto.Asc => days.Order(),
            SortDirectionDto.Desc => days.OrderDescending(),
            _ => days
        };
    }

    private static DailyGameSchedulesDto CreatePage(DailyGameScheduleDto[] schedules, int take) =>
        new DailyGameSchedulesDto(
            schedules.Take(take).ToArray(),
            schedules.Length > take);
}
