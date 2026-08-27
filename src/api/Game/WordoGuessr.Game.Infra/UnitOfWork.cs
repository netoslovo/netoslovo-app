using Wolverine.EntityFrameworkCore;
using WordoGuessr.Game.App.Abstractions;

namespace WordoGuessr.Game.Infra;

internal sealed class UnitOfWork : IGameStoreUnitOfWork
{
    private readonly IDbContextOutbox<GameDbContext> _dbContextOutbox;

    public UnitOfWork(IDbContextOutbox<GameDbContext> dbContextOutbox)
    {
        _dbContextOutbox = dbContextOutbox;
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _dbContextOutbox.SaveChangesAndFlushMessagesAsync(cancellationToken);
    }
}
