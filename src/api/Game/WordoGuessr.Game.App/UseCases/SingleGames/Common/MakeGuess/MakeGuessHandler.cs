using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WordoGuessr.API.BuildingBlocks.CQRS;
using WordoGuessr.Common.App.Exceptions.Persistence;
using WordoGuessr.Common.Domain;
using WordoGuessr.Common.Domain.ValueObjects;
using WordoGuessr.Game.App.Abstractions;
using WordoGuessr.Game.App.Mapping;
using WordoGuessr.Game.Domain;
using WordoGuessr.Game.Dto;
using WordoGuessr.Words.Contract;

namespace WordoGuessr.Game.App.UseCases.SingleGames.Common.MakeGuess;

internal sealed class MakeGuessHandler : ICommandHandler<MakeGuessCommand, Result<GuessOutcomeDto, MakeGuessError>>
{
    private readonly IGameStore _dbContext;
    private readonly IGameStoreUnitOfWork _unitOfWork;
    private readonly IWordsModule _wordsModule;
    private readonly TimeProvider _timeProvider;

    private readonly ILogger<MakeGuessHandler> _logger;

    public MakeGuessHandler(
        IGameStore dbContext,
        IGameStoreUnitOfWork unitOfWork,
        IWordsModule wordsModule,
        TimeProvider timeProvider,
        ILogger<MakeGuessHandler> logger)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _wordsModule = wordsModule ?? throw new ArgumentNullException(nameof(wordsModule));
        _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));

        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<GuessOutcomeDto, MakeGuessError>> Handle(MakeGuessCommand command, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (!Word.TryCreate(command.Word, out var inputWord))
        {
            return Result<GuessOutcomeDto, MakeGuessError>.Failure(MakeGuessError.InvalidInput);
        }

        var now = _timeProvider.GetUtcNow();

        var game = await _dbContext.SingleGamesForAction()
            .FirstOrDefaultAsync(
                sg =>
                    sg.PlayerId == command.PlayerId &&
                    sg.Id == command.GameId &&
                    sg.StateCode == SingleGameStateCode.Active,
                cancellationToken: ct);

        if (game is null)
        {
            return Result<GuessOutcomeDto, MakeGuessError>.Failure(MakeGuessError.GameNotFound);
        }

        var request = new WordsDataRequest(
            TotalWords: new RequestOperation.TotalWords(),
            DistanceToWord: new RequestOperation.DistanceToWord(game.VersionedGameSource.GameSource.Word, inputWord)
        );

        var distanceSnapshot = await _wordsModule.LoadData(request, ct);

        if (!distanceSnapshot.DistanceToWord.IsSuccess)
        {
            return distanceSnapshot.DistanceToWord.Error switch
            {
                WordPairDataErrorCode.TargetWordNotFound =>
                    Result<GuessOutcomeDto, MakeGuessError>.Failure(MakeGuessError.WordNotFound),

                WordPairDataErrorCode.SourceWordNotFound =>
                    await CancelAndReturnError(now, game, ct),

                _ =>
                    throw new InvalidOperationException(
                        $"Unexpected words distance error code: {distanceSnapshot.DistanceToWord.Error}")
            };
        }

        ushort guessDistance;
        ushort totalWords;
        if (distanceSnapshot.Version != game.VersionedGameSource.WordsVersion)
        {
            return await UpdateVersionAndReturnError(now, game, ct);
        }

        guessDistance = distanceSnapshot.DistanceToWord.Value.Distance;
        totalWords = distanceSnapshot.TotalWords;

        var attempt = GuessAttempt.FromPlayer(inputWord, guessDistance, now);
        var guessStatusResult = game.MakeGuess(attempt);

        if (!guessStatusResult.IsSuccess)
        {
            var errorCode = guessStatusResult.Error.MapToMakeGuessError();
            return Result<GuessOutcomeDto, MakeGuessError>.Failure(errorCode);
        }

        try
        {
            await _unitOfWork.SaveChangesAsync(ct);
        }
        catch (ConcurrencyConflictException ex)
        {
            _logger.LogWarning(ex, "Concurrency update error occurred while making guess");
            return Result<GuessOutcomeDto, MakeGuessError>.Failure(MakeGuessError.GameStateChanged);
        }
        catch (UniqueConstraintViolationException ex)
        {
            // значит такая попытка уже была в конкурентном запросе
            // просто залогируем и вернём успешный результат
            _logger.LogWarning(ex, "Unique violation error occurred while making guess");
        }

        var guessDto = attempt.MapToDto(totalWords);
        var guessesDto = game.Guesses.MapToListDto(totalWords);

        var score = game.GetScore();
        var guessOutcomeDto = new GuessOutcomeDto(
            guessStatusResult.Value.MapToDto(),
            guessDto,
            guessesDto,
            score.Value,
            score.MapToDto());

        return Result<GuessOutcomeDto, MakeGuessError>.Success(guessOutcomeDto);
    }

    // TODO: вынести в общий хэлпер-сервис
    private async Task<Result<GuessOutcomeDto, MakeGuessError>> UpdateVersionAndReturnError(
        DateTimeOffset now,
        SingleGame game,
        CancellationToken ct)
    {

        var request = new WordsDataRequest(
            DistancesToWords: new RequestOperation.DistancesToWords(game.VersionedGameSource.GameSource.Word, game.Guesses.Select(g => g.Word).ToArray()));

        var newSnapshot = await _wordsModule.LoadData(request, ct);

        if (!newSnapshot.DistancesToWords.IsSuccess)
        {
            return newSnapshot.DistancesToWords.Error switch
            {
                SourceWordDataErrorCode.SourceWordNotFound =>
                    await CancelAndReturnError(now, game, ct),

                _ => throw new InvalidOperationException(
                    $"Unexpected words distance error code: {newSnapshot.DistancesToWords.Error}")
            };
        }

        var newGameSourceVersion = await _dbContext.VersionedGameSources
            .FirstOrDefaultAsync(
                vgs =>
                    vgs.GameSourceId == game.VersionedGameSource.GameSourceId &&
                    vgs.WordsVersion == newSnapshot.Version,
                ct
            );

        if (newGameSourceVersion is null)
        {
            return await CancelAndReturnError(now, game, ct);
        }

        var newGuesses = newSnapshot.DistancesToWords.Value
            .Where(x => x.Distance.HasValue)
            .ToDictionary(x => x.Word, x => x.Distance!.Value);

        game.UpdateWordsVersion(newGuesses, newGameSourceVersion, now);

        try
        {
            await _unitOfWork.SaveChangesAsync(ct);
            return Result<GuessOutcomeDto, MakeGuessError>.Failure(MakeGuessError.WordsVersionChanged);
        }
        catch (ConcurrencyConflictException ex)
        {
            _logger.LogWarning(ex, "Concurrency update error occurred while making guess");
            return Result<GuessOutcomeDto, MakeGuessError>.Failure(MakeGuessError.GameStateChanged);
        }
    }

    private async Task<Result<GuessOutcomeDto, MakeGuessError>> CancelAndReturnError(DateTimeOffset now, SingleGame game, CancellationToken ct)
    {
        game.Cancel(now);
        try
        {
            await _unitOfWork.SaveChangesAsync(ct);
            return Result<GuessOutcomeDto, MakeGuessError>.Failure(MakeGuessError.GameSourceNotFound);
        }
        catch (ConcurrencyConflictException ex)
        {
            _logger.LogWarning(ex, "Concurrency update error occurred while making guess");
            return Result<GuessOutcomeDto, MakeGuessError>.Failure(MakeGuessError.GameStateChanged);
        }
    }
}
