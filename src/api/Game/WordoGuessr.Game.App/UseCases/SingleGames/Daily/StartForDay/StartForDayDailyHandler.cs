using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WordoGuessr.API.BuildingBlocks.CQRS;
using WordoGuessr.Common.App.Exceptions.Persistence;
using WordoGuessr.Common.Domain;
using WordoGuessr.Game.App.Abstractions;
using WordoGuessr.Game.App.Mapping;
using WordoGuessr.Game.App.Services;
using WordoGuessr.Game.Domain;
using WordoGuessr.Game.Dto;
using WordoGuessr.Words.Contract;

namespace WordoGuessr.Game.App.UseCases.SingleGames.Daily.StartForDay;

internal sealed class StartForDayDailyHandler
    : ICommandHandler<StartForDayDailyCommand, Result<GameDto, StartDailyError>>
{
    private readonly IGameStore _dbContext;
    private readonly IGameStoreUnitOfWork _unitOfWork;
    private readonly IWordsModule _wordsModule;
    private readonly TimeProvider _timeProvider;
    private readonly DisplayWordDtoBuilder _displayWordDtoBuilder;
    private readonly ILogger<StartForDayDailyHandler> _logger;

    public StartForDayDailyHandler(
        IGameStore dbContext,
        IGameStoreUnitOfWork unitOfWork,
        IWordsModule wordsModule,
        TimeProvider timeProvider,
        DisplayWordDtoBuilder displayWordDtoBuilder,
        ILogger<StartForDayDailyHandler> logger)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _wordsModule = wordsModule ?? throw new ArgumentNullException(nameof(wordsModule));
        _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
        _displayWordDtoBuilder = displayWordDtoBuilder ?? throw new ArgumentNullException(nameof(displayWordDtoBuilder));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<GameDto, StartDailyError>> Handle(StartForDayDailyCommand command, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(command);

        var now = _timeProvider.GetUtcNow();
        var today = DailyGameClock.GetDateOnly(now);

        if (command.Day > today)
        {
            return Result<GameDto, StartDailyError>.Failure(StartDailyError.ScheduleNotFound);
        }

        var wordsVersionResult = await _wordsModule.LoadData(new WordsDataRequest(), ct);
        var wordsVersion = wordsVersionResult.Version;

        var dailyGameSourceVersion = await _dbContext.VersionedGameSourcesForGameCreation()
            .FirstOrDefaultAsync(
                gsv =>
                    gsv.WordsVersion == wordsVersion &&
                    _dbContext.SingleGamesDailySchedules.Any(s =>
                        s.Day == command.Day &&
                        s.ApprovedGameSourceId == gsv.GameSourceId),
                ct);

        if (dailyGameSourceVersion is null)
        {
            return Result<GameDto, StartDailyError>.Failure(StartDailyError.ScheduleNotFound);
        }

        var hasActiveDailySingleGame = await _dbContext.DailySingleGames
            .AsNoTracking()
            .AnyAsync(sg => sg.PlayerId == command.PlayerId &&
                sg.VersionedGameSource.GameSourceId == dailyGameSourceVersion.GameSourceId &&
                sg.StateCode == SingleGameStateCode.Active,
                ct);

        if (hasActiveDailySingleGame)
        {
            return Result<GameDto, StartDailyError>.Failure(StartDailyError.AlreadyHasActiveGame);
        }

        var dailyGame = new DailySingleGame(
            dailyGameSourceVersion,
            command.PlayerId,
            command.Day,
            now);

        _dbContext.SingleGames.Add(dailyGame);

        try
        {
            await _unitOfWork.SaveChangesAsync(ct);
        }
        catch (UniqueConstraintViolationException ex)
        {
            _logger.LogWarning(ex, "Concurrency error occurred while creating new daily single game");
            return Result<GameDto, StartDailyError>.Failure(StartDailyError.AlreadyHasActiveGame);
        }

        return Result<GameDto, StartDailyError>.Success(
            dailyGame.MapToDto(_displayWordDtoBuilder.Build(dailyGame)));
    }
}
