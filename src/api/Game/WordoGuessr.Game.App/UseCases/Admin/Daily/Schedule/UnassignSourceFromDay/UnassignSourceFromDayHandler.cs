using Microsoft.EntityFrameworkCore;
using WordoGuessr.API.BuildingBlocks.CQRS;
using WordoGuessr.Common.App.Exceptions.Persistence;
using WordoGuessr.Common.Domain;
using WordoGuessr.Game.App.Abstractions;
using WordoGuessr.Game.App.Services;

namespace WordoGuessr.Game.App.UseCases.Admin.Daily.Schedule.UnassignSourceFromDay;

internal sealed class UnassignSourceFromDayHandler
    : ICommandHandler<UnassignSourceFromDayCommand, Result<UnassignSourceFromDayError>>
{
    private readonly TimeProvider _timeProvider;
    private readonly IGameStore _dbContext;
    private readonly IGameStoreUnitOfWork _unitOfWork;

    public UnassignSourceFromDayHandler(
        TimeProvider timeProvider,
        IGameStore dbContext,
        IGameStoreUnitOfWork unitOfWork)
    {
        _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<Result<UnassignSourceFromDayError>> Handle(
        UnassignSourceFromDayCommand command,
        CancellationToken ct = default)
    {
        var today = DailyGameClock.GetDateOnly(_timeProvider.GetUtcNow());

        if (command.Day <= today)
        {
            return Result<UnassignSourceFromDayError>.Failure(UnassignSourceFromDayError.DayFromPastLocked);
        }

        var schedule = await _dbContext.SingleGamesDailySchedules
            .FirstOrDefaultAsync(s => s.Day == command.Day, ct);

        if (schedule is null)
        {
            return Result<UnassignSourceFromDayError>.Success();
        }

        _dbContext.SingleGamesDailySchedules.Remove(schedule);

        try
        {
            await _unitOfWork.SaveChangesAsync(ct);
        }
        catch (ConcurrencyConflictException)
        {
            return Result<UnassignSourceFromDayError>.Failure(UnassignSourceFromDayError.ConcurrencyConflict);
        }

        return Result<UnassignSourceFromDayError>.Success();
    }
}
