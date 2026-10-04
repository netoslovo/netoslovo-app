using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WordoGuessr.API.BuildingBlocks.CQRS;
using WordoGuessr.Common.App.Exceptions.Persistence;
using WordoGuessr.Common.Domain;
using WordoGuessr.Game.App.Abstractions;
using WordoGuessr.Game.App.Mapping;
using WordoGuessr.Game.Domain;
using WordoGuessr.Game.Dto;

namespace WordoGuessr.Game.App.UseCases.SingleGames.Common.RevealWordLength;

internal sealed class RevealWordLengthHandler
    : ICommandHandler<RevealWordLengthCommand, Result<TextHintDto, RevealWordLengthError>>
{
    private readonly IGameStore _dbContext;
    private readonly IGameStoreUnitOfWork _unitOfWork;
    private readonly TimeProvider _timeProvider;

    private readonly ILogger<RevealWordLengthHandler> _logger;

    public RevealWordLengthHandler(
        IGameStore dbContext,
        IGameStoreUnitOfWork unitOfWork,
        TimeProvider timeProvider,
        ILogger<RevealWordLengthHandler> logger)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<TextHintDto, RevealWordLengthError>> Handle(RevealWordLengthCommand command, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(command);

        var game = await _dbContext.SingleGamesForAction()
            .FirstOrDefaultAsync(
                sg =>
                    sg.PlayerId == command.PlayerId &&
                    sg.Id == command.GameId &&
                    sg.StateCode == SingleGameStateCode.Active,
                cancellationToken: ct);

        if (game is null)
        {
            return Result<TextHintDto, RevealWordLengthError>.Failure(RevealWordLengthError.GameNotFound);
        }

        var now = _timeProvider.GetUtcNow();
        var result = game.RevealWordLength(now);

        if (!result.IsSuccess)
        {
            return Result<TextHintDto, RevealWordLengthError>.Failure(result.Error.MapToRevealWordLengthError());
        }

        try
        {
            await _unitOfWork.SaveChangesAsync(ct);
        }
        catch (ConcurrencyConflictException ex)
        {
            _logger.LogWarning(ex, "Concurrency update error occurred while revealing word length");
            return Result<TextHintDto, RevealWordLengthError>.Failure(RevealWordLengthError.GameStateChanged);
        }

        var score = game.GetScore();
        var textHintDto = new TextHintDto(
            game.GetDisplayWord().MapToDto(),
            game.BuildHintsInfoDto(),
            score.Value,
            score.MapToDto()
        );

        return Result<TextHintDto, RevealWordLengthError>.Success(textHintDto);
    }
}
