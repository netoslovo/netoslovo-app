using Microsoft.EntityFrameworkCore;
using WordoGuessr.API.BuildingBlocks.CQRS;
using WordoGuessr.Common.App.Exceptions.Persistence;
using WordoGuessr.Common.Domain;
using WordoGuessr.Game.App.Abstractions;
using WordoGuessr.Game.App.Services;
using WordoGuessr.Game.Domain;
using WordoGuessr.Words.Contract;

namespace WordoGuessr.Game.App.UseCases.Admin.Daily.Schedule.AssignSourceToDay;


internal sealed class AssignSourceToDayHandler
    : ICommandHandler<AssignSourceToDayCommand, Result<AssignSourceToDayError>>
{
    private readonly TimeProvider _timeProvider;
    private readonly IGameStore _dbContext;
    private readonly IGameStoreUnitOfWork _unitOfWork;
    private readonly IWordsModule _wordsModule;

    public AssignSourceToDayHandler(
        TimeProvider timeProvider,
        IGameStore dbContext,
        IGameStoreUnitOfWork unitOfWork,
        IWordsModule wordsModule)
    {
        _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _wordsModule = wordsModule ?? throw new ArgumentNullException(nameof(wordsModule));
    }

    public async Task<Result<AssignSourceToDayError>> Handle(AssignSourceToDayCommand command, CancellationToken ct = default)
    {
        var now = _timeProvider.GetUtcNow();
        var today = DailyGameClock.GetDateOnly(now);

        var wordsVersionResult = await _wordsModule.LoadData(new WordsDataRequest(), ct);
        var wordsVersion = wordsVersionResult.Version;

        var approvedSourceExists = await _dbContext.ApprovedDailyGameSources
            .AnyAsync(
                s =>
                    s.GameSourceId == command.ApprovedGameSourceId &&
                    _dbContext.VersionedGameSources.Any(vgs =>
                        vgs.GameSourceId == s.GameSourceId &&
                        vgs.WordsVersion == wordsVersion),
                ct);

        if (!approvedSourceExists)
        {
            return Result<AssignSourceToDayError>.Failure(AssignSourceToDayError.ApprovedGameSourceNotFound);
        }

        var schedule = await _dbContext.SingleGamesDailySchedules
            .FirstOrDefaultAsync(s => s.Day == command.Day, ct);

        if (schedule is null)
        {
            schedule = new SingleGameDailySchedule(command.ApprovedGameSourceId, command.Day, now);
            _dbContext.SingleGamesDailySchedules.Add(schedule);
        }
        else
        {
            if (command.Day <= today)
            {
                return Result<AssignSourceToDayError>.Failure(AssignSourceToDayError.DayFromPastLocked);
            }

            schedule.Assign(command.ApprovedGameSourceId, now);
        }

        try
        {
            await _unitOfWork.SaveChangesAsync(ct);
        }
        catch (UniqueConstraintViolationException)
        {
            return Result<AssignSourceToDayError>.Failure(AssignSourceToDayError.ConcurrencyConflict);
        }

        return Result<AssignSourceToDayError>.Success();
    }
}
