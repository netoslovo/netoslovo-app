using Wolverine.EntityFrameworkCore;
using WordoGuessr.Auth.App.Abstractions;

namespace WordoGuessr.Auth.Infra.Database;

internal sealed class OutboxWrapper : IOutbox
{
    private readonly IDbContextOutbox<AuthDbContext> _dbContextOutbox;

    public OutboxWrapper(IDbContextOutbox<AuthDbContext> dbContextOutbox)
    {
        _dbContextOutbox = dbContextOutbox;
    }

    public ValueTask Send<T>(T message) =>
        _dbContextOutbox.SendAsync(message);
}