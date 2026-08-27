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

namespace WordoGuessr.Game.App.UseCases.SingleGames.Common.RevealRandomLetter;

internal sealed class RevealRandomLetterHandler
    : ICommandHandler<RevealRandomLetterCommand, Result<TextHintDto, RevealRandomLetterError>>
{
    private readonly IGameStore _dbContext;
    private readonly IGameStoreUnitOfWork _unitOfWork;
    private readonly TimeProvider _timeProvider;
    private readonly DisplayWordDtoBuilder _displayWordDtoBuilder;

    private readonly ILogger<RevealRandomLetterHandler> _logger;

    public RevealRandomLetterHandler(
        IGameStore dbContext,
        IGameStoreUnitOfWork unitOfWork,
        TimeProvider timeProvider,
        DisplayWordDtoBuilder displayWordDtoBuilder,
        ILogger<RevealRandomLetterHandler> logger)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
        _displayWordDtoBuilder = displayWordDtoBuilder ?? throw new ArgumentNullException(nameof(displayWordDtoBuilder));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<TextHintDto, RevealRandomLetterError>> Handle(
        RevealRandomLetterCommand command,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var game = await _dbContext.SingleGamesForTextHintAction()
            .FirstOrDefaultAsync(
                sg =>
                    sg.PlayerId == command.PlayerId &&
                    sg.Id == command.GameId &&
                    sg.StateCode == SingleGameStateCode.Active,
                cancellationToken: ct);

        if (game is null)
        {
            return Result<TextHintDto, RevealRandomLetterError>.Failure(RevealRandomLetterError.GameNotFound);
        }

        var now = _timeProvider.GetUtcNow();
        var result = game.RevealRandomLetter(now);

        if (!result.IsSuccess)
        {
            return Result<TextHintDto, RevealRandomLetterError>.Failure(result.Error.MapToRevealRandomLetterError());
        }

        try
        {
            await _unitOfWork.SaveChangesAsync(ct);
        }
        catch (ConcurrencyConflictException ex)
        {
            _logger.LogWarning(ex, "Concurrency update error occurred while revealing random lettere length");
            return Result<TextHintDto, RevealRandomLetterError>.Failure(RevealRandomLetterError.GameStateChanged);
        }

        var score = game.GetScore();
        var textHintDto = new TextHintDto(
            _displayWordDtoBuilder.Build(game),
            Helpers.BuildHintsInfo(game),
            score.Value,
            score.MapToDto()
        );

        return Result<TextHintDto, RevealRandomLetterError>.Success(textHintDto);
    }
}
