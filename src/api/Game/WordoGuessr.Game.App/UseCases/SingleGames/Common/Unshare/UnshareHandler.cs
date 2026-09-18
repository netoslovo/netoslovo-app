using Microsoft.EntityFrameworkCore;
using WordoGuessr.API.BuildingBlocks.CQRS;
using WordoGuessr.Game.App.Abstractions;

namespace WordoGuessr.Game.App.UseCases.SingleGames.Common.Unshare;

internal sealed class UnshareHandler : ICommandHandler<UnshareCommand>
{
    private readonly IGameStore _dbContext;

    public UnshareHandler(IGameStore dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task Handle(UnshareCommand command, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        await _dbContext.SingleGameShares
            .Where(sgs => sgs.Id == command.GameId && sgs.SingleGame.PlayerId == command.PlayerId)
            .ExecuteDeleteAsync(ct);
    }
}
