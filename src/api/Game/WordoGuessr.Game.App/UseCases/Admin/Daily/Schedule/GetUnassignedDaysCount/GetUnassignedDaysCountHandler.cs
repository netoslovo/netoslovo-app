using Microsoft.EntityFrameworkCore;
using WordoGuessr.API.BuildingBlocks.CQRS;
using WordoGuessr.Common.Domain;
using WordoGuessr.Game.App.Abstractions;

namespace WordoGuessr.Game.App.UseCases.Admin.Daily.Schedule.GetUnassignedDaysCount;

internal sealed class GetUnassignedDaysCountHandler
    : IQueryHandler<
        GetUnassignedDaysCountQuery,
        Result<int, GetUnassignedDaysCountError>>
{
    private const int MaxIntervalDays = 365;

    private readonly IGameStore _dbContext;

    public GetUnassignedDaysCountHandler(IGameStore dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task<Result<int, GetUnassignedDaysCountError>> Handle(
        GetUnassignedDaysCountQuery query,
        CancellationToken ct)
    {
        var fromDayNumber = query.From.DayNumber;
        var toDayNumber = query.To.DayNumber;
        var intervalDays = toDayNumber - fromDayNumber + 1;
        if (intervalDays > MaxIntervalDays)
        {
            return Result<int, GetUnassignedDaysCountError>
                .Failure(GetUnassignedDaysCountError.TooWideInterval);
        }

        var assignedDays = await _dbContext.SingleGamesDailySchedules
            .AsNoTracking()
            .CountAsync(
                s => s.Day >= query.From && s.Day <= query.To,
                ct);

        return Result<int, GetUnassignedDaysCountError>.Success(intervalDays - assignedDays);
    }
}
