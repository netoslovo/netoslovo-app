using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WordoGuessr.API.BuildingBlocks.CQRS;
using WordoGuessr.Common.App.Exceptions.Persistence;
using WordoGuessr.Common.Domain;
using WordoGuessr.Common.Domain.ValueObjects;
using WordoGuessr.Game.App.Abstractions;
using WordoGuessr.Game.App.Mapping;
using WordoGuessr.Game.App.Services;
using WordoGuessr.Game.Domain;
using WordoGuessr.Game.Dto;
using WordoGuessr.Words.Contract;

namespace WordoGuessr.Game.App.UseCases.SingleGames.Common.RevealHalfwayWord;

internal sealed class RevealHalfwayWordHandler
    : ICommandHandler<RevealHalfwayWordHintCommand, Result<GuessHintDto, RevealHalfwayWordHintError>>
{
    private readonly IGameStore _dbContext;
    private readonly IGameStoreUnitOfWork _unitOfWork;
    private readonly IWordsModule _wordsModule;
    private readonly TimeProvider _timeProvider;

    private readonly ILogger<RevealHalfwayWordHandler> _logger;

    public RevealHalfwayWordHandler(
        IGameStore dbContext,
        IGameStoreUnitOfWork unitOfWork,
        IWordsModule wordsModule,
        TimeProvider timeProvider,
        ILogger<RevealHalfwayWordHandler> logger)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _wordsModule = wordsModule ?? throw new ArgumentNullException(nameof(wordsModule));
        _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));

        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<GuessHintDto, RevealHalfwayWordHintError>> Handle(RevealHalfwayWordHintCommand command, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(command);

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
            return Result<GuessHintDto, RevealHalfwayWordHintError>.Failure(RevealHalfwayWordHintError.GameNotFound);
        }

        var canRevealHalfwayWord = game.CanRevealHalfwayWord();
        if (!canRevealHalfwayWord.IsSuccess)
        {
            return Result<GuessHintDto, RevealHalfwayWordHintError>.Failure(
                canRevealHalfwayWord.Error.MapToRevealHalfwayWordHintError());
        }

        var halfWayRequestOperation = BuildHalfWayRequestOperation(game.VersionedGameSource.GameSource.Word, game.ClosestGuessDistance);

        var request = new WordsDataRequest(
            TotalWords: new RequestOperation.TotalWords(),
            WordByDistance: halfWayRequestOperation
        );

        var halfwayWordResult = await _wordsModule.LoadData(request, ct);

        if (!halfwayWordResult.WordByDistance.IsSuccess)
        {
            return halfwayWordResult.WordByDistance.Error switch
            {
                SourceWordDataErrorCode.SourceWordNotFound =>
                    await CancelAndReturnError(now, game, ct),

                _ => throw new InvalidOperationException(
                    $"Unexpected words distance error type: {halfwayWordResult.WordByDistance.Error}")
            };
        }

        if (halfwayWordResult.Version != game.VersionedGameSource.WordsVersion)
        {
            return await UpdateVersionAndReturnError(now, game, ct);
        }

        var totalWords = halfwayWordResult.TotalWords;
        var halfWayWord = halfwayWordResult.WordByDistance.Value.Word;
        var halfWayWordDistance = halfwayWordResult.WordByDistance.Value.Distance;

        var attempt = GuessAttempt.FromHint(halfWayWord, halfWayWordDistance, now);

        var hintResult = game.RevealHalfwayWord(attempt);
        if (!hintResult.IsSuccess)
        {
            var errorCode = hintResult.Error.MapToRevealHalfwayWordHintError();
            return Result<GuessHintDto, RevealHalfwayWordHintError>.Failure(errorCode);
        }

        try
        {
            await _unitOfWork.SaveChangesAsync(ct);
        }
        catch (ConcurrencyConflictException ex)
        {
            _logger.LogWarning(ex, "Concurrency update error occurred while revealing halfway word");
            return Result<GuessHintDto, RevealHalfwayWordHintError>.Failure(RevealHalfwayWordHintError.GameStateChanged);
        }
        catch (UniqueConstraintViolationException ex)
        {
            // значит такая попытка уже была в конкурентном запросе
            // просто залогируем и вернём успешный результат
            _logger.LogWarning(ex, "Unique violation error occurred while revealing halfway word");
        }

        var fillPercentage = GuessFillPercentageCalculator.Calculate(halfWayWordDistance, totalWords);
        var guessDto = new GuessDto(halfWayWord.Text, halfWayWordDistance, fillPercentage, attempt.Source.MapToDto());

        var guessesDto = game.GetGuessesOrderedByDistance()
            .Select(guess => guess.MapToDto(GuessFillPercentageCalculator.Calculate(guess.Distance, totalWords)))
            .ToList();

        var score = game.GetScore();
        var guessHintDto = new GuessHintDto(
            new GuessOutcomeDto(
                hintResult.Value.MapToDto(),
                guessDto,
                guessesDto,
                score.Value,
                score.MapToDto()),
            Helpers.BuildHintsInfo(game)
        );

        return Result<GuessHintDto, RevealHalfwayWordHintError>.Success(guessHintDto);
    }

    private async Task<Result<GuessHintDto, RevealHalfwayWordHintError>> UpdateVersionAndReturnError(
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
                    $"Unexpected words distance error type: {newSnapshot.DistancesToWords.Error}")
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
        }
        catch (ConcurrencyConflictException ex)
        {
            _logger.LogWarning(ex, "Concurrency update error occurred while revealing halfway word");
            return Result<GuessHintDto, RevealHalfwayWordHintError>.Failure(RevealHalfwayWordHintError.GameStateChanged);
        }
        catch (UniqueConstraintViolationException ex)
        {
            _logger.LogWarning(ex, "Unique violation error occurred while revealing halfway word");
            return Result<GuessHintDto, RevealHalfwayWordHintError>.Failure(RevealHalfwayWordHintError.GameStateChanged);
        }


        return Result<GuessHintDto, RevealHalfwayWordHintError>.Failure(RevealHalfwayWordHintError.WordsVersionChanged);
    }

    private static RequestOperation.WordByDistance BuildHalfWayRequestOperation(Word sourceWord, int? closestGuessDistance)
    {
        Func<ushort, ushort> distanceFunc = closestGuessDistance.HasValue
            ? _ => (ushort)Math.Ceiling(closestGuessDistance.Value / 2d)
            : total => (ushort)Math.Ceiling(total / 2d);

        return new RequestOperation.WordByDistance(sourceWord, distanceFunc);
    }

    private async Task<Result<GuessHintDto, RevealHalfwayWordHintError>> CancelAndReturnError(DateTimeOffset now, SingleGame game, CancellationToken ct)
    {
        game.Cancel(now);
        try
        {
            await _unitOfWork.SaveChangesAsync(ct);
            return Result<GuessHintDto, RevealHalfwayWordHintError>.Failure(RevealHalfwayWordHintError.GameSourceNotFound);
        }
        catch (ConcurrencyConflictException ex)
        {
            _logger.LogWarning(ex, "Concurrency update error occurred while revealing halfway word");
            return Result<GuessHintDto, RevealHalfwayWordHintError>.Failure(RevealHalfwayWordHintError.GameStateChanged);
        }
    }
}
