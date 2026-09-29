using Microsoft.EntityFrameworkCore;
using WordoGuessr.API.BuildingBlocks.CQRS;
using WordoGuessr.Common.App.Exceptions.Persistence;
using WordoGuessr.Common.Domain;
using WordoGuessr.Game.App.Abstractions;
using WordoGuessr.Game.Domain;

namespace WordoGuessr.Game.App.UseCases.SingleGames.Daily.Share;

internal sealed class ShareHandler : ICommandHandler<ShareCommand, Result<Guid, ShareError>>
{
    private readonly IGameStore _dbContext;
    private readonly IGameStoreUnitOfWork _unitOfWork;
    private readonly TimeProvider _timeProvider;

    public ShareHandler(IGameStore dbContext, IGameStoreUnitOfWork unitOfWork, TimeProvider timeProvider)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
    }

    public async Task<Result<Guid, ShareError>> Handle(
        ShareCommand command,
        CancellationToken ct)
    {
        var now = _timeProvider.GetUtcNow();

        var game = await _dbContext.SingleGames
            .AsNoTracking()
            .SingleOrDefaultAsync(
                sg =>
                    sg.Id == command.GameId &&
                    sg.PlayerId == command.PlayerId &&
                    sg.Mode == SingleGameMode.Daily,
                ct);

        if (game is null)
        {
            return Result<Guid, ShareError>.Failure(ShareError.GameNotFound);
        }

        if (game.StateCode == SingleGameStateCode.Active)
        {
            return Result<Guid, ShareError>.Failure(ShareError.GameIsActive);
        }

        var existingShare = await _dbContext.DailyGameShares
            .AsNoTracking()
            .FirstOrDefaultAsync(sgs => sgs.Id == command.GameId, ct);

        if (existingShare is not null)
        {
            return Result<Guid, ShareError>.Success(existingShare.PublicId);
        }

        var newShare = new DailyGameShare(command.GameId, now);
        _dbContext.DailyGameShares.Add(newShare);

        try
        {
            await _unitOfWork.SaveChangesAsync(ct);
        }
        catch (UniqueConstraintViolationException)
        {
            var existingConcurrentShare = await _dbContext.DailyGameShares
                .AsNoTracking()
                .FirstOrDefaultAsync(sgs => sgs.Id == command.GameId, ct);

            if (existingConcurrentShare is null)
            {
                return Result<Guid, ShareError>.Failure(ShareError.ConcurrencyConflict);
            }

            return Result<Guid, ShareError>.Success(existingConcurrentShare.PublicId);
        }

        return Result<Guid, ShareError>.Success(newShare.PublicId);
    }
}
