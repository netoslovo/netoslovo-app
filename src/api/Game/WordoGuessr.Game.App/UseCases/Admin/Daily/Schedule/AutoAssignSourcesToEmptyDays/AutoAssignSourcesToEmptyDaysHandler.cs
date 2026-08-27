using Microsoft.EntityFrameworkCore;
using WordoGuessr.API.BuildingBlocks.CQRS;
using WordoGuessr.Common.App.Exceptions.Persistence;
using WordoGuessr.Common.Domain;
using WordoGuessr.Game.App.Abstractions;
using WordoGuessr.Game.Domain;
using WordoGuessr.Words.Contract;

namespace WordoGuessr.Game.App.UseCases.Admin.Daily.Schedule.AutoAssignSourcesToEmptyDays;

internal sealed class AutoAssignSourcesToEmptyDaysHandler
    : ICommandHandler<AutoAssignSourcesToEmptyDaysCommand, Result<AutoAssignSourcesToEmptyDaysError>>
{
    private const int MaxIntervalDays = 365;
    private readonly IGameStore _dbContext;
    private readonly IGameStoreUnitOfWork _unitOfWork;
    private readonly TimeProvider _timeProvider;
    private readonly IWordsModule _wordsModule;

    public AutoAssignSourcesToEmptyDaysHandler(
        IGameStore dbContext,
        IGameStoreUnitOfWork unitOfWork,
        TimeProvider timeProvider,
        IWordsModule wordsModule)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
        _wordsModule = wordsModule ?? throw new ArgumentNullException(nameof(wordsModule));
    }

    public async Task<Result<AutoAssignSourcesToEmptyDaysError>> Handle(
        AutoAssignSourcesToEmptyDaysCommand command,
        CancellationToken ct = default)
    {
        var now = _timeProvider.GetUtcNow();

        var fromDayNumber = command.From.DayNumber;
        var toDayNumber = command.To.DayNumber;
        var intervalDays = toDayNumber - fromDayNumber + 1;
        if (intervalDays > MaxIntervalDays)
        {
            return Result<AutoAssignSourcesToEmptyDaysError>
                .Failure(AutoAssignSourcesToEmptyDaysError.TooWideInterval);
        }

        var existingSchedules = await _dbContext.SingleGamesDailySchedules
            .AsNoTracking()
            .Where(s =>
                s.Day >= command.From &&
                s.Day <= command.To)
            .Select(s => s.Day)
            .ToArrayAsync(ct);

        var allDays = Enumerable.Range(fromDayNumber, intervalDays)
            .Select(DateOnly.FromDayNumber);

        var missingSchedules = allDays
            .Except(existingSchedules)
            .ToArray();

        var approvedSourcesNeed = missingSchedules.Length;

        var wordsVersionResult = await _wordsModule.LoadData(new WordsDataRequest(), ct);
        var wordsVersion = wordsVersionResult.Version;

        var sources = await _dbContext.ApprovedDailyGameSources
            .AsNoTracking()
            .Where(a =>
                !_dbContext.SingleGamesDailySchedules.Any(s => s.ApprovedGameSourceId == a.GameSourceId) &&
                _dbContext.VersionedGameSources.Any(vgs =>
                    vgs.GameSourceId == a.GameSourceId &&
                    vgs.WordsVersion == wordsVersion))
            .OrderBy(_ => EF.Functions.Random())
            .Take(approvedSourcesNeed)
            .Select(a => a.GameSourceId)
            .ToArrayAsync(ct);

        if (sources.Length < approvedSourcesNeed)
        {
            return Result<AutoAssignSourcesToEmptyDaysError>
                .Failure(AutoAssignSourcesToEmptyDaysError.NotEnoughApprovedSources);
        }

        var newSchedules = new List<SingleGameDailySchedule>();
        for (var i = 0; i < missingSchedules.Length; i++)
        {
            var day = missingSchedules[i];
            var schedule = new SingleGameDailySchedule(sources[i], day, now);
            newSchedules.Add(schedule);
        }

        _dbContext.SingleGamesDailySchedules.AddRange(newSchedules);
        try
        {
            await _unitOfWork.SaveChangesAsync(ct);
        }
        catch (UniqueConstraintViolationException)
        {
            return Result<AutoAssignSourcesToEmptyDaysError>
                .Failure(AutoAssignSourcesToEmptyDaysError.ConcurrencyConflict);
        }

        return Result<AutoAssignSourcesToEmptyDaysError>.Success();
    }
}
