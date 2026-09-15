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

namespace WordoGuessr.Game.App.UseCases.SingleGames.Arcade.Create;

internal sealed class CreateArcadeHandler : ICommandHandler<CreateArcadeCommand, Result<GameDto, CreateArcadeError>>
{
    private readonly IGameStore _dbContext;
    private readonly IGameStoreUnitOfWork _unitOfWork;
    private readonly IWordsModule _wordsModule;
    private readonly TimeProvider _timeProvider;
    private readonly DisplayWordDtoBuilder _displayWordDtoBuilder;
    private readonly ILogger<CreateArcadeHandler> _logger;

    public CreateArcadeHandler(
        IGameStore dbContext,
        IGameStoreUnitOfWork unitOfWork,
        IWordsModule wordsModule,
        TimeProvider timeProvider,
        DisplayWordDtoBuilder displayWordDtoBuilder,
        ILogger<CreateArcadeHandler> logger)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _wordsModule = wordsModule ?? throw new ArgumentNullException(nameof(wordsModule));
        _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
        _displayWordDtoBuilder = displayWordDtoBuilder ?? throw new ArgumentNullException(nameof(displayWordDtoBuilder));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<GameDto, CreateArcadeError>> Handle(CreateArcadeCommand command, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(command);

        var difficulty = Difficulty.FromCode(command.DifficultyCode);

        var wordsVersionResult = await _wordsModule.LoadData(new WordsDataRequest(), ct);
        var wordsVersion = wordsVersionResult.Version;

        var playedSourceIds = _dbContext.ArcadeSingleGames
            .Where(sg => sg.PlayerId == command.PlayerId)
            .Select(sg => sg.VersionedGameSource.GameSourceId);

        var notPlayedSources = _dbContext.VersionedGameSourcesForGameCreation()
            .Where(gsv =>
                !playedSourceIds.Contains(gsv.GameSourceId) &&
                gsv.WordsVersion == wordsVersion &&
                gsv.Difficulty == difficulty);

        var notPlayedSourcesCount = await notPlayedSources
            .CountAsync(ct);

        if (notPlayedSourcesCount == 0)
        {
            return Result<GameDto, CreateArcadeError>.Failure(CreateArcadeError.NoMorePossibleGames);
        }

        var randomOffset = Random.Shared.Next(notPlayedSourcesCount);
        var notPlayedSource = await notPlayedSources
            .OrderBy(gsv => gsv.GameSourceId)
            .Skip(randomOffset)
            .FirstOrDefaultAsync(ct);

        if (notPlayedSource is null)
        {
            return Result<GameDto, CreateArcadeError>.Failure(CreateArcadeError.NoMorePossibleGames);
        }

        var arcadeGame = new ArcadeSingleGame(
            notPlayedSource,
            command.PlayerId,
            _timeProvider.GetUtcNow());

        _dbContext.SingleGames.Add(arcadeGame);
        try
        {
            await _unitOfWork.SaveChangesAsync(ct);
        }
        catch (UniqueConstraintViolationException ex)
        {
            _logger.LogWarning(ex, "Concurrency error occurred while creating new arcade single game");
            return Result<GameDto, CreateArcadeError>.Failure(CreateArcadeError.AlreadyHasTheSameGame);
        }

        return Result<GameDto, CreateArcadeError>.Success(
            arcadeGame.MapToDto(_displayWordDtoBuilder.Build(arcadeGame)));
    }
}
