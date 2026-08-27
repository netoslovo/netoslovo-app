using Microsoft.EntityFrameworkCore.Storage;
using Wolverine.EntityFrameworkCore;
using WordoGuessr.Auth.App.Abstractions;

namespace WordoGuessr.Auth.Infra.Database;

internal sealed class EfAuthTransactionScope : IAuthTransactionScope
{
    private readonly IDbContextTransaction _internalTransaction;
    private readonly IDbContextOutbox<AuthDbContext> _authDbContextOutbox;
    private bool _disposed;

    public EfAuthTransactionScope(IDbContextTransaction internalTransaction, IDbContextOutbox<AuthDbContext> authDbContextOutbox)
    {
        _internalTransaction = internalTransaction ?? throw new ArgumentNullException(nameof(internalTransaction));
        _authDbContextOutbox = authDbContextOutbox ?? throw new ArgumentNullException(nameof(authDbContextOutbox));
    }

    public async Task Commit(CancellationToken ct = default)
    {
        try
        {
            await _authDbContextOutbox.DbContext.SaveChangesAsync(ct);
            await _internalTransaction.CommitAsync(ct);
            await _authDbContextOutbox.FlushOutgoingMessagesAsync();
        }
        catch
        {
            try
            {
                await _internalTransaction.RollbackAsync(CancellationToken.None);
            }
            catch { }

            throw;
        }
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _internalTransaction.Dispose();
            _disposed = true;
        }
    }
}
