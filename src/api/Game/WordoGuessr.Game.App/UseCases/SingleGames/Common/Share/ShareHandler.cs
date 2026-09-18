using Microsoft.EntityFrameworkCore;
using WordoGuessr.API.BuildingBlocks.CQRS;
using WordoGuessr.Common.App.Exceptions.Persistence;
using WordoGuessr.Common.Domain;
using WordoGuessr.Game.App.Abstractions;
using WordoGuessr.Game.Domain;

namespace WordoGuessr.Game.App.UseCases.SingleGames.Common.Share;

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

        var gameExists = await _dbContext.SingleGames.AnyAsync(sg => sg.Id == command.GameId, ct);
        if (!gameExists)
        {
            return Result<Guid, ShareError>.Failure(ShareError.GameNotFound);
        }

        var exisitngShare = await _dbContext.SingleGameShares
            .AsNoTracking()
            .FirstOrDefaultAsync(sgs => sgs.Id == command.GameId, ct);

        if (exisitngShare is not null)
        {
            return Result<Guid, ShareError>.Success(exisitngShare.PublicId);
        }

        var newShare = new SingleGameShare(command.GameId, now, command.ShowGuessWords);
        _dbContext.SingleGameShares.Add(newShare);

        try
        {
            await _unitOfWork.SaveChangesAsync(ct);
        }
        catch (UniqueConstraintViolationException)
        {
            var existingConcurrentShare = await _dbContext.SingleGameShares
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
