using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WordoGuessr.API.BuildingBlocks.CQRS;
using WordoGuessr.Common.App.Exceptions.Persistence;
using WordoGuessr.Common.Domain;
using WordoGuessr.Game.App.Abstractions;
using WordoGuessr.Game.App.Mapping;
using WordoGuessr.Game.Domain;

namespace WordoGuessr.Game.App.UseCases.SingleGames.Common.Surrender;

internal sealed class SurrenderHandler : ICommandHandler<SurrenderCommand, Result<SurrenderError>>
{
    private readonly IGameStore _dbContext;
    private readonly IGameStoreUnitOfWork _unitOfWork;
    private readonly TimeProvider _timeProvider;

    private readonly ILogger<SurrenderHandler> _logger;

    public SurrenderHandler(
        IGameStore dbContext,
        IGameStoreUnitOfWork unitOfWork,
        TimeProvider timeProvider,
        ILogger<SurrenderHandler> logger)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    // TODO: проверка версии слов, чтобы завершать с текущей
    public async Task<Result<SurrenderError>> Handle(
        SurrenderCommand command,
        CancellationToken ct = default)
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
            return Result<SurrenderError>.Failure(SurrenderError.GameNotFound);
        }

        var now = _timeProvider.GetUtcNow();
        var result = game.Surrender(now);

        if (!result.IsSuccess)
        {
            var errorCode = result.Error.MapToSurrenderError();
            return Result<SurrenderError>.Failure(errorCode);
        }

        try
        {
            await _unitOfWork.SaveChangesAsync(ct);
        }
        catch (ConcurrencyConflictException ex)
        {
            _logger.LogWarning(ex, "Concurrency update error occurred while surrending the game");
            return Result<SurrenderError>.Failure(SurrenderError.GameStateChanged);
        }
        catch (UniqueConstraintViolationException ex)
        {
            // возможно конкурентный запрос Surrender
            // просто залогируем и вернём успешный результат
            _logger.LogWarning(ex, "Unique violation error occurred while surrending the game");
        }

        return Result<SurrenderError>.Success();
    }
}
